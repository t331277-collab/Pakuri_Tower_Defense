using UnityEngine;

namespace Pakuri.InGame
{
    [DisallowMultipleComponent]
    public sealed class MonsterUnitActor : MonoBehaviour
    {
        private const string RinMonsterId = "rin";

        [SerializeField] private TextMesh monsterNameLabel;
        [SerializeField] private TextMesh monsterHpLabel;
        [SerializeField] private TextMesh damageTextLabel;
        [SerializeField] private Transform hpBackground;
        [SerializeField] private Transform hpFill;
        [SerializeField] private Transform shieldFill;
        [SerializeField] private InGameDamageTextPopup damageTextPopup;
        [SerializeField] private Animation_Controller animationController;

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

        public void TryPlayActiveSkillAnimation()
        {
            if (ShouldUseRinAnimation())
            {
                ResolveAnimationController()?.PlayRandomAttack();
            }
        }

        public void TryPlayHitAnimation()
        {
            if (ShouldUseRinAnimation())
            {
                ResolveAnimationController()?.PlayHit();
            }
        }

        public void TryPlayDeathAnimation()
        {
            if (ShouldUseRinAnimation())
            {
                ResolveAnimationController()?.PlayDeath();
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

            ResolveAnimationController();
        }

        private Animation_Controller ResolveAnimationController()
        {
            if (animationController == null)
            {
                animationController = GetComponent<Animation_Controller>();
            }

            return animationController;
        }

        private bool ShouldUseRinAnimation()
        {
            var definitionId = Model != null && Model.Identity != null
                ? Model.Identity.DefinitionId
                : string.Empty;
            return string.Equals(definitionId, RinMonsterId, System.StringComparison.OrdinalIgnoreCase);
        }
    }
}
