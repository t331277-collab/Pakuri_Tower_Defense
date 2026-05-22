using System;
using System.Collections;
using System.Collections.Generic;
using Pakuri.Combat;
using Pakuri.Data;
using UnityEngine;

namespace Pakuri.InGame
{
    internal static class SkillTriggerRuntime
    {
        private readonly struct TriggerExecutionContext
        {
            public TriggerExecutionContext(
                BaseUnitRuntimeModel eventTarget,
                BaseUnitRuntimeModel attacker,
                Vector2 eventCenter,
                UnitStatusRuntime status,
                float shieldAbsorbedAmount)
            {
                EventTarget = eventTarget;
                Attacker = attacker;
                EventCenter = eventCenter;
                Status = status;
                ShieldAbsorbedAmount = shieldAbsorbedAmount;
            }

            public BaseUnitRuntimeModel EventTarget { get; }
            public BaseUnitRuntimeModel Attacker { get; }
            public Vector2 EventCenter { get; }
            public UnitStatusRuntime Status { get; }
            public float ShieldAbsorbedAmount { get; }
        }

        public static void ExecuteProjectileHit(
            InGameCombatManager combatManager,
            UnitRosterService roster,
            BaseUnitRuntimeModel source,
            string sourceSkillId,
            bool isMagazineLastProjectile,
            Vector2 eventCenter)
        {
            if (!isMagazineLastProjectile)
            {
                return;
            }

            ExecuteMatchingTriggers(
                combatManager,
                roster,
                source,
                sourceSkillId,
                SkillTriggerEvent.OnMagazineLastProjectileHit,
                new TriggerExecutionContext(source, null, eventCenter, null, 0f));
        }

        public static void ExecuteShieldExpire(
            InGameCombatManager combatManager,
            UnitRosterService roster,
            BaseUnitRuntimeModel shieldTarget,
            UnitStatusRuntime shieldStatus)
        {
            if (shieldTarget == null || shieldStatus == null || !shieldStatus.IsShieldStatus)
            {
                return;
            }

            var source = ResolveSourceModel(roster, shieldStatus.SourceUnitId, shieldStatus.SourceDefinitionId);
            var sourceSkillId = !string.IsNullOrWhiteSpace(shieldStatus.SourceSkillId)
                ? shieldStatus.SourceSkillId
                : string.Empty;
            var center = ResolveUnitPosition(roster, shieldTarget);
            ExecuteMatchingTriggers(
                combatManager,
                roster,
                source,
                sourceSkillId,
                SkillTriggerEvent.OnShieldExpire,
                new TriggerExecutionContext(shieldTarget, null, center, shieldStatus, 0f));
        }

        public static void ExecuteShieldAbsorb(
            InGameCombatManager combatManager,
            UnitRosterService roster,
            BaseUnitRuntimeModel shieldTarget,
            BaseUnitRuntimeModel attacker,
            UnitStatusRuntime shieldStatus,
            float absorbedAmount)
        {
            if (shieldTarget == null || shieldStatus == null || !shieldStatus.IsShieldStatus || absorbedAmount <= 0f)
            {
                return;
            }

            var source = ResolveSourceModel(roster, shieldStatus.SourceUnitId, shieldStatus.SourceDefinitionId);
            var sourceSkillId = !string.IsNullOrWhiteSpace(shieldStatus.SourceSkillId)
                ? shieldStatus.SourceSkillId
                : string.Empty;
            var center = attacker != null
                ? ResolveUnitPosition(roster, attacker)
                : ResolveUnitPosition(roster, shieldTarget);
            ExecuteMatchingTriggers(
                combatManager,
                roster,
                source,
                sourceSkillId,
                SkillTriggerEvent.OnShieldAbsorb,
                new TriggerExecutionContext(attacker, attacker, center, shieldStatus, absorbedAmount));
        }

