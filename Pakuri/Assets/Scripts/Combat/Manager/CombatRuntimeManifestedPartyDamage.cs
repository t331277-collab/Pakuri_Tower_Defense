using System;
using Pakuri.Data;
using UnityEngine;

namespace Pakuri.Combat
{
    public partial class CombatRuntimeController
    {
        private void FireManifestedMonsterSkill(CombatUnitRuntime runtime, CombatSkillRuntime skillRuntime, EnemyRuntime target)
        {
            var skill = skillRuntime != null ? skillRuntime.Skill : null;
            if (runtime == null || skill == null || target == null || runtime.Transform == null || target.Transform == null)
            {
                return;
            }

            if (TryFireManifestedRinShockwave(runtime, skillRuntime, target))
            {
                return;
            }

            if (TryFireManifestedPersistentSkill(runtime, skill, target))
            {
                return;
            }

            if (skill.RuntimeKind == SkillRuntimeKind.Buff || skill.RuntimeKind == SkillRuntimeKind.Shield)
            {
                CreateManifestedSkillVisual(runtime, skill, target);
                statusLabel = $"{runtime.Monster.DisplayName} {skill.DisplayName} activated.";
                return;
            }

            var radius = Mathf.Max(0f, skill.Radius);
            var appliedTotal = 0f;
            if (radius > 0f && (skill.RuntimeKind == SkillRuntimeKind.AreaAttack || skill.RuntimeKind == SkillRuntimeKind.Field))
            {
                for (var i = 0; i < enemies.Count; i++)
                {
                    var enemy = enemies[i];
                    if (enemy == null || enemy.Transform == null || enemy.CurrentHealth <= 0f)
                    {
                        continue;
                    }

                    if (Vector2.Distance(target.Transform.position, enemy.Transform.position) > radius)
                    {
                        continue;
                    }

                    appliedTotal += ApplyManifestedSkillDamage(runtime, skill, enemy);
                }
            }
            else
            {
                appliedTotal = ApplyManifestedSkillDamage(runtime, skill, target);
            }

            CreateManifestedSkillVisual(runtime, skill, target);
            statusLabel = $"{runtime.Monster.DisplayName} {skill.DisplayName} hit for {appliedTotal:0.#}.";
        }

        private void FireManifestedMonsterProjectile(CombatUnitRuntime runtime, SkillDefinition skill, EnemyRuntime target)
        {
            if (runtime == null || runtime.Monster == null || skill == null || target == null || runtime.Transform == null || target.Transform == null)
            {
                return;
            }

            var direction = target.Transform.position - runtime.Transform.position;
            direction.z = 0f;
            if (direction.sqrMagnitude <= 0.0001f)
            {
                direction = Vector3.right;
            }

            direction.Normalize();
            FireManifestedMonsterProjectile(runtime, skill, runtime.Transform.position, direction, 1f, ResolveManifestedProjectilePierce(runtime, skill), 0);
        }

        private int ResolveManifestedProjectilePierce(CombatUnitRuntime runtime, SkillDefinition skill)
        {
            if (skill == null)
            {
                return 0;
            }

            var skillId = skill.SkillId ?? string.Empty;
            if (string.Equals(skillId, "ariel-a", StringComparison.OrdinalIgnoreCase))
            {
                var pierce = 1;
                pierce += HasManifestedChoice(runtime, "ariel-a-trait-4") ? 1 : 0;
                return Mathf.Max(0, pierce);
            }

            if (string.Equals(skillId, "sein-a", StringComparison.OrdinalIgnoreCase))
            {
                var pierce = 1;
                pierce += HasManifestedChoice(runtime, "sein-a-trait-4") ? 1 : 0;
                pierce += HasManifestedChoice(runtime, "sein-a-master-1") ? 1 : 0;
                return Mathf.Max(0, pierce);
            }

            if (string.Equals(skillId, "rin-a", StringComparison.OrdinalIgnoreCase))
            {
                return HasManifestedChoice(runtime, "rin-a-trait-4") ? 1 : 0;
            }

            return 0;
        }

        private void FireManifestedMonsterProjectile(
            CombatUnitRuntime runtime,
            SkillDefinition skill,
            Vector3 direction,
            float damageMultiplier,
            int remainingPierce,
            int nameMarkStacks)
        {
            var origin = runtime != null && runtime.Transform != null ? runtime.Transform.position : Vector3.zero;
            FireManifestedMonsterProjectile(runtime, skill, origin, direction, damageMultiplier, remainingPierce, nameMarkStacks);
        }

