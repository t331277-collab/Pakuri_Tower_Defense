using System;
using Pakuri.Combat;
using Pakuri.Data;
using UnityEngine;

/*
 * 작성된 Active와 Passive 스킬 데이터를 전투용 SkillRuntimeData로 변환한다.
 * Choice 변환은 SkillChoiceCompiler, 노드 정의 변환은 SkillNodeMapper에 맡긴다.
 */
namespace Pakuri.InGame
{

public static class SkillRuntimeCompiler
{
	public static SkillRuntimeData CompileActive(MonsterDefinition monster, SkillDefinition source)
	{
		if (source == null)
		{
			return null;
		}
		SkillRuntimeData skillRuntimeData = CreateConcreteActiveSkill(source);
		MapCommonFields(skillRuntimeData, (monster != null) ? monster.MonsterId : string.Empty, source, (monster != null) ? monster.SkillTriggers : null);
		MapActiveFields(skillRuntimeData, monster, source);
		return skillRuntimeData;
	}

	public static SkillRuntimeData CompileActive(string monsterId, SkillDefinition source)
	{
		return CompileActive(monsterId, source, null);
	}

	public static SkillRuntimeData CompileActive(string ownerId, SkillDefinition source, SkillTriggerDefinition[] triggers)
	{
		if (source == null)
		{
			return null;
		}
		SkillRuntimeData skillRuntimeData = CreateConcreteActiveSkill(source);
		MapCommonFields(skillRuntimeData, ownerId, source, triggers);
		MapActiveFields(skillRuntimeData, null, source);
		return skillRuntimeData;
	}

	public static PassiveSkillRuntimeData CompilePassive(MonsterDefinition monster, PassiveDefinition source)
	{
		if (source == null)
		{
			return null;
		}
		PassiveSkillRuntimeData passiveSkillRuntimeData = CreateRuntimeData<PassiveSkillRuntimeData>();
		passiveSkillRuntimeData.SkillId = source.PassiveId;
		passiveSkillRuntimeData.SkillName = source.DisplayName;
		passiveSkillRuntimeData.Slot = source.Slot;
		passiveSkillRuntimeData.IsActive = false;
		passiveSkillRuntimeData.Element = ((monster != null) ? monster.PrimaryAttribute : DamageAttribute.Physical);
		passiveSkillRuntimeData.Description = source.DescriptionText;
		passiveSkillRuntimeData.Icon = source.SkillIcon;
		passiveSkillRuntimeData.SkillEffectPrefab = source.SkillEffectPrefab;
		passiveSkillRuntimeData.BaseModifierChoices = SkillChoiceCompiler.Compile(source.BaseModifierChoices);
		passiveSkillRuntimeData.EnhancementChoices = SkillChoiceCompiler.Compile(source.EnhancementChoices);
		passiveSkillRuntimeData.MasterChoices = Array.Empty<SkillChoiceRuntimeData>();
		passiveSkillRuntimeData.MultiEffects = source.PassiveEffects ?? Array.Empty<SkillEffectDefinition>();
		passiveSkillRuntimeData.SkillTriggers = FilterSkillTriggersForSkill((monster != null) ? monster.SkillTriggers : null, source.PassiveId);
		passiveSkillRuntimeData.NormalizedPlanNodes = SkillNodeMapper.MapSkillNodeDefinitions(source.NormalizedPlanNodes);
		passiveSkillRuntimeData.TriggerType = PassiveTrigger.Always;
		passiveSkillRuntimeData.ApplyTarget = PassiveTarget.Self;
		return passiveSkillRuntimeData;
	}

