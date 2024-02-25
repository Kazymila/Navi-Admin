using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EditorLayoutController : MonoBehaviour
{

    [Header("Required Stuff")]
    [SerializeField] private MapEditorCanvasManager _canvasManager;
    [SerializeField] private GameObject[] _toolManagers;

    public Button[] buttons;
    public RectTransform[] layoutRects;

    private Button _selectedButton;
    private Animator _animator;

    void Start()
    {
        buttons = GetComponentsInChildren<Button>();
        _animator = GetComponent<Animator>();

        GetLayoutRects();
    }

    private void GetLayoutRects()
    {   // Get the rect transforms (bounding boxes) of the layout
        List<RectTransform> _rects = new List<RectTransform>();

        for (int i = 0; i < this.transform.childCount; i++)
        {
            Transform _widget = this.transform.GetChild(i);
            _rects.Add(_widget.GetComponent<RectTransform>());

            for (int j = 0; j < _widget.childCount; j++)
            {   // Add the rect transform of the child widgets (like sliders)
                Transform _option = _widget.GetChild(j);
                if (_option.childCount > 1 && _option.GetComponent<Button>() != null)
                    _rects.Add(_option.GetChild(1).GetComponent<RectTransform>());
            }
        }
        layoutRects = _rects.ToArray();
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
        return _canvasManager.IsCursorOverUICanvas(layoutRects);
    }

    public void OnEditorButtonSelected(Button _button)
    {   // Keep the button selected and disable the others
        _selectedButton = _button;
        _canvasManager.KeepButtonSelected(_selectedButton, buttons);

        if (_button.name != "Hand") // Disable the hand tool when select other tool
            Camera.main.GetComponent<MapEditorCameraManager>().DisableHandTool();
    }
}
