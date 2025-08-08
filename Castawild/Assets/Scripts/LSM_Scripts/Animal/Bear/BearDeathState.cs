using UnityEngine; 
/// <summary>
/// 죽은 곰을 나타내는 클래스
/// </summary>
public class BearDeathState : BearState
{  
    public BearDeathState(BearAnim _bearAnim, AnimalStateMachine _stateMachine, CwBear _bearObject, string _animBoolName) : base(_bearAnim, _stateMachine, _bearObject, _animBoolName)     
    { 
    } 
    protected override void Awake()
    {
        base.Awake();
    } 
    public override void Enter()
    { 
        Debug.Log("BearDeathState Enter");  
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
