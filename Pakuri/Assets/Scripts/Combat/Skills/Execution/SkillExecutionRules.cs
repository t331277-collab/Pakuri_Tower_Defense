/*
 * 역할: 설계된 스킬 규칙을 실행 가능한 값으로 해석한다.
 * 기본 노드와 패시브, 강화, 마스터를 합성하고 시전과 적중 시점의 판정값을 계산한다.
 */

using System;
using System.Collections.Generic;
using Pakuri.Combat;
using Pakuri.Data;
using UnityEngine;

namespace Pakuri.InGame
{

    /// 설계 규칙을 시전과 적중에서 바로 사용할 값으로 확정한다.
    public static class SkillExecutionRules
    {

        internal static DamageAttribute ResolveSkillAttribute(
            UnitCombatState caster,
            DamageAttribute authoredAttribute)
        {
            return caster != null && caster.SkillDamageAttributeOverride.HasValue
                ? caster.SkillDamageAttributeOverride.Value
                : authoredAttribute;
        }

        /// 원본 스킬의 기본 규칙만 반영한 실행 기준을 만든다.
        public static SkillExecutionState CreateDefinitionSnapshot(SkillDefinition source)
        {
            var snapshot = new SkillExecutionState(source);
            if (source != null)
            {
                ApplyNodes(snapshot, source.Nodes, source.SkillName);
            }
            return snapshot;
        }

        /// 선택된 학습 효과를 현재 실행 기준에 누적한다.
        public static void ApplyChoice(SkillExecutionState snapshot, SkillChoice choice)
        {
            if (snapshot == null || choice == null || choice.Nodes == null)
            {
                return;
            }
            if (choice.SkillEffectPrefab != null)
            {
                snapshot.SkillEffectPrefab = choice.SkillEffectPrefab;
            }
            ApplyNodes(snapshot, choice.Nodes, snapshot.SkillName);
        }

        /// 소유자가 실제로 배운 모든 효과를 순서대로 합성한다.
        internal static SkillExecutionState BuildExecutionData(
            UnitCombatState owner,
            SkillExecutionState runtime,
            UnitSpawnManager roster,
            bool isTrigger = false)
        {
            var skill = runtime != null ? runtime.Data : null;
            var snapshot = CreateDefinitionSnapshot(skill);
            snapshot.IsTrigger = isTrigger;
            if (skill == null || owner == null || owner.Skills == null)
            {
                return snapshot;
            }

            foreach (var passiveName in owner.Skills.LearnedPassiveSkillNames)
            {
                var passiveRuntime = owner.SkillState.FindBySkillName(passiveName);
                if (passiveRuntime?.Data is PassiveSkillDefinition passive
                    && passive.BaseNodes != null)
                {
                    ApplyNodes(snapshot, passive.BaseNodes, skill.SkillName);
                }
            }

            ApplyChoices(snapshot, owner.Skills.ChosenEnhancementNames, skill, owner, roster);
            ApplyChoices(snapshot, owner.Skills.ChosenMasterSkillNames, skill, owner, roster);
            ApplyArtifactModifiers(snapshot, owner, skill);
            if (runtime.armedReloadDamageMultiplier > 1f)
            {
                snapshot.ApplyDynamicDamageMultiplier(
                    runtime.armedReloadDamageMultiplier);
            }
            ApplySkillRuntimeCritModifiers(snapshot);
            return snapshot;
        }

        /// 스킬 종류 조건은 대상마다 다시 계산하지 않고 실행 snapshot에 한 번 반영한다.
        private static void ApplySkillRuntimeCritModifiers(SkillExecutionState snapshot)
        {
            if (snapshot?.Data == null)
            {
                return;
            }

            var runtimeKind = snapshot.Data.RuntimeKind;
            var actions = snapshot.ConditionalCritActions;
            for (var i = 0; i < actions.Count; i++)
            {
                var action = actions[i];
                if (action.ConditionKind != ConditionalCritConditionKind.SkillRuntimeKind
                    || !MatchesSkillRuntimeKind(action, runtimeKind))
                {
                    continue;
                }

                snapshot.CritChanceBonus += action.ChanceBonus;
                snapshot.CritDamageBonus = CombineCritDamageBonus(
                    snapshot.CritDamageBonus,
                    action.DamageBonus);
            }
        }

        private static bool MatchesSkillRuntimeKind(
            ConditionalCritActionOp action,
            SkillRuntimeKind runtimeKind)
        {
            for (var i = 0; i < action.RuntimeKinds.Length; i++)
            {
                if (action.RuntimeKinds[i] == runtimeKind)
                {
                    return true;
                }
            }

            return false;
        }

        private static void ApplyArtifactModifiers(
            SkillExecutionState snapshot,
            UnitCombatState owner,
            SkillDefinition skill)
        {
            var effectNames = owner?.Artifacts?.ActiveArtifactEffectNames;
            if (snapshot == null || skill == null || effectNames == null)
            {
                return;
            }

            var repeatedBonuses = new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase);
            var aggregatedRepeatEffects = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < effectNames.Count; i++)
            {
                if (!TryGetArtifactSkillModifier(
                        effectNames[i], owner, skill, snapshot.IsTrigger, out var effect)
                    || effect.RepeatRule == ArtifactEffectRepeatRule.None
                    || aggregatedRepeatEffects.Contains(effect.EffectName)
                    || !TryGetDirectDamageBonus(effect.Nodes, skill.SkillName, out var bonus))
                {
                    continue;
                }

                var repeatCount = CountEffect(effectNames, effect.EffectName);
                repeatedBonuses.TryGetValue(effect.ArtifactName, out var currentBonus);
                repeatedBonuses[effect.ArtifactName] = currentBonus + bonus * repeatCount;
                aggregatedRepeatEffects.Add(effect.EffectName);
            }

