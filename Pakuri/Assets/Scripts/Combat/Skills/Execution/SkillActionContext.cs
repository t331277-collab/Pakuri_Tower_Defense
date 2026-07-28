using UnityEngine;

namespace Pakuri.InGame
{
    /*
     * Trigger가 선택한 Node 실행에 필요한 사건 값을 불변 형태로 보관한다.
     * 대상 선택과 Node 실행은 각각 SkillTargeting과 SkillNodeExecutor가 담당한다.
     */
    public sealed class SkillActionContext
    {
        public SkillActionContext(
            UnitCombatState source,
            string sourceSkillId,
            UnitCombatState eventTarget,
            Vector2 eventCenter,
            float eventDamage,
            int hitCount,
            SkillExecutionData executionData,
            SkillExecutionContext executionContext = null,
            string nodeOwnerId = "")
        {
            Source = source;
            SourceSkillId = sourceSkillId ?? string.Empty;
            EventTarget = eventTarget;
            EventCenter = eventCenter;
            EventDamage = eventDamage;
            HitCount = Mathf.Max(0, hitCount);
            ExecutionData = executionData;
            ExecutionContext = executionContext;
            NodeOwnerId = nodeOwnerId ?? string.Empty;
        }

        public UnitCombatState Source { get; }

        public string SourceSkillId { get; }

        public UnitCombatState EventTarget { get; }

        public Vector2 EventCenter { get; }

        public float EventDamage { get; }

        public int HitCount { get; }

        public SkillExecutionData ExecutionData { get; }

        public string NodeOwnerId { get; }

        internal SkillExecutionContext ExecutionContext { get; }
    }
}
