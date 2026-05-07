using System;
using System.Collections.Generic;
using Pakuri.Data;
using UnityEngine;

namespace Pakuri.Combat
{
    public partial class CombatRuntimeController
    {
        private const float VegaThreeSwordProjectileSpeed = 16f;
        private const float VegaThreeSwordBulletInterval = 0.12f;
        private const float VegaSilentGreatbladeAreaWidth = 3f;
        private const float VegaSilentGreatbladeAreaHeight = 1f;
        private const float VegaSilentGreatbladeSilenceDuration = 3f;
        private const float VegaExterminationPermitDuration = 6f;
        private const float VegaBlackLedgerRadius = 1.25f;
        private const float VegaFinalSentenceDistributeRadius = 4.5f;

        private sealed class VegaPendingProjectileShot
        {
            public Vector3 Direction;
            public float DelayRemaining;
            public float DamageMultiplier = 1f;
            public int NameMarkStacks = 1;
            public string SkillId = "vega-a";
            public string Name = "VegaSword";
        }

        private readonly List<VegaPendingProjectileShot> vegaPendingShots = new List<VegaPendingProjectileShot>();
        private float vegaSilentGreatbladeCooldownRemaining;
        private float vegaExterminationPermitCooldownRemaining;
        private float vegaBlackLedgerCooldownRemaining;
        private float vegaFinalSentenceCooldownRemaining;
        private float vegaExterminationPermitTimer;
        private float vegaExterminationActionSpeedBonus;
        private float vegaExterminationAttackBonus;
        private float vegaBlackLedgerAreaChargeCooldownRemaining;

        private void ResetVegaSkillCombatTimers()
        {
            vegaPendingShots.Clear();
            vegaSilentGreatbladeCooldownRemaining = 0f;
            vegaExterminationPermitCooldownRemaining = 0f;
            vegaBlackLedgerCooldownRemaining = 0f;
            vegaFinalSentenceCooldownRemaining = 0f;
            vegaExterminationPermitTimer = 0f;
            vegaExterminationActionSpeedBonus = 0f;
            vegaExterminationAttackBonus = 0f;
            vegaBlackLedgerAreaChargeCooldownRemaining = 0f;
        }

        private void UpdateVegaSkillCooldowns()
        {
            var elapsed = Time.deltaTime * GetVegaActionSpeedMultiplier();
            vegaSilentGreatbladeCooldownRemaining = Mathf.Max(0f, vegaSilentGreatbladeCooldownRemaining - elapsed);
            vegaExterminationPermitCooldownRemaining = Mathf.Max(0f, vegaExterminationPermitCooldownRemaining - elapsed);
            vegaBlackLedgerCooldownRemaining = Mathf.Max(0f, vegaBlackLedgerCooldownRemaining - elapsed);
            vegaFinalSentenceCooldownRemaining = Mathf.Max(0f, vegaFinalSentenceCooldownRemaining - elapsed);
            vegaBlackLedgerAreaChargeCooldownRemaining = Mathf.Max(0f, vegaBlackLedgerAreaChargeCooldownRemaining - Time.deltaTime);
        }

        private void UpdateVegaSkillEffects()
        {
            if (!IsSelectedVegaMonster())
            {
                return;
            }

            vegaExterminationPermitTimer = Mathf.Max(0f, vegaExterminationPermitTimer - Time.deltaTime);
            if (Mathf.Approximately(vegaExterminationPermitTimer, 0f))
            {
                vegaExterminationActionSpeedBonus = 0f;
                vegaExterminationAttackBonus = 0f;
            }

            for (var i = 0; i < enemies.Count; i++)
            {
                var enemy = enemies[i];
                if (enemy == null)
                {
                    continue;
                }

                enemy.VegaBlackLedgerAreaVulnerabilityTimer = Mathf.Max(0f, enemy.VegaBlackLedgerAreaVulnerabilityTimer - Time.deltaTime);
                if (Mathf.Approximately(enemy.VegaBlackLedgerAreaVulnerabilityTimer, 0f))
                {
                    enemy.VegaBlackLedgerAreaVulnerabilityBonus = 0f;
                }

                enemy.VegaFinalSentenceVulnerabilityTimer = Mathf.Max(0f, enemy.VegaFinalSentenceVulnerabilityTimer - Time.deltaTime);
                if (Mathf.Approximately(enemy.VegaFinalSentenceVulnerabilityTimer, 0f))
                {
                    enemy.VegaFinalSentenceVulnerabilityBonus = 0f;
                }
            }

            var elapsed = Time.deltaTime * GetVegaActionSpeedMultiplier();
            for (var i = vegaPendingShots.Count - 1; i >= 0; i--)
            {
                var shot = vegaPendingShots[i];
                if (shot == null)
                {
                    vegaPendingShots.RemoveAt(i);
                    continue;
                }

                shot.DelayRemaining -= elapsed;
                if (shot.DelayRemaining > 0f)
                {
                    continue;
                }

                SpawnVegaSwordProjectile(shot);
                vegaPendingShots.RemoveAt(i);
            }
        }

