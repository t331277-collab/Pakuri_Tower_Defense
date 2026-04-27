using Pakuri.Combat;
using UnityEditor;
using UnityEngine;

namespace Pakuri.Data.Editor
{
    public static class PakuriGameDataSeeder
    {
        private const string RootFolder = "Assets/Data";
        private const string CatalogFolder = "Assets/Data/GameData";
        private const string MonsterFolder = "Assets/Data/GameData/Monsters";
        private const string EnemyFolder = "Assets/Data/GameData/Enemies";
        private const string CatalogAssetPath = "Assets/Data/GameData/GameDataCatalog.asset";

        [MenuItem("Pakuri/Seed Default Game Data")]
        public static void SeedDefaultGameData()
        {
            EnsureFolder("Assets", "Data");
            EnsureFolder(RootFolder, "GameData");
            EnsureFolder(CatalogFolder, "Monsters");
            EnsureFolder(CatalogFolder, "Enemies");

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

            var stageOneEnemies = CreateOrUpdateStageOneEnemies();

            catalog.Monsters = monsters;
            catalog.StageOneEnemies = stageOneEnemies;
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
            monster.PrimaryAttribute = ParseAttribute(elementLabel);
            monster.ActiveSkillName = activeSkillName;
            monster.PassiveSkillName = passiveSkillName;
            monster.BaseStats = new CombatStatBlock
            {
                MaxHealth = maxHealth,
                AttackPower = powerStat,
                SpellPower = powerStat,
                MoveSpeed = 1f,
                CriticalChance = DamageCalculator.BaseCriticalChance,
                CriticalDamage = DamageCalculator.BaseCriticalMultiplier
            };
            monster.Defenses = new AttributeDefenseSet();
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
            monster.ActiveSkills = BuildMonsterActiveSkills(monsterId);
            monster.PassiveSkills = BuildMonsterPassives(monsterId);
            EditorUtility.SetDirty(monster);
            return monster;
        }

        private static EnemyDefinition[] CreateOrUpdateStageOneEnemies()
        {
            return new[]
            {
                CreateOrUpdateEnemy("stage1-swordsman", "검사", EnemyEncounterRole.Normal, EnemyAttackType.Melee, DamageAttribute.Physical, 1.00f, 100f, 12f, 0f, 5f, 2f, 2f, 2f, 2f, 2f, StageOneEnemySkillKind.Slash, "베기", 1.0f, 2f, 0f, 1.4f, 0f, "검술 숙련", "물리 피해 10% 증가"),
                CreateOrUpdateEnemy("stage1-shieldbearer", "방패병", EnemyEncounterRole.Normal, EnemyAttackType.Melee, DamageAttribute.Physical, 0.75f, 180f, 8f, 0f, 12f, 3f, 3f, 3f, 2f, 2f, StageOneEnemySkillKind.ShieldUp, "방패 들기", 0f, 8f, 4f, 0f, 0.25f, "두꺼운 갑옷", "방어력 10% 증가"),
                CreateOrUpdateEnemy("stage1-archer", "궁수", EnemyEncounterRole.Normal, EnemyAttackType.Ranged, DamageAttribute.Physical, 0.90f, 80f, 10f, 0f, 3f, 2f, 2f, 2f, 2f, 2f, StageOneEnemySkillKind.AimedShot, "조준 사격", 1.5f, 5f, 0f, 7f, 0f, "정조준", "치명타 확률 8% 증가"),
                CreateOrUpdateEnemy("stage1-rogue", "도적", EnemyEncounterRole.Normal, EnemyAttackType.Ranged, DamageAttribute.Physical, 1.00f, 70f, 15f, 0f, 2f, 2f, 2f, 2f, 2f, 2f, StageOneEnemySkillKind.ShurikenThrow, "수리검 투척", 1.4f, 4f, 0f, 6f, 0f, "날카로운 수리검", "치명타 피해 20% 증가"),
                CreateOrUpdateEnemy("stage1-priest", "사제", EnemyEncounterRole.Normal, EnemyAttackType.Ranged, DamageAttribute.Holy, 0.80f, 90f, 4f, 12f, 3f, 2f, 2f, 2f, 2f, 8f, StageOneEnemySkillKind.Heal, "치유", 1.2f, 6f, 0f, 5f, 50f, "신성 집중", "치유량 15% 증가"),
                CreateOrUpdateEnemy("stage1-guardian-captain", "수호대장", EnemyEncounterRole.Day5Midboss, EnemyAttackType.Melee, DamageAttribute.Physical, 0.85f, 2200f, 18f, 4f, 15f, 5f, 5f, 5f, 4f, 6f, StageOneEnemySkillKind.GuardianFlag, "수호의 깃발", 0f, 10f, 5f, 4f, 100f, "수호 숙련", "받는 피해 12% 감소"),
                CreateOrUpdateEnemy("stage1-attack-captain", "공격대장", EnemyEncounterRole.Day10Midboss, EnemyAttackType.Melee, DamageAttribute.Physical, 1.10f, 3200f, 26f, 0f, 12f, 4f, 4f, 4f, 3f, 3f, StageOneEnemySkillKind.ChargeCommand, "돌격 명령", 0f, 12f, 6f, 5f, 0f, "공격 숙련", "물리 피해 12% 증가"),
                CreateOrUpdateEnemy("stage1-hero-karin", "용사 카린", EnemyEncounterRole.StageBoss, EnemyAttackType.MeleeAndRanged, DamageAttribute.Physical, 1.00f, 5000f, 32f, 10f, 16f, 6f, 6f, 6f, 5f, 12f, StageOneEnemySkillKind.SacredSwordWave, "성검기", 2.2f, 9f, 0f, 8f, 0f, "용사의 힘", "물리 피해 15% 증가")
            };
        }

