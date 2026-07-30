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

        private SkillNode(object operation)
        {
            this.operation = operation;
        }

        internal T? GetOperation<T>() where T : struct
        {
            return operation is T value ? value : null;
        }

        public static SkillNode FromOperation<T>(T op) where T : struct
        {
            return new SkillNode(op);
        }

        public static SkillNode FromOperation<T>(T op, string targetSkillId) where T : struct
        {
            return new SkillNode(op) { TargetSkillId = targetSkillId ?? string.Empty };
        }
    }
}
