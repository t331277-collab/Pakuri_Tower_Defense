/*
 * 역할: 스킬 런타임 변환.
 * 책임: 액티브·패시브·적·Trigger·대상·전달·비주얼 행을 스킬 정의로 변환한다.
 */

using System;
using System.Collections.Generic;
using System.Globalization;
using Pakuri.Combat;
using Pakuri.InGame;
using UnityEngine;

namespace Pakuri.Data
{

/// <summary><c>ActiveSkillBuildData</c>가 나타내는 런타임 값을 보관한다.</summary>
internal sealed class ActiveSkillBuildData
{
	public string SkillId;
	public string DisplayName;
	public SkillSlot Slot;
	public SkillRuntimeKind RuntimeKind;
	public SkillImplementationState ImplementationState = SkillImplementationState.DataOnly;
	public bool IsDefaultLearned;
	public Sprite SkillIcon;
	public GameObject SkillEffectPrefab;
	public RuntimeSkillVisualSpec RuntimeVisual = new RuntimeSkillVisualSpec();
	public RuntimeSkillVisualSpec ImpactRuntimeVisual = new RuntimeSkillVisualSpec();
	public string DescriptionText;
	public DamageAttribute Attribute;
	public float BaseDamage;
	public float AttackPowerCoefficient;
	public float SpellPowerCoefficient;
	public float Radius;
	public float LineLength;
	public int CastRepeatCount = 1;
	public float CastRepeatIntervalSeconds;
	public float CastRange;
	public float EffectRadius;
	public string TargetScope;
	public string ExecutionProfile;
	public float FlatValue;
	public float ProjectileLifetimeSeconds;
	public float IncomingDamageMultiplier = 1f;
	public float MoveSpeedMultiplier = 1f;
	public float OutgoingDamageMultiplier = 1f;
	public float ChainDamageMultiplier;
	public float ChainDelaySeconds;
	public float ChainRadius;
	public bool ExcludePrimaryTarget;
	public float TargetMaxHealthRatio;
	public float ChargeRampSeconds = 3f;
	public float ChargeMoveSpeedMultiplier = 2.5f;
	public float KnockbackDistance;
	public float DamageDelaySeconds;
	public float ExecuteHealthRatioThreshold;
	public bool RequireExecuteThresholdToCast;
	public float ExecuteDamageMultiplier = 1f;
	public float KillCooldownRefundRatio;
	public float BossDamageMultiplier = 1f;
	public string HitTargetCount;
	public bool UsePrefabHitbox;
	public string TargetSelection;
	public string TargetSelectionStatusId;
	public int TargetSelectionStatusMinStacks;
	public float CooldownSeconds;
	public float ActiveDurationSeconds;
	public int MagazineCapacity;
	public float ReloadSeconds;
	public float ShotIntervalSeconds;
	public float BurstIntervalSeconds;
	public int ProjectileBurstCount;
	public int BurstDamageProjectileIndex;
	public float BurstDamageMultiplier = 1f;
	public float ProjectileSpeed;
	public int PierceCount;
	public bool CriticalAllowed = true;
	public string DeploymentRequiredTargetStatusId;
	public int DeploymentRequiredTargetStatusMinStacks;
	public string TargetStatusStackStatusId;
	public int TargetStatusStackMaxStacks;
	public float TargetStatusStackBaseDamage;
	public float TargetStatusStackAttackPowerCoefficient;
	public float TargetStatusStackSpellPowerCoefficient;
	public string ConsumeTargetStatusId;
	public float ConsumeTargetStatusRatio;
	public int ConsumeTargetStatusStacks;
	public string StatusEffectId;
	public float StatusChance;
	public string StatusEffectLabel;
	public GameObject StatusEffectPrefab;
	public float StatusDurationSeconds;
	public int StatusMaxStacks;
	public int StatusStackAmount;
	public string StatusTargetScope;
	public string StatusMergePolicy;
	public string ShieldAmountRefreshPolicy;
	public float StatusActionSpeedBonus;
	public float StatusMoveSpeedBonus;
	public float StatusAttackPowerBonus;
	public float StatusDamageBonusRate;
	public bool StatusPermanent;
	public float StatusDamageTakenBonus;
	public float StatusCriticalDamageTakenBonus;
	public float StatusCriticalDamageBonus;
	public float StatusAilmentResistanceBonus;
	public float StatusCriticalResistanceBonus;
	public float StatusElementResistReduction;
	public float StatusFlatElementResistReduction;
	public float StatusElementDamageTakenBonus;
	public string Summary;
	public SkillChoiceBuildData[] EnhancementChoices = Array.Empty<SkillChoiceBuildData>();
	public SkillChoiceBuildData[] MasterSkillChoices = Array.Empty<SkillChoiceBuildData>();
	public SkillNodeBuildData[] Nodes = Array.Empty<SkillNodeBuildData>();
}

/// <summary><c>PassiveSkillBuildData</c>가 나타내는 런타임 값을 보관한다.</summary>
internal sealed class PassiveSkillBuildData
{
	public string PassiveId;
	public string DisplayName;
	public SkillSlot Slot;
	public SkillSlot RequiredActiveSlot;
	public bool IsAvailableWithoutActiveRequirement;
	public SkillImplementationState ImplementationState = SkillImplementationState.DataOnly;
	public Sprite SkillIcon;
	public string DescriptionText;
	public string Summary;
	public SkillChoiceBuildData[] BaseModifierChoices = Array.Empty<SkillChoiceBuildData>();
	public SkillChoiceBuildData[] EnhancementChoices = Array.Empty<SkillChoiceBuildData>();
	public SkillNodeBuildData[] Nodes = Array.Empty<SkillNodeBuildData>();
}

/// <summary><c>SkillChoiceBuildData</c>가 나타내는 런타임 값을 보관한다.</summary>
internal sealed class SkillChoiceBuildData
{
	public string ChoiceId;
	public string MonsterId;
	public string SkillId;
	public string TargetSkillId;
	public SkillChoiceGroup ChoiceGroup;
	public string Title;
	public Sprite SkillIcon;
	public GameObject SkillEffectPrefab;
	public string DescriptionText;
	public SkillNodeBuildData[] Nodes = Array.Empty<SkillNodeBuildData>();
}

/// <summary><c>GameDataCatalogBuilder</c> 런타임 데이터를 파싱된 저작 데이터에서 생성한다.</summary>
internal sealed partial class GameDataCatalogBuilder
{

