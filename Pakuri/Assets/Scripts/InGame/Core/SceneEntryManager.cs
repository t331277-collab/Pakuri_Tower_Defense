// 'System.Collections.Generic' 네임스페이스의 타입과 API를 이 파일에서 사용한다.
using System.Collections.Generic;
// 'Pakuri.Data' 네임스페이스의 타입과 API를 이 파일에서 사용한다.
using Pakuri.Data;
// 'Pakuri.Run' 네임스페이스의 타입과 API를 이 파일에서 사용한다.
using Pakuri.Run;
// 'UnityEngine' 네임스페이스의 타입과 API를 이 파일에서 사용한다.
using UnityEngine;

// 'Pakuri.InGame' 네임스페이스 범위를 선언해 관련 타입 이름의 충돌을 막는다.
namespace Pakuri.InGame
{
    // 런 진입 시 선택 몬스터와 적을 생성하고 날짜 전환 시 플레이어 파티를 세션에서 복원한다.
    // [낯선 문법] DisallowMultipleComponent attribute: 같은 GameObject에 이 컴포넌트가 중복 부착되는 것을 막는다.
    [DisallowMultipleComponent]
    // [방어 로직][낯선 문법] RequireComponent attribute: 'InGameCombatManager' 의존 컴포넌트가 같은 GameObject에 있도록 Unity에 요구한다.
    [RequireComponent(typeof(InGameCombatManager))]
    // [방어 로직][낯선 문법] RequireComponent attribute: 'EnemySpawnManger' 의존 컴포넌트가 같은 GameObject에 있도록 Unity에 요구한다.
    [RequireComponent(typeof(EnemySpawnManger))]
    // 'SceneEntryManager' 클래스 정의를 시작한다.
    public class SceneEntryManager : MonoBehaviour
    {
        // [낯선 문법] SerializeField attribute: private 상태 'combatManager'을 Unity 직렬화와 Inspector 편집 대상으로 만든다.
        [SerializeField] private InGameCombatManager combatManager;
        // [낯선 문법] SerializeField attribute: private 상태 'unitSpawnManager'을 Unity 직렬화와 Inspector 편집 대상으로 만든다.
        [SerializeField] private EnemySpawnManger unitSpawnManager;

        // 'spawnedPlayerUnit' 필드 또는 property를 선언해 객체 상태를 보관하거나 공개한다.
        private GameObject spawnedPlayerUnit;

        // 'SpawnedPlayerModel' 읽기 전용 property로 계산 결과 또는 상태를 외부에 공개한다.
        public MonsterUnitRuntimeModel SpawnedPlayerModel { get; private set; }
        // 'ActiveSession' 읽기 전용 property로 계산 결과 또는 상태를 외부에 공개한다.
        public RunSession ActiveSession { get; private set; }
        // [낯선 문법] 식 본문 property: 'UnitSpawnManager' 값을 오른쪽 식 하나로 계산해 반환한다.
        public EnemySpawnManger UnitSpawnManager => unitSpawnManager;

        // 필요한 관리자를 확보하고 런에서 선택된 플레이어 몬스터를 생성한다.
        // 'Start' 메소드의 입력과 반환 계약을 선언한다.
        private void Start()
        {
            // 'SpawnSelectedPlayerUnit' 메소드를 호출해 현재 단계의 처리를 실행한다.
            SpawnSelectedPlayerUnit();
        }

