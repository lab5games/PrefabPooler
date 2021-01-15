﻿using UnityEngine;

namespace Lab5Games
{
    public interface IPrefabPoolAsset 
    {
        void Init();
        void Release();

        GameObject Spawn(string prefabName);
        void Recycle(GameObject gameObject);
    }
}
