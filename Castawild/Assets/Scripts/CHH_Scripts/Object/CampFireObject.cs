using System.Collections.Generic;
using UnityEngine;

public class CampFireObject : MonoBehaviour
{
    List<Player> warmPlayer = new List<Player>();

    private void OnTriggerEnter(Collider other)
    {
        Player player = other.GetComponentInParent<Player>();
        if (player == null)
            return;

        warmPlayer.Add(player);
        player.RPC_RequestSetNearFire(1f);
    }

    private void OnTriggerExit(Collider other)
    {
        Player player = other.GetComponentInParent<Player>();
        if (player == null)
            return;

        warmPlayer.Remove(player);
        player.RPC_RequestSetNearFire(-1f);
    }

    public void FinishFire()
    {
        foreach (Player player in warmPlayer)
            player.RPC_RequestSetNearFire(-1f);

        warmPlayer.Clear();
        gameObject.SetActive(false);
    }
}