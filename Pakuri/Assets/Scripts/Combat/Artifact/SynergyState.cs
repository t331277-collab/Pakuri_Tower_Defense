/*
 * 역할: 현재 파티의 유물 시너지 집계 상태.
 * 책임: Phase 3에서 시너지별 보유 개수만 보관한다.
 */

using System;
using System.Collections.Generic;

namespace Pakuri.InGame
{
    [Serializable]
    public sealed class SynergyState
    {
        private readonly Dictionary<string, int> counts =
            new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        public IReadOnlyDictionary<string, int> Counts => counts;

        public int GetCount(string synergyId)
        {
            return !string.IsNullOrWhiteSpace(synergyId)
                && counts.TryGetValue(synergyId, out var count)
                    ? count
                    : 0;
        }

        internal void Clear()
        {
            counts.Clear();
        }

        internal void Add(string synergyId)
        {
            if (string.IsNullOrWhiteSpace(synergyId))
            {
                return;
            }

            counts[synergyId] = GetCount(synergyId) + 1;
        }
    }
}
