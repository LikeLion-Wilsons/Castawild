using UnityEngine;

[CreateAssetMenu(fileName = "AnimalData", menuName = "ScriptableObjects/AnimalData", order = 3)]
public class AnimalData : CharacterData
{
    public enum SpawnType
    {
        beach, forest, river, mountain
    }

    public float maxDetectionRadius;    // 최대 감지 거리
    public float minDetectionRadius;    // 최소 감지 거리
    public float attackRange;        // 공격 범위
    public float attackCooldown;     // 공격 쿨타임 
    public bool canBeHarvested;     // 죽은 후 해체 가능 여부
    public float alertTime;               // 경계 시간 
    public float idleRadius;      // Idle 이동 반경 
    public float escapeRadius;      // 도망 이동 반경  
    public float escapeSpeed;      // 도망 속도
    public SpawnType spawnType;
}