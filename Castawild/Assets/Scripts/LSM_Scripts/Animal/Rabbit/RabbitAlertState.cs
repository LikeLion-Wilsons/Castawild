using UnityEngine;
 
/// <summary>
/// 토끼의 경계상태를 나타내는 클래스
/// </summary>
public class RabbitAlertState : RabbitState
{  
    public RabbitAlertState(RabbitAnim _rabbitAnim, AnimalStateMachine _stateMachine, CwRabbit _rabbitObject, string _animBoolName) : base(_rabbitAnim, _stateMachine, _rabbitObject, _animBoolName)     
    { 
    } 
    private float timer;
    public override void Enter()
    {
        //현재 state 디버그
        Debug.Log("RabbitAlertState Enter");
        base.Enter();
        timer = rabbitObject.AlertTime;
    }
     
    public override void Update()
    { 
        base.Update();
        timer -= Time.deltaTime;
        if (rabbitObject.IsPlayerDetection() == null)
        {
            ChangeIdleState();
            return;
        }
        else if (rabbitObject.IsPlayerDetection() < rabbitObject.MinDetectionRadius)
        {
            ChangeEscapeState();
            return;
        }
        else if (timer <= 0f)
        {
            ChangeEscapeState();
            return;
        }
    }

    public override void Exit()
    { 
        base.Exit(); 
    } 
}
