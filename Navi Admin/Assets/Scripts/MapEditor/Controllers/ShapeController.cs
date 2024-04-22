using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MapDataModel;
using Game.Utils.Math;
using Game.Utils.Triangulation;

public class ShapeController : MonoBehaviour
{
    #region --- Shape Variables ---
    [Header("Shape Settings")]
    public string shapeName;
    public float shapeHeight = 0.5f;
    [SerializeField] private Color _polygonColor = new Color(0.26f, 0, 0.68f, 0.5f);
    public List<Transform> shapePoints;

    [Header("Dots settings")]
    [SerializeField] private GameObject _shapeDotPrefab;
    public Vector3 shapeDotsOffset = new Vector3(0, 0, -0.5f);

    [Header("Shape Mesh")]
    [SerializeField] private GameObject _shapePolygonPrefab;
    [SerializeField] private Material _shapeRenderMaterial;
    private Transform _shapePolygonParent;
    private Transform _shapeRenderParent;
    private MeshFilter _shapePolygonMesh;
    private MeshFilter _shapeRenderMesh;
    #endregion
    private PolygonCollider2D _linePolygonCollider;
    private LineRenderer _lineRenderer;

    private void Awake()
    {
        _shapePolygonParent = GameObject.Find("ShapesPolygons").transform;
        _shapeRenderParent = GameObject.Find("ShapesRender").transform;
        _linePolygonCollider = GetComponent<PolygonCollider2D>();
        _lineRenderer = GetComponent<LineRenderer>();
        _lineRenderer.positionCount = 0;
        shapePoints = new List<Transform>();
    }

    private void OnDestroy()
    {   // Destroy all the points when shape is destroyed
        foreach (Transform point in shapePoints)
            Destroy(point.gameObject);
    }

    #region --- Shape Operations ---
    public int GetPointsCount() => _lineRenderer.positionCount;

    public void InstantiateDot(Vector3 _position)
    {   // Instantiate a new dot in the current shape
        GameObject _newDot = Instantiate(_shapeDotPrefab, _position + shapeDotsOffset,
                            Quaternion.Euler(-90, 0, 0), this.transform);
        _newDot.name = "ShapeDot_" + GetPointsCount();
        AddPoint(_newDot.transform);
    }

    public void AddPoint(Transform point)
    {   // Add a new point to the line
        shapePoints.Add(point);
        _lineRenderer.positionCount++;
        _lineRenderer.SetPosition(shapePoints.Count - 1, point.position);
    }

    public void UpdateLastPoint(Vector3 _position)
    {   // Update the last point of the line
        if (shapePoints.Count > 0)
        {
            _lineRenderer.SetPosition(shapePoints.Count - 1, _position);
            shapePoints[shapePoints.Count - 1].position = _position + shapeDotsOffset;
        }
    }

    public void EndShape()
    {   // Close the shape
        if (shapePoints.Count > 2)
            _lineRenderer.loop = true;
        SetLineCollider();
    }

    public void RemoveLastPoint()
    {   // Remove the last point of the line
        if (shapePoints.Count > 0)
        {
            Destroy(shapePoints[shapePoints.Count - 1].gameObject);
            shapePoints.RemoveAt(shapePoints.Count - 1);
            _lineRenderer.positionCount--;
        }
    }

    public void DestroyShape()
    {   // Destroy the shape
        if (_shapePolygonMesh != null) Destroy(_shapePolygonMesh.gameObject);
        if (_shapeRenderMesh != null) Destroy(_shapeRenderMesh.gameObject);
        Destroy(this.gameObject);
    }

    public float GetLastSegmentLength()
    {   // Get the length of the last segment of the line
        if (shapePoints.Count > 0)
        {
            Vector3 _lastPoint = shapePoints[shapePoints.Count - 1].position;
            Vector3 _previousPoint = shapePoints[shapePoints.Count - 2].position;
            return Vector3.Distance(_lastPoint, _previousPoint);
        }
        else return 0;
    }

    public Vector3 GetLastSegmentCenter()
    {   // Get the center of the last segment of the line
        if (shapePoints.Count > 0)
        {
            Vector3 _lastPoint = shapePoints[shapePoints.Count - 1].position;
            Vector3 _previousPoint = shapePoints[shapePoints.Count - 2].position;
            return (_lastPoint + _previousPoint) / 2;
        }
        else return Vector3.zero;
    }
    #endregion

