using UnityEngine;
using UnityEngine.UI;

namespace Pakuri.InGame
{
    internal sealed class RewardButtonView
    {
        private readonly Color originalColor;

        public RewardButtonView(Button button, string prisonerId)
        {
            Button = button;
            PrisonerId = prisonerId;
            originalColor = button != null && button.image != null ? button.image.color : Color.white;
        }

        public Button Button { get; }
        public string PrisonerId { get; }
        public bool Consumed { get; private set; }

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
