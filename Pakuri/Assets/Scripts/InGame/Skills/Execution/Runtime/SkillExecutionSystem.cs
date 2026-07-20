using System.Collections.Generic;
using Pakuri.Data;
using UnityEngine;

namespace Pakuri.InGame
{
    /*
     * 스킬 실행 시스템의 실행 순서와 상태를 조율한다.
     */
    public sealed class SkillExecutionSystem
    {
        /*
         * 스킬 자동 전달 조건 함수 호출 형식을 정의한다.
         */
        public delegate bool SkillAutoRoutePredicate(UnitRosterEntry entry, SkillRuntimeInstance runtime);

        private readonly SkillExecutorRegistry registry = new SkillExecutorRegistry();
        private readonly SkillChoiceResolver choiceResolver = new SkillChoiceResolver();
        private readonly Dictionary<UnitRosterEntry, UnitSkillController> unitControllers =
            new Dictionary<UnitRosterEntry, UnitSkillController>();
        private readonly List<UnitRosterEntry> staleControllerEntries = new List<UnitRosterEntry>();

        /*
         * 로스터의 모든 유닛 스킬 상태와 자동 시전을 갱신한다.
         */
        public void Tick(
            UnitRosterService roster,
            InGameCombatManager combatManager,
            float deltaTime,
            bool logRoutedContracts,
            SkillAutoRoutePredicate canAutoRoute = null)
        {
            if (roster == null || deltaTime <= 0f)
            {
                return;
            }

            var entries = roster.Entries;
            PruneControllerCache(entries);

            for (var i = 0; i < entries.Count; i++)
            {
                TickEntry(entries[i], roster, combatManager, deltaTime, logRoutedContracts, canAutoRoute);
            }
        }

        /*
         * 수동을 실행하고 성공 여부를 반환한다.
         */
        public bool TryExecuteManual(
            UnitRosterEntry entry,
            SkillRuntimeInstance runtime,
            UnitRosterService roster,
            InGameCombatManager combatManager,
            float deltaTime,
            Vector2 aimDirection,
            Vector2 targetPoint,
            bool logRoutedContracts)
        {
            if (entry == null)
            {
                return false;
            }

            var controller = GetOrCreateController(entry);
            return controller.TryExecuteManual(
                runtime,
                roster,
                combatManager,
                deltaTime,
                aimDirection,
                targetPoint,
                logRoutedContracts);
        }

        /*
         * 처형 선택된을 가능한 상태인지 확인한다.
         */
        public bool CanExecuteSelected(
            UnitRosterEntry entry,
            SkillRuntimeInstance runtime,
            UnitRosterService roster)
        {
            if (entry == null
                || runtime == null
                || !StatusEffectRules.CanAct(entry.Model)
                || !registry.TryResolve(runtime.Data, out _))
            {
                return false;
            }

            var snapshot = choiceResolver.Resolve(entry.Model, runtime, roster);
            return runtime.CanCastWithSnapshot(snapshot);
        }

        /*
         * 선택된을 실행하고 성공 여부를 반환한다.
         */
        public bool TryExecuteSelected(
            UnitRosterEntry entry,
            SkillRuntimeInstance runtime,
            UnitRosterService roster,
            InGameCombatManager combatManager,
            float deltaTime,
            bool logRoutedContracts)
        {
            if (entry == null)
            {
                return false;
            }

            var controller = GetOrCreateController(entry);
            return controller.TryExecuteSelected(
                runtime,
                roster,
                combatManager,
                deltaTime,
                logRoutedContracts);
        }

        /*
         * 트리거된을 실행하고 성공 여부를 반환한다.
         */
        public bool TryExecuteTriggered(
            UnitRosterEntry entry,
            SkillRuntimeInstance runtime,
            UnitRosterService roster,
            InGameCombatManager combatManager,
            bool logRoutedContracts,
            Vector2 targetPoint,
            bool hasTargetPoint,
            float triggeredDamageMultiplier = 1f,
            string triggerSourceSkillId = null)
        {
            if (runtime == null || entry == null)
            {
                return false;
            }

            var aimDirection = entry.Transform != null && hasTargetPoint
                ? targetPoint - (Vector2)entry.Transform.position
                : default;
            var hasAimDirection = hasTargetPoint && aimDirection.sqrMagnitude > 0.0001f;
            return TryExecuteTriggeredSkill(
                entry,
                runtime,
                roster,
                combatManager,
                logRoutedContracts,
                hasAimDirection,
                aimDirection,
                hasTargetPoint,
                targetPoint,
                triggeredDamageMultiplier,
                triggerSourceSkillId);
        }

