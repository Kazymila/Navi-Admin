using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer), typeof(PolygonCollider2D))]
public class WallLineController : MonoBehaviour
{
    #region --- Public & Required Variables ---
    [Header("Wall Stuff")]
    public List<EntrancesController> entrancesList = new List<EntrancesController>();
    public float length;

    [Header("Dots")]
    public WallDotController startDot;
    public WallDotController endDot;

    [Header("Required Components")]
    [SerializeField] private LineRenderer _lineRenderer;
    [SerializeField] private PolygonCollider2D _polygonCollider;

    [Header("3D Render")]
    [SerializeField] private GameObject _renderPrefab;
    #endregion

    private GameObject _renderWall;
    private MeshFilter _meshFilter;
    private Mesh _mesh;

    void Start()
    {
        Transform _renderParent = GameObject.Find("3DRender").transform.GetChild(0);
        _renderWall = Instantiate(_renderPrefab, Vector3.zero, Quaternion.identity, _renderParent);
        _renderWall.name = "Render_" + this.gameObject.name;
        _meshFilter = _renderWall.GetComponent<MeshFilter>();

        _mesh = new Mesh();
        _mesh.name = "Mesh_" + this.gameObject.name;
    }

    public float CalculateLength()
    {   // Calculate the line lenght
        length = Vector3.Distance(startDot.position, endDot.position);
        return length;
    }

    public void ResizeWall(float _newLenght)
    {   // Resize the wall line and update the dots position
        Vector3 _direction = (endDot.position - startDot.position).normalized;
        Vector3 _midPosition = (endDot.position + startDot.position) / 2;

        length = _newLenght;
        Vector3 _startPosition = _midPosition - _direction * (length / 2);
        Vector3 _endPosition = _midPosition + _direction * (length / 2);

        startDot.SetPosition(_startPosition);
        endDot.SetPosition(_endPosition);

        SetLineCollider();
    }

    public void DestroyLine(bool _fromDots = true)
    {   // Delete the line and its references
        if (_fromDots)
        {
            startDot.DeleteLine(startDot.lines.IndexOf(this.gameObject));
            endDot.DeleteLine(endDot.lines.IndexOf(this.gameObject));
        }
        foreach (EntrancesController _entrance in entrancesList) Destroy(_entrance.gameObject);
        Destroy(_renderWall);
        Destroy(this.gameObject);
    }

    public void SetLineCollider()
    {   // Generate the line collider
        List<Vector2> _colliderPoints = CalculateColliderPoints();
        _polygonCollider.SetPath(0,
            _colliderPoints.ConvertAll(
                p => (Vector2)transform.InverseTransformPoint(p)));
    }

    public Tuple<List<Vector3[]>, List<float>, List<int>> GetWallSegments()
    {   // Get points from the wall segments between entrances
        List<Vector3> _points = new List<Vector3>();
        List<float> _distances = new List<float>();
        List<int> _segmentType = new List<int>();     // 0 = wall, 1 = entrance
        List<Vector3[]> _wallPoints = new List<Vector3[]>(); // Pairs of points

        _points.Add(startDot.position);
        foreach (EntrancesController _entrance in entrancesList)
        {
            Vector3 _entranceStart = _entrance.startDot.transform.position;
            Vector3 _entranceEnd = _entrance.endDot.transform.position;
            _entranceStart.z = 0;
            _entranceEnd.z = 0;

            _points.Add(_entranceStart);
            _segmentType.Add(0);
            _points.Add(_entranceEnd);
            _segmentType.Add(1);
        }
        _points.Add(endDot.position);
        _segmentType.Add(0);

        _points.Sort((p1, p2) => Vector3.Distance(p1, startDot.position).CompareTo(Vector3.Distance(p2, startDot.position)));

        for (int i = 0; i < _points.Count - 1; i++)
        {   // Save the distance between each point and group the points in pairs
            float _distance = Vector3.Distance(_points[i], _points[i + 1]);
            if (_distance > 0.0f)
            {   // If the distance is not 0, save the segment
                _distances.Add(_distance);
                _wallPoints.Add(new Vector3[] { _points[i], _points[i + 1] });
            }
            else _segmentType.RemoveAt(i);
        }

        for (int i = 0; i < _segmentType.Count; i++) // Debug the segment type and distance
            Debug.Log("Segment " + i + " type: " + _segmentType[i] + " distance: " + _distances[i]);

        return Tuple.Create(_wallPoints, _distances, _segmentType);
    }

    #region --- 3D Render ---
    public List<Vector2> CalculateColliderPoints()
    {   // Calculate the points of the line collider
        Vector3[] _positions = { startDot.position, endDot.position };
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

    public void GenerateWallMesh()
    {   // Generate the 3D wall mesh
        Vector2[] _points = CalculateColliderPoints().ToArray();

        Vector3[] _vertices = new Vector3[] {
            // Bottom vertices
            new Vector3(_points[0].x, _points[0].y, 0),
            new Vector3(_points[1].x, _points[1].y, 0),
            new Vector3(_points[2].x, _points[2].y, 0),
            new Vector3(_points[3].x, _points[3].y, 0),

            // Top vertices
            new Vector3(_points[0].x, _points[0].y, 2),
            new Vector3(_points[1].x, _points[1].y, 2),
            new Vector3(_points[2].x, _points[2].y, 2),
            new Vector3(_points[3].x, _points[3].y, 2),

            // Front vertices
            new Vector3(_points[0].x, _points[0].y, 0),
            new Vector3(_points[1].x, _points[1].y, 0),
            new Vector3(_points[1].x, _points[1].y, 2),
            new Vector3(_points[0].x, _points[0].y, 2),

            // Back vertices
            new Vector3(_points[3].x, _points[3].y, 0),
            new Vector3(_points[2].x, _points[2].y, 0),
            new Vector3(_points[2].x, _points[2].y, 2),
            new Vector3(_points[3].x, _points[3].y, 2),

            // Left vertices
            new Vector3(_points[0].x, _points[0].y, 0),
            new Vector3(_points[3].x, _points[3].y, 0),
            new Vector3(_points[3].x, _points[3].y, 2),
            new Vector3(_points[0].x, _points[0].y, 2),

            // Right vertices
            new Vector3(_points[1].x, _points[1].y, 0),
            new Vector3(_points[2].x, _points[2].y, 0),
            new Vector3(_points[2].x, _points[2].y, 2),
            new Vector3(_points[1].x, _points[1].y, 2),
            };

        int[] _triangles = new int[] {
            // Bottom face
            0, 1, 2,
            0, 2, 3,

            // Top face
            5, 4, 6,
            6, 4, 7,

            // Front face
            9, 8, 10,
            10, 8, 11,

            // Back face
            12, 13, 14,
            12, 14, 15,

            // Left face
            16, 17, 18,
            16, 18, 19,

            // Right face
            21, 20, 22,
            22, 20, 23,
        };

        _mesh.vertices = _vertices;
        _mesh.triangles = _triangles;
        _mesh.RecalculateNormals();
        _mesh.RecalculateBounds();

        _meshFilter.mesh = _mesh;
        _renderWall.transform.localRotation = Quaternion.Euler(90, 0, 0);
        _renderWall.transform.localPosition = new Vector3(0, 2f, 0);
    }
    #endregion
}
