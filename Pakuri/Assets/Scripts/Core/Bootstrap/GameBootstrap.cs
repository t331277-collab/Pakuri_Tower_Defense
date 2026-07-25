using System;
using System.Collections.Generic;
using Pakuri.NewCore.Catalog;
using Pakuri.NewCore.Combat;
using Pakuri.NewCore.Combat.Actions;
using Pakuri.NewCore.Combat.Effects;
using Pakuri.NewCore.Combat.Skills.Actors;
using Pakuri.NewCore.Combat.Skills.Execution;
using Pakuri.NewCore.Definitions.Choices;
using Pakuri.NewCore.Definitions.Skills;
using Pakuri.NewCore.Definitions.Stage;
using Pakuri.NewCore.Definitions.Units;
using Pakuri.NewCore.Parsing;
using Pakuri.NewCore.Run;
using Pakuri.NewCore.Run.Services;
using Pakuri.NewCore.Spawn;
using Pakuri.NewCore.UI.InGame;
using Pakuri.NewCore.Units.Actors;
using Pakuri.NewCore.Units.Models;
using UnityEngine;

/* CSV/resource initialization과 scene Manager 연결·중앙 combat Tick을 한 bootstrap에서 소유한다. */
namespace Pakuri.NewCore.Bootstrap
{
    [DefaultExecutionOrder(-1000)]
    public class GameBootstrap : MonoBehaviour
    {
        [Serializable]
        public struct SpriteEntry
        {
            public string AssetPath;
            public Sprite Asset;
        }

        [Serializable]
        public struct PrefabEntry
        {
            public string AssetPath;
            public GameObject Asset;
        }

        [Serializable]
        public struct AnimatorControllerEntry
        {
            public string AssetPath;
            public RuntimeAnimatorController Asset;
        }

        [SerializeField] private bool enemyCombatSimulationEnabled = true;
        [SerializeField] private bool skillExecutionEnabled = true;
        [SerializeField] private PlayerInputController playerCombatControl;
        [SerializeField] private EffectManager effectManager;
        [SerializeField] private string defaultMonsterId = "eve";

        [Header("CSV Sources")]
        public TextAsset CatalogMonsters;
        public TextAsset Monsters;
        public TextAsset MonsterRewardChoices;
        public TextAsset[] MonsterSkillsProjectileFiles = Array.Empty<TextAsset>();
        public TextAsset[] MonsterSkillsLineAttackFiles = Array.Empty<TextAsset>();
        public TextAsset[] MonsterSkillsAreaAttackFiles = Array.Empty<TextAsset>();
        public TextAsset[] MonsterSkillsSingleAttackFiles = Array.Empty<TextAsset>();
        public TextAsset[] MonsterSkillsBuffFiles = Array.Empty<TextAsset>();
        public TextAsset[] MonsterSkillsPassiveFiles = Array.Empty<TextAsset>();
        public TextAsset MonsterSkillNodeDefinitions;
        public TextAsset MonsterSkillNodeDefinitionParams;
        public TextAsset[] MonsterSkillGraphNodeFiles = Array.Empty<TextAsset>();
        public TextAsset[] MonsterSkillTriggerFiles = Array.Empty<TextAsset>();
        public TextAsset[] MonsterSkillChoicesProjectileFiles = Array.Empty<TextAsset>();
        public TextAsset[] MonsterSkillChoicesLineAttackFiles = Array.Empty<TextAsset>();
        public TextAsset[] MonsterSkillChoicesAreaAttackFiles = Array.Empty<TextAsset>();
        public TextAsset[] MonsterSkillChoicesSingleAttackFiles = Array.Empty<TextAsset>();
        public TextAsset[] MonsterSkillChoicesBuffFiles = Array.Empty<TextAsset>();
        public TextAsset[] MonsterSkillChoicesPassiveFiles = Array.Empty<TextAsset>();
        public TextAsset StatusEffects;
        public TextAsset Enemies;
        public TextAsset[] EnemySkillBaseFiles = Array.Empty<TextAsset>();
        public TextAsset[] EnemySkillTriggerFiles = Array.Empty<TextAsset>();
        public TextAsset StageDay;
        public TextAsset StageEncounter;
        public TextAsset StageReward;

