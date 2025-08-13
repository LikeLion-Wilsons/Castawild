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
        rabbitAnim.ChangeRabbitAnim(RabbitAnim.RabbitPlayAnim.Alert, true);
        timer = rabbitObject.AlertTime;
    }
     
    public override void FixedUpdateNetwork()
    { 
        base.FixedUpdateNetwork();
        timer -= Time.deltaTime;
        if (rabbitObject.IsPlayerDetection() == null)
        {

            rabbitAnim.ChangeRabbitState(RabbitAnim.RabbitState.Idle);
            return;
        }
        else if (rabbitObject.IsPlayerDetection() < rabbitObject.MinDetectionRadius)
        {
            rabbitAnim.ChangeRabbitState(RabbitAnim.RabbitState.Escape);
            return;
        }
        else if (timer <= 0f)
        {
            rabbitAnim.ChangeRabbitState(RabbitAnim.RabbitState.Escape);
            return;
        }
    }

    public override void Exit()
    {
        rabbitAnim.ChangeRabbitAnim(RabbitAnim.RabbitPlayAnim.Alert, false);
    } 
}
