// 'System' 네임스페이스의 타입과 API를 이 파일에서 사용한다.
using System;
// 'Pakuri.Data' 네임스페이스의 타입과 API를 이 파일에서 사용한다.
using Pakuri.Data;
// 'Pakuri.Run' 네임스페이스의 타입과 API를 이 파일에서 사용한다.
using Pakuri.Run;
// 'UnityEngine' 네임스페이스의 타입과 API를 이 파일에서 사용한다.
using UnityEngine;

// 'Pakuri.InGame' 네임스페이스 범위를 선언해 관련 타입 이름의 충돌을 막는다.
namespace Pakuri.InGame
{
    // 몬스터와 적의 데이터 모델·프리팹·Actor를 만들고 전투 로스터에 등록한다.
    // [낯선 문법] DisallowMultipleComponent attribute: 같은 GameObject에 이 컴포넌트가 중복 부착되는 것을 막는다.
    [DisallowMultipleComponent]
    // [방어 로직][낯선 문법] RequireComponent attribute: 'InGameCombatManager' 의존 컴포넌트가 같은 GameObject에 있도록 Unity에 요구한다.
    [RequireComponent(typeof(InGameCombatManager))]
    // 'EnemySpawnManger' 클래스 정의를 시작한다.
    public class EnemySpawnManger : MonoBehaviour
    {
        // 'ArielMonsterId' 상수에 실행 중 바뀌지 않는 기준값을 선언한다.
        private const string ArielMonsterId = "ariel";
        // 'EveMonsterId' 상수에 실행 중 바뀌지 않는 기준값을 선언한다.
        private const string EveMonsterId = "eve";
        // 'RinMonsterId' 상수에 실행 중 바뀌지 않는 기준값을 선언한다.
        private const string RinMonsterId = "rin";
        // 'SeinMonsterId' 상수에 실행 중 바뀌지 않는 기준값을 선언한다.
        private const string SeinMonsterId = "sein";
        // 'VegaMonsterId' 상수에 실행 중 바뀌지 않는 기준값을 선언한다.
        private const string VegaMonsterId = "vega";
        // [낯선 문법] readonly 필드 'unitFactory'를 초기화하며, 생성 뒤에는 이 참조를 다시 대입할 수 없다.
        private readonly UnitFactory unitFactory = new UnitFactory();

        // [낯선 문법] SerializeField attribute: private 상태 'combatManager'을 Unity 직렬화와 Inspector 편집 대상으로 만든다.
        [SerializeField] private InGameCombatManager combatManager;
        // [낯선 문법] SerializeField attribute: private 상태 'playerSpawnPoint'을 Unity 직렬화와 Inspector 편집 대상으로 만든다.
        [SerializeField] private Transform playerSpawnPoint;
        // [낯선 문법] SerializeField attribute: private 상태 'enemySpawnPoint'을 Unity 직렬화와 Inspector 편집 대상으로 만든다.
        [SerializeField] private Transform enemySpawnPoint;
        // [낯선 문법] SerializeField attribute: private 상태 'runtimeEnemyRoot'을 Unity 직렬화와 Inspector 편집 대상으로 만든다.
        [SerializeField] private Transform runtimeEnemyRoot;
        // [낯선 문법] SerializeField attribute: private 상태 'runtimeMonsterRoot'을 Unity 직렬화와 Inspector 편집 대상으로 만든다.
        [SerializeField] private Transform runtimeMonsterRoot;
        // [낯선 문법] SerializeField attribute: private 상태 'arielUnitPrefab'을 Unity 직렬화와 Inspector 편집 대상으로 만든다.
        [SerializeField] private GameObject arielUnitPrefab;
        // [낯선 문법] SerializeField attribute: private 상태 'eveUnitPrefab'을 Unity 직렬화와 Inspector 편집 대상으로 만든다.
        [SerializeField] private GameObject eveUnitPrefab;
        // [낯선 문법] SerializeField attribute: private 상태 'rinUnitPrefab'을 Unity 직렬화와 Inspector 편집 대상으로 만든다.
        [SerializeField] private GameObject rinUnitPrefab;
        // [낯선 문법] SerializeField attribute: private 상태 'seinUnitPrefab'을 Unity 직렬화와 Inspector 편집 대상으로 만든다.
        [SerializeField] private GameObject seinUnitPrefab;
        // [낯선 문법] SerializeField attribute: private 상태 'vegaUnitPrefab'을 Unity 직렬화와 Inspector 편집 대상으로 만든다.
        [SerializeField] private GameObject vegaUnitPrefab;
        // [낯선 문법] SerializeField attribute: private 상태 'enemyPrefabBindings'을 Unity 직렬화와 Inspector 편집 대상으로 만든다.
        [SerializeField] private EnemyPrefabBinding[] enemyPrefabBindings = Array.Empty<EnemyPrefabBinding>();
        // 적 ID에 연결된 프리팹의 루트 SpriteRenderer에서 초상화용 Sprite를 반환한다.
        // 'ResolveEnemyPortraitSprite' 메소드의 입력과 반환 계약을 선언한다.
        public Sprite ResolveEnemyPortraitSprite(string enemyId)
        {
            // 지역 변수 'prefab'에 오른쪽 계산 또는 조회 결과를 저장한다.
            var prefab = ResolveEnemyPrefab(enemyId);
            // [Fallback][낯선 문법] 삼항 연산자(?:)로 조건에 따라 정상값 또는 대체값을 선택한다.
            var spriteRenderer = prefab != null ? prefab.GetComponent<SpriteRenderer>() : null;
            // [Fallback][낯선 문법] 삼항 연산자(?:)로 조건 결과에 맞는 값 하나를 반환한다.
            return spriteRenderer != null ? spriteRenderer.sprite : null;
        }

