using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer), typeof(PolygonCollider2D))]
public class WallLineController : MonoBehaviour
{
    public WallDotController startDot;
    public WallDotController endDot;

    [Header("Required Components")]
    [SerializeField] private LineRenderer _lineRenderer;
    [SerializeField] private PolygonCollider2D _polygonCollider;

    [Header("3D Render")]
    [SerializeField] private GameObject _renderPrefab;

    private GameObject _renderWall;
    private MeshFilter _meshFilter;
    private Mesh _mesh;

    private float _length;

    void Start()
    {
        _lineRenderer = GetComponent<LineRenderer>();
        _polygonCollider = GetComponent<PolygonCollider2D>();

        Transform _renderParent = GameObject.Find("3DRender").transform.GetChild(0);
        _renderWall = Instantiate(_renderPrefab, Vector3.zero, Quaternion.identity, _renderParent);
        _renderWall.name = "Render_" + this.gameObject.name;
        _meshFilter = _renderWall.GetComponent<MeshFilter>();

        _mesh = new Mesh();
        _mesh.name = "Mesh_" + this.gameObject.name;
    }

    public float CalculateLength()
    {
        _length = Vector3.Distance(startDot.position, endDot.position);
        return _length;
    }

    public void DestroyLine(bool _fromDots = true)
    {   // Delete the line and its references
        if (_fromDots)
        {
            startDot.DeleteLine(startDot.lines.IndexOf(this.gameObject));
            endDot.DeleteLine(endDot.lines.IndexOf(this.gameObject));
        }
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