	/// <summary>전달된 런타임 입력값을 사용해 <c>ActiveDefinition</c>를 구성한다.</summary>
	private static SkillDefinition BuildActiveDefinition(
		string ownerId,
		ActiveSkillBuildData source,
		SkillTriggerDefinition[] triggers,
		StatusEffectDefinition[] statusDefinitions)
	{
		SkillDefinition skillRuntimeData = CreateConcreteActiveSkill(source);
		MapCommonFields(skillRuntimeData, ownerId, source, triggers);
		MapActiveFields(skillRuntimeData, null, source, statusDefinitions);
		return skillRuntimeData;
	}

	/// <summary>전달된 런타임 입력값을 사용해 <c>PassiveDefinition</c>를 구성한다.</summary>
	private static PassiveSkillDefinition BuildPassiveDefinition(MonsterDefinition monster, PassiveSkillBuildData source)
	{
		PassiveSkillDefinition passiveSkillExecutionDefinition = CreateRuntimeData<PassiveSkillDefinition>();
		passiveSkillExecutionDefinition.SkillId = source.PassiveId;
		passiveSkillExecutionDefinition.SkillName = source.DisplayName;
		passiveSkillExecutionDefinition.Slot = source.Slot;
		passiveSkillExecutionDefinition.RuntimeKind = SkillRuntimeKind.Passive;
		passiveSkillExecutionDefinition.ImplementationState = source.ImplementationState;
		passiveSkillExecutionDefinition.RequiredActiveSlot = source.RequiredActiveSlot;
		passiveSkillExecutionDefinition.IsAvailableWithoutActiveRequirement = source.IsAvailableWithoutActiveRequirement;
		passiveSkillExecutionDefinition.IsActive = false;
		passiveSkillExecutionDefinition.Element = DamageAttribute.Physical;
		if (monster != null)
		{
			passiveSkillExecutionDefinition.Element = monster.PrimaryAttribute;
		}
		passiveSkillExecutionDefinition.Description = source.DescriptionText;
		passiveSkillExecutionDefinition.Summary = source.Summary;
		passiveSkillExecutionDefinition.Icon = source.SkillIcon;
		passiveSkillExecutionDefinition.BaseModifierChoices = BuildChoices(source.BaseModifierChoices);
		passiveSkillExecutionDefinition.EnhancementChoices = BuildChoices(source.EnhancementChoices);
		passiveSkillExecutionDefinition.MasterChoices = Array.Empty<SkillChoice>();
		SkillTriggerDefinition[] triggers = null;
		if (monster != null)
		{
			triggers = monster.SkillTriggers;
		}
		passiveSkillExecutionDefinition.SkillTriggers = FilterSkillTriggersForSkill(triggers, source.PassiveId);
		passiveSkillExecutionDefinition.Nodes = MapSkillNodes(source.Nodes);
		return passiveSkillExecutionDefinition;
	}

	/// <summary>전달된 <c>source</c> 값을 사용해 <c>Choices</c>를 구성한다.</summary>
	private static SkillChoice[] BuildChoices(SkillChoiceBuildData[] source)
	{
		var choices = new SkillChoice[source.Length];
		for (var i = 0; i < source.Length; i++)
		{
			var choice = source[i];
			choices[i] = new SkillChoice
			{
				ChoiceId = choice.ChoiceId,
				MonsterId = choice.MonsterId,
				SkillId = choice.SkillId,
				TargetSkillId = choice.TargetSkillId,
				ChoiceGroup = choice.ChoiceGroup,
				Title = choice.Title,
				SkillIcon = choice.SkillIcon,
				SkillEffectPrefab = choice.SkillEffectPrefab,
				DescriptionText = choice.DescriptionText,
				Nodes = MapSkillNodes(choice.Nodes)
			};
		}
		return choices;
	}

