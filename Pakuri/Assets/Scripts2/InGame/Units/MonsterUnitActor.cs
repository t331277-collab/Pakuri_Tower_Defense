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
        [SerializeField] private Animation_Controller animationController;

        private bool defeated;

        public MonsterUnitRuntimeModel Model { get; private set; }
        public bool IsDefeated => defeated;

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
            if (defeated)
            {
                return;
            }

            ResolveAnimationController()?.PlayRandomAttack();
        }

        public void TryPlayHitAnimation()
        {
            if (defeated)
            {
                return;
            }

            ResolveAnimationController()?.PlayHit();
        }

        public void TryPlayDeathAnimation()
        {
            ResolveAnimationController()?.PlayDeath();
        }

        public void MarkDefeated()
        {
            if (defeated)
            {
                return;
            }

            defeated = true;
            DisableTargetColliders();
            TryPlayDeathAnimation();
        }

        public void ReviveForNextDay()
        {
            defeated = false;
            RestoreCombatStateForNextDay();
            RestoreHealthToMaximum();
            EnableTargetColliders();
            ResolveAnimationController()?.ReviveToIdle();
            RefreshDebugView();
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

        private void DisableTargetColliders()
        {
            var colliders = GetComponentsInChildren<Collider2D>();
            for (var i = 0; i < colliders.Length; i++)
            {
                if (colliders[i] != null)
                {
                    colliders[i].enabled = false;
                }
            }
        }

        private void EnableTargetColliders()
        {
            var colliders = GetComponentsInChildren<Collider2D>();
            for (var i = 0; i < colliders.Length; i++)
            {
                if (colliders[i] != null)
                {
                    colliders[i].enabled = true;
                }
            }
        }

        private void RestoreCombatStateForNextDay()
        {
            if (Model == null)
            {
                return;
            }

            Model.AutoAttackEnabled = true;
            if (!IsSelectedPlayerModel(Model))
            {
                Model.AutoSkillEnabled = true;
            }

            Model.Statuses?.Clear();
            var resources = Model.Resources;
            if (resources != null)
            {
                resources.DirectShield = 0f;
                resources.CurrentShield = 0f;
            }

            var activeSkills = Model.SkillRuntime != null ? Model.SkillRuntime.ActiveSkills : null;
            if (activeSkills == null)
            {
                return;
            }

            for (var i = 0; i < activeSkills.Count; i++)
            {
                activeSkills[i]?.ResetRuntimeState();
            }
        }

        private void RestoreHealthToMaximum()
        {
            var resources = Model != null ? Model.Resources : null;
            var stats = Model != null ? Model.Stats : null;
            if (resources == null || stats == null)
            {
                return;
            }

            resources.CurrentHealth = Mathf.Max(0f, stats.MaxHealth);
        }

        private static bool IsSelectedPlayerModel(MonsterUnitRuntimeModel model)
        {
            var identity = model != null ? model.Identity : null;
            return identity != null
                && identity.Side == UnitSide.Player
                && identity.Role == UnitRole.Monster
                && identity.SlotIndex == 0;
        }
    }
}
