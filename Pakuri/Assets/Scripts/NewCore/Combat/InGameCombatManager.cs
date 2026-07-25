using System;
using System.Collections.Generic;
using Pakuri.NewCore.Combat.Status;
using Pakuri.NewCore.Definitions.Skills;
using Pakuri.NewCore.Definitions.Status;
using Pakuri.NewCore.Definitions.Units;
using Pakuri.NewCore.Combat.Skills.Execution;
using Pakuri.NewCore.Units.Models;

/* 스킬 실행과 피해·회복·보호막·상태 적용의 전투 권한을 소유한다. */
namespace Pakuri.NewCore.Combat
{
    public readonly struct CombatResult
    {
        /* 한 번의 전투 처리에서 발생한 체력·보호막 변화와 치명타·처치 결과를 묶는다. */
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

        /* 난수 공급원과 스킬 실행 런타임을 연결하고 trigger dispatcher를 준비한다. */
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

        /* 요청 유닛을 관찰 대상으로 등록하고 스킬 런타임 실행을 시도한다. */
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

        /* 경과 시간만큼 예약된 trigger 실행 상태를 진행한다. */
        public void TickTriggers(float deltaTime)
        {
            triggers.Tick(deltaTime);
        }

        /* 학습된 패시브 효과를 현재 유닛 목록에 다시 계산해 반영한다. */
        public void ApplyPassiveChanges(
            IReadOnlyList<UnitBaseModel> units)
        {
            executionRuntime.ApplyPassives(this, units);
        }

        /* 전투 결과 실행을 종료하고 구독과 임시 상태를 정리한다. */
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

        /* 전투 시작 trigger를 각 유닛 소유 스킬에 전달하고 실행 수를 반환한다. */
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

        /* 탄창의 마지막 투사체 적중 정보를 후속 trigger에 전달한다. */
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

        /* 만료된 보호막 양을 보호막 만료 trigger에 전달한다. */
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

        /* 스킬 계수와 공격 능력치를 계산해 대상에게 피해와 후속 trigger를 적용한다. */
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

        /* 지정 유닛이 현재 전투에서 발생시킨 누적 적중 횟수를 반환한다. */
        public int GetOutgoingHitCount(UnitBaseModel source)
        {
            return source != null
                && outgoingHitCounts.TryGetValue(source, out int count)
                    ? count
                    : 0;
        }

        /* trigger가 제공한 직접 계수로 피해를 계산하고 대상과 후속 이벤트에 반영한다. */
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

        /* 지정 회복량을 대상 체력에 적용하고 실제 변화 결과를 발행한다. */
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
            float applied = target.Heal(amount);
            CombatResult result =
                new CombatResult(source, target, skill.skill_id, applied, 0f, false, false);
            if (applied > 0f)
            {
                HealingApplied?.Invoke(result);
            }

            return result;
        }

        /* 기본 병합 정책으로 대상에게 보호막을 추가한다. */
        public CombatResult AddShield(
            UnitBaseModel source,
            UnitBaseModel target,
            SkillDefinition skill,
            float amount,
            string shieldSourceId = null)
        {
            return AddShield(
                source,
                target,
                skill,
                amount,
                shieldSourceId,
                null,
                null,
                out _);
        }

        /* 보호막 출처와 병합·갱신 정책을 적용하고 application version을 반환한다. */
        public CombatResult AddShield(
            UnitBaseModel source,
            UnitBaseModel target,
            SkillDefinition skill,
            float amount,
            string shieldSourceId,
            string mergePolicy,
            string amountRefreshPolicy,
            out long applicationVersion)
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
            target.TryAddShield(
                amount,
                source,
                string.IsNullOrEmpty(shieldSourceId)
                    ? skill.skill_id
                    : shieldSourceId,
                mergePolicy,
                amountRefreshPolicy,
                out applicationVersion);
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

        /* 시전자 보정이 반영된 상태 효과를 대상에게 적용하고 이벤트를 발행한다. */
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

            float durationBonus = source.ResolveRuntimeModifier(
                "StatusDurationBonus",
                status.status_effect_id);
            float? resolvedDuration = duration;
            if (Math.Abs(durationBonus) > 0.00001f)
            {
                resolvedDuration = Math.Max(
                    0f,
                    (duration
                        ?? status.default_duration_seconds
                        ?? 0f)
                    + durationBonus);
            }

