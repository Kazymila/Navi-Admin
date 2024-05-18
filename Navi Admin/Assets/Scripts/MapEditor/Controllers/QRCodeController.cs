using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ZXing.QrCode;
using ZXing;
using ZXing.QrCode.Internal;

public class QRCodeController : MonoBehaviour
{
    public string qrCodeName;
    private Texture2D _encodedTexture;
    private Animator _markerAnimator;

    // Logo on the QR code
    private Texture2D _onCodeLogoImage;
    private int _logoImageSize;

    void Start()
    {
        _markerAnimator = GetComponent<Animator>();
        _encodedTexture = new Texture2D(256, 256);
    }

    public void PlaySelectAnimation() => _markerAnimator.Play("Selected", 0, 0);

    public Texture2D GetQRCodeTexture() => _encodedTexture;

    public void GenerateQRCode(RawImage _rawImage, Texture2D _onCodeLogo, int _logoSize)
    {   // Generate a QR code from marker position and direction
        Vector3 _direction3D = CalculateQRCodeDirection();
        Vector3 _position3D = CalculateQRCodePosition();

        string _textForEncoding = $"{_position3D}pos:dir{_direction3D}";
        _onCodeLogoImage = _onCodeLogo;
        _logoImageSize = _logoSize;

        GenerateQRCodeFromText(_textForEncoding, _rawImage);
    }

    public Vector3 CalculateQRCodeDirection()
    {   // Calculate the direction to look at the QR code
        if (Camera.main.orthographic)
            return new Vector3(transform.forward.x, 0, transform.forward.y);
        else return transform.forward;
    }

    private Vector3 CalculateQRCodePosition()
    {   // Calculate the 3D position from the 2D position
        if (Camera.main.orthographic)
            return new Vector3(transform.position.x, 0, transform.position.y);
        else return transform.position;
    }

    private void GenerateQRCodeFromText(string _textForEncoding, RawImage _rawImage)
    {   // Generate a QR code from the given text
        BarcodeWriter _qrCodeWriter = new BarcodeWriter
        {
            Format = BarcodeFormat.QR_CODE,
            Options = new QrCodeEncodingOptions
            {
                Height = _encodedTexture.height,
                Width = _encodedTexture.width
            }
        };
        _qrCodeWriter.Options.Hints.Add(EncodeHintType.ERROR_CORRECTION, ErrorCorrectionLevel.H);
        Color32[] _bitmap = _qrCodeWriter.Write(_textForEncoding);

        // add a logo to the center of the QR code
        int _logoStart = (_encodedTexture.width - _logoImageSize) / 2;

        for (int i = 0; i < _logoImageSize; i++)
            for (int j = 0; j < _logoImageSize; j++)
                _bitmap[(_logoStart + i) + (_logoStart + j) * _encodedTexture.width] = _onCodeLogoImage.GetPixel(i, j);

        // Apply the QR code to the raw image
        _encodedTexture.SetPixels32(_bitmap);
        _encodedTexture.Apply();

        _rawImage.texture = _encodedTexture;
    }

    public void RotateMarkerUp()
    {   // Rotate the marker up (only for perspective view)
        this.transform.rotation = Quaternion.Euler(
            -90,
            this.transform.rotation.eulerAngles.y,
            this.transform.rotation.eulerAngles.z
        );
    }
}
