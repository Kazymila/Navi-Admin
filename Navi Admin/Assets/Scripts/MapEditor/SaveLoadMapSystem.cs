using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MapDataModel;
using Firebase.Firestore;
using Firebase.Extensions;
using SFB;

public class SaveLoadMapSystem : MonoBehaviour
{
    #region --- Variables ---
    [Header("UI")]
    [SerializeField] private EditorLayoutController _editorLayoutController;
    [SerializeField] private GameObject _SaveLoadPanel;
    [Header("Managers")]
    [SerializeField] private RoomsManager _roomsManager;
    [SerializeField] private QRCodesManager _qrCodesManager;
    [SerializeField] private Render3DManager _render3DManager;

    [Header("Parents")]
    [SerializeField] private Transform _nodesParent;
    [SerializeField] private Transform _wallsParent;
    [SerializeField] private Transform _entrancesParent;
    [SerializeField] private Transform _shapesParent;

    [Header("Prefabs")]
    [SerializeField] private GameObject _nodePrefab;
    [SerializeField] private GameObject _wallPrefab;
    [SerializeField] private GameObject _entrancePrefab;
    [SerializeField] private GameObject _shapePrefab;
    #endregion

    private FirebaseFirestore _databaseRef;

    private void Awake()
    {   // Initialize the database reference
        _databaseRef = FirebaseFirestore.DefaultInstance;
    }

    ExtensionFilter[] _extensionList = new[] {
        new ExtensionFilter("JSON Files", "json"),
        new ExtensionFilter("All Files", "*" )
        }; // Extension filters for file window

    public void SaveMapData()
    {   // Save the map data to a JSON file
        MapData _mapData = GetMapData();
        string _path = StandaloneFileBrowser.SaveFilePanel("Save Map", "", _mapData.mapName + "MapData", _extensionList);
        if (_path == "") return; // If the path is empty, return

        JsonDataService.SaveData(_path, _mapData);
        _SaveLoadPanel.SetActive(false); // Close the save/load panel
    }

    public void LoadMapData()
    {   // Load the map data from a JSON file

        // TODO: Pop-up message to save or discard the current map

        // Disable all the features and tools and clear the map
        _editorLayoutController.DisableSelectedButton();
        ClearMapData();

        string[] _paths = StandaloneFileBrowser.OpenFilePanel("Load Map", "", _extensionList, false);
        string _path = _paths.Length > 0 ? _paths[0] : "";
        if (_path == "") return; // If the path is empty, return

        MapData _mapData = JsonDataService.LoadData<MapData>(_path);
        LoadFloorData(_mapData, 0); // Load the first floor map

        _SaveLoadPanel.SetActive(false); // Close the save/load panel
    }

