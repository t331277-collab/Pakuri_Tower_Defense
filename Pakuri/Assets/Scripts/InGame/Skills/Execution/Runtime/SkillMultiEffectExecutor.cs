using System;
using System.Collections;
using System.Collections.Generic;
using Pakuri.Combat;
using Pakuri.Data;
using UnityEngine;

/*
 * 스킬과 패시브에 연결된 추가 효과를 조건과 실행 시점에 맞춰 처리한다.
 * Choice·패시브·상태·체력 조건과 지연 시간을 검사하고
 * 추가 피해, 상태 적용·연장, 지속 범위, 영구 패시브 상태와 시각 효과를 실행한다.
 */
namespace Pakuri.InGame
{

    internal static class SkillMultiEffectExecutor
    {
        /*
         * 요청받은 스킬 다중 효과을 실행한다.
         */
        public static bool Execute(
            SkillExecutionContext context,
            SkillExecutionSnapshot snapshot,
            SkillEffectDefinition[] effects,
            Vector2 fallbackCenter)
        {
            return ExecuteFiltered(context, snapshot, effects, fallbackCenter, null, false);
        }

        /*
         * 효과를 지연 없이 즉시 실행한다.
         */
        internal static bool ExecuteDirect(
            SkillExecutionContext context,
            SkillExecutionSnapshot snapshot,
            SkillEffectDefinition effect,
            Vector2 fallbackCenter,
            bool scaleStatusDurationWithSnapshot = false)
        {
            if (effect == null)
            {
                return false;
            }

            return SkillPlanActionDispatcher.ExecuteEffect(context, snapshot, effect, fallbackCenter, scaleStatusDurationWithSnapshot);
        }

        /*
         * 포함 상태 지속시간 보정을 실행한다.
         */
        internal static bool ExecuteWithStatusDurationScaling(
            SkillExecutionContext context,
            SkillExecutionSnapshot snapshot,
            SkillEffectDefinition[] effects,
            Vector2 fallbackCenter)
        {
            return ExecuteFiltered(context, snapshot, effects, fallbackCenter, null, true);
        }

        /*
         * 스킬 오브젝트가 종료될 때 연결된 효과를 실행한다.
         */
        internal static bool ExecuteOnExpire(
            SkillExecutionContext context,
            SkillExecutionSnapshot snapshot,
            SkillEffectDefinition[] effects,
            Vector2 fallbackCenter)
        {
            return ExecuteFiltered(context, snapshot, effects, fallbackCenter, SkillMultiEffectTiming.OnExpire, false);
        }

        /*
         * 배치 시전을 실행한다.
         */
        internal static bool ExecuteOnDeploymentCast(
            SkillExecutionContext context,
            SkillExecutionSnapshot snapshot,
            SkillEffectDefinition[] effects,
            Vector2 fallbackCenter)
        {
            return ExecuteFiltered(context, snapshot, effects, fallbackCenter, SkillMultiEffectTiming.OnDeploymentCast, false);
        }

        /*
         * 적중을 실행한다.
         */
        internal static bool ExecuteOnHit(
            SkillExecutionContext context,
            SkillExecutionSnapshot snapshot,
            SkillEffectDefinition[] effects,
            Vector2 fallbackCenter,
            BaseUnitRuntimeModel eventTarget)
        {
            if (context == null)
            {
                return false;
            }

            var hitContext = new SkillExecutionContext(
                context.CombatManager,
                context.Roster,
                context.CasterEntry,
                context.Runtime,
                context.DeltaTime,
                eventTarget,
                context.HasManualAimDirection,
                context.ManualAimDirection,
                context.HasManualTargetPoint,
                context.ManualTargetPoint,
                context.RecastGeneration);
            return ExecuteFiltered(hitContext, snapshot, effects, fallbackCenter, SkillMultiEffectTiming.OnHit, false);
        }

        /*
         * 적중 횟수를 실행한다.
         */
        internal static bool ExecuteOnHitCount(
            SkillExecutionContext context,
            SkillExecutionSnapshot snapshot,
            SkillEffectDefinition[] effects,
            Vector2 fallbackCenter,
            int hitCount)
        {
            return ExecuteFiltered(context, snapshot, effects, fallbackCenter, SkillMultiEffectTiming.OnHitCount, false, hitCount);
        }

        /*
         * 조건 선별을 실행한다.
         */
        private static bool ExecuteFiltered(
            SkillExecutionContext context,
            SkillExecutionSnapshot snapshot,
            SkillEffectDefinition[] effects,
            Vector2 fallbackCenter,
            SkillMultiEffectTiming? requiredTiming,
            bool scaleStatusDurationWithSnapshot,
            int eventHitCount = 0)
        {
            if (context == null || context.CombatManager == null || effects == null || effects.Length == 0)
            {
                return false;
            }

            var routed = false;
            for (var i = 0; i < effects.Length; i++)
            {
                var effect = effects[i];
                if (!ShouldRun(context, effect, snapshot))
                {
                    continue;
                }

                if (requiredTiming.HasValue)
                {
                    if (effect.EffectTiming != requiredTiming.Value)
                    {
                        continue;
                    }
                }
                else if (effect.EffectTiming == SkillMultiEffectTiming.OnHit
                    || effect.EffectTiming == SkillMultiEffectTiming.OnDeploymentCast
                    || effect.EffectTiming == SkillMultiEffectTiming.OnExpire
                    || effect.EffectTiming == SkillMultiEffectTiming.OnHitCount)
                {
                    continue;
                }

                if (!MatchesHitCountCondition(effect, eventHitCount))
                {
                    continue;
                }

                if (effect.EffectTiming == SkillMultiEffectTiming.Delayed || effect.DelaySeconds > 0f)
                {
                    context.CombatManager.StartCoroutine(ExecuteDelayed(context, snapshot, effect, fallbackCenter, scaleStatusDurationWithSnapshot));
                    routed = true;
                    continue;
                }

                routed = SkillPlanActionDispatcher.ExecuteEffect(context, snapshot, effect, fallbackCenter, scaleStatusDurationWithSnapshot) || routed;
            }

            return routed;
        }

