/*
 * 역할: Stage 시작 시 유물 효과와 시너지 개수를 준비한다.
 * 책임: 보유 유물을 집계하고 정령계약 개별 유물 Effect를 대상 유닛에 배포한다.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using Pakuri.Combat;
using Pakuri.Data;
using UnityEngine;

namespace Pakuri.InGame
{
    public sealed class ArtifactSynergyManager
    {
        public SynergyState Synergies { get; } = new SynergyState();

        public void PrepareStage(
            RunSession session,
            GameDataCatalog catalog = null,
            UnitSpawnManager spawnManager = null)
        {
            if (session == null)
            {
                throw new ArgumentNullException(nameof(session));
            }

            catalog ??= GameDataLoader.CurrentCatalog;
            Synergies.Clear();

            for (var i = 0; i < session.PartyMembers.Count; i++)
            {
                session.PartyMembers[i].Artifacts.ClearActiveEffects();
            }

            for (var ownerIndex = 0; ownerIndex < session.PartyMembers.Count; ownerIndex++)
            {
                var owner = session.PartyMembers[ownerIndex];
                for (var artifactIndex = 0;
                    artifactIndex < owner.Artifacts.OwnedArtifactNames.Count;
                    artifactIndex++)
                {
                    var artifactName = owner.Artifacts.OwnedArtifactNames[artifactIndex];
                    var artifact = catalog.GetData<ArtifactDefinition>(artifactName)
                        ?? throw new InvalidOperationException(
                            $"Artifact data '{artifactName}' is required before preparing a Stage.");
                    Synergies.Add(artifact.SynergyName);
                }
            }

            DistributeSynergyEffects(session, catalog);

            var dominantAttribute = ResolvePartyDominantAttribute(session, catalog);
            var representativeAttributeCount =
                CountDistinctRepresentativeAttributes(session, catalog);

            for (var ownerIndex = 0; ownerIndex < session.PartyMembers.Count; ownerIndex++)
            {
                var owner = session.PartyMembers[ownerIndex];
                for (var artifactIndex = 0;
                    artifactIndex < owner.Artifacts.OwnedArtifactNames.Count;
                    artifactIndex++)
                {
                    var artifactName = owner.Artifacts.OwnedArtifactNames[artifactIndex];
                    var artifact = catalog.GetData<ArtifactDefinition>(artifactName)
                        ?? throw new InvalidOperationException(
                            $"Artifact data '{artifactName}' is required before preparing a Stage.");

                    DistributeEffects(
                        session,
                        owner,
                        artifact,
                        dominantAttribute,
                        representativeAttributeCount);
                }
            }

            var counts = string.Join(
                ", ",
                Synergies.Counts
                    .OrderBy(pair => pair.Key, StringComparer.OrdinalIgnoreCase)
                    .Select(pair => $"{pair.Key}={pair.Value}"));
            Debug.Log($"[ArtifactSynergy] {(counts.Length > 0 ? counts : "none")}");

            if (spawnManager != null)
            {
                ActivateStageEffects(session, catalog, spawnManager);
            }
        }

        private void DistributeSynergyEffects(
            RunSession session,
            GameDataCatalog catalog)
        {
            foreach (var pair in Synergies.Counts)
            {
                var synergy = catalog.GetData<ArtifactSynergyDefinition>(pair.Key);
                if (synergy == null)
                {
                    continue;
                }

                for (var levelIndex = 0; levelIndex < synergy.Levels.Length; levelIndex++)
                {
                    var level = synergy.Levels[levelIndex];
                    if (level == null || level.RequiredCount > pair.Value)
                    {
                        continue;
                    }

                    for (var effectIndex = 0; effectIndex < level.Effects.Length; effectIndex++)
                    {
                        var effect = level.Effects[effectIndex];
                        if (effect == null
                            || (effect.ApplicationMode != ArtifactEffectApplicationMode.SkillModifier
                                && effect.ApplicationMode != ArtifactEffectApplicationMode.PassiveTrigger))
                        {
                            continue;
                        }

                        for (var memberIndex = 0; memberIndex < session.PartyMembers.Count; memberIndex++)
                        {
                            var member = session.PartyMembers[memberIndex];
                            if (effect.Recipient == ArtifactEffectRecipient.AllAllies
                                || (effect.Recipient == ArtifactEffectRecipient.SpecificMonster
                                    && string.Equals(
                                        member.MonsterName,
                                        effect.RecipientMonsterName,
                                        StringComparison.OrdinalIgnoreCase)))
                            {
                                AddEffect(member, effect.EffectName, 1, effect.Nodes);
                            }
                        }
                    }
                }
            }
        }

        private void ActivateStageEffects(
            RunSession session,
            GameDataCatalog catalog,
            UnitSpawnManager spawnManager)
        {
            spawnManager.DespawnSummons();

            SummonDefinition summon = null;
            var learnedSkills = new List<SkillDefinition>();
            var learnedSkillNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var pair in Synergies.Counts)
            {
                var synergy = catalog.GetData<ArtifactSynergyDefinition>(pair.Key);
                if (synergy == null)
                {
                    continue;
                }

                for (var levelIndex = 0; levelIndex < synergy.Levels.Length; levelIndex++)
                {
                    var level = synergy.Levels[levelIndex];
                    if (level == null || level.RequiredCount > pair.Value)
                    {
                        continue;
                    }

                    for (var effectIndex = 0; effectIndex < level.Effects.Length; effectIndex++)
                    {
                        var effect = level.Effects[effectIndex];
                        if (effect == null)
                        {
                            continue;
                        }

                        if (effect.ApplicationMode == ArtifactEffectApplicationMode.SpawnUnit
                            && effect.SpawnSummon != null
                            && summon == null)
                        {
                            summon = effect.SpawnSummon;
                        }

                        if (effect.ApplicationMode == ArtifactEffectApplicationMode.GrantSkill
                            && effect.Recipient == ArtifactEffectRecipient.Summon
                            && effect.OutcomeSkill != null
                            && learnedSkillNames.Add(effect.OutcomeSkill.SkillName))
                        {
                            learnedSkills.Add(effect.OutcomeSkill);
                        }
                    }
                }
            }

            if (summon == null)
            {
                return;
            }

            spawnManager.SpawnTemporarySummon(
                summon,
                learnedSkills,
                ResolveSummonSkillAttribute(session, catalog));
        }

        private void DistributeEffects(
            RunSession session,
            RunSession.RunMonsterState owner,
            ArtifactDefinition artifact,
            DamageAttribute? dominantAttribute,
            int representativeAttributeCount)
        {
            for (var effectIndex = 0; effectIndex < artifact.Effects.Length; effectIndex++)
            {
                var effect = artifact.Effects[effectIndex];
                if (effect == null)
                {
                    continue;
                }

                if (effect.SelectionRule == ArtifactEffectSelectionRule.PartyDominantAttribute
                    && (!dominantAttribute.HasValue
                        || !EffectMatchesAttribute(effect, dominantAttribute.Value)))
                {
                    continue;
                }

                var repeatCount = GetEffectRepeatCount(
                    artifact,
                    effect,
                    representativeAttributeCount);
                if (effect.Recipient == ArtifactEffectRecipient.Owner)
                {
                    AddEffect(owner, effect.EffectName, repeatCount, effect.Nodes);
                    continue;
                }

                if (effect.Recipient == ArtifactEffectRecipient.Stage)
                {
                    AddEffect(owner, effect.EffectName, repeatCount, effect.Nodes);
                    continue;
                }

                for (var memberIndex = 0;
                    memberIndex < session.PartyMembers.Count;
                    memberIndex++)
                {
                    var member = session.PartyMembers[memberIndex];
                    if (effect.Recipient == ArtifactEffectRecipient.AllAllies
                        || (effect.Recipient == ArtifactEffectRecipient.SpecificMonster
                            && string.Equals(
                                member.MonsterName,
                                effect.RecipientMonsterName,
                                StringComparison.OrdinalIgnoreCase)))
                    {
                        AddEffect(member, effect.EffectName, repeatCount, effect.Nodes);
                    }
                }
            }
        }

        private int GetEffectRepeatCount(
            ArtifactDefinition artifact,
            ArtifactEffectDefinition effect,
            int representativeAttributeCount)
        {
            if (effect.RepeatRule == ArtifactEffectRepeatRule.SynergyArtifactCount)
            {
                return Synergies.GetCount(artifact.SynergyName);
            }

            return effect.RepeatRule == ArtifactEffectRepeatRule.DistinctRepresentativeAttributeCount
                ? representativeAttributeCount
                : 1;
        }

        private static void AddEffect(
            RunSession.RunMonsterState member,
            string effectName,
            int repeatCount,
            IReadOnlyList<SkillNode> nodes = null)
        {
            for (var i = 0; i < repeatCount; i++)
            {
                member.Artifacts.AddActiveEffect(effectName);
            }

            if (nodes == null)
            {
                return;
            }

            for (var i = 0; i < nodes.Count; i++)
            {
                var progression = nodes[i]?.GetOperation<FateCoinCritChanceProgressionOp>();
                if (progression.HasValue)
                {
                    member.Artifacts.ConfigureFateCoin(
                        progression.Value.Increment,
                        progression.Value.MaxBonus);
                    return;
                }
            }
        }

        private static bool EffectMatchesAttribute(
            ArtifactEffectDefinition effect,
            DamageAttribute attribute)
        {
            for (var i = 0; i < effect.Nodes.Length; i++)
            {
                var condition = effect.Nodes[i]?.GetOperation<SkillAttributeConditionOp>();
                if (condition.HasValue)
                {
                    return condition.Value.Attribute == attribute;
                }
            }

            return false;
        }

        private static DamageAttribute? ResolvePartyDominantAttribute(
            RunSession session,
            GameDataCatalog catalog)
        {
            var counts = new int[Enum.GetValues(typeof(DamageAttribute)).Length];
            for (var memberIndex = 0; memberIndex < session.PartyMembers.Count; memberIndex++)
            {
                CountLearnedElementalSkills(session.PartyMembers[memberIndex], catalog, counts);
            }

            var maximum = 0;
            for (var i = (int)DamageAttribute.Fire; i < counts.Length; i++)
            {
                maximum = Math.Max(maximum, counts[i]);
            }

            if (maximum == 0)
            {
                return null;
            }

            for (var slot = SkillSlot.A; slot <= SkillSlot.E; slot++)
            {
                for (var memberIndex = 0;
                    memberIndex < session.PartyMembers.Count;
                    memberIndex++)
                {
                    if (TryGetLearnedSkill(
                            session.PartyMembers[memberIndex],
                            catalog,
                            slot,
                            out var skill)
                        && skill.Element != DamageAttribute.Physical
                        && counts[(int)skill.Element] == maximum)
                    {
                        return skill.Element;
                    }
                }
            }

            return null;
        }

        private static DamageAttribute ResolveSummonSkillAttribute(
            RunSession session,
            GameDataCatalog catalog)
        {
            var counts = new int[Enum.GetValues(typeof(DamageAttribute)).Length];
            for (var memberIndex = 0; memberIndex < session.PartyMembers.Count; memberIndex++)
            {
                for (var slot = SkillSlot.A; slot <= SkillSlot.E; slot++)
                {
                    if (TryGetLearnedSkill(
                            session.PartyMembers[memberIndex],
                            catalog,
                            slot,
                            out var skill))
                    {
                        counts[(int)skill.Element]++;
                    }
                }
            }

            var maximum = 0;
            for (var i = 0; i < counts.Length; i++)
            {
                maximum = Math.Max(maximum, counts[i]);
            }

            if (maximum == 0)
            {
                return DamageAttribute.Physical;
            }

            for (var slot = SkillSlot.A; slot <= SkillSlot.E; slot++)
            {
                for (var memberIndex = 0; memberIndex < session.PartyMembers.Count; memberIndex++)
                {
                    if (TryGetLearnedSkill(
                            session.PartyMembers[memberIndex],
                            catalog,
                            slot,
                            out var skill)
                        && counts[(int)skill.Element] == maximum)
                    {
                        return skill.Element;
                    }
                }
            }

            return DamageAttribute.Physical;
        }

        private static int CountDistinctRepresentativeAttributes(
            RunSession session,
            GameDataCatalog catalog)
        {
            var found = new bool[Enum.GetValues(typeof(DamageAttribute)).Length];
            var count = 0;
            for (var memberIndex = 0; memberIndex < session.PartyMembers.Count; memberIndex++)
            {
                var attribute = ResolveRepresentativeAttribute(
                    session.PartyMembers[memberIndex],
                    catalog);
                if (attribute.HasValue && !found[(int)attribute.Value])
                {
                    found[(int)attribute.Value] = true;
                    count++;
                }
            }

            return count;
        }

        private static DamageAttribute? ResolveRepresentativeAttribute(
            RunSession.RunMonsterState member,
            GameDataCatalog catalog)
        {
            var counts = new int[Enum.GetValues(typeof(DamageAttribute)).Length];
            CountLearnedElementalSkills(member, catalog, counts);

            var maximum = 0;
            for (var i = (int)DamageAttribute.Fire; i < counts.Length; i++)
            {
                maximum = Math.Max(maximum, counts[i]);
            }

            if (maximum == 0)
            {
                return null;
            }

            for (var slot = SkillSlot.A; slot <= SkillSlot.E; slot++)
            {
                if (TryGetLearnedSkill(
                        member,
                        catalog,
                        slot,
                        out var skill)
                    && skill.Element != DamageAttribute.Physical
                    && counts[(int)skill.Element] == maximum)
                {
                    return skill.Element;
                }
            }

            return null;
        }

        private static void CountLearnedElementalSkills(
            RunSession.RunMonsterState member,
            GameDataCatalog catalog,
            int[] counts)
        {
            for (var slot = SkillSlot.A; slot <= SkillSlot.E; slot++)
            {
                if (TryGetLearnedSkill(
                        member,
                        catalog,
                        slot,
                        out var skill)
                    && skill.Element != DamageAttribute.Physical)
                {
                    counts[(int)skill.Element]++;
                }
            }
        }

        private static bool TryGetLearnedSkill(
            RunSession.RunMonsterState member,
            GameDataCatalog catalog,
            SkillSlot slot,
            out SkillDefinition skill)
        {
            skill = catalog.GetActiveSkill(member.MonsterName, slot);
            return skill != null && member.Skills.HasActiveSkill(skill.SkillName);
        }
    }
}
