using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using NUnit.Framework;
using Pakuri.NewCore.Bootstrap;
using Pakuri.NewCore.Catalog;
using Pakuri.NewCore.Combat.Skills.Runtime;
using Pakuri.NewCore.Combat.Status;
using Pakuri.NewCore.Definitions.Choices;
using Pakuri.NewCore.Definitions.Skills;
using Pakuri.NewCore.Definitions.Units;
using Pakuri.NewCore.Parsing;
using Pakuri.NewCore.Run;
using Pakuri.NewCore.Units.Models;
using UnityEngine;

/* NewCore 유닛·스킬·run 런타임 상태의 불변 조건을 검증한다. */
namespace Pakuri.NewCore.Tests
{
    public class NewCoreRuntimeStateTests
    {
        private GameDefinitionCatalog catalog;

        [SetUp]
        /* 각 테스트가 독립적으로 실행되도록 임시 객체와 공유 상태를 초기화한다. */
        public void SetUp()
        {
            catalog = GameBootstrap.CreateCatalog(LoadSources());
        }

        [Test]
        /* StageManagerIsTheOnlyPublicCurrencyAndFieldUnitWriter 시나리오의 기대 동작과 상태 변화를 검증한다. */
        public void StageManagerIsTheOnlyPublicCurrencyAndFieldUnitWriter()
        {
            MonsterModel monster = CreateMonster("ariel", false);
            RunSessionModel session = CreateSession(monster);
            StageManager stage =
                NewCoreTestFactory.CreateStageManager(session, 10, 4);

            Assert.That(stage.Gold, Is.EqualTo(10));
            stage.AddGold(5);
            Assert.That(stage.SpendGold(12), Is.True);
            Assert.That(stage.Gold, Is.EqualTo(3));
            Assert.That(stage.SpendGold(4), Is.False);
            Assert.That(stage.CanSpendGold(-1), Is.False);

            stage.AddDarkTrace(6);
            Assert.That(stage.SpendDarkTrace(9), Is.True);
            Assert.That(stage.DarkTrace, Is.EqualTo(1));
            Assert.That(stage.TryRegisterFieldUnit(monster), Is.True);
            Assert.That(stage.TryRegisterFieldUnit(monster), Is.False);
            Assert.That(stage.LivingFieldUnits, Is.EqualTo(new[] { monster }));

            monster.ApplyDamage(monster.MaximumHealth);
            Assert.That(stage.FieldUnits, Is.EqualTo(new[] { monster }));
            Assert.That(stage.LivingFieldUnits, Is.Empty);
            Assert.That(stage.TryUnregisterFieldUnit(monster), Is.True);
        }

        [Test]
        /* PartyRosterPreservesOrderAndRejectsDuplicatesAtFiveSlots 시나리오의 기대 동작과 상태 변화를 검증한다. */
        public void PartyRosterPreservesOrderAndRejectsDuplicatesAtFiveSlots()
        {
            string[] ids = { "ariel", "eve", "rin", "sein", "vega" };
            MonsterModel[] monsters = ids.Select(id => CreateMonster(id, true)).ToArray();
            PartyRoster roster = new PartyRoster(monsters[0]);

            Assert.That(roster.TryAddManifestedMonster(monsters[0]), Is.False);
            for (int index = 1; index < monsters.Length; index++)
            {
                Assert.That(roster.TryAddManifestedMonster(monsters[index]), Is.True);
            }

            Assert.That(roster.Members.Count, Is.EqualTo(PartyRoster.MaximumPartySlots));
            Assert.That(
                roster.Members.Select(item => item.MonsterDefinition.id),
                Is.EqualTo(ids));
            Assert.That(roster.GetByMonsterId("sein"), Is.SameAs(monsters[3]));
            Assert.That(roster.CanAdd("not-in-catalog"), Is.False);
        }

