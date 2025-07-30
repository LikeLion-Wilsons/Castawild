using Fusion;
using UnityEngine;

public class CHHNetworkTree : TestInteractable
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
        interactableType = InteractableType.Tree;
        Refresh();
    }

    public override bool CanInteract()
    {
        return IsAlive;
    }

    public override void Interact(PlayerRef player, int att)
    {
        RPC_Request(player, att);
    }

    //클라->서버
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    void RPC_Request(PlayerRef player, int att)
    {
        Health -= att;
        Debug.Log("Tree Health : " + Health);
        if (Health <= 0)
        {
            //막타 플레이어에게 아이템지급.
            var playerObj = Runner.GetPlayerObject(player);
            var inven = playerObj.GetComponent<Test.PlayerInventory>();
            inven.AddItem(1000, 1);
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
