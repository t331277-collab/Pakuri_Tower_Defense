using System;
using System.Collections.Generic;
using System.Globalization;
using Pakuri.Combat;
using Pakuri.Data;
using UnityEngine;

namespace Pakuri.InGame
{
    public static class InGameSkillDefinitionMapper
    {
        public static SkillData CreateActiveSkillData(MonsterDefinition monster, SkillDefinition source)
        {
            if (source == null)
            {
                return null;
            }

            var skill = CreateConcreteActiveSkill(source);
            MapCommonFields(skill, monster != null ? monster.MonsterId : string.Empty, source, monster != null ? monster.SkillTriggers : null);
            MapActiveFields(skill, monster, source);
            return skill;
        }

        public static SkillData CreateActiveSkillData(string monsterId, SkillDefinition source)
        {
            if (source == null)
            {
                return null;
            }

            var skill = CreateConcreteActiveSkill(source);
            MapCommonFields(skill, monsterId, source);
            MapActiveFields(skill, null, source);
            return skill;
        }

        public static PassiveSkillData CreatePassiveSkillData(MonsterDefinition monster, PassiveDefinition source)
        {
            if (source == null)
            {
                return null;
            }

            var skill = CreateTransient<PassiveSkillData>(source.PassiveId);
            skill.SkillId = source.PassiveId;
            skill.SkillName = source.DisplayName;
            skill.Character = MapCharacter(monster != null ? monster.MonsterId : string.Empty);
            skill.Slot = MapSlot(source.Slot);
            skill.IsActive = false;
            skill.Element = monster != null ? MapElement(monster.PrimaryAttribute) : ElementType.Physical;
            skill.Description = source.DescriptionText;
            skill.Icon = source.SkillIcon;
            skill.SkillEffectPrefab = source.SkillEffectPrefab;
            skill.EnhancementChoices = MapChoices(source.EnhancementChoices);
            skill.MasterChoices = Array.Empty<SkillChoiceEffectSpec>();
            skill.MultiEffects = source.PassiveEffects ?? Array.Empty<SkillEffectDefinition>();
            skill.SkillTriggers = FilterSkillTriggersForSkill(monster != null ? monster.SkillTriggers : null, source.PassiveId);
            skill.NormalizedPlanNodes = MapSkillNodeDefinitions(source.NormalizedPlanNodes);
            skill.TriggerType = PassiveTrigger.Always;
            skill.ApplyTarget = PassiveTarget.Self;
            return skill;
        }

        private static SkillData CreateConcreteActiveSkill(SkillDefinition source)
        {
            switch (source.RuntimeKind)
            {
                case SkillRuntimeKind.MagazineProjectile:
                case SkillRuntimeKind.CooldownProjectile:
                    return CreateTransient<ProjectileSkillData>(source.SkillId);
                case SkillRuntimeKind.LineAttack:
                    return CreateTransient<BeamSkillData>(source.SkillId);
                case SkillRuntimeKind.SingleAttack:
                    return CreateTransient<SingleAttackData>(source.SkillId);
                case SkillRuntimeKind.AreaAttack:
                case SkillRuntimeKind.Field:
                case SkillRuntimeKind.Mark:
                case SkillRuntimeKind.Execute:
                    return CreateTransient<ZoneSkillData>(source.SkillId);
                case SkillRuntimeKind.Buff:
                case SkillRuntimeKind.Heal:
                    return CreateTransient<BuffSkillData>(source.SkillId);
                case SkillRuntimeKind.Shield:
                    return CreateTransient<ShieldSkillData>(source.SkillId);
                case SkillRuntimeKind.Passive:
                    return CreateTransient<PassiveSkillData>(source.SkillId);
                default:
                    return CreateTransient<ProjectileSkillData>(source.SkillId);
            }
        }

        private static T CreateTransient<T>(string objectName)
            where T : ScriptableObject
        {
            var instance = ScriptableObject.CreateInstance<T>();
            instance.name = string.IsNullOrWhiteSpace(objectName) ? typeof(T).Name : objectName;
            instance.hideFlags = HideFlags.DontSave;
            return instance;
        }

        private static void MapCommonFields(
            SkillData skill,
            string monsterId,
            SkillDefinition source,
            SkillTriggerDefinition[] monsterTriggers = null)
        {
            skill.SkillId = source.SkillId;
            skill.SkillName = source.DisplayName;
            skill.Character = MapCharacter(monsterId);
            skill.Slot = MapSlot(source.Slot);
            skill.IsActive = source.RuntimeKind != SkillRuntimeKind.Passive;
            skill.Element = MapElement(source.Attribute);
            skill.Description = source.DescriptionText;
            skill.Icon = source.SkillIcon;
            skill.SkillEffectPrefab = source.SkillEffectPrefab;
            skill.EnhancementChoices = MapChoices(source.EnhancementChoices);
            skill.MasterChoices = MapChoices(source.MasterSkillChoices);
            skill.MultiEffects = source.MultiEffects ?? Array.Empty<SkillEffectDefinition>();
            skill.SkillTriggers = FilterSkillTriggersForSkill(monsterTriggers, source.SkillId);
            skill.NormalizedPlanNodes = MapSkillNodeDefinitions(source.NormalizedPlanNodes);

            skill.Timing.Cooldown = source.CooldownSeconds;
            skill.Timing.ActiveDuration = source.ActiveDurationSeconds;
            skill.Timing.TickInterval = source.ShotIntervalSeconds;
            skill.MagazineCapacity = source.MagazineCapacity;
            skill.ReloadSeconds = source.ReloadSeconds;
            skill.Targeting.Range = 0f;
            skill.Targeting.Radius = source.Radius;
            if (Enum.TryParse<SkillTargetSelection>(source.TargetSelection, true, out var targetSelection))
            {
                skill.Targeting.Selection = targetSelection;
            }

            skill.Targeting.SelectionStatusId = source.TargetSelectionStatusId;
            skill.Targeting.SelectionStatusMinStacks = Mathf.Max(0, source.TargetSelectionStatusMinStacks);
            skill.Targeting.Shape = MapShape(source.RuntimeKind);
            skill.Targeting.CoverAll = source.RuntimeKind == SkillRuntimeKind.Field
                || (source.RuntimeKind == SkillRuntimeKind.SingleAttack
                    && source.Radius <= 0f
                    && string.IsNullOrWhiteSpace(source.TargetSelection));
        }

        private static SkillTriggerDefinition[] FilterSkillTriggersForSkill(
            SkillTriggerDefinition[] triggers,
            string skillId)
        {
            if (triggers == null || triggers.Length == 0 || string.IsNullOrWhiteSpace(skillId))
            {
                return Array.Empty<SkillTriggerDefinition>();
            }

            var count = 0;
            for (var i = 0; i < triggers.Length; i++)
            {
                if (IsTriggerOwnedBySkill(triggers[i], skillId))
                {
                    count++;
                }
            }

            if (count == 0)
            {
                return Array.Empty<SkillTriggerDefinition>();
            }

            var filtered = new SkillTriggerDefinition[count];
            var index = 0;
            for (var i = 0; i < triggers.Length; i++)
            {
                if (IsTriggerOwnedBySkill(triggers[i], skillId))
                {
                    filtered[index] = triggers[i];
                    index++;
                }
            }

            return filtered;
        }

        private static bool IsTriggerOwnedBySkill(SkillTriggerDefinition trigger, string skillId)
        {
            return trigger != null
                && !string.IsNullOrWhiteSpace(trigger.SourceSkillId)
                && string.Equals(trigger.SourceSkillId, skillId, StringComparison.OrdinalIgnoreCase);
        }

