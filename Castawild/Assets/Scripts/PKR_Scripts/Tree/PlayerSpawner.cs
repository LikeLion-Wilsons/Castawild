using Fusion;
using UnityEngine;

namespace Test
{

    public class PlayerSpawner : NetworkBehaviour, IPlayerJoined, IPlayerLeft
    {
        [SerializeField] private NetworkObject PlayerPrefab;
        [SerializeField] private Transform spawnPosition;

        public void PlayerJoined(PlayerRef playerRef)
        {
            if (HasStateAuthority == false) return;

            var x = UnityEngine.Random.Range(-3f, 0f);
            var z = UnityEngine.Random.Range(-3f, 3f);

            var playerObj = Runner.Spawn(PlayerPrefab, spawnPosition.position, Quaternion.identity, playerRef, (runner, o) =>
            {
                //o.GetComponent<Player>().Init();
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