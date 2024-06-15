using System;
using System.Collections;
using System.Collections.Generic;
using Habrador_Computational_Geometry;
using UnityEngine;
using MapDataModel;
using System.Linq;

public class RoomController : MonoBehaviour
{
    public bool destroyRoom;
    public bool isLocked;
    public TranslatedText roomName;
    public string roomType = "";
    public List<WallNodeController> nodes = new List<WallNodeController>();
    public List<WallLineController> walls = new List<WallLineController>();
    public Material colorMaterial;

    private RoomsManager _roomsManager;
    private PolygonCollider2D _polygonCollider;
    private MeshFilter _meshFilter;

    private List<RoomController> _intersectPolygons = new List<RoomController>();

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

    private void Update()
    {   // Destroy room on editor
        if (destroyRoom) DestroyRoom();
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

    #region --- Polygon Operations ---
    private void OnTriggerEnter2D(Collider2D _collision)
    {   // Check if the polygon is colliding with another polygon
        if (_collision.gameObject.tag == "Polygon" && !isLocked)
        {   // If the polygon is bigger than the collided polygon, destroy it
            RoomController _polygon = _collision.gameObject.GetComponent<RoomController>();

            if (GetPolygonArea() > _polygon.GetPolygonArea() && !_intersectPolygons.Contains(_polygon))
            {   // Manage the overlapping polygons and the holes

                // Move the smaller polygon above, to avoid overlapping
                _polygon.transform.localPosition = new Vector3(
                    _polygon.transform.localPosition.x, _polygon.transform.localPosition.y, -0.05f);

                //GeneratePolygonHoles();

                // TODO: Delete overlapping polygons
                //DestroyRoom();
            }
        }
    }

    public void SetPolygonCollider()
    {   // Set the polygon collider points
        Vector2[] _points = GetPoints2D().ToArray();
        if (_meshFilter != null)
        {   // Reduce the area of the collider to avoid overlapping
            Vector2 _centroid = GetPolygonCenter();
            for (int i = 0; i < _points.Length; i++)
                _points[i] = Vector2.Lerp(_points[i], _centroid, 0.005f);
        }
        if (_polygonCollider == null)
            _polygonCollider = this.GetComponent<PolygonCollider2D>();
        _polygonCollider.points = _points;
    }

    public void GeneratePolygonHoles()
    {   // Generate the holes of the polygon
        List<Vector2[]> _holesPoints = new List<Vector2[]>();
        foreach (RoomController polygon in _intersectPolygons)
            _holesPoints.Add(polygon.GetPoints2D().ToArray());
        CreatePolygonMesh(_holesPoints);

        // Discard the nodes that are not part of the polygon
        foreach (WallNodeController node in nodes)
        {   // Check if the node is inside the polygon
            Vector3 _nodePos = node.transform.position + new Vector3(0, 0, 0.5f);
            if (!_meshFilter.mesh.bounds.Contains(_nodePos))
            {   // Remove the node from the room
                node.rooms.Remove(this);
                nodes.Remove(node);
            }
        }
    }

    public void CreatePolygonMesh(List<Vector2[]> _holePoints = null)
    {   // Create the room polygon mesh
        if (isLocked) return;
        Mesh _outerArea = GenerateTriangulateMesh(_holePoints, true);
        Vector2[] _outerPoints = _outerArea.vertices.ToList().ConvertAll(v => new Vector2(v.x, v.y)).ToArray();

        if (_outerPoints.Length > 0)
        {
            if (_holePoints == null) _holePoints = new List<Vector2[]> { _outerPoints };
            else _holePoints.Add(_outerPoints);
        }
        else if (_holePoints == null) _holePoints = new List<Vector2[]> { };

        Mesh _mesh = GenerateTriangulateMesh(_holePoints);
        _meshFilter.mesh = _mesh;

        // If a void polygon is created, destroy it
        if (_meshFilter.mesh.vertices.Length == 0)
            Destroy(this.gameObject);

        SetPolygonCollider();
    }

    public Mesh GenerateTriangulateMesh(List<Vector2[]> _holePoints = null, bool _getOuterArea = false)
    {   // Create a polygon mesh using Constrained Delaunay triangulation
        HashSet<MyVector2> points = GetPoints2D().Select(v => new MyVector2(v.x, v.y)).ToHashSet();

        // Hull points
        List<MyVector2> hullPoints_2d = _ConvexHull.JarvisMarch_2D(points);

        // Holes points
        if (_holePoints == null) _holePoints = new List<Vector2[]> { };
        HashSet<List<MyVector2>> allHolePoints_2d = new HashSet<List<MyVector2>>();
        foreach (Vector2[] hole in _holePoints)
        {
            List<MyVector2> holePoints = hole.Select(v => new MyVector2(v.x, v.y)).ToList();
            allHolePoints_2d.Add(holePoints);
        }
        if (_getOuterArea) allHolePoints_2d.Add(points.ToList());

        // Normalize to range 0-1
        List<MyVector2> allPoints = new List<MyVector2>();
        allPoints.AddRange(hullPoints_2d);
        allPoints.AddRange(points);
        foreach (List<MyVector2> hole in allHolePoints_2d) allPoints.AddRange(hole);

        Normalizer2 normalizer = new Normalizer2(allPoints);
        List<MyVector2> hullPoints_2d_normalized = normalizer.Normalize(hullPoints_2d);
        HashSet<List<MyVector2>> holePoints_2d_normalized = new HashSet<List<MyVector2>>();
        HashSet<MyVector2> points_normalized = normalizer.Normalize(points);

        foreach (List<MyVector2> hole in allHolePoints_2d)
        {   // Normalize the hole points
            List<MyVector2> hole_normalized = normalizer.Normalize(hole);
            holePoints_2d_normalized.Add(hole_normalized);
        }

        // Generate the triangulation
        HalfEdgeData2 triangleData_normalized = _Delaunay.ConstrainedBySloan(
            points_normalized, hullPoints_2d_normalized, holePoints_2d_normalized,
            shouldRemoveTriangles: true, new HalfEdgeData2());

        // UnNormalize and get the triangles
        HalfEdgeData2 triangleData = normalizer.UnNormalize(triangleData_normalized);
        HashSet<Triangle2> triangles_2d = _TransformBetweenDataStructures.HalfEdge2ToTriangle2(triangleData);

        // Make sure the triangles have the correct orientation
        triangles_2d = HelpMethods.OrientTrianglesClockwise(triangles_2d);

        // Create the mesh
        _meshFilter = this.GetComponent<MeshFilter>();
        Mesh _mesh = _TransformBetweenDataStructures.Triangles2ToMesh(triangles_2d, true);

        // Rotate mesh points
        Vector3[] vertices = _mesh.vertices;
        for (int i = 0; i < vertices.Length; i++)
            vertices[i] = Quaternion.Euler(-90, 0, 0) * vertices[i];
        _mesh.vertices = vertices;

        //_meshFilter.mesh = _mesh;
        //SetPolygonCollider();
        return _mesh;
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

    public Vector3 GetPolygonCenter(bool _3Dpolygon = false)
    {   // Get the center of the polygon (2D or 3D)
        Vector3 _centroid = _meshFilter.mesh.bounds.center;
        if (_3Dpolygon) return Quaternion.Euler(90, 0, 0) * _centroid;
        else return _centroid;
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

    public bool CheckWallBelongsToRoom(WallLineController wallData)
    {   // Check if wall belongs to the room
        int nodeCount = 0;
        foreach (WallNodeController node in nodes)
        {   // Check if wall has start and end node in the room
            if (node == wallData.startNode) nodeCount++;
            if (node == wallData.endNode) nodeCount++;
        }
        // Wall does not belong to the room, if both nodes are not in the room
        if (nodeCount < 2) return false;
        return true;
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
        //_mesh.RecalculateNormals();
        //_mesh.RecalculateBounds();
        _meshFilter.mesh = _mesh;
        SetPolygonCollider();
    }
    #endregion
}
