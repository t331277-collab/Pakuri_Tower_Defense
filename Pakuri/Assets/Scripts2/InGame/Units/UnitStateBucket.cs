using System;
using System.Collections.Generic;

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
