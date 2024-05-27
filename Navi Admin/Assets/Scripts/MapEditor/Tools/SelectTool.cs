using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using MapDataModel;
using UnityEngine.UI;

public class SelectTool : MonoBehaviour
{
    #region --- External Variables ---
    [Header("UI Components")]
    [SerializeField] private EditorLayoutController _UIEditorController;
    [SerializeField] private ErrorMessageController _errorMessageBox;
    [SerializeField] private Transform _UIItems;

    [Header("Settings Panels")]
    [SerializeField] private GameObject _entranceSettingsPanel;
    [SerializeField] private GameObject _polygonSettingsPanel;
    [SerializeField] private GameObject _wallSettingsPanel;
    [SerializeField] private GameObject _roomTypePanel;

    private TMP_InputField _wallSizeInput;
    private TMP_InputField _entranceSizeInput;
    private TMP_InputField _entranceLabelInput;
    private TMP_InputField _polygonLabelInput;
    private TMP_InputField _polygonTypeInput;
    private TMP_InputField[] _polygonTranslatedLabelInput;
    private TMP_InputField[] _polygonTranslatedTypeInput;
    private TMP_Dropdown _polygonTypeDropdown;
    private Slider _polygonTypeModeSlider;

    [Header("Color Picker")]
    [SerializeField] private FlexibleColorPicker _colorPicker;
    [SerializeField] private Material _colorOpaquePreview;
    [SerializeField] private Material _colorAlphaPreview;

    [Header("Map Editor Components")]
    [SerializeField] private RoomsManager _polygonsManager;
    [SerializeField] private GameObject _polygonsParent;
    [SerializeField] private MapEditorGridManager _gridManager;
    [SerializeField] private GameObject _wallLabelPrefab;
    [SerializeField] private GameObject _wallSizeLabel;
    #endregion

    #region --- Variables ---
    private List<GameObject> _wallLabels = new List<GameObject>();
    private List<GameObject> _segmentsLabels = new List<GameObject>();
    private EntrancesController _selectedEntrance;
    private WallLineController _selectedWall;
    private WallNodeController _selectedDot;
    private GameObject _selectedEntranceDot;
    private RoomController _selectedPolygon;

    // Remember the old values
    private Vector3 _oldEntranceDotPos;
    private Vector3 _oldEntrancePos;
    private float _oldEntranceSize;
    private float _oldWallSize;
    private Color _oldPolygonColor;

    // States variables
    private bool _movingEntranceDot = false;
    private bool _editingEntrance = false;
    private bool _movingEntrance = false;
    private bool _editingWall = false;
    private bool _movingDot = false;

    #endregion

    private InputMap _input;

    private void OnEnable()
    {
        _input = new InputMap();
        _input.MapEditor.Enable();
        _input.MapEditor.Click.started += ctx => OnSelectClick();
        _input.MapEditor.EndDraw.started += ctx => CancelAction();

        // Enable the polygons manager
        //_polygonsManager.GenerateRooms();
        _polygonsParent.SetActive(true);
    }
    private void OnDisable()
    {
        _input.MapEditor.Disable();
        if (_editingEntrance) CancelEntranceEdit();
        if (_editingWall) CancelWallEdit();

        _polygonsParent.SetActive(false);
        _colorPicker.gameObject.SetActive(false);
        _polygonSettingsPanel.SetActive(false);
    }

    private void Awake()
    {   // Get the components references
        _wallSizeInput = _wallSettingsPanel.GetComponentInChildren<TMP_InputField>();
        _entranceSizeInput = _entranceSettingsPanel.transform.GetChild(2).GetComponentInChildren<TMP_InputField>();
        _entranceLabelInput = _entranceSettingsPanel.transform.GetChild(1).GetComponentInChildren<TMP_InputField>();
        _polygonLabelInput = _polygonSettingsPanel.transform.GetChild(1).GetComponentInChildren<TMP_InputField>();
        _polygonTypeDropdown = _polygonSettingsPanel.transform.GetChild(4).GetComponentInChildren<TMP_Dropdown>();
        _polygonTypeInput = _roomTypePanel.transform.GetChild(1).GetComponentInChildren<TMP_InputField>();

        _polygonTranslatedLabelInput = new TMP_InputField[2];
        _polygonTranslatedLabelInput[0] = _polygonSettingsPanel.transform.GetChild(2).GetComponentInChildren<TMP_InputField>();
        _polygonTranslatedLabelInput[1] = _polygonSettingsPanel.transform.GetChild(3).GetComponentInChildren<TMP_InputField>();

        _polygonTranslatedTypeInput = new TMP_InputField[2];
        _polygonTranslatedTypeInput[0] = _roomTypePanel.transform.GetChild(2).GetComponentInChildren<TMP_InputField>();
        _polygonTranslatedTypeInput[1] = _roomTypePanel.transform.GetChild(3).GetComponentInChildren<TMP_InputField>();

        _polygonTypeModeSlider = _roomTypePanel.transform.GetChild(4).GetComponentInChildren<Slider>();
    }

