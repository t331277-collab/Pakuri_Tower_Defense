using UnityEngine;
using UnityEngine.UI;

namespace Pakuri.InGame
{
    internal enum RewardKind
    {
        Prisoner,
        Gold,
        DarkTrace
    }

    internal sealed class RewardButtonView
    {
        private readonly Color originalColor;

        public RewardButtonView(Button button, RewardKind kind, int amount, string prisonerId)
        {
            Button = button;
            Kind = kind;
            Amount = amount;
            PrisonerId = prisonerId;
            originalColor = button != null && button.image != null ? button.image.color : Color.white;
        }

        public Button Button { get; }
        public RewardKind Kind { get; }
        public int Amount { get; }
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