        private static void MapActiveFields(SkillData skill, MonsterDefinition monster, SkillDefinition source)
        {
            if (skill is ProjectileSkillData projectile)
            {
                projectile.Projectile.MagazineSize = source.MagazineCapacity;
                projectile.Projectile.ReloadTime = source.ReloadSeconds;
                projectile.Projectile.BurstProjectileCount = Math.Max(1, source.ProjectileBurstCount);
                projectile.Projectile.BurstIntervalSeconds = source.BurstIntervalSeconds > 0f
                    ? source.BurstIntervalSeconds
                    : source.ShotIntervalSeconds;
                projectile.Projectile.BurstDamageProjectileIndex = source.BurstDamageProjectileIndex;
                projectile.Projectile.BurstDamageMultiplier = source.BurstDamageMultiplier > 0f
                    ? source.BurstDamageMultiplier
                    : 1f;
                projectile.Projectile.ProjectilesPerShot = 1;
                projectile.Projectile.PierceCount = source.PierceCount;
                projectile.Projectile.ProjectileSpeed = source.ProjectileSpeed;
                projectile.ContactDamageEnabled = source.DamageDelaySeconds <= 0f;
                projectile.StopOnFirstHit = source.DamageDelaySeconds > 0f;
                projectile.ImpactDelaySeconds = Mathf.Max(0f, source.DamageDelaySeconds);
                projectile.ImpactEffectPrefab = source.SkillEffectPrefab;
                projectile.HasImpactArea = source.DamageDelaySeconds > 0f;
                projectile.ImpactArea.Radius = source.Radius;
                projectile.ImpactArea.CoverAll = false;
                MapDamage(projectile.Damage, source);
                MapDamage(projectile.ImpactDamage, source);
                projectile.OnHitStatus = CreateStatusApplication(source);
                projectile.ImpactStatus = CreateStatusApplication(source);
                return;
            }

            if (skill is BeamSkillData beam)
            {
                beam.BeamLength = 0f;
                beam.BeamWidth = source.Radius;
                beam.KnockbackDistance = source.KnockbackDistance;
                MapDamage(beam.DamagePerTick, source);
                beam.OnHitStatus = CreateStatusApplication(source);
                return;
            }

            if (skill is ZoneSkillData zone)
            {
                var hasHitTargetCount = TryResolveHitTargetCount(
                    source.HitTargetCount,
                    out var hitAllTargets,
                    out var hitTargetCount);
                zone.Area.Radius = source.Radius;
                zone.Area.Duration = source.ActiveDurationSeconds > 0f
                    ? source.ActiveDurationSeconds
                    : source.CooldownSeconds;
                zone.Area.TickInterval = source.ShotIntervalSeconds;
                zone.UsesHitTargetCount = hasHitTargetCount;
                zone.HitAllTargets = hitAllTargets;
                zone.HitTargetCount = hitAllTargets ? int.MaxValue : Math.Max(1, hitTargetCount);
                zone.Area.CoverAll = source.RuntimeKind == SkillRuntimeKind.Field || hitAllTargets;
                MapDamage(zone.DamagePerTick, source);
                zone.OnTickStatus = CreateStatusApplication(source);
                return;
            }

            if (skill is SingleAttackData single)
            {
                var hasHitTargetCount = TryResolveHitTargetCount(
                    source.HitTargetCount,
                    out var hitAllTargets,
                    out var hitTargetCount);
                var hasStatusFilteredDeployment = !string.IsNullOrWhiteSpace(source.DeploymentRequiredTargetStatusId);
                var useMultiDeployment = !hitAllTargets
                    && hasHitTargetCount
                    && hitTargetCount > 1
                    && source.SkillEffectPrefab != null;
                single.Area.Radius = source.Radius;
                single.Area.Duration = 0f;
                single.Area.TickInterval = 0f;
                single.UsesHitTargetCount = !useMultiDeployment && (hasHitTargetCount || source.Radius <= 0f);
                single.UsePrefabHitbox = hitAllTargets || useMultiDeployment || hasStatusFilteredDeployment;
                single.UseMultiDeployment = useMultiDeployment || hasStatusFilteredDeployment;
                single.HitAllTargets = hitAllTargets;
                single.HitTargetCount = hitAllTargets ? int.MaxValue : Math.Max(1, hitTargetCount);
                single.DeploymentCount = useMultiDeployment ? Math.Max(1, hitTargetCount) : 1;
                single.DeploymentRequiredTargetStatusId = source.DeploymentRequiredTargetStatusId;
                single.DeploymentRequiredTargetStatusMinStacks = Mathf.Max(0, source.DeploymentRequiredTargetStatusMinStacks);
                single.TargetStatusStackStatusId = source.TargetStatusStackStatusId;
                single.TargetStatusStackMaxStacks = Mathf.Max(0, source.TargetStatusStackMaxStacks);
                single.ConsumeTargetStatusId = source.ConsumeTargetStatusId;
                single.ConsumeTargetStatusRatio = Mathf.Clamp01(source.ConsumeTargetStatusRatio);
                single.ConsumeTargetStatusStacks = Mathf.Max(0, source.ConsumeTargetStatusStacks);
                single.DamageDelaySeconds = Mathf.Max(0f, source.DamageDelaySeconds);
                single.ExecuteHealthRatioThreshold = Mathf.Clamp01(source.ExecuteHealthRatioThreshold);
                single.RequireExecuteThresholdToCast = source.RequireExecuteThresholdToCast;
                single.ExecuteDamageMultiplier = source.ExecuteDamageMultiplier > 0f ? source.ExecuteDamageMultiplier : 1f;
                single.KillCooldownRefundRatio = Mathf.Clamp01(source.KillCooldownRefundRatio);
                single.BossDamageMultiplier = source.BossDamageMultiplier > 0f ? source.BossDamageMultiplier : 1f;
                single.Area.CoverAll = hitAllTargets
                    || (!single.UsesHitTargetCount
                        && source.Radius <= 0f
                        && string.IsNullOrWhiteSpace(source.TargetSelection));
                MapDamage(single.Damage, source);
                single.TargetStatusStackDamage.Element = MapElement(source.Attribute);
                single.TargetStatusStackDamage.BaseDamage = source.TargetStatusStackBaseDamage;
                single.TargetStatusStackDamage.StatCoefficient = GetDominantCoefficient(
                    source.TargetStatusStackAttackPowerCoefficient,
                    source.TargetStatusStackSpellPowerCoefficient,
                    out var targetStatusStatSource);
                single.TargetStatusStackDamage.StatSource = targetStatusStatSource;
                single.TargetStatusStackDamage.CriticalAllowed = false;
                single.OnHitStatus = CreateStatusApplication(source);
                return;
            }

            if (skill is BuffSkillData buff)
            {
                buff.Target = MapBuffTarget(source, StatusEffectKind.None);
                buff.BuffDuration = ResolveStatusDuration(source);
                buff.HasAttachedDamage = source.BaseDamage > 0f;
                MapDamage(buff.AttachedDamage, source);
                buff.AttachedDamageRadius = source.Radius;
                buff.AttachedStatus = CreateStatusApplication(source);
                return;
            }

            if (skill is ShieldSkillData shield)
            {
                shield.Target = MapBuffTarget(source, StatusEffectKind.Shield);
                shield.ShieldBase = source.BaseDamage;
                shield.ShieldCoefficient = GetDominantCoefficient(source, out var statSource);
                shield.ShieldStatSource = statSource;
                shield.ShieldDuration = ResolveStatusDuration(source);
                shield.RefreshRule = StatusEffectRuntime.TryParseShieldRefreshPolicy(source.ShieldAmountRefreshPolicy, out var refreshRule)
                    ? refreshRule
                    : ShieldRefreshRule.TakeHighest;
                shield.ShieldStatus = CreateRuntimeStatusData(source);
                shield.ReflectElement = MapElement(source.Attribute);
            }
        }

