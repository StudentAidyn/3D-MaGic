using System.Collections.Generic;
using UnityEngine;

public class MapBuilder
{
    private List<GameObject> initiatedMapObjectsList;
    public List<GameObject> MapObjects() => initiatedMapObjectsList;
    // Functions: Build ***************************************************************************
    // ********************************************************************************************
    public MapBuilder()
    {  
    }

    public void Init() { }
      
    public void BuildMap(Vector3 _vec3_mapSize, int[,,] _arr_int_rawMapData, List<Cell> _lst_cl_cells, Transform _trn_parent = null)
    {
        initiatedMapObjectsList = new List<GameObject>();

        for (int z = 0; z < _vec3_mapSize.z; z++)
        {
            for(int y = 0; y < _vec3_mapSize.y; y++)
            {
                for(int x = 0; x < _vec3_mapSize.x; x++)
                {
                    Cell current_cell = _lst_cl_cells[_arr_int_rawMapData[x, y, z]];
                    GameObject map_object = BuildMapCell(current_cell._go_gameObject, new Vector3(x, y, z), GetCellRotation(current_cell), _trn_parent);
                    if (map_object) { initiatedMapObjectsList.Add(map_object); }
                }
            }
        }
    }

    private GameObject BuildMapCell(GameObject _go_cellObject, Vector3 _vec3_mapPosition, Vector3 _vec3_rotation, Transform _trn_parent)
    {
        GameObject go_built_gameObject = null;
        
        if (_go_cellObject != null)
        {
            go_built_gameObject = GameObject.Instantiate(_go_cellObject, _vec3_mapPosition, Quaternion.Euler(_vec3_rotation), _trn_parent);
        }

        return go_built_gameObject;
    }

    private Vector3 GetCellRotation(Cell _cell)
    {
        return new Vector3(0f, -_cell._sbyte_rotation * 90f, 0);
    }

    // ********************************************************************************************
    // ********************************************************************************************



    // Functions: Clear/Delete ********************************************************************
    // ********************************************************************************************
    // Clears the GameObject list
    public void ClearBuiltListOfGameObjects()
    {
        if (initiatedMapObjectsList == null) return;
        if (initiatedMapObjectsList.Count < 1) return;
        foreach (GameObject modular_map_cell_object in initiatedMapObjectsList)
        {
            DestroyUnityObject(modular_map_cell_object);
        }

        initiatedMapObjectsList.Clear();
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
