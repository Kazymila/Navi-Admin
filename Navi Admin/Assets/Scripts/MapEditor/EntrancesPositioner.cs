using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntrancesPositioner : MonoBehaviour
{
    [Header("Entrances Settings")]
    [SerializeField] private GameObject _entrancePrefab;
    [SerializeField] private Transform _entranceParent;
    private WallLineController _currentWall;
    private GameObject _currentEntrance;
    private int _entrancesCount = 0;

    private InputMap _input;

    private void OnEnable()
    {
        _input = new InputMap();
        _input.MapEditor.Enable();
        _input.MapEditor.Click.started += ctx => NewEntrance();
    }
    private void OnDisable() => _input.MapEditor.Disable();

    private Vector3 GetCursorPosition()
    {   // Get the cursor position in the world
        Vector3 _cursorPosition = Camera.main.ScreenToWorldPoint(_input.MapEditor.Position.ReadValue<Vector2>());
        _cursorPosition.z = 0;
        return _cursorPosition;
    }

    private void NewEntrance()
    {   // Create a new entrance snap to a existing wall
        RaycastHit2D _hit = Physics2D.Raycast(GetCursorPosition(), Vector2.zero);
        if (_hit.collider != null)
        {
            if (_hit.collider.CompareTag("Wall"))
            {
                _currentWall = _hit.collider.GetComponent<WallLineController>();
                GameObject _newEntrance = Instantiate(_entrancePrefab, GetCursorPosition(), Quaternion.identity, _entranceParent);
                _newEntrance.GetComponent<EntrancesController>().SetEntrancePosition(GetCursorPosition(), _currentWall);
                _newEntrance.name = "Entrance_" + _entrancesCount;
                _currentEntrance = _newEntrance;
                _entrancesCount++;
            }
        }
    }
}
