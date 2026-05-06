using System;
using Pakuri.Data;
using UnityEngine;

namespace Pakuri.Combat
{
    public partial class CombatRuntimeController
    {
        private const float SeinScorchingArrowSpeed = 18f;
        private const float SeinBlazingVolleySpeed = 20f;
        private const float SeinFlameTrajectoryDelay = 0.8f;
        private const float SeinSuperheatedZoneDuration = 4f;
        private const float SeinSuperheatedZoneTickInterval = 0.5f;
        private const float SeinDoomsdayLineWidth = 1.8f;
        private const float SeinFireDefenseReductionDuration = 5f;
        private const float SeinFlameTrajectoryProjectileSpeed = 14f;
        private const float SeinDoomsdaySkyOriginX = 10f;
        private const float SeinDoomsdaySkyOriginY = 10f;

        private float seinBlazingVolleyCooldownRemaining;
        private int seinBlazingVolleyShotsRemaining;
        private float seinBlazingVolleyReloadRemaining;
        private float seinBlazingVolleyShotCooldownRemaining;
        private float seinFlameTrajectoryCooldownRemaining;
        private float seinSuperheatedZoneCooldownRemaining;
        private float seinDoomsdayLineCooldownRemaining;
        private float seinFlameBarrageProcCooldownRemaining;

        private void ResetSeinSkillCombatTimers()
        {
            seinBlazingVolleyCooldownRemaining = 0f;
            seinBlazingVolleyShotsRemaining = IsSelectedSeinMonster() ? GetSeinBlazingVolleyMagazineCapacity() : 0;
            seinBlazingVolleyReloadRemaining = 0f;
            seinBlazingVolleyShotCooldownRemaining = 0f;
            seinFlameTrajectoryCooldownRemaining = 0f;
            seinSuperheatedZoneCooldownRemaining = 0f;
            seinDoomsdayLineCooldownRemaining = 0f;
            seinFlameBarrageProcCooldownRemaining = 0f;
        }

        private void UpdateSeinSkillCooldowns()
        {
            var elapsed = Time.deltaTime * GetSeinActionSpeedMultiplier();
            seinBlazingVolleyCooldownRemaining = Mathf.Max(0f, seinBlazingVolleyCooldownRemaining - elapsed);
            seinBlazingVolleyReloadRemaining = Mathf.Max(0f, seinBlazingVolleyReloadRemaining - elapsed);
            seinBlazingVolleyShotCooldownRemaining = Mathf.Max(0f, seinBlazingVolleyShotCooldownRemaining - elapsed);
            if (Mathf.Approximately(seinBlazingVolleyReloadRemaining, 0f) && seinBlazingVolleyShotsRemaining <= 0)
            {
                seinBlazingVolleyShotsRemaining = GetSeinBlazingVolleyMagazineCapacity();
            }

            seinFlameTrajectoryCooldownRemaining = Mathf.Max(0f, seinFlameTrajectoryCooldownRemaining - elapsed);
            seinSuperheatedZoneCooldownRemaining = Mathf.Max(0f, seinSuperheatedZoneCooldownRemaining - elapsed);
            seinDoomsdayLineCooldownRemaining = Mathf.Max(0f, seinDoomsdayLineCooldownRemaining - elapsed);
            seinFlameBarrageProcCooldownRemaining = Mathf.Max(0f, seinFlameBarrageProcCooldownRemaining - Time.deltaTime);
        }

        private void UpdateSeinSkillEffects()
        {
            if (!IsSelectedSeinMonster())
            {
                return;
            }

            for (var i = 0; i < enemies.Count; i++)
            {
                var enemy = enemies[i];
                if (enemy == null)
                {
                    continue;
                }

                enemy.SeinScorchingArrowTimer = Mathf.Max(0f, enemy.SeinScorchingArrowTimer - Time.deltaTime);
                enemy.SeinSuperheatedZoneTimer = Mathf.Max(0f, enemy.SeinSuperheatedZoneTimer - Time.deltaTime);
                if (Mathf.Approximately(enemy.SeinSuperheatedZoneTimer, 0f))
                {
                    enemy.SeinSuperheatedTickCount = 0;
                }

                enemy.SeinFireDefenseReductionTimer = Mathf.Max(0f, enemy.SeinFireDefenseReductionTimer - Time.deltaTime);
                if (Mathf.Approximately(enemy.SeinFireDefenseReductionTimer, 0f))
                {
                    enemy.SeinFireDefenseReduction = 0f;
                }

                enemy.SeinBurningTrajectoryTimer = Mathf.Max(0f, enemy.SeinBurningTrajectoryTimer - Time.deltaTime);
                if (Mathf.Approximately(enemy.SeinBurningTrajectoryTimer, 0f))
                {
                    enemy.SeinBurningTrajectoryDamageTakenBonus = 0f;
                }

                enemy.SeinThermalSpreadTimer = Mathf.Max(0f, enemy.SeinThermalSpreadTimer - Time.deltaTime);
                if (Mathf.Approximately(enemy.SeinThermalSpreadTimer, 0f))
                {
                    enemy.SeinThermalSpreadDamageTakenBonus = 0f;
                }

                enemy.SeinDoomsdayOmenTimer = Mathf.Max(0f, enemy.SeinDoomsdayOmenTimer - Time.deltaTime);
                if (Mathf.Approximately(enemy.SeinDoomsdayOmenTimer, 0f))
                {
                    enemy.SeinDoomsdayOmenDamageTakenBonus = 0f;
                }
            }
        }

        private bool TryTriggerSeinAutomaticSkills()
        {
            if (!IsSelectedSeinMonster())
            {
                return false;
            }

            var castAny = false;
            castAny |= TryCastSeinBlazingVolley();
            castAny |= TryCastSeinFlameTrajectory();
            castAny |= TryCastSeinSuperheatedZone();
            castAny |= TryCastSeinDoomsdayLine();

            if (!castAny)
            {
                statusLabel = $"{selectedMonsterName}: no Sein active skill is ready.";
            }

            return true;
        }

