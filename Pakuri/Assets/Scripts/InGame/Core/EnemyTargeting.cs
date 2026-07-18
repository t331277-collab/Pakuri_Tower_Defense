// 'System.Collections.Generic' 네임스페이스의 타입과 API를 이 파일에서 사용한다.
using System.Collections.Generic;
// 'UnityEngine' 네임스페이스의 타입과 API를 이 파일에서 사용한다.
using UnityEngine;

// 'Pakuri.InGame' 네임스페이스 범위를 선언해 관련 타입 이름의 충돌을 막는다.
namespace Pakuri.InGame
{
    // 적 유닛이 플레이어, 넥서스, 아군 적을 선택할 때 사용하는 대상 검색 함수 모음이다.
    // 'EnemyTargeting' 클래스 정의를 시작한다.
    internal static class EnemyTargeting
    {
        // 살아 있는 일반 플레이어 중 가장 가까운 대상을 찾고, 없으면 넥서스를 찾는다.
        // 'FindNearestPlayerTarget' 메소드의 입력과 반환 계약을 선언한다.
        public static UnitRosterEntry FindNearestPlayerTarget(UnitRosterEntry enemyEntry, UnitRosterService roster)
        {
            // 지역 변수 'best'에 오른쪽 계산 또는 조회 결과를 저장한다.
            var best = FindNearestPlayerTarget(enemyEntry, roster, includeNexus: false);
            // [Fallback][낯선 문법] null 병합 연산자(??)로 왼쪽 값이 없으면 오른쪽 대체값을 반환한다.
            return best ?? FindNearestPlayerTarget(enemyEntry, roster, includeNexus: true);
        }

        // 넥서스 포함 여부에 맞는 플레이어 후보 중 거리가 가장 가까운 항목을 찾는다.
        // 'FindNearestPlayerTarget' 메소드의 입력과 반환 계약을 선언한다.
        private static UnitRosterEntry FindNearestPlayerTarget(
            // 'enemyEntry' 매개변수 또는 지역값의 타입을 'UnitRosterEntry'로 지정한다.
            UnitRosterEntry enemyEntry,
            // 'roster' 매개변수 또는 지역값의 타입을 'UnitRosterService'로 지정한다.
            UnitRosterService roster,
            // 'includeNexus' 매개변수 또는 지역값의 타입을 'bool'로 지정한다.
            bool includeNexus)
        {
            // 지역 변수 'players'에 오른쪽 계산 또는 조회 결과를 저장한다.
            var players = roster.Players;
            // 'best' 지역 변수를 만들고 오른쪽 계산 또는 조회 결과로 초기화한다.
            UnitRosterEntry best = null;
            // 지역 변수 'bestDistanceSq'에 오른쪽 계산 또는 조회 결과를 저장한다.
            var bestDistanceSq = float.MaxValue;
            // 지역 변수 'origin'에 오른쪽 계산 또는 조회 결과를 저장한다.
            var origin = enemyEntry.Transform.position;
            // 'var i = 0; i < players.Count; i++' 규칙으로 인덱스를 갱신하며 코드를 반복한다.
            for (var i = 0; i < players.Count; i++)
            {
                // 지역 변수 'candidate'에 오른쪽 계산 또는 조회 결과를 저장한다.
                var candidate = players[i];
                // '!IsActive(candidate)' 조건이 참인지 검사해 실행 분기를 결정한다.
                if (!IsActive(candidate))
                {
                    // 'continue' 값을 현재 메소드 호출의 인수로 전달한다.
                    continue;
                }

                // 지역 변수 'isNexus'에 오른쪽 계산 또는 조회 결과를 저장한다.
                var isNexus = IsNexus(candidate);
                // 'isNexus != includeNexus' 조건이 참인지 검사해 실행 분기를 결정한다.
                if (isNexus != includeNexus)
                {
                    // 'continue' 값을 현재 메소드 호출의 인수로 전달한다.
                    continue;
                }

                // 지역 변수 'offset'에 오른쪽 계산 또는 조회 결과를 저장한다.
                var offset = candidate.Transform.position - origin;
                // 'offset.z'에 오른쪽 계산 또는 조회 결과를 저장한다.
                offset.z = 0f;
                // 지역 변수 'distanceSq'에 오른쪽 계산 또는 조회 결과를 저장한다.
                var distanceSq = offset.sqrMagnitude;
                // 'distanceSq >= bestDistanceSq' 조건이 참인지 검사해 실행 분기를 결정한다.
                if (distanceSq >= bestDistanceSq)
                {
                    // 'continue' 값을 현재 메소드 호출의 인수로 전달한다.
                    continue;
                }

                // 'best'에 오른쪽 계산, 조회, 또는 상수 결과를 저장한다.
                best = candidate;
                // 'bestDistanceSq'에 오른쪽 계산, 조회, 또는 상수 결과를 저장한다.
                bestDistanceSq = distanceSq;
            }

            // 계산 또는 조회 결과 'best'을 호출자에게 반환한다.
            return best;
        }

