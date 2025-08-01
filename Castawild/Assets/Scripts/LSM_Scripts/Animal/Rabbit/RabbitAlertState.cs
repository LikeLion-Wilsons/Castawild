using UnityEngine;
 
/// <summary>
/// 토끼의 경계상태를 나타내는 클래스
/// </summary>
public class RabbitAlertState : RabbitState
{  
    public RabbitAlertState(RabbitAnim _animalAnim, AnimalStateMachine _stateMachine, CwRabbit _animalObject, string _animBoolName) : base(_animalAnim, _stateMachine, _animalObject, _animBoolName)     
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
            stateMachine.ChangeState(rabbitAnim.idleState);
            return;
        }
        else if (rabbitObject.IsPlayerDetection() < rabbitObject.MinDetectionRadius)
        {
            stateMachine.ChangeState(rabbitAnim.escapeState);
            return;
        }
        else if (timer <= 0f)
        {
            stateMachine.ChangeState(rabbitAnim.escapeState);
            return;
        }
    }

    public override void Exit()
    { 
        base.Exit(); 
    }
     
}
