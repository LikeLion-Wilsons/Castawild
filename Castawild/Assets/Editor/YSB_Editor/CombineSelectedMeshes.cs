using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class CombineSelectedMeshes : EditorWindow
{
    [MenuItem("Tools/Combine Selected Meshes")]
    public static void ShowWindow()
    {
        GetWindow<CombineSelectedMeshes>("Combine Meshes");
    }

    private void OnGUI()
    {
        if (GUILayout.Button("Combine Selected Objects"))
        {
            CombineSelected();
        }
    }

    private static void CombineSelected()
    {
        GameObject[] selectedObjects = Selection.gameObjects;

        if (selectedObjects.Length == 0)
        {
            Debug.LogWarning("No objects selected!");
            return;
        }

        // 같은 Mesh 기준으로 그룹화
        Dictionary<Mesh, List<Transform>> meshGroups = new Dictionary<Mesh, List<Transform>>();

        foreach (var obj in selectedObjects)
        {
            MeshFilter mf = obj.GetComponent<MeshFilter>();
            MeshRenderer mr = obj.GetComponent<MeshRenderer>();

            if (mf == null || mr == null) continue;

            Mesh mesh = mf.sharedMesh;

            if (!meshGroups.ContainsKey(mesh))
                meshGroups[mesh] = new List<Transform>();

            meshGroups[mesh].Add(obj.transform);
        }

        // 각 그룹별로 Combine
        foreach (var group in meshGroups)
        {
            List<CombineInstance> combineInstances = new List<CombineInstance>();

            foreach (var t in group.Value)
            {
                MeshFilter mf = t.GetComponent<MeshFilter>();

                CombineInstance ci = new CombineInstance
                {
                    mesh = mf.sharedMesh,
                    transform = mf.transform.localToWorldMatrix
                };

                combineInstances.Add(ci);
            }

            Mesh combinedMesh = new Mesh();
            combinedMesh.name = group.Key.name + "_Combined";
            combinedMesh.CombineMeshes(combineInstances.ToArray(), true, true);

            // 새로운 GameObject 생성
            GameObject combinedObj = new GameObject(group.Key.name + "_Combined");
            combinedObj.transform.position = Vector3.zero;
            combinedObj.transform.rotation = Quaternion.identity;

            MeshFilter combinedMF = combinedObj.AddComponent<MeshFilter>();
            MeshRenderer combinedMR = combinedObj.AddComponent<MeshRenderer>();

            combinedMF.sharedMesh = combinedMesh;
            combinedMR.sharedMaterials = group.Value[0].GetComponent<MeshRenderer>().sharedMaterials;

            Debug.Log($"Created combined mesh: {combinedObj.name} with {group.Value.Count} objects");

            // 원본 오브젝트 비활성화
            foreach (var t in group.Value)
            {
                t.gameObject.SetActive(false);
            }
        }
    }
}
