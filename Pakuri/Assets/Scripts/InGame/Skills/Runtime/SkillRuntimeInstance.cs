using System;
using Pakuri.Data;
using UnityEngine;

/*
 * 컴파일된 스킬 하나가 전투 중 가지는 변경 가능한 실행 상태를 관리한다.
 * 재사용 대기시간, 탄창·재장전, Tick, 연속 발사, 적중 횟수를 갱신하고
 * 현재 Choice Snapshot에 따른 시전 가능 여부와 시간 보정값을 적용한다.
 */
namespace Pakuri.InGame
{
    public sealed class SkillRuntimeInstance
    {
        /*
         * 스킬 런타임 인스턴스에 필요한 값을 초기화한다.
         */
        public SkillRuntimeInstance(BaseUnitRuntimeModel owner, SkillRuntimeData data)
        {
            Owner = owner;
            Data = data;
            BasePlan = SkillExecutionPlanCompiler.Compile(data, null, data != null ? data.NormalizedPlanNodes : null);
            ResetRuntimeState();
        }

        public BaseUnitRuntimeModel Owner { get; }
        public SkillRuntimeData Data { get; }
        public SkillExecutionPlan BasePlan { get; }
        public string SkillId => Data != null ? Data.SkillId : string.Empty;
        public SkillSlot Slot => Data != null ? Data.Slot : default;
        public float CooldownRemaining { get; private set; }
        public float CastRemaining { get; private set; }
        public float ActiveDurationRemaining { get; private set; }
        public float TickRemaining { get; private set; }
        public float ReloadRemaining { get; private set; }
        public int MagazineRemaining { get; private set; }
        public int ProjectileLaunchCount { get; private set; }
        public int SkillHitCount { get; private set; }

        private int effectiveMaxMagazineSize;
        private int effectiveBurstProjectileCount;
        private float effectiveReloadDuration;
        private float effectiveTickInterval;
        private float effectiveBurstInterval;
        private float effectiveCooldownDuration;
        private int queuedBurstShotsRemaining;
        private string consecutiveHitTargetUnitId;
        private int consecutiveHitRepeatCount;

        public bool IsCasting => CastRemaining > 0f;
        public bool IsActive => ActiveDurationRemaining > 0f;
        public bool IsReloading => ReloadRemaining > 0f;
        public bool IsBursting => queuedBurstShotsRemaining > 0;
        public int MaxMagazineSize => effectiveMaxMagazineSize;
        public float ReloadDuration => effectiveReloadDuration;
        public float EffectiveCooldownDuration => effectiveCooldownDuration;
        public int EffectiveBurstProjectileCount => effectiveBurstProjectileCount;
        public bool UsesMagazine => MaxMagazineSize > 0;
        public bool HasMagazine => !UsesMagazine || MagazineRemaining > 0;
        public bool CanCast => CanCastWithSnapshot(null);

        /*
         * 런타임 상태값을 초기화한다.
         */
        public void ResetRuntimeState()
        {
            effectiveMaxMagazineSize = ResolveMaxMagazineSize(Data);
            effectiveBurstProjectileCount = ResolveBurstProjectileCount(Data);
            effectiveReloadDuration = ResolveReloadDuration(Data);
            effectiveTickInterval = ResolveTickInterval(Data);
            effectiveBurstInterval = ResolveBurstInterval(Data);
            effectiveCooldownDuration = ResolveCooldownDuration(Data);
            CooldownRemaining = 0f;
            CastRemaining = 0f;
            ActiveDurationRemaining = 0f;
            TickRemaining = 0f;
            ReloadRemaining = 0f;
            MagazineRemaining = MaxMagazineSize;
            queuedBurstShotsRemaining = 0;
            ProjectileLaunchCount = 0;
            SkillHitCount = 0;
            consecutiveHitTargetUnitId = string.Empty;
            consecutiveHitRepeatCount = 0;
        }

        /*
         * 투사체 발사 횟수를 증가시키고 현재 횟수를 반환한다.
         */
        public int AdvanceProjectileLaunchCount()
        {
            if (ProjectileLaunchCount == int.MaxValue)
            {
                ProjectileLaunchCount = 0;
            }

            ProjectileLaunchCount++;
            return ProjectileLaunchCount;
        }

