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
        Debug.Log("RabbitReturnState Enter");
        base.Enter();
        Vector2 randCircle = Random.insideUnitCircle * rabbitObject.HomeRadius;
        targetPosition = rabbitObject.HomeCenter + new Vector3(randCircle.x, 0, randCircle.y);

        rabbitAnim.agent.SetDestination(targetPosition);
    }

    public override void Update()
    { 
        base.Update();
        if (!rabbitAnim.agent.pathPending && rabbitAnim.agent.remainingDistance <= rabbitAnim.agent.stoppingDistance + 0.1f)
        {
            ChangeIdleState();        
        }
    }

    public override void Exit()
    { 
        base.Exit();
    }
     
}
