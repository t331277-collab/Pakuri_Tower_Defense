using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using Pakuri.Combat;
using Pakuri.Data;
using Pakuri.InGame;
using UnityEngine;

public sealed class SkillCatalogRuntimeTests
{
    [Test]
    public void ChoiceNodesApplyOnlyToTheirTargetSkill()
    {
        var skill = new SkillDefinition { SkillId = "skill-a" };
        var choice = new SkillChoice
        {
            Nodes = new[]
            {
                SkillNode.FromOperation(new DamageModifierOp(DamageModifierOpKind.BossMultiplier, 2f), "skill-a"),
                SkillNode.FromOperation(new DamageModifierOp(DamageModifierOpKind.BossMultiplier, 3f), "skill-b")
            }
        };
        var data = new SkillExecutionData(skill);

        data.ApplyChoiceSpec(choice);

        Assert.That(data.DamageModifierOps, Has.Count.EqualTo(1));
        Assert.That(data.DamageModifierOps[0].Multiplier, Is.EqualTo(2f));
    }

    [Test]
    public void CatalogAndRebuildReuseFinalDefinition()
    {
        var catalog = ScriptableObject.CreateInstance<GameDataCatalog>();
        var monster = ScriptableObject.CreateInstance<MonsterDefinition>();
        var skill = new SkillDefinition { SkillId = "skill-a", Slot = SkillSlot.A };

        try
        {
            monster.MonsterId = "monster-a";
            monster.ActiveSkills = new[] { skill };
            catalog.Monsters = new[] { monster };
            catalog.RebuildLookup();

            Assert.That(catalog.GetData<SkillDefinition>("skill-a"), Is.SameAs(skill));
            Assert.That(catalog.GetActiveSkill("monster-a", SkillSlot.A), Is.SameAs(skill));

            var owner = new UnitCombatState();
            owner.Skills.AddActiveSkill(skill.SkillId);
            SkillExecution.RebuildLearnedSkillState(
                owner,
                new[] { skill },
                Array.Empty<PassiveSkillDefinition>());
            var firstState = owner.SkillState.FindBySkillId("skill-a");
            SkillExecution.RebuildLearnedSkillState(
                owner,
                new[] { skill },
                Array.Empty<PassiveSkillDefinition>());
            var secondState = owner.SkillState.FindBySkillId("skill-a");

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
    public void EnemySpawnLearnsAssignedSkillsThroughSharedRuntime()
    {
        var enemy = ScriptableObject.CreateInstance<EnemyDefinition>();
        var active = new SkillDefinition
        {
            SkillId = "enemy-active",
            IsActive = true
        };
        var passive = new PassiveSkillDefinition
        {
            SkillId = "enemy-passive",
            IsActive = false,
            ModifierKind = PassiveModifierKind.DamageUp,
            HasModifierAttribute = true,
            ModifierAttribute = DamageAttribute.Physical,
            ModifierValue = 0.1f
        };

        try
        {
            enemy.EnemyId = "enemy-a";
            enemy.ActiveSkills = new[] { active };
            enemy.PassiveSkill = passive;

            var model = new UnitCombatStateFactory().CreateEnemy(enemy);
            SkillExecution.RebuildLearnedSkillState(
                model,
                enemy.ActiveSkills,
                new[] { enemy.PassiveSkill });

            Assert.That(model.Skills.HasActiveSkill(active.SkillId), Is.True);
            Assert.That(model.Skills.HasPassiveSkill(passive.SkillId), Is.True);
            Assert.That(model.SkillState.FindBySkillId(active.SkillId).Data, Is.SameAs(active));
            Assert.That(model.SkillState.FindBySkillId(passive.SkillId).Data, Is.SameAs(passive));
            Assert.That(
                model.SkillState.PassiveOutgoingDamageBonus(DamageAttribute.Physical),
                Is.EqualTo(0.1f).Within(0.0001f));
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(enemy);
        }
    }

    [Test]
    public void SharedPassiveRuntimePreservesEnemyModifierKinds()
    {
        var owner = new UnitCombatState();
        var passives = new[]
        {
            CreatePassive("damage", PassiveModifierKind.DamageUp, 0.1f, DamageAttribute.Fire),
            CreatePassive("defense", PassiveModifierKind.DefenseUp, 0.1f),
            CreatePassive("crit-chance", PassiveModifierKind.CritChanceUp, 0.08f),
            CreatePassive("crit-damage", PassiveModifierKind.CritDamageUp, 0.2f),
            CreatePassive("healing", PassiveModifierKind.HealingUp, 0.15f),
            CreatePassive("incoming", PassiveModifierKind.IncomingDamageDown, 0.12f)
        };
        for (var i = 0; i < passives.Length; i++)
        {
            owner.Skills.AddPassiveSkill(passives[i].SkillId);
        }

        SkillExecution.RebuildLearnedSkillState(
            owner,
            Array.Empty<SkillDefinition>(),
            passives);

        Assert.That(owner.SkillState.PassiveOutgoingDamageBonus(DamageAttribute.Fire), Is.EqualTo(0.1f).Within(0.0001f));
        Assert.That(owner.SkillState.PassiveOutgoingDamageBonus(DamageAttribute.Ice), Is.Zero.Within(0.0001f));
        Assert.That(owner.SkillState.PassiveDefenseMultiplier(DamageAttribute.Holy), Is.EqualTo(1.1f).Within(0.0001f));
        Assert.That(owner.SkillState.PassiveCriticalChanceBonus(), Is.EqualTo(0.08f).Within(0.0001f));
        Assert.That(owner.SkillState.PassiveCriticalDamageBonus(), Is.EqualTo(0.2f).Within(0.0001f));
        Assert.That(owner.SkillState.PassiveHealingMultiplier(), Is.EqualTo(1.15f).Within(0.0001f));
        Assert.That(owner.SkillState.PassiveIncomingDamageBonus(), Is.EqualTo(-0.12f).Within(0.0001f));
    }

    [Test]
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
            Assert.That(enemy.PassiveSkill, Is.Not.Null, enemy.EnemyId);
            Assert.That(enemy.PassiveSkill.ModifierKind, Is.Not.EqualTo(PassiveModifierKind.None), enemy.EnemyId);
            Assert.That(
                catalog.GetData<PassiveSkillDefinition>(enemy.PassiveSkill.SkillId),
                Is.SameAs(enemy.PassiveSkill),
                enemy.EnemyId);

            var model = new UnitCombatStateFactory().CreateEnemy(enemy);
            SkillExecution.RebuildLearnedSkillState(
                model,
                enemy.ActiveSkills,
                new[] { enemy.PassiveSkill });

            Assert.That(model.Skills.HasPassiveSkill(enemy.PassiveSkill.SkillId), Is.True, enemy.EnemyId);
            Assert.That(
                model.SkillState.FindBySkillId(enemy.PassiveSkill.SkillId)?.Data,
                Is.SameAs(enemy.PassiveSkill),
                enemy.EnemyId);
        }
    }

