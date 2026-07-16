using System;
using System.Collections.Generic;
using System.Globalization;
using Pakuri.Combat;
using UnityEngine;

namespace Pakuri.Data
{
    public static partial class PakuriCsvRuntimeData
    {
        private sealed class EnemyMigrationRow
        {
            public string Id;
            public string StageId;
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
            public string SkillLoadoutId;
            public string PassiveSkillName;
            public string PassiveSkillId;
            public float PassiveSkillValue;
            public float NexusDamage;
            public string PassiveSummary;
        }

        private sealed class EnemySkillLoadoutRow
        {
            public string LoadoutId;
            public SkillSlot RuntimeSlot;
            public string SkillId;
            public string AiRole;
            public int Priority;
            public bool Enabled;
        }

        private sealed class EnemyBaseSkillRow
        {
            public SkillRow Skill;
            public string ExecutionProfile;
            public string TargetScope;
            public string TargetSelection;
            public float CastRange;
            public float EffectRadius;
            public float ProjectileLifetime;
            public float FlatValue;
            public float IncomingDamageMultiplier = 1f;
            public float MoveSpeedMultiplier = 1f;
            public float OutgoingDamageMultiplier = 1f;
            public float ChainDamageMultiplier;
            public float ChainDelaySeconds;
            public float ChainRadius;
            public bool ExcludePrimaryTarget;
            public float StatusActionSpeedBonus;
            public float StatusDurationSeconds;
            public float TargetMaxHealthRatio;
            public string HitTargetCount;
            public float ChargeRampSeconds = 3f;
            public float ChargeMoveSpeedMultiplier = 2.5f;
        }

        private sealed class EnemyMigrationTriggerRow
        {
            public string Id;
            public string SourceSkillId;
            public SkillTriggerEvent TriggerEvent;
            public string TriggeredSkillId;
            public SkillRuntimeKind RuntimeKind;
            public int SortOrder;
            public bool Enabled;
        }

        private static EnemyMigrationRow ParseEnemyMigrationRow(CsvRecord record)
        {
            return new EnemyMigrationRow
            {
                Id = record.ReadRequiredString("enemy_id"),
                StageId = record.ReadRequiredString("stage_id"),
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
                SkillLoadoutId = record.ReadRequiredString("skill_loadout_id"),
                PassiveSkillName = record.ReadString("passive_skill_name"),
                PassiveSkillId = record.ReadString("passive_skill_id"),
                PassiveSkillValue = record.ReadFloat("passive_skill_value"),
                NexusDamage = record.ReadFloat("nexus_damage"),
                PassiveSummary = record.ReadString("passive_summary")
            };
        }

        private static EnemySkillLoadoutRow ParseEnemySkillLoadoutRow(CsvRecord record)
        {
            return new EnemySkillLoadoutRow
            {
                LoadoutId = record.ReadRequiredString("loadout_id"),
                RuntimeSlot = record.ReadEnum<SkillSlot>("runtime_slot"),
                SkillId = record.ReadRequiredString("skill_id"),
                AiRole = record.ReadRequiredString("ai_role"),
                Priority = record.ReadInt("priority"),
                Enabled = record.ReadBool("enabled")
            };
        }

