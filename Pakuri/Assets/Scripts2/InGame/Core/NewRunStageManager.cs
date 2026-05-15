using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Pakuri.Run;
using UnityEngine;

namespace Pakuri.InGame
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(NewRunSceneEntryManager))]
    [RequireComponent(typeof(InGameCombatManager))]
    public sealed class NewRunStageManager : MonoBehaviour
    {
        private const float DefaultClearCheckInterval = 0.25f;

        private readonly List<StageEncounterRow> activeEncounterRows = new List<StageEncounterRow>();
        private readonly List<string> pendingPrisonerEnemyIds = new List<string>();
        private readonly List<string> prisonerCandidatePool = new List<string>();

        [SerializeField] private NewRunSceneEntryManager entryManager;
        [SerializeField] private InGameCombatManager combatManager;
        [SerializeField] private TextAsset stageDayCsv;
        [SerializeField] private TextAsset stageEncounterCsv;
        [SerializeField] private TextAsset stageRewardCsv;
        [SerializeField] private bool startFlowOnStart = true;
        [SerializeField] private float clearCheckInterval = DefaultClearCheckInterval;

        private StageFlowTable table = new StageFlowTable();
        private Coroutine flowCoroutine;
        private StageDayRow currentDay;
        private StageRewardRow currentReward;
        private RunSession activeSession;

        public NewRunStageState State { get; private set; } = NewRunStageState.NotStarted;
        public int CurrentStage => activeSession != null ? activeSession.StageIndex : 1;
        public int CurrentDay => activeSession != null ? activeSession.DayIndex : 1;
        public IReadOnlyList<string> PendingPrisonerEnemyIds => pendingPrisonerEnemyIds;
        public int PendingGoldReward { get; private set; }
        public int PendingDarkTraceReward { get; private set; }
        public int PendingPrisonerCount { get; private set; }
        public float PendingManifestSuccessChance => currentReward != null ? currentReward.ManifestSuccessChance : 0.7f;
        public RunSession ActiveSession => activeSession;
        public string CurrentEncounterId => currentDay != null ? currentDay.EncounterId : string.Empty;
        public string CurrentRewardRuleId => currentReward != null ? currentReward.RewardRuleId : string.Empty;

        private void Start()
        {
            ResolveReferences();
            LoadTables();

            if (startFlowOnStart)
            {
                StartCurrentDay();
            }
        }

        public void StartCurrentDay()
        {
            if (flowCoroutine != null)
            {
                StopCoroutine(flowCoroutine);
            }

            flowCoroutine = StartCoroutine(RunCurrentDayFlow());
        }

        public void ContinueToNextDay()
        {
            if (State != NewRunStageState.RewardReady)
            {
                Debug.LogWarning("NewRunStageManager cannot continue because reward state is not ready.");
                return;
            }

            if (activeSession == null)
            {
                Debug.LogWarning("NewRunStageManager cannot continue because no active run session exists.");
                return;
            }

            pendingPrisonerEnemyIds.Clear();
            PendingGoldReward = 0;
            PendingDarkTraceReward = 0;
            PendingPrisonerCount = 0;
            activeSession.AdvanceDay();
            StartCurrentDay();
        }

        private IEnumerator RunCurrentDayFlow()
        {
            ResolveReferences();
            if (entryManager != null)
            {
                entryManager.SpawnSelectedPlayerUnit();
            }

            activeSession = entryManager != null ? entryManager.ActiveSession : null;
            if (activeSession == null)
            {
                State = NewRunStageState.Error;
                Debug.LogError("NewRunStageManager could not start because NewRunSceneEntryManager has no active session.");
                yield break;
            }

            currentDay = table.FindDay(activeSession.StageIndex, activeSession.DayIndex);
            if (currentDay == null)
            {
                State = NewRunStageState.Error;
                Debug.LogError($"NewRunStageManager has no StageDay row for stage {activeSession.StageIndex}, day {activeSession.DayIndex}.");
                yield break;
            }

            currentReward = table.FindReward(currentDay.RewardRuleId);
            if (currentReward == null)
            {
                State = NewRunStageState.Error;
                Debug.LogError($"NewRunStageManager has no StageReward row for '{currentDay.RewardRuleId}'.");
                yield break;
            }

            table.FindEncounterRows(currentDay.EncounterId, activeEncounterRows);
            if (activeEncounterRows.Count == 0)
            {
                State = NewRunStageState.Error;
                Debug.LogError($"NewRunStageManager has no StageEncounter rows for '{currentDay.EncounterId}'.");
                yield break;
            }

            SelectBossRows();
            State = NewRunStageState.Spawning;
            yield return SpawnEncounterRows(activeEncounterRows);

            State = NewRunStageState.Combat;
            yield return WaitForEnemyClear();

            PrepareReward();
            State = NewRunStageState.RewardReady;
            flowCoroutine = null;
        }

        private IEnumerator SpawnEncounterRows(IReadOnlyList<StageEncounterRow> rows)
        {
            var spawnIndex = 0;
            for (var i = 0; i < rows.Count; i++)
            {
                var row = rows[i];
                var count = Mathf.Max(0, row.Count);
                for (var j = 0; j < count; j++)
                {
                    if (entryManager != null)
                    {
                        var healthMultiplier = row.SelectedAsBoss
                            ? UnityEngine.Random.Range(row.BossHealthMultiplierMin, row.BossHealthMultiplierMax)
                            : 1f;
                        entryManager.SpawnEnemyById(
                            row.EnemyId,
                            spawnIndex,
                            row.SpawnX,
                            row.SpawnYMin,
                            row.SpawnYMax,
                            healthMultiplier,
                            out _);
                    }

                    spawnIndex += 1;
                    yield return new WaitForSeconds(Mathf.Max(0f, row.IntervalSeconds));
                }
            }
        }

        private IEnumerator WaitForEnemyClear()
        {
            var wait = new WaitForSeconds(Mathf.Max(0.05f, clearCheckInterval));
            while (combatManager != null && combatManager.ActiveEnemyCount > 0)
            {
                yield return wait;
            }
        }

        private void PrepareReward()
        {
            pendingPrisonerEnemyIds.Clear();
            PendingGoldReward = currentReward != null ? currentReward.Gold : 0;
            PendingDarkTraceReward = currentReward != null ? currentReward.DarkTrace : 0;
            PendingPrisonerCount = currentReward != null ? currentReward.RollPrisonerCount() : 0;

            AddGuaranteedPrisoners();
            BuildPrisonerCandidatePool();
            AddCandidatePrisonersUntilFull();

            var prisonerSummary = string.Join("|", pendingPrisonerEnemyIds);
            Debug.Log(
                $"Stage reward ready: stage={activeSession.StageIndex}, day={activeSession.DayIndex}, " +
                $"gold={PendingGoldReward}, darkTrace={PendingDarkTraceReward}, prisoners={prisonerSummary}");
        }

        private void AddGuaranteedPrisoners()
        {
            for (var i = 0; i < activeEncounterRows.Count; i++)
            {
                var row = activeEncounterRows[i];
                if (row.GuaranteedPrisoner || row.SelectedAsBoss)
                {
                    AddPrisoner(row.EnemyId);
                }
            }
        }

        private void BuildPrisonerCandidatePool()
        {
            prisonerCandidatePool.Clear();
            for (var i = 0; i < activeEncounterRows.Count; i++)
            {
                var row = activeEncounterRows[i];
                for (var count = 0; count < row.Count; count++)
                {
                    prisonerCandidatePool.Add(row.EnemyId);
                }
            }

            for (var i = 0; i < pendingPrisonerEnemyIds.Count; i++)
            {
                RemoveOnePrisonerCandidate(pendingPrisonerEnemyIds[i]);
            }
        }

        private void AddCandidatePrisonersUntilFull()
        {
            if (PendingPrisonerCount <= 0)
            {
                return;
            }

            while (pendingPrisonerEnemyIds.Count < PendingPrisonerCount && prisonerCandidatePool.Count > 0)
            {
                var poolIndex = UnityEngine.Random.Range(0, prisonerCandidatePool.Count);
                AddPrisoner(prisonerCandidatePool[poolIndex]);
                prisonerCandidatePool.RemoveAt(poolIndex);
            }
        }

        private void AddPrisoner(string enemyId)
        {
            if (string.IsNullOrWhiteSpace(enemyId))
            {
                return;
            }

            pendingPrisonerEnemyIds.Add(enemyId);
        }

        private void RemoveOnePrisonerCandidate(string enemyId)
        {
            if (string.IsNullOrWhiteSpace(enemyId))
            {
                return;
            }

            for (var i = 0; i < prisonerCandidatePool.Count; i++)
            {
                if (string.Equals(prisonerCandidatePool[i], enemyId, StringComparison.OrdinalIgnoreCase))
                {
                    prisonerCandidatePool.RemoveAt(i);
                    return;
                }
            }
        }

        private void SelectBossRows()
        {
            var normalBossCandidates = new List<StageEncounterRow>();
            for (var i = 0; i < activeEncounterRows.Count; i++)
            {
                var row = activeEncounterRows[i];
                row.SelectedAsBoss = false;
                if (row.IsGuaranteedBoss)
                {
                    row.SelectedAsBoss = true;
                    continue;
                }

                if (row.IsBossCandidate)
                {
                    normalBossCandidates.Add(row);
                }
            }

            if (normalBossCandidates.Count == 0)
            {
                return;
            }

            normalBossCandidates[UnityEngine.Random.Range(0, normalBossCandidates.Count)].SelectedAsBoss = true;
        }

        private void ResolveReferences()
        {
            if (entryManager == null)
            {
                entryManager = GetComponent<NewRunSceneEntryManager>();
            }

            if (combatManager == null)
            {
                combatManager = GetComponent<InGameCombatManager>();
            }
        }

        private void LoadTables()
        {
            table = StageFlowTable.Load(stageDayCsv, stageEncounterCsv, stageRewardCsv);
        }
    }

    public enum NewRunStageState
    {
        NotStarted,
        Spawning,
        Combat,
        RewardReady,
        Error
    }

    internal sealed class StageDayRow
    {
        public int Stage;
        public int Day;
        public string CombatType;
        public string EncounterId;
        public string RewardRuleId;
    }

    internal sealed class StageEncounterRow
    {
        public string EncounterId;
        public int SpawnOrder;
        public string EnemyId;
        public int Count;
        public float IntervalSeconds;
        public float SpawnX;
        public float SpawnYMin;
        public float SpawnYMax;
        public bool IsBossCandidate;
        public bool IsGuaranteedBoss;
        public float BossHealthMultiplierMin;
        public float BossHealthMultiplierMax;
        public bool GuaranteedPrisoner;
        public bool SelectedAsBoss;
    }

    internal sealed class StageRewardRow
    {
        public string RewardRuleId;
        public int Gold;
        public int DarkTrace;
        public float PrisonerCount1Chance;
        public float PrisonerCount2Chance;
        public float PrisonerCount3Chance;
        public float ManifestSuccessChance;
        public int EliteBonusPrisoners;

        public int RollPrisonerCount()
        {
            var roll = UnityEngine.Random.value;
            if (roll < PrisonerCount1Chance)
            {
                return 1 + EliteBonusPrisoners;
            }

            if (roll < PrisonerCount1Chance + PrisonerCount2Chance)
            {
                return 2 + EliteBonusPrisoners;
            }

            return 3 + EliteBonusPrisoners;
        }
    }

    internal sealed class StageFlowTable
    {
        private readonly List<StageDayRow> days = new List<StageDayRow>();
        private readonly List<StageEncounterRow> encounters = new List<StageEncounterRow>();
        private readonly List<StageRewardRow> rewards = new List<StageRewardRow>();

        public static StageFlowTable Load(TextAsset dayCsv, TextAsset encounterCsv, TextAsset rewardCsv)
        {
            var table = new StageFlowTable();
            table.LoadDays(dayCsv);
            table.LoadEncounters(encounterCsv);
            table.LoadRewards(rewardCsv);
            return table;
        }

        public StageDayRow FindDay(int stage, int day)
        {
            for (var i = 0; i < days.Count; i++)
            {
                var row = days[i];
                if (row.Stage == stage && row.Day == day)
                {
                    return row;
                }
            }

            return null;
        }

        public StageRewardRow FindReward(string rewardRuleId)
        {
            for (var i = 0; i < rewards.Count; i++)
            {
                var row = rewards[i];
                if (string.Equals(row.RewardRuleId, rewardRuleId, StringComparison.OrdinalIgnoreCase))
                {
                    return row;
                }
            }

            return null;
        }

        public void FindEncounterRows(string encounterId, List<StageEncounterRow> results)
        {
            results.Clear();
            for (var i = 0; i < encounters.Count; i++)
            {
                var row = encounters[i];
                if (string.Equals(row.EncounterId, encounterId, StringComparison.OrdinalIgnoreCase))
                {
                    results.Add(row);
                }
            }

            results.Sort((left, right) => left.SpawnOrder.CompareTo(right.SpawnOrder));
        }

        private void LoadDays(TextAsset csv)
        {
            foreach (var row in ReadRows(csv))
            {
                days.Add(new StageDayRow
                {
                    Stage = ParseInt(row, "stage"),
                    Day = ParseInt(row, "day"),
                    CombatType = Read(row, "combat_type"),
                    EncounterId = Read(row, "encounter_id"),
                    RewardRuleId = Read(row, "reward_rule_id")
                });
            }
        }

        private void LoadEncounters(TextAsset csv)
        {
            foreach (var row in ReadRows(csv))
            {
                encounters.Add(new StageEncounterRow
                {
                    EncounterId = Read(row, "encounter_id"),
                    SpawnOrder = ParseInt(row, "spawn_order"),
                    EnemyId = Read(row, "enemy_id"),
                    Count = ParseInt(row, "count"),
                    IntervalSeconds = ParseFloat(row, "interval_sec"),
                    SpawnX = ParseFloat(row, "spawn_x"),
                    SpawnYMin = ParseFloat(row, "spawn_y_min"),
                    SpawnYMax = ParseFloat(row, "spawn_y_max"),
                    IsBossCandidate = ParseBool(row, "is_boss_candidate"),
                    IsGuaranteedBoss = ParseBool(row, "is_guaranteed_boss"),
                    BossHealthMultiplierMin = ParseFloat(row, "boss_health_multiplier_min", 1f),
                    BossHealthMultiplierMax = ParseFloat(row, "boss_health_multiplier_max", 1f),
                    GuaranteedPrisoner = ParseBool(row, "guaranteed_prisoner")
                });
            }
        }

        private void LoadRewards(TextAsset csv)
        {
            foreach (var row in ReadRows(csv))
            {
                rewards.Add(new StageRewardRow
                {
                    RewardRuleId = Read(row, "reward_rule_id"),
                    Gold = ParseInt(row, "gold"),
                    DarkTrace = ParseInt(row, "dark_trace"),
                    PrisonerCount1Chance = ParseFloat(row, "prisoner_count_1_chance"),
                    PrisonerCount2Chance = ParseFloat(row, "prisoner_count_2_chance"),
                    PrisonerCount3Chance = ParseFloat(row, "prisoner_count_3_chance"),
                    ManifestSuccessChance = ParseFloat(row, "manifest_success_chance", 0.7f),
                    EliteBonusPrisoners = ParseInt(row, "elite_bonus_prisoners")
                });
            }
        }

        private static IEnumerable<Dictionary<string, string>> ReadRows(TextAsset csv)
        {
            if (csv == null || string.IsNullOrWhiteSpace(csv.text))
            {
                yield break;
            }

            var lines = csv.text.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
            if (lines.Length == 0)
            {
                yield break;
            }

            var headers = SplitCsvLine(lines[0]);
            for (var i = 1; i < lines.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(lines[i]))
                {
                    continue;
                }

                var values = SplitCsvLine(lines[i]);
                var row = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                for (var j = 0; j < headers.Count; j++)
                {
                    row[headers[j]] = j < values.Count ? values[j] : string.Empty;
                }

                yield return row;
            }
        }

        private static List<string> SplitCsvLine(string line)
        {
            var values = new List<string>();
            var current = string.Empty;
            var inQuotes = false;

            for (var i = 0; i < line.Length; i++)
            {
                var c = line[i];
                if (c == '"')
                {
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        current += '"';
                        i += 1;
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }
                }
                else if (c == ',' && !inQuotes)
                {
                    values.Add(current);
                    current = string.Empty;
                }
                else
                {
                    current += c;
                }
            }

            values.Add(current);
            return values;
        }

        private static string Read(Dictionary<string, string> row, string key)
        {
            return row.TryGetValue(key, out var value) ? value.Trim() : string.Empty;
        }

        private static int ParseInt(Dictionary<string, string> row, string key)
        {
            return int.TryParse(Read(row, key), NumberStyles.Integer, CultureInfo.InvariantCulture, out var value)
                ? value
                : 0;
        }

        private static float ParseFloat(Dictionary<string, string> row, string key, float fallback = 0f)
        {
            return float.TryParse(Read(row, key), NumberStyles.Float, CultureInfo.InvariantCulture, out var value)
                ? value
                : fallback;
        }

        private static bool ParseBool(Dictionary<string, string> row, string key)
        {
            return bool.TryParse(Read(row, key), out var value) && value;
        }
    }
}
