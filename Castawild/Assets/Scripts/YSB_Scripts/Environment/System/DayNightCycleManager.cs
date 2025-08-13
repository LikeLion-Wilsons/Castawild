using UnityEngine;
using Fusion;
using System.Linq;
using System;

[RequireComponent(typeof(NetworkObject))]
public class DayNightCycleManager : NetworkBehaviour
{
    [Header("Lights")]
    [Tooltip("directional light(sun)")]
    [SerializeField] private Light sunLight;
    [Tooltip("directional light(moon)")]
    [SerializeField] private Light moonLight;

    [Header("Time Settings")]
    [Tooltip("게임 내 낮과 밤의 주기 (s)")]
    [SerializeField] private float dayDurationInSeconds = 600f; //600s

    [Header("Skybox Settings")]
    [SerializeField] private Material daySkybox;
    [SerializeField] private Material nightSkybox;
    [Tooltip("전환 지속시간 (하루 길이 0-1 비율)")]
    [SerializeField, Range(0, 0.5f)] private float transitionDuration = 0.1f;
    private Material _daySkyboxInstance;
    private Material _nightSkyboxInstance;

    [Header("Is Night Time")]
    [HideInInspector] public bool isNightTime => TimeOfDay < 0.25f || TimeOfDay > 0.75f;

    [Header("Light Intensity Control")]
    [Tooltip("빛의 강도를 시간에 따라 조절하는 애니메이션 곡선(0-1)")]
    [SerializeField] private AnimationCurve sunIntensityCurve;
    [Tooltip("달의 강도를 시간에 따라 조절하는 애니메이션 곡선(0-1)")]
    [SerializeField] private AnimationCurve moonIntensityCurve;

    [Header("Developer Mode")]
    [SerializeField] private bool developerMode = false;
    [Tooltip("개발자 모드일 때의 시간 배율")]
    [SerializeField] private float developerModeSpeed = 10f;
    [Tooltip("개발자 모드 toggle 키")]
    [SerializeField] private KeyCode toggleDeveloperModeKey = KeyCode.RightBracket;
    [Tooltip("에디터 미리보기 (0-1)")]
    [SerializeField, Range(0, 1)] private float previewTimeOfDay = 0.25f;

    [Header("Time Pause Settings")]
    [SerializeField] private KeyCode togglePauseKey = KeyCode.P;

    [Header("Time Skip Settings")]
    [SerializeField] private float skipSpeed = 200f;

    // --- Networked Properties ---
    [Networked, OnChangedRender(nameof(OnTimeOfDayChanged))]
    private float TimeOfDay { get; set; }
    private enum TimeSkipState { Normal, Skipping }
    [Networked] private TimeSkipState CurrentState { get; set; }
    [Networked] private float TargetTimeOfDay { get; set; }
    [Networked, Capacity(16)] private NetworkLinkedList<PlayerRef> SleepingPlayers { get; }
    [Networked] private NetworkBool IsTimePaused { get; set; }
    public static event Action OnTimeSkipStarted;

    private void Awake()
    {
        if (daySkybox != null) _daySkyboxInstance = new Material(daySkybox);
        if (nightSkybox != null) _nightSkyboxInstance = new Material(nightSkybox);
    }

