using System.Collections.Generic;
using Pakuri.Combat;
using Pakuri.Data;
using UnityEngine;

namespace Pakuri.InGame
{
    public sealed class EnemyCombatSystem
    {
        private readonly Dictionary<string, EnemyCombatState> enemyStates = new Dictionary<string, EnemyCombatState>();

        public int LastAttackAttemptCount { get; private set; }

        public void Clear()
        {
            enemyStates.Clear();
            LastAttackAttemptCount = 0;
        }

        public void Tick(UnitRosterService roster, float deltaTime, bool logAttackAttempts)
        {
            Tick(roster, null, deltaTime, logAttackAttempts);
        }

        public void Tick(
            UnitRosterService roster,
            InGameCombatManager combatManager,
            float deltaTime,
            bool logAttackAttempts)
        {
            LastAttackAttemptCount = 0;

            if (roster == null || deltaTime <= 0f)
            {
                return;
            }

            var enemies = roster.Enemies;
            for (var i = 0; i < enemies.Count; i++)
            {
                TickEnemy(enemies[i], roster, combatManager, deltaTime, logAttackAttempts);
            }
        }

        private void TickEnemy(
            UnitRosterEntry enemyEntry,
            UnitRosterService roster,
            InGameCombatManager combatManager,
            float deltaTime,
            bool logAttackAttempts)
        {
            if (!EnemyTargeting.IsActive(enemyEntry))
            {
                return;
            }

            var enemyModel = enemyEntry.Model as EnemyUnitRuntimeModel;
            if (enemyModel == null || !enemyModel.AutoAttackEnabled)
            {
                return;
            }

            EnemyCombatRules.TickTemporaryEnemyModifiers(enemyModel, deltaTime);

            var state = GetState(enemyModel);
            var actionDeltaTime = deltaTime * StatusEffectRuntime.ResolveActionSpeedMultiplier(enemyModel);
            EnemyCombatRules.TickEnemyCooldowns(state, actionDeltaTime);

            var target = EnemyTargeting.FindNearestPlayerTarget(enemyEntry, roster);
            if (target != null)
            {
                state.TargetUnitId = target.Model != null && target.Model.Identity != null
                    ? target.Model.Identity.UnitId
                    : null;
            }

            var specialSkill = EnemyCombatRules.ResolveSpecialSkill(enemyModel);
            var canAct = StatusEffectRuntime.CanAct(enemyModel);
            var canUseSpecialSkill = canAct && StatusEffectRuntime.CanUseSpecialSkill(enemyModel);
            var executedSupportSkill = canUseSpecialSkill && TryExecuteCooldownDrivenSpecialSkill(
                enemyEntry,
                enemyModel,
                roster,
                combatManager,
                specialSkill,
                state,
                logAttackAttempts,
                target);

            if (target == null)
            {
                return;
            }

            var offensiveSkill = EnemyCombatRules.ResolvePreferredOffensiveSkill(enemyModel, state, specialSkill);
            if (offensiveSkill.SlotType == EnemySkillSlotType.Special && !canUseSpecialSkill)
            {
                offensiveSkill = EnemyCombatRules.ResolveBasicSkill(enemyModel);
            }

            if (!offensiveSkill.IsAssigned)
            {
                return;
            }

            var distance = Vector2.Distance(enemyEntry.Transform.position, target.Transform.position);
            var attackRange = EnemyCombatRules.ResolveAttackAttemptRange(enemyModel, offensiveSkill);
            if (distance > attackRange)
            {
                if (StatusEffectRuntime.CanMove(enemyModel))
                {
                    MoveToward(enemyEntry, target, enemyModel, deltaTime);
                }

                return;
            }

            if (!canAct || executedSupportSkill || !EnemyCombatRules.IsSkillReady(state, offensiveSkill.SlotType))
            {
                return;
            }

            EnemyCombatRules.SetSkillCooldown(state, offensiveSkill);
            state.AttackAttemptCount++;
            LastAttackAttemptCount++;
            EnemySkillExecutor.Execute(enemyEntry, enemyModel, target, roster, combatManager, offensiveSkill);

            if (logAttackAttempts)
            {
                Debug.Log(BuildAttackAttemptLog(enemyModel, target, offensiveSkill.SkillKind));
            }
        }

