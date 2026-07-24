using Pakuri.NewCore.Combat.Actions;
using Pakuri.NewCore.Presentation.Actors;
using Pakuri.NewCore.Units.Models;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Pakuri.NewCore.Presentation.Scene
{
    public sealed class NewCoreInputController : MonoBehaviour
    {
        [SerializeField] private Camera inputCamera;
        [SerializeField] private bool autoSkillEnabled;

        private PlayerInputController input;
        private MonsterActorBehaviour selectedActor;

        public bool AutoSkillEnabled => autoSkillEnabled;

        public void Bind(
            PlayerInputController controller,
            MonsterActorBehaviour actor)
        {
            input = controller
                ?? throw new System.ArgumentNullException(nameof(controller));
            selectedActor = actor
                ?? throw new System.ArgumentNullException(nameof(actor));
            input.SetAutoSkillEnabled(autoSkillEnabled);
        }

        public void Capture()
        {
            if (input == null
                || selectedActor == null
                || autoSkillEnabled
                || Mouse.current == null)
            {
                return;
            }

            var mouse = Mouse.current;
            var pressed = mouse.leftButton.wasPressedThisFrame;
            var held = mouse.leftButton.isPressed;
            var released = mouse.leftButton.wasReleasedThisFrame;
            if (!pressed && !held && !released)
            {
                return;
            }

            var camera = inputCamera != null ? inputCamera : Camera.main;
            if (camera == null)
            {
                return;
            }

            var screen = mouse.position.ReadValue();
            var world = camera.ScreenToWorldPoint(
                new Vector3(screen.x, screen.y, -camera.transform.position.z));
            var origin = selectedActor.transform.position;
            var aim = new CombatVector2(
                world.x - origin.x,
                world.y - origin.y);
            var target = new CombatVector2(world.x, world.y);
            var phase = pressed
                ? ManualInputPhase.Pressed
                : released
                    ? ManualInputPhase.Released
                    : ManualInputPhase.Held;
            var pointerOverUi = EventSystem.current != null
                && EventSystem.current.IsPointerOverGameObject();
            var skills = selectedActor.Monster.SkillBucket.ActiveSkills;
            for (var index = 0; index < skills.Count; index++)
            {
                input.SubmitManualSkillRequest(
                    skills[index],
                    aim,
                    target,
                    phase,
                    pointerOverUi);
            }
        }

        public void ToggleAutoSkill()
        {
            autoSkillEnabled = !autoSkillEnabled;
            if (input != null)
            {
                input.SetAutoSkillEnabled(autoSkillEnabled);
            }
        }

        public void SynchronizeAutoSkillState()
        {
            if (input != null)
            {
                input.SetAutoSkillEnabled(autoSkillEnabled);
            }
        }
    }
}