            var combinedArtifacts = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < effectNames.Count; i++)
            {
                if (!TryGetArtifactSkillModifier(
                        effectNames[i], owner, skill, snapshot.IsTrigger, out var effect)
                    || aggregatedRepeatEffects.Contains(effect.EffectName))
                {
                    continue;
                }

                if (repeatedBonuses.TryGetValue(effect.ArtifactName, out var repeatedBonus)
                    && !combinedArtifacts.Contains(effect.ArtifactName)
                    && TryGetDirectDamageBonus(effect.Nodes, skill.SkillName, out var directBonus))
                {
                    snapshot.ApplyDynamicDamageMultiplier(1f + directBonus + repeatedBonus);
                    combinedArtifacts.Add(effect.ArtifactName);
                    continue;
                }

                ApplyNodes(snapshot, effect.Nodes, skill.SkillName);
            }

            foreach (var pair in repeatedBonuses)
            {
                if (!combinedArtifacts.Contains(pair.Key))
                {
                    snapshot.ApplyDynamicDamageMultiplier(1f + pair.Value);
                }
            }

            ApplyArtifactSynergyModifiers(snapshot, owner, skill);
        }

        private static void ApplyArtifactSynergyModifiers(
            SkillExecutionState snapshot,
            UnitCombatState owner,
            SkillDefinition skill)
        {
            var effectNames = owner?.Artifacts?.ActiveArtifactEffectNames;
            var catalog = GameDataLoader.CurrentCatalog;
            if (snapshot == null || skill == null || effectNames == null || catalog == null)
            {
                return;
            }

            for (var i = 0; i < effectNames.Count; i++)
            {
                if (!catalog.TryGetData(effectNames[i], out ArtifactSynergyEffectDefinition effect)
                    || effect == null
                    || effect.ApplicationMode != ArtifactEffectApplicationMode.SkillModifier
                    || (effect.TargetSkill != null
                        && !string.Equals(
                            effect.TargetSkill.SkillName,
                            skill.SkillName,
                            StringComparison.OrdinalIgnoreCase))
                    || !ArtifactCombatRules.ConditionsMatch(
                        effect.Nodes,
                        owner,
                        skill,
                        snapshot.IsTrigger))
                {
                    continue;
                }

                ApplyNodes(snapshot, effect.Nodes, skill.SkillName);
            }
        }

        /// trigger 결과에 기본 override를 먼저 넣고 활성 시너지 modifier만 누적한다.
        internal static SkillExecutionState BuildTriggeredSynergyExecutionData(
            UnitCombatState effectOwner,
            SkillExecutionState runtime,
            SkillCastEffect effect)
        {
            var snapshot = CreateDefinitionSnapshot(runtime?.Data);
            snapshot.IsTrigger = true;
            if (effect?.HasRawDamageOverride == true)
            {
                snapshot.SetRawDamageOverride(effect.RawDamageOverride);
            }
            if (effect?.HasDamageAttributeOverride == true)
            {
                snapshot.HasDamageAttributeOverride = true;
                snapshot.DamageAttributeOverride = effect.DamageAttributeOverride;
            }
            if (effect?.HasDamageDelayOverride == true)
            {
                snapshot.HasDamageDelayOverride = true;
                snapshot.DamageDelayOverride = Mathf.Max(0f, effect.DamageDelayOverride);
            }
            if (runtime?.Data != null)
            {
                ApplyArtifactSynergyModifiers(snapshot, effectOwner, runtime.Data);
            }
            return snapshot;
        }

        private static bool TryGetArtifactSkillModifier(
            string effectName,
            UnitCombatState owner,
            SkillDefinition skill,
            bool isTrigger,
            out ArtifactEffectDefinition effect)
        {
            return GameDataLoader.CurrentCatalog.TryGetData(effectName, out effect)
                && effect != null
                && effect.ApplicationMode == ArtifactEffectApplicationMode.SkillModifier
                && (effect.TargetSkill == null
                    || string.Equals(
                        effect.TargetSkill.SkillName,
                        skill.SkillName,
                        StringComparison.OrdinalIgnoreCase))
                && ArtifactCombatRules.ConditionsMatch(
                    effect.Nodes,
                    owner,
                    skill,
                    isTrigger);
        }

        private static bool TryGetDirectDamageBonus(
            IReadOnlyList<SkillNode> nodes,
            string targetSkillName,
            out float bonus)
        {
            bonus = 0f;
            var found = false;
            for (var i = 0; nodes != null && i < nodes.Count; i++)
            {
                var node = nodes[i];
                if (node == null
                    || (!string.IsNullOrWhiteSpace(targetSkillName)
                        && !string.IsNullOrWhiteSpace(node.TargetSkillName)
                        && !string.Equals(node.TargetSkillName, targetSkillName, StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }

                var action = node.GetOperation<SkillActionOp>();
                if (!action.HasValue || action.Value.Kind != SkillActionOpKind.DamageMultiplier)
                {
                    return false;
                }

                bonus += PositiveOrDefault(action.Value.Amount, 1f) - 1f;
                found = true;
            }

            return found;
        }

        private static int CountEffect(IReadOnlyList<string> effectNames, string effectName)
        {
            var count = 0;
            for (var i = 0; i < effectNames.Count; i++)
            {
                if (string.Equals(effectNames[i], effectName, StringComparison.OrdinalIgnoreCase))
                {
                    count++;
                }
            }

            return count;
        }

        /// 학습이 끝난 스킬의 고정 실행값을 런타임 상태에 기록한다.
        internal static void InitializeRuntimeValues(
            SkillExecutionState runtime,
            SkillExecutionState snapshot)
        {
            if (runtime == null)
            {
                return;
            }

            var previousMax = runtime.effectiveMaxMagazineSize;
            var nextMax = CalculateMaxMagazineSize(runtime.Data);
            var nextBurst = BurstProjectileCount(runtime.Data);
            runtime.effectiveReloadDuration = CalculateReloadDuration(runtime.Data);
            runtime.effectiveTickInterval = TickInterval(runtime.Data);
            runtime.effectiveBurstInterval = BurstInterval(runtime.Data);
            runtime.effectiveCooldownDuration = CooldownDuration(runtime.Data);

            if (snapshot != null)
            {
                nextMax = Math.Max(0, nextMax + snapshot.MagazineBonus);
                if (nextBurst > 1)
                {
                    nextBurst += snapshot.AdditionalProjectileBonus;
                }

                runtime.effectiveReloadDuration *= Mathf.Max(
                    0f,
                    snapshot.ReloadTimeMultiplier);
                runtime.effectiveTickInterval *= Mathf.Max(
                    0f,
                    snapshot.ShotIntervalMultiplier);
                runtime.effectiveBurstInterval *= Mathf.Max(
                    0f,
                    snapshot.ShotIntervalMultiplier);
                runtime.effectiveCooldownDuration *= Mathf.Max(
                    0f,
                    snapshot.CooldownMultiplier);
            }

            runtime.effectiveMaxMagazineSize = nextMax;
            runtime.effectiveBurstProjectileCount = Math.Max(1, nextBurst);
            if (previousMax == runtime.effectiveMaxMagazineSize)
            {
                return;
            }

            if (runtime.effectiveMaxMagazineSize <= 0)
            {
                runtime.MagazineRemaining = 0;
                runtime.ReloadRemaining = 0f;
                return;
            }

            if (previousMax <= 0)
            {
                runtime.MagazineRemaining = runtime.effectiveMaxMagazineSize;
                return;
            }

            var delta = runtime.effectiveMaxMagazineSize - previousMax;
            runtime.MagazineRemaining = Mathf.Clamp(
                runtime.MagazineRemaining + delta,
                0,
                runtime.effectiveMaxMagazineSize);
            if (runtime.MagazineRemaining > 0)
            {
                runtime.ReloadRemaining = 0f;
            }
        }

        /// 정의된 탄창 용량을 실행 가능한 값으로 만든다.
        private static int CalculateMaxMagazineSize(SkillDefinition data)
        {
            return data != null ? Math.Max(0, data.MagazineCapacity) : 0;
        }

        /// 한 시전에 이어지는 발사 횟수를 계산한다.
        private static int BurstProjectileCount(SkillDefinition data)
        {
            var projectile = data as ProjectileSkillDefinition;
            return projectile?.Projectile != null
                ? Math.Max(1, projectile.Projectile.BurstProjectileCount)
                : 1;
        }

        /// 재장전 시간을 실행 가능한 값으로 만든다.
        private static float CalculateReloadDuration(SkillDefinition data)
        {
            return data != null ? Mathf.Max(0f, data.ReloadSeconds) : 0f;
        }

        /// 주기 실행 간격을 계산한다.
        private static float TickInterval(SkillDefinition data)
        {
            return data?.Timing != null
                ? Mathf.Max(0f, data.Timing.TickInterval)
                : 0f;
        }

        /// 연사 간격을 계산한다.
        private static float BurstInterval(SkillDefinition data)
        {
            var projectile = data as ProjectileSkillDefinition;
            if (projectile?.Projectile != null
                && projectile.Projectile.BurstIntervalSeconds > 0f)
            {
                return projectile.Projectile.BurstIntervalSeconds;
            }

            return TickInterval(data);
        }

        /// 재사용 대기시간을 계산한다.
        private static float CooldownDuration(SkillDefinition data)
        {
            return data?.Timing != null
                ? Mathf.Max(0f, data.Timing.Cooldown)
                : 0f;
        }

        /// 지속 강화의 공통 보정을 실행값에 합친다.
        /// 선택된 강화 목록을 실행값에 반영한다.
        private static void ApplyChoices(
            SkillExecutionState snapshot,
            IReadOnlyCollection<string> choiceNames,
            SkillDefinition skill,
            UnitCombatState owner,
            UnitSpawnManager roster)
        {
            if (snapshot == null || choiceNames == null || skill == null || owner?.SkillState == null)
            {
                return;
            }

            foreach (var choiceName in choiceNames)
            {
                var choice = owner.SkillState.FindChoice(choiceName);
                if (AppliesToSkill(choice, skill))
                {
                    snapshot.AddActiveChoiceName(choice.ChoiceName);
                    ApplyChoice(snapshot, choice);
                    ApplyDynamicChoiceRules(snapshot, choice, owner, roster);
                }
            }
        }

        /// 현재 전투 대상 수에 따른 보정을 확정한다.
        internal static void ApplyDynamicChoiceRules(
            SkillExecutionState snapshot,
            SkillChoice choice,
            UnitCombatState owner,
            UnitSpawnManager roster)
        {
            if (snapshot == null || choice?.Nodes == null || roster == null)
            {
                return;
            }

            for (var i = 0; i < choice.Nodes.Length; i++)
            {
                var node = choice.Nodes[i];
                if (node == null || !string.Equals(node.TargetSkillName, snapshot.SkillName, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var action = node.GetOperation<CountStatusDamageActionOp>();
                if (action.HasValue)
                {
                    var count = CountMatchingTargets(owner, roster, action.Value.TargetSide, action.Value.StatusKind);
                    if (action.Value.MaximumCount > 0)
                    {
                        count = Mathf.Min(count, action.Value.MaximumCount);
                    }
                    if (count > 0)
                    {
                        snapshot.ApplyDynamicDamageMultiplier(1f + count * action.Value.AmountPerCount);
                    }
                }
            }
        }

        /// 조건을 만족하는 현재 대상 수를 센다.
        private static int CountMatchingTargets(
            UnitCombatState owner,
            UnitSpawnManager roster,
            SkillMultiEffectTargetSide side,
            StatusEffectKind statusKind)
        {
            if (owner == null || roster == null || statusKind == StatusEffectKind.None)
            {
                return 0;
            }

            var entries = CountEntries(owner, roster, side);
            var count = 0;
            for (var i = 0; i < entries.Count; i++)
            {
                if (entries[i] != null && entries[i].IsAlive && HasStatus(entries[i].Model, statusKind))
                {
                    count++;
                }
            }
            return count;
        }

        /// 대상 수 계산에 사용할 후보를 모은다.
        private static IReadOnlyList<CombatUnitEntry> CountEntries(
            UnitCombatState owner,
            UnitSpawnManager roster,
            SkillMultiEffectTargetSide side)
        {
            if (owner?.Identity == null || roster == null)
            {
                return Array.Empty<CombatUnitEntry>();
            }

            var ownerIsEnemy = owner.Identity.Side == UnitSide.Enemy;
            switch (side)
            {
                case SkillMultiEffectTargetSide.Self:
                    var allies = ownerIsEnemy ? roster.Enemies : roster.Players;
                    var self = FindEntryForModel(owner, allies);
                    return IsSkillTarget(self) ? new[] { self } : Array.Empty<CombatUnitEntry>();
                case SkillMultiEffectTargetSide.AllAllies:
                    return FilterSkillTargets(ownerIsEnemy ? roster.Enemies : roster.Players);
                default:
                    return FilterSkillTargets(ownerIsEnemy ? roster.Players : roster.Enemies);
            }
        }

        /// 실제 스킬 대상만 남긴다.
        private static IReadOnlyList<CombatUnitEntry> FilterSkillTargets(IReadOnlyList<CombatUnitEntry> entries)
        {
            if (entries == null || entries.Count == 0)
            {
                return Array.Empty<CombatUnitEntry>();
            }

            var filtered = new List<CombatUnitEntry>();
            for (var i = 0; i < entries.Count; i++)
            {
                if (IsSkillTarget(entries[i]))
                {
                    filtered.Add(entries[i]);
                }
            }
            return filtered;
        }

        /// 항목이 스킬 대상인지 확인한다.
        private static bool IsSkillTarget(CombatUnitEntry entry)
        {
            var role = entry?.Model?.Identity?.Role;
            return entry != null && (role == null || role != UnitRole.Nexus);
        }

        /// 모델에 대응하는 전투 항목을 찾는다.
        private static CombatUnitEntry FindEntryForModel(
            UnitCombatState model,
            IReadOnlyList<CombatUnitEntry> entries)
        {
            if (model == null || entries == null)
            {
                return null;
            }
            for (var i = 0; i < entries.Count; i++)
            {
                if (entries[i] != null && object.ReferenceEquals(entries[i].Model, model))
                {
                    return entries[i];
                }
            }
            return null;
        }

        /// 대상이 지정 상태를 보유하는지 확인한다.
        private static bool HasStatus(UnitCombatState target, StatusEffectKind statusKind)
        {
            if (target == null || statusKind == StatusEffectKind.None)
            {
                return false;
            }
            if (statusKind == StatusEffectKind.Shield)
            {
                return target.Resources != null && target.Resources.CurrentShield > 0f;
            }
            return target.Statuses != null && target.Statuses.GetStacks(statusKind) > 0;
        }

        /// 선택 효과가 현재 스킬에 적용되는지 확인한다.
        private static bool AppliesToSkill(SkillChoice choice, SkillDefinition skill)
        {
            if (choice == null || skill == null)
            {
                return false;
            }
            if (choice.Nodes != null && choice.Nodes.Length > 0)
            {
                for (var i = 0; i < choice.Nodes.Length; i++)
                {
                    if (choice.Nodes[i] != null
                        && string.Equals(choice.Nodes[i].TargetSkillName, skill.SkillName, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
                return false;
            }
            var targetSkillName = string.IsNullOrWhiteSpace(choice.TargetSkillName)
                ? choice.SkillName
                : choice.TargetSkillName;
            return string.Equals(targetSkillName, skill.SkillName, StringComparison.OrdinalIgnoreCase);
        }

	/// 각 노드의 의미를 선언 순서대로 실행 기준에 반영한다.
	internal static void ApplyNodes(SkillExecutionState snapshot, IReadOnlyList<SkillNode> nodes, string targetSkillName = null)
	{

		if (nodes == null || nodes.Count == 0)
		{
			return;
		}
		TargetStatusConditionOp? pendingTargetStatus = null;
		for (int i = 0; i < nodes.Count; i++)
		{
			if (nodes[i] == null
				|| (!string.IsNullOrWhiteSpace(targetSkillName)
					&& !string.IsNullOrWhiteSpace(nodes[i].TargetSkillName)
					&& !string.Equals(nodes[i].TargetSkillName, targetSkillName, StringComparison.OrdinalIgnoreCase)))
			{
				continue;
			}

			var targetStatus = nodes[i].GetOperation<TargetStatusConditionOp>();
			if (targetStatus.HasValue)
			{
				pendingTargetStatus = targetStatus;
				continue;
			}

			CastConditionOp? castCondition = nodes[i].GetOperation<CastConditionOp>();
			if (castCondition.HasValue)
			{
				snapshot.castConditionOps.Add(castCondition.Value);
			}

			DamageModifierOp? damageModifier = nodes[i].GetOperation<DamageModifierOp>();
			if (damageModifier.HasValue)
			{
				snapshot.damageModifierOps.Add(damageModifier.Value);
			}

			CritModifierOp? critModifier = nodes[i].GetOperation<CritModifierOp>();
			if (critModifier.HasValue)
			{
				snapshot.critModifierOps.Add(critModifier.Value);
			}

			KillActionOp? killAction = nodes[i].GetOperation<KillActionOp>();
			if (killAction.HasValue)
			{
				snapshot.killActionOps.Add(killAction.Value);
			}

			SkillCastEffectOp? castEffect = nodes[i].GetOperation<SkillCastEffectOp>();
			if (castEffect.HasValue && castEffect.Value.Effect != null)
			{
				snapshot.castEffects.Add(castEffect.Value.Effect);
			}

			SkillReactionOp? reaction = nodes[i].GetOperation<SkillReactionOp>();
			if (reaction.HasValue && reaction.Value.Reaction != null)
			{
				snapshot.reactions.Add(reaction.Value.Reaction);
			}

			SkillActionOp? skillActionOp = nodes[i].GetOperation<SkillActionOp>();
			if (skillActionOp.HasValue)
			{
				if (pendingTargetStatus.HasValue
					&& skillActionOp.Value.Kind == SkillActionOpKind.DamageMultiplier)
				{
					snapshot.conditionalStatusGroupDamageActions.Add(
						new ConditionalStatusGroupDamageActionOp(
							skillActionOp.Value.Amount,
							pendingTargetStatus.Value.Groups));
					pendingTargetStatus = null;
				}
				else
				{
					ApplyNodeAction(snapshot, skillActionOp.Value);
				}
			}

			DamageAttributeOverrideOp? attributeOverride =
				nodes[i].GetOperation<DamageAttributeOverrideOp>();
			if (attributeOverride.HasValue)
			{
				snapshot.HasDamageAttributeOverride = true;
				snapshot.DamageAttributeOverride = attributeOverride.Value.Attribute;
			}

			ConsecutiveHitActionOp? consecutiveHitAction = nodes[i].GetOperation<ConsecutiveHitActionOp>();
			if (consecutiveHitAction.HasValue)
			{
				ApplyConsecutiveHitAction(snapshot, consecutiveHitAction.Value);
			}

			FirstMagazineProjectileFollowUpActionOp? firstFollowUp =
				nodes[i].GetOperation<FirstMagazineProjectileFollowUpActionOp>();
			if (firstFollowUp.HasValue)
			{
				snapshot.FollowUpProjectileCount += firstFollowUp.Value.Count;
				snapshot.FollowUpProjectileDelaySeconds = firstFollowUp.Value.DelaySeconds;
				snapshot.FollowUpProjectileDamageMultiplier = firstFollowUp.Value.DamageMultiplier;
				snapshot.FollowUpProjectileFirstMagazineOnly = true;
			}

			ArrivalFragmentBurstActionOp? fragments =
				nodes[i].GetOperation<ArrivalFragmentBurstActionOp>();
			if (fragments.HasValue)
			{
				snapshot.ArrivalFragmentCount = fragments.Value.Count;
				snapshot.ArrivalFragmentDelaySeconds = fragments.Value.DelaySeconds;
				snapshot.ArrivalFragmentSearchRadius = fragments.Value.SearchRadius;
				snapshot.ArrivalFragmentRawDamage = fragments.Value.RawDamage;
				snapshot.ArrivalFragmentRadiusMultiplier = snapshot.HasRadiusMultiplierOverride
					? snapshot.RadiusMultiplierOverride
					: snapshot.RadiusMultiplier;
			}

			BranchDamageActionOp? branchDamageAction = nodes[i].GetOperation<BranchDamageActionOp>();
			if (branchDamageAction.HasValue)
			{
				ApplyBranchDamageAction(snapshot, branchDamageAction.Value);
			}

			ConditionalDamageActionOp? conditionalDamageAction = nodes[i].GetOperation<ConditionalDamageActionOp>();
			if (conditionalDamageAction.HasValue)
			{
				ApplyConditionalDamageAction(snapshot, conditionalDamageAction.Value);
			}

		ConditionalCritChanceActionOp? conditionalCritAction = nodes[i].GetOperation<ConditionalCritChanceActionOp>();
		if (conditionalCritAction.HasValue)
		{
			ApplyConditionalCritChanceAction(snapshot, conditionalCritAction.Value);
		}

		ConditionalCritActionOp? conditionalCritAction2 = nodes[i].GetOperation<ConditionalCritActionOp>();
		if (conditionalCritAction2.HasValue)
		{
			snapshot.conditionalCritActions.Add(conditionalCritAction2.Value);
		}

		ConditionalFinalDamageActionOp? conditionalFinalDamageAction = nodes[i].GetOperation<ConditionalFinalDamageActionOp>();
		if (conditionalFinalDamageAction.HasValue)
		{
			snapshot.conditionalFinalDamageActions.Add(conditionalFinalDamageAction.Value);
		}

			BurstDamageActionOp? burstDamageAction = nodes[i].GetOperation<BurstDamageActionOp>();
			if (burstDamageAction.HasValue)
			{
				ApplyBurstDamageAction(snapshot, burstDamageAction.Value);
			}

			BurstStatusActionOp? burstStatusAction = nodes[i].GetOperation<BurstStatusActionOp>();
			if (burstStatusAction.HasValue)
			{
				ApplyBurstStatusAction(snapshot, burstStatusAction.Value);
			}

			StatusConditionalDamageTakenActionOp? statusDamageTakenAction = nodes[i].GetOperation<StatusConditionalDamageTakenActionOp>();
			if (statusDamageTakenAction.HasValue)
			{
				ApplyStatusConditionalDamageTakenAction(snapshot, statusDamageTakenAction.Value);
			}

			FollowUpProjectileActionOp? followUpAction = nodes[i].GetOperation<FollowUpProjectileActionOp>();
			if (followUpAction.HasValue)
			{
				ApplyFollowUpProjectileAction(snapshot, followUpAction.Value);
			}

			ThresholdStatusActionOp? thresholdStatusAction = nodes[i].GetOperation<ThresholdStatusActionOp>();
			if (thresholdStatusAction.HasValue)
			{
				ApplyThresholdStatusAction(snapshot, thresholdStatusAction.Value);
			}

			RepeatPerTargetActionOp? repeatAction = nodes[i].GetOperation<RepeatPerTargetActionOp>();
			if (repeatAction.HasValue)
			{
				ApplyRepeatPerTargetAction(snapshot, repeatAction.Value);
			}

			PullToCenterActionOp? pullAction = nodes[i].GetOperation<PullToCenterActionOp>();
			if (pullAction.HasValue)
			{
				snapshot.PullDistancePerTick = Mathf.Max(
					snapshot.PullDistancePerTick,
					pullAction.Value.DistancePerTick);
			}

			RedistributeConsumedStatusActionOp? redistributeAction = nodes[i].GetOperation<RedistributeConsumedStatusActionOp>();
			if (redistributeAction.HasValue)
			{
				ApplyRedistributeConsumedStatusAction(snapshot, redistributeAction.Value);
			}

			AdditionalDamageActionOp? additionalDamageAction = nodes[i].GetOperation<AdditionalDamageActionOp>();
			if (additionalDamageAction.HasValue)
			{
				ApplyAdditionalDamageAction(snapshot, additionalDamageAction.Value);
			}

			CoreDamageActionOp? coreDamageAction = nodes[i].GetOperation<CoreDamageActionOp>();
			if (coreDamageAction.HasValue)
			{
				ApplyCoreDamageAction(snapshot, coreDamageAction.Value);
			}

			CoreAdditionalDamageActionOp? coreAdditionalDamageAction = nodes[i].GetOperation<CoreAdditionalDamageActionOp>();
			if (coreAdditionalDamageAction.HasValue)
			{
				ApplyCoreAdditionalDamageAction(snapshot, coreAdditionalDamageAction.Value);
			}

			HitChainDamageActionOp? hitChainAction = nodes[i].GetOperation<HitChainDamageActionOp>();
			if (hitChainAction.HasValue)
			{
				ApplyHitChainDamageAction(snapshot, hitChainAction.Value);
			}

			HitCountCooldownRefundActionOp? hitCountRefundAction = nodes[i].GetOperation<HitCountCooldownRefundActionOp>();
			if (hitCountRefundAction.HasValue)
			{
				ApplyHitCountCooldownRefundAction(snapshot, hitCountRefundAction.Value);
			}

			ReloadReducePerHitActionOp? reloadReduceAction = nodes[i].GetOperation<ReloadReducePerHitActionOp>();
			if (reloadReduceAction.HasValue)
			{
				ApplyReloadReducePerHitAction(snapshot, reloadReduceAction.Value);
			}
		}
	}

		/// 수치 변화의 종류에 따라 실행 기준을 갱신한다.
		internal static void ApplyNodeAction(SkillExecutionState snapshot, SkillActionOp action)
	{
		switch (action.Kind)
		{
		case SkillActionOpKind.DamageMultiplier:
			snapshot.DamageMultiplier *= PositiveOrDefault(action.Amount, 1f);
			break;
		case SkillActionOpKind.RawDamageOverride:
			snapshot.SetRawDamageOverride(action.Amount);
			break;
		case SkillActionOpKind.RadiusMultiplierOverride:
			snapshot.HasRadiusMultiplierOverride = true;
			snapshot.RadiusMultiplierOverride = Mathf.Max(0f, action.Amount);
			break;
		case SkillActionOpKind.DamageDelayOverride:
			snapshot.HasDamageDelayOverride = true;
			snapshot.DamageDelayOverride = Mathf.Max(0f, action.Amount);
			break;
		case SkillActionOpKind.ShieldAmountMultiplier:
			snapshot.ShieldAmountMultiplier *= PositiveOrDefault(action.Amount, 1f);
			break;
		case SkillActionOpKind.CooldownMultiplier:
			snapshot.CooldownMultiplier *= PositiveOrDefault(action.Amount, 1f);
			break;
		case SkillActionOpKind.MagazineBonus:
			snapshot.MagazineBonus += action.Count;
			break;
		case SkillActionOpKind.ReloadTimeMultiplier:
			snapshot.ReloadTimeMultiplier *= PositiveOrDefault(action.Amount, 1f);
			break;
		case SkillActionOpKind.ReloadCompleteDamageMultiplier:
			snapshot.ReloadCompleteDamageMultiplier *=
				PositiveOrDefault(action.Amount, 1f);
			break;
		case SkillActionOpKind.PierceBonus:
			snapshot.PierceBonus += action.Count;
			break;
		case SkillActionOpKind.RadiusMultiplier:
			snapshot.RadiusMultiplier *= PositiveOrDefault(action.Amount, 1f);
			break;
		case SkillActionOpKind.RadiusBonus:
			snapshot.RadiusBonus += action.Amount;
			break;
		case SkillActionOpKind.DurationBonus:
			snapshot.DurationBonus += action.Amount;
			break;
		case SkillActionOpKind.DurationMultiplier:
			snapshot.DurationMultiplier *= PositiveOrDefault(action.Amount, 1f);
			break;
		case SkillActionOpKind.DamageDelayMultiplier:
			snapshot.DamageDelayMultiplier *= PositiveOrDefault(action.Amount, 1f);
			break;
		case SkillActionOpKind.AdditionalProjectileBonus:
			snapshot.AdditionalProjectileBonus += action.Count;
			break;
		case SkillActionOpKind.ShotIntervalMultiplier:
			snapshot.ShotIntervalMultiplier *= PositiveOrDefault(action.Amount, 1f);
			break;
		case SkillActionOpKind.StatusStackAmountBonus:
			snapshot.StatusStacksBonus += action.Count;
			break;
		case SkillActionOpKind.StatusStackAmountSet:
			snapshot.HasStatusStacksSet = true;
			snapshot.StatusStacksSet = Mathf.Max(0, action.Count);
			break;
		case SkillActionOpKind.StatusMaxStacksBonus:
			if (!string.IsNullOrWhiteSpace(action.ReferenceName) && action.Count != 0)
			{
				snapshot.statusMaxStacksBonuses.TryGetValue(action.ReferenceName, out var value3);
				snapshot.statusMaxStacksBonuses[action.ReferenceName] = value3 + action.Count;
			}
			break;
		case SkillActionOpKind.TargetStatusStackDamageRateBonus:
			if (!string.IsNullOrWhiteSpace(action.ReferenceName) && !Mathf.Approximately(action.Amount, 0f))
			{
				snapshot.targetStatusStackDamageRateBonuses.TryGetValue(action.ReferenceName, out var value2);
				snapshot.targetStatusStackDamageRateBonuses[action.ReferenceName] = value2 + action.Amount;
			}
			break;
		case SkillActionOpKind.TargetStatusStackDamageMultiplierBonus:
			if (!string.IsNullOrWhiteSpace(action.ReferenceName) && !Mathf.Approximately(action.Amount, 0f))
			{
				snapshot.targetStatusStackDamageMultiplierBonuses.TryGetValue(action.ReferenceName, out var multiplierBonus);
				snapshot.targetStatusStackDamageMultiplierBonuses[action.ReferenceName] = multiplierBonus + action.Amount;
			}
			break;
		case SkillActionOpKind.TriggerProcChanceBonus:
			if (!string.IsNullOrWhiteSpace(action.ReferenceName) && !Mathf.Approximately(action.Amount, 0f))
			{
				snapshot.triggerProcChanceBonuses.TryGetValue(action.ReferenceName, out var value);
				snapshot.triggerProcChanceBonuses[action.ReferenceName] = value + action.Amount;
			}
			break;
		case SkillActionOpKind.HitTargetCountBonus:
			snapshot.HitTargetCountBonus += action.Count;
			break;
		case SkillActionOpKind.LineCastRepeatCountBonus:
			snapshot.LineCastRepeatCountBonus += action.Count;
			break;
		case SkillActionOpKind.StatusActionSpeedBonus:
			ApplyStatusActionSpeedBonus(snapshot, action.ReferenceName, action.Amount);
			break;
		case SkillActionOpKind.StatusActionSpeedMultiplier:
			if (!string.IsNullOrWhiteSpace(action.ReferenceName))
			{
				snapshot.statusActionSpeedMultipliers.TryGetValue(action.ReferenceName, out var speedMultiplier);
				snapshot.statusActionSpeedMultipliers[action.ReferenceName] = speedMultiplier <= 0f
					? PositiveOrDefault(action.Amount, 1f)
					: speedMultiplier * PositiveOrDefault(action.Amount, 1f);
			}
			break;
		case SkillActionOpKind.StatusAttackPowerBonus:
			snapshot.HasStatusAttackPowerBonus = true;
			snapshot.StatusAttackPowerBonus += action.Amount;
			break;
		case SkillActionOpKind.StatusAilmentResistanceBonus:
			snapshot.HasStatusAilmentResistanceBonus = true;
			snapshot.StatusAilmentResistanceBonus += action.Amount;
			break;
		case SkillActionOpKind.StatusDamageBonusRate:
			snapshot.HasStatusDamageBonusRate = true;
			snapshot.StatusDamageBonusRate += action.Amount;
			break;
		case SkillActionOpKind.StatusShieldReceivedBonus:
			snapshot.HasStatusShieldReceivedBonus = true;
			snapshot.StatusShieldReceivedBonus += action.Amount;
			break;
		case SkillActionOpKind.StatusCriticalChanceBonus:
			snapshot.HasStatusCriticalChanceBonus = true;
			snapshot.StatusCriticalChanceBonus += action.Amount;
			break;
		case SkillActionOpKind.StatusDamageTakenBonus:
			snapshot.HasStatusDamageTakenBonus = true;
			snapshot.StatusDamageTakenBonus += action.Amount;
			break;
		case SkillActionOpKind.StatusFlatElementResistReduction:
			snapshot.HasStatusFlatElementResistReduction = true;
			snapshot.StatusFlatElementResistReduction += action.Amount;
			break;
		case SkillActionOpKind.StatusDurationBonus:
			ApplyStatusDurationBonus(snapshot, action.ReferenceName, action.Amount);
			break;
		case SkillActionOpKind.StatusElementDamageTakenBonus:
			snapshot.HasStatusElementDamageTakenBonus = true;
			snapshot.StatusElementDamageTakenBonus += action.Amount;
			break;
		case SkillActionOpKind.StatusCriticalDamageTakenBonus:
			snapshot.HasStatusCriticalDamageTakenBonus = true;
			snapshot.StatusCriticalDamageTakenBonus += action.Amount;
			break;
		case SkillActionOpKind.CritChanceBonus:
			snapshot.CritChanceBonus += action.Amount;
			break;
		case SkillActionOpKind.CritDamageBonus:
			snapshot.CritDamageBonus = (1f + snapshot.CritDamageBonus)
				* Mathf.Max(0f, 1f + action.Amount)
				- 1f;
			break;
		case SkillActionOpKind.MagazineLastProjectileCritDamageBonus:
			snapshot.MagazineLastProjectileCritDamageBonus = CombineCritDamageBonus(
				snapshot.MagazineLastProjectileCritDamageBonus,
				action.Amount);
			break;
		case SkillActionOpKind.MagazineLastProjectileDamageMultiplier:
			snapshot.MagazineLastProjectileDamageMultiplier *=
				PositiveOrDefault(action.Amount, 1f);
			break;
		case SkillActionOpKind.FinalDamageModifier:
			snapshot.FinalDamageModifier *= PositiveOrDefault(action.Amount, 1f);
			break;
		case SkillActionOpKind.CriticalFinalDamageModifier:
			snapshot.CriticalFinalDamageModifier *= PositiveOrDefault(action.Amount, 1f);
			break;
		case SkillActionOpKind.BeamWidthBonus:
			snapshot.BeamWidthBonus += action.Amount;
			break;
		case SkillActionOpKind.KnockbackDistanceMultiplier:
			snapshot.KnockbackDistanceMultiplier *= PositiveOrDefault(action.Amount, 1f);
			break;
		case SkillActionOpKind.TargetStatusStackDamageMultiplier:
			snapshot.TargetStatusStackDamageMultiplier *= PositiveOrDefault(action.Amount, 1f);
			break;
		case SkillActionOpKind.ConsumeTargetStatusRatioOverride:
			snapshot.HasConsumeTargetStatusRatioOverride = true;
			snapshot.ConsumeTargetStatusRatioOverride = Mathf.Clamp01(action.Amount);
			break;
		}
	}

		/// 연속 적중 보정을 실행값에 합친다.
		internal static void ApplyConsecutiveHitAction(SkillExecutionState snapshot, ConsecutiveHitActionOp action)
	{
		snapshot.ConsecutiveHitBonusRate += Mathf.Max(0f, action.BonusRate);
		snapshot.ConsecutiveHitMax += Mathf.Max(0f, action.MaxBonus);
	}

		/// 분기 피해 조건을 실행값에 합친다.
		internal static void ApplyBranchDamageAction(SkillExecutionState snapshot, BranchDamageActionOp action)
	{
		snapshot.BranchChanceBonus += action.ChanceBonus;
		if (action.BranchCount > 0)
		{
			snapshot.HasBranchCount = true;
			snapshot.BranchCount = action.BranchCount;
		}
		if (action.DamageMultiplier > 0f)
		{
			snapshot.HasBranchDamageMultiplier = true;
			snapshot.BranchDamageMultiplier = action.DamageMultiplier;
		}
		if (action.SearchRadius > 0f)
		{
			snapshot.HasBranchSearchRadius = true;
			snapshot.BranchSearchRadius = action.SearchRadius;
		}
	}

		/// 조건부 피해 보정을 실행값에 합친다.
		internal static void ApplyConditionalDamageAction(SkillExecutionState snapshot, ConditionalDamageActionOp action)
	{
		if (action.Condition.StatusKind != StatusEffectKind.None
			&& action.Condition.MinimumStacks > 0
			&& action.DamageMultiplier > 0f)
		{
			snapshot.conditionalDamageActions.Add(action);
		}
	}

		/// 조건부 치명타 보정을 실행값에 합친다.
		internal static void ApplyConditionalCritChanceAction(SkillExecutionState snapshot, ConditionalCritChanceActionOp action)
	{
		if (action.Condition.StatusKind != StatusEffectKind.None
			&& action.Condition.MinimumStacks > 0
			&& !Mathf.Approximately(action.ChanceBonus, 0f))
		{
			snapshot.conditionalCritChanceActions.Add(action);
		}
	}

		/// 폭발 피해 보정을 실행값에 합친다.
		internal static void ApplyBurstDamageAction(SkillExecutionState snapshot, BurstDamageActionOp action)
	{
		if (action.DamageMultiplier > 0f)
		{
			snapshot.burstDamageActions.Add(action);
		}
	}

		/// 폭발 상태 보정을 실행값에 합친다.
		internal static void ApplyBurstStatusAction(SkillExecutionState snapshot, BurstStatusActionOp action)
	{
		if (action.StacksBonus != 0)
		{
			snapshot.burstStatusActions.Add(action);
		}
	}

		/// 상태 조건부 피해 보정을 실행값에 합친다.
		internal static void ApplyStatusConditionalDamageTakenAction(SkillExecutionState snapshot, StatusConditionalDamageTakenActionOp action)
	{
		snapshot.HasStatusConditionalDamageTakenBonus = true;
		snapshot.StatusConditionalDamageTakenBonus += action.Bonus;
		snapshot.StatusConditionalSourceStatusKind = action.RequiredSourceStatus;
	}

		/// 후속 투사체 의미를 실행값에 합친다.
		internal static void ApplyFollowUpProjectileAction(SkillExecutionState snapshot, FollowUpProjectileActionOp action)
	{
		if (action.Count <= 0)
		{
			return;
		}

		snapshot.FollowUpProjectileCount = action.Count;
		snapshot.FollowUpProjectileDelaySeconds = Mathf.Max(0f, action.DelaySeconds);
		snapshot.FollowUpProjectileDamageMultiplier = Mathf.Max(0f, action.DamageMultiplier);
	}

		/// 임계 상태 적용 의미를 실행값에 합친다.
		internal static void ApplyThresholdStatusAction(SkillExecutionState snapshot, ThresholdStatusActionOp action)
	{
		if (action.Condition.StatusKind == StatusEffectKind.None
			|| action.Condition.MinimumStacks <= 0
			|| action.AppliedStatus == StatusEffectKind.None)
		{
			return;
		}

		snapshot.ThresholdStatusKind = action.Condition.StatusKind;
		snapshot.ThresholdStatusMinStacks = action.Condition.MinimumStacks;
		snapshot.ThresholdApplyStatusKind = action.AppliedStatus;
	}

		/// 대상별 반복 실행 의미를 합친다.
		internal static void ApplyRepeatPerTargetAction(SkillExecutionState snapshot, RepeatPerTargetActionOp action)
	{
		if (action.Count <= 0)
		{
			return;
		}

		snapshot.RepeatCountPerTarget += action.Count;
		snapshot.RepeatIntervalSeconds = Mathf.Max(snapshot.RepeatIntervalSeconds, action.IntervalSeconds);
		if (action.DamageMultiplier > 0f)
		{
			snapshot.RepeatDamageMultiplier *= action.DamageMultiplier;
		}
	}

		/// 소비 상태의 재분배 의미를 합친다.
		internal static void ApplyRedistributeConsumedStatusAction(SkillExecutionState snapshot, RedistributeConsumedStatusActionOp action)
	{
		if (action.Ratio <= 0f || action.StatusKind == StatusEffectKind.None || action.SearchRadius <= 0f)
		{
			return;
		}

		snapshot.RedistributeConsumedStatusRatioOnKill = Mathf.Clamp01(action.Ratio);
		snapshot.RedistributeConsumedStatusKind = action.StatusKind;
		snapshot.RedistributeConsumedStatusSearchRadius = Mathf.Max(0f, action.SearchRadius);
		snapshot.RedistributeConsumedStatusTargetCount = Mathf.Max(0, action.TargetCount);
	}

		/// 추가 피해 의미를 실행값에 합친다.
		internal static void ApplyAdditionalDamageAction(SkillExecutionState snapshot, AdditionalDamageActionOp action)
	{
		snapshot.HasOnHitAdditionalDamage = true;
		snapshot.OnHitAdditionalDamageChance = action.Chance;
		snapshot.OnHitAdditionalDamageMultiplier = action.Multiplier;
		snapshot.OnHitAdditionalDamageAttribute = action.Attribute;
		snapshot.OnHitAdditionalDamageTarget = action.Target;
	}

		/// 핵심 피해 의미를 실행값에 합친다.
		internal static void ApplyCoreDamageAction(SkillExecutionState snapshot, CoreDamageActionOp action)
	{
		snapshot.CoreHitboxName = action.HitboxName;
		snapshot.HasCoreDamageMultiplier = true;
		snapshot.CoreDamageMultiplier *= action.Multiplier;
	}

		/// 핵심 적중 추가 피해 의미를 합친다.
		internal static void ApplyCoreAdditionalDamageAction(SkillExecutionState snapshot, CoreAdditionalDamageActionOp action)
	{
		snapshot.CoreHitboxName = action.HitboxName;
		snapshot.HasCoreOnHitAdditionalDamage = true;
		snapshot.CoreOnHitAdditionalDamageChance = action.Chance;
		snapshot.CoreOnHitAdditionalDamageMultiplier = action.Multiplier;
		snapshot.CoreOnHitAdditionalDamageAttribute = action.Attribute;
	}

		/// 연쇄 적중 피해 의미를 실행값에 합친다.
		internal static void ApplyHitChainDamageAction(SkillExecutionState snapshot, HitChainDamageActionOp action)
	{
		if (action.HitPeriod <= 0)
		{
			return;
		}

		snapshot.OnHitChainHitPeriod = action.HitPeriod;
		snapshot.OnHitChainTargetCount = action.TargetCount;
		snapshot.OnHitChainSearchRadius = action.SearchRadius;
		snapshot.OnHitChainDamageMultiplier = action.Multiplier;
		snapshot.OnHitChainDamageAttribute = action.Attribute;
	}

		/// 적중 수 기반 대기 환급 의미를 합친다.
		internal static void ApplyHitCountCooldownRefundAction(SkillExecutionState snapshot, HitCountCooldownRefundActionOp action)
	{
		if (string.IsNullOrWhiteSpace(action.TargetSkillName))
		{
			return;
		}

		snapshot.HitCountCooldownRefundTargetSkillName = action.TargetSkillName;
		snapshot.HitCountCooldownRefundMinTargets = action.MinimumTargets;
		snapshot.HitCountCooldownRefundRatio = action.Ratio;
	}

		/// 적중당 재장전 감소 의미를 합친다.
		internal static void ApplyReloadReducePerHitAction(SkillExecutionState snapshot, ReloadReducePerHitActionOp action)
	{
		if (string.IsNullOrWhiteSpace(action.TargetSkillName))
		{
			return;
		}

		snapshot.ReloadReduceTargetSkillName = action.TargetSkillName;
		snapshot.ReloadReduceSecondsPerHit += action.SecondsPerHit;
	}

		/// 상태의 행동 속도 보정을 실행값에 합친다.
		internal static void ApplyStatusActionSpeedBonus(SkillExecutionState snapshot, string statusName, float bonus)
	{
		snapshot.HasStatusActionSpeedBonus = true;
		if (string.IsNullOrWhiteSpace(statusName))
		{
			snapshot.StatusActionSpeedBonus += bonus;
			return;
		}
		snapshot.StatusActionSpeedBonusStatusName = statusName;
		float total = bonus;
		if (snapshot.statusActionSpeedBonuses.TryGetValue(statusName, out var value))
		{
			total += value;
		}
		snapshot.statusActionSpeedBonuses[statusName] = total;
	}

		/// 상태 지속시간 보정을 실행값에 합친다.
		internal static void ApplyStatusDurationBonus(SkillExecutionState snapshot, string statusName, float bonus)
	{
		if (!string.IsNullOrWhiteSpace(statusName) && !Mathf.Approximately(bonus, 0f))
		{
			float total = bonus;
			if (snapshot.statusDurationBonuses.TryGetValue(statusName, out var value))
			{
				total += value;
			}
			snapshot.statusDurationBonuses[statusName] = total;
		}
	}


        /// 보정값을 허용 범위의 기본값으로 정규화한다.
        private static float PositiveOrDefault(float value, float fallback)
        {
            return value > 0f ? value : fallback;
        }

        /// 시전 시점에 허용되는 후속 효과만 남긴다.
        internal static IReadOnlyList<SkillCastEffect> ResolveCastEffects(
            SkillExecutionState snapshot,
            bool enemyTargetsOnly)
        {
            if (snapshot == null || snapshot.CastEffects == null || snapshot.CastEffects.Count == 0)
            {
                return Array.Empty<SkillCastEffect>();
            }

            if (!enemyTargetsOnly)
            {
                return snapshot.CastEffects;
            }

            var effects = new List<SkillCastEffect>();
            for (var i = 0; i < snapshot.CastEffects.Count; i++)
            {
                var effect = snapshot.CastEffects[i];
                if (effect != null
                    && (effect.ResolvedDefinition?.Targeting?.TargetSide == SkillTargetSide.Enemy
                        || effect.Command?.Targeting?.TargetSide == SkillTargetSide.Enemy))
                {
                    effects.Add(effect);
                }
            }
            return effects;
        }

        /// 시전 조건이 제공하는 처형 기준 보정을 모은다.
        internal static float ResolveCastConditionHealthBonus(SkillExecutionState snapshot)
        {
            if (snapshot == null)
            {
                return 0f;
            }

            var bonus = 0f;
            for (var i = 0; i < snapshot.CastConditionOps.Count; i++)
            {
                bonus += snapshot.CastConditionOps[i].TargetHealthRatioBonus;
            }
            return bonus;
        }

        /// 반복 배치의 횟수와 간격을 확정한다.
        internal static bool ResolveRepeat(
            SkillExecutionState snapshot,
            out int count,
            out float intervalSeconds,
            out float damageMultiplier)
        {
            count = snapshot != null ? Mathf.Max(0, snapshot.RepeatCountPerTarget) : 0;
            intervalSeconds = snapshot != null ? Mathf.Max(0f, snapshot.RepeatIntervalSeconds) : 0f;
            damageMultiplier = snapshot != null ? Mathf.Max(0f, snapshot.RepeatDamageMultiplier) : 1f;
            return count > 0;
        }

        /// 핵심 충돌에 연결된 추가 피해 조건을 확정한다.
        internal static bool ResolveCoreAdditionalDamage(
            SkillExecutionState snapshot,
            bool isCoreHit,
            out float chance,
            out float multiplier,
            out DamageAttribute attribute)
        {
            chance = 0f;
            multiplier = 0f;
            attribute = default;
            if (!isCoreHit || snapshot == null || !snapshot.HasCoreOnHitAdditionalDamage)
            {
                return false;
            }

            chance = Mathf.Clamp01(snapshot.CoreOnHitAdditionalDamageChance);
            multiplier = Mathf.Max(0f, snapshot.CoreOnHitAdditionalDamageMultiplier);
            attribute = snapshot.CoreOnHitAdditionalDamageAttribute;
            return chance > 0f && multiplier > 0f;
        }

        /// 적중 수에 따른 대기 환급 조건을 확정한다.
        internal static bool ResolveHitCountCooldownRefund(
            SkillExecutionState snapshot,
            int hitCount,
            out string targetSkillName,
            out float secondsRatio)
        {
            targetSkillName = null;
            secondsRatio = 0f;
            if (snapshot == null
                || hitCount < snapshot.HitCountCooldownRefundMinTargets
                || string.IsNullOrWhiteSpace(snapshot.HitCountCooldownRefundTargetSkillName)
                || snapshot.HitCountCooldownRefundRatio <= 0f)
            {
                return false;
            }

            targetSkillName = snapshot.HitCountCooldownRefundTargetSkillName;
            secondsRatio = Mathf.Clamp01(snapshot.HitCountCooldownRefundRatio);
            return secondsRatio > 0f;
        }

        /// 소비된 상태의 처치 후 재분배 조건을 확정한다.
        internal static bool ResolveStatusRedistribution(
            SkillExecutionState snapshot,
            int consumedStacks,
            out int stacks,
            out StatusEffectKind statusKind,
            out float searchRadius,
            out int maxTargetCount)
        {
            stacks = 0;
            statusKind = StatusEffectKind.None;
            searchRadius = 0f;
            maxTargetCount = 0;
            if (snapshot == null
                || consumedStacks <= 0
                || snapshot.RedistributeConsumedStatusRatioOnKill <= 0f
                || snapshot.RedistributeConsumedStatusKind == StatusEffectKind.None
                || snapshot.RedistributeConsumedStatusSearchRadius <= 0f)
            {
                return false;
            }

            stacks = Mathf.FloorToInt(
                consumedStacks * Mathf.Clamp01(snapshot.RedistributeConsumedStatusRatioOnKill));
            statusKind = snapshot.RedistributeConsumedStatusKind;
            searchRadius = snapshot.RedistributeConsumedStatusSearchRadius;
            maxTargetCount = snapshot.RedistributeConsumedStatusTargetCount;
            return stacks > 0;
        }

        /// 대상의 상태 보유량을 공통 기준으로 읽는다.
        private static int StatusStacks(UnitCombatState target, StatusEffectKind statusKind)
        {
            if (target == null || statusKind == StatusEffectKind.None)
            {
                return 0;
            }
            if (statusKind == StatusEffectKind.Shield)
            {
                return target.Resources != null && target.Resources.CurrentShield > 0f ? 1 : 0;
            }
            return target.Statuses != null ? target.Statuses.GetStacks(statusKind) : 0;
        }

        /// 대상 상태가 피해에 기여하는 중첩 수를 확정한다.
        internal static int ResolveTargetStatusStackCount(
            SkillExecutionState snapshot,
            UnitCombatState target)
        {
            if (snapshot == null || target == null || snapshot.PreparedTargetStatusStackStatusKind == StatusEffectKind.None)
            {
                return 0;
            }

            var count = StatusStacks(target, snapshot.PreparedTargetStatusStackStatusKind);
            if (snapshot.PreparedTargetStatusStackMaxStacks > 0)
            {
                count = Mathf.Min(count, snapshot.PreparedTargetStatusStackMaxStacks);
            }
            return Mathf.Max(0, count);
        }

        /// 대상 상태 중첩을 추가 피해로 환산한다.
        internal static float ResolveTargetStatusStackDamage(
            SkillExecutionState snapshot,
            UnitCombatState target,
            float baseDamage)
        {
            var count = ResolveTargetStatusStackCount(snapshot, target);
            if (count <= 0)
            {
                return 0f;
            }

            var statusDamage = snapshot.PreparedTargetStatusStackDamage
                * Mathf.Max(0f, snapshot.TargetStatusStackDamageMultiplier);
            var rateDamage = Mathf.Max(0f, baseDamage)
                * snapshot.PreparedTargetStatusStackDamageRateBonus;
            return Mathf.Max(0f, count * (statusDamage + rateDamage));
        }

        /// 적중 시 소비할 대상 상태 중첩을 확정한다.
        internal static int ResolveConsumedStatusStacks(
            SkillExecutionState snapshot,
            UnitCombatState target)
        {
            if (snapshot == null || target == null || snapshot.PreparedConsumeTargetStatusKind == StatusEffectKind.None)
            {
                return 0;
            }

            var available = StatusStacks(target, snapshot.PreparedConsumeTargetStatusKind);
            if (available <= 0)
            {
                return 0;
            }
            if (snapshot.PreparedConsumeTargetStatusStacks > 0)
            {
                return Mathf.Clamp(snapshot.PreparedConsumeTargetStatusStacks, 0, available);
            }
            return Mathf.Clamp(
                Mathf.FloorToInt(available * Mathf.Clamp01(snapshot.PreparedConsumeTargetStatusRatio)),
                0,
                available);
        }

        /// 적중 대상에 적용할 최종 피해 배율을 합성한다.
        internal static float ResolveHitDamageMultiplier(
            SkillExecutionState snapshot,
            UnitCombatState target)
        {
            if (snapshot == null)
            {
                return 1f;
            }
            var multiplier = Mathf.Max(0f, snapshot.DamageMultiplier)
                * ConditionalDamageMultiplier(snapshot, target);
            var stackMultipliers = snapshot.TargetStatusStackDamageMultiplierBonuses;
            foreach (var pair in stackMultipliers)
            {
                if (StatusStacks(target, ParseStatusKind(pair.Key)) > 0)
                {
                    multiplier *= 1f + pair.Value * StatusStacks(target, ParseStatusKind(pair.Key));
                }
            }
            return multiplier;
        }

        /// 치명타 보정 뒤 적용할 최종 피해 배율을 계산한다.
        internal static float ResolveHitFinalDamageModifier(
            SkillExecutionState snapshot,
            UnitCombatState target,
            UnitSpawnManager roster)
        {
            var multiplier = snapshot != null ? Mathf.Max(0f, snapshot.FinalDamageModifier) : 1f;
            if (snapshot == null || target == null)
            {
                return multiplier;
            }

            var actions = snapshot.ConditionalFinalDamageActions;
            for (var i = 0; i < actions.Count; i++)
            {
                if (MatchesConditionalCritCondition(actions[i].ConditionKind, target, roster))
                {
                    multiplier *= Mathf.Max(0f, actions[i].Multiplier);
                }
            }
            return multiplier;
        }

        /// 연속 적중 횟수에 따른 피해 배율을 계산한다.
        internal static float ResolveConsecutiveHitDamageMultiplier(
            SkillExecutionState runtime,
            SkillExecutionState snapshot,
            int repeatCount)
        {
            if (runtime == null || repeatCount < 0)
            {
                return 1f;
            }

            var projectileData = runtime.Data as ProjectileSkillDefinition;
            var bonusRate = projectileData != null
                ? projectileData.ConsecutiveHitBonusRate
                : 0f;
            var bonusMax = projectileData != null
                ? projectileData.ConsecutiveHitMax
                : 0f;
            if (snapshot != null && snapshot.ConsecutiveHitBonusRate > 0f)
            {
                bonusRate = snapshot.ConsecutiveHitBonusRate;
            }
            if (snapshot != null && snapshot.ConsecutiveHitMax > 0f)
            {
                bonusMax = snapshot.ConsecutiveHitMax;
            }
            if (bonusRate <= 0f || bonusMax <= 0f)
            {
                return 1f;
            }

            var bonus = Mathf.Min(
                Mathf.Max(0f, bonusMax),
                Mathf.Max(0f, bonusRate) * repeatCount);
            return 1f + bonus;
        }

        /// 적중 대상 조건의 치명타 보정을 계산한다.
        internal static float ResolveHitCritChanceBonus(
            SkillExecutionState snapshot,
            UnitCombatState target)
        {
            return snapshot == null
                ? 0f
                : ConditionalCritChanceBonus(snapshot, target);
        }

        internal static void ResolveHitCritModifiers(
            SkillExecutionState snapshot,
            UnitCombatState target,
            UnitSpawnManager roster,
            ref float critChanceBonus,
            ref float critDamageBonus)
        {
            if (snapshot == null || target == null)
            {
                return;
            }

            critChanceBonus += ConditionalCritChanceBonus(snapshot, target);
            var actions = snapshot.ConditionalCritActions;
            for (var i = 0; i < actions.Count; i++)
            {
                var action = actions[i];
                if (action.ConditionKind == ConditionalCritConditionKind.SkillRuntimeKind
                    || !MatchesConditionalCritAction(action, target, roster))
                {
                    continue;
                }

                critChanceBonus += action.ChanceBonus;
                critDamageBonus = CombineCritDamageBonus(critDamageBonus, action.DamageBonus);
            }
        }

        internal static float CombineCritDamageBonus(float currentBonus, float addedBonus)
        {
            return (1f + currentBonus) * Mathf.Max(0f, 1f + addedBonus) - 1f;
        }

        /// 처치 결과가 허용하는 대기 회복을 계산한다.
        internal static void ResolveKillRecovery(
            SkillExecutionState snapshot,
            bool wasExecute,
            out bool resetCooldown,
            out float refundRatio)
        {
            resetCooldown = false;
            refundRatio = snapshot != null
                ? Mathf.Clamp01(snapshot.PreparedKillCooldownRefundRatio)
                : 0f;
            if (snapshot == null)
            {
                return;
            }

            for (var i = 0; i < snapshot.KillActionOps.Count; i++)
            {
                var action = snapshot.KillActionOps[i];
                if (action.Kind == KillActionOpKind.CooldownReset
                    && (!action.RequiresExecute || wasExecute))
                {
                    resetCooldown = true;
                }
                if (action.Kind == KillActionOpKind.CooldownRefundBonus)
                {
                    refundRatio = Mathf.Clamp01(refundRatio + action.RatioBonus);
                }
            }
        }

        /// 투사체 분기 조건을 실행값에 확정한다.
        internal static void ResolveProjectileBranch(
            SkillExecutionState data,
            int projectileLaunchIndex,
            out float chance,
            out int count,
            out float damageMultiplier,
            out float searchRadius)
        {
            chance = 0f;
            count = 0;
            damageMultiplier = 1f;
            searchRadius = 0f;
            if (data == null || !data.HasBranchBehavior)
            {
                return;
            }

            chance = data.HasBranchChanceSet
                ? data.BranchChanceSet
                : data.BranchChanceBonus;
            if (data.HasBranchLaunchTrigger
                && projectileLaunchIndex > 0
                && projectileLaunchIndex % data.BranchLaunchPeriod == 0)
            {
                chance = data.BranchLaunchChanceSet;
            }

            count = data.HasBranchCount ? data.BranchCount : chance > 0f ? 1 : 0;
            searchRadius = data.HasBranchSearchRadius ? data.BranchSearchRadius : 4.5f;
            if (chance <= 0f || count <= 0 || searchRadius <= 0f)
            {
                chance = 0f;
                count = 0;
                searchRadius = 0f;
                return;
            }

            chance = Mathf.Clamp01(chance);
            count = Math.Max(1, count);
            damageMultiplier = data.HasBranchDamageMultiplier
                ? Mathf.Max(0f, data.BranchDamageMultiplier)
                : 1f;
            searchRadius = Mathf.Max(0f, searchRadius);
        }

        /// 투사체가 사라질 경계를 계산한다.
        internal static float ProjectileDestroyBoundaryX(
            Vector2 origin,
            Vector2 direction,
            float speed,
            float lifetime)
        {
            var normalizedDirection = direction.sqrMagnitude > 0.0001f
                ? direction.normalized
                : Vector2.right;
            var maxTravelDistance = Mathf.Max(
                40f,
                Mathf.Max(0f, speed) * Mathf.Max(0.1f, lifetime) + 1f);
            return origin.x + normalizedDirection.x * maxTravelDistance;
        }

        /// 현재 대상이 제공하는 조건부 피해 배율을 계산한다.
        internal static float ConditionalDamageMultiplier(
            SkillExecutionState data,
            UnitCombatState target)
        {

            if (data == null || target == null)
            {
                return 1f;
            }

            IReadOnlyList<ConditionalDamageActionOp> actions = data.ConditionalDamageActions;
            var multiplier = 1f;
            for (var i = 0; i < actions.Count; i++)
            {
                ConditionalDamageActionOp action = actions[i];
                if (HasRequiredStacks(target, action.Condition))
                {
                    multiplier *= action.DamageMultiplier;
                }
            }

			var groupedActions = data.ConditionalStatusGroupDamageActions;
			for (var i = 0; i < groupedActions.Count; i++)
			{
				var action = groupedActions[i];
				if (StatusConditionRules.MatchesConditionStatus(target, action.Groups))
				{
					multiplier *= action.DamageMultiplier;
				}
			}

            return multiplier;
        }

        /// 현재 대상이 제공하는 치명타 보정을 계산한다.
        internal static float ConditionalCritChanceBonus(
            SkillExecutionState data,
            UnitCombatState target)
        {
            if (data == null || target == null)
            {
                return 0f;
            }

            IReadOnlyList<ConditionalCritChanceActionOp> actions = data.ConditionalCritChanceActions;
            var bonus = 0f;
            for (var i = 0; i < actions.Count; i++)
            {
                ConditionalCritChanceActionOp action = actions[i];
                if (HasRequiredStacks(target, action.Condition))
                {
                    bonus += action.ChanceBonus;
                }
            }

            return bonus;
        }

        private static bool MatchesConditionalCritAction(
            ConditionalCritActionOp action,
            UnitCombatState target,
            UnitSpawnManager roster)
        {
            switch (action.ConditionKind)
            {
                case ConditionalCritConditionKind.TargetHealthRatioAtMost:
                    return target.Stats != null
                        && target.Stats.MaxHealth > 0f
                        && target.Resources != null
                        && target.Resources.CurrentHealth / target.Stats.MaxHealth <= action.Threshold;
                case ConditionalCritConditionKind.TargetIsBoss:
                    return target.IsBoss;
                case ConditionalCritConditionKind.TargetHighestCurrentHealth:
                    if (roster == null || roster.Enemies == null || target.Resources == null)
                    {
                        return false;
                    }
                    var currentHealth = target.Resources.CurrentHealth;
                    for (var i = 0; i < roster.Enemies.Count; i++)
                    {
                        var entry = roster.Enemies[i];
                        if (entry != null && entry.IsAlive && entry.Model != null && entry.Model.Resources != null
                            && entry.Model.Resources.CurrentHealth > currentHealth + 0.0001f)
                        {
                            return false;
                        }
                    }
                    return true;
                case ConditionalCritConditionKind.TargetHasStatus:
                    return HasRequiredStacks(target, new StatusStackCondition(action.StatusKind, 1));
                default:
                    return false;
            }
        }

        private static bool MatchesConditionalCritCondition(
            ConditionalCritConditionKind conditionKind,
            UnitCombatState target,
            UnitSpawnManager roster)
        {
            return conditionKind == ConditionalCritConditionKind.TargetIsBoss && target != null && target.IsBoss;
        }

        private static StatusEffectKind ParseStatusKind(string name)
        {
            return StatusValueParser.TryParseStatusKind(name, out var kind)
                ? kind
                : StatusEffectKind.None;
        }

        /// 폭발 조건이 제공하는 피해 배율을 계산한다.
        internal static float BurstDamageMultiplier(
            SkillExecutionState data,
            int projectileIndex,
            int burstProjectileCount)
        {
            if (data == null || projectileIndex <= 0)
            {
                return 1f;
            }

            IReadOnlyList<BurstDamageActionOp> actions = data.BurstDamageActions;
            var multiplier = 1f;
            for (var i = 0; i < actions.Count; i++)
            {
                BurstDamageActionOp action = actions[i];
                if (MatchesBurstProjectileIndex(action.ProjectileIndex, projectileIndex, burstProjectileCount))
                {
                    multiplier *= action.DamageMultiplier;
                }
            }

            return multiplier;
        }

        /// 폭발 조건이 제공하는 상태 중첩 보정을 계산한다.
        internal static int BurstStatusStacksBonus(
            SkillExecutionState data,
            int projectileIndex,
            int burstProjectileCount)
        {
            if (data == null || projectileIndex <= 0)
            {
                return 0;
            }

            IReadOnlyList<BurstStatusActionOp> actions = data.BurstStatusActions;
            var bonus = 0;
            for (var i = 0; i < actions.Count; i++)
            {
                BurstStatusActionOp action = actions[i];
                if (MatchesBurstProjectileIndex(action.ProjectileIndex, projectileIndex, burstProjectileCount))
                {
                    bonus += action.StacksBonus;
                }
            }

            return bonus;
        }

        /// 대상이 요구 중첩을 충족하는지 확인한다.
        private static bool HasRequiredStacks(UnitCombatState target, StatusStackCondition condition)
        {
            if (target == null || condition.StatusKind == StatusEffectKind.None || condition.MinimumStacks <= 0)
            {
                return false;
            }

            if (condition.StatusKind == StatusEffectKind.Shield)
            {
                return target.Resources != null && target.Resources.CurrentShield > 0f;
            }

            return target.Statuses != null
                && target.Statuses.GetStacks(condition.StatusKind) >= condition.MinimumStacks;
        }

        /// 현재 투사체가 폭발 조건의 순번인지 확인한다.
        private static bool MatchesBurstProjectileIndex(
            int configuredIndex,
            int projectileIndex,
            int burstProjectileCount)
        {
            if (configuredIndex == 0)
            {
                return burstProjectileCount > 0 && projectileIndex == burstProjectileCount;
            }

            return configuredIndex == projectileIndex;
        }

        /// 시전 시점에 처형 조건을 충족하는지 판정한다.
        internal static bool ShouldRejectCastForExecuteThreshold(
            SkillExecutionContext context,
            SkillExecutionState snapshot,
            SingleSkillDefinition skill)
        {
            if (!skill.RequireExecuteThresholdToCast
                || !TryResolveSingleThreshold(skill, snapshot, out var threshold))
            {
                return false;
            }

            var targets = SkillTargeting.OrderedTargets(context, skill.Targeting);
            var target = targets.Count > 0 ? targets[0] : null;
            return target == null || target.Model == null || !IsWithinSingleThreshold(target.Model, threshold);
        }

        /// 단일 대상 조건을 피해와 치명타 값에 반영한다.
        internal static void ApplySingleDamageModifiers(
            SkillExecutionState snapshot,
            UnitCombatState target,
            ref float damageMultiplier,
            ref float critChanceBonus,
            out bool isExecute)
        {
            isExecute = false;
            if (snapshot != null && IsWithinSingleThreshold(target, snapshot.PreparedExecuteHealthRatioThreshold))
            {
                isExecute = true;
                if (snapshot.PreparedExecuteDamageMultiplier > 0f)
                {
                    damageMultiplier *= snapshot.PreparedExecuteDamageMultiplier;
                }

                for (var i = 0; i < snapshot.DamageModifierOps.Count; i++)
                {
                    var op = snapshot.DamageModifierOps[i];
                    if (op.Kind == DamageModifierOpKind.ExecuteMultiplier)
                    {
                        damageMultiplier *= op.Multiplier;
                    }
                }

                for (var i = 0; i < snapshot.CritModifierOps.Count; i++)
                {
                    critChanceBonus += snapshot.CritModifierOps[i].ChanceBonus;
                }
            }

            if (!target.IsBoss || snapshot == null)
            {
                return;
            }

            if (snapshot.PreparedBossDamageMultiplier > 0f)
            {
                damageMultiplier *= snapshot.PreparedBossDamageMultiplier;
            }

            for (var i = 0; i < snapshot.DamageModifierOps.Count; i++)
            {
                var op = snapshot.DamageModifierOps[i];
                if (op.Kind == DamageModifierOpKind.BossMultiplier)
                {
                    damageMultiplier *= op.Multiplier;
                }
            }
        }

        /// 단일 대상 처형 기준을 확정한다.
        private static bool TryResolveSingleThreshold(
            SingleSkillDefinition skill,
            SkillExecutionState snapshot,
            out float threshold)
        {
            var bonus = 0f;
            if (snapshot != null)
            {
                for (var i = 0; i < snapshot.CastConditionOps.Count; i++)
                {
                    bonus += snapshot.CastConditionOps[i].TargetHealthRatioBonus;
                }
            }

            threshold = Mathf.Clamp01(Mathf.Max(0f, skill.ExecuteHealthRatioThreshold) + bonus);
            return threshold > 0f;
        }

        /// 대상이 처형 기준 안에 있는지 확인한다.
        private static bool IsWithinSingleThreshold(UnitCombatState target, float threshold)
        {
            var resources = target != null ? target.Resources : null;
            var stats = target != null ? target.Stats : null;
            if (resources == null || stats == null || stats.MaxHealth <= 0f || threshold <= 0f)
            {
                return false;
            }

            return resources.CurrentHealth / stats.MaxHealth <= threshold;
        }

        /// 상태 적용에 필요한 최종 수치를 확정한다.
        internal static StatusApplicationSpec StatusSpec(
            StatusApplicationSpec baseStatus,
            SkillExecutionState snapshot)
        {
            StatusRuntimeData statusData = null;
            if (baseStatus != null)
            {
                statusData = baseStatus.Status;
            }

            if (statusData == null)
            {
                return null;
            }

            var kind = statusData.Kind;
            var stacks = 1;
            var chance = 1f;
            var refreshDuration = true;
            if (baseStatus != null)
            {
                stacks = Math.Max(0, baseStatus.Stacks);
                chance = Mathf.Clamp01(baseStatus.Chance);
                refreshDuration = baseStatus.RefreshDuration;
            }

            if (snapshot != null)
            {
                chance = Mathf.Clamp01(chance + snapshot.StatusChanceBonus);
                if (snapshot.HasStatusStacksSet)
                {
                    stacks = Math.Max(0, snapshot.StatusStacksSet);
                }
                else
                {
                    stacks = Math.Max(0, stacks + snapshot.StatusStacksBonus);
                }
            }

            if (stacks <= 0 || chance <= 0f)
            {
                return null;
            }

            if (statusData == null || statusData.Kind != kind)
            {
                statusData = CatalogStatusData(kind);
            }

            var resolvedStatusData = StatusData(statusData, kind, snapshot);
            var duration = resolvedStatusData.Duration;
            var maxStacks = resolvedStatusData.MaxStacks;
            var maxStacksBonus = StatusMaxStacksBonus(snapshot, resolvedStatusData);
            if (maxStacksBonus != 0)
            {
                maxStacks = Mathf.Max(0, maxStacks + maxStacksBonus);
            }

            var permanent = resolvedStatusData.Permanent;
            if (snapshot != null
                && (!Mathf.Approximately(snapshot.DurationMultiplier, 1f)
                    || !Mathf.Approximately(snapshot.DurationBonus, 0f)))
            {
                duration = duration * Mathf.Max(0f, snapshot.DurationMultiplier) + snapshot.DurationBonus;
                if (duration > 0f)
                {
                    permanent = false;
                }
            }

            var durationBonus = StatusDurationBonus(snapshot, resolvedStatusData);
            if (!Mathf.Approximately(durationBonus, 0f))
            {
                duration = Mathf.Max(0f, duration + durationBonus);
                if (duration > 0f)
                {
                    permanent = false;
                }
            }

            var thresholdStatusKind = StatusEffectKind.None;
            var thresholdStatusMinStacks = 0;
            if (snapshot != null)
            {
                thresholdStatusKind = snapshot.ThresholdStatusKind;
                thresholdStatusMinStacks = snapshot.ThresholdStatusMinStacks;
            }

            return new StatusApplicationSpec
            {
                Enabled = true,
                RuntimeResolved = true,
                Status = resolvedStatusData,
                Chance = chance,
                Stacks = stacks,
                RuntimeDurationSeconds = duration,
                RuntimeMaxStacks = maxStacks,
                RuntimePermanent = permanent,
                RefreshDuration = refreshDuration,
                ThresholdSourceStatusKind = thresholdStatusKind,
                ThresholdSourceMinStacks = thresholdStatusMinStacks,
                ThresholdStatus = ThresholdStatusSpec(snapshot)
            };
        }

        /// 직접 상태 효과를 공통 적용값으로 만든다.
        internal static StatusApplicationSpec CreateDirectStatusSpec(
            StatusEffectKind kind,
            int stacks,
            SkillExecutionState snapshot)
        {
            if (kind == StatusEffectKind.None || stacks <= 0)
            {
                return null;
            }

            var statusData = CatalogStatusData(kind);
            statusData = StatusData(statusData, kind, snapshot);
            var duration = statusData.Duration;
            var durationBonus = StatusDurationBonus(snapshot, statusData);
            if (!Mathf.Approximately(durationBonus, 0f))
            {
                duration = Mathf.Max(0f, duration + durationBonus);
            }

            var maxStacks = statusData.MaxStacks;
            var maxStacksBonus = StatusMaxStacksBonus(snapshot, statusData);
            if (maxStacksBonus != 0)
            {
                maxStacks = Mathf.Max(0, maxStacks + maxStacksBonus);
            }

            return new StatusApplicationSpec
            {
                Enabled = true,
                RuntimeResolved = true,
                Status = statusData,
                Chance = 1f,
                Stacks = stacks,
                RuntimeDurationSeconds = duration,
                RuntimeMaxStacks = maxStacks,
                RuntimePermanent = statusData.Permanent && duration <= 0f,
                RefreshDuration = true
            };
        }

        /// 상태 정의와 보정을 런타임 값으로 합친다.
        internal static StatusRuntimeData StatusData(
            StatusRuntimeData statusData,
            StatusEffectKind kind,
            SkillExecutionState snapshot)
        {
            if (snapshot == null)
            {
                return statusData;
            }

            var actionSpeedBonus = snapshot.GetStatusActionSpeedBonus(statusData.StatusTag);
            var actionSpeedMultiplier = snapshot.GetStatusActionSpeedMultiplier(statusData.StatusTag);
            var hasActionSpeedBonus = !Mathf.Approximately(actionSpeedBonus, 0f);
            var hasOverride = snapshot.HasStatusElementDamageTakenBonus
                || snapshot.HasStatusCriticalDamageTakenBonus
                || snapshot.HasStatusAilmentResistanceBonus
                || snapshot.HasStatusDamageBonusRate
                || snapshot.HasStatusShieldReceivedBonus
                || snapshot.HasStatusCriticalChanceBonus
                || snapshot.HasStatusDamageTakenBonus
                || snapshot.HasStatusFlatElementResistReduction
                || snapshot.HasStatusConditionalDamageTakenBonus
                || snapshot.HasStatusAttackPowerBonus
                || hasActionSpeedBonus
                || !Mathf.Approximately(actionSpeedMultiplier, 1f);
            if (!hasOverride)
            {
                return statusData;
            }

            var resolvedStatus = statusData.Clone();
            if (snapshot.HasStatusElementDamageTakenBonus)
            {
                resolvedStatus.ElementDamageTakenBonus += snapshot.StatusElementDamageTakenBonus;
            }

            if (snapshot.HasStatusCriticalDamageTakenBonus)
            {
                resolvedStatus.CriticalDamageTakenBonus += snapshot.StatusCriticalDamageTakenBonus;
            }

            if (snapshot.HasStatusAilmentResistanceBonus)
            {
                resolvedStatus.AilmentResistanceBonus += snapshot.StatusAilmentResistanceBonus;
            }

            if (snapshot.HasStatusDamageBonusRate)
            {
                resolvedStatus.Modifiers.DamageBonusRate += snapshot.StatusDamageBonusRate;
            }

            if (snapshot.HasStatusShieldReceivedBonus)
            {
                resolvedStatus.Modifiers.ShieldReceivedBonus += snapshot.StatusShieldReceivedBonus;
            }

            if (snapshot.HasStatusCriticalChanceBonus)
            {
                resolvedStatus.Modifiers.CritChanceBonusRate += snapshot.StatusCriticalChanceBonus;
            }

            if (snapshot.HasStatusDamageTakenBonus)
            {
                resolvedStatus.DamageTakenBonus += snapshot.StatusDamageTakenBonus;
            }

            if (snapshot.HasStatusFlatElementResistReduction)
            {
                resolvedStatus.FlatElementResistReduction += snapshot.StatusFlatElementResistReduction;
            }

            if (snapshot.HasStatusConditionalDamageTakenBonus)
            {
                resolvedStatus.ConditionalSourceStatusKind = snapshot.StatusConditionalSourceStatusKind;
                resolvedStatus.ConditionalDamageTakenBonus = snapshot.StatusConditionalDamageTakenBonus;
            }

            if (hasActionSpeedBonus)
            {
                resolvedStatus.Modifiers.ActionSpeedBonus += actionSpeedBonus;
            }
            if (!Mathf.Approximately(actionSpeedMultiplier, 1f))
            {
                resolvedStatus.Modifiers.ActionSpeedBonus =
                    (1f + resolvedStatus.Modifiers.ActionSpeedBonus) * actionSpeedMultiplier - 1f;
            }

            if (snapshot.HasStatusAttackPowerBonus)
            {
                resolvedStatus.Modifiers.AttackPowerBonus += snapshot.StatusAttackPowerBonus;
            }

            return resolvedStatus;
        }

        /// 상태 지속시간 보정량을 계산한다.
        private static float StatusDurationBonus(SkillExecutionState snapshot, StatusRuntimeData statusData)
        {
            if (snapshot == null)
            {
                return 0f;
            }

            return snapshot.StatusDurationBonus(statusData.StatusTag);
        }

        /// 상태 최대 중첩 보정량을 계산한다.
        private static int StatusMaxStacksBonus(SkillExecutionState snapshot, StatusRuntimeData statusData)
        {
            if (snapshot == null)
            {
                return 0;
            }

            return snapshot.StatusMaxStacksBonus(statusData.StatusTag);
        }

        /// 임계 상태 효과의 적용값을 확정한다.
        private static StatusApplicationSpec ThresholdStatusSpec(SkillExecutionState snapshot)
        {
            if (snapshot == null || snapshot.ThresholdApplyStatusKind == StatusEffectKind.None)
            {
                return null;
            }

            var kind = snapshot.ThresholdApplyStatusKind;
            var statusData = CatalogStatusData(kind);
            var duration = statusData.Duration;
            var durationBonus = StatusDurationBonus(snapshot, statusData);
            if (!Mathf.Approximately(durationBonus, 0f))
            {
                duration = Mathf.Max(0f, duration + durationBonus);
            }

            return new StatusApplicationSpec
            {
                Enabled = true,
                RuntimeResolved = true,
                Status = statusData,
                Chance = 1f,
                Stacks = statusData.BaseStackAmount,
                RuntimeDurationSeconds = duration,
                RuntimeMaxStacks = statusData.MaxStacks,
                RuntimePermanent = statusData.Permanent && duration <= 0f,
                RefreshDuration = true
            };
        }

        /// 카탈로그에서 상태의 기본값을 가져온다.
        private static StatusRuntimeData CatalogStatusData(StatusEffectKind kind)
        {
            return GameDataLoader.CurrentCatalog?.GetStatusRuntimeData(kind)
                ?? throw new InvalidOperationException($"Status runtime data '{kind}' is not registered.");
        }
    }
}