        private bool TryExecuteCooldownDrivenSpecialSkill(
            UnitRosterEntry enemyEntry,
            EnemyUnitRuntimeModel enemyModel,
            UnitRosterService roster,
            InGameCombatManager combatManager,
            EnemyResolvedSkillData specialSkill,
            EnemyCombatState state,
            bool logAttackAttempts,
            UnitRosterEntry target)
        {
            if (!specialSkill.IsAssigned
                || !EnemyCombatRules.IsCooldownDrivenSelfOrAllySkill(specialSkill.SkillKind)
                || !EnemyCombatRules.IsSkillReady(state, EnemySkillSlotType.Special)
                || !EnemyCombatRules.CanExecuteCooldownDrivenSelfOrAllySkill(specialSkill.SkillKind, roster))
            {
                return false;
            }

            EnemyCombatRules.SetSkillCooldown(state, specialSkill);
            state.AttackAttemptCount++;
            LastAttackAttemptCount++;
            EnemySkillExecutor.Execute(enemyEntry, enemyModel, target, roster, combatManager, specialSkill);

            if (logAttackAttempts)
            {
                Debug.Log(BuildAttackAttemptLog(enemyModel, target, specialSkill.SkillKind));
            }

            return true;
        }

        private static void MoveToward(
            UnitRosterEntry enemyEntry,
            UnitRosterEntry target,
            EnemyUnitRuntimeModel enemyModel,
            float deltaTime)
        {
            var moveSpeed = enemyModel.Stats != null ? Mathf.Max(0f, enemyModel.Stats.MoveSpeed) : 0f;
            moveSpeed *= EnemyCombatRules.ResolveMoveSpeedMultiplier(enemyModel);
            moveSpeed *= StatusEffectRuntime.ResolveMoveSpeedMultiplier(enemyModel);
            if (moveSpeed <= 0f)
            {
                return;
            }

            var current = enemyEntry.Transform.position;
            var targetPosition = target.Transform.position;
            targetPosition.z = current.z;
            enemyEntry.Transform.position = Vector3.MoveTowards(current, targetPosition, moveSpeed * deltaTime);
        }

        private EnemyCombatState GetState(EnemyUnitRuntimeModel enemyModel)
        {
            var unitId = enemyModel.Identity != null ? enemyModel.Identity.UnitId : null;
            if (string.IsNullOrWhiteSpace(unitId))
            {
                unitId = "enemy-unknown";
            }

            if (!enemyStates.TryGetValue(unitId, out var state))
            {
                state = new EnemyCombatState();
                enemyStates.Add(unitId, state);
            }

            return state;
        }

        private static string BuildAttackAttemptLog(
            EnemyUnitRuntimeModel enemyModel,
            UnitRosterEntry target,
            StageOneEnemySkillKind skillKind)
        {
            var enemyName = enemyModel.Identity != null && !string.IsNullOrWhiteSpace(enemyModel.Identity.DisplayName)
                ? enemyModel.Identity.DisplayName
                : enemyModel.Identity != null ? enemyModel.Identity.DefinitionId : "enemy";
            var targetName = target != null
                && target.Model != null
                && target.Model.Identity != null
                && !string.IsNullOrWhiteSpace(target.Model.Identity.DisplayName)
                    ? target.Model.Identity.DisplayName
                    : target != null && target.Model != null && target.Model.Identity != null ? target.Model.Identity.DefinitionId : "target";

            return $"Enemy skill attempt: {enemyName} -> {targetName} ({skillKind})";
        }
    }

    public sealed class EnemyCombatState
    {
        public string TargetUnitId;
        public float BasicSkillCooldownRemaining;
        public float SpecialSkillCooldownRemaining;
        public int AttackAttemptCount;
    }

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

    internal static class EnemyCombatRules
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

