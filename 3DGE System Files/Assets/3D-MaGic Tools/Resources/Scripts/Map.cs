using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.PackageManager;
using UnityEngine;

// RELATED ENUMS
public enum GenerationType
{
    InLineCollapse,
    WaveFunctionCollapse,
    WFC_Redux
}

public enum MapGenerationControl{
    Generate = 0,
    GenerateAndBuild = 1,
    ClearAndGenerateAndBuild = 2,
    ClearGenerateBuildCombine = 3,
    Build = 4,
    Combine = 5
}

public enum MapSaveLoadType
{
    Save,
    Load,
    Load_and_Build
    
}

[ExecuteInEditMode]
public class Map : MonoBehaviour
{

    // Modular Map Components
    [SerializeField] public List<ModularMapCellComponent> MapCellComponentsList = new List<ModularMapCellComponent>();

    public MapController LocalMapController;


    // Map Dimension Controls 
    [SerializeField] private Vector3 _dimensions = new Vector3(10, 3, 10);
    
    // Map Generator Type
    [SerializeField] private GenerationType _mapGenerationType = GenerationType.WaveFunctionCollapse;

    // Generation Type
    [SerializeField] private MapGenerationControl _mapGenerationControl = MapGenerationControl.GenerateAndBuild;

    // Seed controls
    [SerializeField] private bool _useCurrentSeed;

    [SerializeField] private ulong _currentSeed;

    // Parent Object
    [SerializeField] private Transform _parentTransform;

    // File Name
    [SerializeField] private string _fileName = "FILE_NAME";

    public void UpdateDisplayData(MapGenData data)
    {
        _dimensions = data._dimensions;
        _currentSeed = data._seed;
    }

    private void CheckLocalMapController()
    {
        if (LocalMapController == null)
        {
            LocalMapController = new MapController();
        }
    }

    public void GenerateMap()
    {
        CheckLocalMapController();
        LocalMapController.GenerateMap(GetMapGenerationData(), _parentTransform, MapCellComponentsList);
        MapCellComponentsList.Clear();
        _currentSeed = RandomNumber.GetSeed();
    }

    private MapGenData GetMapGenerationData()
    {
        MapGenData mapGenData = new MapGenData();
        mapGenData._dimensions = _dimensions;
        mapGenData._type = _mapGenerationType;
        mapGenData._control = _mapGenerationControl;
        mapGenData._seed = _currentSeed;
        mapGenData._customSeed = _useCurrentSeed;

        return mapGenData;
    }

    public void ClearMap()
    {
        LocalMapController.ClearBuiltMap();
    }


    #region Save/Load

    public bool TryStartSaveSystem()
    {
        if (!MapControllerCheck()) return false;
        DMG_SaveSystem.Init(LocalMapController);
        return true;
    }
    public void SaveMap(string fileName)
    {
        if (!TryStartSaveSystem()) return;
        DMG_SaveSystem.SaveMap(_fileName);
    }

    public void LoadMap(string fileName)
    {
        if (!TryStartSaveSystem()) return;
        DMG_SaveSystem.LoadMap(_fileName);
    }

    public bool MapControllerCheck()
    {
        if (LocalMapController == null)
        {
            Debug.LogError("NO MAPCONTROLLER!");
            return false;
        }

        return true;
    }

    #endregion

}