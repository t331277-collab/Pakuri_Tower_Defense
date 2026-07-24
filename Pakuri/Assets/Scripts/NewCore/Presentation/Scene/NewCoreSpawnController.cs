using System;
using System.Collections.Generic;
using Pakuri.NewCore.Combat.Actions;
using Pakuri.NewCore.Combat.Skills.Execution;
using Pakuri.NewCore.Presentation.Actors;
using Pakuri.NewCore.Spawn;
using Pakuri.NewCore.Units.Models;
using UnityEngine;

namespace Pakuri.NewCore.Presentation.Scene
{
    public sealed class NewCoreSpawnController : MonoBehaviour
    {
        [Serializable]
        public struct EnemyPrefabBinding
        {
            [SerializeField] private string enemyId;
            [SerializeField] private GameObject prefab;

            public string EnemyId => enemyId;
            public GameObject Prefab => prefab;
        }

        [SerializeField] private NewCoreSceneRuntime combatManager;
        [SerializeField] private Transform playerSpawnPoint;
        [SerializeField] private Transform enemySpawnPoint;
        [SerializeField] private Transform runtimeEnemyRoot;
        [SerializeField] private Transform runtimeMonsterRoot;
        [SerializeField] private GameObject arielUnitPrefab;
        [SerializeField] private GameObject eveUnitPrefab;
        [SerializeField] private GameObject rinUnitPrefab;
        [SerializeField] private GameObject seinUnitPrefab;
        [SerializeField] private GameObject vegaUnitPrefab;
        [SerializeField] private EnemyPrefabBinding[] enemyPrefabBindings =
            Array.Empty<EnemyPrefabBinding>();

        private readonly Dictionary<MonsterModel, MonsterActorBehaviour> monsters =
            new Dictionary<MonsterModel, MonsterActorBehaviour>();
        private readonly Dictionary<EnemyModel, EnemyActorBehaviour> enemies =
            new Dictionary<EnemyModel, EnemyActorBehaviour>();
        private readonly Dictionary<string, GameObject> enemyPrefabs =
            new Dictionary<string, GameObject>(StringComparer.Ordinal);
        private readonly List<EnemyModel> defeatedEnemies =
            new List<EnemyModel>();
        private SpawnManager spawns;
        private NewCoreSceneRuntime runtime;

        public void Bind(
            NewCoreSceneRuntime sceneRuntime,
            SpawnManager spawnManager)
        {
            runtime = sceneRuntime
                ?? throw new ArgumentNullException(nameof(sceneRuntime));
            spawns = spawnManager
                ?? throw new ArgumentNullException(nameof(spawnManager));
            if (combatManager == null)
            {
                combatManager = GetComponent<NewCoreSceneRuntime>();
            }

            if (!ReferenceEquals(combatManager, runtime)
                || playerSpawnPoint == null
                || enemySpawnPoint == null
                || runtimeEnemyRoot == null
                || runtimeMonsterRoot == null)
            {
                throw new InvalidOperationException(
                    "New Core spawn scene references are incomplete.");
            }

            BuildEnemyLookup();
        }

        public MonsterActorBehaviour EnsureMonster(MonsterModel model)
        {
            if (monsters.TryGetValue(model, out var actor))
            {
                return actor;
            }

            var prefab = ResolveMonsterPrefab(model.MonsterDefinition.id);
            var slot = runtime.Stage.Session.PartyRoster.Members.IndexOf(model);
            if (slot < 0)
            {
                throw new InvalidOperationException(
                    $"Monster '{model.MonsterDefinition.id}' is not in the party roster.");
            }

            var point = ResolvePartySpawnPoint(slot);
            var instance = Instantiate(
                prefab,
                point.position,
                prefab.transform.rotation,
                runtimeMonsterRoot);
            actor = instance.GetComponent<MonsterActorBehaviour>();
            if (actor == null)
            {
                throw new InvalidOperationException(
                    $"Monster prefab '{prefab.name}' has no New Core actor.");
            }

            actor.Bind(model);
            monsters.Add(model, actor);
            runtime.RegisterMonster(model, actor, slot == 0);
            return actor;
        }

        public void SyncNewSpawns()
        {
            var party = runtime.Stage.Session.PartyRoster.Members;
            for (var index = 0; index < party.Count; index++)
            {
                EnsureMonster(party[index]);
            }

            var records = spawns.SpawnedEnemies;
            for (var index = 0; index < records.Count; index++)
            {
                var model = records[index].Model;
                if (!model.IsAlive
                    || model.HasContactedNexus
                    || enemies.ContainsKey(model))
                {
                    continue;
                }

                var prefab = ResolveEnemyPrefab(
                    model.EnemyDefinition.enemy_id);
                var position = model.Position;
                var instance = Instantiate(
                    prefab,
                    new Vector3(
                        position.X,
                        position.Y,
                        enemySpawnPoint != null
                            ? enemySpawnPoint.position.z
                            : prefab.transform.position.z),
                    prefab.transform.rotation,
                    runtimeEnemyRoot);
                var actor = instance.GetComponent<EnemyActorBehaviour>();
                if (actor == null)
                {
                    throw new InvalidOperationException(
                        $"Enemy prefab '{prefab.name}' has no New Core actor.");
                }

                actor.Bind(model);
                enemies.Add(model, actor);
                runtime.RegisterEnemy(model, actor);
            }
        }

