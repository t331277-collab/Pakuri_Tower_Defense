using System;
using System.Collections.Generic;
using Pakuri.Data;
using UnityEngine;

/*
 * 컴파일된 스킬 하나가 전투 중 가지는 변경 가능한 실행 상태를 관리한다.
 * 재사용 대기시간, 탄창·재장전, Tick, 연속 발사, 적중 횟수를 갱신하고
 * 현재 Choice Snapshot에 따른 시전 가능 여부와 시간 보정값을 적용한다.
 */
namespace Pakuri.InGame
{
    public class SkillRuntimeInstance
    {
        /*
         * 스킬 런타임 인스턴스에 필요한 값을 초기화한다.
         */
        public SkillRuntimeInstance(UnitCombatState owner, SkillRuntimeData data)
        {
            Owner = owner;
            Data = data;
            BasePlan = SkillNodeCompiler.Compile(data, null, data.NormalizedPlanNodes);
            ResetRuntimeState();
        }

        public UnitCombatState Owner { get; }
        public SkillRuntimeData Data { get; }
        public SkillNodePlan BasePlan { get; }
        public string SkillId => Data.SkillId;
        public SkillSlot Slot => Data.Slot;
        public float CooldownRemaining { get; private set; }
        public float CastRemaining { get; private set; }
        public float ActiveDurationRemaining { get; private set; }
        public float TickRemaining { get; private set; }
        public float ReloadRemaining { get; private set; }
        public int MagazineRemaining { get; private set; }
        public int ProjectileLaunchCount { get; private set; }
        public int SkillHitCount { get; private set; }

        private int effectiveMaxMagazineSize;
        private int effectiveBurstProjectileCount;
        private float effectiveReloadDuration;
        private float effectiveTickInterval;
        private float effectiveBurstInterval;
        private float effectiveCooldownDuration;
        private int queuedBurstShotsRemaining;
        private string consecutiveHitTargetUnitId;
        private int consecutiveHitRepeatCount;

        public bool IsCasting => CastRemaining > 0f;
        public bool IsActive => ActiveDurationRemaining > 0f;
        public bool IsReloading => ReloadRemaining > 0f;
        public bool IsBursting => queuedBurstShotsRemaining > 0;
        public int MaxMagazineSize => effectiveMaxMagazineSize;
        public float ReloadDuration => effectiveReloadDuration;
        public float EffectiveCooldownDuration => effectiveCooldownDuration;
        public int EffectiveBurstProjectileCount => effectiveBurstProjectileCount;
        public bool UsesMagazine => MaxMagazineSize > 0;
        public bool HasMagazine => !UsesMagazine || MagazineRemaining > 0;
        public bool CanCast => CanCastWithSnapshot(null);

        /*
         * 런타임 상태값을 초기화한다.
         */
        public void ResetRuntimeState()
        {
            effectiveMaxMagazineSize = ResolveMaxMagazineSize(Data);
            effectiveBurstProjectileCount = ResolveBurstProjectileCount(Data);
            effectiveReloadDuration = ResolveReloadDuration(Data);
            effectiveTickInterval = ResolveTickInterval(Data);
            effectiveBurstInterval = ResolveBurstInterval(Data);
            effectiveCooldownDuration = ResolveCooldownDuration(Data);
            CooldownRemaining = 0f;
            CastRemaining = 0f;
            ActiveDurationRemaining = 0f;
            TickRemaining = 0f;
            ReloadRemaining = 0f;
            MagazineRemaining = MaxMagazineSize;
            queuedBurstShotsRemaining = 0;
            ProjectileLaunchCount = 0;
            SkillHitCount = 0;
            consecutiveHitTargetUnitId = string.Empty;
            consecutiveHitRepeatCount = 0;
        }

        /*
         * 투사체 발사 횟수를 증가시키고 현재 횟수를 반환한다.
         */
        public int AdvanceProjectileLaunchCount()
        {
            if (ProjectileLaunchCount == int.MaxValue)
            {
                ProjectileLaunchCount = 0;
            }

            ProjectileLaunchCount++;
            return ProjectileLaunchCount;
        }

