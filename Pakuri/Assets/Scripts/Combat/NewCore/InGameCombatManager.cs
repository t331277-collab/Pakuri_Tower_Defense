using System;
using System.Collections.Generic;
using Pakuri.NewCore.Combat.Status;
using Pakuri.NewCore.Definitions.Skills;
using Pakuri.NewCore.Definitions.Status;
using Pakuri.NewCore.Definitions.Units;
using Pakuri.NewCore.Combat.Skills.Execution;
using Pakuri.NewCore.Units.Models;

namespace Pakuri.NewCore.Combat
{
    public readonly struct CombatResult
    {
        public CombatResult(
            UnitBaseModel source,
            UnitBaseModel target,
            string skillId,
            float healthChanged,
            float shieldChanged,
            bool critical,
            bool defeated)
        {
            Source = source;
            Target = target;
            SkillId = skillId;
            HealthChanged = healthChanged;
            ShieldChanged = shieldChanged;
            IsCritical = critical;
            IsDefeated = defeated;
        }

        public UnitBaseModel Source { get; }

        public UnitBaseModel Target { get; }

        public string SkillId { get; }

        public float HealthChanged { get; }

        public float ShieldChanged { get; }

        public float DamageAmount =>
            Math.Max(0f, -HealthChanged - ShieldChanged);

        public bool IsCritical { get; }

        public bool IsDefeated { get; }
    }

    public sealed class InGameCombatManager
    {
        private readonly Func<float> randomValue;
        private readonly SkillExecutionRuntime executionRuntime;
        private readonly SkillTriggerDispatcher triggers;
        private IReadOnlyList<UnitBaseModel> registeredUnits =
            Array.Empty<UnitBaseModel>();
        private readonly HashSet<UnitBaseModel> observedUnits =
            new HashSet<UnitBaseModel>();
        private readonly Dictionary<UnitBaseModel, int> outgoingHitCounts =
            new Dictionary<UnitBaseModel, int>();

        public InGameCombatManager(
            Func<float> randomValue,
            SkillExecutionRuntime executionRuntime)
        {
            this.randomValue = randomValue ?? throw new ArgumentNullException(nameof(randomValue));
            this.executionRuntime =
                executionRuntime ?? throw new ArgumentNullException(nameof(executionRuntime));
            triggers = executionRuntime.Triggers;
        }

        public event Action<CombatResult> DamageApplied;

        public event Action<CombatResult> HealingApplied;

        public event Action<CombatResult> ShieldApplied;

        public event Action<UnitBaseModel, UnitBaseModel, StatusDefinition> StatusApplied;

        public event Action<UnitBaseModel, SkillDefinition> SkillActivated;

        public event Action<UnitBaseModel> UnitDefeated;

        public bool TryExecuteSkill(SkillExecutionRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            ObserveUnits(request.RegisteredUnits);
            registeredUnits = request.RegisteredUnits;
            return executionRuntime.TryExecute(this, request);
        }

        public void TickTriggers(float deltaTime)
        {
            triggers.Tick(deltaTime);
        }

        public void ApplyPassiveChanges(
            IReadOnlyList<UnitBaseModel> units)
        {
            executionRuntime.ApplyPassives(this, units);
        }

        public void EndCombat()
        {
            foreach (UnitBaseModel unit in observedUnits)
            {
                unit.StatusExpired -= OnStatusExpired;
            }
            observedUnits.Clear();
            outgoingHitCounts.Clear();
            registeredUnits = Array.Empty<UnitBaseModel>();
            executionRuntime.ResetCombat();
        }

        public int NotifyCombatStart(
            UnitBaseModel owner,
            IReadOnlyList<UnitBaseModel> units)
        {
            ObserveUnits(units);
            registeredUnits = units;
            return triggers.Dispatch(
                "CombatStart",
                owner,
                null,
                null,
                units,
                this);
        }

