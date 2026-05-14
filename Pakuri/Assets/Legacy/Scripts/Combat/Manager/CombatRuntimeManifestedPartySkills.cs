namespace Pakuri.Combat
{
    public partial class CombatRuntimeController
    {
        internal void TickManifestedUnitSkill(CombatUnitRuntime runtime, CombatSkillRuntime skillRuntime, float elapsed)
        {
            manifestedParty.TickUnitSkill(this, runtime, skillRuntime, elapsed);
        }

        private void DispatchManifestedPartyUnitSkill(CombatUnitRuntime runtime, CombatSkillRuntime skillRuntime, float elapsed)
        {
            if (runtime == null || runtime.Transform == null || skillRuntime == null || skillRuntime.Skill == null)
            {
                return;
            }

            if (TryTickEveUnitSkill(runtime, skillRuntime, elapsed))
            {
                return;
            }

            if (TryTickRinUnitSkill(runtime, skillRuntime, elapsed))
            {
                return;
            }

            if (TryTickSeinUnitSkill(runtime, skillRuntime, elapsed))
            {
                return;
            }

            if (TryTickVegaUnitSkill(runtime, skillRuntime, elapsed))
            {
                return;
            }

            if (TryTickArielUnitSkill(runtime, skillRuntime, elapsed))
            {
                return;
            }

            TickCombatSkillRuntime(runtime, skillRuntime, elapsed);
            if (IsManifestedMagazineSkill(skillRuntime.Skill))
            {
                TryFireManifestedMagazineSkill(runtime, skillRuntime);
                return;
            }

            if (skillRuntime.CooldownRemaining > 0f)
            {
                return;
            }

            var target = FindNearestManifestedMonsterTarget(runtime.Transform.position);
            if (target == null)
            {
                skillRuntime.CooldownRemaining = 0.25f;
                return;
            }

            if (IsManifestedProjectileSkill(skillRuntime.Skill))
            {
                FireManifestedMonsterProjectile(runtime, skillRuntime.Skill, target);
            }
            else
            {
                FireManifestedMonsterSkill(runtime, skillRuntime, target);
            }

            skillRuntime.CooldownDuration = ResolveManifestedSkillCooldown(runtime, skillRuntime.Skill);
            skillRuntime.CooldownRemaining = skillRuntime.CooldownDuration;
        }

        private void TickCombatSkillRuntime(CombatUnitRuntime runtime, CombatSkillRuntime skillRuntime, float elapsed)
        {
            if (skillRuntime == null)
            {
                return;
            }

            skillRuntime.Tick(elapsed);
            UpdateManifestedQueuedProjectiles(runtime, skillRuntime, elapsed);
            skillRuntime.TickReload(elapsed, ResolveManifestedMagazineCapacity(runtime, skillRuntime.Skill));
        }

        private void TryFireManifestedMagazineSkill(CombatUnitRuntime runtime, CombatSkillRuntime skillRuntime)
        {
            if (runtime == null || runtime.Transform == null || skillRuntime == null || skillRuntime.Skill == null)
            {
                return;
            }

            if (skillRuntime.ReloadRemaining > 0f || skillRuntime.ShotCooldownRemaining > 0f)
            {
                return;
            }

            if (skillRuntime.ShotsRemaining <= 0)
            {
                skillRuntime.ShotsRemaining = 0;
                skillRuntime.ReloadDuration = ResolveManifestedReloadDuration(runtime, skillRuntime.Skill);
                skillRuntime.ReloadRemaining = skillRuntime.ReloadDuration;
                return;
            }

            var target = FindNearestManifestedMonsterTarget(runtime.Transform.position);
            if (target == null)
            {
                skillRuntime.ShotCooldownRemaining = 0.25f;
                return;
            }

            if (IsManifestedVegaThreeSwordFlurry(skillRuntime.Skill))
            {
                QueueManifestedVegaThreeSwordFlurry(runtime, skillRuntime, target);
            }
            else if (IsManifestedEveDroneBeacon(skillRuntime.Skill))
            {
                DeployManifestedEveDroneBeacon(runtime, skillRuntime.Skill);
            }
            else if (IsManifestedProjectileSkill(skillRuntime.Skill))
            {
                FireManifestedMonsterProjectile(runtime, skillRuntime.Skill, target);
            }
            else
            {
                FireManifestedMonsterSkill(runtime, skillRuntime, target);
            }

            skillRuntime.ShotsRemaining -= 1;
            skillRuntime.ShotInterval = ResolveManifestedShotInterval(runtime, skillRuntime.Skill);
            skillRuntime.ShotCooldownRemaining = skillRuntime.ShotInterval;
            if (skillRuntime.ShotsRemaining <= 0)
            {
                skillRuntime.ShotsRemaining = 0;
                skillRuntime.ReloadDuration = ResolveManifestedReloadDuration(runtime, skillRuntime.Skill);
                skillRuntime.ReloadRemaining = skillRuntime.ReloadDuration;
            }
        }
    }
}
