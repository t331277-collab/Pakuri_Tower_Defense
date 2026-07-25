using System.Collections.Generic;
using Pakuri.NewCore.Definitions.Units;
using Pakuri.NewCore.Units.Models;
using UnityEngine;

/* Unit Model의 위치·월드 상태·피해 숫자 표현을 공통 scene Actor에 투영한다. */
namespace Pakuri.NewCore.Units.Actors
{
    public abstract class UnitActor : MonoBehaviour
    {
        private const float DefaultDurationSeconds = 1f;
        private const float DefaultRiseDistance = 1f;
        private const int DefaultMaximumActivePopups = 12;
        private const float DefaultVerticalSpacing = 0.18f;

        [SerializeField] private float damagePopupDurationSeconds =
            DefaultDurationSeconds;
        [SerializeField] private float damagePopupRiseDistance =
            DefaultRiseDistance;
        [SerializeField] private int maximumActiveDamagePopups =
            DefaultMaximumActivePopups;
        [SerializeField] private float damagePopupVerticalSpacing =
            DefaultVerticalSpacing;

        private readonly List<DamagePopup> damagePopups =
            new List<DamagePopup>();
        private UnitWorldView worldView;
        private TextMesh damageTemplate;
        private Vector3 damageTemplatePosition;
        private Color damageTemplateColor;

        public UnitBaseModel Model { get; private set; }

        public int ActiveDamagePopupCount => damagePopups.Count;

        /* Model을 Actor에 연결하고 초기 Transform과 월드 표시를 동기화한다. */
        public void Bind(UnitBaseModel model)
        {
            Model = model
                ?? throw new System.ArgumentNullException(nameof(model));
            Model.SetPosition(ToModel(transform.position));
            worldView = new UnitWorldView(this);
            InitializeDamagePopups(worldView.DamageTemplate);
            SyncFromModel();
        }

        /* Model 위치·체력·보호막 값을 현재 scene 표현에 투영한다. */
        public virtual void SyncFromModel()
        {
            if (Model == null)
            {
                return;
            }

            var position = Model.Position;
            transform.position = new Vector3(
                position.X,
                position.Y,
                transform.position.z);
            worldView.Refresh(Model);
        }

        /* 확정된 양수 피해를 새 월드 공간 popup으로 표시한다. */
        public void ShowDamage(float amount)
        {
            if (damageTemplate == null || amount <= 0f)
            {
                return;
            }

            RemoveMissingDamagePopups();
            while (damagePopups.Count >= Mathf.Max(
                1,
                maximumActiveDamagePopups))
            {
                DestroyDamagePopup(damagePopups[0]);
                damagePopups.RemoveAt(0);
            }

            GameObject instance = Instantiate(
                damageTemplate.gameObject,
                damageTemplate.transform.parent);
            instance.name = damageTemplate.gameObject.name + "_Popup";
            instance.SetActive(true);
            TextMesh text = instance.GetComponent<TextMesh>();
            if (text == null)
            {
                DestroyDamagePopupObject(instance);
                return;
            }

            Vector3 position = damageTemplatePosition;
            position.y += damagePopups.Count
                * Mathf.Max(0f, damagePopupVerticalSpacing);
            text.transform.localPosition = position;
            text.text = $"{Mathf.RoundToInt(amount)}(Damage)";
            Color color = damageTemplateColor;
            color.a = 1f;
            text.color = color;
            damagePopups.Add(new DamagePopup(
                instance,
                text,
                position,
                color,
                Mathf.Max(0.01f, damagePopupDurationSeconds)));
        }

        /* Unity frame 시간을 피해 popup 생명주기에 전달한다. */
        private void Update()
        {
            TickDamagePopups(Time.unscaledDeltaTime);
        }

        /* public 경과 시간을 검증하고 popup 상승·투명도·삭제를 처리한다. */
        public void TickDamagePopups(float deltaTime)
        {
            if (deltaTime < 0f
                || float.IsNaN(deltaTime)
                || float.IsInfinity(deltaTime))
            {
                throw new System.ArgumentOutOfRangeException(
                    nameof(deltaTime));
            }

            for (int index = damagePopups.Count - 1; index >= 0; index--)
            {
                DamagePopup popup = damagePopups[index];
                if (popup.Text == null)
                {
                    damagePopups.RemoveAt(index);
                    continue;
                }

                popup.ElapsedSeconds += deltaTime;
                float normalized = Mathf.Clamp01(
                    popup.ElapsedSeconds / popup.DurationSeconds);
                Vector3 position = popup.StartPosition;
                position.y += Mathf.Max(
                    0f,
                    damagePopupRiseDistance) * normalized;
                popup.Text.transform.localPosition = position;
                Color color = popup.StartColor;
                color.a = 1f - normalized;
                popup.Text.color = color;
                if (popup.ElapsedSeconds >= popup.DurationSeconds)
                {
                    DestroyDamagePopup(popup);
                    damagePopups.RemoveAt(index);
                }
            }
        }

