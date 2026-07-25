using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using Pakuri.NewCore.Catalog;
using Pakuri.NewCore.Definitions;
using Pakuri.NewCore.Definitions.Choices;
using Pakuri.NewCore.Definitions.Skills;
using Pakuri.NewCore.Definitions.Stage;
using Pakuri.NewCore.Definitions.Status;
using Pakuri.NewCore.Definitions.Units;

/* CSV 행 메타데이터와 타입 변환된 열 접근 계약을 정의한다. */
namespace Pakuri.NewCore.Definitions
{
    public abstract class CsvDefinition
    {
        private readonly IReadOnlyDictionary<string, string> schema;
        private readonly IReadOnlyDictionary<string, object> columns;

        /* 파서가 변환한 행 데이터와 출처 메타데이터를 불변 상태로 저장한다. */
        internal CsvDefinition(CsvDefinitionData data)
        {
            SourcePath = data.SourcePath;
            SourceRecordNumber = data.SourceRecordNumber;
            HasSchemaRow = data.HasSchemaRow;
            schema = new ReadOnlyDictionary<string, string>(
                new Dictionary<string, string>(data.Schema, StringComparer.Ordinal));
            columns = new ReadOnlyDictionary<string, object>(
                new Dictionary<string, object>(data.Columns, StringComparer.Ordinal));
        }

        public string SourcePath { get; }

        public int SourceRecordNumber { get; }

        public bool HasSchemaRow { get; }

        public IReadOnlyDictionary<string, string> Schema => schema;

        public IReadOnlyDictionary<string, object> Columns => columns;

        /* 선택 문자열 열을 반환하고 열이나 값이 없으면 null을 반환한다. */
        protected string OptionalString(string columnName)
        {
            if (!columns.TryGetValue(columnName, out object value))
            {
                return null;
            }

            return value as string;
        }

        /* 선택 정수 열을 반환하고 값이 없으면 null을 반환한다. */
        protected int? OptionalInt(string columnName)
        {
            if (!columns.TryGetValue(columnName, out object value) || value == null)
            {
                return null;
            }

            return (int)value;
        }

        /* 선택 실수 열을 반환하고 값이 없으면 null을 반환한다. */
        protected float? OptionalFloat(string columnName)
        {
            if (!columns.TryGetValue(columnName, out object value) || value == null)
            {
                return null;
            }

            return (float)value;
        }

        /* 선택 논리값 열을 반환하고 값이 없으면 null을 반환한다. */
        protected bool? OptionalBool(string columnName)
        {
            if (!columns.TryGetValue(columnName, out object value) || value == null)
            {
                return null;
            }

            return (bool)value;
        }
    }

    internal class CsvDefinitionData
    {
        /* 파서가 만든 스키마와 열 값, 원본 위치를 정의 생성 입력으로 저장한다. */
        internal CsvDefinitionData(
            string sourcePath,
            int sourceRecordNumber,
            bool hasSchemaRow,
            IReadOnlyDictionary<string, string> schema,
            IReadOnlyDictionary<string, object> columns)
        {
            SourcePath = sourcePath;
            SourceRecordNumber = sourceRecordNumber;
            HasSchemaRow = hasSchemaRow;
            Schema = schema;
            Columns = columns;
        }

        public string SourcePath { get; }

        public int SourceRecordNumber { get; }

        public bool HasSchemaRow { get; }

        public IReadOnlyDictionary<string, string> Schema { get; }

        public IReadOnlyDictionary<string, object> Columns { get; }
    }
}