    private Vector3 GetCursorPosition(bool _considerSnap = true)
    {   // Get the cursor position in the world
        Vector3 _cursorPosition = Camera.main.ScreenToWorldPoint(_input.MapEditor.Position.ReadValue<Vector2>());
        _cursorPosition.z = 0;

        if (_gridManager.snapToGrid && _considerSnap)
        {
            _cursorPosition.x = Mathf.Round(_cursorPosition.x / _gridManager.gridSize);
            _cursorPosition.y = Mathf.Round(_cursorPosition.y / _gridManager.gridSize);
        }
        return _cursorPosition;
    }

    private void Update()
    {
        if (_movingDot && !_UIEditorController.IsCursorOverEditorUI())
        {   // Move the selected dot to the cursor position
            _selectedDot.SetPosition(GetCursorPosition());
            ShowWallsSizeLabel();
        }
        if (_movingEntrance && !_UIEditorController.IsCursorOverEditorUI())
        {   // Move the selected entrance to the cursor position
            _selectedEntrance.SetEntrancePositionFromCursor(GetCursorPosition(false), _selectedEntrance.entranceWall);
            ShowWallSegmentsSize(_selectedEntrance.entranceWall);
        }
        if (_movingEntranceDot && !_UIEditorController.IsCursorOverEditorUI())
        {   // Move the selected entrance dot to the cursor position
            _selectedEntrance.MoveEntranceDot(GetCursorPosition(false), _selectedEntranceDot);
            _entranceSizeInput.text = _selectedEntrance.lenght.ToString("F5");
            ShowWallSegmentsSize(_selectedEntrance.entranceWall);
        }
        if (_editingWall && !_UIEditorController.IsCursorOverEditorUI())
        {   // Update the position of the wall size label
            ShowWallSize();
        }
        if (_editingEntrance && !_UIEditorController.IsCursorOverEditorUI())
        {   // Update the position of the wall segments size labels
            ShowWallSegmentsSize(_selectedEntrance.entranceWall);
        }
    }

    private void OnSelectClick()
    {   // Select the object under the cursor to do something with it
        if (_UIEditorController.IsCursorOverEditorUI()) return;

        if (_movingDot)
        {   // Set the dot position and stop moving it
            _selectedDot.PlaySelectAnimation();
            _movingDot = false;

            _polygonsManager.UpdatePolygons();
            _polygonsManager.gameObject.SetActive(true);
        }
        else if (_movingEntrance)
        {   // Set the entrance position and stop moving it
            if (_selectedEntrance.isOverEntrance) return;
            _oldEntrancePos = _selectedEntrance.transform.localPosition;
            _selectedEntrance.PlaySettedAnimation();
            _selectedEntrance.isSetted = true;
            _movingEntrance = false;
        }
        else if (_movingEntranceDot)
        {   // Set the entrance dot position and stop moving it
            if (_selectedEntrance.isOverEntrance) return;
            _oldEntranceSize = _selectedEntrance.lenght;
            _selectedEntrance.PlaySettedAnimation();
            _selectedEntrance.isSetted = true;
            _selectedEntranceDot = null;
            _movingEntranceDot = false;
        }
        else
        {
            if (!_editingWall) _wallSizeLabel.SetActive(false);
            RaycastToSelect();
        }
        RemoveWallsSizeLabel();
    }

