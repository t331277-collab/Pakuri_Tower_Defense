using System;
using System.Collections.Generic;
using Pakuri.Combat;

namespace Pakuri.Data
{
    public static partial class PakuriCsvRuntimeData
    {
        private sealed class EnemyMigrationRow
        {
            public string Id;
            public string StageId;
            public int SortOrder;
            public string DisplayName;
            public EnemyEncounterRole EncounterRole;
            public EnemyAttackType AttackType;
            public DamageAttribute Attribute;
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
            public string SkillSlotAId;
            public string SkillSlotBId;
            public string PassiveId;
            public float NexusDamage;
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
            public EnemyPassiveTarget PassiveApplyTarget = EnemyPassiveTarget.Self;
            public EnemyPassiveModifierKind PassiveModifierKind;
            public float PassiveModifierValue;
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
                SortOrder = record.ReadInt("sort_order"),
                DisplayName = record.ReadRequiredString("display_name"),
                EncounterRole = record.ReadEnum<EnemyEncounterRole>("encounter_role"),
                AttackType = record.ReadEnum<EnemyAttackType>("attack_type"),
                Attribute = record.ReadEnum<DamageAttribute>("attribute"),
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
                SkillSlotAId = record.ReadRequiredString("skill_slot_a_id"),
                SkillSlotBId = record.ReadRequiredString("skill_slot_b_id"),
                PassiveId = record.ReadRequiredString("passive_id"),
                NexusDamage = record.ReadFloat("nexus_damage")
            };
        }

        private static EnemyBaseSkillRow ParseEnemyBaseSkillRow(CsvRecord record, string tableName)
        {
            if (string.Equals(tableName, "skills_passive.csv", StringComparison.OrdinalIgnoreCase))
            {
                return ParseEnemyPassiveSkillRow(record);
            }

            if (record.HasColumn("runtime_hitbox_offset_x") || record.HasColumn("runtime_hitbox_offset_y"))
            {
                throw new CsvFatalException(
                    $"CSV table '{tableName}' must not define runtime hitbox offset columns. Enemy runtime hitboxes are centered at (0,0).");
            }

            var row = new EnemyBaseSkillRow
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

            if (row.Skill.SkillKind == PakuriCsvSkillKind.Passive
                || row.Skill.RuntimeKind == SkillRuntimeKind.Passive)
            {
                throw new CsvFatalException(
                    $"CSV table '{tableName}' contains passive skill '{row.Skill.Id}'. Enemy passive rows must be authored in 'skills_passive.csv'.");
            }

            return row;
        }

