using System;
using Pakuri.NewCore.Bootstrap;
using Pakuri.NewCore.Definitions.Stage;
using Pakuri.NewCore.Definitions.Units;
using Pakuri.NewCore.Run;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/* 현현 시도 결과 popup과 모집·건너뛰기 command를 소유한다. */
namespace Pakuri.NewCore.UI.InGame
{
    public sealed class ManifestationPanelController : MonoBehaviour
    {
        [SerializeField] private GameBootstrap combatManager;
        [SerializeField] private Sprite arielPrisonPortrait;
        [SerializeField] private Sprite evePrisonPortrait;
        [SerializeField] private Sprite rinPrisonPortrait;
        [SerializeField] private Sprite seinPrisonPortrait;
        [SerializeField] private Sprite vegaPrisonPortrait;

        private GameObject manifestFailurePopup;
        private GameObject manifestSuccessPopup;
        private TMP_Text manifestName;
        private TMP_Text manifestDescription;
        private Image manifestImage;
        private Action completed;

        /* bootstrap과 완료 callback을 연결하고 authored 현현 popup hierarchy를 찾는다. */
        public void Initialize(
            GameBootstrap runtime,
            Action onCompleted)
        {
            combatManager ??= runtime;
            completed = onCompleted
                ?? throw new ArgumentNullException(nameof(onCompleted));
            ResolveSceneUi();
            ResolveButtons();
            HideAll();
        }

        /* 포로와 현재 Reward Definition으로 현현 시도를 시작해 결과 popup을 연다. */
        public bool Begin(
            Prisoner prisoner,
            StageRewardDefinition rewardDefinition)
        {
            if (prisoner == null || rewardDefinition == null)
            {
                return false;
            }

            var result = combatManager.Manifestations.BeginAttempt(
                prisoner,
                rewardDefinition);
            if (!result.Success)
            {
                SetActive(manifestFailurePopup, true);
                return true;
            }

            BindCandidate(result.Candidate);
            SetActive(manifestSuccessPopup, true);
            return true;
        }

        /* 실패·성공 popup을 모두 닫는다. */
        public void HideAll()
        {
            SetActive(manifestFailurePopup, false);
            SetActive(manifestSuccessPopup, false);
        }

        /* authored 실패·건너뛰기·모집 button에 command callback 하나씩 연결한다. */
        private void ResolveButtons()
        {
            Bind(
                Find<Button>("MenifestedFailPopUp/Back"),
                FinishFailure);
            Bind(
                Find<Button>(
                    "MenifestedSuccessPopUp/DontChoiceBtn"),
                Skip);
            Bind(
                Find<Button>(
                    "MenifestedSuccessPopUp/ChoiceBtn"),
                Recruit);
        }

        /* 실패 popup을 닫고 Reward flow 완료를 알린다. */
        private void FinishFailure()
        {
            SetActive(manifestFailurePopup, false);
            completed();
        }

        /* 현현 모집을 건너뛴 경우 성공 popup을 닫고 완료를 알린다. */
        private void Skip()
        {
            if (combatManager.Manifestations.SkipRecruitment())
            {
                SetActive(manifestSuccessPopup, false);
                completed();
            }
        }

        /* 현현 후보를 party에 확정하고 scene Actor를 생성한 뒤 완료를 알린다. */
        private void Recruit()
        {
            var monster =
                combatManager.Manifestations.ConfirmRecruitment();
            combatManager.PresentManifestedMonster(monster);
            SetActive(manifestSuccessPopup, false);
            completed();
        }

        /* 현현 후보의 이름, 설명, portrait를 성공 popup에 표시한다. */
        private void BindCandidate(MonsterDefinition definition)
        {
            if (manifestName != null)
            {
                manifestName.text = definition.display_name;
            }
            if (manifestDescription != null)
            {
                manifestDescription.text =
                    $"{definition.role_summary}\n"
                    + $"Element: {definition.element_label}\n"
                    + $"HP: {definition.max_health:0} / Power: {definition.power_stat:0}";
            }
            if (manifestImage != null)
            {
                Sprite portrait =
                    ResolveMonsterPortrait(definition.id);
                manifestImage.sprite = portrait;
                manifestImage.color = portrait != null
                    ? Color.white
                    : new Color(0f, 0f, 0f, 0.3f);
            }
        }

        /* 현현 popup의 기존 UGUI object를 고정 hierarchy path로 연결한다. */
        private void ResolveSceneUi()
        {
            manifestFailurePopup =
                FindObject("MenifestedFailPopUp");
            manifestSuccessPopup =
                FindObject("MenifestedSuccessPopUp");
            manifestName = Find<TMP_Text>(
                "MenifestedSuccessPopUp/MonsterName");
            manifestDescription = Find<TMP_Text>(
                "MenifestedSuccessPopUp/MonsterDesc");
            manifestImage = Find<Image>(
                "MenifestedSuccessPopUp/MonsterImage");
        }

        /* 고정 Monster id에 대응하는 Inspector portrait를 반환한다. */
        private Sprite ResolveMonsterPortrait(string monsterId)
        {
            switch (monsterId)
            {
                case "ariel": return arielPrisonPortrait;
                case "eve": return evePrisonPortrait;
                case "rin": return rinPrisonPortrait;
                case "sein": return seinPrisonPortrait;
                case "vega": return vegaPrisonPortrait;
                default: return null;
            }
        }

        /* 현재 Canvas 아래 path에 대응하는 GameObject를 반환한다. */
        private GameObject FindObject(string path)
        {
            Transform target = transform.Find(path);
            return target != null ? target.gameObject : null;
        }

        /* 현재 Canvas 아래 path에서 지정 UGUI component를 반환한다. */
        private T Find<T>(string path)
            where T : Component
        {
            Transform target = transform.Find(path);
            return target != null ? target.GetComponent<T>() : null;
        }

        /* button에 manifestation command callback 하나를 연결한다. */
        private static void Bind(
            Button button,
            UnityEngine.Events.UnityAction action)
        {
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(action);
            }
        }

        /* 선택 popup의 활성 상태를 존재하는 경우에만 바꾼다. */
        private static void SetActive(GameObject target, bool active)
        {
            if (target != null)
            {
                target.SetActive(active);
            }
        }
    }
}
