using System;
using System.Collections;
using Pakuri.Data;
using UnityEngine;

namespace Pakuri.InGame
{
    /// 튜토리얼 Phase와 실제 행동 완료 조건을 한 곳에서 판정한다.
    public sealed class TutorialFlowManager : MonoBehaviour
    {
        private enum Step
        {
            None,
            Intro,
            AwaitDayOneCombat,
            AwaitBasicHit,
            AwaitDayOneClear,
            AwaitFirstOffering,
            AwaitOfferingPanel,
            AwaitOfferingCommit,
            AwaitManifestCommit,
            AwaitDayOneRewards,
            ReadyForDayTwo,
            AwaitDayTwoCombat,
            AwaitManualInput,
            ManualInputDelay,
            AwaitAutoSpeed,
            AwaitDayTwoClear,
            AwaitArtifactIntro,
            AwaitArtifactAcquired,
            AwaitDayTwoRewards,
            ReadyForDayThree,
            FreePlay
        }

        private StageManager stageManager;
        private InGameCombatManager combatManager;
        private RewardPanelUI rewardPanel;
        private PrisonPanelUI prisonPanel;
        private OfferingUI offeringPanel;
        private MenifestUI manifestPanel;
        private PlayerCombatInputController playerInput;
        private InGameUtilityPanelController utilityPanel;
        private TutorialLineView lineView;
        private Step step;
        private float resumeTimeScale = 1f;
        private float resumeFixedDeltaTime;
        private bool initialized;

        public void Initialize(
            StageManager stage,
            InGameCombatManager combat,
            InGameUIManager uiManager,
            PlayerCombatInputController input,
            InGameUtilityPanelController utility)
        {
            if (initialized)
            {
                return;
            }

            stageManager = stage;
            combatManager = combat;
            rewardPanel = uiManager != null ? uiManager.RewardPanel : null;
            prisonPanel = uiManager != null ? uiManager.PrisonPanel : null;
            offeringPanel = uiManager != null ? uiManager.OfferingPanel : null;
            manifestPanel = uiManager != null ? uiManager.ManifestPanel : null;
            playerInput = input;
            utilityPanel = utility;
            lineView = GetComponent<TutorialLineView>() ?? gameObject.AddComponent<TutorialLineView>();
            if (stageManager == null
                || combatManager == null
                || rewardPanel == null
                || prisonPanel == null
                || offeringPanel == null
                || manifestPanel == null
                || playerInput == null
                || utilityPanel == null
                || !lineView.Initialize(transform))
            {
                Debug.LogError("TutorialFlowManager initialization failed because required runtime references are missing.", this);
                enabled = false;
                return;
            }

            initialized = true;
            resumeFixedDeltaTime = Time.fixedDeltaTime;
            stageManager.StateChanged += HandleStageStateChanged;
            stageManager.ContinueRequested += HandleContinueRequested;
            combatManager.DamageApplied += HandleDamageApplied;
            lineView.NextRequested += HandleLineNext;
            rewardPanel.RewardConsumed += HandleRewardConsumed;
            offeringPanel.Opened += HandleOfferingOpened;
            offeringPanel.ChoiceCommitted += HandleOfferingCommitted;
            manifestPanel.ManifestCommitted += HandleManifestCommitted;
            prisonPanel.ArtifactAcquired += HandleArtifactAcquired;
            playerInput.ManualInputDetected += HandleManualInputDetected;
            playerInput.AutoSkillChanged += HandleAutoSkillChanged;
            utilityPanel.TimeScaleChanged += HandleTimeScaleChanged;
            utilityPanel.SetTutorialInputEnabled(false);
            Pause();
            step = Step.Intro;
            ShowLine("line1-1");
        }

