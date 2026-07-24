using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Pakuri.NewCore.Bootstrap;
using Pakuri.NewCore.Catalog;
using Pakuri.NewCore.Combat;
using Pakuri.NewCore.Combat.Actions;
using Pakuri.NewCore.Combat.Effects;
using Pakuri.NewCore.Combat.Skills.Actors;
using Pakuri.NewCore.Combat.Skills.Execution;
using Pakuri.NewCore.Definitions.Skills;
using Pakuri.NewCore.Definitions.Units;
using Pakuri.NewCore.Run;
using Pakuri.NewCore.Units.Models;
using UnityEngine;

namespace Pakuri.NewCore.Tests
{
    public sealed class NewCoreCombatLoopTests
    {
        private GameDefinitionCatalog catalog;

        [SetUp]
        public void SetUp()
        {
            catalog = new GameBootstrap(LoadSources()).Catalog;
        }

        [Test]
        public void CentralTickUsesTheBlueprintOrderExactlyOnce()
        {
            RuntimeFixture fixture = CreateFixture("ariel");
            List<CombatTickStep> steps = new List<CombatTickStep>();
            fixture.Actions.StepCompleted += steps.Add;

            fixture.Actions.Tick(0.25f);

            CollectionAssert.AreEqual(
                new[]
                {
                    CombatTickStep.PassiveBefore,
                    CombatTickStep.Cooldowns,
                    CombatTickStep.AutomaticMonsters,
                    CombatTickStep.ManualInput,
                    CombatTickStep.Enemies,
                    CombatTickStep.SkillActors,
                    CombatTickStep.Statuses,
                    CombatTickStep.PassiveAfter
                },
                steps);
            Assert.That(fixture.PassiveApplyCount, Is.EqualTo(2));
        }

        [Test]
        public void ActorRegistrationStartsNextTickAndRemovalWaitsForIterationEnd()
        {
            EffectManager effects = new EffectManager();
            SkillActorManager actors = new SkillActorManager(effects);
            EffectHandle effect = effects.Create(string.Empty, default, default);
            SingleAttackActor actor =
                new SingleAttackActor(
                    (SingleAttackDefinition)catalog.GetSkill("ariel-c"),
                    0.5f,
                    effect);

            actors.Register(actor);
            actors.Tick(0.5f);
            Assert.That(actor.ElapsedSeconds, Is.Zero);
            Assert.That(actors.ActiveActors, Has.Count.EqualTo(1));

            actors.Tick(0.5f);
            Assert.That(actor.ElapsedSeconds, Is.EqualTo(0.5f));
            Assert.That(actors.ActiveActors, Is.Empty);
            Assert.That(effect.IsActive, Is.False);
        }

        [Test]
        public void CombatResultsApplyDamageShieldHealAndStatusThroughModelAuthority()
        {
            RuntimeFixture fixture = CreateFixture("ariel");
            MonsterModel source = fixture.Selected;
            EnemyModel target = CreateEnemy("stage1-swordsman");
            target.ApplyDamage(30f);
            target.TryAddShield(20f);

            CombatResult damage = fixture.Combat.ApplySkillDamage(
                source,
                target,
                catalog.GetSkill("ariel-c"),
                1f);
            Assert.That(damage.ShieldChanged, Is.LessThan(0f));
            Assert.That(target.CurrentShield, Is.Zero);
            Assert.That(target.CurrentHealth, Is.LessThan(target.MaximumHealth - 30f));

            fixture.Combat.Heal(source, target, catalog.GetSkill("Heal"), 10f);
            Assert.That(target.CurrentHealth, Is.GreaterThan(0f));
            fixture.Combat.AddShield(source, target, catalog.GetSkill("ShieldUp"), 25f);
            Assert.That(target.CurrentShield, Is.EqualTo(25f));
            fixture.Combat.ApplyStatus(
                source,
                target,
                catalog.GetStatus("freeze"),
                null,
                1);
            Assert.That(target.CanMove, Is.False);
            Assert.That(target.CanAct, Is.False);
        }

        [Test]
        public void TargetingUsesLivingSideSelectionHealthStacksAndManualPoint()
        {
            SkillTargeting targeting = new SkillTargeting(_ => 0);
            MonsterModel source = CreateMonster("ariel", false);
            EnemyModel near = CreateEnemy("stage1-swordsman");
            EnemyModel far = CreateEnemy("stage1-archer");
            source.SetPosition(new CombatVector2(0f, 0f));
            near.SetPosition(new CombatVector2(1f, 0f));
            far.SetPosition(new CombatVector2(5f, 0f));
            near.ApplyDamage(20f);
            far.ApplyStatus(catalog.GetStatus("name-mark"), source, null, 3);
            var units = new UnitBaseModel[] { source, near, far, new NexusModel(100f) };

            Assert.That(
                targeting.Resolve(source, catalog.GetSkill("ariel-a"), units)[0],
                Is.SameAs(near),
                "Nearest");
            Assert.That(
                targeting.Resolve(source, catalog.GetSkill("ariel-d"), units)[0],
                Is.SameAs(
                    near.CurrentHealth >= far.CurrentHealth ? near : far),
                "HighestHealth");
            Assert.That(
                targeting.Resolve(source, catalog.GetSkill("vega-e"), units)[0],
                Is.SameAs(far),
                "HighestStacks");
            Assert.That(
                targeting.Resolve(
                    source,
                    catalog.GetSkill("ariel-c"),
                    units,
                    new CombatVector2(5f, 0f))[0],
                Is.SameAs(far),
                "ManualPoint");
        }