        // 적 ID와 생성에 사용할 프리팹의 직렬화 연결 정보를 저장한다.
        // [낯선 문법] Serializable attribute: 이 타입의 필드 값을 Unity 직렬화 대상으로 만든다.
        [Serializable]
        // 'EnemyPrefabBinding' 클래스 정의를 시작한다.
        private class EnemyPrefabBinding
        {
            // [낯선 문법] SerializeField attribute: private 상태 'enemyId'을 Unity 직렬화와 Inspector 편집 대상으로 만든다.
            [SerializeField] private string enemyId = string.Empty;
            // [낯선 문법] SerializeField attribute: private 상태 'prefab'을 Unity 직렬화와 Inspector 편집 대상으로 만든다.
            [SerializeField] private GameObject prefab = null;

            // [낯선 문법] 식 본문 property: 'EnemyId' 값을 오른쪽 식 하나로 계산해 반환한다.
            public string EnemyId => enemyId;
            // [낯선 문법] 식 본문 property: 'Prefab' 값을 오른쪽 식 하나로 계산해 반환한다.
            public GameObject Prefab => prefab;
        }

        // 선택 몬스터 데이터로 새 세션과 1P 모델·Actor를 만들고 플레이어로 등록한다.
        // 'SpawnSelectedPlayerUnit' 메소드의 입력과 반환 계약을 선언한다.
        public bool SpawnSelectedPlayerUnit(
            // 'selectedMonsterId' 매개변수 또는 지역값의 타입을 'string'로 지정한다.
            string selectedMonsterId,
            // [낯선 문법] out 인수로 메소드 성공 여부와 함께 추가 결과값을 받아온다.
            out GameObject spawnedUnit,
            // [낯선 문법] out 인수로 메소드 성공 여부와 함께 추가 결과값을 받아온다.
            out MonsterUnitRuntimeModel model,
            // [낯선 문법] out 인수로 메소드 성공 여부와 함께 추가 결과값을 받아온다.
            out MonsterUnitActor actor,
            // [낯선 문법] out 인수로 메소드 성공 여부와 함께 추가 결과값을 받아온다.
            out RunSession session)
        {
            // 'spawnedUnit'에 오른쪽 계산, 조회, 또는 상수 결과를 저장한다.
            spawnedUnit = null;
            // 'model'에 오른쪽 계산, 조회, 또는 상수 결과를 저장한다.
            model = null;
            // 'actor'에 오른쪽 계산, 조회, 또는 상수 결과를 저장한다.
            actor = null;
            // 'session'에 오른쪽 계산, 조회, 또는 상수 결과를 저장한다.
            session = null;

            // [방어 로직] 'string.IsNullOrWhiteSpace(selectedMonsterId)' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
            if (string.IsNullOrWhiteSpace(selectedMonsterId))
            {
                // [방어 로직] 계속 실행할 수 있지만 잘못된 설정을 경고 로그로 남긴다: "EnemySpawnManger cannot spawn a selected monster because monster data is missing.".
                Debug.LogWarning("EnemySpawnManger cannot spawn a selected monster because monster data is missing.");
                // [방어 로직] 필수 대상 또는 유효 조건이 없으므로 실패 결과 false를 반환한다.
                return false;
            }

            // 지역 변수 'prefab'에 오른쪽 계산 또는 조회 결과를 저장한다.
            var prefab = ResolveMonsterPrefab(selectedMonsterId);
            // [방어 로직] 'prefab == null' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
            if (prefab == null)
            {
                // [방어 로직] 계속 실행할 수 있지만 잘못된 설정을 경고 로그로 남긴다: $"No NewRunScene prefab is configured for selected monster '{selectedMonsterId}'.".
                Debug.LogWarning($"No NewRunScene prefab is configured for selected monster '{selectedMonsterId}'.");
                // [방어 로직] 필수 대상 또는 유효 조건이 없으므로 실패 결과 false를 반환한다.
                return false;
            }

            // [방어 로직] '!TryCreateSelectedModel(selectedMonsterId, out model, out session)' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
            if (!TryCreateSelectedModel(selectedMonsterId, out model, out session))
            {
                // 조건 판단의 부정 결과를 false로 반환한다.
                return false;
            }

            // 직렬화된 1P 생성 지점의 위치를 사용한다.
            var spawnPosition = playerSpawnPoint.position;
            // 직렬화된 1P 생성 지점의 회전을 사용한다.
            var spawnRotation = playerSpawnPoint.rotation;
            // 'spawnedUnit'에 오른쪽 계산 또는 조회 결과를 저장한다.
            spawnedUnit = Instantiate(prefab, spawnPosition, spawnRotation, runtimeMonsterRoot);
            // 'spawnedUnit.name'에 오른쪽 계산 또는 조회 결과를 저장한다.
            spawnedUnit.name = $"{prefab.name}_1P";
            // 'actor'에 오른쪽 계산 또는 조회 결과를 저장한다.
            actor = BindMonsterActor(spawnedUnit, model);
            // 생성된 유닛 Transform을 플레이어 피격 기준으로 등록한다.
            RegisterPlayer(model, actor, spawnedUnit.transform);
            // 요청한 검사 또는 처리가 성공했음을 true로 반환한다.
            return true;
        }

