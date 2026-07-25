using System;
using Pakuri.NewCore.Combat.Skills.Execution;
using Pakuri.NewCore.Units.Models;

/* 유닛 행동 컨트롤러가 공유하는 쿨다운 확인과 스킬 실행 경계를 제공한다. */
namespace Pakuri.NewCore.Combat.Actions
{
    public abstract class UnitActionController
    {
        /* 행동할 유닛 모델과 전투 실행 권한을 null 없이 연결한다. */
        protected UnitActionController(
            UnitBaseModel model,
            InGameCombatManager combatManager)
        {
            Model = model;
            CombatManager =
                combatManager;
        }

        public UnitBaseModel Model { get; }

        protected InGameCombatManager CombatManager { get; }

        /* 유닛 생존·행동 가능 상태와 해당 스킬 쿨다운 사용 가능 여부를 확인한다. */
        protected bool CanUse(string skillId)
        {
            if (!Model.IsAlive || !Model.CanAct)
            {
                return false;
            }

            if (Model is MonsterModel monster)
            {
                return monster.SkillBucket
                    .GetCooldown(skillId)
                    .CanUse();
            }

            if (Model is EnemyModel enemy)
            {
                return enemy.SkillBucket
                    .GetCooldown(skillId)
                    .CanUse();
            }

            return false;
        }

        /* 스킬 사용 가능 여부를 확인한 뒤 전투 관리자에 실행을 요청한다. */
        protected bool Execute(SkillExecutionRequest request)
        {
            return CanUse(request.Skill.skill_id)
                && CombatManager.TryExecuteSkill(request);
        }
    }
}
