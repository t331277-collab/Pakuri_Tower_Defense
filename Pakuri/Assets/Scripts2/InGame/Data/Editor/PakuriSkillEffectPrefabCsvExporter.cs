#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Pakuri.Data
{
    internal static class PakuriSkillEffectPrefabCsvExporter
    {
        private const string MonsterAssetFolder = "Assets/Data/GameData/Monsters";
        private const string ChoiceIdColumnName = "choice_id";
        private const string SkillEffectPrefabPathColumnName = "skill_effect_prefab_path";
        private static readonly string[] MonsterSkillChoicesCsvPaths =
        {
            "Assets/CSVdata/runtime/monster/skills/choices/monster_skill_choices_projectile.csv",
            "Assets/CSVdata/runtime/monster/skills/choices/monster_skill_choices_line_attack.csv",
            "Assets/CSVdata/runtime/monster/skills/choices/monster_skill_choices_area_attack.csv",
            "Assets/CSVdata/runtime/monster/skills/choices/monster_skill_choices_single_attack.csv",
            "Assets/CSVdata/runtime/monster/skills/choices/monster_skill_choices_buff.csv",
            "Assets/CSVdata/runtime/monster/skills/choices/monster_skill_choices_passive.csv"
        };

        [MenuItem("Pakuri/Export Skill Effect Prefabs To CSV")]
        public static void ExportAllMenu()
        {
            var result = ExportAllAssignedEffectPrefabsToCsv();
            Debug.Log(
                "Pakuri skill effect prefab export completed. "
                + $"choices={result.ChoiceRowsUpdated}, assignedChoices={result.AssignedChoicePrefabCount}");
        }

        public static ExportResult ExportAllAssignedEffectPrefabsToCsv()
        {
            var assignedChoicePrefabPaths = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            CollectAssignedPrefabPaths(assignedChoicePrefabPaths);
            var choiceRowsUpdated = 0;
            for (var i = 0; i < MonsterSkillChoicesCsvPaths.Length; i++)
            {
                choiceRowsUpdated += UpdateCsvPrefabPathColumn(
                    MonsterSkillChoicesCsvPaths[i],
                    ChoiceIdColumnName,
                    assignedChoicePrefabPaths);
            }

            for (var i = 0; i < MonsterSkillChoicesCsvPaths.Length; i++)
            {
                AssetDatabase.ImportAsset(MonsterSkillChoicesCsvPaths[i]);
            }
            PakuriCsvRuntimeData.SyncImportedSourceCatalogsForEditor();

            return new ExportResult(
                assignedChoicePrefabPaths.Count,
                choiceRowsUpdated);
        }

        private static void CollectAssignedPrefabPaths(Dictionary<string, string> choicePrefabPaths)
        {
            var monsterGuids = AssetDatabase.FindAssets("t:MonsterDefinition", new[] { MonsterAssetFolder });
            foreach (var guid in monsterGuids)
            {
                var monsterAssetPath = AssetDatabase.GUIDToAssetPath(guid);
                var monster = AssetDatabase.LoadAssetAtPath<MonsterDefinition>(monsterAssetPath);
                if (monster == null)
                {
                    continue;
                }

                CollectActiveSkillPrefabPaths(monster.ActiveSkills, choicePrefabPaths, monsterAssetPath);
                CollectPassiveSkillPrefabPaths(monster.PassiveSkills, choicePrefabPaths, monsterAssetPath);
            }
        }

        private static void CollectActiveSkillPrefabPaths(
            SkillDefinition[] skills,
            Dictionary<string, string> choicePrefabPaths,
            string ownerAssetPath)
        {
            if (skills == null)
            {
                return;
            }

            foreach (var skill in skills)
            {
                if (skill == null)
                {
                    continue;
                }

                CollectChoicePrefabPaths(skill.EnhancementChoices, choicePrefabPaths, ownerAssetPath);
                CollectChoicePrefabPaths(skill.MasterSkillChoices, choicePrefabPaths, ownerAssetPath);
            }
        }

        private static void CollectPassiveSkillPrefabPaths(
            PassiveDefinition[] passives,
            Dictionary<string, string> choicePrefabPaths,
            string ownerAssetPath)
        {
            if (passives == null)
            {
                return;
            }

            foreach (var passive in passives)
            {
                if (passive == null)
                {
                    continue;
                }

                CollectChoicePrefabPaths(passive.EnhancementChoices, choicePrefabPaths, ownerAssetPath);
            }
        }

        private static void CollectChoicePrefabPaths(
            SkillChoiceDefinition[] choices,
            Dictionary<string, string> choicePrefabPaths,
            string ownerAssetPath)
        {
            if (choices == null)
            {
                return;
            }

            foreach (var choice in choices)
            {
                if (choice == null)
                {
                    continue;
                }

                AddPrefabPath(choicePrefabPaths, choice.ChoiceId, choice.SkillEffectPrefab, ownerAssetPath);
            }
        }

        private static void AddPrefabPath(
            Dictionary<string, string> prefabPaths,
            string id,
            GameObject prefab,
            string ownerAssetPath)
        {
            if (string.IsNullOrWhiteSpace(id) || prefab == null)
            {
                return;
            }

            var prefabAssetPath = AssetDatabase.GetAssetPath(prefab);
            if (string.IsNullOrWhiteSpace(prefabAssetPath))
            {
                throw new InvalidOperationException(
                    $"{ownerAssetPath} has SkillEffectPrefab assigned for '{id}', but AssetDatabase returned no asset path.");
            }

            prefabAssetPath = prefabAssetPath.Trim().Replace('\\', '/');
            if (!prefabAssetPath.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    $"{ownerAssetPath} has SkillEffectPrefab assigned for '{id}', but '{prefabAssetPath}' is outside Assets.");
            }

            if (prefabPaths.TryGetValue(id, out var existingPath)
                && !string.Equals(existingPath, prefabAssetPath, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    $"SkillEffectPrefab conflict for '{id}': '{existingPath}' and '{prefabAssetPath}'.");
            }

            prefabPaths[id] = prefabAssetPath;
        }

        private static int UpdateCsvPrefabPathColumn(
            string assetPath,
            string idColumnName,
            Dictionary<string, string> prefabPathsById)
        {
            if (prefabPathsById.Count == 0)
            {
                return 0;
            }

            var fullPath = ToFullPath(assetPath);
            if (!File.Exists(fullPath))
            {
                throw new FileNotFoundException($"CSV file is missing at '{assetPath}'.", fullPath);
            }

            var lines = new List<string>(File.ReadAllLines(fullPath, Encoding.UTF8));
            if (lines.Count < 2)
            {
                throw new InvalidOperationException($"CSV file '{assetPath}' must contain a header row and a type row.");
            }

            var headers = PakuriCsvLineCodec.SplitLineToList(lines[0]);
            var idColumnIndex = FindColumnIndex(headers, idColumnName, assetPath);
            var prefabPathColumnIndex = FindOptionalColumnIndex(headers, SkillEffectPrefabPathColumnName);
            if (prefabPathColumnIndex < 0)
            {
                if (!HasMatchingPrefabRow(lines, idColumnIndex, prefabPathsById))
                {
                    return 0;
                }

                prefabPathColumnIndex = headers.Count;
                headers.Add(SkillEffectPrefabPathColumnName);
                lines[0] = PakuriCsvLineCodec.JoinLine(headers);

                var typeCells = PakuriCsvLineCodec.SplitLineToList(lines[1]);
                EnsureCellCount(typeCells, headers.Count);
                typeCells[prefabPathColumnIndex] = "asset_path";
                lines[1] = PakuriCsvLineCodec.JoinLine(typeCells);
            }

            var updatedRows = 0;

            for (var i = 2; i < lines.Count; i++)
            {
                var cells = PakuriCsvLineCodec.SplitLineToList(lines[i]);
                if (idColumnIndex >= cells.Count)
                {
                    continue;
                }

                var rowId = cells[idColumnIndex];
                if (string.IsNullOrWhiteSpace(rowId)
                    || !prefabPathsById.TryGetValue(rowId, out var prefabPath))
                {
                    continue;
                }

                EnsureCellCount(cells, headers.Count);
                if (string.Equals(cells[prefabPathColumnIndex], prefabPath, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                cells[prefabPathColumnIndex] = prefabPath;
                lines[i] = PakuriCsvLineCodec.JoinLine(cells);
                updatedRows++;
            }

            if (updatedRows > 0)
            {
                File.WriteAllText(fullPath, string.Join("\r\n", lines) + "\r\n", new UTF8Encoding(false));
            }

            return updatedRows;
        }

        private static string ToFullPath(string assetPath)
        {
            if (!assetPath.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException($"Asset path must start with 'Assets/': {assetPath}", nameof(assetPath));
            }

            var relativePath = assetPath.Substring("Assets/".Length).Replace('/', Path.DirectorySeparatorChar);
            return Path.Combine(Application.dataPath, relativePath);
        }

        private static int FindColumnIndex(IReadOnlyList<string> headers, string columnName, string assetPath)
        {
            for (var i = 0; i < headers.Count; i++)
            {
                if (string.Equals(headers[i], columnName, StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }
            }

            throw new InvalidOperationException($"CSV file '{assetPath}' is missing column '{columnName}'.");
        }

        private static int FindOptionalColumnIndex(IReadOnlyList<string> headers, string columnName)
        {
            for (var i = 0; i < headers.Count; i++)
            {
                if (string.Equals(headers[i], columnName, StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }
            }

            return -1;
        }

        private static bool HasMatchingPrefabRow(
            IReadOnlyList<string> lines,
            int idColumnIndex,
            Dictionary<string, string> prefabPathsById)
        {
            for (var i = 2; i < lines.Count; i++)
            {
                var cells = PakuriCsvLineCodec.SplitLineToList(lines[i]);
                if (idColumnIndex >= cells.Count)
                {
                    continue;
                }

                var rowId = cells[idColumnIndex];
                if (!string.IsNullOrWhiteSpace(rowId) && prefabPathsById.ContainsKey(rowId))
                {
                    return true;
                }
            }

            return false;
        }

        private static void EnsureCellCount(List<string> cells, int requiredCount)
        {
            while (cells.Count < requiredCount)
            {
                cells.Add(string.Empty);
            }
        }

        public readonly struct ExportResult
        {
            public ExportResult(
                int assignedChoicePrefabCount,
                int choiceRowsUpdated)
            {
                AssignedChoicePrefabCount = assignedChoicePrefabCount;
                ChoiceRowsUpdated = choiceRowsUpdated;
            }

            public int AssignedChoicePrefabCount { get; }
            public int ChoiceRowsUpdated { get; }
        }
    }

    [CustomEditor(typeof(MonsterDefinition))]
    internal sealed class MonsterDefinitionEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space(8f);
            if (GUILayout.Button("Export Skill Effect Prefabs To CSV"))
            {
                var result = PakuriSkillEffectPrefabCsvExporter.ExportAllAssignedEffectPrefabsToCsv();
                Debug.Log(
                    "Pakuri skill effect prefab export completed from MonsterDefinition inspector. "
                    + $"choices={result.ChoiceRowsUpdated}, assignedChoices={result.AssignedChoicePrefabCount}");
            }
        }
    }
}
#endif
