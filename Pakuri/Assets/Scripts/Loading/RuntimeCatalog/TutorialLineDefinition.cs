using System;
using System.Collections.Generic;
using UnityEngine;
using static Pakuri.Data.CsvParser;

namespace Pakuri.Data
{
    [Serializable]
    public sealed class TutorialLineDefinition
    {
        public string LineId;
        public string PhaseId;
        public string SequenceId;
        public int BlockOrder;
        public string Text;

        internal static TutorialLineDefinition[] Load(TextAsset csv)
        {
            var rows = new List<TutorialLineDefinition>();
            var lineIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var record in CsvTable.Load(csv, "TutorialLine.csv").Records)
            {
                var lineId = record.ReadRequiredString("line_id");
                if (!lineIds.Add(lineId))
                {
                    throw new CsvFatalException($"TutorialLine.csv contains duplicate line_id '{lineId}'.");
                }

                rows.Add(new TutorialLineDefinition
                {
                    LineId = lineId,
                    PhaseId = record.ReadRequiredString("phase_id"),
                    SequenceId = record.ReadRequiredString("sequence_id"),
                    BlockOrder = record.ReadInt("block_order"),
                    Text = record.ReadRequiredString("text").Replace("\\n", "\n")
                });
            }

            if (rows.Count != 15)
            {
                throw new CsvFatalException($"TutorialLine.csv requires exactly 15 dialogue blocks but contains {rows.Count}.");
            }

            return rows.ToArray();
        }
    }
}
