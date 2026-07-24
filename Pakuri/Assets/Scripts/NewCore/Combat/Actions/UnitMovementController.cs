using System;
using Pakuri.NewCore.Units.Models;

namespace Pakuri.NewCore.Combat.Actions
{
    public sealed class UnitMovementController
    {
        public bool MoveTowards(
            UnitBaseModel unit,
            CombatVector2 target,
            float moveSpeed,
            float deltaTime,
            float stopDistance)
        {
            if (unit == null)
            {
                throw new ArgumentNullException(nameof(unit));
            }

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

        public bool Displace(UnitBaseModel unit, CombatVector2 displacement)
        {
            if (unit == null)
            {
                throw new ArgumentNullException(nameof(unit));
            }
            if (float.IsNaN(displacement.X)
                || float.IsInfinity(displacement.X)
                || float.IsNaN(displacement.Y)
                || float.IsInfinity(displacement.Y))
            {
                throw new ArgumentOutOfRangeException(nameof(displacement));
            }
            if (!unit.IsAlive || !unit.CanMove || displacement.SqrMagnitude <= 0f)
            {
                return false;
            }

            unit.SetPosition(unit.Position + displacement);
            return true;
        }

        private static void ValidateNonNegativeFinite(float value, string parameterName)
        {
            if (value < 0f || float.IsNaN(value) || float.IsInfinity(value))
            {
                throw new ArgumentOutOfRangeException(parameterName);
            }
        }
    }
}
