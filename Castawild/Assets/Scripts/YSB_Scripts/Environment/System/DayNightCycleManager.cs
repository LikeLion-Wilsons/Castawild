using UnityEngine;
using Fusion;

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

    [Header("Light Intensity Control")]
    [Tooltip("빛의 강도를 시간에 따라 조절하는 애니메이션 곡선(0-1)")]
    [SerializeField] private AnimationCurve sunIntensityCurve;
    [Tooltip("달의 강도를 시간에 따라 조절하는 애니메이션 곡선(0-1)")]
    [SerializeField] private AnimationCurve moonIntensityCurve;

    [Header("Developer Mode")]
    [SerializeField] private bool developerMode = false;
    [SerializeField] private float developerSpeedMultiplier = 10f;
    [SerializeField] private KeyCode toggleDeveloperModeKey = KeyCode.RightBracket;
    [Tooltip("에디터 미리보기 (0-1)")]
    [SerializeField, Range(0, 1)] private float previewTimeOfDay = 0.25f;

    [Networked, OnChangedRender(nameof(OnTimeOfDayChanged))]
    private float TimeOfDay { get; set; }

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority) return;

        float currentSpeedMultiplier = developerMode ? developerSpeedMultiplier : 1f;

        TimeOfDay += (Runner.DeltaTime / dayDurationInSeconds) * currentSpeedMultiplier;
        
        TimeOfDay %= 1f;
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleDeveloperModeKey))
        {
            developerMode = !developerMode;
        }
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
        if (sunLight != null)//태양
        {
            sunLight.transform.rotation = Quaternion.Euler(TimeOfDay * 360f - 90f, 170f, 0);
            if (sunIntensityCurve != null)
            {
                sunLight.intensity = sunIntensityCurve.Evaluate(TimeOfDay);
            }
        }

        if (moonLight != null)//달
        {
            moonLight.transform.rotation = Quaternion.Euler(TimeOfDay * 360f - 90f + 180f, 170f, 0);
            if (moonIntensityCurve != null)
            {
                moonLight.intensity = moonIntensityCurve.Evaluate(TimeOfDay);
            }
        }
    }

    private void OnValidate()
    {
        if (Application.isPlaying) return;

        float previewTime = previewTimeOfDay;
        
        var sunCurve = sunIntensityCurve != null && sunIntensityCurve.keys.Length > 0 ? sunIntensityCurve : AnimationCurve.Linear(0.2f, 1, 0.8f, 1);
        var moonCurve = moonIntensityCurve != null && moonIntensityCurve.keys.Length > 0 ? moonIntensityCurve : AnimationCurve.Linear(0, 0.1f, 1, 0.1f);

        if (sunLight != null)
        {
            sunLight.transform.rotation = Quaternion.Euler(previewTime * 360f - 90f, 170f, 0);
            sunLight.intensity = sunCurve.Evaluate(previewTime);
        }
        if (moonLight != null)
        {
            moonLight.transform.rotation = Quaternion.Euler(previewTime * 360f - 90f + 180f, 170f, 0);
            moonLight.intensity = moonCurve.Evaluate(previewTime);
        }
    }
}