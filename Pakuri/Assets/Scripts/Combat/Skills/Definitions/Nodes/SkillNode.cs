/*
 * 역할: 스킬 노드 공용 래퍼.
 * 책임: 구체 연산값과 대상 스킬 ID를 함께 보관한다.
 */

namespace Pakuri.InGame
{
    public class SkillNode
    {
        private readonly object operation;
        public string TargetSkillId { get; internal set; }

        /// 하나의 의미 단위를 노드로 감싼다.
        private SkillNode(object operation)
        {
            this.operation = operation;
        }

        /// 저장된 의미를 요청한 형태로 꺼낸다.
        internal T? GetOperation<T>() where T : struct
        {
            return operation is T value ? value : null;
        }

        /// 의미 단위만 담은 노드를 만든다.
        public static SkillNode FromOperation<T>(T op) where T : struct
        {
            return new SkillNode(op);
        }

        /// 대상 스킬 범위를 포함한 노드를 만든다.
        public static SkillNode FromOperation<T>(T op, string targetSkillId) where T : struct
        {
            return new SkillNode(op) { TargetSkillId = targetSkillId ?? string.Empty };
        }
    }
}
