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

    [Header("Dots settings")]
    [SerializeField] private GameObject _dotPrefab;

    [Header("Line settings")]
    [SerializeField] private GameObject _linePrefab;
    [SerializeField] private Transform _linesParent;

    [Header("Mesh settings")]
    [SerializeField] private GameObject _shapeMeshPrefab;
    [SerializeField] private Transform _shapesMeshParent;
    [SerializeField] private Color _meshColor = new Color(0.26f, 0, 0.68f, 0.5f);
    #endregion
    private ShapeController _currentShape;
    private int _shapesCount = 0;
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
        _cursorPosition.z = -0.2f;

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
            GameObject _newShape = Instantiate(_linePrefab, _cursorPosition, Quaternion.identity, _linesParent);
            _currentShape = _newShape.GetComponent<ShapeController>();
            _newShape.name = "Shape_" + _shapesCount;
            InstantiateDot(_cursorPosition);
            _shapesCount++;
        }
        InstantiateDot(_cursorPosition);
        _isDrawing = true;
    }

    private void InstantiateDot(Vector3 _position)
    {   // Instantiate a new dot in the current shape
        GameObject _newDot = Instantiate(_dotPrefab, _position + _currentShape.dotOffset,
                            Quaternion.Euler(-90, 0, 0), _currentShape.transform);
        _newDot.name = "ShapeDot_" + _currentShape.GetPointsCount();
        _currentShape.AddPoint(_newDot.transform);
    }

    private void EndShape()
    {   // Close the current shape
        if (_currentShape == null) return;
        _currentShape.RemoveLastPoint();
        _currentShape.EndShape();

        // Create the mesh (polygon) of the shape
        GameObject _newShapeMesh = Instantiate(_shapeMeshPrefab,
            Vector3.zero + new Vector3(0, 0, -0.1f), Quaternion.identity, _shapesMeshParent);
        _newShapeMesh.GetComponent<MeshRenderer>().material.SetColor("_Color1", _meshColor);
        _newShapeMesh.name = "ShapePolygon_" + (_shapesCount - 1);
        _currentShape.CreatePolygonMesh(_newShapeMesh);
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
            _shapesCount--;
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