        [Test]
        public void EveryActiveSkillFamilyHasADeterministicExecutionPath()
        {
            AssertFamilyExecutes("ariel", "ariel-a");
            AssertFamilyExecutes("eve", "eve-b");
            AssertFamilyExecutes("eve", "eve-c");
            AssertFamilyExecutes("ariel", "ariel-c");
            AssertFamilyExecutes("rin", "rin-b");
            AssertEnemyFamilyExecutes("stage1-priest", "Heal");
            AssertEnemyFamilyExecutes("stage1-shieldbearer", "ShieldUp");

            RuntimeFixture passiveFixture = CreateFixture("ariel");
            Assert.That(
                passiveFixture.Combat.TryExecuteSkill(
                    new SkillExecutionRequest(
                        passiveFixture.Selected,
                        catalog.GetSkill("ariel-f"),
                        passiveFixture.Stage.FieldUnits)),
                Is.True);
        }

        [Test]
        public void SelectedChoiceNodesChangeTheSkillPlanWithoutChangingDefinitions()
        {
            RuntimeFixture baseline = CreateFixture("eve");
            baseline.Selected.SkillBucket.TryLearnActive(catalog.GetSkill("eve-c"));
            EnemyModel baselineTarget = CreateEnemy("stage1-swordsman");
            baseline.Stage.TryRegisterFieldUnit(baselineTarget);
            float before = baselineTarget.CurrentHealth;
            baseline.Combat.TryExecuteSkill(
                new SkillExecutionRequest(
                    baseline.Selected,
                    catalog.GetSkill("eve-c"),
                    baseline.Stage.FieldUnits));
            baseline.Actors.Tick(0f);
            baseline.Actors.Tick(0f);
            float baseDamage = before - baselineTarget.CurrentHealth;

            RuntimeFixture enhanced = CreateFixture("eve");
            enhanced.Selected.SkillBucket.TryLearnActive(catalog.GetSkill("eve-c"));
            Assert.That(
                enhanced.Selected.SkillBucket.TrySelectChoice(
                    catalog.GetChoice("eve-c-trait-3")),
                Is.True);
            EnemyModel enhancedTarget = CreateEnemy("stage1-swordsman");
            enhanced.Stage.TryRegisterFieldUnit(enhancedTarget);
            before = enhancedTarget.CurrentHealth;
            enhanced.Combat.TryExecuteSkill(
                new SkillExecutionRequest(
                    enhanced.Selected,
                    catalog.GetSkill("eve-c"),
                    enhanced.Stage.FieldUnits));
            enhanced.Actors.Tick(0f);
            enhanced.Actors.Tick(0f);
            float enhancedDamage = before - enhancedTarget.CurrentHealth;

            Assert.That(enhancedDamage, Is.GreaterThan(baseDamage));
            Assert.That(catalog.GetSkill("eve-c").base_damage, Is.EqualTo(8f));
        }

        [Test]
        public void ManualInputBoundaryRejectsUiAndAutoThenPreservesProjectileAim()
        {
            RuntimeFixture fixture = CreateFixture("ariel");
            var aim = new CombatVector2(1f, 0f);
            var point = new CombatVector2(3f, 0f);

            Assert.That(
                fixture.Input.SubmitManualSkillRequest(
                    catalog.GetSkill("ariel-a"),
                    aim,
                    point,
                    ManualInputPhase.Pressed,
                    false),
                Is.False);

            fixture.Input.SetAutoSkillEnabled(false);
            Assert.That(
                fixture.Input.SubmitManualSkillRequest(
                    catalog.GetSkill("ariel-a"),
                    aim,
                    point,
                    ManualInputPhase.Held,
                    true),
                Is.False);
            Assert.That(
                fixture.Input.SubmitManualSkillRequest(
                    catalog.GetSkill("ariel-a"),
                    aim,
                    point,
                    ManualInputPhase.Held,
                    false),
                Is.True);
            Assert.That(fixture.Input.ContinueProjectileBurst(catalog.GetSkill("ariel-a")), Is.True);
        }

        [Test]
        public void MovementHonorsDeltaTimeStopDistanceAndMovementStatus()
        {
            MonsterModel unit = CreateMonster("ariel", false);
            unit.SetPosition(new CombatVector2(0f, 0f));
            UnitMovementController movement = new UnitMovementController();

            Assert.That(
                movement.MoveTowards(
                    unit,
                    new CombatVector2(10f, 0f),
                    2f,
                    1f,
                    1f),
                Is.False);
            Assert.That(unit.Position.X, Is.EqualTo(2f));

            unit.ApplyStatus(catalog.GetStatus("freeze"), unit);
            movement.MoveTowards(unit, new CombatVector2(10f, 0f), 2f, 1f, 1f);
            Assert.That(unit.Position.X, Is.EqualTo(2f));
        }