        // 세션의 현현 몬스터 상태로 2P~5P 모델과 Actor를 만들고 플레이어로 등록한다.
        // 'SpawnManifestedMonster' 메소드의 입력과 반환 계약을 선언한다.
        public bool SpawnManifestedMonster(
            // 'monster' 매개변수 또는 지역값의 타입을 'MonsterDefinition'로 지정한다.
            MonsterDefinition monster,
            // 'activeSession' 매개변수 또는 지역값의 타입을 'RunSession'로 지정한다.
            RunSession activeSession,
            // 'partySlotIndex' 매개변수 또는 지역값의 타입을 'int'로 지정한다.
            int partySlotIndex,
            // [낯선 문법] out 인수로 메소드 성공 여부와 함께 추가 결과값을 받아온다.
            out GameObject spawnedUnit)
        {
            // 'spawnedUnit'에 오른쪽 계산, 조회, 또는 상수 결과를 저장한다.
            spawnedUnit = null;
            // [방어 로직] 'monster == null || string.IsNullOrWhiteSpace(monster.MonsterId)' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
            if (monster == null || string.IsNullOrWhiteSpace(monster.MonsterId))
            {
                // [방어 로직] 계속 실행할 수 있지만 잘못된 설정을 경고 로그로 남긴다: "EnemySpawnManger cannot manifest a monster because monster data is missing.".
                Debug.LogWarning("EnemySpawnManger cannot manifest a monster because monster data is missing.");
                // [방어 로직] 필수 대상 또는 유효 조건이 없으므로 실패 결과 false를 반환한다.
                return false;
            }

            // [방어 로직] 'activeSession == null' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
            if (activeSession == null)
            {
                // [방어 로직] 계속 실행할 수 있지만 잘못된 설정을 경고 로그로 남긴다: "EnemySpawnManger cannot manifest a monster because no active session exists.".
                Debug.LogWarning("EnemySpawnManger cannot manifest a monster because no active session exists.");
                // [방어 로직] 필수 대상 또는 유효 조건이 없으므로 실패 결과 false를 반환한다.
                return false;
            }

            // 지역 변수 'prefab'에 오른쪽 계산 또는 조회 결과를 저장한다.
            var prefab = ResolveMonsterPrefab(monster.MonsterId);
            // [방어 로직] 'prefab == null' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
            if (prefab == null)
            {
                // [방어 로직] 계속 실행할 수 있지만 잘못된 설정을 경고 로그로 남긴다: $"No NewRunScene prefab is configured for manifested monster '{monster.MonsterId}'.".
                Debug.LogWarning($"No NewRunScene prefab is configured for manifested monster '{monster.MonsterId}'.");
                // [방어 로직] 필수 대상 또는 유효 조건이 없으므로 실패 결과 false를 반환한다.
                return false;
            }

            // [방어 로직] Mathf 범위 함수로 계산값이 허용 범위를 벗어나지 않게 보정한다.
            var clampedSlotIndex = Mathf.Clamp(partySlotIndex, 1, 4);
            // 지역 변수 'runState'에 오른쪽 계산 또는 조회 결과를 저장한다.
            var runState = activeSession.EnsurePartyMemberState(monster);
            // 지역 변수 'model'에 오른쪽 계산 또는 조회 결과를 저장한다.
            var model = unitFactory.CreateManifestedMonster(monster, runState, clampedSlotIndex);
            // [방어 로직] 'model == null' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
            if (model == null)
            {
                // 실행을 막는 오류 상태를 로그로 남긴다: $"EnemySpawnManger could not create manifested monster runtime model for '{monster.MonsterId}'.".
                Debug.LogError($"EnemySpawnManger could not create manifested monster runtime model for '{monster.MonsterId}'.");
                // [방어 로직] 필수 대상 또는 유효 조건이 없으므로 실패 결과 false를 반환한다.
                return false;
            }

            // 'SkillRuntimeFactory.RebuildLearnedActiveSet' 메소드를 호출해 해당 객체의 처리를 실행한다.
            SkillRuntimeFactory.RebuildLearnedActiveSet(model, new InGameSkillCatalog(ResolveCatalog()));

            // 지역 변수 'spawnPoint'에 오른쪽 계산 또는 조회 결과를 저장한다.
            var spawnPoint = ResolveManifestSpawnPoint(clampedSlotIndex);
            // 씬의 파티 슬롯 생성 지점 위치를 사용한다.
            var spawnPosition = spawnPoint.position;
            // 씬의 파티 슬롯 생성 지점 회전을 사용한다.
            var spawnRotation = spawnPoint.rotation;
            // 'spawnedUnit'에 오른쪽 계산 또는 조회 결과를 저장한다.
            spawnedUnit = Instantiate(prefab, spawnPosition, spawnRotation, runtimeMonsterRoot);
            // 'spawnedUnit.name'에 오른쪽 계산 또는 조회 결과를 저장한다.
            spawnedUnit.name = $"{prefab.name}_{clampedSlotIndex + 1}P";

            // 지역 변수 'actor'에 오른쪽 계산 또는 조회 결과를 저장한다.
            var actor = BindMonsterActor(spawnedUnit, model);
            // 생성된 유닛 Transform을 플레이어 피격 기준으로 등록한다.
            RegisterPlayer(model, actor, spawnedUnit.transform);
            // 요청한 검사 또는 처리가 성공했음을 true로 반환한다.
            return true;
        }

