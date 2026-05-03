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
                    TrackArielHolyExposureDamage(enemyHit, projectile.Attribute, damageResult.FinalDamage);
                    HandleRinProjectileHit(projectile, enemyHit, appliedDamage);

                    var appliedStatus = false;
                    if (string.Equals(projectile.SkillId, "ariel-a", StringComparison.OrdinalIgnoreCase)
                        && HasChoice("ariel-a-master-2"))
                    {
                        ApplyArielHolyExposure(enemyHit, 1, 6f, 0.18f, 0f, 0f, 0f);
                        appliedStatus = true;
                    }

                    if (string.Equals(projectile.SkillId, "eve-e", StringComparison.OrdinalIgnoreCase))
                    {
                        ApplyVulnerable(enemyHit, Mathf.Max(1, projectile.StatusStacks));
                        appliedStatus = true;
                    }
                    var statusChance = Mathf.Clamp01((projectile.StatusChance > 0f ? projectile.StatusChance : statusChanceConfigured) + GetEveStatusChanceBonus(enemyHit));
                    if (!appliedStatus && statusChance > 0f && UnityEngine.Random.value < statusChance)
                    {
                        ApplyShock(enemyHit, Mathf.Max(1, projectile.StatusStacks), 1.25f);
                        appliedStatus = !string.IsNullOrWhiteSpace(selectedStatusEffectLabel);
                    }

                    var appliedStatusLabel = string.Equals(projectile.SkillId, "eve-e", StringComparison.OrdinalIgnoreCase) ? "취약" : selectedStatusEffectLabel;
                    statusLabel = appliedStatus
                        ? $"{selectedActiveSkillName} 적중: {enemyHit.DisplayName}에게 {appliedDamage:0.0} {selectedElementLabel} 피해, {appliedStatusLabel} 부여."
                        : $"{selectedActiveSkillName} 적중: {enemyHit.DisplayName}에게 {appliedDamage:0.0} {selectedElementLabel} 피해.";

                    Debug.Log($"[CombatDamage] {selectedMonsterName}.{selectedActiveSkillName} -> {enemyHit.DisplayName}: {damageResult.FormulaLog}; Applied={appliedDamage:0.##}, ShieldLeft={enemyHit.ShieldValue:0.##}, HpLeft={Mathf.Max(0f, enemyHit.CurrentHealth):0.##}");
                    TryApplyProjectileBranch(projectile, enemyHit, damageResult.FinalDamage);
                    TryTriggerEveParticleSeparationProc(enemyHit, projectile.Attribute, projectile.SkillId);

                    if (TryTriggerArielJudgementLightExplosion(projectile))
                    {
                        CleanupProjectile(i);
                        continue;
                    }

                    projectile.HitEnemies.Add(enemyHit);
                    if (projectile.RemainingPierce > 0)
                    {
                        projectile.RemainingPierce -= 1;
                    }
                    else
                    {
                        TryTriggerArielJudgementLightExplosion(projectile);
                        CleanupProjectile(i);
                    }

                    continue;
                }

                if (!HasPlayerProjectileReachedBattlefieldXEdge(projectile))
                {
                    continue;
                }

                statusLabel = $"{selectedActiveSkillName} 투사체가 전장 X 경계에 닿아 소멸했다.";
                TryTriggerArielJudgementLightExplosion(projectile);
                CleanupProjectile(i);
            }
        }

        private bool HasPlayerProjectileReachedBattlefieldXEdge(ProjectileRuntime projectile)
        {
            if (projectile == null || projectile.Transform == null)
            {
                return true;
            }

            var x = projectile.Transform.position.x;
            var minX = 0f;
            var maxX = Mathf.Max(minX, fieldSize.x);
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
                appliedDamage = ApplyDamageToSelectedMonster(appliedDamage, source);
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
                if (projectile.HitEnemies.Contains(enemy))
                {
                    continue;
                }

                if (Vector2.Distance(projectile.Transform.position, enemy.Transform.position) > hitDistance)
                {
                    continue;
                }

                enemyHit = enemy;
                var finalMultiplier = GetEveFinalDamageMultiplier(enemy, projectile.Attribute, projectile.SkillId)
                    * GetArielFinalDamageMultiplier(enemy, projectile.Attribute, projectile.SkillId)
                    * GetRinFinalDamageMultiplier(enemy, projectile.Attribute, projectile.SkillId);
                damageResult = DamageCalculator.Resolve(
                    projectile.BaseDamage,
                    projectile.Attribute,
                    enemy.Defenses,
                    flatDefenseReduction: GetEveFlatDefenseReduction(enemy, projectile.Attribute) + GetArielFlatDefenseReduction(enemy, projectile.Attribute),
                    criticalChanceBonus: GetArielCriticalChanceBonus(projectile.Attribute) + GetRinCriticalChanceBonus(enemy, projectile.Attribute, projectile.SkillId),
                    criticalMultiplierBonus: GetRinCriticalMultiplierBonus(enemy, projectile.Attribute, projectile.SkillId),
                    targetCriticalResistance: enemy.CriticalResistance,
                    criticalDamageTakenBonus: GetEveCriticalDamageTakenBonus(enemy, projectile.SkillId) + GetArielCriticalDamageTakenBonus(enemy, projectile.SkillId),
                    finalDamageMultiplier: enemy.DamageTakenMultiplier * finalMultiplier);
                return true;
            }

            return false;
        }

        private void TryApplyProjectileBranch(ProjectileRuntime projectile, EnemyRuntime sourceEnemy, float primaryFinalDamage)
        {
            if (projectile == null
                || sourceEnemy == null
                || sourceEnemy.Transform == null
                || projectile.BranchTargetCount <= 0
                || projectile.BranchRadius <= 0f
                || projectile.BranchChance <= 0f
                || primaryFinalDamage <= 0f
                || UnityEngine.Random.value >= projectile.BranchChance)
            {
                return;
            }

            var branchDamage = primaryFinalDamage * Mathf.Max(0f, projectile.BranchDamageMultiplier);
            if (branchDamage <= 0f)
            {
                return;
            }

            var branchCount = 0;
            for (var i = 0; i < enemies.Count && branchCount < projectile.BranchTargetCount; i++)
            {
                var branchTarget = FindNearestBranchTarget(sourceEnemy, projectile.BranchRadius, projectile);
                if (branchTarget == null)
                {
                    break;
                }

                var appliedDamage = ApplyDamageToEnemy(branchTarget, branchDamage);
                branchTarget.FlashTimer = 0.08f;
                CreateEveArcBranchLine(sourceEnemy.Transform.position, branchTarget.Transform.position);
                projectile.HitEnemies.Add(branchTarget);
                branchCount += 1;

                Debug.Log($"[CombatDamage] Eve.ArcBranch -> {branchTarget.DisplayName}: Incoming={branchDamage:0.##}, Applied={appliedDamage:0.##}, ShieldLeft={branchTarget.ShieldValue:0.##}, HpLeft={Mathf.Max(0f, branchTarget.CurrentHealth):0.##}");
            }

            if (branchCount > 0)
            {
                statusLabel = $"Arc Bolt branch hit {branchCount} nearby target(s).";
            }
        }

        private EnemyRuntime FindNearestBranchTarget(EnemyRuntime sourceEnemy, float radius, ProjectileRuntime projectile)
        {
            EnemyRuntime best = null;
            var bestDistance = float.MaxValue;
            for (var i = 0; i < enemies.Count; i++)
            {
                var enemy = enemies[i];
                if (enemy == null
                    || enemy == sourceEnemy
                    || enemy.Transform == null
                    || enemy.CurrentHealth <= 0f
                    || projectile.HitEnemies.Contains(enemy))
                {
                    continue;
                }

                var distance = Vector2.Distance(sourceEnemy.Transform.position, enemy.Transform.position);
                if (distance > radius || distance >= bestDistance)
                {
                    continue;
                }

                best = enemy;
                bestDistance = distance;
            }

            return best;
        }

        private float ApplyDamageToEnemy(EnemyRuntime enemy, float incomingDamage)
        {
            if (enemy == null || incomingDamage <= 0f)
            {
                return 0f;
            }

            var remainingDamage = incomingDamage;
            var totalAppliedDamage = 0f;
            if (enemy.ShieldValue > 0f)
            {
                var absorbed = Mathf.Min(enemy.ShieldValue, remainingDamage);
                enemy.ShieldValue -= absorbed;
                remainingDamage -= absorbed;
                totalAppliedDamage += absorbed;
            }

            var appliedDamage = Mathf.Min(enemy.CurrentHealth, remainingDamage);
            enemy.CurrentHealth -= appliedDamage;
            totalAppliedDamage += appliedDamage;
            if (totalAppliedDamage > 0f)
            {
                SpawnDamagePopupForEnemy(enemy, totalAppliedDamage);
            }

            return appliedDamage;
        }

        private float ApplyDamageToSelectedMonster(float incomingDamage, EnemyRuntime sourceEnemy = null)
        {
            if (incomingDamage <= 0f)
            {
                return 0f;
            }

            var remainingDamage = GetArielIncomingDamageAfterReduction(incomingDamage);
            var totalAppliedDamage = 0f;
            if (unitShieldValue > 0f)
            {
                var shieldBeforeAbsorb = unitShieldValue;
                var absorbed = Mathf.Min(unitShieldValue, remainingDamage);
                unitShieldValue -= absorbed;
                remainingDamage -= absorbed;
                totalAppliedDamage += absorbed;
                HandleArielShieldAbsorbed(absorbed, shieldBeforeAbsorb, sourceEnemy);
            }

            var appliedDamage = Mathf.Min(unitCurrentHealth, remainingDamage);
            unitCurrentHealth = Mathf.Max(0f, unitCurrentHealth - appliedDamage);
            totalAppliedDamage += appliedDamage;
            if (totalAppliedDamage > 0f)
            {
                SpawnDamagePopupForSelectedMonster(totalAppliedDamage);
            }

            return appliedDamage;
        }

        private static float GetEnemyHitRadius(EnemyRuntime enemy)
        {
            return enemy != null && enemy.IsBoss ? 0.95f : 0.65f;
        }

        private bool ShouldTrySelectedMonsterAutomaticSkillsThisFrame()
        {
            if (!fireRequestedThisFrame)
            {
                return false;
            }

            // Ariel support skills should align to an actual firing window so held input
            // does not keep retrying them every Update while the main shot is unavailable.
            if (IsSelectedArielMonster())
            {
                return reloadRemaining <= 0f && shotCooldown <= 0f && currentShotsRemaining > 0;
            }

            return true;
        }

        private void UpdateSelectedMonsterCombat()
        {
            UpdateSelectedMonsterSkillCooldowns();
            shotCooldown = Mathf.Max(0f, shotCooldown - Time.deltaTime * GetSelectedMonsterActionSpeedMultiplier());

            if (ShouldTrySelectedMonsterAutomaticSkillsThisFrame())
            {
                TryTriggerSelectedMonsterAutomaticSkills();
            }

            if (reloadRemaining > 0f)
            {
                reloadRemaining = Mathf.Max(0f, reloadRemaining - Time.deltaTime * GetSelectedMonsterActionSpeedMultiplier());
                if (Mathf.Approximately(reloadRemaining, 0f))
                {
                    currentShotsRemaining = GetSelectedMonsterMagazineCapacity();
                    statusLabel = $"{selectedActiveSkillName} 탄창이 재장전됐다.";
                }

                return;
            }

            if (!fireRequestedThisFrame)
            {
                return;
            }

            if (currentShotsRemaining <= 0)
            {
                reloadRemaining = IsSelectedArielMonster() ? GetArielReloadSeconds() : reloadDurationConfigured;
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

            if (IsSelectedEveMonster())
            {
                FireManualEveArcBolt(direction);
                return;
            }

            if (IsSelectedArielMonster())
            {
                FireManualArielJudgementLight(direction);
                return;
            }

            if (IsSelectedRinMonster())
            {
                FireManualRinShatteringFist(direction);
                return;
            }

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
