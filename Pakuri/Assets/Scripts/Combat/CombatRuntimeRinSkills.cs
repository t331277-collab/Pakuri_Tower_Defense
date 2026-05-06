using System;
using Pakuri.Data;
using UnityEngine;

namespace Pakuri.Combat
{
    public partial class CombatRuntimeController
    {
        private const float RinShatteringFistProjectileSpeed = 13f;
        private const float RinShockwaveWidth = 1.6f;
        private const float RinShockwaveKnockback = 0.6f;
        private const float RinHowlingDuration = 5f;
        private const float RinThunderGauntletBranchRadius = 3f;
        private const int RinThunderGauntletBranchInterval = 3;
        private const float RinFinishingBlowExecuteThreshold = 0.30f;
        private const float RinFinishingBlowExecuteMultiplier = 1.80f;
        private const float RinFinishingBlowKillCooldownRefund = 0.35f;
        private const float RinFinishingBlowExplosionRadius = 2.4f;
        private const float RinMapWideSkillRangePadding = 2f;
        private const float RinWaveAmplificationInternalCooldown = 3f;
        private const float RinFinisherInstinctDuration = 4f;
        private const float RinCollapseAftermathDuration = 3f;

        private float rinHowlingCooldownRemaining;
        private float rinShockwaveCooldownRemaining;
        private float rinFinishingBlowCooldownRemaining;
        private float rinCollapseStrikeCooldownRemaining;
        private float rinHowlingTimer;
        private int rinThunderGauntletHitCounter;
        private int rinWaveAmplificationPhysicalHitCount;
        private float rinWaveAmplificationCooldownRemaining;
        private float rinFinisherInstinctActionTimer;
        private float rinFinisherInstinctCritTimer;
        private float rinCollapseAftermathActionTimer;
        private float rinCollapseAftermathAttackTimer;

        private void ResetRinSkillCombatTimers()
        {
            rinHowlingCooldownRemaining = 0f;
            rinShockwaveCooldownRemaining = 0f;
            rinFinishingBlowCooldownRemaining = 0f;
            rinCollapseStrikeCooldownRemaining = 0f;
            rinHowlingTimer = 0f;
            rinThunderGauntletHitCounter = 0;
            rinWaveAmplificationPhysicalHitCount = 0;
            rinWaveAmplificationCooldownRemaining = 0f;
            rinFinisherInstinctActionTimer = 0f;
            rinFinisherInstinctCritTimer = 0f;
            rinCollapseAftermathActionTimer = 0f;
            rinCollapseAftermathAttackTimer = 0f;
        }

        private void UpdateRinSkillCooldowns()
        {
            var elapsed = Time.deltaTime * GetRinActionSpeedMultiplier();
            rinHowlingCooldownRemaining = Mathf.Max(0f, rinHowlingCooldownRemaining - elapsed);
            rinShockwaveCooldownRemaining = Mathf.Max(0f, rinShockwaveCooldownRemaining - elapsed);
            rinFinishingBlowCooldownRemaining = Mathf.Max(0f, rinFinishingBlowCooldownRemaining - elapsed);
            rinCollapseStrikeCooldownRemaining = Mathf.Max(0f, rinCollapseStrikeCooldownRemaining - elapsed);
            rinWaveAmplificationCooldownRemaining = Mathf.Max(0f, rinWaveAmplificationCooldownRemaining - Time.deltaTime);
        }

        private void UpdateRinSkillEffects()
        {
            if (!IsSelectedRinMonster())
            {
                return;
            }

            rinHowlingTimer = Mathf.Max(0f, rinHowlingTimer - Time.deltaTime);
            rinFinisherInstinctActionTimer = Mathf.Max(0f, rinFinisherInstinctActionTimer - Time.deltaTime);
            rinFinisherInstinctCritTimer = Mathf.Max(0f, rinFinisherInstinctCritTimer - Time.deltaTime);
            rinCollapseAftermathActionTimer = Mathf.Max(0f, rinCollapseAftermathActionTimer - Time.deltaTime);
            rinCollapseAftermathAttackTimer = Mathf.Max(0f, rinCollapseAftermathAttackTimer - Time.deltaTime);
        }

        private bool TryTriggerRinAutomaticSkills()
        {
            if (!IsSelectedRinMonster())
            {
                return false;
            }

            var castAny = false;
            castAny |= TryCastRinHowling();
            castAny |= TryCastRinShockwave();
            castAny |= TryCastRinFinishingBlow();
            castAny |= TryCastRinCollapseStrike();

            if (!castAny)
            {
                statusLabel = $"{selectedMonsterName}: no Rin active skill is ready.";
            }

            return true;
        }

