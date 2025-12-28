using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Cell
{
    public int _int_objectID = 0;
    public GameObject _go_gameObject = null;
    public sbyte _sbyte_rotation = 0;
    public LayerTypes _lt_layerTypes = LayerTypes.None;
    public Connection _con_posX;
    public Connection _con_posY;
    public Connection _con_posZ;
    public Connection _con_negX;
    public Connection _con_negY;
    public Connection _con_negZ;
    public List<Bitset> _list_btst_connections = new List<Bitset>();
    public void Load(CellData data)
    {
        _int_objectID = data._int_objectID;
        _go_gameObject = data._go_gameObject;
        _sbyte_rotation = data._sbyte_rotation;
        _lt_layerTypes = data._lt_layerTypes;

        for (int i = 0; i < data._list_bsdt_connections.Length; i++)
        {
            Bitset new_bitset = new Bitset();
            new_bitset.Load(data._list_bsdt_connections[i]);
            _list_btst_connections.Add(new_bitset);
        }
    }

    public void Save(ref CellData data)
    {
        // Cell Data
        data._int_objectID = _int_objectID;
        data._go_gameObject = _go_gameObject;
        data._sbyte_rotation = _sbyte_rotation;
        data._lt_layerTypes = _lt_layerTypes;

        //public Connection _con_posX;
        //public Connection _con_posY;
        //public Connection _con_posZ;
        //public Connection _con_negX;
        //public Connection _con_negY;
        //public Connection _con_negZ;

        // Bitset Data
        List<BitsetData> lst_bitsetData = new List<BitsetData>();
        for (int i = 0; i < _list_btst_connections.Count; i++)
        {
            BitsetData new_data = new BitsetData();
            _list_btst_connections[i].Save(ref new_data);
            lst_bitsetData.Add(new_data);
        }

        data._list_bsdt_connections = lst_bitsetData.ToArray();

    }
}