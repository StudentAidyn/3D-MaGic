using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
public class MapGenerator
{
    protected ModularMapCell[,,] _mapArray;
    protected List<Cell> _cellsList;

    public MapGenerator()
    {
    }
    public ModularMapCell[,,] GetMap() { return _mapArray; }

    protected virtual void Init(Vector3 dimensions, List<Cell> cellsList)
    {
        _mapArray =
            new ModularMapCell[
                (int)dimensions.x,
                (int)dimensions.y,
                (int)dimensions.z];

        int bitsetSize = cellsList.Count;

        for (int z = 0; z < dimensions.z; z++)
        {
            for (int y = 0; y < dimensions.y; y++)
            {
                for (int x = 0; x < dimensions.x; x++)
                {
                    _mapArray[x, y, z] = new ModularMapCell(bitsetSize);
                }

            }

        }

        _cellsList = cellsList;

        Debug.Log("FROM WITHIN MAP GENERATOR: bs_size - " + bitsetSize + ", cell_list size - " + cellsList.Count);
    }
    // GenerateMap Function
    public virtual void Generate(Vector3 dimensions, List<Cell> cellsList) { }

    public virtual async Task GenerateAsync(Vector3 dimensions, List<Cell> cellList) { }

    public void GenerateRawMapData(ref int[,,] rawDataArrayReference, Vector3 dimensions)
    {
        for (int z = 0; z < dimensions.z; z++)
        {
            for (int y = 0; y < dimensions.y; y++)
            {
                for (int x = 0; x < dimensions.x; x++)
                {
                    rawDataArrayReference[x, y, z] = _mapArray[x, y, z]._int_module;
                }

            }
        }
    }



    protected ref ModularMapCell GetModule(Vector3 map_pos)
    {
        return ref _mapArray[(int)map_pos.x, (int)map_pos.y, (int)map_pos.z];
    }





    #region Cell Logic

    public Cell GetCell(int index)
    {
        if (index >= _cellsList.Count || index < 0)
        {
            return null;
        }

        return _cellsList[index];
    }

    // Get Collapsed Object Options
    public Bitset GetEdge_OptionsFrom_(connector_edge _connectorEdge, int index)
    {
        Cell mod_cell = GetCell(index);
        if (mod_cell != null)
        {
            return mod_cell._list_btst_connections[(int)_connectorEdge];
        }
        return new Bitset(0);
    }

    // Non-Collapsed Object Options
    public Bitset GetEdge_AllOptionsFrom_(connector_edge _connectorEdge, Bitset _currentOptions)
    {
        // create new bitset to store new found options => ensure it is reset upon creation
        Bitset found_options = new Bitset(_currentOptions.Size());
        found_options.AllReset();

        // find and fill options
        for (int i = 0; i < _currentOptions.Size(); i++)
        {
            if (_currentOptions[i])
            {
                found_options.Copy(found_options | GetCell(i)._list_btst_connections[(int)_connectorEdge]);
            }
        }

        return found_options;
    }

    #endregion

    #region Helpers

    protected static int Get_Entropy(ref ModularMapCell modularMapCell)
    {
        int entropy = 0;

        for (int i = 0; i < modularMapCell._btst_options.Size(); i++)
        {
            if (modularMapCell._btst_options[i])
            {
                entropy++;
            }
        }

        return entropy;
    }

    protected static void Filter_OptionsTo_Options(Bitset btst_options, ref ModularMapCell modularMapCell)
    {
        modularMapCell._btst_options.Copy(modularMapCell._btst_options & btst_options);
    }

    // AreMapDimensionsPositive if the current Module has been collapsed
    protected static bool Is_Collapsed(ref ModularMapCell modularMapCell)
    {
        return modularMapCell._int_module != -1;
    }

    // Collapses the current Module into one of the options taking in consideration the weights of the objects
    protected static void Collapse_(ref ModularMapCell modularMapCell)
    {
        if (Is_Collapsed(ref modularMapCell))
        {
            Debug.LogError("Is Collapsed");
            return;
        }

        if (modularMapCell._btst_options.IsAllReset())
        {
            int fvalue = modularMapCell._btst_options.bits_get()[0];
            modularMapCell._int_module = 0;
            return;
        }

        List<int> found_modules = new List<int>();

        for (int index = 0; index < modularMapCell._btst_options.Size(); index++)
        {
            if (modularMapCell._btst_options[index])
            {
                found_modules.Add(index);
            }
        }

        if (found_modules.Count == 0) { return; }

        long random_index = (long)RandomNumber.NextMax((ulong)found_modules.Count);

        modularMapCell._int_module = found_modules[(int)random_index];

    }

    #endregion

}
