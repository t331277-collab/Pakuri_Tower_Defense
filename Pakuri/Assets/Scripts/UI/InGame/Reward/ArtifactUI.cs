using System;
using System.Collections.Generic;
using Pakuri.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Pakuri.InGame
{
    /// 보스 보상의 정령계약·처형관 유물 후보를 준비하고 선택 결과를 획득 대상 UI로 전달한다.
    public sealed class ArtifactUI : MonoBehaviour
    {
        private const int MaxArtifactChoices = 3;

        private readonly List<ArtifactDefinition> choices = new List<ArtifactDefinition>();
        private ArtifactButtonView[] buttonViews = new ArtifactButtonView[MaxArtifactChoices];
        private InGameUIManager uiManager;
        private bool referencesBound;
        private bool bindingFailed;

        private void Awake()
        {
            if (!BindObject())
            {
                enabled = false;
            }
        }

        public int PrepareChoices(RunSession session, int requestedCount)
        {
            choices.Clear();
            if (session == null || requestedCount <= 0 || !session.HasArtifactCapacity())
            {
                return 0;
            }

            var artifacts = GameDataLoader.CurrentCatalog.Artifacts;
            for (var i = 0; i < artifacts.Length; i++)
            {
                var artifact = artifacts[i];
                if (artifact != null
                    && (string.Equals(artifact.SynergyName, "spirit-contract", StringComparison.OrdinalIgnoreCase)
                        || string.Equals(artifact.SynergyName, "executioner", StringComparison.OrdinalIgnoreCase))
                    && !session.HasArtifact(artifact.ArtifactName))
                {
                    choices.Add(artifact);
                }
            }

            for (var i = choices.Count - 1; i > 0; i--)
            {
                var swapIndex = UnityEngine.Random.Range(0, i + 1);
                var swap = choices[i];
                choices[i] = choices[swapIndex];
                choices[swapIndex] = swap;
            }

            var limit = Math.Min(Math.Min(requestedCount, MaxArtifactChoices), choices.Count);
            if (choices.Count > limit)
            {
                choices.RemoveRange(limit, choices.Count - limit);
            }

            return choices.Count;
        }

        public bool OpenPreparedChoices()
        {
            if (choices.Count == 0 || !BindObject())
            {
                return false;
            }

            for (var i = 0; i < buttonViews.Length; i++)
            {
                var view = buttonViews[i];
                var hasChoice = i < choices.Count;
                view.Button.gameObject.SetActive(hasChoice);
                view.Button.interactable = hasChoice;
                view.Button.onClick.RemoveAllListeners();
                if (!hasChoice)
                {
                    continue;
                }

                var capturedIndex = i;
                BindChoice(view, choices[i]);
                view.Button.onClick.AddListener(() => SelectArtifact(capturedIndex));
            }

            gameObject.SetActive(true);
            return true;
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void Clear()
        {
            choices.Clear();
            Hide();
        }

        private void SelectArtifact(int choiceIndex)
        {
            if (choiceIndex < 0 || choiceIndex >= choices.Count)
            {
                return;
            }

            var artifactName = choices[choiceIndex].ArtifactName;
            Hide();
            uiManager?.OpenArtifactAcquisition(artifactName);
        }

        private static void BindChoice(ArtifactButtonView view, ArtifactDefinition artifact)
        {
            var synergy = GameDataLoader.CurrentCatalog.GetData<ArtifactSynergyDefinition>(artifact.SynergyName);
            view.Summary.text = synergy != null ? synergy.DisplayName : artifact.SynergyName;
            view.ArtifactName.text = artifact.DisplayName;
            view.Description.text = artifact.Description;
            view.Icon.sprite = artifact.Icon;
            view.Icon.enabled = artifact.Icon != null;
            view.Icon.gameObject.SetActive(artifact.Icon != null);
            if (view.PopUp != null)
            {
                view.PopUp.SetActive(false);
            }
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
            uiManager = UiBindingUtility.BindSceneComponent<InGameUIManager>(
                this,
                nameof(uiManager),
                ref valid);

            for (var i = 0; i < buttonViews.Length; i++)
            {
                buttonViews[i] = new ArtifactButtonView();
                buttonViews[i].BindObject(this, transform, $"Choice{i + 1}", i, ref valid);
            }

            referencesBound = valid;
            bindingFailed = !valid;
            return valid;
        }

        private sealed class ArtifactButtonView
        {
            public Button Button;
            public TMP_Text Summary;
            public TMP_Text ArtifactName;
            public TMP_Text Description;
            public Image Icon;
            public GameObject PopUp;

            public void BindObject(Component owner, Transform root, string path, int index, ref bool valid)
            {
                var choiceRoot = root != null ? root.Find(path) : null;
                if (choiceRoot == null)
                {
                    Debug.LogError($"ArtifactUI requires choice object '{path}'.", owner);
                    valid = false;
                    return;
                }

                Button = UiBindingUtility.BindSelf<Button>(owner, choiceRoot, $"buttonViews[{index}].Button", ref valid);
                Summary = UiBindingUtility.BindChild<TMP_Text>(owner, choiceRoot, "Summary", $"buttonViews[{index}].Summary", ref valid);
                ArtifactName = UiBindingUtility.BindChild<TMP_Text>(owner, choiceRoot, "ArtifactName", $"buttonViews[{index}].ArtifactName", ref valid);
                Description = UiBindingUtility.BindChild<TMP_Text>(owner, choiceRoot, "Desc", $"buttonViews[{index}].Description", ref valid);
                Icon = UiBindingUtility.BindChild<Image>(owner, choiceRoot, "Icon", $"buttonViews[{index}].Icon", ref valid);
                var popUp = choiceRoot.Find("PopUP");
                PopUp = popUp != null ? popUp.gameObject : null;
            }
        }
    }
}