        private static void MapDamage(SkillDamageSpec damage, SkillDefinition source)
        {
            damage.SkillId = source.SkillId;
            damage.Element = MapElement(source.Attribute);
            damage.BaseDamage = source.BaseDamage;
            damage.StatCoefficient = GetDominantCoefficient(source, out var statSource);
            damage.StatSource = statSource;
            damage.CriticalAllowed = source.CriticalAllowed;
        }

        private static float GetDominantCoefficient(SkillDefinition source, out StatSource statSource)
        {
            if (Mathf.Abs(source.SpellPowerCoefficient) >= Mathf.Abs(source.AttackPowerCoefficient))
            {
                statSource = StatSource.Intelligence;
                return source.SpellPowerCoefficient;
            }

            statSource = StatSource.Attack;
            return source.AttackPowerCoefficient;
        }

        private static float GetDominantCoefficient(float attackPowerCoefficient, float spellPowerCoefficient, out StatSource statSource)
        {
            if (Mathf.Abs(spellPowerCoefficient) >= Mathf.Abs(attackPowerCoefficient))
            {
                statSource = StatSource.Intelligence;
                return spellPowerCoefficient;
            }

            statSource = StatSource.Attack;
            return attackPowerCoefficient;
        }

        private static StatusApplicationSpec CreateStatusApplication(SkillDefinition source)
        {
            var application = new StatusApplicationSpec();
            var status = CreateRuntimeStatusData(source);
            application.Status = status;
            application.Chance = Mathf.Clamp01(source != null ? source.StatusChance : 0f);
            application.Stacks = status != null ? Math.Max(1, status.BaseStackAmount) : 1;
            application.RefreshDuration = true;
            return application;
        }

        private static StatusEffectData CreateRuntimeStatusData(SkillDefinition source)
        {
            if (source == null)
            {
                return null;
            }

            var statusKey = !string.IsNullOrWhiteSpace(source.StatusEffectId)
                ? source.StatusEffectId.Trim()
                : source.StatusEffectLabel;
            if (string.IsNullOrWhiteSpace(statusKey) || !StatusEffectUtility.TryParse(statusKey, out var kind))
            {
                return null;
            }

            var status = StatusEffectRuntime.CreateStatusData(kind, source.StatusEffectLabel, source);
            if (status != null && source.StatusEffectPrefab != null)
            {
                status.StatusEffectPrefab = source.StatusEffectPrefab;
            }

            return status;
        }

        private static bool TryResolveHitTargetCount(
            string rawValue,
            out bool hitAllTargets,
            out int hitTargetCount)
        {
            hitAllTargets = false;
            hitTargetCount = 1;
            if (string.IsNullOrWhiteSpace(rawValue))
            {
                return false;
            }

            var normalized = rawValue.Trim();
            if (string.Equals(normalized, "global", StringComparison.OrdinalIgnoreCase)
                || string.Equals(normalized, "all", StringComparison.OrdinalIgnoreCase))
            {
                hitAllTargets = true;
                hitTargetCount = int.MaxValue;
                return true;
            }

            if (int.TryParse(normalized, out var parsed) && parsed > 0)
            {
                hitTargetCount = parsed;
                return true;
            }

            return true;
        }

        private static BuffTarget MapBuffTarget(SkillDefinition source, StatusEffectKind fallbackKind)
        {
            if (source != null && StatusEffectRuntime.TryParseStatusTargetScope(source.StatusTargetScope, out var scope))
            {
                return scope == StatusTargetScope.Self ? BuffTarget.Self : BuffTarget.AllAllies;
            }

            if (source != null)
            {
                var statusKey = !string.IsNullOrWhiteSpace(source.StatusEffectId)
                    ? source.StatusEffectId
                    : source.StatusEffectLabel;
                if (StatusEffectUtility.TryParse(statusKey, out var parsedKind)
                    && parsedKind == StatusEffectKind.SlaughterPermit)
                {
                    return BuffTarget.Self;
                }
            }

            return fallbackKind == StatusEffectKind.Shield ? BuffTarget.AllAllies : BuffTarget.AllAllies;
        }

        private static float ResolveStatusDuration(SkillDefinition source)
        {
            if (source == null)
            {
                return 0f;
            }

            if (source.StatusDurationSeconds > 0f)
            {
                return source.StatusDurationSeconds;
            }

            if (source.ActiveDurationSeconds > 0f)
            {
                return source.ActiveDurationSeconds;
            }

            return source.CooldownSeconds;
        }

