using UnityEngine;

/// <summary>
/// 로밍중인 평상시 곰을 나타내는 클래스
/// </summary>
public class BearIdleState : BearState
{
    public BearIdleState(BearAnim _bearAnim, AnimalStateMachine _stateMachine, CwBear _bearObject, string _animBoolName) : base(_bearAnim, _stateMachine, _bearObject, _animBoolName)
    {
    } 

    protected override void Awake()
    {
        base.Awake();
        bearAnim.agent.speed = animalObject.MoveSpeed;        
    }

    public override void Enter()
    {
        Debug.Log("BearIdleState Enter");
        base.Enter(); 
        bearAnim.agent.isStopped = false;
        bearAnim.agent.updatePosition = true;
         
        // 곰이 귀환 상태라면 가던 길로 계속 이동
        if (bearObject.IsReturned) 
        { 
            bearAnim.agent.SetDestination(bearObject.layOver[bearObject.layOverIndex]);
            bearAnim.anim.SetBool("IdleMove", true);
        } 
    }

    public override void FixedUpdateNetwork()
    {
        base.FixedUpdateNetwork();

        // 곰이 이동할 위치가 비어있다면 새로 생성 후 이동 시작
        if (bearObject.layOver[0] == Vector3.zero && bearObject.layOver[1] == Vector3.zero && bearObject.layOver[2] == Vector3.zero)
        {
            bearObject.RandomNavTriangle(bearAnim.transform.position, bearObject.IdleRadius * 0.8f, bearObject.IdleRadius, -1); 
            bearAnim.agent.SetDestination(bearObject.layOver[bearObject.layOverIndex]);
        }
        else if (bearObject.layOver[0] == bearObject.layOver[1])
        {
            // 제대로 생성 안되었으면 다시 생성
            bearObject.RandomNavTriangle(bearAnim.transform.position, bearObject.IdleRadius * 0.8f, bearObject.IdleRadius, -1);
        }

        // 플레이어 감지
        if (bearObject.IsPlayerDetection() != null)
        {
            idlePosition = bearAnim.transform.position;
            ChangeAlertState();
            return;
        }

        // 곰이 이동 중이 아니고 목적지에 도착했다면 다음 위치로 이동
        if (!bearAnim.agent.pathPending && bearAnim.agent.remainingDistance <= bearAnim.agent.stoppingDistance) 
        {
            bearObject.IsReturned = false; 
            bearObject.layOverIndex = (bearObject.layOverIndex + 1) % bearObject.layOver.Length;  
            bearAnim.agent.SetDestination(bearObject.layOver[bearObject.layOverIndex]);
            bearAnim.anim.SetBool("IdleMove", true);
        } 


    }
    public override void Exit()
    { 
        base.Exit();
        bearAnim.anim.SetBool("IdleMove", false);
    }  
}


