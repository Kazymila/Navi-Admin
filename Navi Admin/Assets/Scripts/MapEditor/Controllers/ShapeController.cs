using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MapDataModel;
using Habrador_Computational_Geometry;

public class ShapeController : MonoBehaviour
{
    #region --- Shape Variables ---
    [Header("Shape Settings")]
    public string shapeName;
    public float shapeHeight = 1.5f;
    [SerializeField] private Color _polygonColor = new Color(0.26f, 0, 0.68f, 0.5f);
    [SerializeField] private bool _changingColor = false;
    public List<Transform> shapePoints;

    [Header("Dots settings")]
    [SerializeField] private GameObject _shapeDotPrefab;
    public Vector3 shapeDotsOffset = new Vector3(0, 0, -0.75f);

    [Header("Shape Mesh")]
    [SerializeField] private GameObject _shapePolygonPrefab;
    [SerializeField] private GameObject _shapeRenderPrefab;
    [SerializeField] private Material _shapeRenderMaterial;
    private Transform _shapeRenderParent;
    private MeshFilter _shapePolygonMesh;
    private MeshFilter _shapeRenderMesh;
    #endregion
    private PolygonCollider2D _linePolygonCollider;
    private LineRenderer _lineRenderer;

    private void Awake()
    {
        _shapeRenderParent = GameObject.Find("MapRenderView").transform.GetChild(2);
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

    private void Update()
    {
        if (_changingColor)
        {   // Change the color of the shape polygon on editor
            _shapePolygonMesh.GetComponent<MeshRenderer>().material.SetColor("_Color1", _polygonColor);
        }
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
        _lineRenderer.SetPosition(shapePoints.Count - 1, point.position - shapeDotsOffset);
    }

    public void UpdateLastPoint(Vector3 _position)
    {   // Update the last point of the line
        if (shapePoints.Count > 0)
        {
            _lineRenderer.SetPosition(shapePoints.Count - 1, _position);
            shapePoints[shapePoints.Count - 1].position = _position + shapeDotsOffset;
        }
    }

    public void MoveShape(Vector3 _position)
    {   // Move the shape to a new position
        this.transform.position = _position + (this.transform.position - shapePoints[0].position);
        this.transform.position = new Vector3(this.transform.position.x, this.transform.position.y, 0);

        foreach (Transform point in shapePoints)
            _lineRenderer.SetPosition(shapePoints.IndexOf(point), point.position - shapeDotsOffset);
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

    public void CreateShapePolygon(bool _generateMesh = true)
    {   // Create the shape polygon object
        GameObject _shapeMesh = Instantiate(
            _shapePolygonPrefab,
            Vector3.zero + new Vector3(0, 0, -0.25f), // Offset to avoid z-fighting (same as shape dots offset
            Quaternion.identity,
            this.transform
            );
        _shapeMesh.transform.SetSiblingIndex(0);

        _shapeMesh.GetComponent<MeshRenderer>().material.SetColor("_Color1", _polygonColor);
        _shapeMesh.name = "ShapePolygon_" + _shapeMesh.transform.GetSiblingIndex();

        if (_generateMesh) // Generate the mesh if needed
            _shapeMesh.GetComponent<MeshFilter>().mesh = GeneratePolygonMesh();

        SetShapeCollider(_shapeMesh.GetComponent<PolygonCollider2D>());
        _shapePolygonMesh = _shapeMesh.GetComponent<MeshFilter>();
    }

    private Mesh GeneratePolygonMesh(bool _rotateTo2D = true)
    {   // Create a polygon mesh from connected points
        HashSet<MyVector2> points = shapePoints.Select(v => new MyVector2(v.position.x, v.position.y)).ToHashSet();

        // Hull points
        List<MyVector2> hullPoints_2d = _ConvexHull.JarvisMarch_2D(points);
        HashSet<List<MyVector2>> holePoints_2d = new HashSet<List<MyVector2>>();

        // Normalize to range 0-1
        Normalizer2 normalizer = new Normalizer2(hullPoints_2d);
        List<MyVector2> hullPoints_2d_normalized = normalizer.Normalize(hullPoints_2d);

        // Generate the triangulation
        HalfEdgeData2 triangleData_normalized = _Delaunay.ConstrainedBySloan(
            points, hullPoints_2d_normalized, holePoints_2d,
            shouldRemoveTriangles: true, new HalfEdgeData2());

        // UnNormalize and get the triangles
        HalfEdgeData2 triangleData = normalizer.UnNormalize(triangleData_normalized);
        HashSet<Triangle2> triangles_2d = _TransformBetweenDataStructures.HalfEdge2ToTriangle2(triangleData);

        // Make sure the triangles have the correct orientation
        triangles_2d = HelpMethods.OrientTrianglesClockwise(triangles_2d);

        // Create the mesh
        Mesh _mesh = _TransformBetweenDataStructures.Triangles2ToMesh(triangles_2d, true);

        if (_rotateTo2D)
        {   // Rotate the mesh points to 2D
            Vector3[] vertices = _mesh.vertices;
            for (int i = 0; i < vertices.Length; i++)
                vertices[i] = Quaternion.Euler(-90, 0, 0) * vertices[i];
            _mesh.vertices = vertices;
        }

        return _mesh;
    }
    #endregion

    #region --- Shape Mesh 3D Render ---
    public void GenerateShapeMesh()
    {   // Create a 3D mesh from shape points
        if (_shapeRenderMesh != null) Destroy(_shapeRenderMesh.gameObject);
        GameObject _shape3D = Instantiate(
            _shapeRenderPrefab,
            Vector3.zero,
            Quaternion.identity,
            _shapeRenderParent
            );
        _shape3D.name = "ShapeRender_" + this.transform.GetSiblingIndex();
        _shape3D.GetComponent<MeshRenderer>().material = _shapePolygonMesh.GetComponent<MeshRenderer>().material;

        // Generate the shape faces and combine them into one mesh
        Mesh _polygonMesh = GeneratePolygonMesh(false);
        Mesh[] _shapeFaces = new Mesh[] { _polygonMesh };
        _shapeFaces = _shapeFaces.Concat(GenerateShapeFaces()).ToArray();

        _shape3D.GetComponent<MeshFilter>().mesh = CombineFaces(_shapeFaces, _shape3D.transform);
        _shapeRenderMesh = _shape3D.GetComponent<MeshFilter>();

        // Move mesh to shape position
        _shape3D.transform.position = new Vector3(this.transform.position.x, 0, this.transform.position.y);
        Vector3[] _vertices = _shapeRenderMesh.mesh.vertices;

        for (int i = 0; i < _vertices.Length; i++)
            _vertices[i] = _vertices[i] - _shape3D.transform.position;

        _shapeRenderMesh.mesh.vertices = _vertices;
        _shapeRenderMesh.mesh.RecalculateBounds();
        _shapeRenderMesh.mesh.RecalculateNormals();

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

            _meshes[i] = GenerateFaceMesh(_facePoints);
        }
        return _meshes;
    }

    private Mesh GenerateFaceMesh(Vector3[] _points)
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

        CreateShapePolygon(false); // Create the shape polygon
        PolygonData _polygonData = _shapeData.polygonData;
        _polygonColor = _polygonData.materialColor.GetColor;
        _shapePolygonMesh.GetComponent<MeshRenderer>().material.SetColor("_Color1", _polygonColor);
        _shapePolygonMesh.mesh.vertices = SerializableVector3.GetVector3Array(_polygonData.vertices);
        _shapePolygonMesh.mesh.triangles = _polygonData.triangles;
        _shapePolygonMesh.transform.localPosition = new Vector3(0, 0, -0.25f);
        _shapePolygonMesh.mesh.RecalculateBounds();
        _shapePolygonMesh.mesh.RecalculateNormals();
    }
    #endregion
}
