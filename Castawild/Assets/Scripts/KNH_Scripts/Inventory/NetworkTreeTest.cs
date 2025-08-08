using Fusion;
using Test;
using UnityEngine;

public class NetworkTreeTest : NetworkBehaviour, IInteractable
{
    [Networked, OnChangedRender(nameof(OnChangedHealth))]
    public int Health { get; set; }

    [Networked] private TickTimer reviveTimer { get; set; }
    [SerializeField] private GameObject tree3;
    [SerializeField] private GameObject tree2;
    [SerializeField] private GameObject tree1;
    public bool IsAlive => Health > 0;
    private int maxHP;
    public void Init(int initHp)
    {
        Health = maxHP = initHp;
    }



    public override void Spawned()
    {
        Refresh();
    }

    public bool CanInteract()
    {
        return IsAlive;
    }

    public void Interact(PlayerRef player)
    {
        RPC_Request(player);
    }

    //클라->서버
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    void RPC_Request(PlayerRef player)
    {
        Health -= 10;
        if (Health <= 0)
        {
            //막타 플레이어에게 아이템지급.
            var playerObj = Runner.GetPlayerObject(player);
            var inven = playerObj.GetComponent<InventoryDataManager>();
            inven.AddItem(0, 1);
            reviveTimer = TickTimer.CreateFromSeconds(Runner, 2f);
        }
    }

    public bool CanRevive()
    {
        return IsAlive == false && reviveTimer.ExpiredOrNotRunning(Runner);
    }

    public void Revive()
    {
        Health = maxHP;
    }

    void OnChangedHealth()
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

