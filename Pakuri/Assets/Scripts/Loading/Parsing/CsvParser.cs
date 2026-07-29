/*
 * 역할: 저수준 CSV 파싱.
 * 책임: 인용된 값을 보존하면서 CSV 텍스트를 Header와 행으로 나누고 원본 오류를 보고한다.
 */

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;

namespace Pakuri.Data
{

    /// <summary><c>CsvParser</c> 원본 값을 런타임 모델로 파싱한다.</summary>
    internal static class CsvParser
    {

        /// <summary><c>CsvFatalException</c> 처리 중 발생한 실패 정보를 전달한다.</summary>
        internal class CsvFatalException : Exception
        {

            /// <summary><c>CsvFatalException</c> 인스턴스를 전달된 런타임 입력값으로 초기화한다.</summary>
            public CsvFatalException(string message)
                : this(message, null)
            {
            }

            /// <summary><c>CsvFatalException</c> 인스턴스를 전달된 런타임 입력값으로 초기화한다.</summary>
            public CsvFatalException(string message, List<string> errors)
                : base(FormatMessage(message, errors))
            {
                if (errors == null)
                {
                    errors = new List<string>();
                }

                Errors = errors;
            }

            public List<string> Errors { get; }

            /// <summary>전달된 런타임 입력값을 사용해 <c>Message</c>를 표시 또는 직렬화 형식으로 변환한다.</summary>
            private static string FormatMessage(string message, List<string> errors)
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

        /// <summary><c>CsvTable</c>가 소유하는 데이터와 동작을 캡슐화한다.</summary>
        internal class CsvTable
        {

            /// <summary><c>CsvTable</c> 인스턴스를 전달된 런타임 입력값으로 초기화한다.</summary>
            internal CsvTable(List<CsvRecord> records)
            {
                Records = records;
            }

            public List<CsvRecord> Records { get; }

            /// <summary>전달된 런타임 입력값을 사용해 <c>요청값</c>를 불러온다.</summary>
            public static CsvTable Load(TextAsset asset, string tableName)
            {
                if (asset == null)
                {
                    throw new CsvFatalException($"Required CSV TextAsset is missing for '{tableName}'.");
                }

                return Load(tableName, asset.text);
            }

            /// <summary>전달된 런타임 입력값을 사용해 <c>요청값</c>를 불러온다.</summary>
            internal static CsvTable Load(string tableName, string contents)
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

        /// <summary><c>CsvRecord</c>가 나타내는 런타임 값을 보관한다.</summary>
        internal class CsvRecord
        {
            internal readonly string[] cells;
            internal readonly Dictionary<string, int> headerLookup;

            /// <summary><c>CsvRecord</c> 인스턴스를 전달된 런타임 입력값으로 초기화한다.</summary>
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

            /// <summary>전달된 <c>columnName</c> 값을 사용해 <c>RequiredString</c>를 읽는다.</summary>
            public string ReadRequiredString(string columnName)
            {
                var value = ReadString(columnName);
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new CsvFatalException($"CSV row {RowNumber} in '{TableName}' requires a non-empty '{columnName}' value.");
                }

                return value;
            }

            /// <summary>전달된 <c>columnName</c> 값을 사용해 <c>String</c>를 읽는다.</summary>
            public string ReadString(string columnName)
            {
                return GetCell(columnName).Trim();
            }

            /// <summary>전달된 <c>columnName</c> 값을 사용해 소유한 런타임 상태에 <c>Column</c>가 있는지 반환한다.</summary>
            public bool HasColumn(string columnName)
            {
                return !string.IsNullOrWhiteSpace(columnName)
                    && headerLookup.ContainsKey(columnName);
            }

            /// <summary>전달된 <c>columnName</c> 값을 사용해 <c>Int</c>를 읽는다.</summary>
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

            /// <summary>전달된 <c>columnName</c> 값을 사용해 <c>Float</c>를 읽는다.</summary>
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

            /// <summary>전달된 <c>columnName</c> 값을 사용해 <c>Bool</c>를 읽는다.</summary>
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

            /// <summary>전달된 <c>columnName</c> 값을 사용해 <c>Enum</c>를 읽는다.</summary>
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

            /// <summary>전달된 <c>columnName</c> 값을 사용해 <c>Cell</c>를 반환한다.</summary>
            internal string GetCell(string columnName)
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

        /// <summary>전달된 <c>line</c> 값을 사용해 <c>SplitCsvLine</c> 결과값을 생성해 반환한다.</summary>
        internal static string[] SplitCsvLine(string line)
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

        /// <summary>전달된 <c>value</c> 값을 사용해 <c>UnescapeCsvCell</c> 결과값을 생성해 반환한다.</summary>
        internal static string UnescapeCsvCell(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            return value.Replace("\\n", "\n");
        }
    }
}
