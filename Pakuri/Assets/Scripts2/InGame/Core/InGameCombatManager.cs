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
        [SerializeField] private bool selectedPlayerPrimarySkillManual = true;
        [SerializeField] private Camera inputCamera;
        [SerializeField] private Transform projectileDestroyBoundary;
        [SerializeField] private float projectileDestroyBoundaryFallbackX = 31f;
        [SerializeField] private GameObject eveAProjectilePrefab;
        [SerializeField] private GameObject arielBShieldEffectPrefab;
        [SerializeField] private GameObject warriorSkillPrefab;
        [SerializeField] private GameObject rogueSkillPrefab;
        [SerializeField] private GameObject priestSkillPrefab;

        public UnitRosterService Roster => roster;

        public int ActiveUnitCount => roster.Count;
        public int ActivePlayerCount => roster.PlayerCount;
        public int ActiveEnemyCount => roster.EnemyCount;
        public int LastEnemyAttackAttemptCount => enemyCombatSimulation.LastAttackAttemptCount;
        public int LastSkillExecutionRoutedCount => skillExecution.LastRoutedCount;
        public int LastSkillExecutionRejectedCount => skillExecution.LastRejectedCount;
        public int SkillChoiceModifierRecordCount => skillExecution.ModifierRecordCount;
        public bool SelectedPlayerPrimarySkillManual => selectedPlayerPrimarySkillManual;

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

        public void EnableSelectedPlayerPrimarySkillAuto()
        {
            selectedPlayerPrimarySkillManual = false;
        }

        public void SetSelectedPlayerPrimarySkillManual(bool manual)
        {
            selectedPlayerPrimarySkillManual = manual;
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
                case Pakuri.Data.StageOneEnemySkillKind.ShurikenThrow:
                    return rogueSkillPrefab;
                case Pakuri.Data.StageOneEnemySkillKind.Heal:
                    return priestSkillPrefab;
                default:
                    return null;
            }
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
            if (!selectedPlayerPrimarySkillManual || !IsPrimaryMouseHeld() || IsPointerOverUi())
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
            return !IsSelectedPlayerPrimarySkill(entry, runtime) || !selectedPlayerPrimarySkillManual;
        }

        private bool IsSelectedPlayerPrimarySkill(UnitRosterEntry entry, SkillRuntimeInstance runtime)
        {
            return entry != null
                && runtime != null
                && runtime.Slot == InGameSkillSlot.A
                && entry == GetSelectedPlayerEntry();
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