        /*
         * 스킬 적중 횟수를 증가시키고 현재 횟수를 반환한다.
         */
        public int AdvanceSkillHitCount()
        {
            if (SkillHitCount == int.MaxValue)
            {
                SkillHitCount = 0;
            }

            SkillHitCount++;
            return SkillHitCount;
        }

        /*
         * 같은 대상을 연속으로 적중했을 때 적용할 피해 배율을 결정한다.
         */
        public float ResolveConsecutiveHitDamageMultiplier(UnitCombatState target, SkillSnapshot snapshot)
        {
            if (target == null)
            {
                return 1f;
            }

            var projectileData = Data as ProjectileSkillRuntimeData;
            var bonusRate = 0f;
            var bonusMax = 0f;
            if (projectileData != null)
            {
                bonusRate = projectileData.ConsecutiveHitBonusRate;
                bonusMax = projectileData.ConsecutiveHitMax;
            }
            if (snapshot != null && snapshot.ConsecutiveHitBonusRate > 0f)
            {
                bonusRate = snapshot.ConsecutiveHitBonusRate;
            }
            if (snapshot != null && snapshot.ConsecutiveHitMax > 0f)
            {
                bonusMax = snapshot.ConsecutiveHitMax;
            }
            if (bonusRate <= 0f || bonusMax <= 0f)
            {
                return 1f;
            }

            var unitId = string.Empty;
            if (target.Identity != null)
            {
                unitId = target.Identity.UnitId;
            }
            if (string.IsNullOrWhiteSpace(unitId))
            {
                consecutiveHitTargetUnitId = string.Empty;
                consecutiveHitRepeatCount = 0;
                return 1f;
            }

            if (string.Equals(consecutiveHitTargetUnitId, unitId, StringComparison.Ordinal))
            {
                consecutiveHitRepeatCount = Math.Min(consecutiveHitRepeatCount + 1, int.MaxValue - 1);
            }
            else
            {
                consecutiveHitTargetUnitId = unitId;
                consecutiveHitRepeatCount = 0;
            }

            var bonus = Mathf.Min(
                Mathf.Max(0f, bonusMax),
                Mathf.Max(0f, bonusRate) * consecutiveHitRepeatCount);
            return 1f + bonus;
        }

        /*
         * 스킬의 시전, 지속시간, 재사용 대기시간을 갱신한다.
         */
        public void Tick(float deltaTime)
        {
            if (deltaTime <= 0f)
            {
                return;
            }

            var actionDeltaTime = deltaTime * StatusCombatRules.ResolveActionSpeedMultiplier(Owner);
            CooldownRemaining = TickDown(CooldownRemaining, actionDeltaTime);
            CastRemaining = TickDown(CastRemaining, actionDeltaTime);
            ActiveDurationRemaining = TickDown(ActiveDurationRemaining, deltaTime);
            TickRemaining = TickDown(TickRemaining, actionDeltaTime);
            ReloadRemaining = TickDown(ReloadRemaining, deltaTime);

            if (UsesMagazine
                && MagazineRemaining <= 0
                && ReloadRemaining <= 0f
                && CooldownRemaining <= 0f
                && !IsBursting)
            {
                MagazineRemaining = MaxMagazineSize;
            }
        }

        /*
         * 시전 포함 실행 정보를 가능한 상태인지 확인한다.
         */
        public bool CanCastWithSnapshot(SkillSnapshot snapshot)
        {
            RefreshRuntimeModifiers(snapshot);
            if (Data == null
                || !Data.IsActive
                || IsCasting
                || !IsCastIntervalReady())
            {
                return false;
            }

            if (IsBursting)
            {
                return !IsReloading;
            }

            return CooldownRemaining <= 0f
                && !IsReloading
                && HasMagazine;
        }

        /*
         * 시전을 시작하고 성공 여부를 반환한다.
         */
        public bool TryBeginCast()
        {
            return TryBeginCast(null);
        }