        public void NotifyMagazineLastProjectileHit(
            UnitBaseModel owner,
            SkillDefinition skill,
            UnitBaseModel target)
        {
            triggers.Dispatch(
                "OnMagazineLastProjectileHit",
                owner,
                skill,
                target,
                registeredUnits,
                this);
        }

        public void NotifyShieldExpired(
            UnitBaseModel owner,
            SkillDefinition skill,
            UnitBaseModel target,
            float expiredAmount)
        {
            triggers.Dispatch(
                "OnShieldExpire",
                owner,
                skill,
                target,
                registeredUnits,
                this,
                eventShieldApplied: expiredAmount);
        }

        public CombatResult ApplySkillDamage(
            UnitBaseModel source,
            UnitBaseModel target,
            SkillDefinition skill,
            float damageMultiplier,
            float criticalChanceBonus = 0f,
            float criticalDamageBonus = 0f,
            bool eventExecuted = false,
            IReadOnlyCollection<string> triggerAncestry = null,
            float baseDamageBonus = 0f,
            float attackPowerCoefficientBonus = 0f)
        {
            RequireLiving(source, nameof(source));
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            if (skill == null)
            {
                throw new ArgumentNullException(nameof(skill));
            }

            ValidateNonNegativeFinite(damageMultiplier, nameof(damageMultiplier));
            ValidateNonNegativeFinite(baseDamageBonus, nameof(baseDamageBonus));
            ValidateNonNegativeFinite(
                attackPowerCoefficientBonus,
                nameof(attackPowerCoefficientBonus));
            float rawDamage = (
                CalculateRawValue(source, skill)
                + baseDamageBonus
                + (ResolveAttackPower(source)
                    * attackPowerCoefficientBonus))
                * damageMultiplier;
            rawDamage *= Math.Max(
                0f,
                1f + source.ResolveRuntimeModifier(
                    "StatusDamageBonusRate",
                    skill.attribute)
                + ResolveEnemyPassive(
                    source,
                    "DamageUp",
                    skill.attribute));

            float defense = Math.Max(0f, ResolveDefense(target, skill.attribute));
            float finalDamage = rawDamage * (100f / (100f + defense));
            bool critical = false;
            if (ReadBool(skill, "critical_allowed"))
            {
                float chance = Clamp01(
                    ResolveCriticalChance(source)
                    + criticalChanceBonus
                    - ResolveCriticalResistance(target));
                critical = randomValue() < chance;
                if (critical)
                {
                    finalDamage *= Math.Max(
                        0f,
                        ResolveCriticalDamage(source) + criticalDamageBonus);
                    finalDamage *= Math.Max(
                        0f,
                        1f + target.ResolveRuntimeModifier(
                            "StatusCriticalDamageTakenBonus"));
                }
            }

            finalDamage *= Math.Max(
                0f,
                1f + ResolveDamageTakenBonus(
                    source,
                    target,
                    skill.runtime_kind,
                    skill.attribute));
            finalDamage *= Math.Max(
                0f,
                1f - ResolveEnemyPassive(
                    target,
                    "IncomingDamageDown",
                    skill.attribute));
            finalDamage = (float)Math.Round(Math.Max(0f, finalDamage));

            float beforeShield = target.CurrentShield;
            List<UnitBaseModel> absorbedShieldOwners =
                new List<UnitBaseModel>();
            List<float> absorbedShieldAmounts = new List<float>();
            float appliedHealthDamage = target.ApplyDamage(
                finalDamage,
                (shieldOwner, shieldSkillId, absorbedAmount) =>
                {
                    absorbedShieldOwners.Add(shieldOwner);
                    absorbedShieldAmounts.Add(absorbedAmount);
                });
            float appliedDamage = appliedHealthDamage
                + Math.Max(0f, beforeShield - target.CurrentShield);
            for (int statusIndex = 0;
                statusIndex < target.StatusEffects.Count;
                statusIndex++)
            {
                target.StatusEffects[statusIndex].TrackIncomingDamage(
                    NormalizeAttribute(skill.attribute),
                    appliedDamage);
            }
            CombatResult result = new CombatResult(
                source,
                target,
                skill.skill_id,
                -appliedHealthDamage,
                target.CurrentShield - beforeShield,
                critical,
                !target.IsAlive);
            if (appliedHealthDamage > 0f || beforeShield != target.CurrentShield)
            {
                outgoingHitCounts.TryGetValue(source, out int hitCount);
                outgoingHitCounts[source] = hitCount + 1;
                DamageApplied?.Invoke(result);
                triggers.Dispatch(
                    "OnOutgoingDamage",
                    source,
                    skill,
                    target,
                    registeredUnits,
                    this,
                    eventAppliedDamage: appliedDamage,
                    trackedAttribute: NormalizeAttribute(skill.attribute),
                    eventExecuted: eventExecuted,
                    triggerAncestry: triggerAncestry);
                for (int shieldIndex = 0;
                    shieldIndex < absorbedShieldOwners.Count;
                    shieldIndex++)
                {
                    triggers.Dispatch(
                        "OnShieldAbsorb",
                        absorbedShieldOwners[shieldIndex] ?? target,
                        skill,
                        source,
                        registeredUnits,
                        this,
                        eventAppliedDamage: appliedDamage,
                        eventShieldAbsorbed:
                            absorbedShieldAmounts[shieldIndex],
                        trackedAttribute: NormalizeAttribute(skill.attribute),
                        eventExecuted: eventExecuted,
                        triggerAncestry: triggerAncestry);
                }
            }

            if (result.IsDefeated)
            {
                UnitDefeated?.Invoke(target);
                triggers.Dispatch(
                    "OnKill",
                    source,
                    skill,
                    target,
                    registeredUnits,
                    this,
                    eventAppliedDamage: appliedDamage,
                    trackedAttribute: NormalizeAttribute(skill.attribute),
                    eventExecuted: true,
                    triggerAncestry: triggerAncestry);
            }
            if (target.IsAlive)
            {
                ApplyOutgoingAdditionalDamage(
                    source,
                    target,
                    skill,
                    rawDamage);
            }

            return result;
        }

