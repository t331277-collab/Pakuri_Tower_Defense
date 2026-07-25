using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Pakuri.NewCore.Bootstrap;
using Pakuri.NewCore.Catalog;
using Pakuri.NewCore.Combat;
using Pakuri.NewCore.Combat.Actions;
using Pakuri.NewCore.Combat.Skills.Execution;
using Pakuri.NewCore.Definitions.Choices;
using Pakuri.NewCore.Definitions.Skills;
using Pakuri.NewCore.Definitions.Stage;
using Pakuri.NewCore.Definitions.Units;
using Pakuri.NewCore.Run;
using Pakuri.NewCore.Units.Actors;
using Pakuri.NewCore.Units.Models;
using UnityEngine;

/* encounter Model 생성과 Monster/Enemy prefab Actor 생성을 한 spawn authority에서 소유한다. */
namespace Pakuri.NewCore.Spawn
{
    public class SpawnedEnemyRecord
    {
        /* 생성된 Enemy Model과 authored spawn 위치를 한 record로 저장한다. */
        internal SpawnedEnemyRecord(
            EnemyModel model,
            StageEncounterDefinition encounter,
            bool isBoss)
        {
            Model = model;
            Encounter = encounter;
            IsBoss = isBoss;
        }

        public EnemyModel Model { get; }

        public StageEncounterDefinition Encounter { get; }

        public bool IsBoss { get; }

        public bool GuaranteesPrisoner =>
            Encounter.guaranteed_prisoner == true;
    }

    public class SpawnManager : MonoBehaviour
    {
        [Serializable]
        public struct EnemyPrefabBinding
        {
            [SerializeField] private string enemyId;
            [SerializeField] private GameObject prefab;

            public string EnemyId => enemyId;

            public GameObject Prefab => prefab;
        }

        [SerializeField] private GameBootstrap combatManager;
        [SerializeField] private Transform playerSpawnPoint;
        [SerializeField] private Transform enemySpawnPoint;
        [SerializeField] private Transform runtimeEnemyRoot;
        [SerializeField] private Transform runtimeMonsterRoot;
        [SerializeField] private GameObject arielUnitPrefab;
        [SerializeField] private GameObject eveUnitPrefab;
        [SerializeField] private GameObject rinUnitPrefab;
        [SerializeField] private GameObject seinUnitPrefab;
        [SerializeField] private GameObject vegaUnitPrefab;
        [SerializeField] private EnemyPrefabBinding[] enemyPrefabBindings =
            Array.Empty<EnemyPrefabBinding>();

        private GameDefinitionCatalog catalog;
        private Func<int, int> randomIndex;
        private Func<float> randomValue;
        private readonly List<SpawnEntry> pending =
            new List<SpawnEntry>();
        private readonly List<SpawnedEnemyRecord> spawned =
            new List<SpawnedEnemyRecord>();
        private readonly Dictionary<MonsterModel, MonsterActor> monsters =
            new Dictionary<MonsterModel, MonsterActor>();
        private readonly Dictionary<EnemyModel, EnemyActor> enemies =
            new Dictionary<EnemyModel, EnemyActor>();
        private readonly Dictionary<string, GameObject> enemyPrefabs =
            new Dictionary<string, GameObject>(StringComparer.Ordinal);
        private readonly List<EnemyModel> defeatedEnemies =
            new List<EnemyModel>();
        private IReadOnlyList<SpawnedEnemyRecord> readOnlySpawned;
        private StageManager stageManager;
        private GameBootstrap runtime;
        private int nextIndex;
        private float nextDelay;

        /* catalog과 random sources를 runtime spawn 상태에 연결한다. */
        public void Initialize(
            GameDefinitionCatalog catalog,
            Func<int, int> randomIndex,
            Func<float> randomValue)
        {
            this.catalog =
                catalog;
            this.randomIndex =
                randomIndex;
            this.randomValue =
                randomValue;
            readOnlySpawned =
                new ReadOnlyCollection<SpawnedEnemyRecord>(spawned);
        }

        public bool HasPendingSpawns => nextIndex < pending.Count;

        public IReadOnlyList<SpawnedEnemyRecord> SpawnedEnemies =>
            readOnlySpawned
            ?? (readOnlySpawned =
                new ReadOnlyCollection<SpawnedEnemyRecord>(spawned));

