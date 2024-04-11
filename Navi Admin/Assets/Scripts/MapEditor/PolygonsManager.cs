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

    public void Generate2DPolygons()
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

    #region --- Graph operations ---

    private void GetPolygonsFromCycles()
    {   // Get polygons from the cycles in the graph
        return;
    }

    private void MinimunCycleBasis(float[,] _graphMatrix)
    {   // Get the minimun cycles from the graph
        int _nodesCount = _graphMatrix.GetLength(0);
        float[,] _shortestPaths = AllPairsShortestPaths(_graphMatrix);
        List<int>[] _cyclesSets = new List<int>[_nodesCount];
        //TODO: Implement the minimun cycle basis algorithm
    }

    private float[,] AllPairsShortestPaths(float[,] _graphMatrix)
    {   // Get the all pairs shortest paths from the graph, using Floyd-Warshall algorithm
        int _nodesCount = _graphMatrix.GetLength(0);
        float[,] dist = new float[_nodesCount, _nodesCount];

        // Initialize the solution matrix same as input graph matrix
        // Or we can say the initial values of shortest distances
        // are based on shortest paths considering no intermediate vertex
        for (int i = 0; i < _nodesCount; i++)
        {
            for (int j = 0; j < _nodesCount; j++)
            {
                dist[i, j] = _graphMatrix[i, j];
            }
        }
        /* Add all vertices one by one to the set of intermediate vertices.
        ---> Before start of a iteration, we have shortest distances
             between all pairs of vertices such that the shortest distances
             consider only the vertices in set {0, 1, 2, .. k-1} as intermediate vertices.
        ---> After the end of a iteration, vertex no. k is added to the set of intermediate
             vertices and the set becomes {0, 1, 2, .. k} */

        for (int k = 0; k < _nodesCount; k++)
        {   // Pick all vertices as source (one by one)
            for (int i = 0; i < _nodesCount; i++)
            {   // Pick all vertices as destination (for the above picked source)
                for (int j = 0; j < _nodesCount; j++)
                {   // If vertex k is on the shortest path from i to j, 
                    // then update the value of dist[i][j]
                    if (dist[i, k] + dist[k, j] < dist[i, j])
                        dist[i, j] = dist[i, k] + dist[k, j];
                }
            }
        }
        return dist;
    }

    private float[,] GetWeightedAdjacentMatrix()
    {   // Get the weighted adjacent matrix from the graph (distance between nodes)
        float[,] _graphMatrix = new float[_adjacentGraph.Length, _adjacentGraph.Length];
        for (int i = 0; i < _adjacentGraph.Length; i++)
        {
            for (int j = 0; j < _adjacentGraph.Length; j++)
            {   // If the nodes are adjacent, get the distance between them (weight)
                if (_adjacentGraph[i].Contains(j))
                    _graphMatrix[i, j] = Vector3.Distance(_nodesParent.transform.GetChild(i).position, _nodesParent.transform.GetChild(j).position);
                else
                    _graphMatrix[i, j] = 99999;
            }
        }
        return _graphMatrix;
    }

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
            _dot.neighborsDots.ForEach(neighbor =>
            {   // Add the neighbors nodes to the adjacent list
                int _index = neighbor.transform.GetSiblingIndex();
                _adjacentGraph[i].Add(_index);
            });
        }
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
            if (i >= polygons.Count) break;
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
