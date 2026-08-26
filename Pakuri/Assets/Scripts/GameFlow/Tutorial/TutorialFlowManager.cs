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
            AwaitDayTwoCombat
        }

        private StageManager stageManager;
        private InGameCombatManager combatManager;
        private RewardPanelUI rewardPanel;
        private PrisonPanelUI prisonPanel;
        private OfferingUI offeringPanel;
        private MenifestUI manifestPanel;
        private TutorialLineView lineView;
        private Step step;
        private float resumeTimeScale = 1f;
        private float resumeFixedDeltaTime;
        private bool initialized;

        public void Initialize(
            StageManager stage,
            InGameCombatManager combat,
            InGameUIManager uiManager)
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
            lineView = GetComponent<TutorialLineView>() ?? gameObject.AddComponent<TutorialLineView>();
            if (stageManager == null
                || combatManager == null
                || rewardPanel == null
                || prisonPanel == null
                || offeringPanel == null
                || manifestPanel == null
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
            }
        }

        private void HandleStageStateChanged(StageState state)
        {
            if (stageManager.CurrentDay != 1)
            {
                return;
            }

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
            if (step != Step.AwaitDayOneRewards || !rewardPanel.AllActiveRewardsConsumed)
            {
                return;
            }

            step = Step.ReadyForDayTwo;
            rewardPanel.SetTutorialInteraction(-1, true, false, true);
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
            if (stageManager.CurrentDay != 1)
            {
                return true;
            }

            if (step != Step.ReadyForDayTwo)
            {
                return false;
            }

            offeringPanel.SetTutorialSkills(null, true);
            step = Step.AwaitDayTwoCombat;
            Resume();
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
        }
    }
}
