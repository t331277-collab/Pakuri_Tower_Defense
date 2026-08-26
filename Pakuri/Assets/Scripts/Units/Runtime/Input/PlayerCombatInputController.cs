/*
 * 역할: 플레이어 전투 입력.
 * 책임: 수동 스킬 입력·조준·대상을 판정하고 선택 플레이어의 자동 스킬 모드를 관리한다.
 */

using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Pakuri.InGame
{

    /// 플레이어 입력을 선택 유닛의 조준·스킬 사용·자동 전투 명령으로 연결한다.
    public class PlayerCombatInputController : MonoBehaviour
    {
        [SerializeField] private Camera inputCamera;
        [SerializeField] private bool autoSkillEnabled;

        private bool hasSavedProjectileInput;
        private Vector2 savedAimDirection;
        private Vector2 savedTargetPoint;

        public bool AutoSkillEnabled => autoSkillEnabled;
        public event Action ManualInputDetected;
        public event Action<bool> AutoSkillChanged;

        internal void HandleManualInput(
            UnitSpawnManager roster,
            SkillExecution skillExecution,
            InGameCombatManager combatManager)
        {
            if (autoSkillEnabled)
            {
                return;
            }

            var player = FindSelectedPlayer(roster);
            if (player == null)
            {
                ClearManualInput();
                return;
            }

            var mouse = Mouse.current;
            var pressed = mouse != null && mouse.leftButton.wasPressedThisFrame;
            var held = mouse != null && mouse.leftButton.isPressed;
            var pointerOverUi = EventSystem.current != null
                && EventSystem.current.IsPointerOverGameObject();
            var hasInput = TryGetCurrentInput(
                player,
                pressed || held,
                pointerOverUi,
                out var currentAim,
                out var currentTarget);
            var activeSkills = player.Model.SkillState.ActiveSkills;

            if (hasInput)
            {
                ManualInputDetected?.Invoke();
            }

            if (!hasInput && !HasBurstingProjectile(activeSkills))
            {
                ClearManualInput();
                return;
            }

            for (var i = 0; i < activeSkills.Count; i++)
            {
                var runtime = activeSkills[i];
                var isProjectile = runtime.Data is ProjectileSkillDefinition;

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
                    aim,
                    target);
            }

            if (!held && !HasBurstingProjectile(activeSkills))
            {
                ClearManualInput();
            }
        }

        public bool CanUseAutoSkill(
            CombatUnitEntry entry,
            UnitSpawnManager roster)
        {
            if (entry.Model is EnemyCombatState)
            {
                return false;
            }

            if (!HasVisibleEnemy(roster)
                || !entry.Model.AutoSkillEnabled)
            {
                return false;
            }

            return entry != FindSelectedPlayer(roster) || autoSkillEnabled;
        }

        public void ToggleAutoSkillMode(UnitSpawnManager roster)
        {
            autoSkillEnabled = !autoSkillEnabled;

            ApplyAutoSkillModeToSelectedPlayer(roster);
            AutoSkillChanged?.Invoke(autoSkillEnabled);
        }

        public void ApplyAutoSkillModeToSelectedPlayer(UnitSpawnManager roster)
        {
            var player = FindSelectedPlayer(roster);
            if (player != null)
            {
                player.Model.AutoSkillEnabled = autoSkillEnabled;
            }
        }

        public CombatUnitEntry FindSelectedPlayer(UnitSpawnManager roster)
        {
            var players = roster.Players;
            for (var i = 0; i < players.Count; i++)
            {
                var entry = players[i];
                if (IsSelectedPlayerModel(entry.Model))
                {
                    return entry;
                }
            }

            return null;
        }

        public static bool IsSelectedPlayerModel(UnitCombatState model)
        {
            return model.Identity.Side == UnitSide.Player
                && model.Identity.Role == UnitRole.Monster
                && model.Identity.SlotIndex == 0;
        }

        public void ClearManualInput()
        {
            hasSavedProjectileInput = false;
            savedAimDirection = Vector2.zero;
            savedTargetPoint = Vector2.zero;
        }

        private bool TryGetCurrentInput(
            CombatUnitEntry player,
            bool wantsInput,
            bool pointerOverUi,
            out Vector2 aimDirection,
            out Vector2 targetPoint)
        {
            aimDirection = Vector2.zero;
            targetPoint = Vector2.zero;
            if (!wantsInput || pointerOverUi)
            {
                return false;
            }

            if (Mouse.current != null)
            {
                var mouse = Mouse.current.position.ReadValue();
                targetPoint = inputCamera.ScreenToWorldPoint(
                    new Vector3(mouse.x, mouse.y, -inputCamera.transform.position.z));
            }

            aimDirection = targetPoint - (Vector2)player.Transform.position;
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
            SkillExecutionState runtime,
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

        private static bool HasBurstingProjectile(IReadOnlyList<SkillExecutionState> skills)
        {
            for (var i = 0; i < skills.Count; i++)
            {
                var runtime = skills[i];
                if (runtime.Data is ProjectileSkillDefinition && runtime.IsBursting)
                {
                    return true;
                }
            }

            return false;
        }

        private bool HasVisibleEnemy(UnitSpawnManager roster)
        {
            var enemies = roster.Enemies;
            for (var i = 0; i < enemies.Count; i++)
            {
                var enemy = enemies[i];
                if (!enemy.IsAlive)
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

    }
}
