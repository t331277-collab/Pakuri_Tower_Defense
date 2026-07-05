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
        public TextAsset[] MonsterSkillsProjectileFiles;
        public TextAsset MonsterSkillsLineAttack;
        public TextAsset[] MonsterSkillsLineAttackFiles;
        public TextAsset MonsterSkillsAreaAttack;
        public TextAsset[] MonsterSkillsAreaAttackFiles;
        public TextAsset MonsterSkillsSingleAttack;
        public TextAsset[] MonsterSkillsSingleAttackFiles;
        public TextAsset MonsterSkillsBuff;
        public TextAsset[] MonsterSkillsBuffFiles;
        public TextAsset MonsterSkillsPassive;
        public TextAsset[] MonsterSkillsPassiveFiles;
        public TextAsset MonsterSkillNodes;
        public TextAsset MonsterSkillNodeParams;
        public TextAsset[] MonsterSkillNodeFiles;
        public TextAsset[] MonsterSkillNodeParamFiles;
        public TextAsset MonsterSkillEffects;
        public TextAsset[] MonsterSkillEffectFiles;
        public TextAsset MonsterSkillTriggers;
        public TextAsset[] MonsterSkillTriggerFiles;
        public TextAsset MonsterSkillChoicesProjectile;
        public TextAsset[] MonsterSkillChoicesProjectileFiles;
        public TextAsset MonsterSkillChoicesLineAttack;
        public TextAsset[] MonsterSkillChoicesLineAttackFiles;
        public TextAsset MonsterSkillChoicesAreaAttack;
        public TextAsset[] MonsterSkillChoicesAreaAttackFiles;
        public TextAsset MonsterSkillChoicesSingleAttack;
        public TextAsset[] MonsterSkillChoicesSingleAttackFiles;
        public TextAsset MonsterSkillChoicesBuff;
        public TextAsset[] MonsterSkillChoicesBuffFiles;
        public TextAsset MonsterSkillChoicesPassive;
        public TextAsset[] MonsterSkillChoicesPassiveFiles;
        public TextAsset StatusEffects;
        public TextAsset StageOneEnemies;
        public TextAsset StageTwoEnemies;
        public TextAsset EnemySkills;
        public TextAsset EnemySkillNodes;
        public TextAsset EnemySkillNodeParams;
    }
}
