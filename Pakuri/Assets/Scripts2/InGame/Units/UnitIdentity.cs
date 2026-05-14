using System;

namespace Pakuri.InGame
{
    public enum UnitSide
    {
        Player,
        Enemy
    }

    public enum UnitRole
    {
        Monster,
        Enemy,
        Summon
    }

    [Serializable]
    public sealed class UnitIdentity
    {
        public string UnitId;
        public string DefinitionId;
        public string DisplayName;
        public UnitSide Side;
        public UnitRole Role;
        public int SlotIndex;
    }
}
