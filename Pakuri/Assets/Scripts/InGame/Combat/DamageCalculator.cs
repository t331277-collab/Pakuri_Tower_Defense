// 'System' 네임스페이스의 타입과 API를 이 파일에서 사용한다.
using System;
// 'UnityEngine' 네임스페이스의 타입과 API를 이 파일에서 사용한다.
using UnityEngine;

// 'Pakuri.Combat' 네임스페이스 범위를 선언해 관련 타입 이름의 충돌을 막는다.
namespace Pakuri.Combat
{
    // 피해 계산에 사용되는 물리 및 원소 속성 종류를 정의한다.
    // 'DamageAttribute' 열거형 정의를 시작한다.
    public enum DamageAttribute
    {
        // 'Physical' 열거값을 선택 가능한 상수 항목으로 정의한다.
        Physical,
        // 'Fire' 열거값을 선택 가능한 상수 항목으로 정의한다.
        Fire,
        // 'Lightning' 열거값을 선택 가능한 상수 항목으로 정의한다.
        Lightning,
        // 'Ice' 열거값을 선택 가능한 상수 항목으로 정의한다.
        Ice,
        // 'Darkness' 열거값을 선택 가능한 상수 항목으로 정의한다.
        Darkness,
        // 'Holy' 열거값을 선택 가능한 상수 항목으로 정의한다.
        Holy
    }

    // 방어력, 속성 저항, 치명타, 최종 배율을 조합해 최종 피해를 계산한다.
    // 'DamageCalculator' 클래스 정의를 시작한다.
    public static class DamageCalculator
    {
        // 'BaseCriticalChance' 상수에 실행 중 바뀌지 않는 기준값을 선언한다.
        public const float BaseCriticalChance = 0.05f;
        // 'BaseCriticalMultiplier' 상수에 실행 중 바뀌지 않는 기준값을 선언한다.
        public const float BaseCriticalMultiplier = 1.5f;

        // 피해 속성별 방어력 값을 저장하고 조회하는 직렬화 가능한 집합이다.
        // [낯선 문법] Serializable attribute: 이 타입의 필드 값을 Unity 직렬화 대상으로 만든다.
        [Serializable]
        // 'AttributeDefenseSet' 클래스 정의를 시작한다.
        public class AttributeDefenseSet
        {
            // 'Physical' 필드 또는 property를 선언해 객체 상태를 보관하거나 공개한다.
            public float Physical;
            // 'Fire' 필드 또는 property를 선언해 객체 상태를 보관하거나 공개한다.
            public float Fire;
            // 'Lightning' 필드 또는 property를 선언해 객체 상태를 보관하거나 공개한다.
            public float Lightning;
            // 'Ice' 필드 또는 property를 선언해 객체 상태를 보관하거나 공개한다.
            public float Ice;
            // 'Darkness' 필드 또는 property를 선언해 객체 상태를 보관하거나 공개한다.
            public float Darkness;
            // 'Holy' 필드 또는 property를 선언해 객체 상태를 보관하거나 공개한다.
            public float Holy;

            // 지정한 피해 속성에 대응하는 방어력 값을 반환한다.
            // 'Get' 메소드의 입력과 반환 계약을 선언한다.
            public float Get(DamageAttribute attribute)
            {
                // 'attribute' 값에 따라 여러 처리 경로 중 하나를 선택한다.
                switch (attribute)
                {
                    // 물리 속성에는 물리 방어력을 반환한다.
                    case DamageAttribute.Physical:
                        // 계산 또는 조회 결과 'Physical'을 호출자에게 반환한다.
                        return Physical;
                    // switch 값이 'DamageAttribute.Fire'일 때 이 분기를 실행한다.
                    case DamageAttribute.Fire:
                        // 계산 또는 조회 결과 'Fire'을 호출자에게 반환한다.
                        return Fire;
                    // switch 값이 'DamageAttribute.Lightning'일 때 이 분기를 실행한다.
                    case DamageAttribute.Lightning:
                        // 계산 또는 조회 결과 'Lightning'을 호출자에게 반환한다.
                        return Lightning;
                    // switch 값이 'DamageAttribute.Ice'일 때 이 분기를 실행한다.
                    case DamageAttribute.Ice:
                        // 계산 또는 조회 결과 'Ice'을 호출자에게 반환한다.
                        return Ice;
                    // switch 값이 'DamageAttribute.Darkness'일 때 이 분기를 실행한다.
                    case DamageAttribute.Darkness:
                        // 계산 또는 조회 결과 'Darkness'을 호출자에게 반환한다.
                        return Darkness;
                    // switch 값이 'DamageAttribute.Holy'일 때 이 분기를 실행한다.
                    case DamageAttribute.Holy:
                        // 계산 또는 조회 결과 'Holy'을 호출자에게 반환한다.
                        return Holy;
                    // 정의되지 않은 피해 속성이 전달되면 잘못된 값을 즉시 알린다.
                    default:
                        throw new ArgumentOutOfRangeException(nameof(attribute), attribute, null);
                }
            }

