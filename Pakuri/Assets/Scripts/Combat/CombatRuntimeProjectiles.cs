using System;
using System.Collections.Generic;
using Pakuri.Data;
using Pakuri.Run;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Pakuri.Combat
{
    public partial class CombatRuntimeController
    {
        private void UpdateProjectiles()
        {
            for (var i = projectiles.Count - 1; i >= 0; i--)
            {
                var projectile = projectiles[i];
                if (projectile == null || projectile.GameObject == null)
                {
                    projectiles.RemoveAt(i);
                    continue;
                }

                var travelDistance = projectile.Speed * Time.deltaTime;
                projectile.Transform.position += projectile.Direction * travelDistance;
                projectile.RemainingLifetime = Mathf.Max(0f, projectile.RemainingLifetime - Time.deltaTime);

                if (projectile.IsEnemyProjectile)
                {
                    if (TryHitEnemyProjectileTarget(projectile, out var targetLabel, out var appliedDamage))
                    {
                        statusLabel = $"{projectile.SourceEnemy.DisplayName} {projectile.SourceEnemy.Definition.ActiveSkillName}: {targetLabel} hit {appliedDamage:0.0}.";
                        CleanupProjectile(i);
                        continue;
                    }

                    if (projectile.RemainingLifetime > 0f)
                    {
                        continue;
                    }

                    CleanupProjectile(i);
                    continue;
                }

                if (TryHitEnemy(projectile, out var enemyHit, out var damageResult))
                {
                    var appliedDamage = ApplyDamageToEnemy(enemyHit, damageResult.FinalDamage);
                    enemyHit.FlashTimer = 0.08f;

                    var appliedStatus = false;
                    if (statusChanceConfigured > 0f && UnityEngine.Random.value < statusChanceConfigured)
                    {
                        enemyHit.ShockStacks = Mathf.Min(enemyHit.ShockStacks + 1, 3);
                        enemyHit.ShockTimer = 1.25f;
                        appliedStatus = !string.IsNullOrWhiteSpace(selectedStatusEffectLabel);
                    }

                    statusLabel = appliedStatus
                        ? $"{selectedActiveSkillName} 적중: {enemyHit.DisplayName}에게 {appliedDamage:0.0} {selectedElementLabel} 피해, {selectedStatusEffectLabel} 부여."
                        : $"{selectedActiveSkillName} 적중: {enemyHit.DisplayName}에게 {appliedDamage:0.0} {selectedElementLabel} 피해.";

                    Debug.Log($"[CombatDamage] {selectedMonsterName}.{selectedActiveSkillName} -> {enemyHit.DisplayName}: {damageResult.FormulaLog}; Applied={appliedDamage:0.##}, ShieldLeft={enemyHit.ShieldValue:0.##}, HpLeft={Mathf.Max(0f, enemyHit.CurrentHealth):0.##}");

                    CleanupProjectile(i);
                    continue;
                }

                if (projectile.RemainingLifetime > 0f)
                {
                    continue;
                }

                statusLabel = $"{selectedActiveSkillName} 투사체가 {projectileLifetimeConfigured:0.0}s 후 소멸했다.";
                CleanupProjectile(i);
            }
        }

        private bool TryHitEnemyProjectileTarget(ProjectileRuntime projectile, out string targetLabel, out float appliedDamage)
        {
            targetLabel = string.Empty;
            appliedDamage = 0f;

            if (projectile == null || projectile.SourceEnemy == null || projectile.SourceEnemy.Definition == null)
            {
                return false;
            }

            var targetTransform = projectile.TargetTransform;
            if (targetTransform == null)
            {
                return false;
            }

            var targetRadius = projectile.TargetsMonster ? 0.7f : 1.0f;
            if (Vector2.Distance(projectile.Transform.position, targetTransform.position) > projectile.HitRadius + targetRadius)
            {
                return false;
            }

            var source = projectile.SourceEnemy;
            if (projectile.TargetsMonster && unitCurrentHealth > 0f)
            {
                var resolution = EnemyAttackResolver.ResolveAgainstMonster(
                    source.Definition,
                    source.AttackPower,
                    source.DamageMultiplier,
                    source.AttackBuffMultiplier,
                    source.CriticalChanceBonus,
                    source.CriticalMultiplierBonus,
                    selectedMonsterDefenses);
                appliedDamage = resolution.FinalDamage;
                unitCurrentHealth = Mathf.Max(0f, unitCurrentHealth - appliedDamage);
                targetLabel = selectedMonsterName;
                return true;
            }

            if (!projectile.TargetsMonster)
            {
                var resolution = EnemyAttackResolver.ResolveAgainstNexus(
                    source.Definition,
                    source.AttackPower,
                    source.DamageMultiplier,
                    source.AttackBuffMultiplier,
                    source.CriticalChanceBonus,
                    source.CriticalMultiplierBonus);
                appliedDamage = resolution.FinalDamage;
                nexusCurrentHealth = Mathf.Max(0f, nexusCurrentHealth - appliedDamage);
                targetLabel = "Nexus";
                return true;
            }

            return false;
        }

        private bool TryHitEnemy(ProjectileRuntime projectile, out EnemyRuntime enemyHit, out DamageResult damageResult)
        {
            enemyHit = null;
            damageResult = default;

            for (var i = 0; i < enemies.Count; i++)
            {
                var enemy = enemies[i];
                if (enemy == null || enemy.Transform == null || enemy.CurrentHealth <= 0f)
                {
                    continue;
                }

                var hitDistance = GetEnemyHitRadius(enemy) + projectile.HitRadius;
                if (Vector2.Distance(projectile.Transform.position, enemy.Transform.position) > hitDistance)
                {
                    continue;
                }

                enemyHit = enemy;
                damageResult = DamageCalculator.Resolve(
                    projectile.BaseDamage,
                    projectile.Attribute,
                    enemy.Defenses,
                    targetCriticalResistance: enemy.CriticalResistance,
                    finalDamageMultiplier: enemy.DamageTakenMultiplier);
                return true;
            }

            return false;
        }

        private static float ApplyDamageToEnemy(EnemyRuntime enemy, float incomingDamage)
        {
            if (enemy == null || incomingDamage <= 0f)
            {
                return 0f;
            }

            var remainingDamage = incomingDamage;
            if (enemy.ShieldValue > 0f)
            {
                var absorbed = Mathf.Min(enemy.ShieldValue, remainingDamage);
                enemy.ShieldValue -= absorbed;
                remainingDamage -= absorbed;
            }

            var appliedDamage = Mathf.Min(enemy.CurrentHealth, remainingDamage);
            enemy.CurrentHealth -= appliedDamage;
            return appliedDamage;
        }

        private static float GetEnemyHitRadius(EnemyRuntime enemy)
        {
            return enemy != null && enemy.IsBoss ? 0.95f : 0.65f;
        }

        private void UpdateSelectedMonsterCombat()
        {
            if (reloadRemaining > 0f)
            {
                reloadRemaining = Mathf.Max(0f, reloadRemaining - Time.deltaTime);
                if (Mathf.Approximately(reloadRemaining, 0f))
                {
                    currentShotsRemaining = magazineCapacityConfigured;
                    statusLabel = $"{selectedActiveSkillName} 탄창이 재장전됐다.";
                }

                return;
            }

            shotCooldown = Mathf.Max(0f, shotCooldown - Time.deltaTime);
            if (!fireRequestedThisFrame)
            {
                return;
            }

            if (currentShotsRemaining <= 0)
            {
                reloadRemaining = reloadDurationConfigured;
                currentShotsRemaining = 0;
                statusLabel = $"{selectedActiveSkillName} 탄창이 비어 재장전 중이다.";
                return;
            }

            if (shotCooldown > 0f)
            {
                statusLabel = $"{selectedActiveSkillName} 재사용 대기: {shotCooldown:0.00}s";
                return;
            }

            FirePrimarySkill();
        }

        private void FirePrimarySkill()
        {
            if (eveAnchor == null || projectileRoot == null)
            {
                return;
            }

            var direction = currentAttackPoint - eveAnchor.position;
            direction.z = 0f;
            if (direction.sqrMagnitude < 0.01f)
            {
                statusLabel = $"{selectedMonsterName} 앞쪽으로 목표 지점을 다시 지정한다.";
                return;
            }

            direction.Normalize();

            nextProjectileSequence += 1;
            var safeSkillName = selectedActiveSkillName.Replace(" ", string.Empty);
            var projectileObject = new GameObject($"{safeSkillName}_{nextProjectileSequence:00}");
            projectileObject.transform.SetParent(projectileRoot, false);
            projectileObject.transform.position = eveAnchor.position;
            projectileObject.transform.localScale = new Vector3(projectileHitRadiusConfigured, projectileHitRadiusConfigured, 1f);

            var renderer = projectileObject.AddComponent<SpriteRenderer>();
            renderer.sprite = selectedProjectileSprite != null ? selectedProjectileSprite : GetSharedSprite();
            renderer.color = Color.white;
            renderer.sortingOrder = 25;

            var projectile = new ProjectileRuntime
            {
                GameObject = projectileObject,
                Transform = projectileObject.transform,
                Renderer = renderer,
                Direction = direction,
                Speed = projectileSpeedConfigured,
                RemainingLifetime = projectileLifetimeConfigured,
                HitRadius = projectileHitRadiusConfigured,
                BaseDamage = baseDamageConfigured + (powerStatConfigured * powerCoefficientConfigured),
                Attribute = selectedDamageAttribute
            };

            projectiles.Add(projectile);
            currentShotsRemaining -= 1;
            shotCooldown = shotIntervalConfigured;
            if (currentShotsRemaining <= 0)
            {
                currentShotsRemaining = 0;
                reloadRemaining = reloadDurationConfigured;
                statusLabel = $"{selectedActiveSkillName} 발사. 탄창이 비어 재장전에 들어간다.";
                return;
            }

            statusLabel = $"{selectedActiveSkillName} 발사: ({currentAttackPoint.x:0.0}, {currentAttackPoint.y:0.0})";
        }

        private void CleanupProjectile(int index)
        {
            if (index < 0 || index >= projectiles.Count)
            {
                return;
            }

            var projectile = projectiles[index];
            if (projectile != null && projectile.GameObject != null)
            {
                Destroy(projectile.GameObject);
            }

            projectiles.RemoveAt(index);
        }
    }
}
