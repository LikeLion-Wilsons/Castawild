using UnityEngine;

public class TreeDebugTool : MonoBehaviour
{
    [Header("디버그 설정")]
    [Tooltip("한 번 키를 누를 때 줄 데미지 양")]
    public float damageAmount = 25f;
    [Tooltip("카메라에서 나무를 찾을 최대 거리")]
    public float raycastDistance = 100f;
    [Tooltip("나무 오브젝트가 위치한 레이어 (필수 설정)")]
    public LayerMask treeLayer;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            TryDamageTree();
        }
    }

    void TryDamageTree()
    {
        // 메인 카메라에서 중앙에 ray
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, raycastDistance, treeLayer))
        {
            TreeHealth treeHealth = hit.collider.GetComponentInParent<TreeHealth>();
            if (treeHealth != null)
            {
                treeHealth.TakeDamage(damageAmount);
                Debug.Log($"[개발자 도구] 나무 '{treeHealth.gameObject.name}'에 데미지 적용. 현재 체력: {treeHealth.CurrentHealth}");
            }
            else
            {
                Debug.Log("[개발자 도구] Ray에 맞았지만, TreeHealth 컴포넌트를 찾을 수 없습니다.");
            }
        }
        else
        {
            Debug.Log("[개발자 도구] Raycast 거리 내에 나무를 찾을 수 없습니다.");
        }
    }
}