        public int GetOutgoingHitCount(UnitBaseModel source)
        {
            return source != null
                && outgoingHitCounts.TryGetValue(source, out int count)
                    ? count
                    : 0;
        }

        public CombatResult ApplyTriggeredDamage(
            UnitBaseModel source,
            UnitBaseModel target,
            string resultSkillId,
            string attribute,
            float baseDamage,
            float attackPowerCoefficient,
            float spellPowerCoefficient,
            float damageMultiplier,
            IReadOnlyCollection<string> triggerAncestry = null)
        {
            RequireLiving(source, nameof(source));
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            ValidateNonNegativeFinite(baseDamage, nameof(baseDamage));
            ValidateNonNegativeFinite(attackPowerCoefficient, nameof(attackPowerCoefficient));
            ValidateNonNegativeFinite(spellPowerCoefficient, nameof(spellPowerCoefficient));
            ValidateNonNegativeFinite(damageMultiplier, nameof(damageMultiplier));
            float rawDamage = (
                baseDamage
                + (ResolveAttackPower(source) * attackPowerCoefficient)
                + (ResolveSpellPower(source) * spellPowerCoefficient))
                * damageMultiplier;
            rawDamage *= Math.Max(
                0f,
                1f + ResolveEnemyPassive(source, "DamageUp", attribute));
            float defense = Math.Max(0f, ResolveDefense(target, attribute));
            float finalDamage = rawDamage * (100f / (100f + defense));
            finalDamage *= Math.Max(
                0f,
                1f + ResolveDamageTakenBonus(source, target, null, attribute));
            finalDamage *= Math.Max(
                0f,
                1f - ResolveEnemyPassive(
                    target,
                    "IncomingDamageDown",
                    attribute));
            finalDamage = (float)Math.Round(Math.Max(0f, finalDamage));
            float beforeShield = target.CurrentShield;
            List<UnitBaseModel> absorbedShieldOwners =
                new List<UnitBaseModel>();
            List<float> absorbedShieldAmounts = new List<float>();
            float healthDamage = target.ApplyDamage(
                finalDamage,
                (shieldOwner, shieldSkillId, absorbedAmount) =>
                {
                    absorbedShieldOwners.Add(shieldOwner);
                    absorbedShieldAmounts.Add(absorbedAmount);
                });
            float appliedDamage = healthDamage
                + Math.Max(0f, beforeShield - target.CurrentShield);
            for (int statusIndex = 0;
                statusIndex < target.StatusEffects.Count;
                statusIndex++)
            {
                target.StatusEffects[statusIndex].TrackIncomingDamage(
                    NormalizeAttribute(attribute),
                    appliedDamage);
            }
            CombatResult result = new CombatResult(
                source,
                target,
                resultSkillId ?? string.Empty,
                -healthDamage,
                target.CurrentShield - beforeShield,
                false,
                !target.IsAlive);
            if (healthDamage > 0f || result.ShieldChanged < 0f)
            {
                DamageApplied?.Invoke(result);
                for (int shieldIndex = 0;
                    shieldIndex < absorbedShieldOwners.Count;
                    shieldIndex++)
                {
                    triggers.Dispatch(
                        "OnShieldAbsorb",
                        absorbedShieldOwners[shieldIndex] ?? target,
                        null,
                        source,
                        registeredUnits,
                        this,
                        eventAppliedDamage: appliedDamage,
                        eventShieldAbsorbed:
                            absorbedShieldAmounts[shieldIndex],
                        trackedAttribute: NormalizeAttribute(attribute),
                        eventExecuted: false,
                        triggerAncestry: triggerAncestry);
                }
            }
            if (result.IsDefeated)
            {
                UnitDefeated?.Invoke(target);
            }
            return result;
        }

