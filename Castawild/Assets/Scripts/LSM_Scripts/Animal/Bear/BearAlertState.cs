using UnityEngine; 
/// <summary>
/// 곰의 경계상태를 나타내는 클래스
/// </summary>
public class BearAlertState : BearState
{  
    public BearAlertState(BearAnim _bearAnim, AnimalStateMachine _stateMachine, CwBear _bearObject, string _animBoolName) : base(_bearAnim, _stateMachine, _bearObject, _animBoolName)     
    {
    }
    private float timer; 
    public override void Enter()
    { 
        Debug.Log("BearAlertState Enter");  
        base.Enter();
        timer = bearObject.AlertTime;
        bearAnim.anim.SetBool("Alert", true);
    }

    public override void Update()
    { 
        base.Update();
        timer -= Time.deltaTime;
        if (bearObject.IsPlayerDetection() == null)
        {
            ChangeIdleState();
            return;
        }
        else if (bearObject.IsPlayerDetection() < bearObject.MinDetectionRadius)
        { 
            ChangeAttackState();
            return;
        }
        else if (timer <= 3f)
        {
            ChangeAttackState();
            return;
        }
    }
    public override void Exit()
    { 
        base.Exit();
        bearAnim.anim.SetBool("Alert", false);
    }  
}