	/// <summary>전달된 <c>source</c> 값을 사용해 <c>ConcreteActiveSkill</c>를 생성한다.</summary>
	private static SkillDefinition CreateConcreteActiveSkill(ActiveSkillBuildData source)
	{
		if (MatchesProfile(source, "DamageArea"))
		{
			return CreateRuntimeData<SingleSkillDefinition>();
		}
		if (MatchesProfile(source, "DamageThenDelayedChain"))
		{
			return CreateRuntimeData<SingleChainSkillDefinition>();
		}
		if (MatchesProfile(source, "ChargeDamageStatus"))
		{
			return CreateRuntimeData<SingleChargeSkillDefinition>();
		}
		if (source.RuntimeKind == SkillRuntimeKind.Heal)
		{
			return CreateRuntimeData<BuffHealSkillDefinition>();
		}
		if (MatchesProfile(source, "ApplySelfIncomingDamageMultiplier"))
		{
			return CreateRuntimeData<BuffSkillDefinition>();
		}
		switch (source.RuntimeKind)
		{
		case SkillRuntimeKind.MagazineProjectile:
		case SkillRuntimeKind.CooldownProjectile:
			return CreateRuntimeData<ProjectileSkillDefinition>();
		case SkillRuntimeKind.LineAttack:
			return CreateRuntimeData<LineSkillDefinition>();
		case SkillRuntimeKind.SingleAttack:
		case SkillRuntimeKind.Mark:
		case SkillRuntimeKind.Execute:
			return CreateRuntimeData<SingleSkillDefinition>();
		case SkillRuntimeKind.AreaAttack:
		case SkillRuntimeKind.Field:
			return CreateRuntimeData<ZoneSkillDefinition>();
		case SkillRuntimeKind.Buff:
			return CreateRuntimeData<BuffSkillDefinition>();
		case SkillRuntimeKind.Shield:
			return CreateRuntimeData<BuffShieldSkillDefinition>();
		default:
			throw new InvalidOperationException("Unsupported active skill runtime kind: " + source.RuntimeKind);
		}
	}

	/// <summary><c>RuntimeData</c>를 생성한다.</summary>
	private static T CreateRuntimeData<T>() where T : SkillDefinition, new()
	{
		return new T();
	}

	/// <summary>전달된 런타임 입력값을 사용해 <c>CommonFields</c>를 대응시킨다.</summary>
	private static void MapCommonFields(SkillDefinition skill, string monsterId, ActiveSkillBuildData source, SkillTriggerDefinition[] monsterTriggers = null)
	{
		skill.SkillId = source.SkillId;
		skill.SkillName = source.DisplayName;
		skill.Slot = source.Slot;
		skill.RuntimeKind = source.RuntimeKind;
		skill.ImplementationState = source.ImplementationState;
		skill.IsDefaultLearned = source.IsDefaultLearned;
		skill.IsActive = source.RuntimeKind != SkillRuntimeKind.Passive;
		skill.Element = source.Attribute;
		skill.Description = source.DescriptionText;
		skill.Summary = source.Summary;
		skill.Icon = source.SkillIcon;
		skill.SkillEffectPrefab = source.SkillEffectPrefab;
		skill.RuntimeVisual = source.RuntimeVisual;
		skill.EnhancementChoices = BuildChoices(source.EnhancementChoices);
		skill.MasterChoices = BuildChoices(source.MasterSkillChoices);
		skill.SkillTriggers = FilterSkillTriggersForSkill(monsterTriggers, source.SkillId);
		skill.Nodes = MapSkillNodes(source.Nodes);
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
			skill.Targeting.SelectionStatusKind = StatusValueParser.ParseStatusKind(
				source.TargetSelectionStatusId);
		}
		skill.Targeting.SelectionStatusMinStacks = Mathf.Max(0, source.TargetSelectionStatusMinStacks);
		skill.Targeting.Shape = MapShape(source.RuntimeKind);
		skill.Targeting.CoverAll = source.RuntimeKind == SkillRuntimeKind.SingleAttack && source.Radius <= 0f && string.IsNullOrWhiteSpace(source.TargetSelection);
	}

	/// <summary>전달된 런타임 입력값을 사용해 <c>FilterSkillTriggersForSkill</c> 결과값을 생성해 반환한다.</summary>
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

	/// <summary>전달된 런타임 입력값을 사용해 <c>TriggerOwnedBySkill</c> 조건 충족 여부를 반환한다.</summary>
	private static bool IsTriggerOwnedBySkill(SkillTriggerDefinition trigger, string skillId)
	{
		if (trigger != null && !string.IsNullOrWhiteSpace(trigger.SourceSkillId))
		{
			return string.Equals(trigger.SourceSkillId, skillId, StringComparison.OrdinalIgnoreCase);
		}
		return false;
	}

