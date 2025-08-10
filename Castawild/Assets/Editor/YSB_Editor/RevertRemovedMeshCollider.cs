using UnityEngine;
using UnityEditor;

public class RevertRemovedMeshCollider : Editor
{
    [MenuItem("Tools/Convex 설정/선택한 오브젝트의 Removed MeshCollider 복구")]
    public static void RevertRemovedMeshColliders()
    {
        GameObject[] selectedObjects = Selection.gameObjects;

        if (selectedObjects.Length == 0)
        {
            Debug.LogWarning("GameObject를 선택하세요.");
            return;
        }

        foreach (GameObject go in selectedObjects)
        {
            GameObject prefabRoot = PrefabUtility.GetOutermostPrefabInstanceRoot(go);
            if (prefabRoot == null)
            {
                Debug.Log($"프리팹 인스턴스가 아님: {go.name}");
                continue;
            }

            // Removed된 MeshCollider가 있으면 복구
            var removedOverrides = PrefabUtility.GetRemovedComponents(go);
            foreach (var removed in removedOverrides)
            {
                if (removed.assetComponent is MeshCollider)
                {
                    PrefabUtility.RevertRemovedComponent(go, removed.assetComponent, InteractionMode.AutomatedAction);
                    Debug.Log($"복구됨: {go.name}의 MeshCollider");
                }
            }

            // 복구 후, MeshCollider가 2개 이상일 경우 1개만 남기고 삭제
            var meshColliders = go.GetComponents<MeshCollider>();
            if (meshColliders.Length > 1)
            {
                Debug.LogWarning($"{go.name}에 MeshCollider가 {meshColliders.Length}개 있음. 하나만 남기고 삭제합니다.");

                // 첫 번째만 남기고 나머지 삭제
                for (int i = 1; i < meshColliders.Length; i++)
                {
                    Object.DestroyImmediate(meshColliders[i], true);
                }
            }
        }

        Debug.Log("완료: Removed된 MeshCollider 복구 + 중복 제거 완료!");
    }
}
