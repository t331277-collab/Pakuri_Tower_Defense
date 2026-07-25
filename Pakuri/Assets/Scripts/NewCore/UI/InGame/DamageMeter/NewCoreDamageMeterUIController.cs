using System;
using System.Collections.Generic;
using Pakuri.NewCore.Bootstrap;
using Pakuri.NewCore.Definitions.Skills;
using Pakuri.NewCore.Run;
using Pakuri.NewCore.Spawn;
using Pakuri.NewCore.Units.Models;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/* party 피해 합계와 skill source 구간을 authored DamageMeter panel에 표시한다. */
namespace Pakuri.NewCore.UI.InGame.DamageMeter
{
    public class NewCoreDamageMeterUIController : MonoBehaviour
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
        [SerializeField] private StageManager stageManager;
        [SerializeField] private SpawnManager unitSpawnManager;
        [SerializeField] private NewCoreDamageMeterTracker tracker;

        private readonly Dictionary<Transform, List<RectTransform>> segments =
            new Dictionary<Transform, List<RectTransform>>();
        private readonly List<SortedSource> sortedSources =
            new List<SortedSource>();
        private GameBootstrap runtime;
        private float refreshRemaining;
        private int lastTrackerVersion = -1;

        /* meter 참조와 button command를 연결하고 overlay를 닫는다. */
        private void Awake()
        {
            ResolveReferences();
            Bind(openButton, Open);
            Bind(closeButton, Close);
            SetOverlayVisible(false);
        }

        /* 열린 meter를 주기 또는 tracker version 변경에 맞춰 갱신한다. */
        private void Update()
        {
            if (meterRoot == null || !meterRoot.activeSelf)
            {
                return;
            }

            refreshRemaining -= Time.deltaTime;
            int version = -1;
            if (tracker != null)
            {
                version = tracker.Version;
            }

            if (refreshRemaining <= 0f
                || version != lastTrackerVersion)
            {
                RefreshNow();
            }
        }

        /* meter overlay를 열고 즉시 현재 피해를 표시한다. */
        public void Open()
        {
            SetOverlayVisible(true);
            RefreshNow();
        }

        /* meter overlay를 닫는다. */
        public void Close()
        {
            SetOverlayVisible(false);
        }

        /* party별 총 피해와 source 구간을 현재 tracker 상태로 그린다. */
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
                float total = 0f;
                if (record != null)
                {
                    total = record.TotalDamage;
                }

                string percentage = "0%";
                if (leaderDamage > 0f)
                {
                    percentage = Mathf.RoundToInt(
                        Mathf.Clamp01(total / leaderDamage)
                        * 100f) + "%";
                }

                SetText(
                    panel,
                    "Monster_Name_Text",
                    monster.MonsterDefinition.display_name);
                SetText(panel, "Total_Damage", Format(total));
                SetText(
                    panel,
                    "Total_Damage_Persent",
                    percentage);
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

        /* meter root와 open button의 상호 배타적 표시 상태를 바꾼다. */
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

        /* party에서 가장 높은 Monster 누적 피해를 구한다. */
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

        /* 한 Monster의 source 피해를 비율 막대 구간으로 그린다. */
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

            float totalWidth = Mathf.Max(
                1f,
                template.rect.width);
            if (background.rect.width > 0f)
            {
                totalWidth = background.rect.width;
            }

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

        /* 피해 source를 authored skill slot 우선순위로 정렬한다. */
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
                if (sort != 0)
                {
                    return sort;
                }

