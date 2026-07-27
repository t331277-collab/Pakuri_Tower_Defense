using System;
using System.Collections.Generic;
using Pakuri.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/*
 * DamageMeterRuntimeTracker의 기록을 파티원별 피해량 패널로 표현하는 UI 컴포넌트.
 * 파티 순서와 선두 피해량을 기준으로 표시 값을 계산하고
 * 스킬·패시브·트리거 출처 이름과 비율 구간을 카탈로그 정보로 구성한다.
 */
namespace Pakuri.InGame
{
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

        /*
         * Unity가 컴포넌트를 초기화할 때 필요한 참조와 상태를 준비한다.
         */
        private void Awake()
        {
            ResolveReferences();
            ResolveSceneUi();
            BindButtons();
            SetOverlayVisible(false);
        }

        /*
         * 컴포넌트가 활성화될 때 이벤트와 표시 상태를 연결한다.
         */
        private void OnEnable()
        {
            RefreshNow();
        }

        /*
         * 매 프레임 현재 상태를 갱신한다.
         */
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

        /*
         * Open 작업을 수행한다.
         */
        public void Open()
        {
            SetOverlayVisible(true);
            RefreshNow();
        }

        /*
         * Close 작업을 수행한다.
         */
        public void Close()
        {
            SetOverlayVisible(false);
        }

        /*
         * RefreshNow 대상의 현재 상태를 갱신한다.
         */
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

        /*
         * BuildPartyOrder에 필요한 결과를 구성한다.
         */
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

