using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;


public struct MapGenData
{
    public Vector3 Dimensions;
    public GenerationType Type;
    public GenerationStep Control;
    public ulong Seed;
    public bool CustomSeed;
}

public class MapController
{
    // Public - Classes
    public CellGenerator LocalCellGenerator;
    public MapGenerator LocalMapGenerator;
    public MapBuilder LocalMapBuilder;
    public MapMeshCombiner LocalMapMeshCombiner;

    // Private
    private MapGenData m_generationData = new();
    private Transform m_parentTransform;
    private List<ModularMapCellComponent> m_cellComponentsList;
    private int[,,] m_rawDataArray;

    private bool m_clearMap = false;
    private bool m_generateMap = false;
    private bool m_buildMap = false;
    private bool m_combineMap = false;

    public MapGenData GetMapGenData() => m_generationData;

    public void ClearInstantiatedMap()
    {
        if (LocalMapBuilder != null) { LocalMapBuilder.ClearBuiltListOfGameObjects(); }
    }

    #region Generate

    public void GenerateMap(MapGenData mapGenerationData, Transform parentTransform = null, List<ModularMapCellComponent> mapCellList = null)
    {
        m_generationData = mapGenerationData;
        m_parentTransform = parentTransform;
        m_cellComponentsList = mapCellList;

        if (!AreMapDimensionsPositive()) { return; }
        SetLocalClassVariables();
        InitializeCustomSystems();
        ExecuteMapGeneration();
    }

    private bool AreMapDimensionsPositive()
    {
        if (m_generationData.Dimensions.x < 1 || m_generationData.Dimensions.y < 1 || m_generationData.Dimensions.z < 1)
        {
            return false;
        }

        return true;
    }



    private void SetLocalClassVariables()
    {
        if (LocalCellGenerator == null)
        { 
            LocalCellGenerator = new(); 
        }

        SetGeneratorFromType();

        if (LocalMapBuilder == null)
        {
            LocalMapBuilder = new MapBuilder();
        }

        if (LocalMapMeshCombiner == null)
        {
            LocalMapMeshCombiner = new MapMeshCombiner();
        }

        // New Pipeline scripts
        //if (LocalMap == null)
        //{
        //    LocalMap = new Map();
        //}
    }

    // Select Generator Type
    private void SetGeneratorFromType()
    {
        switch (m_generationData.Type)
        {
            case (GenerationType.InLineCollapse):
                LocalMapGenerator = new InLineCollapse();
                break;
            case (GenerationType.WaveFunctionCollapse):
                LocalMapGenerator = new WaveFunctionCollapse();
                break;
            case (GenerationType.WFC_Redux):
                LocalMapGenerator = new WFC_Redux();
                break;
            default:
                Debug.LogError("Undefined Generation Type");
                break;
        }
    }

    private void InitializeCustomSystems()
    {
        DMG_SaveSystem.Init(this);
        RandomNumber.Init(m_generationData.CustomSeed, m_generationData.Seed);
    }
    
    // Sets the output cases of the generation pipeline
    private void SetupGenerationVariables()
    {
        switch (m_generationData.Control)
        {
            case GenerationStep.GenerateBuildCombine:
                m_generateMap    = true;
                m_buildMap       = true;
                m_combineMap     = true;
                break;

            case GenerationStep.GenerateBuild:
                m_generateMap     = true;
                m_buildMap        = true;
                m_combineMap      = false;
                break;

            case GenerationStep.Generate:
                m_generateMap    = true;
                m_buildMap       = false;
                m_combineMap     = false;
                break;

            case GenerationStep.Build: 
                m_generateMap    = false;
                m_buildMap       = true;
                m_combineMap     = true;
                break;

            case GenerationStep.Combine: 
                m_generateMap    = false;
                m_buildMap       = false;
                m_combineMap     = true;
                break;

            default:
                break;
        }
    }

    private void ExecuteMapGeneration()
    {
        SetupGenerationVariables();

        if (m_clearMap)
        {
            ClearInstantiatedMap();
        }

        if (m_generateMap)
        {
            LocalCellGenerator.Init(ref m_cellComponentsList);
            Generate();
        }

        if (m_buildMap)
        {
            LocalMapBuilder.Init();
            Build();
        }

        if (m_combineMap)
        {
            LocalMapMeshCombiner.Init();
            Combine();
        }
    }

    private void Generate()
    {
        TimeKeeper.RegisterStartTime();
        LocalMapGenerator.Generate(m_generationData.Dimensions, LocalCellGenerator.GetCells());
        TimeKeeper.RegisterEndTime();
        Debug.Log(TimeKeeper.GetTotalTime());

        m_rawDataArray = new int[(int)m_generationData.Dimensions.x, (int)m_generationData.Dimensions.y, (int)m_generationData.Dimensions.z];
        LocalMapGenerator.GenerateRawMapData(ref m_rawDataArray, m_generationData.Dimensions);
    }

    private void Build()
    {
        if(m_rawDataArray != null)
        {
            LocalMapBuilder.InstantiateMap(m_generationData.Dimensions, m_rawDataArray, LocalCellGenerator.GetCells(), m_parentTransform);
        }
        else
        {
#if UNITY_EDITOR
            Debug.LogWarning("3D-MaGic: NO MAP DATA FOUND!");
#endif
        }
    }


    private void Combine()
    {
        if(LocalMapBuilder != null)
        {
            List<GameObject> local_map_objects = LocalMapBuilder.GetMapObjects();
            if (local_map_objects.Count > 0)
            {
                LocalMapMeshCombiner.CombineMeshes(ref local_map_objects);
                m_parentTransform.gameObject.SetActive(false);
            }
        }
    }

    #endregion


    #region SAVE + LOAD

    public void Save(ref GenData _data)
    {

        _data._Dimensions = m_generationData.Dimensions;
        _data._Seed = m_generationData.Seed;

        if (LocalMapGenerator != null)
        {
            int[,,] rawMapData = new int[(int)m_generationData.Dimensions.x, (int)m_generationData.Dimensions.y, (int)m_generationData.Dimensions.z];
            LocalMapGenerator.GenerateRawMapData(ref rawMapData, m_generationData.Dimensions);

            _data._RawGenData = new int[(int)(m_generationData.Dimensions.x * m_generationData.Dimensions.y * m_generationData.Dimensions.z)];
            for (int z = 0; z < m_generationData.Dimensions.z; z++)
            {
                for (int y = 0; y < m_generationData.Dimensions.y; y++)
                {
                    for (int x = 0; x < m_generationData.Dimensions.x; x++)
                    {
                        _data._RawGenData[x + (int)_data._Dimensions.z * (y + (int)_data._Dimensions.y * z)] = rawMapData[x, y, z];
                    }
                }
            }
        }

    }

    public void Load(GenData _data)
    {
        m_generationData.Dimensions = _data._Dimensions;
        m_generationData.Seed = _data._Seed;

        m_rawDataArray = new int[(int)m_generationData.Dimensions.x, (int)m_generationData.Dimensions.y, (int)m_generationData.Dimensions.z];

        for (int z = 0; z < _data._Dimensions.z; z++)
        {
            for (int y = 0; y < _data._Dimensions.y; y++)
            {
                for (int x = 0; x < _data._Dimensions.x; x++)
                {
                    m_rawDataArray[x, y, z] = _data._RawGenData[x + (int)_data._Dimensions.z * (y + (int)_data._Dimensions.y * z)];
                }
            }
        }
    }

    #endregion
}
