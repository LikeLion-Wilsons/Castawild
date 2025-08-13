using Fusion;
using UnityEngine; 
using UnityEngine.AI;

/// <summary>
/// 호스트에서 AI FSM을 관리하고,
/// 클라이언트에서 동물 애니메이션을 재생하는 클래스
/// </summary>
public class AnimalAnim : NetworkBehaviour
{
    #region Components  
    public Animator anim { get; private set; }
    public Rigidbody rb { get; private set; } 
    public NavMeshAgent agent;
    public bool IdleMoveing { set; get; } = false; // 유휴 상태 이동 여부
    #endregion 

    #region States 
    public AnimalStateMachine stateMachine { get; private set; } // 동물의 상태를 관리하는 상태 머신
    public CwAnimal animalObject { get; private set; } // 동물의 속성을 관리하는 객체    
    #endregion 

    protected virtual void Awake() 
    {
        anim = GetComponent<Animator>();
        rb = GetComponentInParent<Rigidbody>();
        agent = GetComponent<NavMeshAgent>();

        // 상태 머신 인스턴스 생성
        stateMachine = new AnimalStateMachine();
        animalObject = GetComponentInParent<CwAnimal>(); // 플레이어의 속성을 관리하는 객체 생성
        
    }

    protected virtual void Start()
    {
        // 게임 시작 시 초기 상태를 대기 상태(idleState)로 설정
        //sstateMachine.Initialize(idleState);
    }
    public override void FixedUpdateNetwork()
    {
        if(Object.HasStateAuthority)
        {
            stateMachine.currentState.FixedUpdateNetwork();
        }
        
    }  
}