using System.Collections;
using Pakuri.Data;
using Pakuri.Run;
using UnityEngine;

namespace Pakuri.InGame
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(InGameCombatManager))]
    [RequireComponent(typeof(EnemySpawnManger))]
    public sealed class SceneEntryManager : MonoBehaviour
    {
        private const string EveMonsterId = "eve";

        [SerializeField] private InGameCombatManager combatManager;
        [SerializeField] private EnemySpawnManger unitSpawnManager;
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
        public EnemySpawnManger UnitSpawnManager => unitSpawnManager;

        private void Start()
        {
            ResolveReferences();
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

            ResolveReferences();
            var selectedMonsterId = StartContext.HasPendingRun
                ? StartContext.SelectedMonsterId
                : (allowEveFallback ? EveMonsterId : string.Empty);

            if (string.IsNullOrWhiteSpace(selectedMonsterId))
            {
                Debug.LogWarning("SceneEntryManager started without selected monster data.");
                return;
            }

            if (unitSpawnManager == null)
            {
                Debug.LogError("SceneEntryManager cannot spawn the selected monster because EnemySpawnManger is missing.");
                return;
            }

            if (!unitSpawnManager.SpawnSelectedPlayerUnit(
                    selectedMonsterId,
                    out spawnedPlayerUnit,
                    out var model,
                    out var actor,
                    out var session))
            {
                return;
            }

            SpawnedPlayerModel = model;
            SpawnedPlayerActor = actor;
            ActiveSession = session;
            StartContext.Clear();
        }

        public void SpawnInitialEnemyUnit()
        {
            if (spawnedEnemyUnit != null)
            {
                return;
            }

            if (TrySpawnConfiguredEnemy(spawner => spawner.SpawnInitialEnemyUnit(out spawnedEnemyUnit)))
            {
                CaptureLastEnemySpawn();
            }
        }

        public void SpawnRangedEnemyUnit()
        {
            if (spawnedRangedEnemyUnit != null)
            {
                return;
            }

            if (TrySpawnConfiguredEnemy(spawner => spawner.SpawnRangedEnemyUnit(out spawnedRangedEnemyUnit)))
            {
                CaptureLastEnemySpawn();
            }
        }

        public void SpawnShieldEnemyUnit()
        {
            if (spawnedShieldEnemyUnit != null)
            {
                return;
            }

            if (TrySpawnConfiguredEnemy(spawner => spawner.SpawnShieldEnemyUnit(out spawnedShieldEnemyUnit)))
            {
                CaptureLastEnemySpawn();
            }
        }

        public void SpawnBufferEnemyUnit()
        {
            if (spawnedBufferEnemyUnit != null)
            {
                return;
            }

            if (TrySpawnConfiguredEnemy(spawner => spawner.SpawnBufferEnemyUnit(out spawnedBufferEnemyUnit)))
            {
                CaptureLastEnemySpawn();
            }
        }

        public void SpawnGuardianCaptainEnemyUnit()
        {
            if (spawnedGuardianCaptainEnemyUnit != null)
            {
                return;
            }

            if (TrySpawnConfiguredEnemy(spawner => spawner.SpawnGuardianCaptainEnemyUnit(out spawnedGuardianCaptainEnemyUnit)))
            {
                CaptureLastEnemySpawn();
            }
        }

        public void SpawnAttackCaptainEnemyUnit()
        {
            if (spawnedAttackCaptainEnemyUnit != null)
            {
                return;
            }

            if (TrySpawnConfiguredEnemy(spawner => spawner.SpawnAttackCaptainEnemyUnit(out spawnedAttackCaptainEnemyUnit)))
            {
                CaptureLastEnemySpawn();
            }
        }

        public void SpawnHeroKarinEnemyUnit()
        {
            if (spawnedHeroKarinEnemyUnit != null)
            {
                return;
            }

            if (TrySpawnConfiguredEnemy(spawner => spawner.SpawnHeroKarinEnemyUnit(out spawnedHeroKarinEnemyUnit)))
            {
                CaptureLastEnemySpawn();
            }
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
            spawnedUnit = null;
            ResolveReferences();
            if (unitSpawnManager == null)
            {
                Debug.LogError("SceneEntryManager cannot spawn enemies because EnemySpawnManger is missing.");
                return false;
            }

            var spawned = unitSpawnManager.SpawnEnemyById(
                enemyId,
                spawnIndex,
                spawnX,
                spawnYMin,
                spawnYMax,
                healthMultiplier,
                out spawnedUnit);
            if (spawned)
            {
                CaptureLastEnemySpawn();
            }

            return spawned;
        }

        public bool SpawnManifestedMonster(
            MonsterDefinition monster,
            int partySlotIndex,
            out GameObject spawnedUnit)
        {
            spawnedUnit = null;
            ResolveReferences();
            if (unitSpawnManager == null)
            {
                Debug.LogError("SceneEntryManager cannot manifest monsters because EnemySpawnManger is missing.");
                return false;
            }

            return unitSpawnManager.SpawnManifestedMonster(monster, ActiveSession, partySlotIndex, out spawnedUnit);
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

        private delegate bool ConfiguredEnemySpawn(EnemySpawnManger spawner);

        private bool TrySpawnConfiguredEnemy(ConfiguredEnemySpawn spawn)
        {
            ResolveReferences();
            if (unitSpawnManager == null)
            {
                Debug.LogError("SceneEntryManager cannot spawn configured enemies because EnemySpawnManger is missing.");
                return false;
            }

            return spawn(unitSpawnManager);
        }

        private void CaptureLastEnemySpawn()
        {
            if (combatManager == null || combatManager.Roster.Enemies.Count == 0)
            {
                return;
            }

            var lastEntry = combatManager.Roster.Enemies[combatManager.Roster.Enemies.Count - 1];
            SpawnedEnemyModel = lastEntry.Model as EnemyUnitRuntimeModel;
            SpawnedEnemyActor = lastEntry.Actor as EnemyUnitActor;
        }

        private void ResolveReferences()
        {
            if (combatManager == null)
            {
                combatManager = GetComponent<InGameCombatManager>();
            }

            if (combatManager == null)
            {
                combatManager = gameObject.AddComponent<InGameCombatManager>();
            }

            if (unitSpawnManager == null)
            {
                unitSpawnManager = GetComponent<EnemySpawnManger>();
            }

            if (unitSpawnManager == null)
            {
                unitSpawnManager = gameObject.AddComponent<EnemySpawnManger>();
            }
        }
    }

    public static class StartContext
    {
        public static string SelectedMonsterId { get; private set; }
        public static bool HasPendingRun => !string.IsNullOrWhiteSpace(SelectedMonsterId);

        public static void Prepare(string selectedMonsterId)
        {
            SelectedMonsterId = string.IsNullOrWhiteSpace(selectedMonsterId) ? string.Empty : selectedMonsterId;
        }

        public static void Clear()
        {
            SelectedMonsterId = string.Empty;
        }
    }

    public sealed class InGameContextManager
    {
    }

    public sealed class InGameResultManager
    {
    }
}
