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

internal sealed class ActiveSkillBuildData
{
	public string SkillName;
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
	public string TargetSelectionStatusName;
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
	public string DeploymentRequiredTargetStatusName;
	public int DeploymentRequiredTargetStatusMinStacks;
	public string TargetStatusStackStatusName;
	public int TargetStatusStackMaxStacks;
	public float TargetStatusStackBaseDamage;
	public float TargetStatusStackAttackPowerCoefficient;
	public float TargetStatusStackSpellPowerCoefficient;
	public string ConsumeTargetStatusName;
	public float ConsumeTargetStatusRatio;
	public int ConsumeTargetStatusStacks;
	public string StatusEffectName;
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
	public float StatusElementResistReduction;
	public float StatusFlatElementResistReduction;
	public float StatusElementDamageTakenBonus;
	public string Summary;
	public SkillChoiceBuildData[] EnhancementChoices = Array.Empty<SkillChoiceBuildData>();
	public SkillChoiceBuildData[] MasterSkillChoices = Array.Empty<SkillChoiceBuildData>();
	public SkillNodeBuildData[] Nodes = Array.Empty<SkillNodeBuildData>();
}

internal sealed class PassiveSkillBuildData
{
	public string PassiveName;
	public string DisplayName;
	public SkillSlot Slot;
	public SkillSlot RequiredActiveSlot;
	public bool IsAvailableWithoutActiveRequirement;
	public SkillImplementationState ImplementationState = SkillImplementationState.DataOnly;
	public Sprite SkillIcon;
	public string DescriptionText;
	public string Summary;
	public SkillChoiceBuildData[] EnhancementChoices = Array.Empty<SkillChoiceBuildData>();
	public SkillNodeBuildData[] Nodes = Array.Empty<SkillNodeBuildData>();
	public SkillNodeBuildData[] BaseNodes = Array.Empty<SkillNodeBuildData>();
}

internal sealed class SkillChoiceBuildData
{
	public string ChoiceName;
	public string MonsterName;
	public string SkillName;
	public string TargetSkillName;
	public SkillChoiceGroup ChoiceGroup;
	public string Title;
	public Sprite SkillIcon;
	public GameObject SkillEffectPrefab;
	public string DescriptionText;
	public SkillNodeBuildData[] Nodes = Array.Empty<SkillNodeBuildData>();
}

/// GameDataCatalogBuilder 런타임 데이터를 파싱된 저작 데이터에서 생성한다.
internal sealed partial class GameDataCatalogBuilder
{

	private static SkillDefinition BuildActiveDefinition(
		string ownerName,
		ActiveSkillBuildData source,
		StatusEffectDefinition[] statusDefinitions)
	{
		SkillDefinition skillRuntimeData = CreateConcreteActiveSkill(source);
		MapCommonFields(skillRuntimeData, ownerName, source);
		MapActiveFields(skillRuntimeData, null, source, statusDefinitions);
		return skillRuntimeData;
	}

	private static PassiveSkillDefinition BuildPassiveDefinition(MonsterDefinition monster, PassiveSkillBuildData source)
	{
		PassiveSkillDefinition passiveSkillExecutionDefinition = CreateRuntimeData<PassiveSkillDefinition>();
		passiveSkillExecutionDefinition.SkillName = source.PassiveName;
		passiveSkillExecutionDefinition.DisplayName = source.DisplayName;
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
		passiveSkillExecutionDefinition.EnhancementChoices = BuildChoices(source.EnhancementChoices);
		passiveSkillExecutionDefinition.MasterChoices = Array.Empty<SkillChoice>();
		passiveSkillExecutionDefinition.BaseNodes = MapSkillNodes(source.BaseNodes);
		passiveSkillExecutionDefinition.Nodes = MapSkillNodes(source.Nodes);
		return passiveSkillExecutionDefinition;
	}