        private bool TryTriggerVegaAutomaticSkills()
        {
            if (!IsSelectedVegaMonster())
            {
                return false;
            }

            var castAny = false;
            castAny |= TryCastVegaExterminationPermit();
            castAny |= TryCastVegaSilentGreatblade();
            castAny |= TryCastVegaBlackLedgerRelease();
            castAny |= TryCastVegaFinalSentence();

            if (!castAny)
            {
                statusLabel = $"{selectedMonsterName}: no Vega active skill is ready.";
            }

            return true;
        }

        private void FireManualVegaThreeSwordFlurry(Vector3 baseDirection)
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
            var firstMultiplier = GetVegaThreeSwordDamageMultiplier(false);
            var lastMultiplier = GetVegaThreeSwordDamageMultiplier(true);
            QueueVegaSwordShot(baseDirection, 0f, firstMultiplier, GetVegaThreeSwordNameMarkStacks(), "vega-a", "VegaSword1");
            QueueVegaSwordShot(baseDirection, VegaThreeSwordBulletInterval, firstMultiplier, GetVegaThreeSwordNameMarkStacks(), "vega-a", "VegaSword2");
            QueueVegaSwordShot(baseDirection, VegaThreeSwordBulletInterval * 2f, lastMultiplier, GetVegaThreeSwordNameMarkStacks(), "vega-a", "VegaSword3");

            if (HasChoice("vega-a-master-1"))
            {
                QueueVegaSwordShot(baseDirection, VegaThreeSwordBulletInterval * 3f, 0.45f, 1, "vega-a-afterimage", "VegaAfterimage");
            }

            currentShotsRemaining -= 1;
            shotCooldown = GetVegaThreeSwordFlurryShotInterval();
            if (currentShotsRemaining <= 0)
            {
                currentShotsRemaining = 0;
                reloadRemaining = GetVegaThreeSwordFlurryReloadSeconds();
                statusLabel = "Three-Sword Flurry queued. Magazine empty; reloading.";
                return;
            }

            statusLabel = $"Three-Sword Flurry queued. Ammo {currentShotsRemaining}/{GetVegaThreeSwordFlurryMagazineCapacity()}.";
        }

        private bool TryCastVegaSilentGreatblade()
        {
            var skill = FindSelectedSkill(SkillSlot.B);
            if (skill == null || !HasLearnedActive(SkillSlot.B) || vegaSilentGreatbladeCooldownRemaining > 0f || eveAnchor == null)
            {
                return false;
            }

            var target = FindNearestEnemy(eveAnchor.position, GetVegaMapWideSkillRange());
            if (target == null || target.Transform == null)
            {
                return false;
            }

            var damageMultiplier = 1f;
            if (HasChoice("vega-b-trait-1"))
            {
                damageMultiplier *= 1.25f;
            }

            if (HasChoice("vega-b-master-2"))
            {
                damageMultiplier *= 1.70f;
            }

            var silenceDuration = VegaSilentGreatbladeSilenceDuration
                + (HasChoice("vega-b-trait-2") ? 1f : 0f)
                + (HasVegaSealingSwordForm() && HasChoice("vega-g-trait-2") ? 1f : 0f);
            var center = target.Transform.position;
            var hitCount = ApplyVegaTargetRectangleSlash(center, VegaSilentGreatbladeAreaWidth, VegaSilentGreatbladeAreaHeight, GetVegaSkillBaseDamage(skill), damageMultiplier, "vega-b", silenceDuration, GetVegaSilentGreatbladeNameMarkStacks(), skill.SkillEffectPrefab);
            if (HasChoice("vega-b-master-1"))
            {
                hitCount += ApplyVegaTargetRectangleSlash(center, VegaSilentGreatbladeAreaWidth, VegaSilentGreatbladeAreaHeight, GetVegaSkillBaseDamage(skill) * 0.45f, 1f, "vega-b-second", 1f, 0, skill.SkillEffectPrefab);
            }

            vegaSilentGreatbladeCooldownRemaining = GetVegaCooldown(skill, 8f, GetVegaSilentGreatbladeCooldownMultiplier());
            statusLabel = $"Silent Greatblade hit {hitCount} enemy(s).";
            return hitCount > 0;
        }

        private bool TryCastVegaExterminationPermit()
        {
            var skill = FindSelectedSkill(SkillSlot.C);
            if (skill == null || !HasLearnedActive(SkillSlot.C) || vegaExterminationPermitCooldownRemaining > 0f)
            {
                return false;
            }

            var duration = VegaExterminationPermitDuration + (HasChoice("vega-c-trait-1") ? 2f : 0f);
            if (HasVegaExecutionPrep())
            {
                duration *= 1.20f;
            }

            vegaExterminationActionSpeedBonus = 0.25f
                + (HasChoice("vega-c-trait-2") ? 0.10f : 0f)
                + (HasVegaExecutionPrep() ? 0.12f : 0f)
                + (HasVegaExecutionPrep() && HasChoice("vega-h-trait-1") ? 0.06f : 0f);
            vegaExterminationAttackBonus = 0.20f
                + (HasChoice("vega-c-trait-3") ? 0.10f : 0f)
                + (HasVegaExecutionPrep() && HasChoice("vega-h-trait-2") ? 0.08f : 0f);
            if (HasChoice("vega-c-master-1"))
            {
                duration += 2f;
            }

            if (HasChoice("vega-c-master-2"))
            {
                vegaExterminationActionSpeedBonus += 0.25f;
                vegaExterminationAttackBonus += 0.25f;
                duration -= 2f;
            }

            vegaExterminationPermitTimer = Mathf.Max(vegaExterminationPermitTimer, Mathf.Max(1f, duration));
            vegaExterminationPermitCooldownRemaining = GetVegaCooldown(skill, 14f, 1f);
            statusLabel = $"Extermination Permit active for {vegaExterminationPermitTimer:0.#}s.";
            return true;
        }

