using Unity.VisualScripting;
using UnityEngine;
 
/// <summary>
/// 죽은 토끼를 나타내는 클래스
/// </summary>
public class RabbitDeathState : RabbitState
{  
    public RabbitDeathState(RabbitAnim _rabbitAnim, AnimalStateMachine _stateMachine, CwRabbit _rabbitObject, string _animBoolName) : base(_rabbitAnim, _stateMachine, _rabbitObject, _animBoolName)     
    { 
    }  
    public override void Enter()
    {
        rabbitObject.IsDead = true; // 죽은 상태로 변경    
        rabbitAnim.agent.isStopped = true;
        rabbitAnim.agent.updatePosition = false; 
        rabbitObject.AnimalCopse.SetActive(true); // 토끼 시체 활성화  
        rabbitObject.AnimalBody.enabled = false; 
    }

    public override void FixedUpdateNetwork()
    { 
        base.FixedUpdateNetwork();
        if (rabbitObject.IsDead)
            return;
        else 
        {
            rabbitAnim.ChangeRabbitState(RabbitAnim.RabbitState.Idle);
            rabbitObject.gameObject.SetActive(false);
        }            
    }

    public override void Exit()
    {
        rabbitObject.IsDead = false; // 죽은 상태 해제
        rabbitAnim.agent.isStopped = false;
        rabbitAnim.agent.updatePosition = true;
        rabbitObject.AnimalCopse.SetActive(false);
        rabbitAnim.ChangeRabbitAnim(RabbitAnim.RabbitPlayAnim.Death, false);
        rabbitObject.AnimalCopse.SetActive(false); 
        rabbitObject.AnimalBody.enabled = true;
        rabbitObject.IsDead = false; // 시체 상태 해제

    } 
}