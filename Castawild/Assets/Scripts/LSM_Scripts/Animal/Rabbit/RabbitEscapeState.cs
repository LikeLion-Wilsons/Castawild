using UnityEngine;
 
/// <summary>
/// 도망치는 토끼를 나타내는 클래스
/// </summary>
public class RabbitEscapeState : RabbitState
{  
    public RabbitEscapeState(RabbitAnim _rabbitAnim, AnimalStateMachine _stateMachine, CwRabbit _rabbitObject, string _animBoolName) : base(_rabbitAnim, _stateMachine, _rabbitObject, _animBoolName)     
    { 
    }    

    public override void Enter()
    {  
        Debug.Log("RabbitEscapeState Enter");
        base.Enter(); 
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
            ChangeReturnState(); 
    }

    public override void Exit()
    {
        targetPosition = Vector3.zero;
        base.Exit(); 
    }
     
}
