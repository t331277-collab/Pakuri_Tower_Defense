using Pakuri.Data;
using UnityEngine;

namespace Pakuri.Combat
{
    public partial class CombatRuntimeController
    {
        private sealed class ManifestedDroneRuntime
        {
            public CombatUnitRuntime Source;
            public SkillDefinition Skill;
            public GameObject GameObject;
            public Transform Transform;
            public SpriteRenderer Renderer;
            public float RemainingDuration;
            public float AttackCooldownRemaining;
        }

        private void DeployManifestedEveDroneBeacon(CombatUnitRuntime runtime, SkillDefinition skill)
        {
            if (runtime == null || runtime.Transform == null || runtime.Monster == null || skill == null)
            {
                return;
            }

            var droneParent = projectileRoot != null ? projectileRoot : transform;
            var droneObject = new GameObject(string.IsNullOrWhiteSpace(skill.SkillId) ? "ManifestedDroneBeacon" : $"Manifested_{skill.SkillId}_Drone");
            droneObject.transform.SetParent(droneParent, false);
            droneObject.transform.position = runtime.Transform.position + new Vector3(0.45f, 0.45f, 0f);
            droneObject.transform.localScale = Vector3.one * 0.42f;

            var renderer = droneObject.AddComponent<SpriteRenderer>();
            renderer.sprite = runtime.Monster.UnitSprite != null ? runtime.Monster.UnitSprite : GetSharedSprite();
            renderer.color = runtime.Monster.ProjectileColor.a <= 0f ? new Color(0.75f, 0.95f, 1f, 0.85f) : runtime.Monster.ProjectileColor;
            renderer.sortingOrder = 26;

            manifestedDrones.Add(new ManifestedDroneRuntime
            {
                Source = runtime,
                Skill = skill,
                GameObject = droneObject,
                Transform = droneObject.transform,
                Renderer = renderer,
                RemainingDuration = ResolveManifestedSkillVisualDuration(runtime, skill),
                AttackCooldownRemaining = 0f
            });

            statusLabel = $"{runtime.Monster.DisplayName} {skill.DisplayName} drone deployed.";
        }

        private void UpdateManifestedDrones()
        {
            var elapsed = Time.deltaTime;
            for (var i = manifestedDrones.Count - 1; i >= 0; i--)
            {
                var drone = manifestedDrones[i];
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

                var target = FindNearestManifestedMonsterTarget(drone.Transform.position);
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

                FireManifestedMonsterProjectile(drone.Source, drone.Skill, drone.Transform.position, direction, 1f, 0, 1);
                drone.AttackCooldownRemaining = EveDroneAttackPeriod;
            }
        }

        private void RemoveManifestedDroneAt(int index)
        {
            if (index < 0 || index >= manifestedDrones.Count)
            {
                return;
            }

            var drone = manifestedDrones[index];
            if (drone != null && drone.GameObject != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(drone.GameObject);
                }
                else
                {
                    DestroyImmediate(drone.GameObject);
                }
            }

            manifestedDrones.RemoveAt(index);
        }
    }
}
