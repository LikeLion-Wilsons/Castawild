using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

// --- WEATHER PRESETS ---
[System.Serializable]
public struct WeatherPreset
{
    public string name;
    [Header("Wind")]
    [Range(0f, 5f)]
    public float baseWindPower;
    public float baseWindSpeed;
    [Range(0f, 10f)]
    public float burstsPower;
    public float burstsSpeed;
    public float burstsScale;
    [Header("Visuals")]
    [Tooltip("Multiplies with the Sun Color Gradient to tint the main light.")]
    public Color lightColorTint;
    [Tooltip("Sets the intensity of the directional light.")]
    public float lightIntensity;
    [Tooltip("Multiplies with the Fog Color Gradient to tint the scene fog.")]
    public Color fogColorTint;
    [Tooltip("The cloud material to use for this weather type.")]
    public Material cloudMaterial;

    [Header("Precipitation")]
    public bool isRaining;
}
[RequireComponent(typeof(NetworkObject))]
public class WeatherManager : NetworkBehaviour
{
    public static WeatherManager Instance { get; private set; }

    // --- VISUAL & LIGHTING PROPERTIES ---
    [Header("Lighting & Color")]
    [Space(5)]
    public Gradient sunColorGradient;
    public Gradient fogColorGradient;
    public Gradient ambientColorGradient;

    [Header("Color Gradients Enable Flags")]
    public bool overrideSunColor = true;
    public bool overrideFogColor = true;
    public bool overrideAmbientColor = true;

    // --- CLOUD PROPERTIES ---
    [Header("Volumetric Clouds")]
    [Space(5)]
    [Tooltip("Default material for clouds. This will be dynamically replaced by the material from the active Weather Preset.")]
    public Material cloudsMaterial;
    public float Altitude = 1000f;
    public float volumeSize = 500f;
    public int volumeSamples = 25;

    // --- WIND PROPERTIES (EDITOR ONLY) ---
    [Header("Wind Settings (Editor Preview)")]
    [Space(5)]
    [Tooltip("Base wind for trunks. Networked in game.")]
    [Range(0f, 5f)]
    public float baseWindPower = 3f;
    [Tooltip("Base wind speed. Networked in game.")]
    public float baseWindSpeed = 1f;
    [Tooltip("Wind bursts power. Networked in game.")]
    [Range(0f, 10f)]
    public float burstsPower = 0.5f;
    [Tooltip("Wind bursts speed. Networked in game.")]
    public float burstsSpeed = 5f;
    [Tooltip("Wind bursts scale. Networked in game.")]
    public float burstsScale = 10f;

    [Header("Micro Wind (Not Networked)")]
    [Space(5)]
    [Tooltip("Micro wind for leaves")]
    [Range(0f, 1f)]
    public float microPower = 0.1f;
    [Tooltip("Micro wind for leaves")]
    public float microSpeed = 1f;
    [Tooltip("Micro wind for leaves")]
    public float microFrequency = 3f;

    [Space(10)]
    public float renderDistance = 30f;

    // --- DEVELOPER MODE ---
    [Header("Developer Mode")]
    [Space(5)]
    public bool developerMode = false;

    // --- NETWORKED WEATHER PROPERTIES ---
    [Networked, OnChangedRender(nameof(OnWeatherChanged))]
    private float Net_BaseWindPower { get; set; }
    [Networked, OnChangedRender(nameof(OnWeatherChanged))]
    private float Net_BaseWindSpeed { get; set; }
    [Networked, OnChangedRender(nameof(OnWeatherChanged))]
    private float Net_BurstsPower { get; set; }
    [Networked, OnChangedRender(nameof(OnWeatherChanged))]
    private float Net_BurstsSpeed { get; set; }
    [Networked, OnChangedRender(nameof(OnWeatherChanged))]
    private float Net_BurstsScale { get; set; }
    [Networked, OnChangedRender(nameof(OnWeatherChanged))]
    public Color Net_LightColorTint { get; private set; } = Color.white;
    [Networked, OnChangedRender(nameof(OnWeatherChanged))]
    public float Net_LightIntensity { get; private set; } = 1f;
    [Networked, OnChangedRender(nameof(OnWeatherChanged))]
    public Color Net_FogColorTint { get; private set; } = Color.white;
    [Networked, OnChangedRender(nameof(OnPresetIndexChanged))]
    private int Net_CurrentPresetIndex { get; set; } = -1;

    [Networked, OnChangedRender(nameof(OnRainStateChanged))]
    private NetworkBool Net_IsRaining { get; set; }


    [Header("Weather Presets")]
    [Space(5)]
    public WeatherPreset[] weatherPresets;
    public float minChangeInterval = 30f;
    public float maxChangeInterval = 120f;
    public float transitionDuration = 10f;