/* 보존 CSV를 파싱하고 정의 카탈로그를 생성한다. */
namespace Pakuri.NewCore.Parsing
{
    internal class CsvParser
    {
        private static readonly IReadOnlyList<string> requiredCsvPaths =
            Array.AsReadOnly(new[]
            {
                "Assets/CSVdata/authoring/catalog/catalog_monsters.csv",
                "Assets/CSVdata/authoring/enemy/enemies.csv",
                "Assets/CSVdata/authoring/enemy/skills/base/area_attack/skills_area_attack.csv",
                "Assets/CSVdata/authoring/enemy/skills/base/buff/skills_buff.csv",
                "Assets/CSVdata/authoring/enemy/skills/base/heal/skills_heal.csv",
                "Assets/CSVdata/authoring/enemy/skills/base/passive/skills_passive.csv",
                "Assets/CSVdata/authoring/enemy/skills/base/projectile/skills_projectile.csv",
                "Assets/CSVdata/authoring/enemy/skills/base/shield/skills_shield.csv",
                "Assets/CSVdata/authoring/enemy/skills/base/single_attack/skills_single_attack.csv",
                "Assets/CSVdata/authoring/enemy/skills/triggers/buff/buff_skill_triger.csv",
                "Assets/CSVdata/authoring/enemy/skills/triggers/single_attack/single_attack_skill_triger.csv",
                "Assets/CSVdata/authoring/monster/monster_modifier_skill_choice.csv",
                "Assets/CSVdata/authoring/monster/monsters.csv",
                "Assets/CSVdata/authoring/monster/skills/base/area_attack/skills_area_attack.csv",
                "Assets/CSVdata/authoring/monster/skills/base/buff/skills_buff.csv",
                "Assets/CSVdata/authoring/monster/skills/base/line_attack/skills_line_attack.csv",
                "Assets/CSVdata/authoring/monster/skills/base/passive/skills_passive.csv",
                "Assets/CSVdata/authoring/monster/skills/base/projectile/skills_projectile.csv",
                "Assets/CSVdata/authoring/monster/skills/base/single_attack/skills_single_attack.csv",
                "Assets/CSVdata/authoring/monster/skills/choices/area_attack/skill_choices_area_attack.csv",
                "Assets/CSVdata/authoring/monster/skills/choices/area_attack/skill_graph_nodes_area_attack.csv",
                "Assets/CSVdata/authoring/monster/skills/choices/buff/skill_choices_buff.csv",
                "Assets/CSVdata/authoring/monster/skills/choices/buff/skill_graph_nodes_buff.csv",
                "Assets/CSVdata/authoring/monster/skills/choices/line_attack/skill_choices_line_attack.csv",
                "Assets/CSVdata/authoring/monster/skills/choices/line_attack/skill_graph_nodes_line_attack.csv",
                "Assets/CSVdata/authoring/monster/skills/choices/passive/skill_choices_passive.csv",
                "Assets/CSVdata/authoring/monster/skills/choices/passive/skill_graph_nodes_passive.csv",
                "Assets/CSVdata/authoring/monster/skills/choices/projectile/skill_choices_projectile.csv",
                "Assets/CSVdata/authoring/monster/skills/choices/projectile/skill_graph_nodes_projectile.csv",
                "Assets/CSVdata/authoring/monster/skills/choices/single_attack/skill_choices_single_attack.csv",
                "Assets/CSVdata/authoring/monster/skills/choices/single_attack/skill_graph_nodes_single_attack.csv",
                "Assets/CSVdata/authoring/monster/skills/nodes/definitions/skill_node_definition_params.csv",
                "Assets/CSVdata/authoring/monster/skills/nodes/definitions/skill_node_definitions.csv",
                "Assets/CSVdata/authoring/monster/skills/triggers/buff/buff_skill_triger.csv",
                "Assets/CSVdata/authoring/monster/skills/triggers/line_attack/line_attack_skill_triger.csv",
                "Assets/CSVdata/authoring/monster/skills/triggers/passive/passive_skill_triger.csv",
                "Assets/CSVdata/authoring/monster/skills/triggers/projectile/projectile_skill_triger.csv",
                "Assets/CSVdata/authoring/monster/skills/triggers/single_attack/single_attack_skill_triger.csv",
                "Assets/CSVdata/authoring/status/status_effects.csv",
                "Assets/CSVdata/stage_flow/StageDay.csv",
                "Assets/CSVdata/stage_flow/StageEncounter.csv",
                "Assets/CSVdata/stage_flow/StageReward.csv"
            });

        private static readonly IReadOnlyDictionary<string, IReadOnlyList<string>>
            stageSchemas = CreateStageSchemas();

        internal static IReadOnlyList<string> RequiredCsvPaths => requiredCsvPaths;