            // 현재 속성별 방어력 값을 복사한 새 집합을 만든다.
            // 'Clone' 메소드의 입력과 반환 계약을 선언한다.
            public AttributeDefenseSet Clone()
            {
                // 여러 줄로 이어지는 계산 또는 조건 결과를 반환하기 시작한다.
                return new AttributeDefenseSet
                {
                    // 'Physical'에 저장할 여러 줄 계산 또는 선택식을 시작한다.
                    Physical = Physical,
                    // 'Fire'에 저장할 여러 줄 계산 또는 선택식을 시작한다.
                    Fire = Fire,
                    // 'Lightning'에 저장할 여러 줄 계산 또는 선택식을 시작한다.
                    Lightning = Lightning,
                    // 'Ice'에 저장할 여러 줄 계산 또는 선택식을 시작한다.
                    Ice = Ice,
                    // 'Darkness'에 저장할 여러 줄 계산 또는 선택식을 시작한다.
                    Darkness = Darkness,
                    // 'Holy'에 저장할 여러 줄 계산 또는 선택식을 시작한다.
                    Holy = Holy
                };
            }
        }

        // 체력, 공격력, 이동 속도, 치명타 능력치를 한 묶음으로 저장한다.
        // [낯선 문법] Serializable attribute: 이 타입의 필드 값을 Unity 직렬화 대상으로 만든다.
        [Serializable]
        // 'CombatStatBlock' 클래스 정의를 시작한다.
        public class CombatStatBlock
        {
            // 'MaxHealth' 필드 또는 property를 선언해 객체 상태를 보관하거나 공개한다.
            public float MaxHealth = 100f;
            // 'AttackPower' 필드 또는 property를 선언해 객체 상태를 보관하거나 공개한다.
            public float AttackPower = 30f;
            // 'SpellPower' 필드 또는 property를 선언해 객체 상태를 보관하거나 공개한다.
            public float SpellPower = 30f;
            // 'MoveSpeed' 필드 또는 property를 선언해 객체 상태를 보관하거나 공개한다.
            public float MoveSpeed = 1f;
            // [방어 로직][낯선 문법] Range attribute: 'CriticalChance'의 Inspector 입력 범위를 0f, 1f로 제한한다.
            [Range(0f, 1f)] public float CriticalChance = BaseCriticalChance;
            // 'BaseCriticalMultiplier' 필드 또는 property를 선언해 객체 상태를 보관하거나 공개한다.
            public float CriticalDamage = BaseCriticalMultiplier;
            // [방어 로직][낯선 문법] Range attribute: 'CriticalResistance'의 Inspector 입력 범위를 0f, 1f로 제한한다.
            [Range(0f, 1f)] public float CriticalResistance;
        }

