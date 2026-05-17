using System.Collections.Generic;
using Pakuri.Combat;

namespace Pakuri.Data
{
    public static partial class PakuriCsvRuntimeData
    {
        private sealed class EnemyRow
        {
            public string Id;
            public string DisplayName;
            public EnemyEncounterRole EncounterRole;
            public EnemyAttackType AttackType;
            public DamageAttribute Attribute;
            public string UnitSpritePath;
            public string ProjectileSpritePath;
            public float MaxHealth;
            public float AttackPower;
            public float SpellPower;
            public float MoveSpeed;
            public float CriticalChance;
            public float CriticalDamage;
            public float CriticalResistance;
            public float PhysicalDefense;
            public float FireDefense;
            public float LightningDefense;
            public float IceDefense;
            public float DarknessDefense;
            public float HolyDefense;
            public StageOneEnemySkillKind StageOneSkill;
            public string ActiveSkillName;
            public float ActiveSkillCoefficient;
            public float ActiveSkillCooldown;
            public float ActiveSkillDuration;
            public float ActiveSkillRadius;
            public float ActiveSkillFlatValue;
            public string PassiveSkillName;
            public string PassiveSummary;
        }

        private sealed class EnemySkillRow
        {
            public string Id;
            public string DisplayName;
            public StageOneEnemySkillKind StageOneSkill;
            public string SkillEffectPrefabPath;
            public float AttackPowerCoefficient;
            public float SpellPowerCoefficient;
            public float Radius;
            public float CooldownSeconds;
            public float ActiveDuration;
            public float FlatValue;
        }

        private static EnemyRow ParseEnemyRow(CsvRecord record)
        {
            return new EnemyRow
            {
                Id = record.ReadRequiredString("enemy_id"),
                DisplayName = record.ReadRequiredString("display_name"),
                EncounterRole = record.ReadEnum<EnemyEncounterRole>("encounter_role"),
                AttackType = record.ReadEnum<EnemyAttackType>("attack_type"),
                Attribute = record.ReadEnum<DamageAttribute>("attribute"),
                UnitSpritePath = record.ReadString("unit_sprite_path"),
                ProjectileSpritePath = record.ReadString("projectile_sprite_path"),
                MaxHealth = record.ReadFloat("max_health"),
                AttackPower = record.ReadFloat("attack_power"),
                SpellPower = record.ReadFloat("spell_power"),
                MoveSpeed = record.ReadFloat("move_speed"),
                CriticalChance = record.ReadFloat("crit_chance"),
                CriticalDamage = record.ReadFloat("crit_damage"),
                CriticalResistance = record.ReadFloat("crit_resistance"),
                PhysicalDefense = record.ReadFloat("def_physical"),
                FireDefense = record.ReadFloat("def_fire"),
                LightningDefense = record.ReadFloat("def_lightning"),
                IceDefense = record.ReadFloat("def_ice"),
                DarknessDefense = record.ReadFloat("def_darkness"),
                HolyDefense = record.ReadFloat("def_holy"),
                StageOneSkill = record.ReadEnum<StageOneEnemySkillKind>("stage_one_skill"),
                PassiveSkillName = record.ReadString("passive_skill_name"),
                PassiveSummary = record.ReadString("passive_summary")
            };
        }

        private static EnemySkillRow ParseEnemySkillRow(CsvRecord record)
        {
            return new EnemySkillRow
            {
                Id = record.ReadRequiredString("skill_id"),
                DisplayName = record.ReadRequiredString("display_name"),
                StageOneSkill = record.ReadEnum<StageOneEnemySkillKind>("stage_one_skill"),
                SkillEffectPrefabPath = record.ReadString("skill_effect_prefab_path"),
                AttackPowerCoefficient = record.ReadFloat("attack_power_coefficient"),
                SpellPowerCoefficient = record.ReadFloat("spell_power_coefficient"),
                Radius = record.ReadFloat("radius"),
                CooldownSeconds = record.ReadFloat("cooldown_seconds"),
                ActiveDuration = record.ReadFloat("active_duration"),
                FlatValue = record.ReadFloat("flat_value")
            };
        }

        private static void ApplyEnemySkillRow(
            EnemyRow enemy,
            Dictionary<string, EnemySkillRow> enemySkills,
            CsvRecord enemyRecord)
        {
            var skillId = enemy.StageOneSkill.ToString();
            if (!enemySkills.TryGetValue(skillId, out var skill))
            {
                throw new CsvFatalException(
                    $"CSV row {enemyRecord.RowNumber} in '{enemyRecord.TableName}' references unknown enemy skill '{skillId}'.");
            }

            if (skill.StageOneSkill != enemy.StageOneSkill)
            {
                throw new CsvFatalException(
                    $"Enemy skill '{skill.Id}' stage_one_skill '{skill.StageOneSkill}' does not match enemy '{enemy.Id}' stage_one_skill '{enemy.StageOneSkill}'.");
            }

            enemy.ActiveSkillName = skill.DisplayName;
            enemy.ActiveSkillCoefficient = skill.AttackPowerCoefficient > 0f
                ? skill.AttackPowerCoefficient
                : skill.SpellPowerCoefficient;
            enemy.ActiveSkillCooldown = skill.CooldownSeconds;
            enemy.ActiveSkillDuration = skill.ActiveDuration;
            enemy.ActiveSkillRadius = skill.Radius;
            enemy.ActiveSkillFlatValue = skill.FlatValue;
        }
    }
}
