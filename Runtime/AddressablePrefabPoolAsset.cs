using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Lab5Games
{
    [CreateAssetMenu(fileName ="AddressablePrefabPool Asset", menuName ="Lab5Games/Addressable PrefabPool")]
    public class AddressablePrefabPoolAsset : ScriptableObject, IPrefabPoolAsset
    {
        public AddressablePrefabPool[] pools;

        [System.NonSerialized] Transform _parentTransform;
        [System.NonSerialized] Dictionary<string, AddressablePrefabPool> _poolNameDict = new Dictionary<string, AddressablePrefabPool>();
        [System.NonSerialized] Dictionary<AssetReferenceGameObject, AddressablePrefabPool> _poolReferenceDict = new Dictionary<AssetReferenceGameObject, AddressablePrefabPool>();

        public bool isReady { get; private set; }

        public GameObject Spawn(string prefabName)
        {
            AddressablePrefabPool pool = null;
            if (_poolNameDict.TryGetValue(prefabName, out pool))
            {
                return pool.Spawn();
            }
            else
            {
                Debug.LogWarning($"Spawn object failed, not found the AddressablePrefabPool({prefabName}).");
                return null;
            }
        }

        public GameObject Spawn(AssetReferenceGameObject prefabReference)
        {
            AddressablePrefabPool pool = null;
            if(_poolReferenceDict.TryGetValue(prefabReference, out pool))
            {
                return pool.Spawn();
            }
            else
            {
                Debug.LogWarning($"Spawn object failed, not found the AddressablePrefabPool({prefabReference.RuntimeKey}).");
                return null;
            }
        }

        public void Recycle(GameObject gameObject)
        {
            if (gameObject == null)
                throw new ArgumentNullException(nameof(gameObject));

            AddressablePrefabPool pool = null;
            if (_poolNameDict.TryGetValue(gameObject.name, out pool))
            {
                pool.Recycle(gameObject, _parentTransform);
            }
            else
            {
                Debug.LogWarning($"Recycle object failed, not found the AddressablePrefabPool({gameObject.name}).");
                GameObject.Destroy(gameObject);
            }
        }

        public void Recycle(Transform transform)
        {
            if (transform == null)
                throw new ArgumentNullException(nameof(transform));

            Recycle(transform.gameObject);
        }

        public void Recycle<T>(T component) where T : Component
        {
            if (component == null)
                throw new ArgumentNullException(nameof(component));

            Recycle(component.gameObject);
        }
        
        public void Init(Transform parentTransform)
        {
            isReady = false;
            _parentTransform = parentTransform;

            foreach(var p in pools)
            {
                p.CreatePool(parentTransform, OnPoolCreate);
            }
        }

        private void OnPoolCreate(AddressablePrefabPool pool)
        {
            _poolNameDict.Add(pool.PrefabName, pool);
            _poolReferenceDict.Add(pool.prefabReference, pool);

            if(_poolNameDict.Count == pools.Length && _poolReferenceDict.Count == pools.Length)
            {
                isReady = true;
                PrefabPooler.Pools.Add(name, this);
            }
        }

        public void Release()
        {
            foreach(var p in pools)
            {
                p.DestroyPool();
            }

            isReady = false;
            _parentTransform = null;
            
            _poolNameDict.Clear();
            _poolReferenceDict.Clear();

            PrefabPooler.Pools.Remove(name);
        }
    }

    [System.Serializable]
    public class AddressablePrefabPool
    {
        public AssetReferenceGameObject prefabReference;
        public int preloadCount;

        [System.NonSerialized] GameObject _prefab;

        [System.NonSerialized] Stack<GameObject> _avaliables = new Stack<GameObject>();
        [System.NonSerialized] List<GameObject> _spawneds = new List<GameObject>();

        public string PrefabName { get { return _prefab.name; } }

        public delegate void AddressablePrefabPoolCallback(AddressablePrefabPool pool);


        public GameObject Spawn()
        {
            GameObject go = null;

            if (_avaliables.Count > 0)
            {
                go = _avaliables.Pop();
            }
            else
            {
                go = GameObject.Instantiate(_prefab);
                go.name = _prefab.name;
                go.SetActive(false);
                go.transform.localPosition = Vector3.zero;
                go.transform.localEulerAngles = Vector3.zero;
            }

            go.SetActive(true);

            return go;
        }

        public void Recycle(GameObject gameObject, Transform parentTransform)
        {
            if (_spawneds.Remove(gameObject))
            {
                gameObject.SetActive(false);
                if (parentTransform != null) gameObject.transform.SetParent(parentTransform);
                gameObject.transform.localPosition = Vector3.zero;
                gameObject.transform.localEulerAngles = Vector3.zero;

                _avaliables.Push(gameObject);
            }
            else
            {
                GameObject.Destroy(gameObject);
            }
        }

        internal void CreatePool(Transform parentTransform, AddressablePrefabPoolCallback createCallback)
        {
            if (prefabReference == null)
                throw new NullReferenceException("Prefab reference is null.");

            if (prefabReference.RuntimeKeyIsValid() == false)
                throw new Exception("Invalid runtime key of prefab reference: " + prefabReference.RuntimeKey);

            Addressables.LoadAssetAsync<GameObject>(prefabReference).Completed += (handle) =>
            {
                _prefab = handle.Result;

                if (_prefab == null)
                    throw new NullReferenceException("Load prefab asset failed.");

                for (int i = 0; i < preloadCount; i++)
                {
                    GameObject newGo = GameObject.Instantiate(_prefab);
                    newGo.name = _prefab.name;
                    newGo.SetActive(false);
                    if (parentTransform != null) newGo.transform.SetParent(parentTransform);
                    newGo.transform.localPosition = Vector3.zero;
                    newGo.transform.localEulerAngles = Vector3.zero;

                    _avaliables.Push(newGo);
                }

                createCallback?.Invoke(this);
            };
        }

        internal void DestroyPool()
        {
            foreach (var go in _avaliables)
            {
                if (go != null) GameObject.Destroy(go);
            }

            foreach (var go in _spawneds)
            {
                if (go != null) GameObject.Destroy(go);
            }
            
            _prefab = null;

            _avaliables.Clear();
            _spawneds.Clear();

            Addressables.Release(prefabReference);
        }
    }
}