        [Test]
        /* PrisonerInventoryConsumesExactEntriesAndClearsBetweenRewards 시나리오의 기대 동작과 상태 변화를 검증한다. */
        public void PrisonerInventoryConsumesExactEntriesAndClearsBetweenRewards()
        {
            PrisonerInventory inventory = new PrisonerInventory();
            Prisoner first = inventory.Register("stage1-swordsman");
            Prisoner second = inventory.Register("stage1-swordsman");

            Assert.That(inventory.CanConsume(first), Is.True);
            Assert.That(inventory.TryConsume(first), Is.True);
            Assert.That(inventory.TryConsume(first), Is.False);
            Assert.That(inventory.CanConsume(second), Is.True);

            inventory.ReplaceRewards(new[] { "stage1-archer", "stage1-priest" });
            Assert.That(inventory.CanConsume(second), Is.False);
            Assert.That(
                inventory.Prisoners.Select(item => item.EnemyId),
                Is.EqualTo(new[] { "stage1-archer", "stage1-priest" }));

            inventory.Clear();
            Assert.That(inventory.Prisoners, Is.Empty);
        }

        [Test]
        /* UnitOwnsHealthShieldStatusSurvivalAndRoundReset 시나리오의 기대 동작과 상태 변화를 검증한다. */
        public void UnitOwnsHealthShieldStatusSurvivalAndRoundReset()
        {
            MonsterModel target = CreateMonster("ariel", false);
            EnemyModel source = CreateEnemy("stage1-swordsman");

            Assert.That(target.TryAddShield(20f), Is.True);
            Assert.That(target.ApplyDamage(30f), Is.EqualTo(10f));
            Assert.That(target.CurrentShield, Is.Zero);
            Assert.That(target.CurrentHealth, Is.EqualTo(target.MaximumHealth - 10f));
            Assert.That(target.Heal(50f), Is.EqualTo(10f));

            StatusEffect shock =
                target.ApplyStatus(catalog.GetStatus("shock"), source, null, 3);
            Assert.That(shock.CurrentStacks, Is.EqualTo(3));
            Assert.That(
                target.ApplyStatus(catalog.GetStatus("shock"), source, null, 4),
                Is.SameAs(shock));
            Assert.That(shock.CurrentStacks, Is.EqualTo(5));
            target.TickStatusEffects(3f);
            Assert.That(target.StatusEffects.Count, Is.EqualTo(1));
            target.TickStatusEffects(1f);
            Assert.That(target.StatusEffects, Is.Empty);

            target.TryAddShield(5f);
            target.ApplyDamage(target.MaximumHealth + 5f);
            Assert.That(target.IsAlive, Is.False);
            Assert.That(target.Heal(10f), Is.Zero);
            target.SetAutoAttackEnabled(false);
            target.SetAutoSkillEnabled(false);
            target.ResetForNextDay(false);

            Assert.That(target.CurrentHealth, Is.EqualTo(target.MaximumHealth));
            Assert.That(target.CurrentShield, Is.Zero);
            Assert.That(target.IsAlive, Is.True);
            Assert.That(target.AutoAttackEnabled, Is.True);
            Assert.That(target.AutoSkillEnabled, Is.True);
        }

        [Test]
        /* PermanentStatusDoesNotExpireAndExplicitRemovalWorks 시나리오의 기대 동작과 상태 변화를 검증한다. */
        public void PermanentStatusDoesNotExpireAndExplicitRemovalWorks()
        {
            MonsterModel target = CreateMonster("eve", true);
            StatusEffect effect =
                target.ApplyStatus(catalog.GetStatus("vulnerable"), target);

            target.TickStatusEffects(1000f);

            Assert.That(effect.IsPermanent, Is.True);
            Assert.That(effect.RemainingDuration, Is.Null);
            Assert.That(target.StatusEffects, Is.EqualTo(new[] { effect }));
            Assert.That(target.RemoveStatus(effect), Is.True);
            Assert.That(target.RemoveStatus(effect), Is.False);
        }

