using UnityEngine;
using Fusion;

namespace Test
{
	public struct NetworkInputData : INetworkInput
	{
        public Vector2 LookRotation;
        public Vector2 MoveDirection;
		public NetworkButtons Buttons;
		public const int JUMP_BUTTON = 0;
	}
}
