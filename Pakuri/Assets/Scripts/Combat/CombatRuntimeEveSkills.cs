using System;
using Pakuri.Data;
using Pakuri.Run;
using UnityEngine;

namespace Pakuri.Combat
{
    public partial class CombatRuntimeController
    {
        private const float EveBeamDuration = 1.2f;
        private const float EveBeamTickInterval = 0.15f;
        private const float EveFrostFieldDuration = 4f;
        private const float EveFrostFieldTickInterval = 0.5f;
        private const float EveDroneDuration = 5f;
        private const float EveDroneAttackPeriod = 0.8f;
        private const int EveDroneMagazineCapacity = 3;
        private const float EveDroneReloadSeconds = 6f;
        private const float EveVoltageShieldDuration = 12f;
        private const float EveParticleSeparationCooldown = 1.5f;
        private const float EveArcExtraProjectileAngleStep = 3f;
        private const float EveArcBranchRadius = 4.5f;
        private const float EveArcBranchLineWidth = 0.06f;
        private const float EveArcBranchLineDuration = 0.12f;

        private float eveBeamCooldownRemaining;
        private float eveFrostCooldownRemaining;
        private float eveStaticCooldownRemaining;
        private float eveDroneReloadRemaining;
        private float eveParticleSeparationCooldownRemaining;
        private int eveDroneChargesRemaining = EveDroneMagazineCapacity;

        private void ConfigureEveSkillSelectionState(RunSession session)
        {
            learnedActiveSkillIds.Clear();
            learnedPassiveSkillIds.Clear();
            chosenSkillChoiceIds.Clear();

            if (session != null && session.LearnedActives != null)
            {
                for (var i = 0; i < session.LearnedActives.Count; i++)
                {
                    if (!string.IsNullOrWhiteSpace(session.LearnedActives[i]))
                    {
                        learnedActiveSkillIds.Add(session.LearnedActives[i]);
                    }
                }
            }

            if (session != null && session.ChosenRewardIds != null)
            {
                for (var i = 0; i < session.ChosenRewardIds.Count; i++)
                {
                    if (!string.IsNullOrWhiteSpace(session.ChosenRewardIds[i]))
                    {
                        chosenSkillChoiceIds.Add(session.ChosenRewardIds[i]);
                    }
                }
            }

            if (session != null && session.LearnedPassives != null)
            {
                for (var i = 0; i < session.LearnedPassives.Count; i++)
                {
                    if (!string.IsNullOrWhiteSpace(session.LearnedPassives[i]))
                    {
                        learnedPassiveSkillIds.Add(session.LearnedPassives[i]);
                    }
                }
            }

            var arcBolt = FindSelectedSkill(SkillSlot.A);
            if (arcBolt != null && !string.IsNullOrWhiteSpace(arcBolt.SkillId))
            {
                learnedActiveSkillIds.Add(arcBolt.SkillId);
            }
            else if (!string.IsNullOrWhiteSpace(selectedActiveSkillId))
            {
                learnedActiveSkillIds.Add(selectedActiveSkillId);
            }
        }

        private void ResetEveSkillCombatTimers()
        {
            eveBeamCooldownRemaining = 0f;
            eveFrostCooldownRemaining = 0f;
            eveStaticCooldownRemaining = 0f;
            eveDroneReloadRemaining = 0f;
            eveParticleSeparationCooldownRemaining = 0f;
            eveDroneChargesRemaining = GetEveDroneMagazine();
            ApplyEveVoltageCalibrationShield();
        }

        private void UpdateEveSkillCooldowns()
        {
            var elapsed = Time.deltaTime * GetEveActionSpeedMultiplier();
            eveBeamCooldownRemaining = Mathf.Max(0f, eveBeamCooldownRemaining - elapsed);
            eveFrostCooldownRemaining = Mathf.Max(0f, eveFrostCooldownRemaining - elapsed);
            eveStaticCooldownRemaining = Mathf.Max(0f, eveStaticCooldownRemaining - elapsed);
            eveParticleSeparationCooldownRemaining = Mathf.Max(0f, eveParticleSeparationCooldownRemaining - elapsed);
            unitShieldTimer = Mathf.Max(0f, unitShieldTimer - Time.deltaTime);
            if (Mathf.Approximately(unitShieldTimer, 0f))
            {
                unitShieldValue = 0f;
            }

            if (eveDroneReloadRemaining <= 0f)
            {
                return;
            }

            eveDroneReloadRemaining = Mathf.Max(0f, eveDroneReloadRemaining - elapsed);
            if (Mathf.Approximately(eveDroneReloadRemaining, 0f))
            {
                eveDroneChargesRemaining = GetEveDroneMagazine();
                statusLabel = "Drone Beacon magazine reloaded.";
            }
        }

