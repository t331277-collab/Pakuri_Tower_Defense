using System;
using UnityEngine;

namespace Pakuri.Data
{
    [CreateAssetMenu(menuName = "Pakuri/Game Data Catalog", fileName = "GameDataCatalog")]
    public class GameDataCatalog : ScriptableObject
    {
        public MonsterDefinition[] Monsters = Array.Empty<MonsterDefinition>();
        public EnemyDefinition[] StageOneEnemies = Array.Empty<EnemyDefinition>();

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

        public EnemyDefinition GetStageOneEnemyById(string enemyId)
        {
            if (string.IsNullOrWhiteSpace(enemyId) || StageOneEnemies == null)
            {
                return null;
            }

            for (var i = 0; i < StageOneEnemies.Length; i++)
            {
                var enemy = StageOneEnemies[i];
                if (enemy == null)
                {
                    continue;
                }

                if (string.Equals(enemy.EnemyId, enemyId, StringComparison.OrdinalIgnoreCase))
                {
                    return enemy;
                }
            }

            return null;
        }
    }
}