	private static SkillRuntimeData CreateConcreteActiveSkill(SkillDefinition source)
	{
		if (MatchesProfile(source, "DamageArea"))
		{
			return CreateRuntimeData<SingleSkillRuntimeData>();
		}
		if (MatchesProfile(source, "DamageThenDelayedChain"))
		{
			return CreateRuntimeData<SingleChainSkillRuntimeData>();
		}
		if (MatchesProfile(source, "ChargeDamageStatus"))
		{
			return CreateRuntimeData<SingleChargeSkillRuntimeData>();
		}
		if (source.RuntimeKind == SkillRuntimeKind.Heal)
		{
			return CreateRuntimeData<BuffHealSkillRuntimeData>();
		}
		if (MatchesProfile(source, "ApplySelfIncomingDamageMultiplier"))
		{
			return CreateRuntimeData<BuffSkillRuntimeData>();
		}
		switch (source.RuntimeKind)
		{
		case SkillRuntimeKind.MagazineProjectile:
		case SkillRuntimeKind.CooldownProjectile:
			return CreateRuntimeData<ProjectileSkillRuntimeData>();
		case SkillRuntimeKind.LineAttack:
			return CreateRuntimeData<LineSkillRuntimeData>();
		case SkillRuntimeKind.SingleAttack:
		case SkillRuntimeKind.Mark:
		case SkillRuntimeKind.Execute:
			return CreateRuntimeData<SingleSkillRuntimeData>();
		case SkillRuntimeKind.AreaAttack:
		case SkillRuntimeKind.Field:
			return CreateRuntimeData<ZoneSkillRuntimeData>();
		case SkillRuntimeKind.Buff:
			return CreateRuntimeData<BuffSkillRuntimeData>();
		case SkillRuntimeKind.Shield:
			return CreateRuntimeData<BuffShieldSkillRuntimeData>();
		case SkillRuntimeKind.Passive:
			return CreateRuntimeData<PassiveSkillRuntimeData>();
		default:
			return CreateRuntimeData<ProjectileSkillRuntimeData>();
		}
	}

	private static T CreateRuntimeData<T>() where T : SkillRuntimeData, new()
	{
		return new T();
	}

	private static void MapCommonFields(SkillRuntimeData skill, string monsterId, SkillDefinition source, SkillTriggerDefinition[] monsterTriggers = null)
	{
		skill.SkillId = source.SkillId;
		skill.SkillName = source.DisplayName;
		skill.Slot = source.Slot;
		skill.IsActive = source.RuntimeKind != SkillRuntimeKind.Passive;
		skill.Element = source.Attribute;
		skill.Description = source.DescriptionText;
		skill.Icon = source.SkillIcon;
		skill.SkillEffectPrefab = source.SkillEffectPrefab;
		skill.RuntimeVisual = source.RuntimeVisual ?? new RuntimeSkillVisualSpec();
		skill.EnhancementChoices = SkillChoiceCompiler.Compile(source.EnhancementChoices);
		skill.MasterChoices = SkillChoiceCompiler.Compile(source.MasterSkillChoices);
		skill.MultiEffects = source.MultiEffects ?? Array.Empty<SkillEffectDefinition>();
		skill.SkillTriggers = FilterSkillTriggersForSkill(monsterTriggers, source.SkillId);
		skill.NormalizedPlanNodes = SkillNodeMapper.MapSkillNodeDefinitions(source.NormalizedPlanNodes);
		skill.Timing.Cooldown = source.CooldownSeconds;
		skill.Timing.ActiveDuration = source.ActiveDurationSeconds;
		skill.Timing.TickInterval = source.ShotIntervalSeconds;
		skill.MagazineCapacity = source.MagazineCapacity;
		skill.ReloadSeconds = source.ReloadSeconds;
		skill.Targeting.Range = source.CastRange;
		skill.Targeting.Radius = ((source.EffectRadius > 0f) ? source.EffectRadius : source.Radius);
		skill.Targeting.TargetSide = MapEnemyTargetSide(source.TargetScope);
		if (Enum.TryParse<SkillTargetSelection>(source.TargetSelection, ignoreCase: true, out var result))
		{
			skill.Targeting.Selection = result;
		}
		skill.Targeting.SelectionStatusId = source.TargetSelectionStatusId;
		skill.Targeting.SelectionStatusMinStacks = Mathf.Max(0, source.TargetSelectionStatusMinStacks);
		skill.Targeting.Shape = MapShape(source.RuntimeKind);
		skill.Targeting.CoverAll = source.RuntimeKind == SkillRuntimeKind.SingleAttack && source.Radius <= 0f && string.IsNullOrWhiteSpace(source.TargetSelection);
	}