        private bool TryTriggerEveAutomaticSkills()
        {
            if (!IsSelectedEveMonster())
            {
                return false;
            }

            var castAny = false;
            castAny |= TryCastEvePrismRay();
            castAny |= TryCastEveFrostField();
            castAny |= TryCastEveStaticOverride();
            castAny |= TryCastEveDroneBeacon();

            if (!castAny)
            {
                statusLabel = $"{selectedMonsterName}: no automatic support skill is ready.";
            }

            return true;
        }

        private void FireManualEveArcBolt(Vector3 baseDirection)
        {
            if (!IsSelectedEveMonster() || !HasLearnedActive(SkillSlot.A) || eveAnchor == null || projectileRoot == null)
            {
                return;
            }

            var extraProjectiles = 0;
            var pierce = 0;
            var damageMultiplier = 1f;
            var shotIntervalMultiplier = 1f;
            var reloadMultiplier = 1f;
            var statusChance = statusChanceConfigured;
            var branchChance = 0f;
            var branchRadius = 0f;
            var branchDamageMultiplier = 1f;
            var branchTargetCount = 0;

            if (HasChoice("eve-a-trait-1"))
            {
                damageMultiplier *= 1.20f;
            }

            if (HasChoice("eve-a-trait-2"))
            {
                reloadMultiplier *= SpeedBonusToIntervalMultiplier(0.30f);
                pierce += 1;
            }

            if (HasChoice("eve-a-trait-3"))
            {
                extraProjectiles += 1;
                reloadMultiplier *= 1.20f;
            }

            if (HasChoice("eve-a-trait-4"))
            {
                extraProjectiles += 2;
                shotIntervalMultiplier *= SpeedBonusToIntervalMultiplier(-0.25f);
            }

            if (HasChoice("eve-a-trait-5"))
            {
                damageMultiplier *= 1.25f;
                branchChance += 0.35f;
                branchRadius = EveArcBranchRadius;
                branchTargetCount = Mathf.Max(branchTargetCount, 1);
            }

            if (HasChoice("eve-a-master-1"))
            {
                damageMultiplier *= 1.35f;
                branchChance = 1f;
                branchRadius = EveArcBranchRadius;
                branchDamageMultiplier = 0.60f;
                branchTargetCount = Mathf.Max(branchTargetCount, 2);
            }

            if (HasChoice("eve-a-master-2"))
            {
                damageMultiplier *= 1.45f;
                extraProjectiles += 2;
                pierce += 2;
                shotIntervalMultiplier *= SpeedBonusToIntervalMultiplier(-0.20f);
                statusChance = Mathf.Max(statusChance, 1f);
            }

            var projectileCount = 1 + extraProjectiles;
            for (var i = 0; i < projectileCount; i++)
            {
                var angleOffset = projectileCount <= 1 ? 0f : (i - (projectileCount - 1) * 0.5f) * EveArcExtraProjectileAngleStep;
                FireEveProjectile(
                    baseDirection,
                    pierce,
                    damageMultiplier,
                    angleOffset,
                    statusChance,
                    branchChance,
                    branchRadius,
                    branchDamageMultiplier,
                    branchTargetCount);
            }

            currentShotsRemaining -= 1;
            shotCooldown = Mathf.Max(0.05f, shotIntervalConfigured * shotIntervalMultiplier);
            var effectiveReloadDuration = Mathf.Max(0.25f, reloadDurationConfigured * reloadMultiplier);
            if (currentShotsRemaining <= 0)
            {
                currentShotsRemaining = 0;
                reloadRemaining = effectiveReloadDuration;
            }

            statusLabel = $"Arc Bolt manual fire: {projectileCount} shot(s) toward ({currentAttackPoint.x:0.0}, {currentAttackPoint.y:0.0}).";
        }

