using System;

/*
 * 전투 계산에 사용하는 최대 체력, 공격력, 이동 속도와 치명타 수치를 보관한다.
 */
namespace Pakuri.InGame
{
    [Serializable]
    public sealed class UnitCombatStats
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
