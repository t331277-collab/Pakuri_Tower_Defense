using System.Globalization;
using Pakuri.Combat;
using Pakuri.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Pakuri.InGame
{
    public sealed class CharacterInfoPopupUI : MonoBehaviour
    {
        private const float ComparisonEpsilon = 0.0001f;

        private TMP_Text summaryText;
        private Image iconImage;
        private TMP_Text hpText;
        private TMP_Text attackText;
        private TMP_Text spellText;
        private TMP_Text criticalChanceText;
        private TMP_Text criticalDamageText;
        private TMP_Text physicalDefenseText;
        private TMP_Text holyDefenseText;
        private TMP_Text darknessDefenseText;
        private TMP_Text fireDefenseText;
        private TMP_Text lightningDefenseText;
        private TMP_Text iceDefenseText;
        private readonly Image[] artifactImages = new Image[ArtifactState.MaxOwnedArtifactCount];
        private Button closeButton;

        private Color hpBaseColor;
        private Color attackBaseColor;
        private Color spellBaseColor;
        private Color criticalChanceBaseColor;
        private Color criticalDamageBaseColor;
        private Color physicalDefenseBaseColor;
        private Color holyDefenseBaseColor;
        private Color darknessDefenseBaseColor;
        private Color fireDefenseBaseColor;
        private Color lightningDefenseBaseColor;
        private Color iceDefenseBaseColor;
        private bool referencesBound;
        private bool bindingFailed;

        private void Awake()
        {
            if (!BindObject())
            {
                enabled = false;
                return;
            }

            BindStaticButtons();
            Hide();
        }

        public void Open(UnitCombatState model)
        {
            if (!BindObject() || model == null)
            {
                Hide();
                return;
            }

            var definition = GameDataLoader.CurrentCatalog?.GetMonster(model.Identity?.DefinitionName);
            if (definition == null)
            {
                Hide();
                return;
            }

            summaryText.text = !string.IsNullOrWhiteSpace(definition.DisplayName)
                ? definition.DisplayName
                : model.Identity?.DisplayName ?? model.Identity?.DefinitionName;
            iconImage.sprite = definition.MonsterIconImage;
            iconImage.color = iconImage.sprite != null ? Color.white : new Color(0f, 0f, 0f, 0.3f);

            var stats = model.Stats;
            var baseStats = definition.BaseStats;
            SetValue(hpText, stats?.MaxHealth ?? 0f, baseStats?.MaxHealth ?? 0f, "0", hpBaseColor);
            SetValue(attackText, DamageCalculator.CalculateFinalAttackPower(model), baseStats?.AttackPower ?? 0f, "0", attackBaseColor);
            SetValue(spellText, DamageCalculator.CalculateFinalSpellPower(model), baseStats?.SpellPower ?? 0f, "0", spellBaseColor);
            SetValue(
                criticalChanceText,
                DamageCalculator.CalculateFinalCriticalChance(model),
                baseStats?.CriticalChance ?? 0f,
                "0.##%",
                criticalChanceBaseColor);
            SetValue(
                criticalDamageText,
                DamageCalculator.CalculateFinalCriticalDamageMultiplier(model),
                baseStats?.CriticalDamage ?? 0f,
                "0.##%",
                criticalDamageBaseColor);

            var baseDefenses = definition.Defenses;
            SetDefense(physicalDefenseText, model, baseDefenses, DamageAttribute.Physical, physicalDefenseBaseColor);
            SetDefense(holyDefenseText, model, baseDefenses, DamageAttribute.Holy, holyDefenseBaseColor);
            SetDefense(darknessDefenseText, model, baseDefenses, DamageAttribute.Darkness, darknessDefenseBaseColor);
            SetDefense(fireDefenseText, model, baseDefenses, DamageAttribute.Fire, fireDefenseBaseColor);
            SetDefense(lightningDefenseText, model, baseDefenses, DamageAttribute.Lightning, lightningDefenseBaseColor);
            SetDefense(iceDefenseText, model, baseDefenses, DamageAttribute.Ice, iceDefenseBaseColor);
            RefreshArtifacts(model);
            BindStaticButtons();
            UiObjectUtility.SetActive(gameObject, true);
        }

        public void Hide()
        {
            UiObjectUtility.SetActive(gameObject, false);
        }

        private void RefreshArtifacts(UnitCombatState model)
        {
            var catalog = GameDataLoader.CurrentCatalog;
            var ownedArtifacts = model.Artifacts?.OwnedArtifactNames;
            for (var i = 0; i < artifactImages.Length; i++)
            {
                var image = artifactImages[i];
                if (image == null)
                {
                    continue;
                }

                image.sprite = null;
                image.color = Color.white;
                var hasArtifact = ownedArtifacts != null && i < ownedArtifacts.Count;
                if (hasArtifact)
                {
                    var artifact = catalog?.GetData<ArtifactDefinition>(ownedArtifacts[i]);
                    image.sprite = artifact?.Icon;
                    hasArtifact = image.sprite != null;
                }

                UiObjectUtility.SetActive(image.gameObject, hasArtifact);
            }
        }

        private static void SetDefense(
            TMP_Text text,
            UnitCombatState model,
            UnitDefenseStats baseDefenses,
            DamageAttribute attribute,
            Color baseColor)
        {
            var finalValue = DamageCalculator.CalculateFinalDefense(model, attribute);
            var baseValue = baseDefenses != null ? baseDefenses.Get(attribute) : 0f;
            SetValue(text, finalValue, baseValue, "0", baseColor);
        }

        private static void SetValue(
            TMP_Text text,
            float value,
            float baseValue,
            string format,
            Color baseColor)
        {
            if (text == null)
            {
                return;
            }

            text.text = value.ToString(format, CultureInfo.InvariantCulture);
            if (value > baseValue + ComparisonEpsilon)
            {
                text.color = Color.blue;
            }
            else if (value < baseValue - ComparisonEpsilon)
            {
                text.color = Color.red;
            }
            else
            {
                text.color = baseColor;
            }
        }

        private void BindStaticButtons()
        {
            if (closeButton == null)
            {
                return;
            }

            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(Hide);
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
            summaryText = UiBindingUtility.BindChild<TMP_Text>(this, "Summary", nameof(summaryText), ref valid);
            iconImage = UiBindingUtility.BindChild<Image>(this, "Icon", nameof(iconImage), ref valid);
            hpText = UiBindingUtility.BindChild<TMP_Text>(this, "Info/HP", nameof(hpText), ref valid);
            attackText = UiBindingUtility.BindChild<TMP_Text>(this, "Info/ATK", nameof(attackText), ref valid);
            spellText = UiBindingUtility.BindChild<TMP_Text>(this, "Info/SPELL", nameof(spellText), ref valid);
            criticalChanceText = UiBindingUtility.BindChild<TMP_Text>(this, "Info/Criti_Chan", nameof(criticalChanceText), ref valid);
            criticalDamageText = UiBindingUtility.BindChild<TMP_Text>(this, "Info/Criti_Dem", nameof(criticalDamageText), ref valid);
            physicalDefenseText = UiBindingUtility.BindChild<TMP_Text>(this, "Defen/Phy", nameof(physicalDefenseText), ref valid);
            holyDefenseText = UiBindingUtility.BindChild<TMP_Text>(this, "Defen/Holy", nameof(holyDefenseText), ref valid);
            darknessDefenseText = UiBindingUtility.BindChild<TMP_Text>(this, "Defen/Dark", nameof(darknessDefenseText), ref valid);
            fireDefenseText = UiBindingUtility.BindChild<TMP_Text>(this, "Defen/Fire", nameof(fireDefenseText), ref valid);
            lightningDefenseText = UiBindingUtility.BindChild<TMP_Text>(this, "Defen/Lightning", nameof(lightningDefenseText), ref valid);
            iceDefenseText = UiBindingUtility.BindChild<TMP_Text>(this, "Defen/Ice", nameof(iceDefenseText), ref valid);
            artifactImages[0] = UiBindingUtility.BindChild<Image>(this, "Artifact/Arti1", nameof(artifactImages) + "[0]", ref valid);
            artifactImages[1] = UiBindingUtility.BindChild<Image>(this, "Artifact/Arti2", nameof(artifactImages) + "[1]", ref valid);
            artifactImages[2] = UiBindingUtility.BindChild<Image>(this, "Artifact/Arti3", nameof(artifactImages) + "[2]", ref valid);
            closeButton = UiBindingUtility.BindChild<Button>(this, "Close", nameof(closeButton), ref valid);

            if (!valid)
            {
                bindingFailed = true;
                return false;
            }

            hpBaseColor = hpText.color;
            attackBaseColor = attackText.color;
            spellBaseColor = spellText.color;
            criticalChanceBaseColor = criticalChanceText.color;
            criticalDamageBaseColor = criticalDamageText.color;
            physicalDefenseBaseColor = physicalDefenseText.color;
            holyDefenseBaseColor = holyDefenseText.color;
            darknessDefenseBaseColor = darknessDefenseText.color;
            fireDefenseBaseColor = fireDefenseText.color;
            lightningDefenseBaseColor = lightningDefenseText.color;
            iceDefenseBaseColor = iceDefenseText.color;
            referencesBound = true;
            return true;
        }
    }
}
