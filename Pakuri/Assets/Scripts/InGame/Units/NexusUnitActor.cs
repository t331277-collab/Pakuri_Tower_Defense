using System;
using TMPro;
using UnityEngine;

namespace Pakuri.InGame
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(BoxCollider2D))]
    public sealed class NexusUnitActor : MonoBehaviour
    {
        private const string NexusUnitId = "nexus";
        private const float DefaultMaxHealth = 20f;

        [SerializeField] private float maxHealth = DefaultMaxHealth;
        [SerializeField] private TextMeshProUGUI nexusHpInfo;

        private bool defeatNotified;

        public NexusUnitRuntimeModel Model { get; private set; }
        public event Action<NexusUnitActor> Defeated;

        public void Initialize()
        {
            if (Model == null)
            {
                var resolvedMaxHealth = Mathf.Max(1f, maxHealth);
                Model = new NexusUnitRuntimeModel
                {
                    Identity = new UnitIdentity
                    {
                        UnitId = NexusUnitId,
                        DefinitionId = NexusUnitId,
                        DisplayName = "Nexus",
                        Side = UnitSide.Player,
                        Role = UnitRole.Nexus,
                        SlotIndex = 100
                    },
                    Stats = new UnitStatsRuntime
                    {
                        MaxHealth = resolvedMaxHealth
                    },
                    Resources = new UnitResourceRuntime
                    {
                        CurrentHealth = resolvedMaxHealth,
                        CurrentShield = 0f
                    },
                    AutoAttackEnabled = false,
                    AutoSkillEnabled = false
                };
            }

            ResolveReferences();
            EnsureCollider();
            RefreshDebugView();
        }

        public void RefreshDebugView()
        {
            ResolveReferences();
            if (nexusHpInfo == null || Model == null || Model.Resources == null || Model.Stats == null)
            {
                return;
            }

            var current = Mathf.CeilToInt(Mathf.Max(0f, Model.Resources.CurrentHealth));
            var maximum = Mathf.CeilToInt(Mathf.Max(0f, Model.Stats.MaxHealth));
            nexusHpInfo.text = $"{current} / {maximum}";
        }

        public bool TryGetCurrentHealth(out float currentHealth)
        {
            var resources = Model != null ? Model.Resources : null;
            if (resources == null)
            {
                currentHealth = 0f;
                return false;
            }

            currentHealth = Mathf.Max(0f, resources.CurrentHealth);
            return true;
        }

        public void SetCurrentHealth(float currentHealth)
        {
            var resources = Model != null ? Model.Resources : null;
            var stats = Model != null ? Model.Stats : null;
            if (resources == null || stats == null)
            {
                return;
            }

            resources.CurrentHealth = Mathf.Clamp(Mathf.Round(currentHealth), 0f, Mathf.Max(1f, stats.MaxHealth));
            if (resources.CurrentHealth > 0f)
            {
                defeatNotified = false;
            }

            RefreshDebugView();
        }

        public void NotifyDefeated()
        {
            if (defeatNotified)
            {
                return;
            }

            defeatNotified = true;
            RefreshDebugView();
            Defeated?.Invoke(this);
        }

        private void ResolveReferences()
        {
            if (nexusHpInfo == null)
            {
                var hpInfoObject = FindSceneGameObjectByPath("Canvas/Info/NexusHPinfo");
                nexusHpInfo = hpInfoObject != null ? hpInfoObject.GetComponent<TextMeshProUGUI>() : null;
            }
        }

        private void EnsureCollider()
        {
            var collider = GetComponent<BoxCollider2D>();
            if (collider == null)
            {
                collider = gameObject.AddComponent<BoxCollider2D>();
            }

            collider.isTrigger = true;
        }

        private static GameObject FindSceneGameObjectByPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return null;
            }

            var objects = Resources.FindObjectsOfTypeAll<GameObject>();
            for (var i = 0; i < objects.Length; i++)
            {
                var candidate = objects[i];
                if (candidate == null || !candidate.scene.IsValid())
                {
                    continue;
                }

                if (string.Equals(BuildPath(candidate.transform), path, StringComparison.Ordinal))
                {
                    return candidate;
                }
            }

            return null;
        }

        private static string BuildPath(Transform transform)
        {
            if (transform == null)
            {
                return string.Empty;
            }

            var path = transform.name;
            var parent = transform.parent;
            while (parent != null)
            {
                path = parent.name + "/" + path;
                parent = parent.parent;
            }

            return path;
        }
    }
}
