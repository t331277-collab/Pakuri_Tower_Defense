using System;
using UnityEngine;

namespace Pakuri.InGame
{
    public sealed class SkillRuntimeInstance
    {
        public SkillRuntimeInstance(BaseUnitRuntimeModel owner, SkillData data)
        {
            Owner = owner;
            Data = data;
            ResetRuntimeState();
        }

        public BaseUnitRuntimeModel Owner { get; }
        public SkillData Data { get; }
        public string SkillId => Data != null ? Data.SkillId : string.Empty;
        public InGameSkillSlot Slot => Data != null ? Data.Slot : default;
        public float CooldownRemaining { get; private set; }
        public float CastRemaining { get; private set; }
        public float ActiveDurationRemaining { get; private set; }
        public float TickRemaining { get; private set; }
        public float ReloadRemaining { get; private set; }
        public int MagazineRemaining { get; private set; }

        private int effectiveMaxMagazineSize;
        private float effectiveReloadDuration;
        private float effectiveTickInterval;
        private float effectiveCooldownDuration;

        public bool IsCasting => CastRemaining > 0f;
        public bool IsActive => ActiveDurationRemaining > 0f;
        public bool IsReloading => ReloadRemaining > 0f;
        public int MaxMagazineSize => effectiveMaxMagazineSize;
        public float ReloadDuration => effectiveReloadDuration;
        public bool UsesMagazine => MaxMagazineSize > 0;
        public bool HasMagazine => !UsesMagazine || MagazineRemaining > 0;
        public bool CanCast => CanCastWithSnapshot(null);

        public void ResetRuntimeState()
        {
            effectiveMaxMagazineSize = ResolveMaxMagazineSize(Data);
            effectiveReloadDuration = ResolveReloadDuration(Data);
            effectiveTickInterval = ResolveTickInterval(Data);
            effectiveCooldownDuration = ResolveCooldownDuration(Data);
            CooldownRemaining = 0f;
            CastRemaining = 0f;
            ActiveDurationRemaining = 0f;
            TickRemaining = 0f;
            ReloadRemaining = 0f;
            MagazineRemaining = MaxMagazineSize;
        }

        public void Tick(float deltaTime)
        {
            if (deltaTime <= 0f)
            {
                return;
            }

            CooldownRemaining = TickDown(CooldownRemaining, deltaTime);
            CastRemaining = TickDown(CastRemaining, deltaTime);
            ActiveDurationRemaining = TickDown(ActiveDurationRemaining, deltaTime);
            TickRemaining = TickDown(TickRemaining, deltaTime);
            ReloadRemaining = TickDown(ReloadRemaining, deltaTime);

            if (UsesMagazine && MagazineRemaining <= 0 && ReloadRemaining <= 0f)
            {
                MagazineRemaining = MaxMagazineSize;
            }
        }

        public bool CanCastWithSnapshot(SkillExecutionSnapshot snapshot)
        {
            RefreshRuntimeModifiers(snapshot);
            return Data != null
                && Data.IsActive
                && CooldownRemaining <= 0f
                && !IsCasting
                && !IsReloading
                && HasMagazine
                && IsCastIntervalReady();
        }

        public bool TryBeginCast()
        {
            return TryBeginCast(null);
        }

        public bool TryBeginCast(SkillExecutionSnapshot snapshot)
        {
            if (!CanCastWithSnapshot(snapshot))
            {
                return false;
            }

            if (UsesMagazine)
            {
                MagazineRemaining = Math.Max(0, MagazineRemaining - 1);
            }

            var timing = Data.Timing;
            CooldownRemaining = effectiveCooldownDuration;
            CastRemaining = timing != null ? Mathf.Max(0f, timing.CastTime) : 0f;
            ActiveDurationRemaining = timing != null ? Mathf.Max(0f, timing.ActiveDuration) : 0f;
            TickRemaining = effectiveTickInterval;

            if (UsesMagazine && MagazineRemaining <= 0)
            {
                var reload = ReloadDuration;
                if (reload > 0f)
                {
                    ReloadRemaining = reload;
                }
                else
                {
                    MagazineRemaining = MaxMagazineSize;
                }
            }

            return true;
        }

        public bool IsTickReady()
        {
            var timing = Data != null ? Data.Timing : null;
            return timing != null && timing.TickInterval > 0f && TickRemaining <= 0f;
        }

        public void ResetTickInterval()
        {
            TickRemaining = effectiveTickInterval;
        }

        private static float TickDown(float value, float deltaTime)
        {
            return value > 0f ? Mathf.Max(0f, value - deltaTime) : 0f;
        }

        private bool IsCastIntervalReady()
        {
            return effectiveTickInterval <= 0f || TickRemaining <= 0f;
        }

        private void RefreshRuntimeModifiers(SkillExecutionSnapshot snapshot)
        {
            var previousMax = effectiveMaxMagazineSize;
            var nextMax = ResolveMaxMagazineSize(Data);
            effectiveReloadDuration = ResolveReloadDuration(Data);
            effectiveTickInterval = ResolveTickInterval(Data);
            effectiveCooldownDuration = ResolveCooldownDuration(Data);

            if (snapshot != null)
            {
                nextMax = Math.Max(0, nextMax + snapshot.MagazineBonus);
                effectiveReloadDuration *= Mathf.Max(0f, snapshot.ReloadTimeMultiplier);
                effectiveTickInterval *= Mathf.Max(0f, snapshot.ShotIntervalMultiplier);
                effectiveCooldownDuration *= Mathf.Max(0f, snapshot.CooldownMultiplier);
            }

            effectiveMaxMagazineSize = nextMax;
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

        private static int ResolveMaxMagazineSize(SkillData data)
        {
            var projectile = data as ProjectileSkillData;
            return projectile != null && projectile.Projectile != null
                ? Math.Max(0, projectile.Projectile.MagazineSize)
                : 0;
        }

        private static float ResolveReloadDuration(SkillData data)
        {
            var projectile = data as ProjectileSkillData;
            return projectile != null && projectile.Projectile != null
                ? Mathf.Max(0f, projectile.Projectile.ReloadTime)
                : 0f;
        }

        private static float ResolveTickInterval(SkillData data)
        {
            var timing = data != null ? data.Timing : null;
            return timing != null ? Mathf.Max(0f, timing.TickInterval) : 0f;
        }

        private static float ResolveCooldownDuration(SkillData data)
        {
            var timing = data != null ? data.Timing : null;
            return timing != null ? Mathf.Max(0f, timing.Cooldown) : 0f;
        }
    }
}