            if (unitSpawnManager != null
                && unitSpawnManager.SpawnedPlayerModel != null
                && unitSpawnManager.SpawnedPlayerModel.Identity != null)
            {
                partyMonsterIds[0] = unitSpawnManager.SpawnedPlayerModel.Identity.DefinitionId;
            }
        }

        /*
         * ResolveLeaderDamage 결과를 계산해 반환한다.
         */
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

        /*
         * ResolveDisplayName 결과를 계산해 반환한다.
         */
        private string ResolveDisplayName(string monsterId /* 몬스터 식별자 */, string sourceId /* 효과를 발생시킨 대상의 식별자 */)
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

            var choice = manager.GetData<SkillChoiceDefinition>(sourceId);
            if (choice != null && !string.IsNullOrWhiteSpace(choice.Title))
            {
                return choice.Title;
            }

            // 저장된 별칭이 없으면 sourceId를 표시명으로 사용한다.
            return string.IsNullOrWhiteSpace(sourceId) ? "Unknown" : sourceId;
        }

        /*
         * ResolveSortKey 결과를 계산해 반환한다.
         */
        private int ResolveSortKey(string monsterId /* 몬스터 식별자 */, string sourceId /* 효과를 발생시킨 대상의 식별자 */, int firstSeenIndex /* 첫 번째 처음 발견 순서 번호 */)
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

        /*
         * ResolveTriggerSourceDisplayName 결과를 계산해 반환한다.
         */
        private static string ResolveTriggerSourceDisplayName(MonsterDefinition monster /* 몬스터 */, string sourceId /* 효과를 발생시킨 대상의 식별자 */)
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

        /*
         * ResolveChoiceTitleForSource 결과를 계산해 반환한다.
         */
        private static string ResolveChoiceTitleForSource(MonsterDefinition monster /* 몬스터 */, string sourceId /* 효과를 발생시킨 대상의 식별자 */)
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

        /*
         * ResolveChoiceTitle 결과를 계산해 반환한다.
         */
        private static string ResolveChoiceTitle(string choiceId /* 스킬 선택지 식별자 */)
        {
            var choice = GameDataLoader.CurrentCatalog.GetData<SkillChoiceDefinition>(choiceId);
            return choice != null ? choice.Title : string.Empty;
        }

        /*
         * ResolveActiveSkillDisplayName 결과를 계산해 반환한다.
         */
        private static string ResolveActiveSkillDisplayName(MonsterDefinition monster /* 몬스터 */, string sourceId /* 효과를 발생시킨 대상의 식별자 */)
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

        /*
         * ResolvePassiveDisplayName 결과를 계산해 반환한다.
         */
        private static string ResolvePassiveDisplayName(MonsterDefinition monster /* 몬스터 */, string sourceId /* 효과를 발생시킨 대상의 식별자 */)
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

        /*
         * SetOverlayVisible에 필요한 값을 설정한다.
         */
        private void SetOverlayVisible(bool visible /* 화면 표시 여부 */)
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

        /*
         * BindButtons에 필요한 값을 설정한다.
         */
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

        /*
         * ResolveReferences에 필요한 값을 계산해 현재 상태에 반영한다.
         */
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

        /*
         * ResolveSceneUi에 필요한 값을 계산해 현재 상태에 반영한다.
         */
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

        /*
         * ResolveCatalog 결과를 계산해 반환한다.
         */
        private GameDataCatalog ResolveCatalog()
        {
            return GameDataLoader.CurrentCatalog;
        }

        /*
         * FindSceneObject에 해당하는 값을 찾아 반환한다.
         */
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

            /*
             * DamagePanelView에 필요한 값을 초기화한다.
             */
            public DamagePanelView(Transform panelRoot /* 패널 기준 오브젝트 */)
            {
                Bind(panelRoot);
            }

            /*
             * Bind에 필요한 값을 설정한다.
             */
            public void Bind(Transform panelRoot /* 패널 기준 오브젝트 */)
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

            /*
             * SetRuntime에 필요한 값을 설정한다.
             */
            public void SetRuntime(
                MonsterDefinition monster /* 몬스터 */,
                MonsterDamageRecord record /* 읽거나 갱신할 기록 */,
                float leaderDamage /* 선두 피해 */,
                Func<string, string, string> displayNameResolver /* 표시 이름 조회 함수 */,
                Func<string, string, int, int> sortKeyResolver /* 정렬 조회 키 조회 함수 */)
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

            /*
             * SetVisible에 필요한 값을 설정한다.
             */
            public void SetVisible(bool visible /* 화면 표시 여부 */)
            {
                if (root != null)
                {
                    root.SetActive(visible);
                }
            }

            /*
             * RefreshImage 대상의 현재 상태를 갱신한다.
             */
            private void RefreshImage(MonsterDefinition monster /* 몬스터 */)
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

            /*
             * RefreshSegments 대상의 현재 상태를 갱신한다.
             */
            private void RefreshSegments(
                string monsterId /* 몬스터 식별자 */,
                MonsterDamageRecord record /* 읽거나 갱신할 기록 */,
                float leaderDamage /* 선두 피해 */,
                Func<string, string, string> displayNameResolver /* 표시 이름 조회 함수 */,
                Func<string, string, int, int> sortKeyResolver /* 정렬 조회 키 조회 함수 */)
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

            /*
             * BuildVisibleSources에 필요한 결과를 만들어 반환한다.
             */
            private List<SortedSkillSource> BuildVisibleSources(
                MonsterDamageRecord record /* 읽거나 갱신할 기록 */,
                string monsterId /* 몬스터 식별자 */,
                Func<string, string, int, int> sortKeyResolver /* 정렬 조회 키 조회 함수 */)
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

            /*
             * EnsureSegmentCount에 필요한 상태가 준비되어 있는지 확인하고 구성한다.
             */
            private void EnsureSegmentCount(int count /* 처리할 개수 */)
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

            /*
             * SetSegmentCountActive에 필요한 값을 설정한다.
             */
            private void SetSegmentCountActive(int count /* 처리할 개수 */)
            {
                for (var i = 0; i < segments.Count; i++)
                {
                    if (segments[i] != null)
                    {
                        segments[i].gameObject.SetActive(i < count);
                    }
                }
            }

            /*
             * ResolveMeterWidth 결과를 계산해 반환한다.
             */
            private float ResolveMeterWidth()
            {
                if (meterBackground != null && meterBackground.rect.width > 0f)
                {
                    return meterBackground.rect.width;
                }

                return templateSize.x > 0f ? templateSize.x : 1f;
            }

            /*
             * ConfigureSegment에 필요한 값을 설정한다.
             */
            private void ConfigureSegment(RectTransform segment /* 구간 */, float xOffset /* X축 위치 보정 */, float width /* 너비 */)
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

            /*
             * ApplySegmentColor 처리를 대상에 적용한다.
             */
            private static void ApplySegmentColor(RectTransform segment /* 구간 */, int index /* 목록에서의 순서 번호 */)
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

            /*
             * ResolveChildren에 필요한 값을 계산해 현재 상태에 반영한다.
             */
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
            /*
             * SortedSkillSource에 필요한 값을 초기화한다.
             */
            public SortedSkillSource(SkillDamageRecord record /* 읽거나 갱신할 기록 */, int sortKey /* 정렬 조회 키 */, int firstSeenIndex /* 첫 번째 처음 발견 순서 번호 */)
            {
                Record = record;
                SortKey = sortKey;
                FirstSeenIndex = firstSeenIndex;
            }

            public SkillDamageRecord Record { get; }
            public int SortKey { get; }
            public int FirstSeenIndex { get; }
        }

        /*
         * FindImage에 해당하는 값을 찾아 반환한다.
         */
        private static Image FindImage(Transform root /* 검색이나 배치의 기준 오브젝트 */, string path /* 불러오거나 검사할 경로 */)
        {
            var child = root != null ? root.Find(path) : null;
            return child != null ? child.GetComponent<Image>() : null;
        }

        /*
         * FindText에 해당하는 값을 찾아 반환한다.
         */
        private static TMP_Text FindText(Transform root /* 검색이나 배치의 기준 오브젝트 */, string path /* 불러오거나 검사할 경로 */)
        {
            var child = root != null ? root.Find(path) : null;
            return child != null ? child.GetComponent<TMP_Text>() : null;
        }

        /*
         * FormatCompact에 맞는 문자열을 만들어 반환한다.
         */
        private static string FormatCompact(float value /* 처리할 값 */)
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
