using System.Collections.Generic;
using UnityEngine;

/*
 * 전투 유닛의 모델과 Actor를 한 항목으로 연결하고 등록된 아군·적 목록을 관리한다.
 */
namespace Pakuri.InGame
{
    public class CombatUnitEntry
    {
        private Collider2D[] cachedHitboxColliders;

        /*
         * CombatUnitEntry에 필요한 값을 초기화한다.
         */
        public CombatUnitEntry(UnitCombatState model, Component actor, Transform hitboxRoot = null)
        {
            Model = model;
            SetActor(actor);
            SetHitboxRoot(hitboxRoot);
        }

        public UnitCombatState Model { get; }
        public Component Actor { get; private set; }
        public Transform Transform { get; private set; }
        public Transform HitboxRoot { get; private set; }
        public bool IsAlive => Model.Resources.CurrentHealth > 0f;

        /*
         * SetActor에 필요한 값을 설정한다.
         */
        internal void SetActor(Component actor)
        {
            Actor = actor;
            Transform = actor.transform;
            if (HitboxRoot == null)
            {
                HitboxRoot = Transform;
            }

            cachedHitboxColliders = null;
        }

        /*
         * SetHitboxRoot에 필요한 값을 설정한다.
         */
        internal void SetHitboxRoot(Transform hitboxRoot)
        {
            HitboxRoot = hitboxRoot;
            if (HitboxRoot == null)
            {
                HitboxRoot = Transform;
            }

            cachedHitboxColliders = null;
        }

        /*
         * ResolveTargetPoint 결과를 계산해 반환한다.
         */
        public Vector3 ResolveTargetPoint()
        {
            return HitboxRoot.position;
        }

        /*
         * GetHitboxColliders에 해당하는 값을 찾아 반환한다.
         */
        public Collider2D[] GetHitboxColliders()
        {
            if (cachedHitboxColliders == null)
            {
                cachedHitboxColliders = HitboxRoot.GetComponentsInChildren<Collider2D>();
            }

            return cachedHitboxColliders;
        }

        /*
         * ContainsTransform 조건을 만족하는지 확인한다.
         */
        public bool ContainsTransform(Transform candidate)
        {
            if (candidate == null)
            {
                return false;
            }

            if (candidate == Transform || candidate.IsChildOf(Transform))
            {
                return true;
            }

            return candidate == HitboxRoot || candidate.IsChildOf(HitboxRoot);
        }

        /*
         * ShowDamage 작업을 수행한다.
         */
        internal void ShowDamage(float damageAmount, bool isDead)
        {
            if (damageAmount <= 0f)
            {
                return;
            }

            if (Actor is MonsterActor monster)
            {
                monster.ShowDamage(damageAmount);
                if (!isDead)
                {
                    monster.TryPlayHitAnimation();
                }

                return;
            }

            if (Actor is EnemyActor enemy)
            {
                enemy.ShowDamage(damageAmount);
            }
        }

        /*
         * RefreshDisplay 대상의 현재 상태를 갱신하고 결과를 반환한다.
         */
        internal bool RefreshDisplay()
        {
            if (Actor is MonsterActor monster)
            {
                monster.RefreshDisplay();
                return true;
            }

            if (Actor is EnemyActor enemy)
            {
                enemy.RefreshDisplay();
                return true;
            }

            if (Actor is NexusActor nexus)
            {
                nexus.RefreshDisplay();
                return true;
            }

            return false;
        }

        /*
         * HandleDefeat 작업을 수행한다.
         */
        internal void HandleDefeat()
        {
            if (Actor is NexusActor nexus)
            {
                nexus.RefreshDisplay();
                return;
            }

            if (Actor is MonsterActor monster)
            {
                monster.Defeat();
                return;
            }

            Object.Destroy(Actor.gameObject, 0.95f);
        }
    }

    public class CombatUnitRegistry
    {
        private readonly List<CombatUnitEntry> entries = new List<CombatUnitEntry>();
        private readonly List<CombatUnitEntry> players = new List<CombatUnitEntry>();
        private readonly List<CombatUnitEntry> enemies = new List<CombatUnitEntry>();

        public IReadOnlyList<CombatUnitEntry> Entries => entries;
        public IReadOnlyList<CombatUnitEntry> Players => players;
        public IReadOnlyList<CombatUnitEntry> Enemies => enemies;
        public int EnemyCount => enemies.Count;

        /*
         * Register 작업 결과를 반환한다.
         */
        public CombatUnitEntry Register(UnitCombatState model, Component actor, Transform hitboxRoot = null)
        {
            var existing = Find(model, actor);
            if (existing != null)
            {
                existing.SetActor(actor);
                existing.SetHitboxRoot(hitboxRoot);
                return existing;
            }

            var entry = new CombatUnitEntry(model, actor, hitboxRoot);
            entries.Add(entry);

            if (model.Identity.Side == UnitSide.Enemy)
            {
                enemies.Add(entry);
            }
            else
            {
                players.Add(entry);
            }

            return entry;
        }

        /*
         * Unregister 작업 결과를 반환한다.
         */
        public bool Unregister(UnitCombatState model)
        {
            var entry = Find(model, null);
            if (entry == null)
            {
                return false;
            }

            entries.Remove(entry);
            players.Remove(entry);
            enemies.Remove(entry);
            return true;
        }

        /*
         * Clear 작업을 수행한다.
         */
        public void Clear()
        {
            entries.Clear();
            players.Clear();
            enemies.Clear();
        }

        /*
         * Find에 해당하는 값을 찾아 반환한다.
         */
        public CombatUnitEntry Find(UnitCombatState model)
        {
            return Find(model, null);
        }

        /*
         * FindByCollider에 해당하는 값을 찾아 반환한다.
         */
        public CombatUnitEntry FindByCollider(Collider2D collider)
        {
            for (var i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                if (entry.ContainsTransform(collider.transform))
                {
                    return entry;
                }
            }

            return null;
        }

        /*
         * RefreshDisplay 대상의 현재 상태를 갱신하고 결과를 반환한다.
         */
        public bool RefreshDisplay(UnitCombatState model)
        {
            var entry = Find(model);
            if (entry == null)
            {
                return false;
            }

            return entry.RefreshDisplay();
        }

        /*
         * Find에 해당하는 값을 찾아 반환한다.
         */
        private CombatUnitEntry Find(UnitCombatState model, Component actor)
        {
            var unitId = model.Identity.UnitId;
            for (var i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                if (entry.Model.Identity.UnitId == unitId)
                {
                    return entry;
                }

                if (actor != null && entry.Actor == actor)
                {
                    return entry;
                }
            }

            return null;
        }
    }
}
