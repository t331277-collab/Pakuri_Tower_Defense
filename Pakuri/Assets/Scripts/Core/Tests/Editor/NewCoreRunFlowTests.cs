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
using Pakuri.NewCore.Definitions.Choices;
using Pakuri.NewCore.Definitions.Skills;
using Pakuri.NewCore.Definitions.Stage;
using Pakuri.NewCore.Definitions.Units;
using Pakuri.NewCore.Run;
using Pakuri.NewCore.Run.Services;
using Pakuri.NewCore.Spawn;
using Pakuri.NewCore.Units.Models;
using UnityEngine;

namespace Pakuri.NewCore.Tests
{
    public sealed class NewCoreRunFlowTests
    {
        private GameDefinitionCatalog catalog;

        [SetUp]
        public void SetUp()
        {
            catalog = new GameBootstrap(LoadSources()).Catalog;
        }

        [Test]
        public void SpawnSequenceUsesCsvOrderIntervalBossAndPosition()
        {
            RunFixture fixture = CreateRun(
                "ariel",
                1,
                "stage1-day1-normal",
                _ => 0,
                () => 0.5f);

            fixture.Stage.StartCurrentDay();
            Assert.That(fixture.Spawns.SpawnedEnemies.Count, Is.EqualTo(1));
            Assert.That(fixture.Spawns.HasPendingSpawns, Is.True);
            Assert.That(
                fixture.Spawns.SpawnedEnemies[0].Model.MaximumHealth,
                Is.EqualTo(400f).Within(0.0001f));
            Assert.That(
                fixture.Spawns.SpawnedEnemies[0].Model.Position.X,
                Is.EqualTo(9.02f).Within(0.0001f));
            Assert.That(
                fixture.Spawns.SpawnedEnemies[0].Model.Position.Y,
                Is.EqualTo(0f).Within(0.0001f));

            fixture.Stage.TickSpawnSequence(0.99f);
            Assert.That(fixture.Spawns.SpawnedEnemies.Count, Is.EqualTo(1));
            fixture.Stage.TickSpawnSequence(0.01f);
            Assert.That(fixture.Spawns.SpawnedEnemies.Count, Is.EqualTo(2));
            fixture.Stage.TickSpawnSequence(10f);

            Assert.That(fixture.Spawns.SpawnedEnemies.Count, Is.EqualTo(4));
            Assert.That(
                fixture.Spawns.SpawnedEnemies.Count(item => item.IsBoss),
                Is.EqualTo(1));
            Assert.That(
                fixture.Stage.FieldUnits.OfType<EnemyModel>().Count(),
                Is.EqualTo(4));
        }

        [Test]
        public void PhaseThreeDefeatSignalEntersRewardAndGrantsRunRewards()
        {
            RunFixture fixture = CreateRun(
                "ariel",
                1,
                "stage1-day1-normal",
                _ => 0,
                () => 0.5f);
            InGameCombatManager combat = CreateCombat(() => 1f);
            fixture.Stage.ConnectCombat(combat, new NexusModel(100f));
            fixture.Stage.StartCurrentDay();
            fixture.Stage.TickSpawnSequence(10f);
            combat.NotifyCombatStart(
                fixture.InitialMonster,
                fixture.Stage.FieldUnits);

            foreach (SpawnedEnemyRecord record
                in fixture.Spawns.SpawnedEnemies)
            {
                combat.ApplyTriggeredDamage(
                    fixture.InitialMonster,
                    record.Model,
                    "phase4-test",
                    "Physical",
                    record.Model.MaximumHealth * 10f,
                    0f,
                    0f,
                    1f);
            }

            Assert.That(fixture.Stage.IsCombatActive, Is.False);
            Assert.That(
                fixture.Stage.Session.RewardState,
                Is.EqualTo(RewardProcessingState.Pending));

            RewardService rewards = new RewardService(
                catalog,
                _ => 0,
                () => 0.1f);
            RewardResult result = rewards.GenerateAndGrant(
                fixture.Stage,
                fixture.Spawns);

            Assert.That(result.Gold, Is.EqualTo(10));
            Assert.That(result.DarkTrace, Is.EqualTo(10));
            Assert.That(result.PrisonerEnemyIds.Count, Is.EqualTo(2));
            Assert.That(
                result.PrisonerEnemyIds[0],
                Is.EqualTo("stage1-swordsman"));
            Assert.That(fixture.Stage.Gold, Is.EqualTo(10));
            Assert.That(fixture.Stage.DarkTrace, Is.EqualTo(10));
            Assert.That(
                fixture.Stage.Session.PrisonerInventory.Prisoners.Count,
                Is.EqualTo(2));
            Assert.That(
                fixture.Stage.Session.RewardState,
                Is.EqualTo(RewardProcessingState.Processing));
        }

