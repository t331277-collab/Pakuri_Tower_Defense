using System;
using System.Collections.Generic;
using Pakuri.NewCore.Bootstrap;
using Pakuri.NewCore.Run;
using Pakuri.NewCore.Run.Services;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/* Reward panel의 지급 결과, 포로 버튼, 다음 진행 command를 소유한다. */
namespace Pakuri.NewCore.UI.InGame
{
    public class RewardPanelController : MonoBehaviour
    {
        [SerializeField] private GameBootstrap combatManager;
        [SerializeField] private Vector2 rewardButtonFirstColumnPosition =
            new Vector2(-321.97855f, 295f);
        [SerializeField] private float rewardButtonColumnSpacingX =
            533.97855f;
        [SerializeField] private float rewardButtonRowSpacingY = 122f;
        [SerializeField] private int rewardButtonRowsPerColumn = 3;

        private readonly List<Button> prisonerButtons =
            new List<Button>();
        private GameObject rewardPanel;
        private Transform rewardContainer;
        private Button prisonerTemplate;
        private Button darkTemplate;
        private Button goldTemplate;
        private Button nextButton;
        private TMP_Text rewardSummary;
        private Action<Prisoner, Button> prisonerSelected;

        /* bootstrap과 manager callbacks를 연결하고 authored Reward hierarchy를 찾는다. */
        public void Initialize(
            GameBootstrap runtime,
            Action<Prisoner, Button> onPrisonerSelected,
            UnityEngine.Events.UnityAction onContinue)
        {
            combatManager ??= runtime;
            prisonerSelected = onPrisonerSelected;
            ResolveSceneUi();
            Bind(nextButton, onContinue);
            Hide();
        }

        /* 지급 결과와 현재 inventory 포로를 순서대로 Reward button에 표시한다. */
        public void Show(RewardResult reward)
        {

            ClearPrisonerButtons();
            ConfigureRewardButton(
                goldTemplate,
                $"Gold\n{reward.Gold}",
                0,
                null);
            ConfigureRewardButton(
                darkTemplate,
                $"Dark Trace\n{reward.DarkTrace}",
                1,
                null);
            IReadOnlyList<Prisoner> prisoners =
                combatManager.Stage.Session.PrisonerInventory.Prisoners;
            for (int index = 0; index < prisoners.Count; index++)
            {
                Button button;
                if (index == 0)
                {
                    button = prisonerTemplate;
                }
                else
                {
                    button = Instantiate(
                        prisonerTemplate,
                        rewardContainer);
                }

                prisonerButtons.Add(button);
                ConfigureRewardButton(
                    button,
                    "Prisoner\n" + ResolveEnemyName(
                        prisoners[index].EnemyId),
                    index + 2,
                    prisoners[index]);
            }

            if (rewardSummary != null)
            {
                rewardSummary.text =
                    $"Gold {reward.Gold} / Dark Trace {reward.DarkTrace} / Prisoners {prisoners.Count}";
            }

            SetActive(rewardPanel, true);
        }

        /* Reward panel을 다음 combat 구간에서 보이지 않게 닫는다. */
        public void Hide()
        {
            SetActive(rewardPanel, false);
        }

        /* 단일 Reward button의 label, 위치, 포로 command를 구성한다. */
        private void ConfigureRewardButton(
            Button button,
            string label,
            int order,
            Prisoner prisoner)
        {
            if (button == null)
            {
                return;
            }

            button.gameObject.SetActive(true);
            button.interactable = true;
            SetLabel(button, label);
            Arrange(button, order);
            button.onClick.RemoveAllListeners();
            if (prisoner != null)
            {
                button.onClick.AddListener(
                    () => prisonerSelected(prisoner, button));
            }
        }

        /* authored 행·열 간격으로 동적 Reward button 위치를 계산한다. */
        private void Arrange(Button button, int order)
        {
            if (!(button.transform is RectTransform rect))
            {
                return;
            }

            int rows = Mathf.Max(1, rewardButtonRowsPerColumn);
            int column = order / rows;
            int row = order % rows;
            rect.anchoredPosition = new Vector2(
                rewardButtonFirstColumnPosition.x
                    + rewardButtonColumnSpacingX * column,
                rewardButtonFirstColumnPosition.y
                    - rewardButtonRowSpacingY * row);
        }

        /* 이전 동적 포로 button을 삭제하고 template을 숨긴다. */
        private void ClearPrisonerButtons()
        {
            for (int index = 1; index < prisonerButtons.Count; index++)
            {
                if (prisonerButtons[index] != null)
                {
                    Destroy(prisonerButtons[index].gameObject);
                }
            }

            prisonerButtons.Clear();
            if (prisonerTemplate != null)
            {
                prisonerTemplate.gameObject.SetActive(false);
            }
        }

        /* Reward panel의 기존 UGUI object를 고정 hierarchy path로 연결한다. */
        private void ResolveSceneUi()
        {
            rewardPanel = FindObject("RewardPanel");
            rewardContainer = FindTransform(
                "RewardPanel/RewardBtnContainer");
            prisonerTemplate = Find<Button>(
                "RewardPanel/RewardBtnContainer/PrisonerBtn");
            darkTemplate = Find<Button>(
                "RewardPanel/RewardBtnContainer/DarkBtn");
            goldTemplate = Find<Button>(
                "RewardPanel/RewardBtnContainer/GoldBtn");
            nextButton = Find<Button>("RewardPanel/NextBtn");
            rewardSummary = Find<TMP_Text>("RewardPanel/Summary");
        }

        /* Enemy Definition의 authored 표시 이름을 반환한다. */
        private string ResolveEnemyName(string enemyId)
        {
            var definition = combatManager.Catalog.GetEnemy(enemyId);
            if (string.IsNullOrWhiteSpace(definition.display_name))
            {
                return enemyId;
            }

            return definition.display_name;
        }

        /* 현재 Canvas 아래 path에 대응하는 GameObject를 반환한다. */
        private GameObject FindObject(string path)
        {
            Transform target = FindTransform(path);
            if (target == null)
            {
                return null;
            }

            return target.gameObject;
        }

        /* 현재 Canvas 아래 path에서 지정 UGUI component를 반환한다. */
        private T Find<T>(string path)
            where T : Component
        {
            Transform target = FindTransform(path);
            if (target == null)
            {
                return null;
            }

            return target.GetComponent<T>();
        }

        /* 현재 Canvas 아래 authored hierarchy path를 찾는다. */
        private Transform FindTransform(string path)
        {
            return transform.Find(path);
        }

        /* button 자식의 첫 TMP label에 표시 text를 쓴다. */
        private static void SetLabel(Button button, string text)
        {
            TMP_Text label =
                button.GetComponentInChildren<TMP_Text>(true);
            if (label != null)
            {
                label.text = text;
            }
        }

        /* button에 존재하는 command callback 하나를 연결한다. */
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

        /* 선택 panel의 활성 상태를 존재하는 경우에만 바꾼다. */
        private static void SetActive(GameObject target, bool active)
        {
            if (target != null)
            {
                target.SetActive(active);
            }
        }
    }
}
