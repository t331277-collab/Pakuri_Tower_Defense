using System;
using System.Collections.Generic;
using Pakuri.Data;
using UnityEngine;

/*
 * 런 세션 상태를 실제 전투 유닛으로 만드는 생성 관리 컴포넌트.
 * 선택 몬스터와 현현 파티를 생성·복원하고 스테이지 적을 생성하며
 * 모델 작성, 프리팹 인스턴스화, Actor 연결, 전투 로스터 등록을 이어준다.
 */
namespace Pakuri.InGame
{

    public class UnitSpawnManager : MonoBehaviour
    {
        private const string ArielMonsterId = "ariel";
        private const string EveMonsterId = "eve";
        private const string RinMonsterId = "rin";
        private const string SeinMonsterId = "sein";
        private const string VegaMonsterId = "vega";
        private readonly UnitCombatStateFactory unitStateFactory = new UnitCombatStateFactory();

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

        private GameObject spawnedPlayerUnit;

        public UnitCombatState SpawnedPlayerModel { get; private set; }

        /*
         * Nexus 상태를 만들고 Actor와 전투 등록소에 연결한다.
         */
        public void RegisterNexus(NexusActor actor)
        {
            var model = unitStateFactory.CreateNexus(actor.MaxHealth);
            actor.Initialize(model);
            combatManager.RegisterNexus(model, actor, actor.transform);
        }

        /*
         * 적 ID와 생성 프리팹의 연결 정보를 보관한다.
         */
        [Serializable]
        private class EnemyPrefabBinding
        {
            [SerializeField] private string enemyId = string.Empty;
            [SerializeField] private GameObject prefab = null;

            public string EnemyId => enemyId;
            public GameObject Prefab => prefab;
        }

        /*
         * 전달받은 세션의 선택 몬스터로 플레이어 유닛을 만든다.
         */
        public void SpawnSelectedPlayerUnit(RunSession session)
        {
            if (spawnedPlayerUnit != null)
            {
                return;
            }

            spawnedPlayerUnit = CreateSelectedPlayerUnit(
                session,
                out var model,
                out _);

            SpawnedPlayerModel = model;
        }

        /*
         * 세션의 선택 몬스터 모델과 Actor를 만들고 플레이어로 등록한다.
         */
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

        /*
         * 현재 세션의 지정 파티 슬롯에 현현 몬스터를 생성한다.
         */
        public GameObject SpawnManifestedMonster(
            RunSession session,
            MonsterDefinition monster,
            int partySlotIndex)
        {
            return CreateManifestedMonster(monster, session, partySlotIndex);
        }

        /*
         * 세션 상태로 현현 몬스터를 만들고 지정 파티 슬롯에 등록한다.
         */
        private GameObject CreateManifestedMonster(
            MonsterDefinition monster,
            RunSession activeSession,
            int partySlotIndex)
        {
            var prefab = ResolveMonsterPrefab(monster.MonsterId);

            // 현현 유닛은 세션 파티 상태를 먼저 확보한 뒤 학습 스킬 런타임을 복원한다.
            var runState = activeSession.EnsurePartyMemberState(monster);
            var model = unitStateFactory.CreateManifestedMonster(monster, runState, partySlotIndex);
            UnitSkillRuntimeBuilder.RebuildLearnedSkillSet(model);

            var spawnPoint = ResolveManifestSpawnPoint(partySlotIndex);
            var spawnPosition = spawnPoint.position;
            var spawnRotation = spawnPoint.rotation;
            var spawnedUnit = Instantiate(prefab, spawnPosition, spawnRotation, runtimeMonsterRoot);
            spawnedUnit.name = $"{prefab.name}_{partySlotIndex + 1}P";

            var actor = BindMonsterActor(spawnedUnit, model);
            RegisterPlayer(model, actor, spawnedUnit.transform);
            return spawnedUnit;
        }