        private void FireManualRinShatteringFist(Vector3 baseDirection)
        {
            var skill = FindSelectedSkill(SkillSlot.A);
            if (skill == null || !HasLearnedActive(SkillSlot.A) || eveAnchor == null || projectileRoot == null)
            {
                return;
            }

            baseDirection.z = 0f;
            if (baseDirection.sqrMagnitude < 0.01f)
            {
                baseDirection = Vector3.right;
            }

            baseDirection.Normalize();

            var damageMultiplier = 1f;
            if (HasChoice("rin-a-trait-1"))
            {
                damageMultiplier *= 1.25f;
            }

            var pierce = 0;
            if (HasChoice("rin-a-trait-4"))
            {
                pierce += 1;
                damageMultiplier *= 0.90f;
            }

            var shotIntervalMultiplier = 1f;
            if (HasChoice("rin-a-master-1"))
            {
                damageMultiplier *= 1.12f;
                shotIntervalMultiplier *= 0.82f;
            }

            nextProjectileSequence += 1;
            var projectileObject = new GameObject($"ShatteringFist_{nextProjectileSequence:00}");
            projectileObject.transform.SetParent(projectileRoot, false);
            projectileObject.transform.position = eveAnchor.position + baseDirection * 0.2f;
            projectileObject.transform.localScale = new Vector3(projectileHitRadiusConfigured, projectileHitRadiusConfigured, 1f);

            var renderer = projectileObject.AddComponent<SpriteRenderer>();
            renderer.sprite = selectedProjectileSprite != null ? selectedProjectileSprite : GetSharedSprite();
            renderer.color = Color.white;
            renderer.sortingOrder = 25;

            var range = skill.Range > 0f ? skill.Range : 7.5f;
            var lifetime = range / RinShatteringFistProjectileSpeed;
            projectiles.Add(new ProjectileRuntime
            {
                GameObject = projectileObject,
                Transform = projectileObject.transform,
                Renderer = renderer,
                Direction = baseDirection,
                Speed = RinShatteringFistProjectileSpeed,
                RemainingLifetime = Mathf.Max(0.1f, lifetime),
                HitRadius = projectileHitRadiusConfigured,
                BaseDamage = GetRinSkillBaseDamage(skill) * damageMultiplier,
                Attribute = DamageAttribute.Physical,
                SkillId = "rin-a",
                RemainingPierce = Mathf.Max(0, pierce)
            });

            currentShotsRemaining -= 1;
            shotCooldown = Mathf.Max(0.05f, GetRinShatteringFistShotInterval() * shotIntervalMultiplier);
            if (currentShotsRemaining <= 0)
            {
                currentShotsRemaining = 0;
                reloadRemaining = GetRinShatteringFistReloadSeconds();
            }

            statusLabel = $"Shattering Fist fired toward ({currentAttackPoint.x:0.0}, {currentAttackPoint.y:0.0}).";
        }

        private bool TryCastRinHowling()
        {
            var skill = FindSelectedSkill(SkillSlot.B);
            if (skill == null || !HasLearnedActive(SkillSlot.B) || rinHowlingCooldownRemaining > 0f)
            {
                return false;
            }

            var duration = RinHowlingDuration;
            if (HasChoice("rin-b-trait-1"))
            {
                duration *= 1.25f;
            }

            if (HasChoice("rin-b-master-1"))
            {
                duration *= 1.20f;
            }

            rinHowlingTimer = Mathf.Max(rinHowlingTimer, duration);
            rinHowlingCooldownRemaining = GetRinCooldown(skill, 12f, HasChoice("rin-b-trait-3") ? 0.80f : 1f);
            if (HasRinBattleResonance() && HasChoice("rin-g-trait-3") && reloadRemaining > 0f)
            {
                reloadRemaining *= 0.75f;
            }

            var effect = CreateCircleEffect("RinHowling", eveAnchor.position, 2.2f, 0.45f);
            effect.SkillId = "rin-b";
            if (effect.Renderer != null)
            {
                effect.Renderer.color = new Color(1f, 0.82f, 0.42f, 0.42f);
                effect.Renderer.sortingOrder = 24;
            }

            skillEffects.Add(effect);
            statusLabel = $"Howling active for {duration:0.#}s.";
            return true;
        }

        private bool TryCastRinShockwave()
        {
            var skill = FindSelectedSkill(SkillSlot.C);
            if (skill == null || !HasLearnedActive(SkillSlot.C) || rinShockwaveCooldownRemaining > 0f || eveAnchor == null)
            {
                return false;
            }

            var mapWideRange = GetRinMapWideSkillRange();
            var target = FindNearestEnemy(eveAnchor.position, mapWideRange);
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
            var length = mapWideRange;
            var width = RinShockwaveWidth;
            var damageMultiplier = 1f;
            var knockback = RinShockwaveKnockback;

            if (HasChoice("rin-c-trait-1"))
            {
                damageMultiplier *= 1.25f;
            }

            if (HasChoice("rin-c-trait-2"))
            {
                width *= 1.25f;
            }

            if (HasChoice("rin-c-trait-3"))
            {
                knockback *= 1.40f;
            }

            if (HasChoice("rin-c-master-1"))
            {
                width *= 0.75f;
                damageMultiplier *= 1.80f;
                knockback *= 1.50f;
            }

            if (HasChoice("rin-c-master-2"))
            {
                width *= 1.60f;
                damageMultiplier *= 1.25f;
            }

            var effect = CreateLineEffect("RinShockwave", eveAnchor.position, direction, length, width, 0.25f);
            effect.SkillId = "rin-c";
            if (effect.Renderer != null)
            {
                effect.Renderer.color = new Color(1f, 0.88f, 0.56f, 0.68f);
                effect.Renderer.sortingOrder = 24;
            }

            skillEffects.Add(effect);

            var hitCount = 0;
            var damage = GetRinSkillBaseDamage(skill) * damageMultiplier;
            for (var i = 0; i < enemies.Count; i++)
            {
                var enemy = enemies[i];
                if (enemy == null || enemy.CurrentHealth <= 0f || enemy.Transform == null || !IsPointInsideBeam(enemy.Transform.position, effect))
                {
                    continue;
                }

                var physicalDamage = ApplyRinSkillDamage(enemy, damage, DamageAttribute.Physical, 1f, "rin-c");
                ApplyRinKnockback(enemy, direction, knockback);
                if (HasChoice("rin-c-master-1"))
                {
                    ApplyRinAdditionalDamage(enemy, physicalDamage, 0.60f, DamageAttribute.Lightning, "rin-c-master-1");
                }

                if (HasChoice("rin-c-master-2"))
                {
                    ApplyRinSlow(enemy, 0.80f, 1.5f);
                }

                hitCount += 1;
            }

            if (hitCount > 0 && HasChoice("rin-c-trait-5") && reloadRemaining > 0f)
            {
                reloadRemaining = Mathf.Max(0f, reloadRemaining - hitCount * 0.25f);
            }

            rinShockwaveCooldownRemaining = GetRinCooldown(skill, 5.5f, HasChoice("rin-c-trait-4") ? 0.80f : 1f);
            statusLabel = $"Shockwave hit {hitCount} enemy(s).";
            return hitCount > 0;
        }

