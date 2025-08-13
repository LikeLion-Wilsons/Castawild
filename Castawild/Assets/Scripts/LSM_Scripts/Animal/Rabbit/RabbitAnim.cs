using Fusion;
using UnityEngine; 
/// <summary>
/// 호스트에서 AI FSM을 관리하고,
/// 클라이언트에서 동물 애니메이션을 재생하는 클래스
/// </summary>
public class  RabbitAnim : AnimalAnim
{
    #region states 
    public RabbitIdleState idleState { get; private set; }
    public RabbitAlertState alertState { get; private set; }
    public RabbitEscapeState escapeState { get; private set; }
    public RabbitReturnState returnState { get; private set; }
    public RabbitDeathState deathState { get; private set; }
    #endregion 

    #region States 
    public CwRabbit rabbitObject { get; private set; } // 동물의 속성을 관리하는 객체
    public enum RabbitState
    {
        Idle,
        Alert,
        Escape,
        Return,
        Death
    }
    public enum RabbitPlayAnim
    {
        Idle,
        IdleMove,
        Alert,
        Escape,
        Return,
        Death
    }

    [Networked]
    public RabbitState currentState { get; set; } // 현재 상태 

    [Networked]
    public RabbitPlayAnim currentAnim { get; set; } // 현재 해니메이션
    #endregion 

    protected override void Awake() 
    {
        base.Awake();  
        rabbitObject = GetComponent<CwRabbit>();

        // 각 상태 인스턴스 생성 (this: 플레이어 객체, stateMachine: 상태 머신, "Idle"/"Move": 상태 이름)
        idleState = new RabbitIdleState(this, stateMachine, rabbitObject, "Idle");
        alertState = new RabbitAlertState(this, stateMachine, rabbitObject, "Alert");
        escapeState = new RabbitEscapeState(this, stateMachine, rabbitObject, "Escape");
        returnState = new RabbitReturnState(this, stateMachine, rabbitObject, "Return");
        deathState = new RabbitDeathState(this, stateMachine, rabbitObject, "Death");
    }

    protected override void Start()
    {
        base.Start();
        // 게임시작 시 초기 상태를 대기 상태(idleState)로 설정
        if (Object.HasStateAuthority)
        {
            stateMachine.Initialize(idleState);
            currentState = RabbitState.Idle;
            ChangeRabbitAnim(RabbitPlayAnim.Idle, true); // 애니메이션 초기화
            currentAnim = RabbitPlayAnim.Idle;
        }
        else
        {
            switch (currentState)
            {
                case RabbitState.Idle:
                    stateMachine.Initialize(idleState);
                    ChangeRabbitAnim(RabbitPlayAnim.IdleMove, true);
                    break;
                case RabbitState.Alert:
                    stateMachine.Initialize(alertState);
                    ChangeRabbitAnim(RabbitPlayAnim.Alert, true);
                    break;
                case RabbitState.Escape:
                    stateMachine.Initialize(escapeState);
                    ChangeRabbitAnim(RabbitPlayAnim.Escape, true);
                    break;
                case RabbitState.Return:
                    stateMachine.Initialize(returnState);
                    ChangeRabbitAnim(RabbitPlayAnim.Return, true);
                    break;
                case RabbitState.Death:
                    stateMachine.Initialize(deathState);
                    ChangeRabbitAnim(RabbitPlayAnim.Death, true);
                    break;
                default:
                    break;
            }
        }
    }
    public override void FixedUpdateNetwork()
    {
        base.FixedUpdateNetwork();
    }

    #region Host Authority Methods
    public void ChangeRabbitState(RabbitState newState)
    {
        if (Object.HasStateAuthority)
        {
            RPC_ChangeRabbitState(newState);
            currentState = newState;
        }
    }

    public void ChangeRabbitAnim(RabbitPlayAnim newAnim, bool isPlay)
    {
        if (Object.HasStateAuthority)
        {
            RPC_ChangeRabbitAnim(newAnim, isPlay); 
            currentAnim = newAnim;
            if(newAnim == RabbitPlayAnim.IdleMove)
            {
                currentAnim = RabbitPlayAnim.IdleMove;
            } 
        }
    }

    #endregion

    #region Client Authority Methods
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_ChangeRabbitState(RabbitState newState)
    {
        switch (newState)
        {
            case RabbitState.Idle:
                stateMachine.ChangeState(idleState);
                break;
            case RabbitState.Alert:
                stateMachine.ChangeState(alertState);
                break;
            case RabbitState.Escape:
                stateMachine.ChangeState(escapeState);
                break;
            case RabbitState.Return:
                stateMachine.ChangeState(returnState);
                break;
            case RabbitState.Death:
                stateMachine.ChangeState(deathState);
                break;
            default:
                break;
        }
    }
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_ChangeRabbitAnim(RabbitPlayAnim newAnim, bool isPlay)
    {
        switch (newAnim)
        {
            case RabbitPlayAnim.Idle:
                anim.SetBool("Idle", isPlay);
                break;
            case RabbitPlayAnim.IdleMove:
                anim.SetBool("IdleMove", isPlay);
                break;
            case RabbitPlayAnim.Alert:
                anim.SetBool("Alert", isPlay);
                break;
            case RabbitPlayAnim.Escape:
                anim.SetBool("Escape", isPlay);
                break;
            case RabbitPlayAnim.Return:
                anim.SetBool("Return", isPlay);
                break;
            case RabbitPlayAnim.Death:
                anim.SetBool("Death", isPlay);
                break;
            default:
                break;
        }
    }
    #endregion
}
