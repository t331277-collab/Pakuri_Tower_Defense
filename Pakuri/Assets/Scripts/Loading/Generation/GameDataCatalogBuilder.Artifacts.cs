/*
 * 역할: 유물, 시너지와 소환 유닛 런타임 변환.
 * 책임: 검증된 원본 행을 Definition으로 만들고 스킬·소환 참조를 해석한다.
 */

using System;
using System.Collections.Generic;
using Pakuri.InGame;
using UnityEngine;
using static Pakuri.Data.CsvRowParser;
using static Pakuri.Data.CsvSourceModel;
using static Pakuri.Data.SkillGraphParser;

namespace Pakuri.Data
{
    internal sealed partial class GameDataCatalogBuilder
    {
        private SummonDefinition[] BuildSummons(
            SourceModel model,
            StatusEffectDefinition[] statusDefinitions)
        {
            var rows = FilterAndSort(
                model.Summons.Values,
                _ => true,
                (left, right) => string.Compare(left.Name, right.Name, StringComparison.OrdinalIgnoreCase));
            var definitions = new SummonDefinition[rows.Count];
            for (var i = 0; i < rows.Count; i++)
            {
                var row = rows[i];
                var definition = ScriptableObject.CreateInstance<SummonDefinition>();
                definition.SummonName = row.Name;
                definition.DisplayName = row.DisplayName;
                definition.RoleSummary = row.RoleSummary;
                definition.ElementLabel = row.ElementLabel;
                definition.PrimaryAttribute = row.PrimaryAttribute;
                definition.Icon = LoadSprite(row.MonsterIconImagePath);
                definition.Image = LoadSprite(row.ImagePath);
                definition.PowerStat = row.PowerStat;
                definition.BaseStats = new UnitCombatStats
                {
                    MaxHealth = row.MaxHealth,
                    AttackPower = row.BaseAttackPower,
                    SpellPower = row.BaseSpellPower,
                    MoveSpeed = row.BaseMoveSpeed,
                    CriticalChance = row.BaseCriticalChance,
                    CriticalDamage = row.BaseCriticalDamage
                };
                definition.Defenses = new UnitDefenseStats
                {
                    Physical = row.PhysicalDefense,
                    Fire = row.FireDefense,
                    Lightning = row.LightningDefense,
                    Ice = row.IceDefense,
                    Darkness = row.DarknessDefense,
                    Holy = row.HolyDefense
                };
                definition.ActiveSkills = BuildActiveSkills(
                    model,
                    row.Name,
                    statusDefinitions,
                    model.SummonSkills.Values);
                var reactions = BuildSkillReactions(
                    model,
                    trigger => string.Equals(
                        trigger.MonsterName,
                        row.Name,
                        StringComparison.OrdinalIgnoreCase),
                    definition.ActiveSkills,
                    statusDefinitions);
                AttachSkillReactions(
                    definition.ActiveSkills,
                    null,
                    reactions);
                definitions[i] = definition;
            }

            return definitions;
        }

        private void BuildArtifactDefinitions(
            SourceModel model,
            GameDataCatalog catalog,
            StatusEffectDefinition[] statusDefinitions)
        {
            var skills = new Dictionary<string, SkillDefinition>(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < catalog.Monsters.Length; i++)
            {
                AddSkills(skills, catalog.Monsters[i].ActiveSkills);
            }
            for (var i = 0; i < catalog.Summons.Length; i++)
            {
                AddSkills(skills, catalog.Summons[i].ActiveSkills);
            }

            catalog.ArtifactEffects = BuildArtifactEffects(model, skills, statusDefinitions);
            catalog.ArtifactSynergyEffects = BuildArtifactSynergyEffects(model, skills, catalog.Summons);
            catalog.Artifacts = BuildArtifacts(model, catalog.ArtifactEffects);
            catalog.ArtifactSynergies = BuildArtifactSynergies(model, catalog.ArtifactSynergyEffects);
            var levels = new List<ArtifactSynergyLevelDefinition>();
            for (var i = 0; i < catalog.ArtifactSynergies.Length; i++)
            {
                levels.AddRange(catalog.ArtifactSynergies[i].Levels);
            }
            catalog.ArtifactSynergyLevels = levels.ToArray();
        }

        private ArtifactEffectDefinition[] BuildArtifactEffects(
            SourceModel model,
            Dictionary<string, SkillDefinition> skills,
            StatusEffectDefinition[] statusDefinitions)
        {
            var allSkills = new List<SkillDefinition>(skills.Values).ToArray();
            var rows = FilterAndSort(
                model.ArtifactEffects.Values,
                _ => true,
                (left, right) => string.Compare(left.Name, right.Name, StringComparison.OrdinalIgnoreCase));
            var definitions = new ArtifactEffectDefinition[rows.Count];
            for (var i = 0; i < rows.Count; i++)
            {
                var row = rows[i];
                definitions[i] = new ArtifactEffectDefinition
                {
                    EffectName = row.Name,
                    ArtifactName = row.ArtifactName,
                    ApplicationMode = row.ApplicationMode,
                    Recipient = row.Recipient,
                    RepeatRule = row.RepeatRule,
                    SelectionRule = row.SelectionRule,
                    RecipientMonsterName = row.RecipientMonsterName,
                    TargetSkill = ResolveSkill(skills, row.TargetSkillName),
                    OutcomeSkill = ResolveSkill(skills, row.OutcomeSkillName),
                    Nodes = MapSkillNodes(BuildSkillNodes(
                        model,
                        SkillNodeOwnerKind.Effect,
                        row.Name,
                        row.TargetSkillName)),
                    Reactions = BuildSkillReactions(
                        model,
                        trigger => string.Equals(
                            trigger.SourceSkillName,
                            row.Name,
                            StringComparison.OrdinalIgnoreCase),
                        allSkills,
                        statusDefinitions)
                };
            }

            return definitions;
        }

