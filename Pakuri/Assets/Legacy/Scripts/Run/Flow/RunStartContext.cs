using Pakuri.Data;
using UnityEngine;

namespace Pakuri.Run
{
    [DisallowMultipleComponent]
    public sealed class RunStartContext : MonoBehaviour
    {
        public static RunStartContext Instance { get; private set; }

        public MonsterDefinition SelectedMonster { get; private set; }
        public RunSession Session { get; private set; }
        public bool HasPendingRun => SelectedMonster != null && Session != null;

        public static RunStartContext Ensure()
        {
            if (Instance != null)
            {
                return Instance;
            }

            var contextObject = new GameObject("RunStartContext");
            Instance = contextObject.AddComponent<RunStartContext>();
            DontDestroyOnLoad(contextObject);
            return Instance;
        }

        public void PrepareNewRun(MonsterDefinition selectedMonster)
        {
            SelectedMonster = selectedMonster;
            Session = RunSession.Begin(selectedMonster);
        }

        public void Clear()
        {
            SelectedMonster = null;
            Session = null;
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
}