        [Test]
        public void EncounterBossRewardUsesTheActuallySelectedBoss()
        {
            RunFixture fixture = CreateRun(
                "ariel",
                1,
                "stage1-day1-normal",
                count => count > 1 ? 1 : 0,
                () => 0.5f);
            fixture.Stage.StartCurrentDay();
            fixture.Stage.TickSpawnSequence(10f);
            DefeatAllSpawned(fixture);
            fixture.Stage.EvaluateCombatCompletion();

            RewardResult result = new RewardService(
                catalog,
                _ => 0,
                () => 0.1f)
                .GenerateAndGrant(fixture.Stage, fixture.Spawns);

            Assert.That(
                fixture.Spawns.SpawnedEnemies.Single(item => item.IsBoss)
                    .Model.EnemyDefinition.enemy_id,
                Is.EqualTo("stage1-shieldbearer"));
            Assert.That(
                result.PrisonerEnemyIds[0],
                Is.EqualTo("stage1-shieldbearer"));
        }

        [Test]
        public void StartCurrentDayRejectsActiveAndRewardStateReentry()
        {
            RunFixture active = CreateRun(
                "ariel",
                1,
                "stage1-day1-normal",
                _ => 0,
                () => 0.5f);
            active.Stage.StartCurrentDay();
            UnitBaseModel activeFirst = active.Stage.FieldUnits[1];
            Assert.Throws<InvalidOperationException>(
                () => active.Stage.StartCurrentDay());
            Assert.That(active.Stage.IsCombatActive, Is.True);
            Assert.That(active.Stage.FieldUnits[1], Is.SameAs(activeFirst));

            RunFixture pending = CreateRun(
                "ariel",
                1,
                "stage1-day1-normal",
                _ => 0,
                () => 0.5f);
            pending.Stage.StartCurrentDay();
            pending.Stage.TickSpawnSequence(10f);
            DefeatAllSpawned(pending);
            pending.Stage.EvaluateCombatCompletion();
            Assert.Throws<InvalidOperationException>(
                () => pending.Stage.StartCurrentDay());
            Assert.That(
                pending.Stage.Session.RewardState,
                Is.EqualTo(RewardProcessingState.Pending));

            new RewardService(catalog, _ => 0, () => 0.1f)
                .GenerateAndGrant(pending.Stage, pending.Spawns);
            int gold = pending.Stage.Gold;
            Assert.Throws<InvalidOperationException>(
                () => pending.Stage.StartCurrentDay());
            Assert.That(
                pending.Stage.Session.RewardState,
                Is.EqualTo(RewardProcessingState.Processing));
            Assert.That(pending.Stage.Gold, Is.EqualTo(gold));
        }

        [Test]
        public void RewardRejectsAForeignSpawnManagerWithoutMutation()
        {
            RunFixture fixture = CreateRun(
                "ariel",
                1,
                "stage1-day1-normal",
                _ => 0,
                () => 0.5f);
            fixture.Stage.StartCurrentDay();
            fixture.Stage.TickSpawnSequence(10f);
            DefeatAllSpawned(fixture);
            fixture.Stage.EvaluateCombatCompletion();
            SpawnManager foreign = new SpawnManager(
                catalog,
                _ => 0,
                () => 0.5f);

            Assert.Throws<InvalidOperationException>(() =>
                new RewardService(catalog, _ => 0, () => 0.1f)
                    .GenerateAndGrant(fixture.Stage, foreign));
            Assert.That(fixture.Stage.Gold, Is.Zero);
            Assert.That(fixture.Stage.DarkTrace, Is.Zero);
            Assert.That(
                fixture.Stage.Session.PrisonerInventory.Prisoners,
                Is.Empty);
            Assert.That(
                fixture.Stage.Session.RewardState,
                Is.EqualTo(RewardProcessingState.Pending));
        }

