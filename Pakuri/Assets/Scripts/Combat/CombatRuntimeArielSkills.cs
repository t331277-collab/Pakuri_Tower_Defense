using System;
using Pakuri.Data;
using UnityEngine;

namespace Pakuri.Combat
{
    public partial class CombatRuntimeController
    {
        private const float ArielRadiantShieldDuration = 5f;
        private const float ArielBlessingDuration = 4f;
        private const float ArielArchangelShieldDuration = 6f;
        private const float ArielSanctuaryDuration = 5f;
        private const float ArielJudgementExplosionRadius = 2.2f;
        private const float ArielJudgementProjectileSpeed = 17f;

        private float arielRadiantShieldCooldownRemaining;
        private float arielBlessingCooldownRemaining;
        private float arielBrandCooldownRemaining;
        private float arielArchangelCooldownRemaining;
        private float arielBlessingTimer;
        private float arielSanctuaryTimer;
        private float arielSanctuaryProclamationTimer;
        private float arielArchangelShieldValue;
        private float arielArchangelShieldTimer;
        private float arielRadiantShieldBurstDamage;
        private float arielRadiantShieldReflectMultiplier;

        private void ResetArielSkillCombatTimers()
        {
            arielRadiantShieldCooldownRemaining = 0f;
            arielBlessingCooldownRemaining = 0f;
            arielBrandCooldownRemaining = 0f;
            arielArchangelCooldownRemaining = 0f;
            arielBlessingTimer = 0f;
            arielSanctuaryTimer = 0f;
            arielSanctuaryProclamationTimer = 0f;
            arielArchangelShieldValue = 0f;
            arielArchangelShieldTimer = 0f;
            arielRadiantShieldBurstDamage = 0f;
            arielRadiantShieldReflectMultiplier = 0f;

            if (IsSelectedArielMonster() && HasArielGuardianDoctrine())
            {
                var shield = (25f + powerStatConfigured * 0.8f) * GetArielShieldMultiplier();
                if (HasChoice("ariel-g-trait-2"))
                {
                    shield *= 1.40f;
                }

            ApplyArielUnitShield(shield, ArielRadiantShieldDuration);
            }
        }

        private void UpdateSelectedMonsterSkillCooldowns()
        {
            if (IsSelectedEveMonster())
            {
                UpdateEveSkillCooldowns();
                return;
            }

            if (IsSelectedArielMonster())
            {
                UpdateArielSkillCooldowns();
                return;
            }

            if (IsSelectedRinMonster())
            {
                UpdateRinSkillCooldowns();
            }
        }

        private bool TryTriggerSelectedMonsterAutomaticSkills()
        {
            if (IsSelectedEveMonster())
            {
                return TryTriggerEveAutomaticSkills();
            }

            if (IsSelectedArielMonster())
            {
                return TryTriggerArielAutomaticSkills();
            }

            if (IsSelectedRinMonster())
            {
                return TryTriggerRinAutomaticSkills();
            }

            return false;
        }

        private int GetSelectedMonsterMagazineCapacity()
        {
            if (IsSelectedEveMonster())
            {
                return GetEveArcMagazineCapacity();
            }

            if (IsSelectedArielMonster())
            {
                return GetArielJudgementMagazineCapacity();
            }

            if (IsSelectedRinMonster())
            {
                return GetRinShatteringFistMagazineCapacity();
            }

            return magazineCapacityConfigured;
        }

        private float GetSelectedMonsterActionSpeedMultiplier()
        {
            if (IsSelectedEveMonster())
            {
                return GetEveActionSpeedMultiplier();
            }

            if (IsSelectedArielMonster())
            {
                return GetArielActionSpeedMultiplier();
            }

            if (IsSelectedRinMonster())
            {
                return GetRinActionSpeedMultiplier();
            }

            return 1f;
        }

        private void UpdateArielSkillCooldowns()
        {
            var elapsed = Time.deltaTime * GetArielCooldownChargeMultiplier();
            arielRadiantShieldCooldownRemaining = Mathf.Max(0f, arielRadiantShieldCooldownRemaining - elapsed);
            arielBlessingCooldownRemaining = Mathf.Max(0f, arielBlessingCooldownRemaining - elapsed);
            arielBrandCooldownRemaining = Mathf.Max(0f, arielBrandCooldownRemaining - elapsed);
            arielArchangelCooldownRemaining = Mathf.Max(0f, arielArchangelCooldownRemaining - elapsed);

            unitShieldTimer = Mathf.Max(0f, unitShieldTimer - Time.deltaTime);
            if (Mathf.Approximately(unitShieldTimer, 0f) && unitShieldValue > 0f)
            {
                unitShieldValue = 0f;
                TriggerArielRadiantShieldBurst();
            }
        }

