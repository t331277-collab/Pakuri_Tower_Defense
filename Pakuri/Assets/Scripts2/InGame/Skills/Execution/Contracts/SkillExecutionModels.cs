using UnityEngine;

namespace Pakuri.InGame
{
    public sealed class SkillExecutionContext
    {
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
        public SkillData SkillData => Runtime != null ? Runtime.Data : null;
    }

    public enum SkillExecutionStatus
    {
        None,
        Rejected,
        Routed
    }

    public sealed class SkillExecutionResult
    {
        public static readonly SkillExecutionResult None = new SkillExecutionResult(
            SkillExecutionStatus.None,
            string.Empty,
            string.Empty);

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