        private void FireManualSeinScorchingArrow(Vector3 baseDirection)
        {
            var skill = FindSelectedSkill(SkillSlot.A);
            if (skill == null || !HasLearnedActive(SkillSlot.A) || eveAnchor == null || projectileRoot == null)
            {
                return;
            }

            FireSeinProjectile(
                "ScorchingArrow",
                "sein-a",
                skill,
                baseDirection,
                SeinScorchingArrowSpeed,
                GetSeinScorchingArrowDamageMultiplier(),
                GetSeinScorchingArrowPierce(),
                0f);

            currentShotsRemaining -= 1;
            shotCooldown = GetSeinScorchingArrowShotInterval();
            if (currentShotsRemaining <= 0)
            {
                currentShotsRemaining = 0;
                reloadRemaining = GetSeinScorchingArrowReloadSeconds();
            }

            statusLabel = $"Scorching Arrow fired toward ({currentAttackPoint.x:0.0}, {currentAttackPoint.y:0.0}).";
        }

        private bool TryCastSeinBlazingVolley()
        {
            var skill = FindSelectedSkill(SkillSlot.B);
            if (skill == null || !HasLearnedActive(SkillSlot.B) || eveAnchor == null)
            {
                return false;
            }

            if (seinBlazingVolleyReloadRemaining > 0f || seinBlazingVolleyShotCooldownRemaining > 0f)
            {
                return false;
            }

            if (seinBlazingVolleyShotsRemaining <= 0)
            {
                seinBlazingVolleyReloadRemaining = GetSeinBlazingVolleyReloadSeconds(skill);
                return false;
            }

            var target = FindNearestEnemy(eveAnchor.position, GetSeinMapWideSkillRange());
            if (target == null)
            {
                return false;
            }

            var direction = target.Transform.position - eveAnchor.position;
            direction.z = 0f;
            if (direction.sqrMagnitude < 0.01f)
            {
                direction = Vector3.right;
            }

            direction.Normalize();
            var projectileCount = GetSeinBlazingVolleyProjectileCount();
            var damageMultiplier = GetSeinBlazingVolleyDamageMultiplier(projectileCount);
            var perpendicular = new Vector3(-direction.y, direction.x, 0f);
            for (var i = 0; i < projectileCount; i++)
            {
                var offset = (i - (projectileCount - 1) * 0.5f) * 0.16f;
                FireSeinProjectile("BlazingVolley", "sein-b", skill, direction, SeinBlazingVolleySpeed, damageMultiplier, 0, 0f, eveAnchor.position + perpendicular * offset);
            }

            seinBlazingVolleyShotsRemaining -= 1;
            seinBlazingVolleyShotCooldownRemaining = GetSeinBlazingVolleyShotInterval(skill);
            if (seinBlazingVolleyShotsRemaining <= 0)
            {
                seinBlazingVolleyShotsRemaining = 0;
                seinBlazingVolleyReloadRemaining = GetSeinBlazingVolleyReloadSeconds(skill);
            }

            statusLabel = $"Blazing Volley fired {projectileCount} arrow(s). Ammo {seinBlazingVolleyShotsRemaining}/{GetSeinBlazingVolleyMagazineCapacity()}.";
            return true;
        }

        private bool TryCastSeinFlameTrajectory()
        {
            var skill = FindSelectedSkill(SkillSlot.C);
            if (skill == null || !HasLearnedActive(SkillSlot.C) || seinFlameTrajectoryCooldownRemaining > 0f || eveAnchor == null)
            {
                return false;
            }

            var target = FindNearestEnemy(eveAnchor.position, GetSeinMapWideSkillRange());
            if (target == null)
            {
                return false;
            }

            FireSeinFlameTrajectoryProjectile(skill, target);
            seinFlameTrajectoryCooldownRemaining = GetSeinCooldown(skill, 6.5f, HasChoice("sein-c-trait-3") ? 0.80f : 1f);
            statusLabel = $"Flame Trajectory launched at {target.DisplayName}.";
            return true;
        }

        private bool TryCastSeinSuperheatedZone()
        {
            var skill = FindSelectedSkill(SkillSlot.D);
            if (skill == null || !HasLearnedActive(SkillSlot.D) || seinSuperheatedZoneCooldownRemaining > 0f || eveAnchor == null)
            {
                return false;
            }

            var target = FindNearestEnemy(eveAnchor.position, GetSeinMapWideSkillRange());
            if (target == null)
            {
                return false;
            }

            var radius = skill.Radius > 0f ? skill.Radius : 3.2f;
            if (HasChoice("sein-d-trait-3"))
            {
                radius *= 1.30f;
            }

            if (HasSeinThermalSpread() && HasChoice("sein-i-trait-3"))
            {
                radius *= 1.25f;
            }

            var tickInterval = SeinSuperheatedZoneTickInterval;
            if (HasSeinThermalSpread())
            {
                tickInterval *= SpeedBonusToIntervalMultiplier(0.20f);
            }

            if (HasChoice("sein-d-trait-2"))
            {
                tickInterval *= SpeedBonusToIntervalMultiplier(0.25f);
            }

            if (HasChoice("sein-d-master-1"))
            {
                tickInterval *= SpeedBonusToIntervalMultiplier(0.50f);
                radius *= 0.80f;
            }

            var duration = SeinSuperheatedZoneDuration * (HasChoice("sein-d-trait-1") ? 1.25f : 1f);
            var effect = CreateCircleEffect("SeinSuperheatedZone", target.Transform.position, radius, duration);
            effect.SkillId = "sein-d";
            effect.BaseDamage = GetSeinSkillBaseDamage(skill);
            effect.Attribute = DamageAttribute.Fire;
            effect.TickInterval = Mathf.Max(0.05f, tickInterval);
            effect.TickRemaining = 0f;
            effect.SeinSpawnResidualOnExpire = HasChoice("sein-d-master-2");
            if (effect.Renderer != null)
            {
                effect.Renderer.color = new Color(1f, 0.25f, 0.04f, 0.42f);
                effect.Renderer.sortingOrder = 23;
            }

            skillEffects.Add(effect);
            seinSuperheatedZoneCooldownRemaining = GetSeinCooldown(skill, 9f, HasChoice("sein-d-trait-4") ? 0.80f : 1f);
            statusLabel = $"Superheated Zone active for {duration:0.#}s.";
            return true;
        }

