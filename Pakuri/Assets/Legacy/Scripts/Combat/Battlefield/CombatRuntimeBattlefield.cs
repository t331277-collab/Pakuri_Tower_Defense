using System.Collections.Generic;

namespace Pakuri.Combat
{
    public partial class CombatRuntimeController
    {
        private CombatBattlefieldState battlefieldState;

        private CombatBattlefieldState BattlefieldState
        {
            get
            {
                if (battlefieldState == null)
                {
                    battlefieldState = new CombatBattlefieldState(enemies, projectiles, skillEffects, drones);
                }

                return battlefieldState;
            }
        }

        private void AddBattlefieldEnemy(EnemyRuntime enemy)
        {
            BattlefieldState.AddEnemy(enemy);
        }

        private void AddBattlefieldProjectile(ProjectileRuntime projectile)
        {
            BattlefieldState.AddProjectile(projectile);
        }

        private void AddBattlefieldSkillEffect(SkillEffectRuntime effect)
        {
            BattlefieldState.AddSkillEffect(effect);
        }

        private void AddBattlefieldDrone(DroneRuntime drone)
        {
            BattlefieldState.AddDrone(drone);
        }

        private sealed class CombatBattlefieldState
        {
            private readonly List<EnemyRuntime> enemies;
            private readonly List<ProjectileRuntime> projectiles;
            private readonly List<SkillEffectRuntime> skillEffects;
            private readonly List<DroneRuntime> drones;

            public CombatBattlefieldState(
                List<EnemyRuntime> enemies,
                List<ProjectileRuntime> projectiles,
                List<SkillEffectRuntime> skillEffects,
                List<DroneRuntime> drones)
            {
                this.enemies = enemies;
                this.projectiles = projectiles;
                this.skillEffects = skillEffects;
                this.drones = drones;
            }

            public void AddEnemy(EnemyRuntime enemy)
            {
                enemies.Add(enemy);
            }

            public void AddProjectile(ProjectileRuntime projectile)
            {
                projectiles.Add(projectile);
            }

            public void AddSkillEffect(SkillEffectRuntime effect)
            {
                skillEffects.Add(effect);
            }

            public void AddDrone(DroneRuntime drone)
            {
                drones.Add(drone);
            }
        }
    }
}