        public CombatResult Heal(
            UnitBaseModel source,
            UnitBaseModel target,
            SkillDefinition skill,
            float amount)
        {
            RequireLiving(source, nameof(source));
            if (target == null || skill == null)
            {
                throw new ArgumentNullException(target == null ? nameof(target) : nameof(skill));
            }

            ValidateNonNegativeFinite(amount, nameof(amount));
            amount *= Math.Max(
                0f,
                1f + ResolveEnemyPassive(source, "HealingUp", skill.attribute));
            float applied = target.Heal(amount);
            CombatResult result =
                new CombatResult(source, target, skill.skill_id, applied, 0f, false, false);
            if (applied > 0f)
            {
                HealingApplied?.Invoke(result);
            }

            return result;
        }

        public CombatResult AddShield(
            UnitBaseModel source,
            UnitBaseModel target,
            SkillDefinition skill,
            float amount)
        {
            RequireLiving(source, nameof(source));
            if (target == null || skill == null)
            {
                throw new ArgumentNullException(target == null ? nameof(target) : nameof(skill));
            }

            ValidateNonNegativeFinite(amount, nameof(amount));
            amount *= Math.Max(
                0f,
                1f + target.ResolveRuntimeModifier(
                    "StatusShieldReceivedBonus"));
            float before = target.CurrentShield;
            target.TryAddShield(amount, source, skill.skill_id);
            CombatResult result = new CombatResult(
                source,
                target,
                skill.skill_id,
                0f,
                target.CurrentShield - before,
                false,
                false);
            if (result.ShieldChanged > 0f)
            {
                ShieldApplied?.Invoke(result);
            }

            return result;
        }

