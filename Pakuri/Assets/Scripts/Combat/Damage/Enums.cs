/*
 * 역할: 여러 전투·스킬·상태·유닛 시스템이 공유하는 열거형.
 * 책임: 공용 enum 타입과 값 정의만 보관한다.
 */

namespace Pakuri.Combat
{
    /// 피해 계산에 사용할 속성을 구분한다.
    public enum DamageAttribute
    {
        Physical,
        Fire,
        Lightning,
        Ice,
        Darkness,
        Holy
    }
}

namespace Pakuri.Data
{
    /// 적이 저작 데이터에서 맡은 고정 전투 역할을 구분한다.
    public enum EnemyEncounterRole
    {
        Normal,
        Day5Midboss,
        Day10Midboss,
        StageBoss
    }

    /// 유물 효과가 적용할 런타임 기능을 구분한다.
    public enum ArtifactEffectApplicationMode
    {
        SkillModifier,
        PassiveTrigger,
        ExecuteSkill,
        GrantSkill,
        SpawnUnit
    }

    /// 활성 스킬이 배치될 학습 위치를 구분한다.
    public enum SkillSlot
    {
        A,
        B,
        C,
        D,
        E,
        F,
        G,
        H,
        I,
        J
    }

    /// 스킬이 사용할 물리적 실행 방식을 구분한다.
    public enum SkillRuntimeKind
    {
        MagazineProjectile,
        CooldownProjectile,
        LineAttack,
        AreaAttack,
        SingleAttack,
        Buff,
        Shield,
        Heal,
        Mark,
        Execute,
        Passive
    }

    /// 선택이 어느 학습 단계에 속하는지 구분한다.
    public enum SkillChoiceGroup
    {
        ActiveEnhancement,
        ActiveMaster,
        PassiveEnhancement
    }

    /// 스킬 반응을 검사할 전투 시점을 구분한다.
    public enum SkillTriggerEvent
    {
        BuildExecutionData,
        OnCast,
        OnDeploymentCast,
        OnHit,
        OnExpire,
        OnHitCount,
        OnMagazineLastProjectileHit,
        OnReloadComplete,
        OnShieldExpire,
        OnShieldAbsorb,
        OnShieldBreak,
        OnHealOrShieldReceived,
        OnStatusExpire,
        OnOutgoingDamage,
        OnKill,
        OnSkillCast,
        CombatStart,
        BossCombatStart
    }

    /// 상태 효과를 적용할 대상 범위를 구분한다.
    public enum StatusTargetScope
    {
        Unspecified,
        AllAllies,
        Self
    }

    /// 상태 효과의 병합 방식을 구분한다.
    public enum StatusMergePolicy
    {
        Unspecified,
        SameSourceTakeHighest,
        SameSourceRefresh,
        SameSourceAddStacks,
        AlwaysStack
    }

    /// 상태 효과 종류를 구분한다.
    public enum StatusEffectKind
    {
        None,
        Shock,
        Chill,
        Freeze,
        Slow,
        Vulnerable,
        FireResistDown,
        FireExposure,
        Shield,
        Blessing,
        HolyExposure,
        HolyResistDown,
        NameMark,
        Silence,
        SlaughterPermit,
        ActionSpeedUp,
        PassiveBuff,
        SeinAHitMark,
        SeinDHeatStack,
        SeinDSuperheatedPresence
    }

    /// CSV 스킬 행의 종류를 구분한다.
    internal enum PakuriCsvSkillKind
    {
        Active,
        Passive
    }
}

namespace Pakuri.InGame
{
    /// 유닛이 속한 전투 진영을 구분한다.
    public enum UnitSide
    {
        Player,
        Enemy
    }

    /// 유닛의 전투 역할을 구분한다.
    public enum UnitRole
    {
        Monster,
        Enemy,
        Nexus,
        Summon
    }

    /// 스킬이 영향을 줄 진영 관계를 구분한다.
    public enum SkillTargetSide
    {
        Enemy,
        Self,
        Ally,
        AllAllies
    }

    /// 후보 중 우선 대상을 고르는 방식을 구분한다.
    public enum SkillTargetSelection
    {
        Nearest,
        LowestHealth,
        HighestHealth,
        HighestStacks,
        ManualPosition,
        Owner,
        Farthest,
        Random,
        NearestOtherFromEventTarget,
        Densest,
        BattlefieldCenter
    }

    /// 스킬이 대상을 포함할 공간 형태를 구분한다.
    public enum SkillTargetShape
    {
        Single,
        Line,
        Circle,
        Battlefield
    }

    /// 지원 효과가 전투 자원을 바꾸는 방식을 구분한다.
    public enum BuffEffectKind
    {
        Status,
        Heal,
        Shield,
        Charge
    }
}
