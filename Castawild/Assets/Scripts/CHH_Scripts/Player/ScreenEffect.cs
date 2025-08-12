using UnityEngine;
using UnityEngine.Rendering;

public class ScreenEffect : MonoBehaviour
{
    [Header("Cold")]
    [SerializeField] private Material coldEffect;
    [SerializeField] private float coldEffectThreshold = 0.2f;
    [SerializeField] private float maxColdEffectIntensity = 4f;

    [Header("Damaged")]
    [SerializeField] private float bloodEffectThreshold = 0.2f;
    [SerializeField] private Volume takeDamageEffect;
    [SerializeField] private Animator takeDamageEffectAnim;

    public void ContinuousDamageEffect(float hp, float maxHp)
    {
        float hpPercent = hp / maxHp;

        if (hpPercent <= bloodEffectThreshold)
            takeDamageEffect.weight = Mathf.InverseLerp(bloodEffectThreshold, 0f, hpPercent);
        else
            takeDamageEffect.weight = 0f;
    }

    public void ContinuousColdEffect(float temperature, float maxTemperature)
    {
        float temperaturePersent = temperature / maxTemperature;

        if (temperaturePersent <= coldEffectThreshold)
        {
            float intensity = Mathf.Lerp(0f, maxColdEffectIntensity, Mathf.InverseLerp(bloodEffectThreshold, 0f, temperaturePersent));
            coldEffect.SetFloat("_VignetteIntensity", intensity);
        }
        else
            coldEffect.SetFloat("_VignetteIntensity", 0f);
    }

    public void TakeDamageEffect(float takeDamageEffectWeight) => takeDamageEffect.weight = takeDamageEffectWeight;
    public void SetTrigger(string trigger) => takeDamageEffectAnim.SetTrigger(trigger);
}
