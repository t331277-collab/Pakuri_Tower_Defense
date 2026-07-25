using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
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

        /* 파서가 검증한 행 데이터와 출처 메타데이터를 불변 상태로 저장한다. */
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

        /* 필수 문자열 열을 반환하고 비어 있으면 CSV 출처가 포함된 오류를 발생시킨다. */
        protected string RequiredString(string columnName)
        {
            string value = OptionalString(columnName);
            if (string.IsNullOrEmpty(value))
            {
                throw new InvalidDataException(
                    $"{SourcePath} record {SourceRecordNumber} has no value for required column '{columnName}'.");
            }

            return value;
        }

        /* 지정된 모든 필수 열이 현재 행에 존재하는지 검증한다. */
        protected void ValidateRequired(params string[] columnNames)
        {
            foreach (string columnName in columnNames)
            {
                RequiredString(columnName);
            }
        }

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

    internal sealed class CsvDefinitionData
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

/* 보존 CSV를 파싱하고 정의 카탈로그를 검증해 생성한다. */
namespace Pakuri.NewCore.Parsing
{
    internal sealed class CsvParser
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

        private static readonly IReadOnlyDictionary<string, IReadOnlyCollection<string>>
            enumDomains = CreateEnumDomains();

        private static readonly IReadOnlyDictionary<string, IReadOnlyList<string>>
            stageSchemas = CreateStageSchemas();

        internal static IReadOnlyList<string> RequiredCsvPaths => requiredCsvPaths;

