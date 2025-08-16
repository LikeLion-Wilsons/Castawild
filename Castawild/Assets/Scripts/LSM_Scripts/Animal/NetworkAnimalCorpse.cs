using Fusion;
using Test;
using UnityEngine; 

public class NetworkAnimalCorpse : EnvironmentObject
{
    [SerializeField] private AnimalItem lootTable; // 동물 아이템 드랍 테이블  
    private CwAnimal animalObject; 

    public override void Spawned()
    {
        base.Spawned();
        animalObject = GetComponentInParent<CwAnimal>();
    }

    public override void Init(SpawnableDefinition def, int instanceId)
    {
        base.Init(def, instanceId);
    }

    public override void Interact(PlayerRef player, int att)
    {
        // rabbitCopse가 비활성화 상태면 리턴
        if (!animalObject.IsDead) return;        
        Debug.Log($"Interact with {animalObject.name} by {player.PlayerId}");
        RPC_RequestLoot(player);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_RequestLoot(PlayerRef player)
    {
        if (!animalObject.IsDead) return;     
        GiveLootToPlayer(player);        
        Looted();
    } 
    private void GiveLootToPlayer(PlayerRef player)
    {
        if (Runner.TryGetPlayerObject(player, out var playerObj))
        {
            playerObj = Runner.GetPlayerObject(player);
            Player _player = playerObj.GetComponent<Player>();
            InventoryDataManager inventoryData = _player.GetComponent<InventoryDataManager>();
            var lootResults = lootTable.GetLoot(); 
            foreach (var loot in lootResults)
            {
                inventoryData.AddItem(loot.itemId, loot.amount);
            } 

            Debug.Log($"Looted {lootResults.Count} items from {animalObject.name} by {player.PlayerId}");
        }
    } 
    protected void Looted()
    { 
        animalObject.IsDead = false; // 시체 상태로 변경   
        animalObject.AnimalCopse.SetActive(false); // 시체 비활성화       
    }

    public override void Revive()
    {
        base.Revive(); 
    }
}
 