using System;
using System.Collections.Generic;

namespace Pakuri.Data
{
    public static partial class PakuriCsvRuntimeData
    {
        private sealed class SourceModel
        {
            public readonly Dictionary<string, CatalogEntryRow> CatalogMonsters = new Dictionary<string, CatalogEntryRow>(StringComparer.OrdinalIgnoreCase);
            public readonly Dictionary<string, CatalogEntryRow> CatalogStageOneEnemies = new Dictionary<string, CatalogEntryRow>(StringComparer.OrdinalIgnoreCase);
            public readonly Dictionary<string, CatalogEntryRow> CatalogStageTwoEnemies = new Dictionary<string, CatalogEntryRow>(StringComparer.OrdinalIgnoreCase);
            public readonly Dictionary<string, MonsterRow> Monsters = new Dictionary<string, MonsterRow>(StringComparer.OrdinalIgnoreCase);
            public readonly Dictionary<string, RewardChoiceRow> RewardChoices = new Dictionary<string, RewardChoiceRow>(StringComparer.OrdinalIgnoreCase);
            public readonly Dictionary<string, SkillRow> Skills = new Dictionary<string, SkillRow>(StringComparer.OrdinalIgnoreCase);
            public readonly Dictionary<string, SkillEffectRow> SkillEffects = new Dictionary<string, SkillEffectRow>(StringComparer.OrdinalIgnoreCase);
            public readonly Dictionary<string, SkillTriggerRow> SkillTriggers = new Dictionary<string, SkillTriggerRow>(StringComparer.OrdinalIgnoreCase);
            public readonly Dictionary<string, SkillChoiceRow> SkillChoices = new Dictionary<string, SkillChoiceRow>(StringComparer.OrdinalIgnoreCase);
            public readonly Dictionary<string, StatusEffectRow> StatusEffects = new Dictionary<string, StatusEffectRow>(StringComparer.OrdinalIgnoreCase);
            public readonly Dictionary<string, EnemyRow> StageOneEnemies = new Dictionary<string, EnemyRow>(StringComparer.OrdinalIgnoreCase);
            public readonly Dictionary<string, EnemyRow> StageTwoEnemies = new Dictionary<string, EnemyRow>(StringComparer.OrdinalIgnoreCase);
            public readonly Dictionary<string, EnemySkillRow> EnemySkills = new Dictionary<string, EnemySkillRow>(StringComparer.OrdinalIgnoreCase);
        }
    }
}
