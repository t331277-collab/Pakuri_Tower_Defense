using System;
using Pakuri.NewCore.Definitions.Skills;

/* 스킬의 쿨다운, 탄창, 재장전, 발사 간격 런타임 상태를 관리한다. */
namespace Pakuri.NewCore.Combat.Skills.Runtime
{
    public sealed class SkillCooldown
    {
        private const float TimeCompletionTolerance = 0.00001f;

        private readonly float cooldownDuration;
        private readonly int? magazineCapacity;
        private readonly float reloadDuration;
        private readonly float shotIntervalDuration;
        private int magazineBonus;

        /* 스킬 정의에서 쿨다운과 선택적 탄창·재장전·발사 간격을 읽어 초기화한다. */
        public SkillCooldown(SkillDefinition definition)
        {
            Definition = definition ?? throw new ArgumentNullException(nameof(definition));
            cooldownDuration = ValidateDuration(definition.cooldown_seconds, "cooldown_seconds");

            if (definition is ProjectileDefinition projectile)
            {
                if (projectile.magazine_capacity < 0)
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(projectile.magazine_capacity));
                }

                if (projectile.magazine_capacity > 0)
                {
                    magazineCapacity = projectile.magazine_capacity.Value;
                    reloadDuration = ValidateDuration(
                        projectile.reload_seconds,
                        "reload_seconds");
                    if (reloadDuration <= 0f)
                    {
                        throw new ArgumentException(
                            "A magazine skill requires a positive reload_seconds.",
                            nameof(definition));
                    }

                    CurrentMagazine = EffectiveMagazineCapacity;
                }

