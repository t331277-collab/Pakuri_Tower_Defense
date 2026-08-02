using System;
using Pakuri.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Pakuri.InGame
{
    /// 포로 선택, 파티 슬롯, Offering·현현 진입을 관리한다.
    internal sealed class PrisonPanelUI
    {
        private const int PrisonPartySlotCount = 5;

        private readonly GameObject prisonPanel;
        private readonly GameObject prisonerChoicePopUp;
        private readonly Image prisonPrisonerImage;
        private readonly TMP_Text prisonPrisonerNameText;
        private readonly PrisonPartySlotView[] prisonPartySlots = new PrisonPartySlotView[PrisonPartySlotCount];
        private readonly string[] prisonSlotMonsterIds = new string[PrisonPartySlotCount];
        private readonly Func<RunSession> resolveSession;
        private readonly Func<string, string> resolvePrisonerDisplayName;
        private readonly Func<RewardButtonView> resolveActivePrisonerButton;
        private readonly OfferingUI offeringUI;
        private readonly MenifestUI menifestUI;
        private readonly Action refreshInfo;

        public PrisonPanelUI(
            InGamePrisonPanelReferences references,
            Func<RunSession> resolveSession,
            Func<string, string> resolvePrisonerDisplayName,
            Func<RewardButtonView> resolveActivePrisonerButton,
            OfferingUI offeringUI,
            MenifestUI menifestUI,
            Action refreshInfo)
        {
            prisonPanel = references != null ? references.prisonPanel : null;
            prisonerChoicePopUp = references != null ? references.prisonerChoicePopUp : null;
            prisonPrisonerImage = references != null ? references.prisonerImage : null;
            prisonPrisonerNameText = references != null ? references.prisonerNameText : null;
            this.resolveSession = resolveSession;
            this.resolvePrisonerDisplayName = resolvePrisonerDisplayName;
            this.resolveActivePrisonerButton = resolveActivePrisonerButton;
            this.offeringUI = offeringUI;
            this.menifestUI = menifestUI;
            this.refreshInfo = refreshInfo;

            var slotReferences = references != null
                ? new[]
                {
                    references.partySlot1,
                    references.partySlot2,
                    references.partySlot3,
                    references.partySlot4,
                    references.partySlot5
                }
                : Array.Empty<InGamePrisonPartySlotReferences>();
            for (var i = 0; i < prisonPartySlots.Length; i++)
            {
                var slot = i < slotReferences.Length ? slotReferences[i] : null;
                prisonPartySlots[i] = new PrisonPartySlotView(
                    slot != null ? slot.image : null,
                    slot != null ? slot.nameText : null,
                    slot != null ? slot.button : null,
                    slot != null ? slot.reinforcementLabel : null,
                    slot != null ? slot.manifestedLabel : null);
            }
        }

        public bool IsVisible => prisonPanel != null && prisonPanel.activeSelf;

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
            refreshInfo?.Invoke();

            var session = resolveSession?.Invoke();
            var partyMembers = session != null ? session.PartyMembers : null;
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

            var session = resolveSession?.Invoke();
            var partyMembers = session != null ? session.PartyMembers : null;
            var occupiedCount = partyMembers != null
                ? Math.Min(partyMembers.Count, PrisonPartySlotCount)
                : 0;
            if (slotIndex != occupiedCount || menifestUI == null || !menifestUI.TryManifestPrisoner())
            {
                return;
            }

            SetActive(prisonPanel, false);
        }

        private void RefreshPrisonPartySlot(
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
            var activePrisonerButton = resolveActivePrisonerButton?.Invoke();
            var prisonerId = activePrisonerButton != null ? activePrisonerButton.PrisonerId : string.Empty;
            var hasPrisoner = !string.IsNullOrWhiteSpace(prisonerId);
            SetActive(prisonPrisonerImage != null ? prisonPrisonerImage.gameObject : null, hasPrisoner);
            if (!hasPrisoner)
            {
                return;
            }

            if (prisonPrisonerNameText != null)
            {
                prisonPrisonerNameText.text = resolvePrisonerDisplayName(prisonerId);
            }

            var enemy = GameDataLoader.CurrentCatalog.GetData<EnemyDefinition>(prisonerId);
            if (prisonPrisonerImage != null)
            {
                prisonPrisonerImage.sprite = enemy != null ? enemy.Image : null;
                prisonPrisonerImage.color = prisonPrisonerImage.sprite != null
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
    }

    internal sealed class PrisonPartySlotView
    {
        public PrisonPartySlotView(
            Image image,
            TMP_Text nameText,
            Button button,
            GameObject reinforcementLabel,
            GameObject menifestedLabel)
        {
            Image = image;
            NameText = nameText;
            Button = button;
            ReinforcementLabel = reinforcementLabel;
            MenifestedLabel = menifestedLabel;
        }

        public Image Image { get; }
        public TMP_Text NameText { get; }
        public Button Button { get; }
        public GameObject ReinforcementLabel { get; }
        public GameObject MenifestedLabel { get; }
    }
}