        // StartContext의 선택 몬스터로 새 RunSession과 1P 런타임 유닛을 만든다.
        // 'SpawnSelectedPlayerUnit' 메소드의 입력과 반환 계약을 선언한다.
        public void SpawnSelectedPlayerUnit()
        {
            // [방어 로직] 'spawnedPlayerUnit != null' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
            if (spawnedPlayerUnit != null)
            {
                // [방어 로직] 현재 메소드의 남은 처리를 건너뛰고 즉시 호출 지점으로 돌아간다.
                return;
            }

            // 지역 변수 'selectedMonsterId'에 시작 컨텍스트가 전달한 몬스터 ID를 저장한다.
            var selectedMonsterId = StartContext.SelectedMonsterId;

            // [방어 로직] 'string.IsNullOrWhiteSpace(selectedMonsterId)' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
            if (string.IsNullOrWhiteSpace(selectedMonsterId))
            {
                // [방어 로직] 계속 실행할 수 있지만 잘못된 설정을 경고 로그로 남긴다: "SceneEntryManager started without selected monster data.".
                Debug.LogWarning("SceneEntryManager started without selected monster data.");
                // [방어 로직] 현재 메소드의 남은 처리를 건너뛰고 즉시 호출 지점으로 돌아간다.
                return;
            }

            // [방어 로직] 'unitSpawnManager == null' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
            if (unitSpawnManager == null)
            {
                // 실행을 막는 오류 상태를 로그로 남긴다: "SceneEntryManager cannot spawn the selected monster because EnemySpawnManger is missing.".
                Debug.LogError("SceneEntryManager cannot spawn the selected monster because EnemySpawnManger is missing.");
                // [방어 로직] 현재 메소드의 남은 처리를 건너뛰고 즉시 호출 지점으로 돌아간다.
                return;
            }

            //  줄로 이어지는 조건식을 시작하고 최종 결과로 실행 분기를 결정한다.
            if (!unitSpawnManager.SpawnSelectedPlayerUnit(
                    // 'selectedMonsterId' 열거값을 선택 가능한 상수 항목으로 정의한다.
                    selectedMonsterId,
                    // [낯선 문법] out 인수로 메소드 성공 여부와 함께 추가 결과값을 받아온다.
                    out spawnedPlayerUnit,
                    // [낯선 문법] out 인수로 메소드 성공 여부와 함께 추가 결과값을 받아온다.
                    out var model,
                    // 생성 관리자가 내부에서 Actor를 등록하므로 반환 Actor는 보관하지 않는다.
                    out _,
                    // [낯선 문법] out 인수로 메소드 성공 여부와 함께 추가 결과값을 받아온다.
                    out var session))
            {
                // [방어 로직] 현재 메소드의 남은 처리를 건너뛰고 즉시 호출 지점으로 돌아간다.
                return;
            }

            // 'SpawnedPlayerModel'에 오른쪽 계산, 조회, 또는 상수 결과를 저장한다.
            SpawnedPlayerModel = model;
            // 'ActiveSession'에 오른쪽 계산, 조회, 또는 상수 결과를 저장한다.
            ActiveSession = session;
            // 컬렉션에 남은 항목을 모두 제거해 상태를 초기화한다.
            StartContext.Clear();
        }

        // 적 ID, 위치 범위, 체력 배율, 보스 여부를 생성 관리자에 전달한다.
        // 'SpawnEnemyById' 메소드의 입력과 반환 계약을 선언한다.
        public bool SpawnEnemyById(
            // 'enemyId' 매개변수 또는 지역값의 타입을 'string'로 지정한다.
            string enemyId,
            // 'spawnIndex' 매개변수 또는 지역값의 타입을 'int'로 지정한다.
            int spawnIndex,
            // 'spawnX' 매개변수 또는 지역값의 타입을 'float'로 지정한다.
            float spawnX,
            // 'spawnYMin' 매개변수 또는 지역값의 타입을 'float'로 지정한다.
            float spawnYMin,
            // 'spawnYMax' 매개변수 또는 지역값의 타입을 'float'로 지정한다.
            float spawnYMax,
            // 'healthMultiplier' 매개변수 또는 지역값의 타입을 'float'로 지정한다.
            float healthMultiplier,
            // 'isBoss' 매개변수 또는 지역값의 타입을 'bool'로 지정한다.
            bool isBoss,
            // [낯선 문법] out 인수로 메소드 성공 여부와 함께 추가 결과값을 받아온다.
            out GameObject spawnedUnit)
        {
            // 'spawnedUnit'에 오른쪽 계산, 조회, 또는 상수 결과를 저장한다.
            spawnedUnit = null;
            // [방어 로직] 'unitSpawnManager == null' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
            if (unitSpawnManager == null)
            {
                // 실행을 막는 오류 상태를 로그로 남긴다: "SceneEntryManager cannot spawn enemies because EnemySpawnManger is missing.".
                Debug.LogError("SceneEntryManager cannot spawn enemies because EnemySpawnManger is missing.");
                // [방어 로직] 필수 대상 또는 유효 조건이 없으므로 실패 결과 false를 반환한다.
                return false;
            }

            // 지역 변수 'spawned'에 저장할 여러 줄 계산 또는 선택식을 시작한다.
            var spawned = unitSpawnManager.SpawnEnemyById(
                // 'enemyId' 열거값을 선택 가능한 상수 항목으로 정의한다.
                enemyId,
                // 'spawnIndex' 열거값을 선택 가능한 상수 항목으로 정의한다.
                spawnIndex,
                // 'spawnX' 열거값을 선택 가능한 상수 항목으로 정의한다.
                spawnX,
                // 'spawnYMin' 열거값을 선택 가능한 상수 항목으로 정의한다.
                spawnYMin,
                // 'spawnYMax' 열거값을 선택 가능한 상수 항목으로 정의한다.
                spawnYMax,
                // 'healthMultiplier' 열거값을 선택 가능한 상수 항목으로 정의한다.
                healthMultiplier,
                // 'isBoss' 열거값을 선택 가능한 상수 항목으로 정의한다.
                isBoss,
                // [낯선 문법] out 인수로 메소드 성공 여부와 함께 추가 결과값을 받아온다.
                out spawnedUnit);
            // 계산 또는 조회 결과 'spawned'을 호출자에게 반환한다.
            return spawned;
        }

