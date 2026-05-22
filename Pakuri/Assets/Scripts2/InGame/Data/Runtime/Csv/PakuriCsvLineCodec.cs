using System.Collections.Generic;
using System.Text;

namespace Pakuri.Data
{
    public static class PakuriCsvLineCodec
    {
        public static string[] SplitLine(string line)
        {
            return SplitLineToList(line).ToArray();
        }

        public static List<string> SplitLineToList(string line)
        {
            var values = new List<string>();
            var builder = new StringBuilder();
            var inQuotes = false;

            if (line == null)
            {
                values.Add(string.Empty);
                return values;
            }

            for (var i = 0; i < line.Length; i++)
            {
                var character = line[i];
                if (character == '"')
                {
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        builder.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }

                    continue;
                }

                if (character == ',' && !inQuotes)
                {
                    values.Add(builder.ToString());
                    builder.Clear();
                    continue;
                }

                builder.Append(character);
            }

            values.Add(builder.ToString());
            return values;
        }

        public static string JoinLine(IReadOnlyList<string> cells)
        {
            var builder = new StringBuilder();
            for (var i = 0; i < cells.Count; i++)
            {
                if (i > 0)
                {
                    builder.Append(',');
                }

                builder.Append(EscapeCell(cells[i]));
            }

            return builder.ToString();
        }

        public static string EscapeCell(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            if (value.IndexOfAny(new[] { ',', '"', '\r', '\n' }) < 0)
            {
                return value;
            }

            return "\"" + value.Replace("\"", "\"\"") + "\"";
        }

        public static string UnescapeCell(string value)
        {
            return string.IsNullOrEmpty(value)
                ? string.Empty
                : value.Replace("\\n", "\n");
        }
    }
}