        [Header("Unity Assets")]
        public SpriteEntry[] Sprites = Array.Empty<SpriteEntry>();
        public PrefabEntry[] Prefabs = Array.Empty<PrefabEntry>();
        public AnimatorControllerEntry[] AnimatorControllers =
            Array.Empty<AnimatorControllerEntry>();

        private static string preparedMonsterId;
        private StageManager stageView;
        private SpawnManager spawnView;
        private InGameUIManager ui;
        private InGameActionManager actions;
        private SkillTargeting targeting;
        private SkillActorManager skillActors;
        private EffectManager effects;
        private PlayerInputController input;
        private float completionTimer;
        private bool started;
        private bool combatResolutionPending;
        private bool pendingCombatVictory;
        private Dictionary<string, Sprite> sprites;
        private Dictionary<string, GameObject> prefabs;
        private Dictionary<string, RuntimeAnimatorController> controllers;

        public GameDefinitionCatalog Catalog { get; private set; }

        public GameBootstrap RuntimeCatalog => this;

        public TextAsset StageDayAsset => StageDay;

        public TextAsset StageEncounterAsset => StageEncounter;

        public TextAsset StageRewardAsset => StageReward;

        public StageManager Stage { get; private set; }
        public SpawnManager Spawns { get; private set; }
        public InGameCombatManager Combat { get; private set; }
        public NexusModel Nexus { get; private set; }
        public RewardService Rewards { get; private set; }
        public OfferingService Offerings { get; private set; }
        public ManifestationService Manifestations { get; private set; }
        public RewardResult CurrentReward { get; private set; }
        public MonsterModel SelectedMonster => input?.SelectedMonster;

        /* Inspector CSV/resource와 scene component를 검증해 전체 runtime graph를 초기화한다. */
        private void Awake()
        {
            ResolveSceneComponents();
            Catalog = CreateCatalog();
            stageView.ValidateConnections();
            StageDayDefinition firstDay = FindFirstDay(Catalog);
            Spawns = spawnView;
            Spawns.Initialize(
                Catalog,
                RandomIndex,
                () => UnityEngine.Random.value);
            string selectedMonsterId = ConsumeMonsterId();
            Catalog.Monsters.TryGetValue(
                selectedMonsterId,
                out MonsterDefinition selectedDefinition);

            MonsterModel initialMonster = Spawns.CreateMonsterModel(
                selectedDefinition,
                false);
            var session = new RunSessionModel(
                "stage" + firstDay.stage.Value,
                firstDay.day.Value,
                firstDay.encounter_id,
                new PartyRoster(initialMonster),
                new PrisonerInventory());
            Stage = stageView;
            Stage.Initialize(session, Catalog, 0, 0);
            Stage.ConfigureSpawnManager(Spawns);

            effects = effectManager;
            skillActors = new SkillActorManager(effects);
            targeting = new SkillTargeting(
                RandomIndex,
                spawnView.ResolveCombatFootprint);
            var execution = new SkillExecutionRuntime(
                Catalog,
                targeting,
                skillActors,
                effects,
                () => UnityEngine.Random.value);
            Combat = new InGameCombatManager(
                () => UnityEngine.Random.value,
                execution);
            Nexus = new NexusModel(stageView.NexusActor.MaxHealth);
            Nexus.SetPosition(ToModel(
                stageView.NexusActor.transform.position));
            stageView.NexusActor.Bind(Nexus);
            Stage.ConnectCombat(Combat, Nexus);

            input = playerCombatControl;
            actions = new InGameActionManager(
                Stage,
                () => Stage.IsCombatActive
                    && enemyCombatSimulationEnabled
                    && skillExecutionEnabled,
                SyncActorViews,
                input,
                skillActors,
                execution.Triggers,
                Combat);
            Rewards = new RewardService(
                Catalog,
                RandomIndex,
                () => UnityEngine.Random.value);
            Offerings = new OfferingService(
                Catalog,
                Stage,
                RandomIndex);
            Manifestations = new ManifestationService(
                Catalog,
                Stage,
                Spawns,
                RandomIndex,
                () => UnityEngine.Random.value);

            spawnView.BindScene(this);
            effectManager.BindVisualRuntime(
                ResolvePrefab,
                ResolveSprite,
                ResolveAnimator);
            Stage.CombatResolved += HandleCombatResolved;
            Combat.DamageApplied += HandleDamage;
            Combat.SkillActivated += HandleSkillActivated;
        }

