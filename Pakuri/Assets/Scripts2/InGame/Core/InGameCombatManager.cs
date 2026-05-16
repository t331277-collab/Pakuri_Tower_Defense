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
        private readonly EnemyCombatSimulationSystem enemyCombatSimulation = new EnemyCombatSimulationSystem();
        private readonly UnitResourceMutationService resourceMutations = new UnitResourceMutationService();
        private readonly SkillExecutionSystem skillExecution = new SkillExecutionSystem();

        [SerializeField] private bool enemyCombatSimulationEnabled = true;
        [SerializeField] private bool skillExecutionEnabled = true;
        [SerializeField] private bool logEnemyAttackAttempts;
        [SerializeField] private bool logSkillExecutionContracts;
        [SerializeField] private TextAsset skillChoiceModifierCsv;
        [SerializeField] private bool playerAutoSkillEnabled;
        [SerializeField] private Camera inputCamera;
        [SerializeField] private Transform projectileDestroyBoundary;
        [SerializeField] private float projectileDestroyBoundaryFallbackX = 31f;
        [SerializeField] private GameObject eveAProjectilePrefab;
        [SerializeField] private GameObject arielBShieldEffectPrefab;
        [SerializeField] private GameObject warriorSkillPrefab;
        [SerializeField] private GameObject shieldSkillPrefab;
        [SerializeField] private GameObject archerSkillPrefab;
        [SerializeField] private GameObject rogueSkillPrefab;
        [SerializeField] private GameObject priestSkillPrefab;
        [SerializeField] private GameObject shieldKingSkillPrefab;
        [SerializeField] private GameObject warriorKingSkillPrefab;
        [SerializeField] private GameObject karinSkillPrefab;
        [SerializeField] private Transform runtimeSkillRoot;

        public UnitRosterService Roster => roster;

        public int ActiveUnitCount => roster.Count;
        public int ActivePlayerCount => roster.PlayerCount;
        public int ActiveEnemyCount => roster.EnemyCount;
        public int LastEnemyAttackAttemptCount => enemyCombatSimulation.LastAttackAttemptCount;
        public int LastSkillExecutionRoutedCount => skillExecution.LastRoutedCount;
        public int LastSkillExecutionRejectedCount => skillExecution.LastRejectedCount;
        public int SkillChoiceModifierRecordCount => skillExecution.ModifierRecordCount;
        public bool PlayerAutoSkillEnabled => playerAutoSkillEnabled;

        private void Awake()
        {
            roster.Clear();
            enemyCombatSimulation.Clear();
            ReloadSkillChoiceModifierData();
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
                enemyCombatSimulation.Tick(roster, this, Time.deltaTime, logEnemyAttackAttempts);
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
                RefreshUnitActor(target);
            }

            return removed;
        }

        public void EnablePlayerAutoSkillMode()
        {
            playerAutoSkillEnabled = true;
        }

        public GameObject ResolveSkillEffectPrefab(string skillId)
        {
            switch (skillId)
            {
                case "eve-a":
                    return eveAProjectilePrefab;
                case "ariel-b":
                    return arielBShieldEffectPrefab;
                default:
                    return null;
            }
        }

        public float ResolveProjectileDestroyBoundaryX()
        {
            return projectileDestroyBoundary != null
                ? projectileDestroyBoundary.position.x
                : projectileDestroyBoundaryFallbackX;
        }

        public GameObject InstantiateSkillPrefab(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            return prefab != null
                ? Instantiate(prefab, position, rotation, ResolveRuntimeSkillRoot())
                : null;
        }

        public GameObject ResolveEnemySkillPrefab(EnemyUnitRuntimeModel enemy)
        {
            if (enemy == null)
            {
                return null;
            }

            switch (enemy.StageOneSkill)
            {
                case Pakuri.Data.StageOneEnemySkillKind.Slash:
                    return warriorSkillPrefab;
                case Pakuri.Data.StageOneEnemySkillKind.ShieldUp:
                    return shieldSkillPrefab;
                case Pakuri.Data.StageOneEnemySkillKind.AimedShot:
                    return archerSkillPrefab;
                case Pakuri.Data.StageOneEnemySkillKind.ShurikenThrow:
                    return rogueSkillPrefab;
                case Pakuri.Data.StageOneEnemySkillKind.Heal:
                    return priestSkillPrefab;
                case Pakuri.Data.StageOneEnemySkillKind.GuardianFlag:
                    return shieldKingSkillPrefab;
                case Pakuri.Data.StageOneEnemySkillKind.ChargeCommand:
                    return warriorKingSkillPrefab;
                case Pakuri.Data.StageOneEnemySkillKind.SacredSwordWave:
                    return karinSkillPrefab;
                default:
                    return null;
            }
        }

        private Transform ResolveRuntimeSkillRoot()
        {
            if (runtimeSkillRoot != null)
            {
                return runtimeSkillRoot;
            }

            var root = GameObject.Find("RunTimeSkill");
            if (root != null)
            {
                runtimeSkillRoot = root.transform;
                return runtimeSkillRoot;
            }

            var created = new GameObject("RunTimeSkill");
            runtimeSkillRoot = created.transform;
            return runtimeSkillRoot;
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

        public void ReloadSkillChoiceModifierData()
        {
            var library = skillChoiceModifierCsv != null
                ? SkillChoiceModifierCsvParser.ParseLibrary(skillChoiceModifierCsv.text)
                : new SkillChoiceModifierLibrary();
            skillExecution.SetChoiceModifierLibrary(library);
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
    }
}