        private static SkillChoiceEffectSpec[] MapChoices(SkillChoiceDefinition[] source)
        {
            if (source == null || source.Length == 0)
            {
                return Array.Empty<SkillChoiceEffectSpec>();
            }

            var mapped = new SkillChoiceEffectSpec[source.Length];
            for (var i = 0; i < source.Length; i++)
            {
                var choice = source[i];
                var spec = new SkillChoiceEffectSpec
                {
                    ChoiceId = choice != null ? choice.ChoiceId : string.Empty,
                    Title = choice != null ? choice.Title : string.Empty,
                    Description = choice != null ? choice.DescriptionText : string.Empty,
                    Icon = choice != null ? choice.SkillIcon : null,
                    SkillEffectPrefab = choice != null ? choice.SkillEffectPrefab : null,
                    HasDamageMultiplier = choice != null && choice.HasDamageMultiplier,
                    DamageMultiplier = choice != null && choice.HasDamageMultiplier ? choice.DamageMultiplier : 1f,
                    HasShieldAmountMultiplier = false,
                    ShieldAmountMultiplier = 1f,
                    BaseDamageBonus = choice != null ? choice.BaseDamageBonus : 0f,
                    HasCooldownMultiplier = choice != null && choice.HasCooldownMultiplier,
                    CooldownMultiplier = choice != null && choice.HasCooldownMultiplier ? choice.CooldownMultiplier : 1f,
                    HasRadiusMultiplier = choice != null && choice.HasRadiusMultiplier,
                    RadiusMultiplier = choice != null && choice.HasRadiusMultiplier ? choice.RadiusMultiplier : 1f,
                    RadiusBonus = choice != null ? choice.RadiusBonus : 0f,
                    BeamWidthBonus = choice != null ? choice.BeamWidthBonus : 0f,
                    HasKnockbackDistanceMultiplier = choice != null && choice.HasKnockbackDistanceMultiplier,
                    KnockbackDistanceMultiplier = choice != null && choice.HasKnockbackDistanceMultiplier ? choice.KnockbackDistanceMultiplier : 1f,
                    HasExecuteHealthRatioBonus = choice != null && choice.HasExecuteHealthRatioBonus,
                    ExecuteHealthRatioBonus = choice != null ? choice.ExecuteHealthRatioBonus : 0f,
                    HasDurationMultiplier = choice != null && choice.HasDurationMultiplier,
                    DurationMultiplier = choice != null && choice.HasDurationMultiplier ? choice.DurationMultiplier : 1f,
                    DurationBonus = choice != null ? choice.DurationBonus : 0f,
                    HasMagazineBonus = choice != null && choice.HasMagazineBonus,
                    MagazineBonus = choice != null ? choice.MagazineBonus : 0,
                    AdditionalProjectileBonus = choice != null ? choice.AdditionalProjectileBonus : 0,
                    PierceBonus = choice != null ? choice.PierceBonus : 0,
                    HasReloadTimeMultiplier = choice != null && choice.HasReloadTimeMultiplier,
                    ReloadTimeMultiplier = choice != null && choice.HasReloadTimeMultiplier ? choice.ReloadTimeMultiplier : 1f,
                    HasShotIntervalMultiplier = choice != null && choice.HasShotIntervalMultiplier,
                    ShotIntervalMultiplier = choice != null && choice.HasShotIntervalMultiplier ? choice.ShotIntervalMultiplier : 1f,
                    HasBurstDamageProjectileIndex = choice != null && choice.HasBurstDamageProjectileIndex,
                    BurstDamageProjectileIndex = choice != null ? choice.BurstDamageProjectileIndex : 0,
                    HasBurstDamageMultiplier = choice != null && choice.HasBurstDamageMultiplier,
                    BurstDamageMultiplier = choice != null && choice.HasBurstDamageMultiplier ? choice.BurstDamageMultiplier : 1f,
                    HasBurstStatusProjectileIndex = choice != null && choice.HasBurstStatusProjectileIndex,
                    BurstStatusProjectileIndex = choice != null ? choice.BurstStatusProjectileIndex : 0,
                    BurstStatusStacksBonus = choice != null ? choice.BurstStatusStacksBonus : 0,
                    FollowUpProjectileCount = choice != null ? choice.FollowUpProjectileCount : 0,
                    FollowUpProjectileDelaySeconds = choice != null ? choice.FollowUpProjectileDelaySeconds : 0f,
                    FollowUpProjectileDamageMultiplier = choice != null && choice.FollowUpProjectileDamageMultiplier > 0f ? choice.FollowUpProjectileDamageMultiplier : 1f,
                    HasStatusChanceBonus = choice != null && choice.HasStatusChanceBonus,
                    StatusChanceBonus = choice != null ? choice.StatusChanceBonus : 0f,
                    BranchChanceBonus = choice != null ? choice.BranchChanceBonus : 0f,
                    HasBranchChanceSet = choice != null && choice.HasBranchChanceSet,
                    BranchChanceSet = choice != null ? choice.BranchChanceSet : 0f,
                    HasBranchCount = choice != null && choice.HasBranchCount,
                    BranchCount = choice != null ? choice.BranchCount : 0,
                    HasBranchDamageMultiplier = choice != null && choice.HasBranchDamageMultiplier,
                    BranchDamageMultiplier = choice != null && choice.HasBranchDamageMultiplier ? choice.BranchDamageMultiplier : 1f,
                    HasBranchSearchRadius = choice != null && choice.HasBranchSearchRadius,
                    BranchSearchRadius = choice != null ? choice.BranchSearchRadius : 0f,
                    BranchLaunchPeriod = choice != null ? choice.BranchLaunchPeriod : 0,
                    HasBranchLaunchChanceSet = choice != null && choice.HasBranchLaunchChanceSet,
                    BranchLaunchChanceSet = choice != null ? choice.BranchLaunchChanceSet : 0f,
                    HasMaxHealthBonus = choice != null && choice.HasMaxHealthBonus,
                    MaxHealthBonus = choice != null ? choice.MaxHealthBonus : 0f,
                    HitTargetCountBonus = choice != null ? choice.HitTargetCountBonus : 0,
                    CritChanceBonus = choice != null ? choice.CritChanceBonus : 0f,
                    CritDamageBonus = choice != null ? choice.CritDamageBonus : 0f,
                    ExecuteCritChanceBonus = choice != null ? choice.ExecuteCritChanceBonus : 0f,
                    HasBossDamageMultiplier = choice != null && choice.HasBossDamageMultiplier,
                    BossDamageMultiplier = choice != null && choice.HasBossDamageMultiplier ? choice.BossDamageMultiplier : 1f,
                    HasKillCooldownRefundRatioBonus = choice != null && choice.HasKillCooldownRefundRatioBonus,
                    KillCooldownRefundRatioBonus = choice != null ? choice.KillCooldownRefundRatioBonus : 0f,
                    KillResetsCooldown = choice != null && choice.KillResetsCooldown,
                    KillResetsCooldownRequiresExecute = choice != null && choice.KillResetsCooldownRequiresExecute,
                    StatusTag = choice != null ? choice.StatusTag : string.Empty,
                    HasStatusActionSpeedBonus = choice != null && choice.HasStatusActionSpeedBonus,
                    StatusActionSpeedBonus = choice != null ? choice.StatusActionSpeedBonus : 0f,
                    HasStatusAttackPowerBonus = choice != null && choice.HasStatusAttackPowerBonus,
                    StatusAttackPowerBonus = choice != null ? choice.StatusAttackPowerBonus : 0f,
                    StatusStacksBonus = choice != null ? choice.StatusStacksBonus : 0,
                    HasStatusStacksSet = choice != null && choice.HasStatusStacksSet,
                    StatusStacksSet = choice != null ? choice.StatusStacksSet : 0,
                    HasStatusElementDamageTakenBonus = choice != null && choice.HasStatusElementDamageTakenBonus,
                    StatusElementDamageTakenBonus = choice != null ? choice.StatusElementDamageTakenBonus : 0f,
                    HasStatusCriticalDamageTakenBonus = choice != null && choice.HasStatusCriticalDamageTakenBonus,
                    StatusCriticalDamageTakenBonus = choice != null ? choice.StatusCriticalDamageTakenBonus : 0f,
                    HasStatusAilmentResistanceBonus = choice != null && choice.HasStatusAilmentResistanceBonus,
                    StatusAilmentResistanceBonus = choice != null ? choice.StatusAilmentResistanceBonus : 0f,
                    StatusMaxStacksBonusStatusId = choice != null ? choice.StatusMaxStacksBonusStatusId : string.Empty,
                    StatusMaxStacksBonus = choice != null ? choice.StatusMaxStacksBonus : 0,
                    StatusDurationBonusStatusId = choice != null ? choice.StatusDurationBonusStatusId : string.Empty,
                    StatusDurationBonus = choice != null ? choice.StatusDurationBonus : 0f,
                    ThresholdStatusId = choice != null ? choice.ThresholdStatusId : string.Empty,
                    ThresholdStatusMinStacks = choice != null ? choice.ThresholdStatusMinStacks : 0,
                    ThresholdApplyStatusId = choice != null ? choice.ThresholdApplyStatusId : string.Empty,
                    HasTargetStatusStackDamageMultiplier = choice != null && choice.HasTargetStatusStackDamageMultiplier,
                    TargetStatusStackDamageMultiplier = choice != null && choice.HasTargetStatusStackDamageMultiplier ? choice.TargetStatusStackDamageMultiplier : 1f,
                    HasConsumeTargetStatusRatioOverride = choice != null && choice.HasConsumeTargetStatusRatioOverride,
                    ConsumeTargetStatusRatioOverride = choice != null ? choice.ConsumeTargetStatusRatioOverride : 0f,
                    HasConsumeTargetStatusStacksOverride = choice != null && choice.HasConsumeTargetStatusStacksOverride,
                    ConsumeTargetStatusStacksOverride = choice != null ? choice.ConsumeTargetStatusStacksOverride : 0,
                    ConditionalCritChanceBonus = choice != null ? choice.ConditionalCritChanceBonus : 0f,
                    ConditionalCritTargetStatusId = choice != null ? choice.ConditionalCritTargetStatusId : string.Empty,
                    ConditionalCritTargetStatusMinStacks = choice != null ? choice.ConditionalCritTargetStatusMinStacks : 0,
                    RedistributeConsumedStatusRatioOnKill = choice != null ? choice.RedistributeConsumedStatusRatioOnKill : 0f,
                    RedistributeConsumedStatusId = choice != null ? choice.RedistributeConsumedStatusId : string.Empty,
                    RedistributeConsumedStatusSearchRadius = choice != null ? choice.RedistributeConsumedStatusSearchRadius : 0f,
                    RedistributeConsumedStatusTargetCount = choice != null ? choice.RedistributeConsumedStatusTargetCount : 0,
                    CountStatusId = choice != null ? choice.CountStatusId : string.Empty,
                    CountTargetSide = choice != null ? choice.CountTargetSide : SkillMultiEffectTargetSide.Enemy,
                    DamageMultiplierPerCount = choice != null ? choice.DamageMultiplierPerCount : 0f,
                    CountMax = choice != null ? choice.CountMax : 0,
                    ConsecutiveHitBonusRate = choice != null ? choice.ConsecutiveHitBonusRate : 0f,
                    ConsecutiveHitMax = choice != null ? choice.ConsecutiveHitMax : 0f,
                    HasStatusConditionalDamageTakenBonus = choice != null && choice.HasStatusConditionalDamageTakenBonus,
                    StatusConditionalDamageTakenBonus = choice != null ? choice.StatusConditionalDamageTakenBonus : 0f,
                    StatusConditionalSourceStatusId = choice != null ? choice.StatusConditionalSourceStatusId : string.Empty,
                    HasConditionalDamageMultiplier = choice != null && choice.HasConditionalDamageMultiplier,
                    ConditionalDamageMultiplier = choice != null && choice.HasConditionalDamageMultiplier ? choice.ConditionalDamageMultiplier : 1f,
                    ConditionalTargetStatusId = choice != null ? choice.ConditionalTargetStatusId : string.Empty,
                    ConditionalTargetStatusMinStacks = choice != null ? choice.ConditionalTargetStatusMinStacks : 0,
                    HasOnHitAdditionalDamage = choice != null && choice.HasOnHitAdditionalDamage,
                    OnHitAdditionalDamageChance = choice != null ? choice.OnHitAdditionalDamageChance : 0f,
                    OnHitAdditionalDamageMultiplier = choice != null ? choice.OnHitAdditionalDamageMultiplier : 1f,
                    OnHitAdditionalDamageAttribute = choice != null ? choice.OnHitAdditionalDamageAttribute : DamageAttribute.Physical,
                    OnHitAdditionalDamageTarget = choice != null ? choice.OnHitAdditionalDamageTarget : string.Empty,
                    OnHitChainHitPeriod = choice != null ? choice.OnHitChainHitPeriod : 0,
                    OnHitChainTargetCount = choice != null ? choice.OnHitChainTargetCount : 0,
                    OnHitChainSearchRadius = choice != null ? choice.OnHitChainSearchRadius : 0f,
                    OnHitChainDamageMultiplier = choice != null ? choice.OnHitChainDamageMultiplier : 1f,
                    OnHitChainDamageAttribute = choice != null ? choice.OnHitChainDamageAttribute : DamageAttribute.Physical,
                    ReloadReduceTargetSkillId = choice != null ? choice.ReloadReduceTargetSkillId : string.Empty,
                    ReloadReduceSecondsPerHit = choice != null ? choice.ReloadReduceSecondsPerHit : 0f,
                    CoreHitboxName = choice != null ? choice.CoreHitboxName : string.Empty,
                    HasCoreDamageMultiplier = choice != null && choice.HasCoreDamageMultiplier,
                    CoreDamageMultiplier = choice != null && choice.HasCoreDamageMultiplier ? choice.CoreDamageMultiplier : 1f,
                    HasCoreOnHitAdditionalDamage = choice != null && choice.HasCoreOnHitAdditionalDamage,
                    CoreOnHitAdditionalDamageChance = choice != null ? choice.CoreOnHitAdditionalDamageChance : 0f,
                    CoreOnHitAdditionalDamageMultiplier = choice != null ? choice.CoreOnHitAdditionalDamageMultiplier : 1f,
                    CoreOnHitAdditionalDamageAttribute = choice != null ? choice.CoreOnHitAdditionalDamageAttribute : DamageAttribute.Physical,
                    HitCountCooldownRefundTargetSkillId = choice != null ? choice.HitCountCooldownRefundTargetSkillId : string.Empty,
                    HitCountCooldownRefundMinTargets = choice != null ? choice.HitCountCooldownRefundMinTargets : 0,
                    HitCountCooldownRefundRatio = choice != null ? choice.HitCountCooldownRefundRatio : 0f,
                    RequiredSourceStatusId = choice != null ? choice.RequiredSourceStatusId : string.Empty,
                    RequiredSourceStatusMinStacks = choice != null ? choice.RequiredSourceStatusMinStacks : 0,
                    RepeatCountPerTarget = choice != null ? choice.RepeatCountPerTarget : 0,
                    RepeatIntervalSeconds = choice != null ? choice.RepeatIntervalSeconds : 0f,
                    RepeatDamageMultiplier = choice != null && choice.RepeatDamageMultiplier > 0f ? choice.RepeatDamageMultiplier : 1f,
                    NormalizedPlanNodes = choice != null ? MapSkillNodeDefinitions(choice.NormalizedPlanNodes) : Array.Empty<SkillExecutionPlanNode>()
                };

                if (choice != null && !HasNormalizedPlanNodes(choice))
                {
                    ApplyNormalizedChoiceNodes(spec, choice.NormalizedPlanNodes);
                }

                mapped[i] = spec;
            }

            return mapped;
        }