        [Test]
        public void OfferingKeepsPrisonerUntilAVisibleCandidateIsConfirmed()
        {
            RunFixture fixture = CreateRun(
                "ariel",
                1,
                "stage1-day1-normal",
                _ => 0,
                () => 0.5f);
            Prisoner prisoner =
                fixture.Stage.Session.PrisonerInventory.Register(
                    "stage1-swordsman");
            OfferingService offering = new OfferingService(
                catalog,
                fixture.Stage,
                _ => 0);
            int before = LearnedCount(fixture.InitialMonster);

            OfferingOffer offer = offering.GenerateCandidates(
                fixture.InitialMonster,
                prisoner);

            Assert.That(offer.Candidates.Count, Is.EqualTo(3));
            Assert.That(
                fixture.Stage.Session.PrisonerInventory.CanConsume(
                    prisoner),
                Is.True);
            Assert.That(
                offering.TryConfirm(offer.Candidates[0].Id),
                Is.True);
            Assert.That(
                fixture.Stage.Session.PrisonerInventory.CanConsume(
                    prisoner),
                Is.False);
            Assert.That(
                LearnedCount(fixture.InitialMonster),
                Is.GreaterThan(before));
        }

        [Test]
        public void OfferingWithNoEligibleCandidatesDoesNotConsumePrisoner()
        {
            RunFixture fixture = CreateRun(
                "ariel",
                1,
                "stage1-day1-normal",
                _ => 0,
                () => 0.5f);
            ExhaustLearning(fixture.InitialMonster);
            Prisoner prisoner =
                fixture.Stage.Session.PrisonerInventory.Register(
                    "stage1-swordsman");
            OfferingService offering = new OfferingService(
                catalog,
                fixture.Stage,
                _ => 0);

            OfferingOffer offer = offering.GenerateCandidates(
                fixture.InitialMonster,
                prisoner);

            Assert.That(offer.Candidates, Is.Empty);
            Assert.That(offering.PendingOffer, Is.Null);
            Assert.That(
                fixture.Stage.Session.PrisonerInventory.CanConsume(
                    prisoner),
                Is.True);
        }

        [Test]
        public void ManifestationConsumesOnAttemptThenSupportsSkipAndRecruit()
        {
            RunFixture fixture = CreateRun(
                "ariel",
                1,
                "stage1-day1-normal",
                _ => 0,
                () => 0.5f);
            StageRewardDefinition reward =
                catalog.StageRewards["reward-stage1-normal"];

            Prisoner failed =
                fixture.Stage.Session.PrisonerInventory.Register(
                    "stage1-swordsman");
            ManifestationService failureService =
                new ManifestationService(
                    catalog,
                    fixture.Stage,
                    fixture.Spawns,
                    _ => 0,
                    () => 0.9f);
            ManifestationAttemptResult failure =
                failureService.BeginAttempt(failed, reward);
            Assert.That(failure.Success, Is.False);
            Assert.That(
                fixture.Stage.Session.PrisonerInventory.CanConsume(failed),
                Is.False);

            Prisoner skipped =
                fixture.Stage.Session.PrisonerInventory.Register(
                    "stage1-rogue");
            ManifestationService skipService =
                new ManifestationService(
                    catalog,
                    fixture.Stage,
                    fixture.Spawns,
                    _ => 0,
                    () => 0.1f);
            ManifestationAttemptResult success =
                skipService.BeginAttempt(skipped, reward);
            Assert.That(success.Success, Is.True);
            Assert.That(skipService.SkipRecruitment(), Is.True);
            Assert.That(
                fixture.Stage.Session.PartyRoster.Members.Count,
                Is.EqualTo(1));

            Prisoner recruited =
                fixture.Stage.Session.PrisonerInventory.Register(
                    "stage1-priest");
            ManifestationService recruitService =
                new ManifestationService(
                    catalog,
                    fixture.Stage,
                    fixture.Spawns,
                    _ => 0,
                    () => 0.1f);
            Assert.That(
                recruitService.BeginAttempt(recruited, reward).Success,
                Is.True);
            MonsterModel manifested =
                recruitService.ConfirmRecruitment();

            Assert.That(
                fixture.Stage.Session.PartyRoster.Members.Count,
                Is.EqualTo(2));
            Assert.That(
                fixture.Stage.Session.PartyRoster.Members[1],
                Is.SameAs(manifested));
            Assert.That(
                fixture.Stage.FieldUnits.Contains(manifested),
                Is.True);
            Assert.That(manifested.AutoSkillEnabled, Is.True);
        }

