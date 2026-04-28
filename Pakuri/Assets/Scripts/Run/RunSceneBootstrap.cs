using Pakuri.Combat;
using Pakuri.Data;
using UnityEngine;

namespace Pakuri.Run
{
    [DisallowMultipleComponent]
    public sealed class RunSceneBootstrap : MonoBehaviour
    {
        [SerializeField] private CombatRuntimeController combatController;
        [SerializeField] private GameDataCatalog fallbackCatalog;
        [SerializeField] private string fallbackMonsterId = "eve";
        [SerializeField] private bool allowFallbackRun = true;

        public static MonsterDefinition ActiveMonster { get; private set; }
        public static RunSession ActiveSession { get; private set; }
        public static string FallbackMonsterId { get; private set; } = "eve";

        private void Start()
        {
            FallbackMonsterId = fallbackMonsterId;

            if (combatController == null)
            {
                combatController = FindFirstObjectByType<CombatRuntimeController>();
            }

            if (combatController == null)
            {
                Debug.LogError("RunSceneBootstrap requires a CombatRuntimeController in RunScene.");
                return;
            }

            var context = RunStartContext.Instance;
            if (context != null && context.HasPendingRun)
            {
                BeginCombat(context.SelectedMonster, context.Session);
                return;
            }

            if (!allowFallbackRun)
            {
                Debug.LogWarning("RunScene started without RunStartContext. Combat was not started.");
                return;
            }

            var fallbackMonster = ResolveFallbackMonster();
            var fallbackSession = RunSession.Begin(fallbackMonster);
            BeginCombat(fallbackMonster, fallbackSession);
        }

        private void BeginCombat(MonsterDefinition monster, RunSession session)
        {
            ActiveMonster = monster;
            ActiveSession = session;
            combatController.BeginConfiguredDay(monster, session, fallbackCatalog);
        }

        private MonsterDefinition ResolveFallbackMonster()
        {
            if (fallbackCatalog == null || fallbackCatalog.Monsters == null || fallbackCatalog.Monsters.Length == 0)
            {
                return null;
            }

            var monster = fallbackCatalog.GetMonsterById(fallbackMonsterId);
            return monster != null ? monster : fallbackCatalog.Monsters[0];
        }
    }
}