	/// <summary>전달된 런타임 입력값을 사용해 <c>ActiveFields</c>를 대응시킨다.</summary>
	private static void MapActiveFields(
		SkillDefinition skill,
		MonsterDefinition monster,
		ActiveSkillBuildData source,
		StatusEffectDefinition[] statusDefinitions = null)
	{
		if (skill is ProjectileSkillDefinition projectileSkillExecutionDefinition)
		{
			projectileSkillExecutionDefinition.Projectile.MagazineSize = source.MagazineCapacity;
			projectileSkillExecutionDefinition.Projectile.ReloadTime = source.ReloadSeconds;
			projectileSkillExecutionDefinition.Projectile.BurstProjectileCount = Math.Max(1, source.ProjectileBurstCount);
			projectileSkillExecutionDefinition.Projectile.BurstIntervalSeconds = source.ShotIntervalSeconds;
			if (source.BurstIntervalSeconds > 0f)
			{
				projectileSkillExecutionDefinition.Projectile.BurstIntervalSeconds = source.BurstIntervalSeconds;
			}
			projectileSkillExecutionDefinition.Projectile.BurstDamageProjectileIndex = source.BurstDamageProjectileIndex;
			projectileSkillExecutionDefinition.Projectile.BurstDamageMultiplier = 1f;
			if (source.BurstDamageMultiplier > 0f)
			{
				projectileSkillExecutionDefinition.Projectile.BurstDamageMultiplier = source.BurstDamageMultiplier;
			}
			projectileSkillExecutionDefinition.Projectile.ProjectilesPerShot = 1;
			projectileSkillExecutionDefinition.Projectile.PierceCount = source.PierceCount;
			projectileSkillExecutionDefinition.Projectile.ProjectileSpeed = source.ProjectileSpeed;
			projectileSkillExecutionDefinition.Projectile.LifetimeSeconds = source.ProjectileLifetimeSeconds;
			projectileSkillExecutionDefinition.ContactDamageEnabled = source.DamageDelaySeconds <= 0f;
			projectileSkillExecutionDefinition.StopOnFirstHit = source.DamageDelaySeconds > 0f;
			projectileSkillExecutionDefinition.ImpactDelaySeconds = Mathf.Max(0f, source.DamageDelaySeconds);
			projectileSkillExecutionDefinition.ImpactRuntimeVisual = source.ImpactRuntimeVisual;
			projectileSkillExecutionDefinition.HasImpactArea = source.DamageDelaySeconds > 0f;
			projectileSkillExecutionDefinition.ImpactArea.Radius = source.Radius;
			projectileSkillExecutionDefinition.ImpactArea.CoverAll = false;
			MapDamage(projectileSkillExecutionDefinition.Damage, source);
			MapDamage(projectileSkillExecutionDefinition.ImpactDamage, source);
			projectileSkillExecutionDefinition.OnHitStatus = CreateStatusApplication(source, statusDefinitions);
			projectileSkillExecutionDefinition.ImpactStatus = CreateStatusApplication(source, statusDefinitions);
		}
		else if (skill is LineSkillDefinition lineSkillExecutionDefinition)
		{
			lineSkillExecutionDefinition.LineLength = source.LineLength;
			lineSkillExecutionDefinition.CastRepeatCount = Math.Max(1, source.CastRepeatCount);
			lineSkillExecutionDefinition.CastRepeatIntervalSeconds = Mathf.Max(0f, source.CastRepeatIntervalSeconds);
			lineSkillExecutionDefinition.LineWidth = source.Radius;
			lineSkillExecutionDefinition.KnockbackDistance = source.KnockbackDistance;
			MapDamage(lineSkillExecutionDefinition.DamagePerTick, source);
			lineSkillExecutionDefinition.OnHitStatus = CreateStatusApplication(source, statusDefinitions);
		}
		else if (skill is ZoneSkillDefinition zoneSkillExecutionDefinition)
		{
			bool hitAllTargets;
			int hitTargetCount;
			bool usesHitTargetCount = TryResolveHitTargetCount(source.HitTargetCount, out hitAllTargets, out hitTargetCount);
			zoneSkillExecutionDefinition.Area.Radius = source.Radius;
			zoneSkillExecutionDefinition.Area.Duration = source.CooldownSeconds;
			if (source.ActiveDurationSeconds > 0f)
			{
				zoneSkillExecutionDefinition.Area.Duration = source.ActiveDurationSeconds;
			}
			zoneSkillExecutionDefinition.Area.TickInterval = source.ShotIntervalSeconds;
			zoneSkillExecutionDefinition.UsesHitTargetCount = usesHitTargetCount;
			zoneSkillExecutionDefinition.HitAllTargets = hitAllTargets;
			zoneSkillExecutionDefinition.HitTargetCount = hitTargetCount;
			if (hitAllTargets)
			{
				zoneSkillExecutionDefinition.HitTargetCount = int.MaxValue;
			}
			zoneSkillExecutionDefinition.Area.CoverAll = hitAllTargets;
			MapDamage(zoneSkillExecutionDefinition.DamagePerTick, source);
			zoneSkillExecutionDefinition.OnTickStatus = CreateStatusApplication(source, statusDefinitions);
		}
		else if (skill is SingleSkillDefinition singleSkillExecutionDefinition)
		{
			bool hitAllTargets2;
			int hitTargetCount2;
			bool flag = TryResolveHitTargetCount(source.HitTargetCount, out hitAllTargets2, out hitTargetCount2);
			bool flag2 = !string.IsNullOrWhiteSpace(source.DeploymentRequiredTargetStatusId);
			bool flag3 = source.RuntimeVisual != null && source.RuntimeVisual.Hitbox != null && source.RuntimeVisual.Hitbox.HasHitbox();
			bool flag4 = !hitAllTargets2 && flag && hitTargetCount2 > 1 && (source.SkillEffectPrefab != null || flag3);
			singleSkillExecutionDefinition.Area.Radius = source.Radius;
			singleSkillExecutionDefinition.Area.Duration = 0f;
			singleSkillExecutionDefinition.Area.TickInterval = 0f;
			singleSkillExecutionDefinition.UsesHitTargetCount = !flag4 && (flag || source.Radius <= 0f);
			singleSkillExecutionDefinition.UsePrefabHitbox = source.UsePrefabHitbox || hitAllTargets2 || flag4 || flag2;
			singleSkillExecutionDefinition.UseMultiDeployment = flag4 || flag2;
			singleSkillExecutionDefinition.HitAllTargets = hitAllTargets2;
			singleSkillExecutionDefinition.HitTargetCount = hitTargetCount2;
			if (hitAllTargets2 || (source.UsePrefabHitbox && !flag))
			{
				singleSkillExecutionDefinition.HitTargetCount = int.MaxValue;
			}
			singleSkillExecutionDefinition.DeploymentCount = 1;
			if (flag4)
			{
				singleSkillExecutionDefinition.DeploymentCount = hitTargetCount2;
			}
			singleSkillExecutionDefinition.DeploymentRequiredTargetStatusId = source.DeploymentRequiredTargetStatusId;
			singleSkillExecutionDefinition.DeploymentRequiredTargetStatusMinStacks = Mathf.Max(0, source.DeploymentRequiredTargetStatusMinStacks);
			singleSkillExecutionDefinition.TargetStatusStackStatusId = source.TargetStatusStackStatusId;
			singleSkillExecutionDefinition.TargetStatusStackMaxStacks = Mathf.Max(0, source.TargetStatusStackMaxStacks);
			singleSkillExecutionDefinition.ConsumeTargetStatusId = source.ConsumeTargetStatusId;
			singleSkillExecutionDefinition.ConsumeTargetStatusRatio = Mathf.Clamp01(source.ConsumeTargetStatusRatio);
			singleSkillExecutionDefinition.ConsumeTargetStatusStacks = Mathf.Max(0, source.ConsumeTargetStatusStacks);
			singleSkillExecutionDefinition.DamageDelaySeconds = Mathf.Max(0f, source.DamageDelaySeconds);
			singleSkillExecutionDefinition.ExecuteHealthRatioThreshold = Mathf.Clamp01(source.ExecuteHealthRatioThreshold);
			singleSkillExecutionDefinition.RequireExecuteThresholdToCast = source.RequireExecuteThresholdToCast;
			singleSkillExecutionDefinition.ExecuteDamageMultiplier = 1f;
			if (source.ExecuteDamageMultiplier > 0f)
			{
				singleSkillExecutionDefinition.ExecuteDamageMultiplier = source.ExecuteDamageMultiplier;
			}
			singleSkillExecutionDefinition.KillCooldownRefundRatio = Mathf.Clamp01(source.KillCooldownRefundRatio);
			singleSkillExecutionDefinition.BossDamageMultiplier = 1f;
			if (source.BossDamageMultiplier > 0f)
			{
				singleSkillExecutionDefinition.BossDamageMultiplier = source.BossDamageMultiplier;
			}
			singleSkillExecutionDefinition.Area.CoverAll = hitAllTargets2 || (!singleSkillExecutionDefinition.UsesHitTargetCount && source.Radius <= 0f && string.IsNullOrWhiteSpace(source.TargetSelection));
			MapDamage(singleSkillExecutionDefinition.Damage, source);
			singleSkillExecutionDefinition.TargetStatusStackDamage.Element = source.Attribute;
			singleSkillExecutionDefinition.TargetStatusStackDamage.BaseDamage = source.TargetStatusStackBaseDamage;
			singleSkillExecutionDefinition.TargetStatusStackDamage.AttackPowerCoefficient = source.TargetStatusStackAttackPowerCoefficient;
			singleSkillExecutionDefinition.TargetStatusStackDamage.SpellPowerCoefficient = source.TargetStatusStackSpellPowerCoefficient;
			singleSkillExecutionDefinition.TargetStatusStackDamage.CriticalAllowed = false;
			ApplySingleBaseNodes(singleSkillExecutionDefinition, source.Nodes, source.Attribute);
			if (!string.IsNullOrWhiteSpace(singleSkillExecutionDefinition.DeploymentRequiredTargetStatusId))
			{
				singleSkillExecutionDefinition.DeploymentRequiredTargetStatusKind = StatusValueParser.ParseStatusKind(
					singleSkillExecutionDefinition.DeploymentRequiredTargetStatusId);
			}
			if (!string.IsNullOrWhiteSpace(singleSkillExecutionDefinition.TargetStatusStackStatusId))
			{
				singleSkillExecutionDefinition.TargetStatusStackStatusKind = StatusValueParser.ParseStatusKind(
					singleSkillExecutionDefinition.TargetStatusStackStatusId);
			}
			if (!string.IsNullOrWhiteSpace(singleSkillExecutionDefinition.ConsumeTargetStatusId))
			{
				singleSkillExecutionDefinition.ConsumeTargetStatusKind = StatusValueParser.ParseStatusKind(
					singleSkillExecutionDefinition.ConsumeTargetStatusId);
			}
			if (!string.IsNullOrWhiteSpace(singleSkillExecutionDefinition.DeploymentRequiredTargetStatusId))
			{
				singleSkillExecutionDefinition.UsePrefabHitbox = true;
				singleSkillExecutionDefinition.UseMultiDeployment = true;
			}
			singleSkillExecutionDefinition.OnHitStatus = CreateStatusApplication(source, statusDefinitions);
		}
		else if (skill is SingleChainSkillDefinition singleChainSkillExecutionDefinition)
		{
			MapDamage(singleChainSkillExecutionDefinition.Damage, source);
			singleChainSkillExecutionDefinition.ChainDamageMultiplier = source.ChainDamageMultiplier;
			singleChainSkillExecutionDefinition.ChainDelaySeconds = source.ChainDelaySeconds;
			singleChainSkillExecutionDefinition.ChainRadius = source.Radius;
			if (source.ChainRadius > 0f)
			{
				singleChainSkillExecutionDefinition.ChainRadius = source.ChainRadius;
			}
			singleChainSkillExecutionDefinition.ExcludePrimaryTarget = source.ExcludePrimaryTarget;
		}
		else if (skill is SingleChargeSkillDefinition singleChargeSkillExecutionDefinition)
		{
			singleChargeSkillExecutionDefinition.TargetMaxHealthRatio = source.TargetMaxHealthRatio;
			singleChargeSkillExecutionDefinition.RampSeconds = source.ChargeRampSeconds;
			singleChargeSkillExecutionDefinition.MaxMoveSpeedMultiplier = source.ChargeMoveSpeedMultiplier;
			if (source.MoveSpeedMultiplier > 1f)
			{
				singleChargeSkillExecutionDefinition.MaxMoveSpeedMultiplier = source.MoveSpeedMultiplier;
			}
			singleChargeSkillExecutionDefinition.OnHitStatus = CreateStatusApplication(source, statusDefinitions);
		}
		else if (skill is BuffHealSkillDefinition buffHealSkillExecutionDefinition)
		{
			MapDamage(buffHealSkillExecutionDefinition.Healing, source);
			buffHealSkillExecutionDefinition.Healing.BaseDamage = source.FlatValue;
		}
		else if (skill is BuffSkillDefinition buffSkillExecutionDefinition)
		{
			buffSkillExecutionDefinition.Target = MapBuffTarget(source);
			buffSkillExecutionDefinition.UseConfiguredTargeting = !string.IsNullOrWhiteSpace(source.TargetScope);
			buffSkillExecutionDefinition.AttachVisualToCaster = MatchesProfile(source, "ApplyAllyMoveAndDamageMultiplier");
			buffSkillExecutionDefinition.BuffDuration = ResolveStatusDuration(source);
			buffSkillExecutionDefinition.HasAttachedDamage = source.BaseDamage > 0f;
			MapDamage(buffSkillExecutionDefinition.AttachedDamage, source);
			buffSkillExecutionDefinition.AttachedDamageRadius = source.Radius;
			buffSkillExecutionDefinition.AttachedStatus = CreateStatusApplication(source, statusDefinitions);
		}
		else if (skill is BuffShieldSkillDefinition buffShieldSkillExecutionDefinition)
		{
			buffShieldSkillExecutionDefinition.Target = MapBuffTarget(source);
			buffShieldSkillExecutionDefinition.UseConfiguredTargeting = !string.IsNullOrWhiteSpace(source.TargetScope);
			buffShieldSkillExecutionDefinition.AttachVisualToCaster = MatchesProfile(source, "GrantShieldToEnemyAllies");
			buffShieldSkillExecutionDefinition.ShieldBase = source.BaseDamage;
			buffShieldSkillExecutionDefinition.ShieldCoefficient = GetDominantCoefficient(source, out var statSource2);
			buffShieldSkillExecutionDefinition.ShieldStatSource = statSource2;
			buffShieldSkillExecutionDefinition.ShieldDuration = ResolveStatusDuration(source);
			buffShieldSkillExecutionDefinition.ShieldStatus = CreateStatusRuntimeData(source, statusDefinitions);
		}
	}

