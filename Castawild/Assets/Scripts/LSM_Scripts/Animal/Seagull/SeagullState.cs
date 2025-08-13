using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 토끼 FSM 클래스
/// </summary>
public class SeagullState : AnimalState
{  
    public SeagullState(SeagullAnim _seagullAnim, AnimalStateMachine _stateMachine, CwSeagull _seagullObject, string _animBoolName) : base(_seagullAnim, _stateMachine, _seagullObject, _animBoolName)
    {
        this.seagullAnim = _seagullAnim;
        this.seagullObject = _seagullObject; 
    }

    #region Components 
    protected SeagullAnim seagullAnim; // 토끼 애니메이션 클래스
    protected CwSeagull seagullObject; // 토끼 속성을 관리하는 객체 
    
    protected virtual void Awake()
    {  
    }

    #endregion

    public override void Enter()
    { 
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
    protected void ChangeIdleState()
    {
        seagullAnim.stateMachine.ChangeState(seagullAnim.idleState);
    } 
    protected void ChangeEscapeState()
    {
        seagullAnim.stateMachine.ChangeState(seagullAnim.escapeState);
    }  
    protected void ChangeDeathState()
    {
        seagullAnim.stateMachine.ChangeState(seagullAnim.deathState);
    }

}