        // 현재 세션 데이터로 지정 파티 슬롯에 현현 몬스터 생성을 요청한다.
        // 'SpawnManifestedMonster' 메소드의 입력과 반환 계약을 선언한다.
        public bool SpawnManifestedMonster(
            // 'monster' 매개변수 또는 지역값의 타입을 'MonsterDefinition'로 지정한다.
            MonsterDefinition monster,
            // 'partySlotIndex' 매개변수 또는 지역값의 타입을 'int'로 지정한다.
            int partySlotIndex,
            // [낯선 문법] out 인수로 메소드 성공 여부와 함께 추가 결과값을 받아온다.
            out GameObject spawnedUnit)
        {
            // 'spawnedUnit'에 오른쪽 계산, 조회, 또는 상수 결과를 저장한다.
            spawnedUnit = null;
            // [방어 로직] 'unitSpawnManager == null' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
            if (unitSpawnManager == null)
            {
                // 실행을 막는 오류 상태를 로그로 남긴다: "SceneEntryManager cannot manifest monsters because EnemySpawnManger is missing.".
                Debug.LogError("SceneEntryManager cannot manifest monsters because EnemySpawnManger is missing.");
                // [방어 로직] 필수 대상 또는 유효 조건이 없으므로 실패 결과 false를 반환한다.
                return false;
            }

            // 계산 또는 조회 결과 'unitSpawnManager.SpawnManifestedMonster(monster, ActiveSession, partySlotIndex, out spawnedUnit)'을 호출자에게 반환한다.
            return unitSpawnManager.SpawnManifestedMonster(monster, ActiveSession, partySlotIndex, out spawnedUnit);
        }

        // 다음 날짜 시작 시 선택 몬스터와 현현 파티를 기존 Actor 또는 세션 데이터로 복원한다.
        // 'RestorePlayerPartyFromSession' 메소드의 입력과 반환 계약을 선언한다.
        public void RestorePlayerPartyFromSession()
        {
            // [방어 로직] 'ActiveSession == null || combatManager == null || unitSpawnManager == null' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
            if (ActiveSession == null || combatManager == null || unitSpawnManager == null)
            {
                // [방어 로직] 현재 메소드의 남은 처리를 건너뛰고 즉시 호출 지점으로 돌아간다.
                return;
            }

            // 'RestoreSelectedPlayerFromSession' 메소드를 호출해 현재 단계의 처리를 실행한다.
            RestoreSelectedPlayerFromSession();
            // 'RestoreManifestedPlayersFromSession' 메소드를 호출해 현재 단계의 처리를 실행한다.
            RestoreManifestedPlayersFromSession();
        }

