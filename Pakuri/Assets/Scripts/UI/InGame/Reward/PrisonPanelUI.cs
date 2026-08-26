using System;
using Pakuri.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Pakuri.InGame
{
    public enum PrisonActionMode
    {
        Any,
        OfferingOnly,
        ManifestOnly,
        ArtifactRecipient
    }

    /// 포로 선택, 파티 슬롯, Offering·현현 진입을 관리한다.
    public sealed class PrisonPanelUI : MonoBehaviour
    {
        private const int PrisonPartySlotCount = 5;

        private GameObject prisonPanel;
        private GameObject prisonerArea;
        private Image prisonerImage;
        private TMP_Text prisonerNameText;
        private PrisonPartySlotView[] prisonPartySlots = new PrisonPartySlotView[PrisonPartySlotCount];
        private OfferingUI offeringUI;
        private MenifestUI menifestUI;
        private InGameUIManager uiManager;
        private UnitSpawnManager unitSpawnManager;
        private CharacterInfoPopupUI characterInfoPopupUI;

        private readonly string[] prisonSlotMonsterNames = new string[PrisonPartySlotCount];
        private string pendingArtifactName;
        private bool referencesBound;
        private bool bindingFailed;
        private PrisonActionMode actionMode = PrisonActionMode.Any;

        public bool IsVisible => prisonPanel != null && prisonPanel.activeSelf;

        public void SetActionMode(PrisonActionMode mode)
        {
            actionMode = mode;
            if (IsVisible)
            {
                Refresh();
            }
        }

        private void Awake()
        {
            if (!BindObject())
            {
                enabled = false;
                return;
            }

            BindStaticButtons();
        }

        public void BindStaticButtons()
        {
            for (var i = 0; i < prisonPartySlots.Length; i++)
            {
                var capturedIndex = i;
                BindButton(prisonPartySlots[i]?.Button, () => ActivatePrisonPartySlot(capturedIndex));
                BindButton(prisonPartySlots[i]?.MoreInfoButton, () => OpenCharacterInfo(capturedIndex));
            }
        }

        public void Open()
        {
            if (!BindObject())
            {
                return;
            }

            pendingArtifactName = string.Empty;
            characterInfoPopupUI?.Hide();
            BindStaticButtons();
            UiObjectUtility.SetActive(prisonPanel, true);
            Refresh();
        }

        public void OpenArtifactAcquisition(string artifactName)
        {
            if (string.IsNullOrWhiteSpace(artifactName) || !BindObject())
            {
                return;
            }

            pendingArtifactName = artifactName;
            characterInfoPopupUI?.Hide();
            BindStaticButtons();
            UiObjectUtility.SetActive(prisonPanel, true);
            Refresh();
        }

        public void Hide()
        {
            UiObjectUtility.SetActive(prisonPanel, false);
            characterInfoPopupUI?.Hide();
            pendingArtifactName = string.Empty;
        }

        public void Refresh()
        {
            uiManager?.RefreshInfo();

            var session = uiManager?.ResolveSession();
            var partyMembers = session?.PartyMembers;
            var occupiedCount = partyMembers != null
                ? Math.Min(partyMembers.Count, PrisonPartySlotCount)
                : 0;
            var isArtifactAcquisition = !string.IsNullOrWhiteSpace(pendingArtifactName);
            UiObjectUtility.SetActive(prisonerArea, !isArtifactAcquisition);
            for (var i = 0; i < prisonPartySlots.Length; i++)
            {
                var isOccupied = i < occupiedCount;
                var isNextManifestSlot = occupiedCount > 0
                    && occupiedCount < PrisonPartySlotCount
                    && i == occupiedCount;
                var monsterName = isOccupied ? partyMembers[i].MonsterName : string.Empty;
                prisonSlotMonsterNames[i] = monsterName;
                var canAcquireArtifact = isOccupied
                    && session.CanAcquireArtifact(partyMembers[i], pendingArtifactName);
                RefreshPrisonPartySlot(
                    prisonPartySlots[i],
                    monsterName,
                    isOccupied,
                    isNextManifestSlot,
                    isArtifactAcquisition,
                    canAcquireArtifact);
            }

            if (!isArtifactAcquisition)
            {
                RefreshSelectedPrisoner();
            }
        }

        private void ActivatePrisonPartySlot(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= prisonPartySlots.Length)
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(pendingArtifactName))
            {
                if (actionMode != PrisonActionMode.Any && actionMode != PrisonActionMode.ArtifactRecipient)
                {
                    return;
                }

                AcquireArtifact(slotIndex);
                return;
            }

            var monsterName = prisonSlotMonsterNames[slotIndex];
            if (!string.IsNullOrWhiteSpace(monsterName))
            {
                if (actionMode == PrisonActionMode.ManifestOnly || slotIndex != 0 && actionMode == PrisonActionMode.OfferingOnly)
                {
                    return;
                }

                if (offeringUI != null && offeringUI.OpenOfferingPanel(monsterName))
                {
                    UiObjectUtility.SetActive(prisonPanel, false);
                }

                return;
            }

            var session = uiManager?.ResolveSession();
            var partyMembers = session?.PartyMembers;
            var occupiedCount = partyMembers != null
                ? Math.Min(partyMembers.Count, PrisonPartySlotCount)
                : 0;
            if (actionMode == PrisonActionMode.OfferingOnly
                || actionMode == PrisonActionMode.ArtifactRecipient)
            {
                return;
            }

            if (slotIndex != occupiedCount || menifestUI == null || !menifestUI.TryManifestPrisoner())
            {
                return;
            }

            if (!menifestUI.IsFailurePopupVisible)
            {
                UiObjectUtility.SetActive(prisonPanel, false);
            }
        }

        private void AcquireArtifact(int slotIndex)
        {
            var session = uiManager?.ResolveSession();
            if (session == null || slotIndex < 0 || slotIndex >= session.PartyMembers.Count)
            {
                return;
            }

            var member = session.PartyMembers[slotIndex];
            if (!session.TryAcquireArtifact(member, pendingArtifactName))
            {
                Refresh();
                return;
            }

            pendingArtifactName = string.Empty;
            uiManager?.CompleteArtifactAcquisition();
        }

        private void RefreshPrisonPartySlot(
            PrisonPartySlotView slot,
            string monsterName,
            bool isOccupied,
            bool isNextManifestSlot,
            bool isArtifactAcquisition,
            bool canAcquireArtifact)
        {
            if (slot == null)
            {
                return;
            }

            UiObjectUtility.SetActive(slot.Image != null ? slot.Image.gameObject : null, isOccupied);
            UiObjectUtility.SetActive(
                slot.Button != null ? slot.Button.gameObject : null,
                isArtifactAcquisition ? isOccupied : isOccupied || isNextManifestSlot);
            UiObjectUtility.SetActive(slot.ReinforcementLabel, !isArtifactAcquisition && isOccupied);
            UiObjectUtility.SetActive(slot.MenifestedLabel, !isArtifactAcquisition && isNextManifestSlot);
            UiObjectUtility.SetActive(slot.AcquisitionLabel, isArtifactAcquisition && isOccupied);
            UiObjectUtility.SetActive(
                slot.MoreInfoButton != null ? slot.MoreInfoButton.gameObject : null,
                isOccupied && !isArtifactAcquisition);

            if (slot.Button != null)
            {
                var baseInteractable = isArtifactAcquisition
                    ? canAcquireArtifact
                    : isOccupied || isNextManifestSlot;
                if (actionMode == PrisonActionMode.OfferingOnly)
                {
                    baseInteractable = isOccupied && string.Equals(monsterName, "eve", StringComparison.OrdinalIgnoreCase);
                }
                else if (actionMode == PrisonActionMode.ManifestOnly)
                {
                    baseInteractable = isNextManifestSlot;
                }
                else if (actionMode == PrisonActionMode.ArtifactRecipient)
                {
                    baseInteractable = isArtifactAcquisition && canAcquireArtifact;
                }

                slot.Button.interactable = baseInteractable;
            }

            if (slot.MoreInfoButton != null)
            {
                slot.MoreInfoButton.interactable = isOccupied && !isArtifactAcquisition;
            }

            if (!isOccupied)
            {
                return;
            }

            var monster = GameDataLoader.CurrentCatalog.GetMonster(monsterName);
            if (slot.NameText != null)
            {
                slot.NameText.text = monster != null && !string.IsNullOrWhiteSpace(monster.DisplayName)
                    ? monster.DisplayName
                    : monsterName;
            }

            if (slot.Image != null)
            {
                var portrait = monster != null ? monster.Image : null;
                slot.Image.sprite = portrait;
                slot.Image.color = portrait != null ? Color.white : new Color(0f, 0f, 0f, 0.3f);
            }
        }

        private void OpenCharacterInfo(int slotIndex)
        {
            if (characterInfoPopupUI == null || unitSpawnManager == null)
            {
                return;
            }

            var entry = unitSpawnManager.FindPlayerMonsterBySlot(slotIndex);
            if (entry?.Model == null)
            {
                return;
            }

            characterInfoPopupUI.Open(entry.Model);
        }

        private void RefreshSelectedPrisoner()
        {
            var activePrisonerButton = uiManager?.ActivePrisonerButton;
            var prisonerName = activePrisonerButton != null ? activePrisonerButton.PrisonerName : string.Empty;
            var hasPrisoner = !string.IsNullOrWhiteSpace(prisonerName);
            UiObjectUtility.SetActive(prisonerImage != null ? prisonerImage.gameObject : null, hasPrisoner);
            if (!hasPrisoner)
            {
                return;
            }

            if (prisonerNameText != null)
            {
                prisonerNameText.text = uiManager.ResolvePrisonerDisplayName(prisonerName);
            }

            var enemy = GameDataLoader.CurrentCatalog.GetData<EnemyDefinition>(prisonerName);
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
            prisonPanel = gameObject;
            prisonerArea = UiBindingUtility.BindChildObject(
                this,
                transform,
                "Prisonal",
                nameof(prisonerArea),
                ref valid);
            prisonerImage = UiBindingUtility.BindChild<Image>(
                this,
                "Prisonal/Image",
                nameof(prisonerImage),
                ref valid);
            prisonerNameText = UiBindingUtility.BindChild<TMP_Text>(
                this,
                "Prisonal/Image/Name",
                nameof(prisonerNameText),
                ref valid);
            offeringUI = UiBindingUtility.BindSceneComponent<OfferingUI>(
                this,
                nameof(offeringUI),
                ref valid);
            menifestUI = UiBindingUtility.BindSceneComponent<MenifestUI>(
                this,
                nameof(menifestUI),
                ref valid);
            uiManager = UiBindingUtility.BindSceneComponent<InGameUIManager>(
                this,
                nameof(uiManager),
                ref valid);
            unitSpawnManager = UiBindingUtility.BindSceneComponent<UnitSpawnManager>(
                this,
                nameof(unitSpawnManager),
                ref valid);
            characterInfoPopupUI = UiBindingUtility.BindSceneComponent<CharacterInfoPopupUI>(
                this,
                nameof(characterInfoPopupUI),
                ref valid);

            if (prisonPartySlots == null || prisonPartySlots.Length != PrisonPartySlotCount)
            {
                prisonPartySlots = new PrisonPartySlotView[PrisonPartySlotCount];
            }

            for (var i = 0; i < prisonPartySlots.Length; i++)
            {
                prisonPartySlots[i] = new PrisonPartySlotView();
                prisonPartySlots[i].BindObject(this, transform, $"{i + 1}P", i, ref valid);
            }

            referencesBound = valid;
            bindingFailed = !valid;
            return valid;
        }

        [Serializable]
        private sealed class PrisonPartySlotView
        {
            private Image image;
            private TMP_Text nameText;
            private Button button;
            private Button moreInfoButton;
            private GameObject reinforcementLabel;
            private GameObject manifestedLabel;
            private GameObject acquisitionLabel;

            internal void BindObject(
                Component owner,
                Transform root,
                string slotPath,
                int slotIndex,
                ref bool valid)
            {
                var slotRoot = root != null ? root.Find(slotPath) : null;
                if (slotRoot == null)
                {
                    Debug.LogError(
                        $"{owner.GetType().Name} BindObject failed: field 'prisonPartySlots[{slotIndex}]' at path '{slotPath}' requires a slot object.",
                        owner);
                    valid = false;
                    return;
                }

                image = UiBindingUtility.BindChild<Image>(
                    owner,
                    slotRoot,
                    "Image",
                    $"prisonPartySlots[{slotIndex}].image",
                    ref valid);
                nameText = UiBindingUtility.BindChild<TMP_Text>(
                    owner,
                    slotRoot,
                    "Image/Name",
                    $"prisonPartySlots[{slotIndex}].nameText",
                    ref valid);
                button = UiBindingUtility.BindChild<Button>(
                    owner,
                    slotRoot,
                    "Button",
                    $"prisonPartySlots[{slotIndex}].button",
                    ref valid);
                moreInfoButton = UiBindingUtility.BindChild<Button>(
                    owner,
                    slotRoot,
                    "Image/MoreInfo",
                    $"prisonPartySlots[{slotIndex}].moreInfoButton",
                    ref valid);
                reinforcementLabel = UiBindingUtility.BindChildObject(
                    owner,
                    slotRoot,
                    "Button/Reinforcement",
                    $"prisonPartySlots[{slotIndex}].reinforcementLabel",
                    ref valid);
                manifestedLabel = UiBindingUtility.BindChildObject(
                    owner,
                    slotRoot,
                    "Button/Menifested",
                    $"prisonPartySlots[{slotIndex}].manifestedLabel",
                    ref valid);
                acquisitionLabel = UiBindingUtility.BindChildObject(
                    owner,
                    slotRoot,
                    "Button/Acquisition",
                    $"prisonPartySlots[{slotIndex}].acquisitionLabel",
                    ref valid);
            }

            public Image Image => image;
            public TMP_Text NameText => nameText;
            public Button Button => button;
            public Button MoreInfoButton => moreInfoButton;
            public GameObject ReinforcementLabel => reinforcementLabel;
            public GameObject MenifestedLabel => manifestedLabel;
            public GameObject AcquisitionLabel => acquisitionLabel;
        }
    }
}
