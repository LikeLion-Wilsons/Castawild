using UnityEngine; 

/// <summary>
/// 곰 상태 상위클래스
/// </summary>
public class BearState : AnimalState
{  
    public BearState(BearAnim _bearAnim, AnimalStateMachine _stateMachine, CwBear _bearObject, string _animBoolName) : base(_bearAnim, _stateMachine, _bearObject, _animBoolName)
    {
        this.bearAnim = _bearAnim;
        this.bearObject = _bearObject; 
    }

    #region Components 
    protected BearAnim bearAnim; // 곰 애니메이션 
    protected CwBear bearObject; // 곰 속성
    protected Vector3 idlePosition; // 현재 idle 위치 

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
        bearAnim.stateMachine.ChangeState(bearAnim.idleState);
    }

    protected void ChangeAlertState()
    {
        bearAnim.stateMachine.ChangeState(bearAnim.alertState);
    }

    protected void ChangeAttackState()
    {
        bearAnim.stateMachine.ChangeState(bearAnim.attackState);
    }

    protected void ChangeReturnState()
    {
        bearAnim.stateMachine.ChangeState(bearAnim.returnState);
    }

    protected void ChangeDeathState()
    {
        bearAnim.stateMachine.ChangeState(bearAnim.deathState);
    }

}