    internal static class EnemySkillExecutor
    {
        public static void Execute(
            UnitRosterEntry enemyEntry,
            EnemyUnitRuntimeModel enemyModel,
            UnitRosterEntry target,
            UnitRosterService roster,
            InGameCombatManager combatManager,
            EnemyResolvedSkillData skillData)
        {
            if (combatManager == null || enemyEntry == null || enemyModel == null || roster == null || !skillData.IsAssigned)
            {
                return;
            }

            switch (skillData.SkillKind)
            {
                case StageOneEnemySkillKind.Slash:
                    ExecuteSlash(enemyEntry, enemyModel, target, combatManager, skillData);
                    break;
                case StageOneEnemySkillKind.ShurikenThrow:
                    ExecuteShuriken(enemyEntry, enemyModel, target, combatManager, skillData);
                    break;
                case StageOneEnemySkillKind.Heal:
                    ExecuteHeal(enemyModel, roster, combatManager, skillData);
                    break;
                case StageOneEnemySkillKind.ShieldUp:
                    ExecuteShieldUp(enemyEntry, enemyModel, combatManager, skillData);
                    break;
                case StageOneEnemySkillKind.AimedShot:
                    ExecuteAimedShot(enemyEntry, enemyModel, target, combatManager, skillData);
                    break;
                case StageOneEnemySkillKind.GuardianFlag:
                    ExecuteGuardianFlag(enemyEntry, enemyModel, roster, combatManager, skillData);
                    break;
                case StageOneEnemySkillKind.ChargeCommand:
                    ExecuteChargeCommand(enemyEntry, enemyModel, roster, combatManager, skillData);
                    break;
                case StageOneEnemySkillKind.SacredSwordWave:
                    ExecuteSacredSwordWave(enemyEntry, enemyModel, target, combatManager, skillData);
                    break;
            }
        }

        private static void ExecuteSlash(
            UnitRosterEntry enemyEntry,
            EnemyUnitRuntimeModel enemyModel,
            UnitRosterEntry target,
            InGameCombatManager combatManager,
            EnemyResolvedSkillData skillData)
        {
            if (target == null || target.Transform == null || enemyEntry.Transform == null)
            {
                return;
            }

            var damage = ResolveAttackDamage(enemyModel, skillData);
            var radius = Mathf.Max(0.1f, skillData.Radius);
            var direction = target.Transform.position - enemyEntry.Transform.position;
            direction.z = 0f;
            if (direction.sqrMagnitude <= 0.0001f)
            {
                direction = Vector3.left;
            }

            var effects = combatManager.Effects;
            var prefab = effects != null ? effects.ResolveEnemySkillEffectPrefab(enemyModel, skillData.SkillKind) : null;
            if (prefab == null)
            {
                combatManager.ApplyDamage(target.Model, damage, enemyModel.Attribute, enemyModel, true);
                return;
            }

            var origin = enemyEntry.Transform.position + direction.normalized * Mathf.Min(radius * 0.5f, 0.75f);
            var instance = effects.InstantiateSkillPrefab(prefab, origin, ResolveRotation(direction));
            if (instance == null)
            {
                combatManager.ApplyDamage(target.Model, damage, enemyModel.Attribute, enemyModel, true);
                return;
            }

            var actor = instance.GetComponent<InGameEnemySkillHitboxActor>();
            if (actor == null)
            {
                actor = instance.AddComponent<InGameEnemySkillHitboxActor>();
            }

            actor.Initialize(combatManager, enemyModel, damage, enemyModel.Attribute, radius, 0.35f);
        }

        private static void ExecuteShuriken(
            UnitRosterEntry enemyEntry,
            EnemyUnitRuntimeModel enemyModel,
            UnitRosterEntry target,
            InGameCombatManager combatManager,
            EnemyResolvedSkillData skillData)
        {
            ExecuteEnemyProjectile(enemyEntry, enemyModel, target, combatManager, skillData, 9f, 2.5f);
        }

        private static void ExecuteAimedShot(
            UnitRosterEntry enemyEntry,
            EnemyUnitRuntimeModel enemyModel,
            UnitRosterEntry target,
            InGameCombatManager combatManager,
            EnemyResolvedSkillData skillData)
        {
            ExecuteEnemyProjectile(enemyEntry, enemyModel, target, combatManager, skillData, 10f, 2.5f);
        }

