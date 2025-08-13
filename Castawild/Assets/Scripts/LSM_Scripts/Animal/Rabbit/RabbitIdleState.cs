using UnityEngine; 

/// <summary>
/// 돌아다니는 평상시 토끼를 나타내는 클래스
/// </summary>
public class RabbitIdleState : RabbitState
{  
    public RabbitIdleState(RabbitAnim _rabbitAnim, AnimalStateMachine _stateMachine, CwRabbit _rabbitObject, string _animBoolName) : base(_rabbitAnim, _stateMachine, _rabbitObject, _animBoolName)     
    { 
    } 
    protected override void Awake()
    {
        base.Awake();
    }

    public override void Enter()
    { 
        rabbitAnim.ChangeRabbitAnim(RabbitAnim.RabbitPlayAnim.IdleMove, true); 
    }

    public override void FixedUpdateNetwork()
    {
        base.FixedUpdateNetwork();
        if (rabbitObject.IsPlayerDetection() != null) 
        {
            rabbitAnim.ChangeRabbitState(RabbitAnim.RabbitState.Alert);
            return;
        }
        
        if (rabbitAnim.IdleMoveing)
        {
            rabbitAnim.ChangeRabbitAnim(RabbitAnim.RabbitPlayAnim.IdleMove, true);

            if (targetPosition == Vector3.zero)
            {  
                rabbitAnim.agent.speed = animalObject.MoveSpeed;
                targetPosition = rabbitObject.RandomNavSphere(rabbitAnim.transform.position, 5f, rabbitObject.IdleRadius, -1);
                rabbitAnim.agent.SetDestination(targetPosition);
            }

            // 목적지에 도달했으면 상태 종료
            if (!rabbitAnim.agent.pathPending && rabbitAnim.agent.remainingDistance <= rabbitAnim.agent.stoppingDistance)
            {
                if (!rabbitAnim.agent.hasPath || rabbitAnim.agent.velocity.sqrMagnitude == 0f)
                {
                    rabbitAnim.IdleMoveing = false;
                    targetPosition = Vector3.zero;
                }
            }
        }
        else
        {
            rabbitAnim.ChangeRabbitAnim(RabbitAnim.RabbitPlayAnim.IdleMove, false);
            targetPosition = Vector3.zero;
        }
    }
    public override void Exit()
    {  
        rabbitAnim.ChangeRabbitAnim(RabbitAnim.RabbitPlayAnim.Idle, false);
        rabbitAnim.ChangeRabbitAnim(RabbitAnim.RabbitPlayAnim.IdleMove, false);
    } 

}
