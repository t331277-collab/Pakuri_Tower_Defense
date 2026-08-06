/*
 * 역할: 스킬 실행과 전투 사건이 공유할 기준 매뉴얼을 정의한다.
 * 시전자, 대상, 조준, 사건값, 재시전 정보를 한 시점의 입력으로 고정한다.
 */

using Pakuri.Combat;
using UnityEngine;

namespace Pakuri.InGame
{

    /// 기본 스킬과 조건부 스킬이 같은 판단 기준을 공유하게 한다.
    public sealed class SkillExecutionContext
    {

        /// 일반 시전과 조건부 시전에 필요한 전투 기준을 한 입력으로 고정한다.
        public SkillExecutionContext(
            InGameCombatManager combatManager,
            UnitSpawnManager roster,
            CombatUnitEntry casterEntry,
            SkillExecutionState runtime,
            UnitCombatState eventTarget = null,
            bool hasManualAimDirection = false,
            Vector2 manualAimDirection = default,
            bool hasManualTargetPoint = false,
            Vector2 manualTargetPoint = default,
            int recastGeneration = 0,
            bool lockToEventTarget = false,
            bool publishSkillLifecycleEvents = true,
            bool applyDamageMultiplierToShield = true,
            string sourceSkillName = null)
        {
            CombatManager = combatManager;
            Roster = roster;
            CasterEntry = casterEntry;
            Runtime = runtime;
            Source = casterEntry != null ? casterEntry.Model : null;
            SourceSkillName = string.IsNullOrWhiteSpace(sourceSkillName)
                ? runtime != null && runtime.Data != null ? runtime.Data.SkillName : string.Empty
                : sourceSkillName;
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
        public SkillExecutionContext(
            UnitCombatState source,
            string sourceSkillName,
            UnitCombatState eventTarget,
            Vector2 eventCenter,
            float eventDamage,
            int hitCount,
            SkillExecutionState executionData,
            SkillExecutionContext executionContext = null)
        {
            Source = source;
            SourceSkillName = sourceSkillName ?? string.Empty;
            EventTarget = eventTarget;
            EventCenter = eventCenter;
            EventDamage = eventDamage;
            HitCount = Mathf.Max(0, hitCount);
            ExecutionData = executionData;
            IsTrigger = executionData != null && executionData.IsTrigger;
            CopyExecutionValues(executionContext);
        }

        public InGameCombatManager CombatManager { get; private set; }

        public UnitSpawnManager Roster { get; private set; }

        public CombatUnitEntry CasterEntry { get; private set; }

        public SkillExecutionState Runtime { get; private set; }

        public UnitCombatState Source { get; }

        public string SourceSkillName { get; }

        public UnitCombatState EventTarget { get; }

        public Vector2 EventCenter { get; }

        public float EventDamage { get; }

        public int HitCount { get; }

        public SkillExecutionState ExecutionData { get; }

        internal bool IsTrigger { get; set; }

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
        private void CopyExecutionValues(SkillExecutionContext executionContext)
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
            IsTrigger = executionContext.IsTrigger;
        }
    }
}
