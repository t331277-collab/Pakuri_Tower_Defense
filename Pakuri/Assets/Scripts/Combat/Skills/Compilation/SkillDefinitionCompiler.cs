using System;
using System.Collections.Generic;
using System.Globalization;
using Pakuri.Combat;
using Pakuri.Data;
using UnityEngine;

/*
 * 작성된 액티브·패시브 스킬 데이터를 전투용 SkillDefinition으로 변환한다.
 * 선택지는 SkillChoiceCompiler, 노드는 SkillNodeMapper에 맡긴다.
 */
namespace Pakuri.InGame
{

public static class SkillDefinitionCompiler
{
	/*
	 * 런 세션에서 확정된 학습 스킬과 Choice ID를 유닛 저장소에 복사한다.
	 */
	public static void ApplyLearnedSkills(
		UnitSkills target /* 학습 결과를 저장할 유닛 스킬 정보 */,
		IReadOnlyList<string> activeSkillIds /* 학습한 액티브 스킬 식별자 목록 */,
		IReadOnlyList<string> passiveSkillIds /* 학습한 패시브 스킬 식별자 목록 */,
		IReadOnlyList<string> choiceIds /* 선택한 강화·마스터 식별자 목록 */)
	{
		target.Clear();
		for (int i = 0; i < activeSkillIds.Count; i++)
		{
			target.AddActiveSkill(activeSkillIds[i]);
		}

		for (int i = 0; i < passiveSkillIds.Count; i++)
		{
			target.AddPassiveSkill(passiveSkillIds[i]);
		}

		for (int i = 0; i < choiceIds.Count; i++)
		{
			string choiceId = choiceIds[i];
			if (!GameDataLoader.CurrentCatalog.TryGetData(choiceId, out SkillChoiceDefinition choice))
			{
				throw new InvalidOperationException($"Unknown learned skill choice '{choiceId}'.");
			}

			if (choice.ChoiceGroup == SkillChoiceGroup.ActiveMaster)
			{
				target.AddMasterSkill(choiceId);
			}
			else
			{
				target.AddEnhancement(choiceId);
			}
		}
	}

	/*
	 * CompileActive 작업 결과를 반환한다.
	 */
	public static SkillDefinition CompileActive(MonsterDefinition monster /* 몬스터 */, SkillSourceDefinition source /* 변환할 스킬 정의 */)
	{
		SkillDefinition skillRuntimeData = CreateConcreteActiveSkill(source);
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
	public static SkillDefinition CompileActive(string monsterId /* 몬스터 식별자 */, SkillSourceDefinition source /* 변환할 스킬 정의 */)
	{
		return CompileActive(monsterId, source, null);
	}

	/*
	 * CompileActive 작업 결과를 반환한다.
	 */
	public static SkillDefinition CompileActive(string ownerId /* 소유자 식별자 */, SkillSourceDefinition source /* 변환할 스킬 정의 */, SkillTriggerDefinition[] triggers /* 트리거 목록 */)
	{
		SkillDefinition skillRuntimeData = CreateConcreteActiveSkill(source);
		MapCommonFields(skillRuntimeData, ownerId, source, triggers);
		MapActiveFields(skillRuntimeData, null, source);
		return skillRuntimeData;
	}

	/*
	 * CompilePassive 작업 결과를 반환한다.
	 */
	public static PassiveSkillDefinition CompilePassive(MonsterDefinition monster /* 몬스터 */, PassiveDefinition source /* 변환할 패시브 정의 */)
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
		passiveSkillExecutionDefinition.SkillEffectPrefab = source.SkillEffectPrefab;
		passiveSkillExecutionDefinition.BaseModifierChoices = SkillChoiceCompiler.Compile(source.BaseModifierChoices);
		passiveSkillExecutionDefinition.EnhancementChoices = SkillChoiceCompiler.Compile(source.EnhancementChoices);
		passiveSkillExecutionDefinition.MasterChoices = Array.Empty<SkillChoice>();
		SkillTriggerDefinition[] triggers = null;
		if (monster != null)
		{
			triggers = monster.SkillTriggers;
		}
		passiveSkillExecutionDefinition.SkillTriggers = FilterSkillTriggersForSkill(triggers, source.PassiveId);
		StatusRuntimeCompiler.CompileTriggers(passiveSkillExecutionDefinition.SkillTriggers);
		passiveSkillExecutionDefinition.NormalizedNodes = SkillNodeMapper.MapSkillNodeDefinitions(source.NormalizedNodes);
		return passiveSkillExecutionDefinition;
	}

