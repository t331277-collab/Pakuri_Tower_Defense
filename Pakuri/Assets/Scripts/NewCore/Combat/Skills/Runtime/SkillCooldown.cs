using System;
using Pakuri.NewCore.Definitions.Skills;

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

        public bool CanUse()
        {
            return RemainingCooldown <= 0f
                && RemainingShotInterval <= 0f
                && !IsReloading
                && (!magazineCapacity.HasValue || CurrentMagazine > 0);
        }

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

        public void ResetForNextRound()
        {
            RemainingCooldown = 0f;
            RemainingReload = 0f;
            RemainingShotInterval = 0f;
            CurrentMagazine = EffectiveMagazineCapacity;
        }

        public void ReduceCooldown(float ratio)
        {
            ValidateRatio(ratio);
            RemainingCooldown = Reduce(RemainingCooldown, RemainingCooldown * ratio);
        }

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

        public void ScaleCooldown(float multiplier)
        {
            ValidateMultiplier(multiplier);
            RemainingCooldown *= multiplier;
        }

        public void ScaleReload(float multiplier)
        {
            ValidateMultiplier(multiplier);
            RemainingReload *= multiplier;
        }

        public void ScaleShotInterval(float multiplier)
        {
            ValidateMultiplier(multiplier);
            RemainingShotInterval *= multiplier;
        }

        public void ResetCooldown()
        {
            RemainingCooldown = 0f;
        }

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

        private static void ValidateDeltaTime(float deltaTime)
        {
            if (deltaTime < 0f || float.IsNaN(deltaTime) || float.IsInfinity(deltaTime))
            {
                throw new ArgumentOutOfRangeException(nameof(deltaTime));
            }
        }

        private static void ValidateRatio(float ratio)
        {
            if (ratio < 0f || ratio > 1f || float.IsNaN(ratio) || float.IsInfinity(ratio))
            {
                throw new ArgumentOutOfRangeException(nameof(ratio));
            }
        }

        private static void ValidateMultiplier(float multiplier)
        {
            if (multiplier < 0f
                || float.IsNaN(multiplier)
                || float.IsInfinity(multiplier))
            {
                throw new ArgumentOutOfRangeException(nameof(multiplier));
            }
        }

        private static float Reduce(float value, float deltaTime)
        {
            float remaining = value - deltaTime;
            return remaining <= TimeCompletionTolerance ? 0f : remaining;
        }
    }
}