        /*
         * 유닛 항목을 시간 흐름에 따라 갱신한다.
         */
        private void TickEntry(
            UnitRosterEntry entry,
            UnitRosterService roster,
            InGameCombatManager combatManager,
            float deltaTime,
            bool logRoutedContracts,
            SkillAutoRoutePredicate canAutoRoute)
        {
            if (entry == null)
            {
                return;
            }

            var controller = GetOrCreateController(entry);
            controller.Tick(roster, combatManager, deltaTime, logRoutedContracts, canAutoRoute);
        }

        /*
         * 유닛의 스킬 컨트롤러를 조회하고 없으면 생성한다.
         */
        private UnitSkillController GetOrCreateController(UnitRosterEntry entry)
        {
            if (!unitControllers.TryGetValue(entry, out var controller))
            {
                controller = new UnitSkillController(entry, TryRouteSkill);
                unitControllers.Add(entry, controller);
            }

            return controller;
        }

        /*
         * 컨트롤러 캐시를 더 이상 필요한 값이 아닌 항목을 정리한다.
         */
        private void PruneControllerCache(IReadOnlyList<UnitRosterEntry> activeEntries)
        {
            if (unitControllers.Count == 0)
            {
                return;
            }

            staleControllerEntries.Clear();
            foreach (var pair in unitControllers)
            {
                if (!ContainsEntry(activeEntries, pair.Key))
                {
                    staleControllerEntries.Add(pair.Key);
                }
            }

            for (var i = 0; i < staleControllerEntries.Count; i++)
            {
                unitControllers.Remove(staleControllerEntries[i]);
            }

            staleControllerEntries.Clear();
        }

        /*
         * 유닛 항목을 포함하는지 확인한다.
         */
        private static bool ContainsEntry(IReadOnlyList<UnitRosterEntry> entries, UnitRosterEntry candidate)
        {
            for (var i = 0; i < entries.Count; i++)
            {
                if (ReferenceEquals(entries[i], candidate))
                {
                    return true;
                }
            }

            return false;
        }

        /*
         * 스킬 데이터에 맞는 실행기로 요청 전달을 시도한다.
         */
        private bool TryRouteSkill(SkillExecutionRequest request)
        {
            var entry = request.Entry;
            var runtime = request.Runtime;
            if (runtime == null || entry == null || !StatusEffectRules.CanAct(entry.Model))
            {
                return false;
            }

            // 실행 직전에 학습 선택지를 반영한 스냅샷으로 시전 가능 여부를 판단한다.
            var snapshot = choiceResolver.Resolve(entry != null ? entry.Model : null, runtime, request.Roster);
            if (!runtime.CanCastWithSnapshot(snapshot))
            {
                return false;
            }

            if (!registry.TryResolve(runtime.Data, out var executor))
            {
                return false;
            }

            var context = new SkillExecutionContext(
                request.CombatManager,
                request.Roster,
                entry,
                runtime,
                request.DeltaTime,
                hasManualAimDirection: request.HasManualAimDirection,
                manualAimDirection: request.ManualAimDirection,
                hasManualTargetPoint: request.HasManualTargetPoint,
                manualTargetPoint: request.ManualTargetPoint);
            var result = executor.Execute(context, snapshot);
            if (result.Routed)
            {
                // 실행기가 요청을 처리한 경우에만 재사용 대기시간과 시전 트리거를 시작한다.
                if (!runtime.TryBeginCast(snapshot))
                {
                    return false;
                }

                request.NotifyActiveSkillAnimation?.Invoke(entry);
                NotifySkillCastTriggers(request.CombatManager, request.Roster, entry, runtime, context);
                if (request.LogRoutedContracts)
                {
                    Debug.Log($"Skill execution contract routed '{result.SkillId}' through {result.ExecutorName}.");
                }
            }

            return result.Routed;
        }