        private static EnemyDefinition CreateOrUpdateEnemy(
            string enemyId,
            string displayName,
            EnemyEncounterRole encounterRole,
            EnemyAttackType attackType,
            DamageAttribute attribute,
            float moveSpeed,
            float maxHealth,
            float attackPower,
            float spellPower,
            float physicalDefense,
            float fireDefense,
            float lightningDefense,
            float iceDefense,
            float darknessDefense,
            float holyDefense,
            StageOneEnemySkillKind skillKind,
            string activeSkillName,
            float activeCoefficient,
            float activeCooldown,
            float activeDuration,
            float activeRadius,
            float activeFlatValue,
            string passiveSkillName,
            string passiveSummary)
        {
            var assetPath = $"{EnemyFolder}/{enemyId}.asset";
            var enemy = AssetDatabase.LoadAssetAtPath<EnemyDefinition>(assetPath);
            if (enemy == null)
            {
                enemy = ScriptableObject.CreateInstance<EnemyDefinition>();
                AssetDatabase.CreateAsset(enemy, assetPath);
            }

            enemy.EnemyId = enemyId;
            enemy.DisplayName = displayName;
            enemy.EncounterRole = encounterRole;
            enemy.AttackType = attackType;
            enemy.Attribute = attribute;
            enemy.Stats = new CombatStatBlock
            {
                MaxHealth = maxHealth,
                AttackPower = attackPower,
                SpellPower = spellPower,
                MoveSpeed = moveSpeed,
                CriticalChance = DamageCalculator.BaseCriticalChance,
                CriticalDamage = DamageCalculator.BaseCriticalMultiplier
            };
            enemy.Defenses = new AttributeDefenseSet
            {
                Physical = physicalDefense,
                Fire = fireDefense,
                Lightning = lightningDefense,
                Ice = iceDefense,
                Darkness = darknessDefense,
                Holy = holyDefense
            };
            enemy.StageOneSkill = skillKind;
            enemy.ActiveSkillName = activeSkillName;
            enemy.ActiveSkillCoefficient = activeCoefficient;
            enemy.ActiveSkillCooldown = activeCooldown;
            enemy.ActiveSkillDuration = activeDuration;
            enemy.ActiveSkillRadius = activeRadius;
            enemy.ActiveSkillFlatValue = activeFlatValue;
            enemy.PassiveSkillName = passiveSkillName;
            enemy.PassiveSummary = passiveSummary;
            EditorUtility.SetDirty(enemy);
            return enemy;
        }