    [Test]
    public void EnemySkillProfilesUseUnifiedDefinitionFamilies()
    {
        GameDataLoader.EnsureInitialized();
        var catalog = GameDataLoader.CurrentCatalog;

        var chainEnemy = Array.Find(
            catalog.StageTwoEnemies,
            enemy => enemy.EnemyId == "stage2-lightning-scout");
        var chainSkill = Array.Find(
            chainEnemy.ActiveSkills,
            skill => skill.SkillId == "ChainLightning");
        var chainTrigger = Array.Find(
            chainEnemy.SkillTriggers,
            trigger => trigger.TriggerId == "ChainLightning__chain_on_hit");

        Assert.That(chainSkill, Is.TypeOf<SingleSkillDefinition>());
        Assert.That(chainTrigger, Is.Not.Null);
        Assert.That(chainTrigger.TriggerEvent, Is.EqualTo(SkillTriggerEvent.OnHit));
        Assert.That(chainTrigger.TriggerDelaySeconds, Is.EqualTo(0.5f).Within(0.0001f));
        Assert.That(chainTrigger.TriggeredDamageMultiplier, Is.EqualTo(0.5f).Within(0.0001f));
        Assert.That(chainTrigger.PublishSkillLifecycleEvents, Is.False);
        Assert.That(chainTrigger.TriggeredSkill, Is.TypeOf<SingleSkillDefinition>());
        Assert.That(
            chainTrigger.TriggeredSkill.Targeting.Selection,
            Is.EqualTo(SkillTargetSelection.NearestOtherFromEventTarget));
        Assert.That(chainTrigger.TriggeredSkill.Targeting.Radius, Is.EqualTo(7f).Within(0.0001f));

        var chargeEnemy = Array.Find(
            catalog.StageTwoEnemies,
            enemy => enemy.EnemyId == "stage2-drake");
        var chargeSkill = Array.Find(
            chargeEnemy.ActiveSkills,
            skill => skill.SkillId == "OpeningCharge");
        Assert.That(chargeSkill, Is.TypeOf<BuffSkillDefinition>());
        Assert.That(
            ((BuffSkillDefinition)chargeSkill).EffectKind,
            Is.EqualTo(BuffEffectKind.Charge));

        var shieldEnemy = Array.Find(
            catalog.StageOneEnemies,
            enemy => enemy.EnemyId == "stage1-guardian-captain");
        var shieldSkill = Array.Find(
            shieldEnemy.ActiveSkills,
            skill => skill.SkillId == "GuardianFlag");
        Assert.That(shieldSkill, Is.TypeOf<BuffSkillDefinition>());
        Assert.That(
            ((BuffSkillDefinition)shieldSkill).EffectKind,
            Is.EqualTo(BuffEffectKind.Shield));
    }

