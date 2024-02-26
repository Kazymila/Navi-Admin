using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class EditorLayoutController : MonoBehaviour
{

    [Header("Required Stuff")]
    [SerializeField] private GameObject[] _toolManagers;

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

    public void HideEditorInterface()
    {   // Hide the editor interface (when show the 3D view)
        _animator.SetBool("Hide", true);
        Invoke("DisableLayout", 0.2f);
    }

    private void DisableLayout()
    {   // Disable the layout when hide it
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

    public bool IsCursorOverEditorUI()
    {   // Check if the mouse is not in the UI, to avoid drawing over the UI
        return _canvasManager.IsCursorOverUICanvas(_layoutRects);
    }

    public void OnEditorButtonSelected(Button _button)
    {   // Keep the button selected and disable the others
        _selectedButton = _button;
        _canvasManager.KeepButtonSelected(_selectedButton, _buttons);

        if (_button.name != "Hand") // Disable the hand tool when select other tool
            Camera.main.GetComponent<MapEditorCameraManager>().DisableHandTool();
    }
}
