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
		public UnitCombatState EventTarget { get; }

		public UnitCombatState Attacker { get; }

		public Vector2 EventCenter { get; }

		public StatusRuntimeInstance Status { get; }

		public float ShieldAbsorbedAmount { get; }

		public float EventAppliedDamage { get; }

		public DamageAttribute EventAttribute { get; }

		public string EventSourceSkillId { get; }

		public UnitCombatState EventSource { get; }

		public bool EventWasExecute { get; }

		public string EventTriggerSourceSkillId { get; }

		public int EventHitCount { get; }

		/*
		 * TriggerExecutionContext에 필요한 값을 초기화한다.
		 */
		public TriggerExecutionContext(UnitCombatState eventTarget /* 사건 대상 */, UnitCombatState attacker /* 공격자 */, Vector2 eventCenter /* 사건 중심 위치 */, StatusRuntimeInstance status /* 실행 중인 상태 효과 */, float shieldAbsorbedAmount /* 보호막 흡수된 수치 */, float eventAppliedDamage /* 사건 적용된 피해 */, DamageAttribute eventAttribute /* 사건 속성 */, string eventSourceSkillId /* 사건 발생 원본 스킬 식별자 */, UnitCombatState eventSource = null /* 사건 발생 원본 */, bool eventWasExecute = false /* 사건 발생 처형 여부 */, string eventTriggerSourceSkillId = null /* 사건 트리거 발생 원본 스킬 식별자 */, int eventHitCount = 0 /* 사건 적중 횟수 */)
		{
			EventTarget = eventTarget;
			Attacker = attacker;
			EventCenter = eventCenter;
			Status = status;
			ShieldAbsorbedAmount = shieldAbsorbedAmount;
			EventAppliedDamage = eventAppliedDamage;
			EventAttribute = eventAttribute;
			EventSourceSkillId = eventSourceSkillId;
			EventSource = eventSource;
			EventWasExecute = eventWasExecute;
			EventTriggerSourceSkillId = eventTriggerSourceSkillId;
			EventHitCount = Mathf.Max(0, eventHitCount);
		}
	}

	/*
	 * family 실행기가 발행한 lifecycle 사건을 Trigger 판정 경로로 전달한다.
	 * legacy Effect가 같은 사건을 처리하는 동안에는 Node 경로를 막아 중복 실행을 방지한다.
	 */
	internal static void PublishLifecycleEvent(
		SkillTriggerEvent triggerEvent,
		SkillActionContext actionContext,
		bool legacyEffectActive)
	{
		if (legacyEffectActive
			|| actionContext == null
			|| actionContext.Source == null
			|| actionContext.ExecutionContext == null)
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
			eventHitCount: actionContext.HitCount);
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
		SkillTriggerDefinition[] array = SourceOwnedPlanTriggers(source, sourceSkillId, (monsterDefinition != null) ? monsterDefinition.SkillTriggers : null);
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
	 * SourceOwnedPlanTriggers 결과를 계산해 반환한다.
	 */
	private static SkillTriggerDefinition[] SourceOwnedPlanTriggers(UnitCombatState source /* 효과를 발생시킨 유닛 */, string sourceSkillId /* 효과를 발생시킨 스킬 식별자 */, SkillTriggerDefinition[] baseTriggers /* 유닛 기본 Trigger 목록 */)
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
		if (trigger != null && trigger.TriggerEvent == triggerEvent && string.Equals(trigger.SourceSkillId, sourceSkillId, StringComparison.OrdinalIgnoreCase) && MatchesEventSkillId(trigger.EventSkillId, triggerContext.EventSourceSkillId) && StatusConditionRules.MatchesSkillRuntimeKinds(trigger.EventSkillRuntimeKindValues, triggerContext.EventSourceSkillId) && (!trigger.RequireEventExecute || triggerContext.EventWasExecute) && HasAllChoices(source, trigger.RequiresActiveChoiceId) && !HasAnyChoice(source, trigger.ExcludesActiveChoiceId))
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
		if (trigger == null || owner == null || owner.Skills == null || trigger.TriggerEvent != triggerEvent || string.IsNullOrWhiteSpace(trigger.SourceSkillId) || !owner.Skills.HasPassiveSkill(trigger.SourceSkillId) || !MatchesEventSkillId(trigger.EventSkillId, triggerContext.EventSourceSkillId) || !StatusConditionRules.MatchesSkillRuntimeKinds(trigger.EventSkillRuntimeKindValues, triggerContext.EventSourceSkillId) || (trigger.RequireEventExecute && !triggerContext.EventWasExecute) || !HasAllChoices(owner, trigger.RequiresActiveChoiceId) || HasAnyChoice(owner, trigger.ExcludesActiveChoiceId) || !MeetsSourceStatusRequirement(owner, trigger.RequiredSourceStatusKind, trigger.RequiredSourceStatusMinStacks))
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
		if (MatchesTriggerAttribute(trigger.TriggerAttribute, triggerContext.EventAttribute))
		{
			return MatchesEventSourceScope(trigger.EventSourceScope, owner, triggerContext.EventSource);
		}
		return false;
	}

	/*
	 * HasAllChoices 조건을 만족하는지 확인한다.
	 */
	private static bool HasAllChoices(UnitCombatState source /* 효과를 발생시킨 유닛 */, string choiceList /* 선택지 목록 */)
	{
		if (string.IsNullOrWhiteSpace(choiceList))
		{
			return true;
		}
		if (source == null || source.Skills == null)
		{
			return false;
		}
		string[] array = choiceList.Split(';', ',');
		for (int i = 0; i < array.Length; i++)
		{
			string text = ((array[i] != null) ? array[i].Trim() : string.Empty);
			if (!string.IsNullOrWhiteSpace(text) && !source.Skills.HasChoice(text))
			{
				return false;
			}
		}
		return true;
	}

	/*
	 * HasAnyChoice 조건을 만족하는지 확인한다.
	 */
	private static bool HasAnyChoice(UnitCombatState source /* 효과를 발생시킨 유닛 */, string choiceList /* 선택지 목록 */)
	{
		if (string.IsNullOrWhiteSpace(choiceList) || source == null || source.Skills == null)
		{
			return false;
		}
		string[] array = choiceList.Split(';', ',');
		for (int i = 0; i < array.Length; i++)
		{
			string text = ((array[i] != null) ? array[i].Trim() : string.Empty);
			if (!string.IsNullOrWhiteSpace(text) && source.Skills.HasChoice(text))
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
	private static bool MatchesTriggerAttribute(string rawAttribute /* 변환 전 속성 */, DamageAttribute eventAttribute /* 사건 속성 */)
	{
		if (string.IsNullOrWhiteSpace(rawAttribute))
		{
			return true;
		}
		string[] array = rawAttribute.Split(';', ',');
		for (int i = 0; i < array.Length; i++)
		{
			string text = ((array[i] != null) ? array[i].Trim() : string.Empty);
			if (!string.IsNullOrWhiteSpace(text) && string.Equals(text, eventAttribute.ToString(), StringComparison.OrdinalIgnoreCase))
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
	private static bool MatchesEventSourceScope(string scope /* 적용 범위 */, UnitCombatState owner /* 정보를 소유한 유닛 */, UnitCombatState eventSource /* 사건 발생 원본 */)
	{
		if (string.IsNullOrWhiteSpace(scope))
		{
			return true;
		}
		if (owner == null || eventSource == null)
		{
			return false;
		}
		string a = scope.Trim();
		if (string.Equals(a, "owner", StringComparison.OrdinalIgnoreCase))
		{
			return IsSameUnit(owner, eventSource);
		}
		if (string.Equals(a, "all_allies", StringComparison.OrdinalIgnoreCase))
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
	private static bool MatchesEventSkillId(string rawSkillIds /* 변환 전 스킬 식별자 목록 */, string eventSkillId /* 사건 스킬 식별자 */)
	{
		if (string.IsNullOrWhiteSpace(rawSkillIds))
		{
			return true;
		}
		if (string.IsNullOrWhiteSpace(eventSkillId))
		{
			return false;
		}
		string[] array = rawSkillIds.Split(';', ',');
		for (int i = 0; i < array.Length; i++)
		{
			string text = ((array[i] != null) ? array[i].Trim() : string.Empty);
			if (!string.IsNullOrWhiteSpace(text) && string.Equals(text, eventSkillId, StringComparison.OrdinalIgnoreCase))
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
		if (TryExecuteOwnedNodes(combatManager, roster, sourceEntry, source, trigger, triggerContext))
		{
			return;
		}

		SkillTriggerActionKind action = TriggerAction(trigger);
		switch (action)
		{
			case SkillTriggerActionKind.SingleAttack:
				ExecuteSingleAttackAction(combatManager, roster, sourceEntry, source, trigger, triggerContext);
				break;
			case SkillTriggerActionKind.LineAttack:
				ExecuteLineAttackAction(combatManager, roster, sourceEntry, source, trigger, triggerContext);
				break;
			case SkillTriggerActionKind.Effect:
				ExecuteEffectAction(combatManager, roster, sourceEntry, trigger, triggerContext);
				break;
			case SkillTriggerActionKind.CooldownRefund:
				ReduceTargetCooldownAction(roster, sourceEntry, trigger);
				break;
			case SkillTriggerActionKind.ReloadReduce:
				ReduceTargetReloadAction(roster, sourceEntry, trigger);
				break;
			default:
				ExecuteTriggeredSkillAction(combatManager, sourceEntry, trigger, triggerContext);
				break;
		}
	}

	private static bool TryExecuteOwnedNodes(
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
			|| trigger == null
			|| !SkillNodeExecutor.HasRuntimeActions(trigger.Nodes))
		{
			return false;
		}

		SkillUseState runtime = source.SkillState.FindBySkillId(trigger.SourceSkillId);
		SkillExecutionData executionData = runtime != null
			? source.SkillState.CreateExecutionData(source, runtime, roster)
			: null;
		var executionContext = new SkillExecutionContext(
			combatManager,
			roster,
			sourceEntry,
			runtime,
			triggerContext.EventTarget);
		var actionContext = new SkillActionContext(
			source,
			trigger.SourceSkillId,
			triggerContext.EventTarget,
			triggerContext.EventCenter,
			triggerContext.EventAppliedDamage,
			triggerContext.EventHitCount,
			executionData,
			executionContext,
			trigger.TriggerId);
		SkillNodeExecutor.Execute(trigger.Nodes, actionContext);
		return true;
	}

	/*
	 * CSV 로딩에서 확정한 Trigger 동작을 반환한다.
	 */
	private static SkillTriggerActionKind TriggerAction(SkillTriggerDefinition trigger /* 실행할 Trigger */)
	{
		if (trigger.TriggerAction != SkillTriggerActionKind.Auto)
		{
			return trigger.TriggerAction;
		}

		if (trigger.RuntimeKind == SkillRuntimeKind.SingleAttack)
		{
			return SkillTriggerActionKind.SingleAttack;
		}

		return SkillTriggerActionKind.TriggeredSkill;
	}

	/*
	 * ExecuteTriggeredSkillAction 실행 결과를 반환한다.
	 */
	internal static bool ExecuteTriggeredSkillAction(InGameCombatManager combatManager /* 전투 진행 관리자 */, CombatUnitEntry sourceEntry /* 효과를 발생시킨 유닛의 등록 정보 */, SkillTriggerDefinition trigger /* 실행하거나 검사할 트리거 */, TriggerExecutionContext triggerContext /* 트리거 실행에 필요한 정보 */)
	{
		if (combatManager == null || sourceEntry == null || trigger == null || sourceEntry.Model == null || sourceEntry.Model.Skills == null || string.IsNullOrWhiteSpace(trigger.TriggeredSkillId))
		{
			return false;
		}
		SkillUseState skillRuntimeInstance = sourceEntry.Model.SkillState.FindBySkillId(trigger.TriggeredSkillId);
		if (skillRuntimeInstance == null || skillRuntimeInstance.Data == null || !MatchesRuntimeKind(skillRuntimeInstance.Data, trigger.RuntimeKind))
		{
			return false;
		}
		Vector2 targetPoint = ((triggerContext.EventTarget != null) ? triggerContext.EventCenter : triggerContext.EventCenter);
		float triggeredDamageMultiplier = ((trigger.DamageMultiplier > 0f) ? trigger.DamageMultiplier : 1f);
		return combatManager.SkillExecution.TryExecuteTriggered(sourceEntry, skillRuntimeInstance, combatManager.UnitRegistry, combatManager, targetPoint, hasTargetPoint: true, triggeredDamageMultiplier, trigger.SourceSkillId);
	}

	/*
	 * ExecuteEffectAction 실행 결과를 반환한다.
	 */
	internal static bool ExecuteEffectAction(InGameCombatManager combatManager /* 전투 진행 관리자 */, CombatUnitRegistry roster /* 전투에 등록된 유닛 목록 */, CombatUnitEntry sourceEntry /* 효과를 발생시킨 유닛의 등록 정보 */, SkillTriggerDefinition trigger /* 실행하거나 검사할 트리거 */, TriggerExecutionContext triggerContext /* 트리거 실행에 필요한 정보 */)
	{
		if (combatManager == null || roster == null || sourceEntry == null || trigger == null || string.IsNullOrWhiteSpace(trigger.TriggeredEffectId))
		{
			return false;
		}
		SkillEffectDefinition skillEffectDefinition = TriggeredEffect(sourceEntry.Model, trigger.TriggeredEffectId);
		if (skillEffectDefinition == null)
		{
			return false;
		}
		SkillExecutionContext context = new SkillExecutionContext(combatManager, roster, sourceEntry, null, triggerContext.EventTarget);
		SkillExecutionData snapshot = SkillExecutionState.PassiveChoices(sourceEntry.Model, trigger.SourceSkillId);
		return ApplyTriggeredEffect(context, snapshot, skillEffectDefinition, triggerContext.EventCenter);
	}

	/*
	 * Trigger가 선택한 추가 효과 종류에 맞는 적용 기능을 호출한다.
	 */
	private static bool ApplyTriggeredEffect(
		SkillExecutionContext context /* 스킬 실행에 필요한 정보 */,
		SkillExecutionData skillData /* 현재 스킬 강화 정보 */,
		SkillEffectDefinition effect /* 적용할 추가 효과 */,
		Vector2 defaultCenter /* 기본 효과 중심 */)
	{
		if (effect.EffectKind == SkillMultiEffectKind.Damage)
		{
			return ZoneSkillExecutor.ApplyAdditionalDamageEffect(context, skillData, effect, defaultCenter);
		}
		if (effect.EffectKind == SkillMultiEffectKind.Status)
		{
			return SkillStatus.ApplyEffect(context, skillData, effect, defaultCenter, false);
		}
		if (effect.EffectKind == SkillMultiEffectKind.ExtendStatusDuration)
		{
			return SkillStatus.ExtendEffectDuration(context, effect);
		}
		if (effect.EffectKind == SkillMultiEffectKind.RecastZone)
		{
			return ZoneSkillExecutor.ExecuteRecast(context, skillData, effect, defaultCenter);
		}
		return false;
	}

	/*
	 * TriggeredEffect 결과를 계산해 반환한다.
	 */
	private static SkillEffectDefinition TriggeredEffect(UnitCombatState source /* 효과를 발생시킨 유닛 */, string effectId /* 효과 식별자 */)
	{
		if (source == null || source.Identity == null || string.IsNullOrWhiteSpace(effectId))
		{
			return null;
		}
		MonsterDefinition monsterDefinition = GameDataLoader.CurrentCatalog.GetMonster(source.Identity.DefinitionId);
		if (monsterDefinition == null)
		{
			return null;
		}
		SkillEffectDefinition skillEffectDefinition = FindEffect(monsterDefinition.ActiveSkills, effectId);
		if (skillEffectDefinition != null)
		{
			return skillEffectDefinition;
		}
		return FindEffect(monsterDefinition.PassiveSkills, effectId);
	}

	/*
	 * FindEffect에 해당하는 값을 찾아 반환한다.
	 */
	private static SkillEffectDefinition FindEffect(SkillSourceDefinition[] skills /* 스킬 목록 */, string effectId /* 효과 식별자 */)
	{
		if (skills == null || string.IsNullOrWhiteSpace(effectId))
		{
			return null;
		}
		for (int i = 0; i < skills.Length; i++)
		{
			SkillEffectDefinition skillEffectDefinition = FindEffect((skills[i] != null) ? skills[i].MultiEffects : null, effectId);
			if (skillEffectDefinition != null)
			{
				return skillEffectDefinition;
			}
		}
		return null;
	}

	/*
	 * FindEffect에 해당하는 값을 찾아 반환한다.
	 */
	private static SkillEffectDefinition FindEffect(PassiveDefinition[] skills /* 스킬 목록 */, string effectId /* 효과 식별자 */)
	{
		if (skills == null || string.IsNullOrWhiteSpace(effectId))
		{
			return null;
		}
		for (int i = 0; i < skills.Length; i++)
		{
			SkillEffectDefinition skillEffectDefinition = FindEffect((skills[i] != null) ? skills[i].PassiveEffects : null, effectId);
			if (skillEffectDefinition != null)
			{
				return skillEffectDefinition;
			}
		}
		return null;
	}

	/*
	 * FindEffect에 해당하는 값을 찾아 반환한다.
	 */
	private static SkillEffectDefinition FindEffect(SkillEffectDefinition[] effects /* 실행할 효과 목록 */, string effectId /* 효과 식별자 */)
	{
		if (effects == null || string.IsNullOrWhiteSpace(effectId))
		{
			return null;
		}
		foreach (SkillEffectDefinition skillEffectDefinition in effects)
		{
			if (skillEffectDefinition != null && string.Equals(skillEffectDefinition.EffectId, effectId, StringComparison.OrdinalIgnoreCase))
			{
				return skillEffectDefinition;
			}
		}
		return null;
	}


	/*
	 * ReduceTargetCooldownAction 작업 결과를 반환한다.
	 */
	internal static bool ReduceTargetCooldownAction(CombatUnitRegistry roster /* 전투에 등록된 유닛 목록 */, CombatUnitEntry sourceEntry /* 효과를 발생시킨 유닛의 등록 정보 */, SkillTriggerDefinition trigger /* 실행하거나 검사할 트리거 */)
	{
		if (trigger == null || trigger.CooldownRefundRatio <= 0f)
		{
			return false;
		}
		List<SkillUseState> list = TargetRuntimes(roster, sourceEntry, trigger);
		bool flag = false;
		for (int i = 0; i < list.Count; i++)
		{
			SkillUseState skillRuntimeInstance = list[i];
			if (skillRuntimeInstance != null)
			{
				flag = skillRuntimeInstance.ReduceCooldownRemaining(skillRuntimeInstance.EffectiveCooldownDuration * Mathf.Clamp01(trigger.CooldownRefundRatio)) || flag;
			}
		}
		return flag;
	}

	/*
	 * ReduceTargetReloadAction 작업 결과를 반환한다.
	 */
	internal static bool ReduceTargetReloadAction(CombatUnitRegistry roster /* 전투에 등록된 유닛 목록 */, CombatUnitEntry sourceEntry /* 효과를 발생시킨 유닛의 등록 정보 */, SkillTriggerDefinition trigger /* 실행하거나 검사할 트리거 */)
	{
		if (trigger == null || trigger.ReloadReduceRatio <= 0f)
		{
			return false;
		}
		List<SkillUseState> list = TargetRuntimes(roster, sourceEntry, trigger);
		bool flag = false;
		for (int i = 0; i < list.Count; i++)
		{
			SkillUseState skillRuntimeInstance = list[i];
			if (skillRuntimeInstance != null)
			{
				flag = skillRuntimeInstance.ReduceReloadRemaining(skillRuntimeInstance.ReloadDuration * Mathf.Clamp01(trigger.ReloadReduceRatio)) || flag;
			}
		}
		return flag;
	}

	/*
	 * TargetRuntimes 결과를 계산해 반환한다.
	 */
	private static List<SkillUseState> TargetRuntimes(CombatUnitRegistry roster /* 전투에 등록된 유닛 목록 */, CombatUnitEntry sourceEntry /* 효과를 발생시킨 유닛의 등록 정보 */, SkillTriggerDefinition trigger /* 실행하거나 검사할 트리거 */)
	{
		List<SkillUseState> list = new List<SkillUseState>();
		List<CombatUnitEntry> list2 = CooldownTargetEntries(roster, sourceEntry, trigger);
		string text = string.Empty;
		if (trigger != null)
		{
			text = trigger.TriggeredSkillId;
			if (!string.IsNullOrWhiteSpace(trigger.TargetSkillId))
			{
				text = trigger.TargetSkillId;
			}
		}
		for (int i = 0; i < list2.Count; i++)
		{
			CombatUnitEntry unitEntry = list2[i];
			SkillExecutionState unitSkillRuntimeSet = null;
			if (unitEntry != null && unitEntry.Model != null)
			{
				unitSkillRuntimeSet = unitEntry.Model.SkillState;
			}
			if (unitSkillRuntimeSet == null)
			{
				continue;
			}
			if (!string.IsNullOrWhiteSpace(text))
			{
				SkillUseState skillRuntimeInstance = unitSkillRuntimeSet.FindBySkillId(text);
				if (skillRuntimeInstance != null)
				{
					list.Add(skillRuntimeInstance);
				}
				continue;
			}
			IReadOnlyList<SkillUseState> activeSkills = unitSkillRuntimeSet.ActiveSkills;
			int num = 0;
			while (activeSkills != null && num < activeSkills.Count)
			{
				SkillUseState skillRuntimeInstance2 = activeSkills[num];
				if (skillRuntimeInstance2 != null)
				{
					list.Add(skillRuntimeInstance2);
				}
				num++;
			}
		}
		return list;
	}

	/*
	 * CooldownTargetEntries 결과를 계산해 반환한다.
	 */
	private static List<CombatUnitEntry> CooldownTargetEntries(CombatUnitRegistry roster /* 전투에 등록된 유닛 목록 */, CombatUnitEntry sourceEntry /* 효과를 발생시킨 유닛의 등록 정보 */, SkillTriggerDefinition trigger /* 실행하거나 검사할 트리거 */)
	{
		List<CombatUnitEntry> list = new List<CombatUnitEntry>();
		if (trigger != null && trigger.TargetSide == SkillMultiEffectTargetSide.AllAllies)
		{
			IReadOnlyList<CombatUnitEntry> readOnlyList = roster?.Entries;
			UnitSide unitSide = ((sourceEntry != null && sourceEntry.Model != null && sourceEntry.Model.Identity != null) ? sourceEntry.Model.Identity.Side : UnitSide.Player);
			int num = 0;
			while (readOnlyList != null && num < readOnlyList.Count)
			{
				CombatUnitEntry unitEntry = readOnlyList[num];
				UnitIdentity unitIdentity = ((unitEntry != null && unitEntry.Model != null) ? unitEntry.Model.Identity : null);
				if (unitEntry != null && unitEntry.Model != null && unitIdentity != null && unitIdentity.Side == unitSide && unitIdentity.Role != UnitRole.Nexus)
				{
					list.Add(unitEntry);
				}
				num++;
			}
			return list;
		}
		if (sourceEntry != null && sourceEntry.Model != null)
		{
			list.Add(sourceEntry);
		}
		return list;
	}

	/*
	 * ExecuteSingleAttackAction 실행 결과를 반환한다.
	 */
	internal static bool ExecuteSingleAttackAction(InGameCombatManager combatManager /* 전투 진행 관리자 */, CombatUnitRegistry roster /* 전투에 등록된 유닛 목록 */, CombatUnitEntry sourceEntry /* 효과를 발생시킨 유닛의 등록 정보 */, UnitCombatState source /* 효과를 발생시킨 유닛 */, SkillTriggerDefinition trigger /* 실행하거나 검사할 트리거 */, TriggerExecutionContext triggerContext /* 트리거 실행에 필요한 정보 */)
	{
		if (combatManager == null || roster == null || sourceEntry == null)
		{
			return false;
		}
		SkillTargetingSpec targeting = BuildTargeting(trigger);
		Vector2 vector = Center(sourceEntry, roster, triggerContext, trigger, targeting);
		float num = Damage(source, trigger, triggerContext);
		if (num <= 0f)
		{
			return false;
		}
		string sourceSkillId = TriggeredDamageSourceSkillId(trigger);
		SkillEffectDefinition onHitStatusEffect = TriggeredOnHitStatusEffect(source, trigger);
		SkillExecutionData onHitData = SkillExecutionState.ActiveChoices(source, trigger.SourceSkillId);
		RuntimeSkillVisualSpec runtimeVisual = trigger.RuntimeVisual;
		bool flag = runtimeVisual != null && runtimeVisual.HasVisual();
		bool flag2 = runtimeVisual != null && runtimeVisual.Hitbox != null && runtimeVisual.Hitbox.HasHitbox();
		if ((flag2 || IsPrefabHitboxTrigger(trigger)) && combatManager.Effects != null)
		{
			var hitboxVisualName = "RuntimeTriggerHitbox";
			if (!string.IsNullOrWhiteSpace(trigger.TriggerId))
			{
				hitboxVisualName = "RuntimeTriggerHitbox_" + trigger.TriggerId;
			}

			GameObject gameObject;
			if (flag2)
			{
				gameObject = combatManager.Effects.CreateEffect(new EffectCreateRequest(runtimeVisual, null, hitboxVisualName, vector, Quaternion.identity, null, 0f, null, false, true, false));
			}
			else
			{
				gameObject = combatManager.Effects.CreateEffect(new EffectCreateRequest(null, trigger.SkillEffectPrefab, hitboxVisualName, vector, Quaternion.identity, null, 0f, null, false, true, false));
			}

			if (gameObject == null)
			{
				return false;
			}
			Physics2D.SyncTransforms();
			var hitTargetCount = 0;
			if (IsGlobalHitCount(trigger.HitTargetCount))
			{
				hitTargetCount = int.MaxValue;
			}
			else
			{
				hitTargetCount = ParseHitTargetCount(trigger.HitTargetCount);
			}

			bool result = ApplyPrefabHitbox(combatManager, sourceEntry, roster, targeting, gameObject, hitTargetCount, num, trigger.Attribute, sourceSkillId, trigger.TriggerId, triggerContext.EventTarget, onHitStatusEffect, onHitData);
			SingleSkillActor.Attach(gameObject).InitializeTimed(combatManager.Effects, 1f);
			return result;
		}
		var areaHitTargetCount = 0;
		if (IsGlobalHitCount(trigger.HitTargetCount))
		{
			areaHitTargetCount = int.MaxValue;
		}
		else
		{
			areaHitTargetCount = ParseHitTargetCount(trigger.HitTargetCount);
		}

		bool flag3 = ApplyAreaTrigger(combatManager, sourceEntry, roster, targeting, vector, Mathf.Max(0f, trigger.Radius), trigger.CoverAll || trigger.TargetShape == SkillMultiEffectTargetShape.Battlefield, areaHitTargetCount, num, trigger.Attribute, sourceSkillId, trigger.TriggerId, triggerContext.EventTarget, trigger.TargetSelection == SkillMultiEffectTargetSelection.EventTarget, onHitStatusEffect, onHitData);
		string visualName = "RuntimeTriggerVisual";
		if (!string.IsNullOrWhiteSpace(trigger.TriggerId))
		{
			visualName = "RuntimeTriggerVisual_" + trigger.TriggerId;
		}
		if (flag3 && combatManager.Effects != null && (flag || trigger.SkillEffectPrefab != null))
		{
			var visualObject = combatManager.Effects.CreateEffect(new EffectCreateRequest(
				runtimeVisual,
				trigger.SkillEffectPrefab,
				visualName,
				vector,
				Quaternion.identity,
				null,
				0f,
				null,
				false,
				true,
				false));
			if (visualObject != null)
			{
				SingleSkillActor.Attach(visualObject).InitializeTimed(combatManager.Effects, 1f);
			}
		}
		return flag3;
	}

	/*
	 * ExecuteLineAttackAction 실행 결과를 반환한다.
	 */
	internal static bool ExecuteLineAttackAction(InGameCombatManager combatManager /* 전투 진행 관리자 */, CombatUnitRegistry roster /* 전투에 등록된 유닛 목록 */, CombatUnitEntry sourceEntry /* 효과를 발생시킨 유닛의 등록 정보 */, UnitCombatState source /* 효과를 발생시킨 유닛 */, SkillTriggerDefinition trigger /* 실행하거나 검사할 트리거 */, TriggerExecutionContext triggerContext /* 트리거 실행에 필요한 정보 */)
	{
		if (combatManager == null || roster == null || sourceEntry == null || sourceEntry.Transform == null)
		{
			return false;
		}
		SkillTargetingSpec skillTargetingSpec = BuildTargeting(trigger);
		Vector2 vector = sourceEntry.Transform.position;
		CombatUnitEntry unitEntry = ((trigger.TargetSelection == SkillMultiEffectTargetSelection.EventTarget) ? FindPreferredEntry(roster, triggerContext.EventTarget) : SkillTargeting.FindNearestTarget(sourceEntry, roster, skillTargetingSpec));
		if (unitEntry == null || unitEntry == sourceEntry)
		{
			unitEntry = SkillTargeting.FindNearestTarget(sourceEntry, roster, skillTargetingSpec);
		}
		Vector2 vector2 = SkillTargeting.DirectionToTarget(vector, unitEntry);
		if (vector2.sqrMagnitude <= 0.0001f)
		{
			return false;
		}
		vector2.Normalize();
		float num = Damage(source, trigger, triggerContext);
		if (num <= 0f)
		{
			return false;
		}
		SkillExecutionData skillExecutionData = SkillExecutionState.ActiveChoices(source, trigger.SourceSkillId);
		SkillEffectDefinition skillEffectDefinition = TriggeredOnHitStatusEffect(source, trigger);
		SkillEffectDefinition[] onHitEffects;
		if (skillEffectDefinition == null)
		{
			onHitEffects = Array.Empty<SkillEffectDefinition>();
		}
		else
		{
			onHitEffects = new SkillEffectDefinition[1] { skillEffectDefinition };
		}
		float num2 = TriggeredLineLength();
		float num3 = Mathf.Max(0.1f, trigger.Radius);
		Vector2 vector3 = vector + vector2 * (num2 * 0.5f);
		RuntimeSkillVisualSpec visual = trigger.RuntimeVisual;
		bool num4 = visual != null && visual.HasVisual();
		EffectManager effects = combatManager.Effects;
		GameObject gameObject = null;
		if (effects != null && (num4 || trigger.SkillEffectPrefab != null))
		{
			string visualName = "RuntimeTriggerLineVisual";
			if (!string.IsNullOrWhiteSpace(trigger.TriggerId))
			{
				visualName = "RuntimeTriggerLineVisual_" + trigger.TriggerId;
			}
			gameObject = effects.CreateEffect(new EffectCreateRequest(visual, trigger.SkillEffectPrefab, visualName, vector3, EffectVisualBuilder.Rotation(vector2), null, 0f, null, false, true, false));
		}
		if (gameObject != null)
		{
			ConfigureTriggeredLineVisual(gameObject.transform, num2, num3);
			var lineActor = gameObject.GetComponent<LineSkillActor>();
			if (lineActor == null)
			{
				lineActor = gameObject.AddComponent<LineSkillActor>();
			}

			lineActor.InitializeVisualLifetime(effects, 0.1f);
		}
		var criticalChanceBonus = 0f;
		var criticalDamageBonus = 0f;
		if (skillExecutionData != null)
		{
			criticalChanceBonus = skillExecutionData.CritChanceBonus;
			criticalDamageBonus = skillExecutionData.CritDamageBonus;
		}

		return LineSkillActor.ApplyLineTick(combatManager, sourceEntry, roster, skillTargetingSpec, vector, vector2, num2, num3, 0f, num, trigger.Attribute, null, onHitEffects, null, skillExecutionData, source, TriggeredDamageSourceSkillId(trigger), criticalAllowed: true, criticalChanceBonus, criticalDamageBonus, null, null, trigger.TriggerId);
	}

	/*
	 * TriggeredOnHitStatusEffect 결과를 계산해 반환한다.
	 */
	private static SkillEffectDefinition TriggeredOnHitStatusEffect(UnitCombatState source /* 효과를 발생시킨 유닛 */, SkillTriggerDefinition trigger /* 실행하거나 검사할 트리거 */)
	{
		if (source == null || trigger == null || string.IsNullOrWhiteSpace(trigger.TriggeredEffectId))
		{
			return null;
		}
		SkillEffectDefinition skillEffectDefinition = TriggeredEffect(source, trigger.TriggeredEffectId);
		if (skillEffectDefinition == null || skillEffectDefinition.EffectKind != SkillMultiEffectKind.Status || skillEffectDefinition.EffectTiming != SkillMultiEffectTiming.OnHit || skillEffectDefinition.TargetSide != SkillMultiEffectTargetSide.Enemy)
		{
			return null;
		}
		return skillEffectDefinition;
	}

	/*
	 * TriggeredDamageSourceSkillId 결과를 계산해 반환한다.
	 */
	private static string TriggeredDamageSourceSkillId(SkillTriggerDefinition trigger /* 실행하거나 검사할 트리거 */)
	{
		if (!string.IsNullOrWhiteSpace((trigger != null) ? trigger.TriggeredSkillId : string.Empty))
		{
			return trigger.TriggeredSkillId;
		}
		if (trigger == null)
		{
			return string.Empty;
		}
		return trigger.SourceSkillId;
	}

	/*
	 * TriggeredLineLength 결과를 계산해 반환한다.
	 */
	private static float TriggeredLineLength()
	{
		return 31f;
	}

	/*
	 * ConfigureTriggeredLineVisual에 필요한 값을 설정한다.
	 */
	private static void ConfigureTriggeredLineVisual(Transform transform /* 위치 정보 */, float length /* 길이 */, float width /* 너비 */)
	{
		if (transform == null)
		{
			return;
		}
		SpriteRenderer component = transform.GetComponent<SpriteRenderer>();
		if (!(component == null) && !(component.sprite == null))
		{
			Vector3 size = component.sprite.bounds.size;
			Vector3 localScale = transform.localScale;
			if (size.x > 0.0001f)
			{
				localScale.x = Mathf.Sign((localScale.x == 0f) ? 1f : localScale.x) * (length / size.x);
			}
			if (size.y > 0.0001f)
			{
				localScale.y = Mathf.Sign((localScale.y == 0f) ? 1f : localScale.y) * (width / size.y);
			}
			transform.localScale = localScale;
		}
	}

	/*
	 * BuildTargeting에 필요한 결과를 만들어 반환한다.
	 */
	private static SkillTargetingSpec BuildTargeting(SkillTriggerDefinition trigger /* 실행하거나 검사할 트리거 */)
	{
		return new SkillTargetingSpec
		{
			TargetSide = ((trigger.TargetSide == SkillMultiEffectTargetSide.Self) ? SkillTargetSide.Self : ((trigger.TargetSide == SkillMultiEffectTargetSide.AllAllies) ? SkillTargetSide.AllAllies : SkillTargetSide.Enemy)),
			Selection = ((trigger.TargetSelection == SkillMultiEffectTargetSelection.Owner) ? SkillTargetSelection.Owner : ((trigger.TargetSelection == SkillMultiEffectTargetSelection.EventTarget) ? SkillTargetSelection.HighestHealth : SkillTargetSelection.Nearest)),
			Shape = ((trigger.TargetShape == SkillMultiEffectTargetShape.Battlefield) ? SkillTargetShape.Battlefield : ((trigger.TargetShape != SkillMultiEffectTargetShape.Single) ? SkillTargetShape.Circle : SkillTargetShape.Single)),
			Radius = trigger.Radius,
			CoverAll = (trigger.CoverAll || trigger.TargetShape == SkillMultiEffectTargetShape.Battlefield)
		};
	}

	/*
	 * Center 결과를 계산해 반환한다.
	 */
	private static Vector2 Center(CombatUnitEntry sourceEntry /* 효과를 발생시킨 유닛의 등록 정보 */, CombatUnitRegistry roster /* 전투에 등록된 유닛 목록 */, TriggerExecutionContext triggerContext /* 트리거 실행에 필요한 정보 */, SkillTriggerDefinition trigger /* 실행하거나 검사할 트리거 */, SkillTargetingSpec targeting /* 스킬 대상 선택 규칙 */)
	{
		if (trigger != null)
		{
			switch (trigger.CenterMode)
			{
			case SkillMultiEffectCenterMode.Caster:
				if (!(sourceEntry.Transform != null))
				{
					return triggerContext.EventCenter;
				}
				return sourceEntry.Transform.position;
			case SkillMultiEffectCenterMode.NearestEnemy:
			{
				CombatUnitEntry unitEntry = SkillTargeting.FindNearestTarget(sourceEntry, roster, targeting);
				if (unitEntry == null || !(unitEntry.Transform != null))
				{
					return triggerContext.EventCenter;
				}
				return unitEntry.Transform.position;
			}
			case SkillMultiEffectCenterMode.EffectTarget:
				return UnitPosition(roster, triggerContext.EventTarget);
			}
		}
		return triggerContext.EventCenter;
	}

	/*
	 * Damage 결과를 계산해 반환한다.
	 */
	private static float Damage(UnitCombatState source /* 효과를 발생시킨 유닛 */, SkillTriggerDefinition trigger /* 실행하거나 검사할 트리거 */, TriggerExecutionContext triggerContext /* 트리거 실행에 필요한 정보 */)
	{
		switch (trigger.DamageSource)
		{
		case SkillTriggerDamageSource.ShieldAppliedAmount:
			return Mathf.Max(0f, (triggerContext.Status != null) ? triggerContext.Status.AppliedShieldAmount : 0f) * Mathf.Max(0f, trigger.DamageSourceMultiplier) * Mathf.Max(0f, trigger.DamageMultiplier);
		case SkillTriggerDamageSource.ShieldRemainingAmount:
			return Mathf.Max(0f, (triggerContext.Status != null) ? triggerContext.Status.RemainingShieldAmount : 0f) * Mathf.Max(0f, trigger.DamageSourceMultiplier) * Mathf.Max(0f, trigger.DamageMultiplier);
		case SkillTriggerDamageSource.ShieldAbsorbedAmount:
			return Mathf.Max(0f, triggerContext.ShieldAbsorbedAmount) * Mathf.Max(0f, trigger.DamageSourceMultiplier) * Mathf.Max(0f, trigger.DamageMultiplier);
		case SkillTriggerDamageSource.TrackedIncomingDamage:
			return Mathf.Max(0f, (triggerContext.Status != null) ? triggerContext.Status.GetTrackedIncomingDamage(TrackedAttribute(trigger)) : 0f) * Mathf.Max(0f, trigger.DamageSourceMultiplier) * Mathf.Max(0f, trigger.DamageMultiplier);
		case SkillTriggerDamageSource.EventAppliedDamage:
			return Mathf.Max(0f, triggerContext.EventAppliedDamage) * Mathf.Max(0f, trigger.DamageSourceMultiplier) * Mathf.Max(0f, trigger.DamageMultiplier);
		default:
		{
			SkillDamageSpec damage = new SkillDamageSpec
			{
				SkillId = trigger.SourceSkillId,
				Element = trigger.Attribute,
				BaseDamage = trigger.BaseDamage,
				AttackPowerCoefficient = trigger.AttackPowerCoefficient,
				SpellPowerCoefficient = trigger.SpellPowerCoefficient,
				CriticalAllowed = true
			};
			return DamageCalculator.CalculateRawDamage(source, damage) * Mathf.Max(0f, trigger.DamageMultiplier);
		}
		}
	}

	/*
	 * ApplyPrefabHitbox 처리를 대상에 적용한다.
	 */
	private static bool ApplyPrefabHitbox(InGameCombatManager manager /* 전투 진행 관리자 */, CombatUnitEntry sourceEntry /* 효과를 발생시킨 유닛의 등록 정보 */, CombatUnitRegistry roster /* 전투에 등록된 유닛 목록 */, SkillTargetingSpec targeting /* 스킬 대상 선택 규칙 */, GameObject hitboxObject /* 피격 판정 게임 오브젝트 */, int maxTargets /* 처리할 수 있는 최대 대상 수 */, float damage /* 적용하거나 전달할 피해량 */, DamageAttribute attribute /* 피해 속성 */, string sourceSkillId /* 효과를 발생시킨 스킬 식별자 */, string damageMeterSourceId /* 피해량 기록에 사용할 발생 원본 식별자 */, UnitCombatState preferredTarget /* 우선 처리할 대상 유닛 */, SkillEffectDefinition onHitStatusEffect /* 적중 시 적용할 상태 효과 */, SkillExecutionData onHitData /* 적중 효과에 적용할 스킬 강화 정보 */)
	{
		if (manager == null || sourceEntry == null || roster == null || hitboxObject == null || maxTargets <= 0)
		{
			return false;
		}
		Collider2D[] componentsInChildren = hitboxObject.GetComponentsInChildren<Collider2D>();
		if (componentsInChildren == null || componentsInChildren.Length == 0)
		{
			return false;
		}
		List<CombatUnitEntry> list = OrderedTargets(sourceEntry, roster, targeting, preferredTarget, preferredTarget != null);
		bool result = false;
		int num = 0;
		for (int i = 0; i < list.Count; i++)
		{
			CombatUnitEntry unitEntry = list[i];
			if (IsTargetInsideHitbox(componentsInChildren, unitEntry))
			{
				InGameResourceChangeResult damageResult = manager.ApplyDamage(unitEntry.Model, damage, attribute, sourceEntry.Model, criticalAllowed: true, 0f, 0f, sourceSkillId, suppressOutgoingDamageTriggers: false, sourceHitWasExecute: false, damageMeterSourceId);
				if (!damageResult.IsDead)
				{
					TryApplyTriggeredOnHitStatusEffect(manager, unitEntry.Model, onHitStatusEffect, onHitData, sourceEntry.Model);
				}
				result = true;
				num++;
				if (num >= maxTargets)
				{
					break;
				}
			}
		}
		return result;
	}

	/*
	 * OrderedTargets 결과를 계산해 반환한다.
	 */
	private static List<CombatUnitEntry> OrderedTargets(CombatUnitEntry sourceEntry /* 효과를 발생시킨 유닛의 등록 정보 */, CombatUnitRegistry roster /* 전투에 등록된 유닛 목록 */, SkillTargetingSpec targeting /* 스킬 대상 선택 규칙 */, UnitCombatState preferredTarget /* 우선 처리할 대상 유닛 */, bool preferEventTarget /* 사건 대상 우선 여부 */)
	{
		IReadOnlyList<CombatUnitEntry> readOnlyList = SkillTargeting.TargetList(sourceEntry, roster, targeting);
		List<CombatUnitEntry> list = new List<CombatUnitEntry>();
		for (int i = 0; i < readOnlyList.Count; i++)
		{
			CombatUnitEntry unitEntry = readOnlyList[i];
			if (unitEntry != null && unitEntry.IsAlive && unitEntry.Model != null && unitEntry.Transform != null)
			{
				list.Add(unitEntry);
			}
		}
		list.Sort(delegate(CombatUnitEntry left, CombatUnitEntry right)
		{
			if (preferEventTarget)
			{
				bool flag = MatchesModel(left, preferredTarget);
				bool flag2 = MatchesModel(right, preferredTarget);
				if (flag != flag2)
				{
					if (!flag)
					{
						return 1;
					}
					return -1;
				}
			}
			float num = DistanceSquared(sourceEntry, left);
			float value = DistanceSquared(sourceEntry, right);
			return num.CompareTo(value);
		});
		return list;
	}

	/*
	 * ApplyAreaTrigger 처리를 대상에 적용한다.
	 */
	private static bool ApplyAreaTrigger(InGameCombatManager manager /* 전투 진행 관리자 */, CombatUnitEntry sourceEntry /* 효과를 발생시킨 유닛의 등록 정보 */, CombatUnitRegistry roster /* 전투에 등록된 유닛 목록 */, SkillTargetingSpec targeting /* 스킬 대상 선택 규칙 */, Vector2 center /* 효과가 적용될 중심 위치 */, float radius /* 효과가 적용될 반지름 */, bool coverAll /* 범위 안의 모든 대상 포함 여부 */, int maxTargets /* 처리할 수 있는 최대 대상 수 */, float damage /* 적용하거나 전달할 피해량 */, DamageAttribute attribute /* 피해 속성 */, string sourceSkillId /* 효과를 발생시킨 스킬 식별자 */, string damageMeterSourceId /* 피해량 기록에 사용할 발생 원본 식별자 */, UnitCombatState preferredTarget /* 우선 처리할 대상 유닛 */, bool preferEventTarget /* 사건 대상 우선 여부 */, SkillEffectDefinition onHitStatusEffect /* 적중 시 적용할 상태 효과 */, SkillExecutionData onHitData /* 적중 효과에 적용할 스킬 강화 정보 */)
	{
		if (manager == null || sourceEntry == null || roster == null || maxTargets <= 0)
		{
			return false;
		}
		List<CombatUnitEntry> list = OrderedTargets(sourceEntry, roster, targeting, preferredTarget, preferEventTarget);
		if (!coverAll && radius <= 0f)
		{
			CombatUnitEntry unitEntry = (preferEventTarget ? FindPreferredEntry(roster, preferredTarget) : ((list.Count > 0) ? list[0] : null));
			if (unitEntry == null || !unitEntry.IsAlive || unitEntry.Model == null)
			{
				return false;
			}
			InGameResourceChangeResult damageResult = manager.ApplyDamage(unitEntry.Model, damage, attribute, sourceEntry.Model, criticalAllowed: true, 0f, 0f, sourceSkillId, suppressOutgoingDamageTriggers: false, sourceHitWasExecute: false, damageMeterSourceId);
			if (!damageResult.IsDead)
			{
				TryApplyTriggeredOnHitStatusEffect(manager, unitEntry.Model, onHitStatusEffect, onHitData, sourceEntry.Model);
			}
			return true;
		}
		bool result = false;
		int num = 0;
		float num2 = Mathf.Max(0f, radius) * Mathf.Max(0f, radius);
		for (int i = 0; i < list.Count; i++)
		{
			CombatUnitEntry unitEntry2 = list[i];
			if (unitEntry2 != null && unitEntry2.IsAlive && unitEntry2.Model != null && !(unitEntry2.Transform == null) && (coverAll || !(((Vector2)unitEntry2.Transform.position - center).sqrMagnitude > num2)))
			{
				InGameResourceChangeResult damageResult2 = manager.ApplyDamage(unitEntry2.Model, damage, attribute, sourceEntry.Model, criticalAllowed: true, 0f, 0f, sourceSkillId, suppressOutgoingDamageTriggers: false, sourceHitWasExecute: false, damageMeterSourceId);
				if (!damageResult2.IsDead)
				{
					TryApplyTriggeredOnHitStatusEffect(manager, unitEntry2.Model, onHitStatusEffect, onHitData, sourceEntry.Model);
				}
				result = true;
				num++;
				if (num >= maxTargets)
				{
					break;
				}
			}
		}
		return result;
	}

	/*
	 * TryApplyTriggeredOnHitStatusEffect 작업을 시도하고 성공 여부를 반환한다.
	 */
	private static void TryApplyTriggeredOnHitStatusEffect(InGameCombatManager manager /* 전투 진행 관리자 */, UnitCombatState target /* 효과를 받을 대상 유닛 */, SkillEffectDefinition onHitStatusEffect /* 적중 시 적용할 상태 효과 */, SkillExecutionData onHitData /* 적중 효과에 적용할 스킬 강화 정보 */, UnitCombatState source /* 효과를 발생시킨 유닛 */)
	{
		if (manager == null)
		{
			return;
		}
		CombatUnitEntry sourceEntry = manager.UnitRegistry.Find(source);
		SkillExecutionContext context = new SkillExecutionContext(manager, manager.UnitRegistry, sourceEntry, null);
		if (target != null && onHitStatusEffect != null && SkillRequirement.CanRunEffect(context, onHitStatusEffect) && SkillTargeting.MatchesEffectTarget(target, onHitStatusEffect))
		{
			ProjectileStatusHitSpec projectileStatusHitSpec = SkillStatus.EffectStatusSpec(onHitStatusEffect, onHitData);
			if (projectileStatusHitSpec != null && projectileStatusHitSpec.Enabled)
			{
				StatusCombatRules.ApplyStatus(manager, target, projectileStatusHitSpec, source);
			}
		}
	}

	/*
	 * FindPreferredEntry에 해당하는 값을 찾아 반환한다.
	 */
	private static CombatUnitEntry FindPreferredEntry(CombatUnitRegistry roster /* 전투에 등록된 유닛 목록 */, UnitCombatState preferredTarget /* 우선 처리할 대상 유닛 */)
	{
		if (preferredTarget == null || roster == null)
		{
			return null;
		}
		return roster.Find(preferredTarget);
	}

	/*
	 * MatchesModel 조건을 만족하는지 확인한다.
	 */
	private static bool MatchesModel(CombatUnitEntry entry /* 처리할 등록 정보 */, UnitCombatState preferredTarget /* 우선 처리할 대상 유닛 */)
	{
		if (entry != null && preferredTarget != null)
		{
			return entry.Model == preferredTarget;
		}
		return false;
	}

	/*
	 * TrackedAttribute 결과를 계산해 반환한다.
	 */
	private static DamageAttribute TrackedAttribute(SkillTriggerDefinition trigger /* 실행하거나 검사할 트리거 */)
	{
		if (trigger == null)
		{
			return DamageAttribute.Physical;
		}
		if (trigger.TrackedAttribute != DamageAttribute.Physical || trigger.Attribute == DamageAttribute.Physical)
		{
			return trigger.TrackedAttribute;
		}
		return trigger.Attribute;
	}

	/*
	 * IsTargetInsideHitbox 조건을 만족하는지 확인한다.
	 */
	private static bool IsTargetInsideHitbox(Collider2D[] hitboxColliders /* 피격 판정 콜라이더 목록 */, CombatUnitEntry target /* 효과를 받을 대상의 등록 정보 */)
	{
		return UnitHitboxOverlap.IsTargetInsideHitbox(hitboxColliders, target);
	}

	/*
	 * SourceModel 결과를 계산해 반환한다.
	 */
	private static UnitCombatState SourceModel(CombatUnitRegistry roster /* 전투에 등록된 유닛 목록 */, string sourceUnitId /* 발생 원본 유닛 식별자 */, string sourceDefinitionId /* 발생 원본 정의 식별자 */)
	{
		if (roster == null)
		{
			return null;
		}
		IReadOnlyList<CombatUnitEntry> entries = roster.Entries;
		for (int i = 0; i < entries.Count; i++)
		{
			UnitCombatState unitState = ((entries[i] != null) ? entries[i].Model : null);
			UnitIdentity unitIdentity = unitState?.Identity;
			if (unitIdentity != null && !string.IsNullOrWhiteSpace(sourceUnitId) && string.Equals(unitIdentity.UnitId, sourceUnitId, StringComparison.OrdinalIgnoreCase))
			{
				return unitState;
			}
		}
		for (int j = 0; j < entries.Count; j++)
		{
			UnitCombatState unitState2 = ((entries[j] != null) ? entries[j].Model : null);
			UnitIdentity unitIdentity2 = unitState2?.Identity;
			if (unitIdentity2 != null && !string.IsNullOrWhiteSpace(sourceDefinitionId) && string.Equals(unitIdentity2.DefinitionId, sourceDefinitionId, StringComparison.OrdinalIgnoreCase))
			{
				return unitState2;
			}
		}
		return null;
	}

	/*
	 * UnitPosition 결과를 계산해 반환한다.
	 */
	private static Vector2 UnitPosition(CombatUnitRegistry roster /* 전투에 등록된 유닛 목록 */, UnitCombatState model /* 전투 상태를 읽거나 변경할 유닛 */)
	{
		CombatUnitEntry unitEntry = roster?.Find(model);
		if (unitEntry == null || !(unitEntry.Transform != null))
		{
			return Vector2.zero;
		}
		return unitEntry.Transform.position;
	}

	/*
	 * DistanceSquared 결과를 계산해 반환한다.
	 */
	private static float DistanceSquared(CombatUnitEntry sourceEntry /* 효과를 발생시킨 유닛의 등록 정보 */, CombatUnitEntry target /* 효과를 받을 대상의 등록 정보 */)
	{
		if (sourceEntry == null || sourceEntry.Transform == null || target == null || target.Transform == null)
		{
			return float.MaxValue;
		}
		Vector3 vector = target.Transform.position - sourceEntry.Transform.position;
		vector.z = 0f;
		return vector.sqrMagnitude;
	}

	/*
	 * IsPrefabHitboxTrigger 조건을 만족하는지 확인한다.
	 */
	private static bool IsPrefabHitboxTrigger(SkillTriggerDefinition trigger /* 실행하거나 검사할 트리거 */)
	{
		if (trigger != null)
		{
			return trigger.SkillEffectPrefab != null;
		}
		return false;
	}

	/*
	 * IsGlobalHitCount 조건을 만족하는지 확인한다.
	 */
	private static bool IsGlobalHitCount(string rawValue /* 변환 전 원본 문자열 */)
	{
		if (!string.Equals(rawValue, "global", StringComparison.OrdinalIgnoreCase))
		{
			return string.Equals(rawValue, "all", StringComparison.OrdinalIgnoreCase);
		}
		return true;
	}

	/*
	 * ParseHitTargetCount에 필요한 데이터를 읽어 변환한다.
	 */
	private static int ParseHitTargetCount(string rawValue /* 변환 전 원본 문자열 */)
	{
		if (!int.TryParse(rawValue, out var result))
		{
			return 1;
		}
		return Mathf.Max(1, result);
	}

	/*
	 * MatchesRuntimeKind 조건을 만족하는지 확인한다.
	 */
	private static bool MatchesRuntimeKind(SkillDefinition data /* 처리할 실행 데이터 */, SkillRuntimeKind runtimeKind /* 런타임 종류 */)
	{
		switch (runtimeKind)
		{
		case SkillRuntimeKind.MagazineProjectile:
		case SkillRuntimeKind.CooldownProjectile:
			return data is ProjectileSkillDefinition;
		case SkillRuntimeKind.LineAttack:
			return data is LineSkillDefinition;
		case SkillRuntimeKind.SingleAttack:
			return data is SingleSkillDefinition;
		case SkillRuntimeKind.AreaAttack:
		case SkillRuntimeKind.Field:
		case SkillRuntimeKind.Mark:
		case SkillRuntimeKind.Execute:
			return data is ZoneSkillDefinition;
		case SkillRuntimeKind.Buff:
			if (!(data is BuffSkillDefinition))
			{
				return data is SingleChargeSkillDefinition;
			}
			return true;
		case SkillRuntimeKind.Heal:
			return data is BuffSkillDefinition;
		case SkillRuntimeKind.Shield:
			return data is BuffShieldSkillDefinition;
		case SkillRuntimeKind.Passive:
			return data is PassiveSkillDefinition;
		default:
			return false;
		}
	}
}

}
