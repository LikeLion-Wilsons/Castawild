using Fusion;
using UnityEngine;

public enum ThrowType { stone, arrow }
public class ThrowObject : AttackObject
{
    public ThrowType throwType;
    private Rigidbody rigid;

    public override void Spawned()
    {
        rigid = GetComponent<Rigidbody>();
        rigid.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rigid.interpolation = RigidbodyInterpolation.Interpolate;
        canAttack = true;
    }

    public void AddForce(float force, float upForce, Vector3 targetPos)
    {
        Vector3 direction = (targetPos - transform.position).normalized;
        Vector3 forceDir = direction + Vector3.up * upForce;

        rigid.AddForce(forceDir.normalized * force, ForceMode.Impulse);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!HasStateAuthority)
            return;

        if (throwType == ThrowType.arrow)
        {
            RPC_NotifyFall();

            if (collision.gameObject.CompareTag("Player") /*&& collision.gameObject.CompareTag("Animal")*/)
            {
                NetworkObject networkObject = collision.gameObject.GetComponent<NetworkObject>();

                if (collision.gameObject.CompareTag("Player"))
                    Runner.Despawn(Object);
                else
                    transform.SetParent(networkObject.transform);
            }
        }

        // 사람이나 동물일 경우 canAttack false 처리는 그쪽에서
        if (!collision.gameObject.CompareTag("Player") /*&& !collision.gameObject.CompareTag("Animal")*/)
            canAttack = false;
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_NotifyFall()
    {
        rigid.linearVelocity = Vector3.zero;
        rigid.angularVelocity = Vector3.zero;
        rigid.isKinematic = true;

        GetComponent<Collider>().enabled = false;
        GetComponentInChildren<TrailRenderer>().enabled = false;
    }
}