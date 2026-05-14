namespace Pakuri.InGame
{
    public class BaseUnitRuntimeModel
    {
        public UnitIdentity Identity = new UnitIdentity();
        public UnitStatsRuntime Stats = new UnitStatsRuntime();
        public UnitDefenseRuntime Defenses = new UnitDefenseRuntime();
        public UnitResourceRuntime Resources = new UnitResourceRuntime();
        public UnitSkillRuntimeSet SkillRuntime = new UnitSkillRuntimeSet();
        public bool AutoAttackEnabled = true;
        public bool AutoSkillEnabled = true;
    }
}
