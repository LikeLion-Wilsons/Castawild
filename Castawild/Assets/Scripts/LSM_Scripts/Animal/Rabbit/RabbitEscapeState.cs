using UnityEngine;
 
/// <summary>
/// 도망치는 토끼를 나타내는 클래스
/// </summary>
public class RabbitEscpeState : RabbitState
{  
    public RabbitEscpeState(AnimalAnim _animalAnim, AnimalStateMachine _stateMachine, CwAnimal _animalObject, string _animBoolName) : base(_animalAnim, _stateMachine, _animalObject, _animBoolName)     
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
