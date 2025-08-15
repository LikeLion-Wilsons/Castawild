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
        if (other.CompareTag("Player"))
        {
            other.GetComponent<ToolStateManager>().DecreaseToolDuration = true;
            other.GetComponent<PlayerInteractManager>().RPC_ApplyHitInvoke(Att);
        }

        //else if (other.CompareTag("Animal"))
        //{
        //    other.GetComponent<CwAnimal>().TakeDamage(Att);
        //}
    }
}