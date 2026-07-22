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

		/*
		 * TriggerExecutionContext에 필요한 값을 초기화한다.
		 */
		public TriggerExecutionContext(UnitCombatState eventTarget, UnitCombatState attacker, Vector2 eventCenter, StatusRuntimeInstance status, float shieldAbsorbedAmount, float eventAppliedDamage, DamageAttribute eventAttribute, string eventSourceSkillId, UnitCombatState eventSource = null, bool eventWasExecute = false, string eventTriggerSourceSkillId = null)
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
		}
	}

	/*
	 * ExecuteProjectileHit 실행을 처리한다.
	 */
	public static void ExecuteProjectileHit(InGameCombatManager combatManager, CombatUnitRegistry roster, UnitCombatState source, string sourceSkillId, bool isMagazineLastProjectile, Vector2 eventCenter)
	{
		if (isMagazineLastProjectile)
		{
			ExecuteSourceOwnedTriggers(combatManager, roster, source, sourceSkillId, SkillTriggerEvent.OnMagazineLastProjectileHit, new TriggerExecutionContext(source, null, eventCenter, null, 0f, 0f, DamageAttribute.Physical, sourceSkillId, source));
		}
	}

	/*
	 * ExecuteCombatStart 실행을 처리한다.
	 */
	public static void ExecuteCombatStart(InGameCombatManager combatManager, CombatUnitRegistry roster, UnitCombatState source)
	{
		IReadOnlyList<SkillRuntimeInstance> readOnlyList = ((source != null && source.SkillRuntime != null) ? source.SkillRuntime.ActiveSkills : null);
		if (combatManager == null || roster == null || source == null || readOnlyList == null)
		{
			return;
		}
		Vector2 eventCenter = ResolveUnitPosition(roster, source);
		for (int i = 0; i < readOnlyList.Count; i++)
		{
			SkillRuntimeInstance skillRuntimeInstance = readOnlyList[i];
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
	public static void ExecuteShieldExpire(InGameCombatManager combatManager, CombatUnitRegistry roster, UnitCombatState shieldTarget, StatusRuntimeInstance shieldStatus)
	{
		if (shieldTarget != null && shieldStatus != null && shieldStatus.IsShieldStatus)
		{
			UnitCombatState unitState = ResolveSourceModel(roster, shieldStatus.SourceUnitId, shieldStatus.SourceDefinitionId);
			string text = ((!string.IsNullOrWhiteSpace(shieldStatus.SourceSkillId)) ? shieldStatus.SourceSkillId : string.Empty);
			Vector2 eventCenter = ResolveUnitPosition(roster, shieldTarget);
			TriggerExecutionContext triggerContext = new TriggerExecutionContext(shieldTarget, null, eventCenter, shieldStatus, 0f, 0f, DamageAttribute.Physical, text, unitState);
			ExecuteSourceOwnedTriggers(combatManager, roster, unitState, text, SkillTriggerEvent.OnShieldExpire, triggerContext);
			ExecutePassiveOwnerTriggers(combatManager, roster, SkillTriggerEvent.OnShieldExpire, triggerContext);
		}
	}

	/*
	 * ExecuteShieldAbsorb 실행을 처리한다.
	 */
	public static void ExecuteShieldAbsorb(InGameCombatManager combatManager, CombatUnitRegistry roster, UnitCombatState shieldTarget, UnitCombatState attacker, StatusRuntimeInstance shieldStatus, float absorbedAmount)
	{
		if (shieldTarget != null && shieldStatus != null && shieldStatus.IsShieldStatus && !(absorbedAmount <= 0f))
		{
			UnitCombatState unitState = ResolveSourceModel(roster, shieldStatus.SourceUnitId, shieldStatus.SourceDefinitionId);
			string text = ((!string.IsNullOrWhiteSpace(shieldStatus.SourceSkillId)) ? shieldStatus.SourceSkillId : string.Empty);
			Vector2 eventCenter = ((attacker != null) ? ResolveUnitPosition(roster, attacker) : ResolveUnitPosition(roster, shieldTarget));
			TriggerExecutionContext triggerContext = new TriggerExecutionContext(attacker, attacker, eventCenter, shieldStatus, absorbedAmount, 0f, DamageAttribute.Physical, text, unitState);
			ExecuteSourceOwnedTriggers(combatManager, roster, unitState, text, SkillTriggerEvent.OnShieldAbsorb, triggerContext);
			ExecutePassiveOwnerTriggers(combatManager, roster, SkillTriggerEvent.OnShieldAbsorb, triggerContext);
		}
	}

	/*
	 * ExecuteShieldAbsorbs 실행을 처리한다.
	 */
	public static void ExecuteShieldAbsorbs(InGameCombatManager combatManager, CombatUnitRegistry roster, UnitCombatState shieldTarget, UnitCombatState attacker, IReadOnlyList<ShieldAbsorptionRecord> absorbedShields)
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
	public static void ExecuteStatusExpire(InGameCombatManager combatManager, CombatUnitRegistry roster, UnitCombatState statusOwner, StatusRuntimeInstance status)
	{
		if (statusOwner != null && status != null)
		{
			UnitCombatState unitState = ResolveSourceModel(roster, status.SourceUnitId, status.SourceDefinitionId);
			string text = ((!string.IsNullOrWhiteSpace(status.SourceSkillId)) ? status.SourceSkillId : string.Empty);
			Vector2 eventCenter = ResolveUnitPosition(roster, statusOwner);
			TriggerExecutionContext triggerContext = new TriggerExecutionContext(statusOwner, null, eventCenter, status, 0f, 0f, DamageAttribute.Physical, text, unitState);
			ExecuteSourceOwnedTriggers(combatManager, roster, unitState, text, SkillTriggerEvent.OnStatusExpire, triggerContext);
			ExecutePassiveOwnerTriggers(combatManager, roster, SkillTriggerEvent.OnStatusExpire, triggerContext);
		}
	}

	/*
	 * ExecuteExpiredStatuses 실행을 처리한다.
	 */
	public static void ExecuteExpiredStatuses(InGameCombatManager combatManager, CombatUnitRegistry roster, UnitCombatState statusOwner, IReadOnlyList<StatusRuntimeInstance> removedStatuses)
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
	public static void ExecuteShieldExpires(InGameCombatManager combatManager, CombatUnitRegistry roster, UnitCombatState shieldTarget, IReadOnlyList<StatusRuntimeInstance> removedStatuses)
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
	public static void ExecuteOutgoingDamage(InGameCombatManager combatManager, CombatUnitRegistry roster, UnitCombatState source, string sourceSkillId, UnitCombatState eventTarget, DamageAttribute attribute, float eventAppliedDamage, bool eventWasExecute = false)
	{
		if (!(combatManager == null) && roster != null && source != null)
		{
			Vector2 eventCenter = ((eventTarget != null) ? ResolveUnitPosition(roster, eventTarget) : ResolveUnitPosition(roster, source));
			TriggerExecutionContext triggerContext = new TriggerExecutionContext(eventTarget, null, eventCenter, null, 0f, eventAppliedDamage, attribute, sourceSkillId, source, eventWasExecute);
			ExecuteSourceOwnedTriggers(combatManager, roster, source, sourceSkillId, SkillTriggerEvent.OnOutgoingDamage, triggerContext);
			ExecutePassiveOwnerTriggers(combatManager, roster, SkillTriggerEvent.OnOutgoingDamage, triggerContext);
		}
	}

	/*
	 * ExecuteSkillCast 실행을 처리한다.
	 */
	public static void ExecuteSkillCast(InGameCombatManager combatManager, CombatUnitRegistry roster, UnitCombatState source, string sourceSkillId, Vector2 eventCenter, string eventTriggerSourceSkillId = null)
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
	public static void ExecuteKill(InGameCombatManager combatManager, CombatUnitRegistry roster, UnitCombatState source, string sourceSkillId, UnitCombatState eventTarget, DamageAttribute attribute, float eventAppliedDamage, bool eventWasExecute = false)
	{
		if (!(combatManager == null) && roster != null && source != null)
		{
			Vector2 eventCenter = ((eventTarget != null) ? ResolveUnitPosition(roster, eventTarget) : ResolveUnitPosition(roster, source));
			TriggerExecutionContext triggerContext = new TriggerExecutionContext(eventTarget, source, eventCenter, null, 0f, eventAppliedDamage, attribute, sourceSkillId, source, eventWasExecute);
			ExecuteSourceOwnedTriggers(combatManager, roster, source, sourceSkillId, SkillTriggerEvent.OnKill, triggerContext);
			ExecutePassiveOwnerTriggers(combatManager, roster, SkillTriggerEvent.OnKill, triggerContext);
		}
	}

	/*
	 * ExecuteSourceOwnedTriggers 실행을 처리한다.
	 */
	private static void ExecuteSourceOwnedTriggers(InGameCombatManager combatManager, CombatUnitRegistry roster, UnitCombatState source, string sourceSkillId, SkillTriggerEvent triggerEvent, TriggerExecutionContext triggerContext)
	{
		if (combatManager == null || roster == null || source == null || string.IsNullOrWhiteSpace(sourceSkillId))
		{
			return;
		}
		string id = ((source.Identity != null) ? source.Identity.DefinitionId : string.Empty);
		MonsterDefinition monsterDefinition = GameDataLoader.CurrentCatalog.ResolveMonster(id);
		SkillTriggerDefinition[] array = ResolveSourceOwnedPlanTriggers(source, sourceSkillId, (monsterDefinition != null) ? monsterDefinition.SkillTriggers : null);
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
	 * ResolveSourceOwnedPlanTriggers 결과를 계산해 반환한다.
	 */
	private static SkillTriggerDefinition[] ResolveSourceOwnedPlanTriggers(UnitCombatState source, string sourceSkillId, SkillTriggerDefinition[] fallbackTriggers)
	{
		return SkillNodeAction.ResolveTriggers((source != null && source.SkillRuntime != null) ? source.SkillRuntime.FindBySkillId(sourceSkillId) : null, fallbackTriggers);
	}

	/*
	 * ExecutePassiveOwnerTriggers 실행을 처리한다.
	 */
	private static void ExecutePassiveOwnerTriggers(InGameCombatManager combatManager, CombatUnitRegistry roster, SkillTriggerEvent triggerEvent, TriggerExecutionContext triggerContext)
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
			if (unitEntry == null || unitState == null || unitState.SkillProgress == null || unitState.SkillProgress.LearnedPassiveSkillIds.Count == 0)
			{
				continue;
			}
			string id = ((unitState.Identity != null) ? unitState.Identity.DefinitionId : string.Empty);
			MonsterDefinition monsterDefinition = GameDataLoader.CurrentCatalog.ResolveMonster(id);
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
	private static bool ShouldRunSourceOwnedTrigger(SkillTriggerDefinition trigger, UnitCombatState source, string sourceSkillId, SkillTriggerEvent triggerEvent, TriggerExecutionContext triggerContext)
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
	private static bool ShouldRunPassiveOwnerTrigger(SkillTriggerDefinition trigger, UnitCombatState owner, SkillTriggerEvent triggerEvent, TriggerExecutionContext triggerContext)
	{
		if (trigger == null || owner == null || owner.SkillProgress == null || trigger.TriggerEvent != triggerEvent || string.IsNullOrWhiteSpace(trigger.SourceSkillId) || !owner.SkillProgress.LearnedPassiveSkillIds.Contains(trigger.SourceSkillId) || !MatchesEventSkillId(trigger.EventSkillId, triggerContext.EventSourceSkillId) || !StatusConditionRules.MatchesSkillRuntimeKinds(trigger.EventSkillRuntimeKindValues, triggerContext.EventSourceSkillId) || (trigger.RequireEventExecute && !triggerContext.EventWasExecute) || !HasAllChoices(owner, trigger.RequiresActiveChoiceId) || HasAnyChoice(owner, trigger.ExcludesActiveChoiceId) || !MeetsSourceStatusRequirement(owner, trigger.RequiredSourceStatusKind, trigger.RequiredSourceStatusMinStacks))
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
	private static bool HasAllChoices(UnitCombatState source, string choiceList)
	{
		if (string.IsNullOrWhiteSpace(choiceList))
		{
			return true;
		}
		if (source == null || source.SkillProgress == null)
		{
			return false;
		}
		string[] array = choiceList.Split(';', ',');
		for (int i = 0; i < array.Length; i++)
		{
			string text = ((array[i] != null) ? array[i].Trim() : string.Empty);
			if (!string.IsNullOrWhiteSpace(text) && !source.SkillProgress.ChosenChoiceIds.Contains(text))
			{
				return false;
			}
		}
		return true;
	}

	/*
	 * HasAnyChoice 조건을 만족하는지 확인한다.
	 */
	private static bool HasAnyChoice(UnitCombatState source, string choiceList)
	{
		if (string.IsNullOrWhiteSpace(choiceList) || source == null || source.SkillProgress == null)
		{
			return false;
		}
		string[] array = choiceList.Split(';', ',');
		for (int i = 0; i < array.Length; i++)
		{
			string text = ((array[i] != null) ? array[i].Trim() : string.Empty);
			if (!string.IsNullOrWhiteSpace(text) && source.SkillProgress.ChosenChoiceIds.Contains(text))
			{
				return true;
			}
		}
		return false;
	}

	/*
	 * MeetsSourceStatusRequirement 조건을 만족하는지 확인한다.
	 */
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

	/*
	 * MatchesConditionStatus 조건을 만족하는지 확인한다.
	 */
	private static bool MatchesConditionStatus(SkillTriggerDefinition trigger, StatusRuntimeInstance status)
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
	private static bool MatchesTriggerAttribute(string rawAttribute, DamageAttribute eventAttribute)
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
	private static bool PassesProcGate(InGameCombatManager combatManager, UnitCombatState owner, SkillTriggerDefinition trigger)
	{
		if (combatManager == null || owner == null || trigger == null)
		{
			return false;
		}
		float num = SkillUpgrade.ResolvePassiveChoices(owner, trigger.SourceSkillId).ResolveTriggerProcChanceBonus(trigger.TriggerId);
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
	private static bool PassesCountGate(InGameCombatManager combatManager, UnitCombatState owner, SkillTriggerDefinition trigger)
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
	private static bool MatchesEventSourceScope(string scope, UnitCombatState owner, UnitCombatState eventSource)
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
	private static bool MatchesEventSkillId(string rawSkillIds, string eventSkillId)
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

	/*
	 * IsSameUnit 조건을 만족하는지 확인한다.
	 */
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

	/*
	 * BuildPassiveTriggerCooldownKey에 필요한 결과를 만들어 반환한다.
	 */
	private static string BuildPassiveTriggerCooldownKey(UnitCombatState owner, SkillTriggerDefinition trigger)
	{
		string obj = ((owner != null && owner.Identity != null && !string.IsNullOrWhiteSpace(owner.Identity.UnitId)) ? owner.Identity.UnitId : ((owner != null) ? owner.GetHashCode().ToString() : "unknown"));
		string text = ((trigger != null && !string.IsNullOrWhiteSpace(trigger.TriggerId)) ? trigger.TriggerId : ((trigger != null) ? trigger.SourceSkillId : "unknown"));
		return obj + ":" + text;
	}

	/*
	 * ExecuteTrigger 실행을 처리한다.
	 */
	private static void ExecuteTrigger(InGameCombatManager combatManager, CombatUnitRegistry roster, CombatUnitEntry sourceEntry, UnitCombatState source, SkillTriggerDefinition trigger, TriggerExecutionContext triggerContext)
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
	private static IEnumerator ExecuteDelayed(InGameCombatManager combatManager, CombatUnitRegistry roster, CombatUnitEntry sourceEntry, UnitCombatState source, SkillTriggerDefinition trigger, TriggerExecutionContext triggerContext, float delaySeconds)
	{
		yield return new WaitForSeconds(Mathf.Max(0f, delaySeconds));
		ExecuteOnce(combatManager, roster, sourceEntry, source, trigger, triggerContext);
	}

	/*
	 * ExecuteOnce 실행을 처리한다.
	 */
	private static void ExecuteOnce(InGameCombatManager combatManager, CombatUnitRegistry roster, CombatUnitEntry sourceEntry, UnitCombatState source, SkillTriggerDefinition trigger, TriggerExecutionContext triggerContext)
	{
		SkillNodeAction.ExecuteTriggerAction(combatManager, roster, sourceEntry, source, trigger, triggerContext);
	}

	/*
	 * ExecuteTriggeredSkillAction 실행 결과를 반환한다.
	 */
	internal static bool ExecuteTriggeredSkillAction(InGameCombatManager combatManager, CombatUnitEntry sourceEntry, SkillTriggerDefinition trigger, TriggerExecutionContext triggerContext)
	{
		if (combatManager == null || sourceEntry == null || trigger == null || sourceEntry.Model == null || sourceEntry.Model.SkillRuntime == null || string.IsNullOrWhiteSpace(trigger.TriggeredSkillId))
		{
			return false;
		}
		SkillRuntimeInstance skillRuntimeInstance = sourceEntry.Model.SkillRuntime.FindBySkillId(trigger.TriggeredSkillId);
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
	internal static bool ExecuteEffectAction(InGameCombatManager combatManager, CombatUnitRegistry roster, CombatUnitEntry sourceEntry, SkillTriggerDefinition trigger, TriggerExecutionContext triggerContext)
	{
		if (combatManager == null || roster == null || sourceEntry == null || trigger == null || string.IsNullOrWhiteSpace(trigger.TriggeredEffectId))
		{
			return false;
		}
		SkillEffectDefinition skillEffectDefinition = ResolveTriggeredEffect(sourceEntry.Model, trigger.TriggeredEffectId);
		if (skillEffectDefinition == null)
		{
			return false;
		}
		SkillExecutionContext context = new SkillExecutionContext(combatManager, roster, sourceEntry, null, triggerContext.EventTarget);
		SkillSnapshot snapshot = SkillUpgrade.ResolvePassiveChoices(sourceEntry.Model, trigger.SourceSkillId);
		return SkillEffect.ExecuteDirect(context, snapshot, skillEffectDefinition, triggerContext.EventCenter);
	}

	/*
	 * ResolveTriggeredEffect 결과를 계산해 반환한다.
	 */
	private static SkillEffectDefinition ResolveTriggeredEffect(UnitCombatState source, string effectId)
	{
		if (source == null || source.Identity == null || string.IsNullOrWhiteSpace(effectId))
		{
			return null;
		}
		MonsterDefinition monsterDefinition = GameDataLoader.CurrentCatalog.ResolveMonster(source.Identity.DefinitionId);
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
	private static SkillEffectDefinition FindEffect(SkillDefinition[] skills, string effectId)
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
	private static SkillEffectDefinition FindEffect(PassiveDefinition[] skills, string effectId)
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
	private static SkillEffectDefinition FindEffect(SkillEffectDefinition[] effects, string effectId)
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
	internal static bool ReduceTargetCooldownAction(CombatUnitRegistry roster, CombatUnitEntry sourceEntry, SkillTriggerDefinition trigger)
	{
		if (trigger == null || trigger.CooldownRefundRatio <= 0f)
		{
			return false;
		}
		List<SkillRuntimeInstance> list = ResolveTargetRuntimes(roster, sourceEntry, trigger);
		bool flag = false;
		for (int i = 0; i < list.Count; i++)
		{
			SkillRuntimeInstance skillRuntimeInstance = list[i];
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
	internal static bool ReduceTargetReloadAction(CombatUnitRegistry roster, CombatUnitEntry sourceEntry, SkillTriggerDefinition trigger)
	{
		if (trigger == null || trigger.ReloadReduceRatio <= 0f)
		{
			return false;
		}
		List<SkillRuntimeInstance> list = ResolveTargetRuntimes(roster, sourceEntry, trigger);
		bool flag = false;
		for (int i = 0; i < list.Count; i++)
		{
			SkillRuntimeInstance skillRuntimeInstance = list[i];
			if (skillRuntimeInstance != null)
			{
				flag = skillRuntimeInstance.ReduceReloadRemaining(skillRuntimeInstance.ReloadDuration * Mathf.Clamp01(trigger.ReloadReduceRatio)) || flag;
			}
		}
		return flag;
	}

	/*
	 * ResolveTargetRuntimes 결과를 계산해 반환한다.
	 */
	private static List<SkillRuntimeInstance> ResolveTargetRuntimes(CombatUnitRegistry roster, CombatUnitEntry sourceEntry, SkillTriggerDefinition trigger)
	{
		List<SkillRuntimeInstance> list = new List<SkillRuntimeInstance>();
		List<CombatUnitEntry> list2 = ResolveCooldownTargetEntries(roster, sourceEntry, trigger);
		string text = ((trigger != null && !string.IsNullOrWhiteSpace(trigger.TargetSkillId)) ? trigger.TargetSkillId : ((trigger != null) ? trigger.TriggeredSkillId : string.Empty));
		for (int i = 0; i < list2.Count; i++)
		{
			CombatUnitEntry unitEntry = list2[i];
			UnitSkillRuntimeSet unitSkillRuntimeSet = ((unitEntry != null && unitEntry.Model != null) ? unitEntry.Model.SkillRuntime : null);
			if (unitSkillRuntimeSet == null)
			{
				continue;
			}
			if (!string.IsNullOrWhiteSpace(text))
			{
				SkillRuntimeInstance skillRuntimeInstance = unitSkillRuntimeSet.FindBySkillId(text);
				if (skillRuntimeInstance != null)
				{
					list.Add(skillRuntimeInstance);
				}
				continue;
			}
			IReadOnlyList<SkillRuntimeInstance> activeSkills = unitSkillRuntimeSet.ActiveSkills;
			int num = 0;
			while (activeSkills != null && num < activeSkills.Count)
			{
				SkillRuntimeInstance skillRuntimeInstance2 = activeSkills[num];
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
	 * ResolveCooldownTargetEntries 결과를 계산해 반환한다.
	 */
	private static List<CombatUnitEntry> ResolveCooldownTargetEntries(CombatUnitRegistry roster, CombatUnitEntry sourceEntry, SkillTriggerDefinition trigger)
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
	internal static bool ExecuteSingleAttackAction(InGameCombatManager combatManager, CombatUnitRegistry roster, CombatUnitEntry sourceEntry, UnitCombatState source, SkillTriggerDefinition trigger, TriggerExecutionContext triggerContext)
	{
		if (combatManager == null || roster == null || sourceEntry == null)
		{
			return false;
		}
		SkillTargetingSpec targeting = BuildTargeting(trigger);
		Vector2 vector = ResolveCenter(sourceEntry, roster, triggerContext, trigger, targeting);
		float num = ResolveDamage(source, trigger, triggerContext);
		if (num <= 0f)
		{
			return false;
		}
		string sourceSkillId = ResolveTriggeredDamageSourceSkillId(trigger);
		SkillEffectDefinition onHitStatusEffect = ResolveTriggeredOnHitStatusEffect(source, trigger);
		SkillSnapshot onHitSnapshot = SkillUpgrade.ResolveActiveChoices(source, trigger.SourceSkillId);
		RuntimeSkillVisualSpec runtimeVisual = trigger.RuntimeVisual;
		bool flag = EffectManager.HasVisual(runtimeVisual);
		bool flag2 = runtimeVisual != null && runtimeVisual.Hitbox != null && runtimeVisual.Hitbox.HasHitbox();
		if ((flag2 || IsPrefabHitboxTrigger(trigger)) && combatManager.Effects != null)
		{
			GameObject gameObject = (flag2 ? combatManager.Effects.CreateRuntimeVisual(runtimeVisual, string.IsNullOrWhiteSpace(trigger.TriggerId) ? "RuntimeTriggerHitbox" : ("RuntimeTriggerHitbox_" + trigger.TriggerId), vector, Quaternion.identity) : combatManager.Effects.InstantiateSkillPrefab(trigger.SkillEffectPrefab, vector, Quaternion.identity));
			if (gameObject == null)
			{
				return false;
			}
			Physics2D.SyncTransforms();
			bool result = ApplyPrefabHitbox(combatManager, sourceEntry, roster, targeting, gameObject, IsGlobalHitCount(trigger.HitTargetCount) ? int.MaxValue : ParseHitTargetCount(trigger.HitTargetCount), num, trigger.Attribute, sourceSkillId, trigger.TriggerId, triggerContext.EventTarget, onHitStatusEffect, onHitSnapshot);
			UnityEngine.Object.Destroy(gameObject, 1f);
			return result;
		}
		bool flag3 = ApplyAreaTrigger(combatManager, sourceEntry, roster, targeting, vector, Mathf.Max(0f, trigger.Radius), trigger.CoverAll || trigger.TargetShape == SkillMultiEffectTargetShape.Battlefield, IsGlobalHitCount(trigger.HitTargetCount) ? int.MaxValue : ParseHitTargetCount(trigger.HitTargetCount), num, trigger.Attribute, sourceSkillId, trigger.TriggerId, triggerContext.EventTarget, trigger.TargetSelection == SkillMultiEffectTargetSelection.EventTarget, onHitStatusEffect, onHitSnapshot);
		string visualName = "RuntimeTriggerVisual";
		if (!string.IsNullOrWhiteSpace(trigger.TriggerId))
		{
			visualName = "RuntimeTriggerVisual_" + trigger.TriggerId;
		}
		if (flag3 && flag && combatManager.Effects != null)
		{
			combatManager.Effects.SpawnTransient(runtimeVisual, null, visualName, vector, Quaternion.identity, 1f);
		}
		else if (flag3 && trigger.SkillEffectPrefab != null && combatManager.Effects != null)
		{
			combatManager.Effects.SpawnTransient(null, trigger.SkillEffectPrefab, visualName, vector, Quaternion.identity, 1f);
		}
		return flag3;
	}

	/*
	 * ExecuteLineAttackAction 실행 결과를 반환한다.
	 */
	internal static bool ExecuteLineAttackAction(InGameCombatManager combatManager, CombatUnitRegistry roster, CombatUnitEntry sourceEntry, UnitCombatState source, SkillTriggerDefinition trigger, TriggerExecutionContext triggerContext)
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
		float num = ResolveDamage(source, trigger, triggerContext);
		if (num <= 0f)
		{
			return false;
		}
		SkillSnapshot skillExecutionSnapshot = SkillUpgrade.ResolveActiveChoices(source, trigger.SourceSkillId);
		SkillEffectDefinition skillEffectDefinition = ResolveTriggeredOnHitStatusEffect(source, trigger);
		SkillEffectDefinition[] onHitEffects = ((skillEffectDefinition == null) ? Array.Empty<SkillEffectDefinition>() : new SkillEffectDefinition[1] { skillEffectDefinition });
		float num2 = ResolveTriggeredLineLength();
		float num3 = Mathf.Max(0.1f, trigger.Radius);
		Vector2 vector3 = vector + vector2 * (num2 * 0.5f);
		RuntimeSkillVisualSpec visual = trigger.RuntimeVisual;
		bool num4 = EffectManager.HasVisual(visual);
		EffectManager effects = combatManager.Effects;
		GameObject gameObject = null;
		if (num4 && effects != null)
		{
			string visualName = "RuntimeTriggerLineVisual";
			if (!string.IsNullOrWhiteSpace(trigger.TriggerId))
			{
				visualName = "RuntimeTriggerLineVisual_" + trigger.TriggerId;
			}
			gameObject = effects.CreateRuntimeVisual(visual, visualName, vector3, EffectManager.ResolveRotation(vector2));
		}
		else if (trigger.SkillEffectPrefab != null && effects != null)
		{
			gameObject = effects.InstantiateSkillPrefab(trigger.SkillEffectPrefab, vector3, EffectManager.ResolveRotation(vector2));
		}
		if (gameObject != null)
		{
			ConfigureTriggeredLineVisual(gameObject.transform, num2, num3);
			effects.DestroyAfterAnimation(gameObject, 0.1f);
		}
		return LineSkillActor.ApplyLineTick(combatManager, sourceEntry, roster, skillTargetingSpec, vector, vector2, num2, num3, 0f, num, trigger.Attribute, null, onHitEffects, null, skillExecutionSnapshot, source, ResolveTriggeredDamageSourceSkillId(trigger), criticalAllowed: true, skillExecutionSnapshot?.CritChanceBonus ?? 0f, skillExecutionSnapshot?.CritDamageBonus ?? 0f, null, null, trigger.TriggerId);
	}

	/*
	 * ResolveTriggeredOnHitStatusEffect 결과를 계산해 반환한다.
	 */
	private static SkillEffectDefinition ResolveTriggeredOnHitStatusEffect(UnitCombatState source, SkillTriggerDefinition trigger)
	{
		if (source == null || trigger == null || string.IsNullOrWhiteSpace(trigger.TriggeredEffectId))
		{
			return null;
		}
		SkillEffectDefinition skillEffectDefinition = ResolveTriggeredEffect(source, trigger.TriggeredEffectId);
		if (skillEffectDefinition == null || skillEffectDefinition.EffectKind != SkillMultiEffectKind.Status || skillEffectDefinition.EffectTiming != SkillMultiEffectTiming.OnHit || skillEffectDefinition.TargetSide != SkillMultiEffectTargetSide.Enemy)
		{
			return null;
		}
		return skillEffectDefinition;
	}

	/*
	 * ResolveTriggeredDamageSourceSkillId 결과를 계산해 반환한다.
	 */
	private static string ResolveTriggeredDamageSourceSkillId(SkillTriggerDefinition trigger)
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
	 * ResolveTriggeredLineLength 결과를 계산해 반환한다.
	 */
	private static float ResolveTriggeredLineLength()
	{
		return 31f;
	}

	/*
	 * ConfigureTriggeredLineVisual에 필요한 값을 설정한다.
	 */
	private static void ConfigureTriggeredLineVisual(Transform transform, float length, float width)
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
	private static SkillTargetingSpec BuildTargeting(SkillTriggerDefinition trigger)
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
	 * ResolveCenter 결과를 계산해 반환한다.
	 */
	private static Vector2 ResolveCenter(CombatUnitEntry sourceEntry, CombatUnitRegistry roster, TriggerExecutionContext triggerContext, SkillTriggerDefinition trigger, SkillTargetingSpec targeting)
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
				return ResolveUnitPosition(roster, triggerContext.EventTarget);
			}
		}
		return triggerContext.EventCenter;
	}

	/*
	 * ResolveDamage 결과를 계산해 반환한다.
	 */
	private static float ResolveDamage(UnitCombatState source, SkillTriggerDefinition trigger, TriggerExecutionContext triggerContext)
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
			return Mathf.Max(0f, (triggerContext.Status != null) ? triggerContext.Status.GetTrackedIncomingDamage(ResolveTrackedAttribute(trigger)) : 0f) * Mathf.Max(0f, trigger.DamageSourceMultiplier) * Mathf.Max(0f, trigger.DamageMultiplier);
		case SkillTriggerDamageSource.EventAppliedDamage:
			return Mathf.Max(0f, triggerContext.EventAppliedDamage) * Mathf.Max(0f, trigger.DamageSourceMultiplier) * Mathf.Max(0f, trigger.DamageMultiplier);
		default:
		{
			bool flag = Mathf.Abs(trigger.SpellPowerCoefficient) >= Mathf.Abs(trigger.AttackPowerCoefficient);
			SkillDamageSpec damage = new SkillDamageSpec
			{
				SkillId = trigger.SourceSkillId,
				Element = trigger.Attribute,
				BaseDamage = trigger.BaseDamage,
				StatCoefficient = (flag ? trigger.SpellPowerCoefficient : trigger.AttackPowerCoefficient),
				StatSource = (flag ? StatSource.Intelligence : StatSource.Attack),
				CriticalAllowed = true
			};
			return DamageCalculator.ResolveDamage(source, damage, null) * Mathf.Max(0f, trigger.DamageMultiplier);
		}
		}
	}

	/*
	 * ApplyPrefabHitbox 처리를 대상에 적용한다.
	 */
	private static bool ApplyPrefabHitbox(InGameCombatManager manager, CombatUnitEntry sourceEntry, CombatUnitRegistry roster, SkillTargetingSpec targeting, GameObject hitboxObject, int maxTargets, float damage, DamageAttribute attribute, string sourceSkillId, string damageMeterSourceId, UnitCombatState preferredTarget, SkillEffectDefinition onHitStatusEffect, SkillSnapshot onHitSnapshot)
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
		List<CombatUnitEntry> list = ResolveOrderedTargets(sourceEntry, roster, targeting, preferredTarget, preferredTarget != null);
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
					TryApplyTriggeredOnHitStatusEffect(manager, unitEntry.Model, onHitStatusEffect, onHitSnapshot, sourceEntry.Model);
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
	 * ResolveOrderedTargets 결과를 계산해 반환한다.
	 */
	private static List<CombatUnitEntry> ResolveOrderedTargets(CombatUnitEntry sourceEntry, CombatUnitRegistry roster, SkillTargetingSpec targeting, UnitCombatState preferredTarget, bool preferEventTarget)
	{
		IReadOnlyList<CombatUnitEntry> readOnlyList = SkillTargeting.ResolveTargetList(sourceEntry, roster, targeting);
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
			float num = ResolveDistanceSquared(sourceEntry, left);
			float value = ResolveDistanceSquared(sourceEntry, right);
			return num.CompareTo(value);
		});
		return list;
	}

	/*
	 * ApplyAreaTrigger 처리를 대상에 적용한다.
	 */
	private static bool ApplyAreaTrigger(InGameCombatManager manager, CombatUnitEntry sourceEntry, CombatUnitRegistry roster, SkillTargetingSpec targeting, Vector2 center, float radius, bool coverAll, int maxTargets, float damage, DamageAttribute attribute, string sourceSkillId, string damageMeterSourceId, UnitCombatState preferredTarget, bool preferEventTarget, SkillEffectDefinition onHitStatusEffect, SkillSnapshot onHitSnapshot)
	{
		if (manager == null || sourceEntry == null || roster == null || maxTargets <= 0)
		{
			return false;
		}
		List<CombatUnitEntry> list = ResolveOrderedTargets(sourceEntry, roster, targeting, preferredTarget, preferEventTarget);
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
				TryApplyTriggeredOnHitStatusEffect(manager, unitEntry.Model, onHitStatusEffect, onHitSnapshot, sourceEntry.Model);
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
					TryApplyTriggeredOnHitStatusEffect(manager, unitEntry2.Model, onHitStatusEffect, onHitSnapshot, sourceEntry.Model);
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
	private static void TryApplyTriggeredOnHitStatusEffect(InGameCombatManager manager, UnitCombatState target, SkillEffectDefinition onHitStatusEffect, SkillSnapshot onHitSnapshot, UnitCombatState source)
	{
		if (!(manager == null) && target != null && onHitStatusEffect != null && SkillEffect.ShouldRun(new SkillExecutionContext(manager, null, null, null), onHitStatusEffect, onHitSnapshot) && SkillEffect.TargetMatchesCondition(target, onHitStatusEffect))
		{
			ProjectileStatusHitSpec projectileStatusHitSpec = SkillEffect.ResolveStatusSpec(onHitStatusEffect, onHitSnapshot);
			if (projectileStatusHitSpec != null && projectileStatusHitSpec.Enabled)
			{
				StatusCombatRules.ApplyStatus(manager, target, projectileStatusHitSpec, source);
			}
		}
	}

	/*
	 * FindPreferredEntry에 해당하는 값을 찾아 반환한다.
	 */
	private static CombatUnitEntry FindPreferredEntry(CombatUnitRegistry roster, UnitCombatState preferredTarget)
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
	private static bool MatchesModel(CombatUnitEntry entry, UnitCombatState preferredTarget)
	{
		if (entry != null && preferredTarget != null)
		{
			return entry.Model == preferredTarget;
		}
		return false;
	}

	/*
	 * ResolveTrackedAttribute 결과를 계산해 반환한다.
	 */
	private static DamageAttribute ResolveTrackedAttribute(SkillTriggerDefinition trigger)
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
	private static bool IsTargetInsideHitbox(Collider2D[] hitboxColliders, CombatUnitEntry target)
	{
		return UnitHitboxOverlap.IsTargetInsideHitbox(hitboxColliders, target);
	}

	/*
	 * ResolveSourceModel 결과를 계산해 반환한다.
	 */
	private static UnitCombatState ResolveSourceModel(CombatUnitRegistry roster, string sourceUnitId, string sourceDefinitionId)
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
	 * ResolveUnitPosition 결과를 계산해 반환한다.
	 */
	private static Vector2 ResolveUnitPosition(CombatUnitRegistry roster, UnitCombatState model)
	{
		CombatUnitEntry unitEntry = roster?.Find(model);
		if (unitEntry == null || !(unitEntry.Transform != null))
		{
			return Vector2.zero;
		}
		return unitEntry.Transform.position;
	}

	/*
	 * ResolveDistanceSquared 결과를 계산해 반환한다.
	 */
	private static float ResolveDistanceSquared(CombatUnitEntry sourceEntry, CombatUnitEntry target)
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
	private static bool IsPrefabHitboxTrigger(SkillTriggerDefinition trigger)
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
	private static bool IsGlobalHitCount(string rawValue)
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
	private static int ParseHitTargetCount(string rawValue)
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
	private static bool MatchesRuntimeKind(SkillRuntimeData data, SkillRuntimeKind runtimeKind)
	{
		switch (runtimeKind)
		{
		case SkillRuntimeKind.MagazineProjectile:
		case SkillRuntimeKind.CooldownProjectile:
			return data is ProjectileSkillRuntimeData;
		case SkillRuntimeKind.LineAttack:
			return data is LineSkillRuntimeData;
		case SkillRuntimeKind.SingleAttack:
			return data is SingleSkillRuntimeData;
		case SkillRuntimeKind.AreaAttack:
		case SkillRuntimeKind.Field:
		case SkillRuntimeKind.Mark:
		case SkillRuntimeKind.Execute:
			return data is ZoneSkillRuntimeData;
		case SkillRuntimeKind.Buff:
			if (!(data is BuffSkillRuntimeData))
			{
				return data is SingleChargeSkillRuntimeData;
			}
			return true;
		case SkillRuntimeKind.Heal:
			return data is BuffSkillRuntimeData;
		case SkillRuntimeKind.Shield:
			return data is BuffShieldSkillRuntimeData;
		case SkillRuntimeKind.Passive:
			return data is PassiveSkillRuntimeData;
		default:
			return false;
		}
	}
}

}
