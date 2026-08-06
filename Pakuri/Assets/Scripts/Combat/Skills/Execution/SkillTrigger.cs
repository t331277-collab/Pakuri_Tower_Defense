/*
 * 역할: 전투 사건이 스킬 반응으로 이어질지 판정한다.
 */

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Pakuri.Combat;
using Pakuri.Data;
using UnityEngine;

namespace Pakuri.InGame
{

/// 전투 사건의 조건을 판정하고 통과한 반응을 실행 흐름에 넘긴다.
internal static class SkillTrigger
{
	/// 전투마다 반응 횟수와 내부 대기 진행을 분리해 유지한다.
	private sealed class TriggerGateState
	{
		private readonly Dictionary<string, float> cooldowns =
			new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase);
		private readonly Dictionary<string, int> counts =
			new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

		/// 반응의 내부 대기를 소모하고 실행 가능 여부를 돌려준다.
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

		/// 반응 누적 횟수가 실행 시점에 도달했는지 확인한다.
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

	/// 전투 단위의 반응 게이트를 초기화한다.
	internal static void Reset(InGameCombatManager combatManager)
	{
		if (combatManager != null)
		{
			gateStates.Remove(combatManager);
		}
	}

	/// 정의된 반응 목록을 실행 가능한 배열로 정리한다.
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

	/// 사건 발생 순간의 피해와 대상, 상태, 발생원을 후속 판정에 고정한다.
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

		public string EventSourceSkillName { get; }

		public UnitCombatState EventSource { get; }

		public bool EventWasExecute { get; }

		public bool EventWasCritical { get; }

		public string EventTriggerSourceSkillName { get; }

		public int EventHitCount { get; }

		public int RecastGeneration { get; }

		/// 지연 실행 뒤에도 사건 당시의 판정 기준을 그대로 사용하게 한다.
		public TriggerExecutionContext(UnitCombatState eventTarget, UnitCombatState attacker, Vector2 eventCenter, StatusRuntimeInstance status, float shieldAbsorbedAmount, float eventAppliedDamage, DamageAttribute eventAttribute, string eventSourceSkillName, UnitCombatState eventSource = null, bool eventWasExecute = false, string eventTriggerSourceSkillName = null, int eventHitCount = 0, int recastGeneration = 0, bool eventWasCritical = false)
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
			EventSourceSkillName = eventSourceSkillName;
			EventSource = eventSource;
			EventWasExecute = eventWasExecute;
			EventWasCritical = eventWasCritical;
			EventTriggerSourceSkillName = eventTriggerSourceSkillName;
			EventHitCount = Mathf.Max(0, eventHitCount);
			RecastGeneration = Mathf.Max(0, recastGeneration);
		}

		/// 사건 속성에 해당하는 누적 피해를 읽는다.
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