        private void HandleLineNext(string lineId)
        {
            switch (lineId)
            {
                case "line1-1":
                    ShowLine("line1-2");
                    break;
                case "line1-2":
                    step = Step.AwaitDayOneCombat;
                    Resume();
                    stageManager.StartCurrentDay();
                    break;
                case "line1-3":
                    step = Step.AwaitBasicHit;
                    Resume();
                    break;
                case "line1-4":
                    step = Step.AwaitDayOneClear;
                    Resume();
                    break;
                case "line1-5":
                    step = Step.AwaitFirstOffering;
                    ShowLine("line2-1");
                    break;
                case "line2-1":
                    step = Step.AwaitOfferingPanel;
                    offeringPanel.SetTutorialSkills(new[] { "eve-b", "eve-c", "eve-d" }, false);
                    prisonPanel.SetActionMode(PrisonActionMode.OfferingOnly);
                    rewardPanel.SetTutorialInteraction(0, false, false, false);
                    break;
                case "line2-2":
                    step = Step.AwaitOfferingCommit;
                    offeringPanel.SetChoiceInputEnabled(true);
                    break;
                case "line2-3":
                    step = Step.AwaitManifestCommit;
                    prisonPanel.SetActionMode(PrisonActionMode.ManifestOnly);
                    manifestPanel.SetManifestChoiceRequired(true);
                    rewardPanel.SetTutorialInteraction(1, false, false, false);
                    break;
                case "line2-4":
                    step = Step.AwaitDayOneRewards;
                    prisonPanel.SetActionMode(PrisonActionMode.Any);
                    rewardPanel.SetTutorialInteraction(-1, true, false, false);
                    HandleRewardConsumed();
                    break;
                case "line3-1":
                    ShowLine("line3-2");
                    break;
                case "line3-2":
                    step = Step.AwaitAutoSpeed;
                    utilityPanel.SetTutorialInputEnabled(true);
                    Resume();
                    CheckAutoAndSpeed();
                    break;
                case "line3-3":
                    step = Step.AwaitDayTwoClear;
                    utilityPanel.SetTutorialInputEnabled(true);
                    Resume();
                    break;
                case "line4-1":
                    ShowLine("line4-2");
                    break;
                case "line4-2":
                    step = Step.AwaitArtifactAcquired;
                    prisonPanel.SetActionMode(PrisonActionMode.ArtifactRecipient);
                    rewardPanel.SetTutorialInteraction(-1, false, true, false);
                    break;
                case "line4-3":
                    step = Step.AwaitDayTwoRewards;
                    prisonPanel.SetActionMode(PrisonActionMode.Any);
                    rewardPanel.SetTutorialInteraction(-2, true, false, false);
                    HandleRewardConsumed();
                    break;
            }
        }

        private void HandleStageStateChanged(StageState state)
        {
            if (stageManager.CurrentDay == 1)
            {
                if (state == StageState.Combat && step == Step.AwaitDayOneCombat)
                {
                    Pause();
                    ShowLine("line1-3");
                }
                else if (state == StageState.RewardReady && step == Step.AwaitDayOneClear)
                {
                    Pause();
                    StartCoroutine(ShowWhenRewardVisible("line1-5"));
                }
            }
            else if (stageManager.CurrentDay == 2)
            {
                if (state == StageState.Combat && step == Step.AwaitDayTwoCombat)
                {
                    step = Step.AwaitManualInput;
                }
                else if (state == StageState.RewardReady && step == Step.AwaitDayTwoClear)
                {
                    step = Step.AwaitArtifactIntro;
                    Pause();
                    StartCoroutine(ShowWhenRewardVisible("line4-1"));
                }
            }
        }