        /* stage encounter 행을 authored 순서의 spawn queue로 초기화한다. */
        public void BeginEncounter(
            StageManager stage,
            IReadOnlyList<StageEncounterDefinition> encounterRows)
        {

            stageManager = stage;
            pending.Clear();
            spawned.Clear();
            nextIndex = 0;

            List<StageEncounterDefinition> rows =
                new List<StageEncounterDefinition>(encounterRows);
            rows.Sort((left, right) =>
                Nullable.Compare(
                    left.spawn_order,
                    right.spawn_order));
            string encounterId = rows[0].encounter_id;
            List<int> bossCandidates = new List<int>();
            for (int index = 0; index < rows.Count; index++)
            {

                if (rows[index].is_boss_candidate == true)
                {
                    bossCandidates.Add(index);
                }
            }

            int selectedBossRow = -1;
            if (bossCandidates.Count > 0)
            {
                selectedBossRow = bossCandidates[
                    ResolveRandomIndex(bossCandidates.Count)];
            }

            for (int rowIndex = 0;
                rowIndex < rows.Count;
                rowIndex++)
            {
                StageEncounterDefinition row = rows[rowIndex];
                int count = RequiredPositive(
                    row.count,
                    row,
                    "count");
                float interval = RequiredNonNegative(
                    row.interval_sec,
                    row,
                    "interval_sec");
                for (int instance = 0; instance < count; instance++)
                {
                    bool isBoss = row.is_guaranteed_boss == true
                        || (rowIndex == selectedBossRow
                            && instance == 0);
                    float delay = interval;
                    if (pending.Count == 0)
                    {
                        delay = 0f;
                    }

                    pending.Add(new SpawnEntry(
                        row,
                        isBoss,
                        delay));
                }
            }

            nextDelay = pending[0].Delay;
        }

        /* 경과 시간에 도달한 spawn entry를 순서대로 생성하고 개수를 반환한다. */
        public int Tick(float deltaTime)
        {
            ValidateNonNegativeFinite(deltaTime, nameof(deltaTime));
            if (!HasPendingSpawns)
            {
                return 0;
            }

            nextDelay -= deltaTime;
            int created = 0;
            while (HasPendingSpawns && nextDelay <= 0f)
            {
                float carry = -nextDelay;
                SpawnEntry entry = pending[nextIndex++];
                Spawn(entry);
                created++;
                if (HasPendingSpawns)
                {
                    nextDelay = pending[nextIndex].Delay - carry;
                }
                else
                {
                    nextDelay = 0f;
                }
            }

            return created;
        }

        /* Monster Definition과 기본 스킬로 runtime Monster Model을 만든다. */
        public MonsterModel CreateMonsterModel(
            MonsterDefinition definition,
            bool autoSkillEnabled)
        {

            IEnumerable<SkillChoiceDefinition> passiveBases =
                catalog.Choices.Values
                    .Where(choice =>
                        string.Equals(
                            choice.monster_id,
                            definition.id,
                            StringComparison.Ordinal)
                        && string.Equals(
                            choice.choice_group,
                            "PassiveBase",
                            StringComparison.Ordinal));
            return new MonsterModel(
                definition,
                catalog.GetSkill(definition.id + "-a"),
                passiveBases,
                autoSkillEnabled);
        }

        /* 현현 Monster를 party roster 다음 slot에 추가한다. */
        public bool PlaceManifestedMonster(
            StageManager stage,
            MonsterModel monster)
        {

            return stage.TryRegisterFieldUnit(monster);
        }

        /* 단일 spawn entry를 Enemy Model과 record로 확정한다. */
        private void Spawn(SpawnEntry entry)
        {
            EnemyDefinition definition =
                catalog.GetEnemy(entry.Encounter.enemy_id);
            float multiplier = 1f;
            if (entry.IsBoss)
            {
                multiplier = ResolveBossHealthMultiplier(entry.Encounter);
            }

            EnemyModel model = new EnemyModel(
                definition,
                catalog.GetSkill(definition.skill_slot_a_id),
                catalog.GetSkill(definition.skill_slot_b_id),
                (PassiveDefinition)catalog.GetSkill(
                    definition.passive_id),
                multiplier);
            float x = RequiredFinite(
                entry.Encounter.spawn_x,
                entry.Encounter,
                "spawn_x");
            float yMinimum = RequiredFinite(
                entry.Encounter.spawn_y_min,
                entry.Encounter,
                "spawn_y_min");
            float yMaximum = RequiredFinite(
                entry.Encounter.spawn_y_max,
                entry.Encounter,
                "spawn_y_max");

            model.SetPosition(new CombatVector2(
                x,
                yMinimum
                    + ((yMaximum - yMinimum) * NextUnitValue())));
            stageManager.TryRegisterFieldUnit(model);

            spawned.Add(new SpawnedEnemyRecord(
                model,
                entry.Encounter,
                entry.IsBoss));
        }

