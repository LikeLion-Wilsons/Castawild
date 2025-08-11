using UnityEngine;

public class FoodInfo : ToolInfo
{
    [SerializeField] private float restoreHPValue;
    [SerializeField] private float restoreStaminaValue;
    [SerializeField] private float restoreThirstValue;
    [SerializeField] private float restoreHungerValue;

    public float RestoreHPValue => restoreHPValue;
    public float RestoreStaminaValue => restoreStaminaValue;
    public float RestoreThirstValue => restoreThirstValue;
    public float RestoreHungerValue => restoreHungerValue;
}