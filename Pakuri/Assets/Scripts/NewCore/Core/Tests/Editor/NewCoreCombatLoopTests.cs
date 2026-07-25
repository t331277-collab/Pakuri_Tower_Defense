using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using NUnit.Framework;
using Pakuri.NewCore.Bootstrap;
using Pakuri.NewCore.Catalog;
using Pakuri.NewCore.Combat;
using Pakuri.NewCore.Combat.Actions;
using Pakuri.NewCore.Combat.Effects;
using Pakuri.NewCore.Combat.Skills.Actors;
using Pakuri.NewCore.Combat.Skills.Execution;
using Pakuri.NewCore.Combat.Skills.Runtime;
using Pakuri.NewCore.Definitions.Choices;
using Pakuri.NewCore.Definitions.Skills;
using Pakuri.NewCore.Definitions.Units;
using Pakuri.NewCore.Parsing;
using Pakuri.NewCore.Run;
using Pakuri.NewCore.Spawn;
using Pakuri.NewCore.Units.Models;
using UnityEngine;

/* NewCore 중앙 전투 tick과 스킬 계열별 실행 계약을 검증한다. */
namespace Pakuri.NewCore.Tests
{
    internal static class NewCoreTestFactory
    {
        /* CreateComponent 테스트 대상을 필요한 의존성과 함께 구성한다. */
        public static T CreateComponent<T>()
            where T : Component
        {
            return new GameObject(typeof(T).Name).AddComponent<T>();
        }

        /* CreateStageManager 테스트 대상을 필요한 의존성과 함께 구성한다. */
        public static StageManager CreateStageManager(
            RunSessionModel session,
            int initialGold,
            int initialDarkTrace)
        {
            StageManager manager = CreateComponent<StageManager>();
            manager.Initialize(
                session,
                initialGold,
                initialDarkTrace);
            return manager;
        }

        /* CreateStageManager 테스트 대상을 필요한 의존성과 함께 구성한다. */
        public static StageManager CreateStageManager(
            RunSessionModel session,
            GameDefinitionCatalog catalog,
            int initialGold,
            int initialDarkTrace)
        {
            StageManager manager = CreateComponent<StageManager>();
            manager.Initialize(
                session,
                catalog,
                initialGold,
                initialDarkTrace);
            return manager;
        }

        /* CreateSpawnManager 테스트 대상을 필요한 의존성과 함께 구성한다. */
        public static SpawnManager CreateSpawnManager(
            GameDefinitionCatalog catalog,
            Func<int, int> randomIndex,
            Func<float> randomValue)
        {
            SpawnManager manager = CreateComponent<SpawnManager>();
            manager.Initialize(catalog, randomIndex, randomValue);
            return manager;
        }
    }

    public class NewCoreCombatLoopTests
    {
        private GameDefinitionCatalog catalog;

        [SetUp]
        /* 각 테스트가 독립적으로 실행되도록 임시 객체와 공유 상태를 초기화한다. */
        public void SetUp()
        {
            catalog = GameBootstrap.CreateCatalog(LoadSources());
        }

