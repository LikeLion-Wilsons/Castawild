using Fusion;
using UnityEngine;
using System;

public interface INetworkVisibilityObject
{
    event Action<INetworkVisibilityObject> OnDestroyed;
    bool CanBeVisible();
    NetworkObject NetworkObject { get; }
    GameObject VisualRoot { get; }
    Collider Collider { get; }
}