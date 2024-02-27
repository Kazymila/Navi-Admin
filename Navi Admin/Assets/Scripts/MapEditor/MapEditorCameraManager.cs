using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MapEditorCameraManager : MonoBehaviour
{
    #region --- External Variables ---
    [Header("Required Stuff")]
    [SerializeField] private EditorLayoutController _UIEditorController;
    [SerializeField] private RenderLayoutController _UIRenderController;
    [SerializeField] private GameObject _gridPlane;
    #endregion

    #region --- Zoom Variables ---
    [Header("Zoom Settings")]
    [SerializeField] private float _zoomChange = 10f;
    [SerializeField] private float _zoomSmooth = 3f;

    private float _zoom2DMin = 3f;
    private float _zoom2DMax = 15f;
    private float _zoom3DMin = 30f;
    private float _zoom3DMax = 100f;

    private float _hideGridThreshold = 10f;
    #endregion

    #region --- Movement Variables ---
    [Header("Move Settings")]
    [SerializeField] private float _moveSpeed = 5f;
    private Vector3 _PositionOrigin;
    private bool _isDragging2D;
    private bool _isDragging3D;
    #endregion

    #region --- Rotation Variables ---
    [Header("Rotation Settings")]
    [SerializeField] private float _rotationSpeed = 0.5f;
    private Vector3 _RotationOrigin;
    private bool _isRotating;
    #endregion

    private InputMap _input;
    private Camera _cam;

    private void Start()
    {
        _cam = this.gameObject.GetComponent<Camera>();
        _input = new InputMap();
        EnableEditorInput();
    }

    #region --- Input Managment ---
    private void EnableEditorInput()
    {
        _input.MapEditor.Enable();
        _input.MapEditor.Zoom.performed += ctx => Zoom2D();
        _input.MapEditor.Move.started += ctx => MoveCamera2D(ctx);
        _input.MapEditor.Move.canceled += ctx => _isDragging2D = false;
    }
    private void EnableRenderInput()
    {
        _input.RenderView.Enable();
        _input.RenderView.Zoom.performed += ctx => Zoom3D();
        _input.RenderView.Move.started += ctx => MoveCamera3D(ctx);
        _input.RenderView.Move.canceled += ctx => _isDragging3D = false;
        _input.RenderView.Rotate.started += ctx => RotateCamera(ctx);
        _input.RenderView.Rotate.canceled += ctx => _isRotating = false;
    }
    private void DisableEditorInput() => _input.MapEditor.Disable();
    private void DisableRenderInput() => _input.RenderView.Disable();
    private Vector3 GetLookDelta => _input.RenderView.Look.ReadValue<Vector2>();
    private Vector3 GetCursorPosition => _cam.ScreenToWorldPoint(_input.MapEditor.Position.ReadValue<Vector2>());
    private Vector3 GetViewportPosition => _cam.ScreenToViewportPoint(_input.RenderView.Position.ReadValue<Vector2>());
    #endregion

    #region --- Set Camera View ---
    public void SetOrthographicView()
    {   // Set the camera to orthographic view
        EnableEditorInput();
        DisableRenderInput();
        _cam.orthographic = true;
        _cam.orthographicSize = 6;

        _cam.transform.position = new Vector3(0, 0, -10);
        _cam.transform.rotation = Quaternion.Euler(0, 0, 0);
    }
    public void SetPerspectiveView()
    {   // Set the camera to perspective view
        EnableRenderInput();
        DisableEditorInput();
        _cam.orthographic = false;
        _cam.transform.position = new Vector3(0, 10, -10);
        _cam.transform.rotation = Quaternion.Euler(45, 0, 0);
    }
    #endregion

    #region --- Camera Zoom ---
    public void ShowZoomSlider(Slider _zoomSlider)
    {   // Show the zoom slider
        if (_cam.orthographic)
        {
            _zoomSlider.value = _cam.orthographicSize;
            _zoomSlider.GetComponent<SliderController>().ShowPercentage(_zoom2DMin, _zoom2DMax);
        }
        else
        {
            _zoomSlider.value = _cam.fieldOfView;
            _zoomSlider.GetComponent<SliderController>().ShowPercentage(_zoom3DMin, _zoom3DMax);
        }
        _zoomSlider.gameObject.SetActive(!_zoomSlider.gameObject.activeSelf);
    }

    private void Zoom2D()
    {   // Zoom the camera for 2D view
        float _scroll = _input.MapEditor.Zoom.ReadValue<float>();
        if (_scroll > 0) _cam.orthographicSize -= _zoomChange * Time.deltaTime * _zoomSmooth;
        if (_scroll < 0) _cam.orthographicSize += _zoomChange * Time.deltaTime * _zoomSmooth;
        _cam.orthographicSize = Mathf.Clamp(_cam.orthographicSize, _zoom2DMin, _zoom2DMax);

        // Hide the grid when zoom out
        if (_cam.orthographicSize > _hideGridThreshold) _gridPlane.SetActive(false);
        else _gridPlane.SetActive(true);
    }
    private void Zoom3D()
    {   // Zoom the camera for 3D view
        float _scroll = _input.RenderView.Zoom.ReadValue<float>();
        if (_scroll > 0) _cam.fieldOfView -= _zoomChange * Time.deltaTime * _zoomSmooth + 5;
        if (_scroll < 0) _cam.fieldOfView += _zoomChange * Time.deltaTime * _zoomSmooth + 5;
        _cam.fieldOfView = Mathf.Clamp(_cam.fieldOfView, _zoom3DMin, _zoom3DMax);
    }

    public void Zoom2DSlider(Slider _zoomSlider)
    {   // Zoom the camera using the slider for 2D view
        if (_cam.orthographicSize > _zoomSlider.value)
            _cam.orthographicSize -= _zoomChange * Time.deltaTime * _zoomSmooth;
        if (_cam.orthographicSize < _zoomSlider.value)
            _cam.orthographicSize += _zoomChange * Time.deltaTime * _zoomSmooth;

        _cam.orthographicSize = Mathf.Clamp(_cam.orthographicSize, _zoom2DMin, _zoom2DMax);
        _zoomSlider.GetComponent<SliderController>().ShowPercentage(_zoom2DMin, _zoom2DMax);

        // Hide the grid when zoom out
        if (_cam.orthographicSize > _hideGridThreshold) _gridPlane.SetActive(false);
        else _gridPlane.SetActive(true);
    }
    public void Zoom3DSlider(Slider _zoomSlider)
    {   // Zoom the camera using the slider for 3D view
        if (_cam.fieldOfView > _zoomSlider.value)
            _cam.fieldOfView -= _zoomChange * Time.deltaTime * _zoomSmooth + 5;
        if (_cam.fieldOfView < _zoomSlider.value)
            _cam.fieldOfView += _zoomChange * Time.deltaTime * _zoomSmooth + 5;

        _cam.fieldOfView = Mathf.Clamp(_cam.fieldOfView, _zoom3DMin, _zoom3DMax);
        _zoomSlider.GetComponent<SliderController>().ShowPercentage(_zoom3DMin, _zoom3DMax);
    }
    #endregion

    #region --- Camera Movement ---
    private void LateUpdate()
    {
        if (_isDragging2D) // Move the camera for 2D view
        {
            Vector3 _PositionDiff = GetCursorPosition - _cam.transform.position;
            _cam.transform.position = _PositionOrigin - _PositionDiff;
        }
        if (_isDragging3D) // Move the camera for 3D view
        {
            Vector3 _PositionDiff = _cam.transform.right * (GetLookDelta.x * -_moveSpeed);
            _PositionDiff += _cam.transform.up * (GetLookDelta.y * -_moveSpeed);
            _cam.transform.position += _PositionDiff * Time.deltaTime;
        }
        if (_isRotating) // Rotate the camera for 3D view
        {
            Vector3 _RotationDiff = _RotationOrigin - GetViewportPosition;
            _cam.transform.RotateAround(Vector3.zero, _cam.transform.right, _RotationDiff.y * 180 * _rotationSpeed * Time.deltaTime);
            _cam.transform.RotateAround(Vector3.zero, Vector3.up, -_RotationDiff.x * 180 * _rotationSpeed * Time.deltaTime);

            _RotationOrigin = GetViewportPosition;
        }
    }

    private void MoveCamera2D(InputAction.CallbackContext ctx)
    {   // Move the camera for 2D view
        if (_UIEditorController.IsCursorOverEditorUI()) return;
        if (ctx.started) _PositionOrigin = GetCursorPosition;
        _isDragging2D = ctx.started || ctx.performed;
    }

    private void MoveCamera3D(InputAction.CallbackContext ctx)
    {   // Move the camera for 3D view
        if (_UIRenderController.IsCursorOverRenderUI()) return;
        _isDragging3D = ctx.started || ctx.performed;
    }

    public void DragCamera()
    {   // Drag the camera for 3D view with hand tool
        if (_cam.orthographic)
        {
            _input.MapEditor.Hand.Enable();
            _input.MapEditor.Hand.started += ctx => MoveCamera2D(ctx);
            _input.MapEditor.Hand.canceled += ctx => _isDragging2D = false;
        }
        else
        {
            _input.RenderView.Hand.Enable();
            _input.RenderView.Hand.started += ctx => MoveCamera3D(ctx);
            _input.RenderView.Hand.canceled += ctx => _isDragging3D = false;
        }
    }

    public void DisableHandTool()
    {   // Disable the hand tool
        if (_cam.orthographic) _input.MapEditor.Hand.Disable();
        else _input.RenderView.Hand.Disable();
    }

    private void RotateCamera(InputAction.CallbackContext ctx)
    {   // Rotate the camera for 3D view
        _RotationOrigin = GetViewportPosition;
        _isRotating = ctx.started || ctx.performed;
    }
    #endregion
}
