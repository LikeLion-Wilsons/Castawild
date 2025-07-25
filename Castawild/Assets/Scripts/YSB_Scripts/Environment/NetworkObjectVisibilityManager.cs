using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class NetworkObjectVisibilityManager : NetworkSingleton<NetworkObjectVisibilityManager>
{
    private Dictionary<PlayerRef, Transform> playerTransforms = new();
    private List<INetworkVisibilityObject> visibilityObjects = new();

    [SerializeField] private float visibleRange = 50f;
    [SerializeField] private float updateInterval = 0.5f;

    private float timer = 0f;

    public override void Spawned()
    {
        base.Spawned();
        Debug.Log($"[Spawned] VisibilityManager instance: {NetworkObjectVisibilityManager.Instance}");
    }

    public void SetPlayerTransform(PlayerRef playerRef, Transform playerTransform)
    {
        if (playerTransforms.ContainsKey(playerRef))
            playerTransforms[playerRef] = playerTransform;
        else
            playerTransforms.Add(playerRef, playerTransform);
    }

    public void RegisterObject(INetworkVisibilityObject obj)
    {
        if (!visibilityObjects.Contains(obj))
        {
            visibilityObjects.Add(obj);
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (!Runner.IsRunning)
            return;

        timer += Runner.DeltaTime;
        if (timer < updateInterval) return;
        timer = 0f;

        PlayerRef localPlayer = Runner.LocalPlayer;

        if (!playerTransforms.TryGetValue(localPlayer, out Transform playerTransform) || playerTransform == null)
            return;

        for (int i = visibilityObjects.Count - 1; i >= 0; i--)
        {
            var obj = visibilityObjects[i];
            if (obj == null)
            {
                visibilityObjects.RemoveAt(i);
                continue;
            }

            var netObj = obj.GetNetworkObject();
            var visualRoot = obj.VisualRoot;

            if (netObj == null || visualRoot == null)
            {
                visibilityObjects.RemoveAt(i);
                continue;
            }

            bool canShow = obj.CanBeVisible() &&
                Vector3.Distance(playerTransform.position, netObj.transform.position) <= visibleRange;

            if (visualRoot.gameObject.activeSelf != canShow)
                visualRoot.gameObject.SetActive(canShow);
        }
    }
}
