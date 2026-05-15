using System.Collections.Generic;

namespace Pakuri.InGame
{
    public sealed class SkillExecutorRegistry
    {
        private readonly List<IInGameSkillExecutor> executors = new List<IInGameSkillExecutor>();

        public SkillExecutorRegistry()
        {
            RegisterDefaults();
        }

        public int Count => executors.Count;

        public void Register(IInGameSkillExecutor executor)
        {
            if (executor != null && !executors.Contains(executor))
            {
                executors.Add(executor);
            }
        }

        public bool TryResolve(SkillData skillData, out IInGameSkillExecutor executor)
        {
            executor = null;
            if (skillData == null)
            {
                return false;
            }

            for (var i = 0; i < executors.Count; i++)
            {
                if (executors[i] != null && executors[i].CanExecute(skillData))
                {
                    executor = executors[i];
                    return true;
                }
            }

            return false;
        }

        private void RegisterDefaults()
        {
            Register(new ProjectileSkillExecutor());
            Register(new BeamSkillExecutor());
            Register(new ZoneSkillExecutor());
            Register(new BuffSkillExecutor());
            Register(new ShieldSkillExecutor());
            Register(new PassiveSkillExecutor());
        }
    }
}
