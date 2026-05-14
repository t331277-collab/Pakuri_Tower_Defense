using UnityEngine;
using Pakuri.Combat;

namespace Pakuri.InGame
{
    [DisallowMultipleComponent]
    public sealed class InGameCombatManager : MonoBehaviour
    {
        private readonly UnitRosterService roster = new UnitRosterService();
        private readonly EnemyCombatSimulationSystem enemyCombatSimulation = new EnemyCombatSimulationSystem();
        private readonly UnitResourceMutationService resourceMutations = new UnitResourceMutationService();

        [SerializeField] private bool enemyCombatSimulationEnabled = true;
        [SerializeField] private bool logEnemyAttackAttempts;

        public UnitRosterService Roster => roster;

        public int ActiveUnitCount => roster.Count;
        public int ActivePlayerCount => roster.PlayerCount;
        public int ActiveEnemyCount => roster.EnemyCount;
        public int LastEnemyAttackAttemptCount => enemyCombatSimulation.LastAttackAttemptCount;

        private void Awake()
        {
            roster.Clear();
            enemyCombatSimulation.Clear();
        }

        private void Update()
        {
            if (!enemyCombatSimulationEnabled)
            {
                return;
            }

            enemyCombatSimulation.Tick(roster, Time.deltaTime, logEnemyAttackAttempts);
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

        public bool RefreshUnitActor(BaseUnitRuntimeModel model)
        {
            var entry = roster.Find(model);
            return RefreshUnitActor(entry);
        }

        private void RefreshActorIfChanged(InGameResourceChangeResult result)
        {
            if (result.Changed)
            {
                RefreshUnitActor(result.Target);
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
