using Fusion;
using UnityEngine;

namespace Test
{
    public struct NetworkInputData : INetworkInput
    {
        public const int BUTTON_FORWARD = 1;
        public const int BUTTON_BACKWARD = 2;
        public const int BUTTON_LEFT = 3;
        public const int BUTTON_RIGHT = 4;
        public const int BUTTON_INTERACT = 5;
        public const int BUTTON_INVENTORY = 6;
        public const int BUTTON_FIRE = 7;//좌클
        public const int BUTTON_FIRE2 = 8;//우클
        public const int BUTTON_JUMP = 9;

        public Quaternion camerPivotRotation;
        public Vector2 mouseDelta;
        public NetworkButtons Buttons;
        public bool IsUp(int button)
        {
            return Buttons.IsSet(button) == false;
        }

        public bool IsDown(int button)
        {
            return Buttons.IsSet(button);
        }

        public bool WasPressed(NetworkButtons prev, int button)
        {
            return Buttons.WasPressed(prev, button);
        }
    }
}