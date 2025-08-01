using TMPro;
using UnityEngine;
 
/// <summary>
/// 경계가 풀린 토끼를 나타내는 클래스
/// </summary>
public class RabbitReturnState : RabbitState
{  
    public RabbitReturnState(RabbitAnim _animalAnim, AnimalStateMachine _stateMachine, CwRabbit _animalObject, string _animBoolName) : base(_animalAnim, _stateMachine, _animalObject, _animBoolName)     
    { 
    } 

    public override void Enter()
    {
        //현재 state 디버그
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
            rabbitAnim.stateMachine.ChangeState(rabbitAnim.idleState); // 귀가 완료 후 대기
        }
    }

    public override void Exit()
    { 
        base.Exit();
    }
     
}
