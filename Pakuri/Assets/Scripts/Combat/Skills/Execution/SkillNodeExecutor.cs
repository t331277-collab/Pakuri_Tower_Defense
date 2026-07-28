using System.Collections.Generic;

namespace Pakuri.InGame
{
    /*
     * Trigger가 선택한 컴파일 Node를 작성 순서대로 실행한다.
     * Phase 1에서는 기존 Effect 경로를 유지하며 실행 데이터용 modifier Node만 전달한다.
     */
    public static class SkillNodeExecutor
    {
        public static void Execute(
            IReadOnlyList<SkillNode> nodes,
            SkillActionContext context)
        {
            if (nodes == null || nodes.Count == 0 || context == null)
            {
                return;
            }

            context.ExecutionData?.ApplyNodes(nodes);
        }
    }
}