        [Test]
        /* CentralTickUsesTheBlueprintOrderExactlyOnce 시나리오의 기대 동작과 상태 변화를 검증한다. */
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
        /* ActorRegistrationStartsNextTickAndRemovalWaitsForIterationEnd 시나리오의 기대 동작과 상태 변화를 검증한다. */
        public void ActorRegistrationStartsNextTickAndRemovalWaitsForIterationEnd()
        {
            EffectManager effects =
                NewCoreTestFactory.CreateComponent<EffectManager>();
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
        /* CombatResultsApplyDamageShieldHealAndStatusThroughModelAuthority 시나리오의 기대 동작과 상태 변화를 검증한다. */
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
            fixture.Combat.AddShield(source, target, catalog.GetSkill("GuardianFlag"), 25f);
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
        /* TargetingUsesLivingSideSelectionHealthStacksAndManualPoint 시나리오의 기대 동작과 상태 변화를 검증한다. */
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
            UnitBaseModel highestHealth = far;
            if (near.CurrentHealth >= far.CurrentHealth)
            {
                highestHealth = near;
            }
            Assert.That(
                targeting.Resolve(source, catalog.GetSkill("ariel-d"), units)[0],
                Is.SameAs(highestHealth),
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
        /* EveryActiveSkillFamilyHasADeterministicExecutionPath 시나리오의 기대 동작과 상태 변화를 검증한다. */
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
        /* SelectedChoiceNodesChangeTheSkillPlanWithoutChangingDefinitions 테스트의 선행 런타임 상태를 구성한다. */
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
        /* ManualInputBoundaryRejectsUiAndAutoThenPreservesProjectileAim 시나리오의 기대 동작과 상태 변화를 검증한다. */
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
        /* MovementHonorsDeltaTimeStopDistanceAndMovementStatus 시나리오의 기대 동작과 상태 변화를 검증한다. */
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
        /* EnemyUsesNexusWhenNoLivingPlayerAndRequestsExactNexusDamage 시나리오의 기대 동작과 상태 변화를 검증한다. */
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
        /* CooldownBlocksRepeatedExecutionUntilCentralTickCompletesIt 시나리오의 기대 동작과 상태 변화를 검증한다. */
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
        /* EveryReachableNodeAndTriggerContractHasAnExecutableRuntimePath 시나리오의 기대 동작과 상태 변화를 검증한다. */
        public void EveryReachableNodeAndTriggerContractHasAnExecutableRuntimePath()
        {
            Assert.That(
                catalog.Skills.Values.Count(skill =>
                    !string.IsNullOrEmpty(skill.monster_id)),
                Is.EqualTo(50));
            Assert.That(catalog.Choices, Has.Count.EqualTo(252));
            Assert.That(catalog.ChoiceNodes, Has.Count.EqualTo(772));
            Assert.That(
                catalog.Triggers.Values.Count(trigger =>
                    !string.IsNullOrEmpty(trigger.monster_id)),
                Is.EqualTo(57));
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
            for (var index = 0;
                index < catalog.ChoiceNodes.Count;
                index++)
            {
                Assert.DoesNotThrow(
                    () => SkillNodeSupport.ResolveRuntimeOwner(
                        catalog.ChoiceNodes[index]),
                    catalog.ChoiceNodes[index].owner_id
                        + "/"
                        + catalog.ChoiceNodes[index].node_type_id);
            }

            Assert.That(catalog.Triggers, Has.Count.EqualTo(59));
            foreach (SkillTriggerDefinition trigger in catalog.Triggers.Values)
            {
                Assert.DoesNotThrow(() => SkillTriggerSupport.Validate(trigger));
            }

            Assert.DoesNotThrow(() => CreateFixture("ariel"));
        }

        [Test]
        /* EveryMonsterTriggerAcceptsItsMatchingEventAndRejectsANonOwner 시나리오의 기대 동작과 상태 변화를 검증한다. */
        public void EveryMonsterTriggerAcceptsItsMatchingEventAndRejectsANonOwner()
        {
            SkillTriggerDefinition[] triggers = catalog.Triggers.Values
                .Where(trigger =>
                    !string.IsNullOrEmpty(trigger.monster_id))
                .OrderBy(trigger => trigger.trigger_id)
                .ToArray();
            Assert.That(triggers, Has.Length.EqualTo(57));

            for (var triggerIndex = 0;
                triggerIndex < triggers.Length;
                triggerIndex++)
            {
                SkillTriggerDefinition trigger =
                    triggers[triggerIndex];
                RuntimeFixture fixture =
                    CreateFixture(trigger.monster_id, () => 0f);
                EnsureTriggerOwnership(
                    fixture.Selected,
                    trigger);
                EnemyModel eventTarget =
                    CreateEnemy("stage1-guardian-captain");
                eventTarget.SetPosition(new CombatVector2(1f, 0f));
                fixture.Stage.TryRegisterFieldUnit(eventTarget);

                SkillDefinition eventSkill =
                    ResolveMatchingEventSkill(trigger);
                string conditionStatus = ReadTriggerString(
                    trigger,
                    "condition_status_id");
                string conditionSource = ReadTriggerString(
                    trigger,
                    "condition_status_source_skill_id");
                if (!string.IsNullOrEmpty(conditionStatus))
                {
                    eventTarget.ApplyStatus(
                        catalog.GetStatus(conditionStatus),
                        fixture.Selected,
                        null,
                        5,
                        conditionSource);
                }

                var evaluations =
                    new List<(
                        string Id,
                        UnitBaseModel Owner,
                        SkillTriggerEvaluationResult Result)>();
                fixture.Triggers.TriggerEvaluated +=
                    (evaluated, owner, result) =>
                        evaluations.Add((
                            evaluated.trigger_id,
                            owner,
                            result));
                int every = Math.Max(
                    16,
                    ReadTriggerInt(
                        trigger,
                        "trigger_every_count"));
                for (var count = 0; count < every; count++)
                {
                    fixture.Triggers.Dispatch(
                        trigger.trigger_event,
                        fixture.Selected,
                        eventSkill,
                        eventTarget,
                        fixture.Stage.FieldUnits,
                        fixture.Combat,
                        conditionStatus,
                        100f,
                        100f,
                        100f,
                        100f,
                        ReadTriggerString(
                            trigger,
                            "tracked_attribute"),
                        true,
                        conditionSource);
                }
                Assert.That(
                    evaluations.Any(result =>
                        result.Id == trigger.trigger_id
                        && ReferenceEquals(
                            result.Owner,
                            fixture.Selected)
                        && result.Result
                            == SkillTriggerEvaluationResult.Matched),
                    Is.True,
                    trigger.trigger_id
                        + ": "
                        + string.Join(
                            ",",
                            evaluations
                                .Where(result =>
                                    result.Id
                                        == trigger.trigger_id)
                                .Select(result =>
                                    result.Result.ToString())));

                string nonOwnerId = "ariel";
                if (trigger.monster_id == "ariel")
                {
                    nonOwnerId = "eve";
                }
                MonsterModel nonOwner = CreateMonster(nonOwnerId, false);
                evaluations.Clear();
                fixture.Triggers.Dispatch(
                    trigger.trigger_event,
                    nonOwner,
                    eventSkill,
                    eventTarget,
                    new UnitBaseModel[]
                    {
                        nonOwner,
                        eventTarget
                    },
                    fixture.Combat,
                    conditionStatus,
                    100f,
                    100f,
                    100f,
                    100f,
                    ReadTriggerString(
                        trigger,
                        "tracked_attribute"),
                    true,
                    conditionSource);
                Assert.That(
                    evaluations.Any(result =>
                        result.Id == trigger.trigger_id
                        && ReferenceEquals(
                            result.Owner,
                            nonOwner)
                        && result.Result
                            == SkillTriggerEvaluationResult
                                .MissingOwnership),
                    Is.True,
                    trigger.trigger_id);
            }
        }

        [Test]
        /* EveryMonsterBaseDefinitionCreatesItsRuntimeContract 시나리오의 기대 동작과 상태 변화를 검증한다. */
        public void EveryMonsterBaseDefinitionCreatesItsRuntimeContract()
        {
            SkillDefinition[] definitions = catalog.Skills.Values
                .Where(skill =>
                    !string.IsNullOrEmpty(skill.monster_id))
                .OrderBy(skill => skill.skill_id)
                .ToArray();
            Assert.That(definitions, Has.Length.EqualTo(50));
            for (var index = 0; index < definitions.Length; index++)
            {
                SkillDefinition skill = definitions[index];
                RuntimeFixture fixture =
                    CreateFixture(skill.monster_id);
                if (skill is PassiveDefinition passive)
                {
                    char activeSlot = (char)(
                        'A' + char.ToUpperInvariant(
                            passive.slot[0]) - 'F');
                    if (activeSlot != 'A')
                    {
                        Assert.That(
                            fixture.Selected.SkillBucket.TryLearnActive(
                                catalog.GetSkill(
                                    skill.monster_id
                                    + "-"
                                    + char.ToLowerInvariant(
                                        activeSlot))),
                            Is.True,
                            skill.skill_id);
                    }
                    Assert.That(
                        fixture.Selected.SkillBucket.TryLearnPassive(
                            passive),
                        Is.True,
                        skill.skill_id);
                    Assert.DoesNotThrow(
                        () => fixture.Combat.ApplyPassiveChanges(
                            fixture.Stage.FieldUnits),
                        skill.skill_id);
                    fixture.Actors.Tick(0f);
                    fixture.Actors.Tick(0f);
                    Assert.That(
                        fixture.Selected.SkillBucket.PassiveSkills,
                        Does.Contain(passive),
                        skill.skill_id);
                    continue;
                }

                if (skill.slot != "A")
                {
                    Assert.That(
                        fixture.Selected.SkillBucket.TryLearnActive(
                            skill),
                        Is.True,
                        skill.skill_id);
                }
                EnemyModel target =
                    CreateEnemy("stage1-guardian-captain");
                target.SetPosition(new CombatVector2(1f, 0f));
                target.ApplyDamage(
                    target.MaximumHealth * 0.9f);
                target.ApplyStatus(
                    catalog.GetStatus("name-mark"),
                    fixture.Selected,
                    null,
                    5);
                target.ApplyStatus(
                    catalog.GetStatus("shock"),
                    fixture.Selected,
                    null,
                    5);
                fixture.Stage.TryRegisterFieldUnit(target);
                Assert.That(
                    fixture.Combat.TryExecuteSkill(
                        new SkillExecutionRequest(
                            fixture.Selected,
                            skill,
                            fixture.Stage.FieldUnits,
                            new CombatVector2(1f, 0f),
                            target.Position)),
                    Is.True,
                    skill.skill_id);
                Assert.That(
                    fixture.Selected.SkillBucket
                        .GetCooldown(skill.skill_id)
                        .CanUse(),
                    Is.False,
                    skill.skill_id);
            }
        }

        [Test]
        /* EveryMonsterChoiceSelectsThroughItsOwnedRuntimeContract 시나리오의 기대 동작과 상태 변화를 검증한다. */
        public void EveryMonsterChoiceSelectsThroughItsOwnedRuntimeContract()
        {
            SkillChoiceDefinition[] choices = catalog.Choices.Values
                .OrderBy(choice => choice.choice_id)
                .ToArray();
            Assert.That(choices, Has.Length.EqualTo(252));
            for (var index = 0; index < choices.Length; index++)
            {
                SkillChoiceDefinition choice = choices[index];
                MonsterModel owner = PrepareChoiceOwner(choice);
                int activeCount = owner.SkillBucket.ActiveSkills.Count;
                int passiveCount =
                    owner.SkillBucket.PassiveSkills.Count;
                int selectedCount =
                    owner.SkillBucket.SelectedChoices.Count;
                string definitionBefore = string.Join(
                    "|",
                    choice.Columns
                        .OrderBy(pair => pair.Key)
                        .Select(pair =>
                            pair.Key + "=" + pair.Value));

                Assert.That(
                    owner.SkillBucket.TrySelectChoice(choice),
                    Is.True,
                    choice.choice_id);
                Assert.That(
                    owner.SkillBucket.SelectedChoices.Count,
                    Is.EqualTo(selectedCount + 1),
                    choice.choice_id);
                Assert.That(
                    owner.SkillBucket.ActiveSkills.Count,
                    Is.EqualTo(activeCount),
                    choice.choice_id);
                Assert.That(
                    owner.SkillBucket.PassiveSkills.Count,
                    Is.EqualTo(passiveCount),
                    choice.choice_id);
                Assert.That(
                    string.Join(
                        "|",
                        choice.Columns
                            .OrderBy(pair => pair.Key)
                            .Select(pair =>
                                pair.Key + "=" + pair.Value)),
                    Is.EqualTo(definitionBefore),
                    choice.choice_id);

                ChoiceNodeDefinition[] nodes = catalog.ChoiceNodes
                    .Where(node =>
                        node.owner_id == choice.choice_id)
                    .ToArray();
                for (var nodeIndex = 0;
                    nodeIndex < nodes.Length;
                    nodeIndex++)
                {
                    Assert.DoesNotThrow(
                        () => SkillNodeSupport.Resolve(
                            catalog.NodeTypes[
                                nodes[nodeIndex].node_type_id]
                                .handler_id),
                        choice.choice_id);
                }

                SkillTriggerDefinition[] triggers =
                    catalog.Triggers.Values
                        .Where(trigger =>
                            trigger.Columns.Values
                                .OfType<string>()
                                .SelectMany(value =>
                                    value.Split(';'))
                                .Any(value =>
                                    value == choice.choice_id))
                        .ToArray();
                for (var triggerIndex = 0;
                    triggerIndex < triggers.Length;
                    triggerIndex++)
                {
                    Assert.DoesNotThrow(
                        () => SkillTriggerSupport.Validate(
                            triggers[triggerIndex]),
                        choice.choice_id);
                }
                Assert.That(
                    nodes.Length + triggers.Length,
                    Is.GreaterThan(0),
                    choice.choice_id);
            }
        }

        [Test]
        /* EveryPlanGraphRowRunsInsideASuccessfulSkillContract 시나리오의 기대 동작과 상태 변화를 검증한다. */
        public void EveryPlanGraphRowRunsInsideASuccessfulSkillContract()
        {
            ChoiceNodeDefinition[][] graphs = catalog.ChoiceNodes
                .Where(node => node.graph_kind == "Plan")
                .GroupBy(node =>
                    node.owner_kind
                    + "|"
                    + node.owner_id
                    + "|"
                    + (node.graph_index ?? 0)
                    + "|"
                    + node.target_skill_id)
                .Select(group => group.ToArray())
                .ToArray();
            Assert.That(
                graphs.Sum(graph => graph.Length),
                Is.EqualTo(256));

            for (var graphIndex = 0;
                graphIndex < graphs.Length;
                graphIndex++)
            {
                ChoiceNodeDefinition[] graph = graphs[graphIndex];
                ChoiceNodeDefinition first = graph[0];
                RuntimeFixture fixture =
                    CreateFixture(first.monster_id, () => 0f);
                SkillDefinition skill =
                    PrepareGraphOwnerAndResolveSkill(
                        fixture.Selected,
                        first);
                bool requiresKill = graph.Any(node =>
                    node.node_type_id == "CooldownRefund"
                    || node.node_type_id == "CooldownRefundBonus"
                    || node.node_type_id == "CooldownReset");
                string targetId = "stage1-guardian-captain";
                if (requiresKill)
                {
                    targetId = "stage1-swordsman";
                }
                EnemyModel target = CreateEnemy(targetId);
                target.SetPosition(new CombatVector2(1f, 0f));
                target.ApplyDamage(
                    target.MaximumHealth * 0.9f);
                ApplyContractStatus(
                    target,
                    fixture.Selected,
                    "name-mark",
                    10);
                ApplyContractStatus(
                    target,
                    fixture.Selected,
                    "shock",
                    10);
                fixture.Stage.TryRegisterFieldUnit(target);
                PrepareGraphConditions(
                    fixture,
                    graph,
                    target,
                    null);

                var consumed =
                    new HashSet<string>(StringComparer.Ordinal);
                fixture.Execution.NodeContractExecuted +=
                    node => consumed.Add(NodeContractKey(node));
                string stateBefore = ReadRuntimeContractState(
                    fixture,
                    target);
                var executionRequest = new SkillExecutionRequest(
                    fixture.Selected,
                    skill,
                    fixture.Stage.FieldUnits,
                    new CombatVector2(1f, 0f),
                    target.Position,
                    false);
                bool executed = fixture.Execution.TryExecute(
                    fixture.Combat,
                    executionRequest);
                Assert.That(
                    executed,
                    Is.True,
                    first.owner_id + " -> " + skill.skill_id);
                fixture.Actors.Tick(0f);
                for (var tick = 0; tick < 400; tick++)
                {
                    fixture.Actors.Tick(0.1f);
                }
                for (var nodeIndex = 0;
                    nodeIndex < graph.Length;
                    nodeIndex++)
                {
                    if (graph[nodeIndex].node_type_id
                        != "TriggerProcChanceBonus")
                    {
                        continue;
                    }
                    SkillTriggerDefinition procTrigger =
                        catalog.Triggers[graph[nodeIndex].arg_1];
                    fixture.Triggers.Dispatch(
                        procTrigger.trigger_event,
                        fixture.Selected,
                        catalog.GetSkill("eve-a"),
                        target,
                        fixture.Stage.FieldUnits,
                        fixture.Combat,
                        trackedAttribute:
                            ReadTriggerString(
                                procTrigger,
                                "tracked_attribute"));
                }
                Assert.That(
                    ReadRuntimeContractState(fixture, target),
                    Is.Not.EqualTo(stateBefore),
                    first.owner_id
                        + " produced no observable runtime state delta.");
                for (var nodeIndex = 0;
                    nodeIndex < graph.Length;
                    nodeIndex++)
                {
                    Assert.That(
                        consumed.Contains(
                            NodeContractKey(graph[nodeIndex])),
                        Is.True,
                        first.owner_id
                            + "/"
                            + graph[nodeIndex].node_type_id
                            + "/"
                            + graph[nodeIndex].node_order
                            + " consumed="
                            + string.Join(",", consumed)
                            + " target_alive="
                            + target.IsAlive
                            + " target_health="
                            + target.CurrentHealth
                            + " target_shield="
                            + target.CurrentShield
                            + " applied_count="
                            + executionRequest.AppliedTargets.Count
                            + " applied_target="
                            + executionRequest.AppliedTargets.Contains(target));
                }
            }
        }

        [Test]
        /* EveryEffectGraphRowEntersItsActualRuntimeHandler 시나리오의 기대 동작과 상태 변화를 검증한다. */
        public void EveryEffectGraphRowEntersItsActualRuntimeHandler()
        {
            ChoiceNodeDefinition[][] graphs = catalog.ChoiceNodes
                .Where(node => node.graph_kind == "Effect")
                .GroupBy(node =>
                    node.owner_kind
                    + "|"
                    + node.owner_id
                    + "|"
                    + (node.graph_index ?? 0))
                .Select(group => group.ToArray())
                .ToArray();
            Assert.That(
                graphs.Sum(graph => graph.Length),
                Is.EqualTo(516));

            for (var graphIndex = 0;
                graphIndex < graphs.Length;
                graphIndex++)
            {
                ChoiceNodeDefinition[] graph = graphs[graphIndex];
                ChoiceNodeDefinition first = graph[0];
                RuntimeFixture fixture =
                    CreateFixture(first.monster_id, () => 0f);
                SkillDefinition skill =
                    PrepareGraphOwnerAndResolveSkill(
                        fixture.Selected,
                        first);
                MonsterModel ally =
                    CreateMonster(first.monster_id, false);
                EnemyModel target =
                    CreateEnemy("stage1-guardian-captain");
                ally.SetPosition(new CombatVector2(0.5f, 0f));
                target.SetPosition(new CombatVector2(1f, 0f));
                target.ApplyDamage(
                    target.MaximumHealth * 0.9f);
                fixture.Stage.TryRegisterFieldUnit(ally);
                fixture.Stage.TryRegisterFieldUnit(target);
                PrepareGraphConditions(
                    fixture,
                    graph,
                    target,
                    ally);

                var consumed =
                    new HashSet<string>(StringComparer.Ordinal);
                var runtime = new SkillEffectGraphRuntime(
                    catalog,
                    fixture.Actors,
                    fixture.Effects,
                    () => 0f,
                    node => consumed.Add(NodeContractKey(node)));
                var request = new SkillExecutionRequest(
                    fixture.Selected,
                    skill,
                    fixture.Stage.FieldUnits,
                    new CombatVector2(1f, 0f),
                    target.Position,
                    true);
                request.SetEventTarget(target);
                MethodInfo recordAppliedTarget = typeof(
                        SkillExecutionRequest)
                    .GetMethod(
                        "RecordAppliedTarget",
                        BindingFlags.Instance
                            | BindingFlags.NonPublic);
                Assert.That(recordAppliedTarget, Is.Not.Null);
                recordAppliedTarget.Invoke(
                    request,
                    new object[] { target });
                recordAppliedTarget.Invoke(
                    request,
                    new object[] { ally });
                runtime.ExecuteTriggerGraph(
                    fixture.Combat,
                    request,
                    first.owner_id,
                    first.owner_kind,
                    "Effect",
                    first.graph_index);

                for (var nodeIndex = 0;
                    nodeIndex < graph.Length;
                    nodeIndex++)
                {
                    Assert.That(
                        consumed.Contains(
                            NodeContractKey(graph[nodeIndex])),
                        Is.True,
                        first.owner_id
                            + "/"
                            + graph[nodeIndex].node_type_id
                            + "/"
                            + graph[nodeIndex].node_order
                            + "/graph="
                            + graph[nodeIndex].graph_index);
                }
            }
        }

        [Test]
        /* ScheduledActorHonorsInitialDelayAndExactRepeatCount 시나리오의 기대 동작과 상태 변화를 검증한다. */
        public void ScheduledActorHonorsInitialDelayAndExactRepeatCount()
        {
            EffectManager effects =
                NewCoreTestFactory.CreateComponent<EffectManager>();
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
        /* SkillOwnedEffectGraphAppliesStatusAndRuntimeModifier 시나리오의 기대 동작과 상태 변화를 검증한다. */
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
                Is.EqualTo(0.18f).Within(0.0001f));
        }

        [Test]
        /* ProjectileBurstPiercesOrderedTargetsThroughScheduledActors 시나리오의 기대 동작과 상태 변화를 검증한다. */
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
        /* CombatStartTriggerDispatchesOwnedEnemyBuff 시나리오의 기대 동작과 상태 변화를 검증한다. */
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
        /* ActionManagerStartsEachUnitOnceAndResetsTheBoundary 시나리오의 기대 동작과 상태 변화를 검증한다. */
        public void ActionManagerStartsEachUnitOnceAndResetsTheBoundary()
        {
            RuntimeFixture fixture = CreateFixture("ariel");
            EnemyModel first = CreateEnemy("stage2-drake");
            fixture.Stage.TryRegisterFieldUnit(first);

            fixture.Actions.BeginOrExtendCombat(
                fixture.Stage.FieldUnits);
            Assert.That(fixture.Actors.PendingAddCount, Is.EqualTo(1));

            fixture.Actions.BeginOrExtendCombat(
                fixture.Stage.FieldUnits);
            Assert.That(fixture.Actors.PendingAddCount, Is.EqualTo(1));

            EnemyModel laterSpawn = CreateEnemy("stage2-drake");
            fixture.Stage.TryRegisterFieldUnit(laterSpawn);
            fixture.Actions.BeginOrExtendCombat(
                fixture.Stage.FieldUnits);
            Assert.That(fixture.Actors.PendingAddCount, Is.EqualTo(2));

            fixture.Actions.EndCombat();
            Assert.That(fixture.Actors.PendingAddCount, Is.Zero);
            Assert.That(fixture.Effects.ActiveEffects, Is.Empty);

            fixture.Actions.BeginOrExtendCombat(
                fixture.Stage.FieldUnits);
            Assert.That(fixture.Actors.PendingAddCount, Is.EqualTo(2));
        }

        [Test]
        /* ManualInputQueueAndProjectileAimEndWithCombat 시나리오의 기대 동작과 상태 변화를 검증한다. */
        public void ManualInputQueueAndProjectileAimEndWithCombat()
        {
            RuntimeFixture fixture = CreateFixture("ariel");
            fixture.Input.SetAutoSkillEnabled(false);
            var skill = catalog.GetSkill("ariel-a");
            Assert.That(
                fixture.Input.SubmitManualSkillRequest(
                    skill,
                    new CombatVector2(1f, 0f),
                    new CombatVector2(2f, 0f),
                    ManualInputPhase.Pressed,
                    false),
                Is.True);

            fixture.Actions.EndCombat();

            Assert.That(
                fixture.Input.Process(fixture.Stage.FieldUnits),
                Is.False);
            Assert.That(
                fixture.Input.ContinueProjectileBurst(skill),
                Is.False);
        }

        [Test]
        /* CombatResultExposesPositiveHealthShieldAndLethalDamage 시나리오의 기대 동작과 상태 변화를 검증한다. */
        public void CombatResultExposesPositiveHealthShieldAndLethalDamage()
        {
            RuntimeFixture fixture = CreateFixture("ariel");
            var skill = catalog.GetSkill("ariel-a");

            EnemyModel healthTarget =
                CreateEnemy("stage1-swordsman");
            CombatResult health = fixture.Combat.ApplySkillDamage(
                fixture.Selected,
                healthTarget,
                skill,
                0.1f);
            Assert.That(health.HealthChanged, Is.LessThan(0f));
            Assert.That(health.DamageAmount, Is.GreaterThan(0f));
            Assert.That(
                health.DamageAmount,
                Is.EqualTo(-health.HealthChanged)
                    .Within(0.0001f));

            EnemyModel shieldTarget =
                CreateEnemy("stage1-swordsman");
            shieldTarget.TryAddShield(10000f);
            CombatResult shield = fixture.Combat.ApplySkillDamage(
                fixture.Selected,
                shieldTarget,
                skill,
                0.1f);
            Assert.That(shield.HealthChanged, Is.Zero);
            Assert.That(shield.ShieldChanged, Is.LessThan(0f));
            Assert.That(
                shield.DamageAmount,
                Is.EqualTo(-shield.ShieldChanged)
                    .Within(0.0001f));

            EnemyModel lethalTarget =
                CreateEnemy("stage1-swordsman");
            CombatResult lethal = fixture.Combat.ApplySkillDamage(
                fixture.Selected,
                lethalTarget,
                skill,
                100000f);
            Assert.That(lethal.IsDefeated, Is.True);
            Assert.That(
                lethal.DamageAmount,
                Is.EqualTo(-lethal.HealthChanged)
                    .Within(0.0001f));
        }

        [Test]
        /* ReachableSkillsCarryCompleteVisualSpecifications 시나리오의 기대 동작과 상태 변화를 검증한다. */
        public void ReachableSkillsCarryCompleteVisualSpecifications()
        {
            RuntimeFixture projectile = CreateFixture("ariel");
            EnemyModel projectileTarget =
                CreateEnemy("stage1-swordsman");
            projectileTarget.SetPosition(
                new CombatVector2(1f, 0f));
            projectile.Stage.TryRegisterFieldUnit(projectileTarget);
            Assert.That(
                projectile.Combat.TryExecuteSkill(
                    new SkillExecutionRequest(
                        projectile.Selected,
                        catalog.GetSkill("ariel-a"),
                        projectile.Stage.FieldUnits)),
                Is.True);
            projectile.Actors.Tick(0f);
            projectile.Actors.Tick(0f);
            EffectHandle projectileEffect =
                projectile.Effects.ActiveEffects.Single();
            Assert.That(
                projectileEffect.Visual.SpritePath,
                Is.EqualTo(
                    ReadStringColumn(
                        catalog.GetSkill("ariel-a"),
                        "runtime_visual_sprite_path")));
            Assert.That(
                projectileEffect.Visual.AnimatorControllerPath,
                Is.EqualTo(
                    ReadStringColumn(
                        catalog.GetSkill("ariel-a"),
                        "runtime_visual_animator_controller_path")));
            Assert.That(
                projectileEffect.Visual.Scale,
                Is.EqualTo(
                    ReadFloatColumn(
                        catalog.GetSkill("ariel-a"),
                        "runtime_visual_scale"))
                    .Within(0.0001f));

            RuntimeFixture prefab = CreateFixture("rin");
            Assert.That(
                prefab.Selected.SkillBucket.TryLearnActive(
                    catalog.GetSkill("rin-e")),
                Is.True);
            EnemyModel prefabTarget =
                CreateEnemy("stage1-swordsman");
            prefabTarget.SetPosition(new CombatVector2(1f, 0f));
            prefab.Stage.TryRegisterFieldUnit(prefabTarget);
            Assert.That(
                prefab.Combat.TryExecuteSkill(
                    new SkillExecutionRequest(
                        prefab.Selected,
                        catalog.GetSkill("rin-e"),
                        prefab.Stage.FieldUnits,
                        targetPoint: prefabTarget.Position)),
                Is.True);
            Assert.That(
                prefab.Effects.ActiveEffects.Single()
                    .Visual.PrefabPath,
                Is.EqualTo("Assets/Prefab/Skill/Rin/Rin_E.prefab"));

            RuntimeFixture impact = CreateFixture("sein");
            Assert.That(
                impact.Selected.SkillBucket.TryLearnActive(
                    catalog.GetSkill("sein-c")),
                Is.True);
            EnemyModel impactTarget =
                CreateEnemy("stage1-swordsman");
            impactTarget.SetPosition(new CombatVector2(1f, 0f));
            impact.Stage.TryRegisterFieldUnit(impactTarget);
            Assert.That(
                impact.Combat.TryExecuteSkill(
                    new SkillExecutionRequest(
                        impact.Selected,
                        catalog.GetSkill("sein-c"),
                        impact.Stage.FieldUnits)),
                Is.True);
            EffectHandle impactEffect = null;
            for (int tick = 0;
                tick < 30 && impactEffect == null;
                tick++)
            {
                impact.Actors.Tick(0.1f);
                impactEffect = impact.Effects.ActiveEffects
                    .FirstOrDefault(effect =>
                        effect.Visual.SpritePath
                        == "Assets/Image/Monster/Sein/SkillEffect/Sprite/B-1.png");
            }

            Assert.That(impactEffect, Is.Not.Null);
            Assert.That(
                impactEffect.Visual.AnimatorControllerPath,
                Is.EqualTo(
                    "Assets/Image/Monster/Sein/SkillEffect/Sprite/B-1.controller"));

            RuntimeFixture trigger = CreateFixture("sein");
            SelectEnhancementsAndMaster(
                trigger,
                "sein-a",
                "sein-a-master-2");
            EnemyModel triggerTarget =
                CreateEnemy("stage1-swordsman");
            triggerTarget.SetPosition(new CombatVector2(1f, 0f));
            triggerTarget.TryAddShield(10000f);
            trigger.Stage.TryRegisterFieldUnit(triggerTarget);
            Assert.That(
                trigger.Combat.TryExecuteSkill(
                    new SkillExecutionRequest(
                        trigger.Selected,
                        catalog.GetSkill("sein-a"),
                        trigger.Stage.FieldUnits)),
                Is.True);
            EffectHandle triggerEffect = null;
            for (int tick = 0;
                tick < 30 && triggerEffect == null;
                tick++)
            {
                trigger.Actors.Tick(0.1f);
                triggerEffect = trigger.Effects.ActiveEffects
                    .FirstOrDefault(effect =>
                        effect.Visual.SpritePath
                        == "Assets/Image/Monster/Sein/SkillEffect/Sprite/1.png");
            }

            Assert.That(triggerEffect, Is.Not.Null);
            Assert.That(
                triggerEffect.Visual.AnimatorControllerPath,
                Is.EqualTo(
                    "Assets/Image/Monster/Sein/SkillEffect/Sprite/1.controller"));
        }

        [Test]
        /* ChoicePlanAndGraphsStayScopedToTheirTargetSkill 시나리오의 기대 동작과 상태 변화를 검증한다. */
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
        /* LineHitUsesFullGeometryAndAppliesAttachedPayloadOnce 시나리오의 기대 동작과 상태 변화를 검증한다. */
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
        /* TargetStackRateAndRepeatPerTargetExecuteFromEveD 시나리오의 기대 동작과 상태 변화를 검증한다. */
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
        /* AreaFieldReevaluatesRegisteredTargetsAtEveryTick 시나리오의 기대 동작과 상태 변화를 검증한다. */
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
        /* SourceShieldLayersExpireIndependentlyAndReflectAbsorbedDamage 시나리오의 기대 동작과 상태 변화를 검증한다. */
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
                catalog.GetSkill("GuardianFlag"),
                10f);
            Assert.That(
                ally.RemoveShield(ally, "GuardianFlag"),
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
        /* NexusContactIsRemovedAfterExactlyOneDamageRequest 시나리오의 기대 동작과 상태 변화를 검증한다. */
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
        /* EndCombatUnsubscribesStatusExpiryAndClearsTriggerState 시나리오의 기대 동작과 상태 변화를 검증한다. */
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
        /* ThresholdStatusUsesNameMarkTenAndRunsAfterBaseStatus 시나리오의 기대 동작과 상태 변화를 검증한다. */
        public void ThresholdStatusUsesNameMarkTenAndRunsAfterBaseStatus()
        {
            float belowThreshold = ExecuteVegaBAndReadSilenceDuration(9);
            float atThreshold = ExecuteVegaBAndReadSilenceDuration(10);

            Assert.That(belowThreshold, Is.EqualTo(3f).Within(0.0001f));
            Assert.That(atThreshold, Is.EqualTo(4f).Within(0.0001f));
        }

        [Test]
        /* BurstIndexZeroAndFollowUpProjectileRemainSeparateShots 시나리오의 기대 동작과 상태 변화를 검증한다. */
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
        /* TriggerOwnedGraphUsesItsDeclaredOwnerAndEventTarget 시나리오의 기대 동작과 상태 변화를 검증한다. */
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

            Assert.That(
                CountStatus(target, "name-mark"),
                Is.EqualTo(1),
                "The retained trigger payload has max_stacks=1 and stack_amount=2.");
            Assert.That(CountStatus(fixture.Selected, "name-mark"), Is.Zero);
        }