        // 기존 RunSession 상태를 사용해 선택 몬스터의 1P 모델과 Actor를 다시 생성한다.
        // 'RespawnSelectedPlayerUnit' 메소드의 입력과 반환 계약을 선언한다.
        public bool RespawnSelectedPlayerUnit(
            // 'activeSession' 매개변수 또는 지역값의 타입을 'RunSession'로 지정한다.
            RunSession activeSession,
            // [낯선 문법] out 인수로 메소드 성공 여부와 함께 추가 결과값을 받아온다.
            out GameObject spawnedUnit,
            // [낯선 문법] out 인수로 메소드 성공 여부와 함께 추가 결과값을 받아온다.
            out MonsterUnitRuntimeModel model,
            // [낯선 문법] out 인수로 메소드 성공 여부와 함께 추가 결과값을 받아온다.
            out MonsterUnitActor actor)
        {
            // 'spawnedUnit'에 오른쪽 계산, 조회, 또는 상수 결과를 저장한다.
            spawnedUnit = null;
            // 'model'에 오른쪽 계산, 조회, 또는 상수 결과를 저장한다.
            model = null;
            // 'actor'에 오른쪽 계산, 조회, 또는 상수 결과를 저장한다.
            actor = null;
            // [방어 로직] 'activeSession == null || string.IsNullOrWhiteSpace(activeSession.SelectedMonsterId)' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
            if (activeSession == null || string.IsNullOrWhiteSpace(activeSession.SelectedMonsterId))
            {
                // [방어 로직] 계속 실행할 수 있지만 잘못된 설정을 경고 로그로 남긴다: "EnemySpawnManger cannot respawn the selected monster because no active session exists.".
                Debug.LogWarning("EnemySpawnManger cannot respawn the selected monster because no active session exists.");
                // [방어 로직] 필수 대상 또는 유효 조건이 없으므로 실패 결과 false를 반환한다.
                return false;
            }

            // 지역 변수 'catalog'에 오른쪽 계산 또는 조회 결과를 저장한다.
            var catalog = ResolveCatalog();
            // 지역 변수 'monster'에 오른쪽 계산 또는 조회 결과를 저장한다.
            var monster = ResolveMonsterDefinition(activeSession.SelectedMonsterId);
            // [방어 로직] 'monster == null' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
            if (monster == null)
            {
                // 실행을 막는 오류 상태를 로그로 남긴다: $"EnemySpawnManger could not resolve selected monster data for '{activeSession.SelectedMonsterId}' during respawn.".
                Debug.LogError($"EnemySpawnManger could not resolve selected monster data for '{activeSession.SelectedMonsterId}' during respawn.");
                // [방어 로직] 필수 대상 또는 유효 조건이 없으므로 실패 결과 false를 반환한다.
                return false;
            }

            // 지역 변수 'prefab'에 오른쪽 계산 또는 조회 결과를 저장한다.
            var prefab = ResolveMonsterPrefab(monster.MonsterId);
            // [방어 로직] 'prefab == null' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
            if (prefab == null)
            {
                // [방어 로직] 계속 실행할 수 있지만 잘못된 설정을 경고 로그로 남긴다: $"No NewRunScene prefab is configured for selected monster '{monster.MonsterId}' during respawn.".
                Debug.LogWarning($"No NewRunScene prefab is configured for selected monster '{monster.MonsterId}' during respawn.");
                // [방어 로직] 필수 대상 또는 유효 조건이 없으므로 실패 결과 false를 반환한다.
                return false;
            }

            // [Fallback][낯선 문법] null 병합 연산자(??): 왼쪽 값이 null이면 오른쪽 대체값을 사용한다.
            var runState = activeSession.GetPartyMemberState(monster.MonsterId) ?? activeSession.EnsurePartyMemberState(monster);
            // 'model'에 오른쪽 계산 또는 조회 결과를 저장한다.
            model = unitFactory.CreateSelectedMonster(monster, runState, 0);
            // [방어 로직] 'model == null' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
            if (model == null)
            {
                // 실행을 막는 오류 상태를 로그로 남긴다: $"EnemySpawnManger could not recreate a runtime unit model for '{monster.MonsterId}' during respawn.".
                Debug.LogError($"EnemySpawnManger could not recreate a runtime unit model for '{monster.MonsterId}' during respawn.");
                // [방어 로직] 필수 대상 또는 유효 조건이 없으므로 실패 결과 false를 반환한다.
                return false;
            }

            // 'SkillRuntimeFactory.RebuildLearnedActiveSet' 메소드를 호출해 해당 객체의 처리를 실행한다.
            SkillRuntimeFactory.RebuildLearnedActiveSet(model, new InGameSkillCatalog(catalog));

            // 직렬화된 1P 생성 지점의 위치를 사용한다.
            var spawnPosition = playerSpawnPoint.position;
            // 직렬화된 1P 생성 지점의 회전을 사용한다.
            var spawnRotation = playerSpawnPoint.rotation;
            // 'spawnedUnit'에 오른쪽 계산 또는 조회 결과를 저장한다.
            spawnedUnit = Instantiate(prefab, spawnPosition, spawnRotation, runtimeMonsterRoot);
            // 'spawnedUnit.name'에 오른쪽 계산 또는 조회 결과를 저장한다.
            spawnedUnit.name = $"{prefab.name}_1P";
            // 'actor'에 오른쪽 계산 또는 조회 결과를 저장한다.
            actor = BindMonsterActor(spawnedUnit, model);
            // 생성된 유닛 Transform을 플레이어 피격 기준으로 등록한다.
            RegisterPlayer(model, actor, spawnedUnit.transform);
            // 요청한 검사 또는 처리가 성공했음을 true로 반환한다.
            return true;
        }