	private static SkillChoice[] BuildChoices(SkillChoiceBuildData[] source)
	{
		var choices = new SkillChoice[source.Length];
		for (var i = 0; i < source.Length; i++)
		{
			var choice = source[i];
			choices[i] = new SkillChoice
			{
				ChoiceName = choice.ChoiceName,
				MonsterName = choice.MonsterName,
				SkillName = choice.SkillName,
				TargetSkillName = choice.TargetSkillName,
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

	private static SkillDefinition CreateConcreteActiveSkill(ActiveSkillBuildData source)
	{
		if (MatchesProfile(source, "DamageArea"))
		{
			return CreateRuntimeData<SingleSkillDefinition>();
		}
		if (MatchesProfile(source, "DamageThenDelayedChain"))
		{
			return CreateRuntimeData<SingleSkillDefinition>();
		}
		if (MatchesProfile(source, "ChargeDamageStatus"))
		{
			return CreateRuntimeData<BuffSkillDefinition>();
		}
		if (source.RuntimeKind == SkillRuntimeKind.Heal)
		{
			return CreateRuntimeData<BuffSkillDefinition>();
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
			return CreateRuntimeData<ZoneSkillDefinition>();
		case SkillRuntimeKind.Buff:
			return CreateRuntimeData<BuffSkillDefinition>();
		case SkillRuntimeKind.Shield:
			return CreateRuntimeData<BuffSkillDefinition>();
		default:
			throw new InvalidOperationException("Unsupported active skill runtime kind: " + source.RuntimeKind);
		}
	}

	/// RuntimeData를 생성한다.
	private static T CreateRuntimeData<T>() where T : SkillDefinition, new()
	{
		return new T();
	}

	private static void MapCommonFields(
		SkillDefinition skill,
		string monsterName,
		ActiveSkillBuildData source)
	{
		skill.SkillName = source.SkillName;
		skill.DisplayName = source.DisplayName;
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
		skill.Targeting.SelectionStatusName = source.TargetSelectionStatusName;
		if (!string.IsNullOrWhiteSpace(source.TargetSelectionStatusName))
		{
			skill.Targeting.SelectionStatusKind = StatusValueParser.ParseStatusKind(
				source.TargetSelectionStatusName);
		}
		skill.Targeting.SelectionStatusMinStacks = Mathf.Max(0, source.TargetSelectionStatusMinStacks);
		skill.Targeting.Shape = MapShape(source.RuntimeKind);
		skill.Targeting.CoverAll = source.RuntimeKind == SkillRuntimeKind.SingleAttack && source.Radius <= 0f && string.IsNullOrWhiteSpace(source.TargetSelection);
		if (MatchesProfile(source, "DamageThenDelayedChain"))
		{
			skill.Targeting.Radius = 0f;
			skill.Targeting.Shape = SkillTargetShape.Single;
			skill.Targeting.CoverAll = false;
		}
	}

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
			projectileSkillExecutionDefinition.ArrivalDelaySeconds = Mathf.Max(0f, source.DamageDelaySeconds);
			projectileSkillExecutionDefinition.ArrivalSkill = BuildProjectileArrivalSkill(
				source,
				statusDefinitions);
			MapDamage(projectileSkillExecutionDefinition.Damage, source);
			projectileSkillExecutionDefinition.OnHitStatus = CreateStatusApplication(source, statusDefinitions);
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
			zoneSkillExecutionDefinition.Area.Radius = source.Radius;
			zoneSkillExecutionDefinition.Area.Duration = source.CooldownSeconds;
			if (source.ActiveDurationSeconds > 0f)
			{
				zoneSkillExecutionDefinition.Area.Duration = source.ActiveDurationSeconds;
			}
			zoneSkillExecutionDefinition.Area.TickInterval = source.ShotIntervalSeconds;
			MapDamage(zoneSkillExecutionDefinition.DamagePerTick, source);
			zoneSkillExecutionDefinition.OnTickStatus = CreateStatusApplication(source, statusDefinitions);
		}
		else if (skill is SingleSkillDefinition singleSkillExecutionDefinition)
		{
			bool hitAllTargets2;
			int hitTargetCount2;
			bool flag = TryResolveHitTargetCount(source.HitTargetCount, out hitAllTargets2, out hitTargetCount2);
			bool flag2 = !string.IsNullOrWhiteSpace(source.DeploymentRequiredTargetStatusName);
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
			singleSkillExecutionDefinition.DeploymentRequiredTargetStatusName = source.DeploymentRequiredTargetStatusName;
			singleSkillExecutionDefinition.DeploymentRequiredTargetStatusMinStacks = Mathf.Max(0, source.DeploymentRequiredTargetStatusMinStacks);
			singleSkillExecutionDefinition.TargetStatusStackStatusName = source.TargetStatusStackStatusName;
			singleSkillExecutionDefinition.TargetStatusStackMaxStacks = Mathf.Max(0, source.TargetStatusStackMaxStacks);
			singleSkillExecutionDefinition.ConsumeTargetStatusName = source.ConsumeTargetStatusName;
			singleSkillExecutionDefinition.ConsumeTargetStatusRatio = Mathf.Clamp01(source.ConsumeTargetStatusRatio);
			singleSkillExecutionDefinition.ConsumeTargetStatusStacks = Mathf.Max(0, source.ConsumeTargetStatusStacks);
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
			if (!string.IsNullOrWhiteSpace(singleSkillExecutionDefinition.DeploymentRequiredTargetStatusName))
			{
				singleSkillExecutionDefinition.DeploymentRequiredTargetStatusKind = StatusValueParser.ParseStatusKind(
					singleSkillExecutionDefinition.DeploymentRequiredTargetStatusName);
			}
			if (!string.IsNullOrWhiteSpace(singleSkillExecutionDefinition.TargetStatusStackStatusName))
			{
				singleSkillExecutionDefinition.TargetStatusStackStatusKind = StatusValueParser.ParseStatusKind(
					singleSkillExecutionDefinition.TargetStatusStackStatusName);
			}
			if (!string.IsNullOrWhiteSpace(singleSkillExecutionDefinition.ConsumeTargetStatusName))
			{
				singleSkillExecutionDefinition.ConsumeTargetStatusKind = StatusValueParser.ParseStatusKind(
					singleSkillExecutionDefinition.ConsumeTargetStatusName);
			}
			if (!string.IsNullOrWhiteSpace(singleSkillExecutionDefinition.DeploymentRequiredTargetStatusName))
			{
				singleSkillExecutionDefinition.UsePrefabHitbox = true;
				singleSkillExecutionDefinition.UseMultiDeployment = true;
			}
			singleSkillExecutionDefinition.OnHitStatus = CreateStatusApplication(source, statusDefinitions);
			if (MatchesProfile(source, "DamageThenDelayedChain"))
			{
				singleSkillExecutionDefinition.Area.Radius = 0f;
				singleSkillExecutionDefinition.UsesHitTargetCount = true;
				singleSkillExecutionDefinition.UsePrefabHitbox = false;
				singleSkillExecutionDefinition.UseMultiDeployment = false;
				singleSkillExecutionDefinition.HitAllTargets = false;
				singleSkillExecutionDefinition.HitTargetCount = 1;
				singleSkillExecutionDefinition.DeploymentCount = 1;
				singleSkillExecutionDefinition.Area.CoverAll = false;
			}
		}
		else if (skill is BuffSkillDefinition buffSkillExecutionDefinition)
		{
			buffSkillExecutionDefinition.EffectKind = MapBuffEffectKind(source);
			buffSkillExecutionDefinition.Target = MapBuffTarget(source);
			buffSkillExecutionDefinition.UseConfiguredTargeting = !string.IsNullOrWhiteSpace(source.TargetScope);
			buffSkillExecutionDefinition.AttachVisualToCaster = MatchesProfile(source, "ApplyAllyMoveAndDamageMultiplier");
			buffSkillExecutionDefinition.AttachedStatus = CreateStatusApplication(source, statusDefinitions);
			if (buffSkillExecutionDefinition.EffectKind == BuffEffectKind.Heal)
			{
				MapDamage(buffSkillExecutionDefinition.Healing, source);
				buffSkillExecutionDefinition.Healing.BaseDamage = source.FlatValue;
			}
			else if (buffSkillExecutionDefinition.EffectKind == BuffEffectKind.Shield)
			{
				buffSkillExecutionDefinition.AttachVisualToCaster = MatchesProfile(source, "GrantShieldToEnemyAllies");
				buffSkillExecutionDefinition.ShieldBase = source.BaseDamage;
				buffSkillExecutionDefinition.ShieldCoefficient = GetDominantCoefficient(source, out var statSource);
				buffSkillExecutionDefinition.ShieldStatSource = statSource;
				buffSkillExecutionDefinition.ShieldDuration = ResolveStatusDuration(source);
				buffSkillExecutionDefinition.ShieldStatus = CreateStatusRuntimeData(source, statusDefinitions);
			}
			else if (buffSkillExecutionDefinition.EffectKind == BuffEffectKind.Charge)
			{
				buffSkillExecutionDefinition.ChargeTargetMaxHealthRatio = source.TargetMaxHealthRatio;
				buffSkillExecutionDefinition.ChargeRampSeconds = source.ChargeRampSeconds;
				buffSkillExecutionDefinition.ChargeMaxMoveSpeedMultiplier = source.MoveSpeedMultiplier > 1f
					? source.MoveSpeedMultiplier
					: source.ChargeMoveSpeedMultiplier;
			}
		}
	}

	private static SingleSkillDefinition BuildProjectileArrivalSkill(
		ActiveSkillBuildData source,
		StatusEffectDefinition[] statusDefinitions)
	{
		if (source == null || source.DamageDelaySeconds <= 0f)
		{
			return null;
		}

		var radius = Mathf.Max(0f, source.Radius);
		var arrivalSkill = new SingleSkillDefinition
		{
			SkillName = source.SkillName + "@arrival",
			DisplayName = source.DisplayName + " Arrival",
			RuntimeKind = SkillRuntimeKind.SingleAttack,
			ImplementationState = SkillImplementationState.RuntimeImplemented,
			IsActive = false,
			Element = source.Attribute,
			RuntimeVisual = source.ImpactRuntimeVisual,
			Targeting = new SkillTargetingSpec
			{
				TargetSide = SkillTargetSide.Enemy,
				Selection = SkillTargetSelection.Nearest,
				Shape = SkillTargetShape.Circle,
				Radius = radius,
				CoverAll = false
			},
			Area = new AreaBlueprintSpec
			{
				Radius = radius,
				CoverAll = false
			},
			HitAllTargets = true,
			HitTargetCount = int.MaxValue,
			Damage = new SkillDamageSpec(),
			OnHitStatus = new StatusApplicationSpec()
		};

		MapDamage(arrivalSkill.Damage, source);
		arrivalSkill.OnHitStatus = CreateStatusApplication(source, statusDefinitions);
		return arrivalSkill;
	}

	private static BuffEffectKind MapBuffEffectKind(ActiveSkillBuildData source)
	{
		if (MatchesProfile(source, "ChargeDamageStatus"))
		{
			return BuffEffectKind.Charge;
		}
		if (source.RuntimeKind == SkillRuntimeKind.Heal)
		{
			return BuffEffectKind.Heal;
		}
		if (source.RuntimeKind == SkillRuntimeKind.Shield
			&& !MatchesProfile(source, "ApplySelfIncomingDamageMultiplier"))
		{
			return BuffEffectKind.Shield;
		}
		return BuffEffectKind.Status;
	}

	private static void MapDamage(SkillDamageSpec damage, ActiveSkillBuildData source)
	{
		damage.SkillName = source.SkillName;
		damage.Element = source.Attribute;
		damage.BaseDamage = source.BaseDamage;
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

	private static bool MatchesProfile(ActiveSkillBuildData source, string profile)
	{
		return string.Equals(source.ExecutionProfile, profile, StringComparison.OrdinalIgnoreCase);
	}

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

	private static StatusRuntimeData CreateStatusRuntimeData(
		ActiveSkillBuildData source,
		StatusEffectDefinition[] statusDefinitions)
	{
		if (string.IsNullOrWhiteSpace(source.StatusEffectName))
		{
			return null;
		}
		StatusEffectKind kind = StatusValueParser.ParseStatusKind(source.StatusEffectName);
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
		runtimeStatusData.SourceSkillName = source.SkillName;
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

	private static SkillTargetSide MapBuffTarget(ActiveSkillBuildData source)
	{
		StatusValueParser.TryParseTargetScope(source.StatusTargetScope, out var scope);
		if (scope == StatusTargetScope.Self)
		{
			return SkillTargetSide.Self;
		}

		return SkillTargetSide.AllAllies;
	}

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

	private static void ApplySingleBaseNodes(SingleSkillDefinition single, SkillNodeBuildData[] nodes, DamageAttribute attribute)
	{
		foreach (SkillNodeBuildData skillNodeDefinition in nodes)
		{
			if (skillNodeDefinition != null && skillNodeDefinition.EnabledByDefault)
			{
				string a = skillNodeDefinition.HandlerName;
				if (a == null)
				{
					a = string.Empty;
				}
				if (string.Equals(a, "StatusFilteredDeployment", StringComparison.OrdinalIgnoreCase))
				{
					single.DeploymentRequiredTargetStatusName = GetParam(skillNodeDefinition, "status_name");
					single.DeploymentRequiredTargetStatusMinStacks = Mathf.Max(1, GetIntParam(skillNodeDefinition, "min_stacks", 1));
				}
				else if (string.Equals(a, "TargetStatusStackDamage", StringComparison.OrdinalIgnoreCase))
				{
					single.TargetStatusStackStatusName = GetParam(skillNodeDefinition, "status_name");
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

	private static SkillTargetShape MapShape(SkillRuntimeKind runtimeKind)
	{
		switch (runtimeKind)
		{
		case SkillRuntimeKind.LineAttack:
			return SkillTargetShape.Line;
		case SkillRuntimeKind.AreaAttack:
		case SkillRuntimeKind.SingleAttack:
		case SkillRuntimeKind.Mark:
		case SkillRuntimeKind.Execute:
			return SkillTargetShape.Circle;
		default:
			return SkillTargetShape.Single;
		}
	}
}

}
