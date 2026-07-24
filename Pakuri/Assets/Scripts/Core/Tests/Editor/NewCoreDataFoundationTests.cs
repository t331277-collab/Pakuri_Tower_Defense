using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Pakuri.NewCore.Bootstrap;
using Pakuri.NewCore.Catalog;
using Pakuri.NewCore.Definitions.Skills;
using UnityEngine;

namespace Pakuri.NewCore.Tests
{
    public sealed class NewCoreDataFoundationTests
    {
        private const string CatalogPath =
            "Assets/CSVdata/authoring/catalog/catalog_monsters.csv";
        private const string MonstersPath =
            "Assets/CSVdata/authoring/monster/monsters.csv";
        private const string StageDayPath =
            "Assets/CSVdata/stage_flow/StageDay.csv";
        private const string StageRewardPath =
            "Assets/CSVdata/stage_flow/StageReward.csv";

        [Test]
        public void AllRetainedCsvFilesCreateImmutableDefinitions()
        {
            GameDefinitionCatalog catalog = BootstrapCurrentData();

            Assert.That(catalog.SourceFileCount, Is.EqualTo(42));
            Assert.That(catalog.SchemaFileCount, Is.EqualTo(39));
            Assert.That(catalog.AllDefinitions.Count, Is.EqualTo(1836));
            Assert.That(catalog.Skills.Count, Is.EqualTo(82));
            Assert.That(catalog.Monsters.Count, Is.EqualTo(5));
            Assert.That(catalog.Enemies.Count, Is.EqualTo(16));
            Assert.That(catalog.GetMonster("ariel").role_summary, Does.Contain(","));
            Assert.That(
                catalog.GetMonster("ariel").Schema["primary_attribute"],
                Is.EqualTo("enum:DamageAttribute"));

            IDictionary<string, SkillDefinition> mutableView =
                (IDictionary<string, SkillDefinition>)catalog.Skills;
            Assert.Throws<NotSupportedException>(
                () => mutableView.Add("illegal", catalog.GetSkill("ariel-a")));
        }

        [Test]
        public void QuotedFieldsRetainCommasAndEscapedQuotes()
        {
            Dictionary<string, string> sources = LoadSources();
            const string original =
                "\"파티 강화, 방어막, 신성 피해를 주축으로 하는 서포터 타워.\"";
            const string replacement = "\"파티 강화, \"\"인용\"\"과 쉼표를 보존한다.\"";
            sources[MonstersPath] = ReplaceOnce(sources[MonstersPath], original, replacement);

            GameDefinitionCatalog catalog = new GameBootstrap(sources).Catalog;

            Assert.That(
                catalog.GetMonster("ariel").role_summary,
                Is.EqualTo("파티 강화, \"인용\"과 쉼표를 보존한다."));
        }

        [Test]
        public void QuotedMultilineFieldRetainsActualCrLf()
        {
            Dictionary<string, string> sources = LoadSources();
            const string original =
                "\"파티 강화, 방어막, 신성 피해를 주축으로 하는 서포터 타워.\"";
            const string replacement = "\"첫 줄\r\n둘째 줄\"";
            sources[MonstersPath] = ReplaceOnce(sources[MonstersPath], original, replacement);

            GameDefinitionCatalog catalog = new GameBootstrap(sources).Catalog;

            Assert.That(catalog.GetMonster("ariel").role_summary, Is.EqualTo("첫 줄\n둘째 줄"));
        }

        [Test]
        public void BlankRequiredMonsterDisplayNameFailsInitialization()
        {
            Dictionary<string, string> sources = LoadSources();
            sources[MonstersPath] = ReplaceOnce(
                sources[MonstersPath],
                "\"ariel\",\"아리엘\"",
                "\"ariel\",\"\"");

            InvalidDataException exception = Assert.Throws<InvalidDataException>(
                () => new GameBootstrap(sources));

            Assert.That(exception.Message, Does.Contain("display_name"));
            Assert.That(exception.Message, Does.Contain("required column"));
        }

        [Test]
        public void BlankRequiredMonsterPrimaryAttributeFailsInitialization()
        {
            Dictionary<string, string> sources = LoadSources();
            sources[MonstersPath] = ReplaceOnce(
                sources[MonstersPath],
                "\"신성\",\"Holy\",\"240\"",
                "\"신성\",\"\",\"240\"");

            InvalidDataException exception = Assert.Throws<InvalidDataException>(
                () => new GameBootstrap(sources));

            Assert.That(exception.Message, Does.Contain("primary_attribute"));
            Assert.That(exception.Message, Does.Contain("required column"));
        }

        [Test]
        public void DuplicateIdFailsInitialization()
        {
            Dictionary<string, string> sources = LoadSources();
            sources[CatalogPath] = ReplaceOnce(
                sources[CatalogPath],
                "catalog-monster-vega,vega,4",
                "catalog-monster-ariel,vega,4");

            InvalidDataException exception = Assert.Throws<InvalidDataException>(
                () => new GameBootstrap(sources));

            Assert.That(exception.Message, Does.Contain("Duplicate catalog monster id"));
        }

