using UnityEngine;

public class ThrowObject : AttackObject
{
    private Rigidbody rigid;

    public override void Spawned()
    {
        rigid = GetComponent<Rigidbody>();
        rigid.collisionDetectionMode = CollisionDetectionMode.Continuous;
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