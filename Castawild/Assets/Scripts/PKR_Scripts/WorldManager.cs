using UnityEngine;
using Fusion;

namespace Test
{
    //플레이어 입장,퇴장
	public sealed class WorldManager : NetworkBehaviour, IPlayerJoined, IPlayerLeft
	{
		public NetworkObject PlayerPrefab;

        public override void FixedUpdateNetwork()
        {
            //월드내 오브젝트의 상태추적 및 변경작업.
        }

        public void PlayerJoined(PlayerRef playerRef)
		{
			if (HasStateAuthority == false) return;

            
			var player = Runner.Spawn(PlayerPrefab, Vector3.zero, Quaternion.identity, playerRef);
			Runner.SetPlayerObject(playerRef, player);
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
