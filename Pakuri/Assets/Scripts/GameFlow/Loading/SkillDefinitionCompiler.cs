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
		passiveSkillExecutionDefinition.IsActive = false;
		passiveSkillExecutionDefinition.Element = DamageAttribute.Physical;
		if (monster != null)
		{
			passiveSkillExecutionDefinition.Element = monster.PrimaryAttribute;
		}
		passiveSkillExecutionDefinition.Description = source.DescriptionText;
		passiveSkillExecutionDefinition.Icon = source.SkillIcon;
		passiveSkillExecutionDefinition.SkillEffectPrefab = source.SkillEffectPrefab;
		passiveSkillExecutionDefinition.BaseModifierChoices = SkillChoiceCompiler.Compile(source.BaseModifierChoices);
		passiveSkillExecutionDefinition.EnhancementChoices = SkillChoiceCompiler.Compile(source.EnhancementChoices);
		passiveSkillExecutionDefinition.MasterChoices = Array.Empty<SkillChoice>();
		passiveSkillExecutionDefinition.MultiEffects = source.PassiveEffects;
		StatusRuntimeCompiler.CompileSkillEffects(passiveSkillExecutionDefinition.MultiEffects);
		SkillTriggerDefinition[] triggers = null;
		if (monster != null)
		{
			triggers = monster.SkillTriggers;
		}
		passiveSkillExecutionDefinition.SkillTriggers = FilterSkillTriggersForSkill(triggers, source.PassiveId);
		StatusRuntimeCompiler.CompileTriggers(passiveSkillExecutionDefinition.SkillTriggers);
		passiveSkillExecutionDefinition.NormalizedPlanNodes = SkillNodeMapper.MapSkillNodeDefinitions(source.NormalizedPlanNodes);
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
			ApplySingleBasePlanNodes(singleSkillExecutionDefinition, source.NormalizedPlanNodes, source.Attribute);
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
	 * ApplySingleBasePlanNodes 처리를 대상에 적용한다.
	 */
	private static void ApplySingleBasePlanNodes(SingleSkillDefinition single /* 단일 */, SkillNodeDefinition[] nodes /* 노드 목록 */, DamageAttribute attribute /* 피해 속성 */)
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


/*
 * 작성 데이터의 SkillNodeDefinition을 전투 실행용 SkillNode로 변환한다.
 * 노드 종류와 값을 해석해 SkillNode 전투 실행 값으로 옮긴다.
 */
namespace Pakuri.InGame
{
    public static class SkillNodeMapper
    {
	/*
	 * MapSkillNodeDefinitions에 필요한 형식으로 변환해 반환한다.
	 */
	public static SkillNode[] MapSkillNodeDefinitions(SkillNodeDefinition[] source /* 변환할 스킬 노드 정의 목록 */)
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
	 * 선택지의 정리된 노드를 대상 스킬별 실행 값으로 한 번만 변환해 반환한다.
	 * 모든 처리기는 SkillNode로 변환하므로 별도 임시 SkillChoice 객체를 만들지 않는다.
	 */
	internal static SkillNode[] GetChoiceRuntimeNodes(SkillChoice choice /* 적용할 선택지 */, string targetSkillId /* 적용 대상 스킬 식별자 */)
	{
		if (choice == null || choice.Source == null)
		{
			return Array.Empty<SkillNode>();
		}

		if (choice.TryGetRuntimeNodes(targetSkillId, out var cached))
		{
			return cached;
		}

		SkillNodeDefinition[] filtered = FilterSkillNodeDefinitionsForTarget(
			choice.Source.NormalizedPlanNodes,
			targetSkillId);
		SkillNode[] nodes = MapSkillNodeDefinitions(filtered);
		choice.CacheRuntimeNodes(targetSkillId, nodes);
		return nodes;
	}

