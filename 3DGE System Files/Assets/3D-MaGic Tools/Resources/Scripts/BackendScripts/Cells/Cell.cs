using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Cell
{
    public int ID = 0;
    public GameObject PrefabGameObject = null;
    public sbyte Rotation = 0;
    public LayerTypes Layers = LayerTypes.None;
    public Connection ConnectionRight;
    public Connection ConnectionFront;
    public Connection ConnectionUp;
    public Connection ConnectionLeft;
    public Connection ConnectionBack;
    public Connection ConnectionDown;
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
    
    public void Save(ref MicroCell data)
    {
        // Cell Data
        data.ID = ID;
        data.PrefabGameObject = PrefabGameObject;
        data.Rotation = Rotation;
        data.Layers = Layers;

        //public Connection ConnectionRight;
        //public Connection ConnectionFront;
        //public Connection ConnectionUp;
        //public Connection ConnectionLeft;
        //public Connection ConnectionBack;
        //public Connection ConnectionDown;

        // Bitset Data
        List<BitsetData> lst_bitsetData = new List<BitsetData>();
        for (int i = 0; i < Connections.Count; i++)
        {
            BitsetData new_data = new BitsetData();
            Connections[i].Save(ref new_data);
            lst_bitsetData.Add(new_data);
        }

        data.EdgeConnections = lst_bitsetData.ToArray();

    }
}