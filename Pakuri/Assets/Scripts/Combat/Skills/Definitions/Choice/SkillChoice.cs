/*
 * 역할: 학습으로 선택할 스킬 변화를 정의한다.
 * 책임: 강화, 마스터, 패시브 선택을 적용 대상과 규칙에 연결한다.
 */

using System;
using Pakuri.Data;
using UnityEngine;

namespace Pakuri.Data
{
}

namespace Pakuri.InGame
{
    /// 하나의 학습 선택을 표시 정보와 적용 규칙에 연결한다.
    [Serializable]
    public class SkillChoice
    {
        public string ChoiceName;
        public string MonsterName;
        public string SkillName;
        public string TargetSkillName;
        public SkillChoiceGroup ChoiceGroup;
        public string Title;
        public Sprite SkillIcon;
        public GameObject SkillEffectPrefab;
        [TextArea(2, 5)] public string DescriptionText;
        public SkillNode[] Nodes = Array.Empty<SkillNode>();
    }
}
