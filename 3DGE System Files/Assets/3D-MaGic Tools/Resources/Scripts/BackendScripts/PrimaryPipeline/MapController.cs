using System.Collections.Generic;
using UnityEngine;


public struct MapGenData
{
    public Vector3 Dimensions;
    public GenerationType Type;
    public GenerationStep Control;
    public ulong Seed;
    public MapGenData(Vector3 dimensions, GenerationType type, GenerationStep control, ulong seed)
    {
        Dimensions = dimensions;
        Control = control;
        Type = type;
        Seed = seed;
    }
}

public class MapController
{
    // Public - Classes
    public CellGenerator LocalCellGenerator;
    public MapGenerator LocalMapGenerator;
    public MapBuilder LocalMapBuilder;
    public MapMeshCombiner LocalMapMeshCombiner;

    // Private - Data
    private Vector3 m_dimensions;
    private GenerationType m_type;
    private GenerationStep m_control;
    private ulong m_seed;
    private Transform m_parentTransform;

    // Private - Containers
    private List<Cell> m_cells; // <- Active list of Cell objects
    private ModularMapCell[,,] m_mapArray; // <- Active Map
    private List<GameObject> m_instMapObjects = new List<GameObject>(); // <- Active list of Instantiated Game Objects


    public MapGenData GetMapGenData()
    {
        MapGenData mapGenDataPkg = 
            new MapGenData(m_dimensions, m_type, m_control, m_seed);

        return mapGenDataPkg;
    }

    #region GenerateMap

    public void GenerateMap(MapGenData mapGenerationData, Transform parentTransform, List<ModularMapCellComponent> mapCellList)
    {
        UnwrapMapGenData(mapGenerationData);
        m_parentTransform = parentTransform;

        if (!IsInputValid()) { return; }

        SetLocalClass();

        CreateCells(mapCellList);

        GenerateMapData();
        InstantiateMapObjects();
        CombineMapObjects();
    }

    public void GenerateMap(MapGenData mapGenerationData, Transform parentTransform)
    {
        GenerateMap(mapGenerationData, parentTransform, null);
    }
    public void GenerateMap(MapGenData mapGenerationData, List<ModularMapCellComponent> mapCellList)
    {
        GenerateMap(mapGenerationData, null, mapCellList);
    }
    public void GenerateMap(MapGenData mapGenerationData)
    {
        GenerateMap(mapGenerationData, null, null);
    }

    #endregion

    #region CHECK + SETUP DATA
    private void UnwrapMapGenData(MapGenData mapGenerationData)
    {
        m_dimensions = mapGenerationData.Dimensions;
        m_type = mapGenerationData.Type;
        m_control = mapGenerationData.Control;
        m_seed = mapGenerationData.Seed;
    }
    private bool IsInputValid()
    {
        if (!IsMapDimensionPositive()) { return false; }
        
        return true;
    }

    private bool IsMapDimensionPositive()
    {
        return !(m_dimensions.x < 1 || m_dimensions.y < 1 || m_dimensions.z < 1);
    }

    #endregion

    #region SET-LOCAL-CLASS
    private void SetLocalClass()
    {
        SetCellGenerator();
        SetGenerator();
        SetMapBuilder();
        SetMeshCombiner();
    }
    private void SetCellGenerator()
    {
        if (LocalCellGenerator == null)
        {
            LocalCellGenerator = new CellGenerator();
        }
    }
    private void SetGenerator()
    {
        switch (m_type)
        {
            case (GenerationType.InLineCollapse):
                LocalMapGenerator = new InLineCollapse();
                break;
            case (GenerationType.WaveFunctionCollapse):
                LocalMapGenerator = new WFC_Redux();
                break;
            default:
                Debug.LogError("Undefined Generation Type");
                break;
        }
    }
    private void SetMapBuilder()
    {
        if (LocalMapBuilder == null)
        {
            LocalMapBuilder = new MapBuilder();
        }
    }
    private void SetMeshCombiner()
    {
        if (LocalMapMeshCombiner == null)
        {
            LocalMapMeshCombiner = new MapMeshCombiner();
        }
    }
    #endregion

