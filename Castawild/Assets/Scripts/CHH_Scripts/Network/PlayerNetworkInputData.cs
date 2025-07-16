using Fusion;
using UnityEngine;

public struct PlayerNetworkInputData : INetworkInput
{
    public const int moveInput = 1;
    public const int jumpInput = 2;
    public const int crouchInput = 3;
    public const int aimInput = 4;
    public const int sprintInput = 5;
    public const int toolUseInput = 6;

    public Vector2 moveValue;

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