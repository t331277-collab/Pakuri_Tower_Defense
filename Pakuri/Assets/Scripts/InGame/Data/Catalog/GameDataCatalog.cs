using System;
using UnityEngine;

namespace Pakuri.Data
{
    /*
     * 런타임 정의와 ID 조회 표를 한곳에 모아 제공한다.
     */
    public sealed class GameDataCatalog : ScriptableObject
    {
        private readonly GameDataLookup lookup = new GameDataLookup();

        public MonsterDefinition[] Monsters = Array.Empty<MonsterDefinition>();
        public EnemyDefinition[] StageOneEnemies = Array.Empty<EnemyDefinition>();
        public EnemyDefinition[] StageTwoEnemies = Array.Empty<EnemyDefinition>();
        public StatusEffectDefinition[] StatusEffects = Array.Empty<StatusEffectDefinition>();

        /*
         * 현재 배열을 기준으로 모든 ID 조회 표를 다시 만든다.
         */
        public void RebuildLookup()
        {
            lookup.RegisterCatalog(this);
        }

        public T GetData<T>(string id)
            where T : class
        {
            return lookup.GetData<T>(id);
        }

        public bool TryGetData<T>(string id, out T value)
            where T : class
        {
            return lookup.TryGetData(id, out value);
        }

        public MonsterDefinition[] GetMonsters()
        {
            return lookup.GetMonsters(this);
        }

        public MonsterDefinition ResolveMonster(string id)
        {
            return lookup.ResolveMonster(id, this);
        }

        public SkillDefinition[] GetActiveSkills(string monsterId, MonsterDefinition fallbackMonster = null)
        {
            return lookup.GetActiveSkills(monsterId, fallbackMonster);
        }

        public PassiveDefinition[] GetPassiveSkills(string monsterId, MonsterDefinition fallbackMonster = null)
        {
            return lookup.GetPassiveSkills(monsterId, fallbackMonster);
        }

        public MonsterDefinition.RewardChoiceDefinition[] GetRewardChoices(
            string monsterId,
            MonsterDefinition fallbackMonster = null)
        {
            return lookup.GetRewardChoices(monsterId, fallbackMonster);
        }

        public SkillDefinition ResolveActiveSkill(
            string monsterId,
            SkillSlot slot,
            MonsterDefinition fallbackMonster = null)
        {
            return lookup.ResolveActiveSkill(monsterId, slot, fallbackMonster);
        }

        public PassiveDefinition ResolvePassiveSkill(
            string monsterId,
            SkillSlot slot,
            MonsterDefinition fallbackMonster = null)
        {
            return lookup.ResolvePassiveSkill(monsterId, slot, fallbackMonster);
        }
    }
}