        private bool TryCastSeinDoomsdayLine()
        {
            var skill = FindSelectedSkill(SkillSlot.E);
            if (skill == null || !HasLearnedActive(SkillSlot.E) || seinDoomsdayLineCooldownRemaining > 0f || eveAnchor == null)
            {
                return false;
            }

            var target = FindNearestEnemy(eveAnchor.position, GetSeinMapWideSkillRange());
            if (target == null)
            {
                return false;
            }

            var skyOrigin = GetSeinDoomsdaySkyOrigin();
            var direction = target.Transform.position - skyOrigin;
            direction.z = 0f;
            if (direction.sqrMagnitude < 0.01f)
            {
                direction = Vector3.down;
            }

            direction.Normalize();
            var lines = HasChoice("sein-e-trait-4") ? 4 : 3;
            var damageMultiplier = 1f;
            if (HasChoice("sein-e-trait-1"))
            {
                damageMultiplier *= 1.30f;
            }

            if (HasChoice("sein-e-trait-4"))
            {
                damageMultiplier *= 0.85f;
            }

            if (HasChoice("sein-e-master-1"))
            {
                damageMultiplier *= 1.80f;
            }

            var baseDamage = GetSeinSkillBaseDamage(skill) * damageMultiplier;
            var hitEnemies = new System.Collections.Generic.HashSet<EnemyRuntime>();
            for (var i = 0; i < lines; i++)
            {
                var enemy = FindNearestSeinDoomsdayTarget(skyOrigin, hitEnemies);
                if (enemy == null)
                {
                    break;
                }

                CreateSeinDoomsdayTargetLine(skyOrigin, enemy.Transform.position);
                var targetMultiplier = enemy.SeinSuperheatedZoneTimer > 0f && HasChoice("sein-e-trait-5") ? 1.50f : 1f;
                var wasAlive = enemy.CurrentHealth > 0f;
                ApplySeinSkillDamage(enemy, baseDamage, targetMultiplier, "sein-e");
                ApplySeinFireDefenseReduction(enemy, GetSeinDoomsdayFireDefenseReduction());
                ApplySeinDoomsdayOmen(enemy);
                if (wasAlive && enemy.CurrentHealth <= 0f)
                {
                    ChargeSeinCooldownsAfterDoomsdayKill();
                }

                hitEnemies.Add(enemy);
            }

            if (HasChoice("sein-e-master-2"))
            {
                CreateSeinDoomsdayAshZones(hitEnemies);
            }

            seinDoomsdayLineCooldownRemaining = GetSeinCooldown(skill, 16f, GetSeinDoomsdayCooldownMultiplier());
            statusLabel = $"Doomsday Line hit {hitEnemies.Count} enemy(s).";
            return hitEnemies.Count > 0;
        }

        private void FireSeinProjectile(string objectName, string skillId, SkillDefinition skill, Vector3 direction, float speed, float damageMultiplier, int pierce, float criticalChanceBonus, Vector3? spawnPosition = null)
        {
            if (skill == null || projectileRoot == null || eveAnchor == null)
            {
                return;
            }

            direction.z = 0f;
            if (direction.sqrMagnitude < 0.01f)
            {
                direction = Vector3.right;
            }

            direction.Normalize();
            nextProjectileSequence += 1;
            var projectileObject = new GameObject($"{objectName}_{nextProjectileSequence:00}");
            projectileObject.transform.SetParent(projectileRoot, false);
            projectileObject.transform.position = spawnPosition ?? (eveAnchor.position + direction * 0.2f);
            projectileObject.transform.localScale = new Vector3(projectileHitRadiusConfigured, projectileHitRadiusConfigured, 1f);

            var renderer = projectileObject.AddComponent<SpriteRenderer>();
            renderer.sprite = selectedProjectileSprite != null ? selectedProjectileSprite : GetSharedSprite();
            renderer.color = Color.white;
            renderer.sortingOrder = 25;

            projectiles.Add(new ProjectileRuntime
            {
                GameObject = projectileObject,
                Transform = projectileObject.transform,
                Renderer = renderer,
                Direction = direction,
                Speed = speed,
                RemainingLifetime = 60f,
                HitRadius = projectileHitRadiusConfigured,
                BaseDamage = GetSeinSkillBaseDamage(skill) * damageMultiplier,
                Attribute = DamageAttribute.Fire,
                SkillId = skillId,
                RemainingPierce = Mathf.Max(0, pierce)
            });
        }

        private void FireSeinFlameTrajectoryProjectile(SkillDefinition skill, EnemyRuntime target)
        {
            if (skill == null || target == null || target.Transform == null || projectileRoot == null || eveAnchor == null)
            {
                return;
            }

            var targetPosition = target.Transform.position;
            var direction = targetPosition - eveAnchor.position;
            direction.z = 0f;
            if (direction.sqrMagnitude < 0.01f)
            {
                direction = Vector3.right;
            }

            direction.Normalize();
            var radius = GetSeinFlameTrajectoryRadius(skill);
            if (HasChoice("sein-c-master-2"))
            {
                radius *= 1.25f;
            }

            var delayMultiplier = HasChoice("sein-c-trait-4") ? 0.60f : 1f;
            var speed = SeinFlameTrajectoryProjectileSpeed / Mathf.Max(0.05f, delayMultiplier);
            nextProjectileSequence += 1;
            var projectileObject = new GameObject($"FlameTrajectoryArrow_{nextProjectileSequence:00}");
            projectileObject.transform.SetParent(projectileRoot, false);
            projectileObject.transform.position = eveAnchor.position + direction * 0.2f;
            projectileObject.transform.localScale = new Vector3(projectileHitRadiusConfigured, projectileHitRadiusConfigured, 1f);

            var renderer = projectileObject.AddComponent<SpriteRenderer>();
            renderer.sprite = selectedProjectileSprite != null ? selectedProjectileSprite : GetSharedSprite();
            renderer.color = new Color(1f, 0.48f, 0.12f, 1f);
            renderer.sortingOrder = 25;

            projectiles.Add(new ProjectileRuntime
            {
                GameObject = projectileObject,
                Transform = projectileObject.transform,
                Renderer = renderer,
                Direction = direction,
                Speed = speed,
                RemainingLifetime = 3f,
                HitRadius = projectileHitRadiusConfigured,
                BaseDamage = GetSeinSkillBaseDamage(skill) * GetSeinFlameTrajectoryDamageMultiplier(target),
                Attribute = DamageAttribute.Fire,
                SkillId = "sein-c",
                RemainingPierce = 0,
                LockedEnemyTarget = target,
                SeinExplodesOnLockedTarget = true,
                SeinExplosionRadius = radius,
                SeinExplosionDamageMultiplier = 1f
            });
        }