        private static bool HasNormalizedPlanNodes(SkillChoiceDefinition choice)
        {
            return choice != null
                && choice.NormalizedPlanNodes != null
                && choice.NormalizedPlanNodes.Length > 0;
        }

        internal static void ApplyNormalizedChoiceNodes(SkillChoiceEffectSpec spec, SkillNodeDefinition[] nodes)
        {
            if (spec == null || nodes == null || nodes.Length == 0)
            {
                return;
            }

            for (var i = 0; i < nodes.Length; i++)
            {
                var node = nodes[i];
                if (node == null || !node.EnabledByDefault)
                {
                    continue;
                }

                ApplyNormalizedChoiceNode(spec, node);
            }
        }

        private static void ApplyNormalizedChoiceNode(SkillChoiceEffectSpec spec, SkillNodeDefinition node)
        {
            var handlerId = node.HandlerId ?? string.Empty;
            if (string.Equals(handlerId, "DamageMultiplier", StringComparison.OrdinalIgnoreCase))
            {
                spec.HasDamageMultiplier = true;
                spec.DamageMultiplier *= GetFloatParam(node, "multiplier", 1f);
                return;
            }

            if (string.Equals(handlerId, "ShieldAmountMultiplier", StringComparison.OrdinalIgnoreCase))
            {
                spec.HasShieldAmountMultiplier = true;
                spec.ShieldAmountMultiplier *= GetFloatParam(node, "multiplier", 1f);
                return;
            }

            if (string.Equals(handlerId, "CountStatusDamageMultiplier", StringComparison.OrdinalIgnoreCase))
            {
                spec.CountStatusId = GetParam(node, "status_id");
                spec.CountTargetSide = GetEnumParam(node, "target_side", SkillMultiEffectTargetSide.AllAllies);
                spec.DamageMultiplierPerCount += GetFloatParam(node, "amount_per_count", 0f);
                spec.CountMax = GetIntParam(node, "max_count", spec.CountMax);
                return;
            }

            if (string.Equals(handlerId, "CooldownMultiplier", StringComparison.OrdinalIgnoreCase))
            {
                spec.HasCooldownMultiplier = true;
                spec.CooldownMultiplier *= GetFloatParam(node, "multiplier", 1f);
                return;
            }

            if (string.Equals(handlerId, "MagazineBonus", StringComparison.OrdinalIgnoreCase))
            {
                spec.HasMagazineBonus = true;
                spec.MagazineBonus += GetIntParam(node, "bonus", 0);
                return;
            }

            if (string.Equals(handlerId, "ReloadTimeMultiplier", StringComparison.OrdinalIgnoreCase))
            {
                spec.HasReloadTimeMultiplier = true;
                spec.ReloadTimeMultiplier *= GetFloatParam(node, "multiplier", 1f);
                return;
            }

            if (string.Equals(handlerId, "PierceBonus", StringComparison.OrdinalIgnoreCase))
            {
                spec.PierceBonus += GetIntParam(node, "bonus", 0);
                return;
            }

            if (string.Equals(handlerId, "HitTargetCountBonus", StringComparison.OrdinalIgnoreCase))
            {
                spec.HitTargetCountBonus += GetIntParam(node, "bonus", 0);
                return;
            }

            if (string.Equals(handlerId, "RadiusMultiplier", StringComparison.OrdinalIgnoreCase))
            {
                spec.HasRadiusMultiplier = true;
                spec.RadiusMultiplier *= GetFloatParam(node, "multiplier", 1f);
                return;
            }

            if (string.Equals(handlerId, "RadiusBonus", StringComparison.OrdinalIgnoreCase))
            {
                spec.RadiusBonus += GetFloatParam(node, "bonus", 0f);
                return;
            }

            if (string.Equals(handlerId, "DurationBonus", StringComparison.OrdinalIgnoreCase))
            {
                spec.DurationBonus += GetFloatParam(node, "bonus_seconds", 0f);
                return;
            }

            if (string.Equals(handlerId, "StatusActionSpeedBonus", StringComparison.OrdinalIgnoreCase))
            {
                spec.HasStatusActionSpeedBonus = true;
                spec.StatusActionSpeedBonusStatusId = GetParam(node, "status_id");
                spec.StatusActionSpeedBonus += GetFloatParam(node, "bonus", 0f);
                return;
            }

            if (string.Equals(handlerId, "StatusAttackPowerBonus", StringComparison.OrdinalIgnoreCase))
            {
                spec.HasStatusAttackPowerBonus = true;
                spec.StatusAttackPowerBonus += GetFloatParam(node, "bonus", 0f);
                return;
            }

            if (string.Equals(handlerId, "StatusAilmentResistanceBonus", StringComparison.OrdinalIgnoreCase))
            {
                spec.HasStatusAilmentResistanceBonus = true;
                spec.StatusAilmentResistanceBonus += GetFloatParam(node, "bonus", 0f);
                return;
            }

            if (string.Equals(handlerId, "StatusDamageBonusRate", StringComparison.OrdinalIgnoreCase))
            {
                spec.HasStatusDamageBonusRate = true;
                spec.StatusDamageBonusRate += GetFloatParam(node, "bonus", 0f);
                return;
            }

            if (string.Equals(handlerId, "StatusShieldReceivedBonus", StringComparison.OrdinalIgnoreCase))
            {
                spec.HasStatusShieldReceivedBonus = true;
                spec.StatusShieldReceivedBonus += GetFloatParam(node, "bonus", 0f);
                return;
            }

            if (string.Equals(handlerId, "StatusCriticalChanceBonus", StringComparison.OrdinalIgnoreCase))
            {
                spec.HasStatusCriticalChanceBonus = true;
                spec.StatusCriticalChanceBonus += GetFloatParam(node, "bonus", 0f);
                return;
            }

            if (string.Equals(handlerId, "StatusDamageTakenBonus", StringComparison.OrdinalIgnoreCase))
            {
                spec.HasStatusDamageTakenBonus = true;
                spec.StatusDamageTakenBonus += GetFloatParam(node, "bonus", 0f);
                return;
            }

            if (string.Equals(handlerId, "StatusFlatElementResistReduction", StringComparison.OrdinalIgnoreCase))
            {
                spec.HasStatusFlatElementResistReduction = true;
                spec.StatusFlatElementResistReduction += GetFloatParam(node, "bonus", 0f);
                return;
            }

            if (string.Equals(handlerId, "StatusDurationBonus", StringComparison.OrdinalIgnoreCase))
            {
                spec.StatusDurationBonusStatusId = GetParam(node, "status_id");
                spec.StatusDurationBonus += GetFloatParam(node, "bonus_seconds", 0f);
                return;
            }

            if (string.Equals(handlerId, "StatusConditionalDamageTakenBonus", StringComparison.OrdinalIgnoreCase))
            {
                spec.HasStatusConditionalDamageTakenBonus = true;
                spec.StatusConditionalSourceStatusId = GetParam(node, "source_status_id");
                spec.StatusConditionalDamageTakenBonus += GetFloatParam(node, "bonus", 0f);
                return;
            }

            if (string.Equals(handlerId, "StatusElementDamageTakenBonus", StringComparison.OrdinalIgnoreCase))
            {
                spec.HasStatusElementDamageTakenBonus = true;
                spec.StatusElementDamageTakenBonus += GetFloatParam(node, "bonus", 0f);
                return;
            }

            if (string.Equals(handlerId, "StatusCriticalDamageTakenBonus", StringComparison.OrdinalIgnoreCase))
            {
                spec.HasStatusCriticalDamageTakenBonus = true;
                spec.StatusCriticalDamageTakenBonus += GetFloatParam(node, "bonus", 0f);
                return;
            }

            if (string.Equals(handlerId, "AdditionalDamage", StringComparison.OrdinalIgnoreCase))
            {
                spec.HasOnHitAdditionalDamage = true;
                spec.OnHitAdditionalDamageChance = GetFloatParam(node, "chance", 1f);
                spec.OnHitAdditionalDamageMultiplier = GetFloatParam(node, "multiplier", 1f);
                spec.OnHitAdditionalDamageAttribute = GetEnumParam(node, "attribute", DamageAttribute.Physical);
                var target = GetParam(node, "target");
                spec.OnHitAdditionalDamageTarget = string.IsNullOrWhiteSpace(target)
                    ? GetParam(node, "target_side")
                    : target;
                return;
            }

            if (string.Equals(handlerId, "EveryNthHitChainDamage", StringComparison.OrdinalIgnoreCase))
            {
                spec.OnHitChainHitPeriod = GetIntParam(node, "hit_count", 0);
                spec.OnHitChainTargetCount = GetIntParam(node, "max_targets", spec.OnHitChainTargetCount);
                spec.OnHitChainSearchRadius = GetFloatParam(node, "radius", spec.OnHitChainSearchRadius);
                spec.OnHitChainDamageMultiplier = GetFloatParam(node, "multiplier", 1f);
                spec.OnHitChainDamageAttribute = GetEnumParam(node, "attribute", DamageAttribute.Physical);
                return;
            }

            if (string.Equals(handlerId, "RepeatPerTarget", StringComparison.OrdinalIgnoreCase))
            {
                spec.RepeatCountPerTarget = GetIntParam(node, "repeat_count", 0);
                spec.RepeatIntervalSeconds = GetFloatParam(node, "repeat_interval_seconds", 0f);
                spec.RepeatDamageMultiplier = GetFloatParam(node, "repeat_damage_multiplier", 1f);
                return;
            }

            if (string.Equals(handlerId, "TargetStatusCritBonus", StringComparison.OrdinalIgnoreCase))
            {
                spec.ConditionalCritChanceBonus += GetFloatParam(node, "crit_chance_bonus", 0f);
                spec.ConditionalCritTargetStatusId = GetParam(node, "status_id");
                spec.ConditionalCritTargetStatusMinStacks = GetIntParam(node, "min_stacks", 0);
                return;
            }

            if (string.Equals(handlerId, "RedistributeConsumedStatus", StringComparison.OrdinalIgnoreCase))
            {
                spec.RedistributeConsumedStatusRatioOnKill = GetFloatParam(node, "ratio", 0f);
                spec.RedistributeConsumedStatusId = GetParam(node, "status_id");
                spec.RedistributeConsumedStatusSearchRadius = GetFloatParam(node, "radius", 0f);
                spec.RedistributeConsumedStatusTargetCount = GetIntParam(node, "target_count", 0);
            }
        }