        public void SyncActors()
        {
            foreach (var actor in monsters.Values)
            {
                if (actor != null)
                {
                    actor.SyncFromModel();
                }
            }

            defeatedEnemies.Clear();
            foreach (var pair in enemies)
            {
                var actor = pair.Value;
                if (actor != null)
                {
                    actor.SyncFromModel();
                }

                if (!pair.Key.IsAlive
                    || pair.Key.HasContactedNexus)
                {
                    defeatedEnemies.Add(pair.Key);
                }
            }

            for (var index = 0;
                index < defeatedEnemies.Count;
                index++)
            {
                enemies.Remove(defeatedEnemies[index]);
            }
        }

        public bool TryGetActor(
            UnitBaseModel model,
            out UnitActorBehaviour actor)
        {
            if (model is MonsterModel monster
                && monsters.TryGetValue(monster, out var monsterActor))
            {
                actor = monsterActor;
                return true;
            }

            if (model is EnemyModel enemy
                && enemies.TryGetValue(enemy, out var enemyActor))
            {
                actor = enemyActor;
                return true;
            }

            actor = null;
            return false;
        }

        public CombatFootprint ResolveCombatFootprint(
            UnitBaseModel model)
        {
            if (!TryGetActor(model, out UnitActorBehaviour actor)
                || actor == null)
            {
                return default;
            }

            Collider2D collider =
                actor.GetComponentInChildren<Collider2D>(true);
            if (collider == null)
            {
                return default;
            }

            Bounds bounds = collider.bounds;
            Vector3 actorPosition = actor.transform.position;
            return new CombatFootprint(
                new CombatVector2(
                    bounds.center.x - actorPosition.x,
                    bounds.center.y - actorPosition.y),
                bounds.extents.x,
                bounds.extents.y);
        }

        private void BuildEnemyLookup()
        {
            enemyPrefabs.Clear();
            for (var index = 0; index < enemyPrefabBindings.Length; index++)
            {
                var binding = enemyPrefabBindings[index];
                if (string.IsNullOrWhiteSpace(binding.EnemyId)
                    || binding.Prefab == null)
                {
                    throw new InvalidOperationException(
                        "Enemy prefab binding is incomplete.");
                }

                enemyPrefabs.Add(binding.EnemyId, binding.Prefab);
            }
        }

        private GameObject ResolveMonsterPrefab(string monsterId)
        {
            switch (monsterId)
            {
                case "ariel": return Require(arielUnitPrefab, monsterId);
                case "eve": return Require(eveUnitPrefab, monsterId);
                case "rin": return Require(rinUnitPrefab, monsterId);
                case "sein": return Require(seinUnitPrefab, monsterId);
                case "vega": return Require(vegaUnitPrefab, monsterId);
                default:
                    throw new InvalidOperationException(
                        $"No monster prefab is mapped for '{monsterId}'.");
            }
        }

        private GameObject ResolveEnemyPrefab(string enemyId)
        {
            if (!enemyPrefabs.TryGetValue(enemyId, out var prefab))
            {
                throw new InvalidOperationException(
                    $"No enemy prefab is mapped for '{enemyId}'.");
            }

            return prefab;
        }

        private Transform ResolvePartySpawnPoint(int slot)
        {
            if (slot == 0 && playerSpawnPoint != null)
            {
                return playerSpawnPoint;
            }

            var target = GameObject.Find($"{slot + 1}PSpawnPoint");
            if (target == null)
            {
                throw new InvalidOperationException(
                    $"Party spawn point {slot + 1}PSpawnPoint is missing.");
            }

            return target.transform;
        }

        private static GameObject Require(
            GameObject prefab,
            string unitId)
        {
            return prefab != null
                ? prefab
                : throw new InvalidOperationException(
                    $"Unit prefab '{unitId}' is missing.");
        }
    }

    internal static class PartyRosterIndex
    {
        public static int IndexOf(
            this IReadOnlyList<MonsterModel> members,
            MonsterModel model)
        {
            for (var index = 0; index < members.Count; index++)
            {
                if (ReferenceEquals(members[index], model))
                {
                    return index;
                }
            }

            return -1;
        }
    }
}
