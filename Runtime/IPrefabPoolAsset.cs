using UnityEngine;

namespace Lab5Games.Pooling
{
    public interface IPrefabPoolAsset 
    {
        void Init();
        void Release();

        GameObject Spawn(string prefabName);
        void Recycle(GameObject gameObject);
    }
}