        // 로스터 항목의 역할이 넥서스인지 판별한다.
        // 'IsNexus' 메소드의 입력과 반환 계약을 선언한다.
        public static bool IsNexus(UnitRosterEntry entry)
        {
            // [Fallback][낯선 문법] 삼항 연산자(?:)로 조건에 따라 정상값 또는 대체값을 선택한다.
            var identity = entry != null && entry.Model != null ? entry.Model.Identity : null;
            // 계산 또는 조회 결과 'identity != null && identity.Role == UnitRole.Nexus'을 호출자에게 반환한다.
            return identity != null && identity.Role == UnitRole.Nexus;
        }

        // 넥서스를 제외한 살아 있는 플레이어 중 적에게서 가장 먼 대상을 찾는다.
        // 'FindFarthestPlayerTarget' 메소드의 입력과 반환 계약을 선언한다.
        public static UnitRosterEntry FindFarthestPlayerTarget(UnitRosterEntry enemyEntry, UnitRosterService roster)
        {
            // [Fallback][낯선 문법] 삼항 연산자(?:)로 조건에 따라 정상값 또는 대체값을 선택한다.
            var players = roster != null ? roster.Players : null;
            // 'best' 지역 변수를 만들고 오른쪽 계산 또는 조회 결과로 초기화한다.
            UnitRosterEntry best = null;
            // 지역 변수 'bestDistanceSq'에 오른쪽 계산 또는 조회 결과를 저장한다.
            var bestDistanceSq = float.MinValue;
            // [Fallback][낯선 문법] 삼항 연산자(?:)로 조건에 따라 정상값 또는 대체값을 선택한다.
            var origin = enemyEntry != null && enemyEntry.Transform != null ? enemyEntry.Transform.position : Vector3.zero;
            // [방어 로직] 'players == null' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
            if (players == null)
            {
                // [Fallback] 정상 결과를 만들 수 없을 때 기본 결과 'null'을 호출자에게 반환한다.
                return null;
            }

            // 'var i = 0; i < players.Count; i++' 규칙으로 인덱스를 갱신하며 코드를 반복한다.
            for (var i = 0; i < players.Count; i++)
            {
                // 지역 변수 'candidate'에 오른쪽 계산 또는 조회 결과를 저장한다.
                var candidate = players[i];
                // '!IsActive(candidate) || IsNexus(candidate)' 조건이 참인지 검사해 실행 분기를 결정한다.
                if (!IsActive(candidate) || IsNexus(candidate))
                {
                    // 'continue' 값을 현재 메소드 호출의 인수로 전달한다.
                    continue;
                }

                // 지역 변수 'offset'에 오른쪽 계산 또는 조회 결과를 저장한다.
                var offset = candidate.Transform.position - origin;
                // 'offset.z'에 오른쪽 계산 또는 조회 결과를 저장한다.
                offset.z = 0f;
                // 지역 변수 'distanceSq'에 오른쪽 계산 또는 조회 결과를 저장한다.
                var distanceSq = offset.sqrMagnitude;
                // 'distanceSq <= bestDistanceSq' 조건이 참인지 검사해 실행 분기를 결정한다.
                if (distanceSq <= bestDistanceSq)
                {
                    // 'continue' 값을 현재 메소드 호출의 인수로 전달한다.
                    continue;
                }

                // 'best'에 오른쪽 계산, 조회, 또는 상수 결과를 저장한다.
                best = candidate;
                // 'bestDistanceSq'에 오른쪽 계산, 조회, 또는 상수 결과를 저장한다.
                bestDistanceSq = distanceSq;
            }

            // 계산 또는 조회 결과 'best'을 호출자에게 반환한다.
            return best;
        }

