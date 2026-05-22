using System;
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
            MapCommonFields(skill, monster != null ? monster.MonsterId : string.Empty, source);
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

        private static void MapCommonFields(SkillData skill, string monsterId, SkillDefinition source)
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

            skill.Timing.Cooldown = source.CooldownSeconds;
            skill.Timing.ActiveDuration = source.ActiveDurationSeconds;
            skill.Timing.TickInterval = source.ShotIntervalSeconds;
            skill.Targeting.Range = 0f;
            skill.Targeting.Radius = source.Radius;
            if (Enum.TryParse<SkillTargetSelection>(source.TargetSelection, true, out var targetSelection))
            {
                skill.Targeting.Selection = targetSelection;
            }

            skill.Targeting.Shape = MapShape(source.RuntimeKind);
            skill.Targeting.CoverAll = source.RuntimeKind == SkillRuntimeKind.Field
                || (source.RuntimeKind == SkillRuntimeKind.SingleAttack
                    && source.Radius <= 0f
                    && string.IsNullOrWhiteSpace(source.TargetSelection));
        }

        private static void MapActiveFields(SkillData skill, MonsterDefinition monster, SkillDefinition source)
        {
            if (skill is ProjectileSkillData projectile)
            {
                projectile.Projectile.MagazineSize = source.MagazineCapacity;
                projectile.Projectile.ReloadTime = source.ReloadSeconds;
                projectile.Projectile.BurstProjectileCount = Math.Max(1, source.ProjectileBurstCount);
                projectile.Projectile.ProjectilesPerShot = 1;
                projectile.Projectile.PierceCount = source.PierceCount;
                projectile.Projectile.ProjectileSpeed = source.ProjectileSpeed;
                MapDamage(projectile.Damage, source);
                projectile.OnHitStatus = CreateStatusApplication(source);
                return;
            }

            if (skill is BeamSkillData beam)
            {
                beam.BeamLength = 0f;
                beam.BeamWidth = source.Radius;
                MapDamage(beam.DamagePerTick, source);
                beam.OnHitStatus = CreateStatusApplication(source);
                return;
            }

            if (skill is ZoneSkillData zone)
            {
                zone.Area.Radius = source.Radius;
                zone.Area.Duration = source.ActiveDurationSeconds > 0f
                    ? source.ActiveDurationSeconds
                    : source.CooldownSeconds;
                zone.Area.TickInterval = source.ShotIntervalSeconds;
                zone.Area.CoverAll = source.RuntimeKind == SkillRuntimeKind.Field;
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
                single.Area.Radius = source.Radius;
                single.Area.Duration = 0f;
                single.Area.TickInterval = 0f;
                single.UsesHitTargetCount = hasHitTargetCount || source.Radius <= 0f;
                single.UsePrefabHitbox = hitAllTargets;
                single.HitAllTargets = hitAllTargets;
                single.HitTargetCount = hitAllTargets ? int.MaxValue : Math.Max(1, hitTargetCount);
                single.Area.CoverAll = hitAllTargets
                    || (!single.UsesHitTargetCount
                        && source.Radius <= 0f
                        && string.IsNullOrWhiteSpace(source.TargetSelection));
                MapDamage(single.Damage, source);
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
                mapped[i] = new SkillChoiceEffectSpec
                {
                    ChoiceId = choice != null ? choice.ChoiceId : string.Empty,
                    Title = choice != null ? choice.Title : string.Empty,
                    Description = choice != null ? choice.DescriptionText : string.Empty,
                    Icon = choice != null ? choice.SkillIcon : null,
                    SkillEffectPrefab = choice != null ? choice.SkillEffectPrefab : null,
                    HasDamageMultiplier = choice != null && choice.HasDamageMultiplier,
                    DamageMultiplier = choice != null && choice.HasDamageMultiplier ? choice.DamageMultiplier : 1f,
                    BaseDamageBonus = choice != null ? choice.BaseDamageBonus : 0f,
                    HasCooldownMultiplier = choice != null && choice.HasCooldownMultiplier,
                    CooldownMultiplier = choice != null && choice.HasCooldownMultiplier ? choice.CooldownMultiplier : 1f,
                    HasRadiusMultiplier = choice != null && choice.HasRadiusMultiplier,
                    RadiusMultiplier = choice != null && choice.HasRadiusMultiplier ? choice.RadiusMultiplier : 1f,
                    RadiusBonus = choice != null ? choice.RadiusBonus : 0f,
                    BeamWidthBonus = choice != null ? choice.BeamWidthBonus : 0f,
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
                    HasMaxHealthBonus = choice != null && choice.HasMaxHealthBonus,
                    MaxHealthBonus = choice != null ? choice.MaxHealthBonus : 0f,
                    HitTargetCountBonus = choice != null ? choice.HitTargetCountBonus : 0,
                    CritChanceBonus = choice != null ? choice.CritChanceBonus : 0f,
                    CritDamageBonus = choice != null ? choice.CritDamageBonus : 0f,
                    StatusTag = choice != null ? choice.StatusTag : string.Empty,
                    StatusStacksBonus = choice != null ? choice.StatusStacksBonus : 0,
                    HasStatusStacksSet = choice != null && choice.HasStatusStacksSet,
                    StatusStacksSet = choice != null ? choice.StatusStacksSet : 0,
                    HasStatusElementDamageTakenBonus = choice != null && choice.HasStatusElementDamageTakenBonus,
                    StatusElementDamageTakenBonus = choice != null ? choice.StatusElementDamageTakenBonus : 0f,
                    HasStatusCriticalDamageTakenBonus = choice != null && choice.HasStatusCriticalDamageTakenBonus,
                    StatusCriticalDamageTakenBonus = choice != null ? choice.StatusCriticalDamageTakenBonus : 0f,
                    HasStatusAilmentResistanceBonus = choice != null && choice.HasStatusAilmentResistanceBonus,
                    StatusAilmentResistanceBonus = choice != null ? choice.StatusAilmentResistanceBonus : 0f,
                    CountStatusId = choice != null ? choice.CountStatusId : string.Empty,
                    CountTargetSide = choice != null ? choice.CountTargetSide : SkillMultiEffectTargetSide.Enemy,
                    DamageMultiplierPerCount = choice != null ? choice.DamageMultiplierPerCount : 0f,
                    CountMax = choice != null ? choice.CountMax : 0,
                    HasStatusConditionalDamageTakenBonus = choice != null && choice.HasStatusConditionalDamageTakenBonus,
                    StatusConditionalDamageTakenBonus = choice != null ? choice.StatusConditionalDamageTakenBonus : 0f,
                    StatusConditionalSourceStatusId = choice != null ? choice.StatusConditionalSourceStatusId : string.Empty
                };
            }

            return mapped;
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