    #region --- Line Collider ---
    public void SetLineCollider()
    {   // Set the collider points of the line
        Vector3[] _positions = shapePoints.ConvertAll(point => point.position).ToArray();
        _positions = _positions.Concat(new Vector3[] { _positions[0] }).ToArray();

        if (_positions.Count() >= 2)
        {
            int _linesCount = _positions.Count() - 1;
            _linePolygonCollider.pathCount = _linesCount;

            for (int i = 0; i < _linesCount; i++)
            {   // Set the collider points of each line segment of the shape
                List<Vector2> _colliderPoints = CalculateLineColliderPoints(new List<Vector2> { _positions[i], _positions[i + 1] });
                _linePolygonCollider.SetPath(i, _colliderPoints.ConvertAll(p => (Vector2)transform.InverseTransformPoint(p)).ToArray());
            }
        }
        else _linePolygonCollider.pathCount = 0;
    }

    public List<Vector2> CalculateLineColliderPoints(List<Vector2> _positions)
    {   // Calculate the points of the line collider
        float _width = _lineRenderer.startWidth;

        //Calculate the gradient (m) of the line
        float _m = (_positions[1].y - _positions[0].y) / (_positions[1].x - _positions[0].x);
        float _deltaX = _width / 2; // Offset when the line is parallel to the y-axis
        float _deltaY = 0;

        if (!float.IsInfinity(_m)) // If the line is not parallel to the y-axis
        {
            _deltaX = (_width / 2f) * (_m / Mathf.Pow(_m * _m + 1, 0.5f));
            _deltaY = (_width / 2f) * (1 / Mathf.Pow(_m * _m + 1, 0.5f));
        }

        // Calculate offset from each point to mesh
        Vector2[] _offsets = new Vector2[2];
        _offsets[0] = new Vector2(-_deltaX, _deltaY);
        _offsets[1] = new Vector2(_deltaX, -_deltaY);

        // Generate mesh points
        List<Vector2> _colliderPoints = new List<Vector2>{
            _positions[0] + _offsets[0],
            _positions[1] + _offsets[0],
            _positions[1] + _offsets[1],
            _positions[0] + _offsets[1],
        };

        return _colliderPoints;
    }
    #endregion

    #region --- Shape Mesh Polygon ---
    public void SetShapeCollider(PolygonCollider2D _collider)
    {   // Set the collider points of shape polygon
        _collider.points = shapePoints.ConvertAll(point => new Vector2(point.position.x, point.position.y)).ToArray();
    }

    public void GenerateShapePolygon()
    {   // Create a mesh from shape points
        List<Vector2> _points2D = shapePoints.ConvertAll(
            point => new Vector2(point.position.x, point.position.y));
        List<Triangle2D> _outputTriangles = new List<Triangle2D>();
        List<List<Vector2>> _constrainedPoints = new List<List<Vector2>> { _points2D };

        DelaunayTriangulation _triangulation = new DelaunayTriangulation();
        _triangulation.Triangulate(_points2D, 0.0f, _constrainedPoints);
        _triangulation.GetTrianglesDiscardingHoles(_outputTriangles);

        CreateShapePolygon();
        _shapePolygonMesh.mesh = CreateMeshFromTriangles(_outputTriangles);
    }