        // 넥서스를 제외한 살아 있는 플레이어 후보 중 하나를 무작위로 선택한다.
        // 'FindRandomPlayerTarget' 메소드의 입력과 반환 계약을 선언한다.
        public static UnitRosterEntry FindRandomPlayerTarget(UnitRosterService roster)
        {
            // [Fallback][낯선 문법] 삼항 연산자(?:)로 조건에 따라 정상값 또는 대체값을 선택한다.
            var players = roster != null ? roster.Players : null;
            // [방어 로직] 'players == null' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
            if (players == null)
            {
                // [Fallback] 정상 결과를 만들 수 없을 때 기본 결과 'null'을 호출자에게 반환한다.
                return null;
            }

            // 지역 변수 'candidates'에 오른쪽 계산 또는 조회 결과를 저장한다.
            var candidates = new List<UnitRosterEntry>();
            // 'var i = 0; i < players.Count; i++' 규칙으로 인덱스를 갱신하며 코드를 반복한다.
            for (var i = 0; i < players.Count; i++)
            {
                // 지역 변수 'candidate'에 오른쪽 계산 또는 조회 결과를 저장한다.
                var candidate = players[i];
                // 'IsActive(candidate) && !IsNexus(candidate)' 조건이 참인지 검사해 실행 분기를 결정한다.
                if (IsActive(candidate) && !IsNexus(candidate))
                {
                    // Add 호출 결과 또는 지정 항목을 컬렉션에 추가한다.
                    candidates.Add(candidate);
                }
            }

            // [Fallback][낯선 문법] 삼항 연산자(?:)로 조건 결과에 맞는 값 하나를 반환한다.
            return candidates.Count > 0 ? candidates[Random.Range(0, candidates.Count)] : null;
        }

        // 넥서스를 제외한 모든 살아 있는 플레이어 대상을 목록으로 반환한다.
        // 'FindAllPlayerTargets' 메소드의 입력과 반환 계약을 선언한다.
        public static List<UnitRosterEntry> FindAllPlayerTargets(UnitRosterService roster)
        {
            // 지역 변수 'result'에 오른쪽 계산 또는 조회 결과를 저장한다.
            var result = new List<UnitRosterEntry>();
            // [Fallback][낯선 문법] 삼항 연산자(?:)로 조건에 따라 정상값 또는 대체값을 선택한다.
            var players = roster != null ? roster.Players : null;
            // [방어 로직] 'players == null' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
            if (players == null)
            {
                // 계산 또는 조회 결과 'result'을 호출자에게 반환한다.
                return result;
            }

            // 'var i = 0; i < players.Count; i++' 규칙으로 인덱스를 갱신하며 코드를 반복한다.
            for (var i = 0; i < players.Count; i++)
            {
                // 지역 변수 'candidate'에 오른쪽 계산 또는 조회 결과를 저장한다.
                var candidate = players[i];
                // 'IsActive(candidate) && !IsNexus(candidate)' 조건이 참인지 검사해 실행 분기를 결정한다.
                if (IsActive(candidate) && !IsNexus(candidate))
                {
                    // Add 호출 결과 또는 지정 항목을 컬렉션에 추가한다.
                    result.Add(candidate);
                }
            }

            // 계산 또는 조회 결과 'result'을 호출자에게 반환한다.
            return result;
        }

        // 체력이 감소한 적 진영 유닛 중 현재 체력 비율이 가장 낮은 아군을 찾는다.
        // 'FindLowestHealthEnemyAlly' 메소드의 입력과 반환 계약을 선언한다.
        public static UnitRosterEntry FindLowestHealthEnemyAlly(UnitRosterService roster)
        {
            // 지역 변수 'enemies'에 오른쪽 계산 또는 조회 결과를 저장한다.
            var enemies = roster.Enemies;
            // 'best' 지역 변수를 만들고 오른쪽 계산 또는 조회 결과로 초기화한다.
            UnitRosterEntry best = null;
            // 지역 변수 'bestHealthRatio'에 오른쪽 계산 또는 조회 결과를 저장한다.
            var bestHealthRatio = float.MaxValue;

            // 'var i = 0; i < enemies.Count; i++' 규칙으로 인덱스를 갱신하며 코드를 반복한다.
            for (var i = 0; i < enemies.Count; i++)
            {
                // 지역 변수 'candidate'에 오른쪽 계산 또는 조회 결과를 저장한다.
                var candidate = enemies[i];
                // [방어 로직] '!IsActive(candidate) || candidate.Model == null || candidate.Transform == null' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
                if (!IsActive(candidate) || candidate.Model == null || candidate.Transform == null)
                {
                    // 'continue' 값을 현재 메소드 호출의 인수로 전달한다.
                    continue;
                }

                // 지역 변수 'resources'에 오른쪽 계산 또는 조회 결과를 저장한다.
                var resources = candidate.Model.Resources;
                // 지역 변수 'stats'에 오른쪽 계산 또는 조회 결과를 저장한다.
                var stats = candidate.Model.Stats;
                // [방어 로직] 'resources == null || stats == null || stats.MaxHealth <= 0f' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
                if (resources == null || stats == null || stats.MaxHealth <= 0f)
                {
                    // 'continue' 값을 현재 메소드 호출의 인수로 전달한다.
                    continue;
                }

                // [방어 로직] Mathf 범위 함수로 계산값이 허용 범위를 벗어나지 않게 보정한다.
                var healthRatio = Mathf.Clamp01(resources.CurrentHealth / stats.MaxHealth);
                // 'healthRatio >= 1f || healthRatio >= bestHealthRatio' 조건이 참인지 검사해 실행 분기를 결정한다.
                if (healthRatio >= 1f || healthRatio >= bestHealthRatio)
                {
                    // 'continue' 값을 현재 메소드 호출의 인수로 전달한다.
                    continue;
                }

                // 'best'에 오른쪽 계산, 조회, 또는 상수 결과를 저장한다.
                best = candidate;
                // 'bestHealthRatio'에 오른쪽 계산, 조회, 또는 상수 결과를 저장한다.
                bestHealthRatio = healthRatio;
            }

            // 계산 또는 조회 결과 'best'을 호출자에게 반환한다.
            return best;
        }