        [Test]
        public void EnemyUsesNexusWhenNoLivingPlayerAndRequestsExactNexusDamage()
        {
            RuntimeFixture fixture = CreateFixture("ariel");
            fixture.Stage.TryUnregisterFieldUnit(fixture.Selected);
            EnemyModel enemy = CreateEnemy("stage1-swordsman");
            NexusModel nexus = new NexusModel(100f);
            enemy.SetPosition(new CombatVector2(0.1f, 0f));
            nexus.SetPosition(default);
            fixture.Stage.TryRegisterFieldUnit(enemy);
            fixture.Stage.TryRegisterFieldUnit(nexus);
            EnemyActionController controller = new EnemyActionController(
                enemy,
                fixture.Combat,
                fixture.Targeting,
                new UnitMovementController(),
                fixture.Stage,
                nexus,
                0.2f);

            float expected = enemy.EnemyDefinition.nexus_damage.Value;
            controller.Tick(0.1f, fixture.Stage.FieldUnits);

            Assert.That(nexus.CurrentHealth, Is.EqualTo(100f - expected));
            Assert.That(enemy.HasContactedNexus, Is.True);
            Assert.That(fixture.Stage.FieldUnits.Contains(enemy), Is.False);
        }

        [Test]
        public void CooldownBlocksRepeatedExecutionUntilCentralTickCompletesIt()
        {
            RuntimeFixture fixture = CreateFixture("ariel");
            EnemyModel target = CreateEnemy("stage1-swordsman");
            target.SetPosition(new CombatVector2(1f, 0f));
            fixture.Stage.TryRegisterFieldUnit(target);
            var request = new SkillExecutionRequest(
                fixture.Selected,
                catalog.GetSkill("ariel-a"),
                fixture.Stage.FieldUnits);

            Assert.That(fixture.Combat.TryExecuteSkill(request), Is.True);
            Assert.That(fixture.Combat.TryExecuteSkill(request), Is.False);
            fixture.Selected.SkillBucket.TickCooldowns(10f);
            Assert.That(fixture.Combat.TryExecuteSkill(request), Is.True);
        }

        [Test]
        public void EveryReachableNodeAndTriggerContractHasAnExecutableRuntimePath()
        {
            string[] nodeTypes = catalog.ChoiceNodes
                .Select(node => node.node_type_id)
                .Distinct(StringComparer.Ordinal)
                .ToArray();
            Assert.That(nodeTypes, Has.Length.EqualTo(88));
            for (int index = 0; index < nodeTypes.Length; index++)
            {
                Assert.That(catalog.NodeTypes.ContainsKey(nodeTypes[index]), Is.True);
                Assert.DoesNotThrow(() => SkillNodeSupport.Resolve(
                    catalog.NodeTypes[nodeTypes[index]].handler_id));
            }

            Assert.That(catalog.Triggers, Has.Count.EqualTo(59));
            foreach (SkillTriggerDefinition trigger in catalog.Triggers.Values)
            {
                Assert.DoesNotThrow(() => SkillTriggerSupport.Validate(trigger));
            }

            Assert.DoesNotThrow(() => CreateFixture("ariel"));
        }

        [Test]
        public void ScheduledActorHonorsInitialDelayAndExactRepeatCount()
        {
            EffectManager effects = new EffectManager();
            SkillActorManager actors = new SkillActorManager(effects);
            int executions = 0;
            actors.Register(new ScheduledSkillActor(
                catalog.GetSkill("ariel-c"),
                2,
                0.25f,
                _ => executions++,
                null,
                0.5f));

            actors.Tick(0f);
            actors.Tick(0.49f);
            Assert.That(executions, Is.Zero);
            actors.Tick(0.01f);
            Assert.That(executions, Is.EqualTo(1));
            actors.Tick(0.25f);
            Assert.That(executions, Is.EqualTo(2));
        }

        [Test]
        public void SkillOwnedEffectGraphAppliesStatusAndRuntimeModifier()
        {
            RuntimeFixture fixture = CreateFixture("ariel");
            Assert.That(
                fixture.Selected.SkillBucket.TryLearnActive(
                    catalog.GetSkill("ariel-c")),
                Is.True);
            Assert.That(
                fixture.Selected.SkillBucket.TrySelectChoice(
                    catalog.GetChoice("ariel-c-trait-1")),
                Is.True);
            Assert.That(
                fixture.Selected.SkillBucket.TrySelectChoice(
                    catalog.GetChoice("ariel-c-trait-2")),
                Is.True);
            Assert.That(
                fixture.Selected.SkillBucket.TrySelectChoice(
                    catalog.GetChoice("ariel-c-trait-3")),
                Is.True);
            Assert.That(
                fixture.Selected.SkillBucket.TrySelectChoice(
                    catalog.GetChoice("ariel-c-master-1")),
                Is.True);
            EnemyModel target = CreateEnemy("stage1-swordsman");
            fixture.Stage.TryRegisterFieldUnit(target);

            Assert.That(
                fixture.Combat.TryExecuteSkill(new SkillExecutionRequest(
                    fixture.Selected,
                    catalog.GetSkill("ariel-c"),
                    fixture.Stage.FieldUnits)),
                Is.True);

            Assert.That(
                fixture.Selected.StatusEffects.Any(status =>
                    status.Definition.status_effect_id == "blessing"),
                Is.True);
            Assert.That(
                fixture.Selected.ResolveRuntimeModifier(
                    "StatusActionSpeedBonus"),
                Is.EqualTo(0.12f).Within(0.0001f));
        }

