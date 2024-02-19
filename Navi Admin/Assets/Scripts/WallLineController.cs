using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer), typeof(PolygonCollider2D))]
public class WallLineController : MonoBehaviour
{
    public WallDotController startDot;
    public WallDotController endDot;

    [SerializeField] private LineRenderer _lineRenderer;
    [SerializeField] private PolygonCollider2D _polygonCollider;
    private MeshFilter _meshFilter;
    private Mesh _mesh;

    private float _length;

    void Start()
    {
        _lineRenderer = GetComponent<LineRenderer>();
        _polygonCollider = GetComponent<PolygonCollider2D>();

        //_meshFilter = GetComponent<MeshFilter>();
        //_mesh = new Mesh();
        //_mesh.name = "Mesh_" + this.gameObject.name;
    }

    public float CalculateLength()
    {
        _length = Vector3.Distance(startDot.position, endDot.position);
        return _length;
    }

    public void DeleteLine()
    {   // Delete the line and its references
        startDot.DeleteLine(startDot.lines.IndexOf(this.gameObject));
        endDot.DeleteLine(endDot.lines.IndexOf(this.gameObject));
        Destroy(this.gameObject);
    }

    public void SetLineCollider()
    {   // Generate the line collider
        List<Vector2> _colliderPoints = CalculateColliderPoints();
        _polygonCollider.SetPath(0,
            _colliderPoints.ConvertAll(
                p => (Vector2)transform.InverseTransformPoint(p)));
    }

    public List<Vector2> CalculateColliderPoints()
    {   // Calculate the points of the line collider
        Vector3[] _positions = { startDot.position, endDot.position };
        float _width = _lineRenderer.startWidth;

        //Calculate the gradient of the line
        float _m = (_positions[1].y - _positions[0].y) / (_positions[1].x - _positions[0].x);
        float _deltaX = (_width / 2f) * (_m / Mathf.Pow(_m * _m + 1, 0.5f));
        float _deltaY = (_width / 2f) * (1 / Mathf.Pow(_m * _m + 1, 0.5f));

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

    /*
    public void GeneratePlane()
    {   // Generate the plane mesh
        _mesh.vertices = CalculateColliderPoints();
        _mesh.triangles = new int[] { 0, 1, 2, 2, 1, 3 };
        _mesh.RecalculateNormals();
        _mesh.RecalculateBounds();
        _meshFilter.mesh = _mesh;
    }*/
}