        public static void ExecuteStatusExpire(
            InGameCombatManager combatManager,
            UnitRosterService roster,
            BaseUnitRuntimeModel statusOwner,
            UnitStatusRuntime status)
        {
            if (statusOwner == null || status == null)
            {
                return;
            }

            var source = ResolveSourceModel(roster, status.SourceUnitId, status.SourceDefinitionId);
            var sourceSkillId = !string.IsNullOrWhiteSpace(status.SourceSkillId)
                ? status.SourceSkillId
                : string.Empty;
            var center = ResolveUnitPosition(roster, statusOwner);
            ExecuteMatchingTriggers(
                combatManager,
                roster,
                source,
                sourceSkillId,
                SkillTriggerEvent.OnStatusExpire,
                new TriggerExecutionContext(statusOwner, null, center, status, 0f));
        }

        private static void ExecuteMatchingTriggers(
            InGameCombatManager combatManager,
            UnitRosterService roster,
            BaseUnitRuntimeModel source,
            string sourceSkillId,
            SkillTriggerEvent triggerEvent,
            TriggerExecutionContext triggerContext)
        {
            if (combatManager == null || roster == null || source == null || string.IsNullOrWhiteSpace(sourceSkillId))
            {
                return;
            }

            var monsterId = source.Identity != null ? source.Identity.DefinitionId : string.Empty;
            var monster = PakuriDataManager.Instance.ResolveMonster(monsterId);
            var triggers = monster != null ? monster.SkillTriggers : null;
            if (triggers == null || triggers.Length == 0)
            {
                return;
            }

            var sourceMonster = source as MonsterUnitRuntimeModel;
            for (var i = 0; i < triggers.Length; i++)
            {
                var trigger = triggers[i];
                if (!ShouldRun(trigger, sourceMonster, sourceSkillId, triggerEvent))
                {
                    continue;
                }

                ExecuteTrigger(combatManager, roster, source, trigger, triggerContext);
            }
        }

        private static bool ShouldRun(
            SkillTriggerDefinition trigger,
            MonsterUnitRuntimeModel source,
            string sourceSkillId,
            SkillTriggerEvent triggerEvent)
        {
            return trigger != null
                && trigger.RuntimeKind == SkillRuntimeKind.SingleAttack
                && trigger.TriggerEvent == triggerEvent
                && string.Equals(trigger.SourceSkillId, sourceSkillId, StringComparison.OrdinalIgnoreCase)
                && HasAllChoices(source, trigger.RequiresActiveChoiceId)
                && !HasAnyChoice(source, trigger.ExcludesActiveChoiceId);
        }

