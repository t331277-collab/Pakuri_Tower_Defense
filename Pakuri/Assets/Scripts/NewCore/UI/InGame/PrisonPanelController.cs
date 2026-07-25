using System;
using Pakuri.NewCore.Bootstrap;
using Pakuri.NewCore.Definitions.Units;
using Pakuri.NewCore.Run;
using Pakuri.NewCore.Units.Models;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/* Prison panel의 포로 표시, party slot 상태, Offering·현현 분기를 소유한다. */
namespace Pakuri.NewCore.UI.InGame
{
    public sealed class PrisonPanelController : MonoBehaviour
    {
        private const int PartySlots = 5;

        [SerializeField] private GameBootstrap combatManager;
        [SerializeField] private Sprite arielPrisonPortrait;
        [SerializeField] private Sprite evePrisonPortrait;
        [SerializeField] private Sprite rinPrisonPortrait;
        [SerializeField] private Sprite seinPrisonPortrait;
        [SerializeField] private Sprite vegaPrisonPortrait;

        private readonly Button[] partyButtons =
            new Button[PartySlots];
        private readonly Image[] partyImages =
            new Image[PartySlots];
        private readonly TMP_Text[] partyNames =
            new TMP_Text[PartySlots];
        private GameObject prisonPanel;
        private GameObject prisonerChoicePopup;
        private Image prisonerImage;
        private TMP_Text prisonerName;
        private TMP_Text stageInfo;
        private TMP_Text goldInfo;
        private TMP_Text darkInfo;
        private Prisoner activePrisoner;
        private Action<MonsterModel, Prisoner> offeringRequested;
        private Action<Prisoner> manifestationRequested;

        /* bootstrap과 두 party-slot command를 연결하고 authored Prison hierarchy를 찾는다. */
        public void Initialize(
            GameBootstrap runtime,
            Action<MonsterModel, Prisoner> onOfferingRequested,
            Action<Prisoner> onManifestationRequested)
        {
            combatManager ??= runtime;
            offeringRequested = onOfferingRequested
                ?? throw new ArgumentNullException(
                    nameof(onOfferingRequested));
            manifestationRequested = onManifestationRequested
                ?? throw new ArgumentNullException(
                    nameof(onManifestationRequested));
            ResolveSceneUi();
            ResolvePartyButtons();
            CloseFlow();
            RefreshInfo();
        }

        /* 선택 포로를 검증하고 party 선택 overlay에 표시한다. */
        public void Open(Prisoner prisoner, Button sourceButton)
        {
            if (!combatManager.Stage.Session.PrisonerInventory
                .CanConsume(prisoner))
            {
                if (sourceButton != null)
                {
                    sourceButton.interactable = false;
                }
                return;
            }

            activePrisoner = prisoner;
            if (prisonerImage != null)
            {
                prisonerImage.sprite = null;
                prisonerImage.color =
                    new Color(0f, 0f, 0f, 0.3f);
            }

            if (prisonerName != null)
            {
                prisonerName.text =
                    ResolveEnemyName(prisoner.EnemyId);
            }

            RefreshPartySlots();
            SetActive(prisonerChoicePopup, true);
            SetActive(prisonPanel, true);
        }

        /* Offering 또는 현현 popup이 열릴 때 Prison panel만 숨긴다. */
        public void HidePanel()
        {
            SetActive(prisonPanel, false);
        }

        /* 포로 선택 overlay와 Prison panel을 함께 닫고 선택 상태를 비운다. */
        public void CloseFlow()
        {
            activePrisoner = null;
            SetActive(prisonPanel, false);
            SetActive(prisonerChoicePopup, false);
        }

        /* 현재 session stage와 Gold·DarkTrace를 정보 label에 표시한다. */
        public void RefreshInfo()
        {
            if (combatManager == null || combatManager.Stage == null)
            {
                return;
            }

            var session = combatManager.Stage.Session;
            if (stageInfo != null)
            {
                stageInfo.text =
                    $"{session.CurrentStageId} Day {session.CurrentDay}";
            }
            if (goldInfo != null)
            {
                goldInfo.text =
                    combatManager.Stage.Gold.ToString();
            }
            if (darkInfo != null)
            {
                darkInfo.text =
                    combatManager.Stage.DarkTrace.ToString();
            }
        }

        /* 다섯 authored party slot의 Image, Name, Button과 command를 연결한다. */
        private void ResolvePartyButtons()
        {
            for (int index = 0; index < PartySlots; index++)
            {
                int captured = index;
                string path = $"PrisonPanel/{index + 1}P";
                partyImages[index] = Find<Image>(path + "/Image");
                partyNames[index] =
                    Find<TMP_Text>(path + "/Image/Name");
                partyButtons[index] =
                    Find<Button>(path + "/Button");
                Bind(
                    partyButtons[index],
                    () => SelectPartySlot(captured));
            }
        }

        /* ordered party와 다음 빈 slot을 authored 다섯 slot에 투영한다. */
        private void RefreshPartySlots()
        {
            var party =
                combatManager.Stage.Session.PartyRoster.Members;
            for (int index = 0; index < PartySlots; index++)
            {
                bool occupied = index < party.Count;
                bool availableManifestSlot =
                    index == party.Count && party.Count < PartySlots;
                if (partyButtons[index] != null)
                {
                    partyButtons[index].gameObject.SetActive(
                        occupied || availableManifestSlot);
                    partyButtons[index].interactable =
                        occupied || availableManifestSlot;
                }

                if (partyImages[index] != null)
                {
                    partyImages[index].gameObject.SetActive(occupied);
                }
                if (!occupied)
                {
                    continue;
                }

                MonsterDefinition definition =
                    party[index].MonsterDefinition;
                if (partyNames[index] != null)
                {
                    partyNames[index].text =
                        definition.display_name;
                }
                if (partyImages[index] != null)
                {
                    partyImages[index].sprite =
                        ResolveMonsterPortrait(definition.id);
                }
            }
        }

        /* 점유 slot은 Offering, 다음 빈 slot은 현현 command로 전달한다. */
        private void SelectPartySlot(int slot)
        {
            if (activePrisoner == null)
            {
                return;
            }

            var party =
                combatManager.Stage.Session.PartyRoster.Members;
            Prisoner prisoner = activePrisoner;
            if (slot < party.Count)
            {
                offeringRequested(party[slot], prisoner);
            }
            else if (slot == party.Count)
            {
                manifestationRequested(prisoner);
            }
        }

        /* Prison panel과 공통 Info hierarchy의 기존 UGUI object를 연결한다. */
        private void ResolveSceneUi()
        {
            prisonPanel = FindObject("PrisonPanel");
            prisonerChoicePopup =
                FindObject("PrisonerChoicePopUp");
            prisonerImage =
                Find<Image>("PrisonPanel/Prisonal/Image");
            prisonerName = Find<TMP_Text>(
                "PrisonPanel/Prisonal/Image/Name");
            stageInfo = Find<TMP_Text>("Info/StageInfo");
            goldInfo = Find<TMP_Text>("Info/Goldinfo");
            darkInfo = Find<TMP_Text>("Info/Darkinfo");
        }

        /* Enemy Definition의 authored 표시 이름을 반환한다. */
        private string ResolveEnemyName(string enemyId)
        {
            var definition = combatManager.Catalog.GetEnemy(enemyId);
            return string.IsNullOrWhiteSpace(definition.display_name)
                ? enemyId
                : definition.display_name;
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

        /* button에 party-slot command callback 하나를 연결한다. */
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
