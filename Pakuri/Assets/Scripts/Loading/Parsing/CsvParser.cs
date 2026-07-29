using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;

namespace Pakuri.Data
{
    internal static class CsvParser
    {

        internal class CsvFatalException : Exception
        {

            public CsvFatalException(string message )
                : this(message, null)
            {
            }

            public CsvFatalException(string message , List<string> errors )
                : base(FormatMessage(message, errors))
            {
                if (errors == null)
                {
                    errors = new List<string>();
                }

                Errors = errors;
            }

            public List<string> Errors { get; }

            private static string FormatMessage(string message , List<string> errors )
            {
                if (errors == null || errors.Count == 0)
                {
                    return message;
                }

                return string.Concat(
                    message,
                    Environment.NewLine,
                    string.Join(Environment.NewLine, errors));
            }

        }

        internal class CsvTable
        {

            internal CsvTable(List<CsvRecord> records )
            {
                Records = records;
            }

            public List<CsvRecord> Records { get; }

            public static CsvTable Load(TextAsset asset , string tableName )
            {
                if (asset == null)
                {
                    throw new CsvFatalException($"Required CSV TextAsset is missing for '{tableName}'.");
                }

                return Load(tableName, asset.text);
            }

            internal static CsvTable Load(string tableName , string contents )
            {
                var normalizedContents = string.Empty;
                if (!string.IsNullOrEmpty(contents))
                {
                    normalizedContents = contents.Replace("\r\n", "\n").Replace('\r', '\n');
                }
                var lines = normalizedContents.Split('\n');
                if (lines.Length < 2)
                {
                    throw new CsvFatalException($"CSV file '{tableName}' must contain a header row and a type row.");
                }

                var headers = SplitCsvLine(lines[0]);
                var types = SplitCsvLine(lines[1]);
                if (headers.Length == 0)
                {
                    throw new CsvFatalException($"CSV file '{tableName}' has an empty header row.");
                }

                headers[0] = headers[0].TrimStart('\uFEFF');
                types[0] = types[0].TrimStart('\uFEFF');

                if (headers.Length != types.Length)
                {
                    throw new CsvFatalException($"CSV file '{tableName}' has mismatched header/type column counts.");
                }

                var records = new List<CsvRecord>();
                for (var lineIndex = 2; lineIndex < lines.Length; lineIndex++)
                {
                    if (string.IsNullOrWhiteSpace(lines[lineIndex]))
                    {
                        continue;
                    }

                    var cells = SplitCsvLine(lines[lineIndex]);
                    if (cells.Length != headers.Length)
                    {
                        throw new CsvFatalException(
                            $"CSV file '{tableName}' row {lineIndex + 1} has {cells.Length} columns but expected {headers.Length}.");
                    }

                    records.Add(new CsvRecord(tableName, lineIndex + 1, headers, cells));
                }

                return new CsvTable(records);
            }
        }

        internal class CsvRecord
        {
            internal readonly string[] cells;
            internal readonly Dictionary<string, int> headerLookup;

            public CsvRecord(string tableName , int rowNumber , string[] headers , string[] cells )
            {
                TableName = tableName;
                RowNumber = rowNumber;
                this.cells = cells;
                headerLookup = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                for (var i = 0; i < headers.Length; i++)
                {
                    headerLookup[headers[i]] = i;
                }
            }

            public string TableName { get; }
            public int RowNumber { get; }

            public string ReadRequiredString(string columnName )
            {
                var value = ReadString(columnName);
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new CsvFatalException($"CSV row {RowNumber} in '{TableName}' requires a non-empty '{columnName}' value.");
                }

                return value;
            }

            public string ReadString(string columnName )
            {
                return GetCell(columnName).Trim();
            }

            public bool HasColumn(string columnName )
            {
                return !string.IsNullOrWhiteSpace(columnName)
                    && headerLookup.ContainsKey(columnName);
            }

            public int ReadInt(string columnName )
            {
                var value = ReadString(columnName);
                if (string.IsNullOrWhiteSpace(value))
                {
                    return 0;
                }

                if (!int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed))
                {
                    throw new CsvFatalException(
                        $"CSV row {RowNumber} in '{TableName}' has invalid int value '{value}' for '{columnName}'.");
                }

                return parsed;
            }

            public float ReadFloat(string columnName )
            {
                var value = ReadString(columnName);
                if (string.IsNullOrWhiteSpace(value))
                {
                    return 0f;
                }

                if (!float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed))
                {
                    throw new CsvFatalException(
                        $"CSV row {RowNumber} in '{TableName}' has invalid float value '{value}' for '{columnName}'.");
                }

                return parsed;
            }

            public bool ReadBool(string columnName )
            {
                var value = ReadString(columnName);
                if (string.IsNullOrWhiteSpace(value))
                {
                    return false;
                }

                if (!bool.TryParse(value, out var parsed))
                {
                    throw new CsvFatalException(
                        $"CSV row {RowNumber} in '{TableName}' has invalid bool value '{value}' for '{columnName}'.");
                }

                return parsed;
            }

            public TEnum ReadEnum<TEnum>(string columnName )
                where TEnum : struct
            {
                var value = ReadString(columnName);
                if (string.IsNullOrWhiteSpace(value))
                {
                    return default;
                }

                if (!Enum.TryParse<TEnum>(value, true, out var parsed))
                {
                    throw new CsvFatalException(
                        $"CSV row {RowNumber} in '{TableName}' has invalid enum value '{value}' for '{columnName}'.");
                }

                return parsed;
            }

            internal string GetCell(string columnName )
            {
                if (!headerLookup.TryGetValue(columnName, out var index))
                {
                    throw new CsvFatalException($"CSV table '{TableName}' is missing required column '{columnName}'.");
                }

                if (index >= cells.Length)
                {
                    throw new CsvFatalException(
                        $"CSV row {RowNumber} in '{TableName}' is missing value for column '{columnName}'.");
                }

                return UnescapeCsvCell(cells[index]);
            }
        }

        internal static string[] SplitCsvLine(string line )
        {
            var values = new List<string>();
            var builder = new StringBuilder();
            var inQuotes = false;

            if (line == null)
            {
                values.Add(string.Empty);
                return values.ToArray();
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
            return values.ToArray();
        }

        internal static string UnescapeCsvCell(string value )
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            return value.Replace("\\n", "\n");
        }
    }
}
