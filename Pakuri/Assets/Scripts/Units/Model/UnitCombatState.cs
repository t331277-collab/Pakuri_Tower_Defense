/*
 * 역할: 공통 유닛 전투 모델.
 * 책임: 유닛 식별·진영·역할·능력치·방어·자원·스킬·상태·적 확장을 정의한다.
 */

using System;
using System.Collections.Generic;
using Pakuri.Combat;
using UnityEngine;

namespace Pakuri.InGame
{

    /// <summary><c>UnitSide</c>에서 지원하는 값의 종류를 정의한다.</summary>
    public enum UnitSide
    {
        Player,
        Enemy
    }

    /// <summary><c>UnitRole</c>에서 지원하는 값의 종류를 정의한다.</summary>
    public enum UnitRole
    {
        Monster,
        Enemy,
        Nexus
    }

    /// <summary><c>UnitIdentity</c>가 소유하는 데이터와 동작을 캡슐화한다.</summary>
    [Serializable]
    public class UnitIdentity
    {
        public string UnitId;
        public string DefinitionId;
        public string DisplayName;
        public UnitSide Side;
        public UnitRole Role;
        public int SlotIndex;
    }

    /// <summary><c>UnitCombatStats</c>가 소유하는 데이터와 동작을 캡슐화한다.</summary>
    [Serializable]
    public class UnitCombatStats
    {
        public float MaxHealth;
        public float AttackPower;
        public float SpellPower;
        public float MoveSpeed;
        public float CriticalChance;
        public float CriticalDamage;
        public float CriticalResistance;
    }

    /// <summary><c>UnitDefenseStats</c>가 소유하는 데이터와 동작을 캡슐화한다.</summary>
    [Serializable]
    public class UnitDefenseStats
    {
        public float Physical;
        public float Fire;
        public float Lightning;
        public float Ice;
        public float Darkness;
        public float Holy;

        /// <summary>전달된 <c>attribute</c> 값을 사용해 <c>요청값</c>를 반환한다.</summary>
        public float Get(DamageAttribute attribute)
        {
            switch (attribute)
            {
                case DamageAttribute.Physical:
                    return Physical;
                case DamageAttribute.Fire:
                    return Fire;
                case DamageAttribute.Lightning:
                    return Lightning;
                case DamageAttribute.Ice:
                    return Ice;
                case DamageAttribute.Darkness:
                    return Darkness;
                case DamageAttribute.Holy:
                    return Holy;
                default:
                    throw new ArgumentOutOfRangeException(nameof(attribute), attribute, null);
            }
        }
    }

    /// <summary><c>UnitCombatResources</c>가 소유하는 데이터와 동작을 캡슐화한다.</summary>
    [Serializable]
    public class UnitCombatResources
    {
        public float CurrentHealth;
        public float CurrentShield;
        public float DirectShield;
    }

    /// <summary><c>UnitCombatState</c>의 변경 가능한 런타임 상태를 보관한다.</summary>
    public class UnitCombatState
    {
        public UnitIdentity Identity = new UnitIdentity();
        public UnitCombatStats Stats = new UnitCombatStats();
        public UnitDefenseStats Defenses = new UnitDefenseStats();
        public UnitCombatResources Resources = new UnitCombatResources();
        public UnitSkills Skills = new UnitSkills();
        public SkillExecutionState SkillState = new SkillExecutionState();
        public SingleChargeState ActiveCharge;
        public UnitStatusCollection Statuses = new UnitStatusCollection();
        public bool IsBoss;
        public bool AutoAttackEnabled = true;
        public bool AutoSkillEnabled = true;

        public bool IsNexus => Identity.Role == UnitRole.Nexus;

        /// <summary><c>TotalShield</c>를 반환한다.</summary>
        public float GetTotalShield()
        {
            var directShield = Mathf.Max(0f, Resources.DirectShield);
            var statusShield = Mathf.Max(0f, Statuses.GetTotalShieldAmount());
            return Mathf.Round(Mathf.Max(0f, directShield + statusShield));
        }

        /// <summary><c>Shield</c>를 현재 원본 상태와 동기화한다.</summary>
        public void SyncShield()
        {
            Resources.DirectShield = Mathf.Round(Mathf.Max(0f, Resources.DirectShield));
            Resources.CurrentShield = GetTotalShield();
        }
    }

    /// <summary><c>EnemyCombatState</c>의 변경 가능한 런타임 상태를 보관한다.</summary>
    public class EnemyCombatState : UnitCombatState
    {
        public DamageAttribute Attribute;
        public float NexusDamage = 1f;
    }
}
