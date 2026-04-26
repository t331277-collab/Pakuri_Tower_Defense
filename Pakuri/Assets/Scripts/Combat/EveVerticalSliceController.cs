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
    [ExecuteAlways]
    public class EveVerticalSliceController : MonoBehaviour
    {
        [Serializable]
        private sealed class RewardOption
        {
            public string RewardId;
            public string Title;
            public string Description;
            public float DamageMultiplier = 1f;
            public int MagazineBonus;
            public float ShotIntervalMultiplier = 1f;
            public float ReloadDurationMultiplier = 1f;
            public float MaxHealthBonus;
            public float StatusChanceBonus;
            public bool UnlocksPassive;
        }

        private sealed class EnemyRuntime
        {
            public GameObject GameObject;
            public Transform Transform;
            public SpriteRenderer Renderer;
            public float MaxHealth;
            public float CurrentHealth;
            public float MoveSpeed;
            public float ContactDamagePerSecond;
            public bool IsBoss;
            public float ShockTimer;
            public int ShockStacks;
            public float FlashTimer;
            public string DisplayName;
        }

        private sealed class ProjectileRuntime
        {
            public GameObject GameObject;
            public Transform Transform;
            public SpriteRenderer Renderer;
            public Vector3 Direction;
            public float Speed;
            public float RemainingLifetime;
            public float HitRadius;
            public float BaseDamage;
        }

        public readonly struct RewardChoiceView
        {
            public RewardChoiceView(string rewardId, string title, string description)
            {
                RewardId = rewardId;
                Title = title;
                Description = description;
            }

            public string RewardId { get; }
            public string Title { get; }
            public string Description { get; }
        }

        [Header("Scene References")]
        [SerializeField] private Camera targetCamera;
        [SerializeField] private Transform nexusAnchor;
        [SerializeField] private Transform eveAnchor;
        [SerializeField] private Transform enemySpawnAnchor;
        [SerializeField] private Transform inputTargetAnchor;
        [SerializeField] private Transform enemyRoot;
        [SerializeField] private Transform projectileRoot;

        [Header("Battlefield")]
        [SerializeField] private Vector2 fieldSize = new Vector2(32f, 18f);
        [SerializeField] private Vector2 enemySpawnYRange = new Vector2(6f, 10f);

        [Header("Run State")]
        [SerializeField, Min(1)] private int stageIndex = 1;
        [SerializeField, Min(1)] private int dayIndex = 1;
        [SerializeField] private bool autoStartPrototype;
        [SerializeField] private bool useLegacyOnGui;

        [Header("Fallback Defaults")]
        [SerializeField] private float eveMaxHealth = 220f;
        [SerializeField] private float eveSpellPower = 30f;
        [SerializeField] private float eveBaseLightningDamage = 24f;
        [SerializeField] private float eveSpellPowerCoefficient = 0.95f;
        [SerializeField] private float eveProjectileSpeed = 15f;
        [SerializeField] private float eveProjectileLifetime = 5f;
        [SerializeField] private float eveProjectileHitRadius = 0.42f;
        [SerializeField] private int eveMagazineCapacity = 6;
        [SerializeField] private float eveReloadDuration = 4f;
        [SerializeField] private float eveShotInterval = 0.35f;
        [SerializeField, Range(0f, 1f)] private float eveShockChance = 0.15f;

        [Header("Prototype Enemy Defaults")]
        [SerializeField] private float normalEnemyHealth = 100f;
        [SerializeField] private float baseBossHealthMultiplier = 12f;
        [SerializeField] private float enemyMoveSpeed = 1f;
        [SerializeField] private float enemyContactDamagePerSecond = 12f;
        [SerializeField] private float spawnInterval = 1.05f;

        private static Sprite sharedSprite;

        private readonly List<EnemyRuntime> enemies = new List<EnemyRuntime>();
        private readonly List<ProjectileRuntime> projectiles = new List<ProjectileRuntime>();
        private readonly List<RewardOption> rewardOptions = new List<RewardOption>();
        private readonly HashSet<string> blockedRewardIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        private readonly Color nexusColor = new Color(1f, 0.77f, 0.35f, 0.95f);
        private readonly Color spawnMarkerColor = new Color(1f, 0.38f, 0.35f, 0.45f);
        private readonly Color inputMarkerColor = new Color(0.60f, 0.42f, 1f, 0.35f);

        private float nexusMaxHealth = 500f;
        private float nexusCurrentHealth;
        private float unitCurrentHealth;
        private float currentBossHealthMultiplier;
        private int pendingNormalSpawnCount;
        private int spawnedNormalCount;
        private bool pendingBossSpawn;
        private float spawnCooldown;
        private int currentShotsRemaining;
        private float shotCooldown;
        private float reloadRemaining;
        private Vector3 currentAttackPoint;
        private bool fireRequestedThisFrame;
        private bool battleResolved;
        private bool victory;
        private bool waitingForRewardChoice;
        private bool rewardApplied;
        private bool runInitialized;
        private bool lastAppliedRewardUnlockedPassive;
        private int rewardGold;
        private int rewardDarkTrace;
        private int rewardPrisonerCount;
        private int nextProjectileSequence;
        private string guaranteedPrisonerName = "강화된 견습 검사";
        private string encounterLabel = "Fixed Combat";
        private string statusLabel = "Run controller ready.";
        private string appliedRewardSummary = string.Empty;

        private MonsterDefinition selectedMonster;
        private string selectedMonsterName = "이브";
        private string selectedElementLabel = "번개";
        private string selectedActiveSkillName = "아크 볼트";
        private string selectedPassiveSkillName = "전압 보정";
        private string selectedStatusEffectLabel = "감전";
        private Color selectedUnitColor = new Color(0.41f, 0.78f, 1f, 0.95f);
        private Color selectedProjectileColor = new Color(0.61f, 0.93f, 1f, 0.98f);
        private float unitMaxHealthConfigured;
        private float powerStatConfigured;
        private float baseDamageConfigured;
        private float powerCoefficientConfigured;
        private float projectileSpeedConfigured;
        private float projectileLifetimeConfigured;
        private float projectileHitRadiusConfigured;
        private int magazineCapacityConfigured;
        private float reloadDurationConfigured;
        private float shotIntervalConfigured;
        private float statusChanceConfigured;
        private float lastAppliedDamageMultiplier = 1f;
        private int lastAppliedMagazineBonus;
        private float lastAppliedShotIntervalMultiplier = 1f;
        private float lastAppliedReloadDurationMultiplier = 1f;
        private float lastAppliedMaxHealthBonus;
        private float lastAppliedStatusChanceBonus;

        public bool HasActiveRun => runInitialized;
        public bool IsBattleResolved => battleResolved;
        public bool IsVictory => victory;
        public bool IsWaitingForRewardChoice => waitingForRewardChoice;
        public int RewardGold => rewardGold;
        public int RewardDarkTrace => rewardDarkTrace;
        public int RewardPrisonerCount => rewardPrisonerCount;
        public string GuaranteedPrisonerName => guaranteedPrisonerName;
        public string EncounterLabel => encounterLabel;
        public string StatusLabel => statusLabel;
        public string AppliedRewardSummary => appliedRewardSummary;
        public string SelectedMonsterName => selectedMonsterName;
        public string SelectedMonsterPassiveName => selectedPassiveSkillName;
        public float NexusMaxHealth => nexusMaxHealth;
        public float NexusCurrentHealth => nexusCurrentHealth;
        public float UnitMaxHealth => unitMaxHealthConfigured;
        public float UnitCurrentHealth => unitCurrentHealth;
        public int CurrentShotsRemaining => currentShotsRemaining;
        public int MagazineCapacity => magazineCapacityConfigured;
        public float ReloadRemaining => reloadRemaining;
        public float ShotInterval => shotIntervalConfigured;
        public bool LastAppliedRewardUnlockedPassive => lastAppliedRewardUnlockedPassive;
        public float LastAppliedDamageMultiplier => lastAppliedDamageMultiplier;
        public int LastAppliedMagazineBonus => lastAppliedMagazineBonus;
        public float LastAppliedShotIntervalMultiplier => lastAppliedShotIntervalMultiplier;
        public float LastAppliedReloadDurationMultiplier => lastAppliedReloadDurationMultiplier;
        public float LastAppliedMaxHealthBonus => lastAppliedMaxHealthBonus;
        public float LastAppliedStatusChanceBonus => lastAppliedStatusChanceBonus;

        private void OnEnable()
        {
            ResolveSceneReferences();
            ConfigureCamera();
            ApplyFallbackMonsterValues();
            EnsureAnchorVisuals();
        }

        private void OnValidate()
        {
            enemySpawnYRange.x = Mathf.Min(enemySpawnYRange.x, enemySpawnYRange.y);
            enemySpawnYRange.y = Mathf.Max(enemySpawnYRange.x, enemySpawnYRange.y);
            stageIndex = Mathf.Clamp(stageIndex, 1, 4);
            dayIndex = Mathf.Clamp(dayIndex, 1, 11);
            eveProjectileSpeed = Mathf.Max(0.1f, eveProjectileSpeed);
            eveProjectileLifetime = Mathf.Max(0.1f, eveProjectileLifetime);
            eveProjectileHitRadius = Mathf.Max(0.1f, eveProjectileHitRadius);

            if (!isActiveAndEnabled)
            {
                return;
            }

            ResolveSceneReferences();
            ConfigureCamera();
            EnsureAnchorVisuals();
        }

        private void Start()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            if (autoStartPrototype)
            {
                runInitialized = true;
                BeginPrototypeDay(dayIndex);
            }
        }

        private void Update()
        {
            if (!Application.isPlaying || !runInitialized)
            {
                return;
            }

            HandlePointerInput();
            UpdateMarkerPosition();

            if (battleResolved)
            {
                return;
            }

            UpdateSpawning();
            UpdateEnemies();
            UpdateProjectiles();
            UpdateSelectedMonsterCombat();
            CheckBattleResolution();
        }

        private void OnGUI()
        {
            if (!Application.isPlaying || !runInitialized || !useLegacyOnGui)
            {
                return;
            }

            DrawHud();

            if (!battleResolved)
            {
                return;
            }

            if (victory)
            {
                DrawVictoryPanel();
            }
            else
            {
                DrawDefeatPanel();
            }
        }

        public void BeginConfiguredDay(MonsterDefinition monster, RunSession session)
        {
            if (session == null)
            {
                return;
            }

            stageIndex = Mathf.Clamp(session.StageIndex, 1, 4);
            ConfigureMonster(monster);
            ApplyPersistedRewardState(session);
            blockedRewardIds.Clear();
            if (session.ChosenRewardIds != null)
            {
                foreach (var rewardId in session.ChosenRewardIds)
                {
                    if (!string.IsNullOrWhiteSpace(rewardId))
                    {
                        blockedRewardIds.Add(rewardId);
                    }
                }
            }

            runInitialized = true;
            BeginPrototypeDay(session.DayIndex);
        }

        public void ResetPrototypeState()
        {
            runInitialized = false;
            battleResolved = false;
            victory = false;
            waitingForRewardChoice = false;
            rewardApplied = false;
            appliedRewardSummary = string.Empty;
            lastAppliedDamageMultiplier = 1f;
            lastAppliedMagazineBonus = 0;
            lastAppliedShotIntervalMultiplier = 1f;
            lastAppliedReloadDurationMultiplier = 1f;
            lastAppliedMaxHealthBonus = 0f;
            lastAppliedStatusChanceBonus = 0f;
            statusLabel = "Run controller ready.";
            ClearEnemyRuntime();
            ClearProjectileRuntime();
        }

        public int GetRewardChoiceCount()
        {
            return rewardOptions.Count;
        }

        public RewardChoiceView GetRewardChoiceView(int rewardIndex)
        {
            if (rewardIndex < 0 || rewardIndex >= rewardOptions.Count)
            {
                return default;
            }

            var option = rewardOptions[rewardIndex];
            return new RewardChoiceView(option.RewardId, option.Title, option.Description);
        }

        public string ApplyRewardChoice(int rewardIndex)
        {
            if (rewardIndex < 0 || rewardIndex >= rewardOptions.Count)
            {
                return string.Empty;
            }

            var option = rewardOptions[rewardIndex];
            baseDamageConfigured *= Mathf.Max(0.1f, option.DamageMultiplier);
            magazineCapacityConfigured = Mathf.Max(1, magazineCapacityConfigured + option.MagazineBonus);
            shotIntervalConfigured = Mathf.Max(0.05f, shotIntervalConfigured * Mathf.Max(0.1f, option.ShotIntervalMultiplier));
            reloadDurationConfigured = Mathf.Max(0.25f, reloadDurationConfigured * Mathf.Max(0.1f, option.ReloadDurationMultiplier));
            unitMaxHealthConfigured = Mathf.Max(1f, unitMaxHealthConfigured + option.MaxHealthBonus);
            unitCurrentHealth = Mathf.Min(unitCurrentHealth + option.MaxHealthBonus, unitMaxHealthConfigured);
            statusChanceConfigured = Mathf.Clamp01(statusChanceConfigured + option.StatusChanceBonus);
            currentShotsRemaining = Mathf.Min(currentShotsRemaining, magazineCapacityConfigured);
            lastAppliedDamageMultiplier = Mathf.Max(0.1f, option.DamageMultiplier);
            lastAppliedMagazineBonus = option.MagazineBonus;
            lastAppliedShotIntervalMultiplier = Mathf.Max(0.1f, option.ShotIntervalMultiplier);
            lastAppliedReloadDurationMultiplier = Mathf.Max(0.1f, option.ReloadDurationMultiplier);
            lastAppliedMaxHealthBonus = option.MaxHealthBonus;
            lastAppliedStatusChanceBonus = option.StatusChanceBonus;

            lastAppliedRewardUnlockedPassive = option.UnlocksPassive;
            appliedRewardSummary = $"{option.Title} 적용: {option.Description}";
            rewardApplied = true;
            waitingForRewardChoice = false;
            statusLabel = appliedRewardSummary;
            blockedRewardIds.Add(option.RewardId);
            return option.RewardId;
        }

        private void ResolveSceneReferences()
        {
            if (targetCamera == null)
            {
                targetCamera = Camera.main;
            }

            if (targetCamera == null)
            {
                targetCamera = FindFirstObjectByType<Camera>();
            }

            transform.position = Vector3.zero;
            transform.rotation = Quaternion.identity;
            transform.localScale = Vector3.one;

            nexusAnchor = EnsureChild(nexusAnchor, "Nexus", new Vector3(2f, 8f, 0f));
            eveAnchor = EnsureChild(eveAnchor, "EveUnit", new Vector3(6f, 8f, 0f));
            enemySpawnAnchor = EnsureChild(enemySpawnAnchor, "EnemySpawnPoint", new Vector3(29f, 8f, 0f));
            inputTargetAnchor = EnsureChild(inputTargetAnchor, "InputTarget", new Vector3(16f, 8f, 0f));
            enemyRoot = EnsureChild(enemyRoot, "EnemyRoot", Vector3.zero);
            projectileRoot = EnsureChild(projectileRoot, "ProjectileRoot", Vector3.zero);

            if (Application.isPlaying && currentAttackPoint == Vector3.zero)
            {
                currentAttackPoint = inputTargetAnchor.position;
            }
        }

        private Transform EnsureChild(Transform current, string childName, Vector3 worldPosition)
        {
            if (current != null)
            {
                current.position = worldPosition;
                return current;
            }

            var existing = transform.Find(childName);
            if (existing != null)
            {
                existing.position = worldPosition;
                return existing;
            }

            var child = new GameObject(childName).transform;
            child.SetParent(transform, false);
            child.position = worldPosition;
            return child;
        }

        private void ConfigureCamera()
        {
            if (targetCamera == null)
            {
                return;
            }

            targetCamera.orthographic = true;
            targetCamera.clearFlags = CameraClearFlags.SolidColor;
            targetCamera.backgroundColor = new Color(0.91f, 0.95f, 1f, 1f);

            var aspect = Mathf.Max(1f, targetCamera.aspect);
            var cameraPosition = targetCamera.transform.position;
            cameraPosition.x = (fieldSize.x - 1f) * 0.5f;
            cameraPosition.y = (fieldSize.y - 1f) * 0.5f;
            cameraPosition.z = -10f;
            targetCamera.transform.position = cameraPosition;

            var heightDrivenSize = (fieldSize.y * 0.5f) + 1f;
            var widthDrivenSize = (fieldSize.x / (2f * aspect)) + 0.5f;
            targetCamera.orthographicSize = Mathf.Max(heightDrivenSize, widthDrivenSize);
        }

        private void ApplyFallbackMonsterValues()
        {
            selectedMonster = null;
            selectedMonsterName = "이브";
            selectedElementLabel = "번개";
            selectedActiveSkillName = "아크 볼트";
            selectedPassiveSkillName = "전압 보정";
            selectedStatusEffectLabel = "감전";
            selectedUnitColor = new Color(0.41f, 0.78f, 1f, 0.95f);
            selectedProjectileColor = new Color(0.61f, 0.93f, 1f, 0.98f);
            unitMaxHealthConfigured = eveMaxHealth;
            powerStatConfigured = eveSpellPower;
            baseDamageConfigured = eveBaseLightningDamage;
            powerCoefficientConfigured = eveSpellPowerCoefficient;
            projectileSpeedConfigured = eveProjectileSpeed;
            projectileLifetimeConfigured = eveProjectileLifetime;
            projectileHitRadiusConfigured = eveProjectileHitRadius;
            magazineCapacityConfigured = eveMagazineCapacity;
            reloadDurationConfigured = eveReloadDuration;
            shotIntervalConfigured = eveShotInterval;
            statusChanceConfigured = eveShockChance;
        }

        private void ConfigureMonster(MonsterDefinition monster)
        {
            if (monster == null)
            {
                ApplyFallbackMonsterValues();
                EnsureAnchorVisuals();
                return;
            }

            selectedMonster = monster;
            selectedMonsterName = string.IsNullOrWhiteSpace(monster.DisplayName) ? "Unknown" : monster.DisplayName;
            selectedElementLabel = string.IsNullOrWhiteSpace(monster.ElementLabel) ? "기본" : monster.ElementLabel;
            selectedActiveSkillName = string.IsNullOrWhiteSpace(monster.ActiveSkillName) ? "기본 스킬" : monster.ActiveSkillName;
            selectedPassiveSkillName = string.IsNullOrWhiteSpace(monster.PassiveSkillName) ? string.Empty : monster.PassiveSkillName;
            selectedStatusEffectLabel = string.IsNullOrWhiteSpace(monster.StatusEffectLabel) ? string.Empty : monster.StatusEffectLabel;
            selectedUnitColor = monster.UnitColor.a <= 0f ? new Color(0.78f, 0.82f, 0.92f, 0.95f) : monster.UnitColor;
            selectedProjectileColor = monster.ProjectileColor.a <= 0f ? new Color(0.95f, 0.95f, 1f, 0.98f) : monster.ProjectileColor;
            unitMaxHealthConfigured = Mathf.Max(1f, monster.MaxHealth);
            powerStatConfigured = monster.PowerStat;
            baseDamageConfigured = Mathf.Max(1f, monster.BaseDamage);
            powerCoefficientConfigured = monster.PowerCoefficient;
            projectileSpeedConfigured = Mathf.Max(0.1f, monster.ProjectileSpeed);
            projectileLifetimeConfigured = Mathf.Max(0.1f, monster.ProjectileLifetime);
            projectileHitRadiusConfigured = Mathf.Max(0.1f, monster.ProjectileHitRadius);
            magazineCapacityConfigured = Mathf.Max(1, monster.MagazineCapacity);
            reloadDurationConfigured = Mathf.Max(0.1f, monster.ReloadDuration);
            shotIntervalConfigured = Mathf.Max(0.05f, monster.ShotInterval);
            statusChanceConfigured = Mathf.Clamp01(monster.StatusChance);
            EnsureAnchorVisuals();
        }

        private void ApplyPersistedRewardState(RunSession session)
        {
            if (session == null)
            {
                return;
            }

            baseDamageConfigured *= session.DamageMultiplier > 0f ? session.DamageMultiplier : 1f;
            magazineCapacityConfigured = Mathf.Max(1, magazineCapacityConfigured + session.MagazineBonus);
            shotIntervalConfigured = Mathf.Max(0.05f, shotIntervalConfigured * (session.ShotIntervalMultiplier > 0f ? session.ShotIntervalMultiplier : 1f));
            reloadDurationConfigured = Mathf.Max(0.25f, reloadDurationConfigured * (session.ReloadDurationMultiplier > 0f ? session.ReloadDurationMultiplier : 1f));
            unitMaxHealthConfigured = Mathf.Max(1f, unitMaxHealthConfigured + session.MaxHealthBonus);
            statusChanceConfigured = Mathf.Clamp01(statusChanceConfigured + session.StatusChanceBonus);
        }

        private void EnsureAnchorVisuals()
        {
            EnsureSpriteRenderer(nexusAnchor, nexusColor, new Vector2(1.8f, 1.8f), 15);
            EnsureSpriteRenderer(eveAnchor, selectedUnitColor, new Vector2(1.25f, 1.25f), 20);
            EnsureSpriteRenderer(enemySpawnAnchor, spawnMarkerColor, new Vector2(0.65f, 0.65f), 5);
            EnsureSpriteRenderer(inputTargetAnchor, inputMarkerColor, new Vector2(0.85f, 0.85f), 10);
        }

        private SpriteRenderer EnsureSpriteRenderer(Transform target, Color color, Vector2 size, int sortingOrder)
        {
            if (target == null)
            {
                return null;
            }

            var renderer = target.GetComponent<SpriteRenderer>();
            if (renderer == null)
            {
                renderer = target.gameObject.AddComponent<SpriteRenderer>();
            }

            renderer.sprite = GetSharedSprite();
            renderer.color = color;
            renderer.sortingOrder = sortingOrder;
            target.localScale = new Vector3(size.x, size.y, 1f);
            return renderer;
        }

        private static Sprite GetSharedSprite()
        {
            if (sharedSprite != null)
            {
                return sharedSprite;
            }

            var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.HideAndDontSave
            };
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();
            sharedSprite = Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
            sharedSprite.hideFlags = HideFlags.HideAndDontSave;
            return sharedSprite;
        }

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

            ResolveEncounterForDay(dayIndex, out pendingNormalSpawnCount, out pendingBossSpawn, out currentBossHealthMultiplier, out encounterLabel);
            spawnedNormalCount = 0;
            spawnCooldown = 0.2f;
            currentShotsRemaining = magazineCapacityConfigured;
            shotCooldown = 0f;
            reloadRemaining = 0f;
            nexusCurrentHealth = nexusMaxHealth;
            unitCurrentHealth = unitMaxHealthConfigured;
            nextProjectileSequence = 0;
            currentAttackPoint = new Vector3(Mathf.Lerp(eveAnchor.position.x, enemySpawnAnchor.position.x, 0.55f), 8f, 0f);
            inputTargetAnchor.position = currentAttackPoint;
            statusLabel = $"{selectedActiveSkillName} 목표 지점을 클릭해 전투를 시작한다.";
        }

        private void ResolveEncounterForDay(int day, out int normalSpawnCount, out bool spawnBoss, out float bossMultiplier, out string label)
        {
            label = "Fixed Combat";
            normalSpawnCount = 3;
            spawnBoss = true;
            bossMultiplier = baseBossHealthMultiplier;

            if (day == 1)
            {
                return;
            }

            if (day == 5 || day == 10)
            {
                label = "Prototype Midboss Combat";
                normalSpawnCount = 4;
                bossMultiplier = baseBossHealthMultiplier + 6f;
                return;
            }

            if (day == 11)
            {
                label = "Prototype Boss Combat";
                normalSpawnCount = 5;
                bossMultiplier = baseBossHealthMultiplier + 10f;
                return;
            }

            label = "Prototype Normal Combat";
            normalSpawnCount = 4;
            bossMultiplier = baseBossHealthMultiplier + 2f;
        }

        private void UpdateSpawning()
        {
            if (spawnedNormalCount >= pendingNormalSpawnCount && !pendingBossSpawn)
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

            if (pendingBossSpawn)
            {
                pendingBossSpawn = false;
                SpawnEnemy(true, 1);
                spawnCooldown = spawnInterval;
            }
        }

        private void SpawnEnemy(bool isBoss, int sequence)
        {
            if (enemyRoot == null || enemySpawnAnchor == null)
            {
                return;
            }

            var enemyObject = new GameObject(isBoss ? "Enemy_Boss_01" : $"Enemy_Normal_{sequence:00}");
            enemyObject.transform.SetParent(enemyRoot, false);
            enemyObject.transform.position = new Vector3(enemySpawnAnchor.position.x, UnityEngine.Random.Range(enemySpawnYRange.x, enemySpawnYRange.y), 0f);
            enemyObject.transform.localScale = isBoss ? new Vector3(1.55f, 1.55f, 1f) : new Vector3(1.05f, 1.05f, 1f);

            var renderer = enemyObject.AddComponent<SpriteRenderer>();
            renderer.sprite = GetSharedSprite();
            renderer.sortingOrder = isBoss ? 18 : 17;

            var maxHealth = normalEnemyHealth * GetStageValueMultiplier() * (isBoss ? currentBossHealthMultiplier : 1f);
            var runtime = new EnemyRuntime
            {
                GameObject = enemyObject,
                Transform = enemyObject.transform,
                Renderer = renderer,
                MaxHealth = maxHealth,
                CurrentHealth = maxHealth,
                MoveSpeed = enemyMoveSpeed * (isBoss ? 0.85f : 1f),
                ContactDamagePerSecond = enemyContactDamagePerSecond * (isBoss ? 2.5f : 1f),
                IsBoss = isBoss,
                DisplayName = isBoss ? guaranteedPrisonerName : "견습 검사"
            };

            enemies.Add(runtime);
            UpdateEnemyColor(runtime);
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

                var nexusDistance = Vector2.Distance(enemy.Transform.position, nexusAnchor.position);
                if (nexusDistance > 1.4f)
                {
                    var speedMultiplier = enemy.ShockTimer > 0f ? Mathf.Max(0.45f, 1f - (enemy.ShockStacks * 0.15f)) : 1f;
                    enemy.Transform.position = Vector3.MoveTowards(enemy.Transform.position, nexusAnchor.position, enemy.MoveSpeed * speedMultiplier * Time.deltaTime);
                }
                else
                {
                    nexusCurrentHealth = Mathf.Max(0f, nexusCurrentHealth - (enemy.ContactDamagePerSecond * Time.deltaTime));
                }

                UpdateEnemyColor(enemy);
            }
        }

        private void UpdateEnemyColor(EnemyRuntime enemy)
        {
            if (enemy == null || enemy.Renderer == null)
            {
                return;
            }

            var baseColor = enemy.IsBoss
                ? new Color(1f, 0.45f, 0.53f, 0.98f)
                : new Color(1f, 0.88f, 0.46f, 0.98f);

            if (enemy.ShockTimer > 0f)
            {
                baseColor = Color.Lerp(baseColor, new Color(0.48f, 0.91f, 1f, 1f), 0.55f);
            }

            if (enemy.FlashTimer > 0f)
            {
                baseColor = Color.white;
            }

            enemy.Renderer.color = baseColor;
        }

        private void UpdateProjectiles()
        {
            for (var i = projectiles.Count - 1; i >= 0; i--)
            {
                var projectile = projectiles[i];
                if (projectile == null || projectile.GameObject == null)
                {
                    projectiles.RemoveAt(i);
                    continue;
                }

                var travelDistance = projectile.Speed * Time.deltaTime;
                projectile.Transform.position += projectile.Direction * travelDistance;
                projectile.RemainingLifetime = Mathf.Max(0f, projectile.RemainingLifetime - Time.deltaTime);

                if (TryHitEnemy(projectile, out var enemyHit, out var damageResult))
                {
                    enemyHit.CurrentHealth -= damageResult.FinalDamage;
                    enemyHit.FlashTimer = 0.08f;

                    var appliedStatus = false;
                    if (statusChanceConfigured > 0f && UnityEngine.Random.value < statusChanceConfigured)
                    {
                        enemyHit.ShockStacks = Mathf.Min(enemyHit.ShockStacks + 1, 3);
                        enemyHit.ShockTimer = 1.25f;
                        appliedStatus = !string.IsNullOrWhiteSpace(selectedStatusEffectLabel);
                    }

                    statusLabel = appliedStatus
                        ? $"{selectedActiveSkillName} 적중: {enemyHit.DisplayName}에게 {damageResult.FinalDamage:0.0} {selectedElementLabel} 피해, {selectedStatusEffectLabel} 부여."
                        : $"{selectedActiveSkillName} 적중: {enemyHit.DisplayName}에게 {damageResult.FinalDamage:0.0} {selectedElementLabel} 피해.";

                    CleanupProjectile(i);
                    continue;
                }

                if (projectile.RemainingLifetime > 0f)
                {
                    continue;
                }

                statusLabel = $"{selectedActiveSkillName} 투사체가 {projectileLifetimeConfigured:0.0}s 후 소멸했다.";
                CleanupProjectile(i);
            }
        }

        private bool TryHitEnemy(ProjectileRuntime projectile, out EnemyRuntime enemyHit, out DamageResult damageResult)
        {
            enemyHit = null;
            damageResult = default;

            for (var i = 0; i < enemies.Count; i++)
            {
                var enemy = enemies[i];
                if (enemy == null || enemy.Transform == null || enemy.CurrentHealth <= 0f)
                {
                    continue;
                }

                var hitDistance = GetEnemyHitRadius(enemy) + projectile.HitRadius;
                if (Vector2.Distance(projectile.Transform.position, enemy.Transform.position) > hitDistance)
                {
                    continue;
                }

                enemyHit = enemy;
                damageResult = DamageCalculator.Resolve(projectile.BaseDamage, 0f);
                return true;
            }

            return false;
        }

        private static float GetEnemyHitRadius(EnemyRuntime enemy)
        {
            return enemy != null && enemy.IsBoss ? 0.95f : 0.65f;
        }

        private void UpdateSelectedMonsterCombat()
        {
            if (reloadRemaining > 0f)
            {
                reloadRemaining = Mathf.Max(0f, reloadRemaining - Time.deltaTime);
                if (Mathf.Approximately(reloadRemaining, 0f))
                {
                    currentShotsRemaining = magazineCapacityConfigured;
                    statusLabel = $"{selectedActiveSkillName} 탄창이 재장전됐다.";
                }

                return;
            }

            shotCooldown = Mathf.Max(0f, shotCooldown - Time.deltaTime);
            if (!fireRequestedThisFrame)
            {
                return;
            }

            if (currentShotsRemaining <= 0)
            {
                reloadRemaining = reloadDurationConfigured;
                currentShotsRemaining = 0;
                statusLabel = $"{selectedActiveSkillName} 탄창이 비어 재장전 중이다.";
                return;
            }

            if (shotCooldown > 0f)
            {
                statusLabel = $"{selectedActiveSkillName} 재사용 대기: {shotCooldown:0.00}s";
                return;
            }

            FirePrimarySkill();
        }

        private void FirePrimarySkill()
        {
            if (eveAnchor == null || projectileRoot == null)
            {
                return;
            }

            var direction = currentAttackPoint - eveAnchor.position;
            direction.z = 0f;
            if (direction.sqrMagnitude < 0.01f)
            {
                statusLabel = $"{selectedMonsterName} 앞쪽으로 목표 지점을 다시 지정한다.";
                return;
            }

            direction.Normalize();

            nextProjectileSequence += 1;
            var safeSkillName = selectedActiveSkillName.Replace(" ", string.Empty);
            var projectileObject = new GameObject($"{safeSkillName}_{nextProjectileSequence:00}");
            projectileObject.transform.SetParent(projectileRoot, false);
            projectileObject.transform.position = eveAnchor.position;
            projectileObject.transform.localScale = new Vector3(projectileHitRadiusConfigured, projectileHitRadiusConfigured, 1f);

            var renderer = projectileObject.AddComponent<SpriteRenderer>();
            renderer.sprite = GetSharedSprite();
            renderer.color = selectedProjectileColor;
            renderer.sortingOrder = 25;

            var projectile = new ProjectileRuntime
            {
                GameObject = projectileObject,
                Transform = projectileObject.transform,
                Renderer = renderer,
                Direction = direction,
                Speed = projectileSpeedConfigured,
                RemainingLifetime = projectileLifetimeConfigured,
                HitRadius = projectileHitRadiusConfigured,
                BaseDamage = baseDamageConfigured + (powerStatConfigured * powerCoefficientConfigured)
            };

            projectiles.Add(projectile);
            currentShotsRemaining -= 1;
            shotCooldown = shotIntervalConfigured;
            if (currentShotsRemaining <= 0)
            {
                currentShotsRemaining = 0;
                reloadRemaining = reloadDurationConfigured;
                statusLabel = $"{selectedActiveSkillName} 발사. 탄창이 비어 재장전에 들어간다.";
                return;
            }

            statusLabel = $"{selectedActiveSkillName} 발사: ({currentAttackPoint.x:0.0}, {currentAttackPoint.y:0.0})";
        }

        private void CleanupProjectile(int index)
        {
            if (index < 0 || index >= projectiles.Count)
            {
                return;
            }

            var projectile = projectiles[index];
            if (projectile != null && projectile.GameObject != null)
            {
                Destroy(projectile.GameObject);
            }

            projectiles.RemoveAt(index);
        }

        private void CheckBattleResolution()
        {
            if (nexusCurrentHealth <= 0f)
            {
                battleResolved = true;
                victory = false;
                waitingForRewardChoice = false;
                statusLabel = "Nexus가 붕괴했다. 현재 일차를 다시 시도한다.";
                return;
            }

            var allSpawnsFinished = spawnedNormalCount >= pendingNormalSpawnCount && !pendingBossSpawn;
            if (!allSpawnsFinished || enemies.Count > 0)
            {
                return;
            }

            battleResolved = true;
            victory = true;
            PrepareVictoryRewards();
        }

        private void PrepareVictoryRewards()
        {
            rewardPrisonerCount = RollPrisonerCount();
            rewardApplied = false;
            waitingForRewardChoice = false;
            appliedRewardSummary = string.Empty;
            lastAppliedRewardUnlockedPassive = false;

            switch (encounterLabel)
            {
                case "Prototype Midboss Combat":
                    rewardGold = 30;
                    rewardDarkTrace = GetScaledDarkTraceReward(20);
                    break;
                case "Prototype Boss Combat":
                    rewardGold = 50;
                    rewardDarkTrace = GetScaledDarkTraceReward(50);
                    break;
                default:
                    rewardGold = 10;
                    rewardDarkTrace = GetScaledDarkTraceReward(10);
                    break;
            }

            rewardOptions.Clear();
            if (selectedMonster != null && selectedMonster.InitialRewardChoices != null)
            {
                for (var i = 0; i < selectedMonster.InitialRewardChoices.Length; i++)
                {
                    var reward = selectedMonster.InitialRewardChoices[i];
                    if (reward == null || string.IsNullOrWhiteSpace(reward.RewardId) || blockedRewardIds.Contains(reward.RewardId))
                    {
                        continue;
                    }

                    rewardOptions.Add(new RewardOption
                    {
                        RewardId = reward.RewardId,
                        Title = reward.Title,
                        Description = reward.Description,
                        DamageMultiplier = reward.DamageMultiplier,
                        MagazineBonus = reward.MagazineBonus,
                        ShotIntervalMultiplier = reward.ShotIntervalMultiplier,
                        ReloadDurationMultiplier = reward.ReloadDurationMultiplier,
                        MaxHealthBonus = reward.MaxHealthBonus,
                        StatusChanceBonus = reward.StatusChanceBonus,
                        UnlocksPassive = !string.IsNullOrWhiteSpace(selectedPassiveSkillName) &&
                                         reward.Title.IndexOf(selectedPassiveSkillName, StringComparison.OrdinalIgnoreCase) >= 0
                    });
                }
            }

            if (rewardOptions.Count > 0)
            {
                waitingForRewardChoice = true;
                statusLabel = $"{selectedMonsterName} 보상 선택 대기 중. 현재 구현된 후보만 표시된다.";
                return;
            }

            rewardApplied = true;
            appliedRewardSummary = "현재 구현된 새 보상 후보가 남아 있지 않다. 다음 일차로 진행한다.";
            statusLabel = appliedRewardSummary;
        }

        private static int RollPrisonerCount()
        {
            var roll = UnityEngine.Random.value;
            if (roll < 0.05f)
            {
                return 1;
            }

            if (roll < 0.85f)
            {
                return 2;
            }

            return 3;
        }

        private float GetStageValueMultiplier()
        {
            switch (stageIndex)
            {
                case 2:
                    return 1.3f;
                case 3:
                    return 1.6f;
                case 4:
                    return 2.0f;
                default:
                    return 1f;
            }
        }

        private int GetScaledDarkTraceReward(int baseReward)
        {
            return Mathf.RoundToInt(baseReward * GetStageValueMultiplier());
        }

        private void HandlePointerInput()
        {
            fireRequestedThisFrame = false;

            if (targetCamera == null || battleResolved)
            {
                return;
            }

            Vector2 screenPoint = default;
            var pointerPressed = false;

#if ENABLE_INPUT_SYSTEM
            var mouse = Mouse.current;
            if (mouse != null && mouse.leftButton.wasPressedThisFrame)
            {
                screenPoint = mouse.position.ReadValue();
                pointerPressed = true;
            }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
            if (!pointerPressed && Input.GetMouseButtonDown(0))
            {
                screenPoint = Input.mousePosition;
                pointerPressed = true;
            }
#endif

            if (!pointerPressed)
            {
                return;
            }

            var world = targetCamera.ScreenToWorldPoint(new Vector3(screenPoint.x, screenPoint.y, Mathf.Abs(targetCamera.transform.position.z)));
            world.z = 0f;
            world.x = Mathf.Clamp(world.x, 0f, fieldSize.x - 1f);
            world.y = Mathf.Clamp(world.y, 0f, fieldSize.y - 1f);
            currentAttackPoint = world;
            fireRequestedThisFrame = true;
        }

        private void UpdateMarkerPosition()
        {
            if (inputTargetAnchor != null)
            {
                inputTargetAnchor.position = currentAttackPoint;
            }
        }

        private void ClearEnemyRuntime()
        {
            for (var i = enemies.Count - 1; i >= 0; i--)
            {
                var enemy = enemies[i];
                if (enemy != null && enemy.GameObject != null)
                {
                    if (Application.isPlaying)
                    {
                        Destroy(enemy.GameObject);
                    }
                    else
                    {
                        DestroyImmediate(enemy.GameObject);
                    }
                }
            }

            enemies.Clear();
        }

        private void ClearProjectileRuntime()
        {
            for (var i = projectiles.Count - 1; i >= 0; i--)
            {
                var projectile = projectiles[i];
                if (projectile == null || projectile.GameObject == null)
                {
                    continue;
                }

                if (Application.isPlaying)
                {
                    Destroy(projectile.GameObject);
                }
                else
                {
                    DestroyImmediate(projectile.GameObject);
                }
            }

            projectiles.Clear();
        }

        private void DrawHud()
        {
            GUILayout.BeginArea(new Rect(14f, 14f, 380f, 220f), GUI.skin.window);
            GUILayout.Label($"Monster: {selectedMonsterName}");
            GUILayout.Label($"Skill A: {selectedActiveSkillName}");
            GUILayout.Label($"Stage {stageIndex} / Day {dayIndex}");
            GUILayout.Label($"Encounter: {encounterLabel}");
            GUILayout.Label($"Nexus HP: {nexusCurrentHealth:0} / {nexusMaxHealth:0}");
            GUILayout.Label($"Unit HP: {unitCurrentHealth:0} / {unitMaxHealthConfigured:0}");
            GUILayout.Label($"Magazine: {currentShotsRemaining} / {magazineCapacityConfigured}");
            GUILayout.Label(reloadRemaining > 0f
                ? $"Reloading: {reloadRemaining:0.00}s"
                : $"Shot Interval: {shotIntervalConfigured:0.00}s");
            GUILayout.Label($"Projectiles Alive: {projectiles.Count}");
            GUILayout.Label($"Enemies Alive: {enemies.Count}");
            GUILayout.Label($"Focus: ({currentAttackPoint.x:0.0}, {currentAttackPoint.y:0.0})");
            GUILayout.Space(6f);
            GUILayout.Label(statusLabel);
            GUILayout.EndArea();
        }

        private void DrawVictoryPanel()
        {
            GUILayout.BeginArea(new Rect(Screen.width * 0.5f - 240f, 80f, 480f, 400f), GUI.skin.window);
            GUILayout.Label("Victory");
            GUILayout.Label($"Reward Gold: {rewardGold}");
            GUILayout.Label($"Dark Trace: {rewardDarkTrace}");
            GUILayout.Label($"Prisoners: {rewardPrisonerCount} (Boss prisoner guaranteed: {guaranteedPrisonerName})");
            GUILayout.Space(10f);

            if (waitingForRewardChoice)
            {
                GUILayout.Label($"Choose one {selectedMonsterName} reward to continue the prototype loop.");
                for (var i = 0; i < rewardOptions.Count; i++)
                {
                    var option = rewardOptions[i];
                    if (GUILayout.Button(option.Title + "\n" + option.Description, GUILayout.Height(58f)))
                    {
                        ApplyRewardChoice(i);
                    }
                }
            }
            else
            {
                GUILayout.Label(rewardApplied ? appliedRewardSummary : "Reward choice pending.");
                GUILayout.Space(8f);
                if (GUILayout.Button("Next Prototype Day", GUILayout.Height(34f)))
                {
                    BeginPrototypeDay(dayIndex + 1);
                }

                if (GUILayout.Button("Replay Current Day", GUILayout.Height(30f)))
                {
                    BeginPrototypeDay(dayIndex);
                }
            }

            GUILayout.EndArea();
        }

        private void DrawDefeatPanel()
        {
            GUILayout.BeginArea(new Rect(Screen.width * 0.5f - 180f, 100f, 360f, 180f), GUI.skin.window);
            GUILayout.Label("Defeat");
            GUILayout.Label("The prototype battle ends when the Nexus HP reaches zero.");
            GUILayout.Space(8f);
            if (GUILayout.Button("Retry Day", GUILayout.Height(34f)))
            {
                BeginPrototypeDay(dayIndex);
            }
            GUILayout.EndArea();
        }
    }
}
