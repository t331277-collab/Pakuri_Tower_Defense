using System;
using Pakuri.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Pakuri.InGame
{
    /// 포로 몬스터의 영입 성공·실패 화면과 파티 추가 선택을 관리한다.
    public sealed class MenifestUI : MonoBehaviour
    {
        private GameObject manifestedFailPopUp;
        private Button manifestedFailBackButton;
        private GameObject manifestedSuccessPopUp;
        private Button dontChoiceButton;
        private Button choiceButton;
        private TMP_Text monsterNameText;
        private TMP_Text monsterDescText;
        private Image monsterImage;
        private StageManager stageManager;
        private UnitSpawnManager unitSpawnManager;
        private InGameUIManager uiManager;

        private MonsterDefinition pendingManifestMonster;
        private bool referencesBound;
        private bool bindingFailed;

        public bool IsFailurePopupVisible => manifestedFailPopUp != null && manifestedFailPopUp.activeSelf;

        private void Awake()
        {
            if (!BindObject())
            {
                enabled = false;
                return;
            }

            BindButton(manifestedFailBackButton, CompleteAfterFailure);
            BindButton(dontChoiceButton, SkipManifestChoice);
            BindButton(choiceButton, CommitManifestChoice);
        }

        public bool TryManifestPrisoner()
        {
            if (!BindObject())
            {
                return false;
            }

            var session = uiManager?.ResolveSession();
            var activePrisonerButton = uiManager?.ActivePrisonerButton;
            if (session == null || activePrisonerButton == null || activePrisonerButton.Consumed)
            {
                return false;
            }

            uiManager.ConsumeActivePrisonerButton();
            pendingManifestMonster = ResolveNextManifestCandidate(session);
            var successChance = stageManager != null ? stageManager.PendingManifestSuccessChance : 0.7f;
            var succeeded = pendingManifestMonster != null && UnityEngine.Random.value < successChance;
            if (!succeeded)
            {
                UiObjectUtility.SetActive(manifestedFailPopUp, true);
                return true;
            }

            ShowManifestSuccessPopup(pendingManifestMonster);
            return true;
        }

        public void Hide()
        {
            UiObjectUtility.SetActive(manifestedFailPopUp, false);
            UiObjectUtility.SetActive(manifestedSuccessPopUp, false);
        }

        private void ShowManifestSuccessPopup(MonsterDefinition monster)
        {
            UiObjectUtility.SetActive(manifestedSuccessPopUp, true);
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
                monsterImage.sprite = monster != null ? monster.Image : null;
                monsterImage.color = monsterImage.sprite != null
                    ? Color.white
                    : new Color(0f, 0f, 0f, 0.3f);
            }
        }

        private void SkipManifestChoice()
        {
            pendingManifestMonster = null;
            UiObjectUtility.SetActive(manifestedSuccessPopUp, false);
            uiManager?.CompletePrisonAction();
        }

        private void CompleteAfterFailure()
        {
            pendingManifestMonster = null;
            UiObjectUtility.SetActive(manifestedFailPopUp, false);
            uiManager?.CompletePrisonAction();
        }

        private void CommitManifestChoice()
        {
            var session = uiManager?.ResolveSession();
            if (session == null || pendingManifestMonster == null)
            {
                return;
            }

            if (!session.TryAddPartyMonster(pendingManifestMonster, out var slotIndex))
            {
                return;
            }

            unitSpawnManager?.SpawnManifestedMonster(session, pendingManifestMonster, slotIndex);
            pendingManifestMonster = null;
            UiObjectUtility.SetActive(manifestedSuccessPopUp, false);
            uiManager?.RefreshInfo();
            uiManager?.CompletePrisonAction();
        }

        private static MonsterDefinition ResolveNextManifestCandidate(RunSession session)
        {
            var monsters = GameDataLoader.CurrentCatalog.GetMonsters();
            var candidates = new System.Collections.Generic.List<MonsterDefinition>();
            for (var i = 0; i < monsters.Length; i++)
            {
                var monster = monsters[i];
                if (monster == null
                    || string.IsNullOrWhiteSpace(monster.MonsterName)
                    || session.GetPartyMemberState(monster.MonsterName) != null)
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
                $"속성: {monster.ElementLabel}\n" +
                $"HP: {monster.BaseStats.MaxHealth:0} / 전투력: {monster.PowerStat:0}\n" +
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

        private bool BindObject()
        {
            if (referencesBound)
            {
                return true;
            }

            if (bindingFailed)
            {
                return false;
            }

            var valid = true;
            manifestedFailPopUp = UiBindingUtility.BindChildObject(
                this,
                transform,
                "ManifestFailPopup",
                nameof(manifestedFailPopUp),
                ref valid);
            manifestedFailBackButton = UiBindingUtility.BindChild<Button>(
                this,
                "ManifestFailPopup/Back",
                nameof(manifestedFailBackButton),
                ref valid);
            manifestedSuccessPopUp = UiBindingUtility.BindChildObject(
                this,
                transform,
                "ManifestSuccessPopup",
                nameof(manifestedSuccessPopUp),
                ref valid);
            dontChoiceButton = UiBindingUtility.BindChild<Button>(
                this,
                "ManifestSuccessPopup/DontChoiceBtn",
                nameof(dontChoiceButton),
                ref valid);
            choiceButton = UiBindingUtility.BindChild<Button>(
                this,
                "ManifestSuccessPopup/ChoiceBtn",
                nameof(choiceButton),
                ref valid);
            monsterNameText = UiBindingUtility.BindChild<TMP_Text>(
                this,
                "ManifestSuccessPopup/MonsterName",
                nameof(monsterNameText),
                ref valid);
            monsterDescText = UiBindingUtility.BindChild<TMP_Text>(
                this,
                "ManifestSuccessPopup/MonsterDesc",
                nameof(monsterDescText),
                ref valid);
            monsterImage = UiBindingUtility.BindChild<Image>(
                this,
                "ManifestSuccessPopup/MonsterImage",
                nameof(monsterImage),
                ref valid);
            stageManager = UiBindingUtility.BindSceneComponent<StageManager>(
                this,
                nameof(stageManager),
                ref valid);
            unitSpawnManager = UiBindingUtility.BindSceneComponent<UnitSpawnManager>(
                this,
                nameof(unitSpawnManager),
                ref valid);
            uiManager = UiBindingUtility.BindSceneComponent<InGameUIManager>(
                this,
                nameof(uiManager),
                ref valid);

            referencesBound = valid;
            bindingFailed = !valid;
            return valid;
        }

    }
}
