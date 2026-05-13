using UnityEngine;

namespace Pakuri.Combat
{
    public partial class CombatRuntimeController
    {
        private DroneSimulation droneSimulation;

        private DroneSimulation DroneSimulationBoundary
        {
            get
            {
                if (droneSimulation == null)
                {
                    droneSimulation = new DroneSimulation(this);
                }

                return droneSimulation;
            }
        }

        private void UpdateDrones()
        {
            DroneSimulationBoundary.TickSelectedDrones();
        }

        private void UpdateManifestedDrones()
        {
            DroneSimulationBoundary.TickManifestedDrones();
        }

        private void RemoveManifestedDroneAt(int index)
        {
            DroneSimulationBoundary.RemoveManifestedDroneAt(index);
        }

        private sealed class DroneSimulation
        {
            private readonly CombatRuntimeController owner;

            public DroneSimulation(CombatRuntimeController owner)
            {
                this.owner = owner;
            }

            public void TickSelectedDrones()
            {
                for (var i = owner.drones.Count - 1; i >= 0; i--)
                {
                    var drone = owner.drones[i];
                    if (drone == null || drone.GameObject == null)
                    {
                        owner.drones.RemoveAt(i);
                        continue;
                    }

                    drone.RemainingDuration = Mathf.Max(0f, drone.RemainingDuration - Time.deltaTime);
                    drone.AttackRemaining = Mathf.Max(0f, drone.AttackRemaining - Time.deltaTime);
                    if (drone.AttackRemaining <= 0f)
                    {
                        FireDroneProjectile(drone);
                        drone.AttackRemaining = drone.AttackPeriod;
                    }

                    if (drone.RemainingDuration > 0f)
                    {
                        continue;
                    }

                    Object.Destroy(drone.GameObject);
                    owner.drones.RemoveAt(i);
                }
            }

            private void FireDroneProjectile(DroneRuntime drone)
            {
                var target = owner.FindNearestEnemy(drone.Transform.position, drone.Range);
                if (target == null)
                {
                    return;
                }

                var direction = target.Transform.position - drone.Transform.position;
                direction.z = 0f;
                if (direction.sqrMagnitude < 0.01f)
                {
                    direction = Vector3.right;
                }

                direction.Normalize();
                owner.nextProjectileSequence += 1;
                var projectileObject = new GameObject($"DroneShot_{owner.nextProjectileSequence:00}");
                projectileObject.transform.SetParent(owner.projectileRoot, false);
                projectileObject.transform.position = drone.Transform.position;
                projectileObject.transform.localScale = new Vector3(0.24f, 0.24f, 1f);

                var renderer = projectileObject.AddComponent<SpriteRenderer>();
                renderer.sprite = owner.selectedProjectileSprite != null ? owner.selectedProjectileSprite : GetSharedSprite();
                renderer.color = Color.white;
                renderer.sortingOrder = 25;

                owner.AddBattlefieldProjectile(new ProjectileRuntime
                {
                    GameObject = projectileObject,
                    Transform = projectileObject.transform,
                    Renderer = renderer,
                    Direction = direction,
                    Speed = 12f,
                    RemainingLifetime = 2f,
                    HitRadius = 0.28f,
                    BaseDamage = drone.BaseDamage,
                    Attribute = drone.Attribute,
                    SkillId = "eve-e",
                    StatusStacks = drone.VulnerableStacks
                });
            }

            public void TickManifestedDrones()
            {
                var elapsed = Time.deltaTime;
                for (var i = owner.manifestedDrones.Count - 1; i >= 0; i--)
                {
                    var drone = owner.manifestedDrones[i];
                    if (drone == null || drone.Transform == null || drone.GameObject == null || drone.Source == null || drone.Source.CurrentHealth <= 0f)
                    {
                        RemoveManifestedDroneAt(i);
                        continue;
                    }

                    drone.RemainingDuration -= elapsed;
                    if (drone.RemainingDuration <= 0f)
                    {
                        RemoveManifestedDroneAt(i);
                        continue;
                    }

                    drone.AttackCooldownRemaining = Mathf.Max(0f, drone.AttackCooldownRemaining - elapsed);
                    if (drone.AttackCooldownRemaining > 0f)
                    {
                        continue;
                    }

                    var target = owner.FindNearestManifestedMonsterTarget(drone.Transform.position);
                    if (target == null || target.Transform == null)
                    {
                        drone.AttackCooldownRemaining = 0.2f;
                        continue;
                    }

                    var direction = target.Transform.position - drone.Transform.position;
                    direction.z = 0f;
                    if (direction.sqrMagnitude <= 0.0001f)
                    {
                        direction = Vector3.right;
                    }

                    owner.FireManifestedMonsterProjectile(drone.Source, drone.Skill, drone.Transform.position, direction, 1f, 0, 1);
                    drone.AttackCooldownRemaining = EveDroneAttackPeriod;
                }
            }

            public void RemoveManifestedDroneAt(int index)
            {
                if (index < 0 || index >= owner.manifestedDrones.Count)
                {
                    return;
                }

                var drone = owner.manifestedDrones[index];
                if (drone != null && drone.GameObject != null)
                {
                    if (Application.isPlaying)
                    {
                        Object.Destroy(drone.GameObject);
                    }
                    else
                    {
                        Object.DestroyImmediate(drone.GameObject);
                    }
                }

                owner.manifestedDrones.RemoveAt(index);
            }
        }
    }
}