        private void FireEveProjectile(
            Vector3 direction,
            int pierce,
            float damageMultiplier,
            float angleOffset,
            float statusChance,
            float branchChance,
            float branchRadius,
            float branchDamageMultiplier,
            int branchTargetCount)
        {
            direction.z = 0f;
            if (direction.sqrMagnitude < 0.01f)
            {
                direction = Vector3.right;
            }

            direction.Normalize();
            if (Mathf.Abs(angleOffset) > 0.01f)
            {
                direction = Quaternion.Euler(0f, 0f, angleOffset) * direction;
            }

            nextProjectileSequence += 1;
            var projectileObject = new GameObject($"ArcBolt_{nextProjectileSequence:00}");
            projectileObject.transform.SetParent(projectileRoot, false);
            projectileObject.transform.position = eveAnchor.position + (direction * 0.2f);
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
                Speed = projectileSpeedConfigured,
                RemainingLifetime = projectileLifetimeConfigured,
                HitRadius = projectileHitRadiusConfigured,
                BaseDamage = (baseDamageConfigured + (powerStatConfigured * powerCoefficientConfigured)) * damageMultiplier,
                Attribute = DamageAttribute.Lightning,
                SkillId = "eve-a",
                RemainingPierce = Mathf.Max(0, pierce),
                StatusStacks = 1,
                StatusChance = Mathf.Clamp01(statusChance),
                BranchChance = Mathf.Clamp01(branchChance),
                BranchRadius = Mathf.Max(0f, branchRadius),
                BranchDamageMultiplier = Mathf.Max(0f, branchDamageMultiplier),
                BranchTargetCount = Mathf.Max(0, branchTargetCount)
            });
        }

        private bool TryCastEvePrismRay()
        {
            var skill = FindSelectedSkill(SkillSlot.B);
            if (skill == null || !HasLearnedActive(SkillSlot.B) || eveBeamCooldownRemaining > 0f)
            {
                return false;
            }

            var target = FindNearestEnemy(eveAnchor.position, float.PositiveInfinity);
            if (target == null)
            {
                return false;
            }

            return StartEvePrismRay(target, true, "Prism Ray auto-targeted");
        }

        private bool StartEvePrismRay(EnemyRuntime target, bool startsActiveCooldown, string statusPrefix)
        {
            var skill = FindSelectedSkill(SkillSlot.B);
            if (skill == null || target == null)
            {
                return false;
            }

            var damageMultiplier = 1f;
            var width = 3.2f;
            var duration = EveBeamDuration;
            var tickInterval = EveBeamTickInterval;
            var cooldown = Mathf.Max(0.1f, skill.CooldownSeconds);

            if (HasChoice("eve-b-trait-1"))
            {
                damageMultiplier *= 1.25f;
                tickInterval *= SpeedBonusToIntervalMultiplier(0.25f);
            }

            if (HasChoice("eve-b-trait-2"))
            {
                damageMultiplier *= 1.30f;
                width *= 1.30f;
            }

            if (HasChoice("eve-b-trait-3"))
            {
                cooldown *= SpeedBonusToIntervalMultiplier(0.35f);
                duration *= 1.15f;
            }

            if (HasChoice("eve-b-trait-4"))
            {
                damageMultiplier *= 2.0f;
                duration *= 0.5f;
            }

            if (HasChoice("eve-b-trait-5"))
            {
                cooldown *= SpeedBonusToIntervalMultiplier(0.30f);
                tickInterval *= SpeedBonusToIntervalMultiplier(0.20f);
            }

            var origin = eveAnchor.position + Vector3.right * 0.65f;
            var direction = target.Transform.position - origin;
            direction.z = 0f;
            if (direction.sqrMagnitude < 0.01f)
            {
                direction = Vector3.right;
            }

            direction.Normalize();
            var length = fieldSize.x + 3f;
            var effect = CreateLineEffect("PrismRay", origin, direction, length, width, duration);
            effect.SkillId = "eve-b";
            effect.BaseDamage = (skill.BaseDamage + powerStatConfigured * skill.SpellPowerCoefficient) * damageMultiplier;
            effect.Attribute = DamageAttribute.Lightning;
            effect.TickInterval = Mathf.Max(0.05f, tickInterval);
            effect.TickRemaining = 0f;
            effect.SlowChance = 0.20f;
            effect.SlowDuration = 2f;
            skillEffects.Add(effect);

            if (startsActiveCooldown)
            {
                eveBeamCooldownRemaining = cooldown;
            }

            statusLabel = $"{statusPrefix} {target.DisplayName}.";
            return true;
        }

        private bool TryCastEveFrostField()
        {
            var skill = FindSelectedSkill(SkillSlot.C);
            if (skill == null || !HasLearnedActive(SkillSlot.C) || eveFrostCooldownRemaining > 0f)
            {
                return false;
            }

            var target = FindNearestEnemy(eveAnchor.position, float.PositiveInfinity);
            if (target == null)
            {
                return false;
            }

            var radius = Mathf.Max(0.1f, skill.Radius);
            var duration = EveFrostFieldDuration;
            var tickInterval = EveFrostFieldTickInterval;
            var damageMultiplier = 1f;
            var cooldown = Mathf.Max(0.1f, skill.CooldownSeconds);
            var chillStacks = 1;

            if (HasChoice("eve-c-trait-1"))
            {
                radius *= 1.25f;
                duration *= 1.15f;
            }

            if (HasChoice("eve-c-trait-2"))
            {
                tickInterval *= SpeedBonusToIntervalMultiplier(0.25f);
                chillStacks += 1;
            }

            if (HasChoice("eve-c-trait-3"))
            {
                damageMultiplier *= 1.30f;
                cooldown *= 0.85f;
            }

            if (HasChoice("eve-c-trait-4"))
            {
                radius *= 0.80f;
                damageMultiplier *= 1.80f;
            }

            if (HasChoice("eve-c-trait-5"))
            {
                damageMultiplier *= 1.20f;
            }

            var effect = CreateCircleEffect("FrostField", target.Transform.position, radius, duration);
            effect.SkillId = "eve-c";
            effect.BaseDamage = (skill.BaseDamage + powerStatConfigured * skill.SpellPowerCoefficient) * damageMultiplier;
            effect.Attribute = DamageAttribute.Ice;
            effect.TickInterval = Mathf.Max(0.05f, tickInterval);
            effect.TickRemaining = 0f;
            effect.Radius = radius;
            effect.StatusStacks = chillStacks;
            effect.FreezeDuration = HasChoice("eve-c-trait-5") ? 1.0f + GetEveFreezeDurationBonus() : 0f;
            skillEffects.Add(effect);

            eveFrostCooldownRemaining = cooldown;
            statusLabel = $"Frost Field auto-targeted {target.DisplayName}.";
            return true;
        }

        private bool TryCastEveStaticOverride()
        {
            var skill = FindSelectedSkill(SkillSlot.D);
            if (skill == null || !HasLearnedActive(SkillSlot.D) || eveStaticCooldownRemaining > 0f)
            {
                return false;
            }

            var range = 3.5f;
            var radius = 1.8f;
            var damageMultiplier = 1f;
            var stackBonus = 0.35f;
            var cooldown = 7f;

            if (HasChoice("eve-d-trait-1"))
            {
                range *= 1.25f;
                radius *= 1.15f;
            }

            if (HasEveOvercurrentCircuit() && HasChoice("eve-i-trait-3"))
            {
                radius *= 1.25f;
            }

            if (HasChoice("eve-d-trait-2"))
            {
                stackBonus += 0.15f;
            }

            if (HasChoice("eve-d-trait-3"))
            {
                cooldown *= 0.80f;
                damageMultiplier *= 1.15f;
            }

            var target = FindNearestEnemy(eveAnchor.position, float.PositiveInfinity, enemy => enemy.ShockTimer > 0f && enemy.ShockStacks > 0);
            if (target == null)
            {
                return false;
            }

            var baseDamage = (skill.BaseDamage + powerStatConfigured * skill.SpellPowerCoefficient) * damageMultiplier;
            var hitCount = 0;
            for (var i = 0; i < enemies.Count; i++)
            {
                var enemy = enemies[i];
                if (enemy == null || enemy.CurrentHealth <= 0f || Vector2.Distance(enemy.Transform.position, target.Transform.position) > radius)
                {
                    continue;
                }

                var finalMultiplier = 1f + target.ShockStacks * stackBonus;
                if (HasChoice("eve-d-trait-5") && target.ShockStacks >= 3)
                {
                    finalMultiplier *= 1.50f;
                }

                ApplyEveSkillDamage(enemy, baseDamage, DamageAttribute.Lightning, finalMultiplier, "eve-d");
                if (HasChoice("eve-d-trait-4"))
                {
                    ApplyShock(enemy, 1, 1.25f);
                }

                hitCount += 1;
            }

            var effect = CreateCircleEffect("StaticOverride", target.Transform.position, radius, 0.35f);
            effect.SkillId = "eve-d";
            skillEffects.Add(effect);

            eveStaticCooldownRemaining = cooldown;
            statusLabel = $"Static Override detonated around shocked target {target.DisplayName}; hit {hitCount}.";
            return true;
        }

        private bool TryCastEveDroneBeacon()
        {
            var skill = FindSelectedSkill(SkillSlot.E);
            if (skill == null || !HasLearnedActive(SkillSlot.E) || eveAnchor == null || projectileRoot == null)
            {
                return false;
            }

            if (eveDroneReloadRemaining > 0f)
            {
                return false;
            }

            if (eveDroneChargesRemaining <= 0)
            {
                eveDroneReloadRemaining = GetEveDroneReloadSeconds();
                return false;
            }

            var maxBeacons = HasChoice("eve-e-trait-4") ? 2 : 1;
            if (drones.Count >= maxBeacons)
            {
                return false;
            }

            var duration = EveDroneDuration * (HasChoice("eve-e-trait-1") ? 1.20f : 1f);
            var period = EveDroneAttackPeriod * (HasChoice("eve-e-trait-2") ? SpeedBonusToIntervalMultiplier(0.25f) : 1f);
            var damageMultiplier = HasChoice("eve-e-trait-3") ? 1.30f : 1f;
            var vulnerableStacks = HasChoice("eve-e-trait-2") ? 2 : 1;

            var droneObject = new GameObject($"DroneBeacon_{drones.Count + 1:00}");
            droneObject.transform.SetParent(projectileRoot, false);
            droneObject.transform.position = eveAnchor.position + new Vector3(-0.85f - (drones.Count * 0.35f), 0.15f, 0f);
            droneObject.transform.localScale = new Vector3(0.55f, 0.55f, 1f);

            var renderer = droneObject.AddComponent<SpriteRenderer>();
            renderer.sprite = skill.SkillIcon != null ? skill.SkillIcon : GetSharedSprite();
            renderer.color = Color.white;
            renderer.sortingOrder = 23;

            drones.Add(new DroneRuntime
            {
                GameObject = droneObject,
                Transform = droneObject.transform,
                Renderer = renderer,
                RemainingDuration = duration,
                AttackRemaining = 0f,
                AttackPeriod = Mathf.Max(0.05f, period),
                Range = float.PositiveInfinity,
                BaseDamage = (skill.BaseDamage + powerStatConfigured * skill.SpellPowerCoefficient) * damageMultiplier,
                Attribute = DamageAttribute.Ice,
                VulnerableStacks = vulnerableStacks,
                SkillId = "eve-e"
            });

            eveDroneChargesRemaining -= 1;
            if (eveDroneChargesRemaining <= 0)
            {
                eveDroneReloadRemaining = GetEveDroneReloadSeconds();
            }

            statusLabel = "Drone Beacon deployed behind Eve.";
            return true;
        }

        private void UpdateEveSkillEffects()
        {
            UpdatePersistentSkillEffects();
            UpdateDrones();
        }

        private void UpdatePersistentSkillEffects()
        {
            for (var i = skillEffects.Count - 1; i >= 0; i--)
            {
                var effect = skillEffects[i];
                if (effect == null || effect.GameObject == null)
                {
                    skillEffects.RemoveAt(i);
                    continue;
                }

                effect.RemainingDuration = Mathf.Max(0f, effect.RemainingDuration - Time.deltaTime);
                effect.TickRemaining -= Time.deltaTime;
                if (effect.TickInterval > 0f && effect.TickRemaining <= 0f && effect.BaseDamage > 0f)
                {
                    effect.HitThisTick.Clear();
                    TickSkillEffect(effect);
                    effect.TickRemaining = effect.TickInterval;
                }

                if (effect.RemainingDuration > 0f)
                {
                    continue;
                }

                TryHandleSkillEffectExpired(effect);
                Destroy(effect.GameObject);
                skillEffects.RemoveAt(i);
            }
        }

        private void TickSkillEffect(SkillEffectRuntime effect)
        {
            for (var i = 0; i < enemies.Count; i++)
            {
                var enemy = enemies[i];
                if (enemy == null || enemy.CurrentHealth <= 0f)
                {
                    continue;
                }

                var inside = effect.SkillId == "eve-b" || IsVegaLineSkillEffect(effect)
                    ? IsPointInsideBeam(enemy.Transform.position, effect)
                    : Vector2.Distance(enemy.Transform.position, effect.Transform.position) <= effect.Radius + GetEnemyHitRadius(enemy);
                if (!inside)
                {
                    continue;
                }

                if (IsSeinSkillEffect(effect))
                {
                    ApplySeinSkillEffectDamage(effect, enemy);
                    continue;
                }

                if (IsVegaSkillEffect(effect))
                {
                    ApplyVegaSkillEffectDamage(effect, enemy);
                    continue;
                }

                ApplyEveSkillDamage(enemy, effect.BaseDamage, effect.Attribute, 1f, effect.SkillId);
                if (effect.SkillId == "eve-b" && effect.SlowChance > 0f && UnityEngine.Random.value < effect.SlowChance)
                {
                    enemy.SlowMultiplier = 0.65f;
                    enemy.SlowTimer = Mathf.Max(enemy.SlowTimer, effect.SlowDuration);
                }
                else if (effect.SkillId == "eve-c")
                {
                    ApplyChill(enemy, Mathf.Max(1, effect.StatusStacks), 2.5f);
                    if (effect.FreezeDuration > 0f)
                    {
                        enemy.FreezeTimer = Mathf.Max(enemy.FreezeTimer, effect.FreezeDuration);
                    }
                }
            }
        }

        private void UpdateDrones()
        {
            for (var i = drones.Count - 1; i >= 0; i--)
            {
                var drone = drones[i];
                if (drone == null || drone.GameObject == null)
                {
                    drones.RemoveAt(i);
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

                Destroy(drone.GameObject);
                drones.RemoveAt(i);
            }
        }

        private void FireDroneProjectile(DroneRuntime drone)
        {
            var target = FindNearestEnemy(drone.Transform.position, drone.Range);
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
            nextProjectileSequence += 1;
            var projectileObject = new GameObject($"DroneShot_{nextProjectileSequence:00}");
            projectileObject.transform.SetParent(projectileRoot, false);
            projectileObject.transform.position = drone.Transform.position;
            projectileObject.transform.localScale = new Vector3(0.24f, 0.24f, 1f);

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
                Speed = 12f,
                RemainingLifetime = 2f,
                HitRadius = 0.28f,
                BaseDamage = drone.BaseDamage,
                Attribute = drone.Attribute,
                SkillId = "eve-e",
                StatusStacks = drone.VulnerableStacks
            });
        }

        private SkillEffectRuntime CreateLineEffect(string name, Vector3 origin, Vector3 direction, float length, float width, float duration)
        {
            var effectObject = new GameObject(name);
            effectObject.transform.SetParent(projectileRoot, false);
            effectObject.transform.position = origin + direction * (length * 0.5f);
            effectObject.transform.right = direction;
            effectObject.transform.localScale = new Vector3(length, width, 1f);

            var renderer = effectObject.AddComponent<SpriteRenderer>();
            renderer.sprite = GetSharedSprite();
            renderer.color = Color.white;
            renderer.sortingOrder = 22;

            return new SkillEffectRuntime
            {
                GameObject = effectObject,
                Transform = effectObject.transform,
                Renderer = renderer,
                Origin = origin,
                Direction = direction,
                Length = length,
                Width = width,
                RemainingDuration = duration
            };
        }

        private void CreateEveArcBranchLine(Vector3 start, Vector3 end)
        {
            if (projectileRoot == null)
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
            var effect = CreateLineEffect("ArcBranchLightning", start, direction, length, EveArcBranchLineWidth, EveArcBranchLineDuration);
            effect.SkillId = "eve-a-branch-visual";
            if (effect.Renderer != null)
            {
                effect.Renderer.color = new Color(0.80f, 0.96f, 1f, 0.95f);
                effect.Renderer.sortingOrder = 26;
            }

            skillEffects.Add(effect);
        }

        private SkillEffectRuntime CreateCircleEffect(string name, Vector3 position, float radius, float duration)
        {
            var effectObject = new GameObject(name);
            effectObject.transform.SetParent(projectileRoot, false);
            effectObject.transform.position = position;
            effectObject.transform.localScale = new Vector3(radius * 2f, radius * 2f, 1f);

            var renderer = effectObject.AddComponent<SpriteRenderer>();
            renderer.sprite = GetCircleSprite();
            renderer.color = Color.white;
            renderer.sortingOrder = 21;

            return new SkillEffectRuntime
            {
                GameObject = effectObject,
                Transform = effectObject.transform,
                Renderer = renderer,
                Radius = radius,
                RemainingDuration = duration
            };
        }

        private bool IsPointInsideBeam(Vector3 point, SkillEffectRuntime effect)
        {
            var fromOrigin = point - effect.Origin;
            var along = Vector3.Dot(fromOrigin, effect.Direction);
            if (along < 0f || along > effect.Length)
            {
                return false;
            }

            var closest = effect.Origin + effect.Direction * along;
            return Vector2.Distance(point, closest) <= (effect.Width * 0.5f);
        }

        private void ApplyEveSkillDamage(EnemyRuntime enemy, float baseDamage, DamageAttribute attribute, float finalMultiplier, string skillId = "")
        {
            var vulnerableMultiplier = 1f + Mathf.Clamp(enemy.VulnerableStacks, 0, 10) * 0.03f;
            if (attribute == DamageAttribute.Ice && HasChoice("eve-e-trait-5") && enemy.VulnerableStacks >= 5)
            {
                vulnerableMultiplier *= 1.40f;
            }

            var passiveMultiplier = GetEveFinalDamageMultiplier(enemy, attribute, skillId);

            var result = DamageCalculator.Resolve(
                baseDamage,
                attribute,
                enemy.Defenses,
                flatDefenseReduction: GetEveFlatDefenseReduction(enemy, attribute),
                targetCriticalResistance: enemy.CriticalResistance,
                criticalDamageTakenBonus: GetEveCriticalDamageTakenBonus(enemy, skillId),
                finalDamageMultiplier: enemy.DamageTakenMultiplier * finalMultiplier * vulnerableMultiplier * passiveMultiplier);
            var applied = ApplyDamageToEnemy(enemy, result.FinalDamage, attribute);
            enemy.FlashTimer = 0.08f;
            TryTriggerEveParticleSeparationProc(enemy, attribute, skillId);
            Debug.Log($"[CombatDamage] Eve.{attribute} skill -> {enemy.DisplayName}: {result.FormulaLog}; Applied={applied:0.##}, ShieldLeft={enemy.ShieldValue:0.##}, HpLeft={Mathf.Max(0f, enemy.CurrentHealth):0.##}");
        }

        private void ApplyShock(EnemyRuntime enemy, int stacks, float duration)
        {
            if (enemy == null)
            {
                return;
            }

            enemy.ShockStacks = Mathf.Clamp(enemy.ShockStacks + Mathf.Max(1, stacks), 0, 10);
            enemy.ShockTimer = Mathf.Max(enemy.ShockTimer, duration);
        }

        private void ApplyChill(EnemyRuntime enemy, int stacks, float duration)
        {
            if (enemy == null)
            {
                return;
            }

            enemy.ChillStacks = Mathf.Clamp(enemy.ChillStacks + Mathf.Max(1, stacks), 0, 10);
            enemy.ChillTimer = Mathf.Max(enemy.ChillTimer, duration);
        }

        private void ApplyVulnerable(EnemyRuntime enemy, int stacks)
        {
            if (enemy == null)
            {
                return;
            }

            enemy.VulnerableStacks = Mathf.Clamp(enemy.VulnerableStacks + Mathf.Max(1, stacks), 0, 10);
        }

        private EnemyRuntime FindNearestEnemy(Vector3 origin, float range, Func<EnemyRuntime, bool> predicate = null)
        {
            EnemyRuntime best = null;
            var bestDistance = float.MaxValue;
            for (var i = 0; i < enemies.Count; i++)
            {
                var enemy = enemies[i];
                if (enemy == null || enemy.CurrentHealth <= 0f || enemy.Transform == null)
                {
                    continue;
                }

                if (predicate != null && !predicate(enemy))
                {
                    continue;
                }

                var distance = Vector2.Distance(origin, enemy.Transform.position);
                if (distance > range || distance >= bestDistance)
                {
                    continue;
                }

                best = enemy;
                bestDistance = distance;
            }

            return best;
        }

        private bool HasLearnedActive(SkillSlot slot)
        {
            var skill = FindSelectedSkill(slot);
            if (skill == null)
            {
                return false;
            }

            return learnedActiveSkillIds.Contains(skill.SkillId);
        }

        private SkillDefinition FindSelectedSkill(SkillSlot slot)
        {
            if (selectedMonster == null)
            {
                return null;
            }

            var activeSkills = PakuriDataManager.Instance.GetActiveSkills(selectedMonster.MonsterId, selectedMonster);
            for (var i = 0; i < activeSkills.Length; i++)
            {
                var skill = activeSkills[i];
                if (skill != null && skill.Slot == slot)
                {
                    return skill;
                }
            }

            return null;
        }

        private bool HasChoice(string choiceId)
        {
            return !string.IsNullOrWhiteSpace(choiceId) && chosenSkillChoiceIds.Contains(choiceId);
        }

        private bool HasEvePassive(string passiveId, string passiveName)
        {
            return IsSelectedEveMonster()
                && ((string.IsNullOrWhiteSpace(passiveId) == false && chosenSkillChoiceIds.Contains(passiveId))
                    || (string.IsNullOrWhiteSpace(passiveId) == false && learnedPassiveSkillIds.Contains(passiveId)));
        }

        private bool HasEveVoltageCalibration()
        {
            return HasEvePassive("eve-f", "전압 보정");
        }

        private bool HasEveParticleSeparation()
        {
            return HasEvePassive("eve-g", "입자 분리");
        }

        private bool HasEveCoolingAlgorithm()
        {
            return HasEvePassive("eve-h", "냉각 알고리즘");
        }

        private bool HasEveOvercurrentCircuit()
        {
            return HasEvePassive("eve-i", "과전류 회로");
        }

        private bool HasEveWeaknessAnalysis()
        {
            return HasEvePassive("eve-j", "약점 분석");
        }

        private bool IsSelectedEveMonster()
        {
            return selectedMonster != null &&
                string.Equals(selectedMonster.MonsterId, "eve", StringComparison.OrdinalIgnoreCase);
        }

        private int GetEveDroneMagazine()
        {
            return EveDroneMagazineCapacity + (HasChoice("eve-e-trait-1") ? 1 : 0);
        }

        private int GetEveArcMagazineCapacity()
        {
            var bonus = 0;
            if (HasChoice("eve-a-trait-1"))
            {
                bonus += 4;
            }

            if (HasChoice("eve-a-master-1"))
            {
                bonus += 2;
            }

            return magazineCapacityConfigured + bonus;
        }

        private float GetEveDroneReloadSeconds()
        {
            return EveDroneReloadSeconds * (HasChoice("eve-e-trait-4") ? SpeedBonusToIntervalMultiplier(0.30f) : 1f);
        }

        private float GetEveActionSpeedMultiplier()
        {
            return HasEveVoltageCalibration() && HasChoice("eve-f-trait-3") && unitShieldValue > 0f
                ? 1.12f
                : 1f;
        }

        private float GetEveFreezeDurationBonus()
        {
            return HasEveCoolingAlgorithm() && HasChoice("eve-h-trait-2") ? 1.0f : 0f;
        }

        private static float SpeedBonusToIntervalMultiplier(float speedBonus)
        {
            return 1f / Mathf.Max(0.05f, 1f + speedBonus);
        }

        private void ApplyEveVoltageCalibrationShield()
        {
            unitShieldValue = 0f;
            unitShieldTimer = 0f;
            if (!HasEveVoltageCalibration())
            {
                return;
            }

            var shield = powerStatConfigured * 1.20f;
            if (HasChoice("eve-f-trait-1"))
            {
                shield *= 1.40f;
            }

            unitShieldValue = Mathf.Max(0f, shield);
            unitShieldTimer = EveVoltageShieldDuration;
        }

        private float GetEveFinalDamageMultiplier(EnemyRuntime enemy, DamageAttribute attribute, string skillId)
        {
            if (!IsSelectedEveMonster() || enemy == null)
            {
                return 1f;
            }

            var bonus = 0f;
            var shocked = enemy.ShockTimer > 0f && enemy.ShockStacks > 0;
            var chilledOrFrozen = enemy.ChillTimer > 0f || enemy.FreezeTimer > 0f;
            var vulnerable = enemy.VulnerableStacks > 0;

            if (HasEveVoltageCalibration() && shocked)
            {
                bonus += 0.10f + (HasChoice("eve-f-trait-2") ? 0.06f : 0f);
            }

            if (HasEveParticleSeparation() && (attribute == DamageAttribute.Lightning || attribute == DamageAttribute.Ice))
            {
                bonus += 0.08f + (HasChoice("eve-g-trait-2") ? 0.05f : 0f);
            }

            if (HasEveParticleSeparation()
                && HasChoice("eve-g-trait-3")
                && string.Equals(skillId, "eve-b", StringComparison.OrdinalIgnoreCase)
                && enemy.ShieldValue > 0f)
            {
                bonus += 3.0f;
            }

            if (HasEveCoolingAlgorithm() && chilledOrFrozen)
            {
                bonus += 0.14f + (HasChoice("eve-h-trait-1") ? 0.06f : 0f);
            }

            if (HasEveOvercurrentCircuit() && shocked && attribute == DamageAttribute.Lightning)
            {
                bonus += 0.18f + (HasChoice("eve-i-trait-1") ? 0.08f : 0f);
            }

            if (HasEveWeaknessAnalysis() && vulnerable)
            {
                bonus += 0.12f + (HasChoice("eve-j-trait-1") ? 0.06f : 0f);
            }

            if (HasEveWeaknessAnalysis()
                && HasChoice("eve-j-trait-3")
                && string.Equals(skillId, "eve-e", StringComparison.OrdinalIgnoreCase)
                && enemy.VulnerableStacks >= 5)
            {
                bonus += 0.75f;
            }

            return 1f + bonus;
        }

        private float GetEveFlatDefenseReduction(EnemyRuntime enemy, DamageAttribute attribute)
        {
            if (!IsSelectedEveMonster() || enemy == null)
            {
                return 0f;
            }

            var reduction = 0f;
            if (HasEveOvercurrentCircuit()
                && attribute == DamageAttribute.Lightning
                && enemy.ShockTimer > 0f
                && enemy.ShockStacks >= 5)
            {
                reduction += 12f + (HasChoice("eve-i-trait-2") ? 6f : 0f);
            }

            if (HasEveWeaknessAnalysis() && enemy.VulnerableStacks > 0)
            {
                reduction += 8f + (HasChoice("eve-j-trait-2") ? 4f : 0f);
            }

            return reduction;
        }

        private float GetEveCriticalDamageTakenBonus(EnemyRuntime enemy, string skillId)
        {
            if (!HasEveWeaknessAnalysis()
                || !HasChoice("eve-e-master-2")
                || enemy == null
                || enemy.VulnerableStacks <= 0)
            {
                return 0f;
            }

            return Mathf.Clamp(enemy.VulnerableStacks, 0, 15) * 0.01f;
        }

        private float GetEveStatusChanceBonus(EnemyRuntime enemy)
        {
            return HasEveCoolingAlgorithm() && enemy != null && (enemy.ChillTimer > 0f || enemy.FreezeTimer > 0f)
                ? 0.10f
                : 0f;
        }

        private void TryTriggerEveParticleSeparationProc(EnemyRuntime sourceEnemy, DamageAttribute attribute, string sourceSkillId)
        {
            if (!HasEveParticleSeparation()
                || eveParticleSeparationCooldownRemaining > 0f
                || sourceEnemy == null
                || sourceEnemy.CurrentHealth <= 0f
                || string.Equals(sourceSkillId, "eve-b", StringComparison.OrdinalIgnoreCase)
                || (attribute != DamageAttribute.Lightning && attribute != DamageAttribute.Ice))
            {
                return;
            }

            var chance = 0.04f + (HasChoice("eve-g-trait-1") ? 0.03f : 0f);
            if (UnityEngine.Random.value >= chance)
            {
                return;
            }

            if (StartEvePrismRay(sourceEnemy, false, "Particle Separation triggered Prism Ray on"))
            {
                eveParticleSeparationCooldownRemaining = EveParticleSeparationCooldown;
            }
        }

        private void ClearEveSkillRuntimeObjects()
        {
            for (var i = skillEffects.Count - 1; i >= 0; i--)
            {
                if (skillEffects[i] != null && skillEffects[i].GameObject != null)
                {
                    Destroy(skillEffects[i].GameObject);
                }
            }

            skillEffects.Clear();

            for (var i = drones.Count - 1; i >= 0; i--)
            {
                if (drones[i] != null && drones[i].GameObject != null)
                {
                    Destroy(drones[i].GameObject);
                }
            }

            drones.Clear();
        }

        private static Sprite GetCircleSprite()
        {
            if (sharedCircleSprite != null)
            {
                return sharedCircleSprite;
            }

            const int size = 64;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.HideAndDontSave
            };

            var center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
            var radius = size * 0.48f;
            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    var distance = Vector2.Distance(new Vector2(x, y), center);
                    texture.SetPixel(x, y, distance <= radius ? Color.white : Color.clear);
                }
            }

            texture.Apply();
            sharedCircleSprite = Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), size);
            sharedCircleSprite.hideFlags = HideFlags.HideAndDontSave;
            return sharedCircleSprite;
        }
    }
}
