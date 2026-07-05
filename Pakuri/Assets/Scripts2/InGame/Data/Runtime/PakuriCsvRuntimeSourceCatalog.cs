using UnityEngine;

namespace Pakuri.Data
{
    [CreateAssetMenu(menuName = "Pakuri/CSV Runtime Source Catalog", fileName = "PakuriCsvRuntimeSourceCatalog")]
    public sealed class PakuriCsvRuntimeSourceCatalog : ScriptableObject
    {
        public TextAsset CatalogMonsters;
        public TextAsset CatalogStageOneEnemies;
        public TextAsset CatalogStageTwoEnemies;
        public TextAsset Monsters;
        public TextAsset MonsterRewardChoices;
        public TextAsset MonsterSkillsProjectile;
        public TextAsset MonsterSkillsLineAttack;
        public TextAsset MonsterSkillsAreaAttack;
        public TextAsset MonsterSkillsSingleAttack;
        public TextAsset MonsterSkillsBuff;
        public TextAsset MonsterSkillsPassive;
        public TextAsset MonsterSkillNodes;
        public TextAsset MonsterSkillNodeParams;
        public TextAsset MonsterSkillEffects;
        public TextAsset MonsterSkillTriggers;
        public TextAsset MonsterSkillChoicesProjectile;
        public TextAsset MonsterSkillChoicesLineAttack;
        public TextAsset MonsterSkillChoicesAreaAttack;
        public TextAsset MonsterSkillChoicesSingleAttack;
        public TextAsset MonsterSkillChoicesBuff;
        public TextAsset MonsterSkillChoicesPassive;
        public TextAsset StatusEffects;
        public TextAsset StageOneEnemies;
        public TextAsset StageTwoEnemies;
        public TextAsset EnemySkills;
        public TextAsset EnemySkillNodes;
        public TextAsset EnemySkillNodeParams;
    }
}
