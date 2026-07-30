/*
 * 역할: 지속 Zone 런타임 동작.
 * 책임: Zone 배치·recast·주기·Collider 판정·피해·상태·비주얼 수명과 완료를 소유한다.
 */

using System;
using System.Collections.Generic;
using Pakuri.Combat;
using Pakuri.Data;
using UnityEngine;

namespace Pakuri.InGame
{

    /// ZoneSkillActor 런타임 오브젝트를 나타내며 모델과 Unity 컴포넌트를 연결한다.
    public partial class ZoneSkillActor : MonoBehaviour
    {

        private InGameCombatManager combatManager;
        private CombatUnitEntry casterEntry;
        private UnitSpawnManager roster;
        private SkillTargetingSpec targeting;
        private Vector2 center;
        private float radius;
        private bool coverAll;
        private float remainingDuration;
        private float tickInterval;
        private float tickRemaining;
        private int maxHitTargetCount;
        private float damage;
        private DamageAttribute attribute;
        private ProjectileStatusHitSpec statusSpec;
        private SkillUseState runtime;
        private SkillExecutionData snapshot;
        private UnitCombatState sourceModel;
        private bool criticalAllowed;
        private float critChanceBonus;
        private float critDamageBonus;
        private Collider2D[] prefabHitboxColliders;
        private bool usePrefabHitbox;
        private int recastGeneration;

        /// 전달된 런타임 입력값을 사용해 소유한 런타임 상태를 초기화한다.
        public void Initialize(
            InGameCombatManager manager,
            CombatUnitEntry sourceEntry,
            UnitSpawnManager unitRoster,
            SkillTargetingSpec targetingSpec,
            Vector2 areaCenter,
            float areaRadius,
            bool areaCoversAll,
            float durationSeconds,
            float tickIntervalSeconds,
            int maxTargetsPerTick,
            float damagePerTick,
            DamageAttribute damageAttribute,
            ProjectileStatusHitSpec onTickStatus,
            SkillUseState sourceRuntime,
            SkillExecutionData executionData,
            UnitCombatState source,
            bool allowCritical,
            float criticalChanceBonus,
            float criticalDamageBonus,
            int generation = 0)
        {
            combatManager = manager;
            casterEntry = sourceEntry;
            roster = unitRoster;
            targeting = targetingSpec;
            center = areaCenter;
            radius = Mathf.Max(0f, areaRadius);
            coverAll = areaCoversAll;
            remainingDuration = Mathf.Max(0.05f, durationSeconds);
            tickInterval = Mathf.Max(0.05f, tickIntervalSeconds);
            tickRemaining = tickInterval;
            maxHitTargetCount = maxTargetsPerTick <= 0 ? int.MaxValue : maxTargetsPerTick;
            damage = Mathf.Max(0f, damagePerTick);
            attribute = damageAttribute;
            statusSpec = onTickStatus;
            runtime = sourceRuntime;
            snapshot = executionData;
            sourceModel = source;
            criticalAllowed = allowCritical;
            critChanceBonus = criticalChanceBonus;
            critDamageBonus = criticalDamageBonus;
            recastGeneration = Mathf.Max(0, generation);
            prefabHitboxColliders = GetComponentsInChildren<Collider2D>();
            usePrefabHitbox = !coverAll
                && prefabHitboxColliders != null
                && prefabHitboxColliders.Length > 0;
            EffectVisualBuilder.ConfigureZoneEffect(
                gameObject,
                center,
                radius,
                coverAll,
                usePrefabHitbox);
            ApplyCurrentAreaTick();
        }

        /// 현재 Unity 프레임에서 Update 갱신 동작을 진행한다.
        private void Update()
        {
            var deltaTime = Time.deltaTime;
            remainingDuration -= deltaTime;
            tickRemaining -= deltaTime;
            while (remainingDuration > 0f && tickRemaining <= 0f)
            {
                tickRemaining += tickInterval;
                ApplyCurrentAreaTick();
            }

            if (remainingDuration <= 0f)
            {
                TryExecuteExpireEffects();
                combatManager.Effects.RemoveEffect(gameObject);
            }
        }

        /// ExecuteExpireEffects 작업을 시도하고 성공 여부를 반환한다.
        private void TryExecuteExpireEffects()
        {
            if (combatManager != null && casterEntry != null && roster != null)
            {
                var lifecycleContext = new SkillExecutionContext(
                    combatManager,
                    roster,
                    casterEntry,
                    runtime,
                    recastGeneration: recastGeneration);
                SkillTrigger.PublishLifecycleEvent(
                    SkillTriggerEvent.OnExpire,
                    new SkillActionContext(
                        casterEntry.Model,
                        snapshot != null ? snapshot.SkillId : string.Empty,
                        null,
                        center,
                        0f,
                        0,
                        snapshot,
                        lifecycleContext));
            }
        }

        /// CurrentAreaTick를 적용한다.
        private bool ApplyCurrentAreaTick()
        {
            if (usePrefabHitbox)
            {
                return ApplyColliderAreaTick(
                    combatManager,
                    casterEntry,
                    roster,
                    targeting,
                    prefabHitboxColliders,
                    maxHitTargetCount,
                    damage,
                    attribute,
                    statusSpec,
                    sourceModel,
                    SourceSkillId(snapshot, runtime),
                    runtime,
                    criticalAllowed,
                    critChanceBonus,
                    critDamageBonus,
                    snapshot);
            }

            return SkillExecutionRuleResolver.ApplyAreaHits(
                combatManager,
                casterEntry,
                roster,
                targeting,
                center,
                radius,
                coverAll,
                damage,
                attribute,
                statusSpec,
                sourceModel,
                SourceSkillId(snapshot, runtime),
                runtime,
                criticalAllowed,
                critChanceBonus,
                critDamageBonus,
                maxHitTargetCount,
                snapshot);
        }

