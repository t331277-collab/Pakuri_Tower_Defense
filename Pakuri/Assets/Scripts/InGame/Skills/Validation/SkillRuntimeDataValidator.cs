using System;
using System.Collections.Generic;
using Pakuri.Data;
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
#endif

namespace Pakuri.InGame
{
    /*
     * 인게임 스킬 검증 심각도에서 사용하는 선택 값을 정의한다.
     */
    public enum SkillRuntimeValidationSeverity
    {
        Warning,
        Error
    }

    /*
     * 스킬 데이터 검증에서 발견한 문제 한 건을 보관한다.
     */
    public sealed class SkillRuntimeDataValidationIssue
    {
        /*
         * 인게임 스킬 데이터 검증 문제에 필요한 값을 초기화한다.
         */
        public SkillRuntimeDataValidationIssue(
            SkillRuntimeValidationSeverity severity,
            string code,
            string message,
            string monsterId,
            string skillId,
            string slot)
        {
            Severity = severity;
            Code = code;
            Message = message;
            MonsterId = monsterId;
            SkillId = skillId;
            Slot = slot;
        }

        public SkillRuntimeValidationSeverity Severity { get; }
        public string Code { get; }
        public string Message { get; }
        public string MonsterId { get; }
        public string SkillId { get; }
        public string Slot { get; }

        /*
         * 현재 값을 읽기 쉬운 문자열로 반환한다.
         */
        public override string ToString()
        {
            return $"[{Severity}] {Code}: {Message} (monster='{MonsterId}', skill='{SkillId}', slot='{Slot}')";
        }
    }

    /*
     * 스킬 데이터 검증 결과와 문제 목록을 보관한다.
     */
    public sealed class SkillRuntimeDataValidationReport
    {
        private readonly List<SkillRuntimeDataValidationIssue> issues = new List<SkillRuntimeDataValidationIssue>();

        public IReadOnlyList<SkillRuntimeDataValidationIssue> Issues => issues;
        public int ErrorCount { get; private set; }
        public int WarningCount { get; private set; }
        public bool IsValid => ErrorCount == 0;

        /*
         * 검증 문제를 결과 목록에 추가한다.
         */
        internal void Add(SkillRuntimeDataValidationIssue issue)
        {
            if (issue == null)
            {
                return;
            }

            issues.Add(issue);
            if (issue.Severity == SkillRuntimeValidationSeverity.Error)
            {
                ErrorCount++;
                return;
            }

            WarningCount++;
        }
    }

    /*
     * 인게임 스킬 데이터 검증기의 구성 오류를 검사한다.
     */
    public static class SkillRuntimeDataValidator
    {
        private static readonly HashSet<SkillRuntimeKind> SupportedActiveRuntimeKinds = new HashSet<SkillRuntimeKind>
        {
            SkillRuntimeKind.MagazineProjectile,
            SkillRuntimeKind.CooldownProjectile,
            SkillRuntimeKind.LineAttack,
            SkillRuntimeKind.AreaAttack,
            SkillRuntimeKind.SingleAttack,
            SkillRuntimeKind.Field,
            SkillRuntimeKind.Buff,
            SkillRuntimeKind.Shield,
            SkillRuntimeKind.Heal,
            SkillRuntimeKind.Mark,
            SkillRuntimeKind.Execute
        };

        /*
         * 카탈로그를 검증한다.
         */
        public static SkillRuntimeDataValidationReport ValidateCatalog(GameDataCatalog fallbackCatalog = null)
        {
            var report = new SkillRuntimeDataValidationReport();
            var catalog = CsvDataLoader.ResolveCatalogOrFallback(fallbackCatalog);
            if (catalog == null)
            {
                AddError(report, "CatalogMissing", "No GameDataCatalog could be resolved from CSV runtime data or fallback catalog.", string.Empty, string.Empty, string.Empty);
                return report;
            }

            ValidateMonsters(catalog.Monsters, report);
            return report;
        }

        /*
         * 몬스터를 검증한다.
         */
        public static SkillRuntimeDataValidationReport ValidateMonsters(MonsterDefinition[] monsters)
        {
            var report = new SkillRuntimeDataValidationReport();
            ValidateMonsters(monsters, report);
            return report;
        }

