using Pakuri.Combat;
using UnityEngine;

/*
 * 모든 전투 유닛이 공유하는 모델들을 한 전투 상태로 묶어 보관한다.
 * 피해 계산과 상태 효과 처리는 각 Combat 스크립트가 이 상태를 읽고 갱신한다.
 */
namespace Pakuri.InGame
{
    public class UnitCombatState
    {
        public UnitIdentity Identity = new UnitIdentity();
        public UnitCombatStats Stats = new UnitCombatStats();
        public UnitDefenseStats Defenses = new UnitDefenseStats();
        public UnitCombatResources Resources = new UnitCombatResources();
        public UnitSkillRuntimeSet SkillRuntime = new UnitSkillRuntimeSet();
        public SingleChargeState ActiveCharge;
        public UnitStatusCollection Statuses = new UnitStatusCollection();
        public UnitSkillProgress SkillProgress = new UnitSkillProgress();
        public bool IsBoss;
        public bool AutoAttackEnabled = true;
        public bool AutoSkillEnabled = true;

        public bool IsNexus => Identity.Role == UnitRole.Nexus;

        /*
         * 직접 보호막과 활성 상태 보호막의 총량을 반환한다.
         */
        public float GetTotalShield()
        {
            var directShield = Mathf.Max(0f, Resources.DirectShield);
            var statusShield = Mathf.Max(0f, Statuses.GetTotalShieldAmount());
            return Mathf.Round(Mathf.Max(0f, directShield + statusShield));
        }

        /*
         * 직접 보호막과 상태 보호막의 합계를 현재 자원 값에 반영한다.
         */
        public void SyncShield()
        {
            Resources.DirectShield = Mathf.Round(Mathf.Max(0f, Resources.DirectShield));
            Resources.CurrentShield = GetTotalShield();
        }
    }
}
