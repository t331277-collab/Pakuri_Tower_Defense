using Pakuri.NewCore.Presentation.Scene;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Pakuri.NewCore.Presentation.UI
{
    public sealed class NewCoreDamageMeterUIController : MonoBehaviour
    {
        private const int MaximumPartySlots = 5;

        [SerializeField] private Button openButton;
        [SerializeField] private GameObject meterRoot;
        [SerializeField] private Button closeButton;
        [SerializeField] private NewCoreStageController stageManager;
        [SerializeField] private NewCoreSpawnController unitSpawnManager;
        [SerializeField] private NewCoreDamageMeterTracker tracker;

        private NewCoreSceneRuntime runtime;

        private void Awake()
        {
            ResolveReferences();
            Bind(openButton, Open);
            Bind(closeButton, Close);
            Close();
        }

        private void Update()
        {
            if (meterRoot != null && meterRoot.activeSelf)
            {
                RefreshNow();
            }
        }

        public void Open()
        {
            if (meterRoot != null)
            {
                meterRoot.SetActive(true);
            }

            RefreshNow();
        }

        public void Close()
        {
            if (meterRoot != null)
            {
                meterRoot.SetActive(false);
            }
        }

        public void RefreshNow()
        {
            if (runtime == null || runtime.Stage == null || tracker == null)
            {
                return;
            }

            var party = runtime.Stage.Session.PartyRoster.Members;
            var leaderDamage = 0f;
            for (var index = 0; index < party.Count; index++)
            {
                if (tracker.TryGet(
                        party[index].MonsterDefinition.id,
                        out var record))
                {
                    leaderDamage = Mathf.Max(
                        leaderDamage,
                        record.TotalDamage);
                }
            }

            for (var index = 0; index < MaximumPartySlots; index++)
            {
                var panel = meterRoot != null
                    ? meterRoot.transform.Find(
                        $"{index + 1}PDamagePanel")
                    : null;
                if (panel == null)
                {
                    continue;
                }

                var visible = index < party.Count;
                panel.gameObject.SetActive(visible);
                if (!visible)
                {
                    continue;
                }

                var monster = party[index];
                tracker.TryGet(
                    monster.MonsterDefinition.id,
                    out var record);
                var total = record != null ? record.TotalDamage : 0f;
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
                            total / leaderDamage * 100f) + "%"
                        : "0%");
                SetPortrait(panel, monster.MonsterDefinition.MonsterIconImage);
                SetMeter(panel, total, leaderDamage);
            }
        }

        private void ResolveReferences()
        {
            runtime = FindFirstObjectByType<NewCoreSceneRuntime>();
            if (stageManager == null)
            {
                stageManager =
                    FindFirstObjectByType<NewCoreStageController>();
            }

            if (unitSpawnManager == null)
            {
                unitSpawnManager =
                    FindFirstObjectByType<NewCoreSpawnController>();
            }

            if (tracker == null)
            {
                tracker = GetComponent<NewCoreDamageMeterTracker>();
            }

            if (openButton == null)
            {
                var target = transform.Find("DamageMeterUIBtn");
                openButton = target != null
                    ? target.GetComponent<Button>()
                    : null;
            }

            if (meterRoot == null)
            {
                var target = transform.Find("DamageMeterUI");
                meterRoot = target != null ? target.gameObject : null;
            }

            if (closeButton == null && meterRoot != null)
            {
                var target = meterRoot.transform.Find("Close");
                closeButton = target != null
                    ? target.GetComponent<Button>()
                    : null;
            }
        }

        private void SetPortrait(Transform panel, string path)
        {
            var target = panel.Find("Image");
            var image = target != null ? target.GetComponent<Image>() : null;
            if (image != null
                && runtime.RuntimeCatalog.TryGetSprite(
                    path,
                    out var sprite))
            {
                image.sprite = sprite;
                image.enabled = true;
            }
        }

        private static void SetMeter(
            Transform panel,
            float damage,
            float leader)
        {
            var meter = panel.Find("Skill-Meter") as RectTransform;
            var background = panel.Find("MeterBG") as RectTransform;
            if (meter == null || background == null)
            {
                return;
            }

            var ratio = leader > 0f
                ? Mathf.Clamp01(damage / leader)
                : 0f;
            meter.SetSizeWithCurrentAnchors(
                RectTransform.Axis.Horizontal,
                background.rect.width * ratio);
            meter.gameObject.SetActive(ratio > 0f);
            var label = meter.GetComponentInChildren<TMP_Text>(true);
            if (label != null)
            {
                label.text = "Total " + Format(damage);
            }
        }

        private static void SetText(
            Transform root,
            string path,
            string value)
        {
            var target = root.Find(path);
            var text = target != null
                ? target.GetComponent<TMP_Text>()
                : null;
            if (text != null)
            {
                text.text = value;
            }
        }

        private static string Format(float value)
        {
            return value >= 1000000f
                ? (value / 1000000f).ToString("0.#") + "M"
                : value >= 1000f
                    ? (value / 1000f).ToString("0.#") + "K"
                    : Mathf.RoundToInt(value).ToString();
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
    }
}
