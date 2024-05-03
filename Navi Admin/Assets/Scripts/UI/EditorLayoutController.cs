using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class EditorLayoutController : MonoBehaviour
{

    [Header("Required Stuff")]
    [SerializeField] private MapViewManager _mapViewManager;
    [SerializeField] private GameObject[] _featuresManagers;

    private MapEditorCanvasManager _canvasManager;
    private RectTransform[] _layoutRects;
    private Button[] _buttons;

    private GameObject _selectedFeature;
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

        for (int i = 0; i < _featuresManagers.Length; i++)
            _featuresManagers[i].SetActive(false);

        _animator.SetBool("Hide", false);
        this.gameObject.SetActive(false);
    }

    public bool IsCursorOverEditorUI(RectTransform[] _extraRects = null)
    {   // Check if the mouse is not in the UI, to avoid drawing over the UI
        if (_extraRects == null)
            return _canvasManager.IsCursorOverUICanvas(_layoutRects);
        else
        {
            List<RectTransform> _rects = new List<RectTransform>(_layoutRects);
            _rects.AddRange(_extraRects);
            return _canvasManager.IsCursorOverUICanvas(_rects.ToArray());
        }
    }

    public void OnEditorButtonSelected(Button _button)
    {   // Keep the button selected and disable the others
        _selectedButton = _button;
        _canvasManager.KeepButtonSelected(_selectedButton, _buttons);

        if (_button.name != "Hand") // Disable the hand tool when select other tool
            Camera.main.GetComponent<MapEditorCameraManager>().DisableHandTool();

        if (_button.name == "MapView") _button.interactable = true;
    }

    public void ActivateFeature(GameObject _feature)
    {   // Activate the selected feature and disable the others
        if (_mapViewManager.isMapViewActive) _mapViewManager.ShowMapView();

        _feature.SetActive(!_feature.activeSelf);
        _canvasManager.DisableOtherFeatures(_feature, _featuresManagers);
        _selectedFeature = _feature;
    }

    public void DisableSelectedButton()
    {   // Disable the selected button
        if (_selectedButton && _selectedFeature)
        {
            _selectedFeature.SetActive(false);
            _selectedButton.interactable = true;
            _selectedButton = null;
        }
    }
}