        /*
         * 기존 세션 상태로 선택 몬스터의 모델과 Actor를 다시 만든다.
         */
        private GameObject RespawnSelectedPlayerUnit(
            RunSession activeSession,
            out UnitCombatState model,
            out MonsterActor actor)
        {
            var monster = ResolveMonsterDefinition(activeSession.SelectedMonsterId);
            var prefab = ResolveMonsterPrefab(monster.MonsterId);

            // 저장된 파티 상태가 없으면 현재 몬스터 정의로 새 상태를 만든다.
            var runState = activeSession.GetPartyMemberState(monster.MonsterId) ?? activeSession.EnsurePartyMemberState(monster);
            model = unitStateFactory.CreateSelectedMonster(monster, runState, 0);
            UnitSkillRuntimeBuilder.RebuildLearnedSkillSet(model);

            var spawnPosition = playerSpawnPoint.position;
            var spawnRotation = playerSpawnPoint.rotation;
            var spawnedUnit = Instantiate(prefab, spawnPosition, spawnRotation, runtimeMonsterRoot);
            spawnedUnit.name = $"{prefab.name}_1P";
            actor = BindMonsterActor(spawnedUnit, model);
            RegisterPlayer(model, actor, spawnedUnit.transform);
            return spawnedUnit;
        }

        /*
         * 세션의 선택 몬스터와 현현 파티를 기존 Actor 또는 새 인스턴스로 복원한다.
         */
        public void RestorePlayerPartyFromSession(RunSession session)
        {
            RestorePlayerParty(
                session,
                out spawnedPlayerUnit,
                out var model);

            SpawnedPlayerModel = model;
        }

        /*
         * 전달받은 세션 상태로 선택 몬스터와 현현 파티를 복원한다.
         */
        private void RestorePlayerParty(
            RunSession activeSession,
            out GameObject selectedPlayerUnit,
            out UnitCombatState selectedPlayerModel)
        {
            RestoreSelectedPlayerFromSession(
                activeSession,
                out selectedPlayerUnit,
                out selectedPlayerModel);
            RestoreManifestedPlayersFromSession(activeSession);
        }

        /*
         * 적 ID와 인카운터 생성값으로 적 유닛을 만든다.
         */
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

        /*
         * 선택 플레이어를 로스터, 기존 Actor, 새 생성 순서로 복원한다.
         */
        private void RestoreSelectedPlayerFromSession(
            RunSession activeSession,
            out GameObject selectedPlayerUnit,
            out UnitCombatState selectedPlayerModel)
        {
            // 등록된 로스터, 씬에 남은 Actor, 새 프리팹 생성 순서로 중복 생성을 피한다.
            var selectedEntry = FindPlayerEntryBySlot(0);
            if (selectedEntry != null)
            {
                CaptureSelectedPlayer(selectedEntry, out selectedPlayerUnit, out selectedPlayerModel);
                return;
            }

            if (TryReviveExistingPlayerBySlot(activeSession, 0, out selectedEntry))
            {
                CaptureSelectedPlayer(selectedEntry, out selectedPlayerUnit, out selectedPlayerModel);
                return;
            }

            selectedPlayerUnit = RespawnSelectedPlayerUnit(
                activeSession,
                out selectedPlayerModel,
                out _);
        }

        /*
         * 세션의 현현 몬스터를 슬롯별로 부활하거나 다시 생성한다.
         */
        private void RestoreManifestedPlayersFromSession(RunSession activeSession)
        {
            for (var i = 0; i < activeSession.ManifestedMonsterIds.Count; i++)
            {
                // 선택 몬스터가 1P이므로 현현 목록은 순서대로 2P부터 배치한다.
                var slotIndex = Mathf.Clamp(i + 1, 1, 4);
                if (FindPlayerEntryBySlot(slotIndex) != null)
                {
                    continue;
                }

                if (TryReviveExistingPlayerBySlot(activeSession, slotIndex, out _))
                {
                    continue;
                }

                var monsterId = activeSession.ManifestedMonsterIds[i];
                var monster = GameDataLoader.CurrentCatalog.ResolveMonster(monsterId)
                    ?? throw new InvalidOperationException($"Manifested monster data '{monsterId}' is required.");

                CreateManifestedMonster(monster, activeSession, slotIndex);
            }
        }

