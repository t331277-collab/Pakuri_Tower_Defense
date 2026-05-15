namespace Pakuri.InGame
{
    public enum SkillExecutionStatus
    {
        None,
        Rejected,
        Routed
    }

    public sealed class SkillExecutionResult
    {
        public static readonly SkillExecutionResult None = new SkillExecutionResult(
            SkillExecutionStatus.None,
            string.Empty,
            string.Empty);

        public SkillExecutionResult(SkillExecutionStatus status, string skillId, string executorName)
        {
            Status = status;
            SkillId = skillId ?? string.Empty;
            ExecutorName = executorName ?? string.Empty;
        }

        public SkillExecutionStatus Status { get; }
        public string SkillId { get; }
        public string ExecutorName { get; }
        public bool Routed => Status == SkillExecutionStatus.Routed;
    }
}