        /*
         * 몬스터를 검증한다.
         */
        private static void ValidateMonsters(MonsterDefinition[] monsters, SkillRuntimeDataValidationReport report)
        {
            if (monsters == null || monsters.Length == 0)
            {
                AddError(report, "MonstersMissing", "Catalog has no monsters to validate.", string.Empty, string.Empty, string.Empty);
                return;
            }

            var activeSkillIds = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var passiveSkillIds = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < monsters.Length; i++)
            {
                var monster = monsters[i];
                if (monster == null)
                {
                    AddError(report, "MonsterNull", $"Monster entry at index {i} is null.", string.Empty, string.Empty, string.Empty);
                    continue;
                }

                var monsterId = monster.MonsterId;
                if (string.IsNullOrWhiteSpace(monsterId))
                {
                    AddError(report, "MonsterIdEmpty", $"Monster entry at index {i} has an empty MonsterId.", string.Empty, string.Empty, string.Empty);
                }

                ValidateActiveSkills(monster, activeSkillIds, report);
                ValidatePassiveSkills(monster, passiveSkillIds, report);
            }
        }

        /*
         * 활성 스킬을 검증한다.
         */
        private static void ValidateActiveSkills(
            MonsterDefinition monster,
            Dictionary<string, string> activeSkillIds,
            SkillRuntimeDataValidationReport report)
        {
            var monsterId = monster.MonsterId;
            var skills = monster.ActiveSkills;
            if (skills == null || skills.Length == 0)
            {
                AddWarning(report, "ActiveSkillsMissing", "Monster has no active skills.", monsterId, string.Empty, string.Empty);
                return;
            }

            var slots = new Dictionary<SkillSlot, string>();
            for (var i = 0; i < skills.Length; i++)
            {
                var source = skills[i];
                if (source == null)
                {
                    AddError(report, "ActiveSkillNull", $"Active skill entry {i} is null.", monsterId, string.Empty, string.Empty);
                    continue;
                }

                var slot = source.Slot.ToString();
                var skillId = source.SkillId;
                ValidateActiveSource(monsterId, source, activeSkillIds, slots, report);

                var mapped = SkillRuntimeCompiler.CompileActive(monster, source);
                ValidateMappedSkill(monsterId, skillId, slot, source.Slot, true, mapped, report);
            }
        }

        /*
         * 패시브 스킬을 검증한다.
         */
        private static void ValidatePassiveSkills(
            MonsterDefinition monster,
            Dictionary<string, string> passiveSkillIds,
            SkillRuntimeDataValidationReport report)
        {
            var monsterId = monster.MonsterId;
            var passives = monster.PassiveSkills;
            if (passives == null || passives.Length == 0)
            {
                AddWarning(report, "PassiveSkillsMissing", "Monster has no passive skills.", monsterId, string.Empty, string.Empty);
                return;
            }

            var slots = new Dictionary<SkillSlot, string>();
            for (var i = 0; i < passives.Length; i++)
            {
                var source = passives[i];
                if (source == null)
                {
                    AddError(report, "PassiveSkillNull", $"Passive skill entry {i} is null.", monsterId, string.Empty, string.Empty);
                    continue;
                }

                var slot = source.Slot.ToString();
                var skillId = source.PassiveId;
                ValidatePassiveSource(monsterId, source, passiveSkillIds, slots, report);

                var mapped = SkillRuntimeCompiler.CompilePassive(monster, source);
                ValidateMappedSkill(monsterId, skillId, slot, source.Slot, false, mapped, report);
            }
        }

        /*
         * 활성 출처를 검증한다.
         */
        private static void ValidateActiveSource(
            string monsterId,
            SkillDefinition source,
            Dictionary<string, string> activeSkillIds,
            Dictionary<SkillSlot, string> slots,
            SkillRuntimeDataValidationReport report)
        {
            var slot = source.Slot.ToString();
            var skillId = source.SkillId;
            if (string.IsNullOrWhiteSpace(skillId))
            {
                AddError(report, "SkillIdEmpty", "Active skill has an empty SkillId.", monsterId, skillId, slot);
            }
            else
            {
                ValidateDuplicateId(activeSkillIds, skillId, monsterId, "SkillIdDuplicate", "Active SkillId is duplicated.", slot, report);
                ValidateMonsterIdPrefix(monsterId, skillId, slot, "SkillCharacterMismatch", report);
            }

            if (string.IsNullOrWhiteSpace(source.DisplayName))
            {
                AddError(report, "SkillNameEmpty", "Active skill has an empty display name.", monsterId, skillId, slot);
            }

            ValidateSlot(source.Slot, true, monsterId, skillId, slots, report);
            ValidateActiveRuntimeKind(monsterId, source, report);
            ValidateTiming(monsterId, source, report);
        }

