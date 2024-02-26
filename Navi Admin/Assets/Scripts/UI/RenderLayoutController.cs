using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class RenderLayoutController : MonoBehaviour
{
    private MapEditorCanvasManager _canvasManager;
    private RectTransform[] _layoutRects;
    private Button[] _buttons;

    private Button _selectedButton;
    private Animator _animator;

    void Start()
    {
        _canvasManager = this.transform.parent.GetComponent<MapEditorCanvasManager>();
        _buttons = GetComponentsInChildren<Button>();
        _animator = GetComponent<Animator>();

        _layoutRects = _canvasManager.GetLayoutRects(this.transform);
    }

    public bool IsCursorOverRenderUI()
    {   // Check if the mouse is not in the UI
        return _canvasManager.IsCursorOverUICanvas(_layoutRects);
    }

    public void OnRenderButtonSelected(Button _button)
    {   // Keep the button selected and disable the others
        _selectedButton = _button;
        _canvasManager.KeepButtonSelected(_selectedButton, _buttons);

        if (_button.name != "Hand") // Disable the hand tool when select other tool
            Camera.main.GetComponent<MapEditorCameraManager>().DisableHandTool();
    }
}
