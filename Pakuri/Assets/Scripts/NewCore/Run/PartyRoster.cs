using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Pakuri.NewCore.Units.Models;

namespace Pakuri.NewCore.Run
{
    public sealed class PartyRoster
    {
        public const int MaximumPartySlots = 5;

        private readonly List<MonsterModel> members;
        private readonly IReadOnlyList<MonsterModel> readOnlyMembers;

        public PartyRoster(MonsterModel initialMonster)
        {
            if (initialMonster == null)
            {
                throw new ArgumentNullException(nameof(initialMonster));
            }

            members = new List<MonsterModel>(MaximumPartySlots)
            {
                initialMonster
            };
            readOnlyMembers = new ReadOnlyCollection<MonsterModel>(members);
        }

        public IReadOnlyList<MonsterModel> Members => readOnlyMembers;

        public bool CanAdd(string monsterId)
        {
            if (string.IsNullOrEmpty(monsterId) || members.Count >= MaximumPartySlots)
            {
                return false;
            }

            return GetByMonsterId(monsterId) == null;
        }

        public bool TryAddManifestedMonster(MonsterModel monster)
        {
            if (monster == null || !CanAdd(monster.MonsterDefinition.id))
            {
                return false;
            }

            members.Add(monster);
            return true;
        }

        public MonsterModel GetByMonsterId(string monsterId)
        {
            if (monsterId == null)
            {
                throw new ArgumentNullException(nameof(monsterId));
            }

            for (int index = 0; index < members.Count; index++)
            {
                MonsterModel member = members[index];
                if (string.Equals(
                    member.MonsterDefinition.id,
                    monsterId,
                    StringComparison.Ordinal))
                {
                    return member;
                }
            }

            return null;
        }

        internal bool TryRemoveManifestedMonster(MonsterModel monster)
        {
            if (monster == null
                || members.Count <= 1
                || ReferenceEquals(members[0], monster))
            {
                return false;
            }

            return members.Remove(monster);
        }
    }
}
