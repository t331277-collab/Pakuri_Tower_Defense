using Pakuri.Data;
using UnityEngine;

/*
 * 선택된 강화와 마스터 노드를 현재 스킬의 최종 실행값에 적용한다.
 * 전투 실행기는 완성된 SkillSnapshot만 사용하며 Choice 종류를 직접 해석하지 않는다.
 */
namespace Pakuri.InGame
{
    /*
     * 학습한 선택지를 스킬 실행 상태에 적용한다.
     */
    internal class SkillUpgrade
    {
        /*
         * 유닛이 학습한 선택지를 현재 스킬 실행 정보에 적용한다.
         */
        public SkillSnapshot Resolve(UnitCombatState owner /* 정보를 소유한 유닛 */, SkillRuntimeInstance runtime /* 실행 중인 스킬 정보 */)
        {
            return Resolve(owner, runtime, null);
        }

        /*
         * 유닛이 학습한 선택지를 현재 스킬 실행 정보에 적용한다.
         */
        public SkillSnapshot Resolve(UnitCombatState owner /* 정보를 소유한 유닛 */, SkillRuntimeInstance runtime /* 실행 중인 스킬 정보 */, CombatUnitRegistry roster /* 전투에 등록된 유닛 목록 */)
        {
            SkillRuntimeData skillData = null;
            if (runtime != null)
            {
                skillData = runtime.Data;
            }
            var snapshot = new SkillSnapshot(skillData);
            ApplyPassiveBaseModifiers(snapshot, owner, skillData);
            System.Collections.Generic.ICollection<string> chosenChoiceIds = null;
            if (owner != null && owner.SkillProgress != null)
            {
                chosenChoiceIds = owner.SkillProgress.ChosenChoiceIds;
            }
            if (skillData == null || chosenChoiceIds == null || chosenChoiceIds.Count == 0)
            {
                return snapshot;
            }

            ApplyChoices(snapshot, chosenChoiceIds, skillData, owner, roster);
            return snapshot;
        }

        /*
         * 패시브 기본 보정값을 적용한다.
         */
        private static void ApplyPassiveBaseModifiers(
            SkillSnapshot snapshot /* 적용할 스킬 강화 정보 */,
            UnitCombatState owner /* 정보를 소유한 유닛 */,
            SkillRuntimeData skillData /* 스킬 실행 데이터 */)
        {
            if (snapshot == null
                || owner == null
                || owner.Identity.Role != UnitRole.Monster
                || owner.SkillProgress == null
                || skillData == null
                || owner.SkillProgress.LearnedPassiveSkillIds == null
                || owner.SkillProgress.LearnedPassiveSkillIds.Count == 0)
            {
                return;
            }

            foreach (var passiveId in owner.SkillProgress.LearnedPassiveSkillIds)
            {
                var passiveRuntime = owner.SkillRuntime.FindBySkillId(passiveId);
                PassiveSkillRuntimeData passive = null;
                if (passiveRuntime != null)
                {
                    passive = passiveRuntime.Data as PassiveSkillRuntimeData;
                }
                if (passive == null)
                {
                    continue;
                }

                for (var i = 0; i < passive.BaseModifierChoices.Length; i++)
                {
                    var modifier = passive.BaseModifierChoices[i];
                    if (modifier != null && AppliesToSkill(modifier.Source, skillData))
                    {
                        snapshot.ApplyChoiceSpec(modifier);
                    }
                }
            }
        }

        /*
         * 선택지를 적용한다.
         */
        private static void ApplyChoices(
            SkillSnapshot snapshot /* 적용할 스킬 강화 정보 */,
            System.Collections.Generic.ICollection<string> chosenChoiceIds /* 선택된 선택지 식별자 목록 */,
            SkillRuntimeData skillData /* 스킬 실행 데이터 */,
            UnitCombatState owner /* 정보를 소유한 유닛 */,
            CombatUnitRegistry roster /* 전투에 등록된 유닛 목록 */)
        {
            if (snapshot == null || chosenChoiceIds == null || skillData == null)
            {
                return;
            }

            foreach (var choiceId in chosenChoiceIds)
            {
                var choice = owner.SkillRuntime.FindChoice(choiceId);
                if (choice != null
                    && AppliesToSkill(choice.Source, skillData)
                    && SkillRequirement.MeetsSourceStatus(choice.Source, owner))
                {
                    snapshot.AddActiveChoiceId(choice.ChoiceId);
                    snapshot.ApplyChoiceSpec(choice);
                    ApplyDynamicChoiceRules(snapshot, choice.Source, owner, roster);
                }
            }
        }

