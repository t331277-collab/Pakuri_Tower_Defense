using Pakuri.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Pakuri.InGame
{
    [DisallowMultipleComponent]
    public sealed class MonsterPanelUI : MonoBehaviour
    {
        private const int MaxVisibleActiveSlots = 3;

        [SerializeField] private Transform monsterPanelRoot;
        [SerializeField] private Transform selectedMonsterRoot;
        [SerializeField] private Image monsterImage;
        [SerializeField] private ActiveSkillSlotView[] activeSlots = new ActiveSkillSlotView[MaxVisibleActiveSlots];
        [SerializeField] private StageManager stageManager;
        [SerializeField] private SceneEntryManager entryManager;
        [SerializeField] private InGameCombatManager combatManager;

        private string lastMonsterId;

        private void Awake()
        {
            ResolveReferences();
            ResolveSceneUi();
            SetPanelVisible(true);
        }

        private void OnEnable()
        {
            RefreshNow();
        }

        private void Update()
        {
            ResolveReferences();
            RefreshNow();
        }

        public void RefreshNow()
        {
            ResolveReferences();
            ResolveSceneUi();
            SetPanelVisible(true);

            var entry = ResolveSelectedPlayerEntry();
            var model = entry != null ? entry.Model as MonsterUnitRuntimeModel : null;
            if (model == null && entryManager != null)
            {
                model = entryManager.SpawnedPlayerModel;
            }

            var monsterId = model != null && model.Identity != null ? model.Identity.DefinitionId : string.Empty;
            if (string.IsNullOrWhiteSpace(monsterId))
            {
                SetSlotsActive(0);
                return;
            }

            if (selectedMonsterRoot != null)
            {
                selectedMonsterRoot.gameObject.SetActive(true);
            }

            RefreshMonsterImage(monsterId);
            RefreshActiveSlots(model);
        }

        private void RefreshMonsterImage(string monsterId)
        {
            if (monsterImage == null || string.Equals(lastMonsterId, monsterId, System.StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            lastMonsterId = monsterId;
            var monster = PakuriDataManager.Instance.ResolveMonster(monsterId, ResolveCatalog());
            if (monster != null && monster.UnitSprite != null)
            {
                monsterImage.sprite = monster.UnitSprite;
                monsterImage.enabled = true;
            }
        }

        private void RefreshActiveSlots(MonsterUnitRuntimeModel model)
        {
            var runtimes = model != null && model.SkillRuntime != null ? model.SkillRuntime.ActiveSkills : null;
            var runtimeCount = runtimes != null ? runtimes.Count : 0;
            for (var i = 0; i < activeSlots.Length && i < MaxVisibleActiveSlots; i++)
            {
                var view = activeSlots[i];
                if (view == null)
                {
                    continue;
                }

                var runtime = i < runtimeCount ? runtimes[i] : null;
                view.SetRuntime(runtime);
            }
        }

        private void SetSlotsActive(int count)
        {
            for (var i = 0; i < activeSlots.Length; i++)
            {
                if (activeSlots[i] != null)
                {
                    activeSlots[i].SetVisible(i < count);
                }
            }
        }

        private void ResolveReferences()
        {
            if (stageManager == null)
            {
                stageManager = FindSceneObject<StageManager>();
            }

            if (entryManager == null)
            {
                entryManager = FindSceneObject<SceneEntryManager>();
            }

            if (combatManager == null)
            {
                combatManager = FindSceneObject<InGameCombatManager>();
            }
        }

        private void ResolveSceneUi()
        {
            if (monsterPanelRoot == null)
            {
                monsterPanelRoot = string.Equals(transform.name, "MonsterPanel", System.StringComparison.OrdinalIgnoreCase)
                    ? transform
                    : transform.Find("MonsterPanel");
            }

            if (monsterPanelRoot == null)
            {
                return;
            }

            monsterPanelRoot.gameObject.SetActive(true);

            if (selectedMonsterRoot == null)
            {
                selectedMonsterRoot = monsterPanelRoot.Find("1PMonster");
            }

            if (selectedMonsterRoot == null)
            {
                return;
            }

            selectedMonsterRoot.gameObject.SetActive(true);

            if (monsterImage == null)
            {
                monsterImage = FindImage(selectedMonsterRoot, "Monster Image");
            }

            EnsureSlotArray();
            ResolveSlot(0, "Active1");
            ResolveSlot(1, "Active2");
            ResolveSlot(2, "Active3");
        }

        private void ResolveSlot(int index, string childName)
        {
            if (index < 0 || index >= activeSlots.Length || selectedMonsterRoot == null)
            {
                return;
            }

            var existing = activeSlots[index];
            if (existing != null && existing.IsBound)
            {
                return;
            }

            var slotRoot = selectedMonsterRoot.Find(childName);
            if (slotRoot == null)
            {
                return;
            }

            if (existing == null)
            {
                activeSlots[index] = new ActiveSkillSlotView(slotRoot.gameObject);
                return;
            }

            existing.Bind(slotRoot.gameObject);
        }

        private void EnsureSlotArray()
        {
            if (activeSlots == null || activeSlots.Length != MaxVisibleActiveSlots)
            {
                activeSlots = new ActiveSkillSlotView[MaxVisibleActiveSlots];
            }
        }

        private UnitRosterEntry ResolveSelectedPlayerEntry()
        {
            var manager = combatManager;
            return manager != null && manager.Roster.Players.Count > 0 ? manager.Roster.Players[0] : null;
        }

        private GameDataCatalog ResolveCatalog()
        {
            var catalog = PakuriDataManager.Instance.CurrentCatalog;
            return catalog != null ? catalog : PakuriCsvRuntimeData.ResolveCatalogOrFallback(null);
        }

        private static Image FindImage(Transform root, string path)
        {
            var child = root != null ? root.Find(path) : null;
            return child != null ? child.GetComponent<Image>() : null;
        }

        private void SetPanelVisible(bool visible)
        {
            if (monsterPanelRoot != null)
            {
                monsterPanelRoot.gameObject.SetActive(visible);
            }

            if (selectedMonsterRoot != null)
            {
                selectedMonsterRoot.gameObject.SetActive(visible);
            }
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

        [System.Serializable]
        private sealed class ActiveSkillSlotView
        {
            [SerializeField] private GameObject root;
            [SerializeField] private Image skillImage;
            [SerializeField] private Image cooldownOverlay;
            [SerializeField] private TMP_Text label;

            public bool IsBound => root != null;

            public ActiveSkillSlotView(GameObject root)
            {
                Bind(root);
            }

            public void Bind(GameObject root)
            {
                this.root = root;
                skillImage = null;
                cooldownOverlay = null;
                label = null;
                ResolveChildren();
            }

            public void SetRuntime(SkillRuntimeInstance runtime)
            {
                ResolveChildren();

                if (runtime == null || runtime.Data == null)
                {
                    SetVisible(false);
                    return;
                }

                SetVisible(true);
                if (skillImage != null && runtime.Data.Icon != null)
                {
                    skillImage.sprite = runtime.Data.Icon;
                }

                RefreshLabel(runtime);
                RefreshCooldownOverlay(runtime);
            }

            public void SetVisible(bool visible)
            {
                if (root != null)
                {
                    root.SetActive(visible);
                }
            }

            private void RefreshLabel(SkillRuntimeInstance runtime)
            {
                if (label == null)
                {
                    return;
                }

                if (runtime.UsesMagazine)
                {
                    label.gameObject.SetActive(true);
                    label.text = string.Format("{0}/{1}", Mathf.Max(0, runtime.MagazineRemaining), Mathf.Max(0, runtime.MaxMagazineSize));
                    return;
                }

                label.text = string.Empty;
                label.gameObject.SetActive(false);
            }

            private void RefreshCooldownOverlay(SkillRuntimeInstance runtime)
            {
                if (cooldownOverlay == null)
                {
                    return;
                }

                cooldownOverlay.type = Image.Type.Filled;
                cooldownOverlay.fillMethod = Image.FillMethod.Vertical;
                cooldownOverlay.fillOrigin = (int)Image.OriginVertical.Top;

                var remaining = 0f;
                var duration = 0f;
                if (runtime.IsReloading)
                {
                    remaining = runtime.ReloadRemaining;
                    duration = runtime.ReloadDuration;
                }
                else if (runtime.CooldownRemaining > 0f)
                {
                    remaining = runtime.CooldownRemaining;
                    duration = runtime.Data != null && runtime.Data.Timing != null ? runtime.Data.Timing.Cooldown : 0f;
                }

                if (duration <= 0f || remaining <= 0f)
                {
                    cooldownOverlay.fillAmount = 0f;
                    cooldownOverlay.gameObject.SetActive(false);
                    return;
                }

                var remainingRatio = Mathf.Clamp01(remaining / duration);
                cooldownOverlay.gameObject.SetActive(true);
                cooldownOverlay.fillAmount = remainingRatio;
            }

            private void ResolveChildren()
            {
                if (root == null)
                {
                    return;
                }

                if (skillImage == null)
                {
                    skillImage = root.GetComponent<Image>();
                }

                if (cooldownOverlay == null)
                {
                    var overlay = root.transform.Find("CooldownOverlay");
                    cooldownOverlay = overlay != null ? overlay.GetComponent<Image>() : null;
                }

                if (label == null)
                {
                    label = root.GetComponentInChildren<TMP_Text>(true);
                }
            }
        }
    }
}
