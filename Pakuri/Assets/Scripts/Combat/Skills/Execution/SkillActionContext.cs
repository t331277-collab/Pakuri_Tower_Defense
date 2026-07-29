/*
 * 역할: 불변 스킬 사건 문맥.
 * 책임: 지연 반응에서 사용할 발생원·대상·위치·피해·적중 수·확정 실행 데이터를 보관한다.
 */

using UnityEngine;

namespace Pakuri.InGame
{

    /// <summary><c>SkillActionContext</c> 처리에 필요한 불변 실행 문맥을 전달한다.</summary>
    public sealed class SkillActionContext
    {

        /// <summary><c>SkillActionContext</c> 인스턴스를 전달된 런타임 입력값으로 초기화한다.</summary>
        public SkillActionContext(
            UnitCombatState source,
            string sourceSkillId,
            UnitCombatState eventTarget,
            Vector2 eventCenter,
            float eventDamage,
            int hitCount,
            SkillExecutionData executionData,
            SkillExecutionContext executionContext = null)
        {
            Source = source;
            SourceSkillId = sourceSkillId ?? string.Empty;
            EventTarget = eventTarget;
            EventCenter = eventCenter;
            EventDamage = eventDamage;
            HitCount = Mathf.Max(0, hitCount);
            ExecutionData = executionData;
            ExecutionContext = executionContext;
        }

        public UnitCombatState Source { get; }

        public string SourceSkillId { get; }

        public UnitCombatState EventTarget { get; }

        public Vector2 EventCenter { get; }

        public float EventDamage { get; }

        public int HitCount { get; }

        public SkillExecutionData ExecutionData { get; }

        internal SkillExecutionContext ExecutionContext { get; }
    }
}