	private static SkillTriggerDefinition[] FilterSkillTriggersForSkill(SkillTriggerDefinition[] triggers, string skillId)
	{
		if (triggers == null || triggers.Length == 0 || string.IsNullOrWhiteSpace(skillId))
		{
			return Array.Empty<SkillTriggerDefinition>();
		}
		int num = 0;
		for (int i = 0; i < triggers.Length; i++)
		{
			if (IsTriggerOwnedBySkill(triggers[i], skillId))
			{
				num++;
			}
		}
		if (num == 0)
		{
			return Array.Empty<SkillTriggerDefinition>();
		}
		SkillTriggerDefinition[] array = new SkillTriggerDefinition[num];
		int num2 = 0;
		for (int j = 0; j < triggers.Length; j++)
		{
			if (IsTriggerOwnedBySkill(triggers[j], skillId))
			{
				array[num2] = triggers[j];
				num2++;
			}
		}
		return array;
	}

	private static bool IsTriggerOwnedBySkill(SkillTriggerDefinition trigger, string skillId)
	{
		if (trigger != null && !string.IsNullOrWhiteSpace(trigger.SourceSkillId))
		{
			return string.Equals(trigger.SourceSkillId, skillId, StringComparison.OrdinalIgnoreCase);
		}
		return false;
	}

