using System;
using System.Collections.Generic;
using Pakuri.Data;
using Pakuri.Run;
using UnityEngine;

namespace Pakuri.Combat
{
    public partial class CombatRuntimeController
    {
        private const int MaxManifestedPartyMonsterCount = 4;
        private const float ManifestedMonsterAttackInterval = 1.35f;
        private const float ManifestedMonsterProjectileLifetime = 0.22f;
        private const float ManifestedMonsterProjectileSpeedFallback = 15f;

        private static readonly string[] ManifestedMonsterSlotNames =
        {
            "2PMonster",
            "3PMonster",
            "4PMonster",
            "5PMonster"
        };

        public int PartyMonsterCount => 1 + manifestedParty.MonsterCount;

        private void ConfigureSelectedUnitRuntime(RunSession session)
        {
            if (selectedMonster == null || eveAnchor == null)
            {
                selectedUnitRuntime = null;
                return;
            }

            selectedUnitRuntime = eveAnchor.GetComponent<CombatUnitRuntime>();
            if (selectedUnitRuntime == null)
            {
                selectedUnitRuntime = eveAnchor.gameObject.AddComponent<CombatUnitRuntime>();
            }

            selectedUnitRuntime.ConfigureSelected(
                this,
                selectedMonster,
                session != null ? session.EnsurePartyMemberState(selectedMonster) : null,
                eveAnchor.GetComponent<SpriteRenderer>(),
                selectedMonsterHpLabel);
            SyncSelectedUnitRuntimeStats();
            SyncManifestedLearnedSkills(selectedUnitRuntime);
        }

        private void SyncSelectedUnitRuntimeStats()
        {
            if (selectedUnitRuntime == null)
            {
                return;
            }

            selectedUnitRuntime.SyncStats(
                unitMaxHealthConfigured,
                unitCurrentHealth,
                baseDamageConfigured,
                powerStatConfigured);
            selectedUnitRuntime.ShieldValue = unitShieldValue;
            selectedUnitRuntime.ShieldTimer = unitShieldTimer;
            selectedUnitRuntime.ShieldAppliedFrame = unitShieldAppliedFrame;
        }

        public MonsterDefinition GetPartyMonsterDefinition(int partyIndex)
        {
            if (partyIndex <= 0)
            {
                return selectedMonster;
            }

            var manifestedIndex = partyIndex - 1;
            return manifestedIndex >= 0 && manifestedIndex < manifestedMonsters.Count
                ? manifestedMonsters[manifestedIndex].Monster
                : null;
        }

        public int GetPartyMonsterPanelSkillViews(int partyIndex, IList<MonsterPanelSkillView> views, int maxSlots = 3)
        {
            if (partyIndex <= 0)
            {
                return GetMonsterPanelSkillViews(views, maxSlots);
            }

            if (views == null)
            {
                return 0;
            }

            views.Clear();
            var manifestedIndex = partyIndex - 1;
            if (manifestedIndex < 0 || manifestedIndex >= manifestedMonsters.Count || maxSlots <= 0)
            {
                return 0;
            }

            var runtime = manifestedMonsters[manifestedIndex];
            if (runtime == null)
            {
                return 0;
            }

            SyncManifestedLearnedSkills(runtime);
            var added = 0;
            for (var i = 0; i < runtime.Skills.Count && added < maxSlots; i++)
            {
                var skillRuntime = runtime.Skills[i];
                if (skillRuntime == null || skillRuntime.Skill == null)
                {
                    continue;
                }

                var isMagazine = IsManifestedMagazineSkill(skillRuntime.Skill);
                views.Add(new MonsterPanelSkillView(
                    added,
                    skillRuntime.Skill,
                    isMagazine,
                    isMagazine ? Mathf.Max(0, skillRuntime.ShotsRemaining) : 0,
                    isMagazine ? Mathf.Max(1, skillRuntime.MagazineCapacity) : 0,
                    GetManifestedSkillCooldownRemaining(skillRuntime),
                    GetManifestedSkillCooldownDuration(skillRuntime)));
                added += 1;
            }

            return added;
        }

