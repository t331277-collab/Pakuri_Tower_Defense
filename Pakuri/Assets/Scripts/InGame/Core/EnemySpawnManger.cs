using System;
using Pakuri.Data;
using Pakuri.Run;
using UnityEngine;

namespace Pakuri.InGame
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(InGameCombatManager))]
    public sealed class EnemySpawnManger : MonoBehaviour
    {
        private const string ArielMonsterId = "ariel";
        private const string EveMonsterId = "eve";
        private const string RinMonsterId = "rin";
        private const string SeinMonsterId = "sein";
        private const string VegaMonsterId = "vega";
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
        [SerializeField] private GameObject arielUnitPrefab;
        [SerializeField] private GameObject eveUnitPrefab;
        [SerializeField] private GameObject rinUnitPrefab;
        [SerializeField] private GameObject seinUnitPrefab;
        [SerializeField] private GameObject vegaUnitPrefab;
        [SerializeField] private EnemyPrefabBinding[] enemyPrefabBindings = Array.Empty<EnemyPrefabBinding>();
        [SerializeField] private float enemySpawnMinY = -5f;
        [SerializeField] private float enemySpawnMaxY = 5f;

        public InGameCombatManager CombatManager => combatManager;
        public float EnemySpawnMinY => enemySpawnMinY;
        public float EnemySpawnMaxY => enemySpawnMaxY;

        /// <summary>Returns the root unit sprite from the existing enemy-id prefab binding.</summary>
        public Sprite ResolveEnemyPortraitSprite(string enemyId)
        {
            var prefab = ResolveEnemyPrefab(enemyId);
            var spriteRenderer = prefab != null ? prefab.GetComponent<SpriteRenderer>() : null;
            return spriteRenderer != null ? spriteRenderer.sprite : null;
        }

        [Serializable]
        private sealed class EnemyPrefabBinding
        {
            [SerializeField] private string enemyId = string.Empty;
            [SerializeField] private GameObject prefab = null;

            public string EnemyId => enemyId;
            public GameObject Prefab => prefab;
        }

        public bool SpawnSelectedPlayerUnit(
            string selectedMonsterId,
            out GameObject spawnedUnit,
            out MonsterUnitRuntimeModel model,
            out MonsterUnitActor actor,
            out RunSession session)
        {
            spawnedUnit = null;
            model = null;
            actor = null;
            session = null;

            ResolveSpawnPoint();
            if (string.IsNullOrWhiteSpace(selectedMonsterId))
            {
                Debug.LogWarning("EnemySpawnManger cannot spawn a selected monster because monster data is missing.");
                return false;
            }

            var prefab = ResolveMonsterPrefab(selectedMonsterId);
            if (prefab == null)
            {
                Debug.LogWarning($"No NewRunScene prefab is configured for selected monster '{selectedMonsterId}'.");
                return false;
            }

            if (!TryCreateSelectedModel(selectedMonsterId, out model, out session))
            {
                return false;
            }

            var spawnPosition = playerSpawnPoint != null ? playerSpawnPoint.position : Vector3.zero;
            var spawnRotation = playerSpawnPoint != null ? playerSpawnPoint.rotation : Quaternion.identity;
            spawnedUnit = Instantiate(prefab, spawnPosition, spawnRotation, ResolveRuntimeMonsterRoot());
            spawnedUnit.name = $"{prefab.name}_1P";
            actor = BindMonsterActor(spawnedUnit, model);
            RegisterPlayer(model, actor, spawnedUnit != null ? spawnedUnit.transform : null);
            return true;
        }

        public bool SpawnManifestedMonster(
            MonsterDefinition monster,
            RunSession activeSession,
            int partySlotIndex,
            out GameObject spawnedUnit)
        {
            spawnedUnit = null;
            ResolveCombatManager();

            if (monster == null || string.IsNullOrWhiteSpace(monster.MonsterId))
            {
                Debug.LogWarning("EnemySpawnManger cannot manifest a monster because monster data is missing.");
                return false;
            }

            if (activeSession == null)
            {
                Debug.LogWarning("EnemySpawnManger cannot manifest a monster because no active session exists.");
                return false;
            }

            var prefab = ResolveMonsterPrefab(monster.MonsterId);
            if (prefab == null)
            {
                Debug.LogWarning($"No NewRunScene prefab is configured for manifested monster '{monster.MonsterId}'.");
                return false;
            }

            var clampedSlotIndex = Mathf.Clamp(partySlotIndex, 1, 4);
            var runState = activeSession.EnsurePartyMemberState(monster);
            var model = unitFactory.CreateManifestedMonster(monster, runState, clampedSlotIndex);
            if (model == null)
            {
                Debug.LogError($"EnemySpawnManger could not create manifested monster runtime model for '{monster.MonsterId}'.");
                return false;
            }

            SkillRuntimeFactory.RebuildLearnedActiveSet(model, new InGameSkillCatalog(ResolveCatalog()));

            var spawnPoint = ResolveManifestSpawnPoint(clampedSlotIndex);
            var spawnPosition = spawnPoint != null ? spawnPoint.position : new Vector3(-4f, -1.5f + clampedSlotIndex, 0f);
            var spawnRotation = spawnPoint != null ? spawnPoint.rotation : Quaternion.identity;
            spawnedUnit = Instantiate(prefab, spawnPosition, spawnRotation, ResolveRuntimeMonsterRoot());
            spawnedUnit.name = $"{prefab.name}_{clampedSlotIndex + 1}P";

            var actor = BindMonsterActor(spawnedUnit, model);
            RegisterPlayer(model, actor, spawnedUnit != null ? spawnedUnit.transform : null);
            return true;
        }

        public bool RespawnSelectedPlayerUnit(
            RunSession activeSession,
            out GameObject spawnedUnit,
            out MonsterUnitRuntimeModel model,
            out MonsterUnitActor actor)
        {
            spawnedUnit = null;
            model = null;
            actor = null;
            ResolveCombatManager();
            ResolveSpawnPoint();

            if (activeSession == null || string.IsNullOrWhiteSpace(activeSession.SelectedMonsterId))
            {
                Debug.LogWarning("EnemySpawnManger cannot respawn the selected monster because no active session exists.");
                return false;
            }

            var catalog = ResolveCatalog();
            var monster = ResolveMonsterDefinition(activeSession.SelectedMonsterId, catalog);
            if (monster == null)
            {
                Debug.LogError($"EnemySpawnManger could not resolve selected monster data for '{activeSession.SelectedMonsterId}' during respawn.");
                return false;
            }

            var prefab = ResolveMonsterPrefab(monster.MonsterId);
            if (prefab == null)
            {
                Debug.LogWarning($"No NewRunScene prefab is configured for selected monster '{monster.MonsterId}' during respawn.");
                return false;
            }

            var runState = activeSession.GetPartyMemberState(monster.MonsterId) ?? activeSession.EnsurePartyMemberState(monster);
            model = unitFactory.CreateSelectedMonster(monster, runState, 0);
            if (model == null)
            {
                Debug.LogError($"EnemySpawnManger could not recreate a runtime unit model for '{monster.MonsterId}' during respawn.");
                return false;
            }

            SkillRuntimeFactory.RebuildLearnedActiveSet(model, new InGameSkillCatalog(catalog));

            var spawnPosition = playerSpawnPoint != null ? playerSpawnPoint.position : Vector3.zero;
            var spawnRotation = playerSpawnPoint != null ? playerSpawnPoint.rotation : Quaternion.identity;
            spawnedUnit = Instantiate(prefab, spawnPosition, spawnRotation, ResolveRuntimeMonsterRoot());
            spawnedUnit.name = $"{prefab.name}_1P";
            actor = BindMonsterActor(spawnedUnit, model);
            RegisterPlayer(model, actor, spawnedUnit != null ? spawnedUnit.transform : null);
            return true;
        }

        public bool SpawnEnemyById(
            string enemyId,
            int spawnIndex,
            float spawnX,
            float spawnYMin,
            float spawnYMax,
            out GameObject spawnedUnit)
        {
            return SpawnEnemyById(enemyId, spawnIndex, spawnX, spawnYMin, spawnYMax, 1f, false, out spawnedUnit);
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
            return SpawnEnemyById(enemyId, spawnIndex, spawnX, spawnYMin, spawnYMax, healthMultiplier, false, out spawnedUnit);
        }

        public bool SpawnEnemyById(
            string enemyId,
            int spawnIndex,
            float spawnX,
            float spawnYMin,
            float spawnYMax,
            float healthMultiplier,
            bool isBoss,
            out GameObject spawnedUnit)
        {
            var prefab = ResolveEnemyPrefab(enemyId);
            return TrySpawnEnemyUnit(prefab, enemyId, spawnIndex, spawnX, spawnYMin, spawnYMax, healthMultiplier, isBoss, out spawnedUnit);
        }

        private bool TryCreateSelectedModel(
            string monsterId,
            out MonsterUnitRuntimeModel model,
            out RunSession session)
        {
            model = null;
            session = null;

            var catalog = ResolveCatalog();
            if (catalog == null)
            {
                Debug.LogError("EnemySpawnManger could not resolve a game data catalog for the selected monster.");
                return false;
            }

            var monster = ResolveMonsterDefinition(monsterId, catalog);
            if (monster == null)
            {
                Debug.LogError($"EnemySpawnManger could not resolve selected monster data for '{monsterId}'.");
                return false;
            }

            session = RunSession.Begin(monster);
            model = unitFactory.CreateSelectedMonster(monster, session.GetPartyMemberState(monster.MonsterId), 0);
            if (model == null)
            {
                Debug.LogError($"EnemySpawnManger could not create a runtime unit model for '{monsterId}'.");
                return false;
            }

            SkillRuntimeFactory.RebuildLearnedActiveSet(model, new InGameSkillCatalog(catalog));
            return true;
        }

        private bool TrySpawnEnemyUnit(
            GameObject prefab,
            string enemyId,
            int spawnIndex,
            float spawnX,
            float spawnYMin,
            float spawnYMax,
            float healthMultiplier,
            bool isBoss,
            out GameObject spawnedUnit)
        {
            spawnedUnit = null;
            ResolveEnemySpawnPoint();

            if (prefab == null)
            {
                Debug.LogWarning($"No NewRunScene enemy prefab is configured for enemy '{enemyId}'.");
                return false;
            }

            if (!TryCreateEnemyModel(enemyId, spawnIndex, isBoss, out var model))
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

            var actor = BindEnemyActor(spawnedUnit, model);
            RegisterEnemy(model, actor, spawnedUnit != null ? spawnedUnit.transform : null);
            return true;
        }

        private bool TryCreateEnemyModel(string enemyId, int slotIndex, bool isBoss, out EnemyUnitRuntimeModel model)
        {
            model = null;

            var catalog = ResolveCatalog();
            if (catalog == null)
            {
                Debug.LogError("EnemySpawnManger could not resolve a game data catalog for the enemy.");
                return false;
            }

            var enemy = ResolveEnemyDefinition(enemyId, catalog);
            if (enemy == null)
            {
                Debug.LogError($"EnemySpawnManger could not resolve enemy data for '{enemyId}'.");
                return false;
            }

            model = unitFactory.CreateEnemy(enemy, slotIndex, isBoss);
            if (model == null)
            {
                Debug.LogError($"EnemySpawnManger could not create an enemy runtime unit model for '{enemyId}'.");
                return false;
            }

            SkillRuntimeFactory.RebuildAssignedActiveSet(model, enemy.ActiveSkills, enemy.SkillTriggers);

            return true;
        }

        private GameDataCatalog ResolveCatalog()
        {
            var registeredCatalog = PakuriDataManager.Instance.CurrentCatalog;
            if (registeredCatalog != null)
            {
                return registeredCatalog;
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

            return null;
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

            var fromCatalog = catalog != null ? catalog.GetEnemyById(enemyId) : null;
            if (fromCatalog != null)
            {
                return fromCatalog;
            }

            return null;
        }

        private MonsterUnitActor BindMonsterActor(GameObject spawnedUnit, MonsterUnitRuntimeModel model)
        {
            var actor = spawnedUnit != null
                ? spawnedUnit.GetComponentInChildren<MonsterUnitActor>(true)
                : null;
            if (actor == null)
            {
                Debug.LogWarning($"Spawned monster unit '{spawnedUnit?.name}' has no MonsterUnitActor component.");
                return null;
            }

            actor.Initialize(model);
            return actor;
        }

        private EnemyUnitActor BindEnemyActor(GameObject spawnedUnit, EnemyUnitRuntimeModel model)
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

        private void RegisterPlayer(MonsterUnitRuntimeModel model, MonsterUnitActor actor, Transform hitboxRoot)
        {
            ResolveCombatManager();
            if (combatManager != null && model != null)
            {
                combatManager.RegisterPlayerMonster(model, actor, hitboxRoot);
            }
        }

        private void RegisterEnemy(EnemyUnitRuntimeModel model, EnemyUnitActor actor, Transform hitboxRoot)
        {
            ResolveCombatManager();
            if (combatManager != null && model != null)
            {
                combatManager.RegisterEnemy(model, actor, hitboxRoot);
            }
        }

        private GameObject ResolveMonsterPrefab(string monsterId)
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
            if (enemyPrefabBindings != null)
            {
                for (var i = 0; i < enemyPrefabBindings.Length; i++)
                {
                    var binding = enemyPrefabBindings[i];
                    if (binding == null || binding.Prefab == null)
                    {
                        continue;
                    }

                    if (string.Equals(enemyId, binding.EnemyId, StringComparison.OrdinalIgnoreCase))
                    {
                        return binding.Prefab;
                    }
                }
            }

            return null;
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
