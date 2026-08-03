/*
 * 역할 및 책임: 플레이어·적·Nexus 런타임 유닛을 생성·등록·조회·표시 갱신·제거한다.
 */

using System;
using System.Collections.Generic;
using Pakuri.Data;
using UnityEngine;

namespace Pakuri.InGame
{

    public class UnitSpawnManager : MonoBehaviour
    {
        private readonly UnitCombatStateFactory unitStateFactory = new UnitCombatStateFactory();
        private readonly CombatUnitRegistry unitRegistry = new CombatUnitRegistry();

        [SerializeField] private InGameCombatManager combatManager;
        [SerializeField] private Transform playerSpawnPoint;
        [SerializeField] private Transform enemySpawnPoint;
        [SerializeField] private Transform[] partySpawnPoints = new Transform[5];
        [SerializeField] private Transform runtimeEnemyRoot;
        [SerializeField] private Transform runtimeMonsterRoot;
        [SerializeField] private MonsterPrefabBinding[] monsterPrefabBindings = Array.Empty<MonsterPrefabBinding>();
        [SerializeField] private EnemyPrefabBinding[] enemyPrefabBindings = Array.Empty<EnemyPrefabBinding>();

        public IReadOnlyList<CombatUnitEntry> Entries => unitRegistry.Entries;
        public IReadOnlyList<CombatUnitEntry> Players => unitRegistry.Players;
        public IReadOnlyList<CombatUnitEntry> Enemies => unitRegistry.Enemies;
        public int EnemyCount => unitRegistry.EnemyCount;

        /// 전투 씬에 Nexus 모델을 만들고 표시 Actor와 Registry를 연결한다.
        public void RegisterNexus(NexusActor actor)
        {
            for (var i = 0; i < unitRegistry.Players.Count; i++)
            {
                if (unitRegistry.Players[i].Model.IsNexus)
                {
                    return;
                }
            }

            var model = unitStateFactory.CreateNexus(actor.MaxHealth);
            actor.Initialize(model);
            RegisterUnit(model, actor, actor.transform);
        }

        [Serializable]
        private class MonsterPrefabBinding
        {
            [SerializeField] private string monsterId = string.Empty;
            [SerializeField] private GameObject prefab = null;

            public string MonsterId => monsterId;
            public GameObject Prefab => prefab;
        }

        [Serializable]
        private class EnemyPrefabBinding
        {
            [SerializeField] private string enemyId = string.Empty;
            [SerializeField] private GameObject prefab = null;

            public string EnemyId => enemyId;
            public GameObject Prefab => prefab;
        }

        /// RunSession에서 선택한 몬스터를 플레이어 슬롯 0에 생성한다.
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

        /// RunSession 파티에 추가된 몬스터를 지정 슬롯에 생성한다.
        public GameObject SpawnManifestedMonster(
            RunSession session,
            MonsterDefinition monster,
            int partySlotIndex)
        {
            return CreateManifestedMonster(monster, session, partySlotIndex);
        }

        private GameObject CreateManifestedMonster(
            MonsterDefinition monster,
            RunSession activeSession,
            int partySlotIndex)
        {
            var prefab = ResolveMonsterPrefab(monster.MonsterId);

            var runState = activeSession.GetPartyMemberState(monster.MonsterId)
                ?? throw new InvalidOperationException($"Party state '{monster.MonsterId}' is required before spawning.");
            var model = unitStateFactory.CreateManifestedMonster(monster, runState, partySlotIndex);
            model.SkillState.RebuildLearnedSkillState(model);

            var spawnPoint = ResolveManifestSpawnPoint(partySlotIndex);
            var spawnPosition = spawnPoint.position;
            var spawnRotation = spawnPoint.rotation;
            var spawnedUnit = Instantiate(prefab, spawnPosition, spawnRotation, runtimeMonsterRoot);
            spawnedUnit.name = $"{prefab.name}_{partySlotIndex + 1}P";

            var actor = BindMonsterActor(spawnedUnit, model);
            RegisterPlayer(model, actor, spawnedUnit.transform);
            return spawnedUnit;
        }

        /// 적 정의와 스폰 위치를 사용해 전투 씬에 적을 생성한다.
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

        /// RunSession에서 전달된 파티 정보를 사용해 스테이지를 넘어갈 때 플레이어 파티를 복구한다.
        public void RestorePlayerPartyFromSession(RunSession session)
        {
            for (var slotIndex = 0; slotIndex < session.PartyMembers.Count; slotIndex++)
            {
                if (FindPlayerMonsterBySlot(slotIndex) != null)
                {
                    continue;
                }

                if (TryReviveExistingPlayerBySlot(session, slotIndex, out _))
                {
                    continue;
                }

                if (slotIndex == 0)
                {
                    CreateSelectedPlayerUnit(session, out _, out _);
                    continue;
                }

                var monsterId = session.PartyMembers[slotIndex].MonsterId;
                var monster = GameDataLoader.CurrentCatalog.GetMonster(monsterId)
                    ?? throw new InvalidOperationException($"Party monster data '{monsterId}' is required.");

                CreateManifestedMonster(monster, session, slotIndex);
            }
        }

        /// 플레이어 파티에서 지정 슬롯의 몬스터를 찾는다.
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

        public CombatUnitEntry Find(UnitCombatState model)
        {
            return unitRegistry.Find(model);
        }

        public CombatUnitEntry FindByCollider(Collider2D collider)
        {
            return unitRegistry.FindByCollider(collider);
        }

