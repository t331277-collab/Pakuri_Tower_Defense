using System;
using System.Collections.Generic;
using Pakuri.Data;
using UnityEngine;

namespace Pakuri.InGame
{
    internal sealed class InGamePassiveEffectRuntime
    {
        private const int MaxRefreshPasses = 8;

        private readonly HashSet<string> appliedOneShotEffectKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, PassiveStatusBinding> activeStatusBindings =
            new Dictionary<string, PassiveStatusBinding>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, float> triggerCooldowns =
            new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, int> triggerCounts =
            new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<float> healthRatioThresholds = new HashSet<float>();
        private readonly List<string> staleBindingKeys = new List<string>();
        private bool refreshRequested;
        private bool isRefreshing;

        public void Reset()
        {
            appliedOneShotEffectKeys.Clear();
            activeStatusBindings.Clear();
            triggerCooldowns.Clear();
            triggerCounts.Clear();
            healthRatioThresholds.Clear();
            staleBindingKeys.Clear();
            refreshRequested = true;
            isRefreshing = false;
        }

        /*
         * 패시브 Trigger의 재사용 대기시간을 확인하고 다음 준비 시간을 기록한다.
         */
        public bool ConsumeTriggerCooldown(string key, float cooldownSeconds)
        {
            var now = Time.time;
            if (triggerCooldowns.TryGetValue(key, out var readyAt) && readyAt > now)
            {
                return false;
            }

            if (cooldownSeconds > 0f)
            {
                triggerCooldowns[key] = now + cooldownSeconds;
            }
            else
            {
                triggerCooldowns.Remove(key);
            }

            return true;
        }

        /*
         * 패시브 Trigger 횟수를 누적하고 지정 주기마다 실행을 허용한다.
         */
        public bool ConsumeTriggerCount(string key, int triggerEveryCount)
        {
            if (triggerEveryCount <= 1)
            {
                return true;
            }

            triggerCounts.TryGetValue(key, out var currentCount);
            currentCount++;
            if (currentCount < triggerEveryCount)
            {
                triggerCounts[key] = currentCount;
                return false;
            }

            triggerCounts[key] = 0;
            return true;
        }

        /*
         * 체력과 보호막 변경을 패시브 조건 갱신에 반영한다.
         */
        public void NotifyResourceChanged(InGameResourceChangeResult result)
        {
            NotifyHealthChanged(result.Target, result.PreviousHealth, result.CurrentHealth);
            NotifyShieldChanged(result.Target, result.PreviousShield, result.CurrentShield);
        }

        // Code Builder: 로스터나 학습 구성이 바뀌면 다음 조율 시점에 전체 패시브 관계를 다시 만든다.
        public void NotifyRosterChanged()
        {
            refreshRequested = true;
        }

        // Code Builder: 상태 추가·중첩·제거·만료 시 조건형 패시브만 다시 계산하도록 표시한다.
        public void NotifyStatusChanged(BaseUnitRuntimeModel target)
        {
            if (target != null)
            {
                refreshRequested = true;
            }
        }

        // Code Builder: 보호막 보유 여부가 바뀐 경우에만 보호막 조건 패시브를 다시 계산한다.
        public void NotifyShieldChanged(BaseUnitRuntimeModel target, float previousShield, float currentShield)
        {
            if (target != null && (previousShield > 0f) != (currentShield > 0f))
            {
                refreshRequested = true;
            }
        }

        // Code Builder: 피해·회복 자체가 아니라 실제 체력 조건 경계를 통과할 때만 갱신을 요청한다.
        public void NotifyHealthChanged(BaseUnitRuntimeModel target, float previousHealth, float currentHealth)
        {
            var maxHealth = target != null && target.Stats != null ? target.Stats.MaxHealth : 0f;
            if (maxHealth <= 0f || Mathf.Approximately(previousHealth, currentHealth))
            {
                return;
            }

            var previousRatio = Mathf.Clamp01(previousHealth / maxHealth);
            var currentRatio = Mathf.Clamp01(currentHealth / maxHealth);
            foreach (var threshold in healthRatioThresholds)
            {
                if ((previousRatio <= threshold) != (currentRatio <= threshold))
                {
                    refreshRequested = true;
                    return;
                }
            }
        }

