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
            targetCamera.backgroundColor = battlefieldBackgroundColor;

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
            EnsureSpriteRenderer(nexusAnchor, nexusColor, new Vector2(1.8f, 1.8f), 15);
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
            }

            renderer.sprite = sprite != null ? sprite : GetSharedSprite();
            renderer.color = color;
            renderer.sortingOrder = sortingOrder;
            target.localScale = new Vector3(size.x, size.y, 1f);
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

            battlefieldBackgroundAnchor.position = GetBattlefieldCenter();
            var renderer = battlefieldBackgroundAnchor.GetComponent<SpriteRenderer>();
            if (renderer == null)
            {
                renderer = battlefieldBackgroundAnchor.gameObject.AddComponent<SpriteRenderer>();
            }

            renderer.sprite = battlefieldBackgroundSprite != null ? battlefieldBackgroundSprite : GetSharedSprite();
            renderer.color = battlefieldBackgroundColor;
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

            selectedMonsterLabel = EnsureStatusLabel(
                eveAnchor,
                "MonsterHpLabel",
                new Vector3(0f, 1.05f, 0f),
                new Vector3(0.12f, 0.12f, 1f),
                36);
            selectedMonsterHpBarFill = CreateHpBar(
                eveAnchor,
                "MonsterHpBar",
                new Vector3(0f, 0.83f, 0f),
                1.3f,
                0.08f,
                selectedUnitColor,
                34);
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
            if (selectedMonsterLabel != null)
            {
                var current = Application.isPlaying ? unitCurrentHealth : unitMaxHealthConfigured;
                selectedMonsterLabel.text = $"{selectedMonsterName}\nHP {Mathf.CeilToInt(Mathf.Max(0f, current))}/{Mathf.CeilToInt(unitMaxHealthConfigured)}";
            }

            UpdateHpBarFill(selectedMonsterHpBarFill, Application.isPlaying ? unitCurrentHealth : unitMaxHealthConfigured, unitMaxHealthConfigured);
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
