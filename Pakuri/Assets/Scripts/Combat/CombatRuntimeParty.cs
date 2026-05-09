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

        private sealed class ManifestedDroneRuntime
        {
            public CombatUnitRuntime Source;
            public SkillDefinition Skill;
            public GameObject GameObject;
            public Transform Transform;
            public SpriteRenderer Renderer;
            public float RemainingDuration;
            public float AttackCooldownRemaining;
        }

        private readonly List<CombatUnitRuntime> manifestedMonsters = new List<CombatUnitRuntime>();
        private readonly List<ManifestedDroneRuntime> manifestedDrones = new List<ManifestedDroneRuntime>();
        private readonly Transform[] manifestedMonsterSlots = new Transform[MaxManifestedPartyMonsterCount];

        private static readonly string[] ManifestedMonsterSlotNames =
        {
            "2PMonster",
            "3PMonster",
            "4PMonster",
            "5PMonster"
        };

        public int PartyMonsterCount => 1 + manifestedMonsters.Count;

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

                manifestedMonsters.Add(CreateCombatUnitRuntime(monster, session.EnsurePartyMemberState(monster), added));
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

        private readonly struct ManifestedMonsterStatusViews
        {
            public ManifestedMonsterStatusViews(TextMesh nameLabel, TextMesh hpLabel, SpriteRenderer hpBarFill, SpriteRenderer shieldBarFill)
            {
                NameLabel = nameLabel;
                HpLabel = hpLabel;
                HpBarFill = hpBarFill;
                ShieldBarFill = shieldBarFill;
            }

            public TextMesh NameLabel { get; }
            public TextMesh HpLabel { get; }
            public SpriteRenderer HpBarFill { get; }
            public SpriteRenderer ShieldBarFill { get; }
        }

        private ManifestedMonsterStatusViews ResolveManifestedMonsterStatusViews(Transform monsterTransform, bool preferSceneChildren)
        {
            if (monsterTransform == null)
            {
                return default;
            }

            var nameLabel = FindManifestedTextMesh(monsterTransform, "MonsterNameLabel", "Name Label", "NameLabel");
            var hpLabel = FindManifestedTextMesh(monsterTransform, "MonsterHpLabel", "HPLabel", "HPLable", "HP Label");
            var hpBar = FindManifestedSpriteRenderer(monsterTransform, "MonsterHpBar/Fill", "HPBar/Fill", "HpBar/Fill");
            var shieldBar = FindManifestedSpriteRenderer(monsterTransform, "MonsterHpBar/Shield", "HPBar/Shield", "HpBar/Shield");
            if (hpBar == null)
            {
                var generatedBar = EnsureManifestedHpBar(monsterTransform);
                hpBar = generatedBar.HpBarFill;
                shieldBar = shieldBar != null ? shieldBar : generatedBar.ShieldBarFill;
            }
            else
            {
                var normalizedBar = NormalizeManifestedHpBar(hpBar);
                hpBar = normalizedBar.HpBarFill != null ? normalizedBar.HpBarFill : hpBar;
                shieldBar = shieldBar != null ? shieldBar : normalizedBar.ShieldBarFill;
            }

            if (preferSceneChildren)
            {
                return new ManifestedMonsterStatusViews(nameLabel, hpLabel, hpBar, shieldBar);
            }

            return new ManifestedMonsterStatusViews(nameLabel, hpLabel, hpBar, shieldBar);
        }

        private static ManifestedMonsterStatusViews EnsureManifestedHpBar(Transform monsterTransform)
        {
            if (monsterTransform == null)
            {
                return default;
            }

            var barTransform = monsterTransform.Find("MonsterHpBar");
            if (barTransform == null)
            {
                var barObject = new GameObject("MonsterHpBar");
                barTransform = barObject.transform;
                barTransform.SetParent(monsterTransform, false);
                barTransform.localPosition = new Vector3(0f, 0.66f, 0f);
                barTransform.localScale = new Vector3(0.90f, 1f, 1f);
            }

            var background = EnsureManifestedBarRenderer(barTransform, "Background", Color.black, 34);
            if (background != null)
            {
                background.transform.localPosition = Vector3.zero;
                background.transform.localScale = new Vector3(1f, 0.08f, 1f);
            }

            var fill = EnsureManifestedBarRenderer(barTransform, "Fill", Color.red, 35);
            if (fill != null)
            {
                fill.transform.localPosition = new Vector3(0f, 0f, -0.01f);
                fill.transform.localScale = new Vector3(1f, 0.08f, 1f);
            }

            var shield = EnsureManifestedBarRenderer(barTransform, "Shield", Color.white, 36);
            if (shield != null)
            {
                shield.transform.localPosition = new Vector3(-0.5f, 0f, -0.02f);
                shield.transform.localScale = new Vector3(0f, 0.08f, 1f);
            }

            return new ManifestedMonsterStatusViews(null, null, fill, shield);
        }

        private static ManifestedMonsterStatusViews NormalizeManifestedHpBar(SpriteRenderer hpBarFill)
        {
            if (hpBarFill == null || hpBarFill.transform == null || hpBarFill.transform.parent == null)
            {
                return default;
            }

            var barTransform = hpBarFill.transform.parent;
            if (barTransform.localScale == Vector3.zero)
            {
                barTransform.localScale = new Vector3(0.90f, 1f, 1f);
            }

            var background = EnsureManifestedBarRenderer(barTransform, "Background", Color.black, 34);
            if (background != null)
            {
                background.transform.localPosition = Vector3.zero;
                if (Mathf.Approximately(background.transform.localScale.y, 0f))
                {
                    background.transform.localScale = new Vector3(1f, 0.08f, 1f);
                }
            }

            var fill = EnsureManifestedBarRenderer(barTransform, hpBarFill.transform.name, Color.red, 35);
            if (fill != null)
            {
                if (Mathf.Approximately(fill.transform.localScale.y, 0f))
                {
                    fill.transform.localScale = new Vector3(1f, 0.08f, 1f);
                }

                fill.transform.localPosition = new Vector3(fill.transform.localPosition.x, fill.transform.localPosition.y, -0.01f);
            }

            var shield = EnsureManifestedBarRenderer(barTransform, "Shield", Color.white, 36);
            if (shield != null)
            {
                if (Mathf.Approximately(shield.transform.localScale.y, 0f))
                {
                    shield.transform.localScale = new Vector3(0f, 0.08f, 1f);
                }

                shield.transform.localPosition = new Vector3(shield.transform.localPosition.x, shield.transform.localPosition.y, -0.02f);
            }

            return new ManifestedMonsterStatusViews(null, null, fill, shield);
        }

        private static SpriteRenderer EnsureManifestedBarRenderer(Transform parent, string childName, Color color, int sortingOrder)
        {
            if (parent == null || string.IsNullOrWhiteSpace(childName))
            {
                return null;
            }

            var child = parent.Find(childName);
            if (child == null)
            {
                var childObject = new GameObject(childName);
                child = childObject.transform;
                child.SetParent(parent, false);
            }

            var renderer = child.GetComponent<SpriteRenderer>();
            if (renderer == null)
            {
                renderer = child.gameObject.AddComponent<SpriteRenderer>();
            }

            renderer.sprite = GetSharedSprite();
            renderer.color = color;
            renderer.sortingOrder = sortingOrder;
            return renderer;
        }

        private static TextMesh FindManifestedTextMesh(Transform root, params string[] relativePaths)
        {
            if (root == null || relativePaths == null)
            {
                return null;
            }

            for (var i = 0; i < relativePaths.Length; i++)
            {
                var child = root.Find(relativePaths[i]);
                if (child == null)
                {
                    continue;
                }

                var text = child.GetComponent<TextMesh>();
                if (text != null)
                {
                    return text;
                }
            }

            return null;
        }

        private static SpriteRenderer FindManifestedSpriteRenderer(Transform root, params string[] relativePaths)
        {
            if (root == null || relativePaths == null)
            {
                return null;
            }

            for (var i = 0; i < relativePaths.Length; i++)
            {
                var child = root.Find(relativePaths[i]);
                if (child == null)
                {
                    continue;
                }

                var renderer = child.GetComponent<SpriteRenderer>();
                if (renderer != null)
                {
                    return renderer;
                }
            }

            return null;
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
            if (manifestedMonsters.Count == 0 || battleResolved)
            {
                return;
            }

            UpdateManifestedDrones();

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

                runtime.TickManifestedCombat(Time.deltaTime);
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

        internal void TickManifestedUnitSkill(CombatUnitRuntime runtime, CombatSkillRuntime skillRuntime, float elapsed)
        {
            if (runtime == null || runtime.Transform == null || skillRuntime == null || skillRuntime.Skill == null)
            {
                return;
            }

            if (TryTickEveUnitSkill(runtime, skillRuntime, elapsed))
            {
                return;
            }

            if (TryTickRinUnitSkill(runtime, skillRuntime, elapsed))
            {
                return;
            }

            TickCombatSkillRuntime(runtime, skillRuntime, elapsed);
            if (IsManifestedMagazineSkill(skillRuntime.Skill))
            {
                TryFireManifestedMagazineSkill(runtime, skillRuntime);
                return;
            }

            if (skillRuntime.CooldownRemaining > 0f)
            {
                return;
            }

            var target = FindNearestManifestedMonsterTarget(runtime.Transform.position);
            if (target == null)
            {
                skillRuntime.CooldownRemaining = 0.25f;
                return;
            }

            if (IsManifestedProjectileSkill(skillRuntime.Skill))
            {
                FireManifestedMonsterProjectile(runtime, skillRuntime.Skill, target);
            }
            else
            {
                FireManifestedMonsterSkill(runtime, skillRuntime, target);
            }

            skillRuntime.CooldownDuration = ResolveManifestedSkillCooldown(runtime, skillRuntime.Skill);
            skillRuntime.CooldownRemaining = skillRuntime.CooldownDuration;
        }

        private void TickCombatSkillRuntime(CombatUnitRuntime runtime, CombatSkillRuntime skillRuntime, float elapsed)
        {
            if (skillRuntime == null)
            {
                return;
            }

            skillRuntime.Tick(elapsed);
            UpdateManifestedQueuedProjectiles(runtime, skillRuntime, elapsed);
            skillRuntime.TickReload(elapsed, ResolveManifestedMagazineCapacity(runtime, skillRuntime.Skill));
        }

        private void TryFireManifestedMagazineSkill(CombatUnitRuntime runtime, CombatSkillRuntime skillRuntime)
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
            else if (IsManifestedEveDroneBeacon(skillRuntime.Skill))
            {
                DeployManifestedEveDroneBeacon(runtime, skillRuntime.Skill);
            }
            else if (IsManifestedProjectileSkill(skillRuntime.Skill))
            {
                FireManifestedMonsterProjectile(runtime, skillRuntime.Skill, target);
            }
            else
            {
                FireManifestedMonsterSkill(runtime, skillRuntime, target);
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

        private void FireManifestedMonsterSkill(CombatUnitRuntime runtime, CombatSkillRuntime skillRuntime, EnemyRuntime target)
        {
            var skill = skillRuntime != null ? skillRuntime.Skill : null;
            if (runtime == null || skill == null || target == null || runtime.Transform == null || target.Transform == null)
            {
                return;
            }

            if (TryFireManifestedRinShockwave(runtime, skillRuntime, target))
            {
                return;
            }

            if (TryFireManifestedPersistentSkill(runtime, skill, target))
            {
                return;
            }

            if (skill.RuntimeKind == SkillRuntimeKind.Buff || skill.RuntimeKind == SkillRuntimeKind.Shield)
            {
                CreateManifestedSkillVisual(runtime, skill, target);
                statusLabel = $"{runtime.Monster.DisplayName} {skill.DisplayName} activated.";
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

            CreateManifestedSkillVisual(runtime, skill, target);
            statusLabel = $"{runtime.Monster.DisplayName} {skill.DisplayName} hit for {appliedTotal:0.#}.";
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

            skillEffects.Add(effect);

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

        private void FireManifestedMonsterProjectile(CombatUnitRuntime runtime, SkillDefinition skill, EnemyRuntime target)
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
            FireManifestedMonsterProjectile(runtime, skill, runtime.Transform.position, direction, 1f, ResolveManifestedProjectilePierce(runtime, skill), 0);
        }

        private int ResolveManifestedProjectilePierce(CombatUnitRuntime runtime, SkillDefinition skill)
        {
            if (skill == null)
            {
                return 0;
            }

            var skillId = skill.SkillId ?? string.Empty;
            if (string.Equals(skillId, "ariel-a", StringComparison.OrdinalIgnoreCase))
            {
                var pierce = 1;
                pierce += HasManifestedChoice(runtime, "ariel-a-trait-4") ? 1 : 0;
                return Mathf.Max(0, pierce);
            }

            if (string.Equals(skillId, "sein-a", StringComparison.OrdinalIgnoreCase))
            {
                var pierce = 1;
                pierce += HasManifestedChoice(runtime, "sein-a-trait-4") ? 1 : 0;
                pierce += HasManifestedChoice(runtime, "sein-a-master-1") ? 1 : 0;
                return Mathf.Max(0, pierce);
            }

            if (string.Equals(skillId, "rin-a", StringComparison.OrdinalIgnoreCase))
            {
                return HasManifestedChoice(runtime, "rin-a-trait-4") ? 1 : 0;
            }

            return 0;
        }

        private void FireManifestedMonsterProjectile(
            CombatUnitRuntime runtime,
            SkillDefinition skill,
            Vector3 direction,
            float damageMultiplier,
            int remainingPierce,
            int nameMarkStacks)
        {
            var origin = runtime != null && runtime.Transform != null ? runtime.Transform.position : Vector3.zero;
            FireManifestedMonsterProjectile(runtime, skill, origin, direction, damageMultiplier, remainingPierce, nameMarkStacks);
        }

        private void FireManifestedMonsterProjectile(
            CombatUnitRuntime runtime,
            SkillDefinition skill,
            Vector3 origin,
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
            projectileObject.transform.position = origin;
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
                VegaNameMarkStacks = IsManifestedVegaThreeSwordFlurry(skill) ? Mathf.Max(0, nameMarkStacks) : 0,
                IsManifestedProjectile = true,
                ManifestedSource = runtime,
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
                if (!TryApplyRinUnitProjectileHit(projectile, enemy, out damageResult, out appliedDamage))
                {
                    damageResult = DamageCalculator.Resolve(
                        projectile.BaseDamage,
                        projectile.Attribute,
                        enemy.Defenses,
                        targetCriticalResistance: enemy.CriticalResistance,
                        finalDamageMultiplier: enemy.DamageTakenMultiplier);
                    appliedDamage = ApplyDamageToEnemy(enemy, damageResult.FinalDamage, damageResult.Attribute);
                }

                ApplyManifestedProjectileStatus(projectile, enemy);
                TryApplyProjectileBranch(projectile, enemy, damageResult.FinalDamage);
                ApplyManifestedProjectileSourceEffects(projectile, enemy, appliedDamage);
                if (projectile.VegaNameMarkStacks > 0)
                {
                    AddVegaNameMarks(enemy, projectile.VegaNameMarkStacks);
                }
                return true;
            }

            return false;
        }

        private void ApplyManifestedProjectileSourceEffects(ProjectileRuntime projectile, EnemyRuntime enemy, float appliedDamage)
        {
            if (projectile == null || enemy == null || appliedDamage <= 0f)
            {
                return;
            }

            if (string.Equals(projectile.SkillId, "sein-a", StringComparison.OrdinalIgnoreCase))
            {
                enemy.SeinScorchingArrowTimer = Mathf.Max(enemy.SeinScorchingArrowTimer, 4f);
                if (HasManifestedChoice(projectile.ManifestedSource, "sein-a-master-2"))
                {
                    ApplyManifestedAreaDamage(enemy.Transform.position, 1.35f, projectile.BaseDamage * 0.50f, DamageAttribute.Fire);
                }
            }
        }

        private void ApplyManifestedAreaDamage(Vector3 center, float radius, float baseDamage, DamageAttribute attribute)
        {
            if (baseDamage <= 0f || radius <= 0f)
            {
                return;
            }

            for (var i = 0; i < enemies.Count; i++)
            {
                var enemy = enemies[i];
                if (enemy == null || enemy.Transform == null || enemy.CurrentHealth <= 0f)
                {
                    continue;
                }

                if (Vector2.Distance(center, enemy.Transform.position) > radius + GetEnemyHitRadius(enemy))
                {
                    continue;
                }

                var damageResult = DamageCalculator.Resolve(
                    baseDamage,
                    attribute,
                    enemy.Defenses,
                    targetCriticalResistance: enemy.CriticalResistance,
                    finalDamageMultiplier: enemy.DamageTakenMultiplier);
                ApplyDamageToEnemy(enemy, damageResult.FinalDamage, damageResult.Attribute);
                enemy.FlashTimer = 0.08f;
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
            skillEffects.Add(effect);
            statusLabel = $"{runtime.Monster.DisplayName} {skill.DisplayName} field active.";
        }

        private void ApplyManifestedProjectileStatus(ProjectileRuntime projectile, EnemyRuntime enemy)
        {
            if (projectile == null || enemy == null || projectile.StatusChance <= 0f || UnityEngine.Random.value >= Mathf.Clamp01(projectile.StatusChance))
            {
                return;
            }

            var statusId = projectile.ManifestedStatusEffectId ?? string.Empty;
            if (statusId.Contains("媛먯쟾") || statusId.Contains("감전") || string.Equals(statusId, "shock", StringComparison.OrdinalIgnoreCase))
            {
                ApplyShock(enemy, Mathf.Max(1, projectile.StatusStacks), 1.25f);
            }
            else if (statusId.Contains("鍮숆껐") || statusId.Contains("?됯린") || statusId.Contains("빙결") || string.Equals(statusId, "chill", StringComparison.OrdinalIgnoreCase))
            {
                ApplyChill(enemy, Mathf.Max(1, projectile.StatusStacks), 2.5f);
            }
            else if (statusId.Contains("痍⑥빟") || statusId.Contains("취약") || string.Equals(statusId, "vulnerable", StringComparison.OrdinalIgnoreCase))
            {
                ApplyVulnerable(enemy, Mathf.Max(1, projectile.StatusStacks));
            }
        }

        private float ApplyManifestedSkillDamage(CombatUnitRuntime runtime, SkillDefinition skill, EnemyRuntime target)
        {
            return ApplyManifestedSkillDamage(runtime, skill, target, 1f);
        }

        private float ApplyManifestedSkillDamage(CombatUnitRuntime runtime, SkillDefinition skill, EnemyRuntime target, float finalMultiplier)
        {
            if (target == null || skill == null)
            {
                return 0f;
            }

            var baseDamage = ResolveManifestedBaseDamage(runtime, skill);
            var damageResult = DamageCalculator.Resolve(
                baseDamage,
                skill.Attribute,
                target.Defenses,
                targetCriticalResistance: target.CriticalResistance,
                finalDamageMultiplier: target.DamageTakenMultiplier * Mathf.Max(0f, finalMultiplier));
            var applied = ApplyDamageToEnemy(target, damageResult.FinalDamage, damageResult.Attribute);
            target.FlashTimer = 0.08f;
            return applied;
        }

        private void ApplyManifestedSkillEffectDamage(SkillEffectRuntime effect, EnemyRuntime target)
        {
            if (effect == null || effect.ManifestedSource == null || target == null || target.CurrentHealth <= 0f)
            {
                return;
            }

            var damageResult = DamageCalculator.Resolve(
                effect.BaseDamage,
                effect.Attribute,
                target.Defenses,
                targetCriticalResistance: target.CriticalResistance,
                finalDamageMultiplier: target.DamageTakenMultiplier);
            ApplyDamageToEnemy(target, damageResult.FinalDamage, damageResult.Attribute);
            target.FlashTimer = 0.08f;

            if (string.Equals(effect.SkillId, "eve-c", StringComparison.OrdinalIgnoreCase))
            {
                ApplyChill(target, Mathf.Max(1, effect.StatusStacks), 2.5f);
                if (effect.FreezeDuration > 0f)
                {
                    target.FreezeTimer = Mathf.Max(target.FreezeTimer, effect.FreezeDuration);
                }
            }

            if (string.Equals(effect.SkillId, "sein-d", StringComparison.OrdinalIgnoreCase)
                || string.Equals(effect.SkillId, "sein-d-residual", StringComparison.OrdinalIgnoreCase))
            {
                target.SeinSuperheatedZoneTimer = Mathf.Max(target.SeinSuperheatedZoneTimer, 0.7f);
                target.SeinSuperheatedTickCount += 1;
            }
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
            skillEffects.Add(effect);

            statusLabel = $"{runtime.Monster.DisplayName} {skill.DisplayName} frost field deployed.";
        }

        private float ResolveManifestedBaseDamage(CombatUnitRuntime runtime, SkillDefinition skill)
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
            return Mathf.Max(1f, (skill.BaseDamage + (runtime.PowerStat * coefficient)) * ResolveManifestedDamageMultiplier(runtime) * ResolveManifestedSkillDamageMultiplier(runtime, skill));
        }

        private float ResolveManifestedSkillDamageMultiplier(CombatUnitRuntime runtime, SkillDefinition skill)
        {
            if (skill == null)
            {
                return 1f;
            }

            var multiplier = 1f;
            var skillId = skill.SkillId ?? string.Empty;
            if (string.Equals(skillId, "ariel-a", StringComparison.OrdinalIgnoreCase))
            {
                multiplier *= HasManifestedChoice(runtime, "ariel-a-trait-1") ? 1.25f : 1f;
                multiplier *= HasManifestedChoice(runtime, "ariel-a-trait-5") ? 1.06f : 1f;
            }
            else if (string.Equals(skillId, "ariel-c", StringComparison.OrdinalIgnoreCase))
            {
                multiplier *= HasManifestedChoice(runtime, "ariel-c-trait-1") ? 1.25f : 1f;
            }
            else if (string.Equals(skillId, "ariel-d", StringComparison.OrdinalIgnoreCase))
            {
                multiplier *= HasManifestedChoice(runtime, "ariel-d-trait-1") ? 1.30f : 1f;
                multiplier *= HasManifestedChoice(runtime, "ariel-d-trait-4") ? 0.80f : 1f;
            }
            else if (string.Equals(skillId, "ariel-e", StringComparison.OrdinalIgnoreCase))
            {
                multiplier *= HasManifestedChoice(runtime, "ariel-e-trait-1") ? 1.30f : 1f;
                multiplier *= HasManifestedChoice(runtime, "ariel-e-master-2") ? 1.70f : 1f;
            }
            else if (string.Equals(skillId, "sein-a", StringComparison.OrdinalIgnoreCase))
            {
                multiplier *= HasManifestedChoice(runtime, "sein-a-trait-1") ? 1.25f : 1f;
                multiplier *= HasManifestedChoice(runtime, "sein-a-trait-4") ? 1.10f : 1f;
                multiplier *= HasManifestedChoice(runtime, "sein-a-trait-5") ? 0.90f : 1f;
                multiplier *= HasManifestedChoice(runtime, "sein-a-master-1") ? 1.55f : 1f;
            }
            else if (string.Equals(skillId, "sein-b", StringComparison.OrdinalIgnoreCase))
            {
                multiplier *= HasManifestedChoice(runtime, "sein-b-trait-2") ? 1.25f : 1f;
                multiplier *= HasManifestedChoice(runtime, "sein-b-master-1") ? 0.80f : 1f;
                multiplier *= HasManifestedChoice(runtime, "sein-b-master-2") ? 1.90f : 1f;
            }
            else if (string.Equals(skillId, "rin-a", StringComparison.OrdinalIgnoreCase))
            {
                multiplier *= HasManifestedChoice(runtime, "rin-a-trait-1") ? 1.25f : 1f;
                multiplier *= HasManifestedChoice(runtime, "rin-a-trait-4") ? 0.90f : 1f;
                multiplier *= HasManifestedChoice(runtime, "rin-a-master-1") ? 1.12f : 1f;
            }

            else if (string.Equals(skillId, "vega-b", StringComparison.OrdinalIgnoreCase))
            {
                multiplier *= HasManifestedChoice(runtime, "vega-b-trait-1") ? 1.25f : 1f;
                multiplier *= HasManifestedChoice(runtime, "vega-b-master-2") ? 1.70f : 1f;
            }
            else if (string.Equals(skillId, "vega-d", StringComparison.OrdinalIgnoreCase))
            {
                multiplier *= HasManifestedChoice(runtime, "vega-d-trait-1") ? 1.25f : 1f;
                multiplier *= HasManifestedChoice(runtime, "vega-d-master-2") ? 1.30f : 1f;
            }
            else if (string.Equals(skillId, "vega-e", StringComparison.OrdinalIgnoreCase))
            {
                multiplier *= HasManifestedChoice(runtime, "vega-e-trait-1") ? 1.25f : 1f;
                multiplier *= HasManifestedChoice(runtime, "vega-e-master-2") ? 0.80f : 1f;
            }

            return Mathf.Max(0f, multiplier);
        }

        private float ResolveManifestedDamageMultiplier(CombatUnitRuntime runtime)
        {
            return runtime != null && runtime.State != null && runtime.State.DamageMultiplier > 0f
                ? runtime.State.DamageMultiplier
                : 1f;
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

            skillRuntime.PendingVegaProjectileCount = 3;
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
            var damageMultiplier = projectileIndex >= 2 ? 2f : 1f;
            if (HasManifestedChoice(runtime, "vega-a-trait-1"))
            {
                damageMultiplier *= 1.20f;
            }

            if (projectileIndex >= 2 && HasManifestedChoice(runtime, "vega-a-trait-4"))
            {
                damageMultiplier += 0.50f;
            }

            var markStacks = 1 + (HasManifestedChoice(runtime, "vega-f-trait-3") ? 1 : 0);
            FireManifestedMonsterProjectile(runtime, skillRuntime.Skill, skillRuntime.PendingVegaProjectileDirection, damageMultiplier, 999, markStacks);
            skillRuntime.PendingVegaProjectileIndex += 1;
            skillRuntime.PendingVegaProjectileCount -= 1;
            if (skillRuntime.PendingVegaProjectileCount > 0)
            {
                skillRuntime.PendingVegaProjectileDelay = VegaThreeSwordBulletInterval;
            }
        }

        private void DeployManifestedEveDroneBeacon(CombatUnitRuntime runtime, SkillDefinition skill)
        {
            if (runtime == null || runtime.Transform == null || runtime.Monster == null || skill == null)
            {
                return;
            }

            var droneParent = projectileRoot != null ? projectileRoot : transform;
            var droneObject = new GameObject(string.IsNullOrWhiteSpace(skill.SkillId) ? "ManifestedDroneBeacon" : $"Manifested_{skill.SkillId}_Drone");
            droneObject.transform.SetParent(droneParent, false);
            droneObject.transform.position = runtime.Transform.position + new Vector3(0.45f, 0.45f, 0f);
            droneObject.transform.localScale = Vector3.one * 0.42f;

            var renderer = droneObject.AddComponent<SpriteRenderer>();
            renderer.sprite = runtime.Monster.UnitSprite != null ? runtime.Monster.UnitSprite : GetSharedSprite();
            renderer.color = runtime.Monster.ProjectileColor.a <= 0f ? new Color(0.75f, 0.95f, 1f, 0.85f) : runtime.Monster.ProjectileColor;
            renderer.sortingOrder = 26;

            manifestedDrones.Add(new ManifestedDroneRuntime
            {
                Source = runtime,
                Skill = skill,
                GameObject = droneObject,
                Transform = droneObject.transform,
                Renderer = renderer,
                RemainingDuration = ResolveManifestedSkillVisualDuration(runtime, skill),
                AttackCooldownRemaining = 0f
            });

            statusLabel = $"{runtime.Monster.DisplayName} {skill.DisplayName} drone deployed.";
        }

        private void UpdateManifestedDrones()
        {
            var elapsed = Time.deltaTime;
            for (var i = manifestedDrones.Count - 1; i >= 0; i--)
            {
                var drone = manifestedDrones[i];
                if (drone == null || drone.Transform == null || drone.GameObject == null || drone.Source == null || drone.Source.CurrentHealth <= 0f)
                {
                    RemoveManifestedDroneAt(i);
                    continue;
                }

                drone.RemainingDuration -= elapsed;
                if (drone.RemainingDuration <= 0f)
                {
                    RemoveManifestedDroneAt(i);
                    continue;
                }

                drone.AttackCooldownRemaining = Mathf.Max(0f, drone.AttackCooldownRemaining - elapsed);
                if (drone.AttackCooldownRemaining > 0f)
                {
                    continue;
                }

                var target = FindNearestManifestedMonsterTarget(drone.Transform.position);
                if (target == null || target.Transform == null)
                {
                    drone.AttackCooldownRemaining = 0.2f;
                    continue;
                }

                var direction = target.Transform.position - drone.Transform.position;
                direction.z = 0f;
                if (direction.sqrMagnitude <= 0.0001f)
                {
                    direction = Vector3.right;
                }

                FireManifestedMonsterProjectile(drone.Source, drone.Skill, drone.Transform.position, direction, 1f, 0, 1);
                drone.AttackCooldownRemaining = EveDroneAttackPeriod;
            }
        }

        private void RemoveManifestedDroneAt(int index)
        {
            if (index < 0 || index >= manifestedDrones.Count)
            {
                return;
            }

            var drone = manifestedDrones[index];
            if (drone != null && drone.GameObject != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(drone.GameObject);
                }
                else
                {
                    DestroyImmediate(drone.GameObject);
                }
            }

            manifestedDrones.RemoveAt(index);
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

        private float ResolveManifestedProjectileSpeed(CombatUnitRuntime runtime)
        {
            return runtime != null && runtime.Monster != null && runtime.Monster.ProjectileSpeed > 0f
                ? runtime.Monster.ProjectileSpeed
                : ManifestedMonsterProjectileSpeedFallback;
        }

        private float ResolveManifestedProjectileLifetime(CombatUnitRuntime runtime, SkillDefinition skill)
        {
            if (runtime != null && runtime.Monster != null && runtime.Monster.ProjectileLifetime > 0f)
            {
                return runtime.Monster.ProjectileLifetime;
            }

            var range = skill != null && skill.Range > 0f ? skill.Range : 8f;
            return Mathf.Max(0.5f, range / ResolveManifestedProjectileSpeed(runtime));
        }

        private float ResolveManifestedProjectileHitRadius(CombatUnitRuntime runtime)
        {
            return runtime != null && runtime.Monster != null && runtime.Monster.ProjectileHitRadius > 0f
                ? runtime.Monster.ProjectileHitRadius
                : 0.42f;
        }

        private float ResolveManifestedStatusChance(CombatUnitRuntime runtime)
        {
            var chance = runtime != null && runtime.Monster != null ? runtime.Monster.StatusChance : 0f;
            chance += runtime != null && runtime.State != null ? runtime.State.StatusChanceBonus : 0f;
            return Mathf.Clamp01(chance);
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

        private void CreateManifestedSkillVisual(CombatUnitRuntime runtime, SkillDefinition skill, EnemyRuntime target)
        {
            if (runtime == null || runtime.Transform == null || skill == null || target == null || target.Transform == null)
            {
                return;
            }

            var origin = runtime.Transform.position;
            var targetPosition = target.Transform.position;
            var duration = ResolveManifestedSkillVisualDuration(runtime, skill);
            switch (skill.RuntimeKind)
            {
                case SkillRuntimeKind.AreaAttack:
                case SkillRuntimeKind.Field:
                    CreateManifestedCircleVisual(
                        skill,
                        targetPosition,
                        Mathf.Max(0.75f, skill.Radius > 0f ? skill.Radius : GetEnemyHitRadius(target) + 0.35f),
                        new Color(1f, 1f, 1f, 0.58f),
                        23,
                        duration);
                    return;
                case SkillRuntimeKind.Buff:
                case SkillRuntimeKind.Shield:
                    CreateManifestedCircleVisual(
                        skill,
                        origin,
                        Mathf.Max(0.75f, skill.Radius > 0f ? skill.Radius : 0.9f),
                        new Color(0.78f, 0.95f, 1f, 0.56f),
                        24,
                        duration);
                    return;
                case SkillRuntimeKind.Execute:
                case SkillRuntimeKind.Mark:
                    CreateManifestedCircleVisual(
                        skill,
                        targetPosition,
                        Mathf.Max(0.65f, GetEnemyHitRadius(target) + 0.35f),
                        new Color(0.92f, 0.82f, 1f, 0.58f),
                        25,
                        duration);
                    return;
                case SkillRuntimeKind.LineAttack:
                    CreateManifestedLineSkillVisual(origin, targetPosition, skill, Mathf.Max(0.08f, skill.Radius > 0f ? skill.Radius : 0.28f), duration);
                    return;
                default:
                    CreateManifestedLineSkillVisual(origin, targetPosition, skill, 0.08f, duration);
                    return;
            }
        }

        private float ResolveManifestedSkillVisualDuration(CombatUnitRuntime runtime, SkillDefinition skill)
        {
            if (skill == null)
            {
                return ManifestedMonsterProjectileLifetime;
            }

            if (string.Equals(skill.SkillId, "eve-b", StringComparison.OrdinalIgnoreCase))
            {
                return EveBeamDuration;
            }

            if (string.Equals(skill.SkillId, "eve-c", StringComparison.OrdinalIgnoreCase))
            {
                return EveFrostFieldDuration;
            }

            if (string.Equals(skill.SkillId, "eve-e", StringComparison.OrdinalIgnoreCase))
            {
                return EveDroneDuration;
            }

            if (string.Equals(skill.SkillId, "sein-d", StringComparison.OrdinalIgnoreCase))
            {
                return SeinSuperheatedZoneDuration;
            }

            if (string.Equals(skill.SkillId, "vega-c", StringComparison.OrdinalIgnoreCase))
            {
                return VegaExterminationPermitDuration;
            }

            if (string.Equals(skill.SkillId, "ariel-b", StringComparison.OrdinalIgnoreCase))
            {
                return ArielRadiantShieldDuration;
            }

            if (string.Equals(skill.SkillId, "ariel-c", StringComparison.OrdinalIgnoreCase))
            {
                return ArielBlessingDuration;
            }

            switch (skill.RuntimeKind)
            {
                case SkillRuntimeKind.Field:
                    return 4f;
                case SkillRuntimeKind.Buff:
                case SkillRuntimeKind.Shield:
                    return 4f;
                case SkillRuntimeKind.LineAttack:
                    return 0.35f;
                case SkillRuntimeKind.AreaAttack:
                case SkillRuntimeKind.Execute:
                case SkillRuntimeKind.Mark:
                    return 0.28f;
                default:
                    return ManifestedMonsterProjectileLifetime;
            }
        }

        private void CreateManifestedCircleVisual(SkillDefinition skill, Vector3 position, float radius, Color color, int sortingOrder, float duration)
        {
            var effect = CombatEffectFactory.CreateCircle(
                string.IsNullOrWhiteSpace(skill.SkillId) ? "ManifestedMonsterSkillArea" : $"Manifested_{skill.SkillId}_Area",
                projectileRoot != null ? projectileRoot : transform,
                position,
                Mathf.Max(0.05f, radius),
                skill.SkillEffectPrefab,
                GetCircleSprite());
            ConfigureManifestedVisual(effect, color, sortingOrder, duration);
        }

        private void CreateManifestedLineSkillVisual(Vector3 origin, Vector3 target, SkillDefinition skill, float width, float duration)
        {
            var direction = target - origin;
            direction.z = 0f;
            if (direction.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            var distance = direction.magnitude;
            var effect = CombatEffectFactory.CreateLine(
                string.IsNullOrWhiteSpace(skill.SkillId) ? "ManifestedMonsterSkillLine" : $"Manifested_{skill.SkillId}_Line",
                projectileRoot != null ? projectileRoot : transform,
                origin,
                direction,
                distance,
                Mathf.Max(0.05f, width),
                skill.SkillEffectPrefab,
                GetSharedSprite());
            ConfigureManifestedVisual(effect, Color.white, 23, duration);
        }

        private void ConfigureManifestedVisual(CombatEffectInstance effect, Color color, int sortingOrder, float duration)
        {
            if (effect.Renderer != null)
            {
                effect.Renderer.color = color;
                effect.Renderer.sortingOrder = sortingOrder;
            }

            if (effect.GameObject != null)
            {
                Destroy(effect.GameObject, Mathf.Max(0.05f, duration));
            }
        }

        private void UpdateManifestedMonsterLabel(CombatUnitRuntime runtime)
        {
            if (runtime == null || runtime.Monster == null)
            {
                return;
            }

            var hpText = $"HP {Mathf.CeilToInt(Mathf.Max(0f, runtime.CurrentHealth))}/{Mathf.CeilToInt(runtime.MaxHealth)}";
            if (runtime.NameLabel != null)
            {
                runtime.NameLabel.text = runtime.Monster.DisplayName;
            }

            if (runtime.HpLabel != null)
            {
                runtime.HpLabel.text = hpText;
            }
            else if (runtime.Label != null)
            {
                var skillLine = runtime.Skills.Count > 0 && runtime.Skills[0] != null && runtime.Skills[0].Skill != null
                    ? $"{runtime.Skills[0].Skill.DisplayName} {Mathf.CeilToInt(Mathf.Max(0f, runtime.Skills[0].CooldownRemaining))}"
                    : "No learned active";
                runtime.Label.text = $"{runtime.Monster.DisplayName}\n{hpText}\n{skillLine}";
            }

            UpdateManifestedHpShieldBarFill(runtime, runtime.CurrentHealth, runtime.MaxHealth, 0f);
        }

        private static void UpdateManifestedHpShieldBarFill(CombatUnitRuntime runtime, float currentHealth, float maxHealth, float shieldValue)
        {
            if (runtime == null)
            {
                return;
            }

            var hpBarFill = runtime.HpBarFill;
            var shieldBarFill = runtime.ShieldBarFill;
            if (hpBarFill != null && hpBarFill.sprite == null)
            {
                var normalizedBar = NormalizeManifestedHpBar(hpBarFill);
                hpBarFill = normalizedBar.HpBarFill != null ? normalizedBar.HpBarFill : hpBarFill;
                shieldBarFill = shieldBarFill != null ? shieldBarFill : normalizedBar.ShieldBarFill;
            }

            UpdateHpShieldBarFill(hpBarFill, shieldBarFill, currentHealth, maxHealth, shieldValue);
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