        /*
         * 동적 선택지 규칙을 적용한다.
         */
        private static void ApplyDynamicChoiceRules(
            SkillSnapshot snapshot /* 적용할 스킬 강화 정보 */,
            SkillChoiceDefinition choice /* 적용하거나 검사할 스킬 선택지 */,
            UnitCombatState owner /* 정보를 소유한 유닛 */,
            CombatUnitRegistry roster /* 전투에 등록된 유닛 목록 */)
        {
            if (snapshot == null || choice == null || roster == null)
            {
                return;
            }

            if (choice.CountStatusKind != StatusEffectKind.None
                && choice.DamageMultiplierPerCount > 0f)
            {
                ApplyCountStatusDamageMultiplier(
                    snapshot,
                    owner,
                    roster,
                    choice.CountTargetSide,
                    choice.CountStatusKind,
                    choice.DamageMultiplierPerCount,
                    choice.CountMax);
            }

            SkillNodeDefinition[] targetNodes = SkillNodeMapper.FilterSkillNodeDefinitionsForTarget(
                choice.NormalizedPlanNodes,
                snapshot.SkillId);
            SkillNode[] nodes = SkillNodeMapper.MapSkillNodeDefinitions(targetNodes);
            for (int i = 0; i < nodes.Length; i++)
            {
                if (nodes[i] == null)
                {
                    continue;
                }

                CountStatusDamageActionOp? action = nodes[i].CountStatusDamageAction;
                if (!action.HasValue)
                {
                    continue;
                }

                ApplyCountStatusDamageMultiplier(
                    snapshot,
                    owner,
                    roster,
                    action.Value.TargetSide,
                    action.Value.StatusKind,
                    action.Value.AmountPerCount,
                    action.Value.MaximumCount);
            }
        }

        /*
         * 횟수 상태 피해 배율을 적용한다.
         */
        private static void ApplyCountStatusDamageMultiplier(
            SkillSnapshot snapshot /* 적용할 스킬 강화 정보 */,
            UnitCombatState owner /* 정보를 소유한 유닛 */,
            CombatUnitRegistry roster /* 전투에 등록된 유닛 목록 */,
            SkillMultiEffectTargetSide targetSide /* 대상 진영 */,
            StatusEffectKind statusKind /* 상태 효과 종류 */,
            float amountPerCount /* 수치 개별 개수 */,
            int countMax /* 개수 최대 */)
        {
            if (snapshot == null
                || statusKind == StatusEffectKind.None
                || amountPerCount <= 0f
                || roster == null)
            {
                return;
            }

            var count = CountMatchingTargets(owner, roster, targetSide, statusKind);
            if (countMax > 0)
            {
                count = Mathf.Min(count, countMax);
            }

            if (count <= 0)
            {
                return;
            }

            snapshot.ApplyDynamicDamageMultiplier(1f + count * amountPerCount);
        }

        /*
         * 선택지 조건과 일치하는 대상 수를 계산한다.
         */
        private static int CountMatchingTargets(
            UnitCombatState owner /* 정보를 소유한 유닛 */,
            CombatUnitRegistry roster /* 전투에 등록된 유닛 목록 */,
            SkillMultiEffectTargetSide side /* 진영 */,
            StatusEffectKind statusKind /* 상태 효과 종류 */)
        {
            if (owner == null || roster == null || statusKind == StatusEffectKind.None)
            {
                return 0;
            }

            var entries = ResolveCountEntries(owner, roster, side);
            var count = 0;
            for (var i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                if (entry == null || !entry.IsAlive || entry.Model == null)
                {
                    continue;
                }

                if (HasStatus(entry.Model, statusKind))
                {
                    count++;
                }
            }

            return count;
        }

        /*
         * 횟수 유닛 항목을 결정한다.
         */
        private static System.Collections.Generic.IReadOnlyList<CombatUnitEntry> ResolveCountEntries(
            UnitCombatState owner /* 정보를 소유한 유닛 */,
            CombatUnitRegistry roster /* 전투에 등록된 유닛 목록 */,
            SkillMultiEffectTargetSide side /* 진영 */)
        {
            if (roster == null || owner == null || owner.Identity == null)
            {
                return System.Array.Empty<CombatUnitEntry>();
            }

            var ownerIsEnemy = owner.Identity.Side == UnitSide.Enemy;
            switch (side)
            {
                case SkillMultiEffectTargetSide.Self:
                    var allies = roster.Players;
                    if (ownerIsEnemy)
                    {
                        allies = roster.Enemies;
                    }
                    var self = FindEntryForModel(owner, allies);
                    if (IsSkillTarget(self))
                    {
                        return new[] { self };
                    }
                    return System.Array.Empty<CombatUnitEntry>();
                case SkillMultiEffectTargetSide.AllAllies:
                    if (ownerIsEnemy)
                    {
                        return FilterSkillTargets(roster.Enemies);
                    }
                    return FilterSkillTargets(roster.Players);
                default:
                    if (ownerIsEnemy)
                    {
                        return FilterSkillTargets(roster.Players);
                    }
                    return FilterSkillTargets(roster.Enemies);
            }
        }

        /*
         * 스킬 대상을 조건에 맞는 값만 선별한다.
         */
        private static System.Collections.Generic.IReadOnlyList<CombatUnitEntry> FilterSkillTargets(
            System.Collections.Generic.IReadOnlyList<CombatUnitEntry> entries /* 등록 정보 목록 */)
        {
            if (entries == null || entries.Count == 0)
            {
                return System.Array.Empty<CombatUnitEntry>();
            }

            var filtered = new System.Collections.Generic.List<CombatUnitEntry>();
            for (var i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                if (!IsSkillTarget(entry))
                {
                    continue;
                }

                filtered.Add(entry);
            }

            return filtered;
        }