        /*
         * 시전을 시작하고 성공 여부를 반환한다.
         */
        public bool TryBeginCast(SkillSnapshot snapshot)
        {
            RefreshRuntimeModifiers(snapshot);
            if (IsBursting)
            {
                queuedBurstShotsRemaining = Math.Max(0, queuedBurstShotsRemaining - 1);
                if (IsBursting)
                {
                    TickRemaining = effectiveBurstInterval;
                }
                else
                {
                    TickRemaining = effectiveTickInterval;
                    BeginRecoveryIfNeeded();
                }

                return true;
            }

            if (!CanCastWithSnapshot(snapshot))
            {
                return false;
            }

            if (UsesMagazine)
            {
                MagazineRemaining = Math.Max(0, MagazineRemaining - 1);
            }

            var timing = Data.Timing;
            CastRemaining = Mathf.Max(0f, timing.CastTime);
            ActiveDurationRemaining = Mathf.Max(0f, timing.ActiveDuration);
            queuedBurstShotsRemaining = Math.Max(0, effectiveBurstProjectileCount - 1);
            TickRemaining = effectiveTickInterval;
            if (IsBursting)
            {
                TickRemaining = effectiveBurstInterval;
            }

            if (!IsBursting)
            {
                BeginRecoveryIfNeeded();
            }

            return true;
        }

        /*
         * 다음 주기 효과를 실행할 시간이 되었는지 확인한다.
         */
        public bool IsTickReady()
        {
            return Data.Timing.TickInterval > 0f && TickRemaining <= 0f;
        }

        /*
         * 주기 간격을 초기화한다.
         */
        public void ResetTickInterval()
        {
            TickRemaining = effectiveTickInterval;
        }

        /*
         * 현재 연속 발사에서 몇 번째 투사체인지 계산한다.
         */
        public int ResolveCurrentBurstProjectileIndex()
        {
            if (effectiveBurstProjectileCount <= 1 || !IsBursting)
            {
                return 1;
            }

            return Mathf.Clamp(
                effectiveBurstProjectileCount - queuedBurstShotsRemaining + 1,
                1,
                effectiveBurstProjectileCount);
        }

        /*
         * 남은 재장전 시간을 감소시킨다.
         */
        public bool ReduceReloadRemaining(float seconds)
        {
            if (seconds <= 0f || ReloadRemaining <= 0f)
            {
                return false;
            }

            ReloadRemaining = Mathf.Max(0f, ReloadRemaining - seconds);
            if (ReloadRemaining <= 0f && UsesMagazine && MagazineRemaining <= 0 && CooldownRemaining <= 0f && !IsBursting)
            {
                MagazineRemaining = MaxMagazineSize;
            }

            return true;
        }

        /*
         * 남은 재사용 대기시간을 감소시킨다.
         */
        public bool ReduceCooldownRemaining(float seconds)
        {
            if (seconds <= 0f || CooldownRemaining <= 0f)
            {
                return false;
            }

            CooldownRemaining = Mathf.Max(0f, CooldownRemaining - seconds);
            if (CooldownRemaining <= 0f && UsesMagazine && MagazineRemaining <= 0 && ReloadRemaining <= 0f && !IsBursting)
            {
                MagazineRemaining = MaxMagazineSize;
            }

            return true;
        }

        /*
         * 재사용 대기시간을 초기화한다.
         */
        public void ResetCooldown()
        {
            CooldownRemaining = 0f;
            if (UsesMagazine && MagazineRemaining <= 0 && ReloadRemaining <= 0f && !IsBursting)
            {
                MagazineRemaining = MaxMagazineSize;
            }
        }

        /*
         * 남은 시간을 0 이하로 내려가지 않게 감소시킨다.
         */
        private static float TickDown(float value, float deltaTime)
        {
            if (value > 0f)
            {
                return Mathf.Max(0f, value - deltaTime);
            }

            return 0f;
        }

        /*
         * 다음 시전을 실행할 간격이 지났는지 확인한다.
         */
        private bool IsCastIntervalReady()
        {
            return effectiveTickInterval <= 0f || TickRemaining <= 0f;
        }

