using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

/*
 * 선택된 플레이어 몬스터의 수동 스킬 입력과 자동 사용 모드를 처리하는 컴포넌트.
 * 포인터와 스킬 키 입력을 실행 요청으로 바꾸고 적 가시성과 스킬 상태를 확인하며
 * 자동 모드 변경을 선택 유닛의 AutoSkillEnabled 값에 반영한다.
 */
namespace Pakuri.InGame
{

    public class PlayerCombatInputController : MonoBehaviour
    {
        [SerializeField] private Camera inputCamera;
        [SerializeField] private bool autoSkillEnabled;

        private bool hasSavedProjectileInput;
        private Vector2 savedAimDirection;
        private Vector2 savedTargetPoint;

        public bool AutoSkillEnabled => autoSkillEnabled;

        /*
         * 선택 플레이어의 입력을 읽고 실행 가능한 액티브 스킬에 전달한다.
         */
        internal void HandleManualInput(
            CombatUnitRegistry roster /* 전투에 등록된 유닛 목록 */,
            SkillExecution skillExecution /* 스킬 실행 */,
            InGameCombatManager combatManager /* 전투 진행 관리자 */)
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

            // 한 프레임의 마우스 상태를 한 번 읽어 모든 스킬에 같은 입력을 전달한다.
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

            // 연속 발사 중이면 새 마우스 입력이 없어도 저장된 조준으로 남은 탄을 처리한다.
            if (!hasInput && !HasBurstingProjectile(activeSkills))
            {
                ClearManualInput();
                return;
            }

            for (var i = 0; i < activeSkills.Count; i++)
            {
                var runtime = activeSkills[i];
                var isProjectile = runtime.Data is ProjectileSkillDefinition;
                // 각 스킬은 클릭·홀드·연속 발사 규칙에 맞는 입력만 선택한다.
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

        /*
         * 유닛이 자동 스킬을 사용할 수 있는 상태인지 반환한다.
         */
        public bool CanUseAutoSkill(
            CombatUnitEntry entry /* 처리할 등록 정보 */,
            CombatUnitRegistry roster /* 전투에 등록된 유닛 목록 */)
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

            // 선택 플레이어만 UI에서 정한 자동 스킬 모드를 따른다.
            return entry != FindSelectedPlayer(roster) || autoSkillEnabled;
        }

        /*
         * 선택 플레이어의 자동 스킬 사용 여부를 전환한다.
         */
        public void ToggleAutoSkillMode(CombatUnitRegistry roster /* 전투에 등록된 유닛 목록 */)
        {
            autoSkillEnabled = !autoSkillEnabled;
            // 표시 상태와 실제 플레이어 모델의 자동 스킬 설정을 함께 갱신한다.
            ApplyAutoSkillModeToSelectedPlayer(roster);
        }

        /*
         * 현재 자동 스킬 설정을 선택 플레이어 모델에 적용한다.
         */
        public void ApplyAutoSkillModeToSelectedPlayer(CombatUnitRegistry roster /* 전투에 등록된 유닛 목록 */)
        {
            var player = FindSelectedPlayer(roster);
            if (player != null)
            {
                player.Model.AutoSkillEnabled = autoSkillEnabled;
            }
        }

        /*
         * 플레이어 진영의 첫 번째 몬스터를 선택 플레이어로 찾는다.
         */
        public CombatUnitEntry FindSelectedPlayer(CombatUnitRegistry roster /* 전투에 등록된 유닛 목록 */)
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

        /*
         * 모델이 수동 입력을 받는 첫 번째 플레이어 몬스터인지 반환한다.
         */
        public static bool IsSelectedPlayerModel(UnitCombatState model /* 전투 상태를 읽거나 변경할 유닛 */)
        {
            return model.Identity.Side == UnitSide.Player
                && model.Identity.Role == UnitRole.Monster
                && model.Identity.SlotIndex == 0;
        }

        /*
         * 저장된 투사체 조준 입력을 초기화한다.
         */
        public void ClearManualInput()
        {
            hasSavedProjectileInput = false;
            savedAimDirection = Vector2.zero;
            savedTargetPoint = Vector2.zero;
        }

        /*
         * 현재 마우스 입력에서 조준 방향과 목표 지점을 만든다.
         */
        private bool TryGetCurrentInput(
            CombatUnitEntry player /* 플레이어 */,
            bool wantsInput /* 요청 입력 여부 */,
            bool pointerOverUi /* 포인터가 UI 위에 있는지 여부 */,
            out Vector2 aimDirection /* 조준 방향 */,
            out Vector2 targetPoint /* 지정한 대상 위치 */)
        {
            aimDirection = Vector2.zero;
            targetPoint = Vector2.zero;
            if (!wantsInput || pointerOverUi)
            {
                return false;
            }

            // 화면의 마우스 위치를 전투 월드 좌표로 바꿔 조준점을 만든다.
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

        /*
         * 스킬 종류와 연속 발사 상태에 맞는 조준 입력을 선택한다.
         */
        private bool TryGetSkillInput(
            SkillUseState runtime /* 실행 중인 스킬 정보 */,
            bool isProjectile /* 여부 투사체 여부 */,
            bool pressed /* 누름 여부 */,
            bool held /* 누르고 있음 여부 */,
            bool hasCurrentInput /* 보유 현재 입력 여부 */,
            Vector2 currentAim /* 현재 조준 */,
            Vector2 currentTarget /* 현재 대상 */,
            out Vector2 aimDirection /* 조준 방향 */,
            out Vector2 targetPoint /* 지정한 대상 위치 */)
        {
            aimDirection = Vector2.zero;
            targetPoint = Vector2.zero;

            if (!isProjectile)
            {
                // 비투사체 스킬은 클릭한 프레임의 현재 조준만 사용한다.
                if (!pressed || !hasCurrentInput)
                {
                    return false;
                }

                aimDirection = currentAim;
                targetPoint = currentTarget;
                return true;
            }

            // 투사체는 버튼을 누르는 동안 최신 마우스 조준을 사용한다.
            if (hasCurrentInput && held)
            {
                aimDirection = currentAim;
                targetPoint = currentTarget;
                return true;
            }

            // 버튼을 놓아도 진행 중인 연속 발사는 마지막으로 저장한 조준을 유지한다.
            if (runtime.IsBursting && hasSavedProjectileInput)
            {
                aimDirection = savedAimDirection;
                targetPoint = savedTargetPoint;
                return true;
            }

            return false;
        }

        /*
         * 연속 발사 중인 투사체 스킬이 있는지 반환한다.
         */
        private static bool HasBurstingProjectile(IReadOnlyList<SkillUseState> skills /* 스킬 목록 */)
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

        /*
         * 화면 안에 살아 있는 적이 있는지 반환한다.
         */
        private bool HasVisibleEnemy(CombatUnitRegistry roster /* 전투에 등록된 유닛 목록 */)
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
                // 카메라 뒤쪽이거나 화면 경계 밖인 적은 자동 스킬 대상으로 세지 않는다.
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
