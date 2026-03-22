using System.Collections.Generic;
using UnityEngine;

public class MapMeshCombiner
{
    public MapMeshCombiner()
    {

    }

    public void Init() { }

    // Combines Meshes of a SINGLE Game Object parent with their children
    public void CombineMeshes(GameObject gameObject)
    {
        Dictionary<Material, List<MeshFilter>> meshFilters = new Dictionary<Material, List<MeshFilter>>();

        CollectMeshFiltersPerGameObject(ref meshFilters, gameObject);

        GenerateGameObjectsFromDictionary(meshFilters);
    }

    // Combines Meshes of a MULTIPLE Game Object parents with their children

    //IA:
    /*
     Instead of generating 1 single mesh, to allow for multiple meshes, each mesh that it finds will 
    be added to a list of meshes, and each time one gets found it is added to the main list.

    Unity's mesh renderer has a maximum upper limit of tris to renderer, if that limit is reached then it renders
    nothing making this process useless. A way to count and limit the tri count to ensure unity doesn't crash could 
    work.

    Mesh.vertexCount <= gets the current vertex count

     */
    public void CombineMeshes(ref List<GameObject> local_map_objects)
    {

        Dictionary<Material, List<MeshFilter>> meshFilters = new Dictionary<Material, List<MeshFilter>>();
        foreach (GameObject go in local_map_objects)
        {
            CollectMeshFiltersPerGameObject(ref meshFilters, go);
        }

        GenerateGameObjectsFromDictionary(meshFilters);
    }

    private void CollectMeshFiltersPerGameObject(ref Dictionary<Material, List<MeshFilter>> meshFilters, GameObject gameObject)
    {
        AddToMeshFilter(ref meshFilters, gameObject);

        Transform[] childrenTransforms = gameObject.GetComponentsInChildren<Transform>();
        foreach (Transform childTransform in childrenTransforms)
        {
            AddToMeshFilter(ref meshFilters, childTransform.gameObject);
        }
    }
    private void AddToMeshFilter(ref Dictionary<Material, List<MeshFilter>> meshFilters, GameObject gameObject)
    {
        MeshFilter mf = gameObject.GetComponent<MeshFilter>();
        MeshRenderer mr = gameObject.GetComponent<MeshRenderer>();

        if (mr != null)
        {
            if (!meshFilters.ContainsKey(mr.sharedMaterial))
            {
                List<MeshFilter> materialMeshFilters = new List<MeshFilter>();
                meshFilters.Add(mr.sharedMaterial, materialMeshFilters);
            }

            List<MeshFilter> outMeshFilters = null;
            if (mf != null) meshFilters.TryGetValue(mr.sharedMaterial, out outMeshFilters);
            if (outMeshFilters != null) outMeshFilters.Add(mf);
        }

    }

    private void GenerateGameObjectsFromDictionary(Dictionary<Material, List<MeshFilter>> meshFilters)
    {
        foreach(Material mat in meshFilters.Keys)
        {
            List<MeshFilter> listMeshFilter = meshFilters[mat];
            Debug.Log(mat.name + " | " + listMeshFilter.Count);
            string materialName = mat.name;
            GameObject meshFilterObject = GenerateGameObjectFromMeshFilters(listMeshFilter, materialName);
            MeshRenderer meshRenderer = meshFilterObject.AddComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = mat;
        }
    }

    private GameObject GenerateGameObjectFromMeshFilters(List<MeshFilter> meshFilters, string name = "")
    {
        string meshObjectName = name + "_CombinedMeshObject";
        GameObject meshObject = new GameObject(meshObjectName);

        MeshFilter meshObjectMeshFilter = meshObject.AddComponent<MeshFilter>();
        
        Mesh combinedMesh = new Mesh();
        combinedMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        combinedMesh = CombineMeshFilters(meshFilters);

        meshObjectMeshFilter.sharedMesh = combinedMesh;

        return meshObject;
    }

    private Mesh CombineMeshFilters(List<MeshFilter> meshFilters)
    {
        CombineInstance[] instances = new CombineInstance[meshFilters.Count];

        for (int i = 0; i < meshFilters.Count; i++)
        {
            var meshFilter = meshFilters[i];

            instances[i] = new CombineInstance
            {
                mesh = meshFilter.sharedMesh,
                transform = meshFilter.transform.localToWorldMatrix,
            };

            //meshFilter.gameObject.SetActive(false);
        }


        Mesh combinedMesh = new Mesh();
        combinedMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        combinedMesh.CombineMeshes(instances);

        return combinedMesh;
    }

}
