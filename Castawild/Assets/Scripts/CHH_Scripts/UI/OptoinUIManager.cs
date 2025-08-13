using UnityEngine;

public class OptionUIManager : MonoBehaviour
{
    [SerializeField] private Player player;
    [SerializeField] private OptionUI optionUI;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!player.HasInputAuthority)
                return;

            if (optionUI.IsOpen())
            {
                player.RPC_RequestSetUIOpen(false);
                optionUI.Close();
            }
            else
            {
                player.RPC_RequestSetUIOpen(true);
                optionUI.Open();
            }
        }
    }
}