        // 1P 몬스터를 로스터 검색, 기존 Actor 부활, 새 인스턴스 생성 순서로 복원한다.
        // 'RestoreSelectedPlayerFromSession' 메소드의 입력과 반환 계약을 선언한다.
        private void RestoreSelectedPlayerFromSession()
        {
            // 지역 변수 'selectedEntry'에 오른쪽 계산 또는 조회 결과를 저장한다.
            var selectedEntry = FindPlayerEntryBySlot(0);
            // [방어 로직] 'selectedEntry != null' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
            if (selectedEntry != null)
            {
                // 'CaptureSelectedPlayerSpawn' 메소드를 호출해 현재 단계의 처리를 실행한다.
                CaptureSelectedPlayerSpawn(selectedEntry);
                // [방어 로직] 현재 메소드의 남은 처리를 건너뛰고 즉시 호출 지점으로 돌아간다.
                return;
            }

            // [방어 로직] 'TryReviveExistingPlayerBySlot(0, out selectedEntry)' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
            if (TryReviveExistingPlayerBySlot(0, out selectedEntry))
            {
                // 'CaptureSelectedPlayerSpawn' 메소드를 호출해 현재 단계의 처리를 실행한다.
                CaptureSelectedPlayerSpawn(selectedEntry);
                // [방어 로직] 현재 메소드의 남은 처리를 건너뛰고 즉시 호출 지점으로 돌아간다.
                return;
            }

            //  줄로 이어지는 조건식을 시작하고 최종 결과로 실행 분기를 결정한다.
            if (!unitSpawnManager.RespawnSelectedPlayerUnit(
                    // 'ActiveSession' 열거값을 선택 가능한 상수 항목으로 정의한다.
                    ActiveSession,
                    // [낯선 문법] out 인수로 메소드 성공 여부와 함께 추가 결과값을 받아온다.
                    out spawnedPlayerUnit,
                    // [낯선 문법] out 인수로 메소드 성공 여부와 함께 추가 결과값을 받아온다.
                    out var model,
                    // 생성 관리자가 내부에서 Actor를 등록하므로 반환 Actor는 보관하지 않는다.
                    out _))
            {
                // 'spawnedPlayerUnit'에 오른쪽 계산, 조회, 또는 상수 결과를 저장한다.
                spawnedPlayerUnit = null;
                // 'SpawnedPlayerModel'에 오른쪽 계산, 조회, 또는 상수 결과를 저장한다.
                SpawnedPlayerModel = null;
                // [방어 로직] 현재 메소드의 남은 처리를 건너뛰고 즉시 호출 지점으로 돌아간다.
                return;
            }

            // 'SpawnedPlayerModel'에 오른쪽 계산, 조회, 또는 상수 결과를 저장한다.
            SpawnedPlayerModel = model;
        }

        // 세션의 현현 몬스터 목록을 슬롯별로 확인해 누락된 파티원을 부활하거나 다시 생성한다.
        // 'RestoreManifestedPlayersFromSession' 메소드의 입력과 반환 계약을 선언한다.
        private void RestoreManifestedPlayersFromSession()
        {
            // BeforeSceneLoad 단계에서 등록된 현재 카탈로그를 사용한다.
            var catalog = ResolveCatalog();

            // 'var i = 0; i < ActiveSession.ManifestedMonsterIds.Count; i++' 규칙으로 인덱스를 갱신하며 코드를 반복한다.
            for (var i = 0; i < ActiveSession.ManifestedMonsterIds.Count; i++)
            {
                // [방어 로직] Mathf 범위 함수로 계산값이 허용 범위를 벗어나지 않게 보정한다.
                var slotIndex = Mathf.Clamp(i + 1, 1, 4);
                // [방어 로직] 'FindPlayerEntryBySlot(slotIndex) != null' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
                if (FindPlayerEntryBySlot(slotIndex) != null)
                {
                    // 'continue' 값을 현재 메소드 호출의 인수로 전달한다.
                    continue;
                }

                // [방어 로직] 'TryReviveExistingPlayerBySlot(slotIndex, out _)' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
                if (TryReviveExistingPlayerBySlot(slotIndex, out _))
                {
                    // 'continue' 값을 현재 메소드 호출의 인수로 전달한다.
                    continue;
                }

                // 지역 변수 'monsterId'에 오른쪽 계산 또는 조회 결과를 저장한다.
                var monsterId = ActiveSession.ManifestedMonsterIds[i];
                // 지역 변수 'monster'에 오른쪽 계산 또는 조회 결과를 저장한다.
                var monster = PakuriDataManager.Instance.ResolveMonster(monsterId, catalog);
                // [방어 로직] 'monster == null' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
                if (monster == null)
                {
                    // [방어 로직] 계속 실행할 수 있지만 잘못된 설정을 경고 로그로 남긴다: $"SceneEntryManager could not resolve manifested monster '{monsterId}' for day-advance restore.".
                    Debug.LogWarning($"SceneEntryManager could not resolve manifested monster '{monsterId}' for day-advance restore.");
                    // 'continue' 값을 현재 메소드 호출의 인수로 전달한다.
                    continue;
                }

                // [낯선 문법] out 인수로 메소드 성공 여부와 함께 추가 결과값을 받아온다.
                unitSpawnManager.SpawnManifestedMonster(monster, ActiveSession, slotIndex, out _);
            }
        }

