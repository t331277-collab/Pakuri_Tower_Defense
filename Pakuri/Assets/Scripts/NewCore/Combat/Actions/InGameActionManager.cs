using System;
using System.Collections.Generic;
using Pakuri.NewCore.Combat.Skills.Actors;
using Pakuri.NewCore.Combat.Skills.Execution;
using Pakuri.NewCore.Run;
using Pakuri.NewCore.Units.Models;

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

    public sealed class InGameActionManager
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
                stageManager ?? throw new ArgumentNullException(nameof(stageManager));
            this.canProgressCombat =
                canProgressCombat ?? throw new ArgumentNullException(nameof(canProgressCombat));
            this.applyPassiveChanges =
                applyPassiveChanges ?? throw new ArgumentNullException(nameof(applyPassiveChanges));
            this.playerInput =
                playerInput ?? throw new ArgumentNullException(nameof(playerInput));
            this.skillActors =
                skillActors ?? throw new ArgumentNullException(nameof(skillActors));
            this.skillTriggers = skillTriggers;
            this.combatManager =
                combatManager ?? throw new ArgumentNullException(nameof(combatManager));
        }

        public event Action<CombatTickStep> StepCompleted;

        public void RegisterMonster(MonsterActionController controller, bool selected)
        {
            if (controller == null)
            {
                throw new ArgumentNullException(nameof(controller));
            }

            if (monsters.Contains(controller))
            {
                throw new InvalidOperationException("Monster Controller is already registered.");
            }

            if (selected)
            {
                if (monsters.Count != 0)
                {
                    throw new InvalidOperationException(
                        "Only the first registered monster can be selected.");
                }
            }
            monsters.Add(controller);
            if (selected)
            {
                playerInput.Select(controller);
            }
        }

        public void RegisterEnemy(EnemyActionController controller)
        {
            if (controller == null)
            {
                throw new ArgumentNullException(nameof(controller));
            }

            if (enemies.Contains(controller))
            {
                throw new InvalidOperationException("Enemy Controller is already registered.");
            }

            enemies.Add(controller);
        }

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

        public void BeginOrExtendCombat(
            IReadOnlyList<UnitBaseModel> units)
        {
            if (units == null)
            {
                throw new ArgumentNullException(nameof(units));
            }

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

        public void EndCombat()
        {
            combatStartedUnits.Clear();
            playerInput.ResetCombatInput();
            skillActors.Clear();
            combatManager.EndCombat();
        }

        private static void ValidateDeltaTime(float deltaTime)
        {
            if (deltaTime < 0f || float.IsNaN(deltaTime) || float.IsInfinity(deltaTime))
            {
                throw new ArgumentOutOfRangeException(nameof(deltaTime));
            }
        }
    }
}
