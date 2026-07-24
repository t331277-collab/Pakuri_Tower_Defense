using UnityEngine;

namespace Pakuri.NewCore.Presentation.Assets
{
    [CreateAssetMenu(
        fileName = "RunStartSelection",
        menuName = "Pakuri/New Core/Run Start Selection")]
    public sealed class RunStartSelectionAsset : ScriptableObject
    {
        [SerializeField] private string defaultMonsterId = "eve";

        private string selectedMonsterId;

        public string ConsumeMonsterId()
        {
            var value = string.IsNullOrWhiteSpace(selectedMonsterId)
                ? defaultMonsterId
                : selectedMonsterId;
            selectedMonsterId = string.Empty;
            return value;
        }

        public void Prepare(string monsterId)
        {
            selectedMonsterId = string.IsNullOrWhiteSpace(monsterId)
                ? defaultMonsterId
                : monsterId;
        }
    }
}
