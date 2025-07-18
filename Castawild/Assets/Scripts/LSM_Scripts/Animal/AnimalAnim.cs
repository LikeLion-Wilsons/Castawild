using UnityEngine; 
/// <summary>
/// 호스트에서 AI FSM을 관리하고,
/// 클라이언트에서 동물 애니메이션을 재생하는 클래스
/// </summary>
public class AnimalAnim : MonoBehaviour 
{
    #region Components  
    public Animator anim { get; private set; }
    public Rigidbody2D rb { get; private set; }
    public SpriteRenderer sr { get; private set; }  
    #endregion 

    #region States 
    public AnimalStateMachine stateMachine { get; private set; } // 동물의 상태를 관리하는 상태 머신
    public CwAnimal animalObject { get; private set; } // 동물의 속성을 관리하는 객체 

    // 동물의 상태 
    //public AnimalIdleState idleState { get; private set; } 

    #endregion 
    protected void Awake()
    {
        anim = GetComponent<Animator>();
        rb = GetComponentInParent<Rigidbody2D>();  

        // 상태 머신 인스턴스 생성
        stateMachine = new AnimalStateMachine();
        animalObject = GetComponentInParent<CwAnimal>(); // 플레이어의 속성을 관리하는 객체 생성

        // 각 상태 인스턴스 생성 (this: 플레이어 객체, stateMachine: 상태 머신, "Idle"/"Move": 상태 이름)
        //idleState = new AnimalIdleState(this, stateMachine, animalObject, "Idle"); 
    }

    protected void Start()
    {
        // 게임 시작 시 초기 상태를 대기 상태(idleState)로 설정
        //stateMachine.Initialize(idleState);
    }
    protected void Update()
    {
        stateMachine.currentState.Update();
    }  
}
