using UnityEngine;
using Fusion;
using System;

public class NetworkLogManager : NetworkBehaviour
{
    public static NetworkLogManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    public void Log(string message, PlayerRef player)
    {
        if (Runner.LocalPlayer == player)
        {
            Debug.Log($"[Self#{player.PlayerId}]: {message}");
        }
        else
        {
            RPC_Request(message, player);
        }
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    void RPC_Request(string message, PlayerRef player)
    {
        RPC_Broadcast(message, player);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    void RPC_Broadcast(string message, PlayerRef player)
    {
        if (Runner.LocalPlayer == player)
        {
            Debug.Log($"[SelfRPC#{player.PlayerId}]: {message}");
        }
    }
}
