using System;

namespace Pakuri.InGame
{
    public enum UnitSide
    {
        Player,
        Enemy
    }

    public enum UnitRole
    {
        Monster,
        Enemy,
        Summon
    }

    [Serializable]
    public sealed class UnitIdentity
    {
        public string UnitId;
        public string DefinitionId;
        public string DisplayName;
        public UnitSide Side;
        public UnitRole Role;
        public int SlotIndex;
    }

    [Serializable]
    public sealed class UnitStatsRuntime
    {
        public float MaxHealth;
        public float AttackPower;
        public float SpellPower;
        public float MoveSpeed;
        public float CriticalChance;
        public float CriticalDamage;
        public float CriticalResistance;
    }

    [Serializable]
    public sealed class UnitResourceRuntime
    {
        public float CurrentHealth;
        public float CurrentShield;
    }

    public class BaseUnitRuntimeModel
    {
        public UnitIdentity Identity = new UnitIdentity();
        public UnitStatsRuntime Stats = new UnitStatsRuntime();
        public UnitDefenseRuntime Defenses = new UnitDefenseRuntime();
        public UnitResourceRuntime Resources = new UnitResourceRuntime();
        public UnitSkillRuntimeSet SkillRuntime = new UnitSkillRuntimeSet();
        public UnitStatusRuntimeSet Statuses = new UnitStatusRuntimeSet();
        public bool AutoAttackEnabled = true;
        public bool AutoSkillEnabled = true;
    }

    public class UnitRuntimeModel : BaseUnitRuntimeModel
    {
    }

    public sealed class UnitStatusRuntimeSet
    {
        private readonly System.Collections.Generic.List<UnitStatusRuntime> statuses =
            new System.Collections.Generic.List<UnitStatusRuntime>();

        public System.Collections.Generic.IReadOnlyList<UnitStatusRuntime> ActiveStatuses => statuses;
        public int Count => statuses.Count;

        public UnitStatusRuntime Apply(
            string tag,
            int stacks,
            float durationSeconds,
            int maxStacks = 0,
            bool permanent = false,
            bool refreshDuration = true)
        {
            return StatusEffectUtility.TryParse(tag, out var kind)
                ? Apply(kind, stacks, durationSeconds, maxStacks, permanent, refreshDuration)
                : null;
        }

        public UnitStatusRuntime Apply(
            StatusEffectKind kind,
            int stacks,
            float durationSeconds,
            int maxStacks = 0,
            bool permanent = false,
            bool refreshDuration = true)
        {
            if (kind == StatusEffectKind.None)
            {
                return null;
            }

            var definition = StatusEffectUtility.GetDefinition(kind);
            var resolvedDuration = durationSeconds > 0f ? durationSeconds : definition.DefaultDurationSeconds;
            var resolvedMaxStacks = maxStacks > 0 ? maxStacks : definition.DefaultMaxStacks;
            var resolvedPermanent = permanent || definition.Permanent;
            var status = Find(kind);
            if (status == null)
            {
                status = new UnitStatusRuntime(kind);
                statuses.Add(status);
            }

            status.AddStacks(stacks, resolvedMaxStacks);
            status.SetPermanent(resolvedPermanent);
            if (resolvedPermanent || refreshDuration || status.DurationRemaining <= 0f)
            {
                status.SetDuration(resolvedDuration);
            }

            return status;
        }

        public bool Tick(float deltaTime)
        {
            if (deltaTime <= 0f)
            {
                return false;
            }

            var changed = false;
            for (var i = statuses.Count - 1; i >= 0; i--)
            {
                var status = statuses[i];
                if (status == null || status.Tick(deltaTime))
                {
                    statuses.RemoveAt(i);
                    changed = true;
                }
            }

            return changed;
        }

        public bool Has(string tag)
        {
            return StatusEffectUtility.TryParse(tag, out var kind) && Has(kind);
        }

        public bool Has(StatusEffectKind kind)
        {
            var status = Find(kind);
            return status != null && status.Stacks > 0;
        }

        public int GetStacks(string tag)
        {
            return StatusEffectUtility.TryParse(tag, out var kind) ? GetStacks(kind) : 0;
        }

        public int GetStacks(StatusEffectKind kind)
        {
            var status = Find(kind);
            return status != null ? status.Stacks : 0;
        }

        public bool Remove(string tag)
        {
            return StatusEffectUtility.TryParse(tag, out var kind) && Remove(kind);
        }

        public bool Remove(StatusEffectKind kind)
        {
            for (var i = 0; i < statuses.Count; i++)
            {
                if (statuses[i] != null && statuses[i].Kind == kind)
                {
                    statuses.RemoveAt(i);
                    return true;
                }
            }

            return false;
        }

        public void Clear()
        {
            statuses.Clear();
        }

        private UnitStatusRuntime Find(StatusEffectKind kind)
        {
            for (var i = 0; i < statuses.Count; i++)
            {
                var status = statuses[i];
                if (status != null && status.Kind == kind)
                {
                    return status;
                }
            }

            return null;
        }
    }

    public sealed class UnitStatusRuntime
    {
        public UnitStatusRuntime(StatusEffectKind kind)
        {
            Kind = kind;
        }

        public StatusEffectKind Kind { get; }
        public string Tag => StatusEffectUtility.ToId(Kind);
        public int Stacks { get; private set; }
        public float DurationRemaining { get; private set; }
        public bool Permanent { get; private set; }

        public bool IsTimed => !Permanent && DurationRemaining > 0f;

        public void AddStacks(int stacks, int maxStacks)
        {
            var nextStacks = Stacks + System.Math.Max(0, stacks);
            Stacks = maxStacks > 0 ? System.Math.Min(maxStacks, nextStacks) : nextStacks;
        }

        public void SetDuration(float durationSeconds)
        {
            DurationRemaining = System.Math.Max(0f, durationSeconds);
        }

        public void SetPermanent(bool permanent)
        {
            Permanent = permanent;
            if (Permanent)
            {
                DurationRemaining = 0f;
            }
        }

        public bool Tick(float deltaTime)
        {
            if (Permanent)
            {
                return false;
            }

            if (DurationRemaining <= 0f)
            {
                return false;
            }

            DurationRemaining = System.Math.Max(0f, DurationRemaining - deltaTime);
            return DurationRemaining <= 0f;
        }
    }
}
