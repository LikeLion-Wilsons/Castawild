using UnityEngine;

public class CampFireObject : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        other.GetComponent<Player>().RPC_RequestSetNearFire(1f);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        other.GetComponent<Player>().RPC_RequestSetNearFire(-1f);
    }
}