        private bool TryCastRinFinishingBlow()
        {
            var skill = FindSelectedSkill(SkillSlot.D);
            if (skill == null || !HasLearnedActive(SkillSlot.D) || rinFinishingBlowCooldownRemaining > 0f || eveAnchor == null)
            {
                return false;
            }

            var range = GetRinMapWideSkillRange();
            var target = FindRinFinishingBlowTarget(range);
            if (target == null)
            {
                return false;
            }

            var executeThreshold = GetRinFinishingBlowExecuteThreshold();
            var healthRatio = target.MaxHealth > 0f ? target.CurrentHealth / target.MaxHealth : 1f;
            var executeTarget = healthRatio <= Mathf.Clamp01(executeThreshold);
            if (!executeTarget)
            {
                return false;
            }

            var damageMultiplier = 1f;
            if (HasChoice("rin-d-trait-1"))
            {
                damageMultiplier *= 1.30f;
            }

            if (executeTarget)
            {
                damageMultiplier *= RinFinishingBlowExecuteMultiplier;
            }

            if (HasChoice("rin-d-trait-5") && target.IsBoss)
            {
                damageMultiplier *= 1.25f;
            }

            if (HasChoice("rin-d-master-2"))
            {
                damageMultiplier *= 1.90f;
            }

            var damage = GetRinSkillBaseDamage(skill) * damageMultiplier;
            var wasAlive = target.CurrentHealth > 0f;
            var physicalDamage = ApplyRinSkillDamage(target, damage, DamageAttribute.Physical, 1f, "rin-d", executeTarget);
            CreateRinFinishingBlowHitEffect(target);
            if (HasChoice("rin-d-master-2"))
            {
                ApplyRinAdditionalDamage(target, physicalDamage, 0.70f, DamageAttribute.Darkness, "rin-d-master-2");
            }

            rinFinishingBlowCooldownRemaining = GetRinCooldown(skill, 9f, HasChoice("rin-d-master-2") ? 1.25f : 1f);
            if (executeTarget && HasRinFinisherInstinct() && HasChoice("rin-i-trait-3"))
            {
                var collapseCooldown = GetRinCooldown(FindSelectedSkill(SkillSlot.E), 8f, HasChoice("rin-e-trait-3") ? 0.80f : 1f);
                rinCollapseStrikeCooldownRemaining = Mathf.Max(0f, rinCollapseStrikeCooldownRemaining - collapseCooldown * 0.12f);
            }

            var killed = wasAlive && target.CurrentHealth <= 0f;
            if (killed)
            {
                HandleRinFinishingBlowKill(target, physicalDamage);
            }

            statusLabel = killed
                ? $"Finishing Blow executed {target.DisplayName}."
                : $"Finishing Blow hit {target.DisplayName}.";
            return true;
        }

