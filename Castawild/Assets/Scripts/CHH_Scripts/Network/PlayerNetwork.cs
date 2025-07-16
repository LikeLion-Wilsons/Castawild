using Fusion;
using UnityEngine;

public class PlayerNetworkManager : NetworkBehaviour
{
    private CwPlayer player;
    private CharacterController controller;
    private MovementStateManager movementManager;
    private ToolStateManager toolManager;
    private PlayerCameraManager cameraManager;

    void Awake()
    {
        player = GetComponent<CwPlayer>();
        controller = GetComponent<CharacterController>();
        movementManager = GetComponent<MovementStateManager>();
        toolManager = GetComponent<ToolStateManager>();
        cameraManager = GetComponentInChildren<PlayerCameraManager>();
    }

    public override void Spawned()
    {
        if (!HasInputAuthority)
        {
            cameraManager.firstPersonCam.gameObject.SetActive(false);
            cameraManager.thirdPersonCam.gameObject.SetActive(false);
        }
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

    public bool GetHasInputAuthority() => HasInputAuthority;
}
