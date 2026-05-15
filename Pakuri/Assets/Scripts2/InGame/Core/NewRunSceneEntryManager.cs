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
        private const string DefaultRangedEnemyId = "stage1-rogue";
        private const string DefaultBufferEnemyId = "stage1-priest";

        private readonly UnitFactory unitFactory = new UnitFactory();

        [SerializeField] private InGameCombatManager combatManager;
        [SerializeField] private Transform playerSpawnPoint;
        [SerializeField] private Transform enemySpawnPoint;
        [SerializeField] private GameDataCatalog fallbackCatalog;
        [SerializeField] private GameObject arielUnitPrefab;
        [SerializeField] private GameObject eveUnitPrefab;
        [SerializeField] private GameObject rinUnitPrefab;
        [SerializeField] private GameObject seinUnitPrefab;
        [SerializeField] private GameObject vegaUnitPrefab;
        [SerializeField] private GameObject stageOneEnemyPrefab;
        [SerializeField] private GameObject stageOneRangedEnemyPrefab;
        [SerializeField] private GameObject stageOneBufferEnemyPrefab;
        [SerializeField] private string initialEnemyId = DefaultInitialEnemyId;
        [SerializeField] private string rangedEnemyId = DefaultRangedEnemyId;
        [SerializeField] private string bufferEnemyId = DefaultBufferEnemyId;
        [SerializeField] private float enemySpawnMinY = -5f;
        [SerializeField] private float enemySpawnMaxY = 5f;
        [SerializeField] private float enemySpawnIntervalSeconds = 1f;
        [SerializeField] private bool allowEveFallback = true;

        private GameObject spawnedPlayerUnit;
        private GameObject spawnedEnemyUnit;
        private GameObject spawnedRangedEnemyUnit;
        private GameObject spawnedBufferEnemyUnit;
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
            enemySpawnSequence = StartCoroutine(SpawnInitialEnemySequence());
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
            spawnedPlayerUnit = Instantiate(prefab, spawnPosition, spawnRotation);
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

            TrySpawnEnemyUnit(stageOneRangedEnemyPrefab, rangedEnemyId, 1, out spawnedRangedEnemyUnit);
        }

        public void SpawnBufferEnemyUnit()
        {
            if (spawnedBufferEnemyUnit != null)
            {
                return;
            }

            TrySpawnEnemyUnit(stageOneBufferEnemyPrefab, bufferEnemyId, 2, out spawnedBufferEnemyUnit);
        }

        private IEnumerator SpawnInitialEnemySequence()
        {
            SpawnInitialEnemyUnit();

            yield return new WaitForSeconds(Mathf.Max(0f, enemySpawnIntervalSeconds));
            SpawnRangedEnemyUnit();

            yield return new WaitForSeconds(Mathf.Max(0f, enemySpawnIntervalSeconds));
            SpawnBufferEnemyUnit();

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

            var basePosition = enemySpawnPoint != null ? enemySpawnPoint.position : Vector3.zero;
            var spawnPosition = new Vector3(
                basePosition.x,
                UnityEngine.Random.Range(enemySpawnMinY, enemySpawnMaxY),
                basePosition.z);
            var spawnRotation = enemySpawnPoint != null ? enemySpawnPoint.rotation : Quaternion.identity;
            spawnedUnit = Instantiate(prefab, spawnPosition, spawnRotation);
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