        /*
         * 현재 선택지에 맞춰 스킬 런타임 보정값을 다시 계산한다.
         */
        private void RefreshRuntimeModifiers(SkillSnapshot snapshot)
        {
            var previousMax = effectiveMaxMagazineSize;
            var nextMax = ResolveMaxMagazineSize(Data);
            var nextBurst = ResolveBurstProjectileCount(Data);
            effectiveReloadDuration = ResolveReloadDuration(Data);
            effectiveTickInterval = ResolveTickInterval(Data);
            effectiveBurstInterval = ResolveBurstInterval(Data);
            effectiveCooldownDuration = ResolveCooldownDuration(Data);

            if (snapshot != null)
            {
                nextMax = Math.Max(0, nextMax + snapshot.MagazineBonus);
                if (nextBurst > 1)
                {
                    nextBurst += snapshot.AdditionalProjectileBonus;
                }

                effectiveReloadDuration *= Mathf.Max(0f, snapshot.ReloadTimeMultiplier);
                effectiveTickInterval *= Mathf.Max(0f, snapshot.ShotIntervalMultiplier);
                effectiveBurstInterval *= Mathf.Max(0f, snapshot.ShotIntervalMultiplier);
                effectiveCooldownDuration *= Mathf.Max(0f, snapshot.CooldownMultiplier);
            }

            effectiveMaxMagazineSize = nextMax;
            effectiveBurstProjectileCount = Math.Max(1, nextBurst);
            if (previousMax == effectiveMaxMagazineSize)
            {
                return;
            }

            if (effectiveMaxMagazineSize <= 0)
            {
                MagazineRemaining = 0;
                ReloadRemaining = 0f;
                return;
            }

            if (previousMax <= 0)
            {
                MagazineRemaining = effectiveMaxMagazineSize;
                return;
            }

            var delta = effectiveMaxMagazineSize - previousMax;
            MagazineRemaining = Mathf.Clamp(MagazineRemaining + delta, 0, effectiveMaxMagazineSize);
            if (MagazineRemaining > 0)
            {
                ReloadRemaining = 0f;
            }
        }

        /*
         * 최대 탄창 크기를 결정한다.
         */
        private static int ResolveMaxMagazineSize(SkillRuntimeData data)
        {
            return Math.Max(0, data.MagazineCapacity);
        }

        /*
         * 연속 발사 투사체 횟수를 결정한다.
         */
        private static int ResolveBurstProjectileCount(SkillRuntimeData data)
        {
            var projectile = data as ProjectileSkillRuntimeData;
            if (projectile != null && projectile.Projectile != null)
            {
                return Math.Max(1, projectile.Projectile.BurstProjectileCount);
            }

            return 1;
        }

        /*
         * 재장전 지속시간을 결정한다.
         */
        private static float ResolveReloadDuration(SkillRuntimeData data)
        {
            return Mathf.Max(0f, data.ReloadSeconds);
        }

        /*
         * 주기 간격을 결정한다.
         */
        private static float ResolveTickInterval(SkillRuntimeData data)
        {
            return Mathf.Max(0f, data.Timing.TickInterval);
        }

        /*
         * 연속 발사 간격을 결정한다.
         */
        private static float ResolveBurstInterval(SkillRuntimeData data)
        {
            var projectile = data as ProjectileSkillRuntimeData;
            if (projectile != null && projectile.Projectile != null)
            {
                var burstInterval = projectile.Projectile.BurstIntervalSeconds;
                if (burstInterval > 0f)
                {
                    return burstInterval;
                }
            }

            return ResolveTickInterval(data);
        }

        /*
         * 재사용 대기시간 지속시간을 결정한다.
         */
        private static float ResolveCooldownDuration(SkillRuntimeData data)
        {
            return Mathf.Max(0f, data.Timing.Cooldown);
        }

        /*
         * 발사나 시전이 끝났다면 재사용 대기 또는 재장전을 시작한다.
         */
        private void BeginRecoveryIfNeeded()
        {
            if (!UsesMagazine)
            {
                CooldownRemaining = effectiveCooldownDuration;
                return;
            }

            if (MagazineRemaining > 0)
            {
                return;
            }

            CooldownRemaining = effectiveCooldownDuration;
            if (ReloadDuration > 0f)
            {
                ReloadRemaining = ReloadDuration;
                return;
            }

            if (CooldownRemaining <= 0f)
            {
                MagazineRemaining = MaxMagazineSize;
            }
        }
    }
}


