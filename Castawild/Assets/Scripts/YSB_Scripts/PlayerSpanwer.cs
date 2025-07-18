using Fusion;
using UnityEngine;

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
                o.GetComponent<Test.Player>().Init();

                if (o.HasInputAuthority)
                {
                    NetworkObjectVisibilityManager.Instance?.SetPlayerTransform(o.transform);
                }
            });

            Runner.SetPlayerObject(playerRef, playerObj);
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
