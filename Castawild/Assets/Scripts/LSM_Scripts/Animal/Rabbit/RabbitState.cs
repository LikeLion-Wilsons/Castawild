using UnityEngine;

/// <summary>
/// 곰 상태 상위클래스
/// </summary>
public class RabbitState : AnimalState
{  
    public RabbitState(RabbitAnim _rabbitAnim, AnimalStateMachine _stateMachine, CwRabbit _rabbitObject, string _animBoolName) : base(_rabbitAnim, _stateMachine, _rabbitObject, _animBoolName)
    {
        this.rabbitAnim = _rabbitAnim;
        this.rabbitObject = _rabbitObject; 
    }

    #region Components 
    protected RabbitAnim rabbitAnim; // 토끼 애니메이션 
    protected CwRabbit rabbitObject; // 토끼 속성 

    protected virtual void Awake()
    {  
    }

    #endregion

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

    protected void ChangeIdleState()
    {
        rabbitAnim.stateMachine.ChangeState(rabbitAnim.idleState);
    }

    protected void ChangeAlertState()
    {
        rabbitAnim.stateMachine.ChangeState(rabbitAnim.alertState);
    }

    protected void ChangeEscapeState()
    {
        rabbitAnim.stateMachine.ChangeState(rabbitAnim.escapeState);
    }

    protected void ChangeReturnState()
    {
        rabbitAnim.stateMachine.ChangeState(rabbitAnim.returnState);
    }

    protected void ChangeDeathState()
    {
        rabbitAnim.stateMachine.ChangeState(rabbitAnim.deathState);
    }   

}