        /*
         * 지정한 지연시간이 지난 뒤 효과를 실행한다.
         */
        private static IEnumerator ExecuteDelayed(
            SkillExecutionContext context,
            SkillExecutionSnapshot snapshot,
            SkillEffectDefinition effect,
            Vector2 fallbackCenter,
            bool scaleStatusDurationWithSnapshot)
        {
            var delay = effect != null ? Mathf.Max(0f, effect.DelaySeconds) : 0f;
            if (delay > 0f)
            {
                yield return new WaitForSeconds(delay);
            }
            else
            {
                yield return null;
            }

            SkillPlanActionDispatcher.ExecuteEffect(context, snapshot, effect, fallbackCenter, scaleStatusDurationWithSnapshot);
        }

        /*
         * 선택지, 패시브, 출처 상태 조건을 만족해 효과를 실행할 수 있는지 확인한다.
         */
        internal static bool ShouldRun(SkillExecutionContext context, SkillEffectDefinition effect, SkillExecutionSnapshot snapshot)
        {
            if (effect == null)
            {
                return false;
            }

            if (!effect.EnabledByDefault && string.IsNullOrWhiteSpace(effect.RequiresActiveChoiceId))
            {
                return false;
            }

            if (!HasAllChoices(snapshot, effect.RequiresActiveChoiceId))
            {
                return false;
            }

            if (HasAnyChoice(snapshot, effect.ExcludesActiveChoiceId))
            {
                return false;
            }

            if (!HasAllLearnedPassives(context, effect.RequiresPassiveSkillId))
            {
                return false;
            }

            // 필수 조건을 통과한 뒤 제외 패시브와 출처 상태 조건을 마지막으로 확인한다.
            return !HasAnyLearnedPassive(context, effect.ExcludesPassiveSkillId)
                && HasRequiredSourceStatus(context, effect.RequiredSourceStatusId, effect.RequiredSourceStatusMinStacks);
        }

        /*
         * 모든 선택지를 보유하고 있는지 확인한다.
         */
        private static bool HasAllChoices(SkillExecutionSnapshot snapshot, string choiceList)
        {
            if (string.IsNullOrWhiteSpace(choiceList))
            {
                return true;
            }

            if (snapshot == null)
            {
                return false;
            }

            var choices = choiceList.Split(';', ',');
            for (var i = 0; i < choices.Length; i++)
            {
                var choice = choices[i];
                if (!string.IsNullOrWhiteSpace(choice) && !snapshot.HasActiveChoice(choice.Trim()))
                {
                    return false;
                }
            }

            return true;
        }

        /*
         * 하나 이상의 선택지를 보유하고 있는지 확인한다.
         */
        private static bool HasAnyChoice(SkillExecutionSnapshot snapshot, string choiceList)
        {
            if (string.IsNullOrWhiteSpace(choiceList) || snapshot == null)
            {
                return false;
            }

            var choices = choiceList.Split(';', ',');
            for (var i = 0; i < choices.Length; i++)
            {
                var choice = choices[i];
                if (!string.IsNullOrWhiteSpace(choice) && snapshot.HasActiveChoice(choice.Trim()))
                {
                    return true;
                }
            }

            return false;
        }

        /*
         * 모든 학습한 패시브를 보유하고 있는지 확인한다.
         */
        private static bool HasAllLearnedPassives(SkillExecutionContext context, string passiveList)
        {
            if (string.IsNullOrWhiteSpace(passiveList))
            {
                return true;
            }

            var passives = passiveList.Split(';', ',');
            for (var i = 0; i < passives.Length; i++)
            {
                var passiveId = passives[i];
                if (!string.IsNullOrWhiteSpace(passiveId) && !HasLearnedPassive(context, passiveId.Trim()))
                {
                    return false;
                }
            }

            return true;
        }

        /*
         * 하나 이상의 학습한 패시브를 보유하고 있는지 확인한다.
         */
        private static bool HasAnyLearnedPassive(SkillExecutionContext context, string passiveList)
        {
            if (string.IsNullOrWhiteSpace(passiveList))
            {
                return false;
            }

            var passives = passiveList.Split(';', ',');
            for (var i = 0; i < passives.Length; i++)
            {
                var passiveId = passives[i];
                if (!string.IsNullOrWhiteSpace(passiveId) && HasLearnedPassive(context, passiveId.Trim()))
                {
                    return true;
                }
            }

            return false;
        }

        /*
         * 학습한 패시브를 보유하고 있는지 확인한다.
         */
        private static bool HasLearnedPassive(SkillExecutionContext context, string passiveId)
        {
            var monster = context != null ? context.Caster as MonsterUnitRuntimeModel : null;
            return monster != null
                && monster.State != null
                && !string.IsNullOrWhiteSpace(passiveId)
                && monster.State.LearnedPassiveSkillIds.Contains(passiveId);
        }

        /*
         * 필수 출처 상태를 보유하고 있는지 확인한다.
         */
        private static bool HasRequiredSourceStatus(SkillExecutionContext context, string statusId, int minStacks)
        {
            if (string.IsNullOrWhiteSpace(statusId))
            {
                return true;
            }

            if (!StatusEffectUtility.TryParse(statusId, out var kind))
            {
                return false;
            }

            var caster = context != null ? context.Caster : null;
            if (kind == StatusEffectKind.Shield)
            {
                return caster != null
                    && caster.Resources != null
                    && caster.Resources.CurrentShield > 0f;
            }

            return caster != null
                && caster.Statuses != null
                && caster.Statuses.GetStacks(kind) >= Mathf.Max(1, minStacks);
        }