        // 선택 플레이어 로스터 항목에서 GameObject, 모델, Actor 참조를 저장한다.
        // 'CaptureSelectedPlayerSpawn' 메소드의 입력과 반환 계약을 선언한다.
        private void CaptureSelectedPlayerSpawn(UnitRosterEntry entry)
        {
            // [방어 로직] 'entry == null' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
            if (entry == null)
            {
                // 'spawnedPlayerUnit'에 오른쪽 계산, 조회, 또는 상수 결과를 저장한다.
                spawnedPlayerUnit = null;
                // 'SpawnedPlayerModel'에 오른쪽 계산, 조회, 또는 상수 결과를 저장한다.
                SpawnedPlayerModel = null;
                // [방어 로직] 현재 메소드의 남은 처리를 건너뛰고 즉시 호출 지점으로 돌아간다.
                return;
            }

            // 지역 변수 'actor'에 오른쪽 계산 또는 조회 결과를 저장한다.
            var actor = entry.Actor as MonsterUnitActor;
            // 'spawnedPlayerUnit'에 오른쪽 계산, 조회, 또는 상수 결과를 저장한다.
            spawnedPlayerUnit = actor != null ? actor.gameObject : null;
            // 'SpawnedPlayerModel'에 오른쪽 계산, 조회, 또는 상수 결과를 저장한다.
            SpawnedPlayerModel = entry.Model as MonsterUnitRuntimeModel;
        }

        // 플레이어 몬스터 로스터에서 지정 파티 슬롯의 항목을 찾는다.
        // 'FindPlayerEntryBySlot' 메소드의 입력과 반환 계약을 선언한다.
        private UnitRosterEntry FindPlayerEntryBySlot(int slotIndex)
        {
            // [방어 로직] 'combatManager == null || combatManager.Roster == null' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
            if (combatManager == null || combatManager.Roster == null)
            {
                // [Fallback] 정상 결과를 만들 수 없을 때 기본 결과 'null'을 호출자에게 반환한다.
                return null;
            }

            // 지역 변수 'players'에 오른쪽 계산 또는 조회 결과를 저장한다.
            var players = combatManager.Roster.Players;
            // 'var i = 0; i < players.Count; i++' 규칙으로 인덱스를 갱신하며 코드를 반복한다.
            for (var i = 0; i < players.Count; i++)
            {
                // 지역 변수 'entry'에 오른쪽 계산 또는 조회 결과를 저장한다.
                var entry = players[i];
                // [Fallback][낯선 문법] 삼항 연산자(?:)로 조건에 따라 정상값 또는 대체값을 선택한다.
                var identity = entry != null && entry.Model != null ? entry.Model.Identity : null;
                //  줄로 이어지는 조건식을 시작하고 최종 결과로 실행 분기를 결정한다.
                if (identity != null
                    // 앞 조건과 AND로 'identity.Side == UnitSide.Player' 조건을 추가한다.
                    && identity.Side == UnitSide.Player
                    // 앞 조건과 AND로 'identity.Role == UnitRole.Monster' 조건을 추가한다.
                    && identity.Role == UnitRole.Monster
                    // 앞 조건과 AND로 'identity.SlotIndex == slotIndex)' 조건을 추가한다.
                    && identity.SlotIndex == slotIndex)
                {
                    // 계산 또는 조회 결과 'entry'을 호출자에게 반환한다.
                    return entry;
                }
            }

            // [Fallback] 정상 결과를 만들 수 없을 때 기본 결과 'null'을 호출자에게 반환한다.
            return null;
        }

