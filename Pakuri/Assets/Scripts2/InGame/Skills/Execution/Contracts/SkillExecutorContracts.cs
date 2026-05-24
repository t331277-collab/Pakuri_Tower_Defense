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

        public virtual SkillExecutionResult Execute(SkillExecutionContext context, SkillExecutionSnapshot snapshot)
        {
            var skillId = snapshot != null ? snapshot.SkillId : string.Empty;
            return new SkillExecutionResult(SkillExecutionStatus.Routed, skillId, GetType().Name);
        }
    }
}