        private void UpdateSeinLockedTargetProjectile(ProjectileRuntime projectile)
        {
            if (projectile == null || projectile.LockedEnemyTarget == null || projectile.LockedEnemyTarget.Transform == null)
            {
                return;
            }

            var toTarget = projectile.LockedEnemyTarget.Transform.position - projectile.Transform.position;
            toTarget.z = 0f;
            if (toTarget.sqrMagnitude < 0.01f)
            {
                return;
            }

            var targetDirection = toTarget.normalized;
            projectile.Direction = Vector3.Slerp(projectile.Direction, targetDirection, Mathf.Clamp01(Time.deltaTime * 8f)).normalized;
            projectile.Transform.right = projectile.Direction;
            var progress = Mathf.Clamp01(1f - (toTarget.magnitude / Mathf.Max(0.1f, GetSeinMapWideSkillRange())));
            var arcHeight = Mathf.Sin(progress * Mathf.PI) * 0.45f;
            projectile.Transform.localScale = new Vector3(projectileHitRadiusConfigured, projectileHitRadiusConfigured + arcHeight, 1f);
        }

        private void CreateSeinFlameTrajectoryPathSegment(ProjectileRuntime projectile, Vector3 start, Vector3 end)
        {
            if (projectile == null
                || !string.Equals(projectile.SkillId, "sein-c", StringComparison.OrdinalIgnoreCase)
                || !HasChoice("sein-c-master-2"))
            {
                return;
            }

            var direction = end - start;
            direction.z = 0f;
            var length = direction.magnitude;
            if (length <= 0.01f)
            {
                return;
            }

            direction /= length;
            var effect = CreateLineEffect("SeinFlameTrajectoryPathSegment", start, direction, length, 0.75f, 0.22f);
            effect.SkillId = "sein-c-path-visual";
            if (effect.Renderer != null)
            {
                effect.Renderer.color = new Color(1f, 0.56f, 0.22f, 0.48f);
                effect.Renderer.sortingOrder = 24;
            }

            skillEffects.Add(effect);
            for (var i = 0; i < enemies.Count; i++)
            {
                var enemy = enemies[i];
                if (enemy == null
                    || enemy == projectile.LockedEnemyTarget
                    || enemy.CurrentHealth <= 0f
                    || enemy.Transform == null
                    || projectile.HitEnemies.Contains(enemy)
                    || !IsPointInsideBeam(enemy.Transform.position, effect))
                {
                    continue;
                }

                ApplySeinSkillDamage(enemy, projectile.BaseDamage * 0.40f, 1f, "sein-c-path");
                projectile.HitEnemies.Add(enemy);
            }
        }

        private bool TryHandleSeinFlameTrajectoryImpact(ProjectileRuntime projectile, EnemyRuntime enemy)
        {
            if (projectile == null
                || enemy == null
                || !string.Equals(projectile.SkillId, "sein-c", StringComparison.OrdinalIgnoreCase)
                || !projectile.SeinExplodesOnLockedTarget)
            {
                return false;
            }

            var delay = SeinFlameTrajectoryDelay * (HasChoice("sein-c-trait-4") ? 0.60f : 1f);
            var effect = CreateCircleEffect("SeinFlameTrajectoryImpact", enemy.Transform.position, projectile.SeinExplosionRadius, delay + 0.08f);
            effect.SkillId = "sein-c";
            effect.BaseDamage = projectile.BaseDamage * projectile.SeinExplosionDamageMultiplier;
            effect.Attribute = DamageAttribute.Fire;
            effect.TickInterval = 999f;
            effect.TickRemaining = delay;
            effect.SeinSpawnResidualOnExpire = HasChoice("sein-c-master-1");
            if (effect.Renderer != null)
            {
                effect.Renderer.color = new Color(1f, 0.36f, 0.12f, 0.55f);
                effect.Renderer.sortingOrder = 24;
            }

            skillEffects.Add(effect);
            statusLabel = $"Flame Trajectory impacts {enemy.DisplayName}; explosion in {delay:0.#}s.";
            return true;
        }

        private void TrackSeinProjectileHit(ProjectileRuntime projectile, EnemyRuntime enemy, float appliedDamage)
        {
            if (!IsSelectedSeinMonster() || projectile == null || enemy == null || appliedDamage <= 0f)
            {
                return;
            }

            if (string.Equals(projectile.SkillId, "sein-a", StringComparison.OrdinalIgnoreCase))
            {
                enemy.SeinScorchingArrowTimer = Mathf.Max(enemy.SeinScorchingArrowTimer, 4f);
                if (HasChoice("sein-a-master-2"))
                {
                    ApplySeinAreaDamage(enemy.Transform.position, 1.35f, projectile.BaseDamage * 0.50f, "sein-a-explosion");
                }
            }

            if (string.Equals(projectile.SkillId, "sein-c", StringComparison.OrdinalIgnoreCase)
                && projectile.SeinExplodesOnLockedTarget)
            {
                return;
            }

            TryTriggerSeinFlameBarrageProc(enemy, projectile.SkillId);
        }

        private bool IsSeinSkillEffect(SkillEffectRuntime effect)
        {
            return effect != null && !string.IsNullOrWhiteSpace(effect.SkillId) && effect.SkillId.StartsWith("sein-", StringComparison.OrdinalIgnoreCase);
        }

        private void ApplySeinSkillEffectDamage(SkillEffectRuntime effect, EnemyRuntime enemy)
        {
            if (effect == null || enemy == null)
            {
                return;
            }

            var finalMultiplier = 1f;
            if (string.Equals(effect.SkillId, "sein-d", StringComparison.OrdinalIgnoreCase)
                || string.Equals(effect.SkillId, "sein-d-residual", StringComparison.OrdinalIgnoreCase))
            {
                enemy.SeinSuperheatedZoneTimer = Mathf.Max(enemy.SeinSuperheatedZoneTimer, 0.7f);
                enemy.SeinSuperheatedTickCount += 1;
                ApplySeinThermalSpread(enemy);
                if (HasChoice("sein-d-trait-5") && enemy.SeinSuperheatedTickCount >= 4)
                {
                    finalMultiplier *= 1.35f;
                }
            }

            var applied = ApplySeinSkillDamage(enemy, effect.BaseDamage, finalMultiplier, effect.SkillId);
            if (applied > 0f && string.Equals(effect.SkillId, "sein-c", StringComparison.OrdinalIgnoreCase))
            {
                ApplySeinBurningTrajectory(enemy);
            }
        }

