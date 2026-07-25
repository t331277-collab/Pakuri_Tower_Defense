using System;
using System.Collections.Generic;
using Pakuri.NewCore.Combat.Skills.Execution;
using Pakuri.NewCore.Definitions.Skills;
using Pakuri.NewCore.Run;
using Pakuri.NewCore.Units.Models;

/* 적의 대상 선정, 스킬 사용, 이동, 넥서스 공격 행동을 진행한다. */
namespace Pakuri.NewCore.Combat.Actions
{
    public class EnemyActionController : UnitActionController
    {
        private readonly SkillTargeting targeting;
        private readonly UnitMovementController movement;
        private readonly StageManager stageManager;
        private readonly NexusModel nexus;
        private readonly float nexusContactDistance;

        /* 적 모델과 대상 선정·이동·stage·넥서스 전투 의존성을 연결한다. */
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
            this.targeting = targeting;
            this.movement = movement;
            this.stageManager =
                stageManager;
            this.nexus = nexus;

            this.nexusContactDistance = nexusContactDistance;
        }

        public EnemyModel Enemy { get; }

        public bool IsComplete =>
            !Enemy.IsAlive || Enemy.HasContactedNexus;

        /* 쿨다운을 진행하고 사용할 스킬·대상을 정해 공격 또는 이동 행동을 수행한다. */
        public void Tick(float deltaTime, IReadOnlyList<UnitBaseModel> registeredUnits)
        {
            if (IsComplete || !Enemy.AutoAttackEnabled)
            {
                return;
            }

            if (FindNearestPlayer(registeredUnits) == null)
            {
                TickNexus(deltaTime);
                return;
            }

            SkillDefinition skill = ResolveSkill(registeredUnits);
            if (skill == null)
            {
                return;
            }

            UnitBaseModel target = ResolveTarget(skill, registeredUnits);
            if (target == null)
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

        /* 스킬 대상 규칙으로 후보를 선정하고 첫 대상을 반환한다. */
        private UnitBaseModel ResolveTarget(
            SkillDefinition skill,
            IReadOnlyList<UnitBaseModel> registeredUnits)
        {
            IReadOnlyList<UnitBaseModel> targets =
                targeting.Resolve(Enemy, skill, registeredUnits);
            if (targets.Count == 0)
            {
                return null;
            }

            return targets[0];
        }

        /* 적 행동 조건에 맞는 대상을 탐색해 반환한다. */
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

        /* 현재 대상·아군 피해 상태와 쿨다운에 따라 사용할 적 스킬을 선택한다. */
        private SkillDefinition ResolveSkill(IReadOnlyList<UnitBaseModel> registeredUnits)
        {
            SkillDefinition slotB = Enemy.SkillBucket.SlotBSkill;
            if (CanUse(slotB.skill_id))
            {
                string scope = SkillTargeting.ReadString(slotB, "target_scope");
                bool support = string.Equals(scope, "Friendly", StringComparison.Ordinal)
                    || string.Equals(scope, "FriendlyInRadius", StringComparison.Ordinal)
                    || string.Equals(scope, "Self", StringComparison.Ordinal);
                if (!support
                    || !(slotB is HealDefinition)
                    || HasDamagedAlly(registeredUnits))
                {
                    return slotB;
                }
            }

            if (CanUse(Enemy.SkillBucket.SlotASkill.skill_id))
            {
                return Enemy.SkillBucket.SlotASkill;
            }

            return null;
        }

        /* 등록 유닛 중 체력이 감소한 생존 아군이 있는지 확인한다. */
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

        /* 아군이 없을 때 넥서스로 이동하고 도달하면 피해 요청을 한 번 기록한다. */
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
