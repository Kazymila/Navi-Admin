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
    [SerializeField] private float _zoomMin = 5f;
    [SerializeField] private float _zoomMax = 20f;
    [SerializeField] private Slider _zoomSlider;

    private TMP_Text _sliderLabel;
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
        _sliderLabel = _zoomSlider.gameObject.transform.GetChild(1).GetComponent<TMP_Text>();
    }

    void LateUpdate()
    {
        Zoom();
        if (!_isDragging) return;
        _PositionDiff = GetCursorPosition - transform.position;
        transform.position = _PositionOrigin - _PositionDiff;
    }

    public void ShowZoomSlider()
    {
        _zoomSlider.value = _camera.orthographicSize;
        _sliderLabel.text = Mathf.Round(((_zoomSlider.value - 5) / 15) * 100).ToString() + "%";
        _zoomSlider.gameObject.SetActive(!_zoomSlider.gameObject.activeSelf);
    }

    public void OnZoom()
    {
        if (_camera.orthographicSize > _zoomSlider.value)
            _camera.orthographicSize -= _zoomChange * Time.deltaTime * _zoomSmooth;
        if (_camera.orthographicSize < _zoomSlider.value)
            _camera.orthographicSize += _zoomChange * Time.deltaTime * _zoomSmooth;

        _camera.orthographicSize = Mathf.Clamp(_camera.orthographicSize, _zoomMin, _zoomMax);
        _sliderLabel.text = Mathf.Round(((_zoomSlider.value - 5) / 15) * 100).ToString() + "%";
    }

    private void Zoom()
    {
        float scroll = _input.MapEditor.Zoom.ReadValue<float>();
        if (scroll > 0) _camera.orthographicSize -= _zoomChange * Time.deltaTime * _zoomSmooth;
        if (scroll < 0) _camera.orthographicSize += _zoomChange * Time.deltaTime * _zoomSmooth;
        _camera.orthographicSize = Mathf.Clamp(_camera.orthographicSize, _zoomMin, _zoomMax);
    }

    private Vector3 GetCursorPosition => _camera.ScreenToWorldPoint(_input.MapEditor.Position.ReadValue<Vector2>());

    private void OnDrag(InputAction.CallbackContext ctx)
    {
        if (ctx.started) _PositionOrigin = GetCursorPosition;
        _isDragging = ctx.started || ctx.performed;
    }
}