    private void CancelAction()
    {   // Cancel the current action
        if (_movingDot)
        {   // Cancel the dot movement
            _selectedDot.PlaySelectAnimation();
            _movingDot = false;
        }
        else if (_movingEntrance)
        {   // Cancel the entrance movement
            _selectedEntrance.RepositionEntranceOnWall(_oldEntrancePos, _selectedEntrance.entranceWall);
            _selectedEntrance.PlaySettedAnimation();
            _selectedEntrance.isSetted = true;
            _movingEntrance = false;
        }
        else if (_movingEntranceDot)
        {   // Cancel the entrance dot movement
            _selectedEntrance.MoveEntranceDot(_oldEntranceDotPos, _selectedEntranceDot);
            _selectedEntrance.PlaySettedAnimation();
            _selectedEntrance.isSetted = true;
            _selectedEntranceDot = null;
            _movingEntranceDot = false;
        }
    }

    private void RaycastToSelect()
    {   // Raycast to the object under the cursor to select it
        RaycastHit2D _hit = Physics2D.Raycast(GetCursorPosition(false), Vector2.zero);

        if (_hit.collider != null)
        {
            if (_hit.collider.CompareTag("WallDot"))
            {   // Select the dot and start moving it
                if (_editingWall) CancelWallEdit();
                if (_editingEntrance) CancelEntranceEdit();
                _polygonSettingsPanel.SetActive(false);
                _colorPicker.gameObject.SetActive(false);
                _wallSizeLabel.SetActive(false);

                _selectedDot = _hit.collider.GetComponent<WallNodeController>();
                _selectedDot.PlaySelectAnimation();
                _movingDot = true;

                _polygonsManager.gameObject.SetActive(false);
            }
            else if (_hit.collider.CompareTag("EntranceDot"))
            {   // Select a entrance dot and start move it
                _polygonSettingsPanel.SetActive(false);
                _colorPicker.gameObject.SetActive(false);
                if (_editingWall) CancelWallEdit();
                _wallSizeLabel.SetActive(false);
                RemoveSegmentsSizeLabel();

                _selectedEntrance = _hit.collider.transform.parent.GetComponent<EntrancesController>();
                _selectedEntranceDot = _hit.collider.gameObject;
                InitializeEntranceEditing();

                _oldEntranceDotPos = _selectedEntranceDot.transform.position;
                _oldEntranceDotPos.z = 0;
                _movingEntranceDot = true;

                ShowWallSegmentsSize(_selectedEntrance.entranceWall);
            }
            else if (_hit.collider.CompareTag("Entrance"))
            {   // Select an entrance and edit it
                _polygonSettingsPanel.SetActive(false);
                _colorPicker.gameObject.SetActive(false);
                if (_editingWall) CancelWallEdit();
                _wallSizeLabel.SetActive(false);
                RemoveSegmentsSizeLabel();

                _selectedEntrance = _hit.collider.GetComponent<EntrancesController>();
                ShowWallSegmentsSize(_selectedEntrance.entranceWall);
                InitializeEntranceEditing();
                _movingEntrance = true;
            }
            else if (_hit.collider.CompareTag("Wall"))
            {   // Select a wall and edit it
                _polygonSettingsPanel.SetActive(false);
                _colorPicker.gameObject.SetActive(false);

                if (_editingEntrance) CancelEntranceEdit();
                _selectedWall = _hit.collider.GetComponent<WallLineController>();
                _selectedWall.startNode.PlaySelectAnimation();
                _selectedWall.endNode.PlaySelectAnimation();

                _wallSizeInput.text = _selectedWall.length.ToString("F5");
                _oldWallSize = _selectedWall.length;
                _wallSettingsPanel.SetActive(true);
                _editingWall = true;
                ShowWallSize();

                _polygonsManager.gameObject.SetActive(false);
            }
            else if (_hit.collider.CompareTag("Polygon"))
            {   // Select a polygon and edit it
                if (_editingWall) CancelWallEdit();
                if (_editingEntrance) CancelEntranceEdit();
                _wallSizeLabel.SetActive(false);
                RemoveSegmentsSizeLabel();

                _selectedPolygon = _hit.collider.GetComponent<RoomController>();
                InitializePolygonSettingsPanel();
            }
        }
        else if (_editingWall) CancelWallEdit();
    }