        // 적 ID에 대응하는 프리팹을 찾아 전체 적 생성 절차를 실행한다.
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
            // 지역 변수 'prefab'에 오른쪽 계산 또는 조회 결과를 저장한다.
            var prefab = ResolveEnemyPrefab(enemyId);
            // 계산 또는 조회 결과 'TrySpawnEnemyUnit(prefab, enemyId, spawnIndex, spawnX, spawnYMin, spawnYMax, healthMultiplier, isBoss, out spawnedUnit)'을 호출자에게 반환한다.
            return TrySpawnEnemyUnit(prefab, enemyId, spawnIndex, spawnX, spawnYMin, spawnYMax, healthMultiplier, isBoss, out spawnedUnit);
        }

        // 몬스터 데이터를 해석해 새 RunSession과 선택 몬스터 런타임 모델을 만든다.
        // [방어 로직] 성공 여부를 bool로 돌려주는 Try 패턴. 'TryCreateSelectedModel' 메소드의 입력과 반환 계약을 선언한다.
        private bool TryCreateSelectedModel(
            // 'monsterId' 매개변수 또는 지역값의 타입을 'string'로 지정한다.
            string monsterId,
            // [낯선 문법] out 인수로 메소드 성공 여부와 함께 추가 결과값을 받아온다.
            out MonsterUnitRuntimeModel model,
            // [낯선 문법] out 인수로 메소드 성공 여부와 함께 추가 결과값을 받아온다.
            out RunSession session)
        {
            // 'model'에 오른쪽 계산, 조회, 또는 상수 결과를 저장한다.
            model = null;
            // 'session'에 오른쪽 계산, 조회, 또는 상수 결과를 저장한다.
            session = null;

            // 지역 변수 'catalog'에 오른쪽 계산 또는 조회 결과를 저장한다.
            var catalog = ResolveCatalog();
            // [방어 로직] 'catalog == null' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
            if (catalog == null)
            {
                // 실행을 막는 오류 상태를 로그로 남긴다: "EnemySpawnManger could not resolve a game data catalog for the selected monster.".
                Debug.LogError("EnemySpawnManger could not resolve a game data catalog for the selected monster.");
                // [방어 로직] Try 패턴 메소드 'TryCreateSelectedModel'가 결과를 만들지 못했음을 false로 알린다.
                return false;
            }

            // 지역 변수 'monster'에 오른쪽 계산 또는 조회 결과를 저장한다.
            var monster = ResolveMonsterDefinition(monsterId);
            // [방어 로직] 'monster == null' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
            if (monster == null)
            {
                // 실행을 막는 오류 상태를 로그로 남긴다: $"EnemySpawnManger could not resolve selected monster data for '{monsterId}'.".
                Debug.LogError($"EnemySpawnManger could not resolve selected monster data for '{monsterId}'.");
                // [방어 로직] Try 패턴 메소드 'TryCreateSelectedModel'가 결과를 만들지 못했음을 false로 알린다.
                return false;
            }

            // 'session'에 오른쪽 계산, 조회, 또는 상수 결과를 저장한다.
            session = RunSession.Begin(monster);
            // 'model'에 오른쪽 계산 또는 조회 결과를 저장한다.
            model = unitFactory.CreateSelectedMonster(monster, session.GetPartyMemberState(monster.MonsterId), 0);
            // [방어 로직] 'model == null' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
            if (model == null)
            {
                // 실행을 막는 오류 상태를 로그로 남긴다: $"EnemySpawnManger could not create a runtime unit model for '{monsterId}'.".
                Debug.LogError($"EnemySpawnManger could not create a runtime unit model for '{monsterId}'.");
                // [방어 로직] Try 패턴 메소드 'TryCreateSelectedModel'가 결과를 만들지 못했음을 false로 알린다.
                return false;
            }

            // 'SkillRuntimeFactory.RebuildLearnedActiveSet' 메소드를 호출해 해당 객체의 처리를 실행한다.
            SkillRuntimeFactory.RebuildLearnedActiveSet(model, new InGameSkillCatalog(catalog));
            // 요청한 검사 또는 처리가 성공했음을 true로 반환한다.
            return true;
        }

