using System;
using Pakuri.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Pakuri.InGame
{
    /// 포로 선택, 파티 슬롯, Offering·현현 진입을 관리한다.
    public sealed class PrisonPanelUI : MonoBehaviour
    {
        private const int PrisonPartySlotCount = 5;

        [SerializeField] private GameObject prisonPanel;
        [SerializeField] private GameObject prisonerChoicePopUp;
        [SerializeField] private Image prisonerImage;
        [SerializeField] private TMP_Text prisonerNameText;
        [SerializeField] private PrisonPartySlotView[] prisonPartySlots = new PrisonPartySlotView[PrisonPartySlotCount];
        [SerializeField] private OfferingUI offeringUI;
        [SerializeField] private MenifestUI menifestUI;
        [SerializeField] private InGameUIManager uiManager;

        private readonly string[] prisonSlotMonsterIds = new string[PrisonPartySlotCount];

        public bool IsVisible => prisonPanel != null && prisonPanel.activeSelf;

        private void Awake()
        {
            BindStaticButtons();
        }

        public void BindStaticButtons()
        {
            for (var i = 0; i < prisonPartySlots.Length; i++)
            {
                var capturedIndex = i;
                BindButton(prisonPartySlots[i]?.Button, () => ActivatePrisonPartySlot(capturedIndex));
            }
        }

        public void Open()
        {
            SetActive(prisonerChoicePopUp, false);
            SetActive(prisonPanel, true);
            Refresh();
        }

        public void Hide()
        {
            SetActive(prisonPanel, false);
            SetActive(prisonerChoicePopUp, false);
        }

        public void Refresh()
        {
            uiManager?.RefreshInfo();

            var session = uiManager?.ResolveSession();
            var partyMembers = session?.PartyMembers;
            var occupiedCount = partyMembers != null
                ? Math.Min(partyMembers.Count, PrisonPartySlotCount)
                : 0;
            for (var i = 0; i < prisonPartySlots.Length; i++)
            {
                var isOccupied = i < occupiedCount;
                var isNextManifestSlot = occupiedCount > 0
                    && occupiedCount < PrisonPartySlotCount
                    && i == occupiedCount;
                var monsterId = isOccupied ? partyMembers[i].MonsterId : string.Empty;
                prisonSlotMonsterIds[i] = monsterId;
                RefreshPrisonPartySlot(prisonPartySlots[i], monsterId, isOccupied, isNextManifestSlot);
            }

            RefreshSelectedPrisoner();
        }

        private void ActivatePrisonPartySlot(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= prisonPartySlots.Length)
            {
                return;
            }

            var monsterId = prisonSlotMonsterIds[slotIndex];
            if (!string.IsNullOrWhiteSpace(monsterId))
            {
                if (offeringUI != null && offeringUI.OpenOfferingPanel(monsterId))
                {
                    SetActive(prisonPanel, false);
                }

                return;
            }

            var session = uiManager?.ResolveSession();
            var partyMembers = session?.PartyMembers;
            var occupiedCount = partyMembers != null
                ? Math.Min(partyMembers.Count, PrisonPartySlotCount)
                : 0;
            if (slotIndex != occupiedCount || menifestUI == null || !menifestUI.TryManifestPrisoner())
            {
                return;
            }

            SetActive(prisonPanel, false);
        }

        private static void RefreshPrisonPartySlot(
            PrisonPartySlotView slot,
            string monsterId,
            bool isOccupied,
            bool isNextManifestSlot)
        {
            if (slot == null)
            {
                return;
            }

            SetActive(slot.Image != null ? slot.Image.gameObject : null, isOccupied);
            SetActive(slot.Button != null ? slot.Button.gameObject : null, isOccupied || isNextManifestSlot);
            SetActive(slot.ReinforcementLabel, isOccupied);
            SetActive(slot.MenifestedLabel, isNextManifestSlot);

            if (slot.Button != null)
            {
                slot.Button.interactable = isOccupied || isNextManifestSlot;
            }

            if (!isOccupied)
            {
                return;
            }

            var monster = GameDataLoader.CurrentCatalog.GetMonster(monsterId);
            if (slot.NameText != null)
            {
                slot.NameText.text = monster != null && !string.IsNullOrWhiteSpace(monster.DisplayName)
                    ? monster.DisplayName
                    : monsterId;
            }

            if (slot.Image != null)
            {
                var portrait = monster != null ? monster.Image : null;
                slot.Image.sprite = portrait;
                slot.Image.color = portrait != null ? Color.white : new Color(0f, 0f, 0f, 0.3f);
            }
        }

        private void RefreshSelectedPrisoner()
        {
            var activePrisonerButton = uiManager?.ActivePrisonerButton;
            var prisonerId = activePrisonerButton != null ? activePrisonerButton.PrisonerId : string.Empty;
            var hasPrisoner = !string.IsNullOrWhiteSpace(prisonerId);
            SetActive(prisonerImage != null ? prisonerImage.gameObject : null, hasPrisoner);
            if (!hasPrisoner)
            {
                return;
            }

            if (prisonerNameText != null)
            {
                prisonerNameText.text = uiManager.ResolvePrisonerDisplayName(prisonerId);
            }

            var enemy = GameDataLoader.CurrentCatalog.GetData<EnemyDefinition>(prisonerId);
            if (prisonerImage != null)
            {
                prisonerImage.sprite = enemy != null ? enemy.Image : null;
                prisonerImage.color = prisonerImage.sprite != null
                    ? Color.white
                    : new Color(0f, 0f, 0f, 0.3f);
            }
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

        [Serializable]
        private sealed class PrisonPartySlotView
        {
            [SerializeField] private Image image;
            [SerializeField] private TMP_Text nameText;
            [SerializeField] private Button button;
            [SerializeField] private GameObject reinforcementLabel;
            [SerializeField] private GameObject manifestedLabel;

            public Image Image => image;
            public TMP_Text NameText => nameText;
            public Button Button => button;
            public GameObject ReinforcementLabel => reinforcementLabel;
            public GameObject MenifestedLabel => manifestedLabel;
        }
    }
}
