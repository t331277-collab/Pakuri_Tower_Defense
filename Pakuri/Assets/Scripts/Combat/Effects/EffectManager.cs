using System.Collections.Generic;
using Pakuri.Data;
using UnityEngine;

/*
 * 전투 효과 오브젝트의 생성, 상태 효과 등록, 제거를 한곳에서 관리한다.
 * 각 스킬 Actor가 수명과 대상 추적을 담당하고 종료 시 제거를 요청한다.
 * 모든 효과 오브젝트의 실제 삭제는 RemoveEffect 한 곳에서만 수행한다.
 */
namespace Pakuri.InGame
{
    public class EffectManager : MonoBehaviour
    {
        [SerializeField] private Transform runtimeSkillRoot;
        private readonly Dictionary<StatusRuntimeInstance, GameObject> statusEffectVisuals = new Dictionary<StatusRuntimeInstance, GameObject>();

        /*
         * 런타임 비주얼 또는 프리팹으로 효과 오브젝트를 생성한다.
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
            if (visual != null && visual.HasVisual())
            {
                var instance = CreateSkillActorObject(objectName, position, rotation);
                EffectVisualBuilder.Configure(instance, visual, hitboxIsTrigger, includeHitbox);
                return instance;
            }

            if (prefab != null)
            {
                return Instantiate(prefab, position, rotation, runtimeSkillRoot);
            }

            return null;
        }

        /*
         * 비주얼이 없어도 실행되어야 하는 스킬 Actor용 빈 오브젝트를 생성한다.
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
         * 상태 효과 비주얼을 처음 생성하거나 이미 생성된 비주얼을 대상에게 다시 연결한다.
         */
        public void ShowOrRefreshStatusEffect(
            Transform targetTransform /* 상태 효과를 표시할 대상 */,
            StatusRuntimeInstance status /* 실행 중인 상태 효과 */)
        {
            if (targetTransform == null || status == null || status.SourceData == null)
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

                instance = CreateEffect(
                    statusData.RuntimeVisual,
                    statusData.StatusEffectPrefab,
                    objectName,
                    targetTransform.position,
                    Quaternion.identity,
                    includeHitbox: false);
                if (instance == null)
                {
                    return;
                }

                statusEffectVisuals[status] = instance;
            }

            instance.transform.SetParent(targetTransform, true);
            instance.transform.position = targetTransform.position;
        }

        /*
         * 추가 효과 비주얼을 만들고 지정한 시간이 지나면 제거하도록 Actor에 전달한다.
         */
        public void ShowTimedSkillEffect(
            SkillEffectDefinition effect /* 표시할 추가 효과 */,
            Vector3 position /* 표시할 위치 */,
            float durationSeconds /* 표시 시간 */)
        {
            var objectName = "SkillEffectVisual";
            if (!string.IsNullOrWhiteSpace(effect.EffectId))
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
        }

        /*
         * 추가 효과 비주얼을 적용 대상에게 붙이고 수명 관리를 Actor에 전달한다.
         */
        public void ShowFollowingSkillEffects(
            SkillEffectDefinition effect /* 표시할 추가 효과 */,
            IReadOnlyList<CombatUnitEntry> targets /* 비주얼을 붙일 대상 목록 */,
            float durationSeconds /* 표시 시간 */)
        {
            var objectName = "SkillEffectVisual";
            if (!string.IsNullOrWhiteSpace(effect.EffectId))
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

                var instance = CreateEffect(
                    effect.RuntimeVisual,
                    effect.SkillEffectPrefab,
                    objectName,
                    target.Transform.position,
                    Quaternion.identity);
                if (instance != null)
                {
                    BuffSkillActor.Attach(instance).Initialize(
                        this,
                        target.Transform,
                        durationSeconds,
                        Vector3.zero);
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
            foreach (var statusVisual in statusEffectVisuals.Values)
            {
                if (statusVisual != null)
                {
                    statusVisual.SetActive(false);
                    RemoveEffect(statusVisual);
                }
            }

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