        /* 필수 CSV 원문 집합을 정의 객체와 불변 카탈로그로 변환한다. */
        internal GameDefinitionCatalog Parse(IReadOnlyDictionary<string, string> csvFiles)
        {

            Dictionary<string, string> normalizedSources =
                new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (KeyValuePair<string, string> source in csvFiles)
            {
                string path = NormalizePath(source.Key);
                normalizedSources[path] = source.Value;
            }

            List<CsvDefinition> definitions = new List<CsvDefinition>();
            foreach (string path in requiredCsvPaths)
            {
                ParsedTable table = ParseTable(path, normalizedSources[path]);
                foreach (ParsedRow row in table.Rows)
                {
                    CsvDefinitionData data = new CsvDefinitionData(
                        path,
                        row.RecordNumber,
                        table.HasSchemaRow,
                        table.Schema,
                        row.Values);
                    definitions.Add(CreateDefinition(path, data));
                }
            }

            return new GameDefinitionCatalog(
                definitions,
                requiredCsvPaths.Count,
                requiredCsvPaths.Count(
                    path => path.Contains("/authoring/", StringComparison.Ordinal)));
        }

        /* 한 CSV의 헤더·스키마·데이터 행을 타입 변환된 테이블로 만든다. */
        private static ParsedTable ParseTable(string path, string text)
        {
            List<RawRecord> records = ParseRecords(text);

            string[] headers = records[0].Fields.ToArray();

            headers[0] = headers[0].TrimStart('\uFEFF');

            bool hasSchemaRow = path.Contains("/authoring/", StringComparison.Ordinal);
            int dataStartIndex;
            string[] schemaTokens;
            if (hasSchemaRow)
            {

                schemaTokens = records[1].Fields.ToArray();
                dataStartIndex = 2;
            }
            else
            {
                stageSchemas.TryGetValue(
                    path,
                    out IReadOnlyList<string> stageSchema);
                schemaTokens = stageSchema.ToArray();

                dataStartIndex = 1;
            }

            Dictionary<string, string> schema =
                new Dictionary<string, string>(StringComparer.Ordinal);
            for (int index = 0; index < headers.Length; index++)
            {
                schema.Add(headers[index], schemaTokens[index]);
            }

            List<ParsedRow> parsedRows = new List<ParsedRow>();
            for (int rowIndex = dataStartIndex; rowIndex < records.Count; rowIndex++)
            {
                RawRecord record = records[rowIndex];
                Dictionary<string, object> values =
                    new Dictionary<string, object>(StringComparer.Ordinal);
                for (int columnIndex = 0; columnIndex < headers.Length; columnIndex++)
                {
                    values.Add(
                        headers[columnIndex],
                        ConvertValue(
                            schemaTokens[columnIndex],
                            record.Fields[columnIndex]));
                }

                parsedRows.Add(new ParsedRow(record.RecordNumber, values));
            }

            return new ParsedTable(
                hasSchemaRow,
                new ReadOnlyDictionary<string, string>(schema),
                parsedRows);
        }

        /* 따옴표와 줄바꿈 규칙을 적용해 CSV 원문을 원시 레코드 목록으로 분해한다. */
        private static List<RawRecord> ParseRecords(string text)
        {

            List<RawRecord> records = new List<RawRecord>();
            List<string> fields = new List<string>();
            StringBuilder field = new StringBuilder();
            bool inQuotes = false;
            bool closedQuote = false;
            bool fieldStarted = false;
            int line = 1;
            int recordStartLine = 1;

            for (int index = 0; index < text.Length; index++)
            {
                char current = text[index];
                if (inQuotes)
                {
                    if (current == '"')
                    {
                        if (index + 1 < text.Length && text[index + 1] == '"')
                        {
                            field.Append('"');
                            index++;
                        }
                        else
                        {
                            inQuotes = false;
                            closedQuote = true;
                        }
                    }
                    else if (current == '\r' || current == '\n')
                    {
                        if (current == '\r' && index + 1 < text.Length && text[index + 1] == '\n')
                        {
                            index++;
                        }

                        field.Append('\n');
                        line++;
                    }
                    else
                    {
                        field.Append(current);
                    }

                    continue;
                }

                if (current == '"')
                {

                    inQuotes = true;
                    fieldStarted = true;
                    continue;
                }

                if (current == ',')
                {
                    fields.Add(field.ToString());
                    field.Clear();
                    fieldStarted = false;
                    closedQuote = false;
                    continue;
                }

                if (current == '\r' || current == '\n')
                {
                    if (current == '\r' && index + 1 < text.Length && text[index + 1] == '\n')
                    {
                        index++;
                    }

                    fields.Add(field.ToString());
                    records.Add(new RawRecord(recordStartLine, fields));
                    fields = new List<string>();
                    field.Clear();
                    fieldStarted = false;
                    closedQuote = false;
                    line++;
                    recordStartLine = line;
                    continue;
                }

                field.Append(current);
                fieldStarted = true;
            }

            if (fieldStarted || field.Length != 0 || fields.Count != 0 || closedQuote)
            {
                fields.Add(field.ToString());
                records.Add(new RawRecord(recordStartLine, fields));
            }

            return records;
        }