        private static EnemyBaseSkillRow ParseEnemyBaseSkillRow(CsvRecord record, string tableName)
        {
            if (record.HasColumn("runtime_hitbox_offset_x") || record.HasColumn("runtime_hitbox_offset_y"))
            {
                throw new CsvFatalException(
                    $"CSV table '{tableName}' must not define runtime hitbox offset columns. Enemy runtime hitboxes are centered at (0,0).");
            }

            return new EnemyBaseSkillRow
            {
                Skill = ParseSkillRow(record, tableName, "enemy-shared"),
                ExecutionProfile = ReadOptionalStringIfColumnExists(record, "execution_profile"),
                TargetScope = ReadOptionalStringIfColumnExists(record, "target_scope"),
                TargetSelection = ReadOptionalStringIfColumnExists(record, "target_selection"),
                CastRange = ReadOptionalFloatIfColumnExists(record, "cast_range"),
                EffectRadius = ReadOptionalFloatIfColumnExists(record, "effect_radius"),
                ProjectileLifetime = ReadOptionalFloatIfColumnExists(record, "projectile_lifetime"),
                FlatValue = ReadOptionalFloatIfColumnExists(record, "flat_value"),
                IncomingDamageMultiplier = ReadOptionalFloatWithDefaultIfColumnExists(record, "incoming_damage_multiplier", 1f),
                MoveSpeedMultiplier = ReadOptionalFloatWithDefaultIfColumnExists(record, "move_speed_multiplier", 1f),
                OutgoingDamageMultiplier = ReadOptionalFloatWithDefaultIfColumnExists(record, "outgoing_damage_multiplier", 1f),
                ChainDamageMultiplier = ReadOptionalFloatIfColumnExists(record, "chain_damage_multiplier"),
                ChainDelaySeconds = ReadOptionalFloatIfColumnExists(record, "chain_delay_seconds"),
                ChainRadius = ReadOptionalFloatIfColumnExists(record, "chain_radius"),
                ExcludePrimaryTarget = ReadOptionalBoolIfColumnExists(record, "exclude_primary_target"),
                StatusActionSpeedBonus = ReadOptionalFloatIfColumnExists(record, "status_action_speed_bonus"),
                StatusDurationSeconds = ReadOptionalFloatIfColumnExists(record, "status_duration_seconds"),
                TargetMaxHealthRatio = ReadOptionalFloatIfColumnExists(record, "target_max_health_ratio"),
                HitTargetCount = ReadOptionalStringIfColumnExists(record, "hit_target_count"),
                ChargeRampSeconds = ReadOptionalFloatWithDefaultIfColumnExists(record, "charge_ramp_seconds", 3f),
                ChargeMoveSpeedMultiplier = ReadOptionalFloatWithDefaultIfColumnExists(record, "charge_move_speed_multiplier", 2.5f)
            };
        }

        private static EnemyMigrationTriggerRow ParseEnemyMigrationTriggerRow(CsvRecord record)
        {
            return new EnemyMigrationTriggerRow
            {
                Id = record.ReadRequiredString("trigger_id"),
                SourceSkillId = record.ReadRequiredString("source_skill_id"),
                TriggerEvent = record.ReadEnum<SkillTriggerEvent>("trigger_event"),
                TriggeredSkillId = record.ReadRequiredString("triggered_skill_id"),
                RuntimeKind = record.ReadEnum<SkillRuntimeKind>("runtime_kind"),
                SortOrder = record.ReadInt("sort_order"),
                Enabled = record.ReadBool("enabled")
            };
        }

        private static void ValidateEnemyMigrationRows(SourceModel model, List<string> errors)
        {
            if (model.MigratedEnemies.Count != model.StageOneEnemies.Count + model.StageTwoEnemies.Count)
            {
                errors.Add(
                    $"enemies.csv row count '{model.MigratedEnemies.Count}' does not match legacy stage row count '{model.StageOneEnemies.Count + model.StageTwoEnemies.Count}'.");
            }

            if (model.EnemyBaseSkills.Count != model.EnemySkills.Count)
            {
                errors.Add(
                    $"Enemy base skill row count '{model.EnemyBaseSkills.Count}' does not match legacy skill count '{model.EnemySkills.Count}'.");
            }

            var loadoutIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var loadoutSlotKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var referencedLoadoutIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < model.EnemySkillLoadouts.Count; i++)
            {
                var loadout = model.EnemySkillLoadouts[i];
                loadoutIds.Add(loadout.LoadoutId);
                if (!loadoutSlotKeys.Add(loadout.LoadoutId + ":" + loadout.RuntimeSlot))
                {
                    errors.Add($"Enemy loadout '{loadout.LoadoutId}' has duplicate runtime slot '{loadout.RuntimeSlot}'.");
                }

                if (!model.EnemyBaseSkills.ContainsKey(loadout.SkillId))
                {
                    errors.Add($"Enemy loadout '{loadout.LoadoutId}' references unknown base skill '{loadout.SkillId}'.");
                }
            }

