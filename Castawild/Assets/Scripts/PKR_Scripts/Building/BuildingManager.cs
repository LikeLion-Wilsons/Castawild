using Fusion;
// using UnityEditor.PackageManager.Requests;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BuildingManager : NetworkBehaviour
{
    private InventoryDataManager inventory;
    [SerializeField] private BuildingPreview buildingPreview;
    [SerializeField] private NetworkObject campFireCol;
    private NetworkObject networkPrefab;
    private bool isPreviewing = false;
    [Networked] private int CurrentItemId { get; set; } = -1;

    void Start()
    {
        InventoryDataManager.onItemSelected += OnItemSelected;
        inventory = GetComponent<InventoryDataManager>();
    }

    void OnDestroy()
    {
        InventoryDataManager.onItemSelected -= OnItemSelected;
    }

    private void OnItemSelected(int itemID)
    {
        if (HasInputAuthority == false) return;
        CurrentItemId = itemID;

        if (isPreviewing) PreviewStop();

        if (itemID < 300 || itemID >= 400)
            return;

        GameObject previewPrefab = ItemDataBase.Instance.GetItemByID(itemID).buildPreviewPrefab;
        RPC_SetNetworkPrefab(itemID);

        PreviewStart(previewPrefab);
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_SetNetworkPrefab(int itemID) => networkPrefab = ItemDataBase.Instance.GetItemByID(itemID).buildPrefab.GetComponent<NetworkObject>();

    void Update()
    {
        if (HasInputAuthority == false) return;
        if (!isPreviewing) return;

        HandleBuildingInput();
    }

    public void PreviewStart(GameObject prefab)
    {
        isPreviewing = true;
        buildingPreview.enabled = true;
        buildingPreview.PreviewStart(prefab);
    }

    public void PreviewStop()
    {
        isPreviewing = false;
        buildingPreview.PreviewStop();
        buildingPreview.enabled = false;
    }

    private void HandleBuildingInput()
    {
        // 왼쪽 클릭으로 건설
        if (Input.GetMouseButtonDown(0))
        {
            TryBuild();
        }

        // 오른쪽 클릭으로 미리보기 중지
        if (Input.GetMouseButtonDown(1))
        {
            PreviewStop();
        }
    }

    private void TryBuild()
    {
        if (buildingPreview.IsBuildable)
        {
            var pos = buildingPreview.GetPreviewPosition();
            var rot = buildingPreview.GetPreviewRotation();
            Debug.Log("Building at position: " + pos + ", rotation: " + rot);
            RPCRequestBuild(pos, rot);
            PreviewStop();
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    void RPCRequestBuild(Vector3 pos, Quaternion rot)
    {
        var retryPos = buildingPreview.GetPreviewPosition();
        var retryRot = buildingPreview.GetPreviewRotation();
        Debug.Log($" pos:{pos},retryPos:{retryPos}, rot:{rot},retryRot:{retryRot}");

        Runner.Spawn(networkPrefab, pos, rot);
        if (CurrentItemId == 301)
            Runner.Spawn(campFireCol, pos, rot);

        inventory.UseItem(CurrentItemId, 1);
    }
}