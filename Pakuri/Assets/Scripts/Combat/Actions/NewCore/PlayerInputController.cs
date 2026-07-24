using System;
using System.Collections.Generic;
using Pakuri.NewCore.Definitions.Skills;
using Pakuri.NewCore.Units.Models;

namespace Pakuri.NewCore.Combat.Actions
{
    public enum ManualInputPhase
    {
        Pressed,
        Held,
        Released
    }

    public sealed class PlayerInputController
    {
        private readonly Queue<ManualSkillRequest> pending =
            new Queue<ManualSkillRequest>();
        private MonsterActionController selectedController;
        private CombatVector2? lastProjectileAim;
        private CombatVector2? lastProjectileTarget;

        public MonsterModel SelectedMonster => selectedController?.Monster;

        public void Select(MonsterActionController controller)
        {
            selectedController =
                controller ?? throw new ArgumentNullException(nameof(controller));
            pending.Clear();
            lastProjectileAim = null;
            lastProjectileTarget = null;
        }

        public void SetAutoSkillEnabled(bool enabled)
        {
            if (selectedController == null)
            {
                throw new InvalidOperationException("No selected monster is registered.");
            }

            selectedController.Monster.SetAutoSkillEnabled(enabled);
            if (enabled)
            {
                pending.Clear();
            }
        }

        public bool SubmitManualSkillRequest(
            SkillDefinition skill,
            CombatVector2 aimDirection,
            CombatVector2 targetPoint,
            ManualInputPhase phase,
            bool pointerOverUi)
        {
            if (selectedController == null
                || selectedController.Monster.AutoSkillEnabled
                || pointerOverUi
                || phase == ManualInputPhase.Released
                || aimDirection.SqrMagnitude <= 0.0001f)
            {
                return false;
            }

            bool projectile = skill is ProjectileDefinition;
            if (!projectile && phase != ManualInputPhase.Pressed)
            {
                return false;
            }

            if (projectile)
            {
                lastProjectileAim = aimDirection;
                lastProjectileTarget = targetPoint;
            }

            pending.Enqueue(new ManualSkillRequest(skill, aimDirection, targetPoint));
            return true;
        }

        public bool ContinueProjectileBurst(SkillDefinition skill)
        {
            if (!(skill is ProjectileDefinition)
                || !lastProjectileAim.HasValue
                || !lastProjectileTarget.HasValue
                || selectedController == null
                || selectedController.Monster.AutoSkillEnabled)
            {
                return false;
            }

            pending.Enqueue(
                new ManualSkillRequest(
                    skill,
                    lastProjectileAim.Value,
                    lastProjectileTarget.Value));
            return true;
        }

        public bool Process(IReadOnlyList<UnitBaseModel> registeredUnits)
        {
            if (selectedController == null
                || selectedController.Monster.AutoSkillEnabled
                || pending.Count == 0)
            {
                return false;
            }

            ManualSkillRequest request = pending.Dequeue();
            return selectedController.TryExecuteManual(
                request.Skill,
                registeredUnits,
                request.AimDirection,
                request.TargetPoint);
        }

        public void ResetCombatInput()
        {
            pending.Clear();
            lastProjectileAim = null;
            lastProjectileTarget = null;
        }

        private readonly struct ManualSkillRequest
        {
            public ManualSkillRequest(
                SkillDefinition skill,
                CombatVector2 aimDirection,
                CombatVector2 targetPoint)
            {
                Skill = skill ?? throw new ArgumentNullException(nameof(skill));
                AimDirection = aimDirection;
                TargetPoint = targetPoint;
            }

            public SkillDefinition Skill { get; }

            public CombatVector2 AimDirection { get; }

            public CombatVector2 TargetPoint { get; }
        }
    }
}
