using UnityEngine;

namespace Pakuri.InGame
{
    /*
     * 스킬 실행 정보에 필요한 값을 보관한다.
     */
    public sealed class SkillExecutionContext
    {
        /*
         * 스킬 실행 정보에 필요한 값을 초기화한다.
         */
        public SkillExecutionContext(
            InGameCombatManager combatManager,
            UnitRosterService roster,
            UnitRosterEntry casterEntry,
            SkillRuntimeInstance runtime,
            float deltaTime,
            BaseUnitRuntimeModel eventTarget = null,
            bool hasManualAimDirection = false,
            Vector2 manualAimDirection = default,
            bool hasManualTargetPoint = false,
            Vector2 manualTargetPoint = default,
            int recastGeneration = 0)
        {
            CombatManager = combatManager;
            Roster = roster;
            CasterEntry = casterEntry;
            Runtime = runtime;
            DeltaTime = deltaTime;
            EventTarget = eventTarget;
            HasManualAimDirection = hasManualAimDirection;
            ManualAimDirection = manualAimDirection;
            HasManualTargetPoint = hasManualTargetPoint;
            ManualTargetPoint = manualTargetPoint;
            RecastGeneration = Mathf.Max(0, recastGeneration);
        }

        public InGameCombatManager CombatManager { get; }
        public UnitRosterService Roster { get; }
        public UnitRosterEntry CasterEntry { get; }
        public SkillRuntimeInstance Runtime { get; }
        public float DeltaTime { get; }
        public BaseUnitRuntimeModel EventTarget { get; }
        public bool HasManualAimDirection { get; }
        public Vector2 ManualAimDirection { get; }
        public bool HasManualTargetPoint { get; }
        public Vector2 ManualTargetPoint { get; }
        public int RecastGeneration { get; }

        public BaseUnitRuntimeModel Caster => CasterEntry != null ? CasterEntry.Model : null;
        public SkillRuntimeData SkillRuntimeData => Runtime != null ? Runtime.Data : null;
    }

    /*
     * 스킬 실행 상태에서 사용하는 선택 값을 정의한다.
     */
    public enum SkillExecutionStatus
    {
        None,
        Rejected,
        Routed
    }

    /*
     * 스킬 실행 결과에 필요한 값을 보관한다.
     */
    public sealed class SkillExecutionResult
    {
        public static readonly SkillExecutionResult None = new SkillExecutionResult(
            SkillExecutionStatus.None,
            string.Empty,
            string.Empty);

        /*
         * 스킬 실행 결과에 필요한 값을 초기화한다.
         */
        public SkillExecutionResult(SkillExecutionStatus status, string skillId, string executorName)
        {
            Status = status;
            SkillId = skillId ?? string.Empty;
            ExecutorName = executorName ?? string.Empty;
        }

        public SkillExecutionStatus Status { get; }
        public string SkillId { get; }
        public string ExecutorName { get; }
        public bool Routed => Status == SkillExecutionStatus.Routed;
    }
}
