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

public enum GenerationStep{
    GenerateBuildCombine,
    GenerateBuild,
    Generate,
    Build,
    Combine
}


[ExecuteInEditMode]
public class Map : MonoBehaviour
{

    // Modular Map Components
    [SerializeField] public List<ModularMapCellComponent> MapCellComponentsList = new List<ModularMapCellComponent>();

    public MapController LocalMapController;


    // Map Dimension Controls 
    [SerializeField] private Vector3 m_dimensions = new Vector3(10, 3, 10);
    
    // Map Generator Type
    [SerializeField] private GenerationType m_mapGenerationType = GenerationType.WaveFunctionCollapse;

    // Generation Type
    [SerializeField] private GenerationStep m_mapGenerationControl = GenerationStep.GenerateBuild;

    // Seed controls
    [SerializeField] private bool m_useCurrentSeed;

    [SerializeField] private ulong m_currentSeed;

    // Parent Object
    [SerializeField] private Transform m_parentTransform;

    // File Name
    [SerializeField] private string m_fileName = "FILE_NAME";

    public void UpdateDisplayData(MapGenData data)
    {
        m_dimensions = data.Dimensions;
        m_currentSeed = data.Seed;
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
        LocalMapController.GenerateMap(GetMapGenerationData(), m_parentTransform, MapCellComponentsList);
        MapCellComponentsList.Clear();
        m_currentSeed = RandomNumber.GetSeed();
    }

    private MapGenData GetMapGenerationData()
    {
        MapGenData mapGenData = new MapGenData();
        mapGenData.Dimensions = m_dimensions;
        mapGenData.Type = m_mapGenerationType;
        mapGenData.Control = m_mapGenerationControl;
        mapGenData.Seed = m_currentSeed;
        mapGenData.CustomSeed = m_useCurrentSeed;

        return mapGenData;
    }

    public void ClearMap()
    {
        LocalMapController.ClearInstantiatedMap();
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
        DMG_SaveSystem.SaveMap(m_fileName);
    }

    public void LoadMap(string fileName)
    {
        if (!TryStartSaveSystem()) return;
        DMG_SaveSystem.LoadMap(m_fileName);
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