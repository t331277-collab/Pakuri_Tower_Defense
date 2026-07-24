using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Pakuri.NewCore.Combat.Status;
using Pakuri.NewCore.Definitions.Status;
using Pakuri.NewCore.Definitions.Units;

namespace Pakuri.NewCore.Units.Models
{
    public abstract class UnitBaseModel
    {
        private readonly List<StatusEffect> statusEffects = new List<StatusEffect>();
        private readonly List<RuntimeCombatModifier> runtimeModifiers =
            new List<RuntimeCombatModifier>();
        private readonly List<ShieldLayer> shieldLayers =
            new List<ShieldLayer>();
        private readonly IReadOnlyList<StatusEffect> readOnlyStatusEffects;
        private readonly IReadOnlyList<RuntimeCombatModifier> readOnlyRuntimeModifiers;

        protected UnitBaseModel(UnitDefinition definition, float maximumHealth)
        {
            if (!IsFinitePositive(maximumHealth))
            {
                throw new ArgumentOutOfRangeException(nameof(maximumHealth));
            }

            Definition = definition;
            MaximumHealth = maximumHealth;
            CurrentHealth = maximumHealth;
            readOnlyStatusEffects = new ReadOnlyCollection<StatusEffect>(statusEffects);
            readOnlyRuntimeModifiers =
                new ReadOnlyCollection<RuntimeCombatModifier>(runtimeModifiers);
        }

        public UnitDefinition Definition { get; }

        public float MaximumHealth { get; }

        public float CurrentHealth { get; private set; }

        public float CurrentShield { get; private set; }

        public bool IsAlive => CurrentHealth > 0f;

        public CombatVector2 Position { get; private set; }

        public IReadOnlyList<StatusEffect> StatusEffects => readOnlyStatusEffects;

        public IReadOnlyList<RuntimeCombatModifier> RuntimeModifiers =>
            readOnlyRuntimeModifiers;

        public event Action<StatusEffect> StatusExpired;

        public bool CanMove => ResolveStatusPermission(effect => effect.Definition.can_move);

        public bool CanAct => ResolveStatusPermission(effect => effect.Definition.can_act);

        public bool CanUseSpecialSkill =>
            ResolveStatusPermission(effect => effect.Definition.can_use_special_skill);

        public float ActionSpeedMultiplier =>
            Math.Max(
                0f,
                1f
                + ResolveStatusValue(
                    effect => effect.Definition.action_speed_bonus_per_stack)
                + ResolveRuntimeModifier("StatusActionSpeedBonus"));

        public float MoveSpeedMultiplier =>
            Math.Max(
                0f,
                1f
                + ResolveStatusValue(
                    effect => effect.Definition.move_speed_bonus_per_stack)
                + ResolveRuntimeModifier("StatusMoveSpeedBonus"));

        public void SetPosition(CombatVector2 position)
        {
            Position = position;
        }

        public float ApplyDamage(float amount)
        {
            return ApplyDamage(amount, null);
        }

        public float ApplyDamage(
            float amount,
            Action<UnitBaseModel, string, float> shieldAbsorbed)
        {
            ValidateNonNegativeFinite(amount, nameof(amount));
            if (amount == 0f || !IsAlive)
            {
                return 0f;
            }

            float absorbed = Math.Min(CurrentShield, amount);
            CurrentShield -= absorbed;
            float shieldRemaining = absorbed;
            for (int index = shieldLayers.Count - 1;
                index >= 0 && shieldRemaining > 0f;
                index--)
            {
                ShieldLayer layer = shieldLayers[index];
                float layerAbsorbed = Math.Min(layer.Amount, shieldRemaining);
                layer.Amount -= layerAbsorbed;
                shieldRemaining -= layerAbsorbed;
                shieldAbsorbed?.Invoke(
                    layer.Source,
                    layer.SkillId,
                    layerAbsorbed);
                if (layer.Amount <= 0.00001f)
                {
                    shieldLayers.RemoveAt(index);
                }
            }
            float healthDamage = Math.Min(CurrentHealth, amount - absorbed);
            CurrentHealth -= healthDamage;
            return healthDamage;
        }

        public float Heal(float amount)
        {
            ValidateNonNegativeFinite(amount, nameof(amount));
            if (amount == 0f || !IsAlive)
            {
                return 0f;
            }

            float applied = Math.Min(MaximumHealth - CurrentHealth, amount);
            CurrentHealth += applied;
            return applied;
        }

        public bool TryAddShield(float amount)
        {
            return TryAddShield(amount, null, null);
        }

        public bool TryAddShield(
            float amount,
            UnitBaseModel source,
            string skillId)
        {
            ValidateNonNegativeFinite(amount, nameof(amount));
            if (amount == 0f || !IsAlive)
            {
                return false;
            }

            float updated = CurrentShield + amount;
            if (float.IsInfinity(updated))
            {
                throw new OverflowException("Shield value overflowed.");
            }

            CurrentShield = updated;
            shieldLayers.Add(new ShieldLayer(source, skillId, amount));
            return true;
        }

        public void ClearShield()
        {
            CurrentShield = 0f;
            shieldLayers.Clear();
        }

        public float RemoveShield(UnitBaseModel source, string skillId)
        {
            float removed = 0f;
            for (int index = shieldLayers.Count - 1; index >= 0; index--)
            {
                ShieldLayer layer = shieldLayers[index];
                if (ReferenceEquals(layer.Source, source)
                    && string.Equals(
                        layer.SkillId,
                        skillId,
                        StringComparison.Ordinal))
                {
                    removed += layer.Amount;
                    shieldLayers.RemoveAt(index);
                }
            }
            CurrentShield = Math.Max(0f, CurrentShield - removed);
            return removed;
        }

