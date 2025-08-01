using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 토끼 FSM 클래스
/// </summary>
public class RabbitState : AnimalState
{  
    public RabbitState(RabbitAnim _animalAnim, AnimalStateMachine _stateMachine, CwRabbit _animalObject, string _animBoolName) : base(_animalAnim, _stateMachine, _animalObject, _animBoolName)
    {
        this.rabbitAnim = _animalAnim;
        this.rabbitObject = _animalObject; 
    }

    #region Components 
    protected RabbitAnim rabbitAnim; // 토끼 애니메이션 클래스
    protected CwRabbit rabbitObject; // 토끼 속성을 관리하는 객체 
    protected Vector3 targetPosition = Vector3.zero; // 클래스 필드로 유지

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
     
}
