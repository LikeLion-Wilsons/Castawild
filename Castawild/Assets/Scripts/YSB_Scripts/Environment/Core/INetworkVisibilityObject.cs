using Fusion;
using UnityEngine;

public interface INetworkVisibilityObject
{
    bool CanBeVisible();
    NetworkObject GetNetworkObject();
    GameObject VisualRoot { get; }
}