    #region --- Wall Settings ---
    public void ChangeLineSize()
    {   // Change the selected line size with the input field value
        if (!_editingWall) return;
        string _inputText = _wallSizeInput.text;
        _errorMessageBox.HideMessage();

        if (_inputText == "") return;
        else if (float.Parse("0" + _inputText) == 0) return;
        else
        {
            float _newSize = float.Parse("0" + _inputText);
            _selectedWall.ResizeWall(_newSize);
            ShowWallSize();
        }
    }

    public void SetLineSize()
    {   // Set the new line size on confirmation
        if (_wallSizeInput.text == "")
            _errorMessageBox.ShowMessage("EnterSomeValue");
        else
        {
            _wallSettingsPanel.SetActive(false);
            _wallSizeLabel.SetActive(false);
            _editingWall = false;

            _polygonsManager.UpdatePolygons();
            _polygonsManager.gameObject.SetActive(true);
        }
    }

    public void CancelWallEdit(bool _fromButton = false)
    {   // Cancel the wall edit and reset the line
        if (!_fromButton)
        {   // If not canceled by the button, check if the cursor is over the panel
            bool _isOverPanel = RectTransformUtility.RectangleContainsScreenPoint(
                _wallSettingsPanel.GetComponent<RectTransform>(),
                _input.MapEditor.Position.ReadValue<Vector2>(), null);
            if (_isOverPanel) return;
        }
        _wallSizeInput.text = _oldWallSize.ToString("F5");
        _selectedWall.ResizeWall(_oldWallSize);
        _wallSettingsPanel.SetActive(false);
        _editingWall = false;

        _polygonsManager.gameObject.SetActive(true);
    }
    #endregion

    #region --- Entrance Settings ---
    private void InitializeEntranceEditing()
    {   // Set the entrance settings panel and start editing the entrance
        _entranceSizeInput.text = _selectedEntrance.lenght.ToString("F5");
        _oldEntrancePos = _selectedEntrance.transform.localPosition;
        _entranceLabelInput.text = _selectedEntrance.name;
        _oldEntranceSize = _selectedEntrance.lenght;

        _selectedEntrance.PlayMovingAnimation();
        _entranceSettingsPanel.SetActive(true);
        _selectedEntrance.isSetted = false;
        _editingEntrance = true;
    }

    public void ChangeEntranceSize()
    {   // Change the selected entrance size with the input field value
        if (!_editingEntrance || _movingEntranceDot) return;
        string _inputText = _entranceSizeInput.text;
        _errorMessageBox.HideMessage();

        if (_inputText == "") return;
        else if (float.Parse("0" + _inputText) == 0) return;
        else
        {
            float _newSize = float.Parse("0" + _inputText);
            _selectedEntrance.ResizeEntrance(_newSize);
            _selectedEntrance.isSetted = false;
            ShowWallSegmentsSize(_selectedEntrance.entranceWall);
        }
    }
    public void SetEntranceSettings()
    {   // Set the entrance changes on confirmation
        if (_selectedEntrance.isOverEntrance) return;
        if (_entranceSizeInput.text == "")
            _errorMessageBox.ShowMessage("EnterSomeValue");
        else
        {
            _errorMessageBox.HideMessage();
            _entranceSettingsPanel.SetActive(false);
            _entranceSettingsPanel.SetActive(false);
            _selectedEntrance.PlaySettedAnimation();
            _selectedEntrance.isSetted = true;
            _editingEntrance = false;
            _movingEntrance = false;

            if (_entranceLabelInput.text != "") // Set label
                _selectedEntrance.name = _entranceLabelInput.text;

            RemoveSegmentsSizeLabel();
        }
    }
    public void CancelEntranceEdit(bool _fromButton = false)
    {   // Cancel the entrance edit and reset the entrance
        if (_movingEntrance) _selectedEntrance.ResizeEntrance(_oldEntranceSize);
        _entranceSizeInput.text = _oldEntranceSize.ToString("F5");
        _entranceSettingsPanel.SetActive(false);
        _editingEntrance = false;
        _movingEntrance = false;
        RemoveSegmentsSizeLabel();
    }
    #endregion

