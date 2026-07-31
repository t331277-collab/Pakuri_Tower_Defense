/*
 * 역할: 스킬 반응 전달.
 * 책임: Trigger 조건을 평가하고 설정된 후속 결과를 예약하거나 실행한다.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Pakuri.Combat;
using Pakuri.Data;
using UnityEngine;

namespace Pakuri.InGame
{

/// 전투 사건 반응 조건을 평가하고 설정된 후속 스킬 결과를 실행한다.
internal static class SkillTrigger
{

	/// TriggerGateState의 변경 가능한 런타임 상태를 보관한다.
	private sealed class TriggerGateState
	{
		private readonly Dictionary<string, float> cooldowns =
			new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase);
		private readonly Dictionary<string, int> counts =
			new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

		/// 전달된 런타임 입력값을 사용해 Cooldown를 현재 런타임 상태에서 소비한다.
		public bool ConsumeCooldown(string key, float cooldownSeconds)
		{
			float now = Time.time;
			if (cooldowns.TryGetValue(key, out float readyAt) && readyAt > now)
			{
				return false;
			}

			if (cooldownSeconds > 0f)
			{
				cooldowns[key] = now + cooldownSeconds;
			}
			else
			{
				cooldowns.Remove(key);
			}

			return true;
		}

		/// 전달된 런타임 입력값을 사용해 Count를 현재 런타임 상태에서 소비한다.
		public bool ConsumeCount(string key, int triggerEveryCount)
		{
			if (triggerEveryCount <= 1)
			{
				return true;
			}

			counts.TryGetValue(key, out int currentCount);
			currentCount++;
			if (currentCount < triggerEveryCount)
			{
				counts[key] = currentCount;
				return false;
			}

			counts[key] = 0;
			return true;
		}
	}

	private static readonly ConditionalWeakTable<InGameCombatManager, TriggerGateState> gateStates =
		new ConditionalWeakTable<InGameCombatManager, TriggerGateState>();

	/// 전달된 combatManager 값을 사용해 소유한 모든 런타임 값를 초기 런타임 상태로 되돌린다.
	internal static void Reset(InGameCombatManager combatManager)
	{
		if (combatManager != null)
		{
			gateStates.Remove(combatManager);
		}
	}

	/// 전달된 baseTriggers 값을 사용해 Triggers를 결과 컬렉션에 수집한다.
	private static SkillReaction[] CollectTriggers(
		IReadOnlyList<SkillReaction> baseTriggers)
	{
		var triggers = new List<SkillReaction>();
		if (baseTriggers != null)
		{
			for (var i = 0; i < baseTriggers.Count; i++)
			{
				if (baseTriggers[i] != null)
				{
					triggers.Add(baseTriggers[i]);
				}
			}
		}

		return triggers.ToArray();
	}

	/// TriggerExecutionContext 처리에 함께 전달되는 값들을 묶는다.
	internal readonly struct TriggerExecutionContext
	{
		private readonly float[] trackedIncomingDamage;

		public UnitCombatState EventTarget { get; }

		public UnitCombatState Attacker { get; }

		public Vector2 EventCenter { get; }

		public StatusRuntimeInstance Status { get; }

		public float ShieldAbsorbedAmount { get; }

		public float ShieldAppliedAmount { get; }

		public float ShieldRemainingAmount { get; }

		public float EventAppliedDamage { get; }

		public DamageAttribute EventAttribute { get; }

		public string EventSourceSkillId { get; }

		public UnitCombatState EventSource { get; }

		public bool EventWasExecute { get; }

		public string EventTriggerSourceSkillId { get; }

		public int EventHitCount { get; }

		public int RecastGeneration { get; }

		/// TriggerExecutionContext 인스턴스를 전달된 런타임 입력값으로 초기화한다.
		public TriggerExecutionContext(UnitCombatState eventTarget, UnitCombatState attacker, Vector2 eventCenter, StatusRuntimeInstance status, float shieldAbsorbedAmount, float eventAppliedDamage, DamageAttribute eventAttribute, string eventSourceSkillId, UnitCombatState eventSource = null, bool eventWasExecute = false, string eventTriggerSourceSkillId = null, int eventHitCount = 0, int recastGeneration = 0)
		{
			EventTarget = eventTarget;
			Attacker = attacker;
			EventCenter = eventCenter;
			Status = status;
			ShieldAbsorbedAmount = shieldAbsorbedAmount;
			ShieldAppliedAmount = status != null ? status.AppliedShieldAmount : 0f;
			ShieldRemainingAmount = status != null ? status.RemainingShieldAmount : 0f;
			trackedIncomingDamage = new float[(int)DamageAttribute.Holy + 1];
			if (status != null)
			{
				for (var i = 0; i < trackedIncomingDamage.Length; i++)
				{
					trackedIncomingDamage[i] =
						status.GetTrackedIncomingDamage((DamageAttribute)i);
				}
			}
			EventAppliedDamage = eventAppliedDamage;
			EventAttribute = eventAttribute;
			EventSourceSkillId = eventSourceSkillId;
			EventSource = eventSource;
			EventWasExecute = eventWasExecute;
			EventTriggerSourceSkillId = eventTriggerSourceSkillId;
			EventHitCount = Mathf.Max(0, eventHitCount);
			RecastGeneration = Mathf.Max(0, recastGeneration);
		}

		/// 전달된 attribute 값을 사용해 TrackedIncomingDamage 결과값을 생성해 반환한다.
		public float TrackedIncomingDamage(DamageAttribute attribute)
		{
			var index = (int)attribute;
			return trackedIncomingDamage != null
				&& index >= 0
				&& index < trackedIncomingDamage.Length
					? trackedIncomingDamage[index]
					: 0f;
		}
	}

	/// 전달된 런타임 입력값을 사용해 PublishLifecycleEvent 작업을 수행한다.
	internal static void PublishLifecycleEvent(
		SkillTriggerEvent triggerEvent,
		SkillActionContext actionContext)
	{
		if (actionContext == null
			|| actionContext.Source == null
			|| actionContext.ExecutionContext == null
			|| !actionContext.ExecutionContext.PublishSkillLifecycleEvents)
		{
			return;
		}

		SkillExecutionContext executionContext = actionContext.ExecutionContext;
		if (executionContext.CombatManager == null || executionContext.Roster == null)
		{
			return;
		}

		var triggerContext = new TriggerExecutionContext(
			actionContext.EventTarget,
			actionContext.Source,
			actionContext.EventCenter,
			null,
			0f,
			actionContext.EventDamage,
			DamageAttribute.Physical,
			actionContext.SourceSkillId,
			actionContext.Source,
			eventHitCount: actionContext.HitCount,
			recastGeneration: executionContext.RecastGeneration);
		ExecuteSourceOwnedTriggers(
			executionContext.CombatManager,
			executionContext.Roster,
			actionContext.Source,
			actionContext.SourceSkillId,
			triggerEvent,
			triggerContext);
		ExecutePassiveOwnerTriggers(
			executionContext.CombatManager,
			executionContext.Roster,
			triggerEvent,
			triggerContext);
	}

	/// 전달된 런타임 입력값을 사용해 ProjectileHit를 실행한다.
	public static void ExecuteProjectileHit(InGameCombatManager combatManager, UnitSpawnManager roster, UnitCombatState source, string sourceSkillId, bool isMagazineLastProjectile, Vector2 eventCenter)
	{
		if (isMagazineLastProjectile)
		{
			ExecuteSourceOwnedTriggers(combatManager, roster, source, sourceSkillId, SkillTriggerEvent.OnMagazineLastProjectileHit, new TriggerExecutionContext(source, null, eventCenter, null, 0f, 0f, DamageAttribute.Physical, sourceSkillId, source));
		}
	}

	/// 전달된 런타임 입력값을 사용해 CombatStart를 실행한다.
	public static void ExecuteCombatStart(InGameCombatManager combatManager, UnitSpawnManager roster, UnitCombatState source)
	{
		IReadOnlyList<SkillUseState> readOnlyList = null;
		if (source != null && source.Skills != null)
		{
			readOnlyList = source.SkillState.ActiveSkills;
		}
		if (combatManager == null || roster == null || source == null || readOnlyList == null)
		{
			return;
		}
		Vector2 eventCenter = UnitPosition(roster, source);
		for (int i = 0; i < readOnlyList.Count; i++)
		{
			SkillUseState skillRuntimeInstance = readOnlyList[i];
			string text = ((skillRuntimeInstance != null && skillRuntimeInstance.Data != null) ? skillRuntimeInstance.Data.SkillId : string.Empty);
			if (!string.IsNullOrWhiteSpace(text))
			{
				ExecuteSourceOwnedTriggers(combatManager, roster, source, text, SkillTriggerEvent.CombatStart, new TriggerExecutionContext(source, source, eventCenter, null, 0f, 0f, DamageAttribute.Physical, text, source));
			}
		}
	}

	/// 전달된 런타임 입력값을 사용해 ShieldExpire를 실행한다.
	public static void ExecuteShieldExpire(InGameCombatManager combatManager, UnitSpawnManager roster, UnitCombatState shieldTarget, StatusRuntimeInstance shieldStatus)
	{
		if (shieldTarget != null && shieldStatus != null && shieldStatus.IsShieldStatus)
		{
			UnitCombatState unitState = SourceModel(roster, shieldStatus.SourceUnitId, shieldStatus.SourceDefinitionId);
			string text = ((!string.IsNullOrWhiteSpace(shieldStatus.SourceSkillId)) ? shieldStatus.SourceSkillId : string.Empty);
			Vector2 eventCenter = UnitPosition(roster, shieldTarget);
			TriggerExecutionContext triggerContext = new TriggerExecutionContext(shieldTarget, null, eventCenter, shieldStatus, 0f, 0f, DamageAttribute.Physical, text, unitState);
			ExecuteSourceOwnedTriggers(combatManager, roster, unitState, text, SkillTriggerEvent.OnShieldExpire, triggerContext);
			ExecutePassiveOwnerTriggers(combatManager, roster, SkillTriggerEvent.OnShieldExpire, triggerContext);
		}
	}

	/// 전달된 런타임 입력값을 사용해 ShieldAbsorb를 실행한다.
	public static void ExecuteShieldAbsorb(InGameCombatManager combatManager, UnitSpawnManager roster, UnitCombatState shieldTarget, UnitCombatState attacker, StatusRuntimeInstance shieldStatus, float absorbedAmount)
	{
		if (shieldTarget != null && shieldStatus != null && shieldStatus.IsShieldStatus && !(absorbedAmount <= 0f))
		{
			UnitCombatState unitState = SourceModel(roster, shieldStatus.SourceUnitId, shieldStatus.SourceDefinitionId);
			string text = ((!string.IsNullOrWhiteSpace(shieldStatus.SourceSkillId)) ? shieldStatus.SourceSkillId : string.Empty);
			Vector2 eventCenter = ((attacker != null) ? UnitPosition(roster, attacker) : UnitPosition(roster, shieldTarget));
			TriggerExecutionContext triggerContext = new TriggerExecutionContext(attacker, attacker, eventCenter, shieldStatus, absorbedAmount, 0f, DamageAttribute.Physical, text, unitState);
			ExecuteSourceOwnedTriggers(combatManager, roster, unitState, text, SkillTriggerEvent.OnShieldAbsorb, triggerContext);
			ExecutePassiveOwnerTriggers(combatManager, roster, SkillTriggerEvent.OnShieldAbsorb, triggerContext);
		}
	}

	/// 전달된 런타임 입력값을 사용해 ShieldAbsorbs를 실행한다.
	public static void ExecuteShieldAbsorbs(InGameCombatManager combatManager, UnitSpawnManager roster, UnitCombatState shieldTarget, UnitCombatState attacker, IReadOnlyList<ShieldAbsorptionRecord> absorbedShields)
	{
		for (int i = 0; i < absorbedShields.Count; i++)
		{
			ShieldAbsorptionRecord shieldAbsorbRecord = absorbedShields[i];
			if (!(shieldAbsorbRecord.AbsorbedAmount <= 0f))
			{
				ExecuteShieldAbsorb(combatManager, roster, shieldTarget, attacker, shieldAbsorbRecord.Status, shieldAbsorbRecord.AbsorbedAmount);
			}
		}
	}

	/// 전달된 런타임 입력값을 사용해 StatusExpire를 실행한다.
	public static void ExecuteStatusExpire(InGameCombatManager combatManager, UnitSpawnManager roster, UnitCombatState statusOwner, StatusRuntimeInstance status)
	{
		if (statusOwner != null && status != null)
		{
			UnitCombatState unitState = SourceModel(roster, status.SourceUnitId, status.SourceDefinitionId);
			string text = ((!string.IsNullOrWhiteSpace(status.SourceSkillId)) ? status.SourceSkillId : string.Empty);
			Vector2 eventCenter = UnitPosition(roster, statusOwner);
			TriggerExecutionContext triggerContext = new TriggerExecutionContext(statusOwner, null, eventCenter, status, 0f, 0f, DamageAttribute.Physical, text, unitState);
			ExecuteSourceOwnedTriggers(combatManager, roster, unitState, text, SkillTriggerEvent.OnStatusExpire, triggerContext);
			ExecutePassiveOwnerTriggers(combatManager, roster, SkillTriggerEvent.OnStatusExpire, triggerContext);
		}
	}

	/// 전달된 런타임 입력값을 사용해 ExpiredStatuses를 실행한다.
	public static void ExecuteExpiredStatuses(InGameCombatManager combatManager, UnitSpawnManager roster, UnitCombatState statusOwner, IReadOnlyList<StatusRuntimeInstance> removedStatuses)
	{
		for (int i = 0; i < removedStatuses.Count; i++)
		{
			StatusRuntimeInstance status = removedStatuses[i];
			ExecuteStatusExpire(combatManager, roster, statusOwner, status);
		}
		ExecuteShieldExpires(combatManager, roster, statusOwner, removedStatuses);
	}

	/// 전달된 런타임 입력값을 사용해 ShieldExpires를 실행한다.
	public static void ExecuteShieldExpires(InGameCombatManager combatManager, UnitSpawnManager roster, UnitCombatState shieldTarget, IReadOnlyList<StatusRuntimeInstance> removedStatuses)
	{
		for (int i = 0; i < removedStatuses.Count; i++)
		{
			StatusRuntimeInstance unitStatusRuntime = removedStatuses[i];
			if (unitStatusRuntime.IsShieldStatus)
			{
				ExecuteShieldExpire(combatManager, roster, shieldTarget, unitStatusRuntime);
			}
		}
	}

	/// 전달된 런타임 입력값을 사용해 OutgoingDamage를 실행한다.
	public static void ExecuteOutgoingDamage(InGameCombatManager combatManager, UnitSpawnManager roster, UnitCombatState source, string sourceSkillId, UnitCombatState eventTarget, DamageAttribute attribute, float eventAppliedDamage, bool eventWasExecute = false)
	{
		if (!(combatManager == null) && roster != null && source != null)
		{
			Vector2 eventCenter = ((eventTarget != null) ? UnitPosition(roster, eventTarget) : UnitPosition(roster, source));
			TriggerExecutionContext triggerContext = new TriggerExecutionContext(eventTarget, null, eventCenter, null, 0f, eventAppliedDamage, attribute, sourceSkillId, source, eventWasExecute);
			ExecuteSourceOwnedTriggers(combatManager, roster, source, sourceSkillId, SkillTriggerEvent.OnOutgoingDamage, triggerContext);
			ExecutePassiveOwnerTriggers(combatManager, roster, SkillTriggerEvent.OnOutgoingDamage, triggerContext);
		}
	}

	/// 전달된 런타임 입력값을 사용해 SkillCast를 실행한다.
	public static void ExecuteSkillCast(InGameCombatManager combatManager, UnitSpawnManager roster, UnitCombatState source, string sourceSkillId, Vector2 eventCenter, string eventTriggerSourceSkillId = null)
	{
		if (!(combatManager == null) && roster != null && source != null)
		{
			TriggerExecutionContext triggerContext = new TriggerExecutionContext(source, source, eventCenter, null, 0f, 0f, DamageAttribute.Physical, sourceSkillId, source, eventWasExecute: false, eventTriggerSourceSkillId);
			ExecuteSourceOwnedTriggers(combatManager, roster, source, sourceSkillId, SkillTriggerEvent.OnSkillCast, triggerContext);
			ExecutePassiveOwnerTriggers(combatManager, roster, SkillTriggerEvent.OnSkillCast, triggerContext);
		}
	}

	/// 전달된 런타임 입력값을 사용해 Kill를 실행한다.
	public static void ExecuteKill(InGameCombatManager combatManager, UnitSpawnManager roster, UnitCombatState source, string sourceSkillId, UnitCombatState eventTarget, DamageAttribute attribute, float eventAppliedDamage, bool eventWasExecute = false)
	{
		if (!(combatManager == null) && roster != null && source != null)
		{
			Vector2 eventCenter = ((eventTarget != null) ? UnitPosition(roster, eventTarget) : UnitPosition(roster, source));
			TriggerExecutionContext triggerContext = new TriggerExecutionContext(eventTarget, source, eventCenter, null, 0f, eventAppliedDamage, attribute, sourceSkillId, source, eventWasExecute);
			ExecuteSourceOwnedTriggers(combatManager, roster, source, sourceSkillId, SkillTriggerEvent.OnKill, triggerContext);
			ExecutePassiveOwnerTriggers(combatManager, roster, SkillTriggerEvent.OnKill, triggerContext);
		}
	}

	/// 전달된 런타임 입력값을 사용해 SourceOwnedTriggers를 실행한다.
	private static void ExecuteSourceOwnedTriggers(InGameCombatManager combatManager, UnitSpawnManager roster, UnitCombatState source, string sourceSkillId, SkillTriggerEvent triggerEvent, TriggerExecutionContext triggerContext)
	{
		if (combatManager == null || roster == null || source == null || string.IsNullOrWhiteSpace(sourceSkillId))
		{
			return;
		}
		string id = ((source.Identity != null) ? source.Identity.DefinitionId : string.Empty);
		MonsterDefinition monsterDefinition = GameDataLoader.CurrentCatalog.GetMonster(id);
		SkillReaction[] array = SourceOwnedTriggers(source, sourceSkillId, roster);
		if (array == null || array.Length == 0)
		{
			return;
		}
		foreach (SkillReaction trigger in array)
		{
			if (ShouldRunSourceOwnedTrigger(trigger, source, sourceSkillId, triggerEvent, triggerContext))
			{
				ExecuteTrigger(combatManager, roster, roster.Find(source), source, trigger, triggerContext);
			}
		}
	}

	/// 전달된 런타임 입력값을 사용해 SourceOwnedTriggers 결과값을 생성해 반환한다.
	private static SkillReaction[] SourceOwnedTriggers(
		UnitCombatState source,
		string sourceSkillId,
		UnitSpawnManager roster)
	{
		SkillUseState sourceSkill = null;
		if (source != null && source.Skills != null)
		{
			sourceSkill = source.SkillState.FindBySkillId(sourceSkillId);
		}

		if (sourceSkill == null || sourceSkill.Data == null)
		{
			return Array.Empty<SkillReaction>();
		}

		return CollectTriggers(
			source.SkillState.CreateExecutionData(
				source,
				sourceSkill,
				roster).Reactions);
	}

	/// 전달된 런타임 입력값을 사용해 PassiveOwnerTriggers를 실행한다.
	private static void ExecutePassiveOwnerTriggers(InGameCombatManager combatManager, UnitSpawnManager roster, SkillTriggerEvent triggerEvent, TriggerExecutionContext triggerContext)
	{
		if (combatManager == null || roster == null)
		{
			return;
		}
		IReadOnlyList<CombatUnitEntry> entries = roster.Entries;
		for (int i = 0; i < entries.Count; i++)
		{
			CombatUnitEntry unitEntry = entries[i];
			UnitCombatState unitState = unitEntry?.Model;
			if (unitEntry == null || unitState == null || unitState.Skills == null || unitState.Skills.LearnedPassiveSkillIds.Count == 0)
			{
				continue;
			}
			IReadOnlyList<SkillUseState> passives = unitState.SkillState.PassiveSkills;
			for (int passiveIndex = 0; passiveIndex < passives.Count; passiveIndex++)
			{
				var passiveRuntime = passives[passiveIndex];
				var triggers = unitState.SkillState.CreateExecutionData(
					unitState,
					passiveRuntime,
					roster).Reactions;
				if (triggers == null)
				{
					continue;
				}
				for (int triggerIndex = 0; triggerIndex < triggers.Count; triggerIndex++)
				{
					SkillReaction trigger = triggers[triggerIndex];
					if (ShouldRunPassiveOwnerTrigger(trigger, unitState, triggerEvent, triggerContext) && PassesCountGate(combatManager, unitState, trigger) && PassesProcGate(combatManager, unitState, trigger))
					{
						ExecuteTrigger(combatManager, roster, unitEntry, unitState, trigger, triggerContext);
					}
				}
			}
		}
	}

	/// 전달된 런타임 입력값을 사용해 RunSourceOwnedTrigger 실행 필요 여부를 반환한다.
	private static bool ShouldRunSourceOwnedTrigger(SkillReaction trigger, UnitCombatState source, string sourceSkillId, SkillTriggerEvent triggerEvent, TriggerExecutionContext triggerContext)
	{
		if (trigger != null && trigger.Event == triggerEvent && string.Equals(trigger.SourceSkillId, sourceSkillId, StringComparison.OrdinalIgnoreCase) && MatchesEventSkillId(trigger.EventSkillIds, triggerContext.EventSourceSkillId) && StatusConditionRules.MatchesSkillRuntimeKinds(trigger.EventSkillRuntimeKindValues, triggerContext.EventSourceSkillId) && (!trigger.RequireEventExecute || triggerContext.EventWasExecute) && HasAllChoices(source, trigger.RequiredActiveChoiceIds) && !HasAnyChoice(source, trigger.ExcludedActiveChoiceIds))
		{
			return MeetsSourceStatusRequirement(source, trigger.RequiredSourceStatusKind, trigger.RequiredSourceStatusMinStacks);
		}
		return false;
	}

	/// 전달된 런타임 입력값을 사용해 RunPassiveOwnerTrigger 실행 필요 여부를 반환한다.
	private static bool ShouldRunPassiveOwnerTrigger(SkillReaction trigger, UnitCombatState owner, SkillTriggerEvent triggerEvent, TriggerExecutionContext triggerContext)
	{
		if (trigger == null || owner == null || owner.Skills == null || trigger.Event != triggerEvent || string.IsNullOrWhiteSpace(trigger.SourceSkillId) || !owner.Skills.HasPassiveSkill(trigger.SourceSkillId) || !MatchesEventSkillId(trigger.EventSkillIds, triggerContext.EventSourceSkillId) || !StatusConditionRules.MatchesSkillRuntimeKinds(trigger.EventSkillRuntimeKindValues, triggerContext.EventSourceSkillId) || (trigger.RequireEventExecute && !triggerContext.EventWasExecute) || !HasAllChoices(owner, trigger.RequiredActiveChoiceIds) || HasAnyChoice(owner, trigger.ExcludedActiveChoiceIds) || !MeetsSourceStatusRequirement(owner, trigger.RequiredSourceStatusKind, trigger.RequiredSourceStatusMinStacks))
		{
			return false;
		}
		if (!MatchesConditionStatus(trigger, triggerContext.Status))
		{
			return false;
		}
		if (!MatchesConditionStatusSourceSkill(trigger.ConditionStatusSourceSkillIds, triggerContext.EventTarget, triggerContext.EventTriggerSourceSkillId))
		{
			return false;
		}
		if (MatchesTriggerAttribute(trigger.TriggerAttributes, triggerContext.EventAttribute))
		{
			return MatchesEventSourceScope(trigger.EventSourceScope, owner, triggerContext.EventSource);
		}
		return false;
	}

	/// 전달된 런타임 입력값을 사용해 소유한 런타임 상태에 AllChoices가 있는지 반환한다.
	private static bool HasAllChoices(UnitCombatState source, string[] choiceIds)
	{
		if (choiceIds == null || choiceIds.Length == 0)
		{
			return true;
		}
		if (source == null || source.Skills == null)
		{
			return false;
		}
		for (int i = 0; i < choiceIds.Length; i++)
		{
			if (!source.Skills.HasChoice(choiceIds[i]))
			{
				return false;
			}
		}
		return true;
	}

	/// 전달된 런타임 입력값을 사용해 소유한 런타임 상태에 AnyChoice가 있는지 반환한다.
	private static bool HasAnyChoice(UnitCombatState source, string[] choiceIds)
	{
		if (choiceIds == null || choiceIds.Length == 0 || source == null || source.Skills == null)
		{
			return false;
		}
		for (int i = 0; i < choiceIds.Length; i++)
		{
			if (source.Skills.HasChoice(choiceIds[i]))
			{
				return true;
			}
		}
		return false;
	}

	/// 전달된 런타임 입력값을 사용해 MeetsSourceStatusRequirement 조건을 평가하고 결과를 반환한다.
	private static bool MeetsSourceStatusRequirement(UnitCombatState owner, StatusEffectKind statusKind, int minStacks)
	{
		if (statusKind == StatusEffectKind.None)
		{
			return true;
		}
		if (statusKind == StatusEffectKind.Shield)
		{
			if (owner != null && owner.Resources != null)
			{
				return owner.Resources.CurrentShield > 0f;
			}
			return false;
		}
		if (owner != null && owner.Statuses != null)
		{
			return owner.Statuses.GetStacks(statusKind) >= Mathf.Max(1, minStacks);
		}
		return false;
	}

	/// 전달된 런타임 입력값을 사용해 MatchesConditionStatus 조건을 평가하고 결과를 반환한다.
	private static bool MatchesConditionStatus(SkillReaction trigger, StatusRuntimeInstance status)
	{
		if (trigger != null)
		{
			return StatusConditionRules.MatchesConditionStatus(status, trigger.ConditionStatuses);
		}
		return true;
	}

	/// 전달된 런타임 입력값을 사용해 MatchesTriggerAttribute 조건을 평가하고 결과를 반환한다.
	private static bool MatchesTriggerAttribute(DamageAttribute[] attributes, DamageAttribute eventAttribute)
	{
		if (attributes == null || attributes.Length == 0)
		{
			return true;
		}
		for (int i = 0; i < attributes.Length; i++)
		{
			if (attributes[i] == eventAttribute)
			{
				return true;
			}
		}
		return false;
	}

	/// 전달된 런타임 입력값을 사용해 PassesProcGate 조건을 평가하고 결과를 반환한다.
	private static bool PassesProcGate(InGameCombatManager combatManager, UnitCombatState owner, SkillReaction trigger)
	{
		if (combatManager == null || owner == null || trigger == null)
		{
			return false;
		}
		float num = SkillExecutionState.PassiveChoices(owner, trigger.SourceSkillId).TriggerProcChanceBonus(trigger.ReactionId);
		float num2 = ((trigger.ProcChance > 0f) ? Mathf.Clamp01(trigger.ProcChance + num) : Mathf.Clamp01(1f + num));
		if (num2 <= 0f || UnityEngine.Random.value > num2)
		{
			return false;
		}
		return gateStates.GetOrCreateValue(combatManager).ConsumeCooldown(
			BuildPassiveTriggerCooldownKey(owner, trigger),
			trigger.InternalCooldownSeconds);
	}

	/// 전달된 런타임 입력값을 사용해 PassesCountGate 조건을 평가하고 결과를 반환한다.
	private static bool PassesCountGate(InGameCombatManager combatManager, UnitCombatState owner, SkillReaction trigger)
	{
		if (combatManager == null || owner == null || trigger == null)
		{
			return false;
		}
		return gateStates.GetOrCreateValue(combatManager).ConsumeCount(
			BuildPassiveTriggerCooldownKey(owner, trigger),
			trigger.EveryCount);
	}

	/// 전달된 런타임 입력값을 사용해 MatchesEventSourceScope 조건을 평가하고 결과를 반환한다.
	private static bool MatchesEventSourceScope(SkillTriggerEventSourceScope scope, UnitCombatState owner, UnitCombatState eventSource)
	{
		if (scope == SkillTriggerEventSourceScope.Any)
		{
			return true;
		}
		if (owner == null || eventSource == null)
		{
			return false;
		}
		if (scope == SkillTriggerEventSourceScope.Owner)
		{
			return IsSameUnit(owner, eventSource);
		}
		if (scope == SkillTriggerEventSourceScope.AllAllies)
		{
			if (owner.Identity != null && eventSource.Identity != null)
			{
				return owner.Identity.Side == eventSource.Identity.Side;
			}
			return false;
		}
		return false;
	}

	/// 전달된 런타임 입력값을 사용해 MatchesEventSkillId 조건을 평가하고 결과를 반환한다.
	private static bool MatchesEventSkillId(string[] skillIds, string eventSkillId)
	{
		if (skillIds == null || skillIds.Length == 0)
		{
			return true;
		}
		if (string.IsNullOrWhiteSpace(eventSkillId))
		{
			return false;
		}
		for (int i = 0; i < skillIds.Length; i++)
		{
			if (string.Equals(skillIds[i], eventSkillId, StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}
		}
		return false;
	}

	/// 전달된 런타임 입력값을 사용해 MatchesConditionStatusSourceSkill 조건을 평가하고 결과를 반환한다.
	private static bool MatchesConditionStatusSourceSkill(string[] sourceSkillIds, UnitCombatState target, string eventTriggerSourceSkillId = null)
	{
		if (sourceSkillIds == null || sourceSkillIds.Length == 0)
		{
			return true;
		}
		IReadOnlyList<StatusRuntimeInstance> readOnlyList = null;
		if (target != null && target.Statuses != null)
		{
			readOnlyList = target.Statuses.ActiveStatuses;
		}
		int num = 0;
		while (readOnlyList != null && num < readOnlyList.Count)
		{
			StatusRuntimeData runtimeStatusData = null;
			if (readOnlyList[num] != null)
			{
				runtimeStatusData = readOnlyList[num].SourceData;
			}
			string text = string.Empty;
			if (runtimeStatusData != null)
			{
				text = runtimeStatusData.SourceSkillId;
			}
			if (!string.IsNullOrWhiteSpace(text))
			{
				for (int i = 0; i < sourceSkillIds.Length; i++)
				{
					string text2 = sourceSkillIds[i];
					if (!string.IsNullOrWhiteSpace(text2) && string.Equals(text2, text, StringComparison.OrdinalIgnoreCase))
					{
						return true;
					}
				}
			}
			num++;
		}
		if (string.IsNullOrWhiteSpace(eventTriggerSourceSkillId))
		{
			return false;
		}
		for (int j = 0; j < sourceSkillIds.Length; j++)
		{
			string text3 = sourceSkillIds[j];
			if (!string.IsNullOrWhiteSpace(text3) && string.Equals(text3, eventTriggerSourceSkillId, StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}
		}
		return false;
	}

	/// 전달된 런타임 입력값을 사용해 SameUnit 조건 충족 여부를 반환한다.
	private static bool IsSameUnit(UnitCombatState left, UnitCombatState right)
	{
		if (left == right)
		{
			return true;
		}
		string text = ((left != null && left.Identity != null) ? left.Identity.UnitId : string.Empty);
		string b = ((right != null && right.Identity != null) ? right.Identity.UnitId : string.Empty);
		if (!string.IsNullOrWhiteSpace(text))
		{
			return string.Equals(text, b, StringComparison.OrdinalIgnoreCase);
		}
		return false;
	}

	/// 전달된 런타임 입력값을 사용해 PassiveTriggerCooldownKey를 구성한다.
	private static string BuildPassiveTriggerCooldownKey(UnitCombatState owner, SkillReaction trigger)
	{
		string obj = ((owner != null && owner.Identity != null && !string.IsNullOrWhiteSpace(owner.Identity.UnitId)) ? owner.Identity.UnitId : ((owner != null) ? owner.GetHashCode().ToString() : "unknown"));
		string text = ((trigger != null && !string.IsNullOrWhiteSpace(trigger.ReactionId)) ? trigger.ReactionId : ((trigger != null) ? trigger.SourceSkillId : "unknown"));
		return obj + ":" + text;
	}

	/// 전달된 런타임 입력값을 사용해 Trigger를 실행한다.
	private static void ExecuteTrigger(InGameCombatManager combatManager, UnitSpawnManager roster, CombatUnitEntry sourceEntry, UnitCombatState source, SkillReaction trigger, TriggerExecutionContext triggerContext)
	{
		if (trigger == null)
		{
			return;
		}
		int num = Mathf.Max(1, trigger.RepeatCount);
		for (int i = 0; i < num; i++)
		{
			float num2 = Mathf.Max(0f, trigger.DelaySeconds) + ((i > 0) ? (Mathf.Max(0f, trigger.RepeatIntervalSeconds) * (float)i) : 0f);
			if (num2 <= 0f)
			{
				ExecuteOnce(combatManager, roster, sourceEntry, source, trigger, triggerContext);
			}
			else
			{
				combatManager.StartCoroutine(ExecuteDelayed(combatManager, roster, sourceEntry, source, trigger, triggerContext, num2));
			}
		}
	}

	/// 전달된 런타임 입력값을 사용해 Delayed를 실행한다.
	private static IEnumerator ExecuteDelayed(InGameCombatManager combatManager, UnitSpawnManager roster, CombatUnitEntry sourceEntry, UnitCombatState source, SkillReaction trigger, TriggerExecutionContext triggerContext, float delaySeconds)
	{
		yield return new WaitForSeconds(Mathf.Max(0f, delaySeconds));
		ExecuteOnce(combatManager, roster, sourceEntry, source, trigger, triggerContext);
	}

	/// 전달된 런타임 입력값을 사용해 Once를 실행한다.
	private static void ExecuteOnce(InGameCombatManager combatManager, UnitSpawnManager roster, CombatUnitEntry sourceEntry, UnitCombatState source, SkillReaction trigger, TriggerExecutionContext triggerContext)
	{
		TryExecuteOutcome(combatManager, roster, sourceEntry, source, trigger, triggerContext);
	}

	/// 전달된 런타임 입력값을 사용해 ExecuteOutcome 작업을 시도하고 성공 여부를 반환한다.
	private static bool TryExecuteOutcome(
		InGameCombatManager combatManager,
		UnitSpawnManager roster,
		CombatUnitEntry sourceEntry,
		UnitCombatState source,
		SkillReaction trigger,
		TriggerExecutionContext triggerContext)
	{
		if (combatManager == null
			|| roster == null
			|| sourceEntry == null
			|| source == null
			|| trigger == null)
		{
			return false;
		}

		SkillUseState sourceRuntime = source.SkillState.FindBySkillId(trigger.SourceSkillId);
		var targetPoint = triggerContext.EventCenter;
		if (trigger.CenterMode == SkillTriggerCenterMode.Caster
			&& sourceEntry.Transform != null)
		{
			targetPoint = sourceEntry.Transform.position;
		}
		else if (trigger.CenterMode == SkillTriggerCenterMode.EventTarget
			&& triggerContext.EventTarget != null)
		{
			var targetEntry = roster.Find(triggerContext.EventTarget);
			if (targetEntry != null && targetEntry.Transform != null)
			{
				targetPoint = targetEntry.Transform.position;
			}
		}

		if (trigger.Effect != null)
		{
			return sourceRuntime != null
				&& combatManager.SkillExecution.TryExecuteReactionEffect(
					sourceEntry,
					sourceRuntime,
					roster,
					combatManager,
					trigger.Effect,
					triggerContext.EventTarget,
					targetPoint,
					triggerContext.RecastGeneration,
					trigger.SourceSkillId,
					trigger.LockToEventTarget,
					trigger.DamageMultiplier,
					trigger.DamageValueSource
						!= SkillTriggerDamageValueSource.Fixed,
					ResolveTriggeredRawDamage(trigger, triggerContext));
		}

		if (!string.IsNullOrWhiteSpace(trigger.TargetSkillId))
		{
			if (sourceRuntime == null)
			{
				return false;
			}

			var runtime = source.SkillState.FindBySkillId(trigger.TargetSkillId);
			if (runtime == null)
			{
				return false;
			}
			var snapshotRuntime = runtime;

			var hasRawDamageOverride =
				trigger.DamageValueSource != SkillTriggerDamageValueSource.Fixed;
			var beginCast = runtime.Data is BuffSkillDefinition triggeredBuff
				&& triggeredBuff.EffectKind == BuffEffectKind.Charge;
			return combatManager.SkillExecution.TryExecuteReaction(
				sourceEntry,
				runtime,
				snapshotRuntime,
				runtime.Data,
				roster,
				combatManager,
				triggerContext.EventTarget,
				targetPoint,
				true,
				hasRawDamageOverride,
				ResolveTriggeredRawDamage(trigger, triggerContext),
				triggerContext.RecastGeneration,
				trigger.DamageMultiplier,
				trigger.SourceSkillId,
				trigger.LockToEventTarget,
				trigger.PublishSkillLifecycleEvents,
				beginCast);
		}

		if (trigger.Command != null)
		{
			return ExecuteCommand(
				combatManager,
				roster,
				sourceEntry,
				source,
				sourceRuntime,
				trigger.Command,
				triggerContext);
		}

		return false;
	}

	/// 전달된 런타임 입력값을 사용해 Command를 실행한다.
	private static bool ExecuteCommand(
		InGameCombatManager combatManager,
		UnitSpawnManager roster,
		CombatUnitEntry sourceEntry,
		UnitCombatState source,
		SkillUseState sourceRuntime,
		SkillReactionCommand command,
		TriggerExecutionContext triggerContext)
	{
		if (command == null || sourceRuntime == null)
		{
			return false;
		}

		var context = new SkillExecutionContext(
			combatManager,
			roster,
			sourceEntry,
			sourceRuntime,
			triggerContext.EventTarget,
			recastGeneration: triggerContext.RecastGeneration,
			lockToEventTarget: command.LockToEventTarget,
			publishSkillLifecycleEvents: false);
		if (command.Kind == SkillReactionCommandKind.RecastZone)
		{
			var skill = sourceRuntime.Data as ZoneSkillDefinition;
			if (skill == null
				|| (!string.IsNullOrWhiteSpace(command.TargetId)
					&& !string.Equals(command.TargetId, skill.SkillId, StringComparison.OrdinalIgnoreCase))
				|| context.RecastGeneration >= Math.Max(1, command.MaxGeneration))
			{
				return false;
			}

			var inheritedSnapshot = source.SkillState.CreateExecutionData(
				source,
				sourceRuntime,
				roster);
			var snapshot = command.InheritSnapshot
				? inheritedSnapshot
				: new SkillExecutionData(skill);
			return combatManager.SkillExecution.TryExecuteRecast(
				context,
				snapshot,
				skill,
				command,
				triggerContext.EventCenter);
		}

		var targets = SkillTargeting.OrderedTargets(context, command.Targeting);
		var limit = command.Targeting != null
			&& command.Targeting.Shape == SkillTargetShape.Single
				? 1
				: command.MaxTargets > 0
					? command.MaxTargets
					: targets.Count;
		var changed = false;
		for (var i = 0; i < targets.Count && i < limit; i++)
		{
			var target = targets[i] != null ? targets[i].Model : null;
			if (target == null)
			{
				continue;
			}

			if (command.Kind == SkillReactionCommandKind.ExtendStatusDuration)
			{
				changed |= combatManager.ExtendStatusDuration(
					target,
					command.StatusKind,
					command.DurationSeconds);
				continue;
			}
			if (target.Skills == null)
			{
				continue;
			}

			var runtimes = CommandRuntimes(target, command.TargetId);
			for (var runtimeIndex = 0; runtimeIndex < runtimes.Count; runtimeIndex++)
			{
				var targetRuntime = runtimes[runtimeIndex];
				if (command.Kind == SkillReactionCommandKind.RefundCooldown)
				{
					changed |= targetRuntime.ReduceCooldownRemaining(
						targetRuntime.EffectiveCooldownDuration * command.Ratio);
				}
				else if (command.Kind == SkillReactionCommandKind.ReduceReload)
				{
					changed |= targetRuntime.ReduceReloadRemaining(
						targetRuntime.ReloadDuration * command.Ratio);
				}
			}
		}
		return changed;
	}

	/// 전달된 런타임 입력값을 사용해 CommandRuntimes 결과값을 생성해 반환한다.
	private static IReadOnlyList<SkillUseState> CommandRuntimes(
		UnitCombatState target,
		string skillId)
	{
		if (!string.IsNullOrWhiteSpace(skillId))
		{
			var runtime = target.SkillState.FindBySkillId(skillId);
			return runtime != null
				? new[] { runtime }
				: Array.Empty<SkillUseState>();
		}
		return target.SkillState.ActiveSkills;
	}

	/// 전달된 런타임 입력값을 사용해 TriggeredRawDamage를 결정한다.
	private static float ResolveTriggeredRawDamage(
		SkillReaction trigger,
		TriggerExecutionContext context)
	{
		var value = 0f;
		switch (trigger.DamageValueSource)
		{
			case SkillTriggerDamageValueSource.ShieldAppliedAmount:
				value = context.ShieldAppliedAmount;
				break;
			case SkillTriggerDamageValueSource.ShieldRemainingAmount:
				value = context.ShieldRemainingAmount;
				break;
			case SkillTriggerDamageValueSource.ShieldAbsorbedAmount:
				value = context.ShieldAbsorbedAmount;
				break;
			case SkillTriggerDamageValueSource.TrackedIncomingDamage:
				value = context.TrackedIncomingDamage(trigger.TrackedDamageAttribute);
				break;
			case SkillTriggerDamageValueSource.EventAppliedDamage:
				value = context.EventAppliedDamage;
				break;
		}
		return Mathf.Max(0f, value) * Mathf.Max(0f, trigger.DamageValueMultiplier);
	}

	/// 전달된 런타임 입력값을 사용해 SourceModel 결과값을 생성해 반환한다.
	private static UnitCombatState SourceModel(
		UnitSpawnManager roster,
		string sourceUnitId,
		string sourceDefinitionId)
	{
		if (roster == null)
		{
			return null;
		}

		IReadOnlyList<CombatUnitEntry> entries = roster.Entries;
		for (var i = 0; i < entries.Count; i++)
		{
			var model = entries[i] != null ? entries[i].Model : null;
			var identity = model != null ? model.Identity : null;
			if (identity != null
				&& !string.IsNullOrWhiteSpace(sourceUnitId)
				&& string.Equals(identity.UnitId, sourceUnitId, StringComparison.OrdinalIgnoreCase))
			{
				return model;
			}
		}

		for (var i = 0; i < entries.Count; i++)
		{
			var model = entries[i] != null ? entries[i].Model : null;
			var identity = model != null ? model.Identity : null;
			if (identity != null
				&& !string.IsNullOrWhiteSpace(sourceDefinitionId)
				&& string.Equals(identity.DefinitionId, sourceDefinitionId, StringComparison.OrdinalIgnoreCase))
			{
				return model;
			}
		}

		return null;
	}

	/// 전달된 런타임 입력값을 사용해 UnitPosition 결과값을 생성해 반환한다.
	private static Vector2 UnitPosition(UnitSpawnManager roster, UnitCombatState model)
	{
		var entry = roster != null ? roster.Find(model) : null;
		return entry != null && entry.Transform != null
			? (Vector2)entry.Transform.position
			: Vector2.zero;
	}
}

}
