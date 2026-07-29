/*
 * 역할: 필드 유닛 생명주기 소유.
 * 책임: 플레이어·적·Nexus 런타임 유닛을 생성·등록·조회·표시 갱신·제거한다.
 */

using System;
using System.Collections.Generic;
using Pakuri.Data;
using UnityEngine;

namespace Pakuri.InGame
{

    /// <summary><c>UnitSpawnManager</c>가 담당하는 작업을 조정하고 공유 런타임 상태를 소유한다.</summary>
    public class UnitSpawnManager : MonoBehaviour
    {
        private const string ArielMonsterId = "ariel";
        private const string EveMonsterId = "eve";
        private const string RinMonsterId = "rin";
        private const string SeinMonsterId = "sein";
        private const string VegaMonsterId = "vega";
        private readonly UnitCombatStateFactory unitStateFactory = new UnitCombatStateFactory();
        private readonly CombatUnitRegistry unitRegistry = new CombatUnitRegistry();

        [SerializeField] private InGameCombatManager combatManager;
        [SerializeField] private Transform playerSpawnPoint;
        [SerializeField] private Transform enemySpawnPoint;
        [SerializeField] private Transform runtimeEnemyRoot;
        [SerializeField] private Transform runtimeMonsterRoot;
        [SerializeField] private GameObject arielUnitPrefab;
        [SerializeField] private GameObject eveUnitPrefab;
        [SerializeField] private GameObject rinUnitPrefab;
        [SerializeField] private GameObject seinUnitPrefab;
        [SerializeField] private GameObject vegaUnitPrefab;
        [SerializeField] private EnemyPrefabBinding[] enemyPrefabBindings = Array.Empty<EnemyPrefabBinding>();

        public IReadOnlyList<CombatUnitEntry> Entries => unitRegistry.Entries;
        public IReadOnlyList<CombatUnitEntry> Players => unitRegistry.Players;
        public IReadOnlyList<CombatUnitEntry> Enemies => unitRegistry.Enemies;
        public int EnemyCount => unitRegistry.EnemyCount;

        /// <summary>전달된 <c>actor</c> 값을 사용해 <c>Nexus</c>를 소유 런타임 Registry에 등록한다.</summary>
        public void RegisterNexus(NexusActor actor)
        {
            var model = unitStateFactory.CreateNexus(actor.MaxHealth);
            actor.Initialize(model);
            RegisterUnit(model, actor, actor.transform);
        }

        /// <summary><c>EnemyPrefabBinding</c>가 나타내는 런타임 값을 보관한다.</summary>
        [Serializable]
        private class EnemyPrefabBinding
        {
            [SerializeField] private string enemyId = string.Empty;
            [SerializeField] private GameObject prefab = null;

            public string EnemyId => enemyId;
            public GameObject Prefab => prefab;
        }

        /// <summary>전달된 <c>session</c> 값을 사용해 <c>SelectedPlayerUnit</c>를 런타임 씬 오브젝트로 생성하고 등록한다.</summary>
        public void SpawnSelectedPlayerUnit(RunSession session)
        {
            if (FindPlayerMonsterBySlot(0) != null)
            {
                return;
            }

            CreateSelectedPlayerUnit(
                session,
                out _,
                out _);
        }

        /// <summary>전달된 런타임 입력값을 사용해 <c>SelectedPlayerUnit</c>를 생성한다.</summary>
        private GameObject CreateSelectedPlayerUnit(
            RunSession session,
            out UnitCombatState model,
            out MonsterActor actor)
        {
            var selectedMonsterId = session.SelectedMonsterId;
            var prefab = ResolveMonsterPrefab(selectedMonsterId);
            model = CreateSelectedModel(session);

            var spawnPosition = playerSpawnPoint.position;
            var spawnRotation = playerSpawnPoint.rotation;
            var spawnedUnit = Instantiate(prefab, spawnPosition, spawnRotation, runtimeMonsterRoot);
            spawnedUnit.name = $"{prefab.name}_1P";
            actor = BindMonsterActor(spawnedUnit, model);
            RegisterPlayer(model, actor, spawnedUnit.transform);
            return spawnedUnit;
        }

