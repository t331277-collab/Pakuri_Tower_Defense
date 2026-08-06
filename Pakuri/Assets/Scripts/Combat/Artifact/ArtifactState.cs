/*
 * 역할: 한 유닛의 유물 보유 상태.
 * 책임: 최대 세 개의 보유 유물과 현재 Stage에서 적용받는 Effect Name를 분리해 보관한다.
 */

using System;
using System.Collections.Generic;

namespace Pakuri.InGame
{
    [Serializable]
    public sealed class ArtifactState
    {
        public const int MaxOwnedArtifactCount = 3;

        private readonly List<string> ownedArtifactNames = new List<string>();
        private readonly List<string> activeArtifactEffectNames = new List<string>();

        public IReadOnlyList<string> OwnedArtifactNames => ownedArtifactNames;
        public IReadOnlyList<string> ActiveArtifactEffectNames => activeArtifactEffectNames;

        public bool CanAdd(string artifactName)
        {
            return !string.IsNullOrWhiteSpace(artifactName)
                && ownedArtifactNames.Count < MaxOwnedArtifactCount;
        }

        public bool TryAdd(string artifactName)
        {
            if (!CanAdd(artifactName))
            {
                return false;
            }

            ownedArtifactNames.Add(artifactName);
            return true;
        }

        public bool Remove(string artifactName)
        {
            return !string.IsNullOrWhiteSpace(artifactName)
                && ownedArtifactNames.Remove(artifactName);
        }

        public void ClearActiveEffects()
        {
            activeArtifactEffectNames.Clear();
        }

        internal void AddActiveEffect(string effectName)
        {
            if (!string.IsNullOrWhiteSpace(effectName))
            {
                activeArtifactEffectNames.Add(effectName);
            }
        }
    }
}
