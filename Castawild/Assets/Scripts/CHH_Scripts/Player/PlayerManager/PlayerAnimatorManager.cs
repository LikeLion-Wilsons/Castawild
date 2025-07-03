using UnityEngine;

public class PlayerAnimatorManager : MonoBehaviour
{
    private Animator anim;
    private CwPlayer player;

    private int horizontal;
    private int vertical;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        player = CwPlayer.instance;

        horizontal = Animator.StringToHash("Horizontal");
        vertical = Animator.StringToHash("Vertical");
    }

    public void UpdateAnimatorValues(float horizontalMovement, float verticalMovement)
    {
        anim.SetFloat(horizontal, horizontalMovement, 0.1f, Time.deltaTime);
        anim.SetFloat(vertical, verticalMovement, 0.1f, Time.deltaTime);
    }

    private void JumpAnimationTrigger() => player.rigid.AddForce(Vector3.up * player.jumpForce, ForceMode.Impulse);

    private void LandAnimationFinishTrigger()
    {
        Debug.Log("InAir 상태 끝");
        if (player.inputManager.moveInput.magnitude > 0f)
            player.ChangeStateMachine(player.moveState);
        else
            player.ChangeStateMachine(player.idleState);
    }
}