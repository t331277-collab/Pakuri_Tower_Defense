using System;
using System.Collections.Generic;
using Pakuri.NewCore.Definitions.Skills;
using Pakuri.NewCore.Units.Actors;
using Pakuri.NewCore.Units.Models;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

/* 수동 전투 명령 상태와 Unity mouse 입력 번역을 한 player 입력 경계에서 소유한다. */
namespace Pakuri.NewCore.Combat.Actions
{
    public enum ManualInputPhase
    {
        Pressed,
        Held,
        Released
    }

    public class PlayerInputController : MonoBehaviour
    {
        [SerializeField] private Camera inputCamera;
        [SerializeField] private bool autoSkillEnabled;

        private readonly Queue<ManualSkillRequest> pending =
            new Queue<ManualSkillRequest>();
        private MonsterActionController selectedController;
        private MonsterActor selectedActor;
        private CombatVector2? lastProjectileAim;
        private CombatVector2? lastProjectileTarget;

        public MonsterModel SelectedMonster => selectedController?.Monster;

        public int PendingRequestCount => pending.Count;

        public bool AutoSkillEnabled => autoSkillEnabled;

        /* 선택 Monster의 action authority를 바꾸고 이전 수동 입력을 초기화한다. */
        public void Select(MonsterActionController controller)
        {
            selectedController =
                controller;
            pending.Clear();
            lastProjectileAim = null;
            lastProjectileTarget = null;
        }

        /* 선택 Monster scene Actor를 입력 원점으로 연결하고 Inspector 자동 상태를 적용한다. */
        public void BindActor(MonsterActor actor)
        {
            selectedActor = actor;
            SetAutoSkillEnabled(autoSkillEnabled);
        }

        /* 현재 frame의 mouse 상태를 선택 Monster의 수동 스킬 요청으로 변환한다. */
        public void Capture()
        {
            if (selectedController == null
                || selectedActor == null
                || autoSkillEnabled
                || Mouse.current == null)
            {
                return;
            }

            BeginManualFrame();
            Mouse mouse = Mouse.current;
            bool pressed = mouse.leftButton.wasPressedThisFrame;
            bool held = mouse.leftButton.isPressed;
            bool released = mouse.leftButton.wasReleasedThisFrame;
            if (!pressed && !held && !released)
            {
                return;
            }

            Camera camera = inputCamera;
            if (camera == null)
            {
                camera = Camera.main;
            }

            if (camera == null)
            {
                return;
            }

            Vector2 screen = mouse.position.ReadValue();
            Vector3 world = camera.ScreenToWorldPoint(
                new Vector3(
                    screen.x,
                    screen.y,
                    -camera.transform.position.z));
            Vector3 origin = selectedActor.transform.position;
            var aim = new CombatVector2(
                world.x - origin.x,
                world.y - origin.y);
            var target = new CombatVector2(world.x, world.y);
            ManualInputPhase phase = ManualInputPhase.Held;
            if (pressed)
            {
                phase = ManualInputPhase.Pressed;
            }
            else if (released)
            {
                phase = ManualInputPhase.Released;
            }

            bool pointerOverUi = EventSystem.current != null
                && EventSystem.current.IsPointerOverGameObject();
            IReadOnlyList<SkillDefinition> skills =
                selectedActor.Monster.SkillBucket.ActiveSkills;
            for (int index = 0; index < skills.Count; index++)
            {
                SubmitManualSkillRequest(
                    skills[index],
                    aim,
                    target,
                    phase,
                    pointerOverUi);
            }
        }

        /* Inspector/UI 자동 전투 값을 반전하고 선택 Monster에 적용한다. */
        public void ToggleAutoSkill()
        {
            autoSkillEnabled = !autoSkillEnabled;
            if (selectedController != null)
            {
                SetAutoSkillEnabled(autoSkillEnabled);
            }
        }

        /* day reset 뒤 Inspector/UI 자동 전투 값을 선택 Monster에 재적용한다. */
        public void SynchronizeAutoSkillState()
        {
            if (selectedController != null)
            {
                SetAutoSkillEnabled(autoSkillEnabled);
            }
        }

        /* 선택 Monster의 자동 전투 상태를 바꾸고 필요하면 대기 요청을 비운다. */
        public void SetAutoSkillEnabled(bool enabled)
        {

            selectedController.Monster.SetAutoSkillEnabled(enabled);
            if (enabled)
            {
                pending.Clear();
            }
        }

        /* 새 manual frame 시작 전에 이전 frame 요청을 비운다. */
        public void BeginManualFrame()
        {
            pending.Clear();
        }

        /* public 사용자 입력을 검증해 실행 가능한 manual 요청만 queue에 넣는다. */
        public bool SubmitManualSkillRequest(
            SkillDefinition skill,
            CombatVector2 aimDirection,
            CombatVector2 targetPoint,
            ManualInputPhase phase,
            bool pointerOverUi)
        {
            if (selectedController == null
                || selectedController.Monster.AutoSkillEnabled
                || skill == null
                || !selectedController.CanExecuteManual(skill)
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

        /* 저장된 projectile 조준으로 다음 burst 요청을 queue에 넣는다. */
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

        /* 현재 frame queue를 등록 순서대로 선택 Monster Controller에 전달한다. */
        public bool Process(IReadOnlyList<UnitBaseModel> registeredUnits)
        {
            if (selectedController == null
                || selectedController.Monster.AutoSkillEnabled
                || pending.Count == 0)
            {
                return false;
            }

            bool executed = false;
            int frameRequestCount = pending.Count;
            for (int index = 0; index < frameRequestCount; index++)
            {
                ManualSkillRequest request = pending.Dequeue();
                executed |= selectedController.TryExecuteManual(
                    request.Skill,
                    registeredUnits,
                    request.AimDirection,
                    request.TargetPoint);
            }

            return executed;
        }

        /* combat 종료 때 수동 요청과 저장 projectile 조준을 초기화한다. */
        public void ResetCombatInput()
        {
            pending.Clear();
            lastProjectileAim = null;
            lastProjectileTarget = null;
        }

        private readonly struct ManualSkillRequest
        {
            /* 검증된 스킬과 조준·목표 좌표를 한 frame 요청으로 저장한다. */
            public ManualSkillRequest(
                SkillDefinition skill,
                CombatVector2 aimDirection,
                CombatVector2 targetPoint)
            {
                Skill = skill;
                AimDirection = aimDirection;
                TargetPoint = targetPoint;
            }

            public SkillDefinition Skill { get; }

            public CombatVector2 AimDirection { get; }

            public CombatVector2 TargetPoint { get; }
        }
    }
}