    [Test]
    public void TriggeredChargeUsesSharedActiveRuntime()
    {
        var actorObject = new GameObject("TriggeredChargeActor");

        try
        {
            var owner = new EnemyCombatState();
            owner.Resources.CurrentHealth = 1f;
            var charge = new BuffSkillDefinition
            {
                SkillId = "charge",
                IsActive = true,
                EffectKind = BuffEffectKind.Charge,
                Timing = new SkillTimingSpec
                {
                    Cooldown = 30f,
                    ActiveDuration = 5f
                }
            };
            var runtime = new SkillUseState(owner, charge);
            owner.SkillState.AddOrReplace(runtime);
            var entry = new CombatUnitEntry(owner, actorObject.transform);
            var trigger = new SkillTriggerDefinition
            {
                SourceSkillId = charge.SkillId,
                TriggeredSkill = charge,
                UsesExistingSkillRuntime = true,
                PublishSkillLifecycleEvents = false
            };

            var executed = new SkillExecution().TryExecuteTriggered(
                entry,
                runtime,
                trigger,
                null,
                null,
                null,
                Vector2.zero,
                false,
                false,
                0f,
                0);

            Assert.That(executed, Is.True);
            Assert.That(runtime.IsActive, Is.True);
            Assert.That(runtime.ActiveDurationRemaining, Is.EqualTo(5f).Within(0.0001f));

            var decision = typeof(SkillExecution).Assembly.GetType("Pakuri.InGame.EnemyCombatDecision");
            var resolveCharge = decision?.GetMethod(
                "ResolveActiveCharge",
                BindingFlags.Static | BindingFlags.Public);
            Assert.That(resolveCharge, Is.Not.Null);
            Assert.That(resolveCharge.Invoke(null, new object[] { owner }), Is.SameAs(runtime));

            runtime.StopActive();

            Assert.That(runtime.IsActive, Is.False);
            Assert.That(resolveCharge.Invoke(null, new object[] { owner }), Is.Null);
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(actorObject);
        }
    }

