using Fusion;
using UnityEngine;
using static UnityEditor.PlayerSettings;
using static UnityEngine.Rendering.DebugUI.Table;

public class TestPlayerSpawner : NetworkBehaviour, IPlayerJoined, IPlayerLeft
{
    [SerializeField] private NetworkObject playerPrefab;

    public void PlayerJoined(PlayerRef playerRef)
    {
        if (!HasStateAuthority)
            return;

        Vector3 spawnPos = new Vector3(0f, 2f, 0f);

        // 1. 플레이어 오브젝트 생성
        Debug.Log($"Spawning player for {playerRef}");
        var playerObj = Runner.Spawn(playerPrefab, spawnPos, Quaternion.identity, playerRef);
        if (playerObj == null)
            Debug.LogError("Spawned player is NULL");
        else
            Debug.Log("Spawned player successfully");

        // 2. LocalPlayerObject 연결
        Runner.SetPlayerObject(playerRef, playerObj);
    }

    public void PlayerLeft(PlayerRef playerRef)
    {
        if (HasStateAuthority == false)
            return;

        var player = Runner.GetPlayerObject(playerRef);
        if (player != null)
        {
            Runner.Despawn(player);
        }
    }
}