        /*
         * 피해 효과 행동을 실행한다.
         */
        internal static bool ExecuteDamageEffectAction(
            SkillExecutionContext context,
            SkillExecutionSnapshot snapshot,
            SkillEffectDefinition effect,
            Vector2 fallbackCenter)
        {
            var targeting = BuildTargeting(effect);
            var center = ResolveEffectCenter(context, effect, targeting, fallbackCenter);
            var damageSpec = new SkillDamageSpec
            {
                SkillId = effect.SkillId,
                Element = (DamageAttribute)(int)effect.Attribute,
                BaseDamage = effect.BaseDamage,
                StatCoefficient = Mathf.Abs(effect.SpellPowerCoefficient) >= Mathf.Abs(effect.AttackPowerCoefficient)
                    ? effect.SpellPowerCoefficient
                    : effect.AttackPowerCoefficient,
                StatSource = Mathf.Abs(effect.SpellPowerCoefficient) >= Mathf.Abs(effect.AttackPowerCoefficient)
                    ? StatSource.Intelligence
                    : StatSource.Attack,
                CriticalAllowed = true
            };

            var damage = SkillExecutionUtility.ResolveDamage(context.Caster, damageSpec, snapshot) * Mathf.Max(0f, effect.DamageMultiplier);
            var statusSpec = ResolveStatusSpec(effect, snapshot);
            if (HasPersistentZone(effect))
            {
                return SpawnPersistentDamageZone(context, snapshot, effect, targeting, center, damage, statusSpec);
            }

            if (TryExecuteRuntimeHitboxDamageEffect(
                    context,
                    snapshot,
                    effect,
                    targeting,
                    center,
                    damage,
                    statusSpec,
                    damageSpec.CriticalAllowed,
                    out var runtimeHitboxRouted))
            {
                return runtimeHitboxRouted;
            }

            var explicitTarget = ResolveExplicitEventTarget(context, effect);
            if (explicitTarget != null)
            {
                var resolvedDamage = SkillExecutionUtility.ResolveDamageAgainstTarget(damage, snapshot, explicitTarget);
                context.CombatManager.ApplyDamage(
                    explicitTarget,
                    resolvedDamage,
                    effect.Attribute,
                    context.Caster,
                    damageSpec.CriticalAllowed,
                    snapshot != null ? snapshot.CritChanceBonus : 0f,
                    snapshot != null ? snapshot.CritDamageBonus : 0f,
                    !string.IsNullOrWhiteSpace(effect.EffectId) ? effect.EffectId : effect.SkillId);
                SkillStatusApplyUtility.TryApplyStatus(context.CombatManager, explicitTarget, statusSpec, context.Caster);
                SpawnVisual(context, effect, center);
                return true;
            }

            var routed = InGameZoneSkillActor.ApplyAreaTick(
                context.CombatManager,
                context.CasterEntry,
                context.Roster,
                targeting,
                center,
                ResolveRadius(effect, snapshot),
                effect.CoverAll || effect.TargetShape == SkillMultiEffectTargetShape.Battlefield,
                damage,
                effect.Attribute,
                statusSpec,
                context.Caster,
                !string.IsNullOrWhiteSpace(effect.EffectId) ? effect.EffectId : effect.SkillId,
                null,
                damageSpec.CriticalAllowed,
                snapshot != null ? snapshot.CritChanceBonus : 0f,
                snapshot != null ? snapshot.CritDamageBonus : 0f);
            if (routed)
            {
                SpawnVisual(context, effect, center);
            }

            return routed;
        }

        /*
         * 런타임 히트박스 피해 효과를 실행하고 성공 여부를 반환한다.
         */
        private static bool TryExecuteRuntimeHitboxDamageEffect(
            SkillExecutionContext context,
            SkillExecutionSnapshot snapshot,
            SkillEffectDefinition effect,
            SkillTargetingSpec targeting,
            Vector2 center,
            float damage,
            ProjectileStatusHitSpec statusSpec,
            bool criticalAllowed,
            out bool routed)
        {
            routed = false;
            var runtimeVisual = effect != null ? effect.RuntimeVisual : null;
            var runtimeHitbox = runtimeVisual != null ? runtimeVisual.Hitbox : null;
            if (runtimeHitbox == null || !runtimeHitbox.HasHitbox())
            {
                return false;
            }

            var effectId = !string.IsNullOrWhiteSpace(effect.EffectId)
                ? effect.EffectId
                : effect.SkillId;
            if (context == null
                || context.CombatManager == null
                || context.CombatManager.Effects == null)
            {
                Debug.LogError($"Runtime hitbox effect '{effectId}' could not create its RuntimeEffectVisual.");
                return true;
            }

            var instance = context.CombatManager.Effects.SpawnTransient(
                runtimeVisual,
                string.IsNullOrWhiteSpace(effect.EffectId) ? "SkillEffectVisual" : $"SkillEffectVisual_{effect.EffectId}",
                center,
                Quaternion.identity,
                1f);
            if (instance == null)
            {
                Debug.LogError($"Runtime hitbox effect '{effectId}' failed to create its RuntimeEffectVisual.");
                return true;
            }

            SkillExecutionUtility.ApplyPrefabScale(instance.transform, effect.Radius, snapshot);
            Physics2D.SyncTransforms();
            var hitboxColliders = instance.GetComponentsInChildren<Collider2D>();
            if (hitboxColliders == null || hitboxColliders.Length == 0)
            {
                Debug.LogError($"Runtime hitbox effect '{effectId}' created no Collider2D components.");
                return true;
            }

            routed = InGameZoneSkillActor.ApplyColliderAreaTick(
                context.CombatManager,
                context.CasterEntry,
                context.Roster,
                targeting,
                hitboxColliders,
                int.MaxValue,
                damage,
                effect.Attribute,
                statusSpec,
                context.Caster,
                effectId,
                null,
                criticalAllowed,
                snapshot != null ? snapshot.CritChanceBonus : 0f,
                snapshot != null ? snapshot.CritDamageBonus : 0f,
                null,
                effectId);
            return true;
        }