        public static SkillExecutionPlanNode[] MapSkillNodeDefinitions(SkillNodeDefinition[] source)
        {
            if (source == null || source.Length == 0)
            {
                return Array.Empty<SkillExecutionPlanNode>();
            }

            var mapped = new List<SkillExecutionPlanNode>(source.Length);
            for (var i = 0; i < source.Length; i++)
            {
                var node = MapSkillNodeDefinition(source[i]);
                if (node != null)
                {
                    mapped.Add(node);
                }
            }

            return mapped.Count == 0 ? Array.Empty<SkillExecutionPlanNode>() : mapped.ToArray();
        }

        private static SkillExecutionPlanNode MapSkillNodeDefinition(SkillNodeDefinition node)
        {
            if (node == null || !node.EnabledByDefault)
            {
                return null;
            }

            var handlerId = node.HandlerId ?? string.Empty;
            var rowId = node.NodeId;
            if (string.Equals(handlerId, "TargetHealthRatioCondition", StringComparison.OrdinalIgnoreCase))
            {
                return SkillExecutionPlanNode.FromCastCondition(
                    SkillExecutionPlanAuthoringSource.NormalizedRow,
                    rowId,
                    new CastConditionOp(
                        CastConditionOpKind.TargetHealthRatioBonus,
                        GetFloatParam(node, "threshold", 0f)));
            }

            if (string.Equals(handlerId, "TargetHealthRatioThresholdBonus", StringComparison.OrdinalIgnoreCase))
            {
                return SkillExecutionPlanNode.FromCastCondition(
                    SkillExecutionPlanAuthoringSource.NormalizedRow,
                    rowId,
                    new CastConditionOp(
                        CastConditionOpKind.TargetHealthRatioBonus,
                        GetFloatParam(node, "threshold_bonus", 0f)));
            }

            if (string.Equals(handlerId, "ExecuteDamageMultiplier", StringComparison.OrdinalIgnoreCase))
            {
                return SkillExecutionPlanNode.FromDamageModifier(
                    SkillExecutionPlanAuthoringSource.NormalizedRow,
                    rowId,
                    new DamageModifierOp(
                        DamageModifierOpKind.ExecuteMultiplier,
                        GetFloatParam(node, "multiplier", 1f)));
            }

            if (string.Equals(handlerId, "TargetPredicateDamageMultiplier", StringComparison.OrdinalIgnoreCase)
                && string.Equals(GetParam(node, "predicate"), "is_boss", StringComparison.OrdinalIgnoreCase))
            {
                return SkillExecutionPlanNode.FromDamageModifier(
                    SkillExecutionPlanAuthoringSource.NormalizedRow,
                    rowId,
                    new DamageModifierOp(
                        DamageModifierOpKind.BossMultiplier,
                        GetFloatParam(node, "multiplier", 1f)));
            }

            if (string.Equals(handlerId, "BossDamageMultiplier", StringComparison.OrdinalIgnoreCase))
            {
                return SkillExecutionPlanNode.FromDamageModifier(
                    SkillExecutionPlanAuthoringSource.NormalizedRow,
                    rowId,
                    new DamageModifierOp(
                        DamageModifierOpKind.BossMultiplier,
                        GetFloatParam(node, "multiplier", 1f)));
            }

            if (string.Equals(handlerId, "ExecuteCritChanceBonus", StringComparison.OrdinalIgnoreCase))
            {
                return SkillExecutionPlanNode.FromCritModifier(
                    SkillExecutionPlanAuthoringSource.NormalizedRow,
                    rowId,
                    new CritModifierOp(
                        CritModifierOpKind.ExecuteChanceBonus,
                        GetFloatParam(node, "crit_chance_bonus", 0f)));
            }

            if (string.Equals(handlerId, "CooldownReset", StringComparison.OrdinalIgnoreCase)
                || string.Equals(handlerId, "CooldownResetOnKill", StringComparison.OrdinalIgnoreCase))
            {
                return SkillExecutionPlanNode.FromKillAction(
                    SkillExecutionPlanAuthoringSource.NormalizedRow,
                    rowId,
                    new KillActionOp(
                        KillActionOpKind.CooldownReset,
                        0f,
                        GetBoolParam(node, "requires_execute", false)));
            }

            if (string.Equals(handlerId, "CooldownRefund", StringComparison.OrdinalIgnoreCase))
            {
                return SkillExecutionPlanNode.FromKillAction(
                    SkillExecutionPlanAuthoringSource.NormalizedRow,
                    rowId,
                    new KillActionOp(
                        KillActionOpKind.CooldownRefundBonus,
                        GetFloatParam(node, "ratio", 0f),
                        false));
            }

            if (string.Equals(handlerId, "CooldownRefundBonus", StringComparison.OrdinalIgnoreCase))
            {
                return SkillExecutionPlanNode.FromKillAction(
                    SkillExecutionPlanAuthoringSource.NormalizedRow,
                    rowId,
                    new KillActionOp(
                        KillActionOpKind.CooldownRefundBonus,
                        GetFloatParam(node, "ratio_bonus", 0f),
                        false));
            }

            var action = MapSkillActionOp(node, handlerId);
            if (action.HasValue)
            {
                return SkillExecutionPlanNode.FromAction(
                    SkillExecutionPlanAuthoringSource.NormalizedRow,
                    rowId,
                    action.Value);
            }

            return new SkillExecutionPlanNode(
                MapNodeKind(node.NodeKind),
                SkillExecutionPlanAuthoringSource.NormalizedRow,
                rowId);
        }

