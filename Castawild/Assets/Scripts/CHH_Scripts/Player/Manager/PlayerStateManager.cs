using Fusion;
using UnityEngine;

[DisallowMultipleComponent]
public class PlayerStateManager : NetworkBehaviour
{
    private Player player;
    private PlayerMoveManager moveManager;
    private MovementStateManager movementManager;
    private ToolStateManager toolStateManager;

    public override void Spawned()
    {
        movementManager = GetComponent<MovementStateManager>();
        movementManager.Host_ChangeState(MovementState.Idle);

        toolStateManager = GetComponent<ToolStateManager>();
        toolStateManager.Host_ChangeState(ToolState.Idle);
    }

    private void Awake()
    {
        player = GetComponent<Player>();
        moveManager = GetComponent<PlayerMoveManager>();
        movementManager = GetComponent<MovementStateManager>();
        toolStateManager = GetComponent<ToolStateManager>();
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
        if (player.IsCursorLocked)
        {
            movementManager.SetInput(input);
            toolStateManager.SetInput(input);
        }

        if (HasStateAuthority)
        {
            movementManager.currentState?.UpdateState();
            toolStateManager.currentState?.UpdateState();
        }

        if (player.IsCursorLocked)
        {
            movementManager.SetPrevInputButton(input.Buttons);
            toolStateManager.SetPrevInputButton(input.Buttons);
        }
    }
}
