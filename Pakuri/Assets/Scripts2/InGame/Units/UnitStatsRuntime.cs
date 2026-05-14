using System;

namespace Pakuri.InGame
{
    [Serializable]
    public sealed class UnitStatsRuntime
    {
        public float MaxHealth;
        public float AttackPower;
        public float SpellPower;
        public float MoveSpeed;
        public float CriticalChance;
        public float CriticalDamage;
        public float CriticalResistance;
    }
}
