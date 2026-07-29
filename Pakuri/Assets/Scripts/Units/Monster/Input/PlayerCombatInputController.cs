/*
 * 역할: 플레이어 전투 입력.
 * 책임: 수동 스킬 입력·조준·대상을 판정하고 선택 플레이어의 자동 스킬 모드를 관리한다.
 */

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Pakuri.InGame
{

    /// <summary><c>PlayerCombatInputController</c>가 담당하는 입력 또는 표시 흐름을 조정하고 관련 런타임 상태를 갱신한다.</summary>
    public class PlayerCombatInputController : MonoBehaviour
    {
        [SerializeField] private Camera inputCamera;
        [SerializeField] private bool autoSkillEnabled;

        private bool hasSavedProjectileInput;
        private Vector2 savedAimDirection;
        private Vector2 savedTargetPoint;

        public bool AutoSkillEnabled => autoSkillEnabled;

        /// <summary>전달된 런타임 입력값을 사용해 <c>ManualInput</c>를 처리한다.</summary>
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

        /// <summary>전달된 런타임 입력값을 사용해 <c>UseAutoSkill</c> 실행 가능 여부를 반환한다.</summary>
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

        /// <summary>전달된 <c>roster</c> 값을 사용해 <c>AutoSkillMode</c>를 활성 상태를 전환한다.</summary>
        public void ToggleAutoSkillMode(UnitSpawnManager roster)
        {
            autoSkillEnabled = !autoSkillEnabled;

            ApplyAutoSkillModeToSelectedPlayer(roster);
        }

        /// <summary>전달된 <c>roster</c> 값을 사용해 <c>AutoSkillModeToSelectedPlayer</c>를 적용한다.</summary>
        public void ApplyAutoSkillModeToSelectedPlayer(UnitSpawnManager roster)
        {
            var player = FindSelectedPlayer(roster);
            if (player != null)
            {
                player.Model.AutoSkillEnabled = autoSkillEnabled;
            }
        }

        /// <summary>전달된 <c>roster</c> 값을 사용해 <c>SelectedPlayer</c>를 찾는다.</summary>
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

        /// <summary>전달된 <c>model</c> 값을 사용해 <c>SelectedPlayerModel</c> 조건 충족 여부를 반환한다.</summary>
        public static bool IsSelectedPlayerModel(UnitCombatState model)
        {
            return model.Identity.Side == UnitSide.Player
                && model.Identity.Role == UnitRole.Monster
                && model.Identity.SlotIndex == 0;
        }

        /// <summary><c>ManualInput</c>를 소유한 런타임 상태에서 비운다.</summary>
        public void ClearManualInput()
        {
            hasSavedProjectileInput = false;
            savedAimDirection = Vector2.zero;
            savedTargetPoint = Vector2.zero;
        }

        /// <summary>전달된 런타임 입력값을 사용해 <c>CurrentInput</c> 조회를 시도하고 값이 있는지 반환한다.</summary>
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

        /// <summary>전달된 런타임 입력값을 사용해 <c>SkillInput</c> 조회를 시도하고 값이 있는지 반환한다.</summary>
        private bool TryGetSkillInput(
            SkillUseState runtime,
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

        /// <summary>전달된 <c>skills</c> 값을 사용해 소유한 런타임 상태에 <c>BurstingProjectile</c>가 있는지 반환한다.</summary>
        private static bool HasBurstingProjectile(IReadOnlyList<SkillUseState> skills)
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

        /// <summary>전달된 <c>roster</c> 값을 사용해 소유한 런타임 상태에 <c>VisibleEnemy</c>가 있는지 반환한다.</summary>
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
