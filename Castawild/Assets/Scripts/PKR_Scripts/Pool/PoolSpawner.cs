using System.Collections.Generic;
using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Test
{
    public class PoolSpawner : NetworkBehaviour
    {
        [Header("[cube]")]
        [SerializeField] private NetworkObject cubePrefab;
        [SerializeField] private Button cubeSpawn;
        [SerializeField] private Button cubeDespawn;
        [SerializeField] private Button cubeClear;
        [SerializeField] private TMP_InputField cubeInput;
        
        [Header("[sphere]")]
        [SerializeField] private NetworkObject spherePrefab;
        [SerializeField] private Button sphereSpawn;
        [SerializeField] private Button sphereDespawn;
        [SerializeField] private Button sphereClear;
        [SerializeField] private TMP_InputField sphereInput;
        
        [Header("[etc]")]
        [SerializeField] private Button despawnAll;
        
        
        private HashSet<NetworkObject> _cubes = new HashSet<NetworkObject>();
        private HashSet<NetworkObject> _spheres = new HashSet<NetworkObject>();

        void Awake()
        {
            //cube
            cubeInput.onValueChanged.AddListener(OnCubeChanged);
            cubeClear.onClick.AddListener(CubeClear);
            cubeDespawn.onClick.AddListener(CubeDespawn);
            cubeSpawn.onClick.AddListener(CubeSpawn);
            
            //sphere
            sphereInput.onValueChanged.AddListener(OnSphereChanged);
            sphereClear.onClick.AddListener(SphereClear);
            sphereDespawn.onClick.AddListener(SphereDespawn);
            sphereSpawn.onClick.AddListener(SphereSpawn);
            
            //etc
            despawnAll.onClick.AddListener(DespawnAll);
        }

        private void DespawnAll()
        {
            foreach (var t in _cubes)
            {
                Runner.Despawn(t);
            }
            foreach (var t in _spheres)
            {
                Runner.Despawn(t);
            }
        }


        #region Sphere
        public void SphereSpawn()
        {
            if (HasStateAuthority == false) return;
            
            var obj = Runner.Spawn(spherePrefab, transform.position + Random.insideUnitSphere * 3);
            _spheres.Add(obj);
        }
        private void SphereDespawn()
        {
            if (HasStateAuthority == false) return;
            foreach (var t in _spheres)
            {
                Runner.Despawn(t);
            }
            _spheres.Clear();
        }
        public void SphereClear()
        {
            if (HasStateAuthority == false) return;
            Runner.ClearPool(spherePrefab.name);
        }
        private void OnSphereChanged(string value)
        {
            int.TryParse(value, out int count);
            Debug.Log($"OnSphereChanged:{count}");
            Runner.SetMaxPool(spherePrefab.name,count);
        }

        #endregion
        #region Cube
        public void CubeSpawn()
        {
            if (HasStateAuthority == false) return;
            var obj = Runner.Spawn(cubePrefab, transform.position + Random.insideUnitSphere * 3);
            _cubes.Add(obj);
        }
        
        public void CubeDespawn()
        {
            if (HasStateAuthority == false) return;
            foreach (var t in _cubes)
            {
                Runner.Despawn(t);
            }
            _cubes.Clear();
        }
        public void CubeClear()
        {
            if (HasStateAuthority == false) return;
            Runner.ClearPool(cubePrefab.name);
        }
        private void OnCubeChanged(string value)
        {
            int.TryParse(value, out int count);
            Debug.Log($"OnCubeChanged:{count}");
            
            Runner.SetMaxPool(cubePrefab.name,count);
        }
        #endregion

    }
}