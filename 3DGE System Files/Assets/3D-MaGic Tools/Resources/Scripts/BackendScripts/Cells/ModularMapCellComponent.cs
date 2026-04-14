using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MMC_", menuName = "ScriptableObjects/ModularMapCellComponent", order = 1)]
public class ModularMapCellComponent : ScriptableObject
{
    [Header("Object Details")]
    // Mesh and Rotation and the chance Weight of selection
    [SerializeField] 
    private GameObject m_mesh = null;
    private int m_optionIndex = 0;
    [SerializeField] 
    private int m_rotation = 0;
    [SerializeField] 
    private LayerTypes m_layerType;

    public void SetRotation(int rotation) { m_rotation = rotation; }
    public void SetOptionIndex(int optionIndex) { this.m_optionIndex = optionIndex; }
    public int GetOptionIndex() { return m_optionIndex; }
    public LayerTypes GetLayerType() { return m_layerType; }

    // Sets values
    public void SetUp(GameObject mesh, int rotation, float weight, LayerTypes type) 
    { 
        m_mesh = mesh; 
        m_rotation = rotation; 
        m_layerType = type; 
    }

    [Header("Edge Connections")]

    [SerializeField] private bool m_noVariants = false;
    [SerializeField] public bool RemoveAfterBuild = false;

    public bool NoVariants() { return m_noVariants; }

    public void SetEdges(
        Connector _X, Connector _nX, 
        Connector _Y, Connector _nY, 
        Connector _Z, Connector _nZ)
    {
        m_validConnections[(int)ConnectorEdge.Z]. SetConnector(_Z);
        m_validConnections[(int)ConnectorEdge.nZ].SetConnector(_nZ);

        m_validConnections[(int)ConnectorEdge.X]. SetConnector(_X);
        m_validConnections[(int)ConnectorEdge.nX].SetConnector(_nX);

        m_validConnections[(int)ConnectorEdge.Y]. SetConnector(_Y);
        m_validConnections[(int)ConnectorEdge.nY].SetConnector(_nY);
    }
    // an array of valid neighbours
     [SerializeField] private ValidConnections[] m_validConnections = {
        new ValidConnections(ConnectorEdge.Z),
        new ValidConnections(ConnectorEdge.X),
        new ValidConnections(ConnectorEdge.nZ),
        new ValidConnections(ConnectorEdge.nX),
        new ValidConnections(ConnectorEdge.Y),
        new ValidConnections(ConnectorEdge.nY)
    };

    public ValidConnections GetValidConnectionsWith_(ConnectorEdge connectorEdge)
    {
        for (int i = 0; i < m_validConnections.Length; i++)
        {
            if (m_validConnections[i].GetConnectorEdge() == connectorEdge)
            {
                return m_validConnections[i];
            }
            
        }
        return null;
    }

    public bool CanGetConnection(ConnectorEdge connectorEdge, ref ValidConnections validConnection)
    {
        for (int i = 0; i < m_validConnections.Length; i++)
        {
            if (m_validConnections[i].GetConnectorEdge() == connectorEdge)
            {
                validConnection = m_validConnections[i];
                return true;
            }
        }

        return false;
    }

    public Connection GetConnectionWithEdge(ConnectorEdge connectorEdge)
    {
        ValidConnections valid_connection = new ValidConnections(connectorEdge);

        if(CanGetConnection(connectorEdge, ref valid_connection))
        {
            return valid_connection.GetConnection();
        }

        Connection connection = new Connection();
        return connection;
    }

    public GameObject GetMesh() { return m_mesh; }
    public int GetRotation() { return m_rotation; }
}

[System.Serializable]
// ValidConnections class that contains the name of the edge and the name of the valid neighbours
public class ValidConnections
{
    // the name of the side
    [SerializeField] private ConnectorEdge m_connectorEdge = 0; // by default every edge will be SetBitAtIndex to Z
    [SerializeField] private Connection m_connection = new Connection();

    public ValidConnections(ConnectorEdge edge) {
        m_connectorEdge = edge;
    }

    // Variable Controls
    public ConnectorEdge GetConnectorEdge() { return m_connectorEdge; }
    public void SetConnector(Connector connector) { m_connection.Connector = connector; }
    public Connection GetConnection() { return m_connection; }
}

[System.Serializable]
public struct Connection
{
    public Connector Connector;
    public ConnectorProperty Property;
    public sbyte Rotation;
}

public enum ConnectorEdge
{
    Z = 0,
    X = 1,
    nZ = 2,
    nX = 3,
    Y = 4,
    nY = 5
}