        private void TryHandleSkillEffectExpired(SkillEffectRuntime effect)
        {
            if (effect == null || !effect.SeinSpawnResidualOnExpire)
            {
                return;
            }

            if (string.Equals(effect.SkillId, "sein-c", StringComparison.OrdinalIgnoreCase))
            {
                var fallingResidual = CreateCircleEffect("SeinFallingTrajectoryResidual", effect.Transform.position, Mathf.Max(0.75f, effect.Radius * 0.65f), 2f);
                fallingResidual.SkillId = "sein-c-residual";
                fallingResidual.BaseDamage = effect.BaseDamage * 0.25f;
                fallingResidual.Attribute = DamageAttribute.Fire;
                fallingResidual.TickInterval = SeinSuperheatedZoneTickInterval;
                fallingResidual.TickRemaining = 0f;
                if (fallingResidual.Renderer != null)
                {
                    fallingResidual.Renderer.color = new Color(1f, 0.42f, 0.10f, 0.32f);
                    fallingResidual.Renderer.sortingOrder = 22;
                }

                skillEffects.Add(fallingResidual);
                return;
            }

            var residual = CreateCircleEffect("SeinResidualZone", effect.Transform.position, effect.Radius, 3f);
            residual.SkillId = "sein-d-residual";
            residual.BaseDamage = effect.BaseDamage * 0.40f;
            residual.Attribute = DamageAttribute.Fire;
            residual.TickInterval = SeinSuperheatedZoneTickInterval;
            residual.TickRemaining = 0f;
            if (residual.Renderer != null)
            {
                residual.Renderer.color = new Color(1f, 0.45f, 0.12f, 0.32f);
                residual.Renderer.sortingOrder = 22;
            }

            skillEffects.Add(residual);
        }

        private void ApplySeinLineDamage(Vector3 start, Vector3 end, float baseDamage, float width, string skillId)
        {
            var direction = end - start;
            direction.z = 0f;
            var length = direction.magnitude;
            if (length <= 0.01f)
            {
                return;
            }

            direction /= length;
            var effect = CreateLineEffect("SeinFlameTrajectoryPath", start, direction, length, width, 0.25f);
            effect.SkillId = skillId;
            if (effect.Renderer != null)
            {
                effect.Renderer.color = new Color(1f, 0.56f, 0.22f, 0.48f);
                effect.Renderer.sortingOrder = 24;
            }

            skillEffects.Add(effect);
            for (var i = 0; i < enemies.Count; i++)
            {
                var enemy = enemies[i];
                if (enemy == null || enemy.CurrentHealth <= 0f || enemy.Transform == null || !IsPointInsideBeam(enemy.Transform.position, effect))
                {
                    continue;
                }

                ApplySeinSkillDamage(enemy, baseDamage, 1f, skillId);
            }
        }

        private void CreateSeinDoomsdayTargetLine(Vector3 start, Vector3 end)
        {
            var direction = end - start;
            direction.z = 0f;
            var length = direction.magnitude;
            if (length <= 0.01f)
            {
                return;
            }

            direction /= length;
            var effect = CreateLineEffect("SeinDoomsdayLine", start, direction, length, SeinDoomsdayLineWidth, 0.4f);
            effect.SkillId = "sein-e";
            if (effect.Renderer != null)
            {
                effect.Renderer.color = new Color(1f, 0.18f, 0.02f, 0.62f);
                effect.Renderer.sortingOrder = 25;
            }

            skillEffects.Add(effect);
        }

        private EnemyRuntime FindNearestSeinDoomsdayTarget(Vector3 skyOrigin, System.Collections.Generic.HashSet<EnemyRuntime> excluded)
        {
            EnemyRuntime best = null;
            var bestDistance = float.MaxValue;
            for (var i = 0; i < enemies.Count; i++)
            {
                var enemy = enemies[i];
                if (enemy == null || enemy.CurrentHealth <= 0f || enemy.Transform == null || (excluded != null && excluded.Contains(enemy)))
                {
                    continue;
                }

                var distance = Vector2.Distance(skyOrigin, enemy.Transform.position);
                if (distance >= bestDistance)
                {
                    continue;
                }

                best = enemy;
                bestDistance = distance;
            }

            return best;
        }

        private void ApplySeinAreaDamage(Vector3 center, float radius, float baseDamage, string skillId, EnemyRuntime excludedEnemy = null)
        {
            for (var i = 0; i < enemies.Count; i++)
            {
                var enemy = enemies[i];
                if (enemy == null
                    || enemy == excludedEnemy
                    || enemy.CurrentHealth <= 0f
                    || enemy.Transform == null
                    || Vector2.Distance(enemy.Transform.position, center) > radius + GetEnemyHitRadius(enemy))
                {
                    continue;
                }

                ApplySeinSkillDamage(enemy, baseDamage, 1f, skillId);
            }

            var effect = CreateCircleEffect("SeinSmallExplosion", center, radius, 0.25f);
            effect.SkillId = skillId;
            if (effect.Renderer != null)
            {
                effect.Renderer.color = new Color(1f, 0.30f, 0.05f, 0.50f);
                effect.Renderer.sortingOrder = 25;
            }

            skillEffects.Add(effect);
        }

        private void CreateSeinDoomsdayAshZones(System.Collections.Generic.IEnumerable<EnemyRuntime> targets)
        {
            if (targets == null)
            {
                return;
            }

            var skill = FindSelectedSkill(SkillSlot.D);
            var damage = (skill != null ? GetSeinSkillBaseDamage(skill) : 12f + powerStatConfigured * 0.55f) * 0.40f;
            var created = 0;
            foreach (var target in targets)
            {
                if (target == null || target.Transform == null || created >= 3)
                {
                    continue;
                }

                var effect = CreateCircleEffect("SeinAshSuperheatedZone", target.Transform.position, 2.4f, 3f);
                effect.SkillId = "sein-e-ash";
                effect.BaseDamage = damage;
                effect.Attribute = DamageAttribute.Fire;
                effect.TickInterval = SeinSuperheatedZoneTickInterval;
                effect.TickRemaining = 0f;
                if (effect.Renderer != null)
                {
                    effect.Renderer.color = new Color(1f, 0.35f, 0.12f, 0.30f);
                    effect.Renderer.sortingOrder = 22;
                }

                skillEffects.Add(effect);
                created += 1;
            }
        }

