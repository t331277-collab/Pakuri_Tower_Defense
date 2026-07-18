// 'System' 네임스페이스의 타입과 API를 이 파일에서 사용한다.
using System;
// 'System.Collections.Generic' 네임스페이스의 타입과 API를 이 파일에서 사용한다.
using System.Collections.Generic;
// 'Pakuri.Data' 네임스페이스의 타입과 API를 이 파일에서 사용한다.
using Pakuri.Data;
// 'UnityEngine' 네임스페이스의 타입과 API를 이 파일에서 사용한다.
using UnityEngine;

// 'Pakuri.InGame' 네임스페이스 범위를 선언해 관련 타입 이름의 충돌을 막는다.
namespace Pakuri.InGame
{
    // 몬스터·스킬별 효과 프리팹을 조회하고 런타임 스킬 오브젝트의 생성과 정리를 담당한다.
    // 'EffectManager' 클래스 정의를 시작한다.
    public class EffectManager : MonoBehaviour
    {
        // 하나의 스킬 ID와 해당 효과 프리팹 연결을 직렬화한다.
        // [낯선 문법] Serializable attribute: 이 타입의 필드 값을 Unity 직렬화 대상으로 만든다.
        [Serializable]
        // 'MonsterSkillEffectEntry' 클래스 정의를 시작한다.
        private class MonsterSkillEffectEntry
        {
            // 'Empty' 필드 또는 property를 선언해 객체 상태를 보관하거나 공개한다.
            public string SkillId = string.Empty;
            // 'null' 필드 또는 property를 선언해 객체 상태를 보관하거나 공개한다.
            public GameObject Prefab = null;
        }

        // 한 몬스터가 사용하는 스킬별 효과 프리팹 연결 목록을 묶는다.
        // [낯선 문법] Serializable attribute: 이 타입의 필드 값을 Unity 직렬화 대상으로 만든다.
        [Serializable]
        // 'MonsterSkillEffectGroup' 클래스 정의를 시작한다.
        private class MonsterSkillEffectGroup
        {
            // 'Empty' 필드 또는 property를 선언해 객체 상태를 보관하거나 공개한다.
            public string MonsterId = string.Empty;
            // 'SkillEffects' 필드를 선언하고 새 객체 또는 호출 결과로 초기화한다.
            public List<MonsterSkillEffectEntry> SkillEffects = new List<MonsterSkillEffectEntry>();
        }

        // [낯선 문법] SerializeField attribute: private 상태 'runtimeSkillRoot'을 Unity 직렬화와 Inspector 편집 대상으로 만든다.
        [SerializeField] private Transform runtimeSkillRoot;
        // [낯선 문법] SerializeField attribute: private 상태 'monsterSkillEffects'을 Unity 직렬화와 Inspector 편집 대상으로 만든다.
        [SerializeField] private List<MonsterSkillEffectGroup> monsterSkillEffects = new List<MonsterSkillEffectGroup>();

        // [낯선 문법] readonly 필드 'monsterLookup'를 초기화하며, 생성 뒤에는 이 참조를 다시 대입할 수 없다.
        private readonly Dictionary<string, Dictionary<string, GameObject>> monsterLookup =
            // 'new Dictionary<string, Dictionary<string, GameObject>>(StringComparer.OrdinalIgnoreCase);' 식을 평가해 현재 계산 또는 상태 변경의 한 단계를 수행한다.
            new Dictionary<string, Dictionary<string, GameObject>>(StringComparer.OrdinalIgnoreCase);

        // 'true' 필드 또는 property를 선언해 객체 상태를 보관하거나 공개한다.
        private bool lookupDirty = true;

        // 시전자 모델의 몬스터 ID와 스킬 ID로 등록된 효과 프리팹을 찾는다.
        // 'ResolveMonsterSkillEffectPrefab' 메소드의 입력과 반환 계약을 선언한다.
        public GameObject ResolveMonsterSkillEffectPrefab(BaseUnitRuntimeModel caster, string skillId)
        {
            // 지역 변수 'monsterId'에 저장할 여러 줄 계산 또는 선택식을 시작한다.
            var monsterId = caster != null && caster.Identity != null
                // [낯선 문법] 삼항 연산자의 조건 참 결과로 'caster.Identity.DefinitionId' 값을 선택한다.
                ? caster.Identity.DefinitionId
                // [Fallback][낯선 문법] 삼항 연산자의 조건 거짓 대체값으로 'null;' 값을 선택한다.
                : null;
            // 계산 또는 조회 결과 'ResolveMonsterSkillEffectPrefab(monsterId, skillId)'을 호출자에게 반환한다.
            return ResolveMonsterSkillEffectPrefab(monsterId, skillId);
        }