/*
 * 유닛이 보유한 스킬 런타임 목록을 관리한다.
 */
namespace Pakuri.InGame
{
    public class UnitSkillRuntimeSet
    {
        private readonly List<SkillRuntimeInstance> activeSkills = new List<SkillRuntimeInstance>();
        private readonly List<SkillRuntimeInstance> passiveSkills = new List<SkillRuntimeInstance>();

        public IReadOnlyList<SkillRuntimeInstance> ActiveSkills => activeSkills;
        public IReadOnlyList<SkillRuntimeInstance> PassiveSkills => passiveSkills;
        public int Count => activeSkills.Count + passiveSkills.Count;

        /*
         * 유닛의 스킬 런타임 목록을 비운다.
         */
        public void Clear()
        {
            activeSkills.Clear();
            passiveSkills.Clear();
        }

        /*
         * 같은 ID의 스킬을 교체하거나 새 스킬을 추가한다.
         */
        public void AddOrReplace(SkillRuntimeInstance instance)
        {
            var skills = passiveSkills;
            if (instance.Data.IsActive)
            {
                skills = activeSkills;
            }
            var existingIndex = FindIndexBySkillId(skills, instance.SkillId);
            if (existingIndex >= 0)
            {
                skills[existingIndex] = instance;
                return;
            }

            skills.Add(instance);
        }

        /*
         * 스킬 ID가 일치하는 런타임을 찾는다.
         */
        public SkillRuntimeInstance FindBySkillId(string skillId)
        {
            var index = FindIndexBySkillId(activeSkills, skillId);
            if (index >= 0)
            {
                return activeSkills[index];
            }

            index = FindIndexBySkillId(passiveSkills, skillId);
            if (index >= 0)
            {
                return passiveSkills[index];
            }

            return null;
        }

        /*
         * 선택지 ID가 일치하는 컴파일 결과를 찾는다.
         */
        public SkillChoiceRuntimeData FindChoice(string choiceId)
        {
            for (var i = 0; i < activeSkills.Count; i++)
            {
                var choice = FindChoice(activeSkills[i].Data, choiceId);
                if (choice != null)
                {
                    return choice;
                }
            }

            for (var i = 0; i < passiveSkills.Count; i++)
            {
                var choice = FindChoice(passiveSkills[i].Data, choiceId);
                if (choice != null)
                {
                    return choice;
                }
            }

            return null;
        }

        /*
         * 스킬 슬롯이 일치하는 런타임을 찾는다.
         */
        public SkillRuntimeInstance FindBySlot(SkillSlot slot)
        {
            for (var i = 0; i < activeSkills.Count; i++)
            {
                if (activeSkills[i] != null && activeSkills[i].Slot == slot)
                {
                    return activeSkills[i];
                }
            }

            return null;
        }

        /*
         * 유닛이 보유한 모든 스킬 런타임 시간을 갱신한다.
         */
        public void Tick(float deltaTime)
        {
            if (deltaTime <= 0f)
            {
                return;
            }

            for (var i = 0; i < activeSkills.Count; i++)
            {
                activeSkills[i].Tick(deltaTime);
            }
        }

