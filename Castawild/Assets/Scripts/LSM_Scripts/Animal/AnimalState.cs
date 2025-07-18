using UnityEngine;
 
/// <summary>
/// 플레이어의 상태를 나타내는 클래스 
/// </summary>
public class AnimalState
{
    #region Components // Animal의 인스펙터에 있는 컴포넌트들
    protected AnimalStateMachine stateMachine;
    protected AnimalAnim animalAnim; 
    protected CwAnimal animalObject;    
    protected Rigidbody2D rb;
    #endregion

    #region Variables // 상태를 나타내는 변수들
    private string animBoolName;
    protected float xInput;
    protected float yInput;   

    protected float stateTimer;
    protected bool triggerCalled;
    #endregion  

    public AnimalState(AnimalAnim _animalAnim, AnimalStateMachine _stateMachine, CwAnimal _animalObject, string _animBoolName)
    {
        this.animalAnim = _animalAnim;
        this.stateMachine = _stateMachine;
        this.animalObject = _animalObject;
        this.animBoolName = _animBoolName;
    } 

    public virtual void Enter()
    {
        animalAnim.anim.SetBool(animBoolName, true);
        rb = animalAnim.rb;
        triggerCalled = false; 
    }

    public virtual void Update()
    {
        //stateTimer -= Time.deltaTime; 
        animalAnim.anim.SetFloat("yVelocity", rb.linearVelocityY); 
    }

    public virtual void Exit()
    {
        animalAnim.anim.SetBool(animBoolName, false);
    }

    /// <summary>
    /// 애니메이션이 끝났을 때 호출되는 메셔드
    /// </summary>
    public virtual void AnimationFinishTrigger()
    {
        triggerCalled = true;
    }


}
