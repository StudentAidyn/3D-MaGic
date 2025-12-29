using System.Collections.Generic;
using UnityEngine;

public class MapMeshCombiner
{
    public MapMeshCombiner()
    {

    }

    public void Init() { }

    public void CombineMeshes(ref List<GameObject> local_map_objects)
    {
        List<MeshFilter> meshFilters = new List<MeshFilter>();
        foreach (GameObject go in local_map_objects)
        {
            MeshFilter mf = go.GetComponent<MeshFilter>();
            if (mf != null) meshFilters.Add(mf);
            
            MeshFilter[] mfc = go.GetComponentsInChildren<MeshFilter>();
            foreach (MeshFilter filter in mfc)
            {
                meshFilters.Add(filter);
            }
        }

        CombineInstance[] instances = new CombineInstance[meshFilters.Count];

        for (int i = 0; i < meshFilters.Count; i++)
        {
            var meshFilter = meshFilters[i];

            instances[i] = new CombineInstance
            {
                mesh = meshFilter.sharedMesh,
                transform = meshFilter.transform.localToWorldMatrix,
            };

            meshFilter.gameObject.SetActive(false);
        }

        Mesh combinedMesh = new Mesh();
        combinedMesh.CombineMeshes(instances);

        GameObject combinedMeshObject = new GameObject("combinedMeshObject");
        MeshFilter meshFilterObject = combinedMeshObject.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = combinedMeshObject.AddComponent<MeshRenderer>();

        meshFilterObject.sharedMesh = combinedMesh;

        combinedMeshObject.SetActive(true);
    }
}
