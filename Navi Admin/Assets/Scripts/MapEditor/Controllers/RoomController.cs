using System;
using System.Collections;
using System.Collections.Generic;
using Game.Utils.Triangulation;
using Game.Utils.Math;
using UnityEngine;
using MapDataModel;
using System.Linq;

public class RoomController : MonoBehaviour
{
    public string roomName;
    public string roomType = "Room";
    public List<WallNodeController> nodes = new List<WallNodeController>();
    public List<WallLineController> walls = new List<WallLineController>();
    public Material colorMaterial;

    private RoomsManager _roomsManager;
    private PolygonCollider2D _polygonCollider;
    private MeshFilter _meshFilter;

    private void Awake()
    {
        _roomsManager = GameObject.Find("RoomsManager").GetComponent<RoomsManager>();
        colorMaterial = this.GetComponent<MeshRenderer>().material;
        _polygonCollider = this.GetComponent<PolygonCollider2D>();
        _meshFilter = this.GetComponent<MeshFilter>();

        // Set the default color of the polygon
        colorMaterial.SetColor("_Color1", new Color(0.5f, 0.5f, 0.5f, 0.5f));
        transform.position = new Vector3(0, 0, 0.1f);
    }
    public List<Vector2> GetPoints2D()
    {   // Get the 2D points of the polygon
        List<Vector2> _points = new List<Vector2>();
        nodes.ForEach(node => _points.Add(new Vector2(node.transform.position.x, node.transform.position.y)));
        return _points;
    }

    public Vector3 GetClosestDoor(Vector3 _initPos)
    {   // Get the closest room door from a initial position
        List<EntrancesController> _doors = new List<EntrancesController>();
        foreach (WallLineController _wall in walls)
            foreach (EntrancesController _door in _wall.entrances)
                _doors.Add(_door); // Add the doors to the list

        if (_doors.Count > 0)
        {   // Get the closest door
            EntrancesController _closestDoor = _doors[0];
            float _minDistance = Vector3.Distance(_initPos, _closestDoor.GetEntranceCenter(true));
            foreach (EntrancesController _door in _doors)
            {
                float _distance = Vector3.Distance(_initPos, _door.GetEntranceCenter(true));
                if (_distance < _minDistance)
                {
                    _minDistance = _distance;
                    _closestDoor = _door;
                }
            }
            return _closestDoor.GetEntranceCenter(true);
        }
        else return Vector3.zero;
    }
    public void DestroyRoom(WallNodeController _nodeCalled = null)
    {   // Destroy the room and remove it from references
        foreach (WallNodeController _node in nodes)
            if (!_node == _nodeCalled) _node.rooms.Remove(this);
        foreach (WallLineController _wall in walls) _wall.rooms.Remove(this);

        _roomsManager.rooms.Remove(this);
        Destroy(this.gameObject);
    }

    private void OnTriggerEnter2D(Collider2D _collision)
    {   // Check if the polygon is colliding with another polygon
        if (_collision.gameObject.tag == "Polygon")
        {   // If the polygon is bigger than the collided polygon, destroy it
            RoomController _polygon = _collision.gameObject.GetComponent<RoomController>();
            if (GetPolygonArea() > _polygon.GetPolygonArea())
            {
                // TODO: Manage the overlapping polygons and the holes

                DestroyRoom();
            }
        }
    }

    #region --- Polygon Operations ---
    public void SetPolygonCollider()
    {   // Set the polygon collider points
        Vector2[] _points = GetPoints2D().ToArray();
        Vector2 _centroid = GetPolygonCentroid();

        for (int i = 0; i < _points.Length; i++)
        {   // reduce the area of the collider to avoid overlapping
            _points[i] = Vector2.Lerp(_points[i], _centroid, 0.015f);
        }
        if (_polygonCollider == null)
            _polygonCollider = this.GetComponent<PolygonCollider2D>();
        _polygonCollider.points = _points;
    }

    public void CreatePolygonMesh()
    {   // Create a polygon mesh from connected points
        List<Vector2> _points2D = GetPoints2D();
        List<Triangle2D> _outputTriangles = new List<Triangle2D>();
        List<List<Vector2>> _constrainedPoints = new List<List<Vector2>> { _points2D };
        if (_meshFilter == null) _meshFilter = this.GetComponent<MeshFilter>();

        DelaunayTriangulation _triangulation = new DelaunayTriangulation();
        _triangulation.Triangulate(_points2D, 0.0f, _constrainedPoints);
        _triangulation.GetTrianglesDiscardingHoles(_outputTriangles);
        _meshFilter.mesh = CreateMeshFromTriangles(_outputTriangles);
        SetPolygonCollider();
    }

    private Mesh CreateMeshFromTriangles(List<Triangle2D> triangles)
    {   // Create a mesh from a list of triangles
        List<Vector3> vertices = new List<Vector3>(triangles.Count * 3);
        List<int> indices = new List<int>(triangles.Count * 3);

        for (int i = 0; i < triangles.Count; ++i)
        {
            vertices.Add(triangles[i].p0);
            vertices.Add(triangles[i].p1);
            vertices.Add(triangles[i].p2);
            indices.Add(i * 3 + 2); // Changes order
            indices.Add(i * 3 + 1);
            indices.Add(i * 3);
        }

        Mesh mesh = new Mesh();
        mesh.subMeshCount = 1;
        mesh.SetVertices(vertices);
        mesh.SetIndices(indices, MeshTopology.Triangles, 0);
        return mesh;
    }

    public float GetPolygonArea()
    {   // Get the area of the polygon
        List<Vector2> _points2D = GetPoints2D();
        float _area = 0;
        int j = _points2D.Count - 1;
        for (int i = 0; i < _points2D.Count; i++)
        {
            _area += (_points2D[j].x + _points2D[i].x) * (_points2D[j].y - _points2D[i].y);
            j = i;
        }
        return Mathf.Abs(_area / 2);
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
    #endregion

    #region --- Data managment ---
    public PolygonData GetPolygonData()
    {   // Get the polygon data
        PolygonData _polygonData = new PolygonData();
        _polygonData.materialColor = new SerializableColor(colorMaterial.GetColor("_Color1"));
        _polygonData.vertices = SerializableVector3.GetSerializableArray(_meshFilter.mesh.vertices);
        _polygonData.triangles = _meshFilter.mesh.triangles;
        return _polygonData;
    }

    public MeshData GetRenderData()
    {   // Get the mesh data of the polygon
        MeshData _meshData = new MeshData();
        _meshData.vertices = SerializableVector3.GetSerializableArray(
            _meshFilter.mesh.vertices.ToList().ConvertAll(v => Quaternion.Euler(90, 0, 0) * v).ToArray());
        _meshData.triangles = _meshFilter.mesh.triangles;
        return _meshData;
    }

    public void SetPolygonData(PolygonData _polygonData)
    {   // Set the polygon data
        colorMaterial.SetColor("_Color1", _polygonData.materialColor.GetColor);
        if (_meshFilter == null) _meshFilter = this.GetComponent<MeshFilter>();

        Mesh _mesh = new Mesh();
        _mesh.vertices = SerializableVector3.GetVector3Array(_polygonData.vertices);
        _mesh.triangles = _polygonData.triangles;
        _mesh.RecalculateNormals();
        _mesh.RecalculateBounds();
        _meshFilter.mesh = _mesh;
        SetPolygonCollider();
    }
    #endregion
}
