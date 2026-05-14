using Pakuri.Combat;
using UnityEngine;

namespace Pakuri.InGame
{
    public sealed class UnitResourceMutationService
    {
        public InGameResourceChangeResult ApplyDamage(
            BaseUnitRuntimeModel target,
            float baseDamage,
            DamageAttribute attribute = DamageAttribute.Physical)
        {
            if (target == null || target.Resources == null || baseDamage <= 0f)
            {
                return InGameResourceChangeResult.Unchanged(target);
            }

            var resources = target.Resources;
            var beforeHealth = Mathf.Max(0f, resources.CurrentHealth);
            var beforeShield = Mathf.Max(0f, resources.CurrentShield);
            var finalDamage = ResolveDamageAfterDefense(target, baseDamage, attribute);
            var shieldDamage = Mathf.Min(beforeShield, finalDamage);
            var remainingDamage = Mathf.Max(0f, finalDamage - shieldDamage);

            resources.CurrentShield = Mathf.Max(0f, beforeShield - shieldDamage);
            resources.CurrentHealth = Mathf.Max(0f, beforeHealth - remainingDamage);

            return new InGameResourceChangeResult(
                target,
                beforeHealth,
                resources.CurrentHealth,
                beforeShield,
                resources.CurrentShield,
                finalDamage,
                resources.CurrentHealth <= 0f);
        }

        public InGameResourceChangeResult GrantShield(BaseUnitRuntimeModel target, float amount)
        {
            if (target == null || target.Resources == null || amount <= 0f)
            {
                return InGameResourceChangeResult.Unchanged(target);
            }

            var resources = target.Resources;
            var beforeHealth = Mathf.Max(0f, resources.CurrentHealth);
            var beforeShield = Mathf.Max(0f, resources.CurrentShield);
            resources.CurrentHealth = beforeHealth;
            resources.CurrentShield = beforeShield + amount;

            return new InGameResourceChangeResult(
                target,
                beforeHealth,
                resources.CurrentHealth,
                beforeShield,
                resources.CurrentShield,
                0f,
                resources.CurrentHealth <= 0f);
        }

        public InGameResourceChangeResult SetShield(BaseUnitRuntimeModel target, float amount)
        {
            if (target == null || target.Resources == null)
            {
                return InGameResourceChangeResult.Unchanged(target);
            }

            var resources = target.Resources;
            var beforeHealth = Mathf.Max(0f, resources.CurrentHealth);
            var beforeShield = Mathf.Max(0f, resources.CurrentShield);
            resources.CurrentHealth = beforeHealth;
            resources.CurrentShield = Mathf.Max(0f, amount);

            return new InGameResourceChangeResult(
                target,
                beforeHealth,
                resources.CurrentHealth,
                beforeShield,
                resources.CurrentShield,
                0f,
                resources.CurrentHealth <= 0f);
        }

        private static float ResolveDamageAfterDefense(
            BaseUnitRuntimeModel target,
            float baseDamage,
            DamageAttribute attribute)
        {
            var defense = target.Defenses != null ? target.Defenses.Get(attribute) : 0f;
            var safeDefense = Mathf.Max(-95f, defense);
            return Mathf.Max(0f, baseDamage) * (100f / (100f + safeDefense));
        }
    }

    public readonly struct InGameResourceChangeResult
    {
        public InGameResourceChangeResult(
            BaseUnitRuntimeModel target,
            float previousHealth,
            float currentHealth,
            float previousShield,
            float currentShield,
            float appliedDamage,
            bool isDead)
        {
            Target = target;
            PreviousHealth = previousHealth;
            CurrentHealth = currentHealth;
            PreviousShield = previousShield;
            CurrentShield = currentShield;
            AppliedDamage = appliedDamage;
            IsDead = isDead;
        }

        public BaseUnitRuntimeModel Target { get; }
        public float PreviousHealth { get; }
        public float CurrentHealth { get; }
        public float PreviousShield { get; }
        public float CurrentShield { get; }
        public float AppliedDamage { get; }
        public bool IsDead { get; }
        public bool Changed =>
            !Mathf.Approximately(PreviousHealth, CurrentHealth)
            || !Mathf.Approximately(PreviousShield, CurrentShield);

        public static InGameResourceChangeResult Unchanged(BaseUnitRuntimeModel target)
        {
            var resources = target != null ? target.Resources : null;
            var health = resources != null ? Mathf.Max(0f, resources.CurrentHealth) : 0f;
            var shield = resources != null ? Mathf.Max(0f, resources.CurrentShield) : 0f;
            return new InGameResourceChangeResult(target, health, health, shield, shield, 0f, health <= 0f);
        }
    }
}