        private void FireManifestedMonsterProjectile(
            CombatUnitRuntime runtime,
            SkillDefinition skill,
            Vector3 origin,
            Vector3 direction,
            float damageMultiplier,
            int remainingPierce,
            int nameMarkStacks)
        {
            if (runtime == null || runtime.Monster == null || skill == null || runtime.Transform == null)
            {
                return;
            }

            direction.z = 0f;
            if (direction.sqrMagnitude <= 0.0001f)
            {
                direction = Vector3.right;
            }

            direction.Normalize();
            var projectileParent = projectileRoot != null ? projectileRoot : transform;
            var projectileObject = new GameObject(string.IsNullOrWhiteSpace(skill.SkillId) ? "ManifestedProjectile" : $"Manifested_{skill.SkillId}_Projectile");
            projectileObject.transform.SetParent(projectileParent, false);
            projectileObject.transform.position = origin;
            projectileObject.transform.localScale = Vector3.one * 0.35f;
            projectileObject.transform.right = direction;

            var renderer = projectileObject.AddComponent<SpriteRenderer>();
            renderer.sprite = runtime.Monster.ProjectileSprite != null ? runtime.Monster.ProjectileSprite : GetSharedSprite();
            renderer.color = runtime.Monster.ProjectileColor.a <= 0f ? Color.white : runtime.Monster.ProjectileColor;
            renderer.sortingOrder = 24;

            AddBattlefieldProjectile(new ProjectileRuntime
            {
                GameObject = projectileObject,
                Transform = projectileObject.transform,
                Renderer = renderer,
                Direction = direction,
                Speed = ResolveManifestedProjectileSpeed(runtime),
                RemainingLifetime = ResolveManifestedProjectileLifetime(runtime, skill),
                HitRadius = ResolveManifestedProjectileHitRadius(runtime),
                BaseDamage = ResolveManifestedBaseDamage(runtime, skill) * Mathf.Max(0f, damageMultiplier),
                Attribute = skill.Attribute,
                SkillId = skill.SkillId,
                RemainingPierce = Mathf.Max(0, remainingPierce),
                StatusStacks = 1,
                StatusChance = ResolveManifestedStatusChance(runtime),
                VegaNameMarkStacks = IsManifestedVegaThreeSwordFlurry(skill) ? Mathf.Max(0, nameMarkStacks) : 0,
                IsManifestedProjectile = true,
                ManifestedSource = runtime,
                ManifestedSourceName = runtime.Monster.DisplayName,
                ManifestedSkillName = skill.DisplayName,
                ManifestedElementLabel = runtime.Monster.ElementLabel,
                ManifestedStatusEffectId = skill.StatusEffectId
            });

            statusLabel = $"{runtime.Monster.DisplayName} {skill.DisplayName} projectile fired.";
        }

        private bool TryHitManifestedProjectile(ProjectileRuntime projectile, out EnemyRuntime enemyHit, out DamageResult damageResult, out float appliedDamage)
        {
            enemyHit = null;
            damageResult = default;
            appliedDamage = 0f;
            if (projectile == null)
            {
                return false;
            }

            for (var i = 0; i < enemies.Count; i++)
            {
                var enemy = enemies[i];
                if (enemy == null || enemy.Transform == null || enemy.CurrentHealth <= 0f || projectile.HitEnemies.Contains(enemy))
                {
                    continue;
                }

                var hitDistance = GetEnemyHitRadius(enemy) + projectile.HitRadius;
                if (Vector2.Distance(projectile.Transform.position, enemy.Transform.position) > hitDistance)
                {
                    continue;
                }

                enemyHit = enemy;
                if (!TryApplyRinUnitProjectileHit(projectile, enemy, out damageResult, out appliedDamage)
                    && !TryApplySeinUnitProjectileHit(projectile, enemy, out damageResult, out appliedDamage)
                    && !TryApplyVegaUnitProjectileHit(projectile, enemy, out damageResult, out appliedDamage)
                    && !TryApplyArielUnitProjectileHit(projectile, enemy, out damageResult, out appliedDamage))
                {
                    damageResult = DamageCalculator.Resolve(
                        projectile.BaseDamage,
                        projectile.Attribute,
                        enemy.Defenses,
                        targetCriticalResistance: enemy.CriticalResistance,
                        finalDamageMultiplier: enemy.DamageTakenMultiplier);
                    appliedDamage = ApplyDamageToEnemy(enemy, damageResult.FinalDamage, damageResult.Attribute);
                }

                ApplyManifestedProjectileStatus(projectile, enemy);
                TryApplyProjectileBranch(projectile, enemy, damageResult.FinalDamage);
                ApplyManifestedProjectileSourceEffects(projectile, enemy, appliedDamage);
                if (projectile.VegaNameMarkStacks > 0)
                {
                    AddVegaNameMarks(enemy, projectile.VegaNameMarkStacks);
                }
                return true;
            }

            return false;
        }

