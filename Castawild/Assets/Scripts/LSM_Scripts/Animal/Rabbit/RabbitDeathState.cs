using UnityEngine;
 
/// <summary>
/// 죽은 토끼를 나타내는 클래스
/// </summary>
public class RabbitDeathState : RabbitState
{  
    public RabbitDeathState(RabbitAnim _animalAnim, AnimalStateMachine _stateMachine, CwRabbit _animalObject, string _animBoolName) : base(_animalAnim, _stateMachine, _animalObject, _animBoolName)     
    { 
    } 

    public override void Enter()
    {
        //현재 state 디버그
        Debug.Log("RabbitDeathState Enter");
        base.Enter();
    }

    public override void Update()
    { 
        base.Update();
    }

    public override void Exit()
    { 
        base.Exit();
    }
     
}
