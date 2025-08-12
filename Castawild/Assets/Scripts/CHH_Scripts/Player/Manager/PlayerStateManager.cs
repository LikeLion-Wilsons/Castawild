using Fusion;
using UnityEngine;

[DisallowMultipleComponent]
public class PlayerStateManager : NetworkBehaviour
{
    private Player player;
    private MovementStateManager movementManager;
    private ToolStateManager toolStateManager;

    private void Awake()
    {
        player = GetComponent<Player>();
        movementManager = GetComponent<MovementStateManager>();
        toolStateManager = GetComponent<ToolStateManager>();
    }

    public override void FixedUpdateNetwork()
    {
        if (!GetInput<PlayerNetworkInputData>(out var input))
            return;

        All_HandleState(input);

        if (!player.CanMove)
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

        if (movementManager.movementStateDict.TryGetValue(movementManager.CurrentMoveState, out var movementState))
            movementState.UpdateState();
        if (toolStateManager.toolStateDict.TryGetValue(toolStateManager.CurrentToolState, out var toolState))
            toolState.UpdateState();

        if (player.IsCursorLocked)
        {
            movementManager.SetPrevInputButton(input.Buttons);
            toolStateManager.SetPrevInputButton(input.Buttons);
        }
    }
}