        private static SkillDefinition[] BuildMonsterActiveSkills(string monsterId)
        {
            switch (monsterId)
            {
                case "ariel":
                    return Skills("ariel", DamageAttribute.Holy, "심판의 빛", "성광 방패", "축복의 파동", "천상의 낙인", "대천사의 강림");
                case "eve":
                    return new[]
                    {
                        Skill("eve-a", "아크 볼트", SkillSlot.A, SkillRuntimeKind.MagazineProjectile, SkillImplementationState.RuntimeImplemented, DamageAttribute.Lightning, 24f, 0f, 0.95f, 8f, 0f, 1.4f, 6, 4f, 0.35f, "감전 부여 기본 탄창형 투사체"),
                        Skill("eve-b", "프리즘 레이", SkillSlot.B, SkillRuntimeKind.LineAttack, SkillImplementationState.DataOnly, DamageAttribute.Lightning, 30f, 0f, 1.1f, 8.5f, 0f, 7f, 0, 0f, 0f, "번개/얼음 직선 관통 광선"),
                        Skill("eve-c", "프로스트 필드", SkillSlot.C, SkillRuntimeKind.Field, SkillImplementationState.DataOnly, DamageAttribute.Ice, 12f, 0f, 0.45f, 7f, 2.6f, 9f, 0, 0f, 0f, "추위와 빙결을 만드는 장판"),
                        Skill("eve-d", "스태틱 오버라이드", SkillSlot.D, SkillRuntimeKind.AreaAttack, SkillImplementationState.DataOnly, DamageAttribute.Lightning, 35f, 0f, 1.4f, 7f, 2.2f, 10f, 0, 0f, 0f, "감전 스택 폭발"),
                        Skill("eve-e", "드론 비컨", SkillSlot.E, SkillRuntimeKind.Mark, SkillImplementationState.DataOnly, DamageAttribute.Ice, 10f, 0f, 0.3f, 8f, 0f, 13f, 0, 0f, 0f, "설치형 보조와 취약 누적")
                    };
                case "rin":
                    return Skills("rin", DamageAttribute.Physical, "파쇄권", "하울링", "충격파", "종결 일격", "붕괴 타격");
                case "sein":
                    return Skills("sein", DamageAttribute.Fire, "열풍 화살", "작열 난사", "화염궤도", "초열 지대", "종말의 사선");
                case "vega":
                    return Skills("vega", DamageAttribute.Physical, "삼검난무", "침묵의 대태도", "몰살 허가", "검은 명부 개방", "최종선고");
                default:
                    return System.Array.Empty<SkillDefinition>();
            }
        }

        private static PassiveDefinition[] BuildMonsterPassives(string monsterId)
        {
            switch (monsterId)
            {
                case "ariel":
                    return Passives("ariel", "빛의 인도", "수호 교리", "축복 전파", "낙인 계시", "성역 선포");
                case "eve":
                    return Passives("eve", "전압 보정", "입자 분리", "냉각 알고리즘", "과전류 회로", "약점 분석");
                case "rin":
                    return Passives("rin", "양손잡이", "전장의 공명", "파문 증폭", "마무리 본능", "붕괴 여파");
                case "sein":
                    return Passives("sein", "가열 조준", "불꽃 탄막", "연소 궤적", "열압 확산", "종말 예고");
                case "vega":
                    return Passives("vega", "각인 심화", "봉인검식", "처형 준비", "연쇄 참결", "사형 집행인");
                default:
                    return System.Array.Empty<PassiveDefinition>();
            }
        }

