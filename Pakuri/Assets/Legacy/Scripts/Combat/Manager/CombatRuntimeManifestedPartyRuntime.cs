using System.Collections.Generic;
using UnityEngine;

namespace Pakuri.Combat
{
    public partial class CombatRuntimeController
    {
        private readonly ManifestedPartyRuntime manifestedParty = new ManifestedPartyRuntime(MaxManifestedPartyMonsterCount);

        private List<CombatUnitRuntime> manifestedMonsters => manifestedParty.Monsters;
        private List<ManifestedDroneRuntime> manifestedDrones => manifestedParty.Drones;
        private Transform[] manifestedMonsterSlots => manifestedParty.Slots;

        private sealed class ManifestedPartyRuntime
        {
            public ManifestedPartyRuntime(int slotCount)
            {
                Slots = new Transform[Mathf.Max(0, slotCount)];
            }

            public List<CombatUnitRuntime> Monsters { get; } = new List<CombatUnitRuntime>();

            public List<ManifestedDroneRuntime> Drones { get; } = new List<ManifestedDroneRuntime>();

            public Transform[] Slots { get; }

            public int MonsterCount => Monsters.Count;

            public void AddMonster(CombatUnitRuntime runtime)
            {
                if (runtime != null)
                {
                    Monsters.Add(runtime);
                }
            }

            public void ClearMonsters()
            {
                Monsters.Clear();
            }

            public void TickCombat(CombatRuntimeController owner, float elapsed, bool battleResolved)
            {
                if (owner == null || Monsters.Count == 0 || battleResolved)
                {
                    return;
                }

                owner.UpdateManifestedDrones();
                for (var i = 0; i < Monsters.Count; i++)
                {
                    var runtime = Monsters[i];
                    if (!owner.CanTickManifestedPartyUnit(runtime))
                    {
                        continue;
                    }

                    owner.SyncManifestedPartyUnitSkills(runtime);
                    owner.TickManifestedPartyUnitCombat(runtime, elapsed);
                    owner.UpdateManifestedPartyUnitView(runtime);
                }
            }

            public void TickUnitSkill(CombatRuntimeController owner, CombatUnitRuntime runtime, CombatSkillRuntime skillRuntime, float elapsed)
            {
                if (owner == null)
                {
                    return;
                }

                owner.DispatchManifestedPartyUnitSkill(runtime, skillRuntime, elapsed);
            }
        }
    }
}