        /*
         * 선택 플레이어 로스터 항목의 GameObject와 모델을 반환한다.
         */
        private static void CaptureSelectedPlayer(
            CombatUnitEntry entry,
            out GameObject selectedPlayerUnit,
            out UnitCombatState selectedPlayerModel)
        {
            var actor = (MonsterActor)entry.Actor;
            selectedPlayerUnit = actor.gameObject;
            selectedPlayerModel = entry.Model;
        }

        /*
         * 플레이어 로스터에서 지정 파티 슬롯의 몬스터를 찾는다.
         */
        private CombatUnitEntry FindPlayerEntryBySlot(int slotIndex)
        {
            var players = combatManager.UnitRegistry.Players;
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

        /*
         * 씬에 남아 있는 슬롯 Actor를 세션 상태로 부활시키고 로스터에 등록한다.
         */
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
            // 기존 모델을 세션과 맞춘 뒤 다음 날짜에 필요한 전투 상태만 복구한다.
            SyncExistingMonsterModelFromSession(activeSession, model);
            MonsterDayRecovery.Restore(model);
            actor.Revive();
            revivedEntry = combatManager.RegisterPlayerMonster(model, actor, actor.transform);
            return true;
        }

        /*
         * 기존 몬스터 모델의 학습 스킬과 선택 정보를 세션 상태에 맞춘다.
         */
        private void SyncExistingMonsterModelFromSession(
            RunSession activeSession,
            UnitCombatState model)
        {
            var state = activeSession.GetPartyMemberState(model.Identity.DefinitionId);
            CopyListToSet(state.LearnedActives, model.SkillProgress.LearnedActiveSkillIds);
            CopyListToSet(state.LearnedPassives, model.SkillProgress.LearnedPassiveSkillIds);
            CopyListToSet(state.ChosenChoiceIds, model.SkillProgress.ChosenChoiceIds);
            UnitSkillRuntimeBuilder.RebuildLearnedSkillSet(model);
        }

        /*
         * 문자열 목록의 유효한 항목을 대상 Set에 다시 채운다.
         */
        private static void CopyListToSet(IReadOnlyList<string> source, ISet<string> target)
        {
            target.Clear();
            for (var i = 0; i < source.Count; i++)
            {
                if (!string.IsNullOrWhiteSpace(source[i]))
                {
                    target.Add(source[i]);
                }
            }
        }

