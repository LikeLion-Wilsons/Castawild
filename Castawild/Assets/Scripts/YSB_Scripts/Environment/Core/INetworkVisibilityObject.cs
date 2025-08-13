using Fusion;
using UnityEngine;
using System;

public interface INetworkVisibilityObject
{
    event Action<INetworkVisibilityObject> OnDestroyed;
    bool CanBeVisible();
    GameObject GameObject { get; }
    GameObject VisualRoot { get; }
    Collider Collider { get; }
}