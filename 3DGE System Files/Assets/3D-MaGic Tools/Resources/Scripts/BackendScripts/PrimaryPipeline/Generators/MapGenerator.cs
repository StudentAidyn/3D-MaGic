using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;

public class MapGenerator
{
    #region VARIABLES
    
    protected ModularMapCell[,,] m_mapArray;
    protected List<Cell> m_cellsList;
    protected Vector3 m_dimensions;

    #endregion
    public MapGenerator()
    {
    }

    #region PUBLIC-METHODS

    public ModularMapCell[,,] GetMap() { return m_mapArray; }

    // GenerateMap Function
    public virtual void Generate(Vector3 dimensions, ref ModularMapCell[,,] mapArray, List<Cell> cellsList) { }


    #endregion

    #region PRIVATE-METHODS
    protected virtual void Init(Vector3 dimensions, ref ModularMapCell[,,] mapArray, List<Cell> cellsList)
    {
        m_dimensions = dimensions;
        m_cellsList = cellsList;

        m_mapArray = mapArray;
        if(m_mapArray == null)
        {
            SetUpMapArray();
        }
    }

    protected void SetUpMapArray()
    {
        m_mapArray = new ModularMapCell[
                (int)m_dimensions.x,
                (int)m_dimensions.y,
                (int)m_dimensions.z];

        int bitsetSize = m_cellsList.Count;

        for (int z = 0; z < m_dimensions.z; z++)
        {
            for (int y = 0; y < m_dimensions.y; y++)
            {
                for (int x = 0; x < m_dimensions.x; x++)
                {
                    m_mapArray[x, y, z] = new ModularMapCell(bitsetSize);
                }
            }
        }
    }

    protected ref ModularMapCell GetModule(Vector3 mapPosition)
    {
        return ref m_mapArray[(int)mapPosition.x, (int)mapPosition.y, (int)mapPosition.z];
    }


    #region Cell Logic

    protected Cell GetCellFromID(int indexID)
    {
        if (indexID < 0 || m_cellsList.Count < indexID)
        {
            return null;
        }

        return m_cellsList[indexID];
    }

    // Get Collapsed Object Options
    protected Bitset GetEdgeOptionsFromID(ConnectorEdge connectorEdge, int indexID)
    {
        Cell modCell = GetCellFromID(indexID);
        if (modCell != null)
        {
            return modCell.Connections[(int)connectorEdge];
        }
        return new Bitset(0);
    }

    // Non-Collapsed Object Options
    protected Bitset GetAllEdgeOptionsFromEdge(ConnectorEdge connectorEdge, Bitset currentOptions)
    {
        // create new BitsetArray to store new found options => ensure it is ResetBitAtIndex upon creation
        Bitset foundOptions = new Bitset(currentOptions.Size());
        foundOptions.ResetAllBits();

        // find and fill options
        for (int i = 0; i < currentOptions.Size(); i++)
        {
            if (currentOptions[i])
            {
                foundOptions.Copy(foundOptions | GetCellFromID(i).Connections[(int)connectorEdge]);
            }
        }

        return foundOptions;
    }

    #endregion

    #region Helpers

    protected static int GetCellEntropy(ref ModularMapCell modularMapCell)
    {
        int entropy = 0;

        for (int i = 0; i < modularMapCell.Options.Size(); i++)
        {
            if (modularMapCell.Options[i])
            {
                entropy++;
            }
        }

        return entropy;
    }

    protected static void FilterOptionsToCellOptions(Bitset options, ref ModularMapCell modularMapCell)
    {
        modularMapCell.Options.Copy(modularMapCell.Options & options);
    }

    // AreMapDimensionsPositive if the current Module has been collapsed
    protected static bool IsCellCollapsed(ref ModularMapCell modularMapCell)
    {
        return modularMapCell.Module != -1;
    }

    // Collapses the current Module into one of the options taking in consideration the weights of the objects
    protected static void CollapseCell(ref ModularMapCell modularMapCell)
    {
        if (IsCellCollapsed(ref modularMapCell))
        {
            Debug.LogError("Is Collapsed");
            return;
        }

        if (modularMapCell.Options.IsAllReset())
        {
            modularMapCell.Module = 0;
            return;
        }

        List<int> foundModules = new List<int>();

        for (int index = 0; index < modularMapCell.Options.Size(); index++)
        {
            if (modularMapCell.Options[index])
            {
                foundModules.Add(index);
            }
        }

        if (foundModules.Count == 0) { return; }

        long randomIndex = (long)RandomNumber.NextMax((ulong)foundModules.Count);

        modularMapCell.Module = foundModules[(int)randomIndex];

    }

    protected bool IsInVectorBounds(Vector3 vectorToTest, Vector3 vectorMax)
    {
        return (
            (vectorToTest.x >= 0 && vectorToTest.x < vectorMax.x) &&
            (vectorToTest.y >= 0 && vectorToTest.y < vectorMax.y) &&
            (vectorToTest.z >= 0 && vectorToTest.z < vectorMax.z)
            );
    }

    protected bool IsInMapBounds(Vector3 vectorToTest)
    {
        return IsInVectorBounds(vectorToTest, m_dimensions);
    }


    #endregion

    #endregion
}
