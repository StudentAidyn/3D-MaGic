using System.Collections.Generic;
using UnityEngine;


// https://www.youtube.com/watch?v=alU04hvz6L4&ab_channel=CodeMonkey

public class Sc_AstarPathFinding
{
    //// Cost to move around modules
    //private int DEFAULT_MOVEMENT_COST = 10;
    //private int DIAGONAL_MOVEMENT_COST = 14;


    //// Array Path
    //ModularMapCell[,,] Map;

    //// Open and Closed Lists
    //List<ModularMapCell> openList;
    //List<ModularMapCell> closedList;

    //Vector3 mapDimensions;

    //public List<ModularMapCell> GeneratePath(ModularMapCell[,,] _map, Vector3 _dimensions, int _traversalPoints = 2/*, bool _randomPoints = true */)
    //{
    //    if(_traversalPoints < 2) return null;

    //    // Sets the local variables
    //    Map = _map;
    //    mapDimensions = _dimensions;

    //    // Points - randomly generated points wihtin the map
    //    List<ModularMapCell> Points = new List<ModularMapCell>();

    //    // Generates traversal points based on total number of points
    //    for (int i = 0; i < _traversalPoints; i++) {
    //        // Generates random  _dt_start and _dt_end points
    //        Vector3 randomPosition = new Vector3((int)Random.Range(0, mapDimensions.x), 0, (int)Random.Range(0, mapDimensions.z));
    //        Points.Add(GetVectorModule(randomPosition));
    //    }

    //    // Path list - to return
    //    List<ModularMapCell> Path = new List<ModularMapCell>();

    //    for (int i = 0; i < _traversalPoints - 1; i++) {
    //        List<ModularMapCell> PathSegment = AstarPathing(Points[i], Points[i + 1]);
    //        if (PathSegment != null) {
    //            foreach (ModularMapCell module in PathSegment)
    //            {
    //                Path.Add(module);
    //            }
    //        }

    //    }

    //    return Path;
    //}
 
    //private List<ModularMapCell> AstarPathing(ModularMapCell startNode, ModularMapCell endNode) { 

    //    //// OpenList to check the area around it
    //    //openList = new List<ModularMapCell>();
    //    //// Closed List to check the area already checked
    //    //closedList = new List<ModularMapCell>();

    //    //// Setting each value within the Mapable Space (so only x and z of the lower layer
    //    //for(int x = 0; x < mapDimensions.x; x++) {
    //    //    for(int z = 0; z < mapDimensions.z; z++) {
    //    //        ModularMapCell mapModule = GetVectorModule(new Vector3(x, 0, z));
    //    //        mapModule.gScore = int.MaxValue;
    //    //        mapModule.CalculateFScore();

    //    //        mapModule.previousModule = null;
    //    //    }
    //    //}

    //    //startNode.gScore = 0;
    //    //startNode.hScore = CalculateDistance(startNode, endNode);
    //    //startNode.CalculateFScore();

    //    //openList.Add(startNode);

    //    //while (openList.Count > 0)
    //    //{
    //    //    ModularMapCell currentMod = GetLowestFScoreMod(openList);
    //    //    if(currentMod == endNode) {
    //    //        // reached final node
    //    //        return CalculatePath(endNode);
    //    //    }


    //    //    openList.Remove(currentMod);
    //    //    closedList.Add(currentMod);

    //    //    foreach(ModularMapCell neighbour in GetNeighbourList(currentMod))
    //    //    {
    //    //        if (closedList.Contains(neighbour)) continue;

    //    //        int tentativeGScore = currentMod.gScore + CalculateDistance(currentMod, neighbour);
    //    //        if(tentativeGScore < neighbour.gScore) {
    //    //            neighbour.previousModule = currentMod;
    //    //            neighbour.gScore = tentativeGScore;
    //    //            neighbour.hScore = CalculateDistance(neighbour, endNode);
    //    //            neighbour.CalculateFScore();

