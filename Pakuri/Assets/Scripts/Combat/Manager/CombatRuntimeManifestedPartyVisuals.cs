using System;
using Pakuri.Data;
using UnityEngine;

namespace Pakuri.Combat
{
    public partial class CombatRuntimeController
    {
        private void CreateManifestedSkillVisual(CombatUnitRuntime runtime, SkillDefinition skill, EnemyRuntime target)
        {
            if (runtime == null || runtime.Transform == null || skill == null || target == null || target.Transform == null)
            {
                return;
            }

            var origin = runtime.Transform.position;
            var targetPosition = target.Transform.position;
            var duration = ResolveManifestedSkillVisualDuration(runtime, skill);
            switch (skill.RuntimeKind)
            {
                case SkillRuntimeKind.AreaAttack:
                case SkillRuntimeKind.Field:
                    CreateManifestedCircleVisual(
                        skill,
                        targetPosition,
                        Mathf.Max(0.75f, skill.Radius > 0f ? skill.Radius : GetEnemyHitRadius(target) + 0.35f),
                        new Color(1f, 1f, 1f, 0.58f),
                        23,
                        duration);
                    return;
                case SkillRuntimeKind.Buff:
                case SkillRuntimeKind.Shield:
                    CreateManifestedCircleVisual(
                        skill,
                        origin,
                        Mathf.Max(0.75f, skill.Radius > 0f ? skill.Radius : 0.9f),
                        new Color(0.78f, 0.95f, 1f, 0.56f),
                        24,
                        duration);
                    return;
                case SkillRuntimeKind.Execute:
                case SkillRuntimeKind.Mark:
                    CreateManifestedCircleVisual(
                        skill,
                        targetPosition,
                        Mathf.Max(0.65f, GetEnemyHitRadius(target) + 0.35f),
                        new Color(0.92f, 0.82f, 1f, 0.58f),
                        25,
                        duration);
                    return;
                case SkillRuntimeKind.LineAttack:
                    CreateManifestedLineSkillVisual(origin, targetPosition, skill, Mathf.Max(0.08f, skill.Radius > 0f ? skill.Radius : 0.28f), duration);
                    return;
                default:
                    CreateManifestedLineSkillVisual(origin, targetPosition, skill, 0.08f, duration);
                    return;
            }
        }

        private float ResolveManifestedSkillVisualDuration(CombatUnitRuntime runtime, SkillDefinition skill)
        {
            if (skill == null)
            {
                return ManifestedMonsterProjectileLifetime;
            }

            if (string.Equals(skill.SkillId, "eve-b", StringComparison.OrdinalIgnoreCase))
            {
                return EveBeamDuration;
            }

            if (string.Equals(skill.SkillId, "eve-c", StringComparison.OrdinalIgnoreCase))
            {
                return EveFrostFieldDuration;
            }

            if (string.Equals(skill.SkillId, "eve-e", StringComparison.OrdinalIgnoreCase))
            {
                return EveDroneDuration;
            }

            if (string.Equals(skill.SkillId, "sein-d", StringComparison.OrdinalIgnoreCase))
            {
                return SeinSuperheatedZoneDuration;
            }

            if (string.Equals(skill.SkillId, "vega-c", StringComparison.OrdinalIgnoreCase))
            {
                return VegaExterminationPermitDuration;
            }

            if (string.Equals(skill.SkillId, "ariel-b", StringComparison.OrdinalIgnoreCase))
            {
                return ArielRadiantShieldDuration;
            }

            if (string.Equals(skill.SkillId, "ariel-c", StringComparison.OrdinalIgnoreCase))
            {
                return ArielBlessingDuration;
            }

            switch (skill.RuntimeKind)
            {
                case SkillRuntimeKind.Field:
                    return 4f;
                case SkillRuntimeKind.Buff:
                case SkillRuntimeKind.Shield:
                    return 4f;
                case SkillRuntimeKind.LineAttack:
                    return 0.35f;
                case SkillRuntimeKind.AreaAttack:
                case SkillRuntimeKind.Execute:
                case SkillRuntimeKind.Mark:
                    return 0.28f;
                default:
                    return ManifestedMonsterProjectileLifetime;
            }
        }

        private void CreateManifestedCircleVisual(SkillDefinition skill, Vector3 position, float radius, Color color, int sortingOrder, float duration)
        {
            var effect = CombatEffectFactory.CreateCircle(
                string.IsNullOrWhiteSpace(skill.SkillId) ? "ManifestedMonsterSkillArea" : $"Manifested_{skill.SkillId}_Area",
                projectileRoot != null ? projectileRoot : transform,
                position,
                Mathf.Max(0.05f, radius),
                skill.SkillEffectPrefab,
                GetCircleSprite());
            ConfigureManifestedVisual(effect, color, sortingOrder, duration);
        }

        private void CreateManifestedLineSkillVisual(Vector3 origin, Vector3 target, SkillDefinition skill, float width, float duration)
        {
            var direction = target - origin;
            direction.z = 0f;
            if (direction.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            var distance = direction.magnitude;
            var effect = CombatEffectFactory.CreateLine(
                string.IsNullOrWhiteSpace(skill.SkillId) ? "ManifestedMonsterSkillLine" : $"Manifested_{skill.SkillId}_Line",
                projectileRoot != null ? projectileRoot : transform,
                origin,
                direction,
                distance,
                Mathf.Max(0.05f, width),
                skill.SkillEffectPrefab,
                GetSharedSprite());
            ConfigureManifestedVisual(effect, Color.white, 23, duration);
        }

        private void ConfigureManifestedVisual(CombatEffectInstance effect, Color color, int sortingOrder, float duration)
        {
            if (effect.Renderer != null)
            {
                effect.Renderer.color = color;
                effect.Renderer.sortingOrder = sortingOrder;
            }

            if (effect.GameObject != null)
            {
                Destroy(effect.GameObject, Mathf.Max(0.05f, duration));
            }
        }
    }
}
