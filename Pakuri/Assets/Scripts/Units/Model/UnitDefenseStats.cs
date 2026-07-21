using System;
using Pakuri.Combat;

/*
 * 유닛의 속성별 방어력을 보관하고 해당 속성의 방어력을 반환하는 데이터.
 */
namespace Pakuri.InGame
{
    [Serializable]
    public sealed class UnitDefenseStats
    {
        public float Physical;
        public float Fire;
        public float Lightning;
        public float Ice;
        public float Darkness;
        public float Holy;

        public float Get(DamageAttribute attribute)
        {
            switch (attribute)
            {
                case DamageAttribute.Fire:
                    return Fire;
                case DamageAttribute.Lightning:
                    return Lightning;
                case DamageAttribute.Ice:
                    return Ice;
                case DamageAttribute.Darkness:
                    return Darkness;
                case DamageAttribute.Holy:
                    return Holy;
                default:
                    return Physical;
            }
        }

    }
}
