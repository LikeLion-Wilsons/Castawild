using UnityEngine;

// 202 : 짱돌
// 401 : 방망이
// 402 : 횃불
// 403 : 돌 도끼
// 404 : 돌 작살
// 405 : 돌 곡괭이
public class ToolInfo : MonoBehaviour
{
    [SerializeField] private string toolName;
    [SerializeField] private int itemID;
    [Tooltip("제대로 호환되는 오브젝트 때렸을 시 공격력")]
    [SerializeField] private int interactAtt;
    [SerializeField] private float durability = 0.1f;

    public string ToolName => toolName;
    public int ItemID => itemID;
    public int Att => interactAtt;
    public float Durability => durability;
}