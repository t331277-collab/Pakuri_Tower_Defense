using System;
using Pakuri.NewCore.Combat.Skills.Runtime;
using Pakuri.NewCore.Definitions.Skills;
using Pakuri.NewCore.Definitions.Units;

namespace Pakuri.NewCore.Units.Models
{
    public sealed class EnemyModel : UnitBaseModel
    {
        public EnemyModel(
            EnemyDefinition definition,
            SkillDefinition slotASkill,
            SkillDefinition slotBSkill,
            PassiveDefinition passiveSkill)
            : this(
                definition,
                slotASkill,
                slotBSkill,
                passiveSkill,
                1f)
        {
        }

        public EnemyModel(
            EnemyDefinition definition,
            SkillDefinition slotASkill,
            SkillDefinition slotBSkill,
            PassiveDefinition passiveSkill,
            float maximumHealthMultiplier)
            : base(
                definition ?? throw new ArgumentNullException(nameof(definition)),
                RequiredMaximumHealth(
                    definition,
                    maximumHealthMultiplier))
        {
            EnemyDefinition = definition;
            SkillBucket = new EnemySkillBucket(
                definition,
                slotASkill,
                slotBSkill,
                passiveSkill);
            AutoAttackEnabled = true;
        }

        public EnemyDefinition EnemyDefinition { get; }

        public EnemySkillBucket SkillBucket { get; }

        public bool AutoAttackEnabled { get; private set; }

        public bool HasContactedNexus { get; private set; }

        public void SetAutoAttackEnabled(bool enabled)
        {
            AutoAttackEnabled = enabled;
        }

        public void MarkNexusContact()
        {
            HasContactedNexus = true;
        }

        public void ResetForNextDay()
        {
            ResetVitalsAndStatuses();
            SkillBucket.ResetRuntimeState();
            AutoAttackEnabled = true;
            HasContactedNexus = false;
        }

        private static float RequiredMaximumHealth(
            EnemyDefinition definition,
            float multiplier)
        {
            if (definition == null)
            {
                throw new ArgumentNullException(nameof(definition));
            }

            if (multiplier <= 0f
                || float.IsNaN(multiplier)
                || float.IsInfinity(multiplier))
            {
                throw new ArgumentOutOfRangeException(nameof(multiplier));
            }

            return (definition.max_health
                ?? throw new ArgumentException(
                    "Enemy definition has no max_health.",
                    nameof(definition)))
                * multiplier;
        }
    }
}