        /// <summary>전달된 런타임 입력값을 사용해 <c>ManifestedMonster</c>를 런타임 씬 오브젝트로 생성하고 등록한다.</summary>
        public GameObject SpawnManifestedMonster(
            RunSession session,
            MonsterDefinition monster,
            int partySlotIndex)
        {
            return CreateManifestedMonster(monster, session, partySlotIndex);
        }

        /// <summary>전달된 런타임 입력값을 사용해 <c>ManifestedMonster</c>를 생성한다.</summary>
        private GameObject CreateManifestedMonster(
            MonsterDefinition monster,
            RunSession activeSession,
            int partySlotIndex)
        {
            var prefab = ResolveMonsterPrefab(monster.MonsterId);

            var runState = activeSession.GetPartyMemberState(monster.MonsterId)
                ?? throw new InvalidOperationException($"Party state '{monster.MonsterId}' is required before spawning.");
            var model = unitStateFactory.CreateManifestedMonster(monster, runState, partySlotIndex);
            SkillExecution.RebuildLearnedSkillState(model);

            var spawnPoint = ResolveManifestSpawnPoint(partySlotIndex);
            var spawnPosition = spawnPoint.position;
            var spawnRotation = spawnPoint.rotation;
            var spawnedUnit = Instantiate(prefab, spawnPosition, spawnRotation, runtimeMonsterRoot);
            spawnedUnit.name = $"{prefab.name}_{partySlotIndex + 1}P";

            var actor = BindMonsterActor(spawnedUnit, model);
            RegisterPlayer(model, actor, spawnedUnit.transform);
            return spawnedUnit;
        }

        /// <summary>전달된 <c>session</c> 값을 사용해 <c>RestorePlayerPartyFromSession</c> 작업을 수행한다.</summary>
        public void RestorePlayerPartyFromSession(RunSession session)
        {
            RestoreSelectedPlayerFromSession(session);
            RestoreAdditionalPlayersFromSession(session);
        }

        /// <summary>전달된 런타임 입력값을 사용해 <c>EnemyById</c>를 런타임 씬 오브젝트로 생성하고 등록한다.</summary>
        public GameObject SpawnEnemyById(
            string enemyId,
            int spawnIndex,
            float spawnX,
            float spawnYMin,
            float spawnYMax,
            float healthMultiplier,
            bool isBoss)
        {
            var prefab = ResolveEnemyPrefab(enemyId);
            return SpawnEnemyUnit(prefab, enemyId, spawnIndex, spawnX, spawnYMin, spawnYMax, healthMultiplier, isBoss);
        }

        /// <summary>전달된 <c>activeSession</c> 값을 사용해 <c>RestoreSelectedPlayerFromSession</c> 작업을 수행한다.</summary>
        private void RestoreSelectedPlayerFromSession(
            RunSession activeSession)
        {

            var selectedEntry = FindPlayerMonsterBySlot(0);
            if (selectedEntry != null)
            {
                return;
            }

            if (TryReviveExistingPlayerBySlot(activeSession, 0, out _))
            {
                return;
            }

            CreateSelectedPlayerUnit(
                activeSession,
                out _,
                out _);
        }

        /// <summary>전달된 <c>activeSession</c> 값을 사용해 <c>RestoreAdditionalPlayersFromSession</c> 작업을 수행한다.</summary>
        private void RestoreAdditionalPlayersFromSession(RunSession activeSession)
        {
            for (var slotIndex = 1; slotIndex < activeSession.PartyMembers.Count; slotIndex++)
            {
                if (FindPlayerMonsterBySlot(slotIndex) != null)
                {
                    continue;
                }

                if (TryReviveExistingPlayerBySlot(activeSession, slotIndex, out _))
                {
                    continue;
                }

                var monsterId = activeSession.PartyMembers[slotIndex].MonsterId;
                var monster = GameDataLoader.CurrentCatalog.GetMonster(monsterId)
                    ?? throw new InvalidOperationException($"Party monster data '{monsterId}' is required.");

                CreateManifestedMonster(monster, activeSession, slotIndex);
            }
        }