        /* 스키마 토큰에 따라 CSV 문자열을 런타임 기본 타입으로 변환한다. */
        private static object ConvertValue(
            string schemaToken,
            string rawValue)
        {
            if (string.IsNullOrEmpty(rawValue))
            {
                return null;
            }

            string type = schemaToken;
            if (string.IsNullOrEmpty(type))
            {
                type = "string";
            }

            switch (type)
            {
                case "int":
                    if (int.TryParse(
                        rawValue,
                        NumberStyles.Integer,
                        CultureInfo.InvariantCulture,
                        out int integerValue))
                    {
                        return integerValue;
                    }

                    break;

                case "float":
                    if (float.TryParse(
                            rawValue,
                            NumberStyles.Float,
                            CultureInfo.InvariantCulture,
                            out float floatValue)
                        && !float.IsNaN(floatValue)
                        && !float.IsInfinity(floatValue))
                    {
                        return floatValue;
                    }

                    break;

                case "bool":
                    if (string.Equals(rawValue, "true", StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }

                    if (string.Equals(rawValue, "false", StringComparison.OrdinalIgnoreCase))
                    {
                        return false;
                    }

                    break;
            }

            return rawValue;
        }

        /* CSV 경로 권한에 맞는 구체 정의 타입을 생성한다. */
        private static CsvDefinition CreateDefinition(string path, CsvDefinitionData data)
        {
            if (path.EndsWith("/catalog/catalog_monsters.csv", StringComparison.Ordinal))
            {
                return new CatalogMonsterDefinition(data);
            }

            if (path.EndsWith("/enemy/enemies.csv", StringComparison.Ordinal))
            {
                return new EnemyDefinition(data);
            }

            if (path.EndsWith("/monster/monsters.csv", StringComparison.Ordinal))
            {
                return new MonsterDefinition(data);
            }

            if (path.EndsWith("/monster/monster_modifier_skill_choice.csv", StringComparison.Ordinal))
            {
                return new MonsterModifierSkillChoiceDefinition(data);
            }

            if (path.Contains("/skills/choices/", StringComparison.Ordinal)
                && path.Contains("/skill_choices_", StringComparison.Ordinal))
            {
                return new SkillChoiceDefinition(data);
            }

            if (path.Contains("/skills/choices/", StringComparison.Ordinal)
                && path.Contains("/skill_graph_nodes_", StringComparison.Ordinal))
            {
                return new ChoiceNodeDefinition(data);
            }

            if (path.EndsWith(
                "/nodes/definitions/skill_node_definitions.csv",
                StringComparison.Ordinal))
            {
                return new NodeTypeDefinition(data);
            }

            if (path.EndsWith(
                "/nodes/definitions/skill_node_definition_params.csv",
                StringComparison.Ordinal))
            {
                return new NodeParamDefinition(data);
            }

            if (path.Contains("/skills/triggers/", StringComparison.Ordinal))
            {
                return new SkillTriggerDefinition(data);
            }

            if (path.Contains("/skills/base/projectile/", StringComparison.Ordinal))
            {
                return new ProjectileDefinition(data);
            }

            if (path.Contains("/skills/base/line_attack/", StringComparison.Ordinal))
            {
                return new LineAttackDefinition(data);
            }

            if (path.Contains("/skills/base/area_attack/", StringComparison.Ordinal))
            {
                return new AreaAttackDefinition(data);
            }

            if (path.Contains("/skills/base/single_attack/", StringComparison.Ordinal))
            {
                return new SingleAttackDefinition(data);
            }

            if (path.Contains("/skills/base/buff/", StringComparison.Ordinal))
            {
                return new BuffDefinition(data);
            }

            if (path.Contains("/skills/base/heal/", StringComparison.Ordinal))
            {
                return new HealDefinition(data);
            }

            if (path.Contains("/skills/base/shield/", StringComparison.Ordinal))
            {
                return new ShieldDefinition(data);
            }

            if (path.Contains("/skills/base/passive/", StringComparison.Ordinal))
            {
                return new PassiveDefinition(data);
            }

            if (path.EndsWith("/status/status_effects.csv", StringComparison.Ordinal))
            {
                return new StatusDefinition(data);
            }

            if (path.EndsWith("/stage_flow/StageDay.csv", StringComparison.Ordinal))
            {
                return new StageDayDefinition(data);
            }

            if (path.EndsWith("/stage_flow/StageEncounter.csv", StringComparison.Ordinal))
            {
                return new StageEncounterDefinition(data);
            }

            if (path.EndsWith("/stage_flow/StageReward.csv", StringComparison.Ordinal))
            {
                return new StageRewardDefinition(data);
            }

            return null;
        }


        /* CSV 경로를 슬래시 표기로 정규화한다. */
        private static string NormalizePath(string path)
        {

            return path.Replace('\\', '/');
        }


        /* 스키마 행이 없는 Stage CSV의 고정 열 타입 계약을 만든다. */
        private static IReadOnlyDictionary<string, IReadOnlyList<string>> CreateStageSchemas()
        {
            Dictionary<string, IReadOnlyList<string>> schemas =
                new Dictionary<string, IReadOnlyList<string>>(StringComparer.Ordinal)
                {
                    ["Assets/CSVdata/stage_flow/StageDay.csv"] = Array.AsReadOnly(new[]
                    {
                        "int", "int", "string", "string", "string", "string", "float",
                        "bool", "bool", "string"
                    }),
                    ["Assets/CSVdata/stage_flow/StageEncounter.csv"] = Array.AsReadOnly(new[]
                    {
                        "string", "int", "string", "int", "float", "float", "float",
                        "float", "bool", "bool", "float", "float", "bool", "string"
                    }),
                    ["Assets/CSVdata/stage_flow/StageReward.csv"] = Array.AsReadOnly(new[]
                    {
                        "string", "string", "int", "int", "int", "float", "float",
                        "float", "float", "int", "int", "string", "string"
                    })
                };
            return new ReadOnlyDictionary<string, IReadOnlyList<string>>(schemas);
        }

        private class ParsedTable
        {
            /* 파싱된 스키마와 데이터 행을 하나의 테이블 결과로 저장한다. */
            public ParsedTable(
                bool hasSchemaRow,
                IReadOnlyDictionary<string, string> schema,
                IReadOnlyList<ParsedRow> rows)
            {
                HasSchemaRow = hasSchemaRow;
                Schema = schema;
                Rows = rows;
            }

            public bool HasSchemaRow { get; }

            public IReadOnlyDictionary<string, string> Schema { get; }

            public IReadOnlyList<ParsedRow> Rows { get; }
        }

        private class ParsedRow
        {
            /* 원본 레코드 번호와 타입 변환된 열 값을 저장한다. */
            public ParsedRow(int recordNumber, IReadOnlyDictionary<string, object> values)
            {
                RecordNumber = recordNumber;
                Values = values;
            }

            public int RecordNumber { get; }

            public IReadOnlyDictionary<string, object> Values { get; }
        }

        private class RawRecord
        {
            /* CSV 구문 분석 전 원본 레코드 번호와 문자열 필드를 저장한다. */
            public RawRecord(int recordNumber, IReadOnlyList<string> fields)
            {
                RecordNumber = recordNumber;
                Fields = fields;
            }

            public int RecordNumber { get; }

            public IReadOnlyList<string> Fields { get; }
        }
    }
}