                shotIntervalDuration = ValidateDuration(
                    projectile.shot_interval_seconds,
                    "shot_interval_seconds");
            }
        }

        public SkillDefinition Definition { get; }

        public float RemainingCooldown { get; private set; }

        public int? CurrentMagazine { get; private set; }

        public float RemainingReload { get; private set; }

        public float RemainingShotInterval { get; private set; }

        public bool IsReloading => RemainingReload > 0f;

        /* 쿨다운·발사 간격·재장전·탄창 조건이 모두 사용 가능 상태인지 확인한다. */
        public bool CanUse()
        {
            return RemainingCooldown <= 0f
                && RemainingShotInterval <= 0f
                && !IsReloading
                && (!magazineCapacity.HasValue || CurrentMagazine > 0);
        }

        /* 사용 가능하면 타이머를 시작하고 탄창을 소비하며 성공을 반환한다. */
        public bool TryUse()
        {
            if (!CanUse())
            {
                return false;
            }

            RemainingCooldown = cooldownDuration;
            RemainingShotInterval = shotIntervalDuration;
            if (magazineCapacity.HasValue)
            {
                CurrentMagazine--;
                if (CurrentMagazine == 0)
                {
                    RemainingReload = reloadDuration;
                }
            }

            return true;
        }

        /* 쿨다운·발사 간격·재장전을 진행하고 재장전 완료 시 탄창을 채운다. */
        public void Tick(float deltaTime)
        {
            ValidateDeltaTime(deltaTime);
            RemainingCooldown = Reduce(RemainingCooldown, deltaTime);
            RemainingShotInterval = Reduce(RemainingShotInterval, deltaTime);
            if (RemainingReload > 0f)
            {
                RemainingReload = Reduce(RemainingReload, deltaTime);
                if (RemainingReload <= 0f)
                {
                    CurrentMagazine = EffectiveMagazineCapacity;
                }
            }
        }

        /* 다음 round 시작을 위해 쿨다운·재장전·발사 간격과 탄창을 초기화한다. */
        public void ResetForNextRound()
        {
            RemainingCooldown = 0f;
            RemainingReload = 0f;
            RemainingShotInterval = 0f;
            CurrentMagazine = EffectiveMagazineCapacity;
        }

        /* 현재 남은 스킬 쿨다운을 기준 시간의 지정 비율만큼 감소시킨다. */
        public void ReduceCooldown(float ratio)
        {
            ValidateRatio(ratio);
            RemainingCooldown = Reduce(RemainingCooldown, RemainingCooldown * ratio);
        }

        /* 현재 남은 재장전 시간을 기준 시간의 지정 비율만큼 감소시킨다. */
        public void ReduceReload(float ratio)
        {
            ValidateRatio(ratio);
            if (RemainingReload <= 0f)
            {
                return;
            }

            RemainingReload = Reduce(RemainingReload, RemainingReload * ratio);
            if (RemainingReload <= 0f)
            {
                CurrentMagazine = EffectiveMagazineCapacity;
            }
        }

        /* 현재 남은 쿨다운에 지정 배율을 적용한다. */
        public void ScaleCooldown(float multiplier)
        {
            ValidateMultiplier(multiplier);
            RemainingCooldown *= multiplier;
        }

        /* 현재 남은 재장전 시간에 지정 배율을 적용한다. */
        public void ScaleReload(float multiplier)
        {
            ValidateMultiplier(multiplier);
            RemainingReload *= multiplier;
        }

        /* 현재 남은 발사 간격에 지정 배율을 적용한다. */
        public void ScaleShotInterval(float multiplier)
        {
            ValidateMultiplier(multiplier);
            RemainingShotInterval *= multiplier;
        }

        /* 현재 스킬 쿨다운만 즉시 사용 가능 상태로 만든다. */
        public void ResetCooldown()
        {
            RemainingCooldown = 0f;
        }

        /* 기본 탄창 크기에 더할 보너스 탄환 수를 검증해 반영한다. */
        public void SetMagazineBonus(int bonus)
        {
            if (bonus < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(bonus));
            }
            if (!magazineCapacity.HasValue)
            {
                return;
            }
            int difference = bonus - magazineBonus;
            magazineBonus = bonus;
            if (!IsReloading && CurrentMagazine.HasValue)
            {
                CurrentMagazine = Math.Max(
                    0,
                    Math.Min(
                        EffectiveMagazineCapacity.Value,
                        CurrentMagazine.Value + difference));
            }
        }

        private int? EffectiveMagazineCapacity =>
            magazineCapacity.HasValue
                ? magazineCapacity.Value + magazineBonus
                : (int?)null;

        /* 선택적 시간 값이 없으면 0을, 있으면 음수가 아닌 유한값을 반환한다. */
        private static float ValidateDuration(float? value, string columnName)
        {
            if (!value.HasValue)
            {
                return 0f;
            }

            if (value.Value < 0f
                || float.IsNaN(value.Value)
                || float.IsInfinity(value.Value))
            {
                throw new ArgumentException(
                    $"Skill Definition has an invalid {columnName}.");
            }

            return value.Value;
        }

        /* tick 경과 시간이 음수가 아닌 유한값인지 검증한다. */
        private static void ValidateDeltaTime(float deltaTime)
        {
            if (deltaTime < 0f || float.IsNaN(deltaTime) || float.IsInfinity(deltaTime))
            {
                throw new ArgumentOutOfRangeException(nameof(deltaTime));
            }
        }

        /* 감소 비율이 음수가 아닌 유한값인지 검증한다. */
        private static void ValidateRatio(float ratio)
        {
            if (ratio < 0f || ratio > 1f || float.IsNaN(ratio) || float.IsInfinity(ratio))
            {
                throw new ArgumentOutOfRangeException(nameof(ratio));
            }
        }

        /* 시간 배율이 음수가 아닌 유한값인지 검증한다. */
        private static void ValidateMultiplier(float multiplier)
        {
            if (multiplier < 0f
                || float.IsNaN(multiplier)
                || float.IsInfinity(multiplier))
            {
                throw new ArgumentOutOfRangeException(nameof(multiplier));
            }
        }

        /* 남은 시간에서 경과 시간을 빼고 0 미만으로 내려가지 않게 한다. */
        private static float Reduce(float value, float deltaTime)
        {
            float remaining = value - deltaTime;
            return remaining <= TimeCompletionTolerance ? 0f : remaining;
        }
    }
}