        // 최종 방어력이 만들어지는 각 가산·감산·비율 요소를 기록한다.
        // 'DefenseBreakdown' 값 형식 구조체 정의를 시작한다.
        public readonly struct DefenseBreakdown
        {
            // 속성별 방어력 계산 과정과 최종 값을 불변 데이터로 구성한다.
            // 'DefenseBreakdown' 메소드의 입력과 반환 계약을 선언한다.
            public DefenseBreakdown(
                // 'attribute' 매개변수 또는 지역값의 타입을 'DamageAttribute'로 지정한다.
                DamageAttribute attribute,
                // 'baseDefense' 매개변수 또는 지역값의 타입을 'float'로 지정한다.
                float baseDefense,
                // 'flatBonus' 매개변수 또는 지역값의 타입을 'float'로 지정한다.
                float flatBonus,
                // 'flatReduction' 매개변수 또는 지역값의 타입을 'float'로 지정한다.
                float flatReduction,
                // 'percentBonus' 매개변수 또는 지역값의 타입을 'float'로 지정한다.
                float percentBonus,
                // 'percentReductions' 매개변수 또는 지역값의 타입을 'float[]'로 지정한다.
                float[] percentReductions,
                // 'finalDefense' 매개변수 또는 지역값의 타입을 'float'로 지정한다.
                float finalDefense)
            {
                // 'Attribute'에 오른쪽 계산, 조회, 또는 상수 결과를 저장한다.
                Attribute = attribute;
                // 'BaseDefense'에 오른쪽 계산, 조회, 또는 상수 결과를 저장한다.
                BaseDefense = baseDefense;
                // 'FlatBonus'에 오른쪽 계산, 조회, 또는 상수 결과를 저장한다.
                FlatBonus = flatBonus;
                // 'FlatReduction'에 오른쪽 계산, 조회, 또는 상수 결과를 저장한다.
                FlatReduction = flatReduction;
                // 'PercentBonus'에 오른쪽 계산, 조회, 또는 상수 결과를 저장한다.
                PercentBonus = percentBonus;
                // 'PercentReductions'에 오른쪽 계산, 조회, 또는 상수 결과를 저장한다.
                PercentReductions = percentReductions ?? Array.Empty<float>();
                // 'FinalDefense'에 오른쪽 계산, 조회, 또는 상수 결과를 저장한다.
                FinalDefense = finalDefense;
            }

            // 'Attribute' 읽기 전용 property로 계산 결과 또는 상태를 외부에 공개한다.
            public DamageAttribute Attribute { get; }
            // 'BaseDefense' 읽기 전용 property로 계산 결과 또는 상태를 외부에 공개한다.
            public float BaseDefense { get; }
            // 'FlatBonus' 읽기 전용 property로 계산 결과 또는 상태를 외부에 공개한다.
            public float FlatBonus { get; }
            // 'FlatReduction' 읽기 전용 property로 계산 결과 또는 상태를 외부에 공개한다.
            public float FlatReduction { get; }
            // 'PercentBonus' 읽기 전용 property로 계산 결과 또는 상태를 외부에 공개한다.
            public float PercentBonus { get; }
            // 'PercentReductions' 읽기 전용 property로 계산 결과 또는 상태를 외부에 공개한다.
            public float[] PercentReductions { get; }
            // 'FinalDefense' 읽기 전용 property로 계산 결과 또는 상태를 외부에 공개한다.
            public float FinalDefense { get; }
        }

        // 물리 방어력과 기본 치명타 규칙을 적용한 최종 피해량을 반환한다.
        public static float Resolve(
            // 'baseDamage' 매개변수 또는 지역값의 타입을 'float'로 지정한다.
            float baseDamage,
            // 'defense' 매개변수 또는 지역값의 타입을 'float'로 지정한다.
            float defense,
            // [Fallback][낯선 문법] 선택 인수 'criticalChanceBonus'가 생략되면 기본값 '0f'을 사용한다.
            float criticalChanceBonus = 0f,
            // [Fallback][낯선 문법] 선택 인수 'criticalMultiplierBonus'가 생략되면 기본값 '0f'을 사용한다.
            float criticalMultiplierBonus = 0f)
        {
            // [방어 로직] Mathf 범위 함수로 계산값이 허용 범위를 벗어나지 않게 보정한다.
            var safeDefense = Mathf.Max(-95f, defense);
            // 지역 변수 'damageAfterDefense'에 오른쪽 계산 또는 조회 결과를 저장한다.
            var damageAfterDefense = baseDamage * (100f / (100f + safeDefense));
            // [방어 로직] Mathf 범위 함수로 계산값이 허용 범위를 벗어나지 않게 보정한다.
            var criticalChance = Mathf.Clamp01(BaseCriticalChance + criticalChanceBonus);
            // 지역 변수 'isCritical'에 오른쪽 계산 또는 조회 결과를 저장한다.
            var isCritical = UnityEngine.Random.value < criticalChance;
            // 지역 변수 'criticalMultiplier'에 오른쪽 계산 또는 조회 결과를 저장한다.
            var criticalMultiplier = BaseCriticalMultiplier + criticalMultiplierBonus;
            // 치명타가 발생하면 치명타 배율을 적용한다.
            var finalDamage = isCritical ? damageAfterDefense * criticalMultiplier : damageAfterDefense;
            // 계산을 마친 최종 피해량을 반환한다.
            return finalDamage;
        }

