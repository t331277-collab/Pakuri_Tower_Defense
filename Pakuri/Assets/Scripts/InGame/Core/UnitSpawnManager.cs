using System;
using System.Collections.Generic;
using Pakuri.Data;
using Pakuri.Run;
using UnityEngine;

namespace Pakuri.InGame
{
    /*
     * 런 세션을 시작하고 몬스터와 적의 생성, 부활, 로스터 등록을 관리한다.
     */

    public class UnitSpawnManager : MonoBehaviour
    {
        private const string ArielMonsterId = "ariel";
        private const string EveMonsterId = "eve";
        private const string RinMonsterId = "rin";
        private const string SeinMonsterId = "sein";
        private const string VegaMonsterId = "vega";
        private readonly UnitFactory unitFactory = new UnitFactory();

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

        public MonsterUnitRuntimeModel SpawnedPlayerModel { get; private set; }
        public RunSession ActiveSession { get; private set; }

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
         * 런에서 선택된 플레이어 몬스터를 생성한다.
         */
        private void Start()
        {
            SpawnSelectedPlayerUnit();
        }

        /*
         * 시작 컨텍스트의 선택 몬스터로 세션과 플레이어 유닛을 만든다.
         */
        public void SpawnSelectedPlayerUnit()
        {
            if (spawnedPlayerUnit != null)
            {
                return;
            }

            var selectedMonsterId = StartContext.SelectedMonsterId;
            spawnedPlayerUnit = CreateSelectedPlayerUnit(
                selectedMonsterId,
                out var model,
                out _,
                out var session);

            SpawnedPlayerModel = model;
            ActiveSession = session;
            // 씬 전환용 선택값은 세션을 만든 뒤 한 번만 소비한다.
            StartContext.Clear();
        }

        /*
         * 선택 몬스터의 새 세션, 모델, Actor를 만들고 플레이어로 등록한다.
         */
        private GameObject CreateSelectedPlayerUnit(
            string selectedMonsterId,
            out MonsterUnitRuntimeModel model,
            out MonsterUnitActor actor,
            out RunSession session)
        {
            var prefab = ResolveMonsterPrefab(selectedMonsterId);
            model = CreateSelectedModel(selectedMonsterId, out session);

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
            MonsterDefinition monster,
            int partySlotIndex)
        {
            return CreateManifestedMonster(monster, ActiveSession, partySlotIndex);
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
            var model = unitFactory.CreateManifestedMonster(monster, runState, partySlotIndex);
            SkillRuntimeFactory.RebuildLearnedActiveSet(model);

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
            out MonsterUnitRuntimeModel model,
            out MonsterUnitActor actor)
        {
            var catalog = ResolveCatalog();
            var monster = ResolveMonsterDefinition(activeSession.SelectedMonsterId);
            var prefab = ResolveMonsterPrefab(monster.MonsterId);

            // 저장된 파티 상태가 없으면 현재 몬스터 정의로 새 상태를 만든다.
            var runState = activeSession.GetPartyMemberState(monster.MonsterId) ?? activeSession.EnsurePartyMemberState(monster);
            model = unitFactory.CreateSelectedMonster(monster, runState, 0);
            SkillRuntimeFactory.RebuildLearnedActiveSet(model);

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
        public void RestorePlayerPartyFromSession()
        {
            RestorePlayerParty(
                ActiveSession,
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
            out MonsterUnitRuntimeModel selectedPlayerModel)
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
            out MonsterUnitRuntimeModel selectedPlayerModel)
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
            var catalog = ResolveCatalog();

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
                var monster = CsvDataLoader.CurrentCatalog.ResolveMonster(monsterId)
                    ?? throw new InvalidOperationException($"Manifested monster data '{monsterId}' is required.");

                CreateManifestedMonster(monster, activeSession, slotIndex);
            }
        }

        /*
         * 선택 플레이어 로스터 항목의 GameObject와 모델을 반환한다.
         */
        private static void CaptureSelectedPlayer(
            UnitRosterEntry entry,
            out GameObject selectedPlayerUnit,
            out MonsterUnitRuntimeModel selectedPlayerModel)
        {
            var actor = (MonsterUnitActor)entry.Actor;
            selectedPlayerUnit = actor.gameObject;
            selectedPlayerModel = (MonsterUnitRuntimeModel)entry.Model;
        }

