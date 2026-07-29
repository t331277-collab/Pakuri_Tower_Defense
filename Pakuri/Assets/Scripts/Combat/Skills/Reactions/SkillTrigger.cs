using System;
using System.Collections;
using System.Collections.Generic;
using Pakuri.Combat;
using Pakuri.Data;
using UnityEngine;

/*
 * 전투 사건을 스킬 트리거 조건과 연결하고 해당 효과 실행을 조율한다.
 */
namespace Pakuri.InGame
{

internal static class SkillTrigger
{
	// 전투 사건을 조건·확률·횟수로 판정하고 후속 행동을 실행하는 부분을 구현.
	/*
	 * null 항목을 제외한 기본 Trigger 실행 목록을 만든다.
	 */
	private static SkillTriggerDefinition[] CollectTriggers(
		SkillTriggerDefinition[] baseTriggers /* 유닛 기본 Trigger 목록 */)
	{
		var triggers = new List<SkillTriggerDefinition>();
		if (baseTriggers != null)
		{
			for (var i = 0; i < baseTriggers.Length; i++)
			{
				if (baseTriggers[i] != null)
				{
					triggers.Add(baseTriggers[i]);
				}
			}
		}

		return triggers.ToArray();
	}

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

		/*
		 * TriggerExecutionContext에 필요한 값을 초기화한다.
		 */
		public TriggerExecutionContext(UnitCombatState eventTarget /* 사건 대상 */, UnitCombatState attacker /* 공격자 */, Vector2 eventCenter /* 사건 중심 위치 */, StatusRuntimeInstance status /* 실행 중인 상태 효과 */, float shieldAbsorbedAmount /* 보호막 흡수된 수치 */, float eventAppliedDamage /* 사건 적용된 피해 */, DamageAttribute eventAttribute /* 사건 속성 */, string eventSourceSkillId /* 사건 발생 원본 스킬 식별자 */, UnitCombatState eventSource = null /* 사건 발생 원본 */, bool eventWasExecute = false /* 사건 발생 처형 여부 */, string eventTriggerSourceSkillId = null /* 사건 트리거 발생 원본 스킬 식별자 */, int eventHitCount = 0 /* 사건 적중 횟수 */, int recastGeneration = 0 /* 재시전 실행 세대 */)
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

	/*
	 * family 실행기가 발행한 lifecycle 사건을 Trigger 판정 경로로 전달한다.
	 */
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

	/*
	 * ExecuteProjectileHit 실행을 처리한다.
	 */
	public static void ExecuteProjectileHit(InGameCombatManager combatManager /* 전투 진행 관리자 */, CombatUnitRegistry roster /* 전투에 등록된 유닛 목록 */, UnitCombatState source /* 효과를 발생시킨 유닛 */, string sourceSkillId /* 효과를 발생시킨 스킬 식별자 */, bool isMagazineLastProjectile /* 여부 탄창 마지막 투사체 여부 */, Vector2 eventCenter /* 사건 중심 위치 */)
	{
		if (isMagazineLastProjectile)
		{
			ExecuteSourceOwnedTriggers(combatManager, roster, source, sourceSkillId, SkillTriggerEvent.OnMagazineLastProjectileHit, new TriggerExecutionContext(source, null, eventCenter, null, 0f, 0f, DamageAttribute.Physical, sourceSkillId, source));
		}
	}

	/*
	 * ExecuteCombatStart 실행을 처리한다.
	 */
	public static void ExecuteCombatStart(InGameCombatManager combatManager /* 전투 진행 관리자 */, CombatUnitRegistry roster /* 전투에 등록된 유닛 목록 */, UnitCombatState source /* 효과를 발생시킨 유닛 */)
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

	/*
	 * ExecuteShieldExpire 실행을 처리한다.
	 */
	public static void ExecuteShieldExpire(InGameCombatManager combatManager /* 전투 진행 관리자 */, CombatUnitRegistry roster /* 전투에 등록된 유닛 목록 */, UnitCombatState shieldTarget /* 보호막 대상 */, StatusRuntimeInstance shieldStatus /* 보호막 상태 효과 */)
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