        /*
         * 패시브 출처를 검증한다.
         */
        private static void ValidatePassiveSource(
            string monsterId,
            PassiveDefinition source,
            Dictionary<string, string> passiveSkillIds,
            Dictionary<SkillSlot, string> slots,
            SkillRuntimeDataValidationReport report)
        {
            var slot = source.Slot.ToString();
            var skillId = source.PassiveId;
            if (string.IsNullOrWhiteSpace(skillId))
            {
                AddError(report, "PassiveIdEmpty", "Passive skill has an empty PassiveId.", monsterId, skillId, slot);
            }
            else
            {
                ValidateDuplicateId(passiveSkillIds, skillId, monsterId, "PassiveIdDuplicate", "PassiveId is duplicated.", slot, report);
                ValidateMonsterIdPrefix(monsterId, skillId, slot, "PassiveCharacterMismatch", report);
            }

            if (string.IsNullOrWhiteSpace(source.DisplayName))
            {
                AddError(report, "PassiveNameEmpty", "Passive skill has an empty display name.", monsterId, skillId, slot);
            }

            ValidateSlot(source.Slot, false, monsterId, skillId, slots, report);
        }

        /*
         * 중복 ID를 검증한다.
         */
        private static void ValidateDuplicateId(
            Dictionary<string, string> knownIds,
            string skillId,
            string monsterId,
            string code,
            string message,
            string slot,
            SkillRuntimeDataValidationReport report)
        {
            if (knownIds.TryGetValue(skillId, out var existingOwner))
            {
                AddError(report, code, $"{message} First owner: '{existingOwner}'.", monsterId, skillId, slot);
                return;
            }

            knownIds[skillId] = monsterId;
        }

        /*
         * 몬스터 ID 접두사를 검증한다.
         */
        private static void ValidateMonsterIdPrefix(
            string monsterId,
            string skillId,
            string slot,
            string code,
            SkillRuntimeDataValidationReport report)
        {
            if (string.IsNullOrWhiteSpace(monsterId) || string.IsNullOrWhiteSpace(skillId))
            {
                return;
            }

            var expectedPrefix = monsterId + "-";
            if (!skillId.StartsWith(expectedPrefix, StringComparison.OrdinalIgnoreCase))
            {
                AddError(report, code, $"Skill id should start with '{expectedPrefix}' to match its monster owner.", monsterId, skillId, slot);
            }
        }

        /*
         * 슬롯을 검증한다.
         */
        private static void ValidateSlot(
            SkillSlot slot,
            bool active,
            string monsterId,
            string skillId,
            Dictionary<SkillSlot, string> slots,
            SkillRuntimeDataValidationReport report)
        {
            var slotName = slot.ToString();
            if (!Enum.IsDefined(typeof(SkillSlot), slot))
            {
                AddError(report, "SlotUnsupported", "Skill uses an undefined slot value.", monsterId, skillId, slotName);
                return;
            }

            var slotMatchesKind = active ? slot >= SkillSlot.A && slot <= SkillSlot.E : slot >= SkillSlot.F && slot <= SkillSlot.J;
            if (!slotMatchesKind)
            {
                AddError(report, "SlotKindMismatch", active ? "Active skills must use slots A-E." : "Passive skills must use slots F-J.", monsterId, skillId, slotName);
            }

            if (slots.TryGetValue(slot, out var existingSkillId))
            {
                AddError(report, "SlotDuplicate", $"Slot is already used by '{existingSkillId}'.", monsterId, skillId, slotName);
                return;
            }

            slots[slot] = skillId;
        }

        /*
         * 활성 런타임 종류를 검증한다.
         */
        private static void ValidateActiveRuntimeKind(
            string monsterId,
            SkillDefinition source,
            SkillRuntimeDataValidationReport report)
        {
            var slot = source.Slot.ToString();
            if (!Enum.IsDefined(typeof(SkillRuntimeKind), source.RuntimeKind)
                || !SupportedActiveRuntimeKinds.Contains(source.RuntimeKind))
            {
                AddError(report, "RuntimeKindUnsupported", $"Unsupported active runtime kind '{source.RuntimeKind}'.", monsterId, source.SkillId, slot);
            }
        }