	/// <summary>전달된 런타임 입력값을 사용해 <c>Damage</c>를 대응시킨다.</summary>
	private static void MapDamage(SkillDamageSpec damage, ActiveSkillBuildData source)
	{
		damage.SkillId = source.SkillId;
		damage.Element = source.Attribute;
		damage.BaseDamage = source.BaseDamage;
		damage.AttackPowerCoefficient = source.AttackPowerCoefficient;
		damage.SpellPowerCoefficient = source.SpellPowerCoefficient;
		damage.CriticalAllowed = source.CriticalAllowed;
	}

	/// <summary>전달된 <c>targetScope</c> 값을 사용해 <c>EnemyTargetSide</c>를 대응시킨다.</summary>
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

	/// <summary>전달된 런타임 입력값을 사용해 <c>MatchesProfile</c> 조건을 평가하고 결과를 반환한다.</summary>
	private static bool MatchesProfile(ActiveSkillBuildData source, string profile)
	{
		return string.Equals(source.ExecutionProfile, profile, StringComparison.OrdinalIgnoreCase);
	}

	/// <summary>전달된 런타임 입력값을 사용해 <c>DominantCoefficient</c>를 반환한다.</summary>
	private static float GetDominantCoefficient(ActiveSkillBuildData source, out StatSource statSource)
	{
		if (Mathf.Abs(source.SpellPowerCoefficient) >= Mathf.Abs(source.AttackPowerCoefficient))
		{
			statSource = StatSource.Intelligence;
			return source.SpellPowerCoefficient;
		}
		statSource = StatSource.Attack;
		return source.AttackPowerCoefficient;
	}