        private void ApplyManifestedProjectileSourceEffects(ProjectileRuntime projectile, EnemyRuntime enemy, float appliedDamage)
        {
            if (projectile == null || enemy == null || appliedDamage <= 0f)
            {
                return;
            }

            if (IsSeinCombatUnit(projectile.ManifestedSource))
            {
                return;
            }

            if (string.Equals(projectile.SkillId, "sein-a", StringComparison.OrdinalIgnoreCase))
            {
                enemy.SeinScorchingArrowTimer = Mathf.Max(enemy.SeinScorchingArrowTimer, 4f);
                if (HasManifestedChoice(projectile.ManifestedSource, "sein-a-master-2"))
                {
                    ApplyManifestedAreaDamage(enemy.Transform.position, 1.35f, projectile.BaseDamage * 0.50f, DamageAttribute.Fire);
                }
            }
        }

        private void ApplyManifestedAreaDamage(Vector3 center, float radius, float baseDamage, DamageAttribute attribute)
        {
            if (baseDamage <= 0f || radius <= 0f)
            {
                return;
            }

            for (var i = 0; i < enemies.Count; i++)
            {
                var enemy = enemies[i];
                if (enemy == null || enemy.Transform == null || enemy.CurrentHealth <= 0f)
                {
                    continue;
                }

                if (Vector2.Distance(center, enemy.Transform.position) > radius + GetEnemyHitRadius(enemy))
                {
                    continue;
                }

                var damageResult = DamageCalculator.Resolve(
                    baseDamage,
                    attribute,
                    enemy.Defenses,
                    targetCriticalResistance: enemy.CriticalResistance,
                    finalDamageMultiplier: enemy.DamageTakenMultiplier);
                ApplyDamageToEnemy(enemy, damageResult.FinalDamage, damageResult.Attribute);
                enemy.FlashTimer = 0.08f;
            }
        }

        private void ApplyManifestedProjectileStatus(ProjectileRuntime projectile, EnemyRuntime enemy)
        {
            if (projectile == null || enemy == null || projectile.StatusChance <= 0f || UnityEngine.Random.value >= Mathf.Clamp01(projectile.StatusChance))
            {
                return;
            }

            var statusId = projectile.ManifestedStatusEffectId ?? string.Empty;
            if (statusId.Contains("媛먯쟾") || statusId.Contains("감전") || string.Equals(statusId, "shock", StringComparison.OrdinalIgnoreCase))
            {
                ApplyShock(enemy, Mathf.Max(1, projectile.StatusStacks), 1.25f);
            }
            else if (statusId.Contains("鍮숆껐") || statusId.Contains("?됯린") || statusId.Contains("빙결") || string.Equals(statusId, "chill", StringComparison.OrdinalIgnoreCase))
            {
                ApplyChill(enemy, Mathf.Max(1, projectile.StatusStacks), 2.5f);
            }
            else if (statusId.Contains("痍⑥빟") || statusId.Contains("취약") || string.Equals(statusId, "vulnerable", StringComparison.OrdinalIgnoreCase))
            {
                ApplyVulnerable(enemy, Mathf.Max(1, projectile.StatusStacks));
            }
        }

        private float ApplyManifestedSkillDamage(CombatUnitRuntime runtime, SkillDefinition skill, EnemyRuntime target)
        {
            return ApplyManifestedSkillDamage(runtime, skill, target, 1f);
        }