        private bool TryCastRinCollapseStrike()
        {
            var skill = FindSelectedSkill(SkillSlot.E);
            if (skill == null || !HasLearnedActive(SkillSlot.E) || rinCollapseStrikeCooldownRemaining > 0f || eveAnchor == null)
            {
                return false;
            }

            var range = GetRinMapWideSkillRange();
            var target = FindNearestEnemy(eveAnchor.position, range);
            if (target == null)
            {
                return false;
            }

            var radius = skill.Radius > 0f ? skill.Radius : 2.4f;
            var damageMultiplier = 1f;
            if (HasChoice("rin-e-trait-1"))
            {
                damageMultiplier *= 1.30f;
            }

            if (HasChoice("rin-e-trait-2"))
            {
                radius *= 1.25f;
            }

            if (HasChoice("rin-e-master-1"))
            {
                radius *= 0.80f;
                damageMultiplier *= 2.00f;
            }

            if (HasChoice("rin-e-master-2"))
            {
                radius *= 1.50f;
                damageMultiplier *= 1.35f;
            }

            var center = target.Transform.position;
            var hitCount = 0;
            var damage = GetRinSkillBaseDamage(skill) * damageMultiplier;
            for (var i = 0; i < enemies.Count; i++)
            {
                var enemy = enemies[i];
                if (enemy == null
                    || enemy.CurrentHealth <= 0f
                    || enemy.Transform == null
                    || Vector2.Distance(enemy.Transform.position, center) > radius + GetEnemyHitRadius(enemy))
                {
                    continue;
                }

                var targetMultiplier = enemy == target && HasChoice("rin-e-trait-4") ? 1.50f : 1f;
                var physicalDamage = ApplyRinSkillDamage(enemy, damage, DamageAttribute.Physical, targetMultiplier, "rin-e");
                if (HasRinCollapseAftermath())
                {
                    ApplyRinPhysicalDefenseReduction(enemy);
                }

                if (enemy == target && HasChoice("rin-e-master-1"))
                {
                    ApplyRinAdditionalDamage(enemy, physicalDamage, 1.00f, DamageAttribute.Fire, "rin-e-master-1");
                }

                if (HasChoice("rin-e-master-2"))
                {
                    ApplyRinSlow(enemy, 0.75f, 2f);
                    ApplyRinAdditionalDamage(enemy, physicalDamage, 0.45f, DamageAttribute.Darkness, "rin-e-master-2");
                }

                hitCount += 1;
            }

            if (hitCount >= 3 && HasChoice("rin-e-trait-5"))
            {
                rinHowlingCooldownRemaining = Mathf.Max(0f, rinHowlingCooldownRemaining - GetRinCooldown(FindSelectedSkill(SkillSlot.B), 12f, 1f) * 0.20f);
            }

            if (hitCount >= 3 && HasRinCollapseAftermath())
            {
                rinCollapseAftermathActionTimer = Mathf.Max(rinCollapseAftermathActionTimer, RinCollapseAftermathDuration);
                if (HasChoice("rin-j-trait-2"))
                {
                    rinCollapseAftermathAttackTimer = Mathf.Max(rinCollapseAftermathAttackTimer, RinCollapseAftermathDuration);
                }
            }

            var effect = CreateCircleEffect("RinCollapseStrike", center, radius, 0.35f);
            effect.SkillId = "rin-e";
            if (effect.Renderer != null)
            {
                effect.Renderer.color = new Color(1f, 0.56f, 0.32f, 0.45f);
                effect.Renderer.sortingOrder = 24;
            }

            skillEffects.Add(effect);
            rinCollapseStrikeCooldownRemaining = GetRinCooldown(skill, 8f, HasChoice("rin-e-trait-3") ? 0.80f : 1f);
            statusLabel = $"Collapse Strike hit {hitCount} enemy(s).";
            return hitCount > 0;
        }