        // 적 모델을 만들고 무작위 Y 위치에 프리팹을 생성한 뒤 Actor와 로스터를 연결한다.
        // [방어 로직] 성공 여부를 bool로 돌려주는 Try 패턴. 'TrySpawnEnemyUnit' 메소드의 입력과 반환 계약을 선언한다.
        private bool TrySpawnEnemyUnit(
            // 'prefab' 매개변수 또는 지역값의 타입을 'GameObject'로 지정한다.
            GameObject prefab,
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
            // [방어 로직] 'prefab == null' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
            if (prefab == null)
            {
                // [방어 로직] 계속 실행할 수 있지만 잘못된 설정을 경고 로그로 남긴다: $"No NewRunScene enemy prefab is configured for enemy '{enemyId}'.".
                Debug.LogWarning($"No NewRunScene enemy prefab is configured for enemy '{enemyId}'.");
                // [방어 로직] Try 패턴 메소드 'TrySpawnEnemyUnit'가 결과를 만들지 못했음을 false로 알린다.
                return false;
            }

            // [방어 로직] '!TryCreateEnemyModel(enemyId, spawnIndex, isBoss, out var model)' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
            if (!TryCreateEnemyModel(enemyId, spawnIndex, isBoss, out var model))
            {
                // [방어 로직] Try 패턴 메소드 'TrySpawnEnemyUnit'가 결과를 만들지 못했음을 false로 알린다.
                return false;
            }

            // 'ApplyEnemyHealthMultiplier' 메소드를 호출해 현재 단계의 처리를 실행한다.
            ApplyEnemyHealthMultiplier(model, healthMultiplier);
            // 지역 변수 'spawnPosition'에 저장할 여러 줄 계산 또는 선택식을 시작한다.
            var spawnPosition = new Vector3(
                // 'spawnX' 열거값을 선택 가능한 상수 항목으로 정의한다.
                spawnX,
                // 'UnityEngine.Random.Range' 메소드를 호출해 해당 객체의 처리를 실행한다.
                UnityEngine.Random.Range(spawnYMin, spawnYMax),
                // 직렬화된 적 생성 지점의 Z 좌표를 사용한다.
                enemySpawnPoint.position.z);
            // 직렬화된 적 생성 지점의 회전을 사용한다.
            var spawnRotation = enemySpawnPoint.rotation;
            // 'spawnedUnit'에 오른쪽 계산 또는 조회 결과를 저장한다.
            spawnedUnit = Instantiate(prefab, spawnPosition, spawnRotation, runtimeEnemyRoot);
            // 'spawnedUnit.name'에 오른쪽 계산 또는 조회 결과를 저장한다.
            spawnedUnit.name = $"{prefab.name}_Enemy_{spawnIndex}";

            // 지역 변수 'actor'에 오른쪽 계산 또는 조회 결과를 저장한다.
            var actor = BindEnemyActor(spawnedUnit, model);
            // 생성된 유닛 Transform을 적 피격 기준으로 등록한다.
            RegisterEnemy(model, actor, spawnedUnit.transform);
            // 요청한 검사 또는 처리가 성공했음을 true로 반환한다.
            return true;
        }

        // 적 정의와 슬롯·보스 여부로 런타임 모델을 만들고 배정 스킬을 구성한다.
        // [방어 로직] 성공 여부를 bool로 돌려주는 Try 패턴. 'TryCreateEnemyModel' 메소드의 입력과 반환 계약을 선언한다.
        private bool TryCreateEnemyModel(string enemyId, int slotIndex, bool isBoss, out EnemyUnitRuntimeModel model)
        {
            // 'model'에 오른쪽 계산, 조회, 또는 상수 결과를 저장한다.
            model = null;

            // 지역 변수 'catalog'에 오른쪽 계산 또는 조회 결과를 저장한다.
            var catalog = ResolveCatalog();
            // [방어 로직] 'catalog == null' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
            if (catalog == null)
            {
                // 실행을 막는 오류 상태를 로그로 남긴다: "EnemySpawnManger could not resolve a game data catalog for the enemy.".
                Debug.LogError("EnemySpawnManger could not resolve a game data catalog for the enemy.");
                // [방어 로직] Try 패턴 메소드 'TryCreateEnemyModel'가 결과를 만들지 못했음을 false로 알린다.
                return false;
            }

            // 지역 변수 'enemy'에 오른쪽 계산 또는 조회 결과를 저장한다.
            var enemy = ResolveEnemyDefinition(enemyId);
            // [방어 로직] 'enemy == null' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
            if (enemy == null)
            {
                // 실행을 막는 오류 상태를 로그로 남긴다: $"EnemySpawnManger could not resolve enemy data for '{enemyId}'.".
                Debug.LogError($"EnemySpawnManger could not resolve enemy data for '{enemyId}'.");
                // [방어 로직] Try 패턴 메소드 'TryCreateEnemyModel'가 결과를 만들지 못했음을 false로 알린다.
                return false;
            }

            // 'model'에 오른쪽 계산 또는 조회 결과를 저장한다.
            model = unitFactory.CreateEnemy(enemy, slotIndex, isBoss);
            // [방어 로직] 'model == null' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
            if (model == null)
            {
                // 실행을 막는 오류 상태를 로그로 남긴다: $"EnemySpawnManger could not create an enemy runtime unit model for '{enemyId}'.".
                Debug.LogError($"EnemySpawnManger could not create an enemy runtime unit model for '{enemyId}'.");
                // [방어 로직] Try 패턴 메소드 'TryCreateEnemyModel'가 결과를 만들지 못했음을 false로 알린다.
                return false;
            }

            // 'SkillRuntimeFactory.RebuildAssignedActiveSet' 메소드를 호출해 해당 객체의 처리를 실행한다.
            SkillRuntimeFactory.RebuildAssignedActiveSet(model, enemy.ActiveSkills, enemy.SkillTriggers);

            // 요청한 검사 또는 처리가 성공했음을 true로 반환한다.
            return true;
        }

