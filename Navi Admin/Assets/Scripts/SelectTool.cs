using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SelectTool : MonoBehaviour
{
    [Header("Required Stuff")]
    [SerializeField] private EditorLayoutController _UIEditorController;
    [SerializeField] private MapEditorGridManager _gridManager;
    [SerializeField] private GameObject _wallLabelPrefab;
    [SerializeField] private GameObject _wallSizeLabel;
    [SerializeField] private Transform _UIItems;

    private List<GameObject> _wallLabels = new List<GameObject>();
    private WallLineController _selectedLine;
    private WallDotController _selectedDot;
    private bool _movingDot;

    private InputMap _input;

    private void OnEnable()
    {
        _input = new InputMap();
        _input.MapEditor.Enable();
        _input.MapEditor.Click.started += ctx => OnSelectClick();
    }
    private void OnDisable() => _input.MapEditor.Disable();

    private Vector3 GetCursorPosition()
    {   // Get the cursor position in the world
        Vector3 _cursorPosition = Camera.main.ScreenToWorldPoint(_input.MapEditor.Position.ReadValue<Vector2>());
        _cursorPosition.z = 0;

        if (_gridManager.snapToGrid)
        {
            _cursorPosition.x = Mathf.Round(_cursorPosition.x / _gridManager.gridSize);
            _cursorPosition.y = Mathf.Round(_cursorPosition.y / _gridManager.gridSize);
        }

        return _cursorPosition;
    }

    private void Update()
    {
        if (_movingDot && !_UIEditorController.IsCursorOverEditorUI())
        {
            _selectedDot.SetPosition(GetCursorPosition());
            ShowWallsSizeLabel();
        }
    }

    private void OnSelectClick()
    {   // Select the object under the cursor to do something with it
        if (_UIEditorController.IsCursorOverEditorUI()) return;

        if (_movingDot)
        {   // Set the dot position and stop moving it
            _selectedDot.PlayHoverAnimation();
            _movingDot = false;
        }
        else
        {
            _wallSizeLabel.SetActive(false);
            RaycastToObject();
        }
        RemoveWallsSizeLabel();
    }

    private void RaycastToObject()
    {   // Raycast to the object under the cursor to select it
        RaycastHit2D _hit = Physics2D.Raycast(GetCursorPosition(), Vector2.zero);

        if (_hit.collider != null)
        {
            if (_hit.collider.CompareTag("WallDot"))
            {   // Select the dot and start moving it
                _selectedDot = _hit.collider.GetComponent<WallDotController>();
                _selectedDot.PlayHoverAnimation();
                _movingDot = true;
            }
            else if (_hit.collider.CompareTag("Wall"))
            {   // Select the wall and show its size
                _selectedLine = _hit.collider.GetComponent<WallLineController>();
                _selectedLine.startDot.PlayHoverAnimation();
                _selectedLine.endDot.PlayHoverAnimation();

                // Show the wall size label
                float _wallSize = _selectedLine.CalculateLength();
                Vector3 _labelPosition = (_selectedLine.endDot.position + _selectedLine.startDot.position) / 2;
                _wallSizeLabel.transform.position = Camera.main.WorldToScreenPoint(_labelPosition);
                _wallSizeLabel.GetComponentInChildren<TextMeshProUGUI>().text = _wallSize.ToString("F2") + "m";
                _wallSizeLabel.SetActive(true);
            }
        }
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
}