        /*
         * 스킬 ID가 일치하는 런타임의 목록 위치를 찾는다.
         */
        private static int FindIndexBySkillId(List<SkillRuntimeInstance> skills, string skillId)
        {
            for (var i = 0; i < skills.Count; i++)
            {
                var runtime = skills[i];
                if (runtime != null && string.Equals(runtime.SkillId, skillId, StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }
            }

            return -1;
        }

        private static SkillChoiceRuntimeData FindChoice(SkillRuntimeData skill, string choiceId)
        {
            var choice = FindChoice(skill.EnhancementChoices, choiceId);
            if (choice != null)
            {
                return choice;
            }

            choice = FindChoice(skill.MasterChoices, choiceId);
            if (choice != null)
            {
                return choice;
            }

            var passive = skill as PassiveSkillRuntimeData;
            if (passive != null)
            {
                return FindChoice(passive.BaseModifierChoices, choiceId);
            }

            return null;
        }

        private static SkillChoiceRuntimeData FindChoice(SkillChoiceRuntimeData[] choices, string choiceId)
        {
            for (var i = 0; i < choices.Length; i++)
            {
                if (string.Equals(choices[i].ChoiceId, choiceId, StringComparison.OrdinalIgnoreCase))
                {
                    return choices[i];
                }
            }

            return null;
        }
    }
}


/*
 * 유닛 정의와 학습 상태를 읽어 유닛이 사용할 스킬 런타임 목록을 다시 구성한다.
 * 목록 저장과 조회를 담당하는 UnitSkillRuntimeSet과 달리 데이터 선택과 인스턴스 생성을 맡는다.
 */
namespace Pakuri.InGame
{
    public static class UnitSkillRuntimeBuilder
    {
        private static readonly SkillSlot[] ActiveSlots =
        {
            SkillSlot.A,
            SkillSlot.B,
            SkillSlot.C,
            SkillSlot.D,
            SkillSlot.E
        };

        /*
         * 학습한 활성 스킬과 패시브 목록을 다시 구성한다.
         */
        public static void RebuildLearnedSkillSet(UnitCombatState owner)
        {
            if (owner == null)
            {
                return;
            }

            owner.SkillRuntime.Clear();
            PopulateLearnedSkillSet(owner, owner.SkillRuntime);
        }

        /*
         * 지정된 활성 목록을 다시 구성한다.
         */
        public static void RebuildAssignedActiveSet(
            UnitCombatState owner,
            SkillDefinition[] definitions,
            SkillTriggerDefinition[] triggers)
        {
            if (owner == null)
            {
                return;
            }

            owner.SkillRuntime.Clear();
            if (definitions == null)
            {
                return;
            }

            var ownerId = string.Empty;
            if (owner.Identity != null)
            {
                ownerId = owner.Identity.DefinitionId;
            }
            for (var i = 0; i < definitions.Length; i++)
            {
                var data = SkillRuntimeCompiler.CompileActive(ownerId, definitions[i], triggers);
                owner.SkillRuntime.AddOrReplace(new SkillRuntimeInstance(owner, data));
            }
        }

        /*
         * 학습한 활성 스킬과 패시브를 목록에 채운다.
         */
        private static void PopulateLearnedSkillSet(
            UnitCombatState owner,
            UnitSkillRuntimeSet target)
        {
            if (owner == null || target == null)
            {
                return;
            }

            string monsterId = null;
            if (owner.Identity != null)
            {
                monsterId = owner.Identity.DefinitionId;
            }
            if (string.IsNullOrWhiteSpace(monsterId))
            {
                return;
            }

            var monster = GameDataLoader.CurrentCatalog.ResolveMonster(monsterId);
            for (var i = 0; i < ActiveSlots.Length; i++)
            {
                var source = GameDataLoader.CurrentCatalog.ResolveActiveSkill(monsterId, ActiveSlots[i]);
                if (source == null)
                {
                    continue;
                }

                var skillData = SkillRuntimeCompiler.CompileActive(monster, source);
                if (ContainsId(owner.SkillProgress.LearnedActiveSkillIds, skillData.SkillId))
                {
                    target.AddOrReplace(new SkillRuntimeInstance(owner, skillData));
                }
            }

            var passives = GameDataLoader.CurrentCatalog.GetPassiveSkills(monsterId);
            for (var i = 0; i < passives.Length; i++)
            {
                var passive = SkillRuntimeCompiler.CompilePassive(monster, passives[i]);
                if (ContainsId(owner.SkillProgress.LearnedPassiveSkillIds, passive.SkillId))
                {
                    target.AddOrReplace(new SkillRuntimeInstance(owner, passive));
                }
            }
        }

        /*
         * ID를 포함하는지 확인한다.
         */
        private static bool ContainsId(IEnumerable<string> ids, string targetId)
        {
            if (ids == null || string.IsNullOrWhiteSpace(targetId))
            {
                return false;
            }

            foreach (var id in ids)
            {
                if (string.Equals(id, targetId, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
