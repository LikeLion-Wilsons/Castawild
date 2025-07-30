using UnityEngine;
using UnityEngine.UI;

public class UIStats : MonoBehaviour
{
    [SerializeField] Player player;
    [SerializeField] Image hungerBar;//허기
    [SerializeField] Image thirstBar;//목마름
    [SerializeField] Image satiationBar;//만족감
    [SerializeField] Image temperatureBar;//체온
    [SerializeField] Image healthBar;//체력
    [SerializeField] Image staminaBar;//스태미나
    void Start()
    {
        
    }

    void Update()
    {
        if (player == null) return;
        hungerBar.fillAmount = player.playerData.hunger / player.playerData.maxHunger;
        thirstBar.fillAmount = player.playerData.thirst / player.playerData.maxThirst;
        satiationBar.fillAmount = player.playerData.satiation / player.playerData.maxSatiation;
        temperatureBar.fillAmount = player.playerData.temperature / player.playerData.maxTemperature;
        healthBar.fillAmount = player.playerData.hp / player.playerData.maxHp;
        staminaBar.fillAmount = player.playerData.stamina / player.playerData.maxStamina;
    }
}