        /* authored start flag에 따라 첫 party Actor와 stage combat flow를 시작한다. */
        private void Start()
        {
            if (!stageView.StartFlowOnStart)
            {
                return;
            }

            var selected = spawnView.EnsureMonster(
                Stage.Session.PartyRoster.Members[0]);
            playerCombatControl.BindActor(selected);
            Stage.StartCurrentDay();
            playerCombatControl.SynchronizeAutoSkillState();
            spawnView.SyncNewSpawns();
            BeginOrExtendCombat();
            SyncActorViews();
            started = true;
        }

        /* 중앙 frame 순서로 input, spawn, combat, completion, presentation을 실행한다. */
        private void Update()
        {
            if (!started || Stage.Session.Result != RunResult.Active)
            {
                SyncPresentation();
                return;
            }

            var deltaTime = Time.deltaTime;
            if (Stage.IsCombatActive)
            {
                playerCombatControl.Capture();
            }
            Stage.TickSpawnSequence(deltaTime);
            spawnView.SyncNewSpawns();
            BeginOrExtendCombat();
            actions.Tick(deltaTime);
            if (CompletePendingCombatResolution())
            {
                SyncPresentation();
                return;
            }

            completionTimer += deltaTime;
            if (completionTimer >= stageView.ClearCheckInterval)
            {
                completionTimer = 0f;
                Stage.EvaluateCombatCompletion();
            }

            CompletePendingCombatResolution();
            SyncPresentation();
        }

        /* scene 종료 전에 combat event와 Actor/effect runtime을 정리한다. */
        private void OnDestroy()
        {
            if (Stage != null)
            {
                Stage.CombatResolved -= HandleCombatResolved;
                Stage.DisconnectCombat();
            }

            if (Combat != null)
            {
                Combat.DamageApplied -= HandleDamage;
                Combat.SkillActivated -= HandleSkillActivated;
            }

            actions?.EndCombat();
            effectManager?.Clear();
        }

        /* 생성된 Monster Actor를 combat input과 presentation 추적에 등록한다. */
        public void RegisterMonster(
            MonsterModel model,
            bool selected)
        {
            var controller =
                new MonsterActionController(model, Combat);
            actions.RegisterMonster(controller, selected);
        }

        /* 생성된 Enemy Actor를 combat action과 presentation 추적에 등록한다. */
        public void RegisterEnemy(
            EnemyModel model,
            EnemyActor actor)
        {
            var controller = new EnemyActionController(
                model,
                Combat,
                targeting,
                new UnitMovementController(),
                Stage,
                Nexus,
                ResolveNexusContactDistance(actor));
            actions.RegisterEnemy(controller);
        }

        /* 지정 Monster에게 skill을 학습시키고 PassiveBase Choice를 함께 적용한다. */
        public bool TryLearnSkill(
            MonsterModel monster,
            SkillDefinition skill)
        {
            if (!(skill is PassiveDefinition passive))
            {
                return monster.SkillBucket.CanLearnActive(skill)
                    && monster.SkillBucket.TryLearnActive(skill);
            }

            if (!monster.SkillBucket.CanLearnPassive(passive)
                || !monster.SkillBucket.TryLearnPassive(passive))
            {
                return false;
            }

            foreach (SkillChoiceDefinition choice
                in Catalog.Choices.Values)
            {
                if (choice.monster_id
                    == monster.MonsterDefinition.id
                    && choice.skill_id == passive.skill_id
                    && choice.choice_group == "PassiveBase")
                {
                    monster.SkillBucket.TrySelectChoice(choice);
                    break;
                }
            }

            return true;
        }

