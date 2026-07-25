using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Pakuri.NewCore.Bootstrap;
using Pakuri.NewCore.Catalog;
using Pakuri.NewCore.Combat;
using Pakuri.NewCore.Definitions.Stage;
using Pakuri.NewCore.Spawn;
using Pakuri.NewCore.Units.Actors;
using Pakuri.NewCore.Units.Models;
using UnityEngine;

/* run stage 상태와 현재 scene의 stage 진행 연결을 소유한다. */
namespace Pakuri.NewCore.Run
{
    public class StageManager : MonoBehaviour
    {
        [SerializeField] private GameBootstrap combatManager;
        [SerializeField] private SpawnManager unitSpawnManager;
        [SerializeField] private bool startFlowOnStart = true;
        [SerializeField] private float clearCheckInterval = 0.25f;
        [SerializeField] private NexusActor nexusActor;

        private readonly List<UnitBaseModel> fieldUnits =
            new List<UnitBaseModel>();
        private IReadOnlyList<UnitBaseModel> readOnlyFieldUnits;
        private GameDefinitionCatalog catalog;
        private SpawnManager spawnManager;
        private InGameCombatManager runtimeCombatManager;
        private NexusModel nexus;
        private int gold;
        private int darkTrace;

        /* catalog 없는 test/runtime session과 초기 재화를 연결한다. */
        public void Initialize(
            RunSessionModel session,
            int initialGold,
            int initialDarkTrace)
        {
            Initialize(
                session,
                null,
                initialGold,
                initialDarkTrace);
        }

        /* session, catalog, 초기 재화를 stage 상태에 연결한다. */
        public void Initialize(
            RunSessionModel session,
            GameDefinitionCatalog catalog,
            int initialGold,
            int initialDarkTrace)
        {
            Session =
                session;

            this.catalog = catalog;
            gold = initialGold;
            darkTrace = initialDarkTrace;
            readOnlyFieldUnits =
                new ReadOnlyCollection<UnitBaseModel>(fieldUnits);
        }

        public RunSessionModel Session { get; private set; }

        public int Gold => gold;

        public int DarkTrace => darkTrace;

        public bool IsCombatActive { get; private set; }

        public StageDayDefinition CurrentDayDefinition { get; private set; }

        public IReadOnlyList<UnitBaseModel> FieldUnits =>
            readOnlyFieldUnits
            ?? (readOnlyFieldUnits =
                new ReadOnlyCollection<UnitBaseModel>(fieldUnits));

        public bool StartFlowOnStart => startFlowOnStart;

        public float ClearCheckInterval => clearCheckInterval;

        public NexusActor NexusActor => nexusActor;

        public IReadOnlyList<UnitBaseModel> LivingFieldUnits
        {
            get
            {
                List<UnitBaseModel> living =
                    new List<UnitBaseModel>();
                for (int index = 0; index < fieldUnits.Count; index++)
                {
                    if (fieldUnits[index].IsAlive)
                    {
                        living.Add(fieldUnits[index]);
                    }
                }

                return living.AsReadOnly();
            }
        }

        public event Action<bool> CombatResolved;

        /* gameplay 보상으로 Gold를 증가시킨다. */
        public void AddGold(int amount)
        {
            gold = AddCurrency(gold, amount);
        }

        /* 현재 Gold로 요청 비용을 지불할 수 있는지 확인한다. */
        public bool CanSpendGold(int amount)
        {
            return CanSpend(gold, amount);
        }

        /* 지불 가능한 Gold를 차감하고 성공 여부를 반환한다. */
        public bool SpendGold(int amount)
        {
            if (!CanSpendGold(amount))
            {
                return false;
            }

            gold -= amount;
            return true;
        }

        /* gameplay 보상으로 DarkTrace를 증가시킨다. */
        public void AddDarkTrace(int amount)
        {
            darkTrace = AddCurrency(darkTrace, amount);
        }

        /* 현재 DarkTrace로 요청 비용을 지불할 수 있는지 확인한다. */
        public bool CanSpendDarkTrace(int amount)
        {
            return CanSpend(darkTrace, amount);
        }

