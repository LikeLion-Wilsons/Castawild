using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIStats : MonoBehaviour
{
    public Player player;
    [SerializeField] Image hungerBar;//허기
    [SerializeField] Image thirstBar;//목마름
    [SerializeField] Image temperatureBar;//체온
    [SerializeField] Image healthBar;//체력
    [SerializeField] Image staminaBar;//스태미나

    [SerializeField] Color restoreColor;
    [SerializeField] Color decreaseColor;

    void Start()
    {

    }

    void Update()
    {
        if (player == null) return;
        hungerBar.fillAmount = player.Hunger / player.playerData.maxHunger;
        thirstBar.fillAmount = player.Thirst / player.playerData.maxThirst;
        temperatureBar.fillAmount = player.Temperature / player.playerData.maxTemperature;
        healthBar.fillAmount = player.Hp / player.playerData.maxHp;
        staminaBar.fillAmount = player.Stamina / player.playerData.maxStamina;
    }

    public void SetBarColor(StatType stat, float value)
    {
        switch (stat)
        {
            case StatType.Hp:
                SetBarColor(healthBar, value);
                break;
            case StatType.Stamina:
                SetBarColor(staminaBar, value);
                break;
            case StatType.Hunger:
                SetBarColor(hungerBar, value);
                break;
            case StatType.Thirst:
                SetBarColor(thirstBar, value);
                break; 
        }
    }

    private void SetBarColor(Image bar, float value)
    {
        if (value > 0)
            bar.color = restoreColor;
        else if (value < 0)
            bar.color = decreaseColor;
        else
            bar.color = Color.white;
    }
}