        [Test]
        /* MagazineCooldownEnforcesShotIntervalReloadAndReset 시나리오의 기대 동작과 상태 변화를 검증한다. */
        public void MagazineCooldownEnforcesShotIntervalReloadAndReset()
        {
            SkillCooldown cooldown = new SkillCooldown(catalog.GetSkill("ariel-a"));

            Assert.That(cooldown.CurrentMagazine, Is.EqualTo(7));
            for (int shot = 0; shot < 7; shot++)
            {
                Assert.That(cooldown.TryUse(), Is.True);
                if (shot < 6)
                {
                    Assert.That(cooldown.TryUse(), Is.False);
                    cooldown.Tick(0.36f);
                }
            }

            Assert.That(cooldown.CurrentMagazine, Is.Zero);
            Assert.That(cooldown.IsReloading, Is.True);
            Assert.That(cooldown.CanUse(), Is.False);
            cooldown.Tick(4.59f);
            Assert.That(cooldown.IsReloading, Is.True);
            cooldown.Tick(0.01f);
            Assert.That(cooldown.CurrentMagazine, Is.EqualTo(7));
            Assert.That(cooldown.CanUse(), Is.True);

            cooldown.TryUse();
            cooldown.ResetForNextRound();
            Assert.That(cooldown.CurrentMagazine, Is.EqualTo(7));
            Assert.That(cooldown.RemainingCooldown, Is.Zero);
            Assert.That(cooldown.RemainingReload, Is.Zero);
            Assert.That(cooldown.RemainingShotInterval, Is.Zero);
        }

        [Test]
        /* NonMagazineCooldownBlocksUntilDurationElapses 시나리오의 기대 동작과 상태 변화를 검증한다. */
        public void NonMagazineCooldownBlocksUntilDurationElapses()
        {
            SkillCooldown cooldown = new SkillCooldown(catalog.GetSkill("sein-c"));

            Assert.That(cooldown.CurrentMagazine, Is.Null);
            Assert.That(cooldown.TryUse(), Is.True);
            Assert.That(cooldown.CanUse(), Is.False);
            cooldown.Tick(6.49f);
            Assert.That(cooldown.CanUse(), Is.False);
            cooldown.Tick(0.01f);
            Assert.That(cooldown.CanUse(), Is.True);
        }

