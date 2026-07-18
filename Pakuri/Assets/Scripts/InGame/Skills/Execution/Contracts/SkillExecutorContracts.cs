using Pakuri.Data;

namespace Pakuri.InGame
{
    public interface IInGameSkillExecutor
    {
        bool CanExecute(SkillData skillData);
        SkillExecutionResult Execute(SkillExecutionContext context, SkillExecutionSnapshot snapshot);
    }

    public abstract class TypedSkillExecutor<TSkillData> : IInGameSkillExecutor
        where TSkillData : SkillData
    {
        public bool CanExecute(SkillData skillData)
        {
            return skillData is TSkillData;
        }

        public abstract SkillExecutionResult Execute(SkillExecutionContext context, SkillExecutionSnapshot snapshot);
    }
}
