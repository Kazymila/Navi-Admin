using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

public class MapEditorCameraManager : MonoBehaviour
{
    [Header("Zoom Settings")]
    [SerializeField] private float _zoomChange = 15f;
    [SerializeField] private float _zoomSmooth = 5f;

    [SerializeField] private float _zoom2DMin = 5f;
    [SerializeField] private float _zoom2DMax = 20f;
    [SerializeField] private float _zoom3DMin = 30f;
    [SerializeField] private float _zoom3DMax = 100f;

    private Vector3 _PositionOrigin;
    private Vector3 _PositionDiff;
    private bool _isDragging;

    private Camera _camera;
    private InputMap _input;

    private void Start()
    {
        _camera = Camera.main;

        _input = new InputMap();
        _input.MapEditor.Enable();
        _input.MapEditor.MoveCamera.started += ctx => OnDrag(ctx);
        _input.MapEditor.MoveCamera.canceled += ctx => _isDragging = false;
    }

    void LateUpdate()
    {
        Zoom();
        if (!_isDragging) return;
        _PositionDiff = GetCursorPosition - transform.position;
        transform.position = _PositionOrigin - _PositionDiff;
    }

    private Vector3 GetCursorPosition => _camera.ScreenToWorldPoint(_input.MapEditor.Position.ReadValue<Vector2>());

    private void OnDrag(InputAction.CallbackContext ctx)
    {   // Drag the camera using the mouse
        if (ctx.started) _PositionOrigin = GetCursorPosition;
        _isDragging = ctx.started || ctx.performed;
    }

    public void SetOrthographicView()
    {   // Set the camera to orthographic view
        _camera.orthographic = true;
        _camera.orthographicSize = 10;

        _camera.transform.position = new Vector3(0, 0, -10);
        _camera.transform.rotation = Quaternion.Euler(0, 0, 0);
    }

    public void SetPerspectiveView()
    {   // Set the camera to perspective view
        _camera.orthographic = false;
        _camera.transform.position = new Vector3(0, 10, -10);
        _camera.transform.rotation = Quaternion.Euler(45, 0, 0);
    }

    public void ShowZoomSlider(Slider _zoomSlider)
    {   // Show the zoom slider
        if (_camera.orthographic)
        {
            _zoomSlider.value = _camera.orthographicSize;
            SetSliderText(_zoomSlider, _zoom2DMin, _zoom2DMax);
        }
        else
        {
            _zoomSlider.value = _camera.fieldOfView;
            SetSliderText(_zoomSlider, _zoom3DMin, _zoom3DMax);
        }
        _zoomSlider.gameObject.SetActive(!_zoomSlider.gameObject.activeSelf);
    }

    private void Zoom()
    {   // Zoom the camera using the mouse scroll
        float scroll = _input.MapEditor.Zoom.ReadValue<float>();
        if (_camera.orthographic)
        {
            if (scroll > 0) _camera.orthographicSize -= _zoomChange * Time.deltaTime * _zoomSmooth;
            if (scroll < 0) _camera.orthographicSize += _zoomChange * Time.deltaTime * _zoomSmooth;
            _camera.orthographicSize = Mathf.Clamp(_camera.orthographicSize, _zoom2DMin, _zoom2DMax);
        }
        else
        {
            if (scroll > 0) _camera.fieldOfView -= _zoomChange * Time.deltaTime * _zoomSmooth + 5;
            if (scroll < 0) _camera.fieldOfView += _zoomChange * Time.deltaTime * _zoomSmooth + 5;
            _camera.fieldOfView = Mathf.Clamp(_camera.fieldOfView, _zoom3DMin, _zoom3DMax);
        }
    }

    public void Zoom2DSlider(Slider _zoomSlider)
    {   // Zoom the camera using the slider for 2D view
        if (_camera.orthographicSize > _zoomSlider.value)
            _camera.orthographicSize -= _zoomChange * Time.deltaTime * _zoomSmooth;
        if (_camera.orthographicSize < _zoomSlider.value)
            _camera.orthographicSize += _zoomChange * Time.deltaTime * _zoomSmooth;

        _camera.orthographicSize = Mathf.Clamp(_camera.orthographicSize, _zoom2DMin, _zoom2DMax);
        SetSliderText(_zoomSlider, _zoom2DMin, _zoom2DMax);
    }

    public void Zoom3DSlider(Slider _zoomSlider)
    {   // Zoom the camera using the slider for 3D view
        if (_camera.fieldOfView > _zoomSlider.value)
            _camera.fieldOfView -= _zoomChange * Time.deltaTime * _zoomSmooth + 5;
        if (_camera.fieldOfView < _zoomSlider.value)
            _camera.fieldOfView += _zoomChange * Time.deltaTime * _zoomSmooth + 5;

        _camera.fieldOfView = Mathf.Clamp(_camera.fieldOfView, _zoom3DMin, _zoom3DMax);
        SetSliderText(_zoomSlider, _zoom3DMin, _zoom3DMax);
    }

    private void SetSliderText(Slider _zoomSlider, float _min, float _max)
    {   // Set the text of the zoom slider
        TMP_Text _sliderLabel = _zoomSlider.gameObject.transform.GetChild(1).GetComponent<TMP_Text>();
        _sliderLabel.text = Mathf.Round(((_zoomSlider.value - _min) / (_max - _min)) * 100).ToString() + "%";
    }

}
