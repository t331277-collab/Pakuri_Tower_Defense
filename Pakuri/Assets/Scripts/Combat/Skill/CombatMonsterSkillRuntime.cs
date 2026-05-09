using System;
using System.Collections.Generic;
using Pakuri.Run;

namespace Pakuri.Combat
{
    public partial class CombatRuntimeController
    {
        private interface IMonsterSkillRuntime
        {
            string MonsterId { get; }
            void ConfigureSelectionState(RunSession session);
            void ResetCombatState();
            void UpdateCooldowns();
            void UpdateEffects();
            bool TryTriggerAutomaticSkills();
            int GetMagazineCapacity(int fallback);
            float GetActionSpeedMultiplier(float fallback);
        }

        private abstract class MonsterSkillRuntimeBase : IMonsterSkillRuntime
        {
            protected MonsterSkillRuntimeBase(CombatRuntimeController controller, string monsterId)
            {
                Controller = controller ?? throw new ArgumentNullException(nameof(controller));
                MonsterId = monsterId;
            }

            protected CombatRuntimeController Controller { get; }
            public string MonsterId { get; }
            public virtual void ConfigureSelectionState(RunSession session) { }
            public virtual void ResetCombatState() { }
            public virtual void UpdateCooldowns() { }
            public virtual void UpdateEffects() { }
            public virtual bool TryTriggerAutomaticSkills() => false;
            public virtual int GetMagazineCapacity(int fallback) => fallback;
            public virtual float GetActionSpeedMultiplier(float fallback) => fallback;
        }

        private sealed class EveMonsterSkillRuntime : MonsterSkillRuntimeBase
        {
            public EveMonsterSkillRuntime(CombatRuntimeController controller)
                : base(controller, "eve")
            {
            }

            public override void ConfigureSelectionState(RunSession session) => Controller.ConfigureEveSkillSelectionState(session);
            public override void ResetCombatState() => Controller.ResetEveSkillCombatTimers();
            public override void UpdateCooldowns() => Controller.UpdateEveSkillCooldowns();
            public override void UpdateEffects() => Controller.UpdateEveSkillEffects();
            public override bool TryTriggerAutomaticSkills() => Controller.TryTriggerEveAutomaticSkills();
            public override int GetMagazineCapacity(int fallback) => Controller.GetEveArcMagazineCapacity();
            public override float GetActionSpeedMultiplier(float fallback) => Controller.GetEveActionSpeedMultiplier();
        }

        private sealed class ArielMonsterSkillRuntime : MonsterSkillRuntimeBase
        {
            public ArielMonsterSkillRuntime(CombatRuntimeController controller)
                : base(controller, "ariel")
            {
            }

            public override void ResetCombatState() => Controller.ResetArielSkillCombatTimers();
            public override void UpdateCooldowns() => Controller.UpdateArielSkillCooldowns();
            public override void UpdateEffects() => Controller.UpdateArielSkillEffects();
            public override bool TryTriggerAutomaticSkills() => Controller.TryTriggerArielAutomaticSkills();
            public override int GetMagazineCapacity(int fallback) => Controller.GetArielJudgementMagazineCapacity();
            public override float GetActionSpeedMultiplier(float fallback) => Controller.GetArielActionSpeedMultiplier();
        }

        private sealed class RinMonsterSkillRuntime : MonsterSkillRuntimeBase
        {
            public RinMonsterSkillRuntime(CombatRuntimeController controller)
                : base(controller, "rin")
            {
            }

            public override void ResetCombatState() => Controller.ResetRinSkillCombatTimers();
            public override void UpdateCooldowns() => Controller.UpdateRinSkillCooldowns();
            public override void UpdateEffects() => Controller.UpdateRinSkillEffects();
            public override bool TryTriggerAutomaticSkills() => Controller.TryTriggerRinAutomaticSkills();
            public override int GetMagazineCapacity(int fallback) => Controller.GetRinShatteringFistMagazineCapacity();
            public override float GetActionSpeedMultiplier(float fallback) => Controller.GetRinActionSpeedMultiplier();
        }