        /* boss authored health multiplier와 stage day 배율을 결합한다. */
        private float ResolveBossHealthMultiplier(
            StageEncounterDefinition encounter)
        {
            float minimum = RequiredPositive(
                encounter.boss_health_multiplier_min,
                encounter,
                "boss_health_multiplier_min");
            float maximum = RequiredPositive(
                encounter.boss_health_multiplier_max,
                encounter,
                "boss_health_multiplier_max");

            return minimum + ((maximum - minimum) * NextUnitValue());
        }

        /* injected random index를 현재 collection 범위로 검증해 반환한다. */
        private int ResolveRandomIndex(int count)
        {
            int index = randomIndex(count);

            return index;
        }

        /* injected random 값을 0 이상 1 미만 범위로 검증해 반환한다. */
        private float NextUnitValue()
        {
            float value = randomValue();

            return value;
        }

        /* 필수 양수 정수 authored 값을 검증해 반환한다. */
        private static int RequiredPositive(
            int? value,
            StageEncounterDefinition row,
            string column)
        {

            return value.Value;
        }

        /* 필수 양수 실수 authored 값을 검증해 반환한다. */
        private static float RequiredPositive(
            float? value,
            StageEncounterDefinition row,
            string column)
        {
            float result = RequiredFinite(value, row, column);

            return result;
        }

        /* 필수 0 이상 실수 authored 값을 검증해 반환한다. */
        private static float RequiredNonNegative(
            float? value,
            StageEncounterDefinition row,
            string column)
        {
            float result = RequiredFinite(value, row, column);

            return result;
        }

        /* 필수 유한 실수 authored 값을 검증해 반환한다. */
        private static float RequiredFinite(
            float? value,
            StageEncounterDefinition row,
            string column)
        {

            return value.Value;
        }

        /* 잘못된 encounter 행과 field 이름을 포함한 경계 예외를 만든다. */
        private static InvalidOperationException Invalid(
            StageEncounterDefinition row,
            string message)
        {
            return new InvalidOperationException(
                $"{row.SourcePath} record {row.SourceRecordNumber}: {message}");
        }

        /* spawn 위치 값이 유한한 0 이상 수인지 검증한다. */
        private static void ValidateNonNegativeFinite(
            float value,
            string parameterName)
        {
        }

        /* scene bootstrap과 Inspector spawn/prefab 참조를 검증해 Actor 생성 경계를 연결한다. */
        public void BindScene(GameBootstrap sceneRuntime)
        {
            runtime = sceneRuntime;
            if (combatManager == null)
            {
                combatManager = GetComponent<GameBootstrap>();
            }

            BuildEnemyLookup();
        }

        /* party Monster의 prefab Actor를 slot 위치에 한 번 생성하고 runtime에 등록한다. */
        public MonsterActor EnsureMonster(MonsterModel model)
        {
            if (monsters.TryGetValue(model, out MonsterActor actor))
            {
                return actor;
            }

            GameObject prefab =
                ResolveMonsterPrefab(model.MonsterDefinition.id);
            int slot =
                runtime.Stage.Session.PartyRoster.Members.IndexOf(model);

            Transform point = ResolvePartySpawnPoint(slot);
            GameObject instance = Instantiate(
                prefab,
                point.position,
                prefab.transform.rotation,
                runtimeMonsterRoot);
            actor = instance.GetComponent<MonsterActor>();

            actor.Bind(model);
            monsters.Add(model, actor);
            runtime.RegisterMonster(model, actor, slot == 0);
            return actor;
        }

        /* 새 party Monster와 생존 Enemy spawn record를 해당 prefab Actor로 생성한다. */
        public void SyncNewSpawns()
        {
            IReadOnlyList<MonsterModel> party =
                runtime.Stage.Session.PartyRoster.Members;
            for (int index = 0; index < party.Count; index++)
            {
                EnsureMonster(party[index]);
            }

            IReadOnlyList<SpawnedEnemyRecord> records = SpawnedEnemies;
            for (int index = 0; index < records.Count; index++)
            {
                EnemyModel model = records[index].Model;
                if (!model.IsAlive
                    || model.HasContactedNexus
                    || enemies.ContainsKey(model))
                {
                    continue;
                }

                GameObject prefab = ResolveEnemyPrefab(
                    model.EnemyDefinition.enemy_id);
                CombatVector2 position = model.Position;
                GameObject instance = Instantiate(
                    prefab,
                    new Vector3(
                        position.X,
                        position.Y,
                        enemySpawnPoint.position.z),
                    prefab.transform.rotation,
                    runtimeEnemyRoot);
                EnemyActor actor = instance.GetComponent<EnemyActor>();

                actor.Bind(model);
                enemies.Add(model, actor);
                runtime.RegisterEnemy(model, actor);
            }
        }

