using Pakuri.Data;
using UnityEngine;

namespace Pakuri.InGame
{
    internal enum EnemySkillSlotType
    {
        None,
        Basic,
        Special
    }

    internal struct EnemyResolvedSkillData
    {
        public EnemySkillSlotType SlotType;
        public StageOneEnemySkillKind SkillKind;
        public float Coefficient;
        public float CooldownSeconds;
        public float Duration;
        public float Radius;
        public float FlatValue;
        public float ProjectileSpeed;
        public float ProjectileLifetime;
        public float MoveSpeedMultiplier;
        public float OutgoingDamageMultiplier;
        public bool IsAssigned;
    }

    internal static class EnemySkillCooldown
    {
        public static EnemyResolvedSkillData ResolveSpecialSkill(EnemyUnitRuntimeModel enemyModel)
        {
            if (enemyModel == null)
            {
                return default;
            }

            return new EnemyResolvedSkillData
            {
                SlotType = EnemySkillSlotType.Special,
                SkillKind = enemyModel.StageOneSkill,
                Coefficient = enemyModel.ActiveSkillCoefficient,
                CooldownSeconds = enemyModel.ActiveSkillCooldownSeconds,
                Duration = enemyModel.ActiveSkillDuration,
                Radius = enemyModel.ActiveSkillRadius,
                FlatValue = enemyModel.ActiveSkillFlatValue,
                ProjectileSpeed = enemyModel.ActiveSkillProjectileSpeed,
                ProjectileLifetime = enemyModel.ActiveSkillProjectileLifetime,
                MoveSpeedMultiplier = enemyModel.ActiveSkillMoveSpeedMultiplier,
                OutgoingDamageMultiplier = enemyModel.ActiveSkillOutgoingDamageMultiplier,
                IsAssigned = true
            };
        }

        public static EnemyResolvedSkillData ResolveBasicSkill(EnemyUnitRuntimeModel enemyModel)
        {
            if (enemyModel == null || !enemyModel.HasBasicSkill || enemyModel.BasicSkill == enemyModel.StageOneSkill)
            {
                return default;
            }

            return new EnemyResolvedSkillData
            {
                SlotType = EnemySkillSlotType.Basic,
                SkillKind = enemyModel.BasicSkill,
                Coefficient = enemyModel.BasicSkillCoefficient,
                CooldownSeconds = enemyModel.BasicSkillCooldownSeconds,
                Duration = enemyModel.BasicSkillDuration,
                Radius = enemyModel.BasicSkillRadius,
                FlatValue = enemyModel.BasicSkillFlatValue,
                ProjectileSpeed = enemyModel.BasicSkillProjectileSpeed,
                ProjectileLifetime = enemyModel.BasicSkillProjectileLifetime,
                MoveSpeedMultiplier = enemyModel.BasicSkillMoveSpeedMultiplier,
                OutgoingDamageMultiplier = enemyModel.BasicSkillOutgoingDamageMultiplier,
                IsAssigned = true
            };
        }

        public static EnemyResolvedSkillData ResolvePreferredOffensiveSkill(
            EnemyUnitRuntimeModel enemyModel,
            EnemyCombatState state,
            EnemyResolvedSkillData specialSkill)
        {
            var basicSkill = ResolveBasicSkill(enemyModel);
            var specialIsOffensive = IsOffensiveSkill(specialSkill);
            var basicIsOffensive = IsOffensiveSkill(basicSkill);

            if (specialIsOffensive && IsSkillReady(state, EnemySkillSlotType.Special))
            {
                return specialSkill;
            }

            if (basicIsOffensive && IsSkillReady(state, EnemySkillSlotType.Basic))
            {
                return basicSkill;
            }

            if (basicIsOffensive)
            {
                return basicSkill;
            }

            if (specialIsOffensive)
            {
                return specialSkill;
            }

            return default;
        }

