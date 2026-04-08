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

using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

[ExecuteInEditMode]
public class InLineCollapse : MapGenerator
{
    #region PUBLIC-METHODS
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

        int depthPosition = 0;
        int horizontalPosition = 0;
        int verticalPosition = 0;

        int depthDirection = 1;
        int horizontalDirection = 1;

        int executionCounter = 0;


        // GenerateMap for initial bottom floor verticalPosition=0 - zig zag generation
        for (verticalPosition = 0; verticalPosition < localMapDimensions.y; verticalPosition++)
        {
            // Using while loops allows the directions to be swapped easily
            while (depthPosition >= 0 && depthPosition < localMapDimensions.z)
            {
                while (horizontalPosition >= 0 && horizontalPosition < localMapDimensions.x)
                {
                    ModularMapCell modularMapCell = m_mapArray[horizontalPosition, verticalPosition, depthPosition];

                    if (modularMapCell != null)
                    {
                        CollapseCell(ref modularMapCell);

                        Vector3 newModuleCoord = new Vector3(horizontalPosition, verticalPosition, depthPosition);
                        UpdateEdgesWithinCoordinate(newModuleCoord, horizontalDirection, depthDirection);

                        executionCounter++;
                    }
                    horizontalPosition += horizontalDirection;
                }
                horizontalDirection *= -1; // swap horizontal direction
                horizontalPosition += horizontalDirection;
                depthPosition += depthDirection;
            }
            depthDirection *= -1; // swap depth direction
            depthPosition += depthDirection;
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


        // GenerateMap for initial bottom floor verticalPosition=0 - zig zag generation
        for (int y = 0; y < localMapDimensions.y; y++)
        {

            while (z >= 0 && z < localMapDimensions.z)
            {
                while (x >= 0 && x < localMapDimensions.x)
                {
                    ModularMapCell modular_map_cell = m_mapArray[x, y, z];

                    if (modular_map_cell != null)
                    {
                        CollapseCell(ref modular_map_cell);

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

    #endregion

    #region PRIVATE-METHODS

    //NOTE(Aidyn): Explain how the current module edge is calculated in this section specifically
    private ConnectorEdge GetConnectorEdge(ConnectorEdge edge, int flow)
    {
        int connectorNormalizeValue = 3;
        int cappingValue = 4;

        int normalizedFlow = connectorNormalizeValue + flow;
        int modulusFlow = normalizedFlow % cappingValue;

        int edgeAsInt = (int)edge;

        int newEdgeValueAsInt = edgeAsInt + modulusFlow;

        return (ConnectorEdge)newEdgeValueAsInt;
    }

    private void UpdateEdgesWithinCoordinate(Vector3 currentModuleCoordinate, int horizontalFlow, int verticalFlow)
    {
        ModularMapCell currentModule = GetModule(currentModuleCoordinate);

        CheckModuleEdges(currentModule, currentModuleCoordinate + new Vector3(horizontalFlow, 0, 0), GetConnectorEdge(ConnectorEdge.X, horizontalFlow), horizontalFlow, verticalFlow);

        CheckModuleEdges(currentModule, currentModuleCoordinate + new Vector3(0, 1, 0), ConnectorEdge.Y, horizontalFlow, verticalFlow);

        CheckModuleEdges(currentModule, currentModuleCoordinate + new Vector3(0, 0, verticalFlow), GetConnectorEdge(ConnectorEdge.Z, verticalFlow), horizontalFlow, verticalFlow);
    }

    private void CheckModuleEdges(ModularMapCell currentModule, Vector3 nextModuleCoordinate, ConnectorEdge currentModuleEdge, int horizontalDirection, int depthDirection)
    {
        // AreMapDimensionsPositive If currently compared module is within bounds of Map
        if (IsInMapBounds(nextModuleCoordinate))
        {
            // Attempts to Get the Module

            /* NOTE(Aidyn): Refactor this to into 2 steps, make the options collecting occur outside of this method*/
            // A) Gets current module info, could be collected once instead
            int currentModuleIndex = currentModule.Module;
            Bitset options = GetEdgeOptionsFromID(currentModuleEdge, currentModuleIndex);

            // B) Gets next module and filters options into it (that's it)
            ModularMapCell nextModule = GetModule(nextModuleCoordinate);
            FilterOptionsToCellOptions(options, ref nextModule);

            if (currentModuleEdge == GetConnectorEdge(ConnectorEdge.X, horizontalDirection))
            {
                CheckModulePossibleEdges(nextModule, nextModuleCoordinate + new Vector3(0, 0, depthDirection), GetConnectorEdge(ConnectorEdge.Z, depthDirection));
            }
            else if (currentModuleEdge == GetConnectorEdge(ConnectorEdge.Z, depthDirection))
            {
                CheckModulePossibleEdges(nextModule, nextModuleCoordinate + new Vector3(horizontalDirection, 0, 0), GetConnectorEdge(ConnectorEdge.X, horizontalDirection));
            }
        }
    }

    private void CheckModulePossibleEdges(ModularMapCell currentModule, Vector3 nextModuleCoordinate, ConnectorEdge currentModuleEdge)
    {
        // AreMapDimensionsPositive If currently compared module is within bounds of Map
        if (IsInMapBounds(nextModuleCoordinate))
        {
            // Attempts to Get the Module
            ModularMapCell nextModule = GetModule(nextModuleCoordinate);
            Bitset options = GetAllEdgeOptionsFromID(currentModuleEdge, currentModule.Options);
            FilterOptionsToCellOptions(options, ref nextModule);
        }
    }

    #endregion

}



