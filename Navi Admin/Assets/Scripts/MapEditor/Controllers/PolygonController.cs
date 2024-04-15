using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Haze;

public class PolygonController : MonoBehaviour
{
    public string polygonLabel;
    public List<WallDotController> nodes = new List<WallDotController>();
    [SerializeField] private List<Vector2> _polygonPoints2D;

    private PolygonCollider2D _polygonCollider;
    private MeshFilter _meshFilter;
    public Material colorMaterial;

    private void Awake()
    {
        colorMaterial = this.GetComponent<MeshRenderer>().material;
        _polygonCollider = this.GetComponent<PolygonCollider2D>();
        _meshFilter = this.GetComponent<MeshFilter>();

        colorMaterial.SetColor("_Color1", new Color(0.5f, 0.5f, 0.5f, 0.5f));
        transform.position = new Vector3(0, 0, 0.1f);
    }
    public List<Vector2> GetPoints2D()
    {   // Get the 2D points of the polygon
        List<Vector2> _points = new List<Vector2>();
        nodes.ForEach(node => _points.Add(new Vector2(node.transform.position.x, node.transform.position.y)));
        _polygonPoints2D = _points;
        return _points;
    }

    public void SetPolygonCollider()
    {   // Set the polygon collider points
        _polygonCollider.points = _polygonPoints2D.ToArray();
    }

    public void CreatePolygonMesh()
    {   // Create a polygon mesh from connected points
        List<Vector2> _points2D = GetPoints2D();

        // Triangulate the 2D points
        List<Triangulator.Triangle> _triangles = Triangulator.Triangulate(_points2D);
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

        SetPolygonCollider();
    }

    public Vector3 GetPolygonCentroid(bool _3Dpolygon = false)
    {   // Get the centroid of the polygon
        List<Vector2> _points2D = GetPoints2D();
        if (_points2D.Count <= 4)
        {   // If the polygon has less than 4 points, use the mesh center
            Vector3 _centroid = _meshFilter.mesh.bounds.center;
            if (_3Dpolygon) return Quaternion.Euler(90, 0, 0) * _centroid;
            else return _centroid;
        }
        Vector3 centroid = Vector3.zero;
        float signedArea = 0.0f;
        for (int i = 0; i < _points2D.Count; i++)
        {
            int nextIndex = (i + 1) % _points2D.Count;
            float a = _points2D[i].x * _points2D[nextIndex].y - _points2D[nextIndex].x * _points2D[i].y;
            signedArea += a;
            centroid.x += (_points2D[i].x + _points2D[nextIndex].x) * a;
            centroid.y += (_points2D[i].y + _points2D[nextIndex].y) * a;
        }
        signedArea *= 0.3f;
        centroid.x /= (6.0f * signedArea);
        centroid.y /= (6.0f * signedArea);
        if (_3Dpolygon) centroid = Quaternion.Euler(90, 0, 0) * centroid;
        return centroid;
    }
}
