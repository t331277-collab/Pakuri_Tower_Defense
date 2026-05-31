using System.Collections;
using System.Collections.Generic;
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
                isBoss,
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

        public void RestorePlayerPartyFromSession()
        {
            ResolveReferences();
            if (ActiveSession == null || combatManager == null || unitSpawnManager == null)
            {
                return;
            }

            RestoreSelectedPlayerFromSession();
            RestoreManifestedPlayersFromSession();
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

        private void RestoreSelectedPlayerFromSession()
        {
            var selectedEntry = FindPlayerEntryBySlot(0);
            if (selectedEntry != null)
            {
                CaptureSelectedPlayerSpawn(selectedEntry);
                return;
            }

            if (TryReviveExistingPlayerBySlot(0, out selectedEntry))
            {
                CaptureSelectedPlayerSpawn(selectedEntry);
                return;
            }

            if (!unitSpawnManager.RespawnSelectedPlayerUnit(
                    ActiveSession,
                    out spawnedPlayerUnit,
                    out var model,
                    out var actor))
            {
                spawnedPlayerUnit = null;
                SpawnedPlayerModel = null;
                SpawnedPlayerActor = null;
                return;
            }

            SpawnedPlayerModel = model;
            SpawnedPlayerActor = actor;
        }

        private void RestoreManifestedPlayersFromSession()
        {
            var catalog = PakuriDataManager.Instance.CurrentCatalog;
            if (catalog == null)
            {
                catalog = PakuriCsvRuntimeData.ResolveCatalogOrFallback(null);
            }

            for (var i = 0; i < ActiveSession.ManifestedMonsterIds.Count; i++)
            {
                var slotIndex = Mathf.Clamp(i + 1, 1, 4);
                if (FindPlayerEntryBySlot(slotIndex) != null)
                {
                    continue;
                }

                if (TryReviveExistingPlayerBySlot(slotIndex, out _))
                {
                    continue;
                }

                var monsterId = ActiveSession.ManifestedMonsterIds[i];
                var monster = PakuriDataManager.Instance.ResolveMonster(monsterId, catalog);
                if (monster == null)
                {
                    Debug.LogWarning($"SceneEntryManager could not resolve manifested monster '{monsterId}' for day-advance restore.");
                    continue;
                }

                unitSpawnManager.SpawnManifestedMonster(monster, ActiveSession, slotIndex, out _);
            }
        }

        private void CaptureSelectedPlayerSpawn(UnitRosterEntry entry)
        {
            if (entry == null)
            {
                spawnedPlayerUnit = null;
                SpawnedPlayerModel = null;
                SpawnedPlayerActor = null;
                return;
            }

            var actor = entry.Actor as MonsterUnitActor;
            spawnedPlayerUnit = actor != null ? actor.gameObject : null;
            SpawnedPlayerModel = entry.Model as MonsterUnitRuntimeModel;
            SpawnedPlayerActor = actor;
        }

        private UnitRosterEntry FindPlayerEntryBySlot(int slotIndex)
        {
            if (combatManager == null || combatManager.Roster == null)
            {
                return null;
            }

            var players = combatManager.Roster.Players;
            for (var i = 0; i < players.Count; i++)
            {
                var entry = players[i];
                var identity = entry != null && entry.Model != null ? entry.Model.Identity : null;
                if (identity != null
                    && identity.Side == UnitSide.Player
                    && identity.Role == UnitRole.Monster
                    && identity.SlotIndex == slotIndex)
                {
                    return entry;
                }
            }

            return null;
        }

        private bool TryReviveExistingPlayerBySlot(int slotIndex, out UnitRosterEntry revivedEntry)
        {
            revivedEntry = null;
            if (combatManager == null || combatManager.Roster == null)
            {
                return false;
            }

            var actor = FindExistingPlayerActorBySlot(slotIndex);
            var model = actor != null ? actor.Model : null;
            if (model == null)
            {
                return false;
            }

            SyncExistingMonsterModelFromSession(model);
            actor.ReviveForNextDay();
            revivedEntry = combatManager.RegisterPlayerMonster(model, actor, actor.transform);
            return revivedEntry != null;
        }

        private void SyncExistingMonsterModelFromSession(MonsterUnitRuntimeModel model)
        {
            if (model == null || model.Identity == null || ActiveSession == null)
            {
                return;
            }

            var state = ActiveSession.GetPartyMemberState(model.Identity.DefinitionId);
            if (state == null)
            {
                return;
            }

            if (model.State == null)
            {
                model.State = new UnitStateBucket();
            }

            CopyListToSet(state.LearnedActives, model.State.LearnedActiveSkillIds);
            CopyListToSet(state.LearnedPassives, model.State.LearnedPassiveSkillIds);
            CopyListToSet(state.ChosenChoiceIds, model.State.ChosenChoiceIds);
            SkillRuntimeFactory.RebuildLearnedActiveSet(model, new InGameSkillCatalog(ResolveCatalog()));
        }

        private static void CopyListToSet(IReadOnlyList<string> source, ISet<string> target)
        {
            if (source == null || target == null)
            {
                return;
            }

            target.Clear();
            for (var i = 0; i < source.Count; i++)
            {
                if (!string.IsNullOrWhiteSpace(source[i]))
                {
                    target.Add(source[i]);
                }
            }
        }

        private static GameDataCatalog ResolveCatalog()
        {
            var catalog = PakuriDataManager.Instance.CurrentCatalog;
            return catalog != null ? catalog : PakuriCsvRuntimeData.ResolveCatalogOrFallback(null);
        }

        private MonsterUnitActor FindExistingPlayerActorBySlot(int slotIndex)
        {
            var actors = Resources.FindObjectsOfTypeAll<MonsterUnitActor>();
            for (var i = 0; i < actors.Length; i++)
            {
                var actor = actors[i];
                var identity = actor != null && actor.Model != null ? actor.Model.Identity : null;
                if (identity == null
                    || identity.Side != UnitSide.Player
                    || identity.Role != UnitRole.Monster
                    || identity.SlotIndex != slotIndex)
                {
                    continue;
                }

                if (actor.gameObject == null || !actor.gameObject.scene.IsValid())
                {
                    continue;
                }

                return actor;
            }

            return null;
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
