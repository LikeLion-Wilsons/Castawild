using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AnimalLootData", menuName = "ScriptableObjects/AnimalLootData", order = 1)]
public class AnimalItem : ScriptableObject
{
    [Serializable]
    public class LootEntry
    {
        public int itemId;                     // 아이템 고유 ID
        public int minAmount = 1;               // 최소 갯수
        public int maxAmount = 1;               // 최대 갯수
        [Range(0f, 1f)]
        public float dropChance = 1f;           // 드랍 확률 (0~1)
    }

    [SerializeField]
    private List<LootEntry> lootEntries = new List<LootEntry>();

    /// <summary>
    /// 드랍 결과를 랜덤 계산해서 반환
    /// </summary>
    public List<LootResult> GetLoot()
    {
        List<LootResult> results = new List<LootResult>();

        foreach (var entry in lootEntries)
        {
            if (UnityEngine.Random.value <= entry.dropChance)
            {
                int amount = UnityEngine.Random.Range(entry.minAmount, entry.maxAmount + 1);
                results.Add(new LootResult(entry.itemId, amount));
            }
        }

        return results;
    }
}

[Serializable]
public class LootResult
{
    public int itemId;
    public int amount;

    public LootResult(int id, int amount)
    {
        this.itemId = id;
        this.amount = amount;
    }
}