        public void ApplyStatus(
            UnitBaseModel source,
            UnitBaseModel target,
            StatusDefinition status,
            float? duration,
            int? stacks,
            string sourceSkillId = null,
            int? maximumStacks = null)
        {
            RequireLiving(source, nameof(source));
            if (target == null || status == null)
            {
                throw new ArgumentNullException(target == null ? nameof(target) : nameof(status));
            }

            target.ApplyStatus(
                status,
                source,
                duration,
                stacks,
                sourceSkillId,
                maximumStacks);
            StatusApplied?.Invoke(source, target, status);
        }

        public float ApplyNexusDamage(EnemyModel source, NexusModel nexus)
        {
            RequireLiving(source, nameof(source));
            if (nexus == null)
            {
                throw new ArgumentNullException(nameof(nexus));
            }

            float amount = source.EnemyDefinition.nexus_damage
                ?? throw new InvalidOperationException("Enemy has no nexus_damage.");
            bool wasAlive = nexus.IsAlive;
            float applied = nexus.ApplyNexusDamage(amount);
            if (wasAlive && !nexus.IsAlive)
            {
                UnitDefeated?.Invoke(nexus);
            }
            return applied;
        }

        public float CalculateRawValue(UnitBaseModel source, SkillDefinition skill)
        {
            RequireLiving(source, nameof(source));
            if (skill == null)
            {
                throw new ArgumentNullException(nameof(skill));
            }

            float value = ReadFloat(skill, "base_damage");
            value += ReadFloat(skill, "flat_value");
            value += ResolveAttackPower(source) * ReadFloat(skill, "attack_power_coefficient");
            value += ResolveSpellPower(source) * ReadFloat(skill, "spell_power_coefficient");
            return Math.Max(0f, value);
        }

        public void NotifySkillActivated(
            UnitBaseModel source,
            SkillDefinition skill,
            string eventSourceSkillId = null,
            IReadOnlyCollection<string> triggerAncestry = null)
        {
            RequireLiving(source, nameof(source));
            SkillActivated?.Invoke(
                source,
                skill ?? throw new ArgumentNullException(nameof(skill)));
            triggers.Dispatch(
                "OnSkillCast",
                source,
                skill,
                null,
                registeredUnits,
                this,
                eventSourceSkillId: eventSourceSkillId,
                triggerAncestry: triggerAncestry);
        }

        private void ObserveUnits(IReadOnlyList<UnitBaseModel> units)
        {
            for (int index = 0; index < units.Count; index++)
            {
                UnitBaseModel unit = units[index];
                if (unit != null && observedUnits.Add(unit))
                {
                    unit.StatusExpired += OnStatusExpired;
                }
            }
        }

        private void OnStatusExpired(StatusEffect effect)
        {
            triggers.Dispatch(
                "OnStatusExpire",
                effect.ApplyingUnit,
                null,
                effect.AffectedUnit,
                registeredUnits,
                this,
                effect.Definition.status_effect_id,
                trackedIncomingDamage: effect.TrackedIncomingDamage,
                trackedAttribute: effect.LastTrackedAttribute);
            if (string.Equals(
                effect.Definition.effect_type,
                "Shield",
                StringComparison.OrdinalIgnoreCase))
            {
                triggers.Dispatch(
                    "OnShieldExpire",
                    effect.ApplyingUnit,
                    null,
                    effect.AffectedUnit,
                    registeredUnits,
                    this,
                    effect.Definition.status_effect_id,
                    trackedIncomingDamage: effect.TrackedIncomingDamage,
                    trackedAttribute: effect.LastTrackedAttribute);
            }
        }

        private static float ResolveAttackPower(UnitBaseModel unit)
        {
            float value = unit is MonsterModel monster
                ? monster.MonsterDefinition.base_attack_power ?? 0f
                : unit is EnemyModel enemy
                    ? enemy.EnemyDefinition.attack_power ?? 0f
                    : 0f;
            return value * Math.Max(0f, 1f + ResolveStatusSum(
                unit,
                status => status.Definition.attack_power_bonus_per_stack)
                + unit.ResolveRuntimeModifier("StatusAttackPowerBonus"));
        }