        /* 지정 Monster에게 선택 가능한 skill Choice command를 적용한다. */
        public bool TrySelectSkillChoice(
            MonsterModel monster,
            SkillChoiceDefinition choice)
        {
            return monster.SkillBucket.CanSelectChoice(choice)
                && monster.SkillBucket.TrySelectChoice(choice);
        }

        /* 현재 보상 단계를 완료하고 다음 day 전투 준비를 요청한다. */
        public bool CompleteRewardAndAdvance()
        {
            if (Stage.Session.RewardState
                != RewardProcessingState.Processing)
            {
                return false;
            }

            CurrentReward = null;
            var advanced = Stage.CompleteRewardAndAdvance();
            if (advanced)
            {
                playerCombatControl.SynchronizeAutoSkillState();
                spawnView.SyncNewSpawns();
                BeginOrExtendCombat();
                CompletePendingCombatResolution();
            }
            else
            {
                ui?.ShowResult(true);
            }

            return advanced;
        }

        /* 현현된 Monster Model에 대응하는 scene Actor를 생성해 반환한다. */
        public MonsterActor PresentManifestedMonster(
            MonsterModel monster)
        {
            return spawnView.EnsureMonster(monster);
        }

        /* combat 판정 결과를 지연 presentation 처리 상태로 저장한다. */
        private void HandleCombatResolved(bool victory)
        {
            combatResolutionPending = true;
            pendingCombatVictory = victory;
        }

        /* 대기 중 전투 결과를 stage와 UI에 한 번 전달한다. */
        private bool CompletePendingCombatResolution()
        {
            if (!combatResolutionPending)
            {
                return false;
            }

            bool victory = pendingCombatVictory;
            combatResolutionPending = false;
            actions.EndCombat();
            if (!victory)
            {
                ui?.ShowResult(false);
                return true;
            }

            CurrentReward = Rewards.GenerateAndGrant(Stage);
            ui?.ShowReward(CurrentReward);
            return true;
        }

        /* 확정 피해 결과를 대상 Actor 피해 popup으로 전달한다. */
        private void HandleDamage(CombatResult result)
        {
            if (spawnView.TryGetActor(result.Target, out var actor))
            {
                actor.ShowDamage(result.DamageAmount);
                if (!result.IsDefeated
                    && actor is MonsterActor monster)
                {
                    monster.PlayHit();
                }
            }
        }

        /* 현재 stage field 상태에 맞춰 combat 실행 구간을 시작하거나 연장한다. */
        private void BeginOrExtendCombat()
        {
            if (Stage.IsCombatActive)
            {
                actions.BeginOrExtendCombat(Stage.FieldUnits);
            }
        }

        /* 활성화된 스킬 종류에 맞는 Monster 또는 Enemy animation을 요청한다. */
        private void HandleSkillActivated(
            UnitBaseModel source,
            Definitions.Skills.SkillDefinition skill)
        {
            if (spawnView.TryGetActor(source, out var actor)
                && actor is MonsterActor monster)
            {
                monster.PlayAttack();
            }
            else if (actor is EnemyActor enemy)
            {
                enemy.PlayAttack();
            }
        }

        /* Model, Actor, effect, UI 표현을 현재 runtime 상태와 동기화한다. */
        private void SyncPresentation()
        {
            SyncActorViews();
            stageView.NexusActor.SyncFromModel();
            effectManager.SyncVisuals();
        }

        /* SpawnManager가 소유한 모든 Unit Actor를 현재 Model과 동기화한다. */
        private void SyncActorViews()
        {
            spawnView?.SyncActors();
        }

        /* 필수 section 19 scene owner를 현재 GameObject에서 찾아 초기 연결을 검증한다. */
        private void ResolveSceneComponents()
        {
            stageView = GetComponent<StageManager>();
            spawnView = GetComponent<SpawnManager>();
            if (playerCombatControl == null)
            {
                playerCombatControl =
                    GetComponent<PlayerInputController>();
            }

            if (effectManager == null)
            {
                effectManager = GetComponent<EffectManager>();
            }

            ui = FindFirstObjectByType<InGameUIManager>(
                FindObjectsInactive.Include);
        }