        [Test]
        public void ProjectileBurstPiercesOrderedTargetsThroughScheduledActors()
        {
            RuntimeFixture fixture = CreateFixture("vega");
            EnemyModel first = CreateEnemy("stage1-swordsman");
            EnemyModel second = CreateEnemy("stage1-archer");
            first.SetPosition(new CombatVector2(1f, 0f));
            second.SetPosition(new CombatVector2(2f, 0f));
            fixture.Stage.TryRegisterFieldUnit(first);
            fixture.Stage.TryRegisterFieldUnit(second);
            float firstBefore = first.CurrentHealth;
            float secondBefore = second.CurrentHealth;

            Assert.That(
                fixture.Combat.TryExecuteSkill(new SkillExecutionRequest(
                    fixture.Selected,
                    catalog.GetSkill("vega-a"),
                    fixture.Stage.FieldUnits)),
                Is.True);
            for (int tick = 0; tick < 30; tick++)
            {
                fixture.Actors.Tick(0.1f);
            }

            Assert.That(first.CurrentHealth, Is.LessThan(firstBefore));
            Assert.That(second.CurrentHealth, Is.LessThan(secondBefore));
        }

        [Test]
        public void CombatStartTriggerDispatchesOwnedEnemyBuff()
        {
            RuntimeFixture fixture = CreateFixture("ariel");
            EnemyModel enemy = CreateEnemy("stage2-drake");
            fixture.Stage.TryRegisterFieldUnit(enemy);

            Assert.That(
                fixture.Combat.NotifyCombatStart(
                    enemy,
                    fixture.Stage.FieldUnits),
                Is.EqualTo(1));
            Assert.That(fixture.Actors.PendingAddCount, Is.EqualTo(1));
        }

        [Test]
        public void ChoicePlanAndGraphsStayScopedToTheirTargetSkill()
        {
            RuntimeFixture baseline = CreateFixture("ariel");
            EnemyModel baselineTarget = CreateEnemy("stage1-swordsman");
            baselineTarget.SetPosition(new CombatVector2(1f, 0f));
            baseline.Stage.TryRegisterFieldUnit(baselineTarget);
            float baselineDamage = ExecuteProjectileAndReadDamage(
                baseline,
                "ariel-a",
                baselineTarget);

            RuntimeFixture selected = CreateFixture("ariel");
            Assert.That(
                selected.Selected.SkillBucket.TryLearnActive(
                    catalog.GetSkill("ariel-c")),
                Is.True);
            Assert.That(
                selected.Selected.SkillBucket.TrySelectChoice(
                    catalog.GetChoice("ariel-c-trait-1")),
                Is.True);
            EnemyModel selectedTarget = CreateEnemy("stage1-swordsman");
            selectedTarget.SetPosition(new CombatVector2(1f, 0f));
            selected.Stage.TryRegisterFieldUnit(selectedTarget);
            float selectedDamage = ExecuteProjectileAndReadDamage(
                selected,
                "ariel-a",
                selectedTarget);

            Assert.That(selectedDamage, Is.EqualTo(baselineDamage));
            Assert.That(
                selected.Selected.StatusEffects.Any(status =>
                    status.Definition.status_effect_id == "blessing"),
                Is.False);
        }

        [Test]
        public void LineHitUsesFullGeometryAndAppliesAttachedPayloadOnce()
        {
            RuntimeFixture fixture = CreateFixture("vega");
            Assert.That(
                fixture.Selected.SkillBucket.TryLearnActive(
                    catalog.GetSkill("vega-b")),
                Is.True);
            Assert.That(
                fixture.Selected.SkillBucket.TrySelectChoice(
                    catalog.GetChoice("vega-b-trait-5")),
                Is.True);
            EnemyModel onLine = CreateEnemy("stage1-swordsman");
            EnemyModel outsideWidth = CreateEnemy("stage1-archer");
            onLine.SetPosition(new CombatVector2(2f, 0f));
            outsideWidth.SetPosition(new CombatVector2(2f, 2f));
            fixture.Stage.TryRegisterFieldUnit(onLine);
            fixture.Stage.TryRegisterFieldUnit(outsideWidth);
            float outsideBefore = outsideWidth.CurrentHealth;

            Assert.That(
                fixture.Combat.TryExecuteSkill(new SkillExecutionRequest(
                    fixture.Selected,
                    catalog.GetSkill("vega-b"),
                    fixture.Stage.FieldUnits,
                    new CombatVector2(1f, 0f))),
                Is.True);
            fixture.Actors.Tick(0f);
            fixture.Actors.Tick(0f);

            Assert.That(onLine.CurrentHealth, Is.LessThan(onLine.MaximumHealth));
            Assert.That(outsideWidth.CurrentHealth, Is.EqualTo(outsideBefore));
            Assert.That(CountStatus(onLine, "name-mark"), Is.EqualTo(2));
        }

        [Test]
        public void TargetStackRateAndRepeatPerTargetExecuteFromEveD()
        {
            RuntimeFixture fixture = CreateFixture("eve");
            Assert.That(
                fixture.Selected.SkillBucket.TryLearnActive(
                    catalog.GetSkill("eve-d")),
                Is.True);
            SelectEnhancementsAndMaster(
                fixture,
                "eve-d",
                "eve-d-master-1");
            EnemyModel target = CreateEnemy("stage1-swordsman");
            target.ApplyStatus(
                catalog.GetStatus("shock"),
                fixture.Selected,
                null,
                2,
                "eve-a");
            fixture.Stage.TryRegisterFieldUnit(target);

            float before = target.CurrentHealth;
            Assert.That(
                fixture.Combat.TryExecuteSkill(new SkillExecutionRequest(
                    fixture.Selected,
                    catalog.GetSkill("eve-d"),
                    fixture.Stage.FieldUnits)),
                Is.True);
            float afterInitial = target.CurrentHealth;
            fixture.Actors.Tick(0f);
            fixture.Actors.Tick(0f);

            Assert.That(afterInitial, Is.LessThan(before));
            Assert.That(target.CurrentHealth, Is.LessThan(afterInitial));
        }

