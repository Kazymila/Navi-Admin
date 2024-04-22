using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ZXing.QrCode;
using ZXing;

public class QRCodeController : MonoBehaviour
{
    public string qrCodeName;
    private Texture2D _encodedTexture;
    private Animator _markerAnimator;
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
        Vector3 _QRDirection = CalculateQRCodeDirection();
        Vector3 _position3D = CalculateQRCodePosition();
        string _textForEncoding = $"{qrCodeName} :pos: {_position3D} :dir: {_QRDirection}";
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