	/*
	 * ExecuteShieldAbsorb 실행을 처리한다.
	 */
	public static void ExecuteShieldAbsorb(InGameCombatManager combatManager /* 전투 진행 관리자 */, CombatUnitRegistry roster /* 전투에 등록된 유닛 목록 */, UnitCombatState shieldTarget /* 보호막 대상 */, UnitCombatState attacker /* 공격자 */, StatusRuntimeInstance shieldStatus /* 보호막 상태 효과 */, float absorbedAmount /* 흡수된 수치 */)
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

	/*
	 * ExecuteShieldAbsorbs 실행을 처리한다.
	 */
	public static void ExecuteShieldAbsorbs(InGameCombatManager combatManager /* 전투 진행 관리자 */, CombatUnitRegistry roster /* 전투에 등록된 유닛 목록 */, UnitCombatState shieldTarget /* 보호막 대상 */, UnitCombatState attacker /* 공격자 */, IReadOnlyList<ShieldAbsorptionRecord> absorbedShields /* 흡수된 보호막 목록 */)
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

	/*
	 * ExecuteStatusExpire 실행을 처리한다.
	 */
	public static void ExecuteStatusExpire(InGameCombatManager combatManager /* 전투 진행 관리자 */, CombatUnitRegistry roster /* 전투에 등록된 유닛 목록 */, UnitCombatState statusOwner /* 상태 효과 소유자 */, StatusRuntimeInstance status /* 실행 중인 상태 효과 */)
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

	/*
	 * ExecuteExpiredStatuses 실행을 처리한다.
	 */
	public static void ExecuteExpiredStatuses(InGameCombatManager combatManager /* 전투 진행 관리자 */, CombatUnitRegistry roster /* 전투에 등록된 유닛 목록 */, UnitCombatState statusOwner /* 상태 효과 소유자 */, IReadOnlyList<StatusRuntimeInstance> removedStatuses /* 제거된 상태 효과 목록 */)
	{
		for (int i = 0; i < removedStatuses.Count; i++)
		{
			StatusRuntimeInstance status = removedStatuses[i];
			ExecuteStatusExpire(combatManager, roster, statusOwner, status);
		}
		ExecuteShieldExpires(combatManager, roster, statusOwner, removedStatuses);
	}

	/*
	 * ExecuteShieldExpires 실행을 처리한다.
	 */
	public static void ExecuteShieldExpires(InGameCombatManager combatManager /* 전투 진행 관리자 */, CombatUnitRegistry roster /* 전투에 등록된 유닛 목록 */, UnitCombatState shieldTarget /* 보호막 대상 */, IReadOnlyList<StatusRuntimeInstance> removedStatuses /* 제거된 상태 효과 목록 */)
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

	/*
	 * ExecuteOutgoingDamage 실행을 처리한다.
	 */
	public static void ExecuteOutgoingDamage(InGameCombatManager combatManager /* 전투 진행 관리자 */, CombatUnitRegistry roster /* 전투에 등록된 유닛 목록 */, UnitCombatState source /* 효과를 발생시킨 유닛 */, string sourceSkillId /* 효과를 발생시킨 스킬 식별자 */, UnitCombatState eventTarget /* 사건 대상 */, DamageAttribute attribute /* 피해 속성 */, float eventAppliedDamage /* 사건 적용된 피해 */, bool eventWasExecute = false /* 사건 발생 처형 여부 */)
	{
		if (!(combatManager == null) && roster != null && source != null)
		{
			Vector2 eventCenter = ((eventTarget != null) ? UnitPosition(roster, eventTarget) : UnitPosition(roster, source));
			TriggerExecutionContext triggerContext = new TriggerExecutionContext(eventTarget, null, eventCenter, null, 0f, eventAppliedDamage, attribute, sourceSkillId, source, eventWasExecute);
			ExecuteSourceOwnedTriggers(combatManager, roster, source, sourceSkillId, SkillTriggerEvent.OnOutgoingDamage, triggerContext);
			ExecutePassiveOwnerTriggers(combatManager, roster, SkillTriggerEvent.OnOutgoingDamage, triggerContext);
		}
	}

