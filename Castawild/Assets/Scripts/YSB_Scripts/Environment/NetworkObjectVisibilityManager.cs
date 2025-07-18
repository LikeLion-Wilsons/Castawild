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

    public void SetPlayerTransform(Transform player)
    {
        playerTransform = player;
    }

    public void RegisterObject(NetworkObject obj)
    {
        if (!managedObjects.Contains(obj))
            managedObjects.Add(obj);
    }

    public void UnregisterObject(NetworkObject obj)
    {
        if (managedObjects.Contains(obj))
            managedObjects.Remove(obj);
    }

    public override void FixedUpdateNetwork()
    {
        //if (!Object.HasInputAuthority) return;

        if (playerTransform == null)
        {
            TryAutoSetPlayerTransform();
            if (playerTransform == null) return;
        }

        timer += Runner.DeltaTime;
        if (timer < updateInterval) return;

        timer = 0f;

        foreach (var obj in managedObjects)
        {
            if (obj == null) continue;

            float dist = Vector3.Distance(playerTransform.position, obj.transform.position);
            bool shouldBeVisible = dist <= visibleRange;

            if (obj.gameObject.activeSelf != shouldBeVisible)
            {
                obj.gameObject.SetActive(shouldBeVisible);
            }
        }
    }

    private void TryAutoSetPlayerTransform()
    {
        Debug.Log("[VisibilityManager] TryAutoSetPlayerTransform 호출됨");
        foreach (var obj in FindObjectsOfType<NetworkObject>())
        {
            Debug.Log($"[VisibilityManager] NetworkObject 발견: {obj.name}, HasInputAuthority={obj.HasInputAuthority}");
            if (obj.HasInputAuthority)
            {
                playerTransform = obj.transform;
                Debug.Log($"[VisibilityManager] 자동으로 플레이어 연결됨: {playerTransform.name}");
                break;
            }
        }
    }

}
