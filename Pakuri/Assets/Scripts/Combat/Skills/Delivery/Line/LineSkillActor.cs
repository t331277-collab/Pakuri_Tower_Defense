/*
 * 역할: 런타임 Line Hit Actor 동작.
 * 책임: Line 타기팅·반복·Hitbox·피해·상태·넉백·비주얼 수명과 완료를 소유한다.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using Pakuri.Combat;
using Pakuri.Data;
using UnityEngine;

namespace Pakuri.InGame
{

    /// LineSkillActor 런타임 오브젝트를 나타내며 모델과 Unity 컴포넌트를 연결한다.
    public partial class LineSkillActor : MonoBehaviour
    {

        private InGameCombatManager combatManager;
        private EffectManager effectManager;
        private CombatUnitEntry casterEntry;
        private UnitSpawnManager roster;
        private SkillTargetingSpec targeting;
        private Vector2 origin;
        private Vector2 direction = Vector2.right;
        private float length;
        private float width;
        private float knockbackDistance;
        private float remainingDuration;
        private float tickInterval;
        private float tickRemaining;
        private float damage;
        private DamageAttribute attribute;
        private ProjectileStatusHitSpec statusSpec;
        private SkillUseState runtime;
        private SkillExecutionData executionData;
        private UnitCombatState sourceModel;
        private string sourceSkillId;
        private bool criticalAllowed;
        private float critChanceBonus;
        private float critDamageBonus;
        private bool visualOnly;
        private bool executionActor;
        private bool executionLaunchFinished;
        private int pendingOperations;
        private readonly HashSet<string> appliedBaseStatusTargets = new HashSet<string>();
        private readonly List<CombatUnitEntry> collisionTargets = new List<CombatUnitEntry>();
        private readonly Collider2D[] lineHitboxes = new Collider2D[1];
        private BoxCollider2D lineHitbox;

        /// 전달된 런타임 입력값을 사용해 소유한 런타임 상태를 초기화한다.
        public void Initialize(
            InGameCombatManager manager,
            CombatUnitEntry sourceEntry,
            UnitSpawnManager unitRoster,
            SkillTargetingSpec targetingSpec,
            Vector2 lineOrigin,
            Vector2 lineDirection,
            float lineLength,
            float lineWidth,
            float lineKnockbackDistance,
            float durationSeconds,
            float tickIntervalSeconds,
            float damagePerTick,
            DamageAttribute damageAttribute,
            ProjectileStatusHitSpec onHitStatus,
            SkillUseState sourceRuntime,
            SkillExecutionData snapshot,
            UnitCombatState source,
            string skillId,
            bool allowCritical,
            float criticalChanceBonus,
            float criticalDamageBonus)
        {
            combatManager = manager;
            effectManager = manager.Effects;
            visualOnly = false;
            executionActor = false;
            executionLaunchFinished = false;
            pendingOperations = 0;
            casterEntry = sourceEntry;
            roster = unitRoster;
            targeting = targetingSpec;
            origin = lineOrigin;
            direction = lineDirection.sqrMagnitude > 0.0001f ? lineDirection.normalized : Vector2.right;
            length = Mathf.Max(0.1f, lineLength);
            width = Mathf.Max(0.1f, lineWidth);
            knockbackDistance = Mathf.Max(0f, lineKnockbackDistance);
            remainingDuration = Mathf.Max(0.05f, durationSeconds);
            tickInterval = Mathf.Max(0.01f, tickIntervalSeconds);
            tickRemaining = tickInterval;
            damage = Mathf.Max(0f, damagePerTick);
            attribute = damageAttribute;
            statusSpec = onHitStatus;
            runtime = sourceRuntime;
            executionData = snapshot;
            sourceModel = source;
            sourceSkillId = skillId;
            criticalAllowed = allowCritical;
            critChanceBonus = criticalChanceBonus;
            critDamageBonus = criticalDamageBonus;
            appliedBaseStatusTargets.Clear();

            ConfigureVisual();
            ConfigureHitbox();
            ApplyLineTick();
        }

        /// 전달된 런타임 입력값을 사용해 VisualLifetime를 초기화한다.
        public float InitializeVisualLifetime(
            EffectManager manager,
            float durationSeconds)
        {
            effectManager = manager;
            visualOnly = true;
            executionActor = false;
            executionLaunchFinished = false;
            pendingOperations = 0;
            remainingDuration = Mathf.Max(0.05f, durationSeconds);
            return remainingDuration;
        }

        /// Line 실행 Actor의 작업 추적을 시작한다.
        private void BeginExecution(EffectManager manager)
        {
            effectManager = manager;
            executionActor = true;
            executionLaunchFinished = false;
            pendingOperations = 0;
            visualOnly = false;
        }

        /// Line 실행 초기화를 끝낸다.
        private void FinishExecution()
        {
            executionLaunchFinished = true;
            TryCompleteExecution();
        }

        /// Line 지연 작업을 이 Actor의 수명에 연결한다.
        private void StartTrackedCoroutine(IEnumerator operation)
        {
            pendingOperations++;
            StartCoroutine(TrackOperation(operation));
        }

        /// Line 지연 작업 완료를 추적한다.
        private IEnumerator TrackOperation(IEnumerator operation)
        {
            yield return operation;
            pendingOperations = Mathf.Max(0, pendingOperations - 1);
            TryCompleteExecution();
        }

        /// 모든 Line 실행 작업이 끝났으면 삭제를 요청한다.
        private void TryCompleteExecution()
        {
            if (executionActor && executionLaunchFinished && pendingOperations == 0)
            {
                effectManager.RemoveEffect(gameObject);
            }
        }

        /// LineTick를 적용한다.
        private bool ApplyLineTick()
        {
            if (combatManager == null || casterEntry == null || roster == null || lineHitbox == null)
            {
                return false;
            }

            var candidates = SkillTargeting.TargetList(casterEntry, roster, targeting);
            UnitCollisionResolver.CollectTargets(
                roster,
                candidates,
                lineHitboxes,
                Vector2.zero,
                collisionTargets);
            var routed = false;
            for (var i = 0; i < collisionTargets.Count; i++)
            {
                var target = collisionTargets[i];
                if (target == null || !target.IsAlive || target.Model == null || target.Transform == null)
                {
                    continue;
                }

                var hitPosition = (Vector2)target.Transform.position;
                var resolvedDamage = Mathf.Max(0f, damage);
                var finalDamageMultiplier = executionData != null
                    ? Mathf.Max(0f, executionData.DamageMultiplier) * SkillExecutionRuleResolver.ConditionalDamageMultiplier(executionData, target.Model)
                    : 1f;
                var damageResult = combatManager.ApplyDamage(target.Model, resolvedDamage, attribute, sourceModel, criticalAllowed, critChanceBonus, critDamageBonus, sourceSkillId, finalDamageMultiplier: finalDamageMultiplier);
                TryApplyKnockback(target, direction, knockbackDistance);
                if (!damageResult.IsDead)
                {
                    var targetKey = TargetKey(target.Model);
                    TryApplyStatus(combatManager, target.Model, statusSpec, sourceModel, targetKey, appliedBaseStatusTargets);
                }
                SkillExecutionRuleResolver.ApplyHitEnhancements(
                    combatManager,
                    roster,
                    runtime,
                    executionData,
                    casterEntry,
                    sourceModel,
                    sourceSkillId,
                    target,
                    hitPosition,
                    resolvedDamage);
                routed = true;
            }

            return routed;
        }

        /// 현재 Unity 프레임에서 Update 갱신 동작을 진행한다.
        private void Update()
        {
            if (executionActor)
            {
                return;
            }

            var deltaTime = Time.deltaTime;
            remainingDuration -= deltaTime;
            if (!visualOnly)
            {
                tickRemaining -= deltaTime;
                if (remainingDuration > 0f && tickRemaining <= 0f)
                {
                    tickRemaining += tickInterval;
                    ApplyLineTick();
                }
            }

            if (remainingDuration <= 0f)
            {
                effectManager.RemoveEffect(gameObject);
            }
        }

        /// ConfigureVisual 작업을 수행한다.
        private void ConfigureVisual()
        {
            transform.position = origin + direction * (length * 0.5f);
            var angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);

            var spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null && spriteRenderer.sprite != null)
            {
                var size = spriteRenderer.sprite.bounds.size;
                var scale = transform.localScale;
                if (size.x > 0.0001f)
                {
                    scale.x = Mathf.Sign(scale.x == 0f ? 1f : scale.x) * (length / size.x);
                }

                if (size.y > 0.0001f)
                {
                    scale.y = Mathf.Sign(scale.y == 0f ? 1f : scale.y) * (width / size.y);
                }

                transform.localScale = scale;
            }
        }

        /// ConfigureHitbox 작업을 수행한다.
        private void ConfigureHitbox()
        {
            lineHitbox = GetComponent<BoxCollider2D>();
            if (lineHitbox == null)
            {
                lineHitbox = gameObject.AddComponent<BoxCollider2D>();
            }

            var scale = transform.lossyScale;
            lineHitbox.size = new Vector2(
                length / Mathf.Max(0.0001f, Mathf.Abs(scale.x)),
                width / Mathf.Max(0.0001f, Mathf.Abs(scale.y)));
            lineHitbox.offset = Vector2.zero;
            lineHitbox.isTrigger = true;
            lineHitboxes[0] = lineHitbox;
        }

        /// 전달된 런타임 입력값을 사용해 ApplyKnockback 작업을 시도하고 성공 여부를 반환한다.
        private static void TryApplyKnockback(CombatUnitEntry target, Vector2 normalizedDirection, float distance)
        {
            if (target == null
                || target.Transform == null
                || distance <= 0f
                || normalizedDirection.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            target.Transform.position += (Vector3)(normalizedDirection.normalized * distance);
        }

        /// 전달된 런타임 입력값을 사용해 ApplyStatus 작업을 시도하고 성공 여부를 반환한다.
        private static void TryApplyStatus(
            InGameCombatManager manager,
            UnitCombatState target,
            ProjectileStatusHitSpec status,
            UnitCombatState source,
            string targetKey,
            HashSet<string> appliedTargets)
        {
            if (status == null || !status.Enabled)
            {
                return;
            }

            if (appliedTargets != null && !string.IsNullOrWhiteSpace(targetKey) && appliedTargets.Contains(targetKey))
            {
                return;
            }

            if (StatusCombatRules.ApplyStatus(manager, target, status, source)
                && appliedTargets != null
                && !string.IsNullOrWhiteSpace(targetKey))
            {
                appliedTargets.Add(targetKey);
            }
        }

        /// 전달된 target 값을 사용해 TargetKey 결과값을 생성해 반환한다.
        private static string TargetKey(UnitCombatState target)
        {
            var unitId = target != null && target.Identity != null ? target.Identity.UnitId : null;
            if (!string.IsNullOrWhiteSpace(unitId))
            {
                return unitId;
            }

            return target != null ? target.GetHashCode().ToString() : string.Empty;
        }

    }

    /// Line 계열 판정과 적용을 소유한다.
    public partial class LineSkillActor
    {
        /// 전달된 런타임 입력값을 사용해 설정된 런타임 작업를 실행한다.
        internal bool InitializeExecution(
            SkillExecutionContext context,
            SkillExecutionData snapshot,
            LineSkillDefinition skill)
        {
            BeginExecution(context.CombatManager.Effects);
            var origin = context.CasterEntry.Transform != null
                ? context.CasterEntry.Transform.position
                : Vector3.zero;
            var repeatCount = CastRepeatCount(skill, snapshot);
            var directions = CastDirections(context, skill, origin, repeatCount);
            if (directions.Count == 0)
            {
                FinishExecution();
                return false;
            }

            if (!ExecuteOnce(context, snapshot, skill, origin, directions[0]))
            {
                FinishExecution();
                return false;
            }

            if (repeatCount > 1)
            {
                StartTrackedCoroutine(ExecuteRepeatedLineCasts(
                    context,
                    snapshot,
                    skill,
                    origin,
                    directions,
                    CastRepeatInterval(skill)));
            }

            FinishExecution();
            return true;
        }

        /// 전달된 런타임 입력값을 사용해 RepeatedLineCasts를 실행한다.
        private IEnumerator ExecuteRepeatedLineCasts(
            SkillExecutionContext context,
            SkillExecutionData snapshot,
            LineSkillDefinition skill,
            Vector2 origin,
            IReadOnlyList<Vector2> directions,
            float repeatIntervalSeconds)
        {
            for (var i = 1; i < directions.Count; i++)
            {
                yield return new WaitForSeconds(repeatIntervalSeconds);
                if (context == null
                    || context.CombatManager == null
                    || context.CasterEntry == null
                    || context.Caster == null
                    || skill == null)
                {
                    yield break;
                }

                ExecuteOnce(context, snapshot, skill, origin, directions[i]);
            }
        }

        /// 전달된 런타임 입력값을 사용해 CastDirections 결과값을 생성해 반환한다.
        private static List<Vector2> CastDirections(
            SkillExecutionContext context,
            LineSkillDefinition skill,
            Vector2 origin,
            int repeatCount)
        {
            var directions = new List<Vector2>(repeatCount);
            if (context.HasManualAimDirection)
            {
                var direction = context.ManualAimDirection;
                if (direction.sqrMagnitude <= 0.0001f)
                {
                    return directions;
                }

                direction.Normalize();
                for (var i = 0; i < repeatCount; i++)
                {
                    directions.Add(direction);
                }

                return directions;
            }

            var target = SkillTargeting.FindNearestTarget(context.CasterEntry, context.Roster, skill.Targeting);
            var primaryDirection = SkillTargeting.DirectionToTarget(origin, target);
            if (primaryDirection.sqrMagnitude <= 0.0001f || target.Transform == null)
            {
                return directions;
            }

            var centers = SkillTargeting.TargetAnchoredCenters(
                context,
                skill.Targeting,
                target.Transform.position,
                repeatCount,
                false,
                SkillDeploymentRepeatMode.RepeatNearest);
            for (var i = 0; i < centers.Count; i++)
            {
                var direction = centers[i] - origin;
                if (direction.sqrMagnitude <= 0.0001f)
                {
                    continue;
                }

                directions.Add(direction.normalized);
            }

            return directions;
        }

        /// 전달된 런타임 입력값을 사용해 Once를 실행한다.
        private static bool ExecuteOnce(
            SkillExecutionContext context,
            SkillExecutionData snapshot,
            LineSkillDefinition skill,
            Vector2 origin,
            Vector2 direction)
        {
            var damage = DamageCalculator.CalculateRawDamage(context.Caster, skill.DamagePerTick);
            var attribute = skill.DamagePerTick != null ? skill.DamagePerTick.Element : skill.Element;
            var statusSpec = SkillStatus.StatusSpec(skill.OnHitStatus, snapshot);
            var length = LineLength(skill);
            var width = LineWidth(skill, snapshot);
            var knockbackDistance = KnockbackDistance(skill, snapshot);
            var duration = Duration(skill, snapshot);
            var tickInterval = TickInterval(skill, snapshot);
            var center = (Vector2)origin + direction * (length * 0.5f);
            var effects = context.CombatManager.Effects;
            var runtimeVisual = skill.RuntimeVisual;
            var prefab = skill.SkillEffectPrefab;
            if (snapshot != null && snapshot.SkillEffectPrefab != null)
            {
                prefab = snapshot.SkillEffectPrefab;
            }
            if (effects == null)
            {
                return false;
            }

            var rotation = EffectVisualBuilder.Rotation(direction);
            var objectName = "LineSkill";
            if (!string.IsNullOrWhiteSpace(skill.SkillId))
            {
                objectName = "LineSkill_" + skill.SkillId;
            }

            var instance = effects.CreateEffect(new EffectCreateRequest(
                runtimeVisual,
                prefab,
                objectName,
                center,
                rotation,
                null,
                null,
                false,
                false,
                true));
            if (instance == null)
            {
                return false;
            }

            var actor = instance.GetComponent<LineSkillActor>();
            if (actor == null)
            {
                actor = instance.AddComponent<LineSkillActor>();
            }

            actor.Initialize(
                context.CombatManager,
                context.CasterEntry,
                context.Roster,
                skill.Targeting,
                origin,
                direction,
                length,
                width,
                knockbackDistance,
                duration,
                tickInterval,
                damage,
                attribute,
                statusSpec,
                context.Runtime,
                snapshot,
                context.Caster,
                skill.SkillId,
                skill.DamagePerTick != null && skill.DamagePerTick.CriticalAllowed,
                snapshot != null ? snapshot.CritChanceBonus : 0f,
                snapshot != null ? snapshot.CritDamageBonus : 0f);
            SkillTrigger.PublishLifecycleEvent(
                SkillTriggerEvent.OnDeploymentCast,
                new SkillActionContext(context.Caster, skill.SkillId, null, center, 0f, 0, snapshot, context));
            return true;
        }

        /// 전달된 skill 값을 사용해 LineLength 결과값을 생성해 반환한다.
        private static float LineLength(LineSkillDefinition skill)
        {
            return Mathf.Max(0.1f, skill != null ? skill.LineLength : 0f);
        }

        /// 전달된 런타임 입력값을 사용해 CastRepeatCount 결과값을 생성해 반환한다.
        private static int CastRepeatCount(LineSkillDefinition skill, SkillExecutionData snapshot)
        {
            var baseCount = skill != null ? skill.CastRepeatCount : 1;
            var bonus = snapshot != null ? snapshot.LineCastRepeatCountBonus : 0;
            return Mathf.Max(1, baseCount + bonus);
        }

        /// 전달된 skill 값을 사용해 CastRepeatInterval 결과값을 생성해 반환한다.
        private static float CastRepeatInterval(LineSkillDefinition skill)
        {
            return Mathf.Max(0f, skill != null ? skill.CastRepeatIntervalSeconds : 0f);
        }

        /// 전달된 런타임 입력값을 사용해 Duration 결과값을 생성해 반환한다.
        private static float Duration(LineSkillDefinition skill, SkillExecutionData snapshot)
        {
            var timing = skill != null ? skill.Timing : null;
            var duration = timing != null && timing.ActiveDuration > 0f
                ? timing.ActiveDuration
                : TickInterval(skill, snapshot);
            if (snapshot != null)
            {
                duration = duration * Mathf.Max(0f, snapshot.DurationMultiplier) + snapshot.DurationBonus;
            }

            return Mathf.Max(0.05f, duration);
        }

        /// 전달된 런타임 입력값을 사용해 LineWidth 결과값을 생성해 반환한다.
        private static float LineWidth(LineSkillDefinition skill, SkillExecutionData snapshot)
        {
            var width = skill != null ? skill.LineWidth : 0f;
            if (snapshot != null)
            {
                width *= LineVisualWidthScale(snapshot);
            }

            return Mathf.Max(0.1f, width);
        }

        /// 전달된 런타임 입력값을 사용해 KnockbackDistance 결과값을 생성해 반환한다.
        private static float KnockbackDistance(LineSkillDefinition skill, SkillExecutionData snapshot)
        {
            var distance = skill != null ? Mathf.Max(0f, skill.KnockbackDistance) : 0f;
            if (snapshot != null)
            {
                distance *= Mathf.Max(0f, snapshot.KnockbackDistanceMultiplier);
            }

            return Mathf.Max(0f, distance);
        }

        /// 전달된 snapshot 값을 사용해 LineVisualWidthScale 결과값을 생성해 반환한다.
        private static float LineVisualWidthScale(SkillExecutionData snapshot)
        {
            return snapshot != null
                ? Mathf.Max(0.01f, 1f + snapshot.BeamWidthBonus)
                : 1f;
        }

        /// 전달된 런타임 입력값을 사용해 Interval를 경과 시간 기준으로 갱신한다.
        private static float TickInterval(LineSkillDefinition skill, SkillExecutionData snapshot)
        {
            var interval = TickInterval(skill);
            if (snapshot != null)
            {
                interval *= Mathf.Max(0.05f, snapshot.ShotIntervalMultiplier);
            }

            return Mathf.Max(0.05f, interval);
        }

        /// 전달된 skill 값을 사용해 Interval를 경과 시간 기준으로 갱신한다.
        private static float TickInterval(LineSkillDefinition skill)
        {
            var timing = skill != null ? skill.Timing : null;
            return timing != null && timing.TickInterval > 0f
                ? timing.TickInterval
                : 0.1f;
        }

    }
}
