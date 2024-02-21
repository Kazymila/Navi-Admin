using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.UI;
using TMPro;

public class WallDrawer : MonoBehaviour
{
    [Header("Required Stuff")]
    [SerializeField] private MapEditorGridManager _gridManager;
    [SerializeField] private GameObject _wallSizeLabel;
    [SerializeField] private Transform _UILayout;

    [Header("Dots settings")]
    [SerializeField] private GameObject _dotPrefab;
    [SerializeField] private Transform _dotsParent;

    [Header("Line settings")]
    [SerializeField] private GameObject _linePrefab;
    [SerializeField] private Transform _linesParent;

    private RectTransform[] _UIRects;
    private GameObject _lineObject;
    private WallLineController _lineController;
    private WallDotController _startWallDot;
    private WallDotController _endWallDot;
    private int _linesCount = 0;
    private bool _drawingWall;

    private InputMap _input;

    private void OnEnable()
    {
        _input = new InputMap();
        _input.MapEditor.Enable();
        _input.MapEditor.Click.started += ctx => NewWall();
        _input.MapEditor.EndDraw.started += ctx => CancelDraw();
    }
    private void OnDisable() => _input.MapEditor.Disable();

    private void Start()
    {
        _UIRects = new RectTransform[_UILayout.childCount];

        for (int i = 0; i < _UILayout.childCount; i++)
            _UIRects[i] = _UILayout.GetChild(i).GetComponent<RectTransform>();
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

    private WallDotController InstantiateWallDot(Vector3 _position, int _type = 0)
    {   // Instantiate a wall dot and atach it to a line
        Vector3 _dotPosition = _position + new Vector3(0, 0, -0.5f);
        GameObject _wallDot = Instantiate(_dotPrefab, _dotPosition, Quaternion.identity, _dotsParent);
        WallDotController _wallDotController = _wallDot.GetComponent<WallDotController>();
        _wallDot.name = ((_type == 0) ? "Start" : "End") + "Dot_Wall_" + _linesCount;
        _wallDot.transform.localRotation = Quaternion.Euler(-90, 0, 0);
        return _wallDotController;
    }

    private void CancelDraw()
    {   // Cancel the current line drawing
        if (_endWallDot != null)
        {
            _startWallDot.DeleteLine(_startWallDot.lines.IndexOf(_lineObject));
            _endWallDot.DeleteDot(true);
        }
        _drawingWall = false;
        _endWallDot = null;
        _lineObject = null;
        _linesCount--;
    }

    private void Update()
    {
        if (_drawingWall && !IsDrawingInsideCanvas())
        {   // Cancel the drawing if the cursor is outside the canvas
            CancelDraw();
            return;
        }
        else if (_drawingWall && _lineObject != null)
        {   // Drag the line updating the end dot position
            _endWallDot.SetPosition(GetCursorPosition());
            _endWallDot.DotCollider.enabled = false;
            WallSizeOnGUI();
        }
        else _wallSizeLabel.SetActive(false);
    }

    private void NewWall()
    {   // Create a new wall with the line and the start and end dots
        if (!IsDrawingInsideCanvas()) return;
        WallDotController _raycastDot = RaycastToDot();
        Vector3 _cursorPosition = GetCursorPosition(true);
        if (_endWallDot) _endWallDot.DotCollider.enabled = true;

        // If the cursor is over a dot, wall is created or finished here
        if (_raycastDot) OnSelectDot(_raycastDot);

        else if (_lineObject == null && !_drawingWall)
        {   // Create the first dot and line
            _startWallDot = InstantiateWallDot(_cursorPosition, 0);
            _endWallDot = InstantiateWallDot(_cursorPosition, 1);
            _lineObject = CreateLine(_cursorPosition);
            _drawingWall = true;
        }
        else if (_lineObject != null && _drawingWall)
        {   // Set dot and add line from this last dot
            _endWallDot.SetPosition(GetCursorPosition());

            _startWallDot = _endWallDot;
            _endWallDot = InstantiateWallDot(_cursorPosition, 1);
            _lineObject = CreateLine(_cursorPosition);
        }
    }

    private void OnSelectDot(WallDotController _raycastDot)
    {
        if (!_drawingWall)
        {   // Create a line from the selected dot
            _raycastDot.PlayHoverAnimation();
            Vector3 _noSnapPosition = GetCursorPosition(false);

            _startWallDot = _raycastDot;
            _endWallDot = InstantiateWallDot(_noSnapPosition, 1);
            _lineObject = CreateLine(_noSnapPosition);
            _drawingWall = true;
        }
        else
        {   // If was already drawing, the selected dot is setted as the end dot
            // But, if the dots are already connected, cannot set the dot here
            if (_raycastDot.FindNeighbor(_startWallDot))
                _raycastDot.PlayDeniedAnimation();
            else
            {   // End the line and add a new line from this dot
                _raycastDot.PlayHoverAnimation();
                Vector3 _cursorPosition = GetCursorPosition(true);

                _endWallDot.SetPosition(_raycastDot.position);
                SetLineDots(_lineObject, _startWallDot, _raycastDot);
                _endWallDot.DeleteDot(false);
                _endWallDot = _raycastDot;

                _startWallDot = _endWallDot;
                _endWallDot = InstantiateWallDot(_cursorPosition, 1);
                _lineObject = CreateLine(_cursorPosition);
            }
        }
    }

    private WallDotController RaycastToDot()
    {   // Raycast to the dots to check if the mouse is over one of them
        RaycastHit2D _hit = Physics2D.Raycast(GetCursorPosition(), Vector2.zero);

        if (_hit.collider != null && _hit.collider.CompareTag("WallDot"))
        {
            WallDotController _dot = _hit.collider.GetComponent<WallDotController>();
            return _dot;
        }
        else return null;
    }

    private WallDotController RaycastToDotsZAxis()
    {   // Raycast to the dots on z as y axis, to check if the mouse is over one of them
        RaycastHit _hit;
        Ray _ray = Camera.main.ScreenPointToRay(_input.MapEditor.Position.ReadValue<Vector2>());
        if (Physics.Raycast(_ray, out _hit, Mathf.Infinity))
        {
            if (_hit.collider.CompareTag("WallDot"))
            {
                WallDotController _dot = _hit.collider.GetComponent<WallDotController>();
                return _dot;
            }
            else return null;
        }
        else return null;
    }

    private GameObject CreateLine(Vector3 _position)
    {   // Create a new line renderer and set the start and end dots
        GameObject _newLine = Instantiate(_linePrefab, _position, Quaternion.identity, _linesParent);
        _newLine.name = "Wall_" + _linesCount;

        LineRenderer _line = _newLine.GetComponent<LineRenderer>();
        _line.SetPosition(0, _position);
        _line.SetPosition(1, _position);

        _lineController = _newLine.GetComponent<WallLineController>();
        SetLineDots(_newLine, _startWallDot, _endWallDot);
        _linesCount++;

        return _newLine;
    }

    private void SetLineDots(GameObject _line, WallDotController _startDot, WallDotController _endDot)
    {   // Set the dots of a line and add the line to the dots
        _lineController.startDot = _startDot;
        _lineController.endDot = _endDot;
        _startDot.AddLine(_line, 0, _endDot);
        _endDot.AddLine(_line, 1, _startDot);
    }

    private void WallSizeOnGUI()
    {   // Display a label of wall size (on meters)
        float _wallSize = _lineController.CalculateLength();

        if (_wallSize < 0.0001f)
            _wallSizeLabel.SetActive(false);
        else
        {
            Vector3 _labelPosition = (_endWallDot.position + _startWallDot.position) / 2;
            _labelPosition = _labelPosition + new Vector3(0, 0.5f, 0);

            _wallSizeLabel.transform.position = Camera.main.WorldToScreenPoint(_labelPosition);
            _wallSizeLabel.GetComponentInChildren<TextMeshProUGUI>().text = _wallSize.ToString("F2") + "m";
            _wallSizeLabel.SetActive(true);
        }
    }

    private bool IsDrawingInsideCanvas()
    {   // Check if the mouse is not in the UI, to avoid drawing over the UI
        foreach (RectTransform _rect in _UIRects)
        {
            if (RectTransformUtility.RectangleContainsScreenPoint(
                _rect, _input.MapEditor.Position.ReadValue<Vector2>(), null))
            { return false; }
        }
        return true;
    }
}
