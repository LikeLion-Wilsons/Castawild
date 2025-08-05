using Fusion;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BuildingManager : MonoBehaviour
{
    [SerializeField] private BuildingPreview buildingPreview;
    [SerializeField] private GameObject previewPrefab;
    [SerializeField] private NetworkObject networkPrefab;
    private bool isPreviewing = false;

    void Start()
    {
        InventoryDataManager.onItemSelected += OnItemSelected;
    }

    void OnDestroy()
    {
        InventoryDataManager.onItemSelected -= OnItemSelected;
    }

    private void OnItemSelected(int itemID)
    {
        Debug.Log("Selected item ID: " + itemID);
        if (isPreviewing) PreviewStop();
        
        bool isBuildingItem = true; //todo: itemID -> 건설 아이템 판별.
        if (isBuildingItem == false) return;
        
        GameObject prefab = previewPrefab;//todo: itemID -> 건설 프리팹 구하기.
        PreviewStart(prefab);
    }

    void Update()
    {
        if (!isPreviewing) return;

        HandleBuildingInput();
    }

    public void PreviewStart(GameObject prefab)
    {
        isPreviewing = true;
        buildingPreview.PreviewStart(prefab);
    }

    public void PreviewStop()
    {
        isPreviewing = false;
        buildingPreview.PreviewStop();
    }

    private void HandleBuildingInput()
    {
        // 왼쪽 클릭으로 건설
        if (Input.GetMouseButtonDown(0))
        {
            TryBuild();
        }
    }

    private void TryBuild()
    {
        if (buildingPreview.IsBuildable())
        {
            // 실제 건설 오브젝트 생성
            NetworkObject prefab = networkPrefab;
            Vector3 pos = buildingPreview.GetPreviewPosition();
            Quaternion rot = buildingPreview.GetPreviewRotation();

            RequestBuild(prefab,pos,rot);
            PreviewStop();
        }
    }
    
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    void RequestBuild(NetworkObject prefab, Vector3 pos, Quaternion rot)
    {
        NetworkRunner runner = NetworkRunner.GetRunnerForScene(SceneManager.GetActiveScene());
        runner.Spawn(prefab, pos, rot);
    }
}