        private static bool HasAllChoices(MonsterUnitRuntimeModel source, string choiceList)
        {
            if (string.IsNullOrWhiteSpace(choiceList))
            {
                return true;
            }

            if (source == null || source.State == null)
            {
                return false;
            }

            var choices = choiceList.Split(';', ',');
            for (var i = 0; i < choices.Length; i++)
            {
                var choice = choices[i] != null ? choices[i].Trim() : string.Empty;
                if (!string.IsNullOrWhiteSpace(choice) && !source.State.ChosenChoiceIds.Contains(choice))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool HasAnyChoice(MonsterUnitRuntimeModel source, string choiceList)
        {
            if (string.IsNullOrWhiteSpace(choiceList) || source == null || source.State == null)
            {
                return false;
            }

            var choices = choiceList.Split(';', ',');
            for (var i = 0; i < choices.Length; i++)
            {
                var choice = choices[i] != null ? choices[i].Trim() : string.Empty;
                if (!string.IsNullOrWhiteSpace(choice) && source.State.ChosenChoiceIds.Contains(choice))
                {
                    return true;
                }
            }

            return false;
        }

        private static void ExecuteTrigger(
            InGameCombatManager combatManager,
            UnitRosterService roster,
            BaseUnitRuntimeModel source,
            SkillTriggerDefinition trigger,
            TriggerExecutionContext triggerContext)
        {
            var repeatCount = Mathf.Max(1, trigger.RepeatCount);
            for (var i = 0; i < repeatCount; i++)
            {
                if (i == 0 || trigger.RepeatIntervalSeconds <= 0f)
                {
                    ExecuteSingleAttack(combatManager, roster, source, trigger, triggerContext);
                    continue;
                }

                combatManager.StartCoroutine(ExecuteDelayed(
                    combatManager,
                    roster,
                    source,
                    trigger,
                    triggerContext,
                    trigger.RepeatIntervalSeconds * i));
            }
        }

        private static IEnumerator ExecuteDelayed(
            InGameCombatManager combatManager,
            UnitRosterService roster,
            BaseUnitRuntimeModel source,
            SkillTriggerDefinition trigger,
            TriggerExecutionContext triggerContext,
            float delaySeconds)
        {
            yield return new WaitForSeconds(Mathf.Max(0f, delaySeconds));
            ExecuteSingleAttack(combatManager, roster, source, trigger, triggerContext);
        }

        private static bool ExecuteSingleAttack(
            InGameCombatManager combatManager,
            UnitRosterService roster,
            BaseUnitRuntimeModel source,
            SkillTriggerDefinition trigger,
            TriggerExecutionContext triggerContext)
        {
            var sourceEntry = ResolveSourceEntry(roster, source, triggerContext.EventTarget);
            if (sourceEntry == null)
            {
                return false;
            }

            var targeting = BuildTargeting(trigger);
            var center = ResolveCenter(sourceEntry, roster, triggerContext, trigger, targeting);
            var damage = ResolveDamage(source, trigger, triggerContext);
            if (damage <= 0f)
            {
                return false;
            }

            if (IsPrefabHitboxTrigger(trigger) && trigger.SkillEffectPrefab != null && combatManager.Effects != null)
            {
                var instance = combatManager.Effects.InstantiateSkillPrefab(trigger.SkillEffectPrefab, center, Quaternion.identity);
                if (instance == null)
                {
                    return false;
                }

                Physics2D.SyncTransforms();
                var routed = ApplyPrefabHitbox(
                    combatManager,
                    sourceEntry,
                    roster,
                    targeting,
                    instance,
                    IsGlobalHitCount(trigger.HitTargetCount) ? int.MaxValue : ParseHitTargetCount(trigger.HitTargetCount),
                    damage,
                    trigger.Attribute,
                    triggerContext.EventTarget);
                UnityEngine.Object.Destroy(instance, 1f);
                return routed;
            }

            var routedArea = ApplyAreaTrigger(
                combatManager,
                sourceEntry,
                roster,
                targeting,
                center,
                Mathf.Max(0f, trigger.Radius),
                trigger.CoverAll || trigger.TargetShape == SkillMultiEffectTargetShape.Battlefield,
                IsGlobalHitCount(trigger.HitTargetCount) ? int.MaxValue : ParseHitTargetCount(trigger.HitTargetCount),
                damage,
                trigger.Attribute,
                triggerContext.EventTarget,
                trigger.TargetSelection == SkillMultiEffectTargetSelection.EventTarget);
            if (routedArea && trigger.SkillEffectPrefab != null && combatManager.Effects != null)
            {
                SkillVisualSpawnUtility.SpawnTransient(combatManager.Effects, trigger.SkillEffectPrefab, center, Quaternion.identity, 1f);
            }

            return routedArea;
        }

        private static SkillTargetingSpec BuildTargeting(SkillTriggerDefinition trigger)
        {
            return new SkillTargetingSpec
            {
                TargetSide = trigger.TargetSide == SkillMultiEffectTargetSide.Self
                    ? SkillTargetSide.Self
                    : trigger.TargetSide == SkillMultiEffectTargetSide.AllAllies
                        ? SkillTargetSide.AllAllies
                        : SkillTargetSide.Enemy,
                Selection = trigger.TargetSelection == SkillMultiEffectTargetSelection.Owner
                    ? SkillTargetSelection.Owner
                    : SkillTargetSelection.Nearest,
                Shape = trigger.TargetShape == SkillMultiEffectTargetShape.Battlefield
                    ? SkillTargetShape.Battlefield
                    : trigger.TargetShape == SkillMultiEffectTargetShape.Single
                        ? SkillTargetShape.Single
                        : SkillTargetShape.Circle,
                Radius = trigger.Radius,
                CoverAll = trigger.CoverAll || trigger.TargetShape == SkillMultiEffectTargetShape.Battlefield
            };
        }

        private static Vector2 ResolveCenter(
            UnitRosterEntry sourceEntry,
            UnitRosterService roster,
            TriggerExecutionContext triggerContext,
            SkillTriggerDefinition trigger,
            SkillTargetingSpec targeting)
        {
            if (trigger != null)
            {
                switch (trigger.CenterMode)
                {
                    case SkillMultiEffectCenterMode.Caster:
                        return sourceEntry.Transform != null ? (Vector2)sourceEntry.Transform.position : triggerContext.EventCenter;
                    case SkillMultiEffectCenterMode.NearestEnemy:
                        var nearest = SkillExecutionUtility.FindNearestTarget(sourceEntry, roster, targeting);
                        return nearest != null && nearest.Transform != null ? (Vector2)nearest.Transform.position : triggerContext.EventCenter;
                    case SkillMultiEffectCenterMode.EffectTarget:
                        return ResolveUnitPosition(roster, triggerContext.EventTarget);
                }
            }

            return triggerContext.EventCenter;
        }

        private static float ResolveDamage(BaseUnitRuntimeModel source, SkillTriggerDefinition trigger, TriggerExecutionContext triggerContext)
        {
            switch (trigger.DamageSource)
            {
                case SkillTriggerDamageSource.ShieldAppliedAmount:
                    return Mathf.Max(0f, triggerContext.Status != null ? triggerContext.Status.AppliedShieldAmount : 0f)
                        * Mathf.Max(0f, trigger.DamageSourceMultiplier)
                        * Mathf.Max(0f, trigger.DamageMultiplier);
                case SkillTriggerDamageSource.ShieldRemainingAmount:
                    return Mathf.Max(0f, triggerContext.Status != null ? triggerContext.Status.RemainingShieldAmount : 0f)
                        * Mathf.Max(0f, trigger.DamageSourceMultiplier)
                        * Mathf.Max(0f, trigger.DamageMultiplier);
                case SkillTriggerDamageSource.ShieldAbsorbedAmount:
                    return Mathf.Max(0f, triggerContext.ShieldAbsorbedAmount)
                        * Mathf.Max(0f, trigger.DamageSourceMultiplier)
                        * Mathf.Max(0f, trigger.DamageMultiplier);
                case SkillTriggerDamageSource.TrackedIncomingDamage:
                    return Mathf.Max(0f, triggerContext.Status != null
                            ? triggerContext.Status.GetTrackedIncomingDamage(ResolveTrackedAttribute(trigger))
                            : 0f)
                        * Mathf.Max(0f, trigger.DamageSourceMultiplier)
                        * Mathf.Max(0f, trigger.DamageMultiplier);
                default:
                    var useSpellPower = Mathf.Abs(trigger.SpellPowerCoefficient) >= Mathf.Abs(trigger.AttackPowerCoefficient);
                    var damageSpec = new SkillDamageSpec
                    {
                        Element = (ElementType)(int)trigger.Attribute,
                        BaseDamage = trigger.BaseDamage,
                        StatCoefficient = useSpellPower ? trigger.SpellPowerCoefficient : trigger.AttackPowerCoefficient,
                        StatSource = useSpellPower ? StatSource.Intelligence : StatSource.Attack,
                        CriticalAllowed = true
                    };
                    return SkillExecutionUtility.ResolveDamage(source, damageSpec, null) * Mathf.Max(0f, trigger.DamageMultiplier);
            }
        }

        private static bool ApplyPrefabHitbox(
            InGameCombatManager manager,
            UnitRosterEntry sourceEntry,
            UnitRosterService roster,
            SkillTargetingSpec targeting,
            GameObject hitboxObject,
            int maxTargets,
            float damage,
            DamageAttribute attribute,
            BaseUnitRuntimeModel preferredTarget)
        {
            if (manager == null || sourceEntry == null || roster == null || hitboxObject == null || maxTargets <= 0)
            {
                return false;
            }

            var hitboxColliders = hitboxObject.GetComponentsInChildren<Collider2D>();
            if (hitboxColliders == null || hitboxColliders.Length == 0)
            {
                return false;
            }

            var targets = ResolveOrderedTargets(sourceEntry, roster, targeting, preferredTarget, preferredTarget != null);
            var routed = false;
            var hitCount = 0;
            for (var i = 0; i < targets.Count; i++)
            {
                var target = targets[i];
                if (!IsTargetInsideHitbox(hitboxColliders, target))
                {
                    continue;
                }

                manager.ApplyDamage(target.Model, damage, attribute, sourceEntry.Model);
                routed = true;
                hitCount++;
                if (hitCount >= maxTargets)
                {
                    break;
                }
            }

            return routed;
        }

        private static List<UnitRosterEntry> ResolveOrderedTargets(
            UnitRosterEntry sourceEntry,
            UnitRosterService roster,
            SkillTargetingSpec targeting,
            BaseUnitRuntimeModel preferredTarget,
            bool preferEventTarget)
        {
            var candidates = SkillExecutionUtility.ResolveTargetList(sourceEntry, roster, targeting);
            var targets = new List<UnitRosterEntry>();
            for (var i = 0; i < candidates.Count; i++)
            {
                var target = candidates[i];
                if (target != null && target.IsAlive && target.Model != null && target.Transform != null)
                {
                    targets.Add(target);
                }
            }

            targets.Sort((left, right) =>
            {
                if (preferEventTarget)
                {
                    var leftPreferred = MatchesModel(left, preferredTarget);
                    var rightPreferred = MatchesModel(right, preferredTarget);
                    if (leftPreferred != rightPreferred)
                    {
                        return leftPreferred ? -1 : 1;
                    }
                }

                var leftDistance = ResolveDistanceSquared(sourceEntry, left);
                var rightDistance = ResolveDistanceSquared(sourceEntry, right);
                return leftDistance.CompareTo(rightDistance);
            });
            return targets;
        }

        private static bool ApplyAreaTrigger(
            InGameCombatManager manager,
            UnitRosterEntry sourceEntry,
            UnitRosterService roster,
            SkillTargetingSpec targeting,
            Vector2 center,
            float radius,
            bool coverAll,
            int maxTargets,
            float damage,
            DamageAttribute attribute,
            BaseUnitRuntimeModel preferredTarget,
            bool preferEventTarget)
        {
            if (manager == null || sourceEntry == null || roster == null || maxTargets <= 0)
            {
                return false;
            }

            var targets = ResolveOrderedTargets(sourceEntry, roster, targeting, preferredTarget, preferEventTarget);
            if (!coverAll && radius <= 0f)
            {
                var target = preferEventTarget ? FindPreferredEntry(roster, preferredTarget) : (targets.Count > 0 ? targets[0] : null);
                if (target == null || !target.IsAlive || target.Model == null)
                {
                    return false;
                }

                manager.ApplyDamage(target.Model, damage, attribute, sourceEntry.Model);
                return true;
            }

            var routed = false;
            var hitCount = 0;
            var radiusSq = Mathf.Max(0f, radius) * Mathf.Max(0f, radius);
            for (var i = 0; i < targets.Count; i++)
            {
                var target = targets[i];
                if (target == null || !target.IsAlive || target.Model == null || target.Transform == null)
                {
                    continue;
                }

                if (!coverAll)
                {
                    var offset = (Vector2)target.Transform.position - center;
                    if (offset.sqrMagnitude > radiusSq)
                    {
                        continue;
                    }
                }

                manager.ApplyDamage(target.Model, damage, attribute, sourceEntry.Model);
                routed = true;
                hitCount++;
                if (hitCount >= maxTargets)
                {
                    break;
                }
            }

            return routed;
        }

        private static UnitRosterEntry FindPreferredEntry(UnitRosterService roster, BaseUnitRuntimeModel preferredTarget)
        {
            return preferredTarget != null && roster != null ? roster.Find(preferredTarget) : null;
        }

        private static bool MatchesModel(UnitRosterEntry entry, BaseUnitRuntimeModel preferredTarget)
        {
            return entry != null && preferredTarget != null && entry.Model == preferredTarget;
        }

        private static DamageAttribute ResolveTrackedAttribute(SkillTriggerDefinition trigger)
        {
            if (trigger == null)
            {
                return DamageAttribute.Physical;
            }

            return trigger.TrackedAttribute == DamageAttribute.Physical && trigger.Attribute != DamageAttribute.Physical
                ? trigger.Attribute
                : trigger.TrackedAttribute;
        }

        private static bool IsTargetInsideHitbox(Collider2D[] hitboxColliders, UnitRosterEntry target)
        {
            return UnitHitboxUtility.IsTargetInsideHitbox(hitboxColliders, target);
        }

        private static UnitRosterEntry ResolveSourceEntry(UnitRosterService roster, BaseUnitRuntimeModel source, BaseUnitRuntimeModel fallback)
        {
            return roster != null ? roster.Find(source) ?? roster.Find(fallback) : null;
        }

        private static BaseUnitRuntimeModel ResolveSourceModel(UnitRosterService roster, string sourceUnitId, string sourceDefinitionId)
        {
            if (roster == null)
            {
                return null;
            }

            var entries = roster.Entries;
            for (var i = 0; i < entries.Count; i++)
            {
                var model = entries[i] != null ? entries[i].Model : null;
                var identity = model != null ? model.Identity : null;
                if (identity == null)
                {
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(sourceUnitId) && string.Equals(identity.UnitId, sourceUnitId, StringComparison.OrdinalIgnoreCase))
                {
                    return model;
                }
            }

            for (var i = 0; i < entries.Count; i++)
            {
                var model = entries[i] != null ? entries[i].Model : null;
                var identity = model != null ? model.Identity : null;
                if (identity != null
                    && !string.IsNullOrWhiteSpace(sourceDefinitionId)
                    && string.Equals(identity.DefinitionId, sourceDefinitionId, StringComparison.OrdinalIgnoreCase))
                {
                    return model;
                }
            }

            return null;
        }

        private static Vector2 ResolveUnitPosition(UnitRosterService roster, BaseUnitRuntimeModel model)
        {
            var entry = roster != null ? roster.Find(model) : null;
            return entry != null && entry.Transform != null ? (Vector2)entry.Transform.position : Vector2.zero;
        }

        private static float ResolveDistanceSquared(UnitRosterEntry sourceEntry, UnitRosterEntry target)
        {
            if (sourceEntry == null || sourceEntry.Transform == null || target == null || target.Transform == null)
            {
                return float.MaxValue;
            }

            var offset = target.Transform.position - sourceEntry.Transform.position;
            offset.z = 0f;
            return offset.sqrMagnitude;
        }

        private static bool IsPrefabHitboxTrigger(SkillTriggerDefinition trigger)
        {
            return trigger != null && trigger.SkillEffectPrefab != null;
        }

        private static bool IsGlobalHitCount(string rawValue)
        {
            return string.Equals(rawValue, "global", StringComparison.OrdinalIgnoreCase)
                || string.Equals(rawValue, "all", StringComparison.OrdinalIgnoreCase);
        }

        private static int ParseHitTargetCount(string rawValue)
        {
            return int.TryParse(rawValue, out var count) ? Mathf.Max(1, count) : 1;
        }
    }
}