        /* MainMenu가 다음 run의 시작 Monster id를 bootstrap lifecycle에 저장한다. */
        public static void PrepareRun(string monsterId)
        {
            preparedMonsterId = monsterId ?? string.Empty;
        }

        /* 다음 run Monster id를 한 번 소비하고 Inspector 기본값으로 초기화한다. */
        private string ConsumeMonsterId()
        {
            string value = preparedMonsterId;
            if (string.IsNullOrWhiteSpace(value))
            {
                value = defaultMonsterId;
            }

            preparedMonsterId = string.Empty;
            return value;
        }

        /* Inspector TextAsset 전체를 경로별 CSV text로 투영해 immutable catalog를 만든다. */
        private GameDefinitionCatalog CreateCatalog()
        {
            var sources = new Dictionary<string, string>(
                StringComparer.Ordinal);
            Add(sources, "Assets/CSVdata/authoring/catalog/catalog_monsters.csv", CatalogMonsters);
            Add(sources, "Assets/CSVdata/authoring/monster/monsters.csv", Monsters);
            Add(sources, "Assets/CSVdata/authoring/monster/monster_modifier_skill_choice.csv", MonsterRewardChoices);
            AddGroup(sources, "Assets/CSVdata/authoring/monster/skills/base/projectile/", MonsterSkillsProjectileFiles);
            AddGroup(sources, "Assets/CSVdata/authoring/monster/skills/base/line_attack/", MonsterSkillsLineAttackFiles);
            AddGroup(sources, "Assets/CSVdata/authoring/monster/skills/base/area_attack/", MonsterSkillsAreaAttackFiles);
            AddGroup(sources, "Assets/CSVdata/authoring/monster/skills/base/single_attack/", MonsterSkillsSingleAttackFiles);
            AddGroup(sources, "Assets/CSVdata/authoring/monster/skills/base/buff/", MonsterSkillsBuffFiles);
            AddGroup(sources, "Assets/CSVdata/authoring/monster/skills/base/passive/", MonsterSkillsPassiveFiles);
            Add(sources, "Assets/CSVdata/authoring/monster/skills/nodes/definitions/skill_node_definitions.csv", MonsterSkillNodeDefinitions);
            Add(sources, "Assets/CSVdata/authoring/monster/skills/nodes/definitions/skill_node_definition_params.csv", MonsterSkillNodeDefinitionParams);
            AddGraphGroups(sources, MonsterSkillGraphNodeFiles);
            AddTriggerGroups(sources, "monster", MonsterSkillTriggerFiles);
            AddChoiceGroups(sources, MonsterSkillChoicesProjectileFiles);
            AddChoiceGroups(sources, MonsterSkillChoicesLineAttackFiles);
            AddChoiceGroups(sources, MonsterSkillChoicesAreaAttackFiles);
            AddChoiceGroups(sources, MonsterSkillChoicesSingleAttackFiles);
            AddChoiceGroups(sources, MonsterSkillChoicesBuffFiles);
            AddChoiceGroups(sources, MonsterSkillChoicesPassiveFiles);
            Add(sources, "Assets/CSVdata/authoring/status/status_effects.csv", StatusEffects);
            Add(sources, "Assets/CSVdata/authoring/enemy/enemies.csv", Enemies);
            AddEnemyBaseGroups(sources, EnemySkillBaseFiles);
            AddTriggerGroups(sources, "enemy", EnemySkillTriggerFiles);
            Add(sources, "Assets/CSVdata/stage_flow/StageDay.csv", StageDay);
            Add(sources, "Assets/CSVdata/stage_flow/StageEncounter.csv", StageEncounter);
            Add(sources, "Assets/CSVdata/stage_flow/StageReward.csv", StageReward);
            return new CsvParser().Parse(sources);
        }

        /* 외부 CSV text 집합을 parser 경계에서 검증해 immutable catalog로 만든다. */
        public static GameDefinitionCatalog CreateCatalog(
            IReadOnlyDictionary<string, string> retainedCsvFiles)
        {

            return new CsvParser().Parse(retainedCsvFiles);
        }

