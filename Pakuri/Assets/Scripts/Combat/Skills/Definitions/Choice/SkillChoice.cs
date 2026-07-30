/*
 * 역할: 스킬 선택지 계약.
 * 책임: 강화·마스터·패시브 선택지와 적용 노드를 정의한다.
 */

using System;
using Pakuri.Data;
using UnityEngine;

namespace Pakuri.Data
{
    public enum SkillChoiceGroup
    {
        ActiveEnhancement,
        ActiveMaster,
        PassiveEnhancement,
        PassiveBase
    }
}

namespace Pakuri.InGame
{
    [Serializable]
    public class SkillChoice
    {
        public string ChoiceId;
        public string MonsterId;
        public string SkillId;
        public string TargetSkillId;
        public SkillChoiceGroup ChoiceGroup;
        public string Title;
        public Sprite SkillIcon;
        public GameObject SkillEffectPrefab;
        [TextArea(2, 5)] public string DescriptionText;
        public SkillNode[] Nodes = Array.Empty<SkillNode>();
    }
}
