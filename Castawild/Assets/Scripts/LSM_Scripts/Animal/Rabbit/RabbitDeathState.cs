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
        rabbitAnim.agent.isStopped = true;
        rabbitAnim.agent.updatePosition = false;
        rabbitAnim.ChangeRabbitAnim(RabbitAnim.RabbitPlayAnim.Death, true);
        rabbitObject.AnimalCopse.SetActive(true); // 토끼 시체 활성화  
        animalObject.AnimalBody.enabled = false;
    }

    public override void FixedUpdateNetwork()
    { 
        base.FixedUpdateNetwork();
    }

    public override void Exit()
    {
        rabbitAnim.agent.isStopped = false;
        rabbitAnim.agent.updatePosition = true;
        rabbitAnim.ChangeRabbitAnim(RabbitAnim.RabbitPlayAnim.Death, false);
        rabbitObject.AnimalCopse.SetActive(false); 
    } 
}