        /*
         * 연장 상태 지속시간 효과 행동을 실행한다.
         */
        internal static bool ExecuteExtendStatusDurationEffectAction(
            SkillExecutionContext context,
            SkillEffectDefinition effect)
        {
            if (context == null || context.CombatManager == null || context.CasterEntry == null || context.Roster == null || effect == null)
            {
                return false;
            }

            var statusKey = !string.IsNullOrWhiteSpace(effect.StatusEffectId)
                ? effect.StatusEffectId
                : effect.StatusEffectLabel;
            if (!StatusEffectUtility.TryParse(statusKey, out var kind))
            {
                return false;
            }

            var durationDelta = Mathf.Max(0f, effect.StatusDurationSeconds);
            if (durationDelta <= 0f)
            {
                return false;
            }

            var targeting = BuildTargeting(effect);
            var targets = SkillExecutionUtility.ResolveTargetList(context.CasterEntry, context.Roster, targeting);
            var routed = false;
            for (var i = 0; i < targets.Count; i++)
            {
                var target = targets[i];
                if (target == null || !target.IsAlive || target.Model == null)
                {
                    continue;
                }

                routed = context.CombatManager.ExtendStatusDuration(target.Model, kind, durationDelta) || routed;
            }

            return routed;
        }

        /*
         * 상태 효과 행동을 실행한다.
         */
        internal static bool ExecuteStatusEffectAction(
            SkillExecutionContext context,
            SkillExecutionSnapshot snapshot,
            SkillEffectDefinition effect,
            Vector2 fallbackCenter,
            bool scaleStatusDurationWithSnapshot)
        {
            var statusSpec = ResolveStatusSpec(effect, snapshot, scaleStatusDurationWithSnapshot);
            if (statusSpec == null || !statusSpec.Enabled)
            {
                return false;
            }

            var targeting = BuildTargeting(effect);
            var targets = ResolveStatusTargets(context, effect, targeting);
            var visualTargets = effect.VisualAnchorMode == SkillMultiEffectVisualAnchorMode.AppliedTargets
                ? new List<UnitRosterEntry>()
                : null;
            var routed = false;
            for (var i = 0; i < targets.Count; i++)
            {
                var target = targets[i];
                if (target == null || !target.IsAlive || target.Model == null)
                {
                    continue;
                }

                if (!TargetMatchesCondition(target.Model, effect))
                {
                    continue;
                }

                if (statusSpec.StatusData != null && statusSpec.StatusData.Kind == StatusEffectKind.Shield)
                {
                    context.CombatManager.ApplyShieldStatus(
                        target.Model,
                        statusSpec.StatusData,
                        ResolveStatusEffectShieldAmount(context.Caster, effect, snapshot),
                        statusSpec.DurationSeconds,
                        statusSpec.Stacks,
                        statusSpec.MaxStacks,
                        statusSpec.Permanent,
                        statusSpec.RefreshDuration,
                        context.Caster);
                }
                else
                {
                    if (!SkillStatusApplyUtility.TryApplyStatus(context.CombatManager, target.Model, statusSpec, context.Caster))
                    {
                        continue;
                    }
                }
                if (visualTargets != null)
                {
                    visualTargets.Add(target);
                }

                routed = true;
            }

            if (routed)
            {
                if (visualTargets != null)
                {
                    SpawnVisualOnTargets(context, effect, visualTargets, statusSpec.DurationSeconds);
                }
                else
                {
                    SpawnVisual(context, effect, ResolveEffectCenter(context, effect, targeting, fallbackCenter));
                }
            }

            return routed;
        }

        // Code Builder: 패시브 런타임이 상태 변화 통지를 받았을 때 조건을 만족하는 대상만 계산한다.
        internal static List<UnitRosterEntry> ResolvePassiveStatusTargets(
            SkillExecutionContext context,
            SkillExecutionSnapshot snapshot,
            SkillEffectDefinition effect)
        {
            var matches = new List<UnitRosterEntry>();
            if (context == null
                || effect == null
                || effect.EffectKind != SkillMultiEffectKind.Status
                || !ShouldRun(context, effect, snapshot))
            {
                return matches;
            }

            var targeting = BuildTargeting(effect);
            var targets = ResolveStatusTargets(context, effect, targeting);
            for (var i = 0; i < targets.Count; i++)
            {
                var target = targets[i];
                if (target != null
                    && target.IsAlive
                    && target.Model != null
                    && TargetMatchesCondition(target.Model, effect))
                {
                    matches.Add(target);
                }
            }

            return matches;
        }

        // Code Builder: 조건형 패시브 상태는 짧은 임대 갱신 대신 조건이 끝날 때까지 한 번만 유지한다.
        internal static bool ApplyPersistentPassiveStatus(
            SkillExecutionContext context,
            SkillExecutionSnapshot snapshot,
            SkillEffectDefinition effect,
            UnitRosterEntry target,
            Vector2 fallbackCenter)
        {
            if (context == null
                || context.CombatManager == null
                || effect == null
                || target == null
                || !target.IsAlive
                || target.Model == null
                || !TargetMatchesCondition(target.Model, effect))
            {
                return false;
            }

            var statusSpec = ResolveStatusSpec(effect, snapshot);
            if (statusSpec == null || !statusSpec.Enabled || statusSpec.StatusData == null)
            {
                return false;
            }

            var visualDuration = statusSpec.DurationSeconds;
            statusSpec.Permanent = true;
            statusSpec.DurationSeconds = 0f;
            statusSpec.RefreshDuration = false;

            var applied = statusSpec.StatusData.Kind == StatusEffectKind.Shield
                ? context.CombatManager.ApplyShieldStatus(
                    target.Model,
                    statusSpec.StatusData,
                    ResolveStatusEffectShieldAmount(context.Caster, effect, snapshot),
                    statusSpec.DurationSeconds,
                    statusSpec.Stacks,
                    statusSpec.MaxStacks,
                    statusSpec.Permanent,
                    statusSpec.RefreshDuration,
                    context.Caster) != null
                : SkillStatusApplyUtility.TryApplyStatus(context.CombatManager, target.Model, statusSpec, context.Caster);
            if (!applied)
            {
                return false;
            }

            if (effect.VisualAnchorMode == SkillMultiEffectVisualAnchorMode.AppliedTargets)
            {
                SpawnVisualOnTargets(context, effect, new[] { target }, visualDuration);
            }
            else
            {
                var targeting = BuildTargeting(effect);
                SpawnVisual(context, effect, ResolveEffectCenter(context, effect, targeting, fallbackCenter));
            }

            return true;
        }