        /// <summary>전달된 <c>slotIndex</c> 값을 사용해 <c>PlayerMonsterBySlot</c>를 찾는다.</summary>
        public CombatUnitEntry FindPlayerMonsterBySlot(int slotIndex)
        {
            var players = Players;
            for (var i = 0; i < players.Count; i++)
            {
                var entry = players[i];
                var identity = entry.Model.Identity;
                if (identity.Side == UnitSide.Player
                    && identity.Role == UnitRole.Monster
                    && identity.SlotIndex == slotIndex)
                {
                    return entry;
                }
            }

            return null;
        }

        /// <summary>전달된 <c>model</c> 값을 사용해 <c>요청값</c>를 찾는다.</summary>
        public CombatUnitEntry Find(UnitCombatState model)
        {
            return unitRegistry.Find(model);
        }

        /// <summary>전달된 <c>collider</c> 값을 사용해 <c>ByCollider</c>를 찾는다.</summary>
        public CombatUnitEntry FindByCollider(Collider2D collider)
        {
            return unitRegistry.FindByCollider(collider);
        }

        /// <summary>전달된 <c>model</c> 값을 사용해 <c>Display</c>를 현재 런타임 모델을 기준으로 갱신한다.</summary>
        public bool RefreshDisplay(UnitCombatState model)
        {
            return unitRegistry.RefreshDisplay(model);
        }

        /// <summary>전달된 런타임 입력값을 사용해 <c>Unit</c>를 소유 런타임 Registry에 등록한다.</summary>
        private CombatUnitEntry RegisterUnit(
            UnitCombatState model,
            Component actor,
            Transform hitboxRoot = null)
        {
            return unitRegistry.Register(model, actor, hitboxRoot);
        }

        /// <summary>전달된 <c>model</c> 값을 사용해 <c>Unit</c>를 소유 런타임 Registry에서 등록 해제한다.</summary>
        private bool UnregisterUnit(UnitCombatState model)
        {
            return unitRegistry.Unregister(model);
        }

        /// <summary>전달된 <c>model</c> 값을 사용해 <c>DespawnUnit</c> 조건을 평가하고 결과를 반환한다.</summary>
        internal bool DespawnUnit(UnitCombatState model)
        {
            var entry = Find(model);
            if (entry == null)
            {
                return false;
            }

            UnregisterUnit(model);
            Destroy(entry.Actor.gameObject);
            return true;
        }

        /// <summary>전달된 <c>model</c> 값을 사용해 <c>DefeatUnit</c> 작업을 수행한다.</summary>
        internal void DefeatUnit(UnitCombatState model)
        {
            var entry = Find(model);
            if (entry == null)
            {
                return;
            }

            UnregisterUnit(model);
            entry.HandleDefeat();
        }

        /// <summary>전달된 런타임 입력값을 사용해 <c>ReviveExistingPlayerBySlot</c> 작업을 시도하고 성공 여부를 반환한다.</summary>
        private bool TryReviveExistingPlayerBySlot(
            RunSession activeSession,
            int slotIndex,
            out CombatUnitEntry revivedEntry)
        {
            revivedEntry = null;
            var actor = FindExistingPlayerActorBySlot(slotIndex);
            if (actor == null)
            {
                return false;
            }

            var model = actor.Model;

            SyncExistingMonsterModelFromSession(activeSession, model);
            MonsterDayRecovery.Restore(model);
            actor.Revive();
            revivedEntry = RegisterPlayer(model, actor, actor.transform);
            return true;
        }

        /// <summary>전달된 런타임 입력값을 사용해 <c>ExistingMonsterModelFromSession</c>를 현재 원본 상태와 동기화한다.</summary>
        private void SyncExistingMonsterModelFromSession(
            RunSession activeSession,
            UnitCombatState model)
        {
            var state = activeSession.GetPartyMemberState(model.Identity.DefinitionId)
                ?? throw new InvalidOperationException(
                    $"Party state '{model.Identity.DefinitionId}' is required before restoring.");
            model.Skills = state.Skills;
            SkillExecution.RebuildLearnedSkillState(model);
        }

        /// <summary>전달된 <c>slotIndex</c> 값을 사용해 <c>ExistingPlayerActorBySlot</c>를 찾는다.</summary>
        private static MonsterActor FindExistingPlayerActorBySlot(int slotIndex)
        {

            var actors = Resources.FindObjectsOfTypeAll<MonsterActor>();
            for (var i = 0; i < actors.Length; i++)
            {
                var actor = actors[i];
                if (!actor.gameObject.scene.IsValid())
                {
                    continue;
                }

                var identity = actor.Model.Identity;
                if (identity.Side != UnitSide.Player
                    || identity.Role != UnitRole.Monster
                    || identity.SlotIndex != slotIndex)
                {
                    continue;
                }

                return actor;
            }

            return null;
        }

