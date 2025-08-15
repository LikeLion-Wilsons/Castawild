using Fusion;
using UnityEngine;

public enum ThrowType { stone, arrow }
public class ThrowObject : AttackObject
{
    public GameObject thrower;
    public ThrowType throwType;
    private Rigidbody rigid;

    public int Att => att;

    public override void Spawned()
    {
        rigid = GetComponent<Rigidbody>();
        rigid.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rigid.interpolation = RigidbodyInterpolation.Interpolate;
    }

    public void AddForce(float force, float upForce, Vector3 targetPos)
    {
        Vector3 direction = (targetPos - transform.position).normalized;
        Vector3 forceDir = direction + Vector3.up * upForce;

        rigid.AddForce(forceDir.normalized * force, ForceMode.Impulse);
    }

    private void OnCollisionEnter(Collision collision)
    {
        rigid.isKinematic = true;
        GetComponent<Collider>().enabled = false;
        GetComponentInChildren<TrailRenderer>().enabled = false;

        if (throwType == ThrowType.arrow)
        {
            rigid.linearVelocity = Vector3.zero;
            rigid.angularVelocity = Vector3.zero;
        }

        if (collision.gameObject.CompareTag("Player") /*&& collision.gameObject.CompareTag("Animal")*/)
        {
            NetworkObject networkObject = collision.gameObject.GetComponent<NetworkObject>();

            if (collision.gameObject.CompareTag("Player"))
            {
                Player player = networkObject.GetComponent<Player>();
                Runner.Despawn(Object);
                player.Host_TakeDamaged(true, Att);
                thrower.GetComponent<PlayerInteractManager>().RPC_ApplyHitInvoke(Att);
            }

            //else if (collision.gameObject.CompareTag("Animal"))
            //{
            //    collision.gameObject.GetComponent<CwAnimal>().TakeDamage(Att);

            //    if (throwType == ThrowType.arrow)
            //        transform.SetParent(networkObject.transform);
            //}
        }
    }
}