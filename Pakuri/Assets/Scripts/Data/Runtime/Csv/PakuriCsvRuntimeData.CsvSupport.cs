using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;

namespace Pakuri.Data
{
    public static partial class PakuriCsvRuntimeData
    {
        private sealed class CsvFatalException : Exception
        {
            public CsvFatalException(string message)
                : this(message, null)
            {
            }

            public CsvFatalException(string message, List<string> errors)
                : base(message)
            {
                Errors = errors ?? new List<string>();
            }

            public List<string> Errors { get; }
        }

        private sealed class CsvTable
        {
            private CsvTable(string tableName, string[] headers, string[] types, List<CsvRecord> records)
            {
                TableName = tableName;
                Headers = headers;
                Types = types;
                Records = records;
            }

            public string TableName { get; }
            public string[] Headers { get; }
            public string[] Types { get; }
            public List<CsvRecord> Records { get; }

            public static CsvTable Load(TextAsset asset, string tableName)
            {
                if (asset == null)
                {
                    throw new CsvFatalException($"Required CSV TextAsset is missing for '{tableName}'.");
                }

                return Load(tableName, asset.text);
            }

            public static CsvTable Load(string path)
            {
                if (!File.Exists(path))
                {
                    throw new CsvFatalException($"Required CSV file is missing: {path}");
                }

                return Load(Path.GetFileName(path), File.ReadAllText(path, Encoding.UTF8));
            }

            private static CsvTable Load(string tableName, string contents)
            {
                var normalizedContents = string.IsNullOrEmpty(contents)
                    ? string.Empty
                    : contents.Replace("\r\n", "\n").Replace('\r', '\n');
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

                return new CsvTable(tableName, headers, types, records);
            }
        }

        private sealed class CsvRecord
        {
            private readonly string[] cells;
            private readonly Dictionary<string, int> headerLookup;

            public CsvRecord(string tableName, int rowNumber, string[] headers, string[] cells)
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

            public string ReadRequiredString(string columnName)
            {
                var value = ReadString(columnName);
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new CsvFatalException($"CSV row {RowNumber} in '{TableName}' requires a non-empty '{columnName}' value.");
                }

                return value;
            }

            public string ReadString(string columnName)
            {
                return GetCell(columnName).Trim();
            }

            public int ReadInt(string columnName)
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

            public float ReadFloat(string columnName)
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

            public bool ReadBool(string columnName)
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

            public Color ReadColor(string columnName)
            {
                var value = ReadString(columnName);
                if (string.IsNullOrWhiteSpace(value))
                {
                    return Color.white;
                }

                var parts = value.Split('|');
                if (parts.Length != 4)
                {
                    throw new CsvFatalException(
                        $"CSV row {RowNumber} in '{TableName}' has invalid color value '{value}' for '{columnName}'.");
                }

                return new Color(
                    ParseColorComponent(parts[0], columnName),
                    ParseColorComponent(parts[1], columnName),
                    ParseColorComponent(parts[2], columnName),
                    ParseColorComponent(parts[3], columnName));
            }

            public TEnum ReadEnum<TEnum>(string columnName)
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

            private string GetCell(string columnName)
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

                return Unescape(cells[index]);
            }

            private static float ParseColorComponent(string value, string columnName)
            {
                if (!float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed))
                {
                    throw new CsvFatalException($"Invalid color component '{value}' in column '{columnName}'.");
                }

                return parsed;
            }
        }

        private static string[] SplitCsvLine(string line)
        {
            var values = new List<string>();
            var builder = new StringBuilder();
            var inQuotes = false;

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

        private static string Unescape(string value)
        {
            return string.IsNullOrEmpty(value)
                ? string.Empty
                : value.Replace("\\n", "\n");
        }
    }
}
