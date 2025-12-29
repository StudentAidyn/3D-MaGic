using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;


public struct MapGenData
{
    public Vector3 _dimensions;
    public GenerationType _type;
    public MapGenerationControl _control;
    public ulong _seed;
    public bool _customSeed;
}

public class MapController
{
    // Public Variables - Classes
    public CellGenerator LocalCellGenerator;
    public MapGenerator LocalMapGenerator;
    public MapBuilder LocalMapBuilder;
    public MapMeshCombiner LocalMapMeshCombiner;

    // Private
    private MapGenData _generationData = new();
    private Transform _parentTransform;
    private List<ModularMapCellComponent> _cellComponentsList;
    private int[,,] _rawDataArray;

    private bool clearMap       = false;
    private bool generateMap    = false;
    private bool buildMap       = false;
    private bool combineMap     = false;

    public MapGenData GetMapGenData() => _generationData;

    public void ClearBuiltMap()
    {
        if (LocalMapBuilder != null) LocalMapBuilder.ClearBuiltListOfGameObjects();
    }

    #region Generate

    // generateMap Button
    public void GenerateMap(MapGenData mapGenData, Transform parentTransform = null, List<ModularMapCellComponent> mapCellList = null)
    {
        _generationData = mapGenData;
        _parentTransform = parentTransform;
        _cellComponentsList = mapCellList;

        if (!AreMapDimensionsPositive()) { return; }
        SetLocalClasses();
        InitisializeVariables();
        ExecuteMapGeneration();
    }

    private bool AreMapDimensionsPositive()
    {
        if (_generationData._dimensions.x < 1 || _generationData._dimensions.y < 1 || _generationData._dimensions.z < 1)
        {
            return false;
        }

        return true;
    }



    private void SetLocalClasses()
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
        switch (_generationData._type)
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

    private void InitisializeVariables()
    {
        DMG_SaveSystem.Init(this);
        RandomNumber.Init(_generationData._customSeed, _generationData._seed);
    }
    
    // Sets the output cases of the generation pipeline
    private void SetupGenerationVariables()
    {
        switch (_generationData._control)
        {
            case MapGenerationControl.ClearAndGenerateAndBuild:
                clearMap       = true;
                generateMap    = true;
                buildMap       = true;
                combineMap     = false;
                break;
            case MapGenerationControl.Generate:
                clearMap       = false;
                generateMap    = true;
                buildMap       = false;
                combineMap     = false;
                break;
            case MapGenerationControl.GenerateAndBuild:
                clearMap       = false;
                generateMap    = true;
                buildMap       = true;
                combineMap     = false;
                break;
            case MapGenerationControl.ClearGenerateBuildCombine: 
                clearMap       = true;
                generateMap    = true;
                buildMap       = true;
                combineMap     = true;
                break;
            case MapGenerationControl.Combine: 
                clearMap       = false;
                generateMap    = false;
                buildMap       = false;
                combineMap     = true;
                break;
            default:
                break;
        }
    }

    private void ExecuteMapGeneration()
    {
        SetupGenerationVariables();

        if (clearMap)
        {
            ClearBuiltMap();
        }

        if (generateMap)
        {
            LocalCellGenerator.Init(ref _cellComponentsList);
            Generate();
        }

        if (buildMap)
        {
            LocalMapBuilder.Init();
            Build();
        }

        if (combineMap)
        {
            LocalMapMeshCombiner.Init();
            Combine();
        }
    }

    private void Generate()
    {
        TimeKeeper.RegisterStartTime();
        LocalMapGenerator.Generate(_generationData._dimensions, LocalCellGenerator.Cells());
        TimeKeeper.RegisterEndTime();
        Debug.Log(TimeKeeper.GetTotalTime());

        _rawDataArray = new int[(int)_generationData._dimensions.x, (int)_generationData._dimensions.y, (int)_generationData._dimensions.z];
        LocalMapGenerator.GenerateRawMapData(ref _rawDataArray, _generationData._dimensions);
    }

    private void Build()
    {
        if(_rawDataArray != null)
        {
            LocalMapBuilder.BuildMap(_generationData._dimensions, _rawDataArray, LocalCellGenerator.Cells(), _parentTransform);
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
            List<GameObject> local_map_objects = LocalMapBuilder.MapObjects();
            if (local_map_objects.Count > 0)
            {
                LocalMapMeshCombiner.CombineMeshes(ref local_map_objects);

            }
        }
    }

    #endregion


    #region SAVE + LOAD

    public void Save(ref GenData _data)
    {

        _data._Dimensions = _generationData._dimensions;
        _data._Seed = _generationData._seed;

        if (LocalMapGenerator != null)
        {
            int[,,] rawMapData = new int[(int)_generationData._dimensions.x, (int)_generationData._dimensions.y, (int)_generationData._dimensions.z];
            LocalMapGenerator.GenerateRawMapData(ref rawMapData, _generationData._dimensions);

            _data._RawGenData = new int[(int)(_generationData._dimensions.x * _generationData._dimensions.y * _generationData._dimensions.z)];
            for (int z = 0; z < _generationData._dimensions.z; z++)
            {
                for (int y = 0; y < _generationData._dimensions.y; y++)
                {
                    for (int x = 0; x < _generationData._dimensions.x; x++)
                    {
                        _data._RawGenData[x + (int)_data._Dimensions.z * (y + (int)_data._Dimensions.y * z)] = rawMapData[x, y, z];
                    }
                }
            }
        }

    }

    public void Load(GenData _data)
    {
        _generationData._dimensions = _data._Dimensions;
        _generationData._seed = _data._Seed;

        _rawDataArray = new int[(int)_generationData._dimensions.x, (int)_generationData._dimensions.y, (int)_generationData._dimensions.z];

        for (int z = 0; z < _data._Dimensions.z; z++)
        {
            for (int y = 0; y < _data._Dimensions.y; y++)
            {
                for (int x = 0; x < _data._Dimensions.x; x++)
                {
                    _rawDataArray[x, y, z] = _data._RawGenData[x + (int)_data._Dimensions.z * (y + (int)_data._Dimensions.y * z)];
                }
            }
        }
    }

    #endregion
}
