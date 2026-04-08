// Save System Information - https://www.youtube.com/watch?v=1mf730eb5Wo&t 
using System.IO;
using UnityEngine;

[System.Serializable]
public struct GenData
{
    public Vector3 _Dimensions;
    public ulong _Seed;
    public int[] _RawGenData;
}


public static class DMG_SaveSystem
{
    private static DMG_SaveData_Map s_mapSaveData = new DMG_SaveData_Map();
    private static DMG_SaveData_Cells s_cellsSaveData = new DMG_SaveData_Cells();

    private static MapController s_mapController;

    private static string s_fileName = "map";

    [System.Serializable]
    public struct DMG_SaveData_Map
    {
        public GenData GenerationData;
        public CellGroupData CellGroupData;
    }

    [System.Serializable]
    public struct DMG_SaveData_Cells
    {
        public CellGroupData CellGroupData;
    }



    public static void Init(MapController mapController)
    {
        s_mapController = mapController;
    }

    // SAVE/LOAD MAP ******************************************************************************
    // ********************************************************************************************
    private static string SaveFilePath_Map()
    {
        return Application.persistentDataPath + "/" + s_fileName + ".svm";
    }

    public static void SaveMap(string fileName = "map")
    {
        s_fileName = fileName;

        HandleSaveGenData();
        File.WriteAllText(SaveFilePath_Map(), JsonUtility.ToJson(s_mapSaveData, true));
        Debug.Log(SaveFilePath_Map());
    }

    public static void HandleSaveGenData()
    {
        s_mapController.Save(ref s_mapSaveData.GenerationData);
        s_mapController.LocalCellGenerator.Save(ref s_mapSaveData.CellGroupData);
    }

    public static void LoadMap(string _fileName = "map")
    {
        s_fileName = _fileName;

        string save_data = File.ReadAllText(SaveFilePath_Map());

        s_mapSaveData = JsonUtility.FromJson<DMG_SaveData_Map>(save_data);

        HandleLoadGenData();
    }

    private static void HandleLoadGenData()
    {
        s_mapController.Load(s_mapSaveData.GenerationData);
        s_mapController.LocalCellGenerator.Load(s_mapSaveData.CellGroupData);

    }

    // ********************************************************************************************
    // ********************************************************************************************


    // SAVE/LOAD CELLS ****************************************************************************
    // ********************************************************************************************

    private static string SaveFilePath_Cells()
    {
        return Application.persistentDataPath + "/cells.svc";
    }

    public static void SaveCells()
    {
        HandleSaveCellData();
        File.WriteAllText(SaveFilePath_Cells(), JsonUtility.ToJson(s_cellsSaveData, true));
    }

    public static void HandleSaveCellData()
    {
        s_mapController.LocalCellGenerator.Save(ref s_cellsSaveData.CellGroupData);
    }

    public static void LoadCell()
    {
        string save_data = File.ReadAllText(SaveFilePath_Cells());

        s_cellsSaveData = JsonUtility.FromJson<DMG_SaveData_Cells>(save_data);

        HandleLoadCellData();
    }

    private static void HandleLoadCellData()
    {
        s_mapController.LocalCellGenerator.Load(s_cellsSaveData.CellGroupData);
    }

    public static bool HasCellDataFile()
    {
        return File.Exists(SaveFilePath_Cells());
    }

    // ********************************************************************************************
    // ********************************************************************************************
}

