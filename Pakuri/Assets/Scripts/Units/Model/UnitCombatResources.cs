using System;

/*
 * 전투 중 변하는 현재 체력과 직접·상태 보호막 합계를 보관한다.
 */
namespace Pakuri.InGame
{
    [Serializable]
    public sealed class UnitCombatResources
    {
        public float CurrentHealth;
        public float CurrentShield;
        public float DirectShield;
    }
}
