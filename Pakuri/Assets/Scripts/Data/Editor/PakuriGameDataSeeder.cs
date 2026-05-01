using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
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
            var files = GetSkillDocumentPaths(monsterId, 'a', 'e');
            var skills = new List<SkillDefinition>(files.Count);
            for (var i = 0; i < files.Count; i++)
            {
                skills.Add(ParseActiveSkillDocument(monsterId, files[i]));
            }

            return skills.ToArray();
        }

        private static PassiveDefinition[] BuildMonsterPassives(string monsterId)
        {
            var files = GetSkillDocumentPaths(monsterId, 'f', 'j');
            var passives = new List<PassiveDefinition>(files.Count);
            for (var i = 0; i < files.Count; i++)
            {
                passives.Add(ParsePassiveSkillDocument(monsterId, files[i]));
            }

            return passives.ToArray();
        }

        private static List<string> GetSkillDocumentPaths(string monsterId, char firstSlot, char lastSlot)
        {
            var skillFolder = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "reference", "2.Monster", monsterId, "skill"));
            var paths = new List<string>();
            if (!Directory.Exists(skillFolder))
            {
                Debug.LogWarning($"Monster skill document folder not found: {skillFolder}");
                return paths;
            }

            for (var slot = firstSlot; slot <= lastSlot; slot++)
            {
                var matching = Directory.GetFiles(skillFolder, $"{slot}-*.md");
                Array.Sort(matching, StringComparer.OrdinalIgnoreCase);
                if (matching.Length > 0)
                {
                    paths.Add(matching[0]);
                }
                else
                {
                    Debug.LogWarning($"Monster skill document not found: {monsterId}/{slot}-*.md");
                }
            }

            return paths;
        }

        private static SkillDefinition ParseActiveSkillDocument(string monsterId, string path)
        {
            var markdown = File.ReadAllText(path);
            var slot = ParseSlotFromFileName(path);
            var skillName = ReadTableValue(markdown, "스킬명");
            if (string.IsNullOrWhiteSpace(skillName))
            {
                skillName = ReadNameFromHeading(markdown);
            }

            var description = ReadLeadQuote(markdown);
            var skillType = ReadTableValue(markdown, "스킬 타입");
            var attributeLabel = ReadTableValue(markdown, "피해 속성");
            var baseDamage = ReadFirstNumericTableValue(markdown, "기본 .* 피해");
            var attackCoefficient = ReadFirstNumericTableValue(markdown, "공격력 계수");
            var spellCoefficient = ReadFirstNumericTableValue(markdown, "주문력 계수");
            var range = ReadFirstNumericTableValue(markdown, "공격 범위|사거리|지정 사거리");
            var radius = ReadFirstNumericTableValue(markdown, "반경|폭발 반경|파동 반경|참격 반경|타격 범위|피해 범위|범위");
            var cooldown = ReadFirstNumericTableValue(markdown, "쿨타임|쿨다운|재사용 대기시간");
            var magazine = Mathf.RoundToInt(ReadFirstNumericTableValue(markdown, "탄창 수|탄창"));
            var reload = ReadFirstNumericTableValue(markdown, "재장전 시간");
            var shotInterval = ReadFirstNumericTableValue(markdown, "발사 간격|탄환 간격");

            return new SkillDefinition
            {
                SkillId = $"{monsterId}-{slot.ToString().ToLowerInvariant()}",
                DisplayName = skillName,
                Slot = slot,
                RuntimeKind = ParseRuntimeKind(skillType, skillName),
                ImplementationState = IsRuntimeImplementedActive(monsterId, slot) ? SkillImplementationState.RuntimeImplemented : SkillImplementationState.DataOnly,
                IsDefaultLearned = slot == SkillSlot.A,
                DescriptionText = description,
                Attribute = ParseAttribute(attributeLabel),
                BaseDamage = baseDamage,
                AttackPowerCoefficient = attackCoefficient,
                SpellPowerCoefficient = spellCoefficient,
                Range = range,
                Radius = radius,
                CooldownSeconds = cooldown,
                MagazineCapacity = magazine,
                ReloadSeconds = reload,
                ShotIntervalSeconds = shotInterval,
                CriticalAllowed = !ReadTableValue(markdown, "치명타 적용").Contains("불가"),
                StatusEffectId = ReadStatusEffectLabel(markdown),
                Summary = description,
                EnhancementChoices = ReadChoiceTable(markdown, $"{monsterId}-{slot.ToString().ToLowerInvariant()}-trait", "특성"),
                MasterSkillChoices = ReadChoiceTable(markdown, $"{monsterId}-{slot.ToString().ToLowerInvariant()}-master", "마스터 스킬")
            };
        }

        private static PassiveDefinition ParsePassiveSkillDocument(string monsterId, string path)
        {
            var markdown = File.ReadAllText(path);
            var slot = ParseSlotFromFileName(path);
            var passiveName = ReadTableValue(markdown, "패시브명");
            if (string.IsNullOrWhiteSpace(passiveName))
            {
                passiveName = ReadNameFromHeading(markdown);
            }

            var description = ReadLeadQuote(markdown);
            var summary = ReadEffectSummary(markdown);
            var requiredSlot = GetRequiredActiveSlot(slot);

            return new PassiveDefinition
            {
                PassiveId = $"{monsterId}-{slot.ToString().ToLowerInvariant()}",
                DisplayName = passiveName,
                Slot = slot,
                RequiredActiveSlot = requiredSlot,
                IsAvailableWithoutActiveRequirement = slot == SkillSlot.F,
                ImplementationState = IsRuntimeImplementedPassive(monsterId, slot) ? SkillImplementationState.RuntimeImplemented : SkillImplementationState.DataOnly,
                DescriptionText = string.IsNullOrWhiteSpace(summary) ? description : $"{description}\n{summary}",
                Summary = string.IsNullOrWhiteSpace(summary) ? description : summary,
                EnhancementChoices = ReadChoiceTable(markdown, $"{monsterId}-{slot.ToString().ToLowerInvariant()}-trait", "특성")
            };
        }

        private static bool IsRuntimeImplementedActive(string monsterId, SkillSlot slot)
        {
            if (slot == SkillSlot.A)
            {
                return true;
            }

            return IsRuntimeImplementedMonster(monsterId) && slot >= SkillSlot.B && slot <= SkillSlot.E;
        }

        private static bool IsRuntimeImplementedPassive(string monsterId, SkillSlot slot)
        {
            return IsRuntimeImplementedMonster(monsterId) && slot >= SkillSlot.F && slot <= SkillSlot.J;
        }

        private static bool IsRuntimeImplementedMonster(string monsterId)
        {
            return string.Equals(monsterId, "eve", StringComparison.OrdinalIgnoreCase)
                || string.Equals(monsterId, "ariel", StringComparison.OrdinalIgnoreCase);
        }

        private static SkillSlot ParseSlotFromFileName(string path)
        {
            var fileName = Path.GetFileName(path);
            if (string.IsNullOrEmpty(fileName))
            {
                return SkillSlot.A;
            }

            switch (char.ToUpperInvariant(fileName[0]))
            {
                case 'A':
                    return SkillSlot.A;
                case 'B':
                    return SkillSlot.B;
                case 'C':
                    return SkillSlot.C;
                case 'D':
                    return SkillSlot.D;
                case 'E':
                    return SkillSlot.E;
                case 'F':
                    return SkillSlot.F;
                case 'G':
                    return SkillSlot.G;
                case 'H':
                    return SkillSlot.H;
                case 'I':
                    return SkillSlot.I;
                case 'J':
                    return SkillSlot.J;
                default:
                    return SkillSlot.A;
            }
        }

        private static SkillSlot GetRequiredActiveSlot(SkillSlot passiveSlot)
        {
            switch (passiveSlot)
            {
                case SkillSlot.G:
                    return SkillSlot.B;
                case SkillSlot.H:
                    return SkillSlot.C;
                case SkillSlot.I:
                    return SkillSlot.D;
                case SkillSlot.J:
                    return SkillSlot.E;
                default:
                    return SkillSlot.A;
            }
        }

        private static string ReadNameFromHeading(string markdown)
        {
            using (var reader = new StringReader(markdown))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (!line.StartsWith("# ", StringComparison.Ordinal))
                    {
                        continue;
                    }

                    var delimiterIndex = line.LastIndexOf(" - ", StringComparison.Ordinal);
                    return delimiterIndex >= 0 ? line.Substring(delimiterIndex + 3).Trim() : line.TrimStart('#', ' ');
                }
            }

            return string.Empty;
        }

        private static string ReadLeadQuote(string markdown)
        {
            using (var reader = new StringReader(markdown))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.StartsWith("> ", StringComparison.Ordinal))
                    {
                        return line.Substring(2).Trim();
                    }
                }
            }

            return string.Empty;
        }

        private static string ReadTableValue(string markdown, string label)
        {
            var pattern = @"^\|\s*" + Regex.Escape(label) + @"\s*\|\s*(.*?)\s*\|";
            var match = Regex.Match(markdown, pattern, RegexOptions.Multiline);
            return match.Success ? CleanTableCell(match.Groups[1].Value) : string.Empty;
        }

        private static float ReadFirstNumericTableValue(string markdown, string labelRegex)
        {
            var pattern = @"^\|\s*(?:" + labelRegex + @")\s*\|\s*(.*?)\s*\|";
            var match = Regex.Match(markdown, pattern, RegexOptions.Multiline);
            if (!match.Success)
            {
                return 0f;
            }

            return ParseFirstFloat(match.Groups[1].Value);
        }

        private static float ParseFirstFloat(string value)
        {
            var match = Regex.Match(value, @"-?\d+(?:\.\d+)?");
            if (!match.Success)
            {
                return 0f;
            }

            return float.TryParse(match.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed)
                ? parsed
                : 0f;
        }

        private static string ReadStatusEffectLabel(string markdown)
        {
            if (markdown.Contains("감전"))
            {
                return "감전";
            }

            if (markdown.Contains("빙결"))
            {
                return "빙결";
            }

            if (markdown.Contains("화상"))
            {
                return "화상";
            }

            if (markdown.Contains("취약"))
            {
                return "취약";
            }

            return string.Empty;
        }

        private static string ReadEffectSummary(string markdown)
        {
            var rows = ReadRowsUnderSection(markdown, "기본 효과");
            var summaries = new List<string>();
            for (var i = 0; i < rows.Count; i++)
            {
                var columns = SplitTableRow(rows[i]);
                if (columns.Length >= 2 && !IsTableHeader(columns))
                {
                    summaries.Add($"{columns[0]}: {columns[1]}");
                }
            }

            return string.Join("\n", summaries);
        }

        private static SkillChoiceDefinition[] ReadChoiceTable(string markdown, string idPrefix, string sectionKeyword)
        {
            var rows = ReadRowsUnderSection(markdown, sectionKeyword);
            var choices = new List<SkillChoiceDefinition>();
            for (var i = 0; i < rows.Count; i++)
            {
                var columns = SplitTableRow(rows[i]);
                if (columns.Length < 2 || IsTableHeader(columns))
                {
                    continue;
                }

                choices.Add(new SkillChoiceDefinition
                {
                    ChoiceId = $"{idPrefix}-{choices.Count + 1}",
                    Title = CleanTableCell(columns[0]),
                    DescriptionText = CleanTableCell(columns[1])
                });
            }

            return choices.ToArray();
        }

        private static List<string> ReadRowsUnderSection(string markdown, string sectionKeyword)
        {
            var rows = new List<string>();
            var inSection = false;
            using (var reader = new StringReader(markdown))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.StartsWith("## ", StringComparison.Ordinal))
                    {
                        if (inSection)
                        {
                            break;
                        }

                        inSection = line.Contains(sectionKeyword);
                        continue;
                    }

                    if (inSection && line.StartsWith("|", StringComparison.Ordinal))
                    {
                        rows.Add(line);
                    }
                }
            }

            return rows;
        }

        private static string[] SplitTableRow(string row)
        {
            var trimmed = row.Trim().Trim('|');
            var columns = trimmed.Split('|');
            for (var i = 0; i < columns.Length; i++)
            {
                columns[i] = CleanTableCell(columns[i]);
            }

            return columns;
        }

        private static bool IsTableHeader(string[] columns)
        {
            if (columns.Length == 0)
            {
                return true;
            }

            return columns[0].Contains("---") || string.Equals(columns[0], "특성", StringComparison.Ordinal)
                || string.Equals(columns[0], "선택지", StringComparison.Ordinal) || string.Equals(columns[0], "항목", StringComparison.Ordinal);
        }

        private static string CleanTableCell(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Replace("`", string.Empty).Trim();
        }

        private static SkillRuntimeKind ParseRuntimeKind(string skillType, string skillName)
        {
            if (skillType.Contains("탄창"))
            {
                return SkillRuntimeKind.MagazineProjectile;
            }

            if (skillType.Contains("직선") || skillType.Contains("광선") || skillType.Contains("사선"))
            {
                return SkillRuntimeKind.LineAttack;
            }

            if (skillType.Contains("장판") || skillType.Contains("필드") || skillName.Contains("지대") || skillName.Contains("필드"))
            {
                return SkillRuntimeKind.Field;
            }

            if (skillType.Contains("보호막") || skillName.Contains("방패"))
            {
                return SkillRuntimeKind.Shield;
            }

            if (skillType.Contains("표식") || skillName.Contains("낙인") || skillName.Contains("비컨"))
            {
                return SkillRuntimeKind.Mark;
            }

            if (skillType.Contains("광역") || skillType.Contains("범위"))
            {
                return SkillRuntimeKind.AreaAttack;
            }

            if (skillName.Contains("선고") || skillName.Contains("처형"))
            {
                return SkillRuntimeKind.Execute;
            }

            return SkillRuntimeKind.CooldownProjectile;
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