        [Test]
        public void AreaFieldReevaluatesRegisteredTargetsAtEveryTick()
        {
            RuntimeFixture fixture = CreateFixture("eve");
            Assert.That(
                fixture.Selected.SkillBucket.TryLearnActive(
                    catalog.GetSkill("eve-c")),
                Is.True);
            EnemyModel initial = CreateEnemy("stage1-swordsman");
            EnemyModel entering = CreateEnemy("stage1-archer");
            initial.SetPosition(new CombatVector2(1f, 0f));
            entering.SetPosition(new CombatVector2(20f, 0f));
            fixture.Stage.TryRegisterFieldUnit(initial);
            fixture.Stage.TryRegisterFieldUnit(entering);
            float enteringBefore = entering.CurrentHealth;

            Assert.That(
                fixture.Combat.TryExecuteSkill(new SkillExecutionRequest(
                    fixture.Selected,
                    catalog.GetSkill("eve-c"),
                    fixture.Stage.FieldUnits,
                    targetPoint: new CombatVector2(1f, 0f))),
                Is.True);
            fixture.Actors.Tick(0f);
            entering.SetPosition(new CombatVector2(1f, 0f));
            fixture.Actors.Tick(0f);

            Assert.That(initial.CurrentHealth, Is.LessThan(initial.MaximumHealth));
            Assert.That(entering.CurrentHealth, Is.LessThan(enteringBefore));
        }

        [Test]
        public void SourceShieldLayersExpireIndependentlyAndReflectAbsorbedDamage()
        {
            RuntimeFixture fixture = CreateFixture("ariel");
            Assert.That(
                fixture.Selected.SkillBucket.TryLearnActive(
                    catalog.GetSkill("ariel-b")),
                Is.True);
            SelectEnhancementsAndMaster(
                fixture,
                "ariel-b",
                "ariel-b-master-2");
            MonsterModel ally = CreateMonster("eve", false);
            EnemyModel attacker = CreateEnemy("stage1-swordsman");
            fixture.Stage.TryRegisterFieldUnit(ally);
            fixture.Stage.TryRegisterFieldUnit(attacker);

            fixture.Combat.AddShield(
                fixture.Selected,
                ally,
                catalog.GetSkill("ariel-b"),
                20f);
            fixture.Combat.AddShield(
                ally,
                ally,
                catalog.GetSkill("ShieldUp"),
                10f);
            Assert.That(
                ally.RemoveShield(ally, "ShieldUp"),
                Is.EqualTo(10f));
            Assert.That(ally.CurrentShield, Is.EqualTo(20f));

            float shieldBefore = ally.CurrentShield;
            float attackerBefore = attacker.CurrentHealth;
            SkillDefinition attack =
                catalog.GetSkill(attacker.EnemyDefinition.skill_slot_a_id);
            fixture.Combat.ApplySkillDamage(
                attacker,
                ally,
                attack,
                1f,
                eventExecuted: true);
            float absorbed = shieldBefore - ally.CurrentShield;
            fixture.Actors.Tick(0f);
            fixture.Actors.Tick(0f);

            float holyDefense =
                (float)attacker.EnemyDefinition.Columns["def_holy"];
            float expectedReflect = (float)Math.Round(
                absorbed * 0.35f * (100f / (100f + holyDefense)));
            Assert.That(
                attackerBefore - attacker.CurrentHealth,
                Is.EqualTo(expectedReflect));
        }

        [Test]
        public void NexusContactIsRemovedAfterExactlyOneDamageRequest()
        {
            RuntimeFixture fixture = CreateFixture("ariel");
            fixture.Stage.TryUnregisterFieldUnit(fixture.Selected);
            EnemyModel enemy = CreateEnemy("stage1-swordsman");
            NexusModel nexus = new NexusModel(100f);
            enemy.SetPosition(new CombatVector2(0.1f, 0f));
            fixture.Stage.TryRegisterFieldUnit(enemy);
            fixture.Stage.TryRegisterFieldUnit(nexus);
            fixture.Actions.RegisterEnemy(new EnemyActionController(
                enemy,
                fixture.Combat,
                fixture.Targeting,
                new UnitMovementController(),
                fixture.Stage,
                nexus,
                0.2f));

            fixture.Actions.Tick(0.1f);
            float afterContact = nexus.CurrentHealth;
            fixture.Actions.Tick(0.1f);

            Assert.That(
                afterContact,
                Is.EqualTo(100f - enemy.EnemyDefinition.nexus_damage.Value));
            Assert.That(nexus.CurrentHealth, Is.EqualTo(afterContact));
        }