        /* resource path에 대응하는 Sprite를 bootstrap lookup에서 반환한다. */
        public bool TryGetSprite(string assetPath, out Sprite sprite)
        {
            EnsureLookups();
            return sprites.TryGetValue(Normalize(assetPath), out sprite);
        }

        /* resource path에 대응하는 prefab을 bootstrap lookup에서 반환한다. */
        public bool TryGetPrefab(string assetPath, out GameObject prefab)
        {
            EnsureLookups();
            return prefabs.TryGetValue(Normalize(assetPath), out prefab);
        }

        /* resource path에 대응하는 AnimatorController를 bootstrap lookup에서 반환한다. */
        public bool TryGetAnimatorController(
            string assetPath,
            out RuntimeAnimatorController controller)
        {
            EnsureLookups();
            return controllers.TryGetValue(
                Normalize(assetPath),
                out controller);
        }

        /* EffectManager 요청용 prefab resolver로 미등록 경로를 null 반환한다. */
        private GameObject ResolvePrefab(string assetPath)
        {
            if (TryGetPrefab(assetPath, out GameObject prefab))
            {
                return prefab;
            }

            return null;
        }

        /* EffectManager 요청용 Sprite resolver로 미등록 경로를 null 반환한다. */
        private Sprite ResolveSprite(string assetPath)
        {
            if (TryGetSprite(assetPath, out Sprite sprite))
            {
                return sprite;
            }

            return null;
        }

        /* EffectManager 요청용 Animator resolver로 미등록 경로를 null 반환한다. */
        private RuntimeAnimatorController ResolveAnimator(string assetPath)
        {
            if (TryGetAnimatorController(
                    assetPath,
                    out RuntimeAnimatorController controller))
            {
                return controller;
            }

            return null;
        }

        /* 필수 단일 CSV TextAsset을 exact path와 text로 catalog source에 추가한다. */
        private static void Add(
            IDictionary<string, string> sources,
            string path,
            TextAsset asset)
        {

            sources.Add(path, asset.text);
        }

        /* 동일 folder의 필수 CSV TextAsset 배열을 파일명 경로로 추가한다. */
        private static void AddGroup(
            IDictionary<string, string> sources,
            string folder,
            IReadOnlyList<TextAsset> assets)
        {

            for (int index = 0; index < assets.Count; index++)
            {
                TextAsset asset = assets[index];

                Add(sources, folder + asset.name + ".csv", asset);
            }
        }

        /* graph CSV 파일명을 category folder 경로로 투영해 추가한다. */
        private static void AddGraphGroups(
            IDictionary<string, string> sources,
            IReadOnlyList<TextAsset> assets)
        {
            AddCategorized(
                sources,
                "Assets/CSVdata/authoring/monster/skills/choices/",
                assets,
                "skill_graph_nodes_");
        }

        /* Choice CSV 파일명을 category folder 경로로 투영해 추가한다. */
        private static void AddChoiceGroups(
            IDictionary<string, string> sources,
            IReadOnlyList<TextAsset> assets)
        {
            AddCategorized(
                sources,
                "Assets/CSVdata/authoring/monster/skills/choices/",
                assets,
                "skill_choices_");
        }

        /* owner Trigger CSV 파일명을 authored trigger folder 경로로 추가한다. */
        private static void AddTriggerGroups(
            IDictionary<string, string> sources,
            string owner,
            IReadOnlyList<TextAsset> assets)
        {
            AddCategorized(
                sources,
                $"Assets/CSVdata/authoring/{owner}/skills/triggers/",
                assets,
                string.Empty);
        }

        /* Enemy base CSV 파일명을 family folder 경로로 투영해 추가한다. */
        private static void AddEnemyBaseGroups(
            IDictionary<string, string> sources,
            IReadOnlyList<TextAsset> assets)
        {
            AddCategorized(
                sources,
                "Assets/CSVdata/authoring/enemy/skills/base/",
                assets,
                "skills_");
        }