        private static float ResolveSpellPower(UnitBaseModel unit)
        {
            float value = unit is MonsterModel monster
                ? monster.MonsterDefinition.base_spell_power ?? 0f
                : unit is EnemyModel enemy
                    ? enemy.EnemyDefinition.spell_power ?? 0f
                    : 0f;
            return value * Math.Max(
                0f,
                1f + unit.ResolveRuntimeModifier("StatusSpellPowerBonus"));
        }

        private static float ResolveCriticalChance(UnitBaseModel unit)
        {
            float value = unit is MonsterModel monster
                ? monster.MonsterDefinition.base_crit_chance ?? 0f
                : unit is EnemyModel enemy
                    ? enemy.EnemyDefinition.crit_chance ?? 0f
                    : 0f;
            return value + unit.ResolveRuntimeModifier("StatusCriticalChanceBonus")
                + ResolveEnemyPassive(unit, "CritChanceUp", null);
        }

        private static float ResolveCriticalDamage(UnitBaseModel unit)
        {
            float value = unit is MonsterModel monster
                ? monster.MonsterDefinition.base_crit_damage ?? 1f
                : unit is EnemyModel enemy
                    ? enemy.EnemyDefinition.crit_damage ?? 1f
                    : 1f;
            return value + unit.ResolveRuntimeModifier("StatusCriticalDamageBonus")
                + ResolveEnemyPassive(unit, "CritDamageUp", null);
        }

        private static float ResolveCriticalResistance(UnitBaseModel unit)
        {
            float value = unit is MonsterModel monster
                ? monster.MonsterDefinition.base_crit_resistance ?? 0f
                : unit is EnemyModel enemy
                    ? enemy.EnemyDefinition.crit_resistance ?? 0f
                    : 0f;
            return value + ResolveStatusSum(
                unit,
                status => status.Definition.critical_resistance_bonus_per_stack)
                + unit.ResolveRuntimeModifier("StatusCriticalResistanceBonus");
        }

        private static float ResolveDamageTakenBonus(
            UnitBaseModel source,
            UnitBaseModel unit,
            string runtimeKind,
            string attribute)
        {
            float value = ResolveStatusSum(
                unit,
                status => status.Definition.damage_taken_bonus_per_stack)
                + unit.ResolveRuntimeModifier(
                    "StatusDamageTakenBonus",
                    runtimeKind)
                + ResolveStatusSum(
                    unit,
                    status => status.Definition
                        .element_damage_taken_bonus_per_stack)
                + unit.ResolveRuntimeModifier(
                    "StatusElementDamageTakenBonus",
                    attribute);
            for (int index = 0; index < unit.RuntimeModifiers.Count; index++)
            {
                RuntimeCombatModifier modifier = unit.RuntimeModifiers[index];
                if (modifier.Kind == "StatusConditionalDamageTakenBonus"
                    && HasStatus(source, modifier.Filter))
                {
                    value += modifier.Value;
                }
            }
            return value;
        }

        private void ApplyOutgoingAdditionalDamage(
            UnitBaseModel source,
            UnitBaseModel target,
            SkillDefinition skill,
            float rawDamage)
        {
            for (int index = 0; index < source.RuntimeModifiers.Count; index++)
            {
                RuntimeCombatModifier modifier = source.RuntimeModifiers[index];
                if (modifier.Kind != "StatusOutgoingAdditionalDamage"
                    || (!string.IsNullOrEmpty(modifier.Filter)
                        && modifier.Filter != skill.attribute))
                {
                    continue;
                }
                ApplyTriggeredDamage(
                    source,
                    target,
                    skill.skill_id + ":status-additional",
                    string.IsNullOrEmpty(modifier.SecondaryFilter)
                        ? skill.attribute
                        : modifier.SecondaryFilter,
                    rawDamage,
                    0f,
                    0f,
                    Math.Max(0f, modifier.Value));
            }
        }