        [Test]
        public void RewardCompletionClearsPrisonersResetsPartyAndStartsNextDay()
        {
            RunFixture fixture = CreateRun(
                "ariel",
                1,
                "stage1-day1-normal",
                _ => 0,
                () => 0.5f);
            fixture.Stage.StartCurrentDay();
            fixture.Stage.TickSpawnSequence(10f);
            DefeatAllSpawned(fixture);
            fixture.Stage.EvaluateCombatCompletion();
            RewardService rewards = new RewardService(
                catalog,
                _ => 0,
                () => 0.1f);
            rewards.GenerateAndGrant(fixture.Stage, fixture.Spawns);
            fixture.InitialMonster.ApplyDamage(10f);
            fixture.InitialMonster.TryAddShield(5f);

            Assert.That(
                fixture.Stage.CompleteRewardAndAdvance(),
                Is.True);

            Assert.That(fixture.Stage.Session.CurrentDay, Is.EqualTo(2));
            Assert.That(
                fixture.Stage.Session.CurrentEncounterId,
                Is.EqualTo("stage1-day2-normal"));
            Assert.That(
                fixture.Stage.Session.PrisonerInventory.Prisoners,
                Is.Empty);
            Assert.That(
                fixture.InitialMonster.CurrentHealth,
                Is.EqualTo(fixture.InitialMonster.MaximumHealth));
            Assert.That(fixture.InitialMonster.CurrentShield, Is.Zero);
            Assert.That(fixture.InitialMonster.AutoSkillEnabled, Is.False);
            Assert.That(fixture.Stage.IsCombatActive, Is.True);
            Assert.That(fixture.Spawns.SpawnedEnemies.Count, Is.EqualTo(1));
        }

