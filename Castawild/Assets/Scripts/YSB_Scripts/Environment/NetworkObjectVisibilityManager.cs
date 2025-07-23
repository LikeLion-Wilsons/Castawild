using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class NetworkObjectVisibilityManager : NetworkSingleton<NetworkObjectVisibilityManager>
{
    [SerializeField, Tooltip("플레이어 가시 범위")] private float visibleRange = 50f;
    [SerializeField, Tooltip("갱신 주기(초)")] private float updateInterval = 0.5f;

    private Transform playerTransform;
    private List<NetworkObject> managedObjects = new List<NetworkObject>();
    private float timer = 0f;

    public override void Spawned()
    {
        base.Spawned();
    }

    public void SetPlayerTransform(Transform player)
    {
        playerTransform = player;
        Debug.Log($"[VisibilityManager] Player transform set: {player.name}. Forcing visibility update for {managedObjects.Count} objects.");

        // 플레이어가 설정되는 즉시, 이미 등록된 모든 오브젝트의 가시성을 강제로 업데이트합니다.
        // 이것이 후입자 문제를 해결하는 핵심입니다.
        foreach (var obj in managedObjects)
        {
            UpdateObjectVisibility(obj);
        }
    }

    public void RegisterObject(NetworkObject obj)
    {
        if (managedObjects.Contains(obj)) return;
        managedObjects.Add(obj);

        // 오브젝트가 등록될 때 플레이어가 이미 준비되었다면, 즉시 가시성을 업데이트합니다.
        if (playerTransform != null)
        {
            UpdateObjectVisibility(obj);
        }
    }

    public void UnregisterObject(NetworkObject obj)
    {
        if (managedObjects.Contains(obj))
            managedObjects.Remove(obj);
    }

    public override void FixedUpdateNetwork()
    {
        // 플레이어가 아직 설정되지 않았다면 아무것도 하지 않습니다.
        if (playerTransform == null) return;

        timer += Runner.DeltaTime;
        if (timer < updateInterval) return;
        timer = 0f;

        // 주기적으로 모든 오브젝트의 가시성을 업데이트합니다 (플레이어 이동 대응).
        for (int i = managedObjects.Count - 1; i >= 0; i--)
        {
            var obj = managedObjects[i];
            if (obj == null)
            {
                managedObjects.RemoveAt(i);
                continue;
            }
            UpdateObjectVisibility(obj);
        }
    }

    /// <summary>
    /// 단일 오브젝트의 가시성을 플레이어와의 거리에 따라 업데이트하는 핵심 함수입니다.
    /// </summary>
    private void UpdateObjectVisibility(NetworkObject obj)
    {
        if (obj == null || playerTransform == null) return;

        float dist = Vector3.Distance(playerTransform.position, obj.transform.position);
        bool shouldBeVisible = dist <= visibleRange;

        if (obj.gameObject.activeSelf != shouldBeVisible)
        {
            obj.gameObject.SetActive(shouldBeVisible);
        }
    }

    // 이 함수는 더 이상 필요하지 않습니다. SetPlayerTransform이 명시적으로 호출되어야 합니다.
    // private void TryToFindAndSetPlayerTransform() { ... }
}