        /* 지불 가능한 DarkTrace를 차감하고 성공 여부를 반환한다. */
        public bool SpendDarkTrace(int amount)
        {
            if (!CanSpendDarkTrace(amount))
            {
                return false;
            }

            darkTrace -= amount;
            return true;
        }

        /* 활성 stage field에 중복 없는 Unit을 등록한다. */
        public bool TryRegisterFieldUnit(UnitBaseModel unit)
        {
            if (unit == null || fieldUnits.Contains(unit))
            {
                return false;
            }

            fieldUnits.Add(unit);
            return true;
        }

        /* 활성 stage field에서 지정 Unit을 제거한다. */
        public bool TryUnregisterFieldUnit(UnitBaseModel unit)
        {
            return unit != null && fieldUnits.Remove(unit);
        }

        /* 활성 stage field Unit 목록을 비운다. */
        public void ClearFieldUnits()
        {
            fieldUnits.Clear();
        }

        /* 현재 stage 진행이 사용할 SpawnManager authority를 연결한다. */
        public void ConfigureSpawnManager(SpawnManager manager)
        {

            spawnManager = manager;
        }

        /* combat 결과와 Nexus 상태를 stage 진행에 연결한다. */
        public void ConnectCombat(
            InGameCombatManager manager,
            NexusModel nexusModel)
        {

            DisconnectCombat();
            runtimeCombatManager = manager;
            nexus = nexusModel;
            runtimeCombatManager.UnitDefeated += HandleUnitDefeated;
        }

        /* stage가 구독한 combat와 Nexus 연결을 해제한다. */
        public void DisconnectCombat()
        {
            if (runtimeCombatManager != null)
            {
                runtimeCombatManager.UnitDefeated -= HandleUnitDefeated;
            }

            runtimeCombatManager = null;
            nexus = null;
        }

        /* 현재 session day의 encounter queue와 field 상태를 시작한다. */
        public void StartCurrentDay()
        {
            CurrentDayDefinition = FindCurrentDay();
            Session.BeginDay(CurrentDayDefinition);
            PrepareFieldForDay();
            spawnManager.BeginEncounter(
                this,
                GetEncounterRows(
                    CurrentDayDefinition.encounter_id));
            IsCombatActive = true;
            spawnManager.Tick(0f);
            EvaluateCombatCompletion();
        }

        /* 현재 encounter spawn queue를 경과 시간만큼 진행한다. */
        public void TickSpawnSequence(float deltaTime)
        {
            if (!IsCombatActive)
            {
                return;
            }

            spawnManager.Tick(deltaTime);
            EvaluateCombatCompletion();
        }

        /* spawn 종료·생존 적·Nexus 상태로 현재 전투 결과를 판정한다. */
        public void EvaluateCombatCompletion()
        {
            if (!IsCombatActive)
            {
                return;
            }

            if (nexus != null && !nexus.IsAlive)
            {
                IsCombatActive = false;
                Session.MarkDefeat();
                CombatResolved?.Invoke(false);
                return;
            }

            if (spawnManager.HasPendingSpawns
                || HasLivingEnemy())
            {
                return;
            }

            IsCombatActive = false;
            Session.BeginReward();
            CombatResolved?.Invoke(true);
        }

        /* 보상 완료 뒤 다음 authored day로 session을 진행한다. */
        public bool CompleteRewardAndAdvance()
        {
            Session.CompleteReward();
            Session.PrisonerInventory.Clear();

            StageDayDefinition next = FindNextDay();
            if (next == null)
            {
                Session.MarkVictory();
                return false;
            }

            Session.BeginDay(next);
            StartCurrentDay();
            return true;
        }

        /* 현현 Monster를 party와 현재 field에 함께 배치한다. */
        public bool PlaceManifestedMonster(MonsterModel monster)
        {

            return spawnManager.PlaceManifestedMonster(this, monster);
        }

        /* 전달된 SpawnManager가 현재 stage authority인지 확인한다. */
        internal bool OwnsSpawnManager(SpawnManager manager)
        {
            return manager != null
                && ReferenceEquals(spawnManager, manager);
        }

