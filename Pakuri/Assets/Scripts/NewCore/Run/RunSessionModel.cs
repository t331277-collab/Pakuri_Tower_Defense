using System;
using Pakuri.NewCore.Definitions.Stage;

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

        public string CurrentStageId { get; private set; }

        public int CurrentDay { get; private set; }

        public string CurrentEncounterId { get; private set; }

        public PartyRoster PartyRoster { get; }

        public PrisonerInventory PrisonerInventory { get; }

        public RewardProcessingState RewardState { get; private set; }

        public RunResult Result { get; private set; }

        internal void BeginDay(StageDayDefinition day)
        {
            RequireActive();
            if (day == null
                || !day.stage.HasValue
                || !day.day.HasValue)
            {
                throw new ArgumentException(
                    "A stage and day definition is required.",
                    nameof(day));
            }

            CurrentStageId = "stage" + day.stage.Value;
            CurrentDay = day.day.Value;
            CurrentEncounterId = day.encounter_id;
            RewardState = RewardProcessingState.None;
        }

        internal void BeginReward()
        {
            RequireActive();
            if (RewardState != RewardProcessingState.None)
            {
                throw new InvalidOperationException(
                    "Reward processing has already started.");
            }

            RewardState = RewardProcessingState.Pending;
        }

        internal void BeginRewardProcessing()
        {
            RequireActive();
            if (RewardState != RewardProcessingState.Pending)
            {
                throw new InvalidOperationException(
                    "A pending reward is required.");
            }

            RewardState = RewardProcessingState.Processing;
        }

        internal void CompleteReward()
        {
            RequireActive();
            if (RewardState != RewardProcessingState.Processing)
            {
                throw new InvalidOperationException(
                    "A processing reward is required.");
            }

            RewardState = RewardProcessingState.Completed;
        }

        internal void MarkVictory()
        {
            RequireActive();
            Result = RunResult.Victory;
        }

        internal void MarkDefeat()
        {
            RequireActive();
            Result = RunResult.Defeat;
        }

        private void RequireActive()
        {
            if (Result != RunResult.Active)
            {
                throw new InvalidOperationException(
                    "The run is no longer active.");
            }
        }
    }
}
