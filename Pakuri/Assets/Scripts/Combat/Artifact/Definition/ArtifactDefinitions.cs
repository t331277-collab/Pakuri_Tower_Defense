/*
 * 역할: 유물과 유물 시너지 런타임 정의.
 * 책임: CSV에서 생성된 표시 정보, 단계, 효과 대상과 해석된 스킬·소환 참조를 보관한다.
 */

using System;
using Pakuri.InGame;
using UnityEngine;

namespace Pakuri.Data
{
    public enum ArtifactEffectApplicationMode
    {
        SkillModifier,
        PassiveTrigger,
        ExecuteSkill,
        GrantSkill,
        SpawnUnit
    }

    public enum ArtifactEffectRecipient
    {
        AllAllies,
        SpecificMonster,
        Stage,
        Summon,
        ChosenOne,
        Owner
    }

    public enum ArtifactEffectRepeatRule
    {
        None,
        SynergyArtifactCount,
        DistinctRepresentativeAttributeCount
    }

    public enum ArtifactEffectSelectionRule
    {
        None,
        PartyDominantAttribute
    }

    [Serializable]
    public sealed class ArtifactEffectDefinition
    {
        public string EffectName;
        public string ArtifactName;
        public ArtifactEffectApplicationMode ApplicationMode;
        public ArtifactEffectRecipient Recipient;
        public ArtifactEffectRepeatRule RepeatRule;
        public ArtifactEffectSelectionRule SelectionRule;
        public string RecipientMonsterName;
        public SkillDefinition TargetSkill;
        public SkillDefinition OutcomeSkill;
        public SkillNode[] Nodes = Array.Empty<SkillNode>();
        public SkillReaction[] Reactions = Array.Empty<SkillReaction>();
    }

    [Serializable]
    public sealed class ArtifactSynergyEffectDefinition
    {
        public string EffectName;
        public string SynergyLevelName;
        public ArtifactEffectApplicationMode ApplicationMode;
        public ArtifactEffectRecipient Recipient;
        public string RecipientMonsterName;
        public SkillDefinition TargetSkill;
        public SkillDefinition OutcomeSkill;
        public SummonDefinition SpawnSummon;
        public SkillNode[] Nodes = Array.Empty<SkillNode>();
        public SkillReaction[] Reactions = Array.Empty<SkillReaction>();
    }

    [Serializable]
    public sealed class ArtifactDefinition
    {
        public string ArtifactName;
        public string DisplayName;
        public string SynergyName;
        public string Description;
        public Sprite Icon;
        public ArtifactEffectDefinition[] Effects = Array.Empty<ArtifactEffectDefinition>();
    }

    [Serializable]
    public sealed class ArtifactSynergyLevelDefinition
    {
        public string LevelName;
        public int RequiredCount;
        public string Description;
        public ArtifactSynergyEffectDefinition[] Effects = Array.Empty<ArtifactSynergyEffectDefinition>();
    }

    [Serializable]
    public sealed class ArtifactSynergyDefinition
    {
        public string SynergyName;
        public string DisplayName;
        public string Summary;
        public string Description;
        public Sprite Icon;
        public ArtifactSynergyLevelDefinition[] Levels = Array.Empty<ArtifactSynergyLevelDefinition>();
    }
}
