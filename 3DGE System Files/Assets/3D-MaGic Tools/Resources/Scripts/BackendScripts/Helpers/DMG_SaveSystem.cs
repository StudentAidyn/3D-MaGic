// Save System Information - https://www.youtube.com/watch?v=1mf730eb5Wo&t 
using System.IO;
using UnityEngine;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Collections.Generic;

[System.Serializable]
public struct GenData
{
    public Vector3 _Dimensions;
    public ulong _Seed;
    public int[] _RawGenData;
}


public static class DMG_SaveSystem
{
    private static string m_name = "data";

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

    private static string GetPersistentFilePath()
    {
        return Application.persistentDataPath + "/";
    }

    private static string GetFileExtension()
    {
        return ".json";
    }

    private static string GetFullFilePath(string fileName)
    {
        return GetPersistentFilePath() + fileName + GetFileExtension();
    }

    public static bool DoesFileExist(string fileName)
    {
        return File.Exists(GetFullFilePath(fileName));
    }

    public static void Save(ISaveable iSaveable, string fileName)
    {
        string filePath = GetFullFilePath(fileName);

        JToken tokenData = iSaveable.Save();

        JObject objectData = new JObject();
        objectData.Add(m_name, tokenData);

        SaveToFile(filePath, objectData);
    }

    private static void SaveToFile(string filePath, JObject data)
    {
        using (var textWriter = File.CreateText(filePath))
        {
            using (var writer = new JsonTextWriter(textWriter))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(writer, data);
            }
        }
        Debug.Log("Saved to - " + filePath);
    }

    public static void Load(ISaveable iSaveable, string fileName)
    {
        string filePath = GetFullFilePath(fileName);
        JObject dataObject = LoadFromFile(filePath);
        IDictionary<string, JToken> dataDict = dataObject;

        if(dataDict.TryGetValue("data", out JToken data))
        {
            iSaveable.Load(data);
        }

        
    }

    private static JObject LoadFromFile(string savePath)
    {
        if (!File.Exists(savePath)) { return new JObject(); }

        using (var textReader = File.OpenText(savePath))
        {
            using (var reader = new JsonTextReader(textReader))
            {
                reader.FloatParseHandling = FloatParseHandling.Double;
                return JObject.Load(reader);
            }
        }
    }

}