    private void CreateShapePolygon()
    {   // Create the shape polygon object
        GameObject _shapeMesh = Instantiate(_shapePolygonPrefab, Vector3.zero
            + new Vector3(0, 0, -0.1f), Quaternion.identity, _shapePolygonParent);

        _shapeMesh.GetComponent<MeshRenderer>().material.SetColor("_Color1", _polygonColor);
        _shapeMesh.name = "ShapePolygon_" + _shapeMesh.transform.GetSiblingIndex();

        SetShapeCollider(_shapeMesh.GetComponent<PolygonCollider2D>());
        _shapePolygonMesh = _shapeMesh.GetComponent<MeshFilter>();
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
    #endregion

    #region --- Shape Mesh 3D Render ---
    public void GenerateShapeMesh()
    {   // Create a 3D mesh from shape points
        GameObject _shape3D = Instantiate(_shapePolygonMesh.gameObject,
            Vector3.zero, Quaternion.identity, _shapeRenderParent);

        _shape3D.name = _shape3D.name.Replace("(Clone)", "").Replace("Polygon", "Render");
        _shape3D.GetComponent<MeshRenderer>().material = _shapeRenderMaterial;
        _shape3D.layer = LayerMask.NameToLayer("Obstacle");
        Destroy(_shape3D.GetComponent<PolygonCollider2D>());

        // Generate the shape faces and combine them into one mesh
        Mesh _polygonMesh = _shape3D.GetComponent<MeshFilter>().mesh;
        _polygonMesh.vertices = _polygonMesh.vertices.Select(v => new Vector3(v.x, shapeHeight, v.y)).ToArray();

        Mesh[] _shapeFaces = new Mesh[] { _polygonMesh };
        _shapeFaces = _shapeFaces.Concat(GenerateShapeFaces()).ToArray();

        _shape3D.GetComponent<MeshFilter>().mesh = CombineFaces(_shapeFaces, _shape3D.transform);
        _shapeRenderMesh = _shape3D.GetComponent<MeshFilter>();
    }

    private Mesh CombineFaces(Mesh[] _meshes, Transform shapeTransform)
    {   // Combine the faces into one mesh
        CombineInstance[] _combine = new CombineInstance[_meshes.Length];
        for (int i = 0; i < _meshes.Length; i++)
        {
            _combine[i].mesh = _meshes[i];
            _combine[i].transform = shapeTransform.localToWorldMatrix;
        }
        Mesh _combinedMesh = new Mesh();
        _combinedMesh.CombineMeshes(_combine);
        return _combinedMesh;
    }

    private Mesh[] GenerateShapeFaces()
    {   // Rotate points to 3D and generate the faces
        List<Vector3> _polygonPoints = shapePoints.ConvertAll(point => point.position);
        _polygonPoints = _polygonPoints.ConvertAll(point => new Vector3(point.x, 0, point.y));

        Mesh[] _meshes = new Mesh[_polygonPoints.Count];

        for (int i = 0; i < _polygonPoints.Count; i++)
        {
            Vector3[] _facePoints = new Vector3[4];
            _facePoints[0] = _polygonPoints[i] + new Vector3(0, shapeHeight, 0);
            _facePoints[1] = _polygonPoints[(i + 1) % _polygonPoints.Count] + new Vector3(0, shapeHeight, 0);
            _facePoints[2] = _polygonPoints[(i + 1) % _polygonPoints.Count];
            _facePoints[3] = _polygonPoints[i];

            _meshes[i] = GenerateMesh(_facePoints);
        }
        return _meshes;
    }

    private Mesh GenerateMesh(Vector3[] _points)
    {   // Generate a mesh from a list of points
        Mesh _mesh = new Mesh();
        _mesh.vertices = _points.Concat(_points).ToArray();
        _mesh.triangles = new int[] {
            0, 1, 2, 0, 2, 3, // Face 1
            1, 0, 2, 2, 0, 3  // Face 2
        }; // Two sided mesh to avoid culling
        _mesh.RecalculateNormals();
        _mesh.RecalculateBounds();
        return _mesh;
    }
    #endregion

    #region --- Data Management ---
    public PolygonData GetPolygonData()
    {   // Get the polygon data for save
        PolygonData _polygonData = new PolygonData();
        _polygonData.materialColor = new SerializableColor(_polygonColor);
        _polygonData.vertices = SerializableVector3.GetSerializableArray(
            _shapePolygonMesh.mesh.vertices);
        _polygonData.triangles = _shapePolygonMesh.mesh.triangles;
        return _polygonData;
    }

    public MeshData GetRenderData()
    {   // Get the mesh data for save
        GenerateShapeMesh();
        MeshData _shapeData = new MeshData();
        _shapeData.vertices = SerializableVector3.GetSerializableArray(_shapeRenderMesh.mesh.vertices);
        _shapeData.triangles = _shapeRenderMesh.mesh.triangles;
        return _shapeData;
    }

    public void LoadShapeFromData(ShapeData _shapeData)
    {   // Set the shape data from load data
        Vector3[] _points = SerializableVector3.GetVector3Array(_shapeData.shapePoints);
        foreach (Vector3 _point in _points) InstantiateDot(_point - shapeDotsOffset); // Instantiate the points
        EndShape(); // Close the shape

        CreateShapePolygon();
        PolygonData _polygonData = _shapeData.polygonData;
        _shapePolygonMesh.GetComponent<MeshRenderer>().material.SetColor("_Color1", _polygonData.materialColor.GetColor);
        _shapePolygonMesh.mesh.vertices = SerializableVector3.GetVector3Array(_polygonData.vertices);
        _shapePolygonMesh.mesh.triangles = _polygonData.triangles;
    }
    #endregion
}
