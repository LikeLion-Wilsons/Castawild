using Fusion;
using UnityEngine;

public class ThrowObject : NetworkBehaviour
{
    private Vector3 targetPos;
    private Rigidbody rigid;

    public override void Spawned()
    {
        rigid = GetComponent<Rigidbody>();
        rigid.collisionDetectionMode = CollisionDetectionMode.Continuous;
    }

    //public void AddForce(Vector3 forward, float force)
    //{
    //    rigid.AddForce(forward * force, ForceMode.Impulse);
    //}

    public void AddForce(float force, float upForce, Vector3 targetPos)
    {
        Vector3 direction = (targetPos - transform.position).normalized;
        Vector3 forceDir = direction + Vector3.up * upForce;

        rigid.AddForce(forceDir.normalized * force, ForceMode.Impulse);
    }
}