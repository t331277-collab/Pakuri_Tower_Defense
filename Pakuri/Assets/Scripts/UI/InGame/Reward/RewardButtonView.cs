using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Pakuri.InGame
{
    internal sealed class RewardButtonView
    {
        private readonly Color originalColor;

        public RewardButtonView(Button button, TMP_Text summary, TMP_Text what, string prisonerId)
        {
            Button = button;
            Summary = summary;
            What = what;
            PrisonerId = prisonerId;
            originalColor = button != null && button.image != null ? button.image.color : Color.white;
        }

        public Button Button { get; }
        public TMP_Text Summary { get; }
        public TMP_Text What { get; }
        public string PrisonerId { get; private set; }
        public bool Consumed { get; private set; }

        public void SetDisplay(string summary, string what, string prisonerId)
        {
            if (Summary != null)
            {
                Summary.text = summary;
            }

            if (What != null)
            {
                What.text = what;
            }

            PrisonerId = prisonerId;
        }

        public void Reset()
        {
            Consumed = false;
            PrisonerId = string.Empty;
            if (Summary != null)
            {
                Summary.text = string.Empty;
            }

            if (What != null)
            {
                What.text = string.Empty;
            }

            if (Button == null)
            {
                return;
            }

            Button.interactable = true;
            Button.onClick.RemoveAllListeners();
            if (Button.image != null)
            {
                Button.image.color = originalColor;
            }
        }

        public void SetConsumed()
        {
            Consumed = true;
            if (Button == null)
            {
                return;
            }

            Button.interactable = false;
            if (Button.image != null)
            {
                Button.image.color = Color.Lerp(originalColor, Color.black, 0.55f);
            }
        }
    }
}
