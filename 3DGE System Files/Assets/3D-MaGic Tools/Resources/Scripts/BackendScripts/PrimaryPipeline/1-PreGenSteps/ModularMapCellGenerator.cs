using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Newtonsoft.Json.Linq;

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
public class CellGenerator : ISaveable
{
    // Generated GetCells
    private List<Cell> m_cells = new();
    //private Dictionary<int, Cell> dict_int_cl_cells;

    public List<Cell> GetCells() => m_cells;

    // Total GetCells ~ can also be found out through counting m_cellsList list
    private int m_totalCells = 0;
    public int GetCellCount() => m_totalCells;

    private string m_fileName = "cells";



    public CellGenerator()
    {
    }

    #region PUBLIC-METHODS

    public void Init(List<ModularMapCellComponent> mapCellComponents)
    {
        GenerateCells(mapCellComponents);
    }


    public bool GenerateCells(List<ModularMapCellComponent> mapCellComponents)
    {
        if(mapCellComponents == null)
        {
            return false;
        }

        if (mapCellComponents.Count > 0)
        {
            Sort(ref mapCellComponents);
            CreateConnections(in mapCellComponents);
            DMG_SaveSystem.Save(this, m_fileName);
        }
        else
        {
            // Check for the presence for a cell save file
            if (DMG_SaveSystem.DoesFileExist(m_fileName))
            {
                DMG_SaveSystem.Load(this, m_fileName);
            }
            else
            {
                return false;
            }
        }

        if (m_cells.Count == 0)
        {
            Debug.LogError("NO CELLS GENERATED: " + m_cells.Count);
            return false;
        }

        mapCellComponents.Clear();
        return true;

    }

    #endregion

    #region PRIVATE-METHODS

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

