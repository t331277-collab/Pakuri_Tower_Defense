/*
 * 역할: 스킬 실행과 전투 사건이 공유할 기준 매뉴얼을 정의한다.
 * 책임: 시전자, 대상, 조준, 사건값, 재시전 정보를 한 시점의 입력으로 고정한다.
 */

using Pakuri.Combat;
using UnityEngine;

namespace Pakuri.InGame
{

    /// 기본 스킬과 조건부 스킬이 같은 판단 기준을 공유하게 한다.
    public sealed class SkillActionContext
    {

        /// 일반 시전과 조건부 시전에 필요한 전투 기준을 한 입력으로 고정한다.
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

        /// 발생한 사건에 기존 시전 기준을 이어 붙여 후속 판정을 준비한다.
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
            TriggerExecutionState = executionData != null
                ? executionData.TriggerExecutionState
                : null;
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

        internal SkillTrigger.TriggerExecutionState TriggerExecutionState { get; set; }

        public bool HasManualAimDirection { get; private set; }

        public Vector2 ManualAimDirection { get; private set; }

        public bool HasManualTargetPoint { get; private set; }

        public Vector2 ManualTargetPoint { get; private set; }

        public int RecastGeneration { get; private set; }

        public bool LockToEventTarget { get; private set; }

        public bool PublishSkillLifecycleEvents { get; private set; }

        public bool ApplyDamageMultiplierToShield { get; private set; }

        public UnitCombatState Caster => CasterEntry != null ? CasterEntry.Model : Source;

        /// 후속 사건도 원래 시전의 조준과 재시전 제한을 따르게 한다.
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
            TriggerExecutionState = executionContext.TriggerExecutionState;
        }
    }
}