        // 속성 방어와 보정값을 치명타 허용 방식의 전체 계산 함수로 전달한다.
        public static float Resolve(
            // 'baseDamage' 매개변수 또는 지역값의 타입을 'float'로 지정한다.
            float baseDamage,
            // 'attribute' 매개변수 또는 지역값의 타입을 'DamageAttribute'로 지정한다.
            DamageAttribute attribute,
            // 'defenses' 매개변수 또는 지역값의 타입을 'AttributeDefenseSet'로 지정한다.
            AttributeDefenseSet defenses,
            // [Fallback][낯선 문법] 선택 인수 'flatDefenseBonus'가 생략되면 기본값 '0f'을 사용한다.
            float flatDefenseBonus = 0f,
            // [Fallback][낯선 문법] 선택 인수 'flatDefenseReduction'가 생략되면 기본값 '0f'을 사용한다.
            float flatDefenseReduction = 0f,
            // [Fallback][낯선 문법] 선택 인수 'percentDefenseBonus'가 생략되면 기본값 '0f'을 사용한다.
            float percentDefenseBonus = 0f,
            // [Fallback][낯선 문법] 선택 인수 'percentDefenseReductions'가 생략되면 기본값 'null'을 사용한다.
            float[] percentDefenseReductions = null,
            // [Fallback][낯선 문법] 선택 인수 'criticalChanceBonus'가 생략되면 기본값 '0f'을 사용한다.
            float criticalChanceBonus = 0f,
            // [Fallback][낯선 문법] 선택 인수 'criticalMultiplierBonus'가 생략되면 기본값 '0f'을 사용한다.
            float criticalMultiplierBonus = 0f,
            // [Fallback][낯선 문법] 선택 인수 'targetCriticalResistance'가 생략되면 기본값 '0f'을 사용한다.
            float targetCriticalResistance = 0f,
            // [Fallback][낯선 문법] 선택 인수 'criticalDamageTakenBonus'가 생략되면 기본값 '0f'을 사용한다.
            float criticalDamageTakenBonus = 0f,
            // [Fallback][낯선 문법] 선택 인수 'finalDamageMultiplier'가 생략되면 기본값 '1f'을 사용한다.
            float finalDamageMultiplier = 1f)
        {
            // 여러 줄로 이어지는 계산 또는 조건 결과를 반환하기 시작한다.
            return Resolve(
                // 'baseDamage' 열거값을 선택 가능한 상수 항목으로 정의한다.
                baseDamage,
                // 'attribute' 열거값을 선택 가능한 상수 항목으로 정의한다.
                attribute,
                // 'defenses' 열거값을 선택 가능한 상수 항목으로 정의한다.
                defenses,
                // 'true' 열거값을 선택 가능한 상수 항목으로 정의한다.
                true,
                // 'flatDefenseBonus' 열거값을 선택 가능한 상수 항목으로 정의한다.
                flatDefenseBonus,
                // 'flatDefenseReduction' 열거값을 선택 가능한 상수 항목으로 정의한다.
                flatDefenseReduction,
                // 'percentDefenseBonus' 열거값을 선택 가능한 상수 항목으로 정의한다.
                percentDefenseBonus,
                // 'percentDefenseReductions' 열거값을 선택 가능한 상수 항목으로 정의한다.
                percentDefenseReductions,
                // 'criticalChanceBonus' 열거값을 선택 가능한 상수 항목으로 정의한다.
                criticalChanceBonus,
                // 'criticalMultiplierBonus' 열거값을 선택 가능한 상수 항목으로 정의한다.
                criticalMultiplierBonus,
                // 'targetCriticalResistance' 열거값을 선택 가능한 상수 항목으로 정의한다.
                targetCriticalResistance,
                // 'criticalDamageTakenBonus' 열거값을 선택 가능한 상수 항목으로 정의한다.
                criticalDamageTakenBonus,
                // 'finalDamageMultiplier' 값을 현재 메소드 호출의 인수로 전달한다.
                finalDamageMultiplier);
        }