        // Code Builder: 관리자는 호출 순서만 정하고 실제 조건 검사와 효과 수명은 패시브 런타임이 소유한다.
        public void FlushPendingChanges(InGameCombatManager combatManager, UnitRosterService roster)
        {
            if (!refreshRequested || isRefreshing || combatManager == null || roster == null)
            {
                return;
            }

            isRefreshing = true;
            var passCount = 0;
            while (refreshRequested && passCount < MaxRefreshPasses)
            {
                refreshRequested = false;
                RefreshLearnedPassiveEffects(combatManager, roster);
                passCount++;
            }

            isRefreshing = false;
            if (refreshRequested)
            {
                refreshRequested = false;
                Debug.LogWarning("Passive effect refresh stopped after reaching its safety pass limit.");
            }
        }

        private void RefreshLearnedPassiveEffects(InGameCombatManager combatManager, UnitRosterService roster)
        {
            var desiredBindings = new Dictionary<string, PassiveStatusBinding>(StringComparer.OrdinalIgnoreCase);
            var nextHealthRatioThresholds = new HashSet<float>();

            var entries = roster.Entries;
            for (var i = 0; i < entries.Count; i++)
            {
                var ownerEntry = entries[i];
                var owner = ownerEntry != null ? ownerEntry.Model : null;
                var learnedPassives = owner != null && owner.State != null ? owner.State.LearnedPassiveSkillIds : null;
                if (ownerEntry == null || owner == null || learnedPassives == null || learnedPassives.Count == 0)
                {
                    continue;
                }

                foreach (var passiveId in learnedPassives)
                {
                    CollectPassiveEffects(
                        combatManager,
                        roster,
                        ownerEntry,
                        owner,
                        passiveId,
                        desiredBindings,
                        nextHealthRatioThresholds);
                }
            }

            RemoveInactiveBindings(combatManager, desiredBindings);

            healthRatioThresholds.Clear();
            foreach (var threshold in nextHealthRatioThresholds)
            {
                healthRatioThresholds.Add(threshold);
            }
        }

        private void CollectPassiveEffects(
            InGameCombatManager combatManager,
            UnitRosterService roster,
            UnitRosterEntry ownerEntry,
            BaseUnitRuntimeModel owner,
            string passiveId,
            IDictionary<string, PassiveStatusBinding> desiredBindings,
            ISet<float> nextHealthRatioThresholds)
        {
            if (string.IsNullOrWhiteSpace(passiveId)
                || PakuriDataManager.Instance == null
                || !PakuriDataManager.Instance.TryGetData(passiveId, out PassiveDefinition passive)
                || passive == null
                || passive.PassiveEffects == null
                || passive.PassiveEffects.Length == 0)
            {
                return;
            }

            var context = new SkillExecutionContext(combatManager, roster, ownerEntry, null, 0f);
            var fallbackCenter = ownerEntry.Transform != null ? (Vector2)ownerEntry.Transform.position : Vector2.zero;
            var snapshot = BuildPassiveChoiceSnapshot(owner, passiveId);
            for (var i = 0; i < passive.PassiveEffects.Length; i++)
            {
                var effect = passive.PassiveEffects[i];
                if (effect == null
                    || !HasAllLearnedPassives(owner, effect.RequiresPassiveSkillId)
                    || HasAnyLearnedPassive(owner, effect.ExcludesPassiveSkillId))
                {
                    continue;
                }

                if (effect.ApplyOnce)
                {
                    ApplyOneShotEffect(context, snapshot, effect, fallbackCenter, owner, passiveId);
                    continue;
                }

                if (!IsPersistentStatusEffect(effect)
                    || !SkillMultiEffectExecutor.ShouldRun(context, effect, snapshot))
                {
                    continue;
                }

                if (effect.ConditionHealthRatioMax > 0f)
                {
                    nextHealthRatioThresholds.Add(Mathf.Clamp01(effect.ConditionHealthRatioMax));
                }

                var statusSpec = SkillMultiEffectExecutor.ResolveStatusSpec(effect, snapshot);
                if (statusSpec == null || !statusSpec.Enabled || statusSpec.StatusData == null)
                {
                    continue;
                }

                var targets = SkillMultiEffectExecutor.ResolvePassiveStatusTargets(context, snapshot, effect);
                for (var targetIndex = 0; targetIndex < targets.Count; targetIndex++)
                {
                    var target = targets[targetIndex];
                    var binding = new PassiveStatusBinding(
                        target.Model,
                        statusSpec.StatusData.Kind,
                        statusSpec.StatusData.SourceSkillId);
                    var bindingKey = BuildBindingKey(owner, passiveId, effect, target.Model);
                    desiredBindings[bindingKey] = binding;

                    if (activeStatusBindings.TryGetValue(bindingKey, out var activeBinding)
                        && ReferenceEquals(activeBinding.Target, target.Model)
                        && activeBinding.Target.Statuses != null
                        && activeBinding.Target.Statuses.Has(activeBinding.Kind, activeBinding.SourceSkillId))
                    {
                        continue;
                    }

                    if (SkillMultiEffectExecutor.ApplyPersistentPassiveStatus(
                            context,
                            snapshot,
                            effect,
                            target,
                            fallbackCenter))
                    {
                        activeStatusBindings[bindingKey] = binding;
                    }
                    else
                    {
                        activeStatusBindings.Remove(bindingKey);
                    }
                }
            }
        }

