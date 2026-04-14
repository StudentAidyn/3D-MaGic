using System.Collections.Generic;
using UnityEngine;

public class MapMeshCombiner
{
    public MapMeshCombiner()
    {

    }

    #region PUBLIC-METHODS

    // Combines Meshes of a SINGLE Game Object parent with their children
    public List<GameObject> CombineMeshes(GameObject gameObject)
    {
        Dictionary<Material, List<MeshFilter>> meshFilters = new Dictionary<Material, List<MeshFilter>>();

        CollectMeshFiltersPerGameObject(ref meshFilters, gameObject);

        return GenerateGameObjectsFromDictionary(meshFilters);
    }

    // Combines Meshes of a MULTIPLE Game Object parents with their children

    public List<GameObject> CombineMeshes(ref List<GameObject> gameObjects)
    {
        Dictionary<Material, List<MeshFilter>> meshFilters = new Dictionary<Material, List<MeshFilter>>();
        foreach (GameObject gameObject in gameObjects)
        {
            CollectMeshFiltersPerGameObject(ref meshFilters, gameObject);
        }

        return GenerateGameObjectsFromDictionary(meshFilters);
    }

    #endregion

    #region PRIVATE-METHODS

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

    private List<GameObject> GenerateGameObjectsFromDictionary(Dictionary<Material, List<MeshFilter>> meshFilters)
    {
        List<GameObject> combinedObjects = new List<GameObject>();
        foreach(Material mat in meshFilters.Keys)
        {
            List<MeshFilter> listMeshFilter = meshFilters[mat];
            string materialName = mat.name;
            GameObject meshFilterObject = GenerateGameObjectFromMeshFilters(listMeshFilter, materialName);
            MeshRenderer meshRenderer = meshFilterObject.AddComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = mat;
            combinedObjects.Add(meshFilterObject);
        }
        return combinedObjects;
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
        }

        Mesh combinedMesh = new Mesh();
        combinedMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        combinedMesh.CombineMeshes(instances);

        return combinedMesh;
    }

    #endregion
}