        private void UpdateArielSkillEffects()
        {
            if (!IsSelectedArielMonster())
            {
                return;
            }

            arielBlessingTimer = Mathf.Max(0f, arielBlessingTimer - Time.deltaTime);
            arielSanctuaryTimer = Mathf.Max(0f, arielSanctuaryTimer - Time.deltaTime);
            arielSanctuaryProclamationTimer = Mathf.Max(0f, arielSanctuaryProclamationTimer - Time.deltaTime);
            arielArchangelShieldTimer = Mathf.Max(0f, arielArchangelShieldTimer - Time.deltaTime);
            if (Mathf.Approximately(arielArchangelShieldTimer, 0f))
            {
                arielArchangelShieldValue = 0f;
            }
        }

        private bool TryTriggerArielAutomaticSkills()
        {
            if (!IsSelectedArielMonster())
            {
                return false;
            }

            var castAny = false;
            castAny |= TryCastArielRadiantShield();
            castAny |= TryCastArielBlessingWave();
            castAny |= TryCastArielCelestialBrand();
            castAny |= TryCastArielArchangelDescent();

            if (!castAny)
            {
                statusLabel = $"{selectedMonsterName}: ready support skill not found.";
            }

            return true;
        }

        private void FireManualArielJudgementLight(Vector3 baseDirection)
        {
            var skill = FindSelectedSkill(SkillSlot.A);
            if (skill == null || !HasLearnedActive(SkillSlot.A) || eveAnchor == null || projectileRoot == null)
            {
                return;
            }

            var pierce = 1;
            var damageMultiplier = 1f;
            var reloadMultiplier = 1f;

            if (HasChoice("ariel-a-trait-1"))
            {
                damageMultiplier *= 1.25f;
            }

            if (HasChoice("ariel-a-trait-3"))
            {
                reloadMultiplier *= 0.80f;
            }

            if (HasChoice("ariel-a-trait-4"))
            {
                pierce += 1;
            }

            if (HasChoice("ariel-a-trait-5"))
            {
                damageMultiplier *= 1f + GetArielShieldedAllyCount() * 0.06f;
            }

            var isLastShot = currentShotsRemaining <= 1;
            FireArielProjectile(skill, baseDirection, pierce, damageMultiplier, isLastShot && HasChoice("ariel-a-master-1"));
            currentShotsRemaining -= 1;
            shotCooldown = Mathf.Max(0.05f, skill.ShotIntervalSeconds > 0f ? skill.ShotIntervalSeconds : shotIntervalConfigured);

            if (currentShotsRemaining <= 0)
            {
                currentShotsRemaining = 0;
                reloadRemaining = Mathf.Max(0.25f, GetArielReloadSeconds() * reloadMultiplier);
            }

            statusLabel = $"Judgement Light fired toward ({currentAttackPoint.x:0.0}, {currentAttackPoint.y:0.0}).";
        }

        private void FireArielProjectile(SkillDefinition skill, Vector3 direction, int pierce, float damageMultiplier, bool explodeOnDestroy)
        {
            direction.z = 0f;
            if (direction.sqrMagnitude < 0.01f)
            {
                direction = Vector3.right;
            }

            direction.Normalize();
            nextProjectileSequence += 1;
            var projectileObject = new GameObject($"JudgementLight_{nextProjectileSequence:00}");
            projectileObject.transform.SetParent(projectileRoot, false);
            projectileObject.transform.position = eveAnchor.position + direction * 0.2f;
            projectileObject.transform.localScale = new Vector3(projectileHitRadiusConfigured, projectileHitRadiusConfigured, 1f);

            var renderer = projectileObject.AddComponent<SpriteRenderer>();
            renderer.sprite = selectedProjectileSprite != null ? selectedProjectileSprite : GetSharedSprite();
            renderer.color = Color.white;
            renderer.sortingOrder = 25;

            var speed = ArielJudgementProjectileSpeed;
            var range = skill != null && skill.Range > 0f ? skill.Range : projectileSpeedConfigured * projectileLifetimeConfigured;
            var lifetime = Mathf.Max(projectileLifetimeConfigured, range / Mathf.Max(0.1f, speed));
            var baseDamage = GetArielSkillBaseDamage(skill) * damageMultiplier;

            projectiles.Add(new ProjectileRuntime
            {
                GameObject = projectileObject,
                Transform = projectileObject.transform,
                Renderer = renderer,
                Direction = direction,
                Speed = speed,
                RemainingLifetime = lifetime,
                HitRadius = projectileHitRadiusConfigured,
                BaseDamage = baseDamage,
                Attribute = DamageAttribute.Holy,
                SkillId = "ariel-a",
                RemainingPierce = Mathf.Max(0, pierce),
                ArielJudgementExplosionDamage = explodeOnDestroy ? baseDamage : 0f,
                ArielJudgementExplosionCount = explodeOnDestroy ? 2 : 0
            });
        }

