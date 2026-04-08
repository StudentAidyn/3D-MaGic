using System.Collections.Generic;
using UnityEngine;

// RELATED ENUMS
public enum GenerationType
{
    InLineCollapse,
    WaveFunctionCollapse
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

    // Save Controls
    [SerializeField] private string m_fileName = "FILE_NAME";

    // Generation Controls 
    [SerializeField] private Vector3 m_dimensions = new Vector3(10, 3, 10);
    [SerializeField] private GenerationType m_mapGenerationType;
    [SerializeField] private GenerationStep m_mapGenerationControl;
    [SerializeField] private Transform m_parentTransform;

    // m_seed Controls
    [SerializeField] private bool m_useCurrentSeed;
    [SerializeField] private ulong m_currentSeed;

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
    public void SaveMap()
    {
        if (!TryStartSaveSystem()) return;
        DMG_SaveSystem.SaveMap(m_fileName);
    }

    public void LoadMap()
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