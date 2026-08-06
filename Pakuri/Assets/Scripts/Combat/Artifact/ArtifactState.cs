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
        private float fateCoinCritChanceBonus;

        public IReadOnlyList<string> OwnedArtifactNames => ownedArtifactNames;
        public IReadOnlyList<string> ActiveArtifactEffectNames => activeArtifactEffectNames;

        internal float FateCoinCritChanceBonus => fateCoinCritChanceBonus;

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
            fateCoinCritChanceBonus = 0f;
        }

        internal bool HasActiveEffect(string effectName)
        {
            return !string.IsNullOrWhiteSpace(effectName)
                && activeArtifactEffectNames.Contains(effectName);
        }

        internal void AdvanceFateCoin(bool wasCritical)
        {
            fateCoinCritChanceBonus = wasCritical
                ? 0f
                : Math.Min(0.25f, fateCoinCritChanceBonus + 0.05f);
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
