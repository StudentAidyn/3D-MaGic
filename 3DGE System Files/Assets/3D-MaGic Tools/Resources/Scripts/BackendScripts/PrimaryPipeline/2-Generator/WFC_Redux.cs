/*
  Wave Function Collapse

Wave function collapse is best used when you have minimal objects and being accurate with
generation is more important than speed.

Wave Function Collapse (at least this version) will always collapse the corner first. 
Then will adjust the surrounding modules to react to the new collapse and move through 
the map to find modules with the lowest entropy and force them to collapse. In short it
will collapse a module with the lowest entropy, then it will propogate from that collapse
to cause the surrounding modules to react, then repeat. 

This method of collapse is made much slower because of that propagation, since the 
propagation can become O(N) of its own causing the generation time to be O(N^2).

 */


using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public class WFC_Redux : MapGenerator
{
    #region VARIABLES
    
    private int m_totalModules;
    private int m_totalCurrentlyCollapsedModules;
    private int m_totalPropagations = 5;

    private HashSet<Vector3> m_hashedEntropyVectors = new HashSet<Vector3>();
    private HashSet<Vector3> m_hashedVectors = new HashSet<Vector3>();

    #endregion

    #region PUBLIC-METHODS

    public WFC_Redux() : base()
    {
        
    }

    public void SetTotalPropagations(int totalPropagations) => m_totalPropagations = totalPropagations;

    public override void Generate(Vector3 localMapDimensions, ref ModularMapCell[,,] mapArray, List<Cell> cellList)
    {
        Init(localMapDimensions, ref mapArray, cellList);

        // Collapse the corner first
        CollapseModule(Vector3.zero);
        Propagate(Vector3.zero);


        // Loops until the all Modules are collapsed - this is where the loop needs to be freed to properly Generate it correctly
        while (m_totalCurrentlyCollapsedModules < m_totalModules)
        {
            if (!Iterate())
            {
                return;
            }
        }
    }

    #endregion

    #region PRIVATE-METHODS

    protected override void Init(Vector3 mapSize, ref ModularMapCell[,,] mapArray, List<Cell> cells)
    {
        base.Init(mapSize, ref mapArray, cells);

        m_totalModules = (int)(
            mapSize.x *
            mapSize.y *
            mapSize.z);

        for (int y = 0; y < (int)mapSize.y; y++)
        {
            for (int z = 0; z < (int)mapSize.z; z++)
            {
                for (int x = 0; x < (int)mapSize.x; x++)
                {
                    m_hashedVectors.Add(new Vector3(x, y, z));
                }
            }
        }
    }

    // iterates through the WFC 
    private bool Iterate()
    {
        var coords = GetMinEntropyCoords();
        if (coords == null || coords.x == -1) return false;

        CollapseModule(coords);

        Propagate(coords);
        return true;
    }

    // finds and returns the location of **minimum entropy
    // **if more than 1 it will randomize between modules
    private Vector3 GetMinEntropyCoords()
    {
        double lowestEntropy = int.MaxValue; // sets lowest entropy to int Max to ensure the correct lowest entropy selection

        //if the entropy is 0 that means it only has 1 option left thus it is certain
        List<Vector3> lowestEntropyModules = new List<Vector3>();

        // Checking for lowest Entropy Map Module within a select Area

        if (m_hashedEntropyVectors.Count > 0)
        {
            foreach (Vector3 coord in m_hashedEntropyVectors)
            {
                ModularMapCell module = m_mapArray[(int)coord.x, (int)coord.y, (int)coord.z];

                if (!IsCellCollapsed(ref module))
                { 
                    // filters in only modules that aren't yet collapsed
                    int currentEntropy = GetCellEntropy(ref module);
                    if (currentEntropy < lowestEntropy)
                    { // finding the newest lowest entropy
                        lowestEntropyModules.Clear();
                        lowestEntropy = currentEntropy;
                    }

                    if (currentEntropy == lowestEntropy)
                    { 
                        // Checking for any modules with the same entropy
                        lowestEntropyModules.Add(coord);
                    }
                }
                else
                {
                    m_hashedEntropyVectors.Remove(coord);
                }
            }
        }
        else
        {
            foreach(Vector3 coord in m_hashedVectors)
            {
                ModularMapCell module = m_mapArray[(int)coord.x, (int)coord.y, (int)coord.z];

                if (!IsCellCollapsed(ref module))
                { 
                    // filters in only modules that aren't yet collapsed
                    int currentEntropy = GetCellEntropy(ref module);

                    if (currentEntropy < lowestEntropy)
                    { // finding the newest lowest entropy
                        lowestEntropyModules.Clear();
                        lowestEntropy = currentEntropy;
                    }

                    if (currentEntropy == lowestEntropy)
                    { // Checking for any modules with the same entropy
                        lowestEntropyModules.Add(coord);
                    }
                }
                else
                {
                    m_hashedVectors.Remove(coord);
                }
            }
        }


        //choosing on random if needed the returned module
        if (lowestEntropyModules.Count > 1)
        {
            // if there is more than one, select one at random
            ulong RandomVal = RandomNumber.Next() % (ulong)lowestEntropyModules.Count;
            return lowestEntropyModules[(int)RandomVal];
        }
        else if (lowestEntropyModules.Count == 0) return new Vector3(-1, -1);
        return lowestEntropyModules[0];
    }

    private void CollapseModule(Vector3 coords)
    {
        // Collapse the current Min Entropy
        CollapseCell(ref m_mapArray[(int)coords.x, (int)coords.y, (int)coords.z]);
        m_hashedEntropyVectors.Remove(coords);
        m_hashedVectors.Remove(coords);
        m_totalCurrentlyCollapsedModules++;
    }

    // Waves through all modules and adjusts all modules based on the current change
    public void Propagate(Vector3 _coords)
    {
        // New Propagation Model

        // Create open list
        List<Vector3> OpenList = new List<Vector3>();

        // Check around Module  
        ModularMapCell currentMod = GetModule(_coords);

        // Check Edges of the recently Collapsed Module

        // Horizontal - X
        if (CheckCollapsedModuleEdge(currentMod, _coords + new Vector3(1, 0, 0), ConnectorEdge.X))
        {
            Vector3 updatedCoord = _coords + new Vector3(1, 0, 0);
            OpenList.Add(updatedCoord);
        }
        if (CheckCollapsedModuleEdge(currentMod, _coords - new Vector3(1, 0, 0), ConnectorEdge.nX)) 
        {
            Vector3 updatedCoord = _coords - new Vector3(1, 0, 0);
            OpenList.Add(updatedCoord);
        }

        // Vertical - Y
        if (CheckCollapsedModuleEdge(currentMod, _coords + new Vector3(0, 1, 0), ConnectorEdge.Y)) 
        {
            Vector3 updatedCoord = _coords + new Vector3(0, 1, 0);
            OpenList.Add(updatedCoord);
        }
        if (CheckCollapsedModuleEdge(currentMod, _coords - new Vector3(0, 1, 0), ConnectorEdge.nY)) 
        {
            Vector3 updatedCoord = _coords - new Vector3(0, 1, 0);
            OpenList.Add(updatedCoord);
        }

        // Depth - Z
        if (CheckCollapsedModuleEdge(currentMod, _coords + new Vector3(0, 0, 1), ConnectorEdge.Z))
        {
            Vector3 updatedCoord = _coords + new Vector3(0, 0, 1);
            OpenList.Add(updatedCoord);
        }
        if (CheckCollapsedModuleEdge(currentMod,  _coords - new Vector3(0, 0, 1), ConnectorEdge.nZ))
        {
            Vector3 updatedCoord = _coords - new Vector3(0, 0, 1);
            OpenList.Add(updatedCoord);
        }




        // While the OpenList is empty Propagate
        while (OpenList.Count > 0)
        {
            // SetBitAtIndex a local variable and POP first element off openList
            var currentVec = OpenList[0];
            OpenList.RemoveAt(0);


            // Check around Module  
            currentMod = GetModule(currentVec);
            /*
             NOTE(Aidyn): Add mapping between ConnectorEdge and vector3 (+/-)
             */
            // X
            if((currentVec.x - _coords.x) < m_totalPropagations)
            {
                Vector3 addedHorizontalCoord = currentVec + new Vector3(1, 0, 0);
                if (CheckModuleEdge(currentMod, addedHorizontalCoord, ConnectorEdge.X))
                {
                    OpenList.Add(addedHorizontalCoord);
                }

                Vector3 subtractedHorizontalCoord = currentVec - new Vector3(1, 0, 0);
                if (CheckModuleEdge(currentMod, subtractedHorizontalCoord, ConnectorEdge.nX))
                {
                    OpenList.Add(subtractedHorizontalCoord);
                }
            }

            // Y
            if ((currentVec.y - _coords.y) < m_totalPropagations)
            {
                Vector3 addedVerticalCoord = currentVec + new Vector3(0, 1, 0);
                if (CheckModuleEdge(currentMod, addedVerticalCoord, ConnectorEdge.Y))
                {
                    OpenList.Add(addedVerticalCoord);
                }

                Vector3 subtractedVerticalCoord = currentVec - new Vector3(0, 1, 0);
                if (CheckModuleEdge(currentMod, subtractedVerticalCoord, ConnectorEdge.nY))
                {
                    OpenList.Add(subtractedVerticalCoord);
                }
            }

            //Z
            if ((currentVec.z - _coords.z) < m_totalPropagations)
            {
                Vector3 addedDepthCoord = currentVec + new Vector3(0, 0, 1);
                if (CheckModuleEdge(currentMod, addedDepthCoord, ConnectorEdge.Z))
                {
                    OpenList.Add(addedDepthCoord);
                }

                Vector3 subtractedDepthCoord = currentVec - new Vector3(0, 0, 1);
                if (CheckModuleEdge(currentMod, subtractedDepthCoord, ConnectorEdge.nZ))
                {
                    OpenList.Add(subtractedDepthCoord); 
                }
            }
        }

    }


    private bool CheckModuleEdge(ModularMapCell currentModule, Vector3 nextModuleCoordinate, ConnectorEdge comparingEdge)
    {
        bool removed = false;
        if (IsInMapBounds(nextModuleCoordinate))
        {
            ModularMapCell nextModularMapCell = GetModule(nextModuleCoordinate);
            if (!IsCellCollapsed(ref nextModularMapCell))
            {
                m_hashedEntropyVectors.Add(nextModuleCoordinate);
                // Attempts to Get the Module
                Bitset nextModuleOptions = new Bitset(nextModularMapCell.Options);
                Bitset options = GetAllEdgeOptionsFromEdge(comparingEdge, currentModule.Options);

                FilterOptionsToCellOptions(options, ref GetModule(nextModuleCoordinate));

                if (!Bitset.DoesBitsetMatchOther(nextModuleOptions, nextModularMapCell.Options))
                {
                    removed = true;
                }

            }
        }

        return removed;
    }

    private bool CheckCollapsedModuleEdge(ModularMapCell currentModule, Vector3 nextModuleCoordinate, ConnectorEdge comparingEdge)
    {
        bool removed = false;
        if (IsInMapBounds(nextModuleCoordinate))
        {
            ModularMapCell nextModularMapCell = GetModule(nextModuleCoordinate);
            if (!IsCellCollapsed(ref nextModularMapCell))
            {
                // Attempts to Get the Module
                Bitset nextModuleOptions = new Bitset(nextModularMapCell.Options);
                Bitset options = GetEdgeOptionsFromID(comparingEdge, currentModule.Module);

                FilterOptionsToCellOptions(options, ref GetModule(nextModuleCoordinate));

                if (!Bitset.DoesBitsetMatchOther(nextModuleOptions, nextModularMapCell.Options))
                {
                    removed = true;
                }
            }
        }
        return removed;
    }

    #endregion
}
