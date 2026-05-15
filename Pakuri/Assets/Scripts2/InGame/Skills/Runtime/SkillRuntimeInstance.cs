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

        public bool IsCasting => CastRemaining > 0f;
        public bool IsActive => ActiveDurationRemaining > 0f;
        public bool IsReloading => ReloadRemaining > 0f;
        public int MaxMagazineSize => ResolveMaxMagazineSize(Data);
        public float ReloadDuration => ResolveReloadDuration(Data);
        public bool UsesMagazine => MaxMagazineSize > 0;
        public bool HasMagazine => !UsesMagazine || MagazineRemaining > 0;
        public bool CanCast => Data != null
            && Data.IsActive
            && CooldownRemaining <= 0f
            && !IsCasting
            && !IsReloading
            && HasMagazine
            && IsCastIntervalReady();

        public void ResetRuntimeState()
        {
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

        public bool TryBeginCast()
        {
            if (!CanCast)
            {
                return false;
            }

            if (UsesMagazine)
            {
                MagazineRemaining = Math.Max(0, MagazineRemaining - 1);
            }

            var timing = Data.Timing;
            CooldownRemaining = timing != null ? Mathf.Max(0f, timing.Cooldown) : 0f;
            CastRemaining = timing != null ? Mathf.Max(0f, timing.CastTime) : 0f;
            ActiveDurationRemaining = timing != null ? Mathf.Max(0f, timing.ActiveDuration) : 0f;
            TickRemaining = timing != null ? Mathf.Max(0f, timing.TickInterval) : 0f;

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
            var timing = Data != null ? Data.Timing : null;
            TickRemaining = timing != null ? Mathf.Max(0f, timing.TickInterval) : 0f;
        }

        private static float TickDown(float value, float deltaTime)
        {
            return value > 0f ? Mathf.Max(0f, value - deltaTime) : 0f;
        }

        private bool IsCastIntervalReady()
        {
            var timing = Data != null ? Data.Timing : null;
            return timing == null || timing.TickInterval <= 0f || TickRemaining <= 0f;
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
    }
}
