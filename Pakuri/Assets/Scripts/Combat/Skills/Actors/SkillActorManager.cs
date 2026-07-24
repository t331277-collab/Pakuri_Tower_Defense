using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Pakuri.NewCore.Combat.Effects;

namespace Pakuri.NewCore.Combat.Skills.Actors
{
    public sealed class SkillActorManager
    {
        private readonly List<SkillActor> active = new List<SkillActor>();
        private readonly List<SkillActor> pendingAdd = new List<SkillActor>();
        private readonly List<SkillActor> pendingRemove = new List<SkillActor>();
        private readonly IReadOnlyList<SkillActor> readOnlyActive;
        private readonly EffectManager effectManager;

        public SkillActorManager(EffectManager effectManager)
        {
            this.effectManager =
                effectManager ?? throw new ArgumentNullException(nameof(effectManager));
            readOnlyActive = new ReadOnlyCollection<SkillActor>(active);
        }

        public IReadOnlyList<SkillActor> ActiveActors => readOnlyActive;

        public int PendingAddCount => pendingAdd.Count;

        public void Register(SkillActor actor)
        {
            if (actor == null)
            {
                throw new ArgumentNullException(nameof(actor));
            }

            if (active.Contains(actor) || pendingAdd.Contains(actor))
            {
                throw new InvalidOperationException("The Skill Actor is already registered.");
            }

            pendingAdd.Add(actor);
        }

        public void Tick(float deltaTime)
        {
            if (deltaTime < 0f || float.IsNaN(deltaTime) || float.IsInfinity(deltaTime))
            {
                throw new ArgumentOutOfRangeException(nameof(deltaTime));
            }

            for (int index = 0; index < active.Count; index++)
            {
                SkillActor actor = active[index];
                actor.Tick(deltaTime);
                if (actor.IsComplete)
                {
                    pendingRemove.Add(actor);
                }
            }

            for (int index = 0; index < pendingRemove.Count; index++)
            {
                SkillActor actor = pendingRemove[index];
                active.Remove(actor);
                effectManager.Remove(actor.Effect);
            }

            pendingRemove.Clear();

            // 현재 프레임에 생성된 Actor는 목록 끝에 등록하고 다음 Tick부터 실행한다.
            active.AddRange(pendingAdd);
            pendingAdd.Clear();
        }

        public void Clear()
        {
            for (int index = 0; index < active.Count; index++)
            {
                effectManager.Remove(active[index].Effect);
            }

            for (int index = 0; index < pendingAdd.Count; index++)
            {
                effectManager.Remove(pendingAdd[index].Effect);
            }

            active.Clear();
            pendingAdd.Clear();
            pendingRemove.Clear();
        }
    }
}
