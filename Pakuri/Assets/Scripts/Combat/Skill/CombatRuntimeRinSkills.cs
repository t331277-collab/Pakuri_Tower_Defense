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
            TickSelectedRinUnitSkillRuntimes(elapsed);
            var howlingRuntime = FindCombatSkillRuntime(selectedUnitRuntime, "rin-b");
            rinHowlingCooldownRemaining = howlingRuntime != null
                ? howlingRuntime.CooldownRemaining
                : Mathf.Max(0f, rinHowlingCooldownRemaining - elapsed);
            var shockwaveRuntime = FindCombatSkillRuntime(selectedUnitRuntime, "rin-c");
            rinShockwaveCooldownRemaining = shockwaveRuntime != null
                ? shockwaveRuntime.CooldownRemaining
                : Mathf.Max(0f, rinShockwaveCooldownRemaining - elapsed);
            var finishingBlowRuntime = FindCombatSkillRuntime(selectedUnitRuntime, "rin-d");
            rinFinishingBlowCooldownRemaining = finishingBlowRuntime != null
                ? finishingBlowRuntime.CooldownRemaining
                : Mathf.Max(0f, rinFinishingBlowCooldownRemaining - elapsed);
            var collapseStrikeRuntime = FindCombatSkillRuntime(selectedUnitRuntime, "rin-e");
            rinCollapseStrikeCooldownRemaining = collapseStrikeRuntime != null
                ? collapseStrikeRuntime.CooldownRemaining
                : Mathf.Max(0f, rinCollapseStrikeCooldownRemaining - elapsed);
            rinWaveAmplificationCooldownRemaining = Mathf.Max(0f, rinWaveAmplificationCooldownRemaining - Time.deltaTime);
        }

        private void TickSelectedRinUnitSkillRuntimes(float elapsed)
        {
            if (!IsRinCombatUnit(selectedUnitRuntime))
            {
                return;
            }

            SyncManifestedLearnedSkills(selectedUnitRuntime);
            for (var i = 0; i < selectedUnitRuntime.Skills.Count; i++)
            {
                var skillRuntime = selectedUnitRuntime.Skills[i];
                if (skillRuntime == null || skillRuntime.Skill == null)
                {
                    continue;
                }

                skillRuntime.Tick(elapsed);
                skillRuntime.TickReload(elapsed, ResolveManifestedMagazineCapacity(selectedUnitRuntime, skillRuntime.Skill));
            }
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

            var castAny = TryTriggerRinUnitAutomaticSkills(selectedUnitRuntime);

            if (!castAny)
            {
                statusLabel = $"{selectedMonsterName}: no Rin active skill is ready.";
            }

            return true;
        }

        private bool TryTriggerRinUnitAutomaticSkills(CombatUnitRuntime runtime)
        {
            if (!IsRinCombatUnit(runtime))
            {
                return false;
            }

            SyncManifestedLearnedSkills(runtime);
            var castAny = false;
            castAny |= TryCastRinUnitHowling(runtime, FindCombatSkillRuntime(runtime, "rin-b"));
            castAny |= TryCastRinUnitShockwave(runtime, FindCombatSkillRuntime(runtime, "rin-c"));
            castAny |= TryCastRinUnitFinishingBlow(runtime, FindCombatSkillRuntime(runtime, "rin-d"));
            castAny |= TryCastRinUnitCollapseStrike(runtime, FindCombatSkillRuntime(runtime, "rin-e"));

            return castAny;
        }

        private bool IsRinCombatUnit(CombatUnitRuntime runtime)
        {
            return runtime != null
                && runtime.Monster != null
                && string.Equals(runtime.Monster.MonsterId, "rin", StringComparison.OrdinalIgnoreCase)
                && runtime.Transform != null
                && runtime.CurrentHealth > 0f;
        }

        private bool HasRinUnitPassive(CombatUnitRuntime runtime, string passiveId)
        {
            if (IsSelectedCombatUnit(runtime))
            {
                return string.Equals(passiveId, "rin-f", StringComparison.OrdinalIgnoreCase) && HasRinAmbidextrous()
                    || string.Equals(passiveId, "rin-g", StringComparison.OrdinalIgnoreCase) && HasRinBattleResonance()
                    || string.Equals(passiveId, "rin-h", StringComparison.OrdinalIgnoreCase) && HasRinWaveAmplification()
                    || string.Equals(passiveId, "rin-i", StringComparison.OrdinalIgnoreCase) && HasRinFinisherInstinct()
                    || string.Equals(passiveId, "rin-j", StringComparison.OrdinalIgnoreCase) && HasRinCollapseAftermath();
            }

            return HasManifestedPassive(runtime, passiveId);
        }

        private static SkillDefinition FindRinUnitSkill(CombatUnitRuntime runtime, string skillId)
        {
            var skillRuntime = FindCombatSkillRuntime(runtime, skillId);
            return skillRuntime != null ? skillRuntime.Skill : null;
        }

        private static void ReduceRinUnitSkillCooldown(CombatUnitRuntime runtime, string skillId, float amount)
        {
            var skillRuntime = FindCombatSkillRuntime(runtime, skillId);
            if (skillRuntime == null || amount <= 0f)
            {
                return;
            }

            skillRuntime.CooldownRemaining = Mathf.Max(0f, skillRuntime.CooldownRemaining - amount);
        }

        private bool TryTickRinUnitSkill(CombatUnitRuntime runtime, CombatSkillRuntime skillRuntime, float elapsed)
        {
            if (!IsRinCombatUnit(runtime) || skillRuntime == null || skillRuntime.Skill == null)
            {
                return false;
            }

            if (string.Equals(skillRuntime.Skill.SkillId, "rin-a", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            TickCombatSkillRuntime(runtime, skillRuntime, elapsed * GetRinUnitActionSpeedMultiplier(runtime));
            if (skillRuntime.CooldownRemaining > 0f)
            {
                return true;
            }

            var skillId = skillRuntime.Skill.SkillId;
            if (string.Equals(skillId, "rin-b", StringComparison.OrdinalIgnoreCase))
            {
                if (!TryCastRinUnitHowling(runtime, skillRuntime))
                {
                    skillRuntime.CooldownRemaining = 0.25f;
                }
            }
            else if (string.Equals(skillId, "rin-c", StringComparison.OrdinalIgnoreCase))
            {
                if (!TryCastRinUnitShockwave(runtime, skillRuntime))
                {
                    skillRuntime.CooldownRemaining = 0.25f;
                }
            }
            else if (string.Equals(skillId, "rin-d", StringComparison.OrdinalIgnoreCase))
            {
                if (!TryCastRinUnitFinishingBlow(runtime, skillRuntime))
                {
                    skillRuntime.CooldownRemaining = 0.25f;
                }
            }
            else if (string.Equals(skillId, "rin-e", StringComparison.OrdinalIgnoreCase))
            {
                if (!TryCastRinUnitCollapseStrike(runtime, skillRuntime))
                {
                    skillRuntime.CooldownRemaining = 0.25f;
                }
            }
            else
            {
                return false;
            }

            return true;
        }

        private bool TryCastRinUnitHowling(CombatUnitRuntime runtime, CombatSkillRuntime skillRuntime)
        {
            if (!IsRinCombatUnit(runtime)
                || skillRuntime == null
                || skillRuntime.Skill == null
                || skillRuntime.CooldownRemaining > 0f)
            {
                return false;
            }

            var skill = skillRuntime.Skill;
            var duration = RinHowlingDuration;
            if (HasRinUnitChoice(runtime, "rin-b-trait-1"))
            {
                duration *= 1.25f;
            }

            if (HasRinUnitChoice(runtime, "rin-b-master-1"))
            {
                duration *= 1.20f;
            }

            if (IsSelectedCombatUnit(runtime))
            {
                rinHowlingTimer = Mathf.Max(rinHowlingTimer, duration);
                if (HasRinBattleResonance() && HasRinUnitChoice(runtime, "rin-g-trait-3") && reloadRemaining > 0f)
                {
                    reloadRemaining *= 0.75f;
                }
            }
            else
            {
                runtime.RinHowlingTimer = Mathf.Max(runtime.RinHowlingTimer, duration);
            }

            var effect = CreateCircleEffect("RinHowling", runtime.Transform.position, 2.2f, 0.45f, skill.SkillEffectPrefab);
            effect.SkillId = "rin-b";
            if (effect.Renderer != null)
            {
                effect.Renderer.color = new Color(1f, 0.82f, 0.42f, 0.42f);
                effect.Renderer.sortingOrder = 24;
            }

            AddBattlefieldSkillEffect(effect);
            skillRuntime.CooldownDuration = GetRinCooldown(skill, 12f, HasRinUnitChoice(runtime, "rin-b-trait-3") ? 0.80f : 1f);
            skillRuntime.CooldownRemaining = skillRuntime.CooldownDuration;
            if (IsSelectedCombatUnit(runtime))
            {
                rinHowlingCooldownRemaining = skillRuntime.CooldownRemaining;
            }

            statusLabel = $"{runtime.Monster.DisplayName} Howling active for {duration:0.#}s.";
            return true;
        }

        private bool TryCastRinUnitShockwave(CombatUnitRuntime runtime, CombatSkillRuntime skillRuntime)
        {
            if (!IsRinCombatUnit(runtime)
                || skillRuntime == null
                || skillRuntime.Skill == null
                || skillRuntime.CooldownRemaining > 0f)
            {
                return false;
            }

            var target = FindNearestEnemy(runtime.Transform.position, GetRinMapWideSkillRange());
            if (target == null || !TryFireManifestedRinShockwave(runtime, skillRuntime, target))
            {
                return false;
            }

            skillRuntime.CooldownDuration = GetRinCooldown(skillRuntime.Skill, 5.5f, HasRinUnitChoice(runtime, "rin-c-trait-4") ? 0.80f : 1f);
            skillRuntime.CooldownRemaining = skillRuntime.CooldownDuration;
            if (IsSelectedCombatUnit(runtime))
            {
                rinShockwaveCooldownRemaining = skillRuntime.CooldownRemaining;
            }

            return true;
        }

        private bool TryCastRinUnitFinishingBlow(CombatUnitRuntime runtime, CombatSkillRuntime skillRuntime)
        {
            if (!IsRinCombatUnit(runtime)
                || skillRuntime == null
                || skillRuntime.Skill == null
                || skillRuntime.CooldownRemaining > 0f)
            {
                return false;
            }

            var skill = skillRuntime.Skill;
            var range = GetRinMapWideSkillRange();
            var target = FindRinFinishingBlowTarget(runtime.Transform.position, range, runtime);
            if (target == null)
            {
                return false;
            }

            var executeThreshold = GetRinFinishingBlowExecuteThreshold(runtime);
            var healthRatio = target.MaxHealth > 0f ? target.CurrentHealth / target.MaxHealth : 1f;
            var executeTarget = healthRatio <= Mathf.Clamp01(executeThreshold);
            if (!executeTarget)
            {
                return false;
            }

            var damageMultiplier = 1f;
            if (HasRinUnitChoice(runtime, "rin-d-trait-1"))
            {
                damageMultiplier *= 1.30f;
            }

            damageMultiplier *= RinFinishingBlowExecuteMultiplier;
            if (HasRinUnitChoice(runtime, "rin-d-trait-5") && target.IsBoss)
            {
                damageMultiplier *= 1.25f;
            }

            if (HasRinUnitChoice(runtime, "rin-d-master-2"))
            {
                damageMultiplier *= 1.90f;
            }

            var wasAlive = target.CurrentHealth > 0f;
            var physicalDamage = ApplyRinUnitSkillDamage(runtime, skill, target, damageMultiplier, "rin-d", executeTarget);
            CreateRinFinishingBlowHitEffect(target);
            if (HasRinUnitChoice(runtime, "rin-d-master-2"))
            {
                ApplyRinUnitAdditionalDamage(runtime, target, physicalDamage, 0.70f, DamageAttribute.Darkness, "rin-d-master-2");
            }

            skillRuntime.CooldownDuration = GetRinCooldown(skill, 9f, HasRinUnitChoice(runtime, "rin-d-master-2") ? 1.25f : 1f);
            skillRuntime.CooldownRemaining = skillRuntime.CooldownDuration;
            if (IsSelectedCombatUnit(runtime))
            {
                rinFinishingBlowCooldownRemaining = skillRuntime.CooldownRemaining;
            }

            if (executeTarget && HasRinUnitPassive(runtime, "rin-i") && HasRinUnitChoice(runtime, "rin-i-trait-3"))
            {
                ReduceRinUnitSkillCooldown(runtime, "rin-e", GetRinCooldown(FindRinUnitSkill(runtime, "rin-e"), 8f, HasRinUnitChoice(runtime, "rin-e-trait-3") ? 0.80f : 1f) * 0.12f);
            }

            var killed = wasAlive && target.CurrentHealth <= 0f;
            if (killed && IsSelectedCombatUnit(runtime))
            {
                HandleRinFinishingBlowKill(target, physicalDamage);
                skillRuntime.CooldownRemaining = rinFinishingBlowCooldownRemaining;
            }
            else if (killed)
            {
                HandleRinUnitFinishingBlowKill(runtime, target, physicalDamage, skillRuntime);
            }

            statusLabel = killed
                ? $"{runtime.Monster.DisplayName} Finishing Blow executed {target.DisplayName}."
                : $"{runtime.Monster.DisplayName} Finishing Blow hit {target.DisplayName}.";
            return true;
        }

        private bool TryCastRinUnitCollapseStrike(CombatUnitRuntime runtime, CombatSkillRuntime skillRuntime)
        {
            if (!IsRinCombatUnit(runtime)
                || skillRuntime == null
                || skillRuntime.Skill == null
                || skillRuntime.CooldownRemaining > 0f)
            {
                return false;
            }

            var skill = skillRuntime.Skill;
            var range = GetRinMapWideSkillRange();
            var target = FindNearestEnemy(runtime.Transform.position, range);
            if (target == null)
            {
                return false;
            }

            var radius = skill.Radius > 0f ? skill.Radius : 2.4f;
            var damageMultiplier = 1f;
            if (HasRinUnitChoice(runtime, "rin-e-trait-1"))
            {
                damageMultiplier *= 1.30f;
            }

            if (HasRinUnitChoice(runtime, "rin-e-trait-2"))
            {
                radius *= 1.25f;
            }

            if (HasRinUnitChoice(runtime, "rin-e-master-1"))
            {
                radius *= 0.80f;
                damageMultiplier *= 2.00f;
            }

            if (HasRinUnitChoice(runtime, "rin-e-master-2"))
            {
                radius *= 1.50f;
                damageMultiplier *= 1.35f;
            }

            var center = target.Transform.position;
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

                var targetMultiplier = enemy == target && HasRinUnitChoice(runtime, "rin-e-trait-4") ? 1.50f : 1f;
                var physicalDamage = ApplyRinUnitSkillDamage(runtime, skill, enemy, damageMultiplier * targetMultiplier, "rin-e");
                if (HasRinUnitPassive(runtime, "rin-j"))
                {
                    ApplyRinUnitPhysicalDefenseReduction(runtime, enemy);
                }

                if (enemy == target && HasRinUnitChoice(runtime, "rin-e-master-1"))
                {
                    ApplyRinUnitAdditionalDamage(runtime, enemy, physicalDamage, 1.00f, DamageAttribute.Fire, "rin-e-master-1");
                }

                if (HasRinUnitChoice(runtime, "rin-e-master-2"))
                {
                    ApplyRinSlow(enemy, 0.75f, 2f);
                    ApplyRinUnitAdditionalDamage(runtime, enemy, physicalDamage, 0.45f, DamageAttribute.Darkness, "rin-e-master-2");
                }

                hitCount += 1;
            }

            if (hitCount >= 3 && HasRinUnitChoice(runtime, "rin-e-trait-5"))
            {
                ReduceRinUnitSkillCooldown(runtime, "rin-b", GetRinCooldown(FindRinUnitSkill(runtime, "rin-b"), 12f, 1f) * 0.20f);
            }

            if (hitCount >= 3 && HasRinUnitPassive(runtime, "rin-j"))
            {
                if (IsSelectedCombatUnit(runtime))
                {
                    rinCollapseAftermathActionTimer = Mathf.Max(rinCollapseAftermathActionTimer, RinCollapseAftermathDuration);
                }
                else
                {
                    runtime.RinCollapseAftermathActionTimer = Mathf.Max(runtime.RinCollapseAftermathActionTimer, RinCollapseAftermathDuration);
                }

                if (HasRinUnitChoice(runtime, "rin-j-trait-2"))
                {
                    if (IsSelectedCombatUnit(runtime))
                    {
                        rinCollapseAftermathAttackTimer = Mathf.Max(rinCollapseAftermathAttackTimer, RinCollapseAftermathDuration);
                    }
                    else
                    {
                        runtime.RinCollapseAftermathAttackTimer = Mathf.Max(runtime.RinCollapseAftermathAttackTimer, RinCollapseAftermathDuration);
                    }
                }
            }

            var effect = CreateCircleEffect("RinCollapseStrike", center, radius, 0.35f, skill.SkillEffectPrefab);
            effect.SkillId = "rin-e";
            if (effect.Renderer != null)
            {
                effect.Renderer.color = new Color(1f, 0.56f, 0.32f, 0.45f);
                effect.Renderer.sortingOrder = 24;
            }

            AddBattlefieldSkillEffect(effect);
            skillRuntime.CooldownDuration = GetRinCooldown(skill, 8f, HasRinUnitChoice(runtime, "rin-e-trait-3") ? 0.80f : 1f);
            skillRuntime.CooldownRemaining = skillRuntime.CooldownDuration;
            if (IsSelectedCombatUnit(runtime))
            {
                rinCollapseStrikeCooldownRemaining = skillRuntime.CooldownRemaining;
            }

            statusLabel = $"{runtime.Monster.DisplayName} Collapse Strike hit {hitCount} enemy(s).";
            return hitCount > 0;
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
            AddBattlefieldProjectile(new ProjectileRuntime
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

            var effect = CreateCircleEffect("RinHowling", eveAnchor.position, 2.2f, 0.45f, skill.SkillEffectPrefab);
            effect.SkillId = "rin-b";
            if (effect.Renderer != null)
            {
                effect.Renderer.color = new Color(1f, 0.82f, 0.42f, 0.42f);
                effect.Renderer.sortingOrder = 24;
            }

            AddBattlefieldSkillEffect(effect);
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

            var effect = CreateLineEffect("RinShockwave", eveAnchor.position, direction, length, width, 0.25f, skill.SkillEffectPrefab);
            effect.SkillId = "rin-c";
            if (effect.Renderer != null)
            {
                effect.Renderer.color = new Color(1f, 0.88f, 0.56f, 0.68f);
                effect.Renderer.sortingOrder = 24;
            }

            AddBattlefieldSkillEffect(effect);

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

            var effect = CreateCircleEffect("RinCollapseStrike", center, radius, 0.35f, skill.SkillEffectPrefab);
            effect.SkillId = "rin-e";
            if (effect.Renderer != null)
            {
                effect.Renderer.color = new Color(1f, 0.56f, 0.32f, 0.45f);
                effect.Renderer.sortingOrder = 24;
            }

            AddBattlefieldSkillEffect(effect);
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

        private float ApplyRinUnitSkillDamage(CombatUnitRuntime runtime, SkillDefinition skill, EnemyRuntime enemy, float finalMultiplier, string skillId, bool executeTarget = false)
        {
            if (IsSelectedCombatUnit(runtime))
            {
                return ApplyRinSkillDamage(enemy, GetRinUnitSkillBaseDamage(runtime, skill) * finalMultiplier, DamageAttribute.Physical, 1f, skillId, executeTarget);
            }

            if (!IsRinCombatUnit(runtime) || skill == null || enemy == null || enemy.CurrentHealth <= 0f)
            {
                return 0f;
            }

            var attribute = skill.Attribute;
            var result = DamageCalculator.Resolve(
                GetRinUnitSkillBaseDamage(runtime, skill),
                attribute,
                enemy.Defenses,
                percentDefenseReductions: GetRinUnitPercentDefenseReductions(runtime, enemy, attribute),
                criticalChanceBonus: GetRinUnitCriticalChanceBonus(runtime, attribute, skillId, executeTarget),
                criticalMultiplierBonus: GetRinUnitCriticalMultiplierBonus(runtime, attribute, skillId),
                targetCriticalResistance: enemy.CriticalResistance,
                finalDamageMultiplier: enemy.DamageTakenMultiplier * Mathf.Max(0f, finalMultiplier) * GetRinUnitFinalDamageMultiplier(runtime, enemy, attribute, skillId));
            var wasAlive = enemy.CurrentHealth > 0f;
            var applied = ApplyDamageToEnemy(enemy, result.FinalDamage, attribute);
            enemy.FlashTimer = 0.08f;
            if (attribute == DamageAttribute.Physical)
            {
                TrackRinUnitPhysicalDamageHit(runtime, applied, skillId);
                ApplyRinUnitAmbidextrousFollowup(runtime, enemy, applied, skillId);
            }

            ApplyRinUnitHowlingDarkAdditionalDamage(runtime, enemy, applied, $"{skillId}-howling");
            HandleRinUnitEnemyKilledByDamage(runtime, enemy, wasAlive, skillId);
            Debug.Log($"[CombatDamage] ManifestedRin.{skillId} -> {enemy.DisplayName}: {result.FormulaLog}; Applied={applied:0.##}, ShieldLeft={enemy.ShieldValue:0.##}, HpLeft={Mathf.Max(0f, enemy.CurrentHealth):0.##}");
            return applied;
        }

        private float ApplyRinUnitAdditionalDamage(CombatUnitRuntime runtime, EnemyRuntime enemy, float sourceDamage, float multiplier, DamageAttribute attribute, string skillId, bool spawnPopup = true)
        {
            if (IsSelectedCombatUnit(runtime))
            {
                return ApplyRinAdditionalDamage(enemy, sourceDamage, multiplier, attribute, skillId, spawnPopup);
            }

            if (!IsRinCombatUnit(runtime) || enemy == null || enemy.CurrentHealth <= 0f || sourceDamage <= 0f || multiplier <= 0f)
            {
                return 0f;
            }

            var result = DamageCalculator.Resolve(
                sourceDamage * multiplier,
                attribute,
                enemy.Defenses,
                percentDefenseReductions: GetRinUnitPercentDefenseReductions(runtime, enemy, attribute),
                targetCriticalResistance: enemy.CriticalResistance,
                finalDamageMultiplier: enemy.DamageTakenMultiplier);
            var wasAlive = enemy.CurrentHealth > 0f;
            var applied = ApplyDamageToEnemy(enemy, result.FinalDamage, attribute, spawnPopup);
            enemy.FlashTimer = 0.08f;
            HandleRinUnitEnemyKilledByDamage(runtime, enemy, wasAlive, skillId);
            Debug.Log($"[CombatDamage] ManifestedRin.{skillId} -> {enemy.DisplayName}: {result.FormulaLog}; Applied={applied:0.##}, ShieldLeft={enemy.ShieldValue:0.##}, HpLeft={Mathf.Max(0f, enemy.CurrentHealth):0.##}");
            return applied;
        }

        private bool TryApplyRinUnitProjectileHit(ProjectileRuntime projectile, EnemyRuntime enemy, out DamageResult damageResult, out float appliedDamage)
        {
            damageResult = default;
            appliedDamage = 0f;
            var runtime = projectile != null ? projectile.ManifestedSource : null;
            if (!IsRinCombatUnit(runtime) || projectile == null || enemy == null || enemy.CurrentHealth <= 0f)
            {
                return false;
            }

            damageResult = DamageCalculator.Resolve(
                projectile.BaseDamage,
                projectile.Attribute,
                enemy.Defenses,
                percentDefenseReductions: GetRinUnitPercentDefenseReductions(runtime, enemy, projectile.Attribute),
                criticalChanceBonus: GetRinUnitCriticalChanceBonus(runtime, projectile.Attribute, projectile.SkillId),
                criticalMultiplierBonus: GetRinUnitCriticalMultiplierBonus(runtime, projectile.Attribute, projectile.SkillId),
                targetCriticalResistance: enemy.CriticalResistance,
                finalDamageMultiplier: enemy.DamageTakenMultiplier * GetRinUnitFinalDamageMultiplier(runtime, enemy, projectile.Attribute, projectile.SkillId));
            var wasAlive = enemy.CurrentHealth > 0f;
            appliedDamage = ApplyDamageToEnemy(enemy, damageResult.FinalDamage, damageResult.Attribute);
            if (projectile.Attribute == DamageAttribute.Physical)
            {
                TrackRinUnitPhysicalDamageHit(runtime, appliedDamage, projectile.SkillId);
                ApplyRinUnitAmbidextrousFollowup(runtime, enemy, appliedDamage, projectile.SkillId);
            }

            ApplyRinUnitHowlingDarkAdditionalDamage(runtime, enemy, appliedDamage, $"{projectile.SkillId}-howling");
            HandleRinUnitEnemyKilledByDamage(runtime, enemy, wasAlive, projectile.SkillId);
            return true;
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

        private void HandleRinUnitFinishingBlowKill(CombatUnitRuntime runtime, EnemyRuntime target, float physicalDamage, CombatSkillRuntime skillRuntime)
        {
            if (!IsRinCombatUnit(runtime) || target == null)
            {
                return;
            }

            if (HasRinUnitPassive(runtime, "rin-i"))
            {
                runtime.RinFinisherInstinctActionTimer = Mathf.Max(runtime.RinFinisherInstinctActionTimer, RinFinisherInstinctDuration);
                if (HasRinUnitChoice(runtime, "rin-i-trait-2"))
                {
                    runtime.RinFinisherInstinctCritTimer = Mathf.Max(runtime.RinFinisherInstinctCritTimer, RinFinisherInstinctDuration);
                }
            }

            if (HasRinUnitChoice(runtime, "rin-d-master-1"))
            {
                if (skillRuntime != null)
                {
                    skillRuntime.CooldownRemaining = 0f;
                }

                ApplyRinAreaAdditionalDamage(target.Transform.position, RinFinishingBlowExplosionRadius, physicalDamage, 0.90f, DamageAttribute.Holy, "rin-d-master-1");
                return;
            }

            var refund = RinFinishingBlowKillCooldownRefund + (HasRinUnitChoice(runtime, "rin-d-trait-3") ? 0.20f : 0f);
            ReduceRinUnitSkillCooldown(runtime, "rin-d", GetRinCooldown(FindRinUnitSkill(runtime, "rin-d"), 9f, HasRinUnitChoice(runtime, "rin-d-master-2") ? 1.25f : 1f) * refund);
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

        private void ApplyRinUnitAmbidextrousFollowup(CombatUnitRuntime runtime, EnemyRuntime enemy, float sourcePhysicalDamage, string sourceSkillId)
        {
            if (IsSelectedCombatUnit(runtime))
            {
                ApplyRinAmbidextrousFollowup(enemy, sourcePhysicalDamage, sourceSkillId);
                return;
            }

            if (!HasRinUnitPassive(runtime, "rin-f")
                || enemy == null
                || sourcePhysicalDamage <= 0f
                || !IsRinAmbidextrousEligibleSkill(sourceSkillId))
            {
                return;
            }

            var multiplier = 0.35f + (HasRinUnitChoice(runtime, "rin-f-trait-2") ? 0.15f : 0f);
            var followupDamage = ApplyRinUnitAdditionalDamage(runtime, enemy, sourcePhysicalDamage, multiplier, DamageAttribute.Physical, "rin-f", false);
            var lightningDamage = 0f;
            if (HasRinUnitChoice(runtime, "rin-f-trait-3"))
            {
                lightningDamage = ApplyRinUnitAdditionalDamage(runtime, enemy, followupDamage, 0.30f, DamageAttribute.Lightning, "rin-f-trait-3", false);
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

            AddBattlefieldSkillEffect(effect);

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

        private void TrackRinUnitPhysicalDamageHit(CombatUnitRuntime runtime, float appliedDamage, string sourceSkillId)
        {
            if (IsSelectedCombatUnit(runtime))
            {
                TrackRinPhysicalDamageHit(appliedDamage, sourceSkillId);
                return;
            }

            if (!HasRinUnitPassive(runtime, "rin-h")
                || appliedDamage <= 0f
                || runtime.RinWaveAmplificationCooldownRemaining > 0f
                || string.Equals(sourceSkillId, "rin-h", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            runtime.RinWaveAmplificationPhysicalHitCount += 1;
            var requiredHits = HasRinUnitChoice(runtime, "rin-h-trait-1") ? 8 : 10;
            if (runtime.RinWaveAmplificationPhysicalHitCount < requiredHits)
            {
                return;
            }

            runtime.RinWaveAmplificationPhysicalHitCount = 0;
            TryCastRinUnitWaveAmplificationShockwave(runtime);
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

            AddBattlefieldSkillEffect(effect);
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

        private bool TryCastRinUnitWaveAmplificationShockwave(CombatUnitRuntime runtime)
        {
            if (IsSelectedCombatUnit(runtime))
            {
                return TryCastRinWaveAmplificationShockwave();
            }

            if (!IsRinCombatUnit(runtime) || runtime.Transform == null)
            {
                return false;
            }

            var skill = FindRinUnitSkill(runtime, "rin-c");
            if (skill == null)
            {
                return false;
            }

            var mapWideRange = GetRinMapWideSkillRange();
            var target = FindNearestEnemy(runtime.Transform.position, mapWideRange);
            if (target == null)
            {
                return false;
            }

            var direction = target.Transform.position - runtime.Transform.position;
            direction.z = 0f;
            if (direction.sqrMagnitude < 0.01f)
            {
                direction = Vector3.right;
            }

            direction.Normalize();
            var effect = CreateLineEffect("RinWaveAmplification", runtime.Transform.position, direction, mapWideRange, RinShockwaveWidth, 0.25f);
            effect.SkillId = "rin-h";
            if (effect.Renderer != null)
            {
                effect.Renderer.color = new Color(0.62f, 0.86f, 1f, 0.58f);
                effect.Renderer.sortingOrder = 24;
            }

            AddBattlefieldSkillEffect(effect);
            var damageMultiplier = HasRinUnitChoice(runtime, "rin-h-trait-2") ? 0.95f : 0.75f;
            var hitCount = 0;
            for (var i = 0; i < enemies.Count; i++)
            {
                var enemy = enemies[i];
                if (enemy == null || enemy.CurrentHealth <= 0f || enemy.Transform == null || !IsPointInsideBeam(enemy.Transform.position, effect))
                {
                    continue;
                }

                var physicalDamage = ApplyRinUnitSkillDamage(runtime, skill, enemy, damageMultiplier, "rin-h");
                ApplyRinKnockback(enemy, direction, RinShockwaveKnockback);
                if (HasRinUnitChoice(runtime, "rin-h-trait-3"))
                {
                    ApplyRinUnitAdditionalDamage(runtime, enemy, physicalDamage, 0.30f, DamageAttribute.Lightning, "rin-h-trait-3");
                }

                hitCount += 1;
            }

            runtime.RinWaveAmplificationCooldownRemaining = RinWaveAmplificationInternalCooldown;
            statusLabel = $"{runtime.Monster.DisplayName} Wave Amplification auto Shockwave hit {hitCount} enemy(s).";
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

        private void ApplyRinUnitPhysicalDefenseReduction(CombatUnitRuntime runtime, EnemyRuntime enemy)
        {
            if (IsSelectedCombatUnit(runtime))
            {
                ApplyRinPhysicalDefenseReduction(enemy);
                return;
            }

            if (enemy == null)
            {
                return;
            }

            enemy.RinPhysicalDefenseReduction = Mathf.Max(enemy.RinPhysicalDefenseReduction, HasRinUnitChoice(runtime, "rin-j-trait-1") ? 0.26f : 0.18f);
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

        private void HandleRinUnitEnemyKilledByDamage(CombatUnitRuntime runtime, EnemyRuntime enemy, bool wasAlive, string skillId)
        {
            if (IsSelectedCombatUnit(runtime))
            {
                HandleRinEnemyKilledByDamage(enemy, wasAlive);
                return;
            }

            if (!IsRinCombatUnit(runtime) || enemy == null || !wasAlive || enemy.CurrentHealth > 0f)
            {
                return;
            }

            if (HasRinUnitPassive(runtime, "rin-j") && HasRinUnitChoice(runtime, "rin-j-trait-3") && enemy.RinPhysicalDefenseReductionTimer > 0f)
            {
                ReduceRinUnitSkillCooldown(runtime, "rin-d", GetRinCooldown(FindRinUnitSkill(runtime, "rin-d"), 9f, HasRinUnitChoice(runtime, "rin-d-master-2") ? 1.25f : 1f) * 0.15f);
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

        private float[] GetRinUnitPercentDefenseReductions(CombatUnitRuntime runtime, EnemyRuntime enemy, DamageAttribute attribute)
        {
            if (IsSelectedCombatUnit(runtime))
            {
                return GetRinPercentDefenseReductions(enemy, attribute);
            }

            if (!IsRinCombatUnit(runtime)
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

            AddBattlefieldSkillEffect(effect);
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
            return eveAnchor != null
                ? FindRinFinishingBlowTarget(eveAnchor.position, range, selectedUnitRuntime)
                : null;
        }

        private EnemyRuntime FindRinFinishingBlowTarget(Vector3 origin, float range)
        {
            return FindRinFinishingBlowTarget(origin, range, selectedUnitRuntime);
        }

        private EnemyRuntime FindRinFinishingBlowTarget(Vector3 origin, float range, CombatUnitRuntime runtime)
        {
            EnemyRuntime executeTarget = null;
            var executeRatio = float.MaxValue;
            var threshold = GetRinFinishingBlowExecuteThreshold(runtime);

            for (var i = 0; i < enemies.Count; i++)
            {
                var enemy = enemies[i];
                if (enemy == null || enemy.CurrentHealth <= 0f || enemy.Transform == null)
                {
                    continue;
                }

                var distance = Vector2.Distance(origin, enemy.Transform.position);
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
            return GetRinFinishingBlowExecuteThreshold(selectedUnitRuntime);
        }

        private float GetRinFinishingBlowExecuteThreshold(CombatUnitRuntime runtime)
        {
            var threshold = RinFinishingBlowExecuteThreshold;
            if (HasRinUnitChoice(runtime, "rin-d-trait-2"))
            {
                threshold += 0.10f;
            }

            if (HasRinUnitChoice(runtime, "rin-d-master-2"))
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

            AddBattlefieldSkillEffect(effect);
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

        private float GetRinUnitSkillBaseDamage(CombatUnitRuntime runtime, SkillDefinition skill)
        {
            if (IsSelectedCombatUnit(runtime))
            {
                return GetRinSkillBaseDamage(skill);
            }

            if (runtime == null || skill == null)
            {
                return 0f;
            }

            var coefficient = Mathf.Max(skill.AttackPowerCoefficient, skill.SpellPowerCoefficient);
            var effectivePower = runtime.PowerStat;
            if (runtime.RinHowlingTimer > 0f && HasRinUnitPassive(runtime, "rin-g"))
            {
                effectivePower *= 1f + 0.14f + (HasRinUnitChoice(runtime, "rin-g-trait-1") ? 0.08f : 0f);
            }

            if (runtime.RinHowlingTimer > 0f && HasRinUnitChoice(runtime, "rin-b-trait-4"))
            {
                effectivePower *= 1.15f;
            }

            return skill.BaseDamage + effectivePower * coefficient;
        }

        private void ApplyRinUnitHowlingDarkAdditionalDamage(CombatUnitRuntime runtime, EnemyRuntime enemy, float physicalDamage, string skillId)
        {
            if (IsSelectedCombatUnit(runtime))
            {
                ApplyRinHowlingDarkAdditionalDamage(enemy, physicalDamage, skillId);
                return;
            }

            if (runtime == null || runtime.RinHowlingTimer <= 0f || !HasRinUnitChoice(runtime, "rin-b-master-2"))
            {
                return;
            }

            ApplyRinUnitAdditionalDamage(runtime, enemy, physicalDamage, 0.25f, DamageAttribute.Darkness, skillId);
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

        private float GetRinUnitActionSpeedMultiplier(CombatUnitRuntime runtime)
        {
            if (IsSelectedCombatUnit(runtime))
            {
                return GetRinActionSpeedMultiplier();
            }

            if (!IsRinCombatUnit(runtime))
            {
                return 1f;
            }

            var bonus = 0f;
            if (runtime.RinHowlingTimer > 0f)
            {
                bonus += 0.20f;
                if (HasRinUnitPassive(runtime, "rin-g"))
                {
                    bonus += 0.08f;
                }

                if (HasRinUnitChoice(runtime, "rin-b-trait-2"))
                {
                    bonus += 0.10f;
                }

                if (HasRinUnitChoice(runtime, "rin-b-master-1"))
                {
                    bonus += 0.15f;
                }

                if (HasRinUnitChoice(runtime, "rin-b-master-2"))
                {
                    bonus -= 0.05f;
                }
            }

            if (runtime.RinFinisherInstinctActionTimer > 0f)
            {
                bonus += 0.10f;
            }

            if (runtime.RinCollapseAftermathActionTimer > 0f)
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

        private float GetRinUnitFinalDamageMultiplier(CombatUnitRuntime runtime, EnemyRuntime enemy, DamageAttribute attribute, string skillId)
        {
            if (IsSelectedCombatUnit(runtime))
            {
                return GetRinFinalDamageMultiplier(enemy, attribute, skillId);
            }

            if (!IsRinCombatUnit(runtime) || enemy == null)
            {
                return 1f;
            }

            var bonus = 0f;
            if (attribute == DamageAttribute.Physical && HasRinUnitPassive(runtime, "rin-f"))
            {
                bonus += 0.12f + (HasRinUnitChoice(runtime, "rin-f-trait-1") ? 0.06f : 0f);
            }

            if (runtime.RinHowlingTimer > 0f && attribute == DamageAttribute.Physical && HasRinUnitChoice(runtime, "rin-b-master-1"))
            {
                bonus += 0.18f;
            }

            if (HasRinUnitPassive(runtime, "rin-i") && IsRinLowHealthTarget(enemy))
            {
                bonus += 0.16f + (HasRinUnitChoice(runtime, "rin-i-trait-1") ? 0.08f : 0f);
            }

            return 1f + bonus;
        }

        private float GetRinUnitCriticalChanceBonus(CombatUnitRuntime runtime, DamageAttribute attribute, string skillId, bool executeTarget = false)
        {
            if (IsSelectedCombatUnit(runtime))
            {
                return GetRinCriticalChanceBonus(null, attribute, skillId, executeTarget);
            }

            if (!IsRinCombatUnit(runtime))
            {
                return 0f;
            }

            var bonus = 0f;
            if (attribute == DamageAttribute.Physical && string.Equals(skillId, "rin-a", StringComparison.OrdinalIgnoreCase) && HasRinUnitChoice(runtime, "rin-a-trait-5"))
            {
                bonus += 0.10f;
            }

            if (runtime.RinHowlingTimer > 0f && attribute == DamageAttribute.Physical && HasRinUnitChoice(runtime, "rin-b-trait-5"))
            {
                bonus += 0.08f;
            }

            if (runtime.RinHowlingTimer > 0f && attribute == DamageAttribute.Physical && HasRinUnitPassive(runtime, "rin-g") && HasRinUnitChoice(runtime, "rin-g-trait-2"))
            {
                bonus += 0.06f;
            }

            if (executeTarget && string.Equals(skillId, "rin-d", StringComparison.OrdinalIgnoreCase) && HasRinUnitChoice(runtime, "rin-d-master-1"))
            {
                bonus += 0.50f;
            }

            return bonus;
        }

        private float GetRinUnitCriticalMultiplierBonus(CombatUnitRuntime runtime, DamageAttribute attribute, string skillId)
        {
            if (IsSelectedCombatUnit(runtime))
            {
                return GetRinCriticalMultiplierBonus(null, attribute, skillId);
            }

            if (!IsRinCombatUnit(runtime) || attribute != DamageAttribute.Physical)
            {
                return 0f;
            }

            var bonus = 0f;
            if (string.Equals(skillId, "rin-a", StringComparison.OrdinalIgnoreCase) && HasRinUnitChoice(runtime, "rin-a-trait-5"))
            {
                bonus += 0.25f;
            }

            if (string.Equals(skillId, "rin-d", StringComparison.OrdinalIgnoreCase) && HasRinUnitChoice(runtime, "rin-d-trait-4"))
            {
                bonus += 0.40f;
            }

            if (runtime.RinFinisherInstinctCritTimer > 0f && HasRinUnitChoice(runtime, "rin-i-trait-2"))
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