        /* Actor 파괴 전에 남은 피해 popup 인스턴스를 정리한다. */
        private void OnDestroy()
        {
            ClearDamagePopups();
        }

        /* 피해 템플릿을 저장하고 원본 라벨을 비활성 상태로 초기화한다. */
        private void InitializeDamagePopups(TextMesh template)
        {
            ClearDamagePopups();
            damageTemplate = template;
            if (damageTemplate == null)
            {
                return;
            }

            damageTemplatePosition =
                damageTemplate.transform.localPosition;
            damageTemplateColor = damageTemplate.color;
            damageTemplate.text = string.Empty;
            damageTemplate.gameObject.SetActive(false);
        }

        /* 활성 피해 popup을 모두 삭제하고 목록을 비운다. */
        private void ClearDamagePopups()
        {
            for (int index = damagePopups.Count - 1; index >= 0; index--)
            {
                DestroyDamagePopup(damagePopups[index]);
            }

            damagePopups.Clear();
        }

        /* Unity에서 이미 제거된 popup 항목을 활성 목록에서 정리한다. */
        private void RemoveMissingDamagePopups()
        {
            for (int index = damagePopups.Count - 1; index >= 0; index--)
            {
                if (damagePopups[index].Text == null)
                {
                    damagePopups.RemoveAt(index);
                }
            }
        }

        /* popup 값 객체가 소유한 Unity 인스턴스를 삭제한다. */
        private static void DestroyDamagePopup(DamagePopup popup)
        {
            if (popup?.Instance != null)
            {
                DestroyDamagePopupObject(popup.Instance);
            }
        }

        /* Play Mode 여부에 맞는 Unity API로 popup 오브젝트를 삭제한다. */
        private static void DestroyDamagePopupObject(Object target)
        {
            if (Application.isPlaying)
            {
                Destroy(target);
            }
            else
            {
                DestroyImmediate(target);
            }
        }

        /* Unity 좌표를 엔진 중립 전투 좌표로 변환한다. */
        protected static CombatVector2 ToModel(Vector3 value)
        {
            return new CombatVector2(value.x, value.y);
        }

        private sealed class UnitWorldView
        {
            private const string NameObject = "MonsterNameLabel";
            private const string HealthObject = "MonsterHpLabel";
            private const string BackgroundObject = "Background";
            private const string FillObject = "Fill";
            private const string ShieldObject = "Shield";
            private const string DamageObject = "Damage";

            private readonly TextMesh nameLabel;
            private readonly TextMesh healthLabel;
            private readonly Transform background;
            private readonly Transform healthFill;
            private readonly Transform shieldFill;
            private readonly TextMesh damageLabel;

            public TextMesh DamageTemplate => damageLabel;

            /* Actor 자식의 고정 이름 월드 표시 요소를 한 번 찾아 저장한다. */
            public UnitWorldView(Component owner)
            {
                nameLabel = Find<TextMesh>(owner, NameObject);
                healthLabel = Find<TextMesh>(owner, HealthObject);
                background = Find(owner, BackgroundObject);
                healthFill = Find(owner, FillObject);
                shieldFill = Find(owner, ShieldObject);
                damageLabel = Find<TextMesh>(owner, DamageObject);
            }

            /* Model 이름·체력·보호막을 월드 라벨과 bar에 반영한다. */
            public void Refresh(UnitBaseModel model)
            {
                if (nameLabel != null)
                {
                    nameLabel.text = ResolveName(model);
                }

                if (healthLabel != null)
                {
                    healthLabel.text = model.CurrentShield > 0f
                        ? $"HP {Format(model.CurrentHealth)}/{Format(model.MaximumHealth)} +{Format(model.CurrentShield)}"
                        : $"HP {Format(model.CurrentHealth)}/{Format(model.MaximumHealth)}";
                }

                var visible = Mathf.Max(
                    model.MaximumHealth,
                    model.CurrentHealth + model.CurrentShield);
                var total = Mathf.Max(1f, visible);
                var healthRatio = Mathf.Clamp01(model.CurrentHealth / total);
                var shieldRatio = Mathf.Clamp01(model.CurrentShield / total);
                SetSegment(healthFill, 0f, healthRatio);
                SetSegment(shieldFill, healthRatio, shieldRatio);
                if (shieldFill != null)
                {
                    shieldFill.gameObject.SetActive(model.CurrentShield > 0f);
                }
            }

