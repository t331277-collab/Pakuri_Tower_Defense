using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

/* run 보상으로 획득한 포로 항목의 등록·교체·소비를 관리한다. */
namespace Pakuri.NewCore.Run
{
    public class Prisoner
    {
        /* 보상 적 id를 소비 가능한 포로 항목으로 저장한다. */
        internal Prisoner(string enemyId)
        {
            EnemyId = enemyId;
        }

        public string EnemyId { get; }
    }

    public class PrisonerInventory
    {
        private readonly List<Prisoner> prisoners = new List<Prisoner>();
        private readonly IReadOnlyList<Prisoner> readOnlyPrisoners;

        /* 내부 포로 목록을 외부에서 변경할 수 없는 조회 목록으로 감싼다. */
        public PrisonerInventory()
        {
            readOnlyPrisoners = new ReadOnlyCollection<Prisoner>(prisoners);
        }

        public IReadOnlyList<Prisoner> Prisoners => readOnlyPrisoners;

        /* 유효한 적 id로 새 포로를 만들고 현재 보상 목록에 추가한다. */
        public Prisoner Register(string enemyId)
        {

            Prisoner prisoner = new Prisoner(enemyId);
            prisoners.Add(prisoner);
            return prisoner;
        }

        /* 입력 적 id를 모두 검증한 뒤 현재 보상 포로 목록 전체를 교체한다. */
        public void ReplaceRewards(IEnumerable<string> enemyIds)
        {

            List<string> replacement = new List<string>();
            foreach (string enemyId in enemyIds)
            {

                replacement.Add(enemyId);
            }

            prisoners.Clear();
            for (int index = 0; index < replacement.Count; index++)
            {
                prisoners.Add(new Prisoner(replacement[index]));
            }
        }

        /* 동일 포로 인스턴스가 현재 보상 목록에 있는지 확인한다. */
        public bool CanConsume(Prisoner prisoner)
        {
            return prisoner != null && prisoners.Contains(prisoner);
        }

        /* 동일 포로 인스턴스를 현재 목록에서 제거하고 성공 여부를 반환한다. */
        public bool TryConsume(Prisoner prisoner)
        {
            return prisoner != null && prisoners.Remove(prisoner);
        }

        /* 현재 보상 포로를 모두 제거한다. */
        public void Clear()
        {
            prisoners.Clear();
        }
    }
}