        private static void ExecuteEnemyProjectile(
            UnitRosterEntry enemyEntry,
            EnemyUnitRuntimeModel enemyModel,
            UnitRosterEntry target,
            InGameCombatManager combatManager,
            EnemyResolvedSkillData skillData,
            float projectileSpeed,
            float lifetimeSeconds)
        {
            if (target == null || target.Transform == null || enemyEntry.Transform == null)
            {
                return;
            }

            var damage = ResolveAttackDamage(enemyModel, skillData);
            var direction = target.Transform.position - enemyEntry.Transform.position;
            direction.z = 0f;
            var effects = combatManager.Effects;
            var prefab = effects != null ? effects.ResolveEnemySkillEffectPrefab(enemyModel, skillData.SkillKind) : null;
            if (prefab == null)
            {
                combatManager.ApplyDamage(target.Model, damage, enemyModel.Attribute, enemyModel, true);
                return;
            }

            var instance = effects.InstantiateSkillPrefab(prefab, enemyEntry.Transform.position, ResolveRotation(direction));
            if (instance == null)
            {
                combatManager.ApplyDamage(target.Model, damage, enemyModel.Attribute, enemyModel, true);
                return;
            }

            var actor = instance.GetComponent<InGameProjectileActor>();
            if (actor == null)
            {
                actor = instance.AddComponent<InGameProjectileActor>();
            }

            actor.Initialize(
                combatManager,
                enemyModel,
                direction,
                ResolveEnemyProjectileSpeed(skillData, projectileSpeed),
                damage,
                enemyModel.Attribute,
                0,
                ResolveEnemyProjectileBoundaryX(enemyEntry.Transform.position, direction),
                ResolveEnemyProjectileLifetime(skillData, lifetimeSeconds),
                null,
                null,
                null,
                null,
                null,
                null,
                false,
                true);
        }

        private static void ExecuteHeal(
            EnemyUnitRuntimeModel enemyModel,
            UnitRosterService roster,
            InGameCombatManager combatManager,
            EnemyResolvedSkillData skillData)
        {
            var target = EnemyTargeting.FindLowestHealthEnemyAlly(roster);
            if (target == null)
            {
                return;
            }

            var healAmount = Mathf.Max(
                0f,
                (skillData.FlatValue + ResolveSpellPower(enemyModel) * Mathf.Max(0f, skillData.Coefficient))
                * Mathf.Max(0f, enemyModel.PassiveHealingMultiplier));
            combatManager.Heal(target.Model, healAmount);

            var effects = combatManager.Effects;
            var prefab = effects != null ? effects.ResolveEnemySkillEffectPrefab(enemyModel, skillData.SkillKind) : null;
            if (prefab != null && target.Transform != null && effects != null)
            {
                var instance = effects.InstantiateSkillPrefab(prefab, target.Transform.position, Quaternion.identity);
                if (instance != null)
                {
                    var actor = instance.GetComponent<InGameAttachedSkillEffectActor>();
                    if (actor == null)
                    {
                        actor = instance.AddComponent<InGameAttachedSkillEffectActor>();
                    }

                    actor.Initialize(target.Transform, 0.8f, Vector3.zero);
                }
            }
        }

        private static void ExecuteShieldUp(
            UnitRosterEntry enemyEntry,
            EnemyUnitRuntimeModel enemyModel,
            InGameCombatManager combatManager,
            EnemyResolvedSkillData skillData)
        {
            enemyModel.IncomingDamageMultiplier = Mathf.Clamp01(1f - Mathf.Max(0f, skillData.FlatValue));
            enemyModel.IncomingDamageMultiplierRemainingSeconds = Mathf.Max(0.1f, skillData.Duration);
            SpawnAttachedEnemySkillEffect(enemyEntry, enemyModel, combatManager, skillData, skillData.Duration);
        }

        private static void ExecuteGuardianFlag(
            UnitRosterEntry enemyEntry,
            EnemyUnitRuntimeModel enemyModel,
            UnitRosterService roster,
            InGameCombatManager combatManager,
            EnemyResolvedSkillData skillData)
        {
            var allies = EnemyTargeting.FindEnemyAlliesInRadius(enemyEntry, roster, Mathf.Max(0f, skillData.Radius));
            var shield = Mathf.Max(0f, skillData.FlatValue);
            for (var i = 0; i < allies.Count; i++)
            {
                combatManager.GrantShield(allies[i].Model, shield);
            }

            SpawnAttachedEnemySkillEffect(enemyEntry, enemyModel, combatManager, skillData, skillData.Duration);
        }

        private static void ExecuteChargeCommand(
            UnitRosterEntry enemyEntry,
            EnemyUnitRuntimeModel enemyModel,
            UnitRosterService roster,
            InGameCombatManager combatManager,
            EnemyResolvedSkillData skillData)
        {
            var allies = EnemyTargeting.FindEnemyAlliesInRadius(enemyEntry, roster, Mathf.Max(0f, skillData.Radius));
            var duration = Mathf.Max(0.1f, skillData.Duration);
            for (var i = 0; i < allies.Count; i++)
            {
                var ally = allies[i].Model as EnemyUnitRuntimeModel;
                if (ally == null)
                {
                    continue;
                }

                ally.MoveSpeedMultiplier = Mathf.Max(0f, skillData.MoveSpeedMultiplier);
                ally.MoveSpeedMultiplierRemainingSeconds = duration;
                ally.OutgoingDamageMultiplier = Mathf.Max(0f, skillData.OutgoingDamageMultiplier);
                ally.OutgoingDamageMultiplierRemainingSeconds = duration;
            }

            SpawnAttachedEnemySkillEffect(enemyEntry, enemyModel, combatManager, skillData, duration);
        }

