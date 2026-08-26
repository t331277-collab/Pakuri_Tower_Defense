using System;
using Pakuri.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Pakuri.InGame
{
    /// CSV 대사 한 블록의 타이핑 출력과 Skip/Next 입력만 담당한다.
    public sealed class TutorialLineView : MonoBehaviour
    {
        private const float CharacterInterval = 0.03f;

        private GameObject lineRoot;
        private TextMeshProUGUI lineText;
        private TextMeshProUGUI buttonText;
        private Button skipButton;
        private string currentLineId;
        private int visibleCharacterCount;
        private int totalCharacterCount;
        private float elapsed;
        private bool typing;

        public event Action<string> NextRequested;
        public bool IsVisible => lineRoot != null && lineRoot.activeSelf;

        public bool Initialize(Transform tutorialRoot)
        {
            var line = tutorialRoot != null ? tutorialRoot.Find("TutoLine") : null;
            var text = line != null ? line.Find("LinePanel/Text (TMP)") : null;
            var button = line != null ? line.Find("SkipBtn") : null;
            var label = button != null ? button.Find("Text (TMP)") : null;
            var image = button != null ? button.GetComponent<Image>() : null;
            if (line == null || text == null || button == null || label == null || image == null)
            {
                Debug.LogError("TutorialLineView requires TutoLine, LinePanel/Text (TMP), SkipBtn/Image, and SkipBtn/Text (TMP).", this);
                return false;
            }

            lineRoot = line.gameObject;
            lineText = text.GetComponent<TextMeshProUGUI>();
            buttonText = label.GetComponent<TextMeshProUGUI>();
            skipButton = button.GetComponent<Button>() ?? button.gameObject.AddComponent<Button>();
            if (lineText == null || buttonText == null)
            {
                Debug.LogError("TutorialLineView requires TextMeshProUGUI components for dialogue and button labels.", this);
                return false;
            }

            skipButton.targetGraphic = image;
            skipButton.onClick.AddListener(HandleButtonClicked);
            Hide();
            return true;
        }

        public void Show(TutorialLineDefinition line)
        {
            if (line == null || lineRoot == null)
            {
                return;
            }

            currentLineId = line.LineId;
            lineText.text = line.Text;
            lineText.maxVisibleCharacters = 0;
            lineRoot.SetActive(true);
            lineText.ForceMeshUpdate();
            totalCharacterCount = lineText.textInfo.characterCount;
            visibleCharacterCount = 0;
            elapsed = 0f;
            typing = totalCharacterCount > 0;
            skipButton.interactable = true;
            SetButtonLabel(typing ? "SKIP!" : "Next!");
        }

        public void Hide()
        {
            typing = false;
            currentLineId = string.Empty;
            if (lineRoot != null)
            {
                lineRoot.SetActive(false);
            }
        }

        private void Update()
        {
            if (!typing)
            {
                return;
            }

            elapsed += Time.unscaledDeltaTime;
            var nextCount = Mathf.Min(totalCharacterCount, Mathf.FloorToInt(elapsed / CharacterInterval));
            if (nextCount == visibleCharacterCount)
            {
                return;
            }

            visibleCharacterCount = nextCount;
            lineText.maxVisibleCharacters = visibleCharacterCount;
            if (visibleCharacterCount >= totalCharacterCount)
            {
                CompleteTyping();
            }
        }

        private void HandleButtonClicked()
        {
            if (typing)
            {
                lineText.maxVisibleCharacters = totalCharacterCount;
                CompleteTyping();
                return;
            }

            skipButton.interactable = false;
            var completedLineId = currentLineId;
            Hide();
            NextRequested?.Invoke(completedLineId);
        }

        private void CompleteTyping()
        {
            typing = false;
            lineText.maxVisibleCharacters = int.MaxValue;
            SetButtonLabel("Next!");
        }

        private void SetButtonLabel(string label)
        {
            if (buttonText != null)
            {
                buttonText.text = label;
            }
        }

        private void OnDestroy()
        {
            if (skipButton != null)
            {
                skipButton.onClick.RemoveListener(HandleButtonClicked);
            }
        }
    }
}
