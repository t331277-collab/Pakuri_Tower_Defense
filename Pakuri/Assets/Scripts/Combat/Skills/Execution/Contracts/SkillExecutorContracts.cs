using Pakuri.Data;

/*
 * I 인게임 스킬 구현에 필요한 계약을 정의한다.
 */
namespace Pakuri.InGame
{
    public interface IInGameSkillExecutor
    {
        /*
         * 처형을 가능한 상태인지 확인한다.
         */
        bool CanExecute(SkillRuntimeData skillData);
        /*
         * 요청받은 I 인게임 스킬을 실행한다.
         */
        SkillExecutionResult Execute(SkillExecutionContext context, SkillExecutionSnapshot snapshot);
    }

    /*
     * 자료형별 스킬을 실행한다.
     */
    public abstract class TypedSkillExecutor<TSkillData> : IInGameSkillExecutor
        where TSkillData : SkillRuntimeData
    {
        /*
         * 처형을 가능한 상태인지 확인한다.
         */
        public bool CanExecute(SkillRuntimeData skillData)
        {
            return skillData is TSkillData;
        }

        /*
         * 요청받은 자료형별 스킬을 실행한다.
         */
        public abstract SkillExecutionResult Execute(SkillExecutionContext context, SkillExecutionSnapshot snapshot);
    }
}
