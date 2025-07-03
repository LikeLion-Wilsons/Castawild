using UnityEngine;

[CreateAssetMenu(fileName = "CharacterData", menuName = "ScriptableObjects/CharacterData", order = 1)]
public class CharacterData : ScriptableObject
{    
    public string characterName; //엔티티 이름
    public float maxHp; //최대체력 
    public float armor; //방어력
    public float attack; //공격력 
    public float moveSpeed; //이동속도 
}