        /*
         * 실행 시간을 검증한다.
         */
        private static void ValidateTiming(string monsterId, SkillDefinition source, SkillRuntimeDataValidationReport report)
        {
            var slot = source.Slot.ToString();
            if (source.Radius < 0f
                || source.DamageDelaySeconds < 0f
                || source.CooldownSeconds < 0f
                || source.ReloadSeconds < 0f
                || source.ShotIntervalSeconds < 0f
                || source.MagazineCapacity < 0
                || source.ProjectileBurstCount < 0
                || source.ProjectileSpeed < 0f
                || source.PierceCount < 0
                || source.StatusChance < 0f
                || source.StatusChance > 1f)
            {
                AddError(report, "TimingNegative", "Timing, radius, delay, reload, interval, magazine, burst, projectile, pierce, and status chance values must be valid non-negative values.", monsterId, source.SkillId, slot);
            }

            if (source.RuntimeKind == SkillRuntimeKind.MagazineProjectile)
            {
                if (source.MagazineCapacity <= 0)
                {
                    AddError(report, "MagazineMissing", "Projectile skill requires a positive magazine capacity.", monsterId, source.SkillId, slot);
                }

                if (source.ReloadSeconds <= 0f)
                {
                    AddError(report, "ReloadMissing", "Projectile skill requires a positive reload duration.", monsterId, source.SkillId, slot);
                }

                if (source.ShotIntervalSeconds <= 0f)
                {
                    AddError(report, "ShotIntervalMissing", "Projectile skill requires a positive shot interval.", monsterId, source.SkillId, slot);
                }

                if (source.ProjectileSpeed <= 0f)
                {
                    AddError(report, "ProjectileSpeedMissing", "Projectile skill requires a positive projectile speed.", monsterId, source.SkillId, slot);
                }

                return;
            }

            if (source.RuntimeKind == SkillRuntimeKind.CooldownProjectile)
            {
                if (source.ProjectileSpeed <= 0f)
                {
                    AddError(report, "ProjectileSpeedMissing", "Cooldown projectile skill requires a positive projectile speed.", monsterId, source.SkillId, slot);
                }

                return;
            }

            if (source.RuntimeKind != SkillRuntimeKind.Passive && source.CooldownSeconds <= 0f)
            {
                AddError(report, "CooldownMissing", "Non-passive active skill requires a positive cooldown.", monsterId, source.SkillId, slot);
            }
        }

        /*
         * 변환된 스킬을 검증한다.
         */
        private static void ValidateMappedSkill(
            string monsterId,
            string skillId,
            string slot,
            SkillSlot expectedSlot,
            bool expectedActive,
            SkillRuntimeData mapped,
            SkillRuntimeDataValidationReport report)
        {
            if (mapped == null)
            {
                AddError(report, "MappedSkillMissing", "Skill mapper returned null.", monsterId, skillId, slot);
                return;
            }

            if (string.IsNullOrWhiteSpace(mapped.SkillId))
            {
                AddError(report, "MappedSkillIdEmpty", "Mapped SkillRuntimeData has an empty SkillId.", monsterId, skillId, slot);
            }

            if (mapped.IsActive != expectedActive)
            {
                AddError(report, "MappedActiveMismatch", "Mapped SkillRuntimeData active/passive flag does not match the source kind.", monsterId, mapped.SkillId, mapped.Slot.ToString());
            }

            if ((int)mapped.Slot != (int)expectedSlot)
            {
                AddError(report, "MappedSlotMismatch", $"Mapped slot '{mapped.Slot}' does not match source slot '{expectedSlot}'.", monsterId, mapped.SkillId, mapped.Slot.ToString());
            }

            if (mapped.Timing == null)
            {
                AddError(report, "MappedTimingMissing", "Mapped SkillRuntimeData has no timing spec.", monsterId, mapped.SkillId, mapped.Slot.ToString());
            }

            if (mapped.Targeting == null)
            {
                AddError(report, "MappedTargetingMissing", "Mapped SkillRuntimeData has no targeting spec.", monsterId, mapped.SkillId, mapped.Slot.ToString());
            }
        }

        /*
         * 오류를 추가한다.
         */
        private static void AddError(SkillRuntimeDataValidationReport report, string code, string message, string monsterId, string skillId, string slot)
        {
            report.Add(new SkillRuntimeDataValidationIssue(SkillRuntimeValidationSeverity.Error, code, message, monsterId, skillId, slot));
        }

        /*
         * 경고를 추가한다.
         */
        private static void AddWarning(SkillRuntimeDataValidationReport report, string code, string message, string monsterId, string skillId, string slot)
        {
            report.Add(new SkillRuntimeDataValidationIssue(SkillRuntimeValidationSeverity.Warning, code, message, monsterId, skillId, slot));
        }
    }
}

#if UNITY_EDITOR
namespace Pakuri.InGame.Editor
{
    public static class SkillRuntimeDataValidationMenu
    {
        [MenuItem("Pakuri/InGame/Validate Skill Data")]
        public static void ValidateSkillData()
        {
            var report = SkillRuntimeDataValidator.ValidateCatalog();
            foreach (var issue in report.Issues)
            {
                if (issue.Severity == SkillRuntimeValidationSeverity.Error)
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
#endif
