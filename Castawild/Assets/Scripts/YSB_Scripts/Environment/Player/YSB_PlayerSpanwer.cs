using Fusion;
using UnityEngine;
using System.Collections;

namespace YSB_Scripts
{
    public class PlayerSpawner : NetworkBehaviour, IPlayerJoined, IPlayerLeft
    {
        [SerializeField] private NetworkObject PlayerPrefab;

        public void PlayerJoined(PlayerRef playerRef)
        {
            if (HasStateAuthority == false) return;

            var x = UnityEngine.Random.Range(-3f, 0f);
            var z = UnityEngine.Random.Range(-3f, 3f);
            var spawnPosition = transform.position + new Vector3(x, 3f, z);

            var playerObj = Runner.Spawn(PlayerPrefab, spawnPosition, Quaternion.identity, playerRef, (runner, o) =>
            {
                //o.GetComponent<YSB_Player>().Init();

            });
            Runner.SetPlayerObject(playerRef, playerObj);

            if (NetworkObjectVisibilityManager.Instance != null)
            {
                NetworkObjectVisibilityManager.Instance.SetPlayerTransform(playerRef, transform);
            }
            else
            {
                Debug.LogWarning("[Spawned] VisibilityManager instance is null! Delaying registration...");

                StartCoroutine(RegisterPlayerTransformDelayed());
            }
        }

        private IEnumerator RegisterPlayerTransformDelayed()
        {
            while (NetworkObjectVisibilityManager.Instance == null) yield return null;

            NetworkObjectVisibilityManager.Instance.SetPlayerTransform(Object.InputAuthority, transform);
        }

        public void PlayerLeft(PlayerRef playerRef)
        {
            if (HasStateAuthority == false) return;

            var player = Runner.GetPlayerObject(playerRef);
            if (player != null)
            {
                Runner.Despawn(player);
            }
        }

    }
}