        // 모든 방어·치명타·최종 피해 보정을 적용한 최종 피해량을 반환한다.
        public static float Resolve(
            // 'baseDamage' 매개변수 또는 지역값의 타입을 'float'로 지정한다.
            float baseDamage,
            // 'attribute' 매개변수 또는 지역값의 타입을 'DamageAttribute'로 지정한다.
            DamageAttribute attribute,
            // 'defenses' 매개변수 또는 지역값의 타입을 'AttributeDefenseSet'로 지정한다.
            AttributeDefenseSet defenses,
            // 'criticalAllowed' 매개변수 또는 지역값의 타입을 'bool'로 지정한다.
            bool criticalAllowed,
            // [Fallback][낯선 문법] 선택 인수 'flatDefenseBonus'가 생략되면 기본값 '0f'을 사용한다.
            float flatDefenseBonus = 0f,
            // [Fallback][낯선 문법] 선택 인수 'flatDefenseReduction'가 생략되면 기본값 '0f'을 사용한다.
            float flatDefenseReduction = 0f,
            // [Fallback][낯선 문법] 선택 인수 'percentDefenseBonus'가 생략되면 기본값 '0f'을 사용한다.
            float percentDefenseBonus = 0f,
            // [Fallback][낯선 문법] 선택 인수 'percentDefenseReductions'가 생략되면 기본값 'null'을 사용한다.
            float[] percentDefenseReductions = null,
            // [Fallback][낯선 문법] 선택 인수 'criticalChanceBonus'가 생략되면 기본값 '0f'을 사용한다.
            float criticalChanceBonus = 0f,
            // [Fallback][낯선 문법] 선택 인수 'criticalMultiplierBonus'가 생략되면 기본값 '0f'을 사용한다.
            float criticalMultiplierBonus = 0f,
            // [Fallback][낯선 문법] 선택 인수 'targetCriticalResistance'가 생략되면 기본값 '0f'을 사용한다.
            float targetCriticalResistance = 0f,
            // [Fallback][낯선 문법] 선택 인수 'criticalDamageTakenBonus'가 생략되면 기본값 '0f'을 사용한다.
            float criticalDamageTakenBonus = 0f,
            // [Fallback][낯선 문법] 선택 인수 'finalDamageMultiplier'가 생략되면 기본값 '1f'을 사용한다.
            float finalDamageMultiplier = 1f)
        {
            // [Fallback][낯선 문법] 삼항 연산자(?:)로 조건에 따라 정상값 또는 대체값을 선택한다.
            var baseDefense = defenses != null ? defenses.Get(attribute) : 0f;
            // 지역 변수 'breakdown'에 저장할 여러 줄 계산 또는 선택식을 시작한다.
            var breakdown = ResolveDefense(
                // 'attribute' 열거값을 선택 가능한 상수 항목으로 정의한다.
                attribute,
                // 'baseDefense' 열거값을 선택 가능한 상수 항목으로 정의한다.
                baseDefense,
                // 'flatDefenseBonus' 열거값을 선택 가능한 상수 항목으로 정의한다.
                flatDefenseBonus,
                // 'flatDefenseReduction' 열거값을 선택 가능한 상수 항목으로 정의한다.
                flatDefenseReduction,
                // 'percentDefenseBonus' 열거값을 선택 가능한 상수 항목으로 정의한다.
                percentDefenseBonus,
                // 'percentDefenseReductions' 값을 현재 메소드 호출의 인수로 전달한다.
                percentDefenseReductions);

            // [방어 로직] Mathf 범위 함수로 계산값이 허용 범위를 벗어나지 않게 보정한다.
            var safeDefense = Mathf.Max(-95f, breakdown.FinalDefense);
            // 지역 변수 'damageAfterDefense'에 오른쪽 계산 또는 조회 결과를 저장한다.
            var damageAfterDefense = baseDamage * (100f / (100f + safeDefense));
            // [방어 로직] Mathf 범위 함수로 계산값이 허용 범위를 벗어나지 않게 보정한다.
            var criticalChance = Mathf.Clamp01(BaseCriticalChance + criticalChanceBonus - targetCriticalResistance);
            // 지역 변수 'isCritical'에 오른쪽 계산 또는 조회 결과를 저장한다.
            var isCritical = criticalAllowed && UnityEngine.Random.value < criticalChance;
            // 지역 변수 'criticalMultiplier'에 오른쪽 계산 또는 조회 결과를 저장한다.
            var criticalMultiplier = BaseCriticalMultiplier + criticalMultiplierBonus + criticalDamageTakenBonus;
            // 치명타가 발생하면 치명타 배율을 적용한다.
            var afterCritical = isCritical ? damageAfterDefense * criticalMultiplier : damageAfterDefense;
            // 최종 피해 배율이 음수가 되지 않도록 제한한다.
            var safeFinalMultiplier = Mathf.Max(0f, finalDamageMultiplier);
            // 치명타 적용 피해에 최종 피해 배율을 곱한다.
            var finalDamage = afterCritical * safeFinalMultiplier;
            // 계산을 마친 최종 피해량을 반환한다.
            return finalDamage;
        }