    #region --- Polygon Settings ---
    public void InitializePolygonSettingsPanel()
    {   // Initialize the polygon settings panel
        _polygonTranslatedLabelInput[0].text = _selectedPolygon.roomName.englishTranslation;
        _polygonTranslatedLabelInput[1].text = _selectedPolygon.roomName.spanishTranslation;

        _oldPolygonColor = _selectedPolygon.colorMaterial.GetColor("_Color1");
        _colorOpaquePreview.SetColor("_Color1", _oldPolygonColor);
        _colorOpaquePreview.SetColor("_Color2", _oldPolygonColor);
        _colorAlphaPreview.SetColor("_Color1", _oldPolygonColor);
        _colorAlphaPreview.SetColor("_Color2", _oldPolygonColor);
        _colorPicker.color = _oldPolygonColor;

        if (_selectedPolygon.roomType == "") _polygonTypeDropdown.value = 0;
        else _polygonTypeDropdown.value = _polygonsManager.roomsTypesList.IndexOf(_selectedPolygon.roomType);

        _selectedPolygon.nodes.ForEach(node => node.PlaySelectAnimation());
        _polygonLabelInput.text = _selectedPolygon.roomName.key;
        _polygonSettingsPanel.SetActive(true);
    }

    public void ChangePolygonColor()
    {   // Change the selected polygon color with the color picker value
        if (_selectedPolygon == null) return;
        _colorPicker.UpdateCustomMaterial(_selectedPolygon.colorMaterial, true);
        _colorPicker.UpdateCustomMaterial(_colorOpaquePreview, false);
        _colorPicker.UpdateCustomMaterial(_colorAlphaPreview, true);
        _roomTypePanel.SetActive(false);
    }

    public void SetPolygonSettings()
    {   // Set the polygon changes on confirmation
        if (_polygonLabelInput.text != "") // Set label
            _selectedPolygon.roomName.key = _polygonLabelInput.text;

        _selectedPolygon.roomName.englishTranslation = _polygonTranslatedLabelInput[0].text;
        _selectedPolygon.roomName.spanishTranslation = _polygonTranslatedLabelInput[1].text;
        _selectedPolygon.roomType = _polygonTypeDropdown.options[_polygonTypeDropdown.value].text;

        _colorPicker.gameObject.SetActive(false);
        _polygonSettingsPanel.SetActive(false);
        _roomTypePanel.SetActive(false);
    }
    public void CancelPolygonEdit()
    {   // Cancel the polygon edit and reset the polygon
        _selectedPolygon.colorMaterial.SetColor("_Color1", _oldPolygonColor);
        _colorPicker.gameObject.SetActive(false);
        _polygonSettingsPanel.SetActive(false);
    }

    public void ChangePolygonType()
    {   // Change the selected polygon type from dropdown
        if (_selectedPolygon == null) return;

        if (_polygonTypeDropdown.value == _polygonTypeDropdown.options.Count - 1)
        {   // Show the input field to enter a new type
            _roomTypePanel.SetActive(true);
            _colorPicker.gameObject.SetActive(false);
            _polygonTypeInput.text = "";
            _polygonTranslatedTypeInput[0].text = "";
            _polygonTranslatedTypeInput[1].text = "";
            _polygonTypeModeSlider.value = 0;
        }
        else
        {   // Set the selected type and show the fields to edit the type
            int _index = _polygonTypeDropdown.value;
            _colorPicker.gameObject.SetActive(false);

            if (_index == 0) // With unique type, hide the input fields
                _roomTypePanel.SetActive(false);
            else
            {   // Show the input fields to edit the type if is not unique
                _roomTypePanel.SetActive(true);
                _polygonTypeInput.text = _polygonTypeDropdown.options[_index].text;
                _polygonTranslatedTypeInput[0].text = _polygonsManager.roomsTypesData[_index].typeName.englishTranslation;
                _polygonTranslatedTypeInput[1].text = _polygonsManager.roomsTypesData[_index].typeName.spanishTranslation;
                _polygonTypeModeSlider.value = _polygonsManager.roomsTypesData[_index].searchNearestMode ? 1 : 0;
            }
            _selectedPolygon.roomType = _polygonTypeDropdown.options[_index].text;
        }
    }

