using System;
using System.Collections.Generic;
using Pakuri.NewCore.Catalog;
using Pakuri.NewCore.Parsing;

namespace Pakuri.NewCore.Bootstrap
{
    public sealed class GameBootstrap
    {
        public GameBootstrap(IReadOnlyDictionary<string, string> retainedCsvFiles)
        {
            if (retainedCsvFiles == null)
            {
                throw new ArgumentNullException(nameof(retainedCsvFiles));
            }

            Catalog = new CsvParser().Parse(retainedCsvFiles);
        }

        public GameDefinitionCatalog Catalog { get; }
    }
}
