using Pakuri.NewCore.Bootstrap;
using Pakuri.NewCore.Run;
using Pakuri.NewCore.Spawn;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/* party Monster portrait와 active skill cooldown을 authored panel에 표시한다. */
namespace Pakuri.NewCore.UI.InGame.MonsterPanel
{
    public class NewCoreMonsterPanelUI : MonoBehaviour
    {
        private const int MaximumPartySlots = 5;
        private const int MaximumVisibleSkills = 3;

        [SerializeField] private Transform monsterPanelRoot;
        [SerializeField] private StageManager stageManager;
        [SerializeField] private SpawnManager unitSpawnManager;
        [SerializeField] private GameBootstrap combatManager;

        /* panel 갱신에 필요한 scene 참조를 찾는다. */
        private void Awake()
        {
            ResolveReferences();
        }

        /* 매 frame 현재 party와 cooldown 표시를 갱신한다. */
        private void Update()
        {
            RefreshNow();
        }

        /* party slot portrait와 visible skill 정보를 현재 runtime 상태로 그린다. */
        public void RefreshNow()
        {
            if (combatManager == null || combatManager.Stage == null)
            {
                return;
            }

            var party = combatManager.Stage.Session.PartyRoster.Members;
            for (var index = 0; index < MaximumPartySlots; index++)
            {
                Transform slot = null;
                if (monsterPanelRoot != null)
                {
                    slot = monsterPanelRoot.Find(
                        $"{index + 1}PMonster");
                }

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
                Image image = null;
                if (imageTransform != null)
                {
                    image = imageTransform.GetComponent<Image>();
                }

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

        /* 한 Monster의 active skill icon과 cooldown overlay를 갱신한다. */
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
                Image overlay = null;
                if (overlayTransform != null)
                {
                    overlay =
                        overlayTransform.GetComponent<Image>();
                }

                if (overlay != null)
                {
                    var duration = skill.cooldown_seconds ?? 0f;
                    float remaining = cooldown.RemainingCooldown;
                    if (cooldown.IsReloading)
                    {
                        remaining = cooldown.RemainingReload;
                    }

                    overlay.type = Image.Type.Filled;
                    overlay.fillMethod = Image.FillMethod.Vertical;
                    overlay.fillAmount = 0f;
                    if (duration > 0f)
                    {
                        overlay.fillAmount =
                            Mathf.Clamp01(remaining / duration);
                    }

                    overlay.gameObject.SetActive(remaining > 0f);
                }

                var label = slot.GetComponentInChildren<TMP_Text>(true);
                if (label != null)
                {
                    label.text = string.Empty;
                    if (cooldown.CurrentMagazine.HasValue)
                    {
                        label.text =
                            cooldown.CurrentMagazine.Value.ToString();
                    }
                }
            }
        }

        /* authored field가 비어 있으면 scene의 manager와 panel root를 찾는다. */
        private void ResolveReferences()
        {
            if (combatManager == null)
            {
                combatManager =
                    FindFirstObjectByType<GameBootstrap>();
            }

            if (stageManager == null)
            {
                stageManager =
                    FindFirstObjectByType<StageManager>();
            }

            if (unitSpawnManager == null)
            {
                unitSpawnManager =
                    FindFirstObjectByType<SpawnManager>();
            }

            if (monsterPanelRoot == null)
            {
                monsterPanelRoot = transform.Find("MonsterPanel");
            }
        }
    }
}
