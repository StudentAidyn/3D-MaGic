using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MMC_", menuName = "ScriptableObjects/ModularMapCellComponent", order = 1)]
public class ModularMapCellComponent : ScriptableObject
{
    [Header("Object Details")]
    // Mesh and Rotation and the chance Weight of selection
    [SerializeField] private GameObject _mesh = null;
    private int _optionIndex = 0;
    [SerializeField] private int _rotation = 0;
    [SerializeField] private LayerTypes _layerType;

    public void SetRotation(int rotation) { _rotation = rotation; }
    public void SetOptionIndex(int optionIndex) { _optionIndex = optionIndex; }
    public int GetOptionIndex() { return _optionIndex; }
    public LayerTypes GetLayerType() { return _layerType; }

    // Sets values
    public void SetUp(GameObject _mesh, int _rotation, float _weight, LayerTypes _type) { this._mesh = _mesh; this._rotation = _rotation; _layerType = _type; }

    [Header("Edge Connections")]

    [SerializeField] bool _noVariants = false;
    [SerializeField] public bool m_removeAfterBuild = false;

    public bool NoVariants() { return _noVariants; }

    public void SetEdges(
        Connector _X, Connector _nX, 
        Connector _Y, Connector _nY, 
        Connector _Z, Connector _nZ)
    {
        _validConnections[(int)connector_edge.Z]. SetConnector(_Z);
        _validConnections[(int)connector_edge.nZ].SetConnector(_nZ);

        _validConnections[(int)connector_edge.X]. SetConnector(_X);
        _validConnections[(int)connector_edge.nX].SetConnector(_nX);

        _validConnections[(int)connector_edge.Y]. SetConnector(_Y);
        _validConnections[(int)connector_edge.nY].SetConnector(_nY);
    }
    // an array of valid neighbours
     [SerializeField] ValidConnections[] _validConnections = {
        new ValidConnections(connector_edge.Z),
        new ValidConnections(connector_edge.X),
        new ValidConnections(connector_edge.nZ),
        new ValidConnections(connector_edge.nX),
        new ValidConnections(connector_edge.Y),
        new ValidConnections(connector_edge.nY)
    };

    public ValidConnections GetValidConnectionsWith_(connector_edge _connectorEdge)
    {
        for (int i = 0; i < _validConnections.Length; i++)
        {
            if (_validConnections[i].GetConnectorEdge() == _connectorEdge)
            {
                return _validConnections[i];
            }
            
        }
        return null;
    }

    public bool CanGetConnection(connector_edge _connectorEdge, ref ValidConnections _validConnection)
    {
        for (int i = 0; i < _validConnections.Length; i++)
        {
            if (_validConnections[i].GetConnectorEdge() == _connectorEdge)
            {
                _validConnection = _validConnections[i];
                return true;
            }
        }

        return false;
    }

    public Connection GetConnectionWith_(connector_edge _connectorEdge)
    {
        ValidConnections valid_connection = new ValidConnections(_connectorEdge);

        if(CanGetConnection(_connectorEdge, ref valid_connection))
        {
            return valid_connection.GetConnection();
        }

        Connection connection = new Connection();
        return connection;
    }

    public GameObject GetMesh() { return _mesh; }
    public int GetRotation() { return _rotation; }
}

[System.Serializable]
// ValidConnections class that contains the name of the edge and the name of the valid neighbours
public class ValidConnections
{
    // the name of the side
    [SerializeField] private connector_edge _connectorEdge = 0; // by default every edge will be set to Z
    [SerializeField] private Connection _connection = new Connection();

    public ValidConnections(connector_edge _edge) {
        _connectorEdge = _edge;
    }

    // Variable Controls
    public connector_edge GetConnectorEdge() { return _connectorEdge; }
    public void SetConnector(Connector _connector) { _connection._connector = _connector; }
    public Connection GetConnection() { return _connection; }
}

[System.Serializable]
public struct Connection
{
    public Connector _connector;
    public ConnectorProperty _property;
    public sbyte _rotation;
}

public enum connector_edge
{
    Z = 0,
    X = 1,
    nZ = 2,
    nX = 3,
    Y = 4,
    nY = 5
}