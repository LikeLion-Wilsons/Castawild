using UnityEngine; 

//_bearAnim 이런거 통일하기

/// <summary>
/// 경계가 풀린 곰을 나타내는 클래스
/// </summary>
public class BearReturnState : BearState
{  
    public BearReturnState(BearAnim _bearAnim, AnimalStateMachine _stateMachine, CwBear _bearObject, string _animBoolName) : base(_bearAnim, _stateMachine, _bearObject, _animBoolName)     
    { 
    } 
    protected override void Awake()
    {
        base.Awake();
    }

    public override void Enter()
    { 
        Debug.Log("BearReturnState Enter");
        bearAnim.anim.SetBool("IdleMove", true);
        base.Enter(); 
    }

    public override void FixedUpdateNetwork()
    {
        base.FixedUpdateNetwork(); 
        if (bearObject.IsPlayerDetection() != null) //플레이어 감지
        { 
            ChangeAlertState();
            return;
        }
        else
        {
            bearAnim.agent.SetDestination(idlePosition);
            //경로 계산중이 아님 + 목적지 도착함 
            if (!bearAnim.agent.pathPending && bearAnim.agent.remainingDistance <= bearAnim.agent.stoppingDistance)
                ChangeIdleState();
        } 
    }
    public override void Exit()
    { 
        base.Exit();  
    } 

}
