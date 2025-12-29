using System.IO;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Collections;


[System.Serializable]
public struct CellGroupData
{
    public int _int_totalCells;
    public List<CellData> _lst_cd_cellData;
}

[System.Serializable]
public struct CellData
{
    public int _int_objectID;
    public GameObject _go_gameObject;
    public sbyte _sbyte_rotation;
    public LayerTypes _lt_layerTypes;
    public BitsetData[] _list_bsdt_connections;

}


[ExecuteInEditMode]
public class CellGenerator
{
    // VARIABLES *************************************************************************************************
    // ***********************************************************************************************************

    // Generated Cells
    public List<Cell> _lst_cells = new();
    //private Dictionary<int, Cell> dict_int_cl_cells;

    public List<Cell> Cells() => _lst_cells;

    // Total Cells ~ can also be found out through counting _cellsList list
    private int int_totalCells = 0;
    public int CellCount() => int_totalCells;


    // ***********************************************************************************************************
    // ***********************************************************************************************************


    public CellGenerator()
    {
    }

    public void Init(ref List<ModularMapCellComponent> _lst_mapCellComponents)
    {
        GenerateCells(ref _lst_mapCellComponents);
    }


    public bool GenerateCells(ref List<ModularMapCellComponent> _lst_mapCellComponents)
    {
        if (_lst_mapCellComponents.Count > 0)
        {
            Sort(ref _lst_mapCellComponents);
            CreateConnections(in _lst_mapCellComponents);
            DMG_SaveSystem.SaveCells();
        }
        else
        {
            // Check for the presence for a cell save file
            if (DMG_SaveSystem.HasCellDataFile())
            {
                DMG_SaveSystem.LoadCell();
            }
            else
            {
                return false;
            }
        }

        if (_lst_cells.Count == 0)
        {
            Debug.LogError("NO CELLS GENERATED");
            return false;
        }

        _lst_mapCellComponents.Clear();
        return true;

    }

    private void Sort(ref List<ModularMapCellComponent> _lst_mapCellComponents)
    {
        _lst_mapCellComponents = _lst_mapCellComponents.OrderBy(item => ((item.GetMesh() != null) ? item.GetMesh().name : " ")).ToList();
    }
    
    private void ResetCells()
    {
        // Reset Connections
        _lst_cells.Clear();
        int_totalCells = 0;
    }



    // GENERATOR *************************************************************************************************
    // ***********************************************************************************************************

