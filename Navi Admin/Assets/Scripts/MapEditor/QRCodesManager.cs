using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QRCodesManager : MonoBehaviour
{
    #region --- External Variables ---
    [Header("UI Components")]
    [SerializeField] private EditorLayoutController _UIEditorController;
    [SerializeField] private ErrorMessageController _errorMessageBox;

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
    private bool _isRotating;
    private bool _isMoving;

    private InputMap _input;
    #endregion

    private void OnEnable()
    {
        _input = new InputMap();
        _input.MapEditor.Enable();
        _input.MapEditor.Click.started += ctx => OnClick();
        //_input.MapEditor.EndDraw.started += ctx => CancelAction();
    }
    private void OnDisable()
    {
        _input.MapEditor.Disable();
        _QRCodeSettingsPanel.SetActive(false);
    }

    void Start()
    {
        _QRCodePanelRect = new RectTransform[] { _QRCodeSettingsPanel.GetComponent<RectTransform>() };
    }

    private Vector3 GetCursorPosition()
    {   // Get the cursor position in the world
        Vector3 _cursorPosition = Camera.main.ScreenToWorldPoint(_input.MapEditor.Position.ReadValue<Vector2>());
        _cursorPosition.z = 0;

        return _cursorPosition;
    }

    private void Update()
    {
        if (_isRotating)
        {   // Rotate the QR code to the cursor position
            _currentQRCode.transform.LookAt(GetCursorPosition(), Vector3.right);
        }
        if (_isMoving)
        {   // Move the QR code to the cursor position
            _currentQRCode.transform.position = GetCursorPosition();
            _QRCodeXPosInput.text = _currentQRCode.transform.position.x.ToString();
            _QRCodeYPosInput.text = _currentQRCode.transform.position.y.ToString();
        }
    }

    private void OnClick()
    {   // Select and edit a QR code or create a new one
        if (_UIEditorController.IsCursorOverEditorUI(_QRCodePanelRect)) return;
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
            Vector3 _cursorPosition = GetCursorPosition();
            Ray _ray = Camera.main.ScreenPointToRay(_input.MapEditor.Position.ReadValue<Vector2>());

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
                _QRCodeSettingsPanel.SetActive(false);
                GameObject _newQRCode = Instantiate(_QRCodePrefab, _cursorPosition, Quaternion.Euler(-90, -90, 0), this.transform);
                _currentQRCode = _newQRCode.GetComponent<QRCodeController>();
                _newQRCode.name = "QRCode_" + this.transform.childCount;
                _currentQRCode.codeLabel = _newQRCode.name;
                _isRotating = true;

                _QRCodeXPosInput.text = _currentQRCode.transform.position.x.ToString();
                _QRCodeYPosInput.text = _currentQRCode.transform.position.y.ToString();
            }
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
        _currentQRCode.transform.position = new Vector3(_xPos, _yPos, 0);
    }

    public void ConfirmQRCodeSettings()
    {   // Confirm the settings of the current QR code
        _QRCodeSettingsPanel.SetActive(false);
        _initialQRCodeLabel = null;
        _isRotating = false;
        _isMoving = false;
    }

    public void CancelQRCodeSettings()
    {   // Cancel the settings of the current QR code
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

        string _path = Application.persistentDataPath + "/" + _currentQRCode.codeLabel + ".png";
        byte[] _bytes = _currentQRCode.GetQRCodeTexture().EncodeToPNG();
        System.IO.File.WriteAllBytes(_path, _bytes);
        print(_path);
    }

    private void OpenFileExplorer()
    {   // Open the file explorer to choose the path to save the QR code


    }
}
