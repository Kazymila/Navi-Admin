using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SFB;

public class QRCodesManager : MonoBehaviour
{
    #region --- External Variables ---
    [Header("UI Components")]
    [SerializeField] private EditorLayoutController _UIEditorController;
    [SerializeField] private RenderLayoutController _UIRenderController;
    [SerializeField] private ErrorMessageController _errorMessageBox;
    [SerializeField] private MapViewManager _mapViewManager;
    private NavMeshManager _navMeshManager;

    [Header("QR Code Settings")]
    [SerializeField] private GameObject _QRCodePrefab;
    [SerializeField] private GameObject _QRCodeSettingsPanel;
    [SerializeField] private TMP_InputField _QRCodeLabelInput;
    [SerializeField] private TMP_InputField _QRCodeXPosInput;
    [SerializeField] private TMP_InputField _QRCodeYPosInput;
    [SerializeField] private RawImage _QRCodeDisplayImage;
    #endregion

    #region --- Private Variables ---
    private RectTransform[] _QRCodePanelRect;
    private QRCodeController _currentQRCode;

    private Quaternion _initialQRCodeRotation;
    private Vector3 _initialQRCodePosition;
    private string _initialQRCodeLabel = null;

    private float _markerHeight = 0.6f;
    private bool _isRotating;
    private bool _isMoving;

    private InputMap _input;
    #endregion

    private void OnEnable()
    {
        _input = new InputMap();
        if (Camera.main.orthographic)
        {   // Enable the input map for the map editor
            _input.MapEditor.Enable();
            _input.MapEditor.Click.started += ctx => OnClick();

            foreach (Transform _child in this.transform)
            {   // Rotate the QR codes to face the camera in the map editor
                if (_child.position.z != 0 && _child.position.y == _markerHeight)
                {
                    _child.position = new Vector3(_child.position.x, _child.position.z, 0);
                    _child.rotation = Quaternion.Euler(_child.rotation.eulerAngles.y, -90, _child.rotation.eulerAngles.z);
                }
            }
            _mapViewManager.ShowMapView(false);
        }
        else
        {   // Enable the input map for the 3D render view
            _input.RenderView.Enable();
            _input.RenderView.Click.started += ctx => OnClick();

            foreach (Transform _child in this.transform)
            {   // Rotate the QR codes to face the camera in the 3D view
                if (_child.position.y != _markerHeight && _child.position.z == 0)
                {
                    _child.position = new Vector3(_child.position.x, _markerHeight, _child.position.y);
                    _child.rotation = Quaternion.Euler(-90, _child.rotation.eulerAngles.x, _child.rotation.eulerAngles.z);
                }
            }
        }
    }
    private void OnDisable()
    {
        _input.MapEditor.Disable();
        _input.RenderView.Disable();
        _QRCodeSettingsPanel.SetActive(false);
        if (_mapViewManager.isMapViewActive) _mapViewManager.ShowMapView();
    }

    void Start()
    {
        _QRCodePanelRect = new RectTransform[] { _QRCodeSettingsPanel.GetComponent<RectTransform>() };
        _navMeshManager = GameObject.Find("NavMeshManager").GetComponent<NavMeshManager>();
    }

    private Vector3 GetCursorPosition()
    {   // Get the cursor position in the world
        if (Camera.main.orthographic)
        {   // Get the cursor position in the map editor (2D view)
            Vector3 _cursorPosition = Camera.main.ScreenToWorldPoint(_input.MapEditor.Position.ReadValue<Vector2>());
            _cursorPosition.z = 0;
            return _cursorPosition;
        }
        else
        {   // Get the cursor position in the render view (3D view)
            Ray _ray = Camera.main.ScreenPointToRay(_input.RenderView.Position.ReadValue<Vector2>());
            if (Physics.Raycast(_ray, out RaycastHit _hit, Mathf.Infinity, LayerMask.GetMask("Polygon")))
            {
                Vector3 _position = _hit.point;
                _position.y = _markerHeight;
                return _position;
            }
            else return new Vector3(9999, 9999, 9999);
        }
    }