        private void ConfigureManifestedMonsterParty(RunSession session)
        {
            ClearManifestedMonsterParty();
            if (session == null || session.ManifestedMonsterIds == null || session.ManifestedMonsterIds.Count == 0)
            {
                return;
            }

            CacheManifestedMonsterSlots();
            var added = 0;
            for (var i = 0; i < session.ManifestedMonsterIds.Count && added < MaxManifestedPartyMonsterCount; i++)
            {
                var monsterId = session.ManifestedMonsterIds[i];
                var monster = PakuriDataManager.Instance.ResolveMonster(monsterId, gameDataCatalog);
                if (monster == null || string.Equals(monster.MonsterId, selectedMonster != null ? selectedMonster.MonsterId : string.Empty, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                manifestedParty.AddMonster(CreateCombatUnitRuntime(monster, session.EnsurePartyMemberState(monster), added));
                added += 1;
            }
        }

        public void RefreshManifestedMonsterParty(RunSession session)
        {
            ConfigureManifestedMonsterParty(session);
            ResetManifestedMonsterPartyCombat();
        }

        private CombatUnitRuntime CreateCombatUnitRuntime(MonsterDefinition monster, RunSession.RunMonsterState state, int index)
        {
            var slotTransform = ResolveManifestedMonsterSlot(index);
            var usesSceneSlot = slotTransform != null;
            var monsterObject = usesSceneSlot ? slotTransform.gameObject : CreateFallbackManifestedMonsterObject(index, monster);
            monsterObject.name = usesSceneSlot ? ManifestedMonsterSlotNames[index] : $"{index + 2}P_{monster.MonsterId}";
            monsterObject.SetActive(true);
            var monsterTransform = monsterObject.transform;
            if (!usesSceneSlot)
            {
                monsterTransform.position = ResolveManifestedMonsterPosition(index);
            }

            var renderer = monsterObject.GetComponent<SpriteRenderer>();
            if (renderer == null)
            {
                renderer = monsterObject.AddComponent<SpriteRenderer>();
            }

            renderer.sprite = monster.UnitSprite != null ? monster.UnitSprite : GetSharedSprite();
            renderer.color = Color.white;
            renderer.sortingOrder = 19 - index;

            var statusViews = ResolveManifestedMonsterStatusViews(monsterObject.transform, usesSceneSlot);
            var label = statusViews.HpLabel != null
                ? statusViews.HpLabel
                : usesSceneSlot ? null : EnsureManifestedMonsterLabel(monsterObject.transform);

            var runtime = monsterObject.GetComponent<CombatUnitRuntime>();
            if (runtime == null)
            {
                runtime = monsterObject.AddComponent<CombatUnitRuntime>();
            }

            runtime.ConfigureManifested(
                this,
                monster,
                state,
                renderer,
                label,
                statusViews.NameLabel,
                statusViews.HpLabel,
                statusViews.HpBarFill,
                statusViews.ShieldBarFill,
                usesSceneSlot,
                index);
            runtime.ConfigureStatsFromDefinition();
            SyncManifestedLearnedSkills(runtime);
            for (var i = 0; i < runtime.Skills.Count; i++)
            {
                ResetCombatSkillRuntime(runtime, runtime.Skills[i], 0.4f + (index * 0.25f) + (i * 0.15f));
            }

            UpdateManifestedMonsterLabel(runtime);
            return runtime;
        }

        private void CacheManifestedMonsterSlots()
        {
            for (var i = 0; i < ManifestedMonsterSlotNames.Length; i++)
            {
                if (manifestedMonsterSlots[i] != null)
                {
                    continue;
                }

                manifestedMonsterSlots[i] = transform.Find(ManifestedMonsterSlotNames[i]);
            }
        }

        private Transform ResolveManifestedMonsterSlot(int index)
        {
            CacheManifestedMonsterSlots();
            return index >= 0 && index < manifestedMonsterSlots.Length ? manifestedMonsterSlots[index] : null;
        }

        private GameObject CreateFallbackManifestedMonsterObject(int index, MonsterDefinition monster)
        {
            var monsterObject = new GameObject($"{index + 2}P_{(monster != null ? monster.MonsterId : "Monster")}");
            monsterObject.transform.SetParent(transform, false);
            monsterObject.transform.localScale = Vector3.one;
            return monsterObject;
        }

        private Vector3 ResolveManifestedMonsterPosition(int index)
        {
            switch (index)
            {
                case 0:
                    return new Vector3(4.8f, 6.1f, 0f);
                case 1:
                    return new Vector3(4.8f, 9.9f, 0f);
                case 2:
                    return new Vector3(7.1f, 5.1f, 0f);
                default:
                    return new Vector3(7.1f, 10.9f, 0f);
            }
        }

        private void ResetManifestedMonsterPartyCombat()
        {
            for (var i = 0; i < manifestedMonsters.Count; i++)
            {
                var runtime = manifestedMonsters[i];
                if (runtime == null || runtime.Monster == null)
                {
                    continue;
                }

                runtime.MaxHealth = Mathf.Max(1f, runtime.Monster.MaxHealth + (runtime.State != null ? runtime.State.MaxHealthBonus : 0f));
                runtime.CurrentHealth = runtime.MaxHealth;
                runtime.BaseDamage = Mathf.Max(1f, runtime.Monster.BaseDamage);
                runtime.PowerStat = Mathf.Max(0f, runtime.Monster.PowerStat);
                SyncManifestedLearnedSkills(runtime);
                for (var skillIndex = 0; skillIndex < runtime.Skills.Count; skillIndex++)
                {
                    ResetCombatSkillRuntime(runtime, runtime.Skills[skillIndex], 0.4f + (i * 0.25f) + (skillIndex * 0.15f));
                }

                if (runtime.Transform != null)
                {
                    if (!runtime.UsesSceneSlot)
                    {
                        runtime.Transform.position = ResolveManifestedMonsterPosition(i);
                    }

                    runtime.Transform.gameObject.SetActive(true);
                }

                UpdateManifestedMonsterLabel(runtime);
            }
        }

        private void UpdateManifestedMonsterPartyCombat()
        {
            manifestedParty.TickCombat(this, Time.deltaTime, battleResolved);
        }

        private bool CanTickManifestedPartyUnit(CombatUnitRuntime runtime)
        {
            return runtime != null
                && runtime.Transform != null
                && runtime.Monster != null
                && runtime.CurrentHealth > 0f;
        }

        private void SyncManifestedPartyUnitSkills(CombatUnitRuntime runtime)
        {
            SyncManifestedLearnedSkills(runtime);
        }

        private void TickManifestedPartyUnitCombat(CombatUnitRuntime runtime, float elapsed)
        {
            if (runtime.Skills.Count == 0)
            {
                return;
            }

            runtime.TickManifestedCombat(elapsed);
        }

        private void UpdateManifestedPartyUnitView(CombatUnitRuntime runtime)
        {
            UpdateManifestedMonsterLabel(runtime);
        }

        private EnemyRuntime FindNearestManifestedMonsterTarget(Vector3 origin)
        {
            EnemyRuntime best = null;
            var bestDistance = float.MaxValue;
            for (var i = 0; i < enemies.Count; i++)
            {
                var enemy = enemies[i];
                if (enemy == null || enemy.Transform == null || enemy.CurrentHealth <= 0f)
                {
                    continue;
                }

                var distance = Vector2.Distance(origin, enemy.Transform.position);
                if (distance >= bestDistance)
                {
                    continue;
                }

                best = enemy;
                bestDistance = distance;
            }

            return best;
        }

        private bool TryFireManifestedRinShockwave(CombatUnitRuntime runtime, CombatSkillRuntime skillRuntime, EnemyRuntime target)
        {
            var skill = skillRuntime != null ? skillRuntime.Skill : null;
            if (runtime == null || skill == null || target == null || runtime.Transform == null || target.Transform == null)
            {
                return false;
            }

            if (!string.Equals(skill.SkillId, "rin-c", StringComparison.OrdinalIgnoreCase))
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
            var length = GetRinMapWideSkillRange();
            var width = RinShockwaveWidth;
            var knockback = RinShockwaveKnockback;

            if (HasRinUnitChoice(runtime, "rin-c-trait-2"))
            {
                width *= 1.25f;
            }

            if (HasRinUnitChoice(runtime, "rin-c-trait-3"))
            {
                knockback *= 1.40f;
            }

            if (HasRinUnitChoice(runtime, "rin-c-master-1"))
            {
                width *= 0.75f;
                knockback *= 1.50f;
            }

            if (HasRinUnitChoice(runtime, "rin-c-master-2"))
            {
                width *= 1.60f;
            }

            var effect = CreateLineEffect("ManifestedRinShockwave", runtime.Transform.position, direction, length, width, 0.25f, skill.SkillEffectPrefab);
            effect.SkillId = "rin-c";
            if (effect.Renderer != null)
            {
                effect.Renderer.color = new Color(1f, 0.88f, 0.56f, 0.68f);
                effect.Renderer.sortingOrder = 24;
            }

            AddBattlefieldSkillEffect(effect);

            var hitCount = 0;
            var appliedTotal = 0f;
            for (var i = 0; i < enemies.Count; i++)
            {
                var enemy = enemies[i];
                if (enemy == null || enemy.CurrentHealth <= 0f || enemy.Transform == null || !IsPointInsideBeam(enemy.Transform.position, effect))
                {
                    continue;
                }

                var damageMultiplier = 1f;
                damageMultiplier *= HasRinUnitChoice(runtime, "rin-c-trait-1") ? 1.25f : 1f;
                damageMultiplier *= HasRinUnitChoice(runtime, "rin-c-master-1") ? 1.80f : 1f;
                damageMultiplier *= HasRinUnitChoice(runtime, "rin-c-master-2") ? 1.25f : 1f;
                var physicalDamage = ApplyRinUnitSkillDamage(runtime, skill, enemy, damageMultiplier, "rin-c");
                appliedTotal += physicalDamage;
                ApplyRinKnockback(enemy, direction, knockback);

                if (HasRinUnitChoice(runtime, "rin-c-master-1"))
                {
                    ApplyRinUnitAdditionalDamage(runtime, enemy, physicalDamage, 0.60f, DamageAttribute.Lightning, "rin-c-master-1");
                }

                if (HasRinUnitChoice(runtime, "rin-c-master-2"))
                {
                    ApplyRinSlow(enemy, 0.80f, 1.5f);
                }

                hitCount += 1;
            }

            if (hitCount > 0 && HasRinUnitChoice(runtime, "rin-c-trait-5"))
            {
                ReduceManifestedSkillReload(runtime, "rin-a", hitCount * 0.25f);
            }

            statusLabel = $"{runtime.Monster.DisplayName} {skill.DisplayName} shockwave hit {hitCount} enemy(s) for {appliedTotal:0.#}.";
            return true;
        }

        private static void ReduceManifestedSkillReload(CombatUnitRuntime runtime, string skillId, float amount)
        {
            if (runtime == null || string.IsNullOrWhiteSpace(skillId) || amount <= 0f)
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

                skillRuntime.ReloadRemaining = Mathf.Max(0f, skillRuntime.ReloadRemaining - amount);
            }
        }

        private void CreateManifestedGenericField(CombatUnitRuntime runtime, SkillDefinition skill, EnemyRuntime target)
        {
            if (runtime == null || skill == null || target == null || target.Transform == null)
            {
                return;
            }

            var radius = Mathf.Max(0.5f, skill.Radius > 0f ? skill.Radius : 2f);
            var duration = ResolveManifestedSkillVisualDuration(runtime, skill);
            var effect = CreateCircleEffect("ManifestedField", target.Transform.position, radius, duration, skill.SkillEffectPrefab);
            effect.SkillId = skill.SkillId;
            effect.ManifestedSource = runtime;
            effect.BaseDamage = ResolveManifestedBaseDamage(runtime, skill);
            effect.Attribute = skill.Attribute;
            effect.TickInterval = 0.5f;
            effect.TickRemaining = 0f;
            effect.Radius = radius;
            AddBattlefieldSkillEffect(effect);
            statusLabel = $"{runtime.Monster.DisplayName} {skill.DisplayName} field active.";
        }

        private bool TryFireManifestedPersistentSkill(CombatUnitRuntime runtime, SkillDefinition skill, EnemyRuntime target)
        {
            if (runtime == null || skill == null || target == null)
            {
                return false;
            }

            if (skill.RuntimeKind != SkillRuntimeKind.Field)
            {
                return false;
            }

            if (string.Equals(skill.SkillId, "eve-c", StringComparison.OrdinalIgnoreCase))
            {
                CreateManifestedEveFrostField(runtime, skill, target);
                return true;
            }

            CreateManifestedGenericField(runtime, skill, target);
            return true;
        }

        private void CreateManifestedEveFrostField(CombatUnitRuntime runtime, SkillDefinition skill, EnemyRuntime target)
        {
            var radius = Mathf.Max(0.5f, skill.Radius);
            var duration = EveFrostFieldDuration;
            var tickInterval = EveFrostFieldTickInterval;
            var damageMultiplier = 1f;
            var chillStacks = 1;

            if (HasManifestedChoice(runtime, "eve-c-trait-1"))
            {
                radius *= 1.25f;
                duration *= 1.15f;
            }

            if (HasManifestedChoice(runtime, "eve-c-trait-2"))
            {
                tickInterval = Mathf.Max(0.1f, tickInterval * 0.75f);
                chillStacks += 1;
            }

            if (HasManifestedChoice(runtime, "eve-c-trait-3"))
            {
                damageMultiplier *= 1.30f;
            }

            if (HasManifestedChoice(runtime, "eve-c-trait-4"))
            {
                radius *= 0.80f;
                damageMultiplier *= 1.80f;
            }

            if (HasManifestedChoice(runtime, "eve-c-trait-5"))
            {
                damageMultiplier *= 1.20f;
            }

            var effect = CreateCircleEffect("ManifestedFrostField", target.Transform.position, radius, duration, skill.SkillEffectPrefab);
            effect.SkillId = "eve-c";
            effect.ManifestedSource = runtime;
            effect.BaseDamage = (skill.BaseDamage + (runtime.PowerStat * skill.SpellPowerCoefficient))
                * damageMultiplier
                * ResolveManifestedDamageMultiplier(runtime);
            effect.Attribute = DamageAttribute.Ice;
            effect.TickInterval = Mathf.Max(0.05f, tickInterval);
            effect.TickRemaining = 0f;
            effect.Radius = radius;
            effect.StatusStacks = chillStacks;
            effect.FreezeDuration = HasManifestedChoice(runtime, "eve-c-trait-5")
                ? 1.0f + GetManifestedEveFreezeDurationBonus(runtime)
                : 0f;
            AddBattlefieldSkillEffect(effect);

            statusLabel = $"{runtime.Monster.DisplayName} {skill.DisplayName} frost field deployed.";
        }

        private static bool HasManifestedChoice(CombatUnitRuntime runtime, string choiceId)
        {
            return ContainsManifestedRuntimeText(runtime != null && runtime.State != null ? runtime.State.ChosenRewardIds : null, choiceId);
        }

        private bool HasRinUnitChoice(CombatUnitRuntime runtime, string choiceId)
        {
            return IsSelectedCombatUnit(runtime) ? HasChoice(choiceId) : HasManifestedChoice(runtime, choiceId);
        }

        private static bool IsManifestedMonster(CombatUnitRuntime runtime, string monsterId)
        {
            return runtime != null
                && runtime.Monster != null
                && string.Equals(runtime.Monster.MonsterId, monsterId, StringComparison.OrdinalIgnoreCase);
        }

        private static bool HasManifestedPassive(CombatUnitRuntime runtime, string passiveId)
        {
            return ContainsManifestedRuntimeText(runtime != null && runtime.State != null ? runtime.State.LearnedPassives : null, passiveId);
        }

        private static bool ContainsManifestedRuntimeText(IReadOnlyList<string> values, string target)
        {
            if (values == null || string.IsNullOrWhiteSpace(target))
            {
                return false;
            }

            for (var i = 0; i < values.Count; i++)
            {
                if (string.Equals(values[i], target, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private float GetManifestedEveFreezeDurationBonus(CombatUnitRuntime runtime)
        {
            var bonus = 0f;
            if (HasManifestedPassive(runtime, "eve-h") && HasManifestedChoice(runtime, "eve-h-trait-2"))
            {
                bonus += 0.5f;
            }

            return bonus;
        }

        private float ResolveManifestedSkillCooldown(CombatUnitRuntime runtime, SkillDefinition skill)
        {
            if (skill == null)
            {
                return ManifestedMonsterAttackInterval;
            }

            if (skill != null && skill.ShotIntervalSeconds > 0f)
            {
                var multiplier = runtime != null && runtime.State != null && runtime.State.ShotIntervalMultiplier > 0f
                    ? runtime.State.ShotIntervalMultiplier
                    : 1f;
                return Mathf.Max(0.45f, skill.ShotIntervalSeconds * multiplier);
            }

            if (skill.CooldownSeconds > 0f)
            {
                var cooldown = skill.CooldownSeconds;
                var skillId = skill.SkillId ?? string.Empty;
                if (string.Equals(skillId, "eve-c", StringComparison.OrdinalIgnoreCase) && HasManifestedChoice(runtime, "eve-c-trait-3"))
                {
                    cooldown *= 0.85f;
                }
                else if (string.Equals(skillId, "ariel-b", StringComparison.OrdinalIgnoreCase) && HasManifestedChoice(runtime, "ariel-b-trait-3"))
                {
                    cooldown *= 0.80f;
                }
                else if (string.Equals(skillId, "ariel-e", StringComparison.OrdinalIgnoreCase) && HasManifestedChoice(runtime, "ariel-e-trait-3"))
                {
                    cooldown *= 0.80f;
                }
                else if (string.Equals(skillId, "sein-c", StringComparison.OrdinalIgnoreCase) && HasManifestedChoice(runtime, "sein-c-trait-3"))
                {
                    cooldown *= 0.80f;
                }
                else if (string.Equals(skillId, "sein-d", StringComparison.OrdinalIgnoreCase) && HasManifestedChoice(runtime, "sein-d-trait-4"))
                {
                    cooldown *= 0.80f;
                }
                else if (string.Equals(skillId, "vega-b", StringComparison.OrdinalIgnoreCase) && HasManifestedChoice(runtime, "vega-b-trait-3"))
                {
                    cooldown *= 0.80f;
                }
                else if (string.Equals(skillId, "vega-d", StringComparison.OrdinalIgnoreCase))
                {
                    cooldown *= HasManifestedChoice(runtime, "vega-d-trait-3") ? 0.80f : 1f;
                    cooldown *= HasManifestedChoice(runtime, "vega-d-master-2") ? 1.20f : 1f;
                }
                else if (string.Equals(skillId, "vega-e", StringComparison.OrdinalIgnoreCase) && HasManifestedChoice(runtime, "vega-e-trait-3"))
                {
                    cooldown *= 0.80f;
                }
                else if (string.Equals(skillId, "rin-c", StringComparison.OrdinalIgnoreCase) && HasManifestedChoice(runtime, "rin-c-trait-4"))
                {
                    cooldown *= 0.80f;
                }

                return Mathf.Max(0.45f, cooldown);
            }

            return Mathf.Max(0.75f, runtime != null && runtime.Monster != null ? runtime.Monster.ShotInterval : ManifestedMonsterAttackInterval);
        }

        private static bool IsManifestedMagazineSkill(SkillDefinition skill)
        {
            return skill != null && skill.RuntimeKind == SkillRuntimeKind.MagazineProjectile && skill.MagazineCapacity > 0;
        }

        private static bool IsManifestedProjectileSkill(SkillDefinition skill)
        {
            return skill != null && (skill.RuntimeKind == SkillRuntimeKind.MagazineProjectile || skill.RuntimeKind == SkillRuntimeKind.CooldownProjectile);
        }

        private static bool IsManifestedVegaThreeSwordFlurry(SkillDefinition skill)
        {
            return skill != null && string.Equals(skill.SkillId, "vega-a", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsManifestedEveDroneBeacon(SkillDefinition skill)
        {
            return skill != null && string.Equals(skill.SkillId, "eve-e", StringComparison.OrdinalIgnoreCase);
        }

        private void QueueManifestedVegaThreeSwordFlurry(CombatUnitRuntime runtime, CombatSkillRuntime skillRuntime, EnemyRuntime target)
        {
            if (runtime == null || runtime.Transform == null || skillRuntime == null || skillRuntime.Skill == null || target == null || target.Transform == null)
            {
                return;
            }

            var direction = target.Transform.position - runtime.Transform.position;
            direction.z = 0f;
            if (direction.sqrMagnitude <= 0.0001f)
            {
                direction = Vector3.right;
            }

            skillRuntime.PendingVegaProjectileCount = HasVegaUnitChoice(runtime, "vega-a-master-1") ? 4 : 3;
            skillRuntime.PendingVegaProjectileIndex = 0;
            skillRuntime.PendingVegaProjectileDelay = 0f;
            skillRuntime.PendingVegaProjectileDirection = direction.normalized;
            UpdateManifestedQueuedProjectiles(runtime, skillRuntime, 0f);
        }

        private void UpdateManifestedQueuedProjectiles(CombatUnitRuntime runtime, CombatSkillRuntime skillRuntime, float elapsed)
        {
            if (runtime == null || skillRuntime == null || skillRuntime.Skill == null || skillRuntime.PendingVegaProjectileCount <= 0)
            {
                return;
            }

            skillRuntime.PendingVegaProjectileDelay = Mathf.Max(0f, skillRuntime.PendingVegaProjectileDelay - Mathf.Max(0f, elapsed));
            if (skillRuntime.PendingVegaProjectileDelay > 0f)
            {
                return;
            }

            var projectileIndex = skillRuntime.PendingVegaProjectileIndex;
            var isAfterimage = projectileIndex >= 3;
            var damageMultiplier = isAfterimage ? 0.45f : GetVegaUnitThreeSwordDamageMultiplier(runtime, projectileIndex >= 2);
            var markStacks = isAfterimage ? 1 : GetVegaUnitThreeSwordNameMarkStacks(runtime);
            FireManifestedMonsterProjectile(runtime, skillRuntime.Skill, skillRuntime.PendingVegaProjectileDirection, damageMultiplier, 999, markStacks);
            skillRuntime.PendingVegaProjectileIndex += 1;
            skillRuntime.PendingVegaProjectileCount -= 1;
            if (skillRuntime.PendingVegaProjectileCount > 0)
            {
                skillRuntime.PendingVegaProjectileDelay = VegaThreeSwordBulletInterval;
            }
        }

        private void ResetCombatSkillRuntime(CombatUnitRuntime runtime, CombatSkillRuntime skillRuntime, float initialDelay)
        {
            if (skillRuntime == null)
            {
                return;
            }

            skillRuntime.MagazineCapacity = ResolveManifestedMagazineCapacity(runtime, skillRuntime.Skill);
            skillRuntime.ShotsRemaining = skillRuntime.MagazineCapacity;
            skillRuntime.ShotInterval = ResolveManifestedShotInterval(runtime, skillRuntime.Skill);
            skillRuntime.ShotCooldownRemaining = Mathf.Max(0f, initialDelay);
            skillRuntime.ReloadDuration = ResolveManifestedReloadDuration(runtime, skillRuntime.Skill);
            skillRuntime.ReloadRemaining = 0f;
            skillRuntime.CooldownDuration = ResolveManifestedSkillCooldown(runtime, skillRuntime.Skill);
            skillRuntime.CooldownRemaining = IsManifestedMagazineSkill(skillRuntime.Skill) ? 0f : Mathf.Max(0f, initialDelay);
            skillRuntime.PendingVegaProjectileCount = 0;
            skillRuntime.PendingVegaProjectileIndex = 0;
            skillRuntime.PendingVegaProjectileDelay = 0f;
            skillRuntime.PendingVegaProjectileDirection = Vector3.zero;
        }

        private int ResolveManifestedMagazineCapacity(CombatUnitRuntime runtime, SkillDefinition skill)
        {
            var baseCapacity = skill != null && skill.MagazineCapacity > 0
                ? skill.MagazineCapacity
                : runtime != null && runtime.Monster != null ? runtime.Monster.MagazineCapacity : 1;
            var bonus = runtime != null && runtime.State != null ? runtime.State.MagazineBonus : 0;
            if (string.Equals(skill != null ? skill.SkillId : string.Empty, "eve-a", StringComparison.OrdinalIgnoreCase))
            {
                bonus += HasManifestedChoice(runtime, "eve-a-trait-1") ? 4 : 0;
                bonus += HasManifestedChoice(runtime, "eve-a-master-1") ? 2 : 0;
            }
            else if (string.Equals(skill != null ? skill.SkillId : string.Empty, "ariel-a", StringComparison.OrdinalIgnoreCase))
            {
                bonus += HasManifestedChoice(runtime, "ariel-a-trait-2") ? 3 : 0;
                bonus += HasManifestedChoice(runtime, "ariel-f-trait-2") ? 2 : 0;
            }
            else if (string.Equals(skill != null ? skill.SkillId : string.Empty, "rin-a", StringComparison.OrdinalIgnoreCase))
            {
                bonus += HasManifestedChoice(runtime, "rin-a-trait-2") ? 4 : 0;
                bonus += HasManifestedChoice(runtime, "rin-a-master-1") ? 6 : 0;
            }
            else if (string.Equals(skill != null ? skill.SkillId : string.Empty, "vega-a", StringComparison.OrdinalIgnoreCase))
            {
                bonus += HasManifestedChoice(runtime, "vega-a-trait-2") ? 2 : 0;
            }
            return Mathf.Max(1, baseCapacity + bonus);
        }

        private float ResolveManifestedReloadDuration(CombatUnitRuntime runtime, SkillDefinition skill)
        {
            var reload = skill != null && skill.ReloadSeconds > 0f
                ? skill.ReloadSeconds
                : runtime != null && runtime.Monster != null ? runtime.Monster.ReloadDuration : 1f;
            var multiplier = runtime != null && runtime.State != null && runtime.State.ReloadDurationMultiplier > 0f
                ? runtime.State.ReloadDurationMultiplier
                : 1f;
            if (string.Equals(skill != null ? skill.SkillId : string.Empty, "eve-a", StringComparison.OrdinalIgnoreCase))
            {
                multiplier *= HasManifestedChoice(runtime, "eve-a-trait-2") ? SpeedBonusToIntervalMultiplier(0.30f) : 1f;
                multiplier *= HasManifestedChoice(runtime, "eve-a-trait-3") ? 1.20f : 1f;
            }
            else if (string.Equals(skill != null ? skill.SkillId : string.Empty, "ariel-a", StringComparison.OrdinalIgnoreCase))
            {
                multiplier *= HasManifestedChoice(runtime, "ariel-a-trait-3") ? 0.80f : 1f;
            }
            else if (string.Equals(skill != null ? skill.SkillId : string.Empty, "rin-a", StringComparison.OrdinalIgnoreCase))
            {
                multiplier *= HasManifestedChoice(runtime, "rin-a-trait-3") ? SpeedBonusToIntervalMultiplier(0.25f) : 1f;
            }
            else if (string.Equals(skill != null ? skill.SkillId : string.Empty, "sein-a", StringComparison.OrdinalIgnoreCase))
            {
                multiplier *= HasManifestedChoice(runtime, "sein-a-trait-3") ? SpeedBonusToIntervalMultiplier(0.30f) : 1f;
            }
            else if (string.Equals(skill != null ? skill.SkillId : string.Empty, "vega-a", StringComparison.OrdinalIgnoreCase))
            {
                multiplier *= HasManifestedChoice(runtime, "vega-a-trait-3") ? SpeedBonusToIntervalMultiplier(0.25f) : 1f;
            }
            return Mathf.Max(0.25f, reload * multiplier);
        }

        private float ResolveManifestedShotInterval(CombatUnitRuntime runtime, SkillDefinition skill)
        {
            var interval = skill != null && skill.ShotIntervalSeconds > 0f
                ? skill.ShotIntervalSeconds
                : runtime != null && runtime.Monster != null ? runtime.Monster.ShotInterval : ManifestedMonsterAttackInterval;
            var multiplier = runtime != null && runtime.State != null && runtime.State.ShotIntervalMultiplier > 0f
                ? runtime.State.ShotIntervalMultiplier
                : 1f;
            if (string.Equals(skill != null ? skill.SkillId : string.Empty, "eve-a", StringComparison.OrdinalIgnoreCase))
            {
                multiplier *= HasManifestedChoice(runtime, "eve-a-trait-4") ? SpeedBonusToIntervalMultiplier(-0.25f) : 1f;
                multiplier *= HasManifestedChoice(runtime, "eve-a-master-2") ? SpeedBonusToIntervalMultiplier(-0.20f) : 1f;
            }
            else if (string.Equals(skill != null ? skill.SkillId : string.Empty, "rin-a", StringComparison.OrdinalIgnoreCase))
            {
                multiplier *= HasManifestedChoice(runtime, "rin-a-master-1") ? 0.82f : 1f;
            }
            else if (string.Equals(skill != null ? skill.SkillId : string.Empty, "sein-a", StringComparison.OrdinalIgnoreCase))
            {
                multiplier *= HasManifestedChoice(runtime, "sein-a-trait-5") ? 0.80f : 1f;
            }
            return Mathf.Max(0.05f, interval * multiplier);
        }

        private static float GetManifestedSkillCooldownRemaining(CombatSkillRuntime skillRuntime)
        {
            if (skillRuntime == null)
            {
                return 0f;
            }

            if (IsManifestedMagazineSkill(skillRuntime.Skill))
            {
                return skillRuntime.ReloadRemaining > 0f
                    ? Mathf.Max(0f, skillRuntime.ReloadRemaining)
                    : Mathf.Max(0f, skillRuntime.ShotCooldownRemaining);
            }

            return Mathf.Max(0f, skillRuntime.CooldownRemaining);
        }

        private static float GetManifestedSkillCooldownDuration(CombatSkillRuntime skillRuntime)
        {
            if (skillRuntime == null)
            {
                return 0f;
            }

            if (IsManifestedMagazineSkill(skillRuntime.Skill))
            {
                return skillRuntime.ReloadRemaining > 0f
                    ? Mathf.Max(0f, skillRuntime.ReloadDuration)
                    : Mathf.Max(0f, skillRuntime.ShotInterval);
            }

            return Mathf.Max(0f, skillRuntime.CooldownDuration);
        }

        private void SyncManifestedLearnedSkills(CombatUnitRuntime runtime)
        {
            if (runtime == null || runtime.Monster == null || runtime.State == null)
            {
                return;
            }

            runtime.Skills.RemoveAll(skillRuntime => skillRuntime == null || skillRuntime.Skill == null || !runtime.State.LearnedActives.Contains(skillRuntime.Skill.SkillId));
            var activeSkills = runtime.Monster.ActiveSkills ?? Array.Empty<SkillDefinition>();
            for (var i = 0; i < activeSkills.Length; i++)
            {
                var skill = activeSkills[i];
                if (skill == null || string.IsNullOrWhiteSpace(skill.SkillId) || !runtime.State.LearnedActives.Contains(skill.SkillId))
                {
                    continue;
                }

                if (FindCombatSkillRuntime(runtime, skill.SkillId) != null)
                {
                    continue;
                }

                var initialDelay = 0.25f + (runtime.Skills.Count * 0.15f);
                runtime.Skills.Add(new CombatSkillRuntime
                {
                    Skill = skill,
                    CooldownDuration = ResolveManifestedSkillCooldown(runtime, skill),
                    CooldownRemaining = IsManifestedMagazineSkill(skill) ? 0f : initialDelay,
                    MagazineCapacity = ResolveManifestedMagazineCapacity(runtime, skill),
                    ShotsRemaining = ResolveManifestedMagazineCapacity(runtime, skill),
                    ShotInterval = ResolveManifestedShotInterval(runtime, skill),
                    ShotCooldownRemaining = IsManifestedMagazineSkill(skill) ? initialDelay : 0f,
                    ReloadDuration = ResolveManifestedReloadDuration(runtime, skill),
                    ReloadRemaining = 0f
                });
            }

            runtime.Skills.Sort((left, right) => left.Skill.Slot.CompareTo(right.Skill.Slot));
        }

        private static CombatSkillRuntime FindCombatSkillRuntime(CombatUnitRuntime runtime, string skillId)
        {
            for (var i = 0; i < runtime.Skills.Count; i++)
            {
                var skillRuntime = runtime.Skills[i];
                if (skillRuntime != null && skillRuntime.Skill != null && string.Equals(skillRuntime.Skill.SkillId, skillId, StringComparison.OrdinalIgnoreCase))
                {
                    return skillRuntime;
                }
            }

            return null;
        }

        private void ClearManifestedMonsterParty()
        {
            for (var i = manifestedDrones.Count - 1; i >= 0; i--)
            {
                RemoveManifestedDroneAt(i);
            }

            for (var i = manifestedMonsters.Count - 1; i >= 0; i--)
            {
                var runtime = manifestedMonsters[i];
                if (runtime == null || runtime.GameObject == null)
                {
                    continue;
                }

                if (runtime.UsesSceneSlot)
                {
                    runtime.GameObject.SetActive(false);
                    if (runtime.Label != null)
                    {
                        runtime.Label.text = string.Empty;
                    }

                    if (runtime.NameLabel != null)
                    {
                        runtime.NameLabel.text = string.Empty;
                    }

                    UpdateManifestedHpShieldBarFill(runtime, 0f, runtime.MaxHealth, 0f);

                    continue;
                }

                if (Application.isPlaying)
                {
                    Destroy(runtime.GameObject);
                }
                else
                {
                    DestroyImmediate(runtime.GameObject);
                }
            }

            manifestedParty.ClearMonsters();
            CacheManifestedMonsterSlots();
            for (var i = 0; i < manifestedMonsterSlots.Length; i++)
            {
                if (manifestedMonsterSlots[i] != null)
                {
                    manifestedMonsterSlots[i].gameObject.SetActive(false);
                }
            }
        }
    }
}
