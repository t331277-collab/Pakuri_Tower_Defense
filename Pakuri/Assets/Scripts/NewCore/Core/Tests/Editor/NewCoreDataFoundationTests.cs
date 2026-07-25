using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Pakuri.NewCore.Bootstrap;
using Pakuri.NewCore.Catalog;
using Pakuri.NewCore.Definitions.Skills;
using Pakuri.NewCore.Parsing;
using UnityEngine;

/* NewCore CSV 파싱, 정의 생성, 참조 무결성 실패 조건을 검증한다. */
namespace Pakuri.NewCore.Tests
{
    public class NewCoreDataFoundationTests
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
        /* AllRetainedCsvFilesCreateImmutableDefinitions 시나리오의 기대 동작과 상태 변화를 검증한다. */
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
        /* QuotedFieldsRetainCommasAndEscapedQuotes 시나리오의 기대 동작과 상태 변화를 검증한다. */
        public void QuotedFieldsRetainCommasAndEscapedQuotes()
        {
            Dictionary<string, string> sources = LoadSources();
            const string original =
                "\"파티 강화, 방어막, 신성 피해를 주축으로 하는 서포터 타워.\"";
            const string replacement = "\"파티 강화, \"\"인용\"\"과 쉼표를 보존한다.\"";
            sources[MonstersPath] = ReplaceOnce(sources[MonstersPath], original, replacement);

            GameDefinitionCatalog catalog =
                GameBootstrap.CreateCatalog(sources);

            Assert.That(
                catalog.GetMonster("ariel").role_summary,
                Is.EqualTo("파티 강화, \"인용\"과 쉼표를 보존한다."));
        }

        [Test]
        /* QuotedMultilineFieldRetainsActualCrLf 시나리오의 기대 동작과 상태 변화를 검증한다. */
        public void QuotedMultilineFieldRetainsActualCrLf()
        {
            Dictionary<string, string> sources = LoadSources();
            const string original =
                "\"파티 강화, 방어막, 신성 피해를 주축으로 하는 서포터 타워.\"";
            const string replacement = "\"첫 줄\r\n둘째 줄\"";
            sources[MonstersPath] = ReplaceOnce(sources[MonstersPath], original, replacement);

            GameDefinitionCatalog catalog =
                GameBootstrap.CreateCatalog(sources);

            Assert.That(catalog.GetMonster("ariel").role_summary, Is.EqualTo("첫 줄\n둘째 줄"));
        }

        /* BootstrapCurrentData 시나리오의 기대 동작과 상태 변화를 검증한다. */
        private static GameDefinitionCatalog BootstrapCurrentData()
        {
            return GameBootstrap.CreateCatalog(LoadSources());
        }

        /* LoadSources 테스트 입력 데이터를 원본 형식으로 불러온다. */
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

        /* ReplaceOnce 테스트 입력의 지정 부분만 교체해 반환한다. */
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
