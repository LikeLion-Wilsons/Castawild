using TMPro;
using UnityEngine;
 
/// <summary>
/// 도망치는 토끼를 나타내는 클래스
/// </summary>
public class RabbitEscapeState : RabbitState
{  
    public RabbitEscapeState(RabbitAnim _animalAnim, AnimalStateMachine _stateMachine, CwRabbit _animalObject, string _animBoolName) : base(_animalAnim, _stateMachine, _animalObject, _animBoolName)     
    { 
    }

    private Vector3 targetPosition = Vector3.zero;    

    public override void Enter()
    { 
        //현재 state 디버그
        Debug.Log("RabbitEscapeState Enter");
        base.Enter();
        // 타겟이 아직 없으면 한 번만 설정
        if (targetPosition == Vector3.zero)
        {
            rabbitAnim.agent.speed = animalObject.EscapeSpeed;
            targetPosition = rabbitObject.RandomNavSphere(rabbitAnim.transform.position, rabbitObject.EscapeRadius, rabbitObject.EscapeRadius, -1);
            rabbitAnim.agent.SetDestination(targetPosition);
        }

    }

    public override void Update()
    { 
        base.Update(); 
        // 목적지에 도달했으면 상태 종료
        if (!rabbitAnim.agent.pathPending && rabbitAnim.agent.remainingDistance <= rabbitAnim.agent.stoppingDistance)
        {
            stateMachine.ChangeState(rabbitAnim.returnState);
        }
    }

    public override void Exit()
    {
        targetPosition = Vector3.zero;
        base.Exit(); 
    }
     
}
