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
using System.Linq;

public class WFC_Redux : MapGenerator
{
    private int _totalModules;
    private int _totalCurrentlyCollapsedModules;
    private int _totalPropagations = 5;
    HashSet<Vector3> _hashedEntropyVectors = new HashSet<Vector3>();
    HashSet<Vector3> _hashedVectors = new HashSet<Vector3>();

    public WFC_Redux() : base()
    {
        
    }

    public void SetTotalPropagations(int totalPropagations) => _totalPropagations = totalPropagations;

    protected override void Init(Vector3 _vec3_mapSize, List<Cell> _cells)
    {
        base.Init(_vec3_mapSize, _cells);

        _totalModules = (int)(
            _vec3_mapSize.x *
            _vec3_mapSize.y *
            _vec3_mapSize.z);

        for (int y = 0; y < (int)_vec3_mapSize.y; y++)
        {
            for (int z = 0; z < (int)_vec3_mapSize.z; z++)
            {
                for (int x = 0; x < (int)_vec3_mapSize.x; x++)
                {
                    _hashedVectors.Add(new Vector3(x, y, z));
                }
            }
        }
    }

    public override void Generate(Vector3 localMapDimensions, List<Cell> cellList)
    {
        Init(localMapDimensions, cellList);

        // Collapse the corner first
        CollapseModule(Vector3.zero);
        Propagate(Vector3.zero, localMapDimensions);


        // Loops until the all Modules are collapsed - this is where the loop needs to be freed to properly generate it correctly
        while (_totalCurrentlyCollapsedModules < _totalModules)
        {
            if (!Iterate(localMapDimensions))
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
        Propagate(Vector3.zero, localMapDimensions);


        // Loops until the all Modules are collapsed - this is where the loop needs to be freed to properly generate it correctly
        while (_totalCurrentlyCollapsedModules < _totalModules)
        {
            if (!Iterate(localMapDimensions))
            {
                return;
            }
            await Task.Yield();
        }
    }

    // iterates through the WFC 
    private bool Iterate(Vector3 _localSize)
    {
        var coords = GetMinEntropyCoords(_localSize);
        if (coords == null || coords.x == -1) return false;

        CollapseModule(coords);

        Propagate(coords, _localSize);
        return true;
    }

    // finds and returns the location of *minimum entropy
    // *if more than 1 it will randomize between modules
    Vector3 GetMinEntropyCoords(Vector3 _localSize)
    {
        double _lowestEntropy = int.MaxValue; // sets lowest entropy to int Max to ensure the correct lowest entropy selection

        //if the entropy is 0 that means it only has 1 option left thus it is certain
        List<Vector3> lowestEntropyModules = new List<Vector3>();

        // Checking for lowest Entropy Map Module within a select Area

        if (_hashedEntropyVectors.Count > 0)
        {
            foreach (Vector3 coord in _hashedEntropyVectors)
            {
                ModularMapCell module = _mapArray[(int)coord.x, (int)coord.y, (int)coord.z];
                if (!Is_Collapsed(ref module))
                { // filters in only modules that aren't yet collapsed
                    int current_found_entropy = Get_Entropy(ref module);
                    if (current_found_entropy < _lowestEntropy)
                    { // finding the newest lowest entropy
                        lowestEntropyModules.Clear();
                        _lowestEntropy = current_found_entropy;
                    }

                    if (current_found_entropy == _lowestEntropy)
                    { // Checking for any modules with the same entropy
                        lowestEntropyModules.Add(coord);
                    }
                }
                else
                {
                    _hashedEntropyVectors.Remove(coord);
                }
            }
        }
        else
        {
            foreach(Vector3 coord in _hashedVectors)
            {
                ModularMapCell module = _mapArray[(int)coord.x, (int)coord.y, (int)coord.z];
                if (!Is_Collapsed(ref module))
                { // filters in only modules that aren't yet collapsed
                    int current_found_entropy = Get_Entropy(ref module);
                    if (current_found_entropy < _lowestEntropy)
                    { // finding the newest lowest entropy
                        lowestEntropyModules.Clear();
                        _lowestEntropy = current_found_entropy;
                    }

                    if (current_found_entropy == _lowestEntropy)
                    { // Checking for any modules with the same entropy
                        lowestEntropyModules.Add(coord);
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
        Collapse_(ref _mapArray[(int)coords.x, (int)coords.y, (int)coords.z]);
        _hashedEntropyVectors.Remove(coords);
        _hashedVectors.Remove(coords);
        _totalCurrentlyCollapsedModules++;
    }

    // Waves through all modules and adjusts all modules based on the current change
    public void Propagate(Vector3 _coords, Vector3 _localSize)
    {
        // New Propagation Model

        // Create open list
        List<Vector3> OpenList = new List<Vector3>();

        // Check around Module  
        ModularMapCell currentMod = GetModule(_coords);

        // Check Edges of the recently Collapsed Module

        // X
        if (CheckCollapsedModuleEdge(currentMod, _coords.x + 1, _coords + new Vector3(1, 0, 0), connector_edge.X, _localSize.x))
        {
            Vector3 updatedCoord = _coords + new Vector3(1, 0, 0);
            OpenList.Add(updatedCoord);
        }
        if (CheckCollapsedModuleEdge(currentMod, _coords.x - 1, _coords - new Vector3(1, 0, 0), connector_edge.nX, _localSize.x)) 
        {
            Vector3 updatedCoord = _coords - new Vector3(1, 0, 0);
            OpenList.Add(updatedCoord);
        }

        // Y
        if (CheckCollapsedModuleEdge(currentMod, _coords.y + 1, _coords + new Vector3(0, 1, 0), connector_edge.Y, _localSize.y)) 
        {
            Vector3 updatedCoord = _coords + new Vector3(0, 1, 0);
            OpenList.Add(updatedCoord);
        }
        if (CheckCollapsedModuleEdge(currentMod, _coords.y - 1, _coords - new Vector3(0, 1, 0), connector_edge.nY, _localSize.y)) 
        {
            Vector3 updatedCoord = _coords - new Vector3(0, 1, 0);
            OpenList.Add(updatedCoord);
        }

        //Z
        if (CheckCollapsedModuleEdge(currentMod, _coords.z + 1, _coords + new Vector3(0, 0, 1), connector_edge.Z, _localSize.z))
        {
            Vector3 updatedCoord = _coords + new Vector3(0, 0, 1);
            OpenList.Add(updatedCoord);
        }
        if (CheckCollapsedModuleEdge(currentMod, _coords.z - 1, _coords - new Vector3(0, 0, 1), connector_edge.nZ, _localSize.z))
        {
            Vector3 updatedCoord = _coords - new Vector3(0, 0, 1);
            OpenList.Add(updatedCoord);
        }




        // While the OpenList is empty Propagate
        while (OpenList.Count > 0)
        {
            // set a local variable and POP first element off openList
            var currentVec = OpenList[0];
            OpenList.RemoveAt(0);


            // Check around Module  
            currentMod = GetModule(currentVec);
            
            // X
            if((currentVec.x - _coords.x) < _totalPropagations)
            {
                if (CheckModuleEdge(currentMod, currentVec.x + 1, currentVec + new Vector3(1, 0, 0), connector_edge.X, _localSize.x))
                {
                    Vector3 updatedCoord = currentVec + new Vector3(1, 0, 0);
                    OpenList.Add(updatedCoord);
                }
            }
            if ((currentVec.x - _coords.x) < _totalPropagations)
            {
                if (CheckModuleEdge(currentMod, currentVec.x - 1, currentVec - new Vector3(1, 0, 0), connector_edge.nX, _localSize.x))
                {
                    Vector3 updatedCoord = currentVec - new Vector3(1, 0, 0);
                    OpenList.Add(updatedCoord);
                }
            }

            // Y
            if ((currentVec.y - _coords.y) < _totalPropagations)
            {
                if (CheckModuleEdge(currentMod, currentVec.y + 1, currentVec + new Vector3(0, 1, 0), connector_edge.Y, _localSize.y))
                {
                    Vector3 updatedCoord = currentVec + new Vector3(0, 1, 0);
                    OpenList.Add(updatedCoord);
                }
            }
            if ((currentVec.y - _coords.y) < _totalPropagations)
            {
                if (CheckModuleEdge(currentMod, currentVec.y - 1, currentVec - new Vector3(0, 1, 0), connector_edge.nY, _localSize.y))
                {
                    Vector3 updatedCoord = currentVec - new Vector3(0, 1, 0);
                    OpenList.Add(updatedCoord);
                }
            }

            //Z
            if ((currentVec.z - _coords.z) < _totalPropagations)
            {
                if (CheckModuleEdge(currentMod, currentVec.z + 1, currentVec + new Vector3(0, 0, 1), connector_edge.Z, _localSize.z))
                {
                    Vector3 updatedCoord = currentVec + new Vector3(0, 0, 1);
                    OpenList.Add(updatedCoord);
                }
            }
            if ((currentVec.z - _coords.z) < _totalPropagations)
            {
                if (CheckModuleEdge(currentMod, currentVec.z - 1, currentVec - new Vector3(0, 0, 1), connector_edge.nZ, _localSize.z))
                {
                    Vector3 updatedCoord = currentVec - new Vector3(0, 0, 1);
                    OpenList.Add(updatedCoord);
                    
                }
            }
        }

    }


    private bool CheckModuleEdge(ModularMapCell current_module, float _comparedAxis, Vector3 next_module_coordinate, connector_edge _comparingEdge, float _max)
    {
        bool removed = false;
        if ((_comparedAxis >= 0) && (_comparedAxis < _max))
        {
            ModularMapCell next_modular_map_cell = GetModule(next_module_coordinate);
            if (!Is_Collapsed(ref next_modular_map_cell))
            {
                _hashedEntropyVectors.Add(next_module_coordinate);
                // Attempts to Get the Module
                Bitset next_module_options = new Bitset(next_modular_map_cell._btst_options);
                Bitset options = GetEdge_AllOptionsFrom_(_comparingEdge, current_module._btst_options);
                Filter_OptionsTo_Options(options, ref GetModule(next_module_coordinate));



                if (!Bitset.Compare_IsSame(next_module_options, next_modular_map_cell._btst_options))
                {
                    removed = true;
                }

            }
        }

        return removed;
    }

    private bool CheckCollapsedModuleEdge(ModularMapCell current_module, float _comparedAxis, Vector3 next_module_coordinate, connector_edge _comparingEdge, float _max)
    {
        bool removed = false;
        if ((_comparedAxis >= 0) && (_comparedAxis < _max))
        {
            ModularMapCell next_modular_map_cell = GetModule(next_module_coordinate);
            if (!Is_Collapsed(ref next_modular_map_cell))
            {
                // Attempts to Get the Module
                Bitset next_module_options = new Bitset(next_modular_map_cell._btst_options);
                Bitset options = GetEdge_OptionsFrom_(_comparingEdge, current_module._int_module);
                Filter_OptionsTo_Options(options, ref GetModule(next_module_coordinate));
                if (!Bitset.Compare_IsSame(next_module_options, next_modular_map_cell._btst_options))
                {
                    removed = true;
                }


            }

        }

        return removed;
    }

}