    // --- PRIVATE FIELDS ---
    private Mesh quadMesh;
    private Matrix4x4[] matrices;
    private bool hasIssuedMaterialWarning = false;
    private Coroutine _weatherTransitionCoroutine;

    #region --- MonoBehaviour & Fusion Overrides ---

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        quadMesh = Resources.GetBuiltinResource<Mesh>("Quad.fbx");
        UpdateMatrixArray();
    }

    public override void Spawned()
    {
        if (Object.HasStateAuthority)
        {
            if (weatherPresets != null && weatherPresets.Length > 0)
            {
                ApplyWeatherPreset(0);
                if (!developerMode)
                {
                    StartCoroutine(WeatherChangeRoutine());
                }
                // else
                // {
                //     Debug.Log("<color=yellow>DEV_MODE: Automatic weather change is DISABLED.</color>");
                // }
            }
        }

        // Initial visual sync
        OnWeatherChanged();
        OnPresetIndexChanged();
        OnRainStateChanged();
    }

    void Update()
    {
        if (!Application.isPlaying)
        {
            UpdateWindShaderValues();
            UpdateCloudsVolume();
        }
    }

    public override void Render()
    {
        if (Object.HasStateAuthority && developerMode)
        {
            DeveloperMode_CheckInput();
        }

        UpdateCloudsVolume();
    }

    #endregion

    #region --- Developer Mode (Server Only) ---

    private void DeveloperMode_CheckInput()
    {
        if (weatherPresets == null || weatherPresets.Length == 0) return;

        for (int i = 0; i < 9 && i < weatherPresets.Length; i++)
        {
            if (Input.GetKeyDown(KeyCode.F1 + i))
            {
                Debug.Log($"<color=yellow>DEV_MODE: Manually changing weather to preset {i + 1} ('{weatherPresets[i].name}').</color>");

                if (_weatherTransitionCoroutine != null) StopCoroutine(_weatherTransitionCoroutine);

                _weatherTransitionCoroutine = StartCoroutine(TransitionToWeather(i));
                break;
            }
        }
    }

    #endregion

    #region --- Weather Change Logic (Server Only) ---

    private IEnumerator WeatherChangeRoutine()
    {
        while (Object.HasStateAuthority && !developerMode)
        {
            float waitTime = Random.Range(minChangeInterval, maxChangeInterval);
            yield return new WaitForSeconds(waitTime);

            if (weatherPresets != null && weatherPresets.Length > 0)
            {
                int randomIndex = Random.Range(0, weatherPresets.Length);
                if (_weatherTransitionCoroutine != null) StopCoroutine(_weatherTransitionCoroutine);
                _weatherTransitionCoroutine = StartCoroutine(TransitionToWeather(randomIndex));
            }
        }
    }

    private IEnumerator TransitionToWeather(int targetPresetIndex)
    {
        if (targetPresetIndex < 0 || targetPresetIndex >= weatherPresets.Length) yield break;

        Net_CurrentPresetIndex = targetPresetIndex;
        WeatherPreset targetPreset = weatherPresets[targetPresetIndex];

        if (Object.HasStateAuthority)
        {
            Net_IsRaining = targetPreset.isRaining;
        }

        float timer = 0f;
        float startBaseWindPower = Net_BaseWindPower;
        float startBaseWindSpeed = Net_BaseWindSpeed;
        float startBurstsPower = Net_BurstsPower;
        float startBurstsSpeed = Net_BurstsSpeed;
        float startBurstsScale = Net_BurstsScale;
        Color startLightTint = Net_LightColorTint;
        float startLightIntensity = Net_LightIntensity;
        Color startFogTint = Net_FogColorTint;

        while (timer < transitionDuration)
        {
            timer += Runner.DeltaTime;
            float progress = Mathf.Clamp01(timer / transitionDuration);

            Net_BaseWindPower = Mathf.Lerp(startBaseWindPower, targetPreset.baseWindPower, progress);
            Net_BaseWindSpeed = Mathf.Lerp(startBaseWindSpeed, targetPreset.baseWindSpeed, progress);
            Net_BurstsPower = Mathf.Lerp(startBurstsPower, targetPreset.burstsPower, progress);
            Net_BurstsSpeed = Mathf.Lerp(startBurstsSpeed, targetPreset.burstsSpeed, progress);
            Net_BurstsScale = Mathf.Lerp(startBurstsScale, targetPreset.burstsScale, progress);
            Net_LightColorTint = Color.Lerp(startLightTint, targetPreset.lightColorTint, progress);
            Net_LightIntensity = Mathf.Lerp(startLightIntensity, targetPreset.lightIntensity, progress);
            Net_FogColorTint = Color.Lerp(startFogTint, targetPreset.fogColorTint, progress);

            yield return null;
        }

        ApplyWeatherPreset(targetPresetIndex);
        _weatherTransitionCoroutine = null;
    }

    private void ApplyWeatherPreset(int presetIndex)
    {
        if (presetIndex < 0 || presetIndex >= weatherPresets.Length) return;

        WeatherPreset preset = weatherPresets[presetIndex];

        Net_BaseWindPower = preset.baseWindPower;
        Net_BaseWindSpeed = preset.baseWindSpeed;
        Net_BurstsPower = preset.burstsPower;
        Net_BurstsSpeed = preset.burstsSpeed;
        Net_BurstsScale = preset.burstsScale;
        Net_LightColorTint = preset.lightColorTint;
        Net_LightIntensity = preset.lightIntensity;
        Net_FogColorTint = preset.fogColorTint;
        Net_CurrentPresetIndex = presetIndex;

        Net_IsRaining = preset.isRaining;
    }

    #endregion

    #region --- Visual Updates ---

    void OnRainStateChanged()
    {
        WeatherEvents.OnRainStateChanged?.Invoke(Net_IsRaining);
    }

    void OnPresetIndexChanged()
    {
        if (Net_CurrentPresetIndex < 0 || Net_CurrentPresetIndex >= weatherPresets.Length) return;

        var preset = weatherPresets[Net_CurrentPresetIndex];
        if (preset.cloudMaterial != null)
        {
            this.cloudsMaterial = preset.cloudMaterial;
        }
        else
        {
            Debug.LogWarning($"Weather preset '{preset.name}' has a null cloud material assigned.");
        }
    }

    void OnWeatherChanged()
    {
        UpdateWindShaderValues();
    }

    private void UpdateWindShaderValues()
    {
        if (Application.isPlaying)
        {
            Shader.SetGlobalFloat("WindPower", Net_BaseWindPower);
            Shader.SetGlobalFloat("WindSpeed", Net_BaseWindSpeed);
            Shader.SetGlobalFloat("WindBurstsPower", Net_BurstsPower);
            Shader.SetGlobalFloat("WindBurstsSpeed", Net_BurstsSpeed);
            Shader.SetGlobalFloat("WindBurstsScale", Net_BurstsScale);
        }
        else
        {
            Shader.SetGlobalFloat("WindPower", baseWindPower);
            Shader.SetGlobalFloat("WindSpeed", baseWindSpeed);
            Shader.SetGlobalFloat("WindBurstsPower", burstsPower);
            Shader.SetGlobalFloat("WindBurstsSpeed", burstsSpeed);
            Shader.SetGlobalFloat("WindBurstsScale", burstsScale);
        }

        Shader.SetGlobalFloat("MicroPower", microPower);
        Shader.SetGlobalFloat("MicroSpeed", microSpeed);
        Shader.SetGlobalFloat("MicroFrequency", microFrequency);
        Shader.SetGlobalFloat("GrassRenderDist", renderDistance);
    }

    private void UpdateCloudsVolume()
    {
        if (cloudsMaterial == null) return;

        volumeSamples = Mathf.Max(1, volumeSamples);
        volumeSize = Mathf.Max(0, volumeSize);
        UpdateMatrixArray();

        if (!cloudsMaterial.HasProperty("_ScatteringColor"))
        {
            if (!hasIssuedMaterialWarning)
            {
                Debug.LogWarning("The assigned material in the Cloud material slot of the EnvironmentManager isn't supported for dynamic color changes.");
                hasIssuedMaterialWarning = true;
            }
        }
        else
        {
            hasIssuedMaterialWarning = false;
        }

        cloudsMaterial.SetFloat("_cloudsPosition", Altitude);
        cloudsMaterial.SetFloat("_cloudsHeight", volumeSize);

        float volumeOffset = volumeSize / volumeSamples / 2f;
        Vector3 cloudsStartPosition = transform.position + new Vector3(0, Altitude, 0) + (Vector3.up * (volumeOffset * volumeSamples / 2f));

        for (int i = 0; i < volumeSamples; i++)
        {
            matrices[i] = Matrix4x4.TRS(cloudsStartPosition - (Vector3.up * volumeOffset * i), Quaternion.Euler(-90, 0, 0), new Vector3(10000, 10000, 10000));
        }

        Graphics.DrawMeshInstanced(quadMesh, 0, cloudsMaterial, matrices, volumeSamples);
    }

    private void UpdateMatrixArray()
    {
        if (matrices == null || matrices.Length != volumeSamples)
        {
            matrices = new Matrix4x4[volumeSamples];
        }
    }

    #endregion
}
