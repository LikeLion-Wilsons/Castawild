using UnityEngine;

public class CampFireObject : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Player player = other.GetComponentInParent<Player>();
        if (player == null)
            return;

        player.RPC_RequestSetNearFire(1f);
    }

    private void OnTriggerExit(Collider other)
    {
        Player player = other.GetComponentInParent<Player>();
        if (player == null)
            return;

        player.RPC_RequestSetNearFire(-1f);
    }
}