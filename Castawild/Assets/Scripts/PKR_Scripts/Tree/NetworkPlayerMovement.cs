using Fusion;
using UnityEngine;

namespace Test
{
    public class NetworkPlayerMovement : NetworkBehaviour
    {
        private NetworkCharacterController _cc;

        [Networked] private NetworkButtons NetworkButtons { get; set; }

        public override void Spawned()
        {
            _cc = GetBehaviour<NetworkCharacterController>();
        }

        public override void FixedUpdateNetwork()
        {
            if (GetInput<NetworkInputData2>(out var input))
            {
                Vector3 dir = Vector3.zero;

                if (input.IsDown(NetworkInputData2.BUTTON_RIGHT)) dir += Vector3.right;
                else if (input.IsDown(NetworkInputData2.BUTTON_LEFT)) dir += Vector3.left;

                if (input.IsDown(NetworkInputData2.BUTTON_FORWARD))dir += Vector3.forward;
                else if (input.IsDown(NetworkInputData2.BUTTON_BACKWARD))dir += Vector3.back;

                _cc.Move(dir.normalized);

                NetworkButtons = input.Buttons;
            }
        }
    }
}