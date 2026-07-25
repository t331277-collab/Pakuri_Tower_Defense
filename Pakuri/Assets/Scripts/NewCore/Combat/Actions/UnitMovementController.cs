using System;
using Pakuri.NewCore.Units.Models;

/* 전투 좌표에서 유닛 이동과 강제 변위를 계산해 모델에 반영한다. */
namespace Pakuri.NewCore.Combat.Actions
{
    public class UnitMovementController
    {
        /* 유닛 이동 목표까지의 이동량을 계산해 위치에 반영한다. */
        public bool MoveTowards(
            UnitBaseModel unit,
            CombatVector2 target,
            float moveSpeed,
            float deltaTime,
            float stopDistance)
        {

            ValidateNonNegativeFinite(moveSpeed, nameof(moveSpeed));
            ValidateNonNegativeFinite(deltaTime, nameof(deltaTime));
            ValidateNonNegativeFinite(stopDistance, nameof(stopDistance));
            CombatVector2 offset = target - unit.Position;
            float distance = offset.Magnitude;
            if (distance <= stopDistance)
            {
                return true;
            }

            if (!unit.IsAlive || !unit.CanMove || moveSpeed <= 0f || deltaTime <= 0f)
            {
                return false;
            }

            float permitted = Math.Min(
                moveSpeed * unit.MoveSpeedMultiplier * deltaTime,
                distance - stopDistance);
            unit.SetPosition(unit.Position + (offset.Normalized * permitted));
            return CombatVector2.Distance(unit.Position, target) <= stopDistance;
        }

        /* 유닛 이동 강제 변위를 검증해 위치에 반영한다. */
        public bool Displace(UnitBaseModel unit, CombatVector2 displacement)
        {
            if (!unit.IsAlive || !unit.CanMove || displacement.SqrMagnitude <= 0f)
            {
                return false;
            }

            unit.SetPosition(unit.Position + displacement);
            return true;
        }

        /* 유닛 이동 입력과 상태의 필수 조건을 검증한다. */
        private static void ValidateNonNegativeFinite(float value, string parameterName)
        {
        }
    }
}
