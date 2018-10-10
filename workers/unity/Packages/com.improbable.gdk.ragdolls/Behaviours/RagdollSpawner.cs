﻿using Improbable.Gdk.GameObjectRepresentation;
using Improbable.Gdk.Health;
using Improbable.Gdk.ObjectPooling;
using UnityEngine;

namespace Improbable.Gdk.Ragdoll
{
    public class RagdollSpawner : MonoBehaviour
    {
        [Require] private HealthComponent.Requirable.Reader health;

        [SerializeField] private GameObject ragdollPrefab;
        [SerializeField] private UnityEngine.Transform transformToMatch;

        public delegate void RagdollSpawned(GameObject ragdoll);

        public RagdollSpawned OnRagdollSpawned;

        private ObjectPool<PoolableRagdoll> pool;
        private bool hasRagdollSpawned;

        private void OnEnable()
        {
            health.OnHealthModified += OnHealthModified;
            health.OnRespawn += (empty) => hasRagdollSpawned = false;
            pool = ObjectPooler.GetOrCreateObjectPool<PoolableRagdoll>(ragdollPrefab, 2);
        }

        private void OnHealthModified(HealthModifiedInfo info)
        {
            if (info.Died && !hasRagdollSpawned)
            {
                OnDeath(info.Modifier);
            }
        }

        private void OnDeath(HealthModifier deathDetails)
        {
            hasRagdollSpawned = true;
            var ragdoll = pool.Get();
            if (transformToMatch != null)
            {
                ragdoll.MatchTransforms(transformToMatch);
            }
            else
            {
                ragdoll.transform.position = transform.position;
            }

            OnRagdollSpawned?.Invoke(ragdoll.gameObject);

            ragdoll.LaunchRagdoll(deathDetails);
        }

        private void OnValidate()
        {
            ValidateRagdollPrefab(ragdollPrefab);
        }

        private void ValidateRagdollPrefab(GameObject prefab)
        {
            if (prefab.GetComponent<PoolableRagdoll>() == null)
            {
                Debug.LogError($"The Ragdoll prefab '{prefab.name}' is missing a PoolableRagdoll script.");
            }

            if (prefab.GetComponentInChildren<Rigidbody>() == null)
            {
                Debug.LogError(
                    $"The Ragdoll prefab '{prefab.name}' does not contain any Rigidbodies. "
                    + "The prefab will therefore not ragdoll, or be launched.");
            }
        }
    }
}
