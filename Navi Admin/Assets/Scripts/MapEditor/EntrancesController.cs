using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer), typeof(PolygonCollider2D))]
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
    public float lenght = 0.825f; // Default entrance size

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

    public void ActivateDots(bool _show = true)
    {   // Show or hide the entrance dots
        startDot.SetActive(_show);
        endDot.SetActive(_show);
    }

    public float CalculateLength(Vector3[] _positions = null)
    {   // Calculate the line lenght
        if (_positions != null) lenght = Vector3.Distance(_positions[0], _positions[1]);
        else lenght = Vector3.Distance(startDot.transform.position, endDot.transform.position);
        return lenght;
    }

    public void DestroyEntrance()
    {   // Delete the entrance and its references
        entranceWall.entrancesList.Remove(this);
        Destroy(this.gameObject);
    }

    #region --- Entrance Position ---
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
        Vector3 _startPosition = _position - _direction * (lenght / 2);
        Vector3 _endPosition = _position + _direction * (lenght / 2);

        _lineRenderer.SetPosition(0, _startPosition + new Vector3(0, 0, -0.25f));
        _lineRenderer.SetPosition(1, _endPosition + new Vector3(0, 0, -0.25f));

        startDot.transform.position = _startPosition + new Vector3(0, 0, -0.5f);
        endDot.transform.position = _endPosition + new Vector3(0, 0, -0.5f);

        SetLineCollider();
    }

    public void SetFullWallEntrance()
    {   // Set the entrance on the wall limits
        _lineRenderer.SetPosition(0, entranceWall.startDot.position + new Vector3(0, 0, -0.25f));
        _lineRenderer.SetPosition(1, entranceWall.endDot.position + new Vector3(0, 0, -0.25f));

        startDot.transform.position = entranceWall.startDot.position + new Vector3(0, 0, -0.5f);
        endDot.transform.position = entranceWall.endDot.position + new Vector3(0, 0, -0.5f);

        SetLineCollider();
    }

    public void SetEntrancePositionFromCursor(Vector3 _position, WallLineController _wall)
    {   // Set the entrance by cursor distance to wall
        Vector3 _projectedPos = GetProjectedPointOnWall(_position, _wall.startDot.position, _wall.endDot.position);
        Vector3 _direction = (_wall.endDot.position - _wall.startDot.position).normalized;

        // Check if the entrance is out of the wall limits and re position it
        if (Vector3.Distance(_projectedPos, _wall.startDot.position) < (lenght / 2) + 0.15f)
            _projectedPos = _wall.startDot.position + _direction * ((lenght / 2));

        else if (Vector3.Distance(_projectedPos, _wall.endDot.position) < (lenght / 2) + 0.15f)
            _projectedPos = _wall.endDot.position - _direction * ((lenght / 2));

        SetEntrancePosition(_projectedPos, _direction);
        entranceWall = _wall;
    }

    public void RepositionEntranceOnWall(Vector3 _position, WallLineController _wall)
    {   // Reposition the entrance on wall
        Vector3 _projectedPos = GetProjectedPointOnWall(_position, _wall.startDot.position, _wall.endDot.position);
        Vector3 _direction = (_wall.endDot.position - _wall.startDot.position).normalized;

        if (lenght >= _wall.length)
        {   // If the entrance is bigger than the wall, set entrance to same size
            lenght = _wall.length;
            entranceWall = _wall;
            SetFullWallEntrance();
            return;
        }
        if (Vector3.Distance(_projectedPos, _wall.startDot.position) < (lenght / 2) + 0.15f)
            _projectedPos = _wall.startDot.position + _direction * ((lenght / 2));

        else if (Vector3.Distance(_projectedPos, _wall.endDot.position) < (lenght / 2) + 0.15f)
            _projectedPos = _wall.endDot.position - _direction * ((lenght / 2));

        SetEntrancePosition(_projectedPos, _direction);
        entranceWall = _wall;
    }
    #endregion

    public void MoveEntranceDot(Vector3 _position, GameObject _dot)
    {   // Move the entrance dot to the cursor position resizing the entrance
        Vector3 _projectedPos = GetProjectedPointOnWall(_position, entranceWall.startDot.position, entranceWall.endDot.position);
        Vector3 _otherDotPosition = _dot == startDot ? endDot.transform.position : startDot.transform.position;
        _otherDotPosition.z = 0;

        CalculateLength(new Vector3[] { _projectedPos, _otherDotPosition });
        Vector3 _direction = (_otherDotPosition - _projectedPos).normalized;
        Vector3 _newPosition = _otherDotPosition - _direction * lenght;

        _lineRenderer.SetPosition(_dot == startDot ? 0 : 1, _newPosition + new Vector3(0, 0, -0.25f));
        _dot.transform.position = _newPosition + new Vector3(0, 0, -0.5f);
        SetLineCollider();
    }

    public void ResizeEntrance(float _newLenght)
    {   // Change the entrance size
        if (_newLenght > entranceWall.length)
        {   // If the entrance is bigger than the wall
            _errorMessageBox.ShowMessage("EntranceTooBig");
            PlayDeniedAnimation();
        }
        else
        {   // Change the entrance size
            PlaySettedAnimation();
            _errorMessageBox.HideMessage();
        }
        lenght = _newLenght;
        RepositionEntranceOnWall(this.transform.localPosition, entranceWall);
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
    private void OnTriggerStay2D(Collider2D _collider)
    {
        if (_collider.CompareTag("Entrance") && !isSetted)
        {   // Cannot set an entrance over a existing one, so show error message
            _errorMessageBox.ShowMessage("CannotSetOverExistingEntrance");
            PlayDeniedAnimation();
            isOverEntrance = true;
        }
    }

    private void OnTriggerExit2D(Collider2D _collider)
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
