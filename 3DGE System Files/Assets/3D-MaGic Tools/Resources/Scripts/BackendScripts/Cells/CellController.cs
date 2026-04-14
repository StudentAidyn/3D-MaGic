using UnityEngine;

[System.Serializable]
public class CellController: MonoBehaviour
{
    private MapMeshCombiner m_localMapMeshCombiner = new MapMeshCombiner();

    private bool IsMapMeshCombinerSet()
    {
        if (m_localMapMeshCombiner == null)
        {
            Debug.LogWarning("LocalMapMeshCombiner is not set");
            return false;
        }

        return true;
    }
    private void SetLocalVariables()
    {
        if(m_localMapMeshCombiner == null)
        {
            m_localMapMeshCombiner = new MapMeshCombiner();
        }
    }

    public void CombineGameObjectCells(GameObject gameObject)
    {
        if (!IsMapMeshCombinerSet()) SetLocalVariables();

        m_localMapMeshCombiner.CombineMeshes(gameObject);
    }

    public void CreateNewCell()
    {
        //LayerTypes _layerTypes = LayerTypes.None;
        //sbyte m_rotation = 0;

        //Connection _con_posX;
        //Connection _con_posY;
        //Connection _con_posZ;
        //Connection _con_negX;
        //Connection _con_negY;
        //Connection _con_negZ;
    }
}