        private bool TryCastVegaBlackLedgerRelease()
        {
            var skill = FindSelectedSkill(SkillSlot.D);
            if (skill == null || !HasLearnedActive(SkillSlot.D) || vegaBlackLedgerCooldownRemaining > 0f)
            {
                return false;
            }

            var markedTargets = GetVegaMarkedEnemies();
            if (markedTargets.Count == 0)
            {
                return false;
            }

            var radius = (skill.Radius > 0f ? skill.Radius : VegaBlackLedgerRadius) * (HasChoice("vega-d-trait-2") ? 1.25f : 1f);
            var damageMultiplier = HasChoice("vega-d-trait-1") ? 1.25f : 1f;
            var slashCount = 1;
            if (HasChoice("vega-d-master-1"))
            {
                slashCount = 2;
                damageMultiplier *= 0.65f;
            }

            if (HasChoice("vega-d-master-2"))
            {
                radius *= 1.50f;
                damageMultiplier *= 1.30f;
            }

            var hitCount = 0;
            foreach (var markedTarget in markedTargets)
            {
                if (markedTarget == null || markedTarget.Transform == null)
                {
                    continue;
                }

                var centerMultiplier = damageMultiplier;
                if (HasChoice("vega-d-trait-4") && markedTarget.VegaNameMarkStacks >= 10)
                {
                    centerMultiplier *= 1.30f;
                }

                for (var i = 0; i < slashCount; i++)
                {
                    hitCount += ApplyVegaAreaSlash(markedTarget.Transform.position, radius, GetVegaSkillBaseDamage(skill), centerMultiplier, "vega-d", HasChoice("vega-d-trait-5") ? 1 : 0, skill.SkillEffectPrefab);
                }
            }

            vegaBlackLedgerCooldownRemaining = GetVegaCooldown(skill, 11f, GetVegaBlackLedgerCooldownMultiplier());
            if (hitCount > 0)
            {
                TrackVegaAreaDamageDealt();
            }

            statusLabel = $"Black Ledger Release hit {hitCount} enemy instance(s).";
            return hitCount > 0;
        }

        private bool TryCastVegaFinalSentence()
        {
            var skill = FindSelectedSkill(SkillSlot.E);
            if (skill == null || !HasLearnedActive(SkillSlot.E) || vegaFinalSentenceCooldownRemaining > 0f)
            {
                return false;
            }

            var target = FindVegaHighestMarkedEnemy();
            if (target == null)
            {
                return false;
            }

            var stacks = Mathf.Max(0, target.VegaNameMarkStacks);
            var baseDamage = GetVegaSkillBaseDamage(skill) * (HasChoice("vega-e-trait-1") ? 1.25f : 1f);
            if (HasChoice("vega-e-master-2"))
            {
                baseDamage *= 0.80f;
            }

            var stackDamage = stacks * GetVegaFinalSentenceStackDamage();
            var consumedStacks = Mathf.FloorToInt(stacks * (HasChoice("vega-e-master-1") ? 1f : 0.50f));
            var wasAlive = target.CurrentHealth > 0f;
            var applied = ApplyVegaSkillDamage(target, baseDamage + stackDamage, 1f, "vega-e");
            target.VegaNameMarkStacks = Mathf.Max(0, target.VegaNameMarkStacks - consumedStacks);
            CreateVegaAreaVisual("VegaFinalSentence", target.Transform.position, Mathf.Max(0.9f, GetEnemyHitRadius(target) + 0.35f), 0.28f, new Color(0.72f, 0.62f, 1f, 0.55f), 26, skill.SkillEffectPrefab);

            vegaFinalSentenceCooldownRemaining = GetVegaCooldown(skill, 15f, HasChoice("vega-e-trait-3") ? 0.80f : 1f);
            if (wasAlive && target.CurrentHealth <= 0f)
            {
                ChargeVegaCooldownsAfterFinalSentenceKill();
                if (HasChoice("vega-e-trait-5"))
                {
                    DistributeVegaNameMarks(target, Mathf.FloorToInt(consumedStacks * 0.25f), VegaFinalSentenceDistributeRadius);
                }

                if (HasChoice("vega-e-master-2"))
                {
                    vegaFinalSentenceCooldownRemaining *= 0.30f;
                }

                if (HasVegaExecutioner() && HasChoice("vega-j-trait-3"))
                {
                    ReduceVegaBlackLedgerCooldownByRatio(0.20f);
                }
            }
            else if (target.CurrentHealth > 0f)
            {
                ApplyVegaFinalSentenceVulnerability(target);
            }

            statusLabel = $"Final Sentence hit {target.DisplayName} for {applied:0.0} and consumed {consumedStacks} mark(s).";
            return applied > 0f;
        }

