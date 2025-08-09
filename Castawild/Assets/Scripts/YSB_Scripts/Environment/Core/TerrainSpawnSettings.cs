using System.Collections.Generic;
using UnityEngine;
using Fusion;

[System.Serializable]
public class TerrainSpawnSettings
{
    public Terrain terrain;
    public int spawnTextureLayerIndex;
    public int maxObjects = 10;
    public float minSpawnHeight = 50f;
    public float minDistanceBetweenObjects = 4f;
    public List<NetworkObject> activeObjects = new();
}