        private float ApplyManifestedSkillDamage(CombatUnitRuntime runtime, SkillDefinition skill, EnemyRuntime target, float finalMultiplier)
        {
            if (target == null || skill == null)
            {
                return 0f;
            }

            var baseDamage = ResolveManifestedBaseDamage(runtime, skill);
            var damageResult = DamageCalculator.Resolve(
                baseDamage,
                skill.Attribute,
                target.Defenses,
                targetCriticalResistance: target.CriticalResistance,
                finalDamageMultiplier: target.DamageTakenMultiplier * Mathf.Max(0f, finalMultiplier));
            var applied = ApplyDamageToEnemy(target, damageResult.FinalDamage, damageResult.Attribute);
            target.FlashTimer = 0.08f;
            return applied;
        }

        private void ApplyManifestedSkillEffectDamage(SkillEffectRuntime effect, EnemyRuntime target)
        {
            if (effect == null || effect.ManifestedSource == null || target == null || target.CurrentHealth <= 0f)
            {
                return;
            }

            var damageResult = DamageCalculator.Resolve(
                effect.BaseDamage,
                effect.Attribute,
                target.Defenses,
                targetCriticalResistance: target.CriticalResistance,
                finalDamageMultiplier: target.DamageTakenMultiplier);
            ApplyDamageToEnemy(target, damageResult.FinalDamage, damageResult.Attribute);
            target.FlashTimer = 0.08f;

            if (string.Equals(effect.SkillId, "eve-c", StringComparison.OrdinalIgnoreCase))
            {
                ApplyChill(target, Mathf.Max(1, effect.StatusStacks), 2.5f);
                if (effect.FreezeDuration > 0f)
                {
                    target.FreezeTimer = Mathf.Max(target.FreezeTimer, effect.FreezeDuration);
                }
            }

            if (string.Equals(effect.SkillId, "sein-d", StringComparison.OrdinalIgnoreCase)
                || string.Equals(effect.SkillId, "sein-d-residual", StringComparison.OrdinalIgnoreCase))
            {
                target.SeinSuperheatedZoneTimer = Mathf.Max(target.SeinSuperheatedZoneTimer, 0.7f);
                target.SeinSuperheatedTickCount += 1;
            }
        }

        private float ResolveManifestedBaseDamage(CombatUnitRuntime runtime, SkillDefinition skill)
        {
            if (runtime == null || runtime.Monster == null)
            {
                return 1f;
            }

            if (skill == null)
            {
                var fallback = runtime.BaseDamage + (runtime.PowerStat * runtime.Monster.PowerCoefficient);
                return Mathf.Max(1f, fallback * ResolveManifestedDamageMultiplier(runtime));
            }

            var coefficient = Mathf.Max(skill.AttackPowerCoefficient, skill.SpellPowerCoefficient);
            return Mathf.Max(1f, (skill.BaseDamage + (runtime.PowerStat * coefficient)) * ResolveManifestedDamageMultiplier(runtime) * ResolveManifestedSkillDamageMultiplier(runtime, skill));
        }