            /* 지정 비율 구간에 맞춰 bar 폭과 중심 위치를 조정한다. */
            private void SetSegment(
                Transform target,
                float leftRatio,
                float widthRatio)
            {
                if (target == null)
                {
                    return;
                }

                var baseScale = background != null
                    ? background.localScale.x
                    : target.localScale.x;
                var width = RenderedWidth(background, Mathf.Abs(baseScale));
                var segmentWidth = width * Mathf.Clamp01(widthRatio);
                var renderer = target.GetComponent<SpriteRenderer>();
                var unitWidth = renderer != null && renderer.sprite != null
                    ? Mathf.Max(0.0001f, renderer.sprite.bounds.size.x)
                    : 1f;
                var scale = target.localScale;
                var sign = scale.x < 0f || (Mathf.Approximately(scale.x, 0f) && baseScale < 0f)
                    ? -1f
                    : 1f;
                scale.x = sign * segmentWidth / unitWidth;
                target.localScale = scale;

                var center = background != null
                    ? background.localPosition.x
                    : 0f;
                var position = target.localPosition;
                position.x = center - width * 0.5f
                    + width * Mathf.Clamp01(leftRatio)
                    + segmentWidth * 0.5f;
                target.localPosition = position;
            }

            /* Unit Definition 종류에 맞는 표시 이름을 반환한다. */
            private static string ResolveName(UnitBaseModel model)
            {
                if (model.Definition is MonsterDefinition monster)
                {
                    return string.IsNullOrWhiteSpace(monster.display_name)
                        ? monster.id
                        : monster.display_name;
                }

                if (model.Definition is EnemyDefinition enemy)
                {
                    return string.IsNullOrWhiteSpace(enemy.display_name)
                        ? enemy.enemy_id
                        : enemy.display_name;
                }

                return "Nexus";
            }

            /* Sprite bounds를 포함한 실제 표시 폭을 계산한다. */
            private static float RenderedWidth(
                Transform target,
                float fallback)
            {
                if (target == null)
                {
                    return fallback;
                }

                var renderer = target.GetComponent<SpriteRenderer>();
                return renderer != null && renderer.sprite != null
                    ? Mathf.Abs(target.localScale.x)
                        * Mathf.Max(0.0001f, renderer.sprite.bounds.size.x)
                    : Mathf.Abs(target.localScale.x);
            }

            /* Actor 자식에서 고정 이름 Transform을 찾는다. */
            private static Transform Find(
                Component owner,
                string objectName)
            {
                var children = owner.GetComponentsInChildren<Transform>(true);
                for (var index = 0; index < children.Length; index++)
                {
                    if (children[index].name == objectName)
                    {
                        return children[index];
                    }
                }

                return null;
            }

            /* Actor 자식의 고정 이름 오브젝트에서 지정 컴포넌트를 찾는다. */
            private static T Find<T>(
                Component owner,
                string objectName)
                where T : Component
            {
                var target = Find(owner, objectName);
                return target != null ? target.GetComponent<T>() : null;
            }

            /* 정수는 정수로, 소수는 두 자리 이하로 체력 값을 표시한다. */
            private static string Format(float value)
            {
                return Mathf.Approximately(value, Mathf.Round(value))
                    ? Mathf.RoundToInt(value).ToString()
                    : value.ToString("0.##");
            }
        }

        private sealed class DamagePopup
        {
            /* popup 인스턴스의 시작 표시 상태와 지속시간을 저장한다. */
            public DamagePopup(
                GameObject instance,
                TextMesh text,
                Vector3 startPosition,
                Color startColor,
                float durationSeconds)
            {
                Instance = instance;
                Text = text;
                StartPosition = startPosition;
                StartColor = startColor;
                DurationSeconds = durationSeconds;
            }

            public GameObject Instance { get; }

            public TextMesh Text { get; }

            public Vector3 StartPosition { get; }

            public Color StartColor { get; }

            public float DurationSeconds { get; }

            public float ElapsedSeconds { get; set; }
        }
    }
}
