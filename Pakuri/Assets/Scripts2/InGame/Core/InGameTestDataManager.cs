using Pakuri.Data;
using UnityEngine;

using Pakuri.Run;

namespace Pakuri.InGame
{
    [DisallowMultipleComponent]
    public sealed class InGameTestDataManager : MonoBehaviour
    {
        [Header("Test Data Source")]
        [SerializeField] private string sampleMonsterId = UnitFactory.DefaultPhase2AMonsterId;
        [SerializeField] private string sampleEnemyId = "stage1-swordsman";
        [SerializeField] private InGameSkillSlot[] sampleActiveSlots =
        {
            InGameSkillSlot.A,
            InGameSkillSlot.B
        };

        [Header("Test Timing")]
        [Tooltip("Test-only. Resolves the existing CSV runtime catalog in Awake for isolated InGame tests. Production Run flow should keep the MainMenuScene/RunStartContext data handoff.")]
        [SerializeField] private bool loadSamplesOnAwake = true;
        [SerializeField] private bool logSampleResults = true;

        public InGameSkillCatalog SkillCatalog { get; private set; }
        public SkillData[] LoadedSampleActiveSkills { get; private set; }
        public MonsterUnitRuntimeModel LoadedSampleMonsterModel { get; private set; }
        public EnemyUnitRuntimeModel LoadedSampleEnemyModel { get; private set; }

        private void Awake()
        {
            if (loadSamplesOnAwake)
            {
                LoadSamplesForTest();
            }
        }

        public bool LoadSamplesForTest()
        {
            SkillCatalog = new InGameSkillCatalog(null);
            var unitFactory = new UnitFactory();
            var sourceCatalog = PakuriCsvRuntimeData.ResolveCatalogOrFallback(null);
            var resolvedMonsterId = string.IsNullOrWhiteSpace(sampleMonsterId)
                ? UnitFactory.DefaultPhase2AMonsterId
                : sampleMonsterId;
            var monster = PakuriDataManager.Instance.ResolveMonster(resolvedMonsterId, sourceCatalog);
            var session = monster != null ? RunSession.Begin(monster) : null;
            var monsterModel = monster != null
                ? unitFactory.CreateSelectedMonster(monster, session != null ? session.GetPartyMemberState(monster.MonsterId) : null, 0)
                : null;
            var enemyModel = unitFactory.CreateEnemy(ResolveSampleEnemy(sourceCatalog), 0);
            var loadedModels = monsterModel != null && enemyModel != null;
            LoadedSampleMonsterModel = monsterModel;
            LoadedSampleEnemyModel = enemyModel;
            if (LoadedSampleMonsterModel != null)
            {
                SkillRuntimeFactory.RebuildLearnedActiveSet(LoadedSampleMonsterModel, SkillCatalog);
            }

            LoadedSampleActiveSkills = new SkillData[sampleActiveSlots != null ? sampleActiveSlots.Length : 0];
            var loadedCount = 0;
            for (var i = 0; i < LoadedSampleActiveSkills.Length; i++)
            {
                if (SkillCatalog.TryGetActiveSkill(resolvedMonsterId, sampleActiveSlots[i], out var skill))
                {
                    LoadedSampleActiveSkills[i] = skill;
                    loadedCount++;
                }
            }

            if (logSampleResults)
            {
                Debug.Log(
                    $"InGame test data bootstrap loaded {loadedCount}/{LoadedSampleActiveSkills.Length} sample active skills for monster '{sampleMonsterId}'. Phase2-A unit models loaded: {loadedModels}. Monster model: {FormatModelForLog(LoadedSampleMonsterModel)}. Enemy model: {FormatModelForLog(LoadedSampleEnemyModel)}. Source catalog resolved: {SkillCatalog.HasSourceCatalog}.");
            }

            return loadedCount > 0 && loadedModels;
        }

        private EnemyDefinition ResolveSampleEnemy(GameDataCatalog sourceCatalog)
        {
            if (!string.IsNullOrWhiteSpace(sampleEnemyId))
            {
                var registered = PakuriDataManager.Instance.GetData<EnemyDefinition>(sampleEnemyId);
                if (registered != null)
                {
                    return registered;
                }

                var fromCatalog = sourceCatalog != null ? sourceCatalog.GetStageOneEnemyById(sampleEnemyId) : null;
                if (fromCatalog != null)
                {
                    return fromCatalog;
                }
            }

            var enemies = sourceCatalog != null ? sourceCatalog.StageOneEnemies : null;
            if (enemies == null)
            {
                return null;
            }

            for (var i = 0; i < enemies.Length; i++)
            {
                if (enemies[i] != null)
                {
                    return enemies[i];
                }
            }

            return null;
        }

        private static string FormatModelForLog(BaseUnitRuntimeModel model)
        {
            if (model == null || model.Identity == null || model.Stats == null || model.Resources == null)
            {
                return "null";
            }

            return $"{model.Identity.UnitId}/{model.Identity.DisplayName}/hp:{model.Resources.CurrentHealth}/{model.Stats.MaxHealth}/auto:{model.AutoAttackEnabled}";
        }
    }
}
