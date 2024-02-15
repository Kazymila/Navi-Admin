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
        _input.MapEditor.Draw.started += ctx => NewWall();
        //_input.MapEditor.Draw.performed += ctx => _drawingWall = true;
        //_input.MapEditor.Draw.canceled += ctx => SetWall();
        _input.MapEditor.EndDraw.started += ctx => CancelDraw();
    }
    private void OnDisable() => _input.MapEditor.Disable();

    private void Start()
    {
        _UIRects = new RectTransform[_UILayout.childCount];

        for (int i = 0; i < _UILayout.childCount; i++)
            _UIRects[i] = _UILayout.GetChild(i).GetComponent<RectTransform>();
    }

    private Vector3 GetMousePosition()
    {
        Vector3 _mousePosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        _mousePosition.y = 0;

        if (_gridManager.snapToGrid)
        {
            _mousePosition.x = Mathf.Round(_mousePosition.x / _gridManager.gridSize);
            _mousePosition.z = Mathf.Round(_mousePosition.z / _gridManager.gridSize);
        }
        return _mousePosition;
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
            _endWallDot.SetPosition(GetMousePosition());
            WallSizeOnGUI();
        }
        else _wallSizeLabel.SetActive(false);
    }

    private void NewWall()
    {   // Create a new wall with the line and the start and end dots
        if (!IsDrawingInsideCanvas()) return;
        Vector3 _mousePosition = GetMousePosition();

        if (_lineObject == null && !_drawingWall)
        {   // Create the first dot and line
            _startWallDot = InstantiateWallDot(_mousePosition, 0);
            _endWallDot = InstantiateWallDot(_mousePosition, 1);
            _lineObject = CreateLine(_mousePosition);
            _drawingWall = true;
        }
        else if (_lineObject != null && _drawingWall)
        {   // Set dot and add line from this last dot
            _endWallDot.SetPosition(GetMousePosition());

            _startWallDot = _endWallDot;
            _endWallDot = InstantiateWallDot(_mousePosition, 1);
            _lineObject = CreateLine(_mousePosition);
        }
    }

    private GameObject CreateLine(Vector3 _position)
    {   // Create a new line renderer
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
    {   // Set the dots of a line
        WallLineController _lineController = _line.GetComponent<WallLineController>();
        _lineController.startDot = _startWallDot;
        _lineController.endDot = _endWallDot;
        _startDot.AddLine(_line, 0, _endDot);
        _endDot.AddLine(_line, 1, _startDot);
    }

    private void WallSizeOnGUI()
    {   // Display a label of wall size (on meters)
        LineRenderer _line = _lineObject.GetComponent<LineRenderer>();

        float _wallSize = Math.Max(
            Math.Abs(_line.GetPosition(1).x - _line.GetPosition(0).x),
            Math.Abs(_line.GetPosition(1).z - _line.GetPosition(0).z)
            );
        if (_wallSize == 0.0f)
            _wallSizeLabel.SetActive(false);
        else
        {
            Vector3 _labelPosition = _line.GetPosition(1) + new Vector3(0, 0, 0.5f);
            _wallSizeLabel.transform.position = Camera.main.WorldToScreenPoint(_labelPosition);
            _wallSizeLabel.GetComponentInChildren<TextMeshProUGUI>().text = _wallSize.ToString("F2") + "m";
            _wallSizeLabel.SetActive(true);
        }
    }
    private bool IsDrawingInsideCanvas()
    {
        foreach (RectTransform _rect in _UIRects)
        {   // Check if the mouse is not in the UI
            if (RectTransformUtility.RectangleContainsScreenPoint(
                _rect, Mouse.current.position.ReadValue(), null))
            {
                print("OUTSIDE DRAW CANVAS");
                return false;
            }
        }
        print("INSIDE DRAW CANVAS");
        return true;
    }
}
