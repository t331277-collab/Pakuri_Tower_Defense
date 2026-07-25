using System.Collections.Generic;
using Pakuri.Data;
using UnityEngine;

/*
 * 전투 효과 오브젝트의 생성, 상태 비주얼 연결, 제거를 한곳에서 관리한다.
 * 대상 부착 효과는 대상의 자식으로 연결하고, 각 스킬 Actor는 수명이 끝나면 제거를 요청한다.
 */
namespace Pakuri.InGame
{
    public class EffectManager : MonoBehaviour
    {
        [SerializeField] private Transform runtimeSkillRoot;
        private readonly Dictionary<StatusRuntimeInstance, GameObject> statusEffectVisuals = new Dictionary<StatusRuntimeInstance, GameObject>();
        private readonly HashSet<GameObject> targetAttachedEffects = new HashSet<GameObject>();

        /*
         * 런타임 비주얼 또는 프리팹으로 효과 오브젝트만 생성
         */
        public GameObject CreateEffect(
            RuntimeSkillVisualSpec visual /* 런타임 시각 효과 설정 */,
            GameObject prefab /* 생성할 프리팹 */,
            string objectName /* 게임 오브젝트 이름 */,
            Vector3 position /* 배치할 위치 */,
            Quaternion rotation /* 배치할 회전값 */,
            bool hitboxIsTrigger = false /* 피격 판정 트리거 여부 */,
            bool includeHitbox = true /* 피격 판정 포함 여부 */)
        {
            if (visual != null && visual.HasVisual()) // 런타임 비주얼 생성
            {
                var instance = CreateSkillActorObject(objectName, position, rotation);
                EffectVisualBuilder.Configure(instance, visual, hitboxIsTrigger, includeHitbox);
                return instance;
            }

            if (prefab != null) // 프리팹 생성
            {
                return Instantiate(prefab, position, rotation, runtimeSkillRoot);
            }

            return null;
        }

        /*
         * 비주얼을 생성해 대상의 자식으로 연결한다.
         */
        public GameObject CreateTargetVisual(
            RuntimeSkillVisualSpec visual /* 런타임 시각 효과 설정 */,
            GameObject prefab /* 생성할 프리팹 */,
            string objectName /* 게임 오브젝트 이름 */,
            Transform targetTransform /* 비주얼을 붙일 대상 */,
            bool hitboxIsTrigger = false /* 피격 판정 트리거 여부 */,
            bool includeHitbox = true /* 피격 판정 포함 여부 */)
        {

            var instance = CreateEffect(
                visual,
                prefab,
                objectName,
                targetTransform.position,
                Quaternion.identity,
                hitboxIsTrigger,
                includeHitbox);
            if (instance == null)
            {
                return null;
            }

            AttachVisualToTarget(instance, targetTransform);
            targetAttachedEffects.RemoveWhere(effectObject => effectObject == null);
            targetAttachedEffects.Add(instance);
            return instance;
        }

        /*
         * 생성한 비주얼(버프, 상태이상)을 대상에 자식으로 붙인다.
         */
        private static void AttachVisualToTarget(GameObject instance, Transform targetTransform)
        {
            instance.transform.SetParent(targetTransform, true);
            instance.transform.position = targetTransform.position;
        }

        /*
         * 비주얼이나 프리팹이 없을 때 스킬 Actor를 붙일 빈 오브젝트를 생성
         */
        public GameObject CreateSkillActorObject(
            string objectName /* 게임 오브젝트 이름 */,
            Vector3 position /* 배치할 위치 */,
            Quaternion rotation /* 배치할 회전값 */)
        {
            var instance = new GameObject(objectName);
            instance.transform.SetParent(runtimeSkillRoot, false);
            instance.transform.SetPositionAndRotation(position, rotation);
            return instance;
        }

