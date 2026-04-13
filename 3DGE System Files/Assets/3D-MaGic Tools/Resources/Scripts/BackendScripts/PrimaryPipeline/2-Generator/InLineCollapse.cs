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
    private int m_depthDirection;
    private int m_horizontalDirection;
    
    private Vector3 m_horizontal;
    private Vector3 m_vertical;
    private Vector3 m_depth;

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

        int horizontalDirection = 1;
        int depthDirection = 1;

        m_horizontalDirection = horizontalDirection;
        m_depthDirection = depthDirection;

        m_horizontal = new Vector3(m_horizontalDirection, 0, 0);
        m_vertical = new Vector3(0, 1, 0);
        m_depth = new Vector3(0, 0, m_depthDirection);

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

                    CollapseCell(ref modularMapCell);

                    Vector3 newModuleCoord = new Vector3(horizontalPosition, verticalPosition, depthPosition);
                    UpdateEdgesWithinCoordinate(newModuleCoord);

                    horizontalPosition += horizontalDirection;
                }
            
                horizontalDirection *= -1; // swap m_horizontal direction
                m_horizontalDirection = horizontalDirection;
                m_horizontal = new Vector3(m_horizontalDirection, 0, 0);

                horizontalPosition += horizontalDirection;
                depthPosition += depthDirection;
            }
            
            depthDirection *= -1; // swap m_depth direction
            m_depthDirection = depthDirection;
            m_depth = new Vector3(0, 0, m_depthDirection);

            depthPosition += depthDirection;
        }
    }

    #endregion

    #region PRIVATE-METHODS

    // Example 1: edge(Z) = 0, direction(-1)
    // 2 = ((-1 + 3) % 4) + 0
    // 2 = (2 % 4) + 0
    // 2 = 2 + 0 => nZ
    // Example 2: edge(Z) = 0, direction(1)
    // 0 = ((1 + 3) % 4) + 0
    // 0 = (4 % 4) + 0
    // 0 = 0 + 0 => Z
    private ConnectorEdge GetConnectorEdge(ConnectorEdge edge, int direction)
    {
        int connectorNormalizeValue = 3;
        int cappingValue = 4;

        int normalizedFlow = connectorNormalizeValue + direction;
        int modulusFlow = normalizedFlow % cappingValue;

        int edgeAsInt = (int)edge;

        int newEdgeValueAsInt = edgeAsInt + modulusFlow;
        
        return (ConnectorEdge)newEdgeValueAsInt;
    }

    private void UpdateEdgesWithinCoordinate(Vector3 currentModuleCoordinate)
    {
        ModularMapCell currentModule = GetModule(currentModuleCoordinate);
        int currentModuleID = currentModule.Module;


        ConnectorEdge horizontalEdge = GetConnectorEdge(ConnectorEdge.X, m_depthDirection);
        Bitset horizontalOptions = new Bitset(GetEdgeOptionsFromID(horizontalEdge, currentModuleID));

        Vector3 horizontalCoordinate = currentModuleCoordinate + m_horizontal;
        
        ApplyOptionsToModuleAtCoordinate(horizontalOptions, horizontalCoordinate);
        ApplyAllOptionsToNextEdges(horizontalCoordinate);


        ConnectorEdge verticalEdge = ConnectorEdge.Y;
        Bitset verticalOptions = new Bitset(GetEdgeOptionsFromID(verticalEdge, currentModuleID));
        
        Vector3 verticalCoordinate = currentModuleCoordinate + m_vertical;
        
        ApplyOptionsToModuleAtCoordinate(verticalOptions, verticalCoordinate);


        ConnectorEdge depthEdge = GetConnectorEdge(ConnectorEdge.Z, m_depthDirection);
        Bitset depthOptions = new Bitset(GetEdgeOptionsFromID(depthEdge, currentModuleID));

        Vector3 depthCoordinate = currentModuleCoordinate + m_depth;

        ApplyOptionsToModuleAtCoordinate(depthOptions, depthCoordinate);
        ApplyAllOptionsToNextEdges(depthCoordinate);
    }

    private void ApplyOptionsToModuleAtCoordinate(Bitset options, Vector3 moduleCoordinate)
    {
        // AreMapDimensionsPositive If currently compared module is within bounds of Map
        if (IsInMapBounds(moduleCoordinate))
        {
            FilterOptionsToCellOptions(options, ref GetModule(moduleCoordinate));
        }
    }

    private void ApplyAllOptionsToNextEdges(Vector3 moduleCoordinate)
    {
        ConnectorEdge horizontalEdge = GetConnectorEdge(ConnectorEdge.X, m_horizontalDirection);
        ApplyAllOptionsToModuleAtCoordinate(moduleCoordinate, m_horizontal, horizontalEdge);

        ConnectorEdge depthEdge = GetConnectorEdge(ConnectorEdge.Z, m_depthDirection);
        ApplyAllOptionsToModuleAtCoordinate(moduleCoordinate, m_vertical, depthEdge);
    }

    private void ApplyAllOptionsToModuleAtCoordinate(Vector3 moduleCoordinate, Vector3 additionalCoordinates, ConnectorEdge edge)
    {
        Vector3 newCoordinate = moduleCoordinate + additionalCoordinates;
        if (IsInMapBounds(newCoordinate))
        {
            ModularMapCell currentModule = GetModule(moduleCoordinate);
            Bitset allOptions = new Bitset(GetAllEdgeOptionsFromEdge(edge, currentModule.Options));

            ApplyOptionsToModuleAtCoordinate(allOptions, newCoordinate);
        }
    }

    #endregion

}



