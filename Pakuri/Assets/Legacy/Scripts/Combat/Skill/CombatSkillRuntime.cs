using Pakuri.Data;
using UnityEngine;

namespace Pakuri.Combat
{
    public sealed class CombatSkillRuntime
    {
        public SkillDefinition Skill;
        public float CooldownRemaining;
        public float CooldownDuration;
        public int ShotsRemaining;
        public int MagazineCapacity;
        public float ShotCooldownRemaining;
        public float ShotInterval;
        public float ReloadRemaining;
        public float ReloadDuration;
        public int PendingVegaProjectileCount;
        public int PendingVegaProjectileIndex;
        public float PendingVegaProjectileDelay;
        public Vector3 PendingVegaProjectileDirection;

        public void Configure(
            SkillDefinition skill,
            float cooldownDuration,
            float cooldownRemaining,
            int magazineCapacity,
            int shotsRemaining,
            float shotInterval,
            float shotCooldownRemaining,
            float reloadDuration,
            float reloadRemaining)
        {
            Skill = skill;
            CooldownDuration = Mathf.Max(0f, cooldownDuration);
            CooldownRemaining = Mathf.Max(0f, cooldownRemaining);
            MagazineCapacity = Mathf.Max(0, magazineCapacity);
            ShotsRemaining = Mathf.Max(0, shotsRemaining);
            ShotInterval = Mathf.Max(0f, shotInterval);
            ShotCooldownRemaining = Mathf.Max(0f, shotCooldownRemaining);
            ReloadDuration = Mathf.Max(0f, reloadDuration);
            ReloadRemaining = Mathf.Max(0f, reloadRemaining);
            PendingVegaProjectileCount = 0;
            PendingVegaProjectileIndex = 0;
            PendingVegaProjectileDelay = 0f;
            PendingVegaProjectileDirection = Vector3.zero;
        }

        public void Tick(float elapsed)
        {
            elapsed = Mathf.Max(0f, elapsed);
            CooldownRemaining = Mathf.Max(0f, CooldownRemaining - elapsed);
            ShotCooldownRemaining = Mathf.Max(0f, ShotCooldownRemaining - elapsed);
        }

        public void TickReload(float elapsed, int reloadMagazineCapacity)
        {
            if (ReloadRemaining <= 0f)
            {
                return;
            }

            ReloadRemaining = Mathf.Max(0f, ReloadRemaining - Mathf.Max(0f, elapsed));
            if (Mathf.Approximately(ReloadRemaining, 0f))
            {
                ShotsRemaining = Mathf.Max(1, reloadMagazineCapacity);
            }
        }
    }
}