        /// <summary>전달된 <c>session</c> 값을 사용해 <c>SelectedModel</c>를 생성한다.</summary>
        private UnitCombatState CreateSelectedModel(RunSession session)
        {
            var monster = ResolveMonsterDefinition(session.SelectedMonsterId);
            var runState = session.GetPartyMemberState(monster.MonsterId)
                ?? throw new InvalidOperationException($"Party state '{monster.MonsterId}' is required before spawning.");
            var model = unitStateFactory.CreateSelectedMonster(monster, runState, 0);
            SkillExecution.RebuildLearnedSkillState(model);
            return model;
        }

        /// <summary>전달된 런타임 입력값을 사용해 <c>EnemyUnit</c>를 런타임 씬 오브젝트로 생성하고 등록한다.</summary>
        private GameObject SpawnEnemyUnit(
            GameObject prefab,
            string enemyId,
            int spawnIndex,
            float spawnX,
            float spawnYMin,
            float spawnYMax,
            float healthMultiplier,
            bool isBoss)
        {
            var model = CreateEnemyModel(enemyId, spawnIndex, isBoss);
            ApplyEnemyHealthMultiplier(model, healthMultiplier);

            var spawnPosition = new Vector3(
                spawnX,
                UnityEngine.Random.Range(spawnYMin, spawnYMax),
                enemySpawnPoint.position.z);
            var spawnRotation = enemySpawnPoint.rotation;
            var spawnedUnit = Instantiate(prefab, spawnPosition, spawnRotation, runtimeEnemyRoot);
            spawnedUnit.name = $"{prefab.name}_Enemy_{spawnIndex}";

            var actor = BindEnemyActor(spawnedUnit, model);
            RegisterEnemy(model, actor, spawnedUnit.transform);
            return spawnedUnit;
        }

        /// <summary>전달된 런타임 입력값을 사용해 <c>EnemyModel</c>를 생성한다.</summary>
        private EnemyCombatState CreateEnemyModel(string enemyId, int slotIndex, bool isBoss)
        {
            var enemy = ResolveEnemyDefinition(enemyId);
            var model = unitStateFactory.CreateEnemy(enemy, slotIndex, isBoss);

            SkillExecution.RebuildLearnedSkillState(
                model,
                enemy.ActiveSkills,
                enemy.PassiveSkill == null
                    ? Array.Empty<PassiveSkillDefinition>()
                    : new[] { enemy.PassiveSkill });
            return model;
        }

        /// <summary>전달된 <c>monsterId</c> 값을 사용해 <c>MonsterDefinition</c>를 결정한다.</summary>
        private MonsterDefinition ResolveMonsterDefinition(string monsterId)
        {
            return GameDataLoader.CurrentCatalog.GetData<MonsterDefinition>(monsterId)
                ?? throw new InvalidOperationException($"Monster data '{monsterId}' is required.");
        }

        /// <summary>전달된 <c>enemyId</c> 값을 사용해 <c>EnemyDefinition</c>를 결정한다.</summary>
        private EnemyDefinition ResolveEnemyDefinition(string enemyId)
        {
            return GameDataLoader.CurrentCatalog.GetData<EnemyDefinition>(enemyId)
                ?? throw new InvalidOperationException($"Enemy data '{enemyId}' is required.");
        }

        /// <summary>전달된 런타임 입력값을 사용해 <c>MonsterActor</c>를 런타임 사건 또는 씬 대상에 연결한다.</summary>
        private MonsterActor BindMonsterActor(GameObject spawnedUnit, UnitCombatState model)
        {
            var actor = spawnedUnit.GetComponentInChildren<MonsterActor>(true);
            actor.Initialize(model);
            return actor;
        }

        /// <summary>전달된 런타임 입력값을 사용해 <c>EnemyActor</c>를 런타임 사건 또는 씬 대상에 연결한다.</summary>
        private EnemyActor BindEnemyActor(GameObject spawnedUnit, EnemyCombatState model)
        {
            var actor = spawnedUnit.GetComponentInChildren<EnemyActor>(true);
            actor.Initialize(model);
            return actor;
        }

