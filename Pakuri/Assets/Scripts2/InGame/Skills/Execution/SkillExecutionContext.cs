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
            bool hasManualAimDirection = false,
            UnityEngine.Vector2 manualAimDirection = default)
        {
            CombatManager = combatManager;
            Roster = roster;
            CasterEntry = casterEntry;
            Runtime = runtime;
            DeltaTime = deltaTime;
            HasManualAimDirection = hasManualAimDirection;
            ManualAimDirection = manualAimDirection;
        }

        public InGameCombatManager CombatManager { get; }
        public UnitRosterService Roster { get; }
        public UnitRosterEntry CasterEntry { get; }
        public SkillRuntimeInstance Runtime { get; }
        public float DeltaTime { get; }
        public bool HasManualAimDirection { get; }
        public UnityEngine.Vector2 ManualAimDirection { get; }

        public BaseUnitRuntimeModel Caster => CasterEntry != null ? CasterEntry.Model : null;
        public SkillData SkillData => Runtime != null ? Runtime.Data : null;
    }
}
