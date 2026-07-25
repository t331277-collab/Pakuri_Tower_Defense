using System;
using System.Collections.Generic;
using Pakuri.NewCore.Combat.Skills.Actors;
using Pakuri.NewCore.Combat.Skills.Execution;
using Pakuri.NewCore.Run;
using Pakuri.NewCore.Units.Models;

/* 등록된 아군·적 행동을 중앙 tick 순서에 따라 실행하고 정리한다. */
namespace Pakuri.NewCore.Combat.Actions
{
    public enum CombatTickStep
    {
        PassiveBefore,
        Cooldowns,
        AutomaticMonsters,
        ManualInput,
        Enemies,
        SkillActors,
        Statuses,
        PassiveAfter
    }

    public class InGameActionManager
    {
        private readonly StageManager stageManager;
        private readonly Func<bool> canProgressCombat;
        private readonly Action applyPassiveChanges;
        private readonly PlayerInputController playerInput;
        private readonly SkillActorManager skillActors;
        private readonly SkillTriggerDispatcher skillTriggers;
        private readonly InGameCombatManager combatManager;
        private readonly List<MonsterActionController> monsters =
            new List<MonsterActionController>();
        private readonly List<EnemyActionController> enemies =
            new List<EnemyActionController>();
        private readonly HashSet<UnitBaseModel> combatStartedUnits =
            new HashSet<UnitBaseModel>();

        /* 중앙 tick에 필요한 stage·입력·스킬 Actor·trigger·전투 서비스를 연결한다. */
        public InGameActionManager(
            StageManager stageManager,
            Func<bool> canProgressCombat,
            Action applyPassiveChanges,
            PlayerInputController playerInput,
            SkillActorManager skillActors,
            SkillTriggerDispatcher skillTriggers,
            InGameCombatManager combatManager)
        {
            this.stageManager =
                stageManager;
            this.canProgressCombat =
                canProgressCombat;
            this.applyPassiveChanges =
                applyPassiveChanges;
            this.playerInput =
                playerInput;
            this.skillActors =
                skillActors;
            this.skillTriggers = skillTriggers;
            this.combatManager =
                combatManager;
        }

        public event Action<CombatTickStep> StepCompleted;

        /* 몬스터 행동 컨트롤러를 선택 여부와 함께 중복 없이 등록한다. */
        public void RegisterMonster(MonsterActionController controller, bool selected)
        {
            monsters.Add(controller);
            if (selected)
            {
                playerInput.Select(controller);
            }
        }

        /* 적 행동 컨트롤러를 중복 없이 등록한다. */
        public void RegisterEnemy(EnemyActionController controller)
        {

            enemies.Add(controller);
        }

        /* 패시브부터 상태 만료까지 정의된 CombatTickStep 순서로 한 frame을 진행한다. */
        public void Tick(float deltaTime)
        {
            ValidateDeltaTime(deltaTime);
            if (!canProgressCombat())
            {
                return;
            }

            combatManager.ApplyPassiveChanges(stageManager.FieldUnits);
            applyPassiveChanges();
            StepCompleted?.Invoke(CombatTickStep.PassiveBefore);

            IReadOnlyList<UnitBaseModel> units = stageManager.FieldUnits;
            skillTriggers?.Tick(deltaTime);
            for (int index = 0; index < units.Count; index++)
            {
                if (units[index] is MonsterModel monster)
                {
                    monster.SkillBucket.TickCooldowns(
                        deltaTime * monster.ActionSpeedMultiplier);
                }
                else if (units[index] is EnemyModel enemy)
                {
                    enemy.SkillBucket.TickCooldowns(
                        deltaTime * enemy.ActionSpeedMultiplier);
                }
            }
            StepCompleted?.Invoke(CombatTickStep.Cooldowns);

            for (int index = 0; index < monsters.Count; index++)
            {
                monsters[index].TickAutomatic(units);
                if (!canProgressCombat())
                {
                    return;
                }
            }
            StepCompleted?.Invoke(CombatTickStep.AutomaticMonsters);

            playerInput.Process(units);
            if (!canProgressCombat())
            {
                return;
            }
            StepCompleted?.Invoke(CombatTickStep.ManualInput);

            for (int index = 0; index < enemies.Count; index++)
            {
                enemies[index].Tick(deltaTime, units);
                if (!canProgressCombat())
                {
                    return;
                }
            }
            for (int index = enemies.Count - 1; index >= 0; index--)
            {
                if (enemies[index].IsComplete)
                {
                    enemies.RemoveAt(index);
                }
            }
            StepCompleted?.Invoke(CombatTickStep.Enemies);

            skillActors.Tick(deltaTime);
            if (!canProgressCombat())
            {
                return;
            }
            StepCompleted?.Invoke(CombatTickStep.SkillActors);

            for (int index = 0; index < units.Count; index++)
            {
                units[index].TickStatusEffects(deltaTime);
            }
            StepCompleted?.Invoke(CombatTickStep.Statuses);

            combatManager.ApplyPassiveChanges(stageManager.FieldUnits);
            applyPassiveChanges();
            StepCompleted?.Invoke(CombatTickStep.PassiveAfter);
        }

        /* 이번 전투에 새로 참가한 생존 유닛에 전투 시작 trigger를 한 번 전달한다. */
        public void BeginOrExtendCombat(
            IReadOnlyList<UnitBaseModel> units)
        {

            for (int index = 0; index < units.Count; index++)
            {
                UnitBaseModel unit = units[index];
                if (unit != null
                    && unit.IsAlive
                    && combatStartedUnits.Add(unit))
                {
                    combatManager.NotifyCombatStart(unit, units);
                }
            }
        }

        /* 전투 시작 기록·수동 입력·스킬 Actor·전투 구독 상태를 정리한다. */
        public void EndCombat()
        {
            combatStartedUnits.Clear();
            playerInput.ResetCombatInput();
            skillActors.Clear();
            combatManager.EndCombat();
        }

        /* 중앙 tick 경과 시간이 음수가 아닌 유한값인지 검증한다. */
        private static void ValidateDeltaTime(float deltaTime)
        {
        }
    }
}