        private bool TryCastArielRadiantShield()
        {
            var skill = FindSelectedSkill(SkillSlot.B);
            if (skill == null || !HasLearnedActive(SkillSlot.B) || arielRadiantShieldCooldownRemaining > 0f)
            {
                return false;
            }

            var shield = (35f + powerStatConfigured * Mathf.Max(0f, skill.SpellPowerCoefficient)) * GetArielShieldMultiplier();
            if (HasChoice("ariel-b-trait-1"))
            {
                shield *= 1.30f;
            }

            if (HasChoice("ariel-b-master-1"))
            {
                shield *= 1.50f;
            }

            var duration = ArielRadiantShieldDuration + (HasChoice("ariel-b-trait-2") ? 2f : 0f);
            arielRadiantShieldBurstDamage = HasChoice("ariel-b-trait-4") ? shield * 0.60f : 0f;
            arielRadiantShieldReflectMultiplier = HasChoice("ariel-b-master-2") ? 0.35f : 0f;
            ApplyArielUnitShield(shield, duration);

            if (HasChoice("ariel-b-trait-5"))
            {
                arielBlessingTimer = Mathf.Max(arielBlessingTimer, 5f);
            }

            arielRadiantShieldCooldownRemaining = GetArielCooldown(skill, 9f, HasChoice("ariel-b-trait-3") ? 0.80f : 1f);
            statusLabel = $"Radiant Shield applied {shield:0} shield for {duration:0.#}s.";
            return true;
        }

        private bool TryCastArielBlessingWave()
        {
            var skill = FindSelectedSkill(SkillSlot.C);
            if (skill == null || !HasLearnedActive(SkillSlot.C) || arielBlessingCooldownRemaining > 0f)
            {
                return false;
            }

            var target = FindNearestEnemy(eveAnchor.position, float.PositiveInfinity);
            if (target == null)
            {
                return false;
            }

            var radius = Mathf.Max(0.1f, skill.Radius);
            if (HasChoice("ariel-c-trait-4"))
            {
                radius *= 1.25f;
            }

            var duration = ArielBlessingDuration
                + (HasChoice("ariel-c-trait-3") ? 2f : 0f)
                + (HasArielSpreadBlessing() && HasChoice("ariel-h-trait-3") ? 2f : 0f);
            arielBlessingTimer = Mathf.Max(arielBlessingTimer, duration);

            var damage = GetArielSkillBaseDamage(skill);
            if (HasChoice("ariel-c-trait-1"))
            {
                damage *= 1.25f;
            }

            ApplyArielAreaDamage(target.Transform.position, radius, damage, "ariel-c", 1f);
            if (HasChoice("ariel-c-master-2"))
            {
                ApplyArielAreaDamage(target.Transform.position, radius, damage * 0.60f, "ariel-c", 1f);
            }

            var effect = CreateCircleEffect("BlessingWave", target.Transform.position, radius, 0.35f);
            effect.SkillId = "ariel-c";
            skillEffects.Add(effect);

            arielBlessingCooldownRemaining = GetArielCooldown(skill, 8f, 1f);
            statusLabel = $"Blessing Wave hit enemies around {target.DisplayName}; blessing {duration:0.#}s.";
            return true;
        }