        /// <summary>전달된 런타임 입력값을 사용해 <c>Player</c>를 소유 런타임 Registry에 등록한다.</summary>
        private CombatUnitEntry RegisterPlayer(UnitCombatState model, MonsterActor actor, Transform hitboxRoot)
        {
            var entry = RegisterUnit(model, actor, hitboxRoot);
            combatManager.NotifyPlayerUnitRegistered(model);
            return entry;
        }

        /// <summary>전달된 런타임 입력값을 사용해 <c>Enemy</c>를 소유 런타임 Registry에 등록한다.</summary>
        private CombatUnitEntry RegisterEnemy(EnemyCombatState model, EnemyActor actor, Transform hitboxRoot)
        {
            var entry = RegisterUnit(model, actor, hitboxRoot);
            combatManager.NotifyEnemyUnitRegistered(model);
            return entry;
        }

        /// <summary>전달된 <c>monsterId</c> 값을 사용해 <c>MonsterPrefab</c>를 결정한다.</summary>
        private GameObject ResolveMonsterPrefab(string monsterId)
        {
            if (string.Equals(monsterId, ArielMonsterId, StringComparison.OrdinalIgnoreCase))
            {
                return RequirePrefab(arielUnitPrefab, monsterId);
            }

            if (string.Equals(monsterId, EveMonsterId, StringComparison.OrdinalIgnoreCase))
            {
                return RequirePrefab(eveUnitPrefab, monsterId);
            }

            if (string.Equals(monsterId, RinMonsterId, StringComparison.OrdinalIgnoreCase))
            {
                return RequirePrefab(rinUnitPrefab, monsterId);
            }

            if (string.Equals(monsterId, SeinMonsterId, StringComparison.OrdinalIgnoreCase))
            {
                return RequirePrefab(seinUnitPrefab, monsterId);
            }

            if (string.Equals(monsterId, VegaMonsterId, StringComparison.OrdinalIgnoreCase))
            {
                return RequirePrefab(vegaUnitPrefab, monsterId);
            }

            throw new InvalidOperationException($"Monster prefab '{monsterId}' is required.");
        }

        /// <summary>전달된 <c>enemyId</c> 값을 사용해 <c>EnemyPrefab</c>를 결정한다.</summary>
        private GameObject ResolveEnemyPrefab(string enemyId)
        {
            for (var i = 0; i < enemyPrefabBindings.Length; i++)
            {
                var binding = enemyPrefabBindings[i];
                if (string.Equals(enemyId, binding.EnemyId, StringComparison.OrdinalIgnoreCase))
                {
                    return RequirePrefab(binding.Prefab, enemyId);
                }
            }

            throw new InvalidOperationException($"Enemy prefab '{enemyId}' is required.");
        }

        /// <summary>전달된 런타임 입력값을 사용해 <c>RequirePrefab</c> 결과값을 생성해 반환한다.</summary>
        private static GameObject RequirePrefab(GameObject prefab, string unitId)
        {
            return prefab != null
                ? prefab
                : throw new InvalidOperationException($"Unit prefab '{unitId}' is required.");
        }

        /// <summary>전달된 런타임 입력값을 사용해 <c>EnemyHealthMultiplier</c>를 적용한다.</summary>
        private static void ApplyEnemyHealthMultiplier(EnemyCombatState model, float healthMultiplier)
        {
            if (healthMultiplier <= 0f)
            {
                throw new InvalidOperationException("Enemy health multiplier must be greater than zero.");
            }

            if (Mathf.Approximately(healthMultiplier, 1f))
            {
                return;
            }

            model.Stats.MaxHealth *= healthMultiplier;
            model.Resources.CurrentHealth *= healthMultiplier;
        }

        /// <summary>전달된 <c>partySlotIndex</c> 값을 사용해 <c>ManifestSpawnPoint</c>를 결정한다.</summary>
        private static Transform ResolveManifestSpawnPoint(int partySlotIndex)
        {
            return GameObject.Find($"{partySlotIndex + 1}PSpawnPoint").transform;
        }
    }

}