        [Test]
        public void EndCombatUnsubscribesStatusExpiryAndClearsTriggerState()
        {
            RuntimeFixture fixture = CreateFixture("ariel");
            Assert.That(
                fixture.Selected.SkillBucket.TryLearnActive(
                    catalog.GetSkill("ariel-d")),
                Is.True);
            SelectEnhancementsAndMaster(
                fixture,
                "ariel-d",
                "ariel-d-master-2");
            EnemyModel target = CreateEnemy("stage1-swordsman");
            fixture.Stage.TryRegisterFieldUnit(target);
            fixture.Combat.TryExecuteSkill(new SkillExecutionRequest(
                fixture.Selected,
                catalog.GetSkill("ariel-d"),
                fixture.Stage.FieldUnits));
            fixture.Combat.ApplyStatus(
                fixture.Selected,
                target,
                catalog.GetStatus("name-mark"),
                0.01f,
                1,
                "ariel-d");
            fixture.Combat.ApplySkillDamage(
                fixture.Selected,
                target,
                catalog.GetSkill("ariel-a"),
                1f,
                eventExecuted: true);

            fixture.Actions.EndCombat();
            target.TickStatusEffects(0.01f);

            Assert.That(fixture.Actors.ActiveActors, Is.Empty);
            Assert.That(fixture.Actors.PendingAddCount, Is.Zero);
        }

        [Test]
        public void ThresholdStatusUsesNameMarkTenAndRunsAfterBaseStatus()
        {
            float belowThreshold = ExecuteVegaBAndReadSilenceDuration(9);
            float atThreshold = ExecuteVegaBAndReadSilenceDuration(10);

            Assert.That(belowThreshold, Is.EqualTo(3f).Within(0.0001f));
            Assert.That(atThreshold, Is.EqualTo(4f).Within(0.0001f));
        }

        [Test]
        public void BurstIndexZeroAndFollowUpProjectileRemainSeparateShots()
        {
            RuntimeFixture fixture = CreateFixture("vega");
            Assert.That(
                fixture.Selected.SkillBucket.TrySelectChoice(
                    catalog.GetChoice("vega-a-trait-2")),
                Is.True);
            Assert.That(
                fixture.Selected.SkillBucket.TrySelectChoice(
                    catalog.GetChoice("vega-a-trait-3")),
                Is.True);
            Assert.That(
                fixture.Selected.SkillBucket.TrySelectChoice(
                    catalog.GetChoice("vega-a-trait-4")),
                Is.True);
            Assert.That(
                fixture.Selected.SkillBucket.TrySelectChoice(
                    catalog.GetChoice("vega-a-master-1")),
                Is.True);
            EnemyModel target = CreateEnemy("stage1-swordsman");
            target.SetPosition(new CombatVector2(1f, 0f));
            fixture.Stage.TryRegisterFieldUnit(target);
            List<float> hits = new List<float>();
            fixture.Combat.DamageApplied += result =>
            {
                if (result.SkillId == "vega-a")
                {
                    hits.Add(-result.HealthChanged - result.ShieldChanged);
                }
            };

            Assert.That(
                fixture.Combat.TryExecuteSkill(new SkillExecutionRequest(
                    fixture.Selected,
                    catalog.GetSkill("vega-a"),
                    fixture.Stage.FieldUnits)),
                Is.True);
            for (int tick = 0; tick < 100; tick++)
            {
                fixture.Actors.Tick(0.1f);
            }

            hits.Sort();
            Assert.That(hits, Has.Count.EqualTo(4));
            Assert.That(hits[0], Is.LessThan(hits[1]));
            Assert.That(hits[1], Is.EqualTo(hits[2]));
            Assert.That(hits[3], Is.GreaterThan(hits[2]));
        }

        [Test]
        public void TriggerOwnedGraphUsesItsDeclaredOwnerAndEventTarget()
        {
            RuntimeFixture fixture = CreateFixture("vega");
            Assert.That(
                fixture.Selected.SkillBucket.TryLearnActive(
                    catalog.GetSkill("vega-b")),
                Is.True);
            Assert.That(
                fixture.Selected.SkillBucket.TryLearnPassive(
                    (PassiveDefinition)catalog.GetSkill("vega-g")),
                Is.True);
            EnemyModel target = CreateEnemy("stage1-swordsman");
            target.SetPosition(new CombatVector2(1f, 0f));
            fixture.Stage.TryRegisterFieldUnit(target);

            Assert.That(
                fixture.Combat.TryExecuteSkill(new SkillExecutionRequest(
                    fixture.Selected,
                    catalog.GetSkill("vega-b"),
                    fixture.Stage.FieldUnits,
                    new CombatVector2(1f, 0f))),
                Is.True);
            for (int tick = 0; tick < 5; tick++)
            {
                fixture.Actors.Tick(0f);
            }

            Assert.That(CountStatus(target, "name-mark"), Is.EqualTo(2));
            Assert.That(CountStatus(fixture.Selected, "name-mark"), Is.Zero);
        }

        [Test]
        public void CentralTickAppliesLearnedPassiveWithoutExternalCallback()
        {
            RuntimeFixture fixture = CreateFixture("ariel");
            Assert.That(
                fixture.Selected.SkillBucket.TryLearnPassive(
                    (PassiveDefinition)catalog.GetSkill("ariel-f")),
                Is.True);
            MonsterModel ally = CreateMonster("eve", false);
            fixture.Stage.TryRegisterFieldUnit(ally);

            fixture.Actions.Tick(0f);

            Assert.That(
                fixture.Selected.ResolveRuntimeModifier(
                    "StatusDamageBonusRate",
                    "Holy"),
                Is.EqualTo(0.12f).Within(0.0001f));
            Assert.That(
                ally.ResolveRuntimeModifier(
                    "StatusDamageBonusRate",
                    "Holy"),
                Is.EqualTo(0.12f).Within(0.0001f));
        }

