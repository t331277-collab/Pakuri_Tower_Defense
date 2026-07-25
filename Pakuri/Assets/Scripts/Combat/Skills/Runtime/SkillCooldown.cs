using System;
using Pakuri.NewCore.Definitions.Skills;

/* 스킬의 쿨다운, 탄창, 재장전, 발사 간격 런타임 상태를 관리한다. */
namespace Pakuri.NewCore.Combat.Skills.Runtime
{
    public class SkillCooldown
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
            Definition = definition;
            cooldownDuration = ResolveDuration(definition.cooldown_seconds);

            if (definition is ProjectileDefinition projectile)
            {

                if (projectile.magazine_capacity > 0)
                {
                    magazineCapacity = projectile.magazine_capacity.Value;
                    reloadDuration = ResolveDuration(projectile.reload_seconds);

                    CurrentMagazine = EffectiveMagazineCapacity;
                }

                shotIntervalDuration = ResolveDuration(
                    projectile.shot_interval_seconds);
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
            RemainingCooldown = Reduce(RemainingCooldown, RemainingCooldown * ratio);
        }

        /* 현재 남은 재장전 시간을 기준 시간의 지정 비율만큼 감소시킨다. */
        public void ReduceReload(float ratio)
        {
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
            RemainingCooldown *= multiplier;
        }

        /* 현재 남은 재장전 시간에 지정 배율을 적용한다. */
        public void ScaleReload(float multiplier)
        {
            RemainingReload *= multiplier;
        }

        /* 현재 남은 발사 간격에 지정 배율을 적용한다. */
        public void ScaleShotInterval(float multiplier)
        {
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

        private int? EffectiveMagazineCapacity
        {
            get
            {
                if (!magazineCapacity.HasValue)
                {
                    return null;
                }

                return magazineCapacity.Value + magazineBonus;
            }
        }

        /* 선택적 시간 값이 없으면 0을 반환한다. */
        private static float ResolveDuration(float? value)
        {
            if (!value.HasValue)
            {
                return 0f;
            }

            return value.Value;
        }

        /* 남은 시간에서 경과 시간을 빼고 0 미만으로 내려가지 않게 한다. */
        private static float Reduce(float value, float deltaTime)
        {
            float remaining = value - deltaTime;
            if (remaining <= TimeCompletionTolerance)
            {
                return 0f;
            }

            return remaining;
        }
    }
}