    public void ConfirmTypeChange()
    {   // Confirm the type change
        if (_selectedPolygon == null) return;
        if (_polygonTypeInput.text == "")
        {   // Show an error message if the type name is empty
            _errorMessageBox.ShowMessage("EnterSomeValue");
            return;
        }
        _polygonsManager.AddRoomType(
            _polygonTypeInput.text,
            _polygonTranslatedTypeInput[0].text,
            _polygonTranslatedTypeInput[1].text,
            _polygonTypeModeSlider.value == 1 ? true : false
            );
        _errorMessageBox.HideMessage();
        _roomTypePanel.SetActive(false);
    }

    public void CancelTypeChange()
    {   // Cancel the type change
        _roomTypePanel.SetActive(false);
    }
    #endregion

    #region --- UI Labels ---
    private void ShowWallSegmentsSize(WallLineController _wall)
    {   // Show the wall segments size labels
        Tuple<List<Vector3[]>, List<float>> _wallSegments = _wall.GetWallSegments();
        List<Vector3[]> _wallPoints = _wallSegments.Item1;
        List<float> _wallSizes = _wallSegments.Item2;

        for (int i = 0; i < _wallPoints.Count; i++)
        {
            if (_segmentsLabels.Count < _wallPoints.Count)
            {   // Create the label if not exists
                GameObject _label = Instantiate(_wallLabelPrefab, new Vector3(0, 0, 0), Quaternion.identity, _UIItems);
                _label.name = "WallLabel_" + i.ToString();
                _segmentsLabels.Add(_label);
            }
            if (_wallSizes[i] < 0.001f) _segmentsLabels[i].SetActive(false); // ONLY WORKS ON START DOT (!)
            else
            {   // Set the label position and text
                Vector3 _labelPosition = (_wallPoints[i][0] + _wallPoints[i][1]) / 2;
                _segmentsLabels[i].transform.position = Camera.main.WorldToScreenPoint(_labelPosition);
                _segmentsLabels[i].GetComponentInChildren<TextMeshProUGUI>().text = _wallSizes[i].ToString("F2") + "m";
                _segmentsLabels[i].SetActive(true);
            }
        }
    }

    private void ShowWallSize()
    {   // Show the wall size label
        float _wallSize = _selectedWall.CalculateLength();
        Vector3 _labelPosition = (_selectedWall.endNode.GetNodePosition() + _selectedWall.startNode.GetNodePosition()) / 2;
        _wallSizeLabel.transform.position = Camera.main.WorldToScreenPoint(_labelPosition);
        _wallSizeLabel.GetComponentInChildren<TextMeshProUGUI>().text = _wallSize.ToString("F2") + "m";
        _wallSizeLabel.SetActive(true);
    }

    private void ShowWallsSizeLabel()
    {   // Show the walls labels connected to the selected dot
        for (int i = 0; i < _selectedDot.walls.Count; i++)
        {
            if (_wallLabels.Count < _selectedDot.walls.Count)
            {   // Create the label if not exists
                GameObject _label = Instantiate(_wallLabelPrefab, new Vector3(0, 0, 0), Quaternion.identity, _UIItems);
                _label.name = "WallLabel_" + i.ToString();
                _wallLabels.Add(_label);
            }
            WallLineController _line = _selectedDot.walls[i].GetComponent<WallLineController>();
            Vector3 _labelPosition = (_line.endNode.GetNodePosition() + _line.startNode.GetNodePosition()) / 2;

            _wallLabels[i].GetComponentInChildren<TextMeshProUGUI>().text = _line.length.ToString("F2") + "m";
            _wallLabels[i].transform.position = Camera.main.WorldToScreenPoint(_labelPosition);
            _wallLabels[i].SetActive(true);
        }
    }

    private void RemoveWallsSizeLabel()
    {   // Destroy the walls labels
        for (int i = 0; i < _wallLabels.Count; i++)
        {
            Destroy(_wallLabels[i]);
        }
        _wallLabels.Clear();
    }

    private void RemoveSegmentsSizeLabel()
    {   // Destroy the walls labels
        for (int i = 0; i < _segmentsLabels.Count; i++)
        {
            Destroy(_segmentsLabels[i]);
        }
        _segmentsLabels.Clear();
    }
    #endregion
}
