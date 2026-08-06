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

    public enum UnitSide
    {
        Player,
        Enemy
    }

    public enum UnitRole
    {
        Monster,
        Enemy,
        Nexus,
        Summon
    }

    [Serializable]
    public class UnitIdentity
    {
        public string UnitName;
        public string DefinitionName;
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

    [Serializable]
    public class UnitCombatResources
    {
        public float CurrentHealth;
        public float CurrentShield;
        public float DirectShield;
    }

    /// UnitCombatState의 변경 가능한 런타임 상태를 보관한다.
    public class UnitCombatState
    {
        public UnitIdentity Identity = new UnitIdentity();
        public UnitCombatStats Stats = new UnitCombatStats();
        public UnitDefenseStats Defenses = new UnitDefenseStats();
        public UnitCombatResources Resources = new UnitCombatResources();
        public UnitSkills Skills = new UnitSkills();
        public UnitSkills SkillState = new UnitSkills();
        public ArtifactState Artifacts = new ArtifactState();
        public UnitStatusCollection Statuses = new UnitStatusCollection();
        public DamageAttribute? SkillDamageAttributeOverride;
        public bool IsBoss;
        public bool AutoAttackEnabled = true;
        public bool AutoSkillEnabled = true;

        public bool IsNexus => Identity.Role == UnitRole.Nexus;

        /// TotalShield를 반환한다.
        public float GetTotalShield()
        {
            var directShield = Mathf.Max(0f, Resources.DirectShield);
            var statusShield = Mathf.Max(0f, Statuses.GetTotalShieldAmount());
            return Mathf.Round(Mathf.Max(0f, directShield + statusShield));
        }

        /// Shield를 현재 원본 상태와 동기화한다.
        public void SyncShield()
        {
            Resources.DirectShield = Mathf.Round(Mathf.Max(0f, Resources.DirectShield));
            Resources.CurrentShield = GetTotalShield();
        }
    }

    /// EnemyCombatState의 변경 가능한 런타임 상태를 보관한다.
    public class EnemyCombatState : UnitCombatState
    {
        public DamageAttribute Attribute;
        public float NexusDamage = 1f;
    }
}
