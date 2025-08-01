using Polyperfect.Common;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 평상시 토끼를 나타내는 클래스
/// </summary>
public class RabbitIdleState : RabbitState
{  
    public RabbitIdleState(RabbitAnim _rabbitAnim, AnimalStateMachine _stateMachine, CwRabbit _animalObject, string _animBoolName) : base(_rabbitAnim, _stateMachine, _animalObject, _animBoolName)     
    { 
    } 
    protected override void Awake()
    {
        base.Awake();
    }

    public override void Enter()
    {
        //현재 state 디버그
        Debug.Log("RabbitIdleState Enter");
        base.Enter();
        rabbitAnim.anim.SetBool("IdleMove", true); 
    }

    public override void Update()
    {
        base.Update();
        if (rabbitObject.IsPlayerDetection() != null) 
        {
            stateMachine.ChangeState(rabbitAnim.alertState);
            return;
        }
        
        if (rabbitAnim.IdleMoveing)
        {
            rabbitAnim.anim.SetBool("IdleMove", true);

            // 타겟이 아직 없으면 한 번만 설정
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
            rabbitAnim.anim.SetBool("IdleMove", false);
            targetPosition = Vector3.zero;
        }
    }
    public override void Exit()
    { 
        base.Exit();
        rabbitAnim.anim.SetBool("Idle", false);
        rabbitAnim.anim.SetBool("IdleMove", false);
    } 

}
