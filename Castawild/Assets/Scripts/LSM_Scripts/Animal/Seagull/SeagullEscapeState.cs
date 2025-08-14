using UnityEngine;

//_bearAnim 이런거 통일하기

/// <summary>
/// 평상시 곰을 나타내는 클래스
/// </summary>
public class SeagullEscapeState : SeagullState
{  
    public SeagullEscapeState(SeagullAnim _bearAnim, AnimalStateMachine _stateMachine, CwSeagull _animalObject, string _animBoolName) : base(_bearAnim, _stateMachine, _animalObject, _animBoolName)     
    { 
    } 
    protected override void Awake()
    {
        base.Awake();
    }

    public override void Enter()
    { 
        Debug.Log("SeagullEscapeState Enter");  
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
