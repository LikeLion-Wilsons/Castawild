using Fusion;
using UnityEngine;

public interface ISpawnable
{
    int InstanceId { get; set; }
    void Init(SpawnableDefinition definition, int instanceId);
}

public interface IRevivable//나중에 파일분리
{
    void Revive();
    bool CanRevive();
}
