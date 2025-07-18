using UnityEngine;

[CreateAssetMenu(fileName = "AnimalData", menuName = "ScriptableObjects/AnimalData", order = 3)]
public class AnimalData : CharacterData
{
    public float age;               // 나이 (추후 가축 시스템 생기면 활용가능성 있음
    public float size;              // 크기 (부산물 생성 시 활용 가능성 있음)
    public float weight;            // 무게 (부산물 생성 시 활용 가능성 있음)
    public float detectionRadius;    // 감지 거리
    public float fleeThreshold;      // 체력이 몇 % 이하일 때 도망?
    public float wanderInterval;     // 유휴 상태 이동 주기 
    public float attackRange;        // 공격 범위
    public float attackCooldown;     // 공격 쿨타임 
    public bool isAggressive;        // 선공 여부
    public bool isFleeing;           // 도망 여부
    public bool canBeHarvested;     // 죽은 후 해체 가능 여부
    public enum SpawnType
    { 
        beach, forest, river, mountain
    }
}
