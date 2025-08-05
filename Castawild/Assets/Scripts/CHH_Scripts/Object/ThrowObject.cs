using Fusion;
using UnityEngine;

public class ThrowObject : NetworkBehaviour
{
    private Rigidbody rigid;

    public override void Spawned()
    {
        rigid = GetComponent<Rigidbody>();
    }

    public void AddForce(Vector3 forward, float force)
    {
        rigid.AddForce(forward * force, ForceMode.Impulse);
    }
}