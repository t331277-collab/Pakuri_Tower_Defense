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

        /// CombatUnitEntry 인스턴스를 전달된 런타임 입력값으로 초기화한다.
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

        /// 전달된 actor 값을 사용해 Actor를 갱신한다.
        internal void SetActor(Component actor)
        {
            Actor = actor;
            Transform = actor.transform;
            if (HitboxRoot == null)
            {
                HitboxRoot = Transform;
            }

        }

        /// 전달된 hitboxRoot 값을 사용해 HitboxRoot를 갱신한다.
        internal void SetHitboxRoot(Transform hitboxRoot)
        {
            HitboxRoot = hitboxRoot;
            if (HitboxRoot == null)
            {
                HitboxRoot = Transform;
            }

        }

        /// 전달된 candidate 값을 사용해 소유한 컬렉션에 Transform가 있는지 반환한다.
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

        /// 전달된 런타임 입력값을 사용해 Damage를 표시한다.
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

        /// Display를 현재 런타임 모델을 기준으로 갱신한다.
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

        /// Defeat를 처리한다.
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

        /// 전달된 런타임 입력값을 사용해 요청값를 소유 런타임 Registry에 등록한다.
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

        /// 전달된 model 값을 사용해 요청값를 소유 런타임 Registry에서 등록 해제한다.
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

        /// 소유한 모든 런타임 값를 소유한 런타임 상태에서 비운다.
        public void Clear()
        {
            entries.Clear();
            players.Clear();
            enemies.Clear();
        }

        /// 전달된 model 값을 사용해 요청값를 찾는다.
        public CombatUnitEntry Find(UnitCombatState model)
        {
            return Find(model, null);
        }

        /// 전달된 collider 값을 사용해 ByCollider를 찾는다.
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

        /// 전달된 model 값을 사용해 Display를 현재 런타임 모델을 기준으로 갱신한다.
        public bool RefreshDisplay(UnitCombatState model)
        {
            var entry = Find(model);
            if (entry == null)
            {
                return false;
            }

            return entry.RefreshDisplay();
        }

        /// 전달된 런타임 입력값을 사용해 요청값를 찾는다.
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
