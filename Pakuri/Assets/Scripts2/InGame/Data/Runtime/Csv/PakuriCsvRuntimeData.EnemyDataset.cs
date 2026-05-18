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
            public bool HasBasicSkill;
            public StageOneEnemySkillKind BasicSkill;
            public string BasicSkillName;
            public float BasicSkillCoefficient;
            public float BasicSkillCooldown;
            public float BasicSkillDuration;
            public float BasicSkillRadius;
            public float BasicSkillFlatValue;
            public float BasicSkillProjectileSpeed;
            public float BasicSkillProjectileLifetime;
            public float BasicSkillMoveSpeedMultiplier = 1f;
            public float BasicSkillOutgoingDamageMultiplier = 1f;
            public StageOneEnemySkillKind StageOneSkill;
            public string ActiveSkillName;
            public float ActiveSkillCoefficient;
            public float ActiveSkillCooldown;
            public float ActiveSkillDuration;
            public float ActiveSkillRadius;
            public float ActiveSkillFlatValue;
            public float ActiveSkillProjectileSpeed;
            public float ActiveSkillProjectileLifetime;
            public float ActiveSkillMoveSpeedMultiplier = 1f;
            public float ActiveSkillOutgoingDamageMultiplier = 1f;
            public string PassiveSkillName;
            public string PassiveSkillId;
            public float PassiveSkillValue;
            public string PassiveSummary;
        }

        private sealed class EnemySkillRow
        {
            public string Id;
            public string DisplayName;
            public StageOneEnemySkillKind StageOneSkill;
            public float AttackPowerCoefficient;
            public float SpellPowerCoefficient;
            public float Radius;
            public float CooldownSeconds;
            public float ActiveDuration;
            public float FlatValue;
            public float ProjectileSpeed;
            public float ProjectileLifetime;
            public float MoveSpeedMultiplier = 1f;
            public float OutgoingDamageMultiplier = 1f;
        }

        private static EnemyRow ParseEnemyRow(CsvRecord record)
        {
            var row = new EnemyRow
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
                PassiveSkillId = record.ReadString("passive_skill_id"),
                PassiveSkillValue = record.ReadFloat("passive_skill_value"),
                PassiveSummary = record.ReadString("passive_summary")
            };

            if (TryReadOptionalStageOneSkill(record, "basic_skill", out var basicSkill))
            {
                row.HasBasicSkill = true;
                row.BasicSkill = basicSkill;
            }

            return row;
        }

        private static EnemySkillRow ParseEnemySkillRow(CsvRecord record)
        {
            return new EnemySkillRow
            {
                Id = record.ReadRequiredString("skill_id"),
                DisplayName = record.ReadRequiredString("display_name"),
                StageOneSkill = record.ReadEnum<StageOneEnemySkillKind>("stage_one_skill"),
                AttackPowerCoefficient = record.ReadFloat("attack_power_coefficient"),
                SpellPowerCoefficient = record.ReadFloat("spell_power_coefficient"),
                Radius = record.ReadFloat("radius"),
                CooldownSeconds = record.ReadFloat("cooldown_seconds"),
                ActiveDuration = record.ReadFloat("active_duration"),
                FlatValue = record.ReadFloat("flat_value"),
                ProjectileSpeed = record.ReadFloat("projectile_speed"),
                ProjectileLifetime = record.ReadFloat("projectile_lifetime"),
                MoveSpeedMultiplier = ReadOptionalMultiplier(record, "move_speed_multiplier", 1f),
                OutgoingDamageMultiplier = ReadOptionalMultiplier(record, "outgoing_damage_multiplier", 1f)
            };
        }

        private static void ApplyEnemySkillRow(
            EnemyRow enemy,
            Dictionary<string, EnemySkillRow> enemySkills,
            CsvRecord enemyRecord)
        {
            ApplyEnemySkillAssignment(
                enemy.StageOneSkill,
                enemySkills,
                enemyRecord,
                enemy.Id,
                "stage_one_skill",
                out enemy.ActiveSkillName,
                out enemy.ActiveSkillCoefficient,
                out enemy.ActiveSkillCooldown,
                out enemy.ActiveSkillDuration,
                out enemy.ActiveSkillRadius,
                out enemy.ActiveSkillFlatValue,
                out enemy.ActiveSkillProjectileSpeed,
                out enemy.ActiveSkillProjectileLifetime,
                out enemy.ActiveSkillMoveSpeedMultiplier,
                out enemy.ActiveSkillOutgoingDamageMultiplier);

            if (!enemy.HasBasicSkill)
            {
                return;
            }

            ApplyEnemySkillAssignment(
                enemy.BasicSkill,
                enemySkills,
                enemyRecord,
                enemy.Id,
                "basic_skill",
                out enemy.BasicSkillName,
                out enemy.BasicSkillCoefficient,
                out enemy.BasicSkillCooldown,
                out enemy.BasicSkillDuration,
                out enemy.BasicSkillRadius,
                out enemy.BasicSkillFlatValue,
                out enemy.BasicSkillProjectileSpeed,
                out enemy.BasicSkillProjectileLifetime,
                out enemy.BasicSkillMoveSpeedMultiplier,
                out enemy.BasicSkillOutgoingDamageMultiplier);
        }

        private static float ReadOptionalMultiplier(CsvRecord record, string columnName, float defaultValue)
        {
            var value = record.ReadFloat(columnName);
            return value > 0f ? value : defaultValue;
        }

        private static bool TryReadOptionalStageOneSkill(
            CsvRecord record,
            string columnName,
            out StageOneEnemySkillKind skillKind)
        {
            var value = record.ReadString(columnName);
            if (string.IsNullOrWhiteSpace(value))
            {
                skillKind = default;
                return false;
            }

            if (!System.Enum.TryParse(value, true, out skillKind))
            {
                throw new CsvFatalException(
                    $"CSV row {record.RowNumber} in '{record.TableName}' has invalid enum value '{value}' for '{columnName}'.");
            }

            return true;
        }

        private static void ApplyEnemySkillAssignment(
            StageOneEnemySkillKind skillKind,
            Dictionary<string, EnemySkillRow> enemySkills,
            CsvRecord enemyRecord,
            string enemyId,
            string ownerColumnName,
            out string skillName,
            out float coefficient,
            out float cooldown,
            out float duration,
            out float radius,
            out float flatValue,
            out float projectileSpeed,
            out float projectileLifetime,
            out float moveSpeedMultiplier,
            out float outgoingDamageMultiplier)
        {
            var skillId = skillKind.ToString();
            if (!enemySkills.TryGetValue(skillId, out var skill))
            {
                throw new CsvFatalException(
                    $"CSV row {enemyRecord.RowNumber} in '{enemyRecord.TableName}' references unknown enemy skill '{skillId}' from '{ownerColumnName}'.");
            }

            if (skill.StageOneSkill != skillKind)
            {
                throw new CsvFatalException(
                    $"Enemy skill '{skill.Id}' stage_one_skill '{skill.StageOneSkill}' does not match enemy '{enemyId}' {ownerColumnName} '{skillKind}'.");
            }

            skillName = skill.DisplayName;
            coefficient = skill.AttackPowerCoefficient > 0f
                ? skill.AttackPowerCoefficient
                : skill.SpellPowerCoefficient;
            cooldown = skill.CooldownSeconds;
            duration = skill.ActiveDuration;
            radius = skill.Radius;
            flatValue = skill.FlatValue;
            projectileSpeed = skill.ProjectileSpeed;
            projectileLifetime = skill.ProjectileLifetime;
            moveSpeedMultiplier = skill.MoveSpeedMultiplier;
            outgoingDamageMultiplier = skill.OutgoingDamageMultiplier;
        }
    }
}
