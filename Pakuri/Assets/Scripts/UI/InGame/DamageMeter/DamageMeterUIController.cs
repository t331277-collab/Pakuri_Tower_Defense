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

    /// DamageMeterUIController가 담당하는 입력 또는 표시 흐름을 조정하고 관련 런타임 상태를 갱신한다.
    public class DamageMeterUIController : MonoBehaviour
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

        /// Unity가 컴포넌트를 로드할 때 의존성과 소유 런타임 상태를 초기화한다.
        private void Awake()
        {
            ResolveReferences();
            ResolveSceneUi();
            BindButtons();
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

        /// Open 작업을 수행한다.
        public void Open()
        {
            SetOverlayVisible(true);
            RefreshNow();
        }

        /// Close 작업을 수행한다.
        public void Close()
        {
            SetOverlayVisible(false);
        }

        /// Now를 현재 런타임 모델을 기준으로 갱신한다.
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

                var monster = GameDataLoader.CurrentCatalog.GetMonster(monsterId);
                tracker.TryGetRecord(monsterId, out var record);
                panel.SetRuntime(monster, record, leaderDamage, ResolveDisplayName, ResolveSortKey);
            }

            refreshRemaining = RefreshIntervalSeconds;
            lastTrackerVersion = tracker != null ? tracker.Version : -1;
        }

        /// PartyOrder를 구성한다.
        private void BuildPartyOrder()
        {
            Array.Clear(partyMonsterIds, 0, partyMonsterIds.Length);

            var session = stageManager != null ? stageManager.ActiveSession : null;
            if (session != null && session.PartyMembers.Count > 0)
            {
                for (var i = 0; i < session.PartyMembers.Count && i < partyMonsterIds.Length; i++)
                {
                    partyMonsterIds[i] = session.PartyMembers[i].MonsterId;
                }

                return;
            }

            var selectedPlayer = unitSpawnManager != null
                ? unitSpawnManager.FindPlayerMonsterBySlot(0)
                : null;
            if (selectedPlayer != null && selectedPlayer.Model.Identity != null)
            {
                partyMonsterIds[0] = selectedPlayer.Model.Identity.DefinitionId;
            }
        }

        /// LeaderDamage를 결정한다.
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

        /// 전달된 런타임 입력값을 사용해 DisplayName를 결정한다.
        private string ResolveDisplayName(string monsterId, string sourceId)
        {
            var manager = GameDataLoader.CurrentCatalog;
            var monster = manager.GetMonster(monsterId);
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

            var choice = manager.GetData<SkillChoice>(sourceId);
            if (choice != null && !string.IsNullOrWhiteSpace(choice.Title))
            {
                return choice.Title;
            }

            return string.IsNullOrWhiteSpace(sourceId) ? "Unknown" : sourceId;
        }

        /// 전달된 런타임 입력값을 사용해 SortKey를 결정한다.
        private int ResolveSortKey(string monsterId, string sourceId, int firstSeenIndex)
        {
            var monster = GameDataLoader.CurrentCatalog.GetMonster(monsterId);
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

        /// 전달된 런타임 입력값을 사용해 TriggerSourceDisplayName를 결정한다.
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
                    || !string.Equals(trigger.TriggerId, sourceId, StringComparison.OrdinalIgnoreCase))
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

        /// 전달된 런타임 입력값을 사용해 ChoiceTitleForSource를 결정한다.
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

                if (string.Equals(trigger.TriggerId, sourceId, StringComparison.OrdinalIgnoreCase)
                    && trigger.RequiredActiveChoiceIds != null
                    && trigger.RequiredActiveChoiceIds.Length > 0)
                {
                    return ResolveChoiceTitle(trigger.RequiredActiveChoiceIds[0]);
                }
            }

            return string.Empty;
        }

        /// 전달된 choiceId 값을 사용해 ChoiceTitle를 결정한다.
        private static string ResolveChoiceTitle(string choiceId)
        {
            var choice = GameDataLoader.CurrentCatalog.GetData<SkillChoice>(choiceId);
            return choice != null ? choice.Title : string.Empty;
        }

        /// 전달된 런타임 입력값을 사용해 ActiveSkillDisplayName를 결정한다.
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
                    return skill.SkillName;
                }
            }

            return string.Empty;
        }

        /// 전달된 런타임 입력값을 사용해 PassiveDisplayName를 결정한다.
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
                if (passive != null && string.Equals(passive.SkillId, sourceId, StringComparison.OrdinalIgnoreCase))
                {
                    return passive.SkillName;
                }
            }

            return string.Empty;
        }

        /// 전달된 visible 값을 사용해 OverlayVisible를 갱신한다.
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

        /// Buttons를 런타임 사건 또는 씬 대상에 연결한다.
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

        /// References를 결정한다.
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

        /// SceneUi를 결정한다.
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

        /// Catalog를 결정한다.
        private GameDataCatalog ResolveCatalog()
        {
            return GameDataLoader.CurrentCatalog;
        }

        /// SceneObject를 찾는다.
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

        /// DamagePanelView가 소유하는 데이터와 동작을 캡슐화한다.
        [Serializable]
        private class DamagePanelView
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

            /// DamagePanelView 인스턴스를 전달된 런타임 입력값으로 초기화한다.
            public DamagePanelView(Transform panelRoot)
            {
                Bind(panelRoot);
            }

            /// 전달된 panelRoot 값을 사용해 요청값를 런타임 사건 또는 씬 대상에 연결한다.
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

            /// 전달된 런타임 입력값을 사용해 Runtime를 갱신한다.
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

            /// 전달된 visible 값을 사용해 Visible를 갱신한다.
            public void SetVisible(bool visible)
            {
                if (root != null)
                {
                    root.SetActive(visible);
                }
            }

            /// 전달된 monster 값을 사용해 Image를 현재 런타임 모델을 기준으로 갱신한다.
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

            /// 전달된 런타임 입력값을 사용해 Segments를 현재 런타임 모델을 기준으로 갱신한다.
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

            /// 전달된 런타임 입력값을 사용해 VisibleSources를 구성한다.
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

            /// 전달된 count 값을 사용해 EnsureSegmentCount 작업을 수행한다.
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

            /// 전달된 count 값을 사용해 SegmentCountActive를 갱신한다.
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

            /// MeterWidth를 결정한다.
            private float ResolveMeterWidth()
            {
                if (meterBackground != null && meterBackground.rect.width > 0f)
                {
                    return meterBackground.rect.width;
                }

                return templateSize.x > 0f ? templateSize.x : 1f;
            }

            /// 전달된 런타임 입력값을 사용해 ConfigureSegment 작업을 수행한다.
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

            /// 전달된 런타임 입력값을 사용해 SegmentColor를 적용한다.
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

            /// Children를 결정한다.
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

        /// SortedSkillSource 처리에 함께 전달되는 값들을 묶는다.
        private readonly struct SortedSkillSource
        {

            /// SortedSkillSource 인스턴스를 전달된 런타임 입력값으로 초기화한다.
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

        /// 전달된 런타임 입력값을 사용해 Image를 찾는다.
        private static Image FindImage(Transform root, string path)
        {
            var child = root != null ? root.Find(path) : null;
            return child != null ? child.GetComponent<Image>() : null;
        }

        /// 전달된 런타임 입력값을 사용해 Text를 찾는다.
        private static TMP_Text FindText(Transform root, string path)
        {
            var child = root != null ? root.Find(path) : null;
            return child != null ? child.GetComponent<TMP_Text>() : null;
        }

        /// 전달된 value 값을 사용해 Compact를 표시 또는 직렬화 형식으로 변환한다.
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
