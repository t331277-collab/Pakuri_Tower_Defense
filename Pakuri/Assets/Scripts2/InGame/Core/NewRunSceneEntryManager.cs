using System;
using Pakuri.Data;
using Pakuri.Run;
using UnityEngine;

namespace Pakuri.InGame
{
    [DisallowMultipleComponent]
    public sealed class NewRunSceneEntryManager : MonoBehaviour
    {
        private const string ArielMonsterId = "ariel";
        private const string EveMonsterId = "eve";
        private const string RinMonsterId = "rin";
        private const string SeinMonsterId = "sein";
        private const string VegaMonsterId = "vega";

        private readonly UnitFactory unitFactory = new UnitFactory();

        [SerializeField] private Transform playerSpawnPoint;
        [SerializeField] private GameDataCatalog fallbackCatalog;
        [SerializeField] private GameObject arielUnitPrefab;
        [SerializeField] private GameObject eveUnitPrefab;
        [SerializeField] private GameObject rinUnitPrefab;
        [SerializeField] private GameObject seinUnitPrefab;
        [SerializeField] private GameObject vegaUnitPrefab;
        [SerializeField] private bool allowEveFallback = true;

        private GameObject spawnedPlayerUnit;

        public MonsterUnitActor SpawnedPlayerActor { get; private set; }
        public MonsterUnitRuntimeModel SpawnedPlayerModel { get; private set; }
        public RunSession ActiveSession { get; private set; }

        private void Start()
        {
            SpawnSelectedPlayerUnit();
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

            NewRunStartContext.Clear();
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
    }
}