        private void ApplyOneShotEffect(
            SkillExecutionContext context,
            SkillExecutionSnapshot snapshot,
            SkillEffectDefinition effect,
            Vector2 fallbackCenter,
            BaseUnitRuntimeModel owner,
            string passiveId)
        {
            if (!SkillMultiEffectExecutor.ShouldRun(context, effect, snapshot))
            {
                return;
            }

            var key = BuildOneShotKey(owner, passiveId, effect);
            if (appliedOneShotEffectKeys.Contains(key))
            {
                return;
            }

            SkillMultiEffectExecutor.Execute(context, snapshot, new[] { effect }, fallbackCenter);
            appliedOneShotEffectKeys.Add(key);
        }

        private void RemoveInactiveBindings(
            InGameCombatManager combatManager,
            IReadOnlyDictionary<string, PassiveStatusBinding> desiredBindings)
        {
            staleBindingKeys.Clear();
            foreach (var pair in activeStatusBindings)
            {
                if (desiredBindings.ContainsKey(pair.Key))
                {
                    continue;
                }

                var binding = pair.Value;
                combatManager.RemovePassiveStatus(binding.Target, binding.Kind, binding.SourceSkillId);
                staleBindingKeys.Add(pair.Key);
            }

            for (var i = 0; i < staleBindingKeys.Count; i++)
            {
                activeStatusBindings.Remove(staleBindingKeys[i]);
            }
        }

        private static bool IsPersistentStatusEffect(SkillEffectDefinition effect)
        {
            return effect != null
                && effect.EffectKind == SkillMultiEffectKind.Status
                && effect.EffectTiming == SkillMultiEffectTiming.OnCast
                && effect.DelaySeconds <= 0f;
        }

