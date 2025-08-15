using Fusion;
using UnityEngine; 

/// <summary>
/// 토끼 상태 상위클래스
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
        rabbitAnim.ChangeRabbitAnim(rabbitAnim.currentAnim, true); // 애니메이션 시작
    }

    public override void FixedUpdateNetwork()
    { 
        base.FixedUpdateNetwork(); 
    }

    public override void Exit()
    {
        rabbitAnim.ChangeRabbitAnim(rabbitAnim.currentAnim, false); // 애니메이션 시작
    }
}
