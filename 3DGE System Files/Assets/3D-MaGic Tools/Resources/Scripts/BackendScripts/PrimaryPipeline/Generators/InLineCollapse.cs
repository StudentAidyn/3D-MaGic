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

    public override void Generate(Vector3 localMapDimensions, ref ModularMapCell[,,] mapArray, List<Cell> cellList)
    {
        Init(localMapDimensions, ref mapArray, cellList);
        GenerativePattern();
    }

    private void GenerativePattern()
    {
        // Data Setup
        int depthPosition = 0;
        int horizontalPosition = 0;
        int verticalPosition = 0;

        int depthDirection = 1;
        int horizontalDirection = 1;
        
        //** Counts the number of executions, can be used to debug
        //int executionCounter = 0;


        // GenerateMap for initial bottom floor verticalPosition=0 - zig zag generation
        for (verticalPosition = 0; verticalPosition < m_dimensions.y; verticalPosition++)
        {
            // Using while loops allows the directions to be swapped easily
            while (depthPosition >= 0 && depthPosition < m_dimensions.z)
            {
                while (horizontalPosition >= 0 && horizontalPosition < m_dimensions.x)
                {
                    ModularMapCell modularMapCell = m_mapArray[horizontalPosition, verticalPosition, depthPosition];

                    if (modularMapCell != null)
                    {
                        CollapseCell(ref modularMapCell);

                        Vector3 newModuleCoord = new Vector3(horizontalPosition, verticalPosition, depthPosition);
                        UpdateEdgesWithinCoordinate(newModuleCoord, horizontalDirection, depthDirection);

                        //** debugging
                        //executionCounter++;
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

    #endregion

    #region PRIVATE-METHODS

    // Example 1: edge(Z) = 0, flow(-1)
    // 2 = ((-1 + 3) % 4) + 0
    // 2 = (2 % 4) + 0
    // 2 = 2 + 0 => nZ
    // Example 2: edge(Z) = 0, flow(1)
    // 0 = ((1 + 3) % 4) + 0
    // 0 = (4 % 4) + 0
    // 0 = 0 + 0 => Z
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

    private void UpdateEdgesWithinCoordinate(Vector3 currentModuleCoordinate, int horizontalFlow, int depthFlow)
    {
        ModularMapCell currentModule = GetModule(currentModuleCoordinate);
        int currentModuleID = currentModule.Module;


        ConnectorEdge horizontalEdge = GetConnectorEdge(ConnectorEdge.X, depthFlow);
        Bitset horizontalOptions = GetEdgeOptionsFromID(horizontalEdge, currentModuleID);

        Vector3 horizontal = new Vector3(horizontalFlow, 0, 0);
        Vector3 horizontalCoordinate = currentModuleCoordinate + horizontal;
        
        ApplyOptionsToModuleAtCoordinate(horizontalOptions, horizontalCoordinate);
        ApplyAllOptionsToNextEdges(horizontalCoordinate, horizontalFlow, depthFlow);


        ConnectorEdge verticalEdge = ConnectorEdge.Y;
        Bitset verticalOptions = GetEdgeOptionsFromID(verticalEdge, currentModuleID);

        Vector3 vertical = new Vector3(0, 1, 0);
        Vector3 verticalCoordinate = currentModuleCoordinate + vertical;
        
        ApplyOptionsToModuleAtCoordinate(verticalOptions, verticalCoordinate);
        ApplyAllOptionsToNextEdges(verticalCoordinate, horizontalFlow, depthFlow);


        ConnectorEdge depthEdge = GetConnectorEdge(ConnectorEdge.Z, depthFlow);
        Bitset depthOptions = GetEdgeOptionsFromID(depthEdge, currentModuleID);

        Vector3 depth = new Vector3(0, 0, depthFlow);
        Vector3 depthCoordinate = currentModuleCoordinate + depth;

        ApplyOptionsToModuleAtCoordinate(depthOptions, depthCoordinate);
        ApplyAllOptionsToNextEdges(depthCoordinate, horizontalFlow, depthFlow);
    }

    private void ApplyOptionsToModuleAtCoordinate(Bitset options, Vector3 moduleCoordinate)
    {
        // AreMapDimensionsPositive If currently compared module is within bounds of Map
        if (IsInMapBounds(moduleCoordinate))
        {
            ModularMapCell nextModule = GetModule(moduleCoordinate);
            FilterOptionsToCellOptions(options, ref nextModule);
        }
    }

    private void ApplyAllOptionsToNextEdges(Vector3 moduleCoordinate, int horizontalDirection, int depthDirection)
    {
        ApplyAllOptionsToModuleAtCoordinate(moduleCoordinate, new Vector3(horizontalDirection, 0, 0), ConnectorEdge.X, horizontalDirection);
        ApplyAllOptionsToModuleAtCoordinate(moduleCoordinate, new Vector3(0, 0, depthDirection), ConnectorEdge.Z, depthDirection);
    }

    private void ApplyAllOptionsToModuleAtCoordinate(Vector3 moduleCoordinate, Vector3 additionalCoordinates, ConnectorEdge edge, int direction)
    {
        ModularMapCell currentModule = GetModule(moduleCoordinate);
        ConnectorEdge newEdge = GetConnectorEdge(edge, direction);

        Bitset allOptions = GetAllEdgeOptionsFromEdge(newEdge, currentModule.Options);
        Vector3 newCoordinate = moduleCoordinate + additionalCoordinates;

        ApplyOptionsToModuleAtCoordinate(allOptions, newCoordinate);
    }

    #endregion

}



