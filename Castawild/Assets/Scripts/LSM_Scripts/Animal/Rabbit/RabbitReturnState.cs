using UnityEngine;
 
/// <summary>
/// 경계가 풀린 토끼를 나타내는 클래스
/// </summary>
public class RabbitReturnState : RabbitState
{  
    public RabbitReturnState(RabbitAnim _rabbitAnim, AnimalStateMachine _stateMachine, CwRabbit _rabbitObject, string _animBoolName) : base(_rabbitAnim, _stateMachine, _rabbitObject, _animBoolName)     
    { 
    } 

    public override void Enter()
    {
        rabbitAnim.ChangeRabbitAnim(RabbitAnim.RabbitPlayAnim.Return, true);
        Vector2 randCircle = Random.insideUnitCircle * rabbitObject.HomeRadius;
        targetPosition = rabbitObject.HomeCenter + new Vector3(randCircle.x, 0, randCircle.y);
        rabbitAnim.agent.SetDestination(targetPosition);
    }

    public override void FixedUpdateNetwork()
    { 
        base.FixedUpdateNetwork();
        if (!rabbitAnim.agent.pathPending && rabbitAnim.agent.remainingDistance <= rabbitAnim.agent.stoppingDistance + 0.1f)
            rabbitAnim.ChangeRabbitState(RabbitAnim.RabbitState.Idle);
    }

    public override void Exit()
    {
        rabbitAnim.ChangeRabbitAnim(RabbitAnim.RabbitPlayAnim.Return, false);
    }     
}