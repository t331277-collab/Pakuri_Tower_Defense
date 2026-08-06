/*
 * 역할: Damage Meter UI 표시.
 * 책임: 행을 생성하고 Tracker Snapshot을 연결해 합계와 InGame Meter Panel을 갱신한다.
 */

using System;
using System.Collections.Generic;
using Pakuri.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Pakuri.InGame
{

    /// 전투 중 누적된 유닛별 피해량을 Damage Meter 패널에 표시한다.
    public class DamageMeterUIController : MonoBehaviour
    {
        private const int MaxPartySlots = 5;
        private const float RefreshIntervalSeconds = 0.5f;

        private Button openButton;
        private GameObject meterRoot;
        private Button closeButton;
        private DamagePanelView[] panels = new DamagePanelView[MaxPartySlots];
        private StageManager stageManager;
        private UnitSpawnManager unitSpawnManager;
        private DamageMeterRuntimeTracker tracker;

        private bool referencesBound;
        private bool bindingFailed;

        private readonly string[] partyMonsterNames = new string[MaxPartySlots];
        private float refreshRemaining;

        /// Unity가 컴포넌트를 로드할 때 의존성과 소유 런타임 상태를 초기화한다.
        private void Awake()
        {
            if (!BindObject())
            {
                enabled = false;
                return;
            }

            BindButtons();
            for (var i = 0; i < panels.Length; i++)
            {
                panels[i]?.Initialize();
            }
            SetOverlayVisible(false);
        }

        /// Unity가 컴포넌트를 활성화할 때 구독과 활성 상태를 복원한다.
        private void OnEnable()
        {
            RefreshNow();
        }

        /// 현재 Unity 프레임에서 Update 갱신 동작을 진행한다.
        private void Update()
        {
            if (meterRoot == null || !meterRoot.activeSelf)
            {
                return;
            }

            refreshRemaining -= Time.deltaTime;
            if (refreshRemaining <= 0f)
            {
                RefreshNow();
            }
        }

        public void Open()
        {
            SetOverlayVisible(true);
            RefreshNow();
        }

        public void Close()
        {
            SetOverlayVisible(false);
        }

        public void RefreshNow()
        {
            BuildPartyOrder();

            var leaderDamage = ResolveLeaderDamage();
            for (var i = 0; i < panels.Length; i++)
            {
                var panel = panels[i];
                if (panel == null)
                {
                    continue;
                }

                var monsterName = partyMonsterNames[i];
                if (string.IsNullOrWhiteSpace(monsterName))
                {
                    panel.SetVisible(false);
                    continue;
                }

                var monster = GameDataLoader.CurrentCatalog.GetMonster(monsterName);
                tracker.TryGetRecord(monsterName, out var record);
                panel.SetRuntime(monster, record, leaderDamage, ResolveDisplayName, ResolveSortKey);
            }

            refreshRemaining = RefreshIntervalSeconds;
        }

        /// Registry의 플레이어 순서에 맞춰 Damage Meter 행을 배치한다.
        private void BuildPartyOrder()
        {
            Array.Clear(partyMonsterNames, 0, partyMonsterNames.Length);

            var session = stageManager != null ? stageManager.ActiveSession : null;
            if (session != null && session.PartyMembers.Count > 0)
            {
                for (var i = 0; i < session.PartyMembers.Count && i < partyMonsterNames.Length; i++)
                {
                    partyMonsterNames[i] = session.PartyMembers[i].MonsterName;
                }

                return;
            }

            var selectedPlayer = unitSpawnManager != null
                ? unitSpawnManager.FindPlayerMonsterBySlot(0)
                : null;
            if (selectedPlayer != null && selectedPlayer.Model.Identity != null)
            {
                partyMonsterNames[0] = selectedPlayer.Model.Identity.DefinitionName;
            }
        }

        private float ResolveLeaderDamage()
        {
            var max = 0f;
            for (var i = 0; i < partyMonsterNames.Length; i++)
            {
                var monsterName = partyMonsterNames[i];
                if (tracker != null && tracker.TryGetRecord(monsterName, out var record))
                {
                    max = Mathf.Max(max, record.TotalDamage);
                }
            }

            return max;
        }

        private string ResolveDisplayName(string monsterName, string sourceName)
        {
            var manager = GameDataLoader.CurrentCatalog;
            var monster = manager.GetMonster(monsterName);
            if (monster != null)
            {
                var skillName = ResolveSkillDisplayName(monster, sourceName);
                if (!string.IsNullOrWhiteSpace(skillName))
                {
                    return skillName;
                }

                var reaction = FindReaction(monster, sourceName);
                var choiceTitle = ResolveChoiceTitleForReaction(reaction);
                if (!string.IsNullOrWhiteSpace(choiceTitle))
                {
                    return choiceTitle;
                }

                var triggerSourceName = ResolveTriggerSourceDisplayName(monster, sourceName, reaction);
                if (!string.IsNullOrWhiteSpace(triggerSourceName))
                {
                    return triggerSourceName;
                }
            }

            var choice = manager.GetData<SkillChoice>(sourceName);
            if (choice != null && !string.IsNullOrWhiteSpace(choice.Title))
            {
                return choice.Title;
            }

            return string.IsNullOrWhiteSpace(sourceName) ? "Unknown" : sourceName;
        }

        private int ResolveSortKey(string monsterName, string sourceName, int firstSeenIndex)
        {
            var monster = GameDataLoader.CurrentCatalog.GetMonster(monsterName);
            var activeSkills = monster != null ? monster.ActiveSkills : null;
            if (activeSkills != null)
            {
                for (var i = 0; i < activeSkills.Length; i++)
                {
                    var skill = activeSkills[i];
                    if (skill != null && string.Equals(skill.SkillName, sourceName, StringComparison.OrdinalIgnoreCase))
                    {
                        return i;
                    }
                }
            }

            return 1000 + firstSeenIndex;
        }

        private static string ResolveTriggerSourceDisplayName(
            MonsterDefinition monster,
            string sourceName,
            SkillReaction trigger)
        {
            if (trigger == null)
            {
                return string.Empty;
            }

            var sourceSkillName = trigger.SourceSkillName;
            if (string.IsNullOrWhiteSpace(sourceSkillName)
                || string.Equals(sourceSkillName, sourceName, StringComparison.OrdinalIgnoreCase))
            {
                return string.Empty;
            }

            var passiveName = FindSkillDisplayName(monster?.PassiveSkills, sourceSkillName);
            return !string.IsNullOrWhiteSpace(passiveName)
                ? passiveName
                : FindSkillDisplayName(monster?.ActiveSkills, sourceSkillName);
        }

        private static string ResolveChoiceTitleForReaction(SkillReaction reaction)
        {
            return reaction?.RequiredActiveChoiceNames != null
                && reaction.RequiredActiveChoiceNames.Length > 0
                    ? ResolveChoiceTitle(reaction.RequiredActiveChoiceNames[0])
                    : string.Empty;
        }

        private static SkillReaction FindReaction(
            MonsterDefinition monster,
            string reactionName)
        {
            var reaction = FindReaction(monster?.ActiveSkills, reactionName);
            return reaction ?? FindReaction(monster?.PassiveSkills, reactionName);
        }

        private static SkillReaction FindReaction(
            SkillDefinition[] skills,
            string reactionName)
        {
            for (var i = 0; skills != null && i < skills.Length; i++)
            {
                var reactions = SkillExecutionRules.CreateDefinitionSnapshot(
                    skills[i]).Reactions;
                for (var j = 0; j < reactions.Count; j++)
                {
                    if (string.Equals(
                        reactions[j].ReactionName,
                        reactionName,
                        StringComparison.OrdinalIgnoreCase))
                    {
                        return reactions[j];
                    }
                }
            }
            return null;
        }

        private static string ResolveChoiceTitle(string choiceName)
        {
            var choice = GameDataLoader.CurrentCatalog.GetData<SkillChoice>(choiceName);
            return choice != null ? choice.Title : string.Empty;
        }

        private static string ResolveSkillDisplayName(MonsterDefinition monster, string sourceName)
        {
            var activeName = FindSkillDisplayName(monster?.ActiveSkills, sourceName);
            if (!string.IsNullOrWhiteSpace(activeName))
            {
                return activeName;
            }

            return FindSkillDisplayName(monster?.PassiveSkills, sourceName);
        }

        private static string FindSkillDisplayName(SkillDefinition[] skills, string sourceName)
        {
            if (skills == null)
            {
                return string.Empty;
            }

            for (var i = 0; i < skills.Length; i++)
            {
                var skill = skills[i];
                if (skill != null && string.Equals(skill.SkillName, sourceName, StringComparison.OrdinalIgnoreCase))
                {
                    return skill.DisplayName;
                }
            }
            return string.Empty;
        }

        private void SetOverlayVisible(bool visible)
        {
            if (meterRoot != null)
            {
                meterRoot.SetActive(visible);
            }

            if (openButton != null)
            {
                openButton.gameObject.SetActive(!visible);
                openButton.interactable = !visible;
            }
        }

        private void BindButtons()
        {
            if (openButton != null)
            {
                openButton.onClick.RemoveListener(Open);
                openButton.onClick.AddListener(Open);
            }

            if (closeButton != null)
            {
                closeButton.onClick.RemoveListener(Close);
                closeButton.onClick.AddListener(Close);
            }
        }

        private bool BindObject()
        {
            if (referencesBound)
            {
                return !bindingFailed;
            }

            referencesBound = true;
            var valid = true;
            openButton = UiBindingUtility.BindChild<Button>(this, "DamageMeter/OpenButton", nameof(openButton), ref valid);
            meterRoot = UiBindingUtility.BindChildObject(this, transform, "DamageMeter/Panel", nameof(meterRoot), ref valid);
            closeButton = UiBindingUtility.BindChild<Button>(this, "DamageMeter/Panel/Close", nameof(closeButton), ref valid);
            stageManager = UiBindingUtility.BindSceneComponent<StageManager>(this, nameof(stageManager), ref valid);
            unitSpawnManager = UiBindingUtility.BindSceneComponent<UnitSpawnManager>(this, nameof(unitSpawnManager), ref valid);
            tracker = UiBindingUtility.BindSceneComponent<DamageMeterRuntimeTracker>(this, nameof(tracker), ref valid);

            panels = new DamagePanelView[MaxPartySlots];
            for (var i = 0; i < panels.Length; i++)
            {
                panels[i] = new DamagePanelView();
                panels[i].BindObject(this, transform, $"DamageMeter/Panel/{i + 1}PDamagePanel", i, ref valid);
            }

            bindingFailed = !valid;
            return valid;
        }

        [Serializable]
        private class DamagePanelView
        {
            private GameObject root;
            private Image monsterImage;
            private TMP_Text monsterNameText;
            private TMP_Text totalDamageText;
            private TMP_Text totalDamagePercentText;
            private RectTransform meterBackground;
            private RectTransform meterTemplate;
            private List<RectTransform> segments = new List<RectTransform>();

            private Vector2 templateSize;
            private Vector2 templatePosition;
            private Vector2 templateAnchorMin;
            private Vector2 templateAnchorMax;
            private Vector2 templatePivot;

            public void BindObject(
                Component owner,
                Transform rootTransform,
                string path,
                int index,
                ref bool valid)
            {
                root = UiBindingUtility.BindChildObject(owner, rootTransform, path, $"panels[{index}].root", ref valid);
                var panelTransform = root != null ? root.transform : null;
                monsterImage = UiBindingUtility.BindChild<Image>(owner, panelTransform, "Image", $"panels[{index}].monsterImage", ref valid);
                monsterNameText = UiBindingUtility.BindChild<TMP_Text>(owner, panelTransform, "Monster_Name_Text", $"panels[{index}].monsterNameText", ref valid);
                totalDamageText = UiBindingUtility.BindChild<TMP_Text>(owner, panelTransform, "Total_Damage", $"panels[{index}].totalDamageText", ref valid);
                totalDamagePercentText = UiBindingUtility.BindChild<TMP_Text>(owner, panelTransform, "Total_Damage_Persent", $"panels[{index}].totalDamagePercentText", ref valid);
                meterBackground = UiBindingUtility.BindChild<RectTransform>(owner, panelTransform, "MeterBG", $"panels[{index}].meterBackground", ref valid);
                meterTemplate = UiBindingUtility.BindChild<RectTransform>(owner, panelTransform, "Skill-Meter", $"panels[{index}].meterTemplate", ref valid);
                segments = new List<RectTransform>();
            }

            public void Initialize()
            {
                if (segments == null)
                {
                    segments = new List<RectTransform>();
                }

                if (meterTemplate == null)
                {
                    return;
                }

                templateSize = meterTemplate.rect.size;
                if (templateSize.x <= 0f || templateSize.y <= 0f)
                {
                    templateSize = meterTemplate.sizeDelta;
                }

                templatePosition = meterTemplate.anchoredPosition;
                templateAnchorMin = meterTemplate.anchorMin;
                templateAnchorMax = meterTemplate.anchorMax;
                templatePivot = meterTemplate.pivot;
            }

            public void SetRuntime(
                MonsterDefinition monster,
                MonsterDamageRecord record,
                float leaderDamage,
                Func<string, string, string> displayNameResolver,
                Func<string, string, int, int> sortKeyResolver)
            {
                SetVisible(true);

                var monsterName = monster != null ? monster.MonsterName : string.Empty;
                if (monsterNameText != null)
                {
                    monsterNameText.text = monster != null ? monster.DisplayName : monsterName;
                }

                RefreshImage(monster);

                var total = record != null ? record.TotalDamage : 0f;
                if (totalDamageText != null)
                {
                    totalDamageText.text = FormatCompact(total);
                }

                if (totalDamagePercentText != null)
                {
                    var percent = leaderDamage > 0f ? Mathf.RoundToInt(Mathf.Clamp01(total / leaderDamage) * 100f) : 0;
                    totalDamagePercentText.text = percent + "%";
                }

                RefreshSegments(monsterName, record, leaderDamage, displayNameResolver, sortKeyResolver);
            }

            public void SetVisible(bool visible)
            {
                if (root != null)
                {
                    root.SetActive(visible);
                }
            }

            private void RefreshImage(MonsterDefinition monster)
            {
                if (monsterImage == null)
                {
                    return;
                }

                if (monster != null && monster.MonsterIconImage != null)
                {
                    monsterImage.sprite = monster.MonsterIconImage;
                    monsterImage.enabled = true;
                    return;
                }

                monsterImage.sprite = null;
                monsterImage.enabled = false;
            }

            private void RefreshSegments(
                string monsterName,
                MonsterDamageRecord record,
                float leaderDamage,
                Func<string, string, string> displayNameResolver,
                Func<string, string, int, int> sortKeyResolver)
            {
                if (meterTemplate == null)
                {
                    return;
                }

                var visibleSources = BuildVisibleSources(record, monsterName, sortKeyResolver);
                EnsureSegmentCount(visibleSources.Count);
                if (leaderDamage <= 0f || visibleSources.Count == 0)
                {
                    SetSegmentCountActive(0);
                    return;
                }

                var totalWidth = ResolveMeterWidth();
                var cursor = 0f;
                for (var i = 0; i < segments.Count; i++)
                {
                    var segment = segments[i];
                    var visible = i < visibleSources.Count;
                    if (segment != null)
                    {
                        segment.gameObject.SetActive(visible);
                    }

                    if (!visible || segment == null)
                    {
                        continue;
                    }

                    var source = visibleSources[i].Record;
                    var width = totalWidth * Mathf.Clamp01(source.Damage / leaderDamage);
                    width = Mathf.Min(width, Mathf.Max(0f, totalWidth - cursor));
                    ConfigureSegment(segment, cursor, width);
                    ApplySegmentColor(segment, i);
                    cursor += width;

                    var label = segment.GetComponentInChildren<TMP_Text>(true);
                    if (label != null)
                    {
                        var displayName = displayNameResolver(monsterName, source.SourceName);
                        label.text = string.Format("{0} {1}", displayName, FormatCompact(source.Damage));
                    }
                }
            }

            private List<SortedSkillSource> BuildVisibleSources(
                MonsterDamageRecord record,
                string monsterName,
                Func<string, string, int, int> sortKeyResolver)
            {
                var result = new List<SortedSkillSource>();
                var sources = record != null ? record.OrderedSources : null;
                if (sources == null)
                {
                    return result;
                }

                for (var i = 0; i < sources.Count; i++)
                {
                    var source = sources[i];
                    if (source == null || source.Damage <= 0f)
                    {
                        continue;
                    }

                    var sortKey = sortKeyResolver != null ? sortKeyResolver(monsterName, source.SourceName, i) : i;
                    result.Add(new SortedSkillSource(source, sortKey, i));
                }

                result.Sort((left, right) =>
                {
                    var sortCompare = left.SortKey.CompareTo(right.SortKey);
                    return sortCompare != 0 ? sortCompare : left.FirstSeenIndex.CompareTo(right.FirstSeenIndex);
                });
                return result;
            }

            private void EnsureSegmentCount(int count)
            {
                if (meterTemplate == null)
                {
                    return;
                }

                if (segments.Count == 0)
                {
                    segments.Add(meterTemplate);
                }

                while (segments.Count < count)
                {
                    var clone = UnityEngine.Object.Instantiate(meterTemplate, meterTemplate.parent);
                    clone.name = meterTemplate.name;
                    segments.Add(clone);
                }
            }

            private void SetSegmentCountActive(int count)
            {
                for (var i = 0; i < segments.Count; i++)
                {
                    if (segments[i] != null)
                    {
                        segments[i].gameObject.SetActive(i < count);
                    }
                }
            }

            private float ResolveMeterWidth()
            {
                if (meterBackground != null && meterBackground.rect.width > 0f)
                {
                    return meterBackground.rect.width;
                }

                return templateSize.x > 0f ? templateSize.x : 1f;
            }

            private void ConfigureSegment(RectTransform segment, float xOffset, float width)
            {
                segment.anchorMin = templateAnchorMin;
                segment.anchorMax = templateAnchorMax;
                segment.pivot = templatePivot;

                var templateLeft = templatePosition.x - (templateSize.x * templatePivot.x);
                var segmentX = templateLeft + xOffset + (Mathf.Max(0f, width) * templatePivot.x);
                segment.anchoredPosition = new Vector2(segmentX, templatePosition.y);
                segment.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Mathf.Max(0f, width));
                segment.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, templateSize.y);
            }

            private static readonly Color[] SegmentColors =
            {
                new Color(1f, 0.18f, 0.12f, 1f),
                new Color(0.12f, 0.35f, 1f, 1f),
                new Color(0.55f, 1f, 0.2f, 1f),
                new Color(0.25f, 0.82f, 1f, 1f),
                new Color(1f, 0.86f, 0.12f, 1f),
                new Color(0.62f, 0.25f, 1f, 1f),
                new Color(0.02f, 0.42f, 0.16f, 1f),
            };

            private static void ApplySegmentColor(RectTransform segment, int index)
            {
                if (segment == null)
                {
                    return;
                }

                var image = segment.GetComponent<Image>();
                if (image == null)
                {
                    image = segment.GetComponentInChildren<Image>(true);
                }

                if (image != null)
                {
                    image.color = SegmentColors[index % SegmentColors.Length];
                }
            }

        }

        /// SortedSkillSource 처리에 함께 전달되는 값들을 묶는다.
        private readonly struct SortedSkillSource
        {

            public SortedSkillSource(SkillDamageRecord record, int sortKey, int firstSeenIndex)
            {
                Record = record;
                SortKey = sortKey;
                FirstSeenIndex = firstSeenIndex;
            }

            public SkillDamageRecord Record { get; }
            public int SortKey { get; }
            public int FirstSeenIndex { get; }
        }

        private static string FormatCompact(float value)
        {
            var clamped = Mathf.Max(0f, value);
            if (clamped >= 1000000f)
            {
                return clamped < 10000000f ? (clamped / 1000000f).ToString("0.##") + "M" : Mathf.RoundToInt(clamped / 1000000f) + "M";
            }

            if (clamped >= 1000f)
            {
                return clamped < 100000f ? (clamped / 1000f).ToString("0.#") + "K" : Mathf.RoundToInt(clamped / 1000f) + "K";
            }

            return Mathf.RoundToInt(clamped).ToString();
        }
    }
}
