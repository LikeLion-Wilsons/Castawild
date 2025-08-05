using UnityEngine;

public class BuildingPreview : MonoBehaviour
{
    [SerializeField] private Color validColor;
    [SerializeField] private Color invalidColor;
    [SerializeField] private LayerMask groundLayerMask = 1; // 지면 레이어 마스크
    [SerializeField] private LayerMask obstacleLayerMask = 1; // 장애물 레이어 마스크

    private GameObject previewObject;
    private Renderer previewRenderer;
    private Camera cam;
    private bool isPreviewing = false;
    private bool onAirPos = false;

    // 저장된 bounds 정보
    private Bounds savedBounds;
    private bool hasSavedBounds = false;
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
        previewRenderer = previewObject.GetComponent<Renderer>();

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
            hasSavedBounds = true;
        }
        else
        {
            hasSavedBounds = false;
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
        hasSavedBounds = false;
        Destroy(previewObject);
    }

    private bool CheckBuildable()
    {
        if (onAirPos || previewObject == null) return false;

        if (!hasSavedBounds) return true; // 저장된 bounds가 없으면 건설 가능

        // 저장된 bounds를 사용하여 충돌 검사
        // bounds의 중심점을 현재 위치로 업데이트
        Vector3 currentCenter = previewObject.transform.position + (savedBounds.center - previewObject.transform.position);

        // 약간의 여유 공간을 주어서 딱 맞는 경우에도 건설할 수 있도록 함
        Vector3 adjustedExtents = savedBounds.extents * 0.95f; // 5% 여유 공간

        Collider[] obstacles = Physics.OverlapBox(
            currentCenter,
            adjustedExtents,
            previewObject.transform.rotation,
            obstacleLayerMask
        );

        return obstacles.Length == 0;
    }

    private void UpdatePreviewPosition()
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

        if (closestHit.HasValue)
        {
            Vector3 targetPosition = closestHit.Value.point;
            Vector3 normal = closestHit.Value.normal;

            // 콜라이더의 크기를 고려하여 높이 조정
            if (hasSavedBounds)
            {
                // normal 벡터를 고려하여 건설물을 지면에 올바르게 배치
                float colliderHeight = savedBounds.size.y;

                // normal 벡터가 위쪽을 향하는지 확인 (지면인지 확인)
                if (normal.y > 0.5f) // 대략 30도 이내의 경사면
                {
                    // 지면에 수직으로 배치
                    targetPosition.y += colliderHeight * 0.5f;
                }
                else
                {
                    // 경사면에 맞춰서 배치
                    // normal 벡터 방향으로 콜라이더 높이의 절반만큼 이동
                    Vector3 offset = normal * (colliderHeight * 0.5f);
                    targetPosition += offset;
                }
            }

            previewObject.transform.position = targetPosition;
            onAirPos = false;
        }
        else
        {
            previewObject.transform.position = ray.GetPoint(10f);
            onAirPos = true;
        }
        
        savedPosition = previewObject.transform.position;
    }

    private void UpdatePreviewColor()
    {
        bool canBuild = CheckBuildable();
        previewRenderer.material.color = canBuild ? validColor : invalidColor;
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