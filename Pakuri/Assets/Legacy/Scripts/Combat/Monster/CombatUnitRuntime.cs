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
        public TextMesh NameLabel { get; private set; }
        public TextMesh HpLabel { get; private set; }
        public SpriteRenderer HpBarFill { get; private set; }
        public SpriteRenderer ShieldBarFill { get; private set; }
        public bool UsesSceneSlot { get; private set; }
        public int PartyIndex { get; private set; }
        public float MaxHealth { get; set; }
        public float CurrentHealth { get; set; }
        public float BaseDamage { get; set; }
        public float PowerStat { get; set; }
        public float RinHowlingTimer { get; set; }
        public int RinWaveAmplificationPhysicalHitCount { get; set; }
        public float RinWaveAmplificationCooldownRemaining { get; set; }
        public float RinFinisherInstinctActionTimer { get; set; }
        public float RinFinisherInstinctCritTimer { get; set; }
        public float RinCollapseAftermathActionTimer { get; set; }
        public float RinCollapseAftermathAttackTimer { get; set; }
        public float SeinFlameBarrageProcCooldownRemaining { get; set; }
        public float ShieldValue { get; set; }
        public float ShieldTimer { get; set; }
        public int ShieldAppliedFrame { get; set; }
        public CombatUnitRuntime ArielShieldSource { get; set; }
        public float ArielBlessingTimer { get; set; }
        public float ArielSanctuaryTimer { get; set; }
        public float ArielSanctuaryProclamationTimer { get; set; }
        public float ArielArchangelShieldValue { get; set; }
        public float ArielArchangelShieldTimer { get; set; }
        public float ArielRadiantShieldBurstDamage { get; set; }
        public float ArielRadiantShieldReflectMultiplier { get; set; }
        public float VegaExterminationPermitTimer { get; set; }
        public float VegaExterminationActionSpeedBonus { get; set; }
        public float VegaExterminationAttackBonus { get; set; }
        public float VegaBlackLedgerAreaChargeCooldownRemaining { get; set; }
        public GameObject GameObject => gameObject;
        public Transform Transform => transform;
        public List<CombatSkillRuntime> Skills { get; } = new List<CombatSkillRuntime>();

        public void ConfigureManifested(
            CombatRuntimeController owner,
            MonsterDefinition monster,
            RunSession.RunMonsterState state,
            SpriteRenderer renderer,
            TextMesh label,
            TextMesh nameLabel,
            TextMesh hpLabel,
            SpriteRenderer hpBarFill,
            SpriteRenderer shieldBarFill,
            bool usesSceneSlot,
            int partyIndex)
        {
            var bindingChanged = Monster != monster || State != state;
            if (bindingChanged)
            {
                Skills.Clear();
            }

            Owner = owner;
            Monster = monster;
            State = state;
            Renderer = renderer;
            Label = label;
            NameLabel = nameLabel;
            HpLabel = hpLabel;
            HpBarFill = hpBarFill;
            ShieldBarFill = shieldBarFill;
            UsesSceneSlot = usesSceneSlot;
            PartyIndex = partyIndex;
            if (bindingChanged)
            {
                RinHowlingTimer = 0f;
                RinWaveAmplificationPhysicalHitCount = 0;
                RinWaveAmplificationCooldownRemaining = 0f;
                RinFinisherInstinctActionTimer = 0f;
                RinFinisherInstinctCritTimer = 0f;
                RinCollapseAftermathActionTimer = 0f;
                RinCollapseAftermathAttackTimer = 0f;
                SeinFlameBarrageProcCooldownRemaining = 0f;
                ShieldValue = 0f;
                ShieldTimer = 0f;
                ShieldAppliedFrame = -1;
                ArielShieldSource = null;
                ArielBlessingTimer = 0f;
                ArielSanctuaryTimer = 0f;
                ArielSanctuaryProclamationTimer = 0f;
                ArielArchangelShieldValue = 0f;
                ArielArchangelShieldTimer = 0f;
                ArielRadiantShieldBurstDamage = 0f;
                ArielRadiantShieldReflectMultiplier = 0f;
                VegaExterminationPermitTimer = 0f;
                VegaExterminationActionSpeedBonus = 0f;
                VegaExterminationAttackBonus = 0f;
                VegaBlackLedgerAreaChargeCooldownRemaining = 0f;
            }
        }

        public void ConfigureSelected(
            CombatRuntimeController owner,
            MonsterDefinition monster,
            RunSession.RunMonsterState state,
            SpriteRenderer renderer,
            TextMesh label)
        {
            ConfigureManifested(owner, monster, state, renderer, label, null, label, null, null, true, 0);
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

            var delta = Mathf.Max(0f, elapsed);
            RinHowlingTimer = Mathf.Max(0f, RinHowlingTimer - delta);
            RinWaveAmplificationCooldownRemaining = Mathf.Max(0f, RinWaveAmplificationCooldownRemaining - delta);
            RinFinisherInstinctActionTimer = Mathf.Max(0f, RinFinisherInstinctActionTimer - delta);
            RinFinisherInstinctCritTimer = Mathf.Max(0f, RinFinisherInstinctCritTimer - delta);
            RinCollapseAftermathActionTimer = Mathf.Max(0f, RinCollapseAftermathActionTimer - delta);
            RinCollapseAftermathAttackTimer = Mathf.Max(0f, RinCollapseAftermathAttackTimer - delta);
            SeinFlameBarrageProcCooldownRemaining = Mathf.Max(0f, SeinFlameBarrageProcCooldownRemaining - delta);
            if (ShieldAppliedFrame != Time.frameCount)
            {
                ShieldTimer = Mathf.Max(0f, ShieldTimer - delta);
            }

            if (Mathf.Approximately(ShieldTimer, 0f))
            {
                ShieldValue = 0f;
                ShieldAppliedFrame = -1;
                ArielShieldSource = null;
                ArielRadiantShieldBurstDamage = 0f;
                ArielRadiantShieldReflectMultiplier = 0f;
            }

            ArielBlessingTimer = Mathf.Max(0f, ArielBlessingTimer - delta);
            ArielSanctuaryTimer = Mathf.Max(0f, ArielSanctuaryTimer - delta);
            ArielSanctuaryProclamationTimer = Mathf.Max(0f, ArielSanctuaryProclamationTimer - delta);
            ArielArchangelShieldTimer = Mathf.Max(0f, ArielArchangelShieldTimer - delta);
            if (Mathf.Approximately(ArielArchangelShieldTimer, 0f))
            {
                ArielArchangelShieldValue = 0f;
            }

            VegaExterminationPermitTimer = Mathf.Max(0f, VegaExterminationPermitTimer - delta);
            if (Mathf.Approximately(VegaExterminationPermitTimer, 0f))
            {
                VegaExterminationActionSpeedBonus = 0f;
                VegaExterminationAttackBonus = 0f;
            }

            VegaBlackLedgerAreaChargeCooldownRemaining = Mathf.Max(0f, VegaBlackLedgerAreaChargeCooldownRemaining - delta);
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
            NameLabel = null;
            HpLabel = null;
            HpBarFill = null;
            ShieldBarFill = null;
            UsesSceneSlot = false;
            PartyIndex = -1;
            MaxHealth = 1f;
            CurrentHealth = 0f;
            BaseDamage = 1f;
            PowerStat = 0f;
            RinHowlingTimer = 0f;
            RinWaveAmplificationPhysicalHitCount = 0;
            RinWaveAmplificationCooldownRemaining = 0f;
            RinFinisherInstinctActionTimer = 0f;
            RinFinisherInstinctCritTimer = 0f;
            RinCollapseAftermathActionTimer = 0f;
            RinCollapseAftermathAttackTimer = 0f;
            SeinFlameBarrageProcCooldownRemaining = 0f;
            ShieldValue = 0f;
            ShieldTimer = 0f;
            ShieldAppliedFrame = -1;
            ArielShieldSource = null;
            ArielBlessingTimer = 0f;
            ArielSanctuaryTimer = 0f;
            ArielSanctuaryProclamationTimer = 0f;
            ArielArchangelShieldValue = 0f;
            ArielArchangelShieldTimer = 0f;
            ArielRadiantShieldBurstDamage = 0f;
            ArielRadiantShieldReflectMultiplier = 0f;
            VegaExterminationPermitTimer = 0f;
            VegaExterminationActionSpeedBonus = 0f;
            VegaExterminationAttackBonus = 0f;
            VegaBlackLedgerAreaChargeCooldownRemaining = 0f;
            Skills.Clear();
        }
    }
}
