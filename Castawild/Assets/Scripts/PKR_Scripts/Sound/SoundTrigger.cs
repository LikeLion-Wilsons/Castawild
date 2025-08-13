using Fusion;
using UnityEngine;

public class SoundTrigger : MonoBehaviour
{
    [SerializeField] private AudioSource source;

    private int triggerCount = 0;

    bool IsLocalPlayer(Collider other)
    {
        if (other.CompareTag("Player") == false) return false;

        var netObj = other.GetComponentInParent<NetworkObject>();
        if (netObj == null) return false;
        if (netObj.HasInputAuthority == false) return false;

        return true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (IsLocalPlayer(other) == false) return;
        triggerCount++;
        if (triggerCount > 0 && !source.isPlaying)
        {
            Debug.Log($"Sound Trigger Enter : {source.clip.name}");
            source.Play();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (IsLocalPlayer(other) == false) return;
        triggerCount--;
        if (triggerCount <= 0)
        {
            Debug.Log($"Sound Trigger Exit : {source.clip.name}");
            source.Stop();
            triggerCount = 0;
        }
    }
}