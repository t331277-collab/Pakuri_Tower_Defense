using System;
using System.Collections.Generic;
using Pakuri.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/*
 * DamageMeterRuntimeTracker의 기록을 파티원별 피해량 패널로 표현하는 UI 컴포넌트.
 * 파티 순서와 선두 피해량을 기준으로 표시 값을 계산하고
 * 스킬·패시브·Trigger 출처 이름과 비율 구간을 카탈로그 정보로 구성한다.
 */
namespace Pakuri.InGame
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(DamageMeterRuntimeTracker))]
    public sealed class DamageMeterUIController : MonoBehaviour
    {
        private const int MaxPartySlots = 5;
        private const float RefreshIntervalSeconds = 0.2f;

        [SerializeField] private Button openButton;
        [SerializeField] private GameObject meterRoot;
        [SerializeField] private Button closeButton;
        [SerializeField] private DamagePanelView[] panels = new DamagePanelView[MaxPartySlots];
        [SerializeField] private StageManager stageManager;
        [SerializeField] private UnitSpawnManager unitSpawnManager;
        [SerializeField] private DamageMeterRuntimeTracker tracker;

        private readonly string[] partyMonsterIds = new string[MaxPartySlots];
        private float refreshRemaining;
        private int lastTrackerVersion = -1;

        private void Awake()
        {
            ResolveReferences();
            ResolveSceneUi();
            BindButtons();
            SetOverlayVisible(false);
        }

        private void OnEnable()
        {
            RefreshNow();
        }

        private void Update()
        {
            ResolveReferences();
            ResolveSceneUi();

            if (meterRoot == null || !meterRoot.activeSelf)
            {
                return;
            }

            refreshRemaining -= Time.deltaTime;
            var trackerVersion = tracker != null ? tracker.Version : -1;
            if (refreshRemaining <= 0f || trackerVersion != lastTrackerVersion)
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
            ResolveReferences();
            ResolveSceneUi();
            BuildPartyOrder();

            var catalog = ResolveCatalog();
            var leaderDamage = ResolveLeaderDamage();
            for (var i = 0; i < panels.Length; i++)
            {
                var panel = panels[i];
                if (panel == null)
                {
                    continue;
                }

                var monsterId = partyMonsterIds[i];
                if (string.IsNullOrWhiteSpace(monsterId))
                {
                    panel.SetVisible(false);
                    continue;
                }

                var monster = CsvDataLoader.CurrentCatalog.ResolveMonster(monsterId);
                tracker.TryGetRecord(monsterId, out var record);
                panel.SetRuntime(monster, record, leaderDamage, ResolveDisplayName, ResolveSortKey);
            }

            refreshRemaining = RefreshIntervalSeconds;
            lastTrackerVersion = tracker != null ? tracker.Version : -1;
        }

        private void BuildPartyOrder()
        {
            Array.Clear(partyMonsterIds, 0, partyMonsterIds.Length);

            var session = stageManager != null ? stageManager.ActiveSession : null;
            if (session != null && !string.IsNullOrWhiteSpace(session.SelectedMonsterId))
            {
                partyMonsterIds[0] = session.SelectedMonsterId;
            }
            else if (unitSpawnManager != null
                && unitSpawnManager.SpawnedPlayerModel != null
                && unitSpawnManager.SpawnedPlayerModel.Identity != null)
            {
                partyMonsterIds[0] = unitSpawnManager.SpawnedPlayerModel.Identity.DefinitionId;
            }

            var manifested = session != null ? session.ManifestedMonsterIds : null;
            if (manifested == null)
            {
                return;
            }

            for (var i = 0; i < manifested.Count && i + 1 < partyMonsterIds.Length; i++)
            {
                partyMonsterIds[i + 1] = manifested[i];
            }
        }

        private float ResolveLeaderDamage()
        {
            var max = 0f;
            for (var i = 0; i < partyMonsterIds.Length; i++)
            {
                var monsterId = partyMonsterIds[i];
                if (tracker != null && tracker.TryGetRecord(monsterId, out var record))
                {
                    max = Mathf.Max(max, record.TotalDamage);
                }
            }

            return max;
        }

        private string ResolveDisplayName(string monsterId, string sourceId)
        {
            var manager = CsvDataLoader.CurrentCatalog;
            var monster = manager.ResolveMonster(monsterId);
            if (monster != null)
            {
                var activeName = ResolveActiveSkillDisplayName(monster, sourceId);
                if (!string.IsNullOrWhiteSpace(activeName))
                {
                    return activeName;
                }

                var passiveName = ResolvePassiveDisplayName(monster, sourceId);
                if (!string.IsNullOrWhiteSpace(passiveName))
                {
                    return passiveName;
                }

                var choiceTitle = ResolveChoiceTitleForSource(monster, sourceId);
                if (!string.IsNullOrWhiteSpace(choiceTitle))
                {
                    return choiceTitle;
                }

                var triggerSourceName = ResolveTriggerSourceDisplayName(monster, sourceId);
                if (!string.IsNullOrWhiteSpace(triggerSourceName))
                {
                    return triggerSourceName;
                }
            }

            var choice = manager.GetData<SkillChoiceDefinition>(sourceId);
            if (choice != null && !string.IsNullOrWhiteSpace(choice.Title))
            {
                return choice.Title;
            }

            // Code Builder: 저장된 별칭 없이 sourceId를 최종 표시명으로 사용한다.
            return string.IsNullOrWhiteSpace(sourceId) ? "Unknown" : sourceId;
        }

        private int ResolveSortKey(string monsterId, string sourceId, int firstSeenIndex)
        {
            var monster = CsvDataLoader.CurrentCatalog.ResolveMonster(monsterId);
            var activeSkills = monster != null ? monster.ActiveSkills : null;
            if (activeSkills != null)
            {
                for (var i = 0; i < activeSkills.Length; i++)
                {
                    var skill = activeSkills[i];
                    if (skill != null && string.Equals(skill.SkillId, sourceId, StringComparison.OrdinalIgnoreCase))
                    {
                        return i;
                    }
                }
            }

            return 1000 + firstSeenIndex;
        }

        private static string ResolveTriggerSourceDisplayName(MonsterDefinition monster, string sourceId)
        {
            var triggers = monster != null ? monster.SkillTriggers : null;
            if (triggers == null)
            {
                return string.Empty;
            }

            for (var i = 0; i < triggers.Length; i++)
            {
                var trigger = triggers[i];
                if (trigger == null
                    || (!string.Equals(trigger.TriggerId, sourceId, StringComparison.OrdinalIgnoreCase)
                        && !string.Equals(trigger.TriggeredEffectId, sourceId, StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }

                var sourceSkillId = trigger.SourceSkillId;
                if (string.IsNullOrWhiteSpace(sourceSkillId)
                    || string.Equals(sourceSkillId, sourceId, StringComparison.OrdinalIgnoreCase))
                {
                    return string.Empty;
                }

                var passiveName = ResolvePassiveDisplayName(monster, sourceSkillId);
                if (!string.IsNullOrWhiteSpace(passiveName))
                {
                    return passiveName;
                }

                var activeName = ResolveActiveSkillDisplayName(monster, sourceSkillId);
                if (!string.IsNullOrWhiteSpace(activeName))
                {
                    return activeName;
                }
            }

            return string.Empty;
        }

        private static string ResolveChoiceTitleForSource(MonsterDefinition monster, string sourceId)
        {
            var triggers = monster != null ? monster.SkillTriggers : null;
            if (triggers == null)
            {
                return string.Empty;
            }

            for (var i = 0; i < triggers.Length; i++)
            {
                var trigger = triggers[i];
                if (trigger == null)
                {
                    continue;
                }

                if ((string.Equals(trigger.TriggerId, sourceId, StringComparison.OrdinalIgnoreCase)
                        || string.Equals(trigger.TriggeredEffectId, sourceId, StringComparison.OrdinalIgnoreCase))
                    && !string.IsNullOrWhiteSpace(trigger.RequiresActiveChoiceId))
                {
                    return ResolveChoiceTitle(trigger.RequiresActiveChoiceId);
                }
            }

            var activeSkills = monster.ActiveSkills;
            if (activeSkills == null)
            {
                return string.Empty;
            }

            for (var skillIndex = 0; skillIndex < activeSkills.Length; skillIndex++)
            {
                var effects = activeSkills[skillIndex] != null ? activeSkills[skillIndex].MultiEffects : null;
                if (effects == null)
                {
                    continue;
                }

                for (var effectIndex = 0; effectIndex < effects.Length; effectIndex++)
                {
                    var effect = effects[effectIndex];
                    if (effect != null
                        && string.Equals(effect.EffectId, sourceId, StringComparison.OrdinalIgnoreCase)
                        && !string.IsNullOrWhiteSpace(effect.RequiresActiveChoiceId))
                    {
                        return ResolveChoiceTitle(effect.RequiresActiveChoiceId);
                    }
                }
            }

            return string.Empty;
        }

        private static string ResolveChoiceTitle(string choiceId)
        {
            var choice = CsvDataLoader.CurrentCatalog.GetData<SkillChoiceDefinition>(choiceId);
            return choice != null ? choice.Title : string.Empty;
        }

        private static string ResolveActiveSkillDisplayName(MonsterDefinition monster, string sourceId)
        {
            var activeSkills = monster != null ? monster.ActiveSkills : null;
            if (activeSkills == null)
            {
                return string.Empty;
            }

            for (var i = 0; i < activeSkills.Length; i++)
            {
                var skill = activeSkills[i];
                if (skill != null && string.Equals(skill.SkillId, sourceId, StringComparison.OrdinalIgnoreCase))
                {
                    return skill.DisplayName;
                }
            }

            return string.Empty;
        }

        private static string ResolvePassiveDisplayName(MonsterDefinition monster, string sourceId)
        {
            var passives = monster != null ? monster.PassiveSkills : null;
            if (passives == null)
            {
                return string.Empty;
            }

            for (var i = 0; i < passives.Length; i++)
            {
                var passive = passives[i];
                if (passive != null && string.Equals(passive.PassiveId, sourceId, StringComparison.OrdinalIgnoreCase))
                {
                    return passive.DisplayName;
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

        private void ResolveReferences()
        {
            if (stageManager == null)
            {
                stageManager = FindSceneObject<StageManager>();
            }

            if (unitSpawnManager == null)
            {
                unitSpawnManager = FindSceneObject<UnitSpawnManager>();
            }

            if (tracker == null)
            {
                tracker = GetComponent<DamageMeterRuntimeTracker>();
            }
        }

        private void ResolveSceneUi()
        {
            if (openButton == null)
            {
                var buttonTransform = transform.Find("DamageMeterUIBtn");
                openButton = buttonTransform != null ? buttonTransform.GetComponent<Button>() : null;
            }

            if (meterRoot == null)
            {
                var root = transform.Find("DamageMeterUI");
                meterRoot = root != null ? root.gameObject : null;
            }

            if (closeButton == null && meterRoot != null)
            {
                var close = meterRoot.transform.Find("Close");
                closeButton = close != null ? close.GetComponent<Button>() : null;
            }

            if (panels == null || panels.Length != MaxPartySlots)
            {
                panels = new DamagePanelView[MaxPartySlots];
            }

            if (meterRoot == null)
            {
                return;
            }

            for (var i = 0; i < panels.Length; i++)
            {
                if (panels[i] != null && panels[i].IsBound)
                {
                    continue;
                }

                var panelRoot = meterRoot.transform.Find(string.Format("{0}PDamagePanel", i + 1));
                if (panelRoot == null)
                {
                    continue;
                }

                panels[i] = new DamagePanelView(panelRoot);
            }
        }

        private GameDataCatalog ResolveCatalog()
        {
            return CsvDataLoader.CurrentCatalog;
        }

        private static T FindSceneObject<T>() where T : UnityEngine.Object
        {
            var objects = Resources.FindObjectsOfTypeAll<T>();
            for (var i = 0; i < objects.Length; i++)
            {
                var component = objects[i] as Component;
                if (component != null && component.gameObject.scene.IsValid())
                {
                    return objects[i];
                }
            }

            return null;
        }

        [Serializable]
        private sealed class DamagePanelView
        {
            [SerializeField] private GameObject root;
            [SerializeField] private Image monsterImage;
            [SerializeField] private TMP_Text monsterNameText;
            [SerializeField] private TMP_Text totalDamageText;
            [SerializeField] private TMP_Text totalDamagePercentText;
            [SerializeField] private RectTransform meterBackground;
            [SerializeField] private RectTransform meterTemplate;
            [SerializeField] private readonly List<RectTransform> segments = new List<RectTransform>();

            private Vector2 templateSize;
            private Vector2 templatePosition;
            private Vector2 templateAnchorMin;
            private Vector2 templateAnchorMax;
            private Vector2 templatePivot;

            public bool IsBound => root != null;

            public DamagePanelView(Transform panelRoot)
            {
                Bind(panelRoot);
            }

            public void Bind(Transform panelRoot)
            {
                root = panelRoot != null ? panelRoot.gameObject : null;
                monsterImage = null;
                monsterNameText = null;
                totalDamageText = null;
                totalDamagePercentText = null;
                meterBackground = null;
                meterTemplate = null;
                segments.Clear();
                ResolveChildren();
            }

            public void SetRuntime(
                MonsterDefinition monster,
                MonsterDamageRecord record,
                float leaderDamage,
                Func<string, string, string> displayNameResolver,
                Func<string, string, int, int> sortKeyResolver)
            {
                ResolveChildren();
                SetVisible(true);

                var monsterId = monster != null ? monster.MonsterId : string.Empty;
                if (monsterNameText != null)
                {
                    monsterNameText.text = monster != null ? monster.DisplayName : monsterId;
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

                RefreshSegments(monsterId, record, leaderDamage, displayNameResolver, sortKeyResolver);
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
                string monsterId,
                MonsterDamageRecord record,
                float leaderDamage,
                Func<string, string, string> displayNameResolver,
                Func<string, string, int, int> sortKeyResolver)
            {
                if (meterTemplate == null)
                {
                    return;
                }

                var visibleSources = BuildVisibleSources(record, monsterId, sortKeyResolver);
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
                        var displayName = displayNameResolver(monsterId, source.SourceId);
                        label.text = string.Format("{0} {1}", displayName, FormatCompact(source.Damage));
                    }
                }
            }

            private List<SortedSkillSource> BuildVisibleSources(
                MonsterDamageRecord record,
                string monsterId,
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

                    var sortKey = sortKeyResolver != null ? sortKeyResolver(monsterId, source.SourceId, i) : i;
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

            private void ResolveChildren()
            {
                if (root == null)
                {
                    return;
                }

                if (monsterImage == null)
                {
                    monsterImage = FindImage(root.transform, "Image");
                }

                if (monsterNameText == null)
                {
                    monsterNameText = FindText(root.transform, "Monster_Name_Text");
                }

                if (totalDamageText == null)
                {
                    totalDamageText = FindText(root.transform, "Total_Damage");
                }

                if (totalDamagePercentText == null)
                {
                    totalDamagePercentText = FindText(root.transform, "Total_Damage_Persent");
                }

                if (meterBackground == null)
                {
                    var bg = root.transform.Find("MeterBG");
                    meterBackground = bg != null ? bg as RectTransform : null;
                }

                if (meterTemplate == null)
                {
                    var meter = root.transform.Find("Skill-Meter");
                    meterTemplate = meter != null ? meter as RectTransform : null;
                    if (meterTemplate != null)
                    {
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
                }
            }
        }

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

        private static Image FindImage(Transform root, string path)
        {
            var child = root != null ? root.Find(path) : null;
            return child != null ? child.GetComponent<Image>() : null;
        }

        private static TMP_Text FindText(Transform root, string path)
        {
            var child = root != null ? root.Find(path) : null;
            return child != null ? child.GetComponent<TMP_Text>() : null;
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
