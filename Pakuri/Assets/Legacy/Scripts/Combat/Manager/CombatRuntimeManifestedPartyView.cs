using UnityEngine;

namespace Pakuri.Combat
{
    public partial class CombatRuntimeController
    {
        private readonly struct ManifestedMonsterStatusViews
        {
            public ManifestedMonsterStatusViews(TextMesh nameLabel, TextMesh hpLabel, SpriteRenderer hpBarFill, SpriteRenderer shieldBarFill)
            {
                NameLabel = nameLabel;
                HpLabel = hpLabel;
                HpBarFill = hpBarFill;
                ShieldBarFill = shieldBarFill;
            }

            public TextMesh NameLabel { get; }
            public TextMesh HpLabel { get; }
            public SpriteRenderer HpBarFill { get; }
            public SpriteRenderer ShieldBarFill { get; }
        }

        private ManifestedMonsterStatusViews ResolveManifestedMonsterStatusViews(Transform monsterTransform, bool preferSceneChildren)
        {
            if (monsterTransform == null)
            {
                return default;
            }

            var nameLabel = FindManifestedTextMesh(monsterTransform, "MonsterNameLabel", "Name Label", "NameLabel");
            var hpLabel = FindManifestedTextMesh(monsterTransform, "MonsterHpLabel", "HPLabel", "HPLable", "HP Label");
            var hpBar = FindManifestedSpriteRenderer(monsterTransform, "MonsterHpBar/Fill", "HPBar/Fill", "HpBar/Fill");
            var shieldBar = FindManifestedSpriteRenderer(monsterTransform, "MonsterHpBar/Shield", "HPBar/Shield", "HpBar/Shield");
            if (hpBar == null)
            {
                var generatedBar = EnsureManifestedHpBar(monsterTransform);
                hpBar = generatedBar.HpBarFill;
                shieldBar = shieldBar != null ? shieldBar : generatedBar.ShieldBarFill;
            }
            else
            {
                var normalizedBar = NormalizeManifestedHpBar(hpBar);
                hpBar = normalizedBar.HpBarFill != null ? normalizedBar.HpBarFill : hpBar;
                shieldBar = shieldBar != null ? shieldBar : normalizedBar.ShieldBarFill;
            }

            if (preferSceneChildren)
            {
                return new ManifestedMonsterStatusViews(nameLabel, hpLabel, hpBar, shieldBar);
            }

            return new ManifestedMonsterStatusViews(nameLabel, hpLabel, hpBar, shieldBar);
        }

        private static ManifestedMonsterStatusViews EnsureManifestedHpBar(Transform monsterTransform)
        {
            if (monsterTransform == null)
            {
                return default;
            }

            var barTransform = monsterTransform.Find("MonsterHpBar");
            if (barTransform == null)
            {
                var barObject = new GameObject("MonsterHpBar");
                barTransform = barObject.transform;
                barTransform.SetParent(monsterTransform, false);
                barTransform.localPosition = new Vector3(0f, 0.66f, 0f);
                barTransform.localScale = new Vector3(0.90f, 1f, 1f);
            }

            var background = EnsureManifestedBarRenderer(barTransform, "Background", Color.black, 34);
            if (background != null)
            {
                background.transform.localPosition = Vector3.zero;
                background.transform.localScale = new Vector3(1f, 0.08f, 1f);
            }

            var fill = EnsureManifestedBarRenderer(barTransform, "Fill", Color.red, 35);
            if (fill != null)
            {
                fill.transform.localPosition = new Vector3(0f, 0f, -0.01f);
                fill.transform.localScale = new Vector3(1f, 0.08f, 1f);
            }

            var shield = EnsureManifestedBarRenderer(barTransform, "Shield", Color.white, 36);
            if (shield != null)
            {
                shield.transform.localPosition = new Vector3(-0.5f, 0f, -0.02f);
                shield.transform.localScale = new Vector3(0f, 0.08f, 1f);
            }

            return new ManifestedMonsterStatusViews(null, null, fill, shield);
        }

        private static ManifestedMonsterStatusViews NormalizeManifestedHpBar(SpriteRenderer hpBarFill)
        {
            if (hpBarFill == null || hpBarFill.transform == null || hpBarFill.transform.parent == null)
            {
                return default;
            }

            var barTransform = hpBarFill.transform.parent;
            if (barTransform.localScale == Vector3.zero)
            {
                barTransform.localScale = new Vector3(0.90f, 1f, 1f);
            }

            var background = EnsureManifestedBarRenderer(barTransform, "Background", Color.black, 34);
            if (background != null)
            {
                background.transform.localPosition = Vector3.zero;
                if (Mathf.Approximately(background.transform.localScale.y, 0f))
                {
                    background.transform.localScale = new Vector3(1f, 0.08f, 1f);
                }
            }

            var fill = EnsureManifestedBarRenderer(barTransform, hpBarFill.transform.name, Color.red, 35);
            if (fill != null)
            {
                if (Mathf.Approximately(fill.transform.localScale.y, 0f))
                {
                    fill.transform.localScale = new Vector3(1f, 0.08f, 1f);
                }

                fill.transform.localPosition = new Vector3(fill.transform.localPosition.x, fill.transform.localPosition.y, -0.01f);
            }

            var shield = EnsureManifestedBarRenderer(barTransform, "Shield", Color.white, 36);
            if (shield != null)
            {
                if (Mathf.Approximately(shield.transform.localScale.y, 0f))
                {
                    shield.transform.localScale = new Vector3(0f, 0.08f, 1f);
                }

                shield.transform.localPosition = new Vector3(shield.transform.localPosition.x, shield.transform.localPosition.y, -0.02f);
            }

            return new ManifestedMonsterStatusViews(null, null, fill, shield);
        }

