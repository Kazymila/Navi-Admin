using System.Collections.Generic;
using GraphPlanarityTesting.Graphs.DataStructures;
using GraphPlanarityTesting.PlanarityTesting.BoyerMyrvold;
using MapDataModel;
using UnityEngine;
using TMPro;
using System.Linq;

public class RoomsManager : MonoBehaviour
{
    #region --- Variables ---
    public List<RoomController> rooms = new List<RoomController>();
    public List<string> roomsTypesList = new List<string>();
    public List<RoomTypeData> roomsTypesData = new List<RoomTypeData>();

    [Header("Polygons Elements")]
    [SerializeField] private Transform _2DPolygonsParent;
    [SerializeField] private Transform _3DPolygonsParent;
    [SerializeField] private GameObject _2DPolygonPrefab;
    [SerializeField] private GameObject _3DPolygonPrefab;

    [Header("Graph Elements")]
    [SerializeField] private GameObject _nodesParent;
    [SerializeField] private GameObject _wallsParent;

    [Header("UI Elements")]
    [SerializeField] private TMP_Dropdown _roomsTypesDropdown;
    [SerializeField] private GameObject _textlabelPrefab;
    [SerializeField] private Transform _labelsParent;
    #endregion

    private void Update()
    {   // Remove the null rooms
        rooms.RemoveAll(room => room == null);
    }

    public void UpdatePolygons()
    {   // Update the polygons meshes and colliders
        rooms.ForEach(polygon => polygon.CreatePolygonMesh());
    }

    #region --- Rooms Types Dropdown ---
    private void Awake()
    {   // Setup the rooms types dropdown
        roomsTypesList.Add("Unique");
        roomsTypesData.Add(new RoomTypeData
        {
            typeID = 0,
            searchNearestMode = false,
            typeName = new TranslatedText
            {
                key = "Unique",
                englishTranslation = "Unique",
                spanishTranslation = "Único"
            },
        });
        SetRoomsTypesDropdown();
    }

    public void SetRoomsTypesDropdown()
    {   // Set the dropdown to the rooms types dropdown
        _roomsTypesDropdown.ClearOptions();
        _roomsTypesDropdown.AddOptions(roomsTypesList);
        _roomsTypesDropdown.options.Add(new TMP_Dropdown.OptionData("+ New type"));
    }

    public void AddRoomType(string _roomType, string _eng, string _esp, bool _nearestMode)
    {   // Add a new room type or update a existing one
        if (roomsTypesList.Contains(_roomType))
        {   // Update the room type data
            RoomTypeData _typeData = new RoomTypeData
            {
                typeID = roomsTypesList.Count - 1,
                searchNearestMode = _nearestMode,
                typeName = new TranslatedText
                {
                    key = _roomType,
                    englishTranslation = _eng,
                    spanishTranslation = _esp
                }
            };
            roomsTypesData[roomsTypesList.IndexOf(_roomType)] = _typeData;
        }
        else
        {   // Add the new room type
            roomsTypesList.Add(_roomType);
            RoomTypeData _typeData = new RoomTypeData
            {
                typeID = roomsTypesList.Count - 1,
                searchNearestMode = _nearestMode,
                typeName = new TranslatedText
                {
                    key = _roomType,
                    englishTranslation = _eng,
                    spanishTranslation = _esp
                }
            };
            SetRoomsTypesDropdown();
            roomsTypesData.Add(_typeData);
            _roomsTypesDropdown.value = roomsTypesList.Count - 1;
        }
    }
    #endregion

