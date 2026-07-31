/*
 * 역할: 하나의 스킬 규칙을 공통 형태로 표현한다.
 * 책임: 규칙의 실제 값과 적용할 스킬 범위를 함께 전달한다.
 */

namespace Pakuri.InGame
{
    /// 서로 다른 스킬 규칙을 같은 노드 배열에서 다루게 한다.
    public class SkillNode
    {
        private readonly object operation;
        public string TargetSkillId { get; internal set; }

        /// 하나의 규칙을 대상 범위와 함께 다룰 수 있게 감싼다.
        private SkillNode(object operation)
        {
            this.operation = operation;
        }

        /// 요청한 규칙 종류가 맞을 때만 해석 가능한 값을 돌려준다.
        internal T? GetOperation<T>() where T : struct
        {
            return operation is T value ? value : null;
        }

        /// 공통으로 적용할 규칙 노드를 만든다.
        public static SkillNode FromOperation<T>(T op) where T : struct
        {
            return new SkillNode(op);
        }

        /// 특정 스킬에만 적용할 규칙 노드를 만든다.
        public static SkillNode FromOperation<T>(T op, string targetSkillId) where T : struct
        {
            return new SkillNode(op) { TargetSkillId = targetSkillId ?? string.Empty };
        }
    }
}
