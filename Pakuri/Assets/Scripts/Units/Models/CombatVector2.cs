using System;

namespace Pakuri.NewCore.Units.Models
{
    public readonly struct CombatVector2 : IEquatable<CombatVector2>
    {
        public CombatVector2(float x, float y)
        {
            if (!IsFinite(x) || !IsFinite(y))
            {
                throw new ArgumentOutOfRangeException(nameof(x));
            }

            X = x;
            Y = y;
        }

        public float X { get; }

        public float Y { get; }

        public float SqrMagnitude => (X * X) + (Y * Y);

        public float Magnitude => (float)Math.Sqrt(SqrMagnitude);

        public CombatVector2 Normalized
        {
            get
            {
                float magnitude = Magnitude;
                return magnitude <= 0.00001f
                    ? default
                    : new CombatVector2(X / magnitude, Y / magnitude);
            }
        }

        public static CombatVector2 operator +(CombatVector2 left, CombatVector2 right)
        {
            return new CombatVector2(left.X + right.X, left.Y + right.Y);
        }

        public static CombatVector2 operator -(CombatVector2 left, CombatVector2 right)
        {
            return new CombatVector2(left.X - right.X, left.Y - right.Y);
        }

        public static CombatVector2 operator *(CombatVector2 value, float multiplier)
        {
            return new CombatVector2(value.X * multiplier, value.Y * multiplier);
        }

        public static float Distance(CombatVector2 left, CombatVector2 right)
        {
            return (left - right).Magnitude;
        }

        public bool Equals(CombatVector2 other)
        {
            return X.Equals(other.X) && Y.Equals(other.Y);
        }

        public override bool Equals(object obj)
        {
            return obj is CombatVector2 other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y);
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }
    }
}
