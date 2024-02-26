using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class SliderController : MonoBehaviour
{
    private MapEditorCanvasManager _canvasManager;
    private TMP_Text _sliderLabel;
    private Slider _slider;

    private RectTransform[] _rect;

    private void InitializeSlider()
    {
        _canvasManager = GameObject.Find("Canvas").GetComponent<MapEditorCanvasManager>();
        _sliderLabel = this.transform.GetComponentInChildren<TMP_Text>();
        _slider = this.gameObject.GetComponent<Slider>();

        _rect = new RectTransform[2];
        _rect[0] = this.transform.GetChild(0).GetComponent<RectTransform>();
        _rect[1] = this.transform.parent.GetComponent<RectTransform>();
    }

    private void Update()
    {
        HideZoomSlider();
    }

    private void HideZoomSlider()
    {   // Hide the zoom slider when the cursor is not over it
        bool _isOverSlider = RectTransformUtility.RectangleContainsScreenPoint(
            _rect[0], _canvasManager.GetCursorPosition(), null);

        bool _isOverParent = RectTransformUtility.RectangleContainsScreenPoint(
            _rect[1], _canvasManager.GetCursorPosition(), null);

        if (!_isOverSlider && !_isOverParent)
        {
            this.gameObject.SetActive(false);
        }
    }

    public void ShowPercentage(float _min, float _max)
    {   // Show the percentage of the slider
        if (_slider == null) InitializeSlider();
        _sliderLabel.text = (100 - Mathf.Round(((_slider.value - _min) / (_max - _min)) * 100)).ToString() + "%";
    }
}
