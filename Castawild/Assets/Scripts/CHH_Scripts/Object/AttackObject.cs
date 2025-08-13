using Fusion;
using UnityEngine;

public class AttackObject : NetworkBehaviour
{
    [Tooltip("플레이어 공격 포함 안한 도구 자체의 공격력")]
    [SerializeField] private int att;
    public Player player;

    private void Awake()
    {
        player = GetComponentInParent<Player>();
    }

    public int Att
    {
        get
        {
            int totalAtt = player.playerData.attack + att;
            return totalAtt;
        }
    }
}
