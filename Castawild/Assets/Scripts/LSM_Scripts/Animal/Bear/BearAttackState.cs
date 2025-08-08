using UnityEngine;

/// <summary>
/// 공격중인 곰을 나타내는 클래스
/// </summary>
public class BearAttackState : BearState
{  
    public BearAttackState(BearAnim _bearAnim, AnimalStateMachine _stateMachine, CwBear _bearObject, string _animBoolName) : base(_bearAnim, _stateMachine, _bearObject, _animBoolName)     
    { 
    } 

    private int AttackType = 0;
    protected override void Awake()
    {
        base.Awake();
    }

    public override void Enter()
    { 
        Debug.Log("BearAttackState Enter");  
        base.Enter(); 
    }

    public override void Update()
    { 
        base.Update();
        if (bearObject.IsPlayerDetection() == null) //플레이어 사라짐
        {
            LookAtPlayer();
            bearAnim.anim.SetInteger("Attack", 0);
            ChangeReturnState();
        }
        else if (bearObject.IsPlayerDetection() >= 2) //플레이어 멈
        {
            LookAtPlayer();
            bearAnim.anim.SetBool("Run", true);
            bearAnim.agent.SetDestination(bearObject.GetPlayerPosition());            
            return;
        }
        else if (bearObject.IsPlayerDetection() < 2) //플레이어 가까움
        {
            bearAnim.agent.isStopped = true;
            bearAnim.agent.updatePosition = false;
            LookAtPlayer(); 
            AttackPlayer();
        }
    }
    public override void Exit()
    { 
        base.Exit();  
    }

    private void LookAtPlayer()
    {
        Vector3 playerPos = bearObject.GetPlayerPosition();
        Vector3 dir = playerPos - bearObject.transform.position;
        dir.y = 0;
        Quaternion lookRot = Quaternion.LookRotation(dir);
        bearObject.transform.rotation = Quaternion.RotateTowards(bearObject.transform.rotation, lookRot, Time.deltaTime * 10f);
    }

    private void AttackPlayer()
    {
        bearAnim.anim.SetInteger("Attack", AttackType); 
        AttackType = Random.Range(1, 3);  
    }

}
