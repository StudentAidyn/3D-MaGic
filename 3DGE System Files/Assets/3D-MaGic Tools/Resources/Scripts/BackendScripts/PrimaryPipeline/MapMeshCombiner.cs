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

    public void CombineMeshes(ref List<GameObject> gameObjects)
    {
        Dictionary<Material, List<MeshFilter>> meshFilters = new Dictionary<Material, List<MeshFilter>>();
        foreach (GameObject gameObject in gameObjects)
        {
            CollectMeshFiltersPerGameObject(ref meshFilters, gameObject);
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
            string _materialName = mat.name;
            GameObject _meshFilterObject = GenerateGameObjectFromMeshFilters(listMeshFilter, _materialName);
            MeshRenderer _meshRenderer = _meshFilterObject.AddComponent<MeshRenderer>();
            _meshRenderer.sharedMaterial = mat;
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
