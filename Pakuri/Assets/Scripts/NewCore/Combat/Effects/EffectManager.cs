using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Pakuri.NewCore.Units.Models;
using UnityEngine;
using Object = UnityEngine.Object;

/* 스킬 시각 요청을 해석해 리소스 인스턴스와 핸들의 생성·동기화·삭제를 단독 소유한다. */
namespace Pakuri.NewCore.Combat.Effects
{
    public readonly struct EffectVisualRequest
    {
        /* 실행 계층이 해석한 엔진 중립 시각 값만 저장한다. */
        public EffectVisualRequest(
            string prefabPath,
            string spritePath,
            string animatorControllerPath,
            float scale,
            float scaleX,
            float scaleY,
            float scaleZ,
            int sortingOrder)
        {
            PrefabPath = prefabPath ?? string.Empty;
            SpritePath = spritePath ?? string.Empty;
            AnimatorControllerPath =
                animatorControllerPath ?? string.Empty;
            Scale = scale;
            ScaleX = scaleX;
            ScaleY = scaleY;
            ScaleZ = scaleZ;
            SortingOrder = sortingOrder;
        }

        public string PrefabPath { get; }

        public string SpritePath { get; }

        public string AnimatorControllerPath { get; }

        public float Scale { get; }

        public float ScaleX { get; }

        public float ScaleY { get; }

        public float ScaleZ { get; }

        public int SortingOrder { get; }
    }

    public readonly struct EffectVisualSpec
    {
        /* 최종 시각 사양의 경로와 크기·정렬 값을 불변 상태로 저장한다. */
        public EffectVisualSpec(
            string prefabPath,
            string spritePath,
            string animatorControllerPath,
            float scale,
            float scaleX,
            float scaleY,
            float scaleZ,
            int sortingOrder)
        {
            PrefabPath = prefabPath ?? string.Empty;
            SpritePath = spritePath ?? string.Empty;
            AnimatorControllerPath =
                animatorControllerPath ?? string.Empty;
            Scale = scale > 0f ? scale : 1f;
            ScaleX = scaleX;
            ScaleY = scaleY;
            ScaleZ = scaleZ;
            SortingOrder = sortingOrder;
        }

        public string PrefabPath { get; }

        public string SpritePath { get; }

        public string AnimatorControllerPath { get; }

        public float Scale { get; }

        public float ScaleX { get; }

        public float ScaleY { get; }

        public float ScaleZ { get; }

        public int SortingOrder { get; }

        public bool UsesLocalScale =>
            ScaleX != 0f || ScaleY != 0f || ScaleZ != 0f;

        public bool HasResource =>
            !string.IsNullOrWhiteSpace(PrefabPath)
            || !string.IsNullOrWhiteSpace(SpritePath)
            || !string.IsNullOrWhiteSpace(AnimatorControllerPath);
    }

    public sealed class EffectHandle
    {
        /* Manager가 확정한 식별자·사양·배치를 활성 핸들로 저장한다. */
        internal EffectHandle(
            int id,
            EffectVisualSpec visual,
            CombatVector2 position,
            CombatVector2 direction)
        {
            Id = id;
            Visual = visual;
            Position = position;
            Direction = direction;
            IsActive = true;
        }

        public int Id { get; }

        public EffectVisualSpec Visual { get; }

        public string ResourcePath =>
            !string.IsNullOrWhiteSpace(Visual.PrefabPath)
                ? Visual.PrefabPath
                : Visual.SpritePath;

        public CombatVector2 Position { get; internal set; }

        public CombatVector2 Direction { get; internal set; }

        public bool IsActive { get; internal set; }
    }

    public sealed class EffectManager : MonoBehaviour
    {
        private readonly List<EffectHandle> handles = new List<EffectHandle>();
        private readonly Dictionary<int, GameObject> instances =
            new Dictionary<int, GameObject>();
        private readonly List<int> removals = new List<int>();
        [SerializeField] private Transform runtimeSkillRoot;
        private Func<string, GameObject> prefabResolver;
        private Func<string, Sprite> spriteResolver;
        private Func<string, RuntimeAnimatorController> animatorResolver;
        private IReadOnlyList<EffectHandle> readOnlyHandles;
        private int nextId = 1;

        public IReadOnlyList<EffectHandle> ActiveEffects =>
            readOnlyHandles
            ?? (readOnlyHandles =
                new ReadOnlyCollection<EffectHandle>(handles));

        /* Unity 시각 루트와 카탈로그 조회 경계를 연결한다. */
        public void BindVisualRuntime(
            Transform visualRoot,
            Func<string, GameObject> resolvePrefab,
            Func<string, Sprite> resolveSprite,
            Func<string, RuntimeAnimatorController> resolveAnimator)
        {
            runtimeSkillRoot = visualRoot
                ?? throw new ArgumentNullException(nameof(visualRoot));
            prefabResolver = resolvePrefab
                ?? throw new ArgumentNullException(nameof(resolvePrefab));
            spriteResolver = resolveSprite
                ?? throw new ArgumentNullException(nameof(resolveSprite));
            animatorResolver = resolveAnimator
                ?? throw new ArgumentNullException(nameof(resolveAnimator));
        }

