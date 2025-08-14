using Fusion;
using UnityEngine;

[DisallowMultipleComponent]
public class PlayerStateManager : NetworkBehaviour
{
    private Player player;
    private PlayerMoveManager moveManager;
    private MovementStateManager movementManager;
    private ToolStateManager toolStateManager;
    private PlayerFlagManager flagManager;

    private bool isUIClosed = false;

    public override void Spawned()
    {
        movementManager = GetComponent<MovementStateManager>();
        movementManager.Host_ChangeState(MovementState.Idle);

        toolStateManager = GetComponent<ToolStateManager>();
        toolStateManager.Host_ChangeState(ToolState.Idle);

        if (HasInputAuthority)
        {
            OptionUI.openUI -= CloseUI;
            OptionUI.openUI += CloseUI;
        }
    }

    private void CloseUI(bool isOpen)
    {
        if (!isOpen)
            isUIClosed = true;
    }

    private void Awake()
    {
        player = GetComponent<Player>();
        moveManager = GetComponent<PlayerMoveManager>();
        movementManager = GetComponent<MovementStateManager>();
        toolStateManager = GetComponent<ToolStateManager>();
        flagManager = GetComponent<PlayerFlagManager>();
    }

    public override void FixedUpdateNetwork()
    {
        if (!GetInput<PlayerNetworkInputData>(out var input))
            return;

        All_HandleState(input);

        if (!moveManager.CanMove)
            return;

        movementManager.MoveValue = input.moveValue;
    }

    private void All_HandleState(PlayerNetworkInputData input)
    {
        if (player.IsCursorLocked || isUIClosed)
        {
            isUIClosed = false;
            input.moveValue = Vector2.zero;
            movementManager.SetInput(input);
            toolStateManager.SetInput(input);
        }

        if (HasStateAuthority)
        {
            movementManager.currentState?.UpdateState();
            if (!flagManager.IsDead)
                toolStateManager.currentState?.UpdateState();
        }

        if (player.IsCursorLocked)
        {
            movementManager.SetPrevInputButton(input.Buttons);
            toolStateManager.SetPrevInputButton(input.Buttons);
        }
    }
}