        // 기준 적 유닛을 중심으로 지정 반경 안에 있는 살아 있는 적 아군을 찾는다.
        // 'FindEnemyAlliesInRadius' 메소드의 입력과 반환 계약을 선언한다.
        public static List<UnitRosterEntry> FindEnemyAlliesInRadius(
            // 'source' 매개변수 또는 지역값의 타입을 'UnitRosterEntry'로 지정한다.
            UnitRosterEntry source,
            // 'roster' 매개변수 또는 지역값의 타입을 'UnitRosterService'로 지정한다.
            UnitRosterService roster,
            // 'radius' 매개변수 또는 지역값의 타입을 'float'로 지정한다.
            float radius)
        {
            // 지역 변수 'result'에 오른쪽 계산 또는 조회 결과를 저장한다.
            var result = new List<UnitRosterEntry>();
            // [방어 로직] 'source == null || source.Transform == null || roster == null' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
            if (source == null || source.Transform == null || roster == null)
            {
                // 계산 또는 조회 결과 'result'을 호출자에게 반환한다.
                return result;
            }

            // [Fallback][낯선 문법] 삼항 연산자(?:)로 조건에 따라 정상값 또는 대체값을 선택한다.
            var radiusSq = radius > 0f ? radius * radius : 0f;
            // 지역 변수 'origin'에 오른쪽 계산 또는 조회 결과를 저장한다.
            var origin = source.Transform.position;
            // 지역 변수 'enemies'에 오른쪽 계산 또는 조회 결과를 저장한다.
            var enemies = roster.Enemies;
            // 'var i = 0; i < enemies.Count; i++' 규칙으로 인덱스를 갱신하며 코드를 반복한다.
            for (var i = 0; i < enemies.Count; i++)
            {
                // 지역 변수 'candidate'에 오른쪽 계산 또는 조회 결과를 저장한다.
                var candidate = enemies[i];
                // [방어 로직] '!IsActive(candidate) || candidate.Model == null || candidate.Transform == null' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
                if (!IsActive(candidate) || candidate.Model == null || candidate.Transform == null)
                {
                    // 'continue' 값을 현재 메소드 호출의 인수로 전달한다.
                    continue;
                }

                // 지역 변수 'offset'에 오른쪽 계산 또는 조회 결과를 저장한다.
                var offset = candidate.Transform.position - origin;
                // 'offset.z'에 오른쪽 계산 또는 조회 결과를 저장한다.
                offset.z = 0f;
                // [방어 로직] 'radius <= 0f && candidate != source' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
                if (radius <= 0f && candidate != source)
                {
                    // 'continue' 값을 현재 메소드 호출의 인수로 전달한다.
                    continue;
                }

                // 'radius > 0f && offset.sqrMagnitude > radiusSq' 조건이 참인지 검사해 실행 분기를 결정한다.
                if (radius > 0f && offset.sqrMagnitude > radiusSq)
                {
                    // 'continue' 값을 현재 메소드 호출의 인수로 전달한다.
                    continue;
                }

                // Add 호출 결과 또는 지정 항목을 컬렉션에 추가한다.
                result.Add(candidate);
            }

            // 계산 또는 조회 결과 'result'을 호출자에게 반환한다.
            return result;
        }

        // 로스터 항목이 살아 있고 유효한 Transform을 가졌는지 판별한다.
        // 'IsActive' 메소드의 입력과 반환 계약을 선언한다.
        public static bool IsActive(UnitRosterEntry entry)
        {
            // 계산 또는 조회 결과 'entry != null && entry.IsAlive && entry.Transform != null'을 호출자에게 반환한다.
            return entry != null && entry.IsAlive && entry.Transform != null;
        }
    }
}
