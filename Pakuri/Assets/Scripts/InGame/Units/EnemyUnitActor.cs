using UnityEngine;

namespace Pakuri.InGame
{
    /*
     * 적 모델과 이름·체력·보호막·피해 숫자 표시를 연결하는 컴포넌트.
     */
    [DisallowMultipleComponent]
    public sealed class EnemyUnitActor : MonoBehaviour
    {
        [SerializeField] private TextMesh enemyNameLabel;
        [SerializeField] private TextMesh enemyHpLabel;
        [SerializeField] private TextMesh damageTextLabel;
        [SerializeField] private Transform hpBackground;
        [SerializeField] private Transform hpFill;
        [SerializeField] private Transform shieldFill;
        [SerializeField] private InGameDamageTextPopup damageTextPopup;

        public EnemyUnitRuntimeModel Model { get; private set; }

        public void Initialize(EnemyUnitRuntimeModel model)
        {
            Model = model;
            ResolveDebugViewReferences();
            RefreshDebugView();
        }

        public void ShowDamage(float damageAmount)
        {
            if (damageTextPopup != null)
            {
                damageTextPopup.Show(damageAmount);
            }
        }

        public void RefreshDebugView()
        {
            UnitActorView.Refresh(Model, enemyNameLabel, enemyHpLabel, hpBackground, hpFill, shieldFill);
        }

        private void ResolveDebugViewReferences()
        {
            if (enemyNameLabel == null)
            {
                enemyNameLabel = UnitActorView.FindTextMesh(this, UnitActorView.NameLabelObjectName);
            }

            if (enemyHpLabel == null)
            {
                enemyHpLabel = UnitActorView.FindTextMesh(this, UnitActorView.HpLabelObjectName);
            }

            if (damageTextLabel == null)
            {
                damageTextLabel = UnitActorView.FindTextMesh(this, UnitActorView.DamageTextObjectName);
            }

            if (damageTextPopup == null)
            {
                damageTextPopup = UnitActorView.EnsureDamagePopup(this, damageTextLabel);
            }

            if (hpBackground == null)
            {
                hpBackground = UnitActorView.FindChildTransform(this, UnitActorView.HpBackgroundObjectName);
            }

            if (hpFill == null)
            {
                hpFill = UnitActorView.FindChildTransform(this, UnitActorView.HpFillObjectName);
            }

            if (shieldFill == null)
            {
                shieldFill = UnitActorView.FindChildTransform(this, UnitActorView.ShieldFillObjectName);
            }
        }
    }
}
