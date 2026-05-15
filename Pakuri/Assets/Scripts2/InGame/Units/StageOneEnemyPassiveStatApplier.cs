using Pakuri.Data;

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

            switch (enemy.StageOneSkill)
            {
                case StageOneEnemySkillKind.Slash:
                    enemy.PassiveOutgoingDamageMultiplier *= 1.10f;
                    break;
                case StageOneEnemySkillKind.ShieldUp:
                    MultiplyDefenses(enemy.Defenses, 1.10f);
                    break;
                case StageOneEnemySkillKind.AimedShot:
                    if (enemy.Stats != null)
                    {
                        enemy.Stats.CriticalChance += 0.08f;
                    }

                    break;
                case StageOneEnemySkillKind.ShurikenThrow:
                    if (enemy.Stats != null)
                    {
                        enemy.Stats.CriticalDamage += 0.20f;
                    }

                    break;
                case StageOneEnemySkillKind.Heal:
                    enemy.PassiveHealingMultiplier *= 1.15f;
                    break;
                case StageOneEnemySkillKind.GuardianFlag:
                    enemy.PassiveIncomingDamageMultiplier *= 0.88f;
                    break;
                case StageOneEnemySkillKind.ChargeCommand:
                    enemy.PassiveOutgoingDamageMultiplier *= 1.12f;
                    break;
                case StageOneEnemySkillKind.SacredSwordWave:
                    enemy.PassiveOutgoingDamageMultiplier *= 1.15f;
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
