using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PolygonsManager : MonoBehaviour
{
    public List<PolygonController> _polygons = new List<PolygonController>();
    [SerializeField] private GameObject _polygonPrefab;
    [SerializeField] private GameObject _nodesParent;

    private int _polygonsCount = 0;

    private List<int>[] _adjacentGraph; // Adjacent list of the graph
    private List<int>[] _cycles; // Cycles in the graph
    private int _cycleIndex = 0;

    public void GeneratePolygons()
    {   // Create polygons from the graph cycles (closed areas)
        GetCyclesOnGraph();

        for (int i = 0; i < _cycles.Length; i++)
        {   // Create a polygon for each cycle in the graph
            CreatePolygon(_cycles[i]);
        }
    }

    public void CreatePolygon(List<int> _cycleNodes)
    {   // Create a polygon from the given cycle nodes
        GameObject _polygon = Instantiate(_polygonPrefab, Vector3.zero, Quaternion.identity, this.transform);
        PolygonController _polygonController = _polygon.GetComponent<PolygonController>();
        _cycleNodes.ForEach(nodeIndex =>
        {   // Add the nodes to the polygon
            WallDotController _dot = _nodesParent.transform.GetChild(nodeIndex).GetComponent<WallDotController>();
            _polygonController.nodes.Add(_dot);
        });
        _polygon.name = "Polygon_" + _polygonsCount;
        _polygonController.polygonLabel = _polygon.name;
        _polygonController.CreatePolygonMesh();
        _polygons.Add(_polygonController);
        _polygonsCount++;
    }

    #region --- Graph operations ---
    public void GetCyclesOnGraph()
    {   // Get the closed areas (cycles) from the graph
        _cycleIndex = 0;
        GetAdjacentList();
        int _nodesCount = _nodesParent.transform.childCount;

        _cycles = new List<int>[_nodesCount];
        int[] markers = new int[_nodesCount];
        int[] parents = new int[_nodesCount];
        GetCyclesByDFS(1, 0, markers, parents);

        // Remove the empty cycles
        List<int>[] _cyclesOnGraph = new List<int>[_cycleIndex];
        for (int i = 0; i < _cycleIndex; i++)
            if (_cycles[i].Count > 0)
                _cyclesOnGraph[i] = _cycles[i];
        _cycles = _cyclesOnGraph;

        print("Cycles count: " + _cycleIndex);
        PrintGraphList("Node ", _adjacentGraph);
        PrintGraphList("Cycle ", _cycles);
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
            {
                int _index = neighbor.transform.GetSiblingIndex();
                _adjacentGraph[i].Add(_index);
            });
        }
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
}