        /*
         * 스킬 시전 사실을 트리거 런타임에 전달한다.
         */
        private static void NotifySkillCastTriggers(
            InGameCombatManager combatManager,
            UnitRosterService roster,
            UnitRosterEntry entry,
            SkillRuntimeInstance runtime,
            SkillExecutionContext context,
            string triggerSourceSkillId = null)
        {
            if (combatManager == null || entry == null || runtime == null || runtime.Data == null)
            {
                return;
            }

            var center = context != null && context.HasManualTargetPoint
                ? context.ManualTargetPoint
                : entry.Transform != null ? (Vector2)entry.Transform.position : Vector2.zero;
            SkillTriggerRuntime.ExecuteSkillCast(
                combatManager,
                roster,
                entry.Model,
                runtime.Data.SkillId,
                center,
                triggerSourceSkillId);
        }

        /*
         * 트리거된 스킬을 실행하고 성공 여부를 반환한다.
         */
        private bool TryExecuteTriggeredSkill(
            UnitRosterEntry entry,
            SkillRuntimeInstance runtime,
            UnitRosterService roster,
            InGameCombatManager combatManager,
            bool logRoutedContracts,
            bool hasManualAimDirection,
            Vector2 manualAimDirection,
            bool hasManualTargetPoint,
            Vector2 manualTargetPoint,
            float triggeredDamageMultiplier,
            string triggerSourceSkillId)
        {
            if (runtime == null || entry == null)
            {
                return false;
            }

            var snapshot = choiceResolver.Resolve(entry.Model, runtime, roster);
            if (!Mathf.Approximately(triggeredDamageMultiplier, 1f))
            {
                snapshot.ApplyDynamicDamageMultiplier(triggeredDamageMultiplier);
            }

            if (!registry.TryResolve(runtime.Data, out var executor))
            {
                return false;
            }

            var context = new SkillExecutionContext(
                combatManager,
                roster,
                entry,
                runtime,
                0f,
                hasManualAimDirection: hasManualAimDirection,
                manualAimDirection: manualAimDirection,
                hasManualTargetPoint: hasManualTargetPoint,
                manualTargetPoint: manualTargetPoint);
            var result = executor.Execute(context, snapshot);
            if (result.Routed)
            {
                NotifySkillCastTriggers(combatManager, roster, entry, runtime, context, triggerSourceSkillId);
                if (logRoutedContracts)
                {
                    Debug.Log($"Triggered skill execution routed '{result.SkillId}' through {result.ExecutorName}.");
                }
            }

            return result.Routed;
        }
    }

    /*
     * 스킬 데이터 형식에 맞는 실행기를 등록하고 조회한다.
     */
    internal sealed class SkillExecutorRegistry
    {
        private readonly System.Collections.Generic.List<IInGameSkillExecutor> executors =
            new System.Collections.Generic.List<IInGameSkillExecutor>();

        /*
         * 스킬 등록소에 필요한 값을 초기화한다.
         */
        public SkillExecutorRegistry()
        {
            RegisterDefaults();
        }

        public int Count => executors.Count;

        /*
         * 스킬 실행기를 자료형 기준으로 등록한다.
         */
        public void Register(IInGameSkillExecutor executor)
        {
            if (executor != null && !executors.Contains(executor))
            {
                executors.Add(executor);
            }
        }

        /*
         * 스킬 데이터에 맞는 실행기를 찾는다.
         */
        public bool TryResolve(SkillRuntimeData skillData, out IInGameSkillExecutor executor)
        {
            executor = null;
            if (skillData == null)
            {
                return false;
            }

            for (var i = 0; i < executors.Count; i++)
            {
                if (executors[i] != null && executors[i].CanExecute(skillData))
                {
                    executor = executors[i];
                    return true;
                }
            }

            return false;
        }

        /*
         * 기본 실행기를 등록한다.
         */
        private void RegisterDefaults()
        {
            Register(new ProjectileSkillExecutor());
            Register(new BeamSkillExecutor());
            Register(new SingleAttackSkillExecutor());
            Register(new ZoneSkillExecutor());
            Register(new BuffSkillExecutor());
            Register(new ShieldSkillExecutor());
            Register(new HealSkillExecutor());
            Register(new ChainAttackSkillExecutor());
            Register(new ChargeSkillExecutor());
        }
    }