            foreach (var enemy in model.MigratedEnemies.Values)
            {
                if (!string.Equals(enemy.StageId, "stage_one", StringComparison.OrdinalIgnoreCase)
                    && !string.Equals(enemy.StageId, "stage_two", StringComparison.OrdinalIgnoreCase))
                {
                    errors.Add($"Enemy '{enemy.Id}' has unsupported stage_id '{enemy.StageId}'.");
                }

                if (!loadoutIds.Contains(enemy.SkillLoadoutId))
                {
                    errors.Add($"Enemy '{enemy.Id}' references missing skill_loadout_id '{enemy.SkillLoadoutId}'.");
                }

                if (!referencedLoadoutIds.Add(enemy.SkillLoadoutId))
                {
                    errors.Add($"Enemy skill_loadout_id '{enemy.SkillLoadoutId}' is assigned to more than one Enemy.");
                }

                var legacyRows = string.Equals(enemy.StageId, "stage_two", StringComparison.OrdinalIgnoreCase)
                    ? model.StageTwoEnemies
                    : model.StageOneEnemies;
                if (!legacyRows.TryGetValue(enemy.Id, out var legacy))
                {
                    errors.Add($"Enemy '{enemy.Id}' has no matching legacy row for parity validation.");
                    continue;
                }

                ValidateEnemyMigrationParity(enemy, legacy, errors);
                ValidateEnemyLoadoutParity(enemy, legacy, model.EnemySkillLoadouts, errors);
            }

            foreach (var loadoutId in loadoutIds)
            {
                if (!referencedLoadoutIds.Contains(loadoutId))
                {
                    errors.Add($"Enemy loadout '{loadoutId}' is not referenced by enemies.csv.");
                }
            }

            foreach (var legacySkill in model.EnemySkills.Values)
            {
                if (!model.EnemyBaseSkills.TryGetValue(legacySkill.Id, out var migrated) || migrated.Skill == null)
                {
                    errors.Add($"Legacy enemy skill '{legacySkill.Id}' has no migrated base row.");
                    continue;
                }

                ValidateEnemySkillMigrationParity(legacySkill, migrated, errors);
            }

            ValidateEnemyNodeMigrationParity(model, errors);
            ValidateEnemyCombatStartTrigger(model, "OpeningCharge", SkillRuntimeKind.SingleAttack, errors);
            ValidateEnemyCombatStartTrigger(model, "Intimidation", SkillRuntimeKind.Buff, errors);
        }

        private static void ValidateEnemyMigrationParity(
            EnemyMigrationRow migrated,
            EnemyRow legacy,
            List<string> errors)
        {
            if (!string.Equals(migrated.DisplayName, legacy.DisplayName, StringComparison.Ordinal)
                || migrated.EncounterRole != legacy.EncounterRole
                || migrated.AttackType != legacy.AttackType
                || migrated.Attribute != legacy.Attribute
                || !string.Equals(migrated.UnitSpritePath, legacy.UnitSpritePath, StringComparison.Ordinal)
                || !string.Equals(migrated.ProjectileSpritePath, legacy.ProjectileSpritePath, StringComparison.Ordinal)
                || !Approximately(migrated.MaxHealth, legacy.MaxHealth)
                || !Approximately(migrated.AttackPower, legacy.AttackPower)
                || !Approximately(migrated.SpellPower, legacy.SpellPower)
                || !Approximately(migrated.MoveSpeed, legacy.MoveSpeed)
                || !Approximately(migrated.CriticalChance, legacy.CriticalChance)
                || !Approximately(migrated.CriticalDamage, legacy.CriticalDamage)
                || !Approximately(migrated.CriticalResistance, legacy.CriticalResistance)
                || !Approximately(migrated.PhysicalDefense, legacy.PhysicalDefense)
                || !Approximately(migrated.FireDefense, legacy.FireDefense)
                || !Approximately(migrated.LightningDefense, legacy.LightningDefense)
                || !Approximately(migrated.IceDefense, legacy.IceDefense)
                || !Approximately(migrated.DarknessDefense, legacy.DarknessDefense)
                || !Approximately(migrated.HolyDefense, legacy.HolyDefense)
                || !string.Equals(migrated.PassiveSkillName, legacy.PassiveSkillName, StringComparison.Ordinal)
                || !string.Equals(migrated.PassiveSkillId, legacy.PassiveSkillId, StringComparison.OrdinalIgnoreCase)
                || !Approximately(migrated.PassiveSkillValue, legacy.PassiveSkillValue)
                || !Approximately(migrated.NexusDamage, legacy.NexusDamage)
                || !string.Equals(migrated.PassiveSummary, legacy.PassiveSummary, StringComparison.Ordinal))
            {
                errors.Add($"Enemy '{migrated.Id}' does not match its legacy stage row.");
            }
        }