        [Test]
        public void EqualDistanceOrderIsStableAndRejectedSelectionDoesNotMutate()
        {
            MonsterModel source = CreateMonster("ariel", false);
            EnemyModel first = CreateEnemy("stage1-swordsman");
            EnemyModel second = CreateEnemy("stage1-archer");
            first.SetPosition(new CombatVector2(1f, 0f));
            second.SetPosition(new CombatVector2(-1f, 0f));
            var ordered = new UnitBaseModel[] { source, first, second };

            Assert.That(
                new SkillTargeting(_ => 0).Resolve(
                    source,
                    catalog.GetSkill("ariel-a"),
                    ordered)[0],
                Is.SameAs(first));

            RuntimeFixture fixture = CreateFixture("ariel");
            MonsterModel another = CreateMonster("eve", false);
            MonsterActionController controller =
                new MonsterActionController(another, fixture.Combat);
            Assert.Throws<InvalidOperationException>(() =>
                fixture.Actions.RegisterMonster(controller, true));
            Assert.DoesNotThrow(() =>
                fixture.Actions.RegisterMonster(controller, false));
        }

        [Test]
        public void TriggeredSkillCarriesAncestryAndCannotRetriggerItsOrigin()
        {
            RuntimeFixture fixture = CreateFixture("sein", () => 0f);
            Assert.That(
                fixture.Selected.SkillBucket.TryLearnActive(
                    catalog.GetSkill("sein-b")),
                Is.True);
            Assert.That(
                fixture.Selected.SkillBucket.TryLearnPassive(
                    (PassiveDefinition)catalog.GetSkill("sein-g")),
                Is.True);
            EnemyModel target = CreateEnemy("stage2-arsen");
            target.SetPosition(new CombatVector2(1f, 0f));
            fixture.Stage.TryRegisterFieldUnit(target);

            Assert.That(
                fixture.Combat.TryExecuteSkill(new SkillExecutionRequest(
                    fixture.Selected,
                    catalog.GetSkill("sein-b"),
                    fixture.Stage.FieldUnits)),
                Is.True);
            for (int tick = 0; tick < 400; tick++)
            {
                fixture.Actors.Tick(0.1f);
            }

            Assert.That(fixture.Actors.ActiveActors, Is.Empty);
            Assert.That(fixture.Actors.PendingAddCount, Is.Zero);
            Assert.That(
                fixture.Combat.GetOutgoingHitCount(fixture.Selected),
                Is.LessThan(50));
        }

        private void AssertFamilyExecutes(string monsterId, string skillId)
        {
            RuntimeFixture fixture = CreateFixture(monsterId);
            SkillDefinition skill = catalog.GetSkill(skillId);
            if (!ReferenceEquals(fixture.Selected.SkillBucket.ActiveSkills[0], skill))
            {
                Assert.That(fixture.Selected.SkillBucket.TryLearnActive(skill), Is.True);
            }

            EnemyModel target = CreateEnemy("stage1-swordsman");
            target.SetPosition(new CombatVector2(1f, 0f));
            fixture.Stage.TryRegisterFieldUnit(target);
            Assert.That(
                fixture.Combat.TryExecuteSkill(
                    new SkillExecutionRequest(
                        fixture.Selected,
                        skill,
                        fixture.Stage.FieldUnits)),
                Is.True,
                skillId);
        }

        private void AssertEnemyFamilyExecutes(string enemyId, string skillId)
        {
            RuntimeFixture fixture = CreateFixture("ariel");
            EnemyModel caster = CreateEnemy(enemyId);
            fixture.Stage.TryRegisterFieldUnit(caster);
            if (skillId == "Heal")
            {
                caster.ApplyDamage(20f);
            }

            Assert.That(
                fixture.Combat.TryExecuteSkill(
                    new SkillExecutionRequest(
                        caster,
                        catalog.GetSkill(skillId),
                        fixture.Stage.FieldUnits)),
                Is.True,
                skillId);
        }

        private RuntimeFixture CreateFixture(
            string monsterId,
            Func<float> randomValue = null)
        {
            randomValue = randomValue ?? (() => 1f);
            MonsterModel monster = CreateMonster(monsterId, true);
            RunSessionModel session = new RunSessionModel(
                "stage1",
                1,
                "encounter",
                new PartyRoster(monster),
                new PrisonerInventory());
            StageManager stage = new StageManager(session, 0, 0);
            stage.TryRegisterFieldUnit(monster);
            EffectManager effects = new EffectManager();
            SkillActorManager actors = new SkillActorManager(effects);
            SkillTargeting targeting = new SkillTargeting(_ => 0);
            SkillExecutionRuntime execution =
                new SkillExecutionRuntime(
                    catalog,
                    targeting,
                    actors,
                    effects,
                    randomValue);
            InGameCombatManager combat =
                new InGameCombatManager(randomValue, execution);
            MonsterActionController monsterController =
                new MonsterActionController(monster, combat);
            PlayerInputController input = new PlayerInputController();
            RuntimeFixture fixture = new RuntimeFixture
            {
                Selected = monster,
                Stage = stage,
                Effects = effects,
                Actors = actors,
                Targeting = targeting,
                Combat = combat,
                Input = input
            };
            fixture.Actions = new InGameActionManager(
                stage,
                () => true,
                () => fixture.PassiveApplyCount++,
                input,
                actors,
                execution.Triggers,
                combat);
            fixture.Actions.RegisterMonster(monsterController, true);
            return fixture;
        }

