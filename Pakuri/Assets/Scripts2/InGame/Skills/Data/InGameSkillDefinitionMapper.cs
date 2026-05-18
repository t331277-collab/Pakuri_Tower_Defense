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

            skill.Timing.Cooldown = source.CooldownSeconds;
            skill.Timing.ActiveDuration = source.ActiveDurationSeconds;
            skill.Timing.TickInterval = source.ShotIntervalSeconds;
            skill.Targeting.Range = 0f;
            skill.Targeting.Radius = source.Radius;
            skill.Targeting.Shape = MapShape(source.RuntimeKind);
            skill.Targeting.CoverAll = source.RuntimeKind == SkillRuntimeKind.Field;
        }

        private static void MapActiveFields(SkillData skill, MonsterDefinition monster, SkillDefinition source)
        {
            if (skill is ProjectileSkillData projectile)
            {
                projectile.Projectile.MagazineSize = source.MagazineCapacity;
                projectile.Projectile.ReloadTime = source.ReloadSeconds;
                projectile.Projectile.ProjectilesPerShot = 1;
                projectile.Projectile.PierceCount = source.PierceCount;
                projectile.Projectile.ProjectileSpeed = source.ProjectileSpeed;
                MapDamage(projectile.Damage, source);
                projectile.OnHitStatus = CreateStatusApplication(source.StatusEffectId, source.StatusChance, source.StatusEffectLabel);
                return;
            }

            if (skill is BeamSkillData beam)
            {
                beam.BeamLength = 0f;
                beam.BeamWidth = source.Radius;
                MapDamage(beam.DamagePerTick, source);
                beam.OnHitStatus = CreateStatusApplication(source.StatusEffectId, source.StatusChance, source.StatusEffectLabel);
                return;
            }

            if (skill is ZoneSkillData zone)
            {
                zone.Area.Radius = source.Radius;
                zone.Area.Duration = source.CooldownSeconds;
                zone.Area.TickInterval = source.ShotIntervalSeconds;
                zone.Area.CoverAll = source.RuntimeKind == SkillRuntimeKind.Field;
                MapDamage(zone.DamagePerTick, source);
                zone.OnTickStatus = CreateStatusApplication(source.StatusEffectId, source.StatusChance, source.StatusEffectLabel);
                return;
            }

            if (skill is BuffSkillData buff)
            {
                buff.Target = BuffTarget.AllAllies;
                buff.BuffDuration = source.CooldownSeconds;
                buff.HasAttachedDamage = source.BaseDamage > 0f;
                MapDamage(buff.AttachedDamage, source);
                buff.AttachedDamageRadius = source.Radius;
                buff.AttachedStatus = CreateStatusApplication(source.StatusEffectId, source.StatusChance, source.StatusEffectLabel);
                return;
            }

            if (skill is ShieldSkillData shield)
            {
                shield.Target = BuffTarget.AllAllies;
                shield.ShieldBase = source.BaseDamage;
                shield.ShieldCoefficient = GetDominantCoefficient(source, out var statSource);
                shield.ShieldStatSource = statSource;
                shield.RefreshRule = ShieldRefreshRule.TakeHighest;
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

        private static StatusApplicationSpec CreateStatusApplication(string statusEffectId, float chance, string statusEffectLabel)
        {
            var application = new StatusApplicationSpec();
            var statusKey = !string.IsNullOrWhiteSpace(statusEffectId)
                ? statusEffectId.Trim()
                : statusEffectLabel != null ? statusEffectLabel.Trim() : string.Empty;
            if (string.IsNullOrWhiteSpace(statusKey))
            {
                application.Chance = 0f;
                return application;
            }

            if (!StatusEffectUtility.TryParse(statusKey, out var kind))
            {
                application.Chance = 0f;
                return application;
            }

            var definition = StatusEffectUtility.GetDefinition(kind);
            var status = CreateTransient<StatusEffectData>(definition.Id);
            status.Kind = kind;
            status.StatusTag = definition.Id;
            status.StatusName = string.IsNullOrWhiteSpace(statusEffectLabel) ? definition.DisplayName : statusEffectLabel;
            application.Status = status;
            application.Chance = Mathf.Clamp01(chance);
            application.Stacks = 1;
            application.RefreshDuration = true;
            return application;
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
                    SkillEffectPrefab = choice != null ? choice.SkillEffectPrefab : null
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