    private void Update()
    {
        if (_isRotating)
        {   // Rotate the QR code to the cursor position
            _currentQRCode.transform.LookAt(GetCursorPosition(), Vector3.left);
        }
        if (_isMoving)
        {   // Move the QR code to the cursor position
            _currentQRCode.transform.position = GetCursorPosition();
            UpdatePositionInputFields();
        }
    }

    private void OnClick()
    {   // Select and edit a QR code or create a new one
        if (_UIEditorController.IsCursorOverEditorUI(_QRCodePanelRect)) return;
        if (!Camera.main.orthographic && _UIRenderController.IsCursorOverRenderUI(_QRCodePanelRect)) return;

        if (_isRotating) _currentQRCode.CalculateQRCodeDirection();

        if (_isRotating || _isMoving)
        {   // If the QR code is rotating, stop rotating it
            _isRotating = false;
            _isMoving = false;
            _QRCodeLabelInput.text = _currentQRCode.codeLabel;
            _currentQRCode.GenerateQRCode(_QRCodeDisplayImage);
            _QRCodeSettingsPanel.SetActive(true);
        }
        else
        {   // Check if the cursor is over an existing QR code or create a new one
            Ray _ray;
            if (Camera.main.orthographic)
                _ray = Camera.main.ScreenPointToRay(_input.MapEditor.Position.ReadValue<Vector2>());
            else _ray = Camera.main.ScreenPointToRay(_input.RenderView.Position.ReadValue<Vector2>());

            Vector3 _cursorPosition = GetCursorPosition();

            if (Physics.Raycast(_ray, out RaycastHit _hit) && _hit.collider.CompareTag("QRCode"))
            {   // If select an existing QR code, select it
                _currentQRCode = _hit.collider.GetComponent<QRCodeController>();
                _currentQRCode.PlaySelectAnimation();
                _initialQRCodeLabel = _currentQRCode.codeLabel;
                _initialQRCodePosition = _currentQRCode.transform.position;
                _initialQRCodeRotation = _currentQRCode.transform.rotation;

                _QRCodeLabelInput.text = _currentQRCode.codeLabel;
                _QRCodeDisplayImage.texture = _currentQRCode.GetQRCodeTexture();
                _QRCodeSettingsPanel.SetActive(true);
            }
            else
            {   // If not select an existing QR code, create a new one
                if (_navMeshManager.isNavigationEnabled) return;
                if (_currentQRCode != null)
                {
                    if (_cursorPosition == new Vector3(9999, 9999, 9999)) return;
                    if (!Camera.main.orthographic) _currentQRCode.RotateMarkerUp();
                    _QRCodeSettingsPanel.SetActive(false);
                }
                GameObject _newQRCode = Instantiate(_QRCodePrefab, _cursorPosition, Quaternion.Euler(-90, -90, 0), this.transform);
                _currentQRCode = _newQRCode.GetComponent<QRCodeController>();
                _newQRCode.name = "QRCode_" + this.transform.childCount;
                _currentQRCode.codeLabel = _newQRCode.name;
                _isRotating = true;
                UpdatePositionInputFields();
            }
        }
    }

    private void UpdatePositionInputFields()
    {   // Update the input fields for the QR code settings
        if (Camera.main.orthographic)
        {   // Update the position input fields for the map editor
            _QRCodeXPosInput.text = _currentQRCode.transform.position.x.ToString();
            _QRCodeYPosInput.text = _currentQRCode.transform.position.y.ToString();
        }
        else
        {   // Update the position input fields for the 3D view
            _QRCodeXPosInput.text = _currentQRCode.transform.position.x.ToString();
            _QRCodeYPosInput.text = _currentQRCode.transform.position.z.ToString();
        }
    }

