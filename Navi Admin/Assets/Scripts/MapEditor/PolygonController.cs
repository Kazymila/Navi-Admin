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

    [Header("Required Components")]
    [SerializeField] private PolygonCollider2D _polygonCollider;
    [SerializeField] private MeshFilter _meshFilter;
    public Material colorMaterial;

    private void Start()
    {
        colorMaterial = this.GetComponent<MeshRenderer>().material;
        _polygonCollider = this.GetComponent<PolygonCollider2D>();
        _meshFilter = this.GetComponent<MeshFilter>();

        colorMaterial.SetColor("_Color1", new Color(0.5f, 0.5f, 0.5f, 0.5f));
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

    public Vector3 GetPolygonCenter(bool _3Dpolygon = false)
    {   // Get the center of the polygon
        Vector3 _center = Vector3.zero;
        nodes.ForEach(node => _center += node.transform.position);
        if (_3Dpolygon) _center = Quaternion.Euler(90, 0, 0) * _center;
        _center /= nodes.Count;
        return _center;
    }
}
