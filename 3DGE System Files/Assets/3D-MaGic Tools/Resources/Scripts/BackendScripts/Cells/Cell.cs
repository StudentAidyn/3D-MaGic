using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Cell : ISaveable
{
    public int ID = 0;
    public GameObject PrefabGameObject = null;
    public sbyte Rotation = 0;
    public LayerTypes Layers = LayerTypes.None;
    public Connection ConnectionRight;
    public Connection ConnectionUp;
    public Connection ConnectionFront;
    public Connection ConnectionLeft;
    public Connection ConnectionDown;
    public Connection ConnectionBack;
    public List<Bitset> Connections = new List<Bitset>();

    public Vector3 GetRotationInDegrees()
    { 
        return new Vector3(0f, -(Rotation) * 90f, 0); 
    }

    public void Load(MicroCell data)
    {
        ID = data.ID;
        PrefabGameObject = data.PrefabGameObject;
        Rotation = data.Rotation;
        Layers = data.Layers;

        for (int i = 0; i < data.EdgeConnections.Length; i++)
        {
            Bitset newBitset = new Bitset();
            newBitset.Load(data.EdgeConnections[i]);
            Connections.Add(newBitset);
        }
    }

    public void Load(JToken token)
    {
        MicroCell cellData = JsonUtility.FromJson<MicroCell>(token.ToString());
        Load(cellData);
    }

    public void Save(ref MicroCell data)
    {
        // Cell Data
        data.ID = ID;
        data.PrefabGameObject = PrefabGameObject;
        data.Rotation = Rotation;
        data.Layers = Layers;

        // Bitset Data
        List<BitsetData> lst_bitsetData = new List<BitsetData>();
        for (int i = 0; i < Connections.Count; i++)
        {
            BitsetData newData = new BitsetData();
            Connections[i].Save(ref newData);
            lst_bitsetData.Add(newData);
        }

        data.EdgeConnections = lst_bitsetData.ToArray();

    }

    public JToken Save()
    {
        MicroCell data = new MicroCell();
        Save(ref data);
        string jsonData = JsonUtility.ToJson(data);
        return JToken.Parse(jsonData);
    }
}