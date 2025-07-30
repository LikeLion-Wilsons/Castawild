using Fusion;
using Test;
using UnityEngine;

public class TestNetworkTree : NetworkBehaviour
{
    [Networked, OnChangedRender(nameof(OnChangedHealth))]
    public int Health { get; set; }
    public bool IsAlive => Health > 0;

    [Networked] private TickTimer reviveTimer { get; set; }

    [SerializeField] private GameObject tree3;
    [SerializeField] private GameObject tree2;
    [SerializeField] private GameObject tree1;
    private int maxHP;
    //  OnChangedRender(nameof(OnChangedHealth) : Health가 변경되면 OnChangedHealth 호출

    public bool CanInteract() => IsAlive;

    public void Init(int initHp)
    {
        Health = maxHP = initHp;
    }

    //클라->서버
    // 나무나 돌 캘 때 이 함수 호출
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_Request(PlayerRef player)
    {
        Health -= 10;
        if (Health <= 0)
        {
            //막타 플레이어에게 아이템지급.
            var playerObj = Runner.GetPlayerObject(player);
            var inven = playerObj.GetComponent<PlayerInventory>();
            inven.AddItem(1000, 1);
            reviveTimer = TickTimer.CreateFromSeconds(Runner, 2f);
        }
    }

    public override void Spawned()
    {
        Refresh();
    }

    public bool CanRevive()
    {
        return IsAlive == false && reviveTimer.ExpiredOrNotRunning(Runner);
    }

    public void Revive()
    {
        Health = maxHP;
    }

    public void OnChangedHealth()
    {
        Refresh();
    }

    void Refresh()
    {
        tree3.SetActive(Health >= 30);
        tree2.SetActive(Health >= 20);
        tree1.SetActive(Health >= 10);
    }
}