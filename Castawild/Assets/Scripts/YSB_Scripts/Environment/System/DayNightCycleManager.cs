using UnityEngine;
using Fusion;
using System.Linq;

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
    [Tooltip("개발자 모드일 때의 시간 배율")]
    [SerializeField] private float developerModeSpeed = 10f;
    [Tooltip("개발자 모드 toggle 키")]
    [SerializeField] private KeyCode toggleDeveloperModeKey = KeyCode.RightBracket;
    [Tooltip("에디터 미리보기 (0-1)")]
    [SerializeField, Range(0, 1)] private float previewTimeOfDay = 0.25f;

    [Header("Time Skip Settings")]
    [SerializeField] private float skipSpeed = 200f;

    // --- Networked Properties ---
    [Networked, OnChangedRender(nameof(OnTimeOfDayChanged))]
    private float TimeOfDay { get; set; }
    private enum TimeSkipState { Normal, Skipping }
    [Networked] private TimeSkipState CurrentState { get; set; }
    [Networked] private float TargetTimeOfDay { get; set; }
    [Networked, Capacity(16)] private NetworkLinkedList<PlayerRef> SleepingPlayers { get; }


    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority) return;

        // --- State Machine for Time Progression ---
        switch (CurrentState)
        {
            case TimeSkipState.Normal:
                // Normal time progression
                float currentSpeedMultiplier = developerMode ? developerModeSpeed : 1f;
                TimeOfDay += (Runner.DeltaTime / dayDurationInSeconds) * currentSpeedMultiplier;
                TimeOfDay %= 1f;
                break;

            case TimeSkipState.Skipping:
                // Fast-forwarding time
                float distance = (TargetTimeOfDay - TimeOfDay + 1f) % 1f;
                if (distance < 0.01f || distance > 0.99f) // Check if we are very close
                {
                    TimeOfDay = TargetTimeOfDay;
                    CurrentState = TimeSkipState.Normal;
                    SleepingPlayers.Clear(); // Everyone wakes up
                }
                else
                {
                    TimeOfDay += (Runner.DeltaTime / dayDurationInSeconds) * skipSpeed;
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
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void Rpc_SetSleepingState(NetworkBool isSleeping, RpcInfo info = default)
    {
        var playerRef = info.Source;

        if (isSleeping)
        {
            if (!SleepingPlayers.Contains(playerRef))
            {
                SleepingPlayers.Add(playerRef);
            }
        }
        else
        {
            SleepingPlayers.Remove(playerRef);
        }

        CheckAndTriggerTimeSkip();
    }

    private void CheckAndTriggerTimeSkip()
    {
        if (!Object.HasStateAuthority) return;
        if (CurrentState == TimeSkipState.Skipping) return; 

        for (int i = SleepingPlayers.Count - 1; i >= 0; i--)
        {
            if (!Runner.IsPlayerValid(SleepingPlayers.Get(i)))
            {
                SleepingPlayers.Remove(SleepingPlayers.Get(i));
            }
        }
        
        if (Runner.ActivePlayers.Count() > 0 && SleepingPlayers.Count >= Runner.ActivePlayers.Count())
        {
            TargetTimeOfDay = 0.25f;
            CurrentState = TimeSkipState.Skipping;
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
    }
}