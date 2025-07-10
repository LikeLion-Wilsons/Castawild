using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TerrainSpawnSettings
{
    public Terrain terrain;
    public int spawnTextureLayerIndex = 0;
    public int maxTrees = 100;
    [HideInInspector] public List<GameObject> activeTrees = new();
}

