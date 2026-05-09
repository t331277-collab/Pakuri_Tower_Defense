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

        private bool TryTickVegaUnitSkill(CombatUnitRuntime runtime, CombatSkillRuntime skillRuntime, float elapsed)
        {
            if (!IsVegaCombatUnit(runtime) || skillRuntime == null || skillRuntime.Skill == null)
            {
                return false;
            }

            var scaledElapsed = elapsed * GetVegaUnitActionSpeedMultiplier(runtime);
            TickCombatSkillRuntime(runtime, skillRuntime, scaledElapsed);

            switch (skillRuntime.Skill.Slot)
            {
                case SkillSlot.A:
                    skillRuntime.TickReload(scaledElapsed, ResolveManifestedMagazineCapacity(runtime, skillRuntime.Skill));
                    TryFireVegaUnitThreeSwordFlurry(runtime, skillRuntime);
                    return true;
                case SkillSlot.B:
                    TryCastVegaUnitSilentGreatblade(runtime, skillRuntime);
                    return true;
                case SkillSlot.C:
                    TryCastVegaUnitExterminationPermit(runtime, skillRuntime);
                    return true;
                case SkillSlot.D:
                    TryCastVegaUnitBlackLedgerRelease(runtime, skillRuntime);
                    return true;
                case SkillSlot.E:
                    TryCastVegaUnitFinalSentence(runtime, skillRuntime);
                    return true;
                default:
                    return false;
            }
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

        private bool TryFireVegaUnitThreeSwordFlurry(CombatUnitRuntime runtime, CombatSkillRuntime skillRuntime)
        {
            if (!IsVegaCombatUnit(runtime)
                || skillRuntime == null
                || skillRuntime.Skill == null
                || skillRuntime.ReloadRemaining > 0f
                || skillRuntime.ShotCooldownRemaining > 0f)
            {
                return false;
            }

            if (skillRuntime.ShotsRemaining <= 0)
            {
                skillRuntime.ReloadDuration = ResolveManifestedReloadDuration(runtime, skillRuntime.Skill);
                skillRuntime.ReloadRemaining = skillRuntime.ReloadDuration;
                return false;
            }

            var target = FindNearestManifestedMonsterTarget(runtime.Transform.position);
            if (target == null || target.Transform == null)
            {
                skillRuntime.ShotCooldownRemaining = 0.25f;
                return false;
            }

            QueueManifestedVegaThreeSwordFlurry(runtime, skillRuntime, target);
            skillRuntime.ShotsRemaining -= 1;
            skillRuntime.ShotInterval = ResolveManifestedShotInterval(runtime, skillRuntime.Skill);
            skillRuntime.ShotCooldownRemaining = skillRuntime.ShotInterval;
            if (skillRuntime.ShotsRemaining <= 0)
            {
                skillRuntime.ShotsRemaining = 0;
                skillRuntime.ReloadDuration = ResolveManifestedReloadDuration(runtime, skillRuntime.Skill);
                skillRuntime.ReloadRemaining = skillRuntime.ReloadDuration;
            }

            statusLabel = $"{runtime.Monster.DisplayName} Three-Sword Flurry queued.";
            return true;
        }

        private bool TryCastVegaUnitSilentGreatblade(CombatUnitRuntime runtime, CombatSkillRuntime skillRuntime)
        {
            var skill = skillRuntime != null ? skillRuntime.Skill : null;
            if (!IsVegaCombatUnit(runtime) || skill == null || skillRuntime.CooldownRemaining > 0f)
            {
                return false;
            }

            var target = FindNearestManifestedMonsterTarget(runtime.Transform.position);
            if (target == null || target.Transform == null)
            {
                skillRuntime.CooldownRemaining = 0.25f;
                return false;
            }

            var damageMultiplier = 1f;
            if (HasVegaUnitChoice(runtime, "vega-b-trait-1"))
            {
                damageMultiplier *= 1.25f;
            }

            if (HasVegaUnitChoice(runtime, "vega-b-master-2"))
            {
                damageMultiplier *= 1.70f;
            }

            var silenceDuration = VegaSilentGreatbladeSilenceDuration
                + (HasVegaUnitChoice(runtime, "vega-b-trait-2") ? 1f : 0f)
                + (HasVegaUnitPassive(runtime, "vega-g") && HasVegaUnitChoice(runtime, "vega-g-trait-2") ? 1f : 0f);
            var center = target.Transform.position;
            var hitCount = ApplyVegaUnitTargetRectangleSlash(
                runtime,
                center,
                VegaSilentGreatbladeAreaWidth,
                VegaSilentGreatbladeAreaHeight,
                GetVegaUnitSkillBaseDamage(runtime, skill),
                damageMultiplier,
                "vega-b",
                silenceDuration,
                GetVegaUnitSilentGreatbladeNameMarkStacks(runtime),
                skill.SkillEffectPrefab);
            if (HasVegaUnitChoice(runtime, "vega-b-master-1"))
            {
                hitCount += ApplyVegaUnitTargetRectangleSlash(
                    runtime,
                    center,
                    VegaSilentGreatbladeAreaWidth,
                    VegaSilentGreatbladeAreaHeight,
                    GetVegaUnitSkillBaseDamage(runtime, skill) * 0.45f,
                    1f,
                    "vega-b-second",
                    1f,
                    0,
                    skill.SkillEffectPrefab);
            }

            skillRuntime.CooldownDuration = GetVegaUnitCooldown(runtime, skill, 8f, GetVegaUnitSilentGreatbladeCooldownMultiplier(runtime));
            skillRuntime.CooldownRemaining = skillRuntime.CooldownDuration;
            statusLabel = $"{runtime.Monster.DisplayName} Silent Greatblade hit {hitCount} enemy(s).";
            return hitCount > 0;
        }

        private bool TryCastVegaUnitExterminationPermit(CombatUnitRuntime runtime, CombatSkillRuntime skillRuntime)
        {
            var skill = skillRuntime != null ? skillRuntime.Skill : null;
            if (!IsVegaCombatUnit(runtime) || skill == null || skillRuntime.CooldownRemaining > 0f)
            {
                return false;
            }

            var duration = VegaExterminationPermitDuration + (HasVegaUnitChoice(runtime, "vega-c-trait-1") ? 2f : 0f);
            if (HasVegaUnitPassive(runtime, "vega-h"))
            {
                duration *= 1.20f;
            }

            runtime.VegaExterminationActionSpeedBonus = 0.25f
                + (HasVegaUnitChoice(runtime, "vega-c-trait-2") ? 0.10f : 0f)
                + (HasVegaUnitPassive(runtime, "vega-h") ? 0.12f : 0f)
                + (HasVegaUnitPassive(runtime, "vega-h") && HasVegaUnitChoice(runtime, "vega-h-trait-1") ? 0.06f : 0f);
            runtime.VegaExterminationAttackBonus = 0.20f
                + (HasVegaUnitChoice(runtime, "vega-c-trait-3") ? 0.10f : 0f)
                + (HasVegaUnitPassive(runtime, "vega-h") && HasVegaUnitChoice(runtime, "vega-h-trait-2") ? 0.08f : 0f);
            if (HasVegaUnitChoice(runtime, "vega-c-master-1"))
            {
                duration += 2f;
            }

            if (HasVegaUnitChoice(runtime, "vega-c-master-2"))
            {
                runtime.VegaExterminationActionSpeedBonus += 0.25f;
                runtime.VegaExterminationAttackBonus += 0.25f;
                duration -= 2f;
            }

            runtime.VegaExterminationPermitTimer = Mathf.Max(runtime.VegaExterminationPermitTimer, Mathf.Max(1f, duration));
            skillRuntime.CooldownDuration = GetVegaUnitCooldown(runtime, skill, 14f, 1f);
            skillRuntime.CooldownRemaining = skillRuntime.CooldownDuration;
            CreateVegaAreaVisual("VegaUnitExterminationPermit", runtime.Transform.position, 0.9f, 0.3f, new Color(0.72f, 0.62f, 1f, 0.48f), 25, skill.SkillEffectPrefab);
            statusLabel = $"{runtime.Monster.DisplayName} Extermination Permit active for {runtime.VegaExterminationPermitTimer:0.#}s.";
            return true;
        }

        private bool TryCastVegaUnitBlackLedgerRelease(CombatUnitRuntime runtime, CombatSkillRuntime skillRuntime)
        {
            var skill = skillRuntime != null ? skillRuntime.Skill : null;
            if (!IsVegaCombatUnit(runtime) || skill == null || skillRuntime.CooldownRemaining > 0f)
            {
                return false;
            }

            var markedTargets = GetVegaMarkedEnemies();
            if (markedTargets.Count == 0)
            {
                return false;
            }

            var radius = (skill.Radius > 0f ? skill.Radius : VegaBlackLedgerRadius) * (HasVegaUnitChoice(runtime, "vega-d-trait-2") ? 1.25f : 1f);
            var damageMultiplier = HasVegaUnitChoice(runtime, "vega-d-trait-1") ? 1.25f : 1f;
            var slashCount = 1;
            if (HasVegaUnitChoice(runtime, "vega-d-master-1"))
            {
                slashCount = 2;
                damageMultiplier *= 0.65f;
            }

            if (HasVegaUnitChoice(runtime, "vega-d-master-2"))
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
                if (HasVegaUnitChoice(runtime, "vega-d-trait-4") && markedTarget.VegaNameMarkStacks >= 10)
                {
                    centerMultiplier *= 1.30f;
                }

                for (var i = 0; i < slashCount; i++)
                {
                    hitCount += ApplyVegaUnitAreaSlash(
                        runtime,
                        markedTarget.Transform.position,
                        radius,
                        GetVegaUnitSkillBaseDamage(runtime, skill),
                        centerMultiplier,
                        "vega-d",
                        HasVegaUnitChoice(runtime, "vega-d-trait-5") ? 1 : 0,
                        skill.SkillEffectPrefab);
                }
            }

            skillRuntime.CooldownDuration = GetVegaUnitCooldown(runtime, skill, 11f, GetVegaUnitBlackLedgerCooldownMultiplier(runtime));
            skillRuntime.CooldownRemaining = skillRuntime.CooldownDuration;
            if (hitCount > 0)
            {
                TrackVegaUnitAreaDamageDealt(runtime);
            }

            statusLabel = $"{runtime.Monster.DisplayName} Black Ledger Release hit {hitCount} enemy instance(s).";
            return hitCount > 0;
        }

        private bool TryCastVegaUnitFinalSentence(CombatUnitRuntime runtime, CombatSkillRuntime skillRuntime)
        {
            var skill = skillRuntime != null ? skillRuntime.Skill : null;
            if (!IsVegaCombatUnit(runtime) || skill == null || skillRuntime.CooldownRemaining > 0f)
            {
                return false;
            }

            var target = FindVegaHighestMarkedEnemy();
            if (target == null)
            {
                return false;
            }

            var stacks = Mathf.Max(0, target.VegaNameMarkStacks);
            var baseDamage = GetVegaUnitSkillBaseDamage(runtime, skill) * (HasVegaUnitChoice(runtime, "vega-e-trait-1") ? 1.25f : 1f);
            if (HasVegaUnitChoice(runtime, "vega-e-master-2"))
            {
                baseDamage *= 0.80f;
            }

            var stackDamage = stacks * GetVegaUnitFinalSentenceStackDamage(runtime);
            var consumedStacks = Mathf.FloorToInt(stacks * (HasVegaUnitChoice(runtime, "vega-e-master-1") ? 1f : 0.50f));
            var wasAlive = target.CurrentHealth > 0f;
            var applied = ApplyVegaUnitSkillDamage(runtime, target, baseDamage + stackDamage, 1f, "vega-e");
            target.VegaNameMarkStacks = Mathf.Max(0, target.VegaNameMarkStacks - consumedStacks);
            CreateVegaAreaVisual("VegaUnitFinalSentence", target.Transform.position, Mathf.Max(0.9f, GetEnemyHitRadius(target) + 0.35f), 0.28f, new Color(0.72f, 0.62f, 1f, 0.55f), 26, skill.SkillEffectPrefab);

            skillRuntime.CooldownDuration = GetVegaUnitCooldown(runtime, skill, 15f, HasVegaUnitChoice(runtime, "vega-e-trait-3") ? 0.80f : 1f);
            skillRuntime.CooldownRemaining = skillRuntime.CooldownDuration;
            if (wasAlive && target.CurrentHealth <= 0f)
            {
                ChargeVegaUnitCooldownsAfterFinalSentenceKill(runtime);
                if (HasVegaUnitChoice(runtime, "vega-e-trait-5"))
                {
                    DistributeVegaUnitNameMarks(runtime, target, Mathf.FloorToInt(consumedStacks * 0.25f), VegaFinalSentenceDistributeRadius);
                }

                if (HasVegaUnitChoice(runtime, "vega-e-master-2"))
                {
                    skillRuntime.CooldownRemaining *= 0.30f;
                }

                if (HasVegaUnitPassive(runtime, "vega-j") && HasVegaUnitChoice(runtime, "vega-j-trait-3"))
                {
                    ReduceVegaUnitBlackLedgerCooldownByRatio(runtime, 0.20f);
                }
            }
            else if (target.CurrentHealth > 0f)
            {
                ApplyVegaUnitFinalSentenceVulnerability(runtime, target);
            }

            statusLabel = $"{runtime.Monster.DisplayName} Final Sentence hit {target.DisplayName} for {applied:0.0} and consumed {consumedStacks} mark(s).";
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

        private int ApplyVegaUnitTargetRectangleSlash(CombatUnitRuntime runtime, Vector3 center, float width, float height, float baseDamage, float damageMultiplier, string skillId, float silenceDuration, int markStacks, GameObject effectPrefab = null)
        {
            CreateVegaRectangleVisual("VegaUnitSilentGreatbladeArea", center, width, height, 0.24f, new Color(0.70f, 0.62f, 1f, 0.58f), 25, effectPrefab);
            var hitCount = 0;
            for (var i = 0; i < enemies.Count; i++)
            {
                var enemy = enemies[i];
                if (enemy == null || enemy.CurrentHealth <= 0f || enemy.Transform == null || !IsPointInsideVegaRectangle(enemy.Transform.position, center, width, height, GetEnemyHitRadius(enemy)))
                {
                    continue;
                }

                ApplyVegaUnitSkillDamage(runtime, enemy, baseDamage, damageMultiplier, skillId);
                ApplyVegaSilence(enemy, silenceDuration + (HasVegaUnitChoice(runtime, "vega-b-master-2") && enemy.VegaNameMarkStacks >= 10 ? 1f : 0f));
                AddVegaNameMarks(enemy, markStacks);
                hitCount += 1;
            }

            return hitCount;
        }

        private int ApplyVegaUnitAreaSlash(CombatUnitRuntime runtime, Vector3 center, float radius, float baseDamage, float damageMultiplier, string skillId, int markStacks, GameObject effectPrefab = null)
        {
            CreateVegaAreaVisual("VegaUnitBlackLedgerSlash", center, radius, 0.24f, new Color(0.48f, 0.38f, 0.86f, 0.42f), 24, effectPrefab);
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

                var applied = ApplyVegaUnitSkillDamage(runtime, enemy, baseDamage, damageMultiplier, skillId);
                AddVegaNameMarks(enemy, markStacks);
                if (applied > 0f)
                {
                    ApplyVegaUnitBlackLedgerAreaVulnerability(runtime, enemy);
                }

                hitCount += 1;
            }

            return hitCount;
        }

        private float ApplyVegaUnitSkillDamage(CombatUnitRuntime runtime, EnemyRuntime enemy, float baseDamage, float finalMultiplier, string skillId)
        {
            if (!IsVegaCombatUnit(runtime) || enemy == null || enemy.CurrentHealth <= 0f || baseDamage <= 0f)
            {
                return 0f;
            }

            var result = DamageCalculator.Resolve(
                baseDamage,
                DamageAttribute.Physical,
                enemy.Defenses,
                flatDefenseReduction: GetVegaUnitFlatDefenseReduction(runtime, enemy, DamageAttribute.Physical),
                criticalChanceBonus: GetVegaUnitCriticalChanceBonus(runtime, enemy, DamageAttribute.Physical, skillId),
                targetCriticalResistance: enemy.CriticalResistance,
                finalDamageMultiplier: enemy.DamageTakenMultiplier * Mathf.Max(0f, finalMultiplier) * GetVegaUnitFinalDamageMultiplier(runtime, enemy, DamageAttribute.Physical, skillId));
            var applied = ApplyDamageToEnemy(enemy, result.FinalDamage, DamageAttribute.Physical);
            enemy.FlashTimer = 0.08f;
            Debug.Log($"[CombatDamage] {runtime.Monster.DisplayName}.{skillId} -> {enemy.DisplayName}: {result.FormulaLog}; Applied={applied:0.##}, ShieldLeft={enemy.ShieldValue:0.##}, HpLeft={Mathf.Max(0f, enemy.CurrentHealth):0.##}");
            return applied;
        }

        private bool TryApplyVegaUnitProjectileHit(ProjectileRuntime projectile, EnemyRuntime enemy, out DamageResult damageResult, out float appliedDamage)
        {
            damageResult = default;
            appliedDamage = 0f;
            var runtime = projectile != null ? projectile.ManifestedSource : null;
            if (!IsVegaCombatUnit(runtime)
                || projectile == null
                || enemy == null
                || enemy.CurrentHealth <= 0f
                || !IsVegaProjectileSkill(projectile.SkillId))
            {
                return false;
            }

            var wasAlive = enemy.CurrentHealth > 0f;
            damageResult = DamageCalculator.Resolve(
                projectile.BaseDamage,
                projectile.Attribute,
                enemy.Defenses,
                flatDefenseReduction: GetVegaUnitFlatDefenseReduction(runtime, enemy, projectile.Attribute),
                criticalChanceBonus: GetVegaUnitCriticalChanceBonus(runtime, enemy, projectile.Attribute, projectile.SkillId),
                targetCriticalResistance: enemy.CriticalResistance,
                finalDamageMultiplier: enemy.DamageTakenMultiplier * GetVegaUnitFinalDamageMultiplier(runtime, enemy, projectile.Attribute, projectile.SkillId));
            appliedDamage = ApplyDamageToEnemy(enemy, damageResult.FinalDamage, damageResult.Attribute);
            enemy.FlashTimer = 0.08f;
            if (appliedDamage > 0f && wasAlive && enemy.CurrentHealth <= 0f && HasVegaUnitChoice(runtime, "vega-a-master-2"))
            {
                TransferVegaUnitCondemnationMark(runtime, enemy, 3);
            }

            return true;
        }

        private void TransferVegaUnitCondemnationMark(CombatUnitRuntime runtime, EnemyRuntime sourceEnemy, int stacks)
        {
            if (!IsVegaCombatUnit(runtime) || sourceEnemy == null || sourceEnemy.Transform == null)
            {
                return;
            }

            var target = FindNearestEnemy(sourceEnemy.Transform.position, GetVegaMapWideSkillRange(), enemy => enemy != sourceEnemy);
            if (target == null)
            {
                return;
            }

            AddVegaNameMarks(target, stacks);
            ApplyVegaUnitSkillDamage(runtime, target, GetVegaUnitSkillBaseDamage(runtime, FindVegaUnitSkill(runtime, SkillSlot.A)) * 0.35f, 1f, "vega-a-chain");
            CreateVegaLineVisual("VegaUnitCondemnationChain", sourceEnemy.Transform.position, (target.Transform.position - sourceEnemy.Transform.position).normalized, Vector2.Distance(sourceEnemy.Transform.position, target.Transform.position), 0.32f, 0.2f, new Color(0.72f, 0.62f, 1f, 0.58f), 26);
        }

        private void DistributeVegaUnitNameMarks(CombatUnitRuntime runtime, EnemyRuntime sourceEnemy, int totalStacks, float radius)
        {
            if (!IsVegaCombatUnit(runtime) || sourceEnemy == null || sourceEnemy.Transform == null || totalStacks <= 0)
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

        private float GetVegaUnitSkillBaseDamage(CombatUnitRuntime runtime, SkillDefinition skill)
        {
            if (!IsVegaCombatUnit(runtime) || skill == null)
            {
                return 0f;
            }

            var effectivePower = runtime.PowerStat * (1f + (runtime.VegaExterminationPermitTimer > 0f ? runtime.VegaExterminationAttackBonus : 0f));
            return skill.BaseDamage + effectivePower * skill.AttackPowerCoefficient;
        }

        private float GetVegaUnitThreeSwordDamageMultiplier(CombatUnitRuntime runtime, bool isLastShot)
        {
            var multiplier = isLastShot ? 2f : 1f;
            if (HasVegaUnitChoice(runtime, "vega-a-trait-1"))
            {
                multiplier *= 1.20f;
            }

            if (isLastShot && HasVegaUnitChoice(runtime, "vega-a-trait-4"))
            {
                multiplier += 0.50f;
            }

            return multiplier;
        }

        private int GetVegaUnitThreeSwordNameMarkStacks(CombatUnitRuntime runtime)
        {
            return 1
                + (runtime != null && runtime.VegaExterminationPermitTimer > 0f && HasVegaUnitChoice(runtime, "vega-c-master-1") ? 1 : 0)
                + (HasVegaUnitPassive(runtime, "vega-f") && HasVegaUnitChoice(runtime, "vega-f-trait-3") ? 1 : 0);
        }

        private int GetVegaUnitSilentGreatbladeNameMarkStacks(CombatUnitRuntime runtime)
        {
            return (HasVegaUnitPassive(runtime, "vega-g") ? 2 : 0) + (HasVegaUnitChoice(runtime, "vega-b-trait-5") ? 2 : 0);
        }

        private float GetVegaUnitSilentGreatbladeCooldownMultiplier(CombatUnitRuntime runtime)
        {
            return HasVegaUnitChoice(runtime, "vega-b-trait-3") ? 0.80f : 1f;
        }

        private float GetVegaUnitBlackLedgerCooldownMultiplier(CombatUnitRuntime runtime)
        {
            var multiplier = HasVegaUnitChoice(runtime, "vega-d-trait-3") ? 0.80f : 1f;
            if (HasVegaUnitChoice(runtime, "vega-d-master-2"))
            {
                multiplier *= 1.20f;
            }

            return multiplier;
        }

        private float GetVegaUnitFinalSentenceStackDamage(CombatUnitRuntime runtime)
        {
            var multiplier = 1f
                + (HasVegaUnitChoice(runtime, "vega-e-trait-2") ? 0.25f : 0f)
                + (HasVegaUnitChoice(runtime, "vega-e-master-1") ? 0.80f : 0f);
            return (6f + (runtime != null ? runtime.PowerStat : 0f) * 0.18f) * multiplier;
        }

        private float GetVegaUnitCooldown(CombatUnitRuntime runtime, SkillDefinition skill, float fallback, float multiplier)
        {
            var cooldown = skill != null && skill.CooldownSeconds > 0f ? skill.CooldownSeconds : fallback;
            return Mathf.Max(0.1f, cooldown * Mathf.Max(0.05f, multiplier));
        }

        private float GetVegaUnitActionSpeedMultiplier(CombatUnitRuntime runtime)
        {
            return 1f + (runtime != null && runtime.VegaExterminationPermitTimer > 0f ? runtime.VegaExterminationActionSpeedBonus : 0f);
        }

        private float GetVegaUnitFinalDamageMultiplier(CombatUnitRuntime runtime, EnemyRuntime enemy, DamageAttribute attribute, string skillId)
        {
            if (!IsVegaCombatUnit(runtime) || enemy == null || attribute != DamageAttribute.Physical)
            {
                return 1f;
            }

            var multiplier = 1f;
            if (runtime.VegaExterminationPermitTimer > 0f && HasVegaUnitChoice(runtime, "vega-c-trait-5") && enemy.VegaNameMarkStacks > 0)
            {
                multiplier *= 1.15f;
            }

            if (HasVegaUnitPassive(runtime, "vega-f") && enemy.VegaNameMarkStacks > 0)
            {
                multiplier *= 1f + 0.10f + (HasVegaUnitChoice(runtime, "vega-f-trait-1") ? 0.05f : 0f);
            }

            if (HasVegaUnitPassive(runtime, "vega-g") && enemy.VegaSilenceTimer > 0f)
            {
                multiplier *= 1f + 0.14f + (HasVegaUnitChoice(runtime, "vega-g-trait-1") ? 0.06f : 0f);
            }

            if (runtime.VegaExterminationPermitTimer > 0f
                && HasVegaUnitPassive(runtime, "vega-h")
                && HasVegaUnitChoice(runtime, "vega-h-trait-3")
                && enemy.VegaNameMarkStacks > 0)
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
                && HasVegaUnitChoice(runtime, "vega-a-trait-5")
                && enemy.VegaNameMarkStacks >= 10)
            {
                multiplier *= 1.25f;
            }

            return multiplier;
        }

        private float GetVegaUnitFlatDefenseReduction(CombatUnitRuntime runtime, EnemyRuntime enemy, DamageAttribute attribute)
        {
            if (!IsVegaCombatUnit(runtime) || enemy == null || attribute != DamageAttribute.Physical)
            {
                return 0f;
            }

            var reduction = 0f;
            if (HasVegaUnitPassive(runtime, "vega-f") && enemy.VegaNameMarkStacks >= 10)
            {
                reduction += 8f + (HasVegaUnitChoice(runtime, "vega-f-trait-2") ? 4f : 0f);
            }

            return reduction;
        }

        private float GetVegaUnitCriticalChanceBonus(CombatUnitRuntime runtime, EnemyRuntime enemy, DamageAttribute attribute, string skillId)
        {
            if (!IsVegaCombatUnit(runtime) || enemy == null || attribute != DamageAttribute.Physical)
            {
                return 0f;
            }

            var bonus = 0f;
            if (string.Equals(skillId, "vega-e", StringComparison.OrdinalIgnoreCase)
                && HasVegaUnitChoice(runtime, "vega-e-trait-4")
                && enemy.VegaNameMarkStacks >= 20)
            {
                bonus += 0.35f;
            }

            if (HasVegaUnitPassive(runtime, "vega-g")
                && HasVegaUnitChoice(runtime, "vega-g-trait-3")
                && enemy.VegaSilenceTimer > 0f
                && enemy.VegaNameMarkStacks > 0)
            {
                bonus += 0.10f;
            }

            return bonus;
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

        private static bool IsVegaCombatUnit(CombatUnitRuntime runtime)
        {
            return runtime != null
                && runtime.Monster != null
                && string.Equals(runtime.Monster.MonsterId, "vega", StringComparison.OrdinalIgnoreCase);
        }

        private bool HasVegaUnitChoice(CombatUnitRuntime runtime, string choiceId)
        {
            return IsSelectedCombatUnit(runtime) ? HasChoice(choiceId) : HasManifestedChoice(runtime, choiceId);
        }

        private bool HasVegaUnitPassive(CombatUnitRuntime runtime, string passiveId)
        {
            if (IsSelectedCombatUnit(runtime))
            {
                return string.Equals(passiveId, "vega-f", StringComparison.OrdinalIgnoreCase) && HasVegaDeepEngraving()
                    || string.Equals(passiveId, "vega-g", StringComparison.OrdinalIgnoreCase) && HasVegaSealingSwordForm()
                    || string.Equals(passiveId, "vega-h", StringComparison.OrdinalIgnoreCase) && HasVegaExecutionPrep()
                    || string.Equals(passiveId, "vega-i", StringComparison.OrdinalIgnoreCase) && HasVegaChainCleaving()
                    || string.Equals(passiveId, "vega-j", StringComparison.OrdinalIgnoreCase) && HasVegaExecutioner();
            }

            return HasManifestedPassive(runtime, passiveId);
        }

        private SkillDefinition FindVegaUnitSkill(CombatUnitRuntime runtime, SkillSlot slot)
        {
            if (runtime == null)
            {
                return null;
            }

            for (var i = 0; i < runtime.Skills.Count; i++)
            {
                var skill = runtime.Skills[i] != null ? runtime.Skills[i].Skill : null;
                if (skill != null && skill.Slot == slot)
                {
                    return skill;
                }
            }

            return null;
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

        private void ApplyVegaUnitBlackLedgerAreaVulnerability(CombatUnitRuntime runtime, EnemyRuntime enemy)
        {
            if (!HasVegaUnitPassive(runtime, "vega-i") || enemy == null)
            {
                return;
            }

            enemy.VegaBlackLedgerAreaVulnerabilityBonus = Mathf.Max(
                enemy.VegaBlackLedgerAreaVulnerabilityBonus,
                0.15f
                + (HasVegaUnitChoice(runtime, "vega-i-trait-1") ? 0.07f : 0f)
                + (HasVegaUnitChoice(runtime, "vega-i-trait-3") && enemy.VegaNameMarkStacks >= 10 ? 0.10f : 0f));
            enemy.VegaBlackLedgerAreaVulnerabilityTimer = Mathf.Max(
                enemy.VegaBlackLedgerAreaVulnerabilityTimer,
                4f + (HasVegaUnitChoice(runtime, "vega-i-trait-2") ? 2f : 0f));
        }

        private void ApplyVegaUnitFinalSentenceVulnerability(CombatUnitRuntime runtime, EnemyRuntime enemy)
        {
            if (!HasVegaUnitPassive(runtime, "vega-j") || enemy == null)
            {
                return;
            }

            enemy.VegaFinalSentenceVulnerabilityBonus = Mathf.Max(
                enemy.VegaFinalSentenceVulnerabilityBonus,
                0.10f + (HasVegaUnitChoice(runtime, "vega-j-trait-2") ? 0.05f : 0f));
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

        private void TrackVegaUnitAreaDamageDealt(CombatUnitRuntime runtime)
        {
            if (!HasVegaUnitPassive(runtime, "vega-i") || runtime.VegaBlackLedgerAreaChargeCooldownRemaining > 0f)
            {
                return;
            }

            ReduceVegaUnitBlackLedgerCooldownByRatio(runtime, 0.03f);
            runtime.VegaBlackLedgerAreaChargeCooldownRemaining = 1f;
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

        private void ChargeVegaUnitCooldownsAfterFinalSentenceKill(CombatUnitRuntime runtime)
        {
            if (!HasVegaUnitPassive(runtime, "vega-j"))
            {
                return;
            }

            var ratio = 0.20f + (HasVegaUnitChoice(runtime, "vega-j-trait-1") ? 0.10f : 0f);
            ReduceVegaUnitCooldownByRatio(runtime, SkillSlot.B, ratio);
            ReduceVegaUnitCooldownByRatio(runtime, SkillSlot.C, ratio);
            ReduceVegaUnitCooldownByRatio(runtime, SkillSlot.D, ratio);
            ReduceVegaUnitCooldownByRatio(runtime, SkillSlot.E, ratio);
            ReduceVegaUnitReloadByRatio(runtime, "vega-a", ratio);
        }

        private void ReduceVegaBlackLedgerCooldownByRatio(float ratio)
        {
            ReduceVegaCooldownByRatio(ref vegaBlackLedgerCooldownRemaining, GetVegaCooldown(FindSelectedSkill(SkillSlot.D), 11f, GetVegaBlackLedgerCooldownMultiplier()), ratio);
        }

        private void ReduceVegaUnitBlackLedgerCooldownByRatio(CombatUnitRuntime runtime, float ratio)
        {
            ReduceVegaUnitCooldownByRatio(runtime, SkillSlot.D, ratio);
        }

        private static void ReduceVegaUnitCooldownByRatio(CombatUnitRuntime runtime, SkillSlot slot, float ratio)
        {
            if (runtime == null || ratio <= 0f)
            {
                return;
            }

            for (var i = 0; i < runtime.Skills.Count; i++)
            {
                var skillRuntime = runtime.Skills[i];
                if (skillRuntime == null || skillRuntime.Skill == null || skillRuntime.Skill.Slot != slot)
                {
                    continue;
                }

                skillRuntime.CooldownRemaining = Mathf.Max(0f, skillRuntime.CooldownRemaining - skillRuntime.CooldownDuration * Mathf.Clamp01(ratio));
            }
        }

        private static void ReduceVegaUnitReloadByRatio(CombatUnitRuntime runtime, string skillId, float ratio)
        {
            if (runtime == null || string.IsNullOrWhiteSpace(skillId) || ratio <= 0f)
            {
                return;
            }

            for (var i = 0; i < runtime.Skills.Count; i++)
            {
                var skillRuntime = runtime.Skills[i];
                if (skillRuntime == null
                    || skillRuntime.Skill == null
                    || !string.Equals(skillRuntime.Skill.SkillId, skillId, StringComparison.OrdinalIgnoreCase)
                    || skillRuntime.ReloadRemaining <= 0f)
                {
                    continue;
                }

                skillRuntime.ReloadRemaining = Mathf.Max(0f, skillRuntime.ReloadRemaining - skillRuntime.ReloadDuration * Mathf.Clamp01(ratio));
            }
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
