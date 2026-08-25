/*
 * 역할: CSV 참조로 생성된 런타임 에셋 조회.
 * 책임: 정규화된 에셋 경로로 Sprite·Prefab·AnimatorController·AnimationClip을 색인한다.
 */

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Pakuri.Data
{

    public class CsvRuntimeCatalog : ScriptableObject
    {

        /// SpriteEntry 처리에 함께 전달되는 값들을 묶는다.
        [Serializable]
        public struct SpriteEntry
        {
            public string AssetPath;
            public Sprite Asset;
        }

        /// PrefabEntry 처리에 함께 전달되는 값들을 묶는다.
        [Serializable]
        public struct PrefabEntry
        {
            public string AssetPath;
            public GameObject Asset;
        }

        /// AnimatorControllerEntry 처리에 함께 전달되는 값들을 묶는다.
        [Serializable]
        public struct AnimatorControllerEntry
        {
            public string AssetPath;
            public RuntimeAnimatorController Asset;
        }

        /// AnimationClipEntry 처리에 함께 전달되는 값들을 묶는다.
        [Serializable]
        public struct AnimationClipEntry
        {
            public string AssetPath;
            public AnimationClip Asset;
        }

        [Header("CSV Sources")]
        public TextAsset CatalogMonsters;
        public TextAsset Monsters;
        public TextAsset MonsterRewardChoices;
        public TextAsset[] MonsterSkillsProjectileFiles;
        public TextAsset[] MonsterSkillsLineAttackFiles;
        public TextAsset[] MonsterSkillsAreaAttackFiles;
        public TextAsset[] MonsterSkillsSingleAttackFiles;
        public TextAsset[] MonsterSkillsBuffFiles;
        public TextAsset[] MonsterSkillsPassiveFiles;
        public TextAsset MonsterSkillNodeDefinitions;
        public TextAsset MonsterSkillNodeDefinitionParams;
        public TextAsset[] MonsterSkillGraphNodeFiles;
        public TextAsset[] MonsterSkillTriggerFiles;
        public TextAsset[] MonsterSkillChoicesProjectileFiles;
        public TextAsset[] MonsterSkillChoicesLineAttackFiles;
        public TextAsset[] MonsterSkillChoicesAreaAttackFiles;
        public TextAsset[] MonsterSkillChoicesSingleAttackFiles;
        public TextAsset[] MonsterSkillChoicesBuffFiles;
        public TextAsset[] MonsterSkillChoicesPassiveFiles;
        public TextAsset StatusEffects;
        public TextAsset Enemies;
        public TextAsset[] EnemySkillBaseFiles;
        public TextAsset[] EnemySkillTriggerFiles;

        [Header("Artifact And Summon Sources")]
        public TextAsset Artifacts;
        public TextAsset ArtifactSynergies;
        public TextAsset ArtifactEffects;
        public TextAsset ArtifactSynergyEffects;
        public TextAsset[] ArtifactSkillGraphNodeFiles;
        public TextAsset[] ArtifactSkillTriggerFiles;
        public TextAsset SummonUnits;
        public TextAsset SummonSkills;

        [Header("Stage Sources")]
        public TextAsset StageDay;
        public TextAsset Stage1Encounter;
        public TextAsset Stage1Reward;
        public TextAsset Stage2Encounter;
        public TextAsset Stage2Reward;

        [Header("Unity Assets")]
        public SpriteEntry[] Sprites = Array.Empty<SpriteEntry>();
        public PrefabEntry[] Prefabs = Array.Empty<PrefabEntry>();
        public AnimatorControllerEntry[] AnimatorControllers = Array.Empty<AnimatorControllerEntry>();
        public AnimationClipEntry[] AnimationClips = Array.Empty<AnimationClipEntry>();

        private Dictionary<string, Sprite> spriteLookup;
        private Dictionary<string, GameObject> prefabLookup;
        private Dictionary<string, RuntimeAnimatorController> animatorControllerLookup;
        private Dictionary<string, AnimationClip> animationClipLookup;

        public bool TryGetSprite(string assetPath, out Sprite sprite)
        {
            EnsureLookups();
            return spriteLookup.TryGetValue(Normalize(assetPath), out sprite);
        }

        public bool TryGetPrefab(string assetPath, out GameObject prefab)
        {
            EnsureLookups();
            return prefabLookup.TryGetValue(Normalize(assetPath), out prefab);
        }

        public bool TryGetAnimatorController(
            string assetPath,
            out RuntimeAnimatorController animatorController)
        {
            EnsureLookups();
            return animatorControllerLookup.TryGetValue(Normalize(assetPath), out animatorController);
        }

        public bool HasSprite(string assetPath)
        {
            return TryGetSprite(assetPath, out _);
        }

        public bool HasPrefab(string assetPath)
        {
            return TryGetPrefab(assetPath, out _);
        }

        public bool HasAnimatorController(string assetPath)
        {
            return TryGetAnimatorController(assetPath, out _);
        }

        public bool TryGetAnimationClip(string assetPath, out AnimationClip animationClip)
        {
            EnsureLookups();
            return animationClipLookup.TryGetValue(Normalize(assetPath), out animationClip);
        }

        public bool HasAnimationClip(string assetPath)
        {
            return TryGetAnimationClip(assetPath, out _);
        }

        /// CSV를 다시 읽을 수 있도록 Asset lookup 캐시를 비운다.
        public void ResetLookups()
        {
            spriteLookup = null;
            prefabLookup = null;
            animatorControllerLookup = null;
            animationClipLookup = null;
        }

        /// Unity가 컴포넌트를 활성화할 때 구독과 활성 상태를 복원한다.
        private void OnEnable()
        {
            ResetLookups();
        }

        private void EnsureLookups()
        {
            if (spriteLookup != null
                && prefabLookup != null
                && animatorControllerLookup != null
                && animationClipLookup != null)
            {
                return;
            }

            spriteLookup = new Dictionary<string, Sprite>(StringComparer.OrdinalIgnoreCase);
            prefabLookup = new Dictionary<string, GameObject>(StringComparer.OrdinalIgnoreCase);
            animatorControllerLookup = new Dictionary<string, RuntimeAnimatorController>(StringComparer.OrdinalIgnoreCase);
            animationClipLookup = new Dictionary<string, AnimationClip>(StringComparer.OrdinalIgnoreCase);

            AddEntries(Sprites, spriteLookup, entry => entry.AssetPath, entry => entry.Asset);
            AddEntries(Prefabs, prefabLookup, entry => entry.AssetPath, entry => entry.Asset);
            AddEntries(
                AnimatorControllers,
                animatorControllerLookup,
                entry => entry.AssetPath,
                entry => entry.Asset);
            AddEntries(
                AnimationClips,
                animationClipLookup,
                entry => entry.AssetPath,
                entry => entry.Asset);
        }

        private static void AddEntries<TEntry, TAsset>(
            TEntry[] entries,
            Dictionary<string, TAsset> lookup,
            Func<TEntry, string> getPath,
            Func<TEntry, TAsset> getAsset)
            where TAsset : UnityEngine.Object
        {
            if (entries == null)
            {
                return;
            }

            for (var i = 0; i < entries.Length; i++)
            {
                var path = Normalize(getPath(entries[i]));
                var asset = getAsset(entries[i]);
                if (string.IsNullOrWhiteSpace(path) || asset == null || lookup.ContainsKey(path))
                {
                    continue;
                }

                lookup.Add(path, asset);
            }
        }

        private static string Normalize(string assetPath)
        {
            if (string.IsNullOrWhiteSpace(assetPath))
            {
                return string.Empty;
            }

            return assetPath.Trim().Replace('\\', '/');
        }
    }
}
