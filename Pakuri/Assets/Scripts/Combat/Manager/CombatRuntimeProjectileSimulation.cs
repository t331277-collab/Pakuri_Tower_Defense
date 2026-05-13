using UnityEngine;

namespace Pakuri.Combat
{
    public partial class CombatRuntimeController
    {
        private ProjectileSimulation projectileSimulation;

        private ProjectileSimulation ProjectileSimulationBoundary
        {
            get
            {
                if (projectileSimulation == null)
                {
                    projectileSimulation = new ProjectileSimulation(this);
                }

                return projectileSimulation;
            }
        }

        private void UpdateProjectiles()
        {
            ProjectileSimulationBoundary.Tick();
        }

        private sealed class ProjectileSimulation
        {
            private readonly CombatRuntimeController owner;

            public ProjectileSimulation(CombatRuntimeController owner)
            {
                this.owner = owner;
            }

            public void Tick()
            {
                owner.UpdateProjectilesCore();
            }

            public int LastProjectileIndex => owner.projectiles.Count - 1;

            public ProjectileRuntime GetProjectileAt(int index)
            {
                if (index < 0 || index >= owner.projectiles.Count)
                {
                    return null;
                }

                return owner.projectiles[index];
            }

            public bool RemoveMissingProjectileAt(int index, ProjectileRuntime projectile)
            {
                if (projectile != null && projectile.GameObject != null)
                {
                    return false;
                }

                if (index >= 0 && index < owner.projectiles.Count)
                {
                    owner.projectiles.RemoveAt(index);
                }

                return true;
            }

            public void TickLifetime(ProjectileRuntime projectile, float elapsedSeconds)
            {
                if (projectile == null)
                {
                    return;
                }

                projectile.RemainingLifetime = Mathf.Max(0f, projectile.RemainingLifetime - elapsedSeconds);
            }

            public bool HasRemainingLifetime(ProjectileRuntime projectile)
            {
                return projectile != null && projectile.RemainingLifetime > 0f;
            }

            public bool HasPlayerProjectileReachedBattlefieldXEdge(ProjectileRuntime projectile)
            {
                if (projectile == null || projectile.Transform == null)
                {
                    return true;
                }

                var x = projectile.Transform.position.x;
                var minX = 0f;
                var maxX = Mathf.Max(minX, owner.fieldSize.x);
                if (projectile.Direction.x < -0.01f)
                {
                    return x <= minX;
                }

                if (projectile.Direction.x > 0.01f)
                {
                    return x >= maxX;
                }

                return x <= minX || x >= maxX;
            }

            public void CleanupProjectileAt(int index)
            {
                if (index < 0 || index >= owner.projectiles.Count)
                {
                    return;
                }

                var projectile = owner.projectiles[index];
                if (projectile != null && projectile.GameObject != null)
                {
                    Object.Destroy(projectile.GameObject);
                }

                owner.projectiles.RemoveAt(index);
            }
        }
    }
}
