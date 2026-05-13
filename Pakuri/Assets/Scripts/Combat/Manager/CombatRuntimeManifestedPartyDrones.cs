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

    }
}
