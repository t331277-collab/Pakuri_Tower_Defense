using System;
using UnityEngine;

namespace Pakuri.Data
{
    [CreateAssetMenu(menuName = "Pakuri/Game Data Catalog", fileName = "GameDataCatalog")]
    public class GameDataCatalog : ScriptableObject
    {
        public MonsterDefinition[] Monsters = Array.Empty<MonsterDefinition>();
        public EnemyDefinition[] StageOneEnemies = Array.Empty<EnemyDefinition>();
        public EnemyDefinition[] StageTwoEnemies = Array.Empty<EnemyDefinition>();
        public StatusEffectDefinitionData[] StatusEffects = Array.Empty<StatusEffectDefinitionData>();

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

        public EnemyDefinition GetEnemyById(string enemyId)
        {
            var enemy = GetEnemyById(enemyId, StageOneEnemies);
            return enemy != null ? enemy : GetEnemyById(enemyId, StageTwoEnemies);
        }

        private static EnemyDefinition GetEnemyById(string enemyId, EnemyDefinition[] enemies)
        {
            if (string.IsNullOrWhiteSpace(enemyId) || enemies == null)
            {
                return null;
            }

            for (var i = 0; i < enemies.Length; i++)
            {
                var enemy = enemies[i];
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

        public StatusEffectDefinitionData GetStatusEffectById(string statusEffectId)
        {
            if (string.IsNullOrWhiteSpace(statusEffectId) || StatusEffects == null)
            {
                return null;
            }

            for (var i = 0; i < StatusEffects.Length; i++)
            {
                var status = StatusEffects[i];
                if (status == null)
                {
                    continue;
                }

                if (string.Equals(status.StatusEffectId, statusEffectId, StringComparison.OrdinalIgnoreCase))
                {
                    return status;
                }
            }

            return null;
        }
    }
}