        private ArtifactSynergyEffectDefinition[] BuildArtifactSynergyEffects(
            SourceModel model,
            Dictionary<string, SkillDefinition> skills,
            SummonDefinition[] summons)
        {
            var summonLookup = new Dictionary<string, SummonDefinition>(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < summons.Length; i++)
            {
                summonLookup[summons[i].SummonName] = summons[i];
            }

            var rows = FilterAndSort(
                model.ArtifactSynergyEffects.Values,
                _ => true,
                (left, right) => string.Compare(left.Name, right.Name, StringComparison.OrdinalIgnoreCase));
            var definitions = new ArtifactSynergyEffectDefinition[rows.Count];
            for (var i = 0; i < rows.Count; i++)
            {
                var row = rows[i];
                definitions[i] = new ArtifactSynergyEffectDefinition
                {
                    EffectName = row.Name,
                    SynergyLevelName = row.SynergyLevelName,
                    ApplicationMode = row.ApplicationMode,
                    Recipient = row.Recipient,
                    RecipientMonsterName = row.RecipientMonsterName,
                    TargetSkill = ResolveSkill(skills, row.TargetSkillName),
                    OutcomeSkill = ResolveSkill(skills, row.OutcomeSkillName),
                    SpawnSummon = ResolveSummon(summonLookup, row.SpawnSummonName)
                };
            }

            return definitions;
        }

        private ArtifactDefinition[] BuildArtifacts(
            SourceModel model,
            ArtifactEffectDefinition[] effects)
        {
            var rows = FilterAndSort(
                model.Artifacts.Values,
                _ => true,
                (left, right) => string.Compare(left.Name, right.Name, StringComparison.OrdinalIgnoreCase));
            var definitions = new ArtifactDefinition[rows.Count];
            for (var i = 0; i < rows.Count; i++)
            {
                var row = rows[i];
                definitions[i] = new ArtifactDefinition
                {
                    ArtifactName = row.Name,
                    DisplayName = row.DisplayName,
                    SynergyName = row.SynergyName,
                    Description = row.DescriptionText,
                    Icon = LoadSprite(row.IconPath),
                    Effects = Array.FindAll(
                        effects,
                        effect => string.Equals(effect.ArtifactName, row.Name, StringComparison.OrdinalIgnoreCase))
                };
            }

            return definitions;
        }

        private ArtifactSynergyDefinition[] BuildArtifactSynergies(
            SourceModel model,
            ArtifactSynergyEffectDefinition[] effects)
        {
            var rows = FilterAndSort(
                model.ArtifactSynergies.Values,
                _ => true,
                (left, right) => string.Compare(left.Name, right.Name, StringComparison.OrdinalIgnoreCase));
            var definitions = new ArtifactSynergyDefinition[rows.Count];
            for (var i = 0; i < rows.Count; i++)
            {
                var row = rows[i];
                var levels = new ArtifactSynergyLevelDefinition[row.Levels.Length];
                for (var levelIndex = 0; levelIndex < levels.Length; levelIndex++)
                {
                    var level = row.Levels[levelIndex];
                    levels[levelIndex] = new ArtifactSynergyLevelDefinition
                    {
                        LevelName = level.Name,
                        RequiredCount = level.RequiredCount,
                        Description = level.DescriptionText,
                        Effects = Array.FindAll(
                            effects,
                            effect => string.Equals(effect.SynergyLevelName, level.Name, StringComparison.OrdinalIgnoreCase))
                    };
                }

                definitions[i] = new ArtifactSynergyDefinition
                {
                    SynergyName = row.Name,
                    DisplayName = row.DisplayName,
                    Summary = row.Summary,
                    Description = row.DescriptionText,
                    Icon = LoadSprite(row.IconPath),
                    Levels = levels
                };
            }

            return definitions;
        }

        private static void AddSkills(
            Dictionary<string, SkillDefinition> lookup,
            SkillDefinition[] skills)
        {
            for (var i = 0; skills != null && i < skills.Length; i++)
            {
                if (skills[i] != null && !string.IsNullOrWhiteSpace(skills[i].SkillName))
                {
                    lookup[skills[i].SkillName] = skills[i];
                }
            }
        }

        private static SkillDefinition ResolveSkill(
            Dictionary<string, SkillDefinition> lookup,
            string skillName)
        {
            return !string.IsNullOrWhiteSpace(skillName) && lookup.TryGetValue(skillName, out var skill)
                ? skill
                : null;
        }

        private static SummonDefinition ResolveSummon(
            Dictionary<string, SummonDefinition> lookup,
            string summonName)
        {
            return !string.IsNullOrWhiteSpace(summonName) && lookup.TryGetValue(summonName, out var summon)
                ? summon
                : null;
        }
    }
}
