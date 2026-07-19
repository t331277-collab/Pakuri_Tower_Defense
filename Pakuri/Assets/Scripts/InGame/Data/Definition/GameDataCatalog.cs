using System;
using UnityEngine;

namespace Pakuri.Data
{
    /*
     * 런타임에서 사용하는 몬스터, 적, 상태 정의를 한곳에 모아 제공한다.
     */
    public class GameDataCatalog : ScriptableObject
    {
        public MonsterDefinition[] Monsters = Array.Empty<MonsterDefinition>();
        public EnemyDefinition[] StageOneEnemies = Array.Empty<EnemyDefinition>();
        public EnemyDefinition[] StageTwoEnemies = Array.Empty<EnemyDefinition>();
        public StatusEffectDefinitionData[] StatusEffects = Array.Empty<StatusEffectDefinitionData>();

        /*
         * ID가 일치하는 몬스터 정의를 찾는다.
         */
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

        /*
         * 1스테이지를 먼저 확인하고 없으면 2스테이지에서 적 정의를 찾는다.
         */
        public EnemyDefinition GetEnemyById(string enemyId)
        {
            var enemy = GetEnemyById(enemyId, StageOneEnemies);
            // 같은 ID가 두 목록에 있으면 1스테이지 정의를 우선한다.
            return enemy != null ? enemy : GetEnemyById(enemyId, StageTwoEnemies);
        }

        /*
         * 지정한 적 목록에서 ID가 일치하는 정의를 찾는다.
         */
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

        /*
         * ID가 일치하는 상태 효과 정의를 찾는다.
         */
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
