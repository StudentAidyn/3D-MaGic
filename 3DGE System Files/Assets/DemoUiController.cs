using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class DemoUiController : MonoBehaviour
{
    [SerializeField] private int m_cameraIndex;
    [SerializeField] private List<Camera> m_cameras = new List<Camera>();

    [SerializeField] private int m_scaler = 5;
    [SerializeField] private GenerationType m_generationType = GenerationType.WaveFunctionCollapse;
    [SerializeField] private Transform m_parent;

    [SerializeField] private List<ModularMapCellComponent> m_modularMapCellComponents = new List<ModularMapCellComponent>();

    [SerializeField] private Slider m_scaleSlider;
    [SerializeField] private TMP_Text m_scaleText;
    [SerializeField] private TMP_Dropdown m_genTypeDropDown;
    [SerializeField] private Button m_actionButton;
    [SerializeField] private TMP_Text m_actionText;
    [SerializeField] private Button m_changePOVButton;

    private MapController m_controller;
    private enum m_status { Generate, Delete }
    private m_status m_currentStatus = m_status.Generate;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        m_controller = new MapController();
        m_currentStatus = m_status.Generate;

        InactivateAllCameras();
        SwapActiveCameras(0);
        ChangeGenerationType(0);
    }

    public void OnEnable()
    {
        if(m_scaleSlider) { m_scaleSlider.onValueChanged.AddListener(ChangeScaleSlider); }
        if(m_genTypeDropDown) { m_genTypeDropDown.onValueChanged.AddListener(ChangeGenerationType); }
        if(m_actionButton) { m_actionButton.onClick.AddListener(Action); }
        if(m_changePOVButton) { m_changePOVButton.onClick.AddListener(ChangeCameraAngle); }
    }

    public void OnDisable()
    {
        if (m_scaleSlider) { m_scaleSlider.onValueChanged.RemoveListener(ChangeScaleSlider); }
        if (m_genTypeDropDown) { m_genTypeDropDown.onValueChanged.RemoveListener(ChangeGenerationType); }
        if (m_actionButton) { m_actionButton.onClick.RemoveListener(Action); }
        if (m_changePOVButton) { m_changePOVButton.onClick.RemoveListener(ChangeCameraAngle); }
    }
    private void InactivateAllCameras()
    {
        foreach (Camera camera in m_cameras)
        {
            camera.gameObject.SetActive(false);
        }
    }
    public void ChangeCameraAngle()
    {
        int newCameraIndex = m_cameraIndex + 1;
        if (newCameraIndex >= m_cameras.Count) { newCameraIndex = 0; }
        SwapActiveCameras(newCameraIndex);
        m_cameraIndex = newCameraIndex;
    }

    private void SwapActiveCameras(int index)
    {
        m_cameras[m_cameraIndex].gameObject.SetActive(false);
        m_cameras[index].gameObject.SetActive(true);
    }

    public void Action()
    {
        switch (m_currentStatus) {
            case m_status.Generate: Generate(); break;
            case m_status.Delete: DeleteMap(); break;
            default: break;
        }
        UpdateActionButtonText(m_currentStatus.ToString());
    }
    public void Generate()
    {
        MapGenData mapData = new MapGenData();
        mapData.Dimensions = new Vector3(m_scaler, 3, m_scaler);
        mapData.Control = GenerationStep.GenerateBuildCombine;
        mapData.Type = m_generationType;
        mapData.Seed = RandomNumber.GetNewSeed();

        if (m_controller.HasCells()) { m_controller.GenerateMap(mapData, m_parent); }
        else { m_controller.GenerateMap(mapData, m_parent, m_modularMapCellComponents); }
        m_controller.ClearBuiltListOfGameObjects();

        m_currentStatus = m_status.Delete;
    }

    public void DeleteMap()
    {
        m_controller.ClearInstantiatedMapObjects();
        m_currentStatus = m_status.Generate;
    }

    private void UpdateActionButtonText(string text)
    {
        m_actionText.text = text;
    }

    public void ChangeGenerationType(int index)
    {
        switch (index)
        {
            case 0: m_generationType = GenerationType.WaveFunctionCollapse; break;
            case 1: m_generationType = GenerationType.InLineCollapse; break;
            default: break;
        }
    }

    public void ChangeScaleSlider(float sliderValue)
    {
        m_scaler = (int)sliderValue;
        m_scaleText.text = GetScaleValueAsString();
    }

    public string GetScaleValueAsString()
    {
        string scale = m_scaler.ToString();
        string scaleDisplayed = scale + "x3x" + scale;
        return scaleDisplayed;
    }
}
