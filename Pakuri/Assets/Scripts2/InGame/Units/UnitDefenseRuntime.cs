using System;
using Pakuri.Combat;

namespace Pakuri.InGame
{
    [Serializable]
    public sealed class UnitDefenseRuntime
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

        public static UnitDefenseRuntime FromDefinition(AttributeDefenseSet source)
        {
            if (source == null)
            {
                return new UnitDefenseRuntime();
            }

            return new UnitDefenseRuntime
            {
                Physical = source.Physical,
                Fire = source.Fire,
                Lightning = source.Lightning,
                Ice = source.Ice,
                Darkness = source.Darkness,
                Holy = source.Holy
            };
        }
    }
}