	/// <summary>전달된 런타임 입력값을 사용해 <c>StatusApplication</c>를 생성한다.</summary>
	private static StatusApplicationSpec CreateStatusApplication(
		ActiveSkillBuildData source,
		StatusEffectDefinition[] statusDefinitions)
	{
		StatusApplicationSpec statusApplicationSpec = new StatusApplicationSpec();
		StatusRuntimeData runtimeStatusData = (statusApplicationSpec.Status = CreateStatusRuntimeData(source, statusDefinitions));
		statusApplicationSpec.Chance = Mathf.Clamp01(source.StatusChance);
		statusApplicationSpec.Stacks = 1;
		if (runtimeStatusData != null)
		{
			statusApplicationSpec.Stacks = Math.Max(1, runtimeStatusData.BaseStackAmount);
		}
		statusApplicationSpec.RefreshDuration = true;
		return statusApplicationSpec;
	}

	/// <summary>전달된 런타임 입력값을 사용해 <c>StatusRuntimeData</c>를 생성한다.</summary>
	private static StatusRuntimeData CreateStatusRuntimeData(
		ActiveSkillBuildData source,
		StatusEffectDefinition[] statusDefinitions)
	{
		if (string.IsNullOrWhiteSpace(source.StatusEffectId))
		{
			return null;
		}
		StatusEffectKind kind = StatusValueParser.ParseStatusKind(source.StatusEffectId);
		StatusRuntimeData runtimeStatusData =
			GetStatusRuntimeData(kind, statusDefinitions, source.StatusEffectLabel);

		if (source.StatusDurationSeconds > 0f)
		{
			runtimeStatusData.Duration = source.StatusDurationSeconds;
			runtimeStatusData.Permanent = false;
		}
		if (source.StatusMaxStacks > 0)
		{
			runtimeStatusData.MaxStacks = source.StatusMaxStacks;
			runtimeStatusData.IsStackable = runtimeStatusData.MaxStacks != 1;
		}
		if (source.StatusStackAmount > 0)
		{
			runtimeStatusData.BaseStackAmount = source.StatusStackAmount;
		}
		if (source.StatusPermanent && runtimeStatusData.Duration <= 0f)
		{
			runtimeStatusData.Permanent = true;
		}
		if (!Mathf.Approximately(source.StatusMoveSpeedBonus, 0f))
		{
			runtimeStatusData.MoveSpeedBonus = source.StatusMoveSpeedBonus;
		}
		runtimeStatusData.MovementSlowRate = runtimeStatusData.MoveSpeedBonus < 0f
			? -runtimeStatusData.MoveSpeedBonus
			: 0f;
		if (!Mathf.Approximately(source.StatusDamageTakenBonus, 0f))
		{
			runtimeStatusData.DamageTakenBonus = source.StatusDamageTakenBonus;
		}
		if (!Mathf.Approximately(source.StatusCriticalDamageTakenBonus, 0f))
		{
			runtimeStatusData.CriticalDamageTakenBonus = source.StatusCriticalDamageTakenBonus;
		}
		runtimeStatusData.AilmentResistanceBonus = source.StatusAilmentResistanceBonus;
		if (!Mathf.Approximately(source.StatusCriticalResistanceBonus, 0f))
		{
			runtimeStatusData.CriticalResistanceBonus = source.StatusCriticalResistanceBonus;
		}
		if (!Mathf.Approximately(source.StatusElementResistReduction, 0f))
		{
			runtimeStatusData.ElementResistReduction = source.StatusElementResistReduction;
		}
		runtimeStatusData.FlatElementResistReduction = source.StatusFlatElementResistReduction;
		if (!Mathf.Approximately(source.StatusElementDamageTakenBonus, 0f))
		{
			runtimeStatusData.ElementDamageTakenBonus = source.StatusElementDamageTakenBonus;
		}
		if (!Mathf.Approximately(source.StatusActionSpeedBonus, 0f))
		{
			runtimeStatusData.Modifiers.ActionSpeedBonus = source.StatusActionSpeedBonus;
		}
		if (!Mathf.Approximately(source.StatusAttackPowerBonus, 0f))
		{
			runtimeStatusData.Modifiers.AttackPowerBonus = source.StatusAttackPowerBonus;
		}
		runtimeStatusData.Modifiers.DamageBonusRate = source.StatusDamageBonusRate;
		runtimeStatusData.SourceSkillId = source.SkillId;
		if (!string.IsNullOrWhiteSpace(source.StatusTargetScope))
		{
			runtimeStatusData.TargetScope = StatusValueParser.ParseTargetScope(source.StatusTargetScope);
		}
		if (!string.IsNullOrWhiteSpace(source.StatusMergePolicy))
		{
			runtimeStatusData.MergePolicy = StatusValueParser.ParseMergePolicy(source.StatusMergePolicy);
		}
		if (!string.IsNullOrWhiteSpace(source.ShieldAmountRefreshPolicy))
		{
			runtimeStatusData.ShieldAmountRefreshPolicy =
				StatusValueParser.ParseShieldRefreshRule(source.ShieldAmountRefreshPolicy);
		}
		if (source.StatusEffectPrefab != null)
		{
			runtimeStatusData.StatusEffectPrefab = source.StatusEffectPrefab;
		}
		if (source.RuntimeVisual != null
			&& source.RuntimeVisual.Anchor == RuntimeSkillVisualAnchor.StatusTarget)
		{
			runtimeStatusData.RuntimeVisual = source.RuntimeVisual;
		}
		runtimeStatusData.Modifiers.ResistReduction = runtimeStatusData.ElementResistReduction;
		return runtimeStatusData;
	}