                return left.FirstSeenIndex.CompareTo(
                    right.FirstSeenIndex);
            });
        }

        /* source에 대응하는 active skill slot 또는 후순위 key를 구한다. */
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

        /* source id를 사용자 표시용 skill 또는 choice 이름으로 변환한다. */
        private string ResolveDisplayName(string sourceId)
        {
            string baseId = ResolveBaseSourceId(sourceId);
            if (runtime.Catalog.Skills.TryGetValue(
                    baseId,
                    out SkillDefinition skill))
            {
                if (string.IsNullOrWhiteSpace(skill.display_name))
                {
                    return sourceId;
                }

                return skill.display_name;
            }

            if (runtime.Catalog.Choices.TryGetValue(
                    sourceId,
                    out var choice))
            {
                if (string.IsNullOrWhiteSpace(choice.title))
                {
                    return sourceId;
                }

                return choice.title;
            }

            if (runtime.Catalog.Triggers.TryGetValue(
                    sourceId,
                    out SkillTriggerDefinition trigger))
            {
                string triggerSkillId =
                    trigger.triggered_skill_id;
                if (string.IsNullOrEmpty(triggerSkillId))
                {
                    triggerSkillId = trigger.source_skill_id;
                }

                if (runtime.Catalog.Skills.TryGetValue(
                        triggerSkillId,
                        out SkillDefinition triggerSkill)
                    && !string.IsNullOrWhiteSpace(
                        triggerSkill.display_name))
                {
                    return triggerSkill.display_name;
                }
            }

            if (string.IsNullOrWhiteSpace(sourceId))
            {
                return "Unknown";
            }

            return sourceId;
        }

        /* 파생 source id의 구분자 앞 Base skill id를 반환한다. */
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
            if (separator > 0)
            {
                return sourceId.Substring(0, separator);
            }

            return sourceId;
        }

        /* panel에 속한 재사용 meter segment 목록을 구한다. */
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

        /* 필요한 source 수만큼 template segment를 복제한다. */
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

        /* 요청 수만큼 segment만 활성화한다. */
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

        /* authored field가 비어 있으면 scene hierarchy에서 meter 참조를 찾는다. */
        private void ResolveReferences()
        {
            if (runtime == null)
            {
                runtime = FindFirstObjectByType<GameBootstrap>(
                    FindObjectsInactive.Include);
            }
            if (stageManager == null)
            {
                stageManager = FindFirstObjectByType<StageManager>(
                    FindObjectsInactive.Include);
            }
            if (unitSpawnManager == null)
            {
                unitSpawnManager =
                    FindFirstObjectByType<SpawnManager>(
                        FindObjectsInactive.Include);
            }
            if (tracker == null)
            {
                tracker = GetComponent<NewCoreDamageMeterTracker>();
            }
            if (openButton == null)
            {
                Transform target = transform.Find("DamageMeterUIBtn");
                if (target != null)
                {
                    openButton = target.GetComponent<Button>();
                }
            }
            if (meterRoot == null)
            {
                Transform target = transform.Find("DamageMeterUI");
                if (target != null)
                {
                    meterRoot = target.gameObject;
                }
            }
            if (closeButton == null && meterRoot != null)
            {
                Transform target =
                    meterRoot.transform.Find("Close");
                if (target != null)
                {
                    closeButton = target.GetComponent<Button>();
                }
            }
        }

        /* runtime catalog sprite로 Monster portrait를 갱신한다. */
        private void SetPortrait(
            Transform panel,
            string path)
        {
            Transform target = panel.Find("Image");
            Image image = null;
            if (target != null)
            {
                image = target.GetComponent<Image>();
            }

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

        /* 지정 자식 경로의 TMP label을 갱신한다. */
        private static void SetText(
            Transform root,
            string path,
            string value)
        {
            Transform target = root.Find(path);
            TMP_Text text = null;
            if (target != null)
            {
                text = target.GetComponent<TMP_Text>();
            }

            if (text != null)
            {
                text.text = value;
            }
        }

        /* 피해량을 K 또는 M 단위의 짧은 표시 문자열로 변환한다. */
        private static string Format(float value)
        {
            float clamped = Mathf.Max(0f, value);
            if (clamped >= 1000000f)
            {
                if (clamped < 10000000f)
                {
                    return (clamped / 1000000f)
                        .ToString("0.##") + "M";
                }

                return Mathf.RoundToInt(
                    clamped / 1000000f) + "M";
            }
            if (clamped >= 1000f)
            {
                if (clamped < 100000f)
                {
                    return (clamped / 1000f)
                        .ToString("0.#") + "K";
                }

                return Mathf.RoundToInt(
                    clamped / 1000f) + "K";
            }
            return Mathf.RoundToInt(clamped).ToString();
        }

        /* Button의 기존 listener를 정리하고 meter command를 연결한다. */
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
            /* 정렬할 피해 source와 안정적 순서 key를 묶는다. */
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
