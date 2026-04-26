using UnityEditor;
using UnityEngine;

namespace Pakuri.Data.Editor
{
    public static class PakuriGameDataSeeder
    {
        private const string RootFolder = "Assets/Data";
        private const string CatalogFolder = "Assets/Data/GameData";
        private const string MonsterFolder = "Assets/Data/GameData/Monsters";
        private const string CatalogAssetPath = "Assets/Data/GameData/GameDataCatalog.asset";

        [MenuItem("Pakuri/Seed Default Game Data")]
        public static void SeedDefaultGameData()
        {
            EnsureFolder("Assets", "Data");
            EnsureFolder(RootFolder, "GameData");
            EnsureFolder(CatalogFolder, "Monsters");

            var catalog = AssetDatabase.LoadAssetAtPath<GameDataCatalog>(CatalogAssetPath);
            if (catalog == null)
            {
                catalog = ScriptableObject.CreateInstance<GameDataCatalog>();
                AssetDatabase.CreateAsset(catalog, CatalogAssetPath);
            }

            var monsters = new[]
            {
                CreateOrUpdateMonster(
                    "ariel",
                    "아리엘",
                    "파티 강화, 방어막, 신성 피해를 주축으로 하는 서포터 타워.",
                    "신성",
                    "심판의 빛",
                    "빛의 인도",
                    new Color(0.96f, 0.84f, 0.42f, 1f),
                    new Color(1f, 0.94f, 0.68f, 1f),
                    240f,
                    26f,
                    28f,
                    1.0f,
                    14f,
                    5f,
                    0.44f,
                    6,
                    4.2f,
                    0.42f,
                    0f,
                    string.Empty,
                    new[]
                    {
                        Reward("ariel-a-power", "심판의 빛 증폭", "심판의 빛 피해 +25%", 1.25f, 0, 1f, 1f, 0f, 0f),
                        Reward("ariel-a-magazine", "성광 장전", "탄창 +2, 사격 리듬 유지력 증가", 1f, 2, 1f, 1f, 0f, 0f),
                        Reward("ariel-f-guiding-light", "빛의 인도", "최대 체력 +30, 심판의 빛 피해 +10%", 1.10f, 0, 1f, 1f, 30f, 0f)
                    }),
                CreateOrUpdateMonster(
                    "eve",
                    "이브",
                    "번개/얼음 속성 엔진형 + 상태 제어 보조형 타워.",
                    "번개",
                    "아크 볼트",
                    "전압 보정",
                    new Color(0.41f, 0.78f, 1f, 1f),
                    new Color(0.61f, 0.93f, 1f, 1f),
                    220f,
                    30f,
                    24f,
                    0.95f,
                    15f,
                    5f,
                    0.42f,
                    6,
                    4f,
                    0.35f,
                    0.15f,
                    "감전",
                    new[]
                    {
                        Reward("eve-a-power", "아크 볼트 증폭", "아크 볼트 피해 +20%", 1.2f, 0, 1f, 1f, 0f, 0f),
                        Reward("eve-a-magazine", "축전지 확장", "탄창 +2, 보스전 유지력 증가", 1f, 2, 1f, 1f, 0f, 0f),
                        Reward("eve-f-voltage-calibration", "전압 보정", "발사 간격 -10%, 재장전 -10%, 감전 확률 +10%", 1f, 0, 0.9f, 0.9f, 0f, 0.10f)
                    }),
                CreateOrUpdateMonster(
                    "rin",
                    "린",
                    "많은 탄창을 활용한 물리 연사, 짧은 광역 충격파, 넉백 보조.",
                    "물리",
                    "파쇄권",
                    "양손잡이",
                    new Color(0.95f, 0.56f, 0.42f, 1f),
                    new Color(1f, 0.74f, 0.58f, 1f),
                    260f,
                    28f,
                    26f,
                    1.05f,
                    16f,
                    4f,
                    0.48f,
                    8,
                    3.6f,
                    0.28f,
                    0f,
                    string.Empty,
                    new[]
                    {
                        Reward("rin-a-power", "파쇄권 증폭", "파쇄권 피해 +20%", 1.2f, 0, 1f, 1f, 0f, 0f),
                        Reward("rin-a-magazine", "압축 장전", "탄창 +3, 재장전 부담 완화", 1f, 3, 1f, 0.95f, 0f, 0f),
                        Reward("rin-f-ambidextrous", "양손잡이", "최대 체력 +35, 파쇄권 피해 +12%", 1.12f, 0, 1f, 1f, 35f, 0f)
                    }),
                CreateOrUpdateMonster(
                    "sein",
                    "세인",
                    "화염 탄창 화력과 화염 저항 감소를 주축으로 한 화염 저격수.",
                    "화염",
                    "열풍 화살",
                    "가열 조준",
                    new Color(1f, 0.55f, 0.26f, 1f),
                    new Color(1f, 0.78f, 0.34f, 1f),
                    210f,
                    32f,
                    30f,
                    0.9f,
                    18f,
                    5f,
                    0.40f,
                    6,
                    4f,
                    0.32f,
                    0f,
                    string.Empty,
                    new[]
                    {
                        Reward("sein-a-power", "열풍 화살 증폭", "열풍 화살 피해 +22%", 1.22f, 0, 1f, 1f, 0f, 0f),
                        Reward("sein-a-magazine", "화염 장전", "탄창 +2, 발사 간격 -5%", 1f, 2, 0.95f, 1f, 0f, 0f),
                        Reward("sein-f-heated-aim", "가열 조준", "최대 체력 +20, 열풍 화살 피해 +15%", 1.15f, 0, 1f, 1f, 20f, 0f)
                    }),
                CreateOrUpdateMonster(
                    "vega",
                    "베가",
                    "물리 관통 피해, 침묵, 이름표식 기반 광역 참격과 처형.",
                    "물리",
                    "삼검난무",
                    "각인 심화",
                    new Color(0.62f, 0.56f, 0.98f, 1f),
                    new Color(0.80f, 0.72f, 1f, 1f),
                    225f,
                    31f,
                    25f,
                    1.0f,
                    17f,
                    5f,
                    0.38f,
                    9,
                    3.8f,
                    0.18f,
                    0f,
                    string.Empty,
                    new[]
                    {
                        Reward("vega-a-power", "삼검난무 증폭", "삼검난무 피해 +18%", 1.18f, 0, 1f, 1f, 0f, 0f),
                        Reward("vega-a-magazine", "검기 연장", "탄창 +3, 발사 간격 -8%", 1f, 3, 0.92f, 1f, 0f, 0f),
                        Reward("vega-f-deep-engraving", "각인 심화", "최대 체력 +25, 삼검난무 피해 +12%", 1.12f, 0, 1f, 1f, 25f, 0f)
                    })
            };

            catalog.Monsters = monsters;
            EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Selection.activeObject = catalog;
            Debug.Log("Pakuri default game data seeded.");
        }

