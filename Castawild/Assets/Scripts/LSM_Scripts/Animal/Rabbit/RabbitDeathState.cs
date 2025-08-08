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
