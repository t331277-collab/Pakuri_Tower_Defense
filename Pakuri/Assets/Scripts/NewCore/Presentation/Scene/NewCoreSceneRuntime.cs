using System;
using Pakuri.NewCore.Bootstrap;
using Pakuri.NewCore.Catalog;
using Pakuri.NewCore.Combat;
using Pakuri.NewCore.Combat.Actions;
using Pakuri.NewCore.Combat.Effects;
using Pakuri.NewCore.Combat.Skills.Actors;
using Pakuri.NewCore.Combat.Skills.Execution;
using Pakuri.NewCore.Definitions.Stage;
using Pakuri.NewCore.Presentation.Actors;
using Pakuri.NewCore.Presentation.Assets;
using Pakuri.NewCore.Presentation.UI;
using Pakuri.NewCore.Run;
using Pakuri.NewCore.Run.Services;
using Pakuri.NewCore.Spawn;
using Pakuri.NewCore.Units.Models;
using UnityEngine;

namespace Pakuri.NewCore.Presentation.Scene
{
    [DefaultExecutionOrder(-1000)]
    public sealed class NewCoreSceneRuntime : MonoBehaviour
    {
        private const string CatalogResourcePath =
            "Pakuri/NewCore/RuntimeCatalog";
        private const string SelectionResourcePath =
            "Pakuri/NewCore/RunStartSelection";

        [SerializeField] private bool enemyCombatSimulationEnabled = true;
        [SerializeField] private bool skillExecutionEnabled = true;
        [SerializeField] private NewCoreInputController playerCombatControl;
        [SerializeField] private NewCoreEffectView effectManager;

        private NewCoreStageController stageView;
        private NewCoreSpawnController spawnView;
        private NewCoreInGameUIController ui;
        private InGameActionManager actions;
        private SkillTargeting targeting;
        private SkillActorManager skillActors;
        private EffectManager effects;
        private PlayerInputController input;
        private float completionTimer;
        private bool started;
        private bool combatResolutionPending;
        private bool pendingCombatVictory;

        public GameDefinitionCatalog Catalog { get; private set; }
        public NewCoreRuntimeCatalogAsset RuntimeCatalog { get; private set; }
        public StageManager Stage { get; private set; }
        public SpawnManager Spawns { get; private set; }
        public InGameCombatManager Combat { get; private set; }
        public NexusModel Nexus { get; private set; }
        public RewardService Rewards { get; private set; }
        public OfferingService Offerings { get; private set; }
        public ManifestationService Manifestations { get; private set; }
        public RewardResult CurrentReward { get; private set; }
        public MonsterModel SelectedMonster => input?.SelectedMonster;

        private void Awake()
        {
            var catalogAsset =
                Resources.Load<NewCoreRuntimeCatalogAsset>(
                    CatalogResourcePath);
            if (catalogAsset == null)
            {
                throw new InvalidOperationException(
                    $"New Core runtime catalog '{CatalogResourcePath}' is missing.");
            }

            var selection =
                Resources.Load<RunStartSelectionAsset>(
                    SelectionResourcePath);
            if (selection == null)
            {
                throw new InvalidOperationException(
                    $"Run selection asset '{SelectionResourcePath}' is missing.");
            }

            ResolveSceneComponents();
            GameBootstrap bootstrap = catalogAsset.CreateBootstrap();
            stageView.ValidateConnections(catalogAsset, bootstrap.Catalog);
            RuntimeCatalog = catalogAsset;
            Catalog = bootstrap.Catalog;
            var firstDay = FindFirstDay(Catalog);
            Spawns = new SpawnManager(
                Catalog,
                RandomIndex,
                () => UnityEngine.Random.value);
            var selectedMonsterId = selection.ConsumeMonsterId();
            if (!Catalog.Monsters.TryGetValue(
                    selectedMonsterId,
                    out var selectedDefinition))
            {
                throw new InvalidOperationException(
                    $"Selected monster '{selectedMonsterId}' does not exist.");
            }

            var initialMonster = Spawns.CreateMonsterModel(
                selectedDefinition,
                false);
            var session = new RunSessionModel(
                "stage" + firstDay.stage.Value,
                firstDay.day.Value,
                firstDay.encounter_id,
                new PartyRoster(initialMonster),
                new PrisonerInventory());
            Stage = new StageManager(session, Catalog, 0, 0);
            Stage.ConfigureSpawnManager(Spawns);

            effects = new EffectManager();
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

            input = new PlayerInputController();
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

            spawnView.Bind(this, Spawns);
            effectManager.Bind(catalogAsset, effects);
            Stage.CombatResolved += HandleCombatResolved;
            Combat.DamageApplied += HandleDamage;
            Combat.SkillActivated += HandleSkillActivated;
        }

