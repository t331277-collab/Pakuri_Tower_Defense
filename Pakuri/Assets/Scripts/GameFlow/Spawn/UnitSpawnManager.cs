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

    /// UnitSpawnManager가 담당하는 작업을 조정하고 공유 런타임 상태를 소유한다.
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

        /// 전달된 actor 값을 사용해 Nexus를 소유 런타임 Registry에 등록한다.
        public void RegisterNexus(NexusActor actor)
        {
            var model = unitStateFactory.CreateNexus(actor.MaxHealth);
            actor.Initialize(model);
            RegisterUnit(model, actor, actor.transform);
        }

        /// EnemyPrefabBinding가 나타내는 런타임 값을 보관한다.
        [Serializable]
        private class EnemyPrefabBinding
        {
            [SerializeField] private string enemyId = string.Empty;
            [SerializeField] private GameObject prefab = null;

            public string EnemyId => enemyId;
            public GameObject Prefab => prefab;
        }

        /// 전달된 session 값을 사용해 SelectedPlayerUnit를 런타임 씬 오브젝트로 생성하고 등록한다.
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

        /// 전달된 런타임 입력값을 사용해 SelectedPlayerUnit를 생성한다.
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

        /// 전달된 런타임 입력값을 사용해 ManifestedMonster를 런타임 씬 오브젝트로 생성하고 등록한다.
        public GameObject SpawnManifestedMonster(
            RunSession session,
            MonsterDefinition monster,
            int partySlotIndex)
        {
            return CreateManifestedMonster(monster, session, partySlotIndex);
        }

        /// 전달된 런타임 입력값을 사용해 ManifestedMonster를 생성한다.
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

        /// 전달된 session 값을 사용해 RestorePlayerPartyFromSession 작업을 수행한다.
        public void RestorePlayerPartyFromSession(RunSession session)
        {
            RestoreSelectedPlayerFromSession(session);
            RestoreAdditionalPlayersFromSession(session);
        }

        /// 전달된 런타임 입력값을 사용해 EnemyById를 런타임 씬 오브젝트로 생성하고 등록한다.
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

        /// 전달된 activeSession 값을 사용해 RestoreSelectedPlayerFromSession 작업을 수행한다.
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

        /// 전달된 activeSession 값을 사용해 RestoreAdditionalPlayersFromSession 작업을 수행한다.
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

        /// 전달된 slotIndex 값을 사용해 PlayerMonsterBySlot를 찾는다.
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

        /// 전달된 model 값을 사용해 요청값를 찾는다.
        public CombatUnitEntry Find(UnitCombatState model)
        {
            return unitRegistry.Find(model);
        }

        /// 전달된 collider 값을 사용해 ByCollider를 찾는다.
        public CombatUnitEntry FindByCollider(Collider2D collider)
        {
            return unitRegistry.FindByCollider(collider);
        }

        /// 전달된 model 값을 사용해 Display를 현재 런타임 모델을 기준으로 갱신한다.
        public bool RefreshDisplay(UnitCombatState model)
        {
            return unitRegistry.RefreshDisplay(model);
        }

        /// 전달된 런타임 입력값을 사용해 Unit를 소유 런타임 Registry에 등록한다.
        private CombatUnitEntry RegisterUnit(
            UnitCombatState model,
            Component actor,
            Transform hitboxRoot = null)
        {
            return unitRegistry.Register(model, actor, hitboxRoot);
        }

        /// 전달된 model 값을 사용해 Unit를 소유 런타임 Registry에서 등록 해제한다.
        private bool UnregisterUnit(UnitCombatState model)
        {
            return unitRegistry.Unregister(model);
        }

        /// 전달된 model 값을 사용해 DespawnUnit 조건을 평가하고 결과를 반환한다.
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

        /// 전달된 model 값을 사용해 DefeatUnit 작업을 수행한다.
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

        /// 전달된 런타임 입력값을 사용해 ReviveExistingPlayerBySlot 작업을 시도하고 성공 여부를 반환한다.
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

        /// 전달된 런타임 입력값을 사용해 ExistingMonsterModelFromSession를 현재 원본 상태와 동기화한다.
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

        /// 전달된 slotIndex 값을 사용해 ExistingPlayerActorBySlot를 찾는다.
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

        /// 전달된 session 값을 사용해 SelectedModel를 생성한다.
        private UnitCombatState CreateSelectedModel(RunSession session)
        {
            var monster = ResolveMonsterDefinition(session.SelectedMonsterId);
            var runState = session.GetPartyMemberState(monster.MonsterId)
                ?? throw new InvalidOperationException($"Party state '{monster.MonsterId}' is required before spawning.");
            var model = unitStateFactory.CreateSelectedMonster(monster, runState, 0);
            SkillExecution.RebuildLearnedSkillState(model);
            return model;
        }

        /// 전달된 런타임 입력값을 사용해 EnemyUnit를 런타임 씬 오브젝트로 생성하고 등록한다.
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

        /// 전달된 런타임 입력값을 사용해 EnemyModel를 생성한다.
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

        /// 전달된 monsterId 값을 사용해 MonsterDefinition를 결정한다.
        private MonsterDefinition ResolveMonsterDefinition(string monsterId)
        {
            return GameDataLoader.CurrentCatalog.GetData<MonsterDefinition>(monsterId)
                ?? throw new InvalidOperationException($"Monster data '{monsterId}' is required.");
        }

        /// 전달된 enemyId 값을 사용해 EnemyDefinition를 결정한다.
        private EnemyDefinition ResolveEnemyDefinition(string enemyId)
        {
            return GameDataLoader.CurrentCatalog.GetData<EnemyDefinition>(enemyId)
                ?? throw new InvalidOperationException($"Enemy data '{enemyId}' is required.");
        }

        /// 전달된 런타임 입력값을 사용해 MonsterActor를 런타임 사건 또는 씬 대상에 연결한다.
        private MonsterActor BindMonsterActor(GameObject spawnedUnit, UnitCombatState model)
        {
            var actor = spawnedUnit.GetComponentInChildren<MonsterActor>(true);
            actor.Initialize(model);
            return actor;
        }

        /// 전달된 런타임 입력값을 사용해 EnemyActor를 런타임 사건 또는 씬 대상에 연결한다.
        private EnemyActor BindEnemyActor(GameObject spawnedUnit, EnemyCombatState model)
        {
            var actor = spawnedUnit.GetComponentInChildren<EnemyActor>(true);
            actor.Initialize(model);
            return actor;
        }

        /// 전달된 런타임 입력값을 사용해 Player를 소유 런타임 Registry에 등록한다.
        private CombatUnitEntry RegisterPlayer(UnitCombatState model, MonsterActor actor, Transform hitboxRoot)
        {
            var entry = RegisterUnit(model, actor, hitboxRoot);
            combatManager.NotifyPlayerUnitRegistered(model);
            return entry;
        }

        /// 전달된 런타임 입력값을 사용해 Enemy를 소유 런타임 Registry에 등록한다.
        private CombatUnitEntry RegisterEnemy(EnemyCombatState model, EnemyActor actor, Transform hitboxRoot)
        {
            var entry = RegisterUnit(model, actor, hitboxRoot);
            combatManager.NotifyEnemyUnitRegistered(model);
            return entry;
        }

        /// 전달된 monsterId 값을 사용해 MonsterPrefab를 결정한다.
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

        /// 전달된 enemyId 값을 사용해 EnemyPrefab를 결정한다.
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

        /// 전달된 런타임 입력값을 사용해 RequirePrefab 결과값을 생성해 반환한다.
        private static GameObject RequirePrefab(GameObject prefab, string unitId)
        {
            return prefab != null
                ? prefab
                : throw new InvalidOperationException($"Unit prefab '{unitId}' is required.");
        }

        /// 전달된 런타임 입력값을 사용해 EnemyHealthMultiplier를 적용한다.
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

        /// 전달된 partySlotIndex 값을 사용해 ManifestSpawnPoint를 결정한다.
        private static Transform ResolveManifestSpawnPoint(int partySlotIndex)
        {
            return GameObject.Find($"{partySlotIndex + 1}PSpawnPoint").transform;
        }
    }

}
