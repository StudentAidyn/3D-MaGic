// Unity Editor Script - https://www.youtube.com/watch?v=eCIv4i_i9bE&ab_channel=SasquatchBStudios
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UnityEngine;

// Custom Editor Display of Map Script

[CustomEditor(typeof(CellController))]
public class CellEditor : Editor
{
    public VisualTreeAsset LocalVisualTree;
    private CellController LocalCellController;

    //private GameObject MeshObject = null;
    //private int OptionIndex = 0;
    //private int RotationDefault = 0;
    //private LayerTypes LayerType;

    //public bool HasVariants = false;
    //public bool RemoveAfterBuild = false;

    //public void SetEdges(
    //    Connector _X, Connector _nX,
    //    Connector _Y, Connector _nY,
    //    Connector _Z, Connector _nZ)
    //{
    //    _validConnections[(int)connector_edge.Z].SetConnector(_Z);
    //    _validConnections[(int)connector_edge.nZ].SetConnector(_nZ);

    //    _validConnections[(int)connector_edge.X].SetConnector(_X);
    //    _validConnections[(int)connector_edge.nX].SetConnector(_nX);

    //    _validConnections[(int)connector_edge.Y].SetConnector(_Y);
    //    _validConnections[(int)connector_edge.nY].SetConnector(_nY);
    //}

    //// an array of valid neighbours
    //[SerializeField]
    //ValidConnections[] _validConnections = {
    //    new ValidConnections(connector_edge.Z),
    //    new ValidConnections(connector_edge.X),
    //    new ValidConnections(connector_edge.nZ),
    //    new ValidConnections(connector_edge.nX),
    //    new ValidConnections(connector_edge.Y),
    //    new ValidConnections(connector_edge.nY)
    //};

    // GUI Buttons
    private Button CreateCellButton;
    private Button CombineMeshButton;

    private void OnEnable()
    {
        LocalCellController = (CellController)target;
    }

    public override VisualElement CreateInspectorGUI()
    {
        VisualElement root = new VisualElement();

        LocalVisualTree.CloneTree(root);

        //find and assign buttons
        CreateCellButton = root.Q<Button>("Bt_Create");
        CreateCellButton.RegisterCallback<ClickEvent>(CreateCellEvent);

        CombineMeshButton = root.Q<Button>("Bt_Combine");
        CombineMeshButton.RegisterCallback<ClickEvent>(CombineMeshEvent);

        return root;
    }

    private void CreateCellEvent(ClickEvent _event)
    {
        LocalCellController.CreateNewCell();
    }

    private void CombineMeshEvent(ClickEvent _event)
    {
        //LocalCellController.CombineGameObjectCells(MeshObject);
    }


}
