using UnityEngine;

namespace Pakuri.InGame
{
    public readonly struct SkillExecutionRequest
    {
        public SkillExecutionRequest(
            UnitRosterEntry entry,
            SkillRuntimeInstance runtime,
            UnitRosterService roster,
            InGameCombatManager combatManager,
            float deltaTime,
            bool logRoutedContracts,
            bool hasManualAimDirection,
            Vector2 manualAimDirection,
            bool hasManualTargetPoint,
            Vector2 manualTargetPoint,
            System.Action<UnitRosterEntry> notifyActiveSkillAnimation)
        {
            Entry = entry;
            Runtime = runtime;
            Roster = roster;
            CombatManager = combatManager;
            DeltaTime = deltaTime;
            LogRoutedContracts = logRoutedContracts;
            HasManualAimDirection = hasManualAimDirection;
            ManualAimDirection = manualAimDirection;
            HasManualTargetPoint = hasManualTargetPoint;
            ManualTargetPoint = manualTargetPoint;
            NotifyActiveSkillAnimation = notifyActiveSkillAnimation;
        }

        public UnitRosterEntry Entry { get; }
        public SkillRuntimeInstance Runtime { get; }
        public UnitRosterService Roster { get; }
        public InGameCombatManager CombatManager { get; }
        public float DeltaTime { get; }
        public bool LogRoutedContracts { get; }
        public bool HasManualAimDirection { get; }
        public Vector2 ManualAimDirection { get; }
        public bool HasManualTargetPoint { get; }
        public Vector2 ManualTargetPoint { get; }
        public System.Action<UnitRosterEntry> NotifyActiveSkillAnimation { get; }
    }
}
