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
                ActiveSkillName = record.ReadString("active_skill_name"),
                ActiveSkillCoefficient = record.ReadFloat("active_skill_coefficient"),
                ActiveSkillCooldown = record.ReadFloat("active_skill_cooldown"),
                ActiveSkillDuration = record.ReadFloat("active_skill_duration"),
                ActiveSkillRadius = record.ReadFloat("active_skill_radius"),
                ActiveSkillFlatValue = record.ReadFloat("active_skill_flat_value"),
                PassiveSkillName = record.ReadString("passive_skill_name"),
                PassiveSummary = record.ReadString("passive_summary")
            };
        }
    }
}