        private void QueueVegaSwordShot(Vector3 direction, float delay, float damageMultiplier, int markStacks, string skillId, string name)
        {
            vegaPendingShots.Add(new VegaPendingProjectileShot
            {
                Direction = direction,
                DelayRemaining = Mathf.Max(0f, delay),
                DamageMultiplier = Mathf.Max(0f, damageMultiplier),
                NameMarkStacks = Mathf.Max(0, markStacks),
                SkillId = skillId,
                Name = name
            });
        }

        private void SpawnVegaSwordProjectile(VegaPendingProjectileShot shot)
        {
            if (shot == null || eveAnchor == null || projectileRoot == null)
            {
                return;
            }

            nextProjectileSequence += 1;
            var projectileObject = new GameObject($"{shot.Name}_{nextProjectileSequence:00}");
            projectileObject.transform.SetParent(projectileRoot, false);
            projectileObject.transform.position = eveAnchor.position + shot.Direction * 0.2f;
            projectileObject.transform.localScale = new Vector3(projectileHitRadiusConfigured, projectileHitRadiusConfigured, 1f);
            projectileObject.transform.right = shot.Direction;

            var renderer = projectileObject.AddComponent<SpriteRenderer>();
            renderer.sprite = selectedProjectileSprite != null ? selectedProjectileSprite : GetSharedSprite();
            renderer.color = new Color(0.78f, 0.68f, 1f, 0.98f);
            renderer.sortingOrder = 25;

            var skill = FindSelectedSkill(SkillSlot.A);
            projectiles.Add(new ProjectileRuntime
            {
                GameObject = projectileObject,
                Transform = projectileObject.transform,
                Renderer = renderer,
                Direction = shot.Direction,
                Speed = VegaThreeSwordProjectileSpeed,
                RemainingLifetime = projectileLifetimeConfigured,
                HitRadius = projectileHitRadiusConfigured,
                BaseDamage = GetVegaSkillBaseDamage(skill) * shot.DamageMultiplier,
                Attribute = DamageAttribute.Physical,
                SkillId = shot.SkillId,
                RemainingPierce = 999,
                VegaNameMarkStacks = shot.NameMarkStacks
            });
        }

        private int ApplyVegaTargetRectangleSlash(Vector3 center, float width, float height, float baseDamage, float damageMultiplier, string skillId, float silenceDuration, int markStacks, GameObject effectPrefab = null)
        {
            CreateVegaRectangleVisual("VegaSilentGreatbladeArea", center, width, height, 0.24f, new Color(0.70f, 0.62f, 1f, 0.58f), 25, effectPrefab);
            var hitCount = 0;
            for (var i = 0; i < enemies.Count; i++)
            {
                var enemy = enemies[i];
                if (enemy == null || enemy.CurrentHealth <= 0f || enemy.Transform == null || !IsPointInsideVegaRectangle(enemy.Transform.position, center, width, height, GetEnemyHitRadius(enemy)))
                {
                    continue;
                }

                ApplyVegaSkillDamage(enemy, baseDamage, damageMultiplier, skillId);
                ApplyVegaSilence(enemy, silenceDuration + (HasChoice("vega-b-master-2") && enemy.VegaNameMarkStacks >= 10 ? 1f : 0f));
                AddVegaNameMarks(enemy, markStacks);
                hitCount += 1;
            }

            return hitCount;
        }

        private int ApplyVegaAreaSlash(Vector3 center, float radius, float baseDamage, float damageMultiplier, string skillId, int markStacks, GameObject effectPrefab = null)
        {
            CreateVegaAreaVisual("VegaBlackLedgerSlash", center, radius, 0.24f, new Color(0.48f, 0.38f, 0.86f, 0.42f), 24, effectPrefab);
            var hitCount = 0;
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

                var applied = ApplyVegaSkillDamage(enemy, baseDamage, damageMultiplier, skillId);
                AddVegaNameMarks(enemy, markStacks);
                if (applied > 0f)
                {
                    ApplyVegaBlackLedgerAreaVulnerability(enemy);
                }

                hitCount += 1;
            }

