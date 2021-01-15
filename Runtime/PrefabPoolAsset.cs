using UnityEngine;
using System.Collections.Generic;

namespace Lab5Games
{
    [CreateAssetMenu(fileName ="PrefabPool Asset", menuName ="Lab5Games/PrefabPool")]
    public class PrefabPoolAsset : ScriptableObject, IPrefabPoolAsset
    {
        public PrefabPool[] pools;

        [System.NonSerialized] public Transform parent;
        [System.NonSerialized] private Dictionary<string, PrefabPool> _poolDict = new Dictionary<string, PrefabPool>();

        public void Init()
        {
            foreach(var pool in pools)
            {
                pool.CreatePool(parent);
            }
        }

        public void Release()
        {
            foreach(var pool in pools)
            {
                pool.DestroyPool();
            }

            parent = null;
            _poolDict.Clear();
        }

        public GameObject Spawn(GameObject prefab)
        {
            return Spawn(prefab.name);
        }

        public GameObject Spawn(string prefabName)
        {
            if(_poolDict.TryGetValue(prefabName, out PrefabPool pool))
            {
                return pool.Spawn();
            }
            else
            {
                throw new System.NullReferenceException($"Spawn GameObject failed, not found the PrefabPool({prefabName}).");
            }
        }

        public void Recycle(Transform transform)
        {
            Recycle(transform.gameObject);
        }

        public void Recycle<T>(T component) where T : Component
        {
            Recycle(component.gameObject);
        }

        public void Recycle(GameObject gameObject)
        {
            if(_poolDict.TryGetValue(gameObject.name, out PrefabPool pool))
            {
                pool.Recycle(gameObject, parent);
            }
            else
            {
                GameObject.Destroy(gameObject);
            }
        }
    }

    [System.Serializable]
    public class PrefabPool
    {
        public GameObject prefab;
        public int preloadCount;

        [System.NonSerialized] private Stack<GameObject> _avaliables = new Stack<GameObject>();
        [System.NonSerialized] private List<GameObject> _spawneds = new List<GameObject>();

        public string Name { get { return prefab.name; } }

        public GameObject Spawn()
        {
            GameObject go = null;

            if(_avaliables.Count > 0)
            {
                go = _avaliables.Pop();
            }
            else
            {
                go = GameObject.Instantiate(prefab);
                go.name = prefab.name;
                go.SetActive(false);
                go.transform.localPosition = Vector3.zero;
                go.transform.localRotation = Quaternion.identity;
            }

            go.SetActive(true);

            return go;
        }

        public void Recycle(GameObject gameObject, Transform parent)
        {
            if(_spawneds.Contains(gameObject))
            {
                gameObject.SetActive(false);

                if (parent) gameObject.transform.SetParent(parent);

                gameObject.transform.localPosition = Vector3.zero;
                gameObject.transform.localRotation = Quaternion.identity;

                _avaliables.Push(gameObject);
            }
            else
            {
                GameObject.Destroy(gameObject);
            }
        }

        internal void CreatePool(Transform parent)
        {
            if (prefab == null)
                throw new System.NullReferenceException("Prefab is null.");

            for(int i=0; i<preloadCount; i++)
            {
                GameObject newGo = GameObject.Instantiate(prefab);
                newGo.name = prefab.name;
                newGo.SetActive(false);

                if (parent) newGo.transform.SetParent(parent);

                newGo.transform.localPosition = Vector3.zero;
                newGo.transform.localRotation = Quaternion.identity;

                _avaliables.Push(newGo);
            }
        }

        internal void DestroyPool()
        {
            foreach(var go in _avaliables)
            {
                if (go) GameObject.Destroy(go);
            }

            foreach(var go in _spawneds)
            {
                if (go) GameObject.Destroy(go);
            }


            _avaliables.Clear();
            _spawneds.Clear();
        }
    }
}
