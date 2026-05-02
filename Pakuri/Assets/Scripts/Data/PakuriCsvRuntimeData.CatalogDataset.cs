using System.Collections.Generic;

namespace Pakuri.Data
{
    public static partial class PakuriCsvRuntimeData
    {
        private sealed class CatalogEntryRow
        {
            public string Id;
            public string RefId;
            public int SortOrder;
        }

        private static CatalogEntryRow ParseCatalogEntry(CsvRecord record, string refColumnName)
        {
            return new CatalogEntryRow
            {
                Id = record.ReadRequiredString("id"),
                RefId = record.ReadRequiredString(refColumnName),
                SortOrder = record.ReadInt("sort_order")
            };
        }

        private static void ValidateCatalogEntries<T>(
            Dictionary<string, CatalogEntryRow> entries,
            Dictionary<string, T> targetLookup,
            string tableName,
            List<string> errors)
        {
            if (entries.Count == 0)
            {
                errors.Add($"{tableName} has no rows.");
                return;
            }

            foreach (var entry in entries.Values)
            {
                if (!targetLookup.ContainsKey(entry.RefId))
                {
                    errors.Add($"{tableName} entry '{entry.Id}' references unknown id '{entry.RefId}'.");
                }
            }
        }
    }
}
