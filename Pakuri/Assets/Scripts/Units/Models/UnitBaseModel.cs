using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Pakuri.NewCore.Combat.Status;
using Pakuri.NewCore.Definitions.Status;
using Pakuri.NewCore.Definitions.Units;

/* 전투 좌표 값과 유닛의 체력·보호막·상태 런타임 권한을 정의한다. */
namespace Pakuri.NewCore.Units.Models
{
    public readonly struct CombatVector2 : IEquatable<CombatVector2>
    {
        /* 외부 좌표 입력을 엔진 독립 전투 좌표로 저장한다. */
        public CombatVector2(float x, float y)
        {

            X = x;
            Y = y;
        }

        public float X { get; }

        public float Y { get; }

        public float SqrMagnitude => (X * X) + (Y * Y);

        public float Magnitude => (float)Math.Sqrt(SqrMagnitude);

        public CombatVector2 Normalized
        {
            get
            {
                float magnitude = Magnitude;
                if (magnitude <= 0.00001f)
                {
                    return default;
                }

                return new CombatVector2(X / magnitude, Y / magnitude);
            }
        }

        /* 두 전투 좌표의 성분별 합을 반환한다. */
        public static CombatVector2 operator +(CombatVector2 left, CombatVector2 right)
        {
            return new CombatVector2(left.X + right.X, left.Y + right.Y);
        }

        /* 두 전투 좌표의 성분별 차를 반환한다. */
        public static CombatVector2 operator -(CombatVector2 left, CombatVector2 right)
        {
            return new CombatVector2(left.X - right.X, left.Y - right.Y);
        }

        /* 전투 좌표를 지정 배율로 확장한 값을 반환한다. */
        public static CombatVector2 operator *(CombatVector2 value, float multiplier)
        {
            return new CombatVector2(value.X * multiplier, value.Y * multiplier);
        }

        /* 두 전투 좌표 사이의 유클리드 거리를 반환한다. */
        public static float Distance(CombatVector2 left, CombatVector2 right)
        {
            return (left - right).Magnitude;
        }

        /* 두 전투 좌표의 성분 값이 같은지 비교한다. */
        public bool Equals(CombatVector2 other)
        {
            return X.Equals(other.X) && Y.Equals(other.Y);
        }

        /* 객체가 같은 전투 좌표 값을 나타내는지 비교한다. */
        public override bool Equals(object obj)
        {
            return obj is CombatVector2 other && Equals(other);
        }

        /* 두 좌표 성분에서 값 기반 해시 코드를 만든다. */
        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y);
        }

    }

    public abstract class UnitBaseModel
    {
        private readonly List<StatusEffect> statusEffects = new List<StatusEffect>();
        private readonly List<RuntimeCombatModifier> runtimeModifiers =
            new List<RuntimeCombatModifier>();
        private readonly List<ShieldLayer> shieldLayers =
            new List<ShieldLayer>();
        private readonly IReadOnlyList<StatusEffect> readOnlyStatusEffects;
        private readonly IReadOnlyList<RuntimeCombatModifier> readOnlyRuntimeModifiers;
        private long nextShieldVersion;

        /* 정의와 유효한 최대 체력으로 유닛의 초기 생명·상태 컬렉션을 구성한다. */
        protected UnitBaseModel(UnitDefinition definition, float maximumHealth)
        {

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

        /* 유닛의 엔진 독립 전투 좌표를 새 값으로 설정한다. */
        public void SetPosition(CombatVector2 position)
        {
            Position = position;
        }

        /* 보호막 흡수 통지 없이 유닛에 피해를 적용한다. */
        public float ApplyDamage(float amount)
        {
            return ApplyDamage(amount, null);
        }

        /* 보호막 레이어를 먼저 소모하고 남은 피해를 체력에 적용한다. */
        public float ApplyDamage(
            float amount,
            Action<UnitBaseModel, string, float> shieldAbsorbed)
        {
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

        /* 생존 유닛에 최대 체력까지 실제 회복량을 적용한다. */
        public float Heal(float amount)
        {
            if (amount == 0f || !IsAlive)
            {
                return 0f;
            }

            float applied = Math.Min(MaximumHealth - CurrentHealth, amount);
            CurrentHealth += applied;
            return applied;
        }

        /* 출처 없는 단순 보호막을 현재 유닛에 추가한다. */
        public bool TryAddShield(float amount)
        {
            return TryAddShield(amount, null, null);
        }

        /* 출처와 스킬 식별자가 있는 보호막을 기본 병합 규칙으로 추가한다. */
        public bool TryAddShield(
            float amount,
            UnitBaseModel source,
            string skillId)
        {
            return TryAddShield(
                amount,
                source,
                skillId,
                null,
                null,
                out _);
        }

        /* public 보호막 입력과 병합 정책을 적용해 레이어 버전을 생성하거나 갱신한다. */
        public bool TryAddShield(
            float amount,
            UnitBaseModel source,
            string skillId,
            string mergePolicy,
            string amountRefreshPolicy,
            out long applicationVersion)
        {
            applicationVersion = 0L;
            if (amount == 0f || !IsAlive)
            {
                return false;
            }

            long version = ++nextShieldVersion;
            if (UsesSameSourceMerge(mergePolicy))
            {
                ShieldLayer existing = FindShieldLayer(source, skillId);
                if (existing != null)
                {
                    float refreshedAmount = ResolveRefreshedShieldAmount(
                        existing.Amount,
                        amount,
                        amountRefreshPolicy);
                    float updatedShield =
                        CurrentShield - existing.Amount + refreshedAmount;

                    existing.Amount = refreshedAmount;
                    existing.Version = version;
                    CurrentShield = updatedShield;
                    applicationVersion = version;
                    return true;
                }
            }

            float updated = CurrentShield + amount;

            CurrentShield = updated;
            shieldLayers.Add(new ShieldLayer(
                source,
                skillId,
                amount,
                version));
            applicationVersion = version;
            return true;
        }

        /* 전체 보호막 값과 출처별 레이어를 함께 제거한다. */
        public void ClearShield()
        {
            CurrentShield = 0f;
            shieldLayers.Clear();
        }

        /* 지정 출처와 스킬의 모든 보호막 레이어를 제거한다. */
        public float RemoveShield(UnitBaseModel source, string skillId)
        {
            return RemoveShield(source, skillId, null);
        }

        /* 지정 적용 버전과 일치하는 보호막 레이어만 제거한다. */
        public float RemoveShield(
            UnitBaseModel source,
            string skillId,
            long applicationVersion)
        {
            return RemoveShield(
                source,
                skillId,
                (long?)applicationVersion);
        }

        /* 출처·스킬·선택 버전 조건으로 보호막 레이어를 찾아 실제 제거량을 계산한다. */
        private float RemoveShield(
            UnitBaseModel source,
            string skillId,
            long? applicationVersion)
        {
            float removed = 0f;
            for (int index = shieldLayers.Count - 1; index >= 0; index--)
            {
                ShieldLayer layer = shieldLayers[index];
                if (ReferenceEquals(layer.Source, source)
                    && string.Equals(
                        layer.SkillId,
                        skillId,
                        StringComparison.Ordinal)
                    && (!applicationVersion.HasValue
                        || layer.Version == applicationVersion.Value))
                {
                    removed += layer.Amount;
                    shieldLayers.RemoveAt(index);
                }
            }
            CurrentShield = Math.Max(0f, CurrentShield - removed);
            return removed;
        }

        /* 현재 보호막 또는 지정 스킬 출처의 활성 보호막 존재 여부를 반환한다. */
        public bool HasShieldFrom(string skillId)
        {
            if (string.IsNullOrEmpty(skillId))
            {
                return CurrentShield > 0f;
            }

            for (int index = 0; index < shieldLayers.Count; index++)
            {
                if (shieldLayers[index].Amount > 0f
                    && string.Equals(
                        shieldLayers[index].SkillId,
                        skillId,
                        StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        /* public 상태 적용 요청을 기존 인스턴스 갱신 또는 새 상태 생성으로 처리한다. */
        public StatusEffect ApplyStatus(
            StatusDefinition definition,
            UnitBaseModel applyingUnit,
            float? durationSeconds = null,
            int? stackAmount = null,
            string sourceSkillId = null,
            int? maximumStacks = null)
        {

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
                    existing.Refresh(
                        durationSeconds,
                        stackAmount,
                        maximumStacks);
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
                    sourceSkillId,
                    maximumStacks);
            statusEffects.Add(effect);
            return effect;
        }

        /* 지정 상태 인스턴스를 현재 활성 목록에서 제거한다. */
        public bool RemoveStatus(StatusEffect effect)
        {
            return effect != null && statusEffects.Remove(effect);
        }

        /* 지정 상태 스택의 일부를 소비 비율만큼 제거한다. */
        public int ConsumeStatus(string statusId, float ratio)
        {
            if (string.IsNullOrEmpty(statusId))
            {
                return 0;
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

        /* 런타임 수정치를 지속 상태 목록에 추가한다. */
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

        /* 종류와 선택 필터가 일치하는 활성 런타임 수정치 합을 반환한다. */
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

        /* 상태·수정치의 경과 시간과 만료 생명주기를 진행한다. */
        public void TickStatusEffects(float deltaTime)
        {
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

        /* 모든 활성 상태 효과와 런타임 수정치를 제거한다. */
        public void ClearStatusEffects()
        {
            statusEffects.Clear();
            runtimeModifiers.Clear();
        }

        /* 파생 유닛의 재초기화를 위해 체력·보호막·상태를 초기값으로 복원한다. */
        protected void ResetVitalsAndStatuses()
        {
            CurrentHealth = MaximumHealth;
            CurrentShield = 0f;
            shieldLayers.Clear();
            statusEffects.Clear();
            runtimeModifiers.Clear();
        }

        private class ShieldLayer
        {
            /* 보호막 출처·스킬·양·적용 버전을 하나의 소모 레이어로 저장한다. */
            public ShieldLayer(
                UnitBaseModel source,
                string skillId,
                float amount,
                long version)
            {
                Source = source;
                SkillId = skillId;
                Amount = amount;
                Version = version;
            }

            public UnitBaseModel Source { get; }

            public string SkillId { get; }

            public float Amount { get; set; }

            public long Version { get; set; }
        }

        /* 동일 출처와 스킬 식별자를 가진 최신 보호막 레이어를 찾는다. */
        private ShieldLayer FindShieldLayer(
            UnitBaseModel source,
            string skillId)
        {
            for (int index = shieldLayers.Count - 1; index >= 0; index--)
            {
                ShieldLayer layer = shieldLayers[index];
                if (ReferenceEquals(layer.Source, source)
                    && string.Equals(
                        layer.SkillId,
                        skillId,
                        StringComparison.Ordinal))
                {
                    return layer;
                }
            }

            return null;
        }

        /* 보호막 병합 정책이 동일 출처 레이어 갱신을 요구하는지 확인한다. */
        private static bool UsesSameSourceMerge(string mergePolicy)
        {
            return string.Equals(
                    mergePolicy,
                    "same_source_refresh",
                    StringComparison.OrdinalIgnoreCase)
                || string.Equals(
                    mergePolicy,
                    "same_source_take_highest",
                    StringComparison.OrdinalIgnoreCase);
        }

        /* 보호막 양 갱신 정책에 따라 누적·교체·최댓값 결과를 계산한다. */
        private static float ResolveRefreshedShieldAmount(
            float currentAmount,
            float incomingAmount,
            string amountRefreshPolicy)
        {
            if (string.Equals(
                amountRefreshPolicy,
                "stack",
                StringComparison.OrdinalIgnoreCase))
            {
                return currentAmount + incomingAmount;
            }

            if (string.Equals(
                amountRefreshPolicy,
                "replace",
                StringComparison.OrdinalIgnoreCase))
            {
                return incomingAmount;
            }

            return Math.Max(currentAmount, incomingAmount);
        }

        /* 활성 상태 중 하나라도 권한을 금지하면 false를 반환한다. */
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

        /* 활성 상태별 스택이 반영된 선택 수치의 합을 계산한다. */
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

        /* 값이 유한한 양수인지 확인한다. */

    }
}
