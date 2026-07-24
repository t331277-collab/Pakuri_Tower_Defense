using System;
using System.Collections.Generic;
using Pakuri.NewCore.Combat.Skills.Execution;
using Pakuri.NewCore.Definitions.Skills;
using Pakuri.NewCore.Run;
using Pakuri.NewCore.Units.Models;

namespace Pakuri.NewCore.Combat.Actions
{
    public sealed class EnemyActionController : UnitActionController
    {
        private readonly SkillTargeting targeting;
        private readonly UnitMovementController movement;
        private readonly StageManager stageManager;
        private readonly NexusModel nexus;
        private readonly float nexusContactDistance;

        public EnemyActionController(
            EnemyModel enemy,
            InGameCombatManager combatManager,
            SkillTargeting targeting,
            UnitMovementController movement,
            StageManager stageManager,
            NexusModel nexus,
            float nexusContactDistance)
            : base(enemy, combatManager)
        {
            Enemy = enemy;
            this.targeting = targeting ?? throw new ArgumentNullException(nameof(targeting));
            this.movement = movement ?? throw new ArgumentNullException(nameof(movement));
            this.stageManager =
                stageManager ?? throw new ArgumentNullException(nameof(stageManager));
            this.nexus = nexus ?? throw new ArgumentNullException(nameof(nexus));
            if (nexusContactDistance < 0f
                || float.IsNaN(nexusContactDistance)
                || float.IsInfinity(nexusContactDistance))
            {
                throw new ArgumentOutOfRangeException(nameof(nexusContactDistance));
            }

            this.nexusContactDistance = nexusContactDistance;
        }

        public EnemyModel Enemy { get; }

        public bool IsComplete =>
            !Enemy.IsAlive || Enemy.HasContactedNexus;

        public void Tick(float deltaTime, IReadOnlyList<UnitBaseModel> registeredUnits)
        {
            if (IsComplete || !Enemy.AutoAttackEnabled)
            {
                return;
            }

            UnitBaseModel target = FindNearestPlayer(registeredUnits);
            if (target == null)
            {
                TickNexus(deltaTime);
                return;
            }

            SkillDefinition skill = ResolveSkill(registeredUnits);
            if (skill == null)
            {
                return;
            }

            float range = Math.Max(
                SkillTargeting.ReadFloat(skill, "cast_range"),
                SkillTargeting.ReadFloat(skill, "radius"));
            float distance = CombatVector2.Distance(Enemy.Position, target.Position);
            if (distance > range)
            {
                movement.MoveTowards(
                    Enemy,
                    target.Position,
                    Enemy.EnemyDefinition.move_speed ?? 0f,
                    deltaTime,
                    range);
                return;
            }

            Execute(new SkillExecutionRequest(Enemy, skill, registeredUnits));
        }

        private UnitBaseModel FindNearestPlayer(IReadOnlyList<UnitBaseModel> registeredUnits)
        {
            List<UnitBaseModel> players = new List<UnitBaseModel>();
            for (int index = 0; index < registeredUnits.Count; index++)
            {
                if (registeredUnits[index] is MonsterModel)
                {
                    players.Add(registeredUnits[index]);
                }
            }

            return targeting.FindNearestLiving(Enemy, players, false);
        }

        private SkillDefinition ResolveSkill(IReadOnlyList<UnitBaseModel> registeredUnits)
        {
            SkillDefinition slotB = Enemy.SkillBucket.SlotBSkill;
            if (CanUse(slotB.skill_id))
            {
                string scope = SkillTargeting.ReadString(slotB, "target_scope");
                bool support = string.Equals(scope, "Friendly", StringComparison.Ordinal)
                    || string.Equals(scope, "FriendlyInRadius", StringComparison.Ordinal)
                    || string.Equals(scope, "Self", StringComparison.Ordinal);
                if (!support || HasDamagedAlly(registeredUnits))
                {
                    return slotB;
                }
            }

            return CanUse(Enemy.SkillBucket.SlotASkill.skill_id)
                ? Enemy.SkillBucket.SlotASkill
                : null;
        }

        private static bool HasDamagedAlly(IReadOnlyList<UnitBaseModel> units)
        {
            for (int index = 0; index < units.Count; index++)
            {
                if (units[index] is EnemyModel enemy
                    && enemy.IsAlive
                    && enemy.CurrentHealth < enemy.MaximumHealth)
                {
                    return true;
                }
            }

            return false;
        }

        private void TickNexus(float deltaTime)
        {
            if (Enemy.HasContactedNexus)
            {
                return;
            }
            if (movement.MoveTowards(
                Enemy,
                nexus.Position,
                Enemy.EnemyDefinition.move_speed ?? 0f,
                deltaTime,
                nexusContactDistance))
            {
                CombatManager.ApplyNexusDamage(Enemy, nexus);
                Enemy.MarkNexusContact();
                stageManager.TryUnregisterFieldUnit(Enemy);
            }
        }
    }
}