	/*
	 * FilterSkillNodeDefinitionsForTarget에 해당하는 값을 찾아 반환한다.
	 */
	public static SkillNodeDefinition[] FilterSkillNodeDefinitionsForTarget(SkillNodeDefinition[] source /* 변환할 스킬 노드 정의 목록 */, string targetSkillId /* 대상 스킬 식별자 */)
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
	public static bool HasSkillNodeForTarget(SkillNodeDefinition[] source /* 변환할 스킬 노드 정의 목록 */, string targetSkillId /* 대상 스킬 식별자 */)
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
	internal static bool CanProcessPlanNode(string ownerKind /* 소유자 종류 */, string handlerId /* 처리기 식별자 */)
	{
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
	private static SkillNode MapSkillNodeDefinition(SkillNodeDefinition node /* 노드 */)
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
		if (string.Equals(node.OwnerKind, "Skill", StringComparison.OrdinalIgnoreCase)
			&& IsSingleBaseFieldHandler(text))
		{
			return null;
		}
		if (string.Equals(text, "TargetHealthRatioCondition", StringComparison.OrdinalIgnoreCase))
		{
			return SkillNode.FromOperation(new CastConditionOp(GetFloatParam(node, "threshold", 0f)));
		}
		if (string.Equals(text, "TargetHealthRatioThresholdBonus", StringComparison.OrdinalIgnoreCase))
		{
			return SkillNode.FromOperation(new CastConditionOp(GetFloatParam(node, "threshold_bonus", 0f)));
		}
		if (string.Equals(text, "ExecuteDamageMultiplier", StringComparison.OrdinalIgnoreCase))
		{
			return SkillNode.FromOperation(new DamageModifierOp(DamageModifierOpKind.ExecuteMultiplier, GetFloatParam(node, "multiplier", 1f)));
		}
		if (string.Equals(text, "TargetPredicateDamageMultiplier", StringComparison.OrdinalIgnoreCase) && string.Equals(GetParam(node, "predicate"), "is_boss", StringComparison.OrdinalIgnoreCase))
		{
			return SkillNode.FromOperation(new DamageModifierOp(DamageModifierOpKind.BossMultiplier, GetFloatParam(node, "multiplier", 1f)));
		}
		if (string.Equals(text, "BossDamageMultiplier", StringComparison.OrdinalIgnoreCase))
		{
			return SkillNode.FromOperation(new DamageModifierOp(DamageModifierOpKind.BossMultiplier, GetFloatParam(node, "multiplier", 1f)));
		}
		if (string.Equals(text, "ExecuteCritChanceBonus", StringComparison.OrdinalIgnoreCase))
		{
			return SkillNode.FromOperation(new CritModifierOp(GetFloatParam(node, "crit_chance_bonus", 0f)));
		}
		if (string.Equals(text, "CooldownReset", StringComparison.OrdinalIgnoreCase) || string.Equals(text, "CooldownResetOnKill", StringComparison.OrdinalIgnoreCase))
		{
			return SkillNode.FromOperation(new KillActionOp(KillActionOpKind.CooldownReset, 0f, GetBoolParam(node, "requires_execute", defaultValue: false)));
		}
		if (string.Equals(text, "CooldownRefund", StringComparison.OrdinalIgnoreCase))
		{
			return SkillNode.FromOperation(new KillActionOp(KillActionOpKind.CooldownRefundBonus, GetFloatParam(node, "ratio", 0f), requiresExecute: false));
		}
		if (string.Equals(text, "CooldownRefundBonus", StringComparison.OrdinalIgnoreCase))
		{
			return SkillNode.FromOperation(new KillActionOp(KillActionOpKind.CooldownRefundBonus, GetFloatParam(node, "ratio_bonus", 0f), requiresExecute: false));
		}
		if (string.Equals(text, "CountStatusDamageMultiplier", StringComparison.OrdinalIgnoreCase))
		{
			string statusId = GetParam(node, "status_id");
			StatusEffectKind statusKind = StatusRuntimeCompiler.ParseStatusKind(statusId);
			return SkillNode.FromOperation(new CountStatusDamageActionOp(
				GetEnumParam(node, "target_side", SkillMultiEffectTargetSide.AllAllies),
				statusKind,
				GetFloatParam(node, "amount_per_count", 0f),
				GetIntParam(node, "max_count", 0)));
		}
		if (string.Equals(text, "ConsecutiveHitDamageBonus", StringComparison.OrdinalIgnoreCase))
		{
			return SkillNode.FromOperation(new ConsecutiveHitActionOp(
				GetFloatParam(node, "bonus_rate", 0f),
				GetFloatParam(node, "max_bonus", 0f)));
		}
		if (string.Equals(text, "BranchDamage", StringComparison.OrdinalIgnoreCase))
		{
			return SkillNode.FromOperation(new BranchDamageActionOp(
				GetFloatParam(node, "chance_bonus", 0f),
				GetIntParam(node, "count", 0),
				GetFloatParam(node, "damage_multiplier", 0f),
				GetFloatParam(node, "search_radius", 0f)));
		}
		if (string.Equals(text, "ConditionalDamageMultiplier", StringComparison.OrdinalIgnoreCase))
		{
			string statusId = GetParam(node, "status_id");
			StatusEffectKind statusKind = StatusRuntimeCompiler.ParseStatusKind(statusId);
			return SkillNode.FromOperation(new ConditionalDamageActionOp(
				GetFloatParam(node, "multiplier", 1f),
				statusKind,
				GetIntParam(node, "min_stacks", 1)));
		}
		if (string.Equals(text, "StatusConditionalDamageTakenBonus", StringComparison.OrdinalIgnoreCase))
		{
			string sourceStatusId = GetParam(node, "source_status_id");
			StatusEffectKind sourceStatusKind = StatusRuntimeCompiler.ParseStatusKind(sourceStatusId);
			return SkillNode.FromOperation(new StatusConditionalDamageTakenActionOp(
				GetFloatParam(node, "bonus", 0f),
				sourceStatusKind));
		}
		if (string.Equals(text, "TargetStatusCritBonus", StringComparison.OrdinalIgnoreCase))
		{
			StatusEffectKind statusKind = StatusRuntimeCompiler.ParseStatusKind(GetParam(node, "status_id"));
			return SkillNode.FromOperation(new ConditionalCritChanceActionOp(
				GetFloatParam(node, "crit_chance_bonus", 0f),
				statusKind,
				GetIntParam(node, "min_stacks", 0)));
		}
		if (string.Equals(text, "BurstDamageRule", StringComparison.OrdinalIgnoreCase))
		{
			return SkillNode.FromOperation(new BurstDamageActionOp(
				GetIntParam(node, "projectile_index", 0),
				GetFloatParam(node, "multiplier", 1f)));
		}
		if (string.Equals(text, "BurstStatusStacksBonus", StringComparison.OrdinalIgnoreCase))
		{
			return SkillNode.FromOperation(new BurstStatusActionOp(
				GetIntParam(node, "projectile_index", 0),
				GetIntParam(node, "bonus", 0)));
		}
		if (string.Equals(text, "FollowUpProjectile", StringComparison.OrdinalIgnoreCase))
		{
			return SkillNode.FromOperation(new FollowUpProjectileActionOp(
				GetIntParam(node, "count", 0),
				GetFloatParam(node, "delay_seconds", 0f),
				GetFloatParam(node, "damage_multiplier", 1f)));
		}
		if (string.Equals(text, "ThresholdApplyStatus", StringComparison.OrdinalIgnoreCase))
		{
			StatusEffectKind sourceStatus = StatusRuntimeCompiler.ParseStatusKind(GetParam(node, "source_status_id"));
			StatusEffectKind appliedStatus = StatusRuntimeCompiler.ParseStatusKind(GetParam(node, "apply_status_id"));
			return SkillNode.FromOperation(new ThresholdStatusActionOp(
				sourceStatus,
				GetIntParam(node, "min_stacks", 0),
				appliedStatus));
		}
		if (string.Equals(text, "RepeatPerTarget", StringComparison.OrdinalIgnoreCase))
		{
			return SkillNode.FromOperation(new RepeatPerTargetActionOp(
				GetIntParam(node, "repeat_count", 0),
				GetFloatParam(node, "repeat_interval_seconds", 0f),
				GetFloatParam(node, "repeat_damage_multiplier", 1f)));
		}
		if (string.Equals(text, "RedistributeConsumedStatus", StringComparison.OrdinalIgnoreCase))
		{
			StatusEffectKind statusKind = StatusRuntimeCompiler.ParseStatusKind(GetParam(node, "status_id"));
			return SkillNode.FromOperation(new RedistributeConsumedStatusActionOp(
				GetFloatParam(node, "ratio", 0f),
				statusKind,
				GetFloatParam(node, "radius", 0f),
				GetIntParam(node, "target_count", 0)));
		}
		if (string.Equals(text, "AdditionalDamage", StringComparison.OrdinalIgnoreCase))
		{
			string target = GetParam(node, "target");
			if (string.IsNullOrWhiteSpace(target))
			{
				target = GetParam(node, "target_side");
			}
			return SkillNode.FromOperation(new AdditionalDamageActionOp(
				GetFloatParam(node, "chance", 1f),
				GetFloatParam(node, "multiplier", 1f),
				GetEnumParam(node, "attribute", DamageAttribute.Physical),
				target));
		}
		if (string.Equals(text, "CoreDamageMultiplier", StringComparison.OrdinalIgnoreCase))
		{
			return SkillNode.FromOperation(new CoreDamageActionOp(
				GetParam(node, "hitbox_name"),
				GetFloatParam(node, "multiplier", 1f)));
		}
		if (string.Equals(text, "CoreAdditionalDamage", StringComparison.OrdinalIgnoreCase))
		{
			return SkillNode.FromOperation(new CoreAdditionalDamageActionOp(
				GetParam(node, "hitbox_name"),
				GetFloatParam(node, "chance", 1f),
				GetFloatParam(node, "multiplier", 1f),
				GetEnumParam(node, "attribute", DamageAttribute.Physical)));
		}
		if (string.Equals(text, "EveryNthHitChainDamage", StringComparison.OrdinalIgnoreCase))
		{
			return SkillNode.FromOperation(new HitChainDamageActionOp(
				GetIntParam(node, "hit_count", 0),
				GetIntParam(node, "max_targets", 0),
				GetFloatParam(node, "radius", 0f),
				GetFloatParam(node, "multiplier", 1f),
				GetEnumParam(node, "attribute", DamageAttribute.Physical)));
		}
		if (string.Equals(text, "HitCountCooldownRefund", StringComparison.OrdinalIgnoreCase))
		{
			return SkillNode.FromOperation(new HitCountCooldownRefundActionOp(
				GetParam(node, "target_skill_id"),
				GetIntParam(node, "min_targets", 0),
				GetFloatParam(node, "ratio", 0f)));
		}
		if (string.Equals(text, "ReloadReducePerHit", StringComparison.OrdinalIgnoreCase))
		{
			return SkillNode.FromOperation(new ReloadReducePerHitActionOp(
				GetParam(node, "target_skill_id"),
				GetFloatParam(node, "seconds_per_hit", 0f)));
		}
		if (string.Equals(text, "RequiredSourceStatus", StringComparison.OrdinalIgnoreCase))
		{
			StatusEffectKind statusKind = StatusRuntimeCompiler.ParseStatusKind(GetParam(node, "status_id"));
			return SkillNode.FromOperation(new SourceStatusRequirementOp(
				statusKind,
				GetIntParam(node, "min_stacks", 1)));
		}
		var skillActionOp = MapSkillActionOp(node, text);
		return SkillNode.FromOperation(skillActionOp);
	}