        [Test]
        /* CentralTickAppliesLearnedPassiveWithoutExternalCallback 시나리오의 기대 동작과 상태 변화를 검증한다. */
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
        /* EqualDistanceOrderIsStable 시나리오의 기대 동작과 상태 변화를 검증한다. */
        public void EqualDistanceOrderIsStable()
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

        }

        [Test]
        /* TriggeredSkillCarriesAncestryAndCannotRetriggerItsOrigin 시나리오의 기대 동작과 상태 변화를 검증한다. */
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

        [Test]
        /* VegaTargetingAndDeploymentUseOnlyTheConfiguredNameMark 시나리오의 기대 동작과 상태 변화를 검증한다. */
        public void VegaTargetingAndDeploymentUseOnlyTheConfiguredNameMark()
        {
            RuntimeFixture fixture = CreateFixture("vega");
            Assert.That(
                fixture.Selected.SkillBucket.TryLearnActive(
                    catalog.GetSkill("vega-d")),
                Is.True);
            EnemyModel manyOtherStacks = CreateEnemy("stage2-arsen");
            EnemyModel marked = CreateEnemy("stage2-arsen");
            manyOtherStacks.SetPosition(new CombatVector2(5f, 0f));
            marked.SetPosition(new CombatVector2(2f, 0f));
            manyOtherStacks.ApplyStatus(
                catalog.GetStatus("shock"),
                fixture.Selected,
                null,
                10,
                "setup");
            marked.ApplyStatus(
                catalog.GetStatus("name-mark"),
                fixture.Selected,
                null,
                2,
                "setup");
            fixture.Stage.TryRegisterFieldUnit(manyOtherStacks);
            fixture.Stage.TryRegisterFieldUnit(marked);

            Assert.That(
                fixture.Targeting.Resolve(
                    fixture.Selected,
                    catalog.GetSkill("vega-e"),
                    fixture.Stage.FieldUnits)[0],
                Is.SameAs(marked));

            float unmarkedBefore = manyOtherStacks.CurrentHealth;
            float markedBefore = marked.CurrentHealth;
            Assert.That(
                fixture.Combat.TryExecuteSkill(new SkillExecutionRequest(
                    fixture.Selected,
                    catalog.GetSkill("vega-d"),
                    fixture.Stage.FieldUnits)),
                Is.True);
            Assert.That(manyOtherStacks.CurrentHealth, Is.EqualTo(unmarkedBefore));
            Assert.That(marked.CurrentHealth, Is.LessThan(markedBefore));
        }

