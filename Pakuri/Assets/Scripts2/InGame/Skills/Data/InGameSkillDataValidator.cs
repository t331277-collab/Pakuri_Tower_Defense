using System;
using System.Collections.Generic;
using Pakuri.Data;

namespace Pakuri.InGame
{
    public enum InGameSkillValidationSeverity
    {
        Warning,
        Error
    }

    public sealed class InGameSkillDataValidationIssue
    {
        public InGameSkillDataValidationIssue(
            InGameSkillValidationSeverity severity,
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

        public InGameSkillValidationSeverity Severity { get; }
        public string Code { get; }
        public string Message { get; }
        public string MonsterId { get; }
        public string SkillId { get; }
        public string Slot { get; }

        public override string ToString()
        {
            return $"[{Severity}] {Code}: {Message} (monster='{MonsterId}', skill='{SkillId}', slot='{Slot}')";
        }
    }

    public sealed class InGameSkillDataValidationReport
    {
        private readonly List<InGameSkillDataValidationIssue> issues = new List<InGameSkillDataValidationIssue>();

        public IReadOnlyList<InGameSkillDataValidationIssue> Issues => issues;
        public int ErrorCount { get; private set; }
        public int WarningCount { get; private set; }
        public bool IsValid => ErrorCount == 0;

        internal void Add(InGameSkillDataValidationIssue issue)
        {
            if (issue == null)
            {
                return;
            }

            issues.Add(issue);
            if (issue.Severity == InGameSkillValidationSeverity.Error)
            {
                ErrorCount++;
                return;
            }

            WarningCount++;
        }
    }

    public static class InGameSkillDataValidator
    {
        private static readonly HashSet<SkillRuntimeKind> SupportedActiveRuntimeKinds = new HashSet<SkillRuntimeKind>
        {
            SkillRuntimeKind.MagazineProjectile,
            SkillRuntimeKind.CooldownProjectile,
            SkillRuntimeKind.LineAttack,
            SkillRuntimeKind.AreaAttack,
            SkillRuntimeKind.Field,
            SkillRuntimeKind.Buff,
            SkillRuntimeKind.Shield,
            SkillRuntimeKind.Heal,
            SkillRuntimeKind.Mark,
            SkillRuntimeKind.Execute
        };

        public static InGameSkillDataValidationReport ValidateCatalog(GameDataCatalog fallbackCatalog = null)
        {
            var report = new InGameSkillDataValidationReport();
            var catalog = PakuriCsvRuntimeData.ResolveCatalogOrFallback(fallbackCatalog);
            if (catalog == null)
            {
                AddError(report, "CatalogMissing", "No GameDataCatalog could be resolved from CSV runtime data or fallback catalog.", string.Empty, string.Empty, string.Empty);
                return report;
            }

            ValidateMonsters(catalog.Monsters, report);
            return report;
        }

        public static InGameSkillDataValidationReport ValidateMonsters(MonsterDefinition[] monsters)
        {
            var report = new InGameSkillDataValidationReport();
            ValidateMonsters(monsters, report);
            return report;
        }

        private static void ValidateMonsters(MonsterDefinition[] monsters, InGameSkillDataValidationReport report)
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

        private static void ValidateActiveSkills(
            MonsterDefinition monster,
            Dictionary<string, string> activeSkillIds,
            InGameSkillDataValidationReport report)
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

                var mapped = InGameSkillDefinitionMapper.CreateActiveSkillData(monster, source);
                ValidateMappedSkill(monsterId, skillId, slot, source.Slot, true, mapped, report);
            }
        }

        private static void ValidatePassiveSkills(
            MonsterDefinition monster,
            Dictionary<string, string> passiveSkillIds,
            InGameSkillDataValidationReport report)
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