	/*
	 * CreateConcreteActiveSkill에 필요한 결과를 만들어 반환한다.
	 */
	private static SkillDefinition CreateConcreteActiveSkill(SkillSourceDefinition source /* 변환할 스킬 정의 */)
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

	/*
	 * CreateRuntimeData에 필요한 결과를 만들어 반환한다.
	 */
	private static T CreateRuntimeData<T>() where T : SkillDefinition, new()
	{
		return new T();
	}

	/*
	 * MapCommonFields에 필요한 값을 변환해 현재 상태에 반영한다.
	 */
	private static void MapCommonFields(SkillDefinition skill /* 실행하거나 검사할 스킬 */, string monsterId /* 몬스터 식별자 */, SkillSourceDefinition source /* 변환할 스킬 정의 */, SkillTriggerDefinition[] monsterTriggers = null /* 몬스터 트리거 목록 */)
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
		skill.EnhancementChoices = SkillChoiceCompiler.Compile(source.EnhancementChoices);
		skill.MasterChoices = SkillChoiceCompiler.Compile(source.MasterSkillChoices);
		skill.SkillTriggers = FilterSkillTriggersForSkill(monsterTriggers, source.SkillId);
		StatusRuntimeCompiler.CompileTriggers(skill.SkillTriggers);
		skill.NormalizedNodes = SkillNodeMapper.MapSkillNodeDefinitions(source.NormalizedNodes);
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
	private static SkillTriggerDefinition[] FilterSkillTriggersForSkill(SkillTriggerDefinition[] triggers /* 트리거 목록 */, string skillId /* 스킬 식별자 */)
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
	private static bool IsTriggerOwnedBySkill(SkillTriggerDefinition trigger /* 실행하거나 검사할 트리거 */, string skillId /* 스킬 식별자 */)
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
	private static void MapActiveFields(SkillDefinition skill /* 실행하거나 검사할 스킬 */, MonsterDefinition monster /* 몬스터 */, SkillSourceDefinition source /* 변환할 스킬 정의 */)
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
			projectileSkillExecutionDefinition.OnHitStatus = CreateStatusApplication(source);
			projectileSkillExecutionDefinition.ImpactStatus = CreateStatusApplication(source);
		}
		else if (skill is LineSkillDefinition lineSkillExecutionDefinition)
		{
			lineSkillExecutionDefinition.LineLength = source.LineLength;
			lineSkillExecutionDefinition.CastRepeatCount = Math.Max(1, source.CastRepeatCount);
			lineSkillExecutionDefinition.CastRepeatIntervalSeconds = Mathf.Max(0f, source.CastRepeatIntervalSeconds);
			lineSkillExecutionDefinition.LineWidth = source.Radius;
			lineSkillExecutionDefinition.KnockbackDistance = source.KnockbackDistance;
			MapDamage(lineSkillExecutionDefinition.DamagePerTick, source);
			lineSkillExecutionDefinition.OnHitStatus = CreateStatusApplication(source);
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
			zoneSkillExecutionDefinition.OnTickStatus = CreateStatusApplication(source);
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
			ApplySingleBaseNodes(singleSkillExecutionDefinition, source.NormalizedNodes, source.Attribute);
			if (!string.IsNullOrWhiteSpace(singleSkillExecutionDefinition.DeploymentRequiredTargetStatusId))
			{
				singleSkillExecutionDefinition.DeploymentRequiredTargetStatusKind = StatusRuntimeCompiler.ParseStatusKind(
					singleSkillExecutionDefinition.DeploymentRequiredTargetStatusId);
			}
			if (!string.IsNullOrWhiteSpace(singleSkillExecutionDefinition.TargetStatusStackStatusId))
			{
				singleSkillExecutionDefinition.TargetStatusStackStatusKind = StatusRuntimeCompiler.ParseStatusKind(
					singleSkillExecutionDefinition.TargetStatusStackStatusId);
			}
			if (!string.IsNullOrWhiteSpace(singleSkillExecutionDefinition.ConsumeTargetStatusId))
			{
				singleSkillExecutionDefinition.ConsumeTargetStatusKind = StatusRuntimeCompiler.ParseStatusKind(
					singleSkillExecutionDefinition.ConsumeTargetStatusId);
			}
			if (!string.IsNullOrWhiteSpace(singleSkillExecutionDefinition.DeploymentRequiredTargetStatusId))
			{
				singleSkillExecutionDefinition.UsePrefabHitbox = true;
				singleSkillExecutionDefinition.UseMultiDeployment = true;
			}
			singleSkillExecutionDefinition.OnHitStatus = CreateStatusApplication(source);
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
			singleChargeSkillExecutionDefinition.OnHitStatus = CreateStatusApplication(source);
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
			buffSkillExecutionDefinition.AttachedStatus = CreateStatusApplication(source);
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
			buffShieldSkillExecutionDefinition.ShieldStatus = CreateStatusRuntimeData(source);
		}
	}

	/*
	 * MapDamage에 필요한 값을 변환해 현재 상태에 반영한다.
	 */
	private static void MapDamage(SkillDamageSpec damage /* 피해량 계산 설정 */, SkillSourceDefinition source /* 변환할 스킬 정의 */)
	{
		damage.SkillId = source.SkillId;
		damage.Element = source.Attribute;
		damage.BaseDamage = source.BaseDamage;
		damage.AttackPowerCoefficient = source.AttackPowerCoefficient;
		damage.SpellPowerCoefficient = source.SpellPowerCoefficient;
		damage.CriticalAllowed = source.CriticalAllowed;
	}

	/*
	 * MapEnemyTargetSide에 필요한 형식으로 변환해 반환한다.
	 */
	private static SkillTargetSide MapEnemyTargetSide(string targetScope /* 대상 적용 범위 */)
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
	private static bool MatchesProfile(SkillSourceDefinition source /* 변환할 스킬 정의 */, string profile /* 실행 설정 */)
	{
		return string.Equals(source.ExecutionProfile, profile, StringComparison.OrdinalIgnoreCase);
	}

	/*
	 * GetDominantCoefficient에 해당하는 값을 찾아 반환한다.
	 */
	private static float GetDominantCoefficient(SkillSourceDefinition source /* 변환할 스킬 정의 */, out StatSource statSource /* 능력치 발생 원본 */)
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
	 * CreateStatusApplication에 필요한 결과를 만들어 반환한다.
	 */
	private static StatusApplicationSpec CreateStatusApplication(SkillSourceDefinition source /* 변환할 스킬 정의 */)
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
	private static StatusRuntimeData CreateStatusRuntimeData(SkillSourceDefinition source /* 변환할 스킬 정의 */)
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
	private static bool TryResolveHitTargetCount(string rawValue /* 변환 전 원본 문자열 */, out bool hitAllTargets /* 적중 전체 대상 목록 여부 */, out int hitTargetCount /* 적중시킬 대상 수 */)
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
	private static SkillTargetSide MapBuffTarget(SkillSourceDefinition source /* 변환할 스킬 정의 */)
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
	private static float ResolveStatusDuration(SkillSourceDefinition source /* 변환할 스킬 정의 */)
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
	 * ApplySingleBaseNodes 처리를 대상에 적용한다.
	 */
	private static void ApplySingleBaseNodes(SingleSkillDefinition single /* 단일 */, SkillNodeDefinition[] nodes /* 노드 목록 */, DamageAttribute attribute /* 피해 속성 */)
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
					single.TargetStatusStackDamage.AttackPowerCoefficient = SkillNodeMapper.GetFloatParam(skillNodeDefinition, "attack_power_coefficient", 0f);
					single.TargetStatusStackDamage.SpellPowerCoefficient = SkillNodeMapper.GetFloatParam(skillNodeDefinition, "spell_power_coefficient", 0f);
					single.TargetStatusStackDamage.CriticalAllowed = false;
				}
			}
		}
	}


	/*
	 * MapShape에 필요한 형식으로 변환해 반환한다.
	 */
	private static SkillTargetShape MapShape(SkillRuntimeKind runtimeKind /* 런타임 종류 */)
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
