using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Haze;
using System.Linq;

public class ShapeController : MonoBehaviour
{
    public float shape3DHeight = 0.5f;
    public Vector3 dotOffset = new Vector3(0, 0, -0.5f);
    private PolygonCollider2D _linePolygonCollider;
    private LineRenderer _lineRenderer;
    private GameObject _shapePolygonMesh;

    private List<Transform> _points;

    private void Awake()
    {
        _linePolygonCollider = GetComponent<PolygonCollider2D>();
        _lineRenderer = GetComponent<LineRenderer>();
        _lineRenderer.positionCount = 0;
        _points = new List<Transform>();
    }

    private void OnDestroy()
    {   // Destroy all the points when shape is destroyed
        foreach (Transform point in _points)
            Destroy(point.gameObject);
    }

    public int GetPointsCount() => _lineRenderer.positionCount;

    public void AddPoint(Transform point)
    {   // Add a new point to the line
        _points.Add(point);
        _lineRenderer.positionCount++;
        _lineRenderer.SetPosition(_points.Count - 1, point.position);
    }

    public void UpdateLastPoint(Vector3 _position)
    {   // Update the last point of the line
        if (_points.Count > 0)
        {
            _lineRenderer.SetPosition(_points.Count - 1, _position);
            _points[_points.Count - 1].position = _position + dotOffset;
        }
    }

    public void EndShape()
    {   // Close the shape
        if (_points.Count > 2)
            _lineRenderer.loop = true;
        SetLineCollider();
    }

    public void RemoveLastPoint()
    {   // Remove the last point of the line
        if (_points.Count > 0)
        {
            Destroy(_points[_points.Count - 1].gameObject);
            _points.RemoveAt(_points.Count - 1);
            _lineRenderer.positionCount--;
        }
    }

    public void DestroyShape()
    {   // Destroy the shape
        if (_shapePolygonMesh != null) Destroy(_shapePolygonMesh);
        Destroy(this.gameObject);
    }

    #region --- Line Collider ---
    public void SetLineCollider()
    {   // Set the collider points of the line
        Vector3[] _positions = _points.ConvertAll(point => point.position).ToArray();
        _positions = _positions.Concat(new Vector3[] { _positions[0] }).ToArray();

        if (_positions.Count() >= 2)
        {
            int _linesCount = _positions.Count() - 1;
            _linePolygonCollider.pathCount = _linesCount;

            for (int i = 0; i < _linesCount; i++)
            {   // Set the collider points of each line segment of the shape
                List<Vector2> _colliderPoints = CalculateLineColliderPoints(new List<Vector2> { _positions[i], _positions[i + 1] });
                _linePolygonCollider.SetPath(i, _colliderPoints.ConvertAll(p => (Vector2)transform.InverseTransformPoint(p)).ToArray());
            }
        }
        else _linePolygonCollider.pathCount = 0;
    }

    public List<Vector2> CalculateLineColliderPoints(List<Vector2> _positions)
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
        Vector2[] _offsets = new Vector2[2];
        _offsets[0] = new Vector2(-_deltaX, _deltaY);
        _offsets[1] = new Vector2(_deltaX, -_deltaY);

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

    #region --- Last segment length ---
    public float GetLastSegmentLength()
    {   // Get the length of the last segment of the line
        if (_points.Count > 0)
        {
            Vector3 _lastPoint = _points[_points.Count - 1].position;
            Vector3 _previousPoint = _points[_points.Count - 2].position;
            return Vector3.Distance(_lastPoint, _previousPoint);
        }
        else return 0;
    }

    public Vector3 GetLastSegmentCenter()
    {   // Get the center of the last segment of the line
        if (_points.Count > 0)
        {
            Vector3 _lastPoint = _points[_points.Count - 1].position;
            Vector3 _previousPoint = _points[_points.Count - 2].position;
            return (_lastPoint + _previousPoint) / 2;
        }
        else return Vector3.zero;
    }
    #endregion