        private static SkillExecutionSnapshot BuildPassiveChoiceSnapshot(BaseUnitRuntimeModel owner, string passiveId)
        {
            var snapshot = new SkillExecutionSnapshot(null);
            var chosenChoiceIds = owner != null && owner.State != null ? owner.State.ChosenChoiceIds : null;
            if (chosenChoiceIds == null || chosenChoiceIds.Count == 0 || string.IsNullOrWhiteSpace(passiveId))
            {
                return snapshot;
            }

            var manager = PakuriDataManager.Instance;
            foreach (var choiceId in chosenChoiceIds)
            {
                if (manager == null || !manager.TryGetData(choiceId, out SkillChoiceDefinition choice) || choice == null)
                {
                    continue;
                }

                var targetSkillId = !string.IsNullOrWhiteSpace(choice.TargetSkillId)
                    ? choice.TargetSkillId
                    : choice.SkillId;
                if (!string.Equals(targetSkillId, passiveId, System.StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (!MeetsSourceStatusRequirement(choice, owner))
                {
                    continue;
                }

                snapshot.AddActiveChoiceId(choice.ChoiceId);
                snapshot.ApplyChoiceDefinition(choice);
            }

            return snapshot;
        }

        private static bool MeetsSourceStatusRequirement(SkillChoiceDefinition choice, BaseUnitRuntimeModel owner)
        {
            if (choice == null || string.IsNullOrWhiteSpace(choice.RequiredSourceStatusId))
            {
                return true;
            }

            if (!StatusEffectUtility.TryParse(choice.RequiredSourceStatusId, out var kind))
            {
                return false;
            }

            if (kind == StatusEffectKind.Shield)
            {
                return owner != null
                    && owner.Resources != null
                    && owner.Resources.CurrentShield > 0f;
            }

            return owner != null
                && owner.Statuses != null
                && owner.Statuses.GetStacks(kind) >= Mathf.Max(1, choice.RequiredSourceStatusMinStacks);
        }

        private static bool HasAllLearnedPassives(BaseUnitRuntimeModel owner, string passiveList)
        {
            if (string.IsNullOrWhiteSpace(passiveList))
            {
                return true;
            }

            var passives = passiveList.Split(';', ',');
            for (var i = 0; i < passives.Length; i++)
            {
                var passiveId = passives[i];
                if (!string.IsNullOrWhiteSpace(passiveId) && !HasLearnedPassive(owner, passiveId.Trim()))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool HasAnyLearnedPassive(BaseUnitRuntimeModel owner, string passiveList)
        {
            if (string.IsNullOrWhiteSpace(passiveList))
            {
                return false;
            }

            var passives = passiveList.Split(';', ',');
            for (var i = 0; i < passives.Length; i++)
            {
                var passiveId = passives[i];
                if (!string.IsNullOrWhiteSpace(passiveId) && HasLearnedPassive(owner, passiveId.Trim()))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool HasLearnedPassive(BaseUnitRuntimeModel owner, string passiveId)
        {
            return owner != null
                && owner.State != null
                && !string.IsNullOrWhiteSpace(passiveId)
                && owner.State.LearnedPassiveSkillIds.Contains(passiveId);
        }

        private static string BuildOneShotKey(BaseUnitRuntimeModel owner, string passiveId, SkillEffectDefinition effect)
        {
            var unitId = ResolveUnitKey(owner);
            var effectId = !string.IsNullOrWhiteSpace(effect.EffectId) ? effect.EffectId : effect.SkillId;
            return unitId + ":" + passiveId + ":" + effectId;
        }

        private static string BuildBindingKey(
            BaseUnitRuntimeModel owner,
            string passiveId,
            SkillEffectDefinition effect,
            BaseUnitRuntimeModel target)
        {
            var effectId = !string.IsNullOrWhiteSpace(effect.EffectId) ? effect.EffectId : effect.SkillId;
            return ResolveUnitKey(owner)
                + ":" + passiveId
                + ":" + effectId
                + ":" + ResolveUnitKey(target);
        }

        private static string ResolveUnitKey(BaseUnitRuntimeModel unit)
        {
            return unit != null && unit.Identity != null && !string.IsNullOrWhiteSpace(unit.Identity.UnitId)
                ? unit.Identity.UnitId
                : unit != null ? unit.GetHashCode().ToString() : "unknown";
        }

        private readonly struct PassiveStatusBinding
        {
            public PassiveStatusBinding(BaseUnitRuntimeModel target, StatusEffectKind kind, string sourceSkillId)
            {
                Target = target;
                Kind = kind;
                SourceSkillId = sourceSkillId;
            }

            public BaseUnitRuntimeModel Target { get; }
            public StatusEffectKind Kind { get; }
            public string SourceSkillId { get; }
        }
    }
}
