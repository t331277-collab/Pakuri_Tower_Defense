using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;


/*
 * CSV 텍스트를 표와 행으로 나누고 각 값을 필요한 자료형으로 읽는다.
 */
namespace Pakuri.Data
{
    internal static class CsvParser
    {
        /*
         * CSV 로딩을 중단해야 하는 오류와 세부 오류 목록을 전달한다.
         */
        internal class CsvFatalException : Exception
        {
            /*
             * CSV 치명 오류와 세부 오류 목록을 보관한다.
             */
            public CsvFatalException(string message /* 메시지 */)
                : this(message, null)
            {
            }

            /*
             * CSV 치명 오류와 세부 오류 목록을 보관한다.
             */
            public CsvFatalException(string message /* 메시지 */, List<string> errors /* 검증 오류를 모을 목록 */)
                : base(FormatMessage(message, errors))
            {
                if (errors == null)
                {
                    errors = new List<string>();
                }

                Errors = errors;
            }

            public List<string> Errors { get; }

            /*
             * FormatMessage에 맞는 문자열을 만들어 반환한다.
             */
            private static string FormatMessage(string message /* 메시지 */, List<string> errors /* 검증 오류를 모을 목록 */)
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

            /*
             * 오류 메시지와 세부 내용을 문자열로 만든다.
             */
            public override string ToString()
            {
                return base.ToString();
            }
        }

        /*
         * CSV의 헤더, 자료형, 데이터 행을 한 묶음으로 보관한다.
         */
        internal class CsvTable
        {
            /*
             * CSV 헤더와 행 목록을 구성한다.
             */
            internal CsvTable(string tableName /* CSV 표 이름 */, string[] headers /* 머리글 목록 */, string[] types /* 형식 목록 */, List<CsvRecord> records /* 기록 목록 */)
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

            /*
             * 필요한 CSV 또는 자산을 불러온다.
             */
            public static CsvTable Load(TextAsset asset /* 읽을 텍스트 에셋 */, string tableName /* CSV 표 이름 */)
            {
                if (asset == null)
                {
                    throw new CsvFatalException($"Required CSV TextAsset is missing for '{tableName}'.");
                }

                return Load(tableName, asset.text);
            }

            /*
             * 필요한 CSV 또는 자산을 불러온다.
             */
            public static CsvTable Load(string path /* 불러오거나 검사할 경로 */)
            {
                if (!File.Exists(path))
                {
                    throw new CsvFatalException($"Required CSV file is missing: {path}");
                }

                return Load(Path.GetFileName(path), File.ReadAllText(path, Encoding.UTF8));
            }

            /*
             * 필요한 CSV 또는 자산을 불러온다.
             */
            internal static CsvTable Load(string tableName /* CSV 표 이름 */, string contents /* 파싱할 CSV 문자열 */)
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

                return new CsvTable(tableName, headers, types, records);
            }
        }

        /*
         * CSV 한 행의 값과 헤더별 열 위치를 보관한다.
         */
        internal class CsvRecord
        {
            internal readonly string[] cells;
            internal readonly Dictionary<string, int> headerLookup;

            /*
             * CSV 한 행과 열 위치 정보를 구성한다.
             */
            public CsvRecord(string tableName /* CSV 표 이름 */, int rowNumber /* 행 번호 */, string[] headers /* 머리글 목록 */, string[] cells /* 셀 목록 */)
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

            /*
             * CSV 행에서 필요한 값을 읽는다.
             */
            public string ReadRequiredString(string columnName /* 읽거나 검사할 CSV 열 이름 */)
            {
                var value = ReadString(columnName);
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new CsvFatalException($"CSV row {RowNumber} in '{TableName}' requires a non-empty '{columnName}' value.");
                }

                return value;
            }

            /*
             * CSV 행에서 필요한 값을 읽는다.
             */
            public string ReadString(string columnName /* 읽거나 검사할 CSV 열 이름 */)
            {
                return GetCell(columnName).Trim();
            }

            /*
             * 필요한 조건을 만족하는지 확인한다.
             */
            public bool HasColumn(string columnName /* 읽거나 검사할 CSV 열 이름 */)
            {
                return !string.IsNullOrWhiteSpace(columnName)
                    && headerLookup.ContainsKey(columnName);
            }

            /*
             * CSV 행에서 필요한 값을 읽는다.
             */
            public int ReadInt(string columnName /* 읽거나 검사할 CSV 열 이름 */)
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

            /*
             * CSV 행에서 필요한 값을 읽는다.
             */
            public float ReadFloat(string columnName /* 읽거나 검사할 CSV 열 이름 */)
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

            /*
             * CSV 행에서 필요한 값을 읽는다.
             */
            public bool ReadBool(string columnName /* 읽거나 검사할 CSV 열 이름 */)
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

            /*
             * CSV 행에서 필요한 값을 읽는다.
             */
            public TEnum ReadEnum<TEnum>(string columnName /* 읽거나 검사할 CSV 열 이름 */)
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

            /*
             * 계산에 필요한 값을 반환한다.
             */
            internal string GetCell(string columnName /* 읽거나 검사할 CSV 열 이름 */)
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

        /*
         * 따옴표로 묶인 쉼표와 연속 따옴표를 구분해 CSV 한 줄을 열 단위로 나눈다.
         */
        internal static string[] SplitCsvLine(string line /* 직선 */)
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

        /*
         * CSV 셀 안의 줄바꿈 표기를 실제 줄바꿈으로 바꾼다.
         */
        internal static string UnescapeCsvCell(string value /* 처리할 값 */)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            return value.Replace("\\n", "\n");
        }
    }
}
