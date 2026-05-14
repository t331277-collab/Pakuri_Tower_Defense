using System;
using System.Collections.Generic;
using Pakuri.Data;
using Pakuri.Run;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Pakuri.Combat
{
    public partial class CombatRuntimeController
    {
        private readonly struct SelectedMonsterStatusLayout
        {
            public SelectedMonsterStatusLayout(
                Vector3 barLocalPosition,
                Vector3 hpTextLocalPosition,
                Vector3 nameLocalPosition,
                Vector3 textScale,
                float barWidth,
                float barHeight)
            {
                BarLocalPosition = barLocalPosition;
                HpTextLocalPosition = hpTextLocalPosition;
                NameLocalPosition = nameLocalPosition;
                TextScale = textScale;
                BarWidth = barWidth;
                BarHeight = barHeight;
            }

            public Vector3 BarLocalPosition { get; }
            public Vector3 HpTextLocalPosition { get; }
            public Vector3 NameLocalPosition { get; }
            public Vector3 TextScale { get; }
            public float BarWidth { get; }
            public float BarHeight { get; }
        }

        private const float DamagePopupDuration = 1f;
        private const float DamagePopupRiseDistance = 0.55f;

        private void ResolveSceneReferences()
        {
            if (targetCamera == null)
            {
                targetCamera = Camera.main;
            }

            if (targetCamera == null)
            {
                targetCamera = FindFirstObjectByType<Camera>();
            }

            transform.position = Vector3.zero;
            transform.rotation = Quaternion.identity;
            transform.localScale = Vector3.one;

            nexusAnchor = EnsureChild(nexusAnchor, "Nexus", new Vector3(2f, 8f, 0f));
            eveAnchor = EnsureChild(eveAnchor, "EveUnit", new Vector3(6f, 8f, 0f));
            enemySpawnAnchor = EnsureChild(enemySpawnAnchor, "EnemySpawnPoint", DefaultEnemySpawnPosition);
            inputTargetAnchor = EnsureChild(inputTargetAnchor, "InputTarget", new Vector3(16f, 8f, 0f));
            enemyRoot = EnsureChild(enemyRoot, "EnemyRoot", Vector3.zero);
            projectileRoot = EnsureChild(projectileRoot, "ProjectileRoot", Vector3.zero);
            battlefieldBackgroundAnchor = EnsureChild(battlefieldBackgroundAnchor, "BattlefieldBackground", GetBattlefieldCenter());

            if (Application.isPlaying && currentAttackPoint == Vector3.zero)
            {
                currentAttackPoint = inputTargetAnchor.position;
            }
        }

        private Transform EnsureChild(Transform current, string childName, Vector3 worldPosition)
        {
            if (current != null)
            {
                return current;
            }

            var existing = transform.Find(childName);
            if (existing != null)
            {
                return existing;
            }

            var child = new GameObject(childName).transform;
            child.SetParent(transform, false);
            child.position = worldPosition;
            return child;
        }

        private void ConfigureCamera()
        {
            if (targetCamera == null)
            {
                return;
            }

            targetCamera.orthographic = true;
            targetCamera.clearFlags = CameraClearFlags.SolidColor;
            targetCamera.backgroundColor = Color.white;

            var aspect = Mathf.Max(1f, targetCamera.aspect);
            var cameraPosition = targetCamera.transform.position;
            cameraPosition.x = (fieldSize.x - 1f) * 0.5f;
            cameraPosition.y = (fieldSize.y - 1f) * 0.5f;
            cameraPosition.z = -10f;
            targetCamera.transform.position = cameraPosition;

            var heightDrivenSize = (fieldSize.y * 0.5f) + 1f;
            var widthDrivenSize = (fieldSize.x / (2f * aspect)) + 0.5f;
            targetCamera.orthographicSize = Mathf.Max(heightDrivenSize, widthDrivenSize);
            EnsureBattlefieldBackgroundVisual();
        }

        private void ApplyFallbackMonsterValues()
        {
            selectedMonster = null;
            selectedMonsterName = "이브";
            selectedElementLabel = "번개";
            selectedActiveSkillId = "eve-a";
            selectedPassiveSkillId = "eve-f";
            selectedActiveSkillName = "아크 볼트";
            selectedPassiveSkillName = "전압 보정";
            selectedStatusEffectLabel = "감전";
            selectedDamageAttribute = DamageAttribute.Lightning;
            selectedMonsterDefenses = new AttributeDefenseSet();
            selectedUnitColor = new Color(0.41f, 0.78f, 1f, 0.95f);
            selectedProjectileColor = new Color(0.61f, 0.93f, 1f, 0.98f);
            selectedUnitSprite = null;
            selectedProjectileSprite = null;
            unitMaxHealthConfigured = eveMaxHealth;
            powerStatConfigured = eveSpellPower;
            baseDamageConfigured = eveBaseLightningDamage;
            powerCoefficientConfigured = eveSpellPowerCoefficient;
            projectileSpeedConfigured = eveProjectileSpeed;
            projectileLifetimeConfigured = eveProjectileLifetime;
            projectileHitRadiusConfigured = eveProjectileHitRadius;
            magazineCapacityConfigured = eveMagazineCapacity;
            reloadDurationConfigured = eveReloadDuration;
            shotIntervalConfigured = eveShotInterval;
            statusChanceConfigured = eveShockChance;
        }

        private void ConfigureMonster(MonsterDefinition monster)
        {
            if (monster == null)
            {
                ApplyFallbackMonsterValues();
                EnsureAnchorVisuals();
                return;
            }

            selectedMonster = monster;
            selectedMonsterName = string.IsNullOrWhiteSpace(monster.DisplayName) ? "Unknown" : monster.DisplayName;
            selectedElementLabel = string.IsNullOrWhiteSpace(monster.ElementLabel) ? "기본" : monster.ElementLabel;
            selectedActiveSkillId = ResolveSelectedActiveSkillId(monster);
            selectedPassiveSkillId = ResolveSelectedPassiveSkillId(monster);
            selectedActiveSkillName = string.IsNullOrWhiteSpace(monster.ActiveSkillName) ? "기본 스킬" : monster.ActiveSkillName;
            selectedPassiveSkillName = string.IsNullOrWhiteSpace(monster.PassiveSkillName) ? string.Empty : monster.PassiveSkillName;
            selectedStatusEffectLabel = string.IsNullOrWhiteSpace(monster.StatusEffectLabel) ? string.Empty : monster.StatusEffectLabel;
            selectedDamageAttribute = monster.PrimaryAttribute;
            selectedMonsterDefenses = monster.Defenses != null ? monster.Defenses.Clone() : new AttributeDefenseSet();
            selectedUnitColor = monster.UnitColor.a <= 0f ? new Color(0.78f, 0.82f, 0.92f, 0.95f) : monster.UnitColor;
            selectedProjectileColor = monster.ProjectileColor.a <= 0f ? new Color(0.95f, 0.95f, 1f, 0.98f) : monster.ProjectileColor;
            selectedUnitSprite = monster.UnitSprite;
            selectedProjectileSprite = monster.ProjectileSprite;
            unitMaxHealthConfigured = Mathf.Max(1f, monster.MaxHealth);
            powerStatConfigured = monster.PowerStat;
            baseDamageConfigured = Mathf.Max(1f, monster.BaseDamage);
            powerCoefficientConfigured = monster.PowerCoefficient;
            projectileSpeedConfigured = Mathf.Max(0.1f, monster.ProjectileSpeed);
            projectileLifetimeConfigured = Mathf.Max(0.1f, monster.ProjectileLifetime);
            projectileHitRadiusConfigured = Mathf.Max(0.1f, monster.ProjectileHitRadius);
            magazineCapacityConfigured = Mathf.Max(1, monster.MagazineCapacity);
            reloadDurationConfigured = Mathf.Max(0.1f, monster.ReloadDuration);
            shotIntervalConfigured = Mathf.Max(0.05f, monster.ShotInterval);
            statusChanceConfigured = Mathf.Clamp01(monster.StatusChance);
            EnsureAnchorVisuals();
        }

        private static string ResolveSelectedActiveSkillId(MonsterDefinition monster)
        {
            if (monster == null || monster.ActiveSkills == null)
            {
                return string.Empty;
            }

            for (var i = 0; i < monster.ActiveSkills.Length; i++)
            {
                var skill = monster.ActiveSkills[i];
                if (skill != null && skill.Slot == SkillSlot.A && !string.IsNullOrWhiteSpace(skill.SkillId))
                {
                    return skill.SkillId;
                }
            }

            return string.Empty;
        }

        private static string ResolveSelectedPassiveSkillId(MonsterDefinition monster)
        {
            if (monster == null || monster.PassiveSkills == null)
            {
                return string.Empty;
            }

            for (var i = 0; i < monster.PassiveSkills.Length; i++)
            {
                var passive = monster.PassiveSkills[i];
                if (passive != null && passive.Slot == SkillSlot.F && !string.IsNullOrWhiteSpace(passive.PassiveId))
                {
                    return passive.PassiveId;
                }
            }

            return string.Empty;
        }

        private void ApplyPersistedRewardState(RunSession session)
        {
            if (session == null)
            {
                return;
            }

            baseDamageConfigured *= session.DamageMultiplier > 0f ? session.DamageMultiplier : 1f;
            magazineCapacityConfigured = Mathf.Max(1, magazineCapacityConfigured + session.MagazineBonus);
            shotIntervalConfigured = Mathf.Max(0.05f, shotIntervalConfigured * (session.ShotIntervalMultiplier > 0f ? session.ShotIntervalMultiplier : 1f));
            reloadDurationConfigured = Mathf.Max(0.25f, reloadDurationConfigured * (session.ReloadDurationMultiplier > 0f ? session.ReloadDurationMultiplier : 1f));
            unitMaxHealthConfigured = Mathf.Max(1f, unitMaxHealthConfigured + session.MaxHealthBonus);
            statusChanceConfigured = Mathf.Clamp01(statusChanceConfigured + session.StatusChanceBonus);
        }

        private void EnsureAnchorVisuals()
        {
            EnsureSpriteRenderer(nexusAnchor, nexusColor, new Vector2(1.8f, 1.8f), 15, nexusSprite);
            EnsureSpriteRenderer(eveAnchor, Color.white, new Vector2(1.25f, 1.25f), 20, selectedUnitSprite);
            EnsureSpriteRenderer(enemySpawnAnchor, spawnMarkerColor, new Vector2(0.65f, 0.65f), 5);
            EnsureSpriteRenderer(inputTargetAnchor, inputMarkerColor, new Vector2(0.85f, 0.85f), 10);
            EnsureSelectedMonsterStatusVisuals();
        }

        private SpriteRenderer EnsureSpriteRenderer(Transform target, Color color, Vector2 size, int sortingOrder, Sprite sprite = null)
        {
            if (target == null)
            {
                return null;
            }

            var renderer = target.GetComponent<SpriteRenderer>();
            if (renderer == null)
            {
                renderer = target.gameObject.AddComponent<SpriteRenderer>();
                target.localScale = new Vector3(size.x, size.y, 1f);
            }

            renderer.sprite = sprite != null ? sprite : GetSharedSprite();
            renderer.color = color;
            renderer.sortingOrder = sortingOrder;
            return renderer;
        }

        private Vector3 GetBattlefieldCenter()
        {
            return new Vector3((fieldSize.x - 1f) * 0.5f, (fieldSize.y - 1f) * 0.5f, 1f);
        }

        private void EnsureBattlefieldBackgroundVisual()
        {
            if (battlefieldBackgroundAnchor == null)
            {
                return;
            }

            var renderer = battlefieldBackgroundAnchor.GetComponent<SpriteRenderer>();
            if (renderer == null)
            {
                renderer = battlefieldBackgroundAnchor.gameObject.AddComponent<SpriteRenderer>();
            }

            renderer.sprite = battlefieldBackgroundSprite != null ? battlefieldBackgroundSprite : GetSharedSprite();
            renderer.color = Color.white;
            renderer.sortingOrder = -50;

            if (autoFitBattlefieldBackgroundToField)
            {
                var bounds = renderer.sprite != null ? renderer.sprite.bounds.size : Vector3.one;
                var width = Mathf.Max(0.01f, bounds.x);
                var height = Mathf.Max(0.01f, bounds.y);
                battlefieldBackgroundAnchor.localScale = new Vector3(fieldSize.x / width, fieldSize.y / height, 1f);
            }
        }

        private void EnsureSelectedMonsterStatusVisuals()
        {
            if (eveAnchor == null)
            {
                return;
            }

            var layout = ResolveSelectedMonsterStatusLayout();

            selectedMonsterNameLabel = EnsureStatusLabel(
                eveAnchor,
                "MonsterNameLabel",
                layout.NameLocalPosition,
                layout.TextScale,
                37);
            selectedMonsterHpLabel = EnsureStatusLabel(
                eveAnchor,
                "MonsterHpLabel",
                layout.HpTextLocalPosition,
                layout.TextScale,
                36);
            selectedMonsterHpBarFill = CreateHpBar(
                eveAnchor,
                "MonsterHpBar",
                layout.BarLocalPosition,
                layout.BarWidth,
                layout.BarHeight,
                Color.red,
                34);
            selectedMonsterShieldBarFill = CreateShieldBarFill(eveAnchor, "MonsterHpBar", layout.BarHeight, 36);
            ConfigureHpBarLayout(selectedMonsterHpBarFill, layout.BarLocalPosition, layout.BarWidth, layout.BarHeight);
            UpdateSelectedMonsterStatusVisuals();
        }

        private static TextMesh EnsureStatusLabel(Transform parent, string labelName, Vector3 localPosition, Vector3 localScale, int sortingOrder)
        {
            var labelTransform = parent.Find(labelName);
            if (labelTransform == null)
            {
                labelTransform = new GameObject(labelName).transform;
                labelTransform.SetParent(parent, false);
            }

            labelTransform.localPosition = localPosition;
            labelTransform.localScale = localScale;

            var label = labelTransform.GetComponent<TextMesh>();
            if (label == null)
            {
                label = labelTransform.gameObject.AddComponent<TextMesh>();
            }

            label.anchor = TextAnchor.MiddleCenter;
            label.alignment = TextAlignment.Center;
            label.fontSize = 32;
            label.color = Color.white;

            var renderer = label.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.sortingOrder = sortingOrder;
            }

            return label;
        }

        private void UpdateSelectedMonsterStatusVisuals()
        {
            var current = Application.isPlaying ? unitCurrentHealth : unitMaxHealthConfigured;
            var shield = Application.isPlaying ? unitShieldValue : 0f;
            SyncSelectedUnitRuntimeStats();

            if (selectedMonsterNameLabel != null)
            {
                selectedMonsterNameLabel.text = selectedMonsterName;
            }

            if (selectedMonsterHpLabel != null)
            {
                selectedMonsterHpLabel.text = shield > 0f
                    ? $"HP {Mathf.CeilToInt(Mathf.Max(0f, current))}/{Mathf.CeilToInt(unitMaxHealthConfigured)} SH {Mathf.CeilToInt(shield)}"
                    : $"HP {Mathf.CeilToInt(Mathf.Max(0f, current))}/{Mathf.CeilToInt(unitMaxHealthConfigured)}";
            }

            UpdateHpShieldBarFill(
                selectedMonsterHpBarFill,
                selectedMonsterShieldBarFill,
                current,
                unitMaxHealthConfigured,
                shield);
        }

        private SelectedMonsterStatusLayout ResolveSelectedMonsterStatusLayout()
        {
            if (!autoLayoutSelectedMonsterStatus)
            {
                var manualScale = new Vector3(
                    Mathf.Max(0.01f, selectedMonsterStatusManualTextScale.x),
                    Mathf.Max(0.01f, selectedMonsterStatusManualTextScale.y),
                    Mathf.Max(0.01f, selectedMonsterStatusManualTextScale.z));
                return new SelectedMonsterStatusLayout(
                    selectedMonsterStatusManualBarLocalPosition,
                    selectedMonsterStatusManualHpTextLocalPosition,
                    selectedMonsterStatusManualNameLocalPosition,
                    manualScale,
                    Mathf.Max(0.1f, selectedMonsterStatusManualBarWidth),
                    Mathf.Max(0.01f, selectedMonsterStatusManualBarHeight));
            }

            var spriteRenderer = eveAnchor != null ? eveAnchor.GetComponent<SpriteRenderer>() : null;
            var spriteSize = spriteRenderer != null && spriteRenderer.sprite != null
                ? spriteRenderer.sprite.bounds.size
                : Vector3.one;
            var spriteWidth = Mathf.Max(0.5f, spriteSize.x);
            var spriteHeight = Mathf.Max(0.5f, spriteSize.y);
            var textScaleValue = Mathf.Clamp(
                Mathf.Max(spriteWidth, spriteHeight) * selectedMonsterStatusAutoTextScaleMultiplier,
                selectedMonsterStatusAutoMinTextScale,
                selectedMonsterStatusAutoMaxTextScale);
            var textScale = new Vector3(textScaleValue, textScaleValue, 1f);
            var barWidth = Mathf.Clamp(
                spriteWidth * selectedMonsterStatusAutoBarWidthMultiplier,
                selectedMonsterStatusAutoMinBarWidth,
                Mathf.Max(selectedMonsterStatusAutoMinBarWidth, selectedMonsterStatusAutoMaxBarWidth));
            var barHeight = Mathf.Max(0.01f, selectedMonsterStatusAutoBarHeight);
            var barY = (spriteHeight * 0.5f) + selectedMonsterStatusAutoTopPadding;
            var hpTextY = barY + barHeight + selectedMonsterStatusAutoHpTextGap;
            var nameY = hpTextY + textScaleValue + selectedMonsterStatusAutoNameGap;

            return new SelectedMonsterStatusLayout(
                new Vector3(0f, barY, 0f),
                new Vector3(0f, hpTextY, 0f),
                new Vector3(0f, nameY, 0f),
                textScale,
                barWidth,
                barHeight);
        }

        private void UpdateDamagePopups()
        {
            for (var i = damagePopups.Count - 1; i >= 0; i--)
            {
                var popup = damagePopups[i];
                if (popup == null || popup.GameObject == null || popup.Transform == null || popup.Text == null)
                {
                    damagePopups.RemoveAt(i);
                    continue;
                }

                popup.RemainingDuration = Mathf.Max(0f, popup.RemainingDuration - Time.deltaTime);
                popup.Transform.position += Vector3.up * popup.RiseSpeed * Time.deltaTime;

                var color = popup.Text.color;
                color.a = popup.TotalDuration > 0f ? Mathf.Clamp01(popup.RemainingDuration / popup.TotalDuration) : 0f;
                popup.Text.color = color;

                if (popup.RemainingDuration > 0f)
                {
                    continue;
                }

                Destroy(popup.GameObject);
                damagePopups.RemoveAt(i);
            }
        }

        private void ClearDamagePopupRuntime()
        {
            for (var i = damagePopups.Count - 1; i >= 0; i--)
            {
                var popup = damagePopups[i];
                if (popup == null || popup.GameObject == null)
                {
                    continue;
                }

                Destroy(popup.GameObject);
            }

            damagePopups.Clear();
        }

        private void SpawnDamagePopupForEnemy(EnemyRuntime enemy, float damageAmount)
        {
            SpawnDamagePopupForEnemy(enemy, damageAmount, null);
        }

        private void SpawnDamagePopupForEnemy(EnemyRuntime enemy, float damageAmount, DamageAttribute? attribute)
        {
            if (enemy == null || enemy.Transform == null || damageAmount <= 0f)
            {
                return;
            }

            var basePosition = enemy.Label != null
                ? enemy.Label.transform.position
                : enemy.Transform.position + Vector3.up * (enemy.IsBoss ? 1.25f : 0.9f);
            SpawnDamagePopup(
                basePosition + new Vector3(0f, enemy.IsBoss ? 0.18f : 0.12f, 0f),
                FormatDamagePopupAmount(damageAmount, attribute),
                enemy.Label);
        }

        private void SpawnDamagePopupForEnemy(EnemyRuntime enemy, string popupText)
        {
            if (enemy == null || enemy.Transform == null || string.IsNullOrWhiteSpace(popupText))
            {
                return;
            }

            var basePosition = enemy.Label != null
                ? enemy.Label.transform.position
                : enemy.Transform.position + Vector3.up * (enemy.IsBoss ? 1.25f : 0.9f);
            SpawnDamagePopup(basePosition + new Vector3(0f, enemy.IsBoss ? 0.18f : 0.12f, 0f), popupText, enemy.Label);
        }

        private void SpawnDamagePopupForSelectedMonster(float damageAmount)
        {
            SpawnDamagePopupForSelectedMonster(damageAmount, null);
        }

        private void SpawnDamagePopupForSelectedMonster(float damageAmount, DamageAttribute? attribute)
        {
            if (damageAmount <= 0f)
            {
                return;
            }

            Vector3 basePosition;
            if (selectedMonsterNameLabel != null)
            {
                basePosition = selectedMonsterNameLabel.transform.position;
            }
            else if (selectedMonsterHpLabel != null)
            {
                basePosition = selectedMonsterHpLabel.transform.position;
            }
            else if (eveAnchor != null)
            {
                basePosition = eveAnchor.position + Vector3.up * 1.05f;
            }
            else
            {
                return;
            }

            SpawnDamagePopup(
                basePosition + new Vector3(0f, 0.14f, 0f),
                FormatDamagePopupAmount(damageAmount, attribute),
                selectedMonsterHpLabel ?? selectedMonsterNameLabel);
        }

        private void SpawnDamagePopup(Vector3 worldPosition, string popupTextValue, TextMesh template)
        {
            var parent = projectileRoot != null ? projectileRoot : transform;
            if (parent == null)
            {
                return;
            }

            var popupObject = new GameObject("DamagePopup");
            popupObject.transform.SetParent(parent, false);
            popupObject.transform.position = new Vector3(worldPosition.x, worldPosition.y, 0f);
            popupObject.transform.localScale = ResolveDamagePopupScale(template);

            var popupText = popupObject.AddComponent<TextMesh>();
            ConfigureDamagePopupText(popupText, template);
            popupText.text = popupTextValue;
            popupText.color = Color.white;

            var renderer = popupText.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                var templateRenderer = template != null ? template.GetComponent<MeshRenderer>() : null;
                if (templateRenderer != null && templateRenderer.sharedMaterial != null)
                {
                    renderer.sharedMaterial = templateRenderer.sharedMaterial;
                }

                renderer.sortingOrder = templateRenderer != null ? templateRenderer.sortingOrder + 2 : 38;
            }

            damagePopups.Add(new DamagePopupRuntime
            {
                GameObject = popupObject,
                Transform = popupObject.transform,
                Text = popupText,
                RemainingDuration = DamagePopupDuration,
                TotalDuration = DamagePopupDuration,
                RiseSpeed = DamagePopupRiseDistance / DamagePopupDuration
            });
        }

        private static void ConfigureDamagePopupText(TextMesh popupText, TextMesh template)
        {
            if (popupText == null)
            {
                return;
            }

            if (template != null)
            {
                popupText.font = template.font;
                popupText.fontSize = template.fontSize;
                popupText.fontStyle = template.fontStyle;
                popupText.characterSize = template.characterSize;
                popupText.anchor = template.anchor;
                popupText.alignment = template.alignment;
                popupText.tabSize = template.tabSize;
                popupText.lineSpacing = template.lineSpacing;
                popupText.richText = template.richText;
                return;
            }

            popupText.anchor = TextAnchor.MiddleCenter;
            popupText.alignment = TextAlignment.Center;
            popupText.fontSize = 32;
            popupText.color = Color.white;
        }

        private static Vector3 ResolveDamagePopupScale(TextMesh template)
        {
            if (template != null)
            {
                var scale = template.transform.lossyScale;
                if (scale.x > 0f && scale.y > 0f)
                {
                    return new Vector3(scale.x, scale.y, 1f);
                }
            }

            return new Vector3(0.12f, 0.12f, 1f);
        }

        private static void ConfigureHpBarLayout(SpriteRenderer hpFill, Vector3 localPosition, float width, float height)
        {
            if (hpFill == null)
            {
                return;
            }

            var root = hpFill.transform.parent;
            if (root == null)
            {
                return;
            }

            root.localPosition = localPosition;
            root.localScale = new Vector3(width, 1f, 1f);

            var background = root.Find("Background");
            if (background != null)
            {
                background.localPosition = Vector3.zero;
                background.localScale = new Vector3(1f, height, 1f);
            }

            var fill = root.Find("Fill");
            if (fill != null)
            {
                fill.localPosition = new Vector3(fill.localPosition.x, 0f, -0.01f);
                fill.localScale = new Vector3(fill.localScale.x, height, 1f);
            }

            var shield = root.Find("Shield");
            if (shield != null)
            {
                shield.localPosition = new Vector3(shield.localPosition.x, 0f, -0.02f);
                shield.localScale = new Vector3(shield.localScale.x, height, 1f);
            }
        }

        private static string FormatDamagePopupAmount(float damageAmount)
        {
            return Mathf.Max(1, Mathf.RoundToInt(damageAmount)).ToString();
        }

        private static string FormatDamagePopupAmount(float damageAmount, DamageAttribute? attribute)
        {
            var amount = FormatDamagePopupAmount(damageAmount);
            // Debug-only attribute suffix for combat damage popup inspection.
            return attribute.HasValue ? $"{amount}({GetDamageAttributeKoreanLabel(attribute.Value)})" : amount;
        }

        private static string FormatDamagePopupTerm(float damageAmount, DamageAttribute attribute)
        {
            return FormatDamagePopupAmount(damageAmount, attribute);
        }

        private static string GetDamageAttributeKoreanLabel(DamageAttribute attribute)
        {
            switch (attribute)
            {
                case DamageAttribute.Physical:
                    return "물리";
                case DamageAttribute.Fire:
                    return "화염";
                case DamageAttribute.Lightning:
                    return "번개";
                case DamageAttribute.Ice:
                    return "얼음";
                case DamageAttribute.Darkness:
                    return "어둠";
                case DamageAttribute.Holy:
                    return "신성";
                default:
                    return attribute.ToString();
            }
        }

        private static Sprite GetSharedSprite()
        {
            if (sharedSprite != null)
            {
                return sharedSprite;
            }

            var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.HideAndDontSave
            };
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();
            sharedSprite = Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
            sharedSprite.hideFlags = HideFlags.HideAndDontSave;
            return sharedSprite;
        }
    }
}
