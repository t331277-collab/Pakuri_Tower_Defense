/*
 * 역할: 학습으로 선택할 스킬 변화를 정의한다.
 * 책임: 강화, 마스터, 패시브 선택을 적용 대상과 규칙에 연결한다.
 */

using System;
using Pakuri.Data;
using UnityEngine;

namespace Pakuri.Data
{
    /// 선택이 어느 학습 단계에 속하는지 구분한다.
    public enum SkillChoiceGroup
    {
        ActiveEnhancement,
        ActiveMaster,
        PassiveEnhancement
    }
}

namespace Pakuri.InGame
{
    /// 하나의 학습 선택을 표시 정보와 적용 규칙에 연결한다.
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
