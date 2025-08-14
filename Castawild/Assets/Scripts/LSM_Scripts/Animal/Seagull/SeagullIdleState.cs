using UnityEngine;
/// <summary>
/// 평상시 곰을 나타내는 클래스
/// </summary>
public class SeagullIdleState : SeagullState
{  
    public SeagullIdleState(SeagullAnim _seagullAnim, AnimalStateMachine _stateMachine, CwSeagull _seagullObject, string _animBoolName) : base(_seagullAnim, _stateMachine, _seagullObject, _animBoolName)     
    { 
    } 
    protected override void Awake()
    {
        base.Awake();
    }

    public override void Enter()
    {
        Debug.Log("BearIdleState Enter");
        base.Enter(); 
    }

    public override void FixedUpdateNetwork()
    { 
        base.FixedUpdateNetwork(); 
    }
    public override void Exit()
    { 
        base.Exit(); 
    }  
}

