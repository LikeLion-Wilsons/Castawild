using UnityEngine;

public class TimeManager : MonoBehaviour
{

    // 인스펙터에 스크롤 바 로 조정할 수 있도록 설정
    [Range(0.1f, 100.0f)]
    public float gameTimeScale = 1.0f;


    private void Update()
    {
        Time.timeScale = gameTimeScale;
    }
}