    public void ChangeQRCodeLabel()
    {   // Change the label of the current QR code
        if (_currentQRCode == null) return;
        if (_QRCodeLabelInput.text == "") return;
        if (_QRCodeLabelInput.text.Contains(":"))
            _errorMessageBox.ShowMessage("QRLabelCharactersAllowed");

        _currentQRCode.codeLabel = _QRCodeLabelInput.text;
        UpdateQRCode();
    }

    public void ChangeQRCodePosition()
    {   // Change the position of the current QR code by input
        if (_currentQRCode == null) return;
        if (_isMoving || _isRotating) return;
        if (_QRCodeXPosInput.text == "" || _QRCodeYPosInput.text == "") return;

        float _xPos = float.Parse(_QRCodeXPosInput.text);
        float _yPos = float.Parse(_QRCodeYPosInput.text);

        if (Camera.main.orthographic)
            _currentQRCode.transform.position = new Vector3(_xPos, _yPos, 0);
        else _currentQRCode.transform.position = new Vector3(_xPos, _markerHeight, _yPos);
    }

    public void ConfirmQRCodeSettings()
    {   // Confirm the settings of the current QR code
        if (!Camera.main.orthographic) _currentQRCode.RotateMarkerUp();
        _QRCodeSettingsPanel.SetActive(false);
        _initialQRCodeLabel = null;
        _isRotating = false;
        _isMoving = false;
    }

    public void CancelQRCodeSettings()
    {   // Cancel the settings of the current QR code
        if (!Camera.main.orthographic) _currentQRCode.RotateMarkerUp();
        _QRCodeSettingsPanel.SetActive(false);
        _isRotating = false;
        _isMoving = false;

        if (_initialQRCodeLabel == null) DeleteQRCode();
        else
        {   // Reset the current QR code to the initial settings
            _currentQRCode.codeLabel = _initialQRCodeLabel;
            _currentQRCode.transform.position = _initialQRCodePosition;
            _currentQRCode.transform.rotation = _initialQRCodeRotation;

            _QRCodeLabelInput.text = _initialQRCodeLabel;
            _QRCodeXPosInput.text = _initialQRCodePosition.x.ToString();
            _QRCodeYPosInput.text = _initialQRCodePosition.y.ToString();
            _currentQRCode.GenerateQRCode(_QRCodeDisplayImage);
            _initialQRCodeLabel = null;
        }
    }

    public void DeleteQRCode()
    {   // Delete the current QR code
        if (_currentQRCode == null) return;

        Destroy(_currentQRCode.gameObject);
        _QRCodeSettingsPanel.SetActive(false);
    }

    public void MoveQRCode()
    {   // Move the current QR code
        if (_currentQRCode == null) return;
        if (!Camera.main.orthographic)
            _currentQRCode.RotateMarkerUp();
        _isMoving = true;
        _isRotating = false;
    }

    public void RotateQRCode()
    {   // Rotate the current QR code
        if (_currentQRCode == null) return;
        _isRotating = true;
        _isMoving = false;
    }

    public void UpdateQRCode()
    {   // Update the current QR code
        if (_currentQRCode == null) return;
        _currentQRCode.GenerateQRCode(_QRCodeDisplayImage);
    }

    public void SaveQRCode()
    {   // Save the QR Code as a png image
        if (_currentQRCode == null) return;

        var extensionList = new[] { // Extension filter for the save file window
            new ExtensionFilter("Image Files", "png", "jpg", "jpeg" ),
            new ExtensionFilter("All Files", "*" )
        };
        string _path = StandaloneFileBrowser.SaveFilePanel("Save QRCode", "", _currentQRCode.codeLabel, extensionList);
        if (_path == "") return;
        byte[] _bytes = _currentQRCode.GetQRCodeTexture().EncodeToPNG();
        System.IO.File.WriteAllBytes(_path, _bytes);
        print(_path);
    }
}
