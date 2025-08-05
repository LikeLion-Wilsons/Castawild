using Fusion;
using UnityEngine;

public class Stone_Throwable : NetworkBehaviour
{
    private Rigidbody rigid;

    public override void Spawned()
    {
        rigid = GetComponent<Rigidbody>();
        rigid.AddForce(GetComponent<Transform>().forward * 30f, ForceMode.Impulse);
    }
}