        internal SpawnManager ActiveSpawnManager =>
            spawnManager;

        /* 새 day 시작 전 party Unit만 field에 다시 등록한다. */
        private void PrepareFieldForDay()
        {
            fieldUnits.Clear();
            for (int index = 0;
                index < Session.PartyRoster.Members.Count;
                index++)
            {
                MonsterModel monster =
                    Session.PartyRoster.Members[index];
                monster.ResetForNextDay(index == 0);
                fieldUnits.Add(monster);
            }

            if (nexus != null)
            {
                fieldUnits.Add(nexus);
            }
        }

        /* 현재 field에 전투 가능한 Enemy가 남았는지 확인한다. */
        private bool HasLivingEnemy()
        {
            for (int index = 0; index < fieldUnits.Count; index++)
            {
                if (fieldUnits[index] is EnemyModel
                    && fieldUnits[index].IsAlive)
                {
                    return true;
                }
            }

            return false;
        }

        /* session stage/day에 대응하는 authored day를 찾는다. */
        private StageDayDefinition FindCurrentDay()
        {
            StageDayDefinition found = null;
            foreach (StageDayDefinition day in catalog.StageDays.Values)
            {
                if (day.day == Session.CurrentDay
                    && string.Equals(
                        day.encounter_id,
                        Session.CurrentEncounterId,
                        StringComparison.Ordinal))
                {

                    found = day;
                }
            }

            return found;
        }

        /* 현재 day 다음 순서의 authored day를 찾는다. */
        private StageDayDefinition FindNextDay()
        {
            int stage = CurrentDayDefinition.stage.GetValueOrDefault();
            int dayNumber = CurrentDayDefinition.day.GetValueOrDefault();
            StageDayDefinition sameStage = FindDay(stage, dayNumber + 1);
            return sameStage ?? FindDay(stage + 1, 1);
        }

        /* 지정 stage/day와 정확히 일치하는 authored day를 찾는다. */
        private StageDayDefinition FindDay(int stage, int day)
        {
            StageDayDefinition found = null;
            foreach (StageDayDefinition candidate
                in catalog.StageDays.Values)
            {
                if (candidate.stage != stage
                    || candidate.day != day)
                {
                    continue;
                }

                found = candidate;
            }

            return found;
        }

        /* encounter id에 속한 authored spawn 행을 sequence 순서로 반환한다. */
        private IReadOnlyList<StageEncounterDefinition> GetEncounterRows(
            string encounterId)
        {
            List<StageEncounterDefinition> rows =
                new List<StageEncounterDefinition>();
            for (int index = 0;
                index < catalog.StageEncounters.Count;
                index++)
            {
                StageEncounterDefinition row =
                    catalog.StageEncounters[index];
                if (string.Equals(
                    row.encounter_id,
                    encounterId,
                    StringComparison.Ordinal))
                {
                    rows.Add(row);
                }
            }

            rows.Sort((left, right) =>
                Nullable.Compare(
                    left.spawn_order,
                    right.spawn_order));
            return rows.AsReadOnly();
        }

        /* 패배한 Unit을 현재 stage field에서 제거한다. */
        private void HandleUnitDefeated(UnitBaseModel unit)
        {
            EvaluateCombatCompletion();
        }

        /* 비어 있는 bootstrap과 spawn 참조를 같은 GameObject의 컴포넌트로 연결한다. */
        public void ValidateConnections()
        {
            if (combatManager == null)
            {
                combatManager = GetComponent<GameBootstrap>();
            }

            if (unitSpawnManager == null)
            {
                unitSpawnManager = GetComponent<SpawnManager>();
            }

        }

        /* 재화 증가량을 overflow 없이 더한다. */
        private static int AddCurrency(
            int current,
            int amount)
        {

            return checked(current + amount);
        }

        /* 음수가 아닌 비용이 현재 재화 이하인지 확인한다. */
        private static bool CanSpend(int current, int amount)
        {
            return amount >= 0 && current >= amount;
        }
    }
}
