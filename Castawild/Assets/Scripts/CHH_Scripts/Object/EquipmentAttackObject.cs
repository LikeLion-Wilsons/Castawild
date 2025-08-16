using UnityEngine;

public class EquipmentAttackObject : AttackObject
{
    private Player player;

    public int Att
    {
        get
        {
            int totalAtt = player.playerData.attack + att;
            return totalAtt;
        }
    }

    private void Awake()
    {
        player = GetComponentInParent<Player>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!HasStateAuthority)
            return;

        Player player = other.GetComponentInParent<Player>();
        if (player != null)
        {
            other.transform.parent.GetComponent<Player>().Host_TakeDamaged(true, Att);
            other.transform.parent.GetComponent<ToolStateManager>().DecreaseToolDuration = true;

            player.GetComponent<PlayerInteractManager>().RPC_ApplyHitInvoke(Att);
        }

        else if (other.TryGetComponent<CwAnimal>(out CwAnimal animal))
        {
            animal.TakeDamage(Att);
        }
    }
}