using System;
using System.Collections.Generic;
using static Pakuri.Data.CsvSourceModel;
using static Pakuri.Data.SkillGraphBuilder;


namespace Pakuri.Data
{
    /*
     * CSV가 참조하는 Sprite, Prefab, Animator 경로를 수집하고 중복을 정리한다.
     */
    internal static class CsvAssetReferenceCollector
    {
        internal readonly struct ReferencedAssetPath
        {
            /*
             * 참조 자산 경로를 구성한다.
             */
            public ReferencedAssetPath(string assetPath, string ownerLabel)
            {
                AssetPath = assetPath;
                OwnerLabel = ownerLabel;
            }

            public string AssetPath { get; }
            public string OwnerLabel { get; }
        }

        /*
         * 종류별 자산 경로와 중복 확인용 경로 집합을 보관한다.
         */
        internal sealed class ReferencedAssetSet
        {
            internal readonly HashSet<string> spritePathLookup = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            internal readonly HashSet<string> prefabPathLookup = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            internal readonly HashSet<string> animatorControllerPathLookup = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            public List<ReferencedAssetPath> SpritePaths { get; } = new List<ReferencedAssetPath>();
            public List<ReferencedAssetPath> PrefabPaths { get; } = new List<ReferencedAssetPath>();
            public List<ReferencedAssetPath> AnimatorControllerPaths { get; } = new List<ReferencedAssetPath>();

            /*
             * 항목을 대상 목록에 추가한다.
             */
            public void AddSprite(string assetPath, string ownerLabel)
            {
                Add(assetPath, ownerLabel, spritePathLookup, SpritePaths);
            }

            /*
             * 항목을 대상 목록에 추가한다.
             */
            public void AddPrefab(string assetPath, string ownerLabel)
            {
                Add(assetPath, ownerLabel, prefabPathLookup, PrefabPaths);
            }

            /*
             * 항목을 대상 목록에 추가한다.
             */
            public void AddAnimatorController(string assetPath, string ownerLabel)
            {
                Add(assetPath, ownerLabel, animatorControllerPathLookup, AnimatorControllerPaths);
            }

            /*
             * 항목을 대상 목록에 추가한다.
             */
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

        /*
         * 원본 모델이 사용하는 Sprite, Prefab, Animator 참조를 모은다.
         */
        internal static ReferencedAssetSet CollectReferencedAssets(SourceModel model)
        {
            var assets = new ReferencedAssetSet();
            if (model == null)
            {
                return assets;
            }

            foreach (var skill in model.Skills.Values)
            {
                assets.AddSprite(skill.SkillIconPath, $"Skill '{skill.Id}' skill_icon_path");
                assets.AddPrefab(skill.SkillEffectPrefabPath, $"Skill '{skill.Id}' skill_effect_prefab_path");
                assets.AddPrefab(skill.Status.StatusEffectPrefabPath, $"Skill '{skill.Id}' status_effect_prefab_path");
                assets.AddSprite(skill.RuntimeVisualSpritePath, $"Skill '{skill.Id}' runtime_visual_sprite_path");
                assets.AddAnimatorController(
                    skill.RuntimeVisualAnimatorControllerPath,
                    $"Skill '{skill.Id}' runtime_visual_animator_controller_path");
                assets.AddSprite(
                    skill.RuntimeImpactVisualSpritePath,
                    $"Skill '{skill.Id}' runtime_impact_visual_sprite_path");
                assets.AddAnimatorController(
                    skill.RuntimeImpactVisualAnimatorControllerPath,
                    $"Skill '{skill.Id}' runtime_impact_visual_animator_controller_path");
            }

            foreach (var enemySkill in model.EnemyBaseSkills.Values)
            {
                var skill = enemySkill != null ? enemySkill.Skill : null;
                if (skill == null)
                {
                    continue;
                }

                assets.AddSprite(skill.RuntimeVisualSpritePath, $"Enemy base skill '{skill.Id}' runtime_visual_sprite_path");
                assets.AddAnimatorController(
                    skill.RuntimeVisualAnimatorControllerPath,
                    $"Enemy base skill '{skill.Id}' runtime_visual_animator_controller_path");
            }

            foreach (var choice in model.SkillChoices.Values)
            {
                assets.AddSprite(choice.SkillIconPath, $"Skill choice '{choice.Id}' skill_icon_path");
                assets.AddPrefab(choice.SkillEffectPrefabPath, $"Skill choice '{choice.Id}' skill_effect_prefab_path");
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
                    assets.AddSprite(param.Value, $"Skill node param '{param.NodeId}.{param.ParamKey}'");
                }
                else if (param.ParamKey != null
                    && param.ParamKey.IndexOf("animator", StringComparison.OrdinalIgnoreCase) >= 0
                    && param.ParamKey.IndexOf("controller", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    assets.AddAnimatorController(param.Value, $"Skill node param '{param.NodeId}.{param.ParamKey}'");
                }
                else
                {
                    assets.AddPrefab(param.Value, $"Skill node param '{param.NodeId}.{param.ParamKey}'");
                }
            }

            foreach (var trigger in model.SkillTriggers.Values)
            {
                assets.AddPrefab(trigger.SkillEffectPrefabPath, $"Skill trigger '{trigger.Id}' skill_effect_prefab_path");
                assets.AddSprite(trigger.RuntimeVisualSpritePath, $"Skill trigger '{trigger.Id}' runtime_visual_sprite_path");
                assets.AddAnimatorController(
                    trigger.RuntimeVisualAnimatorControllerPath,
                    $"Skill trigger '{trigger.Id}' runtime_visual_animator_controller_path");
            }

            foreach (var status in model.StatusEffects.Values)
            {
                assets.AddPrefab(status.StatusEffectPrefabPath, $"Status effect '{status.Id}' status_effect_prefab_path");
            }

            foreach (var monster in model.Monsters.Values)
            {
                assets.AddSprite(monster.MonsterIconImagePath, $"Monster '{monster.Id}' MonsterIconImage");
            }

            return assets;
        }
    }
}