        private static SkillActionOp? MapSkillActionOp(SkillNodeDefinition node, string handlerId)
        {
            if (string.Equals(handlerId, "DamageMultiplier", StringComparison.OrdinalIgnoreCase))
            {
                return new SkillActionOp(SkillActionOpKind.DamageMultiplier, GetFloatParam(node, "multiplier", 1f));
            }

            if (string.Equals(handlerId, "ShieldAmountMultiplier", StringComparison.OrdinalIgnoreCase))
            {
                return new SkillActionOp(SkillActionOpKind.ShieldAmountMultiplier, GetFloatParam(node, "multiplier", 1f));
            }

            if (string.Equals(handlerId, "CountStatusDamageMultiplier", StringComparison.OrdinalIgnoreCase))
            {
                return new SkillActionOp(
                    SkillActionOpKind.CountStatusDamageMultiplier,
                    GetFloatParam(node, "amount_per_count", 0f),
                    GetIntParam(node, "max_count", 0),
                    GetParam(node, "status_id"),
                    null,
                    GetEnumParam(node, "target_side", SkillMultiEffectTargetSide.AllAllies));
            }

            if (string.Equals(handlerId, "CooldownMultiplier", StringComparison.OrdinalIgnoreCase))
            {
                return new SkillActionOp(SkillActionOpKind.CooldownMultiplier, GetFloatParam(node, "multiplier", 1f));
            }

            if (string.Equals(handlerId, "MagazineBonus", StringComparison.OrdinalIgnoreCase))
            {
                return new SkillActionOp(SkillActionOpKind.MagazineBonus, intValue: GetIntParam(node, "bonus", 0));
            }

            if (string.Equals(handlerId, "ReloadTimeMultiplier", StringComparison.OrdinalIgnoreCase))
            {
                return new SkillActionOp(SkillActionOpKind.ReloadTimeMultiplier, GetFloatParam(node, "multiplier", 1f));
            }

            if (string.Equals(handlerId, "PierceBonus", StringComparison.OrdinalIgnoreCase))
            {
                return new SkillActionOp(SkillActionOpKind.PierceBonus, intValue: GetIntParam(node, "bonus", 0));
            }

            if (string.Equals(handlerId, "RadiusMultiplier", StringComparison.OrdinalIgnoreCase))
            {
                return new SkillActionOp(SkillActionOpKind.RadiusMultiplier, GetFloatParam(node, "multiplier", 1f));
            }

            if (string.Equals(handlerId, "RadiusBonus", StringComparison.OrdinalIgnoreCase))
            {
                return new SkillActionOp(SkillActionOpKind.RadiusBonus, GetFloatParam(node, "bonus", 0f));
            }

            if (string.Equals(handlerId, "DurationBonus", StringComparison.OrdinalIgnoreCase))
            {
                return new SkillActionOp(SkillActionOpKind.DurationBonus, GetFloatParam(node, "bonus_seconds", 0f));
            }

            if (string.Equals(handlerId, "HitTargetCountBonus", StringComparison.OrdinalIgnoreCase))
            {
                return new SkillActionOp(SkillActionOpKind.HitTargetCountBonus, intValue: GetIntParam(node, "bonus", 0));
            }

            if (string.Equals(handlerId, "StatusActionSpeedBonus", StringComparison.OrdinalIgnoreCase))
            {
                return new SkillActionOp(
                    SkillActionOpKind.StatusActionSpeedBonus,
                    GetFloatParam(node, "bonus", 0f),
                    stringValue: GetParam(node, "status_id"));
            }

            if (string.Equals(handlerId, "StatusAttackPowerBonus", StringComparison.OrdinalIgnoreCase))
            {
                return new SkillActionOp(SkillActionOpKind.StatusAttackPowerBonus, GetFloatParam(node, "bonus", 0f));
            }

            if (string.Equals(handlerId, "StatusAilmentResistanceBonus", StringComparison.OrdinalIgnoreCase))
            {
                return new SkillActionOp(SkillActionOpKind.StatusAilmentResistanceBonus, GetFloatParam(node, "bonus", 0f));
            }

            if (string.Equals(handlerId, "StatusDamageBonusRate", StringComparison.OrdinalIgnoreCase))
            {
                return new SkillActionOp(SkillActionOpKind.StatusDamageBonusRate, GetFloatParam(node, "bonus", 0f));
            }

            if (string.Equals(handlerId, "StatusShieldReceivedBonus", StringComparison.OrdinalIgnoreCase))
            {
                return new SkillActionOp(SkillActionOpKind.StatusShieldReceivedBonus, GetFloatParam(node, "bonus", 0f));
            }

            if (string.Equals(handlerId, "StatusCriticalChanceBonus", StringComparison.OrdinalIgnoreCase))
            {
                return new SkillActionOp(SkillActionOpKind.StatusCriticalChanceBonus, GetFloatParam(node, "bonus", 0f));
            }

            if (string.Equals(handlerId, "StatusDamageTakenBonus", StringComparison.OrdinalIgnoreCase))
            {
                return new SkillActionOp(SkillActionOpKind.StatusDamageTakenBonus, GetFloatParam(node, "bonus", 0f));
            }

            if (string.Equals(handlerId, "StatusFlatElementResistReduction", StringComparison.OrdinalIgnoreCase))
            {
                return new SkillActionOp(SkillActionOpKind.StatusFlatElementResistReduction, GetFloatParam(node, "bonus", 0f));
            }

            if (string.Equals(handlerId, "StatusDurationBonus", StringComparison.OrdinalIgnoreCase))
            {
                return new SkillActionOp(
                    SkillActionOpKind.StatusDurationBonus,
                    GetFloatParam(node, "bonus_seconds", 0f),
                    stringValue: GetParam(node, "status_id"));
            }

            if (string.Equals(handlerId, "StatusConditionalDamageTakenBonus", StringComparison.OrdinalIgnoreCase))
            {
                return new SkillActionOp(
                    SkillActionOpKind.StatusConditionalDamageTakenBonus,
                    GetFloatParam(node, "bonus", 0f),
                    stringValue: GetParam(node, "source_status_id"));
            }

            if (string.Equals(handlerId, "StatusElementDamageTakenBonus", StringComparison.OrdinalIgnoreCase))
            {
                return new SkillActionOp(SkillActionOpKind.StatusElementDamageTakenBonus, GetFloatParam(node, "bonus", 0f));
            }

            if (string.Equals(handlerId, "StatusCriticalDamageTakenBonus", StringComparison.OrdinalIgnoreCase))
            {
                return new SkillActionOp(SkillActionOpKind.StatusCriticalDamageTakenBonus, GetFloatParam(node, "bonus", 0f));
            }

            return null;
        }

