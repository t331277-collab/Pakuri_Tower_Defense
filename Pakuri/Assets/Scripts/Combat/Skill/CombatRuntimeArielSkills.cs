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
        private const float ArielArchangelEffectDuration = 1.2f;
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
        private int unitShieldAppliedFrame = -1;

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
            unitShieldAppliedFrame = -1;

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
            GetSelectedMonsterSkillRuntime()?.UpdateCooldowns();
        }

        private bool TryTriggerSelectedMonsterAutomaticSkills()
        {
            return GetSelectedMonsterSkillRuntime()?.TryTriggerAutomaticSkills() ?? false;
        }

        private int GetSelectedMonsterMagazineCapacity()
        {
            return GetSelectedMonsterSkillRuntime()?.GetMagazineCapacity(magazineCapacityConfigured)
                ?? magazineCapacityConfigured;
        }

        private float GetSelectedMonsterActionSpeedMultiplier()
        {
            return GetSelectedMonsterSkillRuntime()?.GetActionSpeedMultiplier(1f) ?? 1f;
        }

        private void UpdateArielSkillCooldowns()
        {
            var elapsed = Time.deltaTime * GetArielCooldownChargeMultiplier();
            arielRadiantShieldCooldownRemaining = Mathf.Max(0f, arielRadiantShieldCooldownRemaining - elapsed);
            arielBlessingCooldownRemaining = Mathf.Max(0f, arielBlessingCooldownRemaining - elapsed);
            arielBrandCooldownRemaining = Mathf.Max(0f, arielBrandCooldownRemaining - elapsed);
            arielArchangelCooldownRemaining = Mathf.Max(0f, arielArchangelCooldownRemaining - elapsed);

        }

        private void UpdateSelectedUnitShieldTimer(float elapsed)
        {
            if (unitShieldTimer <= 0f && unitShieldValue <= 0f)
            {
                return;
            }

            if (unitShieldAppliedFrame != Time.frameCount)
            {
                unitShieldTimer = Mathf.Max(0f, unitShieldTimer - Mathf.Max(0f, elapsed));
            }

            if (Mathf.Approximately(unitShieldTimer, 0f) && unitShieldValue > 0f)
            {
                unitShieldValue = 0f;
                unitShieldAppliedFrame = -1;
                arielArchangelShieldValue = 0f;
                arielArchangelShieldTimer = 0f;

                if (selectedUnitRuntime != null && selectedUnitRuntime.ArielRadiantShieldBurstDamage > 0f)
                {
                    TriggerArielUnitRadiantShieldBurst(selectedUnitRuntime);
                }
                else
                {
                    TriggerArielRadiantShieldBurst();
                }

                if (selectedUnitRuntime != null)
                {
                    selectedUnitRuntime.ShieldValue = 0f;
                    selectedUnitRuntime.ShieldTimer = 0f;
                    selectedUnitRuntime.ShieldAppliedFrame = -1;
                    selectedUnitRuntime.ArielShieldSource = null;
                    selectedUnitRuntime.ArielArchangelShieldValue = 0f;
                    selectedUnitRuntime.ArielArchangelShieldTimer = 0f;
                    selectedUnitRuntime.ArielRadiantShieldBurstDamage = 0f;
                    selectedUnitRuntime.ArielRadiantShieldReflectMultiplier = 0f;
                }
            }
            else if (selectedUnitRuntime != null)
            {
                selectedUnitRuntime.ShieldValue = unitShieldValue;
                selectedUnitRuntime.ShieldTimer = unitShieldTimer;
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

            var effect = CreateCircleEffect("BlessingWave", target.Transform.position, radius, 0.35f, skill.SkillEffectPrefab);
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

            CreateArielArchangelDescentEffect(skill);

            arielArchangelCooldownRemaining = GetArielCooldown(skill, 17f, GetArielArchangelCooldownMultiplier());
            statusLabel = $"Archangel Descent hit {hitCount} enemy(s), shield {shield:0}.";
            return true;
        }

        private bool TryTickArielUnitSkill(CombatUnitRuntime runtime, CombatSkillRuntime skillRuntime, float elapsed)
        {
            if (!IsArielCombatUnit(runtime) || skillRuntime == null || skillRuntime.Skill == null)
            {
                return false;
            }

            var scaledElapsed = Mathf.Max(0f, elapsed) * GetArielUnitCooldownChargeMultiplier(runtime);
            skillRuntime.Tick(scaledElapsed);
            switch (skillRuntime.Skill.Slot)
            {
                case SkillSlot.A:
                    skillRuntime.TickReload(elapsed * GetArielUnitActionSpeedMultiplier(runtime), ResolveManifestedMagazineCapacity(runtime, skillRuntime.Skill));
                    TryFireArielUnitJudgementLight(runtime, skillRuntime);
                    return true;
                case SkillSlot.B:
                    TryCastArielUnitRadiantShield(runtime, skillRuntime);
                    return true;
                case SkillSlot.C:
                    TryCastArielUnitBlessingWave(runtime, skillRuntime);
                    return true;
                case SkillSlot.D:
                    TryCastArielUnitCelestialBrand(runtime, skillRuntime);
                    return true;
                case SkillSlot.E:
                    TryCastArielUnitArchangelDescent(runtime, skillRuntime);
                    return true;
                default:
                    return false;
            }
        }

        private bool TryFireArielUnitJudgementLight(CombatUnitRuntime runtime, CombatSkillRuntime skillRuntime)
        {
            var skill = skillRuntime != null ? skillRuntime.Skill : null;
            if (!IsArielCombatUnit(runtime)
                || skill == null
                || runtime.Transform == null
                || skillRuntime.ReloadRemaining > 0f
                || skillRuntime.ShotCooldownRemaining > 0f)
            {
                return false;
            }

            if (skillRuntime.ShotsRemaining <= 0)
            {
                skillRuntime.ShotsRemaining = 0;
                skillRuntime.ReloadDuration = ResolveManifestedReloadDuration(runtime, skill);
                skillRuntime.ReloadRemaining = skillRuntime.ReloadDuration;
                return false;
            }

            var target = FindNearestManifestedMonsterTarget(runtime.Transform.position);
            if (target == null || target.Transform == null)
            {
                skillRuntime.ShotCooldownRemaining = 0.25f;
                return false;
            }

            var direction = target.Transform.position - runtime.Transform.position;
            direction.z = 0f;
            if (direction.sqrMagnitude <= 0.0001f)
            {
                direction = Vector3.right;
            }

            var damageMultiplier = 1f;
            if (HasArielUnitChoice(runtime, "ariel-a-trait-1"))
            {
                damageMultiplier *= 1.25f;
            }

            if (HasArielUnitChoice(runtime, "ariel-a-trait-5"))
            {
                damageMultiplier *= 1f + GetArielUnitShieldedAllyCount() * 0.06f;
            }

            var isLastShot = skillRuntime.ShotsRemaining <= 1;
            FireManifestedMonsterProjectile(runtime, skill, runtime.Transform.position, direction, damageMultiplier, ResolveManifestedProjectilePierce(runtime, skill), 0);
            if (isLastShot && HasArielUnitChoice(runtime, "ariel-a-master-1") && projectiles.Count > 0)
            {
                var projectile = projectiles[projectiles.Count - 1];
                if (projectile != null && projectile.ManifestedSource == runtime && string.Equals(projectile.SkillId, "ariel-a", StringComparison.OrdinalIgnoreCase))
                {
                    projectile.ArielJudgementExplosionDamage = projectile.BaseDamage;
                    projectile.ArielJudgementExplosionCount = 2;
                }
            }

            skillRuntime.ShotsRemaining -= 1;
            skillRuntime.ShotInterval = ResolveManifestedShotInterval(runtime, skill);
            skillRuntime.ShotCooldownRemaining = skillRuntime.ShotInterval;
            if (skillRuntime.ShotsRemaining <= 0)
            {
                skillRuntime.ShotsRemaining = 0;
                skillRuntime.ReloadDuration = ResolveManifestedReloadDuration(runtime, skill);
                skillRuntime.ReloadRemaining = skillRuntime.ReloadDuration;
            }

            return true;
        }

        private bool TryCastArielUnitRadiantShield(CombatUnitRuntime runtime, CombatSkillRuntime skillRuntime)
        {
            var skill = skillRuntime != null ? skillRuntime.Skill : null;
            if (!IsArielCombatUnit(runtime) || skill == null || skillRuntime.CooldownRemaining > 0f)
            {
                return false;
            }

            var shield = (35f + runtime.PowerStat * Mathf.Max(0f, skill.SpellPowerCoefficient)) * GetArielUnitShieldMultiplier(runtime);
            shield *= HasArielUnitChoice(runtime, "ariel-b-trait-1") ? 1.30f : 1f;
            shield *= HasArielUnitChoice(runtime, "ariel-b-master-1") ? 1.50f : 1f;

            var duration = ArielRadiantShieldDuration + (HasArielUnitChoice(runtime, "ariel-b-trait-2") ? 2f : 0f);
            var burstDamage = HasArielUnitChoice(runtime, "ariel-b-trait-4") ? shield * 0.60f : 0f;
            var reflectMultiplier = HasArielUnitChoice(runtime, "ariel-b-master-2") ? 0.35f : 0f;
            ApplyArielTeamShield(runtime, shield, duration, false, burstDamage, reflectMultiplier);
            if (HasArielUnitChoice(runtime, "ariel-b-trait-5"))
            {
                runtime.ArielBlessingTimer = Mathf.Max(runtime.ArielBlessingTimer, 5f);
            }

            skillRuntime.CooldownDuration = GetArielUnitCooldown(runtime, skill, 9f, HasArielUnitChoice(runtime, "ariel-b-trait-3") ? 0.80f : 1f);
            skillRuntime.CooldownRemaining = skillRuntime.CooldownDuration;
            CreateManifestedSkillVisual(runtime, skill, FindNearestManifestedMonsterTarget(runtime.Transform.position));
            statusLabel = $"{runtime.Monster.DisplayName} Radiant Shield applied {shield:0} shield to party.";
            return true;
        }

        private bool TryCastArielUnitBlessingWave(CombatUnitRuntime runtime, CombatSkillRuntime skillRuntime)
        {
            var skill = skillRuntime != null ? skillRuntime.Skill : null;
            if (!IsArielCombatUnit(runtime) || skill == null || runtime.Transform == null || skillRuntime.CooldownRemaining > 0f)
            {
                return false;
            }

            var target = FindNearestManifestedMonsterTarget(runtime.Transform.position);
            if (target == null || target.Transform == null)
            {
                skillRuntime.CooldownRemaining = 0.25f;
                return false;
            }

            var radius = Mathf.Max(0.1f, skill.Radius);
            radius *= HasArielUnitChoice(runtime, "ariel-c-trait-4") ? 1.25f : 1f;
            var duration = ArielBlessingDuration
                + (HasArielUnitChoice(runtime, "ariel-c-trait-3") ? 2f : 0f)
                + (HasArielUnitPassive(runtime, "ariel-h") && HasArielUnitChoice(runtime, "ariel-h-trait-3") ? 2f : 0f);
            runtime.ArielBlessingTimer = Mathf.Max(runtime.ArielBlessingTimer, duration);

            var damage = GetArielUnitSkillBaseDamage(runtime, skill);
            damage *= HasArielUnitChoice(runtime, "ariel-c-trait-1") ? 1.25f : 1f;
            ApplyArielUnitAreaDamage(runtime, target.Transform.position, radius, damage, "ariel-c", 1f);
            if (HasArielUnitChoice(runtime, "ariel-c-master-2"))
            {
                ApplyArielUnitAreaDamage(runtime, target.Transform.position, radius, damage * 0.60f, "ariel-c", 1f);
            }

            CreateManifestedSkillVisual(runtime, skill, target);
            skillRuntime.CooldownDuration = GetArielUnitCooldown(runtime, skill, 8f, 1f);
            skillRuntime.CooldownRemaining = skillRuntime.CooldownDuration;
            statusLabel = $"{runtime.Monster.DisplayName} Blessing Wave hit around {target.DisplayName}.";
            return true;
        }

        private bool TryCastArielUnitCelestialBrand(CombatUnitRuntime runtime, CombatSkillRuntime skillRuntime)
        {
            var skill = skillRuntime != null ? skillRuntime.Skill : null;
            if (!IsArielCombatUnit(runtime) || skill == null || skillRuntime.CooldownRemaining > 0f)
            {
                return false;
            }

            var targetCount = HasArielUnitChoice(runtime, "ariel-d-trait-4") ? 2 : 1;
            var damageMultiplier = 1f;
            damageMultiplier *= HasArielUnitChoice(runtime, "ariel-d-trait-1") ? 1.30f : 1f;
            damageMultiplier *= HasArielUnitChoice(runtime, "ariel-d-trait-4") ? 0.80f : 1f;
            var exposureBonus = 0.18f + (HasArielUnitChoice(runtime, "ariel-d-trait-2") ? 0.08f : 0f);
            var duration = 6f + (HasArielUnitChoice(runtime, "ariel-d-trait-3") ? 3f : 0f);
            var hitCount = 0;
            for (var i = 0; i < targetCount; i++)
            {
                var target = FindStrongestUnbrandedArielTarget();
                if (target == null)
                {
                    break;
                }

                ApplyArielUnitSkillDamage(runtime, target, GetArielUnitSkillBaseDamage(runtime, skill), DamageAttribute.Holy, damageMultiplier, "ariel-d");
                ApplyArielHolyExposure(
                    target,
                    1,
                    duration,
                    exposureBonus,
                    HasArielUnitPassive(runtime, "ariel-i") && HasArielUnitChoice(runtime, "ariel-i-trait-3") ? 8f : 0f,
                    HasArielUnitChoice(runtime, "ariel-d-master-1") ? 0.25f : 0f,
                    HasArielUnitChoice(runtime, "ariel-d-master-2") ? 0.20f : 0f);
                hitCount += 1;
            }

            skillRuntime.CooldownDuration = GetArielUnitCooldown(runtime, skill, 10f, HasArielUnitPassive(runtime, "ariel-i") && HasArielUnitChoice(runtime, "ariel-i-trait-2") ? 0.80f : 1f);
            skillRuntime.CooldownRemaining = skillRuntime.CooldownDuration;
            statusLabel = $"{runtime.Monster.DisplayName} Celestial Brand marked {hitCount} target(s).";
            return hitCount > 0;
        }

        private bool TryCastArielUnitArchangelDescent(CombatUnitRuntime runtime, CombatSkillRuntime skillRuntime)
        {
            var skill = skillRuntime != null ? skillRuntime.Skill : null;
            if (!IsArielCombatUnit(runtime) || skill == null || skillRuntime.CooldownRemaining > 0f)
            {
                return false;
            }

            var damageMultiplier = 1f;
            damageMultiplier *= HasArielUnitChoice(runtime, "ariel-e-trait-1") ? 1.30f : 1f;
            damageMultiplier *= HasArielUnitChoice(runtime, "ariel-e-master-2") ? 1.70f : 1f;
            var hitCount = 0;
            for (var i = 0; i < enemies.Count; i++)
            {
                var enemy = enemies[i];
                if (enemy == null || enemy.CurrentHealth <= 0f)
                {
                    continue;
                }

                var targetMultiplier = enemy.HolyExposureTimer > 0f && HasArielUnitChoice(runtime, "ariel-e-trait-4") ? 1.50f : 1f;
                ApplyArielUnitSkillDamage(runtime, enemy, GetArielUnitSkillBaseDamage(runtime, skill), DamageAttribute.Holy, damageMultiplier * targetMultiplier, "ariel-e");
                hitCount += 1;
            }

            var shield = (50f + runtime.PowerStat * 1.6f) * GetArielUnitShieldMultiplier(runtime);
            shield *= HasArielUnitChoice(runtime, "ariel-e-trait-2") ? 1.30f : 1f;
            shield *= HasArielUnitChoice(runtime, "ariel-e-master-2") ? 0.70f : 1f;
            var duration = ArielArchangelShieldDuration + (HasArielUnitChoice(runtime, "ariel-e-trait-5") ? 3f : 0f);
            ApplyArielTeamShield(runtime, shield, duration, true, 0f, 0f);
            if (HasArielUnitChoice(runtime, "ariel-e-master-1"))
            {
                runtime.ArielSanctuaryTimer = Mathf.Max(runtime.ArielSanctuaryTimer, ArielSanctuaryDuration);
            }

            if (HasArielUnitPassive(runtime, "ariel-j"))
            {
                runtime.ArielSanctuaryProclamationTimer = Mathf.Max(runtime.ArielSanctuaryProclamationTimer, ArielSanctuaryDuration);
            }

            CreateArielArchangelDescentEffect(skill);
            skillRuntime.CooldownDuration = GetArielUnitCooldown(runtime, skill, 17f, GetArielUnitArchangelCooldownMultiplier(runtime));
            skillRuntime.CooldownRemaining = skillRuntime.CooldownDuration;
            statusLabel = $"{runtime.Monster.DisplayName} Archangel Descent hit {hitCount} enemy(s), shield {shield:0}.";
            return true;
        }

        private void CreateArielArchangelDescentEffect(SkillDefinition skill)
        {
            var fieldCenter = new Vector3(fieldSize.x * 0.5f, fieldSize.y * 0.5f, 0f);
            var fieldRadius = Mathf.Max(
                1f,
                Mathf.Sqrt((fieldSize.x * fieldSize.x) + (fieldSize.y * fieldSize.y)) * 0.5f);
            var effect = CreateCircleEffect("ArchangelDescent", fieldCenter, fieldRadius, ArielArchangelEffectDuration, skill != null ? skill.SkillEffectPrefab : null);
            effect.SkillId = "ariel-e";
            if (effect.Renderer != null)
            {
                effect.Renderer.color = new Color(1f, 0.94f, 0.68f, 0.52f);
                effect.Renderer.sortingOrder = 28;
            }

            skillEffects.Add(effect);
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
            var applied = ApplyDamageToEnemy(enemy, result.FinalDamage, attribute);
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
            ApplyArielTeamShield(selectedUnitRuntime, shield, duration, markAsArchangelShield, arielRadiantShieldBurstDamage, arielRadiantShieldReflectMultiplier);
        }

        private void ApplyArielSelectedShield(float shield, float duration, bool markAsArchangelShield = false)
        {
            var clampedShield = Mathf.Max(0f, shield);
            var clampedDuration = Mathf.Max(0f, duration);
            var previousShield = unitShieldValue;
            unitShieldValue = Mathf.Max(unitShieldValue, clampedShield);
            unitShieldTimer = Mathf.Max(unitShieldTimer, clampedDuration);
            unitShieldAppliedFrame = Time.frameCount;

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

        private void ApplyArielTeamShield(
            CombatUnitRuntime source,
            float shield,
            float duration,
            bool markAsArchangelShield,
            float burstDamage,
            float reflectMultiplier)
        {
            ApplyArielSelectedShield(shield, duration, markAsArchangelShield);
            if (selectedUnitRuntime != null)
            {
                selectedUnitRuntime.ShieldValue = unitShieldValue;
                selectedUnitRuntime.ShieldTimer = unitShieldTimer;
                selectedUnitRuntime.ShieldAppliedFrame = unitShieldAppliedFrame;
                selectedUnitRuntime.ArielShieldSource = source;
                selectedUnitRuntime.ArielArchangelShieldValue = arielArchangelShieldValue;
                selectedUnitRuntime.ArielArchangelShieldTimer = arielArchangelShieldTimer;
                selectedUnitRuntime.ArielRadiantShieldBurstDamage = burstDamage;
                selectedUnitRuntime.ArielRadiantShieldReflectMultiplier = reflectMultiplier;
            }

            for (var i = 0; i < manifestedMonsters.Count; i++)
            {
                ApplyArielRuntimeShield(manifestedMonsters[i], source, shield, duration, markAsArchangelShield, burstDamage, reflectMultiplier);
            }
        }

        private void ApplyArielRuntimeShield(
            CombatUnitRuntime target,
            CombatUnitRuntime source,
            float shield,
            float duration,
            bool markAsArchangelShield,
            float burstDamage,
            float reflectMultiplier)
        {
            if (target == null || target.CurrentHealth <= 0f)
            {
                return;
            }

            var clampedShield = Mathf.Max(0f, shield);
            var clampedDuration = Mathf.Max(0f, duration);
            var previousShield = target.ShieldValue;
            target.ShieldValue = Mathf.Max(target.ShieldValue, clampedShield);
            target.ShieldTimer = Mathf.Max(target.ShieldTimer, clampedDuration);
            target.ShieldAppliedFrame = Time.frameCount;
            target.ArielShieldSource = source;
            target.ArielRadiantShieldBurstDamage = burstDamage;
            target.ArielRadiantShieldReflectMultiplier = reflectMultiplier;

            if (markAsArchangelShield)
            {
                if (clampedShield >= previousShield && clampedShield > 0f)
                {
                    target.ArielArchangelShieldValue = clampedShield;
                    target.ArielArchangelShieldTimer = clampedDuration;
                }
            }
            else if (clampedShield > previousShield && target.ArielArchangelShieldValue > 0f)
            {
                target.ArielArchangelShieldValue = 0f;
                target.ArielArchangelShieldTimer = 0f;
            }

            UpdateManifestedMonsterLabel(target);
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

        private void HandleArielUnitShieldAbsorbed(CombatUnitRuntime target, float absorbed, float shieldBeforeAbsorb, EnemyRuntime sourceEnemy)
        {
            if (target == null || absorbed <= 0f)
            {
                return;
            }

            if (IsSelectedCombatUnit(target))
            {
                HandleArielShieldAbsorbed(absorbed, shieldBeforeAbsorb, sourceEnemy);
                return;
            }

            if (shieldBeforeAbsorb > 0f && target.ArielArchangelShieldValue > 0f)
            {
                var archangelShare = Mathf.Clamp01(target.ArielArchangelShieldValue / shieldBeforeAbsorb);
                target.ArielArchangelShieldValue = Mathf.Max(0f, target.ArielArchangelShieldValue - (absorbed * archangelShare));
                if (Mathf.Approximately(target.ArielArchangelShieldValue, 0f))
                {
                    target.ArielArchangelShieldTimer = 0f;
                }
            }

            var source = target.ArielShieldSource != null ? target.ArielShieldSource : target;
            if (target.ArielRadiantShieldReflectMultiplier > 0f && sourceEnemy != null && sourceEnemy.CurrentHealth > 0f)
            {
                ApplyArielUnitSkillDamage(source, sourceEnemy, absorbed * target.ArielRadiantShieldReflectMultiplier, DamageAttribute.Holy, 1f, "ariel-b-reflect");
            }

            if (target.ShieldValue <= 0f)
            {
                TriggerArielUnitRadiantShieldBurst(target);
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

        private void TriggerArielUnitRadiantShieldBurst(CombatUnitRuntime target)
        {
            if (target == null || target.ArielRadiantShieldBurstDamage <= 0f || target.Transform == null)
            {
                if (target != null)
                {
                    target.ArielRadiantShieldBurstDamage = 0f;
                }

                return;
            }

            var source = target.ArielShieldSource != null ? target.ArielShieldSource : target;
            ApplyArielUnitAreaDamage(source, target.Transform.position, 3f, target.ArielRadiantShieldBurstDamage, "ariel-b-burst", 1f);
            target.ArielRadiantShieldBurstDamage = 0f;
        }

        private void ExplodeArielJudgementLight(Vector3 center, float baseDamage, int explosions)
        {
            for (var i = 0; i < Mathf.Max(1, explosions); i++)
            {
                ApplyArielAreaDamage(center, ArielJudgementExplosionRadius, baseDamage, "ariel-a-explosion", 1f);
            }

            var skill = FindSelectedSkill(SkillSlot.A);
            var effectPrefab = skill != null ? skill.SkillEffectPrefab : null;
            var effect = CreateCircleEffect("JudgementLightExplosion", center, ArielJudgementExplosionRadius, 0.45f, effectPrefab);
            effect.SkillId = "ariel-a-explosion";
            if (effect.Renderer != null)
            {
                effect.Renderer.color = new Color(1f, 1f, 1f, 0.65f);
                effect.Renderer.sortingOrder = 29;
            }

            skillEffects.Add(effect);
        }

        private void ExplodeArielUnitJudgementLight(CombatUnitRuntime runtime, Vector3 center, float baseDamage, int explosions)
        {
            if (!IsArielCombatUnit(runtime))
            {
                return;
            }

            for (var i = 0; i < Mathf.Max(1, explosions); i++)
            {
                ApplyArielUnitAreaDamage(runtime, center, ArielJudgementExplosionRadius, baseDamage, "ariel-a-explosion", 1f);
            }

            var skill = FindArielUnitSkill(runtime, SkillSlot.A);
            CreateManifestedCircleVisual(
                skill,
                center,
                ArielJudgementExplosionRadius,
                new Color(1f, 1f, 1f, 0.65f),
                29,
                0.45f);
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
            if (IsArielCombatUnit(projectile.ManifestedSource))
            {
                ExplodeArielUnitJudgementLight(projectile.ManifestedSource, center, damage, count);
            }
            else
            {
                ExplodeArielJudgementLight(center, damage, count);
            }
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
            return GetArielUnitShieldedAllyCount();
        }

        private int GetArielUnitShieldedAllyCount()
        {
            var count = unitShieldValue > 0f ? 1 : 0;
            for (var i = 0; i < manifestedMonsters.Count; i++)
            {
                var runtime = manifestedMonsters[i];
                if (runtime != null && runtime.ShieldValue > 0f)
                {
                    count += 1;
                }
            }

            return count;
        }

        private bool HasArielArchangelShieldActive()
        {
            return arielArchangelShieldTimer > 0f && arielArchangelShieldValue > 0f && unitShieldValue > 0f;
        }

        private bool HasArielUnitArchangelShieldActive(CombatUnitRuntime runtime)
        {
            if (IsSelectedCombatUnit(runtime))
            {
                return HasArielArchangelShieldActive();
            }

            return runtime != null && runtime.ArielArchangelShieldTimer > 0f && runtime.ArielArchangelShieldValue > 0f && runtime.ShieldValue > 0f;
        }

        private bool TryApplyArielUnitProjectileHit(ProjectileRuntime projectile, EnemyRuntime enemy, out DamageResult damageResult, out float appliedDamage)
        {
            damageResult = default;
            appliedDamage = 0f;
            var runtime = projectile != null ? projectile.ManifestedSource : null;
            if (!IsArielCombatUnit(runtime)
                || projectile == null
                || enemy == null
                || enemy.CurrentHealth <= 0f
                || !string.Equals(projectile.SkillId, "ariel-a", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            damageResult = ResolveArielUnitDamage(runtime, projectile.BaseDamage, projectile.Attribute, enemy, 1f, projectile.SkillId);
            appliedDamage = ApplyDamageToEnemy(enemy, damageResult.FinalDamage, damageResult.Attribute);
            TrackArielHolyExposureDamage(enemy, projectile.Attribute, damageResult.FinalDamage);
            if (HasArielUnitChoice(runtime, "ariel-a-master-2"))
            {
                ApplyArielHolyExposure(enemy, 1, 6f, 0.18f, 0f, 0f, 0f);
            }

            return true;
        }

        private void ApplyArielUnitAreaDamage(CombatUnitRuntime runtime, Vector3 center, float radius, float baseDamage, string skillId, float finalMultiplier)
        {
            for (var i = 0; i < enemies.Count; i++)
            {
                var enemy = enemies[i];
                if (enemy == null || enemy.CurrentHealth <= 0f || Vector2.Distance(enemy.Transform.position, center) > radius + GetEnemyHitRadius(enemy))
                {
                    continue;
                }

                ApplyArielUnitSkillDamage(runtime, enemy, baseDamage, DamageAttribute.Holy, finalMultiplier, skillId);
            }
        }

        private float ApplyArielUnitSkillDamage(CombatUnitRuntime runtime, EnemyRuntime enemy, float baseDamage, DamageAttribute attribute, float finalMultiplier, string skillId)
        {
            if (!IsArielCombatUnit(runtime) || enemy == null || enemy.CurrentHealth <= 0f || baseDamage <= 0f)
            {
                return 0f;
            }

            var result = ResolveArielUnitDamage(runtime, baseDamage, attribute, enemy, finalMultiplier, skillId);
            var applied = ApplyDamageToEnemy(enemy, result.FinalDamage, attribute);
            enemy.FlashTimer = 0.08f;
            TrackArielHolyExposureDamage(enemy, attribute, result.FinalDamage);
            Debug.Log($"[CombatDamage] {runtime.Monster.DisplayName}.{skillId} -> {enemy.DisplayName}: {result.FormulaLog}; Applied={applied:0.##}, ShieldLeft={enemy.ShieldValue:0.##}, HpLeft={Mathf.Max(0f, enemy.CurrentHealth):0.##}");
            return applied;
        }

        private DamageResult ResolveArielUnitDamage(CombatUnitRuntime runtime, float baseDamage, DamageAttribute attribute, EnemyRuntime enemy, float finalMultiplier, string skillId)
        {
            return DamageCalculator.Resolve(
                baseDamage,
                attribute,
                enemy.Defenses,
                flatDefenseReduction: GetArielUnitFlatDefenseReduction(runtime, enemy, attribute),
                criticalChanceBonus: GetArielUnitCriticalChanceBonus(runtime, attribute),
                targetCriticalResistance: enemy.CriticalResistance,
                criticalDamageTakenBonus: GetArielUnitCriticalDamageTakenBonus(runtime, enemy, skillId),
                finalDamageMultiplier: enemy.DamageTakenMultiplier * Mathf.Max(0f, finalMultiplier) * GetArielUnitFinalDamageMultiplier(runtime, enemy, attribute, skillId));
        }

        private float GetArielUnitSkillBaseDamage(CombatUnitRuntime runtime, SkillDefinition skill)
        {
            if (IsSelectedCombatUnit(runtime))
            {
                return GetArielSkillBaseDamage(skill);
            }

            if (runtime == null || skill == null)
            {
                return 0f;
            }

            var effectivePower = runtime.PowerStat;
            if (runtime.ArielBlessingTimer > 0f && HasArielUnitChoice(runtime, "ariel-c-master-1"))
            {
                effectivePower *= 1.18f;
            }

            return skill.BaseDamage + effectivePower * skill.SpellPowerCoefficient;
        }

        private float GetArielUnitCooldown(CombatUnitRuntime runtime, SkillDefinition skill, float fallback, float multiplier)
        {
            var cooldown = skill != null && skill.CooldownSeconds > 0f ? skill.CooldownSeconds : fallback;
            return Mathf.Max(0.1f, cooldown * Mathf.Max(0.05f, multiplier));
        }

        private float GetArielUnitHolyDamageMultiplier(CombatUnitRuntime runtime)
        {
            if (IsSelectedCombatUnit(runtime))
            {
                return GetArielHolyDamageMultiplier();
            }

            var bonus = 0f;
            if (HasArielUnitPassive(runtime, "ariel-f"))
            {
                bonus += 0.12f + (HasArielUnitChoice(runtime, "ariel-f-trait-1") ? 0.06f : 0f);
            }

            var hasShield = IsSelectedCombatUnit(runtime) ? unitShieldValue > 0f : runtime != null && runtime.ShieldValue > 0f;
            if (HasArielUnitPassive(runtime, "ariel-g") && HasArielUnitChoice(runtime, "ariel-g-trait-3") && hasShield)
            {
                bonus += 0.10f;
            }

            if (runtime != null && runtime.ArielBlessingTimer > 0f)
            {
                if (HasArielUnitChoice(runtime, "ariel-b-trait-5"))
                {
                    bonus += 0.12f;
                }

                if (HasArielUnitPassive(runtime, "ariel-h"))
                {
                    bonus += 0.15f + (HasArielUnitChoice(runtime, "ariel-h-trait-1") ? 0.07f : 0f);
                }

                if (HasArielUnitChoice(runtime, "ariel-c-trait-5") && hasShield)
                {
                    bonus += 0.10f;
                }
            }

            if (HasArielUnitPassive(runtime, "ariel-j") && HasArielUnitArchangelShieldActive(runtime))
            {
                bonus += 0.20f + (HasArielUnitChoice(runtime, "ariel-j-trait-2") ? 0.10f : 0f);
            }

            return 1f + bonus;
        }

        private float GetArielUnitFinalDamageMultiplier(CombatUnitRuntime runtime, EnemyRuntime enemy, DamageAttribute attribute, string skillId)
        {
            if (IsSelectedCombatUnit(runtime))
            {
                return GetArielFinalDamageMultiplier(enemy, attribute, skillId);
            }

            if (!IsArielCombatUnit(runtime) || enemy == null)
            {
                return 1f;
            }

            var bonus = attribute == DamageAttribute.Holy ? GetArielUnitHolyDamageMultiplier(runtime) - 1f : 0f;
            var hasShield = IsSelectedCombatUnit(runtime) ? unitShieldValue > 0f : runtime.ShieldValue > 0f;
            if (enemy.HolyExposureTimer > 0f)
            {
                bonus += enemy.HolyExposureDamageTakenBonus;
                if (HasArielUnitPassive(runtime, "ariel-i"))
                {
                    bonus += 0.10f + (HasArielUnitChoice(runtime, "ariel-i-trait-1") ? 0.05f : 0f);
                }

                if (HasArielUnitChoice(runtime, "ariel-d-trait-5") && hasShield)
                {
                    bonus += 0.10f;
                }
            }

            return 1f + bonus;
        }

        private float GetArielUnitFlatDefenseReduction(CombatUnitRuntime runtime, EnemyRuntime enemy, DamageAttribute attribute)
        {
            if (IsSelectedCombatUnit(runtime))
            {
                return GetArielFlatDefenseReduction(enemy, attribute);
            }

            if (!IsArielCombatUnit(runtime) || enemy == null || attribute != DamageAttribute.Holy || enemy.HolyExposureTimer <= 0f)
            {
                return 0f;
            }

            return enemy.HolyExposureFlatDefenseReduction;
        }

        private float GetArielUnitCriticalDamageTakenBonus(CombatUnitRuntime runtime, EnemyRuntime enemy, string skillId)
        {
            if (IsSelectedCombatUnit(runtime))
            {
                return GetArielCriticalDamageTakenBonus(enemy, skillId);
            }

            if (!IsArielCombatUnit(runtime) || enemy == null || enemy.HolyExposureTimer <= 0f)
            {
                return 0f;
            }

            return enemy.HolyExposureCriticalDamageTakenBonus;
        }

        private float GetArielUnitCriticalChanceBonus(CombatUnitRuntime runtime, DamageAttribute attribute)
        {
            if (IsSelectedCombatUnit(runtime))
            {
                return GetArielCriticalChanceBonus(attribute);
            }

            return IsArielCombatUnit(runtime)
                && attribute == DamageAttribute.Holy
                && HasArielUnitPassive(runtime, "ariel-f")
                && HasArielUnitChoice(runtime, "ariel-f-trait-3")
                ? 0.08f
                : 0f;
        }

        private float GetArielUnitIncomingDamageAfterReduction(CombatUnitRuntime runtime, float incomingDamage)
        {
            if (IsSelectedCombatUnit(runtime))
            {
                return GetArielIncomingDamageAfterReduction(incomingDamage);
            }

            if (!IsArielCombatUnit(runtime) || runtime.ArielSanctuaryTimer <= 0f || !HasArielUnitChoice(runtime, "ariel-e-master-1"))
            {
                return incomingDamage;
            }

            return incomingDamage * 0.82f;
        }

        private float GetArielUnitActionSpeedMultiplier(CombatUnitRuntime runtime)
        {
            if (IsSelectedCombatUnit(runtime))
            {
                return GetArielActionSpeedMultiplier();
            }

            var bonus = 0f;
            if (runtime != null && runtime.ArielBlessingTimer > 0f && !HasArielUnitChoice(runtime, "ariel-c-master-1"))
            {
                bonus += 0.12f + (HasArielUnitChoice(runtime, "ariel-c-trait-2") ? 0.06f : 0f);
            }

            if (HasArielUnitPassive(runtime, "ariel-j") && runtime != null && runtime.ArielSanctuaryProclamationTimer > 0f)
            {
                bonus += 0.15f + (HasArielUnitChoice(runtime, "ariel-j-trait-1") ? 0.07f : 0f);
            }

            return 1f + bonus;
        }

        private float GetArielUnitCooldownChargeMultiplier(CombatUnitRuntime runtime)
        {
            if (IsSelectedCombatUnit(runtime))
            {
                return GetArielCooldownChargeMultiplier();
            }

            var bonus = 0f;
            if (HasArielUnitPassive(runtime, "ariel-h") && runtime != null && runtime.ArielBlessingTimer > 0f)
            {
                bonus += 0.10f + (HasArielUnitChoice(runtime, "ariel-h-trait-2") ? 0.05f : 0f);
            }

            return 1f + bonus;
        }

        private float GetArielUnitShieldMultiplier(CombatUnitRuntime runtime)
        {
            var bonus = 0f;
            if (HasArielUnitPassive(runtime, "ariel-g"))
            {
                bonus += 0.18f + (HasArielUnitChoice(runtime, "ariel-g-trait-1") ? 0.08f : 0f);
            }

            return 1f + bonus;
        }

        private float GetArielUnitArchangelCooldownMultiplier(CombatUnitRuntime runtime)
        {
            var multiplier = HasArielUnitChoice(runtime, "ariel-e-trait-3") ? 0.80f : 1f;
            if (HasArielUnitPassive(runtime, "ariel-j") && HasArielUnitChoice(runtime, "ariel-j-trait-3"))
            {
                multiplier *= 0.85f;
            }

            return multiplier;
        }

        private SkillDefinition FindArielUnitSkill(CombatUnitRuntime runtime, SkillSlot slot)
        {
            if (IsSelectedCombatUnit(runtime))
            {
                return FindSelectedSkill(slot);
            }

            if (runtime == null)
            {
                return null;
            }

            for (var i = 0; i < runtime.Skills.Count; i++)
            {
                var skillRuntime = runtime.Skills[i];
                if (skillRuntime != null && skillRuntime.Skill != null && skillRuntime.Skill.Slot == slot)
                {
                    return skillRuntime.Skill;
                }
            }

            return null;
        }

        private bool HasArielUnitChoice(CombatUnitRuntime runtime, string choiceId)
        {
            if (IsSelectedCombatUnit(runtime))
            {
                return HasChoice(choiceId);
            }

            return HasManifestedChoice(runtime, choiceId);
        }

        private bool HasArielUnitPassive(CombatUnitRuntime runtime, string passiveId)
        {
            if (IsSelectedCombatUnit(runtime))
            {
                return HasArielPassive(passiveId, string.Empty);
            }

            return HasManifestedPassive(runtime, passiveId);
        }

        private bool IsArielCombatUnit(CombatUnitRuntime runtime)
        {
            return runtime != null
                && runtime.Monster != null
                && string.Equals(runtime.Monster.MonsterId, "ariel", StringComparison.OrdinalIgnoreCase);
        }

        private bool HasArielPassive(string passiveId, string passiveName)
        {
            return IsSelectedArielMonster()
                && ((!string.IsNullOrWhiteSpace(passiveId) && chosenSkillChoiceIds.Contains(passiveId))
                    || (!string.IsNullOrWhiteSpace(passiveId) && learnedPassiveSkillIds.Contains(passiveId)));
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
