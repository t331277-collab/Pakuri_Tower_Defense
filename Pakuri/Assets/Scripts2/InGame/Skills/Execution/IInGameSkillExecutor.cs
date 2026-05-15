namespace Pakuri.InGame
{
    public interface IInGameSkillExecutor
    {
        bool CanExecute(SkillData skillData);
        SkillExecutionResult Execute(SkillExecutionContext context, SkillExecutionSnapshot snapshot);
    }
}
