using UnityEngine;
using UnityEditor;
using System.IO;

public class VisualRootSetupTool
{
    [MenuItem("Tools/Setup Visual Root Controllers")]
    private static void SetupVisualRoots()
    {
        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab");

        int processed = 0;
        foreach (string guid in prefabGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            if (prefab == null) continue;

            // Visual Root 찾기
            Transform visualRoot = prefab.transform.Find("Visual Root");
            if (visualRoot == null) continue; // 없으면 스킵

            bool changed = false;

            // VisualRootController 부착
            if (visualRoot.GetComponent<VisualRootController>() == null)
            {
                visualRoot.gameObject.AddComponent<VisualRootController>();
                changed = true;
            }

            // EnvironmentObjects에 visualRoot 할당
            EnvironmentObject envObj = prefab.GetComponentInChildren<EnvironmentObject>(true);
            if (envObj != null)
            {
                var so = new SerializedObject(envObj);
                SerializedProperty visualRootProp = so.FindProperty("visualRoot");
                if (visualRootProp != null && visualRootProp.objectReferenceValue == null)
                {
                    visualRootProp.objectReferenceValue = visualRoot.gameObject;
                    so.ApplyModifiedProperties();
                    changed = true;
                }
            }

            if (changed)
            {
                EditorUtility.SetDirty(prefab);
                processed++;
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"Visual Root 세팅 완료. 변경된 Prefab: {processed}개");
    }
}
