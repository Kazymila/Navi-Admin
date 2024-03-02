using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntrancesController : MonoBehaviour
{
    [Header("Required Components")]
    [SerializeField] private PolygonCollider2D _polygonCollider;
    [SerializeField] private LineRenderer _lineRenderer;
    [SerializeField] private GameObject _startDot;
    [SerializeField] private GameObject _endDot;

    [HideInInspector]
    public WallLineController _entranceWall;
    public bool _isOverEntrance = false;
    public bool _isSetted = false;

    private ErrorMessageController _errorMessageBox;
    private Animator _animator;
    private float _lenght = 0.80f; // Default lenght

    private void Start()
    {
        _animator = GetComponent<Animator>();
        _errorMessageBox = FindObjectOfType<ErrorMessageController>();
        _startDot.transform.localRotation = Quaternion.Euler(-90, 0, 0);
        _endDot.transform.localRotation = Quaternion.Euler(-90, 0, 0);
    }

    public void PlaySettedAnimation() => _animator.Play("Setted", 0, 0);

    public void PlayDeniedAnimation() => _animator.Play("Denied", 0, 0);

    public float CalculateLength()
    {   // Calculate the line lenght
        _lenght = Vector3.Distance(_startDot.transform.position, _endDot.transform.position);
        return _lenght;
    }

    private Vector3 GetProjectedPointOnWall(Vector3 _cursorPosition, Vector3 _startWallDot, Vector3 _endWallDot)
    {   // Project the cursor position on the wall line and return the projected point
        Vector3 _wallVector = _endWallDot - _startWallDot;
        Vector3 _cursorToWallVector = _cursorPosition - _startWallDot;

        float _projection = Vector3.Dot(_cursorToWallVector, _wallVector);
        float _wallLenght = Vector3.Distance(_startWallDot, _endWallDot);
        float _d = _projection / (_wallLenght * _wallLenght);

        if (_d <= 0) return _startWallDot;
        else if (_d >= 1) return _endWallDot;
        else return _startWallDot + _d * _wallVector;
    }

    public void SetEntrancePosition(Vector3 _position, WallLineController _wall)
    {   // Set the entrance line and dots position snap to the wall
        Vector3 _onWallPosition = GetProjectedPointOnWall(_position, _wall.startDot.position, _wall.endDot.position);
        Vector3 _direction = (_wall.endDot.position - _wall.startDot.position).normalized;

        // Check if the entrance is out of the wall limits and re position it
        if (Vector3.Distance(_onWallPosition, _wall.startDot.position) < (_lenght / 2) + 0.2f)
            _onWallPosition = _wall.startDot.position + _direction * ((_lenght / 2) + 0.2f);

        else if (Vector3.Distance(_onWallPosition, _wall.endDot.position) < (_lenght / 2) + 0.2f)
            _onWallPosition = _wall.endDot.position - _direction * ((_lenght / 2) + 0.2f);

        Vector3 _startPosition = _onWallPosition - _direction * (_lenght / 2);
        Vector3 _endPosition = _onWallPosition + _direction * (_lenght / 2);

        _lineRenderer.SetPosition(0, _startPosition + new Vector3(0, 0, -0.2f));
        _lineRenderer.SetPosition(1, _endPosition + new Vector3(0, 0, -0.2f));

        _startDot.transform.position = _startPosition + new Vector3(0, 0, -0.3f);
        _endDot.transform.position = _endPosition + new Vector3(0, 0, -0.3f);

        _entranceWall = _wall;
        SetLineCollider();
    }

    public void ChangeEntranceSize(float _newLenght)
    {   // Change the line size moving the end dot
        Vector3 _direction = (_endDot.transform.position - _startDot.transform.position).normalized;
        Vector3 _newPosition = _startDot.transform.position + _direction * _newLenght;
        _newPosition.z = 0.0f;

        _endDot.transform.position = _newPosition;
        SetLineCollider();
    }
    #region --- Line Collider ---
    public void SetLineCollider()
    {   // Generate the line collider
        List<Vector2> _colliderPoints = CalculateColliderPoints();
        _polygonCollider.SetPath(0,
            _colliderPoints.ConvertAll(
                p => (Vector2)transform.InverseTransformPoint(p)));
    }

    public List<Vector2> CalculateColliderPoints()
    {   // Calculate the points of the line collider
        Vector3[] _positions = { _startDot.transform.position, _endDot.transform.position };
        float _width = _lineRenderer.startWidth;

        //Calculate the gradient (m) of the line
        float _m = (_positions[1].y - _positions[0].y) / (_positions[1].x - _positions[0].x);
        float _deltaX = _width / 2; // Offset when the line is parallel to the y-axis
        float _deltaY = 0;

        if (!float.IsInfinity(_m)) // If the line is not parallel to the y-axis
        {
            _deltaX = (_width / 2f) * (_m / Mathf.Pow(_m * _m + 1, 0.5f));
            _deltaY = (_width / 2f) * (1 / Mathf.Pow(_m * _m + 1, 0.5f));
        }

        // Calculate offset from each point to mesh
        Vector3[] _offsets = new Vector3[2];
        _offsets[0] = new Vector3(-_deltaX, _deltaY);
        _offsets[1] = new Vector3(_deltaX, -_deltaY);

        // Generate mesh points
        List<Vector2> _colliderPoints = new List<Vector2>{
            _positions[0] + _offsets[0],
            _positions[1] + _offsets[0],
            _positions[1] + _offsets[1],
            _positions[0] + _offsets[1],
        };

        return _colliderPoints;
    }
    #endregion

    #region --- Trigger Events ---
    void OnTriggerStay2D(Collider2D _collider)
    {
        if (_collider.CompareTag("Entrance") && !_isSetted)
        {   // Cannot set an entrance over a existing one, so show error message
            _errorMessageBox.ShowMessage("CannotSetOverExistingEntrance");
            PlayDeniedAnimation();
            _isOverEntrance = true;
        }
    }

    void OnTriggerExit2D(Collider2D _collider)
    {
        if (_collider.CompareTag("Entrance"))
        {   // Hide the error message
            _errorMessageBox.HideMessage();
            _isOverEntrance = false;

            if (!_isSetted) _animator.Play("Reset", 0, 0);
            else PlaySettedAnimation();
        }
    }
    #endregion
}
