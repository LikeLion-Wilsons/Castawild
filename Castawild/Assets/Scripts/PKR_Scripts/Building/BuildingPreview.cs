using TMPro;
using UnityEngine;

public class BuildingPreview : MonoBehaviour
{
    [SerializeField] private Color validColor;
    [SerializeField] private Color invalidColor;
    [SerializeField] private LayerMask groundLayerMask = 1; // 지면 레이어 마스크
    [SerializeField] private LayerMask obstacleLayerMask = 1; // 장애물 레이어 마스크
    [SerializeField] private float maxSlopeAngle = 70f;
    [SerializeField] private float gridSize = 1f;

    private GameObject previewObject;
    private Renderer[] previewRenderers;
    private Camera cam;
    private bool isPreviewing = false;
    private bool onAirPos = false;

    private bool onWallPos = false;

    // 저장된 bounds 정보
    private Bounds savedBounds;
    private Vector3 savedPosition;
    private Quaternion savedRotation;
    public bool IsBuildable => CheckBuildable();

    void Awake()
    {
        cam = Camera.main;
    }

    void Update()
    {
        if (!isPreviewing) return;
        //R키를 누르면 회전
        if (Input.GetKeyDown(KeyCode.R))
        {
            previewObject.transform.Rotate(0, 45, 0);
            savedRotation = previewObject.transform.rotation;
        }

        UpdatePreviewPosition();
        UpdatePreviewColor();
    }

    public void PreviewStart(GameObject prefab)
    {
        isPreviewing = true;
        previewObject = Instantiate(prefab);
        previewRenderers = previewObject.GetComponentsInChildren<Renderer>();

        // 미리보기 오브젝트의 콜라이더를 비활성화하여 자기 자신과 충돌하지 않도록 함
        SetupPreviewObject();
    }

    private void SetupPreviewObject()
    {
        if (previewObject == null) return;

        // 콜라이더 정보를 저장한 후 비활성화
        Collider previewCollider = previewObject.GetComponent<Collider>();
        if (previewCollider != null)
        {
            savedBounds = previewCollider.bounds;
        }

        // 모든 콜라이더를 비활성화
        Collider[] colliders = previewObject.GetComponentsInChildren<Collider>();
        foreach (Collider collider in colliders)
        {
            collider.enabled = false;
        }
    }

    public void PreviewStop()
    {
        isPreviewing = false;
        Destroy(previewObject);
    }

    private bool CheckBuildable()
    {
        if (onWallPos || onAirPos || previewObject == null) return false;

        // bounds의 중심점을 현재 위치로 업데이트하고 스케일 적용
        Vector3 currentCenter = previewObject.transform.position + savedBounds.center + Vector3.up * 0.1f;

        Vector3 adjustedExtents = savedBounds.extents;

        Collider[] obstacles = Physics.OverlapBox(
            currentCenter,
            adjustedExtents,
            previewObject.transform.rotation,
            obstacleLayerMask
        );

        return obstacles.Length == 0;
    }

    public void UpdatePreviewPosition()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit[] hits = Physics.RaycastAll(ray, 20f, groundLayerMask);

        RaycastHit? closestHit = null;
        float closestDistance = float.MaxValue;

        foreach (RaycastHit hit in hits)
        {
            // 미리보기 오브젝트와 충돌하지 않았는지 확인
            if (hit.collider.gameObject == previewObject)
                continue;

            // 가장 가까운 hit 찾기
            if (hit.distance < closestDistance)
            {
                closestDistance = hit.distance;
                closestHit = hit;
            }
        }

        Vector3 targetPosition;
        if (closestHit.HasValue)
        {
            //Vector3 targetPosition = closestHit.Value.point;
            targetPosition = closestHit.Value.point;
            Vector3 normal = closestHit.Value.normal;

            float slopeAngle = Vector3.Angle(Vector3.up, normal);
            if (slopeAngle <= maxSlopeAngle) // 지면으로 판단
            {
                onWallPos = false;
            }
            else // 벽면으로 판단
            {
                // bounds의 x 또는 z 중 더 큰 값의 절반만큼 벽면으로부터 떨어뜨림
                float offset = Mathf.Max(savedBounds.size.x, savedBounds.size.z) * 0.5f;
                targetPosition += normal * offset;
                onWallPos = true;//벽면에는 설치불가로 가정.
            }

            onAirPos = false;
        }

        else
        {
            targetPosition = ray.GetPoint(10f);
            //previewObject.transform.position = ray.GetPoint(10f);

            onAirPos = true;
        }

        targetPosition.x = Mathf.Round(targetPosition.x / gridSize) * gridSize;
        targetPosition.y = Mathf.Round(targetPosition.y / gridSize) * gridSize;
        targetPosition.z = Mathf.Round(targetPosition.z / gridSize) * gridSize;
        previewObject.transform.position = targetPosition;

        savedRotation = previewObject.transform.rotation;
        savedPosition = previewObject.transform.position;
    }

    public void UpdatePreviewColor()
    {
        bool canBuild = CheckBuildable();

        foreach (Renderer renderer in previewRenderers)
        {
            if (renderer == null)
                continue;

            Material[] materials = renderer.materials;
            for (int i = 0; i < materials.Length; i++)
            {
                materials[i].color = canBuild ? validColor : invalidColor;
            }

            renderer.materials = materials;
        }
    }

    public Vector3 GetPreviewPosition()
    {
        return savedPosition;
    }

    public Quaternion GetPreviewRotation()
    {
        return savedRotation;
    }
}