        [Test]
        public void ActionLifecycleClearsEffectsBeforeStartingTheNextDay()
        {
            RunFixture fixture = CreateRun(
                "sein",
                1,
                "stage1-day1-normal",
                _ => 0,
                () => 0.5f);
            var effects = new EffectManager();
            var actors = new SkillActorManager(effects);
            var targeting = new SkillTargeting(_ => 0);
            var execution = new SkillExecutionRuntime(
                catalog,
                targeting,
                actors,
                effects,
                () => 1f);
            var combat = new InGameCombatManager(
                () => 1f,
                execution);
            var input = new PlayerInputController();
            var actions = new InGameActionManager(
                fixture.Stage,
                () => fixture.Stage.IsCombatActive,
                () => { },
                input,
                actors,
                execution.Triggers,
                combat);
            actions.RegisterMonster(
                new MonsterActionController(
                    fixture.InitialMonster,
                    combat),
                true);
            fixture.Stage.ConnectCombat(
                combat,
                new NexusModel(100f));
            bool resolutionPending = false;
            fixture.Stage.CombatResolved += _ =>
                resolutionPending = true;

            fixture.Stage.StartCurrentDay();
            actions.BeginOrExtendCombat(
                fixture.Stage.FieldUnits);
            fixture.Stage.TickSpawnSequence(10f);
            actions.BeginOrExtendCombat(
                fixture.Stage.FieldUnits);
            var records = fixture.Spawns.SpawnedEnemies;
            for (int index = 1; index < records.Count; index++)
            {
                combat.ApplyTriggeredDamage(
                    fixture.InitialMonster,
                    records[index].Model,
                    "lifecycle-test",
                    "Physical",
                    records[index].Model.MaximumHealth * 10f,
                    0f,
                    0f,
                    1f);
            }

            Assert.That(
                fixture.InitialMonster.SkillBucket.TryLearnActive(
                    catalog.GetSkill("sein-c")),
                Is.True);
            var finalEnemy = records[0].Model;
            finalEnemy.ApplyDamage(
                finalEnemy.MaximumHealth - 1f);
            Assert.That(
                combat.TryExecuteSkill(
                    new SkillExecutionRequest(
                        fixture.InitialMonster,
                        catalog.GetSkill("sein-c"),
                        fixture.Stage.FieldUnits)),
                Is.True);
            for (int tick = 0;
                tick < 100 && !resolutionPending;
                tick++)
            {
                actors.Tick(0.1f);
            }

            Assert.That(resolutionPending, Is.True);
            Assert.That(effects.ActiveEffects, Is.Not.Empty);
            actions.EndCombat();
            Assert.That(fixture.Stage.IsCombatActive, Is.False);
            Assert.That(actors.ActiveActors, Is.Empty);
            Assert.That(actors.PendingAddCount, Is.Zero);
            Assert.That(effects.ActiveEffects, Is.Empty);
            new RewardService(
                    catalog,
                    _ => 0,
                    () => 0.1f)
                .GenerateAndGrant(
                    fixture.Stage,
                    fixture.Spawns);

            Assert.That(
                fixture.Stage.CompleteRewardAndAdvance(),
                Is.True);
            input.SetAutoSkillEnabled(true);
            actions.BeginOrExtendCombat(
                fixture.Stage.FieldUnits);

            Assert.That(
                fixture.Stage.Session.CurrentDay,
                Is.EqualTo(2));
            Assert.That(fixture.Stage.IsCombatActive, Is.True);
            Assert.That(
                fixture.InitialMonster.AutoSkillEnabled,
                Is.True);
            Assert.That(effects.ActiveEffects, Is.Empty);
        }

        [Test]
        public void FinalRewardCompletesRunAndNexusDeathSignalsDefeat()
        {
            RunFixture victory = CreateRun(
                "ariel",
                11,
                "stage2-day11-boss",
                _ => 0,
                () => 0.5f);
            victory.Stage.StartCurrentDay();
            victory.Stage.TickSpawnSequence(10f);
            DefeatAllSpawned(victory);
            victory.Stage.EvaluateCombatCompletion();
            new RewardService(catalog, _ => 0, () => 0.1f)
                .GenerateAndGrant(victory.Stage, victory.Spawns);

            Assert.That(
                victory.Stage.CompleteRewardAndAdvance(),
                Is.False);
            Assert.That(
                victory.Stage.Session.Result,
                Is.EqualTo(RunResult.Victory));

            RunFixture defeat = CreateRun(
                "ariel",
                1,
                "stage1-day1-normal",
                _ => 0,
                () => 0.5f);
            InGameCombatManager combat = CreateCombat(() => 1f);
            NexusModel nexus = new NexusModel(1f);
            defeat.Stage.ConnectCombat(combat, nexus);
            defeat.Stage.StartCurrentDay();
            combat.ApplyNexusDamage(
                defeat.Spawns.SpawnedEnemies[0].Model,
                nexus);

            Assert.That(
                defeat.Stage.Session.Result,
                Is.EqualTo(RunResult.Defeat));
            Assert.That(defeat.Stage.IsCombatActive, Is.False);
        }

