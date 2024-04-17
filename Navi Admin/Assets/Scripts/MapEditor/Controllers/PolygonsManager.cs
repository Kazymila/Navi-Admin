using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using GraphPlanarityTesting.Graphs.DataStructures;
using GraphPlanarityTesting.PlanarityTesting.BoyerMyrvold;
using MapDataModel;
using UnityEngine;
using TMPro;

public class PolygonsManager : MonoBehaviour
{
    [Header("Polygons Elements")]
    public List<PolygonController> polygons = new List<PolygonController>();
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

    public void UpdatePolygons()
    {   // Update the polygons meshes and colliders
        polygons.ForEach(polygon => polygon.CreatePolygonMesh());
    }
    public PolygonData[] GetPolygonsData(bool _fromRender = false)
    {   // Get the polygons data to save it
        PolygonData[] _polygonsData = new PolygonData[polygons.Count];

        for (int i = 0; i < polygons.Count; i++)
        {   // Get the data of each polygon
            Vector3[] _vertices = new Vector3[polygons[i].nodes.Count];
            for (int j = 0; j < polygons[i].nodes.Count; j++)
            {   // Get the vertices of the polygon
                if (_fromRender) // If from render, rotate the points in 90 degrees
                    _vertices[j] = new Vector3(polygons[i].nodes[j].transform.position.x,
                                                0, polygons[i].nodes[j].transform.position.y);
                else _vertices[j] = polygons[i].nodes[j].transform.position;
            }
            PolygonData _polygonData = new PolygonData
            {
                polygonName = polygons[i].name,
                polygonColor = new SerializableColor(polygons[i].colorMaterial.GetColor("_Color1")),
                vertices = SerializableVector3.GetSerializableArray(_vertices)
            };
            _polygonsData[i] = _polygonData;
        }
        return _polygonsData;
    }

    #region --- Generate Polygons ---
    public void GeneratePolygons()
    {   // Generate the polygons from the graph
        List<List<int>> _graphFaces = GetGraphFaces();
        PrintGraphFaces(_graphFaces);

        if (polygons.Count == 0) // If there are no polygons, create them
            foreach (List<int> _face in _graphFaces) CreatePolygon(_face);
        else
        {   // Check if the polygons already exist in the graph
            for (int i = 0; i < _graphFaces.Count; i++)
            {
                List<WallDotController> _nodes = new List<WallDotController>();
                foreach (int _node in _graphFaces[i])
                    _nodes.Add(_nodesParent.transform.GetChild(_node).GetComponent<WallDotController>());

                // Check if the polygon already exists
                bool _polygonExists = false;
                foreach (PolygonController _polygon in polygons)
                {
                    if (new HashSet<WallDotController>(_nodes).SetEquals(_polygon.nodes))
                    {
                        _polygonExists = true;
                        break;
                    }
                }   // If the polygon does not exist, create it
                if (!_polygonExists) CreatePolygon(_graphFaces[i]);
            }
        }
    }

    public void Generate3DPolygons()
    {   // Generate the 3D polygons from the 2D polygons
        foreach (Transform _3Dpolygon in _3DPolygonsParent) Destroy(_3Dpolygon.gameObject);

        foreach (PolygonController _polygon in polygons)
        {
            GameObject _3Dpolygon = Instantiate(_3DPolygonPrefab, Vector3.zero, Quaternion.Euler(90, 0, 0), _3DPolygonsParent);
            _3Dpolygon.GetComponent<MeshCollider>().sharedMesh = _polygon.GetComponent<MeshFilter>().mesh;
            _3Dpolygon.GetComponent<MeshFilter>().mesh = _polygon.GetComponent<MeshFilter>().mesh;
            _3Dpolygon.GetComponent<MeshRenderer>().material = _polygon.colorMaterial;
            _3Dpolygon.name = _polygon.name;
        }
    }

    public void CreatePolygon(List<int> _faceNodes)
    {   // Create a polygon from the given cycle nodes
        GameObject _polygon = Instantiate(_2DPolygonPrefab, Vector3.zero, Quaternion.identity, _2DPolygonsParent);
        PolygonController _polygonController = _polygon.GetComponent<PolygonController>();

        _faceNodes.ForEach(nodeIndex =>
        {   // Add the nodes to the polygon and the polygon to the nodes
            WallDotController _dot = _nodesParent.transform.GetChild(nodeIndex).GetComponent<WallDotController>();
            _dot.polygons.Add(_polygonController);
            _polygonController.nodes.Add(_dot);
        });
        _polygon.name = "Polygon_" + _polygonsCount;
        _polygonController.polygonLabel = "Room_" + _polygonsCount;
        _polygonController.CreatePolygonMesh();
        polygons.Add(_polygonController);
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
            int source = _wall.startDot.transform.GetSiblingIndex();
            int target = _wall.endDot.transform.GetSiblingIndex();
            _graph.AddEdge(source, target);
        }
        return _graph;
    }
    #endregion

    #region --- UI Elements ---
    public void ShowPolygonsLabels()
    {   // Show the labels of the polygons
        foreach (PolygonController _polygon in polygons)
        {
            GameObject _label = Instantiate(_textlabelPrefab, Vector3.zero, Quaternion.identity, _labelsParent);
            _label.transform.position = Camera.main.WorldToScreenPoint(_polygon.GetPolygonCentroid());
            _label.GetComponent<TextMeshProUGUI>().text = _polygon.polygonLabel;
        }
    }
    public void UpdateLabelsPosition()
    {   // Update the position of the labels to follow the camera
        for (int i = 0; i < _labelsParent.childCount; i++)
        {
            if (i >= polygons.Count) break;
            Transform _label = _labelsParent.GetChild(i);
            _label.position = Camera.main.WorldToScreenPoint(polygons[i].GetPolygonCentroid());
        }
    }
    public void RemovePolygonsLabels()
    {   // Remove the labels of the polygons
        foreach (Transform _label in _labelsParent)
            Destroy(_label.gameObject);
    }
    #endregion
}
