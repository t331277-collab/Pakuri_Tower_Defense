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

        private sealed class ManifestedMonsterRuntime
        {
            public MonsterDefinition Monster;
            public RunSession.RunMonsterState State;
            public GameObject GameObject;
            public Transform Transform;
            public SpriteRenderer Renderer;
            public TextMesh Label;
            public bool UsesSceneSlot;
            public float MaxHealth;
            public float CurrentHealth;
            public float BaseDamage;
            public float PowerStat;
            public readonly List<ManifestedSkillRuntime> Skills = new List<ManifestedSkillRuntime>();
        }

        private sealed class ManifestedSkillRuntime
        {
            public SkillDefinition Skill;
            public float CooldownRemaining;
            public float CooldownDuration;
            public int ShotsRemaining;
            public int MagazineCapacity;
            public float ShotCooldownRemaining;
            public float ShotInterval;
            public float ReloadRemaining;
            public float ReloadDuration;
            public int PendingVegaProjectileCount;
            public int PendingVegaProjectileIndex;
            public float PendingVegaProjectileDelay;
            public Vector3 PendingVegaProjectileDirection;
        }

        private readonly List<ManifestedMonsterRuntime> manifestedMonsters = new List<ManifestedMonsterRuntime>();
        private readonly Transform[] manifestedMonsterSlots = new Transform[MaxManifestedPartyMonsterCount];

        private static readonly string[] ManifestedMonsterSlotNames =
        {
            "2PMonster",
            "3PMonster",
            "4PMonster",
            "5PMonster"
        };

        public int PartyMonsterCount => 1 + manifestedMonsters.Count;

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

                manifestedMonsters.Add(CreateManifestedMonsterRuntime(monster, session.EnsurePartyMemberState(monster), added));
                added += 1;
            }
        }

        public void RefreshManifestedMonsterParty(RunSession session)
        {
            ConfigureManifestedMonsterParty(session);
            ResetManifestedMonsterPartyCombat();
        }

        private ManifestedMonsterRuntime CreateManifestedMonsterRuntime(MonsterDefinition monster, RunSession.RunMonsterState state, int index)
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

            var label = EnsureManifestedMonsterLabel(monsterObject.transform);

            var runtime = new ManifestedMonsterRuntime
            {
                Monster = monster,
                State = state,
                GameObject = monsterObject,
                Transform = monsterTransform,
                Renderer = renderer,
                Label = label,
                UsesSceneSlot = usesSceneSlot,
                MaxHealth = Mathf.Max(1f, monster.MaxHealth + (state != null ? state.MaxHealthBonus : 0f)),
                CurrentHealth = Mathf.Max(1f, monster.MaxHealth + (state != null ? state.MaxHealthBonus : 0f)),
                BaseDamage = Mathf.Max(1f, monster.BaseDamage),
                PowerStat = Mathf.Max(0f, monster.PowerStat),
            };
            SyncManifestedLearnedSkills(runtime);
            for (var i = 0; i < runtime.Skills.Count; i++)
            {
                ResetManifestedSkillRuntime(runtime, runtime.Skills[i], 0.4f + (index * 0.25f) + (i * 0.15f));
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

        private TextMesh EnsureManifestedMonsterLabel(Transform monsterTransform)
        {
            if (monsterTransform == null)
            {
                return null;
            }

            var labelTransform = monsterTransform.Find("PartyMonsterLabel");
            if (labelTransform == null)
            {
                labelTransform = new GameObject("PartyMonsterLabel").transform;
                labelTransform.SetParent(monsterTransform, false);
                labelTransform.localPosition = new Vector3(0f, 0.9f, 0f);
                labelTransform.localScale = new Vector3(0.12f, 0.12f, 1f);
            }

            var label = labelTransform.GetComponent<TextMesh>();
            if (label == null)
            {
                label = labelTransform.gameObject.AddComponent<TextMesh>();
            }

            label.anchor = TextAnchor.MiddleCenter;
            label.alignment = TextAlignment.Center;
            label.fontSize = 30;
            label.color = Color.white;
            var labelRenderer = label.GetComponent<MeshRenderer>();
            if (labelRenderer != null)
            {
                labelRenderer.sortingOrder = 38;
            }

            return label;
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
                    ResetManifestedSkillRuntime(runtime, runtime.Skills[skillIndex], 0.4f + (i * 0.25f) + (skillIndex * 0.15f));
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
            if (manifestedMonsters.Count == 0 || battleResolved)
            {
                return;
            }

            for (var i = 0; i < manifestedMonsters.Count; i++)
            {
                var runtime = manifestedMonsters[i];
                if (runtime == null || runtime.Transform == null || runtime.Monster == null || runtime.CurrentHealth <= 0f)
                {
                    continue;
                }

                SyncManifestedLearnedSkills(runtime);
                if (runtime.Skills.Count == 0)
                {
                    UpdateManifestedMonsterLabel(runtime);
                    continue;
                }

                for (var skillIndex = 0; skillIndex < runtime.Skills.Count; skillIndex++)
                {
                    var skillRuntime = runtime.Skills[skillIndex];
                    if (skillRuntime == null || skillRuntime.Skill == null)
                    {
                        continue;
                    }

                    TickManifestedSkillRuntime(runtime, skillRuntime);
                    if (IsManifestedMagazineSkill(skillRuntime.Skill))
                    {
                        TryFireManifestedMagazineSkill(runtime, skillRuntime);
                        continue;
                    }

                    if (skillRuntime.CooldownRemaining > 0f)
                    {
                        continue;
                    }

                    var target = FindNearestManifestedMonsterTarget(runtime.Transform.position);
                    if (target == null)
                    {
                        skillRuntime.CooldownRemaining = 0.25f;
                        continue;
                    }

                    if (IsManifestedProjectileSkill(skillRuntime.Skill))
                    {
                        FireManifestedMonsterProjectile(runtime, skillRuntime.Skill, target);
                    }
                    else
                    {
                        FireManifestedMonsterSkill(runtime, skillRuntime.Skill, target);
                    }

                    skillRuntime.CooldownDuration = ResolveManifestedSkillCooldown(runtime, skillRuntime.Skill);
                    skillRuntime.CooldownRemaining = skillRuntime.CooldownDuration;
                }

                UpdateManifestedMonsterLabel(runtime);
            }
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

        private void TickManifestedSkillRuntime(ManifestedMonsterRuntime runtime, ManifestedSkillRuntime skillRuntime)
        {
            if (skillRuntime == null)
            {
                return;
            }

            var elapsed = Time.deltaTime;
            skillRuntime.CooldownRemaining = Mathf.Max(0f, skillRuntime.CooldownRemaining - elapsed);
            skillRuntime.ShotCooldownRemaining = Mathf.Max(0f, skillRuntime.ShotCooldownRemaining - elapsed);
            UpdateManifestedQueuedProjectiles(runtime, skillRuntime, elapsed);
            if (skillRuntime.ReloadRemaining <= 0f)
            {
                return;
            }

            skillRuntime.ReloadRemaining = Mathf.Max(0f, skillRuntime.ReloadRemaining - elapsed);
            if (Mathf.Approximately(skillRuntime.ReloadRemaining, 0f))
            {
                skillRuntime.ShotsRemaining = Mathf.Max(1, ResolveManifestedMagazineCapacity(runtime, skillRuntime.Skill));
            }
        }

        private void TryFireManifestedMagazineSkill(ManifestedMonsterRuntime runtime, ManifestedSkillRuntime skillRuntime)
        {
            if (runtime == null || runtime.Transform == null || skillRuntime == null || skillRuntime.Skill == null)
            {
                return;
            }

            if (skillRuntime.ReloadRemaining > 0f || skillRuntime.ShotCooldownRemaining > 0f)
            {
                return;
            }

            if (skillRuntime.ShotsRemaining <= 0)
            {
                skillRuntime.ShotsRemaining = 0;
                skillRuntime.ReloadDuration = ResolveManifestedReloadDuration(runtime, skillRuntime.Skill);
                skillRuntime.ReloadRemaining = skillRuntime.ReloadDuration;
                return;
            }

            var target = FindNearestManifestedMonsterTarget(runtime.Transform.position);
            if (target == null)
            {
                skillRuntime.ShotCooldownRemaining = 0.25f;
                return;
            }

            if (IsManifestedVegaThreeSwordFlurry(skillRuntime.Skill))
            {
                QueueManifestedVegaThreeSwordFlurry(runtime, skillRuntime, target);
            }
            else if (IsManifestedProjectileSkill(skillRuntime.Skill))
            {
                FireManifestedMonsterProjectile(runtime, skillRuntime.Skill, target);
            }
            else
            {
                FireManifestedMonsterSkill(runtime, skillRuntime.Skill, target);
            }

            skillRuntime.ShotsRemaining -= 1;
            skillRuntime.ShotInterval = ResolveManifestedShotInterval(runtime, skillRuntime.Skill);
            skillRuntime.ShotCooldownRemaining = skillRuntime.ShotInterval;
            if (skillRuntime.ShotsRemaining <= 0)
            {
                skillRuntime.ShotsRemaining = 0;
                skillRuntime.ReloadDuration = ResolveManifestedReloadDuration(runtime, skillRuntime.Skill);
                skillRuntime.ReloadRemaining = skillRuntime.ReloadDuration;
            }
        }

        private void FireManifestedMonsterSkill(ManifestedMonsterRuntime runtime, SkillDefinition skill, EnemyRuntime target)
        {
            if (runtime == null || skill == null || target == null || runtime.Transform == null || target.Transform == null)
            {
                return;
            }

            var radius = Mathf.Max(0f, skill.Radius);
            var appliedTotal = 0f;
            if (radius > 0f && (skill.RuntimeKind == SkillRuntimeKind.AreaAttack || skill.RuntimeKind == SkillRuntimeKind.Field))
            {
                for (var i = 0; i < enemies.Count; i++)
                {
                    var enemy = enemies[i];
                    if (enemy == null || enemy.Transform == null || enemy.CurrentHealth <= 0f)
                    {
                        continue;
                    }

                    if (Vector2.Distance(target.Transform.position, enemy.Transform.position) > radius)
                    {
                        continue;
                    }

                    appliedTotal += ApplyManifestedSkillDamage(runtime, skill, enemy);
                }
            }
            else
            {
                appliedTotal = ApplyManifestedSkillDamage(runtime, skill, target);
            }

            CreateManifestedAttackVisual(runtime.Transform.position, target.Transform.position, runtime, skill);
            statusLabel = $"{runtime.Monster.DisplayName} {skill.DisplayName} hit for {appliedTotal:0.#}.";
        }

        private void FireManifestedMonsterProjectile(ManifestedMonsterRuntime runtime, SkillDefinition skill, EnemyRuntime target)
        {
            if (runtime == null || runtime.Monster == null || skill == null || target == null || runtime.Transform == null || target.Transform == null)
            {
                return;
            }

            var direction = target.Transform.position - runtime.Transform.position;
            direction.z = 0f;
            if (direction.sqrMagnitude <= 0.0001f)
            {
                direction = Vector3.right;
            }

            direction.Normalize();
            FireManifestedMonsterProjectile(runtime, skill, direction, 1f, 0, 1);
        }

        private void FireManifestedMonsterProjectile(
            ManifestedMonsterRuntime runtime,
            SkillDefinition skill,
            Vector3 direction,
            float damageMultiplier,
            int remainingPierce,
            int nameMarkStacks)
        {
            if (runtime == null || runtime.Monster == null || skill == null || runtime.Transform == null)
            {
                return;
            }

            direction.z = 0f;
            if (direction.sqrMagnitude <= 0.0001f)
            {
                direction = Vector3.right;
            }

            direction.Normalize();
            var projectileParent = projectileRoot != null ? projectileRoot : transform;
            var projectileObject = new GameObject(string.IsNullOrWhiteSpace(skill.SkillId) ? "ManifestedProjectile" : $"Manifested_{skill.SkillId}_Projectile");
            projectileObject.transform.SetParent(projectileParent, false);
            projectileObject.transform.position = runtime.Transform.position;
            projectileObject.transform.localScale = Vector3.one * 0.35f;
            projectileObject.transform.right = direction;

            var renderer = projectileObject.AddComponent<SpriteRenderer>();
            renderer.sprite = runtime.Monster.ProjectileSprite != null ? runtime.Monster.ProjectileSprite : GetSharedSprite();
            renderer.color = runtime.Monster.ProjectileColor.a <= 0f ? Color.white : runtime.Monster.ProjectileColor;
            renderer.sortingOrder = 24;

            projectiles.Add(new ProjectileRuntime
            {
                GameObject = projectileObject,
                Transform = projectileObject.transform,
                Renderer = renderer,
                Direction = direction,
                Speed = ResolveManifestedProjectileSpeed(runtime),
                RemainingLifetime = ResolveManifestedProjectileLifetime(runtime, skill),
                HitRadius = ResolveManifestedProjectileHitRadius(runtime),
                BaseDamage = ResolveManifestedBaseDamage(runtime, skill) * Mathf.Max(0f, damageMultiplier),
                Attribute = skill.Attribute,
                SkillId = skill.SkillId,
                RemainingPierce = Mathf.Max(0, remainingPierce),
                StatusStacks = 1,
                StatusChance = ResolveManifestedStatusChance(runtime),
                VegaNameMarkStacks = Mathf.Max(0, nameMarkStacks),
                IsManifestedProjectile = true,
                ManifestedSourceName = runtime.Monster.DisplayName,
                ManifestedSkillName = skill.DisplayName,
                ManifestedElementLabel = runtime.Monster.ElementLabel,
                ManifestedStatusEffectId = skill.StatusEffectId
            });

            statusLabel = $"{runtime.Monster.DisplayName} {skill.DisplayName} projectile fired.";
        }

        private bool TryHitManifestedProjectile(ProjectileRuntime projectile, out EnemyRuntime enemyHit, out DamageResult damageResult, out float appliedDamage)
        {
            enemyHit = null;
            damageResult = default;
            appliedDamage = 0f;
            if (projectile == null)
            {
                return false;
            }

            for (var i = 0; i < enemies.Count; i++)
            {
                var enemy = enemies[i];
                if (enemy == null || enemy.Transform == null || enemy.CurrentHealth <= 0f || projectile.HitEnemies.Contains(enemy))
                {
                    continue;
                }

                var hitDistance = GetEnemyHitRadius(enemy) + projectile.HitRadius;
                if (Vector2.Distance(projectile.Transform.position, enemy.Transform.position) > hitDistance)
                {
                    continue;
                }

                enemyHit = enemy;
                damageResult = DamageCalculator.Resolve(
                    projectile.BaseDamage,
                    projectile.Attribute,
                    enemy.Defenses,
                    targetCriticalResistance: enemy.CriticalResistance,
                    finalDamageMultiplier: enemy.DamageTakenMultiplier);
                appliedDamage = ApplyDamageToEnemy(enemy, damageResult.FinalDamage, damageResult.Attribute);
                ApplyManifestedProjectileStatus(projectile, enemy);
                if (projectile.VegaNameMarkStacks > 0)
                {
                    AddVegaNameMarks(enemy, projectile.VegaNameMarkStacks);
                }
                return true;
            }

            return false;
        }

        private void ApplyManifestedProjectileStatus(ProjectileRuntime projectile, EnemyRuntime enemy)
        {
            if (projectile == null || enemy == null || projectile.StatusChance <= 0f || UnityEngine.Random.value >= Mathf.Clamp01(projectile.StatusChance))
            {
                return;
            }

            var statusId = projectile.ManifestedStatusEffectId ?? string.Empty;
            if (statusId.Contains("감전") || string.Equals(statusId, "shock", StringComparison.OrdinalIgnoreCase))
            {
                ApplyShock(enemy, Mathf.Max(1, projectile.StatusStacks), 1.25f);
            }
            else if (statusId.Contains("빙결") || statusId.Contains("냉기") || string.Equals(statusId, "chill", StringComparison.OrdinalIgnoreCase))
            {
                ApplyChill(enemy, Mathf.Max(1, projectile.StatusStacks), 2.5f);
            }
            else if (statusId.Contains("취약") || string.Equals(statusId, "vulnerable", StringComparison.OrdinalIgnoreCase))
            {
                ApplyVulnerable(enemy, Mathf.Max(1, projectile.StatusStacks));
            }
        }

        private float ApplyManifestedSkillDamage(ManifestedMonsterRuntime runtime, SkillDefinition skill, EnemyRuntime target)
        {
            var baseDamage = ResolveManifestedBaseDamage(runtime, skill);
            var damageResult = DamageCalculator.Resolve(
                baseDamage,
                skill.Attribute,
                target.Defenses,
                targetCriticalResistance: target.CriticalResistance,
                finalDamageMultiplier: target.DamageTakenMultiplier);
            var applied = ApplyDamageToEnemy(target, damageResult.FinalDamage, damageResult.Attribute);
            target.FlashTimer = 0.08f;
            return applied;
        }

        private float ResolveManifestedBaseDamage(ManifestedMonsterRuntime runtime, SkillDefinition skill)
        {
            if (runtime == null || runtime.Monster == null)
            {
                return 1f;
            }

            if (skill == null)
            {
                var fallback = runtime.BaseDamage + (runtime.PowerStat * runtime.Monster.PowerCoefficient);
                return Mathf.Max(1f, fallback * ResolveManifestedDamageMultiplier(runtime));
            }

            var coefficient = Mathf.Max(skill.AttackPowerCoefficient, skill.SpellPowerCoefficient);
            return Mathf.Max(1f, (skill.BaseDamage + (runtime.PowerStat * coefficient)) * ResolveManifestedDamageMultiplier(runtime));
        }

        private float ResolveManifestedDamageMultiplier(ManifestedMonsterRuntime runtime)
        {
            return runtime != null && runtime.State != null && runtime.State.DamageMultiplier > 0f
                ? runtime.State.DamageMultiplier
                : 1f;
        }

        private float ResolveManifestedSkillCooldown(ManifestedMonsterRuntime runtime, SkillDefinition skill)
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
                return Mathf.Max(0.45f, skill.CooldownSeconds);
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

        private void QueueManifestedVegaThreeSwordFlurry(ManifestedMonsterRuntime runtime, ManifestedSkillRuntime skillRuntime, EnemyRuntime target)
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

            skillRuntime.PendingVegaProjectileCount = 3;
            skillRuntime.PendingVegaProjectileIndex = 0;
            skillRuntime.PendingVegaProjectileDelay = 0f;
            skillRuntime.PendingVegaProjectileDirection = direction.normalized;
            UpdateManifestedQueuedProjectiles(runtime, skillRuntime, 0f);
        }

        private void UpdateManifestedQueuedProjectiles(ManifestedMonsterRuntime runtime, ManifestedSkillRuntime skillRuntime, float elapsed)
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
            var damageMultiplier = projectileIndex >= 2 ? 2f : 1f;
            FireManifestedMonsterProjectile(runtime, skillRuntime.Skill, skillRuntime.PendingVegaProjectileDirection, damageMultiplier, 999, 1);
            skillRuntime.PendingVegaProjectileIndex += 1;
            skillRuntime.PendingVegaProjectileCount -= 1;
            if (skillRuntime.PendingVegaProjectileCount > 0)
            {
                skillRuntime.PendingVegaProjectileDelay = VegaThreeSwordBulletInterval;
            }
        }

        private void ResetManifestedSkillRuntime(ManifestedMonsterRuntime runtime, ManifestedSkillRuntime skillRuntime, float initialDelay)
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

        private int ResolveManifestedMagazineCapacity(ManifestedMonsterRuntime runtime, SkillDefinition skill)
        {
            var baseCapacity = skill != null && skill.MagazineCapacity > 0
                ? skill.MagazineCapacity
                : runtime != null && runtime.Monster != null ? runtime.Monster.MagazineCapacity : 1;
            var bonus = runtime != null && runtime.State != null ? runtime.State.MagazineBonus : 0;
            return Mathf.Max(1, baseCapacity + bonus);
        }

        private float ResolveManifestedReloadDuration(ManifestedMonsterRuntime runtime, SkillDefinition skill)
        {
            var reload = skill != null && skill.ReloadSeconds > 0f
                ? skill.ReloadSeconds
                : runtime != null && runtime.Monster != null ? runtime.Monster.ReloadDuration : 1f;
            var multiplier = runtime != null && runtime.State != null && runtime.State.ReloadDurationMultiplier > 0f
                ? runtime.State.ReloadDurationMultiplier
                : 1f;
            return Mathf.Max(0.25f, reload * multiplier);
        }

        private float ResolveManifestedShotInterval(ManifestedMonsterRuntime runtime, SkillDefinition skill)
        {
            var interval = skill != null && skill.ShotIntervalSeconds > 0f
                ? skill.ShotIntervalSeconds
                : runtime != null && runtime.Monster != null ? runtime.Monster.ShotInterval : ManifestedMonsterAttackInterval;
            var multiplier = runtime != null && runtime.State != null && runtime.State.ShotIntervalMultiplier > 0f
                ? runtime.State.ShotIntervalMultiplier
                : 1f;
            return Mathf.Max(0.05f, interval * multiplier);
        }

        private float ResolveManifestedProjectileSpeed(ManifestedMonsterRuntime runtime)
        {
            return runtime != null && runtime.Monster != null && runtime.Monster.ProjectileSpeed > 0f
                ? runtime.Monster.ProjectileSpeed
                : ManifestedMonsterProjectileSpeedFallback;
        }

        private float ResolveManifestedProjectileLifetime(ManifestedMonsterRuntime runtime, SkillDefinition skill)
        {
            if (runtime != null && runtime.Monster != null && runtime.Monster.ProjectileLifetime > 0f)
            {
                return runtime.Monster.ProjectileLifetime;
            }

            var range = skill != null && skill.Range > 0f ? skill.Range : 8f;
            return Mathf.Max(0.5f, range / ResolveManifestedProjectileSpeed(runtime));
        }

        private float ResolveManifestedProjectileHitRadius(ManifestedMonsterRuntime runtime)
        {
            return runtime != null && runtime.Monster != null && runtime.Monster.ProjectileHitRadius > 0f
                ? runtime.Monster.ProjectileHitRadius
                : 0.42f;
        }

        private float ResolveManifestedStatusChance(ManifestedMonsterRuntime runtime)
        {
            var chance = runtime != null && runtime.Monster != null ? runtime.Monster.StatusChance : 0f;
            chance += runtime != null && runtime.State != null ? runtime.State.StatusChanceBonus : 0f;
            return Mathf.Clamp01(chance);
        }

        private static float GetManifestedSkillCooldownRemaining(ManifestedSkillRuntime skillRuntime)
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

        private static float GetManifestedSkillCooldownDuration(ManifestedSkillRuntime skillRuntime)
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

        private void CreateManifestedAttackVisual(Vector3 origin, Vector3 target, ManifestedMonsterRuntime runtime, SkillDefinition skill)
        {
            var direction = target - origin;
            direction.z = 0f;
            if (direction.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            var distance = direction.magnitude;
            var effect = CombatEffectFactory.CreateLine(
                string.IsNullOrWhiteSpace(skill.SkillId) ? "ManifestedMonsterSkill" : $"Manifested_{skill.SkillId}",
                projectileRoot != null ? projectileRoot : transform,
                origin,
                direction,
                distance,
                0.08f,
                skill.SkillEffectPrefab,
                GetSharedSprite());
            if (effect.Renderer != null)
            {
                effect.Renderer.color = Color.white;
                effect.Renderer.sortingOrder = 23;
            }

            if (effect.GameObject != null)
            {
                Destroy(effect.GameObject, ManifestedMonsterProjectileLifetime);
            }
        }

        private void UpdateManifestedMonsterLabel(ManifestedMonsterRuntime runtime)
        {
            if (runtime == null || runtime.Label == null || runtime.Monster == null)
            {
                return;
            }

            var skillLine = runtime.Skills.Count > 0 && runtime.Skills[0] != null && runtime.Skills[0].Skill != null
                ? $"{runtime.Skills[0].Skill.DisplayName} {Mathf.CeilToInt(Mathf.Max(0f, runtime.Skills[0].CooldownRemaining))}"
                : "No learned active";
            runtime.Label.text = $"{runtime.Monster.DisplayName}\nHP {Mathf.CeilToInt(Mathf.Max(0f, runtime.CurrentHealth))}/{Mathf.CeilToInt(runtime.MaxHealth)}\n{skillLine}";
        }

        private void SyncManifestedLearnedSkills(ManifestedMonsterRuntime runtime)
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

                if (FindManifestedSkillRuntime(runtime, skill.SkillId) != null)
                {
                    continue;
                }

                var initialDelay = 0.25f + (runtime.Skills.Count * 0.15f);
                runtime.Skills.Add(new ManifestedSkillRuntime
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

        private static ManifestedSkillRuntime FindManifestedSkillRuntime(ManifestedMonsterRuntime runtime, string skillId)
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

            manifestedMonsters.Clear();
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
