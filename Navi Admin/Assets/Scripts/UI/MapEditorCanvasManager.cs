using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class MapEditorCanvasManager : MonoBehaviour
{
    [Header("Button Colors")]
    [SerializeField] private Color _normalColor;
    [SerializeField] private Color _highlightedColor;
    [SerializeField] private Color _pressedColor;
    [SerializeField] private Color _selectedColor;
    [SerializeField] private Color _disabledColor;

    private InputMap _input;

    void Start()
    {
        _input = new InputMap();
        _input.MapEditor.Enable();
    }

    public Vector2 GetCursorPosition()
    {   // Get the cursor position in the canvas
        if (Camera.main.orthographic) return _input.MapEditor.Position.ReadValue<Vector2>();
        else return _input.RenderView.Position.ReadValue<Vector2>();
    }

    #region --- Button Managment ---
    private void ChangeButtonsColors(Button[] _buttons)
    {   // Change the colors of the buttons
        ColorBlock _buttonColors = _buttons[0].colors;
        _buttonColors.normalColor = _normalColor;
        _buttonColors.highlightedColor = _highlightedColor;
        _buttonColors.pressedColor = _pressedColor;
        _buttonColors.selectedColor = _selectedColor;
        _buttonColors.disabledColor = _disabledColor;

        foreach (Button _button in _buttons)
        {
            _button.colors = _buttonColors;
        }
    }

    public void SetAllButtonsInteractable(string _selectedButtonName, Button[] _buttons)
    {   // Set all buttons interactable and hide child widgets (reset buttons status)
        foreach (Button _button in _buttons)
        {
            _button.interactable = true;
            if (_button.name != _selectedButtonName)
            {   // If button have a child (like slider), disable it when press other button
                if (_button.gameObject.transform.childCount > 1)
                    _button.gameObject.transform.GetChild(1).gameObject.SetActive(false);
            }
        }
    }

    public void KeepButtonSelected(Button _selectedButton, Button[] _buttons)
    {
        /* Keep the button as selected after clicking on it

        - To keep the selected color of a button, 
          we use the DISABLE COLOR as the selected color.
        */
        int _buttonIndex = System.Array.IndexOf(_buttons, _selectedButton);
        if (_buttonIndex == -1) return;

        SetAllButtonsInteractable(_selectedButton.name, _buttons);
        _selectedButton.interactable = false;
    }
    #endregion

    #region --- UI Managment ---
    public bool IsCursorOverUICanvas(RectTransform[] _rects)
    {   // Check if the cursor is over the UI, to avoid conflicts with the drawing canvas
        foreach (RectTransform _rect in _rects)
        {
            bool _isOverUI = RectTransformUtility.RectangleContainsScreenPoint(
                _rect, GetCursorPosition(), null);

            if (_isOverUI && _rect.gameObject.activeSelf) return true;
        }
        return false;
    }

    public RectTransform[] GetLayoutRects(Transform _layout)
    {   // Get the rect transforms (bounding boxes) of the layout
        List<RectTransform> _rects = new List<RectTransform>();

        for (int i = 0; i < _layout.childCount; i++)
        {
            Transform _widget = _layout.GetChild(i);
            _rects.Add(_widget.GetComponent<RectTransform>());

            for (int j = 0; j < _widget.childCount; j++)
            {   // Add the rect transform of the child widgets (like sliders)
                Transform _option = _widget.GetChild(j);
                if (_option.childCount > 1 && _option.GetComponent<Button>() != null)
                    _rects.Add(_option.GetChild(1).GetComponent<RectTransform>());
            }
        }
        return _rects.ToArray();
    }
    #endregion
}
