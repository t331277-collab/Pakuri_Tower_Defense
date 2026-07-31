/*
 * 역할: 스킬 실행과 사건 전달에 필요한 불변 값.
 * 책임: 실행 기반값과 사건 정보를 하나의 전달 단위로 보관한다.
 */

using Pakuri.Combat;
using UnityEngine;

namespace Pakuri.InGame
{

    /// 실행과 사건 전달에 필요한 불변 값을 묶는다.
    public sealed class SkillActionContext
    {

        /// 스킬 실행의 공통 기반값을 고정한다.
        public SkillActionContext(
            InGameCombatManager combatManager,
            UnitSpawnManager roster,
            CombatUnitEntry casterEntry,
            SkillExecutionData runtime,
            UnitCombatState eventTarget = null,
            bool hasManualAimDirection = false,
            Vector2 manualAimDirection = default,
            bool hasManualTargetPoint = false,
            Vector2 manualTargetPoint = default,
            int recastGeneration = 0,
            bool lockToEventTarget = false,
            bool publishSkillLifecycleEvents = true,
            bool applyDamageMultiplierToShield = true,
            string sourceSkillId = null)
        {
            CombatManager = combatManager;
            Roster = roster;
            CasterEntry = casterEntry;
            Runtime = runtime;
            Source = casterEntry != null ? casterEntry.Model : null;
            SourceSkillId = string.IsNullOrWhiteSpace(sourceSkillId)
                ? runtime != null && runtime.Data != null ? runtime.Data.SkillId : string.Empty
                : sourceSkillId;
            EventTarget = eventTarget;
            EventCenter = casterEntry != null && casterEntry.Transform != null
                ? (Vector2)casterEntry.Transform.position
                : Vector2.zero;
            EventDamage = 0f;
            HitCount = 0;
            ExecutionData = runtime;
            HasManualAimDirection = hasManualAimDirection;
            ManualAimDirection = manualAimDirection;
            HasManualTargetPoint = hasManualTargetPoint;
            ManualTargetPoint = manualTargetPoint;
            RecastGeneration = Mathf.Max(0, recastGeneration);
            LockToEventTarget = lockToEventTarget;
            PublishSkillLifecycleEvents = publishSkillLifecycleEvents;
            ApplyDamageMultiplierToShield = applyDamageMultiplierToShield;
        }

        /// 사건값과 실행 기반값을 한 전달 단위로 합친다.
        public SkillActionContext(
            UnitCombatState source,
            string sourceSkillId,
            UnitCombatState eventTarget,
            Vector2 eventCenter,
            float eventDamage,
            int hitCount,
            SkillExecutionData executionData,
            SkillActionContext executionContext = null)
        {
            Source = source;
            SourceSkillId = sourceSkillId ?? string.Empty;
            EventTarget = eventTarget;
            EventCenter = eventCenter;
            EventDamage = eventDamage;
            HitCount = Mathf.Max(0, hitCount);
            ExecutionData = executionData;
            CopyExecutionValues(executionContext);
        }

        public InGameCombatManager CombatManager { get; private set; }

        public UnitSpawnManager Roster { get; private set; }

        public CombatUnitEntry CasterEntry { get; private set; }

        public SkillExecutionData Runtime { get; private set; }

        public UnitCombatState Source { get; }

        public string SourceSkillId { get; }

        public UnitCombatState EventTarget { get; }

        public Vector2 EventCenter { get; }

        public float EventDamage { get; }

        public int HitCount { get; }

        public SkillExecutionData ExecutionData { get; }

        public bool HasManualAimDirection { get; private set; }

        public Vector2 ManualAimDirection { get; private set; }

        public bool HasManualTargetPoint { get; private set; }

        public Vector2 ManualTargetPoint { get; private set; }

        public int RecastGeneration { get; private set; }

        public bool LockToEventTarget { get; private set; }

        public bool PublishSkillLifecycleEvents { get; private set; }

        public bool ApplyDamageMultiplierToShield { get; private set; }

        public UnitCombatState Caster => CasterEntry != null ? CasterEntry.Model : Source;

        private void CopyExecutionValues(SkillActionContext executionContext)
        {
            if (executionContext == null)
            {
                return;
            }

            CombatManager = executionContext.CombatManager;
            Roster = executionContext.Roster;
            CasterEntry = executionContext.CasterEntry;
            Runtime = executionContext.Runtime;
            HasManualAimDirection = executionContext.HasManualAimDirection;
            ManualAimDirection = executionContext.ManualAimDirection;
            HasManualTargetPoint = executionContext.HasManualTargetPoint;
            ManualTargetPoint = executionContext.ManualTargetPoint;
            RecastGeneration = executionContext.RecastGeneration;
            LockToEventTarget = executionContext.LockToEventTarget;
            PublishSkillLifecycleEvents = executionContext.PublishSkillLifecycleEvents;
            ApplyDamageMultiplierToShield = executionContext.ApplyDamageMultiplierToShield;
        }
    }
}
