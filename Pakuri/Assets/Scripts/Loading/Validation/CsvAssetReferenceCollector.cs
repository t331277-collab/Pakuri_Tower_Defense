/*
 * 역할: CSV 에셋 참조 수집.
 * 책임: 런타임 카탈로그가 제공해야 하는 Sprite·Prefab·AnimatorController 경로를 모두 수집한다.
 */

using System;
using System.Collections.Generic;
using static Pakuri.Data.CsvRowParser;
using static Pakuri.Data.CsvSourceModel;
using static Pakuri.Data.SkillGraphParser;

namespace Pakuri.Data
{

    internal static class CsvAssetReferenceCollector
    {

        /// ReferencedAssetPath 처리에 함께 전달되는 값들을 묶는다.
        internal readonly struct ReferencedAssetPath
        {

            public ReferencedAssetPath(string assetPath, string ownerLabel)
            {
                AssetPath = assetPath;
                OwnerLabel = ownerLabel;
            }

            public string AssetPath { get; }
            public string OwnerLabel { get; }
        }

        internal class ReferencedAssetSet
        {
            internal readonly HashSet<string> spritePathLookup = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            internal readonly HashSet<string> prefabPathLookup = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            internal readonly HashSet<string> animatorControllerPathLookup = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            public List<ReferencedAssetPath> SpritePaths { get; } = new List<ReferencedAssetPath>();
            public List<ReferencedAssetPath> PrefabPaths { get; } = new List<ReferencedAssetPath>();
            public List<ReferencedAssetPath> AnimatorControllerPaths { get; } = new List<ReferencedAssetPath>();

            public void AddSprite(string assetPath, string ownerLabel)
            {
                Add(assetPath, ownerLabel, spritePathLookup, SpritePaths);
            }

            public void AddPrefab(string assetPath, string ownerLabel)
            {
                Add(assetPath, ownerLabel, prefabPathLookup, PrefabPaths);
            }

            public void AddAnimatorController(string assetPath, string ownerLabel)
            {
                Add(assetPath, ownerLabel, animatorControllerPathLookup, AnimatorControllerPaths);
            }

            internal static void Add(
                string assetPath,
                string ownerLabel,
                HashSet<string> lookup,
                List<ReferencedAssetPath> paths)
            {
                if (string.IsNullOrWhiteSpace(assetPath))
                {
                    return;
                }

                var normalizedPath = assetPath.Trim().Replace('\\', '/');
                if (lookup.Add(normalizedPath))
                {
                    paths.Add(new ReferencedAssetPath(normalizedPath, ownerLabel));
                }
            }
        }