        private float ApplySeinSkillDamage(EnemyRuntime enemy, float baseDamage, float finalMultiplier, string skillId)
        {
            if (enemy == null || baseDamage <= 0f)
            {
                return 0f;
            }

            var result = DamageCalculator.Resolve(
                baseDamage,
                DamageAttribute.Fire,
                enemy.Defenses,
                flatDefenseReduction: GetSeinFlatDefenseReduction(enemy, DamageAttribute.Fire),
                criticalChanceBonus: GetSeinCriticalChanceBonus(DamageAttribute.Fire),
                criticalMultiplierBonus: GetSeinCriticalMultiplierBonus(DamageAttribute.Fire),
                targetCriticalResistance: enemy.CriticalResistance,
                finalDamageMultiplier: enemy.DamageTakenMultiplier * Mathf.Max(0f, finalMultiplier) * GetSeinFinalDamageMultiplier(enemy, DamageAttribute.Fire, skillId));
            var applied = ApplyDamageToEnemy(enemy, result.FinalDamage, DamageAttribute.Fire);
            enemy.FlashTimer = 0.08f;
            if (applied > 0f)
            {
                TryTriggerSeinFlameBarrageProc(enemy, skillId);
            }

            Debug.Log($"[CombatDamage] Sein.{skillId} -> {enemy.DisplayName}: {result.FormulaLog}; Applied={applied:0.##}, ShieldLeft={enemy.ShieldValue:0.##}, HpLeft={Mathf.Max(0f, enemy.CurrentHealth):0.##}");
            return applied;
        }

        private void ApplySeinFireDefenseReduction(EnemyRuntime enemy, float flatReduction)
        {
            if (enemy == null || flatReduction <= 0f)
            {
                return;
            }

            enemy.SeinFireDefenseReduction = Mathf.Max(enemy.SeinFireDefenseReduction, flatReduction);
            enemy.SeinFireDefenseReductionTimer = Mathf.Max(enemy.SeinFireDefenseReductionTimer, SeinFireDefenseReductionDuration);
        }

        private float GetSeinFlatDefenseReduction(EnemyRuntime enemy, DamageAttribute attribute)
        {
            if (!IsSelectedSeinMonster() || enemy == null || attribute != DamageAttribute.Fire || enemy.SeinFireDefenseReductionTimer <= 0f)
            {
                return 0f;
            }

            return Mathf.Max(0f, enemy.SeinFireDefenseReduction);
        }

        private float GetSeinFinalDamageMultiplier(EnemyRuntime enemy, DamageAttribute attribute, string skillId)
        {
            if (!IsSelectedSeinMonster() || attribute != DamageAttribute.Fire)
            {
                return 1f;
            }

            var bonus = 0f;
            if (HasSeinHeatedAim())
            {
                bonus += 0.12f + (HasChoice("sein-f-trait-1") ? 0.06f : 0f);
            }

            if (enemy != null && enemy.SeinBurningTrajectoryTimer > 0f)
            {
                bonus += enemy.SeinBurningTrajectoryDamageTakenBonus;
            }

            if (enemy != null && enemy.SeinThermalSpreadTimer > 0f)
            {
                bonus += enemy.SeinThermalSpreadDamageTakenBonus;
            }

            if (enemy != null && enemy.SeinDoomsdayOmenTimer > 0f)
            {
                bonus += enemy.SeinDoomsdayOmenDamageTakenBonus;
            }

            return 1f + bonus;
        }

        private float GetSeinCriticalChanceBonus(DamageAttribute attribute)
        {
            return IsSelectedSeinMonster() && attribute == DamageAttribute.Fire && HasSeinHeatedAim()
                ? 0.08f
                : 0f;
        }

        private float GetSeinCriticalMultiplierBonus(DamageAttribute attribute)
        {
            return IsSelectedSeinMonster() && attribute == DamageAttribute.Fire && HasSeinHeatedAim() && HasChoice("sein-f-trait-3")
                ? 0.20f
                : 0f;
        }

        private void ApplySeinBurningTrajectory(EnemyRuntime enemy)
        {
            if (!HasSeinBurningTrajectory() || enemy == null)
            {
                return;
            }

            ApplySeinFireDefenseReduction(enemy, 12f + (HasChoice("sein-h-trait-1") ? 6f : 0f));
            if (HasChoice("sein-h-trait-3"))
            {
                enemy.SeinBurningTrajectoryDamageTakenBonus = Mathf.Max(enemy.SeinBurningTrajectoryDamageTakenBonus, 0.10f);
                enemy.SeinBurningTrajectoryTimer = Mathf.Max(enemy.SeinBurningTrajectoryTimer, 5f);
            }
        }

        private void ApplySeinThermalSpread(EnemyRuntime enemy)
        {
            if (!HasSeinThermalSpread() || enemy == null)
            {
                return;
            }

            enemy.SeinThermalSpreadDamageTakenBonus = Mathf.Max(
                enemy.SeinThermalSpreadDamageTakenBonus,
                0.15f + (HasChoice("sein-i-trait-1") ? 0.07f : 0f));
            enemy.SeinThermalSpreadTimer = Mathf.Max(enemy.SeinThermalSpreadTimer, HasChoice("sein-i-trait-2") ? 6f : 4f);
        }

        private void ApplySeinDoomsdayOmen(EnemyRuntime enemy)
        {
            if (!HasSeinDoomsdayOmen() || enemy == null)
            {
                return;
            }

            enemy.SeinDoomsdayOmenDamageTakenBonus = Mathf.Max(
                enemy.SeinDoomsdayOmenDamageTakenBonus,
                0.20f + (HasChoice("sein-j-trait-1") ? 0.10f : 0f));
            enemy.SeinDoomsdayOmenTimer = Mathf.Max(enemy.SeinDoomsdayOmenTimer, 5f);
        }