	/*
	 * IsSingleBaseFieldHandler 조건을 만족하는지 확인한다.
	 */
	private static bool IsSingleBaseFieldHandler(string handlerId /* 처리기 식별자 */)
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
	private static bool IsRuntimePlanHandler(string handlerId /* 처리기 식별자 */)
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
			|| string.Equals(handlerId, "HitTargetCountBonus", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "LineCastRepeatCountBonus", StringComparison.OrdinalIgnoreCase))
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
		if (string.Equals(handlerId, "BurstDamageRule", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "FollowUpProjectile", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "ThresholdApplyStatus", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "TargetStatusStackDamageMultiplier", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "ConsumeTargetStatusRatioOverride", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "BurstStatusStacksBonus", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "RepeatPerTarget", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "TargetStatusCritBonus", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "RedistributeConsumedStatus", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "AdditionalDamage", StringComparison.OrdinalIgnoreCase)
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
	 * MapSkillActionOp에 필요한 형식으로 변환해 반환한다.
	 */
	private static SkillActionOp MapSkillActionOp(SkillNodeDefinition node /* 노드 */, string handlerId /* 처리기 식별자 */)
	{
		if (string.Equals(handlerId, "DamageMultiplier", StringComparison.OrdinalIgnoreCase))
		{
			return new SkillActionOp(SkillActionOpKind.DamageMultiplier, GetFloatParam(node, "multiplier", 1f));
		}
		if (string.Equals(handlerId, "ShieldAmountMultiplier", StringComparison.OrdinalIgnoreCase))
		{
			return new SkillActionOp(SkillActionOpKind.ShieldAmountMultiplier, GetFloatParam(node, "multiplier", 1f));
		}
		if (string.Equals(handlerId, "CooldownMultiplier", StringComparison.OrdinalIgnoreCase))
		{
			return new SkillActionOp(SkillActionOpKind.CooldownMultiplier, GetFloatParam(node, "multiplier", 1f));
		}
		if (string.Equals(handlerId, "MagazineBonus", StringComparison.OrdinalIgnoreCase))
		{
			return new SkillActionOp(SkillActionOpKind.MagazineBonus, GetIntParam(node, "bonus", 0));
		}
		if (string.Equals(handlerId, "ReloadTimeMultiplier", StringComparison.OrdinalIgnoreCase))
		{
			return new SkillActionOp(SkillActionOpKind.ReloadTimeMultiplier, GetFloatParam(node, "multiplier", 1f));
		}
		if (string.Equals(handlerId, "PierceBonus", StringComparison.OrdinalIgnoreCase))
		{
			return new SkillActionOp(SkillActionOpKind.PierceBonus, GetIntParam(node, "bonus", 0));
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
			return new SkillActionOp(SkillActionOpKind.AdditionalProjectileBonus, GetIntParam(node, "bonus", 0));
		}
		if (string.Equals(handlerId, "ShotIntervalMultiplier", StringComparison.OrdinalIgnoreCase))
		{
			return new SkillActionOp(SkillActionOpKind.ShotIntervalMultiplier, GetFloatParam(node, "multiplier", 1f));
		}
		if (string.Equals(handlerId, "StatusStackAmountBonus", StringComparison.OrdinalIgnoreCase))
		{
			return new SkillActionOp(SkillActionOpKind.StatusStackAmountBonus, GetIntParam(node, "bonus", 0));
		}
		if (string.Equals(handlerId, "StatusStackAmountSet", StringComparison.OrdinalIgnoreCase))
		{
			return new SkillActionOp(SkillActionOpKind.StatusStackAmountSet, GetIntParam(node, "value", 0));
		}
		if (string.Equals(handlerId, "StatusMaxStacksBonus", StringComparison.OrdinalIgnoreCase))
		{
			return new SkillActionOp(SkillActionOpKind.StatusMaxStacksBonus, GetParam(node, "status_id"), GetIntParam(node, "bonus", 0));
		}
		if (string.Equals(handlerId, "TargetStatusStackDamageRateBonus", StringComparison.OrdinalIgnoreCase))
		{
			return new SkillActionOp(SkillActionOpKind.TargetStatusStackDamageRateBonus, GetParam(node, "status_id"), GetFloatParam(node, "bonus_rate_per_stack", 0f));
		}
		if (string.Equals(handlerId, "TriggerProcChanceBonus", StringComparison.OrdinalIgnoreCase))
		{
			return new SkillActionOp(SkillActionOpKind.TriggerProcChanceBonus, GetParam(node, "trigger_id"), GetFloatParam(node, "bonus", 0f));
		}
		if (string.Equals(handlerId, "HitTargetCountBonus", StringComparison.OrdinalIgnoreCase))
		{
			return new SkillActionOp(SkillActionOpKind.HitTargetCountBonus, GetIntParam(node, "bonus", 0));
		}
		if (string.Equals(handlerId, "LineCastRepeatCountBonus", StringComparison.OrdinalIgnoreCase))
		{
			return new SkillActionOp(SkillActionOpKind.LineCastRepeatCountBonus, GetIntParam(node, "bonus", 0));
		}
		if (string.Equals(handlerId, "StatusActionSpeedBonus", StringComparison.OrdinalIgnoreCase))
		{
			return new SkillActionOp(SkillActionOpKind.StatusActionSpeedBonus, GetParam(node, "status_id"), GetFloatParam(node, "bonus", 0f));
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
			return new SkillActionOp(SkillActionOpKind.StatusDurationBonus, GetParam(node, "status_id"), GetFloatParam(node, "bonus_seconds", 0f));
		}
		if (string.Equals(handlerId, "StatusElementDamageTakenBonus", StringComparison.OrdinalIgnoreCase))
		{
			return new SkillActionOp(SkillActionOpKind.StatusElementDamageTakenBonus, GetFloatParam(node, "bonus", 0f));
		}
		if (string.Equals(handlerId, "StatusCriticalDamageTakenBonus", StringComparison.OrdinalIgnoreCase))
		{
			return new SkillActionOp(SkillActionOpKind.StatusCriticalDamageTakenBonus, GetFloatParam(node, "bonus", 0f));
		}
		if (string.Equals(handlerId, "CritChanceBonus", StringComparison.OrdinalIgnoreCase))
		{
			return new SkillActionOp(SkillActionOpKind.CritChanceBonus, GetFloatParam(node, "bonus", 0f));
		}
		if (string.Equals(handlerId, "CritDamageBonus", StringComparison.OrdinalIgnoreCase))
		{
			return new SkillActionOp(SkillActionOpKind.CritDamageBonus, GetFloatParam(node, "bonus", 0f));
		}
		if (string.Equals(handlerId, "BeamWidthBonus", StringComparison.OrdinalIgnoreCase))
		{
			return new SkillActionOp(SkillActionOpKind.BeamWidthBonus, GetFloatParam(node, "bonus", 0f));
		}
		if (string.Equals(handlerId, "KnockbackDistanceMultiplier", StringComparison.OrdinalIgnoreCase))
		{
			return new SkillActionOp(SkillActionOpKind.KnockbackDistanceMultiplier, GetFloatParam(node, "multiplier", 1f));
		}
		if (string.Equals(handlerId, "TargetStatusStackDamageMultiplier", StringComparison.OrdinalIgnoreCase))
		{
			return new SkillActionOp(SkillActionOpKind.TargetStatusStackDamageMultiplier, GetFloatParam(node, "multiplier", 1f));
		}
		if (string.Equals(handlerId, "ConsumeTargetStatusRatioOverride", StringComparison.OrdinalIgnoreCase))
		{
			return new SkillActionOp(SkillActionOpKind.ConsumeTargetStatusRatioOverride, GetFloatParam(node, "ratio", 0f));
		}
		throw new InvalidOperationException("Unsupported skill node handler: " + handlerId);
	}

	/*
	 * GetParam에 해당하는 값을 찾아 반환한다.
	 */
	internal static string GetParam(SkillNodeDefinition node /* 노드 */, string key /* 조회 키 */)
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
	internal static float GetFloatParam(SkillNodeDefinition node /* 노드 */, string key /* 조회 키 */, float defaultValue /* 값이 없을 때 사용할 기본값 */)
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
	internal static int GetIntParam(SkillNodeDefinition node /* 노드 */, string key /* 조회 키 */, int defaultValue /* 값이 없을 때 사용할 기본값 */)
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
	internal static bool GetBoolParam(SkillNodeDefinition node /* 노드 */, string key /* 조회 키 */, bool defaultValue /* 값이 없을 때 사용할 기본값 */)
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
	internal static T GetEnumParam<T>(SkillNodeDefinition node /* 노드 */, string key /* 조회 키 */, T defaultValue /* 값이 없을 때 사용할 기본값 */) where T : struct
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
 * 스킬 전체 변환을 조율하는 SkillDefinitionCompiler와 달리 선택지 필드 변환만 담당한다.
 */
namespace Pakuri.InGame
{
    internal static class SkillChoiceCompiler
    {
	/*
	 * Compile 작업 결과를 반환한다.
	 */
	internal static SkillChoice[] Compile(SkillChoiceDefinition[] source /* 변환할 스킬 선택지 정의 목록 */)
	{
		SkillChoice[] array = new SkillChoice[source.Length];
		for (int i = 0; i < source.Length; i++)
		{
			SkillChoiceDefinition skillChoiceDefinition = source[i];
			array[i] = new SkillChoice
			{
				Source = skillChoiceDefinition
			};
		}
		return array;
	}

    }
}