        private void HandleRinProjectileHit(ProjectileRuntime projectile, EnemyRuntime enemy, float physicalDamage)
        {
            if (!IsSelectedRinMonster()
                || projectile == null
                || enemy == null
                || physicalDamage <= 0f
                || !string.Equals(projectile.SkillId, "rin-a", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            TrackRinPhysicalDamageHit(physicalDamage, "rin-a");
            ApplyRinHowlingDarkAdditionalDamage(enemy, physicalDamage, "rin-a-howling");
            if (!HasChoice("rin-a-master-2"))
            {
                return;
            }

            ApplyRinAdditionalDamage(enemy, physicalDamage, 0.40f, DamageAttribute.Lightning, "rin-a-master-2");
            rinThunderGauntletHitCounter += 1;
            if (rinThunderGauntletHitCounter % RinThunderGauntletBranchInterval != 0)
            {
                return;
            }

            var branchCount = 0;
            while (branchCount < 2)
            {
                var branchTarget = FindNearestRinBranchTarget(enemy, RinThunderGauntletBranchRadius, projectile);
                if (branchTarget == null)
                {
                    break;
                }

                ApplyRinAdditionalDamage(branchTarget, physicalDamage, 0.40f, DamageAttribute.Lightning, "rin-a-chain");
                CreateEveArcBranchLine(enemy.Transform.position, branchTarget.Transform.position);
                projectile.HitEnemies.Add(branchTarget);
                branchCount += 1;
            }
        }

        private float ApplyRinSkillDamage(EnemyRuntime enemy, float baseDamage, DamageAttribute attribute, float finalMultiplier, string skillId, bool executeTarget = false)
        {
            if (enemy == null || enemy.CurrentHealth <= 0f)
            {
                return 0f;
            }

            var result = DamageCalculator.Resolve(
                baseDamage,
                attribute,
                enemy.Defenses,
                percentDefenseReductions: GetRinPercentDefenseReductions(enemy, attribute),
                criticalChanceBonus: GetRinCriticalChanceBonus(enemy, attribute, skillId, executeTarget),
                criticalMultiplierBonus: GetRinCriticalMultiplierBonus(enemy, attribute, skillId),
                targetCriticalResistance: enemy.CriticalResistance,
                finalDamageMultiplier: enemy.DamageTakenMultiplier * Mathf.Max(0f, finalMultiplier) * GetRinFinalDamageMultiplier(enemy, attribute, skillId));
            var wasAlive = enemy.CurrentHealth > 0f;
            var applied = ApplyDamageToEnemy(enemy, result.FinalDamage, attribute);
            enemy.FlashTimer = 0.08f;
            if (attribute == DamageAttribute.Physical)
            {
                TrackRinPhysicalDamageHit(applied, skillId);
                ApplyRinAmbidextrousFollowup(enemy, applied, skillId);
            }

            ApplyRinHowlingDarkAdditionalDamage(enemy, applied, $"{skillId}-howling");
            HandleRinEnemyKilledByDamage(enemy, wasAlive);
            Debug.Log($"[CombatDamage] Rin.{skillId} -> {enemy.DisplayName}: {result.FormulaLog}; Applied={applied:0.##}, ShieldLeft={enemy.ShieldValue:0.##}, HpLeft={Mathf.Max(0f, enemy.CurrentHealth):0.##}");
            return applied;
        }

        private float ApplyRinAdditionalDamage(EnemyRuntime enemy, float physicalDamage, float multiplier, DamageAttribute attribute, string skillId, bool spawnPopup = true)
        {
            if (enemy == null || enemy.CurrentHealth <= 0f || physicalDamage <= 0f || multiplier <= 0f)
            {
                return 0f;
            }

            var result = DamageCalculator.Resolve(
                physicalDamage * multiplier,
                attribute,
                enemy.Defenses,
                percentDefenseReductions: GetRinPercentDefenseReductions(enemy, attribute),
                targetCriticalResistance: enemy.CriticalResistance,
                finalDamageMultiplier: enemy.DamageTakenMultiplier);
            var wasAlive = enemy.CurrentHealth > 0f;
            var applied = ApplyDamageToEnemy(enemy, result.FinalDamage, attribute, spawnPopup);
            enemy.FlashTimer = 0.08f;
            HandleRinEnemyKilledByDamage(enemy, wasAlive);
            Debug.Log($"[CombatDamage] Rin.{skillId} -> {enemy.DisplayName}: {result.FormulaLog}; Applied={applied:0.##}, ShieldLeft={enemy.ShieldValue:0.##}, HpLeft={Mathf.Max(0f, enemy.CurrentHealth):0.##}");
            return applied;
        }

        private void ApplyRinHowlingDarkAdditionalDamage(EnemyRuntime enemy, float physicalDamage, string skillId)
        {
            if (rinHowlingTimer <= 0f || !HasChoice("rin-b-master-2"))
            {
                return;
            }

            ApplyRinAdditionalDamage(enemy, physicalDamage, 0.25f, DamageAttribute.Darkness, skillId);
        }

        private void HandleRinFinishingBlowKill(EnemyRuntime target, float physicalDamage)
        {
            if (HasRinFinisherInstinct())
            {
                rinFinisherInstinctActionTimer = Mathf.Max(rinFinisherInstinctActionTimer, RinFinisherInstinctDuration);
                if (HasChoice("rin-i-trait-2"))
                {
                    rinFinisherInstinctCritTimer = Mathf.Max(rinFinisherInstinctCritTimer, RinFinisherInstinctDuration);
                }
            }

            if (HasChoice("rin-d-master-1"))
            {
                rinFinishingBlowCooldownRemaining = 0f;
                ApplyRinAreaAdditionalDamage(target.Transform.position, RinFinishingBlowExplosionRadius, physicalDamage, 0.90f, DamageAttribute.Holy, "rin-d-master-1");
                return;
            }

            var refund = RinFinishingBlowKillCooldownRefund + (HasChoice("rin-d-trait-3") ? 0.20f : 0f);
            var baseCooldown = GetRinCooldown(FindSelectedSkill(SkillSlot.D), 9f, HasChoice("rin-d-master-2") ? 1.25f : 1f);
            rinFinishingBlowCooldownRemaining = Mathf.Max(0f, rinFinishingBlowCooldownRemaining - baseCooldown * refund);
        }

        private void ApplyRinAmbidextrousFollowup(EnemyRuntime enemy, float sourcePhysicalDamage, string sourceSkillId)
        {
            if (!HasRinAmbidextrous()
                || enemy == null
                || sourcePhysicalDamage <= 0f
                || !IsRinAmbidextrousEligibleSkill(sourceSkillId))
            {
                return;
            }

            var multiplier = 0.35f + (HasChoice("rin-f-trait-2") ? 0.15f : 0f);
            var followupDamage = ApplyRinAdditionalDamage(enemy, sourcePhysicalDamage, multiplier, DamageAttribute.Physical, "rin-f", false);
            var lightningDamage = 0f;
            if (HasChoice("rin-f-trait-3"))
            {
                lightningDamage = ApplyRinAdditionalDamage(enemy, followupDamage, 0.30f, DamageAttribute.Lightning, "rin-f-trait-3", false);
            }

            CreateRinAmbidextrousFollowupEffect(enemy, followupDamage, lightningDamage);
        }

        private bool IsRinAmbidextrousEligibleSkill(string sourceSkillId)
        {
            return string.Equals(sourceSkillId, "rin-c", StringComparison.OrdinalIgnoreCase)
                || string.Equals(sourceSkillId, "rin-d", StringComparison.OrdinalIgnoreCase)
                || string.Equals(sourceSkillId, "rin-e", StringComparison.OrdinalIgnoreCase);
        }

        private void CreateRinAmbidextrousFollowupEffect(EnemyRuntime enemy, float physicalDamage, float lightningDamage)
        {
            if (enemy == null || enemy.Transform == null || physicalDamage <= 0f)
            {
                return;
            }

            var radius = Mathf.Max(0.55f, GetEnemyHitRadius(enemy) + 0.18f);
            var effect = CreateCircleEffect("RinAmbidextrousFollowup", enemy.Transform.position, radius, 0.22f);
            effect.SkillId = "rin-f";
            if (effect.Renderer != null)
            {
                effect.Renderer.color = new Color(1f, 1f, 1f, 0.72f);
                effect.Renderer.sortingOrder = 27;
            }

            skillEffects.Add(effect);

            var popupText = lightningDamage > 0f
                ? $"{FormatDamagePopupTerm(physicalDamage, DamageAttribute.Physical)} + {FormatDamagePopupTerm(lightningDamage, DamageAttribute.Lightning)}"
                : FormatDamagePopupTerm(physicalDamage, DamageAttribute.Physical);
            SpawnDamagePopupForEnemy(enemy, popupText);
        }

        private void TrackRinPhysicalDamageHit(float appliedDamage, string sourceSkillId)
        {
            if (!HasRinWaveAmplification()
                || appliedDamage <= 0f
                || rinWaveAmplificationCooldownRemaining > 0f
                || string.Equals(sourceSkillId, "rin-h", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            rinWaveAmplificationPhysicalHitCount += 1;
            var requiredHits = HasChoice("rin-h-trait-1") ? 8 : 10;
            if (rinWaveAmplificationPhysicalHitCount < requiredHits)
            {
                return;
            }

            rinWaveAmplificationPhysicalHitCount = 0;
            TryCastRinWaveAmplificationShockwave();
        }

        private bool TryCastRinWaveAmplificationShockwave()
        {
            var skill = FindSelectedSkill(SkillSlot.C);
            if (skill == null || !HasLearnedActive(SkillSlot.C) || eveAnchor == null)
            {
                return false;
            }

            var mapWideRange = GetRinMapWideSkillRange();
            var target = FindNearestEnemy(eveAnchor.position, mapWideRange);
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
            var effect = CreateLineEffect("RinWaveAmplification", eveAnchor.position, direction, mapWideRange, RinShockwaveWidth, 0.25f);
            effect.SkillId = "rin-h";
            if (effect.Renderer != null)
            {
                effect.Renderer.color = new Color(0.62f, 0.86f, 1f, 0.58f);
                effect.Renderer.sortingOrder = 24;
            }

            skillEffects.Add(effect);
            var damageMultiplier = HasChoice("rin-h-trait-2") ? 0.95f : 0.75f;
            var hitCount = 0;
            var damage = GetRinSkillBaseDamage(skill) * damageMultiplier;
            for (var i = 0; i < enemies.Count; i++)
            {
                var enemy = enemies[i];
                if (enemy == null || enemy.CurrentHealth <= 0f || enemy.Transform == null || !IsPointInsideBeam(enemy.Transform.position, effect))
                {
                    continue;
                }

                var physicalDamage = ApplyRinSkillDamage(enemy, damage, DamageAttribute.Physical, 1f, "rin-h");
                ApplyRinKnockback(enemy, direction, RinShockwaveKnockback);
                if (HasChoice("rin-h-trait-3"))
                {
                    ApplyRinAdditionalDamage(enemy, physicalDamage, 0.30f, DamageAttribute.Lightning, "rin-h-trait-3");
                }

                hitCount += 1;
            }

            rinWaveAmplificationCooldownRemaining = RinWaveAmplificationInternalCooldown;
            statusLabel = $"Wave Amplification auto Shockwave hit {hitCount} enemy(s).";
            return hitCount > 0;
        }

        private void ApplyRinPhysicalDefenseReduction(EnemyRuntime enemy)
        {
            if (enemy == null)
            {
                return;
            }

            enemy.RinPhysicalDefenseReduction = Mathf.Max(enemy.RinPhysicalDefenseReduction, HasChoice("rin-j-trait-1") ? 0.26f : 0.18f);
            enemy.RinPhysicalDefenseReductionTimer = Mathf.Max(enemy.RinPhysicalDefenseReductionTimer, 4f);
        }

        private void HandleRinEnemyKilledByDamage(EnemyRuntime enemy, bool wasAlive)
        {
            if (!IsSelectedRinMonster() || enemy == null || !wasAlive || enemy.CurrentHealth > 0f)
            {
                return;
            }

            if (HasRinCollapseAftermath() && HasChoice("rin-j-trait-3") && enemy.RinPhysicalDefenseReductionTimer > 0f)
            {
                var finishingCooldown = GetRinCooldown(FindSelectedSkill(SkillSlot.D), 9f, HasChoice("rin-d-master-2") ? 1.25f : 1f);
                rinFinishingBlowCooldownRemaining = Mathf.Max(0f, rinFinishingBlowCooldownRemaining - finishingCooldown * 0.15f);
            }
        }

        private float[] GetRinPercentDefenseReductions(EnemyRuntime enemy, DamageAttribute attribute)
        {
            if (!IsSelectedRinMonster()
                || enemy == null
                || attribute != DamageAttribute.Physical
                || enemy.RinPhysicalDefenseReductionTimer <= 0f
                || enemy.RinPhysicalDefenseReduction <= 0f)
            {
                return null;
            }

            return new[] { enemy.RinPhysicalDefenseReduction };
        }

        private void ApplyRinAreaAdditionalDamage(Vector3 center, float radius, float physicalDamage, float multiplier, DamageAttribute attribute, string skillId)
        {
            for (var i = 0; i < enemies.Count; i++)
            {
                var enemy = enemies[i];
                if (enemy == null
                    || enemy.CurrentHealth <= 0f
                    || enemy.Transform == null
                    || Vector2.Distance(enemy.Transform.position, center) > radius + GetEnemyHitRadius(enemy))
                {
                    continue;
                }

                ApplyRinAdditionalDamage(enemy, physicalDamage, multiplier, attribute, skillId);
            }

            var effect = CreateCircleEffect("RinFinishingExplosion", center, radius, 0.35f);
            effect.SkillId = skillId;
            if (effect.Renderer != null)
            {
                effect.Renderer.color = new Color(1f, 0.95f, 0.72f, 0.48f);
                effect.Renderer.sortingOrder = 25;
            }

            skillEffects.Add(effect);
        }

        private EnemyRuntime FindNearestRinBranchTarget(EnemyRuntime sourceEnemy, float radius, ProjectileRuntime projectile)
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

        private EnemyRuntime FindRinFinishingBlowTarget(float range)
        {
            EnemyRuntime executeTarget = null;
            var executeRatio = float.MaxValue;
            var threshold = GetRinFinishingBlowExecuteThreshold();

            for (var i = 0; i < enemies.Count; i++)
            {
                var enemy = enemies[i];
                if (enemy == null || enemy.CurrentHealth <= 0f || enemy.Transform == null)
                {
                    continue;
                }

                var distance = Vector2.Distance(eveAnchor.position, enemy.Transform.position);
                if (distance > range)
                {
                    continue;
                }

                var ratio = enemy.MaxHealth > 0f ? enemy.CurrentHealth / enemy.MaxHealth : 1f;
                if (ratio <= Mathf.Clamp01(threshold) && ratio < executeRatio)
                {
                    executeTarget = enemy;
                    executeRatio = ratio;
                }
            }

            return executeTarget;
        }

        private float GetRinFinishingBlowExecuteThreshold()
        {
            var threshold = RinFinishingBlowExecuteThreshold;
            if (HasChoice("rin-d-trait-2"))
            {
                threshold += 0.10f;
            }

            if (HasChoice("rin-d-master-2"))
            {
                threshold -= 0.10f;
            }

            return Mathf.Clamp01(threshold);
        }

        private void CreateRinFinishingBlowHitEffect(EnemyRuntime target)
        {
            if (target == null || target.Transform == null)
            {
                return;
            }

            var radius = Mathf.Max(0.85f, GetEnemyHitRadius(target) + 0.35f);
            var effect = CreateCircleEffect("RinFinishingBlowHit", target.Transform.position, radius, 0.22f);
            effect.SkillId = "rin-d";
            if (effect.Renderer != null)
            {
                effect.Renderer.color = new Color(1f, 0.38f, 0.24f, 0.62f);
                effect.Renderer.sortingOrder = 26;
            }

            skillEffects.Add(effect);
        }

        private void ApplyRinKnockback(EnemyRuntime enemy, Vector3 direction, float distance)
        {
            if (enemy == null || enemy.Transform == null || distance <= 0f)
            {
                return;
            }

            var position = enemy.Transform.position + direction.normalized * distance;
            position.x = Mathf.Clamp(position.x, 0f, fieldSize.x);
            position.y = Mathf.Clamp(position.y, BattlefieldMinY, BattlefieldMaxY);
            position.z = enemy.Transform.position.z;
            enemy.Transform.position = position;
        }

        private void ApplyRinSlow(EnemyRuntime enemy, float multiplier, float duration)
        {
            if (enemy == null)
            {
                return;
            }

            enemy.SlowMultiplier = Mathf.Min(enemy.SlowMultiplier, Mathf.Clamp(multiplier, 0.1f, 1f));
            enemy.SlowTimer = Mathf.Max(enemy.SlowTimer, duration);
        }

        private float GetRinSkillBaseDamage(SkillDefinition skill)
        {
            if (skill == null)
            {
                return 0f;
            }

            var effectivePower = powerStatConfigured;
            if (rinHowlingTimer > 0f && HasRinBattleResonance())
            {
                effectivePower *= 1f + 0.14f + (HasChoice("rin-g-trait-1") ? 0.08f : 0f);
            }

            if (rinHowlingTimer > 0f && HasChoice("rin-b-trait-4"))
            {
                effectivePower *= 1.15f;
            }

            if (rinCollapseAftermathAttackTimer > 0f && HasChoice("rin-j-trait-2"))
            {
                effectivePower *= 1.15f;
            }

            return skill.BaseDamage + effectivePower * skill.AttackPowerCoefficient;
        }

        private int GetRinShatteringFistMagazineCapacity()
        {
            var skill = FindSelectedSkill(SkillSlot.A);
            var capacity = skill != null && skill.MagazineCapacity > 0 ? skill.MagazineCapacity : magazineCapacityConfigured;
            if (HasChoice("rin-a-trait-2"))
            {
                capacity += 4;
            }

            if (HasChoice("rin-a-master-1"))
            {
                capacity += 6;
            }

            return Mathf.Max(1, capacity);
        }

        private float GetRinShatteringFistReloadSeconds()
        {
            var skill = FindSelectedSkill(SkillSlot.A);
            var reload = skill != null && skill.ReloadSeconds > 0f ? skill.ReloadSeconds : reloadDurationConfigured;
            if (HasChoice("rin-a-trait-3"))
            {
                reload *= SpeedBonusToIntervalMultiplier(0.25f);
            }

            return Mathf.Max(0.25f, reload);
        }

        private float GetRinShatteringFistShotInterval()
        {
            var skill = FindSelectedSkill(SkillSlot.A);
            return Mathf.Max(0.05f, skill != null && skill.ShotIntervalSeconds > 0f ? skill.ShotIntervalSeconds : shotIntervalConfigured);
        }

        private float GetRinCooldown(SkillDefinition skill, float fallback, float multiplier)
        {
            var cooldown = skill != null && skill.CooldownSeconds > 0f ? skill.CooldownSeconds : fallback;
            return Mathf.Max(0.1f, cooldown * Mathf.Max(0.05f, multiplier));
        }

        private float GetRinMapWideSkillRange()
        {
            var width = Mathf.Max(fieldSize.x, EnemySpawnX);
            var height = Mathf.Max(fieldSize.y, BattlefieldMaxY);
            return Mathf.Sqrt(width * width + height * height) + RinMapWideSkillRangePadding;
        }

        private float GetRinActionSpeedMultiplier()
        {
            if (rinHowlingTimer <= 0f)
            {
                var inactiveBonus = 0f;
                if (rinFinisherInstinctActionTimer > 0f)
                {
                    inactiveBonus += 0.10f;
                }

                if (rinCollapseAftermathActionTimer > 0f)
                {
                    inactiveBonus += 0.12f;
                }

                return 1f + inactiveBonus;
            }

            var bonus = 0.20f;
            if (HasRinBattleResonance())
            {
                bonus += 0.08f;
            }

            if (HasChoice("rin-b-trait-2"))
            {
                bonus += 0.10f;
            }

            if (HasChoice("rin-b-master-1"))
            {
                bonus += 0.15f;
            }

            if (HasChoice("rin-b-master-2"))
            {
                bonus -= 0.05f;
            }

            if (rinFinisherInstinctActionTimer > 0f)
            {
                bonus += 0.10f;
            }

            if (rinCollapseAftermathActionTimer > 0f)
            {
                bonus += 0.12f;
            }

            return Mathf.Max(0.1f, 1f + bonus);
        }

        private float GetRinFinalDamageMultiplier(EnemyRuntime enemy, DamageAttribute attribute, string skillId)
        {
            if (!IsSelectedRinMonster() || enemy == null)
            {
                return 1f;
            }

            var bonus = 0f;
            if (attribute == DamageAttribute.Physical && HasRinAmbidextrous())
            {
                bonus += 0.12f + (HasChoice("rin-f-trait-1") ? 0.06f : 0f);
            }

            if (rinHowlingTimer > 0f && attribute == DamageAttribute.Physical && HasChoice("rin-b-master-1"))
            {
                bonus += 0.18f;
            }

            if (HasRinFinisherInstinct() && IsRinLowHealthTarget(enemy))
            {
                bonus += 0.16f + (HasChoice("rin-i-trait-1") ? 0.08f : 0f);
            }

            return 1f + bonus;
        }

        private float GetRinCriticalChanceBonus(EnemyRuntime enemy, DamageAttribute attribute, string skillId, bool executeTarget = false)
        {
            if (!IsSelectedRinMonster())
            {
                return 0f;
            }

            var bonus = 0f;
            if (attribute == DamageAttribute.Physical && string.Equals(skillId, "rin-a", StringComparison.OrdinalIgnoreCase) && HasChoice("rin-a-trait-5"))
            {
                bonus += 0.10f;
            }

            if (rinHowlingTimer > 0f && attribute == DamageAttribute.Physical && HasChoice("rin-b-trait-5"))
            {
                bonus += 0.08f;
            }

            if (rinHowlingTimer > 0f && attribute == DamageAttribute.Physical && HasRinBattleResonance() && HasChoice("rin-g-trait-2"))
            {
                bonus += 0.06f;
            }

            if (executeTarget && string.Equals(skillId, "rin-d", StringComparison.OrdinalIgnoreCase) && HasChoice("rin-d-master-1"))
            {
                bonus += 0.50f;
            }

            return bonus;
        }

        private float GetRinCriticalMultiplierBonus(EnemyRuntime enemy, DamageAttribute attribute, string skillId)
        {
            if (!IsSelectedRinMonster() || attribute != DamageAttribute.Physical)
            {
                return 0f;
            }

            var bonus = 0f;
            if (string.Equals(skillId, "rin-a", StringComparison.OrdinalIgnoreCase) && HasChoice("rin-a-trait-5"))
            {
                bonus += 0.25f;
            }

            if (string.Equals(skillId, "rin-d", StringComparison.OrdinalIgnoreCase) && HasChoice("rin-d-trait-4"))
            {
                bonus += 0.40f;
            }

            if (rinFinisherInstinctCritTimer > 0f && HasChoice("rin-i-trait-2"))
            {
                bonus += 0.25f;
            }

            return bonus;
        }

        private bool IsRinLowHealthTarget(EnemyRuntime enemy)
        {
            return enemy != null && enemy.MaxHealth > 0f && enemy.CurrentHealth / enemy.MaxHealth <= 0.35f;
        }

        private bool HasRinPassive(string passiveId, string passiveName)
        {
            return IsSelectedRinMonster()
                && ((!string.IsNullOrWhiteSpace(passiveId) && chosenSkillChoiceIds.Contains(passiveId))
                    || (!string.IsNullOrWhiteSpace(passiveId) && learnedPassiveSkillIds.Contains(passiveId)));
        }

        private bool HasRinAmbidextrous()
        {
            return HasRinPassive("rin-f", "양손잡이");
        }

        private bool HasRinBattleResonance()
        {
            return HasRinPassive("rin-g", "전장의 공명");
        }

        private bool HasRinWaveAmplification()
        {
            return HasRinPassive("rin-h", "파문 증폭");
        }

        private bool HasRinFinisherInstinct()
        {
            return HasRinPassive("rin-i", "마무리 본능");
        }

        private bool HasRinCollapseAftermath()
        {
            return HasRinPassive("rin-j", "붕괴 여파");
        }

        private bool IsSelectedRinMonster()
        {
            return selectedMonster != null &&
                string.Equals(selectedMonster.MonsterId, "rin", StringComparison.OrdinalIgnoreCase);
        }
    }
}
