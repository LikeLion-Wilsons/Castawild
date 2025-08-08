using UnityEngine; 
/// <summary>
/// 호스트에서 AI FSM을 관리하고,
/// 클라이언트에서 동물 애니메이션을 재생하는 클래스
/// </summary>
public class SeagullAnim : AnimalAnim
{
    #region states 
    public SeagullIdleState idleState { get; private set; } 
    public SeagullEscapeState escapeState { get; private set; } 
    public SeagullDeathState deathState { get; private set; }
    #endregion


    #region States 
    public CwSeagull seagullObject { get; private set; } // 속성을 관리하는 객체  
    #endregion

    protected override void Awake() 
    {
        base.Awake();
        seagullObject = GetComponent<CwSeagull>();

        // 각 상태 인스턴스 생성 (this: 플레이어 객체, stateMachine: 상태 머신, "Idle"/"Move": 상태 이름)
        idleState = new SeagullIdleState(this, stateMachine, seagullObject, "Idle");        
        escapeState = new SeagullEscapeState(this, stateMachine, seagullObject, "Escape"); 
        deathState = new SeagullDeathState(this, stateMachine, seagullObject, "Death");
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