	/// 스킬 생명주기 사건을 게이트 판정에 연결한다.
	internal static void PublishLifecycleEvent(
		SkillTriggerEvent triggerEvent,
		SkillExecutionContext actionContext)
	{
		if (actionContext == null
			|| actionContext.Source == null
			|| actionContext.IsTrigger
			|| !actionContext.PublishSkillLifecycleEvents)
		{
			return;
		}

		if (actionContext.CombatManager == null || actionContext.Roster == null)
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
			actionContext.SourceSkillName,
			actionContext.Source,
			eventHitCount: actionContext.HitCount,
			recastGeneration: actionContext.RecastGeneration);
		ExecuteSourceOwnedTriggers(
			actionContext.CombatManager,
			actionContext.Roster,
			actionContext.Source,
			actionContext.SourceSkillName,
			triggerEvent,
			triggerContext);
		ExecutePassiveOwnerTriggers(
			actionContext.CombatManager,
			actionContext.Roster,
			triggerEvent,
			triggerContext);
	}

	/// 투사체 적중 사건을 반응 판정에 전달한다.
	public static void ExecuteProjectileHit(InGameCombatManager combatManager, UnitSpawnManager roster, UnitCombatState source, string sourceSkillName, bool isMagazineLastProjectile, Vector2 eventCenter)
	{
		if (isMagazineLastProjectile)
		{
			ExecuteSourceOwnedTriggers(combatManager, roster, source, sourceSkillName, SkillTriggerEvent.OnMagazineLastProjectileHit, new TriggerExecutionContext(source, null, eventCenter, null, 0f, 0f, DamageAttribute.Physical, sourceSkillName, source));
		}
	}

	/// 전투 시작 사건을 반응 판정에 전달한다.
	public static void ExecuteCombatStart(InGameCombatManager combatManager, UnitSpawnManager roster, UnitCombatState source)
	{
		IReadOnlyList<SkillExecutionState> readOnlyList = null;
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
			SkillExecutionState skillRuntimeInstance = readOnlyList[i];
			string text = ((skillRuntimeInstance != null && skillRuntimeInstance.Data != null) ? skillRuntimeInstance.Data.SkillName : string.Empty);
			if (!string.IsNullOrWhiteSpace(text))
			{
				ExecuteSourceOwnedTriggers(combatManager, roster, source, text, SkillTriggerEvent.CombatStart, new TriggerExecutionContext(source, source, eventCenter, null, 0f, 0f, DamageAttribute.Physical, text, source));
			}
		}
		ExecuteArtifactOwnerTriggers(
			combatManager,
			roster,
			roster.Find(source),
			source,
			SkillTriggerEvent.CombatStart,
			new TriggerExecutionContext(
				source,
				source,
				eventCenter,
				null,
				0f,
				0f,
				DamageAttribute.Physical,
				string.Empty,
				source));
	}

	/// 보호막 종료 사건을 반응 판정에 전달한다.
	public static void ExecuteShieldExpire(InGameCombatManager combatManager, UnitSpawnManager roster, UnitCombatState shieldTarget, StatusRuntimeInstance shieldStatus)
	{
		if (shieldTarget != null && shieldStatus != null && shieldStatus.IsShieldStatus)
		{
			UnitCombatState unitState = SourceModel(roster, shieldStatus.SourceUnitName, shieldStatus.SourceDefinitionName);
			string text = ((!string.IsNullOrWhiteSpace(shieldStatus.SourceSkillName)) ? shieldStatus.SourceSkillName : string.Empty);
			Vector2 eventCenter = UnitPosition(roster, shieldTarget);
			TriggerExecutionContext triggerContext = new TriggerExecutionContext(shieldTarget, null, eventCenter, shieldStatus, 0f, 0f, DamageAttribute.Physical, text, unitState);
			ExecuteSourceOwnedTriggers(combatManager, roster, unitState, text, SkillTriggerEvent.OnShieldExpire, triggerContext);
			ExecutePassiveOwnerTriggers(combatManager, roster, SkillTriggerEvent.OnShieldExpire, triggerContext);
		}
	}

	/// 보호막 흡수 사건을 반응 판정에 전달한다.
	public static void ExecuteShieldAbsorb(InGameCombatManager combatManager, UnitSpawnManager roster, UnitCombatState shieldTarget, UnitCombatState attacker, StatusRuntimeInstance shieldStatus, float absorbedAmount)
	{
		if (shieldTarget != null && shieldStatus != null && shieldStatus.IsShieldStatus && !(absorbedAmount <= 0f))
		{
			UnitCombatState unitState = SourceModel(roster, shieldStatus.SourceUnitName, shieldStatus.SourceDefinitionName);
			string text = ((!string.IsNullOrWhiteSpace(shieldStatus.SourceSkillName)) ? shieldStatus.SourceSkillName : string.Empty);
			Vector2 eventCenter = ((attacker != null) ? UnitPosition(roster, attacker) : UnitPosition(roster, shieldTarget));
			TriggerExecutionContext triggerContext = new TriggerExecutionContext(attacker, attacker, eventCenter, shieldStatus, absorbedAmount, 0f, DamageAttribute.Physical, text, unitState);
			ExecuteSourceOwnedTriggers(combatManager, roster, unitState, text, SkillTriggerEvent.OnShieldAbsorb, triggerContext);
			ExecutePassiveOwnerTriggers(combatManager, roster, SkillTriggerEvent.OnShieldAbsorb, triggerContext);
		}
	}

	/// 여러 보호막 흡수 사건을 반응 판정에 전달한다.
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

	/// 상태 종료 사건을 반응 판정에 전달한다.
	public static void ExecuteStatusExpire(InGameCombatManager combatManager, UnitSpawnManager roster, UnitCombatState statusOwner, StatusRuntimeInstance status)
	{
		if (statusOwner != null && status != null)
		{
			UnitCombatState unitState = SourceModel(roster, status.SourceUnitName, status.SourceDefinitionName);
			string text = ((!string.IsNullOrWhiteSpace(status.SourceSkillName)) ? status.SourceSkillName : string.Empty);
			Vector2 eventCenter = UnitPosition(roster, statusOwner);
			TriggerExecutionContext triggerContext = new TriggerExecutionContext(statusOwner, null, eventCenter, status, 0f, 0f, DamageAttribute.Physical, text, unitState);
			ExecuteSourceOwnedTriggers(combatManager, roster, unitState, text, SkillTriggerEvent.OnStatusExpire, triggerContext);
			ExecutePassiveOwnerTriggers(combatManager, roster, SkillTriggerEvent.OnStatusExpire, triggerContext);
		}
	}

	/// 여러 상태 종료 사건을 반응 판정에 전달한다.
	public static void ExecuteExpiredStatuses(InGameCombatManager combatManager, UnitSpawnManager roster, UnitCombatState statusOwner, IReadOnlyList<StatusRuntimeInstance> removedStatuses)
	{
		for (int i = 0; i < removedStatuses.Count; i++)
		{
			StatusRuntimeInstance status = removedStatuses[i];
			ExecuteStatusExpire(combatManager, roster, statusOwner, status);
		}
		ExecuteShieldExpires(combatManager, roster, statusOwner, removedStatuses);
	}

	/// 여러 보호막 종료 사건을 반응 판정에 전달한다.
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

	/// 외부 피해 사건을 반응 판정에 전달한다.
	public static void ExecuteOutgoingDamage(InGameCombatManager combatManager, UnitSpawnManager roster, UnitCombatState source, string sourceSkillName, UnitCombatState eventTarget, DamageAttribute attribute, float eventAppliedDamage, bool eventWasExecute, float sourceBaseDamage, bool eventWasCritical = false)
	{
		if (!(combatManager == null) && roster != null && source != null && eventAppliedDamage > 0f)
		{
			Vector2 eventCenter = ((eventTarget != null) ? UnitPosition(roster, eventTarget) : UnitPosition(roster, source));
			TriggerExecutionContext triggerContext = new TriggerExecutionContext(eventTarget, null, eventCenter, null, 0f, eventAppliedDamage, attribute, sourceSkillName, source, eventWasExecute, eventWasCritical: eventWasCritical);
			ExecuteSourceOwnedTriggers(combatManager, roster, source, sourceSkillName, SkillTriggerEvent.OnOutgoingDamage, triggerContext);
			ExecutePassiveOwnerTriggers(combatManager, roster, SkillTriggerEvent.OnOutgoingDamage, triggerContext);
			ApplyOutgoingAdditionalDamageStatuses(combatManager, eventTarget, source, sourceSkillName, attribute, sourceBaseDamage);
		}
	}

	/// outgoing 피해 상태가 만든 추가 피해를 기존 피해 적용 경로에 전달한다.
	private static void ApplyOutgoingAdditionalDamageStatuses(
		InGameCombatManager combatManager,
		UnitCombatState target,
		UnitCombatState source,
		string sourceSkillName,
		DamageAttribute triggerAttribute,
		float sourceBaseDamage)
	{
		if (combatManager == null || target == null || source == null || target.Resources.CurrentHealth <= 0f)
		{
			return;
		}

		var specs = StatusCombatRules.OutgoingAdditionalDamageSpecs(source, triggerAttribute);
		for (var i = 0; i < specs.Count; i++)
		{
			if (target.Resources.CurrentHealth <= 0f)
			{
				break;
			}

			var spec = specs[i];
			if (spec.Multiplier <= 0f)
			{
				continue;
			}

			combatManager.ApplyDamage(
				target,
				sourceBaseDamage * spec.Multiplier,
				spec.DamageAttribute,
				source,
				criticalAllowed: true,
				sourceSkillName: sourceSkillName,
				suppressOutgoingDamageTriggers: true);
		}
	}

	/// 스킬 시전 사건을 반응 판정에 전달한다.
	public static void ExecuteSkillCast(InGameCombatManager combatManager, UnitSpawnManager roster, UnitCombatState source, string sourceSkillName, Vector2 eventCenter, string eventTriggerSourceSkillName = null)
	{
		if (!(combatManager == null) && roster != null && source != null)
		{
			TriggerExecutionContext triggerContext = new TriggerExecutionContext(source, source, eventCenter, null, 0f, 0f, DamageAttribute.Physical, sourceSkillName, source, eventWasExecute: false, eventTriggerSourceSkillName: eventTriggerSourceSkillName);
			ExecuteSourceOwnedTriggers(combatManager, roster, source, sourceSkillName, SkillTriggerEvent.OnSkillCast, triggerContext);
			ExecutePassiveOwnerTriggers(combatManager, roster, SkillTriggerEvent.OnSkillCast, triggerContext);
		}
	}

	/// 처치 사건을 반응 판정에 전달한다.
	public static void ExecuteKill(InGameCombatManager combatManager, UnitSpawnManager roster, UnitCombatState source, string sourceSkillName, UnitCombatState eventTarget, DamageAttribute attribute, float eventAppliedDamage, bool eventWasExecute = false)
	{
		if (!(combatManager == null) && roster != null && source != null)
		{
			Vector2 eventCenter = ((eventTarget != null) ? UnitPosition(roster, eventTarget) : UnitPosition(roster, source));
			TriggerExecutionContext triggerContext = new TriggerExecutionContext(eventTarget, source, eventCenter, null, 0f, eventAppliedDamage, attribute, sourceSkillName, source, eventWasExecute);
			ExecuteSourceOwnedTriggers(combatManager, roster, source, sourceSkillName, SkillTriggerEvent.OnKill, triggerContext);
			ExecutePassiveOwnerTriggers(combatManager, roster, SkillTriggerEvent.OnKill, triggerContext);
		}
	}

	/// 사건을 만든 스킬의 반응을 판정한다.
	private static void ExecuteSourceOwnedTriggers(InGameCombatManager combatManager, UnitSpawnManager roster, UnitCombatState source, string sourceSkillName, SkillTriggerEvent triggerEvent, TriggerExecutionContext triggerContext)
	{
		if (combatManager == null || roster == null || source == null || string.IsNullOrWhiteSpace(sourceSkillName))
		{
			return;
		}
		SkillReaction[] array = SourceOwnedTriggers(source, sourceSkillName, roster);
		if (array == null || array.Length == 0)
		{
			return;
		}
		foreach (SkillReaction trigger in array)
		{
			if (ShouldRunSourceOwnedTrigger(trigger, source, sourceSkillName, triggerEvent, triggerContext))
			{
                SkillExecution.ScheduleReaction(
					combatManager,
					roster,
					roster.Find(source),
					source,
					trigger,
					triggerContext,
					ResolveTriggeredDamage(trigger, triggerContext));
			}
		}
	}

	/// 사건을 만든 스킬에 연결된 반응을 모은다.
	private static SkillReaction[] SourceOwnedTriggers(
		UnitCombatState source,
		string sourceSkillName,
		UnitSpawnManager roster)
	{
		SkillExecutionState sourceSkill = null;
		if (source != null && source.Skills != null)
		{
			sourceSkill = source.SkillState.FindBySkillName(sourceSkillName);
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

	/// 소유자의 지속 반응을 판정한다.
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
			if (unitEntry == null || unitState == null || unitState.Skills == null)
			{
				continue;
			}
			IReadOnlyList<SkillExecutionState> passives = unitState.SkillState.PassiveSkills;
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
					if (ShouldRunPassiveOwnerTrigger(trigger, unitState, triggerEvent, triggerContext)
						&& PassesCountGate(combatManager, unitState, trigger)
						&& PassesProcGate(combatManager, unitState, trigger))
					{
                        SkillExecution.ScheduleReaction(
							combatManager,
							roster,
						unitEntry,
						unitState,
						trigger,
						triggerContext,
						ResolveTriggeredDamage(trigger, triggerContext));
					}
				}
			}
			ExecuteArtifactOwnerTriggers(
				combatManager,
				roster,
				unitEntry,
				unitState,
				triggerEvent,
				triggerContext);
		}
	}

	private static void ExecuteArtifactOwnerTriggers(
		InGameCombatManager combatManager,
		UnitSpawnManager roster,
		CombatUnitEntry ownerEntry,
		UnitCombatState owner,
		SkillTriggerEvent triggerEvent,
		TriggerExecutionContext triggerContext)
	{
		var effectNames = owner?.Artifacts?.ActiveArtifactEffectNames;
		if (combatManager == null || roster == null || ownerEntry == null || effectNames == null)
		{
			return;
		}

		for (var effectIndex = 0; effectIndex < effectNames.Count; effectIndex++)
		{
			if (GameDataLoader.CurrentCatalog.TryGetData(
					effectNames[effectIndex],
					out ArtifactEffectDefinition effect)
				&& effect != null
				&& effect.ApplicationMode == ArtifactEffectApplicationMode.PassiveTrigger)
			{
				ExecuteArtifactReactions(
					combatManager,
					roster,
					ownerEntry,
					owner,
					triggerEvent,
					triggerContext,
					effect.Reactions);

				continue;
			}

			if (GameDataLoader.CurrentCatalog.TryGetData(
					effectNames[effectIndex],
					out ArtifactSynergyEffectDefinition synergyEffect)
				&& synergyEffect != null
				&& synergyEffect.ApplicationMode == ArtifactEffectApplicationMode.PassiveTrigger)
			{
				ExecuteArtifactReactions(
					combatManager,
					roster,
					ownerEntry,
					owner,
					triggerEvent,
					triggerContext,
					synergyEffect.Reactions);
			}
		}
	}

	private static void ExecuteArtifactReactions(
		InGameCombatManager combatManager,
		UnitSpawnManager roster,
		CombatUnitEntry ownerEntry,
		UnitCombatState owner,
		SkillTriggerEvent triggerEvent,
		TriggerExecutionContext triggerContext,
		IReadOnlyList<SkillReaction> reactions)
	{
		for (var triggerIndex = 0; reactions != null && triggerIndex < reactions.Count; triggerIndex++)
		{
			var trigger = reactions[triggerIndex];
			if (ShouldRunArtifactOwnerTrigger(trigger, owner, triggerEvent, triggerContext)
				&& PassesCountGate(combatManager, owner, trigger)
				&& PassesProcGate(combatManager, owner, trigger))
			{
				SkillExecution.ScheduleReaction(
					combatManager,
					roster,
					ownerEntry,
					owner,
					trigger,
					triggerContext,
					ResolveTriggeredDamage(trigger, triggerContext));
			}
		}
	}

	/// 발생원 반응의 모든 게이트를 통과했는지 확인한다.
	private static bool ShouldRunSourceOwnedTrigger(SkillReaction trigger, UnitCombatState source, string sourceSkillName, SkillTriggerEvent triggerEvent, TriggerExecutionContext triggerContext)
	{
		if (trigger != null && trigger.Event == triggerEvent && string.Equals(trigger.SourceSkillName, sourceSkillName, StringComparison.OrdinalIgnoreCase) && MatchesEventSkillName(trigger.EventSkillNames, triggerContext.EventSourceSkillName) && StatusConditionRules.MatchesSkillRuntimeKinds(trigger.EventSkillRuntimeKindValues, triggerContext.EventSourceSkillName) && (!trigger.RequireEventExecute || triggerContext.EventWasExecute) && (!trigger.RequireEventCritical || triggerContext.EventWasCritical) && HasAllChoices(source, trigger.RequiredActiveChoiceNames) && !HasAnyChoice(source, trigger.ExcludedActiveChoiceNames))
		{
			return MeetsSourceStatusRequirement(source, trigger.RequiredSourceStatusKind, trigger.RequiredSourceStatusMinStacks);
		}
		return false;
	}

	/// 지속 반응의 모든 게이트를 통과했는지 확인한다.
	private static bool ShouldRunPassiveOwnerTrigger(SkillReaction trigger, UnitCombatState owner, SkillTriggerEvent triggerEvent, TriggerExecutionContext triggerContext)
	{
		if (trigger == null || owner == null || owner.Skills == null || trigger.Event != triggerEvent || string.IsNullOrWhiteSpace(trigger.SourceSkillName) || !owner.Skills.HasPassiveSkill(trigger.SourceSkillName) || !MatchesEventSkillName(trigger.EventSkillNames, triggerContext.EventSourceSkillName) || !StatusConditionRules.MatchesSkillRuntimeKinds(trigger.EventSkillRuntimeKindValues, triggerContext.EventSourceSkillName) || (trigger.RequireEventExecute && !triggerContext.EventWasExecute) || (trigger.RequireEventCritical && !triggerContext.EventWasCritical) || !HasAllChoices(owner, trigger.RequiredActiveChoiceNames) || HasAnyChoice(owner, trigger.ExcludedActiveChoiceNames) || !MeetsSourceStatusRequirement(owner, trigger.RequiredSourceStatusKind, trigger.RequiredSourceStatusMinStacks))
		{
			return false;
		}
		if (!MatchesConditionStatus(trigger, triggerContext.Status))
		{
			return false;
		}
		if (!MatchesConditionStatusSourceSkill(trigger.ConditionStatusSourceSkillNames, triggerContext.EventTarget, triggerContext.EventTriggerSourceSkillName))
		{
			return false;
		}
		if (MatchesTriggerAttribute(trigger.TriggerAttributes, triggerContext.EventAttribute))
		{
			return MatchesEventSourceScope(trigger.EventSourceScope, owner, triggerContext.EventSource);
		}
		return false;
	}

	private static bool ShouldRunArtifactOwnerTrigger(
		SkillReaction trigger,
		UnitCombatState owner,
		SkillTriggerEvent triggerEvent,
		TriggerExecutionContext triggerContext)
	{
		return trigger != null
			&& owner != null
			&& trigger.Event == triggerEvent
			&& MatchesEventSkillName(trigger.EventSkillNames, triggerContext.EventSourceSkillName)
			&& StatusConditionRules.MatchesSkillRuntimeKinds(
				trigger.EventSkillRuntimeKindValues,
				triggerContext.EventSourceSkillName)
			&& (!trigger.RequireEventExecute || triggerContext.EventWasExecute)
			&& (!trigger.RequireEventCritical || triggerContext.EventWasCritical)
			&& MeetsSourceStatusRequirement(
				owner,
				trigger.RequiredSourceStatusKind,
				trigger.RequiredSourceStatusMinStacks)
			&& MatchesConditionStatus(trigger, triggerContext.Status)
			&& MatchesConditionStatusSourceSkill(
				trigger.ConditionStatusSourceSkillNames,
				triggerContext.EventTarget,
				triggerContext.EventTriggerSourceSkillName)
			&& MatchesTriggerAttribute(trigger.TriggerAttributes, triggerContext.EventAttribute)
			&& MatchesEventSourceScope(trigger.EventSourceScope, owner, triggerContext.EventSource);
	}

	/// 소유자가 요구 선택을 모두 갖췄는지 확인한다.
	private static bool HasAllChoices(UnitCombatState source, string[] choiceNames)
	{
		if (choiceNames == null || choiceNames.Length == 0)
		{
			return true;
		}
		if (source == null || source.Skills == null)
		{
			return false;
		}
		for (int i = 0; i < choiceNames.Length; i++)
		{
			if (!source.Skills.HasChoice(choiceNames[i]))
			{
				return false;
			}
		}
		return true;
	}

	/// 소유자가 요구 선택 중 하나를 갖췄는지 확인한다.
	private static bool HasAnyChoice(UnitCombatState source, string[] choiceNames)
	{
		if (choiceNames == null || choiceNames.Length == 0 || source == null || source.Skills == null)
		{
			return false;
		}
		for (int i = 0; i < choiceNames.Length; i++)
		{
			if (source.Skills.HasChoice(choiceNames[i]))
			{
				return true;
			}
		}
		return false;
	}

	/// 소유자의 상태 조건을 충족하는지 확인한다.
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

	/// 사건 상태가 반응 조건과 맞는지 확인한다.
	private static bool MatchesConditionStatus(SkillReaction trigger, StatusRuntimeInstance status)
	{
		if (trigger != null)
		{
			return StatusConditionRules.MatchesConditionStatus(status, trigger.ConditionStatuses);
		}
		return true;
	}

	/// 사건 속성이 반응 조건과 맞는지 확인한다.
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

	/// 확률 게이트를 통과하는지 확인한다.
	private static bool PassesProcGate(InGameCombatManager combatManager, UnitCombatState owner, SkillReaction trigger)
	{
		if (combatManager == null || owner == null || trigger == null)
		{
			return false;
		}
		float num = UnitSkills.PassiveChoices(owner, trigger.SourceSkillName).TriggerProcChanceBonus(trigger.ReactionName);
		float num2 = ((trigger.ProcChance > 0f) ? Mathf.Clamp01(trigger.ProcChance + num) : Mathf.Clamp01(1f + num));
		if (num2 <= 0f || UnityEngine.Random.value > num2)
		{
			return false;
		}
		return gateStates.GetOrCreateValue(combatManager).ConsumeCooldown(
			BuildPassiveTriggerCooldownKey(owner, trigger),
			trigger.InternalCooldownSeconds);
	}

	/// 누적 횟수 게이트를 통과하는지 확인한다.
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

	/// 지연 전에 사건이 제공한 수치를 반응 입력으로 고정한다.
	private static float ResolveTriggeredDamage(
		SkillReaction trigger,
		TriggerExecutionContext context)
	{
		if (trigger == null)
		{
			return 0f;
		}

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

	/// 사건 발생원 범위가 반응 조건과 맞는지 확인한다.
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

	/// 사건 스킬 식별자가 반응 조건과 맞는지 확인한다.
	private static bool MatchesEventSkillName(string[] skillNames, string eventSkillName)
	{
		if (skillNames == null || skillNames.Length == 0)
		{
			return true;
		}
		if (string.IsNullOrWhiteSpace(eventSkillName))
		{
			return false;
		}
		for (int i = 0; i < skillNames.Length; i++)
		{
			if (string.Equals(skillNames[i], eventSkillName, StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}
		}
		return false;
	}

	/// 상태 발생 스킬이 반응 조건과 맞는지 확인한다.
	private static bool MatchesConditionStatusSourceSkill(string[] sourceSkillNames, UnitCombatState target, string eventTriggerSourceSkillName = null)
	{
		if (sourceSkillNames == null || sourceSkillNames.Length == 0)
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
				text = runtimeStatusData.SourceSkillName;
			}
			if (!string.IsNullOrWhiteSpace(text))
			{
				for (int i = 0; i < sourceSkillNames.Length; i++)
				{
					string text2 = sourceSkillNames[i];
					if (!string.IsNullOrWhiteSpace(text2) && string.Equals(text2, text, StringComparison.OrdinalIgnoreCase))
					{
						return true;
					}
				}
			}
			num++;
		}
		if (string.IsNullOrWhiteSpace(eventTriggerSourceSkillName))
		{
			return false;
		}
		for (int j = 0; j < sourceSkillNames.Length; j++)
		{
			string text3 = sourceSkillNames[j];
			if (!string.IsNullOrWhiteSpace(text3) && string.Equals(text3, eventTriggerSourceSkillName, StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}
		}
		return false;
	}

	/// 두 모델이 같은 전투 유닛인지 확인한다.
	private static bool IsSameUnit(UnitCombatState left, UnitCombatState right)
	{
		if (left == right)
		{
			return true;
		}
		string text = ((left != null && left.Identity != null) ? left.Identity.UnitName : string.Empty);
		string b = ((right != null && right.Identity != null) ? right.Identity.UnitName : string.Empty);
		if (!string.IsNullOrWhiteSpace(text))
		{
			return string.Equals(text, b, StringComparison.OrdinalIgnoreCase);
		}
		return false;
	}

	/// 지속 반응의 내부 대기 식별자를 만든다.
	private static string BuildPassiveTriggerCooldownKey(UnitCombatState owner, SkillReaction trigger)
	{
		string obj = ((owner != null && owner.Identity != null && !string.IsNullOrWhiteSpace(owner.Identity.UnitName)) ? owner.Identity.UnitName : ((owner != null) ? owner.GetHashCode().ToString() : "unknown"));
		string text = ((trigger != null && !string.IsNullOrWhiteSpace(trigger.ReactionName)) ? trigger.ReactionName : ((trigger != null) ? trigger.SourceSkillName : "unknown"));
		return obj + ":" + text;
	}

	/// 상태가 기억한 발생원을 현재 전투 명단에서 복원한다.
	private static UnitCombatState SourceModel(
		UnitSpawnManager roster,
		string sourceUnitName,
		string sourceDefinitionName)
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
				&& !string.IsNullOrWhiteSpace(sourceUnitName)
				&& string.Equals(identity.UnitName, sourceUnitName, StringComparison.OrdinalIgnoreCase))
			{
				return model;
			}
		}

		for (var i = 0; i < entries.Count; i++)
		{
			var model = entries[i] != null ? entries[i].Model : null;
			var identity = model != null ? model.Identity : null;
			if (identity != null
				&& !string.IsNullOrWhiteSpace(sourceDefinitionName)
				&& string.Equals(identity.DefinitionName, sourceDefinitionName, StringComparison.OrdinalIgnoreCase))
			{
				return model;
			}
		}

		return null;
	}

	/// 사건 중심으로 사용할 유닛의 현재 위치를 정한다.
	private static Vector2 UnitPosition(UnitSpawnManager roster, UnitCombatState model)
	{
		var entry = roster != null ? roster.Find(model) : null;
		return entry != null && entry.Transform != null
			? (Vector2)entry.Transform.position
			: Vector2.zero;
	}
}

}