    // generate connections based on the connection rules - can generate during editor (out of play state)
    private void CreateConnections(in List<ModularMapCellComponent> _lst_mapCellComponents)
    {
        ResetCells();


        // GenerateMap Cells
        for (int modules_index = 0; modules_index < _lst_mapCellComponents.Count; modules_index++)
        {
            ModularMapCellComponent current_modular_cell = _lst_mapCellComponents[modules_index];
            if (!current_modular_cell.NoVariants())
            {
                for (sbyte i = 0; i < 4; i++)
                {
                    Cell cell = new();
                    cell._int_objectID = int_totalCells;
                    cell._lt_layerTypes = current_modular_cell.GetLayerType();
                    cell._go_gameObject = current_modular_cell.GetMesh();
                    cell._sbyte_rotation = (sbyte)((current_modular_cell.GetRotation() + i) % 4);

                    cell._con_posZ = current_modular_cell.GetConnectionWith_((connector_edge)(i % 4));
                    cell._con_posX = current_modular_cell.GetConnectionWith_((connector_edge)((i + 1) % 4));
                    cell._con_negZ = current_modular_cell.GetConnectionWith_((connector_edge)((i + 2) % 4));
                    cell._con_negX = current_modular_cell.GetConnectionWith_((connector_edge)((i + 3) % 4));

                    // DISPLAY CONNECTION TYPES
                    //Debug.Log(cell._con_posZ._connector + " || " + cell._con_posX._connector + " || " + cell._con_negZ._connector + " || " + cell._con_negX._connector);

                    cell._con_posY = current_modular_cell.GetConnectionWith_(connector_edge.Y);
                    cell._con_negY = current_modular_cell.GetConnectionWith_(connector_edge.nY);

                    if (cell._con_posY._property == ConnectorProperty.Rotational) cell._con_posY._rotation = (sbyte)((cell._con_posY._rotation + i) % 4);
                    if (cell._con_negY._property == ConnectorProperty.Rotational) cell._con_negY._rotation = (sbyte)((cell._con_negY._rotation + i) % 4);

                    _lst_cells.Add(cell);

                    int_totalCells++;
                }
            }
            else
            {
                Cell cell = new Cell();
                cell._int_objectID = int_totalCells;
                cell._lt_layerTypes = current_modular_cell.GetLayerType();
                cell._go_gameObject = current_modular_cell.GetMesh();
                cell._sbyte_rotation = 0;

                cell._con_posZ = current_modular_cell.GetConnectionWith_(connector_edge.Z);
                cell._con_posX = current_modular_cell.GetConnectionWith_(connector_edge.X);
                cell._con_negZ = current_modular_cell.GetConnectionWith_(connector_edge.nZ);
                cell._con_negX = current_modular_cell.GetConnectionWith_(connector_edge.nX);

                cell._con_posY = current_modular_cell.GetConnectionWith_(connector_edge.Y);
                cell._con_negY = current_modular_cell.GetConnectionWith_(connector_edge.nY);

                _lst_cells.Add(cell);
                int_totalCells++;
            }
        }

        // Compare_IsSame each CELL to ALL OTHER CELLS
        int cell_size = _lst_cells.Count;
        for (int current_cell_index = 0; current_cell_index < cell_size; current_cell_index++) {
            List<Bitset> found_connections = new List<Bitset>();
            for(int i = 0; i < 6; i++)
            {
                Bitset new_bitset = new Bitset(cell_size);
                found_connections.Add(new_bitset);
            }
            for (int other_cell_index = 0; other_cell_index < cell_size; other_cell_index++) {

                // compare cell to all its sides and opposite sides to test for connections.
                if (CompareConnections(_lst_cells[current_cell_index]._con_posX, _lst_cells[other_cell_index]._con_negX))
                {
                    found_connections[(int)connector_edge.X].set(_lst_cells[other_cell_index]._int_objectID);
                }

                if (CompareConnections(_lst_cells[current_cell_index]._con_posY, _lst_cells[other_cell_index]._con_negY))
                {
                    found_connections[(int)connector_edge.Y].set(_lst_cells[other_cell_index]._int_objectID);
                }

                if (CompareConnections(_lst_cells[current_cell_index]._con_posZ, _lst_cells[other_cell_index]._con_negZ))
                {
                    found_connections[(int)connector_edge.Z].set(_lst_cells[other_cell_index]._int_objectID);
                }

                if (CompareConnections(_lst_cells[current_cell_index]._con_negX, _lst_cells[other_cell_index]._con_posX))
                {
                    found_connections[(int)connector_edge.nX].set(_lst_cells[other_cell_index]._int_objectID);
                }

                if (CompareConnections(_lst_cells[current_cell_index]._con_negY, _lst_cells[other_cell_index]._con_posY))
                {
                    found_connections[(int)connector_edge.nY].set(_lst_cells[other_cell_index]._int_objectID);
                }

                if (CompareConnections(_lst_cells[current_cell_index]._con_negZ, _lst_cells[other_cell_index]._con_posZ))
                {
                    found_connections[(int)connector_edge.nZ].set(_lst_cells[other_cell_index]._int_objectID);
                }

            }

            for (int i = 0; i < 6; i++)
            {
                _lst_cells[current_cell_index]._list_btst_connections.Add(found_connections[i]);
            }
            
        }

        Debug.Log("EdgesCreated");
    }

    // Compares 2 Edges passed through based on the rules given 
    bool CompareConnections(Connection _currentConnection, Connection _comparedConnection)
    {
        // Check if both connections share the same Connector
        if (_currentConnection._connector == _comparedConnection._connector)
        {
            // Check edge properties
            // RULINGS:
            // 1) Check if both connections properties are EXACT
            if (_currentConnection._property == ConnectorProperty.Exact &&
                _comparedConnection._property == ConnectorProperty.Exact)
            {
                return true;
            }


            // 2) Check if both connections properties are OPPOSITES (FLIPPED A & B)
            if (_currentConnection._property == ConnectorProperty.FlippedA &&
                _comparedConnection._property == ConnectorProperty.FlippedB)
            {
                return true;
            }

            if (_currentConnection._property == ConnectorProperty.FlippedB &&
                _comparedConnection._property == ConnectorProperty.FlippedA)
            {
                return true;
            }


            // 3) Check if both connections are rotational, then check if they share the same rotation.
            if (_currentConnection._property == ConnectorProperty.Rotational &&
                _comparedConnection._property == ConnectorProperty.Rotational)
            {
                if(_currentConnection._rotation == _comparedConnection._rotation)
                {
                    return true;
                }
            }
        }

        // else return false
        return false;
    }

    // ***********************************************************************************************************
    // ***********************************************************************************************************






    // SAVE & LOAD ***********************************************************************************************
    // ***********************************************************************************************************

    public void Save(ref CellGroupData data)
    {
        data._int_totalCells = _lst_cells.Count;
        data._lst_cd_cellData = new();
        foreach (Cell cell in _lst_cells)
        {
            // Cell Data
            CellData cell_data = new CellData();
            cell.Save(ref cell_data);

            data._lst_cd_cellData.Add(cell_data);
        }
    }

    public void Load(CellGroupData data)
    {
        ResetCells();

        int_totalCells = data._int_totalCells;

        foreach (var cell in data._lst_cd_cellData)
        {
            Cell new_cell = new Cell();
            new_cell.Load(cell);
            _lst_cells.Add(new_cell);
        }

        Debug.Log("LOADED NEW CELLS");
    }

    // ***********************************************************************************************************
    // ***********************************************************************************************************
}



