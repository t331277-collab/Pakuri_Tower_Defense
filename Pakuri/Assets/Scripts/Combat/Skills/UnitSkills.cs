using System;
using System.Collections.Generic;
using Pakuri.Data;
using UnityEngine;

/*
 * 실행 준비가 끝난 스킬 하나가 전투 중 가지는 변경 가능한 상태를 관리한다.
 * 재사용 대기시간, 탄창·재장전, Tick, 연속 발사, 적중 횟수를 갱신하고
 * 현재 Choice Snapshot에 따른 시전 가능 여부와 시간 보정값을 적용한다.
 */
namespace Pakuri.InGame
{
    public class SkillUseState
    {
        /*
         * 스킬 사용 상태에 필요한 값을 초기화한다.
         */
        public SkillUseState(UnitCombatState owner /* 정보를 소유한 유닛 */, SkillExecutionDefinition data /* 처리할 실행 데이터 */)
        {
            Owner = owner;
            Data = data;
            BasePlan = SkillNodeCompiler.Compile(data, null, data.NormalizedPlanNodes);
            ResetRuntimeState();
        }

        public UnitCombatState Owner { get; }
        public SkillExecutionDefinition Data { get; }
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
         * 재사용 대기시간, 탄창, 연속 적중 상태를 초기화한다.
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
        public float ResolveConsecutiveHitDamageMultiplier(UnitCombatState target /* 효과를 받을 대상 유닛 */, SkillSnapshot snapshot /* 적용할 스킬 강화 정보 */)
        {
            if (target == null)
            {
                return 1f;
            }

            var projectileData = Data as ProjectileSkillDefinition;
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
        public void Tick(float deltaTime /* 이전 갱신 이후 지난 시간 */)
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
        public bool CanCastWithSnapshot(SkillSnapshot snapshot /* 적용할 스킬 강화 정보 */)
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
        public bool TryBeginCast(SkillSnapshot snapshot /* 적용할 스킬 강화 정보 */)
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
        public bool ReduceReloadRemaining(float seconds /* 초 */)
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
        public bool ReduceCooldownRemaining(float seconds /* 초 */)
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
        private static float TickDown(float value /* 처리할 값 */, float deltaTime /* 이전 갱신 이후 지난 시간 */)
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
         * 현재 선택지에 맞춰 스킬 사용 보정값을 다시 계산한다.
         */
        private void RefreshRuntimeModifiers(SkillSnapshot snapshot /* 적용할 스킬 강화 정보 */)
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
        private static int ResolveMaxMagazineSize(SkillExecutionDefinition data /* 처리할 실행 데이터 */)
        {
            return Math.Max(0, data.MagazineCapacity);
        }

        /*
         * 연속 발사 투사체 횟수를 결정한다.
         */
        private static int ResolveBurstProjectileCount(SkillExecutionDefinition data /* 처리할 실행 데이터 */)
        {
            var projectile = data as ProjectileSkillDefinition;
            if (projectile != null && projectile.Projectile != null)
            {
                return Math.Max(1, projectile.Projectile.BurstProjectileCount);
            }

            return 1;
        }

        /*
         * 재장전 지속시간을 결정한다.
         */
        private static float ResolveReloadDuration(SkillExecutionDefinition data /* 처리할 실행 데이터 */)
        {
            return Mathf.Max(0f, data.ReloadSeconds);
        }

        /*
         * 주기 간격을 결정한다.
         */
        private static float ResolveTickInterval(SkillExecutionDefinition data /* 처리할 실행 데이터 */)
        {
            return Mathf.Max(0f, data.Timing.TickInterval);
        }

        /*
         * 연속 발사 간격을 결정한다.
         */
        private static float ResolveBurstInterval(SkillExecutionDefinition data /* 처리할 실행 데이터 */)
        {
            var projectile = data as ProjectileSkillDefinition;
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
        private static float ResolveCooldownDuration(SkillExecutionDefinition data /* 처리할 실행 데이터 */)
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
 * 유닛이 학습한 스킬과 Choice, 스킬별 전투 상태를 한곳에서 관리한다.
 * 실행 직전에는 현재 전투 상황을 반영한 Snapshot을 만들어 SkillExecution에 전달한다.
 */
namespace Pakuri.InGame
{
    public class UnitSkills
    {
        private readonly List<SkillUseState> activeSkills = new List<SkillUseState>();
        private readonly List<SkillUseState> passiveSkills = new List<SkillUseState>();
        public readonly HashSet<string> LearnedActiveSkillIds = new HashSet<string>();
        public readonly HashSet<string> LearnedPassiveSkillIds = new HashSet<string>();
        public readonly HashSet<string> ChosenChoiceIds = new HashSet<string>();

        public IReadOnlyList<SkillUseState> ActiveSkills => activeSkills;
        public IReadOnlyList<SkillUseState> PassiveSkills => passiveSkills;
        public int Count => activeSkills.Count + passiveSkills.Count;

        /*
         * 현재 학습 상태와 전투 상황을 반영한 스킬 Snapshot을 만든다.
         */
        public SkillSnapshot CreateSnapshot(
            UnitCombatState owner /* 스킬을 사용하는 유닛 */,
            SkillUseState skill /* 실행할 스킬 상태 */,
            CombatUnitRegistry roster /* 전투에 등록된 유닛 목록 */)
        {
            return BuildSnapshot(owner, skill, roster);
        }

        /*
         * 유닛의 활성 스킬과 패시브 실행 목록을 비운다.
         */
        public void Clear()
        {
            activeSkills.Clear();
            passiveSkills.Clear();
        }

        /*
         * 같은 ID의 스킬을 교체하거나 새 스킬을 추가한다.
         */
        public void AddOrReplace(SkillUseState instance /* 생성된 게임 오브젝트 */)
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
         * 스킬 ID가 일치하는 사용 상태를 찾는다.
         */
        public SkillUseState FindBySkillId(string skillId /* 스킬 식별자 */)
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
         * 선택지 ID가 일치하는 실행 정의를 찾는다.
         */
        public SkillChoice FindChoice(string choiceId /* 스킬 선택지 식별자 */)
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
         * 스킬 슬롯이 일치하는 사용 상태를 찾는다.
         */
        public SkillUseState FindBySlot(SkillSlot slot /* 스킬이나 유닛이 배치될 슬롯 */)
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
         * 유닛이 보유한 모든 활성 스킬의 시간을 갱신한다.
         */
        public void Tick(float deltaTime /* 이전 갱신 이후 지난 시간 */)
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
         * 스킬 ID가 일치하는 사용 상태의 목록 위치를 찾는다.
         */
        private static int FindIndexBySkillId(List<SkillUseState> skills /* 스킬 목록 */, string skillId /* 스킬 식별자 */)
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

        /*
         * FindChoice에 해당하는 값을 찾아 반환한다.
         */
        private static SkillChoice FindChoice(SkillExecutionDefinition skill /* 실행하거나 검사할 스킬 */, string choiceId /* 스킬 선택지 식별자 */)
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

            var passive = skill as PassiveSkillDefinition;
            if (passive != null)
            {
                return FindChoice(passive.BaseModifierChoices, choiceId);
            }

            return null;
        }

        /*
         * FindChoice에 해당하는 값을 찾아 반환한다.
         */
        private static SkillChoice FindChoice(SkillChoice[] choices /* 선택지 목록 */, string choiceId /* 스킬 선택지 식별자 */)
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
        /*
         * 유닛이 학습한 선택지를 현재 스킬 실행 정보에 적용한다.
         */
        private SkillSnapshot BuildSnapshot(UnitCombatState owner /* 정보를 소유한 유닛 */, SkillUseState runtime /* 실행 중인 스킬 정보 */, CombatUnitRegistry roster /* 전투에 등록된 유닛 목록 */)
        {
            SkillExecutionDefinition skillData = null;
            if (runtime != null)
            {
                skillData = runtime.Data;
            }
            var snapshot = new SkillSnapshot(skillData);
            ApplyPassiveBaseModifiers(snapshot, owner, skillData);
            System.Collections.Generic.ICollection<string> chosenChoiceIds = null;
            if (owner != null && owner.Skills != null)
            {
                chosenChoiceIds = owner.Skills.ChosenChoiceIds;
            }
            if (skillData == null || chosenChoiceIds == null || chosenChoiceIds.Count == 0)
            {
                return snapshot;
            }

            ApplyChoices(snapshot, chosenChoiceIds, skillData, owner, roster);
            return snapshot;
        }

        /*
         * 패시브 기본 보정값을 적용한다.
         */
        private static void ApplyPassiveBaseModifiers(
            SkillSnapshot snapshot /* 적용할 스킬 강화 정보 */,
            UnitCombatState owner /* 정보를 소유한 유닛 */,
            SkillExecutionDefinition skillData /* 스킬 실행 데이터 */)
        {
            if (snapshot == null
                || owner == null
                || owner.Identity.Role != UnitRole.Monster
                || owner.Skills == null
                || skillData == null
                || owner.Skills.LearnedPassiveSkillIds == null
                || owner.Skills.LearnedPassiveSkillIds.Count == 0)
            {
                return;
            }

            foreach (var passiveId in owner.Skills.LearnedPassiveSkillIds)
            {
                var passiveRuntime = owner.Skills.FindBySkillId(passiveId);
                PassiveSkillDefinition passive = null;
                if (passiveRuntime != null)
                {
                    passive = passiveRuntime.Data as PassiveSkillDefinition;
                }
                if (passive == null)
                {
                    continue;
                }

                for (var i = 0; i < passive.BaseModifierChoices.Length; i++)
                {
                    var modifier = passive.BaseModifierChoices[i];
                    if (modifier != null && AppliesToSkill(modifier.Source, skillData))
                    {
                        snapshot.ApplyChoiceSpec(modifier);
                    }
                }
            }
        }

        /*
         * 선택지를 적용한다.
         */
        private static void ApplyChoices(
            SkillSnapshot snapshot /* 적용할 스킬 강화 정보 */,
            System.Collections.Generic.ICollection<string> chosenChoiceIds /* 선택된 선택지 식별자 목록 */,
            SkillExecutionDefinition skillData /* 스킬 실행 데이터 */,
            UnitCombatState owner /* 정보를 소유한 유닛 */,
            CombatUnitRegistry roster /* 전투에 등록된 유닛 목록 */)
        {
            if (snapshot == null || chosenChoiceIds == null || skillData == null)
            {
                return;
            }

            foreach (var choiceId in chosenChoiceIds)
            {
                var choice = owner.Skills.FindChoice(choiceId);
                if (choice != null
                    && AppliesToSkill(choice.Source, skillData)
                    && SkillRequirement.MeetsSourceStatus(choice.Source, owner))
                {
                    snapshot.AddActiveChoiceId(choice.ChoiceId);
                    snapshot.ApplyChoiceSpec(choice);
                    ApplyDynamicChoiceRules(snapshot, choice.Source, owner, roster);
                }
            }
        }

        /*
         * 동적 선택지 규칙을 적용한다.
         */
        private static void ApplyDynamicChoiceRules(
            SkillSnapshot snapshot /* 적용할 스킬 강화 정보 */,
            SkillChoiceDefinition choice /* 적용하거나 검사할 스킬 선택지 */,
            UnitCombatState owner /* 정보를 소유한 유닛 */,
            CombatUnitRegistry roster /* 전투에 등록된 유닛 목록 */)
        {
            if (snapshot == null || choice == null || roster == null)
            {
                return;
            }

            if (choice.CountStatusKind != StatusEffectKind.None
                && choice.DamageMultiplierPerCount > 0f)
            {
                ApplyCountStatusDamageMultiplier(
                    snapshot,
                    owner,
                    roster,
                    choice.CountTargetSide,
                    choice.CountStatusKind,
                    choice.DamageMultiplierPerCount,
                    choice.CountMax);
            }

            SkillNodeDefinition[] targetNodes = SkillNodeMapper.FilterSkillNodeDefinitionsForTarget(
                choice.NormalizedPlanNodes,
                snapshot.SkillId);
            SkillNode[] nodes = SkillNodeMapper.MapSkillNodeDefinitions(targetNodes);
            for (int i = 0; i < nodes.Length; i++)
            {
                if (nodes[i] == null)
                {
                    continue;
                }

                CountStatusDamageActionOp? action = nodes[i].CountStatusDamageAction;
                if (!action.HasValue)
                {
                    continue;
                }

                ApplyCountStatusDamageMultiplier(
                    snapshot,
                    owner,
                    roster,
                    action.Value.TargetSide,
                    action.Value.StatusKind,
                    action.Value.AmountPerCount,
                    action.Value.MaximumCount);
            }
        }

        /*
         * 횟수 상태 피해 배율을 적용한다.
         */
        private static void ApplyCountStatusDamageMultiplier(
            SkillSnapshot snapshot /* 적용할 스킬 강화 정보 */,
            UnitCombatState owner /* 정보를 소유한 유닛 */,
            CombatUnitRegistry roster /* 전투에 등록된 유닛 목록 */,
            SkillMultiEffectTargetSide targetSide /* 대상 진영 */,
            StatusEffectKind statusKind /* 상태 효과 종류 */,
            float amountPerCount /* 수치 개별 개수 */,
            int countMax /* 개수 최대 */)
        {
            if (snapshot == null
                || statusKind == StatusEffectKind.None
                || amountPerCount <= 0f
                || roster == null)
            {
                return;
            }

            var count = CountMatchingTargets(owner, roster, targetSide, statusKind);
            if (countMax > 0)
            {
                count = Mathf.Min(count, countMax);
            }

            if (count <= 0)
            {
                return;
            }

            snapshot.ApplyDynamicDamageMultiplier(1f + count * amountPerCount);
        }

        /*
         * 선택지 조건과 일치하는 대상 수를 계산한다.
         */
        private static int CountMatchingTargets(
            UnitCombatState owner /* 정보를 소유한 유닛 */,
            CombatUnitRegistry roster /* 전투에 등록된 유닛 목록 */,
            SkillMultiEffectTargetSide side /* 진영 */,
            StatusEffectKind statusKind /* 상태 효과 종류 */)
        {
            if (owner == null || roster == null || statusKind == StatusEffectKind.None)
            {
                return 0;
            }

            var entries = ResolveCountEntries(owner, roster, side);
            var count = 0;
            for (var i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                if (entry == null || !entry.IsAlive || entry.Model == null)
                {
                    continue;
                }

                if (HasStatus(entry.Model, statusKind))
                {
                    count++;
                }
            }

            return count;
        }

        /*
         * 횟수 유닛 항목을 결정한다.
         */
        private static System.Collections.Generic.IReadOnlyList<CombatUnitEntry> ResolveCountEntries(
            UnitCombatState owner /* 정보를 소유한 유닛 */,
            CombatUnitRegistry roster /* 전투에 등록된 유닛 목록 */,
            SkillMultiEffectTargetSide side /* 진영 */)
        {
            if (roster == null || owner == null || owner.Identity == null)
            {
                return System.Array.Empty<CombatUnitEntry>();
            }

            var ownerIsEnemy = owner.Identity.Side == UnitSide.Enemy;
            switch (side)
            {
                case SkillMultiEffectTargetSide.Self:
                    var allies = roster.Players;
                    if (ownerIsEnemy)
                    {
                        allies = roster.Enemies;
                    }
                    var self = FindEntryForModel(owner, allies);
                    if (IsSkillTarget(self))
                    {
                        return new[] { self };
                    }
                    return System.Array.Empty<CombatUnitEntry>();
                case SkillMultiEffectTargetSide.AllAllies:
                    if (ownerIsEnemy)
                    {
                        return FilterSkillTargets(roster.Enemies);
                    }
                    return FilterSkillTargets(roster.Players);
                default:
                    if (ownerIsEnemy)
                    {
                        return FilterSkillTargets(roster.Players);
                    }
                    return FilterSkillTargets(roster.Enemies);
            }
        }

        /*
         * 스킬 대상을 조건에 맞는 값만 선별한다.
         */
        private static System.Collections.Generic.IReadOnlyList<CombatUnitEntry> FilterSkillTargets(
            System.Collections.Generic.IReadOnlyList<CombatUnitEntry> entries /* 등록 정보 목록 */)
        {
            if (entries == null || entries.Count == 0)
            {
                return System.Array.Empty<CombatUnitEntry>();
            }

            var filtered = new System.Collections.Generic.List<CombatUnitEntry>();
            for (var i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                if (!IsSkillTarget(entry))
                {
                    continue;
                }

                filtered.Add(entry);
            }

            return filtered;
        }

        /*
         * 유닛이 선택지 효과의 적용 대상인지 확인한다.
         */
        private static bool IsSkillTarget(CombatUnitEntry entry /* 처리할 등록 정보 */)
        {
            UnitIdentity identity = null;
            if (entry != null && entry.Model != null)
            {
                identity = entry.Model.Identity;
            }
            return entry != null && (identity == null || identity.Role != UnitRole.Nexus);
        }

        /*
         * 유닛 항목 대상 모델을 찾는다.
         */
        private static CombatUnitEntry FindEntryForModel(
            UnitCombatState model /* 전투 상태를 읽거나 변경할 유닛 */,
            System.Collections.Generic.IReadOnlyList<CombatUnitEntry> entries /* 등록 정보 목록 */)
        {
            if (model == null || entries == null)
            {
                return null;
            }

            for (var i = 0; i < entries.Count; i++)
            {
                if (entries[i] != null && object.ReferenceEquals(entries[i].Model, model))
                {
                    return entries[i];
                }
            }

            return null;
        }

        /*
         * 상태를 보유하고 있는지 확인한다.
         */
        private static bool HasStatus(UnitCombatState model /* 전투 상태를 읽거나 변경할 유닛 */, StatusEffectKind statusKind /* 상태 효과 종류 */, int minimumStacks = 1 /* 최소 중첩 수 */)
        {
            if (model == null || statusKind == StatusEffectKind.None || minimumStacks <= 0)
            {
                return false;
            }

            if (statusKind == StatusEffectKind.Shield)
            {
                return model.Resources != null && model.Resources.CurrentShield > 0f;
            }

            return model.Statuses != null && model.Statuses.GetStacks(statusKind) >= minimumStacks;
        }

        /*
         * 선택지 효과가 현재 스킬에 적용되는지 확인한다.
         */
        private static bool AppliesToSkill(SkillChoiceDefinition choice /* 적용하거나 검사할 스킬 선택지 */, SkillExecutionDefinition skillData /* 스킬 실행 데이터 */)
        {
            if (choice == null || skillData == null)
            {
                return false;
            }

            if (choice.NormalizedPlanNodes != null && choice.NormalizedPlanNodes.Length > 0)
            {
                return SkillNodeMapper.HasSkillNodeForTarget(
                    choice.NormalizedPlanNodes,
                    skillData.SkillId);
            }

            var targetSkillId = choice.SkillId;
            if (!string.IsNullOrWhiteSpace(choice.TargetSkillId))
            {
                targetSkillId = choice.TargetSkillId;
            }
            return !string.IsNullOrWhiteSpace(targetSkillId)
                && string.Equals(targetSkillId, skillData.SkillId, System.StringComparison.OrdinalIgnoreCase);
        }

        /*
         * 패시브에 연결된 강화 선택지를 Snapshot으로 만든다.
         */
        public static SkillSnapshot ResolvePassiveChoices(UnitCombatState owner /* 정보를 소유한 유닛 */, string passiveId /* 패시브 식별자 */)
        {
            return ResolveChoices(owner, passiveId, true);
        }

        /*
         * 활성 스킬에 연결된 강화와 마스터 선택지를 Snapshot으로 만든다.
         */
        public static SkillSnapshot ResolveActiveChoices(UnitCombatState owner /* 정보를 소유한 유닛 */, string skillId /* 스킬 식별자 */)
        {
            return ResolveChoices(owner, skillId, false);
        }

        /*
         * ResolveChoices 결과를 계산해 반환한다.
         */
        private static SkillSnapshot ResolveChoices(UnitCombatState owner /* 정보를 소유한 유닛 */, string skillId /* 스킬 식별자 */, bool useTargetSkillId /* 사용 대상 스킬 식별자 여부 */)
        {
            var snapshot = new SkillSnapshot(null);
            System.Collections.Generic.ICollection<string> chosenChoiceIds = null;
            if (owner != null && owner.Skills != null)
            {
                chosenChoiceIds = owner.Skills.ChosenChoiceIds;
            }
            if (chosenChoiceIds == null || chosenChoiceIds.Count == 0 || string.IsNullOrWhiteSpace(skillId))
            {
                return snapshot;
            }

            foreach (var choiceId in chosenChoiceIds)
            {
                var choice = owner.Skills.FindChoice(choiceId);
                if (choice == null)
                {
                    continue;
                }

                var choiceSkillId = choice.Source.SkillId;
                if (useTargetSkillId && !string.IsNullOrWhiteSpace(choice.Source.TargetSkillId))
                {
                    choiceSkillId = choice.Source.TargetSkillId;
                }

                if (!string.Equals(choiceSkillId, skillId, System.StringComparison.OrdinalIgnoreCase)
                    || !SkillRequirement.MeetsSourceStatus(choice.Source, owner))
                {
                    continue;
                }

                snapshot.AddActiveChoiceId(choice.ChoiceId);
                snapshot.ApplyChoiceSpec(choice);
            }

            return snapshot;
        }
    }
}


/*
 * 유닛 정의와 학습 상태를 읽어 유닛이 사용할 스킬 목록을 다시 구성한다.
 * UnitSkills가 보관할 전투 상태를 만들고 학습하지 않은 스킬은 목록에 넣지 않는다.
 */
namespace Pakuri.InGame
{
    public static class UnitSkillsBuilder
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
        public static void RebuildLearnedSkillSet(UnitCombatState owner /* 정보를 소유한 유닛 */)
        {
            if (owner == null)
            {
                return;
            }

            owner.Skills.Clear();
            PopulateLearnedSkillSet(owner, owner.Skills);
        }

        /*
         * 지정된 활성 목록을 다시 구성한다.
         */
        public static void RebuildAssignedActiveSet(
            UnitCombatState owner /* 정보를 소유한 유닛 */,
            SkillDefinition[] definitions /* 정의 목록 */,
            SkillTriggerDefinition[] triggers /* 트리거 목록 */)
        {
            if (owner == null)
            {
                return;
            }

            owner.Skills.Clear();
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
                var data = SkillDefinitionCompiler.CompileActive(ownerId, definitions[i], triggers);
                owner.Skills.AddOrReplace(new SkillUseState(owner, data));
            }
        }

        /*
         * 학습한 활성 스킬과 패시브를 목록에 채운다.
         */
        private static void PopulateLearnedSkillSet(
            UnitCombatState owner /* 정보를 소유한 유닛 */,
            UnitSkills target /* 실행 스킬을 저장할 목록 */)
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

                var skillData = SkillDefinitionCompiler.CompileActive(monster, source);
                if (ContainsId(owner.Skills.LearnedActiveSkillIds, skillData.SkillId))
                {
                    target.AddOrReplace(new SkillUseState(owner, skillData));
                }
            }

            var passives = GameDataLoader.CurrentCatalog.GetPassiveSkills(monsterId);
            for (var i = 0; i < passives.Length; i++)
            {
                var passive = SkillDefinitionCompiler.CompilePassive(monster, passives[i]);
                if (ContainsId(owner.Skills.LearnedPassiveSkillIds, passive.SkillId))
                {
                    target.AddOrReplace(new SkillUseState(owner, passive));
                }
            }
        }

        /*
         * ID를 포함하는지 확인한다.
         */
        private static bool ContainsId(IEnumerable<string> ids /* 식별자 목록 */, string targetId /* 대상 식별자 */)
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
