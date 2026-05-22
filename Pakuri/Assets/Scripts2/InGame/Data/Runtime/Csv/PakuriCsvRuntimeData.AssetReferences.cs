using System;
using System.Collections.Generic;

namespace Pakuri.Data
{
    public static partial class PakuriCsvRuntimeData
    {
        private readonly struct ReferencedAssetPath
        {
            public ReferencedAssetPath(string assetPath, string ownerLabel)
            {
                AssetPath = assetPath;
                OwnerLabel = ownerLabel;
            }

            public string AssetPath { get; }
            public string OwnerLabel { get; }
        }

        private sealed class ReferencedAssetSet
        {
            private readonly HashSet<string> spritePathLookup = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            private readonly HashSet<string> prefabPathLookup = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            public List<ReferencedAssetPath> SpritePaths { get; } = new List<ReferencedAssetPath>();
            public List<ReferencedAssetPath> PrefabPaths { get; } = new List<ReferencedAssetPath>();

            public void AddSprite(string assetPath, string ownerLabel)
            {
                Add(assetPath, ownerLabel, spritePathLookup, SpritePaths);
            }

            public void AddPrefab(string assetPath, string ownerLabel)
            {
                Add(assetPath, ownerLabel, prefabPathLookup, PrefabPaths);
            }

            private static void Add(
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

        private static ReferencedAssetSet CollectReferencedAssets(SourceModel model)
        {
            var assets = new ReferencedAssetSet();
            if (model == null)
            {
                return assets;
            }

            foreach (var skill in model.Skills.Values)
            {
                assets.AddSprite(skill.SkillIconPath, $"Skill '{skill.Id}' skill_icon_path");
                assets.AddPrefab(skill.Status.StatusEffectPrefabPath, $"Skill '{skill.Id}' status_effect_prefab_path");
            }

            foreach (var choice in model.SkillChoices.Values)
            {
                assets.AddSprite(choice.SkillIconPath, $"Skill choice '{choice.Id}' skill_icon_path");
                assets.AddPrefab(choice.SkillEffectPrefabPath, $"Skill choice '{choice.Id}' skill_effect_prefab_path");
            }

            foreach (var effect in model.SkillEffects.Values)
            {
                assets.AddPrefab(effect.SkillEffectPrefabPath, $"Skill effect '{effect.Id}' skill_effect_prefab_path");
                assets.AddPrefab(effect.Status.StatusEffectPrefabPath, $"Skill effect '{effect.Id}' status_effect_prefab_path");
            }

            foreach (var trigger in model.SkillTriggers.Values)
            {
                assets.AddPrefab(trigger.SkillEffectPrefabPath, $"Skill trigger '{trigger.Id}' skill_effect_prefab_path");
            }

            foreach (var status in model.StatusEffects.Values)
            {
                assets.AddPrefab(status.StatusEffectPrefabPath, $"Status effect '{status.Id}' status_effect_prefab_path");
            }

            foreach (var enemy in model.StageOneEnemies.Values)
            {
                assets.AddSprite(enemy.UnitSpritePath, $"Enemy '{enemy.Id}' unit_sprite_path");
                assets.AddSprite(enemy.ProjectileSpritePath, $"Enemy '{enemy.Id}' projectile_sprite_path");
            }

            return assets;
        }
    }
}