        /*
         * 스킬 적중 횟수를 증가시키고 현재 횟수를 반환한다.
         */
        public int AdvanceSkillHitCount()
        {
            if (SkillHitCount == int.MaxValue)
            {
                SkillHitCount = 0;
            }

            SkillHitCount++;
            return SkillHitCount;
        }

        /*
         * 같은 대상을 연속으로 적중했을 때 적용할 피해 배율을 결정한다.
         */
        public float ResolveConsecutiveHitDamageMultiplier(BaseUnitRuntimeModel target, SkillExecutionSnapshot snapshot)
        {
            if (target == null)
            {
                return 1f;
            }

            var projectileData = Data as ProjectileSkillRuntimeData;
            var bonusRate = snapshot != null && snapshot.ConsecutiveHitBonusRate > 0f
                ? snapshot.ConsecutiveHitBonusRate
                : projectileData != null ? projectileData.ConsecutiveHitBonusRate : 0f;
            var bonusMax = snapshot != null && snapshot.ConsecutiveHitMax > 0f
                ? snapshot.ConsecutiveHitMax
                : projectileData != null ? projectileData.ConsecutiveHitMax : 0f;
            if (bonusRate <= 0f || bonusMax <= 0f)
            {
                return 1f;
            }

            var unitId = target.Identity != null ? target.Identity.UnitId : string.Empty;
            if (string.IsNullOrWhiteSpace(unitId))
            {
                consecutiveHitTargetUnitId = string.Empty;
                consecutiveHitRepeatCount = 0;
                return 1f;
            }

            if (string.Equals(consecutiveHitTargetUnitId, unitId, StringComparison.Ordinal))
            {
                consecutiveHitRepeatCount = Math.Min(consecutiveHitRepeatCount + 1, int.MaxValue - 1);
            }
            else
            {
                consecutiveHitTargetUnitId = unitId;
                consecutiveHitRepeatCount = 0;
            }

            var bonus = Mathf.Min(
                Mathf.Max(0f, bonusMax),
                Mathf.Max(0f, bonusRate) * consecutiveHitRepeatCount);
            return 1f + bonus;
        }

        /*
         * 스킬의 시전, 지속시간, 재사용 대기시간을 갱신한다.
         */
        public void Tick(float deltaTime)
        {
            if (deltaTime <= 0f)
            {
                return;
            }

            var actionDeltaTime = deltaTime * StatusEffectRules.ResolveActionSpeedMultiplier(Owner);
            CooldownRemaining = TickDown(CooldownRemaining, actionDeltaTime);
            CastRemaining = TickDown(CastRemaining, actionDeltaTime);
            ActiveDurationRemaining = TickDown(ActiveDurationRemaining, deltaTime);
            TickRemaining = TickDown(TickRemaining, actionDeltaTime);
            ReloadRemaining = TickDown(ReloadRemaining, deltaTime);

            if (UsesMagazine
                && MagazineRemaining <= 0
                && ReloadRemaining <= 0f
                && CooldownRemaining <= 0f
                && !IsBursting)
            {
                MagazineRemaining = MaxMagazineSize;
            }
        }

        /*
         * 시전 포함 실행 정보를 가능한 상태인지 확인한다.
         */
        public bool CanCastWithSnapshot(SkillExecutionSnapshot snapshot)
        {
            RefreshRuntimeModifiers(snapshot);
            if (Data == null
                || !Data.IsActive
                || IsCasting
                || !IsCastIntervalReady())
            {
                return false;
            }

            if (IsBursting)
            {
                return !IsReloading;
            }

            return CooldownRemaining <= 0f
                && !IsReloading
                && HasMagazine;
        }

        /*
         * 시전을 시작하고 성공 여부를 반환한다.
         */
        public bool TryBeginCast()
        {
            return TryBeginCast(null);
        }

