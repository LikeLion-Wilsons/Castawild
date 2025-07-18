using UnityEngine;
 
/// <summary>
/// 평상시 토끼를 나타내는 클래스
/// </summary>
public class RabbitIdleState : RabbitState
{  
    public RabbitIdleState(AnimalAnim _animalAnim, AnimalStateMachine _stateMachine, CwAnimal _animalObject, string _animBoolName) : base(_animalAnim, _stateMachine, _animalObject, _animBoolName)     
    { 
    } 

    public override void Enter()
    { 
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
