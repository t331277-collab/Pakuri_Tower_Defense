using System;
using Pakuri.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Pakuri.InGame
{
    /// 포로 몬스터의 영입 성공·실패 화면과 파티 추가 선택을 관리한다.
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
        private readonly Func<RunSession> resolveSession;
        private readonly Func<StageManager> resolveStageManager;
        private readonly Func<UnitSpawnManager> resolveUnitSpawnManager;
        private readonly Func<RewardButtonView> resolveActivePrisonerButton;
        private readonly Action consumePrisonerButton;
        private readonly Action completePrisonAction;
        private readonly Action refreshInfo;

        private MonsterDefinition pendingManifestMonster;

        public MenifestUI(
            InGameMenifestReferences references,
            Func<RunSession> resolveSession,
            Func<StageManager> resolveStageManager,
            Func<UnitSpawnManager> resolveUnitSpawnManager,
            Func<RewardButtonView> resolveActivePrisonerButton,
            Action consumePrisonerButton,
            Action completePrisonAction,
            Action refreshInfo)
        {
            manifestedFailPopUp = references != null ? references.failPopUp : null;
            manifestedFailBackButton = references != null ? references.failBackButton : null;
            manifestedSuccessPopUp = references != null ? references.successPopUp : null;
            dontChoiceButton = references != null ? references.dontChoiceButton : null;
            choiceButton = references != null ? references.choiceButton : null;
            monsterNameText = references != null ? references.monsterNameText : null;
            monsterDescText = references != null ? references.monsterDescText : null;
            monsterImage = references != null ? references.monsterImage : null;
            this.resolveSession = resolveSession;
            this.resolveStageManager = resolveStageManager;
            this.resolveUnitSpawnManager = resolveUnitSpawnManager;
            this.resolveActivePrisonerButton = resolveActivePrisonerButton;
            this.consumePrisonerButton = consumePrisonerButton;
            this.completePrisonAction = completePrisonAction;
            this.refreshInfo = refreshInfo;

            BindButton(manifestedFailBackButton, CompleteAfterFailure);
            BindButton(dontChoiceButton, SkipManifestChoice);
            BindButton(choiceButton, CommitManifestChoice);
        }

        public bool TryManifestPrisoner()
        {
            var session = resolveSession?.Invoke();
            var activePrisonerButton = resolveActivePrisonerButton?.Invoke();
            if (session == null || activePrisonerButton == null || activePrisonerButton.Consumed)
            {
                return false;
            }

            consumePrisonerButton?.Invoke();
            pendingManifestMonster = ResolveNextManifestCandidate(session);
            var stageManager = resolveStageManager?.Invoke();
            var successChance = stageManager != null ? stageManager.PendingManifestSuccessChance : 0.7f;
            var succeeded = pendingManifestMonster != null && UnityEngine.Random.value < successChance;
            if (!succeeded)
            {
                SetActive(manifestedFailPopUp, true);
                return true;
            }

            ShowManifestSuccessPopup(pendingManifestMonster);
            return true;
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
                monsterImage.sprite = null;
                monsterImage.color = new Color(0f, 0f, 0f, 0.3f);
                if (monster != null && monster.Image != null)
                {
                    monsterImage.sprite = monster.Image;
                    monsterImage.color = Color.white;
                }
            }
        }

        private void SkipManifestChoice()
        {
            pendingManifestMonster = null;
            SetActive(manifestedSuccessPopUp, false);
            completePrisonAction?.Invoke();
        }

        private void CompleteAfterFailure()
        {
            pendingManifestMonster = null;
            SetActive(manifestedFailPopUp, false);
            completePrisonAction?.Invoke();
        }

        private void CommitManifestChoice()
        {
            var session = resolveSession?.Invoke();
            if (session == null || pendingManifestMonster == null)
            {
                return;
            }

            if (!session.TryAddPartyMonster(pendingManifestMonster, out var slotIndex))
            {
                return;
            }

            var unitSpawnManager = resolveUnitSpawnManager?.Invoke();
            if (unitSpawnManager != null)
            {
                unitSpawnManager.SpawnManifestedMonster(session, pendingManifestMonster, slotIndex);
            }

            pendingManifestMonster = null;
            SetActive(manifestedSuccessPopUp, false);
            refreshInfo?.Invoke();
            completePrisonAction?.Invoke();
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

        private static void SetActive(GameObject target, bool active)
        {
            if (target != null)
            {
                target.SetActive(active);
            }
        }
    }
}