        public bool RefreshDisplay(UnitCombatState model)
        {
            return unitRegistry.RefreshDisplay(model);
        }

        private CombatUnitEntry RegisterUnit(
            UnitCombatState model,
            Component actor,
            Transform hitboxRoot = null)
        {
            return unitRegistry.Register(model, actor, hitboxRoot);
        }

        private bool UnregisterUnit(UnitCombatState model)
        {
            return unitRegistry.Unregister(model);
        }

        /// 전투가 끝난 유닛을 Registry에서 제거하고 씬 오브젝트를 파괴한다.
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

        /// 쓰러진 유닛을 Registry에서 제거하고 Actor의 패배 연출을 시작한다.
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

        /// 기존 플레이어 몬스터가 남아 있으면 RunSession 상태를 반영해 되살리고 다시 등록한다.
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

        /// 다음 스테이지에 맞춰 기존 몬스터의 학습 스킬 상태를 RunSession과 동기화한다.
        private void SyncExistingMonsterModelFromSession(
            RunSession activeSession,
            UnitCombatState model)
        {
            var state = activeSession.GetPartyMemberState(model.Identity.DefinitionId)
                ?? throw new InvalidOperationException(
                    $"Party state '{model.Identity.DefinitionId}' is required before restoring.");
            model.Skills = state.Skills;
            model.SkillState.RebuildLearnedSkillState(model);
        }

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

        /// 선택한 몬스터의 RunSession 파티 상태를 사용해 전투 모델을 만든다.
        private UnitCombatState CreateSelectedModel(RunSession session)
        {
            var monster = ResolveMonsterDefinition(session.SelectedMonsterId);
            var runState = session.GetPartyMemberState(monster.MonsterId)
                ?? throw new InvalidOperationException($"Party state '{monster.MonsterId}' is required before spawning.");
            var model = unitStateFactory.CreateSelectedMonster(monster, runState, 0);
            model.SkillState.RebuildLearnedSkillState(model);
            return model;
        }

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

        private EnemyCombatState CreateEnemyModel(string enemyId, int slotIndex, bool isBoss)
        {
            var enemy = ResolveEnemyDefinition(enemyId);
            var model = unitStateFactory.CreateEnemy(enemy, slotIndex, isBoss);

            model.SkillState.RebuildLearnedSkillState(
                model,
                enemy.ActiveSkills,
                enemy.PassiveSkill == null
                    ? Array.Empty<PassiveSkillDefinition>()
                    : new[] { enemy.PassiveSkill });
            return model;
        }

        private MonsterDefinition ResolveMonsterDefinition(string monsterId)
        {
            return GameDataLoader.CurrentCatalog.GetData<MonsterDefinition>(monsterId)
                ?? throw new InvalidOperationException($"Monster data '{monsterId}' is required.");
        }

        private EnemyDefinition ResolveEnemyDefinition(string enemyId)
        {
            return GameDataLoader.CurrentCatalog.GetData<EnemyDefinition>(enemyId)
                ?? throw new InvalidOperationException($"Enemy data '{enemyId}' is required.");
        }

        private MonsterActor BindMonsterActor(GameObject spawnedUnit, UnitCombatState model)
        {
            var actor = spawnedUnit.GetComponentInChildren<MonsterActor>(true);
            actor.Initialize(model);
            return actor;
        }

        private EnemyActor BindEnemyActor(GameObject spawnedUnit, EnemyCombatState model)
        {
            var actor = spawnedUnit.GetComponentInChildren<EnemyActor>(true);
            actor.Initialize(model);
            return actor;
        }

        /// 플레이어 유닛을 Registry에 등록하고 전투 관리자에 생성 사실을 알린다.
        private CombatUnitEntry RegisterPlayer(UnitCombatState model, MonsterActor actor, Transform hitboxRoot)
        {
            var entry = RegisterUnit(model, actor, hitboxRoot);
            combatManager.NotifyPlayerUnitRegistered(model);
            return entry;
        }

        /// 적 유닛을 Registry에 등록하고 전투 관리자에 생성 사실을 알린다.
        private CombatUnitEntry RegisterEnemy(EnemyCombatState model, EnemyActor actor, Transform hitboxRoot)
        {
            var entry = RegisterUnit(model, actor, hitboxRoot);
            combatManager.NotifyEnemyUnitRegistered(model);
            return entry;
        }

        private GameObject ResolveMonsterPrefab(string monsterId)
        {
            for (var i = 0; i < monsterPrefabBindings.Length; i++)
            {
                var binding = monsterPrefabBindings[i];
                if (string.Equals(monsterId, binding.MonsterId, StringComparison.OrdinalIgnoreCase))
                {
                    return RequirePrefab(binding.Prefab, monsterId);
                }
            }

            throw new InvalidOperationException($"Monster prefab '{monsterId}' is required.");
        }

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

        private static GameObject RequirePrefab(GameObject prefab, string unitId)
        {
            return prefab != null
                ? prefab
                : throw new InvalidOperationException($"Unit prefab '{unitId}' is required.");
        }

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

        private Transform ResolveManifestSpawnPoint(int partySlotIndex)
        {
            if (partySpawnPoints == null
                || partySlotIndex < 0
                || partySlotIndex >= partySpawnPoints.Length
                || partySpawnPoints[partySlotIndex] == null)
            {
                throw new InvalidOperationException(
                    $"Party spawn point {partySlotIndex + 1} is required in UnitSpawnManager.");
            }

            return partySpawnPoints[partySlotIndex];
        }
    }

}
