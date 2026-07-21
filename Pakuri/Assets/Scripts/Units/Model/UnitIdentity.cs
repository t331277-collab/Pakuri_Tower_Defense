using System;

/*
 * 전투 유닛의 소속, 역할, 데이터 ID와 전투 내 고유 ID를 보관한다.
 */
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
        Summon,
        Nexus
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
