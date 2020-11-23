using System;
using System.Collections.Generic;

namespace Lab5Games
{
    public static class PrefabPooler
    { 
        public static PrefabPoolAssetDict Pools = new PrefabPoolAssetDict();
    }

    public class PrefabPoolAssetDict : Dictionary<string, IPrefabPoolAsset>
    {

    }
}
