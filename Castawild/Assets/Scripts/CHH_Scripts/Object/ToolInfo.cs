using UnityEngine;

// 400 : 짱돌
// 401 : 방망이
// 402 : 횃불
// 403 : 돌 도끼
// 404 : 돌 작살
// 405 : 돌 곡괭이
public class ToolInfo : MonoBehaviour
{
    [SerializeField] private string toolName;
    [SerializeField] private int itemID;
    [SerializeField] private int att;
    public string ToolName => toolName;
    public int ItemID => itemID;
    public int Att => att;
}