	private static void MapActiveFields(SkillRuntimeData skill, MonsterDefinition monster, SkillDefinition source)
	{
		if (skill is ProjectileSkillRuntimeData projectileSkillRuntimeData)
		{
			projectileSkillRuntimeData.Projectile.MagazineSize = source.MagazineCapacity;
			projectileSkillRuntimeData.Projectile.ReloadTime = source.ReloadSeconds;
			projectileSkillRuntimeData.Projectile.BurstProjectileCount = Math.Max(1, source.ProjectileBurstCount);
			projectileSkillRuntimeData.Projectile.BurstIntervalSeconds = ((source.BurstIntervalSeconds > 0f) ? source.BurstIntervalSeconds : source.ShotIntervalSeconds);
			projectileSkillRuntimeData.Projectile.BurstDamageProjectileIndex = source.BurstDamageProjectileIndex;
			projectileSkillRuntimeData.Projectile.BurstDamageMultiplier = ((source.BurstDamageMultiplier > 0f) ? source.BurstDamageMultiplier : 1f);
			projectileSkillRuntimeData.Projectile.ProjectilesPerShot = 1;
			projectileSkillRuntimeData.Projectile.PierceCount = source.PierceCount;
			projectileSkillRuntimeData.Projectile.ProjectileSpeed = source.ProjectileSpeed;
			projectileSkillRuntimeData.Projectile.LifetimeSeconds = source.ProjectileLifetimeSeconds;
			projectileSkillRuntimeData.ContactDamageEnabled = source.DamageDelaySeconds <= 0f;
			projectileSkillRuntimeData.StopOnFirstHit = source.DamageDelaySeconds > 0f;
			projectileSkillRuntimeData.ImpactDelaySeconds = Mathf.Max(0f, source.DamageDelaySeconds);
			projectileSkillRuntimeData.ImpactRuntimeVisual = source.ImpactRuntimeVisual ?? new RuntimeSkillVisualSpec();
			projectileSkillRuntimeData.HasImpactArea = source.DamageDelaySeconds > 0f;
			projectileSkillRuntimeData.ImpactArea.Radius = source.Radius;
			projectileSkillRuntimeData.ImpactArea.CoverAll = false;
			MapDamage(projectileSkillRuntimeData.Damage, source);
			MapDamage(projectileSkillRuntimeData.ImpactDamage, source);
			projectileSkillRuntimeData.OnHitStatus = CreateStatusApplication(source);
			projectileSkillRuntimeData.ImpactStatus = CreateStatusApplication(source);
		}
		else if (skill is LineSkillRuntimeData lineSkillRuntimeData)
		{
			lineSkillRuntimeData.LineLength = 0f;
			lineSkillRuntimeData.LineWidth = source.Radius;
			lineSkillRuntimeData.KnockbackDistance = source.KnockbackDistance;
			MapDamage(lineSkillRuntimeData.DamagePerTick, source);
			lineSkillRuntimeData.OnHitStatus = CreateStatusApplication(source);
		}
		else if (skill is ZoneSkillRuntimeData zoneSkillRuntimeData)
		{
			bool hitAllTargets;
			int hitTargetCount;
			bool usesHitTargetCount = TryResolveHitTargetCount(source.HitTargetCount, out hitAllTargets, out hitTargetCount);
			zoneSkillRuntimeData.Area.Radius = source.Radius;
			zoneSkillRuntimeData.Area.Duration = ((source.ActiveDurationSeconds > 0f) ? source.ActiveDurationSeconds : source.CooldownSeconds);
			zoneSkillRuntimeData.Area.TickInterval = source.ShotIntervalSeconds;
			zoneSkillRuntimeData.UsesHitTargetCount = usesHitTargetCount;
			zoneSkillRuntimeData.HitAllTargets = hitAllTargets;
			zoneSkillRuntimeData.HitTargetCount = (hitAllTargets ? int.MaxValue : Math.Max(1, hitTargetCount));
			zoneSkillRuntimeData.Area.CoverAll = hitAllTargets;
			MapDamage(zoneSkillRuntimeData.DamagePerTick, source);
			zoneSkillRuntimeData.OnTickStatus = CreateStatusApplication(source);
		}
		else if (skill is SingleSkillRuntimeData singleSkillRuntimeData)
		{
			bool hitAllTargets2;
			int hitTargetCount2;
			bool flag = TryResolveHitTargetCount(source.HitTargetCount, out hitAllTargets2, out hitTargetCount2);
			bool flag2 = !string.IsNullOrWhiteSpace(source.DeploymentRequiredTargetStatusId);
			bool flag3 = source.RuntimeVisual != null && source.RuntimeVisual.Hitbox != null && source.RuntimeVisual.Hitbox.HasHitbox();
			bool flag4 = !hitAllTargets2 && flag && hitTargetCount2 > 1 && (source.SkillEffectPrefab != null || flag3);
			singleSkillRuntimeData.Area.Radius = source.Radius;
			singleSkillRuntimeData.Area.Duration = 0f;
			singleSkillRuntimeData.Area.TickInterval = 0f;
			singleSkillRuntimeData.UsesHitTargetCount = !flag4 && (flag || source.Radius <= 0f);
			singleSkillRuntimeData.UsePrefabHitbox = source.UsePrefabHitbox || hitAllTargets2 || flag4 || flag2;
			singleSkillRuntimeData.UseMultiDeployment = flag4 || flag2;
			singleSkillRuntimeData.HitAllTargets = hitAllTargets2;
			singleSkillRuntimeData.HitTargetCount = ((hitAllTargets2 || (source.UsePrefabHitbox && !flag)) ? int.MaxValue : Math.Max(1, hitTargetCount2));
			singleSkillRuntimeData.DeploymentCount = ((!flag4) ? 1 : Math.Max(1, hitTargetCount2));
			singleSkillRuntimeData.DeploymentRequiredTargetStatusId = source.DeploymentRequiredTargetStatusId;
			singleSkillRuntimeData.DeploymentRequiredTargetStatusMinStacks = Mathf.Max(0, source.DeploymentRequiredTargetStatusMinStacks);
			singleSkillRuntimeData.TargetStatusStackStatusId = source.TargetStatusStackStatusId;
			singleSkillRuntimeData.TargetStatusStackMaxStacks = Mathf.Max(0, source.TargetStatusStackMaxStacks);
			singleSkillRuntimeData.ConsumeTargetStatusId = source.ConsumeTargetStatusId;
			singleSkillRuntimeData.ConsumeTargetStatusRatio = Mathf.Clamp01(source.ConsumeTargetStatusRatio);
			singleSkillRuntimeData.ConsumeTargetStatusStacks = Mathf.Max(0, source.ConsumeTargetStatusStacks);
			singleSkillRuntimeData.DamageDelaySeconds = Mathf.Max(0f, source.DamageDelaySeconds);
			singleSkillRuntimeData.ExecuteHealthRatioThreshold = Mathf.Clamp01(source.ExecuteHealthRatioThreshold);
			singleSkillRuntimeData.RequireExecuteThresholdToCast = source.RequireExecuteThresholdToCast;
			singleSkillRuntimeData.ExecuteDamageMultiplier = ((source.ExecuteDamageMultiplier > 0f) ? source.ExecuteDamageMultiplier : 1f);
			singleSkillRuntimeData.KillCooldownRefundRatio = Mathf.Clamp01(source.KillCooldownRefundRatio);
			singleSkillRuntimeData.BossDamageMultiplier = ((source.BossDamageMultiplier > 0f) ? source.BossDamageMultiplier : 1f);
			singleSkillRuntimeData.Area.CoverAll = hitAllTargets2 || (!singleSkillRuntimeData.UsesHitTargetCount && source.Radius <= 0f && string.IsNullOrWhiteSpace(source.TargetSelection));
			MapDamage(singleSkillRuntimeData.Damage, source);
			singleSkillRuntimeData.TargetStatusStackDamage.Element = source.Attribute;
			singleSkillRuntimeData.TargetStatusStackDamage.BaseDamage = source.TargetStatusStackBaseDamage;
			singleSkillRuntimeData.TargetStatusStackDamage.StatCoefficient = GetDominantCoefficient(source.TargetStatusStackAttackPowerCoefficient, source.TargetStatusStackSpellPowerCoefficient, out var statSource);
			singleSkillRuntimeData.TargetStatusStackDamage.StatSource = statSource;
			singleSkillRuntimeData.TargetStatusStackDamage.CriticalAllowed = false;
			ApplySingleBasePlanNodes(singleSkillRuntimeData, source.NormalizedPlanNodes, source.Attribute);
			if (!string.IsNullOrWhiteSpace(singleSkillRuntimeData.DeploymentRequiredTargetStatusId))
			{
				singleSkillRuntimeData.UsePrefabHitbox = true;
				singleSkillRuntimeData.UseMultiDeployment = true;
			}
			singleSkillRuntimeData.OnHitStatus = CreateStatusApplication(source);
		}
		else if (skill is SingleChainSkillRuntimeData singleChainSkillRuntimeData)
		{
			MapDamage(singleChainSkillRuntimeData.Damage, source);
			singleChainSkillRuntimeData.ChainDamageMultiplier = source.ChainDamageMultiplier;
			singleChainSkillRuntimeData.ChainDelaySeconds = source.ChainDelaySeconds;
			singleChainSkillRuntimeData.ChainRadius = ((source.ChainRadius > 0f) ? source.ChainRadius : source.Radius);
			singleChainSkillRuntimeData.ExcludePrimaryTarget = source.ExcludePrimaryTarget;
		}
		else if (skill is SingleChargeSkillRuntimeData singleChargeSkillRuntimeData)
		{
			singleChargeSkillRuntimeData.TargetMaxHealthRatio = source.TargetMaxHealthRatio;
			singleChargeSkillRuntimeData.RampSeconds = source.ChargeRampSeconds;
			singleChargeSkillRuntimeData.MaxMoveSpeedMultiplier = ((source.MoveSpeedMultiplier > 1f) ? source.MoveSpeedMultiplier : source.ChargeMoveSpeedMultiplier);
			singleChargeSkillRuntimeData.OnHitStatus = CreateStatusApplication(source);
		}
		else if (skill is BuffHealSkillRuntimeData buffHealSkillRuntimeData)
		{
			MapDamage(buffHealSkillRuntimeData.Healing, source);
			buffHealSkillRuntimeData.Healing.BaseDamage = source.FlatValue;
		}
		else if (skill is BuffSkillRuntimeData buffSkillRuntimeData)
		{
			buffSkillRuntimeData.Target = MapBuffTarget(source, StatusEffectKind.None);
			buffSkillRuntimeData.UseConfiguredTargeting = !string.IsNullOrWhiteSpace(source.TargetScope);
			buffSkillRuntimeData.AttachVisualToCaster = MatchesProfile(source, "ApplyAllyMoveAndDamageMultiplier");
			buffSkillRuntimeData.BuffDuration = ResolveStatusDuration(source);
			buffSkillRuntimeData.HasAttachedDamage = source.BaseDamage > 0f;
			MapDamage(buffSkillRuntimeData.AttachedDamage, source);
			buffSkillRuntimeData.AttachedDamageRadius = source.Radius;
			buffSkillRuntimeData.AttachedStatus = CreateStatusApplication(source);
		}
		else if (skill is BuffShieldSkillRuntimeData buffShieldSkillRuntimeData)
		{
			buffShieldSkillRuntimeData.Target = MapBuffTarget(source, StatusEffectKind.Shield);
			buffShieldSkillRuntimeData.UseConfiguredTargeting = !string.IsNullOrWhiteSpace(source.TargetScope);
			buffShieldSkillRuntimeData.AttachVisualToCaster = MatchesProfile(source, "GrantShieldToEnemyAllies");
			buffShieldSkillRuntimeData.ShieldBase = source.BaseDamage;
			buffShieldSkillRuntimeData.ShieldCoefficient = GetDominantCoefficient(source, out var statSource2);
			buffShieldSkillRuntimeData.ShieldStatSource = statSource2;
			buffShieldSkillRuntimeData.ShieldDuration = ResolveStatusDuration(source);
			buffShieldSkillRuntimeData.RefreshRule = ((!StatusRuntimeDataFactory.TryParseShieldRefreshRule(source.ShieldAmountRefreshPolicy, out var rule)) ? ShieldRefreshRule.TakeHighest : rule);
			buffShieldSkillRuntimeData.ShieldStatus = CreateStatusRuntimeData(source);
			buffShieldSkillRuntimeData.ReflectElement = source.Attribute;
		}
	}

