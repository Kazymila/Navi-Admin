using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SelectTool : MonoBehaviour
{
    #region --- External Variables ---
    [Header("UI Components")]
    [SerializeField] private EditorLayoutController _UIEditorController;
    [SerializeField] private ErrorMessageController _errorMessageBox;
    [SerializeField] private GameObject _wallSizePanel;
    [SerializeField] private Transform _UIItems;

    [Header("Map Editor Components")]
    [SerializeField] private MapEditorGridManager _gridManager;
    [SerializeField] private GameObject _wallLabelPrefab;
    [SerializeField] private GameObject _wallSizeLabel;
    #endregion

    private List<GameObject> _wallLabels = new List<GameObject>();
    private WallLineController _selectedLine;
    private WallDotController _selectedDot;
    private bool _movingDot;

    private TMP_InputField _wallSizeInput;
    private bool _changingLineSize;
    private float _oldWallSize;

    private InputMap _input;

    private void OnEnable()
    {
        _input = new InputMap();
        _input.MapEditor.Enable();
        _input.MapEditor.Click.started += ctx => OnSelectClick();
    }
    private void OnDisable() => _input.MapEditor.Disable();

    private void Start()
    {
        _wallSizeInput = _wallSizePanel.GetComponentInChildren<TMP_InputField>();
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
    }

    private void OnSelectClick()
    {   // Select the object under the cursor to do something with it
        if (_UIEditorController.IsCursorOverEditorUI()) return;

        if (_movingDot)
        {   // Set the dot position and stop moving it
            _selectedDot.PlaySelectAnimation();
            _movingDot = false;
        }
        else
        {
            if (!_changingLineSize) _wallSizeLabel.SetActive(false);
            RaycastToObject();
        }
        RemoveWallsSizeLabel();
    }

    private void RaycastToObject()
    {   // Raycast to the object under the cursor to select it
        RaycastHit2D _hit = Physics2D.Raycast(GetCursorPosition(false), Vector2.zero);

        if (_hit.collider != null)
        {
            if (_hit.collider.CompareTag("WallDot"))
            {   // Select the dot and start moving it
                if (_changingLineSize) CancelLineSizeChange();
                _selectedDot = _hit.collider.GetComponent<WallDotController>();
                _selectedDot.PlaySelectAnimation();
                _movingDot = true;
            }
            else if (_hit.collider.CompareTag("Wall"))
            {   // Select the wall and show its size
                _selectedLine = _hit.collider.GetComponent<WallLineController>();
                _selectedLine.startDot.PlaySelectAnimation();
                _selectedLine.endDot.PlaySelectAnimation();

                _wallSizeInput.text = _selectedLine.CalculateLength().ToString("F2");
                _oldWallSize = _selectedLine.CalculateLength();
                _wallSizePanel.SetActive(true);
                _changingLineSize = true;
                ShowWallSize();
            }
        }
        else CancelLineSizeChange();
    }

    #region --- Change Wall Line Size ---
    public void ChangeLineSize()
    {   // Change the selected line size with the input field value
        string _inputText = _wallSizeInput.text;
        _errorMessageBox.HideMessage();

        if (_inputText == "") return;
        else if (float.Parse("0" + _inputText) == 0) return;
        else
        {
            float _newSize = float.Parse("0" + _inputText);
            _selectedLine.ChangeSize(_newSize);
            ShowWallSize();
        }
    }
    public void SetLineSize()
    {   // Set the new line size on confirmation
        if (_wallSizeInput.text == "")
            _errorMessageBox.ShowMessage("Please enter a value");
        else
        {
            _wallSizePanel.SetActive(false);
            _wallSizeLabel.SetActive(false);
            _changingLineSize = false;
        }
    }

    public void CancelLineSizeChange(bool _fromButton = false)
    {   // Cancel the line size change and reset the line
        if (!_fromButton)
        {   // If not canceled by the button, check if the cursor is over the panel
            bool _isOverPanel = RectTransformUtility.RectangleContainsScreenPoint(
                _wallSizePanel.GetComponent<RectTransform>(),
                _input.MapEditor.Position.ReadValue<Vector2>(), null);
            if (_isOverPanel) return;
        }
        _wallSizeInput.text = _oldWallSize.ToString("F2");
        _selectedLine.ChangeSize(_oldWallSize);
        _wallSizePanel.SetActive(false);
        _wallSizeLabel.SetActive(false);
        _changingLineSize = false;
    }
    #endregion

    #region --- Wall Size Label ---
    private void ShowWallSize()
    {   // Show the wall size label
        float _wallSize = _selectedLine.CalculateLength();
        Vector3 _labelPosition = (_selectedLine.endDot.position + _selectedLine.startDot.position) / 2;
        _wallSizeLabel.transform.position = Camera.main.WorldToScreenPoint(_labelPosition);
        _wallSizeLabel.GetComponentInChildren<TextMeshProUGUI>().text = _wallSize.ToString("F2") + "m";
        _wallSizeLabel.SetActive(true);
    }

    private void ShowWallsSizeLabel()
    {   // Show the walls labels connected to the selected dot
        for (int i = 0; i < _selectedDot.linesCount; i++)
        {
            if (_wallLabels.Count < _selectedDot.linesCount)
            {
                GameObject _label = Instantiate(_wallLabelPrefab, new Vector3(0, 0, 0), Quaternion.identity, _UIItems);
                _label.name = "WallLabel_" + i.ToString();
                _wallLabels.Add(_label);
            }
            WallLineController _line = _selectedDot.lines[i].GetComponent<WallLineController>();
            Vector3 _labelPosition = (_line.endDot.position + _line.startDot.position) / 2;

            float _wallSize = _line.CalculateLength();
            _wallLabels[i].GetComponentInChildren<TextMeshProUGUI>().text = _wallSize.ToString("F2") + "m";
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
    #endregion
}
