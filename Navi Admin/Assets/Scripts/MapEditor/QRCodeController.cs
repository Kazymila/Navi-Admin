using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ZXing.QrCode;
using ZXing;
using System;

public class QRCodeController : MonoBehaviour
{
    public String codeLabel;
    private Texture2D _encodedTexture;
    private Animator _markerAnimator;

    void Start()
    {
        _markerAnimator = GetComponent<Animator>();
        _encodedTexture = new Texture2D(256, 256);
    }

    public void PlaySelectAnimation() => _markerAnimator.Play("Selected", 0, 0);

    public Texture2D GetQRCodeTexture() => _encodedTexture;

    public void GenerateQRCode(RawImage _rawImage)
    {   // Generate a QR code from marker position and direction
        string _textForEncoding = $"{codeLabel}:x({transform.position.x})y({transform.position.y})";
        GenerateQRCodeFromText(_textForEncoding, _rawImage);
    }

    public void GenerateQRCodeFromText(string _textForEncoding, RawImage _rawImage)
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
}
