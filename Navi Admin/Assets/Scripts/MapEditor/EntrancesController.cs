using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntrancesController : MonoBehaviour
{
    [Header("Required Components")]
    [SerializeField] private PolygonCollider2D _polygonCollider;
    [SerializeField] private LineRenderer _lineRenderer;

    public GameObject startDot;
    public GameObject endDot;

    [HideInInspector]
    public WallLineController entranceWall;
    public bool isOverEntrance = false; // To check if CAN be setted
    public bool isSetted = false;   // To check if is already setted
    public float width = 0.825f; // Default entrance size

    private ErrorMessageController _errorMessageBox;
    private Animator _animator;

    private void Start()
    {
        _animator = GetComponent<Animator>();
        _errorMessageBox = FindObjectOfType<ErrorMessageController>();
        startDot.transform.localRotation = Quaternion.Euler(-90, 0, 0);
        endDot.transform.localRotation = Quaternion.Euler(-90, 0, 0);
        this.transform.localPosition = new Vector3(
            this.transform.localPosition.x,
            this.transform.localPosition.y,
            -0.25f);
        PlayMovingAnimation();
    }

    public void PlayMovingAnimation() => _animator.Play("Reset", 0, 0);
    public void PlaySettedAnimation() => _animator.Play("Setted", 0, 0);
    public void PlayDeniedAnimation() => _animator.Play("Denied", 0, 0);

    public float CalculateLength()
    {   // Calculate the line lenght
        width = Vector3.Distance(startDot.transform.position, endDot.transform.position);
        return width;
    }

    private Vector3 GetProjectedPointOnWall(Vector3 _cursorPosition, Vector3 _startWallDot, Vector3 _endWallDot)
    {   // Project the cursor position on the wall line and return the projected point
        _cursorPosition.z = 0;
        Vector3 _wallVector = _endWallDot - _startWallDot;
        Vector3 _cursorToWallVector = _cursorPosition - _startWallDot;

        float _projection = Vector3.Dot(_cursorToWallVector, _wallVector);
        float _wallLenght = Vector3.Distance(_startWallDot, _endWallDot);
        float _d = _projection / (_wallLenght * _wallLenght);

        if (_d <= 0) return _startWallDot;
        else if (_d >= 1) return _endWallDot;
        else return _startWallDot + _d * _wallVector;
    }

    public void SetEntrancePosition(Vector3 _position, Vector3 _direction)
    {   // Set the entrance line and dots position snap to the wall
        Vector3 _startPosition = _position - _direction * (width / 2);
        Vector3 _endPosition = _position + _direction * (width / 2);

        _lineRenderer.SetPosition(0, _startPosition + new Vector3(0, 0, -0.25f));
        _lineRenderer.SetPosition(1, _endPosition + new Vector3(0, 0, -0.25f));

        startDot.transform.position = _startPosition + new Vector3(0, 0, -0.25f);
        endDot.transform.position = _endPosition + new Vector3(0, 0, -0.25f);

        SetLineCollider();
    }

    public void SetEntrancePositionFromCursor(Vector3 _position, WallLineController _wall)
    {   // Set the entrance by cursor distance to wall
        Vector3 _projectedPos = GetProjectedPointOnWall(_position, _wall.startDot.position, _wall.endDot.position);
        Vector3 _direction = (_wall.endDot.position - _wall.startDot.position).normalized;

        // Check if the entrance is out of the wall limits and re position it
        if (Vector3.Distance(_projectedPos, _wall.startDot.position) < (width / 2) + 0.2f)
            _projectedPos = _wall.startDot.position + _direction * ((width / 2) + 0.2f);

        else if (Vector3.Distance(_projectedPos, _wall.endDot.position) < (width / 2) + 0.2f)
            _projectedPos = _wall.endDot.position - _direction * ((width / 2) + 0.2f);

        SetEntrancePosition(_projectedPos, _direction);
        entranceWall = _wall;
    }

    public void RepositionEntranceOnWall(Vector3 _position, WallLineController _wall)
    {   // Reposition the entrance on wall
        Vector3 _projectedPos = GetProjectedPointOnWall(_position, _wall.startDot.position, _wall.endDot.position);
        Vector3 _direction = (_wall.endDot.position - _wall.startDot.position).normalized;

        SetEntrancePosition(_projectedPos, _direction);
        entranceWall = _wall;
    }

    public void ChangeEntranceSize(float _newLenght)
    {   // Change the line size moving the dots
        width = _newLenght;
        RepositionEntranceOnWall(this.transform.localPosition, entranceWall);
    }

    public void MoveEntranceDot(Vector3 _position, GameObject _dot)
    {   // Move the entrance dot to the cursor position
        // TODO: move the entrance dot
        SetLineCollider();
    }

    public void DestroyEntrance()
    {   // Delete the entrance and its references
        entranceWall.entrancesList.Remove(this);
        Destroy(this.gameObject);
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
        Vector3[] _positions = { startDot.transform.position, endDot.transform.position };
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
        if (_collider.CompareTag("Entrance") && !isSetted)
        {   // Cannot set an entrance over a existing one, so show error message
            _errorMessageBox.ShowMessage("CannotSetOverExistingEntrance");
            PlayDeniedAnimation();
            isOverEntrance = true;
        }
    }

    void OnTriggerExit2D(Collider2D _collider)
    {
        if (_collider.CompareTag("Entrance"))
        {   // Hide the error message
            _errorMessageBox.HideMessage();
            isOverEntrance = false;

            if (!isSetted) PlayMovingAnimation();
            else PlaySettedAnimation();
        }
    }
    #endregion
}