        /*
         * 유닛이 선택지 효과의 적용 대상인지 확인한다.
         */
        private static bool IsSkillTarget(CombatUnitEntry entry /* 처리할 등록 정보 */)
        {
            UnitIdentity identity = null;
            if (entry != null && entry.Model != null)
            {
                identity = entry.Model.Identity;
            }
            return entry != null && (identity == null || identity.Role != UnitRole.Nexus);
        }

        /*
         * 유닛 항목 대상 모델을 찾는다.
         */
        private static CombatUnitEntry FindEntryForModel(
            UnitCombatState model /* 전투 상태를 읽거나 변경할 유닛 */,
            System.Collections.Generic.IReadOnlyList<CombatUnitEntry> entries /* 등록 정보 목록 */)
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

        /*
         * 상태를 보유하고 있는지 확인한다.
         */
        private static bool HasStatus(UnitCombatState model /* 전투 상태를 읽거나 변경할 유닛 */, StatusEffectKind statusKind /* 상태 효과 종류 */, int minimumStacks = 1 /* 최소 중첩 수 */)
        {
            if (model == null || statusKind == StatusEffectKind.None || minimumStacks <= 0)
            {
                return false;
            }

            if (statusKind == StatusEffectKind.Shield)
            {
                return model.Resources != null && model.Resources.CurrentShield > 0f;
            }

            return model.Statuses != null && model.Statuses.GetStacks(statusKind) >= minimumStacks;
        }

        /*
         * 선택지 효과가 현재 스킬에 적용되는지 확인한다.
         */
        private static bool AppliesToSkill(SkillChoiceDefinition choice /* 적용하거나 검사할 스킬 선택지 */, SkillRuntimeData skillData /* 스킬 실행 데이터 */)
        {
            if (choice == null || skillData == null)
            {
                return false;
            }

            if (choice.NormalizedPlanNodes != null && choice.NormalizedPlanNodes.Length > 0)
            {
                return SkillNodeMapper.HasSkillNodeForTarget(
                    choice.NormalizedPlanNodes,
                    skillData.SkillId);
            }

            var targetSkillId = choice.SkillId;
            if (!string.IsNullOrWhiteSpace(choice.TargetSkillId))
            {
                targetSkillId = choice.TargetSkillId;
            }
            return !string.IsNullOrWhiteSpace(targetSkillId)
                && string.Equals(targetSkillId, skillData.SkillId, System.StringComparison.OrdinalIgnoreCase);
        }

        /*
         * 패시브에 연결된 강화 선택지를 Snapshot으로 만든다.
         */
        internal static SkillSnapshot ResolvePassiveChoices(UnitCombatState owner /* 정보를 소유한 유닛 */, string passiveId /* 패시브 식별자 */)
        {
            return ResolveChoices(owner, passiveId, true);
        }

        /*
         * 활성 스킬에 연결된 강화와 마스터 선택지를 Snapshot으로 만든다.
         */
        internal static SkillSnapshot ResolveActiveChoices(UnitCombatState owner /* 정보를 소유한 유닛 */, string skillId /* 스킬 식별자 */)
        {
            return ResolveChoices(owner, skillId, false);
        }

        /*
         * ResolveChoices 결과를 계산해 반환한다.
         */
        private static SkillSnapshot ResolveChoices(UnitCombatState owner /* 정보를 소유한 유닛 */, string skillId /* 스킬 식별자 */, bool useTargetSkillId /* 사용 대상 스킬 식별자 여부 */)
        {
            var snapshot = new SkillSnapshot(null);
            System.Collections.Generic.ICollection<string> chosenChoiceIds = null;
            if (owner != null && owner.SkillProgress != null)
            {
                chosenChoiceIds = owner.SkillProgress.ChosenChoiceIds;
            }
            if (chosenChoiceIds == null || chosenChoiceIds.Count == 0 || string.IsNullOrWhiteSpace(skillId))
            {
                return snapshot;
            }

            foreach (var choiceId in chosenChoiceIds)
            {
                var choice = owner.SkillRuntime.FindChoice(choiceId);
                if (choice == null)
                {
                    continue;
                }

                var choiceSkillId = choice.Source.SkillId;
                if (useTargetSkillId && !string.IsNullOrWhiteSpace(choice.Source.TargetSkillId))
                {
                    choiceSkillId = choice.Source.TargetSkillId;
                }

                if (!string.Equals(choiceSkillId, skillId, System.StringComparison.OrdinalIgnoreCase)
                    || !SkillRequirement.MeetsSourceStatus(choice.Source, owner))
                {
                    continue;
                }

                snapshot.AddActiveChoiceId(choice.ChoiceId);
                snapshot.ApplyChoiceSpec(choice);
            }

            return snapshot;
        }


    }
}
