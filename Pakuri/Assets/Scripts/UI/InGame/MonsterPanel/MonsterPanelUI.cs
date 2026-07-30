/*
 * 역할: 몬스터 목록 및 상세 표시.
 * 책임: 몬스터 Card를 생성하고 선택을 추적하며 유닛 능력치·스킬·체력·활성 상태를 표시한다.
 */

using Pakuri.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Pakuri.InGame
{

    /// MonsterPanelUI 상태를 Unity UI 또는 월드 오브젝트로 표시한다.
    public class MonsterPanelUI : MonoBehaviour
    {
        private const int MaxPartySlots = 5;
        private const int MaxVisibleActiveSlots = 3;

        [SerializeField] private Transform monsterPanelRoot;
        [SerializeField] private MonsterPanelSlotView[] monsterSlots = new MonsterPanelSlotView[MaxPartySlots];
        [SerializeField] private StageManager stageManager;
        [SerializeField] private UnitSpawnManager unitSpawnManager;

        /// Unity가 컴포넌트를 로드할 때 의존성과 소유 런타임 상태를 초기화한다.
        private void Awake()
        {
            ResolveReferences();
            ResolveSceneUi();
            SetPanelVisible(true);
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
            RefreshNow();
        }

        /// Now를 현재 런타임 모델을 기준으로 갱신한다.
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

        /// PlayerModelsBySlot를 결정한다.
        private UnitCombatState[] ResolvePlayerModelsBySlot()
        {
            var models = new UnitCombatState[MaxPartySlots];
            System.Collections.Generic.IReadOnlyList<CombatUnitEntry> players = null;
            if (unitSpawnManager != null)
            {
                players = unitSpawnManager.Players;
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

            return models;
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
        }

        /// SceneUi를 결정한다.
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

        /// 전달된 slotIndex 값을 사용해 MonsterSlot를 결정한다.
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

        /// EnsureMonsterSlotArray 작업을 수행한다.
        private void EnsureMonsterSlotArray()
        {
            if (monsterSlots == null || monsterSlots.Length != MaxPartySlots)
            {
                monsterSlots = new MonsterPanelSlotView[MaxPartySlots];
            }
        }

        /// Catalog를 결정한다.
        private GameDataCatalog ResolveCatalog()
        {
            return GameDataLoader.CurrentCatalog;
        }

        /// 전달된 런타임 입력값을 사용해 Image를 찾는다.
        private static Image FindImage(Transform root, string path)
        {
            var child = root != null ? root.Find(path) : null;
            return child != null ? child.GetComponent<Image>() : null;
        }

        /// 전달된 visible 값을 사용해 PanelVisible를 갱신한다.
        private void SetPanelVisible(bool visible)
        {
            if (monsterPanelRoot != null)
            {
                monsterPanelRoot.gameObject.SetActive(visible);
            }
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

        /// MonsterPanelSlotView가 소유하는 데이터와 동작을 캡슐화한다.
        [System.Serializable]
        private class MonsterPanelSlotView
        {
            [SerializeField] private GameObject root;
            [SerializeField] private Image monsterImage;
            [SerializeField] private ActiveSkillSlotView[] activeSlots = new ActiveSkillSlotView[MaxVisibleActiveSlots];

            private string lastMonsterId;

            public bool IsBound => root != null;

            /// MonsterPanelSlotView 인스턴스를 전달된 런타임 입력값으로 초기화한다.
            public MonsterPanelSlotView(Transform rootTransform)
            {
                Bind(rootTransform);
            }

            /// 전달된 rootTransform 값을 사용해 요청값를 런타임 사건 또는 씬 대상에 연결한다.
            public void Bind(Transform rootTransform)
            {
                root = rootTransform != null ? rootTransform.gameObject : null;
                monsterImage = null;
                activeSlots = new ActiveSkillSlotView[MaxVisibleActiveSlots];
                lastMonsterId = string.Empty;
                ResolveChildren();
            }

            /// 전달된 런타임 입력값을 사용해 Runtime를 갱신한다.
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

            /// 전달된 런타임 입력값을 사용해 MonsterImage를 현재 런타임 모델을 기준으로 갱신한다.
            private void RefreshMonsterImage(string monsterId, GameDataCatalog catalog)
            {
                if (monsterImage == null || string.Equals(lastMonsterId, monsterId, System.StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                lastMonsterId = monsterId;
                var monster = GameDataLoader.CurrentCatalog.GetMonster(monsterId);
                if (monster != null && monster.MonsterIconImage != null)
                {
                    monsterImage.sprite = monster.MonsterIconImage;
                    monsterImage.enabled = true;
                    return;
                }

                monsterImage.sprite = null;
                monsterImage.enabled = false;
            }

            /// 전달된 model 값을 사용해 ActiveSlots를 현재 런타임 모델을 기준으로 갱신한다.
            private void RefreshActiveSlots(UnitCombatState model)
            {
                System.Collections.Generic.IReadOnlyList<SkillUseState> runtimes = null;
                if (model != null && model.Skills != null)
                {
                    runtimes = model.SkillState.ActiveSkills;
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

                    SkillUseState runtime = null;
                    if (i < runtimeCount)
                    {
                        runtime = runtimes[i];
                    }
                    view.SetRuntime(runtime);
                }
            }

            /// 전달된 count 값을 사용해 SlotsActive를 갱신한다.
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

            /// 전달된 visible 값을 사용해 Visible를 갱신한다.
            private void SetVisible(bool visible)
            {
                if (root != null)
                {
                    root.SetActive(visible);
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
                    monsterImage = FindImage(root.transform, "Monster Image");
                }

                EnsureSlotArray();
                ResolveSlot(0, "Active1");
                ResolveSlot(1, "Active2");
                ResolveSlot(2, "Active3");
            }

            /// 전달된 런타임 입력값을 사용해 Slot를 결정한다.
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

            /// EnsureSlotArray 작업을 수행한다.
            private void EnsureSlotArray()
            {
                if (activeSlots == null || activeSlots.Length != MaxVisibleActiveSlots)
                {
                    activeSlots = new ActiveSkillSlotView[MaxVisibleActiveSlots];
                }
            }
        }

        /// ActiveSkillSlotView가 소유하는 데이터와 동작을 캡슐화한다.
        [System.Serializable]
        private class ActiveSkillSlotView
        {
            [SerializeField] private GameObject root;
            [SerializeField] private Image skillImage;
            [SerializeField] private Image cooldownOverlay;
            [SerializeField] private TMP_Text label;

            public bool IsBound => root != null;

            /// ActiveSkillSlotView 인스턴스를 전달된 런타임 입력값으로 초기화한다.
            public ActiveSkillSlotView(GameObject root)
            {
                Bind(root);
            }

            /// 전달된 root 값을 사용해 요청값를 런타임 사건 또는 씬 대상에 연결한다.
            public void Bind(GameObject root)
            {
                this.root = root;
                skillImage = null;
                cooldownOverlay = null;
                label = null;
                ResolveChildren();
            }

            /// 전달된 runtime 값을 사용해 Runtime를 갱신한다.
            public void SetRuntime(SkillUseState runtime)
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

            /// 전달된 visible 값을 사용해 Visible를 갱신한다.
            public void SetVisible(bool visible)
            {
                if (root != null)
                {
                    root.SetActive(visible);
                }
            }

            /// 전달된 runtime 값을 사용해 Label를 현재 런타임 모델을 기준으로 갱신한다.
            private void RefreshLabel(SkillUseState runtime)
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

            /// 전달된 runtime 값을 사용해 CooldownOverlay를 현재 런타임 모델을 기준으로 갱신한다.
            private void RefreshCooldownOverlay(SkillUseState runtime)
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

            /// Children를 결정한다.
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
