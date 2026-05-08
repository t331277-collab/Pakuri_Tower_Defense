using System.Collections.Generic;
using Pakuri.Data;
using Pakuri.Run;
using UnityEngine;

namespace Pakuri.Combat
{
    public sealed class CombatUnitRuntime : MonoBehaviour
    {
        public CombatRuntimeController Owner { get; private set; }
        public MonsterDefinition Monster { get; private set; }
        public RunSession.RunMonsterState State { get; private set; }
        public SpriteRenderer Renderer { get; private set; }
        public TextMesh Label { get; private set; }
        public bool UsesSceneSlot { get; private set; }
        public int PartyIndex { get; private set; }
        public float MaxHealth { get; set; }
        public float CurrentHealth { get; set; }
        public float BaseDamage { get; set; }
        public float PowerStat { get; set; }
        public GameObject GameObject => gameObject;
        public Transform Transform => transform;
        public List<CombatSkillRuntime> Skills { get; } = new List<CombatSkillRuntime>();

        public void ConfigureManifested(
            CombatRuntimeController owner,
            MonsterDefinition monster,
            RunSession.RunMonsterState state,
            SpriteRenderer renderer,
            TextMesh label,
            bool usesSceneSlot,
            int partyIndex)
        {
            if (Monster != monster || State != state)
            {
                Skills.Clear();
            }

            Owner = owner;
            Monster = monster;
            State = state;
            Renderer = renderer;
            Label = label;
            UsesSceneSlot = usesSceneSlot;
            PartyIndex = partyIndex;
        }

        public void ConfigureSelected(
            CombatRuntimeController owner,
            MonsterDefinition monster,
            RunSession.RunMonsterState state,
            SpriteRenderer renderer,
            TextMesh label)
        {
            ConfigureManifested(owner, monster, state, renderer, label, true, 0);
        }

        public void SyncStats(float maxHealth, float currentHealth, float baseDamage, float powerStat)
        {
            MaxHealth = Mathf.Max(1f, maxHealth);
            CurrentHealth = Mathf.Clamp(currentHealth, 0f, MaxHealth);
            BaseDamage = Mathf.Max(1f, baseDamage);
            PowerStat = Mathf.Max(0f, powerStat);
        }

        public void ConfigureStatsFromDefinition()
        {
            if (Monster == null)
            {
                MaxHealth = 1f;
                CurrentHealth = 1f;
                BaseDamage = 1f;
                PowerStat = 0f;
                return;
            }

            MaxHealth = Mathf.Max(1f, Monster.MaxHealth + (State != null ? State.MaxHealthBonus : 0f));
            CurrentHealth = MaxHealth;
            BaseDamage = Mathf.Max(1f, Monster.BaseDamage);
            PowerStat = Mathf.Max(0f, Monster.PowerStat);
        }

        public void TickManifestedCombat(float elapsed)
        {
            if (Owner == null || Monster == null || CurrentHealth <= 0f)
            {
                return;
            }

            for (var i = 0; i < Skills.Count; i++)
            {
                Owner.TickManifestedUnitSkill(this, Skills[i], elapsed);
            }
        }

        public void ClearManifestedBinding()
        {
            Owner = null;
            Monster = null;
            State = null;
            Renderer = null;
            Label = null;
            UsesSceneSlot = false;
            PartyIndex = -1;
            MaxHealth = 1f;
            CurrentHealth = 0f;
            BaseDamage = 1f;
            PowerStat = 0f;
            Skills.Clear();
        }
    }
}
