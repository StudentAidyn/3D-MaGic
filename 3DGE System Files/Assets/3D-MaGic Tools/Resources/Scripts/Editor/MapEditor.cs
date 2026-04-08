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

    private Map m_map;
    private Button m_generateButton;
    private Button m_clearGeneratedButton;
    private Button m_saveButton;
    private Button m_loadButton;

    private SerializedProperty m_fileNameValue;

    private PropertyField m_toggleSeed;
    private VisualElement m_elementsToHide;
    private SerializedProperty m_seedValue;

    private void OnEnable()
    {
        m_map = (Map)target;
        m_seedValue = serializedObject.FindProperty("_useCurrentSeed");
        m_fileNameValue = serializedObject.FindProperty("_fileName");
    }

    public override VisualElement CreateInspectorGUI()
    {

        VisualElement root = new VisualElement();

        _visualTree.CloneTree(root);
        
        //find and assign buttons
        m_generateButton = root.Q<Button>("Bt_Generate");
        m_generateButton.RegisterCallback<ClickEvent>(GenerateClick);

        m_clearGeneratedButton = root.Q<Button>("Bt_Clear");
        m_clearGeneratedButton.RegisterCallback<ClickEvent>(ClearClick);

        m_saveButton = root.Q<Button>("Bt_Save");
        m_saveButton.RegisterCallback<ClickEvent>(SaveClick);


        m_loadButton = root.Q<Button>("Bt_Load");
        m_loadButton.RegisterCallback<ClickEvent>(LoadClick);


        m_toggleSeed = root.Q<PropertyField>("CustSeed");
        m_toggleSeed.RegisterCallback<ChangeEvent<bool>>(OnBoolChange_Seed);

        m_elementsToHide = root.Q<VisualElement>("Seed");

        DisplayCheck();

        return root;
    }

    private void GenerateClick(ClickEvent _event)
    {
        m_map.GenerateMap();
    }

    private void ClearClick(ClickEvent _event)
    {
        m_map.ClearMap();
    }
    
    private void OnBoolChange_Seed(ChangeEvent<bool> evt)
    {
        DisplayCheck();
    }

    private void DisplayCheck()
    {
        if(m_seedValue != null)
        {
            if (m_seedValue.boolValue)
            {
                m_elementsToHide.SetEnabled(true);
                m_elementsToHide.style.opacity = 1f;
            }
            else
            {
                m_elementsToHide.SetEnabled(false);
                m_elementsToHide.style.opacity = 0.5f;
            }
        }

    }

    #region Save/Load

    private void LoadClick(ClickEvent _event)
    {
        m_map.LoadMap(m_fileNameValue.stringValue);
        m_map.UpdateDisplayData(m_map.LocalMapController.GetMapGenData());
    }

    private void SaveClick(ClickEvent _event)
    {
        m_map.SaveMap(m_fileNameValue.stringValue);
    }

    #endregion
}