	private static void MapDamage(SkillDamageSpec damage, SkillDefinition source)
	{
		damage.SkillId = source.SkillId;
		damage.Element = source.Attribute;
		damage.BaseDamage = source.BaseDamage;
		damage.StatCoefficient = GetDominantCoefficient(source, out var statSource);
		damage.StatSource = statSource;
		damage.UseCombinedStatCoefficients = source.UseCombinedStatCoefficients;
		damage.AttackPowerCoefficient = source.AttackPowerCoefficient;
		damage.SpellPowerCoefficient = source.SpellPowerCoefficient;
		damage.CriticalAllowed = source.CriticalAllowed;
	}

	private static SkillTargetSide MapEnemyTargetSide(string targetScope)
	{
		if (string.IsNullOrWhiteSpace(targetScope))
		{
			return SkillTargetSide.Enemy;
		}
		if (string.Equals(targetScope, "Self", StringComparison.OrdinalIgnoreCase))
		{
			return SkillTargetSide.Self;
		}
		if (targetScope.StartsWith("Friendly", StringComparison.OrdinalIgnoreCase))
		{
			return SkillTargetSide.AllAllies;
		}
		return SkillTargetSide.Enemy;
	}

	private static bool MatchesProfile(SkillDefinition source, string profile)
	{
		if (source != null)
		{
			return string.Equals(source.ExecutionProfile, profile, StringComparison.OrdinalIgnoreCase);
		}
		return false;
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
		StatusApplicationSpec statusApplicationSpec = new StatusApplicationSpec();
		StatusRuntimeData runtimeStatusData = (statusApplicationSpec.Status = CreateStatusRuntimeData(source));
		statusApplicationSpec.Chance = Mathf.Clamp01(source?.StatusChance ?? 0f);
		statusApplicationSpec.Stacks = ((runtimeStatusData == null) ? 1 : Math.Max(1, runtimeStatusData.BaseStackAmount));
		statusApplicationSpec.RefreshDuration = true;
		return statusApplicationSpec;
	}