        /*
         * 상태 대상을 결정한다.
         */
        private static IReadOnlyList<UnitRosterEntry> ResolveStatusTargets(
            SkillExecutionContext context,
            SkillEffectDefinition effect,
            SkillTargetingSpec targeting)
        {
            var explicitTarget = ResolveExplicitEventTarget(context, effect);
            var explicitEntry = explicitTarget != null && context != null && context.Roster != null
                ? context.Roster.Find(explicitTarget)
                : null;
            return explicitEntry != null
                ? new List<UnitRosterEntry> { explicitEntry }
                : SkillExecutionUtility.ResolveTargetList(context.CasterEntry, context.Roster, targeting);
        }

        /*
         * 대상이 효과의 체력과 상태 조건을 만족하는지 확인한다.
         */
        internal static bool TargetMatchesCondition(BaseUnitRuntimeModel target, SkillEffectDefinition effect)
        {
            if (effect == null)
            {
                return true;
            }

            var statusMatches = true;
            if (!string.IsNullOrWhiteSpace(effect.ConditionStatusId))
            {
                statusMatches = StatusEffectRules.MatchesConditionStatus(
                    target,
                    effect.ConditionStatusId,
                    effect.ConditionStatusSourceSkillId);
            }

            var skillMatches = true;
            if (!string.IsNullOrWhiteSpace(effect.ConditionSkillAttribute))
            {
                skillMatches = HasActiveSkillAttribute(target, effect.ConditionSkillAttribute);
            }

            var healthRatioMatches = true;
            if (effect.ConditionHealthRatioMax > 0f)
            {
                healthRatioMatches = IsWithinHealthRatio(target, effect.ConditionHealthRatioMax);
            }

            return statusMatches && skillMatches && healthRatioMatches;
        }

        /*
         * 현재 적중 횟수가 효과 실행 조건을 만족하는지 확인한다.
         */
        private static bool MatchesHitCountCondition(SkillEffectDefinition effect, int hitCount)
        {
            return effect == null || effect.ConditionHitCountMin <= 0 || hitCount >= effect.ConditionHitCountMin;
        }

        /*
         * 대상 체력 비율이 지정 범위 안인지 확인한다.
         */
        private static bool IsWithinHealthRatio(BaseUnitRuntimeModel target, float maxRatio)
        {
            var resources = target != null ? target.Resources : null;
            var stats = target != null ? target.Stats : null;
            return resources != null
                && stats != null
                && stats.MaxHealth > 0f
                && resources.CurrentHealth / stats.MaxHealth <= Mathf.Clamp01(maxRatio);
        }

        /*
         * 상태 지속시간 보너스를 결정한다.
         */
        private static float ResolveStatusDurationBonus(
            SkillExecutionSnapshot snapshot,
            RuntimeStatusData statusData,
            StatusEffectKind kind)
        {
            if (snapshot == null)
            {
                return 0f;
            }

            var statusId = statusData != null && !string.IsNullOrWhiteSpace(statusData.StatusTag)
                ? statusData.StatusTag
                : StatusEffectUtility.GetDefinition(kind).Id;
            return snapshot.ResolveStatusDurationBonus(statusId);
        }

        /*
         * 상태 설정을 결정한다.
         */
        internal static ProjectileStatusHitSpec ResolveStatusSpec(
            SkillEffectDefinition effect,
            SkillExecutionSnapshot snapshot = null,
            bool scaleDurationWithSnapshot = false)
        {
            var statusData = CreateStatusData(effect);
            if (statusData == null)
            {
                return null;
            }

            statusData = SkillStatusSpecUtility.ResolveStatusData(statusData, statusData.Kind, snapshot);
            var definition = StatusEffectUtility.GetDefinition(statusData.Kind);
            var duration = statusData.Duration > 0f ? statusData.Duration : definition.DefaultDurationSeconds;
            var targetedDurationBonus = ResolveStatusDurationBonus(snapshot, statusData, statusData.Kind);
            if (!Mathf.Approximately(targetedDurationBonus, 0f))
            {
                duration = Mathf.Max(0f, duration + targetedDurationBonus);
            }

            if (scaleDurationWithSnapshot && snapshot != null)
            {
                duration = duration * Mathf.Max(0f, snapshot.DurationMultiplier) + snapshot.DurationBonus;
            }

            return new ProjectileStatusHitSpec
            {
                Enabled = true,
                Kind = statusData.Kind,
                StatusData = statusData,
                Chance = Mathf.Clamp01(effect.StatusChance > 0f ? effect.StatusChance : 1f),
                Stacks = Mathf.Max(1, effect.StatusStackAmount > 0 ? effect.StatusStackAmount : statusData.BaseStackAmount),
                DurationSeconds = duration,
                MaxStacks = statusData.MaxStacks,
                Permanent = statusData.Permanent,
                RefreshDuration = true
            };
        }