        [Test]
        /* MonsterSkillBucketEnforcesLearningAndChoiceLimits 시나리오의 기대 동작과 상태 변화를 검증한다. */
        public void MonsterSkillBucketEnforcesLearningAndChoiceLimits()
        {
            MonsterModel monster = CreateMonster("ariel", false);
            MonsterSkillBucket bucket = monster.SkillBucket;

            Assert.That(bucket.TryLearnActive(catalog.GetSkill("ariel-b")), Is.True);
            Assert.That(bucket.TryLearnActive(catalog.GetSkill("ariel-c")), Is.True);
            Assert.That(bucket.TryLearnActive(catalog.GetSkill("ariel-d")), Is.False);
            Assert.That(bucket.TryLearnActive(catalog.GetSkill("ariel-b")), Is.False);
            Assert.That(bucket.TryLearnActive(catalog.GetSkill("eve-b")), Is.False);

            for (char slot = 'f'; slot <= 'h'; slot++)
            {
                Assert.That(
                    bucket.TryLearnPassive(
                        (PassiveDefinition)catalog.GetSkill($"ariel-{slot}")),
                    Is.True);
            }
            Assert.That(
                bucket.TryLearnPassive(
                    (PassiveDefinition)catalog.GetSkill("ariel-i")),
                Is.False);
            Assert.That(
                bucket.TryLearnPassive(
                    (PassiveDefinition)catalog.GetSkill("ariel-j")),
                Is.False);

            Assert.That(
                bucket.TryLearnPassive((PassiveDefinition)catalog.GetSkill("ariel-f")),
                Is.False);
            Assert.That(
                bucket.TryLearnPassive((PassiveDefinition)catalog.GetSkill("eve-f")),
                Is.False);

            SkillChoiceDefinition master = catalog.GetChoice("ariel-a-master-1");
            Assert.That(bucket.TrySelectChoice(master), Is.False);
            Assert.That(
                bucket.TrySelectChoice(catalog.GetChoice("ariel-a-trait-1")),
                Is.True);
            Assert.That(
                bucket.TrySelectChoice(catalog.GetChoice("ariel-a-trait-2")),
                Is.True);
            Assert.That(
                bucket.TrySelectChoice(catalog.GetChoice("ariel-a-trait-3")),
                Is.True);
            Assert.That(
                bucket.TrySelectChoice(catalog.GetChoice("ariel-a-trait-4")),
                Is.False);
            Assert.That(bucket.TrySelectChoice(master), Is.True);
            Assert.That(
                bucket.TrySelectChoice(catalog.GetChoice("ariel-a-master-2")),
                Is.False);
            Assert.That(bucket.TrySelectChoice(master), Is.False);

            Assert.That(
                bucket.TrySelectChoice(catalog.GetChoice("ariel-f-trait-1")),
                Is.True);
            Assert.That(
                bucket.TrySelectChoice(catalog.GetChoice("ariel-f-trait-2")),
                Is.True);
            Assert.That(
                bucket.TrySelectChoice(catalog.GetChoice("ariel-f-trait-3")),
                Is.True);
            Assert.That(
                bucket.SelectedChoices.Count(choice =>
                    choice.skill_id == "ariel-f"
                    && choice.choice_group
                        == "PassiveEnhancement"),
                Is.EqualTo(
                    MonsterSkillBucket
                        .MaximumPassiveEnhancementsPerSkill));
            Assert.That(
                bucket.TrySelectChoice(catalog.GetChoice("ariel-f-trait-1")),
                Is.False);
        }

        [Test]
        /* PassiveSlotsRequireTheirPairedLearnedActiveSlots 시나리오의 기대 동작과 상태 변화를 검증한다. */
        public void PassiveSlotsRequireTheirPairedLearnedActiveSlots()
        {
            MonsterSkillBucket bucket =
                CreateMonster("ariel", false).SkillBucket;
            Assert.That(
                bucket.CanLearnPassive(
                    (PassiveDefinition)catalog.GetSkill("ariel-f")),
                Is.True);
            Assert.That(
                bucket.CanLearnPassive(
                    (PassiveDefinition)catalog.GetSkill("ariel-g")),
                Is.False);
            Assert.That(
                bucket.TryLearnActive(catalog.GetSkill("ariel-b")),
                Is.True);
            Assert.That(
                bucket.CanLearnPassive(
                    (PassiveDefinition)catalog.GetSkill("ariel-g")),
                Is.True);

            MonsterSkillBucket slotD =
                CreateMonster("ariel", false).SkillBucket;
            Assert.That(
                slotD.TryLearnActive(catalog.GetSkill("ariel-d")),
                Is.True);
            Assert.That(
                slotD.CanLearnPassive(
                    (PassiveDefinition)catalog.GetSkill("ariel-i")),
                Is.True);
            Assert.That(
                slotD.CanLearnPassive(
                    (PassiveDefinition)catalog.GetSkill("ariel-j")),
                Is.False);

            MonsterSkillBucket slotE =
                CreateMonster("ariel", false).SkillBucket;
            Assert.That(
                slotE.TryLearnActive(catalog.GetSkill("ariel-e")),
                Is.True);
            Assert.That(
                slotE.CanLearnPassive(
                    (PassiveDefinition)catalog.GetSkill("ariel-j")),
                Is.True);
            Assert.That(
                slotE.CanLearnPassive(
                    (PassiveDefinition)catalog.GetSkill("eve-j")),
                Is.False);
        }

