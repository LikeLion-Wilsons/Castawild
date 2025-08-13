using Fusion;
using UnityEngine;

// 202 : 짱돌
// 401 : 방망이
// 402 : 횃불
// 403 : 돌 도끼
// 404 : 돌 작살
// 405 : 돌 곡괭이
public class ToolInfo : MonoBehaviour
{
    [SerializeField] private int itemID;
    [SerializeField] private string toolName;
    [Tooltip("제대로 호환되는 오브젝트 때렸을 시 공격력")]
    [SerializeField] private int interactAtt;
    [SerializeField] private float durability = 0.1f;

    public string ToolName => toolName;
    public int ItemID => itemID;
    public int Att => interactAtt;
    public float Durability => durability;

    public ToolInfoData GetData() => new ToolInfoData(itemID, toolName, interactAtt, durability);
}

public struct ToolInfoData : INetworkStruct
{
    public int itemID;
    public NetworkString<_32> toolName;
    public int att;
    public float durability;

    public ToolInfoData(int itemID, NetworkString<_32> toolName, int att, float durability)
    {
        this.itemID = itemID;
        this.toolName = toolName;
        this.att = att;
        this.durability = durability;
    }

    public bool IsEmpty() => itemID == -1 || string.IsNullOrEmpty(toolName.ToString());
    public static ToolInfoData Empty => new ToolInfoData(-1, string.Empty, 0, 1f);
}