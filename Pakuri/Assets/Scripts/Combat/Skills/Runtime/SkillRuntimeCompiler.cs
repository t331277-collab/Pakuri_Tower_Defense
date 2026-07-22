using System;
using System.Collections.Generic;
using System.Globalization;
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
	/*
	 * CompileActive 작업 결과를 반환한다.
	 */
	public static SkillRuntimeData CompileActive(MonsterDefinition monster, SkillDefinition source)
	{
		SkillRuntimeData skillRuntimeData = CreateConcreteActiveSkill(source);
		string monsterId = string.Empty;
		SkillTriggerDefinition[] triggers = null;
		if (monster != null)
		{
			monsterId = monster.MonsterId;
			triggers = monster.SkillTriggers;
		}
		MapCommonFields(skillRuntimeData, monsterId, source, triggers);
		MapActiveFields(skillRuntimeData, monster, source);
		return skillRuntimeData;
	}

	/*
	 * CompileActive 작업 결과를 반환한다.
	 */
	public static SkillRuntimeData CompileActive(string monsterId, SkillDefinition source)
	{
		return CompileActive(monsterId, source, null);
	}

	/*
	 * CompileActive 작업 결과를 반환한다.
	 */
	public static SkillRuntimeData CompileActive(string ownerId, SkillDefinition source, SkillTriggerDefinition[] triggers)
	{
		SkillRuntimeData skillRuntimeData = CreateConcreteActiveSkill(source);
		MapCommonFields(skillRuntimeData, ownerId, source, triggers);
		MapActiveFields(skillRuntimeData, null, source);
		return skillRuntimeData;
	}

	/*
	 * CompilePassive 작업 결과를 반환한다.
	 */
	public static PassiveSkillRuntimeData CompilePassive(MonsterDefinition monster, PassiveDefinition source)
	{
		PassiveSkillRuntimeData passiveSkillRuntimeData = CreateRuntimeData<PassiveSkillRuntimeData>();
		passiveSkillRuntimeData.SkillId = source.PassiveId;
		passiveSkillRuntimeData.SkillName = source.DisplayName;
		passiveSkillRuntimeData.Slot = source.Slot;
		passiveSkillRuntimeData.IsActive = false;
		passiveSkillRuntimeData.Element = DamageAttribute.Physical;
		if (monster != null)
		{
			passiveSkillRuntimeData.Element = monster.PrimaryAttribute;
		}
		passiveSkillRuntimeData.Description = source.DescriptionText;
		passiveSkillRuntimeData.Icon = source.SkillIcon;
		passiveSkillRuntimeData.SkillEffectPrefab = source.SkillEffectPrefab;
		passiveSkillRuntimeData.BaseModifierChoices = SkillChoiceCompiler.Compile(source.BaseModifierChoices);
		passiveSkillRuntimeData.EnhancementChoices = SkillChoiceCompiler.Compile(source.EnhancementChoices);
		passiveSkillRuntimeData.MasterChoices = Array.Empty<SkillChoiceRuntimeData>();
		passiveSkillRuntimeData.MultiEffects = source.PassiveEffects;
		StatusRuntimeCompiler.CompileSkillEffects(passiveSkillRuntimeData.MultiEffects);
		SkillTriggerDefinition[] triggers = null;
		if (monster != null)
		{
			triggers = monster.SkillTriggers;
		}
		passiveSkillRuntimeData.SkillTriggers = FilterSkillTriggersForSkill(triggers, source.PassiveId);
		StatusRuntimeCompiler.CompileTriggers(passiveSkillRuntimeData.SkillTriggers);
		passiveSkillRuntimeData.NormalizedPlanNodes = SkillNodeMapper.MapSkillNodeDefinitions(source.NormalizedPlanNodes);
		return passiveSkillRuntimeData;
	}

	/*
	 * CreateConcreteActiveSkill에 필요한 결과를 만들어 반환한다.
	 */
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
		default:
			throw new InvalidOperationException("Unsupported active skill runtime kind: " + source.RuntimeKind);
		}
	}

	/*
	 * CreateRuntimeData에 필요한 결과를 만들어 반환한다.
	 */
	private static T CreateRuntimeData<T>() where T : SkillRuntimeData, new()
	{
		return new T();
	}

	/*
	 * MapCommonFields에 필요한 값을 변환해 현재 상태에 반영한다.
	 */
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
		skill.RuntimeVisual = source.RuntimeVisual;
		skill.EnhancementChoices = SkillChoiceCompiler.Compile(source.EnhancementChoices);
		skill.MasterChoices = SkillChoiceCompiler.Compile(source.MasterSkillChoices);
		skill.MultiEffects = source.MultiEffects;
		StatusRuntimeCompiler.CompileSkillEffects(skill.MultiEffects);
		skill.SkillTriggers = FilterSkillTriggersForSkill(monsterTriggers, source.SkillId);
		StatusRuntimeCompiler.CompileTriggers(skill.SkillTriggers);
		skill.NormalizedPlanNodes = SkillNodeMapper.MapSkillNodeDefinitions(source.NormalizedPlanNodes);
		skill.Timing.Cooldown = source.CooldownSeconds;
		skill.Timing.ActiveDuration = source.ActiveDurationSeconds;
		skill.Timing.TickInterval = source.ShotIntervalSeconds;
		skill.MagazineCapacity = source.MagazineCapacity;
		skill.ReloadSeconds = source.ReloadSeconds;
		skill.Targeting.Range = source.CastRange;
		skill.Targeting.Radius = source.Radius;
		if (source.EffectRadius > 0f)
		{
			skill.Targeting.Radius = source.EffectRadius;
		}
		skill.Targeting.TargetSide = MapEnemyTargetSide(source.TargetScope);
		if (Enum.TryParse<SkillTargetSelection>(source.TargetSelection, ignoreCase: true, out var result))
		{
			skill.Targeting.Selection = result;
		}
		skill.Targeting.SelectionStatusId = source.TargetSelectionStatusId;
		if (!string.IsNullOrWhiteSpace(source.TargetSelectionStatusId))
		{
			skill.Targeting.SelectionStatusKind = StatusRuntimeCompiler.ParseStatusKind(
				source.TargetSelectionStatusId);
		}
		skill.Targeting.SelectionStatusMinStacks = Mathf.Max(0, source.TargetSelectionStatusMinStacks);
		skill.Targeting.Shape = MapShape(source.RuntimeKind);
		skill.Targeting.CoverAll = source.RuntimeKind == SkillRuntimeKind.SingleAttack && source.Radius <= 0f && string.IsNullOrWhiteSpace(source.TargetSelection);
	}

	/*
	 * FilterSkillTriggersForSkill에 해당하는 값을 찾아 반환한다.
	 */
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

	/*
	 * IsTriggerOwnedBySkill 조건을 만족하는지 확인한다.
	 */
	private static bool IsTriggerOwnedBySkill(SkillTriggerDefinition trigger, string skillId)
	{
		if (trigger != null && !string.IsNullOrWhiteSpace(trigger.SourceSkillId))
		{
			return string.Equals(trigger.SourceSkillId, skillId, StringComparison.OrdinalIgnoreCase);
		}
		return false;
	}

	/*
	 * MapActiveFields에 필요한 값을 변환해 현재 상태에 반영한다.
	 */
	private static void MapActiveFields(SkillRuntimeData skill, MonsterDefinition monster, SkillDefinition source)
	{
		if (skill is ProjectileSkillRuntimeData projectileSkillRuntimeData)
		{
			projectileSkillRuntimeData.Projectile.MagazineSize = source.MagazineCapacity;
			projectileSkillRuntimeData.Projectile.ReloadTime = source.ReloadSeconds;
			projectileSkillRuntimeData.Projectile.BurstProjectileCount = Math.Max(1, source.ProjectileBurstCount);
			projectileSkillRuntimeData.Projectile.BurstIntervalSeconds = source.ShotIntervalSeconds;
			if (source.BurstIntervalSeconds > 0f)
			{
				projectileSkillRuntimeData.Projectile.BurstIntervalSeconds = source.BurstIntervalSeconds;
			}
			projectileSkillRuntimeData.Projectile.BurstDamageProjectileIndex = source.BurstDamageProjectileIndex;
			projectileSkillRuntimeData.Projectile.BurstDamageMultiplier = 1f;
			if (source.BurstDamageMultiplier > 0f)
			{
				projectileSkillRuntimeData.Projectile.BurstDamageMultiplier = source.BurstDamageMultiplier;
			}
			projectileSkillRuntimeData.Projectile.ProjectilesPerShot = 1;
			projectileSkillRuntimeData.Projectile.PierceCount = source.PierceCount;
			projectileSkillRuntimeData.Projectile.ProjectileSpeed = source.ProjectileSpeed;
			projectileSkillRuntimeData.Projectile.LifetimeSeconds = source.ProjectileLifetimeSeconds;
			projectileSkillRuntimeData.ContactDamageEnabled = source.DamageDelaySeconds <= 0f;
			projectileSkillRuntimeData.StopOnFirstHit = source.DamageDelaySeconds > 0f;
			projectileSkillRuntimeData.ImpactDelaySeconds = Mathf.Max(0f, source.DamageDelaySeconds);
			projectileSkillRuntimeData.ImpactRuntimeVisual = source.ImpactRuntimeVisual;
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
			zoneSkillRuntimeData.Area.Duration = source.CooldownSeconds;
			if (source.ActiveDurationSeconds > 0f)
			{
				zoneSkillRuntimeData.Area.Duration = source.ActiveDurationSeconds;
			}
			zoneSkillRuntimeData.Area.TickInterval = source.ShotIntervalSeconds;
			zoneSkillRuntimeData.UsesHitTargetCount = usesHitTargetCount;
			zoneSkillRuntimeData.HitAllTargets = hitAllTargets;
			zoneSkillRuntimeData.HitTargetCount = hitTargetCount;
			if (hitAllTargets)
			{
				zoneSkillRuntimeData.HitTargetCount = int.MaxValue;
			}
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
			singleSkillRuntimeData.HitTargetCount = hitTargetCount2;
			if (hitAllTargets2 || (source.UsePrefabHitbox && !flag))
			{
				singleSkillRuntimeData.HitTargetCount = int.MaxValue;
			}
			singleSkillRuntimeData.DeploymentCount = 1;
			if (flag4)
			{
				singleSkillRuntimeData.DeploymentCount = hitTargetCount2;
			}
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
			singleSkillRuntimeData.ExecuteDamageMultiplier = 1f;
			if (source.ExecuteDamageMultiplier > 0f)
			{
				singleSkillRuntimeData.ExecuteDamageMultiplier = source.ExecuteDamageMultiplier;
			}
			singleSkillRuntimeData.KillCooldownRefundRatio = Mathf.Clamp01(source.KillCooldownRefundRatio);
			singleSkillRuntimeData.BossDamageMultiplier = 1f;
			if (source.BossDamageMultiplier > 0f)
			{
				singleSkillRuntimeData.BossDamageMultiplier = source.BossDamageMultiplier;
			}
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
				singleSkillRuntimeData.DeploymentRequiredTargetStatusKind = StatusRuntimeCompiler.ParseStatusKind(
					singleSkillRuntimeData.DeploymentRequiredTargetStatusId);
			}
			if (!string.IsNullOrWhiteSpace(singleSkillRuntimeData.TargetStatusStackStatusId))
			{
				singleSkillRuntimeData.TargetStatusStackStatusKind = StatusRuntimeCompiler.ParseStatusKind(
					singleSkillRuntimeData.TargetStatusStackStatusId);
			}
			if (!string.IsNullOrWhiteSpace(singleSkillRuntimeData.ConsumeTargetStatusId))
			{
				singleSkillRuntimeData.ConsumeTargetStatusKind = StatusRuntimeCompiler.ParseStatusKind(
					singleSkillRuntimeData.ConsumeTargetStatusId);
			}
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
			singleChainSkillRuntimeData.ChainRadius = source.Radius;
			if (source.ChainRadius > 0f)
			{
				singleChainSkillRuntimeData.ChainRadius = source.ChainRadius;
			}
			singleChainSkillRuntimeData.ExcludePrimaryTarget = source.ExcludePrimaryTarget;
		}
		else if (skill is SingleChargeSkillRuntimeData singleChargeSkillRuntimeData)
		{
			singleChargeSkillRuntimeData.TargetMaxHealthRatio = source.TargetMaxHealthRatio;
			singleChargeSkillRuntimeData.RampSeconds = source.ChargeRampSeconds;
			singleChargeSkillRuntimeData.MaxMoveSpeedMultiplier = source.ChargeMoveSpeedMultiplier;
			if (source.MoveSpeedMultiplier > 1f)
			{
				singleChargeSkillRuntimeData.MaxMoveSpeedMultiplier = source.MoveSpeedMultiplier;
			}
			singleChargeSkillRuntimeData.OnHitStatus = CreateStatusApplication(source);
		}
		else if (skill is BuffHealSkillRuntimeData buffHealSkillRuntimeData)
		{
			MapDamage(buffHealSkillRuntimeData.Healing, source);
			buffHealSkillRuntimeData.Healing.BaseDamage = source.FlatValue;
		}
		else if (skill is BuffSkillRuntimeData buffSkillRuntimeData)
		{
			buffSkillRuntimeData.Target = MapBuffTarget(source);
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
			buffShieldSkillRuntimeData.Target = MapBuffTarget(source);
			buffShieldSkillRuntimeData.UseConfiguredTargeting = !string.IsNullOrWhiteSpace(source.TargetScope);
			buffShieldSkillRuntimeData.AttachVisualToCaster = MatchesProfile(source, "GrantShieldToEnemyAllies");
			buffShieldSkillRuntimeData.ShieldBase = source.BaseDamage;
			buffShieldSkillRuntimeData.ShieldCoefficient = GetDominantCoefficient(source, out var statSource2);
			buffShieldSkillRuntimeData.ShieldStatSource = statSource2;
			buffShieldSkillRuntimeData.ShieldDuration = ResolveStatusDuration(source);
			ShieldRefreshRule rule;
		if (!StatusRuntimeCompiler.TryParseShieldRefreshRule(source.ShieldAmountRefreshPolicy, out rule))
			{
				rule = ShieldRefreshRule.TakeHighest;
			}
			buffShieldSkillRuntimeData.RefreshRule = rule;
			buffShieldSkillRuntimeData.ShieldStatus = CreateStatusRuntimeData(source);
			buffShieldSkillRuntimeData.ReflectElement = source.Attribute;
		}
	}

	/*
	 * MapDamage에 필요한 값을 변환해 현재 상태에 반영한다.
	 */
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

	/*
	 * MapEnemyTargetSide에 필요한 형식으로 변환해 반환한다.
	 */
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

	/*
	 * MatchesProfile 조건을 만족하는지 확인한다.
	 */
	private static bool MatchesProfile(SkillDefinition source, string profile)
	{
		return string.Equals(source.ExecutionProfile, profile, StringComparison.OrdinalIgnoreCase);
	}

	/*
	 * GetDominantCoefficient에 해당하는 값을 찾아 반환한다.
	 */
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

	/*
	 * GetDominantCoefficient에 해당하는 값을 찾아 반환한다.
	 */
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

	/*
	 * CreateStatusApplication에 필요한 결과를 만들어 반환한다.
	 */
	private static StatusApplicationSpec CreateStatusApplication(SkillDefinition source)
	{
		StatusApplicationSpec statusApplicationSpec = new StatusApplicationSpec();
		StatusRuntimeData runtimeStatusData = (statusApplicationSpec.Status = CreateStatusRuntimeData(source));
		statusApplicationSpec.Chance = Mathf.Clamp01(source.StatusChance);
		statusApplicationSpec.Stacks = 1;
		if (runtimeStatusData != null)
		{
			statusApplicationSpec.Stacks = Math.Max(1, runtimeStatusData.BaseStackAmount);
		}
		statusApplicationSpec.RefreshDuration = true;
		return statusApplicationSpec;
	}

	/*
	 * CreateStatusRuntimeData에 필요한 결과를 만들어 반환한다.
	 */
	private static StatusRuntimeData CreateStatusRuntimeData(SkillDefinition source)
	{
		if (string.IsNullOrWhiteSpace(source.StatusEffectId))
		{
			return null;
		}
		StatusEffectKind kind = StatusRuntimeCompiler.ParseStatusKind(source.StatusEffectId);
		StatusRuntimeData runtimeStatusData = StatusRuntimeCompiler.Create(kind, source.StatusEffectLabel, source);
		if (runtimeStatusData != null && source.StatusEffectPrefab != null)
		{
			runtimeStatusData.StatusEffectPrefab = source.StatusEffectPrefab;
		}
		return runtimeStatusData;
	}

	/*
	 * TryResolveHitTargetCount 작업을 시도하고 성공 여부를 반환한다.
	 */
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
		hitTargetCount = int.Parse(text);
		return true;
	}

	/*
	 * MapBuffTarget에 필요한 형식으로 변환해 반환한다.
	 */
	private static SkillTargetSide MapBuffTarget(SkillDefinition source)
	{
		StatusRuntimeCompiler.TryParseTargetScope(source.StatusTargetScope, out var scope);
		if (scope == StatusTargetScope.Self)
		{
			return SkillTargetSide.Self;
		}

		return SkillTargetSide.AllAllies;
	}

	/*
	 * ResolveStatusDuration 결과를 계산해 반환한다.
	 */
	private static float ResolveStatusDuration(SkillDefinition source)
	{
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


	/*
	 * ApplySingleBasePlanNodes 처리를 대상에 적용한다.
	 */
	private static void ApplySingleBasePlanNodes(SingleSkillRuntimeData single, SkillNodeDefinition[] nodes, DamageAttribute attribute)
	{
		foreach (SkillNodeDefinition skillNodeDefinition in nodes)
		{
			if (skillNodeDefinition != null && skillNodeDefinition.EnabledByDefault)
			{
				string a = skillNodeDefinition.HandlerId;
				if (a == null)
				{
					a = string.Empty;
				}
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


	/*
	 * MapShape에 필요한 형식으로 변환해 반환한다.
	 */
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


/*
 * 작성 데이터의 SkillNodeDefinition을 전투 실행용 SkillNode로 변환한다.
 * 실행 순서를 만드는 SkillNodeCompiler와 달리 노드 종류와 파라미터 해석만 담당한다.
 */
namespace Pakuri.InGame
{
    public static class SkillNodeMapper
    {
	/*
	 * MapSkillNodeDefinitions에 필요한 형식으로 변환해 반환한다.
	 */
	public static SkillNode[] MapSkillNodeDefinitions(SkillNodeDefinition[] source)
	{
		if (source.Length == 0)
		{
			return Array.Empty<SkillNode>();
		}
		List<SkillNode> list = new List<SkillNode>(source.Length);
		for (int i = 0; i < source.Length; i++)
		{
			SkillNode skillExecutionPlanNode = MapSkillNodeDefinition(source[i]);
			if (skillExecutionPlanNode != null)
			{
				list.Add(skillExecutionPlanNode);
			}
		}
		if (list.Count != 0)
		{
			return list.ToArray();
		}
		return Array.Empty<SkillNode>();
	}

	/*
	 * FilterSkillNodeDefinitionsForTarget에 해당하는 값을 찾아 반환한다.
	 */
	public static SkillNodeDefinition[] FilterSkillNodeDefinitionsForTarget(SkillNodeDefinition[] source, string targetSkillId)
	{
		if (source == null || source.Length == 0)
		{
			return Array.Empty<SkillNodeDefinition>();
		}
		if (string.IsNullOrWhiteSpace(targetSkillId))
		{
			return source;
		}
		List<SkillNodeDefinition> list = new List<SkillNodeDefinition>(source.Length);
		foreach (SkillNodeDefinition skillNodeDefinition in source)
		{
			if (skillNodeDefinition != null && skillNodeDefinition.EnabledByDefault && string.Equals(skillNodeDefinition.TargetSkillId, targetSkillId, StringComparison.OrdinalIgnoreCase))
			{
				list.Add(skillNodeDefinition);
			}
		}
		if (list.Count != 0)
		{
			return list.ToArray();
		}
		return Array.Empty<SkillNodeDefinition>();
	}

	/*
	 * HasSkillNodeForTarget 조건을 만족하는지 확인한다.
	 */
	public static bool HasSkillNodeForTarget(SkillNodeDefinition[] source, string targetSkillId)
	{
		if (source == null || source.Length == 0 || string.IsNullOrWhiteSpace(targetSkillId))
		{
			return false;
		}
		foreach (SkillNodeDefinition skillNodeDefinition in source)
		{
			if (skillNodeDefinition != null && skillNodeDefinition.EnabledByDefault && string.Equals(skillNodeDefinition.TargetSkillId, targetSkillId, StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}
		}
		return false;
	}

	/*
	 * CanProcessPlanNode 조건을 만족하는지 확인한다.
	 */
	internal static bool CanProcessPlanNode(string ownerKind, string handlerId)
	{
		if (string.Equals(ownerKind, "Choice", StringComparison.OrdinalIgnoreCase)
			&& SkillChoiceCompiler.UsesChoiceFields(handlerId))
		{
			return true;
		}
		if (string.Equals(ownerKind, "Skill", StringComparison.OrdinalIgnoreCase)
			&& IsSingleBaseFieldHandler(handlerId))
		{
			return true;
		}
		return IsRuntimePlanHandler(handlerId);
	}

	/*
	 * MapSkillNodeDefinition에 필요한 형식으로 변환해 반환한다.
	 */
	private static SkillNode MapSkillNodeDefinition(SkillNodeDefinition node)
	{
		if (node == null || !node.EnabledByDefault)
		{
			return null;
		}
		string text = node.HandlerId;
		if (text == null)
		{
			text = string.Empty;
		}
		if (string.Equals(node.OwnerKind, "Choice", StringComparison.OrdinalIgnoreCase)
			&& SkillChoiceCompiler.UsesChoiceFields(text))
		{
			return null;
		}
		if (string.Equals(node.OwnerKind, "Skill", StringComparison.OrdinalIgnoreCase)
			&& IsSingleBaseFieldHandler(text))
		{
			return null;
		}
		if (string.Equals(text, "TargetHealthRatioCondition", StringComparison.OrdinalIgnoreCase))
		{
			return SkillNode.FromCastCondition(new CastConditionOp(GetFloatParam(node, "threshold", 0f)));
		}
		if (string.Equals(text, "TargetHealthRatioThresholdBonus", StringComparison.OrdinalIgnoreCase))
		{
			return SkillNode.FromCastCondition(new CastConditionOp(GetFloatParam(node, "threshold_bonus", 0f)));
		}
		if (string.Equals(text, "ExecuteDamageMultiplier", StringComparison.OrdinalIgnoreCase))
		{
			return SkillNode.FromDamageModifier(new DamageModifierOp(DamageModifierOpKind.ExecuteMultiplier, GetFloatParam(node, "multiplier", 1f)));
		}
		if (string.Equals(text, "TargetPredicateDamageMultiplier", StringComparison.OrdinalIgnoreCase) && string.Equals(GetParam(node, "predicate"), "is_boss", StringComparison.OrdinalIgnoreCase))
		{
			return SkillNode.FromDamageModifier(new DamageModifierOp(DamageModifierOpKind.BossMultiplier, GetFloatParam(node, "multiplier", 1f)));
		}
		if (string.Equals(text, "BossDamageMultiplier", StringComparison.OrdinalIgnoreCase))
		{
			return SkillNode.FromDamageModifier(new DamageModifierOp(DamageModifierOpKind.BossMultiplier, GetFloatParam(node, "multiplier", 1f)));
		}
		if (string.Equals(text, "ExecuteCritChanceBonus", StringComparison.OrdinalIgnoreCase))
		{
			return SkillNode.FromCritModifier(new CritModifierOp(GetFloatParam(node, "crit_chance_bonus", 0f)));
		}
		if (string.Equals(text, "CooldownReset", StringComparison.OrdinalIgnoreCase) || string.Equals(text, "CooldownResetOnKill", StringComparison.OrdinalIgnoreCase))
		{
			return SkillNode.FromKillAction(new KillActionOp(KillActionOpKind.CooldownReset, 0f, GetBoolParam(node, "requires_execute", defaultValue: false)));
		}
		if (string.Equals(text, "CooldownRefund", StringComparison.OrdinalIgnoreCase))
		{
			return SkillNode.FromKillAction(new KillActionOp(KillActionOpKind.CooldownRefundBonus, GetFloatParam(node, "ratio", 0f), requiresExecute: false));
		}
		if (string.Equals(text, "CooldownRefundBonus", StringComparison.OrdinalIgnoreCase))
		{
			return SkillNode.FromKillAction(new KillActionOp(KillActionOpKind.CooldownRefundBonus, GetFloatParam(node, "ratio_bonus", 0f), requiresExecute: false));
		}
		var skillActionOp = MapSkillActionOp(node, text);
		return SkillNode.FromAction(skillActionOp);
	}

	/*
	 * IsSingleBaseFieldHandler 조건을 만족하는지 확인한다.
	 */
	private static bool IsSingleBaseFieldHandler(string handlerId)
	{
		if (string.Equals(handlerId, "StatusFilteredDeployment", StringComparison.OrdinalIgnoreCase))
		{
			return true;
		}
		return string.Equals(handlerId, "TargetStatusStackDamage", StringComparison.OrdinalIgnoreCase);
	}

	/*
	 * IsRuntimePlanHandler 조건을 만족하는지 확인한다.
	 */
	private static bool IsRuntimePlanHandler(string handlerId)
	{
		if (string.Equals(handlerId, "TargetHealthRatioCondition", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "TargetHealthRatioThresholdBonus", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "ExecuteDamageMultiplier", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "TargetPredicateDamageMultiplier", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "BossDamageMultiplier", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "ExecuteCritChanceBonus", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "CooldownReset", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "CooldownResetOnKill", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "CooldownRefund", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "CooldownRefundBonus", StringComparison.OrdinalIgnoreCase))
		{
			return true;
		}
		if (string.Equals(handlerId, "DamageMultiplier", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "ShieldAmountMultiplier", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "CountStatusDamageMultiplier", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "CooldownMultiplier", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "MagazineBonus", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "ReloadTimeMultiplier", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "PierceBonus", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "RadiusMultiplier", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "RadiusBonus", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "DurationBonus", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "DurationMultiplier", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "DamageDelayMultiplier", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "AdditionalProjectileBonus", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "ShotIntervalMultiplier", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "ConsecutiveHitDamageBonus", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "BranchDamage", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "StatusStackAmountBonus", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "StatusStackAmountSet", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "StatusMaxStacksBonus", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "ConditionalDamageMultiplier", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "TargetStatusStackDamageRateBonus", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "TriggerProcChanceBonus", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "HitTargetCountBonus", StringComparison.OrdinalIgnoreCase))
		{
			return true;
		}
		if (string.Equals(handlerId, "StatusActionSpeedBonus", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "StatusAttackPowerBonus", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "StatusAilmentResistanceBonus", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "StatusDamageBonusRate", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "StatusShieldReceivedBonus", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "StatusCriticalChanceBonus", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "StatusDamageTakenBonus", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "StatusFlatElementResistReduction", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "StatusDurationBonus", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "StatusConditionalDamageTakenBonus", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "StatusElementDamageTakenBonus", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "StatusCriticalDamageTakenBonus", StringComparison.OrdinalIgnoreCase))
		{
			return true;
		}
		return false;
	}

	/*
	 * MapSkillActionOp에 필요한 형식으로 변환해 반환한다.
	 */
	private static SkillActionOp MapSkillActionOp(SkillNodeDefinition node, string handlerId)
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
			string statusId = GetParam(node, "status_id");
			StatusEffectKind statusKind = StatusRuntimeCompiler.ParseStatusKind(statusId);
			return new SkillActionOp(SkillActionOpKind.CountStatusDamageMultiplier, GetFloatParam(node, "amount_per_count", 0f), GetIntParam(node, "max_count", 0), statusId, null, GetEnumParam(node, "target_side", SkillMultiEffectTargetSide.AllAllies), 0f, 0f, statusKind);
		}
		if (string.Equals(handlerId, "CooldownMultiplier", StringComparison.OrdinalIgnoreCase))
		{
			return new SkillActionOp(SkillActionOpKind.CooldownMultiplier, GetFloatParam(node, "multiplier", 1f));
		}
		if (string.Equals(handlerId, "MagazineBonus", StringComparison.OrdinalIgnoreCase))
		{
			return new SkillActionOp(SkillActionOpKind.MagazineBonus, 0f, GetIntParam(node, "bonus", 0));
		}
		if (string.Equals(handlerId, "ReloadTimeMultiplier", StringComparison.OrdinalIgnoreCase))
		{
			return new SkillActionOp(SkillActionOpKind.ReloadTimeMultiplier, GetFloatParam(node, "multiplier", 1f));
		}
		if (string.Equals(handlerId, "PierceBonus", StringComparison.OrdinalIgnoreCase))
		{
			return new SkillActionOp(SkillActionOpKind.PierceBonus, 0f, GetIntParam(node, "bonus", 0));
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
		if (string.Equals(handlerId, "DurationMultiplier", StringComparison.OrdinalIgnoreCase))
		{
			return new SkillActionOp(SkillActionOpKind.DurationMultiplier, GetFloatParam(node, "multiplier", 1f));
		}
		if (string.Equals(handlerId, "DamageDelayMultiplier", StringComparison.OrdinalIgnoreCase))
		{
			return new SkillActionOp(SkillActionOpKind.DamageDelayMultiplier, GetFloatParam(node, "multiplier", 1f));
		}
		if (string.Equals(handlerId, "AdditionalProjectileBonus", StringComparison.OrdinalIgnoreCase))
		{
			return new SkillActionOp(SkillActionOpKind.AdditionalProjectileBonus, 0f, GetIntParam(node, "bonus", 0));
		}
		if (string.Equals(handlerId, "ShotIntervalMultiplier", StringComparison.OrdinalIgnoreCase))
		{
			return new SkillActionOp(SkillActionOpKind.ShotIntervalMultiplier, GetFloatParam(node, "multiplier", 1f));
		}
		if (string.Equals(handlerId, "ConsecutiveHitDamageBonus", StringComparison.OrdinalIgnoreCase))
		{
			return new SkillActionOp(SkillActionOpKind.ConsecutiveHitDamageBonus, GetFloatParam(node, "bonus_rate", 0f), 0, null, null, SkillMultiEffectTargetSide.Enemy, GetFloatParam(node, "max_bonus", 0f));
		}
		if (string.Equals(handlerId, "BranchDamage", StringComparison.OrdinalIgnoreCase))
		{
			return new SkillActionOp(SkillActionOpKind.BranchDamage, GetFloatParam(node, "chance_bonus", 0f), GetIntParam(node, "count", 0), null, null, SkillMultiEffectTargetSide.Enemy, GetFloatParam(node, "damage_multiplier", 0f), GetFloatParam(node, "search_radius", 0f));
		}
		if (string.Equals(handlerId, "StatusStackAmountBonus", StringComparison.OrdinalIgnoreCase))
		{
			return new SkillActionOp(SkillActionOpKind.StatusStackAmountBonus, 0f, GetIntParam(node, "bonus", 0), GetParam(node, "status_id"));
		}
		if (string.Equals(handlerId, "StatusStackAmountSet", StringComparison.OrdinalIgnoreCase))
		{
			return new SkillActionOp(SkillActionOpKind.StatusStackAmountSet, 0f, GetIntParam(node, "value", 0), GetParam(node, "status_id"));
		}
		if (string.Equals(handlerId, "StatusMaxStacksBonus", StringComparison.OrdinalIgnoreCase))
		{
			return new SkillActionOp(SkillActionOpKind.StatusMaxStacksBonus, 0f, GetIntParam(node, "bonus", 0), GetParam(node, "status_id"));
		}
		if (string.Equals(handlerId, "ConditionalDamageMultiplier", StringComparison.OrdinalIgnoreCase))
		{
			string statusId = GetParam(node, "status_id");
			StatusEffectKind statusKind = StatusRuntimeCompiler.ParseStatusKind(statusId);
			return new SkillActionOp(SkillActionOpKind.ConditionalDamageMultiplier, GetFloatParam(node, "multiplier", 1f), GetIntParam(node, "min_stacks", 1), statusId, null, SkillMultiEffectTargetSide.Enemy, 0f, 0f, statusKind);
		}
		if (string.Equals(handlerId, "TargetStatusStackDamageRateBonus", StringComparison.OrdinalIgnoreCase))
		{
			return new SkillActionOp(SkillActionOpKind.TargetStatusStackDamageRateBonus, GetFloatParam(node, "bonus_rate_per_stack", 0f), 0, GetParam(node, "status_id"));
		}
		if (string.Equals(handlerId, "TriggerProcChanceBonus", StringComparison.OrdinalIgnoreCase))
		{
			return new SkillActionOp(SkillActionOpKind.TriggerProcChanceBonus, GetFloatParam(node, "bonus", 0f), 0, GetParam(node, "trigger_id"));
		}
		if (string.Equals(handlerId, "HitTargetCountBonus", StringComparison.OrdinalIgnoreCase))
		{
			return new SkillActionOp(SkillActionOpKind.HitTargetCountBonus, 0f, GetIntParam(node, "bonus", 0));
		}
		if (string.Equals(handlerId, "StatusActionSpeedBonus", StringComparison.OrdinalIgnoreCase))
		{
			return new SkillActionOp(SkillActionOpKind.StatusActionSpeedBonus, GetFloatParam(node, "bonus", 0f), 0, GetParam(node, "status_id"));
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
			return new SkillActionOp(SkillActionOpKind.StatusDurationBonus, GetFloatParam(node, "bonus_seconds", 0f), 0, GetParam(node, "status_id"));
		}
		if (string.Equals(handlerId, "StatusConditionalDamageTakenBonus", StringComparison.OrdinalIgnoreCase))
		{
			var sourceStatusId = GetParam(node, "source_status_id");
			return new SkillActionOp(
				SkillActionOpKind.StatusConditionalDamageTakenBonus,
				GetFloatParam(node, "bonus", 0f),
				0,
				sourceStatusId,
				null,
				SkillMultiEffectTargetSide.Enemy,
				0f,
				0f,
				StatusRuntimeCompiler.ParseStatusKind(sourceStatusId));
		}
		if (string.Equals(handlerId, "StatusElementDamageTakenBonus", StringComparison.OrdinalIgnoreCase))
		{
			return new SkillActionOp(SkillActionOpKind.StatusElementDamageTakenBonus, GetFloatParam(node, "bonus", 0f));
		}
		if (string.Equals(handlerId, "StatusCriticalDamageTakenBonus", StringComparison.OrdinalIgnoreCase))
		{
			return new SkillActionOp(SkillActionOpKind.StatusCriticalDamageTakenBonus, GetFloatParam(node, "bonus", 0f));
		}
		throw new InvalidOperationException("Unsupported skill node handler: " + handlerId);
	}

	/*
	 * GetParam에 해당하는 값을 찾아 반환한다.
	 */
	internal static string GetParam(SkillNodeDefinition node, string key)
	{
		if (node == null || node.Params == null || string.IsNullOrWhiteSpace(key))
		{
			return string.Empty;
		}
		for (int i = 0; i < node.Params.Length; i++)
		{
			SkillNodeParamDefinition skillNodeParamDefinition = node.Params[i];
			if (skillNodeParamDefinition != null && string.Equals(skillNodeParamDefinition.ParamKey, key, StringComparison.OrdinalIgnoreCase))
			{
				if (skillNodeParamDefinition.Value == null)
				{
					return string.Empty;
				}
				return skillNodeParamDefinition.Value;
			}
		}
		return string.Empty;
	}

	/*
	 * GetFloatParam에 해당하는 값을 찾아 반환한다.
	 */
	internal static float GetFloatParam(SkillNodeDefinition node, string key, float defaultValue)
	{
		string param = GetParam(node, key);
		if (string.IsNullOrWhiteSpace(param) || !float.TryParse(param, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
		{
			return defaultValue;
		}
		return result;
	}

	/*
	 * GetIntParam에 해당하는 값을 찾아 반환한다.
	 */
	internal static int GetIntParam(SkillNodeDefinition node, string key, int defaultValue)
	{
		string param = GetParam(node, key);
		if (string.IsNullOrWhiteSpace(param) || !int.TryParse(param, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
		{
			return defaultValue;
		}
		return result;
	}

	/*
	 * GetBoolParam에 해당하는 값을 찾아 반환한다.
	 */
	internal static bool GetBoolParam(SkillNodeDefinition node, string key, bool defaultValue)
	{
		string param = GetParam(node, key);
		if (string.IsNullOrWhiteSpace(param))
		{
			return defaultValue;
		}
		if (bool.TryParse(param, out var result))
		{
			return result;
		}
		if (!string.Equals(param, "1", StringComparison.OrdinalIgnoreCase) && !string.Equals(param, "yes", StringComparison.OrdinalIgnoreCase))
		{
			return string.Equals(param, "y", StringComparison.OrdinalIgnoreCase);
		}
		return true;
	}

	/*
	 * GetEnumParam에 해당하는 값을 찾아 반환한다.
	 */
	internal static T GetEnumParam<T>(SkillNodeDefinition node, string key, T defaultValue) where T : struct
	{
		string param = GetParam(node, key);
		if (string.IsNullOrWhiteSpace(param) || !Enum.TryParse<T>(param, ignoreCase: true, out var result))
		{
			return defaultValue;
		}
		return result;
	}
    }
}


/*
 * Enhancement와 Master 선택지를 전투용 데이터와 실행 노드로 변환한다.
 * 스킬 전체 변환을 조율하는 SkillRuntimeCompiler와 달리 선택지 필드 변환만 담당한다.
 */
namespace Pakuri.InGame
{
    internal static class SkillChoiceCompiler
    {
	/*
	 * Compile 작업 결과를 반환한다.
	 */
	internal static SkillChoiceRuntimeData[] Compile(SkillChoiceDefinition[] source)
	{
		SkillChoiceRuntimeData[] array = new SkillChoiceRuntimeData[source.Length];
		for (int i = 0; i < source.Length; i++)
		{
			SkillChoiceDefinition skillChoiceDefinition = source[i];
			if (!string.IsNullOrWhiteSpace(skillChoiceDefinition.CountStatusId))
			{
				skillChoiceDefinition.CountStatusKind = StatusRuntimeCompiler.ParseStatusKind(
					skillChoiceDefinition.CountStatusId);
			}
			if (!string.IsNullOrWhiteSpace(skillChoiceDefinition.ConditionalCritTargetStatusId))
			{
				skillChoiceDefinition.ConditionalCritTargetStatusKind = StatusRuntimeCompiler.ParseStatusKind(
					skillChoiceDefinition.ConditionalCritTargetStatusId);
			}
			if (!string.IsNullOrWhiteSpace(skillChoiceDefinition.RequiredSourceStatusId))
			{
				skillChoiceDefinition.RequiredSourceStatusKind = StatusRuntimeCompiler.ParseStatusKind(
					skillChoiceDefinition.RequiredSourceStatusId);
			}
			array[i] = new SkillChoiceRuntimeData
			{
				Source = skillChoiceDefinition
			};
		}
		return array;
	}

	/*
	 * ApplyChoiceFieldNodes 처리를 대상에 적용한다.
	 */
	internal static void ApplyChoiceFieldNodes(SkillChoiceRuntimeData spec, SkillNodeDefinition[] nodes)
	{
		if (spec == null || nodes == null || nodes.Length == 0)
		{
			return;
		}
		foreach (SkillNodeDefinition skillNodeDefinition in nodes)
		{
			if (skillNodeDefinition != null && skillNodeDefinition.EnabledByDefault && UsesChoiceFields(skillNodeDefinition.HandlerId))
			{
				ApplyNormalizedChoiceNode(spec, skillNodeDefinition);
			}
		}
	}

	/*
	 * UsesChoiceFields 조건을 만족하는지 확인한다.
	 */
	internal static bool UsesChoiceFields(string handlerId)
	{
		if (string.Equals(handlerId, "BurstDamageRule", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "FollowUpProjectile", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "ThresholdApplyStatus", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "TargetStatusStackDamageMultiplier", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "ConsumeTargetStatusRatioOverride", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "BurstStatusStacksBonus", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "RepeatPerTarget", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "TargetStatusCritBonus", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "RedistributeConsumedStatus", StringComparison.OrdinalIgnoreCase))
		{
			return true;
		}
		if (string.Equals(handlerId, "AdditionalDamage", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "BeamWidthBonus", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "CoreAdditionalDamage", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "CoreDamageMultiplier", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "CritChanceBonus", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "CritDamageBonus", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "EveryNthHitChainDamage", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "HitCountCooldownRefund", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "KnockbackDistanceMultiplier", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "ReloadReducePerHit", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "RequiredSourceStatus", StringComparison.OrdinalIgnoreCase))
		{
			return true;
		}
		return false;
	}

	/*
	 * ApplyNormalizedChoiceNode 처리를 대상에 적용한다.
	 */
	private static void ApplyNormalizedChoiceNode(SkillChoiceRuntimeData spec, SkillNodeDefinition node)
	{
		SkillChoiceDefinition source = spec.Source;
		string a = node.HandlerId;
		if (a == null)
		{
			a = string.Empty;
		}
		if (string.Equals(a, "DamageMultiplier", StringComparison.OrdinalIgnoreCase))
		{
			source.HasDamageMultiplier = true;
			source.DamageMultiplier *= SkillNodeMapper.GetFloatParam(node, "multiplier", 1f);
		}
		else if (string.Equals(a, "ShieldAmountMultiplier", StringComparison.OrdinalIgnoreCase))
		{
			spec.HasShieldAmountMultiplier = true;
			spec.ShieldAmountMultiplier *= SkillNodeMapper.GetFloatParam(node, "multiplier", 1f);
		}
		else if (string.Equals(a, "CountStatusDamageMultiplier", StringComparison.OrdinalIgnoreCase))
		{
			source.CountStatusId = SkillNodeMapper.GetParam(node, "status_id");
			source.CountStatusKind = StatusRuntimeCompiler.ParseStatusKind(source.CountStatusId);
			source.CountTargetSide = SkillNodeMapper.GetEnumParam(node, "target_side", SkillMultiEffectTargetSide.AllAllies);
			source.DamageMultiplierPerCount += SkillNodeMapper.GetFloatParam(node, "amount_per_count", 0f);
			source.CountMax = SkillNodeMapper.GetIntParam(node, "max_count", source.CountMax);
		}
		else if (string.Equals(a, "CooldownMultiplier", StringComparison.OrdinalIgnoreCase))
		{
			source.HasCooldownMultiplier = true;
			source.CooldownMultiplier *= SkillNodeMapper.GetFloatParam(node, "multiplier", 1f);
		}
		else if (string.Equals(a, "CritChanceBonus", StringComparison.OrdinalIgnoreCase))
		{
			source.CritChanceBonus += SkillNodeMapper.GetFloatParam(node, "bonus", 0f);
		}
		else if (string.Equals(a, "CritDamageBonus", StringComparison.OrdinalIgnoreCase))
		{
			source.CritDamageBonus += SkillNodeMapper.GetFloatParam(node, "bonus", 0f);
		}
		else if (string.Equals(a, "MagazineBonus", StringComparison.OrdinalIgnoreCase))
		{
			source.HasMagazineBonus = true;
			source.MagazineBonus += SkillNodeMapper.GetIntParam(node, "bonus", 0);
		}
		else if (string.Equals(a, "ReloadTimeMultiplier", StringComparison.OrdinalIgnoreCase))
		{
			source.HasReloadTimeMultiplier = true;
			source.ReloadTimeMultiplier *= SkillNodeMapper.GetFloatParam(node, "multiplier", 1f);
		}
		else if (string.Equals(a, "PierceBonus", StringComparison.OrdinalIgnoreCase))
		{
			source.PierceBonus += SkillNodeMapper.GetIntParam(node, "bonus", 0);
		}
		else if (string.Equals(a, "HitTargetCountBonus", StringComparison.OrdinalIgnoreCase))
		{
			source.HitTargetCountBonus += SkillNodeMapper.GetIntParam(node, "bonus", 0);
		}
		else if (string.Equals(a, "RadiusMultiplier", StringComparison.OrdinalIgnoreCase))
		{
			source.HasRadiusMultiplier = true;
			source.RadiusMultiplier *= SkillNodeMapper.GetFloatParam(node, "multiplier", 1f);
		}
		else if (string.Equals(a, "RadiusBonus", StringComparison.OrdinalIgnoreCase))
		{
			source.RadiusBonus += SkillNodeMapper.GetFloatParam(node, "bonus", 0f);
		}
		else if (string.Equals(a, "BeamWidthBonus", StringComparison.OrdinalIgnoreCase))
		{
			source.BeamWidthBonus += SkillNodeMapper.GetFloatParam(node, "bonus", 0f);
		}
		else if (string.Equals(a, "KnockbackDistanceMultiplier", StringComparison.OrdinalIgnoreCase))
		{
			source.HasKnockbackDistanceMultiplier = true;
			source.KnockbackDistanceMultiplier *= SkillNodeMapper.GetFloatParam(node, "multiplier", 1f);
		}
		else if (string.Equals(a, "ReloadReducePerHit", StringComparison.OrdinalIgnoreCase))
		{
			source.ReloadReduceTargetSkillId = SkillNodeMapper.GetParam(node, "target_skill_id");
			source.ReloadReduceSecondsPerHit += SkillNodeMapper.GetFloatParam(node, "seconds_per_hit", 0f);
		}
		else if (string.Equals(a, "CoreDamageMultiplier", StringComparison.OrdinalIgnoreCase))
		{
			source.CoreHitboxName = SkillNodeMapper.GetParam(node, "hitbox_name");
			source.HasCoreDamageMultiplier = true;
			source.CoreDamageMultiplier *= SkillNodeMapper.GetFloatParam(node, "multiplier", 1f);
		}
		else if (string.Equals(a, "CoreAdditionalDamage", StringComparison.OrdinalIgnoreCase))
		{
			source.CoreHitboxName = SkillNodeMapper.GetParam(node, "hitbox_name");
			source.HasCoreOnHitAdditionalDamage = true;
			source.CoreOnHitAdditionalDamageChance = SkillNodeMapper.GetFloatParam(node, "chance", 1f);
			source.CoreOnHitAdditionalDamageMultiplier = SkillNodeMapper.GetFloatParam(node, "multiplier", 1f);
			source.CoreOnHitAdditionalDamageAttribute = SkillNodeMapper.GetEnumParam(node, "attribute", DamageAttribute.Physical);
		}
		else if (string.Equals(a, "HitCountCooldownRefund", StringComparison.OrdinalIgnoreCase))
		{
			source.HitCountCooldownRefundTargetSkillId = SkillNodeMapper.GetParam(node, "target_skill_id");
			source.HitCountCooldownRefundMinTargets = SkillNodeMapper.GetIntParam(node, "min_targets", 0);
			source.HitCountCooldownRefundRatio = SkillNodeMapper.GetFloatParam(node, "ratio", 0f);
		}
		else if (string.Equals(a, "DurationBonus", StringComparison.OrdinalIgnoreCase))
		{
			source.DurationBonus += SkillNodeMapper.GetFloatParam(node, "bonus_seconds", 0f);
		}
		else if (string.Equals(a, "DamageDelayMultiplier", StringComparison.OrdinalIgnoreCase))
		{
			source.HasDamageDelayMultiplier = true;
			source.DamageDelayMultiplier *= SkillNodeMapper.GetFloatParam(node, "multiplier", 1f);
		}
		else if (string.Equals(a, "ConsecutiveHitDamageBonus", StringComparison.OrdinalIgnoreCase))
		{
			source.ConsecutiveHitBonusRate += SkillNodeMapper.GetFloatParam(node, "bonus_rate", 0f);
			source.ConsecutiveHitMax += SkillNodeMapper.GetFloatParam(node, "max_bonus", 0f);
		}
		else if (string.Equals(a, "BurstDamageRule", StringComparison.OrdinalIgnoreCase))
		{
			source.HasBurstDamageProjectileIndex = true;
			source.BurstDamageProjectileIndex = SkillNodeMapper.GetIntParam(node, "projectile_index", 0);
			source.HasBurstDamageMultiplier = true;
			source.BurstDamageMultiplier = SkillNodeMapper.GetFloatParam(node, "multiplier", 1f);
		}
		else if (string.Equals(a, "FollowUpProjectile", StringComparison.OrdinalIgnoreCase))
		{
			source.FollowUpProjectileCount = SkillNodeMapper.GetIntParam(node, "count", 0);
			source.FollowUpProjectileDelaySeconds = SkillNodeMapper.GetFloatParam(node, "delay_seconds", 0f);
			source.FollowUpProjectileDamageMultiplier = SkillNodeMapper.GetFloatParam(node, "damage_multiplier", 1f);
		}
		else if (string.Equals(a, "ThresholdApplyStatus", StringComparison.OrdinalIgnoreCase))
		{
			source.ThresholdStatusId = SkillNodeMapper.GetParam(node, "source_status_id");
			source.ThresholdStatusKind = StatusRuntimeCompiler.ParseStatusKind(source.ThresholdStatusId);
			source.ThresholdStatusMinStacks = SkillNodeMapper.GetIntParam(node, "min_stacks", 0);
			source.ThresholdApplyStatusId = SkillNodeMapper.GetParam(node, "apply_status_id");
			source.ThresholdApplyStatusKind = StatusRuntimeCompiler.ParseStatusKind(source.ThresholdApplyStatusId);
		}
		else if (string.Equals(a, "TargetStatusStackDamageMultiplier", StringComparison.OrdinalIgnoreCase))
		{
			source.HasTargetStatusStackDamageMultiplier = true;
			source.TargetStatusStackDamageMultiplier = SkillNodeMapper.GetFloatParam(node, "multiplier", 1f);
		}
		else if (string.Equals(a, "ConsumeTargetStatusRatioOverride", StringComparison.OrdinalIgnoreCase))
		{
			source.HasConsumeTargetStatusRatioOverride = true;
			source.ConsumeTargetStatusRatioOverride = SkillNodeMapper.GetFloatParam(node, "ratio", 0f);
		}
		else if (string.Equals(a, "BurstStatusStacksBonus", StringComparison.OrdinalIgnoreCase))
		{
			source.HasBurstStatusProjectileIndex = true;
			source.BurstStatusProjectileIndex = SkillNodeMapper.GetIntParam(node, "projectile_index", 0);
			source.BurstStatusStacksBonus = SkillNodeMapper.GetIntParam(node, "bonus", 0);
		}
		else if (string.Equals(a, "StatusActionSpeedBonus", StringComparison.OrdinalIgnoreCase))
		{
			source.HasStatusActionSpeedBonus = true;
			spec.StatusActionSpeedBonusStatusId = SkillNodeMapper.GetParam(node, "status_id");
			source.StatusActionSpeedBonus += SkillNodeMapper.GetFloatParam(node, "bonus", 0f);
		}
		else if (string.Equals(a, "StatusAttackPowerBonus", StringComparison.OrdinalIgnoreCase))
		{
			source.HasStatusAttackPowerBonus = true;
			source.StatusAttackPowerBonus += SkillNodeMapper.GetFloatParam(node, "bonus", 0f);
		}
		else if (string.Equals(a, "StatusAilmentResistanceBonus", StringComparison.OrdinalIgnoreCase))
		{
			source.HasStatusAilmentResistanceBonus = true;
			source.StatusAilmentResistanceBonus += SkillNodeMapper.GetFloatParam(node, "bonus", 0f);
		}
		else if (string.Equals(a, "StatusDamageBonusRate", StringComparison.OrdinalIgnoreCase))
		{
			spec.HasStatusDamageBonusRate = true;
			spec.StatusDamageBonusRate += SkillNodeMapper.GetFloatParam(node, "bonus", 0f);
		}
		else if (string.Equals(a, "StatusShieldReceivedBonus", StringComparison.OrdinalIgnoreCase))
		{
			spec.HasStatusShieldReceivedBonus = true;
			spec.StatusShieldReceivedBonus += SkillNodeMapper.GetFloatParam(node, "bonus", 0f);
		}
		else if (string.Equals(a, "StatusCriticalChanceBonus", StringComparison.OrdinalIgnoreCase))
		{
			spec.HasStatusCriticalChanceBonus = true;
			spec.StatusCriticalChanceBonus += SkillNodeMapper.GetFloatParam(node, "bonus", 0f);
		}
		else if (string.Equals(a, "StatusDamageTakenBonus", StringComparison.OrdinalIgnoreCase))
		{
			spec.HasStatusDamageTakenBonus = true;
			spec.StatusDamageTakenBonus += SkillNodeMapper.GetFloatParam(node, "bonus", 0f);
		}
		else if (string.Equals(a, "StatusFlatElementResistReduction", StringComparison.OrdinalIgnoreCase))
		{
			spec.HasStatusFlatElementResistReduction = true;
			spec.StatusFlatElementResistReduction += SkillNodeMapper.GetFloatParam(node, "bonus", 0f);
		}
		else if (string.Equals(a, "StatusDurationBonus", StringComparison.OrdinalIgnoreCase))
		{
			source.StatusDurationBonusStatusId = SkillNodeMapper.GetParam(node, "status_id");
			source.StatusDurationBonus += SkillNodeMapper.GetFloatParam(node, "bonus_seconds", 0f);
		}
		else if (string.Equals(a, "StatusConditionalDamageTakenBonus", StringComparison.OrdinalIgnoreCase))
		{
			source.HasStatusConditionalDamageTakenBonus = true;
			source.StatusConditionalSourceStatusId = SkillNodeMapper.GetParam(node, "source_status_id");
			source.StatusConditionalSourceStatusKind = StatusRuntimeCompiler.ParseStatusKind(
				source.StatusConditionalSourceStatusId);
			source.StatusConditionalDamageTakenBonus += SkillNodeMapper.GetFloatParam(node, "bonus", 0f);
		}
		else if (string.Equals(a, "StatusElementDamageTakenBonus", StringComparison.OrdinalIgnoreCase))
		{
			source.HasStatusElementDamageTakenBonus = true;
			source.StatusElementDamageTakenBonus += SkillNodeMapper.GetFloatParam(node, "bonus", 0f);
		}
		else if (string.Equals(a, "StatusCriticalDamageTakenBonus", StringComparison.OrdinalIgnoreCase))
		{
			source.HasStatusCriticalDamageTakenBonus = true;
			source.StatusCriticalDamageTakenBonus += SkillNodeMapper.GetFloatParam(node, "bonus", 0f);
		}
		else if (string.Equals(a, "AdditionalDamage", StringComparison.OrdinalIgnoreCase))
		{
			source.HasOnHitAdditionalDamage = true;
			source.OnHitAdditionalDamageChance = SkillNodeMapper.GetFloatParam(node, "chance", 1f);
			source.OnHitAdditionalDamageMultiplier = SkillNodeMapper.GetFloatParam(node, "multiplier", 1f);
			source.OnHitAdditionalDamageAttribute = SkillNodeMapper.GetEnumParam(node, "attribute", DamageAttribute.Physical);
			string param = SkillNodeMapper.GetParam(node, "target");
			source.OnHitAdditionalDamageTarget = param;
		if (string.IsNullOrWhiteSpace(source.OnHitAdditionalDamageTarget))
		{
			source.OnHitAdditionalDamageTarget = SkillNodeMapper.GetParam(node, "target_side");
		}
		}
		else if (string.Equals(a, "EveryNthHitChainDamage", StringComparison.OrdinalIgnoreCase))
		{
			source.OnHitChainHitPeriod = SkillNodeMapper.GetIntParam(node, "hit_count", 0);
			source.OnHitChainTargetCount = SkillNodeMapper.GetIntParam(node, "max_targets", source.OnHitChainTargetCount);
			source.OnHitChainSearchRadius = SkillNodeMapper.GetFloatParam(node, "radius", source.OnHitChainSearchRadius);
			source.OnHitChainDamageMultiplier = SkillNodeMapper.GetFloatParam(node, "multiplier", 1f);
			source.OnHitChainDamageAttribute = SkillNodeMapper.GetEnumParam(node, "attribute", DamageAttribute.Physical);
		}
		else if (string.Equals(a, "RepeatPerTarget", StringComparison.OrdinalIgnoreCase))
		{
			source.RepeatCountPerTarget = SkillNodeMapper.GetIntParam(node, "repeat_count", 0);
			source.RepeatIntervalSeconds = SkillNodeMapper.GetFloatParam(node, "repeat_interval_seconds", 0f);
			source.RepeatDamageMultiplier = SkillNodeMapper.GetFloatParam(node, "repeat_damage_multiplier", 1f);
		}
		else if (string.Equals(a, "TargetStatusCritBonus", StringComparison.OrdinalIgnoreCase))
		{
			source.ConditionalCritChanceBonus += SkillNodeMapper.GetFloatParam(node, "crit_chance_bonus", 0f);
			source.ConditionalCritTargetStatusId = SkillNodeMapper.GetParam(node, "status_id");
			source.ConditionalCritTargetStatusKind = StatusRuntimeCompiler.ParseStatusKind(
				source.ConditionalCritTargetStatusId);
			source.ConditionalCritTargetStatusMinStacks = SkillNodeMapper.GetIntParam(node, "min_stacks", 0);
		}
		else if (string.Equals(a, "RedistributeConsumedStatus", StringComparison.OrdinalIgnoreCase))
		{
			source.RedistributeConsumedStatusRatioOnKill = SkillNodeMapper.GetFloatParam(node, "ratio", 0f);
			source.RedistributeConsumedStatusId = SkillNodeMapper.GetParam(node, "status_id");
			source.RedistributeConsumedStatusKind = StatusRuntimeCompiler.ParseStatusKind(
				source.RedistributeConsumedStatusId);
			source.RedistributeConsumedStatusSearchRadius = SkillNodeMapper.GetFloatParam(node, "radius", 0f);
			source.RedistributeConsumedStatusTargetCount = SkillNodeMapper.GetIntParam(node, "target_count", 0);
		}
		else if (string.Equals(a, "RequiredSourceStatus", StringComparison.OrdinalIgnoreCase))
		{
			source.RequiredSourceStatusId = SkillNodeMapper.GetParam(node, "status_id");
			source.RequiredSourceStatusKind = StatusRuntimeCompiler.ParseStatusKind(source.RequiredSourceStatusId);
			source.RequiredSourceStatusMinStacks = SkillNodeMapper.GetIntParam(node, "min_stacks", 1);
		}
	}
    }
}
