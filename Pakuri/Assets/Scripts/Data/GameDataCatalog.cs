using System;
using UnityEngine;

namespace Pakuri.Data
{
    [CreateAssetMenu(menuName = "Pakuri/Game Data Catalog", fileName = "GameDataCatalog")]
    public class GameDataCatalog : ScriptableObject
    {
        public MonsterDefinition[] Monsters = Array.Empty<MonsterDefinition>();

        public MonsterDefinition GetMonsterById(string monsterId)
        {
            if (string.IsNullOrWhiteSpace(monsterId) || Monsters == null)
            {
                return null;
            }

            for (var i = 0; i < Monsters.Length; i++)
            {
                var monster = Monsters[i];
                if (monster == null)
                {
                    continue;
                }

                if (string.Equals(monster.MonsterId, monsterId, StringComparison.OrdinalIgnoreCase))
                {
                    return monster;
                }
            }

            return null;
        }
    }
}