        /*
         * 상태 데이터를 생성한다.
         */
        private static RuntimeStatusData CreateStatusData(SkillEffectDefinition effect)
        {
            if (effect == null)
            {
                return null;
            }

            var statusKey = !string.IsNullOrWhiteSpace(effect.StatusEffectId)
                ? effect.StatusEffectId
                : effect.StatusEffectLabel;
            if (!StatusEffectUtility.TryParse(statusKey, out var kind))
            {
                return null;
            }

            var status = StatusEffectFactory.Create(kind, effect.StatusEffectLabel);
            if (status == null)
            {
                return null;
            }

            status.SourceSkillId = !string.IsNullOrWhiteSpace(effect.EffectId) ? effect.EffectId : effect.SkillId;
            if (effect.StatusEffectPrefab != null)
            {
                status.StatusEffectPrefab = effect.StatusEffectPrefab;
            }

            if (StatusEffectFactory.TryParseTargetScope(effect.StatusTargetScope, out var scope))
            {
                status.TargetScope = scope;
            }

            status.MergePolicy = StatusEffectFactory.TryParseMergePolicy(effect.StatusMergePolicy, out var mergePolicy)
                ? mergePolicy
                : StatusMergePolicy.SameSourceRefresh;
            status.ShieldAmountRefreshPolicy = StatusEffectFactory.TryParseShieldRefreshRule(effect.ShieldAmountRefreshPolicy, out var shieldPolicy)
                ? shieldPolicy
                : ShieldRefreshRule.TakeHighest;
            if (effect.StatusDurationSeconds > 0f)
            {
                status.Duration = effect.StatusDurationSeconds;
                status.Permanent = false;
            }

            if (effect.StatusMaxStacks > 0)
            {
                status.MaxStacks = effect.StatusMaxStacks;
                status.IsStackable = status.MaxStacks != 1;
            }

            if (effect.StatusStackAmount > 0)
            {
                status.BaseStackAmount = effect.StatusStackAmount;
            }

            status.Modifiers.ActionSpeedBonus = effect.StatusActionSpeedBonus;
            status.Modifiers.AttackPowerBonus = effect.StatusAttackPowerBonus;
            status.Modifiers.SpellPowerBonus = effect.StatusSpellPowerBonus;
            status.Modifiers.DamageBonusRate = effect.StatusDamageBonusRate;
            status.Modifiers.ShieldReceivedBonus = effect.StatusShieldReceivedBonus;
            status.Modifiers.CritChanceBonusRate = effect.StatusCriticalChanceBonus;
            status.Modifiers.CritDamageBonusRate = effect.StatusCriticalDamageBonus;
            status.MoveSpeedBonus = effect.StatusMoveSpeedBonus;
            status.MovementSlowRate = effect.StatusMoveSpeedBonus < 0f ? -effect.StatusMoveSpeedBonus : 0f;
            status.DamageTakenBonus = effect.StatusDamageTakenBonus;
            status.CriticalDamageTakenBonus = effect.StatusCriticalDamageTakenBonus;
            status.AilmentResistanceBonus = effect.StatusAilmentResistanceBonus;
            status.CriticalResistanceBonus = effect.StatusCriticalResistanceBonus;
            status.ElementResistReduction = effect.StatusElementResistReduction;
            status.FlatElementResistReduction = effect.StatusFlatElementResistReduction;
            status.ElementDamageTakenBonus = effect.StatusElementDamageTakenBonus;
            status.ConditionalTargetStatusTag = effect.StatusConditionalTargetStatusId;
            status.ConditionalStatusChanceBonus = effect.StatusConditionalStatusChanceBonus;
            status.ConditionalIncomingSkillRuntimeKinds = effect.StatusConditionalIncomingSkillRuntimeKinds;
            status.ConditionalOutgoingSkillRuntimeKinds = effect.StatusConditionalOutgoingSkillRuntimeKinds;
            status.AppliedStatusDurationBonusStatusId = effect.StatusAppliedStatusDurationBonusStatusId;
            status.AppliedStatusDurationBonus = effect.StatusAppliedStatusDurationBonus;
            status.OutgoingAdditionalDamageMultiplier = effect.StatusOutgoingAdditionalDamageMultiplier;
            status.OutgoingAdditionalDamageTriggerAttribute = effect.StatusOutgoingAdditionalDamageTriggerAttribute;
            status.OutgoingAdditionalDamageAttribute = effect.StatusOutgoingAdditionalDamageAttribute;
            status.HasElementModifierTarget = !Mathf.Approximately(effect.StatusDamageBonusRate, 0f)
                || !Mathf.Approximately(effect.StatusElementResistReduction, 0f)
                || !Mathf.Approximately(effect.StatusFlatElementResistReduction, 0f)
                || !Mathf.Approximately(effect.StatusElementDamageTakenBonus, 0f);
            status.ElementModifierTarget = (DamageAttribute)(int)effect.Attribute;
            status.Modifiers.ResistReduction = status.ElementResistReduction;
            status.Modifiers.ResistReductionElement = status.ElementModifierTarget;
            return status;
        }

        /*
         * 활성 스킬 속성을 보유하고 있는지 확인한다.
         */
        private static bool HasActiveSkillAttribute(BaseUnitRuntimeModel target, string rawAttribute)
        {
            if (target == null
                || target.SkillRuntime == null
                || string.IsNullOrWhiteSpace(rawAttribute)
                || !Enum.TryParse(rawAttribute.Trim(), true, out DamageAttribute attribute))
            {
                return false;
            }

            var activeSkills = target.SkillRuntime.ActiveSkills;
            for (var i = 0; i < activeSkills.Count; i++)
            {
                var runtime = activeSkills[i];
                if (runtime != null && runtime.Data != null && runtime.Data.Element == attribute)
                {
                    return true;
                }
            }

            return false;
        }

