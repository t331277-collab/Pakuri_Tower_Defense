/*
 * 역할: 한 유닛의 유물 보유 상태.
 * 책임: 최대 세 개의 보유 유물과 현재 Stage에서 적용받는 Effect ID를 분리해 보관한다.
 */

using System;
using System.Collections.Generic;

namespace Pakuri.InGame
{
    [Serializable]
    public sealed class ArtifactState
    {
        public const int MaxOwnedArtifactCount = 3;

        private readonly List<string> ownedArtifactIds = new List<string>();
        private readonly List<string> activeArtifactEffectIds = new List<string>();

        public IReadOnlyList<string> OwnedArtifactIds => ownedArtifactIds;
        public IReadOnlyList<string> ActiveArtifactEffectIds => activeArtifactEffectIds;

        public bool CanAdd(string artifactId)
        {
            return !string.IsNullOrWhiteSpace(artifactId)
                && ownedArtifactIds.Count < MaxOwnedArtifactCount;
        }

        public bool TryAdd(string artifactId)
        {
            if (!CanAdd(artifactId))
            {
                return false;
            }

            ownedArtifactIds.Add(artifactId);
            return true;
        }

        public bool Remove(string artifactId)
        {
            return !string.IsNullOrWhiteSpace(artifactId)
                && ownedArtifactIds.Remove(artifactId);
        }

        public void ClearActiveEffects()
        {
            activeArtifactEffectIds.Clear();
        }

        internal void AddActiveEffect(string effectId)
        {
            if (!string.IsNullOrWhiteSpace(effectId))
            {
                activeArtifactEffectIds.Add(effectId);
            }
        }
    }
}