        private bool TryCastArielCelestialBrand()
        {
            var skill = FindSelectedSkill(SkillSlot.D);
            if (skill == null || !HasLearnedActive(SkillSlot.D) || arielBrandCooldownRemaining > 0f)
            {
                return false;
            }

            var targetCount = HasChoice("ariel-d-trait-4") ? 2 : 1;
            var damageMultiplier = 1f;
            if (HasChoice("ariel-d-trait-1"))
            {
                damageMultiplier *= 1.30f;
            }

            if (HasChoice("ariel-d-trait-4"))
            {
                damageMultiplier *= 0.80f;
            }

            var exposureBonus = 0.18f + (HasChoice("ariel-d-trait-2") ? 0.08f : 0f);
            var duration = 6f + (HasChoice("ariel-d-trait-3") ? 3f : 0f);
            var hitCount = 0;
            for (var i = 0; i < targetCount; i++)
            {
                var target = FindStrongestUnbrandedArielTarget();
                if (target == null)
                {
                    break;
                }

                ApplyArielSkillDamage(target, GetArielSkillBaseDamage(skill), DamageAttribute.Holy, damageMultiplier, "ariel-d");
                ApplyArielHolyExposure(
                    target,
                    1,
                    duration,
                    exposureBonus,
                    HasArielBrandRevelation() && HasChoice("ariel-i-trait-3") ? 8f : 0f,
                    HasChoice("ariel-d-master-1") ? 0.25f : 0f,
                    HasChoice("ariel-d-master-2") ? 0.20f : 0f);
                hitCount += 1;
            }

            arielBrandCooldownRemaining = GetArielCooldown(skill, 10f, (HasArielBrandRevelation() && HasChoice("ariel-i-trait-2")) ? 0.80f : 1f);
            statusLabel = $"Celestial Brand marked {hitCount} target(s).";
            return hitCount > 0;
        }

        private bool TryCastArielArchangelDescent()
        {
            var skill = FindSelectedSkill(SkillSlot.E);
            if (skill == null || !HasLearnedActive(SkillSlot.E) || arielArchangelCooldownRemaining > 0f)
            {
                return false;
            }

            var damageMultiplier = 1f;
            if (HasChoice("ariel-e-trait-1"))
            {
                damageMultiplier *= 1.30f;
            }

            if (HasChoice("ariel-e-master-2"))
            {
                damageMultiplier *= 1.70f;
            }

            var hitCount = 0;
            for (var i = 0; i < enemies.Count; i++)
            {
                var enemy = enemies[i];
                if (enemy == null || enemy.CurrentHealth <= 0f)
                {
                    continue;
                }

                var targetMultiplier = enemy.HolyExposureTimer > 0f && HasChoice("ariel-e-trait-4") ? 1.50f : 1f;
                ApplyArielSkillDamage(enemy, GetArielSkillBaseDamage(skill), DamageAttribute.Holy, damageMultiplier * targetMultiplier, "ariel-e");
                hitCount += 1;
            }

            var shield = (50f + powerStatConfigured * 1.6f) * GetArielShieldMultiplier();
            if (HasChoice("ariel-e-trait-2"))
            {
                shield *= 1.30f;
            }

            if (HasChoice("ariel-e-master-2"))
            {
                shield *= 0.70f;
            }

            var duration = ArielArchangelShieldDuration + (HasChoice("ariel-e-trait-5") ? 3f : 0f);
            ApplyArielUnitShield(shield, duration, true);
            if (HasChoice("ariel-e-master-1"))
            {
                arielSanctuaryTimer = Mathf.Max(arielSanctuaryTimer, ArielSanctuaryDuration);
            }

            if (HasArielSanctuaryProclamation())
            {
                arielSanctuaryProclamationTimer = Mathf.Max(arielSanctuaryProclamationTimer, ArielSanctuaryDuration);
            }

            if (projectileRoot != null)
            {
                var fieldCenter = new Vector3(fieldSize.x * 0.5f, fieldSize.y * 0.5f, 0f);
                var fieldRadius = Mathf.Sqrt((fieldSize.x * fieldSize.x) + (fieldSize.y * fieldSize.y)) * 0.5f;
                var effect = CreateCircleEffect("ArchangelDescent", fieldCenter, fieldRadius, 0.45f);
                effect.SkillId = "ariel-e";
                if (effect.Renderer != null)
                {
                    effect.Renderer.color = new Color(1f, 0.94f, 0.68f, 0.32f);
                    effect.Renderer.sortingOrder = 20;
                }

                skillEffects.Add(effect);
            }

            arielArchangelCooldownRemaining = GetArielCooldown(skill, 17f, GetArielArchangelCooldownMultiplier());
            statusLabel = $"Archangel Descent hit {hitCount} enemy(s), shield {shield:0}.";
            return true;
        }

