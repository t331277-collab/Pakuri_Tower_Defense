using System;
using System.Collections;
using Pakuri.Data;
using Pakuri.Run;
using UnityEngine;

namespace Pakuri.InGame
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(InGameCombatManager))]
    public sealed class NewRunSceneEntryManager : MonoBehaviour
    {
        private const string ArielMonsterId = "ariel";
        private const string EveMonsterId = "eve";
        private const string RinMonsterId = "rin";
        private const string SeinMonsterId = "sein";
        private const string VegaMonsterId = "vega";
        private const string DefaultInitialEnemyId = "stage1-swordsman";
        private const string DefaultShieldEnemyId = "stage1-shieldbearer";
        private const string DefaultRangedEnemyId = "stage1-rogue";
        private const string DefaultBufferEnemyId = "stage1-priest";
        private const string DefaultGuardianCaptainEnemyId = "stage1-guardian-captain";
        private const string DefaultAttackCaptainEnemyId = "stage1-attack-captain";
        private const string DefaultHeroKarinEnemyId = "stage1-hero-karin";
        private const string RuntimeObjectRootName = "RunTimeObject";
        private const string RuntimeEnemyRootName = "RunTimeEnemy";
        private const string RuntimeMonsterRootName = "RunTimeMonster";

        private readonly UnitFactory unitFactory = new UnitFactory();

        [SerializeField] private InGameCombatManager combatManager;
        [SerializeField] private Transform playerSpawnPoint;
        [SerializeField] private Transform enemySpawnPoint;
        [SerializeField] private Transform runtimeObjectRoot;
        [SerializeField] private Transform runtimeEnemyRoot;
        [SerializeField] private Transform runtimeMonsterRoot;
        [SerializeField] private GameDataCatalog fallbackCatalog;
        [SerializeField] private GameObject arielUnitPrefab;
        [SerializeField] private GameObject eveUnitPrefab;
        [SerializeField] private GameObject rinUnitPrefab;
        [SerializeField] private GameObject seinUnitPrefab;
        [SerializeField] private GameObject vegaUnitPrefab;
        [SerializeField] private GameObject stageOneEnemyPrefab;
        [SerializeField] private GameObject stageOneShieldEnemyPrefab;
        [SerializeField] private GameObject stageOneRangedEnemyPrefab;
        [SerializeField] private GameObject stageOneBufferEnemyPrefab;
        [SerializeField] private GameObject stageOneGuardianCaptainPrefab;
        [SerializeField] private GameObject stageOneAttackCaptainPrefab;
        [SerializeField] private GameObject stageOneHeroKarinPrefab;
        [SerializeField] private string initialEnemyId = DefaultInitialEnemyId;
        [SerializeField] private string shieldEnemyId = DefaultShieldEnemyId;
        [SerializeField] private string rangedEnemyId = DefaultRangedEnemyId;
        [SerializeField] private string bufferEnemyId = DefaultBufferEnemyId;
        [SerializeField] private string guardianCaptainEnemyId = DefaultGuardianCaptainEnemyId;
        [SerializeField] private string attackCaptainEnemyId = DefaultAttackCaptainEnemyId;
        [SerializeField] private string heroKarinEnemyId = DefaultHeroKarinEnemyId;
        [SerializeField] private float enemySpawnMinY = -5f;
        [SerializeField] private float enemySpawnMaxY = 5f;
        [SerializeField] private float enemySpawnIntervalSeconds = 1f;
        [SerializeField] private bool spawnInitialEnemySequenceOnStart;
        [SerializeField] private bool allowEveFallback = true;

        private GameObject spawnedPlayerUnit;
        private GameObject spawnedEnemyUnit;
        private GameObject spawnedShieldEnemyUnit;
        private GameObject spawnedRangedEnemyUnit;
        private GameObject spawnedBufferEnemyUnit;
        private GameObject spawnedGuardianCaptainEnemyUnit;
        private GameObject spawnedAttackCaptainEnemyUnit;
        private GameObject spawnedHeroKarinEnemyUnit;
        private Coroutine enemySpawnSequence;

        public MonsterUnitActor SpawnedPlayerActor { get; private set; }
        public MonsterUnitRuntimeModel SpawnedPlayerModel { get; private set; }
        public EnemyUnitActor SpawnedEnemyActor { get; private set; }
        public EnemyUnitRuntimeModel SpawnedEnemyModel { get; private set; }
        public RunSession ActiveSession { get; private set; }
        public InGameCombatManager CombatManager => combatManager;

        private void Start()
        {
            ResolveCombatManager();
            SpawnSelectedPlayerUnit();
            if (spawnInitialEnemySequenceOnStart)
            {
                enemySpawnSequence = StartCoroutine(SpawnInitialEnemySequence());
            }
        }

        public void SpawnSelectedPlayerUnit()
        {
            if (spawnedPlayerUnit != null)
            {
                return;
            }

            ResolveSpawnPoint();

            var selectedMonsterId = NewRunStartContext.HasPendingRun
                ? NewRunStartContext.SelectedMonsterId
                : (allowEveFallback ? EveMonsterId : string.Empty);

            if (string.IsNullOrWhiteSpace(selectedMonsterId))
            {
                Debug.LogWarning("NewRunSceneEntryManager started without selected monster data.");
                return;
            }

            var prefab = ResolvePrefab(selectedMonsterId);
            if (prefab == null)
            {
                Debug.LogWarning($"No NewRunScene prefab is configured for selected monster '{selectedMonsterId}'.");
                return;
            }

            if (!TryCreateSelectedModel(selectedMonsterId, out var model))
            {
                return;
            }

            var spawnPosition = playerSpawnPoint != null ? playerSpawnPoint.position : Vector3.zero;
            var spawnRotation = playerSpawnPoint != null ? playerSpawnPoint.rotation : Quaternion.identity;
            spawnedPlayerUnit = Instantiate(prefab, spawnPosition, spawnRotation, ResolveRuntimeMonsterRoot());
            spawnedPlayerUnit.name = $"{prefab.name}_1P";
            SpawnedPlayerModel = model;
            BindSpawnedActor(spawnedPlayerUnit, model);
            RegisterSpawnedPlayer();

            NewRunStartContext.Clear();
        }

        public void SpawnInitialEnemyUnit()
        {
            if (spawnedEnemyUnit != null)
            {
                return;
            }

            TrySpawnEnemyUnit(stageOneEnemyPrefab, initialEnemyId, 0, out spawnedEnemyUnit);
        }

        public void SpawnRangedEnemyUnit()
        {
            if (spawnedRangedEnemyUnit != null)
            {
                return;
            }

            TrySpawnEnemyUnit(stageOneRangedEnemyPrefab, rangedEnemyId, 2, out spawnedRangedEnemyUnit);
        }

        public void SpawnShieldEnemyUnit()
        {
            if (spawnedShieldEnemyUnit != null)
            {
                return;
            }

            TrySpawnEnemyUnit(stageOneShieldEnemyPrefab, shieldEnemyId, 1, out spawnedShieldEnemyUnit);
        }

        public void SpawnBufferEnemyUnit()
        {
            if (spawnedBufferEnemyUnit != null)
            {
                return;
            }

            TrySpawnEnemyUnit(stageOneBufferEnemyPrefab, bufferEnemyId, 3, out spawnedBufferEnemyUnit);
        }

        public void SpawnGuardianCaptainEnemyUnit()
        {
            if (spawnedGuardianCaptainEnemyUnit != null)
            {
                return;
            }

            TrySpawnEnemyUnit(stageOneGuardianCaptainPrefab, guardianCaptainEnemyId, 4, out spawnedGuardianCaptainEnemyUnit);
        }

        public void SpawnAttackCaptainEnemyUnit()
        {
            if (spawnedAttackCaptainEnemyUnit != null)
            {
                return;
            }

            TrySpawnEnemyUnit(stageOneAttackCaptainPrefab, attackCaptainEnemyId, 5, out spawnedAttackCaptainEnemyUnit);
        }

        public void SpawnHeroKarinEnemyUnit()
        {
            if (spawnedHeroKarinEnemyUnit != null)
            {
                return;
            }

            TrySpawnEnemyUnit(stageOneHeroKarinPrefab, heroKarinEnemyId, 6, out spawnedHeroKarinEnemyUnit);
        }

        public bool SpawnEnemyById(
            string enemyId,
            int spawnIndex,
            float spawnX,
            float spawnYMin,
            float spawnYMax,
            out GameObject spawnedUnit)
        {
            return SpawnEnemyById(enemyId, spawnIndex, spawnX, spawnYMin, spawnYMax, 1f, out spawnedUnit);
        }

        public bool SpawnEnemyById(
            string enemyId,
            int spawnIndex,
            float spawnX,
            float spawnYMin,
            float spawnYMax,
            float healthMultiplier,
            out GameObject spawnedUnit)
        {
            var prefab = ResolveEnemyPrefab(enemyId);
            return TrySpawnEnemyUnit(prefab, enemyId, spawnIndex, spawnX, spawnYMin, spawnYMax, healthMultiplier, out spawnedUnit);
        }

        public bool SpawnManifestedMonster(
            MonsterDefinition monster,
            int partySlotIndex,
            out GameObject spawnedUnit)
        {
            spawnedUnit = null;
            ResolveCombatManager();

            if (monster == null || string.IsNullOrWhiteSpace(monster.MonsterId))
            {
                Debug.LogWarning("NewRunSceneEntryManager cannot manifest a monster because monster data is missing.");
                return false;
            }

            if (ActiveSession == null)
            {
                Debug.LogWarning("NewRunSceneEntryManager cannot manifest a monster because no active session exists.");
                return false;
            }

            var prefab = ResolvePrefab(monster.MonsterId);
            if (prefab == null)
            {
                Debug.LogWarning($"No NewRunScene prefab is configured for manifested monster '{monster.MonsterId}'.");
                return false;
            }

            var clampedSlotIndex = Mathf.Clamp(partySlotIndex, 1, 4);
            var runState = ActiveSession.EnsurePartyMemberState(monster);
            var model = unitFactory.CreateManifestedMonster(monster, runState, clampedSlotIndex);
            if (model == null)
            {
                Debug.LogError($"NewRunSceneEntryManager could not create manifested monster runtime model for '{monster.MonsterId}'.");
                return false;
            }

            SkillRuntimeFactory.RebuildLearnedActiveSet(model, new InGameSkillCatalog(ResolveCatalog()));

            var spawnPoint = ResolveManifestSpawnPoint(clampedSlotIndex);
            var spawnPosition = spawnPoint != null ? spawnPoint.position : new Vector3(-4f, -1.5f + clampedSlotIndex, 0f);
            var spawnRotation = spawnPoint != null ? spawnPoint.rotation : Quaternion.identity;
            spawnedUnit = Instantiate(prefab, spawnPosition, spawnRotation, ResolveRuntimeMonsterRoot());
            spawnedUnit.name = $"{prefab.name}_{clampedSlotIndex + 1}P";

            var actor = spawnedUnit.GetComponentInChildren<MonsterUnitActor>(true);
            if (actor == null)
            {
                Debug.LogWarning($"Manifested monster unit '{spawnedUnit.name}' has no MonsterUnitActor component.");
            }
            else
            {
                actor.Initialize(model);
            }

            if (combatManager != null)
            {
                combatManager.RegisterPlayerMonster(model, actor);
            }

            return true;
        }

        private IEnumerator SpawnInitialEnemySequence()
        {
            SpawnInitialEnemyUnit();

            yield return new WaitForSeconds(Mathf.Max(0f, enemySpawnIntervalSeconds));
            SpawnShieldEnemyUnit();

            yield return new WaitForSeconds(Mathf.Max(0f, enemySpawnIntervalSeconds));
            SpawnRangedEnemyUnit();

            yield return new WaitForSeconds(Mathf.Max(0f, enemySpawnIntervalSeconds));
            SpawnBufferEnemyUnit();

            yield return new WaitForSeconds(Mathf.Max(0f, enemySpawnIntervalSeconds));
            SpawnGuardianCaptainEnemyUnit();

            yield return new WaitForSeconds(Mathf.Max(0f, enemySpawnIntervalSeconds));
            SpawnAttackCaptainEnemyUnit();

            yield return new WaitForSeconds(Mathf.Max(0f, enemySpawnIntervalSeconds));
            SpawnHeroKarinEnemyUnit();

            enemySpawnSequence = null;
        }

        private bool TryCreateSelectedModel(string monsterId, out MonsterUnitRuntimeModel model)
        {
            model = null;

            var catalog = ResolveCatalog();
            if (catalog == null)
            {
                Debug.LogError("NewRunSceneEntryManager could not resolve a game data catalog for the selected monster.");
                return false;
            }

            var monster = ResolveMonsterDefinition(monsterId, catalog);
            if (monster == null)
            {
                Debug.LogError($"NewRunSceneEntryManager could not resolve selected monster data for '{monsterId}'.");
                return false;
            }

            ActiveSession = RunSession.Begin(monster);
            model = unitFactory.CreateSelectedMonster(monster, ActiveSession.GetPartyMemberState(monster.MonsterId), 0);
            if (model == null)
            {
                Debug.LogError($"NewRunSceneEntryManager could not create a runtime unit model for '{monsterId}'.");
                return false;
            }

            SkillRuntimeFactory.RebuildLearnedActiveSet(model, new InGameSkillCatalog(catalog));
            return true;
        }

        private GameDataCatalog ResolveCatalog()
        {
            var registeredCatalog = PakuriDataManager.Instance.CurrentCatalog;
            if (registeredCatalog != null)
            {
                return registeredCatalog;
            }

            if (fallbackCatalog != null)
            {
                PakuriDataManager.Instance.RegisterCatalog(fallbackCatalog);
                return fallbackCatalog;
            }

            return PakuriCsvRuntimeData.ResolveCatalogOrFallback(null);
        }

        private MonsterDefinition ResolveMonsterDefinition(string monsterId, GameDataCatalog catalog)
        {
            if (string.IsNullOrWhiteSpace(monsterId))
            {
                return null;
            }

            var registered = PakuriDataManager.Instance.GetData<MonsterDefinition>(monsterId);
            if (registered != null)
            {
                return registered;
            }

            var fromCatalog = catalog != null ? catalog.GetMonsterById(monsterId) : null;
            if (fromCatalog != null)
            {
                return fromCatalog;
            }

            return fallbackCatalog != null && fallbackCatalog != catalog
                ? fallbackCatalog.GetMonsterById(monsterId)
                : null;
        }

        private EnemyDefinition ResolveEnemyDefinition(string enemyId, GameDataCatalog catalog)
        {
            if (string.IsNullOrWhiteSpace(enemyId))
            {
                return null;
            }

            var registered = PakuriDataManager.Instance.GetData<EnemyDefinition>(enemyId);
            if (registered != null)
            {
                return registered;
            }

            var fromCatalog = catalog != null ? catalog.GetStageOneEnemyById(enemyId) : null;
            if (fromCatalog != null)
            {
                return fromCatalog;
            }

            return fallbackCatalog != null && fallbackCatalog != catalog
                ? fallbackCatalog.GetStageOneEnemyById(enemyId)
                : null;
        }

        private void BindSpawnedActor(GameObject spawnedUnit, MonsterUnitRuntimeModel model)
        {
            SpawnedPlayerActor = spawnedUnit != null
                ? spawnedUnit.GetComponentInChildren<MonsterUnitActor>(true)
                : null;
            if (SpawnedPlayerActor == null)
            {
                Debug.LogWarning($"Spawned player unit '{spawnedUnit?.name}' has no MonsterUnitActor component.");
                return;
            }

            SpawnedPlayerActor.Initialize(model);
        }

        private bool TrySpawnEnemyUnit(GameObject prefab, string enemyId, int spawnIndex, out GameObject spawnedUnit)
        {
            ResolveEnemySpawnPoint();
            var basePosition = enemySpawnPoint != null ? enemySpawnPoint.position : Vector3.zero;
            return TrySpawnEnemyUnit(prefab, enemyId, spawnIndex, basePosition.x, enemySpawnMinY, enemySpawnMaxY, out spawnedUnit);
        }

        private bool TrySpawnEnemyUnit(
            GameObject prefab,
            string enemyId,
            int spawnIndex,
            float spawnX,
            float spawnYMin,
            float spawnYMax,
            out GameObject spawnedUnit)
        {
            return TrySpawnEnemyUnit(prefab, enemyId, spawnIndex, spawnX, spawnYMin, spawnYMax, 1f, out spawnedUnit);
        }

        private bool TrySpawnEnemyUnit(
            GameObject prefab,
            string enemyId,
            int spawnIndex,
            float spawnX,
            float spawnYMin,
            float spawnYMax,
            float healthMultiplier,
            out GameObject spawnedUnit)
        {
            spawnedUnit = null;
            ResolveEnemySpawnPoint();

            if (prefab == null)
            {
                Debug.LogWarning($"No NewRunScene enemy prefab is configured for enemy '{enemyId}'.");
                return false;
            }

            if (!TryCreateEnemyModel(enemyId, spawnIndex, out var model))
            {
                return false;
            }

            ApplyEnemyHealthMultiplier(model, healthMultiplier);
            var spawnPosition = new Vector3(
                spawnX,
                UnityEngine.Random.Range(spawnYMin, spawnYMax),
                enemySpawnPoint != null ? enemySpawnPoint.position.z : 0f);
            var spawnRotation = enemySpawnPoint != null ? enemySpawnPoint.rotation : Quaternion.identity;
            spawnedUnit = Instantiate(prefab, spawnPosition, spawnRotation, ResolveRuntimeEnemyRoot());
            spawnedUnit.name = $"{prefab.name}_Enemy_{spawnIndex}";
            SpawnedEnemyModel = model;
            SpawnedEnemyActor = BindSpawnedEnemyActor(spawnedUnit, model);
            RegisterSpawnedEnemy(model, SpawnedEnemyActor);
            return true;
        }

        private bool TryCreateEnemyModel(string enemyId, int slotIndex, out EnemyUnitRuntimeModel model)
        {
            model = null;

            var catalog = ResolveCatalog();
            if (catalog == null)
            {
                Debug.LogError("NewRunSceneEntryManager could not resolve a game data catalog for the initial enemy.");
                return false;
            }

            var enemy = ResolveEnemyDefinition(enemyId, catalog);
            if (enemy == null)
            {
                Debug.LogError($"NewRunSceneEntryManager could not resolve enemy data for '{enemyId}'.");
                return false;
            }

            model = unitFactory.CreateEnemy(enemy, slotIndex);
            if (model == null)
            {
                Debug.LogError($"NewRunSceneEntryManager could not create an enemy runtime unit model for '{enemyId}'.");
                return false;
            }

            return true;
        }

        private static void ApplyEnemyHealthMultiplier(EnemyUnitRuntimeModel model, float healthMultiplier)
        {
            if (model == null || healthMultiplier <= 0f || Mathf.Approximately(healthMultiplier, 1f))
            {
                return;
            }

            if (model.Stats != null)
            {
                model.Stats.MaxHealth *= healthMultiplier;
            }

            if (model.Resources != null)
            {
                model.Resources.CurrentHealth *= healthMultiplier;
            }
        }

        private EnemyUnitActor BindSpawnedEnemyActor(GameObject spawnedUnit, EnemyUnitRuntimeModel model)
        {
            var actor = spawnedUnit != null
                ? spawnedUnit.GetComponentInChildren<EnemyUnitActor>(true)
                : null;
            if (actor == null)
            {
                Debug.LogWarning($"Spawned enemy unit '{spawnedUnit?.name}' has no EnemyUnitActor component.");
                return null;
            }

            actor.Initialize(model);
            return actor;
        }

        private void RegisterSpawnedPlayer()
        {
            ResolveCombatManager();
            if (combatManager == null || SpawnedPlayerModel == null)
            {
                return;
            }

            combatManager.RegisterPlayerMonster(SpawnedPlayerModel, SpawnedPlayerActor);
        }

        private void RegisterSpawnedEnemy(EnemyUnitRuntimeModel model, EnemyUnitActor actor)
        {
            ResolveCombatManager();
            if (combatManager == null || model == null)
            {
                return;
            }

            combatManager.RegisterEnemy(model, actor);
        }

        private GameObject ResolvePrefab(string monsterId)
        {
            if (string.Equals(monsterId, ArielMonsterId, StringComparison.OrdinalIgnoreCase))
            {
                return arielUnitPrefab;
            }

            if (string.Equals(monsterId, EveMonsterId, StringComparison.OrdinalIgnoreCase))
            {
                return eveUnitPrefab;
            }

            if (string.Equals(monsterId, RinMonsterId, StringComparison.OrdinalIgnoreCase))
            {
                return rinUnitPrefab;
            }

            if (string.Equals(monsterId, SeinMonsterId, StringComparison.OrdinalIgnoreCase))
            {
                return seinUnitPrefab;
            }

            if (string.Equals(monsterId, VegaMonsterId, StringComparison.OrdinalIgnoreCase))
            {
                return vegaUnitPrefab;
            }

            return null;
        }

        private GameObject ResolveEnemyPrefab(string enemyId)
        {
            if (string.Equals(enemyId, initialEnemyId, StringComparison.OrdinalIgnoreCase))
            {
                return stageOneEnemyPrefab;
            }

            if (string.Equals(enemyId, shieldEnemyId, StringComparison.OrdinalIgnoreCase))
            {
                return stageOneShieldEnemyPrefab;
            }

            if (string.Equals(enemyId, rangedEnemyId, StringComparison.OrdinalIgnoreCase))
            {
                return stageOneRangedEnemyPrefab;
            }

            if (string.Equals(enemyId, bufferEnemyId, StringComparison.OrdinalIgnoreCase))
            {
                return stageOneBufferEnemyPrefab;
            }

            if (string.Equals(enemyId, guardianCaptainEnemyId, StringComparison.OrdinalIgnoreCase))
            {
                return stageOneGuardianCaptainPrefab;
            }

            if (string.Equals(enemyId, attackCaptainEnemyId, StringComparison.OrdinalIgnoreCase))
            {
                return stageOneAttackCaptainPrefab;
            }

            if (string.Equals(enemyId, heroKarinEnemyId, StringComparison.OrdinalIgnoreCase))
            {
                return stageOneHeroKarinPrefab;
            }

            return null;
        }

        private void ResolveSpawnPoint()
        {
            if (playerSpawnPoint != null)
            {
                return;
            }

            var spawnPointObject = GameObject.Find("1PSpawnPoint");
            if (spawnPointObject != null)
            {
                playerSpawnPoint = spawnPointObject.transform;
            }
        }

        private void ResolveEnemySpawnPoint()
        {
            if (enemySpawnPoint != null)
            {
                return;
            }

            var spawnPointObject = GameObject.Find("SpawnPoint");
            if (spawnPointObject != null)
            {
                enemySpawnPoint = spawnPointObject.transform;
            }
        }

        private static Transform ResolveManifestSpawnPoint(int partySlotIndex)
        {
            var spawnPointObject = GameObject.Find($"{partySlotIndex + 1}PSpawnPoint");
            return spawnPointObject != null ? spawnPointObject.transform : null;
        }

        private Transform ResolveRuntimeMonsterRoot()
        {
            runtimeMonsterRoot = ResolveRuntimeRoot(runtimeMonsterRoot, RuntimeMonsterRootName, true);
            return runtimeMonsterRoot;
        }

        private Transform ResolveRuntimeEnemyRoot()
        {
            runtimeEnemyRoot = ResolveRuntimeRoot(runtimeEnemyRoot, RuntimeEnemyRootName, true);
            return runtimeEnemyRoot;
        }

        private Transform ResolveRuntimeObjectRoot()
        {
            runtimeObjectRoot = ResolveRuntimeRoot(runtimeObjectRoot, RuntimeObjectRootName, false);
            return runtimeObjectRoot;
        }

        private Transform ResolveRuntimeRoot(Transform cachedRoot, string rootName, bool parentUnderRuntimeObject)
        {
            if (cachedRoot == null)
            {
                var existing = GameObject.Find(rootName);
                cachedRoot = existing != null ? existing.transform : new GameObject(rootName).transform;
            }

            if (parentUnderRuntimeObject && cachedRoot.parent == null)
            {
                cachedRoot.SetParent(ResolveRuntimeObjectRoot(), true);
            }

            return cachedRoot;
        }

        private void ResolveCombatManager()
        {
            if (combatManager != null)
            {
                return;
            }

            combatManager = GetComponent<InGameCombatManager>();
            if (combatManager != null)
            {
                return;
            }

            combatManager = gameObject.AddComponent<InGameCombatManager>();
        }
    }
}
