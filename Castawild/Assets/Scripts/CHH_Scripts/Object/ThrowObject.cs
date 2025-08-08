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

        if (!collision.gameObject.CompareTag("Player") /*&& collision.gameObject.CompareTag("Animal")*/)
            canAttack = false;
    }
}