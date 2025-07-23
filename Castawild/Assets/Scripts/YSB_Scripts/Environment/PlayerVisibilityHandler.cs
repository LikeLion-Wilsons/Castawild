using Fusion;
using UnityEngine;

public class PlayerVisibilityHandler : NetworkBehaviour
{
    public override void Spawned()
    {
        if (Object.HasInputAuthority)
        {
            // NetworkObjectVisibilityManager에 자신의 Transform을 등록
            NetworkObjectVisibilityManager.Instance?.SetPlayerTransform(transform);
        }
    }
}