        // BeforeSceneLoad 단계에서 등록된 게임 데이터 카탈로그를 반환한다.
        // 'ResolveCatalog' 메소드의 입력과 반환 계약을 선언한다.
        private GameDataCatalog ResolveCatalog()
        {
            // BeforeSceneLoad 초기화가 등록한 현재 카탈로그를 호출자에게 반환한다.
            return PakuriDataManager.Instance.CurrentCatalog;
        }

        // 데이터 관리자에서 몬스터 ID에 대응하는 정의를 찾는다.
        // 'ResolveMonsterDefinition' 메소드의 입력과 반환 계약을 선언한다.
        private MonsterDefinition ResolveMonsterDefinition(string monsterId)
        {
            // [방어 로직] 'string.IsNullOrWhiteSpace(monsterId)' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
            if (string.IsNullOrWhiteSpace(monsterId))
            {
                // [Fallback] 정상 결과를 만들 수 없을 때 기본 결과 'null'을 호출자에게 반환한다.
                return null;
            }

            // 등록된 몬스터 정의 조회 결과를 호출자에게 반환한다.
            return PakuriDataManager.Instance.GetData<MonsterDefinition>(monsterId);
        }

        // 데이터 관리자에서 적 ID에 대응하는 정의를 찾는다.
        // 'ResolveEnemyDefinition' 메소드의 입력과 반환 계약을 선언한다.
        private EnemyDefinition ResolveEnemyDefinition(string enemyId)
        {
            // [방어 로직] 'string.IsNullOrWhiteSpace(enemyId)' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
            if (string.IsNullOrWhiteSpace(enemyId))
            {
                // [Fallback] 정상 결과를 만들 수 없을 때 기본 결과 'null'을 호출자에게 반환한다.
                return null;
            }

            // 등록된 적 정의 조회 결과를 호출자에게 반환한다.
            return PakuriDataManager.Instance.GetData<EnemyDefinition>(enemyId);
        }

        // 생성된 몬스터 프리팹에서 Actor를 찾아 런타임 모델로 초기화한다.
        // 'BindMonsterActor' 메소드의 입력과 반환 계약을 선언한다.
        private MonsterUnitActor BindMonsterActor(GameObject spawnedUnit, MonsterUnitRuntimeModel model)
        {
            // 생성된 몬스터 프리팹에서 필수 MonsterUnitActor를 가져온다.
            var actor = spawnedUnit.GetComponentInChildren<MonsterUnitActor>(true);

            // 'actor.Initialize' 메소드를 호출해 해당 객체의 처리를 실행한다.
            actor.Initialize(model);
            // 계산 또는 조회 결과 'actor'을 호출자에게 반환한다.
            return actor;
        }

        // 생성된 적 프리팹에서 Actor를 찾아 런타임 모델로 초기화한다.
        // 'BindEnemyActor' 메소드의 입력과 반환 계약을 선언한다.
        private EnemyUnitActor BindEnemyActor(GameObject spawnedUnit, EnemyUnitRuntimeModel model)
        {
            // 생성된 적 프리팹에서 필수 EnemyUnitActor를 가져온다.
            var actor = spawnedUnit.GetComponentInChildren<EnemyUnitActor>(true);

            // 'actor.Initialize' 메소드를 호출해 해당 객체의 처리를 실행한다.
            actor.Initialize(model);
            // 계산 또는 조회 결과 'actor'을 호출자에게 반환한다.
            return actor;
        }

        // 플레이어 몬스터 모델과 Actor를 전투 관리자 로스터에 등록한다.
        // 'RegisterPlayer' 메소드의 입력과 반환 계약을 선언한다.
        private void RegisterPlayer(MonsterUnitRuntimeModel model, MonsterUnitActor actor, Transform hitboxRoot)
        {
            // [방어 로직] 'combatManager != null && model != null' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
            if (combatManager != null && model != null)
            {
                // 'combatManager.RegisterPlayerMonster' 메소드를 호출해 해당 객체의 처리를 실행한다.
                combatManager.RegisterPlayerMonster(model, actor, hitboxRoot);
            }
        }

        // 적 모델과 Actor를 전투 관리자 로스터에 등록한다.
        // 'RegisterEnemy' 메소드의 입력과 반환 계약을 선언한다.
        private void RegisterEnemy(EnemyUnitRuntimeModel model, EnemyUnitActor actor, Transform hitboxRoot)
        {
            // [방어 로직] 'combatManager != null && model != null' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
            if (combatManager != null && model != null)
            {
                // 'combatManager.RegisterEnemy' 메소드를 호출해 해당 객체의 처리를 실행한다.
                combatManager.RegisterEnemy(model, actor, hitboxRoot);
            }
        }

