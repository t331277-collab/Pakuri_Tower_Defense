using System;
using System.Collections.Generic;
using Pakuri.NewCore.Definitions.Skills;
using Pakuri.NewCore.Units.Models;

/* 한 번의 스킬 실행에 필요한 시전자, 대상, 트리거 문맥과 실행 결과를 운반한다. */
namespace Pakuri.NewCore.Combat.Skills.Execution
{
    public sealed class SkillExecutionRequest
    {
        /* 스킬 실행에 필요한 불변 입력값을 저장한다. */
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

        /* 현재 전투 이벤트가 가리키는 대상을 지정한다. */
        public void SetEventTarget(UnitBaseModel target)
        {
            EventTarget = target;
        }

        /* 효과가 적용된 대상을 중복 없이 실행 결과에 기록한다. */
        internal void RecordAppliedTarget(UnitBaseModel target)
        {
            if (target != null && !appliedTargets.Contains(target))
            {
                appliedTargets.Add(target);
            }
        }

        /* 적중 대상을 기록하고 적중 완료 콜백을 호출한다. */
        internal void NotifyHitCompleted(UnitBaseModel target)
        {
            RecordAppliedTarget(target);
            EventTarget = target;
            HitCompleted?.Invoke(target);
        }

        /* 처치 수를 증가시키고 대상 처치 콜백을 호출한다. */
        internal void NotifyTargetDefeated(UnitBaseModel target)
        {
            DefeatedTargetCount++;
            TargetDefeated?.Invoke(target);
        }

        /* 상위 트리거 경로와 발동 원본 스킬을 현재 요청에 상속한다. */
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
