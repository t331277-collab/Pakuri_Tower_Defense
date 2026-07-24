using System;
using System.Collections.Generic;
using Pakuri.NewCore.Definitions.Skills;
using Pakuri.NewCore.Units.Models;

namespace Pakuri.NewCore.Combat.Skills.Execution
{
    public sealed class SkillExecutionRequest
    {
        public SkillExecutionRequest(
            UnitBaseModel caster,
            SkillDefinition skill,
            IReadOnlyList<UnitBaseModel> registeredUnits,
            CombatVector2? aimDirection = null,
            CombatVector2? targetPoint = null,
            bool isTriggered = false,
            string hitZone = null)
        {
            Caster = caster ?? throw new ArgumentNullException(nameof(caster));
            Skill = skill ?? throw new ArgumentNullException(nameof(skill));
            RegisteredUnits =
                registeredUnits ?? throw new ArgumentNullException(nameof(registeredUnits));
            AimDirection = aimDirection;
            TargetPoint = targetPoint;
            IsTriggered = isTriggered;
            HitZone = hitZone;
        }

        public UnitBaseModel Caster { get; }

        public SkillDefinition Skill { get; }

        public IReadOnlyList<UnitBaseModel> RegisteredUnits { get; }

        public CombatVector2? AimDirection { get; }

        public CombatVector2? TargetPoint { get; }

        public bool IsTriggered { get; }

        public string HitZone { get; }

        public UnitBaseModel EventTarget { get; private set; }

        public IReadOnlyList<UnitBaseModel> AppliedTargets => appliedTargets;

        private readonly List<UnitBaseModel> appliedTargets =
            new List<UnitBaseModel>();
        private readonly HashSet<string> triggerAncestry =
            new HashSet<string>(StringComparer.Ordinal);

        internal Action<UnitBaseModel> HitCompleted { get; set; }

        internal Action<UnitBaseModel> TargetDefeated { get; set; }

        internal int DefeatedTargetCount { get; private set; }

        internal IReadOnlyCollection<string> TriggerAncestry =>
            triggerAncestry;

        internal string TriggerSourceSkillId { get; private set; }

        public void SetEventTarget(UnitBaseModel target)
        {
            EventTarget = target;
        }

        internal void RecordAppliedTarget(UnitBaseModel target)
        {
            if (target != null && !appliedTargets.Contains(target))
            {
                appliedTargets.Add(target);
            }
        }

        internal void NotifyHitCompleted(UnitBaseModel target)
        {
            RecordAppliedTarget(target);
            EventTarget = target;
            HitCompleted?.Invoke(target);
        }

        internal void NotifyTargetDefeated(UnitBaseModel target)
        {
            DefeatedTargetCount++;
            TargetDefeated?.Invoke(target);
        }

        internal void InheritTriggerAncestry(
            IReadOnlyCollection<string> ancestors,
            string triggerId,
            string sourceSkillId = null)
        {
            if (ancestors != null)
            {
                foreach (string ancestor in ancestors)
                {
                    if (!string.IsNullOrEmpty(ancestor))
                    {
                        triggerAncestry.Add(ancestor);
                    }
                }
            }
            if (!string.IsNullOrEmpty(triggerId))
            {
                triggerAncestry.Add(triggerId);
            }
            if (!string.IsNullOrEmpty(sourceSkillId))
            {
                TriggerSourceSkillId = sourceSkillId;
            }
        }
    }
}