        // 지원하는 몬스터 ID를 직렬화된 몬스터 유닛 프리팹으로 변환한다.
        // 'ResolveMonsterPrefab' 메소드의 입력과 반환 계약을 선언한다.
        private GameObject ResolveMonsterPrefab(string monsterId)
        {
            // 'string.Equals(monsterId, ArielMonsterId, StringComparison.OrdinalIgnoreCase)' 조건이 참인지 검사해 실행 분기를 결정한다.
            if (string.Equals(monsterId, ArielMonsterId, StringComparison.OrdinalIgnoreCase))
            {
                // 계산 또는 조회 결과 'arielUnitPrefab'을 호출자에게 반환한다.
                return arielUnitPrefab;
            }

            // 'string.Equals(monsterId, EveMonsterId, StringComparison.OrdinalIgnoreCase)' 조건이 참인지 검사해 실행 분기를 결정한다.
            if (string.Equals(monsterId, EveMonsterId, StringComparison.OrdinalIgnoreCase))
            {
                // 계산 또는 조회 결과 'eveUnitPrefab'을 호출자에게 반환한다.
                return eveUnitPrefab;
            }

            // 'string.Equals(monsterId, RinMonsterId, StringComparison.OrdinalIgnoreCase)' 조건이 참인지 검사해 실행 분기를 결정한다.
            if (string.Equals(monsterId, RinMonsterId, StringComparison.OrdinalIgnoreCase))
            {
                // 계산 또는 조회 결과 'rinUnitPrefab'을 호출자에게 반환한다.
                return rinUnitPrefab;
            }

            // 'string.Equals(monsterId, SeinMonsterId, StringComparison.OrdinalIgnoreCase)' 조건이 참인지 검사해 실행 분기를 결정한다.
            if (string.Equals(monsterId, SeinMonsterId, StringComparison.OrdinalIgnoreCase))
            {
                // 계산 또는 조회 결과 'seinUnitPrefab'을 호출자에게 반환한다.
                return seinUnitPrefab;
            }

            // 'string.Equals(monsterId, VegaMonsterId, StringComparison.OrdinalIgnoreCase)' 조건이 참인지 검사해 실행 분기를 결정한다.
            if (string.Equals(monsterId, VegaMonsterId, StringComparison.OrdinalIgnoreCase))
            {
                // 계산 또는 조회 결과 'vegaUnitPrefab'을 호출자에게 반환한다.
                return vegaUnitPrefab;
            }

            // [Fallback] 정상 결과를 만들 수 없을 때 기본 결과 'null'을 호출자에게 반환한다.
            return null;
        }

        // 직렬화된 적 프리팹 연결 배열에서 적 ID와 일치하는 프리팹을 찾는다.
        // 'ResolveEnemyPrefab' 메소드의 입력과 반환 계약을 선언한다.
        private GameObject ResolveEnemyPrefab(string enemyId)
        {
            // [방어 로직] 'enemyPrefabBindings != null' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
            if (enemyPrefabBindings != null)
            {
                // 'var i = 0; i < enemyPrefabBindings.Length; i++' 규칙으로 인덱스를 갱신하며 코드를 반복한다.
                for (var i = 0; i < enemyPrefabBindings.Length; i++)
                {
                    // 지역 변수 'binding'에 오른쪽 계산 또는 조회 결과를 저장한다.
                    var binding = enemyPrefabBindings[i];
                    // [방어 로직] 'binding == null || binding.Prefab == null' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
                    if (binding == null || binding.Prefab == null)
                    {
                        // 'continue' 값을 현재 메소드 호출의 인수로 전달한다.
                        continue;
                    }

                    // 'string.Equals(enemyId, binding.EnemyId, StringComparison.OrdinalIgnoreCase)' 조건이 참인지 검사해 실행 분기를 결정한다.
                    if (string.Equals(enemyId, binding.EnemyId, StringComparison.OrdinalIgnoreCase))
                    {
                        // 계산 또는 조회 결과 'binding.Prefab'을 호출자에게 반환한다.
                        return binding.Prefab;
                    }
                }
            }

            // [Fallback] 정상 결과를 만들 수 없을 때 기본 결과 'null'을 호출자에게 반환한다.
            return null;
        }

        // 적의 최대 체력과 현재 체력에 스테이지 체력 배율을 적용한다.
        // 'ApplyEnemyHealthMultiplier' 메소드의 입력과 반환 계약을 선언한다.
        private static void ApplyEnemyHealthMultiplier(EnemyUnitRuntimeModel model, float healthMultiplier)
        {
            // [방어 로직] 'model == null || healthMultiplier <= 0f || Mathf.Approximately(healthMultiplier, 1f)' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
            if (model == null || healthMultiplier <= 0f || Mathf.Approximately(healthMultiplier, 1f))
            {
                // [방어 로직] 현재 메소드의 남은 처리를 건너뛰고 즉시 호출 지점으로 돌아간다.
                return;
            }

            // [방어 로직] 'model.Stats != null' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
            if (model.Stats != null)
            {
                // 'model.Stats.MaxHealth *= healthMultiplier;' 식을 평가해 현재 계산 또는 상태 변경의 한 단계를 수행한다.
                model.Stats.MaxHealth *= healthMultiplier;
            }

            // [방어 로직] 'model.Resources != null' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
            if (model.Resources != null)
            {
                // 'model.Resources.CurrentHealth *= healthMultiplier;' 식을 평가해 현재 계산 또는 상태 변경의 한 단계를 수행한다.
                model.Resources.CurrentHealth *= healthMultiplier;
            }
        }

        // 현현 파티 슬롯 번호에 대응하는 씬의 nPSpawnPoint를 찾는다.
        // 'ResolveManifestSpawnPoint' 메소드의 입력과 반환 계약을 선언한다.
        private static Transform ResolveManifestSpawnPoint(int partySlotIndex)
        {
            // 씬에서 슬롯 이름으로 필수 생성 지점을 찾아 Transform을 반환한다.
            return GameObject.Find($"{partySlotIndex + 1}PSpawnPoint").transform;
        }
    }
}
