using System;

namespace Pakuri.NewCore.Run
{
    public enum RewardProcessingState
    {
        None,
        Pending,
        Processing,
        Completed
    }

    public enum RunResult
    {
        Active,
        Victory,
        Defeat
    }

    public sealed class RunSessionModel
    {
        public RunSessionModel(
            string currentStageId,
            int currentDay,
            string currentEncounterId,
            PartyRoster partyRoster,
            PrisonerInventory prisonerInventory)
        {
            if (string.IsNullOrWhiteSpace(currentStageId))
            {
                throw new ArgumentException("Current stage id is required.", nameof(currentStageId));
            }

            if (currentDay < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(currentDay));
            }

            if (string.IsNullOrWhiteSpace(currentEncounterId))
            {
                throw new ArgumentException(
                    "Current encounter id is required.",
                    nameof(currentEncounterId));
            }

            CurrentStageId = currentStageId;
            CurrentDay = currentDay;
            CurrentEncounterId = currentEncounterId;
            PartyRoster = partyRoster ?? throw new ArgumentNullException(nameof(partyRoster));
            PrisonerInventory =
                prisonerInventory ?? throw new ArgumentNullException(nameof(prisonerInventory));
            RewardState = RewardProcessingState.None;
            Result = RunResult.Active;
        }

        public string CurrentStageId { get; }

        public int CurrentDay { get; }

        public string CurrentEncounterId { get; }

        public PartyRoster PartyRoster { get; }

        public PrisonerInventory PrisonerInventory { get; }

        public RewardProcessingState RewardState { get; }

        public RunResult Result { get; }
    }
}
