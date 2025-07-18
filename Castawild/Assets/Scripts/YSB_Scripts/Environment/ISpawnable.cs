using Fusion;
using UnityEngine;

public interface ISpawnable
{
    int InstanceId { get; set; }
    void Init(SpawnableDefinition definition, int instanceId);
    event System.Action<NetworkBehaviour> OnDied;
    void Revive();
    GameObject GetGameObject();
}