        [Test]
        /* VegaFinalSentenceAddsConfiguredDamageForEachNameMarkStack 시나리오의 기대 동작과 상태 변화를 검증한다. */
        public void VegaFinalSentenceAddsConfiguredDamageForEachNameMarkStack()
        {
            float oneStackDamage = ExecuteVegaEAndReadDamage(1);
            float fourStackDamage = ExecuteVegaEAndReadDamage(4);

            Assert.That(fourStackDamage, Is.GreaterThan(oneStackDamage));
        }

        [Test]
        /* RinExecuteCastAndCooldownRewardsRequireThresholdAndKill 시나리오의 기대 동작과 상태 변화를 검증한다. */
        public void RinExecuteCastAndCooldownRewardsRequireThresholdAndKill()
        {
            RuntimeFixture blocked = CreateFixture("rin");
            Assert.That(
                blocked.Selected.SkillBucket.TryLearnActive(
                    catalog.GetSkill("rin-d")),
                Is.True);
            EnemyModel healthy = CreateEnemy("stage2-arsen");
            blocked.Stage.TryRegisterFieldUnit(healthy);
            Assert.That(
                blocked.Combat.TryExecuteSkill(new SkillExecutionRequest(
                    blocked.Selected,
                    catalog.GetSkill("rin-d"),
                    blocked.Stage.FieldUnits)),
                Is.False);

            RuntimeFixture survived = CreateFixture("rin");
            Assert.That(
                survived.Selected.SkillBucket.TryLearnActive(
                    catalog.GetSkill("rin-d")),
                Is.True);
            EnemyModel shielded = CreateEnemy("stage2-arsen");
            shielded.ApplyDamage(shielded.MaximumHealth * 0.75f);
            shielded.TryAddShield(10000f);
            survived.Stage.TryRegisterFieldUnit(shielded);
            Assert.That(
                survived.Combat.TryExecuteSkill(new SkillExecutionRequest(
                    survived.Selected,
                    catalog.GetSkill("rin-d"),
                    survived.Stage.FieldUnits)),
                Is.True);
            Assert.That(shielded.IsAlive, Is.True);
            Assert.That(
                survived.Selected.SkillBucket.GetCooldown("rin-d")
                    .RemainingCooldown,
                Is.EqualTo(9f).Within(0.0001f));

            RuntimeFixture killed = CreateFixture("rin");
            Assert.That(
                killed.Selected.SkillBucket.TryLearnActive(
                    catalog.GetSkill("rin-d")),
                Is.True);
            EnemyModel victim = CreateEnemy("stage1-swordsman");
            victim.ApplyDamage(victim.MaximumHealth - 1f);
            killed.Stage.TryRegisterFieldUnit(victim);
            Assert.That(
                killed.Combat.TryExecuteSkill(new SkillExecutionRequest(
                    killed.Selected,
                    catalog.GetSkill("rin-d"),
                    killed.Stage.FieldUnits)),
                Is.True);
            Assert.That(victim.IsAlive, Is.False);
            Assert.That(
                killed.Selected.SkillBucket.GetCooldown("rin-d")
                    .RemainingCooldown,
                Is.EqualTo(5.85f).Within(0.0001f));
        }

        [Test]
        /* RinExecuteFiltersCandidatesBeforeLowestHealthSelection 시나리오의 기대 동작과 상태 변화를 검증한다. */
        public void RinExecuteFiltersCandidatesBeforeLowestHealthSelection()
        {
            RuntimeFixture fixture = CreateFixture("rin");
            Assert.That(
                fixture.Selected.SkillBucket.TryLearnActive(
                    catalog.GetSkill("rin-d")),
                Is.True);
            EnemyModel healthyLowerAbsoluteHealth =
                CreateEnemy("stage1-swordsman");
            EnemyModel qualifyingHigherAbsoluteHealth =
                CreateEnemy("stage2-arsen");
            qualifyingHigherAbsoluteHealth.ApplyDamage(
                qualifyingHigherAbsoluteHealth.MaximumHealth * 0.75f);
            fixture.Stage.TryRegisterFieldUnit(healthyLowerAbsoluteHealth);
            fixture.Stage.TryRegisterFieldUnit(
                qualifyingHigherAbsoluteHealth);

            float healthyBefore = healthyLowerAbsoluteHealth.CurrentHealth;
            float qualifyingBefore =
                qualifyingHigherAbsoluteHealth.CurrentHealth;
            Assert.That(
                fixture.Combat.TryExecuteSkill(new SkillExecutionRequest(
                    fixture.Selected,
                    catalog.GetSkill("rin-d"),
                    fixture.Stage.FieldUnits)),
                Is.True);

            Assert.That(
                healthyLowerAbsoluteHealth.CurrentHealth,
                Is.EqualTo(healthyBefore));
            Assert.That(
                qualifyingHigherAbsoluteHealth.CurrentHealth,
                Is.LessThan(qualifyingBefore));
        }

        [Test]
        /* SkillStatusMaximumStacksCapsRepeatedSeinEApplications 시나리오의 기대 동작과 상태 변화를 검증한다. */
        public void SkillStatusMaximumStacksCapsRepeatedSeinEApplications()
        {
            RuntimeFixture fixture = CreateFixture("sein");
            Assert.That(
                fixture.Selected.SkillBucket.TryLearnActive(
                    catalog.GetSkill("sein-e")),
                Is.True);
            EnemyModel target = CreateEnemy("stage2-arsen");
            target.SetPosition(new CombatVector2(1f, 0f));
            fixture.Stage.TryRegisterFieldUnit(target);

            for (int cast = 0; cast < 2; cast++)
            {
                Assert.That(
                    fixture.Combat.TryExecuteSkill(new SkillExecutionRequest(
                        fixture.Selected,
                        catalog.GetSkill("sein-e"),
                        fixture.Stage.FieldUnits)),
                    Is.True);
                fixture.Selected.SkillBucket.GetCooldown("sein-e")
                    .ResetCooldown();
            }

            Assert.That(CountStatus(target, "fire-resist-down"), Is.EqualTo(1));
            Assert.That(
                target.StatusEffects.Single(status =>
                    status.Definition.status_effect_id == "fire-resist-down")
                    .MaximumStacks,
                Is.EqualTo(1));
        }

        [Test]
        /* AreaRuntimeKindTriggersVegaICooldownRefund 시나리오의 기대 동작과 상태 변화를 검증한다. */
        public void AreaRuntimeKindTriggersVegaICooldownRefund()
        {
            RuntimeFixture fixture = CreateFixture("vega");
            Assert.That(
                fixture.Selected.SkillBucket.TryLearnActive(
                    catalog.GetSkill("vega-d")),
                Is.True);
            Assert.That(
                fixture.Selected.SkillBucket.TryLearnPassive(
                    (PassiveDefinition)catalog.GetSkill("vega-i")),
                Is.True);
            MonsterModel ally = CreateMonster("eve", false);
            EnemyModel target = CreateEnemy("stage2-arsen");
            target.ApplyStatus(
                catalog.GetStatus("name-mark"),
                fixture.Selected,
                null,
                1,
                "setup");
            fixture.Stage.TryRegisterFieldUnit(ally);
            fixture.Stage.TryRegisterFieldUnit(target);
            Assert.That(
                fixture.Combat.TryExecuteSkill(new SkillExecutionRequest(
                    fixture.Selected,
                    catalog.GetSkill("vega-d"),
                    fixture.Stage.FieldUnits)),
                Is.True);
            float before = fixture.Selected.SkillBucket.GetCooldown("vega-d")
                .RemainingCooldown;

            fixture.Combat.ApplySkillDamage(
                ally,
                target,
                catalog.GetSkill("eve-c"),
                0.1f);
            fixture.Actors.Tick(0f);
            fixture.Actors.Tick(0f);

            Assert.That(
                fixture.Selected.SkillBucket.GetCooldown("vega-d")
                    .RemainingCooldown,
                Is.LessThan(before));
        }

        [Test]
        /* TriggeredSeinBCastUsesSeinGOriginForReloadReduction 시나리오의 기대 동작과 상태 변화를 검증한다. */
        public void TriggeredSeinBCastUsesSeinGOriginForReloadReduction()
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
            Assert.That(
                fixture.Selected.SkillBucket.TrySelectChoice(
                    catalog.GetChoice("sein-g-trait-3")),
                Is.True);
            EnemyModel target = CreateEnemy("stage2-arsen");
            target.SetPosition(new CombatVector2(1f, 0f));
            target.TryAddShield(100000f);
            fixture.Stage.TryRegisterFieldUnit(target);
            fixture.Combat.NotifyCombatStart(
                fixture.Selected,
                fixture.Stage.FieldUnits);
            var seinACooldown =
                fixture.Selected.SkillBucket.GetCooldown("sein-a");
            for (int shot = 0; shot < 8; shot++)
            {
                Assert.That(seinACooldown.TryUse(), Is.True);
                seinACooldown.Tick(0.32f);
            }
            float before = seinACooldown.RemainingReload;
            Assert.That(before, Is.GreaterThan(0f));
            Assert.That(fixture.Selected.IsAlive, Is.True);
            Assert.That(fixture.Selected.CanAct, Is.True);
            Assert.That(
                fixture.Targeting.ResolveOrderedAll(
                    fixture.Selected,
                    catalog.GetSkill("sein-b"),
                    fixture.Stage.FieldUnits,
                    target.Position),
                Is.Not.Empty);
            List<string> activatedSkills = new List<string>();
            fixture.Combat.SkillActivated += (_, skill) =>
                activatedSkills.Add(skill.skill_id);

            fixture.Combat.ApplySkillDamage(
                fixture.Selected,
                target,
                catalog.GetSkill("sein-a"),
                0.1f);
            for (int tick = 0; tick < 5; tick++)
            {
                fixture.Actors.Tick(0f);
            }

