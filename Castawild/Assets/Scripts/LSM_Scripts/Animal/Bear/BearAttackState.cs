using UnityEngine;
using UnityEngine.AI;

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

    public override void FixedUpdateNetwork()
    { 
        base.FixedUpdateNetwork();
        if (bearObject.IsPlayerDetection() == null) 
        {
            LookAtPlayer(); 
            ChangeReturnState();
        }
        else if (bearObject.IsPlayerDetection() > bearObject.AttackRange) 
        {
            bearAnim.agent.isStopped = false;
            bearAnim.agent.updatePosition = true;
            LookAtPlayer();
            bearAnim.anim.SetBool("Run", true);            
            NavMeshHit hit;
            if (NavMesh.SamplePosition(bearObject.GetPlayerPosition(), out hit, 1f, NavMesh.AllAreas))
            {
                bearAnim.agent.SetDestination(hit.position);
            }
        }
        else if (bearObject.IsPlayerDetection() <= bearObject.AttackRange) 
        {
            bearAnim.anim.SetBool("Run", false);
            bearAnim.agent.isStopped = true;
            bearAnim.agent.updatePosition = false;
            LookAtPlayer(); 
            AttackPlayer();
        }
    }
    public override void Exit()
    { 
        base.Exit();
        bearAnim.anim.SetBool("Run", false);
    } 
    private void LookAtPlayer()
    {
        Vector3 playerPos = bearObject.GetPlayerPosition();
        Vector3 dir = playerPos - bearObject.transform.position;
        dir.y = 0;
        Quaternion lookRot = Quaternion.LookRotation(dir);
        bearObject.transform.rotation = Quaternion.RotateTowards(bearObject.transform.rotation, lookRot, Time.deltaTime * 100);
    }

    private void AttackPlayer()
    {
        AttackType = Random.Range(1, 4);
        bearAnim.anim.SetInteger("AttackIndex", AttackType); 
    }

}


