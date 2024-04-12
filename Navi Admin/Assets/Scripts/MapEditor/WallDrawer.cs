using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class WallDrawer : MonoBehaviour
{
    #region --- External Variables ---
    [Header("Required Stuff")]
    [SerializeField] private EditorLayoutController _UIEditorController;
    [SerializeField] private ErrorMessageController _errorMessageBox;
    [SerializeField] private MapEditorGridManager _gridManager;
    [SerializeField] private GameObject _sizeLabel;

    [Header("Dots settings")]
    [SerializeField] private GameObject _dotPrefab;
    [SerializeField] private Transform _dotsParent;

    [Header("Line settings")]
    [SerializeField] private GameObject _linePrefab;
    [SerializeField] private Transform _linesParent;
    #endregion
    private GameObject _lineObject;
    private WallLineController _lineController;
    private WallDotController _startWallDot;
    private WallDotController _endWallDot;
    private float _wallWidth = 0.15f;
    private int _linesCount = 0;
    private bool _isDrawing;

    private InputMap _input;

    private void OnEnable()
    {
        _input = new InputMap();
        _input.MapEditor.Enable();
        _input.MapEditor.Click.started += ctx => NewWall();
        _input.MapEditor.EndDraw.started += ctx => CancelDraw();
    }
    private void OnDisable() => _input.MapEditor.Disable();

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
        if (_isDrawing && _UIEditorController.IsCursorOverEditorUI())
        {   // Cancel the drawing if the cursor is outside the canvas
            CancelDraw();
            return;
        }
        else if (_isDrawing && _lineObject != null)
        {   // Drag the line updating the end dot position
            _endWallDot.SetPosition(GetCursorPosition());
            _endWallDot.dotCollider.enabled = false;
            _lineController.gameObject.GetComponent<LineRenderer>().startWidth = _wallWidth;
            _lineController.gameObject.GetComponent<LineRenderer>().endWidth = _wallWidth;
            WallSizeOnGUI();
        }
        else _sizeLabel.SetActive(false);
    }

    #region --- Wall Creation ---
    private void NewWall()
    {   // Create a new wall with the line and the start and end dots
        if (_UIEditorController.IsCursorOverEditorUI()) return;
        WallDotController _raycastDot = RaycastToDot();
        Vector3 _cursorPosition = GetCursorPosition(true);
        if (_endWallDot) _endWallDot.dotCollider.enabled = true;

        // If the cursor is over a dot, wall is created or finished here
        if (_raycastDot) OnSelectDot(_raycastDot);

        else if (_lineObject == null && !_isDrawing)
        {   // Create the first dot and line
            _startWallDot = InstantiateWallDot(_cursorPosition);
            _endWallDot = InstantiateWallDot(_cursorPosition);
            _lineObject = CreateLine(_cursorPosition);
            _isDrawing = true;
            _lineObject.GetComponent<WallLineController>().isDrawing = true;
        }
        else if (_lineObject != null && _isDrawing)
        {   // Set dot and add line from this last dot
            _endWallDot.SetPosition(GetCursorPosition());

            _startWallDot = _endWallDot;
            _endWallDot = InstantiateWallDot(_cursorPosition);
            _lineObject = CreateLine(_cursorPosition);
        }
    }

    private GameObject CreateLine(Vector3 _position)
    {   // Create a new line renderer and set the start and end dots
        GameObject _newLine = Instantiate(_linePrefab, _position, Quaternion.identity, _linesParent);
        _newLine.name = "Wall_" + _linesCount;

        LineRenderer _line = _newLine.GetComponent<LineRenderer>();
        _line.SetPosition(0, _position);
        _line.SetPosition(1, _position);
        _line.startWidth = _wallWidth;
        _line.endWidth = _wallWidth;

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
    private WallDotController InstantiateWallDot(Vector3 _position)
    {   // Instantiate a wall dot and atach it to a line
        GameObject _wallDot = Instantiate(_dotPrefab, _position, Quaternion.identity, _dotsParent);
        return _wallDot.GetComponent<WallDotController>();
    }

    private void CancelDraw()
    {   // Cancel the current line drawing
        if (_endWallDot != null)
        {
            _startWallDot.DeleteLine(_startWallDot.lines.IndexOf(_lineObject));
            _endWallDot.DeleteDot(true);
        }
        _lineObject.GetComponent<WallLineController>().isDrawing = true;
        _isDrawing = false;
        _endWallDot = null;
        _lineObject = null;
        _linesCount--;
    }
    #endregion

    #region --- Dots Raycasting ---
    private void OnSelectDot(WallDotController _raycastDot)
    {
        if (!_isDrawing)
        {   // Create a line from the selected dot
            _startWallDot = _raycastDot;
            _raycastDot.PlaySelectAnimation();
            _endWallDot = InstantiateWallDot(_raycastDot.position);
            _lineObject = CreateLine(_raycastDot.position);
            _isDrawing = true;
            _lineObject.GetComponent<WallLineController>().isDrawing = true;
        }
        else
        {   // If was already drawing, the selected dot is setted as the end dot
            if (_raycastDot.FindNeighborDot(_startWallDot))
            {   // Iif the dots are already connected, cannot set the dot here
                _errorMessageBox.ShowTimedMessage("DotsAlreadyConnected", 2);
                _raycastDot.PlayDeniedAnimation();
            }
            else
            {   // End the line and add a new line from this dot
                _raycastDot.PlaySelectAnimation();
                Vector3 _cursorPosition = GetCursorPosition(true);

                _endWallDot.SetPosition(_raycastDot.position);
                SetLineDots(_lineObject, _startWallDot, _raycastDot);
                _endWallDot.DeleteDot(false);
                _endWallDot = _raycastDot;

                _startWallDot = _endWallDot;
                _endWallDot = InstantiateWallDot(_cursorPosition);
                _lineObject = CreateLine(_cursorPosition);
                _startWallDot.SetPosition(_raycastDot.position);

                if (_startWallDot.GetComponent<WallDotController>().CheckForCycle())
                    print("Cycle detected");
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
    #endregion

    #region --- Utils ---
    private void WallSizeOnGUI()
    {   // Display a label of wall size (on meters)
        float _wallSize = _lineController.CalculateLength();

        if (_wallSize < 0.0001f)
            _sizeLabel.SetActive(false);
        else
        {
            Vector3 _labelPosition = (_endWallDot.position + _startWallDot.position) / 2;
            _labelPosition = _labelPosition + new Vector3(0, 0.5f, 0);

            _sizeLabel.transform.position = Camera.main.WorldToScreenPoint(_labelPosition);
            _sizeLabel.GetComponentInChildren<TextMeshProUGUI>().text = _wallSize.ToString("F2") + "m";
            _sizeLabel.SetActive(true);
        }
    }
    #endregion
}
