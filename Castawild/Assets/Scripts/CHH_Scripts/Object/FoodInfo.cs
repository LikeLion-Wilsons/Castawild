using Fusion;
using UnityEngine;

public class FoodInfo : MonoBehaviour
{
    [SerializeField] private float restoreHPValue;
    [SerializeField] private float restoreStaminaValue;
    [SerializeField] private float restoreThirstValue;
    [SerializeField] private float restoreHungerValue;

    public float RestoreHPValue => restoreHPValue;
    public float RestoreStaminaValue => restoreStaminaValue;
    public float RestoreThirstValue => restoreThirstValue;
    public float RestoreHungerValue => restoreHungerValue;
    public FoodInfoData GetData() => new FoodInfoData(restoreHPValue, restoreStaminaValue, restoreThirstValue, restoreHungerValue);
}

public struct FoodInfoData : INetworkStruct
{
    public float restoreHPValue;
    public float restoreStaminaValue;
    public float restoreHungerValue;
    public float restoreThirstValue;

    public FoodInfoData(float restoreHPValue, float restoreStaminaValue, float restoreThirstValue, float restoreHungerValue)
    {
        this.restoreHPValue = restoreHPValue;
        this.restoreStaminaValue = restoreStaminaValue;
        this.restoreHungerValue = restoreHungerValue;
        this.restoreThirstValue = restoreThirstValue;
    }

    public bool IsEmpty() => restoreHPValue == -999f;
    public static FoodInfoData Empty => new FoodInfoData(-999f, 999f, 999f, 999f);
}