        private void Start()
        {
            if (!stageView.StartFlowOnStart)
            {
                return;
            }

            var selected = spawnView.EnsureMonster(
                Stage.Session.PartyRoster.Members[0]);
            playerCombatControl.Bind(input, selected);
            Stage.StartCurrentDay();
            playerCombatControl.SynchronizeAutoSkillState();
            spawnView.SyncNewSpawns();
            BeginOrExtendCombat();
            SyncActorViews();
            started = true;
        }

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

        public void RegisterMonster(
            MonsterModel model,
            MonsterActorBehaviour actor,
            bool selected)
        {
            var controller =
                new MonsterActionController(model, Combat);
            actions.RegisterMonster(controller, selected);
        }

        public void RegisterEnemy(
            EnemyModel model,
            EnemyActorBehaviour actor)
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
                stageView.ShowResult(true);
            }

            return advanced;
        }

        public MonsterActorBehaviour PresentManifestedMonster(
            MonsterModel monster)
        {
            return spawnView.EnsureMonster(monster);
        }

        private void HandleCombatResolved(bool victory)
        {
            combatResolutionPending = true;
            pendingCombatVictory = victory;
        }

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
                stageView.ShowResult(false);
                return true;
            }

            CurrentReward = Rewards.GenerateAndGrant(Stage);
            ui?.ShowReward(CurrentReward);
            return true;
        }

        private void HandleDamage(CombatResult result)
        {
            if (spawnView.TryGetActor(result.Target, out var actor))
            {
                actor.ShowDamage(result.DamageAmount);
                if (!result.IsDefeated
                    && actor is MonsterActorBehaviour monster)
                {
                    monster.PlayHit();
                }
            }
        }

        private void BeginOrExtendCombat()
        {
            if (Stage.IsCombatActive)
            {
                actions.BeginOrExtendCombat(Stage.FieldUnits);
            }
        }

        private void HandleSkillActivated(
            UnitBaseModel source,
            Definitions.Skills.SkillDefinition skill)
        {
            if (spawnView.TryGetActor(source, out var actor)
                && actor is MonsterActorBehaviour monster)
            {
                monster.PlayAttack();
            }
            else if (actor is EnemyActorBehaviour enemy)
            {
                enemy.PlayAttack();
            }
        }

        private void SyncPresentation()
        {
            SyncActorViews();
            stageView.NexusActor.SyncFromModel();
            effectManager.Sync();
        }

        private void SyncActorViews()
        {
            spawnView?.SyncActors();
        }

        private void ResolveSceneComponents()
        {
            stageView = GetComponent<NewCoreStageController>();
            spawnView = GetComponent<NewCoreSpawnController>();
            if (playerCombatControl == null)
            {
                playerCombatControl =
                    GetComponent<NewCoreInputController>();
            }

            if (effectManager == null)
            {
                effectManager = GetComponent<NewCoreEffectView>();
            }

            ui = FindFirstObjectByType<NewCoreInGameUIController>(
                FindObjectsInactive.Include);
            if (stageView == null
                || spawnView == null
                || playerCombatControl == null
                || effectManager == null)
            {
                throw new InvalidOperationException(
                    "New Core GameManager components are incomplete.");
            }
        }

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

            return found
                ?? throw new InvalidOperationException(
                    "Runtime catalog has no StageDay.");
        }

        private static int RandomIndex(int count)
        {
            if (count <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            return UnityEngine.Random.Range(0, count);
        }

        private static float ResolveNexusContactDistance(
            EnemyActorBehaviour actor)
        {
            var collider = actor.GetComponentInChildren<Collider2D>(true);
            return collider != null
                ? Mathf.Max(
                    collider.bounds.extents.x,
                    collider.bounds.extents.y)
                : 0f;
        }

        private static CombatVector2 ToModel(Vector3 value)
        {
            return new CombatVector2(value.x, value.y);
        }
    }
}