        private void ApplyArielAreaDamage(Vector3 center, float radius, float baseDamage, string skillId, float finalMultiplier)
        {
            for (var i = 0; i < enemies.Count; i++)
            {
                var enemy = enemies[i];
                if (enemy == null || enemy.CurrentHealth <= 0f || Vector2.Distance(enemy.Transform.position, center) > radius + GetEnemyHitRadius(enemy))
                {
                    continue;
                }

                ApplyArielSkillDamage(enemy, baseDamage, DamageAttribute.Holy, finalMultiplier, skillId);
            }
        }

        private void ApplyArielSkillDamage(EnemyRuntime enemy, float baseDamage, DamageAttribute attribute, float finalMultiplier, string skillId)
        {
            if (enemy == null || enemy.CurrentHealth <= 0f)
            {
                return;
            }

            var arielMultiplier = GetArielFinalDamageMultiplier(enemy, attribute, skillId);
            var result = DamageCalculator.Resolve(
                baseDamage,
                attribute,
                enemy.Defenses,
                flatDefenseReduction: GetArielFlatDefenseReduction(enemy, attribute),
                criticalChanceBonus: GetArielCriticalChanceBonus(attribute),
                targetCriticalResistance: enemy.CriticalResistance,
                criticalDamageTakenBonus: GetArielCriticalDamageTakenBonus(enemy, skillId),
                finalDamageMultiplier: enemy.DamageTakenMultiplier * Mathf.Max(0f, finalMultiplier) * arielMultiplier);
            var applied = ApplyDamageToEnemy(enemy, result.FinalDamage);
            enemy.FlashTimer = 0.08f;
            TrackArielHolyExposureDamage(enemy, attribute, result.FinalDamage);
            Debug.Log($"[CombatDamage] Ariel.{skillId} -> {enemy.DisplayName}: {result.FormulaLog}; Applied={applied:0.##}, ShieldLeft={enemy.ShieldValue:0.##}, HpLeft={Mathf.Max(0f, enemy.CurrentHealth):0.##}");
        }

        private void ApplyArielHolyExposure(
            EnemyRuntime enemy,
            int stacks,
            float duration,
            float damageTakenBonus,
            float flatDefenseReduction,
            float criticalDamageTakenBonus,
            float detonationMultiplier)
        {
            if (enemy == null)
            {
                return;
            }

            enemy.HolyExposureStacks = Mathf.Clamp(enemy.HolyExposureStacks + Mathf.Max(1, stacks), 0, 10);
            enemy.HolyExposureTimer = Mathf.Max(enemy.HolyExposureTimer, duration);
            enemy.HolyExposureDamageTakenBonus = Mathf.Max(enemy.HolyExposureDamageTakenBonus, damageTakenBonus);
            enemy.HolyExposureFlatDefenseReduction = Mathf.Max(enemy.HolyExposureFlatDefenseReduction, flatDefenseReduction);
            enemy.HolyExposureCriticalDamageTakenBonus = Mathf.Max(enemy.HolyExposureCriticalDamageTakenBonus, criticalDamageTakenBonus);
            enemy.HolyExposureDetonationMultiplier = Mathf.Max(enemy.HolyExposureDetonationMultiplier, detonationMultiplier);
        }

        private void ResolveArielHolyExposureExpired(EnemyRuntime enemy)
        {
            if (enemy == null)
            {
                return;
            }

            if (enemy.HolyExposureDetonationMultiplier > 0f && enemy.HolyExposureAccumulatedDamage > 0f)
            {
                ApplyArielSkillDamage(
                    enemy,
                    enemy.HolyExposureAccumulatedDamage * enemy.HolyExposureDetonationMultiplier,
                    DamageAttribute.Holy,
                    1f,
                    "ariel-d-detonation");
            }

            enemy.HolyExposureStacks = 0;
            enemy.HolyExposureDamageTakenBonus = 0f;
            enemy.HolyExposureFlatDefenseReduction = 0f;
            enemy.HolyExposureCriticalDamageTakenBonus = 0f;
            enemy.HolyExposureDetonationMultiplier = 0f;
            enemy.HolyExposureAccumulatedDamage = 0f;
        }

