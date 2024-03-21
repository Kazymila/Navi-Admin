using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Haze;

public class PolygonsManager : MonoBehaviour
{
    [SerializeField] private GameObject _nodesParent;
    [SerializeField] private GameObject _polygonPrefab;

    private List<PolygonController> _polygons;
    private int polygonsCount = 0;

    private List<int>[] _adjacentGraph;
    private List<int>[] _cycles;
    private int cyclenumber = 0;

    #region --- Find Polygons (Cycles in Graph) ---

    public void GetClosedAreas()
    {   // Get the closed areas (cycles) from the graph
        GetAdjacentList();
        int _nodesCount = _nodesParent.transform.childCount;

        _cycles = new List<int>[_nodesCount];
        int[] markers = new int[_nodesCount];
        int[] parents = new int[_nodesCount];
        GetCyclesByDFS(1, 0, markers, parents);

        PrintListInfo("Node ", _adjacentGraph);
        PrintListInfo("Cycle ", _cycles);
    }
    private void PrintListInfo(string _iteratorName, List<int>[] _list)
    {   // Print the info of a list (adjacent list or cycles in graph)
        for (int i = 0; i < _list.Length; i++)
        {
            string _innerList = "";
            _list[i].ForEach(neighbor => _innerList += _innerList + ", ");
            print(_iteratorName + i + ": " + _innerList);
        }
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
            _cycles[cyclenumber] = inCycleNodes;
            cyclenumber++;
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

    private void GetPointsFromCycles()
    {   // Get positions of nodes in cycles to create polygons
        // TODO
    }
    #endregion


    public void CreatePolygonMesh(List<WallLineController> _walls)
    {   // Create a polygon mesh from connected points

        List<Vector3> _points = new List<Vector3>();
        _walls.ForEach(wall =>
        {
            if (!_points.Contains(wall.startDot.transform.position))
                _points.Add(wall.startDot.transform.position);
            if (!_points.Contains(wall.endDot.transform.position))
                _points.Add(wall.endDot.transform.position);
        });

        // Convert the 3D points to 2D points
        List<Vector2> _points2D = new List<Vector2>();
        _points.ForEach(point => _points2D.Add(new Vector2(point.x, point.y)));

        // Triangulate the 2D points
        List<Triangulator.Triangle> _triangles = Triangulator.Triangulate(_points2D);
        List<Vector3> _vertices = new List<Vector3>();
        List<int> _indices = new List<int>();

        Triangulator.AddTrianglesToMesh(ref _vertices, ref _indices, _triangles, 0.05f, true);
        polygonsCount++;

        // Create the mesh
        Mesh _mesh = new Mesh();
        _mesh.vertices = _vertices.ToArray();
        _mesh.triangles = _indices.ToArray();
        _mesh.RecalculateNormals();
        _mesh.RecalculateBounds();

        GameObject _polygon = Instantiate(_polygonPrefab, Vector3.zero, Quaternion.identity, this.transform);
        _polygon.GetComponent<MeshFilter>().mesh = _mesh;
        _polygon.name = "Polygon_" + polygonsCount;
        /*
        MeshRenderer _meshRenderer = _polygon.AddComponent<MeshRenderer>();
        _meshRenderer.material = new Material(Shader.Find("Standard"));
        _meshRenderer.material.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        */
    }
}
