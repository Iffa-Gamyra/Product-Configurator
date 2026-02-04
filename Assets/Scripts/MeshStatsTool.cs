using UnityEngine;
using UnityEditor;

public static class MeshStatsTool
{
    [MenuItem("Tools/Mesh Stats/Print Selected (Tris & Verts)")]
    public static void PrintSelected()
    {
        var go = Selection.activeGameObject;
        if (go == null)
        {
            Debug.Log("Select a GameObject in the Hierarchy (or open a prefab and select its root).");
            return;
        }

        long tris = 0;
        long verts = 0;

        // MeshFilter meshes
        var filters = go.GetComponentsInChildren<MeshFilter>(true);
        foreach (var mf in filters)
        {
            var mesh = mf.sharedMesh;
            if (mesh == null) continue;
            verts += mesh.vertexCount;
            tris += mesh.triangles.Length / 3;
        }

        // Skinned meshes (if any)
        var skinned = go.GetComponentsInChildren<SkinnedMeshRenderer>(true);
        foreach (var smr in skinned)
        {
            var mesh = smr.sharedMesh;
            if (mesh == null) continue;
            verts += mesh.vertexCount;
            tris += mesh.triangles.Length / 3;
        }

        Debug.Log($"Mesh Stats for '{go.name}':  Verts = {verts:N0},  Tris = {tris:N0}");
    }
}
