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
    public partial class CombatRuntimeController : MonoBehaviour
    {
        [Serializable]
        private sealed class RewardOption
        {
            public string RewardId;
            public string RewardKind;
            public string Title;
            public string Description;
            public string PrisonerName;
            public int GoldAmount;
            public int DarkTraceAmount;
            public bool Claimed;
        }

        private sealed class EnemyRuntime
        {
            public GameObject GameObject;
            public Transform Transform;
            public SpriteRenderer Renderer;
            public TextMesh Label;
            public SpriteRenderer HpBarFill;
            public EnemyDefinition Definition;
            public AttributeDefenseSet Defenses;
            public float MaxHealth;
            public float CurrentHealth;
            public float MoveSpeed;
            public float ContactDamagePerSecond;
            public float AttackPower;
            public float SpellPower;
            public float CriticalChanceBonus;
            public float CriticalMultiplierBonus;
            public float CriticalResistance;
            public float DamageMultiplier = 1f;
            public float HealingMultiplier = 1f;
            public float DamageTakenMultiplier = 1f;
            public float DamageReductionTimer;
            public float ShieldValue;
            public SpriteRenderer ShieldBarFill;
            public float ActiveCooldownRemaining;
            public float AttackBuffTimer;
            public float AttackBuffMultiplier = 1f;
            public float MoveSpeedBuffTimer;
            public float MoveSpeedBuffMultiplier = 1f;
            public bool IsBoss;
            public float ShockTimer;
            public int ShockStacks;
            public float ChillTimer;
            public int ChillStacks;
            public float FreezeTimer;
            public int VulnerableStacks;
            public float SlowTimer;
            public float SlowMultiplier = 1f;
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
            public DamageAttribute Attribute;
            public string SkillId;
            public int RemainingPierce;
            public int StatusStacks;
            public float StatusChance;
            public float BranchChance;
            public float BranchRadius;
            public float BranchDamageMultiplier = 1f;
            public int BranchTargetCount;
            public readonly HashSet<EnemyRuntime> HitEnemies = new HashSet<EnemyRuntime>();
            public bool IsEnemyProjectile;
            public EnemyRuntime SourceEnemy;
            public Transform TargetTransform;
            public bool TargetsMonster;
        }

        private sealed class SkillEffectRuntime
        {
            public GameObject GameObject;
            public Transform Transform;
            public SpriteRenderer Renderer;
            public string SkillId;
            public float RemainingDuration;
            public float TickRemaining;
            public float TickInterval;
            public float BaseDamage;
            public DamageAttribute Attribute;
            public Vector3 Origin;
            public Vector3 Direction;
            public float Length;
            public float Width;
            public float Radius;
            public int StatusStacks;
            public float FreezeDuration;
            public float SlowChance;
            public float SlowDuration;
            public readonly HashSet<EnemyRuntime> HitThisTick = new HashSet<EnemyRuntime>();
        }

        private sealed class DroneRuntime
        {
            public GameObject GameObject;
            public Transform Transform;
            public SpriteRenderer Renderer;
            public float RemainingDuration;
            public float AttackRemaining;
            public float AttackPeriod;
            public float Range;
            public float BaseDamage;
            public DamageAttribute Attribute;
            public string SkillId;
            public int VulnerableStacks;
        }

        public readonly struct RewardChoiceView
        {
            public RewardChoiceView(
                string rewardId,
                string rewardKind,
                string title,
                string description,
                string prisonerName,
                int goldAmount,
                int darkTraceAmount,
                bool claimed)
            {
                RewardId = rewardId;
                RewardKind = rewardKind;
                Title = title;
                Description = description;
                PrisonerName = prisonerName;
                GoldAmount = goldAmount;
                DarkTraceAmount = darkTraceAmount;
                Claimed = claimed;
            }

            public string RewardId { get; }
            public string RewardKind { get; }
            public string Title { get; }
            public string Description { get; }
            public string PrisonerName { get; }
            public int GoldAmount { get; }
            public int DarkTraceAmount { get; }
            public bool Claimed { get; }
        }

        [Header("Scene References")]
        [SerializeField] private Camera targetCamera;
        [SerializeField] private GameDataCatalog gameDataCatalog;
        [SerializeField] private Transform nexusAnchor;
        [SerializeField] private Transform eveAnchor;
        [SerializeField] private Transform enemySpawnAnchor;
        [SerializeField] private Transform inputTargetAnchor;
        [SerializeField] private Transform enemyRoot;
        [SerializeField] private Transform projectileRoot;

        [Header("Battlefield")]
        [SerializeField] private Vector2 fieldSize = new Vector2(32f, 18f);
        [SerializeField] private Vector2 enemySpawnYRange = new Vector2(0f, 17f);

        [Header("Battlefield Visuals")]
        [SerializeField] private Transform battlefieldBackgroundAnchor;
        [SerializeField] private Sprite battlefieldBackgroundSprite;
        [SerializeField] private Color battlefieldBackgroundColor = Color.white;
        [SerializeField] private bool autoFitBattlefieldBackgroundToField;

        [Header("Nexus Visuals")]
        [SerializeField] private Sprite nexusSprite;
        [SerializeField] private Color nexusColor = Color.white;

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
        private static Sprite sharedCircleSprite;
        private const float BattlefieldMinY = 0f;
        private const float BattlefieldMaxY = 17f;
        private const float EnemySpawnX = 33f;
        private const float BossSpawnY = 8f;
        private static readonly Vector3 DefaultEnemySpawnPosition = new Vector3(EnemySpawnX, BossSpawnY, 0f);
        private static readonly Dictionary<string, EnemyDefinition> fallbackStageOneEnemyCache = new Dictionary<string, EnemyDefinition>(StringComparer.OrdinalIgnoreCase);

        private readonly List<EnemyRuntime> enemies = new List<EnemyRuntime>();
        private readonly List<ProjectileRuntime> projectiles = new List<ProjectileRuntime>();
        private readonly List<SkillEffectRuntime> skillEffects = new List<SkillEffectRuntime>();
        private readonly List<DroneRuntime> drones = new List<DroneRuntime>();
        private readonly List<RewardOption> rewardOptions = new List<RewardOption>();
        private readonly HashSet<string> blockedRewardIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> learnedActiveSkillNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> learnedPassiveSkillNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> chosenSkillChoiceIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private readonly List<EnemyDefinition> currentNormalEnemyPool = new List<EnemyDefinition>();
        private readonly List<EnemyDefinition> currentGuaranteedPrisonerDefinitions = new List<EnemyDefinition>();
        private readonly List<string> rewardPrisonerNames = new List<string>();

        private readonly Color spawnMarkerColor = Color.white;
        private readonly Color inputMarkerColor = Color.white;

        private float nexusMaxHealth = 500f;
        private float nexusCurrentHealth;
        private float unitCurrentHealth;
        private float unitShieldValue;
        private float unitShieldTimer;
        private float currentBossHealthMultiplier;
        private int pendingNormalSpawnCount;
        private int spawnedNormalCount;
        private bool pendingBossSpawn;
        private int pendingBossSpawnCount;
        private int spawnedBossCount;
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
        private RunCombatType currentCombatType = RunCombatType.Normal;

        private MonsterDefinition selectedMonster;
        private string selectedMonsterName = "이브";
        private string selectedElementLabel = "번개";
        private string selectedActiveSkillName = "아크 볼트";
        private string selectedPassiveSkillName = "전압 보정";
        private string selectedStatusEffectLabel = "감전";
        private DamageAttribute selectedDamageAttribute = DamageAttribute.Lightning;
        private AttributeDefenseSet selectedMonsterDefenses = new AttributeDefenseSet();
        private TextMesh selectedMonsterLabel;
        private SpriteRenderer selectedMonsterHpBarFill;
        private SpriteRenderer selectedMonsterShieldBarFill;
        private Color selectedUnitColor = new Color(0.41f, 0.78f, 1f, 0.95f);
        private Color selectedProjectileColor = new Color(0.61f, 0.93f, 1f, 0.98f);
        private Sprite selectedUnitSprite;
        private Sprite selectedProjectileSprite;
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
        private EnemyDefinition currentMidbossDefinition;
        private EnemyDefinition currentDay5MidbossDefinition;
        private EnemyDefinition currentDay10MidbossDefinition;
        private EnemyDefinition currentBossDefinition;
        private EnemyDefinition currentNormalBossDefinition;

        public bool HasActiveRun => runInitialized;
        public bool IsBattleResolved => battleResolved;
        public bool IsVictory => victory;
        public bool IsWaitingForRewardChoice => waitingForRewardChoice;
        public int RewardGold => rewardGold;
        public int RewardDarkTrace => rewardDarkTrace;
        public int RewardPrisonerCount => rewardPrisonerCount;
        public string GuaranteedPrisonerName => guaranteedPrisonerName;
        public string RewardPrisonerSummary => string.Join(", ", rewardPrisonerNames);
        public string EncounterLabel => encounterLabel;
        public RunCombatType CurrentCombatType => currentCombatType;
        public string StatusLabel => statusLabel;
        public string AppliedRewardSummary => appliedRewardSummary;
        public string SelectedMonsterName => selectedMonsterName;
        public string SelectedMonsterPassiveName => selectedPassiveSkillName;
        public float NexusMaxHealth => nexusMaxHealth;
        public float NexusCurrentHealth => nexusCurrentHealth;
        public float UnitMaxHealth => unitMaxHealthConfigured;
        public float UnitCurrentHealth => unitCurrentHealth;
        public int CurrentShotsRemaining => currentShotsRemaining;
        public int MagazineCapacity => GetEveArcMagazineCapacity();
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
            var minY = Mathf.Clamp(Mathf.Min(enemySpawnYRange.x, enemySpawnYRange.y), BattlefieldMinY, BattlefieldMaxY);
            var maxY = Mathf.Clamp(Mathf.Max(enemySpawnYRange.x, enemySpawnYRange.y), BattlefieldMinY, BattlefieldMaxY);
            enemySpawnYRange = new Vector2(minY, maxY);
            stageIndex = Mathf.Clamp(stageIndex, 1, 4);
            dayIndex = Mathf.Clamp(dayIndex, 1, 11);
            eveProjectileSpeed = Mathf.Max(0.1f, eveProjectileSpeed);
            eveProjectileLifetime = Mathf.Max(0.1f, eveProjectileLifetime);
            eveProjectileHitRadius = Mathf.Max(0.1f, eveProjectileHitRadius);

            if (!Application.isPlaying || !isActiveAndEnabled)
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
                UpdateSelectedMonsterStatusVisuals();
                return;
            }

            UpdateSpawning();
            UpdateEnemies();
            UpdateProjectiles();
            UpdateEveSkillEffects();
            UpdateSelectedMonsterCombat();
            UpdateSelectedMonsterStatusVisuals();
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

        public void BeginConfiguredDay(MonsterDefinition monster, RunSession session, GameDataCatalog catalog = null)
        {
            if (session == null)
            {
                return;
            }

            if (catalog != null)
            {
                gameDataCatalog = catalog;
            }

            stageIndex = Mathf.Clamp(session.StageIndex, 1, 4);
            session.RefreshDayModel();
            currentCombatType = session.CurrentCombatType;
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

            ConfigureEveSkillSelectionState(session);

            runInitialized = true;
            BeginPrototypeDay(session.DayIndex);
        }

        public void ApplyDebugSelection(MonsterDefinition monster, RunSession session, GameDataCatalog catalog = null)
        {
            if (session == null)
            {
                return;
            }

            if (catalog != null)
            {
                gameDataCatalog = catalog;
            }

            ConfigureMonster(monster);
            ApplyPersistedRewardState(session);
            ConfigureEveSkillSelectionState(session);

            var magazineCapacity = GetEveArcMagazineCapacity();
            if (currentShotsRemaining > magazineCapacity)
            {
                currentShotsRemaining = magazineCapacity;
            }

            if (!runInitialized)
            {
                unitCurrentHealth = unitMaxHealthConfigured;
                nexusCurrentHealth = nexusMaxHealth;
                currentShotsRemaining = magazineCapacity;
            }

            UpdateSelectedMonsterStatusVisuals();
            statusLabel = $"{selectedMonsterName} debug skill selection updated.";
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
            ClearEveSkillRuntimeObjects();
        }
    }
}
