using System;
using System.Collections.Generic;
using Pakuri.Combat;
using AttributeDefenseSet = Pakuri.Combat.DamageCalculator.AttributeDefenseSet;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Pakuri.InGame
{
    public readonly struct DamageApplicationOptions
    {
        public DamageApplicationOptions(
            BaseUnitRuntimeModel source,
            bool criticalAllowed = false,
            float critChanceBonus = 0f,
            float critDamageBonus = 0f,
            string sourceSkillId = null,
            bool suppressOutgoingDamageTriggers = false,
            bool sourceHitWasExecute = false)
        {
            Source = source;
            CriticalAllowed = criticalAllowed;
            CritChanceBonus = critChanceBonus;
            CritDamageBonus = critDamageBonus;
            SourceSkillId = sourceSkillId;
            SuppressOutgoingDamageTriggers = suppressOutgoingDamageTriggers;
            SourceHitWasExecute = sourceHitWasExecute;
        }

        public BaseUnitRuntimeModel Source { get; }
        public bool CriticalAllowed { get; }
        public float CritChanceBonus { get; }
        public float CritDamageBonus { get; }
        public string SourceSkillId { get; }
        public bool SuppressOutgoingDamageTriggers { get; }
        public bool SourceHitWasExecute { get; }
    }

    [DisallowMultipleComponent]
    public sealed class InGameCombatManager : MonoBehaviour
    {
        private const float PassiveEffectRefreshInterval = 0.25f;

        private readonly UnitRosterService roster = new UnitRosterService();
        private readonly EnemyCombatSystem enemyCombatSystem = new EnemyCombatSystem();
        private readonly UnitResourceMutationService resourceMutations = new UnitResourceMutationService();
        private readonly SkillExecutionSystem skillExecution = new SkillExecutionSystem();
        private readonly Dictionary<string, GameObject> statusEffectVisuals = new Dictionary<string, GameObject>();
        private readonly HashSet<string> appliedOneShotPassiveEffects = new HashSet<string>();
        private readonly Dictionary<string, float> passiveTriggerCooldowns = new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, int> passiveTriggerCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        private float passiveEffectRefreshRemaining;
        private bool hasLatchedManualProjectileInput;
        private Vector2 latchedManualProjectileAimDirection;
        private Vector2 latchedManualProjectileTargetPoint;

        [SerializeField] private bool enemyCombatSimulationEnabled = true;
        [SerializeField] private bool skillExecutionEnabled = true;
        [SerializeField] private bool logEnemyAttackAttempts;
        [SerializeField] private bool logSkillExecutionContracts;
        [SerializeField] private bool playerAutoSkillEnabled;
        [SerializeField] private Camera inputCamera;
        [SerializeField] private Transform projectileDestroyBoundary;
        [SerializeField] private float projectileDestroyBoundaryFallbackX = 31f;
        [SerializeField] private EffectManager effectManager;

        public UnitRosterService Roster => roster;
        public EffectManager Effects => ResolveEffectManager();

        public int ActiveUnitCount => roster.Count;
        public int ActivePlayerCount => roster.PlayerCount;
        public int ActiveEnemyCount => roster.EnemyCount;
        public int LastEnemyAttackAttemptCount => enemyCombatSystem.LastAttackAttemptCount;
        public int LastSkillExecutionRoutedCount => skillExecution.LastRoutedCount;
        public int LastSkillExecutionRejectedCount => skillExecution.LastRejectedCount;
        public int SkillChoiceModifierRecordCount => 0;
        public bool PlayerAutoSkillEnabled => playerAutoSkillEnabled;

        private void Awake()
        {
            roster.Clear();
            enemyCombatSystem.Clear();
            ResetPassiveEffectState();
        }

        private void Update()
        {
            TickLearnedPassiveEffects(Time.deltaTime);

            if (skillExecutionEnabled)
            {
                skillExecution.Tick(
                    roster,
                    this,
                    Time.deltaTime,
                    logSkillExecutionContracts,
                    ShouldAutoRouteSkill);
                HandleSelectedPlayerManualSkillInput();
            }

            if (enemyCombatSimulationEnabled)
            {
                enemyCombatSystem.Tick(roster, this, Time.deltaTime, logEnemyAttackAttempts);
            }

            TickUnitStatuses(Time.deltaTime);
        }

        public UnitRosterEntry RegisterPlayerMonster(MonsterUnitRuntimeModel model, MonsterUnitActor actor, Transform hitboxRoot = null)
        {
            var entry = roster.Register(model, actor, hitboxRoot);
            if (IsSelectedPlayerModel(model))
            {
                SetSelectedPlayerAutoSkillMode(playerAutoSkillEnabled);
            }

            return entry;
        }

        public UnitRosterEntry RegisterEnemy(EnemyUnitRuntimeModel model, EnemyUnitActor actor, Transform hitboxRoot = null)
        {
            return roster.Register(model, actor, hitboxRoot);
        }

        public bool UnregisterUnit(BaseUnitRuntimeModel model)
        {
            return roster.Unregister(model);
        }

        public InGameResourceChangeResult ApplyDamage(
            BaseUnitRuntimeModel target,
            float baseDamage,
            DamageAttribute attribute = DamageAttribute.Physical)
        {
            return ApplyDamage(target, baseDamage, attribute, null);
        }

        public InGameResourceChangeResult ApplyDamage(
            BaseUnitRuntimeModel target,
            float baseDamage,
            DamageAttribute attribute,
            BaseUnitRuntimeModel source,
            bool criticalAllowed = false,
            float critChanceBonus = 0f,
            float critDamageBonus = 0f,
            string sourceSkillId = null,
            bool suppressOutgoingDamageTriggers = false,
            bool sourceHitWasExecute = false)
        {
            var depletedShields = new List<UnitStatusRuntime>();
            var absorbedShields = new List<ShieldAbsorbRecord>();
            var options = new DamageApplicationOptions(source, criticalAllowed, critChanceBonus, critDamageBonus, sourceSkillId, suppressOutgoingDamageTriggers, sourceHitWasExecute);
            var result = resourceMutations.ApplyDamage(target, baseDamage, attribute, options, depletedShields, absorbedShields);
            RefreshActorIfChanged(result);
            ShowDamageIfChanged(result);
            DispatchShieldAbsorbTriggers(target, source, absorbedShields);
            DispatchShieldExpireTriggers(target, depletedShields);
            DispatchOutgoingDamageTriggers(target, attribute, options, result, baseDamage);
            DispatchKillTriggers(target, attribute, options, result);
            RemoveUnitIfDead(result);
            return result;
        }

        public InGameResourceChangeResult GrantShield(BaseUnitRuntimeModel target, float amount)
        {
            var result = resourceMutations.GrantShield(target, amount);
            RefreshActorIfChanged(result);
            return result;
        }

        public InGameResourceChangeResult SetShield(BaseUnitRuntimeModel target, float amount)
        {
            var result = resourceMutations.SetShield(target, amount);
            RefreshActorIfChanged(result);
            return result;
        }

        public InGameResourceChangeResult Heal(BaseUnitRuntimeModel target, float amount)
        {
            var result = resourceMutations.Heal(target, amount);
            RefreshActorIfChanged(result);
            return result;
        }

        public UnitStatusRuntime ApplyStatus(
            BaseUnitRuntimeModel target,
            string statusTag,
            int stacks,
            float durationSeconds,
            int maxStacks = 0,
            bool permanent = false,
            bool refreshDuration = true)
        {
            return StatusEffectUtility.TryParse(statusTag, out var kind)
                ? ApplyStatus(target, kind, stacks, durationSeconds, maxStacks, permanent, refreshDuration)
                : null;
        }

        public UnitStatusRuntime ApplyStatus(
            BaseUnitRuntimeModel target,
            StatusEffectKind kind,
            int stacks,
            float durationSeconds,
            int maxStacks = 0,
            bool permanent = false,
            bool refreshDuration = true)
        {
            if (target == null || target.Statuses == null || kind == StatusEffectKind.None)
            {
                return null;
            }

            var status = target.Statuses.Apply(
                kind,
                stacks,
                durationSeconds,
                maxStacks,
                permanent,
                refreshDuration);
            resourceMutations.SynchronizeShieldView(target);
            RefreshUnitActor(target);
            SpawnOrRefreshStatusEffectVisual(target, StatusEffectRuntime.CreateStatusData(kind, null), status);
            return status;
        }

        public UnitStatusRuntime ApplyStatus(
            BaseUnitRuntimeModel target,
            StatusEffectData statusData,
            int stacks,
            float durationSeconds,
            int maxStacks = 0,
            bool permanent = false,
            bool refreshDuration = true,
            BaseUnitRuntimeModel source = null)
        {
            if (target == null || target.Statuses == null || statusData == null || statusData.Kind == StatusEffectKind.None)
            {
                return null;
            }

            var status = target.Statuses.Apply(
                statusData,
                stacks,
                durationSeconds,
                maxStacks,
                permanent,
                refreshDuration);
            if (status != null)
            {
                status.SetSourceUnit(source);
            }
            resourceMutations.SynchronizeShieldView(target);
            RefreshUnitActor(target);
            SpawnOrRefreshStatusEffectVisual(target, statusData, status);
            return status;
        }

        public UnitStatusRuntime ApplyShieldStatus(
            BaseUnitRuntimeModel target,
            StatusEffectData statusData,
            float shieldAmount,
            float durationSeconds,
            int stacks = 1,
            int maxStacks = 0,
            bool permanent = false,
            bool refreshDuration = true,
            BaseUnitRuntimeModel source = null)
        {
            if (target == null || target.Statuses == null || statusData == null || statusData.Kind != StatusEffectKind.Shield)
            {
                return null;
            }

            var adjustedShieldAmount = Mathf.Max(0f, shieldAmount) * StatusEffectRuntime.ResolveShieldReceivedMultiplier(target);
            var status = target.Statuses.Apply(
                statusData,
                stacks,
                durationSeconds,
                maxStacks,
                permanent,
                refreshDuration,
                adjustedShieldAmount);
            if (status != null)
            {
                status.SetSourceUnit(source);
            }

            resourceMutations.SynchronizeShieldView(target);
            RefreshUnitActor(target);
            SpawnOrRefreshStatusEffectVisual(target, statusData, status);
            return status;
        }

        public bool ExtendStatusDuration(BaseUnitRuntimeModel target, string statusTag, float durationDelta)
        {
            return StatusEffectUtility.TryParse(statusTag, out var kind)
                && ExtendStatusDuration(target, kind, durationDelta);
        }

        public bool ExtendStatusDuration(BaseUnitRuntimeModel target, StatusEffectKind kind, float durationDelta)
        {
            if (target == null || target.Statuses == null || kind == StatusEffectKind.None || durationDelta <= 0f)
            {
                return false;
            }

            var changed = target.Statuses.ExtendDurations(
                kind,
                durationDelta,
                status => status != null && !status.Permanent && (!status.IsShieldStatus || status.RemainingShieldAmount > 0f));
            if (!changed)
            {
                return false;
            }

            resourceMutations.SynchronizeShieldView(target);
            RefreshUnitActor(target);

            var activeStatuses = target.Statuses.ActiveStatuses;
            for (var i = 0; i < activeStatuses.Count; i++)
            {
                var status = activeStatuses[i];
                if (status == null || status.Kind != kind || status.SourceData == null)
                {
                    continue;
                }

                SpawnOrRefreshStatusEffectVisual(target, status.SourceData, status);
            }

            return true;
        }

        public void ResetPassiveEffectState()
        {
            appliedOneShotPassiveEffects.Clear();
            passiveTriggerCooldowns.Clear();
            passiveTriggerCounts.Clear();
            passiveEffectRefreshRemaining = 0f;
        }

        public bool ConsumePassiveTriggerCooldown(string key, float cooldownSeconds)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return true;
            }

            var now = Time.time;
            if (passiveTriggerCooldowns.TryGetValue(key, out var readyAt) && readyAt > now)
            {
                return false;
            }

            if (cooldownSeconds > 0f)
            {
                passiveTriggerCooldowns[key] = now + cooldownSeconds;
            }
            else
            {
                passiveTriggerCooldowns.Remove(key);
            }

            return true;
        }

        public bool ConsumePassiveTriggerCount(string key, int triggerEveryCount)
        {
            if (string.IsNullOrWhiteSpace(key) || triggerEveryCount <= 1)
            {
                return true;
            }

            passiveTriggerCounts.TryGetValue(key, out var currentCount);
            currentCount++;
            if (currentCount < triggerEveryCount)
            {
                passiveTriggerCounts[key] = currentCount;
                return false;
            }

            passiveTriggerCounts[key] = 0;
            return true;
        }

        public bool TryExecuteTriggeredSkill(UnitRosterEntry casterEntry, SkillRuntimeInstance runtime, Vector2 targetPoint, bool hasTargetPoint)
        {
            return skillExecution.TryExecuteTriggered(
                casterEntry,
                runtime,
                roster,
                this,
                logSkillExecutionContracts,
                targetPoint,
                hasTargetPoint);
        }

        public void DispatchSkillCastTriggers(UnitRosterEntry sourceEntry, string sourceSkillId, Vector2 eventCenter)
        {
            var source = sourceEntry != null ? sourceEntry.Model : null;
            if (source == null || string.IsNullOrWhiteSpace(sourceSkillId))
            {
                return;
            }

            SkillTriggerRuntime.ExecuteSkillCast(this, roster, source, sourceSkillId, eventCenter);
        }

        private void TickLearnedPassiveEffects(float deltaTime)
        {
            passiveEffectRefreshRemaining -= Mathf.Max(0f, deltaTime);
            if (passiveEffectRefreshRemaining > 0f)
            {
                return;
            }

            passiveEffectRefreshRemaining = PassiveEffectRefreshInterval;
            InGamePassiveEffectRuntime.ApplyLearnedPassiveEffects(this, roster, appliedOneShotPassiveEffects);
        }

        private void DispatchOutgoingDamageTriggers(
            BaseUnitRuntimeModel target,
            DamageAttribute attribute,
            DamageApplicationOptions options,
            InGameResourceChangeResult result,
            float sourceBaseDamage)
        {
            if (options.Source == null || options.SuppressOutgoingDamageTriggers || result.AppliedDamage <= 0f)
            {
                return;
            }

            SkillTriggerRuntime.ExecuteOutgoingDamage(
                this,
                roster,
                options.Source,
                options.SourceSkillId,
                target,
                attribute,
                result.AppliedDamage,
                options.SourceHitWasExecute);

            ApplyOutgoingAdditionalDamageStatuses(target, attribute, options, sourceBaseDamage);
        }

        private void DispatchKillTriggers(
            BaseUnitRuntimeModel target,
            DamageAttribute attribute,
            DamageApplicationOptions options,
            InGameResourceChangeResult result)
        {
            if (!result.IsDead || options.Source == null)
            {
                return;
            }

            SkillTriggerRuntime.ExecuteKill(
                this,
                roster,
                options.Source,
                options.SourceSkillId,
                target,
                attribute,
                result.AppliedDamage,
                options.SourceHitWasExecute);
        }

        private void ApplyOutgoingAdditionalDamageStatuses(
            BaseUnitRuntimeModel target,
            DamageAttribute triggerAttribute,
            DamageApplicationOptions options,
            float sourceBaseDamage)
        {
            if (target == null
                || options.Source == null
                || sourceBaseDamage <= 0f
                || (target.Resources != null && target.Resources.CurrentHealth <= 0f))
            {
                return;
            }

            var specs = StatusEffectRuntime.ResolveOutgoingAdditionalDamageSpecs(options.Source, triggerAttribute);
            for (var i = 0; i < specs.Count; i++)
            {
                if (target.Resources != null && target.Resources.CurrentHealth <= 0f)
                {
                    break;
                }

                var spec = specs[i];
                if (spec.Multiplier <= 0f)
                {
                    continue;
                }

                ApplyDamage(
                    target,
                    Mathf.Max(0f, sourceBaseDamage) * spec.Multiplier,
                    spec.DamageAttribute,
                    options.Source,
                    true,
                    0f,
                    0f,
                    options.SourceSkillId,
                    true);
            }
        }

        public bool HasStatus(BaseUnitRuntimeModel target, string statusTag)
        {
            return target != null && target.Statuses != null && target.Statuses.Has(statusTag);
        }

        public bool HasStatus(BaseUnitRuntimeModel target, StatusEffectKind kind)
        {
            return target != null && target.Statuses != null && target.Statuses.Has(kind);
        }

        public int GetStatusStacks(BaseUnitRuntimeModel target, string statusTag)
        {
            return target != null && target.Statuses != null ? target.Statuses.GetStacks(statusTag) : 0;
        }

        public int GetStatusStacks(BaseUnitRuntimeModel target, StatusEffectKind kind)
        {
            return target != null && target.Statuses != null ? target.Statuses.GetStacks(kind) : 0;
        }

        public bool RemoveStatus(BaseUnitRuntimeModel target, string statusTag)
        {
            if (target == null || target.Statuses == null)
            {
                return false;
            }

            var removed = target.Statuses.Remove(statusTag);
            if (removed)
            {
                resourceMutations.SynchronizeShieldView(target);
                RefreshUnitActor(target);
            }

            return removed;
        }

        public bool RemoveStatus(BaseUnitRuntimeModel target, StatusEffectKind kind)
        {
            if (target == null || target.Statuses == null)
            {
                return false;
            }

            var removed = target.Statuses.Remove(kind);
            if (removed)
            {
                resourceMutations.SynchronizeShieldView(target);
                RefreshUnitActor(target);
            }

            return removed;
        }

        public void EnablePlayerAutoSkillMode()
        {
            SetSelectedPlayerAutoSkillMode(true);
        }

        public void ToggleSelectedPlayerAutoSkillMode()
        {
            SetSelectedPlayerAutoSkillMode(!playerAutoSkillEnabled);
        }

        public void SetSelectedPlayerAutoSkillMode(bool enabled)
        {
            playerAutoSkillEnabled = enabled;
            var player = GetSelectedPlayerEntry();
            if (player != null && player.Model != null)
            {
                player.Model.AutoSkillEnabled = enabled;
            }
        }

        public float ResolveProjectileDestroyBoundaryX()
        {
            return projectileDestroyBoundary != null
                ? projectileDestroyBoundary.position.x
                : projectileDestroyBoundaryFallbackX;
        }

        public UnitRosterEntry FindUnitByCollider(Collider2D collider)
        {
            if (collider == null)
            {
                return null;
            }

            var entries = roster.Entries;
            for (var i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                if (entry == null || entry.Transform == null)
                {
                    continue;
                }

                if (entry.ContainsTransform(collider.transform))
                {
                    return entry;
                }
            }

            return null;
        }

        public bool RefreshUnitActor(BaseUnitRuntimeModel model)
        {
            var entry = roster.Find(model);
            return RefreshUnitActor(entry);
        }

        private void HandleSelectedPlayerManualSkillInput()
        {
            if (playerAutoSkillEnabled)
            {
                return;
            }

            var player = GetSelectedPlayerEntry();
            if (player == null || player.Model == null || player.Model.SkillRuntime == null)
            {
                ClearLatchedManualProjectileInput();
                return;
            }

            var mousePressedThisFrame = IsPrimaryMousePressedThisFrame();
            var mouseHeld = IsPrimaryMouseHeld();
            var pointerOverUi = IsPointerOverUi();
            var hasCurrentManualInput = TryResolveCurrentManualInput(
                player,
                mousePressedThisFrame || mouseHeld,
                pointerOverUi,
                out var currentAimDirection,
                out var currentTargetPoint);
            if (!hasCurrentManualInput
                && !HasProjectileBursting(player.Model.SkillRuntime.ActiveSkills))
            {
                ClearLatchedManualProjectileInput();
                return;
            }

            var activeSkills = player.Model.SkillRuntime.ActiveSkills;
            for (var i = 0; i < activeSkills.Count; i++)
            {
                var runtime = activeSkills[i];
                if (runtime == null)
                {
                    continue;
                }

                var isProjectile = runtime.Data is ProjectileSkillData;
                if (!TryResolveManualSkillInputForRuntime(
                        runtime,
                        isProjectile,
                        mousePressedThisFrame,
                        mouseHeld,
                        hasCurrentManualInput,
                        currentAimDirection,
                        currentTargetPoint,
                        out var aimDirection,
                        out var targetPoint))
                {
                    continue;
                }

                skillExecution.TryExecuteManual(
                    player,
                    runtime,
                    roster,
                    this,
                    Time.deltaTime,
                    aimDirection,
                    targetPoint,
                    logSkillExecutionContracts);
            }

            if (!mouseHeld && !HasProjectileBursting(activeSkills))
            {
                ClearLatchedManualProjectileInput();
            }
        }

        private void SpawnOrRefreshStatusEffectVisual(
            BaseUnitRuntimeModel target,
            StatusEffectData statusData,
            UnitStatusRuntime status)
        {
            if (target == null
                || statusData == null
                || statusData.StatusEffectPrefab == null
                || status == null
                || Effects == null)
            {
                return;
            }

            var entry = roster.Find(target);
            if (entry == null || entry.Transform == null)
            {
                return;
            }

            var unitId = target.Identity != null ? target.Identity.UnitId : string.Empty;
            var sourceId = !string.IsNullOrWhiteSpace(status.SourceSkillId)
                ? status.SourceSkillId
                : statusData.SourceSkillId;
            var key = $"{unitId}:{status.Kind}:{sourceId}:{statusData.StatusEffectPrefab.GetInstanceID()}";
            if (statusEffectVisuals.TryGetValue(key, out var existing) && existing == null)
            {
                statusEffectVisuals.Remove(key);
                existing = null;
            }

            var lifetime = status.Permanent
                ? 3600f
                : Mathf.Max(0.1f, status.DurationRemaining);
            if (existing == null)
            {
                existing = Effects.InstantiateSkillPrefab(statusData.StatusEffectPrefab, entry.Transform.position, Quaternion.identity);
                if (existing == null)
                {
                    return;
                }

                statusEffectVisuals[key] = existing;
            }

            var actor = existing.GetComponent<InGameAttachedSkillEffectActor>();
            if (actor == null)
            {
                actor = existing.AddComponent<InGameAttachedSkillEffectActor>();
            }

            actor.Initialize(entry.Transform, lifetime, Vector3.zero);
        }

        private bool ShouldAutoRouteSkill(UnitRosterEntry entry, SkillRuntimeInstance runtime)
        {
            if (!HasVisibleLivingEnemyInMainCamera()
                || entry == null
                || entry.Model == null
                || !entry.Model.AutoSkillEnabled)
            {
                return false;
            }

            return !IsSelectedPlayerEntry(entry) || playerAutoSkillEnabled;
        }

        private void TickUnitStatuses(float deltaTime)
        {
            if (deltaTime <= 0f)
            {
                return;
            }

            var entries = roster.Entries;
            for (var i = 0; i < entries.Count; i++)
            {
                var model = entries[i] != null ? entries[i].Model : null;
                if (model == null || model.Statuses == null)
                {
                    continue;
                }

                var removedStatuses = new List<UnitStatusRuntime>();
                if (model.Statuses.Tick(deltaTime, removedStatuses))
                {
                    resourceMutations.SynchronizeShieldView(model);
                    RefreshUnitActor(model);
                    DispatchStatusExpireTriggers(model, removedStatuses);
                    DispatchShieldExpireTriggers(model, removedStatuses);
                }
            }
        }

        private void DispatchShieldAbsorbTriggers(
            BaseUnitRuntimeModel shieldTarget,
            BaseUnitRuntimeModel attacker,
            IReadOnlyList<ShieldAbsorbRecord> absorbedShields)
        {
            if (shieldTarget == null || absorbedShields == null || absorbedShields.Count == 0)
            {
                return;
            }

            for (var i = 0; i < absorbedShields.Count; i++)
            {
                var record = absorbedShields[i];
                if (record.Status == null || record.AbsorbedAmount <= 0f)
                {
                    continue;
                }

                SkillTriggerRuntime.ExecuteShieldAbsorb(this, roster, shieldTarget, attacker, record.Status, record.AbsorbedAmount);
            }
        }

        private void DispatchShieldExpireTriggers(BaseUnitRuntimeModel shieldTarget, IReadOnlyList<UnitStatusRuntime> removedStatuses)
        {
            if (shieldTarget == null || removedStatuses == null || removedStatuses.Count == 0)
            {
                return;
            }

            for (var i = 0; i < removedStatuses.Count; i++)
            {
                var status = removedStatuses[i];
                if (status == null || !status.IsShieldStatus)
                {
                    continue;
                }

                SkillTriggerRuntime.ExecuteShieldExpire(this, roster, shieldTarget, status);
            }
        }

        private void DispatchStatusExpireTriggers(BaseUnitRuntimeModel statusOwner, IReadOnlyList<UnitStatusRuntime> removedStatuses)
        {
            if (statusOwner == null || removedStatuses == null || removedStatuses.Count == 0)
            {
                return;
            }

            for (var i = 0; i < removedStatuses.Count; i++)
            {
                var status = removedStatuses[i];
                if (status == null)
                {
                    continue;
                }

                SkillTriggerRuntime.ExecuteStatusExpire(this, roster, statusOwner, status);
            }
        }

        private UnitRosterEntry GetSelectedPlayerEntry()
        {
            return roster.Players.Count > 0 ? roster.Players[0] : null;
        }

        private bool IsSelectedPlayerEntry(UnitRosterEntry entry)
        {
            return entry != null && entry == GetSelectedPlayerEntry();
        }

        private static bool IsSelectedPlayerModel(BaseUnitRuntimeModel model)
        {
            return model != null
                && model.Identity != null
                && model.Identity.Side == UnitSide.Player
                && model.Identity.SlotIndex == 0;
        }

        private Vector2 ResolveAimDirection(UnitRosterEntry player, Vector2 targetPoint)
        {
            if (player == null || player.Transform == null)
            {
                return Vector2.zero;
            }

            return targetPoint - (Vector2)player.Transform.position;
        }

        private Vector2 ResolveMouseWorldPoint()
        {
            var cameraToUse = inputCamera != null ? inputCamera : Camera.main;
            if (cameraToUse == null)
            {
                return Vector2.zero;
            }

            var mouse = Mouse.current.position.ReadValue();
            var world = cameraToUse.ScreenToWorldPoint(new Vector3(mouse.x, mouse.y, -cameraToUse.transform.position.z));
            return world;
        }

        private static bool IsPrimaryMousePressedThisFrame()
        {
            var mouse = Mouse.current;
            return mouse != null && mouse.leftButton.wasPressedThisFrame;
        }

        private static bool IsPrimaryMouseHeld()
        {
            var mouse = Mouse.current;
            return mouse != null && mouse.leftButton.isPressed;
        }

        private bool TryResolveCurrentManualInput(
            UnitRosterEntry player,
            bool wantsManualInput,
            bool pointerOverUi,
            out Vector2 aimDirection,
            out Vector2 targetPoint)
        {
            aimDirection = Vector2.zero;
            targetPoint = Vector2.zero;
            if (!wantsManualInput || pointerOverUi)
            {
                return false;
            }

            targetPoint = ResolveMouseWorldPoint();
            aimDirection = ResolveAimDirection(player, targetPoint);
            if (aimDirection.sqrMagnitude <= 0.0001f)
            {
                return false;
            }

            latchedManualProjectileAimDirection = aimDirection;
            latchedManualProjectileTargetPoint = targetPoint;
            hasLatchedManualProjectileInput = true;
            return true;
        }

        private bool TryResolveManualSkillInputForRuntime(
            SkillRuntimeInstance runtime,
            bool isProjectile,
            bool mousePressedThisFrame,
            bool mouseHeld,
            bool hasCurrentManualInput,
            Vector2 currentAimDirection,
            Vector2 currentTargetPoint,
            out Vector2 aimDirection,
            out Vector2 targetPoint)
        {
            aimDirection = Vector2.zero;
            targetPoint = Vector2.zero;
            if (!isProjectile)
            {
                if (!mousePressedThisFrame || !hasCurrentManualInput)
                {
                    return false;
                }

                aimDirection = currentAimDirection;
                targetPoint = currentTargetPoint;
                return true;
            }

            if (hasCurrentManualInput && mouseHeld)
            {
                aimDirection = currentAimDirection;
                targetPoint = currentTargetPoint;
                return true;
            }

            if (runtime.IsBursting && hasLatchedManualProjectileInput)
            {
                aimDirection = latchedManualProjectileAimDirection;
                targetPoint = latchedManualProjectileTargetPoint;
                return true;
            }

            return false;
        }

        private static bool HasProjectileBursting(IReadOnlyList<SkillRuntimeInstance> activeSkills)
        {
            if (activeSkills == null)
            {
                return false;
            }

            for (var i = 0; i < activeSkills.Count; i++)
            {
                var runtime = activeSkills[i];
                if (runtime != null
                    && runtime.Data is ProjectileSkillData
                    && runtime.IsBursting)
                {
                    return true;
                }
            }

            return false;
        }

        private void ClearLatchedManualProjectileInput()
        {
            hasLatchedManualProjectileInput = false;
            latchedManualProjectileAimDirection = Vector2.zero;
            latchedManualProjectileTargetPoint = Vector2.zero;
        }

        private static bool IsPointerOverUi()
        {
            return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
        }

        private void RefreshActorIfChanged(InGameResourceChangeResult result)
        {
            if (result.Changed)
            {
                RefreshUnitActor(result.Target);
            }
        }

        private void ShowDamageIfChanged(InGameResourceChangeResult result)
        {
            if (!result.Changed || result.AppliedDamage <= 0f)
            {
                return;
            }

            var entry = roster.Find(result.Target);
            if (entry == null || entry.Actor == null)
            {
                return;
            }

            var monsterActor = entry.Actor as MonsterUnitActor;
            if (monsterActor != null)
            {
                monsterActor.ShowDamage(result.AppliedDamage);
                if (!result.IsDead)
                {
                    monsterActor.TryPlayHitAnimation();
                }

                return;
            }

            var enemyActor = entry.Actor as EnemyUnitActor;
            if (enemyActor != null)
            {
                enemyActor.ShowDamage(result.AppliedDamage);
            }
        }

        private void RemoveUnitIfDead(InGameResourceChangeResult result)
        {
            if (!result.Changed || !result.IsDead || result.Target == null)
            {
                return;
            }

            var entry = roster.Find(result.Target);
            if (entry == null)
            {
                return;
            }

            var actor = entry.Actor;
            roster.Unregister(result.Target);
            if (actor != null)
            {
                var monsterActor = actor as MonsterUnitActor;
                if (monsterActor != null)
                {
                    monsterActor.TryPlayDeathAnimation();
                }

                Destroy(actor.gameObject, 0.95f);
            }
        }

        private static bool RefreshUnitActor(UnitRosterEntry entry)
        {
            if (entry == null || entry.Actor == null)
            {
                return false;
            }

            var monsterActor = entry.Actor as MonsterUnitActor;
            if (monsterActor != null)
            {
                monsterActor.RefreshDebugView();
                return true;
            }

            var enemyActor = entry.Actor as EnemyUnitActor;
            if (enemyActor != null)
            {
                enemyActor.RefreshDebugView();
                return true;
            }

            return false;
        }

        private EffectManager ResolveEffectManager()
        {
            if (effectManager == null)
            {
                effectManager = GetComponent<EffectManager>();
            }

            return effectManager;
        }

        private bool HasVisibleLivingEnemyInMainCamera()
        {
            var cameraToUse = Camera.main != null ? Camera.main : inputCamera;
            var enemies = roster.Enemies;
            for (var i = 0; i < enemies.Count; i++)
            {
                var enemy = enemies[i];
                if (enemy == null || !enemy.IsAlive || enemy.Transform == null)
                {
                    continue;
                }

                if (cameraToUse == null)
                {
                    return true;
                }

                var viewport = cameraToUse.WorldToViewportPoint(enemy.Transform.position);
                if (viewport.z >= 0f
                    && viewport.x >= 0f
                    && viewport.x <= 1f
                    && viewport.y >= 0f
                    && viewport.y <= 1f)
                {
                    return true;
                }
            }

            return false;
        }
    }

        public sealed class UnitResourceMutationService
        {
            public InGameResourceChangeResult ApplyDamage(
                BaseUnitRuntimeModel target,
                float baseDamage,
                DamageAttribute attribute = DamageAttribute.Physical)
            {
                return ApplyDamage(target, baseDamage, attribute, default, null, null);
            }

            public InGameResourceChangeResult ApplyDamage(
                BaseUnitRuntimeModel target,
                float baseDamage,
                DamageAttribute attribute,
                ICollection<UnitStatusRuntime> depletedShieldStatuses)
            {
                return ApplyDamage(target, baseDamage, attribute, default, depletedShieldStatuses, null);
            }

            public InGameResourceChangeResult ApplyDamage(
                BaseUnitRuntimeModel target,
                float baseDamage,
                DamageAttribute attribute,
                DamageApplicationOptions options,
                ICollection<UnitStatusRuntime> depletedShieldStatuses,
                ICollection<ShieldAbsorbRecord> absorbedShieldStatuses)
            {
                if (target == null || target.Resources == null || baseDamage <= 0f)
                {
                    return InGameResourceChangeResult.Unchanged(target);
                }

                var resources = target.Resources;
                var beforeHealth = Mathf.Max(0f, resources.CurrentHealth);
                var beforeShield = ComputeTotalShield(target);
                var finalDamage = ResolveFinalDamage(target, baseDamage, attribute, options);
                if (target.Statuses != null)
                {
                    target.Statuses.RecordIncomingDamage(attribute, finalDamage);
                }

                var statusShieldDamage = target.Statuses != null
                    ? target.Statuses.ConsumeShield(finalDamage, depletedShieldStatuses, absorbedShieldStatuses)
                    : 0f;
                var damageAfterStatusShield = Mathf.Max(0f, finalDamage - statusShieldDamage);
                var directShieldBefore = Mathf.Max(0f, resources.DirectShield);
                var directShieldDamage = Mathf.Min(directShieldBefore, damageAfterStatusShield);
                var remainingDamage = Mathf.Max(0f, damageAfterStatusShield - directShieldDamage);

                resources.DirectShield = RoundResource(Mathf.Max(0f, directShieldBefore - directShieldDamage));
                resources.CurrentHealth = RoundResource(Mathf.Max(0f, beforeHealth - remainingDamage));
                SynchronizeShieldView(target);

                return new InGameResourceChangeResult(
                    target,
                    beforeHealth,
                    resources.CurrentHealth,
                    beforeShield,
                    resources.CurrentShield,
                    finalDamage,
                    resources.CurrentHealth <= 0f);
            }

        public InGameResourceChangeResult GrantShield(BaseUnitRuntimeModel target, float amount)
        {
            if (target == null || target.Resources == null || amount <= 0f)
            {
                return InGameResourceChangeResult.Unchanged(target);
            }

            var resources = target.Resources;
            var beforeHealth = Mathf.Max(0f, resources.CurrentHealth);
            var beforeShield = ComputeTotalShield(target);
            resources.CurrentHealth = RoundResource(beforeHealth);
            resources.DirectShield = RoundResource(Mathf.Max(0f, resources.DirectShield) + amount);
            SynchronizeShieldView(target);

            return new InGameResourceChangeResult(
                target,
                beforeHealth,
                resources.CurrentHealth,
                beforeShield,
                resources.CurrentShield,
                0f,
                resources.CurrentHealth <= 0f);
        }

        public InGameResourceChangeResult SetShield(BaseUnitRuntimeModel target, float amount)
        {
            if (target == null || target.Resources == null)
            {
                return InGameResourceChangeResult.Unchanged(target);
            }

            var resources = target.Resources;
            var beforeHealth = Mathf.Max(0f, resources.CurrentHealth);
            var beforeShield = ComputeTotalShield(target);
            resources.CurrentHealth = RoundResource(beforeHealth);
            resources.DirectShield = RoundResource(Mathf.Max(0f, amount));
            SynchronizeShieldView(target);

            return new InGameResourceChangeResult(
                target,
                beforeHealth,
                resources.CurrentHealth,
                beforeShield,
                resources.CurrentShield,
                0f,
                resources.CurrentHealth <= 0f);
        }

        public InGameResourceChangeResult Heal(BaseUnitRuntimeModel target, float amount)
        {
            if (target == null || target.Resources == null || target.Stats == null || amount <= 0f)
            {
                return InGameResourceChangeResult.Unchanged(target);
            }

            var resources = target.Resources;
            var beforeHealth = Mathf.Max(0f, resources.CurrentHealth);
            var beforeShield = ComputeTotalShield(target);
            var maxHealth = Mathf.Max(0f, target.Stats.MaxHealth);
            resources.CurrentHealth = RoundResource(Mathf.Min(maxHealth, beforeHealth + amount));
            SynchronizeShieldView(target);

            return new InGameResourceChangeResult(
                target,
                beforeHealth,
                resources.CurrentHealth,
                beforeShield,
                resources.CurrentShield,
                0f,
                resources.CurrentHealth <= 0f);
        }

        private static float ResolveDamageAfterDefense(
            BaseUnitRuntimeModel target,
            float baseDamage,
            DamageAttribute attribute)
        {
            var defense = target.Defenses != null ? target.Defenses.Get(attribute) : 0f;
            defense -= StatusEffectRuntime.ResolveFlatElementResistReduction(target, attribute);
            var statusReduction = StatusEffectRuntime.ResolveElementResistReduction(target, attribute);
            defense *= Mathf.Clamp01(1f - statusReduction);
            var safeDefense = Mathf.Max(-95f, defense);
            return Mathf.Max(0f, baseDamage) * (100f / (100f + safeDefense));
        }

        private static float ResolveFinalDamage(
            BaseUnitRuntimeModel target,
            float baseDamage,
            DamageAttribute attribute,
            DamageApplicationOptions options)
        {
            if (options.CriticalAllowed && options.Source != null)
            {
                var sourceStats = options.Source.Stats;
                var sourceCriticalChance = (sourceStats != null ? sourceStats.CriticalChance : DamageCalculator.BaseCriticalChance)
                    + StatusEffectRuntime.ResolveCriticalChanceBonus(options.Source);
                var sourceCriticalDamage = sourceStats != null ? sourceStats.CriticalDamage : DamageCalculator.BaseCriticalMultiplier;
                sourceCriticalDamage += StatusEffectRuntime.ResolveCriticalDamageBonus(options.Source);
                var targetCriticalResistance = (target != null && target.Stats != null ? target.Stats.CriticalResistance : 0f)
                    + StatusEffectRuntime.ResolveCriticalResistanceBonus(target);
                var criticalDamageTakenBonus = StatusEffectRuntime.ResolveCriticalDamageTakenBonus(target);
                var damage = DamageCalculator.Resolve(
                    Mathf.Max(0f, baseDamage),
                    attribute,
                    target != null ? ToAttributeDefenseSet(target.Defenses) : null,
                    flatDefenseReduction: StatusEffectRuntime.ResolveFlatElementResistReduction(target, attribute),
                    percentDefenseReductions: new[] { StatusEffectRuntime.ResolveElementResistReduction(target, attribute) },
                    criticalChanceBonus: sourceCriticalChance + options.CritChanceBonus - DamageCalculator.BaseCriticalChance,
                    criticalMultiplierBonus: sourceCriticalDamage + options.CritDamageBonus - DamageCalculator.BaseCriticalMultiplier,
                    targetCriticalResistance: targetCriticalResistance,
                    criticalDamageTakenBonus: criticalDamageTakenBonus,
                    finalDamageMultiplier: ResolveIncomingDamageMultiplier(target, options.Source, attribute)).FinalDamage;
                return Mathf.Round(Mathf.Max(0f, damage));
            }

            return Mathf.Round(ResolveDamageAfterDefense(target, baseDamage, attribute) * ResolveIncomingDamageMultiplier(target, options.Source, attribute));
        }

        private static AttributeDefenseSet ToAttributeDefenseSet(UnitDefenseRuntime defenses)
        {
            if (defenses == null)
            {
                return null;
            }

            return new AttributeDefenseSet
            {
                Physical = defenses.Physical,
                Fire = defenses.Fire,
                Lightning = defenses.Lightning,
                Ice = defenses.Ice,
                Darkness = defenses.Darkness,
                Holy = defenses.Holy
            };
        }

        private static float ResolveIncomingDamageMultiplier(BaseUnitRuntimeModel target, BaseUnitRuntimeModel source, DamageAttribute attribute)
        {
            var statusMultiplier = StatusEffectRuntime.ResolveIncomingDamageMultiplier(target, source, attribute);
            var enemy = target as EnemyUnitRuntimeModel;
            if (enemy == null)
            {
                return statusMultiplier;
            }

            var multiplier = Mathf.Max(0f, enemy.PassiveIncomingDamageMultiplier);
            if (enemy.IncomingDamageMultiplierRemainingSeconds > 0f)
            {
                multiplier *= Mathf.Max(0f, enemy.IncomingDamageMultiplier);
            }

            return multiplier * statusMultiplier;
        }

        public void SynchronizeShieldView(BaseUnitRuntimeModel target)
        {
            if (target == null || target.Resources == null)
            {
                return;
            }

            target.Resources.DirectShield = RoundResource(Mathf.Max(0f, target.Resources.DirectShield));
            target.Resources.CurrentShield = ComputeTotalShield(target);
        }

        private static float ComputeTotalShield(BaseUnitRuntimeModel target)
        {
            if (target == null || target.Resources == null)
            {
                return 0f;
            }

            var directShield = Mathf.Max(0f, target.Resources.DirectShield);
            var timedShield = target.Statuses != null ? Mathf.Max(0f, target.Statuses.GetTotalShieldAmount()) : 0f;
            return RoundResource(directShield + timedShield);
        }

        private static float RoundResource(float value)
        {
            return Mathf.Round(Mathf.Max(0f, value));
        }
    }

    public readonly struct InGameResourceChangeResult
    {
        public InGameResourceChangeResult(
            BaseUnitRuntimeModel target,
            float previousHealth,
            float currentHealth,
            float previousShield,
            float currentShield,
            float appliedDamage,
            bool isDead)
        {
            Target = target;
            PreviousHealth = previousHealth;
            CurrentHealth = currentHealth;
            PreviousShield = previousShield;
            CurrentShield = currentShield;
            AppliedDamage = appliedDamage;
            IsDead = isDead;
        }

        public BaseUnitRuntimeModel Target { get; }
        public float PreviousHealth { get; }
        public float CurrentHealth { get; }
        public float PreviousShield { get; }
        public float CurrentShield { get; }
        public float AppliedDamage { get; }
        public bool IsDead { get; }
        public bool Changed =>
            !Mathf.Approximately(PreviousHealth, CurrentHealth)
            || !Mathf.Approximately(PreviousShield, CurrentShield);

        public static InGameResourceChangeResult Unchanged(BaseUnitRuntimeModel target)
        {
            var resources = target != null ? target.Resources : null;
            var health = resources != null ? Mathf.Max(0f, resources.CurrentHealth) : 0f;
            var shield = resources != null ? Mathf.Max(0f, resources.CurrentShield) : 0f;
            return new InGameResourceChangeResult(target, health, health, shield, shield, 0f, health <= 0f);
        }
    }
}