        // 고정 및 비율 방어 보정을 순서대로 적용해 최종 방어력 내역을 만든다.
        // 'ResolveDefense' 메소드의 입력과 반환 계약을 선언한다.
        public static DefenseBreakdown ResolveDefense(
            // 'attribute' 매개변수 또는 지역값의 타입을 'DamageAttribute'로 지정한다.
            DamageAttribute attribute,
            // 'baseDefense' 매개변수 또는 지역값의 타입을 'float'로 지정한다.
            float baseDefense,
            // 'flatDefenseBonus' 매개변수 또는 지역값의 타입을 'float'로 지정한다.
            float flatDefenseBonus,
            // 'flatDefenseReduction' 매개변수 또는 지역값의 타입을 'float'로 지정한다.
            float flatDefenseReduction,
            // 'percentDefenseBonus' 매개변수 또는 지역값의 타입을 'float'로 지정한다.
            float percentDefenseBonus,
            // 'percentDefenseReductions' 매개변수 또는 지역값의 타입을 'float[]'로 지정한다.
            float[] percentDefenseReductions)
        {
            // 지역 변수 'finalDefense'에 오른쪽 계산 또는 조회 결과를 저장한다.
            var finalDefense = (baseDefense + flatDefenseBonus - flatDefenseReduction) * (1f + percentDefenseBonus);
            // [Fallback][낯선 문법] null 병합 연산자(??): 왼쪽 값이 null이면 오른쪽 대체값을 사용한다.
            var safeReductions = percentDefenseReductions ?? System.Array.Empty<float>();
            // 'var i = 0; i < safeReductions.Length; i++' 규칙으로 인덱스를 갱신하며 코드를 반복한다.
            for (var i = 0; i < safeReductions.Length; i++)
            {
                // [방어 로직] Mathf 범위 함수로 계산값이 허용 범위를 벗어나지 않게 보정한다.
                finalDefense *= 1f - Mathf.Clamp01(safeReductions[i]);
            }

            // 여러 줄로 이어지는 계산 또는 조건 결과를 반환하기 시작한다.
            return new DefenseBreakdown(
                // 'attribute' 열거값을 선택 가능한 상수 항목으로 정의한다.
                attribute,
                // 'baseDefense' 열거값을 선택 가능한 상수 항목으로 정의한다.
                baseDefense,
                // 'flatDefenseBonus' 열거값을 선택 가능한 상수 항목으로 정의한다.
                flatDefenseBonus,
                // 'flatDefenseReduction' 열거값을 선택 가능한 상수 항목으로 정의한다.
                flatDefenseReduction,
                // 'percentDefenseBonus' 열거값을 선택 가능한 상수 항목으로 정의한다.
                percentDefenseBonus,
                // 'safeReductions' 열거값을 선택 가능한 상수 항목으로 정의한다.
                safeReductions,
                // 'finalDefense' 값을 현재 메소드 호출의 인수로 전달한다.
                finalDefense);
        }

    }
}
