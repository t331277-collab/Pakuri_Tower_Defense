using Pakuri.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/*
 * 전투 중 파티 몬스터의 초상화와 체력, 활성 스킬 상태를 표시하는 컴포넌트.
 */
namespace Pakuri.InGame
{
    public class MonsterPanelUI : MonoBehaviour
    {
        private const int MaxPartySlots = 5;
        private const int MaxVisibleActiveSlots = 3;

        [SerializeField] private Transform monsterPanelRoot;
        [SerializeField] private MonsterPanelSlotView[] monsterSlots = new MonsterPanelSlotView[MaxPartySlots];
        [SerializeField] private StageManager stageManager;
        [SerializeField] private UnitSpawnManager unitSpawnManager;
        [SerializeField] private InGameCombatManager combatManager;

        /*
         * Unity가 컴포넌트를 초기화할 때 필요한 참조와 상태를 준비한다.
         */
        private void Awake()
        {
            ResolveReferences();
            ResolveSceneUi();
            SetPanelVisible(true);
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
            RefreshNow();
        }

        /*
         * RefreshNow 대상의 현재 상태를 갱신한다.
         */
        public void RefreshNow()
        {
            ResolveReferences();
            ResolveSceneUi();
            SetPanelVisible(true);

            var modelsBySlot = ResolvePlayerModelsBySlot();
            var catalog = ResolveCatalog();
            for (var i = 0; i < monsterSlots.Length; i++)
            {
                var slotView = monsterSlots[i];
                if (slotView == null)
                {
                    continue;
                }

                slotView.SetRuntime(modelsBySlot[i], catalog);
            }
        }

        /*
         * ResolvePlayerModelsBySlot 결과를 계산해 반환한다.
         */
        private UnitCombatState[] ResolvePlayerModelsBySlot()
        {
            var models = new UnitCombatState[MaxPartySlots];
            System.Collections.Generic.IReadOnlyList<CombatUnitEntry> players = null;
            if (combatManager != null && combatManager.UnitRegistry != null)
            {
                players = combatManager.UnitRegistry.Players;
            }

            if (players != null)
            {
                for (var i = 0; i < players.Count; i++)
                {
                    var entry = players[i];
                    if (entry == null)
                    {
                        continue;
                    }

                    var model = entry.Model;
                    var identity = model.Identity;
                    if (identity.Side != UnitSide.Player || identity.Role != UnitRole.Monster)
                    {
                        continue;
                    }

                    var slotIndex = identity.SlotIndex;
                    if (slotIndex < 0 || slotIndex >= models.Length)
                    {
                        continue;
                    }

                    models[slotIndex] = model;
                }
            }

            if (models[0] == null && unitSpawnManager != null)
            {
                models[0] = unitSpawnManager.SpawnedPlayerModel;
            }

            return models;
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

            if (combatManager == null)
            {
                combatManager = FindSceneObject<InGameCombatManager>();
            }
        }

        /*
         * ResolveSceneUi에 필요한 값을 계산해 현재 상태에 반영한다.
         */
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
            EnsureMonsterSlotArray();
            for (var i = 0; i < monsterSlots.Length; i++)
            {
                ResolveMonsterSlot(i);
            }
        }

        /*
         * ResolveMonsterSlot에 필요한 값을 계산해 현재 상태에 반영한다.
         */
        private void ResolveMonsterSlot(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= monsterSlots.Length || monsterPanelRoot == null)
            {
                return;
            }

            var existing = monsterSlots[slotIndex];
            if (existing != null && existing.IsBound)
            {
                return;
            }

            var slotRoot = monsterPanelRoot.Find(string.Format("{0}PMonster", slotIndex + 1));
            if (slotRoot == null)
            {
                return;
            }

            if (existing == null)
            {
                monsterSlots[slotIndex] = new MonsterPanelSlotView(slotRoot);
                return;
            }

