using Pakuri.Data;

/*
 * 카탈로그 상태 정의와 스킬 설정으로 실행용 상태 데이터를 만든다.
 */
namespace Pakuri.InGame
{
    public static class StatusEffectFactory
    {
        public static RuntimeStatusData Create(
            StatusEffectKind kind,
            string label,
            SkillDefinition source = null)
        {
            return StatusEffectRules.CreateStatusData(kind, label, source);
        }

        public static bool TryParseTargetScope(string rawValue, out StatusTargetScope scope)
        {
            return StatusEffectRules.TryParseStatusTargetScope(rawValue, out scope);
        }

        public static bool TryParseMergePolicy(string rawValue, out StatusMergePolicy policy)
        {
            return StatusEffectRules.TryParseStatusMergePolicy(rawValue, out policy);
        }

        public static bool TryParseShieldRefreshRule(string rawValue, out ShieldRefreshRule rule)
        {
            return StatusEffectRules.TryParseShieldRefreshPolicy(rawValue, out rule);
        }
    }
}
