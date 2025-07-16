using Fusion;
using UnityEngine;

public class PlayerNetworkManager : NetworkBehaviour
{
    private CwPlayer player;
    private CharacterController controller;
    private MovementStateManager movementManager;
    private PlayerInputManager inputManager;

    void Awake()
    {
        player = GetComponent<CwPlayer>();
        movementManager = GetComponent<MovementStateManager>();
        inputManager = GetComponent<PlayerInputManager>();
        controller = GetComponent<CharacterController>();
    }

    private void Update()
    {
        inputManager.HandleMovementInput();
    }

    public override void FixedUpdateNetwork()
    {
        Vector3 moveDir = movementManager.GetMoveDir(HasInputAuthority);
        Vector3 moveVelocity = moveDir * movementManager.currentMoveSpeed;
        Vector3 gravityVelocity = movementManager.Gravity();

        Vector3 finalVelocity = new Vector3(moveVelocity.x, gravityVelocity.y, moveVelocity.z);

        controller.Move(finalVelocity * Time.fixedDeltaTime);
    }
}