        /*
         * 시전을 시작하고 성공 여부를 반환한다.
         */
        public bool TryBeginCast(SkillExecutionSnapshot snapshot)
        {
            RefreshRuntimeModifiers(snapshot);
            if (IsBursting)
            {
                queuedBurstShotsRemaining = Math.Max(0, queuedBurstShotsRemaining - 1);
                if (IsBursting)
                {
                    TickRemaining = effectiveBurstInterval;
                }
                else
                {
                    TickRemaining = effectiveTickInterval;
                    BeginRecoveryIfNeeded();
                }

                return true;
            }

            if (!CanCastWithSnapshot(snapshot))
            {
                return false;
            }

            if (UsesMagazine)
            {
                MagazineRemaining = Math.Max(0, MagazineRemaining - 1);
            }

            var timing = Data.Timing;
            CastRemaining = timing != null ? Mathf.Max(0f, timing.CastTime) : 0f;
            ActiveDurationRemaining = timing != null ? Mathf.Max(0f, timing.ActiveDuration) : 0f;
            queuedBurstShotsRemaining = Math.Max(0, effectiveBurstProjectileCount - 1);
            TickRemaining = IsBursting ? effectiveBurstInterval : effectiveTickInterval;

            if (!IsBursting)
            {
                BeginRecoveryIfNeeded();
            }

            return true;
        }

        /*
         * 다음 주기 효과를 실행할 시간이 되었는지 확인한다.
         */
        public bool IsTickReady()
        {
            var timing = Data != null ? Data.Timing : null;
            return timing != null && timing.TickInterval > 0f && TickRemaining <= 0f;
        }

        /*
         * 주기 간격을 초기화한다.
         */
        public void ResetTickInterval()
        {
            TickRemaining = effectiveTickInterval;
        }

        /*
         * 현재 연속 발사에서 몇 번째 투사체인지 계산한다.
         */
        public int ResolveCurrentBurstProjectileIndex()
        {
            if (effectiveBurstProjectileCount <= 1 || !IsBursting)
            {
                return 1;
            }

            return Mathf.Clamp(
                effectiveBurstProjectileCount - queuedBurstShotsRemaining + 1,
                1,
                effectiveBurstProjectileCount);
        }

        /*
         * 남은 재장전 시간을 감소시킨다.
         */
        public bool ReduceReloadRemaining(float seconds)
        {
            if (seconds <= 0f || ReloadRemaining <= 0f)
            {
                return false;
            }

            ReloadRemaining = Mathf.Max(0f, ReloadRemaining - seconds);
            if (ReloadRemaining <= 0f && UsesMagazine && MagazineRemaining <= 0 && CooldownRemaining <= 0f && !IsBursting)
            {
                MagazineRemaining = MaxMagazineSize;
            }

            return true;
        }

        /*
         * 남은 재사용 대기시간을 감소시킨다.
         */
        public bool ReduceCooldownRemaining(float seconds)
        {
            if (seconds <= 0f || CooldownRemaining <= 0f)
            {
                return false;
            }

            CooldownRemaining = Mathf.Max(0f, CooldownRemaining - seconds);
            if (CooldownRemaining <= 0f && UsesMagazine && MagazineRemaining <= 0 && ReloadRemaining <= 0f && !IsBursting)
            {
                MagazineRemaining = MaxMagazineSize;
            }

            return true;
        }

        /*
         * 재사용 대기시간을 초기화한다.
         */
        public void ResetCooldown()
        {
            CooldownRemaining = 0f;
            if (UsesMagazine && MagazineRemaining <= 0 && ReloadRemaining <= 0f && !IsBursting)
            {
                MagazineRemaining = MaxMagazineSize;
            }
        }

        /*
         * 남은 시간을 0 이하로 내려가지 않게 감소시킨다.
         */
        private static float TickDown(float value, float deltaTime)
        {
            return value > 0f ? Mathf.Max(0f, value - deltaTime) : 0f;
        }

        /*
         * 다음 시전을 실행할 간격이 지났는지 확인한다.
         */
        private bool IsCastIntervalReady()
        {
            return effectiveTickInterval <= 0f || TickRemaining <= 0f;
        }

