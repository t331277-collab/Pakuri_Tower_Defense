using UnityEngine;

namespace Pakuri.Data
{
    [CreateAssetMenu(menuName = "Pakuri/CSV Runtime Source Catalog", fileName = "PakuriCsvRuntimeSourceCatalog")]
    public sealed class PakuriCsvRuntimeSourceCatalog : ScriptableObject
    {
        public TextAsset CatalogMonsters;
        public TextAsset CatalogStageOneEnemies;
        public TextAsset Monsters;
    public TextAsset MonsterRewardChoices;
    public TextAsset MonsterSkills;
    public TextAsset MonsterSkillEffects;
    public TextAsset MonsterSkillTriggers;
    public TextAsset MonsterSkillChoices;
    public TextAsset StatusEffects;
        public TextAsset StageOneEnemies;
        public TextAsset EnemySkills;
    }
}