    #region --- Generate Rooms/Polygons ---
    public void GenerateRooms()
    {   // Generate the rooms from the graph
        if (_nodesParent.transform.childCount < 3) return; // If there are no nodes, return
        List<List<int>> _graphFaces = GetGraphFaces();
        //PrintGraphFaces(_graphFaces);

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

        for (int i = 0; i < _2DPolygonsParent.childCount; i++)
        {   // Create the 3D polygons from the 2D polygons
            RoomController _polygon = _2DPolygonsParent.GetChild(i).GetComponent<RoomController>();
            if (_polygon.nodes.Count < 3) continue; // Skip the polygon if it has less than 3 nodes

            GameObject _3Dpolygon = Instantiate(_3DPolygonPrefab, Vector3.zero, Quaternion.Euler(90, 0, 0), _3DPolygonsParent);
            _3Dpolygon.GetComponent<MeshCollider>().sharedMesh = _polygon.GetComponent<MeshFilter>().mesh;
            _3Dpolygon.GetComponent<MeshFilter>().mesh = _polygon.GetComponent<MeshFilter>().mesh;
            _3Dpolygon.GetComponent<MeshRenderer>().material = _polygon.colorMaterial;
            _3Dpolygon.name = "RoomRender_" + i;
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
        _polygon.name = "Room_" + (rooms.Count > 0 ? (rooms.Count - 1) : 0);
        _polygonController.roomName.key = _polygon.name;
        _polygonController.CreatePolygonMesh();
        rooms.Add(_polygonController);
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
            _label.transform.position = Camera.main.WorldToScreenPoint(_polygon.GetPolygonCenter());
            _label.GetComponent<TextMeshProUGUI>().text = _polygon.roomName.key;
        }
    }
    public void UpdateLabelsPosition()
    {   // Update the position of the labels to follow the camera
        for (int i = 0; i < _labelsParent.childCount; i++)
        {
            if (i >= rooms.Count) break;
            Transform _label = _labelsParent.GetChild(i);
            _label.position = Camera.main.WorldToScreenPoint(rooms[i].GetPolygonCenter());
        }
    }
    public void RemoveRoomsLabels()
    {   // Remove the labels of the polygons
        foreach (Transform _label in _labelsParent)
            Destroy(_label.gameObject);
    }
    #endregion

    #region -- Rooms Data Management --
    public RoomData[] GetRoomsData()
    {   // Get the rooms data to save it
        RoomData[] _roomsData = new RoomData[rooms.Count];

        for (int i = 0; i < rooms.Count; i++)
        {   // Get the data of each room
            RoomData _data = new RoomData
            {
                roomID = i,
                roomName = rooms[i].roomName,
                roomType = roomsTypesList.IndexOf(rooms[i].roomType),
                nodes = rooms[i].nodes.ConvertAll(node => node.transform.GetSiblingIndex()).ToArray(),
                walls = rooms[i].walls.ConvertAll(wall => wall.transform.GetSiblingIndex()).ToArray(),
                polygonData = rooms[i].GetPolygonData(),
                renderData = rooms[i].GetRenderData(),
            };
            _roomsData[i] = _data;
        }
        return _roomsData;
    }

    public void LoadRoomsData(RoomData[] _roomsData, RoomTypeData[] _roomsTypes)
    {   // Load the rooms data to the map
        ClearRooms();
        roomsTypesData = _roomsTypes.ToList();

        foreach (RoomTypeData _type in _roomsTypes)
            roomsTypesList.Add(_type.typeName.key);

        SetRoomsTypesDropdown();

        foreach (RoomData _roomData in _roomsData)
        {   // Create the room from the data
            GameObject _polygon = Instantiate(_2DPolygonPrefab, Vector3.zero, Quaternion.identity, _2DPolygonsParent);
            RoomController _polygonController = _polygon.GetComponent<RoomController>();

            _polygonController.roomName = _roomData.roomName;
            _polygonController.roomType = roomsTypesList[_roomData.roomType];

            foreach (int _nodeIndex in _roomData.nodes)
            {   // Add the nodes to the polygon and the polygon to the nodes
                WallNodeController _node = _nodesParent.transform.GetChild(_nodeIndex).GetComponent<WallNodeController>();
                _node.rooms.Add(_polygonController);
                _polygonController.nodes.Add(_node);
            }
            foreach (int _wallIndex in _roomData.walls)
            {   // Add the walls to the polygon and the polygon to the walls
                WallLineController _wall = _wallsParent.transform.GetChild(_wallIndex).GetComponent<WallLineController>();
                _wall.rooms.Add(_polygonController);
                _polygonController.walls.Add(_wall);
            }
            _polygonController.SetPolygonData(_roomData.polygonData);
            rooms.Add(_polygonController);
        }
    }

    public void ClearRooms()
    {   // Clear the rooms data
        foreach (RoomController _room in rooms)
            Destroy(_room.gameObject);
        roomsTypesList.Clear();
        roomsTypesData.Clear();
        rooms.Clear();
    }
    #endregion
}