        private RunFixture CreateRun(
            string monsterId,
            int day,
            string encounterId,
            Func<int, int> randomIndex,
            Func<float> randomValue)
        {
            MonsterModel initial = CreateMonster(monsterId, false);
            string stageId = encounterId.StartsWith(
                    "stage2-",
                    StringComparison.Ordinal)
                ? "stage2"
                : "stage1";
            RunSessionModel session = new RunSessionModel(
                stageId,
                day,
                encounterId,
                new PartyRoster(initial),
                new PrisonerInventory());
            StageManager stage = new StageManager(
                session,
                catalog,
                0,
                0);
            SpawnManager spawns = new SpawnManager(
                catalog,
                randomIndex,
                randomValue);
            stage.ConfigureSpawnManager(spawns);
            return new RunFixture
            {
                InitialMonster = initial,
                Stage = stage,
                Spawns = spawns
            };
        }

        private InGameCombatManager CreateCombat(
            Func<float> randomValue)
        {
            EffectManager effects = new EffectManager();
            SkillActorManager actors = new SkillActorManager(effects);
            SkillExecutionRuntime execution = new SkillExecutionRuntime(
                catalog,
                new SkillTargeting(_ => 0),
                actors,
                effects,
                randomValue);
            return new InGameCombatManager(randomValue, execution);
        }

        private MonsterModel CreateMonster(
            string monsterId,
            bool autoSkill)
        {
            MonsterDefinition definition =
                catalog.GetMonster(monsterId);
            return new MonsterModel(
                definition,
                catalog.GetSkill(monsterId + "-a"),
                catalog.Choices.Values.Where(choice =>
                    choice.monster_id == monsterId
                    && choice.choice_group == "PassiveBase"),
                autoSkill);
        }

        private static void DefeatAllSpawned(RunFixture fixture)
        {
            foreach (SpawnedEnemyRecord record
                in fixture.Spawns.SpawnedEnemies)
            {
                record.Model.ApplyDamage(record.Model.MaximumHealth);
            }
        }

        private void ExhaustLearning(MonsterModel monster)
        {
            bool changed;
            do
            {
                changed = false;
                foreach (SkillDefinition skill in catalog.Skills.Values)
                {
                    if (skill.monster_id
                        != monster.MonsterDefinition.id)
                    {
                        continue;
                    }

                    changed |= skill is PassiveDefinition passive
                        ? monster.SkillBucket.TryLearnPassive(passive)
                        : monster.SkillBucket.TryLearnActive(skill);
                }

                foreach (SkillChoiceDefinition choice
                    in catalog.Choices.Values)
                {
                    if (choice.monster_id
                        == monster.MonsterDefinition.id)
                    {
                        changed |= monster.SkillBucket
                            .TrySelectChoice(choice);
                    }
                }
            }
            while (changed);
        }

        private static int LearnedCount(MonsterModel monster)
        {
            return monster.SkillBucket.ActiveSkills.Count
                + monster.SkillBucket.PassiveSkills.Count
                + monster.SkillBucket.SelectedChoices.Count;
        }

        private static Dictionary<string, string> LoadSources()
        {
            string csvRoot = Path.Combine(
                Application.dataPath,
                "CSVdata");
            string[] files = Directory.GetFiles(
                csvRoot,
                "*.csv",
                SearchOption.AllDirectories);
            Dictionary<string, string> sources =
                new Dictionary<string, string>(
                    StringComparer.Ordinal);
            UTF8Encoding strictUtf8 =
                new UTF8Encoding(false, true);

            foreach (string file in files)
            {
                string relative =
                    file.Substring(Application.dataPath.Length)
                        .TrimStart(
                            Path.DirectorySeparatorChar,
                            Path.AltDirectorySeparatorChar)
                        .Replace('\\', '/');
                sources.Add(
                    "Assets/" + relative,
                    File.ReadAllText(file, strictUtf8));
            }

            Assert.That(files.Length, Is.EqualTo(42));
            return sources;
        }

        private sealed class RunFixture
        {
            public MonsterModel InitialMonster { get; set; }

            public StageManager Stage { get; set; }

            public SpawnManager Spawns { get; set; }
        }
    }
}
