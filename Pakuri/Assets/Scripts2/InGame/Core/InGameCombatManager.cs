using UnityEngine;
using Pakuri.Combat;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Pakuri.InGame
{
    [DisallowMultipleComponent]
    public sealed class InGameCombatManager : MonoBehaviour
    {
        private readonly UnitRosterService roster = new UnitRosterService();
        private readonly EnemyCombatSystem enemyCombatSystem = new EnemyCombatSystem();
        private readonly UnitResourceMutationService resourceMutations = new UnitResourceMutationService();
        private readonly SkillExecutionSystem skillExecution = new SkillExecutionSystem();

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
        }

        private void Update()
        {
            if (skillExecutionEnabled)
            {
                skillExecution.Tick(
                    roster,
                    this,
                    Time.deltaTime,
                    logSkillExecutionContracts,
                    ShouldAutoRouteSkill);
                HandleSelectedPlayerPrimarySkillInput();
            }

            if (enemyCombatSimulationEnabled)
            {
                enemyCombatSystem.Tick(roster, this, Time.deltaTime, logEnemyAttackAttempts);
            }

            TickUnitStatuses(Time.deltaTime);
        }

        public UnitRosterEntry RegisterPlayerMonster(MonsterUnitRuntimeModel model, MonsterUnitActor actor)
        {
            return roster.Register(model, actor);
        }

        public UnitRosterEntry RegisterEnemy(EnemyUnitRuntimeModel model, EnemyUnitActor actor)
        {
            return roster.Register(model, actor);
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
            var result = resourceMutations.ApplyDamage(target, baseDamage, attribute);
            RefreshActorIfChanged(result);
            ShowDamageIfChanged(result);
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
            return status;
        }

        public UnitStatusRuntime ApplyStatus(
            BaseUnitRuntimeModel target,
            StatusEffectData statusData,
            int stacks,
            float durationSeconds,
            int maxStacks = 0,
            bool permanent = false,
            bool refreshDuration = true)
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
            resourceMutations.SynchronizeShieldView(target);
            RefreshUnitActor(target);
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
            bool refreshDuration = true)
        {
            if (target == null || target.Statuses == null || statusData == null || statusData.Kind != StatusEffectKind.Shield)
            {
                return null;
            }

            var status = target.Statuses.Apply(
                statusData,
                stacks,
                durationSeconds,
                maxStacks,
                permanent,
                refreshDuration,
                shieldAmount);
            resourceMutations.SynchronizeShieldView(target);
            RefreshUnitActor(target);
            return status;
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
            playerAutoSkillEnabled = true;
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

                var candidate = collider.transform;
                if (candidate == entry.Transform || candidate.IsChildOf(entry.Transform))
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

        private void HandleSelectedPlayerPrimarySkillInput()
        {
            if (playerAutoSkillEnabled || !IsPrimaryMouseHeld() || IsPointerOverUi())
            {
                return;
            }

            var player = GetSelectedPlayerEntry();
            var runtime = FindActiveSkillRuntime(player, InGameSkillSlot.A);
            var aimDirection = ResolveMouseAimDirection(player);
            if (player == null || runtime == null || aimDirection.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            skillExecution.TryExecuteManual(
                player,
                runtime,
                roster,
                this,
                Time.deltaTime,
                aimDirection,
                logSkillExecutionContracts);
        }

        private bool ShouldAutoRouteSkill(UnitRosterEntry entry, SkillRuntimeInstance runtime)
        {
            return !IsSelectedPlayerPrimarySkill(entry, runtime) || playerAutoSkillEnabled;
        }

        private bool IsSelectedPlayerPrimarySkill(UnitRosterEntry entry, SkillRuntimeInstance runtime)
        {
            return entry != null
                && runtime != null
                && runtime.Slot == InGameSkillSlot.A
                && entry == GetSelectedPlayerEntry();
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

                if (model.Statuses.Tick(deltaTime))
                {
                    resourceMutations.SynchronizeShieldView(model);
                    RefreshUnitActor(model);
                }
            }
        }

        private UnitRosterEntry GetSelectedPlayerEntry()
        {
            return roster.Players.Count > 0 ? roster.Players[0] : null;
        }

        private static SkillRuntimeInstance FindActiveSkillRuntime(UnitRosterEntry entry, InGameSkillSlot slot)
        {
            var runtimeSet = entry != null && entry.Model != null ? entry.Model.SkillRuntime : null;
            var activeSkills = runtimeSet != null ? runtimeSet.ActiveSkills : null;
            if (activeSkills == null)
            {
                return null;
            }

            for (var i = 0; i < activeSkills.Count; i++)
            {
                var runtime = activeSkills[i];
                if (runtime != null && runtime.Slot == slot)
                {
                    return runtime;
                }
            }

            return null;
        }

        private Vector2 ResolveMouseAimDirection(UnitRosterEntry player)
        {
            if (player == null || player.Transform == null)
            {
                return Vector2.zero;
            }

            var cameraToUse = inputCamera != null ? inputCamera : Camera.main;
            if (cameraToUse == null)
            {
                return Vector2.right;
            }

            var mouse = Mouse.current.position.ReadValue();
            var world = cameraToUse.ScreenToWorldPoint(new Vector3(mouse.x, mouse.y, -cameraToUse.transform.position.z));
            var direction = world - player.Transform.position;
            direction.z = 0f;
            return direction;
        }

        private static bool IsPrimaryMouseHeld()
        {
            var mouse = Mouse.current;
            return mouse != null && mouse.leftButton.isPressed;
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
    }

    public sealed class UnitResourceMutationService
    {
        public InGameResourceChangeResult ApplyDamage(
            BaseUnitRuntimeModel target,
            float baseDamage,
            DamageAttribute attribute = DamageAttribute.Physical)
        {
            if (target == null || target.Resources == null || baseDamage <= 0f)
            {
                return InGameResourceChangeResult.Unchanged(target);
            }

            var resources = target.Resources;
            var beforeHealth = Mathf.Max(0f, resources.CurrentHealth);
            var beforeShield = ComputeTotalShield(target);
            var finalDamage = Mathf.Round(ResolveDamageAfterDefense(target, baseDamage, attribute) * ResolveIncomingDamageMultiplier(target, attribute));
            var statusShieldDamage = target.Statuses != null ? target.Statuses.ConsumeShield(finalDamage) : 0f;
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
            var statusReduction = StatusEffectRuntime.ResolveElementResistReduction(target, attribute);
            defense *= Mathf.Clamp01(1f - statusReduction);
            var safeDefense = Mathf.Max(-95f, defense);
            return Mathf.Max(0f, baseDamage) * (100f / (100f + safeDefense));
        }

        private static float ResolveIncomingDamageMultiplier(BaseUnitRuntimeModel target, DamageAttribute attribute)
        {
            var statusMultiplier = StatusEffectRuntime.ResolveIncomingDamageMultiplier(target, attribute);
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
