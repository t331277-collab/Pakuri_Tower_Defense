/*
 * 역할: 비공개 런타임 유닛 Registry 보조.
 * 책임: UnitSpawnManager 조회와 표시 전달에 쓰이는 모델·Actor·Transform·Collider 연결을 보관한다.
 */

using System.Collections.Generic;
using UnityEngine;

namespace Pakuri.InGame
{

    /// CombatUnitEntry 한 항목의 런타임 모델과 씬 참조를 연결한다.
    public class CombatUnitEntry
    {

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

        internal void SetActor(Component actor)
        {
            Actor = actor;
            Transform = actor.transform;
            if (HitboxRoot == null)
            {
                HitboxRoot = Transform;
            }

        }

        internal void SetHitboxRoot(Transform hitboxRoot)
        {
            HitboxRoot = hitboxRoot;
            if (HitboxRoot == null)
            {
                HitboxRoot = Transform;
            }

        }

        internal bool ContainsTransform(Transform candidate)
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

        internal void ShowDamage(float damageAmount, bool isDead, bool isCritical)
        {
            if (damageAmount <= 0f)
            {
                return;
            }

            if (Actor is MonsterActor monster)
            {
                monster.ShowDamage(damageAmount, isCritical);
                if (!isDead)
                {
                    monster.TryPlayHitAnimation();
                }

                return;
            }

            if (Actor is EnemyActor enemy)
            {
                enemy.ShowDamage(damageAmount, isCritical);
            }
        }

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

        /// 패배한 유닛을 Registry에서 제거하고 Actor에 패배 상태를 알린다.
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

    /// UnitSpawnManager가 사용하는 유닛 모델·Actor·Transform·Collider 연결 정보를 보관한다.
    internal sealed class CombatUnitRegistry
    {
        private readonly List<CombatUnitEntry> entries = new List<CombatUnitEntry>();
        private readonly List<CombatUnitEntry> players = new List<CombatUnitEntry>();
        private readonly List<CombatUnitEntry> enemies = new List<CombatUnitEntry>();

        public IReadOnlyList<CombatUnitEntry> Entries => entries;
        public IReadOnlyList<CombatUnitEntry> Players => players;
        public IReadOnlyList<CombatUnitEntry> Enemies => enemies;
        public int EnemyCount => enemies.Count;

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

        public void Clear()
        {
            entries.Clear();
            players.Clear();
            enemies.Clear();
        }

        public CombatUnitEntry Find(UnitCombatState model)
        {
            return Find(model, null);
        }

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

        public bool RefreshDisplay(UnitCombatState model)
        {
            var entry = Find(model);
            if (entry == null)
            {
                return false;
            }

            return entry.RefreshDisplay();
        }

        private CombatUnitEntry Find(UnitCombatState model, Component actor)
        {
            if (model == null || model.Identity == null)
            {
                return null;
            }

            var unitName = model.Identity.UnitName;
            for (var i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                if (entry != null
                    && entry.Model != null
                    && entry.Model.Identity != null
                    && entry.Model.Identity.UnitName == unitName)
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