        private sealed class SeinMonsterSkillRuntime : MonsterSkillRuntimeBase
        {
            public SeinMonsterSkillRuntime(CombatRuntimeController controller)
                : base(controller, "sein")
            {
            }

            public override void ResetCombatState() => Controller.ResetSeinSkillCombatTimers();
            public override void UpdateCooldowns() => Controller.UpdateSeinSkillCooldowns();
            public override void UpdateEffects() => Controller.UpdateSeinSkillEffects();
            public override bool TryTriggerAutomaticSkills() => Controller.TryTriggerSeinAutomaticSkills();
            public override int GetMagazineCapacity(int fallback) => Controller.GetSeinScorchingArrowMagazineCapacity();
            public override float GetActionSpeedMultiplier(float fallback) => Controller.GetSeinActionSpeedMultiplier();
        }

        private sealed class VegaMonsterSkillRuntime : MonsterSkillRuntimeBase
        {
            public VegaMonsterSkillRuntime(CombatRuntimeController controller)
                : base(controller, "vega")
            {
            }

            public override void ResetCombatState() => Controller.ResetVegaSkillCombatTimers();
            public override void UpdateCooldowns() => Controller.UpdateVegaSkillCooldowns();
            public override void UpdateEffects() => Controller.UpdateVegaSkillEffects();
            public override bool TryTriggerAutomaticSkills() => Controller.TryTriggerVegaAutomaticSkills();
            public override int GetMagazineCapacity(int fallback) => Controller.GetVegaThreeSwordFlurryMagazineCapacity();
            public override float GetActionSpeedMultiplier(float fallback) => Controller.GetVegaActionSpeedMultiplier();
        }

        private IMonsterSkillRuntime[] monsterSkillRuntimes = Array.Empty<IMonsterSkillRuntime>();
        private readonly Dictionary<string, IMonsterSkillRuntime> monsterSkillRuntimeLookup = new Dictionary<string, IMonsterSkillRuntime>(StringComparer.OrdinalIgnoreCase);

        private void EnsureMonsterSkillRuntimes()
        {
            if (monsterSkillRuntimes.Length > 0)
            {
                return;
            }

            monsterSkillRuntimes = new IMonsterSkillRuntime[]
            {
                new EveMonsterSkillRuntime(this),
                new ArielMonsterSkillRuntime(this),
                new RinMonsterSkillRuntime(this),
                new SeinMonsterSkillRuntime(this),
                new VegaMonsterSkillRuntime(this),
            };

            monsterSkillRuntimeLookup.Clear();
            for (var i = 0; i < monsterSkillRuntimes.Length; i++)
            {
                var runtime = monsterSkillRuntimes[i];
                if (runtime != null && !string.IsNullOrWhiteSpace(runtime.MonsterId))
                {
                    monsterSkillRuntimeLookup[runtime.MonsterId] = runtime;
                }
            }
        }

        private IMonsterSkillRuntime GetSelectedMonsterSkillRuntime()
        {
            EnsureMonsterSkillRuntimes();
            if (selectedMonster == null || string.IsNullOrWhiteSpace(selectedMonster.MonsterId))
            {
                return null;
            }

            return monsterSkillRuntimeLookup.TryGetValue(selectedMonster.MonsterId, out var runtime)
                ? runtime
                : null;
        }

        private void ConfigureMonsterSkillRuntimeSelectionState(RunSession session)
        {
            EnsureMonsterSkillRuntimes();
            for (var i = 0; i < monsterSkillRuntimes.Length; i++)
            {
                monsterSkillRuntimes[i]?.ConfigureSelectionState(session);
            }
        }

        private void ResetMonsterSkillRuntimes()
        {
            EnsureMonsterSkillRuntimes();
            for (var i = 0; i < monsterSkillRuntimes.Length; i++)
            {
                monsterSkillRuntimes[i]?.ResetCombatState();
            }
        }

        private void UpdateMonsterSkillRuntimeEffects()
        {
            EnsureMonsterSkillRuntimes();
            for (var i = 0; i < monsterSkillRuntimes.Length; i++)
            {
                monsterSkillRuntimes[i]?.UpdateEffects();
            }
        }
    }
}
