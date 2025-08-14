// using System.Collections.Generic;
// using Fusion;
// using UnityEngine;

// public class NetworkObjectVisibilityManager : NetworkSingleton<NetworkObjectVisibilityManager>
// {
//     [SerializeField] private float visibleRange = 50f;
//     [SerializeField] private float updateInterval = 0.5f;

//     private float timer = 0f;
//     private Dictionary<PlayerRef, Transform> playerTransforms = new();
//     private readonly List<INetworkVisibilityObject> a_allVisibilityObjects = new();

//     public override void Spawned()
//     {
//         base.Spawned();
//         if (!HasStateAuthority) return;
//     }

//     public void RegisterObject(INetworkVisibilityObject obj)
//     {
//         if (!a_allVisibilityObjects.Contains(obj))
//         {
//             a_allVisibilityObjects.Add(obj);
//             obj.OnDestroyed += HandleObjectDestroyed;
//         }
//     }

//     private void HandleObjectDestroyed(INetworkVisibilityObject obj)
//     {
//         SetObjectVisibility(obj, false);
//     }

//     public void SetPlayerTransform(PlayerRef playerRef, Transform playerTransform)
//     {
//         if (playerTransforms.ContainsKey(playerRef))
//             playerTransforms[playerRef] = playerTransform;
//         else
//             playerTransforms.Add(playerRef, playerTransform);
//     }

//     private void FixedUpdateNetwork()
//     {
//         if (Runner == null || !Runner.IsRunning)
//             return;

//         timer += Time.deltaTime;
//         if (timer < updateInterval) return;
//         timer = 0f;

//         PlayerRef localPlayer = Runner.LocalPlayer;

//         if (!playerTransforms.TryGetValue(localPlayer, out Transform playerTransform) || playerTransform == null)
//             return;

//         Vector3 playerPos = playerTransform.position;
//         float sqrVisibleRange = visibleRange * visibleRange;

//         for (int i = a_allVisibilityObjects.Count - 1; i >= 0; i--)
//         {
//             var obj = a_allVisibilityObjects[i];
//             if (obj == null || obj.NetworkObject == null)
//             {
//                 a_allVisibilityObjects.RemoveAt(i);
//                 continue;
//             }

//             Vector3 objPos = obj.NetworkObject.transform.position;
//             float distSqr = (objPos - playerPos).sqrMagnitude;
//             bool shouldBeVisible = obj.CanBeVisible() && distSqr <= sqrVisibleRange;

//             SetObjectVisibility(obj, shouldBeVisible);
//         }
//     }

//     private void SetObjectVisibility(INetworkVisibilityObject obj, bool isVisible)
//     {
//         if (obj.VisualRoot == null) return;

//         if (obj.VisualRoot.activeSelf != isVisible)
//         {
//             obj.VisualRoot.SetActive(isVisible);
//             if (obj.Collider != null)
//                 obj.Collider.enabled = isVisible;
//         }
//     }

//     public void Despawned()
//     {
//         foreach (var obj in a_allVisibilityObjects)
//         {
//             if (obj != null)
//             {
//                 obj.OnDestroyed -= HandleObjectDestroyed;
//             }
//         }
//         a_allVisibilityObjects.Clear();
//         playerTransforms.Clear();
//     }
// }