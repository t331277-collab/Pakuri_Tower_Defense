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

        private float eveBeamCooldownRemaining;
        private float eveFrostCooldownRemaining;
        private float eveStaticCooldownRemaining;
        private float eveDroneReloadRemaining;
        private int eveDroneChargesRemaining = EveDroneMagazineCapacity;

        private void ConfigureEveSkillSelectionState(RunSession session)
        {
            learnedActiveSkillNames.Clear();
            chosenSkillChoiceIds.Clear();

            if (session != null && session.LearnedActives != null)
            {
                for (var i = 0; i < session.LearnedActives.Count; i++)
                {
                    if (!string.IsNullOrWhiteSpace(session.LearnedActives[i]))
                    {
                        learnedActiveSkillNames.Add(session.LearnedActives[i]);
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

            var arcBolt = FindSelectedSkill(SkillSlot.A);
            if (arcBolt != null && !string.IsNullOrWhiteSpace(arcBolt.DisplayName))
            {
                learnedActiveSkillNames.Add(arcBolt.DisplayName);
            }
            else if (!string.IsNullOrWhiteSpace(selectedActiveSkillName))
            {
                learnedActiveSkillNames.Add(selectedActiveSkillName);
            }
        }

        private void ResetEveSkillCombatTimers()
        {
            eveBeamCooldownRemaining = 0f;
            eveFrostCooldownRemaining = 0f;
            eveStaticCooldownRemaining = 0f;
            eveDroneReloadRemaining = 0f;
            eveDroneChargesRemaining = GetEveDroneMagazine();
        }

        private void UpdateEveSkillCooldowns()
        {
            eveBeamCooldownRemaining = Mathf.Max(0f, eveBeamCooldownRemaining - Time.deltaTime);
            eveFrostCooldownRemaining = Mathf.Max(0f, eveFrostCooldownRemaining - Time.deltaTime);
            eveStaticCooldownRemaining = Mathf.Max(0f, eveStaticCooldownRemaining - Time.deltaTime);

            if (eveDroneReloadRemaining <= 0f)
            {
                return;
            }

            eveDroneReloadRemaining = Mathf.Max(0f, eveDroneReloadRemaining - Time.deltaTime);
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

            if (HasChoice("eve-a-trait-1"))
            {
                damageMultiplier *= 1.20f;
            }

            if (HasChoice("eve-a-trait-2"))
            {
                reloadMultiplier *= 0.70f;
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
                shotIntervalMultiplier *= 1.25f;
            }

            if (HasChoice("eve-a-trait-5"))
            {
                damageMultiplier *= 1.25f;
                statusChance += 0.35f;
            }

            var projectileCount = 1 + extraProjectiles;
            for (var i = 0; i < projectileCount; i++)
            {
                var angleOffset = projectileCount <= 1 ? 0f : (i - (projectileCount - 1) * 0.5f) * 4f;
                FireEveProjectile(baseDirection, pierce, damageMultiplier, angleOffset, statusChance);
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

        private void FireEveProjectile(Vector3 direction, int pierce, float damageMultiplier, float angleOffset, float statusChance)
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
                StatusChance = Mathf.Clamp01(statusChance)
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

            var damageMultiplier = 1f;
            var width = 3.2f;
            var duration = EveBeamDuration;
            var tickInterval = EveBeamTickInterval;
            var cooldown = Mathf.Max(0.1f, skill.CooldownSeconds);

            if (HasChoice("eve-b-trait-1"))
            {
                damageMultiplier *= 1.25f;
                tickInterval *= 0.75f;
            }

            if (HasChoice("eve-b-trait-2"))
            {
                damageMultiplier *= 1.30f;
                width *= 1.30f;
            }

            if (HasChoice("eve-b-trait-3"))
            {
                cooldown *= 0.65f;
                duration *= 1.15f;
            }

            if (HasChoice("eve-b-trait-4"))
            {
                damageMultiplier *= 2.0f;
                duration *= 0.5f;
            }

            if (HasChoice("eve-b-trait-5"))
            {
                cooldown *= 0.70f;
                tickInterval *= 0.80f;
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

            eveBeamCooldownRemaining = cooldown;
            statusLabel = $"Prism Ray auto-targeted {target.DisplayName}.";
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
                tickInterval *= 0.75f;
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
            effect.FreezeDuration = HasChoice("eve-c-trait-5") ? 1.0f : 0f;
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

                ApplyEveSkillDamage(enemy, baseDamage, DamageAttribute.Lightning, finalMultiplier);
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
            var period = EveDroneAttackPeriod * (HasChoice("eve-e-trait-2") ? 0.75f : 1f);
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
                VulnerableStacks = vulnerableStacks
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

                var inside = effect.SkillId == "eve-b"
                    ? IsPointInsideBeam(enemy.Transform.position, effect)
                    : Vector2.Distance(enemy.Transform.position, effect.Transform.position) <= effect.Radius + GetEnemyHitRadius(enemy);
                if (!inside)
                {
                    continue;
                }

                ApplyEveSkillDamage(enemy, effect.BaseDamage, effect.Attribute, 1f);
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

        private void ApplyEveSkillDamage(EnemyRuntime enemy, float baseDamage, DamageAttribute attribute, float finalMultiplier)
        {
            var vulnerableMultiplier = 1f + Mathf.Clamp(enemy.VulnerableStacks, 0, 10) * 0.03f;
            if (attribute == DamageAttribute.Ice && HasChoice("eve-e-trait-5") && enemy.VulnerableStacks >= 5)
            {
                vulnerableMultiplier *= 1.40f;
            }

            var result = DamageCalculator.Resolve(
                baseDamage,
                attribute,
                enemy.Defenses,
                targetCriticalResistance: enemy.CriticalResistance,
                finalDamageMultiplier: enemy.DamageTakenMultiplier * finalMultiplier * vulnerableMultiplier);
            var applied = ApplyDamageToEnemy(enemy, result.FinalDamage);
            enemy.FlashTimer = 0.08f;
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

            return learnedActiveSkillNames.Contains(skill.DisplayName);
        }

        private SkillDefinition FindSelectedSkill(SkillSlot slot)
        {
            if (selectedMonster == null || selectedMonster.ActiveSkills == null)
            {
                return null;
            }

            for (var i = 0; i < selectedMonster.ActiveSkills.Length; i++)
            {
                var skill = selectedMonster.ActiveSkills[i];
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
            return magazineCapacityConfigured + (HasChoice("eve-a-trait-1") ? 4 : 0);
        }

        private float GetEveDroneReloadSeconds()
        {
            return EveDroneReloadSeconds * (HasChoice("eve-e-trait-4") ? 0.70f : 1f);
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