        public StatusEffect ApplyStatus(
            StatusDefinition definition,
            UnitBaseModel applyingUnit,
            float? durationSeconds = null,
            int? stackAmount = null,
            string sourceSkillId = null)
        {
            if (definition == null)
            {
                throw new ArgumentNullException(nameof(definition));
            }

            if (applyingUnit == null)
            {
                throw new ArgumentNullException(nameof(applyingUnit));
            }

            for (int index = 0; index < statusEffects.Count; index++)
            {
                StatusEffect existing = statusEffects[index];
                if (ReferenceEquals(existing.Definition, definition)
                    && ReferenceEquals(existing.ApplyingUnit, applyingUnit)
                    && string.Equals(
                        existing.SourceSkillId,
                        sourceSkillId,
                        StringComparison.Ordinal))
                {
                    existing.Refresh(durationSeconds, stackAmount);
                    return existing;
                }
            }

            StatusEffect effect =
                new StatusEffect(
                    definition,
                    applyingUnit,
                    this,
                    durationSeconds,
                    stackAmount,
                    sourceSkillId);
            statusEffects.Add(effect);
            return effect;
        }

        public bool RemoveStatus(StatusEffect effect)
        {
            return effect != null && statusEffects.Remove(effect);
        }

        public int ConsumeStatus(string statusId, float ratio)
        {
            if (string.IsNullOrEmpty(statusId))
            {
                return 0;
            }
            if (ratio < 0f
                || ratio > 1f
                || float.IsNaN(ratio)
                || float.IsInfinity(ratio))
            {
                throw new ArgumentOutOfRangeException(nameof(ratio));
            }
            int removed = 0;
            for (int index = statusEffects.Count - 1; index >= 0; index--)
            {
                StatusEffect effect = statusEffects[index];
                if (effect.Definition.status_effect_id != statusId) continue;
                int amount = (int)Math.Ceiling(effect.CurrentStacks * ratio);
                removed += effect.RemoveStacks(amount);
                if (effect.CurrentStacks <= 0)
                {
                    statusEffects.RemoveAt(index);
                }
            }
            return removed;
        }

        public RuntimeCombatModifier AddRuntimeModifier(
            string kind,
            float value,
            string filter,
            UnitBaseModel source,
            float durationSeconds,
            string secondaryFilter = null)
        {
            RuntimeCombatModifier modifier = new RuntimeCombatModifier(
                kind,
                value,
                filter,
                secondaryFilter,
                source,
                durationSeconds);
            runtimeModifiers.Add(modifier);
            return modifier;
        }

        public float ResolveRuntimeModifier(string kind, string filter = null)
        {
            float result = 0f;
            for (int index = 0; index < runtimeModifiers.Count; index++)
            {
                RuntimeCombatModifier modifier = runtimeModifiers[index];
                if (modifier.Kind == kind
                    && (string.IsNullOrEmpty(modifier.Filter)
                        || string.IsNullOrEmpty(filter)
                        || string.Equals(
                            modifier.Filter,
                            filter,
                            StringComparison.Ordinal)))
                {
                    result += modifier.Value;
                }
            }
            return result;
        }

        public void TickStatusEffects(float deltaTime)
        {
            ValidateNonNegativeFinite(deltaTime, nameof(deltaTime));
            for (int index = statusEffects.Count - 1; index >= 0; index--)
            {
                StatusEffect effect = statusEffects[index];
                effect.Tick(deltaTime);
                if (effect.IsExpired)
                {
                    statusEffects.RemoveAt(index);
                    StatusExpired?.Invoke(effect);
                }
            }
            for (int index = runtimeModifiers.Count - 1; index >= 0; index--)
            {
                runtimeModifiers[index].Tick(deltaTime);
                if (runtimeModifiers[index].IsExpired)
                {
                    runtimeModifiers.RemoveAt(index);
                }
            }
        }

        public void ClearStatusEffects()
        {
            statusEffects.Clear();
            runtimeModifiers.Clear();
        }

        protected void ResetVitalsAndStatuses()
        {
            CurrentHealth = MaximumHealth;
            CurrentShield = 0f;
            shieldLayers.Clear();
            statusEffects.Clear();
            runtimeModifiers.Clear();
        }

        private sealed class ShieldLayer
        {
            public ShieldLayer(
                UnitBaseModel source,
                string skillId,
                float amount)
            {
                Source = source;
                SkillId = skillId;
                Amount = amount;
            }

            public UnitBaseModel Source { get; }

            public string SkillId { get; }

            public float Amount { get; set; }
        }

        private bool ResolveStatusPermission(Func<StatusEffect, bool?> selector)
        {
            for (int index = 0; index < statusEffects.Count; index++)
            {
                if (selector(statusEffects[index]) == false)
                {
                    return false;
                }
            }

            return true;
        }

        private float ResolveStatusValue(
            Func<StatusEffect, float?> selector)
        {
            float value = 0f;
            for (int index = 0; index < statusEffects.Count; index++)
            {
                value += (selector(statusEffects[index]) ?? 0f)
                    * statusEffects[index].CurrentStacks;
            }
            return value;
        }

        private static bool IsFinitePositive(float value)
        {
            return value > 0f && !float.IsNaN(value) && !float.IsInfinity(value);
        }

        private static void ValidateNonNegativeFinite(float value, string parameterName)
        {
            if (value < 0f || float.IsNaN(value) || float.IsInfinity(value))
            {
                throw new ArgumentOutOfRangeException(parameterName);
            }
        }
    }
}
