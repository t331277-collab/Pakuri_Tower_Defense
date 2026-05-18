using System;
using System.Collections.Generic;
using Pakuri.Data;
using Pakuri.Run;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Pakuri.InGame
{
    internal sealed class MenifestUI
    {
        private readonly GameObject manifestedFailPopUp;
        private readonly Button manifestedFailBackButton;
        private readonly GameObject manifestedSuccessPopUp;
        private readonly Button dontChoiceButton;
        private readonly Button choiceButton;
        private readonly TMP_Text monsterNameText;
        private readonly TMP_Text monsterDescText;
        private readonly Image monsterImage;
        private readonly GameObject prisonerChoicePopUp;
        private readonly Func<RunSession> resolveSession;
        private readonly Func<GameDataCatalog> resolveCatalog;
        private readonly Func<StageManager> resolveStageManager;
        private readonly Func<SceneEntryManager> resolveEntryManager;
        private readonly Func<InGameUIManager.RewardButtonView> resolveActivePrisonerButton;
        private readonly Action consumePrisonerButton;
        private readonly Action refreshInfo;

        private MonsterDefinition pendingManifestMonster;

        public MenifestUI(
            GameObject manifestedFailPopUp,
            Button manifestedFailBackButton,
            GameObject manifestedSuccessPopUp,
            Button dontChoiceButton,
            Button choiceButton,
            TMP_Text monsterNameText,
            TMP_Text monsterDescText,
            Image monsterImage,
            GameObject prisonerChoicePopUp,
            Func<RunSession> resolveSession,
            Func<GameDataCatalog> resolveCatalog,
            Func<StageManager> resolveStageManager,
            Func<SceneEntryManager> resolveEntryManager,
            Func<InGameUIManager.RewardButtonView> resolveActivePrisonerButton,
            Action consumePrisonerButton,
            Action refreshInfo)
        {
            this.manifestedFailPopUp = manifestedFailPopUp;
            this.manifestedFailBackButton = manifestedFailBackButton;
            this.manifestedSuccessPopUp = manifestedSuccessPopUp;
            this.dontChoiceButton = dontChoiceButton;
            this.choiceButton = choiceButton;
            this.monsterNameText = monsterNameText;
            this.monsterDescText = monsterDescText;
            this.monsterImage = monsterImage;
            this.prisonerChoicePopUp = prisonerChoicePopUp;
            this.resolveSession = resolveSession;
            this.resolveCatalog = resolveCatalog;
            this.resolveStageManager = resolveStageManager;
            this.resolveEntryManager = resolveEntryManager;
            this.resolveActivePrisonerButton = resolveActivePrisonerButton;
            this.consumePrisonerButton = consumePrisonerButton;
            this.refreshInfo = refreshInfo;

            BindButton(this.manifestedFailBackButton, () => SetActive(this.manifestedFailPopUp, false));
            BindButton(this.dontChoiceButton, SkipManifestChoice);
            BindButton(this.choiceButton, CommitManifestChoice);
        }

        public void TryManifestPrisoner()
        {
            var session = resolveSession?.Invoke();
            var activePrisonerButton = resolveActivePrisonerButton?.Invoke();
            if (session == null || activePrisonerButton == null || activePrisonerButton.Consumed)
            {
                return;
            }

            session.ClaimPrisonerReward(activePrisonerButton.PrisonerId);
            consumePrisonerButton?.Invoke();
            SetActive(prisonerChoicePopUp, false);

            pendingManifestMonster = ResolveNextManifestCandidate(session);
            var stageManager = resolveStageManager?.Invoke();
            var successChance = stageManager != null ? stageManager.PendingManifestSuccessChance : 0.7f;
            var succeeded = pendingManifestMonster != null && UnityEngine.Random.value < successChance;
            if (!succeeded)
            {
                SetActive(manifestedFailPopUp, true);
                return;
            }

            ShowManifestSuccessPopup(pendingManifestMonster);
        }

        public void Hide()
        {
            SetActive(manifestedFailPopUp, false);
            SetActive(manifestedSuccessPopUp, false);
        }

        private void ShowManifestSuccessPopup(MonsterDefinition monster)
        {
            SetActive(manifestedSuccessPopUp, true);

            if (monsterNameText != null)
            {
                monsterNameText.text = monster != null ? monster.DisplayName : "Unknown";
            }

            if (monsterDescText != null)
            {
                monsterDescText.text = BuildManifestDescription(monster);
            }

            if (monsterImage != null)
            {
                monsterImage.sprite = monster != null ? monster.UnitSprite : null;
                monsterImage.color = monster != null && monster.UnitSprite != null ? Color.white : new Color(0f, 0f, 0f, 0.3f);
            }
        }

        private void SkipManifestChoice()
        {
            pendingManifestMonster = null;
            SetActive(manifestedSuccessPopUp, false);
            SetActive(prisonerChoicePopUp, false);
        }

        private void CommitManifestChoice()
        {
            var session = resolveSession?.Invoke();
            if (session == null || pendingManifestMonster == null)
            {
                return;
            }

            session.RecordManifestedMonster(pendingManifestMonster);
            var slotIndex = Mathf.Clamp(session.ManifestedMonsterIds.Count, 1, 4);
            var entryManager = resolveEntryManager?.Invoke();
            if (entryManager != null)
            {
                entryManager.SpawnManifestedMonster(pendingManifestMonster, slotIndex, out _);
            }

            pendingManifestMonster = null;
            SetActive(manifestedSuccessPopUp, false);
            SetActive(prisonerChoicePopUp, false);
            refreshInfo?.Invoke();
        }

        private MonsterDefinition ResolveNextManifestCandidate(RunSession session)
        {
            var monsters = PakuriDataManager.Instance.GetMonsters(resolveCatalog?.Invoke());
            var candidates = new List<MonsterDefinition>();
            for (var i = 0; i < monsters.Length; i++)
            {
                var monster = monsters[i];
                if (monster == null
                    || string.IsNullOrWhiteSpace(monster.MonsterId)
                    || string.Equals(monster.MonsterId, session.SelectedMonsterId, StringComparison.OrdinalIgnoreCase)
                    || session.HasManifestedMonster(monster.MonsterId))
                {
                    continue;
                }

                candidates.Add(monster);
            }

            return candidates.Count > 0 ? candidates[UnityEngine.Random.Range(0, candidates.Count)] : null;
        }

        private static string BuildManifestDescription(MonsterDefinition monster)
        {
            if (monster == null)
            {
                return string.Empty;
            }

            return
                $"{monster.RoleSummary}\n" +
                $"?띿꽦: {monster.ElementLabel}\n" +
                $"HP: {monster.MaxHealth:0} / 怨듦꺽: {monster.PowerStat:0}\n" +
                $"A: {monster.ActiveSkillName} / F: {monster.PassiveSkillName}";
        }

        private static void BindButton(Button button, UnityEngine.Events.UnityAction action)
        {
            if (button == null)
            {
                return;
            }

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(action);
        }

        private static void SetActive(GameObject target, bool active)
        {
            if (target != null)
            {
                target.SetActive(active);
            }
        }
    }
}