        /* category가 포함된 CSV 배열을 exact authored root 아래에 추가한다. */
        private static void AddCategorized(
            IDictionary<string, string> sources,
            string root,
            IReadOnlyList<TextAsset> assets,
            string prefix)
        {

            for (int index = 0; index < assets.Count; index++)
            {
                TextAsset asset = assets[index];

                string category = ResolveCategory(asset.name, prefix);
                Add(
                    sources,
                    root + category + "/" + asset.name + ".csv",
                    asset);
            }
        }

        /* CSV 파일명 prefix와 trigger suffix를 제거해 authored category를 복원한다. */
        private static string ResolveCategory(
            string name,
            string prefix)
        {
            string value = name;
            if (!string.IsNullOrEmpty(prefix)
                && value.StartsWith(prefix, StringComparison.Ordinal))
            {
                value = value.Substring(prefix.Length);
            }

            const string triggerSuffix = "_skill_triger";
            if (value.EndsWith(triggerSuffix, StringComparison.Ordinal))
            {
                value = value.Substring(
                    0,
                    value.Length - triggerSuffix.Length);
            }

            return value;
        }

        /* Inspector resource 배열을 최초 요청 때 path lookup으로 만든다. */
        private void EnsureLookups()
        {
            if (sprites != null && prefabs != null && controllers != null)
            {
                return;
            }

            sprites = CreateLookup(
                Sprites,
                value => value.AssetPath,
                value => value.Asset);
            prefabs = CreateLookup(
                Prefabs,
                value => value.AssetPath,
                value => value.Asset);
            controllers = CreateLookup(
                AnimatorControllers,
                value => value.AssetPath,
                value => value.Asset);
        }

        /* resource entry 배열을 case-insensitive path-to-object lookup으로 변환한다. */
        private static Dictionary<string, TAsset> CreateLookup<TEntry, TAsset>(
            IReadOnlyList<TEntry> entries,
            Func<TEntry, string> path,
            Func<TEntry, TAsset> asset)
            where TAsset : UnityEngine.Object
        {
            var result = new Dictionary<string, TAsset>(
                StringComparer.OrdinalIgnoreCase);
            if (entries == null)
            {
                return result;
            }

            for (int index = 0; index < entries.Count; index++)
            {
                string key = Normalize(path(entries[index]));
                TAsset value = asset(entries[index]);
                if (!string.IsNullOrEmpty(key) && value != null)
                {
                    if (result.TryGetValue(
                            key,
                            out TAsset existing))
                    {

                        continue;
                    }

                    result.Add(key, value);
                }
            }

            return result;
        }

        /* resource path 공백과 slash 방향을 lookup key 형식으로 정규화한다. */
        private static string Normalize(string assetPath)
        {
            if (string.IsNullOrWhiteSpace(assetPath))
            {
                return string.Empty;
            }

            return assetPath.Trim().Replace('\\', '/');
        }

        /* catalog에서 stage/day 순서상 첫 authored day를 선택한다. */
        private static StageDayDefinition FindFirstDay(
            GameDefinitionCatalog catalog)
        {
            StageDayDefinition found = null;
            foreach (var day in catalog.StageDays.Values)
            {
                if (!day.stage.HasValue || !day.day.HasValue)
                {
                    continue;
                }

                if (found == null
                    || day.stage.Value < found.stage.Value
                    || (day.stage.Value == found.stage.Value
                        && day.day.Value < found.day.Value))
                {
                    found = day;
                }
            }

            return found;
        }

        /* Unity random 값을 유효한 collection index로 변환한다. */
        private static int RandomIndex(int count)
        {

            return UnityEngine.Random.Range(0, count);
        }

        /* Enemy와 Nexus collider 폭에서 접촉 판정 거리를 계산한다. */
        private static float ResolveNexusContactDistance(
            EnemyActor actor)
        {
            var collider = actor.GetComponentInChildren<Collider2D>(true);
            if (collider == null)
            {
                return 0f;
            }

            return Mathf.Max(
                collider.bounds.extents.x,
                collider.bounds.extents.y);
        }

        /* Unity 좌표를 엔진 중립 전투 좌표로 변환한다. */
        private static CombatVector2 ToModel(Vector3 value)
        {
            return new CombatVector2(value.x, value.y);
        }
    }
}
