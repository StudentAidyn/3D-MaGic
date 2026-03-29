using System.IO;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Collections;


[System.Serializable]
public struct CellGroupData
{
    public int TotalCells;
    public List<MicroCell> MicroCells;
}

[System.Serializable]
public struct MicroCell
{
    public int ID;
    public GameObject PrefabGameObject;
    public sbyte Rotation;
    public LayerTypes Layers;
    public BitsetData[] EdgeConnections;

}


[ExecuteInEditMode]
public class CellGenerator
{
    // VARIABLES *************************************************************************************************
    // ***********************************************************************************************************

    // Generated GetCells
    private List<Cell> m_cells = new();
    //private Dictionary<int, Cell> dict_int_cl_cells;

    public List<Cell> GetCells() => m_cells;

    // Total GetCells ~ can also be found out through counting _cellsList list
    private int m_totalCells = 0;
    public int GetCellCount() => m_totalCells;


    // ***********************************************************************************************************
    // ***********************************************************************************************************


    public CellGenerator()
    {
    }

    public void Init(ref List<ModularMapCellComponent> mapCellComponents)
    {
        GenerateCells(ref mapCellComponents);
    }


    public bool GenerateCells(ref List<ModularMapCellComponent> mapCellComponents)
    {
        if (mapCellComponents.Count > 0)
        {
            Sort(ref mapCellComponents);
            CreateConnections(in mapCellComponents);
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

        if (m_cells.Count == 0)
        {
            Debug.LogError("NO CELLS GENERATED");
            return false;
        }

        mapCellComponents.Clear();
        return true;

    }

    private void Sort(ref List<ModularMapCellComponent> mapCellComponents)
    {
        mapCellComponents = mapCellComponents.OrderBy(item => ((item.GetMesh() != null) ? item.GetMesh().name : " ")).ToList();
    }
    
    private void ResetCells()
    {
        // Reset Connections
        m_cells.Clear();
        m_totalCells = 0;
    }



    // GENERATOR *************************************************************************************************
    // ***********************************************************************************************************

    // generate connections based on the connection rules - can generate during editor (out of play state)
    private void CreateConnections(in List<ModularMapCellComponent> mapCellComponents)
    {
        ResetCells();


        // GenerateMap GetCells
        for (int modulesIndex = 0; modulesIndex < mapCellComponents.Count; modulesIndex++)
        {
            ModularMapCellComponent currentModularCell = mapCellComponents[modulesIndex];
            if (!currentModularCell.NoVariants())
            {
                for (sbyte i = 0; i < 4; i++)
                {
                    Cell cell = new();
                    cell.ID = m_totalCells;
                    cell.Layers = currentModularCell.GetLayerType();
                    cell.PrefabGameObject = currentModularCell.GetMesh();
                    cell.Rotation = (sbyte)((currentModularCell.GetRotation() + i) % 4);

                    cell.ConnectionUp = currentModularCell.GetConnectionWith_((ConnectorEdge)(i % 4));
                    cell.ConnectionRight = currentModularCell.GetConnectionWith_((ConnectorEdge)((i + 1) % 4));
                    cell.ConnectionDown = currentModularCell.GetConnectionWith_((ConnectorEdge)((i + 2) % 4));
                    cell.ConnectionLeft = currentModularCell.GetConnectionWith_((ConnectorEdge)((i + 3) % 4));

                    // DISPLAY CONNECTION TYPES
                    //Debug.Log(cell.ConnectionUp._connector + " || " + cell.ConnectionRight._connector + " || " + cell.ConnectionDown._connector + " || " + cell.ConnectionLeft._connector);

                    cell.ConnectionFront = currentModularCell.GetConnectionWith_(ConnectorEdge.Y);
                    cell.ConnectionBack = currentModularCell.GetConnectionWith_(ConnectorEdge.nY);

                    if (cell.ConnectionFront._property == ConnectorProperty.Rotational) cell.ConnectionFront._rotation = (sbyte)((cell.ConnectionFront._rotation + i) % 4);
                    if (cell.ConnectionBack._property == ConnectorProperty.Rotational) cell.ConnectionBack._rotation = (sbyte)((cell.ConnectionBack._rotation + i) % 4);

                    m_cells.Add(cell);

                    m_totalCells++;
                }
            }
            else
            {
                Cell cell = new Cell();
                cell.ID = m_totalCells;
                cell.Layers = currentModularCell.GetLayerType();
                cell.PrefabGameObject = currentModularCell.GetMesh();
                cell.Rotation = 0;

                cell.ConnectionUp = currentModularCell.GetConnectionWith_(ConnectorEdge.Z);
                cell.ConnectionRight = currentModularCell.GetConnectionWith_(ConnectorEdge.X);
                cell.ConnectionDown = currentModularCell.GetConnectionWith_(ConnectorEdge.nZ);
                cell.ConnectionLeft = currentModularCell.GetConnectionWith_(ConnectorEdge.nX);

                cell.ConnectionFront = currentModularCell.GetConnectionWith_(ConnectorEdge.Y);
                cell.ConnectionBack = currentModularCell.GetConnectionWith_(ConnectorEdge.nY);

                m_cells.Add(cell);
                m_totalCells++;
            }
        }

        // DoesBitsetMatchOther each CELL to ALL OTHER CELLS
        int cellSize = m_cells.Count;
        for (int currentCellIndex = 0; currentCellIndex < cellSize; currentCellIndex++) {
            List<Bitset> foundConnections = new List<Bitset>();
            for(int i = 0; i < 6; i++)
            {
                Bitset newBitset = new Bitset(cellSize);
                foundConnections.Add(newBitset);
            }
            for (int otherCellIndex = 0; otherCellIndex < cellSize; otherCellIndex++) {

                // compare cell to all its sides and opposite sides to test for connections.
                // TODO => TURN THIS INTO A FUNCTION TO KISS AND DRY
                if (CompareConnections(m_cells[currentCellIndex].ConnectionRight, m_cells[otherCellIndex].ConnectionLeft))
                {
                    foundConnections[(int)ConnectorEdge.X].SetBitAtIndex(m_cells[otherCellIndex].ID);
                }

                if (CompareConnections(m_cells[currentCellIndex].ConnectionFront, m_cells[otherCellIndex].ConnectionBack))
                {
                    foundConnections[(int)ConnectorEdge.Y].SetBitAtIndex(m_cells[otherCellIndex].ID);
                }

                if (CompareConnections(m_cells[currentCellIndex].ConnectionUp, m_cells[otherCellIndex].ConnectionDown))
                {
                    foundConnections[(int)ConnectorEdge.Z].SetBitAtIndex(m_cells[otherCellIndex].ID);
                }

                if (CompareConnections(m_cells[currentCellIndex].ConnectionLeft, m_cells[otherCellIndex].ConnectionRight))
                {
                    foundConnections[(int)ConnectorEdge.nX].SetBitAtIndex(m_cells[otherCellIndex].ID);
                }

                if (CompareConnections(m_cells[currentCellIndex].ConnectionBack, m_cells[otherCellIndex].ConnectionFront))
                {
                    foundConnections[(int)ConnectorEdge.nY].SetBitAtIndex(m_cells[otherCellIndex].ID);
                }

                if (CompareConnections(m_cells[currentCellIndex].ConnectionDown, m_cells[otherCellIndex].ConnectionUp))
                {
                    foundConnections[(int)ConnectorEdge.nZ].SetBitAtIndex(m_cells[otherCellIndex].ID);
                }

            }

            for (int i = 0; i < 6; i++)
            {
                m_cells[currentCellIndex].Connections.Add(foundConnections[i]);
            }
            
        }
    }

    // Compares 2 Edges passed through based on the rules given 
    bool CompareConnections(Connection currentConnection, Connection comparedConnection)
    {
        // Check if both connections share the same Connector
        if (currentConnection._connector == comparedConnection._connector)
        {
            // Check edge properties
            // RULINGS:
            // 1) Check if both connections properties are EXACT
            if (currentConnection._property == ConnectorProperty.Exact &&
                comparedConnection._property == ConnectorProperty.Exact)
            {
                return true;
            }


            // 2) Check if both connections properties are OPPOSITES (FLIPPED A & B)
            if (currentConnection._property == ConnectorProperty.FlippedA &&
                comparedConnection._property == ConnectorProperty.FlippedB)
            {
                return true;
            }

            if (currentConnection._property == ConnectorProperty.FlippedB &&
                comparedConnection._property == ConnectorProperty.FlippedA)
            {
                return true;
            }


            // 3) Check if both connections are rotational, then check if they share the same rotation.
            if (currentConnection._property == ConnectorProperty.Rotational &&
                comparedConnection._property == ConnectorProperty.Rotational)
            {
                if(currentConnection._rotation == comparedConnection._rotation)
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
        data.TotalCells = m_cells.Count;
        data.MicroCells = new();
        foreach (Cell cell in m_cells)
        {
            // Cell Data
            MicroCell microCell = new MicroCell();
            cell.Save(ref microCell);

            data.MicroCells.Add(microCell);
        }
    }

    public void Load(CellGroupData data)
    {
        ResetCells();

        m_totalCells = data.TotalCells;

        foreach (var cell in data.MicroCells)
        {
            Cell newCell = new Cell();
            newCell.Load(cell);
            m_cells.Add(newCell);
        }

        Debug.Log("LOADED NEW CELLS");
    }

    // ***********************************************************************************************************
    // ***********************************************************************************************************
}