        // 몬스터 ID와 스킬 ID를 정규화해 효과 프리팹 조회 테이블에서 찾는다.
        // 'ResolveMonsterSkillEffectPrefab' 메소드의 입력과 반환 계약을 선언한다.
        public GameObject ResolveMonsterSkillEffectPrefab(string monsterId, string skillId)
        {
            // [방어 로직] 'string.IsNullOrWhiteSpace(monsterId) || string.IsNullOrWhiteSpace(skillId)' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
            if (string.IsNullOrWhiteSpace(monsterId) || string.IsNullOrWhiteSpace(skillId))
            {
                // [Fallback] 정상 결과를 만들 수 없을 때 기본 결과 'null'을 호출자에게 반환한다.
                return null;
            }

            // 'EnsureLookup' 메소드를 호출해 현재 단계의 처리를 실행한다.
            EnsureLookup();
            // 여러 줄로 이어지는 계산 또는 조건 결과를 반환하기 시작한다.
            return monsterLookup.TryGetValue(NormalizeKey(monsterId), out var skillMap)
                   // [낯선 문법] out 인수로 메소드 성공 여부와 함께 추가 결과값을 받아온다.
                   && skillMap.TryGetValue(NormalizeKey(skillId), out var prefab)
                // [낯선 문법] 삼항 연산자의 조건 참 결과로 'prefab' 값을 선택한다.
                ? prefab
                // [Fallback][낯선 문법] 삼항 연산자의 조건 거짓 대체값으로 'null;' 값을 선택한다.
                : null;
        }

        // 전달된 프리팹을 런타임 스킬 루트 아래 지정 위치와 회전으로 생성한다.
        // 'InstantiateSkillPrefab' 메소드의 입력과 반환 계약을 선언한다.
        public GameObject InstantiateSkillPrefab(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            // 여러 줄로 이어지는 계산 또는 조건 결과를 반환하기 시작한다.
            return prefab != null
                // Unity 원본 Object를 복제해 런타임 인스턴스를 생성한다.
                ? Instantiate(prefab, position, rotation, runtimeSkillRoot)
                // [Fallback][낯선 문법] 삼항 연산자의 조건 거짓 대체값으로 'null;' 값을 선택한다.
                : null;
        }

        // 프리팹 없이 사용할 빈 런타임 스킬 GameObject를 만들고 스킬 루트에 배치한다.
        // 'CreateRuntimeSkillObject' 메소드의 입력과 반환 계약을 선언한다.
        public GameObject CreateRuntimeSkillObject(string objectName, Vector3 position, Quaternion rotation)
        {
            // [Fallback][낯선 문법] 삼항 연산자(?:)로 조건에 따라 정상값 또는 대체값을 선택한다.
            var instance = new GameObject(string.IsNullOrWhiteSpace(objectName) ? "RuntimeSkillVisual" : objectName);
            // 지역 변수 'transform'에 오른쪽 계산 또는 조회 결과를 저장한다.
            var transform = instance.transform;
            // 직렬화된 런타임 스킬 루트를 생성한 오브젝트의 부모로 설정한다.
            transform.SetParent(runtimeSkillRoot, false);
            // 'transform.SetPositionAndRotation' 메소드를 호출해 해당 객체의 처리를 실행한다.
            transform.SetPositionAndRotation(position, rotation);
            // 계산 또는 조회 결과 'instance'을 호출자에게 반환한다.
            return instance;
        }

        // 런타임 스킬 루트 아래 생성된 모든 효과 오브젝트를 비활성화하고 제거한다.
        // 'ClearRuntimeSkillObjects' 메소드의 입력과 반환 계약을 선언한다.
        public void ClearRuntimeSkillObjects()
        {
            // 지역 변수 'root'에 직렬화된 런타임 스킬 루트를 저장한다.
            var root = runtimeSkillRoot;
            // 'var i = root.childCount - 1; i >= 0; i--' 규칙으로 인덱스를 갱신하며 코드를 반복한다.
            for (var i = root.childCount - 1; i >= 0; i--)
            {
                // 지역 변수 'child'에 오른쪽 계산 또는 조회 결과를 저장한다.
                var child = root.GetChild(i);
                // [방어 로직] 'child == null' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
                if (child == null)
                {
                    // 'continue' 값을 현재 메소드 호출의 인수로 전달한다.
                    continue;
                }

                // 'child.gameObject.SetActive' 메소드를 호출해 해당 객체의 처리를 실행한다.
                child.gameObject.SetActive(false);
                // 지정 Unity Object를 수명 종료 시점에 제거한다.
                Destroy(child.gameObject);
            }
        }

