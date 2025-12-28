/*
 In Line Collapse

In line collapse is great for when you have all the possible combinations of each block, 
as the generation is less accurate to minimal blocks but has faster generation speeds
than using Wave Function Collapse. 

In Line collapse works by moving along the X axis until it hits the edge, then moving 
over 1 in the Z and repeating but in reverse. Once it hits the edge of the Z axis it then
moves up on the Y axis. It move forwards and backwards zig-zagging back and forth 
building each layer. It is almost the most efficient variant of WFC a part from using a 
form of prior knowledge of a grid, like using noise, or something along those lines.
 
 */

using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;

[ExecuteInEditMode]
public class InLineCollapse : MapGenerator
{
    /// this WFC cycles through the whole array every time,
    /// using overlapping chunks will help with performance and accuracy.
    
    // GenerateMap Sets up all the default variables
    public InLineCollapse() : base()
    { 
        // Setup (if required)
    }

    public override void Generate(Vector3 localMapDimensions, List<Cell> cellList)
    {
        Init(localMapDimensions, cellList);

        int z = 0;
        int x = 0;

        int zFlow = 1;
        int xFlow = 1;

        int counter = 0;


        // GenerateMap for initial bottom floor y=0 - zig zag generation
        for (int y = 0; y < localMapDimensions.y; y++)
        {

            while (z >= 0 && z < localMapDimensions.z)
            {
                while (x >= 0 && x < localMapDimensions.x)
                {
                    ModularMapCell modular_map_cell = _mapArray[x, y, z];

                    if (modular_map_cell != null)
                    {
                        Collapse_(ref modular_map_cell);

                        Update_EdgesWithin_(new Vector3(x, y, z), localMapDimensions, xFlow, zFlow);

                        counter++;
                    }
                    x += xFlow;
                }
                xFlow *= -1;
                x += xFlow;
                z += zFlow;
            }
            zFlow *= -1;
            z += zFlow;
        }
    }

    public override async Task GenerateAsync(Vector3 localMapDimensions, List<Cell> cellList)
    {
        Init(localMapDimensions, cellList);

        int z = 0;
        int x = 0;

        int zFlow = 1;
        int xFlow = 1;

        int counter = 0;


        // GenerateMap for initial bottom floor y=0 - zig zag generation
        for (int y = 0; y < localMapDimensions.y; y++)
        {

            while (z >= 0 && z < localMapDimensions.z)
            {
                while (x >= 0 && x < localMapDimensions.x)
                {
                    ModularMapCell modular_map_cell = _mapArray[x, y, z];

                    if (modular_map_cell != null)
                    {
                        Collapse_(ref modular_map_cell);

                        Update_EdgesWithin_(new Vector3(x, y, z), localMapDimensions, xFlow, zFlow);

                        await Task.Yield();
                    }
                    x += xFlow;
                }
                xFlow *= -1;
                x += xFlow;
                z += zFlow;
            }
            zFlow *= -1;
            z += zFlow;
        }
    }

    //NOTE(Aidyn): Explain how the current module edge is calculated in this section specifically
    private connector_edge GetConnectorEdge(connector_edge _edge, int _flow)
    {
        return (connector_edge)((int)_edge + ((3 + _flow) % 4));
    }
        
    private void Update_EdgesWithin_(Vector3 current_module_coordinate, Vector3 localMapDimensions, int x_flow, int z_flow)
    {
        ModularMapCell current_module = GetModule(current_module_coordinate);

        CheckModuleEdges(current_module, current_module_coordinate + new Vector3(x_flow, 0, 0), GetConnectorEdge(connector_edge.X, x_flow), localMapDimensions, x_flow, z_flow);

        CheckModuleEdges(current_module, current_module_coordinate + new Vector3(0, 1, 0), connector_edge.Y, localMapDimensions, x_flow, z_flow);

        CheckModuleEdges(current_module, current_module_coordinate + new Vector3(0, 0, z_flow), GetConnectorEdge(connector_edge.Z, z_flow), localMapDimensions, x_flow, z_flow);
    }

    private void CheckModuleEdges(ModularMapCell current_module, Vector3 next_module_coordinate, connector_edge current_module_edge, Vector3 local_map_size, int x_flow, int z_flow)
    {
        // AreMapDimensionsPositive If currently compared module is within bounds of Map
        if ((next_module_coordinate.x >= 0 && next_module_coordinate.x < local_map_size.x) &&
            (next_module_coordinate.y >= 0 && next_module_coordinate.y < local_map_size.y) &&
            (next_module_coordinate.z >= 0 && next_module_coordinate.z < local_map_size.z))
        {
            // Attempts to Get the Module
            ModularMapCell next_module = GetModule(next_module_coordinate);
            int current_module_index = current_module._int_module;
            Bitset options = GetEdge_OptionsFrom_(current_module_edge, current_module_index);
            Filter_OptionsTo_Options(options, ref next_module);

            if (current_module_edge == GetConnectorEdge(connector_edge.X, x_flow))
            {
                CheckModulePossibleEdges(next_module, next_module_coordinate + new Vector3(0, 0, z_flow), GetConnectorEdge(connector_edge.Z, z_flow), local_map_size);
            }
            else if (current_module_edge == GetConnectorEdge(connector_edge.Z, z_flow))
            {
                CheckModulePossibleEdges(next_module, next_module_coordinate + new Vector3(x_flow, 0, 0), GetConnectorEdge(connector_edge.X, x_flow), local_map_size);
            }
        }
    }


    private void CheckModulePossibleEdges(ModularMapCell current_module, Vector3 next_module_coordinate, connector_edge current_module_edge, Vector3 local_map_size)
    {
        // AreMapDimensionsPositive If currently compared module is within bounds of Map
        if ((next_module_coordinate.x >= 0 && next_module_coordinate.x < local_map_size.x) &&
            (next_module_coordinate.y >= 0 && next_module_coordinate.y < local_map_size.y) &&
            (next_module_coordinate.z >= 0 && next_module_coordinate.z < local_map_size.z))
        {
            // Attempts to Get the Module
            ModularMapCell next_module = GetModule(next_module_coordinate);
            Bitset options = GetEdge_AllOptionsFrom_(current_module_edge, current_module._btst_options);
            Filter_OptionsTo_Options(options, ref next_module);

            //int Avalue = next_module._btst_options.bits_get()[0];
            //string Astrand = Convert.ToString(Avalue, 2);

            //int fvalue = next_module._btst_options.bits_get()[0];
            //string strand = Convert.ToString(fvalue, 2);
            //Debug.Log("AFTER: " + next_module_coordinate + " || " + strand + " :: " + Astrand);
        }
    }

}



