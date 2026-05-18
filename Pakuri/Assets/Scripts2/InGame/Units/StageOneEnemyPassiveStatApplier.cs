namespace Pakuri.InGame
{
    internal static class StageOneEnemyPassiveStatApplier
    {
        public static void Apply(EnemyUnitRuntimeModel enemy)
        {
            if (enemy == null)
            {
                return;
            }

            var value = System.Math.Max(0f, enemy.PassiveSkillValue);
            if (string.IsNullOrWhiteSpace(enemy.PassiveSkillId) || value <= 0f)
            {
                return;
            }

            switch (enemy.PassiveSkillId.Trim().ToLowerInvariant())
            {
                case "physicaldamageup":
                    enemy.PassivePhysicalDamageMultiplier *= 1f + value;
                    break;
                case "defenseup":
                    MultiplyDefenses(enemy.Defenses, 1f + value);
                    break;
                case "critchanceup":
                    if (enemy.Stats != null)
                    {
                        enemy.Stats.CriticalChance += value;
                    }

                    break;
                case "critdamageup":
                    if (enemy.Stats != null)
                    {
                        enemy.Stats.CriticalDamage += value;
                    }

                    break;
                case "healingup":
                    enemy.PassiveHealingMultiplier *= 1f + value;
                    break;
                case "incomingdamagedown":
                    enemy.PassiveIncomingDamageMultiplier *= System.Math.Max(0f, 1f - value);
                    break;
            }
        }

        private static void MultiplyDefenses(UnitDefenseRuntime defenses, float multiplier)
        {
            if (defenses == null)
            {
                return;
            }

            defenses.Physical *= multiplier;
            defenses.Fire *= multiplier;
            defenses.Lightning *= multiplier;
            defenses.Ice *= multiplier;
            defenses.Darkness *= multiplier;
            defenses.Holy *= multiplier;
        }
    }
}