	private static StatusRuntimeData CreateStatusRuntimeData(SkillDefinition source)
	{
		if (source == null)
		{
			return null;
		}
		string value = ((!string.IsNullOrWhiteSpace(source.StatusEffectId)) ? source.StatusEffectId.Trim() : source.StatusEffectLabel);
		if (string.IsNullOrWhiteSpace(value) || !StatusEffectLookup.TryParse(value, out var kind))
		{
			return null;
		}
		StatusRuntimeData runtimeStatusData = StatusRuntimeDataFactory.Create(kind, source.StatusEffectLabel, source);
		if (runtimeStatusData != null && source.StatusEffectPrefab != null)
		{
			runtimeStatusData.StatusEffectPrefab = source.StatusEffectPrefab;
		}
		return runtimeStatusData;
	}

	private static bool TryResolveHitTargetCount(string rawValue, out bool hitAllTargets, out int hitTargetCount)
	{
		hitAllTargets = false;
		hitTargetCount = 1;
		if (string.IsNullOrWhiteSpace(rawValue))
		{
			return false;
		}
		string text = rawValue.Trim();
		if (string.Equals(text, "global", StringComparison.OrdinalIgnoreCase) || string.Equals(text, "all", StringComparison.OrdinalIgnoreCase))
		{
			hitAllTargets = true;
			hitTargetCount = int.MaxValue;
			return true;
		}
		if (int.TryParse(text, out var result) && result > 0)
		{
			hitTargetCount = result;
			return true;
		}
		return true;
	}