        private void TrackArielHolyExposureDamage(EnemyRuntime enemy, DamageAttribute attribute, float finalDamage)
        {
            if (enemy == null || attribute != DamageAttribute.Holy || enemy.HolyExposureTimer <= 0f || finalDamage <= 0f)
            {
                return;
            }

            enemy.HolyExposureAccumulatedDamage += finalDamage;
        }

        private void ApplyArielUnitShield(float shield, float duration, bool markAsArchangelShield = false)
        {
            var clampedShield = Mathf.Max(0f, shield);
            var clampedDuration = Mathf.Max(0f, duration);
            var previousShield = unitShieldValue;
            unitShieldValue = Mathf.Max(unitShieldValue, clampedShield);
            unitShieldTimer = Mathf.Max(unitShieldTimer, clampedDuration);

            if (!IsSelectedArielMonster())
            {
                return;
            }

            if (markAsArchangelShield)
            {
                if (clampedShield >= previousShield && clampedShield > 0f)
                {
                    arielArchangelShieldValue = clampedShield;
                    arielArchangelShieldTimer = clampedDuration;
                }

                return;
            }

            if (clampedShield > previousShield && arielArchangelShieldValue > 0f)
            {
                arielArchangelShieldValue = 0f;
                arielArchangelShieldTimer = 0f;
            }
        }

        private void HandleArielShieldAbsorbed(float absorbed, float shieldBeforeAbsorb, EnemyRuntime sourceEnemy)
        {
            if (!IsSelectedArielMonster() || absorbed <= 0f)
            {
                return;
            }

            if (shieldBeforeAbsorb > 0f && arielArchangelShieldValue > 0f)
            {
                var archangelShare = Mathf.Clamp01(arielArchangelShieldValue / shieldBeforeAbsorb);
                arielArchangelShieldValue = Mathf.Max(0f, arielArchangelShieldValue - (absorbed * archangelShare));
                if (Mathf.Approximately(arielArchangelShieldValue, 0f))
                {
                    arielArchangelShieldTimer = 0f;
                }
            }

            if (arielRadiantShieldReflectMultiplier > 0f)
            {
                if (sourceEnemy != null && sourceEnemy.CurrentHealth > 0f)
                {
                    ApplyArielSkillDamage(sourceEnemy, absorbed * arielRadiantShieldReflectMultiplier, DamageAttribute.Holy, 1f, "ariel-b-reflect");
                }
            }

            if (unitShieldValue <= 0f)
            {
                TriggerArielRadiantShieldBurst();
            }
        }

        private void TriggerArielRadiantShieldBurst()
        {
            if (!IsSelectedArielMonster() || arielRadiantShieldBurstDamage <= 0f || eveAnchor == null)
            {
                arielRadiantShieldBurstDamage = 0f;
                return;
            }

            ApplyArielAreaDamage(eveAnchor.position, 3f, arielRadiantShieldBurstDamage, "ariel-b-burst", 1f);
            arielRadiantShieldBurstDamage = 0f;
        }

        private void ExplodeArielJudgementLight(Vector3 center, float baseDamage, int explosions)
        {
            for (var i = 0; i < Mathf.Max(1, explosions); i++)
            {
                ApplyArielAreaDamage(center, ArielJudgementExplosionRadius, baseDamage, "ariel-a-explosion", 1f);
            }

            var effect = CreateCircleEffect("JudgementLightExplosion", center, ArielJudgementExplosionRadius, 0.45f);
            effect.SkillId = "ariel-a-explosion";
            if (effect.Renderer != null)
            {
                effect.Renderer.color = new Color(1f, 1f, 1f, 0.65f);
                effect.Renderer.sortingOrder = 29;
            }

            skillEffects.Add(effect);
        }