        public static float ResolveAttackAttemptRange(EnemyUnitRuntimeModel enemyModel, EnemyResolvedSkillData skillData)
        {
            if (skillData.Radius > 0f)
            {
                return Mathf.Max(0.1f, skillData.Radius);
            }

            switch (enemyModel.AttackType)
            {
                case EnemyAttackType.Ranged:
                    return 5f;
                case EnemyAttackType.MeleeAndRanged:
                    return 4f;
                case EnemyAttackType.Buffer:
                    return 5f;
                default:
                    return 1.4f;
            }
        }

        public static void TickEnemyCooldowns(EnemyCombatState state, float deltaTime)
        {
            state.BasicSkillCooldownRemaining = Mathf.Max(0f, state.BasicSkillCooldownRemaining - deltaTime);
            state.SpecialSkillCooldownRemaining = Mathf.Max(0f, state.SpecialSkillCooldownRemaining - deltaTime);
        }

        public static bool IsSkillReady(EnemyCombatState state, EnemySkillSlotType slotType)
        {
            switch (slotType)
            {
                case EnemySkillSlotType.Basic:
                    return state.BasicSkillCooldownRemaining <= 0f;
                case EnemySkillSlotType.Special:
                    return state.SpecialSkillCooldownRemaining <= 0f;
                default:
                    return false;
            }
        }

        public static void SetSkillCooldown(EnemyCombatState state, EnemyResolvedSkillData skillData)
        {
            var cooldown = Mathf.Max(0.1f, skillData.CooldownSeconds);
            if (skillData.SlotType == EnemySkillSlotType.Basic)
            {
                state.BasicSkillCooldownRemaining = cooldown;
            }
            else if (skillData.SlotType == EnemySkillSlotType.Special)
            {
                state.SpecialSkillCooldownRemaining = cooldown;
            }
        }

        public static bool IsCooldownDrivenSelfOrAllySkill(StageOneEnemySkillKind skillKind)
        {
            switch (skillKind)
            {
                case StageOneEnemySkillKind.Heal:
                case StageOneEnemySkillKind.ShieldUp:
                case StageOneEnemySkillKind.GuardianFlag:
                case StageOneEnemySkillKind.ChargeCommand:
                    return true;
                default:
                    return false;
            }
        }

        public static bool CanExecuteCooldownDrivenSelfOrAllySkill(
            StageOneEnemySkillKind skillKind,
            UnitRosterService roster)
        {
            return skillKind != StageOneEnemySkillKind.Heal || EnemyTargeting.FindLowestHealthEnemyAlly(roster) != null;
        }

        public static void TickTemporaryEnemyModifiers(EnemyUnitRuntimeModel enemyModel, float deltaTime)
        {
            TickTemporaryMultiplier(ref enemyModel.IncomingDamageMultiplierRemainingSeconds, ref enemyModel.IncomingDamageMultiplier, deltaTime);
            TickTemporaryMultiplier(ref enemyModel.OutgoingDamageMultiplierRemainingSeconds, ref enemyModel.OutgoingDamageMultiplier, deltaTime);
            TickTemporaryMultiplier(ref enemyModel.MoveSpeedMultiplierRemainingSeconds, ref enemyModel.MoveSpeedMultiplier, deltaTime);
        }

        public static float ResolveMoveSpeedMultiplier(EnemyUnitRuntimeModel enemyModel)
        {
            return enemyModel != null && enemyModel.MoveSpeedMultiplierRemainingSeconds > 0f
                ? Mathf.Max(0f, enemyModel.MoveSpeedMultiplier)
                : 1f;
        }

        private static bool IsOffensiveSkill(EnemyResolvedSkillData skillData)
        {
            return skillData.IsAssigned && !IsCooldownDrivenSelfOrAllySkill(skillData.SkillKind);
        }

        private static void TickTemporaryMultiplier(ref float remainingSeconds, ref float multiplier, float deltaTime)
        {
            if (remainingSeconds <= 0f)
            {
                multiplier = 1f;
                return;
            }

            remainingSeconds = Mathf.Max(0f, remainingSeconds - deltaTime);
            if (remainingSeconds <= 0f)
            {
                multiplier = 1f;
            }
        }
    }
}
