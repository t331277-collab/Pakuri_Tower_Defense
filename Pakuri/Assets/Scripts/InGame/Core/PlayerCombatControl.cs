using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Pakuri.InGame
{
    /*
     * 선택 플레이어의 수동 조준과 자동 스킬 허용 조건을 처리한다.
     * Code Builder: 입력과 화면 판정을 InGameCombatManager에서 분리했다.
     */
    [DisallowMultipleComponent]
    public sealed class PlayerCombatControl : MonoBehaviour
    {
        [SerializeField] private Camera inputCamera;
        [SerializeField] private bool autoSkillEnabled;

        private bool hasSavedProjectileInput;
        private Vector2 savedAimDirection;
        private Vector2 savedTargetPoint;

        public bool AutoSkillEnabled => autoSkillEnabled;

        internal void HandleManualInput(
            UnitRosterService roster,
            SkillExecutionSystem skillExecution,
            InGameCombatManager combatManager,
            float deltaTime,
            bool logExecution)
        {
            if (autoSkillEnabled)
            {
                return;
            }

            var player = FindSelectedPlayer(roster);
            if (player == null || player.Model == null || player.Model.SkillRuntime == null)
            {
                ClearManualInput();
                return;
            }

            var pressed = IsMousePressed();
            var held = IsMouseHeld();
            var hasInput = TryGetCurrentInput(
                player,
                inputCamera,
                pressed || held,
                IsPointerOverUi(),
                out var currentAim,
                out var currentTarget);
            var activeSkills = player.Model.SkillRuntime.ActiveSkills;

            if (!hasInput && !HasBurstingProjectile(activeSkills))
            {
                ClearManualInput();
                return;
            }

            for (var i = 0; i < activeSkills.Count; i++)
            {
                var runtime = activeSkills[i];
                if (runtime == null)
                {
                    continue;
                }

                var isProjectile = runtime.Data is ProjectileSkillData;
                if (!TryGetSkillInput(
                        runtime,
                        isProjectile,
                        pressed,
                        held,
                        hasInput,
                        currentAim,
                        currentTarget,
                        out var aim,
                        out var target))
                {
                    continue;
                }

                skillExecution.TryExecuteManual(
                    player,
                    runtime,
                    roster,
                    combatManager,
                    deltaTime,
                    aim,
                    target,
                    logExecution);
            }

            if (!held && !HasBurstingProjectile(activeSkills))
            {
                ClearManualInput();
            }
        }

        public bool CanUseAutoSkill(
            UnitRosterEntry entry,
            UnitRosterService roster)
        {
            if (entry != null && entry.Model is EnemyUnitRuntimeModel)
            {
                return false;
            }

            if (!HasVisibleEnemy(roster, inputCamera)
                || entry == null
                || entry.Model == null
                || !entry.Model.AutoSkillEnabled)
            {
                return false;
            }

            return !IsSelectedPlayer(entry, roster) || autoSkillEnabled;
        }

        public void ToggleAutoSkillMode(UnitRosterService roster)
        {
            SetAutoSkillMode(!autoSkillEnabled, roster);
        }

        public void SetAutoSkillMode(bool enabled, UnitRosterService roster)
        {
            autoSkillEnabled = enabled;
            ApplyAutoSkillModeToSelectedPlayer(roster);
        }

        public void ApplyAutoSkillModeToSelectedPlayer(UnitRosterService roster)
        {
            var player = FindSelectedPlayer(roster);
            if (player != null && player.Model != null)
            {
                player.Model.AutoSkillEnabled = autoSkillEnabled;
            }
        }

        public UnitRosterEntry FindSelectedPlayer(UnitRosterService roster)
        {
            if (roster == null)
            {
                return null;
            }

            var players = roster.Players;
            for (var i = 0; i < players.Count; i++)
            {
                var entry = players[i];
                if (entry != null && IsSelectedPlayerModel(entry.Model))
                {
                    return entry;
                }
            }

            return null;
        }

        public static bool IsSelectedPlayerModel(BaseUnitRuntimeModel model)
        {
            return model != null
                && model.Identity != null
                && model.Identity.Side == UnitSide.Player
                && model.Identity.Role == UnitRole.Monster
                && model.Identity.SlotIndex == 0;
        }

        public void ClearManualInput()
        {
            hasSavedProjectileInput = false;
            savedAimDirection = Vector2.zero;
            savedTargetPoint = Vector2.zero;
        }

        private bool IsSelectedPlayer(UnitRosterEntry entry, UnitRosterService roster)
        {
            return entry != null && entry == FindSelectedPlayer(roster);
        }

        private bool TryGetCurrentInput(
            UnitRosterEntry player,
            Camera inputCamera,
            bool wantsInput,
            bool pointerOverUi,
            out Vector2 aimDirection,
            out Vector2 targetPoint)
        {
            aimDirection = Vector2.zero;
            targetPoint = Vector2.zero;
            if (!wantsInput || pointerOverUi || inputCamera == null)
            {
                return false;
            }

            targetPoint = GetMouseWorldPoint(inputCamera);
            aimDirection = player == null || player.Transform == null
                ? Vector2.zero
                : targetPoint - (Vector2)player.Transform.position;
            if (aimDirection.sqrMagnitude <= 0.0001f)
            {
                return false;
            }

            savedAimDirection = aimDirection;
            savedTargetPoint = targetPoint;
            hasSavedProjectileInput = true;
            return true;
        }

        private bool TryGetSkillInput(
            SkillRuntimeInstance runtime,
            bool isProjectile,
            bool pressed,
            bool held,
            bool hasCurrentInput,
            Vector2 currentAim,
            Vector2 currentTarget,
            out Vector2 aimDirection,
            out Vector2 targetPoint)
        {
            aimDirection = Vector2.zero;
            targetPoint = Vector2.zero;

            if (!isProjectile)
            {
                if (!pressed || !hasCurrentInput)
                {
                    return false;
                }

                aimDirection = currentAim;
                targetPoint = currentTarget;
                return true;
            }

            if (hasCurrentInput && held)
            {
                aimDirection = currentAim;
                targetPoint = currentTarget;
                return true;
            }

            if (runtime.IsBursting && hasSavedProjectileInput)
            {
                aimDirection = savedAimDirection;
                targetPoint = savedTargetPoint;
                return true;
            }

            return false;
        }

        private static bool HasBurstingProjectile(IReadOnlyList<SkillRuntimeInstance> skills)
        {
            if (skills == null)
            {
                return false;
            }

            for (var i = 0; i < skills.Count; i++)
            {
                var runtime = skills[i];
                if (runtime != null && runtime.Data is ProjectileSkillData && runtime.IsBursting)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool HasVisibleEnemy(UnitRosterService roster, Camera inputCamera)
        {
            if (roster == null || inputCamera == null)
            {
                return false;
            }

            var enemies = roster.Enemies;
            for (var i = 0; i < enemies.Count; i++)
            {
                var enemy = enemies[i];
                if (enemy == null || !enemy.IsAlive || enemy.Transform == null)
                {
                    continue;
                }

                var viewport = inputCamera.WorldToViewportPoint(enemy.Transform.position);
                if (viewport.z >= 0f
                    && viewport.x >= 0f
                    && viewport.x <= 1f
                    && viewport.y >= 0f
                    && viewport.y <= 1f)
                {
                    return true;
                }
            }

            return false;
        }

        private static Vector2 GetMouseWorldPoint(Camera inputCamera)
        {
            if (inputCamera == null || Mouse.current == null)
            {
                return Vector2.zero;
            }

            var mouse = Mouse.current.position.ReadValue();
            var world = inputCamera.ScreenToWorldPoint(
                new Vector3(mouse.x, mouse.y, -inputCamera.transform.position.z));
            return world;
        }

        private static bool IsMousePressed()
        {
            var mouse = Mouse.current;
            return mouse != null && mouse.leftButton.wasPressedThisFrame;
        }

        private static bool IsMouseHeld()
        {
            var mouse = Mouse.current;
            return mouse != null && mouse.leftButton.isPressed;
        }

        private static bool IsPointerOverUi()
        {
            return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
        }
    }
}
