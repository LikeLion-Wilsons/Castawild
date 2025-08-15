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
        rabbitAnim.ChangeRabbitAnim(RabbitAnim.RabbitPlayAnim.Death, true);
        rabbitObject.AnimalCopse.SetActive(true); // 토끼 시체 활성화  
    }

    public override void FixedUpdateNetwork()
    { 
        base.FixedUpdateNetwork();
    }

    public override void Exit()
    {
        rabbitAnim.ChangeRabbitAnim(RabbitAnim.RabbitPlayAnim.Death, false);
    } 
}