        private void TryTriggerSeinFlameBarrageProc(EnemyRuntime sourceEnemy, string sourceSkillId)
        {
            if (!HasSeinFlameBarrage()
                || sourceEnemy == null
                || sourceEnemy.CurrentHealth <= 0f
                || seinFlameBarrageProcCooldownRemaining > 0f
                || string.Equals(sourceSkillId, "sein-g", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var chance = 0.04f + (HasChoice("sein-g-trait-1") ? 0.03f : 0f);
            if (UnityEngine.Random.value >= chance)
            {
                return;
            }

            if (FireSeinFlameBarrageProc(sourceEnemy))
            {
                seinFlameBarrageProcCooldownRemaining = 1.5f;
                if (HasChoice("sein-g-trait-3") && reloadRemaining > 0f)
                {
                    reloadRemaining *= 0.90f;
                }
            }
        }

        private bool FireSeinFlameBarrageProc(EnemyRuntime target)
        {
            var skill = FindSelectedSkill(SkillSlot.B);
            if (skill == null || !HasLearnedActive(SkillSlot.B) || target == null || target.Transform == null || eveAnchor == null)
            {
                return false;
            }

            var direction = target.Transform.position - eveAnchor.position;
            direction.z = 0f;
            if (direction.sqrMagnitude < 0.01f)
            {
                direction = Vector3.right;
            }

            direction.Normalize();
            var projectileCount = GetSeinBlazingVolleyProjectileCount();
            var damageMultiplier = 0.60f + (HasChoice("sein-g-trait-2") ? 0.20f : 0f);
            for (var i = 0; i < projectileCount; i++)
            {
                var offsetAngle = (i - (projectileCount - 1) * 0.5f) * 2.5f;
                var rotatedDirection = Quaternion.Euler(0f, 0f, offsetAngle) * direction;
                FireSeinProjectile("FlameBarrage", "sein-g", skill, rotatedDirection, SeinBlazingVolleySpeed, damageMultiplier, 0, 0f);
            }

            statusLabel = $"Flame Barrage triggered {projectileCount} arrow(s).";
            return true;
        }

        private void ChargeSeinCooldownsAfterDoomsdayKill()
        {
            if (!HasSeinDoomsdayOmen())
            {
                return;
            }

            var chargeRatio = 0.20f + (HasChoice("sein-j-trait-2") ? 0.10f : 0f);
            ReduceCooldownByRatio(ref seinBlazingVolleyCooldownRemaining, GetSeinBlazingVolleyCooldown(FindSelectedSkill(SkillSlot.B)), chargeRatio);
            ReduceCooldownByRatio(ref seinFlameTrajectoryCooldownRemaining, GetSeinCooldown(FindSelectedSkill(SkillSlot.C), 6.5f, HasChoice("sein-c-trait-3") ? 0.80f : 1f), chargeRatio);
            ReduceCooldownByRatio(ref seinSuperheatedZoneCooldownRemaining, GetSeinCooldown(FindSelectedSkill(SkillSlot.D), 9f, HasChoice("sein-d-trait-4") ? 0.80f : 1f), chargeRatio);
            ReduceCooldownByRatio(ref seinDoomsdayLineCooldownRemaining, GetSeinCooldown(FindSelectedSkill(SkillSlot.E), 16f, GetSeinDoomsdayCooldownMultiplier()), chargeRatio);
            if (reloadRemaining > 0f)
            {
                reloadRemaining = Mathf.Max(0f, reloadRemaining - GetSeinScorchingArrowReloadSeconds() * chargeRatio);
            }
        }

        private static void ReduceCooldownByRatio(ref float cooldownRemaining, float cooldownDuration, float ratio)
        {
            cooldownRemaining = Mathf.Max(0f, cooldownRemaining - Mathf.Max(0f, cooldownDuration) * Mathf.Clamp01(ratio));
        }

        private float GetSeinSkillBaseDamage(SkillDefinition skill)
        {
            return skill != null
                ? skill.BaseDamage + powerStatConfigured * skill.AttackPowerCoefficient
                : 0f;
        }

        private int GetSeinScorchingArrowMagazineCapacity()
        {
            var skill = FindSelectedSkill(SkillSlot.A);
            var capacity = skill != null && skill.MagazineCapacity > 0 ? skill.MagazineCapacity : magazineCapacityConfigured;
            if (HasChoice("sein-a-trait-2"))
            {
                capacity += 4;
            }

            if (HasSeinHeatedAim() && HasChoice("sein-f-trait-2"))
            {
                capacity += 3;
            }

            return Mathf.Max(1, capacity);
        }

        private float GetSeinScorchingArrowReloadSeconds()
        {
            var skill = FindSelectedSkill(SkillSlot.A);
            var reload = skill != null && skill.ReloadSeconds > 0f ? skill.ReloadSeconds : reloadDurationConfigured;
            if (HasChoice("sein-a-trait-3"))
            {
                reload *= SpeedBonusToIntervalMultiplier(0.30f);
            }

            return Mathf.Max(0.25f, reload);
        }

        private float GetSeinScorchingArrowShotInterval()
        {
            var skill = FindSelectedSkill(SkillSlot.A);
            var interval = skill != null && skill.ShotIntervalSeconds > 0f ? skill.ShotIntervalSeconds : shotIntervalConfigured;
            if (HasChoice("sein-a-trait-5"))
            {
                interval *= 0.80f;
            }

            return Mathf.Max(0.05f, interval);
        }

        private int GetSeinScorchingArrowPierce()
        {
            var pierce = 1;
            if (HasChoice("sein-a-trait-4"))
            {
                pierce += 1;
            }

            if (HasChoice("sein-a-master-1"))
            {
                pierce += 1;
            }

            return Mathf.Max(0, pierce);
        }

        private float GetSeinScorchingArrowDamageMultiplier()
        {
            var multiplier = 1f;
            if (HasChoice("sein-a-trait-1"))
            {
                multiplier *= 1.25f;
            }

            if (HasChoice("sein-a-trait-4"))
            {
                multiplier *= 1.10f;
            }

            if (HasChoice("sein-a-trait-5"))
            {
                multiplier *= 0.90f;
            }

            if (HasChoice("sein-a-master-1"))
            {
                multiplier *= 1.55f;
            }

            return multiplier;
        }

        private int GetSeinBlazingVolleyProjectileCount()
        {
            var count = 5;
            if (HasChoice("sein-b-trait-1"))
            {
                count += 2;
            }

            if (HasChoice("sein-b-master-1"))
            {
                count += 4;
            }

            if (HasChoice("sein-b-master-2"))
            {
                count -= 2;
            }

            return Mathf.Max(1, count);
        }

        private int GetSeinBlazingVolleyMagazineCapacity()
        {
            var skill = FindSelectedSkill(SkillSlot.B);
            return Mathf.Max(1, skill != null && skill.MagazineCapacity > 0 ? skill.MagazineCapacity : 4);
        }

        private int GetSeinBlazingVolleyCurrentAmmo()
        {
            if (seinBlazingVolleyShotsRemaining <= 0 && seinBlazingVolleyReloadRemaining <= 0f)
            {
                seinBlazingVolleyShotsRemaining = GetSeinBlazingVolleyMagazineCapacity();
            }

            return Mathf.Clamp(seinBlazingVolleyShotsRemaining, 0, GetSeinBlazingVolleyMagazineCapacity());
        }

        private float GetSeinBlazingVolleyDamageMultiplier(int projectileCount)
        {
            var multiplier = 1f;
            if (HasChoice("sein-b-trait-2"))
            {
                multiplier *= 1.25f;
            }

            if (HasChoice("sein-b-master-1"))
            {
                multiplier *= 0.80f;
            }

            if (HasChoice("sein-b-master-2"))
            {
                multiplier *= 1.90f;
            }

            return multiplier;
        }

        private float GetSeinBlazingVolleyCooldown(SkillDefinition skill)
        {
            return GetSeinBlazingVolleyReloadSeconds(skill);
        }

        private float GetSeinBlazingVolleyCooldownRemaining()
        {
            return seinBlazingVolleyReloadRemaining > 0f
                ? seinBlazingVolleyReloadRemaining
                : seinBlazingVolleyShotCooldownRemaining;
        }

        private float GetSeinBlazingVolleyCooldownDuration(SkillDefinition skill)
        {
            return seinBlazingVolleyReloadRemaining > 0f
                ? GetSeinBlazingVolleyReloadSeconds(skill)
                : GetSeinBlazingVolleyShotInterval(skill);
        }

        private float GetSeinBlazingVolleyReloadSeconds(SkillDefinition skill)
        {
            var cooldown = skill != null && skill.ReloadSeconds > 0f ? skill.ReloadSeconds : 6f;
            if (HasChoice("sein-b-trait-3"))
            {
                cooldown *= SpeedBonusToIntervalMultiplier(0.30f);
            }

            if (HasChoice("sein-b-trait-4"))
            {
                cooldown *= 0.90f;
            }

            return Mathf.Max(0.1f, cooldown);
        }

        private float GetSeinBlazingVolleyShotInterval(SkillDefinition skill)
        {
            var interval = skill != null && skill.ShotIntervalSeconds > 0f ? skill.ShotIntervalSeconds : 0.18f;
            if (HasChoice("sein-b-trait-4"))
            {
                interval *= 0.75f;
            }

            return Mathf.Max(0.05f, interval);
        }

        private float GetSeinFlameTrajectoryRadius(SkillDefinition skill)
        {
            var radius = skill != null && skill.Radius > 0f ? skill.Radius : 1.8f;
            if (HasChoice("sein-c-trait-2"))
            {
                radius *= 1.25f;
            }

            if (HasSeinBurningTrajectory() && HasChoice("sein-h-trait-2"))
            {
                radius *= 1.25f;
            }

            return radius;
        }

        private float GetSeinFlameTrajectoryDamageMultiplier(EnemyRuntime target)
        {
            var multiplier = 1f;
            if (HasChoice("sein-c-trait-1"))
            {
                multiplier *= 1.30f;
            }

            if (target != null && target.SeinScorchingArrowTimer > 0f && HasChoice("sein-c-trait-5"))
            {
                multiplier *= 1.35f;
            }

            return multiplier;
        }

        private float GetSeinDoomsdayFireDefenseReduction()
        {
            return 10f
                + (HasChoice("sein-e-trait-3") ? 8f : 0f)
                + (HasSeinDoomsdayOmen() && HasChoice("sein-j-trait-3") ? 8f : 0f);
        }

        private float GetSeinDoomsdayCooldownMultiplier()
        {
            var multiplier = HasChoice("sein-e-trait-2") ? 0.80f : 1f;
            if (HasChoice("sein-e-master-1"))
            {
                multiplier *= 1.25f;
            }

            return multiplier;
        }

        private Vector3 GetSeinDoomsdaySkyOrigin()
        {
            var x = Mathf.Clamp(SeinDoomsdaySkyOriginX, 0f, Mathf.Max(SeinDoomsdaySkyOriginX, fieldSize.x));
            var y = Mathf.Clamp(SeinDoomsdaySkyOriginY, 0f, Mathf.Max(SeinDoomsdaySkyOriginY, fieldSize.y));
            return new Vector3(x, y, 0f);
        }

        private float GetSeinCooldown(SkillDefinition skill, float fallback, float multiplier)
        {
            var cooldown = skill != null && skill.CooldownSeconds > 0f ? skill.CooldownSeconds : fallback;
            return Mathf.Max(0.1f, cooldown * Mathf.Max(0.05f, multiplier));
        }

        private float GetSeinActionSpeedMultiplier()
        {
            return 1f;
        }

        private float GetSeinMapWideSkillRange()
        {
            var width = Mathf.Max(fieldSize.x, EnemySpawnX);
            var height = Mathf.Max(fieldSize.y, BattlefieldMaxY);
            return Mathf.Sqrt(width * width + height * height) + 2f;
        }

        private bool IsSelectedSeinMonster()
        {
            return selectedMonster != null &&
                string.Equals(selectedMonster.MonsterId, "sein", StringComparison.OrdinalIgnoreCase);
        }

        private bool HasSeinPassive(string passiveId, string passiveName)
        {
            return IsSelectedSeinMonster()
                && ((!string.IsNullOrWhiteSpace(passiveId) && chosenSkillChoiceIds.Contains(passiveId))
                    || (!string.IsNullOrWhiteSpace(passiveId) && learnedPassiveSkillIds.Contains(passiveId)));
        }

        private bool HasSeinHeatedAim()
        {
            return HasSeinPassive("sein-f", "가열 조준");
        }

        private bool HasSeinFlameBarrage()
        {
            return HasSeinPassive("sein-g", "불꽃 탄막");
        }

        private bool HasSeinBurningTrajectory()
        {
            return HasSeinPassive("sein-h", "연소 궤적");
        }

        private bool HasSeinThermalSpread()
        {
            return HasSeinPassive("sein-i", "열압 확산");
        }

        private bool HasSeinDoomsdayOmen()
        {
            return HasSeinPassive("sein-j", "종말 예고");
        }
    }
}
