using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using static UnityEngine.UI.ScrollRect;

public class PlayerMovement : MonoBehaviour
{
    private Player player;
    private Rigidbody rigid;
    private NavMeshAgent agent;
    private Camera cam;

    [SerializeField] private InputActionAsset inputActions;
    [SerializeField] private float walkSpeed = 2f;
    [SerializeField] private float runSpeed = 4f;
    [SerializeField] private float crouchSpeed = 1f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float airMoveSpeed = 3f;

    private InputAction moveAction;
    private InputAction jumpAction;

    private Vector2 moveInput;
    private bool isJumping;

    private void OnEnable()
    {
        inputActions.FindActionMap("Player").Enable();
    }

    private void OnDisable()
    {
        inputActions.FindActionMap("Player").Disable();
    }

    private void Awake()
    {
        InitializeComponents();
        InitializeInputActions();
    }

    private void InitializeInputActions()
    {
        moveAction = InputSystem.actions.FindAction("Move");
        jumpAction = InputSystem.actions.FindAction("Jump");
    }

    private void InitializeComponents()
    {
        player = GetComponent<Player>();
        rigid = GetComponent<Rigidbody>();
        agent = GetComponent<NavMeshAgent>();
        cam = Camera.main;
    }

    private void Update()
    {
        moveInput = moveAction.ReadValue<Vector2>();

        if (!isJumping)
        {
            Move(moveInput);

            if (jumpAction.WasPressedThisFrame())
                StartJump();
        }
    }

    private void StartJump()
    {
        isJumping = true;

        agent.enabled = false;
        rigid.isKinematic = false;
        rigid.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    private void FixedUpdate()
    {
        if (isJumping)
        {
            Vector3 moveDir = GetCameraRelativeDirection(moveInput);
            Vector3 velocity = rigid.linearVelocity;

            velocity.x = moveDir.x * airMoveSpeed;
            velocity.z = moveDir.z * airMoveSpeed;

            rigid.linearVelocity = velocity;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (isJumping && collision.gameObject.CompareTag("Ground"))
        {
            isJumping = false;

            rigid.isKinematic = true;
            agent.enabled = true;

            agent.Warp(transform.position);
        }
    }

    private void Move(Vector2 input)
    {
        Vector3 direction = GetCameraRelativeDirection(input);
        Vector3 target = transform.position + direction * 0.5f;

        agent.SetDestination(target);
    }

    private Vector3 GetCameraRelativeDirection(Vector2 input)
    {
        Vector3 camForward = cam.transform.forward;
        Vector3 camRight = cam.transform.right;

        camForward.y = 0;
        camRight.y = 0;

        return (camRight * input.x + camForward * input.y).normalized;
    }
}
