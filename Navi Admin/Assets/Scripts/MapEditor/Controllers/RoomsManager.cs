using System.Collections.Generic;
using GraphPlanarityTesting.Graphs.DataStructures;
using GraphPlanarityTesting.PlanarityTesting.BoyerMyrvold;
using MapDataModel;
using UnityEngine;
using TMPro;

public class RoomsManager : MonoBehaviour
{
    #region --- Variables ---
    public List<RoomController> rooms = new List<RoomController>();

    [Header("Polygons Elements")]
    [SerializeField] private Transform _2DPolygonsParent;
    [SerializeField] private Transform _3DPolygonsParent;
    [SerializeField] private GameObject _2DPolygonPrefab;
    [SerializeField] private GameObject _3DPolygonPrefab;

    [Header("Graph Elements")]
    [SerializeField] private GameObject _nodesParent;
    [SerializeField] private GameObject _wallsParent;

    [Header("UI Elements")]
    [SerializeField] private GameObject _textlabelPrefab;
    [SerializeField] private Transform _labelsParent;
    private int _polygonsCount = 0;
    #endregion

    public void UpdatePolygons()
    {   // Update the polygons meshes and colliders
        rooms.ForEach(polygon => polygon.CreatePolygonMesh());
    }

    public void DestroyPolygon(RoomController _polygon)
    {   // Destroy the given polygon and remove it from references
        foreach (WallNodeController _node in _polygon.nodes)
            _node.rooms.Remove(_polygon);

        foreach (WallLineController _wall in _polygon.walls)
            _wall.rooms.Remove(_polygon);

        rooms.Remove(_polygon);
        Destroy(_polygon.gameObject);
    }

    #region -- Rooms Data Management --
    public RoomRenderData[] GetRoomsRenderData()
    {   // Get the rooms render data to save it
        RoomRenderData[] _roomsRenderData = new RoomRenderData[rooms.Count];

        for (int i = 0; i < rooms.Count; i++)
        {   // Get the data of each room
            RoomRenderData _roomData = new RoomRenderData
            {
                roomID = i,
                roomName = rooms[i].roomName,
                polygonColor = new SerializableColor(rooms[i].colorMaterial.GetColor("_Color1")),
                vertices = SerializableVector3.GetSerializableArray(rooms[i].polygonVertices),
                triangles = rooms[i].polygonTriangles,

                //entrancePoints = TODO: Add the entrance points
            };
            _roomsRenderData[i] = _roomData;
        }
        return _roomsRenderData;
    }
    #endregion

    #region --- Generate Rooms/Polygons ---
    public void GenerateRooms()
    {   // Generate the rooms from the graph
        List<List<int>> _graphFaces = GetGraphFaces();
        PrintGraphFaces(_graphFaces);

        if (rooms.Count == 0) // If there are no rooms, create them
            foreach (List<int> _face in _graphFaces) CreatePolygon(_face);
        else
        {   // Check if the polygons already exist in the graph
            for (int i = 0; i < _graphFaces.Count; i++)
            {
                List<WallNodeController> _nodes = new List<WallNodeController>();
                foreach (int _node in _graphFaces[i])
                    _nodes.Add(_nodesParent.transform.GetChild(_node).GetComponent<WallNodeController>());

                // Check if the rooms already exists
                bool _polygonExists = false;
                foreach (RoomController _polygon in rooms)
                {
                    if (new HashSet<WallNodeController>(_nodes).SetEquals(_polygon.nodes))
                    {
                        _polygonExists = true;
                        break;
                    }
                }   // If the room does not exist, create it
                if (!_polygonExists) CreatePolygon(_graphFaces[i]);
            }
        }
    }

    public void Generate3DPolygons()
    {   // Generate the 3D polygons from the 2D polygons
        foreach (Transform _3Dpolygon in _3DPolygonsParent) Destroy(_3Dpolygon.gameObject);

        foreach (RoomController _polygon in rooms)
        {
            GameObject _3Dpolygon = Instantiate(_3DPolygonPrefab, Vector3.zero, Quaternion.Euler(90, 0, 0), _3DPolygonsParent);
            _3Dpolygon.GetComponent<MeshCollider>().sharedMesh = _polygon.GetComponent<MeshFilter>().mesh;
            _3Dpolygon.GetComponent<MeshFilter>().mesh = _polygon.GetComponent<MeshFilter>().mesh;
            _3Dpolygon.GetComponent<MeshRenderer>().material = _polygon.colorMaterial;
            _3Dpolygon.name = _polygon.roomName;
        }
    }

