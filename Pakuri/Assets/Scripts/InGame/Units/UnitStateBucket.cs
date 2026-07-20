using System;
using System.Collections.Generic;

/*
 * 유닛이 학습한 활성·패시브 스킬과 선택한 선택지 ID를 보관하는 상태 데이터.
 */
namespace Pakuri.InGame
{
    [Serializable]
    public sealed class UnitStateBucket
    {
        public readonly HashSet<string> LearnedActiveSkillIds = new HashSet<string>();
        public readonly HashSet<string> LearnedPassiveSkillIds = new HashSet<string>();
        public readonly HashSet<string> ChosenChoiceIds = new HashSet<string>();
    }
}
