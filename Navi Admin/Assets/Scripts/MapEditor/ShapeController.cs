using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Haze;
using System.Linq;

public class ShapeController : MonoBehaviour
{
    private PolygonCollider2D _polygonCollider;
    private LineRenderer _lineRenderer;
    private MeshFilter _meshFilter;

    private List<Transform> _points;

    private void Awake()
    {
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

    public void UpdateLastPoint(Vector3 position)
    {   // Update the last point of the line
        if (_points.Count > 0)
        {
            _lineRenderer.SetPosition(_points.Count - 1, position);
            _points[_points.Count - 1].position = position;
        }
    }

    public void EndShape()
    {   // Close the shape
        if (_points.Count > 2)
            _lineRenderer.loop = true;
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

    public void SetShapeCollider(PolygonCollider2D _collider)
    {   // Set the collider points of the shape
        _collider.points = _points.ConvertAll(point => new Vector2(point.position.x, point.position.y)).ToArray();
    }

    public void CreateShapeMesh(GameObject _shapeMesh)
    {   // Create a mesh from shape points
        _meshFilter = _shapeMesh.GetComponent<MeshFilter>();
        _polygonCollider = _shapeMesh.GetComponent<PolygonCollider2D>();
        SetShapeCollider(_polygonCollider);

        List<Triangulator.Triangle> _triangles = Triangulator.Triangulate(_polygonCollider.points.ToList());
        List<Vector3> _vertices = new List<Vector3>();
        List<int> _indices = new List<int>();

        Triangulator.AddTrianglesToMesh(ref _vertices, ref _indices, _triangles, 0.01f, true);

        // Create the mesh
        Mesh _mesh = new Mesh();
        _mesh.vertices = _vertices.ToArray();
        _mesh.triangles = _indices.ToArray();
        _mesh.RecalculateNormals();
        _mesh.RecalculateBounds();
        _meshFilter.mesh = _mesh;
    }

}
