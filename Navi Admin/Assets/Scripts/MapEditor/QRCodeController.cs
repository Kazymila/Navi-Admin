using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ZXing.QrCode;
using ZXing;
using System;
using Unity.VisualScripting;

public class QRCodeController : MonoBehaviour
{
    public String codeLabel;
    private Texture2D _encodedTexture;
    private Animator _markerAnimator;

    private Vector3 _QRDirection;
    private float _markerHeight = 0.6f;

    void Start()
    {
        _markerAnimator = GetComponent<Animator>();
        _encodedTexture = new Texture2D(256, 256);
    }

    public void PlaySelectAnimation() => _markerAnimator.Play("Selected", 0, 0);

    public Texture2D GetQRCodeTexture() => _encodedTexture;

    public void GenerateQRCode(RawImage _rawImage)
    {   // Generate a QR code from marker position and direction
        Vector3 _position3D = CalculateQRCodePosition();
        string _textForEncoding = $"{codeLabel}:pos:x{_position3D.x}y{_position3D.y}z{_position3D.z}:dir:x{_QRDirection.x}y{_QRDirection.y}z{_QRDirection.z}";
        GenerateQRCodeFromText(_textForEncoding, _rawImage);
    }

    public void CalculateQRCodeDirection()
    {   // Calculate the direction to look at the QR code
        if (Camera.main.orthographic)
            _QRDirection = new Vector3(transform.forward.x, 0, transform.forward.y);
        else _QRDirection = transform.forward;

        Debug.DrawRay(transform.position, _QRDirection, Color.red, 10f);
        print(_QRDirection);
    }

    private Vector3 CalculateQRCodePosition()
    {   // Calculate the 3D position from the 2D position
        if (Camera.main.orthographic)
            return new Vector3(transform.position.x, _markerHeight, transform.position.y);
        else return transform.position;
    }

    private void GenerateQRCodeFromText(string _textForEncoding, RawImage _rawImage)
    {   // Generate a QR code from the given text
        Color32[] _pixels = EncodeQRCode(_textForEncoding);
        _encodedTexture.SetPixels32(_pixels);
        _encodedTexture.Apply();

        _rawImage.texture = _encodedTexture;
    }

    private Color32[] EncodeQRCode(string _textForEncoding)
    {   // Encode the given text into a QR code
        BarcodeWriter _qrCodeWriter = new BarcodeWriter
        {
            Format = BarcodeFormat.QR_CODE,
            Options = new QrCodeEncodingOptions
            {
                Height = _encodedTexture.height,
                Width = _encodedTexture.width
            }
        };
        return _qrCodeWriter.Write(_textForEncoding);
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
