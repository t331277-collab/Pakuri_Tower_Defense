using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

#pragma warning disable 0649

namespace Pakuri.InGame
{
    [Serializable]
    internal sealed class InGameUIReferences
    {
        public InGameRewardPanelReferences reward = new InGameRewardPanelReferences();
        public InGamePrisonPanelReferences prison = new InGamePrisonPanelReferences();
        public InGameOfferingReferences offering = new InGameOfferingReferences();
        public InGameMenifestReferences menifest = new InGameMenifestReferences();
        public InGameInfoReferences info = new InGameInfoReferences();
    }

    [Serializable]
    internal sealed class InGameRewardPanelReferences
    {
        public GameObject rewardPanel;
        public Transform rewardButtonContainer;
        public Button prisonerTemplateButton;
        public Button goldTemplateButton;
        public Button darkTemplateButton;
        public Button nextButton;
        public TMP_Text rewardSummaryText;
    }

    [Serializable]
    internal sealed class InGamePrisonPanelReferences
    {
        public GameObject prisonPanel;
        public GameObject prisonerChoicePopUp;
        public Image prisonerImage;
        public TMP_Text prisonerNameText;
        public InGamePrisonPartySlotReferences partySlot1 = new InGamePrisonPartySlotReferences();
        public InGamePrisonPartySlotReferences partySlot2 = new InGamePrisonPartySlotReferences();
        public InGamePrisonPartySlotReferences partySlot3 = new InGamePrisonPartySlotReferences();
        public InGamePrisonPartySlotReferences partySlot4 = new InGamePrisonPartySlotReferences();
        public InGamePrisonPartySlotReferences partySlot5 = new InGamePrisonPartySlotReferences();
    }

    [Serializable]
    internal sealed class InGamePrisonPartySlotReferences
    {
        public Image image;
        public TMP_Text nameText;
        public Button button;
        public GameObject reinforcementLabel;
        public GameObject manifestedLabel;
    }

    [Serializable]
    internal sealed class InGameOfferingReferences
    {
        public GameObject offeringPanel;
        public InGameOfferingChoiceReferences choice1 = new InGameOfferingChoiceReferences();
        public InGameOfferingChoiceReferences choice2 = new InGameOfferingChoiceReferences();
        public InGameOfferingChoiceReferences choice3 = new InGameOfferingChoiceReferences();
    }

    [Serializable]
    internal sealed class InGameOfferingChoiceReferences
    {
        public Button button;
        public TMP_Text summaryLabel;
        public TMP_Text skillNameLabel;
        public TMP_Text titleLabel;
        public TMP_Text descriptionLabel;
        public Image iconImage;
        public GameObject popUp;
        public TMP_Text popUpText;
    }

    [Serializable]
    internal sealed class InGameMenifestReferences
    {
        public GameObject failPopUp;
        public Button failBackButton;
        public GameObject successPopUp;
        public Button dontChoiceButton;
        public Button choiceButton;
        public TMP_Text monsterNameText;
        public TMP_Text monsterDescText;
        public Image monsterImage;
    }

    [Serializable]
    internal sealed class InGameInfoReferences
    {
        public TMP_Text stageInfoText;
        public TMP_Text goldInfoText;
        public TMP_Text darkInfoText;
        public TMP_Text prisonStageInfoText;
        public TMP_Text prisonGoldInfoText;
        public TMP_Text prisonDarkInfoText;
    }
}

#pragma warning restore 0649