        // 런타임 시작 시 직렬화된 효과 목록을 다시 색인하도록 표시한다.
        // 'Awake' 메소드의 입력과 반환 계약을 선언한다.
        private void Awake()
        {
            // 'lookupDirty'에 오른쪽 계산, 조회, 또는 상수 결과를 저장한다.
            lookupDirty = true;
        }

        // Inspector 값이 바뀌면 효과 조회 테이블을 다시 만들도록 표시한다.
        // 'OnValidate' 메소드의 입력과 반환 계약을 선언한다.
        private void OnValidate()
        {
            // 'lookupDirty'에 오른쪽 계산, 조회, 또는 상수 결과를 저장한다.
            lookupDirty = true;
        }

        // 몬스터 ID와 스킬 ID를 키로 하는 중첩 효과 프리팹 조회 테이블을 지연 생성한다.
        // 'EnsureLookup' 메소드의 입력과 반환 계약을 선언한다.
        private void EnsureLookup()
        {
            // '!lookupDirty' 조건이 참인지 검사해 실행 분기를 결정한다.
            if (!lookupDirty)
            {
                // [방어 로직] 현재 메소드의 남은 처리를 건너뛰고 즉시 호출 지점으로 돌아간다.
                return;
            }

            // 'lookupDirty'에 오른쪽 계산, 조회, 또는 상수 결과를 저장한다.
            lookupDirty = false;
            // 컬렉션에 남은 항목을 모두 제거해 상태를 초기화한다.
            monsterLookup.Clear();

            // 'var i = 0; i < monsterSkillEffects.Count; i++' 규칙으로 인덱스를 갱신하며 코드를 반복한다.
            for (var i = 0; i < monsterSkillEffects.Count; i++)
            {
                // 지역 변수 'group'에 오른쪽 계산 또는 조회 결과를 저장한다.
                var group = monsterSkillEffects[i];
                // [방어 로직] 'group == null || string.IsNullOrWhiteSpace(group.MonsterId)' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
                if (group == null || string.IsNullOrWhiteSpace(group.MonsterId))
                {
                    // 'continue' 값을 현재 메소드 호출의 인수로 전달한다.
                    continue;
                }

                // 지역 변수 'monsterId'에 오른쪽 계산 또는 조회 결과를 저장한다.
                var monsterId = NormalizeKey(group.MonsterId);
                // [방어 로직] '!monsterLookup.TryGetValue(monsterId, out var skillMap)' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
                if (!monsterLookup.TryGetValue(monsterId, out var skillMap))
                {
                    // 'skillMap'에 오른쪽 계산 또는 조회 결과를 저장한다.
                    skillMap = new Dictionary<string, GameObject>(StringComparer.OrdinalIgnoreCase);
                    // Add 호출 결과 또는 지정 항목을 컬렉션에 추가한다.
                    monsterLookup.Add(monsterId, skillMap);
                }

                // [방어 로직] 'group.SkillEffects == null' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
                if (group.SkillEffects == null)
                {
                    // 'continue' 값을 현재 메소드 호출의 인수로 전달한다.
                    continue;
                }

                // 'var j = 0; j < group.SkillEffects.Count; j++' 규칙으로 인덱스를 갱신하며 코드를 반복한다.
                for (var j = 0; j < group.SkillEffects.Count; j++)
                {
                    // 지역 변수 'entry'에 오른쪽 계산 또는 조회 결과를 저장한다.
                    var entry = group.SkillEffects[j];
                    // [방어 로직] 'entry == null || string.IsNullOrWhiteSpace(entry.SkillId) || entry.Prefab == null' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
                    if (entry == null || string.IsNullOrWhiteSpace(entry.SkillId) || entry.Prefab == null)
                    {
                        // 'continue' 값을 현재 메소드 호출의 인수로 전달한다.
                        continue;
                    }

                    // 'skillMap[NormalizeKey(entry.SkillId)] = entry.Prefab;' 식을 평가해 현재 계산 또는 상태 변경의 한 단계를 수행한다.
                    skillMap[NormalizeKey(entry.SkillId)] = entry.Prefab;
                }
            }

        }

        // 조회 키의 앞뒤 공백을 제거하고 빈 값은 빈 문자열로 통일한다.
        // 'NormalizeKey' 메소드의 입력과 반환 계약을 선언한다.
        private static string NormalizeKey(string value)
        {
            // 여러 줄로 이어지는 계산 또는 조건 결과를 반환하기 시작한다.
            return string.IsNullOrWhiteSpace(value)
                // [낯선 문법] 삼항 연산자의 조건 참 결과로 'string.Empty' 값을 선택한다.
                ? string.Empty
                // [Fallback][낯선 문법] 삼항 연산자의 조건 거짓 대체값으로 'value.Trim();' 값을 선택한다.
                : value.Trim();
        }
    }
}
