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

    /// 파티 몬스터 목록과 선택한 몬스터의 능력치·스킬을 전투 화면에 표시한다.
    public class MonsterPanelUI : MonoBehaviour
    {
        private const int MaxPartySlots = 5;
        private const int MaxVisibleActiveSlots = 3;

        private Transform monsterPanelRoot;
        private MonsterPanelSlotView[] monsterSlots = new MonsterPanelSlotView[MaxPartySlots];
        private UnitSpawnManager unitSpawnManager;
        private bool referencesBound;
        private bool bindingFailed;

        /// Unity가 컴포넌트를 로드할 때 의존성과 소유 런타임 상태를 초기화한다.
        private void Awake()
        {
            if (!BindObject())
            {
                enabled = false;
                return;
            }

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
            RefreshNow();
        }

        public void RefreshNow()
        {
            SetPanelVisible(true);

            var modelsBySlot = ResolvePlayerModelsBySlot();
            for (var i = 0; i < monsterSlots.Length; i++)
            {
                var slotView = monsterSlots[i];
                if (slotView == null)
                {
                    continue;
                }

                slotView.SetRuntime(modelsBySlot[i]);
            }
        }

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

        private void SetPanelVisible(bool visible)
        {
            if (monsterPanelRoot != null)
            {
                monsterPanelRoot.gameObject.SetActive(visible);
            }
        }

        [System.Serializable]
        private class MonsterPanelSlotView
        {
            private GameObject root;
            private Image monsterImage;
            private ActiveSkillSlotView[] activeSlots = new ActiveSkillSlotView[MaxVisibleActiveSlots];

            private string lastMonsterName;

            public void SetRuntime(UnitCombatState model)
            {
                if (root == null)
                {
                    return;
                }

                var monsterName = string.Empty;
                if (model != null)
                {
                    monsterName = model.Identity.DefinitionName;
                }
                if (string.IsNullOrWhiteSpace(monsterName))
                {
                    SetVisible(false);
                    SetSlotsActive(0);
                    return;
                }

                SetVisible(true);
                RefreshMonsterImage(monsterName);
                RefreshActiveSlots(model);
            }

            private void RefreshMonsterImage(string monsterName)
            {
                if (monsterImage == null || string.Equals(lastMonsterName, monsterName, System.StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                lastMonsterName = monsterName;
                var monster = GameDataLoader.CurrentCatalog.GetMonster(monsterName);
                if (monster != null && monster.MonsterIconImage != null)
                {
                    monsterImage.sprite = monster.MonsterIconImage;
                    monsterImage.enabled = true;
                    return;
                }

                monsterImage.sprite = null;
                monsterImage.enabled = false;
            }

            private void RefreshActiveSlots(UnitCombatState model)
            {
                System.Collections.Generic.IReadOnlyList<SkillExecutionState> runtimes = null;
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

                    SkillExecutionState runtime = null;
                    if (i < runtimeCount)
                    {
                        runtime = runtimes[i];
                    }
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

            private void SetVisible(bool visible)
            {
                if (root != null)
                {
                    root.SetActive(visible);
                }
            }

            internal void BindObject(
                Component owner,
                Transform panelRoot,
                string slotPath,
                int slotIndex,
                ref bool valid)
            {
                var slotRoot = panelRoot != null ? panelRoot.Find(slotPath) : null;
                if (slotRoot == null)
                {
                    Debug.LogError(
                        $"{owner.GetType().Name} BindObject failed: field 'monsterSlots[{slotIndex}]' at path '{slotPath}' requires a slot object.",
                        owner);
                    valid = false;
                    return;
                }

                root = slotRoot.gameObject;
                monsterImage = UiBindingUtility.BindChild<Image>(
                    owner,
                    slotRoot,
                    "Monster Image",
                    $"monsterSlots[{slotIndex}].monsterImage",
                    ref valid);
                activeSlots = new ActiveSkillSlotView[MaxVisibleActiveSlots];
                for (var i = 0; i < activeSlots.Length; i++)
                {
                    activeSlots[i] = new ActiveSkillSlotView();
                    activeSlots[i].BindObject(
                        owner,
                        slotRoot,
                        $"Active{i + 1}",
                        slotIndex,
                        i,
                        ref valid);
                }
            }

        }

        [System.Serializable]
        private class ActiveSkillSlotView
        {
            private GameObject root;
            private Image skillImage;
            private Image cooldownOverlay;
            private TMP_Text label;

            public void SetRuntime(SkillExecutionState runtime)
            {
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

            private void RefreshLabel(SkillExecutionState runtime)
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

            private void RefreshCooldownOverlay(SkillExecutionState runtime)
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

            internal void BindObject(
                Component owner,
                Transform slotRoot,
                string skillPath,
                int slotIndex,
                int skillIndex,
                ref bool valid)
            {
                var skillRoot = slotRoot != null ? slotRoot.Find(skillPath) : null;
                if (skillRoot == null)
                {
                    Debug.LogError(
                        $"{owner.GetType().Name} BindObject failed: field 'monsterSlots[{slotIndex}].activeSlots[{skillIndex}]' at path '{skillPath}' requires a skill object.",
                        owner);
                    valid = false;
                    return;
                }

                root = skillRoot.gameObject;
                skillImage = UiBindingUtility.BindSelf<Image>(
                    owner,
                    skillRoot,
                    $"monsterSlots[{slotIndex}].activeSlots[{skillIndex}].skillImage",
                    ref valid);
                cooldownOverlay = UiBindingUtility.BindChild<Image>(
                    owner,
                    skillRoot,
                    "CooldownOverlay",
                    $"monsterSlots[{slotIndex}].activeSlots[{skillIndex}].cooldownOverlay",
                    ref valid);
                var labelPath = skillIndex == 0 ? "Text (TMP)" : $"Text (TMP) ({skillIndex})";
                label = UiBindingUtility.BindChild<TMP_Text>(
                    owner,
                    skillRoot,
                    labelPath,
                    $"monsterSlots[{slotIndex}].activeSlots[{skillIndex}].label",
                    ref valid);
            }

        }

        private bool BindObject()
        {
            if (referencesBound)
            {
                return true;
            }

            if (bindingFailed)
            {
                return false;
            }

            var valid = true;
            monsterPanelRoot = UiBindingUtility.BindScene<Transform>(
                this,
                "HUD/MonsterPanel",
                nameof(monsterPanelRoot),
                ref valid);
            unitSpawnManager = UiBindingUtility.BindSceneComponent<UnitSpawnManager>(
                this,
                nameof(unitSpawnManager),
                ref valid);
            monsterSlots = new MonsterPanelSlotView[MaxPartySlots];
            for (var i = 0; i < monsterSlots.Length; i++)
            {
                monsterSlots[i] = new MonsterPanelSlotView();
                monsterSlots[i].BindObject(
                    this,
                    monsterPanelRoot,
                    $"{i + 1}PMonster",
                    i,
                    ref valid);
            }

            referencesBound = valid;
            bindingFailed = !valid;
            return valid;
        }
    }
}
