using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapEditorUIController : MonoBehaviour
{
    [Header("Button Colors")]
    [SerializeField] private Color _normalColor;
    [SerializeField] private Color _highlightedColor;
    [SerializeField] private Color _pressedColor;
    [SerializeField] private Color _selectedColor;
    [SerializeField] private Color _disabledColor;

    [Header("Required Stuff")]
    [SerializeField] private GameObject[] _toolManagers;

    private Button[] _buttons;
    private Animator _animator;
    private Button _selectedButton;

    void Start()
    {
        _buttons = GetComponentsInChildren<Button>();
        _animator = GetComponent<Animator>();
    }

    private void ChangeButtonsColors()
    {

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

    public void HideEditorInterface()
    {
        _animator.SetBool("Hide", true);
        Invoke("DisableLayout", 0.2f);
    }

    private void DisableLayout()
    {
        if (_selectedButton)
        {
            _selectedButton.interactable = true;
            _selectedButton = null;
        }

        for (int i = 0; i < _toolManagers.Length; i++)
            _toolManagers[i].SetActive(false);

        _animator.SetBool("Hide", false);
        this.gameObject.SetActive(false);
    }

    public void SetAllButtonsInteractable(string _selectedButtonName)
    {
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

    public void OnButtonSelected(Button _button)
    {
        /* Keep the button as selected after clicking on it

        - To keep the selected color of a button, 
          we use the disable color as the selected color.
        */
        _selectedButton = _button;
        int _buttonIndex = System.Array.IndexOf(_buttons, _selectedButton);

        if (_buttonIndex == -1) return;

        SetAllButtonsInteractable(_selectedButton.name);
        _selectedButton.interactable = false;

        if (_button.name != "Hand") // Disable the hand tool when select other tool
            Camera.main.GetComponent<MapEditorCameraManager>().DisableHandTool();
    }
}
