using System;
using System.Collections.Generic;

namespace Pakuri.InGame
{
    public sealed class SkillChoiceModifierLibrary
    {
        private readonly Dictionary<string, SkillChoiceModifierRecord> records =
            new Dictionary<string, SkillChoiceModifierRecord>(StringComparer.OrdinalIgnoreCase);

        public int Count => records.Count;

        public void Clear()
        {
            records.Clear();
        }

        public void AddOrReplace(SkillChoiceModifierRecord record)
        {
            if (record == null || string.IsNullOrWhiteSpace(record.ChoiceId))
            {
                return;
            }

            records[record.ChoiceId] = record;
        }

        public bool TryGet(string choiceId, out SkillChoiceModifierRecord record)
        {
            record = null;
            return !string.IsNullOrWhiteSpace(choiceId) && records.TryGetValue(choiceId, out record);
        }
    }
}
