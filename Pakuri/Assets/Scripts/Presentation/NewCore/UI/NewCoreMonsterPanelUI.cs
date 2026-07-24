using Pakuri.NewCore.Presentation.Scene;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Pakuri.NewCore.Presentation.UI
{
    public sealed class NewCoreMonsterPanelUI : MonoBehaviour
    {
        private const int MaximumPartySlots = 5;
        private const int MaximumVisibleSkills = 3;

        [SerializeField] private Transform monsterPanelRoot;
        [SerializeField] private NewCoreStageController stageManager;
        [SerializeField] private NewCoreSpawnController unitSpawnManager;
        [SerializeField] private NewCoreSceneRuntime combatManager;

        private void Awake()
        {
            ResolveReferences();
        }

        private void Update()
        {
            RefreshNow();
        }

        public void RefreshNow()
        {
            if (combatManager == null || combatManager.Stage == null)
            {
                return;
            }

            var party = combatManager.Stage.Session.PartyRoster.Members;
            for (var index = 0; index < MaximumPartySlots; index++)
            {
                var slot = monsterPanelRoot != null
                    ? monsterPanelRoot.Find($"{index + 1}PMonster")
                    : null;
                if (slot == null)
                {
                    continue;
                }

                var visible = index < party.Count;
                slot.gameObject.SetActive(visible);
                if (!visible)
                {
                    continue;
                }

                var monster = party[index];
                var imageTransform = slot.Find("Monster Image");
                var image = imageTransform != null
                    ? imageTransform.GetComponent<Image>()
                    : null;
                if (image != null
                    && combatManager.RuntimeCatalog.TryGetSprite(
                        monster.MonsterDefinition.MonsterIconImage,
                        out var portrait))
                {
                    image.sprite = portrait;
                    image.enabled = true;
                }

                RefreshSkills(slot, monster);
            }
        }

        private void RefreshSkills(
            Transform root,
            Units.Models.MonsterModel monster)
        {
            var skills = monster.SkillBucket.ActiveSkills;
            for (var index = 0; index < MaximumVisibleSkills; index++)
            {
                var slot = root.Find($"Active{index + 1}");
                if (slot == null)
                {
                    continue;
                }

                var visible = index < skills.Count;
                slot.gameObject.SetActive(visible);
                if (!visible)
                {
                    continue;
                }

                var skill = skills[index];
                var image = slot.GetComponent<Image>();
                if (image != null
                    && skill.Columns.TryGetValue(
                        "runtime_visual_sprite_path",
                        out var pathValue)
                    && pathValue is string path
                    && combatManager.RuntimeCatalog.TryGetSprite(
                        path,
                        out var sprite))
                {
                    image.sprite = sprite;
                }

                var cooldown =
                    monster.SkillBucket.GetCooldown(skill.skill_id);
                var overlayTransform = slot.Find("CooldownOverlay");
                var overlay = overlayTransform != null
                    ? overlayTransform.GetComponent<Image>()
                    : null;
                if (overlay != null)
                {
                    var duration = skill.cooldown_seconds ?? 0f;
                    var remaining = cooldown.IsReloading
                        ? cooldown.RemainingReload
                        : cooldown.RemainingCooldown;
                    overlay.type = Image.Type.Filled;
                    overlay.fillMethod = Image.FillMethod.Vertical;
                    overlay.fillAmount = duration > 0f
                        ? Mathf.Clamp01(remaining / duration)
                        : 0f;
                    overlay.gameObject.SetActive(remaining > 0f);
                }

                var label = slot.GetComponentInChildren<TMP_Text>(true);
                if (label != null)
                {
                    label.text = cooldown.CurrentMagazine.HasValue
                        ? cooldown.CurrentMagazine.Value.ToString()
                        : string.Empty;
                }
            }
        }

        private void ResolveReferences()
        {
            if (combatManager == null)
            {
                combatManager =
                    FindFirstObjectByType<NewCoreSceneRuntime>();
            }

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

            if (monsterPanelRoot == null)
            {
                monsterPanelRoot = transform.Find("MonsterPanel");
            }
        }
    }
}
