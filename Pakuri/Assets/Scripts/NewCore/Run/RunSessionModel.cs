using System;
using Pakuri.NewCore.Definitions.Stage;

/* 현재 run의 stage·day·보상 단계와 최종 결과 상태를 소유한다. */
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
        /* 초기 stage·day·encounter와 파티·포로 저장소를 활성 run 상태로 구성한다. */
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

        /* 활성 run을 지정 stage day로 전환하고 보상 상태를 초기화한다. */
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

        /* 전투 종료 후 보상 상태를 대기로 전환한다. */
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

        /* 대기 중인 보상 상태를 처리 중으로 전환한다. */
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

        /* 처리 중인 보상 상태를 완료로 전환한다. */
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

        /* 활성 run의 최종 결과를 승리로 기록한다. */
        internal void MarkVictory()
        {
            RequireActive();
            Result = RunResult.Victory;
        }

        /* 활성 run의 최종 결과를 패배로 기록한다. */
        internal void MarkDefeat()
        {
            RequireActive();
            Result = RunResult.Defeat;
        }

        /* run 결과가 아직 Active인지 확인하고 종료 상태면 예외를 발생시킨다. */
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