        /*
         * 상태 효과 보호막 수치를 결정한다.
         */
        private static float ResolveStatusEffectShieldAmount(BaseUnitRuntimeModel caster, SkillEffectDefinition effect, SkillExecutionSnapshot snapshot)
        {
            if (effect == null)
            {
                return 0f;
            }

            var useSpellPower = Mathf.Abs(effect.SpellPowerCoefficient) >= Mathf.Abs(effect.AttackPowerCoefficient);
            var stats = caster != null ? caster.Stats : null;
            var stat = 0f;
            if (stats != null)
            {
                stat = useSpellPower
                    ? stats.SpellPower * StatusEffectRules.ResolveSpellPowerMultiplier(caster)
                    : stats.AttackPower * StatusEffectRules.ResolveAttackPowerMultiplier(caster);
            }

            var coefficient = useSpellPower ? effect.SpellPowerCoefficient : effect.AttackPowerCoefficient;
            var shield = (effect.BaseDamage + stat * coefficient) * Mathf.Max(0f, effect.DamageMultiplier);
            if (snapshot != null)
            {
                shield = (shield + snapshot.BaseDamageBonus) * Mathf.Max(0f, snapshot.ShieldAmountMultiplier);
            }

            return Mathf.Max(0f, shield);
        }

        /*
         * 대상 지정을 구성한다.
         */
        private static SkillTargetingSpec BuildTargeting(SkillEffectDefinition effect)
        {
            return new SkillTargetingSpec
            {
                TargetSide = MapTargetSide(effect.TargetSide),
                Selection = MapTargetSelection(effect.TargetSelection),
                Shape = MapTargetShape(effect.TargetShape),
                Radius = effect.Radius,
                CoverAll = effect.CoverAll || effect.TargetShape == SkillMultiEffectTargetShape.Battlefield
            };
        }

        /*
         * 대상 진영을 런타임 값으로 변환한다.
         */
        private static SkillTargetSide MapTargetSide(SkillMultiEffectTargetSide side)
        {
            switch (side)
            {
                case SkillMultiEffectTargetSide.Self:
                    return SkillTargetSide.Self;
                case SkillMultiEffectTargetSide.AllAllies:
                    return SkillTargetSide.AllAllies;
                default:
                    return SkillTargetSide.Enemy;
            }
        }

        /*
         * 대상 선택 방식을 런타임 값으로 변환한다.
         */
        private static SkillTargetSelection MapTargetSelection(SkillMultiEffectTargetSelection selection)
        {
            switch (selection)
            {
                case SkillMultiEffectTargetSelection.Owner:
                    return SkillTargetSelection.Owner;
                case SkillMultiEffectTargetSelection.EventTarget:
                    return SkillTargetSelection.Nearest;
                default:
                    return SkillTargetSelection.Nearest;
            }
        }

        /*
         * 대상 형태를 런타임 값으로 변환한다.
         */
        private static SkillTargetShape MapTargetShape(SkillMultiEffectTargetShape shape)
        {
            switch (shape)
            {
                case SkillMultiEffectTargetShape.Battlefield:
                    return SkillTargetShape.Battlefield;
                case SkillMultiEffectTargetShape.Single:
                    return SkillTargetShape.Single;
                default:
                    return SkillTargetShape.Circle;
            }
        }

        /*
         * 효과 중심점을 결정한다.
         */
        private static Vector2 ResolveEffectCenter(
            SkillExecutionContext context,
            SkillEffectDefinition effect,
            SkillTargetingSpec targeting,
            Vector2 fallbackCenter)
        {
            if (effect != null)
            {
                switch (effect.CenterMode)
                {
                    case SkillMultiEffectCenterMode.EffectTarget:
                        if (context != null && context.EventTarget != null)
                        {
                            var eventEntry = context.Roster != null ? context.Roster.Find(context.EventTarget) : null;
                            if (eventEntry != null && eventEntry.Transform != null)
                            {
                                return eventEntry.Transform.position;
                            }
                        }

                        return fallbackCenter;
                    case SkillMultiEffectCenterMode.PrimarySkillCenter:
                        return fallbackCenter;
                    case SkillMultiEffectCenterMode.Caster:
                        return context != null && context.CasterEntry != null && context.CasterEntry.Transform != null
                            ? (Vector2)context.CasterEntry.Transform.position
                            : fallbackCenter;
                    case SkillMultiEffectCenterMode.NearestEnemy:
                        var enemyTargeting = new SkillTargetingSpec
                        {
                            TargetSide = SkillTargetSide.Enemy,
                            Selection = SkillTargetSelection.Nearest,
                            Shape = SkillTargetShape.Circle,
                            Radius = effect.Radius,
                            CoverAll = false
                        };
                        var enemyTarget = SkillExecutionUtility.FindNearestTarget(context.CasterEntry, context.Roster, enemyTargeting);
                        return enemyTarget != null && enemyTarget.Transform != null
                            ? (Vector2)enemyTarget.Transform.position
                            : fallbackCenter;
                }
            }

            var target = SkillExecutionUtility.FindNearestTarget(context.CasterEntry, context.Roster, targeting);
            if (target != null && target.Transform != null)
            {
                return target.Transform.position;
            }

            return fallbackCenter;
        }

        /*
         * 지속형 지속 범위를 보유하고 있는지 확인한다.
         */
        private static bool HasPersistentZone(SkillEffectDefinition effect)
        {
            return effect != null && effect.ActiveDurationSeconds > 0f && effect.TickIntervalSeconds > 0f;
        }

        /*
         * 명시된 이벤트 대상을 결정한다.
         */
        private static BaseUnitRuntimeModel ResolveExplicitEventTarget(SkillExecutionContext context, SkillEffectDefinition effect)
        {
            return effect != null
                && effect.TargetSelection == SkillMultiEffectTargetSelection.EventTarget
                ? context != null ? context.EventTarget : null
                : null;
        }