        // 씬에 남아 있는 슬롯 Actor를 세션 상태와 동기화하고 다음 날짜 상태로 부활시킨다.
        // [방어 로직] 성공 여부를 bool로 돌려주는 Try 패턴. 'TryReviveExistingPlayerBySlot' 메소드의 입력과 반환 계약을 선언한다.
        private bool TryReviveExistingPlayerBySlot(int slotIndex, out UnitRosterEntry revivedEntry)
        {
            // 'revivedEntry'에 오른쪽 계산, 조회, 또는 상수 결과를 저장한다.
            revivedEntry = null;
            // [방어 로직] 'combatManager == null || combatManager.Roster == null' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
            if (combatManager == null || combatManager.Roster == null)
            {
                // [방어 로직] Try 패턴 메소드 'TryReviveExistingPlayerBySlot'가 결과를 만들지 못했음을 false로 알린다.
                return false;
            }

            // 지역 변수 'actor'에 오른쪽 계산 또는 조회 결과를 저장한다.
            var actor = FindExistingPlayerActorBySlot(slotIndex);
            // [Fallback][낯선 문법] 삼항 연산자(?:)로 조건에 따라 정상값 또는 대체값을 선택한다.
            var model = actor != null ? actor.Model : null;
            // [방어 로직] 'model == null' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
            if (model == null)
            {
                // [방어 로직] Try 패턴 메소드 'TryReviveExistingPlayerBySlot'가 결과를 만들지 못했음을 false로 알린다.
                return false;
            }

            // 'SyncExistingMonsterModelFromSession' 메소드를 호출해 현재 단계의 처리를 실행한다.
            SyncExistingMonsterModelFromSession(model);
            // 'MonsterUnitRuntimeStateService.RestoreForNextDay' 메소드를 호출해 해당 객체의 처리를 실행한다.
            MonsterUnitRuntimeStateService.RestoreForNextDay(model);
            // 'actor.ReviveForNextDay' 메소드를 호출해 해당 객체의 처리를 실행한다.
            actor.ReviveForNextDay();
            // 'revivedEntry'에 오른쪽 계산 또는 조회 결과를 저장한다.
            revivedEntry = combatManager.RegisterPlayerMonster(model, actor, actor.transform);
            // 계산 또는 조회 결과 'revivedEntry != null'을 호출자에게 반환한다.
            return revivedEntry != null;
        }

        // 기존 몬스터 모델의 학습 스킬과 선택 정보를 현재 RunSession 상태로 맞춘다.
        // 'SyncExistingMonsterModelFromSession' 메소드의 입력과 반환 계약을 선언한다.
        private void SyncExistingMonsterModelFromSession(MonsterUnitRuntimeModel model)
        {
            // [방어 로직] 'model == null || model.Identity == null || ActiveSession == null' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
            if (model == null || model.Identity == null || ActiveSession == null)
            {
                // [방어 로직] 현재 메소드의 남은 처리를 건너뛰고 즉시 호출 지점으로 돌아간다.
                return;
            }

            // 지역 변수 'state'에 오른쪽 계산 또는 조회 결과를 저장한다.
            var state = ActiveSession.GetPartyMemberState(model.Identity.DefinitionId);
            // [방어 로직] 'state == null' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
            if (state == null)
            {
                // [방어 로직] 현재 메소드의 남은 처리를 건너뛰고 즉시 호출 지점으로 돌아간다.
                return;
            }

            // [방어 로직] 'model.State == null' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
            if (model.State == null)
            {
                // 'model.State'에 오른쪽 계산 또는 조회 결과를 저장한다.
                model.State = new UnitStateBucket();
            }

            // 'CopyListToSet' 메소드를 호출해 현재 단계의 처리를 실행한다.
            CopyListToSet(state.LearnedActives, model.State.LearnedActiveSkillIds);
            // 'CopyListToSet' 메소드를 호출해 현재 단계의 처리를 실행한다.
            CopyListToSet(state.LearnedPassives, model.State.LearnedPassiveSkillIds);
            // 'CopyListToSet' 메소드를 호출해 현재 단계의 처리를 실행한다.
            CopyListToSet(state.ChosenChoiceIds, model.State.ChosenChoiceIds);
            // 'SkillRuntimeFactory.RebuildLearnedActiveSet' 메소드를 호출해 해당 객체의 처리를 실행한다.
            SkillRuntimeFactory.RebuildLearnedActiveSet(model, new InGameSkillCatalog(ResolveCatalog()));
        }

        // 문자열 목록의 유효한 항목을 대상 Set에 중복 없이 다시 채운다.
        // 'CopyListToSet' 메소드의 입력과 반환 계약을 선언한다.
        private static void CopyListToSet(IReadOnlyList<string> source, ISet<string> target)
        {
            // [방어 로직] 'source == null || target == null' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
            if (source == null || target == null)
            {
                // [방어 로직] 현재 메소드의 남은 처리를 건너뛰고 즉시 호출 지점으로 돌아간다.
                return;
            }

            // 컬렉션에 남은 항목을 모두 제거해 상태를 초기화한다.
            target.Clear();
            // 'var i = 0; i < source.Count; i++' 규칙으로 인덱스를 갱신하며 코드를 반복한다.
            for (var i = 0; i < source.Count; i++)
            {
                // [방어 로직] '!string.IsNullOrWhiteSpace(source[i])' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
                if (!string.IsNullOrWhiteSpace(source[i]))
                {
                    // Add 호출 결과 또는 지정 항목을 컬렉션에 추가한다.
                    target.Add(source[i]);
                }
            }
        }

