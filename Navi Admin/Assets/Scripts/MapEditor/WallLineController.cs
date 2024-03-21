using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(LineRenderer), typeof(PolygonCollider2D))]
public class WallLineController : MonoBehaviour
{
    #region --- Public & Required Variables ---
    [Header("Wall Stuff")]
    public List<EntrancesController> entrancesList = new List<EntrancesController>();
    public List<WallLineController> connectedWalls = new List<WallLineController>();
    public float length;

    [Header("Dots")]
    public WallDotController startDot;
    public WallDotController endDot;

    [Header("Required Components")]
    [SerializeField] private LineRenderer _lineRenderer;
    [SerializeField] private PolygonCollider2D _polygonCollider;

    [Header("3D Render")]
    [SerializeField] private GameObject _renderPrefab;

    private PolygonsManager _polygonsManager;
    #endregion

    #region --- Wall Segments Variables ---
    private List<Vector3[]> _wallSegmentsPoints = new List<Vector3[]>();
    private List<float> _wallSegmentsLenghts = new List<float>();

    private List<Vector3[]> _segmentsPoints = new List<Vector3[]>();
    private List<float> _segmentsLengths = new List<float>();
    private List<int> _segmentsType = new List<int>();
    #endregion

    #region --- 3D Render Variables ---
    private GameObject _renderWall;
    private MeshFilter _meshFilter;
    private Mesh[] _meshes;
    #endregion

    void Start()
    {
        _polygonsManager = GameObject.Find("PolygonsManager").GetComponent<PolygonsManager>();

        Transform _renderParent = GameObject.Find("3DRender").transform.GetChild(0);
        _renderWall = Instantiate(_renderPrefab, Vector3.zero, Quaternion.identity, _renderParent);
        _renderWall.name = "Render_" + this.gameObject.name;
        _meshFilter = _renderWall.GetComponent<MeshFilter>();
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

    public void AddConnectedWall(WallLineController _wall)
    {   // Add a connected wall to the list and check for loops (closed areas)
        if (connectedWalls.Contains(_wall))
        {
            print("loop");
            // _polygonsManager.CreatePolygonMesh(connectedWalls);

        }
        connectedWalls.Add(_wall);
        if (!_wall.connectedWalls.Contains(this)) _wall.connectedWalls.Add(this);
    }

    public void SetLineCollider()
    {   // Generate the line collider
        Vector3[] _positions = { startDot.position, endDot.position };
        List<Vector2> _colliderPoints = CalculateMeshPoints(_positions);
        _polygonCollider.SetPath(0,
            _colliderPoints.ConvertAll(
                p => (Vector2)transform.InverseTransformPoint(p)));
    }

    #region --- Wall segments ---
    public Tuple<List<Vector3[]>, List<float>> GetWallSegments()
    {   // Get points from the wall segments between entrances
        List<Vector3> _points = GetSegmentsPoints();
        CalculateSegments(_points);

        _wallSegmentsPoints.Clear();
        _wallSegmentsLenghts.Clear();

        for (int i = 0; i < _segmentsPoints.Count; i++)
        {   // Save only the wall segments points and distances
            if (_segmentsType[i] == 0)
            {   // If the segment is a wall, consider this segment
                _wallSegmentsPoints.Add(_segmentsPoints[i]);
                _wallSegmentsLenghts.Add(_segmentsLengths[i]);
            }
        }
        return Tuple.Create(_wallSegmentsPoints, _wallSegmentsLenghts);
    }

    private Tuple<List<Vector3[]>, List<float>> CalculateSegments(List<Vector3> _points)
    {   // Calculate the segments length and group the points in pairs to get the wall segments
        _segmentsPoints.Clear();
        _segmentsLengths.Clear();

        for (int i = 0; i < _points.Count - 1; i++)
        {   // Calculate the distance between each point
            float _distance = Vector3.Distance(_points[i], _points[i + 1]);

            if (_distance > 0.0f)
            {   // If the distance is not 0, save the segment
                _segmentsLengths.Add(_distance);
                _segmentsPoints.Add(new Vector3[] { _points[i], _points[i + 1] });
            }
            else _segmentsType.RemoveAt(i);
        }
        for (int i = 0; i < _segmentsPoints.Count; i++)
        {   // Print the segments points and distances
            print(" Segment " + i + " | ditance: " + _segmentsLengths[i] + " | type: " + _segmentsType[i]);
        }
        return Tuple.Create(_segmentsPoints, _segmentsLengths);
    }

    private List<Vector3> GetSegmentsPoints()
    {   // Get the points from the wall segments between entrances
        List<Vector3> _points = new List<Vector3>();
        _segmentsType.Clear();

        _points.Add(startDot.position);
        foreach (EntrancesController _entrance in entrancesList)
        {   // Save the entrance points and the segment type between them
            Vector3 _entranceStart = _entrance.startDot.transform.position;
            Vector3 _entranceEnd = _entrance.endDot.transform.position;
            _entranceStart.z = 0;
            _entranceEnd.z = 0;

            _points.Add(_entranceStart);
            _segmentsType.Add(0);
            _points.Add(_entranceEnd);
            _segmentsType.Add(1);
        }
        _points.Add(endDot.position);
        _segmentsType.Add(0);

        _points.Sort((p1, p2) => Vector3.Distance(p1, startDot.position).CompareTo(Vector3.Distance(p2, startDot.position)));
        return _points;
    }
    #endregion

    #region --- 3D Render ---
    public void GenerateWallMesh()
    {   // Generate the 3D wall mesh
        _wallSegmentsPoints = GetWallSegments().Item1;
        _meshes = new Mesh[_wallSegmentsPoints.Count];

        _renderWall.transform.localRotation = Quaternion.identity;
        _renderWall.transform.localPosition = Vector3.zero;

        for (int i = 0; i < _wallSegmentsPoints.Count; i++)
        {   // Generate the mesh for each wall segment
            _meshes[i] = GenerateMesh(_wallSegmentsPoints[i]);
        }

        CombineInstance[] _combine = new CombineInstance[_meshes.Length];
        for (int i = 0; i < _meshes.Length; i++)
        {   // Combine the meshes into one mesh
            _combine[i].mesh = _meshes[i];
            _combine[i].transform = _renderWall.transform.localToWorldMatrix;
        }

        _meshFilter.mesh = new Mesh();
        _meshFilter.mesh.CombineMeshes(_combine);
        _renderWall.transform.localRotation = Quaternion.Euler(90, 0, 0);
        _renderWall.transform.localPosition = new Vector3(0, 2f, 0);
    }
    public Mesh GenerateMesh(Vector3[] _positions)
    {   // Generate the 3D mesh from line points
        Vector2[] _points = CalculateMeshPoints(_positions).ToArray();

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

        // Reverse the triangles if the line is drawn from right to left
        if (_positions[0].x > _positions[1].x) _triangles = _triangles.Reverse().ToArray();

        // If the line is vertical and drawn from bottom to top
        if (_positions[0].x == _positions[1].x && _positions[0].y > _positions[1].y)
            _triangles = _triangles.Reverse().ToArray();

        // Generate the mesh
        Mesh _mesh = new Mesh();
        _mesh.vertices = _vertices;
        _mesh.triangles = _triangles;
        _mesh.RecalculateNormals();
        _mesh.RecalculateBounds();
        return _mesh;
    }

    public List<Vector2> CalculateMeshPoints(Vector3[] _positions)
    {   // Calculate the points of the line collider
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
}
