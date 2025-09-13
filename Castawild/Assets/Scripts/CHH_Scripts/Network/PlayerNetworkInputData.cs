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
    public const int interactInput = 7;
    public const int removeInput = 8;

    public Vector3 lookValue;
    public Vector2 moveValue; // 애니메이션, 입력 들어왔는지 판가름용
    public Vector3 moveDir; // 캐릭터 움직이는 방향
    public Vector3 camForward;
    public ViewType currentView;

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