    /*
     * 학습한 선택지를 스킬 실행 상태에 적용한다.
     */
    internal sealed class SkillChoiceResolver
    {
        /*
         * 유닛이 학습한 선택지를 현재 스킬 실행 정보에 적용한다.
         */
        public SkillExecutionSnapshot Resolve(BaseUnitRuntimeModel owner, SkillRuntimeInstance runtime)
        {
            return Resolve(owner, runtime, null);
        }

        /*
         * 유닛이 학습한 선택지를 현재 스킬 실행 정보에 적용한다.
         */
        public SkillExecutionSnapshot Resolve(BaseUnitRuntimeModel owner, SkillRuntimeInstance runtime, UnitRosterService roster)
        {
            var skillData = runtime != null ? runtime.Data : null;
            var snapshot = new SkillExecutionSnapshot(skillData);
            ApplyPassiveBaseModifiers(snapshot, owner as MonsterUnitRuntimeModel, skillData);
            var chosenChoiceIds = owner != null && owner.State != null
                ? owner.State.ChosenChoiceIds
                : null;
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
            SkillExecutionSnapshot snapshot,
            MonsterUnitRuntimeModel owner,
            SkillRuntimeData skillData)
        {
            if (snapshot == null
                || owner == null
                || owner.State == null
                || skillData == null
                || owner.State.LearnedPassiveSkillIds == null
                || owner.State.LearnedPassiveSkillIds.Count == 0)
            {
                return;
            }

            foreach (var passiveId in owner.State.LearnedPassiveSkillIds)
            {
                var passive = owner.SkillRuntime.FindBySkillId(passiveId)?.Data as PassiveSkillRuntimeData;
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
            SkillExecutionSnapshot snapshot,
            System.Collections.Generic.ICollection<string> chosenChoiceIds,
            SkillRuntimeData skillData,
            BaseUnitRuntimeModel owner,
            UnitRosterService roster)
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
                    && MeetsSourceStatusRequirement(choice.Source, owner))
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
            SkillExecutionSnapshot snapshot,
            SkillChoiceDefinition choice,
            BaseUnitRuntimeModel owner,
            UnitRosterService roster)
        {
            if (snapshot == null || choice == null || roster == null)
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(choice.CountStatusId)
                && choice.DamageMultiplierPerCount > 0f)
            {
                ApplyCountStatusDamageMultiplier(
                    snapshot,
                    owner,
                    roster,
                    choice.CountTargetSide,
                    choice.CountStatusId,
                    choice.DamageMultiplierPerCount,
                    choice.CountMax);
            }

