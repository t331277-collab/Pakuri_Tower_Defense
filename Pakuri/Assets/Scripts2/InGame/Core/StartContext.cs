namespace Pakuri.InGame
{
    public static class StartContext
    {
        public static string SelectedMonsterId { get; private set; }
        public static bool HasPendingRun => !string.IsNullOrWhiteSpace(SelectedMonsterId);

        public static void Prepare(string selectedMonsterId)
        {
            SelectedMonsterId = string.IsNullOrWhiteSpace(selectedMonsterId) ? string.Empty : selectedMonsterId;
        }

        public static void Clear()
        {
            SelectedMonsterId = string.Empty;
        }
    }
}