        private MonsterModel CreateMonster(string monsterId, bool autoSkill)
        {
            MonsterDefinition definition = catalog.GetMonster(monsterId);
            return new MonsterModel(
                definition,
                catalog.GetSkill($"{monsterId}-a"),
                catalog.Choices.Values.Where(choice =>
                    choice.monster_id == monsterId
                    && choice.choice_group == "PassiveBase"),
                autoSkill);
        }

        private EnemyModel CreateEnemy(string enemyId)
        {
            EnemyDefinition definition = catalog.GetEnemy(enemyId);
            return new EnemyModel(
                definition,
                catalog.GetSkill(definition.skill_slot_a_id),
                catalog.GetSkill(definition.skill_slot_b_id),
                (PassiveDefinition)catalog.GetSkill(definition.passive_id));
        }

        private float ExecuteProjectileAndReadDamage(
            RuntimeFixture fixture,
            string skillId,
            EnemyModel target)
        {
            float before = target.CurrentHealth;
            Assert.That(
                fixture.Combat.TryExecuteSkill(new SkillExecutionRequest(
                    fixture.Selected,
                    catalog.GetSkill(skillId),
                    fixture.Stage.FieldUnits)),
                Is.True);
            for (int tick = 0; tick < 30; tick++)
            {
                fixture.Actors.Tick(0.1f);
            }
            return before - target.CurrentHealth;
        }

        private void SelectEnhancementsAndMaster(
            RuntimeFixture fixture,
            string skillId,
            string masterId)
        {
            for (int index = 1; index <= 3; index++)
            {
                Assert.That(
                    fixture.Selected.SkillBucket.TrySelectChoice(
                        catalog.GetChoice($"{skillId}-trait-{index}")),
                    Is.True);
            }
            Assert.That(
                fixture.Selected.SkillBucket.TrySelectChoice(
                    catalog.GetChoice(masterId)),
                Is.True);
        }

        private static int CountStatus(UnitBaseModel unit, string statusId)
        {
            return unit.StatusEffects
                .Where(status =>
                    status.Definition.status_effect_id == statusId)
                .Sum(status => status.CurrentStacks);
        }

        private float ExecuteVegaBAndReadSilenceDuration(int nameMarkStacks)
        {
            RuntimeFixture fixture = CreateFixture("vega");
            Assert.That(
                fixture.Selected.SkillBucket.TryLearnActive(
                    catalog.GetSkill("vega-b")),
                Is.True);
            foreach (int trait in new[] { 1, 3, 4 })
            {
                Assert.That(
                    fixture.Selected.SkillBucket.TrySelectChoice(
                        catalog.GetChoice($"vega-b-trait-{trait}")),
                    Is.True);
            }
            Assert.That(
                fixture.Selected.SkillBucket.TrySelectChoice(
                    catalog.GetChoice("vega-b-master-2")),
                Is.True);
            EnemyModel target = CreateEnemy("stage1-swordsman");
            target.SetPosition(new CombatVector2(1f, 0f));
            target.ApplyStatus(
                catalog.GetStatus("name-mark"),
                fixture.Selected,
                null,
                nameMarkStacks,
                "setup");
            Assert.That(
                CountStatus(target, "name-mark"),
                Is.EqualTo(nameMarkStacks));
            fixture.Stage.TryRegisterFieldUnit(target);

            Assert.That(
                fixture.Combat.TryExecuteSkill(new SkillExecutionRequest(
                    fixture.Selected,
                    catalog.GetSkill("vega-b"),
                    fixture.Stage.FieldUnits,
                    new CombatVector2(1f, 0f))),
                Is.True);
            fixture.Actors.Tick(0f);
            fixture.Actors.Tick(0f);
            Assert.That(
                CountStatus(target, "name-mark"),
                Is.EqualTo(nameMarkStacks));
            return target.StatusEffects.Single(status =>
                status.Definition.status_effect_id == "silence")
                .RemainingDuration.Value;
        }

        private static Dictionary<string, string> LoadSources()
        {
            string csvRoot = Path.Combine(Application.dataPath, "CSVdata");
            string[] files = Directory.GetFiles(csvRoot, "*.csv", SearchOption.AllDirectories);
            Dictionary<string, string> sources =
                new Dictionary<string, string>(StringComparer.Ordinal);
            UTF8Encoding strictUtf8 = new UTF8Encoding(false, true);
            foreach (string file in files)
            {
                string relative = file.Substring(Application.dataPath.Length)
                    .TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                    .Replace('\\', '/');
                sources.Add($"Assets/{relative}", File.ReadAllText(file, strictUtf8));
            }

            return sources;
        }

        private sealed class RuntimeFixture
        {
            public MonsterModel Selected;
            public StageManager Stage;
            public EffectManager Effects;
            public SkillActorManager Actors;
            public SkillTargeting Targeting;
            public InGameCombatManager Combat;
            public PlayerInputController Input;
            public InGameActionManager Actions;
            public int PassiveApplyCount;
        }
    }
}
