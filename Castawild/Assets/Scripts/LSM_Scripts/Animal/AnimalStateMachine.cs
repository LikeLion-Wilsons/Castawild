using UnityEngine; 
/// <summary>
/// 상태 머신을 관리하는 클래스
/// </summary>
public class AnimalStateMachine 
{
    //현재의 상태를 나타내는 변수
    public AnimalState currentState { get; private set; }

    /// <summary>
    /// 상태 머신을 초기화하는 메서드
    /// </summary>
    /// <param name="_startState">초기 상태로 지정할 변수</param>
    public void Initialize(AnimalState _startState)
    {
        if (_startState == null)
        {
            Debug.LogError("Start state cannot be null.");
            return; // 초기 상태가 null인 경우 에러 메시지 출력 후 종료
        }
        currentState = _startState; 
        currentState.Enter();
    }

    /// <summary>
    /// 상태를 변경하는 메서드
    /// </summary>
    /// <param name="_newState">변경 할 상태 변수</param>
    public void ChangeState(AnimalState _newState)
    {
        if (currentState == null || _newState == null || currentState == _newState)
        {            
            return; // 현재 상태가 없거나 새 상태가 null이거나 현재 상태와 새 상태가 같으면 아무 작업도 하지 않음
        }
        currentState.Exit();
        currentState = _newState;
        currentState.Enter();
    }
}