        [Test]
        public void InvalidPrimitiveFailsInitialization()
        {
            Dictionary<string, string> sources = LoadSources();
            sources[CatalogPath] = ReplaceOnce(
                sources[CatalogPath],
                "catalog-monster-ariel,ariel,0",
                "catalog-monster-ariel,ariel,not-an-int");

            InvalidDataException exception = Assert.Throws<InvalidDataException>(
                () => new GameBootstrap(sources));

            Assert.That(exception.Message, Does.Contain("not a valid int"));
        }

        [Test]
        public void InvalidFloatFailsInitialization()
        {
            Dictionary<string, string> sources = LoadSources();
            sources[StageRewardPath] = ReplaceOnce(
                sources[StageRewardPath],
                "reward-stage1-normal,Normal,1,10,10,0.05",
                "reward-stage1-normal,Normal,1,10,10,not-a-float");

            InvalidDataException exception = Assert.Throws<InvalidDataException>(
                () => new GameBootstrap(sources));

            Assert.That(exception.Message, Does.Contain("not a valid float"));
        }

        [Test]
        public void InvalidBoolFailsInitialization()
        {
            Dictionary<string, string> sources = LoadSources();
            sources[StageDayPath] = ReplaceOnce(
                sources[StageDayPath],
                "stage1-day1-normal,reward-stage1-normal,0,false,false,Stage 1 opening",
                "stage1-day1-normal,reward-stage1-normal,0,not-a-bool,false,Stage 1 opening");

            InvalidDataException exception = Assert.Throws<InvalidDataException>(
                () => new GameBootstrap(sources));

            Assert.That(exception.Message, Does.Contain("not a valid bool"));
        }

        [Test]
        public void InvalidEnumFailsInitialization()
        {
            Dictionary<string, string> sources = LoadSources();
            sources[MonstersPath] = ReplaceOnce(
                sources[MonstersPath],
                "\"Holy\",\"240\"",
                "\"UnknownAttribute\",\"240\"");

            InvalidDataException exception = Assert.Throws<InvalidDataException>(
                () => new GameBootstrap(sources));

            Assert.That(exception.Message, Does.Contain("Invalid enum value"));
            Assert.That(exception.Message, Does.Contain("primary_attribute"));
        }

        [Test]
        public void MissingReferenceFailsInitialization()
        {
            Dictionary<string, string> sources = LoadSources();
            sources[CatalogPath] = ReplaceOnce(
                sources[CatalogPath],
                "catalog-monster-vega,vega,4",
                "catalog-monster-vega,missing-monster,4");

            InvalidDataException exception = Assert.Throws<InvalidDataException>(
                () => new GameBootstrap(sources));

            Assert.That(exception.Message, Does.Contain("Missing reference 'missing-monster'"));
            Assert.That(exception.Message, Does.Contain("monster_id"));
        }

        [Test]
        public void MissingRetainedCsvFailsInitialization()
        {
            Dictionary<string, string> sources = LoadSources();
            sources.Remove(CatalogPath);

            InvalidDataException exception = Assert.Throws<InvalidDataException>(
                () => new GameBootstrap(sources));

            Assert.That(exception.Message, Does.Contain("Required retained CSV is missing"));
            Assert.That(exception.Message, Does.Contain(CatalogPath));
        }

        [Test]
        public void UnterminatedQuotedFieldFailsInitialization()
        {
            Dictionary<string, string> sources = LoadSources();
            sources[CatalogPath] += "\"";

            InvalidDataException exception = Assert.Throws<InvalidDataException>(
                () => new GameBootstrap(sources));

            Assert.That(exception.Message, Does.Contain("Unterminated quoted field"));
        }

        private static GameDefinitionCatalog BootstrapCurrentData()
        {
            return new GameBootstrap(LoadSources()).Catalog;
        }

        private static Dictionary<string, string> LoadSources()
        {
            string csvRoot = Path.Combine(Application.dataPath, "CSVdata");
            string[] files = Directory.GetFiles(csvRoot, "*.csv", SearchOption.AllDirectories);
            Dictionary<string, string> sources =
                new Dictionary<string, string>(StringComparer.Ordinal);
            UTF8Encoding strictUtf8 = new UTF8Encoding(false, true);

            foreach (string file in files)
            {
                string relative = file.Substring(Application.dataPath.Length)
                    .TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                    .Replace('\\', '/');
                sources.Add($"Assets/{relative}", File.ReadAllText(file, strictUtf8));
            }

            Assert.That(files.Length, Is.EqualTo(42));
            return sources;
        }

        private static string ReplaceOnce(string source, string oldValue, string newValue)
        {
            int index = source.IndexOf(oldValue, StringComparison.Ordinal);
            Assert.That(index, Is.GreaterThanOrEqualTo(0), $"Expected text was not found: {oldValue}");
            Assert.That(
                source.IndexOf(oldValue, index + oldValue.Length, StringComparison.Ordinal),
                Is.EqualTo(-1),
                $"Expected text was not unique: {oldValue}");
            return source.Substring(0, index)
                + newValue
                + source.Substring(index + oldValue.Length);
        }
    }
}
