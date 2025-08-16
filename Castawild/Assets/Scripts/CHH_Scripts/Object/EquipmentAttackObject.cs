using UnityEditor.EditorTools;
using UnityEngine;

public class EquipmentAttackObject : AttackObject
{
    private Player player;
    int punchSoundIndex = 0;

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

        Player otherPlayer = other.GetComponentInParent<Player>();
        if (otherPlayer != null)
        {
            other.transform.parent.GetComponent<Player>().Host_TakeDamaged(true, Att);
            other.transform.parent.GetComponent<ToolStateManager>().DecreaseToolDuration = true;

            player.GetComponent<PlayerInteractManager>().RPC_ApplyHitInvoke(Att);

            PlayAttackSound(player);
        }

        else if (other.TryGetComponent<CwAnimal>(out CwAnimal animal))
        {
            animal.TakeDamage(Att);

            player.GetComponent<PlayerInteractManager>().RPC_ApplyHitInvoke(Att);

            PlayAttackSound(player);
        }
    }

    private void PlayAttackSound(Player player)
    {
        if (att == 0f)
        {
            Sound[] punchSound = { Sound.Player_Punch1, Sound.Player_Punch2 };
            SoundManager.Instance.PlayGlobalSound3D(punchSound[punchSoundIndex], player.soundPosition.position);
            punchSoundIndex = (punchSoundIndex + 1) % 2;
        }
        else
            SoundManager.Instance.PlayGlobalSound3D(Sound.Player_Attack, player.soundPosition.position);
    }
}