using UnityEngine;

public class BuildingPreview : MonoBehaviour
{
    [SerializeField] private Color validColor;
    [SerializeField] private Color invalidColor;
    [SerializeField] private LayerMask groundLayerMask = 1; // 지면 레이어 마스크
    [SerializeField] private LayerMask obstacleLayerMask = 1; // 장애물 레이어 마스크

    private GameObject previewObject;
    private Renderer previewRenderer;
    private Camera playerCamera;
    private bool isActive = false;
    private Material validMaterial;
    private Material invalidMaterial;

    void Start()
    {
        // 카메라 찾기 개선
        playerCamera = Camera.main;
        if (playerCamera == null)
        {
            playerCamera = FindObjectOfType<Camera>();
        }

        if (playerCamera == null)
        {
            Debug.LogError("BuildingPreview: 카메라를 찾을 수 없습니다!");
            return;
        }

        Debug.Log($"BuildingPreview: 카메라 설정 완료 - {playerCamera.name}");

        validMaterial = CreateMaterial(validColor, 0.5f);
        invalidMaterial = CreateMaterial(invalidColor, 0.5f);
    }

    void Update()
    {
        if (!isActive || previewObject == null) return;

        UpdatePreviewPosition();
        UpdatePreviewColor();
    }

    public void PreviewStart(GameObject prefab)
    {
        if (previewObject != null)
        {
            Destroy(previewObject);
        }

        previewObject = Instantiate(prefab);
        previewRenderer = previewObject.GetComponent<Renderer>();

        SetupPreviewObject();

        isActive = true;

        Debug.Log($"BuildingPreview: 미리보기 시작 - {prefab.name}");
    }

    public void PreviewStop()
    {
        if (previewObject != null)
        {
            Destroy(previewObject);
            previewObject = null;
        }
        isActive = false;

        Debug.Log("BuildingPreview: 미리보기 종료");
    }

    public Vector3 GetPreviewPosition()
    {
        if (previewObject != null)
        {
            return previewObject.transform.position;
        }
        return Vector3.zero;
    }

    public bool IsBuildable()
    {
        return CheckBuildable();
    }

    private void UpdatePreviewPosition()
    {
        if (playerCamera == null)
        {
            Debug.LogWarning("BuildingPreview: 카메라가 null입니다!");
            return;
        }

        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        // 디버깅을 위한 레이캐스트 정보 출력
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, groundLayerMask))
        {
            previewObject.transform.position = hit.point;
            Debug.Log($"BuildingPreview: 마우스 위치 업데이트 - {hit.point}, 레이어: {LayerMask.LayerToName(hit.collider.gameObject.layer)}");
        }
        else
        {
            Debug.LogWarning($"BuildingPreview: 레이캐스트 실패 - groundLayerMask: {groundLayerMask.value}");
        }
    }

    private void UpdatePreviewColor()
    {
        bool canBuild = CheckBuildable();

        if (previewRenderer != null)
        {
            previewRenderer.material = canBuild ? validMaterial : invalidMaterial;
        }
    }

    private bool CheckBuildable()
    {
        if (previewObject == null) return false;

        // 미리보기 오브젝트의 콜라이더를 사용하여 충돌 검사
        Collider previewCollider = previewObject.GetComponent<Collider>();
        if (previewCollider == null) return true;

        // 현재 위치에서 장애물과의 충돌 검사
        Vector3 center = previewObject.transform.position;
        Vector3 size = previewCollider.bounds.size;

        Collider[] obstacles = Physics.OverlapBox(center, size * 0.5f, previewObject.transform.rotation, obstacleLayerMask);

        return obstacles.Length == 0;
    }

    private void SetupPreviewObject()
    {
        if (previewObject == null) return;

        // 모든 컴포넌트 비활성화 (렌더러 제외)
        MonoBehaviour[] scripts = previewObject.GetComponentsInChildren<MonoBehaviour>();
        foreach (MonoBehaviour script in scripts)
        {
            if (script != this)
            {
                script.enabled = false;
            }
        }

        // 콜라이더를 트리거로 변경
        Collider[] colliders = previewObject.GetComponentsInChildren<Collider>();
        foreach (Collider collider in colliders)
        {
            collider.isTrigger = true;
        }

        // 리지드바디 비활성화
        Rigidbody[] rigidbodies = previewObject.GetComponentsInChildren<Rigidbody>();
        foreach (Rigidbody rb in rigidbodies)
        {
            rb.isKinematic = true;
        }
    }

    private Material CreateMaterial(Color color, float alpha)
    {
        Material material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        material.SetFloat("_Mode", 3); // Transparent 모드
        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        material.SetInt("_ZWrite", 0);
        material.DisableKeyword("_ALPHATEST_ON");
        material.EnableKeyword("_ALPHABLEND_ON");
        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        material.renderQueue = 3000;

        color.a = alpha;
        material.color = color;

        return material;
    }

    public Quaternion GetPreviewRotation()
    {
        if (previewObject != null)
        {
            return previewObject.transform.rotation;
        }
        return Quaternion.identity;
    }
}