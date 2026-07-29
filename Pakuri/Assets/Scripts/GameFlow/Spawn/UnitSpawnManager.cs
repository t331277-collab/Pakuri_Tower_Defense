using System;
using System.Collections.Generic;
using Pakuri.Data;
using UnityEngine;

/*
 * 런 세션 상태를 실제 전투 유닛으로 만드는 생성 관리 컴포넌트.
 * 선택 몬스터와 현현 파티를 생성·복원하고 스테이지 적을 생성하며
 * 모델 작성, 프리팹 생성, 유닛 액터 연결, 전투 유닛 목록 등록을 이어준다.
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
        public void RegisterNexus(NexusActor actor /* 화면에서 유닛을 표현하는 컴포넌트 */)
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
        public void SpawnSelectedPlayerUnit(RunSession session /* 현재 게임 진행 상태 */)
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
            RunSession session /* 현재 게임 진행 상태 */,
            out UnitCombatState model /* 전투 상태를 읽거나 변경할 유닛 */,
            out MonsterActor actor /* 화면에서 유닛을 표현하는 컴포넌트 */)
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
            RunSession session /* 현재 게임 진행 상태 */,
            MonsterDefinition monster /* 몬스터 */,
            int partySlotIndex /* 파티 슬롯 순서 번호 */)
        {
            return CreateManifestedMonster(monster, session, partySlotIndex);
        }

        /*
         * 세션 상태로 현현 몬스터를 만들고 지정 파티 슬롯에 등록한다.
         */
        private GameObject CreateManifestedMonster(
            MonsterDefinition monster /* 몬스터 */,
            RunSession activeSession /* 현재 활성화된 게임 진행 상태 */,
            int partySlotIndex /* 파티 슬롯 순서 번호 */)
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

        /*
         * 기존 세션 상태로 선택 몬스터의 모델과 Actor를 다시 만든다.
         */
        private GameObject RespawnSelectedPlayerUnit(
            RunSession activeSession /* 현재 활성화된 게임 진행 상태 */,
            out UnitCombatState model /* 전투 상태를 읽거나 변경할 유닛 */,
            out MonsterActor actor /* 화면에서 유닛을 표현하는 컴포넌트 */)
        {
            var monster = ResolveMonsterDefinition(activeSession.SelectedMonsterId);
            var prefab = ResolveMonsterPrefab(monster.MonsterId);

            var runState = activeSession.GetPartyMemberState(monster.MonsterId)
                ?? throw new InvalidOperationException($"Party state '{monster.MonsterId}' is required before respawning.");
            model = unitStateFactory.CreateSelectedMonster(monster, runState, 0);
            SkillExecution.RebuildLearnedSkillState(model);

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
        public void RestorePlayerPartyFromSession(RunSession session /* 현재 게임 진행 상태 */)
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
            RunSession activeSession /* 현재 활성화된 게임 진행 상태 */,
            out GameObject selectedPlayerUnit /* 선택된 플레이어 유닛 */,
            out UnitCombatState selectedPlayerModel /* 선택된 플레이어 상태 모델 */)
        {
            RestoreSelectedPlayerFromSession(
                activeSession,
                out selectedPlayerUnit,
                out selectedPlayerModel);
            RestoreAdditionalPlayersFromSession(activeSession);
        }

        /*
         * 적 ID와 인카운터 생성값으로 적 유닛을 만든다.
         */
        public GameObject SpawnEnemyById(
            string enemyId /* 적 식별자 */,
            int spawnIndex /* 생성 순서 번호 */,
            float spawnX /* 생성 X축 */,
            float spawnYMin /* 생성 Y축 최소 */,
            float spawnYMax /* 생성 Y축 최대 */,
            float healthMultiplier /* 체력 배율 */,
            bool isBoss /* 여부 보스 여부 */)
        {
            var prefab = ResolveEnemyPrefab(enemyId);
            return SpawnEnemyUnit(prefab, enemyId, spawnIndex, spawnX, spawnYMin, spawnYMax, healthMultiplier, isBoss);
        }

        /*
         * 선택 플레이어를 로스터, 기존 Actor, 새 생성 순서로 복원한다.
         */
        private void RestoreSelectedPlayerFromSession(
            RunSession activeSession /* 현재 활성화된 게임 진행 상태 */,
            out GameObject selectedPlayerUnit /* 선택된 플레이어 유닛 */,
            out UnitCombatState selectedPlayerModel /* 선택된 플레이어 상태 모델 */)
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
         * 세션 파티의 2P 이후 몬스터를 슬롯별로 부활하거나 다시 생성한다.
         */
        private void RestoreAdditionalPlayersFromSession(RunSession activeSession /* 현재 활성화된 게임 진행 상태 */)
        {
            for (var slotIndex = 1; slotIndex < activeSession.PartyMembers.Count; slotIndex++)
            {
                if (FindPlayerEntryBySlot(slotIndex) != null)
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

        /*
         * 선택 플레이어 로스터 항목의 GameObject와 모델을 반환한다.
         */
        private static void CaptureSelectedPlayer(
            CombatUnitEntry entry /* 처리할 등록 정보 */,
            out GameObject selectedPlayerUnit /* 선택된 플레이어 유닛 */,
            out UnitCombatState selectedPlayerModel /* 선택된 플레이어 상태 모델 */)
        {
            var actor = (MonsterActor)entry.Actor;
            selectedPlayerUnit = actor.gameObject;
            selectedPlayerModel = entry.Model;
        }

        /*
         * 플레이어 로스터에서 지정 파티 슬롯의 몬스터를 찾는다.
         */
        private CombatUnitEntry FindPlayerEntryBySlot(int slotIndex /* 배치할 슬롯 순서 번호 */)
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
            RunSession activeSession /* 현재 활성화된 게임 진행 상태 */,
            int slotIndex /* 배치할 슬롯 순서 번호 */,
            out CombatUnitEntry revivedEntry /* 부활한 등록 정보 */)
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
            RunSession activeSession /* 현재 활성화된 게임 진행 상태 */,
            UnitCombatState model /* 전투 상태를 읽거나 변경할 유닛 */)
        {
            var state = activeSession.GetPartyMemberState(model.Identity.DefinitionId)
                ?? throw new InvalidOperationException(
                    $"Party state '{model.Identity.DefinitionId}' is required before restoring.");
            model.Skills.ApplyLearnedSkills(
                state.LearnedActives,
                state.LearnedPassives,
                state.ChosenChoiceIds);
            SkillExecution.RebuildLearnedSkillState(model);
        }

        /*
         * 로드된 씬에서 지정 플레이어 슬롯의 기존 Actor를 찾는다.
         */
        private static MonsterActor FindExistingPlayerActorBySlot(int slotIndex /* 배치할 슬롯 순서 번호 */)
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
        private UnitCombatState CreateSelectedModel(RunSession session /* 현재 게임 진행 상태 */)
        {
            var monster = ResolveMonsterDefinition(session.SelectedMonsterId);
            var runState = session.GetPartyMemberState(monster.MonsterId)
                ?? throw new InvalidOperationException($"Party state '{monster.MonsterId}' is required before spawning.");
            var model = unitStateFactory.CreateSelectedMonster(monster, runState, 0);
            SkillExecution.RebuildLearnedSkillState(model);
            return model;
        }

        /*
         * 적 모델과 Actor를 만들고 무작위 Y 위치에 생성한 뒤 로스터에 등록한다.
         */
        private GameObject SpawnEnemyUnit(
            GameObject prefab /* 생성할 프리팹 */,
            string enemyId /* 적 식별자 */,
            int spawnIndex /* 생성 순서 번호 */,
            float spawnX /* 생성 X축 */,
            float spawnYMin /* 생성 Y축 최소 */,
            float spawnYMax /* 생성 Y축 최대 */,
            float healthMultiplier /* 체력 배율 */,
            bool isBoss /* 여부 보스 여부 */)
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
        private EnemyCombatState CreateEnemyModel(string enemyId /* 적 식별자 */, int slotIndex /* 배치할 슬롯 순서 번호 */, bool isBoss /* 여부 보스 여부 */)
        {
            var enemy = ResolveEnemyDefinition(enemyId);
            var model = unitStateFactory.CreateEnemy(enemy, slotIndex, isBoss);
            // 로스터 등록 전에 A/B 스킬과 전투 시작 Trigger 런타임을 완성한다.
            SkillExecution.RebuildAssignedSkillState(model, enemy.ActiveSkills, enemy.SkillTriggers);
            return model;
        }

        /*
         * 몬스터 ID에 대응하는 필수 정의를 반환한다.
         */
        private MonsterDefinition ResolveMonsterDefinition(string monsterId /* 몬스터 식별자 */)
        {
            return GameDataLoader.CurrentCatalog.GetData<MonsterDefinition>(monsterId)
                ?? throw new InvalidOperationException($"Monster data '{monsterId}' is required.");
        }

        /*
         * 적 ID에 대응하는 필수 정의를 반환한다.
         */
        private EnemyDefinition ResolveEnemyDefinition(string enemyId /* 적 식별자 */)
        {
            return GameDataLoader.CurrentCatalog.GetData<EnemyDefinition>(enemyId)
                ?? throw new InvalidOperationException($"Enemy data '{enemyId}' is required.");
        }

        /*
         * 생성된 몬스터 프리팹의 Actor를 런타임 모델로 초기화한다.
         */
        private MonsterActor BindMonsterActor(GameObject spawnedUnit /* 생성된 유닛 */, UnitCombatState model /* 전투 상태를 읽거나 변경할 유닛 */)
        {
            var actor = spawnedUnit.GetComponentInChildren<MonsterActor>(true);
            actor.Initialize(model);
            return actor;
        }

        /*
         * 생성된 적 프리팹의 Actor를 런타임 모델로 초기화한다.
         */
        private EnemyActor BindEnemyActor(GameObject spawnedUnit /* 생성된 유닛 */, EnemyCombatState model /* 처리할 상태 모델 */)
        {
            var actor = spawnedUnit.GetComponentInChildren<EnemyActor>(true);
            actor.Initialize(model);
            return actor;
        }

        /*
         * 몬스터 모델과 Actor를 플레이어 로스터에 등록한다.
         */
        private void RegisterPlayer(UnitCombatState model /* 전투 상태를 읽거나 변경할 유닛 */, MonsterActor actor /* 화면에서 유닛을 표현하는 컴포넌트 */, Transform hitboxRoot /* 피격 판정의 기준 위치 */)
        {
            combatManager.RegisterPlayerMonster(model, actor, hitboxRoot);
        }

        /*
         * 적 모델과 Actor를 적 로스터에 등록한다.
         */
        private void RegisterEnemy(EnemyCombatState model /* 처리할 상태 모델 */, EnemyActor actor /* 화면에서 유닛을 표현하는 컴포넌트 */, Transform hitboxRoot /* 피격 판정의 기준 위치 */)
        {
            combatManager.RegisterEnemy(model, actor, hitboxRoot);
        }

        /*
         * 몬스터 ID에 연결된 필수 프리팹을 반환한다.
         */
        private GameObject ResolveMonsterPrefab(string monsterId /* 몬스터 식별자 */)
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
        private GameObject ResolveEnemyPrefab(string enemyId /* 적 식별자 */)
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
        private static GameObject RequirePrefab(GameObject prefab /* 생성할 프리팹 */, string unitId /* 유닛 식별자 */)
        {
            return prefab != null
                ? prefab
                : throw new InvalidOperationException($"Unit prefab '{unitId}' is required.");
        }

        /*
         * 적의 최대 체력과 현재 체력에 인카운터 체력 배율을 적용한다.
         */
        private static void ApplyEnemyHealthMultiplier(EnemyCombatState model /* 처리할 상태 모델 */, float healthMultiplier /* 체력 배율 */)
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
        private static Transform ResolveManifestSpawnPoint(int partySlotIndex /* 파티 슬롯 순서 번호 */)
        {
            return GameObject.Find($"{partySlotIndex + 1}PSpawnPoint").transform;
        }
    }

}