        private static void ExecuteSacredSwordWave(
            UnitRosterEntry enemyEntry,
            EnemyUnitRuntimeModel enemyModel,
            UnitRosterEntry target,
            InGameCombatManager combatManager,
            EnemyResolvedSkillData skillData)
        {
            ExecuteEnemyProjectile(enemyEntry, enemyModel, target, combatManager, skillData, 12f, 4f);
        }

        private static float ResolveAttackDamage(EnemyUnitRuntimeModel enemyModel, EnemyResolvedSkillData skillData)
        {
            var attribute = enemyModel != null ? enemyModel.Attribute : DamageAttribute.Physical;
            return Mathf.Max(0f, ResolveAttackPower(enemyModel) * Mathf.Max(0f, skillData.Coefficient) * ResolveOutgoingDamageMultiplier(enemyModel, attribute));
        }

        private static float ResolveAttackPower(EnemyUnitRuntimeModel enemyModel)
        {
            return enemyModel != null && enemyModel.Stats != null ? enemyModel.Stats.AttackPower : 0f;
        }

        private static float ResolveSpellPower(EnemyUnitRuntimeModel enemyModel)
        {
            return enemyModel != null && enemyModel.Stats != null ? enemyModel.Stats.SpellPower : 0f;
        }

        private static float ResolveOutgoingDamageMultiplier(EnemyUnitRuntimeModel enemyModel, DamageAttribute attribute)
        {
            if (enemyModel == null)
            {
                return 1f;
            }

            var multiplier = Mathf.Max(0f, enemyModel.PassiveOutgoingDamageMultiplier);
            if (attribute == DamageAttribute.Physical)
            {
                multiplier *= Mathf.Max(0f, enemyModel.PassivePhysicalDamageMultiplier);
            }

            if (enemyModel.OutgoingDamageMultiplierRemainingSeconds > 0f)
            {
                multiplier *= Mathf.Max(0f, enemyModel.OutgoingDamageMultiplier);
            }

            return multiplier;
        }

        private static float ResolveEnemyProjectileSpeed(EnemyResolvedSkillData skillData, float fallbackSpeed)
        {
            return skillData.ProjectileSpeed > 0f
                ? skillData.ProjectileSpeed
                : fallbackSpeed;
        }

        private static float ResolveEnemyProjectileLifetime(EnemyResolvedSkillData skillData, float fallbackLifetime)
        {
            return skillData.ProjectileLifetime > 0f
                ? skillData.ProjectileLifetime
                : fallbackLifetime;
        }

        private static void SpawnAttachedEnemySkillEffect(
            UnitRosterEntry target,
            EnemyUnitRuntimeModel enemyModel,
            InGameCombatManager combatManager,
            EnemyResolvedSkillData skillData,
            float duration)
        {
            if (target == null || target.Transform == null || combatManager == null)
            {
                return;
            }

            var effects = combatManager.Effects;
            var prefab = effects != null ? effects.ResolveEnemySkillEffectPrefab(enemyModel, skillData.SkillKind) : null;
            if (prefab == null)
            {
                return;
            }

            var instance = effects.InstantiateSkillPrefab(prefab, target.Transform.position, Quaternion.identity);
            if (instance == null)
            {
                return;
            }

            var actor = instance.GetComponent<InGameAttachedSkillEffectActor>();
            if (actor == null)
            {
                actor = instance.AddComponent<InGameAttachedSkillEffectActor>();
            }

            actor.Initialize(target.Transform, Mathf.Max(0.1f, duration), Vector3.zero);
        }

        private static Quaternion ResolveRotation(Vector3 direction)
        {
            if (direction.sqrMagnitude <= 0.0001f)
            {
                return Quaternion.identity;
            }

            var angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            return Quaternion.Euler(0f, 0f, angle);
        }

        private static float ResolveEnemyProjectileBoundaryX(Vector3 origin, Vector3 direction)
        {
            var normalized = direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector3.left;
            return origin.x + normalized.x * 40f;
        }
    }
}
