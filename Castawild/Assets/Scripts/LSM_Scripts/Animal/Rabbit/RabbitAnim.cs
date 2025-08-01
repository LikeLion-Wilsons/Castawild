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
        // 게임 시작 시 초기 상태를 대기 상태(idleState)로 설정
        stateMachine.Initialize(idleState);
    }
    protected override void Update()
    {
        base.Update();
    }

    public void IdleMove()
    {
        IdleMoveing = true;
    }
}