        /* 모든 활성 Monster/Enemy Actor를 Model과 동기화하고 종료 Enemy mapping을 제거한다. */
        public void SyncActors()
        {
            foreach (MonsterActor actor in monsters.Values)
            {
                if (actor != null)
                {
                    actor.SyncFromModel();
                }
            }

            defeatedEnemies.Clear();
            foreach (KeyValuePair<EnemyModel, EnemyActor> pair in enemies)
            {
                EnemyActor actor = pair.Value;
                if (actor != null)
                {
                    actor.SyncFromModel();
                }

                if (!pair.Key.IsAlive || pair.Key.HasContactedNexus)
                {
                    defeatedEnemies.Add(pair.Key);
                }
            }

            for (int index = 0;
                index < defeatedEnemies.Count;
                index++)
            {
                enemies.Remove(defeatedEnemies[index]);
            }
        }

        /* Unit Model에 연결된 현재 scene Actor를 반환한다. */
        public bool TryGetActor(
            UnitBaseModel model,
            out UnitActor actor)
        {
            if (model is MonsterModel monster
                && monsters.TryGetValue(
                    monster,
                    out MonsterActor monsterActor))
            {
                actor = monsterActor;
                return true;
            }

            if (model is EnemyModel enemy
                && enemies.TryGetValue(
                    enemy,
                    out EnemyActor enemyActor))
            {
                actor = enemyActor;
                return true;
            }

            actor = null;
            return false;
        }

        /* Actor collider bounds를 전투 targeting용 엔진 중립 footprint로 변환한다. */
        public CombatFootprint ResolveCombatFootprint(
            UnitBaseModel model)
        {
            if (!TryGetActor(model, out UnitActor actor)
                || actor == null)
            {
                return default;
            }

            Collider2D collider =
                actor.GetComponentInChildren<Collider2D>(true);
            if (collider == null)
            {
                return default;
            }

            Bounds bounds = collider.bounds;
            Vector3 actorPosition = actor.transform.position;
            return new CombatFootprint(
                new CombatVector2(
                    bounds.center.x - actorPosition.x,
                    bounds.center.y - actorPosition.y),
                bounds.extents.x,
                bounds.extents.y);
        }

        /* Inspector Enemy id-prefab 배열을 중복 없는 runtime lookup으로 만든다. */
        private void BuildEnemyLookup()
        {
            enemyPrefabs.Clear();
            for (int index = 0;
                index < enemyPrefabBindings.Length;
                index++)
            {
                EnemyPrefabBinding binding = enemyPrefabBindings[index];

                enemyPrefabs.Add(binding.EnemyId, binding.Prefab);
            }
        }

        /* 고정 Monster id에 대응하는 Inspector prefab을 반환한다. */
        private GameObject ResolveMonsterPrefab(string monsterId)
        {
            switch (monsterId)
            {
                case "ariel": return Require(arielUnitPrefab, monsterId);
                case "eve": return Require(eveUnitPrefab, monsterId);
                case "rin": return Require(rinUnitPrefab, monsterId);
                case "sein": return Require(seinUnitPrefab, monsterId);
                case "vega": return Require(vegaUnitPrefab, monsterId);
                default:
                    break;
            }

            return null;
        }

        /* Enemy id runtime lookup에서 대응 prefab을 반환한다. */
        private GameObject ResolveEnemyPrefab(string enemyId)
        {
            enemyPrefabs.TryGetValue(enemyId, out GameObject prefab);
            return prefab;
        }

        /* party slot에 대응하는 기존 scene spawn point를 반환한다. */
        private Transform ResolvePartySpawnPoint(int slot)
        {
            if (slot == 0)
            {
                return playerSpawnPoint;
            }

            GameObject target =
                GameObject.Find($"{slot + 1}PSpawnPoint");

            return target.transform;
        }

        /* 필수 Inspector prefab이 연결되었는지 확인해 반환한다. */
        private static GameObject Require(
            GameObject prefab,
            string unitId)
        {

            return prefab;
        }

        private class SpawnEntry
        {
            /* authored spawn 시각·순서·위치를 runtime queue 항목으로 저장한다. */
            public SpawnEntry(
                StageEncounterDefinition encounter,
                bool isBoss,
                float delay)
            {
                Encounter = encounter;
                IsBoss = isBoss;
                Delay = delay;
            }

            public StageEncounterDefinition Encounter { get; }

            public bool IsBoss { get; }

            public float Delay { get; }
        }
    }

    internal static class PartyRosterIndex
    {
        /* party reference 순서에서 지정 Monster의 slot index를 찾는다. */
        public static int IndexOf(
            this IReadOnlyList<MonsterModel> members,
            MonsterModel model)
        {
            for (int index = 0; index < members.Count; index++)
            {
                if (ReferenceEquals(members[index], model))
                {
                    return index;
                }
            }

            return -1;
        }
    }
}
