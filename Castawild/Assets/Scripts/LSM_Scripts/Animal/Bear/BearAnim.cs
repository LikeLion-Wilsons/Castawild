using UnityEngine; 
/// <summary>
/// 호스트에서 AI FSM을 관리하고,
/// 클라이언트에서 동물 애니메이션을 재생하는 클래스
/// </summary>
public class BearAnim : AnimalAnim
{
    #region states 
    public BearIdleState idleState { get; private set; }
    public BearAlertState alertState { get; private set; }
    public BearAttackState attackState { get; private set; }
    public BearReturnState returnState { get; private set; }
    public BearDeathState deathState { get; private set; }
    #endregion
    #region States 
    public CwBear bearObject { get; private set; } // 속성을 관리하는 객체   

    #endregion

    protected override void Awake() 
    {
        base.Awake();  
        bearObject = GetComponent<CwBear>(); 

        // 각 상태 인스턴스 생성 (this: 플레이어 객체, stateMachine: 상태 머신, "Idle"/"Move": 상태 이름)
        idleState = new BearIdleState(this, stateMachine, bearObject, "Idle");
        alertState = new BearAlertState(this, stateMachine, bearObject, "Alert");
        attackState = new BearAttackState(this, stateMachine, bearObject, "Attack");
        returnState = new BearReturnState(this, stateMachine, bearObject, "Return");
        deathState = new BearDeathState(this, stateMachine, bearObject, "Death");
    }

    protected override void Start()
    {
        base.Start();        
        stateMachine.Initialize(idleState);
    }
    protected override void Update()
    {
        base.Update();
    }
     
}