    public void CreatePolygon(List<int> _faceNodes)
    {   // Create a polygon from the given cycle nodes
        GameObject _polygon = Instantiate(_2DPolygonPrefab, Vector3.zero, Quaternion.identity, _2DPolygonsParent);
        RoomController _polygonController = _polygon.GetComponent<RoomController>();

        _faceNodes.ForEach(nodeIndex =>
        {   // Add the nodes to the polygon and the polygon to the nodes
            WallNodeController _dot = _nodesParent.transform.GetChild(nodeIndex).GetComponent<WallNodeController>();
            _dot.rooms.Add(_polygonController);
            _polygonController.nodes.Add(_dot);
        });
        _polygon.name = "Polygon_" + _polygonsCount;
        _polygonController.roomName = "Room_" + _polygonsCount;
        _polygonController.CreatePolygonMesh();
        rooms.Add(_polygonController);
        _polygonsCount++;
    }
    #endregion

    #region --- Graph planarity ---
    private List<List<int>> GetGraphFaces()
    {   // Get the faces of the graph
        UndirectedAdjacencyListGraph<int> _graph = GenerateAdjacentList();
        BoyerMyrvold<int> boyerMyrvold = new BoyerMyrvold<int>();

        var _faces = boyerMyrvold.TryGetPlanarFaces(_graph, out var planarFaces);

        if (_faces)
        {   // If the graph is planar, return the faces (with no duplicates)
            List<List<int>> _uniqueFaces = new List<List<int>>();
            foreach (var face in planarFaces.Faces)
            {
                bool _faceEqual = false;
                foreach (var anotherFace in _uniqueFaces)
                {
                    if (face == anotherFace) continue;
                    if (new HashSet<int>(face).SetEquals(anotherFace))
                    {
                        _faceEqual = true;
                        break;
                    }
                }
                if (!_faceEqual) _uniqueFaces.Add(face);
            }
            return _uniqueFaces;
        }
        else return null; // If the graph is not planar, return null
    }

    private void PrintGraphFaces(List<List<int>> _faces)
    {   // Print the faces of the graph
        foreach (var face in _faces)
        {
            string _face = "";
            face.ForEach(node => _face += node + ", ");
            print("Face: " + _face);
        }
    }

    private UndirectedAdjacencyListGraph<int> GenerateAdjacentList()
    {   // Generate the adjacent list of the map graph
        var _graph = new UndirectedAdjacencyListGraph<int>();

        foreach (Transform node in _nodesParent.transform)
        {   // Add the vertices to the graph
            _graph.AddVertex(node.GetSiblingIndex());
        }
        foreach (Transform wall in _wallsParent.transform)
        {   // Add the edges to the graph
            WallLineController _wall = wall.GetComponent<WallLineController>();
            int source = _wall.startNode.transform.GetSiblingIndex();
            int target = _wall.endNode.transform.GetSiblingIndex();
            _graph.AddEdge(source, target);
        }
        return _graph;
    }
    #endregion

    #region --- UI Elements ---
    public void ShowRoomsLabels()
    {   // Show the labels of the polygons
        foreach (RoomController _polygon in rooms)
        {
            GameObject _label = Instantiate(_textlabelPrefab, Vector3.zero, Quaternion.identity, _labelsParent);
            _label.transform.position = Camera.main.WorldToScreenPoint(_polygon.GetPolygonCentroid());
            _label.GetComponent<TextMeshProUGUI>().text = _polygon.roomName;
        }
    }
    public void UpdateLabelsPosition()
    {   // Update the position of the labels to follow the camera
        for (int i = 0; i < _labelsParent.childCount; i++)
        {
            if (i >= rooms.Count) break;
            Transform _label = _labelsParent.GetChild(i);
            _label.position = Camera.main.WorldToScreenPoint(rooms[i].GetPolygonCentroid());
        }
    }
    public void RemoveRoomsLabels()
    {   // Remove the labels of the polygons
        foreach (Transform _label in _labelsParent)
            Destroy(_label.gameObject);
    }
    #endregion
}
