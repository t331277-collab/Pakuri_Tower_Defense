using UnityEditor;
using UnityEngine;

namespace Pakuri.InGame.Editor
{
    public static class InGameSkillDataValidationMenu
    {
        [MenuItem("Pakuri/InGame/Validate Skill Data")]
        public static void ValidateSkillData()
        {
            var report = InGameSkillDataValidator.ValidateCatalog();
            foreach (var issue in report.Issues)
            {
                if (issue.Severity == InGameSkillValidationSeverity.Error)
                {
                    Debug.LogError(issue.ToString());
                    continue;
                }

                Debug.LogWarning(issue.ToString());
            }

            if (report.IsValid)
            {
                Debug.Log($"InGame skill data validation passed with {report.WarningCount} warning(s).");
                return;
            }

            Debug.LogError($"InGame skill data validation failed with {report.ErrorCount} error(s) and {report.WarningCount} warning(s).");
        }
    }
}
