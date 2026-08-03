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
        [SerializeField] private GameObject manifestedFailPopUp;
        [SerializeField] private Button manifestedFailBackButton;
        [SerializeField] private GameObject manifestedSuccessPopUp;
        [SerializeField] private Button dontChoiceButton;
        [SerializeField] private Button choiceButton;
        [SerializeField] private TMP_Text monsterNameText;
        [SerializeField] private TMP_Text monsterDescText;
        [SerializeField] private Image monsterImage;
        [SerializeField] private StageManager stageManager;
        [SerializeField] private UnitSpawnManager unitSpawnManager;
        [SerializeField] private InGameUIManager uiManager;

        private MonsterDefinition pendingManifestMonster;

        public bool IsFailurePopupVisible => manifestedFailPopUp != null && manifestedFailPopUp.activeSelf;

        private void Awake()
        {
            BindButton(manifestedFailBackButton, CompleteAfterFailure);
            BindButton(dontChoiceButton, SkipManifestChoice);
            BindButton(choiceButton, CommitManifestChoice);
        }

        public bool TryManifestPrisoner()
        {
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
                    || string.IsNullOrWhiteSpace(monster.MonsterId)
                    || session.GetPartyMemberState(monster.MonsterId) != null)
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

    }
}
