using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Pakuri.NewCore.Run
{
    public sealed class Prisoner
    {
        internal Prisoner(string enemyId)
        {
            EnemyId = enemyId;
        }

        public string EnemyId { get; }
    }

    public sealed class PrisonerInventory
    {
        private readonly List<Prisoner> prisoners = new List<Prisoner>();
        private readonly IReadOnlyList<Prisoner> readOnlyPrisoners;

        public PrisonerInventory()
        {
            readOnlyPrisoners = new ReadOnlyCollection<Prisoner>(prisoners);
        }

        public IReadOnlyList<Prisoner> Prisoners => readOnlyPrisoners;

        public Prisoner Register(string enemyId)
        {
            if (string.IsNullOrWhiteSpace(enemyId))
            {
                throw new ArgumentException("Enemy id is required.", nameof(enemyId));
            }

            Prisoner prisoner = new Prisoner(enemyId);
            prisoners.Add(prisoner);
            return prisoner;
        }

        public void ReplaceRewards(IEnumerable<string> enemyIds)
        {
            if (enemyIds == null)
            {
                throw new ArgumentNullException(nameof(enemyIds));
            }

            List<string> replacement = new List<string>();
            foreach (string enemyId in enemyIds)
            {
                if (string.IsNullOrWhiteSpace(enemyId))
                {
                    throw new ArgumentException(
                        "Reward prisoner enemy ids cannot be empty.",
                        nameof(enemyIds));
                }

                replacement.Add(enemyId);
            }

            prisoners.Clear();
            for (int index = 0; index < replacement.Count; index++)
            {
                prisoners.Add(new Prisoner(replacement[index]));
            }
        }

        public bool CanConsume(Prisoner prisoner)
        {
            return prisoner != null && prisoners.Contains(prisoner);
        }

        public bool TryConsume(Prisoner prisoner)
        {
            return prisoner != null && prisoners.Remove(prisoner);
        }

        public void Clear()
        {
            prisoners.Clear();
        }
    }
}
