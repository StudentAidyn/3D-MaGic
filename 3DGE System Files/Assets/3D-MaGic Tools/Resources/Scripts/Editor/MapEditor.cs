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
    public VisualTreeAsset LocalVisualTree;

    private Map m_map;

    private Button m_generateButton;
    private Button m_clearGeneratedButton;
    private Button m_saveButton;
    private Button m_loadButton;

    private PropertyField m_toggleSeed;
    private VisualElement m_elementsToHide;
    private SerializedProperty m_seedValue;

    private void OnEnable()
    {
        m_map = (Map)target;
        m_seedValue = serializedObject.FindProperty("m_useCurrentSeed");
    }

    public override VisualElement CreateInspectorGUI()
    {

        VisualElement root = new VisualElement();

        LocalVisualTree.CloneTree(root);

        //find and assign buttons
        m_generateButton = root.Q<Button>("gen-buttons__generate");
        m_generateButton.RegisterCallback<ClickEvent>(GenerateClick);

        m_clearGeneratedButton = root.Q<Button>("gen-buttons__clear-prev");
        m_clearGeneratedButton.RegisterCallback<ClickEvent>(ClearClick);

        m_saveButton = root.Q<Button>("save-load__save");
        m_saveButton.RegisterCallback<ClickEvent>(SaveClick);


        m_loadButton = root.Q<Button>("save-load__load");
        m_loadButton.RegisterCallback<ClickEvent>(LoadClick);


        m_toggleSeed = root.Q<PropertyField>("seed-group__custom-check");
        m_toggleSeed.RegisterCallback<ChangeEvent<bool>>(OnBoolChangeSeed);

        m_elementsToHide = root.Q<VisualElement>("seed-group__value");

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
    
    private void OnBoolChangeSeed(ChangeEvent<bool> evt)
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
        m_map.LoadMap();
        m_map.UpdateDisplayData(m_map.LocalMapController.GetMapGenData());
    }

    private void SaveClick(ClickEvent _event)
    {
        m_map.SaveMap();
    }

    #endregion
}