        [Test]
        /* PassiveBasePrerequisiteBlocksLearningAndSelectionUntilActiveIsLearned 시나리오의 기대 동작과 상태 변화를 검증한다. */
        public void PassiveBasePrerequisiteBlocksLearningAndSelectionUntilActiveIsLearned()
        {
            MonsterModel monster = CreateMonster("sein", false);
            MonsterSkillBucket bucket = monster.SkillBucket;
            PassiveDefinition passive =
                (PassiveDefinition)catalog.GetSkill("sein-i");
            SkillChoiceDefinition passiveBase =
                catalog.GetChoice("sein-i-base-shot-interval");

            Assert.That(passiveBase.target_skill_id, Is.EqualTo("sein-d"));
            Assert.That(bucket.CanLearnPassive(passive), Is.False);
            Assert.That(bucket.TryLearnPassive(passive), Is.False);
            Assert.That(bucket.CanSelectChoice(passiveBase), Is.False);
            Assert.That(bucket.TrySelectChoice(passiveBase), Is.False);

            Assert.That(bucket.TryLearnActive(catalog.GetSkill("sein-d")), Is.True);
            Assert.That(bucket.CanLearnPassive(passive), Is.True);
            Assert.That(bucket.TryLearnPassive(passive), Is.True);
            Assert.That(bucket.CanSelectChoice(passiveBase), Is.True);
            Assert.That(bucket.TrySelectChoice(passiveBase), Is.True);
            Assert.That(bucket.CanSelectChoice(passiveBase), Is.False);
            Assert.That(bucket.TrySelectChoice(passiveBase), Is.False);
        }

        [Test]
        /* EnemyBucketPreservesTwoSlotsAndSharesDuplicateSkillRuntime 시나리오의 기대 동작과 상태 변화를 검증한다. */
        public void EnemyBucketPreservesTwoSlotsAndSharesDuplicateSkillRuntime()
        {
            EnemyModel enemy = CreateEnemy("stage1-swordsman");

            Assert.That(enemy.SkillBucket.ActiveSkills.Count, Is.EqualTo(2));
            Assert.That(enemy.SkillBucket.SlotASkill.skill_id, Is.EqualTo("Slash"));
            Assert.That(enemy.SkillBucket.SlotBSkill.skill_id, Is.EqualTo("Slash"));
            Assert.That(enemy.SkillBucket.Cooldowns.Count, Is.EqualTo(1));
            Assert.That(
                enemy.SkillBucket.GetCooldown("Slash"),
                Is.SameAs(enemy.SkillBucket.Cooldowns["Slash"]));
            Assert.That(enemy.SkillBucket.PassiveSkills.Count, Is.EqualTo(1));

            enemy.MarkNexusContact();
            enemy.SetAutoAttackEnabled(false);
            enemy.ApplyDamage(10f);
            enemy.ResetForNextDay();
            Assert.That(enemy.HasContactedNexus, Is.False);
            Assert.That(enemy.AutoAttackEnabled, Is.True);
            Assert.That(enemy.CurrentHealth, Is.EqualTo(enemy.MaximumHealth));
        }

        [Test]
        /* NexusUsesTheCommonHealthAuthorityWithoutADefinition 시나리오의 기대 동작과 상태 변화를 검증한다. */
        public void NexusUsesTheCommonHealthAuthorityWithoutADefinition()
        {
            NexusModel nexus = new NexusModel(100f);

            Assert.That(nexus.Definition, Is.Null);
            Assert.That(nexus.ApplyNexusDamage(30f), Is.EqualTo(30f));
            Assert.That(nexus.CurrentHealth, Is.EqualTo(70f));
            nexus.ApplyNexusDamage(100f);
            Assert.That(nexus.IsAlive, Is.False);
            Assert.That(nexus.CurrentHealth, Is.Zero);
        }