	/*
	 * ExecuteSkillCast 실행을 처리한다.
	 */
	public static void ExecuteSkillCast(InGameCombatManager combatManager /* 전투 진행 관리자 */, CombatUnitRegistry roster /* 전투에 등록된 유닛 목록 */, UnitCombatState source /* 효과를 발생시킨 유닛 */, string sourceSkillId /* 효과를 발생시킨 스킬 식별자 */, Vector2 eventCenter /* 사건 중심 위치 */, string eventTriggerSourceSkillId = null /* 사건 트리거 발생 원본 스킬 식별자 */)
	{
		if (!(combatManager == null) && roster != null && source != null)
		{
			TriggerExecutionContext triggerContext = new TriggerExecutionContext(source, source, eventCenter, null, 0f, 0f, DamageAttribute.Physical, sourceSkillId, source, eventWasExecute: false, eventTriggerSourceSkillId);
			ExecuteSourceOwnedTriggers(combatManager, roster, source, sourceSkillId, SkillTriggerEvent.OnSkillCast, triggerContext);
			ExecutePassiveOwnerTriggers(combatManager, roster, SkillTriggerEvent.OnSkillCast, triggerContext);
		}
	}

	/*
	 * ExecuteKill 실행을 처리한다.
	 */
	public static void ExecuteKill(InGameCombatManager combatManager /* 전투 진행 관리자 */, CombatUnitRegistry roster /* 전투에 등록된 유닛 목록 */, UnitCombatState source /* 효과를 발생시킨 유닛 */, string sourceSkillId /* 효과를 발생시킨 스킬 식별자 */, UnitCombatState eventTarget /* 사건 대상 */, DamageAttribute attribute /* 피해 속성 */, float eventAppliedDamage /* 사건 적용된 피해 */, bool eventWasExecute = false /* 사건 발생 처형 여부 */)
	{
		if (!(combatManager == null) && roster != null && source != null)
		{
			Vector2 eventCenter = ((eventTarget != null) ? UnitPosition(roster, eventTarget) : UnitPosition(roster, source));
			TriggerExecutionContext triggerContext = new TriggerExecutionContext(eventTarget, source, eventCenter, null, 0f, eventAppliedDamage, attribute, sourceSkillId, source, eventWasExecute);
			ExecuteSourceOwnedTriggers(combatManager, roster, source, sourceSkillId, SkillTriggerEvent.OnKill, triggerContext);
			ExecutePassiveOwnerTriggers(combatManager, roster, SkillTriggerEvent.OnKill, triggerContext);
		}
	}

	/*
	 * ExecuteSourceOwnedTriggers 실행을 처리한다.
	 */
	private static void ExecuteSourceOwnedTriggers(InGameCombatManager combatManager /* 전투 진행 관리자 */, CombatUnitRegistry roster /* 전투에 등록된 유닛 목록 */, UnitCombatState source /* 효과를 발생시킨 유닛 */, string sourceSkillId /* 효과를 발생시킨 스킬 식별자 */, SkillTriggerEvent triggerEvent /* 트리거를 발생시킨 사건 종류 */, TriggerExecutionContext triggerContext /* 트리거 실행에 필요한 정보 */)
	{
		if (combatManager == null || roster == null || source == null || string.IsNullOrWhiteSpace(sourceSkillId))
		{
			return;
		}
		string id = ((source.Identity != null) ? source.Identity.DefinitionId : string.Empty);
		MonsterDefinition monsterDefinition = GameDataLoader.CurrentCatalog.GetMonster(id);
		SkillTriggerDefinition[] array = SourceOwnedTriggers(source, sourceSkillId, (monsterDefinition != null) ? monsterDefinition.SkillTriggers : null);
		if (array == null || array.Length == 0)
		{
			return;
		}
		foreach (SkillTriggerDefinition trigger in array)
		{
			if (ShouldRunSourceOwnedTrigger(trigger, source, sourceSkillId, triggerEvent, triggerContext))
			{
				ExecuteTrigger(combatManager, roster, roster.Find(source), source, trigger, triggerContext);
			}
		}
	}

