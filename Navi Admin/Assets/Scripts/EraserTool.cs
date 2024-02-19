using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EraserTool : MonoBehaviour
{
    private InputMap _input;

    private void OnEnable()
    {
        _input = new InputMap();
        _input.MapEditor.Enable();
        _input.MapEditor.Click.started += ctx => EraseSelection();
    }
    private void OnDisable() => _input.MapEditor.Disable();

    private void EraseSelection()
    {   // Raycast to the object under the cursor to erase it
        Vector3 _cursorPosition = Camera.main.ScreenToWorldPoint(_input.MapEditor.Position.ReadValue<Vector2>());
        RaycastHit2D _hit = Physics2D.Raycast(_cursorPosition, Vector2.zero);

        if (_hit.collider != null)
        {
            if (_hit.collider.CompareTag("WallDot"))
            {  // Delete the selected dot
                WallDotController _selectedDot = _hit.collider.GetComponent<WallDotController>();
                _selectedDot.DeleteDot();
            }
            else if (_hit.collider.CompareTag("Wall"))
            {   // Delete the selected line
                WallLineController _selectedLine = _hit.collider.GetComponent<WallLineController>();
                _selectedLine.DeleteLine();
            }
        }
    }

}
