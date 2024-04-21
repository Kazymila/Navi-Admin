using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntrancesTool : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private EditorLayoutController _UIEditorController;

    [Header("Entrances Settings")]
    [SerializeField] private GameObject _entrancePrefab;
    [SerializeField] private Transform _entranceParent;

    private EntrancesController _currentEntrance;
    private WallLineController _currentWall;
    private int _entrancesCount = 0;
    private bool _movingEntrance;

    private InputMap _input;

    private void OnEnable()
    {
        _input = new InputMap();
        _input.MapEditor.Enable();
        _input.MapEditor.Click.started += ctx => SetEntrance();
        _input.MapEditor.EndDraw.started += ctx => CancelDraw();
    }
    private void OnDisable()
    {
        _input.MapEditor.Disable();
        CancelDraw();
    }

    private Vector3 GetCursorPosition()
    {   // Get the cursor position in the world
        Vector3 _cursorPosition = Camera.main.ScreenToWorldPoint(_input.MapEditor.Position.ReadValue<Vector2>());
        _cursorPosition.z = 0;
        return _cursorPosition;
    }

    private void LateUpdate()
    {
        if (_UIEditorController.IsCursorOverEditorUI()) return;
        RaycastHit2D _hit = Physics2D.Raycast(GetCursorPosition(), Vector2.zero);

        if (!_movingEntrance)
        {   // If not moving an entrance, create a new one
            if (_hit.collider != null && _hit.collider.CompareTag("Wall"))
            {   // Create a new entrance on click over a wall
                _currentWall = _hit.collider.GetComponent<WallLineController>();
                InstantiateEntrance(_hit.point, _currentWall);
            }
        }
        else
        {
            if (_hit.collider != null)
            {   // Set the entrance position to a new wall
                if (_hit.collider.CompareTag("Wall") && _hit.collider.name != _currentWall.name)
                    _currentWall = _hit.collider.GetComponent<WallLineController>();
            }
            // Move the entrance to the cursor position
            if (_currentEntrance != null)
                _currentEntrance.SetEntrancePositionFromCursor(GetCursorPosition(), _currentWall);
        }
    }

    private void SetEntrance()
    {   // Set a entrance on click over a wall
        if (_currentEntrance == null) return;
        if (_UIEditorController.IsCursorOverEditorUI()) return;
        if (_currentEntrance.isOverEntrance) return;

        _currentWall.entrances.Add(_currentEntrance);
        _currentEntrance.PlaySettedAnimation();
        _currentEntrance.isSetted = true;
        _movingEntrance = false;
        _currentEntrance = null;
        _currentWall = null;
    }

    private void InstantiateEntrance(Vector3 _position, WallLineController _wall)
    {   // Instantiate a new entrance on the given position
        GameObject _newEntrance = Instantiate(_entrancePrefab, _position, Quaternion.identity, _entranceParent);
        _currentEntrance = _newEntrance.GetComponent<EntrancesController>();
        _currentEntrance.SetEntrancePositionFromCursor(_position, _wall);
        _currentEntrance.name = "Entrance_" + _entrancesCount;
        _movingEntrance = true;
        _entrancesCount++;
    }

    private void CancelDraw()
    {   // Cancel the current entrance draw
        if (_currentEntrance != null)
        {
            Destroy(_currentEntrance.gameObject);
            _currentEntrance = null;
            _currentWall = null;

            _movingEntrance = false;
        }
    }
}
