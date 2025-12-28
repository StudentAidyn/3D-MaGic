// Unity Editor Script - https://www.youtube.com/watch?v=eCIv4i_i9bE&ab_channel=SasquatchBStudios
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UnityEngine;

// Custom Editor Display of Map Script

[CustomEditor(typeof(Map))]
public class MapEditor : Editor
{
    public VisualTreeAsset _visualTree;

    private Map _map;
    private Button _generateButton;
    private Button _clearGeneratedButton;
    private Button _saveButton;
    private Button _loadButton;

    private SerializedProperty _fileNameValue;

    private PropertyField _toggleSeed;
    private VisualElement _elementsToHide;
    private SerializedProperty _seedValue;

    private void OnEnable()
    {
        _map = (Map)target;
        _seedValue = serializedObject.FindProperty("_useCurrentSeed");
        _fileNameValue = serializedObject.FindProperty("_fileName");
    }

    public override VisualElement CreateInspectorGUI()
    {

        VisualElement root = new VisualElement();

        _visualTree.CloneTree(root);
        

        //find and assign buttons
        _generateButton = root.Q<Button>("Bt_Generate");
        _generateButton.RegisterCallback<ClickEvent>(GenerateClick);

        _clearGeneratedButton = root.Q<Button>("Bt_Clear");
        _clearGeneratedButton.RegisterCallback<ClickEvent>(ClearClick);

        _saveButton = root.Q<Button>("Bt_Save");
        _saveButton.RegisterCallback<ClickEvent>(SaveClick);


        _loadButton = root.Q<Button>("Bt_Load");
        _loadButton.RegisterCallback<ClickEvent>(LoadClick);


        _toggleSeed = root.Q<PropertyField>("CustSeed");
        _toggleSeed.RegisterCallback<ChangeEvent<bool>>(OnBoolChange_Seed);

        _elementsToHide = root.Q<VisualElement>("Seed");

        DisplayCheck();

        return root;
    }

    private void GenerateClick(ClickEvent _event)
    {
        _map.GenerateMap();
    }

    private void ClearClick(ClickEvent _event)
    {
        _map.ClearMap();
    }
    
    private void OnBoolChange_Seed(ChangeEvent<bool> evt)
    {
        DisplayCheck();
    }

    private void DisplayCheck()
    {
        if(_seedValue != null)
        {
            if (_seedValue.boolValue)
            {
                _elementsToHide.SetEnabled(true);
                _elementsToHide.style.opacity = 1f;
            }
            else
            {
                _elementsToHide.SetEnabled(false);
                _elementsToHide.style.opacity = 0.5f;
            }
        }

    }

    #region Save/Load

    private void LoadClick(ClickEvent _event)
    {
        _map.LoadMap(_fileNameValue.stringValue);
        _map.UpdateDisplayData(_map.LocalMapController.GetMapGenData());
    }

    private void SaveClick(ClickEvent _event)
    {
        _map.SaveMap(_fileNameValue.stringValue);
    }

    #endregion
}