        private float ResolveManifestedSkillDamageMultiplier(CombatUnitRuntime runtime, SkillDefinition skill)
        {
            if (skill == null)
            {
                return 1f;
            }

            var multiplier = 1f;
            var skillId = skill.SkillId ?? string.Empty;
            if (string.Equals(skillId, "ariel-a", StringComparison.OrdinalIgnoreCase))
            {
                multiplier *= HasManifestedChoice(runtime, "ariel-a-trait-1") ? 1.25f : 1f;
                multiplier *= HasManifestedChoice(runtime, "ariel-a-trait-5") ? 1.06f : 1f;
            }
            else if (string.Equals(skillId, "ariel-c", StringComparison.OrdinalIgnoreCase))
            {
                multiplier *= HasManifestedChoice(runtime, "ariel-c-trait-1") ? 1.25f : 1f;
            }
            else if (string.Equals(skillId, "ariel-d", StringComparison.OrdinalIgnoreCase))
            {
                multiplier *= HasManifestedChoice(runtime, "ariel-d-trait-1") ? 1.30f : 1f;
                multiplier *= HasManifestedChoice(runtime, "ariel-d-trait-4") ? 0.80f : 1f;
            }
            else if (string.Equals(skillId, "ariel-e", StringComparison.OrdinalIgnoreCase))
            {
                multiplier *= HasManifestedChoice(runtime, "ariel-e-trait-1") ? 1.30f : 1f;
                multiplier *= HasManifestedChoice(runtime, "ariel-e-master-2") ? 1.70f : 1f;
            }
            else if (string.Equals(skillId, "sein-a", StringComparison.OrdinalIgnoreCase))
            {
                multiplier *= HasManifestedChoice(runtime, "sein-a-trait-1") ? 1.25f : 1f;
                multiplier *= HasManifestedChoice(runtime, "sein-a-trait-4") ? 1.10f : 1f;
                multiplier *= HasManifestedChoice(runtime, "sein-a-trait-5") ? 0.90f : 1f;
                multiplier *= HasManifestedChoice(runtime, "sein-a-master-1") ? 1.55f : 1f;
            }
            else if (string.Equals(skillId, "sein-b", StringComparison.OrdinalIgnoreCase))
            {
                multiplier *= HasManifestedChoice(runtime, "sein-b-trait-2") ? 1.25f : 1f;
                multiplier *= HasManifestedChoice(runtime, "sein-b-master-1") ? 0.80f : 1f;
                multiplier *= HasManifestedChoice(runtime, "sein-b-master-2") ? 1.90f : 1f;
            }
            else if (string.Equals(skillId, "rin-a", StringComparison.OrdinalIgnoreCase))
            {
                multiplier *= HasManifestedChoice(runtime, "rin-a-trait-1") ? 1.25f : 1f;
                multiplier *= HasManifestedChoice(runtime, "rin-a-trait-4") ? 0.90f : 1f;
                multiplier *= HasManifestedChoice(runtime, "rin-a-master-1") ? 1.12f : 1f;
            }
            else if (string.Equals(skillId, "vega-b", StringComparison.OrdinalIgnoreCase))
            {
                multiplier *= HasManifestedChoice(runtime, "vega-b-trait-1") ? 1.25f : 1f;
                multiplier *= HasManifestedChoice(runtime, "vega-b-master-2") ? 1.70f : 1f;
            }
            else if (string.Equals(skillId, "vega-d", StringComparison.OrdinalIgnoreCase))
            {
                multiplier *= HasManifestedChoice(runtime, "vega-d-trait-1") ? 1.25f : 1f;
                multiplier *= HasManifestedChoice(runtime, "vega-d-master-2") ? 1.30f : 1f;
            }
            else if (string.Equals(skillId, "vega-e", StringComparison.OrdinalIgnoreCase))
            {
                multiplier *= HasManifestedChoice(runtime, "vega-e-trait-1") ? 1.25f : 1f;
                multiplier *= HasManifestedChoice(runtime, "vega-e-master-2") ? 0.80f : 1f;
            }

            return Mathf.Max(0f, multiplier);
        }

        private float ResolveManifestedDamageMultiplier(CombatUnitRuntime runtime)
        {
            return runtime != null && runtime.State != null && runtime.State.DamageMultiplier > 0f
                ? runtime.State.DamageMultiplier
                : 1f;
        }

        private float ResolveManifestedProjectileSpeed(CombatUnitRuntime runtime)
        {
            return runtime != null && runtime.Monster != null && runtime.Monster.ProjectileSpeed > 0f
                ? runtime.Monster.ProjectileSpeed
                : ManifestedMonsterProjectileSpeedFallback;
        }

        private float ResolveManifestedProjectileLifetime(CombatUnitRuntime runtime, SkillDefinition skill)
        {
            if (runtime != null && runtime.Monster != null && runtime.Monster.ProjectileLifetime > 0f)
            {
                return runtime.Monster.ProjectileLifetime;
            }

            var range = skill != null && skill.Range > 0f ? skill.Range : 8f;
            return Mathf.Max(0.5f, range / ResolveManifestedProjectileSpeed(runtime));
        }

        private float ResolveManifestedProjectileHitRadius(CombatUnitRuntime runtime)
        {
            return runtime != null && runtime.Monster != null && runtime.Monster.ProjectileHitRadius > 0f
                ? runtime.Monster.ProjectileHitRadius
                : 0.42f;
        }

        private float ResolveManifestedStatusChance(CombatUnitRuntime runtime)
        {
            var chance = runtime != null && runtime.Monster != null ? runtime.Monster.StatusChance : 0f;
            chance += runtime != null && runtime.State != null ? runtime.State.StatusChanceBonus : 0f;
            return Mathf.Clamp01(chance);
        }
    }
}