    // Generate connections based on the connection rules - can Generate during editor (out of play state)
    private void CreateConnections(in List<ModularMapCellComponent> mapCellComponents)
    {
        ResetCells();
        sbyte totalRotations = 4;
        sbyte totalVariants = totalRotations;

        // GenerateMap GetCells
        for (int modulesIndex = 0; modulesIndex < mapCellComponents.Count; modulesIndex++)
        {
            ModularMapCellComponent currentModularCell = mapCellComponents[modulesIndex];
            if (!currentModularCell.NoVariants())
            {
                
                for (sbyte i = 0; i < totalVariants; i++)
                {
                    Cell cell = new();
                    cell.ID = m_totalCells;
                    cell.Layers = currentModularCell.GetLayerType();
                    cell.PrefabGameObject = currentModularCell.GetMesh();
                    cell.Rotation = (sbyte)((currentModularCell.GetRotation() + i) % totalRotations);

                    /* SIDES */
                    cell.ConnectionFront = currentModularCell.GetConnectionWithEdge((ConnectorEdge)(i % totalRotations));
                    cell.ConnectionRight = currentModularCell.GetConnectionWithEdge((ConnectorEdge)((i + 1) % totalRotations));
                    cell.ConnectionBack = currentModularCell.GetConnectionWithEdge((ConnectorEdge)((i + 2) % totalRotations));
                    cell.ConnectionLeft = currentModularCell.GetConnectionWithEdge((ConnectorEdge)((i + 3) % totalRotations));

                    cell.ConnectionUp = currentModularCell.GetConnectionWithEdge(ConnectorEdge.Y);
                    cell.ConnectionDown = currentModularCell.GetConnectionWithEdge(ConnectorEdge.nY);

                    if (cell.ConnectionUp.Property == ConnectorProperty.Rotational) cell.ConnectionUp.Rotation = (sbyte)((cell.ConnectionUp.Rotation + i) % 4);
                    if (cell.ConnectionDown.Property == ConnectorProperty.Rotational) cell.ConnectionDown.Rotation = (sbyte)((cell.ConnectionDown.Rotation + i) % 4);

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

                cell.ConnectionFront = currentModularCell.GetConnectionWithEdge(ConnectorEdge.Z);
                cell.ConnectionRight = currentModularCell.GetConnectionWithEdge(ConnectorEdge.X);
                cell.ConnectionBack = currentModularCell.GetConnectionWithEdge(ConnectorEdge.nZ);
                cell.ConnectionLeft = currentModularCell.GetConnectionWithEdge(ConnectorEdge.nX);

                cell.ConnectionUp = currentModularCell.GetConnectionWithEdge(ConnectorEdge.Y);
                cell.ConnectionDown = currentModularCell.GetConnectionWithEdge(ConnectorEdge.nY);

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

                if (CompareConnections(m_cells[currentCellIndex].ConnectionUp, m_cells[otherCellIndex].ConnectionDown))
                {
                    foundConnections[(int)ConnectorEdge.Y].SetBitAtIndex(m_cells[otherCellIndex].ID);
                }

                if (CompareConnections(m_cells[currentCellIndex].ConnectionFront, m_cells[otherCellIndex].ConnectionBack))
                {
                    foundConnections[(int)ConnectorEdge.Z].SetBitAtIndex(m_cells[otherCellIndex].ID);
                }

                if (CompareConnections(m_cells[currentCellIndex].ConnectionLeft, m_cells[otherCellIndex].ConnectionRight))
                {
                    foundConnections[(int)ConnectorEdge.nX].SetBitAtIndex(m_cells[otherCellIndex].ID);
                }

                if (CompareConnections(m_cells[currentCellIndex].ConnectionDown, m_cells[otherCellIndex].ConnectionUp))
                {
                    foundConnections[(int)ConnectorEdge.nY].SetBitAtIndex(m_cells[otherCellIndex].ID);
                }

                if (CompareConnections(m_cells[currentCellIndex].ConnectionBack, m_cells[otherCellIndex].ConnectionFront))
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
    private bool CompareConnections(Connection currentConnection, Connection comparedConnection)
    {
        // Check if both connections share the same Connector
        if (currentConnection.Connector == comparedConnection.Connector)
        {
            // Check edge properties
            // RULINGS:
            // 1) Check if both connections properties are EXACT
            if (currentConnection.Property == ConnectorProperty.Exact &&
                comparedConnection.Property == ConnectorProperty.Exact)
            {
                return true;
            }


            // 2) Check if both connections properties are OPPOSITES (FLIPPED A & B)
            if (currentConnection.Property == ConnectorProperty.FlippedA &&
                comparedConnection.Property == ConnectorProperty.FlippedB)
            {
                return true;
            }

            if (currentConnection.Property == ConnectorProperty.FlippedB &&
                comparedConnection.Property == ConnectorProperty.FlippedA)
            {
                return true;
            }


            // 3) Check if both connections are rotational, then check if they share the same rotation.
            if (currentConnection.Property == ConnectorProperty.Rotational &&
                comparedConnection.Property == ConnectorProperty.Rotational)
            {
                if(currentConnection.Rotation == comparedConnection.Rotation)
                {
                    return true;
                }
            }
        }

        // else return false
        return false;
    }

    #endregion

    #region SAVE-LOAD

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

    public JToken Save()
    {
        JObject state = new JObject();
        IDictionary<string, JToken> stateDict = state;
        stateDict["TotalCells"] = m_cells.Count;
        
        JObject cells = new JObject();
        IDictionary<string, JToken> cellsDict = cells;
        for (int index = 0; index < m_cells.Count; index++)
        {
            Cell cell = m_cells[index];
            string indexAsString = index.ToString();
            cellsDict[indexAsString] = cell.Save();
        }
        stateDict["CellList"] = cells;

        return state;
    }

    public void Load(JToken token)
    {
        if (token is JObject jObject)
        {
            IDictionary<string, JToken> stateDict = jObject;
            if (stateDict.TryGetValue("TotalCells", out JToken totalCells))
            {
                m_totalCells = totalCells.ToObject<int>();
                
            }

            if (stateDict.TryGetValue("CellList", out JToken cellList))
            {
                m_cells.Clear();
                IDictionary<string, JToken> cellDict = cellList.ToObject<IDictionary<string, JToken>>();
                for (int index = 0; index < m_totalCells; index++)
                {
                    string indexAsString = index.ToString();
                    if (cellDict.TryGetValue(indexAsString, out JToken cell))
                    {
                        Cell newCell = new Cell();
                        newCell.Load(cell);
                        m_cells.Add(newCell);
                    }
                }
            }
        }
    }

    #endregion
}



