using System.Collections.Generic;
using UnityEngine;

public class MapBuilder
{
    private List<GameObject> m_instantiatedCellGameObjects;
    public List<GameObject> GetMapObjects() { return m_instantiatedCellGameObjects; }


    public MapBuilder()
    {  
    }

    public void Init() { }
      
    public void InstantiateMap(Vector3 mapSize, int[,,] rawMapData, List<Cell> cells, Transform parentTransform = null)
    {
        for (int z = 0; z < mapSize.z; z++)
        {
            for(int y = 0; y < mapSize.y; y++)
            {
                for(int x = 0; x < mapSize.x; x++)
                {
                    int cellID = rawMapData[x, y, z];
                    Cell currentCell = cells[cellID];

                    GameObject cellGameObject = currentCell.PrefabGameObject;
                    Vector3 mapPosition = new Vector3(x, y, z);
                    Vector3 cellRotation = currentCell.GetRotationInDegrees();

                    InstantiateCellGameObject(cellGameObject, mapPosition, cellRotation, parentTransform);
                }
            }
        }
    }

    private void InstantiateCellGameObject(GameObject cellGameObject, Vector3 mapPosition, Vector3 cellRotation, Transform parentTransform)
    {
        if(m_instantiatedCellGameObjects == null) m_instantiatedCellGameObjects = new List<GameObject>();

        if (cellGameObject != null)
        {
            GameObject newMapGameObject = GameObject.Instantiate(cellGameObject, mapPosition, Quaternion.Euler(cellRotation), parentTransform);
            if(m_instantiatedCellGameObjects != null) m_instantiatedCellGameObjects.Add(newMapGameObject);
        }
    }

    // ********************************************************************************************
    // ********************************************************************************************



    // Functions: Clear/Delete ********************************************************************
    // ********************************************************************************************
    // Clears the GameObject list
    public void ClearBuiltListOfGameObjects()
    {
        if (m_instantiatedCellGameObjects == null) return;
        if (m_instantiatedCellGameObjects.Count < 1) return;
        foreach (GameObject modular_map_cell_object in m_instantiatedCellGameObjects)
        {
            DestroyUnityObject(modular_map_cell_object);
        }

        m_instantiatedCellGameObjects.Clear();
    }

    // destroys objects during edit and play mode
    public void DestroyUnityObject(Object obj)
    {
        if (Application.isPlaying)
            GameObject.Destroy(obj);
        else
            GameObject.DestroyImmediate(obj);
    }

    // ********************************************************************************************
    // ********************************************************************************************
}