                var mapped = InGameSkillDefinitionMapper.CreatePassiveSkillData(monster, source);
                ValidateMappedSkill(monsterId, skillId, slot, source.Slot, false, mapped, report);
            }
        }

        private static void ValidateActiveSource(
            string monsterId,
            SkillDefinition source,
            Dictionary<string, string> activeSkillIds,
            Dictionary<SkillSlot, string> slots,
            InGameSkillDataValidationReport report)
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

        private static void ValidatePassiveSource(
            string monsterId,
            PassiveDefinition source,
            Dictionary<string, string> passiveSkillIds,
            Dictionary<SkillSlot, string> slots,
            InGameSkillDataValidationReport report)
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

        private static void ValidateDuplicateId(
            Dictionary<string, string> knownIds,
            string skillId,
            string monsterId,
            string code,
            string message,
            string slot,
            InGameSkillDataValidationReport report)
        {
            if (knownIds.TryGetValue(skillId, out var existingOwner))
            {
                AddError(report, code, $"{message} First owner: '{existingOwner}'.", monsterId, skillId, slot);
                return;
            }

            knownIds[skillId] = monsterId;
        }

        private static void ValidateMonsterIdPrefix(
            string monsterId,
            string skillId,
            string slot,
            string code,
            InGameSkillDataValidationReport report)
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

        private static void ValidateSlot(
            SkillSlot slot,
            bool active,
            string monsterId,
            string skillId,
            Dictionary<SkillSlot, string> slots,
            InGameSkillDataValidationReport report)
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

        private static void ValidateActiveRuntimeKind(
            string monsterId,
            SkillDefinition source,
            InGameSkillDataValidationReport report)
        {
            var slot = source.Slot.ToString();
            if (!Enum.IsDefined(typeof(SkillRuntimeKind), source.RuntimeKind)
                || !SupportedActiveRuntimeKinds.Contains(source.RuntimeKind))
            {
                AddError(report, "RuntimeKindUnsupported", $"Unsupported active runtime kind '{source.RuntimeKind}'.", monsterId, source.SkillId, slot);
            }
        }

        private static void ValidateTiming(string monsterId, SkillDefinition source, InGameSkillDataValidationReport report)
        {
            var slot = source.Slot.ToString();
            if (source.Radius < 0f || source.CooldownSeconds < 0f || source.ReloadSeconds < 0f || source.ShotIntervalSeconds < 0f || source.MagazineCapacity < 0)
            {
                AddError(report, "TimingNegative", "Timing, radius, reload, interval, and magazine values must not be negative.", monsterId, source.SkillId, slot);
            }

            if (source.RuntimeKind == SkillRuntimeKind.MagazineProjectile || source.RuntimeKind == SkillRuntimeKind.CooldownProjectile)
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

                return;
            }

            if (source.RuntimeKind != SkillRuntimeKind.Passive && source.CooldownSeconds <= 0f)
            {
                AddError(report, "CooldownMissing", "Non-passive active skill requires a positive cooldown.", monsterId, source.SkillId, slot);
            }
        }

        private static void ValidateMappedSkill(
            string monsterId,
            string skillId,
            string slot,
            SkillSlot expectedSlot,
            bool expectedActive,
            SkillData mapped,
            InGameSkillDataValidationReport report)
        {
            if (mapped == null)
            {
                AddError(report, "MappedSkillMissing", "Skill mapper returned null.", monsterId, skillId, slot);
                return;
            }

            if (string.IsNullOrWhiteSpace(mapped.SkillId))
            {
                AddError(report, "MappedSkillIdEmpty", "Mapped SkillData has an empty SkillId.", monsterId, skillId, slot);
            }

            if (mapped.IsActive != expectedActive)
            {
                AddError(report, "MappedActiveMismatch", "Mapped SkillData active/passive flag does not match the source kind.", monsterId, mapped.SkillId, mapped.Slot.ToString());
            }

            if ((int)mapped.Slot != (int)expectedSlot)
            {
                AddError(report, "MappedSlotMismatch", $"Mapped slot '{mapped.Slot}' does not match source slot '{expectedSlot}'.", monsterId, mapped.SkillId, mapped.Slot.ToString());
            }

            if (mapped.Timing == null)
            {
                AddError(report, "MappedTimingMissing", "Mapped SkillData has no timing spec.", monsterId, mapped.SkillId, mapped.Slot.ToString());
            }

            if (mapped.Targeting == null)
            {
                AddError(report, "MappedTargetingMissing", "Mapped SkillData has no targeting spec.", monsterId, mapped.SkillId, mapped.Slot.ToString());
            }
        }

        private static void AddError(InGameSkillDataValidationReport report, string code, string message, string monsterId, string skillId, string slot)
        {
            report.Add(new InGameSkillDataValidationIssue(InGameSkillValidationSeverity.Error, code, message, monsterId, skillId, slot));
        }

        private static void AddWarning(InGameSkillDataValidationReport report, string code, string message, string monsterId, string skillId, string slot)
        {
            report.Add(new InGameSkillDataValidationIssue(InGameSkillValidationSeverity.Warning, code, message, monsterId, skillId, slot));
        }
    }
}
