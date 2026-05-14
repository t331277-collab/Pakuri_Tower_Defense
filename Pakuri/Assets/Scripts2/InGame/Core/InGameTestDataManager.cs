using Pakuri.Data;
using UnityEngine;

namespace Pakuri.InGame
{
    [DisallowMultipleComponent]
    public sealed class InGameTestDataManager : MonoBehaviour
    {
        [Header("Test Data Source")]
        [SerializeField] private GameDataCatalog fallbackCatalog;
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
            SkillCatalog = new InGameSkillCatalog(fallbackCatalog);
            var unitFactory = new UnitFactory();
            var loadedModels = unitFactory.TryCreatePhase2ATestModels(
                fallbackCatalog,
                out var monsterModel,
                out var enemyModel,
                sampleMonsterId,
                sampleEnemyId);
            LoadedSampleMonsterModel = monsterModel;
            LoadedSampleEnemyModel = enemyModel;

            LoadedSampleActiveSkills = new SkillData[sampleActiveSlots != null ? sampleActiveSlots.Length : 0];
            var loadedCount = 0;
            for (var i = 0; i < LoadedSampleActiveSkills.Length; i++)
            {
                if (SkillCatalog.TryGetActiveSkill(sampleMonsterId, sampleActiveSlots[i], out var skill))
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