        /*
         * 상태 효과 비주얼을 생성 현재 아리엘 - D 만 구현되어있고 나중에 상태이상 공통 비쥬얼 이펙트로 사용할 예정
         */
        public void ShowOrRefreshStatusEffect(
            Transform targetTransform /* 상태 효과를 표시할 대상 */,
            StatusRuntimeInstance status /* 실행 중인 상태 효과 */)
        {
            if (targetTransform == null || status == null || status.SourceData == null) // 이펙트 생성하지 않음
            {
                return;
            }

            var statusData = status.SourceData;
            var hasRuntimeVisual = statusData.RuntimeVisual != null && statusData.RuntimeVisual.HasVisual();
            if (!hasRuntimeVisual && statusData.StatusEffectPrefab == null)
            {
                return;
            }

            statusEffectVisuals.TryGetValue(status, out var instance);
            if (instance == null)
            {
                statusEffectVisuals.Remove(status);
                var objectName = "RuntimeStatusVisual";
                if (!string.IsNullOrWhiteSpace(status.SourceSkillId))
                {
                    objectName = "RuntimeStatusVisual_" + status.SourceSkillId;
                }

                instance = CreateTargetVisual(
                    statusData.RuntimeVisual,
                    statusData.StatusEffectPrefab,
                    objectName,
                    targetTransform,
                    includeHitbox: false);
                if (instance == null)
                {
                    return;
                }

                statusEffectVisuals[status] = instance;
            }

            AttachVisualToTarget(instance, targetTransform);
        }

        /*
         * 비주얼을 생성하고 지정한 시간이 지나면 제거하도록 Actor에 전달
         */
        public GameObject CreateEffect(
            SkillEffectDefinition effect /* 생성할 추가 효과 */,
            Vector3 position /* 배치할 위치 */,
            float durationSeconds /* 표시 시간 */)
        {
            var objectName = "SkillEffectVisual";
            if (!string.IsNullOrWhiteSpace(effect.EffectId)) // 이름 추가
            {
                objectName = "SkillEffectVisual_" + effect.EffectId;
            }

            var instance = CreateEffect(
                effect.RuntimeVisual,
                effect.SkillEffectPrefab,
                objectName,
                position,
                Quaternion.identity);
            if (instance != null)
            {
                SingleSkillActor.Attach(instance).InitializeTimed(this, durationSeconds);
            }

            return instance;
        }

        /*
         * 이펙트 비주얼을 적용 대상에게 붙이고 수명 관리를 Actor에 전달한다.
         */
        public void ShowFollowingSkillEffects(
            SkillEffectDefinition effect /* 표시할 추가 효과 */,
            IReadOnlyList<CombatUnitEntry> targets /* 비주얼을 붙일 대상 목록 */,
            float durationSeconds /* 표시 시간 */)
        {
            var objectName = "SkillEffectVisual";
            if (!string.IsNullOrWhiteSpace(effect.EffectId)) // 이름 추가
            {
                objectName = "SkillEffectVisual_" + effect.EffectId;
            }

            for (var i = 0; i < targets.Count; i++)
            {
                var target = targets[i];
                if (target == null || target.Transform == null)
                {
                    continue;
                }

                var instance = CreateTargetVisual(
                    effect.RuntimeVisual,
                    effect.SkillEffectPrefab,
                    objectName,
                    target.Transform);
                if (instance != null)
                {
                    BuffSkillActor.Attach(instance).Initialize(
                        this,
                        durationSeconds);
                }
            }
        }

        /*
         * 효과 등록을 해제하고 오브젝트를 삭제한다.
         */
        public void RemoveEffect(
            GameObject instance /* 제거할 효과 오브젝트 */,
            StatusRuntimeInstance status = null /* 연결된 상태 효과 */)
        {
            if (status != null)
            {
                if (instance == null)
                {
                    statusEffectVisuals.TryGetValue(status, out instance);
                }

                statusEffectVisuals.Remove(status);
            }

            targetAttachedEffects.Remove(instance);
            if (instance == null)
            {
                return;
            }

            Destroy(instance);
        }

        /*
         * 스킬 루트 아래의 모든 효과를 RemoveEffect를 통해 정리한다.
         */
        public void ClearEffects()
        {
            var attachedEffects = new List<GameObject>(targetAttachedEffects);
            for (var i = 0; i < attachedEffects.Count; i++)
            {
                var attachedEffect = attachedEffects[i];
                if (attachedEffect != null)
                {
                    attachedEffect.SetActive(false);
                }

                RemoveEffect(attachedEffect);
            }

            targetAttachedEffects.Clear();
            statusEffectVisuals.Clear();
            for (var i = runtimeSkillRoot.childCount - 1; i >= 0; i--)
            {
                var child = runtimeSkillRoot.GetChild(i).gameObject;
                child.SetActive(false);
                RemoveEffect(child);
            }

        }
    }
}
