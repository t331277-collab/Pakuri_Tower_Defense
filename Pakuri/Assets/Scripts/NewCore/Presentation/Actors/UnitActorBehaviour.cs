using Pakuri.NewCore.Definitions.Units;
using Pakuri.NewCore.Units.Models;
using UnityEngine;

namespace Pakuri.NewCore.Presentation.Actors
{
    public abstract class UnitActorBehaviour : MonoBehaviour
    {
        private UnitWorldView worldView;
        private DamageNumberPopupBehaviour damagePopups;

        public UnitBaseModel Model { get; private set; }

        public void Bind(UnitBaseModel model)
        {
            Model = model
                ?? throw new System.ArgumentNullException(nameof(model));
            Model.SetPosition(ToModel(transform.position));
            worldView = new UnitWorldView(this);
            damagePopups = GetComponent<DamageNumberPopupBehaviour>();
            if (damagePopups == null)
            {
                damagePopups =
                    gameObject.AddComponent<DamageNumberPopupBehaviour>();
            }
            damagePopups.Initialize(worldView.DamageTemplate);
            SyncFromModel();
        }

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

        public void ShowDamage(float amount)
        {
            if (damagePopups == null)
            {
                return;
            }

            damagePopups.Show(amount);
        }

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

            public UnitWorldView(Component owner)
            {
                nameLabel = Find<TextMesh>(owner, NameObject);
                healthLabel = Find<TextMesh>(owner, HealthObject);
                background = Find(owner, BackgroundObject);
                healthFill = Find(owner, FillObject);
                shieldFill = Find(owner, ShieldObject);
                damageLabel = Find<TextMesh>(owner, DamageObject);
            }

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

            private static T Find<T>(
                Component owner,
                string objectName)
                where T : Component
            {
                var target = Find(owner, objectName);
                return target != null ? target.GetComponent<T>() : null;
            }

            private static string Format(float value)
            {
                return Mathf.Approximately(value, Mathf.Round(value))
                    ? Mathf.RoundToInt(value).ToString()
                    : value.ToString("0.##");
            }
        }
    }
}
