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
        _cursorPosition.y = 0;

        if (_gridManager.snapToGrid && _considerSnap)
        {
            _cursorPosition.x = Mathf.Round(_cursorPosition.x / _gridManager.gridSize);
            _cursorPosition.z = Mathf.Round(_cursorPosition.z / _gridManager.gridSize);
        }
        return _cursorPosition;
    }

    private WallDotController InstantiateWallDot(Vector3 _position, int _type = 0)
    {   // Instantiate a wall dot and atach it to a line
        GameObject _wallDot = Instantiate(_dotPrefab, _position, Quaternion.identity, _dotsParent);
        _wallDot.name = ((_type == 0) ? "Start" : "End") + "Dot_Wall_" + _linesCount;
        WallDotController _wallDotController = _wallDot.GetComponent<WallDotController>();
        _wallDot.transform.localRotation = Quaternion.Euler(0, 0, 0);
        return _wallDotController;
    }

    private void CancelDraw()
    {   // Cancel the current line drawing
        if (_endWallDot != null) _endWallDot.DeleteDot();
        _drawingWall = false;
        _endWallDot = null;
        _lineObject = null;
        _linesCount--;
    }

    private void Update()
    {
        if (_drawingWall && !IsDrawingInsideCanvas())
        {   // Cancel the drawing if the mouse is outside the canvas
            CancelDraw();
            return;
        }
        else if (_drawingWall && _lineObject != null)
        {   // Drag the line updating the end dot position
            _endWallDot.SetPosition(GetCursorPosition());
            WallSizeOnGUI();
        }
        else _wallSizeLabel.SetActive(false);
    }

    private void NewWall()
    {   // Create a new wall with the line and the start and end dots
        if (!IsDrawingInsideCanvas()) return;
        Vector3 _cursorPosition = GetCursorPosition(true);
        WallDotController raycast_dot = RaycastToDot();

        if (raycast_dot && !_drawingWall)
        {   // If press when the cursor is over a dot, create a line from this dot
            raycast_dot.PlayHoverAnimation();
            Vector3 _noSnapPosition = GetCursorPosition(false);
            _startWallDot = raycast_dot;
            _endWallDot = InstantiateWallDot(_noSnapPosition, 1);
            _lineObject = CreateLine(_noSnapPosition);
            _drawingWall = true;
        }
        else if (raycast_dot && _drawingWall && raycast_dot != _endWallDot)
        {   // But if was already drawing, end the line and add a new line from this dot
            raycast_dot.PlayHoverAnimation();
            _endWallDot.SetPosition(raycast_dot.position);
            SetLineDots(_lineObject, _startWallDot, raycast_dot);
            _endWallDot.DeleteDot(false);
            _endWallDot = raycast_dot;

            _startWallDot = _endWallDot;
            _endWallDot = InstantiateWallDot(_cursorPosition, 1);
            _lineObject = CreateLine(_cursorPosition);
        }
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

    private WallDotController RaycastToDot()
    {   // Raycast to the dots to check if the mouse is over one of them
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

        SetLineDots(_newLine, _startWallDot, _endWallDot);
        _linesCount++;

        return _newLine;
    }

    private void SetLineDots(GameObject _line, WallDotController _startDot, WallDotController _endDot)
    {   // Set the dots of a line and add the line to the dots
        WallLineController _lineController = _line.GetComponent<WallLineController>();
        _lineController.startDot = _startWallDot;
        _lineController.endDot = _endWallDot;
        _startDot.AddLine(_line, 0, _endDot);
        _endDot.AddLine(_line, 1, _startDot);
    }

    private void WallSizeOnGUI()
    {   // Display a label of wall size (on meters)
        float _wallSize = _lineObject.GetComponent<WallLineController>().CalculateLength();

        if (_wallSize < 0.0001f)
            _wallSizeLabel.SetActive(false);
        else
        {
            Vector3 _labelPosition = (_endWallDot.position + _startWallDot.position) / 2;
            _labelPosition = _labelPosition + new Vector3(0, 0, 0.5f);

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
                _rect, Mouse.current.position.ReadValue(), null))
            { return false; }
        }
        return true;
    }
}
