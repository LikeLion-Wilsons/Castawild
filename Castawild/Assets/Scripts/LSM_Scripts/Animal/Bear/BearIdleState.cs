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
        for (int i = 0; i < 3; i++)
            layOver[i] = bearObject.RandomNavSphere(bearAnim.transform.position, bearObject.IdleRadius, bearObject.IdleRadius, -1);
    }

    public override void Enter()
    {
        Debug.Log("BearIdleState Enter");
        base.Enter();

        if(IsReturned) //가던 길 이어서 가기
        { 
            bearAnim.agent.SetDestination(layOver[layOverIndex]);
            bearAnim.anim.SetBool("IdleMove", true);
        } 
    }

    public override void Update()
    {
        base.Update();

        if (bearObject.IsPlayerDetection() != null) //플레이어 감지
        {
            idlePosition = bearAnim.transform.position; 
            ChangeAlertState();  
            return;
        } 
        if (!bearAnim.agent.pathPending && bearAnim.agent.remainingDistance <= bearAnim.agent.stoppingDistance) //다음 목적지를 찾아서
        {
            IsReturned = false; 
            layOverIndex = (layOverIndex + 1) % layOver.Length;  
            bearAnim.agent.SetDestination(layOver[layOverIndex]);
            bearAnim.anim.SetBool("IdleMove", true);
        }
        
    }
    public override void Exit()
    { 
        base.Exit();
        bearAnim.anim.SetBool("IdleMove", false);
    }  
}


