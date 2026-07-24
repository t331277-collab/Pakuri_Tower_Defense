using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace Pakuri.NewCore.Definitions
{
    public abstract class CsvDefinition
    {
        private readonly IReadOnlyDictionary<string, string> schema;
        private readonly IReadOnlyDictionary<string, object> columns;

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

        protected void ValidateRequired(params string[] columnNames)
        {
            foreach (string columnName in columnNames)
            {
                RequiredString(columnName);
            }
        }

        protected string OptionalString(string columnName)
        {
            if (!columns.TryGetValue(columnName, out object value))
            {
                return null;
            }

            return value as string;
        }

        protected int? OptionalInt(string columnName)
        {
            if (!columns.TryGetValue(columnName, out object value) || value == null)
            {
                return null;
            }

            return (int)value;
        }

        protected float? OptionalFloat(string columnName)
        {
            if (!columns.TryGetValue(columnName, out object value) || value == null)
            {
                return null;
            }

            return (float)value;
        }

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
