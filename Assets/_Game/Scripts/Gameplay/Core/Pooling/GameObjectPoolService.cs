using System.Collections.Generic;
using UnityEngine;

namespace DienGame.Gameplay.Core.Pooling
{
    public static class GameObjectPoolService
    {
        // Giới hạn số object đang rảnh cho mỗi prefab để tránh pool tăng vô hạn.
        private const int MaxInactiveInstancesPerPrefab = 256;

        private static readonly Dictionary<int, Stack<GameObject>> Pools = new();
        private static readonly Dictionary<int, GameObject> Prefabs = new();
        private static Transform s_root;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            Pools.Clear();
            Prefabs.Clear();
            s_root = null;
        }

        public static void Warm(GameObject prefab, int count)
        {
            if (prefab == null || count <= 0)
            {
                return;
            }

            int key = prefab.GetInstanceID();
            Stack<GameObject> pool = GetOrCreatePool(prefab, key);

            int targetCount = Mathf.Min(count, MaxInactiveInstancesPerPrefab);

            while (pool.Count < targetCount)
            {
                GameObject instance = CreateInstance(prefab, key);
                instance.SetActive(false);
                instance.transform.SetParent(GetRoot(), false);
                pool.Push(instance);
            }
        }

        public static GameObject Spawn(
            GameObject prefab,
            Vector3 position,
            Quaternion rotation)
        {
            if (prefab == null)
            {
                return null;
            }

            int key = prefab.GetInstanceID();
            Stack<GameObject> pool = GetOrCreatePool(prefab, key);
            GameObject instance = null;

            while (pool.Count > 0 && instance == null)
            {
                instance = pool.Pop();
            }

            if (instance == null)
            {
                instance = CreateInstance(prefab, key);
            }

            Transform instanceTransform = instance.transform;
            instanceTransform.SetParent(null, false);
            instanceTransform.SetPositionAndRotation(position, rotation);

            PooledInstance marker = instance.GetComponent<PooledInstance>();
            if (marker != null)
            {
                marker.MarkRented();
            }

            instance.SetActive(true);
            marker?.NotifySpawned();

            return instance;
        }

        public static void Release(GameObject instance)
        {
            if (instance == null)
            {
                return;
            }

            PooledInstance marker = instance.GetComponent<PooledInstance>();

            if (marker == null || marker.PrefabKey == 0)
            {
                Object.Destroy(instance);
                return;
            }

            if (marker.IsInPool)
            {
                return;
            }

            // Scene cũ có thể đã bị unload hoặc RuntimePools bị hủy.
            Transform root = GetRoot();
            if (!Pools.TryGetValue(marker.PrefabKey, out Stack<GameObject> pool) ||
                !Prefabs.ContainsKey(marker.PrefabKey))
            {
                Object.Destroy(instance);
                return;
            }

            marker.NotifyReturned();
            marker.MarkReturned();

            instance.SetActive(false);

            if (pool.Count >= MaxInactiveInstancesPerPrefab)
            {
                Object.Destroy(instance);
                return;
            }

            instance.transform.SetParent(root, false);
            pool.Push(instance);
        }

        private static Stack<GameObject> GetOrCreatePool(GameObject prefab, int key)
        {
            // Bảo đảm pool cũ không sống qua một lần đổi/reload scene.
            GetRoot();

            if (!Pools.TryGetValue(key, out Stack<GameObject> pool))
            {
                pool = new Stack<GameObject>();
                Pools[key] = pool;
                Prefabs[key] = prefab;
            }

            return pool;
        }

        private static GameObject CreateInstance(GameObject prefab, int key)
        {
            GameObject instance = Object.Instantiate(prefab);
            instance.name = prefab.name + "_Pooled";

            PooledInstance marker = instance.GetComponent<PooledInstance>();
            if (marker == null)
            {
                marker = instance.AddComponent<PooledInstance>();
            }

            marker.Initialize(key);
            return instance;
        }

        private static Transform GetRoot()
        {
            if (s_root != null)
            {
                return s_root;
            }

            // Root thuộc scene gameplay đã bị hủy thì các instance rảnh
            // cũng đã bị hủy. Xóa cache để tránh tham chiếu Unity "fake null".
            Pools.Clear();
            Prefabs.Clear();

            GameObject root = new GameObject("RuntimePools");
            s_root = root.transform;
            return s_root;
        }
    }

    [DisallowMultipleComponent]
    internal sealed class PooledInstance : MonoBehaviour
    {
        private readonly List<IPoolLifecycle> _lifecycles = new();
        private bool _cacheBuilt;

        public int PrefabKey { get; private set; }
        public bool IsInPool { get; private set; }

        public void Initialize(int prefabKey)
        {
            PrefabKey = prefabKey;
            IsInPool = false;
        }

        public void MarkRented()
        {
            IsInPool = false;
        }

        public void MarkReturned()
        {
            IsInPool = true;
        }

        public void NotifySpawned()
        {
            EnsureLifecycleCache();

            for (int i = 0; i < _lifecycles.Count; i++)
            {
                _lifecycles[i]?.OnSpawnedFromPool();
            }
        }

        public void NotifyReturned()
        {
            EnsureLifecycleCache();

            for (int i = 0; i < _lifecycles.Count; i++)
            {
                _lifecycles[i]?.OnReturnedToPool();
            }
        }

        private void EnsureLifecycleCache()
        {
            if (_cacheBuilt)
            {
                return;
            }

            _cacheBuilt = true;
            _lifecycles.Clear();

            MonoBehaviour[] behaviours = GetComponentsInChildren<MonoBehaviour>(true);

            for (int i = 0; i < behaviours.Length; i++)
            {
                if (behaviours[i] is IPoolLifecycle lifecycle)
                {
                    _lifecycles.Add(lifecycle);
                }
            }
        }
    }
}
