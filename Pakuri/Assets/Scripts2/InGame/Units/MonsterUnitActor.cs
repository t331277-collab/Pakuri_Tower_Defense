using UnityEngine;

namespace Pakuri.InGame
{
    [DisallowMultipleComponent]
    public sealed class MonsterUnitActor : MonoBehaviour
    {
        [SerializeField] private TextMesh monsterNameLabel;
        [SerializeField] private TextMesh monsterHpLabel;
        [SerializeField] private TextMesh damageTextLabel;
        [SerializeField] private Transform hpBackground;
        [SerializeField] private Transform hpFill;
        [SerializeField] private Transform shieldFill;
        [SerializeField] private InGameDamageTextPopup damageTextPopup;

        public MonsterUnitRuntimeModel Model { get; private set; }

        public void Initialize(MonsterUnitRuntimeModel model)
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
            UnitActorView.Refresh(Model, monsterNameLabel, monsterHpLabel, hpBackground, hpFill, shieldFill);
        }

        private void ResolveDebugViewReferences()
        {
            if (monsterNameLabel == null)
            {
                monsterNameLabel = UnitActorView.FindTextMesh(this, UnitActorView.NameLabelObjectName);
            }

            if (monsterHpLabel == null)
            {
                monsterHpLabel = UnitActorView.FindTextMesh(this, UnitActorView.HpLabelObjectName);
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
