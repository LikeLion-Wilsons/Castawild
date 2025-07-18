using UnityEngine;
 
/// <summary>
/// 토끼 FSM 클래스
/// </summary>
public class RabbitState : AnimalState
{  
    public RabbitState(AnimalAnim _animalAnim, AnimalStateMachine _stateMachine, CwAnimal _animalObject, string _animBoolName) : base(_animalAnim, _stateMachine, _animalObject, _animBoolName)     
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
