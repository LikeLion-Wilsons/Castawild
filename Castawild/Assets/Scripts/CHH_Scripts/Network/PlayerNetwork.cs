using Fusion;
using UnityEditor.EditorTools;
using UnityEngine;

public class PlayerNetworkManager : NetworkBehaviour
{
    private CwPlayer player;
    private CharacterController controller;
    private MovementStateManager movementManager;
    private ToolStateManager toolManager;


    void Awake()
    {
        player = GetComponent<CwPlayer>();
        movementManager = GetComponent<MovementStateManager>();
        toolManager = GetComponent<ToolStateManager>();
        controller = GetComponent<CharacterController>();
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasInputAuthority)
            return;

        if (GetInput<PlayerNetworkInputData>(out var input))
        {
            movementManager.SetInput(input);
            toolManager.SetInput(input);

            movementManager.UpdateMoveAnimation();
            movementManager.currentState.UpdateState();
            toolManager.currentState.UpdateState();

            movementManager.SetPrevInputButton(input.Buttons);
            toolManager.SetPrevInputButton(input.Buttons);





            Vector3 moveDir = movementManager.GetMoveDir(input.moveValue);
            Vector3 moveVelocity = moveDir * movementManager.currentMoveSpeed;
            Vector3 gravityVelocity = movementManager.Gravity();

            Vector3 finalVelocity = new Vector3(moveVelocity.x, gravityVelocity.y, moveVelocity.z);

            controller.Move(finalVelocity * Time.fixedDeltaTime);
        }
    }
}