        [Test]
        /* MutableAuthorityPropertiesHaveNoPublicSetters 시나리오의 기대 동작과 상태 변화를 검증한다. */
        public void MutableAuthorityPropertiesHaveNoPublicSetters()
        {
            AssertNoPublicSetter(typeof(StageManager), nameof(StageManager.Gold));
            AssertNoPublicSetter(typeof(StageManager), nameof(StageManager.DarkTrace));
            AssertNoPublicSetter(typeof(UnitBaseModel), nameof(UnitBaseModel.CurrentHealth));
            AssertNoPublicSetter(typeof(UnitBaseModel), nameof(UnitBaseModel.CurrentShield));
            AssertNoPublicSetter(typeof(RunSessionModel), nameof(RunSessionModel.PartyRoster));
            AssertNoPublicSetter(
                typeof(RunSessionModel),
                nameof(RunSessionModel.PrisonerInventory));
            AssertNoPublicSetter(
                typeof(MonsterSkillBucket),
                nameof(MonsterSkillBucket.SelectedChoices));

            Assert.That(
                typeof(RunSessionModel)
                    .GetMethods(BindingFlags.Instance | BindingFlags.Public)
                    .Any(method => method.Name.StartsWith("Set", StringComparison.Ordinal)),
                Is.False);
        }

        [Test]
        /* PhaseOneCatalogRemainsCompatibleWithRuntimeModels 시나리오의 기대 동작과 상태 변화를 검증한다. */
        public void PhaseOneCatalogRemainsCompatibleWithRuntimeModels()
        {
            MonsterModel monster = CreateMonster("vega", false);
            EnemyModel enemy = CreateEnemy("stage2-arsen");

            Assert.That(catalog.SourceFileCount, Is.EqualTo(42));
            Assert.That(catalog.AllDefinitions.Count, Is.EqualTo(1836));
            Assert.That(monster.MonsterDefinition, Is.SameAs(catalog.GetMonster("vega")));
            Assert.That(
                monster.SkillBucket.ActiveSkills[0],
                Is.SameAs(catalog.GetSkill("vega-a")));
            Assert.That(enemy.EnemyDefinition, Is.SameAs(catalog.GetEnemy("stage2-arsen")));
            Assert.That(
                enemy.SkillBucket.PassiveSkill,
                Is.SameAs(catalog.GetSkill("enemy-commander-resistance")));
        }

        /* CreateSession 테스트 대상을 필요한 의존성과 함께 구성한다. */
        private RunSessionModel CreateSession(MonsterModel initialMonster)
        {
            return new RunSessionModel(
                "stage1",
                1,
                "stage1-day1-normal",
                new PartyRoster(initialMonster),
                new PrisonerInventory());
        }

        /* CreateMonster 테스트 대상을 필요한 의존성과 함께 구성한다. */
        private MonsterModel CreateMonster(string monsterId, bool autoSkillEnabled)
        {
            MonsterDefinition definition = catalog.GetMonster(monsterId);
            return new MonsterModel(
                definition,
                catalog.GetSkill($"{monsterId}-a"),
                catalog.Choices.Values.Where(
                    choice => string.Equals(
                            choice.monster_id,
                            monsterId,
                            StringComparison.Ordinal)
                        && string.Equals(
                            choice.choice_group,
                            "PassiveBase",
                            StringComparison.Ordinal)),
                autoSkillEnabled);
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

        /* AssertNoPublicSetter 검증 조건을 공통 보조 절차로 확인한다. */
        private static void AssertNoPublicSetter(Type type, string propertyName)
        {
            PropertyInfo property = type.GetProperty(propertyName);
            Assert.That(property, Is.Not.Null);
            Assert.That(property.SetMethod == null || !property.SetMethod.IsPublic, Is.True);
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

            Assert.That(files.Length, Is.EqualTo(42));
            return sources;
        }
    }
}
