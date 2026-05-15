using System.Collections.Generic;
using System.Text;

namespace Pakuri.InGame
{
    public static class SkillChoiceModifierCsvParser
    {
        public static SkillChoiceModifierLibrary ParseLibrary(string csvText)
        {
            var library = new SkillChoiceModifierLibrary();
            if (string.IsNullOrWhiteSpace(csvText))
            {
                return library;
            }

            var rows = ParseRows(csvText);
            if (rows.Count <= 1)
            {
                return library;
            }

            var headers = rows[0];
            for (var i = 1; i < rows.Count; i++)
            {
                var row = ToDictionary(headers, rows[i]);
                library.AddOrReplace(SkillChoiceModifierRecord.FromRow(row));
            }

            return library;
        }

        private static Dictionary<string, string> ToDictionary(IReadOnlyList<string> headers, IReadOnlyList<string> row)
        {
            var result = new Dictionary<string, string>();
            for (var i = 0; i < headers.Count; i++)
            {
                var key = headers[i];
                if (string.IsNullOrWhiteSpace(key))
                {
                    continue;
                }

                result[key] = i < row.Count ? row[i] : string.Empty;
            }

            return result;
        }

        private static List<List<string>> ParseRows(string csvText)
        {
            var rows = new List<List<string>>();
            var row = new List<string>();
            var cell = new StringBuilder();
            var inQuotes = false;

            for (var i = 0; i < csvText.Length; i++)
            {
                var c = csvText[i];
                if (c == '"')
                {
                    if (inQuotes && i + 1 < csvText.Length && csvText[i + 1] == '"')
                    {
                        cell.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }

                    continue;
                }

                if (c == ',' && !inQuotes)
                {
                    row.Add(cell.ToString());
                    cell.Length = 0;
                    continue;
                }

                if ((c == '\n' || c == '\r') && !inQuotes)
                {
                    if (c == '\r' && i + 1 < csvText.Length && csvText[i + 1] == '\n')
                    {
                        i++;
                    }

                    row.Add(cell.ToString());
                    cell.Length = 0;
                    AddRowIfNotEmpty(rows, row);
                    row = new List<string>();
                    continue;
                }

                cell.Append(c);
            }

            row.Add(cell.ToString());
            AddRowIfNotEmpty(rows, row);
            return rows;
        }

        private static void AddRowIfNotEmpty(ICollection<List<string>> rows, List<string> row)
        {
            if (row == null || row.Count == 0)
            {
                return;
            }

            for (var i = 0; i < row.Count; i++)
            {
                if (!string.IsNullOrWhiteSpace(row[i]))
                {
                    rows.Add(row);
                    return;
                }
            }
        }
    }
}
