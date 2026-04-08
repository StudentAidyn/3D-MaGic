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

public class WaveFunctionCollapse : MapGenerator
{
    private int m_totalModules;
    private int _totalCurrentlyCollapsedModules;

    public WaveFunctionCollapse() : base()
    {
        
    }

    protected override void Init(Vector3 mapSize, List<Cell> cells)
    {
        base.Init(mapSize, cells);

        m_totalModules = (int)(
            mapSize.x *
            mapSize.y *
            mapSize.z); 
    }

    public override void Generate(Vector3 localMapDimensions, List<Cell> cellList)
    {
        Init(localMapDimensions, cellList);

        // Collapse the corner first
        CollapseModule(Vector3.zero);
        Propagate(Vector3.zero);


        // Loops until the all Modules are collapsed - this is where the loop needs to be freed to properly generate it correctly
        while (_totalCurrentlyCollapsedModules < m_totalModules)
        {
            if (!Iterate())
            {
                return;
            }
        }
    }


    /// this WFC cycles through the whole array every time,
    /// using overlapping chunks will help with performance and accuracy.
    public override async Task GenerateAsync(Vector3 localMapDimensions, List<Cell> cellList)
    {
        Init(localMapDimensions, cellList);

        // Collapse the corner first
        CollapseModule(Vector3.zero);
        Propagate(Vector3.zero);


        // Loops until the all Modules are collapsed - this is where the loop needs to be freed to properly generate it correctly
        while (_totalCurrentlyCollapsedModules < m_totalModules)
        {
            if (!Iterate())
            {
                return;
            }
            await Task.Yield();
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

    // finds and returns the location of *minimum entropy
    // *if more than 1 it will randomize between modules
    Vector3 GetMinEntropyCoords()
    {
        double _lowestEntropy = int.MaxValue; // sets lowest entropy to int Max to ensure the correct lowest entropy selection

        //if the entropy is 0 that means it only has 1 option left thus it is certain
        List<Vector3> lowestEntropyModules = new List<Vector3>();

        // Checking for lowest Entropy Map Module within a select Area

        // IDEA(Aidyn): To optimise this, maybe make a function to look for the area around the last collapsed object
        // IDEA(Aidyn): To optimise this more, create a list of affected options... then use that to search reducing the search results.

        for (int y = 0; y < (int)m_dimensions.y; y++)
        {
            for (int z = 0; z < (int)m_dimensions.z; z++)
            {
                for (int x = 0; x < (int)m_dimensions.x; x++)
                {
                    ModularMapCell module = m_mapArray[x, y, z];
                    if (!IsCellCollapsed(ref module))
                    { // filters in only modules that aren't yet collapsed
                        int current_found_entropy = GetCellEntropy(ref module);
                        if (current_found_entropy < _lowestEntropy)
                        { // finding the newest lowest entropy
                            lowestEntropyModules.Clear();
                            _lowestEntropy = current_found_entropy;
                        }

                        if (current_found_entropy == _lowestEntropy)
                        { // Checking for any modules with the same entropy
                            lowestEntropyModules.Add(new Vector3(x, y, z));
                        }
                    }
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
        _totalCurrentlyCollapsedModules++;
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

        // X
        if (CheckCollapsedModuleEdge(currentMod, _coords + new Vector3(1, 0, 0), ConnectorEdge.X)) OpenList.Add(_coords + new Vector3(1, 0, 0));
        if (CheckCollapsedModuleEdge(currentMod, _coords - new Vector3(1, 0, 0), ConnectorEdge.nX)) OpenList.Add(_coords - new Vector3(1, 0, 0));

        // Y
        if (CheckCollapsedModuleEdge(currentMod, _coords + new Vector3(0, 1, 0), ConnectorEdge.Y)) OpenList.Add(_coords + new Vector3(0, 1, 0));
        if (CheckCollapsedModuleEdge(currentMod, _coords - new Vector3(0, 1, 0), ConnectorEdge.nY)) OpenList.Add(_coords - new Vector3(0, 1, 0));

        //Z
        if (CheckCollapsedModuleEdge(currentMod, _coords + new Vector3(0, 0, 1), ConnectorEdge.Z)) OpenList.Add(_coords + new Vector3(0, 0, 1));
        if (CheckCollapsedModuleEdge(currentMod, _coords - new Vector3(0, 0, 1), ConnectorEdge.nZ)) OpenList.Add(_coords - new Vector3(0, 0, 1));




        // While the OpenList is empty Propagate
        while (OpenList.Count > 0)
        {
            // SetBitAtIndex a local variable and POP first element off openList
            var currentVec = OpenList[0];
            OpenList.RemoveAt(0);


            // Check around Module  
            currentMod = GetModule(currentVec);

            // X
            if (CheckModuleEdge(currentMod, currentVec + new Vector3(1, 0, 0), ConnectorEdge.X)) OpenList.Add(currentVec + new Vector3(1, 0, 0));
            if (CheckModuleEdge(currentMod, currentVec - new Vector3(1, 0, 0), ConnectorEdge.nX)) OpenList.Add(currentVec - new Vector3(1, 0, 0));

            // Y
            if (CheckModuleEdge(currentMod, currentVec + new Vector3(0, 1, 0), ConnectorEdge.Y)) OpenList.Add(currentVec + new Vector3(0, 1, 0));
            if (CheckModuleEdge(currentMod, currentVec - new Vector3(0, 1, 0), ConnectorEdge.nY)) OpenList.Add(currentVec - new Vector3(0, 1, 0));

            //Z
            if (CheckModuleEdge(currentMod, currentVec + new Vector3(0, 0, 1), ConnectorEdge.Z)) OpenList.Add(currentVec + new Vector3(0, 0, 1));
            if (CheckModuleEdge(currentMod, currentVec - new Vector3(0, 0, 1), ConnectorEdge.nZ)) OpenList.Add(currentVec - new Vector3(0, 0, 1));
        }

    }


    private bool CheckModuleEdge(ModularMapCell current_module, Vector3 next_module_coordinate, ConnectorEdge _comparingEdge)
    {
        bool removed = false;
        if (IsInMapBounds(next_module_coordinate))
        {
            ModularMapCell next_modular_map_cell = GetModule(next_module_coordinate);
            if (!IsCellCollapsed(ref next_modular_map_cell))
            {
                // Attempts to Get the Module
                Bitset next_module_options = new Bitset(next_modular_map_cell.Options);
                Bitset options = GetAllEdgeOptionsFromID(_comparingEdge, current_module.Options);
                FilterOptionsToCellOptions(options, ref GetModule(next_module_coordinate));



                if (!Bitset.DoesBitsetMatchOther(next_module_options, next_modular_map_cell.Options))
                {
                    removed = true;
                }

            }
        }

        return removed;
    }

    private bool CheckCollapsedModuleEdge(ModularMapCell current_module, Vector3 next_module_coordinate, ConnectorEdge _comparingEdge)
    {
        bool removed = false;
        if (IsInMapBounds(next_module_coordinate))
        {
            ModularMapCell next_modular_map_cell = GetModule(next_module_coordinate);
            if (!IsCellCollapsed(ref next_modular_map_cell))
            {
                // Attempts to Get the Module
                Bitset next_module_options = new Bitset(next_modular_map_cell.Options);
                Bitset options = GetEdgeOptionsFromID(_comparingEdge, current_module.Module);
                FilterOptionsToCellOptions(options, ref GetModule(next_module_coordinate));
                if (!Bitset.DoesBitsetMatchOther(next_module_options, next_modular_map_cell.Options))
                {
                    removed = true;
                }


            }

        }

        return removed;
    }

}
