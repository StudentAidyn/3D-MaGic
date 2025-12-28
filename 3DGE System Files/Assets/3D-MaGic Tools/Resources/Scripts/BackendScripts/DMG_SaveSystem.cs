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
    private static DMG_SaveData_Map dmg_saveData_map = new DMG_SaveData_Map();
    private static DMG_SaveData_Cells dmg_saveData_cells = new DMG_SaveData_Cells();

    private static MapController mc_controller;

    private static string str_fileName = "map";

    [System.Serializable]
    public struct DMG_SaveData_Map
    {
        public GenData _GenData;
        public CellGroupData _CellGroupData;
    }

    [System.Serializable]
    public struct DMG_SaveData_Cells
    {
        public CellGroupData _CellGroupData;
    }



    public static void Init(MapController _map)
    {
        mc_controller = _map;
    }

    // SAVE/LOAD MAP ******************************************************************************
    // ********************************************************************************************
    private static string SaveFilePath_Map()
    {
        return Application.persistentDataPath + "/" + str_fileName + ".svm";
    }

    public static void SaveMap(string _fileName = "map")
    {
        str_fileName = _fileName;

        HandleSaveGenData();
        File.WriteAllText(SaveFilePath_Map(), JsonUtility.ToJson(dmg_saveData_map, true));
        Debug.Log(SaveFilePath_Map());
    }

    public static void HandleSaveGenData()
    {
        mc_controller.Save(ref dmg_saveData_map._GenData);
        mc_controller.LocalCellGenerator.Save(ref dmg_saveData_map._CellGroupData);
    }

    public static void LoadMap(string _fileName = "map")
    {
        str_fileName = _fileName;

        string save_data = File.ReadAllText(SaveFilePath_Map());

        dmg_saveData_map = JsonUtility.FromJson<DMG_SaveData_Map>(save_data);

        HandleLoadGenData();
    }

    private static void HandleLoadGenData()
    {
        mc_controller.Load(dmg_saveData_map._GenData);
        mc_controller.LocalCellGenerator.Load(dmg_saveData_map._CellGroupData);

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
        File.WriteAllText(SaveFilePath_Cells(), JsonUtility.ToJson(dmg_saveData_cells, true));
    }

    public static void HandleSaveCellData()
    {
        mc_controller.LocalCellGenerator.Save(ref dmg_saveData_cells._CellGroupData);
    }

    public static void LoadCell()
    {
        string save_data = File.ReadAllText(SaveFilePath_Cells());

        dmg_saveData_cells = JsonUtility.FromJson<DMG_SaveData_Cells>(save_data);

        HandleLoadCellData();
    }

    private static void HandleLoadCellData()
    {
        mc_controller.LocalCellGenerator.Load(dmg_saveData_cells._CellGroupData);
    }

    public static bool HasCellDataFile()
    {
        return File.Exists(SaveFilePath_Cells());
    }

    // ********************************************************************************************
    // ********************************************************************************************
}

