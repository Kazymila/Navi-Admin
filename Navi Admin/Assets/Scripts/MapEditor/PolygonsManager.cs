using System;
using System.Collections;
using System.Collections.Generic;
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
    [SerializeField] private GameObject _nodesParent;

    [Header("UI Elements")]
    [SerializeField] private GameObject _textlabelPrefab;
    [SerializeField] private Transform _labelsParent;

    private List<int>[] _adjacentGraph; // Adjacent list of the graph
    private List<int>[] _cycles; // Cycles in the graph
    private int _cycleIndex = 0;
    private int _polygonsCount = 0;

    public void UpdatePolygons()
    {   // Update the polygons meshes and colliders
        polygons.ForEach(polygon => polygon.CreatePolygonMesh());
    }

    public void GeneratePolygons()
    {   // Create polygons from the graph cycles (closed areas)
        GetCyclesOnGraph();

        if (polygons.Count == 0)
        {   // If there are no polygons, create them
            foreach (List<int> _cycle in _cycles) CreatePolygon(_cycle);
        }
        else
        {   // Check if the polygons already exist in the graph
            for (int i = 0; i < _cycles.Length; i++)
            {
                List<WallDotController> _nodes = new List<WallDotController>();
                foreach (int _node in _cycles[i])
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
                if (!_polygonExists) CreatePolygon(_cycles[i]);
            }
        }
    }

    public void CreatePolygon(List<int> _cycleNodes)
    {   // Create a polygon from the given cycle nodes
        GameObject _polygon = Instantiate(_2DPolygonPrefab, Vector3.zero, Quaternion.identity, _2DPolygonsParent);
        PolygonController _polygonController = _polygon.GetComponent<PolygonController>();
        _polygonController.colorMaterial.SetColor("_Color1", new Color(0.5f, 0.5f, 0.5f, 0.5f));
        _cycleNodes.ForEach(nodeIndex =>
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

    #region --- Graph operations ---
    public void GetCyclesOnGraph()
    {   // Get the closed areas (cycles) from the graph
        _cycleIndex = 0;
        GetAdjacentList();
        int _nodesCount = _nodesParent.transform.childCount;

        _cycles = new List<int>[_nodesCount];
        int[] visited = new int[_nodesCount];
        int[] parents = new int[_nodesCount];

        GetCyclesByDFS(_adjacentGraph[0][0], 0, visited, parents);

        // Remove the empty cycles
        List<int>[] _cyclesOnGraph = new List<int>[_cycleIndex];
        for (int i = 0; i < _cycleIndex; i++)
            if (_cycles[i].Count > 0)
                _cyclesOnGraph[i] = _cycles[i];
        _cycles = _cyclesOnGraph;

        //print("Cycles count: " + _cycleIndex);
        //PrintGraphList("Node ", _adjacentGraph);
        //PrintGraphList("Cycle ", _cycles);
    }

    private void GetCyclesByDFS(int u, int p, int[] visited, int[] parents)
    {   // Get the cycles from the graph using DFS algorithm
        if (visited[u] == 2) return; // already (completely) visited node
        if (visited[u] == 1)
        {   // cycle detected
            List<int> inCycleNodes = new List<int>();
            int current = p;
            inCycleNodes.Add(current);
            while (current != u)
            {   // backtrack the node which are in the current cycle thats found
                current = parents[current];
                inCycleNodes.Add(current);
            }
            _cycles[_cycleIndex] = inCycleNodes;
            _cycleIndex++;
            return;
        }
        parents[u] = p;
        visited[u] = 1; // partially visited

        // Simple DFS on graph
        foreach (int v in _adjacentGraph[u])
        {   // if it has not been visited previously
            if (v == parents[u]) continue;
            GetCyclesByDFS(v, u, visited, parents);
        }
        visited[u] = 2; // completely visited
    }

    private void GetAdjacentList()
    {   // Get the adjacent list of the graph from wall dots
        _adjacentGraph = new List<int>[_nodesParent.transform.childCount];

        for (int i = 0; i < _adjacentGraph.Length; i++)
            _adjacentGraph[i] = new List<int>();

        for (int i = 0; i < _nodesParent.transform.childCount; i++)
        {
            WallDotController _dot = _nodesParent.transform.GetChild(i).GetComponent<WallDotController>();
            //List<WallDotController> _nodes = SortNodesByDistance(_dot, _dot.neighborsDots);
            _dot.neighborsDots.ForEach(neighbor =>
            {   // Add the neighbors nodes to the adjacent list
                int _index = neighbor.transform.GetSiblingIndex();
                _adjacentGraph[i].Add(_index);
            });
        }
    }

    private List<WallDotController> SortNodesByDistance(WallDotController _dot, List<WallDotController> _nodes)
    {   // Sort the nodes by distance to the given dot
        _nodes.Sort((a, b) => Vector3.Distance(a.transform.position, _dot.transform.position).CompareTo(Vector3.Distance(b.transform.position, _dot.transform.position)));
        return _nodes;
    }

    private void PrintGraphList(string _iteratorName, List<int>[] _list)
    {   // Print the info of a list (adjacent list or cycles in graph)
        for (int i = 0; i < _list.Length; i++)
        {
            string _innerList = "";
            _list[i].ForEach(node => _innerList += node + ", ");
            print(_iteratorName + i + ": " + _innerList);
        }
    }
    #endregion

    #region --- UI Elements ---
    public void ShowPolygonsLabels()
    {   // Show the labels of the polygons
        foreach (PolygonController _polygon in polygons)
        {
            GameObject _label = Instantiate(_textlabelPrefab, Vector3.zero, Quaternion.identity, _labelsParent);
            _label.transform.position = Camera.main.WorldToScreenPoint(_polygon.GetPolygonCenter());
            _label.GetComponent<TextMeshProUGUI>().text = _polygon.polygonLabel;
        }
    }
    public void UpdateLabelsPosition()
    {   // Update the position of the labels to follow the camera
        for (int i = 0; i < _labelsParent.childCount; i++)
        {
            Transform _label = _labelsParent.GetChild(i);
            _label.position = Camera.main.WorldToScreenPoint(polygons[i].GetPolygonCenter());
        }
    }
    public void RemovePolygonsLabels()
    {   // Remove the labels of the polygons
        foreach (Transform _label in _labelsParent)
            Destroy(_label.gameObject);
    }
    #endregion
}