        private static void ValidateEnemyLoadoutParity(
            EnemyMigrationRow migrated,
            EnemyRow legacy,
            List<EnemySkillLoadoutRow> loadouts,
            List<string> errors)
        {
            var enabledCount = 0;
            var basicCount = 0;
            var activeCount = 0;
            var expectedBasicSkillId = legacy.BasicSkill.ToString();
            var expectedActiveSkillId = legacy.StageOneSkill.ToString();

            for (var i = 0; i < loadouts.Count; i++)
            {
                var loadout = loadouts[i];
                if (!loadout.Enabled
                    || !string.Equals(loadout.LoadoutId, migrated.SkillLoadoutId, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                enabledCount++;
                if (loadout.RuntimeSlot == SkillSlot.A
                    && string.Equals(loadout.SkillId, expectedBasicSkillId, StringComparison.OrdinalIgnoreCase))
                {
                    basicCount++;
                }

                if (loadout.RuntimeSlot == SkillSlot.B
                    && string.Equals(loadout.SkillId, expectedActiveSkillId, StringComparison.OrdinalIgnoreCase))
                {
                    activeCount++;
                }
            }

            if (enabledCount != 2 || basicCount != 1 || activeCount != 1)
            {
                errors.Add(
                    $"Enemy '{migrated.Id}' loadout '{migrated.SkillLoadoutId}' must preserve " +
                    $"legacy A='{expectedBasicSkillId}' and B='{expectedActiveSkillId}'.");
            }
        }

        private static void ValidateEnemySkillMigrationParity(
            EnemySkillRow legacy,
            EnemyBaseSkillRow migrated,
            List<string> errors)
        {
            var skill = migrated.Skill;
            if (!string.Equals(skill.DisplayName, legacy.DisplayName, StringComparison.Ordinal)
                || !Approximately(skill.AttackPowerCoefficient, legacy.AttackPowerCoefficient)
                || !Approximately(skill.SpellPowerCoefficient, legacy.SpellPowerCoefficient)
                || !Approximately(skill.CooldownSeconds, legacy.CooldownSeconds)
                || !Approximately(migrated.CastRange > 0f ? migrated.CastRange : skill.Radius, legacy.Radius)
                || !Approximately(skill.ActiveDurationSeconds, legacy.ActiveDuration)
                || !Approximately(migrated.FlatValue, legacy.FlatValue)
                || !Approximately(skill.ProjectileSpeed, legacy.ProjectileSpeed)
                || !Approximately(migrated.ProjectileLifetime, legacy.ProjectileLifetime)
                || !Approximately(migrated.MoveSpeedMultiplier, legacy.MoveSpeedMultiplier)
                || !Approximately(migrated.OutgoingDamageMultiplier, legacy.OutgoingDamageMultiplier))
            {
                errors.Add($"Enemy base skill '{legacy.Id}' does not match EnemySkillData.csv.");
            }
        }

        private static void ValidateEnemyCombatStartTrigger(
            SourceModel model,
            string skillId,
            SkillRuntimeKind runtimeKind,
            List<string> errors)
        {
            var count = 0;
            foreach (var trigger in model.EnemyMigrationTriggers.Values)
            {
                if (trigger.Enabled
                    && trigger.TriggerEvent == SkillTriggerEvent.CombatStart
                    && string.Equals(trigger.SourceSkillId, skillId, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(trigger.TriggeredSkillId, skillId, StringComparison.OrdinalIgnoreCase)
                    && trigger.RuntimeKind == runtimeKind)
                {
                    count++;
                }
            }

            if (count != 1)
            {
                errors.Add($"Enemy skill '{skillId}' requires exactly one enabled CombatStart trigger; found '{count}'.");
            }
        }

        private static void ValidateEnemyNodeMigrationParity(SourceModel model, List<string> errors)
        {
            for (var i = 0; i < model.EnemySkillNodes.Count; i++)
            {
                var node = model.EnemySkillNodes[i];
                if (node == null
                    || !model.EnemyBaseSkills.TryGetValue(node.SkillId, out var migrated)
                    || migrated == null)
                {
                    continue;
                }

                if (!string.Equals(migrated.ExecutionProfile, node.ActionOp, StringComparison.OrdinalIgnoreCase))
                {
                    errors.Add(
                        $"Enemy base skill '{node.SkillId}' execution_profile '{migrated.ExecutionProfile}' does not match legacy action_op '{node.ActionOp}'.");
                }
            }

            for (var i = 0; i < model.EnemySkillNodeParams.Count; i++)
            {
                var param = model.EnemySkillNodeParams[i];
                if (param == null
                    || !model.EnemyBaseSkills.TryGetValue(param.SkillId, out var migrated)
                    || migrated == null
                    || migrated.Skill == null)
                {
                    continue;
                }

                if (!EnemyNodeParamMatchesBase(param, migrated))
                {
                    errors.Add(
                        $"Enemy node param '{param.SkillId}.{param.ParamKey}={param.ParamValue}' was not preserved by the migrated base row.");
                }
            }
        }

        private static bool EnemyNodeParamMatchesBase(
            EnemySkillNodeParamRow param,
            EnemyBaseSkillRow migrated)
        {
            if (string.Equals(param.ParamKey, "attribute", StringComparison.OrdinalIgnoreCase))
            {
                return Enum.TryParse(param.ParamValue, true, out DamageAttribute attribute)
                    && migrated.Skill.Attribute == attribute;
            }

            if (!float.TryParse(
                    param.ParamValue,
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out var expected))
            {
                return false;
            }

            switch (param.ParamKey != null ? param.ParamKey.ToLowerInvariant() : string.Empty)
            {
                case "fallback_speed":
                    return Approximately(migrated.Skill.ProjectileSpeed, expected);
                case "fallback_lifetime":
                    return Approximately(migrated.ProjectileLifetime, expected);
                case "delay":
                    return Approximately(migrated.ChainDelaySeconds, expected);
                case "chain_multiplier":
                    return Approximately(migrated.ChainDamageMultiplier, expected);
                case "chain_radius":
                    return Approximately(migrated.ChainRadius, expected);
                case "action_speed_bonus":
                    return Approximately(migrated.StatusActionSpeedBonus, expected);
                case "duration":
                case "status_duration":
                    return Approximately(migrated.StatusDurationSeconds, expected);
                case "target_max_health_ratio":
                    return Approximately(migrated.TargetMaxHealthRatio, expected);
                case "ramp_seconds":
                    return Approximately(migrated.ChargeRampSeconds, expected);
                case "move_speed_multiplier":
                    return Approximately(migrated.ChargeMoveSpeedMultiplier, expected);
                case "multiplier":
                    return Approximately(migrated.OutgoingDamageMultiplier, expected);
                default:
                    return false;
            }
        }

        private static bool Approximately(float left, float right)
        {
            return Mathf.Abs(left - right) <= 0.0001f;
        }
    }
}
