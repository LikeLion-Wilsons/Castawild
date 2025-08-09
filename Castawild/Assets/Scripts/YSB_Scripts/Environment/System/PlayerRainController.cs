using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class PlayerRainController : MonoBehaviour
{
    private ParticleSystem _rainParticles;

    private void Awake()
    {
        _rainParticles = GetComponent<ParticleSystem>();
        // 파티클이 실수로라도 재생되지 않도록 확실히 멈추고 시작합니다.
        _rainParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    private void OnEnable()
    {
        // 이벤트 구독
        WeatherEvents.OnRainStateChanged += HandleRainStateChange;
    }

    private void OnDisable()
    {
        // 이벤트 구독 해제 (메모리 누수 방지)
        WeatherEvents.OnRainStateChanged -= HandleRainStateChange;
    }

    private void HandleRainStateChange(bool isRaining)
    {
        if (_rainParticles == null) return;
        Debug.Log($"Rain state changed: {(isRaining ? "Raining" : "Not Raining")}", this);
        if (isRaining)
        {
            if (!_rainParticles.isPlaying)
            {
                _rainParticles.Play();
            }
        }
        else
        {
            if (_rainParticles.isPlaying)
            {
                _rainParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }
        }
    }

    private void LateUpdate()
    {
        transform.rotation = Quaternion.identity;//회전 없애기 위해서
    }
}