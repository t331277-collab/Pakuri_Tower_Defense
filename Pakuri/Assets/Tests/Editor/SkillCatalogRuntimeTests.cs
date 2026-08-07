using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Pakuri.Combat;
using Pakuri.Data;
using Pakuri.InGame;
using UnityEngine;

public sealed class SkillCatalogRuntimeTests
{
    [Test]
    public void MagazineReloadCompletesOnceAndArmsNextDamage()
    {
        var skill = new ProjectileSkillDefinition
        {
            SkillName = "reload-contract-test",
            RuntimeKind = SkillRuntimeKind.MagazineProjectile,
            MagazineCapacity = 1,
            ReloadSeconds = 0.1f,
            Nodes = new[]
            {
                SkillNode.FromOperation(new SkillActionOp(
                    SkillActionOpKind.ReloadCompleteDamageMultiplier,
                    1.25f))
            }
        };
        var runtime = new SkillExecutionState(null, skill);
        var snapshot = SkillExecutionRules.CreateDefinitionSnapshot(skill);

        Assert.That(SkillExecution.TryBeginCast(runtime, snapshot), Is.True);
        Assert.That(runtime.MagazineRemaining, Is.Zero);

        SkillExecution.Tick(runtime, 0.1f);

        var consume = typeof(SkillExecution).GetMethod(
            "ConsumeReloadCompleteEvent",
            BindingFlags.Static | BindingFlags.NonPublic);
        var armed = typeof(SkillExecutionState).GetField(
            "armedReloadDamageMultiplier",
            BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.That(consume, Is.Not.Null);
        Assert.That(armed, Is.Not.Null);
        Assert.That(runtime.MagazineRemaining, Is.EqualTo(1));
        Assert.That((bool)consume.Invoke(null, new object[] { runtime }), Is.True);
        Assert.That((bool)consume.Invoke(null, new object[] { runtime }), Is.False);
        Assert.That((float)armed.GetValue(runtime), Is.EqualTo(1.25f).Within(0.0001f));
    }

    [Test]
    public void NexusLearnsExistingSupportSkillWithoutAutoCasting()
    {
        var seinC = new ProjectileSkillDefinition
        {
            SkillName = "sein-c",
            RuntimeKind = SkillRuntimeKind.CooldownProjectile
        };

        var nexus = new UnitCombatStateFactory().CreateNexus(100f, seinC);

        Assert.That(nexus.IsNexus, Is.True);
        Assert.That(nexus.AutoSkillEnabled, Is.False);
        Assert.That(nexus.Skills.HasActiveSkill("sein-c"), Is.True);
        Assert.That(nexus.SkillState.FindByDefinition(seinC), Is.Not.Null);
    }

    [Test]
    /// 선택 의미가 지정 스킬에만 반영되는지 확인한다.
    public void ChoiceNodesApplyOnlyToTheirTargetSkill()
    {
        var skill = new SkillDefinition { SkillName = "skill-a" };
        var choice = new SkillChoice
        {
            Nodes = new[]
            {
                SkillNode.FromOperation(new DamageModifierOp(DamageModifierOpKind.BossMultiplier, 2f), "skill-a"),
                SkillNode.FromOperation(new DamageModifierOp(DamageModifierOpKind.BossMultiplier, 3f), "skill-b")
            }
        };
        var data = SkillExecutionRules.CreateDefinitionSnapshot(skill);

        SkillExecutionRules.ApplyChoice(data, choice);

        Assert.That(data.DamageModifierOps, Has.Count.EqualTo(1));
        Assert.That(data.DamageModifierOps[0].Multiplier, Is.EqualTo(2f));
    }

    [Test]
    /// 반응 배율이 기존 피해 보정과 합성되는지 확인한다.
    public void ReactionDamageMultiplierScalesExistingSkillModifier()
    {
        var data = new SkillExecutionState(new SkillDefinition { SkillName = "vega-b" });
        var apply = typeof(SkillExecutionState).GetMethod(
            "ApplyDynamicDamageMultiplier",
            BindingFlags.Instance | BindingFlags.NonPublic);

        Assert.That(apply, Is.Not.Null);
        apply.Invoke(data, new object[] { 1.25f });

        var scale = typeof(SkillExecutionState).GetMethod(
            "ScaleDamageMultiplier",
            BindingFlags.Instance | BindingFlags.NonPublic);

        Assert.That(scale, Is.Not.Null);
        scale.Invoke(data, new object[] { 0.45f });

        Assert.That(data.DamageMultiplier, Is.EqualTo(0.5625f).Within(0.0001f));

        var copyWithMultiplier = typeof(SkillExecutionState).GetMethod(
            "CopyWithDamageMultiplier",
            BindingFlags.Instance | BindingFlags.NonPublic);

        Assert.That(copyWithMultiplier, Is.Not.Null);
        var copy = (SkillExecutionState)copyWithMultiplier.Invoke(data, new object[] { 0.5f });
        Assert.That(copy.DamageMultiplier, Is.EqualTo(0.28125f).Within(0.0001f));
    }

    [Test]
    /// 카탈로그 정의와 재구성 결과가 일치하는지 확인한다.
    public void CatalogAndRebuildReuseFinalDefinition()
    {
        var catalog = ScriptableObject.CreateInstance<GameDataCatalog>();
        var monster = ScriptableObject.CreateInstance<MonsterDefinition>();
        var skill = new SkillDefinition { SkillName = "skill-a", Slot = SkillSlot.A };

        try
        {
            monster.MonsterName = "monster-a";
            monster.ActiveSkills = new[] { skill };
            catalog.Monsters = new[] { monster };
            catalog.RebuildLookup();

            Assert.That(catalog.GetData<SkillDefinition>("skill-a"), Is.SameAs(skill));
            Assert.That(catalog.GetActiveSkill("monster-a", SkillSlot.A), Is.SameAs(skill));

            var owner = new UnitCombatState();
            owner.Skills.AddActiveSkill(skill.SkillName);
            owner.SkillState.RebuildLearnedSkillState(
                owner,
                new[] { skill },
                Array.Empty<PassiveSkillDefinition>());
            var firstState = owner.SkillState.FindBySkillName("skill-a");
            owner.SkillState.RebuildLearnedSkillState(
                owner,
                new[] { skill },
                Array.Empty<PassiveSkillDefinition>());
            var secondState = owner.SkillState.FindBySkillName("skill-a");

            Assert.That(secondState, Is.Not.SameAs(firstState));
            Assert.That(firstState.Data, Is.SameAs(skill));
            Assert.That(secondState.Data, Is.SameAs(skill));
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(monster);
            UnityEngine.Object.DestroyImmediate(catalog);
        }
    }

    [Test]
    /// 유물·시너지·소환 CSV가 해석된 Definition과 lookup을 생성하는지 확인한다.
    public void ArtifactAndSummonCatalogBuildsResolvedDefinitions()
    {
        var catalog = ReloadGameDataCatalog();

        Assert.That(catalog.Artifacts, Has.Length.EqualTo(50));
        Assert.That(catalog.ArtifactSynergies, Has.Length.EqualTo(6));
        Assert.That(catalog.ArtifactSynergyLevels, Has.Length.EqualTo(24));
        Assert.That(catalog.ArtifactEffects, Has.Length.EqualTo(63));
        Assert.That(catalog.ArtifactSynergyEffects, Has.Length.EqualTo(28));
        Assert.That(catalog.Summons, Has.Length.EqualTo(1));

        var summon = catalog.GetSummon("spirit-king");
        Assert.That(summon, Is.Not.Null);
        Assert.That(summon.BaseStats.MaxHealth, Is.EqualTo(1000f));
        Assert.That(summon.ActiveSkills, Has.Length.EqualTo(5));
        Assert.That(catalog.GetMonsters(), Has.Length.EqualTo(5));
        Assert.That(catalog.GetData<ArtifactDefinition>("elemental-prism").Icon, Is.Not.Null);
        Assert.That(catalog.GetData<ArtifactDefinition>("resonance-compass").Icon, Is.Not.Null);
        Assert.That(
            catalog.GetData<ArtifactEffectDefinition>("ember-crown-effect").Nodes,
            Has.Length.EqualTo(2));
        Assert.That(
            catalog.GetData<ArtifactEffectDefinition>("frost-lens-status-effect").Nodes,
            Has.Length.EqualTo(2));
        Assert.That(
            catalog.GetData<ArtifactEffectDefinition>("elemental-prism-holy-effect").Nodes,
            Has.Length.EqualTo(2));
        Assert.That(
            catalog.GetData<ArtifactEffectDefinition>("black-candlestick-mark-effect").Nodes,
            Has.Length.EqualTo(2));
        Assert.That(
            catalog.GetData<ArtifactEffectDefinition>("rift-gem-effect").Reactions,
            Has.Length.EqualTo(6));
        Assert.That(
            catalog.GetData<ArtifactEffectDefinition>("resonance-compass-effect").Reactions,
            Has.Length.EqualTo(5));
        Assert.That(
            catalog.GetData<ArtifactEffectDefinition>("spirit-elixir-contract-count-effect").RepeatRule,
            Is.EqualTo(ArtifactEffectRepeatRule.SynergyArtifactCount));
        Assert.That(
            catalog.GetData<ArtifactEffectDefinition>("elemental-codex-effect").RepeatRule,
            Is.EqualTo(ArtifactEffectRepeatRule.DistinctRepresentativeAttributeCount));
        Assert.That(
            catalog.GetData<ArtifactEffectDefinition>("elemental-prism-holy-effect").SelectionRule,
            Is.EqualTo(ArtifactEffectSelectionRule.PartyDominantAttribute));
        Assert.That(catalog.GetActiveSkill(summon.SummonName, SkillSlot.A), Is.TypeOf<SingleSkillDefinition>());
        Assert.That(catalog.GetActiveSkill(summon.SummonName, SkillSlot.B), Is.TypeOf<ZoneSkillDefinition>());

        var spawn = catalog.GetData<ArtifactSynergyEffectDefinition>(
            "spirit-contract-level-1-spawn-spirit-king");
        var grant = catalog.GetData<ArtifactSynergyEffectDefinition>(
            "spirit-contract-level-1-grant-elemental-explosion");
        Assert.That(spawn.SpawnSummon, Is.SameAs(summon));
        Assert.That(
            catalog.GetData<ArtifactSynergyLevelDefinition>("spirit-contract-level-1").Effects,
            Does.Contain(spawn));
        Assert.That(
            grant.OutcomeSkill,
            Is.SameAs(catalog.GetData<SkillDefinition>("spirit-king-elemental-explosion")));
    }

    [Test]
    /// 파수꾼 유물의 적용 대상, 사건, 방어막, 반사 계약을 확인한다.
    public void SentinelArtifactsBuildResolvedRuntimeContracts()
    {
        var catalog = ReloadGameDataCatalog();
        var sentinelArtifacts = catalog.Artifacts
            .Where(artifact => artifact.SynergyName == "sentinel")
            .ToArray();
        var effects = sentinelArtifacts.SelectMany(artifact => artifact.Effects).ToArray();

        Assert.That(sentinelArtifacts, Has.Length.EqualTo(10));
        Assert.That(effects, Has.Length.EqualTo(10));
        Assert.That(
            effects.Single(effect => effect.EffectName == "unbreakable-promise-effect").Recipient,
            Is.EqualTo(ArtifactEffectRecipient.Owner));
        Assert.That(
            effects.Where(effect => effect.EffectName != "unbreakable-promise-effect"),
            Is.All.Matches<ArtifactEffectDefinition>(
                effect => effect.Recipient == ArtifactEffectRecipient.AllAllies));

        var pureWhite = effects.Single(effect => effect.EffectName == "pure-white-shield-effect")
            .Reactions.Single();
        var pilgrim = effects.Single(effect => effect.EffectName == "pilgrims-cloak-effect")
            .Reactions.Single();
        var pureWhiteShield = (BuffSkillDefinition)pureWhite.Effect.ResolvedDefinition;
        var pilgrimShield = (BuffSkillDefinition)pilgrim.Effect.ResolvedDefinition;
        Assert.That(pureWhite.Event, Is.EqualTo(SkillTriggerEvent.CombatStart));
        Assert.That(pureWhiteShield.ShieldTargetMaxHealthRatio, Is.EqualTo(0.12f));
        Assert.That(pureWhiteShield.ShieldDuration, Is.EqualTo(9999f));
        Assert.That(pilgrim.Event, Is.EqualTo(SkillTriggerEvent.BossCombatStart));
        Assert.That(pilgrimShield.ShieldTargetMaxHealthRatio, Is.EqualTo(0.50f));
        Assert.That(pilgrimShield.ShieldDuration, Is.EqualTo(10f));
        Assert.That(
            effects.Single(effect => effect.EffectName == "unbreakable-promise-effect")
                .Reactions.Single().Event,
            Is.EqualTo(SkillTriggerEvent.OnShieldBreak));
        Assert.That(
            effects.Single(effect => effect.EffectName == "blue-cross-effect")
                .Reactions.Single().Event,
            Is.EqualTo(SkillTriggerEvent.OnHealOrShieldReceived));

        var censerDuration = effects.Single(effect => effect.EffectName == "guardians-censer-effect")
            .Nodes.Select(GetNodeOperation<SkillActionOp>)
            .Single(op => op.HasValue && op.Value.Kind == SkillActionOpKind.StatusDurationBonus)
            .Value;
        Assert.That(censerDuration.ReferenceName, Is.EqualTo("shield"));
        Assert.That(censerDuration.Amount, Is.EqualTo(2f));

        var prayer = effects.Single(effect => effect.EffectName == "prayer-stone-effect")
            .Nodes.Select(GetNodeOperation<CooldownChargeSpeedBonusOp>)
            .Single(op => op.HasValue).Value;
        Assert.That(prayer.Bonus, Is.EqualTo(0.12f));

        var reflections = new[]
        {
            catalog.GetData<ArtifactSynergyEffectDefinition>(
                "sentinel-level-2-shield-reflection").Reactions.Single(),
            catalog.GetData<ArtifactSynergyEffectDefinition>(
                "sentinel-level-4-shield-reflection").Reactions.Single(),
            effects.Single(effect => effect.EffectName == "reflection-mirror-effect")
                .Reactions.Single()
        };
        Assert.That(
            reflections.Select(reaction => reaction.DamageValueMultiplier),
            Is.EqualTo(new[] { 0.25f, 0.20f, 0.20f }));
        Assert.That(
            reflections,
            Is.All.Matches<SkillReaction>(reaction =>
                reaction.DamageValueSource
                    == SkillTriggerDamageValueSource.ShieldAbsorbedAmount));
        Assert.That(reflections, Is.All.Matches<SkillReaction>(reaction => reaction.IsTrigger));
        Assert.That(
            reflections.Select(reaction => reaction.Effect.ResolvedDefinition.Element),
            Is.All.EqualTo(DamageAttribute.Holy));
    }

    [Test]
    public void ArtilleryArtifactsAndSupportSnapshotResolveFinalContracts()
    {
        var catalog = ReloadGameDataCatalog();
        var artifacts = catalog.Artifacts
            .Where(artifact => artifact.SynergyName == "artillery")
            .ToArray();
        var effects = artifacts.SelectMany(artifact => artifact.Effects).ToArray();

        Assert.That(artifacts, Has.Length.EqualTo(10));
        Assert.That(effects, Has.Length.EqualTo(11));
        Assert.That(
            effects.Where(effect => effect.Recipient == ArtifactEffectRecipient.AllAllies)
                .Select(effect => effect.EffectName),
            Is.EquivalentTo(new[] { "infinite-shell-effect", "piercing-feather-effect" }));
        Assert.That(
            effects.Where(effect => effect.Recipient != ArtifactEffectRecipient.AllAllies),
            Is.All.Matches<ArtifactEffectDefinition>(
                effect => effect.Recipient == ArtifactEffectRecipient.Owner));

        var lightning = effects.Single(
            effect => effect.EffectName == "lightning-magazine-effect").Reactions.Single();
        Assert.That(lightning.Event, Is.EqualTo(SkillTriggerEvent.OnOutgoingDamage));
        Assert.That(lightning.ProcChance, Is.EqualTo(0.20f));
        Assert.That(lightning.TriggerAttributes, Is.EqualTo(new[] { DamageAttribute.Lightning }));
        Assert.That(
            lightning.EventSkillRuntimeKindValues.Single().Kind,
            Is.EqualTo(SkillRuntimeKind.MagazineProjectile));
        Assert.That(
            lightning.Effect.OnHitStatusOverride.Status.Kind,
            Is.EqualTo(StatusEffectKind.Shock));

        var support = catalog.GetData<ArtifactSynergyEffectDefinition>(
            "artillery-level-1-support-bombardment").Reactions.Single();
        Assert.That(support.Event, Is.EqualTo(SkillTriggerEvent.OnReloadComplete));
        Assert.That(support.CasterScope, Is.EqualTo(SkillReactionCasterScope.Nexus));
        Assert.That(support.Effect.ResolvedDefinition.SkillName, Is.EqualTo("sein-c"));
        Assert.That(support.Effect.TargetSelectionOverride, Is.EqualTo(SkillTargetSelection.Densest));
        Assert.That(support.Effect.RawDamageOverride, Is.EqualTo(60f));
        Assert.That(support.Effect.DamageAttributeOverride, Is.EqualTo(DamageAttribute.Physical));
        Assert.That(support.Effect.DamageDelayOverride, Is.EqualTo(0.1f));

        var owner = new UnitCombatState();
        AddActiveArtifactEffect(owner, "artillery-level-2-support-bombardment");
        AddActiveArtifactEffect(owner, "artillery-level-3-shrapnel");
        AddActiveArtifactEffect(owner, "artillery-level-4-support-bombardment");
        var runtime = new SkillExecutionState(owner, support.Effect.ResolvedDefinition);
        var build = typeof(SkillExecutionRules).GetMethod(
            "BuildTriggeredSynergyExecutionData",
            BindingFlags.Static | BindingFlags.NonPublic);
        Assert.That(build, Is.Not.Null);
        var snapshot = (SkillExecutionState)build.Invoke(
            null,
            new object[] { owner, runtime, support.Effect });

        Assert.That(snapshot.RawDamageOverride, Is.EqualTo(120f));
        Assert.That(snapshot.DamageAttributeOverride, Is.EqualTo(DamageAttribute.Physical));
        Assert.That(snapshot.DamageDelayOverride, Is.EqualTo(0.1f));
        Assert.That(snapshot.RadiusMultiplierOverride, Is.EqualTo(2f));
        Assert.That(snapshot.ArrivalFragmentCount, Is.EqualTo(3));
        Assert.That(snapshot.ArrivalFragmentDelaySeconds, Is.EqualTo(0.3f));
        Assert.That(snapshot.ArrivalFragmentSearchRadius, Is.EqualTo(3f));
        Assert.That(snapshot.ArrivalFragmentRawDamage, Is.EqualTo(30f));
        Assert.That(snapshot.ArrivalFragmentRadiusMultiplier, Is.EqualTo(1.15f));
    }

    [Test]
    /// 파수꾼 단계 방어 증가와 최종 피해 감소가 지정 순서로 계산되는지 확인한다.
    public void SentinelDefenseAndFinalDamageUseRuntimeArtifactRules()
    {
        ReloadGameDataCatalog();
        var target = new UnitCombatState();
        var defenseEffects = new[]
        {
            "sentinel-level-1-defense-resistance",
            "sentinel-level-2-defense-resistance",
            "sentinel-level-3-defense-resistance-shield-reduction",
            "sentinel-level-4-defense-resistance"
        };
        var expectedRates = new[] { 0.05f, 0.10f, 0.15f, 0.20f };
        var expectedFlat = new[] { 8f, 12f, 18f, 25f };
        Assert.That(ArtifactCombatRules.Resolve(target).DefenseBonusRate, Is.Zero);
        for (var i = 0; i < defenseEffects.Length; i++)
        {
            AddActiveArtifactEffect(target, defenseEffects[i]);
            var modifiers = ArtifactCombatRules.Resolve(target);
            Assert.That(modifiers.DefenseBonusRate, Is.EqualTo(expectedRates[i]).Within(0.0001f));
            Assert.That(modifiers.FlatDefenseBonus, Is.EqualTo(expectedFlat[i]).Within(0.0001f));
        }

        target.Defenses.Holy = 100f;
        var defensePassive = CreatePassive(
            "sentinel-test-defense",
            PassiveModifierKind.DefenseUp,
            0.20f);
        target.Skills.AddPassiveSkill(defensePassive.SkillName);
        target.SkillState.RebuildLearnedSkillState(
            target,
            Array.Empty<SkillDefinition>(),
            new[] { defensePassive });
        target.Statuses.Apply(new StatusRuntimeData
        {
            Kind = StatusEffectKind.HolyResistDown,
            HasElementModifierTarget = true,
            ElementModifierTarget = DamageAttribute.Holy,
            ElementResistReduction = 0.10f
        }, 1, 0f, permanent: true);
        var defense = ((100f * 1.20f) * 1.20f + 25f) * 0.90f;
        var noSourceRule = new AttackRule(null, false, 0f, 0f, null, false, false, null, 1f);
        Assert.That(
            DamageCalculator.CalculateFinalDamage(target, 100f, DamageAttribute.Holy, noSourceRule),
            Is.EqualTo(Mathf.Round(100f * 100f / (100f + defense))));

        var finalTarget = new UnitCombatState();
        AddActiveArtifactEffect(
            finalTarget,
            "sentinel-level-3-shield-final-damage-reduction");
        var finalRule = new AttackRule(
            null,
            false,
            0f,
            0f,
            null,
            false,
            false,
            null,
            1f,
            finalDamageModifier: 1.15f);
        Assert.That(
            DamageCalculator.CalculateFinalDamage(
                finalTarget,
                100f,
                DamageAttribute.Physical,
                finalRule),
            Is.EqualTo(115f));
        finalTarget.Statuses.Apply(ShieldStatus("sentinel-test-shield"), 1, 10f, shieldAmount: 10f);
        Assert.That(
            DamageCalculator.CalculateFinalDamage(
                finalTarget,
                100f,
                DamageAttribute.Physical,
                finalRule),
            Is.EqualTo(Mathf.Round(100f * 1.15f * 0.90f)));
        finalTarget.Statuses.Apply(
            IncomingDamageStatus(StatusEffectKind.Vulnerable, -0.20f),
            1,
            0f,
            permanent: true);
        Assert.That(
            DamageCalculator.CalculateFinalDamage(
                finalTarget,
                100f,
                DamageAttribute.Physical,
                finalRule),
            Is.EqualTo(Mathf.Round(100f * 0.80f * 1.15f * 0.90f)));
    }

    [Test]
    /// 서로 다른 원천의 보호막은 수치만 합산하고 각 시간에 따로 사라지는지 확인한다.
    public void ShieldsKeepIndependentSourceDurationsAndSummedAmount()
    {
        var statuses = new UnitStatusCollection();
        statuses.Apply(ShieldStatus("pure-white-shield-effect"), 1, 2f, shieldAmount: 12f);
        statuses.Apply(ShieldStatus("pilgrims-cloak-effect"), 1, 10f, shieldAmount: 50f);

        Assert.That(statuses.ActiveStatuses, Has.Count.EqualTo(2));
        Assert.That(statuses.GetTotalShieldAmount(), Is.EqualTo(62f));

        var removed = new List<StatusRuntimeInstance>();
        Assert.That(statuses.Tick(2.1f, removed), Is.True);
        Assert.That(removed, Has.Count.EqualTo(1));
        Assert.That(removed[0].SourceSkillName, Is.EqualTo("pure-white-shield-effect"));
        Assert.That(statuses.GetTotalShieldAmount(), Is.EqualTo(50f));
        Assert.That(statuses.Tick(8f), Is.True);
        Assert.That(statuses.GetTotalShieldAmount(), Is.Zero);
    }

    [Test]
    /// 기도석 쿨타임 충전은 보호막 보유 전·중·후에만 동적으로 달라지는지 확인한다.
    public void PrayerStoneCooldownChargeOnlyAcceleratesWhileShielded()
    {
        ReloadGameDataCatalog();
        var owner = new UnitCombatState();
        AddActiveArtifactEffect(owner, "prayer-stone-effect");
        var runtime = new SkillExecutionState(
            owner,
            new SkillDefinition
            {
                SkillName = "cooldown-test",
                Timing = new SkillTimingSpec { Cooldown = 10f }
            });
        SetCooldownRemaining(runtime, 10f);

        SkillExecution.Tick(runtime, 1f);
        Assert.That(runtime.CooldownRemaining, Is.EqualTo(9f).Within(0.0001f));
        owner.Statuses.Apply(ShieldStatus("cooldown-test-shield"), 1, 10f, shieldAmount: 10f);
        SkillExecution.Tick(runtime, 1f);
        Assert.That(runtime.CooldownRemaining, Is.EqualTo(7.88f).Within(0.0001f));
        owner.Statuses.ConsumeShield(10f);
        SkillExecution.Tick(runtime, 1f);
        Assert.That(runtime.CooldownRemaining, Is.EqualTo(6.88f).Within(0.0001f));
    }

    [Test]
    /// 반사 피해는 같은 원천끼리 합치고 다른 원천은 별도 항목으로 유지하는지 확인한다.
    public void ShieldReflectionAccumulatorGroupsBySourceUnit()
    {
        var catalog = ReloadGameDataCatalog();
        var mirror = catalog.GetData<ArtifactEffectDefinition>("reflection-mirror-effect")
            .Reactions.Single();
        var arielMaster = SkillExecutionRules.CreateDefinitionSnapshot(
                catalog.GetActiveSkill("ariel", SkillSlot.B))
            .Reactions.Single(reaction =>
                reaction.ReactionName == "ariel-b-master2-shield-absorb-reflect");
        var sentinel = catalog.GetData<ArtifactSynergyEffectDefinition>(
            "sentinel-level-2-shield-reflection").Reactions.Single();
        var sourceA = new UnitCombatState();
        var sourceB = new UnitCombatState();
        var sourceAObject = new GameObject("ReflectionSourceA");
        var sourceBObject = new GameObject("ReflectionSourceB");

        try
        {
            var accumulatorType = typeof(SkillExecution).Assembly.GetType(
                "Pakuri.InGame.SkillTrigger+ShieldReflectionAccumulator");
            Assert.That(accumulatorType, Is.Not.Null);
            var accumulator = Activator.CreateInstance(
                accumulatorType,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                new object[] { null, null },
                null);
            var tryAdd = accumulatorType.GetMethod("TryAdd");
            var entriesField = accumulatorType.GetField(
                "entries",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(tryAdd, Is.Not.Null);
            Assert.That(entriesField, Is.Not.Null);
            var triggerContext = Activator.CreateInstance(
                tryAdd.GetParameters()[3].ParameterType);
            Assert.That(
                tryAdd.Invoke(accumulator, new object[]
                {
                    new CombatUnitEntry(sourceA, sourceAObject.transform),
                    sourceA,
                    mirror,
                    triggerContext,
                    20f
                }),
                Is.True);
            Assert.That(
                tryAdd.Invoke(accumulator, new object[]
                {
                    new CombatUnitEntry(sourceA, sourceAObject.transform),
                    sourceA,
                    arielMaster,
                    triggerContext,
                    35f
                }),
                Is.True);
            Assert.That(
                tryAdd.Invoke(accumulator, new object[]
                {
                    new CombatUnitEntry(sourceB, sourceBObject.transform),
                    sourceB,
                    sentinel,
                    triggerContext,
                    25f
                }),
                Is.True);

            var entries = (System.Collections.IList)entriesField.GetValue(accumulator);
            float RawDamageFor(UnitCombatState source)
            {
                for (var i = 0; i < entries.Count; i++)
                {
                    var entry = entries[i];
                    var entryType = entry.GetType();
                    if (ReferenceEquals(
                        entryType.GetField("Source").GetValue(entry),
                        source))
                    {
                        return (float)entryType.GetField("RawDamage").GetValue(entry);
                    }
                }

                return 0f;
            }

            Assert.That(entries, Has.Count.EqualTo(2));
            Assert.That(RawDamageFor(sourceA), Is.EqualTo(55f));
            Assert.That(RawDamageFor(sourceB), Is.EqualTo(25f));
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(sourceBObject);
            UnityEngine.Object.DestroyImmediate(sourceAObject);
        }
    }

    [Test]
    /// Stage 1/2의 Day5·Day10 Midboss와 Day11 Boss가 모두 유물 선택 세 개를 제공하는지 확인한다.
    public void StageArtifactRewardsIncludeMidbossAndBoss()
    {
        var stage = ReloadGameDataCatalog().Stage;
        var rewardNames = new[]
        {
            "reward-stage1-midboss",
            "reward-stage1-day10-midboss",
            "reward-stage1-boss",
            "reward-stage2-midboss",
            "reward-stage2-day10-midboss",
            "reward-stage2-boss"
        };

        for (var i = 0; i < rewardNames.Length; i++)
        {
            Assert.That(stage.FindReward(rewardNames[i]).ArtifactChoiceCount, Is.EqualTo(3));
        }

        Assert.That(stage.FindReward("reward-stage1-normal").ArtifactChoiceCount, Is.Zero);
        Assert.That(stage.FindReward("reward-stage2-normal").ArtifactChoiceCount, Is.Zero);
    }

    [Test]
    /// 적 유닛이 공통 런타임으로 스킬을 학습하는지 확인한다.
    public void EnemySpawnLearnsAssignedSkillsThroughSharedRuntime()
    {
        var enemy = ScriptableObject.CreateInstance<EnemyDefinition>();
        var active = new SkillDefinition
        {
            SkillName = "enemy-active",
            IsActive = true
        };
        var passive = new PassiveSkillDefinition
        {
            SkillName = "enemy-passive",
            IsActive = false,
            ModifierKind = PassiveModifierKind.DamageUp,
            HasModifierAttribute = true,
            ModifierAttribute = DamageAttribute.Physical,
            ModifierValue = 0.1f
        };

        try
        {
            enemy.EnemyName = "enemy-a";
            enemy.ActiveSkills = new[] { active };
            enemy.PassiveSkill = passive;

            var model = new UnitCombatStateFactory().CreateEnemy(enemy);
            model.SkillState.RebuildLearnedSkillState(
                model,
                enemy.ActiveSkills,
                new[] { enemy.PassiveSkill });

            Assert.That(model.Skills.HasActiveSkill(active.SkillName), Is.True);
            Assert.That(model.Skills.HasPassiveSkill(passive.SkillName), Is.True);
            Assert.That(model.SkillState.FindBySkillName(active.SkillName).Data, Is.SameAs(active));
            Assert.That(model.SkillState.FindBySkillName(passive.SkillName).Data, Is.SameAs(passive));
            Assert.That(
                model.SkillState.PassiveOutgoingDamageMultiplier(DamageAttribute.Physical),
                Is.EqualTo(1.1f).Within(0.0001f));
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(enemy);
        }
    }

    [Test]
    /// 공통 지속 런타임이 적 보정 종류를 보존하는지 확인한다.
    public void SharedPassiveRuntimePreservesEnemyModifierKinds()
    {
        var owner = new UnitCombatState();
        var passives = new[]
        {
            CreatePassive("damage", PassiveModifierKind.DamageUp, 0.1f, DamageAttribute.Fire),
            CreatePassive("defense", PassiveModifierKind.DefenseUp, 0.1f),
            CreatePassive("defense-2", PassiveModifierKind.DefenseUp, 0.2f),
            CreatePassive("crit-chance", PassiveModifierKind.CritChanceUp, 0.08f),
            CreatePassive("crit-damage", PassiveModifierKind.CritDamageUp, 0.2f),
            CreatePassive("healing", PassiveModifierKind.HealingUp, 0.15f),
            CreatePassive("incoming", PassiveModifierKind.IncomingDamageDown, 0.12f)
        };
        for (var i = 0; i < passives.Length; i++)
        {
            owner.Skills.AddPassiveSkill(passives[i].SkillName);
        }

        owner.SkillState.RebuildLearnedSkillState(
            owner,
            Array.Empty<SkillDefinition>(),
            passives);

        Assert.That(owner.SkillState.PassiveOutgoingDamageMultiplier(DamageAttribute.Fire), Is.EqualTo(1.1f).Within(0.0001f));
        Assert.That(owner.SkillState.PassiveOutgoingDamageMultiplier(DamageAttribute.Ice), Is.EqualTo(1f).Within(0.0001f));
        Assert.That(owner.SkillState.PassiveDefenseMultiplier(DamageAttribute.Holy), Is.EqualTo(1.32f).Within(0.0001f));
        Assert.That(owner.SkillState.PassiveCriticalChanceBonus(), Is.EqualTo(0.08f).Within(0.0001f));
        Assert.That(owner.SkillState.PassiveCriticalDamageMultiplier(), Is.EqualTo(1.2f).Within(0.0001f));
        Assert.That(owner.SkillState.PassiveHealingMultiplier(), Is.EqualTo(1.15f).Within(0.0001f));
        Assert.That(owner.SkillState.PassiveIncomingDamageMultiplier(), Is.EqualTo(0.88f).Within(0.0001f));
    }

    [Test]
    /// 최종 피해가 방어력, 주는 피해, 받는 피해 배율을 분리해 곱하는지 확인한다.
    public void DamageFormulaMultipliesResolvedGroups()
    {
        var source = new UnitCombatState();
        var target = new UnitCombatState();
        source.Stats.AttackPower = -10f;
        source.Stats.SpellPower = -20f;
        target.Defenses.Lightning = 40f;

        source.Statuses.Apply(DamageStatus(StatusEffectKind.Shock, 0.10f, DamageAttribute.Lightning), 1, 0f, permanent: true);
        source.Statuses.Apply(DamageStatus(StatusEffectKind.Blessing, 0.10f, DamageAttribute.Lightning), 1, 0f, permanent: true);
        target.Statuses.Apply(new StatusRuntimeData
        {
            Kind = StatusEffectKind.FireResistDown,
            HasElementModifierTarget = true,
            ElementModifierTarget = DamageAttribute.Lightning,
            ElementResistReduction = 0.10f,
            FlatElementResistReduction = 5f
        }, 1, 0f, permanent: true);
        target.Statuses.Apply(IncomingDamageStatus(StatusEffectKind.Vulnerable, 0.20f), 1, 0f, permanent: true);
        target.Statuses.Apply(IncomingDamageStatus(StatusEffectKind.ActionSpeedUp, -0.15f), 1, 0f, permanent: true);

        var rawDamage = DamageCalculator.CalculateRawDamage(
            source,
            new SkillDamageSpec
            {
                BaseDamage = 100f,
                AttackPowerCoefficient = 1f,
                SpellPowerCoefficient = 1f
            });
        var attackRule = new AttackRule(source, false, 0f, 0f, "test-skill", false, false, null, 1.15f);
        var finalDamage = DamageCalculator.CalculateFinalDamage(target, rawDamage, DamageAttribute.Lightning, attackRule);
        var expected = Mathf.Round(100f * (100f / 131f) * 1.15f * 1.10f * 1.10f * 1.20f * 0.85f);

        Assert.That(rawDamage, Is.EqualTo(100f));
        Assert.That(finalDamage, Is.EqualTo(expected));
    }

    [Test]
    /// 치명타 확률은 합산 후 보정하고 치명타 피해는 배율로 합성하는지 확인한다.
    public void CriticalRulesClampChanceAndMultiplyDamage()
    {
        var source = new UnitCombatState();
        var target = new UnitCombatState();
        source.Stats.CriticalChance = -0.5f;
        source.Stats.CriticalDamage = 1.5f;
        source.Statuses.Apply(CriticalDamageStatus(StatusEffectKind.Blessing, 0.30f), 1, 0f, permanent: true);
        source.Statuses.Apply(CriticalDamageStatus(StatusEffectKind.ActionSpeedUp, 0.20f), 1, 0f, permanent: true);
        target.Statuses.Apply(new StatusRuntimeData
        {
            Kind = StatusEffectKind.Vulnerable,
            CriticalDamageTakenBonus = 0.10f
        }, 1, 0f, permanent: true);

        var attackRule = new AttackRule(source, true, 0f, 0.25f, "test-skill", false, false, null, 1f);

        Assert.That(DamageCalculator.ResolveCriticalChance(target, attackRule), Is.Zero);
        Assert.That(
            DamageCalculator.ResolveCriticalDamageMultiplier(target, attackRule),
            Is.EqualTo(1.5f * 1.30f * 1.20f * 1.25f * 1.10f).Within(0.0001f));
    }

    [Test]
    /// 적 카탈로그가 공통 지속 상태를 구성하는지 확인한다.
    public void EnemyCatalogBuildsSharedLearnedPassives()
    {
        GameDataLoader.EnsureInitialized();
        var catalog = GameDataLoader.CurrentCatalog;
        var enemies = new List<EnemyDefinition>();
        enemies.AddRange(catalog.StageOneEnemies);
        enemies.AddRange(catalog.StageTwoEnemies);

        Assert.That(enemies, Has.Count.EqualTo(16));
        for (var i = 0; i < enemies.Count; i++)
        {
            var enemy = enemies[i];
            Assert.That(enemy.PassiveSkill, Is.Not.Null, enemy.EnemyName);
            Assert.That(enemy.PassiveSkill.ModifierKind, Is.Not.EqualTo(PassiveModifierKind.None), enemy.EnemyName);
            Assert.That(
                catalog.GetData<PassiveSkillDefinition>(enemy.PassiveSkill.SkillName),
                Is.SameAs(enemy.PassiveSkill),
                enemy.EnemyName);

            var model = new UnitCombatStateFactory().CreateEnemy(enemy);
            model.SkillState.RebuildLearnedSkillState(
                model,
                enemy.ActiveSkills,
                new[] { enemy.PassiveSkill });

            Assert.That(model.Skills.HasPassiveSkill(enemy.PassiveSkill.SkillName), Is.True, enemy.EnemyName);
            Assert.That(
                model.SkillState.FindBySkillName(enemy.PassiveSkill.SkillName)?.Data,
                Is.SameAs(enemy.PassiveSkill),
                enemy.EnemyName);
        }
    }

    [Test]
    /// 적 스킬 프로필이 통합 정의 계열을 사용하는지 확인한다.
    public void EnemySkillProfilesUseUnifiedDefinitionFamilies()
    {
        GameDataLoader.EnsureInitialized();
        var catalog = GameDataLoader.CurrentCatalog;

        var chainEnemy = Array.Find(
            catalog.StageTwoEnemies,
            enemy => enemy.EnemyName == "stage2-lightning-scout");
        var chainSkill = Array.Find(
            chainEnemy.ActiveSkills,
            skill => skill.SkillName == "ChainLightning");
        var chainTrigger = CollectReactions(chainEnemy.ActiveSkills).Find(
            reaction => reaction.ReactionName == "ChainLightning__chain_on_hit");

        Assert.That(chainSkill, Is.TypeOf<SingleSkillDefinition>());
        Assert.That(chainTrigger, Is.Not.Null);
        Assert.That(chainTrigger.Event, Is.EqualTo(SkillTriggerEvent.OnHit));
        Assert.That(chainTrigger.DelaySeconds, Is.EqualTo(0.5f).Within(0.0001f));
        Assert.That(chainTrigger.DamageMultiplier, Is.EqualTo(0.5f).Within(0.0001f));
        Assert.That(chainTrigger.PublishSkillLifecycleEvents, Is.False);
        Assert.That(chainTrigger.Effect, Is.Not.Null);
        Assert.That(
            chainTrigger.Effect.ResolvedDefinition,
            Is.TypeOf<SingleSkillDefinition>());
        var chainOutcome = (SingleSkillDefinition)chainTrigger.Effect.ResolvedDefinition;
        Assert.That(chainOutcome.Damage, Is.SameAs(
            ((SingleSkillDefinition)chainSkill).Damage));
        Assert.That(
            chainOutcome.Targeting.Selection,
            Is.EqualTo(SkillTargetSelection.NearestOtherFromEventTarget));
        Assert.That(
            chainOutcome.Targeting.Radius,
            Is.EqualTo(7f).Within(0.0001f));
        Assert.That(
            Array.Exists(
                chainEnemy.ActiveSkills,
                skill => skill.SkillName.Contains("__chain")),
            Is.False);

        var chargeEnemy = Array.Find(
            catalog.StageTwoEnemies,
            enemy => enemy.EnemyName == "stage2-drake");
        var chargeSkill = Array.Find(
            chargeEnemy.ActiveSkills,
            skill => skill.SkillName == "OpeningCharge");
        Assert.That(chargeSkill, Is.TypeOf<BuffSkillDefinition>());
        Assert.That(
            ((BuffSkillDefinition)chargeSkill).EffectKind,
            Is.EqualTo(BuffEffectKind.Charge));

        var shieldEnemy = Array.Find(
            catalog.StageOneEnemies,
            enemy => enemy.EnemyName == "stage1-guardian-captain");
        var shieldSkill = Array.Find(
            shieldEnemy.ActiveSkills,
            skill => skill.SkillName == "GuardianFlag");
        Assert.That(shieldSkill, Is.TypeOf<BuffSkillDefinition>());
        Assert.That(
            ((BuffSkillDefinition)shieldSkill).EffectKind,
            Is.EqualTo(BuffEffectKind.Shield));
    }

    [Test]
    /// 반응 돌진이 공통 활성 런타임을 사용하는지 확인한다.
    public void TriggeredChargeUsesSharedActiveRuntime()
    {
        var actorObject = new GameObject("TriggeredChargeActor");

        try
        {
            var owner = new EnemyCombatState();
            owner.Resources.CurrentHealth = 1f;
            var charge = new BuffSkillDefinition
            {
                SkillName = "charge",
                IsActive = true,
                RuntimeKind = SkillRuntimeKind.Buff,
                EffectKind = BuffEffectKind.Charge,
                Timing = new SkillTimingSpec
                {
                    Cooldown = 30f,
                    ActiveDuration = 5f
                }
            };
            var runtime = new SkillExecutionState(owner, charge);
            owner.SkillState.AddOrReplace(runtime);
            var entry = new CombatUnitEntry(owner, actorObject.transform);
            var executed = new SkillExecution().TryExecuteReaction(
                entry,
                runtime,
                runtime,
                charge,
                null,
                null,
                null,
                Vector2.zero,
                false,
                false,
                0f,
                0,
                1f,
                charge.SkillName,
                false,
                false,
                true);

            Assert.That(executed, Is.True);
            Assert.That(runtime.IsActive, Is.True);
            Assert.That(runtime.ActiveDurationRemaining, Is.EqualTo(5f).Within(0.0001f));

            var decision = typeof(SkillExecution).Assembly.GetType("Pakuri.InGame.EnemyCombatDecision");
            var resolveCharge = decision?.GetMethod(
                "ResolveActiveCharge",
                BindingFlags.Static | BindingFlags.Public);
            Assert.That(resolveCharge, Is.Not.Null);
            Assert.That(resolveCharge.Invoke(null, new object[] { owner }), Is.SameAs(runtime));

            SkillExecution.StopActive(runtime);

            Assert.That(runtime.IsActive, Is.False);
            Assert.That(resolveCharge.Invoke(null, new object[] { owner }), Is.Null);
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(actorObject);
        }
    }

    [Test]
    /// 반응 준비가 원래 정의 식별자를 보존하는지 확인한다.
    public void TriggeredPreparationKeepsTriggeredDefinitionIdentity()
    {
        var actorObject = new GameObject("TriggeredIdentityActor");

        try
        {
            var owner = new EnemyCombatState();
            var source = new SingleSkillDefinition { SkillName = "source-skill" };
            var triggered = new BuffSkillDefinition
            {
                SkillName = "triggered-charge",
                RuntimeKind = SkillRuntimeKind.Buff,
                EffectKind = BuffEffectKind.Charge
            };
            var sourceSnapshot = SkillExecutionRules.CreateDefinitionSnapshot(source);
            var triggeredRuntime = new SkillExecutionState(owner, triggered);
            var context = new SkillExecutionContext(
                null,
                null,
                new CombatUnitEntry(owner, actorObject.transform),
                triggeredRuntime);
            var prepare = typeof(SkillExecution).GetMethod(
                "PrepareExecutionData",
                BindingFlags.Static | BindingFlags.NonPublic);
            var preparedSkillName = typeof(SkillExecutionState).GetProperty(
                "PreparedSkillName",
                BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.That(prepare, Is.Not.Null);
            Assert.That(preparedSkillName, Is.Not.Null);
            Assert.That(
                prepare.Invoke(null, new object[] { context, sourceSnapshot, triggered }),
                Is.True);
            Assert.That(
                preparedSkillName.GetValue(sourceSnapshot),
                Is.EqualTo(triggered.SkillName));
            Assert.That(sourceSnapshot.SkillName, Is.EqualTo(source.SkillName));
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(actorObject);
        }
    }

    [Test]
    /// 몬스터 런타임이 전투 세션 스킬을 공유하는지 확인한다.
    public void MonsterRuntimeSharesRunSessionSkills()
    {
        var monster = ScriptableObject.CreateInstance<MonsterDefinition>();

        try
        {
            monster.MonsterName = "monster-a";
            var session = RunSession.Begin(monster);
            var runState = session.GetPartyMemberState(monster.MonsterName);
            var model = new UnitCombatStateFactory().CreateSelectedMonster(monster, runState);

            Assert.That(model.Skills, Is.SameAs(runState.Skills));

            runState.Skills.AddActiveSkill("skill-a");

            Assert.That(model.Skills.HasActiveSkill("skill-a"), Is.True);
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(monster);
        }
    }

    [Test]
    /// 유물 상태가 최대 세 개를 지키고 Stage 준비 결과를 런타임과 공유하는지 확인한다.
    public void MonsterRuntimeSharesPreparedArtifactState()
    {
        var catalog = ReloadGameDataCatalog();
        var monster = catalog.GetMonster("sein");
        var session = RunSession.Begin(monster);
        var runState = session.GetPartyMemberState(monster.MonsterName);

        Assert.That(runState.Artifacts.TryAdd("ember-crown"), Is.True);
        Assert.That(runState.Artifacts.TryAdd("frost-lens"), Is.True);
        Assert.That(runState.Artifacts.TryAdd("storm-capacitor"), Is.True);
        Assert.That(runState.Artifacts.TryAdd("radiant-chalice"), Is.False);

        new ArtifactSynergyManager().PrepareStage(session, catalog);
        var model = new UnitCombatStateFactory().CreateSelectedMonster(monster, runState);

        Assert.That(model.Artifacts, Is.SameAs(runState.Artifacts));
        Assert.That(model.Artifacts.ActiveArtifactEffectNames, Has.Count.EqualTo(6));
        Assert.That(
            model.Artifacts.ActiveArtifactEffectNames,
            Does.Contain("frost-lens-status-effect"));
    }

    [Test]
    /// RunSession 유물 획득이 파티 중복과 유닛별 세 개 제한을 함께 지키는지 확인한다.
    public void ArtifactAcquisitionRejectsPartyDuplicateAndFullRecipient()
    {
        var catalog = ReloadGameDataCatalog();
        var ariel = catalog.GetMonster("ariel");
        var eve = catalog.GetMonster("eve");
        var session = RunSession.Begin(ariel);

        Assert.That(session.TryAddPartyMonster(eve, out _), Is.True);
        var arielState = session.GetPartyMemberState("ariel");
        var eveState = session.GetPartyMemberState("eve");

        Assert.That(session.TryAcquireArtifact(arielState, "ember-crown"), Is.True);
        Assert.That(session.TryAcquireArtifact(eveState, "ember-crown"), Is.False);
        Assert.That(session.TryAcquireArtifact(arielState, "frost-lens"), Is.True);
        Assert.That(session.TryAcquireArtifact(arielState, "storm-capacitor"), Is.True);
        Assert.That(session.TryAcquireArtifact(arielState, "radiant-chalice"), Is.False);
        Assert.That(session.CanAcquireArtifact(eveState, "radiant-chalice"), Is.True);
    }

    [Test]
    /// 유물 후보가 남은 개수만 표시되고 모든 파티원이 가득 차면 생성되지 않는지 확인한다.
    public void ArtifactChoicesRespectRemainingPoolAndPartyCapacity()
    {
        var catalog = ReloadGameDataCatalog();
        var session = RunSession.Begin(catalog.GetMonster("ariel"));
        Assert.That(session.TryAddPartyMonster(catalog.GetMonster("eve"), out _), Is.True);
        Assert.That(session.TryAddPartyMonster(catalog.GetMonster("rin"), out _), Is.True);

        var artifactNames = new[]
        {
            "elemental-prism",
            "ember-crown",
            "frost-lens",
            "storm-capacitor",
            "radiant-chalice",
            "black-candlestick",
            "spirit-elixir",
            "rift-gem",
            "elemental-codex"
        };
        for (var i = 0; i < artifactNames.Length - 1; i++)
        {
            Assert.That(session.PartyMembers[i / 3].Artifacts.TryAdd(artifactNames[i]), Is.True);
        }

        var testObject = new GameObject("ArtifactUITest");
        testObject.SetActive(false);
        try
        {
            var artifactUI = testObject.AddComponent<ArtifactUI>();
            Assert.That(artifactUI.PrepareChoices(session, 3), Is.EqualTo(3));

            Assert.That(session.PartyMembers[2].Artifacts.TryAdd(artifactNames[8]), Is.True);
            Assert.That(artifactUI.PrepareChoices(session, 3), Is.Zero);
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(testObject);
        }
    }

    [Test]
    /// 활성 유물 Effect Node가 기존 스킬 snapshot 마지막에 합성되는지 확인한다.
    public void PreparedArtifactModifierAppliesToSkillSnapshot()
    {
        var catalog = ReloadGameDataCatalog();
        var monster = catalog.GetMonster("sein");
        var skill = catalog.GetActiveSkill("sein", SkillSlot.A);
        var session = RunSession.Begin(monster);
        var runState = session.GetPartyMemberState(monster.MonsterName);
        runState.Artifacts.TryAdd("ember-crown");
        new ArtifactSynergyManager().PrepareStage(session, catalog);

        var model = new UnitCombatStateFactory().CreateSelectedMonster(monster, runState);
        model.SkillState.RebuildLearnedSkillState(
            model,
            new[] { skill },
            Array.Empty<PassiveSkillDefinition>());
        var snapshot = model.SkillState.CreateExecutionData(
            model,
            model.SkillState.FindBySkillName(skill.SkillName),
            null);

        Assert.That(snapshot.DamageMultiplier, Is.EqualTo(1.18f).Within(0.0001f));
    }

    [Test]
    /// 대표 속성·정령계약 개수·서로 다른 대표 속성이 Stage 유물 효과에 반영되는지 확인한다.
    public void SpiritContractArtifactsResolvePartyStateAtStageStart()
    {
        var catalog = ReloadGameDataCatalog();
        var ariel = catalog.GetMonster("ariel");
        var eve = catalog.GetMonster("eve");
        var session = RunSession.Begin(ariel);

        Assert.That(session.TryAddPartyMonster(eve, out _), Is.True);
        var arielState = session.GetPartyMemberState("ariel");
        var eveState = session.GetPartyMemberState("eve");
        arielState.Skills.AddActiveSkill("ariel-c");
        Assert.That(arielState.Artifacts.TryAdd("elemental-prism"), Is.True);
        Assert.That(arielState.Artifacts.TryAdd("spirit-elixir"), Is.True);
        Assert.That(arielState.Artifacts.TryAdd("elemental-codex"), Is.True);
        Assert.That(eveState.Artifacts.TryAdd("rift-gem"), Is.True);

        var manager = new ArtifactSynergyManager();
        manager.PrepareStage(session, catalog);

        Assert.That(manager.Synergies.GetCount("spirit-contract"), Is.EqualTo(4));
        Assert.That(
            arielState.Artifacts.ActiveArtifactEffectNames,
            Does.Contain("elemental-prism-holy-effect"));
        Assert.That(
            arielState.Artifacts.ActiveArtifactEffectNames,
            Does.Not.Contain("elemental-prism-lightning-effect"));
        Assert.That(
            arielState.Artifacts.ActiveArtifactEffectNames.Count(
                Name => Name == "spirit-elixir-contract-count-effect"),
            Is.EqualTo(4));
        Assert.That(
            arielState.Artifacts.ActiveArtifactEffectNames.Count(
                Name => Name == "elemental-codex-effect"),
            Is.EqualTo(2));
        Assert.That(
            arielState.Artifacts.ActiveArtifactEffectNames,
            Does.Not.Contain("rift-gem-effect"));
        Assert.That(
            eveState.Artifacts.ActiveArtifactEffectNames,
            Does.Contain("rift-gem-effect"));
        Assert.That(
            eveState.Artifacts.ActiveArtifactEffectNames,
            Does.Contain("elemental-prism-holy-effect"));
        Assert.That(
            eveState.Artifacts.ActiveArtifactEffectNames,
            Does.Not.Contain("spirit-elixir-contract-count-effect"));
        Assert.That(
            eveState.Artifacts.ActiveArtifactEffectNames,
            Does.Not.Contain("elemental-codex-effect"));

        var model = new UnitCombatStateFactory().CreateSelectedMonster(ariel, arielState);
        model.SkillState.RebuildLearnedSkillState(
            model,
            catalog.GetActiveSkills(ariel.MonsterName),
            catalog.GetPassiveSkills(ariel.MonsterName));
        var snapshot = model.SkillState.CreateExecutionData(
            model,
            model.SkillState.FindBySkillName("ariel-a"),
            null);

        Assert.That(snapshot.DamageMultiplier, Is.EqualTo(1.12f * 1.18f * 1.08f).Within(0.0001f));
    }

    [Test]
    /// 균열 보석과 공명 나침반이 기존 Trigger 결과로 생성되는지 확인한다.
    public void SpiritContractTriggerArtifactsBuildExistingRuntimeReactions()
    {
        var catalog = ReloadGameDataCatalog();
        var rift = catalog.GetData<ArtifactEffectDefinition>("rift-gem-effect");
        var compass = catalog.GetData<ArtifactEffectDefinition>("resonance-compass-effect");

        Assert.That(
            Array.TrueForAll(
                rift.Reactions,
                reaction => reaction.Event == SkillTriggerEvent.CombatStart
                    && reaction.Effect?.ResolvedDefinition is BuffSkillDefinition),
            Is.True);
        Assert.That(
            Array.TrueForAll(
                compass.Reactions,
                reaction => reaction.Event == SkillTriggerEvent.OnOutgoingDamage
                    && reaction.ProcChance == 0.08f
                    && reaction.DamageValueSource
                        == SkillTriggerDamageValueSource.EventAppliedDamage
                    && reaction.DamageValueMultiplier == 0.30f
                    && reaction.Effect?.ResolvedDefinition is SingleSkillDefinition),
            Is.True);
    }

    [Test]
    /// 반응 Node가 최종 실행값을 만드는지 확인한다.
    public void TriggerNodesGenerateFinalRuntimeOutcomes()
    {
        GameDataLoader.EnsureInitialized();
        var catalog = GameDataLoader.CurrentCatalog;
        var triggers = new List<SkillReaction>();
        foreach (var monster in catalog.Monsters)
        {
            triggers.AddRange(CollectReactions(
                monster.ActiveSkills,
                monster.PassiveSkills));
        }

        Assert.That(triggers, Has.Count.EqualTo(82));
        Assert.That(
            triggers.FindAll(trigger => trigger.Effect != null),
            Has.Count.EqualTo(62));
        Assert.That(
            triggers.FindAll(trigger =>
                trigger.Effect != null
                && trigger.Effect.ResolvedDefinition == null),
            Is.Empty);
        Assert.That(
            triggers.FindAll(trigger => trigger.Command != null),
            Has.Count.EqualTo(20));
        Assert.That(
            triggers.FindAll(trigger =>
                trigger.Command?.Kind == SkillReactionCommandKind.RefundCooldown),
            Has.Count.EqualTo(14));
        Assert.That(
            triggers.FindAll(trigger =>
                trigger.Command?.Kind == SkillReactionCommandKind.ReduceReload),
            Has.Count.EqualTo(6));
        Assert.That(
            triggers.FindAll(trigger =>
                trigger.Command?.Kind == SkillReactionCommandKind.ExtendStatusDuration),
            Is.Empty);
        var zoneRecast = triggers.Find(
            trigger => trigger.ReactionName == "eve-e-master-1");
        Assert.That(zoneRecast?.Effect, Is.Not.Null);
        Assert.That(zoneRecast.Effect.IsRecast, Is.True);
        Assert.That(
            zoneRecast.Effect.ResolvedDefinition,
            Is.TypeOf<ZoneSkillDefinition>());
        Assert.That(zoneRecast.DelaySeconds, Is.EqualTo(0.5f));
        Assert.That(zoneRecast.Effect.RadiusMultiplier, Is.EqualTo(0.6f));
        Assert.That(zoneRecast.Effect.DurationSeconds, Is.EqualTo(3f));
        Assert.That(zoneRecast.Effect.MaxGeneration, Is.EqualTo(1));
        Assert.That(zoneRecast.Effect.InheritSnapshot, Is.True);
        Assert.That(
            triggers.FindAll(trigger =>
                trigger.Effect == null
                && trigger.Command == null),
            Is.Empty);
        Assert.That(
            triggers.FindAll(trigger =>
                trigger.DamageValueSource != SkillTriggerDamageValueSource.Fixed),
            Has.Count.EqualTo(7));
        Assert.That(
            triggers.FindAll(trigger => trigger.PublishSkillLifecycleEvents),
            Has.Count.EqualTo(4));
        Assert.That(
            triggers.FindAll(trigger =>
                trigger.Effect?.ResolvedDefinition is SingleSkillDefinition),
            Has.Count.EqualTo(28));
        Assert.That(
            triggers.FindAll(trigger =>
                trigger.Effect?.ResolvedDefinition is BuffSkillDefinition
                && ((BuffSkillDefinition)trigger.Effect.ResolvedDefinition).EffectKind
                    == BuffEffectKind.Status),
            Has.Count.EqualTo(33));
        Assert.That(
            triggers.FindAll(trigger =>
                trigger.Effect?.ResolvedDefinition is ZoneSkillDefinition),
            Has.Count.EqualTo(1));
    }

    [Test]
    /// 반응 의미 분류 기준이 유지되는지 확인한다.
    public void TriggerSemanticClassificationBaselineIsStable()
    {
        GameDataLoader.EnsureInitialized();
        var triggers = new List<SkillReaction>();
        foreach (var monster in GameDataLoader.CurrentCatalog.Monsters)
        {
            triggers.AddRange(CollectReactions(
                monster.ActiveSkills,
                monster.PassiveSkills));
        }

        var leakedNonTriggers = triggers.FindAll(trigger =>
            trigger.Event == SkillTriggerEvent.OnCast
            || (trigger.Event == SkillTriggerEvent.OnSkillCast
                && (trigger.EventSkillNames == null || trigger.EventSkillNames.Length == 0)));
        var workingTriggers = triggers.FindAll(trigger =>
            trigger.Effect != null
            || trigger.Command != null);
        var incompleteTriggers = triggers.FindAll(trigger =>
            trigger.Effect == null
            && trigger.Command == null);
        var castEffects = new List<SkillCastEffect>();
        foreach (var monster in GameDataLoader.CurrentCatalog.Monsters)
        {
            foreach (var skill in monster.ActiveSkills)
            {
                CollectCastEffects(skill, castEffects);
            }
            foreach (var passive in monster.PassiveSkills)
            {
                CollectCastEffects(passive, castEffects);
            }
        }

        Assert.That(workingTriggers, Has.Count.EqualTo(82));
        Assert.That(incompleteTriggers, Is.Empty);
        Assert.That(leakedNonTriggers, Is.Empty);
        Assert.That(castEffects, Has.Count.EqualTo(73));
        Assert.That(
            workingTriggers.Exists(trigger =>
                trigger.ReactionName == "ariel-b-trait4-shield-expire"),
            Is.True);
        Assert.That(
            workingTriggers.Exists(trigger =>
                trigger.ReactionName == "eve-b-master-2"
                && trigger.Effect != null),
            Is.True);
        var arielShieldDamage = castEffects.Find(
            effect => effect.EffectName == "ariel-b-trait-5");
        Assert.That(arielShieldDamage, Is.Not.Null);
        var arielShieldDefinition =
            arielShieldDamage.ResolvedDefinition as BuffSkillDefinition;
        Assert.That(arielShieldDefinition, Is.Not.Null);
        Assert.That(arielShieldDefinition.AttachedStatus?.Status, Is.Not.Null);
        Assert.That(
            arielShieldDefinition.AttachedStatus.Status.Modifiers.DamageBonusRate,
            Is.EqualTo(0.12f));
        Assert.That(arielShieldDefinition.AttachedStatus.Status.Duration, Is.EqualTo(5f));
        Assert.That(
            arielShieldDefinition.Targeting.TargetSide,
            Is.EqualTo(SkillTargetSide.AllAllies));
        Assert.That(
            arielShieldDefinition.AttachedStatus.Status.ConditionalTargetStatusGroups,
            Has.Length.EqualTo(1));
        Assert.That(
            arielShieldDefinition.AttachedStatus.Status
                .ConditionalTargetStatusGroups[0].Requirements[0].Kind,
            Is.EqualTo(StatusEffectKind.Shield));
        Assert.That(
            castEffects.Exists(effect =>
                effect.EffectName == "eve-h-trait-3"),
            Is.False);
        Assert.That(
            castEffects.Exists(effect =>
                effect.EffectName == "ariel-e-trait-4"),
            Is.False);
        Assert.That(
            castEffects.Exists(effect =>
                effect.EffectName == "ariel-a-master-2"),
            Is.False);
        Assert.That(
            workingTriggers.Exists(trigger =>
                trigger.ReactionName
                    == "ariel-a-master2-holy-exposure-on-hit"
                && trigger.Effect?.ResolvedDefinition is BuffSkillDefinition),
            Is.True);
        var vegaSecondSlash = castEffects.Find(
            effect => effect.EffectName == "vega-b-master1-second-slash");
        Assert.That(vegaSecondSlash, Is.Not.Null);
        Assert.That(
            vegaSecondSlash.ResolvedDefinition?.SkillName,
            Is.EqualTo("vega-b"));
        Assert.That(vegaSecondSlash.DamageMultiplier, Is.EqualTo(0.45f));
        Assert.That(vegaSecondSlash.DelaySeconds, Is.EqualTo(0.4f));
        Assert.That(vegaSecondSlash.UseSourcePreparedAim, Is.True);
        Assert.That(
            vegaSecondSlash.OnHitStatusOverride?.Status?.Kind,
            Is.EqualTo(StatusEffectKind.Silence));
        var arielSecondWave = castEffects.Find(
            effect => effect.EffectName == "ariel-c-master-2");
        Assert.That(arielSecondWave, Is.Not.Null);
        Assert.That(arielSecondWave.UseSourcePreparedCenter, Is.True);
        Assert.That(arielSecondWave.DelaySeconds, Is.EqualTo(1f));
    }

    [Test]
    /// Stage 패시브 수명과 동적 대상·시전자 조건이 카탈로그와 계산 경로에 보존되는지 확인한다.
    public void PassiveStageModifiersPreserveLifetimeAndDynamicConditions()
    {
        var catalog = ReloadGameDataCatalog();
        var effects = new List<SkillCastEffect>();
        foreach (var monster in catalog.Monsters)
        {
            foreach (var passive in monster.PassiveSkills)
            {
                CollectCastEffects(passive, effects);
            }
        }

        var passiveChoices = catalog.Monsters
            .SelectMany(monster => monster.PassiveSkills)
            .SelectMany(passive => passive.EnhancementChoices)
            .ToArray();
        Assert.That(passiveChoices, Has.Length.EqualTo(75));
        Assert.That(
            passiveChoices.All(choice => choice.ChoiceGroup == SkillChoiceGroup.PassiveEnhancement),
            Is.True);
        Assert.That(
            effects.Count(effect => effect.EffectName.Contains("-base-effect-")),
            Is.EqualTo(30));

        var modifiers = effects
            .Where(effect => effect.ResolvedDefinition is BuffSkillDefinition buff
                && buff.EffectKind == BuffEffectKind.Status
                && buff.AttachedStatus?.Status?.Kind == StatusEffectKind.PassiveBuff)
            .ToArray();
        StatusRuntimeData Modifier(string effectName) =>
            ((BuffSkillDefinition)modifiers.Single(effect =>
                effect.EffectName == effectName).ResolvedDefinition).AttachedStatus.Status;

        Assert.That(modifiers, Has.Length.EqualTo(58));
        Assert.That(modifiers.All(effect =>
        {
            var status = ((BuffSkillDefinition)effect.ResolvedDefinition)
                .AttachedStatus.Status;
            return status.Permanent && Mathf.Approximately(status.Duration, 9999f);
        }), Is.True);

        var eveShield = (BuffSkillDefinition)effects.Single(effect =>
            effect.EffectName == "eve-f-base-effect-1").ResolvedDefinition;
        Assert.That(eveShield.ShieldDuration, Is.EqualTo(12f));

        var shieldedAlly = new UnitCombatState();
        shieldedAlly.Statuses.Apply(
            Modifier("eve-f-trait-3"),
            1,
            9999f,
            permanent: true);
        Assert.That(StatusCombatRules.ActionSpeedMultiplier(shieldedAlly), Is.EqualTo(1f));
        shieldedAlly.Resources.CurrentShield = 1f;
        Assert.That(
            StatusCombatRules.ActionSpeedMultiplier(shieldedAlly),
            Is.EqualTo(1.12f).Within(0.0001f));

        var attacker = new UnitCombatState();
        var enemy = new UnitCombatState();
        attacker.Statuses.Apply(
            Modifier("vega-g-trait-3"),
            1,
            9999f,
            permanent: true);
        Assert.That(StatusCombatRules.CriticalChanceBonus(attacker, enemy), Is.Zero);
        enemy.Statuses.Apply(
            new StatusRuntimeData { Kind = StatusEffectKind.Silence },
            1,
            0f,
            permanent: true);
        enemy.Statuses.Apply(
            new StatusRuntimeData { Kind = StatusEffectKind.NameMark },
            1,
            0f,
            permanent: true);
        Assert.That(
            StatusCombatRules.CriticalChanceBonus(attacker, enemy),
            Is.EqualTo(0.10f).Within(0.0001f));

        var source = new UnitCombatState();
        var ally = new UnitCombatState();
        var aura = ally.Statuses.Apply(
            Modifier("vega-h-base-effect-1"),
            1,
            9999f,
            permanent: true);
        aura.SetSourceUnit(source);
        Assert.That(StatusCombatRules.ActionSpeedMultiplier(ally), Is.EqualTo(1f));

        source.Statuses.Apply(
            new StatusRuntimeData
            {
                Kind = StatusEffectKind.SlaughterPermit
            },
            1,
            0f,
            permanent: true);
        Assert.That(
            StatusCombatRules.ActionSpeedMultiplier(ally),
            Is.EqualTo(1.12f).Within(0.0001f));

        var seinOwner = new UnitCombatState();
        var seinBase = catalog.GetData<PassiveSkillDefinition>("sein-i");
        var seinTarget = catalog.GetData<SkillDefinition>("sein-d");
        seinOwner.Skills.AddPassiveSkill(seinBase.SkillName);
        seinOwner.Skills.AddActiveSkill(seinTarget.SkillName);
        seinOwner.SkillState.RebuildLearnedSkillState(
            seinOwner,
            new[] { seinTarget },
            new[] { seinBase });
        var seinSnapshot = seinOwner.SkillState.CreateExecutionData(
            seinOwner,
            seinOwner.SkillState.FindBySkillName(seinTarget.SkillName),
            null);
        Assert.That(seinSnapshot.ShotIntervalMultiplier, Is.EqualTo(0.8f).Within(0.0001f));

        var vegaOwner = new UnitCombatState();
        var vegaBase = catalog.GetData<PassiveSkillDefinition>("vega-h");
        var vegaTarget = catalog.GetData<SkillDefinition>("vega-c");
        vegaOwner.Skills.AddPassiveSkill(vegaBase.SkillName);
        vegaOwner.Skills.AddActiveSkill(vegaTarget.SkillName);
        vegaOwner.SkillState.RebuildLearnedSkillState(
            vegaOwner,
            new[] { vegaTarget },
            new[] { vegaBase });
        var vegaSnapshot = vegaOwner.SkillState.CreateExecutionData(
            vegaOwner,
            vegaOwner.SkillState.FindBySkillName(vegaTarget.SkillName),
            null);
        Assert.That(vegaSnapshot.DurationMultiplier, Is.EqualTo(1.2f).Within(0.0001f));
    }

    [Test]
    /// 지속 사건 효과가 공통 실행 경로를 사용하는지 확인한다.
    public void PassiveEventEffectsAndStateCommandsUseSharedRuntimePaths()
    {
        GameDataLoader.EnsureInitialized();

        var passiveReactionCount = 0;
        var passiveReactionOutcomeCount = 0;
        var passiveEffectCount = 0;
        var passiveSkillReuseCount = 0;
        var passiveCommandCount = 0;
        var cooldownRefundCount = 0;
        var reloadReductionCount = 0;
        foreach (var monster in GameDataLoader.CurrentCatalog.Monsters)
        {
            foreach (var trigger in CollectReactions(
                monster.ActiveSkills,
                monster.PassiveSkills))
            {
                var passiveOwned = false;
                for (var i = 0; i < monster.PassiveSkills.Length; i++)
                {
                    if (string.Equals(
                        monster.PassiveSkills[i]?.SkillName,
                        trigger.SourceSkillName,
                        StringComparison.OrdinalIgnoreCase))
                    {
                        passiveOwned = true;
                        break;
                    }
                }
                if (passiveOwned)
                {
                    passiveReactionCount++;
                    if (trigger.Effect != null
                        || trigger.Command != null)
                    {
                        passiveReactionOutcomeCount++;
                    }
                    if (trigger.Effect != null)
                    {
                        passiveEffectCount++;
                    }
                    else if (trigger.Command != null)
                    {
                        passiveCommandCount++;
                    }
                }

                if (trigger.Command?.Kind == SkillReactionCommandKind.RefundCooldown)
                {
                    cooldownRefundCount++;
                }
                else if (trigger.Command?.Kind == SkillReactionCommandKind.ReduceReload)
                {
                    reloadReductionCount++;
                }
            }
        }

        Assert.That(passiveReactionCount, Is.EqualTo(48));
        Assert.That(passiveReactionOutcomeCount, Is.EqualTo(48));
        Assert.That(passiveEffectCount, Is.EqualTo(28));
        Assert.That(passiveSkillReuseCount, Is.EqualTo(0));
        Assert.That(passiveCommandCount, Is.EqualTo(20));
        Assert.That(cooldownRefundCount, Is.EqualTo(14));
        Assert.That(reloadReductionCount, Is.EqualTo(6));
    }

    [Test]
    /// 런타임 종류가 기존 실행 계열과 맞는지 확인한다.
    public void RuntimeKindsMatchExistingExecutorFamilies()
    {
        GameDataLoader.EnsureInitialized();
        var definitions = new List<SkillDefinition>();
        foreach (var monster in GameDataLoader.CurrentCatalog.Monsters)
        {
            definitions.AddRange(monster.ActiveSkills);
        }
        foreach (var enemy in GameDataLoader.CurrentCatalog.StageOneEnemies)
        {
            definitions.AddRange(enemy.ActiveSkills);
        }
        foreach (var enemy in GameDataLoader.CurrentCatalog.StageTwoEnemies)
        {
            definitions.AddRange(enemy.ActiveSkills);
        }
        foreach (var summon in GameDataLoader.CurrentCatalog.Summons)
        {
            definitions.AddRange(summon.ActiveSkills);
        }

        foreach (var definition in definitions)
        {
            switch (definition.RuntimeKind)
            {
                case SkillRuntimeKind.MagazineProjectile:
                case SkillRuntimeKind.CooldownProjectile:
                    Assert.That(definition, Is.TypeOf<ProjectileSkillDefinition>(), definition.SkillName);
                    break;
                case SkillRuntimeKind.LineAttack:
                    Assert.That(definition, Is.TypeOf<LineSkillDefinition>(), definition.SkillName);
                    break;
                case SkillRuntimeKind.SingleAttack:
                case SkillRuntimeKind.Mark:
                case SkillRuntimeKind.Execute:
                    Assert.That(definition, Is.TypeOf<SingleSkillDefinition>(), definition.SkillName);
                    break;
                case SkillRuntimeKind.AreaAttack:
                    Assert.That(
                        definition is SingleSkillDefinition || definition is ZoneSkillDefinition,
                        Is.True,
                        definition.SkillName);
                    break;
                case SkillRuntimeKind.Buff:
                case SkillRuntimeKind.Shield:
                case SkillRuntimeKind.Heal:
                    Assert.That(definition, Is.TypeOf<BuffSkillDefinition>(), definition.SkillName);
                    break;
                default:
                    Assert.Fail(
                        definition.SkillName + " has unsupported runtime kind "
                        + definition.RuntimeKind);
                    break;
            }
        }
    }

    /// 시전 효과 목록을 테스트 입력으로 모은다.
    private static void CollectCastEffects(
        SkillDefinition skill,
        List<SkillCastEffect> effects)
    {
        if (skill == null)
        {
            return;
        }
        effects.AddRange(SkillExecutionRules.CreateDefinitionSnapshot(skill).CastEffects);
        CollectCastEffects(skill.EnhancementChoices, effects);
        CollectCastEffects(skill.MasterChoices, effects);
    }

    private static void CollectCastEffects(
        SkillChoice[] choices,
        List<SkillCastEffect> effects)
    {
        for (var i = 0; choices != null && i < choices.Length; i++)
        {
            if (choices[i] != null)
            {
                var snapshot = new SkillExecutionState(null);
                SkillExecutionRules.ApplyChoice(snapshot, choices[i]);
                effects.AddRange(snapshot.CastEffects);
            }
        }
    }

    /// 반응 목록을 테스트 입력으로 모은다.
    private static List<SkillReaction> CollectReactions(
        SkillDefinition[] skills,
        PassiveSkillDefinition[] passives = null)
    {
        var reactions = new List<SkillReaction>();
        for (var i = 0; skills != null && i < skills.Length; i++)
        {
            if (skills[i] != null)
            {
                reactions.AddRange(SkillExecutionRules.CreateDefinitionSnapshot(skills[i]).Reactions);
            }
        }
        for (var i = 0; passives != null && i < passives.Length; i++)
        {
            if (passives[i] != null)
            {
                reactions.AddRange(SkillExecutionRules.CreateDefinitionSnapshot(passives[i]).Reactions);
            }
        }
        return reactions;
    }

    [Test]
    /// 상태 카탈로그가 생성된 런타임 값을 사용하는지 확인한다.
    public void StatusCatalogUsesGeneratedRuntimeData()
    {
        GameDataLoader.EnsureInitialized();
        var catalog = GameDataLoader.CurrentCatalog;

        Assert.That(catalog.StatusEffects, Is.Not.Empty);
        foreach (var definition in catalog.StatusEffects)
        {
            Assert.That(definition.RuntimeData, Is.Not.Null, definition.StatusEffectName);
            Assert.That(
                catalog.GetStatusRuntimeData(definition.Kind),
                Is.SameAs(definition.RuntimeData),
                definition.StatusEffectName);
            Assert.That(definition.RuntimeData.Definition, Is.SameAs(definition));
        }
    }

    [Test]
    /// 충돌 Resolver가 겹침과 이동 판정을 사용하는지 확인한다.
    public void CollisionResolverUsesOverlapAndMovementCast()
    {
        var rosterObject = new GameObject("CollisionTestRoster");
        var sourceObject = new GameObject("CollisionTestSource");
        var targetObject = new GameObject("CollisionTestTarget");

        try
        {
            var roster = rosterObject.AddComponent<UnitSpawnManager>();
            var sourceCollider = sourceObject.AddComponent<BoxCollider2D>();
            targetObject.AddComponent<BoxCollider2D>();
            targetObject.transform.position = new Vector3(0.5f, 0f, 0f);

            var sourceModel = CreateCollisionTestUnit("source", UnitSide.Player);
            var targetModel = CreateCollisionTestUnit("target", UnitSide.Enemy);
            var register = typeof(UnitSpawnManager).GetMethod(
                "RegisterUnit",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(register, Is.Not.Null);
            register.Invoke(roster, new object[] { sourceModel, sourceCollider, sourceObject.transform });
            var targetEntry = (CombatUnitEntry)register.Invoke(
                roster,
                new object[] { targetModel, targetObject.transform, targetObject.transform });

            var results = new List<CombatUnitEntry>();
            CollectCollisionTargets(
                roster,
                new[] { targetEntry },
                new Collider2D[] { sourceCollider },
                Vector2.zero,
                results);
            Assert.That(results, Is.EqualTo(new[] { targetEntry }));

            targetObject.transform.position = new Vector3(3f, 0f, 0f);
            CollectCollisionTargets(
                roster,
                new[] { targetEntry },
                new Collider2D[] { sourceCollider },
                new Vector2(3f, 0f),
                results);
            Assert.That(results, Is.EqualTo(new[] { targetEntry }));

            sourceObject.transform.position = new Vector3(3f, 0f, 0f);
            targetObject.transform.position = new Vector3(1.5f, 0f, 0f);
            CollectCollisionTargets(
                roster,
                new[] { targetEntry },
                new Collider2D[] { sourceCollider },
                new Vector2(-3f, 0f),
                results);
            Assert.That(results, Is.EqualTo(new[] { targetEntry }));
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(targetObject);
            UnityEngine.Object.DestroyImmediate(sourceObject);
            UnityEngine.Object.DestroyImmediate(rosterObject);
        }
    }

    /// 충돌 검증용 전투 모델을 만든다.
    private static UnitCombatState CreateCollisionTestUnit(string unitName, UnitSide side)
    {
        var unit = new UnitCombatState();
        unit.Identity.UnitName = unitName;
        unit.Identity.Side = side;
        unit.Resources.CurrentHealth = 1f;
        return unit;
    }

    /// 반복 EditMode 실행에서도 Unity 객체 수명에 남은 정적 카탈로그를 재사용하지 않는다.
    private static GameDataCatalog ReloadGameDataCatalog()
    {
        var flags = BindingFlags.Static | BindingFlags.NonPublic;
        typeof(GameDataLoader).GetField("runtimeCatalog", flags)?.SetValue(null, null);
        typeof(GameDataLoader).GetField("initialized", flags)?.SetValue(null, false);
        typeof(GameDataLoader).GetField("failed", flags)?.SetValue(null, false);
        GameDataLoader.EnsureInitialized();
        return GameDataLoader.CurrentCatalog;
    }

    private static StatusRuntimeData DamageStatus(
        StatusEffectKind kind,
        float bonus,
        DamageAttribute attribute)
    {
        return new StatusRuntimeData
        {
            Kind = kind,
            HasElementModifierTarget = true,
            ElementModifierTarget = attribute,
            Modifiers = new BuffModifierSpec { DamageBonusRate = bonus }
        };
    }

    private static StatusRuntimeData IncomingDamageStatus(StatusEffectKind kind, float bonus)
    {
        return new StatusRuntimeData
        {
            Kind = kind,
            DamageTakenBonus = bonus
        };
    }

    private static StatusRuntimeData ShieldStatus(string sourceSkillName)
    {
        return new StatusRuntimeData
        {
            Kind = StatusEffectKind.Shield,
            StatusTag = "shield",
            StatusName = sourceSkillName,
            SourceSkillName = sourceSkillName,
            MergePolicy = StatusMergePolicy.SameSourceTakeHighest,
            ShieldAmountRefreshPolicy = ShieldRefreshRule.TakeHighest
        };
    }

    private static T? GetNodeOperation<T>(SkillNode node) where T : struct
    {
        var method = typeof(SkillNode).GetMethod(
            "GetOperation",
            BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.That(method, Is.Not.Null);
        return (T?)method.MakeGenericMethod(typeof(T)).Invoke(node, null);
    }

    private static void AddActiveArtifactEffect(UnitCombatState target, string effectName)
    {
        var method = typeof(ArtifactState).GetMethod(
            "AddActiveEffect",
            BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.That(method, Is.Not.Null);
        method.Invoke(target.Artifacts, new object[] { effectName });
    }

    private static void SetCooldownRemaining(SkillExecutionState runtime, float value)
    {
        var property = typeof(SkillExecutionState).GetProperty(
            "CooldownRemaining",
            BindingFlags.Instance | BindingFlags.Public);
        Assert.That(property, Is.Not.Null);
        property.SetValue(runtime, value);
    }

    private static StatusRuntimeData CriticalDamageStatus(StatusEffectKind kind, float bonus)
    {
        return new StatusRuntimeData
        {
            Kind = kind,
            Modifiers = new BuffModifierSpec { CritDamageBonusRate = bonus }
        };
    }

    /// 지속 스킬 검증용 정의를 만든다.
    private static PassiveSkillDefinition CreatePassive(
        string skillName,
        PassiveModifierKind kind,
        float value,
        DamageAttribute? attribute = null)
    {
        return new PassiveSkillDefinition
        {
            SkillName = skillName,
            IsActive = false,
            ModifierKind = kind,
            HasModifierAttribute = attribute.HasValue,
            ModifierAttribute = attribute.GetValueOrDefault(),
            ModifierValue = value
        };
    }

    /// 충돌 검증 대상 목록을 모은다.
    private static void CollectCollisionTargets(
        UnitSpawnManager roster,
        IReadOnlyList<CombatUnitEntry> candidates,
        IReadOnlyList<Collider2D> hitboxes,
        Vector2 movement,
        List<CombatUnitEntry> results)
    {
        var resolver = typeof(UnitSpawnManager).Assembly.GetType("Pakuri.InGame.UnitCollisionResolver");
        var collect = resolver?.GetMethod(
            "CollectTargets",
            BindingFlags.Static | BindingFlags.Public,
            null,
            new[]
            {
                typeof(UnitSpawnManager),
                typeof(IReadOnlyList<CombatUnitEntry>),
                typeof(IReadOnlyList<Collider2D>),
                typeof(Vector2),
                typeof(List<CombatUnitEntry>)
            },
            null);
        Assert.That(collect, Is.Not.Null);
        collect.Invoke(null, new object[] { roster, candidates, hitboxes, movement, results });
    }
}