        internal static ReferencedAssetSet CollectReferencedAssets(SourceModel model)
        {
            var assets = new ReferencedAssetSet();
            if (model == null)
            {
                return assets;
            }

            foreach (var artifact in model.Artifacts.Values)
            {
                assets.AddSprite(artifact.IconPath, $"Artifact '{artifact.Name}' artifact_icon");
            }

            foreach (var synergy in model.ArtifactSynergies.Values)
            {
                assets.AddSprite(synergy.IconPath, $"Artifact synergy '{synergy.Name}' Icon_Image");
            }

            foreach (var skill in model.Skills.Values)
            {
                assets.AddSprite(skill.SkillIconPath, $"Skill '{skill.Name}' skill_icon_path");
                assets.AddPrefab(skill.SkillEffectPrefabPath, $"Skill '{skill.Name}' skill_effect_prefab_path");
                assets.AddPrefab(skill.Status.StatusEffectPrefabPath, $"Skill '{skill.Name}' status_effect_prefab_path");
                assets.AddSprite(skill.RuntimeVisualSpritePath, $"Skill '{skill.Name}' runtime_visual_sprite_path");
                assets.AddAnimatorController(
                    skill.RuntimeVisualAnimatorControllerPath,
                    $"Skill '{skill.Name}' runtime_visual_animator_controller_path");
                assets.AddSprite(
                    skill.RuntimeImpactVisualSpritePath,
                    $"Skill '{skill.Name}' runtime_impact_visual_sprite_path");
                assets.AddAnimatorController(
                    skill.RuntimeImpactVisualAnimatorControllerPath,
                    $"Skill '{skill.Name}' runtime_impact_visual_animator_controller_path");
            }

            foreach (var summon in model.Summons.Values)
            {
                assets.AddSprite(summon.MonsterIconImagePath, $"Summon '{summon.Name}' MonsterIconImage");
                assets.AddSprite(summon.ImagePath, $"Summon '{summon.Name}' Image");
            }

            foreach (var skill in model.SummonSkills.Values)
            {
                assets.AddSprite(skill.SkillIconPath, $"Summon skill '{skill.Name}' skill_icon_path");
                assets.AddPrefab(skill.SkillEffectPrefabPath, $"Summon skill '{skill.Name}' skill_effect_prefab_path");
                assets.AddPrefab(skill.Status.StatusEffectPrefabPath, $"Summon skill '{skill.Name}' status_effect_prefab_path");
                assets.AddSprite(skill.RuntimeVisualSpritePath, $"Summon skill '{skill.Name}' runtime_visual_sprite_path");
                assets.AddAnimatorController(
                    skill.RuntimeVisualAnimatorControllerPath,
                    $"Summon skill '{skill.Name}' runtime_visual_animator_controller_path");
                assets.AddSprite(
                    skill.RuntimeImpactVisualSpritePath,
                    $"Summon skill '{skill.Name}' runtime_impact_visual_sprite_path");
                assets.AddAnimatorController(
                    skill.RuntimeImpactVisualAnimatorControllerPath,
                    $"Summon skill '{skill.Name}' runtime_impact_visual_animator_controller_path");
            }

            foreach (var enemySkill in model.EnemyBaseSkills.Values)
            {
                SkillRow skill = null;
                if (enemySkill != null)
                {
                    skill = enemySkill.Skill;
                }
                if (skill == null)
                {
                    continue;
                }

                assets.AddSprite(skill.RuntimeVisualSpritePath, $"Enemy base skill '{skill.Name}' runtime_visual_sprite_path");
                assets.AddAnimatorController(
                    skill.RuntimeVisualAnimatorControllerPath,
                    $"Enemy base skill '{skill.Name}' runtime_visual_animator_controller_path");
            }

            foreach (var choice in model.SkillChoices.Values)
            {
                assets.AddSprite(choice.SkillIconPath, $"Skill choice '{choice.Name}' skill_icon_path");
            }

            foreach (var param in model.SkillNodeParams)
            {
                if (param == null || param.ValueType != SkillNodeValueType.AssetPath)
                {
                    continue;
                }

                if (param.ParamKey != null
                    && param.ParamKey.IndexOf("sprite", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    assets.AddSprite(param.Value, $"Skill node param '{param.NodeName}.{param.ParamKey}'");
                }
                else if (param.ParamKey != null
                    && param.ParamKey.IndexOf("animator", StringComparison.OrdinalIgnoreCase) >= 0
                    && param.ParamKey.IndexOf("controller", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    assets.AddAnimatorController(param.Value, $"Skill node param '{param.NodeName}.{param.ParamKey}'");
                }
                else
                {
                    assets.AddPrefab(param.Value, $"Skill node param '{param.NodeName}.{param.ParamKey}'");
                }
            }

            foreach (var status in model.StatusEffects.Values)
            {
                assets.AddPrefab(status.StatusEffectPrefabPath, $"Status effect '{status.Name}' status_effect_prefab_path");
            }

            foreach (var monster in model.Monsters.Values)
            {
                assets.AddSprite(monster.MonsterIconImagePath, $"Monster '{monster.Name}' MonsterIconImage");
                assets.AddSprite(monster.ImagePath, $"Monster '{monster.Name}' Image");
            }

            foreach (var enemy in model.Enemies.Values)
            {
                assets.AddSprite(enemy.ImagePath, $"Enemy '{enemy.Name}' Image");
            }

            return assets;
        }
    }
}
