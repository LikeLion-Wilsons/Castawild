using Fusion;
using UnityEngine;

public interface ISpawnable
{
    int InstanceId { get; set; }
    GameObject VisualRoot { get; }  // 시각적 루트

    void Init(SpawnableDefinition definition, int instanceId);

    event System.Action<NetworkBehaviour> OnDied;
}

public interface IRevivable//나중에 파일분리
{
    void Revive();
    bool CanRevive();
}
