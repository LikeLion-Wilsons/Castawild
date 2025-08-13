using Fusion;
using UnityEngine;

public class SoundTrigger : MonoBehaviour
{
    private int triggerCount = 0;

    bool IsLocalPlayer(Collider other)
    {
        if (other.gameObject.layer != LayerMask.NameToLayer("Player")) return false;

        var netObj = other.GetComponentInParent<NetworkObject>();
        if (netObj == null) return false;
        if (netObj.HasInputAuthority == false) return false;

        return true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (IsLocalPlayer(other) == false) return;
        if (triggerCount <1)
        {
            Debug.Log($"Sound Trigger Enter");
            SoundManager.Instance.PlayBGM("Env_2");
        }
        triggerCount++;
    }

    private void OnTriggerExit(Collider other)
    {
        if (IsLocalPlayer(other) == false) return;
        triggerCount--;
        if (triggerCount <= 0)
        {
            Debug.Log($"Sound Trigger Exit");
            SoundManager.Instance.PlayBGM("의미없는텍스트");
            triggerCount = 0;
        }
    }
}