        /*
         * 로드된 씬에서 지정 플레이어 슬롯의 기존 Actor를 찾는다.
         */
        private static MonsterActor FindExistingPlayerActorBySlot(int slotIndex)
        {
            // 전체 로드 객체 검색에는 에셋도 포함되므로 실제 씬에 속한 Actor만 사용한다.
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

        /*
         * 세션 상태로 선택 몬스터의 런타임 모델을 만든다.
         */
        private UnitCombatState CreateSelectedModel(RunSession session)
        {
            var monster = ResolveMonsterDefinition(session.SelectedMonsterId);
            var model = unitStateFactory.CreateSelectedMonster(monster, session.GetPartyMemberState(monster.MonsterId), 0);
            UnitSkillRuntimeBuilder.RebuildLearnedSkillSet(model);
            return model;
        }

        /*
         * 적 모델과 Actor를 만들고 무작위 Y 위치에 생성한 뒤 로스터에 등록한다.
         */
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

            // 인카운터가 지정한 Y 범위 안에서 생성 위치를 정한다.
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

        /*
         * 적 정의로 런타임 모델을 만들고 배정 스킬을 구성한다.
         */
        private EnemyCombatState CreateEnemyModel(string enemyId, int slotIndex, bool isBoss)
        {
            var enemy = ResolveEnemyDefinition(enemyId);
            var model = unitStateFactory.CreateEnemy(enemy, slotIndex, isBoss);
            // 로스터 등록 전에 A/B 스킬과 전투 시작 Trigger 런타임을 완성한다.
            UnitSkillRuntimeBuilder.RebuildAssignedActiveSet(model, enemy.ActiveSkills, enemy.SkillTriggers);
            return model;
        }

        /*
         * 몬스터 ID에 대응하는 필수 정의를 반환한다.
         */
        private MonsterDefinition ResolveMonsterDefinition(string monsterId)
        {
            return GameDataLoader.CurrentCatalog.GetData<MonsterDefinition>(monsterId)
                ?? throw new InvalidOperationException($"Monster data '{monsterId}' is required.");
        }

        /*
         * 적 ID에 대응하는 필수 정의를 반환한다.
         */
        private EnemyDefinition ResolveEnemyDefinition(string enemyId)
        {
            return GameDataLoader.CurrentCatalog.GetData<EnemyDefinition>(enemyId)
                ?? throw new InvalidOperationException($"Enemy data '{enemyId}' is required.");
        }

        /*
         * 생성된 몬스터 프리팹의 Actor를 런타임 모델로 초기화한다.
         */
        private MonsterActor BindMonsterActor(GameObject spawnedUnit, UnitCombatState model)
        {
            var actor = spawnedUnit.GetComponentInChildren<MonsterActor>(true);
            actor.Initialize(model);
            return actor;
        }

        /*
         * 생성된 적 프리팹의 Actor를 런타임 모델로 초기화한다.
         */
        private EnemyActor BindEnemyActor(GameObject spawnedUnit, EnemyCombatState model)
        {
            var actor = spawnedUnit.GetComponentInChildren<EnemyActor>(true);
            actor.Initialize(model);
            return actor;
        }

        /*
         * 몬스터 모델과 Actor를 플레이어 로스터에 등록한다.
         */
        private void RegisterPlayer(UnitCombatState model, MonsterActor actor, Transform hitboxRoot)
        {
            combatManager.RegisterPlayerMonster(model, actor, hitboxRoot);
        }

        /*
         * 적 모델과 Actor를 적 로스터에 등록한다.
         */
        private void RegisterEnemy(EnemyCombatState model, EnemyActor actor, Transform hitboxRoot)
        {
            combatManager.RegisterEnemy(model, actor, hitboxRoot);
        }

        /*
         * 몬스터 ID에 연결된 필수 프리팹을 반환한다.
         */
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

        /*
         * 직렬화된 적 프리팹 목록에서 ID와 일치하는 필수 프리팹을 반환한다.
         */
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

        /*
         * 필수 프리팹 참조를 반환하고 누락되면 즉시 오류를 낸다.
         */
        private static GameObject RequirePrefab(GameObject prefab, string unitId)
        {
            return prefab != null
                ? prefab
                : throw new InvalidOperationException($"Unit prefab '{unitId}' is required.");
        }

        /*
         * 적의 최대 체력과 현재 체력에 인카운터 체력 배율을 적용한다.
         */
        private static void ApplyEnemyHealthMultiplier(EnemyCombatState model, float healthMultiplier)
        {
            if (healthMultiplier <= 0f)
            {
                throw new InvalidOperationException("Enemy health multiplier must be greater than zero.");
            }

            // 1배는 기본 체력을 그대로 사용한다.
            if (Mathf.Approximately(healthMultiplier, 1f))
            {
                return;
            }

            model.Stats.MaxHealth *= healthMultiplier;
            model.Resources.CurrentHealth *= healthMultiplier;
        }

        /*
         * 파티 슬롯 번호에 대응하는 씬 생성 지점을 반환한다.
         */
        private static Transform ResolveManifestSpawnPoint(int partySlotIndex)
        {
            return GameObject.Find($"{partySlotIndex + 1}PSpawnPoint").transform;
        }
    }

}