    public void SaveMapToDatabase()
    {   // Save the map data to the database in server
        MapData _mapData = GetMapData();
        string _mapDataJson = JsonDataService.ToJson(_mapData);

        Dictionary<string, object> _mapDataDict = new Dictionary<string, object>
        {
            { "MapData", _mapDataJson }
        };

        DocumentReference _docRef = _databaseRef.Collection("MapData").Document("ExampleMap");
        _docRef.SetAsync(_mapDataDict).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
                Debug.Log("Map data saved to the database");
            else
                Debug.LogError("Error saving map data to the database");
        });
    }

    public void ClearMapData()
    {   // Clear the map data from the map editor
        _roomsManager.ClearRooms();

        foreach (Transform _node in _nodesParent)
            _node.GetComponent<WallNodeController>().DeleteNode(true);

        foreach (Transform _shape in _shapesParent)
            _shape.GetComponent<ShapeController>().DestroyShape();

        _qrCodesManager.ClearQRCodes();
    }

    #region --- Generate Map Data ---
    // -------------------------------------------
    // ------------ Generate Map Data ------------
    // -------------------------------------------
    public MapData GetMapData()
    {   // Get the map data from the map editor
        _render3DManager.ShowRenderElements(false); // Disable the 3D render
        MapData _mapData = new MapData
        {
            mapName = "ExampleMap",           // Default map name
            buildingName = "ExampleBuilding", // Default building name
            floors = new FloorData[] {
                new FloorData
                {
                    floorLevel = 0,          // Default floor level
                    floorName = "Floor 0",   // Default floor name
                    nodes = GetNodesData(),
                    walls = GetWallsData(),
                    rooms = _roomsManager.GetRoomsData(),
                    shapes = GetShapesData(),
                    qrCodes = _qrCodesManager.GetQRCodesData(),
                    events = new EventData[] { }
                }
            }
        };
        return _mapData;
    }

    public NodeData[] GetNodesData()
    {   // Get the nodes data from the map editor
        NodeData[] _nodesData = new NodeData[_nodesParent.childCount];
        for (int i = 0; i < _nodesParent.childCount; i++)
        {
            WallNodeController _node = _nodesParent.GetChild(i).GetComponent<WallNodeController>();
            NodeData _nodeData = new NodeData
            {
                nodeID = i,
                nodePosition = new SerializableVector3(_node.GetNodePosition()),
                neighborsNodes = _node.neighborsNodes.ConvertAll(node => node.transform.GetSiblingIndex()).ToArray(),
                walls = _node.walls.ConvertAll(wall => wall.transform.GetSiblingIndex()).ToArray(),
                rooms = _node.rooms.ConvertAll(room => room.transform.GetSiblingIndex()).ToArray()
            };
            _nodesData[i] = _nodeData;
        };
        return _nodesData;
    }

    public WallData[] GetWallsData()
    {   // Get the walls data from the map editor
        WallData[] _wallsData = new WallData[_wallsParent.childCount];
        for (int i = 0; i < _wallsParent.childCount; i++)
        {
            WallLineController _wall = _wallsParent.GetChild(i).GetComponent<WallLineController>();
            WallData _wallData = new WallData
            {
                wallID = i,
                wallLenght = _wall.length,
                wallWidth = _wall.width,
                startNode = _wall.startNode.transform.GetSiblingIndex(),
                endNode = _wall.endNode.transform.GetSiblingIndex(),
                wallPosition = new SerializableVector3(_wall.transform.position),
                rooms = _wall.rooms.ConvertAll(room => room.transform.GetSiblingIndex()).ToArray(),
                entrances = GetEntrancesData(_wall.entrances),
                renderData = _wall.GetWallRenderData()
            };
            _wallsData[i] = _wallData;
        };
        return _wallsData;
    }

    public EntranceData[] GetEntrancesData(List<EntrancesController> _entrances)
    {   // Get the entrances data from the map editor
        EntranceData[] _entrancesData = new EntranceData[_entrances.Count];
        for (int i = 0; i < _entrances.Count; i++)
        {
            EntrancesController _entrance = _entrances[i];
            EntranceData _entranceData = new EntranceData
            {
                entranceID = _entrance.transform.GetSiblingIndex(),
                entranceLenght = _entrance.lenght,
                entrancePosition = new SerializableVector3(_entrance.gameObject.transform.position),
                startNodePosition = new SerializableVector3(_entrance.startDot.transform.position),
                endNodePosition = new SerializableVector3(_entrance.endDot.transform.position)
            };
            _entrancesData[i] = _entranceData;
        };
        return _entrancesData;
    }

    public ShapeData[] GetShapesData()
    {   // Get the shapes data from the map editor
        ShapeData[] _shapesData = new ShapeData[_shapesParent.childCount];
        for (int i = 0; i < _shapesParent.childCount; i++)
        {
            ShapeController _shape = _shapesParent.GetChild(i).GetComponent<ShapeController>();
            ShapeData _shapeData = new ShapeData
            {
                shapeID = i,
                shapeName = _shape.shapeName,
                shapePoints = SerializableVector3.GetSerializableArray(
                    _shape.shapePoints.ConvertAll(point => point.position).ToArray()),
                shapePosition = new SerializableVector3(_shape.transform.position),
                polygonData = _shape.GetPolygonData(),
                renderData = _shape.GetRenderData()
            };
            _shapesData[i] = _shapeData;
        };
        return _shapesData;
    }
    #endregion

    #region --- Load Map Data ---
    // -------------------------------------------
    // ------------ Load Map Data ------------
    // -------------------------------------------
    public void LoadFloorData(MapData _mapData, int _floorID)
    {   // Load the map data to the map editor
        FloorData _floorData = _mapData.floors[_floorID];
        CreateNodes(_floorData.nodes);
        LoadWallsData(_floorData.walls);
        LoadNodesData(_floorData.nodes);
        LoadShapesData(_floorData.shapes);

        _roomsManager.LoadRoomsData(_floorData.rooms, _mapData.roomTypes);
        _qrCodesManager.LoadQRCodesData(_floorData.qrCodes);
    }

    public void CreateNodes(NodeData[] _nodesData)
    {   // Create the nodes from the nodes data
        for (int i = 0; i < _nodesData.Length; i++)
        {
            NodeData _nodeData = _nodesData[i];
            GameObject _node = Instantiate(_nodePrefab, _nodesParent);
            _node.transform.position = _nodeData.nodePosition.GetVector3 + new Vector3(0, 0, -0.5f);
            _node.name = "Node_" + _nodeData.nodeID;
        };
    }

    public void LoadNodesData(NodeData[] _nodesData)
    {   // Load the nodes data, because the nodes are created after the walls
        for (int i = 0; i < _nodesData.Length; i++)
        {
            NodeData _nodeData = _nodesData[i];
            WallNodeController _node = _nodesParent.GetChild(i).GetComponent<WallNodeController>();
            _node.neighborsNodes = _nodeData.neighborsNodes.ToList().ConvertAll(
                node => _nodesParent.GetChild(node).GetComponent<WallNodeController>());
            _node.walls = _nodeData.walls.ToList().ConvertAll(
                wall => _wallsParent.GetChild(wall).gameObject);

            foreach (GameObject _wall in _node.walls)
            {   // Add the line types to the node
                WallLineController _wallController = _wall.GetComponent<WallLineController>();
                if (_node == _wallController.startNode) _node.linesType.Add(0);
                else _node.linesType.Add(1);
            }
        };
    }

    public void LoadWallsData(WallData[] _wallsData)
    {   // Load the walls data to the map editor
        for (int i = 0; i < _wallsData.Length; i++)
        {
            WallData _wallData = _wallsData[i];
            GameObject _wall = Instantiate(_wallPrefab, _wallsParent);
            _wall.transform.position = _wallData.wallPosition.GetVector3;
            _wall.name = "Wall_" + _wallData.wallID;

            WallLineController _wallController = _wall.GetComponent<WallLineController>();
            _wallController.length = _wallData.wallLenght;
            _wallController.width = _wallData.wallWidth;
            _wallController.startNode = _nodesParent.GetChild(_wallData.startNode).GetComponent<WallNodeController>();
            _wallController.endNode = _nodesParent.GetChild(_wallData.endNode).GetComponent<WallNodeController>();
            _wallController.SetLineRenderer(_wallController.startNode.GetNodePosition(),
                _wallController.endNode.GetNodePosition(), _wallData.wallWidth);
            _wallController.SetLineCollider();

            LoadEntrancesData(_wallData.entrances, _wallController);
        };
    }

    public void LoadEntrancesData(EntranceData[] _entrancesData, WallLineController _wall)
    {   // Load the entrances data to the map editor
        for (int i = 0; i < _entrancesData.Length; i++)
        {
            EntranceData _entranceData = _entrancesData[i];
            GameObject _entrance = Instantiate(_entrancePrefab, _entrancesParent);
            _entrance.transform.position = _entranceData.entrancePosition.GetVector3;
            _entrance.name = "Entrance_" + _entranceData.entranceID;

            EntrancesController _entranceController = _entrance.GetComponent<EntrancesController>();
            _entranceController.lenght = _entranceData.entranceLenght;
            _entranceController.entranceWall = _wall;

            _entranceController.startDot.transform.position = _entranceData.startNodePosition.GetVector3;
            _entranceController.endDot.transform.position = _entranceData.endNodePosition.GetVector3;

            _entranceController.SetLineRenderer(
                _entranceController.startDot.transform.position + new Vector3(0, 0, 0.5f),
                _entranceController.endDot.transform.position + new Vector3(0, 0, 0.5f)
                );
            _entranceController.SetLineCollider();
            _entranceController.PlaySettedAnimation();
            _wall.entrances.Add(_entranceController);
        };
    }

    public void LoadShapesData(ShapeData[] _shapesData)
    {   // Load the shapes data to the map editor
        for (int i = 0; i < _shapesData.Length; i++)
        {
            ShapeData _shapeData = _shapesData[i];
            GameObject _shape = Instantiate(_shapePrefab, _shapesParent);
            _shape.transform.position = _shapeData.shapePosition.GetVector3;
            _shape.name = "Shape_" + _shapeData.shapeID;

            ShapeController _shapeController = _shape.GetComponent<ShapeController>();
            _shapeController.shapeName = _shapeData.shapeName;
            _shapeController.LoadShapeFromData(_shapeData);
        };
    }

    #endregion
}