        /* 필수 CSV 원문 집합을 정의 객체와 검증된 불변 카탈로그로 변환한다. */
        internal GameDefinitionCatalog Parse(IReadOnlyDictionary<string, string> csvFiles)
        {
            if (csvFiles == null)
            {
                throw new ArgumentNullException(nameof(csvFiles));
            }

            Dictionary<string, string> normalizedSources =
                new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (KeyValuePair<string, string> source in csvFiles)
            {
                string path = NormalizePath(source.Key);
                if (!normalizedSources.TryAdd(path, source.Value))
                {
                    throw new InvalidDataException($"Duplicate CSV source path '{path}'.");
                }
            }

            HashSet<string> required = new HashSet<string>(requiredCsvPaths, StringComparer.Ordinal);
            foreach (string path in requiredCsvPaths)
            {
                if (!normalizedSources.ContainsKey(path))
                {
                    throw new InvalidDataException($"Required retained CSV is missing: '{path}'.");
                }
            }

            foreach (string path in normalizedSources.Keys)
            {
                if (!required.Contains(path))
                {
                    throw new InvalidDataException($"Unexpected CSV source path '{path}'.");
                }
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

            GameDefinitionCatalog catalog = new GameDefinitionCatalog(
                definitions,
                requiredCsvPaths.Count,
                requiredCsvPaths.Count(
                    path => path.Contains("/authoring/", StringComparison.Ordinal)));
            ValidateReferences(catalog);
            return catalog;
        }

        /* 한 CSV의 헤더·스키마·데이터 행을 검증하고 타입 변환된 테이블로 만든다. */
        private static ParsedTable ParseTable(string path, string text)
        {
            List<RawRecord> records = ParseRecords(path, text);
            if (records.Count == 0)
            {
                throw new InvalidDataException($"{path}: CSV is empty.");
            }

            string[] headers = records[0].Fields.ToArray();
            if (headers.Length == 0)
            {
                throw new InvalidDataException($"{path}: CSV header is empty.");
            }

            headers[0] = headers[0].TrimStart('\uFEFF');
            HashSet<string> seenHeaders = new HashSet<string>(StringComparer.Ordinal);
            foreach (string header in headers)
            {
                if (string.IsNullOrEmpty(header))
                {
                    throw Invalid(path, records[0].RecordNumber, "CSV contains an empty column name.");
                }

                if (!seenHeaders.Add(header))
                {
                    throw Invalid(
                        path,
                        records[0].RecordNumber,
                        $"Duplicate CSV column '{header}'.");
                }
            }

            bool hasSchemaRow = path.Contains("/authoring/", StringComparison.Ordinal);
            int dataStartIndex;
            string[] schemaTokens;
            if (hasSchemaRow)
            {
                if (records.Count < 2)
                {
                    throw Invalid(path, records[0].RecordNumber, "Authoring CSV has no schema row.");
                }

                RequireWidth(path, records[1], headers.Length);
                schemaTokens = records[1].Fields.ToArray();
                dataStartIndex = 2;
            }
            else
            {
                if (!stageSchemas.TryGetValue(path, out IReadOnlyList<string> stageSchema))
                {
                    throw new InvalidDataException($"{path}: No schema contract is registered.");
                }

                schemaTokens = stageSchema.ToArray();
                if (schemaTokens.Length != headers.Length)
                {
                    throw new InvalidDataException(
                        $"{path}: Registered schema width {schemaTokens.Length} does not match header width {headers.Length}.");
                }

                dataStartIndex = 1;
            }

            Dictionary<string, string> schema =
                new Dictionary<string, string>(StringComparer.Ordinal);
            for (int index = 0; index < headers.Length; index++)
            {
                ValidateSchemaToken(path, records[Math.Min(1, records.Count - 1)].RecordNumber, schemaTokens[index]);
                schema.Add(headers[index], schemaTokens[index]);
            }

            List<ParsedRow> parsedRows = new List<ParsedRow>();
            for (int rowIndex = dataStartIndex; rowIndex < records.Count; rowIndex++)
            {
                RawRecord record = records[rowIndex];
                RequireWidth(path, record, headers.Length);
                Dictionary<string, object> values =
                    new Dictionary<string, object>(StringComparer.Ordinal);
                for (int columnIndex = 0; columnIndex < headers.Length; columnIndex++)
                {
                    values.Add(
                        headers[columnIndex],
                        ConvertValue(
                            path,
                            record.RecordNumber,
                            headers[columnIndex],
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
        private static List<RawRecord> ParseRecords(string path, string text)
        {
            if (text == null)
            {
                throw new InvalidDataException($"{path}: CSV text is null.");
            }

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

                if (closedQuote && current != ',' && current != '\r' && current != '\n')
                {
                    throw Invalid(
                        path,
                        recordStartLine,
                        "Unexpected character after a closing quote.");
                }

                if (current == '"')
                {
                    if (fieldStarted || field.Length != 0)
                    {
                        throw Invalid(path, recordStartLine, "Quote found inside an unquoted field.");
                    }

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

            if (inQuotes)
            {
                throw Invalid(path, recordStartLine, "Unterminated quoted field.");
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
            string path,
            int recordNumber,
            string column,
            string schemaToken,
            string rawValue)
        {
            if (string.IsNullOrEmpty(rawValue))
            {
                return null;
            }

            string type = string.IsNullOrEmpty(schemaToken) ? "string" : schemaToken;
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

                    throw InvalidValue(path, recordNumber, column, rawValue, type);

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

                    throw InvalidValue(path, recordNumber, column, rawValue, type);

                case "bool":
                    if (string.Equals(rawValue, "true", StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }

                    if (string.Equals(rawValue, "false", StringComparison.OrdinalIgnoreCase))
                    {
                        return false;
                    }

                    throw InvalidValue(path, recordNumber, column, rawValue, type);
            }

            if (type.StartsWith("enum:", StringComparison.Ordinal) || type == "enum")
            {
                ValidateEnum(path, recordNumber, column, type, rawValue);
            }

            return rawValue;
        }

        /* 열거형 스키마 값이 등록된 허용 집합에 포함되는지 검증한다. */
        private static void ValidateEnum(
            string path,
            int recordNumber,
            string column,
            string schemaToken,
            string value)
        {
            string domainKey = schemaToken == "enum"
                ? $"enum:{column}"
                : schemaToken.Split('|')[0];
            if (!enumDomains.TryGetValue(domainKey, out IReadOnlyCollection<string> allowed))
            {
                throw Invalid(
                    path,
                    recordNumber,
                    $"Enum schema '{schemaToken}' for column '{column}' has no allowed-value contract.");
            }

            bool allowedLiteral = schemaToken.Split('|')
                .Skip(1)
                .Any(item => string.Equals(item, value, StringComparison.Ordinal));
            if (!allowed.Contains(value) && !allowedLiteral)
            {
                throw Invalid(
                    path,
                    recordNumber,
                    $"Invalid enum value '{value}' for column '{column}' ({schemaToken}).");
            }
        }

        /* CSV 스키마 토큰이 파서가 지원하는 타입 계약인지 검증한다. */
        private static void ValidateSchemaToken(string path, int recordNumber, string schemaToken)
        {
            if (string.IsNullOrEmpty(schemaToken)
                || schemaToken == "string"
                || schemaToken == "id"
                || schemaToken == "int"
                || schemaToken == "float"
                || schemaToken == "bool"
                || schemaToken == "asset_path"
                || schemaToken == "skill_id"
                || schemaToken == "choice_id"
                || schemaToken == "status_id"
                || schemaToken == "enum"
                || schemaToken.StartsWith("enum:", StringComparison.Ordinal))
            {
                return;
            }

            throw Invalid(path, recordNumber, $"Unsupported schema type '{schemaToken}'.");
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

            throw new InvalidDataException($"{path}: No Definition type is registered.");
        }

        /* 생성된 모든 정의 사이 식별자 참조와 소유 관계를 검증한다. */
        private static void ValidateReferences(GameDefinitionCatalog catalog)
        {
            foreach (CatalogMonsterDefinition entry in catalog.CatalogMonsters)
            {
                Require(catalog.Monsters, entry, nameof(entry.monster_id), entry.monster_id);
            }

            foreach (SkillDefinition skill in catalog.Skills.Values)
            {
                if (!string.IsNullOrEmpty(skill.monster_id))
                {
                    Require(catalog.Monsters, skill, nameof(skill.monster_id), skill.monster_id);
                }

                ValidateStatusColumns(catalog, skill);
            }

            foreach (EnemyDefinition enemy in catalog.Enemies.Values)
            {
                Require(catalog.Skills, enemy, nameof(enemy.skill_slot_a_id), enemy.skill_slot_a_id);
                Require(catalog.Skills, enemy, nameof(enemy.skill_slot_b_id), enemy.skill_slot_b_id);
                Require(catalog.Skills, enemy, nameof(enemy.passive_id), enemy.passive_id);
            }

            foreach (SkillChoiceDefinition choice in catalog.Choices.Values)
            {
                Require(catalog.Monsters, choice, nameof(choice.monster_id), choice.monster_id);
                Require(catalog.Skills, choice, nameof(choice.skill_id), choice.skill_id);
                RequireOptional(
                    catalog.Skills,
                    choice,
                    nameof(choice.target_skill_id),
                    choice.target_skill_id);
            }

            foreach (MonsterModifierSkillChoiceDefinition mapping in catalog.ModifierChoices)
            {
                Require(catalog.Choices, mapping, nameof(mapping.choice_id), mapping.choice_id);
                Require(catalog.Monsters, mapping, nameof(mapping.monster_id), mapping.monster_id);
                RequireOptional(
                    catalog.Skills,
                    mapping,
                    nameof(mapping.active_skill_id),
                    mapping.active_skill_id);
                RequireOptional(
                    catalog.Skills,
                    mapping,
                    nameof(mapping.passive_skill_id),
                    mapping.passive_skill_id);
            }

            foreach (NodeParamDefinition parameter in catalog.NodeParams)
            {
                Require(
                    catalog.NodeTypes,
                    parameter,
                    nameof(parameter.node_type_id),
                    parameter.node_type_id);
            }

            foreach (ChoiceNodeDefinition node in catalog.ChoiceNodes)
            {
                Require(catalog.Monsters, node, nameof(node.monster_id), node.monster_id);
                Require(catalog.NodeTypes, node, nameof(node.node_type_id), node.node_type_id);
                RequireOptional(
                    catalog.Skills,
                    node,
                    nameof(node.target_skill_id),
                    node.target_skill_id);
                RequireOptional(
                    catalog.Choices,
                    node,
                    nameof(node.excludes_active_choice_id),
                    node.excludes_active_choice_id);
                ValidateOwnerReference(catalog, node, node.owner_kind, node.owner_id, "owner_id");
            }

            foreach (SkillTriggerDefinition trigger in catalog.Triggers.Values)
            {
                Require(catalog.Skills, trigger, nameof(trigger.source_skill_id), trigger.source_skill_id);
                RequireOptional(
                    catalog.Monsters,
                    trigger,
                    nameof(trigger.monster_id),
                    trigger.monster_id);
                RequireOptionalColumn(catalog.Choices, trigger, "requires_active_choice_id");
                RequireOptionalColumn(catalog.Choices, trigger, "excludes_active_choice_id");
                RequireOptionalColumn(catalog.Skills, trigger, "event_skill_id");
                RequireOptionalColumn(catalog.Skills, trigger, "target_skill_id");
                ValidateStatusColumns(catalog, trigger);

                string graphOwnerKind = StringColumn(trigger, "triggered_graph_owner_kind");
                string graphOwnerId = StringColumn(trigger, "triggered_graph_owner_id");
                if (!string.IsNullOrEmpty(graphOwnerKind) || !string.IsNullOrEmpty(graphOwnerId))
                {
                    ValidateOwnerReference(
                        catalog,
                        trigger,
                        graphOwnerKind,
                        graphOwnerId,
                        "triggered_graph_owner_id");
                }
            }

            foreach (StageEncounterDefinition encounter in catalog.StageEncounters)
            {
                Require(catalog.Enemies, encounter, nameof(encounter.enemy_id), encounter.enemy_id);
            }

            HashSet<string> encounterIds = new HashSet<string>(
                catalog.StageEncounters.Select(item => item.encounter_id),
                StringComparer.Ordinal);
            foreach (StageDayDefinition day in catalog.StageDays.Values)
            {
                if (!encounterIds.Contains(day.encounter_id))
                {
                    throw Missing(day, nameof(day.encounter_id), day.encounter_id);
                }

                Require(
                    catalog.StageRewards,
                    day,
                    nameof(day.reward_rule_id),
                    day.reward_rule_id);
            }

            ValidateNodeArguments(catalog);
        }

        /* 선택 노드 인수가 노드 정의의 파라미터 계약과 일치하는지 검증한다. */
        private static void ValidateNodeArguments(GameDefinitionCatalog catalog)
        {
            Dictionary<string, NodeParamDefinition[]> parametersByType =
                catalog.NodeParams.GroupBy(item => item.node_type_id, StringComparer.Ordinal)
                    .ToDictionary(
                        group => group.Key,
                        group => group.OrderBy(item => item.param_order).ToArray(),
                        StringComparer.Ordinal);

            foreach (ChoiceNodeDefinition node in catalog.ChoiceNodes)
            {
                if (!parametersByType.TryGetValue(node.node_type_id, out NodeParamDefinition[] parameters))
                {
                    continue;
                }

                foreach (NodeParamDefinition parameter in parameters)
                {
                    int order = parameter.param_order ?? 0;
                    if (order < 1 || order > 12)
                    {
                        throw Invalid(
                            parameter.SourcePath,
                            parameter.SourceRecordNumber,
                            $"param_order '{order}' is outside arg_1 through arg_12.");
                    }

                    string argument = StringColumn(node, $"arg_{order}");
                    if (parameter.required == true && string.IsNullOrEmpty(argument))
                    {
                        throw Invalid(
                            node.SourcePath,
                            node.SourceRecordNumber,
                            $"Required node parameter '{parameter.param_key}' is empty.");
                    }

                    if (!string.IsNullOrEmpty(argument))
                    {
                        ValidateNodeArgument(catalog, node, parameter, argument);
                    }
                }
            }
        }

        /* 단일 노드 인수를 선언된 값 타입과 참조 도메인에 맞춰 검증한다. */
        private static void ValidateNodeArgument(
            GameDefinitionCatalog catalog,
            ChoiceNodeDefinition node,
            NodeParamDefinition parameter,
            string argument)
        {
            switch (parameter.value_type)
            {
                case "int":
                    if (!int.TryParse(
                        argument,
                        NumberStyles.Integer,
                        CultureInfo.InvariantCulture,
                        out _))
                    {
                        throw InvalidNodeArgument(node, parameter, argument);
                    }

                    break;

                case "float":
                    if (!float.TryParse(
                            argument,
                            NumberStyles.Float,
                            CultureInfo.InvariantCulture,
                            out float floatValue)
                        || float.IsNaN(floatValue)
                        || float.IsInfinity(floatValue))
                    {
                        throw InvalidNodeArgument(node, parameter, argument);
                    }

                    break;

                case "bool":
                    if (!string.Equals(argument, "true", StringComparison.OrdinalIgnoreCase)
                        && !string.Equals(argument, "false", StringComparison.OrdinalIgnoreCase))
                    {
                        throw InvalidNodeArgument(node, parameter, argument);
                    }

                    break;

                case "skill_id":
                    string skillId = argument;
                    int effectSeparator = argument.IndexOf('@');
                    if (effectSeparator >= 0)
                    {
                        string effectSelector = argument.Substring(effectSeparator + 1);
                        if (effectSelector != "effect1"
                            || argument.IndexOf('@', effectSeparator + 1) >= 0)
                        {
                            throw InvalidNodeArgument(node, parameter, argument);
                        }

                        skillId = argument.Substring(0, effectSeparator);
                    }

                    Require(catalog.Skills, node, parameter.param_key, skillId);
                    break;

                case "status_id":
                    Require(catalog.Statuses, node, parameter.param_key, argument);
                    break;

                case "enum":
                    if (!string.IsNullOrEmpty(parameter.allowed_values))
                    {
                        string[] allowed = parameter.allowed_values.Split('|');
                        if (!allowed.Contains(argument, StringComparer.Ordinal))
                        {
                            throw InvalidNodeArgument(node, parameter, argument);
                        }
                    }

                    break;
            }
        }

        /* 정의의 상태 식별자 열들이 상태 카탈로그를 참조하는지 검증한다. */
        private static void ValidateStatusColumns(
            GameDefinitionCatalog catalog,
            CsvDefinition definition)
        {
            string[] columns =
            {
                "status_effect_id",
                "condition_status_id",
                "deployment_required_target_status_id",
                "target_selection_status_id",
                "target_status_stack_status_id",
                "consume_target_status_id"
            };

            foreach (string column in columns)
            {
                RequireOptionalColumn(catalog.Statuses, definition, column);
            }
        }

        /* 노드 소유자 종류에 따라 선택지·스킬·트리거 참조를 검증한다. */
        private static void ValidateOwnerReference(
            GameDefinitionCatalog catalog,
            CsvDefinition definition,
            string ownerKind,
            string ownerId,
            string columnName)
        {
            if (string.IsNullOrEmpty(ownerKind) || string.IsNullOrEmpty(ownerId))
            {
                throw Invalid(
                    definition.SourcePath,
                    definition.SourceRecordNumber,
                    $"Owner kind and {columnName} must both be present.");
            }

            switch (ownerKind)
            {
                case "Choice":
                    Require(catalog.Choices, definition, columnName, ownerId);
                    break;
                case "Skill":
                    Require(catalog.Skills, definition, columnName, ownerId);
                    break;
                case "Trigger":
                    Require(catalog.Triggers, definition, columnName, ownerId);
                    break;
                default:
                    throw Invalid(
                        definition.SourcePath,
                        definition.SourceRecordNumber,
                        $"Unsupported owner kind '{ownerKind}'.");
            }
        }

        /* 값이 있는 선택 열만 지정 카탈로그 식별자 집합과 대조한다. */
        private static void RequireOptionalColumn<T>(
            IReadOnlyDictionary<string, T> definitions,
            CsvDefinition owner,
            string columnName)
        {
            string rawIds = StringColumn(owner, columnName);
            if (string.IsNullOrEmpty(rawIds))
            {
                return;
            }

            // Trigger CSV의 복수 참조는 세미콜론으로 구분한다.
            foreach (string id in rawIds.Split(';'))
            {
                Require(definitions, owner, columnName, id);
            }
        }

        /* 선택 식별자가 존재할 때 지정 카탈로그에 포함되는지 검증한다. */
        private static void RequireOptional<T>(
            IReadOnlyDictionary<string, T> definitions,
            CsvDefinition owner,
            string columnName,
            string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                Require(definitions, owner, columnName, id);
            }
        }

        /* 필수 식별자가 지정 카탈로그에 포함되는지 검증한다. */
        private static void Require<T>(
            IReadOnlyDictionary<string, T> definitions,
            CsvDefinition owner,
            string columnName,
            string id)
        {
            if (string.IsNullOrEmpty(id) || !definitions.ContainsKey(id))
            {
                throw Missing(owner, columnName, id);
            }
        }

        /* 누락된 참조의 소유 행과 열을 포함한 오류를 생성한다. */
        private static InvalidDataException Missing(
            CsvDefinition owner,
            string columnName,
            string id)
        {
            return Invalid(
                owner.SourcePath,
                owner.SourceRecordNumber,
                $"Missing reference '{id ?? string.Empty}' from column '{columnName}'.");
        }

        /* 정의의 원시 열 사전에서 문자열 값을 읽는다. */
        private static string StringColumn(CsvDefinition definition, string columnName)
        {
            if (!definition.Columns.TryGetValue(columnName, out object value))
            {
                return null;
            }

            return value as string;
        }

        /* 노드 인수와 파라미터 계약을 포함한 검증 오류를 생성한다. */
        private static InvalidDataException InvalidNodeArgument(
            ChoiceNodeDefinition node,
            NodeParamDefinition parameter,
            string argument)
        {
            return Invalid(
                node.SourcePath,
                node.SourceRecordNumber,
                $"Node argument '{argument}' is invalid for parameter '{parameter.param_key}' ({parameter.value_type}).");
        }

        /* 타입 변환에 실패한 CSV 값의 출처와 기대 타입을 포함한 오류를 생성한다. */
        private static InvalidDataException InvalidValue(
            string path,
            int recordNumber,
            string column,
            string value,
            string type)
        {
            return Invalid(
                path,
                recordNumber,
                $"Value '{value}' in column '{column}' is not a valid {type}.");
        }

        /* 레코드 열 수가 헤더 너비와 같은지 검증한다. */
        private static void RequireWidth(string path, RawRecord record, int expectedWidth)
        {
            if (record.Fields.Count != expectedWidth)
            {
                throw Invalid(
                    path,
                    record.RecordNumber,
                    $"Expected {expectedWidth} columns but found {record.Fields.Count}.");
            }
        }

        /* CSV 경로와 레코드 번호가 포함된 표준 파서 오류를 생성한다. */
        private static InvalidDataException Invalid(
            string path,
            int recordNumber,
            string message)
        {
            return new InvalidDataException($"{path} record {recordNumber}: {message}");
        }

        /* CSV 경로를 빈 값 검증 후 슬래시 표기로 정규화한다. */
        private static string NormalizePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new InvalidDataException("CSV source path is empty.");
            }

            return path.Replace('\\', '/');
        }

        /* CSV enum 스키마 토큰별 허용 문자열 도메인을 만든다. */
        private static IReadOnlyDictionary<string, IReadOnlyCollection<string>>
            CreateEnumDomains()
        {
            Dictionary<string, IReadOnlyCollection<string>> domains =
                new Dictionary<string, IReadOnlyCollection<string>>(StringComparer.Ordinal)
                {
                    ["enum:DamageAttribute"] = Domain(
                        "Darkness", "Fire", "Holy", "Ice", "Lightning", "Physical"),
                    ["enum:EnemyAttackType"] = Domain(
                        "Buffer", "Melee", "MeleeAndRanged", "Ranged"),
                    ["enum:EnemyEncounterRole"] = Domain(
                        "Day10Midboss", "Day5Midboss", "Normal", "StageBoss"),
                    ["enum:EnemyPassiveModifierKind"] = Domain(
                        "CritChanceUp", "CritDamageUp", "DamageUp", "DefenseUp",
                        "HealingUp", "IncomingDamageDown"),
                    ["enum:PakuriCsvSkillKind"] = Domain("Active"),
                    ["enum:RuntimeSkillVisualAnchor"] = Domain("StatusTarget"),
                    ["enum:SkillChoiceGroup"] = Domain(
                        "ActiveEnhancement", "ActiveMaster", "PassiveBase",
                        "PassiveEnhancement"),
                    ["enum:SkillMultiEffectCenterMode"] = Domain(
                        "Caster", "EffectTarget", "NearestEnemy", "PrimarySkillCenter"),
                    ["enum:SkillMultiEffectTargetSelection"] = Domain(
                        "EventTarget", "Nearest", "Owner"),
                    ["enum:SkillMultiEffectTargetShape"] = Domain(
                        "Battlefield", "Circle", "Single"),
                    ["enum:SkillMultiEffectTargetSide"] = Domain(
                        "AllAllies", "Enemy", "Self"),
                    ["enum:SkillRuntimeKind"] = Domain(
                        "AreaAttack", "Buff", "CooldownProjectile", "Field", "Heal",
                        "LineAttack", "MagazineProjectile", "Passive", "Shield",
                        "SingleAttack"),
                    ["enum:SkillSlot"] = Domain("A", "B", "C", "D", "E", "F", "G", "H", "I", "J"),
                    ["enum:SkillTargetSelection"] = Domain(
                        "HighestHealth", "HighestStacks", "LowestHealth", "Nearest"),
                    ["enum:SkillTriggerActionKind"] = Domain(
                        "CooldownRefund", "Effect", "LineAttack", "ReloadReduce",
                        "SingleAttack", "TriggeredSkill"),
                    ["enum:SkillTriggerDamageSource"] = Domain(
                        "EventAppliedDamage", "Fixed", "ShieldAbsorbedAmount",
                        "ShieldAppliedAmount", "TrackedIncomingDamage"),
                    ["enum:SkillTriggerEvent"] = Domain(
                        "CombatStart", "OnKill", "OnMagazineLastProjectileHit",
                        "OnOutgoingDamage", "OnShieldAbsorb", "OnShieldExpire",
                        "OnSkillCast", "OnStatusExpire"),
                    ["enum:StatusEffectClassification"] = Domain("Buff", "Debuff"),
                    ["enum:graph_kind"] = Domain("Effect", "Plan"),
                    ["enum:node_kind"] = Domain(
                        "Action", "CastCondition", "CritModifier", "DamageModifier",
                        "OnExpireAction", "OnHitAction", "OnKillAction"),
                    ["enum:owner_kind"] = Domain("Choice", "Skill", "Trigger"),
                    ["enum:triggered_graph_kind"] = Domain("Effect"),
                    ["enum:triggered_graph_owner_kind"] = Domain("Choice", "Trigger"),
                    ["enum:value_type"] = Domain(
                        "asset_path", "bool", "enum", "float", "int", "skill_id",
                        "status_id", "string")
                };
            return new ReadOnlyDictionary<string, IReadOnlyCollection<string>>(domains);
        }

        /* 허용 열거형 값 집합을 대소문자 구분 불변 컬렉션으로 만든다. */
        private static IReadOnlyCollection<string> Domain(params string[] values)
        {
            return new HashSet<string>(values, StringComparer.Ordinal);
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

        private sealed class ParsedTable
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

        private sealed class ParsedRow
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

        private sealed class RawRecord
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