        private static SkillExecutionPlanNodeKind MapNodeKind(string value)
        {
            return !string.IsNullOrWhiteSpace(value)
                && Enum.TryParse(value, true, out SkillExecutionPlanNodeKind kind)
                    ? kind
                    : SkillExecutionPlanNodeKind.Action;
        }

        private static string GetParam(SkillNodeDefinition node, string key)
        {
            if (node == null || node.Params == null || string.IsNullOrWhiteSpace(key))
            {
                return string.Empty;
            }

            for (var i = 0; i < node.Params.Length; i++)
            {
                var param = node.Params[i];
                if (param != null && string.Equals(param.ParamKey, key, StringComparison.OrdinalIgnoreCase))
                {
                    return param.Value ?? string.Empty;
                }
            }

            return string.Empty;
        }

        private static float GetFloatParam(SkillNodeDefinition node, string key, float fallback)
        {
            var raw = GetParam(node, key);
            return !string.IsNullOrWhiteSpace(raw)
                && float.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out var value)
                    ? value
                    : fallback;
        }

        private static int GetIntParam(SkillNodeDefinition node, string key, int fallback)
        {
            var raw = GetParam(node, key);
            return !string.IsNullOrWhiteSpace(raw)
                && int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value)
                    ? value
                    : fallback;
        }

        private static bool GetBoolParam(SkillNodeDefinition node, string key, bool fallback)
        {
            var raw = GetParam(node, key);
            if (string.IsNullOrWhiteSpace(raw))
            {
                return fallback;
            }

            if (bool.TryParse(raw, out var value))
            {
                return value;
            }

            return string.Equals(raw, "1", StringComparison.OrdinalIgnoreCase)
                || string.Equals(raw, "yes", StringComparison.OrdinalIgnoreCase)
                || string.Equals(raw, "y", StringComparison.OrdinalIgnoreCase);
        }

        private static T GetEnumParam<T>(SkillNodeDefinition node, string key, T fallback)
            where T : struct
        {
            var raw = GetParam(node, key);
            return !string.IsNullOrWhiteSpace(raw)
                && Enum.TryParse(raw, true, out T value)
                    ? value
                    : fallback;
        }

        private static CharacterType MapCharacter(string monsterId)
        {
            var id = monsterId;
            switch (id != null ? id.ToLowerInvariant() : string.Empty)
            {
                case "ariel":
                    return CharacterType.Ariel;
                case "rin":
                    return CharacterType.Rin;
                case "sein":
                    return CharacterType.Sein;
                case "vega":
                    return CharacterType.Vega;
                case "eve":
                default:
                    return CharacterType.Eve;
            }
        }

        private static InGameSkillSlot MapSlot(SkillSlot slot)
        {
            return (InGameSkillSlot)(int)slot;
        }

        public static SkillSlot MapSlot(InGameSkillSlot slot)
        {
            return (SkillSlot)(int)slot;
        }

        private static ElementType MapElement(DamageAttribute attribute)
        {
            return (ElementType)(int)attribute;
        }

        private static SkillTargetShape MapShape(SkillRuntimeKind runtimeKind)
        {
            switch (runtimeKind)
            {
                case SkillRuntimeKind.LineAttack:
                    return SkillTargetShape.Line;
                case SkillRuntimeKind.AreaAttack:
                case SkillRuntimeKind.SingleAttack:
                case SkillRuntimeKind.Field:
                case SkillRuntimeKind.Mark:
                case SkillRuntimeKind.Execute:
                    return SkillTargetShape.Circle;
                default:
                    return SkillTargetShape.Single;
            }
        }
    }
}