            CollectionAssert.Contains(activatedSkills, "sein-b");
            Assert.That(seinACooldown.RemainingReload, Is.LessThan(before));
        }

        [Test]
        /* ManualClickDrainsCompleteUsableSkillBatchWithoutStaleRequests 시나리오의 기대 동작과 상태 변화를 검증한다. */
        public void ManualClickDrainsCompleteUsableSkillBatchWithoutStaleRequests()
        {
            RuntimeFixture fixture = CreateFixture("ariel");
            SkillDefinition skillA = catalog.GetSkill("ariel-a");
            SkillDefinition skillB = catalog.GetSkill("ariel-b");
            SkillDefinition skillC = catalog.GetSkill("ariel-c");
            Assert.That(
                fixture.Selected.SkillBucket.TryLearnActive(skillB),
                Is.True);
            Assert.That(
                fixture.Selected.SkillBucket.TryLearnActive(skillC),
                Is.True);
            EnemyModel target = CreateEnemy("stage1-swordsman");
            target.SetPosition(new CombatVector2(2f, 0f));
            fixture.Stage.TryRegisterFieldUnit(target);
            fixture.Input.SetAutoSkillEnabled(false);
            fixture.Input.BeginManualFrame();

            foreach (SkillDefinition skill
                in new[] { skillA, skillB, skillC })
            {
                Assert.That(
                    fixture.Input.SubmitManualSkillRequest(
                        skill,
                        new CombatVector2(1f, 0f),
                        target.Position,
                        ManualInputPhase.Pressed,
                        false),
                    Is.True,
                    skill.skill_id);
            }

            Assert.That(fixture.Input.PendingRequestCount, Is.EqualTo(3));
            Assert.That(
                fixture.Input.Process(fixture.Stage.FieldUnits),
                Is.True);
            Assert.That(fixture.Input.PendingRequestCount, Is.Zero);
            Assert.That(
                fixture.Input.Process(fixture.Stage.FieldUnits),
                Is.False);
            Assert.That(
                fixture.Selected.SkillBucket.GetCooldown(
                    skillA.skill_id).CanUse(),
                Is.False);
            Assert.That(
                fixture.Selected.SkillBucket.GetCooldown(
                    skillB.skill_id).CanUse(),
                Is.False);
            Assert.That(
                fixture.Selected.SkillBucket.GetCooldown(
                    skillC.skill_id).CanUse(),
                Is.False);

            fixture.Selected.SkillBucket.TickCooldowns(100f);
            fixture.Input.BeginManualFrame();
            Assert.That(
                fixture.Input.SubmitManualSkillRequest(
                    skillA,
                    new CombatVector2(1f, 0f),
                    target.Position,
                    ManualInputPhase.Pressed,
                    false),
                Is.True);
            fixture.Input.BeginManualFrame();
            Assert.That(fixture.Input.PendingRequestCount, Is.Zero);
        }

        [Test]
        /* ManualSpatialFamiliesCreateActorsWithoutPointerTarget 시나리오의 기대 동작과 상태 변화를 검증한다. */
        public void ManualSpatialFamiliesCreateActorsWithoutPointerTarget()
        {
            RuntimeFixture projectile = CreateFixture("ariel");
            Assert.That(
                projectile.Combat.TryExecuteSkill(
                    new SkillExecutionRequest(
                        projectile.Selected,
                        catalog.GetSkill("ariel-a"),
                        projectile.Stage.FieldUnits,
                        new CombatVector2(1f, 0f),
                        new CombatVector2(6f, 0f))),
                Is.True);
            projectile.Actors.Tick(0f);
            projectile.Actors.Tick(0f);
            Assert.That(
                projectile.Effects.ActiveEffects.Single().Position,
                Is.EqualTo(projectile.Selected.Position));

            RuntimeFixture line = CreateFixture("eve");
            Assert.That(
                line.Selected.SkillBucket.TryLearnActive(
                    catalog.GetSkill("eve-b")),
                Is.True);
            Assert.That(
                line.Combat.TryExecuteSkill(
                    new SkillExecutionRequest(
                        line.Selected,
                        catalog.GetSkill("eve-b"),
                        line.Stage.FieldUnits,
                        new CombatVector2(0f, 1f),
                        new CombatVector2(0f, 7f))),
                Is.True);
            EffectHandle lineEffect = line.Effects.ActiveEffects.Single();
            Assert.That(lineEffect.Position, Is.EqualTo(line.Selected.Position));
            Assert.That(
                lineEffect.Direction,
                Is.EqualTo(new CombatVector2(0f, 1f)));

            RuntimeFixture area = CreateFixture("eve");
            Assert.That(
                area.Selected.SkillBucket.TryLearnActive(
                    catalog.GetSkill("eve-e")),
                Is.True);
            var areaPoint = new CombatVector2(7f, 3f);
            Assert.That(
                area.Combat.TryExecuteSkill(
                    new SkillExecutionRequest(
                        area.Selected,
                        catalog.GetSkill("eve-e"),
                        area.Stage.FieldUnits,
                        new CombatVector2(7f, 3f),
                        areaPoint)),
                Is.True);
            Assert.That(
                area.Effects.ActiveEffects.Single().Position,
                Is.EqualTo(areaPoint));

            RuntimeFixture single = CreateFixture("rin");
            Assert.That(
                single.Selected.SkillBucket.TryLearnActive(
                    catalog.GetSkill("rin-e")),
                Is.True);
            var singlePoint = new CombatVector2(5f, -2f);
            Assert.That(
                single.Combat.TryExecuteSkill(
                    new SkillExecutionRequest(
                        single.Selected,
                        catalog.GetSkill("rin-e"),
                        single.Stage.FieldUnits,
                        new CombatVector2(5f, -2f),
                        singlePoint)),
                Is.True);
            Assert.That(
                single.Effects.ActiveEffects.Single().Position,
                Is.EqualTo(singlePoint));
        }

        [Test]
        /* AreaAndLineReevaluateLateEntrantsAtAuthoredVisualScale 시나리오의 기대 동작과 상태 변화를 검증한다. */
        public void AreaAndLineReevaluateLateEntrantsAtAuthoredVisualScale()
        {
            RuntimeFixture area = CreateFixture("eve");
            SkillDefinition areaSkill = catalog.GetSkill("eve-e");
            Assert.That(
                area.Selected.SkillBucket.TryLearnActive(areaSkill),
                Is.True);
            EnemyModel areaTarget = CreateEnemy("stage1-swordsman");
            areaTarget.SetPosition(new CombatVector2(20f, 20f));
            area.Stage.TryRegisterFieldUnit(areaTarget);
            var center = new CombatVector2(5f, 2f);
            float areaBefore = areaTarget.CurrentHealth;
            Assert.That(
                area.Combat.TryExecuteSkill(
                    new SkillExecutionRequest(
                        area.Selected,
                        areaSkill,
                        area.Stage.FieldUnits,
                        center - area.Selected.Position,
                        center)),
                Is.True);
            EffectHandle areaEffect = area.Effects.ActiveEffects.Single();
            Assert.That(areaEffect.Position, Is.EqualTo(center));
            Assert.That(areaEffect.Direction, Is.EqualTo(default(
                CombatVector2)));
            area.Actors.Tick(0f);
            area.Actors.Tick(0f);
            areaTarget.SetPosition(center);
            area.Actors.Tick(0.8f);
            Assert.That(
                areaTarget.CurrentHealth,
                Is.LessThan(areaBefore));

            RuntimeFixture line = CreateFixture("eve");
            SkillDefinition lineSkill = catalog.GetSkill("eve-b");
            Assert.That(
                line.Selected.SkillBucket.TryLearnActive(lineSkill),
                Is.True);
            EnemyModel lineTarget = CreateEnemy("stage1-swordsman");
            lineTarget.SetPosition(new CombatVector2(3f, 10f));
            line.Stage.TryRegisterFieldUnit(lineTarget);
            float lineBefore = lineTarget.CurrentHealth;
            Assert.That(
                line.Combat.TryExecuteSkill(
                    new SkillExecutionRequest(
                        line.Selected,
                        lineSkill,
                        line.Stage.FieldUnits,
                        new CombatVector2(1f, 0f),
                        new CombatVector2(8f, 0f))),
                Is.True);
            EffectHandle lineEffect = line.Effects.ActiveEffects.Single();
            float authoredScale = ReadFloatColumn(
                lineSkill,
                "runtime_visual_scale");
            Assert.That(
                lineEffect.Visual.Scale,
                Is.EqualTo(authoredScale).Within(0.0001f));
            Assert.That(
                lineEffect.Direction,
                Is.EqualTo(new CombatVector2(1f, 0f)));
            line.Actors.Tick(0f);
            line.Actors.Tick(0f);
            lineTarget.SetPosition(new CombatVector2(3f, 0f));
            line.Actors.Tick(0.15f);
            Assert.That(
                lineTarget.CurrentHealth,
                Is.LessThan(lineBefore));
            Assert.That(
                lineEffect.Visual.Scale,
                Is.EqualTo(authoredScale).Within(0.0001f));
        }

        [Test]
        /* ProjectileUsesFixedSweptDirectionAndFinitePierceBudget 시나리오의 기대 동작과 상태 변화를 검증한다. */
        public void ProjectileUsesFixedSweptDirectionAndFinitePierceBudget()
        {
            MonsterModel source = CreateMonster("ariel", false);
            EnemyModel behind = CreateEnemy("stage1-swordsman");
            EnemyModel first = CreateEnemy("stage1-swordsman");
            EnemyModel second = CreateEnemy("stage1-archer");
            EnemyModel third = CreateEnemy("stage1-priest");
            EnemyModel offAxis = CreateEnemy("stage1-shieldbearer");
            source.SetPosition(default);
            behind.SetPosition(new CombatVector2(-1f, 0f));
            first.SetPosition(new CombatVector2(1f, 0f));
            second.SetPosition(new CombatVector2(2f, 0f));
            third.SetPosition(new CombatVector2(3f, 0f));
            offAxis.SetPosition(new CombatVector2(2f, 5f));
            var units = new UnitBaseModel[]
            {
                source,
                behind,
                first,
                second,
                third,
                offAxis
            };
            SkillTargeting targeting = new SkillTargeting(
                _ => 0,
                _ => new CombatFootprint(0.1f, 0.1f));
            EffectManager effects =
                NewCoreTestFactory.CreateComponent<EffectManager>();
            EffectHandle effect = effects.Create(
                string.Empty,
                default,
                new CombatVector2(1f, 0f));
            var hits = new List<UnitBaseModel>();
            var impactPositions = new List<CombatVector2>();
            var actor = new ProjectileActor(
                (ProjectileDefinition)catalog.GetSkill("ariel-a"),
                source,
                units,
                targeting,
                default,
                new CombatVector2(1f, 0f),
                10f,
                5f,
                2,
                (target, position) =>
                {
                    hits.Add(target);
                    impactPositions.Add(position);
                },
                effect,
                effects);

            actor.Tick(0.5f);
            CollectionAssert.AreEqual(
                new[] { first, second },
                hits);
            Assert.That(actor.IsComplete, Is.True);
            Assert.That(actor.Direction, Is.EqualTo(new CombatVector2(1f, 0f)));
            Assert.That(actor.HitCount, Is.EqualTo(2));
            Assert.That(actor.Position.X, Is.LessThan(second.Position.X));
            Assert.That(
                impactPositions[0].X,
                Is.LessThan(first.Position.X));
            CollectionAssert.DoesNotContain(hits, behind);
            CollectionAssert.DoesNotContain(hits, third);
            CollectionAssert.DoesNotContain(hits, offAxis);
            actor.Tick(0.5f);
            Assert.That(hits, Has.Count.EqualTo(2));
        }

        [Test]
        /* EmptyProjectileKeepsDirectionUntilFallbackLifetime 시나리오의 기대 동작과 상태 변화를 검증한다. */
        public void EmptyProjectileKeepsDirectionUntilFallbackLifetime()
        {
            RuntimeFixture fixture = CreateFixture("ariel");
            Assert.That(
                fixture.Combat.TryExecuteSkill(
                    new SkillExecutionRequest(
                        fixture.Selected,
                        catalog.GetSkill("ariel-a"),
                        fixture.Stage.FieldUnits,
                        new CombatVector2(1f, 0f),
                        new CombatVector2(10f, 0f))),
                Is.True);
            fixture.Actors.Tick(0f);
            fixture.Actors.Tick(0f);
            fixture.Actors.Tick(0f);
            ProjectileActor actor = fixture.Actors.ActiveActors
                .OfType<ProjectileActor>()
                .Single();

            fixture.Actors.Tick(0.5f);

            Assert.That(actor.IsComplete, Is.False);
            Assert.That(actor.Position.X, Is.GreaterThan(0f));
            Assert.That(
                actor.Direction,
                Is.EqualTo(new CombatVector2(1f, 0f)));
        }

        [Test]
        /* PriestTargetsLowestHealthRatioAllyAndHealsAtItsPosition 시나리오의 기대 동작과 상태 변화를 검증한다. */
        public void PriestTargetsLowestHealthRatioAllyAndHealsAtItsPosition()
        {
            RuntimeFixture fixture = CreateFixture("ariel");
            fixture.Selected.SetPosition(new CombatVector2(20f, 0f));
            EnemyModel priest = CreateEnemy("stage1-priest");
            EnemyModel ratioTarget =
                CreateEnemy("stage1-shieldbearer");
            EnemyModel lowerAbsoluteHealth =
                CreateEnemy("stage1-priest");
            priest.SetPosition(new CombatVector2(10f, 0f));
            ratioTarget.SetPosition(default);
            lowerAbsoluteHealth.SetPosition(new CombatVector2(2f, 0f));
            ratioTarget.ApplyDamage(
                ratioTarget.MaximumHealth * 0.5f);
            lowerAbsoluteHealth.ApplyDamage(
                lowerAbsoluteHealth.MaximumHealth * 0.25f);
            fixture.Stage.TryRegisterFieldUnit(priest);
            fixture.Stage.TryRegisterFieldUnit(ratioTarget);
            fixture.Stage.TryRegisterFieldUnit(lowerAbsoluteHealth);
            var nexus = new NexusModel(100f);
            var controller = new EnemyActionController(
                priest,
                fixture.Combat,
                fixture.Targeting,
                new UnitMovementController(),
                fixture.Stage,
                nexus,
                0.2f);

            float startX = priest.Position.X;
            controller.Tick(1f, fixture.Stage.FieldUnits);
            Assert.That(priest.Position.X, Is.LessThan(startX));
            float before = ratioTarget.CurrentHealth;
            for (int tick = 0;
                tick < 20
                    && ratioTarget.CurrentHealth == before;
                tick++)
            {
                controller.Tick(1f, fixture.Stage.FieldUnits);
            }

            float expectedAmount = 50f
                + (priest.EnemyDefinition.spell_power.Value * 1.2f);
            Assert.That(
                ratioTarget.CurrentHealth,
                Is.EqualTo(Math.Min(
                    ratioTarget.MaximumHealth,
                    before + expectedAmount)).Within(0.0001f));
            Assert.That(
                lowerAbsoluteHealth.CurrentHealth,
                Is.EqualTo(
                    lowerAbsoluteHealth.MaximumHealth * 0.75f)
                    .Within(0.0001f));
            EffectHandle healEffect =
                fixture.Effects.ActiveEffects.Last();
            Assert.That(
                healEffect.Position,
                Is.EqualTo(ratioTarget.Position));
            fixture.Actors.Tick(0f);
            fixture.Actors.Tick(0.99f);
            Assert.That(
                fixture.Effects.ActiveEffects.Contains(healEffect),
                Is.True);
            fixture.Actors.Tick(0.02f);
            Assert.That(
                fixture.Effects.ActiveEffects.Contains(healEffect),
                Is.False);
        }

        [Test]
        /* ShieldUpIsTimedIncomingDamageBuffAndCreatesNoShield 시나리오의 기대 동작과 상태 변화를 검증한다. */
        public void ShieldUpIsTimedIncomingDamageBuffAndCreatesNoShield()
        {
            SkillDefinition shieldUp = catalog.GetSkill("ShieldUp");
            Assert.That(shieldUp, Is.TypeOf<BuffDefinition>());
            RuntimeFixture fixture = CreateFixture("ariel");
            EnemyModel baseline =
                CreateEnemy("stage1-shieldbearer");
            EnemyModel buffed =
                CreateEnemy("stage1-shieldbearer");
            fixture.Stage.TryRegisterFieldUnit(baseline);
            fixture.Stage.TryRegisterFieldUnit(buffed);

            Assert.That(
                fixture.Combat.TryExecuteSkill(
                    new SkillExecutionRequest(
                        buffed,
                        shieldUp,
                        fixture.Stage.FieldUnits)),
                Is.True);
            Assert.That(buffed.CurrentShield, Is.Zero);
            Assert.That(
                buffed.ResolveRuntimeModifier(
                    "StatusDamageTakenBonus"),
                Is.EqualTo(-0.75f).Within(0.0001f));

            CombatResult baselineHit =
                fixture.Combat.ApplySkillDamage(
                    fixture.Selected,
                    baseline,
                    catalog.GetSkill("ariel-c"),
                    1f,
                    eventExecuted: true);
            CombatResult buffedHit =
                fixture.Combat.ApplySkillDamage(
                    fixture.Selected,
                    buffed,
                    catalog.GetSkill("ariel-c"),
                    1f,
                    eventExecuted: true);
            Assert.That(
                buffedHit.DamageAmount,
                Is.EqualTo(
                    (float)Math.Round(
                        baselineHit.DamageAmount * 0.25f))
                    .Within(0.0001f));

            buffed.TickStatusEffects(3.99f);
            Assert.That(
                buffed.ResolveRuntimeModifier(
                    "StatusDamageTakenBonus"),
                Is.EqualTo(-0.75f).Within(0.0001f));
            buffed.TickStatusEffects(0.02f);
            Assert.That(
                buffed.ResolveRuntimeModifier(
                    "StatusDamageTakenBonus"),
                Is.Zero);
        }

        [Test]
        /* ArielBaseVisualSurvivesOneSecondMinimum 시나리오의 기대 동작과 상태 변화를 검증한다. */
        public void ArielBaseVisualSurvivesOneSecondMinimum()
        {
            RuntimeFixture fixture = CreateFixture("ariel");
            SkillDefinition skill = catalog.GetSkill("ariel-e");
            Assert.That(
                fixture.Selected.SkillBucket.TryLearnActive(skill),
                Is.True);
            EnemyModel target = CreateEnemy("stage1-swordsman");
            target.SetPosition(new CombatVector2(4f, 0f));
            fixture.Stage.TryRegisterFieldUnit(target);

            Assert.That(
                fixture.Combat.TryExecuteSkill(
                    new SkillExecutionRequest(
                        fixture.Selected,
                        skill,
                        fixture.Stage.FieldUnits,
                        targetPoint: target.Position)),
                Is.True);
            fixture.Actors.Tick(0f);
            string baseVisualPath =
                ReadStringColumn(
                    skill,
                    "runtime_visual_sprite_path");
            SingleAttackActor baseActor =
                fixture.Actors.ActiveActors
                    .OfType<SingleAttackActor>()
                    .Single(actor =>
                        actor.Effect.Visual.SpritePath
                        == baseVisualPath);

            fixture.Actors.Tick(0.99f);
            Assert.That(baseActor.IsComplete, Is.False);
            fixture.Actors.Tick(0.02f);
            Assert.That(baseActor.IsComplete, Is.True);
            Assert.That(
                fixture.Effects.ActiveEffects.Any(effect =>
                    effect.Visual.SpritePath != baseVisualPath),
                Is.True,
                "The separate six-second shield graph visual remains active.");
            fixture.Actors.Tick(4.98f);
            Assert.That(
                fixture.Effects.ActiveEffects.Any(effect =>
                    effect.Visual.SpritePath != baseVisualPath),
                Is.True);
            fixture.Actors.Tick(0.02f);
            Assert.That(
                fixture.Effects.ActiveEffects.Any(effect =>
                    effect.Visual.SpritePath != baseVisualPath),
                Is.False);
        }

        [Test]
        /* UnitAnchorAndAutomaticAreaUseResolvedTargetPositions 시나리오의 기대 동작과 상태 변화를 검증한다. */
        public void UnitAnchorAndAutomaticAreaUseResolvedTargetPositions()
        {
            RuntimeFixture anchored = CreateFixture("ariel");
            SkillDefinition anchorSkill =
                catalog.GetSkill("ariel-d");
            Assert.That(
                anchored.Selected.SkillBucket.TryLearnActive(
                    anchorSkill),
                Is.True);
            EnemyModel anchorTarget =
                CreateEnemy("stage1-swordsman");
            anchorTarget.SetPosition(new CombatVector2(4f, 2f));
            anchored.Stage.TryRegisterFieldUnit(anchorTarget);
            Assert.That(
                anchored.Combat.TryExecuteSkill(
                    new SkillExecutionRequest(
                        anchored.Selected,
                        anchorSkill,
                        anchored.Stage.FieldUnits)),
                Is.True);
            Assert.That(
                anchored.Effects.ActiveEffects.Single().Position,
                Is.EqualTo(anchorTarget.Position));

            RuntimeFixture area = CreateFixture("eve");
            SkillDefinition areaSkill = catalog.GetSkill("eve-e");
            Assert.That(
                area.Selected.SkillBucket.TryLearnActive(areaSkill),
                Is.True);
            EnemyModel areaTarget =
                CreateEnemy("stage1-swordsman");
            areaTarget.SetPosition(new CombatVector2(6f, -1f));
            area.Stage.TryRegisterFieldUnit(areaTarget);
            Assert.That(
                area.Combat.TryExecuteSkill(
                    new SkillExecutionRequest(
                        area.Selected,
                        areaSkill,
                        area.Stage.FieldUnits)),
                Is.True);
            Assert.That(
                area.Effects.ActiveEffects.Single().Position,
                Is.EqualTo(areaTarget.Position));
            Assert.That(
                area.Effects.ActiveEffects.Single().Direction,
                Is.EqualTo(default(CombatVector2)));
        }

        [Test]
        /* ArielBShieldsOnlyAlliesWithAuthoredFormulaAndEnhancement 시나리오의 기대 동작과 상태 변화를 검증한다. */
        public void ArielBShieldsOnlyAlliesWithAuthoredFormulaAndEnhancement()
        {
            RuntimeFixture fixture = CreateFixture("ariel");
            SkillDefinition shield = catalog.GetSkill("ariel-b");
            Assert.That(
                fixture.Selected.SkillBucket.TryLearnActive(shield),
                Is.True);
            Assert.That(
                fixture.Selected.SkillBucket.TrySelectChoice(
                    catalog.GetChoice("ariel-b-trait-1")),
                Is.True);
            MonsterModel ally = CreateMonster("eve", false);
            EnemyModel enemy = CreateEnemy("stage1-swordsman");
            fixture.Stage.TryRegisterFieldUnit(ally);
            fixture.Stage.TryRegisterFieldUnit(enemy);

            Assert.That(
                fixture.Combat.TryExecuteSkill(
                    new SkillExecutionRequest(
                        fixture.Selected,
                        shield,
                        fixture.Stage.FieldUnits)),
                Is.True);
            float expected =
                (35f
                    + fixture.Combat.CalculateSpellPower(
                        fixture.Selected) * 1.4f)
                * 1.3f;
            Assert.That(
                fixture.Selected.CurrentShield,
                Is.EqualTo(expected).Within(0.0001f));
            Assert.That(
                ally.CurrentShield,
                Is.EqualTo(expected).Within(0.0001f));
            Assert.That(enemy.CurrentShield, Is.Zero);

            fixture.Actors.Tick(0f);
            fixture.Actors.Tick(4f);
            fixture.Selected.SkillBucket
                .GetCooldown(shield.skill_id)
                .ResetCooldown();
            Assert.That(
                fixture.Combat.TryExecuteSkill(
                    new SkillExecutionRequest(
                        fixture.Selected,
                        shield,
                        fixture.Stage.FieldUnits)),
                Is.True);
            Assert.That(
                ally.CurrentShield,
                Is.EqualTo(expected).Within(0.0001f),
                "same_source_refresh/take_highest must refresh, not stack.");

            fixture.Actors.Tick(0f);
            fixture.Actors.Tick(0.99f);
            Assert.That(ally.CurrentShield, Is.GreaterThan(0f));
            fixture.Actors.Tick(0.02f);
            Assert.That(
                ally.CurrentShield,
                Is.EqualTo(expected).Within(0.0001f),
                "The first expiration actor must not remove the refreshed layer.");
            fixture.Actors.Tick(3.98f);
            Assert.That(ally.CurrentShield, Is.GreaterThan(0f));
            fixture.Actors.Tick(0.02f);
            Assert.That(ally.CurrentShield, Is.Zero);
        }

        [Test]
        /* EveFShieldsOnlyAlliesWithLearnedLightningActiveSkills 시나리오의 기대 동작과 상태 변화를 검증한다. */
        public void EveFShieldsOnlyAlliesWithLearnedLightningActiveSkills()
        {
            RuntimeFixture fixture = CreateFixture("eve");
            PassiveDefinition passive =
                (PassiveDefinition)catalog.GetSkill("eve-f");
            Assert.That(
                fixture.Selected.SkillBucket.TryLearnPassive(passive),
                Is.True);
            Assert.That(
                fixture.Selected.SkillBucket.TrySelectChoice(
                    catalog.GetChoice("eve-f-trait-1")),
                Is.True);
            MonsterModel lightningAlly = CreateMonster("eve", false);
            MonsterModel holyAlly = CreateMonster("ariel", false);
            fixture.Stage.TryRegisterFieldUnit(lightningAlly);
            fixture.Stage.TryRegisterFieldUnit(holyAlly);

            fixture.Combat.ApplyPassiveChanges(
                fixture.Stage.FieldUnits);
            fixture.Actors.Tick(0f);
            fixture.Actors.Tick(0f);

            float expected = fixture.Combat.CalculateSpellPower(
                    fixture.Selected)
                * 1.2f
                * 1.4f;
            Assert.That(
                fixture.Selected.CurrentShield,
                Is.EqualTo(expected).Within(0.0001f));
            Assert.That(
                lightningAlly.CurrentShield,
                Is.EqualTo(expected).Within(0.0001f));
            Assert.That(holyAlly.CurrentShield, Is.Zero);

            fixture.Actors.Tick(11.99f);
            Assert.That(
                lightningAlly.CurrentShield,
                Is.GreaterThan(0f));
            fixture.Actors.Tick(0.02f);
            Assert.That(lightningAlly.CurrentShield, Is.Zero);
        }

        [Test]
        /* EveFPassiveEnhancementsKeepTheirSeparateGraphEffects 시나리오의 기대 동작과 상태 변화를 검증한다. */
        public void EveFPassiveEnhancementsKeepTheirSeparateGraphEffects()
        {
            RuntimeFixture shocked = CreateFixture("eve");
            PassiveDefinition passive =
                (PassiveDefinition)catalog.GetSkill("eve-f");
            Assert.That(
                shocked.Selected.SkillBucket.TryLearnPassive(passive),
                Is.True);
            Assert.That(
                shocked.Selected.SkillBucket.TrySelectChoice(
                    catalog.GetChoice("eve-f-trait-2")),
                Is.True);
            EnemyModel shockedEnemy =
                CreateEnemy("stage1-swordsman");
            shockedEnemy.ApplyStatus(
                catalog.GetStatus("shock"),
                shocked.Selected,
                null,
                1);
            shocked.Stage.TryRegisterFieldUnit(shockedEnemy);
            shocked.Combat.ApplyPassiveChanges(
                shocked.Stage.FieldUnits);
            Assert.That(
                shockedEnemy.ResolveRuntimeModifier(
                    "StatusDamageTakenBonus"),
                Is.EqualTo(0.16f).Within(0.0001f));

            RuntimeFixture shielded = CreateFixture("eve");
            Assert.That(
                shielded.Selected.SkillBucket.TryLearnPassive(
                    (PassiveDefinition)catalog.GetSkill("eve-f")),
                Is.True);
            Assert.That(
                shielded.Selected.SkillBucket.TrySelectChoice(
                    catalog.GetChoice("eve-f-trait-3")),
                Is.True);
            MonsterModel lightningAlly =
                CreateMonster("eve", false);
            MonsterModel holyAlly =
                CreateMonster("ariel", false);
            shielded.Stage.TryRegisterFieldUnit(lightningAlly);
            shielded.Stage.TryRegisterFieldUnit(holyAlly);
            shielded.Combat.ApplyPassiveChanges(
                shielded.Stage.FieldUnits);

            Assert.That(
                shielded.Selected.ResolveRuntimeModifier(
                    "StatusActionSpeedBonus"),
                Is.EqualTo(0.12f).Within(0.0001f));
            Assert.That(
                lightningAlly.ResolveRuntimeModifier(
                    "StatusActionSpeedBonus"),
                Is.EqualTo(0.12f).Within(0.0001f));
            Assert.That(
                holyAlly.ResolveRuntimeModifier(
                    "StatusActionSpeedBonus"),
                Is.Zero);
        }

        [Test]
        /* ArielEShieldGraphUsesFlatPlusSpellPowerOnly 시나리오의 기대 동작과 상태 변화를 검증한다. */
        public void ArielEShieldGraphUsesFlatPlusSpellPowerOnly()
        {
            RuntimeFixture fixture = CreateFixture("ariel");
            SkillDefinition skill = catalog.GetSkill("ariel-e");
            Assert.That(
                fixture.Selected.SkillBucket.TryLearnActive(skill),
                Is.True);
            MonsterModel ally = CreateMonster("eve", false);
            EnemyModel target = CreateEnemy("stage1-swordsman");
            target.SetPosition(new CombatVector2(1f, 0f));
            fixture.Stage.TryRegisterFieldUnit(ally);
            fixture.Stage.TryRegisterFieldUnit(target);

            Assert.That(
                fixture.Combat.TryExecuteSkill(
                    new SkillExecutionRequest(
                        fixture.Selected,
                        skill,
                        fixture.Stage.FieldUnits)),
                Is.True);
            fixture.Actors.Tick(0f);
            fixture.Actors.Tick(0f);

            float expected = 50f
                + fixture.Combat.CalculateSpellPower(
                    fixture.Selected) * 1.6f;
            Assert.That(
                fixture.Selected.CurrentShield,
                Is.EqualTo(expected).Within(0.0001f));
            Assert.That(
                ally.CurrentShield,
                Is.EqualTo(expected).Within(0.0001f));
            Assert.That(target.CurrentShield, Is.Zero);
        }

        [Test]
        /* EnemyMovesTowardNexusWhenAllSkillsAreCoolingDown 시나리오의 기대 동작과 상태 변화를 검증한다. */
        public void EnemyMovesTowardNexusWhenAllSkillsAreCoolingDown()
        {
            RuntimeFixture fixture = CreateFixture("ariel");
            EnemyModel enemy = CreateEnemy("stage1-swordsman");
            var nexus = new NexusModel(100f);
            enemy.SetPosition(default);
            nexus.SetPosition(new CombatVector2(10f, 0f));
            fixture.Selected.ApplyDamage(
                fixture.Selected.MaximumHealth);
            fixture.Stage.TryRegisterFieldUnit(enemy);
            fixture.Stage.TryRegisterFieldUnit(nexus);
            foreach (var cooldown in enemy.SkillBucket.Cooldowns.Values)
            {
                Assert.That(cooldown.TryUse(), Is.True);
            }
            var controller = new EnemyActionController(
                enemy,
                fixture.Combat,
                fixture.Targeting,
                new UnitMovementController(),
                fixture.Stage,
                nexus,
                0.2f);

            controller.Tick(1f, fixture.Stage.FieldUnits);

            Assert.That(enemy.Position.X, Is.GreaterThan(0f));
        }

        [Test]
        /* EnemyRetargetsLivingMonsterOnItsNextActionStep 시나리오의 기대 동작과 상태 변화를 검증한다. */
        public void EnemyRetargetsLivingMonsterOnItsNextActionStep()
        {
            RuntimeFixture fixture = CreateFixture("ariel");
            MonsterModel survivor = CreateMonster("eve", false);
            EnemyModel enemy = CreateEnemy("stage1-swordsman");
            fixture.Selected.SetPosition(
                new CombatVector2(-2f, 0f));
            survivor.SetPosition(new CombatVector2(10f, 0f));
            enemy.SetPosition(default);
            fixture.Selected.ApplyDamage(
                fixture.Selected.MaximumHealth);
            fixture.Stage.TryRegisterFieldUnit(survivor);
            fixture.Stage.TryRegisterFieldUnit(enemy);
            var controller = new EnemyActionController(
                enemy,
                fixture.Combat,
                fixture.Targeting,
                new UnitMovementController(),
                fixture.Stage,
                new NexusModel(100f),
                0.2f);

            controller.Tick(1f, fixture.Stage.FieldUnits);

            Assert.That(enemy.Position.X, Is.GreaterThan(0f));
        }

        [Test]
        /* GuardianFlagCooldownFallsThroughToSlashTrace 시나리오의 기대 동작과 상태 변화를 검증한다. */
        public void GuardianFlagCooldownFallsThroughToSlashTrace()
        {
            RuntimeFixture fixture = CreateFixture("ariel");
            EnemyModel guardian =
                CreateEnemy("stage1-guardian-captain");
            guardian.SetPosition(new CombatVector2(3f, 0f));
            fixture.Selected.SetPosition(default);
            fixture.Stage.TryRegisterFieldUnit(guardian);
            var activated = new List<string>();
            fixture.Combat.SkillActivated += (_, skill) =>
                activated.Add(skill.skill_id);
            var controller = new EnemyActionController(
                guardian,
                fixture.Combat,
                fixture.Targeting,
                new UnitMovementController(),
                fixture.Stage,
                new NexusModel(100f),
                0.2f);

            controller.Tick(0f, fixture.Stage.FieldUnits);
            Assert.That(
                activated,
                Is.EqualTo(new[] { "GuardianFlag" }));
            Assert.That(
                guardian.SkillBucket.GetCooldown(
                    "GuardianFlag").CanUse(),
                Is.False);
            float initialX = guardian.Position.X;
            for (var tick = 0;
                tick < 6 && !activated.Contains("Slash");
                tick++)
            {
                controller.Tick(1f, fixture.Stage.FieldUnits);
            }
            Assert.That(guardian.Position.X, Is.LessThan(initialX));
            Assert.That(activated, Does.Contain("Slash"));
            Assert.That(
                guardian.SkillBucket.GetCooldown("Slash").CanUse(),
                Is.False);
            float before = fixture.Selected.CurrentHealth;
            fixture.Actors.Tick(0f);
            fixture.Actors.Tick(0f);
            Assert.That(
                fixture.Selected.CurrentHealth,
                Is.LessThan(before));
            Assert.That(
                fixture.Effects.ActiveEffects.Any(effect =>
                    effect.Visual.HasResource),
                Is.True);
        }

        /* PrepareChoiceOwner 테스트의 선행 런타임 상태를 구성한다. */
        private MonsterModel PrepareChoiceOwner(
            SkillChoiceDefinition choice)
        {
            MonsterModel owner =
                CreateMonster(choice.monster_id, false);
            PrepareChoiceOwner(owner, choice);
            return owner;
        }

        /* PrepareChoiceOwner 테스트의 선행 런타임 상태를 구성한다. */
        private void PrepareChoiceOwner(
            MonsterModel owner,
            SkillChoiceDefinition choice)
        {
            SkillDefinition skill =
                catalog.GetSkill(choice.skill_id);
            if (skill is PassiveDefinition passive)
            {
                char requiredSlot = (char)(
                    'A' + char.ToUpperInvariant(
                        passive.slot[0]) - 'F');
                EnsureActiveSlot(owner, requiredSlot);
                if (!string.IsNullOrEmpty(choice.target_skill_id))
                {
                    SkillDefinition target =
                        catalog.GetSkill(choice.target_skill_id);
                    if (!(target is PassiveDefinition))
                    {
                        EnsureActiveSkill(owner, target);
                    }
                }
                Assert.That(
                    owner.SkillBucket.TryLearnPassive(passive),
                    Is.True,
                    choice.choice_id);
            }
            else
            {
                EnsureActiveSkill(owner, skill);
            }

            if (choice.choice_group == "ActiveMaster")
            {
                SkillChoiceDefinition[] enhancements =
                    catalog.Choices.Values
                        .Where(candidate =>
                            candidate.monster_id == choice.monster_id
                            && candidate.skill_id == choice.skill_id
                            && candidate.choice_group
                                == "ActiveEnhancement")
                        .OrderBy(candidate =>
                            candidate.choice_id)
                        .ToArray();
                Assert.That(
                    enhancements.Length,
                    Is.GreaterThanOrEqualTo(
                        MonsterSkillBucket
                            .MaximumActiveEnhancementsPerSkill),
                    choice.choice_id);
                for (var index = 0;
                    index < enhancements.Length
                        && owner.SkillBucket.SelectedChoices.Count(
                            selected =>
                                selected.skill_id
                                    == choice.skill_id
                                && selected.choice_group
                                    == "ActiveEnhancement")
                            < MonsterSkillBucket
                                .MaximumActiveEnhancementsPerSkill;
                    index++)
                {
                    if (owner.SkillBucket.SelectedChoices.Contains(
                        enhancements[index]))
                    {
                        continue;
                    }
                    Assert.That(
                        owner.SkillBucket.TrySelectChoice(
                            enhancements[index]),
                        Is.True,
                        choice.choice_id);
                }
            }
        }

        /* EnsureActiveSlot 검증 조건을 공통 보조 절차로 확인한다. */
        private void EnsureActiveSlot(
            MonsterModel owner,
            char slot)
        {
            SkillDefinition skill = catalog.GetSkill(
                owner.MonsterDefinition.id
                + "-"
                + char.ToLowerInvariant(slot));
            EnsureActiveSkill(owner, skill);
        }

        /* EnsureActiveSkill 검증 조건을 공통 보조 절차로 확인한다. */
        private static void EnsureActiveSkill(
            MonsterModel owner,
            SkillDefinition skill)
        {
            if (owner.SkillBucket.ActiveSkills.Any(
                learned => learned.skill_id == skill.skill_id))
            {
                return;
            }

            Assert.That(
                owner.SkillBucket.TryLearnActive(skill),
                Is.True,
                skill.skill_id);
        }

        /* AssertFamilyExecutes 검증 조건을 공통 보조 절차로 확인한다. */
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

        /* AssertEnemyFamilyExecutes 검증 조건을 공통 보조 절차로 확인한다. */
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

        /* CreateFixture 테스트 대상을 필요한 의존성과 함께 구성한다. */
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
            StageManager stage =
                NewCoreTestFactory.CreateStageManager(session, 0, 0);
            stage.TryRegisterFieldUnit(monster);
            EffectManager effects =
                NewCoreTestFactory.CreateComponent<EffectManager>();
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
            PlayerInputController input =
                NewCoreTestFactory.CreateComponent<PlayerInputController>();
            RuntimeFixture fixture = new RuntimeFixture
            {
                Selected = monster,
                Stage = stage,
                Effects = effects,
                Actors = actors,
                Targeting = targeting,
                Combat = combat,
                Triggers = execution.Triggers,
                Execution = execution,
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

        /* CreateMonster 테스트 대상을 필요한 의존성과 함께 구성한다. */
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

        /* CreateEnemy 테스트 대상을 필요한 의존성과 함께 구성한다. */
        private EnemyModel CreateEnemy(string enemyId)
        {
            EnemyDefinition definition = catalog.GetEnemy(enemyId);
            return new EnemyModel(
                definition,
                catalog.GetSkill(definition.skill_slot_a_id),
                catalog.GetSkill(definition.skill_slot_b_id),
                (PassiveDefinition)catalog.GetSkill(definition.passive_id));
        }

        /* ExecuteProjectileAndReadDamage 시나리오의 기대 동작과 상태 변화를 검증한다. */
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

        /* SelectEnhancementsAndMaster 테스트의 선행 런타임 상태를 구성한다. */
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

        /* CountStatus 검증에 필요한 실제 런타임 값을 읽어 반환한다. */
        private static int CountStatus(UnitBaseModel unit, string statusId)
        {
            return unit.StatusEffects
                .Where(status =>
                    status.Definition.status_effect_id == statusId)
                .Sum(status => status.CurrentStacks);
        }

        /* EnsureTriggerOwnership 검증 조건을 공통 보조 절차로 확인한다. */
        private void EnsureTriggerOwnership(
            MonsterModel owner,
            SkillTriggerDefinition trigger)
        {
            EnsureSkillLearned(
                owner,
                catalog.GetSkill(trigger.source_skill_id));
            string required = ReadTriggerString(
                trigger,
                "requires_active_choice_id");
            if (string.IsNullOrEmpty(required))
            {
                return;
            }

            string[] choiceIds = required.Split(';', ',');
            for (var index = 0; index < choiceIds.Length; index++)
            {
                SkillChoiceDefinition choice =
                    catalog.GetChoice(choiceIds[index].Trim());
                EnsureSkillLearned(
                    owner,
                    catalog.GetSkill(choice.skill_id));
                if (choice.choice_group == "ActiveMaster")
                {
                    SkillChoiceDefinition[] enhancements =
                        catalog.Choices.Values
                            .Where(candidate =>
                                candidate.monster_id
                                    == choice.monster_id
                                && candidate.skill_id
                                    == choice.skill_id
                                && candidate.choice_group
                                    == "ActiveEnhancement")
                            .OrderBy(candidate =>
                                candidate.choice_id)
                            .Take(
                                MonsterSkillBucket
                                    .MaximumActiveEnhancementsPerSkill)
                            .ToArray();
                    for (var enhancementIndex = 0;
                        enhancementIndex < enhancements.Length;
                        enhancementIndex++)
                    {
                        if (!owner.SkillBucket.SelectedChoices
                            .Contains(enhancements[
                                enhancementIndex]))
                        {
                            Assert.That(
                                owner.SkillBucket.TrySelectChoice(
                                    enhancements[
                                        enhancementIndex]),
                                Is.True,
                                trigger.trigger_id);
                        }
                    }
                }
                if (!owner.SkillBucket.SelectedChoices.Contains(
                    choice))
                {
                    Assert.That(
                        owner.SkillBucket.TrySelectChoice(choice),
                        Is.True,
                        trigger.trigger_id);
                }
            }
        }

        /* PrepareGraphOwnerAndResolveSkill 테스트의 선행 런타임 상태를 구성한다. */
        private SkillDefinition PrepareGraphOwnerAndResolveSkill(
            MonsterModel owner,
            ChoiceNodeDefinition node)
        {
            if (node.owner_kind == "Choice")
            {
                SkillChoiceDefinition choice =
                    catalog.GetChoice(node.owner_id);
                PrepareChoiceOwner(owner, choice);
                if (!owner.SkillBucket.SelectedChoices.Contains(
                    choice))
                {
                    Assert.That(
                        owner.SkillBucket.TrySelectChoice(choice),
                        Is.True,
                        node.owner_id);
                }
            }
            else if (node.owner_kind == "Trigger")
            {
                EnsureTriggerOwnership(
                    owner,
                    catalog.Triggers[node.owner_id]);
            }

            string skillId = node.target_skill_id;
            if (string.IsNullOrEmpty(skillId))
            {
                if (node.owner_kind == "Choice")
                {
                    SkillChoiceDefinition choice =
                        catalog.GetChoice(node.owner_id);
                    skillId = choice.target_skill_id;
                    if (string.IsNullOrEmpty(skillId))
                    {
                        skillId = choice.skill_id;
                    }
                }
                else if (node.owner_kind == "Trigger")
                {
                    skillId =
                        catalog.Triggers[node.owner_id]
                            .source_skill_id;
                }
                else
                {
                    skillId = node.owner_id;
                }
            }

            SkillDefinition skill = catalog.GetSkill(skillId);
            EnsureSkillLearned(owner, skill);
            return skill;
        }

        /* PrepareGraphConditions 테스트의 선행 런타임 상태를 구성한다. */
        private void PrepareGraphConditions(
            RuntimeFixture fixture,
            IReadOnlyList<ChoiceNodeDefinition> graph,
            UnitBaseModel target,
            MonsterModel ally)
        {
            for (var index = 0; index < graph.Count; index++)
            {
                ChoiceNodeDefinition node = graph[index];
                if (node.node_type_id == "StatusDurationBonus")
                {
                    ChoiceNodeDefinition statusSource =
                        catalog.ChoiceNodes.FirstOrDefault(
                            candidate =>
                                candidate.monster_id
                                    == node.monster_id
                                && candidate.target_skill_id
                                    == node.target_skill_id
                                && candidate.node_type_id
                                    == "ApplyStatus"
                                && candidate.arg_1
                                    == node.arg_1
                                && candidate.owner_kind
                                    == "Choice");
                    if (statusSource != null)
                    {
                        SkillChoiceDefinition sourceChoice =
                            catalog.GetChoice(
                                statusSource.owner_id);
                        PrepareChoiceOwner(
                            fixture.Selected,
                            sourceChoice);
                        if (!fixture.Selected.SkillBucket
                            .SelectedChoices.Contains(sourceChoice))
                        {
                            Assert.That(
                                fixture.Selected.SkillBucket
                                    .TrySelectChoice(sourceChoice),
                                Is.True,
                                node.owner_id);
                        }
                    }
                }
                if (node.node_type_id == "CooldownRefund"
                    || node.node_type_id == "CooldownRefundBonus"
                    || node.node_type_id == "CooldownReset")
                {
                    target.ApplyDamage(
                        Math.Max(0f, target.CurrentHealth - 1f));
                }
                if (node.node_type_id == "TargetStatusCritBonus")
                {
                    int requiredCritStacks = 1;
                    if (int.TryParse(
                            node.arg_4,
                            out int parsedRequiredCritStacks))
                    {
                        requiredCritStacks = parsedRequiredCritStacks;
                    }
                    ApplyContractStatus(
                        target,
                        fixture.Selected,
                        node.arg_1,
                        Math.Max(1, requiredCritStacks));
                }
                if (node.node_type_id == "RequiredSourceStatus")
                {
                    ApplyContractStatus(
                        fixture.Selected,
                        fixture.Selected,
                        node.arg_1,
                        10);
                }
                else if (node.node_type_id == "ConditionStatus")
                {
                    int stacks = 1;
                    if (int.TryParse(node.arg_4, out int parsedStacks))
                    {
                        stacks = parsedStacks;
                    }
                    ApplyContractStatus(
                        target,
                        fixture.Selected,
                        node.arg_1,
                        Math.Max(10, stacks),
                        node.arg_3);
                    if (ally != null)
                    {
                        ApplyContractStatus(
                            ally,
                            fixture.Selected,
                            node.arg_1,
                            10,
                            node.arg_3);
                    }
                }
                else if (node.node_type_id
                    == "ConditionAnyStatus"
                    || node.node_type_id
                        == "ConditionStatusExpression")
                {
                    char separator = ';';
                    if (node.node_type_id
                        == "ConditionStatusExpression")
                    {
                        separator = '&';
                    }
                    string[] statusIds =
                        (node.arg_1 ?? string.Empty)
                            .Split(separator);
                    for (var statusIndex = 0;
                        statusIndex < statusIds.Length;
                        statusIndex++)
                    {
                        ApplyContractStatus(
                            target,
                            fixture.Selected,
                            statusIds[statusIndex],
                            10);
                        if (ally != null)
                        {
                            ApplyContractStatus(
                                ally,
                                fixture.Selected,
                                statusIds[statusIndex],
                                10);
                        }
                    }
                }
                else if (node.node_type_id
                    == "ConditionSkillAttribute"
                    && ally != null)
                {
                    SkillDefinition matching =
                        catalog.Skills.Values.FirstOrDefault(
                            candidate =>
                                candidate.monster_id
                                    == ally.MonsterDefinition.id
                                && !(candidate
                                    is PassiveDefinition)
                                && candidate.attribute
                                    == node.arg_1);
                    if (matching != null)
                    {
                        EnsureActiveSkill(ally, matching);
                    }
                }
            }

            bool requiresShield = graph.Any(node =>
                string.Equals(
                    node.arg_1,
                    "shield",
                    StringComparison.OrdinalIgnoreCase)
                || string.Equals(
                    node.arg_2,
                    "shield",
                    StringComparison.OrdinalIgnoreCase));
            if (requiresShield)
            {
                target.TryAddShield(100000f);
                if (ally != null)
                {
                    ally.TryAddShield(100000f);
                }
            }
            int setupHits = 0;
            if (graph.Any(node =>
                    node.node_type_id == "ConditionHitCountMin"))
            {
                setupHits = 20;
            }
            for (var hit = 0; hit < setupHits; hit++)
            {
                fixture.Combat.ApplySkillDamage(
                    fixture.Selected,
                    target,
                    catalog.GetSkill(
                        fixture.Selected.MonsterDefinition.id
                        + "-a"),
                    0.01f);
            }
        }

        /* ApplyContractStatus 테스트의 선행 런타임 상태를 구성한다. */
        private void ApplyContractStatus(
            UnitBaseModel target,
            UnitBaseModel source,
            string statusId,
            int stacks,
            string sourceSkillId = null)
        {
            if (string.IsNullOrWhiteSpace(statusId))
            {
                return;
            }
            if (statusId == "shield")
            {
                target.TryAddShield(
                    100000f,
                    source,
                    sourceSkillId);
                return;
            }
            if (catalog.Statuses.TryGetValue(
                statusId,
                out var status))
            {
                target.ApplyStatus(
                    status,
                    source,
                    null,
                    stacks,
                    sourceSkillId);
            }
        }

        /* EnsureSkillLearned 검증 조건을 공통 보조 절차로 확인한다. */
        private void EnsureSkillLearned(
            MonsterModel owner,
            SkillDefinition skill)
        {
            if (skill is PassiveDefinition passive)
            {
                if (owner.SkillBucket.PassiveSkills.Contains(passive))
                {
                    return;
                }
                char requiredSlot = (char)(
                    'A' + char.ToUpperInvariant(
                        passive.slot[0]) - 'F');
                EnsureActiveSlot(owner, requiredSlot);
                Assert.That(
                    owner.SkillBucket.TryLearnPassive(passive),
                    Is.True,
                    skill.skill_id);
                return;
            }

            EnsureActiveSkill(owner, skill);
        }

        /* ResolveMatchingEventSkill 시나리오의 기대 동작과 상태 변화를 검증한다. */
        private SkillDefinition ResolveMatchingEventSkill(
            SkillTriggerDefinition trigger)
        {
            string configuredIds = ReadTriggerString(
                trigger,
                "event_skill_id");
            if (!string.IsNullOrEmpty(configuredIds))
            {
                return catalog.GetSkill(
                    configuredIds.Split(';', ',')[0].Trim());
            }

            string runtimeKinds = ReadTriggerString(
                trigger,
                "event_skill_runtime_kinds");
            string attributes = ReadTriggerString(
                trigger,
                "trigger_attribute");
            SkillDefinition fallback =
                catalog.GetSkill(trigger.source_skill_id);
            return catalog.Skills.Values.FirstOrDefault(skill =>
                MatchesTriggerRuntimeKind(
                    runtimeKinds,
                    skill.runtime_kind)
                && MatchesTriggerAttribute(
                    attributes,
                    skill.attribute))
                ?? fallback;
        }

        /* MatchesTriggerRuntimeKind 테스트 계약과 실제 값의 일치 여부를 판정한다. */
        private static bool MatchesTriggerRuntimeKind(
            string configured,
            string runtimeKind)
        {
            if (string.IsNullOrEmpty(configured))
            {
                return true;
            }
            string[] values = configured.Split(';', ',');
            for (var index = 0; index < values.Length; index++)
            {
                string value = values[index].Trim();
                if (value == runtimeKind
                    || (value == "Area"
                        && (runtimeKind == "AreaAttack"
                            || runtimeKind == "Field")))
                {
                    return true;
                }
            }
            return false;
        }

        /* MatchesTriggerAttribute 테스트 계약과 실제 값의 일치 여부를 판정한다. */
        private static bool MatchesTriggerAttribute(
            string configured,
            string attribute)
        {
            if (string.IsNullOrEmpty(configured))
            {
                return true;
            }
            string resolved = attribute;
            if (string.IsNullOrEmpty(resolved))
            {
                resolved = "Physical";
            }
            return configured.Split(';', ',')
                .Any(value => value.Trim() == resolved);
        }

        /* ReadTriggerString 검증에 필요한 실제 런타임 값을 읽어 반환한다. */
        private static string ReadTriggerString(
            SkillTriggerDefinition trigger,
            string column)
        {
            if (trigger.Columns.TryGetValue(
                    column,
                    out object value))
            {
                return value as string;
            }
            return null;
        }

        /* ReadTriggerInt 검증에 필요한 실제 런타임 값을 읽어 반환한다. */
        private static int ReadTriggerInt(
            SkillTriggerDefinition trigger,
            string column)
        {
            if (trigger.Columns.TryGetValue(
                    column,
                    out object value)
                && value is int number)
            {
                return number;
            }
            return 0;
        }

        /* NodeContractKey 시나리오의 기대 동작과 상태 변화를 검증한다. */
        private static string NodeContractKey(
            ChoiceNodeDefinition node)
        {
            return node.owner_kind
                + "|"
                + node.owner_id
                + "|"
                + node.graph_kind
                + "|"
                + (node.graph_index ?? 0)
                + "|"
                + (node.node_order ?? 0)
                + "|"
                + node.node_type_id;
        }

        /* ReadRuntimeContractState 검증에 필요한 실제 런타임 값을 읽어 반환한다. */
        private static string ReadRuntimeContractState(
            RuntimeFixture fixture,
            UnitBaseModel target)
        {
            return string.Join(
                "|",
                fixture.Selected.CurrentHealth,
                fixture.Selected.CurrentShield,
                fixture.Selected.StatusEffects.Count,
                fixture.Selected.RuntimeModifiers.Count,
                target.CurrentHealth,
                target.CurrentShield,
                target.StatusEffects.Count,
                target.RuntimeModifiers.Count,
                fixture.Actors.ActiveActors.Count,
                fixture.Actors.PendingAddCount,
                fixture.Effects.ActiveEffects.Count,
                string.Join(
                    ",",
                    fixture.Selected.SkillBucket.Cooldowns
                        .OrderBy(pair => pair.Key)
                        .Select(pair =>
                            pair.Key
                            + ":"
                            + pair.Value.RemainingCooldown
                            + ":"
                            + pair.Value.RemainingReload)));
        }

        /* ReadStringColumn 검증에 필요한 실제 런타임 값을 읽어 반환한다. */
        private static string ReadStringColumn(
            SkillDefinition definition,
            string column)
        {
            if (definition.Columns.TryGetValue(
                    column,
                    out object value))
            {
                return value as string;
            }
            return null;
        }

        /* ReadFloatColumn 검증에 필요한 실제 런타임 값을 읽어 반환한다. */
        private static float ReadFloatColumn(
            SkillDefinition definition,
            string column)
        {
            if (definition.Columns.TryGetValue(
                    column,
                    out object value)
                && value is float number)
            {
                return number;
            }
            return 0f;
        }

        /* ExecuteVegaBAndReadSilenceDuration 시나리오의 기대 동작과 상태 변화를 검증한다. */
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

        /* ExecuteVegaEAndReadDamage 시나리오의 기대 동작과 상태 변화를 검증한다. */
        private float ExecuteVegaEAndReadDamage(int nameMarkStacks)
        {
            RuntimeFixture fixture = CreateFixture("vega");
            Assert.That(
                fixture.Selected.SkillBucket.TryLearnActive(
                    catalog.GetSkill("vega-e")),
                Is.True);
            EnemyModel target = CreateEnemy("stage2-arsen");
            target.ApplyStatus(
                catalog.GetStatus("name-mark"),
                fixture.Selected,
                null,
                nameMarkStacks,
                "setup");
            fixture.Stage.TryRegisterFieldUnit(target);
            float before = target.CurrentHealth;

            Assert.That(
                fixture.Combat.TryExecuteSkill(new SkillExecutionRequest(
                    fixture.Selected,
                    catalog.GetSkill("vega-e"),
                    fixture.Stage.FieldUnits)),
                Is.True);
            return before - target.CurrentHealth;
        }

        /* LoadSources 테스트 입력 데이터를 원본 형식으로 불러온다. */
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

        private class RuntimeFixture
        {
            public MonsterModel Selected;
            public StageManager Stage;
            public EffectManager Effects;
            public SkillActorManager Actors;
            public SkillTargeting Targeting;
            public InGameCombatManager Combat;
            public SkillTriggerDispatcher Triggers;
            public SkillExecutionRuntime Execution;
            public PlayerInputController Input;
            public InGameActionManager Actions;
            public int PassiveApplyCount;
        }
    }
}
