using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EraserTool : MonoBehaviour
{
    [SerializeField] private EditorLayoutController _UIEditorController;
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

        if (_hit.collider != null && !_UIEditorController.IsCursorOverEditorUI())
        {
            if (_hit.collider.CompareTag("WallDot"))
            {  // Delete the selected dot
                WallNodeController _selectedDot = _hit.collider.GetComponent<WallNodeController>();
                _selectedDot.DeleteNode();
            }
            else if (_hit.collider.CompareTag("Entrance"))
            {   // Delete the selected entrance
                EntrancesController _selectedEntrance = _hit.collider.GetComponent<EntrancesController>();
                _selectedEntrance.DestroyEntrance();
            }
            else if (_hit.collider.CompareTag("Wall"))
            {   // Delete the selected line
                WallLineController _selectedLine = _hit.collider.GetComponent<WallLineController>();
                _selectedLine.DestroyLine();
            }
            else if (_hit.collider.CompareTag("ShapeDot"))
            {   // Delete the selected shape
                ShapeController _selectedShape = _hit.collider.transform.parent.GetComponent<ShapeController>();
                _selectedShape.DestroyShape();
            }
            else if (_hit.collider.CompareTag("ShapeLine"))
            {   // Delete the selected shape
                ShapeController _selectedShape = _hit.collider.GetComponent<ShapeController>();
                _selectedShape.DestroyShape();
            }
            else if (_hit.collider.CompareTag("ShapeMesh"))
            {   // Delete the selected shape
                GameObject _selectedShape = GameObject.Find("Shape_" + _hit.collider.name.Split("_")[1]);
                _selectedShape.GetComponent<ShapeController>().DestroyShape();
            }
        }
    }

}