        private bool TryTriggerArielJudgementLightExplosion(ProjectileRuntime projectile)
        {
            if (projectile == null
                || projectile.ArielJudgementExplosionCount <= 0
                || projectile.ArielJudgementExplosionDamage <= 0f
                || !string.Equals(projectile.SkillId, "ariel-a", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            var center = projectile.Transform != null ? projectile.Transform.position : currentAttackPoint;
            var damage = projectile.ArielJudgementExplosionDamage;
            var count = projectile.ArielJudgementExplosionCount;
            projectile.ArielJudgementExplosionDamage = 0f;
            projectile.ArielJudgementExplosionCount = 0;
            ExplodeArielJudgementLight(center, damage, count);
            return true;
        }

        private EnemyRuntime FindStrongestUnbrandedArielTarget()
        {
            EnemyRuntime best = null;
            var bestScore = float.MinValue;
            for (var i = 0; i < enemies.Count; i++)
            {
                var enemy = enemies[i];
                if (enemy == null || enemy.CurrentHealth <= 0f || enemy.HolyExposureTimer > 0f)
                {
                    continue;
                }

                var score = enemy.MaxHealth + enemy.CurrentHealth;
                if (score <= bestScore)
                {
                    continue;
                }

                best = enemy;
                bestScore = score;
            }

            return best ?? FindNearestEnemy(eveAnchor.position, float.PositiveInfinity);
        }

        private float GetArielSkillBaseDamage(SkillDefinition skill)
        {
            if (skill == null)
            {
                return 0f;
            }

            var effectivePower = powerStatConfigured;
            if (arielBlessingTimer > 0f && HasChoice("ariel-c-master-1"))
            {
                effectivePower *= 1.18f;
            }

            return skill.BaseDamage + effectivePower * skill.SpellPowerCoefficient;
        }

        private float GetArielCooldown(SkillDefinition skill, float fallback, float multiplier)
        {
            var cooldown = skill != null && skill.CooldownSeconds > 0f ? skill.CooldownSeconds : fallback;
            return Mathf.Max(0.1f, cooldown * Mathf.Max(0.05f, multiplier));
        }

        private int GetArielJudgementMagazineCapacity()
        {
            var capacity = magazineCapacityConfigured;
            var skill = FindSelectedSkill(SkillSlot.A);
            if (skill != null && skill.MagazineCapacity > 0)
            {
                capacity = skill.MagazineCapacity;
            }

            if (HasChoice("ariel-a-trait-2"))
            {
                capacity += 3;
            }

            if (HasArielGuidingLight() && HasChoice("ariel-f-trait-2"))
            {
                capacity += 2;
            }

            return Mathf.Max(1, capacity);
        }

        private float GetArielReloadSeconds()
        {
            var skill = FindSelectedSkill(SkillSlot.A);
            return skill != null && skill.ReloadSeconds > 0f ? skill.ReloadSeconds : reloadDurationConfigured;
        }

        private float GetArielHolyDamageMultiplier()
        {
            var bonus = 0f;
            if (HasArielGuidingLight())
            {
                bonus += 0.12f + (HasChoice("ariel-f-trait-1") ? 0.06f : 0f);
            }

            if (HasArielGuardianDoctrine() && HasChoice("ariel-g-trait-3") && unitShieldValue > 0f)
            {
                bonus += 0.10f;
            }

            if (arielBlessingTimer > 0f)
            {
                if (HasChoice("ariel-b-trait-5"))
                {
                    bonus += 0.12f;
                }

                if (HasArielSpreadBlessing())
                {
                    bonus += 0.15f + (HasChoice("ariel-h-trait-1") ? 0.07f : 0f);
                }

                if (HasChoice("ariel-c-trait-5") && unitShieldValue > 0f)
                {
                    bonus += 0.10f;
                }
            }

            if (HasArielSanctuaryProclamation() && HasArielArchangelShieldActive())
            {
                bonus += 0.20f + (HasChoice("ariel-j-trait-2") ? 0.10f : 0f);
            }

            return 1f + bonus;
        }

        private float GetArielFinalDamageMultiplier(EnemyRuntime enemy, DamageAttribute attribute, string skillId)
        {
            if (!IsSelectedArielMonster() || enemy == null)
            {
                return 1f;
            }

            var bonus = attribute == DamageAttribute.Holy ? GetArielHolyDamageMultiplier() - 1f : 0f;
            if (enemy.HolyExposureTimer > 0f)
            {
                bonus += enemy.HolyExposureDamageTakenBonus;
                if (HasArielBrandRevelation())
                {
                    bonus += 0.10f + (HasChoice("ariel-i-trait-1") ? 0.05f : 0f);
                }

                if (HasChoice("ariel-d-trait-5") && unitShieldValue > 0f)
                {
                    bonus += 0.10f;
                }
            }

            return 1f + bonus;
        }

        private float GetArielFlatDefenseReduction(EnemyRuntime enemy, DamageAttribute attribute)
        {
            if (!IsSelectedArielMonster() || enemy == null || attribute != DamageAttribute.Holy || enemy.HolyExposureTimer <= 0f)
            {
                return 0f;
            }

            return enemy.HolyExposureFlatDefenseReduction;
        }

        private float GetArielCriticalDamageTakenBonus(EnemyRuntime enemy, string skillId)
        {
            if (!IsSelectedArielMonster() || enemy == null || enemy.HolyExposureTimer <= 0f)
            {
                return 0f;
            }

            return enemy.HolyExposureCriticalDamageTakenBonus;
        }

        private float GetArielCriticalChanceBonus(DamageAttribute attribute)
        {
            return IsSelectedArielMonster()
                && attribute == DamageAttribute.Holy
                && HasArielGuidingLight()
                && HasChoice("ariel-f-trait-3")
                ? 0.08f
                : 0f;
        }

        private float GetArielIncomingDamageAfterReduction(float incomingDamage)
        {
            if (!IsSelectedArielMonster() || arielSanctuaryTimer <= 0f || !HasChoice("ariel-e-master-1"))
            {
                return incomingDamage;
            }

            return incomingDamage * 0.82f;
        }

        private float GetArielActionSpeedMultiplier()
        {
            var bonus = 0f;
            if (arielBlessingTimer > 0f && !HasChoice("ariel-c-master-1"))
            {
                bonus += 0.12f + (HasChoice("ariel-c-trait-2") ? 0.06f : 0f);
            }

            if (HasArielSanctuaryProclamation() && arielSanctuaryProclamationTimer > 0f)
            {
                bonus += 0.15f + (HasChoice("ariel-j-trait-1") ? 0.07f : 0f);
            }

            return 1f + bonus;
        }

        private float GetArielCooldownChargeMultiplier()
        {
            var bonus = 0f;
            if (HasArielSpreadBlessing() && arielBlessingTimer > 0f)
            {
                bonus += 0.10f + (HasChoice("ariel-h-trait-2") ? 0.05f : 0f);
            }

            return 1f + bonus;
        }

        private float GetArielShieldMultiplier()
        {
            var bonus = 0f;
            if (HasArielGuardianDoctrine())
            {
                bonus += 0.18f + (HasChoice("ariel-g-trait-1") ? 0.08f : 0f);
            }

            return 1f + bonus;
        }

        private float GetArielArchangelCooldownMultiplier()
        {
            var multiplier = HasChoice("ariel-e-trait-3") ? 0.80f : 1f;
            if (HasArielSanctuaryProclamation() && HasChoice("ariel-j-trait-3"))
            {
                multiplier *= 0.85f;
            }

            return multiplier;
        }

        private int GetArielShieldedAllyCount()
        {
            return unitShieldValue > 0f ? 1 : 0;
        }

        private bool HasArielArchangelShieldActive()
        {
            return arielArchangelShieldTimer > 0f && arielArchangelShieldValue > 0f && unitShieldValue > 0f;
        }

        private bool HasArielPassive(string passiveId, string passiveName)
        {
            return IsSelectedArielMonster()
                && ((!string.IsNullOrWhiteSpace(passiveId) && chosenSkillChoiceIds.Contains(passiveId))
                    || (!string.IsNullOrWhiteSpace(passiveName) && learnedPassiveSkillNames.Contains(passiveName)));
        }

        private bool HasArielGuidingLight()
        {
            return HasArielPassive("ariel-f", "빛의 인도");
        }

        private bool HasArielGuardianDoctrine()
        {
            return HasArielPassive("ariel-g", "수호 교리");
        }

        private bool HasArielSpreadBlessing()
        {
            return HasArielPassive("ariel-h", "축복 전파");
        }

        private bool HasArielBrandRevelation()
        {
            return HasArielPassive("ariel-i", "낙인 계시");
        }

        private bool HasArielSanctuaryProclamation()
        {
            return HasArielPassive("ariel-j", "성역 선포");
        }

        private bool IsSelectedArielMonster()
        {
            return selectedMonster != null &&
                string.Equals(selectedMonster.MonsterId, "ariel", StringComparison.OrdinalIgnoreCase);
        }
    }
}