    private void OnDestroy()
    {
        if (_daySkyboxInstance != null) Destroy(_daySkyboxInstance);
        if (_nightSkyboxInstance != null) Destroy(_nightSkyboxInstance);
    }

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority) return;
        if (IsTimePaused) return;

        switch (CurrentState)
        {
            case TimeSkipState.Normal:
                float currentSpeedMultiplier = developerMode ? developerModeSpeed : 1f;
                TimeOfDay += Runner.DeltaTime / dayDurationInSeconds * currentSpeedMultiplier;
                TimeOfDay %= 1f;
                break;

            case TimeSkipState.Skipping:
                float distance = (TargetTimeOfDay - TimeOfDay + 1f) % 1f;
                if (distance < 0.01f || distance > 0.99f)
                {
                    TimeOfDay = TargetTimeOfDay;
                    CurrentState = TimeSkipState.Normal;
                    SleepingPlayers.Clear();
                    Rpc_NotifyTimeSkipStarted();
                    Debug.Log("Time skip completed. Time of day set to: " + TimeOfDay);
                }
                else
                {
                    TimeOfDay += Runner.DeltaTime / dayDurationInSeconds * skipSpeed;
                    TimeOfDay %= 1f;
                }
                break;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleDeveloperModeKey))
        {
            developerMode = !developerMode;
        }

        if (Input.GetKeyDown(togglePauseKey))
        {
            Rpc_ToggleTimePause();
        }
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void Rpc_ToggleTimePause()
    {
        IsTimePaused = !IsTimePaused;
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void Rpc_SetSleepingState(NetworkBool isSleeping, PlayerRef playerRef)
    {
        Debug.Log($"Rpc_SetSleepingState called: isSleeping={isSleeping}, playerRef={playerRef}");
        if (isSleeping)
        {
            if (!SleepingPlayers.Contains(playerRef))
                SleepingPlayers.Add(playerRef);
        }
        else
        {
            SleepingPlayers.Remove(playerRef);
        }

        CleanupSleepingPlayers();

        TryTriggerTimeSkip();
    }

    private void CleanupSleepingPlayers()
    {
        if (!Object.HasStateAuthority) return;

        for (int i = SleepingPlayers.Count - 1; i >= 0; i--)
        {
            var playerRef = SleepingPlayers.Get(i);
            if (!Runner.IsPlayerValid(playerRef))
            {
                SleepingPlayers.Remove(playerRef);
            }
        }
    }

    private void TryTriggerTimeSkip()
    {
        if (!Object.HasStateAuthority) return;
        if (CurrentState == TimeSkipState.Skipping) return;

        if (Runner.ActivePlayers.Count() > 0 && SleepingPlayers.Count >= Runner.ActivePlayers.Count())
        {
            TargetTimeOfDay = 0.25f;
            CurrentState = TimeSkipState.Skipping;
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void Rpc_NotifyTimeSkipStarted()
    {
        OnTimeSkipStarted?.Invoke();
    }

    private void OnTimeOfDayChanged()
    {
        UpdateLights();
    }

    public override void Render()
    {
        UpdateLights();
    }

    private void UpdateLights()
    {
        // Light rotation and intensity
        if (sunLight != null)
        {
            sunLight.transform.rotation = Quaternion.Euler(TimeOfDay * 360f - 90f, 170f, 0);
            if (sunIntensityCurve != null) sunLight.intensity = sunIntensityCurve.Evaluate(TimeOfDay);
        }

        if (moonLight != null)
        {
            moonLight.transform.rotation = Quaternion.Euler(TimeOfDay * 360f - 90f + 180f, 170f, 0);
            if (moonIntensityCurve != null) moonLight.intensity = moonIntensityCurve.Evaluate(TimeOfDay);
        }

        // Skybox transition
        UpdateSkybox();

        // Weather integration
        var weatherManager = WeatherManager.Instance;
        if (weatherManager != null)
        {
            Light activeLight = sunLight.intensity > moonLight.intensity ? sunLight : moonLight;
            if (activeLight == null) return;

            float dot = Vector3.Dot(activeLight.transform.forward, Vector3.up);
            float time = (dot + 1f) / 2f;

            activeLight.intensity *= weatherManager.Net_LightIntensity;

            if (weatherManager.overrideSunColor)
            {
                activeLight.color = weatherManager.sunColorGradient.Evaluate(time) * weatherManager.Net_LightColorTint;
            }

            if (weatherManager.overrideFogColor)
                RenderSettings.fogColor = weatherManager.fogColorGradient.Evaluate(time) * weatherManager.Net_FogColorTint;

            if (weatherManager.overrideAmbientColor)
                RenderSettings.ambientLight = weatherManager.ambientColorGradient.Evaluate(time);

            if (weatherManager.cloudsMaterial != null && weatherManager.cloudsMaterial.HasProperty("_ScatteringColor"))
            {
                weatherManager.cloudsMaterial.SetColor("_ScatteringColor", activeLight.color);
            }
            Shader.SetGlobalColor("_WaterColor", RenderSettings.fogColor);
        }
    }

    private void UpdateSkybox()
    {
        if (_daySkyboxInstance == null || _nightSkyboxInstance == null) return;

        float sunriseTime = 0.25f;
        float sunsetTime = 0.75f;
        float halfDuration = transitionDuration / 2f;

        // DUSK (Day -> Night)
        if (TimeOfDay >= sunsetTime - halfDuration && TimeOfDay <= sunsetTime)
        {
            RenderSettings.skybox = _daySkyboxInstance;
            float t = Mathf.InverseLerp(sunsetTime, sunsetTime - halfDuration, TimeOfDay);
            SetSkyboxTint(_daySkyboxInstance, t);
        }
        else if (TimeOfDay > sunsetTime && TimeOfDay <= sunsetTime + halfDuration)
        {
            RenderSettings.skybox = _nightSkyboxInstance;
            float t = Mathf.InverseLerp(sunsetTime, sunsetTime + halfDuration, TimeOfDay);
            SetSkyboxTint(_nightSkyboxInstance, t);
        }
        // DAWN (Night -> Day)
        else if (TimeOfDay >= sunriseTime - halfDuration && TimeOfDay <= sunriseTime)
        {
            RenderSettings.skybox = _nightSkyboxInstance;
            float t = Mathf.InverseLerp(sunriseTime, sunriseTime - halfDuration, TimeOfDay);
            SetSkyboxTint(_nightSkyboxInstance, t);
        }
        else if (TimeOfDay > sunriseTime && TimeOfDay <= sunriseTime + halfDuration)
        {
            RenderSettings.skybox = _daySkyboxInstance;
            float t = Mathf.InverseLerp(sunriseTime, sunriseTime + halfDuration, TimeOfDay);
            SetSkyboxTint(_daySkyboxInstance, t);
        }
        // DAY TIME
        else if (TimeOfDay > sunriseTime + halfDuration && TimeOfDay < sunsetTime - halfDuration)
        {
            RenderSettings.skybox = _daySkyboxInstance;
            SetSkyboxTint(_daySkyboxInstance, 1f);
        }
        // NIGHT TIME
        else
        {
            RenderSettings.skybox = _nightSkyboxInstance;
            SetSkyboxTint(_nightSkyboxInstance, 1f);
        }
    }

    private void SetSkyboxTint(Material skybox, float t)
    {
        if (skybox != null && skybox.HasProperty("_Tint"))
        {
            skybox.SetColor("_Tint", Color.Lerp(Color.black, Color.white, t));
        }
    }


    private void OnValidate()
    {
        if (Application.isPlaying) return;

        float previewTime = previewTimeOfDay;

        var sunCurve = sunIntensityCurve != null && sunIntensityCurve.keys.Length > 0 ? sunIntensityCurve : AnimationCurve.Linear(0.2f, 1, 0.8f, 1);
        var moonCurve = moonIntensityCurve != null && moonIntensityCurve.keys.Length > 0 ? moonIntensityCurve : AnimationCurve.Linear(0, 0.1f, 1, 0.1f);
        //태양
        if (sunLight != null)
        {
            sunLight.transform.rotation = Quaternion.Euler(previewTime * 360f - 90f, 170f, 0);
            sunLight.intensity = sunCurve.Evaluate(previewTime);
        }
        //달
        if (moonLight != null)
        {
            moonLight.transform.rotation = Quaternion.Euler(previewTime * 360f - 90f + 180f, 170f, 0);
            moonLight.intensity = moonCurve.Evaluate(previewTime);
        }

        // Preview skybox
        if (daySkybox != null && nightSkybox != null)
        {
            if (previewTime > 0.25f && previewTime < 0.75f)
            {
                RenderSettings.skybox = daySkybox;
            }
            else
            {
                RenderSettings.skybox = nightSkybox;
            }
        }
    }
}
