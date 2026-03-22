using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CellController: MonoBehaviour
{
    MapMeshCombiner LocalMapMeshCombiner = new MapMeshCombiner();

    private bool IsCombineVariablesActive()
    {
        if (LocalMapMeshCombiner == null)
        {
            Debug.LogWarning("LocalMapMeshCombiner is not set");
            return false;
        }

        return true;
    }
    private bool SetupVariables()
    {
        if(LocalMapMeshCombiner == null)
        {
            LocalMapMeshCombiner = new MapMeshCombiner();
        }

        return true;
    }
    public void CombineGameObjectCells(GameObject gameObject)
    {
        if (!IsCombineVariablesActive()) SetupVariables();

        LocalMapMeshCombiner.CombineMeshes(gameObject);
    }

    public void CreateNewCell()
    {
        LayerTypes _lt_layerTypes = LayerTypes.None;
        sbyte _sbyte_rotation = 0;

        Connection _con_posX;
        Connection _con_posY;
        Connection _con_posZ;
        Connection _con_negX;
        Connection _con_negY;
        Connection _con_negZ;
    }
}