            target.ApplyStatus(
                status,
                source,
                resolvedDuration,
                stacks,
                sourceSkillId,
                maximumStacks);
            StatusApplied?.Invoke(source, target, status);
        }

        /* 적 정의의 nexus 피해를 적용하고 파괴 시 처치 이벤트를 발행한다. */
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

        /* 스킬 기본값과 공격력·주문력 계수를 합산한 음수 없는 원시 값을 계산한다. */
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

        /* 유닛의 정의와 런타임 보정을 반영한 음수 없는 주문력을 반환한다. */
        public float CalculateSpellPower(UnitBaseModel source)
        {
            RequireLiving(source, nameof(source));
            return Math.Max(0f, ResolveSpellPower(source));
        }

        /* 스킬 활성화 이벤트를 발행하고 OnSkillCast trigger를 전달한다. */
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

        /* 신규 유닛의 상태 만료 이벤트를 한 번만 구독한다. */
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

        /* 만료된 상태 정보를 status·shield 만료 trigger로 전달한다. */
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

        /* 유닛 정의와 상태·런타임 보정을 반영한 공격력을 계산한다. */
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

        /* 유닛 정의와 런타임 보정을 반영한 주문력을 계산한다. */
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

        /* 유닛 정의와 보정을 합산한 치명타 확률을 계산한다. */
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

        /* 유닛 정의와 보정을 합산한 치명타 피해 배율을 계산한다. */
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

        /* 대상 정의와 상태 보정을 합산한 치명타 저항을 계산한다. */
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

        /* 피해 종류·속성·조건 상태에 따른 대상의 받는 피해 보정을 계산한다. */
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

        /* 시전자의 추가 피해 modifier를 일치하는 속성의 trigger 피해로 적용한다. */
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

        /* shield 별칭 또는 지정 status id가 유닛에 적용되어 있는지 확인한다. */
        private static bool HasStatus(
            UnitBaseModel unit,
            string statusId)
        {
            if (string.Equals(
                    statusId,
                    "shield",
                    StringComparison.OrdinalIgnoreCase))
            {
                return unit != null && unit.CurrentShield > 0f;
            }
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

        /* 유닛 정의와 속성별 상태 보정을 반영한 방어력을 계산한다. */
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

        /* 적 패시브 중 요청 종류와 속성에 맞는 보정 합계를 반환한다. */
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

        /* 유닛의 모든 상태 효과에서 지정 선택자가 반환한 값을 합산한다. */
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

        /* 스킬 정의의 지정 열이 float면 반환하고 아니면 0을 반환한다. */
        private static float ReadFloat(SkillDefinition skill, string column)
        {
            return skill.Columns.TryGetValue(column, out object value) && value is float number
                ? number
                : 0f;
        }

        /* 스킬 정의의 지정 열이 true인 bool인지 확인한다. */
        private static bool ReadBool(SkillDefinition skill, string column)
        {
            return skill.Columns.TryGetValue(column, out object value)
                && value is bool flag
                && flag;
        }

        /* 스킬 정의의 지정 열이 문자열이면 반환하고 아니면 null을 반환한다. */
        private static string ReadString(
            SkillDefinition skill,
            string column)
        {
            return skill.Columns.TryGetValue(column, out object value)
                ? value as string
                : null;
        }

        /* 입력 값을 0 이상 1 이하 범위로 제한한다. */
        private static float Clamp01(float value)
        {
            return Math.Min(1f, Math.Max(0f, value));
        }

        /* 빈 속성 값을 기본 Physical 속성으로 정규화한다. */
        private static string NormalizeAttribute(string attribute)
        {
            return string.IsNullOrEmpty(attribute)
                ? "Physical"
                : attribute;
        }

        /* 유닛이 null이 아니고 생존 상태인지 검증한다. */
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

        /* 입력 값이 음수가 아닌 유한한 수인지 검증한다. */
        private static void ValidateNonNegativeFinite(float value, string parameterName)
        {
            if (value < 0f || float.IsNaN(value) || float.IsInfinity(value))
            {
                throw new ArgumentOutOfRangeException(parameterName);
            }
        }
    }
}
