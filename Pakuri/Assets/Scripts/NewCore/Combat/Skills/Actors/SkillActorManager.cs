using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Pakuri.NewCore.Combat.Effects;
using Pakuri.NewCore.Definitions.Skills;

/* 스킬 Actor의 공통·시간·예약 생명주기와 중앙 Tick 컬렉션을 소유한다. */
namespace Pakuri.NewCore.Combat.Skills.Actors
{
    public sealed class SkillActorManager
    {
        private readonly List<SkillActor> active = new List<SkillActor>();
        private readonly List<SkillActor> pendingAdd = new List<SkillActor>();
        private readonly List<SkillActor> pendingRemove = new List<SkillActor>();
        private readonly IReadOnlyList<SkillActor> readOnlyActive;
        private readonly EffectManager effectManager;

        /* 완료 Actor의 시각 제거를 위임할 EffectManager와 활성 목록을 구성한다. */
        public SkillActorManager(EffectManager effectManager)
        {
            this.effectManager =
                effectManager ?? throw new ArgumentNullException(nameof(effectManager));
            readOnlyActive = new ReadOnlyCollection<SkillActor>(active);
        }

        public IReadOnlyList<SkillActor> ActiveActors => readOnlyActive;

        public int PendingAddCount => pendingAdd.Count;

        /* 새 Actor를 중복 검사 후 다음 Tick 등록 대기 목록에 추가한다. */
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

        /* 독립 시각의 지속시간을 전용 Actor로 중앙 생명주기에 등록한다. */
        internal void RegisterEffectLifetime(
            SkillDefinition definition,
            float duration,
            EffectHandle effect)
        {
            Register(new EffectLifetimeActor(definition, duration, effect));
        }

        /* public 경과 시간을 검증하고 활성 Actor 실행·완료 제거·대기 등록을 처리한다. */
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

        /* 활성·대기 Actor의 시각 효과와 모든 생명주기 컬렉션을 정리한다. */
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

        private sealed class EffectLifetimeActor : TimedSkillActor
        {
            /* 전투 의미 없는 시각 핸들만 지정 시간 뒤 완료하도록 구성한다. */
            public EffectLifetimeActor(
                SkillDefinition definition,
                float duration,
                EffectHandle effect)
                : base(definition, duration, effect)
            {
            }
        }
    }

    public abstract class SkillActor
    {
        /* 스킬 정의와 선택 시각 핸들을 공통 Actor 생명주기에 저장한다. */
        protected SkillActor(SkillDefinition definition, EffectHandle effect)
        {
            Definition = definition ?? throw new ArgumentNullException(nameof(definition));
            Effect = effect;
        }

        public SkillDefinition Definition { get; }

        public EffectHandle Effect { get; }

        public float ElapsedSeconds { get; private set; }

        public bool IsComplete { get; protected set; }

        /* public 경과 시간을 검증하고 미완료 Actor의 전용 Tick을 호출한다. */
        public void Tick(float deltaTime)
        {
            if (deltaTime < 0f || float.IsNaN(deltaTime) || float.IsInfinity(deltaTime))
            {
                throw new ArgumentOutOfRangeException(nameof(deltaTime));
            }

            if (IsComplete)
            {
                return;
            }

            ElapsedSeconds += deltaTime;
            TickActor(deltaTime);
        }

        /* 파생 Actor가 자신의 진행·완료 규칙을 처리한다. */
        protected abstract void TickActor(float deltaTime);
    }

    public abstract class TimedSkillActor : SkillActor
    {
        private readonly float duration;

        /* 유효한 지속시간과 공통 스킬·시각 상태를 시간 Actor에 저장한다. */
        protected TimedSkillActor(
            SkillDefinition definition,
            float duration,
            EffectHandle effect)
            : base(definition, effect)
        {
            if (duration < 0f || float.IsNaN(duration) || float.IsInfinity(duration))
            {
                throw new ArgumentOutOfRangeException(nameof(duration));
            }

            this.duration = duration;
        }

        /* 누적 시간이 지정 지속시간에 도달하면 Actor를 완료한다. */
        protected override void TickActor(float deltaTime)
        {
            IsComplete = ElapsedSeconds >= duration;
        }
    }

    public sealed class ScheduledSkillActor : SkillActor
    {
        private readonly int executionCount;
        private readonly float intervalSeconds;
        private readonly float initialDelaySeconds;
        private readonly Action<int> execute;
        private int executed;

        /* 예약 실행 횟수·간격·초기 지연과 실행 콜백을 검증해 저장한다. */
        public ScheduledSkillActor(
            SkillDefinition definition,
            int executionCount,
            float intervalSeconds,
            Action<int> execute,
            EffectHandle effect,
            float initialDelaySeconds = 0f)
            : base(definition, effect)
        {
            if (executionCount < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(executionCount));
            }

            if (intervalSeconds < 0f
                || float.IsNaN(intervalSeconds)
                || float.IsInfinity(intervalSeconds))
            {
                throw new ArgumentOutOfRangeException(nameof(intervalSeconds));
            }

            if (initialDelaySeconds < 0f
                || float.IsNaN(initialDelaySeconds)
                || float.IsInfinity(initialDelaySeconds))
            {
                throw new ArgumentOutOfRangeException(nameof(initialDelaySeconds));
            }

            this.executionCount = executionCount;
            this.intervalSeconds = intervalSeconds;
            this.initialDelaySeconds = initialDelaySeconds;
            this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
        }

        /* 누적 시간이 도달한 예약 콜백을 순서대로 실행하고 전체 횟수 후 완료한다. */
        protected override void TickActor(float deltaTime)
        {
            while (executed < executionCount
                && ElapsedSeconds + 0.00001f
                    >= initialDelaySeconds + (intervalSeconds * executed))
            {
                execute(executed);
                executed++;
            }

            IsComplete = executed >= executionCount;
        }
    }
}