        private static SpriteRenderer EnsureManifestedBarRenderer(Transform parent, string childName, Color color, int sortingOrder)
        {
            if (parent == null || string.IsNullOrWhiteSpace(childName))
            {
                return null;
            }

            var child = parent.Find(childName);
            if (child == null)
            {
                var childObject = new GameObject(childName);
                child = childObject.transform;
                child.SetParent(parent, false);
            }

            var renderer = child.GetComponent<SpriteRenderer>();
            if (renderer == null)
            {
                renderer = child.gameObject.AddComponent<SpriteRenderer>();
            }

            renderer.sprite = GetSharedSprite();
            renderer.color = color;
            renderer.sortingOrder = sortingOrder;
            return renderer;
        }

        private static TextMesh FindManifestedTextMesh(Transform root, params string[] relativePaths)
        {
            if (root == null || relativePaths == null)
            {
                return null;
            }

            for (var i = 0; i < relativePaths.Length; i++)
            {
                var child = root.Find(relativePaths[i]);
                if (child == null)
                {
                    continue;
                }

                var text = child.GetComponent<TextMesh>();
                if (text != null)
                {
                    return text;
                }
            }

            return null;
        }

        private static SpriteRenderer FindManifestedSpriteRenderer(Transform root, params string[] relativePaths)
        {
            if (root == null || relativePaths == null)
            {
                return null;
            }

            for (var i = 0; i < relativePaths.Length; i++)
            {
                var child = root.Find(relativePaths[i]);
                if (child == null)
                {
                    continue;
                }

                var renderer = child.GetComponent<SpriteRenderer>();
                if (renderer != null)
                {
                    return renderer;
                }
            }

            return null;
        }

        private TextMesh EnsureManifestedMonsterLabel(Transform monsterTransform)
        {
            if (monsterTransform == null)
            {
                return null;
            }

            var labelTransform = monsterTransform.Find("PartyMonsterLabel");
            if (labelTransform == null)
            {
                labelTransform = new GameObject("PartyMonsterLabel").transform;
                labelTransform.SetParent(monsterTransform, false);
                labelTransform.localPosition = new Vector3(0f, 0.9f, 0f);
                labelTransform.localScale = new Vector3(0.12f, 0.12f, 1f);
            }

            var label = labelTransform.GetComponent<TextMesh>();
            if (label == null)
            {
                label = labelTransform.gameObject.AddComponent<TextMesh>();
            }

            label.anchor = TextAnchor.MiddleCenter;
            label.alignment = TextAlignment.Center;
            label.fontSize = 30;
            label.color = Color.white;
            var labelRenderer = label.GetComponent<MeshRenderer>();
            if (labelRenderer != null)
            {
                labelRenderer.sortingOrder = 38;
            }

            return label;
        }

        private void UpdateManifestedMonsterLabel(CombatUnitRuntime runtime)
        {
            if (runtime == null || runtime.Monster == null)
            {
                return;
            }

            var shieldText = runtime.ShieldValue > 0f ? $" SH {Mathf.CeilToInt(runtime.ShieldValue)}" : string.Empty;
            var hpText = $"HP {Mathf.CeilToInt(Mathf.Max(0f, runtime.CurrentHealth))}/{Mathf.CeilToInt(runtime.MaxHealth)}{shieldText}";
            if (runtime.NameLabel != null)
            {
                runtime.NameLabel.text = runtime.Monster.DisplayName;
            }

            if (runtime.HpLabel != null)
            {
                runtime.HpLabel.text = hpText;
            }
            else if (runtime.Label != null)
            {
                var skillLine = runtime.Skills.Count > 0 && runtime.Skills[0] != null && runtime.Skills[0].Skill != null
                    ? $"{runtime.Skills[0].Skill.DisplayName} {Mathf.CeilToInt(Mathf.Max(0f, runtime.Skills[0].CooldownRemaining))}"
                    : "No learned active";
                runtime.Label.text = $"{runtime.Monster.DisplayName}\n{hpText}\n{skillLine}";
            }

            UpdateManifestedHpShieldBarFill(runtime, runtime.CurrentHealth, runtime.MaxHealth, runtime.ShieldValue);
        }

        private static void UpdateManifestedHpShieldBarFill(CombatUnitRuntime runtime, float currentHealth, float maxHealth, float shieldValue)
        {
            if (runtime == null)
            {
                return;
            }

            var hpBarFill = runtime.HpBarFill;
            var shieldBarFill = runtime.ShieldBarFill;
            if (hpBarFill != null && hpBarFill.sprite == null)
            {
                var normalizedBar = NormalizeManifestedHpBar(hpBarFill);
                hpBarFill = normalizedBar.HpBarFill != null ? normalizedBar.HpBarFill : hpBarFill;
                shieldBarFill = shieldBarFill != null ? shieldBarFill : normalizedBar.ShieldBarFill;
            }

            UpdateHpShieldBarFill(hpBarFill, shieldBarFill, currentHealth, maxHealth, shieldValue);
        }
    }
}