        private static EnemyBaseSkillRow ParseEnemyPassiveSkillRow(CsvRecord record)
        {
            return new EnemyBaseSkillRow
            {
                Skill = new SkillRow
                {
                    Id = record.ReadRequiredString("skill_id"),
                    MonsterId = "enemy-shared",
                    SkillKind = PakuriCsvSkillKind.Passive,
                    Slot = SkillSlot.F,
                    DisplayName = record.ReadRequiredString("display_name"),
                    RuntimeKind = SkillRuntimeKind.Passive,
                    ImplementationState = SkillImplementationState.RuntimeImplemented,
                    IsAvailableWithoutActiveRequirement = true
                },
                PassiveApplyTarget = record.ReadEnum<EnemyPassiveTarget>("apply_target"),
                PassiveModifierKind = record.ReadEnum<EnemyPassiveModifierKind>("modifier_kind"),
                PassiveModifierValue = record.ReadFloat("modifier_value")
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
            var referencedActiveSkillIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var referencedPassiveIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var stageSortKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var enemy in model.Enemies.Values)
            {
                if (!string.Equals(enemy.StageId, "stage_one", StringComparison.OrdinalIgnoreCase)
                    && !string.Equals(enemy.StageId, "stage_two", StringComparison.OrdinalIgnoreCase))
                {
                    errors.Add($"Enemy '{enemy.Id}' has unsupported stage_id '{enemy.StageId}'.");
                }

                if (enemy.SortOrder < 0)
                {
                    errors.Add($"Enemy '{enemy.Id}' has negative sort_order '{enemy.SortOrder}'.");
                }
                else if (!stageSortKeys.Add(enemy.StageId + ":" + enemy.SortOrder))
                {
                    errors.Add($"Enemy stage '{enemy.StageId}' has duplicate sort_order '{enemy.SortOrder}'.");
                }

                ValidateEnemySkillSlot(model, enemy, enemy.SkillSlotAId, SkillSlot.A, referencedActiveSkillIds, errors);
                ValidateEnemySkillSlot(model, enemy, enemy.SkillSlotBId, SkillSlot.B, referencedActiveSkillIds, errors);
                ValidateEnemyPassive(model, enemy, referencedPassiveIds, errors);
            }

            foreach (var baseSkill in model.EnemyBaseSkills.Values)
            {
                if (baseSkill == null || baseSkill.Skill == null)
                {
                    continue;
                }

                if (baseSkill.Skill.SkillKind == PakuriCsvSkillKind.Passive)
                {
                    if (!referencedPassiveIds.Contains(baseSkill.Skill.Id))
                    {
                        errors.Add($"Enemy passive skill '{baseSkill.Skill.Id}' is not referenced by enemies.csv passive_id.");
                    }
                }
                else if (!referencedActiveSkillIds.Contains(baseSkill.Skill.Id))
                {
                    errors.Add($"Enemy base skill '{baseSkill.Skill.Id}' is not referenced by an Enemy A/B skill slot.");
                }
            }

            foreach (var trigger in model.EnemyMigrationTriggers.Values)
            {
                if (!model.EnemyBaseSkills.TryGetValue(trigger.SourceSkillId, out var sourceSkill)
                    || sourceSkill == null
                    || sourceSkill.Skill == null)
                {
                    errors.Add($"Enemy trigger '{trigger.Id}' references unknown source skill '{trigger.SourceSkillId}'.");
                }
                else if (trigger.RuntimeKind != sourceSkill.Skill.RuntimeKind)
                {
                    errors.Add(
                        $"Enemy trigger '{trigger.Id}' runtime_kind '{trigger.RuntimeKind}' does not match source skill kind '{sourceSkill.Skill.RuntimeKind}'.");
                }

                if (!model.EnemyBaseSkills.ContainsKey(trigger.TriggeredSkillId))
                {
                    errors.Add($"Enemy trigger '{trigger.Id}' references unknown triggered skill '{trigger.TriggeredSkillId}'.");
                }
            }

            ValidateEnemyCombatStartTrigger(model, "OpeningCharge", SkillRuntimeKind.Buff, errors);
            ValidateEnemyCombatStartTrigger(model, "Intimidation", SkillRuntimeKind.Buff, errors);
        }

        private static void ValidateEnemySkillSlot(
            SourceModel model,
            EnemyMigrationRow enemy,
            string skillId,
            SkillSlot slot,
            HashSet<string> referencedSkillIds,
            List<string> errors)
        {
            if (!model.EnemyBaseSkills.TryGetValue(skillId, out var skill)
                || skill == null
                || skill.Skill == null)
            {
                errors.Add($"Enemy '{enemy.Id}' slot '{slot}' references unknown base skill '{skillId}'.");
                return;
            }

            if (skill.Skill.SkillKind != PakuriCsvSkillKind.Active
                || skill.Skill.RuntimeKind == SkillRuntimeKind.Passive)
            {
                errors.Add($"Enemy '{enemy.Id}' slot '{slot}' must reference an active skill, but '{skillId}' is passive.");
                return;
            }

            referencedSkillIds.Add(skillId);
        }

        private static void ValidateEnemyPassive(
            SourceModel model,
            EnemyMigrationRow enemy,
            HashSet<string> referencedPassiveIds,
            List<string> errors)
        {
            var passiveId = enemy.PassiveId != null ? enemy.PassiveId.Trim() : string.Empty;
            if (!model.EnemyBaseSkills.TryGetValue(passiveId, out var passive)
                || passive == null
                || passive.Skill == null)
            {
                errors.Add($"Enemy '{enemy.Id}' references unknown passive_id '{passiveId}'.");
                return;
            }

            if (passive.Skill.SkillKind != PakuriCsvSkillKind.Passive
                || passive.Skill.RuntimeKind != SkillRuntimeKind.Passive
                || passive.Skill.Slot != SkillSlot.F)
            {
                errors.Add($"Enemy '{enemy.Id}' passive_id '{passiveId}' must reference an Enemy passive definition.");
            }

            if (passive.PassiveApplyTarget != EnemyPassiveTarget.Self)
            {
                errors.Add($"Enemy passive '{passiveId}' has unsupported apply_target '{passive.PassiveApplyTarget}'.");
            }

            if (passive.PassiveModifierKind == EnemyPassiveModifierKind.None)
            {
                errors.Add($"Enemy passive '{passiveId}' requires a supported modifier_kind.");
            }

            if (passive.PassiveModifierValue <= 0f)
            {
                errors.Add($"Enemy passive '{passiveId}' requires a positive modifier_value.");
            }

            referencedPassiveIds.Add(passiveId);
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

    }
}