        private static SkillDefinition[] Skills(string owner, DamageAttribute attribute, string a, string b, string c, string d, string e)
        {
            return new[]
            {
                Skill($"{owner}-a", a, SkillSlot.A, SkillRuntimeKind.MagazineProjectile, SkillImplementationState.RuntimeImplemented, attribute, 24f, 0f, 1f, 8f, 0f, 1.5f, 6, 4f, 0.35f, "기본 탄창형 공격"),
                Skill($"{owner}-b", b, SkillSlot.B, SkillRuntimeKind.CooldownProjectile, SkillImplementationState.DataOnly, attribute, 30f, 0.8f, 0.8f, 8f, 0f, 7f, 0, 0f, 0f, "보조 액티브"),
                Skill($"{owner}-c", c, SkillSlot.C, SkillRuntimeKind.AreaAttack, SkillImplementationState.DataOnly, attribute, 26f, 0.7f, 0.9f, 7f, 2.5f, 8f, 0, 0f, 0f, "범위 또는 제어 액티브"),
                Skill($"{owner}-d", d, SkillSlot.D, SkillRuntimeKind.Field, SkillImplementationState.DataOnly, attribute, 34f, 0.8f, 1f, 7f, 2.8f, 10f, 0, 0f, 0f, "지속/표식/보호 액티브"),
                Skill($"{owner}-e", e, SkillSlot.E, SkillRuntimeKind.AreaAttack, SkillImplementationState.DataOnly, attribute, 58f, 1.2f, 1.2f, 9f, 4f, 15f, 0, 0f, 0f, "광역 또는 결전기")
            };
        }

        private static SkillDefinition Skill(
            string id,
            string name,
            SkillSlot slot,
            SkillRuntimeKind kind,
            SkillImplementationState state,
            DamageAttribute attribute,
            float baseDamage,
            float attackCoefficient,
            float spellCoefficient,
            float range,
            float radius,
            float cooldown,
            int magazine,
            float reload,
            float shotInterval,
            string summary)
        {
            return new SkillDefinition
            {
                SkillId = id,
                DisplayName = name,
                Slot = slot,
                RuntimeKind = kind,
                ImplementationState = state,
                Attribute = attribute,
                BaseDamage = baseDamage,
                AttackPowerCoefficient = attackCoefficient,
                SpellPowerCoefficient = spellCoefficient,
                Range = range,
                Radius = radius,
                CooldownSeconds = cooldown,
                MagazineCapacity = magazine,
                ReloadSeconds = reload,
                ShotIntervalSeconds = shotInterval,
                CriticalAllowed = true,
                Summary = summary
            };
        }

        private static PassiveDefinition[] Passives(string owner, string f, string g, string h, string i, string j)
        {
            return new[]
            {
                Passive($"{owner}-f", f, SkillSlot.F, SkillSlot.A, "A 또는 기본 전투 성능 강화"),
                Passive($"{owner}-g", g, SkillSlot.G, SkillSlot.B, "B 액티브 습득 후 해금"),
                Passive($"{owner}-h", h, SkillSlot.H, SkillSlot.C, "C 액티브 습득 후 해금"),
                Passive($"{owner}-i", i, SkillSlot.I, SkillSlot.D, "D 액티브 습득 후 해금"),
                Passive($"{owner}-j", j, SkillSlot.J, SkillSlot.E, "E 액티브 습득 후 해금")
            };
        }

        private static PassiveDefinition Passive(string id, string name, SkillSlot slot, SkillSlot requiredActive, string summary)
        {
            return new PassiveDefinition
            {
                PassiveId = id,
                DisplayName = name,
                Slot = slot,
                RequiredActiveSlot = requiredActive,
                ImplementationState = SkillImplementationState.DataOnly,
                Summary = summary
            };
        }

        private static DamageAttribute ParseAttribute(string elementLabel)
        {
            switch (elementLabel)
            {
                case "화염":
                    return DamageAttribute.Fire;
                case "번개":
                    return DamageAttribute.Lightning;
                case "얼음":
                case "냉기":
                    return DamageAttribute.Ice;
                case "어둠":
                    return DamageAttribute.Darkness;
                case "신성":
                    return DamageAttribute.Holy;
                default:
                    return DamageAttribute.Physical;
            }
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
