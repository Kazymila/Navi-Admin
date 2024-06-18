using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ShapeTool : MonoBehaviour
{
    #region --- External Variables ---
    [Header("Required Stuff")]
    [SerializeField] private EditorLayoutController _UIEditorController;
    [SerializeField] private ErrorMessageController _errorMessageBox;
    [SerializeField] private MapEditorGridManager _gridManager;
    [SerializeField] private GameObject _sizeLabel;

    [Header("Shapes settings")]
    [SerializeField] private GameObject _linePrefab;
    [SerializeField] private Transform _shapeParent;

    #endregion
    private ShapeController _currentShape;
    private bool _isDrawing;

    private InputMap _input;

    private void OnEnable()
    {
        _input = new InputMap();
        _input.MapEditor.Enable();
        _input.MapEditor.Click.started += ctx => NewShape();
        _input.MapEditor.EndDraw.started += ctx => CancelDraw();
    }
    private void OnDisable() => _input.MapEditor.Disable();

    private Vector3 GetCursorPosition(bool _considerSnap = true)
    {   // Get the cursor position in the world
        Vector3 _cursorPosition = Camera.main.ScreenToWorldPoint(_input.MapEditor.Position.ReadValue<Vector2>());
        _cursorPosition.z = _shapeParent.position.z;

        if (_gridManager.snapToGrid && _considerSnap)
        {
            _cursorPosition.x = Mathf.Round(_cursorPosition.x / _gridManager.gridSize);
            _cursorPosition.y = Mathf.Round(_cursorPosition.y / _gridManager.gridSize);
        }
        return _cursorPosition;
    }

    private void Update()
    {
        if (_UIEditorController.IsCursorOverEditorUI()) return;

        if (_isDrawing)
        {   // Update the last point of the current shape
            Vector3 _cursorPosition = GetCursorPosition(true);
            _currentShape.UpdateLastPoint(_cursorPosition);
            ShowLastSegmentLength();
        }
    }

    private void NewShape()
    {   // Create a new shape or add a new point to the current shape
        if (_UIEditorController.IsCursorOverEditorUI()) return;
        Vector3 _cursorPosition = GetCursorPosition(true);

        if (_currentShape == null)
        {   // Create a new shape
            GameObject _newShape = Instantiate(_linePrefab, _cursorPosition, Quaternion.identity, _shapeParent);
            _currentShape = _newShape.GetComponent<ShapeController>();
            _newShape.name = "Shape_" + (_shapeParent.childCount > 0 ? (_shapeParent.childCount - 1) : 0);
            _currentShape.InstantiateDot(_cursorPosition);
        }
        _currentShape.InstantiateDot(_cursorPosition);
        _isDrawing = true;
    }

    private void EndShape()
    {   // Close the current shape
        if (_currentShape == null) return;
        _currentShape.RemoveLastPoint();
        _currentShape.EndShape();

        // Create the mesh (polygon) of the shape
        _currentShape.CreateShapePolygon();
        _currentShape = null;
        _isDrawing = false;
    }

    private void CancelDraw()
    {   // Cancel the current drawing or end the shape
        if (_currentShape == null) return;

        if (_currentShape.GetPointsCount() <= 2)
        {   // Destroy the shape if it has less than 2 points
            Destroy(_currentShape.gameObject);
            _currentShape = null;
            _isDrawing = false;
        }
        else EndShape();
        _sizeLabel.SetActive(false);
    }

    private void ShowLastSegmentLength()
    {   // Show the length of the last segment of the current shape
        if (_currentShape == null) return;
        float _length = _currentShape.GetLastSegmentLength();
        Vector3 _labelPosition = _currentShape.GetLastSegmentCenter();
        _labelPosition = _labelPosition + new Vector3(0, 0.5f, 0);

        _sizeLabel.transform.position = Camera.main.WorldToScreenPoint(_labelPosition);
        _sizeLabel.GetComponentInChildren<TextMeshProUGUI>().text = _length.ToString("F2") + "m";
        _sizeLabel.SetActive(true);
    }
}