	private static BuffTarget MapBuffTarget(SkillDefinition source, StatusEffectKind fallbackKind)
	{
		if (source != null && StatusRuntimeDataFactory.TryParseTargetScope(source.StatusTargetScope, out var scope))
		{
			if (scope != StatusTargetScope.Self)
			{
				return BuffTarget.AllAllies;
			}
			return BuffTarget.Self;
		}
		if (source != null && StatusEffectLookup.TryParse((!string.IsNullOrWhiteSpace(source.StatusEffectId)) ? source.StatusEffectId : source.StatusEffectLabel, out var kind) && kind == StatusEffectKind.SlaughterPermit)
		{
			return BuffTarget.Self;
		}
		if (fallbackKind != StatusEffectKind.Shield)
		{
			return BuffTarget.AllAllies;
		}
		return BuffTarget.AllAllies;
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


	private static void ApplySingleBasePlanNodes(SingleSkillRuntimeData single, SkillNodeDefinition[] nodes, DamageAttribute attribute)
	{
		if (single == null || nodes == null)
		{
			return;
		}
		foreach (SkillNodeDefinition skillNodeDefinition in nodes)
		{
			if (skillNodeDefinition != null && skillNodeDefinition.EnabledByDefault)
			{
				string a = skillNodeDefinition.HandlerId ?? string.Empty;
				if (string.Equals(a, "StatusFilteredDeployment", StringComparison.OrdinalIgnoreCase))
				{
					single.DeploymentRequiredTargetStatusId = SkillNodeMapper.GetParam(skillNodeDefinition, "status_id");
					single.DeploymentRequiredTargetStatusMinStacks = Mathf.Max(1, SkillNodeMapper.GetIntParam(skillNodeDefinition, "min_stacks", 1));
				}
				else if (string.Equals(a, "TargetStatusStackDamage", StringComparison.OrdinalIgnoreCase))
				{
					single.TargetStatusStackStatusId = SkillNodeMapper.GetParam(skillNodeDefinition, "status_id");
					single.TargetStatusStackMaxStacks = Mathf.Max(0, SkillNodeMapper.GetIntParam(skillNodeDefinition, "max_stacks", 0));
					single.TargetStatusStackDamage.Element = attribute;
					single.TargetStatusStackDamage.BaseDamage = SkillNodeMapper.GetFloatParam(skillNodeDefinition, "base_damage", 0f);
					float floatParam = SkillNodeMapper.GetFloatParam(skillNodeDefinition, "attack_power_coefficient", 0f);
					float floatParam2 = SkillNodeMapper.GetFloatParam(skillNodeDefinition, "spell_power_coefficient", 0f);
					single.TargetStatusStackDamage.StatCoefficient = GetDominantCoefficient(floatParam, floatParam2, out var statSource);
					single.TargetStatusStackDamage.StatSource = statSource;
					single.TargetStatusStackDamage.CriticalAllowed = false;
				}
			}
		}
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
