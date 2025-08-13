using UnityEngine;  

/// <summary>
/// 동물의 상태를 나타내는 클래스 
/// </summary>
public class AnimalState
{
    #region Components // Animal의 인스펙터에 있는 컴포넌트들
    protected AnimalStateMachine stateMachine;
    protected AnimalAnim animalAnim; 
    protected CwAnimal animalObject;
    protected Vector3 targetPosition = Vector3.zero;
    #endregion

    #region Variables // 상태를 나타내는 변수들
    private string animBoolName;  
    protected float stateTimer; 
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
    }

    public virtual void FixedUpdateNetwork()
    { 
    }

    public virtual void Exit()
    {
        animalAnim.anim.SetBool(animBoolName, false);
    } 

}
