using System.Collections.Generic;
using UnityEngine;

public class MapBuilder
{
    public MapBuilder()
    {  
    }

    public List<GameObject> InstantiateMap(Vector3 dimensions, ModularMapCell[,,] mapArray, List<Cell> cells, Transform parentTransform = null)
    {
        int[,,] rawMapData = new int[
                (int)dimensions.y,
                (int)dimensions.x,
                (int)dimensions.z];

        for (int z = 0; z < dimensions.z; z++)
        {
            for (int y = 0; y < dimensions.y; y++)
            {
                for (int x = 0; x < dimensions.x; x++)
                {
                    rawMapData[x, y, z] = mapArray[x, y, z].Module;
                }
            }
        }

        return InstantiateMap(dimensions, rawMapData, cells, parentTransform);
    }
    public List<GameObject> InstantiateMap(Vector3 dimensions, int[,,] rawMapData, List<Cell> cells, Transform parentTransform = null)
    {
        List<GameObject> m_instantiatedCellGameObjects = new List<GameObject>();

        for (int z = 0; z < dimensions.z; z++)
        {
            for(int y = 0; y < dimensions.y; y++)
            {
                for(int x = 0; x < dimensions.x; x++)
                {
                    int cellID = rawMapData[x, y, z];
                    Cell currentCell = cells[cellID];

                    GameObject cellGameObject = currentCell.PrefabGameObject;
                    Vector3 mapPosition = new Vector3(x, y, z);
                    Vector3 cellRotation = currentCell.GetRotationInDegrees();

                    GameObject newGameObject = InstantiateCellGameObject(cellGameObject, mapPosition, cellRotation, parentTransform);
                    if (newGameObject != null) m_instantiatedCellGameObjects.Add(newGameObject);
                }
            }
        }

        return m_instantiatedCellGameObjects;
    }

    private GameObject InstantiateCellGameObject(GameObject cellGameObject, Vector3 mapPosition, Vector3 cellRotation, Transform parentTransform)
    {
        if (cellGameObject != null)
        {
            GameObject newMapGameObject = GameObject.Instantiate(cellGameObject, mapPosition, Quaternion.Euler(cellRotation), parentTransform);
            return newMapGameObject;
        }

        return null;
    }
}
