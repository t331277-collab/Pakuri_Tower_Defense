using System;
using System.Collections.Generic;
using Pakuri.Data;
using Pakuri.Run;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Pakuri.Combat
{
    public partial class CombatRuntimeController
    {
        private void BeginPrototypeDay(int requestedDay)
        {
            dayIndex = requestedDay > 11 ? 1 : Mathf.Max(1, requestedDay);
            battleResolved = false;
            victory = false;
            waitingForRewardChoice = false;
            rewardApplied = false;
            appliedRewardSummary = string.Empty;
            lastAppliedRewardUnlockedPassive = false;
            fireRequestedThisFrame = false;
            rewardOptions.Clear();
            ClearEnemyRuntime();
            ClearProjectileRuntime();

            currentCombatType = RunDayModel.Resolve(stageIndex, dayIndex).CombatType;
            ResolveStageOneEnemyPool(dayIndex);
            ResolveEncounterForDay(dayIndex, out pendingNormalSpawnCount, out pendingBossSpawn, out currentBossHealthMultiplier, out encounterLabel);
            ResolveBossPrisonersForCurrentCombat();
            pendingBossSpawnCount = pendingBossSpawn ? Mathf.Max(1, currentGuaranteedPrisonerDefinitions.Count) : 0;
            spawnedNormalCount = 0;
            spawnedBossCount = 0;
            spawnCooldown = 0.2f;
            currentShotsRemaining = magazineCapacityConfigured;
            shotCooldown = 0f;
            reloadRemaining = 0f;
            nexusCurrentHealth = nexusMaxHealth;
            unitCurrentHealth = unitMaxHealthConfigured;
            nextProjectileSequence = 0;
            currentAttackPoint = new Vector3(Mathf.Lerp(eveAnchor.position.x, enemySpawnAnchor.position.x, 0.55f), 8f, 0f);
            inputTargetAnchor.position = currentAttackPoint;
            UpdateSelectedMonsterStatusVisuals();
            statusLabel = $"{selectedActiveSkillName} 목표 지점을 클릭해 전투를 시작한다.";
        }

        private void ResolveEncounterForDay(int day, out int normalSpawnCount, out bool spawnBoss, out float bossMultiplier, out string label)
        {
            label = "Fixed Combat";
            normalSpawnCount = 3;
            spawnBoss = true;
            bossMultiplier = baseBossHealthMultiplier;

            switch (currentCombatType)
            {
                case RunCombatType.Day5Midboss:
                    label = "Day 5 Midboss Combat";
                    normalSpawnCount = 4;
                    bossMultiplier = baseBossHealthMultiplier + 6f;
                    return;
                case RunCombatType.Day10Midboss:
                    label = "Day 10 Midboss Combat";
                    normalSpawnCount = 4;
                    bossMultiplier = baseBossHealthMultiplier + 6f;
                    return;
                case RunCombatType.Boss:
                    label = "Stage Boss Combat";
                    normalSpawnCount = 5;
                    bossMultiplier = baseBossHealthMultiplier + 10f;
                    return;
                case RunCombatType.Elite:
                    label = "Elite Combat";
                    normalSpawnCount = 4;
                    bossMultiplier = baseBossHealthMultiplier + 2f;
                    return;
                default:
                    if (day == 1)
                    {
                        label = "Opening Normal Combat";
                        return;
                    }

                    label = "Normal Combat";
                    normalSpawnCount = 4;
                    bossMultiplier = baseBossHealthMultiplier + 2f;
                    return;
            }
        }

        private void ResolveStageOneEnemyPool(int day)
        {
            currentNormalEnemyPool.Clear();
            currentMidbossDefinition = null;
            currentDay5MidbossDefinition = null;
            currentDay10MidbossDefinition = null;
            currentBossDefinition = null;
            currentNormalBossDefinition = null;

            var stageOneEnemies = gameDataCatalog != null ? gameDataCatalog.StageOneEnemies : null;
            if (stageOneEnemies != null)
            {
                for (var i = 0; i < stageOneEnemies.Length; i++)
                {
                    var enemy = stageOneEnemies[i];
                    if (enemy == null)
                    {
                        continue;
                    }

                    switch (enemy.EncounterRole)
                    {
                        case EnemyEncounterRole.Day5Midboss:
                            currentDay5MidbossDefinition = enemy;
                            currentMidbossDefinition = enemy;
                            break;
                        case EnemyEncounterRole.Day10Midboss:
                            currentDay10MidbossDefinition = enemy;
                            if (day == 10 || day == 11)
                            {
                                currentMidbossDefinition = enemy;
                            }
                            break;
                        case EnemyEncounterRole.StageBoss:
                            currentBossDefinition = enemy;
                            break;
                        default:
                            currentNormalEnemyPool.Add(enemy);
                            break;
                    }
                }
            }

            if (currentNormalEnemyPool.Count == 0)
            {
                AddFallbackStageOneNormals();
            }

            if (currentMidbossDefinition == null)
            {
                currentMidbossDefinition = day == 10
                    ? CreateStageOneEnemy("stage1-attack-captain", "공격대장", EnemyEncounterRole.Day10Midboss, EnemyAttackType.Melee, DamageAttribute.Physical, 1.10f, 3200f, 26f, 0f, 12f, 4f, 4f, 4f, 3f, 3f, StageOneEnemySkillKind.ChargeCommand, "돌격 명령", 0f, 12f, 6f, 5f, 0f, "공격 숙련", "물리 피해 12% 증가")
                    : CreateStageOneEnemy("stage1-guardian-captain", "수호대장", EnemyEncounterRole.Day5Midboss, EnemyAttackType.Melee, DamageAttribute.Physical, 0.85f, 2200f, 18f, 4f, 15f, 5f, 5f, 5f, 4f, 6f, StageOneEnemySkillKind.GuardianFlag, "수호의 깃발", 0f, 10f, 5f, 4f, 100f, "수호 숙련", "받는 피해 12% 감소");
            }

            if (currentDay5MidbossDefinition == null)
            {
                currentDay5MidbossDefinition = CreateStageOneEnemy("stage1-guardian-captain", "수호대장", EnemyEncounterRole.Day5Midboss, EnemyAttackType.Melee, DamageAttribute.Physical, 0.85f, 2200f, 18f, 4f, 15f, 5f, 5f, 5f, 4f, 6f, StageOneEnemySkillKind.GuardianFlag, "수호의 깃발", 0f, 10f, 5f, 4f, 100f, "수호 숙련", "받는 피해 12% 감소");
            }

            if (currentDay10MidbossDefinition == null)
            {
                currentDay10MidbossDefinition = CreateStageOneEnemy("stage1-attack-captain", "공격대장", EnemyEncounterRole.Day10Midboss, EnemyAttackType.Melee, DamageAttribute.Physical, 1.10f, 3200f, 26f, 0f, 12f, 4f, 4f, 4f, 3f, 3f, StageOneEnemySkillKind.ChargeCommand, "돌격 명령", 0f, 12f, 6f, 5f, 0f, "공격 숙련", "물리 피해 12% 증가");
            }

            if (currentBossDefinition == null)
            {
                currentBossDefinition = CreateStageOneEnemy("stage1-hero-karin", "용사 카린", EnemyEncounterRole.StageBoss, EnemyAttackType.MeleeAndRanged, DamageAttribute.Physical, 1.00f, 5000f, 32f, 10f, 16f, 6f, 6f, 6f, 5f, 12f, StageOneEnemySkillKind.SacredSwordWave, "성검기", 2.2f, 9f, 0f, 8f, 0f, "용사의 힘", "물리 피해 15% 증가");
            }

            if (currentNormalEnemyPool.Count > 0)
            {
                currentNormalBossDefinition = currentNormalEnemyPool[UnityEngine.Random.Range(0, currentNormalEnemyPool.Count)];
            }
        }

        private void ResolveBossPrisonersForCurrentCombat()
        {
            currentGuaranteedPrisonerDefinitions.Clear();

            switch (currentCombatType)
            {
                case RunCombatType.Day5Midboss:
                    AddGuaranteedPrisoner(currentDay5MidbossDefinition ?? currentMidbossDefinition);
                    break;
                case RunCombatType.Day10Midboss:
                    AddGuaranteedPrisoner(currentDay10MidbossDefinition ?? currentMidbossDefinition);
                    break;
                case RunCombatType.Boss:
                    AddGuaranteedPrisoner(currentDay5MidbossDefinition);
                    AddGuaranteedPrisoner(currentDay10MidbossDefinition);
                    AddGuaranteedPrisoner(currentBossDefinition);
                    break;
                default:
                    AddGuaranteedPrisoner(currentNormalBossDefinition);
                    break;
            }

            guaranteedPrisonerName = currentGuaranteedPrisonerDefinitions.Count > 0
                ? currentGuaranteedPrisonerDefinitions[0].DisplayName
                : "강화된 견습 검사";
        }

        private void AddGuaranteedPrisoner(EnemyDefinition definition)
        {
            if (definition == null)
            {
                return;
            }

            if (!currentGuaranteedPrisonerDefinitions.Contains(definition))
            {
                currentGuaranteedPrisonerDefinitions.Add(definition);
            }
        }

        private void AddFallbackStageOneNormals()
        {
            currentNormalEnemyPool.Add(CreateStageOneEnemy("stage1-swordsman", "검사", EnemyEncounterRole.Normal, EnemyAttackType.Melee, DamageAttribute.Physical, 1.00f, 100f, 12f, 0f, 5f, 2f, 2f, 2f, 2f, 2f, StageOneEnemySkillKind.Slash, "베기", 1.0f, 2f, 0f, 1.4f, 0f, "검술 숙련", "물리 피해 10% 증가"));
            currentNormalEnemyPool.Add(CreateStageOneEnemy("stage1-shieldbearer", "방패병", EnemyEncounterRole.Normal, EnemyAttackType.Melee, DamageAttribute.Physical, 0.75f, 180f, 8f, 0f, 12f, 3f, 3f, 3f, 2f, 2f, StageOneEnemySkillKind.ShieldUp, "방패 들기", 0f, 8f, 4f, 0f, 0.25f, "두꺼운 갑옷", "방어력 10% 증가"));
            currentNormalEnemyPool.Add(CreateStageOneEnemy("stage1-archer", "궁수", EnemyEncounterRole.Normal, EnemyAttackType.Ranged, DamageAttribute.Physical, 0.90f, 80f, 10f, 0f, 3f, 2f, 2f, 2f, 2f, 2f, StageOneEnemySkillKind.AimedShot, "조준 사격", 1.5f, 5f, 0f, 7f, 0f, "정조준", "치명타 확률 8% 증가"));
            currentNormalEnemyPool.Add(CreateStageOneEnemy("stage1-rogue", "도적", EnemyEncounterRole.Normal, EnemyAttackType.Ranged, DamageAttribute.Physical, 1.00f, 70f, 15f, 0f, 2f, 2f, 2f, 2f, 2f, 2f, StageOneEnemySkillKind.ShurikenThrow, "수리검 투척", 1.4f, 4f, 0f, 6f, 0f, "날카로운 수리검", "치명타 피해 20% 증가"));
            currentNormalEnemyPool.Add(CreateStageOneEnemy("stage1-priest", "사제", EnemyEncounterRole.Normal, EnemyAttackType.Ranged, DamageAttribute.Holy, 0.80f, 90f, 4f, 12f, 3f, 2f, 2f, 2f, 2f, 8f, StageOneEnemySkillKind.Heal, "치유", 1.2f, 6f, 0f, 5f, 50f, "신성 집중", "치유량 15% 증가"));
        }

        private static EnemyDefinition CreateStageOneEnemy(
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
            if (!fallbackStageOneEnemyCache.TryGetValue(enemyId, out var enemy) || enemy == null)
            {
                enemy = ScriptableObject.CreateInstance<EnemyDefinition>();
                enemy.hideFlags = HideFlags.DontSave;
                fallbackStageOneEnemyCache[enemyId] = enemy;
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
            return enemy;
        }

        private void UpdateSpawning()
        {
            if (spawnedNormalCount >= pendingNormalSpawnCount && spawnedBossCount >= pendingBossSpawnCount)
            {
                return;
            }

            spawnCooldown -= Time.deltaTime;
            if (spawnCooldown > 0f)
            {
                return;
            }

            if (spawnedNormalCount < pendingNormalSpawnCount)
            {
                spawnedNormalCount += 1;
                SpawnEnemy(false, spawnedNormalCount);
                spawnCooldown = spawnInterval;
                return;
            }

            if (spawnedBossCount < pendingBossSpawnCount)
            {
                spawnedBossCount += 1;
                pendingBossSpawn = spawnedBossCount < pendingBossSpawnCount;
                SpawnEnemy(true, spawnedBossCount);
                spawnCooldown = spawnInterval;
            }
        }

        private void SpawnEnemy(bool isBoss, int sequence)
        {
            if (enemyRoot == null || enemySpawnAnchor == null)
            {
                return;
            }

            var definition = ResolveEnemyDefinitionForSpawn(isBoss, sequence);
            var displayName = definition != null && !string.IsNullOrWhiteSpace(definition.DisplayName)
                ? definition.DisplayName
                : (isBoss ? guaranteedPrisonerName : "견습 검사");
            var enemyObject = new GameObject(isBoss ? $"Enemy_Boss_{displayName}" : $"Enemy_Normal_{sequence:00}_{displayName}");
            enemyObject.transform.SetParent(enemyRoot, false);
            var spawnPosition = ResolveEnemySpawnPosition(isBoss);
            spawnPosition.z = 0f;
            enemyObject.transform.position = spawnPosition;
            enemyObject.transform.localScale = isBoss ? new Vector3(1.55f, 1.55f, 1f) : new Vector3(1.05f, 1.05f, 1f);

            var renderer = enemyObject.AddComponent<SpriteRenderer>();
            renderer.sprite = definition != null && definition.UnitSprite != null ? definition.UnitSprite : GetSharedSprite();
            renderer.sortingOrder = isBoss ? 18 : 17;
            var label = CreateEnemyLabel(enemyObject.transform, displayName, isBoss);
            var hpBarFill = CreateHpBar(
                enemyObject.transform,
                "EnemyHpBar",
                new Vector3(0f, isBoss ? 1.05f : 0.74f, 0f),
                isBoss ? 1.35f : 1.05f,
                0.08f,
                new Color(0.95f, 0.20f, 0.20f, 0.98f),
                34);

            var baseStats = definition != null && definition.Stats != null ? definition.Stats : null;
            var maxHealth = baseStats != null
                ? baseStats.MaxHealth * GetStageValueMultiplier() * GetEncounterHealthMultiplier(definition, isBoss)
                : normalEnemyHealth * GetStageValueMultiplier() * (isBoss ? currentBossHealthMultiplier : 1f);
            var runtime = new EnemyRuntime
            {
                GameObject = enemyObject,
                Transform = enemyObject.transform,
                Renderer = renderer,
                Label = label,
                HpBarFill = hpBarFill,
                Definition = definition,
                Defenses = definition != null && definition.Defenses != null ? definition.Defenses.Clone() : new AttributeDefenseSet(),
                MaxHealth = maxHealth,
                CurrentHealth = maxHealth,
                MoveSpeed = (baseStats != null ? baseStats.MoveSpeed : enemyMoveSpeed) * (isBoss ? 0.85f : 1f),
                ContactDamagePerSecond = enemyContactDamagePerSecond * (isBoss ? 2.5f : 1f),
                AttackPower = baseStats != null ? baseStats.AttackPower : 12f,
                SpellPower = baseStats != null ? baseStats.SpellPower : 0f,
                CriticalChanceBonus = baseStats != null ? baseStats.CriticalChance - DamageCalculator.BaseCriticalChance : 0f,
                CriticalMultiplierBonus = baseStats != null ? baseStats.CriticalDamage - DamageCalculator.BaseCriticalMultiplier : 0f,
                CriticalResistance = baseStats != null ? baseStats.CriticalResistance : 0f,
                IsBoss = isBoss,
                DisplayName = displayName
            };

            ApplyStageOnePassive(runtime);
            enemies.Add(runtime);
            UpdateEnemyColor(runtime);
            UpdateEnemyLabel(runtime);
        }

        private Vector3 ResolveEnemySpawnPosition(bool isBoss)
        {
            var spawnPosition = enemySpawnAnchor != null ? enemySpawnAnchor.position : DefaultEnemySpawnPosition;
            spawnPosition.x = EnemySpawnX;
            spawnPosition.y = isBoss ? BossSpawnY : UnityEngine.Random.Range(enemySpawnYRange.x, enemySpawnYRange.y);
            spawnPosition.z = 0f;
            return spawnPosition;
        }

        private static TextMesh CreateEnemyLabel(Transform parent, string displayName, bool isBoss)
        {
            var labelObject = new GameObject("EnemyHpLabel");
            labelObject.transform.SetParent(parent, false);
            labelObject.transform.localPosition = new Vector3(0f, isBoss ? 1.25f : 0.9f, 0f);
            labelObject.transform.localScale = new Vector3(0.12f, 0.12f, 1f);

            var label = labelObject.AddComponent<TextMesh>();
            label.anchor = TextAnchor.MiddleCenter;
            label.alignment = TextAlignment.Center;
            label.fontSize = 32;
            label.color = Color.white;
            label.text = displayName;

            var renderer = label.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.sortingOrder = 35;
            }

            return label;
        }

        private static SpriteRenderer CreateHpBar(
            Transform parent,
            string barName,
            Vector3 localPosition,
            float width,
            float height,
            Color fillColor,
            int sortingOrder)
        {
            var root = parent.Find(barName);
            if (root == null)
            {
                root = new GameObject(barName).transform;
                root.SetParent(parent, false);
            }

            root.localPosition = localPosition;
            root.localScale = new Vector3(width, 1f, 1f);

            var background = EnsureHpBarPart(root, "Background", new Color(0f, 0f, 0f, 0.75f), sortingOrder);
            background.transform.localPosition = Vector3.zero;
            background.transform.localScale = new Vector3(1f, height, 1f);

            var fill = EnsureHpBarPart(root, "Fill", fillColor, sortingOrder + 1);
            fill.transform.localScale = new Vector3(1f, height, 1f);
            fill.transform.localPosition = Vector3.zero;
            return fill;
        }

        private static SpriteRenderer EnsureHpBarPart(Transform root, string partName, Color color, int sortingOrder)
        {
            var part = root.Find(partName);
            if (part == null)
            {
                part = new GameObject(partName).transform;
                part.SetParent(root, false);
            }

            var renderer = part.GetComponent<SpriteRenderer>();
            if (renderer == null)
            {
                renderer = part.gameObject.AddComponent<SpriteRenderer>();
            }

            renderer.sprite = GetSharedSprite();
            renderer.color = color;
            renderer.sortingOrder = sortingOrder;
            return renderer;
        }

        private static void UpdateHpBarFill(SpriteRenderer fill, float currentHealth, float maxHealth)
        {
            if (fill == null)
            {
                return;
            }

            var ratio = maxHealth > 0f ? Mathf.Clamp01(currentHealth / maxHealth) : 0f;
            var localScale = fill.transform.localScale;
            fill.transform.localScale = new Vector3(ratio, localScale.y, 1f);
            fill.transform.localPosition = new Vector3(-0.5f + (ratio * 0.5f), 0f, -0.01f);
        }

        private EnemyDefinition ResolveEnemyDefinitionForSpawn(bool isBoss, int sequence)
        {
            if (!isBoss)
            {
                if (currentNormalEnemyPool.Count == 0)
                {
                    AddFallbackStageOneNormals();
                }

                return currentNormalEnemyPool.Count == 0 ? null : currentNormalEnemyPool[(sequence - 1) % currentNormalEnemyPool.Count];
            }

            if (currentCombatType == RunCombatType.Day5Midboss)
            {
                return currentDay5MidbossDefinition ?? currentMidbossDefinition;
            }

            if (currentCombatType == RunCombatType.Day10Midboss)
            {
                return currentDay10MidbossDefinition ?? currentMidbossDefinition;
            }

            if (currentCombatType == RunCombatType.Boss)
            {
                if (sequence == 1)
                {
                    return currentDay5MidbossDefinition;
                }

                if (sequence == 2)
                {
                    return currentDay10MidbossDefinition;
                }

                return currentBossDefinition;
            }

            if (currentNormalBossDefinition != null)
            {
                return currentNormalBossDefinition;
            }

            return currentNormalEnemyPool.Count == 0
                ? currentMidbossDefinition
                : currentNormalEnemyPool[UnityEngine.Random.Range(0, currentNormalEnemyPool.Count)];
        }

        private float GetEncounterHealthMultiplier(EnemyDefinition definition, bool isBoss)
        {
            if (definition == null)
            {
                return isBoss ? currentBossHealthMultiplier : 1f;
            }

            if (definition.EncounterRole == EnemyEncounterRole.Normal && isBoss)
            {
                return currentBossHealthMultiplier;
            }

            return 1f;
        }

        private void ApplyStageOnePassive(EnemyRuntime enemy)
        {
            if (enemy == null || enemy.Definition == null)
            {
                return;
            }

            switch (enemy.Definition.StageOneSkill)
            {
                case StageOneEnemySkillKind.Slash:
                    enemy.DamageMultiplier *= 1.10f;
                    break;
                case StageOneEnemySkillKind.ShieldUp:
                    MultiplyAllDefenses(enemy.Defenses, 1.10f);
                    break;
                case StageOneEnemySkillKind.AimedShot:
                    enemy.CriticalChanceBonus += 0.08f;
                    break;
                case StageOneEnemySkillKind.ShurikenThrow:
                    enemy.CriticalMultiplierBonus += 0.20f;
                    break;
                case StageOneEnemySkillKind.Heal:
                    enemy.HealingMultiplier *= 1.15f;
                    break;
                case StageOneEnemySkillKind.GuardianFlag:
                    enemy.DamageTakenMultiplier *= 0.88f;
                    break;
                case StageOneEnemySkillKind.ChargeCommand:
                    enemy.DamageMultiplier *= 1.12f;
                    break;
                case StageOneEnemySkillKind.SacredSwordWave:
                    enemy.DamageMultiplier *= 1.15f;
                    break;
            }
        }

        private static void MultiplyAllDefenses(AttributeDefenseSet defenses, float multiplier)
        {
            if (defenses == null)
            {
                return;
            }

            defenses.Physical *= multiplier;
            defenses.Fire *= multiplier;
            defenses.Lightning *= multiplier;
            defenses.Ice *= multiplier;
            defenses.Darkness *= multiplier;
            defenses.Holy *= multiplier;
        }

        private void UpdateEnemies()
        {
            for (var i = enemies.Count - 1; i >= 0; i--)
            {
                var enemy = enemies[i];
                if (enemy == null || enemy.GameObject == null)
                {
                    enemies.RemoveAt(i);
                    continue;
                }

                if (enemy.CurrentHealth <= 0f)
                {
                    Destroy(enemy.GameObject);
                    enemies.RemoveAt(i);
                    continue;
                }

                enemy.FlashTimer = Mathf.Max(0f, enemy.FlashTimer - Time.deltaTime);
                enemy.ShockTimer = Mathf.Max(0f, enemy.ShockTimer - Time.deltaTime);
                enemy.DamageReductionTimer = Mathf.Max(0f, enemy.DamageReductionTimer - Time.deltaTime);
                if (Mathf.Approximately(enemy.DamageReductionTimer, 0f))
                {
                    enemy.DamageTakenMultiplier = GetBaseDamageTakenMultiplier(enemy);
                }

                enemy.AttackBuffTimer = Mathf.Max(0f, enemy.AttackBuffTimer - Time.deltaTime);
                if (Mathf.Approximately(enemy.AttackBuffTimer, 0f))
                {
                    enemy.AttackBuffMultiplier = 1f;
                }

                enemy.MoveSpeedBuffTimer = Mathf.Max(0f, enemy.MoveSpeedBuffTimer - Time.deltaTime);
                if (Mathf.Approximately(enemy.MoveSpeedBuffTimer, 0f))
                {
                    enemy.MoveSpeedBuffMultiplier = 1f;
                }

                enemy.ActiveCooldownRemaining = Mathf.Max(0f, enemy.ActiveCooldownRemaining - Time.deltaTime);

                var targetTransform = GetEnemyPriorityTarget();
                if (targetTransform == null)
                {
                    continue;
                }

                var targetDistance = Vector2.Distance(enemy.Transform.position, targetTransform.position);
                var attackRange = GetEnemyAttackRange(enemy);
                if (targetDistance > attackRange)
                {
                    var speedMultiplier = enemy.ShockTimer > 0f ? Mathf.Max(0.45f, 1f - (enemy.ShockStacks * 0.15f)) : 1f;
                    enemy.Transform.position = Vector3.MoveTowards(enemy.Transform.position, targetTransform.position, enemy.MoveSpeed * enemy.MoveSpeedBuffMultiplier * speedMultiplier * Time.deltaTime);
                }
                else
                {
                    TryUseStageOneEnemySkill(enemy);
                }

                UpdateEnemyColor(enemy);
                UpdateEnemyLabel(enemy);
            }
        }

        private void UpdateEnemyColor(EnemyRuntime enemy)
        {
            if (enemy == null || enemy.Renderer == null)
            {
                return;
            }

            var baseColor = Color.white;

            if (enemy.ShockTimer > 0f)
            {
                baseColor = Color.white;
            }

            if (enemy.FlashTimer > 0f)
            {
                baseColor = Color.white;
            }

            enemy.Renderer.color = baseColor;
        }

        private static void UpdateEnemyLabel(EnemyRuntime enemy)
        {
            if (enemy == null)
            {
                return;
            }

            if (enemy.Label != null)
            {
                enemy.Label.text = $"{enemy.DisplayName}\nHP {Mathf.CeilToInt(Mathf.Max(0f, enemy.CurrentHealth))}/{Mathf.CeilToInt(enemy.MaxHealth)}";
            }

            UpdateHpBarFill(enemy.HpBarFill, enemy.CurrentHealth, enemy.MaxHealth);
        }

        private float GetBaseDamageTakenMultiplier(EnemyRuntime enemy)
        {
            if (enemy == null || enemy.Definition == null)
            {
                return 1f;
            }

            return enemy.Definition.StageOneSkill == StageOneEnemySkillKind.GuardianFlag ? 0.88f : 1f;
        }

        private static float GetEnemyAttackRange(EnemyRuntime enemy)
        {
            if (enemy == null || enemy.Definition == null)
            {
                return 1.4f;
            }

            switch (enemy.Definition.AttackType)
            {
                case EnemyAttackType.Ranged:
                    return Mathf.Max(5f, enemy.Definition.ActiveSkillRadius);
                case EnemyAttackType.MeleeAndRanged:
                    return Mathf.Max(4f, enemy.Definition.ActiveSkillRadius);
                default:
                    return 1.4f;
            }
        }

        private Transform GetEnemyPriorityTarget()
        {
            if (unitCurrentHealth > 0f && eveAnchor != null)
            {
                return eveAnchor;
            }

            return nexusAnchor;
        }

        private void TryUseStageOneEnemySkill(EnemyRuntime enemy)
        {
            if (enemy == null || enemy.Definition == null || enemy.ActiveCooldownRemaining > 0f)
            {
                return;
            }

            var definition = enemy.Definition;
            enemy.ActiveCooldownRemaining = Mathf.Max(0.1f, definition.ActiveSkillCooldown);

            switch (definition.StageOneSkill)
            {
                case StageOneEnemySkillKind.ShieldUp:
                    enemy.DamageTakenMultiplier = Mathf.Min(enemy.DamageTakenMultiplier, 0.75f);
                    enemy.DamageReductionTimer = Mathf.Max(enemy.DamageReductionTimer, definition.ActiveSkillDuration);
                    statusLabel = $"{enemy.DisplayName} {definition.ActiveSkillName}: {definition.ActiveSkillDuration:0.#}초 동안 받는 피해 25% 감소.";
                    break;
                case StageOneEnemySkillKind.Heal:
                    HealLowestStageOneEnemy(enemy);
                    break;
                case StageOneEnemySkillKind.GuardianFlag:
                    ApplyGuardianFlag(enemy);
                    break;
                case StageOneEnemySkillKind.ChargeCommand:
                    ApplyChargeCommand(enemy);
                    break;
                default:
                    if (UsesEnemyProjectile(enemy))
                    {
                        FireEnemyProjectile(enemy);
                    }
                    else
                    {
                        ApplyEnemyDamageToPriorityTarget(enemy);
                    }
                    break;
            }
        }

        private static bool UsesEnemyProjectile(EnemyRuntime enemy)
        {
            if (enemy == null || enemy.Definition == null)
            {
                return false;
            }

            return enemy.Definition.AttackType == EnemyAttackType.Ranged ||
                enemy.Definition.AttackType == EnemyAttackType.MeleeAndRanged;
        }

        private void FireEnemyProjectile(EnemyRuntime enemy)
        {
            if (enemy == null || enemy.Transform == null || projectileRoot == null)
            {
                return;
            }

            var targetTransform = GetEnemyPriorityTarget();
            if (targetTransform == null)
            {
                return;
            }

            var direction = targetTransform.position - enemy.Transform.position;
            direction.z = 0f;
            if (direction.sqrMagnitude < 0.01f)
            {
                ApplyEnemyDamageToPriorityTarget(enemy);
                return;
            }

            var targetsMonster = unitCurrentHealth > 0f && targetTransform == eveAnchor;
            var speed = 7.5f;
            var distance = direction.magnitude;
            direction.Normalize();

            var projectileObject = new GameObject($"EnemyProjectile_{enemy.DisplayName}");
            projectileObject.transform.SetParent(projectileRoot, false);
            projectileObject.transform.position = enemy.Transform.position;
            projectileObject.transform.localScale = new Vector3(0.28f, 0.28f, 1f);

            var renderer = projectileObject.AddComponent<SpriteRenderer>();
            renderer.sprite = enemy.Definition.ProjectileSprite != null ? enemy.Definition.ProjectileSprite : GetSharedSprite();
            renderer.color = enemy.IsBoss ? new Color(1f, 0.25f, 0.25f, 0.98f) : new Color(1f, 0.55f, 0.25f, 0.95f);
            renderer.sortingOrder = 24;

            projectiles.Add(new ProjectileRuntime
            {
                GameObject = projectileObject,
                Transform = projectileObject.transform,
                Renderer = renderer,
                Direction = direction,
                Speed = speed,
                RemainingLifetime = Mathf.Max(1f, (distance / speed) + 0.75f),
                HitRadius = 0.28f,
                Attribute = enemy.Definition.Attribute,
                IsEnemyProjectile = true,
                SourceEnemy = enemy,
                TargetTransform = targetTransform,
                TargetsMonster = targetsMonster
            });

            statusLabel = $"{enemy.DisplayName} {enemy.Definition.ActiveSkillName}: projectile fired.";
        }

        private void ApplyEnemyDamageToPriorityTarget(EnemyRuntime enemy)
        {
            if (enemy == null || enemy.Definition == null)
            {
                return;
            }

            var definition = enemy.Definition;
            if (unitCurrentHealth > 0f)
            {
                var resolution = EnemyAttackResolver.ResolveAgainstMonster(
                    definition,
                    enemy.AttackPower,
                    enemy.DamageMultiplier,
                    enemy.AttackBuffMultiplier,
                    enemy.CriticalChanceBonus,
                    enemy.CriticalMultiplierBonus,
                    selectedMonsterDefenses);
                unitCurrentHealth = Mathf.Max(0f, unitCurrentHealth - resolution.FinalDamage);
                statusLabel = $"{enemy.DisplayName} {definition.ActiveSkillName}: {selectedMonsterName}에게 {resolution.FinalDamage:0.0} {definition.Attribute} 피해.";
                return;
            }

            var nexusResolution = EnemyAttackResolver.ResolveAgainstNexus(
                definition,
                enemy.AttackPower,
                enemy.DamageMultiplier,
                enemy.AttackBuffMultiplier,
                enemy.CriticalChanceBonus,
                enemy.CriticalMultiplierBonus);
            nexusCurrentHealth = Mathf.Max(0f, nexusCurrentHealth - nexusResolution.FinalDamage);
            statusLabel = $"{enemy.DisplayName} {definition.ActiveSkillName}: Nexus에 {nexusResolution.FinalDamage:0.0} {definition.Attribute} 피해.";
        }

        private void HealLowestStageOneEnemy(EnemyRuntime caster)
        {
            EnemyRuntime target = null;
            for (var i = 0; i < enemies.Count; i++)
            {
                var enemy = enemies[i];
                if (enemy == null || enemy.CurrentHealth <= 0f || enemy.CurrentHealth >= enemy.MaxHealth)
                {
                    continue;
                }

                if (target == null || enemy.CurrentHealth / enemy.MaxHealth < target.CurrentHealth / target.MaxHealth)
                {
                    target = enemy;
                }
            }

            if (target == null)
            {
                target = caster;
            }

            var healAmount = (caster.Definition.ActiveSkillFlatValue + caster.SpellPower * caster.Definition.ActiveSkillCoefficient) * caster.HealingMultiplier;
            target.CurrentHealth = Mathf.Min(target.MaxHealth, target.CurrentHealth + healAmount);
            statusLabel = $"{caster.DisplayName} {caster.Definition.ActiveSkillName}: {target.DisplayName} {healAmount:0.0} 회복.";
        }

        private void ApplyGuardianFlag(EnemyRuntime caster)
        {
            var shieldAmount = Mathf.Max(0f, caster.Definition.ActiveSkillFlatValue);
            for (var i = 0; i < enemies.Count; i++)
            {
                var enemy = enemies[i];
                if (enemy == null || enemy.CurrentHealth <= 0f)
                {
                    continue;
                }

                if (Vector2.Distance(caster.Transform.position, enemy.Transform.position) <= Mathf.Max(0.1f, caster.Definition.ActiveSkillRadius))
                {
                    enemy.ShieldValue = Mathf.Max(enemy.ShieldValue, shieldAmount);
                }
            }

            statusLabel = $"{caster.DisplayName} {caster.Definition.ActiveSkillName}: 주변 적에게 보호막 {shieldAmount:0} 부여.";
        }

        private void ApplyChargeCommand(EnemyRuntime caster)
        {
            for (var i = 0; i < enemies.Count; i++)
            {
                var enemy = enemies[i];
                if (enemy == null || enemy.CurrentHealth <= 0f)
                {
                    continue;
                }

                if (Vector2.Distance(caster.Transform.position, enemy.Transform.position) <= Mathf.Max(0.1f, caster.Definition.ActiveSkillRadius))
                {
                    enemy.MoveSpeedBuffMultiplier = 1.20f;
                    enemy.MoveSpeedBuffTimer = Mathf.Max(enemy.MoveSpeedBuffTimer, caster.Definition.ActiveSkillDuration);
                    enemy.AttackBuffMultiplier = 1.15f;
                    enemy.AttackBuffTimer = Mathf.Max(enemy.AttackBuffTimer, caster.Definition.ActiveSkillDuration);
                }
            }

            statusLabel = $"{caster.DisplayName} {caster.Definition.ActiveSkillName}: 주변 적 이동속도 20%, 공격력 15% 증가.";
        }
    }
}