            var targetNodes = SkillRuntimeCompiler.FilterSkillNodeDefinitionsForTarget(
                choice.NormalizedPlanNodes,
                snapshot.SkillId);
            var nodes = SkillRuntimeCompiler.MapSkillNodeDefinitions(targetNodes);
            for (var i = 0; i < nodes.Length; i++)
            {
                var action = nodes[i] != null ? nodes[i].Action : null;
                if (!action.HasValue || action.Value.Kind != SkillActionOpKind.CountStatusDamageMultiplier)
                {
                    continue;
                }

                ApplyCountStatusDamageMultiplier(
                    snapshot,
                    owner,
                    roster,
                    action.Value.TargetSide,
                    action.Value.StringValue,
                    action.Value.FloatValue,
                    action.Value.IntValue);
            }
        }

        /*
         * 횟수 상태 피해 배율을 적용한다.
         */
        private static void ApplyCountStatusDamageMultiplier(
            SkillExecutionSnapshot snapshot,
            BaseUnitRuntimeModel owner,
            UnitRosterService roster,
            SkillMultiEffectTargetSide targetSide,
            string statusId,
            float amountPerCount,
            int countMax)
        {
            if (snapshot == null
                || string.IsNullOrWhiteSpace(statusId)
                || amountPerCount <= 0f
                || roster == null)
            {
                return;
            }

            var count = CountMatchingTargets(owner, roster, targetSide, statusId);
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
            BaseUnitRuntimeModel owner,
            UnitRosterService roster,
            SkillMultiEffectTargetSide side,
            string statusId)
        {
            if (owner == null || roster == null || string.IsNullOrWhiteSpace(statusId))
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

                if (HasStatus(entry.Model, statusId))
                {
                    count++;
                }
            }

            return count;
        }

        /*
         * 횟수 유닛 항목을 결정한다.
         */
        private static System.Collections.Generic.IReadOnlyList<UnitRosterEntry> ResolveCountEntries(
            BaseUnitRuntimeModel owner,
            UnitRosterService roster,
            SkillMultiEffectTargetSide side)
        {
            if (roster == null || owner == null || owner.Identity == null)
            {
                return System.Array.Empty<UnitRosterEntry>();
            }

            var ownerIsEnemy = owner.Identity.Side == UnitSide.Enemy;
            switch (side)
            {
                case SkillMultiEffectTargetSide.Self:
                    var self = FindEntryForModel(owner, ownerIsEnemy ? roster.Enemies : roster.Players);
                    return IsSkillTarget(self) ? new[] { self } : System.Array.Empty<UnitRosterEntry>();
                case SkillMultiEffectTargetSide.AllAllies:
                    return FilterSkillTargets(ownerIsEnemy ? roster.Enemies : roster.Players);
                default:
                    return FilterSkillTargets(ownerIsEnemy ? roster.Players : roster.Enemies);
            }
        }

        /*
         * 스킬 대상을 조건에 맞는 값만 선별한다.
         */
        private static System.Collections.Generic.IReadOnlyList<UnitRosterEntry> FilterSkillTargets(
            System.Collections.Generic.IReadOnlyList<UnitRosterEntry> entries)
        {
            if (entries == null || entries.Count == 0)
            {
                return System.Array.Empty<UnitRosterEntry>();
            }

            var filtered = new System.Collections.Generic.List<UnitRosterEntry>();
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
        private static bool IsSkillTarget(UnitRosterEntry entry)
        {
            var identity = entry != null && entry.Model != null ? entry.Model.Identity : null;
            return entry != null && (identity == null || identity.Role != UnitRole.Nexus);
        }

        /*
         * 유닛 항목 대상 모델을 찾는다.
         */
        private static UnitRosterEntry FindEntryForModel(
            BaseUnitRuntimeModel model,
            System.Collections.Generic.IReadOnlyList<UnitRosterEntry> entries)
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
        private static bool HasStatus(BaseUnitRuntimeModel model, string statusId, int minimumStacks = 1)
        {
            if (model == null || string.IsNullOrWhiteSpace(statusId) || minimumStacks <= 0)
            {
                return false;
            }

            if (!StatusEffectUtility.TryParse(statusId, out var kind))
            {
                return false;
            }

            if (kind == StatusEffectKind.Shield)
            {
                return model.Resources != null && model.Resources.CurrentShield > 0f;
            }

            return model.Statuses != null && model.Statuses.GetStacks(kind) >= minimumStacks;
        }

        /*
         * 선택지 효과가 현재 스킬에 적용되는지 확인한다.
         */
        private static bool AppliesToSkill(SkillChoiceDefinition choice, SkillRuntimeData skillData)
        {
            if (choice == null || skillData == null)
            {
                return false;
            }

            if (choice.NormalizedPlanNodes != null && choice.NormalizedPlanNodes.Length > 0)
            {
                return SkillRuntimeCompiler.HasSkillNodeForTarget(
                    choice.NormalizedPlanNodes,
                    skillData.SkillId);
            }

            var targetSkillId = !string.IsNullOrWhiteSpace(choice.TargetSkillId)
                ? choice.TargetSkillId
                : choice.SkillId;
            return !string.IsNullOrWhiteSpace(targetSkillId)
                && string.Equals(targetSkillId, skillData.SkillId, System.StringComparison.OrdinalIgnoreCase);
        }

        /*
         * 출처 유닛이 필수 상태와 최소 중첩을 만족하는지 확인한다.
         */
        private static bool MeetsSourceStatusRequirement(SkillChoiceDefinition choice, BaseUnitRuntimeModel owner)
        {
            if (choice == null || string.IsNullOrWhiteSpace(choice.RequiredSourceStatusId))
            {
                return true;
            }

            return HasStatus(owner, choice.RequiredSourceStatusId, Mathf.Max(1, choice.RequiredSourceStatusMinStacks));
        }

    }
}