	/// <summary>전달된 런타임 입력값을 사용해 <c>ResolveHitTargetCount</c> 작업을 시도하고 성공 여부를 반환한다.</summary>
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

	/// <summary>전달된 <c>source</c> 값을 사용해 <c>BuffTarget</c>를 대응시킨다.</summary>
	private static SkillTargetSide MapBuffTarget(ActiveSkillBuildData source)
	{
		StatusValueParser.TryParseTargetScope(source.StatusTargetScope, out var scope);
		if (scope == StatusTargetScope.Self)
		{
			return SkillTargetSide.Self;
		}

		return SkillTargetSide.AllAllies;
	}

	/// <summary>전달된 <c>source</c> 값을 사용해 <c>StatusDuration</c>를 결정한다.</summary>
	private static float ResolveStatusDuration(ActiveSkillBuildData source)
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

	/// <summary>전달된 런타임 입력값을 사용해 <c>SingleBaseNodes</c>를 적용한다.</summary>
	private static void ApplySingleBaseNodes(SingleSkillDefinition single, SkillNodeBuildData[] nodes, DamageAttribute attribute)
	{
		foreach (SkillNodeBuildData skillNodeDefinition in nodes)
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
					single.DeploymentRequiredTargetStatusId = GetParam(skillNodeDefinition, "status_id");
					single.DeploymentRequiredTargetStatusMinStacks = Mathf.Max(1, GetIntParam(skillNodeDefinition, "min_stacks", 1));
				}
				else if (string.Equals(a, "TargetStatusStackDamage", StringComparison.OrdinalIgnoreCase))
				{
					single.TargetStatusStackStatusId = GetParam(skillNodeDefinition, "status_id");
					single.TargetStatusStackMaxStacks = Mathf.Max(0, GetIntParam(skillNodeDefinition, "max_stacks", 0));
					single.TargetStatusStackDamage.Element = attribute;
					single.TargetStatusStackDamage.BaseDamage = GetFloatParam(skillNodeDefinition, "base_damage", 0f);
					single.TargetStatusStackDamage.AttackPowerCoefficient = GetFloatParam(skillNodeDefinition, "attack_power_coefficient", 0f);
					single.TargetStatusStackDamage.SpellPowerCoefficient = GetFloatParam(skillNodeDefinition, "spell_power_coefficient", 0f);
					single.TargetStatusStackDamage.CriticalAllowed = false;
				}
			}
		}
	}

	/// <summary>전달된 <c>runtimeKind</c> 값을 사용해 <c>Shape</c>를 대응시킨다.</summary>
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
