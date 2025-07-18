using System;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

namespace Test
{
    public class PoolProvider : Fusion.Behaviour, INetworkObjectProvider
    {
        private Dictionary<NetworkPrefabId, Queue<NetworkObject>> _free = new Dictionary<NetworkPrefabId, Queue<NetworkObject>>();
        private Dictionary<NetworkPrefabId, string> _prefabName = new Dictionary<NetworkPrefabId, string>();
        private Dictionary<string, NetworkPrefabId> _prefabId = new Dictionary<string,NetworkPrefabId>();
        Dictionary<string, int> _maxPoolCount = new Dictionary<string, int>();//-1,0,N
        //-1: 기본값, 풀링안함
        //0 : 풀링 무한대.
        //N : N갯수만큼 무한대.

        #region Provider
        //NetworkRunner가 NetworkObject를 생성할 때 호출.(Runner.Spawn
        //기본구현은 Instantiate();
        public NetworkObjectAcquireResult AcquirePrefabInstance(NetworkRunner runner, in NetworkPrefabAcquireContext context, out NetworkObject instance)
        {
            instance = null;

            if (runner.SceneManager.IsBusy)
            {
                return NetworkObjectAcquireResult.Retry;
            }

            NetworkObject prefab;
            try
            {
                //runner의 프리팹테이블에서 로드.
                //프리팹테이블은 NetworkProjectConfig에서 [Rebuild Prefab Table] 클릭시, 등록됨.
                //Prefabs에는, 어드레서블, Resources 모두포함.
                //현재는 IsSynchronous동기방식만 지원. 비동기로드 향후지원예정.(어드레서블)
                prefab = runner.Prefabs.Load(context.PrefabId, isSynchronous: context.IsSynchronous);
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to load prefab: {ex}");
                return NetworkObjectAcquireResult.Failed;
            }

            if (!prefab)
            {
                return NetworkObjectAcquireResult.Retry;
            }

            _prefabName.TryAdd(context.PrefabId, prefab.name);
            _maxPoolCount.TryAdd(prefab.name, -1);
            _prefabId.TryAdd(prefab.name, context.PrefabId);

            //스폰.
            instance = InstantiatePrefab(runner, prefab, context.PrefabId);
            Assert.Check(instance);


            if (context.DontDestroyOnLoad)
            {
                runner.MakeDontDestroyOnLoad(instance.gameObject);
            }
            else
            {
                runner.MoveToRunnerScene(instance.gameObject);
            }

            //네트워크상에서 인스턴스 "갯수"관리목적. 풀링이랑 목적이 다름.
            runner.Prefabs.AddInstance(context.PrefabId);
            return NetworkObjectAcquireResult.Success;
        }

        //NetworkRunner가 NetworkObject를 제거할 때 호출.(Runner.Despawn
        //기본구현은 Destroy(gameObject);
        public void ReleaseInstance(NetworkRunner runner, in NetworkObjectReleaseContext context)
        {
            var instance = context.Object;

            //프리팹만 풀링.
            if (!context.IsBeingDestroyed)
            {
                if (context.TypeId.IsPrefab)
                {
                    DestroyPrefabInstance(runner, context.TypeId.AsPrefabId, instance);
                }
                else
                {
                    Destroy(instance.gameObject);
                }
            }

            if (context.TypeId.IsPrefab)
            {
                //네트워크상에서 인스턴스 갯수관리목적.
                runner.Prefabs.RemoveInstance(context.TypeId.AsPrefabId);
            }
        }

        public NetworkPrefabId GetPrefabId(NetworkRunner runner, NetworkObjectGuid prefabGuid)
        {
            return runner.Prefabs.GetId(prefabGuid);
        }
        #endregion

        
        #region Pooling Methods
        protected NetworkObject InstantiatePrefab(NetworkRunner runner, NetworkObject prefab, NetworkPrefabId contextPrefabId)
        {
            var result = default(NetworkObject);


            //풀링에 있으면 꺼내고, 없으면 생성.
            if (_free.TryGetValue(contextPrefabId, out var freeQ))
            {
                if (freeQ.Count > 0)
                {
                    result = freeQ.Dequeue();
                    result.transform.SetParent(null);
                    result.transform.position = Vector3.zero;
                    result.transform.rotation = Quaternion.identity;
                    result.transform.localScale = Vector3.one;
                    result.gameObject.SetActive(true);
                    return result;
                }
            }
            else
            {
                //id만 등록.
                _free.Add(contextPrefabId, new Queue<NetworkObject>());
               
            }

            result = Instantiate(prefab);

            return result;
        }

        protected void DestroyPrefabInstance(NetworkRunner runner, NetworkPrefabId prefabId, NetworkObject instance)
        {
            //get maxCount.
            string prefabName = _prefabName[prefabId];
            int maxCount = _maxPoolCount[prefabName];
            
            //풀링대상이 아니라면, Destroy.
            if (maxCount < 0)
            {
                Destroy(instance.gameObject);
                return;
            }
            
            //0이면, 무조건 풀링
            if (maxCount == 0)
            {
                _free[prefabId].Enqueue(instance);
                var go = instance.gameObject;
                go.SetActive(false);
                go.transform.SetParent(transform);
                go.transform.position = Vector3.zero;
                go.transform.rotation = Quaternion.identity;
                go.transform.localScale = Vector3.one;
                return;
            }
            

            //maxCount에 따라, 풀링 or Destroy.
            var q = _free[prefabId];
            if (maxCount > q.Count)
            {
                _free[prefabId].Enqueue(instance);
                var go = instance.gameObject;
                go.SetActive(false);
                go.transform.SetParent(transform);
                go.transform.position = Vector3.zero;
                go.transform.rotation = Quaternion.identity;
                go.transform.localScale = Vector3.one;
            }
            else
            {
                Destroy(instance.gameObject);
            }
        }

        #endregion
        
        #region Util
        public void SetMaxPool(string prefabName, int maxCount)
        {
            _maxPoolCount[prefabName] = maxCount;
        }

        public void ClearPool(string prefabName)
        {
            if (_prefabId.TryGetValue(prefabName, out NetworkPrefabId id) == false) return;
            if (_free.TryGetValue(id, out Queue<NetworkObject> q) == false) return;
            while (q.Count > 0)
            {
                NetworkObject t = q.Dequeue();
                t.gameObject.SetActive(false);
                Destroy(t.gameObject);
            }
        }
        public void AllClearPool()
        {
            foreach (var q in _free.Values)
            {
                while (q.Count > 0)
                {
                    NetworkObject t = q.Dequeue();
                    Destroy(t.gameObject);
                }
            }
        }
        #endregion
    }

    public static class NetworkRunnerExtensions
    {
        public static void SetMaxPool(this NetworkRunner runner, string prefabName, int maxPoolCount = 10)
        {
            var provider = runner.GetComponent<PoolProvider>();
            if (provider == null) return;

            provider.SetMaxPool(prefabName, maxPoolCount);
        }
        public static void ClearPool(this NetworkRunner runner, string prefabName)
        {
            var provider = runner.GetComponent<PoolProvider>();
            if (provider == null) return;

            provider.ClearPool(prefabName);
        }
        public static void AllClearPool(this NetworkRunner runner)
        {
            var provider = runner.GetComponent<PoolProvider>();
            if (provider == null) return;

            provider.AllClearPool();
        }
    }
}