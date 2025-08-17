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
        WeatherEvents.OnRainStateChanged += HandleRainStateChange;
    }

    private void OnDisable()
    {
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
                SoundManager.Instance.PlayBGM(Sound.Env_Rain);
            }
        }
        else
        {
            if (_rainParticles.isPlaying)
            {
                _rainParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                SoundManager.Instance.StopBGM();
                SoundManager.Instance.PlayBGM(Sound.Env_Title);
            }
        }
    }

    private void LateUpdate()
    {
        transform.rotation = Quaternion.identity;//회전 없애기 위해서
    }
}