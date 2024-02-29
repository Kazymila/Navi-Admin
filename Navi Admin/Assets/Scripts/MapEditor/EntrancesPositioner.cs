using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntrancesPositioner : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private ErrorMessageController _errorMessageBox;

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
    private void OnDisable() => _input.MapEditor.Disable();

    private Vector3 GetCursorPosition()
    {   // Get the cursor position in the world
        Vector3 _cursorPosition = Camera.main.ScreenToWorldPoint(_input.MapEditor.Position.ReadValue<Vector2>());
        _cursorPosition.z = 0;
        return _cursorPosition;
    }

    private void Update()
    {
        if (_movingEntrance)
        {
            _currentEntrance.SetEntrancePosition(GetCursorPosition(), _currentWall);
        }
    }

    private void SetEntrance()
    {   // Set or create a new entrance on click
        if (_movingEntrance)
        {   // Set the entrance position
            _currentEntrance.PlaySettedAnimation();
            _currentEntrance.SetLineCollider();
            _movingEntrance = false;
            _currentEntrance = null;
            _currentWall = null;
        }
        else NewEntrance();
    }

    private void NewEntrance()
    {   // Create a new entrance snap to a existing wall
        RaycastHit2D _hit = Physics2D.Raycast(GetCursorPosition(), Vector2.zero);
        if (_hit.collider != null)
        {
            if (_hit.collider.CompareTag("Entrance"))
            {
                // TODO: Error that cannot set an entrance over a existing one
                _errorMessageBox.ShowTimedMessage("CannotSetOverExistingEntrance", 2f);
                //_currentEntrance.PlayDeniedAnimation();
            }
            else if (_hit.collider.CompareTag("Wall"))
            {   // Create a new entrance
                _currentWall = _hit.collider.GetComponent<WallLineController>();
                GameObject _newEntrance = Instantiate(_entrancePrefab, GetCursorPosition(), Quaternion.identity, _entranceParent);
                _currentEntrance = _newEntrance.GetComponent<EntrancesController>();
                _currentEntrance.SetEntrancePosition(GetCursorPosition(), _currentWall);
                _currentEntrance.name = "Entrance_" + _entrancesCount;
                _entrancesCount++;
                _movingEntrance = true;
            }
        }
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