        /// 전달된 런타임 입력값을 사용해 ColliderAreaTick를 적용한다.
        internal static bool ApplyColliderAreaTick(
            InGameCombatManager manager,
            CombatUnitEntry sourceEntry,
            UnitSpawnManager unitRoster,
            SkillTargetingSpec targetingSpec,
            Collider2D[] hitboxColliders,
            int maxTargetsPerTick,
            float damagePerTick,
            DamageAttribute damageAttribute,
            ProjectileStatusHitSpec onHitStatus,
            UnitCombatState source,
            string sourceSkillId,
            SkillUseState sourceRuntime,
            bool criticalAllowed,
            float critChanceBonus,
            float critDamageBonus,
            SkillExecutionData executionData)
        {
            if (manager == null || sourceEntry == null || unitRoster == null || hitboxColliders == null || hitboxColliders.Length == 0)
            {
                return false;
            }

            var candidates = SkillTargeting.TargetList(sourceEntry, unitRoster, targetingSpec);
            var eligibleTargets = new List<CombatUnitEntry>();
            UnitCollisionResolver.CollectTargets(
                unitRoster,
                candidates,
                hitboxColliders,
                Vector2.zero,
                eligibleTargets);

            var routed = SkillExecutionRuleResolver.ApplyResolvedHits(
                manager,
                sourceEntry,
                unitRoster,
                eligibleTargets,
                maxTargetsPerTick,
                damagePerTick,
                damageAttribute,
                onHitStatus,
                source,
                sourceSkillId,
                sourceRuntime,
                criticalAllowed,
                critChanceBonus,
                critDamageBonus,
                executionData);
            return routed;
        }

        /// 전달된 런타임 입력값을 사용해 SourceSkillId 결과값을 생성해 반환한다.
        private static string SourceSkillId(SkillExecutionData executionData, SkillUseState sourceRuntime)
        {
            if (sourceRuntime != null && !string.IsNullOrWhiteSpace(sourceRuntime.SkillId))
            {
                return sourceRuntime.SkillId;
            }

            if (executionData != null)
            {
                return executionData.SkillId;
            }

            return string.Empty;
        }
    }

    /// Zone 계열 판정과 적용을 소유한다.
    public partial class ZoneSkillActor
    {
        /// 전달된 런타임 입력값으로 Zone Actor를 생성한다.
        internal bool InitializeExecution(
            SkillExecutionContext context,
            SkillExecutionData snapshot)
        {
            var centers = snapshot.PreparedCenters;
            var radius = snapshot.PreparedRadius;
            var duration = snapshot.PreparedDuration;
            var tickInterval = snapshot.PreparedTickInterval;
            var hitTargetCount = snapshot.PreparedHitTargetCount;
            var damage = snapshot.PreparedDamage;
            var attribute = snapshot.PreparedDamageAttribute;
            var statusSpec = snapshot.PreparedStatus;
            var coverAll = snapshot.PreparedCoverAll;
            var effects = context.CombatManager.Effects;
            var runtimeVisual = snapshot.PreparedRuntimeVisual;
            var prefab = snapshot.SkillEffectPrefab;

            var routed = false;
            for (var i = 0; i < centers.Count; i++)
            {
                var center = centers[i];
                var objectName = snapshot.PreparedIsRecast ? "InGameRecastZone" : "ZoneSkill";
                if (!string.IsNullOrWhiteSpace(snapshot.SkillId))
                {
                    objectName += "_" + snapshot.SkillId;
                }

                var instance = effects.CreateEffect(new EffectCreateRequest(
                    runtimeVisual,
                    prefab,
                    objectName,
                    center,
                    Quaternion.identity,
                    null,
                    null,
                    false,
                    true,
                    true));

                EffectVisualBuilder.ConfigureAreaEffect(
                    instance,
                    snapshot.PreparedBaseRadius,
                    snapshot.RadiusMultiplier,
                    snapshot.RadiusBonus,
                    snapshot.PreparedVisualRadiusMultiplier);

                var actor = instance.GetComponent<ZoneSkillActor>();
                if (actor == null)
                {
                    actor = instance.AddComponent<ZoneSkillActor>();
                }

                actor.Initialize(
                    context.CombatManager,
                    context.CasterEntry,
                    context.Roster,
                    snapshot.PreparedTargeting,
                    center,
                    radius,
                    coverAll,
                    duration,
                    tickInterval,
                    hitTargetCount,
                    damage,
                    attribute,
                    statusSpec,
                    context.Runtime,
                    snapshot,
                    context.Caster,
                    snapshot.PreparedCriticalAllowed,
                    snapshot != null ? snapshot.CritChanceBonus : 0f,
                    snapshot != null ? snapshot.CritDamageBonus : 0f,
                    snapshot.PreparedRecastGeneration);
                routed = true;
            }

            context.CombatManager.Effects.RemoveEffect(gameObject);
            return routed;
        }

    }
}
