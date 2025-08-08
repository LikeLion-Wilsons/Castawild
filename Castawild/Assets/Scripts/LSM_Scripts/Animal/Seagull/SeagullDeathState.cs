using Polyperfect.Common;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

//_bearAnim 이런거 통일하기

/// <summary>
/// 평상시 갈메기을 나타내는 클래스
/// </summary>
public class SeagullDeathState : SeagullState
{  
    public SeagullDeathState(SeagullAnim _seagullAnim, AnimalStateMachine _stateMachine, CwSeagull _seagullObject, string _animBoolName) : base(_seagullAnim, _stateMachine, _seagullObject, _animBoolName)     
    { 
    } 
    protected override void Awake()
    {
        base.Awake();
    }

    public override void Enter()
    { 
        Debug.Log("SeagullDeathState Enter");  
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
