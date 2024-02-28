using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntrancesController : MonoBehaviour
{
    [Header("Required Components")]
    [SerializeField] private LineRenderer _lineRenderer;
    [SerializeField] private GameObject _startDot;
    [SerializeField] private GameObject _endDot;
    public WallLineController _entranceWall;
    private float _lenght = 0.80f; // Default lenght

    private void Start()
    {
        _startDot.transform.localRotation = Quaternion.Euler(-90, 0, 0);
        _endDot.transform.localRotation = Quaternion.Euler(-90, 0, 0);
    }

    public float CalculateLength()
    {
        _lenght = Vector3.Distance(_startDot.transform.position, _endDot.transform.position);
        return _lenght;
    }
    public void SetEntrancePosition(Vector3 _setPosition, WallLineController _wall)
    {   // Set the entrance line and dots position
        Vector3 _direction = (_wall.endDot.position - _wall.startDot.position).normalized;
        Vector3 _endPosition = _setPosition + _direction * _lenght;
        Vector3 _startPosition = _setPosition;

        _lineRenderer.SetPosition(0, _startPosition + new Vector3(0, 0, -0.5f));
        _lineRenderer.SetPosition(1, _endPosition + new Vector3(0, 0, -0.5f));

        _startDot.transform.position = _startPosition + new Vector3(0, 0, -0.6f);
        _endDot.transform.position = _endPosition + new Vector3(0, 0, -0.6f);

        _entranceWall = _wall;
    }

    public void ChangeEntranceSize(float _newLenght)
    {   // Change the line size moving the end dot
        Vector3 _direction = (_endDot.transform.position - _startDot.transform.position).normalized;
        Vector3 _newPosition = _startDot.transform.position + _direction * _newLenght;
        _newPosition.z = 0.0f;

        _endDot.transform.position = _newPosition;
        //SetLineCollider();
    }


}
