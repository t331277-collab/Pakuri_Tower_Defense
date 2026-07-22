using System;
using System.Collections.Generic;
using Pakuri.Combat;
using UnityEngine;

/*
 * 모든 전투 유닛의 식별, 능력치, 자원, 방어력과 스킬 진행 상태를 한곳에 정의한다.
 * 적 전용 상태만 EnemyCombatState에 추가하고 몬스터와 Nexus는 역할 값으로 구분한다.
 */
namespace Pakuri.InGame
{
    public enum UnitSide
    {
        Player,
        Enemy
    }

    public enum UnitRole
    {
        Monster,
        Enemy,
        Nexus
    }

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

    [Serializable]
    public class UnitDefenseStats
    {
        public float Physical;
        public float Fire;
        public float Lightning;
        public float Ice;
        public float Darkness;
        public float Holy;

        /*
         * 피해 속성에 해당하는 방어력을 반환한다.
         */
        public float Get(DamageAttribute attribute /* 피해 속성 */)
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

    [Serializable]
    public class UnitCombatResources
    {
        public float CurrentHealth;
        public float CurrentShield;
        public float DirectShield;
    }

    public class UnitCombatState
    {
        public UnitIdentity Identity = new UnitIdentity();
        public UnitCombatStats Stats = new UnitCombatStats();
        public UnitDefenseStats Defenses = new UnitDefenseStats();
        public UnitCombatResources Resources = new UnitCombatResources();
        public UnitSkills Skills = new UnitSkills();
        public SingleChargeState ActiveCharge;
        public UnitStatusCollection Statuses = new UnitStatusCollection();
        public bool IsBoss;
        public bool AutoAttackEnabled = true;
        public bool AutoSkillEnabled = true;

        public bool IsNexus => Identity.Role == UnitRole.Nexus;

        /*
         * 직접 보호막과 활성 상태 보호막의 총량을 반환한다.
         */
        public float GetTotalShield()
        {
            var directShield = Mathf.Max(0f, Resources.DirectShield);
            var statusShield = Mathf.Max(0f, Statuses.GetTotalShieldAmount());
            return Mathf.Round(Mathf.Max(0f, directShield + statusShield));
        }

        /*
         * 직접 보호막과 상태 보호막의 합계를 현재 자원 값에 반영한다.
         */
        public void SyncShield()
        {
            Resources.DirectShield = Mathf.Round(Mathf.Max(0f, Resources.DirectShield));
            Resources.CurrentShield = GetTotalShield();
        }
    }

    public class EnemyCombatState : UnitCombatState
    {
        public DamageAttribute Attribute;
        public float NexusDamage = 1f;
        public float PassivePhysicalDamageMultiplier = 1f;
        public float PassiveFireDamageMultiplier = 1f;
        public float PassiveLightningDamageMultiplier = 1f;
        public float PassiveIceDamageMultiplier = 1f;
        public float PassiveDarknessDamageMultiplier = 1f;
        public float PassiveHolyDamageMultiplier = 1f;
        public float PassiveIncomingDamageMultiplier = 1f;
        public float PassiveHealingMultiplier = 1f;
    }
}
