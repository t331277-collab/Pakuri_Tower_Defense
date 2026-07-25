using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Pakuri.NewCore.Units.Models;

/* run 파티의 순서, 정원, 중복 없는 몬스터 구성을 관리한다. */
namespace Pakuri.NewCore.Run
{
    public sealed class PartyRoster
    {
        public const int MaximumPartySlots = 5;

        private readonly List<MonsterModel> members;
        private readonly IReadOnlyList<MonsterModel> readOnlyMembers;

        /* 최초 몬스터를 첫 파티원으로 넣고 최대 5칸의 읽기 전용 roster를 구성한다. */
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

        /* monster id가 비어 있지 않고 정원·중복 조건을 만족하는지 확인한다. */
        public bool CanAdd(string monsterId)
        {
            if (string.IsNullOrEmpty(monsterId) || members.Count >= MaximumPartySlots)
            {
                return false;
            }

            return GetByMonsterId(monsterId) == null;
        }

        /* 현현 몬스터가 정원과 중복 조건을 통과하면 파티 끝에 추가한다. */
        public bool TryAddManifestedMonster(MonsterModel monster)
        {
            if (monster == null || !CanAdd(monster.MonsterDefinition.id))
            {
                return false;
            }

            members.Add(monster);
            return true;
        }

        /* monster id가 일치하는 파티원을 반환하고 없으면 null을 반환한다. */
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

        /* 최초 몬스터를 보존하며 현현 몬스터를 파티에서 제거한다. */
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