	/*
	 * SourceOwnedTriggers 결과를 계산해 반환한다.
	 */
	private static SkillTriggerDefinition[] SourceOwnedTriggers(UnitCombatState source /* 효과를 발생시킨 유닛 */, string sourceSkillId /* 효과를 발생시킨 스킬 식별자 */, SkillTriggerDefinition[] baseTriggers /* 유닛 기본 Trigger 목록 */)
	{
		SkillUseState sourceSkill = null;
		if (source != null && source.Skills != null)
		{
			sourceSkill = source.SkillState.FindBySkillId(sourceSkillId);
		}

		if (sourceSkill == null || sourceSkill.Data == null)
		{
			return baseTriggers;
		}

		return CollectTriggers(sourceSkill.Data.SkillTriggers);
	}

	/*
	 * ExecutePassiveOwnerTriggers 실행을 처리한다.
	 */
	private static void ExecutePassiveOwnerTriggers(InGameCombatManager combatManager /* 전투 진행 관리자 */, CombatUnitRegistry roster /* 전투에 등록된 유닛 목록 */, SkillTriggerEvent triggerEvent /* 트리거를 발생시킨 사건 종류 */, TriggerExecutionContext triggerContext /* 트리거 실행에 필요한 정보 */)
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
			string id = ((unitState.Identity != null) ? unitState.Identity.DefinitionId : string.Empty);
			MonsterDefinition monsterDefinition = GameDataLoader.CurrentCatalog.GetMonster(id);
			SkillTriggerDefinition[] array = ((monsterDefinition != null) ? monsterDefinition.SkillTriggers : null);
			if (array == null || array.Length == 0)
			{
				continue;
			}
			foreach (SkillTriggerDefinition trigger in array)
			{
				if (ShouldRunPassiveOwnerTrigger(trigger, unitState, triggerEvent, triggerContext) && PassesCountGate(combatManager, unitState, trigger) && PassesProcGate(combatManager, unitState, trigger))
				{
					ExecuteTrigger(combatManager, roster, unitEntry, unitState, trigger, triggerContext);
				}
			}
		}
	}

	/*
	 * ShouldRunSourceOwnedTrigger 조건을 만족하는지 확인한다.
	 */
	private static bool ShouldRunSourceOwnedTrigger(SkillTriggerDefinition trigger /* 실행하거나 검사할 트리거 */, UnitCombatState source /* 효과를 발생시킨 유닛 */, string sourceSkillId /* 효과를 발생시킨 스킬 식별자 */, SkillTriggerEvent triggerEvent /* 트리거를 발생시킨 사건 종류 */, TriggerExecutionContext triggerContext /* 트리거 실행에 필요한 정보 */)
	{
		if (trigger != null && trigger.TriggerEvent == triggerEvent && string.Equals(trigger.SourceSkillId, sourceSkillId, StringComparison.OrdinalIgnoreCase) && MatchesEventSkillId(trigger.EventSkillIds, triggerContext.EventSourceSkillId) && StatusConditionRules.MatchesSkillRuntimeKinds(trigger.EventSkillRuntimeKindValues, triggerContext.EventSourceSkillId) && (!trigger.RequireEventExecute || triggerContext.EventWasExecute) && HasAllChoices(source, trigger.RequiredActiveChoiceIds) && !HasAnyChoice(source, trigger.ExcludedActiveChoiceIds))
		{
			return MeetsSourceStatusRequirement(source, trigger.RequiredSourceStatusKind, trigger.RequiredSourceStatusMinStacks);
		}
		return false;
	}

	/*
	 * ShouldRunPassiveOwnerTrigger 조건을 만족하는지 확인한다.
	 */
	private static bool ShouldRunPassiveOwnerTrigger(SkillTriggerDefinition trigger /* 실행하거나 검사할 트리거 */, UnitCombatState owner /* 정보를 소유한 유닛 */, SkillTriggerEvent triggerEvent /* 트리거를 발생시킨 사건 종류 */, TriggerExecutionContext triggerContext /* 트리거 실행에 필요한 정보 */)
	{
		if (trigger == null || owner == null || owner.Skills == null || trigger.TriggerEvent != triggerEvent || string.IsNullOrWhiteSpace(trigger.SourceSkillId) || !owner.Skills.HasPassiveSkill(trigger.SourceSkillId) || !MatchesEventSkillId(trigger.EventSkillIds, triggerContext.EventSourceSkillId) || !StatusConditionRules.MatchesSkillRuntimeKinds(trigger.EventSkillRuntimeKindValues, triggerContext.EventSourceSkillId) || (trigger.RequireEventExecute && !triggerContext.EventWasExecute) || !HasAllChoices(owner, trigger.RequiredActiveChoiceIds) || HasAnyChoice(owner, trigger.ExcludedActiveChoiceIds) || !MeetsSourceStatusRequirement(owner, trigger.RequiredSourceStatusKind, trigger.RequiredSourceStatusMinStacks))
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
			return MatchesEventSourceScope(trigger.EventSourceScopeValue, owner, triggerContext.EventSource);
		}
		return false;
	}

	/*
	 * HasAllChoices 조건을 만족하는지 확인한다.
	 */
	private static bool HasAllChoices(UnitCombatState source /* 효과를 발생시킨 유닛 */, string[] choiceIds /* 선택지 목록 */)
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

	/*
	 * HasAnyChoice 조건을 만족하는지 확인한다.
	 */
	private static bool HasAnyChoice(UnitCombatState source /* 효과를 발생시킨 유닛 */, string[] choiceIds /* 선택지 목록 */)
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

	/*
	 * MeetsSourceStatusRequirement 조건을 만족하는지 확인한다.
	 */
	private static bool MeetsSourceStatusRequirement(UnitCombatState owner /* 정보를 소유한 유닛 */, StatusEffectKind statusKind /* 상태 효과 종류 */, int minStacks /* 최소 중첩 수 */)
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

	/*
	 * MatchesConditionStatus 조건을 만족하는지 확인한다.
	 */
	private static bool MatchesConditionStatus(SkillTriggerDefinition trigger /* 실행하거나 검사할 트리거 */, StatusRuntimeInstance status /* 실행 중인 상태 효과 */)
	{
		if (trigger != null)
		{
			return StatusConditionRules.MatchesConditionStatus(status, trigger.ConditionStatuses);
		}
		return true;
	}

	/*
	 * MatchesTriggerAttribute 조건을 만족하는지 확인한다.
	 */
	private static bool MatchesTriggerAttribute(DamageAttribute[] attributes /* 허용 속성 목록 */, DamageAttribute eventAttribute /* 사건 속성 */)
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

	/*
	 * PassesProcGate 조건을 만족하는지 확인한다.
	 */
	private static bool PassesProcGate(InGameCombatManager combatManager /* 전투 진행 관리자 */, UnitCombatState owner /* 정보를 소유한 유닛 */, SkillTriggerDefinition trigger /* 실행하거나 검사할 트리거 */)
	{
		if (combatManager == null || owner == null || trigger == null)
		{
			return false;
		}
		float num = SkillExecutionState.PassiveChoices(owner, trigger.SourceSkillId).TriggerProcChanceBonus(trigger.TriggerId);
		float num2 = ((trigger.ProcChance > 0f) ? Mathf.Clamp01(trigger.ProcChance + num) : Mathf.Clamp01(1f + num));
		if (num2 <= 0f || UnityEngine.Random.value > num2)
		{
			return false;
		}
		return combatManager.PassiveEffects.ConsumeTriggerCooldown(BuildPassiveTriggerCooldownKey(owner, trigger), trigger.InternalCooldownSeconds);
	}

	/*
	 * PassesCountGate 조건을 만족하는지 확인한다.
	 */
	private static bool PassesCountGate(InGameCombatManager combatManager /* 전투 진행 관리자 */, UnitCombatState owner /* 정보를 소유한 유닛 */, SkillTriggerDefinition trigger /* 실행하거나 검사할 트리거 */)
	{
		if (combatManager == null || owner == null || trigger == null)
		{
			return false;
		}
		return combatManager.PassiveEffects.ConsumeTriggerCount(BuildPassiveTriggerCooldownKey(owner, trigger), trigger.TriggerEveryCount);
	}

	/*
	 * MatchesEventSourceScope 조건을 만족하는지 확인한다.
	 */
	private static bool MatchesEventSourceScope(SkillTriggerEventSourceScope scope /* 적용 범위 */, UnitCombatState owner /* 정보를 소유한 유닛 */, UnitCombatState eventSource /* 사건 발생 원본 */)
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

	/*
	 * MatchesEventSkillId 조건을 만족하는지 확인한다.
	 */
	private static bool MatchesEventSkillId(string[] skillIds /* 허용 스킬 식별자 목록 */, string eventSkillId /* 사건 스킬 식별자 */)
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

	/*
	 * MatchesConditionStatusSourceSkill 조건을 만족하는지 확인한다.
	 */
	private static bool MatchesConditionStatusSourceSkill(string[] sourceSkillIds /* 발생 원본 스킬 식별자 목록 */, UnitCombatState target /* 효과를 받을 대상 유닛 */, string eventTriggerSourceSkillId = null /* 사건 트리거 발생 원본 스킬 식별자 */)
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

	/*
	 * IsSameUnit 조건을 만족하는지 확인한다.
	 */
	private static bool IsSameUnit(UnitCombatState left /* 왼쪽 */, UnitCombatState right /* 오른쪽 */)
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

	/*
	 * BuildPassiveTriggerCooldownKey에 필요한 결과를 만들어 반환한다.
	 */
	private static string BuildPassiveTriggerCooldownKey(UnitCombatState owner /* 정보를 소유한 유닛 */, SkillTriggerDefinition trigger /* 실행하거나 검사할 트리거 */)
	{
		string obj = ((owner != null && owner.Identity != null && !string.IsNullOrWhiteSpace(owner.Identity.UnitId)) ? owner.Identity.UnitId : ((owner != null) ? owner.GetHashCode().ToString() : "unknown"));
		string text = ((trigger != null && !string.IsNullOrWhiteSpace(trigger.TriggerId)) ? trigger.TriggerId : ((trigger != null) ? trigger.SourceSkillId : "unknown"));
		return obj + ":" + text;
	}

	/*
	 * ExecuteTrigger 실행을 처리한다.
	 */
	private static void ExecuteTrigger(InGameCombatManager combatManager /* 전투 진행 관리자 */, CombatUnitRegistry roster /* 전투에 등록된 유닛 목록 */, CombatUnitEntry sourceEntry /* 효과를 발생시킨 유닛의 등록 정보 */, UnitCombatState source /* 효과를 발생시킨 유닛 */, SkillTriggerDefinition trigger /* 실행하거나 검사할 트리거 */, TriggerExecutionContext triggerContext /* 트리거 실행에 필요한 정보 */)
	{
		if (trigger == null)
		{
			return;
		}
		int num = Mathf.Max(1, trigger.RepeatCount);
		for (int i = 0; i < num; i++)
		{
			float num2 = Mathf.Max(0f, trigger.TriggerDelaySeconds) + ((i > 0) ? (Mathf.Max(0f, trigger.RepeatIntervalSeconds) * (float)i) : 0f);
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

	/*
	 * ExecuteDelayed 실행 결과를 반환한다.
	 */
	private static IEnumerator ExecuteDelayed(InGameCombatManager combatManager /* 전투 진행 관리자 */, CombatUnitRegistry roster /* 전투에 등록된 유닛 목록 */, CombatUnitEntry sourceEntry /* 효과를 발생시킨 유닛의 등록 정보 */, UnitCombatState source /* 효과를 발생시킨 유닛 */, SkillTriggerDefinition trigger /* 실행하거나 검사할 트리거 */, TriggerExecutionContext triggerContext /* 트리거 실행에 필요한 정보 */, float delaySeconds /* 실행 전 대기 시간(초) */)
	{
		yield return new WaitForSeconds(Mathf.Max(0f, delaySeconds));
		ExecuteOnce(combatManager, roster, sourceEntry, source, trigger, triggerContext);
	}

	/*
	 * ExecuteOnce 실행을 처리한다.
	 */
	private static void ExecuteOnce(InGameCombatManager combatManager /* 전투 진행 관리자 */, CombatUnitRegistry roster /* 전투에 등록된 유닛 목록 */, CombatUnitEntry sourceEntry /* 효과를 발생시킨 유닛의 등록 정보 */, UnitCombatState source /* 효과를 발생시킨 유닛 */, SkillTriggerDefinition trigger /* 실행하거나 검사할 트리거 */, TriggerExecutionContext triggerContext /* 트리거 실행에 필요한 정보 */)
	{
		TryExecuteOutcome(combatManager, roster, sourceEntry, source, trigger, triggerContext);
	}

	private static bool TryExecuteOutcome(
		InGameCombatManager combatManager,
		CombatUnitRegistry roster,
		CombatUnitEntry sourceEntry,
		UnitCombatState source,
		SkillTriggerDefinition trigger,
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

		SkillUseState runtime = source.SkillState.FindBySkillId(trigger.SourceSkillId);
		if (trigger.TriggeredSkill != null)
		{
			if (runtime == null)
			{
				return false;
			}

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

			var hasRawDamageOverride =
				trigger.DamageValueSource != SkillTriggerDamageValueSource.Fixed;
			return combatManager.SkillExecution.TryExecuteTriggered(
				sourceEntry,
				runtime,
				trigger,
				roster,
				combatManager,
				triggerContext.EventTarget,
				targetPoint,
				true,
				hasRawDamageOverride,
				ResolveTriggeredRawDamage(trigger, triggerContext),
				triggerContext.RecastGeneration);
		}

		if (trigger.Command != null)
		{
			return ExecuteCommand(
				combatManager,
				roster,
				sourceEntry,
				source,
				runtime,
				trigger.Command,
				triggerContext);
		}

		return false;
	}

	private static bool ExecuteCommand(
		InGameCombatManager combatManager,
		CombatUnitRegistry roster,
		CombatUnitEntry sourceEntry,
		UnitCombatState source,
		SkillUseState sourceRuntime,
		SkillTriggerCommand command,
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
		if (command.Kind == SkillTriggerCommandKind.RecastZone)
		{
			var snapshot = source.SkillState.CreateExecutionData(
				source,
				sourceRuntime,
				roster);
			return ZoneSkillExecutor.ExecuteRecast(
				context,
				snapshot,
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

			if (command.Kind == SkillTriggerCommandKind.ExtendStatusDuration)
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
				if (command.Kind == SkillTriggerCommandKind.RefundCooldown)
				{
					changed |= targetRuntime.ReduceCooldownRemaining(
						targetRuntime.EffectiveCooldownDuration * command.Ratio);
				}
				else if (command.Kind == SkillTriggerCommandKind.ReduceReload)
				{
					changed |= targetRuntime.ReduceReloadRemaining(
						targetRuntime.ReloadDuration * command.Ratio);
				}
			}
		}
		return changed;
	}

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

	private static float ResolveTriggeredRawDamage(
		SkillTriggerDefinition trigger,
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

	private static UnitCombatState SourceModel(
		CombatUnitRegistry roster,
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

	private static Vector2 UnitPosition(CombatUnitRegistry roster, UnitCombatState model)
	{
		var entry = roster != null ? roster.Find(model) : null;
		return entry != null && entry.Transform != null
			? (Vector2)entry.Transform.position
			: Vector2.zero;
	}
}

}
