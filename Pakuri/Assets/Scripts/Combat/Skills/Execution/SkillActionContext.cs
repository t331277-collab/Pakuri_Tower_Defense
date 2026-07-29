using UnityEngine;

namespace Pakuri.InGame
{
    /*
     * Trigger가 선택한 Node 실행에 필요한 사건 값을 불변 형태로 보관한다.
     * 대상 선택과 Node 실행은 각각 SkillTargeting과 SkillNodeExecutor가 담당한다.
     */
    public sealed class SkillActionContext
    {
        /*
         * 한 사건에서 고정된 시전자·대상·위치·피해·상태 값을 묶는다.
         * 지연 Trigger도 발생 당시 값을 유지하도록 생성 이후에는 변경하지 않는다.
         */
        public SkillActionContext(
            UnitCombatState source,
            string sourceSkillId,
            UnitCombatState eventTarget,
            Vector2 eventCenter,
            float eventDamage,
            int hitCount,
            SkillExecutionData executionData,
            SkillExecutionContext executionContext = null,
            string nodeOwnerId = "",
            StatusRuntimeInstance eventStatus = null,
            float shieldAbsorbedAmount = 0f)
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
            EventStatus = eventStatus;
            ShieldAbsorbedAmount = Mathf.Max(0f, shieldAbsorbedAmount);
        }

        public UnitCombatState Source { get; }

        public string SourceSkillId { get; }

        public UnitCombatState EventTarget { get; }

        public Vector2 EventCenter { get; }

        public float EventDamage { get; }

        public int HitCount { get; }

        public SkillExecutionData ExecutionData { get; }

        public string NodeOwnerId { get; }

        public StatusRuntimeInstance EventStatus { get; }

        public float ShieldAbsorbedAmount { get; }

        internal SkillExecutionContext ExecutionContext { get; }
    }
}
