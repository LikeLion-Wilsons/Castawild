using Fusion;
using System.Net.Sockets;
using UnityEngine;

public class ThrowObject : NetworkBehaviour
{
    private Rigidbody rigid;

    public override void Spawned()
    {
        rigid = GetComponent<Rigidbody>();
        rigid.AddForce(GetComponent<Transform>().forward * 10f, ForceMode.Impulse);
    }
}