            return hitCount;
        }

        private void ApplyVegaSkillEffectDamage(SkillEffectRuntime effect, EnemyRuntime enemy)
        {
            if (effect == null || enemy == null || effect.HitThisTick.Contains(enemy))
            {
                return;
            }

            effect.HitThisTick.Add(enemy);
            ApplyVegaSkillDamage(enemy, effect.BaseDamage, 1f, effect.SkillId);

            AddVegaNameMarks(enemy, effect.StatusStacks);
        }

        private float ApplyVegaSkillDamage(EnemyRuntime enemy, float baseDamage, float finalMultiplier, string skillId)
        {
            if (enemy == null || enemy.CurrentHealth <= 0f || baseDamage <= 0f)
            {
                return 0f;
            }

            var result = DamageCalculator.Resolve(
                baseDamage,
                DamageAttribute.Physical,
                enemy.Defenses,
                flatDefenseReduction: GetVegaFlatDefenseReduction(enemy, DamageAttribute.Physical),
                criticalChanceBonus: GetVegaCriticalChanceBonus(enemy, DamageAttribute.Physical, skillId),
                targetCriticalResistance: enemy.CriticalResistance,
                finalDamageMultiplier: enemy.DamageTakenMultiplier * Mathf.Max(0f, finalMultiplier) * GetVegaFinalDamageMultiplier(enemy, DamageAttribute.Physical, skillId));
            var applied = ApplyDamageToEnemy(enemy, result.FinalDamage, DamageAttribute.Physical);
            enemy.FlashTimer = 0.08f;
            Debug.Log($"[CombatDamage] Vega.{skillId} -> {enemy.DisplayName}: {result.FormulaLog}; Applied={applied:0.##}, ShieldLeft={enemy.ShieldValue:0.##}, HpLeft={Mathf.Max(0f, enemy.CurrentHealth):0.##}");
            return applied;
        }

        private void HandleVegaProjectileHit(ProjectileRuntime projectile, EnemyRuntime enemy, float appliedDamage, bool wasAlive)
        {
            if (!IsSelectedVegaMonster()
                || projectile == null
                || enemy == null
                || !IsVegaProjectileSkill(projectile.SkillId)
                || appliedDamage <= 0f)
            {
                return;
            }

            AddVegaNameMarks(enemy, projectile.VegaNameMarkStacks);
            if (wasAlive && enemy.CurrentHealth <= 0f && HasChoice("vega-a-master-2"))
            {
                TransferVegaCondemnationMark(enemy, 3);
            }
        }

        private void TransferVegaCondemnationMark(EnemyRuntime sourceEnemy, int stacks)
        {
            var target = FindNearestEnemy(sourceEnemy.Transform.position, GetVegaMapWideSkillRange(), enemy => enemy != sourceEnemy);
            if (target == null)
            {
                return;
            }

            AddVegaNameMarks(target, stacks);
            ApplyVegaSkillDamage(target, GetVegaSkillBaseDamage(FindSelectedSkill(SkillSlot.A)) * 0.35f, 1f, "vega-a-chain");
            CreateVegaLineVisual("VegaCondemnationChain", sourceEnemy.Transform.position, (target.Transform.position - sourceEnemy.Transform.position).normalized, Vector2.Distance(sourceEnemy.Transform.position, target.Transform.position), 0.32f, 0.2f, new Color(0.72f, 0.62f, 1f, 0.58f), 26);
        }

        private void DistributeVegaNameMarks(EnemyRuntime sourceEnemy, int totalStacks, float radius)
        {
            if (sourceEnemy == null || sourceEnemy.Transform == null || totalStacks <= 0)
            {
                return;
            }

            var targets = new List<EnemyRuntime>();
            for (var i = 0; i < enemies.Count; i++)
            {
                var enemy = enemies[i];
                if (enemy == null
                    || enemy == sourceEnemy
                    || enemy.CurrentHealth <= 0f
                    || enemy.Transform == null
                    || Vector2.Distance(sourceEnemy.Transform.position, enemy.Transform.position) > radius)
                {
                    continue;
                }

                targets.Add(enemy);
            }

            if (targets.Count == 0)
            {
                return;
            }

            for (var i = 0; i < totalStacks; i++)
            {
                AddVegaNameMarks(targets[i % targets.Count], 1);
            }
        }

        private SkillEffectRuntime CreateVegaLineVisual(string name, Vector3 origin, Vector3 direction, float length, float width, float duration, Color color, int sortingOrder, GameObject effectPrefab = null)
        {
            direction.z = 0f;
            if (direction.sqrMagnitude < 0.01f)
            {
                direction = Vector3.right;
            }

            direction.Normalize();
            var effect = CreateLineEffect(name, origin, direction, Mathf.Max(0.1f, length), Mathf.Max(0.05f, width), Mathf.Max(0.05f, duration), effectPrefab);
            effect.SkillId = name;
            if (effect.Renderer != null)
            {
                effect.Renderer.color = color;
                effect.Renderer.sortingOrder = sortingOrder;
            }

            skillEffects.Add(effect);
            return effect;
        }

        private SkillEffectRuntime CreateVegaRectangleVisual(string name, Vector3 center, float width, float height, float duration, Color color, int sortingOrder, GameObject effectPrefab = null)
        {
            var clampedWidth = Mathf.Max(0.1f, width);
            var clampedHeight = Mathf.Max(0.05f, height);
            var origin = center - Vector3.right * (clampedWidth * 0.5f);
            var effect = CreateLineEffect(name, origin, Vector3.right, clampedWidth, clampedHeight, Mathf.Max(0.05f, duration), effectPrefab);
            effect.SkillId = name;
            if (effect.Renderer != null)
            {
                effect.Renderer.color = color;
                effect.Renderer.sortingOrder = sortingOrder;
            }

            skillEffects.Add(effect);
            return effect;
        }

        private void CreateVegaAreaVisual(string name, Vector3 center, float radius, float duration, Color color, int sortingOrder, GameObject effectPrefab = null)
        {
            var effect = CreateCircleEffect(name, center, radius, duration, effectPrefab);
            effect.SkillId = name;
            if (effect.Renderer != null)
            {
                effect.Renderer.color = color;
                effect.Renderer.sortingOrder = sortingOrder;
            }

            skillEffects.Add(effect);
        }

        private Vector3 GetVegaAimDirection()
        {
            if (eveAnchor == null)
            {
                return Vector3.right;
            }

            var direction = currentAttackPoint - eveAnchor.position;
            direction.z = 0f;
            if (direction.sqrMagnitude < 0.01f)
            {
                var target = FindNearestEnemy(eveAnchor.position, GetVegaMapWideSkillRange());
                if (target != null)
                {
                    direction = target.Transform.position - eveAnchor.position;
                }
            }

            direction.z = 0f;
            return direction.sqrMagnitude < 0.01f ? Vector3.right : direction.normalized;
        }

        private List<EnemyRuntime> GetVegaMarkedEnemies()
        {
            var marked = new List<EnemyRuntime>();
            for (var i = 0; i < enemies.Count; i++)
            {
                var enemy = enemies[i];
                if (enemy != null && enemy.CurrentHealth > 0f && enemy.Transform != null && enemy.VegaNameMarkStacks > 0)
                {
                    marked.Add(enemy);
                }
            }

            return marked;
        }

        private EnemyRuntime FindVegaHighestMarkedEnemy()
        {
            EnemyRuntime best = null;
            var bestStacks = 0;
            for (var i = 0; i < enemies.Count; i++)
            {
                var enemy = enemies[i];
                if (enemy == null || enemy.CurrentHealth <= 0f || enemy.Transform == null || enemy.VegaNameMarkStacks <= bestStacks)
                {
                    continue;
                }

                best = enemy;
                bestStacks = enemy.VegaNameMarkStacks;
            }

            return best;
        }

        private void AddVegaNameMarks(EnemyRuntime enemy, int stacks)
        {
            if (enemy == null || stacks <= 0)
            {
                return;
            }

            enemy.VegaNameMarkStacks = Mathf.Clamp(enemy.VegaNameMarkStacks + stacks, 0, 999);
        }

        private void ApplyVegaSilence(EnemyRuntime enemy, float duration)
        {
            if (enemy == null || duration <= 0f)
            {
                return;
            }

            enemy.VegaSilenceTimer = Mathf.Max(enemy.VegaSilenceTimer, duration);
        }

        private float GetVegaSkillBaseDamage(SkillDefinition skill)
        {
            if (skill == null)
            {
                return 0f;
            }

            var effectivePower = powerStatConfigured * (1f + (vegaExterminationPermitTimer > 0f ? vegaExterminationAttackBonus : 0f));
            return skill.BaseDamage + effectivePower * skill.AttackPowerCoefficient;
        }

        private float GetVegaThreeSwordDamageMultiplier(bool isLastShot)
        {
            var multiplier = isLastShot ? 2f : 1f;
            if (HasChoice("vega-a-trait-1"))
            {
                multiplier *= 1.20f;
            }

            if (isLastShot && HasChoice("vega-a-trait-4"))
            {
                multiplier += 0.50f;
            }

            return multiplier;
        }

        private int GetVegaThreeSwordNameMarkStacks()
        {
            return 1
                + (vegaExterminationPermitTimer > 0f && HasChoice("vega-c-master-1") ? 1 : 0)
                + (HasVegaDeepEngraving() && HasChoice("vega-f-trait-3") ? 1 : 0);
        }

        private int GetVegaSilentGreatbladeNameMarkStacks()
        {
            return (HasVegaSealingSwordForm() ? 2 : 0) + (HasChoice("vega-b-trait-5") ? 2 : 0);
        }

        private int GetVegaThreeSwordFlurryMagazineCapacity()
        {
            var skill = FindSelectedSkill(SkillSlot.A);
            var capacity = skill != null && skill.MagazineCapacity > 0 ? skill.MagazineCapacity : magazineCapacityConfigured;
            if (HasChoice("vega-a-trait-2"))
            {
                capacity += 2;
            }

            return Mathf.Max(1, capacity);
        }

        private float GetVegaThreeSwordFlurryReloadSeconds()
        {
            var skill = FindSelectedSkill(SkillSlot.A);
            var reload = skill != null && skill.ReloadSeconds > 0f ? skill.ReloadSeconds : reloadDurationConfigured;
            if (HasChoice("vega-a-trait-3"))
            {
                reload *= SpeedBonusToIntervalMultiplier(0.25f);
            }

            if (vegaExterminationPermitTimer > 0f && HasChoice("vega-c-trait-4"))
            {
                reload *= SpeedBonusToIntervalMultiplier(0.30f);
            }

            return Mathf.Max(0.25f, reload);
        }

        private float GetVegaThreeSwordFlurryShotInterval()
        {
            var skill = FindSelectedSkill(SkillSlot.A);
            return Mathf.Max(0.05f, skill != null && skill.ShotIntervalSeconds > 0f ? skill.ShotIntervalSeconds : shotIntervalConfigured);
        }

        private float GetVegaSilentGreatbladeCooldownMultiplier()
        {
            return HasChoice("vega-b-trait-3") ? 0.80f : 1f;
        }

        private float GetVegaBlackLedgerCooldownMultiplier()
        {
            var multiplier = HasChoice("vega-d-trait-3") ? 0.80f : 1f;
            if (HasChoice("vega-d-master-2"))
            {
                multiplier *= 1.20f;
            }

            return multiplier;
        }

        private float GetVegaFinalSentenceStackDamage()
        {
            var multiplier = 1f + (HasChoice("vega-e-trait-2") ? 0.25f : 0f) + (HasChoice("vega-e-master-1") ? 0.80f : 0f);
            return (6f + powerStatConfigured * 0.18f) * multiplier;
        }

        private float GetVegaCooldown(SkillDefinition skill, float fallback, float multiplier)
        {
            var cooldown = skill != null && skill.CooldownSeconds > 0f ? skill.CooldownSeconds : fallback;
            return Mathf.Max(0.1f, cooldown * Mathf.Max(0.05f, multiplier));
        }

        private float GetVegaActionSpeedMultiplier()
        {
            return 1f + (vegaExterminationPermitTimer > 0f ? vegaExterminationActionSpeedBonus : 0f);
        }

        private float GetVegaFinalDamageMultiplier(EnemyRuntime enemy, DamageAttribute attribute, string skillId)
        {
            if (!IsSelectedVegaMonster() || enemy == null || attribute != DamageAttribute.Physical)
            {
                return 1f;
            }

            var multiplier = 1f;
            if (vegaExterminationPermitTimer > 0f && HasChoice("vega-c-trait-5") && enemy.VegaNameMarkStacks > 0)
            {
                multiplier *= 1.15f;
            }

            if (HasVegaDeepEngraving() && enemy.VegaNameMarkStacks > 0)
            {
                multiplier *= 1f + 0.10f + (HasChoice("vega-f-trait-1") ? 0.05f : 0f);
            }

            if (HasVegaSealingSwordForm() && enemy.VegaSilenceTimer > 0f)
            {
                multiplier *= 1f + 0.14f + (HasChoice("vega-g-trait-1") ? 0.06f : 0f);
            }

            if (vegaExterminationPermitTimer > 0f && HasVegaExecutionPrep() && HasChoice("vega-h-trait-3") && enemy.VegaNameMarkStacks > 0)
            {
                multiplier *= 1.10f;
            }

            if (IsVegaAreaDamageSkill(skillId) && enemy.VegaBlackLedgerAreaVulnerabilityTimer > 0f)
            {
                multiplier *= 1f + enemy.VegaBlackLedgerAreaVulnerabilityBonus;
            }

            if (enemy.VegaFinalSentenceVulnerabilityTimer > 0f)
            {
                multiplier *= 1f + enemy.VegaFinalSentenceVulnerabilityBonus;
            }

            if (string.Equals(skillId, "vega-a", StringComparison.OrdinalIgnoreCase)
                && HasChoice("vega-a-trait-5")
                && enemy.VegaNameMarkStacks >= 10)
            {
                multiplier *= 1.25f;
            }

            return multiplier;
        }

        private float GetVegaFlatDefenseReduction(EnemyRuntime enemy, DamageAttribute attribute)
        {
            if (!IsSelectedVegaMonster() || enemy == null || attribute != DamageAttribute.Physical)
            {
                return 0f;
            }

            var reduction = 0f;
            if (HasVegaDeepEngraving() && enemy.VegaNameMarkStacks >= 10)
            {
                reduction += 8f + (HasChoice("vega-f-trait-2") ? 4f : 0f);
            }

            return reduction;
        }

        private float GetVegaCriticalChanceBonus(EnemyRuntime enemy, DamageAttribute attribute, string skillId)
        {
            if (!IsSelectedVegaMonster() || enemy == null || attribute != DamageAttribute.Physical)
            {
                return 0f;
            }

            var bonus = 0f;
            if (string.Equals(skillId, "vega-e", StringComparison.OrdinalIgnoreCase)
                && HasChoice("vega-e-trait-4")
                && enemy.VegaNameMarkStacks >= 20)
            {
                bonus += 0.35f;
            }

            if (HasVegaSealingSwordForm()
                && HasChoice("vega-g-trait-3")
                && enemy.VegaSilenceTimer > 0f
                && enemy.VegaNameMarkStacks > 0)
            {
                bonus += 0.10f;
            }

            return bonus;
        }

        private bool IsVegaProjectileSkill(string skillId)
        {
            return string.Equals(skillId, "vega-a", StringComparison.OrdinalIgnoreCase)
                || string.Equals(skillId, "vega-a-afterimage", StringComparison.OrdinalIgnoreCase);
        }

        private bool IsVegaSkillEffect(SkillEffectRuntime effect)
        {
            return effect != null && !string.IsNullOrWhiteSpace(effect.SkillId) && effect.SkillId.StartsWith("vega-", StringComparison.OrdinalIgnoreCase);
        }

        private bool IsVegaAreaDamageSkill(string skillId)
        {
            return string.Equals(skillId, "vega-d", StringComparison.OrdinalIgnoreCase);
        }

        private bool IsVegaLineSkillEffect(SkillEffectRuntime effect)
        {
            return effect != null && string.Equals(effect.SkillId, "vega-b-second", StringComparison.OrdinalIgnoreCase);
        }

        private bool IsPointInsideVegaRectangle(Vector3 point, Vector3 center, float width, float height, float padding)
        {
            var halfWidth = Mathf.Max(0.05f, width * 0.5f) + Mathf.Max(0f, padding);
            var halfHeight = Mathf.Max(0.05f, height * 0.5f) + Mathf.Max(0f, padding);
            var delta = point - center;
            return Mathf.Abs(delta.x) <= halfWidth && Mathf.Abs(delta.y) <= halfHeight;
        }

        private float GetVegaMapWideSkillRange()
        {
            var width = Mathf.Max(fieldSize.x, EnemySpawnX);
            var height = Mathf.Max(fieldSize.y, BattlefieldMaxY);
            return Mathf.Sqrt(width * width + height * height) + 2f;
        }

        private bool IsSelectedVegaMonster()
        {
            return selectedMonster != null &&
                string.Equals(selectedMonster.MonsterId, "vega", StringComparison.OrdinalIgnoreCase);
        }

        private void ApplyVegaBlackLedgerAreaVulnerability(EnemyRuntime enemy)
        {
            if (!HasVegaChainCleaving() || enemy == null)
            {
                return;
            }

            enemy.VegaBlackLedgerAreaVulnerabilityBonus = Mathf.Max(
                enemy.VegaBlackLedgerAreaVulnerabilityBonus,
                0.15f
                + (HasChoice("vega-i-trait-1") ? 0.07f : 0f)
                + (HasChoice("vega-i-trait-3") && enemy.VegaNameMarkStacks >= 10 ? 0.10f : 0f));
            enemy.VegaBlackLedgerAreaVulnerabilityTimer = Mathf.Max(
                enemy.VegaBlackLedgerAreaVulnerabilityTimer,
                4f + (HasChoice("vega-i-trait-2") ? 2f : 0f));
        }

        private void ApplyVegaFinalSentenceVulnerability(EnemyRuntime enemy)
        {
            if (!HasVegaExecutioner() || enemy == null)
            {
                return;
            }

            enemy.VegaFinalSentenceVulnerabilityBonus = Mathf.Max(
                enemy.VegaFinalSentenceVulnerabilityBonus,
                0.10f + (HasChoice("vega-j-trait-2") ? 0.05f : 0f));
            enemy.VegaFinalSentenceVulnerabilityTimer = Mathf.Max(enemy.VegaFinalSentenceVulnerabilityTimer, 5f);
        }

        private void TrackVegaAreaDamageDealt()
        {
            if (!HasVegaChainCleaving() || vegaBlackLedgerAreaChargeCooldownRemaining > 0f)
            {
                return;
            }

            ReduceVegaBlackLedgerCooldownByRatio(0.03f);
            vegaBlackLedgerAreaChargeCooldownRemaining = 1f;
        }

        private void ChargeVegaCooldownsAfterFinalSentenceKill()
        {
            if (!HasVegaExecutioner())
            {
                return;
            }

            var ratio = 0.20f + (HasChoice("vega-j-trait-1") ? 0.10f : 0f);
            ReduceVegaCooldownByRatio(ref vegaSilentGreatbladeCooldownRemaining, GetVegaCooldown(FindSelectedSkill(SkillSlot.B), 8f, GetVegaSilentGreatbladeCooldownMultiplier()), ratio);
            ReduceVegaCooldownByRatio(ref vegaExterminationPermitCooldownRemaining, GetVegaCooldown(FindSelectedSkill(SkillSlot.C), 14f, 1f), ratio);
            ReduceVegaCooldownByRatio(ref vegaBlackLedgerCooldownRemaining, GetVegaCooldown(FindSelectedSkill(SkillSlot.D), 11f, GetVegaBlackLedgerCooldownMultiplier()), ratio);
            ReduceVegaCooldownByRatio(ref vegaFinalSentenceCooldownRemaining, GetVegaCooldown(FindSelectedSkill(SkillSlot.E), 15f, HasChoice("vega-e-trait-3") ? 0.80f : 1f), ratio);
            if (reloadRemaining > 0f)
            {
                reloadRemaining = Mathf.Max(0f, reloadRemaining - GetVegaThreeSwordFlurryReloadSeconds() * ratio);
            }
        }

        private void ReduceVegaBlackLedgerCooldownByRatio(float ratio)
        {
            ReduceVegaCooldownByRatio(ref vegaBlackLedgerCooldownRemaining, GetVegaCooldown(FindSelectedSkill(SkillSlot.D), 11f, GetVegaBlackLedgerCooldownMultiplier()), ratio);
        }

        private static void ReduceVegaCooldownByRatio(ref float cooldownRemaining, float cooldownDuration, float ratio)
        {
            cooldownRemaining = Mathf.Max(0f, cooldownRemaining - Mathf.Max(0f, cooldownDuration) * Mathf.Clamp01(ratio));
        }

        private bool HasVegaPassive(string passiveId, string passiveName)
        {
            return IsSelectedVegaMonster()
                && ((!string.IsNullOrWhiteSpace(passiveId) && chosenSkillChoiceIds.Contains(passiveId))
                    || (!string.IsNullOrWhiteSpace(passiveId) && learnedPassiveSkillIds.Contains(passiveId)));
        }

        private bool HasVegaDeepEngraving()
        {
            return HasVegaPassive("vega-f", "각인 심화");
        }

        private bool HasVegaSealingSwordForm()
        {
            return HasVegaPassive("vega-g", "봉인검식");
        }

        private bool HasVegaExecutionPrep()
        {
            return HasVegaPassive("vega-h", "처형 준비");
        }

        private bool HasVegaChainCleaving()
        {
            return HasVegaPassive("vega-i", "연쇄 참결");
        }

        private bool HasVegaExecutioner()
        {
            return HasVegaPassive("vega-j", "사형 집행인");
        }
    }
}
