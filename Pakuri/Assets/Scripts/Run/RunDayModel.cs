using System;

namespace Pakuri.Run
{
    public enum RunCombatType
    {
        Normal,
        Elite,
        Day5Midboss,
        Day10Midboss,
        Boss,
        Shop
    }

    [Serializable]
    public readonly struct RunDayModel
    {
        public RunDayModel(int stageIndex, int dayIndex, RunCombatType combatType, bool hasEliteOption, bool hasShopOption)
        {
            StageIndex = Math.Max(1, Math.Min(stageIndex, 4));
            DayIndex = Math.Max(1, Math.Min(dayIndex, 11));
            CombatType = combatType;
            HasEliteOption = hasEliteOption;
            HasShopOption = hasShopOption;
        }

        public int StageIndex { get; }
        public int DayIndex { get; }
        public RunCombatType CombatType { get; }
        public bool HasEliteOption { get; }
        public bool HasShopOption { get; }

        public static RunDayModel Resolve(int stageIndex, int dayIndex)
        {
            var clampedDay = Math.Max(1, Math.Min(dayIndex, 11));
            if (clampedDay == 5)
            {
                return new RunDayModel(stageIndex, clampedDay, RunCombatType.Day5Midboss, false, false);
            }

            if (clampedDay == 10)
            {
                return new RunDayModel(stageIndex, clampedDay, RunCombatType.Day10Midboss, false, false);
            }

            if (clampedDay == 11)
            {
                return new RunDayModel(stageIndex, clampedDay, RunCombatType.Boss, false, false);
            }

            var canOfferElite = clampedDay >= 2 && clampedDay <= 4 || clampedDay >= 6 && clampedDay <= 9;
            var canOfferShop = clampedDay >= 6 && clampedDay <= 9;
            return new RunDayModel(stageIndex, clampedDay, RunCombatType.Normal, canOfferElite, canOfferShop);
        }
    }
}