        private static bool HasStatus(
            UnitBaseModel unit,
            string statusId)
        {
            for (int index = 0; index < unit.StatusEffects.Count; index++)
            {
                if (unit.StatusEffects[index].Definition.status_effect_id
                    == statusId)
                {
                    return true;
                }
            }
            return false;
        }

        private static float ResolveDefense(UnitBaseModel unit, string attribute)
        {
            UnitDefinition definition = unit.Definition;
            if (definition == null)
            {
                return 0f;
            }

            string column = string.Equals(attribute, "Fire", StringComparison.Ordinal)
                ? "def_fire"
                : string.Equals(attribute, "Lightning", StringComparison.Ordinal)
                    ? "def_lightning"
                    : string.Equals(attribute, "Ice", StringComparison.Ordinal)
                        ? "def_ice"
                        : string.Equals(attribute, "Darkness", StringComparison.Ordinal)
                            ? "def_darkness"
                            : string.Equals(attribute, "Holy", StringComparison.Ordinal)
                                ? "def_holy"
                                : "def_physical";
            float defense =
                definition.Columns.TryGetValue(column, out object value)
                && value is float number
                ? number
                : 0f;
            defense *= Math.Max(
                0f,
                1f - unit.ResolveRuntimeModifier(
                    "StatusElementResistReduction",
                    attribute));
            defense -= unit.ResolveRuntimeModifier(
                "StatusFlatElementResistReduction",
                attribute);
            defense *= Math.Max(
                0f,
                1f + ResolveEnemyPassive(unit, "DefenseUp", attribute));
            return defense;
        }

        private static float ResolveEnemyPassive(
            UnitBaseModel unit,
            string modifierKind,
            string attribute)
        {
            if (!(unit is EnemyModel enemy))
            {
                return 0f;
            }
            PassiveDefinition passive = enemy.SkillBucket.PassiveSkill;
            string kind = ReadString(passive, "modifier_kind");
            string passiveAttribute = ReadString(passive, "attribute");
            if (kind != modifierKind
                || (!string.IsNullOrEmpty(passiveAttribute)
                    && !string.IsNullOrEmpty(attribute)
                    && passiveAttribute != attribute))
            {
                return 0f;
            }
            return ReadFloat(passive, "modifier_value");
        }

        private static float ResolveStatusSum(
            UnitBaseModel unit,
            Func<Combat.Status.StatusEffect, float?> selector)
        {
            float result = 0f;
            for (int index = 0; index < unit.StatusEffects.Count; index++)
            {
                Combat.Status.StatusEffect status = unit.StatusEffects[index];
                result += (selector(status) ?? 0f) * status.CurrentStacks;
            }

            return result;
        }

        private static float ReadFloat(SkillDefinition skill, string column)
        {
            return skill.Columns.TryGetValue(column, out object value) && value is float number
                ? number
                : 0f;
        }

        private static bool ReadBool(SkillDefinition skill, string column)
        {
            return skill.Columns.TryGetValue(column, out object value)
                && value is bool flag
                && flag;
        }

        private static string ReadString(
            SkillDefinition skill,
            string column)
        {
            return skill.Columns.TryGetValue(column, out object value)
                ? value as string
                : null;
        }

        private static float Clamp01(float value)
        {
            return Math.Min(1f, Math.Max(0f, value));
        }

        private static string NormalizeAttribute(string attribute)
        {
            return string.IsNullOrEmpty(attribute)
                ? "Physical"
                : attribute;
        }

        private static void RequireLiving(UnitBaseModel unit, string parameterName)
        {
            if (unit == null)
            {
                throw new ArgumentNullException(parameterName);
            }

            if (!unit.IsAlive)
            {
                throw new InvalidOperationException("A defeated unit cannot execute combat results.");
            }
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
