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
                    int _cellID = rawMapData[x, y, z];
                    Cell _currentCell = cells[_cellID];

                    GameObject _cellGameObject = _currentCell.PrefabGameObject;
                    Vector3 _mapPosition = new Vector3(x, y, z);
                    Vector3 _cellRotation = _currentCell.GetRotationInDegrees();

                    InstantiateCellGameObject(_cellGameObject, _mapPosition, _cellRotation, parentTransform);
                }
            }
        }
    }

    private void InstantiateCellGameObject(GameObject cellGameObject, Vector3 mapPosition, Vector3 cellRotation, Transform parentTransform)
    {
        if(m_instantiatedCellGameObjects == null) m_instantiatedCellGameObjects = new List<GameObject>();

        if (cellGameObject != null)
        {
            GameObject _newMapGameObject = GameObject.Instantiate(cellGameObject, mapPosition, Quaternion.Euler(cellRotation), parentTransform);
            if(m_instantiatedCellGameObjects != null) m_instantiatedCellGameObjects.Add(_newMapGameObject);
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
    public void DestroyUnityObject(UnityEngine.Object obj)
    {
        if (Application.isPlaying)
            GameObject.Destroy(obj);
        else
            GameObject.DestroyImmediate(obj);
    }

    // ********************************************************************************************
    // ********************************************************************************************
}
