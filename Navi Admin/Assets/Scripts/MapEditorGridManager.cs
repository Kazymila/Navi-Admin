using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class MapEditorGridManager : MonoBehaviour
{
    [Header("Grid GUI Elements")]
    [SerializeField] private GameObject _overlayPanel;
    [SerializeField] private GameObject _gridSizePanel;
    [SerializeField] private TMP_InputField _gridWidthInput;
    [SerializeField] private TMP_InputField _gridHeightInput;
    [SerializeField] private Slider _gridSizeSlider;

    [Header("Grid Settings")]
    public float gridSize = 1f;
    public bool gridActive = true;
    public bool snapToGrid = false;

    private GameObject _grid;
    private TMP_Text _sliderLabel;

    void Start()
    {
        _grid = this.gameObject.transform.GetChild(0).gameObject;
        _sliderLabel = _gridSizeSlider.gameObject.transform.GetChild(1).GetComponent<TMP_Text>();
    }

    public void ViewGrid()
    {
        gridActive = !gridActive;
        _grid.SetActive(gridActive);
        _gridSizeSlider.gameObject.SetActive(false);
    }

    public void SnapToGrid()
    {
        snapToGrid = !snapToGrid;
        _gridSizeSlider.gameObject.SetActive(false);
    }

    public void ChangeGridSize()
    {
        /*
        // Change the grid size by width and height
        float _width = float.Parse(_gridWidthInput.text);
        float _height = float.Parse(_gridHeightInput.text);
        gridSize = new Vector2(_width, _height);
        //_overlayPanel.SetActive(false);
        //_gridSizePanel.SetActive(false);
        */

        float _sliderValue = _gridSizeSlider.value;
        Vector2 _SizeVector = new Vector2(10 - _sliderValue, 10 - _sliderValue);

        Material material = _grid.GetComponent<MeshRenderer>().materials[0];
        material.SetVector("_Size", _SizeVector);

        gridSize = _sliderValue + 1;
        _sliderLabel.text = gridSize.ToString() + " m";
    }

    public void ShowGridSizeSlider()
    {
        //_overlayPanel.SetActive(true);
        //_gridSizePanel.SetActive(true);
        _gridSizeSlider.gameObject.SetActive(true);
    }
}
