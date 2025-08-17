using UnityEngine;

public enum ThrowType { stone, arrow }
public class ThrowObject : AttackObject
{
    public GameObject thrower;
    public ThrowType throwType;
    private Rigidbody rigid;
    private bool canAttack = true;

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
        if (!canAttack)
            return;

        GetComponentInChildren<TrailRenderer>().enabled = false;

        if (throwType == ThrowType.arrow)
        {
            rigid.isKinematic = true;
            rigid.linearVelocity = Vector3.zero;
            rigid.angularVelocity = Vector3.zero;
            GetComponent<Collider>().enabled = false;
        }

        if (collision.gameObject.CompareTag("Player"))
        {
            Player otherPlayer = collision.gameObject.GetComponent<Player>();
            int attack = Att - otherPlayer.Defense;

            otherPlayer.Host_TakeDamaged(true, attack);
            thrower.GetComponent<PlayerInteractManager>().RPC_ApplyHitInvoke(attack);

            if (throwType == ThrowType.arrow)
                Runner.Despawn(Object);
        }

        else if (collision.gameObject.TryGetComponent<CwAnimal>(out CwAnimal animal))
        {
            animal.TakeDamage(Att);

            if (throwType == ThrowType.arrow)
                transform.SetParent(animal.transform);

            thrower.GetComponent<PlayerInteractManager>().RPC_ApplyHitInvoke(Att);
        }

        canAttack = false;
    }
}