            existing.Bind(slotRoot);
        }

        /*
         * EnsureMonsterSlotArray에 필요한 상태가 준비되어 있는지 확인하고 구성한다.
         */
        private void EnsureMonsterSlotArray()
        {
            if (monsterSlots == null || monsterSlots.Length != MaxPartySlots)
            {
                monsterSlots = new MonsterPanelSlotView[MaxPartySlots];
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
         * FindImage에 해당하는 값을 찾아 반환한다.
         */
        private static Image FindImage(Transform root, string path)
        {
            var child = root != null ? root.Find(path) : null;
            return child != null ? child.GetComponent<Image>() : null;
        }

        /*
         * SetPanelVisible에 필요한 값을 설정한다.
         */
        private void SetPanelVisible(bool visible)
        {
            if (monsterPanelRoot != null)
            {
                monsterPanelRoot.gameObject.SetActive(visible);
            }
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

        [System.Serializable]
        private class MonsterPanelSlotView
        {
            [SerializeField] private GameObject root;
            [SerializeField] private Image monsterImage;
            [SerializeField] private ActiveSkillSlotView[] activeSlots = new ActiveSkillSlotView[MaxVisibleActiveSlots];

            private string lastMonsterId;

            public bool IsBound => root != null;

            /*
             * MonsterPanelSlotView에 필요한 값을 초기화한다.
             */
            public MonsterPanelSlotView(Transform rootTransform)
            {
                Bind(rootTransform);
            }

            /*
             * Bind에 필요한 값을 설정한다.
             */
            public void Bind(Transform rootTransform)
            {
                root = rootTransform != null ? rootTransform.gameObject : null;
                monsterImage = null;
                activeSlots = new ActiveSkillSlotView[MaxVisibleActiveSlots];
                lastMonsterId = string.Empty;
                ResolveChildren();
            }

            /*
             * SetRuntime에 필요한 값을 설정한다.
             */
            public void SetRuntime(UnitCombatState model, GameDataCatalog catalog)
            {
                ResolveChildren();
                if (root == null)
                {
                    return;
                }

                var monsterId = string.Empty;
                if (model != null)
                {
                    monsterId = model.Identity.DefinitionId;
                }
                if (string.IsNullOrWhiteSpace(monsterId))
                {
                    SetVisible(false);
                    SetSlotsActive(0);
                    return;
                }

                SetVisible(true);
                RefreshMonsterImage(monsterId, catalog);
                RefreshActiveSlots(model);
            }

            /*
             * RefreshMonsterImage 대상의 현재 상태를 갱신한다.
             */
            private void RefreshMonsterImage(string monsterId, GameDataCatalog catalog)
            {
                if (monsterImage == null || string.Equals(lastMonsterId, monsterId, System.StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                lastMonsterId = monsterId;
                var monster = GameDataLoader.CurrentCatalog.ResolveMonster(monsterId);
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
             * RefreshActiveSlots 대상의 현재 상태를 갱신한다.
             */
            private void RefreshActiveSlots(UnitCombatState model)
            {
                System.Collections.Generic.IReadOnlyList<SkillRuntimeInstance> runtimes = null;
                if (model != null && model.SkillRuntime != null)
                {
                    runtimes = model.SkillRuntime.ActiveSkills;
                }

                var runtimeCount = 0;
                if (runtimes != null)
                {
                    runtimeCount = runtimes.Count;
                }

                for (var i = 0; i < activeSlots.Length && i < MaxVisibleActiveSlots; i++)
                {
                    var view = activeSlots[i];
                    if (view == null)
                    {
                        continue;
                    }

                    SkillRuntimeInstance runtime = null;
                    if (i < runtimeCount)
                    {
                        runtime = runtimes[i];
                    }
                    view.SetRuntime(runtime);
                }
            }

            /*
             * SetSlotsActive에 필요한 값을 설정한다.
             */
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

            /*
             * SetVisible에 필요한 값을 설정한다.
             */
            private void SetVisible(bool visible)
            {
                if (root != null)
                {
                    root.SetActive(visible);
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
                    monsterImage = FindImage(root.transform, "Monster Image");
                }

                EnsureSlotArray();
                ResolveSlot(0, "Active1");
                ResolveSlot(1, "Active2");
                ResolveSlot(2, "Active3");
            }

            /*
             * ResolveSlot에 필요한 값을 계산해 현재 상태에 반영한다.
             */
            private void ResolveSlot(int index, string childName)
            {
                if (index < 0 || index >= activeSlots.Length || root == null)
                {
                    return;
                }

                var existing = activeSlots[index];
                if (existing != null && existing.IsBound)
                {
                    return;
                }

                var slotRoot = root.transform.Find(childName);
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

            /*
             * EnsureSlotArray에 필요한 상태가 준비되어 있는지 확인하고 구성한다.
             */
            private void EnsureSlotArray()
            {
                if (activeSlots == null || activeSlots.Length != MaxVisibleActiveSlots)
                {
                    activeSlots = new ActiveSkillSlotView[MaxVisibleActiveSlots];
                }
            }
        }

        [System.Serializable]
        private class ActiveSkillSlotView
        {
            [SerializeField] private GameObject root;
            [SerializeField] private Image skillImage;
            [SerializeField] private Image cooldownOverlay;
            [SerializeField] private TMP_Text label;

            public bool IsBound => root != null;

            /*
             * ActiveSkillSlotView에 필요한 값을 초기화한다.
             */
            public ActiveSkillSlotView(GameObject root)
            {
                Bind(root);
            }

            /*
             * Bind에 필요한 값을 설정한다.
             */
            public void Bind(GameObject root)
            {
                this.root = root;
                skillImage = null;
                cooldownOverlay = null;
                label = null;
                ResolveChildren();
            }

            /*
             * SetRuntime에 필요한 값을 설정한다.
             */
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

            /*
             * SetVisible에 필요한 값을 설정한다.
             */
            public void SetVisible(bool visible)
            {
                if (root != null)
                {
                    root.SetActive(visible);
                }
            }

            /*
             * RefreshLabel 대상의 현재 상태를 갱신한다.
             */
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

            /*
             * RefreshCooldownOverlay 대상의 현재 상태를 갱신한다.
             */
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

            /*
             * ResolveChildren에 필요한 값을 계산해 현재 상태에 반영한다.
             */
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