        // BeforeSceneLoad 단계에서 등록된 게임 데이터 카탈로그를 반환한다.
        // 'ResolveCatalog' 메소드의 입력과 반환 계약을 선언한다.
        private static GameDataCatalog ResolveCatalog()
        {
            // BeforeSceneLoad 초기화가 등록한 현재 카탈로그를 호출자에게 반환한다.
            return PakuriDataManager.Instance.CurrentCatalog;
        }

        // 로드된 씬 오브젝트 전체에서 지정 플레이어 슬롯의 기존 MonsterUnitActor를 찾는다.
        // 'FindExistingPlayerActorBySlot' 메소드의 입력과 반환 계약을 선언한다.
        private MonsterUnitActor FindExistingPlayerActorBySlot(int slotIndex)
        {
            // 지역 변수 'actors'에 오른쪽 계산 또는 조회 결과를 저장한다.
            var actors = Resources.FindObjectsOfTypeAll<MonsterUnitActor>();
            // 'var i = 0; i < actors.Length; i++' 규칙으로 인덱스를 갱신하며 코드를 반복한다.
            for (var i = 0; i < actors.Length; i++)
            {
                // 지역 변수 'actor'에 오른쪽 계산 또는 조회 결과를 저장한다.
                var actor = actors[i];
                // [Fallback][낯선 문법] 삼항 연산자(?:)로 조건에 따라 정상값 또는 대체값을 선택한다.
                var identity = actor != null && actor.Model != null ? actor.Model.Identity : null;
                //  줄로 이어지는 조건식을 시작하고 최종 결과로 실행 분기를 결정한다.
                if (identity == null
                    // [방어 로직] 앞 조건과 OR로 'identity.Side != UnitSide.Player' 조건을 추가한다.
                    || identity.Side != UnitSide.Player
                    // [방어 로직] 앞 조건과 OR로 'identity.Role != UnitRole.Monster' 조건을 추가한다.
                    || identity.Role != UnitRole.Monster
                    // [방어 로직] 앞 조건과 OR로 'identity.SlotIndex != slotIndex)' 조건을 추가한다.
                    || identity.SlotIndex != slotIndex)
                {
                    // 'continue' 값을 현재 메소드 호출의 인수로 전달한다.
                    continue;
                }

                // [방어 로직] 'actor.gameObject == null || !actor.gameObject.scene.IsValid()' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
                if (actor.gameObject == null || !actor.gameObject.scene.IsValid())
                {
                    // 'continue' 값을 현재 메소드 호출의 인수로 전달한다.
                    continue;
                }

                // 계산 또는 조회 결과 'actor'을 호출자에게 반환한다.
                return actor;
            }

            // [Fallback] 정상 결과를 만들 수 없을 때 기본 결과 'null'을 호출자에게 반환한다.
            return null;
        }

    }

    // 씬 전환 사이에 선택 몬스터 ID를 임시 보관하는 정적 진입 컨텍스트다.
    // 'StartContext' 클래스 정의를 시작한다.
    public static class StartContext
    {
        // 'SelectedMonsterId' 읽기 전용 property로 계산 결과 또는 상태를 외부에 공개한다.
        public static string SelectedMonsterId { get; private set; }
        // 다음 인게임 씬에서 사용할 선택 몬스터 ID를 저장한다.
        // 'Prepare' 메소드의 입력과 반환 계약을 선언한다.
        public static void Prepare(string selectedMonsterId)
        {
            // 'SelectedMonsterId'에 오른쪽 계산, 조회, 또는 상수 결과를 저장한다.
            SelectedMonsterId = string.IsNullOrWhiteSpace(selectedMonsterId) ? string.Empty : selectedMonsterId;
        }

        // 소비가 끝난 선택 몬스터 ID를 비운다.
        // 'Clear' 메소드의 입력과 반환 계약을 선언한다.
        public static void Clear()
        {
            // 'SelectedMonsterId'에 오른쪽 계산, 조회, 또는 상수 결과를 저장한다.
            SelectedMonsterId = string.Empty;
        }
    }

}
