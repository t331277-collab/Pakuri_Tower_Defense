using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Pakuri.Combat;
using UnityEngine;

namespace Pakuri.Data
{
    internal enum PakuriCsvSkillKind
    {
        Active,
        Passive
    }

    internal enum PakuriCsvChoiceGroup
    {
        ActiveEnhancement,
        ActiveMaster,
        PassiveEnhancement
    }

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
            private readonly Dictionary<string, int> headerLookup = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            private CsvTable(string tableName, string[] headers, string[] types, List<CsvRecord> records)
            {
                TableName = tableName;
                Headers = headers;
                Types = types;
                Records = records;

                for (var i = 0; i < headers.Length; i++)
                {
                    headerLookup[headers[i]] = i;
                }
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

                    records.Add(new CsvRecord(tableName, lineIndex + 1, headers, types, cells));
                }

                return new CsvTable(tableName, headers, types, records);
            }
        }

        private sealed class CsvRecord
        {
            private readonly string[] cells;
            private readonly Dictionary<string, int> headerLookup;

            public CsvRecord(string tableName, int rowNumber, string[] headers, string[] types, string[] cells)
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

        private sealed class SourceModel
        {
            public readonly Dictionary<string, CatalogEntryRow> CatalogMonsters = new Dictionary<string, CatalogEntryRow>(StringComparer.OrdinalIgnoreCase);
            public readonly Dictionary<string, CatalogEntryRow> CatalogStageOneEnemies = new Dictionary<string, CatalogEntryRow>(StringComparer.OrdinalIgnoreCase);
            public readonly Dictionary<string, MonsterRow> Monsters = new Dictionary<string, MonsterRow>(StringComparer.OrdinalIgnoreCase);
            public readonly Dictionary<string, RewardChoiceRow> RewardChoices = new Dictionary<string, RewardChoiceRow>(StringComparer.OrdinalIgnoreCase);
            public readonly Dictionary<string, SkillRow> Skills = new Dictionary<string, SkillRow>(StringComparer.OrdinalIgnoreCase);
            public readonly Dictionary<string, SkillChoiceRow> SkillChoices = new Dictionary<string, SkillChoiceRow>(StringComparer.OrdinalIgnoreCase);
            public readonly Dictionary<string, EnemyRow> StageOneEnemies = new Dictionary<string, EnemyRow>(StringComparer.OrdinalIgnoreCase);
        }

        private sealed class CatalogEntryRow
        {
            public string Id;
            public string RefId;
            public int SortOrder;
        }

        private sealed class MonsterRow
        {
            public string Id;
            public string DisplayName;
            public string RoleSummary;
            public string ElementLabel;
            public DamageAttribute PrimaryAttribute;
            public string ActiveSkillName;
            public string PassiveSkillName;
            public string UnitSpritePath;
            public string ProjectileSpritePath;
            public Color UnitColor;
            public Color ProjectileColor;
            public float MaxHealth;
            public float PowerStat;
            public float BaseDamage;
            public float PowerCoefficient;
            public float ProjectileSpeed;
            public float ProjectileLifetime;
            public float ProjectileHitRadius;
            public int MagazineCapacity;
            public float ReloadDuration;
            public float ShotInterval;
            public float StatusChance;
            public string StatusEffectLabel;
            public float BaseAttackPower;
            public float BaseSpellPower;
            public float BaseMoveSpeed;
            public float BaseCriticalChance;
            public float BaseCriticalDamage;
            public float BaseCriticalResistance;
            public float PhysicalDefense;
            public float FireDefense;
            public float LightningDefense;
            public float IceDefense;
            public float DarknessDefense;
            public float HolyDefense;
        }

        private sealed class RewardChoiceRow
        {
            public string Id;
            public string MonsterId;
            public int SortOrder;
            public string Title;
            public string Description;
            public float DamageMultiplier;
            public int MagazineBonus;
            public float ShotIntervalMultiplier;
            public float ReloadDurationMultiplier;
            public float MaxHealthBonus;
            public float StatusChanceBonus;
        }

        private sealed class SkillRow
        {
            public string Id;
            public string MonsterId;
            public PakuriCsvSkillKind SkillKind;
            public SkillSlot Slot;
            public string DisplayName;
            public SkillRuntimeKind RuntimeKind;
            public SkillImplementationState ImplementationState;
            public bool IsDefaultLearned;
            public bool IsAvailableWithoutActiveRequirement;
            public SkillSlot RequiredActiveSlot;
            public string SkillIconPath;
            public string SkillEffectPrefabPath;
            public string DescriptionText;
            public string Summary;
            public DamageAttribute Attribute;
            public float BaseDamage;
            public float AttackPowerCoefficient;
            public float SpellPowerCoefficient;
            public float Range;
            public float Radius;
            public float CooldownSeconds;
            public int MagazineCapacity;
            public float ReloadSeconds;
            public float ShotIntervalSeconds;
            public bool CriticalAllowed;
            public string StatusEffectId;
        }

        private sealed class SkillChoiceRow
        {
            public string Id;
            public string MonsterId;
            public string SkillId;
            public PakuriCsvChoiceGroup ChoiceGroup;
            public int SortOrder;
            public string Title;
            public string DescriptionText;
            public string SkillIconPath;
            public string SkillEffectPrefabPath;
        }

        private sealed class EnemyRow
        {
            public string Id;
            public string DisplayName;
            public EnemyEncounterRole EncounterRole;
            public EnemyAttackType AttackType;
            public DamageAttribute Attribute;
            public string UnitSpritePath;
            public string ProjectileSpritePath;
            public float MaxHealth;
            public float AttackPower;
            public float SpellPower;
            public float MoveSpeed;
            public float CriticalChance;
            public float CriticalDamage;
            public float CriticalResistance;
            public float PhysicalDefense;
            public float FireDefense;
            public float LightningDefense;
            public float IceDefense;
            public float DarknessDefense;
            public float HolyDefense;
            public StageOneEnemySkillKind StageOneSkill;
            public string ActiveSkillName;
            public float ActiveSkillCoefficient;
            public float ActiveSkillCooldown;
            public float ActiveSkillDuration;
            public float ActiveSkillRadius;
            public float ActiveSkillFlatValue;
            public string PassiveSkillName;
            public string PassiveSummary;
        }
    }
}
