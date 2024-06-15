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
    [SerializeField] private GameObject _nodePrefab;
    [SerializeField] private Transform _nodesParent;

    [Header("Line settings")]
    [SerializeField] private GameObject _linePrefab;
    [SerializeField] private Transform _linesParent;
    [SerializeField] private float _wallWidth = 0.15f;
    #endregion
    private GameObject _lineObject;
    private WallLineController _lineController;
    private WallNodeController _startWallNode;
    private WallNodeController _endWallNode;
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
            _endWallNode.SetPosition(GetCursorPosition());
            _endWallNode.dotCollider.enabled = false;
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
        WallNodeController _raycastDot = RaycastToDot();
        Vector3 _cursorPosition = GetCursorPosition(true);
        if (_endWallNode) _endWallNode.dotCollider.enabled = true;

        // If the cursor is over a dot, wall is created or finished here
        if (_raycastDot) OnSelectDot(_raycastDot);

        else if (_lineObject == null && !_isDrawing)
        {   // Create the first dot and line
            _startWallNode = InstantiateWallDot(_cursorPosition);
            _endWallNode = InstantiateWallDot(_cursorPosition);
            _lineObject = CreateLine(_cursorPosition);
            _isDrawing = true;
            _lineObject.GetComponent<WallLineController>().isDrawing = true;
        }
        else if (_lineObject != null && _isDrawing)
        {   // Set dot and add line from this last dot
            _endWallNode.SetPosition(GetCursorPosition());

            _startWallNode = _endWallNode;
            _endWallNode = InstantiateWallDot(_cursorPosition);
            _lineObject = CreateLine(_cursorPosition);
        }
    }

    private GameObject CreateLine(Vector3 _position)
    {   // Create a new line renderer and set the start and end dots
        GameObject _newLine = Instantiate(_linePrefab, _position, Quaternion.identity, _linesParent);
        _newLine.name = "Wall_" + _newLine.transform.GetSiblingIndex();
        _lineController = _newLine.GetComponent<WallLineController>();
        _lineController.SetLineRenderer(_position, _position, _wallWidth);
        SetLineDots(_newLine, _startWallNode, _endWallNode);
        return _newLine;
    }

    private void SetLineDots(GameObject _line, WallNodeController _startDot, WallNodeController _endDot)
    {   // Set the dots of a line and add the line to the dots
        _lineController.startNode = _startDot;
        _lineController.endNode = _endDot;
        _startDot.AddLine(_line, 0, _endDot);
        _endDot.AddLine(_line, 1, _startDot);
    }
    private WallNodeController InstantiateWallDot(Vector3 _position)
    {   // Instantiate a wall dot and atach it to a line
        GameObject _wallDot = Instantiate(_nodePrefab, _position, Quaternion.identity, _nodesParent);
        return _wallDot.GetComponent<WallNodeController>();
    }

    private void CancelDraw()
    {   // Cancel the current line drawing
        if (_endWallNode != null)
        {
            _startWallNode.DeleteLine(_startWallNode.walls.IndexOf(_lineObject));
            _endWallNode.DeleteNode(true);
        }
        _lineObject.GetComponent<WallLineController>().isDrawing = true;
        _isDrawing = false;
        _endWallNode = null;
        _lineObject = null;
    }
    #endregion

    #region --- Dots Raycasting ---
    private void OnSelectDot(WallNodeController _raycastDot)
    {
        if (!_isDrawing)
        {   // Create a line from the selected dot
            _startWallNode = _raycastDot;
            _raycastDot.PlaySelectAnimation();
            _endWallNode = InstantiateWallDot(_raycastDot.GetNodePosition());
            _lineObject = CreateLine(_raycastDot.GetNodePosition());
            _isDrawing = true;
            _lineObject.GetComponent<WallLineController>().isDrawing = true;
        }
        else
        {   // If was already drawing, the selected dot is setted as the end dot
            if (_raycastDot.FindNeighborNode(_startWallNode))
            {   // Iif the dots are already connected, cannot set the dot here
                _errorMessageBox.ShowTimedMessage("DotsAlreadyConnected", 2);
                _raycastDot.PlayDeniedAnimation();
            }
            else
            {   // End the line and add a new line from this dot
                _raycastDot.PlaySelectAnimation();
                Vector3 _cursorPosition = GetCursorPosition(true);

                _endWallNode.SetPosition(_raycastDot.GetNodePosition());
                SetLineDots(_lineObject, _startWallNode, _raycastDot);
                _endWallNode.DeleteNode(false);
                _endWallNode = _raycastDot;

                _startWallNode = _endWallNode;
                _endWallNode = InstantiateWallDot(_cursorPosition);
                _lineObject = CreateLine(_cursorPosition);
                _startWallNode.SetPosition(_raycastDot.GetNodePosition());
            }
        }
    }

    private WallNodeController RaycastToDot()
    {   // Raycast to the dots to check if the mouse is over one of them
        RaycastHit2D _hit = Physics2D.Raycast(GetCursorPosition(), Vector2.zero);

        if (_hit.collider != null && _hit.collider.CompareTag("WallDot"))
        {
            WallNodeController _dot = _hit.collider.GetComponent<WallNodeController>();
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
            Vector3 _labelPosition = (_endWallNode.GetNodePosition() + _startWallNode.GetNodePosition()) / 2;
            _labelPosition = _labelPosition + new Vector3(0, 0.5f, 0);

            _sizeLabel.transform.position = Camera.main.WorldToScreenPoint(_labelPosition);
            _sizeLabel.GetComponentInChildren<TextMeshProUGUI>().text = _wallSize.ToString("F2") + "m";
            _sizeLabel.SetActive(true);
        }
    }
    #endregion
}