    #region PRIMARY-EXECUTIONS
    public void ClearInstantiatedMap()
    {
        ClearBuiltListOfGameObjects();
    }
    public void ClearBuiltListOfGameObjects()
    {
        if (m_instMapObjects == null ||
            m_instMapObjects.Count < 1) { return; }

        foreach (GameObject modularMapCellObject in m_instMapObjects)
        {
            DestroyUnityObject(modularMapCellObject);
        }

        m_instMapObjects.Clear();
    }

    // destroys objects during edit and play mode
    public void DestroyUnityObject(Object obj)
    {
        if (Application.isPlaying)
            GameObject.Destroy(obj);
        else
            GameObject.DestroyImmediate(obj);
    }


    private void CreateCells(List<ModularMapCellComponent> mapCellList)
    {
        LocalCellGenerator.Init(ref mapCellList);
        m_cells = LocalCellGenerator.GetCells();
    }

    private void GenerateMapData()
    {
        LocalMapGenerator.Generate(m_dimensions, ref m_mapArray, m_cells);
    }

    private void InstantiateMapObjects()
    {
        if (m_mapArray != null)
        {
            int[,,] rawMapData = new int[(int)m_dimensions.x, (int)m_dimensions.y, (int)m_dimensions.z];
            ConvertModulesToRawMapData(m_dimensions, m_mapArray, ref rawMapData);

            InstantiateObjectsFromRawData(rawMapData);
        }
            #if UNITY_EDITOR
        else
        {
            Debug.LogWarning("3D-MaGic: NO MAP DATA FOUND!");
        }
            #endif
    }

    private void InstantiateObjectsFromRawData(int[,,] rawMapData)
    {
        m_instMapObjects = LocalMapBuilder.InstantiateMap(m_dimensions, rawMapData, m_cells, m_parentTransform);
    }

    private void CombineMapObjects()
    {
        if (m_instMapObjects.Count > 0)
        {
            LocalMapMeshCombiner.CombineMeshes(ref m_instMapObjects);
            m_parentTransform.gameObject.SetActive(false);
        }
    }


    private void ConvertModulesToRawMapData(Vector3 dimensions, ModularMapCell[,,] mapArray, ref int[,,] rawMapData)
    {
        for (int z = 0; z < dimensions.z; z++)
        {
            for (int y = 0; y < dimensions.y; y++)
            {
                for (int x = 0; x < dimensions.x; x++)
                {
                    rawMapData[x, y, z] = mapArray[x, y, z].Module;
                }
            }
        }
    }

    #endregion

    #region SAVE + LOAD

    public void Save(ref GenData genData)
    {

        genData._Dimensions = m_dimensions;
        genData._Seed = m_seed;

        if (LocalMapGenerator != null)
        {
            int[,,] rawMapData = new int[(int)m_dimensions.x, (int)m_dimensions.y, (int)m_dimensions.z];
            ConvertModulesToRawMapData(m_dimensions, m_mapArray, ref rawMapData);

            genData._RawGenData = new int[(int)(m_dimensions.x * m_dimensions.y * m_dimensions.z)];
            for (int z = 0; z < m_dimensions.z; z++)
            {
                for (int y = 0; y < m_dimensions.y; y++)
                {
                    for (int x = 0; x < m_dimensions.x; x++)
                    {
                        genData._RawGenData[x + (int)genData._Dimensions.z * (y + (int)genData._Dimensions.y * z)] = rawMapData[x, y, z];
                    }
                }
            }
        }

    }

    public void Load(GenData _data)
    {
        m_dimensions = _data._Dimensions;
        m_seed = _data._Seed;


        int[,,] rawMapData = new int[(int)m_dimensions.x, (int)m_dimensions.y, (int)m_dimensions.z];

        // Converts single data array of raw map data =>(to) into 3-Dimensional Array of raw map data
        for (int z = 0; z < _data._Dimensions.z; z++)
        {
            for (int y = 0; y < _data._Dimensions.y; y++)
            {
                for (int x = 0; x < _data._Dimensions.x; x++)
                {
                    rawMapData[x, y, z] = _data._RawGenData[x + (int)_data._Dimensions.z * (y + (int)_data._Dimensions.y * z)];
                }
            }
        }

        InstantiateObjectsFromRawData(rawMapData);
    }

    #endregion
}