        /* serialized 시각 루트와 bootstrap resource resolver를 연결한다. */
        public void BindVisualRuntime(
            Func<string, GameObject> resolvePrefab,
            Func<string, Sprite> resolveSprite,
            Func<string, RuntimeAnimatorController> resolveAnimator)
        {
            BindVisualRuntime(
                runtimeSkillRoot,
                resolvePrefab,
                resolveSprite,
                resolveAnimator);
        }

        /* 단일 Sprite 경로 요청을 기본 시각 요청으로 변환한다. */
        public EffectHandle Create(
            string resourcePath,
            CombatVector2 position,
            CombatVector2 direction)
        {
            return Create(
                new EffectVisualRequest(
                    string.Empty,
                    resourcePath,
                    string.Empty,
                    1f,
                    0f,
                    0f,
                    0f,
                    0),
                position,
                direction);
        }

        /* 엔진 중립 요청을 최종 사양으로 확정하고 활성 핸들을 만든다. */
        public EffectHandle Create(
            EffectVisualRequest request,
            CombatVector2 position,
            CombatVector2 direction)
        {
            return CreateHandle(BuildVisualSpec(request), position, direction);
        }

        /* 기존 공개 호출 호환을 위해 완성된 사양을 활성 핸들로 등록한다. */
        public EffectHandle Create(
            EffectVisualSpec visual,
            CombatVector2 position,
            CombatVector2 direction)
        {
            return CreateHandle(visual, position, direction);
        }

        /* 활성 핸들의 위치와 방향을 갱신한다. */
        public bool TryUpdate(
            EffectHandle handle,
            CombatVector2 position,
            CombatVector2 direction)
        {
            if (handle == null || !handle.IsActive || !handles.Contains(handle))
            {
                return false;
            }

            handle.Position = position;
            handle.Direction = direction;
            return true;
        }

        /* 지정 핸들을 활성 목록에서 제거하고 연결 인스턴스를 삭제한다. */
        public bool Remove(EffectHandle handle)
        {
            if (handle == null || !handle.IsActive || !handles.Remove(handle))
            {
                return false;
            }

            handle.IsActive = false;
            RemoveInstance(handle.Id);
            return true;
        }

        /* 활성 핸들에 대응하는 Unity 인스턴스를 생성하고 Transform을 동기화한다. */
        public void SyncVisuals()
        {
            if (runtimeSkillRoot == null)
            {
                return;
            }

            for (int index = 0; index < handles.Count; index++)
            {
                EffectHandle handle = handles[index];
                if (!instances.TryGetValue(handle.Id, out GameObject instance))
                {
                    instance = CreateInstance(handle);
                    instances.Add(handle.Id, instance);
                }

                SyncInstance(instance, handle);
            }

            removals.Clear();
            foreach (KeyValuePair<int, GameObject> pair in instances)
            {
                if (!ContainsHandle(pair.Key))
                {
                    removals.Add(pair.Key);
                }
            }

            for (int index = 0; index < removals.Count; index++)
            {
                RemoveInstance(removals[index]);
            }
        }

        /* 모든 핸들과 Unity 인스턴스를 즉시 비활성화하고 정리한다. */
        public void Clear()
        {
            for (int index = 0; index < handles.Count; index++)
            {
                handles[index].IsActive = false;
            }

            handles.Clear();
            removals.Clear();
            foreach (GameObject instance in instances.Values)
            {
                DestroyInstance(instance);
            }

            instances.Clear();
        }

        /* 요청의 기본값 정책을 적용해 최종 시각 사양을 만든다. */
        private static EffectVisualSpec BuildVisualSpec(EffectVisualRequest request)
        {
            return new EffectVisualSpec(
                request.PrefabPath,
                request.SpritePath,
                request.AnimatorControllerPath,
                request.Scale,
                request.ScaleX,
                request.ScaleY,
                request.ScaleZ,
                request.SortingOrder);
        }

        /* 확정 사양과 배치로 새 활성 핸들을 만든다. */
        private EffectHandle CreateHandle(
            EffectVisualSpec visual,
            CombatVector2 position,
            CombatVector2 direction)
        {
            EffectHandle handle =
                new EffectHandle(nextId++, visual, position, direction);
            handles.Add(handle);
            return handle;
        }