    #region --- Shape Mesh Polygon ---
    public void SetShapeCollider(PolygonCollider2D _collider)
    {   // Set the collider points of the shape
        _collider.points = _points.ConvertAll(point => new Vector2(point.position.x, point.position.y)).ToArray();
    }

    public void CreatePolygonMesh(GameObject _shapeMesh)
    {   // Create a mesh from shape points
        _shapePolygonMesh = _shapeMesh;
        SetShapeCollider(_shapeMesh.GetComponent<PolygonCollider2D>());

        List<Triangulator.Triangle> _triangles = Triangulator.Triangulate(
            _points.ConvertAll(point => new Vector2(point.position.x, point.position.y)));
        List<Vector3> _vertices = new List<Vector3>();
        List<int> _indices = new List<int>();

        Triangulator.AddTrianglesToMesh(ref _vertices, ref _indices, _triangles, 0.01f, true);

        // Create the mesh
        Mesh _mesh = new Mesh();
        _mesh.vertices = _vertices.ToArray();
        _mesh.triangles = _indices.ToArray();
        _mesh.RecalculateNormals();
        _mesh.RecalculateBounds();
        _shapeMesh.GetComponent<MeshFilter>().mesh = _mesh;
    }
    #endregion

    #region --- Shape Mesh 3D ---
    public void GenerateShapeMesh(Transform _renderParent, Material _renderMaterial)
    {   // Create a 3D mesh from shape points
        GameObject _shape3D = Instantiate(_shapePolygonMesh, _renderParent);
        _shape3D.name = _shape3D.name.Replace("(Clone)", "").Replace("Polygon", "Mesh");
        _shape3D.transform.position += new Vector3(0, shape3DHeight, 0);
        _shape3D.transform.rotation = Quaternion.Euler(90, 0, 0);

        _shape3D.layer = LayerMask.NameToLayer("Obstacle");
        Destroy(_shape3D.GetComponent<PolygonCollider2D>());
        _shape3D.GetComponent<MeshRenderer>().material = _renderMaterial;

        // Rotate points to 3D
        List<Vector3> _polygonPoints = _points.ConvertAll(point => point.position);
        _polygonPoints = _polygonPoints.ConvertAll(point => new Vector3(point.x, point.z, point.y));

        // Add to the mesh the lateral faces
        for (int i = 0; i < _polygonPoints.Count; i++)
        {
            Vector3[] _facePoints = new Vector3[4];
            _facePoints[0] = _polygonPoints[i] + new Vector3(0, shape3DHeight, 0);
            _facePoints[1] = _polygonPoints[(i + 1) % _polygonPoints.Count] + new Vector3(0, shape3DHeight, 0);
            _facePoints[2] = _polygonPoints[(i + 1) % _polygonPoints.Count];
            _facePoints[3] = _polygonPoints[i];

            AddShapeFace(_shape3D, _facePoints);
        }
    }

    private void AddShapeFace(GameObject _shape3D, Vector3[] _facePoints)
    {   // Add a face to the 3D shape mesh
        GameObject _face = new GameObject("Face", typeof(MeshFilter), typeof(MeshRenderer));
        _face.transform.SetParent(_shape3D.transform);
        _face.transform.rotation = Quaternion.identity;
        _face.transform.localPosition = Vector3.zero;
        _face.transform.localScale = Vector3.one;

        // Two sided mesh to avoid culling
        Mesh _mesh = new Mesh();
        _mesh.vertices = _facePoints.Concat(_facePoints).ToArray();
        _mesh.triangles = new int[] {
            0, 1, 2, 0, 2, 3, // Face 1
            1, 0, 2, 2, 0, 3  // Face 2
        };
        _mesh.RecalculateNormals();
        _mesh.RecalculateBounds();

        _face.GetComponent<MeshFilter>().mesh = _mesh;
        _face.GetComponent<MeshRenderer>().material = _shape3D.GetComponent<MeshRenderer>().material;
    }
    #endregion
}