        /*
         * 플레이어 로스터에서 지정 파티 슬롯의 몬스터를 찾는다.
         */
        private UnitRosterEntry FindPlayerEntryBySlot(int slotIndex)
        {
            var players = combatManager.Roster.Players;
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
            out UnitRosterEntry revivedEntry)
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
            MonsterUnitRuntimeStateService.RestoreForNextDay(model);
            actor.ReviveForNextDay();
            revivedEntry = combatManager.RegisterPlayerMonster(model, actor, actor.transform);
            return true;
        }

        /*
         * 기존 몬스터 모델의 학습 스킬과 선택 정보를 세션 상태에 맞춘다.
         */
        private void SyncExistingMonsterModelFromSession(
            RunSession activeSession,
            MonsterUnitRuntimeModel model)
        {
            var state = activeSession.GetPartyMemberState(model.Identity.DefinitionId);
            CopyListToSet(state.LearnedActives, model.State.LearnedActiveSkillIds);
            CopyListToSet(state.LearnedPassives, model.State.LearnedPassiveSkillIds);
            CopyListToSet(state.ChosenChoiceIds, model.State.ChosenChoiceIds);
            SkillRuntimeFactory.RebuildLearnedActiveSet(model);
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
        private static MonsterUnitActor FindExistingPlayerActorBySlot(int slotIndex)
        {
            // 전체 로드 객체 검색에는 에셋도 포함되므로 실제 씬에 속한 Actor만 사용한다.
            var actors = Resources.FindObjectsOfTypeAll<MonsterUnitActor>();
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
         * 선택 몬스터의 새 RunSession과 런타임 모델을 만든다.
         */
        private MonsterUnitRuntimeModel CreateSelectedModel(
            string monsterId,
            out RunSession session)
        {
            var catalog = ResolveCatalog();
            var monster = ResolveMonsterDefinition(monsterId);
            session = RunSession.Begin(monster);
            var model = unitFactory.CreateSelectedMonster(monster, session.GetPartyMemberState(monster.MonsterId), 0);
            SkillRuntimeFactory.RebuildLearnedActiveSet(model);
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
        private EnemyUnitRuntimeModel CreateEnemyModel(string enemyId, int slotIndex, bool isBoss)
        {
            var enemy = ResolveEnemyDefinition(enemyId);
            var model = unitFactory.CreateEnemy(enemy, slotIndex, isBoss);
            // 로스터 등록 전에 A/B 스킬과 전투 시작 Trigger 런타임을 완성한다.
            SkillRuntimeFactory.RebuildAssignedActiveSet(model, enemy.ActiveSkills, enemy.SkillTriggers);
            return model;
        }

        /*
         * 현재 등록된 게임 데이터 카탈로그를 반환한다.
         */
        private GameDataCatalog ResolveCatalog()
        {
            return CsvDataLoader.CurrentCatalog;
        }

        /*
         * 몬스터 ID에 대응하는 필수 정의를 반환한다.
         */
        private MonsterDefinition ResolveMonsterDefinition(string monsterId)
        {
            return CsvDataLoader.CurrentCatalog.GetData<MonsterDefinition>(monsterId)
                ?? throw new InvalidOperationException($"Monster data '{monsterId}' is required.");
        }

        /*
         * 적 ID에 대응하는 필수 정의를 반환한다.
         */
        private EnemyDefinition ResolveEnemyDefinition(string enemyId)
        {
            return CsvDataLoader.CurrentCatalog.GetData<EnemyDefinition>(enemyId)
                ?? throw new InvalidOperationException($"Enemy data '{enemyId}' is required.");
        }

        /*
         * 생성된 몬스터 프리팹의 Actor를 런타임 모델로 초기화한다.
         */
        private MonsterUnitActor BindMonsterActor(GameObject spawnedUnit, MonsterUnitRuntimeModel model)
        {
            var actor = spawnedUnit.GetComponentInChildren<MonsterUnitActor>(true);
            actor.Initialize(model);
            return actor;
        }

        /*
         * 생성된 적 프리팹의 Actor를 런타임 모델로 초기화한다.
         */
        private EnemyUnitActor BindEnemyActor(GameObject spawnedUnit, EnemyUnitRuntimeModel model)
        {
            var actor = spawnedUnit.GetComponentInChildren<EnemyUnitActor>(true);
            actor.Initialize(model);
            return actor;
        }

        /*
         * 몬스터 모델과 Actor를 플레이어 로스터에 등록한다.
         */
        private void RegisterPlayer(MonsterUnitRuntimeModel model, MonsterUnitActor actor, Transform hitboxRoot)
        {
            combatManager.RegisterPlayerMonster(model, actor, hitboxRoot);
        }

        /*
         * 적 모델과 Actor를 적 로스터에 등록한다.
         */
        private void RegisterEnemy(EnemyUnitRuntimeModel model, EnemyUnitActor actor, Transform hitboxRoot)
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
        private static void ApplyEnemyHealthMultiplier(EnemyUnitRuntimeModel model, float healthMultiplier)
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

    /*
     * 씬 전환 사이에 선택 몬스터 ID를 임시 보관한다.
     */
    public static class StartContext
    {
        public static string SelectedMonsterId { get; private set; }

        /*
         * 다음 인게임 씬에서 사용할 선택 몬스터 ID를 저장한다.
         */
        public static void Prepare(string selectedMonsterId)
        {
            SelectedMonsterId = string.IsNullOrWhiteSpace(selectedMonsterId) ? string.Empty : selectedMonsterId;
        }

        /*
         * 사용이 끝난 선택 몬스터 ID를 비운다.
         */
        public static void Clear()
        {
            SelectedMonsterId = string.Empty;
        }
    }
}