        /* 시각 사양의 prefab 또는 Sprite를 해석해 Unity 인스턴스를 만든다. */
        private GameObject CreateInstance(EffectHandle handle)
        {
            EffectVisualSpec visual = handle.Visual;
            GameObject instance;
            if (!string.IsNullOrWhiteSpace(visual.PrefabPath))
            {
                GameObject prefab = prefabResolver(visual.PrefabPath);
                if (prefab == null)
                {
                    throw new InvalidOperationException(
                        $"No visual prefab is mapped for '{visual.PrefabPath}'.");
                }

                instance = Object.Instantiate(prefab, runtimeSkillRoot);
            }
            else if (!string.IsNullOrWhiteSpace(visual.SpritePath))
            {
                Sprite sprite = spriteResolver(visual.SpritePath);
                if (sprite == null)
                {
                    throw new InvalidOperationException(
                        $"No visual sprite is mapped for '{visual.SpritePath}'.");
                }

                instance = new GameObject(
                    string.IsNullOrWhiteSpace(sprite.name)
                        ? "NewCoreSkillVisual"
                        : sprite.name);
                instance.transform.SetParent(runtimeSkillRoot, false);
                instance.AddComponent<SpriteRenderer>().sprite = sprite;
            }
            else
            {
                if (!visual.HasResource)
                {
                    return null;
                }

                throw new InvalidOperationException(
                    "The effect visual has no creatable prefab or sprite.");
            }

            ConfigureRuntimeVisual(instance, visual);
            return instance;
        }

        /* Sprite·Animator·scale·sorting 값을 생성 인스턴스에 적용한다. */
        private void ConfigureRuntimeVisual(
            GameObject instance,
            EffectVisualSpec visual)
        {
            if (!string.IsNullOrWhiteSpace(visual.SpritePath))
            {
                Sprite sprite = spriteResolver(visual.SpritePath);
                if (sprite == null)
                {
                    throw new InvalidOperationException(
                        $"No visual sprite is mapped for '{visual.SpritePath}'.");
                }

                SpriteRenderer renderer = instance.GetComponent<SpriteRenderer>();
                if (renderer == null)
                {
                    renderer = instance.AddComponent<SpriteRenderer>();
                }

                renderer.sprite = sprite;
                renderer.sortingOrder = visual.SortingOrder;
            }

            if (!string.IsNullOrWhiteSpace(
                    visual.AnimatorControllerPath))
            {
                RuntimeAnimatorController controller =
                    animatorResolver(visual.AnimatorControllerPath);
                if (controller == null)
                {
                    throw new InvalidOperationException(
                        "No visual AnimatorController is mapped for "
                        + $"'{visual.AnimatorControllerPath}'.");
                }

                Animator animator = instance.GetComponent<Animator>();
                if (animator == null)
                {
                    animator = instance.AddComponent<Animator>();
                }

                animator.runtimeAnimatorController = controller;
            }

            if (visual.UsesLocalScale)
            {
                instance.transform.localScale = new Vector3(
                    visual.ScaleX == 0f ? 1f : visual.ScaleX,
                    visual.ScaleY == 0f ? 1f : visual.ScaleY,
                    visual.ScaleZ == 0f ? 1f : visual.ScaleZ);
            }
            else if (!string.IsNullOrWhiteSpace(visual.SpritePath)
                || !string.IsNullOrWhiteSpace(
                    visual.AnimatorControllerPath))
            {
                instance.transform.localScale =
                    Vector3.one * visual.Scale;
            }
        }

        /* 핸들의 전투 좌표와 방향을 Unity Transform에 투영한다. */
        private static void SyncInstance(
            GameObject instance,
            EffectHandle handle)
        {
            if (instance == null)
            {
                return;
            }

            instance.transform.position = new Vector3(
                handle.Position.X,
                handle.Position.Y,
                instance.transform.position.z);
            if (handle.Direction.SqrMagnitude > 0.0001f)
            {
                instance.transform.right = new Vector3(
                    handle.Direction.X,
                    handle.Direction.Y,
                    0f);
            }
        }

        /* 활성 핸들 목록에 지정 식별자가 남아 있는지 검사한다. */
        private bool ContainsHandle(int id)
        {
            for (int index = 0; index < handles.Count; index++)
            {
                if (handles[index].Id == id)
                {
                    return true;
                }
            }

            return false;
        }

        /* 지정 식별자의 Unity 인스턴스를 삭제하고 소유 목록에서 제거한다. */
        private void RemoveInstance(int id)
        {
            if (!instances.TryGetValue(id, out GameObject instance))
            {
                return;
            }

            DestroyInstance(instance);
            instances.Remove(id);
        }

        /* Play Mode 여부에 맞는 Unity 삭제 API로 인스턴스를 정리한다. */
        private static void DestroyInstance(GameObject instance)
        {
            if (instance == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Object.Destroy(instance);
            }
            else
            {
                Object.DestroyImmediate(instance);
            }
        }
    }
}
