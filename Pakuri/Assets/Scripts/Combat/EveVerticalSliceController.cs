using System;
using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Pakuri.Combat
{
    [ExecuteAlways]
    public class EveVerticalSliceController : MonoBehaviour
    {
        private enum RewardType
        {
            ArcBoltPower,
            ArcBoltMagazine,
            ArcBoltTempo
        }

        [Serializable]
        private sealed class RewardOption
        {
            public RewardType Type;
            public string Title;
            public string Description;
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

        [Header("Eve Defaults")]
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

        private readonly Color nexusColor = new Color(1f, 0.77f, 0.35f, 0.95f);
        private readonly Color eveColor = new Color(0.41f, 0.78f, 1f, 0.95f);
        private readonly Color spawnMarkerColor = new Color(1f, 0.38f, 0.35f, 0.45f);
        private readonly Color inputMarkerColor = new Color(0.60f, 0.42f, 1f, 0.35f);
        private readonly Color projectileColor = new Color(0.61f, 0.93f, 1f, 0.98f);

        private float nexusMaxHealth = 500f;
        private float nexusCurrentHealth;
        private float eveCurrentHealth;
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
        private int rewardGold;
        private int rewardDarkTrace;
        private int rewardPrisonerCount;
        private int nextProjectileSequence;
        private string guaranteedPrisonerName = "강화된 견습 검사";
        private string encounterLabel = "Fixed Combat";
        private string statusLabel = "Prototype ready.";
        private string appliedRewardSummary = string.Empty;

        private void OnEnable()
        {
            ResolveSceneReferences();
            ConfigureCamera();
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

            BeginPrototypeDay(dayIndex);
        }

        private void Update()
        {
            if (!Application.isPlaying)
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
            UpdateEveCombat();
            CheckBattleResolution();
        }

        private void OnGUI()
        {
            if (!Application.isPlaying)
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

        private void EnsureAnchorVisuals()
        {
            EnsureSpriteRenderer(nexusAnchor, nexusColor, new Vector2(1.8f, 1.8f), 15);
            EnsureSpriteRenderer(eveAnchor, eveColor, new Vector2(1.25f, 1.25f), 20);
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
            fireRequestedThisFrame = false;
            rewardOptions.Clear();
            ClearEnemyRuntime();
            ClearProjectileRuntime();

            ResolveEncounterForDay(dayIndex, out pendingNormalSpawnCount, out pendingBossSpawn, out currentBossHealthMultiplier, out encounterLabel);
            spawnedNormalCount = 0;
            spawnCooldown = 0.2f;
            currentShotsRemaining = eveMagazineCapacity;
            shotCooldown = 0f;
            reloadRemaining = 0f;
            nexusCurrentHealth = nexusMaxHealth;
            eveCurrentHealth = eveMaxHealth;
            nextProjectileSequence = 0;
            currentAttackPoint = new Vector3(Mathf.Lerp(eveAnchor.position.x, enemySpawnAnchor.position.x, 0.55f), 8f, 0f);
            inputTargetAnchor.position = currentAttackPoint;
            statusLabel = "Click to launch Arc Bolt toward the selected point.";
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

            var enemyObject = new GameObject(isBoss ? "Enemy_Boss_01" : string.Format("Enemy_Normal_{0:00}", sequence));
            enemyObject.transform.SetParent(enemyRoot, false);
            enemyObject.transform.position = new Vector3(enemySpawnAnchor.position.x, UnityEngine.Random.Range(enemySpawnYRange.x, enemySpawnYRange.y), 0f);
            enemyObject.transform.localScale = isBoss ? new Vector3(1.55f, 1.55f, 1f) : new Vector3(1.05f, 1.05f, 1f);

            var renderer = enemyObject.AddComponent<SpriteRenderer>();
            renderer.sprite = GetSharedSprite();
            renderer.sortingOrder = isBoss ? 18 : 17;

            var maxHealth = normalEnemyHealth * (isBoss ? currentBossHealthMultiplier : 1f);
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

                    if (UnityEngine.Random.value < eveShockChance)
                    {
                        enemyHit.ShockStacks = Mathf.Min(enemyHit.ShockStacks + 1, 3);
                        enemyHit.ShockTimer = 1.25f;
                    }

                    statusLabel = string.Format(
                        "Arc Bolt hit {0} for {1:0.0} lightning damage{2}.",
                        enemyHit.DisplayName,
                        damageResult.FinalDamage,
                        damageResult.IsCritical ? " (CRIT)" : string.Empty);

                    CleanupProjectile(i);
                    continue;
                }

                if (projectile.RemainingLifetime > 0f)
                {
                    continue;
                }

                statusLabel = string.Format(
                    "Arc Bolt dissipated after {0:0.0}s without hitting a target.",
                    eveProjectileLifetime);
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

        private float GetEnemyHitRadius(EnemyRuntime enemy)
        {
            return enemy != null && enemy.IsBoss ? 0.95f : 0.65f;
        }

        private void UpdateEveCombat()
        {
            if (reloadRemaining > 0f)
            {
                reloadRemaining = Mathf.Max(0f, reloadRemaining - Time.deltaTime);
                if (Mathf.Approximately(reloadRemaining, 0f))
                {
                    currentShotsRemaining = eveMagazineCapacity;
                    statusLabel = "Arc Bolt magazine reloaded.";
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
                reloadRemaining = eveReloadDuration;
                currentShotsRemaining = 0;
                statusLabel = "Arc Bolt magazine empty. Reloading.";
                return;
            }

            if (shotCooldown > 0f)
            {
                statusLabel = string.Format("Arc Bolt cooling down: {0:0.00}s", shotCooldown);
                return;
            }

            FireArcBolt();
        }

        private void FireArcBolt()
        {
            if (eveAnchor == null || projectileRoot == null)
            {
                return;
            }

            var direction = currentAttackPoint - eveAnchor.position;
            direction.z = 0f;
            if (direction.sqrMagnitude < 0.01f)
            {
                statusLabel = "Choose a point away from Eve to fire Arc Bolt.";
                return;
            }

            direction.Normalize();

            nextProjectileSequence += 1;
            var projectileObject = new GameObject(string.Format("ArcBolt_{0:00}", nextProjectileSequence));
            projectileObject.transform.SetParent(projectileRoot, false);
            projectileObject.transform.position = eveAnchor.position;
            projectileObject.transform.localScale = new Vector3(0.42f, 0.42f, 1f);

            var renderer = projectileObject.AddComponent<SpriteRenderer>();
            renderer.sprite = GetSharedSprite();
            renderer.color = projectileColor;
            renderer.sortingOrder = 25;

            var projectile = new ProjectileRuntime
            {
                GameObject = projectileObject,
                Transform = projectileObject.transform,
                Renderer = renderer,
                Direction = direction,
                Speed = eveProjectileSpeed,
                RemainingLifetime = eveProjectileLifetime,
                HitRadius = eveProjectileHitRadius,
                BaseDamage = eveBaseLightningDamage + (eveSpellPower * eveSpellPowerCoefficient)
            };

            projectiles.Add(projectile);
            currentShotsRemaining -= 1;
            shotCooldown = eveShotInterval;
            if (currentShotsRemaining <= 0)
            {
                currentShotsRemaining = 0;
                reloadRemaining = eveReloadDuration;
                statusLabel = string.Format(
                    "Arc Bolt launched toward ({0:0.0}, {1:0.0}). Magazine empty. Reloading.",
                    currentAttackPoint.x,
                    currentAttackPoint.y);
                return;
            }

            statusLabel = string.Format(
                "Arc Bolt launched toward ({0:0.0}, {1:0.0}).",
                currentAttackPoint.x,
                currentAttackPoint.y);
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
                statusLabel = "Nexus collapsed. Restart the prototype day.";
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
            waitingForRewardChoice = true;
            rewardApplied = false;
            appliedRewardSummary = string.Empty;

            switch (encounterLabel)
            {
                case "Prototype Midboss Combat":
                    rewardGold = 30;
                    rewardDarkTrace = 20;
                    break;
                case "Prototype Boss Combat":
                    rewardGold = 50;
                    rewardDarkTrace = 50;
                    break;
                default:
                    rewardGold = 10;
                    rewardDarkTrace = 10;
                    break;
            }

            rewardOptions.Clear();
            rewardOptions.Add(new RewardOption
            {
                Type = RewardType.ArcBoltPower,
                Title = "아크 볼트 증폭",
                Description = "아크 볼트 기본 피해 +20%, 주문력 +5"
            });
            rewardOptions.Add(new RewardOption
            {
                Type = RewardType.ArcBoltMagazine,
                Title = "축전지 확장",
                Description = "탄창 +2, 보스 포커스 유지력 증가"
            });
            rewardOptions.Add(new RewardOption
            {
                Type = RewardType.ArcBoltTempo,
                Title = "쿨링 최적화",
                Description = "발사 간격 -10%, 재장전 -10%"
            });

            statusLabel = "Victory. Choose one Eve upgrade to continue the prototype loop.";
        }

        private int RollPrisonerCount()
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

        private void ApplyReward(RewardType rewardType)
        {
            switch (rewardType)
            {
                case RewardType.ArcBoltPower:
                    eveBaseLightningDamage *= 1.2f;
                    eveSpellPower += 5f;
                    appliedRewardSummary = "아크 볼트 증폭 적용: 기본 피해 +20%, 주문력 +5";
                    break;
                case RewardType.ArcBoltMagazine:
                    eveMagazineCapacity += 2;
                    appliedRewardSummary = "축전지 확장 적용: 탄창 +2";
                    break;
                case RewardType.ArcBoltTempo:
                    eveShotInterval *= 0.9f;
                    eveReloadDuration *= 0.9f;
                    appliedRewardSummary = "쿨링 최적화 적용: 발사 간격 -10%, 재장전 -10%";
                    break;
            }

            rewardApplied = true;
            waitingForRewardChoice = false;
            statusLabel = appliedRewardSummary;
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
            GUILayout.BeginArea(new Rect(14f, 14f, 360f, 196f), GUI.skin.window);
            GUILayout.Label(string.Format("Stage {0} / Day {1}", stageIndex, dayIndex));
            GUILayout.Label(string.Format("Encounter: {0}", encounterLabel));
            GUILayout.Label(string.Format("Nexus HP: {0:0} / {1:0}", nexusCurrentHealth, nexusMaxHealth));
            GUILayout.Label(string.Format("Eve HP: {0:0} / {1:0}", eveCurrentHealth, eveMaxHealth));
            GUILayout.Label(string.Format("Arc Bolt Magazine: {0} / {1}", currentShotsRemaining, eveMagazineCapacity));
            GUILayout.Label(reloadRemaining > 0f
                ? string.Format("Reloading: {0:0.00}s", reloadRemaining)
                : string.Format("Shot Interval: {0:0.00}s", eveShotInterval));
            GUILayout.Label(string.Format("Projectiles Alive: {0}", projectiles.Count));
            GUILayout.Label(string.Format("Enemies Alive: {0}", enemies.Count));
            GUILayout.Label(string.Format("Focus: ({0:0.0}, {1:0.0})", currentAttackPoint.x, currentAttackPoint.y));
            GUILayout.Space(6f);
            GUILayout.Label(statusLabel);
            GUILayout.EndArea();
        }

        private void DrawVictoryPanel()
        {
            GUILayout.BeginArea(new Rect(Screen.width * 0.5f - 230f, 80f, 460f, 360f), GUI.skin.window);
            GUILayout.Label("Victory");
            GUILayout.Label(string.Format("Reward Gold: {0}", rewardGold));
            GUILayout.Label(string.Format("Dark Trace: {0}", rewardDarkTrace));
            GUILayout.Label(string.Format("Prisoners: {0} (Boss prisoner guaranteed: {1})", rewardPrisonerCount, guaranteedPrisonerName));
            GUILayout.Space(10f);

            if (waitingForRewardChoice)
            {
                GUILayout.Label("Choose one Eve reward to continue the prototype loop.");
                for (var i = 0; i < rewardOptions.Count; i++)
                {
                    var option = rewardOptions[i];
                    if (GUILayout.Button(option.Title + "\n" + option.Description, GUILayout.Height(58f)))
                    {
                        ApplyReward(option.Type);
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