        /*
         * 현재 선택지에 맞춰 스킬 런타임 보정값을 다시 계산한다.
         */
        private void RefreshRuntimeModifiers(SkillExecutionSnapshot snapshot)
        {
            var previousMax = effectiveMaxMagazineSize;
            var nextMax = ResolveMaxMagazineSize(Data);
            var nextBurst = ResolveBurstProjectileCount(Data);
            effectiveReloadDuration = ResolveReloadDuration(Data);
            effectiveTickInterval = ResolveTickInterval(Data);
            effectiveBurstInterval = ResolveBurstInterval(Data);
            effectiveCooldownDuration = ResolveCooldownDuration(Data);

            if (snapshot != null)
            {
                nextMax = Math.Max(0, nextMax + snapshot.MagazineBonus);
                if (nextBurst > 1)
                {
                    nextBurst += snapshot.AdditionalProjectileBonus;
                }

                effectiveReloadDuration *= Mathf.Max(0f, snapshot.ReloadTimeMultiplier);
                effectiveTickInterval *= Mathf.Max(0f, snapshot.ShotIntervalMultiplier);
                effectiveBurstInterval *= Mathf.Max(0f, snapshot.ShotIntervalMultiplier);
                effectiveCooldownDuration *= Mathf.Max(0f, snapshot.CooldownMultiplier);
            }

            effectiveMaxMagazineSize = nextMax;
            effectiveBurstProjectileCount = Math.Max(1, nextBurst);
            if (previousMax == effectiveMaxMagazineSize)
            {
                return;
            }

            if (effectiveMaxMagazineSize <= 0)
            {
                MagazineRemaining = 0;
                ReloadRemaining = 0f;
                return;
            }

            if (previousMax <= 0)
            {
                MagazineRemaining = effectiveMaxMagazineSize;
                return;
            }

            var delta = effectiveMaxMagazineSize - previousMax;
            MagazineRemaining = Mathf.Clamp(MagazineRemaining + delta, 0, effectiveMaxMagazineSize);
            if (MagazineRemaining > 0)
            {
                ReloadRemaining = 0f;
            }
        }

        /*
         * 최대 탄창 크기를 결정한다.
         */
        private static int ResolveMaxMagazineSize(SkillRuntimeData data)
        {
            return data != null
                ? Math.Max(0, data.MagazineCapacity)
                : 0;
        }

        /*
         * 연속 발사 투사체 횟수를 결정한다.
         */
        private static int ResolveBurstProjectileCount(SkillRuntimeData data)
        {
            var projectile = data as ProjectileSkillRuntimeData;
            return projectile != null && projectile.Projectile != null
                ? Math.Max(1, projectile.Projectile.BurstProjectileCount)
                : 1;
        }

        /*
         * 재장전 지속시간을 결정한다.
         */
        private static float ResolveReloadDuration(SkillRuntimeData data)
        {
            return data != null
                ? Mathf.Max(0f, data.ReloadSeconds)
                : 0f;
        }

        /*
         * 주기 간격을 결정한다.
         */
        private static float ResolveTickInterval(SkillRuntimeData data)
        {
            var timing = data != null ? data.Timing : null;
            return timing != null ? Mathf.Max(0f, timing.TickInterval) : 0f;
        }

        /*
         * 연속 발사 간격을 결정한다.
         */
        private static float ResolveBurstInterval(SkillRuntimeData data)
        {
            var projectile = data as ProjectileSkillRuntimeData;
            if (projectile != null && projectile.Projectile != null)
            {
                var burstInterval = projectile.Projectile.BurstIntervalSeconds;
                if (burstInterval > 0f)
                {
                    return burstInterval;
                }
            }

            return ResolveTickInterval(data);
        }

        /*
         * 재사용 대기시간 지속시간을 결정한다.
         */
        private static float ResolveCooldownDuration(SkillRuntimeData data)
        {
            var timing = data != null ? data.Timing : null;
            return timing != null ? Mathf.Max(0f, timing.Cooldown) : 0f;
        }

        /*
         * 발사나 시전이 끝났다면 재사용 대기 또는 재장전을 시작한다.
         */
        private void BeginRecoveryIfNeeded()
        {
            if (!UsesMagazine)
            {
                CooldownRemaining = effectiveCooldownDuration;
                return;
            }

            if (MagazineRemaining > 0)
            {
                return;
            }

            CooldownRemaining = effectiveCooldownDuration;
            if (ReloadDuration > 0f)
            {
                ReloadRemaining = ReloadDuration;
                return;
            }

            if (CooldownRemaining <= 0f)
            {
                MagazineRemaining = MaxMagazineSize;
            }
        }
    }
}