    [Test]
    public void TriggeredPreparationKeepsTriggeredDefinitionIdentity()
    {
        var actorObject = new GameObject("TriggeredIdentityActor");

        try
        {
            var owner = new EnemyCombatState();
            var source = new SingleSkillDefinition { SkillId = "source-skill" };
            var triggered = new BuffSkillDefinition
            {
                SkillId = "triggered-charge",
                EffectKind = BuffEffectKind.Charge
            };
            var sourceSnapshot = new SkillExecutionData(source);
            var triggeredRuntime = new SkillUseState(owner, triggered);
            var context = new SkillExecutionContext(
                null,
                null,
                new CombatUnitEntry(owner, actorObject.transform),
                triggeredRuntime);
            var prepare = typeof(SkillExecution).GetMethod(
                "PrepareExecutionData",
                BindingFlags.Static | BindingFlags.NonPublic);
            var preparedSkillId = typeof(SkillExecutionData).GetProperty(
                "PreparedSkillId",
                BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.That(prepare, Is.Not.Null);
            Assert.That(preparedSkillId, Is.Not.Null);
            Assert.That(
                prepare.Invoke(null, new object[] { context, sourceSnapshot, triggered }),
                Is.True);
            Assert.That(
                preparedSkillId.GetValue(sourceSnapshot),
                Is.EqualTo(triggered.SkillId));
            Assert.That(sourceSnapshot.SkillId, Is.EqualTo(source.SkillId));
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(actorObject);
        }
    }

    [Test]
    public void MonsterRuntimeSharesRunSessionSkills()
    {
        var monster = ScriptableObject.CreateInstance<MonsterDefinition>();

        try
        {
            monster.MonsterId = "monster-a";
            var session = RunSession.Begin(monster);
            var runState = session.GetPartyMemberState(monster.MonsterId);
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
    public void TriggerNodesGenerateFinalRuntimeOutcomes()
    {
        GameDataLoader.EnsureInitialized();
        var catalog = GameDataLoader.CurrentCatalog;
        var triggers = new List<SkillTriggerDefinition>();
        foreach (var monster in catalog.Monsters)
        {
            triggers.AddRange(monster.SkillTriggers);
        }

        Assert.That(triggers, Has.Count.EqualTo(158));
        Assert.That(
            triggers.FindAll(trigger => trigger.TriggeredSkill != null),
            Has.Count.EqualTo(55));
        Assert.That(
            triggers.FindAll(trigger => trigger.Command != null),
            Has.Count.EqualTo(22));
        Assert.That(
            triggers.FindAll(trigger =>
                trigger.Command?.Kind == SkillTriggerCommandKind.RecastZone),
            Has.Count.EqualTo(1));
        Assert.That(
            triggers.FindAll(trigger =>
                trigger.Command?.Kind == SkillTriggerCommandKind.RefundCooldown),
            Has.Count.EqualTo(14));
        Assert.That(
            triggers.FindAll(trigger =>
                trigger.Command?.Kind == SkillTriggerCommandKind.ReduceReload),
            Has.Count.EqualTo(6));
        Assert.That(
            triggers.FindAll(trigger =>
                trigger.Command?.Kind == SkillTriggerCommandKind.ExtendStatusDuration),
            Has.Count.EqualTo(1));
        Assert.That(
            triggers.FindAll(trigger =>
                trigger.TriggeredSkill == null && trigger.Command == null),
            Has.Count.EqualTo(81));
        Assert.That(
            triggers.FindAll(trigger =>
                trigger.DamageValueSource != SkillTriggerDamageValueSource.Fixed),
            Has.Count.EqualTo(7));
        Assert.That(
            triggers.FindAll(trigger => trigger.UsesExistingSkillRuntime),
            Has.Count.EqualTo(4));
        Assert.That(
            triggers.FindAll(trigger => trigger.PublishSkillLifecycleEvents),
            Has.Count.EqualTo(4));
        Assert.That(
            triggers.FindAll(trigger =>
                trigger.TriggeredSkill != null
                && !trigger.UsesExistingSkillRuntime
                && trigger.PublishSkillLifecycleEvents),
            Is.Empty);
        Assert.That(
            triggers.FindAll(trigger =>
                trigger.TriggeredSkill is SingleSkillDefinition),
            Has.Count.EqualTo(27));
        Assert.That(
            triggers.FindAll(trigger =>
                trigger.TriggeredSkill is BuffSkillDefinition),
            Has.Count.EqualTo(24));
        Assert.That(
            triggers.FindAll(trigger =>
                trigger.TriggeredSkill is BuffSkillDefinition buff
                && buff.EffectKind == BuffEffectKind.Shield),
                Has.Count.EqualTo(3));
    }

    [Test]
    public void TriggerSemanticClassificationBaselineIsStable()
    {
        GameDataLoader.EnsureInitialized();
        var triggers = new List<SkillTriggerDefinition>();
        foreach (var monster in GameDataLoader.CurrentCatalog.Monsters)
        {
            triggers.AddRange(monster.SkillTriggers);
        }

        var nonTriggers = triggers.FindAll(trigger =>
            trigger.TriggerEvent == SkillTriggerEvent.OnCast
            || (trigger.TriggerEvent == SkillTriggerEvent.OnSkillCast
                && (trigger.EventSkillIds == null || trigger.EventSkillIds.Length == 0)));
        var semanticTriggers = triggers.FindAll(trigger => !nonTriggers.Contains(trigger));
        var workingTriggers = semanticTriggers.FindAll(trigger =>
            trigger.TriggeredSkill != null || trigger.Command != null);
        var incompleteTriggers = semanticTriggers.FindAll(trigger =>
            trigger.TriggeredSkill == null && trigger.Command == null);

        Assert.That(workingTriggers, Has.Count.EqualTo(65));
        Assert.That(incompleteTriggers, Has.Count.EqualTo(17));
        Assert.That(nonTriggers, Has.Count.EqualTo(76));
        Assert.That(
            nonTriggers.FindAll(trigger => trigger.TriggerEvent == SkillTriggerEvent.OnCast),
            Has.Count.EqualTo(75));
        Assert.That(
            nonTriggers.FindAll(trigger =>
                trigger.TriggerId == "vega-b-master1-second-slash"),
            Has.Count.EqualTo(1));
        Assert.That(
            workingTriggers.Exists(trigger =>
                trigger.TriggerId == "ariel-b-trait4-shield-expire"),
            Is.True);
        Assert.That(
            incompleteTriggers.Exists(trigger =>
                trigger.TriggerId == "eve-b-master-2"),
            Is.True);
        Assert.That(
            nonTriggers.Exists(trigger =>
                trigger.TriggerId == "ariel-b-trait-5"),
            Is.True);
    }

    [Test]
    public void StatusCatalogUsesGeneratedRuntimeData()
    {
        GameDataLoader.EnsureInitialized();
        var catalog = GameDataLoader.CurrentCatalog;

        Assert.That(catalog.StatusEffects, Is.Not.Empty);
        foreach (var definition in catalog.StatusEffects)
        {
            Assert.That(definition.RuntimeData, Is.Not.Null, definition.StatusEffectId);
            Assert.That(
                catalog.GetStatusRuntimeData(definition.Kind),
                Is.SameAs(definition.RuntimeData),
                definition.StatusEffectId);
            Assert.That(definition.RuntimeData.Definition, Is.SameAs(definition));
        }
    }

    [Test]
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

    private static UnitCombatState CreateCollisionTestUnit(string unitId, UnitSide side)
    {
        var unit = new UnitCombatState();
        unit.Identity.UnitId = unitId;
        unit.Identity.Side = side;
        unit.Resources.CurrentHealth = 1f;
        return unit;
    }

    private static PassiveSkillDefinition CreatePassive(
        string skillId,
        PassiveModifierKind kind,
        float value,
        DamageAttribute? attribute = null)
    {
        return new PassiveSkillDefinition
        {
            SkillId = skillId,
            IsActive = false,
            ModifierKind = kind,
            HasModifierAttribute = attribute.HasValue,
            ModifierAttribute = attribute.GetValueOrDefault(),
            ModifierValue = value
        };
    }

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
