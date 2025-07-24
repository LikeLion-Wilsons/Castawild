using Fusion;
using System.Collections.Generic;
using UnityEngine;

public interface INetworkVisibilityObject
{    
    bool CanBeVisible();
    NetworkObject GetNetworkObject();
}