        /*
         * 지속형 피해 지속 범위를 생성한다.
         */
        private static bool SpawnPersistentDamageZone(
            SkillExecutionContext context,
            SkillExecutionSnapshot snapshot,
            SkillEffectDefinition effect,
            SkillTargetingSpec targeting,
            Vector2 center,
            float damage,
            ProjectileStatusHitSpec statusSpec)
        {
            if (context == null
                || context.CombatManager == null
                || context.CombatManager.Effects == null
                || context.CasterEntry == null
                || context.Roster == null)
            {
                return false;
            }

            var duration = effect.ActiveDurationSeconds;
            if (snapshot != null)
            {
                duration = duration * Mathf.Max(0f, snapshot.DurationMultiplier) + snapshot.DurationBonus;
            }

            var tickInterval = effect.TickIntervalSeconds;
            if (snapshot != null)
            {
                tickInterval *= Mathf.Max(0.05f, snapshot.ShotIntervalMultiplier);
            }

            duration = Mathf.Max(0.05f, duration);
            tickInterval = Mathf.Max(0.05f, tickInterval);
            var coverAll = effect.CoverAll || effect.TargetShape == SkillMultiEffectTargetShape.Battlefield;
            var radius = ResolveRadius(effect, snapshot);

            GameObject instance = null;
            var hasRuntimeVisual = EffectVisualUtility.HasVisual(effect.RuntimeVisual);
            if (hasRuntimeVisual && context.CombatManager.Effects != null)
            {
                instance = context.CombatManager.Effects.CreateRuntimeVisual(
                    effect.RuntimeVisual,
                    string.IsNullOrWhiteSpace(effect.EffectId) ? "SkillEffectZone" : $"SkillEffectZone_{effect.EffectId}",
                    center,
                    Quaternion.identity);
                if (instance != null)
                {
                    SkillExecutionUtility.ApplyPrefabScale(instance.transform, effect.Radius, snapshot);
                    Physics2D.SyncTransforms();
                }
            }
            else if (effect.SkillEffectPrefab != null && context.CombatManager.Effects != null)
            {
                instance = context.CombatManager.Effects.InstantiateSkillPrefab(effect.SkillEffectPrefab, center, Quaternion.identity);
                if (instance != null)
                {
                    SkillExecutionUtility.ApplyPrefabScale(instance.transform, effect.Radius, snapshot);
                    Physics2D.SyncTransforms();
                }
            }

            if (instance == null)
            {
                instance = context.CombatManager.Effects.CreateRuntimeSkillObject(
                    string.IsNullOrWhiteSpace(effect.EffectId) ? "SkillEffectZone" : $"SkillEffectZone_{effect.EffectId}",
                    center,
                    Quaternion.identity);
            }

            var actor = instance.GetComponent<InGameZoneSkillActor>();
            if (actor == null)
            {
                actor = instance.AddComponent<InGameZoneSkillActor>();
            }

            actor.Initialize(
                context.CombatManager,
                context.CasterEntry,
                context.Roster,
                targeting,
                center,
                radius,
                coverAll,
                duration,
                tickInterval,
                int.MaxValue,
                damage,
                effect.Attribute,
                statusSpec,
                context.Runtime,
                snapshot,
                System.Array.Empty<SkillEffectDefinition>(),
                context.Caster,
                true,
                snapshot != null ? snapshot.CritChanceBonus : 0f,
                snapshot != null ? snapshot.CritDamageBonus : 0f);
            return true;
        }

        /*
         * 비주얼을 생성한다.
         */
        private static void SpawnVisual(SkillExecutionContext context, SkillEffectDefinition effect, Vector2 center)
        {
            if (effect == null || context == null || context.CombatManager == null || context.CombatManager.Effects == null)
            {
                return;
            }

            if (EffectVisualUtility.HasVisual(effect.RuntimeVisual))
            {
                context.CombatManager.Effects.SpawnTransient(
                    effect.RuntimeVisual,
                    string.IsNullOrWhiteSpace(effect.EffectId) ? "SkillEffectVisual" : $"SkillEffectVisual_{effect.EffectId}",
                    center,
                    Quaternion.identity,
                    1f);
            }
            else if (effect.SkillEffectPrefab != null)
            {
                context.CombatManager.Effects.SpawnTransient(
                    effect.SkillEffectPrefab,
                    center,
                    Quaternion.identity,
                    1f);
            }
        }

        /*
         * 비주얼 대상을 생성한다.
         */
        private static void SpawnVisualOnTargets(
            SkillExecutionContext context,
            SkillEffectDefinition effect,
            IReadOnlyList<UnitRosterEntry> targets,
            float duration)
        {
            if (effect == null
                || context == null
                || context.CombatManager == null
                || context.CombatManager.Effects == null
                || targets == null)
            {
                return;
            }

            var lifetime = Mathf.Max(0.1f, duration);
            for (var i = 0; i < targets.Count; i++)
            {
                var target = targets[i];
                if (target == null || target.Transform == null)
                {
                    continue;
                }

                GameObject instance = null;
                if (EffectVisualUtility.HasVisual(effect.RuntimeVisual))
                {
                    instance = context.CombatManager.Effects.CreateRuntimeVisual(
                        effect.RuntimeVisual,
                        string.IsNullOrWhiteSpace(effect.EffectId) ? "SkillEffectVisual" : $"SkillEffectVisual_{effect.EffectId}",
                        target.Transform.position,
                        Quaternion.identity);
                }
                else if (effect.SkillEffectPrefab != null)
                {
                    instance = context.CombatManager.Effects.InstantiateSkillPrefab(
                        effect.SkillEffectPrefab,
                        target.Transform.position,
                        Quaternion.identity);
                }

                if (instance != null)
                {
                    context.CombatManager.Effects.AttachToTarget(instance, target.Transform, lifetime, Vector3.zero);
                }
            }
        }

        /*
         * 반경을 결정한다.
         */
        private static float ResolveRadius(SkillEffectDefinition effect, SkillExecutionSnapshot snapshot)
        {
            return SkillAreaUtility.ResolveRadius(effect != null ? effect.Radius : 0f, snapshot);
        }
    }
}

