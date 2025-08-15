using Fusion;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AnimalSpawner : NetworkBehaviour
{
    [SerializeField] private CwAnimal _AnimalPrefab;
    [SerializeField] private Transform[] points;
    [SerializeField] private List<CwAnimal> animals = new List<CwAnimal>(); 

    public override void Spawned()
    {
        for (int i = 0; i < points.Length; i++)
        {
            SpawnAnimal(points[i].position);
        } 
    }

    public override void FixedUpdateNetwork()
    {         

    }

    void ReSpawnAnimal(Vector3 spawnPos)
    {

    }


    /*
    void SpawnAnimal(Vector3 spawnPos)
    {
        Debug.Log($"SpawnAnimal: {spawnPos}");
        if (HasStateAuthority == false) return;
        var animal = Runner.Spawn(_AnimalPrefab, spawnPos, Quaternion.identity, null, (runner, o) =>
        {
            o.GetComponent<CwAnimal>().Init();
        }); 
        animals.Add(animal);
        Debug.Log(animal.transform.position); 
    }
    */
    void SpawnAnimal(Vector3 spawnPos)
    {
        if (!HasStateAuthority) return;

        var animal = Runner.Spawn(_AnimalPrefab, spawnPos, Quaternion.identity, null, (runner, o) =>
        {
            var agent = o.GetComponent<NavMeshAgent>();
            if (agent != null)
            {
                // 1) 잠시 끄기: transform과 agent 내부좌표 충돌 방지
                bool wasEnabled = agent.enabled;
                agent.enabled = false;

                // 2) NavMesh 상의 유효 지점으로 보정 (반경은 맵에 맞게 조절)
                Vector3 desired = o.transform.position;  // Runner.Spawn이 이미 원하는 위치를 넣어줌
                if (NavMesh.SamplePosition(desired, out var hit, 2.0f, agent.areaMask))
                    desired = hit.position;
                else
                    Debug.LogWarning($"[Spawn] NavMesh 근처를 찾지 못함: {desired}");

                // 3) 다시 켠 다음 Warp: 내부 nextPosition과 transform을 동시에 세팅
                agent.enabled = wasEnabled;
                if (agent.enabled)
                {
                    agent.Warp(desired);   // 내부좌표/Transform 동시 텔레포트
                    agent.ResetPath();     // 잔존 경로 제거(예상치 못한 이동 방지)
                    agent.velocity = Vector3.zero;
                }
                else
                {
                    o.transform.position = desired; // 그래도 꺼져있다면 transform만 세팅
                }

                // (선택) 스폰 직후엔 정지 상태로 두고 싶다면:
                // agent.isStopped = true;
            }

            // 동물 초기화는 위치 확정 후에
            o.GetComponent<CwAnimal>().Init();
        });

        animals.Add(animal);
    }

}

