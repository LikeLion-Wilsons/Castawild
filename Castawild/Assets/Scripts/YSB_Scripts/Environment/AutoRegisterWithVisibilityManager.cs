using Fusion;
using UnityEngine;

/// <summary>
/// 이 컴포넌트를 가진 NetworkObject는 스폰될 때 자동으로 NetworkObjectVisibilityManager에 등록되고,
/// 디스폰될 때 자동으로 해제됩니다.
/// 가시성 관리가 필요한 모든 프리팹(나무, 바위 등)에 추가해주세요.
/// </summary>
public class AutoRegisterWithVisibilityManager : NetworkBehaviour
{
    public override void Spawned()
    {
        base.Spawned();
        NetworkObjectVisibilityManager.Instance?.RegisterObject(Object);
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        base.Despawned(runner, hasState);
        NetworkObjectVisibilityManager.Instance?.UnregisterObject(Object);
    }
}
