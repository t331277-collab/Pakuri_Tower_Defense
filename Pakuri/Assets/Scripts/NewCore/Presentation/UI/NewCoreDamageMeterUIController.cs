using System;
using System.Collections.Generic;
using Pakuri.NewCore.Definitions.Skills;
using Pakuri.NewCore.Presentation.Scene;
using Pakuri.NewCore.Units.Models;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Pakuri.NewCore.Presentation.UI
{
    public sealed class NewCoreDamageMeterUIController : MonoBehaviour
    {
        private const int MaximumPartySlots = 5;
        private const float RefreshIntervalSeconds = 0.2f;

        private static readonly Color[] SegmentColors =
        {
            new Color(1f, 0.18f, 0.12f, 1f),
            new Color(0.12f, 0.35f, 1f, 1f),
            new Color(0.55f, 1f, 0.2f, 1f),
            new Color(0.25f, 0.82f, 1f, 1f),
            new Color(1f, 0.86f, 0.12f, 1f),
            new Color(0.62f, 0.25f, 1f, 1f),
            new Color(0.02f, 0.42f, 0.16f, 1f)
        };

        [SerializeField] private Button openButton;
        [SerializeField] private GameObject meterRoot;
        [SerializeField] private Button closeButton;
        [SerializeField] private NewCoreStageController stageManager;
        [SerializeField] private NewCoreSpawnController unitSpawnManager;
        [SerializeField] private NewCoreDamageMeterTracker tracker;

        private readonly Dictionary<Transform, List<RectTransform>> segments =
            new Dictionary<Transform, List<RectTransform>>();
        private readonly List<SortedSource> sortedSources =
            new List<SortedSource>();
        private NewCoreSceneRuntime runtime;
        private float refreshRemaining;
        private int lastTrackerVersion = -1;

        private void Awake()
        {
            ResolveReferences();
            Bind(openButton, Open);
            Bind(closeButton, Close);
            SetOverlayVisible(false);
        }

        private void Update()
        {
            if (meterRoot == null || !meterRoot.activeSelf)
            {
                return;
            }

            refreshRemaining -= Time.deltaTime;
            int version = tracker != null ? tracker.Version : -1;
            if (refreshRemaining <= 0f
                || version != lastTrackerVersion)
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
            if (runtime == null
                || runtime.Stage == null
                || tracker == null
                || meterRoot == null)
            {
                return;
            }

            IReadOnlyList<MonsterModel> party =
                runtime.Stage.Session.PartyRoster.Members;
            float leaderDamage = ResolveLeaderDamage(party);
            for (int index = 0; index < MaximumPartySlots; index++)
            {
                Transform panel = meterRoot.transform.Find(
                    $"{index + 1}PDamagePanel");
                if (panel == null)
                {
                    continue;
                }

                bool visible = index < party.Count;
                panel.gameObject.SetActive(visible);
                if (!visible)
                {
                    continue;
                }

                MonsterModel monster = party[index];
                tracker.TryGet(
                    monster.MonsterDefinition.id,
                    out DamageRecord record);
                float total = record != null
                    ? record.TotalDamage
                    : 0f;
                SetText(
                    panel,
                    "Monster_Name_Text",
                    monster.MonsterDefinition.display_name);
                SetText(panel, "Total_Damage", Format(total));
                SetText(
                    panel,
                    "Total_Damage_Persent",
                    leaderDamage > 0f
                        ? Mathf.RoundToInt(
                            Mathf.Clamp01(total / leaderDamage)
                            * 100f) + "%"
                        : "0%");
                SetPortrait(
                    panel,
                    monster.MonsterDefinition.MonsterIconImage);
                RenderSegments(
                    panel,
                    monster,
                    record,
                    leaderDamage);
            }

            refreshRemaining = RefreshIntervalSeconds;
            lastTrackerVersion = tracker.Version;
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
            }
        }

        private float ResolveLeaderDamage(
            IReadOnlyList<MonsterModel> party)
        {
            float result = 0f;
            for (int index = 0; index < party.Count; index++)
            {
                if (tracker.TryGet(
                        party[index].MonsterDefinition.id,
                        out DamageRecord record))
                {
                    result = Mathf.Max(
                        result,
                        record.TotalDamage);
                }
            }
            return result;
        }

        private void RenderSegments(
            Transform panel,
            MonsterModel monster,
            DamageRecord record,
            float leaderDamage)
        {
            RectTransform template =
                panel.Find("Skill-Meter") as RectTransform;
            RectTransform background =
                panel.Find("MeterBG") as RectTransform;
            if (template == null || background == null)
            {
                return;
            }

            BuildSortedSources(monster, record);
            List<RectTransform> panelSegments =
                ResolveSegments(panel, template);
            EnsureSegmentCount(
                panelSegments,
                template,
                sortedSources.Count);
            if (leaderDamage <= 0f || sortedSources.Count == 0)
            {
                SetSegmentCountActive(panelSegments, 0);
                return;
            }

            float totalWidth = background.rect.width > 0f
                ? background.rect.width
                : Mathf.Max(1f, template.rect.width);
            Vector2 templateSize = template.rect.size;
            if (templateSize.x <= 0f || templateSize.y <= 0f)
            {
                templateSize = template.sizeDelta;
            }
            Vector2 templatePosition = template.anchoredPosition;
            Vector2 anchorMin = template.anchorMin;
            Vector2 anchorMax = template.anchorMax;
            Vector2 pivot = template.pivot;
            float templateLeft =
                templatePosition.x - (templateSize.x * pivot.x);
            float cursor = 0f;
            for (int index = 0; index < panelSegments.Count; index++)
            {
                RectTransform segment = panelSegments[index];
                bool visible = index < sortedSources.Count;
                segment.gameObject.SetActive(visible);
                if (!visible)
                {
                    continue;
                }

                DamageSourceRecord source =
                    sortedSources[index].Record;
                float width = totalWidth * Mathf.Clamp01(
                    source.Damage / leaderDamage);
                width = Mathf.Min(
                    width,
                    Mathf.Max(0f, totalWidth - cursor));
                segment.anchorMin = anchorMin;
                segment.anchorMax = anchorMax;
                segment.pivot = pivot;
                segment.anchoredPosition = new Vector2(
                    templateLeft
                        + cursor
                        + (width * pivot.x),
                    templatePosition.y);
                segment.SetSizeWithCurrentAnchors(
                    RectTransform.Axis.Horizontal,
                    Mathf.Max(0f, width));
                segment.SetSizeWithCurrentAnchors(
                    RectTransform.Axis.Vertical,
                    templateSize.y);
                Image image = segment.GetComponent<Image>();
                if (image != null)
                {
                    image.color =
                        SegmentColors[index % SegmentColors.Length];
                }

                TMP_Text label =
                    segment.GetComponentInChildren<TMP_Text>(true);
                if (label != null)
                {
                    label.text =
                        ResolveDisplayName(source.SourceId)
                        + " "
                        + Format(source.Damage);
                }
                cursor += width;
            }
        }

        private void BuildSortedSources(
            MonsterModel monster,
            DamageRecord record)
        {
            sortedSources.Clear();
            if (record == null)
            {
                return;
            }

            for (int index = 0;
                index < record.OrderedSources.Count;
                index++)
            {
                DamageSourceRecord source =
                    record.OrderedSources[index];
                if (source.Damage <= 0f)
                {
                    continue;
                }

                sortedSources.Add(new SortedSource(
                    source,
                    ResolveSortKey(
                        monster,
                        source.SourceId,
                        index),
                    index));
            }

            sortedSources.Sort((left, right) =>
            {
                int sort = left.SortKey.CompareTo(
                    right.SortKey);
                return sort != 0
                    ? sort
                    : left.FirstSeenIndex.CompareTo(
                        right.FirstSeenIndex);
            });
        }

        private int ResolveSortKey(
            MonsterModel monster,
            string sourceId,
            int firstSeenIndex)
        {
            string baseId = ResolveBaseSourceId(sourceId);
            if (runtime.Catalog.Skills.TryGetValue(
                    baseId,
                    out SkillDefinition skill)
                && !(skill is PassiveDefinition)
                && skill.monster_id
                    == monster.MonsterDefinition.id
                && !string.IsNullOrEmpty(skill.slot)
                && skill.slot.Length == 1
                && skill.slot[0] >= 'A'
                && skill.slot[0] <= 'E')
            {
                return skill.slot[0] - 'A';
            }

            return 1000 + firstSeenIndex;
        }

        private string ResolveDisplayName(string sourceId)
        {
            string baseId = ResolveBaseSourceId(sourceId);
            if (runtime.Catalog.Skills.TryGetValue(
                    baseId,
                    out SkillDefinition skill))
            {
                return string.IsNullOrWhiteSpace(skill.display_name)
                    ? sourceId
                    : skill.display_name;
            }

            if (runtime.Catalog.Choices.TryGetValue(
                    sourceId,
                    out var choice))
            {
                return string.IsNullOrWhiteSpace(choice.title)
                    ? sourceId
                    : choice.title;
            }

            if (runtime.Catalog.Triggers.TryGetValue(
                    sourceId,
                    out SkillTriggerDefinition trigger))
            {
                string triggerSkillId =
                    string.IsNullOrEmpty(trigger.triggered_skill_id)
                        ? trigger.source_skill_id
                        : trigger.triggered_skill_id;
                if (runtime.Catalog.Skills.TryGetValue(
                        triggerSkillId,
                        out SkillDefinition triggerSkill)
                    && !string.IsNullOrWhiteSpace(
                        triggerSkill.display_name))
                {
                    return triggerSkill.display_name;
                }
            }

            return string.IsNullOrWhiteSpace(sourceId)
                ? "Unknown"
                : sourceId;
        }

        private static string ResolveBaseSourceId(
            string sourceId)
        {
            if (string.IsNullOrEmpty(sourceId))
            {
                return sourceId;
            }

            int separator = sourceId.IndexOf(
                ':',
                StringComparison.Ordinal);
            return separator > 0
                ? sourceId.Substring(0, separator)
                : sourceId;
        }

        private List<RectTransform> ResolveSegments(
            Transform panel,
            RectTransform template)
        {
            if (!segments.TryGetValue(
                    panel,
                    out List<RectTransform> result))
            {
                result = new List<RectTransform> { template };
                segments.Add(panel, result);
            }
            return result;
        }

        private static void EnsureSegmentCount(
            List<RectTransform> panelSegments,
            RectTransform template,
            int count)
        {
            while (panelSegments.Count < count)
            {
                RectTransform clone = Instantiate(
                    template,
                    template.parent);
                clone.name = template.name;
                panelSegments.Add(clone);
            }
        }

        private static void SetSegmentCountActive(
            IReadOnlyList<RectTransform> panelSegments,
            int count)
        {
            for (int index = 0;
                index < panelSegments.Count;
                index++)
            {
                panelSegments[index].gameObject.SetActive(
                    index < count);
            }
        }

        private void ResolveReferences()
        {
            if (runtime == null)
            {
                runtime = FindFirstObjectByType<NewCoreSceneRuntime>(
                    FindObjectsInactive.Include);
            }
            if (stageManager == null)
            {
                stageManager = FindFirstObjectByType<NewCoreStageController>(
                    FindObjectsInactive.Include);
            }
            if (unitSpawnManager == null)
            {
                unitSpawnManager =
                    FindFirstObjectByType<NewCoreSpawnController>(
                        FindObjectsInactive.Include);
            }
            if (tracker == null)
            {
                tracker = GetComponent<NewCoreDamageMeterTracker>();
            }
            if (openButton == null)
            {
                Transform target = transform.Find("DamageMeterUIBtn");
                openButton = target != null
                    ? target.GetComponent<Button>()
                    : null;
            }
            if (meterRoot == null)
            {
                Transform target = transform.Find("DamageMeterUI");
                meterRoot = target != null
                    ? target.gameObject
                    : null;
            }
            if (closeButton == null && meterRoot != null)
            {
                Transform target =
                    meterRoot.transform.Find("Close");
                closeButton = target != null
                    ? target.GetComponent<Button>()
                    : null;
            }
        }

        private void SetPortrait(
            Transform panel,
            string path)
        {
            Transform target = panel.Find("Image");
            Image image = target != null
                ? target.GetComponent<Image>()
                : null;
            if (image == null)
            {
                return;
            }

            if (runtime.RuntimeCatalog.TryGetSprite(
                    path,
                    out Sprite sprite))
            {
                image.sprite = sprite;
                image.enabled = true;
            }
            else
            {
                image.sprite = null;
                image.enabled = false;
            }
        }

        private static void SetText(
            Transform root,
            string path,
            string value)
        {
            Transform target = root.Find(path);
            TMP_Text text = target != null
                ? target.GetComponent<TMP_Text>()
                : null;
            if (text != null)
            {
                text.text = value;
            }
        }

        private static string Format(float value)
        {
            float clamped = Mathf.Max(0f, value);
            if (clamped >= 1000000f)
            {
                return clamped < 10000000f
                    ? (clamped / 1000000f).ToString("0.##") + "M"
                    : Mathf.RoundToInt(
                        clamped / 1000000f) + "M";
            }
            if (clamped >= 1000f)
            {
                return clamped < 100000f
                    ? (clamped / 1000f).ToString("0.#") + "K"
                    : Mathf.RoundToInt(
                        clamped / 1000f) + "K";
            }
            return Mathf.RoundToInt(clamped).ToString();
        }

        private static void Bind(
            Button button,
            UnityEngine.Events.UnityAction action)
        {
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(action);
            }
        }

        private readonly struct SortedSource
        {
            public SortedSource(
                DamageSourceRecord record,
                int sortKey,
                int firstSeenIndex)
            {
                Record = record;
                SortKey = sortKey;
                FirstSeenIndex = firstSeenIndex;
            }

            public DamageSourceRecord Record { get; }

            public int SortKey { get; }

            public int FirstSeenIndex { get; }
        }
    }
}