    //    //            if(!openList.Contains(neighbour)) {
    //    //                openList.Add(neighbour);
    //    //            }
    //    //        }
    //    //    }

    //    //}
    //    return null;
    //}

    //private List<ModularMapCell> GetNeighbourList(ModularMapCell _mod) {
    //    List<ModularMapCell> neighbours = new List<ModularMapCell>();


    //    if(_mod._vec3_mapPosition.x - 1 > 0) {
    //        neighbours.Add(GetVectorModule(_mod._vec3_mapPosition - new Vector3(1, 0, 0)));
    //        if (_mod._vec3_mapPosition.z - 1 > 0)
    //        {
    //            neighbours.Add(GetVectorModule(_mod._vec3_mapPosition - new Vector3(1, 0, 1)));
    //        }
    //        if (_mod._vec3_mapPosition.z + 1 < mapDimensions.z)
    //        {
    //            neighbours.Add(GetVectorModule(_mod._vec3_mapPosition + new Vector3(-1, 0, 1)));
    //        }
    //    }
    //    if (_mod._vec3_mapPosition.x + 1 < mapDimensions.x)
    //    {
    //        neighbours.Add(GetVectorModule(_mod._vec3_mapPosition + new Vector3(1, 0, 0)));
    //        if (_mod._vec3_mapPosition.z - 1 > 0)
    //        {
    //            neighbours.Add(GetVectorModule(_mod._vec3_mapPosition - new Vector3(-1, 0, 1)));
    //        }
    //        if (_mod._vec3_mapPosition.z + 1 < mapDimensions.z)
    //        {
    //            neighbours.Add(GetVectorModule(_mod._vec3_mapPosition + new Vector3(1, 0, 1)));
    //        }
    //    }
    //    if (_mod._vec3_mapPosition.z - 1 > 0)
    //    {
    //        neighbours.Add(GetVectorModule(_mod._vec3_mapPosition - new Vector3(0, 0, 1)));
    //    }
    //    if (_mod._vec3_mapPosition.z + 1 < mapDimensions.z)
    //    {
    //        neighbours.Add(GetVectorModule(_mod._vec3_mapPosition + new Vector3(0, 0, 1)));
    //    }

    //    return neighbours;
    //}

    //public ModularMapCell GetVectorModule(Vector3 _coords)
    //{
    //    return Map[(int)_coords.x, (int)_coords.y, (int)_coords.z];
    //}

    //private List<ModularMapCell> CalculatePath(ModularMapCell _endMod) {
    //    List<ModularMapCell> path = new List<ModularMapCell>();
    //    path.Add(_endMod);
    //    //ModularMapCell current = _endMod;
    //    //while (current.previousModule != null) { 
    //    //    path.Add(current.previousModule);
    //    //    current = current.previousModule;
    //    //}
    //    //path.Reverse();
    //    return path;
    //}

    //private int CalculateDistance(ModularMapCell _mod, ModularMapCell _other)
    //{
    //    int xDistance = (int)Mathf.Abs(_mod._vec3_mapPosition.x - _other._vec3_mapPosition.x);
    //    int zDistance = (int)Mathf.Abs(_mod._vec3_mapPosition.z - _other._vec3_mapPosition.z);
    //    int remaining = Mathf.Abs(xDistance - zDistance);
    //    return DIAGONAL_MOVEMENT_COST * Mathf.Min(xDistance, zDistance) + DEFAULT_MOVEMENT_COST * remaining;
    //}

    //private ModularMapCell GetLowestFScoreMod(List<ModularMapCell> modList) {
    //    ModularMapCell lowestFScoreMod = modList[0];

    //    //for(int i = 1; i < modList.Count; i++) {
    //    //    if (modList[i].fScore  < lowestFScoreMod.fScore) {
    //    //        lowestFScoreMod = modList[i];
    //    //    }
    //    //}

    //    return lowestFScoreMod;
    //}
}