        private void HandleDamageApplied(AttackRule attack, InGameResourceChangeResult result)
        {
            var source = attack.Source;
            var target = result.Target;
            if (step != Step.AwaitBasicHit
                || result.AppliedDamage <= 0f
                || source?.Identity == null
                || target?.Identity == null
                || target.Identity.Side != UnitSide.Enemy
                || !string.Equals(source.Identity.DefinitionName, "eve", StringComparison.OrdinalIgnoreCase)
                || !string.Equals(attack.SourceSkillName, "eve-a", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            step = Step.None;
            Pause();
            ShowLine("line1-4");
        }

        private void HandleOfferingOpened()
        {
            if (step != Step.AwaitOfferingPanel)
            {
                return;
            }

            Pause();
            ShowLine("line2-2");
        }

        private void HandleOfferingCommitted(string skillName)
        {
            if (step != Step.AwaitOfferingCommit
                || (skillName != "eve-b" && skillName != "eve-c" && skillName != "eve-d"))
            {
                return;
            }

            step = Step.None;
            Pause();
            rewardPanel.SetTutorialInteraction(-1, false, false, false);
            ShowLine("line2-3");
        }

        private void HandleManifestCommitted(string monsterName)
        {
            if (step != Step.AwaitManifestCommit || string.IsNullOrWhiteSpace(monsterName))
            {
                return;
            }

            step = Step.None;
            manifestPanel.SetManifestChoiceRequired(false);
            Pause();
            rewardPanel.SetTutorialInteraction(-1, false, false, false);
            ShowLine("line2-4");
        }

        private void HandleRewardConsumed()
        {
            if (!rewardPanel.AllActiveRewardsConsumed)
            {
                return;
            }

            if (step == Step.AwaitDayOneRewards)
            {
                step = Step.ReadyForDayTwo;
                rewardPanel.SetTutorialInteraction(-1, true, false, true);
            }
            else if (step == Step.AwaitDayTwoRewards)
            {
                step = Step.ReadyForDayThree;
                rewardPanel.SetTutorialInteraction(-2, true, false, true);
            }
        }

        private void HandleManualInputDetected()
        {
            if (step != Step.AwaitManualInput)
            {
                return;
            }

            step = Step.ManualInputDelay;
            StartCoroutine(PauseAfterManualInputDelay());
        }

        private IEnumerator PauseAfterManualInputDelay()
        {
            yield return new WaitForSecondsRealtime(2f);
            if (step != Step.ManualInputDelay
                || stageManager.CurrentDay != 2
                || stageManager.State != StageState.Combat)
            {
                yield break;
            }

            step = Step.None;
            Pause();
            ShowLine("line3-1");
        }

        private void HandleAutoSkillChanged(bool enabled)
        {
            CheckAutoAndSpeed();
        }

        private void HandleTimeScaleChanged(float timeScale)
        {
            CheckAutoAndSpeed();
        }

        private void CheckAutoAndSpeed()
        {
            var speed = utilityPanel.CurrentTimeScale;
            if (step != Step.AwaitAutoSpeed
                || !playerInput.AutoSkillEnabled
                || (!Mathf.Approximately(speed, 1.5f) && !Mathf.Approximately(speed, 2f)))
            {
                return;
            }

            step = Step.None;
            utilityPanel.SetTutorialInputEnabled(false);
            Pause();
            ShowLine("line3-3");
        }

        private void HandleArtifactAcquired(string artifactName)
        {
            if (step != Step.AwaitArtifactAcquired || string.IsNullOrWhiteSpace(artifactName))
            {
                return;
            }

            step = Step.None;
            Pause();
            rewardPanel.SetTutorialInteraction(-1, false, false, false);
            ShowLine("line4-3");
        }

        private IEnumerator ShowWhenRewardVisible(string lineId)
        {
            while (rewardPanel != null && !rewardPanel.IsVisible)
            {
                yield return null;
            }

            rewardPanel.SetTutorialInteraction(-1, false, false, false);
            ShowLine(lineId);
        }

        private bool HandleContinueRequested()
        {
            if (stageManager.CurrentDay == 1)
            {
                if (step != Step.ReadyForDayTwo)
                {
                    return false;
                }

                offeringPanel.SetTutorialSkills(null, true);
                step = Step.AwaitDayTwoCombat;
                Resume();
                return true;
            }

            if (stageManager.CurrentDay == 2)
            {
                if (step != Step.ReadyForDayThree)
                {
                    return false;
                }

                step = Step.FreePlay;
                Resume();
                return true;
            }

            return true;
        }

        private void ShowLine(string lineId)
        {
            var line = GameDataLoader.CurrentCatalog.GetTutorialLine(lineId);
            if (line == null)
            {
                Debug.LogError($"TutorialFlowManager cannot find dialogue '{lineId}'.", this);
                return;
            }

            lineView.Show(line);
        }

        private void Pause()
        {
            if (Time.timeScale > 0f)
            {
                resumeTimeScale = Time.timeScale;
                resumeFixedDeltaTime = Time.fixedDeltaTime;
            }

            Time.timeScale = 0f;
        }

        private void Resume()
        {
            Time.timeScale = Mathf.Max(1f, resumeTimeScale);
            Time.fixedDeltaTime = resumeFixedDeltaTime;
        }

        private void OnDestroy()
        {
            if (!initialized)
            {
                return;
            }

            stageManager.StateChanged -= HandleStageStateChanged;
            stageManager.ContinueRequested -= HandleContinueRequested;
            combatManager.DamageApplied -= HandleDamageApplied;
            lineView.NextRequested -= HandleLineNext;
            rewardPanel.RewardConsumed -= HandleRewardConsumed;
            offeringPanel.Opened -= HandleOfferingOpened;
            offeringPanel.ChoiceCommitted -= HandleOfferingCommitted;
            manifestPanel.ManifestCommitted -= HandleManifestCommitted;
            prisonPanel.ArtifactAcquired -= HandleArtifactAcquired;
            playerInput.ManualInputDetected -= HandleManualInputDetected;
            playerInput.AutoSkillChanged -= HandleAutoSkillChanged;
            utilityPanel.TimeScaleChanged -= HandleTimeScaleChanged;
        }
    }
}
