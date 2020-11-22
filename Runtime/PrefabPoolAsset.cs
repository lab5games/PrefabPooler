using UnityEngine;
using System;
using System.Collections.Generic;

namespace Lab5Games
{
    [CreateAssetMenu(fileName ="PrefabPool Asset", menuName ="Lab5Games/PrefabPool")]
    public class PrefabPoolAsset : ScriptableObject
    {
        public PrefabPool[] pools;

        [System.NonSerialized] Transform _parentTransform;
        [System.NonSerialized] Dictionary<string, PrefabPool> _poolNameDict = new Dictionary<string, PrefabPool>();


        public GameObject Spawn(GameObject prefab)
        {
            return Spawn(prefab.name);
        }

        public GameObject Spawn(string prefabName)
        {
            PrefabPool pool = null;
            if (_poolNameDict.TryGetValue(prefabName, out pool))
            {
                return pool.Spawn();
            }
            else
            {
                Debug.LogWarning($"Spawn object failed, not found the PrefabPool({prefabName}).");
                return null;
            }
        }

        public void Recycle(GameObject gameObject)
        {
            if (gameObject == null)
                throw new ArgumentNullException(nameof(gameObject));

            PrefabPool pool = null;
            if(_poolNameDict.TryGetValue(gameObject.name, out pool))
            {
                pool.Recycle(gameObject, _parentTransform);
            }
            else
            {
                Debug.LogWarning($"Recycle object failed, not found the PrefabPool({gameObject.name}).");
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
            _parentTransform = parentTransform;

            foreach(var p in pools)
            {
                p.CreatePool(parentTransform);
            }
        }

        public void Relase()
        {
            foreach(var p in pools)
            {
                p.DestoryPool();
            }

            _parentTransform = null;
            _poolNameDict.Clear();
        }
    }

    [System.Serializable]
    public class PrefabPool
    {
        public GameObject prefab;
        public int preloadCount;

        [System.NonSerialized] Stack<GameObject> _avaliables = new Stack<GameObject>();
        [System.NonSerialized] List<GameObject> _spawneds = new List<GameObject>();

        public string PrefabName { get { return prefab.name; } }

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
                go.transform.localEulerAngles = Vector3.zero;
            }

            go.SetActive(true);

            return go;
        }

        public void Recycle(GameObject gameObject, Transform parentTransform)
        {
            if(_spawneds.Remove(gameObject))
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

        internal void CreatePool(Transform parentTransform)
        {
            if (prefab == null)
                throw new NullReferenceException("Prefab is null.");

            for(int i=0; i<preloadCount; i++)
            {
                GameObject newGo = GameObject.Instantiate(prefab);
                newGo.name = prefab.name;
                newGo.SetActive(false);
                if(parentTransform != null) newGo.transform.SetParent(parentTransform);
                newGo.transform.localPosition = Vector3.zero;
                newGo.transform.localEulerAngles = Vector3.zero;

                _avaliables.Push(newGo);
            }
        }

        internal void DestoryPool()
        {
            foreach(var go in _avaliables)
            {
                if (go != null) GameObject.Destroy(go);
            }

            foreach(var go in _spawneds)
            {
                if (go != null) GameObject.Destroy(go);
            }

            _avaliables.Clear();
            _spawneds.Clear();
        }
    }
}