        private static MonsterDefinition CreateOrUpdateMonster(
            string monsterId,
            string displayName,
            string roleSummary,
            string elementLabel,
            string activeSkillName,
            string passiveSkillName,
            Color unitColor,
            Color projectileColor,
            float maxHealth,
            float powerStat,
            float baseDamage,
            float powerCoefficient,
            float projectileSpeed,
            float projectileLifetime,
            float projectileHitRadius,
            int magazineCapacity,
            float reloadDuration,
            float shotInterval,
            float statusChance,
            string statusEffectLabel,
            MonsterDefinition.RewardChoiceDefinition[] rewards)
        {
            var assetPath = $"{MonsterFolder}/{monsterId}.asset";
            var monster = AssetDatabase.LoadAssetAtPath<MonsterDefinition>(assetPath);
            if (monster == null)
            {
                monster = ScriptableObject.CreateInstance<MonsterDefinition>();
                AssetDatabase.CreateAsset(monster, assetPath);
            }

            monster.MonsterId = monsterId;
            monster.DisplayName = displayName;
            monster.RoleSummary = roleSummary;
            monster.ElementLabel = elementLabel;
            monster.ActiveSkillName = activeSkillName;
            monster.PassiveSkillName = passiveSkillName;
            monster.UnitColor = unitColor;
            monster.ProjectileColor = projectileColor;
            monster.MaxHealth = maxHealth;
            monster.PowerStat = powerStat;
            monster.BaseDamage = baseDamage;
            monster.PowerCoefficient = powerCoefficient;
            monster.ProjectileSpeed = projectileSpeed;
            monster.ProjectileLifetime = projectileLifetime;
            monster.ProjectileHitRadius = projectileHitRadius;
            monster.MagazineCapacity = magazineCapacity;
            monster.ReloadDuration = reloadDuration;
            monster.ShotInterval = shotInterval;
            monster.StatusChance = statusChance;
            monster.StatusEffectLabel = statusEffectLabel;
            monster.InitialRewardChoices = rewards;
            EditorUtility.SetDirty(monster);
            return monster;
        }

        private static MonsterDefinition.RewardChoiceDefinition Reward(
            string rewardId,
            string title,
            string description,
            float damageMultiplier,
            int magazineBonus,
            float shotIntervalMultiplier,
            float reloadDurationMultiplier,
            float maxHealthBonus,
            float statusChanceBonus)
        {
            return new MonsterDefinition.RewardChoiceDefinition
            {
                RewardId = rewardId,
                Title = title,
                Description = description,
                DamageMultiplier = damageMultiplier,
                MagazineBonus = magazineBonus,
                ShotIntervalMultiplier = shotIntervalMultiplier,
                ReloadDurationMultiplier = reloadDurationMultiplier,
                MaxHealthBonus = maxHealthBonus,
                StatusChanceBonus = statusChanceBonus
            };
        }

        private static void EnsureFolder(string parentFolder, string childFolderName)
        {
            var childPath = $"{parentFolder}/{childFolderName}";
            if (!AssetDatabase.IsValidFolder(childPath))
            {
                AssetDatabase.CreateFolder(parentFolder, childFolderName);
            }
        }
    }
}
