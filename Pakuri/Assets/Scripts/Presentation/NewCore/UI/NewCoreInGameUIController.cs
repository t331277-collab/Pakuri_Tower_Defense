using System;
using System.Collections.Generic;
using Pakuri.NewCore.Definitions.Units;
using Pakuri.NewCore.Presentation.Scene;
using Pakuri.NewCore.Run;
using Pakuri.NewCore.Run.Services;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Pakuri.NewCore.Presentation.UI
{
    public sealed class NewCoreInGameUIController : MonoBehaviour
    {
        private const int PartySlots = 5;

        [SerializeField] private NewCoreStageController stageManager;
        [SerializeField] private NewCoreSpawnController unitSpawnManager;
        [SerializeField] private NewCoreSceneRuntime combatManager;
        [SerializeField] private Sprite arielPrisonPortrait;
        [SerializeField] private Sprite evePrisonPortrait;
        [SerializeField] private Sprite rinPrisonPortrait;
        [SerializeField] private Sprite seinPrisonPortrait;
        [SerializeField] private Sprite vegaPrisonPortrait;
        [SerializeField] private Vector2 rewardButtonFirstColumnPosition =
            new Vector2(-321.97855f, 295f);
        [SerializeField] private float rewardButtonColumnSpacingX =
            533.97855f;
        [SerializeField] private float rewardButtonRowSpacingY = 122f;
        [SerializeField] private int rewardButtonRowsPerColumn = 3;

        private readonly List<Button> prisonerButtons = new List<Button>();
        private readonly Button[] partyButtons = new Button[PartySlots];
        private readonly Image[] partyImages = new Image[PartySlots];
        private readonly TMP_Text[] partyNames = new TMP_Text[PartySlots];
        private readonly Button[] offeringButtons = new Button[3];
        private GameObject rewardPanel;
        private Transform rewardContainer;
        private Button prisonerTemplate;
        private Button darkTemplate;
        private Button goldTemplate;
        private Button nextButton;
        private TMP_Text rewardSummary;
        private GameObject prisonPanel;
        private GameObject prisonerChoicePopup;
        private Image prisonerImage;
        private TMP_Text prisonerName;
        private GameObject offeringPanel;
        private GameObject manifestFailurePopup;
        private GameObject manifestSuccessPopup;
        private TMP_Text manifestName;
        private TMP_Text manifestDescription;
        private Image manifestImage;
        private TMP_Text stageInfo;
        private TMP_Text goldInfo;
        private TMP_Text darkInfo;
        private Prisoner activePrisoner;
        private OfferingOffer activeOffer;

        private void Awake()
        {
            ResolveRuntime();
            ResolveSceneUi();
            Bind(nextButton, ContinueRun);
            ResolvePartyButtons();
            ResolveManifestButtons();
            SetActive(rewardPanel, false);
            SetActive(prisonPanel, false);
            SetActive(prisonerChoicePopup, false);
            SetActive(offeringPanel, false);
            SetActive(manifestFailurePopup, false);
            SetActive(manifestSuccessPopup, false);
            RefreshInfo();
        }

        private void Update()
        {
            RefreshInfo();
        }

        public void ShowReward(RewardResult reward)
        {
            if (reward == null)
            {
                throw new ArgumentNullException(nameof(reward));
            }

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
            var prisoners =
                combatManager.Stage.Session.PrisonerInventory.Prisoners;
            for (var index = 0; index < prisoners.Count; index++)
            {
                var button = index == 0
                    ? prisonerTemplate
                    : Instantiate(prisonerTemplate, rewardContainer);
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
            RefreshInfo();
        }

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
                    () => OpenPrisoner(prisoner, button));
            }
        }

        private void OpenPrisoner(
            Prisoner prisoner,
            Button sourceButton)
        {
            if (!combatManager.Stage.Session.PrisonerInventory.CanConsume(prisoner))
            {
                sourceButton.interactable = false;
                return;
            }

            activePrisoner = prisoner;
            if (prisonerImage != null)
            {
                prisonerImage.sprite = null;
                prisonerImage.color = new Color(0f, 0f, 0f, 0.3f);
            }

            if (prisonerName != null)
            {
                prisonerName.text = ResolveEnemyName(prisoner.EnemyId);
            }

            RefreshPartySlots();
            SetActive(prisonerChoicePopup, true);
            SetActive(prisonPanel, true);
        }

        private void ResolvePartyButtons()
        {
            for (var index = 0; index < PartySlots; index++)
            {
                var captured = index;
                var path = $"PrisonPanel/{index + 1}P";
                partyImages[index] = Find<Image>(path + "/Image");
                partyNames[index] = Find<TMP_Text>(path + "/Image/Name");
                partyButtons[index] = Find<Button>(path + "/Button");
                Bind(
                    partyButtons[index],
                    () => SelectPartySlot(captured));
            }
        }

        private void RefreshPartySlots()
        {
            var party = combatManager.Stage.Session.PartyRoster.Members;
            for (var index = 0; index < PartySlots; index++)
            {
                var occupied = index < party.Count;
                var availableManifestSlot =
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

                var definition = party[index].MonsterDefinition;
                if (partyNames[index] != null)
                {
                    partyNames[index].text = definition.display_name;
                }

                if (partyImages[index] != null)
                {
                    partyImages[index].sprite =
                        ResolveMonsterPortrait(definition.id);
                }
            }
        }

        private void SelectPartySlot(int slot)
        {
            if (activePrisoner == null)
            {
                return;
            }

            var party = combatManager.Stage.Session.PartyRoster.Members;
            if (slot < party.Count)
            {
                OpenOffering(party[slot]);
            }
            else if (slot == party.Count)
            {
                BeginManifestation();
            }
        }

        private void OpenOffering(
            Units.Models.MonsterModel monster)
        {
            activeOffer = combatManager.Offerings.GenerateCandidates(
                monster,
                activePrisoner);
            if (activeOffer.Candidates.Count == 0)
            {
                return;
            }

            for (var index = 0; index < offeringButtons.Length; index++)
            {
                var button = offeringButtons[index];
                var visible = index < activeOffer.Candidates.Count;
                if (button == null)
                {
                    continue;
                }

                button.gameObject.SetActive(visible);
                button.onClick.RemoveAllListeners();
                if (!visible)
                {
                    continue;
                }

                var candidate = activeOffer.Candidates[index];
                SetLabel(button, ResolveOfferingLabel(candidate));
                button.onClick.AddListener(
                    () => ConfirmOffering(candidate.Id));
            }

            SetActive(prisonPanel, false);
            SetActive(offeringPanel, true);
        }

        private void ConfirmOffering(string candidateId)
        {
            if (!combatManager.Offerings.TryConfirm(candidateId))
            {
                return;
            }

            activePrisoner = null;
            activeOffer = null;
            SetActive(offeringPanel, false);
            SetActive(prisonerChoicePopup, false);
            SetActive(rewardPanel, true);
            RefreshInfo();
        }

        private void BeginManifestation()
        {
            if (combatManager.CurrentReward == null)
            {
                return;
            }

            var result = combatManager.Manifestations.BeginAttempt(
                activePrisoner,
                combatManager.CurrentReward.Definition);
            activePrisoner = null;
            SetActive(prisonPanel, false);
            if (!result.Success)
            {
                SetActive(manifestFailurePopup, true);
                return;
            }

            BindManifestCandidate(result.Candidate);
            SetActive(manifestSuccessPopup, true);
        }

        private void ResolveManifestButtons()
        {
            Bind(
                Find<Button>("MenifestedFailPopUp/Back"),
                FinishManifestFailure);
            Bind(
                Find<Button>("MenifestedSuccessPopUp/DontChoiceBtn"),
                SkipManifest);
            Bind(
                Find<Button>("MenifestedSuccessPopUp/ChoiceBtn"),
                RecruitManifest);
        }

        private void FinishManifestFailure()
        {
            SetActive(manifestFailurePopup, false);
            SetActive(prisonerChoicePopup, false);
            SetActive(rewardPanel, true);
        }

        private void SkipManifest()
        {
            if (combatManager.Manifestations.SkipRecruitment())
            {
                SetActive(manifestSuccessPopup, false);
                SetActive(prisonerChoicePopup, false);
                SetActive(rewardPanel, true);
            }
        }

        private void RecruitManifest()
        {
            var monster =
                combatManager.Manifestations.ConfirmRecruitment();
            combatManager.PresentManifestedMonster(monster);
            SetActive(manifestSuccessPopup, false);
            SetActive(prisonerChoicePopup, false);
            SetActive(rewardPanel, true);
            RefreshInfo();
        }

        private void BindManifestCandidate(
            MonsterDefinition definition)
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
                var portrait = ResolveMonsterPortrait(definition.id);
                manifestImage.sprite = portrait;
                manifestImage.color = portrait != null
                    ? Color.white
                    : new Color(0f, 0f, 0f, 0.3f);
            }
        }

        private void ContinueRun()
        {
            if (combatManager.CompleteRewardAndAdvance())
            {
                SetActive(rewardPanel, false);
                SetActive(prisonerChoicePopup, false);
            }
        }

        private void RefreshInfo()
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
                goldInfo.text = combatManager.Stage.Gold.ToString();
            }

            if (darkInfo != null)
            {
                darkInfo.text = combatManager.Stage.DarkTrace.ToString();
            }
        }

        private void ResolveRuntime()
        {
            if (combatManager == null)
            {
                combatManager = FindFirstObjectByType<NewCoreSceneRuntime>();
            }

            if (stageManager == null)
            {
                stageManager = FindFirstObjectByType<NewCoreStageController>();
            }

            if (unitSpawnManager == null)
            {
                unitSpawnManager = FindFirstObjectByType<NewCoreSpawnController>();
            }
        }

        private void ResolveSceneUi()
        {
            rewardPanel = FindObject("RewardPanel");
            rewardContainer = FindTransform("RewardPanel/RewardBtnContainer");
            prisonerTemplate = Find<Button>(
                "RewardPanel/RewardBtnContainer/PrisonerBtn");
            darkTemplate = Find<Button>(
                "RewardPanel/RewardBtnContainer/DarkBtn");
            goldTemplate = Find<Button>(
                "RewardPanel/RewardBtnContainer/GoldBtn");
            nextButton = Find<Button>("RewardPanel/NextBtn");
            rewardSummary = Find<TMP_Text>("RewardPanel/Summary");
            prisonPanel = FindObject("PrisonPanel");
            prisonerChoicePopup = FindObject("PrisonerChoicePopUp");
            prisonerImage = Find<Image>("PrisonPanel/Prisonal/Image");
            prisonerName = Find<TMP_Text>(
                "PrisonPanel/Prisonal/Image/Name");
            offeringPanel = FindObject("OfferingPanel");
            for (var index = 0; index < offeringButtons.Length; index++)
            {
                offeringButtons[index] = Find<Button>(
                    $"OfferingPanel/Choice{index + 1}");
            }

            manifestFailurePopup = FindObject("MenifestedFailPopUp");
            manifestSuccessPopup = FindObject("MenifestedSuccessPopUp");
            manifestName = Find<TMP_Text>(
                "MenifestedSuccessPopUp/MonsterName");
            manifestDescription = Find<TMP_Text>(
                "MenifestedSuccessPopUp/MonsterDesc");
            manifestImage = Find<Image>(
                "MenifestedSuccessPopUp/MonsterImage");
            stageInfo = Find<TMP_Text>("Info/StageInfo");
            goldInfo = Find<TMP_Text>("Info/Goldinfo");
            darkInfo = Find<TMP_Text>("Info/Darkinfo");
        }

        private string ResolveEnemyName(string enemyId)
        {
            var definition = combatManager.Catalog.GetEnemy(enemyId);
            return string.IsNullOrWhiteSpace(definition.display_name)
                ? enemyId
                : definition.display_name;
        }

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

        private static string ResolveOfferingLabel(
            OfferingCandidate candidate)
        {
            if (candidate.Skill != null)
            {
                return string.IsNullOrWhiteSpace(
                    candidate.Skill.display_name)
                        ? candidate.Id
                        : candidate.Skill.display_name;
            }

            return string.IsNullOrWhiteSpace(candidate.Choice.title)
                ? candidate.Id
                : candidate.Choice.title;
        }

        private void Arrange(Button button, int order)
        {
            var rect = button.transform as RectTransform;
            if (rect == null)
            {
                return;
            }

            var rows = Mathf.Max(1, rewardButtonRowsPerColumn);
            var column = order / rows;
            var row = order % rows;
            rect.anchoredPosition = new Vector2(
                rewardButtonFirstColumnPosition.x
                    + rewardButtonColumnSpacingX * column,
                rewardButtonFirstColumnPosition.y
                    - rewardButtonRowSpacingY * row);
        }

        private void ClearPrisonerButtons()
        {
            for (var index = 1; index < prisonerButtons.Count; index++)
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

        private GameObject FindObject(string path)
        {
            var target = FindTransform(path);
            return target != null ? target.gameObject : null;
        }

        private T Find<T>(string path)
            where T : Component
        {
            var target = FindTransform(path);
            return target != null ? target.GetComponent<T>() : null;
        }

        private Transform FindTransform(string path)
        {
            return transform.Find(path);
        }

        private static void SetLabel(Button button, string text)
        {
            var label = button.GetComponentInChildren<TMP_Text>(true);
            if (label != null)
            {
                label.text = text;
            }
        }

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

        private static void SetActive(GameObject target, bool active)
        {
            if (target != null)
            {
                target.SetActive(active);
            }
        }
    }
}
